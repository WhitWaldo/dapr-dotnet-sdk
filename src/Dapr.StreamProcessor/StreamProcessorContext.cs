namespace Dapr.StreamProcessor;

/// <summary>
/// Provides the context for a stream processor.
/// </summary>
public abstract class StreamProcessorContext
{
    /// <summary>
    /// The name of the processor context.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Used to register observable sources.
    /// </summary>
    /// <returns></returns>
    public abstract ValueTask<IAsyncEnumerable<TOut>> DefineObserverAsync<TOut>(string identifier, Action<TOut> action);

    /// <summary>
    /// Used to register observable operators.
    /// </summary>
    /// <returns></returns>
    public abstract ValueTask<IAsyncEnumerable<TOut>> DefineObservableAsync<TIn, TOut>(string identifier, Func<TIn, TOut> action);

    /// <summary>
    /// Used to subscribe to observable streams.
    /// </summary>
    /// <returns></returns>
    public abstract Task SubscribeAsync();
}
