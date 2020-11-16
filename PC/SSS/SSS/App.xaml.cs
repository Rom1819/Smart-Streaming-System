/*
 *	 Smart Streaming System
 *	 Assembly: SSS
 *	 File: AboutWindow.xaml.cs
 *	 Copyright (C) 2012-2015 - Ronch IT.	   	
 *
 *   This file is part of Smart Streaming System.
 */

namespace SSS
{
    using OpenSource.UPnP.AV.RENDERER.CP;
    using SSS.Audio;
    using SSS.UPnP;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App CurrentInstance { get { return Application.Current as App; } }
        public static bool NeedUpdate { get; private set; }

        private string[] autoStreamTo = new string[0];

        private AVRendererDiscovery rendererDiscovery = null;
        internal SssDevice sssDevice = null;
        internal WasapiProvider wasapiProvider = null;

        private SettingsWindow settingsWindow = null;
        private AboutWindow aboutWindow = null;
        private frm_Server server = null;
        private Help help = null;
        private RecordWindow recordWindow = null;
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        private System.Windows.Forms.ToolStripMenuItem streamToMenu = null;
        private System.Windows.Forms.ToolStripMenuItem searchingItem = null;
        private bool directClose = false;   //Skip the statements in Application_Exit function.

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var startTest = 0;
        start:
            var existingProcess = Process.GetProcessesByName(Constants.SSS_PROCESS_NAME);
            if (existingProcess != null && existingProcess.Length > 1)
            {
                if (e.Args != null && e.Args.Contains(Constants.RESTART_ARGUMENT_NAME) && (startTest++) < Constants.NUMBER_OF_RESTART_TEST)
                {
                    Thread.Sleep(100);
                    goto start;
                }
                MessageBox.Show("Smart Streaming System is already running !", "Smart Streaming System", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                directClose = true;   //Skip the statements in Application_Exit function, otherwise it will casue null object exception
                this.Shutdown();
            }
            else
            {
                if (SSS.Properties.Settings.Default.Debug)
                {
                    AppDomain.CurrentDomain.UnhandledException += (ss, ee) =>
                    {
                        var ex = (Exception)ee.ExceptionObject;
                        StringBuilder error = new StringBuilder();
                        error.AppendLine("Date: " + DateTime.Now.ToString());
                        error.AppendLine("Message: " + ex.Message);
                        error.AppendLine("Detail: " + ex.ToString());
                        error.AppendLine("------------------------------");
                        File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Constants.SSS_CRASHLOG_FILENAME), error.ToString());
                        //MessageBox.Show("An unhandled error has occured ! See the '" + Constants.SSS_CRASHLOG_FILENAME + "' on your desktop for more information", "Smart Streaming System", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    };
                }
                if (new NAudio.CoreAudioApi.MMDeviceEnumerator().EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.DeviceState.Active).Count == 0)    //Check the available interface.
                {
                    System.Windows.Forms.MessageBox.Show("Unable to find your sound interface, please check your sound interface in control panel.", "Cannot find the sound interface");
                    directClose = true;    //Skip the statements in Application_Exit function, otherwise it will casue null object exception
                    this.Shutdown();
                }
                else
                {
                    this.CheckAutomaticDeviceStreamed(e);
                    this.CheckNewVersion();
                    this.InitializeUI();
                    this.rendererDiscovery = new AVRendererDiscovery((new AVRendererDiscovery.DiscoveryHandler(RendererAddedSink)));
                    this.rendererDiscovery.OnRendererRemoved += new AVRendererDiscovery.DiscoveryHandler(new AVRendererDiscovery.DiscoveryHandler(RendererRemovedSink));
                    this.wasapiProvider = new WasapiProvider();
                    this.sssDevice = new SssDevice();
                    this.sssDevice.Start();
                    notifyIcon.ShowBalloonTip(2000, "Smart Streaming System is running", "Right-click on this icon to show the menu !", System.Windows.Forms.ToolTipIcon.Info);
                }
            }
        }

        private void InitializeUI()
        {
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            this.settingsWindow = new SettingsWindow();
            this.aboutWindow = new AboutWindow();
            this.server = new frm_Server();
            this.recordWindow = new RecordWindow();
            this.help = new Help();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                Icon = SSS.Properties.Resources.sss32,
                Text = string.Format("Smart Streaming System {0}.{1}{2}", fileVersion.ProductMajorPart, fileVersion.ProductMinorPart, (fileVersion.ProductPrivatePart % 2) == 0 ? "" : " (BETA)"),
                Visible = true
            };
            this.notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.notifyIcon.ContextMenuStrip.Items.Add("Host Examination", null, (s, e2) => this.server.Show());
            this.notifyIcon.ContextMenuStrip.Items.Add("Record", null, (s, e2) => this.recordWindow.Show());
            this.notifyIcon.ContextMenuStrip.Items.Add("Settings", null, (s, e2) => this.settingsWindow.Show());
            this.notifyIcon.ContextMenuStrip.Items.Add("Help", null, (s, e2) => this.help.Show());
            this.notifyIcon.ContextMenuStrip.Items.Add("Developers", null, (s, e2) => this.aboutWindow.Show());
            this.notifyIcon.ContextMenuStrip.Items.Add("-");
            this.notifyIcon.ContextMenuStrip.Items.Add("Exit", null, (s, e2) => this.Shutdown());
        }  

        private void CheckAutomaticDeviceStreamed(StartupEventArgs startupEvent)
        {
            if (startupEvent.Args.Any(a => a.StartsWith(Constants.STREAM_TO_ARGUMENT_NAME, StringComparison.InvariantCultureIgnoreCase)))
            {
                autoStreamTo = startupEvent.Args.First(a => a.StartsWith(Constants.STREAM_TO_ARGUMENT_NAME, StringComparison.InvariantCultureIgnoreCase)).Substring(Constants.STREAM_TO_ARGUMENT_NAME.Length).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        private void CheckNewVersion()
        {
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Task.Factory.StartNew(() =>
            {
                // Check update !
                WebClient wc = new WebClient();
                try
                {
                    string lastVersionStr = wc.DownloadString(Constants.UPDATE_VERSION_URL);
                    if (!string.IsNullOrEmpty(lastVersionStr))
                    {
                        Version lastVersion = new Version(lastVersionStr);
                        Version currentVersion = new Version(fileVersion.FileVersion);
                        App.NeedUpdate = (lastVersion > currentVersion);
                        if (App.NeedUpdate)
                        {
                            var response = MessageBox.Show("A new version of Smart Streaming System is available !\n\nDo you want to download it now?", "Smart Streaming System ", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if (response == MessageBoxResult.Yes)
                            {
                                Process.Start(Constants.DOWNLOAD_SSS_URL);
                            }
                        }
                    }
                }
                catch { }
            });
        }

        private void CloseStreamingConnections()
        {
            foreach (System.Windows.Forms.ToolStripItem tsi in this.streamToMenu.DropDownItems)
            {
                var item = tsi as System.Windows.Forms.ToolStripMenuItem;
                if (item != null && item.Checked)
                {
                    item.Checked = false;
                    var renderer = item.Tag as AVRenderer;
                    if (renderer != null && renderer.Connections.Count > 0)
                    {
                        var connectionAV = renderer.Connections[0] as AVConnection;
                        connectionAV.Stop();
                    }
                }
            }
        }

        private void RendererAddedSink(AVRendererDiscovery sender, AVRenderer renderer)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!this.streamToMenu.DropDownItems.ContainsKey(renderer.UniqueDeviceName))
                {
                    var menuItem = new System.Windows.Forms.ToolStripMenuItem
                        (renderer.FriendlyName, (renderer.device.favicon != null) ? renderer.device.favicon.ToBitmap() : null, streamMenu_RendererSelected)
                    {
                        Name = renderer.UniqueDeviceName,
                        Tag = renderer
                    };
                    this.streamToMenu.DropDownItems.Add(menuItem);
                    if (renderer != null && renderer.Connections.Count > 0)
                    {
                        var connectionAV = renderer.Connections[0] as AVConnection;
                        connectionAV.OnPlayStateChanged += new AVConnection.PlayStateChangedHandler(connectionAV_OnPlayStateChanged);
                        if (autoStreamTo.Any(r => r.Equals(renderer.FriendlyName, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            streamMenu_RendererSelected(menuItem, EventArgs.Empty);
                        }
                    }
                }
                if (this.streamToMenu.DropDownItems.ContainsKey(this.searchingItem.Name) && this.streamToMenu.DropDownItems.Count >= 2)
                {
                    this.streamToMenu.DropDownItems.RemoveByKey(this.searchingItem.Name);
                }
            }));
        }

        private void RendererRemovedSink(AVRendererDiscovery sender, AVRenderer renderer)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.streamToMenu.DropDownItems.ContainsKey(renderer.UniqueDeviceName))
                {
                    this.streamToMenu.DropDownItems.RemoveByKey(renderer.UniqueDeviceName);
                    if (renderer != null && renderer.Connections.Count > 0)
                    {
                        var connectionAV = renderer.Connections[0] as AVConnection;
                        connectionAV.OnPlayStateChanged -= new AVConnection.PlayStateChangedHandler(connectionAV_OnPlayStateChanged);
                    }
                }
                if (!this.streamToMenu.DropDownItems.ContainsKey(this.searchingItem.Name) && this.streamToMenu.DropDownItems.Count == 2)
                {
                    this.streamToMenu.DropDownItems.Add(searchingItem);
                }
            }));
        }

        private void streamMenu_RendererSelected(object sender, EventArgs e)
        {
            var rendererItem = (System.Windows.Forms.ToolStripMenuItem)sender;
            var renderer = rendererItem.Tag as AVRenderer;
            if (renderer != null && renderer.Connections.Count > 0)
            {
                var connectionAV = renderer.Connections[0] as AVConnection;
                if (rendererItem.Checked)
                {
                    rendererItem.Checked = false;
                    connectionAV.Stop();
                }
                else
                {
                    rendererItem.Checked = true;
                    var media = this.sssDevice.ContentDirectory.GetWasapiMediaItem();
                    if (media != null)
                    {
                        connectionAV.Stop();
                        renderer.CreateConnection(media, DateTime.Now.Ticks);
                        connectionAV.Play();
                    }
                }
            }
        }

        private void connectionAV_OnPlayStateChanged(AVConnection sender, AVConnection.PlayState NewState)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.streamToMenu.DropDownItems.ContainsKey(sender.Parent.UniqueDeviceName) &&
                    sender.MediaResource != null && this.sssDevice.ContentDirectory.GetWasapiUris(AudioSettings.GetStreamFormat()).Contains(sender.MediaResource.ContentUri))
                {
                    var item = this.streamToMenu.DropDownItems[sender.Parent.UniqueDeviceName] as System.Windows.Forms.ToolStripMenuItem;
                    {
                        if (NewState == AVConnection.PlayState.STOPPED)
                        {
                            item.Checked = false;
                        }
                        else if (NewState == AVConnection.PlayState.PLAYING)
                        {
                            item.Checked = true;
                        }
                    }
                }
            }));
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (!directClose)
            {
                this.CloseStreamingConnections();
                this.sssDevice.Stop();
                this.wasapiProvider.Dispose();
                if (this.notifyIcon != null)
                {
                    this.notifyIcon.Dispose();
                }
            }
        }
    }
}
