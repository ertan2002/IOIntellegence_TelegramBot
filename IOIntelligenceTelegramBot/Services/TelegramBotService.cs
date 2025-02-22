using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using IoIntelligence.Client.Services;

namespace IOIntelligenceTelegramBot.Services
{
    public class TelegramBotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly CustomUpdateHandler _updateHandler;

        public TelegramBotService(string botToken, string apiKey)
        {
            _botClient = new TelegramBotClient(botToken);
            var apiClient = new IoIntelligenceClient(apiKey);            
            _updateHandler = new CustomUpdateHandler(apiClient);
        }

        public void Start(
            ReceiverOptions? receiverOptions = null,
            CancellationToken cancellationToken = default)
        {
            var options = receiverOptions ?? new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(),
                DropPendingUpdates = true,
            };

            _botClient.StartReceiving(
                _updateHandler,
                options,
                cancellationToken);
        }
    }   
}
