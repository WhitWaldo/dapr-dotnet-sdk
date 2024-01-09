using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Dapr.Workflow.Analyzers.Tests.MsTest.Verifiers
{
    public static partial class CSharpAnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public class Test : CSharpAnalyzerTest<TAnalyzer, MSTestVerifier>
        {
            public Test()
            {
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var compilationOptions = solution.GetProject(projectId)?.CompilationOptions;
                    compilationOptions = compilationOptions?.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
                    if (compilationOptions is not null)
                    {
                        solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);
                    }

                    return solution;
                });
            }
        }
    }
}
