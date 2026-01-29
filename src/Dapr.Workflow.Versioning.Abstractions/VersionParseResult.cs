namespace Dapr.Workflow.Versioning.Abstractions;

/// <summary>
/// The result of deriving a canonical name and version from a workflow type.
/// </summary>
/// <param name="CanonicalName">The canonical name for the workflow family.</param>
/// <param name="Version">The derived or explicit version string.</param>
/// <param name="IsExplicit"><see langword="true"/> if provided explicitly (e.g. by <see cref="WorkflowVersionAttribute"/>); otherwise <see langword="false"/>.</param>
public readonly record struct VersionParseResult(string CanonicalName, string Version, bool IsExplicit);
