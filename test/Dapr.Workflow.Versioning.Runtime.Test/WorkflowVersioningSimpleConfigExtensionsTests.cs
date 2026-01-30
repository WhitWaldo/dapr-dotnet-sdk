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
using Microsoft.Extensions.Options;

namespace Dapr.Workflow.Versioning.Runtime.Test;

public class WorkflowVersioningSimpleConfigExtensionsTests
{
    [Fact]
    public void UseDefaultWorkflowStrategy_ConfiguresOptionsDelegate()
    {
        var services = new ServiceCollection();
        services.AddOptions<WorkflowVersioningOptions>();
        services.AddSingleton<IWorkflowVersionStrategyFactory, RecordingStrategyFactory>();

        services.UseDefaultWorkflowStrategy<StubStrategy>("orders");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WorkflowVersioningOptions>>().Value;

        var strategy = options.DefaultStrategy!(provider);

        Assert.IsType<StubStrategy>(strategy);
        var factory = (RecordingStrategyFactory)provider.GetRequiredService<IWorkflowVersionStrategyFactory>();
        Assert.Equal(typeof(StubStrategy), factory.LastStrategyType);
        Assert.Equal("DEFAULT", factory.LastCanonicalName);
        Assert.Equal("orders", factory.LastOptionsName);
        Assert.Same(provider, factory.LastServices);
    }

    [Fact]
    public void UseDefaultWorkflowSelector_ConfiguresOptionsDelegate()
    {
        var services = new ServiceCollection();
        services.AddOptions<WorkflowVersioningOptions>();
        services.AddSingleton<IWorkflowVersionSelectorFactory, RecordingSelectorFactory>();

        services.UseDefaultWorkflowSelector<StubSelector>("orders");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<WorkflowVersioningOptions>>().Value;

        var selector = options.DefaultSelector!(provider);

        Assert.IsType<StubSelector>(selector);
        var factory = (RecordingSelectorFactory)provider.GetRequiredService<IWorkflowVersionSelectorFactory>();
        Assert.Equal(typeof(StubSelector), factory.LastSelectorType);
        Assert.Equal("DEFAULT", factory.LastCanonicalName);
        Assert.Equal("orders", factory.LastOptionsName);
        Assert.Same(provider, factory.LastServices);
    }

    [Fact]
    public void ConfigureStrategyOptions_RegistersNamedOptions()
    {
        var services = new ServiceCollection();
        services.AddOptions();

        services.ConfigureStrategyOptions<StrategyOptions>("orders", options => options.AllowPrerelease = true);

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<StrategyOptions>>().Get("orders");

        Assert.True(options.AllowPrerelease);
    }

    [Fact]
    public void ConfigureStrategyOptions_ThrowsWhenArgumentsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            WorkflowVersioningSimpleConfigExtensions.ConfigureStrategyOptions<StrategyOptions>(null!, "orders",
                _ => { }));

        Assert.Throws<ArgumentNullException>(() =>
            services.ConfigureStrategyOptions<StrategyOptions>("orders", null!));
    }

    private sealed class StrategyOptions
    {
        public bool AllowPrerelease { get; set; }
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

    private sealed class RecordingStrategyFactory : IWorkflowVersionStrategyFactory
    {
        public Type? LastStrategyType { get; private set; }
        public string? LastCanonicalName { get; private set; }
        public string? LastOptionsName { get; private set; }
        public IServiceProvider? LastServices { get; private set; }

        public IWorkflowVersionStrategy Create(Type strategyType, string canonicalName, string? optionsName,
            IServiceProvider services)
        {
            LastStrategyType = strategyType;
            LastCanonicalName = canonicalName;
            LastOptionsName = optionsName;
            LastServices = services;
            return new StubStrategy();
        }
    }

    private sealed class RecordingSelectorFactory : IWorkflowVersionSelectorFactory
    {
        public Type? LastSelectorType { get; private set; }
        public string? LastCanonicalName { get; private set; }
        public string? LastOptionsName { get; private set; }
        public IServiceProvider? LastServices { get; private set; }

        public IWorkflowVersionSelector Create(Type selectorType, string canonicalName, string? optionsName,
            IServiceProvider services)
        {
            LastSelectorType = selectorType;
            LastCanonicalName = canonicalName;
            LastOptionsName = optionsName;
            LastServices = services;
            return new StubSelector();
        }
    }
}
