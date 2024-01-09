using Microsoft.CodeAnalysis;

namespace Dapr.Workflow.Analyzers.Helpers;

/// <summary>
/// Extensions providing helper methods for SyntaxNode instances.
/// </summary>
public static class SyntaxNodeExtensions
{
	/// <summary>
	/// Checks if a syntax node is the last one in the file to avoid analyzing the same type multiple times.
	/// </summary>
	public static bool IsLastTokenInFile(this SyntaxNode node)
	{
		//Get the root node of the syntax tree
		var root = node.SyntaxTree.GetRoot();

		//Get the last token in the root node
		var lastToken = root.GetLastToken();

		//Check if the node contains the last token
		return node.Span.Contains(lastToken.Span);
	}
}
