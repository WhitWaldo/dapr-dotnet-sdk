// ------------------------------------------------------------------------
//  Copyright 2026 The Dapr Authors
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  ------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dapr.Workflow.Versioning;

/// <summary>
/// Incremental source generator that discovers Dapr Workflow types, reads optional versioning metadata
/// and emits a registry and registration method that:
/// - registers all workflow implementations, and
/// - registers a canonical-name alias that dispatches to the latest implementation as determined by the runtime
/// (strategy and selector).
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class WorkflowSourceGenerator : IIncrementalGenerator
{
    private const string WorkflowBaseMetadataName = "Dapr.Workflow.Workflow`2";
    private const string WorkflowVersionAttributeFullName = "Dapr.Workflow.Versioning.WorkflowVersionAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Cache the attribute symbol
        var known = context.CompilationProvider.Select((c, _) =>
            new KnownSymbols(
                WorkflowBase: c.GetTypeByMetadataName(WorkflowBaseMetadataName),
                WorkflowVersionAttribute: c.GetTypeByMetadataName(WorkflowVersionAttributeFullName)));

        // Discover candidate class symbols
        var candidates = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
            static (ctx, _) =>
                (INamedTypeSymbol?)ctx.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax)ctx.Node));

        //Combine the attribute symbol with each candidate symbol
        var inputs = candidates.Combine(known);

        // Filter and transform with proper symbol equality checks
        var discovered = inputs
            .Select((pair, _) =>
            {
                var (symbol, ks) = pair;
                if (symbol is null)
                    return null;

                // Check derives from Dapr.Workflow.Workflow<,>
                if (!InheritsFromWorkflow(symbol, ks.WorkflowBase))
                    return null;

                // Look for [WorkflowVersion] by symbol identity
                AttributeData? attrData = null;
                if (ks.WorkflowVersionAttribute is not null)
                {
                    attrData = symbol.GetAttributes()
                        .FirstOrDefault(a =>
                            SymbolEqualityComparer.Default.Equals(a.AttributeClass, ks.WorkflowVersionAttribute));
                }

                attrData ??= symbol.GetAttributes().FirstOrDefault(a =>
                    string.Equals(a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        $"global::{WorkflowVersionAttributeFullName}", StringComparison.Ordinal));

                return BuildDiscoveredWorkflow(symbol, attrData);
            })
            .Where(x => x is not null);

        // Collect and emit
        context.RegisterSourceOutput(discovered.Collect(), EmitRegistry);
    }

    private static bool InheritsFromWorkflow(INamedTypeSymbol symbol, INamedTypeSymbol? workflowBase)
    {
        if (workflowBase is null) return false; // Consumer didn’t reference Dapr.Workflow (no Workflows present)

        for (var t = symbol.BaseType; t is not null; t = t.BaseType)
        {
            var od = t.OriginalDefinition;
            if (od is INamedTypeSymbol &&
                SymbolEqualityComparer.Default.Equals(od, workflowBase))
                return true;
        }

        return false;
    }


    private static DiscoveredWorkflow? BuildDiscoveredWorkflow(
        INamedTypeSymbol workflowSymbol,
        AttributeData? workflowVersionAttribute)
    {
        // Extract TInput,TOutput from the base type Workflow<TInput,TOutput>.
        var (input, output) = GetWorkflowTypeArgs(workflowSymbol);
        if (input is null || output is null)
            return null;

        string? canonical = null;
        string? version = null;
        string? optionsName = null;
        string? strategyTypeName = null;

        if (workflowVersionAttribute is not null)
        {
            foreach (var kvp in workflowVersionAttribute.NamedArguments)
            {
                var name = kvp.Key;
                var tc = kvp.Value;
                
                switch (name)
                {
                    case "CanonicalName":
                        canonical = tc.Value?.ToString();
                        break;
                    case "Version":
                        version = tc.Value?.ToString();
                        break;
                    case "OptionsName":
                        optionsName = tc.Value?.ToString();
                        break;
                    case "StrategyType":
                        if (tc.Value is INamedTypeSymbol typeSym)
                        {
                            strategyTypeName = typeSym.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        }

                        break;
                }
            }
        }

        // Build the discovered descriptor using fully-qualified type names for stability.
        return new DiscoveredWorkflow(
            WorkflowTypeName: workflowSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            InputTypeName: input.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            OutputTypeName: output.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            DeclaredCanonicalName: string.IsNullOrWhiteSpace(canonical) ? null : canonical,
            DeclaredVersion: string.IsNullOrWhiteSpace(version) ? null : version,
            StrategyTypeName: strategyTypeName,
            OptionsName: string.IsNullOrWhiteSpace(optionsName) ? null : optionsName
        );
    }


    private static (ITypeSymbol? In, ITypeSymbol? Out) GetWorkflowTypeArgs(INamedTypeSymbol symbol)
    {
        for (var t = symbol.BaseType; t is not null; t = t.BaseType)
        {
            if (t.OriginalDefinition.ToDisplayString() != WorkflowBaseMetadataName)
                continue;

            if (t is { TypeArguments.Length: 2 })
                return (t.TypeArguments[0], t.TypeArguments[1]);
        }

        return (null, null);
    }


    /// <summary>
    /// Final source-emission step for the generator. Receives the collected set of discovered
    /// workflow descriptors and adds the generated registry/registration source to the compilation.
    /// </summary>
    private static void EmitRegistry(
        SourceProductionContext context,
        ImmutableArray<DiscoveredWorkflow?> discoveredItems)
    {
        // Nothing to emit if we found no workflows.
        if (discoveredItems.IsDefaultOrEmpty)
            return;

        // Remove nulls, de-dupe by fully-qualified type name, and stabilize the order for deterministic output.
        var list = discoveredItems
            .Where(x => x is not null)
            .Select(x => x!)
            .Distinct(new DiscoveredWorkflowComparer())
            .OrderBy(x => x.WorkflowTypeName, StringComparer.Ordinal)
            .ToList();

        if (list.Count == 0)
            return;

        // Generate the full source and add it to the compilation.
        var source = GenerateRegistrySource(list);
        context.AddSource("Dapr_Workflow_Versioning.g.cs", source);
    }

    private static string GenerateRegistrySource(IReadOnlyList<DiscoveredWorkflow> discovered)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Dapr.Workflow;");
        sb.AppendLine("using Dapr.Workflow.Versioning;");

        sb.AppendLine();
        sb.AppendLine("namespace Dapr.Workflow.Versioning");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Generated workflow registry that registers discovered workflows and");
        sb.AppendLine("    /// adds canonical-name aliases that dispatch to the latest implementation at runtime.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    internal static partial class GeneratedWorkflowVersionRegistry");
        sb.AppendLine("    {");

        // Emit per-workflow strongly-typed runner methods (used by functional registration).
        foreach (var wf in discovered)
        {
            var runnerName = "__Run_" + SanitizeForId(wf.WorkflowTypeName);
            sb.AppendLine(
                $"        private static Task<{wf.OutputTypeName}> {runnerName}(global::Dapr.Workflow.WorkflowContext ctx, {wf.InputTypeName} input)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var wf = new {wf.WorkflowTypeName}();");
            sb.AppendLine("            return wf.RunAsync(ctx, input);");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        // Emit per-workflow strongly-typed alias registration methods (avoid reflection for generics).
        foreach (var wf in discovered)
        {
            var runnerName = "__Run_" + SanitizeForId(wf.WorkflowTypeName);
            var registrarName = "__RegisterAlias_" + SanitizeForId(wf.WorkflowTypeName);
            sb.AppendLine(
                $"        private static void {registrarName}(global::Dapr.Workflow.WorkflowRuntimeOptions options, string canonicalName)");
            sb.AppendLine("        {");
            sb.AppendLine(
                $"            options.RegisterWorkflow<{wf.InputTypeName}, {wf.OutputTypeName}>(canonicalName, {runnerName});");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        // Emit runtime registration entry struct to carry discovery + declared hints.
        sb.AppendLine("        private readonly struct Entry");
        sb.AppendLine("        {");
        sb.AppendLine("            public readonly Type WorkflowType;");
        sb.AppendLine("            public readonly string WorkflowTypeName;");
        sb.AppendLine("            public readonly string? DeclaredCanonicalName;");
        sb.AppendLine("            public readonly string? DeclaredVersion;");
        sb.AppendLine("            public readonly Type? StrategyType;");
        sb.AppendLine("            public readonly string? OptionsName;");
        sb.AppendLine("            public readonly Type InputType;");
        sb.AppendLine("            public readonly Type OutputType;");
        sb.AppendLine();
        sb.AppendLine(
            "            public Entry(Type wfType, string wfName, string? canonical, string? version, Type? strategyType, string? optionsName, Type inType, Type outType)");
        sb.AppendLine("            {");
        sb.AppendLine("                WorkflowType = wfType;");
        sb.AppendLine("                WorkflowTypeName = wfName;");
        sb.AppendLine("                DeclaredCanonicalName = canonical;");
        sb.AppendLine("                DeclaredVersion = version;");
        sb.AppendLine("                StrategyType = strategyType;");
        sb.AppendLine("                OptionsName = optionsName;");
        sb.AppendLine("                InputType = inType;");
        sb.AppendLine("                OutputType = outType;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();

        // Emit public API: RegisterGeneratedWorkflows
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Registers all discovered workflow types with the Dapr Workflow runtime and");
        sb.AppendLine("        /// adds canonical-name aliases that dispatch to the latest implementation.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine(
            "        /// <param name=\"options\">The workflow runtime options used for registration.</param>");
        sb.AppendLine(
            "        /// <param name=\"services\">Application service provider (DI root) used to resolve strategy/selector runtime services.</param>");
        sb.AppendLine(
            "        public static void RegisterGeneratedWorkflows(global::Dapr.Workflow.WorkflowRuntimeOptions options, global::System.IServiceProvider services)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (options is null) throw new ArgumentNullException(nameof(options));");
        sb.AppendLine("            if (services is null) throw new ArgumentNullException(nameof(services));");
        sb.AppendLine();

        // Build the entries array
        sb.AppendLine(
            "            // Discovered workflows (compile-time). Any declared canonical/version are hints; otherwise runtime strategy derives them.");
        sb.AppendLine("            var entries = new List<Entry>");
        sb.AppendLine("            {");
        foreach (var wf in discovered)
        {
            var strategyTypeLit = string.IsNullOrWhiteSpace(wf.StrategyTypeName)
                ? "null"
                : $"typeof({wf.StrategyTypeName})";
            var canonicalLit = wf.DeclaredCanonicalName is null ? "null" : CodeLiteral(wf.DeclaredCanonicalName);
            var versionLit = wf.DeclaredVersion is null ? "null" : CodeLiteral(wf.DeclaredVersion);
            var optionsLit = wf.OptionsName is null ? "null" : CodeLiteral(wf.OptionsName);

            sb.AppendLine(
                $"                new Entry(typeof({wf.WorkflowTypeName}), {CodeLiteral(wf.WorkflowTypeName)}, {canonicalLit}, {versionLit}, {strategyTypeLit}, {optionsLit}, typeof({wf.InputTypeName}), typeof({wf.OutputTypeName})),");
        }

        sb.AppendLine("            };");
        sb.AppendLine();

        // 1) Register every workflow type as-is.
        sb.AppendLine("            // Register concrete workflow implementations.");
        foreach (var wf in discovered)
        {
            sb.AppendLine($"            options.RegisterWorkflow<{wf.WorkflowTypeName}>();");
        }

        sb.AppendLine();

        // 2) Derive (canonical, version) using per-entry strategy override or default strategy; build families.
        sb.AppendLine("            // Resolve runtime services.");
        sb.AppendLine(
            "            var resolver = services.GetService(typeof(global::Dapr.Workflow.Versioning.IWorkflowVersionResolver)) as global::Dapr.Workflow.Versioning.IWorkflowVersionResolver");
        sb.AppendLine(
            "                ?? throw new InvalidOperationException(\"IWorkflowVersionResolver is not registered. Call AddDaprWorkflowVersioning() at startup.\");");
        sb.AppendLine(
            "            var opts = services.GetService(typeof(global::Dapr.Workflow.Versioning.WorkflowVersioningOptions)) as global::Dapr.Workflow.Versioning.WorkflowVersioningOptions");
        sb.AppendLine(
            "                ?? throw new InvalidOperationException(\"WorkflowVersioningOptions is not registered.\");");
        sb.AppendLine(
            "            var strategyFactory = services.GetService(typeof(global::Dapr.Workflow.Versioning.IWorkflowVersionStrategyFactory)) as global::Dapr.Workflow.Versioning.IWorkflowVersionStrategyFactory;");
        sb.AppendLine("            var defaultStrategy = opts.DefaultStrategy?.Invoke(services)");
        sb.AppendLine(
            "                ?? throw new InvalidOperationException(\"No default workflow versioning strategy configured.\");");
        sb.AppendLine();

        sb.AppendLine("            // Build families: canonical name -> versions");
        sb.AppendLine(
            "            var families = new Dictionary<string, List<(string Version, Entry Entry)>>(StringComparer.Ordinal);");
        sb.AppendLine("            foreach (var e in entries)");
        sb.AppendLine("            {");
        sb.AppendLine("                string canonical = e.DeclaredCanonicalName ?? string.Empty;");
        sb.AppendLine("                string version = e.DeclaredVersion ?? string.Empty;");
        sb.AppendLine("                var strategy = defaultStrategy;");
        sb.AppendLine("                if (e.StrategyType is not null && strategyFactory is not null)");
        sb.AppendLine("                {");
        sb.AppendLine(
            "                    strategy = strategyFactory.Create(e.StrategyType, e.DeclaredCanonicalName ?? e.WorkflowType.Name, e.OptionsName, services);");
        sb.AppendLine("                }");
        sb.AppendLine("                if (string.IsNullOrEmpty(canonical) || string.IsNullOrEmpty(version))");
        sb.AppendLine("                {");
        sb.AppendLine("                    if (!strategy.TryParse(e.WorkflowType.Name, out var c, out var v))");
        sb.AppendLine("                    {");
        sb.AppendLine(
            "                        var diag = services.GetService(typeof(global::Dapr.Workflow.Versioning.IWorkflowVersioningDiagnostics)) as global::Dapr.Workflow.Versioning.IWorkflowVersioningDiagnostics;");
        sb.AppendLine(
            "                        var msg = diag?.CouldNotParseMessage(e.WorkflowTypeName) ?? $\"Could not parse workflow type '{e.WorkflowTypeName}'.\";");
        sb.AppendLine("                        throw new InvalidOperationException(msg);");
        sb.AppendLine("                    }");
        sb.AppendLine("                    canonical = string.IsNullOrEmpty(canonical) ? c : canonical;");
        sb.AppendLine("                    version = string.IsNullOrEmpty(version) ? v : version;");
        sb.AppendLine("                }");
        sb.AppendLine("                if (!families.TryGetValue(canonical, out var list))");
        sb.AppendLine("                {");
        sb.AppendLine("                    list = new List<(string, Entry)>();");
        sb.AppendLine("                    families[canonical] = list;");
        sb.AppendLine("                }");
        sb.AppendLine("                list.Add((version, e));");
        sb.AppendLine("            }");
        sb.AppendLine();

        // 3) For each family, ask resolver for latest; register alias to latest using the generated strongly-typed registrar.
        sb.AppendLine("            foreach (var kvp in families)");
        sb.AppendLine("            {");
        sb.AppendLine("                var canonical = kvp.Key;");
        sb.AppendLine("                var list = kvp.Value;");
        sb.AppendLine();
        sb.AppendLine(
            "                var versions = list.Select(v => new global::Dapr.Workflow.Versioning.WorkflowVersionIdentity(canonical, v.Version, v.Entry.WorkflowTypeName, v.Entry.WorkflowType.Assembly.GetName().Name)).ToList();");
        sb.AppendLine(
            "                var family = new global::Dapr.Workflow.Versioning.WorkflowFamily { CanonicalName = canonical, Versions = versions };");
        sb.AppendLine(
            "                if (!resolver.TryGetLatest(family, out var latest, out var diagId, out var diagMsg))");
        sb.AppendLine("                {");
        sb.AppendLine(
            "                    // If selection fails, surface a clear error (generator output remains deterministic).");
        sb.AppendLine(
            "                    throw new InvalidOperationException(diagMsg ?? $\"Failed to select latest version for '{canonical}'.\");");
        sb.AppendLine("                }");
        sb.AppendLine();
        sb.AppendLine("                // Find the matching Entry and call its strongly-typed alias registrar.");
        sb.AppendLine(
            "                var chosen = list.FirstOrDefault(v => string.Equals(v.Entry.WorkflowTypeName, latest.TypeName, StringComparison.Ordinal)).Entry;");
        sb.AppendLine(
            "                if (chosen.WorkflowType is null) throw new InvalidOperationException($\"Selected latest type '{latest.TypeName}' not found in entries.\");");
        sb.AppendLine();

        // Emit a chain of if (...) __RegisterAlias_<T>()
        sb.AppendLine("                // Dispatch to the strongly-typed alias registration method.");
        bool first = true;
        foreach (var wf in discovered)
        {
            var registrarName = "__RegisterAlias_" + SanitizeForId(wf.WorkflowTypeName);
            var cond = $"chosen.WorkflowType == typeof({wf.WorkflowTypeName})";
            sb.AppendLine(first
                ? $"                if ({cond}) {registrarName}(options, canonical);"
                : $"                else if ({cond}) {registrarName}(options, canonical);");
            first = false;
        }

        sb.AppendLine("                else");
        sb.AppendLine("                {");
        sb.AppendLine(
            "                    throw new InvalidOperationException($\"No registration method generated for selected type '{latest.TypeName}'.\");");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }"); // end RegisterGeneratedWorkflows

        sb.AppendLine("    }"); // class
        sb.AppendLine("}"); // namespace

        return sb.ToString();
    }

    private static string SanitizeForId(string fullyQualified)
    {
        // Turn "global::Namespace.Type+Nested`1" into a safe identifier-ish string.
        var s = fullyQualified
            .Replace("global::", string.Empty)
            .Replace("::", "_")
            .Replace(".", "_")
            .Replace("+", "_")
            .Replace("`", "_")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace(",", "_")
            .Replace(" ", string.Empty);
        // Remove leading underscores if any
        while (s.StartsWith("_", StringComparison.Ordinal)) s = s.Substring(1);
        return s;
    }

    private static string CodeLiteral(string s) => "@\"" + s.Replace("\"", "\"\"") + "\"";

    private sealed record DiscoveredWorkflow(
        string WorkflowTypeName,
        string InputTypeName,
        string OutputTypeName,
        string? DeclaredCanonicalName,
        string? DeclaredVersion,
        string? StrategyTypeName,
        string? OptionsName
    );

    private sealed class DiscoveredWorkflowComparer : IEqualityComparer<DiscoveredWorkflow>
    {
        public bool Equals(DiscoveredWorkflow? x, DiscoveredWorkflow? y)
            => StringComparer.Ordinal.Equals(x?.WorkflowTypeName, y?.WorkflowTypeName);

        public int GetHashCode(DiscoveredWorkflow obj)
            => StringComparer.Ordinal.GetHashCode(obj.WorkflowTypeName);
    }
}
