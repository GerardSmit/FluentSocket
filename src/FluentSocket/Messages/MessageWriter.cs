using System;
using System.Buffers.Binary;
using System.Text;
using FluentSocket.Protocol;

namespace FluentSocket;

public ref struct MessageWriter
{
    private readonly IMessageProtocol _protocol;
    private Span<byte> _buffer;

    public MessageWriter(Span<byte> buffer, IMessageProtocol protocol)
    {
        _buffer = buffer;
        _protocol = protocol;
    }

    public void WriteByte(byte value)
    {
        var bytesWritten = _protocol.WriteByte(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteBytes(ReadOnlySpan<byte> value)
    {
        var bytesWritten = _protocol.WriteBytes(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteInt16(short value)
    {
        var bytesWritten = _protocol.WriteInt16(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteUInt16(ushort value)
    {
        var bytesWritten = _protocol.WriteUInt16(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteInt32(int value)
    {
        var bytesWritten = _protocol.WriteInt32(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteUInt32(uint value)
    {
        var bytesWritten = _protocol.WriteUInt32(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteInt64(long value)
    {
        var bytesWritten = _protocol.WriteInt64(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteUInt64(ulong value)
    {
        var bytesWritten = _protocol.WriteUInt64(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteSingle(float value)
    {
        var bytesWritten = _protocol.WriteSingle(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteDouble(double value)
    {
        var bytesWritten = _protocol.WriteDouble(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteString(string value)
    {
        var bytesWritten = _protocol.WriteString(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteBoolean(bool value)
    {
        var bytesWritten = _protocol.WriteBoolean(_buffer, value);
        _buffer = _buffer.Slice(bytesWritten);
    }

    public void WriteObject<T>(T value)
        where T : IMessage
    {
        value.Write(ref this);
    }

#if NET7_0_OR_GREATER
    public void WriteUnion<T>(T value)
        where T : IUnionMessage<T>
    {
        T.Write(ref this, value);
    }
#endif
}
