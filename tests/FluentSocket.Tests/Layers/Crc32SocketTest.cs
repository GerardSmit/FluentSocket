using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using FluentSocket.Tests.Extensions;
using Xunit;

namespace FluentSocket.Tests;

public class Crc32SocketTest
{
    [Fact]
    public async Task Write()
    {
        // Arrange
        var socketBase = new Mock<SocketSourceBase>();
        var expected = new byte[]
        {
            // Checksum
            27,
            223,
            5,
            165,

            // Data
            1
        };

        socketBase
            .Setup(x => x.SendAsync(
                It.Is<Memory<byte>>(v => v.ToArray().SequenceEqual(expected)),
                It.IsAny<CancellationToken>()))
            .Returns(default(ValueTask))
            .Verifiable();

        var socket = socketBase.Object.LayerBuilder()
            .AddCrc32()
            .Build();

        // Act
        await socket.SendAsync(new byte[] { 1 });

        // Assert
        socketBase.Verify();
    }

    [Fact]
    public async Task Read()
    {
        // Arrange
        var socketBase = new Mock<SocketSourceBase>();

        socketBase
            .Setup(x => x.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Memory<byte> buffer, CancellationToken _) =>
            {
                // Checksum
                buffer.Span[0] = 27;
                buffer.Span[1] = 223;
                buffer.Span[2] = 5;
                buffer.Span[3] = 165;

                // Data
                buffer.Span[4] = 1;

                return 5;
            });

        var (socket, result) = socketBase.Object.LayerBuilder()
            .AddCrc32()
            .Test();

        // Act
        var bytes = new byte[5];

        await socket.ReceiveAsync(bytes);

        // Assert
        Assert.Equal(1, result.ReceiveData.Span[0]);
    }

    [Fact]
    public async Task ReadInvalid()
    {
        // Arrange
        var socketBase = new Mock<SocketSourceBase>();

        socketBase
            .Setup(x => x.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Memory<byte> buffer, CancellationToken _) =>
            {
                // Invalid checksum
                buffer.Span[0] = 0;
                buffer.Span[1] = 0;
                buffer.Span[2] = 0;
                buffer.Span[3] = 0;

                // Data
                buffer.Span[4] = 1;

                return 5;
            });

        var (socket, result) = socketBase.Object.LayerBuilder()
            .AddCrc32()
            .Test();

        // Act
        var bytes = new byte[5];
        await socket.ReceiveAsync(bytes);

        // Assert
        Assert.Equal(0, result.ReceiveData.Length);
    }
}
