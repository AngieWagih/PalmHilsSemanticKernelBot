namespace PalmHilsSemanticKernelBot.Controllers.Response
{
    public class BotResponse
    {
        public BotResponse(string message, BotStatus botStatus, DateTime timestamp)
        {
            Message = message;
            BotStatus = botStatus;
            Timestamp = timestamp;
        }

        public string Message { get; set; } = string.Empty;
        public BotStatus BotStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum BotStatus
    {
        Running,
        Stopped,
        Error
    }
}
