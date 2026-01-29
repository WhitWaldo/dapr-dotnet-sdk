namespace Dapr.Workflow.Versioning;

/// <summary>
/// Default selector that chooses the maximum version according to the active
/// <see cref="IWorkflowVersionStrategy.Compare(string, string)"/> implementation. 
/// </summary>
public sealed class MaxVersionSelector : IWorkflowVersionSelector
{
    /// <inheritdoc />
    public WorkflowVersionIdentity SelectLatest(string canonicalName, IReadOnlyCollection<WorkflowVersionIdentity> candidates,
        IWorkflowVersionStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentOutOfRangeException.ThrowIfEqual(0, candidates.Count, nameof(candidates));
        
        var latest = candidates.OrderBy(v => v.Version, strategy).Last();
        return latest;
    }
}
