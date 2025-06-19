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

using System.Text.Json.Serialization;

namespace DaprAspireCommon;

/// <summary>
/// Represents a given job in the system.
/// </summary>
public sealed record Job
{
    /// <summary>
    /// The identifier of the job.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The current job status.
    /// </summary>
    [JsonPropertyName("status")]
    public JobStatus Status { get; init; } = JobStatus.Processing;
}
