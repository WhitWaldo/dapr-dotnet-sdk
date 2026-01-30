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

public class DefaultWorkflowVersionSelectorFactoryTests
{
    [Fact]
    public void Create_ThrowsWhenSelectorTypeIsNull()
    {
        var factory = new DefaultWorkflowVersionSelectorFactory();
        var services = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => factory.Create(null!, "Orders", null, services));
    }

    [Fact]
    public void Create_ThrowsWhenServicesIsNull()
    {
        var factory = new DefaultWorkflowVersionSelectorFactory();

        Assert.Throws<ArgumentNullException>(() => factory.Create(typeof(TestSelector), "Orders", null, null!));
    }

    [Fact]
    public void Create_ReturnsRegisteredSelectorInstance()
    {
        var selector = new TestSelector();
        var services = new ServiceCollection()
            .AddSingleton(selector)
            .BuildServiceProvider();
        var factory = new DefaultWorkflowVersionSelectorFactory();

        var resolved = factory.Create(typeof(TestSelector), "Orders", null, services);

        Assert.Same(selector, resolved);
    }

    [Fact]
    public void Create_UsesActivatorUtilitiesWhenNotRegistered()
    {
        var services = new ServiceCollection()
            .AddSingleton(new Dependency())
            .BuildServiceProvider();
        var factory = new DefaultWorkflowVersionSelectorFactory();

        var resolved = factory.Create(typeof(InjectableSelector), "Orders", null, services);

        Assert.IsType<InjectableSelector>(resolved);
        Assert.NotNull(((InjectableSelector)resolved).Dependency);
    }

    [Fact]
    public void Create_ThrowsWhenSelectorTypeDoesNotImplementInterface()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var factory = new DefaultWorkflowVersionSelectorFactory();

        Assert.Throws<InvalidOperationException>(() => factory.Create(typeof(object), "Orders", null, services));
    }

    private sealed class Dependency;

    private sealed class InjectableSelector(Dependency dependency) : IWorkflowVersionSelector
    {
        public Dependency Dependency { get; } = dependency;

        public WorkflowVersionIdentity SelectLatest(string canonicalName,
            IReadOnlyCollection<WorkflowVersionIdentity> candidates,
            IWorkflowVersionStrategy strategy)
            => throw new NotSupportedException();
    }

    private sealed class TestSelector : IWorkflowVersionSelector
    {
        public WorkflowVersionIdentity SelectLatest(string canonicalName,
            IReadOnlyCollection<WorkflowVersionIdentity> candidates,
            IWorkflowVersionStrategy strategy)
            => throw new NotSupportedException();
    }
}
