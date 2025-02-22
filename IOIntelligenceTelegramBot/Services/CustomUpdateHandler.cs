using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Text.RegularExpressions;
using IoIntelligence.Client.Interfaces;
using IoIntelligence.Client.Models.AIModel.Chat;

namespace IOIntelligenceTelegramBot.Services
{
    public class CustomUpdateHandler : IUpdateHandler
    {
        private readonly IIoIntelligenceClient _apiClient;
        private readonly Dictionary<long, string> _userModels = new();
        private readonly Dictionary<long, List<string>> _cachedModelLists = new();


        public CustomUpdateHandler(IIoIntelligenceClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message || message.Text is null)
                return;

            try
            {
                switch (message.Text.Split(' ')[0].ToLower())
                {
                    case "/start":
                        await SendWelcomeMessage(botClient, message.Chat.Id, cancellationToken);
                        break;

                    case "/models":
                        await ListModels(botClient, message.Chat.Id, cancellationToken);
                        break;

                    case "/setmodel" when message.Text.Length > 10:
                        await SetModel(botClient, message, cancellationToken);
                        break;

                    default:
                        await HandleChatMessage(botClient, message, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    $"Error: {ex.Message}",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task SendWelcomeMessage(ITelegramBotClient botClient, long chatId, CancellationToken ct)
        {
            var text = "Welcome to IO Intelligence Bot!\n" +
                       "Available commands:\n" +
                       "/models - List available models\n" +
                       "/setmodel [number] - Set model using number from /models list";

            await botClient.SendMessage(chatId, text, cancellationToken: ct);
        }


        private async Task ListModels(ITelegramBotClient botClient, long chatId, CancellationToken ct)
        {
            var models = (await _apiClient.Models.GetModelsAsync()).ToList();
            _cachedModelLists[chatId] = models.Select(m => m.Id).ToList();

            var modelList = string.Join("\n", models.Select((m, i) => $"{i + 1}. {m.Id}"));
            await botClient.SendMessage(
                chatId,
                $"Available models:\n{modelList}\n\nUse /setmodel [number] to select",
                cancellationToken: ct);
        }


        private async Task SetModel(ITelegramBotClient botClient, Message message, CancellationToken ct)
        {
            var parts = message.Text.Split(' ');
            if (parts.Length < 2 || !int.TryParse(parts[1], out var modelNumber))
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    "Invalid format. Use /setmodel [number] where number is from /models list",
                    cancellationToken: ct);
                return;
            }
                        
            if (!_cachedModelLists.TryGetValue(message.Chat.Id, out var models))
            {
                models = (await _apiClient.Models.GetModelsAsync()).Select(m => m.Id).ToList();
            }

            var index = modelNumber - 1;
            if (index < 0 || index >= models.Count)
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    $"Invalid model number. Please choose between 1-{models.Count}",
                    cancellationToken: ct);
                return;
            }

            _userModels[message.Chat.Id] = models[index];
            await botClient.SendMessage(
                message.Chat.Id,
                $"Model set to: {models[index]}",
                cancellationToken: ct);
        }


        private async Task HandleChatMessage(ITelegramBotClient botClient, Message message, CancellationToken ct)
        {    
            if (!_userModels.TryGetValue(message.Chat.Id, out var modelId))
            {
                await botClient.SendMessage(
                    message.Chat.Id,
                    "Please select a model first using /setmodel [number]",
                    cancellationToken: ct);
                return;
            }

            var request = new ChatCompletionRequest
            {
                Model = modelId,
                Messages = new List<ChatCompletionMessage>
                    {
                        new() { Role = "system", Content = "You are a helpful assistant." },
                        new() { Role = "user", Content = message.Text }
                    }
            };

            var response = await _apiClient.Models.CreateChatCompletionAsync(request);            
            var rawReply = response.Choices.First().Message.Content;
            var cleanReply = SanitizeResponse(rawReply);


            await botClient.SendMessage(
                message.Chat.Id,
                cleanReply,
                cancellationToken: ct);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception}");
            return Task.CompletedTask;
        }

        private string SanitizeResponse(string response)
        {
            // Remove all <think> blocks and their content
            var cleaned = Regex.Replace(response, @"<think>[\s\S]*?</think>", "", RegexOptions.Multiline);

            // Trim whitespace and special characters from start/end
            cleaned = Regex.Replace(cleaned, @"^[\s\u200B\p{C}]+|[\s\u200B\p{C}]+$", "");

            return cleaned.Trim();
        }

    }

}
