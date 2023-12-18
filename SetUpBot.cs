using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups; 

namespace LocalFunctionProj
{
    public class SetUpBot
    {
        private readonly TelegramBotClient _botClient;

        public SetUpBot()
        {
            _botClient = new TelegramBotClient(System.Environment.GetEnvironmentVariable("TelegramBotToken", EnvironmentVariableTarget.Process));
        }

        private const string SetUpFunctionName = "setup";
        private const string UpdateFunctionName = "handleupdate";

        [Function(SetUpFunctionName)]
        public async Task RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            var handleUpdateFunctionUrl = req.Url.ToString().Replace(SetUpFunctionName, UpdateFunctionName,
                                                ignoreCase: true, culture: CultureInfo.InvariantCulture);
            await _botClient.SetWebhookAsync(handleUpdateFunctionUrl);
        }

        [Function(UpdateFunctionName)]
        public async Task Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            var request = await req.ReadAsStringAsync();
            var update = JsonConvert.DeserializeObject<Telegram.Bot.Types.Update>(request);

            if (update.Type != UpdateType.Message)
                return;
            if (update.Message!.Type != MessageType.Text)
                return;

             if (update.Message.Text.StartsWith("/start"))
            {
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                new KeyboardButton[][]
                {
                    new KeyboardButton[]
                    {
                    new KeyboardButton("Спеціальності"),
                     new KeyboardButton("Кафедра прикладної математики"),
                    new KeyboardButton("Як добратись"),
                    },
                    new KeyboardButton[]
                    {
                    new KeyboardButton("Освітні програми"),
                    new KeyboardButton("Офіційний сайт"),
                    },
                    new KeyboardButton[]
                    {
                    new KeyboardButton("Назад"),
                    }
                }
                )
                {
                ResizeKeyboard = true
            };
                await _botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "Виберіть опцію:",
                    replyMarkup: replyKeyboardMarkup
                );
            }
            else if (update.Message.Text.Contains("Назад",StringComparison.InvariantCultureIgnoreCase))
            {
            var replyKeyboardRemove = new ReplyKeyboardRemove();
            await _botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                 text: "Клавіатуру приховано. Щоб отримати її знову скористайтеся командою /start.",
                replyMarkup: replyKeyboardRemove
            );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: GetBotResponseForInput(update.Message.Text)
                );
            }
        }

        private string GetBotResponseForInput(string text)
        {
            
                if (text.Contains("Спеціальності", StringComparison.InvariantCultureIgnoreCase))
                 {
                    return "Доступні спеціальності:\n(122) Комп'ютерні науки\n(121) Програмне забезпечення";
                }
                if (text.Contains("Як добратись", StringComparison.InvariantCultureIgnoreCase))
                 {
                    return "вул. Митрополита Андрея 5, 4-й н.к., кім. 213";
                }
                if (text.Contains("Освітні програми", StringComparison.InvariantCultureIgnoreCase))
                 {
                    return "Доступні освітні програми:\n(012) Дошкільна освіта\n(013) Початкова освіта\n(073) Менеджмент";
                }
                if (text.Contains("Офіційний сайт", StringComparison.InvariantCultureIgnoreCase))
                 {
                    return "https://lpnu.ua/";
                }
                if (text.Contains("Кафедра прикладної математики", StringComparison.InvariantCultureIgnoreCase))
                 {
                    return "Присутня";
                }
                return $"Натисніть на одну із доступних кнопок 👀";
        }
    }
}
