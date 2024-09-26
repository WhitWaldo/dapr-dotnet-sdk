namespace Dapr.Common.PayloadHandlers.Serialization;

/// <summary>
/// Identifies a payload handler used for serialization operations.
/// </summary>
public interface ISerializationProvider
{
    /// <summary>
    /// The name of the serialization operation.
    /// </summary>
    string SerializationName { get; }

    /// <summary>
    /// Serializes an object to an array of bytes.
    /// </summary>
    /// <param name="data">The data to serialize.</param>
    /// <returns>The array of serialized bytes.</returns>
    byte[] Serialize(object data);

    /// <summary>
    /// Deserializes the bytes to a specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="serializedBytes">The bytes to deserialize.</param>
    /// <returns>The deserialized type.</returns>
    T? Deserialize<T>(byte[] serializedBytes);
}
