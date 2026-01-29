namespace Dapr.Workflow.Versioning;

/// <summary>
/// Provides shared, localized-friendly diagnostic titles and messages used by both the source generator and the
/// runtime.
/// </summary>
/// <remarks>
/// Implements can integrate with localization frameworks. The message content should be deterministic and
/// safe to surface to developers.
/// </remarks>
public interface IWorkflowVersionDiagnostics
{
    /// <summary>
    /// Gets the title used when a strategy type cannot be constructed or is invalid.
    /// </summary>
    string UnknownStrategyTitle { get; }

    /// <summary>
    /// Formats the message shown when a strategy type could not be created or does not implement
    /// <see cref="IWorkflowVersionStrategy"/>.
    /// </summary>
    /// <param name="typeName">The CLR type name of the workflow.</param>
    /// <param name="strategyType">The provided strategy type.</param>
    /// <returns>A human-readable message string.</returns>
    string UnknownStrategyMessage(string typeName, Type strategyType);
    
    /// <summary>
    /// Gets the title used when the version information cannot be parsed from a type name.
    /// </summary>
    string CouldNotParseTitle { get; }

    /// <summary>
    /// Formats the message shown when no available strategy can derive a canonical name and version.
    /// </summary>
    /// <param name="typeName">The CLR type name of the workflow.</param>
    /// <returns>A human-readable message string.</returns>
    string CouldNotParseMessage(string typeName);
    
    /// <summary>
    /// Gets the title used when a canonical family contains no versions.
    /// </summary>
    string EmptyFamilyTitle { get; }

    /// <summary>
    /// Formats the message shown when a canonical family has no versions.
    /// </summary>
    /// <param name="canonicalName">The canonical name of the workflow family.</param>
    /// <returns>A human-readable message string.</returns>
    string EmptyFamilyMessage(string canonicalName);

    /// <summary>
    /// Gets the title used when latest version selection is ambiguous.
    /// </summary>
    string AmbiguousLatestTitle { get; }

    /// <summary>
    /// Formats the message shown when the version selector cannot determine a unique latest version.
    /// </summary>
    /// <param name="canonicalName">The canonical name of the workflow family.</param>
    /// <param name="versions">The set of tied version strings.</param>
    /// <returns>A human-readable message string.</returns>
    string AmbiguousLatestMessage(string canonicalName, IEnumerable<string> versions);
}
