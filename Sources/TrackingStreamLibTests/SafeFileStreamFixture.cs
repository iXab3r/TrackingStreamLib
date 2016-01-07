namespace TrackingStreamLib.Tests
{
    using System;
    using System.IO;

    using NUnit.Framework;

    using Moq;

    using Shouldly;

    [TestFixture()]
    public class SafeFileStreamFixture
    {
        private DirectoryInfo m_tempDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp"));

        [TestFixtureSetUp]
        public void Initialization()
        {
            if (m_tempDir.Exists)
            {
                m_tempDir.Delete(true);
            }
            m_tempDir.Create();
        }

        [Test()]
        public void SimpleReadWorks()
        {
            var tempFile = new FileInfo(Path.Combine(m_tempDir.FullName, "SimpleReadWorks"));
            using (var writer = File.Open(tempFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            using (var reader = new SafeFileStream(tempFile.FullName))
            {
                var sampleData = new byte[] { 0, 1, 2, 3, 4, 5 };
                writer.Write(sampleData, 0, sampleData.Length);
                writer.Flush();
                var result = new byte[sampleData.Length];

                var bytesRead = reader.Read(result, 0, result.Length);

                Assert.AreEqual(sampleData.Length, bytesRead);
                Assert.AreEqual(result, sampleData);
            }
        }

        [Test()]
        public void NonexistentFileReadWorks()
        {
            var tempFile = new FileInfo(Path.Combine(m_tempDir.FullName, "NonexistentFileReadWorks"));
            using (var reader = new SafeFileStream(tempFile.FullName))
            {
                var result = new byte[10];
                var bytesRead = reader.Read(result, 0, result.Length);

                Assert.AreEqual(0, bytesRead);
            }
        }

        [Test()]
        public void PositionEqualsToZeroAfterFileRemoval()
        {
            var tempFile = new FileInfo(Path.Combine(m_tempDir.FullName, "PositionEqualsToZeroAfterFileRemoval"));
            var sampleData = new byte[] { 0, 1, 2, 3, 4, 5 };
            using (var writer = File.Open(tempFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                writer.Write(sampleData, 0, sampleData.Length);
            }
            File.Delete(tempFile.FullName);

            using (var reader = new SafeFileStream(tempFile.FullName))
            {
                var position = reader.Position;
                Assert.AreEqual(0, position);
            }
        }

        [Test()]
        public void LengthEqualsToZeroAfterFileRemoval()
        {
            var tempFile = new FileInfo(Path.Combine(m_tempDir.FullName, "LengthEqualsToZeroAfterFileRemoval"));
            var sampleData = new byte[] { 0, 1, 2, 3, 4, 5 };
            using (var writer = File.OpenWrite(tempFile.FullName))
            {
                writer.Write(sampleData, 0, sampleData.Length);
            }

            File.Delete(tempFile.FullName);

            using (var reader = new SafeFileStream(tempFile.FullName))
            {
                var length = reader.Length;
                Assert.AreEqual(0, length);
            }
        }

        [Test()]
        public void ReadAfterRealtimeDeleteWorks()
        {
            var tempFile = new FileInfo(Path.Combine(m_tempDir.FullName, "ReadAfterRealtimeDeleteWorks"));
            var sampleData = new byte[] { 0, 1, 2, 3, 4, 5 };
            using (var writer = File.Open(tempFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                writer.Write(sampleData, 0, sampleData.Length);
            }

            using (var reader = new SafeFileStream(tempFile.FullName))
            {
                var result = new byte[sampleData.Length];

                // 1st step
                var bytesRead = reader.Read(result, 0, result.Length);
                Assert.AreEqual(sampleData.Length, bytesRead);
                Assert.AreEqual(result, sampleData);

                File.Delete(tempFile.FullName);

                //2nd step
                bytesRead = reader.Read(result, 0, result.Length);
                Assert.AreEqual(0, bytesRead);

                using (var writer = File.Open(tempFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
                {
                    writer.Write(sampleData, 0, sampleData.Length);
                }

                // 3rd step
                bytesRead = reader.Read(result, 0, result.Length);
                Assert.AreEqual(sampleData.Length, bytesRead);
                Assert.AreEqual(result, sampleData);
            }
        }
    }
}