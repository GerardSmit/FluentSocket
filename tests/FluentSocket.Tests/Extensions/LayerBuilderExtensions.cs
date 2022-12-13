using System;
using System.Threading.Tasks;

namespace FluentSocket.Tests.Extensions;

public static class LayerBuilderExtensions
{
    public static (SocketSourceBase, LayerResult) Test(this LayerBuilder builder)
    {
        var layer = new LayerResult();
        var socket = builder.AddLayer(layer).Build();

        return (socket, layer);
    }

    public class LayerResult : Layer
    {
        public Memory<byte> SendData { get; set; }

        public Memory<byte> ReceiveData { get; set; }

        public override ValueTask SendAsync(SendDelegate next, Memory<byte> data)
        {
            SendData = data;
            return next(data);
        }

        public override ValueTask ReceiveAsync(ReceiveDelegate next, Memory<byte> data)
        {
            ReceiveData = data;
            return next(data);
        }
    }
}
