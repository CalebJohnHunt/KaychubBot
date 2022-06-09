using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramHook.Controllers
{
    [Route("api")]
    [ApiController]
    public class BotController : ControllerBase
    {
        const string botToken = "5183417049:AAH-63IZkCRMmx_9BoZaF37vuX-zWQQdTW8";
        static TelegramBotClient botClient = new(botToken);

        [HttpPost]
        [Route("echo")]
        public async Task<IActionResult> Main([FromBody] Update update)
        {
            Console.Write("foo");
            var chat_id = update.Message?.Chat.Id;
            var message = update.Message?.Text;
            if (chat_id is not null && message is not null)
            {
                await botClient.SendTextMessageAsync(chat_id, message);
            }
            return Ok();

        }

        [HttpPost]
        [Route("foo")]
        public string bar()
        {
            Console.WriteLine("foo");
            return "bar";
        }
    }
}
