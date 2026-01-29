using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Workflow.Versioning;

/// <summary>
/// Default factory that builds strategies via DI and supports named options binding through the resolved services.
/// </summary>
public sealed class DefaultWorkflowVersionStrategyFactory : IWorkflowVersionStrategyFactory
{
    /// <inheritdoc />
    public IWorkflowVersionStrategy Create(
        Type strategyType, 
        string canonicalName, 
        string? optionsName,
        IServiceProvider services)
    {
        
        if (strategyType is null) throw new ArgumentNullException(nameof(strategyType));
        if (services is null) throw new ArgumentNullException(nameof(services));

        // Prefer DI/ActivatorUtilities so constructor injection works.
        var instance = (IWorkflowVersionStrategy?)(
            services.GetService(strategyType) ??
            ActivatorUtilities.CreateInstance(services, strategyType));

        if (instance is null)
        {
            throw new InvalidOperationException(
                $"Could not construct strategy of type '{strategyType.FullName}'.");
        }

        return instance;
    }
}
