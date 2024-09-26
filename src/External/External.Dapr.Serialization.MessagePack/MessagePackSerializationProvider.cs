using Dapr.Common.PayloadHandlers.Serialization;

namespace External.Dapr.Serialization;

/// <summary>
/// 
/// </summary>
public sealed class MessagePackSerializationProvider : ISerializationProvider
{
    /// <summary>
    /// The name of the serialization operation.
    /// </summary>
    public string SerializationName => "msgpack";

    /// <summary>
    /// Serializes an object to an array of bytes.
    /// </summary>
    /// <param name="data">The data to serialize.</param>
    /// <returns>The array of serialized bytes.</returns>
    public byte[] Serialize(object data)
    {

        throw new NotImplementedException();
    }

    /// <summary>
    /// Deserializes the bytes to a specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="serializedBytes">The bytes to deserialize.</param>
    /// <returns>The deserialized type.</returns>
    public T Deserialize<T>(byte[] serializedBytes)
    {
        throw new NotImplementedException();
    }
}
