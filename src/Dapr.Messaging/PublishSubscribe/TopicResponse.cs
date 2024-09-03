using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapr.Messaging.PublishSubscribe;

/// <summary>
/// 
/// </summary>
public sealed record TopicResponse
{
    /// <summary>
    /// 
    /// </summary>
    public string Id { get; init; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public string Source { get; init; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public string Type { get; init; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public string SpecVersion { get; init; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public string DataContentType { get; init; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public string Topic { get; init; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public string PubSubName { get; init; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public string? Path { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public object? Extensions { get; init; } // TODO: Determine what this should look like.
}
