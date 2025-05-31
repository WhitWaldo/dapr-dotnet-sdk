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

using System.Buffers;
using System.Text;
using BenchmarkDotNet.Attributes;
using Dapr.Cryptography.Encryption;
using Dapr.Cryptography.Encryption.Extensions;
using Dapr.Cryptography.Encryption.Models;
using Microsoft.Extensions.DependencyInjection;

namespace PerfCryptoLocal8;

[MemoryDiagnoser]
public class EncryptionBenchmarks
{
    private const string ComponentName = "localstorage";
    private const string KeyName = "rsa-private-key.pem";
    private IDaprEncryptionClient _daprClient;
    
    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddDaprEncryptionClient();
        var provider = services.BuildServiceProvider();

        _daprClient = provider.GetRequiredService<IDaprEncryptionClient>();
    }

    [Benchmark]
    public async Task<byte[]> EncryptTinyDataFromBytes()
    {
        var testValueBytes = Encoding.UTF8.GetBytes("This is a tiny test!");
        var encryptedBytes = await _daprClient.EncryptAsync(ComponentName, testValueBytes, KeyName,
            new EncryptionOptions(KeyWrapAlgorithm.Rsa));
        return encryptedBytes.ToArray();
    }

    [Benchmark]
    public async Task<byte[]> EncryptTinyDataFromStream()
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("This is a tiny test!"));
        var encryptedBytes = _daprClient.EncryptAsync(ComponentName, ms, KeyName, new EncryptionOptions(KeyWrapAlgorithm.Rsa));
        var bufferedEncryptedBytes = new ArrayBufferWriter<byte>();
        await foreach (var bytes in encryptedBytes)
        {
            bufferedEncryptedBytes.Write(bytes.Span);
        }

        return bufferedEncryptedBytes.WrittenSpan.ToArray();
    }
}
