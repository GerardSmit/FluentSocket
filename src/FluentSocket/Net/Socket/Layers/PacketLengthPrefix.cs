using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Threading.Tasks;

namespace FluentSocket;

public enum LengthType
{
    UInt8,

    UInt16LittleEndian,
    UInt16BigEndian,
    UInt32LittleEndian,
    UInt32BigEndian,
    UInt64LittleEndian,
    UInt64BigEndian,

    Int16LittleEndian,
    Int16BigEndian,
    Int32LittleEndian,
    Int32BigEndian
}

public class PacketLengthPrefix : Layer
{
    private struct PendingBuffer
    {
        public IMemoryOwner<byte> Buffer;
        public int ReceivedLength;
        public int PacketLength;

        public PendingBuffer(IMemoryOwner<byte> buffer, int receivedLength, int packetLength)
        {
            Buffer = buffer;
            ReceivedLength = receivedLength;
            PacketLength = packetLength;
        }

        public void Deconstruct(out IMemoryOwner<byte> buffer, out int receivedLength, out int packetLength)
        {
            buffer = Buffer;
            receivedLength = ReceivedLength;
            packetLength = PacketLength;
        }
    }

    private readonly LengthType _lengthType;
    private readonly int _lengthSize;
    private readonly MemoryPool<byte> _memoryPool;

    private PendingBuffer? _pendingBuffer;

    public PacketLengthPrefix(LengthType lengthType, MemoryPool<byte> memoryPool = null)
    {
        _lengthType = lengthType;
        _memoryPool = memoryPool ?? MemoryPool<byte>.Shared;
        _lengthSize = GetLength(lengthType);
    }

    public override async ValueTask SendAsync(SendDelegate next, Memory<byte> data)
    {
        using var owner = _memoryPool.Rent(data.Length + _lengthSize);
        var buffer = owner.Memory;

        WritePacketLength(buffer.Span, data.Length);
        data.CopyTo(buffer.Slice(_lengthSize));

        await next(buffer.Slice(0, data.Length + _lengthSize));
    }

    public override async ValueTask ReceiveAsync(ReceiveDelegate next, Memory<byte> data)
    {
        // Handle pending data
        if (_pendingBuffer.HasValue)
        {
            var (owner, receivedLength, packetLength) = _pendingBuffer.Value;

            var remaining = packetLength - receivedLength;
            var copyLength = Math.Min(remaining, data.Length);

            data.Slice(0, copyLength).CopyTo(owner.Memory.Slice(receivedLength));

            receivedLength += copyLength;

            if (receivedLength == packetLength)
            {
                try
                {
                    await next(owner.Memory.Slice(0, packetLength));
                }
                finally
                {
                    owner.Dispose();
                    _pendingBuffer = null;
                }

                data = data.Slice(copyLength);
            }
            else
            {
                _pendingBuffer = new PendingBuffer(owner, receivedLength, packetLength);
                return;
            }
        }

        // Parse packet
        while (data.Length > 0)
        {
            var packetSize = ReadPacketLength(data.Span);
            data = data.Slice(_lengthSize);

            if (data.Length < packetSize)
            {
                var owner = _memoryPool.Rent(packetSize);
                data.CopyTo(owner.Memory);

                _pendingBuffer = new PendingBuffer(owner, data.Length, packetSize);
                return;
            }

            await next(data.Slice(0, packetSize));

            data = data.Slice(packetSize);
        }
    }

    private static int GetLength(LengthType lengthType)
    {
        return lengthType switch
        {
            LengthType.UInt8 => 1,
            LengthType.UInt16LittleEndian => 2,
            LengthType.UInt16BigEndian => 2,
            LengthType.UInt32LittleEndian => 4,
            LengthType.UInt32BigEndian => 4,
            LengthType.UInt64LittleEndian => 8,
            LengthType.UInt64BigEndian => 8,
            LengthType.Int16LittleEndian => 2,
            LengthType.Int16BigEndian => 2,
            LengthType.Int32LittleEndian => 4,
            LengthType.Int32BigEndian => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(lengthType), lengthType, null)
        };
    }

    private void WritePacketLength(Span<byte> buffer, int length)
    {
        switch (_lengthType)
        {
            case LengthType.UInt8:
                buffer[0] = (byte)length;
                break;
            case LengthType.UInt16LittleEndian:
                BinaryPrimitives.WriteUInt16LittleEndian(buffer, (ushort)length);
                break;
            case LengthType.UInt16BigEndian:
                BinaryPrimitives.WriteUInt16BigEndian(buffer, (ushort)length);
                break;
            case LengthType.UInt32LittleEndian:
                BinaryPrimitives.WriteUInt32LittleEndian(buffer, (uint)length);
                break;
            case LengthType.UInt32BigEndian:
                BinaryPrimitives.WriteUInt32BigEndian(buffer, (uint)length);
                break;
            case LengthType.UInt64LittleEndian:
                BinaryPrimitives.WriteUInt64LittleEndian(buffer, (ulong)length);
                break;
            case LengthType.UInt64BigEndian:
                BinaryPrimitives.WriteUInt64BigEndian(buffer, (ulong)length);
                break;
            case LengthType.Int16LittleEndian:
                BinaryPrimitives.WriteInt16LittleEndian(buffer, (short)length);
                break;
            case LengthType.Int16BigEndian:
                BinaryPrimitives.WriteInt16BigEndian(buffer, (short)length);
                break;
            case LengthType.Int32LittleEndian:
                BinaryPrimitives.WriteInt32LittleEndian(buffer, length);
                break;
            case LengthType.Int32BigEndian:
                BinaryPrimitives.WriteInt32BigEndian(buffer, length);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private int ReadPacketLength(ReadOnlySpan<byte> buffer)
    {
        return _lengthType switch
        {
            LengthType.UInt8 => buffer[0],
            LengthType.UInt16LittleEndian => BinaryPrimitives.ReadUInt16LittleEndian(buffer),
            LengthType.UInt16BigEndian => BinaryPrimitives.ReadUInt16BigEndian(buffer),
            LengthType.UInt32LittleEndian => (int)BinaryPrimitives.ReadUInt32LittleEndian(buffer),
            LengthType.UInt32BigEndian => (int)BinaryPrimitives.ReadUInt32BigEndian(buffer),
            LengthType.UInt64LittleEndian => (int)BinaryPrimitives.ReadUInt64LittleEndian(buffer),
            LengthType.UInt64BigEndian => (int)BinaryPrimitives.ReadUInt64BigEndian(buffer),
            LengthType.Int16LittleEndian => BinaryPrimitives.ReadInt16LittleEndian(buffer),
            LengthType.Int16BigEndian => BinaryPrimitives.ReadInt16BigEndian(buffer),
            LengthType.Int32LittleEndian => BinaryPrimitives.ReadInt32LittleEndian(buffer),
            LengthType.Int32BigEndian => BinaryPrimitives.ReadInt32BigEndian(buffer),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
