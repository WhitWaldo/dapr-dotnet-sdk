using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Workflow.Versioning;

/// <summary>
/// Dependency injection extensions for configuring workflow versioning runtime services.
/// </summary>
public static class WorkflowVersioningServiceCollectionExtensions
{
    /// <summary>
    /// Registers workflow versioning runtime services, including factories, resolver and diagnostics.
    /// </summary>
    /// <param name="services">The application's service collection.</param>
    /// <param name="configure">Optional delegate to set global defaults via <see cref="WorkflowVersioningOptions"/>.</param>
    /// <returns></returns>
    public static IServiceCollection AddDaprWorkflowVersioning(
        this IServiceCollection services,
        Action<WorkflowVersioningOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        // Options container for defaults
        var opts = new WorkflowVersioningOptions();
        configure?.Invoke(opts);
        
        // Register singletons for options, diagnostics, factories and resolver
        services.AddSingleton(opts);
        services.AddSingleton<IWorkflowVersionDiagnostics, DefaultWorkflowVersioningDiagnostics>();
        services.AddSingleton<IWorkflowVersionStrategyFactory, DefaultWorkflowVersionStrategyFactory>();
        services.AddSingleton<IWorkflowVersionSelectorFactory, DefaultWorkflowVersionSelectorFactory>();
        services.AddSingleton<IWorkflowVersionResolver, WorkflowVersionResolver>();

        return services;
    }
}
