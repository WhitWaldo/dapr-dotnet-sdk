using System.IO.Compression;

namespace Dapr.Common.PayloadHandlers.Compression;

/// <summary>
/// Identifies a payload handler responsible for compression operations.
/// </summary>
public interface ICompressionProvider
{
    /// <summary>
    /// The name of the compression encoding.
    /// </summary>
    string EncodingName { get; }

    /// <summary>
    /// Creates a new compression stream.
    /// </summary>
    /// <param name="stream">The stream that compressed data is written to.</param>
    /// <param name="compressionLevel">The compression level.</param>
    /// <returns>A stream used to compress data.</returns>
    Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel);

    /// <summary>
    /// Creates a decompression stream.
    /// </summary>
    /// <param name="stream">The stream the compressed data is copied from.</param>
    /// <returns>A stream used to decompress data.</returns>
    Stream CreateDecompressionStream(Stream stream);
}
