using Microsoft.Extensions.Configuration;

namespace Dapr.Common;

/// <summary>
/// Helper utility in building a shared Dapr configuration.
/// </summary>
public sealed class DaprConfigurationBuilder
{
    /// <summary>
    /// The name of the environment variable storing the Dapr API token.
    /// </summary>
    public const string DaprApiTokenName = "DAPR_API_TOKEN";
    /// <summary>
    /// The name of the environment variable storing the Dapr app token.
    /// </summary>
    public const string AppApiTokenName = "APP_API_TOKEN";
    /// <summary>
    /// The name of the environment variable storing the HTTP endpoint value.
    /// </summary>
    public const string DaprHttpEndpointName = "DAPR_HTTP_ENDPOINT";
    /// <summary>
    /// The name of the environment variable storing the HTTP port value.
    /// </summary>
    public const string DaprHttpPortName = "DAPR_HTTP_PORT";
    /// <summary>
    /// The name of the environment variable storing the gRPC endpoint value.
    /// </summary>
    public const string DaprGrpcEndpointName = "DAPR_GRPC_ENDPOINT";
    /// <summary>
    /// The name of the environment variable storing the gRPC port value.
    /// </summary>
    public const string DaprGrpcPortName = "DAPR_GRPC_PORT";
    /// <summary>
    /// The default HTTP port for the Dapr sidecar.
    /// </summary>
    public const int DefaultHttpPort = 3500;
    /// <summary>
    /// The default gRPC port for the Dapr sidecar.
    /// </summary>
    public const int DefaultGrpcPort = 50001;

    private readonly IConfiguration? configuration;

    /// <summary>
    /// Instantiates a new <see cref="DaprConfigurationBuilder"/>.
    /// </summary>
    /// <param name="configuration">An optional <see cref="IConfiguration"/> instance to source configuration values from.</param>
    public DaprConfigurationBuilder(IConfiguration? configuration)
    {
        this.configuration = configuration;
    }

    /// <summary>
    /// Builds the Dapr HTTP endpoint using the value from the IConfiguration, if available, then falling back
    /// to the value in the environment variable(s) and finally otherwise using the default value (an empty string).
    /// </summary>
    public Uri GetHttpEndpoint(string? optionsEndpoint)
    {
        if (optionsEndpoint is not null)
            return new Uri(optionsEndpoint);

        //Prioritize pulling from IConfiguration with a fallback of pulling from the environment variable directly
        var httpEndpoint = GetResourceValue(DaprHttpEndpointName);
        var httpPort = GetResourceValue(DaprHttpPortName);
        int? parsedHttpPort = string.IsNullOrWhiteSpace(httpPort) ? null : int.Parse(httpPort);

        var endpoint = BuildEndpoint(httpEndpoint, parsedHttpPort);
        var uri = new Uri(string.IsNullOrWhiteSpace(endpoint) ? $"http://localhost:{DefaultHttpPort}/" : endpoint);

        if (uri.Scheme != "http" && uri.Scheme != "https")
            throw new InvalidOperationException("The HTTP endpoint must use http or https.");

        return uri;
    }

    /// <summary>
    /// Builds the Dapr gRPC endpoint using the value from the IConfiguration, if available, then falling back
    /// to the value in the environment variable(s) and finally otherwise using the default value (an empty string).
    /// </summary>
    public Uri GetGrpcEndpoint(string? optionsEndpoint)
    {
        if (optionsEndpoint is not null)
            return new Uri(optionsEndpoint);

        //Prioritize pulling from IConfiguration with a fallback from pulling from the environment variable directly
        var grpcEndpoint = GetResourceValue(DaprGrpcEndpointName);
        var grpcPort = GetResourceValue(DaprGrpcPortName);
        int? parsedGrpcPort = string.IsNullOrWhiteSpace(grpcPort) ? null : int.Parse(grpcPort);

        var endpoint = BuildEndpoint(grpcEndpoint, parsedGrpcPort);
        var uri = new Uri(string.IsNullOrWhiteSpace(endpoint) ? $"http://localhost:{DefaultGrpcPort}/" : endpoint);

        if (uri.Scheme != "http" && uri.Scheme != "https")
            throw new InvalidOperationException("The gRPC endpoint must use http or https.");

        if (uri.Scheme.Equals(Uri.UriSchemeHttp))
        {
            // Set correct switch to make secure gRPC service calls. This switch must be set before creating the GrpcChannel.
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        return uri;
    }

    /// <summary>
    /// Retrieves the Dapr API token first from the <see cref="IConfiguration"/>, if available, then falling back
    /// to the value in the environment variable(s) directly, then finally otherwise using the default value (an
    /// empty string).
    /// </summary>
    public string GetApiToken(string? token) => token ?? GetResourceValue(DaprApiTokenName);

    /// <summary>
    /// Retrieves the specified value prioritizing pulling it from <see cref="IConfiguration"/>, falling back
    /// to an environment variable, and using an empty string as a default.
    /// </summary>
    /// <param name="name">The name of the value to retrieve.</param>
    /// <returns>The value of the resource.</returns>
    private string GetResourceValue(string name)
    {
        //Attempt to retrieve first from the configuration
        var configurationValue = configuration?.GetValue<string?>(name);
        if (configurationValue is not null)
            return configurationValue;

        //Fall back to the environment variable with the same name or default to an empty string
        var envVar = Environment.GetEnvironmentVariable(name);
        return envVar ?? string.Empty;
    }

    /// <summary>
    /// Builds the endpoint provided an optional endpoint and optional port.
    /// </summary>
    /// <remarks>
    /// Marked as internal for testing purposes.
    /// </remarks>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="endpointPort">The port</param>
    /// <returns>A constructed endpoint value.</returns>
    private static string BuildEndpoint(string? endpoint, int? endpointPort)
    {
        if (string.IsNullOrWhiteSpace(endpoint) && endpointPort is null)
            return string.Empty;

        var endpointBuilder = new UriBuilder();
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            //Extract the scheme, host and port from the endpoint
            var uri = new Uri(endpoint);
            endpointBuilder.Scheme = uri.Scheme;
            endpointBuilder.Host = uri.Host;
            endpointBuilder.Port = uri.Port;

            //Update the port if provided separately
            if (endpointPort is not null)
                endpointBuilder.Port = (int)endpointPort;
        }
        else if (string.IsNullOrWhiteSpace(endpoint) && endpointPort is not null)
        {
            endpointBuilder.Host = "localhost";
            endpointBuilder.Port = (int)endpointPort;
        }

        return endpointBuilder.ToString();
    }
}
