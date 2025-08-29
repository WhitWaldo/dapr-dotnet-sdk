using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dapr.Workflow.Patterns.Sagas;

/// <summary>
/// Helper class to start a saga workflow.
/// </summary>
public static class SagaRunner
{
    /// <summary>
    /// Executes a Saga workflow with the given steps.
    /// </summary>
    /// <param name="client">The <see cref="DaprWorkflowClient"/>.</param>
    /// <param name="steps">The steps to perform in the saga.</param>
    /// <param name="instanceId">A preferred instance ID for the saga workflow, if any.</param>
    /// <param name="startTime">An optional time to schedule the start of the saga workflow.</param>
    public static async Task RunSagaAsync(DaprWorkflowClient client, List<SagaStep> steps, string? instanceId = null, DateTimeOffset? startTime = null)
    {
        var input = new SagaWorkflowInput { Steps = steps };
        await client.ScheduleNewWorkflowAsync(nameof(SagaWorkflow), instanceId, input, startTime);
    }
}
