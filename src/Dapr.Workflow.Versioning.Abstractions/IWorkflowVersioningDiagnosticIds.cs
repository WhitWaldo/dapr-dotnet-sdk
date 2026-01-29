namespace Dapr.Workflow.Versioning.Abstractions;

/// <summary>
/// Standard diagnostic IDs provides by the workflow versioning generator and runtime.
/// </summary>
/// <remarks>
/// These IDs are intentionally stable and can be used for filtering or documentation.
/// </remarks>
public static class IWorkflowVersioningDiagnosticIds
{
    /// <summary>
    /// Diagnostic with a <see cref="WorkflowVersionAttribute.StrategyType"/> cannot be instantiated or does not
    /// implement <see cref="IWorkflowVersionStrategy"/>.
    /// </summary>
    public const string UnknownStrategy = "DWV001";

    /// <summary>
    /// Diagnostic with no strategy can parse the workflow type name into a canonical name and version.
    /// </summary>
    public const string CouldNotParse = "DWV002";

    /// <summary>
    /// Diagnostic when a canonical family has no versions (e.g., all were filtered out).
    /// </summary>
    public const string EmptyFamily = "DWV003";

    /// <summary>
    /// Diagnostic when selection policy cannot determine a unique latest version (ambiguous winners).
    /// </summary>
    public const string AmbiguousLatest = "DWV004";
}
