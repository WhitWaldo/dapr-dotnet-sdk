using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowSagas;

internal sealed class CancelHotelActivity(ILogger<CancelHotelActivity> logger) : WorkflowActivity<CancelHotelRequest, object?>
{
    public override async Task<object?> RunAsync(WorkflowActivityContext context, CancelHotelRequest input)
    {
        logger.LogInformation("Starting activity: {ActivityName}", nameof(CancelHotelActivity));
        
        // Simulate work
        await Task.Delay(TimeSpan.FromSeconds(2));
        
        logger.LogInformation("Successfully cancelled hotel reservation for confirmation: {ConfirmationNumber}", input.ConfirmationNumber);
        return null;
    }
}

internal sealed record CancelHotelRequest(string ConfirmationNumber);
