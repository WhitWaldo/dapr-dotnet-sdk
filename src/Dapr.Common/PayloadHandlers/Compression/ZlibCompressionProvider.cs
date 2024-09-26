using System.IO.Compression;

namespace Dapr.Common.PayloadHandlers.Compression;

/// <summary>
/// ZLib compression provider.
/// </summary>
public sealed class ZlibCompressionProvider : ICompressionProvider
{
    private readonly CompressionLevel _defaultCompressionLevel;

    /// <summary>
    /// Initializes a new instance of a <see cref="ZlibCompressionProvider"/>.
    /// </summary>
    /// <param name="defaultCompressionLevel">The default compression level to use when compressing data.</param>
    public ZlibCompressionProvider(CompressionLevel defaultCompressionLevel)
    {
        _defaultCompressionLevel = defaultCompressionLevel;
    }

    /// <summary>
    /// The name of the compression encoding.
    /// </summary>
    public string EncodingName => "zlib";

    /// <summary>
    /// Creates a new compression stream.
    /// </summary>
    /// <param name="stream">The stream that compressed data is written to.</param>
    /// <param name="compressionLevel">The compression level.</param>
    /// <returns>A stream used to compress data.</returns>
    public Stream CreateCompressionStream(Stream stream, CompressionLevel? compressionLevel) => new ZLibStream(stream, compressionLevel ?? _defaultCompressionLevel, leaveOpen: true);

    /// <summary>
    /// Creates a decompression stream.
    /// </summary>
    /// <param name="stream">The stream the compressed data is copied from.</param>
    /// <returns>A stream used to decompress data.</returns>
    public Stream CreateDecompressionStream(Stream stream) => new ZLibStream(stream, CompressionMode.Decompress, leaveOpen: true);
}
