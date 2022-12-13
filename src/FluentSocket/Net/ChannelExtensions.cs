using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;

namespace FluentSocket;

public static class ChannelExtensions
{
    private static readonly ObjectPool<PacketOwner> PacketPool = new DefaultObjectPool<PacketOwner>(
        new PacketOwnerPolicy(),
        maximumRetained: 1000);

    public static async ValueTask ReceivePacketsAsync(
        this SocketSourceBase socket,
        ChannelWriter<Packet> channel,
        MemoryPool<byte> memoryPool = null,
        int bufferSize = 4096,
        CancellationToken cancellationToken = default)
    {
        await foreach (var packet in ReceivePacketsAsync(socket, memoryPool, bufferSize, cancellationToken))
        {
            await channel.WriteAsync(packet, cancellationToken);
        }
    }

    public static async IAsyncEnumerable<Packet> ReceivePacketsAsync(
        this SocketSourceBase socket,
        MemoryPool<byte> memoryPool = null,
        int bufferSize = 4096,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        memoryPool ??= MemoryPool<byte>.Shared;

        var owner = memoryPool.Rent(bufferSize);
        var buffer = owner.Memory;

        var length = await socket.ReceiveAsync(buffer, cancellationToken);

        if (length == 0)
        {
            owner.Dispose();
            yield break;
        }

        var received = buffer.Slice(0, length);

        var packetOwner = PacketPool.Get();
        packetOwner.Initialize(owner);

        try
        {
            while (received.Length > 0 && !cancellationToken.IsCancellationRequested)
            {
                var packetSize = BinaryPrimitives.ReadUInt16LittleEndian(received.Span.Slice(0, 2));
                received = received.Slice(2);

                if (received.Length < packetSize)
                {
                    packetOwner.EnableOwnerDisposing();
                    yield return await ReceiveLargePacketAsync(memoryPool, socket, packetSize, received, cancellationToken);
                    yield break;
                }

                var packet = new Packet(
                    received.Slice(0, packetSize),
                    packetOwner
                );

                packetOwner.IncrementReceived();
                received = received.Slice(packetSize);

                yield return packet;
            }
        }
        finally
        {
            packetOwner.EnableOwnerDisposing();
        }
    }

    private static async ValueTask<Packet> ReceiveLargePacketAsync(
        MemoryPool<byte> memoryPool,
        SocketSourceBase socket,
        ushort packetSize,
        ReadOnlyMemory<byte> received,
        CancellationToken cancellationToken)
    {
        var owner = memoryPool.Rent(packetSize);
        var buffer = owner.Memory.Slice(0, packetSize);

        received.CopyTo(buffer);

        await socket.ReceiveAsync(buffer.Slice(received.Length), cancellationToken);

        return new Packet(buffer, owner);
    }

    private class PacketOwnerPolicy : IPooledObjectPolicy<PacketOwner>
    {
        public PacketOwner Create()
        {
            return new PacketOwner(PacketPool);
        }

        public bool Return(PacketOwner obj)
        {
            obj.ClearReferences();
            return true;
        }
    }

    /// <summary>
    /// Disposes the <see cref="IMemoryOwner{T}"/> after all packets have been handled.
    /// </summary>
    private sealed class PacketOwner : IDisposable
    {
        private readonly ObjectPool<PacketOwner> _pool;
        private readonly object _lock = new();

        private IMemoryOwner<byte> _memoryOwner;
        private int _received;
        private int _handled;
        private bool _disposeOwnerOnIncrement;
        private bool _isDisposed;

        public PacketOwner(ObjectPool<PacketOwner> pool)
        {
            _pool = pool;
        }

        public void Initialize(IMemoryOwner<byte> memoryOwner)
        {
            Reset();
            _memoryOwner = memoryOwner;
        }

        public void Reset()
        {
            _memoryOwner = null;
            _received = 0;
            _handled = 0;
            _disposeOwnerOnIncrement = false;
            _isDisposed = false;
        }

        internal void ClearReferences()
        {
            _memoryOwner = null;
        }

        /// <summary>
        /// Called when a packet has been received.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void IncrementReceived()
        {
            if (_disposeOwnerOnIncrement)
            {
                throw new InvalidOperationException("Cannot increment received after dispose has been enabled.");
            }

            lock (_lock)
            {
                _received++;
            }
        }

        /// <summary>
        /// Called when the packet has been handled.
        /// </summary>
        private void IncrementHandled()
        {
            var shouldDispose = false;

            lock (_lock)
            {
                _handled++;

                if (_disposeOwnerOnIncrement && _received == _handled)
                {
                    shouldDispose = true;
                }
            }

            if (shouldDispose)
            {
                DisposeOwner();
            }
        }

        /// <summary>
        /// Called when the socket is done receiving packets.
        /// </summary>
        public void EnableOwnerDisposing()
        {
            var shouldDispose = false;

            lock (_lock)
            {
                _disposeOwnerOnIncrement = true;

                if (_received == _handled)
                {
                    shouldDispose = true;
                }
            }

            if (shouldDispose)
            {
                DisposeOwner();
            }
        }

        private void DisposeOwner()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _memoryOwner.Dispose();
            _pool.Return(this);
        }

        public void Dispose() => IncrementHandled();
    }
}
