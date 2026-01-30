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

namespace Dapr.Workflow.Versioning.Runtime.Test;

public class DefaultWorkflowVersioningDiagnosticsTests
{
    [Fact]
    public void Titles_AreStable()
    {
        var diagnostics = new DefaultWorkflowVersioningDiagnostics();

        Assert.Equal("Invalid workflow versioning strategy", diagnostics.UnknownStrategyTitle);
        Assert.Equal("Unable to derive canonical name and version", diagnostics.CouldNotParseTitle);
        Assert.Equal("No versions discovered for workflow family", diagnostics.EmptyFamilyTitle);
        Assert.Equal("Ambiguous latest workflow version", diagnostics.AmbiguousLatestTitle);
    }

    [Fact]
    public void UnknownStrategyMessage_UsesFallbacksForMissingValues()
    {
        var diagnostics = new DefaultWorkflowVersioningDiagnostics();

        var message = diagnostics.UnknownStrategyMessage(string.Empty, null!);

        Assert.Contains("<unknown>", message);
        Assert.Contains("<null>", message);
        Assert.Contains("implemenet IWorkflowVersionStrategy", message);
    }

    [Fact]
    public void CouldNotParseMessage_UsesUnknownWhenTypeNameBlank()
    {
        var diagnostics = new DefaultWorkflowVersioningDiagnostics();

        var message = diagnostics.CouldNotParseMessage("   ");

        Assert.Contains("<unknown>", message);
    }

    [Fact]
    public void EmptyFamilyMessage_UsesUnknownWhenCanonicalNameBlank()
    {
        var diagnostics = new DefaultWorkflowVersioningDiagnostics();

        var message = diagnostics.EmptyFamilyMessage("\t");

        Assert.Contains("<unknown>", message);
    }

    [Fact]
    public void AmbiguousLatestMessage_UsesFallbacksWhenInputsMissing()
    {
        var diagnostics = new DefaultWorkflowVersioningDiagnostics();
        var message = diagnostics.AmbiguousLatestMessage(null!, []);

        Assert.Contains("<unknown>", message);
        Assert.Contains("<none>", message);
    }
}
