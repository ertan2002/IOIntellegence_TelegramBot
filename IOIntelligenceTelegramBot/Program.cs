using IOIntelligenceTelegramBot.Services;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

Console.WriteLine("Hello, World!");

string botToken = "YOUR_TELEGRAM_BOT_TOKEN"; // Get your bot token from BotFather.
string apiKey = "YOUR_IO_INTELLEGENCE_API_TOKEN"; // Get your API key from https://ai.io.net/ai/api-keys


var botService = new TelegramBotService(botToken, apiKey);

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = new[] { UpdateType.Message },
    DropPendingUpdates = true
};

var cancellationToken = new CancellationTokenSource().Token;
botService.Start(receiverOptions, cancellationToken);


Console.WriteLine("Bot started. Press any key to exit...");
Console.ReadKey();
