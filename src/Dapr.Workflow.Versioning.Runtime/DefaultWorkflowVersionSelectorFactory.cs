using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Workflow.Versioning;

/// <summary>
/// Default factory that constructs <see cref="IWorkflowVersionSelector"/> instances using DI and, when supported by
/// the selector, provides scope information for per-family configuration (canonical and options name).
/// </summary>
/// <remarks>
/// The factory uses the following resolution order to build the selector:
/// <list type="number">
///   <item><description>Attempt to resolve the exact selectorType from DI.</description></item>
///   <item><description>Fallback to <see cref="ActivatorUtilities.CreateInstance(IServiceProvider, Type, object[])"/> so constructor dependencies are injected.</description></item>
///   <item><description>Throw if neither path produces an instance.</description></item>
/// </list>
/// </remarks>
public sealed class DefaultWorkflowVersionSelectorFactory : IWorkflowVersionSelectorFactory
{
    /// <inheritdoc />
    public IWorkflowVersionSelector Create(Type selectorType, string canonicalName, string? optionsName,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(selectorType);
        ArgumentNullException.ThrowIfNull(services);
        
        // Prefer container resolution so any existing registrations (singletons, annotations, etc.) are honored
        var instance = services.GetService(selectorType) as IWorkflowVersionSelector ??
                       ActivatorUtilities.CreateInstance(services, selectorType) as IWorkflowVersionSelector;

        if (instance is null)
            throw new InvalidOperationException($"Could not construct selector of type '{selectorType.FullName}'. " +
                                                $"Ensure it implements {nameof(IWorkflowVersionSelector)} and is " +
                                                "resolvable via DI or has an injectable constructor.");

        return instance;
    }
}
