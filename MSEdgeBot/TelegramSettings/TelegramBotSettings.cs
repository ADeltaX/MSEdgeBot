namespace MSEdgeBot
{
    public static class TelegramBotSettings
    {
        public const string BOT_VERSION = "EdgeBot 1.1";
        public static string DATABASE_FILENAME = "DataStore" + System.IO.Path.DirectorySeparatorChar + "EdgeUpdatesDB.sqlite";
        public static string[] DIRS_TO_INITALIZE = { "DataStore", "dw", "tdlib" };

        public const string DEV_CHANNEL = SecretKeys.DEV_CHANNEL;
        public const string PROD_CHANNEL = SecretKeys.PROD_CHANNEL;

        public const string DEV_KEY = SecretKeys.DEV_KEY;
        public const string PROD_KEY = SecretKeys.PROD_KEY;

        public const string API_HASH = SecretKeys.API_HASH;
        public const int API_ID = SecretKeys.API_ID;
    }
}
