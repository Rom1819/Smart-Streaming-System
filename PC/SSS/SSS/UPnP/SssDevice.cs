namespace SSS.UPnP
{
    using OpenSource.UPnP;
    using SSS.Audio;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represent the SSS UPnP device.
    /// </summary>
    internal class SssDevice
    {
        private bool isRunning = true;
        private byte[] bufferMp3 = new byte[2048];
        private byte[] bufferPcm = new byte[2048];

        internal ConcurrentDictionary<int, PipeStream> sessionMp3Streams = new ConcurrentDictionary<int, PipeStream>();
        internal ConcurrentDictionary<int, PipeStream> sessionPcmStreams = new ConcurrentDictionary<int, PipeStream>();

        public UPnPDevice Device { get; private set; }
        public DvContentDirectory ContentDirectory { get; private set; }
        public DvConnectionManager ConnectionManager { get; private set; }

        public SssDevice()
        {
            this.Device = UPnPDevice.CreateRootDevice(1800, 1.0, "\\");

            this.Device.FriendlyName = "Smart Streaming System (" + Environment.MachineName + "):"; //Environment.MachineName + ": Smart Streaming System";
            this.Device.Manufacturer = "Ronch IT";
            this.Device.ManufacturerURL = "";
            this.Device.ModelURL = new Uri("");
            this.Device.ModelName = "Windows Media Connect SSS"; //"Smart Streaming System";
            this.Device.ModelDescription = "Smart Streaming System (SSS) is a Windows application to stream the sound from your PC to an UPnP / DLNA device";
            this.Device.ModelNumber = "1.4";
            this.Device.SerialNumber = "SSS_UPNP_14";
            this.Device.HasPresentation = true;
            this.Device.PresentationURL = "about/sss.html";
            this.Device.DeviceURN = "urn:schemas-upnp-org:device:MediaServer:1";
            this.Device.Icon = Properties.Resources.sss128.ToBitmap();
            this.Device.Icon2 = Properties.Resources.sss48.ToBitmap();
            this.Device.AddCustomFieldInDescription("dlna:X_DLNADOC", "DMS-1.50", "urn:schemas-dlna-org:device-1-0");

            DvX_MS_MediaReceiverRegistrar X_MS_MediaReceiverRegistrar = new DvX_MS_MediaReceiverRegistrar();
            X_MS_MediaReceiverRegistrar.External_IsAuthorized = new DvX_MS_MediaReceiverRegistrar.Delegate_IsAuthorized(X_MS_MediaReceiverRegistrar.IsAuthorized);
            X_MS_MediaReceiverRegistrar.External_IsValidated = new DvX_MS_MediaReceiverRegistrar.Delegate_IsValidated(X_MS_MediaReceiverRegistrar.IsValidated);
            X_MS_MediaReceiverRegistrar.External_RegisterDevice = new DvX_MS_MediaReceiverRegistrar.Delegate_RegisterDevice(X_MS_MediaReceiverRegistrar.RegisterDevice);
            this.Device.AddService(X_MS_MediaReceiverRegistrar);

            ConnectionManager = new DvConnectionManager();
            ConnectionManager.External_GetCurrentConnectionIDs = new DvConnectionManager.Delegate_GetCurrentConnectionIDs(ConnectionManager.GetCurrentConnectionIDs);
            ConnectionManager.External_GetCurrentConnectionInfo = new DvConnectionManager.Delegate_GetCurrentConnectionInfo(ConnectionManager.GetCurrentConnectionInfo);
            ConnectionManager.External_GetProtocolInfo = new DvConnectionManager.Delegate_GetProtocolInfo(ConnectionManager.GetProtocolInfo);
            this.Device.AddService(ConnectionManager);

            ContentDirectory = new DvContentDirectory();
            ContentDirectory.External_Browse = new DvContentDirectory.Delegate_Browse(ContentDirectory.Browse);
            ContentDirectory.External_GetSearchCapabilities = new DvContentDirectory.Delegate_GetSearchCapabilities(ContentDirectory.GetSearchCapabilities);
            ContentDirectory.External_GetSortCapabilities = new DvContentDirectory.Delegate_GetSortCapabilities(ContentDirectory.GetSortCapabilities);
            ContentDirectory.External_GetSystemUpdateID = new DvContentDirectory.Delegate_GetSystemUpdateID(ContentDirectory.GetSystemUpdateID);
            ContentDirectory.External_X_GetFeatureList = new DvContentDirectory.Delegate_X_GetFeatureList(ContentDirectory.X_GetFeatureList);
            ContentDirectory.External_Search = new DvContentDirectory.Delegate_Search(ContentDirectory.Search);
            this.Device.AddService(ContentDirectory);

            // Setting the initial value of evented variables
            X_MS_MediaReceiverRegistrar.Evented_AuthorizationDeniedUpdateID = 0;
            X_MS_MediaReceiverRegistrar.Evented_ValidationSucceededUpdateID = 0;
            X_MS_MediaReceiverRegistrar.Evented_ValidationRevokedUpdateID = 0;
            X_MS_MediaReceiverRegistrar.Evented_AuthorizationGrantedUpdateID = 0;
            ConnectionManager.Evented_SourceProtocolInfo = "Sample String";
            ConnectionManager.Evented_SinkProtocolInfo = "Sample String";
            ConnectionManager.Evented_CurrentConnectionIDs = "Sample String";
            ContentDirectory.Evented_ContainerUpdateIDs = "Sample String";
            ContentDirectory.Evented_SystemUpdateID = 0;

            // Add Virtual Directory
            this.Device.AddVirtualDirectory("about", HeaderHandler, PageHandler);
            this.Device.AddVirtualDirectory("stream", HeaderHandler, PageHandler);

            // Duplicate audio stream
            Thread duplicateMp3AudioThread = new Thread(new ThreadStart(duplicateMp3AudioStream));
            Thread duplicatePcmAudioThread = new Thread(new ThreadStart(duplicatePcmAudioStream));
            duplicateMp3AudioThread.Start();
            duplicatePcmAudioThread.Start();
        }

        public void Start()
        {
            if (SSS.Properties.Settings.Default.HTTPPort > 0)
            {
                this.Device.StartDevice(SSS.Properties.Settings.Default.HTTPPort);
            }
            else
            {
                this.Device.StartDevice();
            }
        }

        public void Stop()
        {
            this.isRunning = false;
            this.Device.StopDevice();
        }

        private void duplicateMp3AudioStream()
        {
            while (this.isRunning)
            {
                int bytesRead = App.CurrentInstance.wasapiProvider.LoopbackMp3Stream.Read(bufferMp3, 0, bufferMp3.Length);
                if (bytesRead > 0)
                {
                    Parallel.ForEach<PipeStream>(sessionMp3Streams.Values, (p, s, l) =>
                    {
                        p.Write(bufferMp3, 0, bytesRead);
                    });
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void duplicatePcmAudioStream()
        {
            while (this.isRunning)
            {
                int bytesRead = App.CurrentInstance.wasapiProvider.LoopbackL16Stream.Read(bufferPcm, 0, bufferPcm.Length);
                if (bytesRead > 0)
                {
                    Parallel.ForEach<PipeStream>(sessionPcmStreams.Values, (p, s, l) =>
                    {
                        p.Write(bufferPcm, 0, bytesRead);
                    });
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void HeaderHandler(OpenSource.UPnP.UPnPDevice sender, OpenSource.UPnP.HTTPMessage msg, OpenSource.UPnP.HTTPSession WebSession, string VirtualDir)
        {
            msg.AddTag("transferMode.dlna.org", "Streaming");
            msg.AddTag("contentFeatures.dlna.org", "DLNA.ORG_OP=01;DLNA.ORG_CI=0;DLNA.ORG_FLAGS=01700000000000000000000000000000");
        }

        private void PageHandler(OpenSource.UPnP.UPnPDevice sender, OpenSource.UPnP.HTTPMessage msg, OpenSource.UPnP.HTTPSession WebSession, string VirtualDir)
        {
            if (VirtualDir.Equals("/stream", StringComparison.InvariantCultureIgnoreCase) && msg.DirectiveObj.Equals("/sss.mp3", StringComparison.InvariantCultureIgnoreCase))
            {
                WebSession.OnStreamDone += (s, e) =>
                {
                    if (sessionMp3Streams.ContainsKey(s.SessionID))
                    {
                        PipeStream value;
                        sessionMp3Streams.TryRemove(s.SessionID, out value);
                        App.CurrentInstance.wasapiProvider.UpdateClientsList();
                    }
                };
                PipeStream stream = sessionMp3Streams.GetOrAdd(WebSession.SessionID, new PipeStream());
                App.CurrentInstance.wasapiProvider.UpdateClientsList();
                WebSession.SendStreamObject(stream, "audio/mpeg");
            }
            else if (VirtualDir.Equals("/stream", StringComparison.InvariantCultureIgnoreCase) && msg.DirectiveObj.Equals("/sss.wav", StringComparison.InvariantCultureIgnoreCase))
            {
                WebSession.OnStreamDone += (s, e) =>
                {
                    if (sessionPcmStreams.ContainsKey(s.SessionID))
                    {
                        PipeStream value;
                        sessionPcmStreams.TryRemove(s.SessionID, out value);
                        App.CurrentInstance.wasapiProvider.UpdateClientsList();
                    }
                };
                PipeStream stream = sessionPcmStreams.GetOrAdd(WebSession.SessionID, new PipeStream());
                App.CurrentInstance.wasapiProvider.UpdateClientsList();
                var audioFormat = AudioSettings.GetAudioFormat();
                WebSession.SendStreamObject(stream, "audio/L16;rate=" + audioFormat.SampleRate + ";channels=" + audioFormat.Channels);
            }
            else if (VirtualDir.Equals("/about", StringComparison.InvariantCultureIgnoreCase))
            {
                OpenSource.UPnP.HTTPMessage response = new OpenSource.UPnP.HTTPMessage();
                response.StatusCode = 200;
                response.StatusData = "OK";
                response.AddTag("Content-Type", "text/html");
                response.BodyBuffer = System.Text.Encoding.UTF8.GetBytes(Properties.Resources.About);
                WebSession.Send(response);
            }
            else
            {
                OpenSource.UPnP.HTTPMessage response = new OpenSource.UPnP.HTTPMessage();
                response.StatusCode = 404;
                response.StatusData = "Not Found";
                response.AddTag("Content-Type", "text/html");
                response.BodyBuffer = System.Text.Encoding.UTF8.GetBytes(Properties.Resources.Error404);
                WebSession.Send(response);
            }
        }
    }
}

