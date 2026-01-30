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

namespace Dapr.Workflow.Versioning.Runtime.Test;

public class MaxVersionSelectorTests
{
    [Fact]
    public void SelectLatest_UsesStrategyToPickMax()
    {
        var selector = new MaxVersionSelector();
        var strategy = new NumericStrategy();
        var candidates = new[]
        {
            new WorkflowVersionIdentity("Orders", "10", "OrdersWorkflowV10"),
            new WorkflowVersionIdentity("Orders", "2", "OrdersWorkflowV2"),
            new WorkflowVersionIdentity("Orders", "7", "OrdersWorkflowV7"),
        };

        var latest = selector.SelectLatest("Orders", candidates, strategy);

        Assert.Equal("10", latest.Version);
    }

    [Fact]
    public void SelectLatest_ThrowsWhenCandidatesNull()
    {
        var selector = new MaxVersionSelector();
        var strategy = new NumericStrategy();

        Assert.Throws<ArgumentNullException>(() => selector.SelectLatest("Orders", null!, strategy));
    }

    [Fact]
    public void SelectLatest_ThrowsWhenStrategyNull()
    {
        var selector = new MaxVersionSelector();
        var candidates = new[]
        {
            new WorkflowVersionIdentity("Orders", "1", "OrdersWorkflowV1"),
        };

        Assert.Throws<ArgumentNullException>(() => selector.SelectLatest("Orders", candidates, null!));
    }

    [Fact]
    public void SelectLatest_ThrowsWhenCandidatesEmpty()
    {
        var selector = new MaxVersionSelector();
        var strategy = new NumericStrategy();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            selector.SelectLatest("Orders", Array.Empty<WorkflowVersionIdentity>(), strategy));
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
}
