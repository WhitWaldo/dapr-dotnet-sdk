namespace Dapr.Workflow.Versioning;

/// <summary>
/// Provides application-wife configuration for workflow versioning behavior, including default for the versioning
/// strategy and version selector.
/// </summary>
/// <remarks>
/// <para>
/// These options are consumed by the workflow versioning runtime when resolving the <see cref="IWorkflowVersionStrategy"/>
/// and <see cref="IWorkflowVersionSelector"/> used to parse, compare and select workflow versions at startup.
/// </para>
/// <para>
/// Both <see cref="DefaultStrategy"/> and <see cref="DefaultSelector"/> are specified as <see cref="Func{T,TResult}"/>
/// delegates so the runtime can resolve instances on demand using dependency injection and/or a custom factory. This enables
/// scenarios where different scopes or named options are applied per canonical workflow family.
/// </para>
/// </remarks>
public sealed class WorkflowVersioningOptions
{
    /// <summary>
    /// Gets or sets a factory delegate that returns the default <see cref="IWorkflowVersionStrategy"/> to use when a
    /// workflow type does not specify a <c>StrategyType</c> override via <see cref="WorkflowVersionAttribute"/>.
    /// </summary>
    public Func<IServiceProvider, IWorkflowVersionStrategy>? DefaultStrategy { get; set; }
    
    /// <summary>
    /// Gets or sets a factory delegate that returns the default <see cref="IWorkflowVersionSelector"/> used to choose
    /// the "latest" version from a set of candidates that share the same canonical name.
    /// </summary>
    public Func<IServiceProvider, IWorkflowVersionSelector>? DefaultSelector { get; set; }
}
