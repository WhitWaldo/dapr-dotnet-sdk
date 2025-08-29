using System.Collections.Generic;

namespace Dapr.Workflow.Patterns.Sagas;

/// <summary>
/// Provides the input to a <see cref="SagaWorkflow"/>.
/// </summary>
public class SagaWorkflowInput
{
    /// <summary>
    /// The various steps to perform as the input to a saga workflow.
    /// </summary>
    public List<SagaStep> Steps { get; set; } = [];
}
