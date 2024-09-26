// ------------------------------------------------------------------------
// Copyright 2024 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using System.Reflection;
using Dapr.Common;
using Dapr.Common.PayloadHandlers.Compression;
using Dapr.Common.PayloadHandlers.Serialization;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Dapr.Messaging.PublishSubscribe.Extensions;

/// <summary>
/// Contains extension methods for using Dapr Publish/Subscribe with dependency injection.
/// </summary>
public static class PublishSubscribeServiceCollectionExtensions
{
    /// <summary>
    /// Adds Dapr Publish/Subscribe support to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="options">Various options to override configuration values with.</param>
    /// <returns></returns>
    public static IServiceCollection AddDaprPubSubClient(this IServiceCollection services, DaprPubSubClientOptions? options = null) => AddDaprPubSubClient(services, options, null);

    /// <summary>
    /// Adds Dapr Publish/Subscribe support to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="options">Various options to override configuration values with.</param>
    /// <param name="configure">Optionally allows greater configuration of the <see cref="DaprPublishSubscribeClient"/> using injected services.</param>
    /// <returns></returns>
    public static IServiceCollection AddDaprPubSubClient(this IServiceCollection services, DaprPubSubClientOptions? options, Action<IServiceProvider>? configure)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddSingleton<DaprConfigurationBuilder>();

        //Register each of the serialization and compression providers, if any
        if (options is not null)
        {
            foreach (var serializationProvider in options.SerializationProviders) 
                services.TryAddSingleton<ISerializationProvider>(_ => serializationProvider);

            foreach (var compressionProvider in options.CompressionProviders) 
                services.TryAddSingleton<ICompressionProvider>(_ => compressionProvider);
        }

        //Register the IHttpClientFactory implementation
        services.AddHttpClient();

        services.TryAddScoped<EncodingHandler>();
        
        services.TryAddSingleton(serviceProvider =>
        {
            configure?.Invoke(serviceProvider);

            var configurationBuilder = serviceProvider.GetRequiredService<DaprConfigurationBuilder>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory?>();
            var grpcEndpoint = configurationBuilder.GetGrpcEndpoint(options?.GrpcEndpoint);

            //Provision and set up the HttpClient
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var httpClient = httpClientFactory is not null ? httpClientFactory.CreateClient() : new HttpClient();
            // HTTP Client timeout
            if (options?.RequestTimeout is not null && options.RequestTimeout > TimeSpan.Zero)
                httpClient.Timeout = (TimeSpan)options.RequestTimeout;
            //Dapr API token
            if (!string.IsNullOrWhiteSpace(options?.DapiApiToken) || !string.IsNullOrWhiteSpace(configurationBuilder.GetApiToken(options.DapiApiToken)))
                httpClient.DefaultRequestHeaders.Add("dapr-api-token", options.DapiApiToken);
            //User Agent
            httpClient.DefaultRequestHeaders.Add("dapr-sdk-dotnet", GetUserAgent());

            var channel = GrpcChannel.ForAddress(grpcEndpoint, options.GrpcChannelOptions ?? new GrpcChannelOptions
            {
                ThrowOperationCanceledOnCancellation = true,
                HttpClient = httpClient,
                LoggerFactory = loggerFactory
            });

            return new Client.Autogen.Grpc.v1.Dapr.DaprClient(channel);
        });

        services.TryAddSingleton<DaprPublishSubscribeClient, DaprPublishSubscribeGrpcClient>();

        return services;
    }

    /// <summary>
    /// Gets the user-agent header value.
    /// </summary>
    private static string GetUserAgent()
    {
        var assembly = typeof(DaprPublishSubscribeClient).Assembly;
        var assemblyVersion = assembly
            .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
            .FirstOrDefault()?
            .InformationalVersion;

        return $"v{assemblyVersion}";
    }
}
