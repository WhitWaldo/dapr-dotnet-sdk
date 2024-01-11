using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dapr.Workflow.Analyzers.Helpers;

internal static class TypeHelpers
{
	/// <summary>
	/// Helper method to check if a type implements an interface.
	/// </summary>
	internal static bool Implements(INamedTypeSymbol symbol, ITypeSymbol type)
	{
		return symbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, type));
	}

	/// <summary>
	/// Helper method to get all the types in a given namespace.
	/// </summary>
	internal static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
	{
		foreach (var type in ns.GetTypeMembers())
		{
			yield return type;
			foreach (var nestedType in type.GetTypeMembers())
			{
				yield return nestedType;
			}
		}

		foreach (var nestedNamespace in ns.GetNamespaceMembers())
		{
			foreach (var type in GetAllTypes(nestedNamespace))
			{
				yield return type;
			}
		}
	}

    /// <summary>
    /// Identifies the dependency injection registration operation in which AddDaprWorkflow is called
    /// and records each of the registrations of the indicated registration method (e.g. RegisterWorkflow or RegisterActivity).
    /// </summary>
    /// <param name="registrationMethodName">The name of the registration method to look for.</param>
    /// <param name="syntaxNode">The invocation expression syntax to work with.</param>
    /// <param name="model">The semantic model to analyze.</param>
    /// <returns>A hashset of each of the <see cref="INamedTypeSymbol"/> comprising the registered types.</returns>
    internal static IReadOnlyCollection<INamedTypeSymbol> IdentifyWorkflowDependencyInjectionTypeRegistrations(
        string registrationMethodName, InvocationExpressionSyntax syntaxNode, SemanticModel model)
    {
        var registeredTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.IncludeNullability);
        
        //Check if the syntax node is a call to AddDaprWorkflow
        if (syntaxNode.Expression is MemberAccessExpressionSyntax {Name.Identifier.Text: "AddDaprWorkflow"})
        {
            //Get the lambda expression passed as the argument to AddDaprWorkflow
            var lambda = syntaxNode.ArgumentList.Arguments.FirstOrDefault()?.Expression as SimpleLambdaExpressionSyntax;
            
            //If the lambda expression is not null, get its body
            if (lambda?.Body is BlockSyntax lambdaBody)
            {
                //Loop through the statements in the lambda body
                foreach (var statement in lambdaBody.Statements)
                {
                    //Check if the statement is an expression statement
                    if (statement is ExpressionStatementSyntax expressionStatement)
                    {
                        //Check if the expression is a call to RegisterWorkflow
                        if (expressionStatement.Expression is InvocationExpressionSyntax registerWorkflowCall &&
                            registerWorkflowCall.Expression is MemberAccessExpressionSyntax registerWorkflowAccess &&
                            registerWorkflowAccess.Name.Identifier.Text == registrationMethodName)
                        {
                            //Get the generic name syntax of the RegisterWorkflow call
                            var genericName = registerWorkflowAccess.Name as GenericNameSyntax;
                            
                            //If the generic name syntax is not null, get the first type argument
                            var typeArgument = genericName?.TypeArgumentList.Arguments.FirstOrDefault();

                            //If there isn't one, we're not interested in this method
                            if (typeArgument != null)
                            {
                                var typeSymbol = model.GetTypeInfo(typeArgument).Type;
                                
                                //If the type symbol is not null and is a named type symbol, add the list of registered types
                                if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
                                {
                                    registeredTypes.Add(namedTypeSymbol);
                                }
                            }
                        }
                    }
                }
            }   
        }

        return registeredTypes;
    }
}
