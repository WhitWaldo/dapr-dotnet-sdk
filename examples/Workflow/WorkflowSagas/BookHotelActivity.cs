using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowSagas;

internal sealed class BookHotelActivity(ILogger<BookHotelActivity> logger) : WorkflowActivity<BookHotelRequest, BookHotelResult>
{
    public override async Task<BookHotelResult> RunAsync(WorkflowActivityContext context, BookHotelRequest input)
    {
        logger.LogInformation("Starting activity: {ActivityName}", nameof(BookHotelActivity));
        
        // Simulate work
        await Task.Delay(TimeSpan.FromSeconds(2));

        var result = new BookHotelResult("98765");
        logger.LogInformation("Booked hotel successfully with result: {Result}", result);
        return result;
    }
}

internal sealed record BookHotelRequest(DateOnly From, DateOnly To);
internal sealed record BookHotelResult(string ConfirmationNumber);
