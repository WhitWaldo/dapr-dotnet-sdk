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

public class DefaultWorkflowVersionStrategyFactoryTests
{
    [Fact]
    public void Create_ThrowsWhenStrategyTypeIsNull()
    {
        var factory = new DefaultWorkflowVersionStrategyFactory();
        var services = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => factory.Create(null!, "Orders", null, services));
    }

    [Fact]
    public void Create_ThrowsWhenServicesIsNull()
    {
        var factory = new DefaultWorkflowVersionStrategyFactory();

        Assert.Throws<ArgumentNullException>(() => factory.Create(typeof(TestStrategy), "Orders", null, null!));
    }

    [Fact]
    public void Create_ReturnsRegisteredStrategyInstance()
    {
        var strategy = new TestStrategy();
        var services = new ServiceCollection()
            .AddSingleton(strategy)
            .BuildServiceProvider();
        var factory = new DefaultWorkflowVersionStrategyFactory();

        var resolved = factory.Create(typeof(TestStrategy), "Orders", null, services);

        Assert.Same(strategy, resolved);
    }

    [Fact]
    public void Create_UsesActivatorUtilitiesWhenNotRegistered()
    {
        var services = new ServiceCollection()
            .AddSingleton(new Dependency())
            .BuildServiceProvider();
        var factory = new DefaultWorkflowVersionStrategyFactory();

        var resolved = factory.Create(typeof(InjectableStrategy), "Orders", null, services);

        Assert.IsType<InjectableStrategy>(resolved);
        Assert.NotNull(((InjectableStrategy)resolved).Dependency);
    }

    [Fact]
    public void Create_ThrowsWhenStrategyTypeDoesNotImplementInterface()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var factory = new DefaultWorkflowVersionStrategyFactory();

        Assert.Throws<InvalidOperationException>(() => factory.Create(typeof(object), "Orders", null, services));
    }

    private sealed class Dependency;

    private sealed class InjectableStrategy(Dependency dependency) : IWorkflowVersionStrategy
    {
        public Dependency Dependency { get; } = dependency;

        public bool TryParse(string typeName, out string canonicalName, out string version)
        {
            canonicalName = string.Empty;
            version = string.Empty;
            return false;
        }

        public int Compare(string? v1, string? v2) => 0;
    }

    private sealed class TestStrategy : IWorkflowVersionStrategy
    {
        public bool TryParse(string typeName, out string canonicalName, out string version)
        {
            canonicalName = string.Empty;
            version = string.Empty;
            return false;
        }

        public int Compare(string? v1, string? v2) => 0;
    }
}
