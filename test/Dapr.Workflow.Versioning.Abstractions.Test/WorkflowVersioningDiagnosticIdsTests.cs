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

public class WorkflowVersioningDiagnosticIdsTests
{
    [Theory]
    [InlineData(IWorkflowVersioningDiagnosticIds.UnknownStrategy, "DWV001")]
    [InlineData(IWorkflowVersioningDiagnosticIds.CouldNotParse, "DWV002")]
    [InlineData(IWorkflowVersioningDiagnosticIds.EmptyFamily, "DWV003")]
    [InlineData(IWorkflowVersioningDiagnosticIds.AmbiguousLatest, "DWV004")]
    public void DiagnosticIds_AreStable(string actual, string expected)
    {
        Assert.Equal(expected, actual);
    }
}
