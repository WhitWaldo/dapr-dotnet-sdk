using System.IO.Compression;
using System.Text;
using Dapr.Common.PayloadHandlers.Compression;
using Dapr.Common.PayloadHandlers.Serialization;
using Dapr.Messaging.PublishSubscribe;
using Dapr.Messaging.PublishSubscribe.Extensions;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
    .AddDaprPubSubClient(new DaprPubSubClientOptions
    {
        CompressionProviders = new List<ICompressionProvider> { new GzipCompressionProvider(CompressionLevel.SmallestSize) },
        SerializationProviders = new List<ISerializationProvider> { new JsonSerializer() }
    })
    .BuildServiceProvider();

var messagingClient = serviceProvider.GetRequiredService<DaprPublishSubscribeClient>();

//Processor for each of the messages returned from the subscription
async Task<TopicResponseAction> HandleMessage(TopicMessage message, CancellationToken cancellationToken = default)
{
    try
    {
        //Do something with the message
        Console.WriteLine(Encoding.UTF8.GetString(message.Data.Span));
        return await Task.FromResult(TopicResponseAction.Success);
    }
    catch
    {
        return await Task.FromResult(TopicResponseAction.Retry);
    }
}

//Create a dynamic streaming subscription and subscribe with a timeout of 20 seconds
var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
var subscription = await messagingClient.SubscribeAsync("pubsub", "myTopic",
    new DaprSubscriptionOptions(new MessageHandlingPolicy(TimeSpan.FromSeconds(10), TopicResponseAction.Retry)),
    HandleMessage, cancellationTokenSource.Token);

await Task.Delay(TimeSpan.FromMinutes(1));

//When you're done with the subscription, simply dispose of it
await subscription.DisposeAsync();
