using System.Collections.Generic;

namespace Dapr.Workflow.Patterns.Sagas;

/// <summary>
/// Used to build a collection of the steps in a larger saga pattern.
/// </summary>
public sealed class SagaBuilder
{
    /// <summary>
    /// The collection of steps currently contained in this builder instance.
    /// </summary>
    private readonly List<SagaStep> _steps = [];

    /// <summary>
    /// Adds a step to the end of the current steps in the saga.
    /// </summary>
    /// <param name="stepName">The name of the step.</param>
    /// <param name="executeActivityName">The name of the activity to perform to move forward through the saga.</param>
    /// <param name="compensateActivityName">The name of the activity to perform to move backwards through the saga.</param>
    /// <returns>Returns the current instance of the <see cref="SagaBuilder"/>.</returns>
    public SagaBuilder AddStep(string stepName, string executeActivityName, string compensateActivityName)
    {
        _steps.Add(new SagaStep(stepName, executeActivityName, compensateActivityName));
        return this;
    }

    /// <summary>
    /// Returns an ordered list of the steps comprising the saga.
    /// </summary>
    /// <returns>AN ordered list of steps.</returns>
    public IReadOnlyList<SagaStep> Build() => _steps;
}
