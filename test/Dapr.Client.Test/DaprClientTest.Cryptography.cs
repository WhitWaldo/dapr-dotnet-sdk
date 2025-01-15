// ------------------------------------------------------------------------
// Copyright 2025 The Dapr Authors
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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Dapr.Client.Test;

public partial class DaprClientTest
{
    [Fact]
    public async Task EncryptAsync_WithPlaintextBytes_ReturnsEncryptedBytes()
    {
        const string vaultResourceName = "vault";
        var plaintextBytes = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 });
        const string keyName = "key";
        var encryptionOptions = new EncryptionOptions(KeyWrapAlgorithm.Rsa);
        var cancellationToken = CancellationToken.None;

        var mockClient = new MockClient();
        
        
        
    }
}
