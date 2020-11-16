namespace SSS.Audio
{
    using NAudio.Wave;
    using System;

    /// <summary>
    /// Convert IWaveProvider into a WaveStream
    /// </summary>
    internal class WaveProviderToWaveStream : WaveStream
    {
        private readonly IWaveProvider source;
        private long position;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveProviderToWaveStream"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public WaveProviderToWaveStream(IWaveProvider source)
        {
            this.source = source;
        }

        /// <summary>
        /// Retrieves the WaveFormat for this stream
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get { return source.WaveFormat; }
        }

        /// <summary>
        /// Don't know the real length of the source, just return a big number
        /// </summary>
        public override long Length
        {
            get { return Int32.MaxValue; }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public override long Position
        {
            get
            {
                // we'll just return the number of bytes read so far
                return position;
            }
            set
            {
                // can't set position on the source
                // n.b. could alternatively ignore this
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = source.Read(buffer, offset, count);
            position += bytesRead;
            return bytesRead;
        }
    }
}