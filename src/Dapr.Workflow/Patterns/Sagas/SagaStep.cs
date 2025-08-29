namespace Dapr.Workflow.Patterns.Sagas;

/// <summary>
/// Generic serializable step providing the name, execution and compensation operations in a larger saga pattern.
/// </summary>
/// <param name="StepName">The name of the step.</param>
/// <param name="ExecutionActivityName">The name of the activity to execute going forward through the saga steps.</param>
/// <param name="CompensationActivityName">The name of the activity to execute to go backwards through the saga steps.</param>
/// <param name="Input">The optional input argument passed into both the execution and compensation activities.</param>
public record SagaStep(string StepName, string ExecutionActivityName, string CompensationActivityName, object? Input = null);
