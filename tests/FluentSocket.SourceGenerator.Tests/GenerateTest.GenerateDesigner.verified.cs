//HintName: Message.cs
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
