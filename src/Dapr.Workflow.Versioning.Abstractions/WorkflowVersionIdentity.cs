namespace Dapr.Workflow.Versioning.Abstractions;

/// <summary>
/// Identifies a single workflow version within a canonical family.
/// </summary>
/// <param name="CanonicalName">The canonical family name (e.g. <c>"OrderProcessingWorkflow"</c>).</param>
/// <param name="Version">The version string (strategy-defined; e.g., "<c>"3"</c>, <c>"1.5.3"</c>, or a date.</param>
/// <param name="TypeName">The CLR type name that implements this workflow version.</param>
/// <param name="AssemblyName">Optional assembly name that contains the workflow type.</param>
public readonly record struct WorkflowVersionIdentity(string CanonicalName, string Version, string TypeName, string? AssemblyName = null)
{
    /// <inheritdoc />
    public override string ToString() => $"{CanonicalName}@{Version} ({TypeName})";
}
