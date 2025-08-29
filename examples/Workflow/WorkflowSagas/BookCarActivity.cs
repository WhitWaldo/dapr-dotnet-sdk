using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowSagas;

internal sealed class BookCarActivity(ILogger<BookCarActivity> logger) : WorkflowActivity<BookCarRequest, BookCarResult>
{
    public override async Task<BookCarResult> RunAsync(WorkflowActivityContext context, BookCarRequest input)
    {
        logger.LogInformation("Starting activity: {ActivityName}", nameof(BookCarActivity));
        
        //Simulate work
        await Task.Delay(TimeSpan.FromSeconds(2));
        
        logger.LogInformation("Forcing failure to trigger compensation for activity: {ActivityName}", nameof(BookCarActivity));
        throw new Exception("Failed to book car");
    }
}

internal sealed record BookCarRequest(string Type, DateTimeOffset From, DateTimeOffset To);
internal sealed record BookCarResult(string ConfirmationNumber);
