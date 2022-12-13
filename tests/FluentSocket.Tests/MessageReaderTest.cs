using FluentSocket.Protocol;
using Xunit;

namespace FluentSocket.Tests;

public class MessageReaderTest
{

    [Fact]
    public void TestInt64()
    {
        // Arrange
        var bytes = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadInt64();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestUInt64()
    {
        // Arrange
        var bytes = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadUInt64();

        // Assert
        Assert.Equal(1UL, result);
    }

    [Fact]
    public void TestInt32()
    {
        // Arrange
        var bytes = new byte[] { 1, 0, 0, 0 };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadInt32();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestUInt32()
    {
        // Arrange
        var bytes = new byte[] { 1, 0, 0, 0 };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadUInt32();

        // Assert
        Assert.Equal(1u, result);
    }

    [Fact]
    public void TestInt16()
    {
        // Arrange
        var bytes = new byte[] { 1, 0 };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadInt16();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestUInt16()
    {
        // Arrange
        var bytes = new byte[] { 1, 0 };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadUInt16();

        // Assert
        Assert.Equal(1u, result);
    }

    [Fact]
    public void TestByte()
    {
        // Arrange
        var bytes = new byte[] { 1 };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadByte();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void TestSingle()
    {
        // Arrange
        var bytes = new byte[] { 0, 0, 128, 63 };

        var messageReader = new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadSingle();

        // Assert
        Assert.Equal(1f, result);
    }

    [Fact]
    public void TestDouble()
    {
        // Arrange
        var bytes = new byte[] { 0, 0, 0, 0, 0, 0, 240, 63 };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadDouble();

        // Assert
        Assert.Equal(1d, result);
    }

    [Fact]
    public void TestString()
    {
        // Arrange
        var bytes = new byte[]
        {
            4, 0, 0, 0, // Length
            116, 101, 115, 116 // "test"
        };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result = messageReader.ReadString();

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void ReadMultipleInt32()
    {
        // Arrange
        var bytes = new byte[]
        {
            1, 0, 0, 0, // 1
            2, 0, 0, 0 // 2
        };

        var messageReader =new MessageReader(bytes, LittleEndianProtocol.Utf8);

        // Act
        var result1 = messageReader.ReadInt32();
        var result2 = messageReader.ReadInt32();

        // Assert
        Assert.Equal(1, result1);
        Assert.Equal(2, result2);
    }

}
