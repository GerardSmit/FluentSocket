using System;
using FluentSocket.Protocol;
using Xunit;

namespace FluentSocket.Tests;

public class MessageWriterTest
{
    [Fact]
    public void TestInt64()
    {
        // Arrange
        var bytes = new byte[8];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteInt64(1);

        // Assert
        Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, bytes);
    }

    [Fact]
    public void TestUInt64()
    {
        // Arrange
        var bytes = new byte[8];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteUInt64(1);

        // Assert
        Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, bytes);
    }

    [Fact]
    public void TestInt32()
    {
        // Arrange
        var bytes = new byte[4];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteInt32(1);

        // Assert
        Assert.Equal(new byte[] { 1, 0, 0, 0 }, bytes);
    }

    [Fact]
    public void TestUInt32()
    {
        // Arrange
        var bytes = new byte[4];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteUInt32(1);

        // Assert
        Assert.Equal(new byte[] { 1, 0, 0, 0 }, bytes);
    }

    [Fact]
    public void TestInt16()
    {
        // Arrange
        var bytes = new byte[2];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteInt16(1);

        // Assert
        Assert.Equal(new byte[] { 1, 0 }, bytes);
    }

    [Fact]
    public void TestUInt16()
    {
        // Arrange
        var bytes = new byte[2];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteUInt16(1);

        // Assert
        Assert.Equal(new byte[] { 1, 0 }, bytes);
    }

    [Fact]
    public void TestByte()
    {
        // Arrange
        var bytes = new byte[1];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteByte(1);

        // Assert
        Assert.Equal(new byte[] { 1 }, bytes);
    }

    [Fact]
    public void TestSingle()
    {
        // Arrange
        var bytes = new byte[4];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteSingle(1);

        // Assert
        Assert.Equal(new byte[] { 0, 0, 128, 63 }, bytes);
    }

    [Fact]
    public void TestDouble()
    {
        // Arrange
        var bytes = new byte[8];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteDouble(1);

        // Assert
        Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 0, 240, 63 }, bytes);
    }

    [Fact]
    public void TestString()
    {
        // Arrange
        var bytes = new byte[8];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteString("test");

        // Assert
        Assert.Equal(new byte[]
        {
            4, 0, 0, 0, // Length
            116, 101, 115, 116 // "test"
        }, bytes);
    }

    [Fact]
    public void WriteMultipleInt32()
    {
        // Arrange
        var bytes = new byte[8];

        var messageWriter =new MessageWriter(bytes, LittleEndianProtocol.Utf8);

        // Act
        messageWriter.WriteInt32(1);
        messageWriter.WriteInt32(2);

        // Assert
        Assert.Equal(new byte[]
        {
            1, 0, 0, 0, // 1
            2, 0, 0, 0 // 2
        }, bytes);
    }

}
