namespace Dapr.Workflow.Versioning;

/// <summary>
/// Represents all discovered versions of a workflow family that share the same canonical name.
/// </summary>
/// <param name="CanonicalName">Gets the canonical name shared by the versions in this family.</param>
/// <param name="Versions">Gets the unordered collection of versions discovered for this family.</param>
public sealed record WorkflowFamily(string CanonicalName, IReadOnlyCollection<WorkflowVersionIdentity> Versions);
