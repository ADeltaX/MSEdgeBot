# MSEdgeBot
The bot that empowers the Telegram Channel https://t.me/MSEdgeUpdates, written in C# (.NET Core 3.1), powered by TDLib

## How to build
Create a new Class file named "SecretKeys.cs" and put it in MSEdgeBot\TelegramSettings\

and copy this (and replace it with your values):
```cs
public static class SecretKeys
{
    public const string DEV_CHANNEL = "CHANNEL_NAME";
    public const string PROD_CHANNEL = "CHANNEL_NAME";

    public const string DEV_KEY = "TELEGRAM_BOT_TOKEN";
    public const string PROD_KEY = "TELEGRAM_BOT_TOKEN";

    public const string API_HASH = "TELEGRAM_MTPROTO_API_HASH";
    public const int API_ID = TELEGRAM_MTPROTO_API_ID;
}
```

...and now you can compile! RELEASE_PROD configuration will build as "production" meanwhile other configurations will build as "development"

Enjoy!
