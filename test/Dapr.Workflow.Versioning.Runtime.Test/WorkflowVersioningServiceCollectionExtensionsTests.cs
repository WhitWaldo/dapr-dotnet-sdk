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

using Microsoft.Extensions.DependencyInjection;

namespace Dapr.Workflow.Versioning.Runtime.Test;

public class WorkflowVersioningServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDaprWorkflowVersioning_ThrowsWhenServicesNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            WorkflowVersioningServiceCollectionExtensions.AddDaprWorkflowVersioning(null!));
    }

    [Fact]
    public void AddDaprWorkflowVersioning_RegistersDefaultServices()
    {
        var services = new ServiceCollection();

        services.AddDaprWorkflowVersioning();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<WorkflowVersioningOptions>());
        Assert.IsType<DefaultWorkflowVersioningDiagnostics>(
            provider.GetRequiredService<IWorkflowVersionDiagnostics>());
        Assert.IsType<DefaultWorkflowVersionStrategyFactory>(
            provider.GetRequiredService<IWorkflowVersionStrategyFactory>());
        Assert.IsType<DefaultWorkflowVersionSelectorFactory>(
            provider.GetRequiredService<IWorkflowVersionSelectorFactory>());
        Assert.IsType<WorkflowVersionResolver>(
            provider.GetRequiredService<IWorkflowVersionResolver>());
    }

    [Fact]
    public void AddDaprWorkflowVersioning_AppliesConfigurationDelegate()
    {
        var services = new ServiceCollection();

        services.AddDaprWorkflowVersioning(options =>
        {
            options.DefaultStrategy = _ => new StubStrategy();
            options.DefaultSelector = _ => new StubSelector();
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<WorkflowVersioningOptions>();

        Assert.NotNull(options.DefaultStrategy);
        Assert.NotNull(options.DefaultSelector);
    }

    private sealed class StubStrategy : IWorkflowVersionStrategy
    {
        public bool TryParse(string typeName, out string canonicalName, out string version)
        {
            canonicalName = string.Empty;
            version = string.Empty;
            return false;
        }

        public int Compare(string? v1, string? v2) => 0;
    }

    private sealed class StubSelector : IWorkflowVersionSelector
    {
        public WorkflowVersionIdentity SelectLatest(string canonicalName,
            IReadOnlyCollection<WorkflowVersionIdentity> candidates,
            IWorkflowVersionStrategy strategy)
            => throw new NotSupportedException();
    }
}
