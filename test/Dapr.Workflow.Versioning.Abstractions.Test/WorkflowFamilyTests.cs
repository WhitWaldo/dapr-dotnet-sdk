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

public class WorkflowFamilyTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var versions = new[]
        {
            new WorkflowVersionIdentity("Orders", "1", "OrdersWorkflowV1"),
            new WorkflowVersionIdentity("Orders", "2", "OrdersWorkflowV2"),
        };

        var family = new WorkflowFamily("Orders", versions);

        Assert.Equal("Orders", family.CanonicalName);
        Assert.Same(versions, family.Versions);
    }

    [Fact]
    public void Equality_UsesReferenceEqualityForVersionsCollection()
    {
        var versions = new[]
        {
            new WorkflowVersionIdentity("Orders", "1", "OrdersWorkflowV1"),
        };
        var sameReference = new WorkflowFamily("Orders", versions);
        var sameValuesDifferentCollection = new WorkflowFamily("Orders", new[]
        {
            new WorkflowVersionIdentity("Orders", "1", "OrdersWorkflowV1"),
        });

        Assert.Equal(sameReference, new WorkflowFamily("Orders", versions));
        Assert.NotEqual(sameReference, sameValuesDifferentCollection);
    }
}
