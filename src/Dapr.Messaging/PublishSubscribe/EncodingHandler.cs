using System.Collections.Immutable;
using System.IO.Compression;
using Dapr.Common.PayloadHandlers.Compression;
using Dapr.Common.PayloadHandlers.Serialization;
using Exception = System.Exception;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Dapr.Messaging.PublishSubscribe;

/// <summary>
/// A sample handler to provide bidirectional serialization and compression operations.
/// </summary>
public sealed class EncodingHandler
{
    private readonly IReadOnlyDictionary<string, ICompressionProvider> compressionProviders;
    private readonly IReadOnlyDictionary<string, ISerializationProvider> serializationProviders;

    private const string SerializationProviderKey = "serialization_provider";
    private const string CompressionProviderKey = "compression_provider";

    /// <summary>
    /// Instantiates a new instance of an <see cref="EncodingHandler"/>.
    /// </summary>
    /// <param name="compressionProviders">Various DI-injected compression providers.</param>
    /// <param name="serializationProviders">Various DI-injected serialization providers.</param>
    public EncodingHandler(IEnumerable<ICompressionProvider> compressionProviders,
        IEnumerable<ISerializationProvider> serializationProviders)
    {
        this.compressionProviders = compressionProviders.ToImmutableDictionary(kvp => kvp.EncodingName, kvp => kvp);
        this.serializationProviders = serializationProviders.ToImmutableDictionary(kvp => kvp.SerializationName, kvp => kvp);
    }
    
    /// <summary>
    /// Serializes and optionally compressed data according to the specified providers.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="serializerProviderName">The name of the serialization provider to use.</param>
    /// <param name="compressionProviderName">The optional name of the compression provider to use.</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<EncodedPayload> SerializeAsync(object data, string serializerProviderName, string? compressionProviderName)
    {
        if (!serializationProviders.TryGetValue(serializerProviderName, out var serializationProvider))
        {
            throw new Exception(
                $"Unable to locate indicated serialization provider by name '{serializerProviderName}'. Please make sure it was properly registered in your dependency injection provider.");
        }

        //Serialize the data
        var encodedData = serializationProvider.Serialize(data);

        //Optionally compress the data
        if (compressionProviderName is not null)
        {
            if (!compressionProviders.TryGetValue(compressionProviderName, out var compressionProvider))
            {
                throw new Exception(
                    $"Unable to locate indicated compression provider by name '{compressionProviderName}'. Please make sure it was properly registered in your dependency injection provider.");
            }

            using var compressedBytes = new MemoryStream();
            await using var compressionStream = compressionProvider.CreateCompressionStream(compressedBytes, CompressionLevel.SmallestSize);
            await compressionStream.WriteAsync(encodedData);

            //Write out the compressed bytes to a byte array
            encodedData = compressedBytes.ToArray();
        }

        //Create the metadata payload
        var metadata = new Dictionary<string, string> { { SerializationProviderKey, serializerProviderName } };
        if (compressionProviderName is not null) metadata.Add(CompressionProviderKey, compressionProviderName);
        return new EncodedPayload(encodedData, metadata);
    }

    /// <summary>
    /// Deserializes the specified payload to the indicated type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="payload">The payload to deserialize.</param>
    /// <returns>A nullable deserialized value.</returns>
    /// <exception cref="Exception"></exception>
    public async Task<T?> DeserializeAsync<T>(EncodedPayload payload)
    {
        var payloadBytes = payload.Payload;

        //Decompress the bytes if any compression provider was specified
        if (payload.Metadata.TryGetValue(CompressionProviderKey, out var compressionProviderName))
        {
            if (!compressionProviders.TryGetValue(compressionProviderName, out var compressionProvider))
                throw new Exception(
                    $"Unable to locate indicated compression provider by name '{compressionProviderName}'. Please make sure it was properly registered in your dependency injection provider.");

            using var decompressedBytes = new MemoryStream();
            await using var decompressionStream = compressionProvider.CreateDecompressionStream(decompressedBytes);
            await decompressionStream.WriteAsync(payloadBytes);

            //Write out the decompressed bytes to the payload byte array
            payloadBytes = decompressedBytes.ToArray();
        }
        
        if (!payload.Metadata.TryGetValue(SerializationProviderKey, out var serializationProviderName))
            throw new Exception("Unable to determine serialization provider for payload based on message metadata"); //In practice, try deserializing to JSON by default

        if (!serializationProviders.TryGetValue(serializationProviderName, out var serializationProvider))
            throw new Exception(
                $"Unable to locate indicated serialization provider by name '{serializationProviderName}'. Please make sure it was properly registered in your dependency injection provider.");

        //Deserialize the bytes
        return serializationProvider.Deserialize<T>(payloadBytes);
    }
}

/// <summary>
/// Contains the encoded message payload.
/// </summary>
/// <param name="Payload">The bytes comprising the encoded payload.</param>
/// <param name="Metadata">Metadata containing information about the various providers used to encode the payload.</param>
public sealed record EncodedPayload(byte[] Payload, Dictionary<string, string> Metadata)
{
    /// <summary>
    /// Serializes this instance to a JSON string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => JsonSerializer.Serialize(this);

    /// <summary>
    /// Deserializes a JSON string into an instance of an <see cref="EncodedPayload"/>.
    /// </summary>
    /// <param name="serializedPayload">The JSON serialized payload.</param>
    /// <returns></returns>
    public static EncodedPayload? FromString(string serializedPayload) => JsonSerializer.Deserialize<EncodedPayload>(serializedPayload);
}
