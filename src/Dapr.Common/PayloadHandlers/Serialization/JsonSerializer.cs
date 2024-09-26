using System.Text.Json;

namespace Dapr.Common.PayloadHandlers.Serialization;

/// <summary>
/// JSON serialization provider.
/// </summary>
public sealed class JsonSerializer : ISerializationProvider
{
    private readonly JsonSerializerOptions? _serializationOptions;

    /// <summary>
    /// The name of the serialization operation.
    /// </summary>
    public string SerializationName => "json";

    /// <summary>
    /// Initializes a new instance of a <see cref="JsonSerializer"/>.
    /// </summary>
    /// <param name="options"></param>
    public JsonSerializer(JsonSerializerOptions? options = null)
    {
        _serializationOptions = options;
    }

    /// <summary>
    /// Serializes an object to an array of bytes.
    /// </summary>
    /// <param name="data">The data to serialize.</param>
    /// <returns>The array of serialized bytes.</returns>
    public byte[] Serialize(object data) => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data, _serializationOptions);

    /// <summary>
    /// Deserializes the bytes to a specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="serializedBytes">The bytes to deserialize.</param>
    /// <returns>The deserialized type.</returns>
    public T? Deserialize<T>(byte[] serializedBytes) =>
        System.Text.Json.JsonSerializer.Deserialize<T>(serializedBytes, _serializationOptions);
}
