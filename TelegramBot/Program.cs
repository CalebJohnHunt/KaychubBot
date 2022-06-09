namespace TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Types;
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
        Update[] updates;
        var random = new Random();

        Dictionary<long, int> userNumber = new();

        while (true)
        {
            // Min of 5 seconds between checks
            var delay = Task.Delay(new TimeSpan(0, 0, 1));
            try
            {
                updates = await botClient.GetUpdatesAsync(offset: update_id);
            }
            catch (TimeoutException) { continue; };

            Console.WriteLine("Got updates");
            
            foreach (var update in updates)
            {
                update_id = update.Id + 1;

                var userId = update.Message!.From!.Id;

                if (userNumber.TryGetValue(userId, out var ans))
                {
                    // We already chose a number for them
                    if (!int.TryParse(update.Message!.Text, out var userAns))
                    {
                        await botClient.SendTextMessageAsync(userId, "Don't play");
                        continue;
                    }

                    userNumber.Remove(userId);

                    if (userAns == ans)
                    {
                        await botClient.SendTextMessageAsync(userId, "Correct!");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(userId, $"Nope! The number was: {ans}");
                    }
                }
                else
                {
                    userNumber.Add(userId, random.Next(0, 11));
                    await botClient.SendTextMessageAsync(userId, "Guess my number between 0 and 10!");
                }
            }
            await delay;
        }
    }
}