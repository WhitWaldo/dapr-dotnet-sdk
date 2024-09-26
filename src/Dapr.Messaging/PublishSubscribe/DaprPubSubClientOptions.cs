using Dapr.Common.PayloadHandlers.Compression;
using Dapr.Common.PayloadHandlers.Serialization;
using Grpc.Net.Client;

namespace Dapr.Messaging.PublishSubscribe;

/// <summary>
/// Options used to configure the DaprPubSub client.
/// </summary>
public sealed record DaprPubSubClientOptions
{
    /// <summary>
    /// Adds the provided token on every request to the Dapr runtime.
    /// </summary>
    public string? DapiApiToken { get; init; }

    /// <summary>
    /// Sets the timeout for requests made by the Dapr client.
    /// </summary>
    public TimeSpan? RequestTimeout { get; init; }

    /// <summary>
    /// Options used to create the GrpcChannel.
    /// </summary>
    public GrpcChannelOptions? GrpcChannelOptions { get; init; }

    /// <summary>
    /// The gRPC endpoint used by the Dapr client to communicate with the Dapr runtime.
    /// </summary>
    public string? GrpcEndpoint { get; init; }

    /// <summary>
    /// A collection of compression providers.
    /// </summary>
    public IList<ICompressionProvider> CompressionProviders { get; init; } = new List<ICompressionProvider>();

    /// <summary>
    /// A collection of serialization providers.
    /// </summary>
    public IList<ISerializationProvider> SerializationProviders { get; init; } = new List<ISerializationProvider>();
}
