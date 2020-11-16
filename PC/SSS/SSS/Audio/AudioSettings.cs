namespace SSS.Audio
{
    using SSS.Properties;
    using System;

    /// <summary>
    /// Represent the SSS audio settings
    /// </summary>
    internal static class AudioSettings
    {
        /// <summary>
        /// Gets the MP3 bitrate.
        /// </summary>
        /// <returns></returns>
        public static int GetMP3Bitrate()
        {
            return Settings.Default.Mp3Bitrate;
        }

        /// <summary>
        /// Sets the MP3 bitrate.
        /// </summary>
        /// <param name="bitrate">The bitrate.</param>
        public static void SetMP3Bitrate(int bitrate)
        {
            Settings.Default.Mp3Bitrate = bitrate;
        }

        /// <summary>
        /// Gets the audio format for the capture.
        /// </summary>
        /// <returns></returns>
        public static NAudio.Wave.WaveFormat GetAudioFormat()
        {
            switch (Settings.Default.CaptureFormat)
            {
                case "Pcm32kHz16bitMono":
                    return AudioFormats.Pcm32kHz16bitMono;
                case "Pcm32kHz16bitStereo":
                    return AudioFormats.Pcm32kHz16bitStereo;
                case "Pcm44kHz16bitMono":
                    return AudioFormats.Pcm44kHz16bitMono;
                case "Pcm44kHz16bitStereo":
                    return AudioFormats.Pcm44kHz16bitStereo;
                case "Pcm48kHz16bitMono":
                    return AudioFormats.Pcm48kHz16bitMono;
                case "Pcm48kHz16bitStereo":
                default:
                    return AudioFormats.Pcm48kHz16bitStereo;
            }
        }

        /// <summary>
        /// Sets the audio format for capture.
        /// </summary>
        /// <param name="format">The format.</param>
        public static void SetAudioFormat(NAudio.Wave.WaveFormat format)
        {
            if (format == AudioFormats.Pcm32kHz16bitMono)
            {
                Settings.Default.CaptureFormat = "Pcm32kHz16bitMono";
            }
            else if (format == AudioFormats.Pcm32kHz16bitStereo)
            {
                Settings.Default.CaptureFormat = "Pcm32kHz16bitStereo";
            }
            else if (format == AudioFormats.Pcm44kHz16bitMono)
            {
                Settings.Default.CaptureFormat = "Pcm44kHz16bitMono";
            }
            else if (format == AudioFormats.Pcm44kHz16bitStereo)
            {
                Settings.Default.CaptureFormat = "Pcm44kHz16bitStereo";
            }
            else if (format == AudioFormats.Pcm48kHz16bitMono)
            {
                Settings.Default.CaptureFormat = "Pcm48kHz16bitMono";
            }
            else
            {
                Settings.Default.CaptureFormat = "Pcm48kHz16bitStereo";
            }
        }

        /// <summary>
        /// Gets the default stream format.
        /// </summary>
        /// <returns></returns>
        public static AudioFormats.Format GetStreamFormat()
        {
            AudioFormats.Format format = AudioFormats.Format.Mp3;
            Enum.TryParse<AudioFormats.Format>(Settings.Default.StreamPreferenceFormat, out format);
            return format;
        }

        /// <summary>
        /// Sets the default stream format.
        /// </summary>
        /// <param name="format">The format.</param>
        public static void SetStreamFormat(AudioFormats.Format format)
        {
            Settings.Default.StreamPreferenceFormat = format.ToString();
        }
    }

    /// <summary>
    /// Represent SSS audio formats
    /// </summary>
    internal static class AudioFormats
    {
        public enum Format { Pcm, Mp3 };

        public static int[] Mp3BitRates = new int[] { 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320 };

        public static readonly NAudio.Wave.WaveFormat Pcm32kHz16bitMono = new NAudio.Wave.WaveFormat(32000, 16, 1);
        public static readonly NAudio.Wave.WaveFormat Pcm32kHz16bitStereo = new NAudio.Wave.WaveFormat(32000, 16, 2);
        public static readonly NAudio.Wave.WaveFormat Pcm44kHz16bitMono = new NAudio.Wave.WaveFormat(44100, 16, 1);
        public static readonly NAudio.Wave.WaveFormat Pcm44kHz16bitStereo = new NAudio.Wave.WaveFormat(44100, 16, 2);
        public static readonly NAudio.Wave.WaveFormat Pcm48kHz16bitMono = new NAudio.Wave.WaveFormat(48000, 16, 1);
        public static readonly NAudio.Wave.WaveFormat Pcm48kHz16bitStereo = new NAudio.Wave.WaveFormat(48000, 16, 2);

        public static string AsString(NAudio.Wave.WaveFormat format)
        {
            string channelsText;
            switch (format.Channels)
            {
                case 1:
                    channelsText = "Mono";
                    break;
                case 2:
                    channelsText = "Stereo";
                    break;
                default:
                    channelsText = format.Channels + " channels";
                    break;
            };
            return string.Format("{0} Hz, {1} bit, {2}", format.SampleRate, format.BitsPerSample, channelsText);
        }
    }
}
