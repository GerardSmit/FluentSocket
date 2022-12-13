using System;
using System.Buffers.Binary;
using System.Text;

namespace FluentSocket.Protocol;

public class BigEndianProtocol : IMessageProtocol
{
    public static BigEndianProtocol Utf8 { get; } = new(Encoding.UTF8);

    private readonly Encoding _encoding;

    public BigEndianProtocol(Encoding encoding)
    {
        _encoding = encoding;
    }

    public byte ReadByte(ReadOnlySpan<byte> source, out int size)
    {
        size = 1;
        return source[0];
    }

    public int WriteByte(Span<byte> destination, byte value)
    {
        destination[0] = value;
        return 1;
    }

    public int ReadBytes(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        source.Slice(0, destination.Length).CopyTo(destination);
        return destination.Length;
    }

    public int WriteBytes(Span<byte> destination, ReadOnlySpan<byte> source)
    {
        source.CopyTo(destination);
        return source.Length;
    }

    public byte ReadUInt8(ReadOnlySpan<byte> source, out int size)
    {
        size = 1;
        return source[0];
    }

    public int WriteUInt8(Span<byte> destination, byte value)
    {
        destination[0] = value;
        return 1;
    }

    public short ReadInt16(ReadOnlySpan<byte> source, out int size)
    {
        size = 2;
        return BinaryPrimitives.ReadInt16BigEndian(source);
    }

    public int WriteInt16(Span<byte> destination, short value)
    {
        BinaryPrimitives.WriteInt16BigEndian(destination, value);
        return 2;
    }

    public ushort ReadUInt16(ReadOnlySpan<byte> source, out int size)
    {
        size = 2;
        return BinaryPrimitives.ReadUInt16BigEndian(source);
    }

    public int WriteUInt16(Span<byte> destination, ushort value)
    {
        BinaryPrimitives.WriteUInt16BigEndian(destination, value);
        return 2;
    }

    public int ReadInt32(ReadOnlySpan<byte> source, out int size)
    {
        size = 4;
        return BinaryPrimitives.ReadInt32BigEndian(source);
    }

    public int WriteInt32(Span<byte> destination, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(destination, value);
        return 4;
    }

    public uint ReadUInt32(ReadOnlySpan<byte> source, out int size)
    {
        size = 4;
        return BinaryPrimitives.ReadUInt32BigEndian(source);
    }

    public int WriteUInt32(Span<byte> destination, uint value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(destination, value);
        return 4;
    }

    public long ReadInt64(ReadOnlySpan<byte> source, out int size)
    {
        size = 8;
        return BinaryPrimitives.ReadInt64BigEndian(source);
    }

    public int WriteInt64(Span<byte> destination, long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(destination, value);
        return 8;
    }

    public ulong ReadUInt64(ReadOnlySpan<byte> source, out int size)
    {
        size = 8;
        return BinaryPrimitives.ReadUInt64BigEndian(source);
    }

    public int WriteUInt64(Span<byte> destination, ulong value)
    {
        BinaryPrimitives.WriteUInt64BigEndian(destination, value);
        return 8;
    }

    public float ReadSingle(ReadOnlySpan<byte> source, out int size)
    {
        size = 4;
        var intValue = BinaryPrimitives.ReadInt32BigEndian(source);
        return System.Runtime.CompilerServices.Unsafe.As<int, float>(ref intValue);
    }

    public int WriteSingle(Span<byte> destination, float value)
    {
        var intValue = System.Runtime.CompilerServices.Unsafe.As<float, int>(ref value);
        BinaryPrimitives.WriteInt32BigEndian(destination, intValue);
        return 4;
    }

    public double ReadDouble(ReadOnlySpan<byte> source, out int size)
    {
        size = 8;
        var longValue = BinaryPrimitives.ReadInt64BigEndian(source);
        return System.Runtime.CompilerServices.Unsafe.As<long, double>(ref longValue);
    }

    public int WriteDouble(Span<byte> destination, double value)
    {
        var longValue = System.Runtime.CompilerServices.Unsafe.As<double, long>(ref value);
        BinaryPrimitives.WriteInt64BigEndian(destination, longValue);
        return 8;
    }

    public string ReadString(ReadOnlySpan<byte> source, out int size)
    {
        var stringLength = BinaryPrimitives.ReadInt32BigEndian(source);
        var value = _encoding.GetString(source.Slice(4, stringLength));
        size = stringLength + 4;
        return value;
    }

    public int WriteString(Span<byte> destination, string value)
    {
        var stringLength = Encoding.UTF8.GetBytes(value.AsSpan(), destination.Slice(4));
        BinaryPrimitives.WriteInt32BigEndian(destination, stringLength);
        return stringLength + 4;
    }

    public bool ReadBoolean(ReadOnlySpan<byte> source, out int size)
    {
        size = 1;
        return source[0] != 0;
    }

    public int WriteBoolean(Span<byte> destination, bool value)
    {
        destination[0] = (byte)(value ? 1 : 0);
        return 1;
    }
}
