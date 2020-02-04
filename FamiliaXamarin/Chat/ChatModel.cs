namespace Familia.Chat
{
    public class ChatModel
    {

        public static readonly int TypeMessage;
        public static readonly int TypeLog = 1;
        public static readonly int TypeAction = 2;
        public static readonly int TypeMyMessage = 3;

        public int Type { get; set; }
    
        public string Message { get; set; }

    }
}
