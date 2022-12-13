using System;
using System.Buffers;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace FluentSocket.Tests;

public class PacketChannelLayerTest
{
    [Fact]
    public async Task SinglePacket()
    {
        // Arrange
        var buffer = new byte[64];
        var socket = new Mock<SocketSourceBase>();

        socket
            .SetupSequence(x => x.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))

            // Single packet
            .ReturnsAsync(() =>
            {
                // Length
                buffer[0] = 1;
                buffer[1] = 0;

                // Content
                buffer[2] = 1;

                return 3;
            })

            // Close
            .ReturnsAsync(0);


        // Act
        await socket.Object.LayerBuilder()
            .PacketLength(LengthType.Int16LittleEndian)
            .PacketChannel(out var reader)
            .ListenAsync(buffer);

        var packet = await reader.ReadAsync();

        // Assert
        Assert.Equal(1, packet.Span[0]);

        packet.Dispose();
    }

    [Fact]
    public async Task MultiplePackets()
    {
        // Arrange
        var socket = new Mock<SocketSourceBase>();
        var buffer = new byte[64];

        socket
            .SetupSequence(x => x.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))

            // Multiple packets
            .ReturnsAsync(() =>
            {
                // - Packet 1
                // Length
                buffer[0] = 1;
                buffer[1] = 0;

                // Content
                buffer[2] = 1;

                // - Packet 2
                // Length
                buffer[3] = 1;
                buffer[4] = 0;

                // Content
                buffer[5] = 2;

                // - Packet 3
                // Length
                buffer[6] = 1;
                buffer[7] = 0;

                // Content
                buffer[8] = 3;

                return 9;
            })

            // Close
            .ReturnsAsync(0);

        // Act
        await socket.Object.LayerBuilder()
            .PacketLength(LengthType.Int16LittleEndian)
            .PacketChannel(out var reader)
            .ListenAsync(buffer);

        var packet1 = await reader.ReadAsync();
        var packet2 = await reader.ReadAsync();
        var packet3 = await reader.ReadAsync();

        // Assert
        Assert.Equal(1, packet1.Span[0]);
        packet1.Dispose();

        Assert.Equal(2, packet2.Span[0]);
        packet2.Dispose();

        Assert.Equal(3, packet3.Span[0]);
        packet3.Dispose();
    }

    [Fact]
    public async Task LargePacket()
    {
        // Arrange
        var socket = new Mock<SocketSourceBase>();
        var buffer = new byte[64];

        socket
            .SetupSequence(x => x.ReceiveAsync(It.IsAny<Memory<byte>>(), It.IsAny<CancellationToken>()))

            // First receive first part of packet
            .ReturnsAsync(() =>
            {
                // Packet 1 - Length
                buffer[0] = 1;
                buffer[1] = 0;
                return 2;
            })

            // Second receive second part of packet
            .ReturnsAsync(() =>
            {
                // Packet 1 - Content
                buffer[0] = 1;

                // Packet 2 - Length
                buffer[1] = 1;
                buffer[2] = 0;

                // Packet 2 - Content
                buffer[3] = 2;

                return 4;
            })

            // Close
            .ReturnsAsync(0);

        // Act
        await socket.Object.LayerBuilder()
            .PacketLength(LengthType.Int16LittleEndian)
            .PacketChannel(out var reader)
            .ListenAsync(buffer);

        var packet1 = await reader.ReadAsync();
        var packet2 = await reader.ReadAsync();

        // Assert
        Assert.Equal(1, packet1.Span[0]);
        Assert.Equal(2, packet2.Span[0]);

        packet1.Dispose();
    }
}
