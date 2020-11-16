namespace SSS.Audio
{
    using NAudio.Lame;
    using NAudio.Wave;
    using System;
    using System.Threading;

    internal class WasapiProvider
    {
        private byte[] buffer = new byte[1024];

        private bool isRunning = true;
        private bool hasMP3Clients = false;
        private bool hasPCMClients = false;

        private WasapiLoopbackCapture loopbackWaveIn = null;
        private PipeStream recordingStream = null;
        private WaveStream rawConvertedStream = null;
        private WaveStream pcmStream = null;
        private LameMP3FileWriter mp3Writer = null;

        public PipeStream LoopbackMp3Stream { get; private set; }
        public PipeStream LoopbackL16Stream { get; private set; }

        public WasapiProvider()
        {
            // Init Pipes
            this.recordingStream = new PipeStream();
            this.LoopbackMp3Stream = new PipeStream();
            this.LoopbackL16Stream = new PipeStream();

            // Init Wave Processor thread
            Thread waveProcessorThread = new Thread(new ThreadStart(this.waveProcessor)) { Priority = ThreadPriority.Highest };

            // Init Wasapi Capture
            this.loopbackWaveIn = new WasapiLoopbackCapture();
            this.loopbackWaveIn.DataAvailable += new EventHandler<WaveInEventArgs>(this.loopbackWaveIn_DataAvailable);

            // Init Raw Wav (16bit)
            WaveStream rawWave16b = new Wave32To16Stream(new RawSourceWaveStream(this.recordingStream, NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(this.loopbackWaveIn.WaveFormat.SampleRate, this.loopbackWaveIn.WaveFormat.Channels)));

            // Convert Raw Wav to PCM with audio format in settings
            var audioFormat = AudioSettings.GetAudioFormat();
            if (rawWave16b.WaveFormat.SampleRate == audioFormat.SampleRate
                && rawWave16b.WaveFormat.BitsPerSample == audioFormat.BitsPerSample
                && rawWave16b.WaveFormat.Channels == audioFormat.Channels)
            {
                // No conversion !
                this.rawConvertedStream = null;
                this.pcmStream = WaveFormatConversionStream.CreatePcmStream(rawWave16b);
            }
            else
            {
                // Resampler
                this.rawConvertedStream = new WaveProviderToWaveStream(new MediaFoundationResampler(rawWave16b, audioFormat));
                this.pcmStream = WaveFormatConversionStream.CreatePcmStream(rawConvertedStream);
            }

            // Init MP3 Encoder
            this.mp3Writer = new LameMP3FileWriter(this.LoopbackMp3Stream, pcmStream.WaveFormat, AudioSettings.GetMP3Bitrate());

            // Start Recording
            this.loopbackWaveIn.StartRecording();

            // Start Wave Processor thread
            waveProcessorThread.Start();
        }


        public void Dispose()
        {
            this.isRunning = false;
            Thread.Sleep(200);
            this.loopbackWaveIn.StopRecording();
            this.recordingStream.Flush();
            this.recordingStream.Dispose();
            if (this.rawConvertedStream != null)
            {
                this.rawConvertedStream.Flush();
                this.rawConvertedStream.Dispose();
            }
            this.pcmStream.Flush();
            this.pcmStream.Dispose();
            this.LoopbackMp3Stream.Flush();
            this.LoopbackMp3Stream.Dispose();
            this.LoopbackL16Stream.Flush();
            this.LoopbackL16Stream.Dispose();
        }

        internal void UpdateClientsList()
        {
            this.hasMP3Clients = App.CurrentInstance.sssDevice.sessionMp3Streams.Count > 0;
            this.hasPCMClients = App.CurrentInstance.sssDevice.sessionPcmStreams.Count > 0;
        }

        private void waveProcessor()
        {
            while (this.isRunning)
            {
                if (this.hasMP3Clients || this.hasPCMClients)
                {
                    try
                    {
                        int readBytes = this.pcmStream.Read(buffer, 0, buffer.Length);
                        if (readBytes > 0)
                        {
                            if (this.hasMP3Clients)
                            {
                                // MP3 stream
                                this.mp3Writer.Write(this.buffer, 0, readBytes);
                            }
                            if (this.hasPCMClients)
                            {
                                // L16 stream
                                if (BitConverter.IsLittleEndian)
                                {
                                    for (int i = 0; i < readBytes; i += 2)
                                    {
                                        Array.Reverse(buffer, i, 2);
                                    }
                                }
                                this.LoopbackL16Stream.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }
                    catch
                    {
                        if (this.isRunning)
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void loopbackWaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (this.hasMP3Clients || this.hasPCMClients)
            {
                this.recordingStream.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }
    }
}
