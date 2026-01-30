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

public class VersionParseResultTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var result = new VersionParseResult("Orders", "3.1.0", true);

        Assert.Equal("Orders", result.CanonicalName);
        Assert.Equal("3.1.0", result.Version);
        Assert.True(result.IsExplicit);
    }

    [Fact]
    public void Equality_UsesValueSemantics()
    {
        var left = new VersionParseResult("Orders", "3.1.0", true);
        var right = new VersionParseResult("Orders", "3.1.0", true);
        var different = new VersionParseResult("Orders", "3.1.1", true);

        Assert.Equal(left, right);
        Assert.NotEqual(left, different);
    }

    [Fact]
    public void Deconstruct_ReturnsExpectedValues()
    {
        var result = new VersionParseResult("Orders", "3.1.0", false);

        var (canonicalName, version, isExplicit) = result;

        Assert.Equal("Orders", canonicalName);
        Assert.Equal("3.1.0", version);
        Assert.False(isExplicit);
    }
}
