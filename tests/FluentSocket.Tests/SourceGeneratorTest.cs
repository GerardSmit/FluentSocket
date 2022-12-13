using System.Threading.Tasks;
using Moq;
using FluentSocket.Protocol;
using FluentSocket.Tests.Messages;
using Xunit;

namespace FluentSocket.Tests;

public class SourceGeneratorTest
{
    [Fact]
    public void TestWrite()
    {
        // Arrange
        var bytes = new byte[9];
        var writer = new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        writer.WriteUnion(new MovePlayerMessage
        {
            X = 1,
            Y = 2
        });

        // Assert
        Assert.Equal(new byte[]
        {
            0, // MovePlayerMessage
            1, 0, 0, 0, // X
            2, 0, 0, 0 // Y
        }, bytes);
    }

    [Fact]
    public void TestRead()
    {
        // Arrange
        var bytes = new byte[]
        {
            0, // MovePlayerMessage
            1, 0, 0, 0, // X
            2, 0, 0, 0 // Y
        };
        var reader = new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var message = reader.ReadUnion<IServerMessage>();

        // Assert
        Assert.IsType<MovePlayerMessage>(message);

        var movePlayerMessage = (MovePlayerMessage)message;
        Assert.Equal(1, movePlayerMessage.X);
        Assert.Equal(2, movePlayerMessage.Y);
    }

    [Fact]
    public void TestMessageHandler()
    {
        // Arrange
        var bytes = new byte[]
        {
            0, // MovePlayerMessage
            1, 0, 0, 0, // X
            2, 0, 0, 0 // Y
        };
        var reader = new MessageReader(bytes, LittleEndianProtocol.Utf8);
        var mock = new Mock<IServerMessageHandler>();

        // Act
        mock.Object.Handle(ref reader);

        // Assert
        mock.Verify(x => x.HandleMovePlayerMessage(It.IsAny<MovePlayerMessage>()), Times.Once);
    }

    [Fact]
    public void TestHandler()
    {
        // Arrange
        var mock = new Mock<IServerMessageHandler>();
        var message = new MovePlayerMessage
        {
            X = 1,
            Y = 2
        };

        // Act
        mock.Object.Handle(message);

        // Assert
        mock.Verify(x => x.HandleMovePlayerMessage(It.IsAny<MovePlayerMessage>()), Times.Once);
    }

    [Fact]
    public async Task TestHandlerAsync()
    {
        // Arrange
        var mock = new Mock<IAsyncServerMessageHandler>();
        var message = new MovePlayerMessage
        {
            X = 1,
            Y = 2
        };

        // Act
        await mock.Object.HandleAsync(message);

        // Assert
        mock.Verify(x => x.HandleMovePlayerMessageAsync(It.IsAny<MovePlayerMessage>()), Times.Once);
    }
}
