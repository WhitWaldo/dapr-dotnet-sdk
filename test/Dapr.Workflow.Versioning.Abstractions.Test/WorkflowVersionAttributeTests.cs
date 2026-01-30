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

using System.Reflection;

namespace Dapr.Workflow.Versioning.Abstractions.Test;

public class WorkflowVersionAttributeTests
{
    [Fact]
    public void Properties_DefaultToNull()
    {
        var attribute = new WorkflowVersionAttribute();

        Assert.Null(attribute.CanonicalName);
        Assert.Null(attribute.Version);
        Assert.Null(attribute.StrategyType);
    }

    [Fact]
    public void Properties_SetViaInit()
    {
        var attribute = new WorkflowVersionAttribute
        {
            CanonicalName = "Orders",
            Version = "3.1.0",
            StrategyType = typeof(object),
        };

        Assert.Equal("Orders", attribute.CanonicalName);
        Assert.Equal("3.1.0", attribute.Version);
        Assert.Equal(typeof(object), attribute.StrategyType);
    }

    [Fact]
    public void AttributeUsage_IsClassOnlyAndNotInheritedOrRepeatable()
    {
        var usage = typeof(WorkflowVersionAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>();

        Assert.NotNull(usage);
        Assert.Equal(AttributeTargets.Class, usage!.ValidOn);
        Assert.False(usage.Inherited);
        Assert.False(usage.AllowMultiple);
    }
}
