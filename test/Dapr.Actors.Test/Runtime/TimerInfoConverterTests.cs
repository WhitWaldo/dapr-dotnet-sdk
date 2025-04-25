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

using System;
using System.Text;
using System.Text.Json;
using Xunit;
#pragma warning disable CS0618 // Type or member is obsolete

namespace Dapr.Actors.Runtime;

public class TimerInfoConverterTests
{
    [Fact]
    public void ShouldDeserializeProperly()
    {
        const string testValue = "test value";
        const string callbackName = "MyCallback";
        
        var myState = Encoding.UTF8.GetBytes(testValue);

        var dueTime = TimeSpan.FromSeconds(14);
        var period = TimeSpan.FromSeconds(3);
        var ttl = TimeSpan.FromHours(4);


        var timerInfo = new TimerInfo(callbackName, myState, dueTime, period, ttl);
        var options = new JsonSerializerOptions();
        options.Converters.Add(new TimerInfoConverter());
        var serializedValue = JsonSerializer.Serialize(timerInfo, options);

        var deserializedValue = JsonSerializer.Deserialize<TimerInfo>(serializedValue, options);

        Assert.Equal(testValue, Encoding.UTF8.GetString(deserializedValue.Data));
        Assert.Equal(callbackName, deserializedValue.Callback);
        Assert.Equal(dueTime, deserializedValue.DueTime);
        Assert.Equal(period, deserializedValue.Period);
        Assert.Equal(ttl, deserializedValue.Ttl);
    }
}
