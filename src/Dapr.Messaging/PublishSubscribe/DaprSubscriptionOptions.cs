namespace Dapr.Messaging.PublishSubscribe;

/// <summary>
/// Options used to configure the dynamic Dapr subscription.
/// </summary>
public sealed record DaprSubscriptionOptions
{
    /// <summary>
    /// Subscription metadata.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// The optional name of the dead-letter topic to send messages to.
    /// </summary>
    public string? DeadLetterTopic { get; init; }
}
