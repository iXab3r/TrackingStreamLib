namespace TrackingStreamLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Reader, that allows to enumerate through file strings
    /// </summary>
    public class StreamLinesReader : BaseLinesReader
    {
        /// <summary>
        ///     Initializes string reader using default encoding
        /// </summary>
        /// <param name="baseStream"></param>
        public StreamLinesReader(Stream baseStream)
            : base(baseStream)
        {
        }

        /// <summary>
        ///     Initializes string reader using specified encoding
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="encoding">Strings encoding</param>
        /// <param name="bufferSize">Block size</param>
        public StreamLinesReader(Stream baseStream, Encoding encoding, int bufferSize)
            : base(baseStream, encoding, bufferSize)
        {
        }

        /// <summary>
        ///     Enumerates through stream lines
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReadLines(int maxLineLength)
        {
            foreach (var line in ReadLinesInternal())
            {
                var lineIndex = 0;
                var lineLength = line.Length;
                while (lineLength - lineIndex > maxLineLength)
                {
                    var sub = line.Substring(lineIndex, maxLineLength);
                    lineIndex += maxLineLength;
                    yield return sub;
                }
                yield return line.Substring(lineIndex);
            }
        }

        /// <summary>
        ///     Enumerates through stream lines
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> ReadLines()
        {
            return ReadLines(int.MaxValue);
        }

        /// <summary>
        ///     Enumerates through stream lines
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReadLinesInternal()
        {
            var buffer = new byte[BufferSize];

            m_positon = baseStream.Position;

            var remainingBufferPart = new byte[0];
            while (baseStream.Position < baseStream.Length)
            {
                var bytesLeft = baseStream.Length - baseStream.Position;
                var bytesToRead = (int) Math.Min(bytesLeft, buffer.Length);
                var bytesRead = baseStream.Read(buffer, 0, bytesToRead);

                if (bytesRead < buffer.Length)
                {
                    Array.Resize(ref buffer, bytesRead);
                }

                var bytesToProcess = remainingBufferPart.Concat(buffer).ToArray();
                var bytesProcessed = 0;
                int nextLineEnd;
                while (ArrayUtils.TryToFindFirstSequence(bytesToProcess, newLineBytes, bytesProcessed, out nextLineEnd))
                {
                    var lineLength = nextLineEnd + newLineBytes.Length - bytesProcessed;
                    var line = ByteBufferToString(bytesToProcess, bytesProcessed, lineLength);
                    yield return line;

                    m_positon += lineLength;
                    bytesProcessed += lineLength;
                }
                remainingBufferPart = bytesToProcess.Skip(bytesProcessed).ToArray();
            }
            if (remainingBufferPart.Length > 0)
            {
                var lastLine = ByteBufferToString(remainingBufferPart, 0, remainingBufferPart.Length);
                yield return lastLine;
            }
        }
    }
}