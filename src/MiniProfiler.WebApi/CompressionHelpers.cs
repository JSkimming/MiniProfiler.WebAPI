namespace MiniProfiler.WebApi
{
    using System;
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Helper class to compress and decompress serialized data.
    /// </summary>
    internal static class CompressionHelpers
    {
        /// <summary>
        /// Compresses the supplied <paramref name="uncompressedBuffer"/>.
        /// </summary>
        /// <param name="uncompressedBuffer">The uncompressed buffer.</param>
        /// <returns>The compressed buffer of the supplied <paramref name="uncompressedBuffer"/>.</returns>
        internal static byte[] Compress(this byte[] uncompressedBuffer)
        {
            if (uncompressedBuffer == null) throw new ArgumentNullException("uncompressedBuffer");

            using (var uncompressedStream = new MemoryStream(uncompressedBuffer))
            using (var compressedStream = new MemoryStream())
            using (var gZipStream = new GZipStream(compressedStream, CompressionMode.Compress, true))
            {
                // Need to move to the beginning of the uncompressed stream.
                uncompressedStream.Seek(0, SeekOrigin.Begin);

                // Copy the uncompressed stream to the GZip stream. The GZip
                // stream must then be closed to ensure everything is written to
                // the underlying compressed stream.
                uncompressedStream.CopyTo(gZipStream);
                gZipStream.Close();

                return compressedStream.ToArray();
            }
        }

        /// <summary>
        /// Decompresses the supplied <paramref name="compressedBuffer"/>.
        /// </summary>
        /// <param name="compressedBuffer">The compressed buffer.</param>
        /// <returns>The decompressed buffer <paramref name="compressedBuffer"/>.</returns>
        internal static byte[] Decompress(this byte[] compressedBuffer)
        {
            if (compressedBuffer == null) throw new ArgumentNullException("compressedBuffer");

            using (var compressedStream = new MemoryStream(compressedBuffer))
            using (var gZipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var uncompressedStream = new MemoryStream())
            {
                // Copy the compressed stream to the uncompressed stream. The
                // GZip stream must then be closed to ensure everything is
                // written to the uncompressed stream.
                gZipStream.CopyTo(uncompressedStream);
                gZipStream.Close();

                return uncompressedStream.ToArray();
            }
        }
    }
}
