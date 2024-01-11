// using System.Collections.Immutable;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CodeActions;
// using Microsoft.CodeAnalysis.CodeFixes;
// using Microsoft.CodeAnalysis.CSharp;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using Microsoft.CodeAnalysis.Editing;
//
// namespace Dapr.Workflow.Analyzers.CodeFixes;
//
// /// <summary>
// /// Implements a Roslyn code fix provider that adds a registration for each type that implements `IWorkflowActivity`
// /// but which isn't also registered within the `AddDaprWorkflow` dependency injection registration. 
// /// </summary>
// [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(
// 	CSharpWorkflowActivityDependencyInjectionAnalyzerCodeFixProvider))]
// public sealed class CSharpWorkflowActivityDependencyInjectionAnalyzerCodeFixProvider : CodeFixProvider
// {
// 	//Specify the diagnostic IDs of analyzers that are expected to be linked
//     /// <inheritdoc/>
// 	public sealed override ImmutableArray<string> FixableDiagnosticIds =>
// 		ImmutableArray.Create(CSharpWorkflowActivityDependencyInjectionValidationAnalyzer.DiagnosticId);
//
//     /// <inheritdoc/>
// 	public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
//
// 	/// <summary>
// 	/// Computes one or more fixes for the specified <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" />.
// 	/// </summary>
// 	/// <param name="context">
// 	/// A <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" /> containing context information about the diagnostics to fix.
// 	/// The context must only contain diagnostics with a <see cref="P:Microsoft.CodeAnalysis.Diagnostic.Id" /> included in the <see cref="P:Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider.FixableDiagnosticIds" /> for the current provider.
// 	/// </param>
// 	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
// 	{
// 		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
//
// 		var diagnostic = context.Diagnostics.First();
// 		var diagnosticSpan = diagnostic.Location.SourceSpan;
//
// 		//Find the type declaration identified by the diagnostic
// 		var declaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
// 			.OfType<TypeDeclarationSyntax>()
// 			.FirstOrDefault();
//
// 		//Register a code action that will invoke the fix
// 		if (declaration is not null)
// 		{
// 			context.RegisterCodeFix(
// 				CodeAction.Create(title: Resources.WF0002CodeFixTitle,
// 					createChangedDocument: c => RegisterWorkflowActivityAsync(context.Document, declaration, c),
// 					equivalenceKey: Resources.WF0002CodeFixTitle), diagnostic);
// 		}
// 	}
// 	
// 	private async Task<Document> RegisterWorkflowActivityAsync(Document document, TypeDeclarationSyntax typeDecl,
// 		CancellationToken cancellationToken)
// 	{
// 		//Get the semantic model and symbol of the type declaration
// 		var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
//
// 		if (semanticModel is null)
// 			return document;
//
// 		if (ModelExtensions.GetDeclaredSymbol(semanticModel, typeDecl, cancellationToken) is not INamedTypeSymbol typeSymbol)
// 			return document;
// 		
// 		//Get the well-known type (IWorkflow)
// 		var workflowType = semanticModel.Compilation.GetTypeByMetadataName("IWorkflowActivity");
// 		
// 		//Check if the type implements IWorkflow
// 		var interfaceType =
// 			typeSymbol.AllInterfaces.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i, workflowType));
// 		if (interfaceType is null)
// 			return document;
// 		
// 		//Get the name of the registration method
// 		const string methodName = "RegisterWorkflowActivity";
//
// 		//Create a document editor
// 		var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
// 		
// 		//Find the lambda expression that configures the workflow options
// 		var lambdaExpr = editor.OriginalRoot.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>()
// 			.FirstOrDefault(l => l.Parameter.Identifier.ValueText == "options");
// 		if (lambdaExpr is null)
// 			return document;
// 		
// 		//Find the block of the lambda expression
// 		if (lambdaExpr.Body is not BlockSyntax block)
// 		{
// 			return document;
// 		}
// 		
// 		//Create a statement that invokes the registration method with the type argument
// 		var statement = SyntaxFactory.ExpressionStatement(
// 			SyntaxFactory.InvocationExpression(
// 				SyntaxFactory.MemberAccessExpression(
// 					SyntaxKind.SimpleMemberAccessExpression,
// 					SyntaxFactory.IdentifierName("options"),
// 					SyntaxFactory.GenericName(
// 						SyntaxFactory.Identifier(methodName),
// 						SyntaxFactory.TypeArgumentList(
// 							SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
// 								SyntaxFactory.IdentifierName(typeSymbol.Name)))))));
// 		
// 		//Add the statement to the end of the block
// 		editor.ReplaceNode(block, block.AddStatements(statement));
// 		
// 		//Return the updated document
// 		return editor.GetChangedDocument();
// 	}
// }
