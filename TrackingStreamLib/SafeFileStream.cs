namespace TrackingStreamLib
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class SafeFileStream : Stream
    {
        private readonly FileInfo m_file;

        private Stream m_dataStream;

        private readonly FileSystemWatcher m_watcher;

        public SafeFileStream(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            m_file = new FileInfo(filePath);
            var directoryName = m_file.DirectoryName;
            var fileName = m_file.Name;
            m_watcher = new FileSystemWatcher(directoryName, fileName);
            m_watcher.Created += delegate { CloseExistingStream(); };
            m_watcher.Deleted += delegate { CloseExistingStream(); };
            m_watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_watcher.EnableRaisingEvents = false;
                m_watcher.Dispose();
            }
            base.Dispose(disposing);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CloseExistingStream()
        {
            if (m_dataStream == null)
            {
                return;
            }
            m_dataStream.Close();
            m_dataStream.Dispose();
            m_dataStream = null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool TryToReinitializeStream()
        {
            return TryToPerformOperation(
                () =>
                {
                    m_dataStream = new FileStream(
                        m_file.FullName,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite | FileShare.Delete | FileShare.Inheritable);
                });
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static bool TryToPerformOperation(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException || ex is UnauthorizedAccessException)
                {
                    return false;
                }
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool TryToPerformStreamOperation(Action action)
        {
            if (m_dataStream == null && !TryToReinitializeStream())
            {
                return false;
            }
            return TryToPerformOperation(action);
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
            // nop
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter. </param><param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long result = 0;
            if (!TryToPerformStreamOperation(() => result = m_dataStream.Seek(offset, origin)))
            {
                result = 0;
            }
            return result;
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes. </param><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.
        /// </returns>
        /// <param name="array">When this method returns, contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1<paramref name=")"/> replaced by the bytes read from the current source. </param><param name="offset">The byte offset in <paramref name="array"/> at which the read bytes will be placed. </param><param name="count">The maximum number of bytes to read. </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception><exception cref="T:System.NotSupportedException">The stream does not support reading. </exception><exception cref="T:System.IO.IOException">An I/O error occurred. </exception><exception cref="T:System.ArgumentException"><paramref name="offset"/> and <paramref name="count"/> describe an invalid range in <paramref name="array"/>. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] array, int offset, int count)
        {
            var bytesRead = 0;
            if (!TryToPerformStreamOperation(() => bytesRead = m_dataStream.Read(array, offset, count)) || bytesRead == 0)
            {
                CloseExistingStream();
            }

            return bytesRead;
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. </param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. </param><param name="count">The number of bytes to be written to the current stream. </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        public override bool CanRead => true;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        public override bool CanSeek => true;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        public override bool CanWrite => false;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length
        {
            get
            {
                long result = 0;
                TryToPerformStreamOperation(() => result = m_dataStream.Length);
                return result;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get
            {
                long result = 0;
                TryToPerformStreamOperation(() => result = m_dataStream.Position);
                return result;
            }
            set
            {
                TryToPerformStreamOperation(() => m_dataStream.Position = value);
            }
        }

        /// <summary>
        ///  Полный путь к файлу
        /// </summary>
        public string FilePath => m_file.FullName;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"[SFS] {m_file.Name}";
        }
    }
}