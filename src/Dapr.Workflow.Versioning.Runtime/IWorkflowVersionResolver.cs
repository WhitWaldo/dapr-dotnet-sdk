namespace Dapr.Workflow.Versioning;

/// <summary>
/// Resolves the latest <see cref="WorkflowVersionIdentity"/> for a given <see cref="WorkflowFamily"/> using the
/// active strategy and selector.
/// </summary>
public interface IWorkflowVersionResolver
{
    /// <summary>
    /// Attempts to select the latest version for the provided family.
    /// </summary>
    /// <param name="family">The workflow family (canonical name and version candidates).</param>
    /// <param name="latest">On success, receives the selected latest identity.</param>
    /// <param name="diagnosticId">On failure, receives a stable diagnostic ID.</param>
    /// <param name="diagnosticMessage">On failure, receives a human-readable message.</param>
    /// <returns><see langword="true"/> if selection succeeded; otherwise <see langword="false"/>.</returns>
    bool TryGetLatest(WorkflowFamily family, out WorkflowVersionIdentity latest, out string? diagnosticId,
        out string? diagnosticMessage);
}
