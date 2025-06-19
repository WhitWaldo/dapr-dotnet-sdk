using DaprAspireCommon;
using DaprAspireWeb.Components;
using DaprAspireWeb.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<JobService>();

var app = builder.Build();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Handle the PubSub topic subscription
app.MapPost("/events/jobupdate", ([FromBody] Job job, [FromServices] JobService jobSvc) =>
{
    jobSvc.UpdateStatus(job);
    return TypedResults.Ok();
}).WithTopic(Constants.PubSub.PubSubComponentName, Constants.PubSub.TopicName);

app.UseRouting();
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
