using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowSagas;

internal sealed class BookFlightActivity(ILogger<BookFlightActivity> logger) : WorkflowActivity<BookFlightRequest, BookFlightResult>
{
    public override async Task<BookFlightResult> RunAsync(WorkflowActivityContext context, BookFlightRequest input)
    {
        logger.LogInformation("Starting activity: {ActivityName}", nameof(BookFlightActivity));
        
        //Simulate work
        await Task.Delay(TimeSpan.FromSeconds(2));

        var result = new BookFlightResult("123456");
        logger.LogInformation("Activity completed with result: {Result}", result);
        return result;
    }
}

internal sealed record BookFlightRequest(string From, string To);
internal sealed record BookFlightResult(string ConfirmationNumber);
