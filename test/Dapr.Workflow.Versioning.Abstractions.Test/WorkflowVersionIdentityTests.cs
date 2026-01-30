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

namespace Dapr.Workflow.Versioning.Abstractions.Test;

public class WorkflowVersionIdentityTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var identity = new WorkflowVersionIdentity("Orders", "3", "OrdersWorkflow", "Orders.Assembly");

        Assert.Equal("Orders", identity.CanonicalName);
        Assert.Equal("3", identity.Version);
        Assert.Equal("OrdersWorkflow", identity.TypeName);
        Assert.Equal("Orders.Assembly", identity.AssemblyName);
    }

    [Fact]
    public void AssemblyName_IsOptional()
    {
        var identity = new WorkflowVersionIdentity("Orders", "3", "OrdersWorkflow");

        Assert.Null(identity.AssemblyName);
    }

    [Fact]
    public void ToString_FormatsWithCanonicalVersionAndTypeName()
    {
        var identity = new WorkflowVersionIdentity("Orders", "3", "OrdersWorkflow", "Orders.Assembly");

        Assert.Equal("Orders@3 (OrdersWorkflow)", identity.ToString());
        Assert.DoesNotContain("Orders.Assembly", identity.ToString());
    }

    [Fact]
    public void Equality_IncludesAssemblyName()
    {
        var left = new WorkflowVersionIdentity("Orders", "3", "OrdersWorkflow", "Orders.Assembly");
        var right = new WorkflowVersionIdentity("Orders", "3", "OrdersWorkflow", "Orders.Assembly");
        var differentAssembly = new WorkflowVersionIdentity("Orders", "3", "OrdersWorkflow", "Other.Assembly");

        Assert.Equal(left, right);
        Assert.NotEqual(left, differentAssembly);
    }
}
