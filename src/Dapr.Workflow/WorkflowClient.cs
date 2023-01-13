﻿// ------------------------------------------------------------------------
// Copyright 2022 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;

namespace Dapr.Workflow
{
    // TODO: This will be replaced by the official Dapr Workflow management client.
    /// <summary>
    /// Defines client operations for managing Dapr Workflow instances.
    /// </summary>
    public sealed class WorkflowClient : IAsyncDisposable
    {
        readonly DurableTaskClient innerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowClient"/> class.
        /// </summary>
        /// <param name="innerClient">The Durable Task client used to communicate with the Dapr sidecar.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="innerClient"/> is <c>null</c>.</exception>
        public WorkflowClient(DurableTaskClient innerClient)
        {
            this.innerClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
        }

        /// <summary>
        /// Schedules a new workflow instance for execution.
        /// </summary>
        /// <param name="name">The name of the orchestrator to schedule.</param>
        /// <param name="instanceId">
        /// The unique ID of the orchestration instance to schedule. If not specified, a new GUID value is used.
        /// </param>
        /// <param name="startTime">
        /// The time when the orchestration instance should start executing. If not specified or if a date-time in the past
        /// is specified, the orchestration instance will be scheduled immediately.
        /// </param>
        /// <param name="input">
        /// The optional input to pass to the scheduled orchestration instance. This must be a serializable value.
        /// </param>
        public Task<string> ScheduleNewWorkflowAsync(
            string name,
            string? instanceId = null,
            object? input = null,
            DateTime? startTime = null)
        {
            StartOrchestrationOptions options = new(instanceId, startTime);
            return this.innerClient.ScheduleNewOrchestrationInstanceAsync(name, input, options);
        }

        /// <summary>
        /// Fetches runtime metadata for the specified workflow instance.
        /// </summary>
        /// <param name="instanceId">The unique ID of the orchestration instance to fetch.</param>
        /// <param name="getInputsAndOutputs">
        /// Specify <c>true</c> to fetch the orchestration instance's inputs, outputs, and custom status, or <c>false</c> to
        /// omit them. Defaults to false.
        /// </param>
        public async Task<WorkflowMetadata> GetWorkflowMetadataAsync(string instanceId, bool getInputsAndOutputs = false)
        {
            OrchestrationMetadata? metadata = await this.innerClient.GetInstanceMetadataAsync(
                instanceId,
                getInputsAndOutputs);
            return new WorkflowMetadata(metadata);
        }

        /// <summary>
        /// Disposes any unmanaged resources associated with this client.
        /// </summary>
        public ValueTask DisposeAsync()
        {
            return ((IAsyncDisposable)this.innerClient).DisposeAsync();
        }
    }
}
