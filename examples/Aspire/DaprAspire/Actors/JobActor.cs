// ------------------------------------------------------------------------
//  Copyright 2025 The Dapr Authors
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  ------------------------------------------------------------------------

using Dapr.Actors.Runtime;
using Dapr.Client;
using DaprAspireCommon;

namespace DaprAspire.Actors;

/// <summary>
/// Represents a scheduled job and its current status.
/// </summary>
internal sealed class JobActor(ActorHost host, DaprClient daprClient) : Actor(host), IJobActor, IRemindable
{
    private const string JobStateName = "job";
    private const string UpdateReminderName = "update";
    private readonly static Random Random = new();
    
    /// <summary>
    /// Save the job to state and set up the reminder to handle updates.
    /// </summary>
    /// <param name="job"></param>
    public async Task ScheduleJobAsync(Job job)
    {
        //Save in state
        await StateManager.SetStateAsync(JobStateName, job);
        
        //Set up the reminder
        await RegisterReminderAsync(UpdateReminderName, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }
    
    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        switch (reminderName)
        {
            case UpdateReminderName:
                await UpdateJobStateAsync();
                break;
        }
    }

    private async Task UpdateJobStateAsync()
    {
        var job = await StateManager.GetStateAsync<Job>(JobStateName);

        if (job.Status == JobStatus.Processing)
        {
            //Determine if the job has been completed or not
            if (DetermineJobCompleted())
            {
                //Update the state on the actor
                var updatedJob = job with { Status = JobStatus.Completed };
                await StateManager.SetStateAsync(JobStateName, updatedJob);
                
                //Publish the updated state via PubSub
                await daprClient.PublishEventAsync(Constants.PubSub.PubSubComponentName, Constants.PubSub.TopicName,
                    updatedJob);
            }
        }
        
        if (job.Status == JobStatus.Completed)
        {
            //Clear the reminder as the job is done
            var reminder = await GetReminderAsync(UpdateReminderName);
            await UnregisterReminderAsync(reminder);
        }
    }

    /// <summary>
    /// Determines if the given job has been completed.
    /// </summary>
    /// <returns>If true, the job has been completed; otherwise the job is still pending.</returns>
    private static bool DetermineJobCompleted()
    {
        //There's a 30% chance the job is completed
        var result = Random.Next(1, 101);
        return result <= 30;
    }
}
