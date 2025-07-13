// Models/ChatMessage.cs
namespace VoaDownloaderWpf.Models
{
    public enum MessageSender { User, AI, System }

    public class ChatMessage
    {
        public MessageSender Sender { get; set; }
        public string Message { get; set; }
    }
}