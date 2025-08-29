using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowSagas;

internal sealed class CancelFlightActivity(ILogger<CancelFlightActivity> logger) : WorkflowActivity<CancelFlightRequest, object?>
{
    public override async Task<object?> RunAsync(WorkflowActivityContext context, CancelFlightRequest input)
    {
        logger.LogInformation("Starting activity: {ActivityName}", nameof(CancelFlightActivity));
        
        // Simulate work
        await Task.Delay(TimeSpan.FromSeconds(2));
        
        logger.LogInformation("Successfully cancelled flight reservation for confirmation: {ConfirmationNumber}", input.ConfirmationNumber);
        return null;
    }
}

internal sealed record CancelFlightRequest(string ConfirmationNumber);
