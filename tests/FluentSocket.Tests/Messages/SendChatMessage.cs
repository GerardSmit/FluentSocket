namespace FluentSocket.Tests.Messages;

public partial class SendChatMessage : IServerMessage
{
    public string Text { get; set; }
}
