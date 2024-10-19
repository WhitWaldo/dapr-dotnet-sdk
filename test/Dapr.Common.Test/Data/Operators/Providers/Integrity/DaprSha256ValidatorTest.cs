﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Common.Data.Operations.Providers.Integrity.Checksum;
using Xunit;

namespace Dapr.Common.Test.Data.Operators.Providers.Integrity;

public class DaprSha256ValidatorTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCalculateChecksum()
    {
        // Arrange
        var validator = new DaprSha256Validator();
        var input = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3, 4, 5 });

        // Act
        var result = await validator.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Metadata.ContainsKey("Checksum"));
    }

    [Fact]
    public async Task ReverseAsync_ShouldValidateChecksum()
    {
        // Arrange
        var validator = new DaprSha256Validator();
        var input = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3, 4, 5 });
        var result = await validator.ExecuteAsync(input);

        // Act & Assert
        await validator.ReverseAsync(result, CancellationToken.None);
    }

    [Fact]
    public async Task ReverseAsync_ShouldThrowExceptionForInvalidChecksum()
    {
        // Arrange
        var validator = new DaprSha256Validator();
        var input = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3, 4, 5 });
        var result = await validator.ExecuteAsync(input);
        result = result with { Payload = new ReadOnlyMemory<byte>(new byte[] { 6, 7, 8, 9, 0 }) };

        // Act & Assert
        await Assert.ThrowsAsync<DaprException>(() => validator.ReverseAsync(result, CancellationToken.None));
    }
}
