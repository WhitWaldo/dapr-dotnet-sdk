using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Dapr.Workflow.Analyzers.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dapr.Workflow.Analyzers;

/// <summary>
/// Implements a Roslyn analyzer that validates that all types that implement `IWorkflowActivity` are
/// also registered in the `AddDaprWorkflow` method during dependency injection registration.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DAPR1002WorkflowActivityDependencyInjectionValidationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The unique diagnostic identifier for this analyzer.
    /// </summary>
	public const string DiagnosticId = "DAPR1002";

    private const string HelpLink =
        "https://github.com/dapr/dotnet-sdk/Dapr.Workflow.Analyzers/blob/master/Documentation/DAPR1002.md";

    private static readonly DiagnosticDescriptor CSharpWorkflowActivityDependencyInjectionValidationRule =
		new(DiagnosticId, "Missing workflow activity dependency injection registration", "The workflow activity type '{0}' is not registered with the service provider", "DependencyInjection",
			DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Every type that implements IWorkflowActivity should be registered with the service provider.", helpLinkUri: HelpLink);

    /// <inheritdoc/>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create(CSharpWorkflowActivityDependencyInjectionValidationRule);

    /// <inheritdoc/>
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterCompilationStartAction(AnalyzeCompilation);
	}

	private static void AnalyzeCompilation(CompilationStartAnalysisContext context)
	{
		//Get the compilation and the well-known types
		var compilation = context.Compilation;
		var workflowType = compilation.GetTypeByMetadataName("IWorkflowActivity");
		
		//Check if the type exists
		if (workflowType == null)
			return;
		
		//Create a hash set to store the registered types
		var registeredTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
		
		//Register an action to analyze the syntax nodes of the compilation
        context.RegisterSemanticModelAction(c => AnalyzeSemanticModel(c, registeredTypes, workflowType));
	}

    private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context,
        ISet<INamedTypeSymbol> registeredTypes, INamedTypeSymbol workflowType)
    {
        //Get the semantic model and the syntax tree
        var model = context.SemanticModel;
        var tree = model.SyntaxTree;

        //Get the root node of the syntax tree
        var root = tree.GetRoot();

        //Find all the invocation expressions in the syntax tree
        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

        //Analyze each invocation expression to find the registered types
        foreach (var invocation in invocations)
        {
            registeredTypes.UnionWith(
                TypeHelpers.IdentifyWorkflowDependencyInjectionTypeRegistrations("RegisterActivity", invocation,
                    model));
        }
        
        //Get all the types in the compilation
        var allTypes = TypeHelpers.GetAllTypes(model.Compilation.Assembly.GlobalNamespace);

        //Iterate through the types and compare to those identified in the dependency injection registration
        //to report on those not found
        foreach (var type in allTypes)
        {
            //Skip abstract classes
            if (type.IsAbstract) continue;

            //Check if the type implements the workflow type (e.g. IWorkflow or IWorkflowActivity)
            if (!TypeHelpers.Implements(type, workflowType)) continue;

            //Check if the type is registered
            if (registeredTypes.Contains(type)) continue;

            //Report a diagnostic
            var diagnostic = Diagnostic.Create(CSharpWorkflowActivityDependencyInjectionValidationRule,
                type.Locations[0], type.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
