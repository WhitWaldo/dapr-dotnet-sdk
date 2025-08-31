using Dapr.Workflow;
using Microsoft.Extensions.Logging;

namespace WorkflowSagas;

internal sealed class BookTripWorkflow : Workflow<BookTripRequest, BookTripResult>
{
    public override async Task<BookTripResult> RunAsync(WorkflowContext context, BookTripRequest input)
    {
        var logger = context.CreateReplaySafeLogger<BookTripWorkflow>();
        logger.LogInformation("Starting workflow: {WorkflowName}", nameof(BookTripWorkflow));

        try
        {
            // Book flight
            
            
            logger.LogInformation("Booked flight successfully with confirmation number: {FlightConfirmationNumber}", );
            
            // Book hotel
            
            // Book car


            var result = new BookTripResult();
            logger.LogInformation("Booked trip successfully: {ConfirmationNumberResult}", result);
            return result;
        }
        
    }
}

internal sealed record BookTripRequest();
internal sealed record BookTripResult(string HotelConfirmationNumber, string CarConfirmationNumber, string FlightConfirmationNumber);
