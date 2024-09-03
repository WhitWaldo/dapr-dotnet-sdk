using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapr.Messaging.PublishSubscribe;

/// <summary>
/// 
/// </summary>
public enum TopicResponseType
{
    /// <summary>
    /// 
    /// </summary>
    Success,
    /// <summary>
    /// 
    /// </summary>
    Retry,
    /// <summary>
    /// 
    /// </summary>
    Drop
}
