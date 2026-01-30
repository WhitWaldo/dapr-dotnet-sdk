// ------------------------------------------------------------------------
// Copyright 2026 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Workflow.Versioning.Runtime.Test;

public class WorkflowVersionResolverTests
{
    [Fact]
    public void Constructor_ThrowsWhenArgumentsNull()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var options = new WorkflowVersioningOptions();
        var diagnostics = new TestDiagnostics();

        Assert.Throws<ArgumentNullException>(() => new WorkflowVersionResolver(null!, options, diagnostics));
        Assert.Throws<ArgumentNullException>(() => new WorkflowVersionResolver(provider, null!, diagnostics));
        Assert.Throws<ArgumentNullException>(() => new WorkflowVersionResolver(provider, options, null!));
    }

    [Fact]
    public void TryGetLatest_ReturnsFalseForEmptyFamily()
    {
        var resolver = CreateResolver(_ => new NumericStrategy(), _ => new MaxVersionSelector());
        var family = new WorkflowFamily("Orders", Array.Empty<WorkflowVersionIdentity>());

        var success = resolver.TryGetLatest(family, out var latest, out var diagnosticId, out var message);

        Assert.False(success);
        Assert.Equal(default, latest);
        Assert.Equal(IWorkflowVersioningDiagnosticIds.EmptyFamily, diagnosticId);
        Assert.Equal("empty:Orders", message);
    }

    [Fact]
    public void TryGetLatest_ReturnsFalseForNullVersions()
    {
        var resolver = CreateResolver(_ => new NumericStrategy(), _ => new MaxVersionSelector());
        var family = new WorkflowFamily("Orders", null!);

        var success = resolver.TryGetLatest(family, out var latest, out var diagnosticId, out var message);

        Assert.False(success);
        Assert.Equal(default, latest);
        Assert.Equal(IWorkflowVersioningDiagnosticIds.EmptyFamily, diagnosticId);
        Assert.Equal("empty:Orders", message);
    }

    [Fact]
    public void TryGetLatest_ThrowsWhenNoDefaultStrategyConfigured()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var resolver = new WorkflowVersionResolver(provider, new WorkflowVersioningOptions(), new TestDiagnostics());
        var family = new WorkflowFamily("Orders", new[]
        {
            new WorkflowVersionIdentity("Orders", "1", "OrdersWorkflowV1"),
        });

        Assert.Throws<InvalidOperationException>(() =>
            resolver.TryGetLatest(family, out _, out _, out _));
    }

    [Fact]
    public void TryGetLatest_UsesMaxVersionSelectorWhenDefaultSelectorMissing()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var options = new WorkflowVersioningOptions
        {
            DefaultStrategy = _ => new NumericStrategy(),
            DefaultSelector = null,
        };
        var resolver = new WorkflowVersionResolver(provider, options, new TestDiagnostics());
        var family = new WorkflowFamily("Orders", new[]
        {
            new WorkflowVersionIdentity("Orders", "1", "OrdersWorkflowV1"),
            new WorkflowVersionIdentity("Orders", "2", "OrdersWorkflowV2"),
        });

        var success = resolver.TryGetLatest(family, out var latest, out var diagnosticId, out var message);

        Assert.True(success);
        Assert.Equal("2", latest.Version);
        Assert.Null(diagnosticId);
        Assert.Null(message);
    }

    [Fact]
    public void TryGetLatest_ReturnsEmptyFamilyDiagnosticWhenSelectorThrowsArgumentException()
    {
        var resolver = CreateResolver(_ => new NumericStrategy(), _ => new ThrowingSelector(new ArgumentException()));
        var family = new WorkflowFamily("Orders", new[]
        {
            new WorkflowVersionIdentity("Orders", "1", "OrdersWorkflowV1"),
        });

        var success = resolver.TryGetLatest(family, out var latest, out var diagnosticId, out var message);

        Assert.False(success);
        Assert.Equal(default, latest);
        Assert.Equal(IWorkflowVersioningDiagnosticIds.EmptyFamily, diagnosticId);
        Assert.Equal("empty:Orders", message);
    }

    [Fact]
    public void TryGetLatest_ReturnsAmbiguousLatestDiagnosticWhenSelectorThrowsInvalidOperationException()
    {
        var diagnostics = new TestDiagnostics();
        var resolver = CreateResolver(_ => new NumericStrategy(), _ => new ThrowingSelector(new InvalidOperationException()),
            diagnostics);
        var family = new WorkflowFamily("Orders", new[]
        {
            new WorkflowVersionIdentity("Orders", "1", "OrdersWorkflowV1"),
            new WorkflowVersionIdentity("Orders", "2", "OrdersWorkflowV2"),
            new WorkflowVersionIdentity("Orders", "2", "OrdersWorkflowV2b"),
        });

        var success = resolver.TryGetLatest(family, out var latest, out var diagnosticId, out var message);

        Assert.False(success);
        Assert.Equal(default, latest);
        Assert.Equal(IWorkflowVersioningDiagnosticIds.AmbiguousLatest, diagnosticId);
        Assert.Equal("ambiguous:Orders:2,2", message);
        Assert.Equal(new[] { "2", "2" }, diagnostics.LastAmbiguousVersions);
    }

    private static WorkflowVersionResolver CreateResolver(
        Func<IServiceProvider, IWorkflowVersionStrategy> strategyFactory,
        Func<IServiceProvider, IWorkflowVersionSelector> selectorFactory,
        TestDiagnostics? diagnostics = null)
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var options = new WorkflowVersioningOptions
        {
            DefaultStrategy = strategyFactory,
            DefaultSelector = selectorFactory,
        };
        diagnostics ??= new TestDiagnostics();
        return new WorkflowVersionResolver(provider, options, diagnostics);
    }

    private sealed class NumericStrategy : IWorkflowVersionStrategy
    {
        public bool TryParse(string typeName, out string canonicalName, out string version)
        {
            canonicalName = string.Empty;
            version = string.Empty;
            return false;
        }

        public int Compare(string? v1, string? v2)
            => int.Parse(v1 ?? "0", CultureInfo.InvariantCulture)
                .CompareTo(int.Parse(v2 ?? "0", CultureInfo.InvariantCulture));
    }

    private sealed class ThrowingSelector(Exception exception) : IWorkflowVersionSelector
    {
        private readonly Exception _exception = exception;

        public WorkflowVersionIdentity SelectLatest(string canonicalName,
            IReadOnlyCollection<WorkflowVersionIdentity> candidates,
            IWorkflowVersionStrategy strategy)
            => throw _exception;
    }

    private sealed class TestDiagnostics : IWorkflowVersionDiagnostics
    {
        public string?[]? LastAmbiguousVersions { get; private set; }

        public string UnknownStrategyTitle => "unknown-strategy";
        public string CouldNotParseTitle => "could-not-parse";
        public string EmptyFamilyTitle => "empty-family";
        public string AmbiguousLatestTitle => "ambiguous-latest";
        public string AmbiguousLatestMessage(string canonicalName, IReadOnlyList<string>? versions)
        {
            LastAmbiguousVersions = versions?.Select(v => (string?)v).ToArray();
            var list = versions is null ? string.Empty : string.Join(",", versions);
            return $"ambiguous:{canonicalName}:{list}";
        }

        public string UnknownStrategyMessage(string typeName, Type strategyType)
            => $"unknown:{typeName}:{strategyType?.Name}";

        public string CouldNotParseMessage(string typeName)
            => $"parse:{typeName}";

        public string EmptyFamilyMessage(string canonicalName)
            => $"empty:{canonicalName}";
    }
}
