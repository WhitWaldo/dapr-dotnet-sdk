using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using C = Dapr.AppCallback.Autogen.Grpc.v1;
using P = Dapr.Client.Autogen.Grpc.v1;

namespace Dapr.Messaging.PublishSubscribe;



/// <summary>
/// 
/// </summary>
public sealed class DaprPublishSubscribeGrpcClient : DaprPublishSubscribeClient
{
    /// <summary>
    /// 
    /// </summary>
    private readonly P.Dapr.DaprClient _client;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="options"></param>
    public DaprPublishSubscribeGrpcClient(GrpcChannel channel, DaprPublishSubscribeClientOptions? options)
    {
        _client = new P.Dapr.DaprClient(channel);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pubsubName">The name of the pub/sub component.</param>
    /// <param name="topicName">The name of the topic.</param>
    /// <param name="options"></param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns></returns>
    public override IAsyncEnumerable<TopicResponse> SubscribeAsync(string pubsubName, string topicName, DaprSubscriptionOptions options, CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<TopicResponse>();
        _ = FetchDataFromSidecar(pubsubName, topicName, options, channel.Writer, cancellationToken);
        return ReadMessagesFromChannelAsync(channel.Reader, cancellationToken);
    }

    private async IAsyncEnumerable<TopicResponse> ReadMessagesFromChannelAsync(ChannelReader<TopicResponse> reader,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await reader.WaitToReadAsync(cancellationToken))
        {
            while (reader.TryRead(out var message))
                yield return message;
        }
    }

    private async Task FetchDataFromSidecar(string pubsubName, string topicName, DaprSubscriptionOptions options, ChannelWriter<TopicResponse> channelWriter, CancellationToken cancellationToken)
    {
        try
        {
            using var result = this._client.SubscribeTopicEventsAlpha1(cancellationToken: cancellationToken);
            var initialRequest = new P.SubscribeTopicEventsInitialRequestAlpha1
            {
                PubsubName = pubsubName,
                Topic = topicName,
                DeadLetterTopic = options?.DeadLetterTopic ?? string.Empty
            };

            if (options?.Metadata.Count > 0)
            {
                foreach (var (key, value) in options.Metadata)
                {
                    initialRequest.Metadata.Add(key, value);
                }
            }

            await result.RequestStream.WriteAsync(new() { InitialRequest = initialRequest }, cancellationToken);

            await foreach (var response in result.ResponseStream.ReadAllAsync(cancellationToken))
            {
                var resp = new TopicResponse
                {
                    Id = response.Id,
                    Source = response.Source,
                    Type = response.Type,
                    SpecVersion = response.SpecVersion,
                    DataContentType = response.DataContentType,
                    Data = response.Data.Memory,
                    Topic = response.Topic,
                    PubSubName = response.PubsubName,
                    Path = response.Path,
                    Extensions = response.Extensions
                };

                await channelWriter.WriteAsync(resp, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            //Ignore our own cancellation
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled &&
                                      cancellationToken.IsCancellationRequested)
        {
            //Ignore a remote cancellation due to our own cancellation
        }
        finally
        {
            channelWriter.Complete();
        }
    }
}
