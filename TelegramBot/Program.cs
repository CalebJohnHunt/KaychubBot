namespace TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

class TelegramBot
{
    static Random rand = new Random();
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine($"Usage:{Environment.NewLine}    {Environment.GetCommandLineArgs()[0]} <bot token>");
            return;
        }
        string botToken = args[0];

        ITelegramBotClient botClient = new TelegramBotClient(botToken);


        //await guessMyNumber(botClient);
        await KeyBoarding(botClient);
    }

    static async Task KeyBoarding(ITelegramBotClient botClient)
    {
        int update_id = 0;
        Update[] updates;

        while (true)
        {
            try 
            { 
                updates = await botClient.GetUpdatesAsync(offset: update_id,
                    allowedUpdates: new[] { UpdateType.CallbackQuery, UpdateType.Message }
                    );
            }
            catch (Exception) { continue; }
            foreach (var update in updates)
            {
                update_id = update.Id + 1;
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await BeginGame(botClient, update.Message!.From!.Id);
                        break;
                    case UpdateType.CallbackQuery:
                        QueryData q = new QueryData(update.CallbackQuery!.Data!);
                        switch (q.step)
                        {
                            case 0:
                                await AnswerCallbackQueryEmpty(botClient, update.CallbackQuery.Id);
                                await BeginGame(botClient, update.CallbackQuery.From.Id);
                                break;
                            case 1:
                                await GiveOption(botClient, update.CallbackQuery.Id, update.CallbackQuery.From.Id, q);
                                break;
                            case 2:
                                await TellResults(botClient, update.CallbackQuery.Id, update.CallbackQuery.From.Id, q);
                                break;
                            default:
                                continue;// throw new NotImplementedException("Steps > 0 not supported");
                        }
                        break;
                }
            }
        }
    }

    static async Task AnswerCallbackQueryEmpty(ITelegramBotClient botClient, string queryId)
    {
        await botClient.AnswerCallbackQueryAsync(queryId);
    }

    static async Task TellResults(ITelegramBotClient botClient, string queryId, long userId, QueryData q)
    {
        var ans = AnswerCallbackQueryEmpty(botClient, queryId);

        var possibleDoors = new List<byte> { 0, 1, 2 };
        possibleDoors.Remove(q.doorUsed);
        possibleDoors.Remove(q.doorShown);

        var finalDoor = q.choseStay ? q.doorUsed : possibleDoors[0];

        string text = "You lost.";
        if (finalDoor == q.carLoc)
        {
            text = "You win!🚗";
        }

        var cho = botClient.SendTextMessageAsync(userId, text);

        await ans;
        await cho;

        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Play again", q.Step(0).ToString())
            }
        });

        await botClient.SendTextMessageAsync(userId, "Play again?", replyMarkup: inlineKeyboard);
    }

    static async Task GiveOption(ITelegramBotClient botClient, string queryId, long userId, QueryData q)
    {
        var ans = AnswerCallbackQueryEmpty(botClient, queryId);

        var possibleDoors = new List<byte> { 0, 1, 2 };
        possibleDoors.Remove(q.doorUsed);
        possibleDoors.Remove(q.carLoc);
        byte goatDoor = possibleDoors[rand.Next(possibleDoors.Count)];

        q.DoorShown(goatDoor)
            .Step(2)
            .ChoseStay(true);

        InlineKeyboardMarkup inlineKeyboard = new(new[]
            {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Stay", q.ToString()),
                InlineKeyboardButton.WithCallbackData("Switch", q.ChoseStay(false).ToString())
            }
        });

        possibleDoors = new List<byte> { 0, 1, 2 };
        possibleDoors.Remove(q.doorUsed);
        possibleDoors.Remove(q.doorShown);

        Dictionary<byte, string> d = new() {
            { q.doorShown, "🐐" },
            { q.doorUsed, "✅"},
            { possibleDoors[0], "❓" }
        };

        var cho = botClient.SendTextMessageAsync(userId,
            $"🚪 🚪 🚪{Environment.NewLine}{d[0]} {d[1]} {d[2]}{Environment.NewLine}Switch or stay?",
            replyMarkup: inlineKeyboard);

        await ans;
        await cho;
    }
    static async Task BeginGame(ITelegramBotClient botClient, long userId)
    {
        var q1 = new QueryData()
                    .Step(1)
                    .CarLoc((byte)rand.Next(3))
                    .DoorUsed(0);
        var q2 = ((QueryData)q1.Clone()).DoorUsed(1);
        var q3 = ((QueryData)q1.Clone()).DoorUsed(2);

        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Door 1", q1.ToString()),
                                InlineKeyboardButton.WithCallbackData("Door 2", q2.ToString()),
                                InlineKeyboardButton.WithCallbackData("Door 3", q3.ToString())
                            }
                        });
        await botClient.SendTextMessageAsync(
            userId,
            "Pick a door",
            replyMarkup: inlineKeyboard);
    }

    static async Task guessMyNumber(ITelegramBotClient botClient)
    {
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
            catch (TimeoutException) { Console.WriteLine("Timeout");  continue; };

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