using Dapr.Actors;
using Dapr.Actors.Client;
using DaprAspire.Actors;
using DaprAspireCommon;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddActors(opt =>
{
    opt.Actors.RegisterActor<JobActor>();
});

var app = builder.Build();

app.MapPost("schedule-job", async ([FromBody] Job job) =>
{
    var jobActor = ActorProxy.Create<IJobActor>(new ActorId(job.Id.ToString()), nameof(JobActor));
    await jobActor.ScheduleJobAsync(job);
});

app.Run();
