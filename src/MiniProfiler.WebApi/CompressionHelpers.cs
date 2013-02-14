namespace MiniProfiler.WebApi
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Helper class to compress and decompress serialized data.
    /// </summary>
    internal static class CompressionHelpers
    {
        /// <summary>
        /// Decompresses the supplied <paramref name="compressedBytes"/>.
        /// </summary>
        /// <param name="compressedBytes">The compressed bytes.</param>
        /// <returns>The decompressed <paramref name="compressedBytes"/>.</returns>
        internal static MemoryStream Decompress(this byte[] compressedBytes)
        {
            using (var compressedStream = new MemoryStream(compressedBytes))
            using (var gZipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                // Copy the compressed stream to the uncompressed stream. The
                // GZip stream must then be closed to ensure everything is
                // written to the uncompressed stream.
                var uncompressedStream = new MemoryStream();
                gZipStream.CopyTo(uncompressedStream);
                gZipStream.Close();

                // Move to the beginning.
                uncompressedStream.Seek(0, SeekOrigin.Begin);

                return uncompressedStream;
            }
        }
    }
}
