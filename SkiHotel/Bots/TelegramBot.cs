using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SkiHotel.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SkiHotel
{
    public static class LocalizationManager
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Languages = new Dictionary<string, Dictionary<string, string>>();

        private static string _appLanguage = "en";
        public static event Action LanguageChanged;

        public static string AppLanguage
        {
            get => _appLanguage;
            set
            {
                if (_appLanguage != value)
                {
                    _appLanguage = value;
                    LanguageChanged?.Invoke();
                }
            }
        }

        public static void LoadLanguages()
        {
            string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization");
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            string[] files = Directory.GetFiles(baseDir, "*.json");
            foreach (var file in files)
            {
                string langCode = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                Languages[langCode] = dictionary;
            }
        }

        public static string Get(string langCode, string key, params object[] args)
        {
            if (string.IsNullOrEmpty(langCode) || !Languages.ContainsKey(langCode))
            {
                langCode = "uk";
            }

            if (Languages.TryGetValue(langCode, out var dict) && dict.TryGetValue(key, out var value))
            {
                return args.Length > 0 ? string.Format(value, args) : value;
            }

            return $"[{key}]";
        }

        public static bool IsMenuKey(string langCode, string key, string text)
        {
            return Get(langCode, key) == text;
        }
    }

    public class UserProfile
    {
        public long ChatId { get; set; }
        public string Language { get; set; } = "uk";
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public int ShoeSize { get; set; }
        public string SkillLevel { get; set; }
        public string RegistrationStep { get; set; } = "None";

        public string GetFullName() => string.IsNullOrEmpty(FullName) ? LocalizationManager.Get(Language, "DefaultNotSpecified") : FullName;
        public string GetPhone() => string.IsNullOrEmpty(Phone) ? LocalizationManager.Get(Language, "DefaultNotSpecified") : Phone;
        public string GetEmail() => string.IsNullOrEmpty(Email) ? LocalizationManager.Get(Language, "DefaultNotSpecified") : Email;
        public string GetSkillLevel() => string.IsNullOrEmpty(SkillLevel) ? LocalizationManager.Get(Language, "DefaultBeginner") : SkillLevel;
    }

    public class TelegramBotHandler
    {
        private static ITelegramBotClient _botClient;
        private static CancellationTokenSource _cts;

        // ВПИШИ СЮДИ ВАШІ ЧИСЛОВІ TELEGRAM ID ЧЕРЕЗ КОМУ:
        private static readonly long[] AdminChatIds = { 1108484299, 1629241520, 1092932367 };

        private static readonly List<UserProfile> Users = new List<UserProfile>();
        private static readonly HashSet<string> SentOrderSignatures = new HashSet<string>();
        private static readonly HashSet<string> ProcessedCallbacks = new HashSet<string>();

        private static bool HasValidCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            foreach (char c in input.Trim())
            {
                if (char.IsLetterOrDigit(c))
                    return true;
            }
            return false;
        }

        public static void StartBot()
        {
            try
            {
                LocalizationManager.LoadLanguages();

                _botClient = new TelegramBotClient("8697508694:AAG5voEsyUVLzPqvN7TXbUme7XKO-mx69hE");
                _cts = new CancellationTokenSource();

                _botClient.StartReceiving(
                    HandleUpdateAsync,
                    HandleErrorAsync,
                    cancellationToken: _cts.Token
                );

                Console.WriteLine(LocalizationManager.Get("uk", "BotStarted"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(LocalizationManager.Get("uk", "BotStartError", ex.Message));
            }
        }

        public static async Task SendNewOrderNotificationAsync(Order order)
        {
            if (_botClient == null || order == null)
                return;

            if (!HasValidCharacters(order.ClientName) ||
               (!HasValidCharacters(order.Phone) && !HasValidCharacters(order.Email)))
                return;

            string signature = $"{order.ClientName.Trim().ToLower()}|{order.Phone?.Trim()}|{order.Email?.Trim().ToLower()}|{order.SkiPassDays}|{order.TotalPrice}";
            if (!SentOrderSignatures.Add(signature))
                return;

            string message =
                $"🎿 *Нове замовлення #{order.Id}*\n" +
                $"👤 Клієнт: {order.ClientName}\n" +
                $"📞 Телефон: {(string.IsNullOrWhiteSpace(order.Phone) ? "-" : order.Phone)}\n" +
                $"📧 Email: {(string.IsNullOrWhiteSpace(order.Email) ? "-" : order.Email)}\n" +
                $"📅 Днів: {order.SkiPassDays}\n" +
                $"💰 Сума: {order.TotalPrice} UAH";

            foreach (long adminId in AdminChatIds)
            {
                try
                {
                    await _botClient.SendMessage(
                        chatId: adminId,
                        text: message,
                        parseMode: ParseMode.Markdown
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Telegram notification error for {adminId}: {ex.Message}");
                }
            }
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.CallbackQuery != null)
                {
                    await HandleCallbackQueryAsync(bot, update.CallbackQuery, cancellationToken);
                    return;
                }

                if (update.Message != null && update.Message.Text != null)
                {
                    long chatId = update.Message.Chat.Id;
                    string text = update.Message.Text.Trim();

                    if (!HasValidCharacters(text))
                    {
                        await bot.SendMessage(chatId, "Повідомлення не може бути порожнім або без символів.", cancellationToken: cancellationToken);
                        return;
                    }

                    UserProfile user = Users.FirstOrDefault(u => u.ChatId == chatId);
                    if (user == null)
                    {
                        user = new UserProfile { ChatId = chatId };
                        Users.Add(user);
                    }

                    string lang = user.Language;

                    if (text == LocalizationManager.Get(lang, "CmdStart") || LocalizationManager.IsMenuKey(lang, "MenuMain", text))
                    {
                        user.RegistrationStep = "None";
                        await SendMainMenu(bot, chatId, lang, cancellationToken);
                        return;
                    }

                    if (LocalizationManager.IsMenuKey(lang, "MenuLanguage", text))
                    {
                        await SendLanguageSelector(bot, chatId, cancellationToken);
                        return;
                    }

                    if (LocalizationManager.IsMenuKey(lang, "MenuProfile", text))
                    {
                        string info = LocalizationManager.Get(lang, "ProfileFormat",
                            user.GetFullName(),
                            user.GetPhone(),
                            user.GetEmail(),
                            user.Height,
                            user.Weight,
                            user.ShoeSize,
                            user.GetSkillLevel());

                        await bot.SendMessage(chatId, info, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        return;
                    }

                    if (LocalizationManager.IsMenuKey(lang, "MenuAbout", text))
                    {
                        await bot.SendMessage(chatId, LocalizationManager.Get(lang, "AboutText"), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        return;
                    }

                    if (LocalizationManager.IsMenuKey(lang, "MenuEditData", text))
                    {
                        user.RegistrationStep = "WaitingForName";
                        await bot.SendMessage(chatId, LocalizationManager.Get(lang, "PromptName"), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        return;
                    }

                    if (LocalizationManager.IsMenuKey(lang, "MenuAdmin", text) || text == LocalizationManager.Get(lang, "CmdAdmin"))
                    {
                        if (AdminChatIds.Contains(chatId))
                        {
                            await bot.SendMessage(chatId, LocalizationManager.Get(lang, "AdminStats", Users.Count), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await bot.SendMessage(chatId, LocalizationManager.Get(lang, "AdminAccessDenied"), cancellationToken: cancellationToken);
                        }
                        return;
                    }

                    switch (user.RegistrationStep)
                    {
                        case "WaitingForName":
                            user.FullName = text;
                            user.RegistrationStep = "WaitingForPhone";
                            await bot.SendMessage(chatId, LocalizationManager.Get(lang, "PromptPhone"), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForPhone":
                            user.Phone = text;
                            user.RegistrationStep = "WaitingForEmail";
                            await bot.SendMessage(chatId, LocalizationManager.Get(lang, "PromptEmail"), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForEmail":
                            user.Email = text;
                            user.RegistrationStep = "WaitingForHeight";
                            await bot.SendMessage(chatId, LocalizationManager.Get(lang, "PromptHeight"), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForHeight":
                            if (!int.TryParse(text, out int h) || h <= 0)
                            {
                                await bot.SendMessage(chatId, "Введіть коректний зріст числом:", cancellationToken: cancellationToken);
                                return;
                            }
                            user.Height = h;
                            user.RegistrationStep = "WaitingForWeight";
                            await bot.SendMessage(chatId, LocalizationManager.Get(lang, "PromptWeight"), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForWeight":
                            if (!int.TryParse(text, out int w) || w <= 0)
                            {
                                await bot.SendMessage(chatId, "Введіть коректну вагу числом:", cancellationToken: cancellationToken);
                                return;
                            }
                            user.Weight = w;
                            user.RegistrationStep = "WaitingForShoe";
                            await bot.SendMessage(chatId, LocalizationManager.Get(lang, "PromptShoe"), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            break;

                        case "WaitingForShoe":
                            if (!int.TryParse(text, out int s) || s <= 0)
                            {
                                await bot.SendMessage(chatId, "Введіть коректний розмір взуття числом:", cancellationToken: cancellationToken);
                                return;
                            }
                            user.ShoeSize = s;
                            user.RegistrationStep = "None";

                            await bot.SendMessage(chatId, LocalizationManager.Get(lang, "DataSaved"), parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                            await SendMainMenu(bot, chatId, lang, cancellationToken);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task HandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery?.Message == null || string.IsNullOrEmpty(callbackQuery.Data))
                return;

            long chatId = callbackQuery.Message.Chat.Id;
            int messageId = callbackQuery.Message.MessageId;
            string callbackKey = $"{chatId}:{messageId}:{callbackQuery.Data}";

            if (!ProcessedCallbacks.Add(callbackKey))
            {
                await bot.AnswerCallbackQuery(callbackQuery.Id, "Дію вже виконано.", cancellationToken: cancellationToken);
                return;
            }

            UserProfile user = Users.FirstOrDefault(u => u.ChatId == chatId);
            if (user == null)
            {
                user = new UserProfile { ChatId = chatId };
                Users.Add(user);
            }

            if (callbackQuery.Data.StartsWith("lang_"))
            {
                string selectedLang = callbackQuery.Data.Replace("lang_", "");
                user.Language = selectedLang;

                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                await bot.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null, cancellationToken: cancellationToken);
                await bot.SendMessage(chatId, LocalizationManager.Get(selectedLang, "LanguageSelected"), cancellationToken: cancellationToken);
                await SendMainMenu(bot, chatId, selectedLang, cancellationToken);
            }
        }

        private static async Task SendLanguageSelector(ITelegramBotClient bot, long chatId, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Українська 🇺🇦", "lang_uk"),
                    InlineKeyboardButton.WithCallbackData("English 🇬🇧", "lang_en")
                }
            });

            await bot.SendMessage(
                chatId: chatId,
                text: LocalizationManager.Get("uk", "ChooseLanguagePrompt"),
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken
            );
        }

        private static async Task SendMainMenu(ITelegramBotClient bot, long chatId, string lang, CancellationToken cancellationToken)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { LocalizationManager.Get(lang, "MenuProfile"), LocalizationManager.Get(lang, "MenuEditData") },
                new KeyboardButton[] { LocalizationManager.Get(lang, "MenuOrder"), LocalizationManager.Get(lang, "MenuAbout") },
                new KeyboardButton[] { LocalizationManager.Get(lang, "MenuLanguage"), LocalizationManager.Get(lang, "MenuMain") }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendMessage(
                chatId: chatId,
                text: LocalizationManager.Get(lang, "WelcomeMessage"),
                replyMarkup: replyKeyboard,
                cancellationToken: cancellationToken
            );
        }

        private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine(LocalizationManager.Get("uk", "BotError", exception.Message));
            return Task.CompletedTask;
        }
    }
}