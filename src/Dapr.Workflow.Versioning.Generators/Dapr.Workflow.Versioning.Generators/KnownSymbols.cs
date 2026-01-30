using Microsoft.CodeAnalysis;

namespace Dapr.Workflow.Versioning;

internal sealed record KnownSymbols(INamedTypeSymbol? WorkflowBase, INamedTypeSymbol? WorkflowVersionAttribute);
