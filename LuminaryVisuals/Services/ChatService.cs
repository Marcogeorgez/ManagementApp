namespace LuminaryVisuals.Services
{
    public class ChatService
    {
        public List<string> Messages { get; set; } = new List<string>();

        public void AddMessage(string message)
        {
            Messages.Add(message);
        }
    }

}
