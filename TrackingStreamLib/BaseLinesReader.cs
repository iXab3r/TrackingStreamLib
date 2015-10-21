namespace TrackingStreamLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    ///     Abstract class that allows to enumerate through file strings
    /// </summary>
    public abstract class BaseLinesReader : IDisposable
    {
        /// <summary>
        ///     Default buffer size - size of blocks, that will be read from stream
        /// </summary>
        public const int DefaultBufferSize = ushort.MaxValue;

        protected readonly Stream baseStream;

        private readonly char m_bom = '\uFEFF';

        private readonly Encoding encoding;

        protected readonly byte[] newLineBytes;

        protected long m_positon;

        /// <summary>
        ///     Initializes string reader using default encoding
        /// </summary>
        /// <param name="baseStream"></param>
        protected BaseLinesReader(Stream baseStream)
            : this(baseStream, Encoding.Default)
        {
        }

        /// <summary>
        ///     Initializes string reader using specified encoding
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="encoding">Strings encoding</param>
        /// <param name="bufferSize">Block size</param>
        protected BaseLinesReader(Stream baseStream, Encoding encoding, int bufferSize = DefaultBufferSize)
        {
            if (baseStream == null || !baseStream.CanRead)
            {
                throw new ArgumentException("stream");
            }

            this.baseStream = baseStream;
            this.encoding = encoding;
            BufferSize = bufferSize;

            newLineBytes = this.encoding.GetBytes(new[] {'\n'});
        }


        /// <summary>
        ///     Current reader offset
        /// </summary>
        public long Positon => m_positon;

        /// <summary>
        ///     Data stream length
        /// </summary>
        public long Length => baseStream.Length;

        /// <summary>
        ///     Underlaying data stream
        /// </summary>
        public Stream BaseStream => baseStream;

        /// <summary>
        ///     Internal buffer size
        /// </summary>
        public int BufferSize { get; }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        ///     Enumerates through stream lines
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> ReadLines();

        /// <summary>
        ///     Converts byte buffer to string
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected string ByteBufferToString(byte[] buffer, int startIndex, int length)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            var result = encoding.GetString(buffer, startIndex, length);
            result = result.TrimStart(m_bom).Trim('\r', '\n');
            return result;
        }
    }
}