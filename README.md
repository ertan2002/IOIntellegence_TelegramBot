# 🤖 IO Intelligence Telegram Bot

## Prerequisites

1. **Telegram Bot Token**  
2. **IO Intelligence API Key**

---

## Configuration Guide

### Getting Your Telegram Bot Token

1. **Create a New Bot**  
   - Open Telegram and search for `@BotFather`  
   - Start a chat and send:  
     `/newbot`

2. **Configure Your Bot**  
   - Follow BotFather's prompts:  
     1. Choose a **display name** (e.g., "My AI Assistant")  
     2. Create a **username** ending with `_bot` (e.g., `MyAIAssistant_bot`)

3. **Retrieve Your Token**  
   - After creation, BotFather will provide an HTTP API token:  
     ```plaintext
     1234567890:ABCdefGhIJKlmNoPQRsTUVwxyZ-1234567890
     ```  
   - **Important:** Keep this token secure!

### Obtaining IO Intelligence API Key

1. **Access API Portal**  
   - Visit [IO Intelligence API Keys](https://ai.io.net/ai/api-keys)  
   - Log in or create an account

2. **Generate New Key**  
   1. Click **"Create New Secret Key"**  
   2. Fill in required details:  
      - Key name (e.g., "Telegram Bot Production")  
      - Expiration date (max 180 days)  
   3. Click **"Create Secret Key"**

3. **Secure Your Key**  
   - Copy the key immediately - it won't be shown again!  
   - Example key format:  
     ```plaintext
     sk-abc123DEF456ghi789JKL012mno345PQR678
     ```

---

## Quick Start

```csharp
// Program.cs
string botToken = "YOUR_TELEGRAM_BOT_TOKEN";  // From BotFather
string apiKey = "YOUR_IO_INTELLIGENCE_API_KEY";  // From https://ai.io.net/ai/api-keys

var botService = new TelegramBotService(botToken, apiKey);

var receiverOptions = new ReceiverOptions 
{
    AllowedUpdates = new[] { UpdateType.Message },
    DropPendingUpdates = true
};

var cancellationToken = new CancellationTokenSource().Token;
botService.Start(receiverOptions, cancellationToken);

Console.WriteLine("Bot started! Press any key to exit...");
Console.ReadKey();
