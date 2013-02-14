namespace MiniProfiler.WebApi
{
    using System;
    using Xunit;

    public class CompressionHelpersUnitTests
    {
        [Fact]
        public void CompressionHelpers_CompressDecompress_ResultsInSameData()
        {
            // Arrange
            var rand = new Random();
            var expected = new byte[1024];
            rand.NextBytes(expected);

            // Act
            byte[] actual = expected.Compress().Decompress();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
