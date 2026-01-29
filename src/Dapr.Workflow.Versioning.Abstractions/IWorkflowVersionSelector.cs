namespace Dapr.Workflow.Versioning;

/// <summary>
/// Defines the policy that selects the "latest" workflow version from a set of candidates that share the same
/// canonical name.
/// </summary>
/// <remarks>
/// The selector may apply arbitrary rules (e.g., exclude pre-release tags, prefer a specific branch, or implement
/// canary behaviors) on top of the comparison semantics provided by the active strategy.
/// </remarks>
public interface IWorkflowVersionSelector
{
    /// <summary>
    /// Selects the "latest" version identity from a non-empty set of candidates.
    /// </summary>
    /// <param name="canonicalName">The canonical name shared by all <paramref name="candidates"/>.</param>
    /// <param name="candidates">The collection of workflow version identities to select from.</param>
    /// <param name="strategy">The active versioning strategy, used to order version strings or resolve tiebreakers.</param>
    /// <returns>The chosen latest <see cref="WorkflowVersionIdentity"/>.</returns>
    WorkflowVersionIdentity SelectLatest(string canonicalName, IReadOnlyCollection<WorkflowVersionIdentity> candidates,
        IWorkflowVersionStrategy strategy);
}
