using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dapr.Workflow.Patterns.Sagas;

/// <summary>
/// SDK-provided workflow for implementing saga support.
/// </summary>
public class SagaWorkflow : Workflow<SagaWorkflowInput, object?>
{
    /// <inheritdoc />
    public override async Task<object?> RunAsync(WorkflowContext context, SagaWorkflowInput input)
    {
        var completedSteps = new Stack<SagaStep>();
        object? lastResult = null;

        foreach (var step in input.Steps)
        {
            try
            {
                // Determine input for this step - use step's input if provided, otherwise use previous step's output
                var stepInput = step.Input ?? lastResult;
                
                // Execute the step and capture the result
                var result = await context.CallActivityAsync<object?>(step.ExecutionActivityName, stepInput);
                lastResult = result;
                
                // Store the step with the actual input that was used for execution
                var executedStep = step.Input != null ? step : step with { Input = stepInput };
                completedSteps.Push(executedStep);
            }
            catch
            {
                // Compensate in reverse order using the same inputs that were used for execution
                while (completedSteps.Count > 0)
                {
                    var completedStep = completedSteps.Pop();
                    await context.CallActivityAsync(completedStep.CompensationActivityName, completedStep.Input);
                }

                throw;
            }
        }

        return lastResult;
    }
}

/// <summary>
/// SDK-provided workflow for implementing saga support with typed output.
/// </summary>
/// <typeparam name="TOutput">The type of the workflow output.</typeparam>
public class SagaWorkflow<TOutput> : Workflow<SagaWorkflowInput, TOutput>
{
    /// <inheritdoc />
    public override async Task<TOutput> RunAsync(WorkflowContext context, SagaWorkflowInput input)
    {
        var completedSteps = new Stack<SagaStep>();
        object? lastResult = null;

        foreach (var step in input.Steps)
        {
            try
            {
                // Determine input for this step - use step's input if provided, otherwise use previous step's output
                var stepInput = step.Input ?? lastResult;
                
                // Execute the step and capture the result
                var result = await context.CallActivityAsync<object?>(step.ExecutionActivityName, stepInput);
                lastResult = result;
                
                // Store the step with the actual input that was used for execution
                var executedStep = step.Input != null ? step : step with { Input = stepInput };
                completedSteps.Push(executedStep);
            }
            catch
            {
                // Compensate in reverse order using the same inputs that were used for execution
                while (completedSteps.Count > 0)
                {
                    var completedStep = completedSteps.Pop();
                    await context.CallActivityAsync(completedStep.CompensationActivityName, completedStep.Input);
                }

                throw;
            }
        }

        // Cast the last result to the expected output type
        return (TOutput)(lastResult ?? default(TOutput)!);
    }
}
