//HintName: Message.cs
namespace Tests
{
    public partial class IncomingMessage : global::FluentSocket.IMessage
    {
        public void Write(ref global::FluentSocket.MessageWriter writer)
        {
            writer.WriteInt32(this.Id);
            global::Tests.IIncomingMessage.Write(ref writer, Message);
        }

        public void Read(ref global::FluentSocket.MessageReader reader)
        {
            this.Id = reader.ReadInt32();
            Message = global::Tests.IIncomingMessage.Read(ref reader);
        }
    }
}

namespace Tests
{
    partial interface IIncomingMessage : global::FluentSocket.IUnionMessage<global::Tests.IIncomingMessage>
    {
#if NET7_0_OR_GREATER
        static void global::FluentSocket.IUnionMessage<global::Tests.IIncomingMessage>.Write(ref global::FluentSocket.MessageWriter writer, global::Tests.IIncomingMessage value) => IIncomingMessageExtensions.WriteUnion(ref writer, value);
        static global::Tests.IIncomingMessage global::FluentSocket.IUnionMessage<global::Tests.IIncomingMessage>.Read(ref global::FluentSocket.MessageReader reader) => IIncomingMessageExtensions.ReadUnion<global::Tests.IIncomingMessage>(ref reader);
#endif
    }

    public partial interface IIncomingMessageHandler
    {
        void HandleEchoMessage(global::Tests.EchoMessage message);
    }

    public partial interface IAsyncIncomingMessageHandler
    {
        global::System.Threading.Tasks.ValueTask HandleEchoMessageAsync(global::Tests.EchoMessage message);
    }
}

namespace FluentSocket
{
    public static partial class IIncomingMessageExtensions
    {
        public static void WriteUnion(ref this global::FluentSocket.MessageWriter writer, global::Tests.IIncomingMessage value)
        {
            switch (value)
            {
                case global::Tests.EchoMessage v:
                    writer.WriteByte(0);
                    writer.WriteObject(v);
                    break;
            }
        }

        public static global::Tests.IIncomingMessage ReadUnion<T>(ref this global::FluentSocket.MessageReader reader)
            where T : global::Tests.IIncomingMessage
        {
            switch (reader.ReadByte())
            {
                case 0:
                    return reader.ReadObject<global::Tests.EchoMessage>();
                default:
                    throw new global::System.ArgumentException("Invalid union type.");
            }
        }

        public static void Handle<THandler>(this THandler handler, ref global::FluentSocket.MessageReader reader)
            where THandler : global::Tests.IIncomingMessageHandler
        {
            switch (reader.ReadByte())
            {
                case 0:
                    handler.HandleEchoMessage(reader.ReadObject<global::Tests.EchoMessage>());
                    break;
                default:
                    throw new global::System.ArgumentException("Invalid union type.");
            }
        }

        public static void Handle<THandler>(this THandler handler, global::Tests.IIncomingMessage value)
            where THandler : global::Tests.IIncomingMessageHandler
        {
            switch (value)
            {
                case global::Tests.EchoMessage v:
                    handler.HandleEchoMessage(v);
                    break;
                default:
                    throw new global::System.ArgumentException("Invalid union type.");
            }
        }

        public static global::System.Threading.Tasks.ValueTask HandleAsync<THandler>(this THandler handler, global::Tests.IIncomingMessage value)
            where THandler : global::Tests.IAsyncIncomingMessageHandler
        {
            switch (value)
            {
                case global::Tests.EchoMessage v:
                    return handler.HandleEchoMessageAsync(v);
                default:
                    throw new global::System.ArgumentException("Invalid union type.");
            }
        }
    }
}

namespace Tests
{
    public partial class EchoMessage : global::FluentSocket.IMessage
    {
        public void Write(ref global::FluentSocket.MessageWriter writer)
        {
            writer.WriteString(this.Text);
        }

        public void Read(ref global::FluentSocket.MessageReader reader)
        {
            this.Text = reader.ReadString();
        }
    }
}
