using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dapr.AppCallback.Autogen.Grpc.v1;

namespace Dapr.Messaging.PublishSubscribe;

/// <summary>
/// 
/// </summary>
public abstract class DaprPublishSubscribeClient
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pubsubName"></param>
    /// <param name="topicName"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract IAsyncEnumerable<TopicResponse> SubscribeAsync(string pubsubName, string topicName, DaprSubscriptionOptions options, CancellationToken cancellationToken);

    //public abstract async Task<IReadOnlyList<TopicResponse>> SubscribeAsync(string pubsubName, string topicName);

    //public abstract async IAsyncEnumerable<TopicResponse> SubscribeAsync(string pubsubName, string topicName, [EnumeratorCancellation] CancellationToken cancellationToken);
}
