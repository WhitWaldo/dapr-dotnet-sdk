using System.IO.Compression;

namespace Dapr.Common.PayloadHandlers.Compression;

/// <summary>
/// Brotli compression provider.
/// </summary>
public sealed class BrotliCompressionProvider : ICompressionProvider
{
    private readonly CompressionLevel _defaultCompressionLevel;

    /// <summary>
    /// Initializes a new instance of a <see cref="BrotliCompressionProvider"/>.
    /// </summary>
    /// <param name="defaultCompressionLevel">The default compression level to use when compressing data.</param>
    public BrotliCompressionProvider(CompressionLevel defaultCompressionLevel)
    {
        _defaultCompressionLevel = defaultCompressionLevel;
    }

    /// <summary>
    /// The name of the compression encoding.
    /// </summary>
    public string EncodingName => "brotli";

    /// <summary>
    /// Creates a new compression stream.
    /// </summary>
    /// <param name="stream">The stream that compressed data is written to.</param>
    /// <param name="compressionLevel">The compression level.</param>
    /// <returns>A stream used to compress data.</returns>
    public Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel) => new BrotliStream(stream, compressionLevel ?? _defaultCompressionLevel, leaveOpen: true);

    /// <summary>
    /// Creates a decompression stream.
    /// </summary>
    /// <param name="stream">The stream the compressed data is copied from.</param>
    /// <returns>A stream used to decompress data.</returns>
    public Stream CreateDecompressionStream(Stream stream) => new BrotliStream(stream, CompressionMode.Decompress, leaveOpen: true);
}
