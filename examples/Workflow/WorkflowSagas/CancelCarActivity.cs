using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowSagas;

internal sealed class CancelCarActivity(ILogger<CancelCarActivity> logger) : WorkflowActivity<CancelCarRequest, object?>
{
    public override async Task<object?> RunAsync(WorkflowActivityContext context, CancelCarRequest input)
    {
        logger.LogInformation("Starting activity: {ActivityName}", nameof(CancelCarActivity));
        
        // Simulate work
        await Task.Delay(TimeSpan.FromSeconds(2));
        
        logger.LogInformation("Successfully cancelled car reservation for confirmation: {ConfirmationNumber}", input.ConfirmationNumber);
        return null;
    }
}

internal sealed record CancelCarRequest(string ConfirmationNumber);
