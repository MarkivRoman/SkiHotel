using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SkiHotel
{
    public class UserProfile
    {
        public long ChatId { get; set; }
        public string FullName { get; set; } = "Не вказано";
        public string Phone { get; set; } = "Не вказано";
        public string Email { get; set; } = "Не вказано";
        public int Height { get; set; }
        public int Weight { get; set; }
        public int ShoeSize { get; set; }
        public string SkillLevel { get; set; } = "Початківець";
        public string RegistrationStep { get; set; } = "None";
    }

    public class TelegramBotHandler
    {
        private static ITelegramBotClient _botClient;
        private static CancellationTokenSource _cts;

        // Вкажіть свій Chat ID з Telegram для доступу до адмін-панелі
        private const long AdminChatId = 123456789;

        private static readonly List<UserProfile> Users = new List<UserProfile>();

        public static void StartBot()
        {
            try
            {
                // Токен вашого бота від BotFather
                _botClient = new TelegramBotClient("8697508694:AAG5voEsyUVLzPqvN7TXbUme7XKO-mx69hE");
                _cts = new CancellationTokenSource();

                _botClient.StartReceiving(
                    HandleUpdateAsync,
                    HandleErrorAsync,
                    cancellationToken: _cts.Token
                );

                Console.WriteLine("Бот SkiHotel успішно запущений!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка запуску бота: {ex.Message}");
            }
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
                {
                    long chatId = update.Message.Chat.Id;
                    string text = update.Message.Text;

                    UserProfile user = Users.FirstOrDefault(u => u.ChatId == chatId);
                    if (user == null)
                    {
                        user = new UserProfile { ChatId = chatId };
                        Users.Add(user);
                    }

                    if (text == "/start" || text == "🏠 Головне меню")
                    {
                        user.RegistrationStep = "None";
                        await SendMainMenu(bot, chatId, cancellationToken);
                        return;
                    }

                    if (text == "👤 Мій профіль")
                    {
                        string info = $"📋 *Ваш профіль у SkiHotel:*\n\n" +
                                     $"• Ім'я: {user.FullName}\n" +
                                     $"• Телефон: {user.Phone}\n" +
                                     $"• Email: {user.Email}\n" +
                                     $"• Зріст: {user.Height} см\n" +
                                     $"• Вага: {user.Weight} кг\n" +
                                     $"• Розмір взуття: {user.ShoeSize}\n" +
                                     $"• Рівень катання: {user.SkillLevel}";

                        await bot.SendMessage(chatId, info, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        return;
                    }

                    if (text == "✏️ Заповнити / Змінити дані")
                    {
                        user.RegistrationStep = "WaitingForName";
                        await bot.SendMessage(chatId, "Введіть ваше *Ім'я та Прізвище*:", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        return;
                    }

                    if (text == "📊 Адмін-панель" || text == "/admin")
                    {
                        if (chatId == AdminChatId)
                        {
                            await bot.SendMessage(chatId, $"📈 *Статистика SkiHotel:*\n\nВсього користувачів у боті: *{Users.Count}*", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await bot.SendMessage(chatId, "⛔ У вас немає прав адміністратора.", cancellationToken: cancellationToken);
                        }
                        return;
                    }

                    switch (user.RegistrationStep)
                    {
                        case "WaitingForName":
                            user.FullName = text;
                            user.RegistrationStep = "WaitingForPhone";
                            await bot.SendMessage(chatId, "Введіть ваш *Номер телефону*:", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForPhone":
                            user.Phone = text;
                            user.RegistrationStep = "WaitingForEmail";
                            await bot.SendMessage(chatId, "Введіть ваш *Email*:", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForEmail":
                            user.Email = text;
                            user.RegistrationStep = "WaitingForHeight";
                            await bot.SendMessage(chatId, "Вкажіть ваш *Зріст (см)*:", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForHeight":
                            int h;
                            if (int.TryParse(text, out h)) user.Height = h;
                            user.RegistrationStep = "WaitingForWeight";
                            await bot.SendMessage(chatId, "Вкажіть вашу *Вагу (кг)*:", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForWeight":
                            int w;
                            if (int.TryParse(text, out w)) user.Weight = w;
                            user.RegistrationStep = "WaitingForShoe";
                            await bot.SendMessage(chatId, "Вкажіть ваш *Розмір взуття*:", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForShoe":
                            int s;
                            if (int.TryParse(text, out s)) user.ShoeSize = s;
                            user.RegistrationStep = "None";

                            await bot.SendMessage(chatId, "✅ *Дані успішно збережено!* Тепер ви можете швидше оформлювати замовлення без черги.", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            await SendMainMenu(bot, chatId, cancellationToken);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка обробки повідомлення: {ex.Message}");
            }
        }

        private static async Task SendMainMenu(ITelegramBotClient bot, long chatId, CancellationToken cancellationToken)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "👤 Мій профіль", "✏️ Заповнити / Змінити дані" },
                new KeyboardButton[] { "⛷️ Замовити / Оплатити", "🏠 Головне меню" }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendMessage(
                chatId: chatId,
                text: "Вітаємо у SkiHotel! 🏔️\nОберіть потрібну дію з меню нижче:",
                replyMarkup: replyKeyboard,
                cancellationToken: cancellationToken
            );
        }

        private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Помилка бота: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}