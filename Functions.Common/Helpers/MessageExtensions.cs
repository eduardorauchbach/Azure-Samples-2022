namespace Functions.Common.Helpers
{
    public static class MessageExtensions
    {
        public static bool IsNullOrEmpty(this byte[] message)
        {
            return message is null || message.Length == 0;
        }
    }
}
