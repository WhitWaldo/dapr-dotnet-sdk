using CommunityToolkit.Aspire.Hosting.Dapr;
using Projects;

var builder = DistributedApplication.CreateBuilder(args); //Entry point for Aspire applications

// Defines a state store and assigns to a variable so it can be referenced by later projects
var daprStateStore =
    builder.AddDaprStateStore("kvstate", new DaprComponentOptions
    {
        LocalPath = "../dapr/statestore.yaml" //Provides a path relative to the AppHost project to locate the component definition at
    });

// Defines a PubSub component and assigns to a variable so it can be referenced by later projects 
var daprPubSub = builder.AddDaprPubSub("pubsub", new DaprComponentOptions
{
    LocalPath = "../dapr/pubsub.yaml" //Provides a path relative to the AppHost project to locate the component definition at
});

var processor = builder.AddProject<DaprAspire>("processor") // Typed to the project it's referring to, the name is used by Aspire, but not by Dapr
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "processor", // The app ID provided to Dapr
        DaprGrpcPort = 50002, // The gRPC port used to connect to the Dapr sidecar (though not needed in this sample)
        DaprHttpPort = 3501 // The HTTP port used to connect to the Dapr sidecar
    })
    .WithReference(daprStateStore) // Provides access to the state store component defined in the resource above
    .WithReference(daprPubSub); // Provides access to the PubSub component defined in the resource above

builder.AddProject<DaprAspireWeb>("web") // Doesn't need to be assigned to a variable as nothing references it
    .WithDaprSidecar(new DaprSidecarOptions
    {
        AppId = "web", // The app ID provided to Dapr 
        DaprGrpcPort = 50003, // The gRPC port used to connect to the Dapr sidecar (though not needed in this sample) 
        DaprHttpPort = 3502 // The HTTP port used to connect ot the Dapr sidecar
    })
    .WithHttpEndpoint(name: "web-endpoint", port: 5001, isProxied: false)
    .WithExternalHttpEndpoints()
    .WithReference(processor)
    .WithReference(daprPubSub); // Provides access to the PubSub component defined in the resource above

builder.Build().Run();
