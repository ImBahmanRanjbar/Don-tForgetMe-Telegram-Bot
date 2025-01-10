using Telegram.Bot;
using Telegram.Bot.Types;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static TelegramBotClient bot;
    private static ConcurrentDictionary<long, bool> activeUsers = new ConcurrentDictionary<long, bool>();
    private const string BOT_TOKEN = "......"; 
    private const string DAILY_MESSAGE = "I love you ❤️,"; 

    static async Task Main(string[] args)
    {
        bot = new TelegramBotClient(BOT_TOKEN);
        var me = await bot.GetMeAsync();
        Console.WriteLine($"Bot started successfully: @{me.Username}");

        StartDailyMessageTimer();
        bot.StartReceiving(UpdateHandler, ErrorHandler);

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }

    private static Task ErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Error occurred: {exception.Message}");
        return Task.CompletedTask;
    }

    private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message is not { } message || message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;
        Console.WriteLine($"Received message: {messageText} from chat id: {chatId}");

        switch (messageText.ToLower())
        {
            case "/start":
                activeUsers.TryAdd(chatId, true);
                await bot.SendTextMessageAsync(chatId, " The bot has started! Hello. Bahman will have a message for you.");
                await bot.SendTextMessageAsync(chatId, " . To stop the bot :/stop");
                Console.WriteLine("Sent /start confirmation message.");
                break;

            case "/stop":
                activeUsers[chatId] = false;
                await bot.SendTextMessageAsync(chatId, " It's like you don't want to hear what I'm saying. Goodbye for now.");
                Console.WriteLine("Sent /stop confirmation message.");
                break;
        }
    }






    private static void StartDailyMessageTimer()
    {
        var timer = new Timer(async _ => await SendDailyMessages(), null, CalculateTimeToNextDay(), TimeSpan.FromHours(24));
    }

    private static TimeSpan CalculateTimeToNextDay()
    {
        var now = DateTime.Now;
        var next = now.Date.AddDays(1).AddHours(9);
        if (next < now)
            next = next.AddDays(1);
        return next - now;
    }

    private static async Task SendDailyMessages()
    {
        foreach (var user in activeUsers)
        {
            if (user.Value)
            {
                try
                {
                    await bot.SendTextMessageAsync(user.Key, DAILY_MESSAGE);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to {user.Key}: {ex.Message}");
                }
            }
        }
    }
}
