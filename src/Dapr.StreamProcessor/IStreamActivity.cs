namespace Dapr.StreamProcessor;

/// <summary>
/// Identifies a source observable that doesn't itself accept a reactive input.
/// </summary>
/// <typeparam name="TResult"></typeparam>
internal interface IStreamSource<out TResult> : IObservable<TResult>
{
    public IAsyncEnumerable<TResult> RunAsync();
}

/// <summary>
/// Base implementation of the multi stream operator interfaces.
/// </summary>
/// <typeparam name="TResult">The output type.</typeparam>
internal interface IMultiStreamOperatorBase<out TResult> : IObservable<TResult>, IEventStreamResource
{
    public IAsyncEnumerable<TResult> RunAsync(params IAsyncEnumerable<object>[] inputs);
}

/// <summary>
/// Identifies an operator that does not accept an input, but returns an output stream of a specified type.
/// </summary>
/// <typeparam name="TResult">The output type.</typeparam>
internal interface ISourceOperator<out TResult> : IMultiStreamOperatorBase<TResult>
{
    public IAsyncEnumerable<TResult> RunAsync();
}

/// <summary>
/// Identifies an operator that accepts two streams of data and returns a stream of a specified output type.
/// </summary>
/// <typeparam name="TSource1">The first input type.</typeparam>
/// <typeparam name="TSource2">The second input type.</typeparam>
/// <typeparam name="TResult">The output type.</typeparam>
internal interface IMultiStreamOperator<in TSource1, in TSource2, out TResult> : IMultiStreamOperatorBase<TResult>
{
    public IAsyncEnumerable<TResult> RunAsync(IAsyncEnumerable<TSource1> input1, IAsyncEnumerable<TSource2> input2);
}

/// <summary>
/// Identifies an operator that accepts a stream of a single type and returns a stream of an output type.
/// </summary>
/// <typeparam name="TSource">The input type.</typeparam>
/// <typeparam name="TResult">The output type.</typeparam>
internal interface IUnaryStreamOperator<in TSource, out TResult> : IObserver<TSource>, IObservable<TResult>
{
    public IAsyncEnumerable<TResult> RunAsync(IAsyncEnumerable<TSource> input);
}

/// <summary>
/// Identifies a subscriber that subscribes to zero or more sources, but doesn't produce an observable output.
/// </summary>
/// <typeparam name="TSource"></typeparam>
internal interface IStreamSubscriber<in TSource> : IObserver<TSource>
{
    public ValueTask RunAsync(IAsyncEnumerable<TSource> input);
}

internal interface IEventStreamResource
{
    public string Name { get;  }
}


internal interface IStreamPipelineBuilder
{
    public IStreamPipelineBuilder AddSource(string name);
}

internal interface IStreamSource : IStreamPipelineBuilder
{

}

internal abstract class EventSource<T> : ISourceOperator<T>
{
    public IAsyncEnumerable<T> RunAsync()
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<T> RunAsync(params IAsyncEnumerable<object>[] inputs)
    {
        throw new NotImplementedException();
    }

    public abstract string Name { get; }

    /// <summary>Notifies the provider that an observer is to receive notifications.</summary>
    /// <param name="observer">The object that is to receive notifications.</param>
    /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        throw new NotImplementedException();
    }
}

internal abstract class EventOperator : IEventStreamResource
{
    public abstract string Name { get; }
}

internal abstract class EventSubscriber : IEventStreamResource
{
    public abstract string Name { get; }
}
