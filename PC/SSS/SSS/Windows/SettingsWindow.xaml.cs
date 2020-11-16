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
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Windows;

    /// <summary>
    /// TODO : refactor this code with WPF style :)
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbRunAtWindowsStartup.IsChecked = this.GetRegisterInStartup();
            // Load values
            this.comboBox1.Items.Add(Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm32kHz16bitMono));
            this.comboBox1.Items.Add(Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm32kHz16bitStereo));
            this.comboBox1.Items.Add(Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm44kHz16bitMono));
            this.comboBox1.Items.Add(Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm44kHz16bitStereo));
            this.comboBox1.Items.Add(Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm48kHz16bitMono));
            this.comboBox1.Items.Add(Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm48kHz16bitStereo));
            foreach (var bitrate in Audio.AudioFormats.Mp3BitRates)
            {
                this.comboBox2.Items.Add(bitrate);
            }
            // Select setting values
            this.comboBox1.SelectedItem = Audio.AudioFormats.AsString(Audio.AudioSettings.GetAudioFormat());
            this.comboBox2.SelectedItem = Audio.AudioSettings.GetMP3Bitrate();
            this.radioButton1.IsChecked = (Audio.AudioSettings.GetStreamFormat() == Audio.AudioFormats.Format.Mp3);
            this.radioButton2.IsChecked = !this.radioButton1.IsChecked;
            this.cbDebug.IsChecked = SSS.Properties.Settings.Default.Debug;
            this.cbUseSpecificPort.IsChecked = SSS.Properties.Settings.Default.HTTPPort > 0;
            this.textBox1.Text = SSS.Properties.Settings.Default.HTTPPort.ToString();
        }

        private void btValid_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbUseSpecificPort.IsChecked.HasValue && this.cbUseSpecificPort.IsChecked.Value)
            {
                int port = Int32.Parse(this.textBox1.Text);
                if (port > 65534 || port < 80)
                {
                    MessageBox.Show("Invalid port value !", "Smart Streaming System", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            this.RegisterInStartup(this.cbRunAtWindowsStartup.IsChecked.Value);
            if (this.comboBox1.SelectedItem.ToString() == Audio.AudioFormats.AsString(Audio.AudioSettings.GetAudioFormat()) &&
                this.comboBox2.SelectedItem.ToString() == Audio.AudioSettings.GetMP3Bitrate().ToString() &&
                ((this.radioButton1.IsChecked.Value && Audio.AudioSettings.GetStreamFormat() == Audio.AudioFormats.Format.Mp3) ||
                 (this.radioButton2.IsChecked.Value && Audio.AudioSettings.GetStreamFormat() == Audio.AudioFormats.Format.Pcm)) &&
                this.cbDebug.IsChecked == SSS.Properties.Settings.Default.Debug &&
                this.cbUseSpecificPort.IsChecked == SSS.Properties.Settings.Default.HTTPPort > 0 && (this.cbUseSpecificPort.IsChecked.Value ? SSS.Properties.Settings.Default.HTTPPort.ToString() == this.textBox1.Text : true))
            {
                // Just close !
                this.Close();
            }
            else
            {
                var msg = MessageBox.Show("You need to restart Smart Streaming System to save new settings.\nDo you want to continue ?", "Smart Streaming System", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (msg == MessageBoxResult.Yes)
                {
                    if (this.comboBox1.SelectedItem.ToString() == Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm32kHz16bitMono))
                    {
                        Audio.AudioSettings.SetAudioFormat(Audio.AudioFormats.Pcm32kHz16bitMono);
                    }
                    else if (this.comboBox1.SelectedItem.ToString() == Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm32kHz16bitStereo))
                    {
                        Audio.AudioSettings.SetAudioFormat(Audio.AudioFormats.Pcm32kHz16bitStereo);
                    }
                    else if (this.comboBox1.SelectedItem.ToString() == Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm44kHz16bitMono))
                    {
                        Audio.AudioSettings.SetAudioFormat(Audio.AudioFormats.Pcm44kHz16bitMono);
                    }
                    else if (this.comboBox1.SelectedItem.ToString() == Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm44kHz16bitStereo))
                    {
                        Audio.AudioSettings.SetAudioFormat(Audio.AudioFormats.Pcm44kHz16bitStereo);
                    }
                    else if (this.comboBox1.SelectedItem.ToString() == Audio.AudioFormats.AsString(Audio.AudioFormats.Pcm48kHz16bitMono))
                    {
                        Audio.AudioSettings.SetAudioFormat(Audio.AudioFormats.Pcm48kHz16bitMono);
                    }
                    else
                    {
                        Audio.AudioSettings.SetAudioFormat(Audio.AudioFormats.Pcm48kHz16bitStereo);
                    }
                    Audio.AudioSettings.SetMP3Bitrate(int.Parse(this.comboBox2.SelectedItem.ToString()));
                    Audio.AudioSettings.SetStreamFormat(this.radioButton1.IsChecked.Value ? Audio.AudioFormats.Format.Mp3 : Audio.AudioFormats.Format.Pcm);
                    SSS.Properties.Settings.Default.HTTPPort = (this.cbUseSpecificPort.IsChecked.HasValue && this.cbUseSpecificPort.IsChecked.Value) ? Int32.Parse(this.textBox1.Text) : 0;
                    SSS.Properties.Settings.Default.Debug = this.cbDebug.IsChecked.HasValue && this.cbDebug.IsChecked.Value;
                    // Save !
                    SSS.Properties.Settings.Default.Save();
                    // Restart !
                    this.Close();
                    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location, Constants.RESTART_ARGUMENT_NAME);
                    Application.Current.Shutdown();
                }
                else if (msg == MessageBoxResult.No)
                {
                    // Just close !
                    this.Close();
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.Close();
            }
        }

        private void textBox1_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
            if (!e.Handled)
            {
                int result = 0;
                Int32.TryParse(textBox1.Text + e.Text, out result);
                e.Handled = !(result > 0 && result < 65534);
            }
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(text);
        }

        private void cbUseSpecificPort_Checked(object sender, RoutedEventArgs e)
        {
            if (cbUseSpecificPort.IsChecked.Value && (this.textBox1.Text == "0" || string.IsNullOrEmpty(this.textBox1.Text)))
            {
                this.textBox1.Text = Constants.DEFAULT_HTTP_PORT.ToString();
            }
        }

        private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(Constants.DOWNLOAD_SSS_URL);
            this.Close();
        }

        private void RegisterInStartup(bool isChecked)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(Constants.REGISTRY_START_SUBKEY, true);
                if (isChecked)
                {
                    registryKey.SetValue(Constants.REGISTRY_SSS_KEY, System.Windows.Forms.Application.ExecutablePath);
                }
                else if (!isChecked && GetRegisterInStartup())
                {
                    registryKey.DeleteValue(Constants.REGISTRY_SSS_KEY);
                }
            }
            catch { }
        }

        private bool GetRegisterInStartup()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(Constants.REGISTRY_START_SUBKEY, true);
                return registryKey.GetValue(Constants.REGISTRY_SSS_KEY, null) != null;
            }
            catch { }
            return false;
        }
    }
}
