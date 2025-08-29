using Dapr.Workflow;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
    services.AddDaprWorkflow(options =>
    {

    });
});


