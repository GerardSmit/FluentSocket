using System;
using System.Buffers.Binary;
using System.Text;
using FluentSocket.Protocol;

namespace FluentSocket;

public ref struct MessageReader
{
    private readonly IMessageProtocol _protocol;
    private ReadOnlySpan<byte> _buffer;

    public MessageReader(ReadOnlySpan<byte> buffer, IMessageProtocol protocol)
    {
        _buffer = buffer;
        _protocol = protocol;
    }

    public byte ReadByte()
    {
        var value = _protocol.ReadByte(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public void ReadBytes(Span<byte> destination)
    {
         var bytesConsumed = _protocol.ReadBytes(_buffer, destination);
            _buffer = _buffer.Slice(bytesConsumed);
    }

    public short ReadInt16()
    {
        var value = _protocol.ReadInt16(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public ushort ReadUInt16()
    {
        var value = _protocol.ReadUInt16(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public int ReadInt32()
    {
        var value = _protocol.ReadInt32(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public uint ReadUInt32()
    {
        var value = _protocol.ReadUInt32(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public long ReadInt64()
    {
        var value = _protocol.ReadInt64(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public ulong ReadUInt64()
    {
        var value = _protocol.ReadUInt64(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public float ReadSingle()
    {
        var value = _protocol.ReadSingle(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public double ReadDouble()
    {
        var value = _protocol.ReadDouble(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public string ReadString()
    {
        var value = _protocol.ReadString(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public bool ReadBoolean()
    {
        var value = _protocol.ReadBoolean(_buffer, out var bytesConsumed);
        _buffer = _buffer.Slice(bytesConsumed);
        return value;
    }

    public T ReadObject<T>()
        where T : IMessage, new()
    {
        var obj = new T();
        obj.Read(ref this);
        return obj;
    }

#if NET7_0_OR_GREATER
    public T ReadUnion<T>(T value)
        where T : IUnionMessage<T>
    {
        return T.Read(ref this);
    }
#endif
}
