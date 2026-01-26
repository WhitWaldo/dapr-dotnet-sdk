using Dapr.AI.Conversation;
using Dapr.AI.Conversation.ConversationRoles;
using Dapr.Testcontainers.Common;
using Dapr.Testcontainers.Common.Options;
using Dapr.Testcontainers.Harnesses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.IntegrationTest.AI;

public sealed class ConversationTests
{
    [Fact]
    public async Task ShouldConverseWithConversationBuildingBlock()
    {
        var options = new DaprRuntimeOptions();
        var componentsDir = TestDirectoryManager.CreateTestDirectory("ai-conversation-components");

        await using var environment = await DaprTestEnvironment.CreateWithPooledNetworkAsync(needsActorState: false);
        await environment.StartAsync();

        var harness = new DaprHarnessBuilder(options, environment).BuildConversation(componentsDir);

        await using var testApp = await DaprHarnessBuilder.ForHarness(harness)
            .ConfigureServices(builder =>
            {
                builder.Services.AddSingleton<DaprConversationClient>(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();

                    // The harness provides these; align with the Workflow tests style that wires endpoints from IConfiguration.
                    var grpcEndpoint = config["DAPR_GRPC_ENDPOINT"];
                    var httpEndpoint = config["DAPR_HTTP_ENDPOINT"];

                    var clientBuilder = new DaprConversationClientBuilder(config);

                    // These extension points are provided by the shared Dapr client builder infrastructure.
                    if (!string.IsNullOrWhiteSpace(grpcEndpoint))
                        clientBuilder.UseGrpcEndpoint(grpcEndpoint);

                    if (!string.IsNullOrWhiteSpace(httpEndpoint))
                        clientBuilder.UseHttpEndpoint(httpEndpoint);

                    return clientBuilder.Build();
                });
            })
            .BuildAndStartAsync();

        using var scope = testApp.CreateScope();
        var conversationClient = scope.ServiceProvider.GetRequiredService<DaprConversationClient>();

        var inputs = new[]
        {
            new ConversationInput([
                new UserMessage
                {
                    Content = [
                        new MessageContent("Reply with exactly one short word: pong")
                    ]
                }
            ])
        };

        var conversationOptions =
            new ConversationOptions(Testcontainers.Constants.DaprComponentNames.ConversationComponentName);

        var response = await conversationClient.ConverseAsync(inputs, conversationOptions);

        Assert.NotNull(response);
        Assert.NotNull(response.Outputs);
        Assert.NotEmpty(response.Outputs);
        Assert.NotNull(response.ConversationId);

        var firstResult = response.Outputs[0];
        Assert.NotNull(firstResult.Choices);
        Assert.NotEmpty(firstResult.Choices);

        var firstChoice = firstResult.Choices[0];
        Assert.NotNull(firstChoice.Message);

        // We keep it flexible (models can add punctuation/newlines), but require we got actual text back.
        var text = firstChoice.Message.Content;
        Assert.False(string.IsNullOrWhiteSpace(text));
    }

    [Fact]
    public async Task ShouldSupportMultiTurnConversation()
    {
        var options = new DaprRuntimeOptions();
        var componentsDir = TestDirectoryManager.CreateTestDirectory("ai-conversation-multiturn-components");

        await using var environment = await DaprTestEnvironment.CreateWithPooledNetworkAsync(needsActorState: false);
        await environment.StartAsync();

        var harness = new DaprHarnessBuilder(options, environment).BuildConversation(componentsDir);
        await using var testApp = await DaprHarnessBuilder.ForHarness(harness)
            .ConfigureServices(builder =>
            {
                builder.Services.AddSingleton<DaprConversationClient>(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var grpcEndpoint = config["DAPR_GRPC_ENDPOINT"];
                    var httpEndpoint = config["DAPR_HTTP_ENDPOINT"];

                    var clientBuilder = new DaprConversationClientBuilder(config);

                    if (!string.IsNullOrWhiteSpace(grpcEndpoint))
                        clientBuilder.UseGrpcEndpoint(grpcEndpoint);

                    if (!string.IsNullOrWhiteSpace(httpEndpoint))
                        clientBuilder.UseHttpEndpoint(httpEndpoint);

                    return clientBuilder.Build();
                });
            })
            .BuildAndStartAsync();

        using var scope = testApp.CreateScope();
        var conversationClient = scope.ServiceProvider.GetRequiredService<DaprConversationClient>();

        var inputs = new[]
        {
            new ConversationInput([
                new SystemMessage
                {
                    Content = [
                        new MessageContent("You are a helpful assistant. Keep answers very short.")
                    ]
                },
                new UserMessage
                {
                    Content = [
                        new MessageContent("Remember this number: 42. Reply with OK.")
                    ]
                },
                new AssistantMessage
                {
                    Content = [
                    new MessageContent("OK")
                    ]
                },
                new UserMessage
                {
                    Content = [
                    new MessageContent("What number did I ask you to remember?")
                    ]
                }
            ])
        };

        var conversationOptions =
            new ConversationOptions(Testcontainers.Constants.DaprComponentNames.ConversationComponentName);

        var response = await conversationClient.ConverseAsync(inputs, conversationOptions);

        Assert.NotNull(response);
        Assert.NotNull(response.Outputs);
        Assert.NotEmpty(response.Outputs);
        Assert.NotNull(response.ConversationId);

        var text = response.Outputs[0].Choices[0].Message.Content;
        Assert.False(string.IsNullOrWhiteSpace(text));

        // Again, avoid brittle exact matching; just require the remembered value is present.
        Assert.Contains("42", text);
    }
}
