//using System.Collections.Immutable;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Diagnostics;

//namespace Dapr.Workflow.Analyzers.Analyzers;

///// <summary>
///// Implements a Roslyn analyzer that validates that no types that implement IWorkflow call non-deterministic
///// APIs except through types invoking IWorkflowActivity.
///// </summary>
//[DiagnosticAnalyzer(LanguageNames.CSharp)]
//public sealed class DAPR0500WorkflowInvocationsMustBeDeterministic : DiagnosticAnalyzer
//{
//    /// <summary>
//    /// The ID for diagnostics used by the <see cref="DAPR0500WorkflowInvocationsMustBeDeterministic"/> analyzer.
//    /// </summary>
//    public const string DiagnosticId = "DAPR0500";

//    private const string HelpUri = "";

//    private static readonly DiagnosticDescriptor Descriptor =
//        new (DiagnosticId, "", "", "Usage", DiagnosticSeverity.Warning, true, "", HelpUri);

//    /// <inheritdoc />
//    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptor);

//    /// <inheritdoc />
//    public override void Initialize(AnalysisContext context)
//    {
//        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
//        context.EnableConcurrentExecution();

//        // https://github.com/dotnet/roslyn/blob/main/docs/compilers/Deterministic%20Inputs.md
//        // https://github.com/Azure/azure-functions-durable-extension/tree/dev/src/WebJobs.Extensions.DurableTask.Analyzers
//    }
//}

