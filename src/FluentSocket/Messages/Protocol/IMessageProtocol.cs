using System;

namespace FluentSocket.Protocol;

public interface IMessageProtocol
{
    byte ReadByte(ReadOnlySpan<byte> source, out int size);

    int WriteByte(Span<byte> destination, byte value);

    int ReadBytes(ReadOnlySpan<byte> source, Span<byte> destination);

    int WriteBytes(Span<byte> destination, ReadOnlySpan<byte> source);

    byte ReadUInt8(ReadOnlySpan<byte> source, out int size);

    int WriteUInt8(Span<byte> destination, byte value);

    short ReadInt16(ReadOnlySpan<byte> source, out int size);

    int WriteInt16(Span<byte> destination, short value);

    ushort ReadUInt16(ReadOnlySpan<byte> source, out int size);

    int WriteUInt16(Span<byte> destination, ushort value);

    int ReadInt32(ReadOnlySpan<byte> source, out int size);

    int WriteInt32(Span<byte> destination, int value);

    uint ReadUInt32(ReadOnlySpan<byte> source, out int size);

    int WriteUInt32(Span<byte> destination, uint value);

    long ReadInt64(ReadOnlySpan<byte> source, out int size);

    int WriteInt64(Span<byte> destination, long value);

    ulong ReadUInt64(ReadOnlySpan<byte> source, out int size);

    int WriteUInt64(Span<byte> destination, ulong value);

    float ReadSingle(ReadOnlySpan<byte> source, out int size);

    int WriteSingle(Span<byte> destination, float value);

    double ReadDouble(ReadOnlySpan<byte> source, out int size);

    int WriteDouble(Span<byte> destination, double value);

    string ReadString(ReadOnlySpan<byte> source, out int size);

    int WriteString(Span<byte> destination, string value);

    bool ReadBoolean(ReadOnlySpan<byte> source, out int size);

    int WriteBoolean(Span<byte> destination, bool value);
}
