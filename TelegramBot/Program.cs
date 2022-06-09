namespace TelegramBot;
using Telegram.Bot;
class TelegramBot
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine($"Usage:{Environment.NewLine}    {Environment.GetCommandLineArgs()[0]} <bot token>");
            return;
        }
        string botToken = args[0];

        var botClient = new TelegramBotClient(botToken);
        int update_id = 0;
        Telegram.Bot.Types.Update[] updates;

        while (true)
        {
            try
            {
                updates = await botClient.GetUpdatesAsync(offset: update_id + 1);
            }
            catch (TimeoutException) { continue; };
            Console.WriteLine("Got updates");
            foreach (var update in updates)
            {
                update_id = update.Id;
                var message = update.Message;
                var text = message?.Text;
                var user = message?.From;
                var chat_id = user?.Id;
                if (chat_id is not null && text is not null)
                {
                    try
                    {
                        await botClient.SendTextMessageAsync(chat_id, text);
                    }
                    catch (TimeoutException) { continue; }
                    Console.WriteLine(text);
                }

            }
            await Task.Delay(new TimeSpan(0, 1, 0));
        }
    }
}