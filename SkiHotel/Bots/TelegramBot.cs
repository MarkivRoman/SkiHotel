using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SkiHotel.Models;
using SkiHotel.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SkiHotel
{
    public static class LocalizationManager
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Languages =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

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
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                                 ?? new Dictionary<string, string>();
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

    public class BotAccount
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Language { get; set; } = "uk";
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public int Height { get; set; } = 0;
        public int Weight { get; set; } = 0;
        public int ShoeSize { get; set; } = 0;
        public string SkillLevel { get; set; } = "Beginner";
        public long? LastChatId { get; set; }
    }

    public class DraftOrder
    {
        public int Days { get; set; } = 1;
        public bool OwnEquipment { get; set; } = false;
        public bool HotelRoom { get; set; } = false;
        public bool Breakfast { get; set; } = false;
        public bool Sauna { get; set; } = false;
        public decimal TotalPrice { get; set; } = 0m;

        public string PendingFullName { get; set; } = "";
        public string PendingPhone { get; set; } = "";
        public string PendingEmail { get; set; } = "";
        public int PendingHeight { get; set; } = 0;
        public int PendingWeight { get; set; } = 0;
        public int PendingShoeSize { get; set; } = 0;
    }

    public class BotSession
    {
        public long ChatId { get; set; }
        public string Language { get; set; } = "uk";
        public int? LoggedInAccountId { get; set; }
        public string Step { get; set; } = "None";

        public string PendingUsername { get; set; }
        public DraftOrder Draft { get; set; } = new DraftOrder();
    }

    public class TelegramBotHandler
    {
        private const int OrderCooldownSeconds = 20;

        private static ITelegramBotClient _botClient;
        private static CancellationTokenSource _cts;

        private static readonly long[] AdminChatIds = { 1108484299, 1629241520, 1092932367 };

        private static readonly ConcurrentDictionary<long, BotSession> Sessions = new ConcurrentDictionary<long, BotSession>();
        private static readonly ConcurrentDictionary<long, DateTime> LastOrderTimeByChat = new ConcurrentDictionary<long, DateTime>();
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

        public static async Task SendOrderConfirmedNotificationAsync(Order order)
        {
            if (_botClient == null || order == null || !order.BotChatId.HasValue)
                return;

            long chatId = order.BotChatId.Value;
            string lang = "uk";

            if (Sessions.TryGetValue(chatId, out var session) && !string.IsNullOrEmpty(session.Language))
            {
                lang = session.Language;
            }

            string text = lang == "en"
                ? $"🎉 *Booking #{order.Id} Confirmed!*\n\n" +
                  $"Dear *{order.ClientName}*, your payment/booking has been confirmed by our administrator!\n" +
                  $"📅 Days: *{order.SkiPassDays}*\n" +
                  $"💰 Total: *{order.TotalPrice} UAH*\n\n" +
                  $"We warmly look forward to welcoming you at *SkiHotel*! Pack your winter spirit and get ready for unforgettable mountain slopes! 🏔️⛷️"
                : $"🎉 *Бронювання #{order.Id} підтверджено!*\n\n" +
                  $"Шановний(-а) *{order.ClientName}*, наш адміністратор підтвердив ваше замовлення!\n" +
                  $"📅 Кількість днів: *{order.SkiPassDays}*\n" +
                  $"💰 Сума: *{order.TotalPrice} грн*\n\n" +
                  $"Щиро чекаємо на вас у *SkiHotel*! Готуйте гарний настрій — попереду незабутні схили та теплий відпочинок у горах! 🏔️⛷️";

            try
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: text,
                    parseMode: ParseMode.Markdown
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Telegram confirmation notification error for {chatId}: {ex.Message}");
            }
        }

        #region Database Operations for BotAccounts

        private static BotAccount GetAccountByUsername(string username)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM BotAccounts WHERE LOWER(Username) = LOWER(@Username)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username.Trim());
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        return MapBotAccount(reader);
                    }
                }
            }
        }

        private static BotAccount GetAccountById(int id)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM BotAccounts WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        return MapBotAccount(reader);
                    }
                }
            }
        }

        private static int GetBotAccountsCount()
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM BotAccounts", conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private static BotAccount CreateBotAccount(string username, string passwordHash, string lang, long chatId)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = @"
                    INSERT INTO BotAccounts 
                    (Username, PasswordHash, Language, FullName, Phone, Email, Height, Weight, ShoeSize, SkillLevel, LastChatId)
                    VALUES 
                    (@Username, @PasswordHash, @Language, '', '', '', 0, 0, 0, 'Beginner', @ChatId);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username.Trim());
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@Language", lang);
                    cmd.Parameters.AddWithValue("@ChatId", chatId);

                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return GetAccountById(newId);
                }
            }
        }

        private static void UpdateBotAccount(BotAccount account)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = @"
                    UPDATE BotAccounts SET
                        Language = @Language,
                        FullName = @FullName,
                        Phone = @Phone,
                        Email = @Email,
                        Height = @Height,
                        Weight = @Weight,
                        ShoeSize = @ShoeSize,
                        SkillLevel = @SkillLevel,
                        LastChatId = @LastChatId
                    WHERE Id = @Id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Language", account.Language ?? "uk");
                    cmd.Parameters.AddWithValue("@FullName", account.FullName ?? "");
                    cmd.Parameters.AddWithValue("@Phone", account.Phone ?? "");
                    cmd.Parameters.AddWithValue("@Email", account.Email ?? "");
                    cmd.Parameters.AddWithValue("@Height", account.Height);
                    cmd.Parameters.AddWithValue("@Weight", account.Weight);
                    cmd.Parameters.AddWithValue("@ShoeSize", account.ShoeSize);
                    cmd.Parameters.AddWithValue("@SkillLevel", account.SkillLevel ?? "Beginner");
                    cmd.Parameters.AddWithValue("@LastChatId", (object)account.LastChatId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", account.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static BotAccount MapBotAccount(SqlDataReader reader)
        {
            return new BotAccount
            {
                Id = Convert.ToInt32(reader["Id"]),
                Username = reader["Username"]?.ToString() ?? "",
                PasswordHash = reader["PasswordHash"]?.ToString() ?? "",
                Language = reader["Language"]?.ToString() ?? "uk",
                FullName = reader["FullName"]?.ToString() ?? "",
                Phone = reader["Phone"]?.ToString() ?? "",
                Email = reader["Email"]?.ToString() ?? "",
                Height = reader["Height"] != DBNull.Value ? Convert.ToInt32(reader["Height"]) : 0,
                Weight = reader["Weight"] != DBNull.Value ? Convert.ToInt32(reader["Weight"]) : 0,
                ShoeSize = reader["ShoeSize"] != DBNull.Value ? Convert.ToInt32(reader["ShoeSize"]) : 0,
                SkillLevel = reader["SkillLevel"]?.ToString() ?? "Beginner",
                LastChatId = reader["LastChatId"] != DBNull.Value ? Convert.ToInt64(reader["LastChatId"]) : (long?)null
            };
        }

        private static List<Order> GetRecentOrdersForAccount(BotAccount account)
        {
            var result = new List<Order>();
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT TOP 3 * FROM Orders
                    WHERE (@ChatId IS NOT NULL AND BotChatId = @ChatId)
                       OR (@FullName <> '' AND LOWER(ClientName) = LOWER(@FullName))
                    ORDER BY Id DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ChatId", (object)account.LastChatId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FullName", account.FullName ?? "");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Order
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                ClientName = reader["ClientName"]?.ToString() ?? "",
                                SkiPassDays = reader["SkiPassDays"] != DBNull.Value ? Convert.ToInt32(reader["SkiPassDays"]) : 1,
                                TotalPrice = reader["TotalPrice"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPrice"]) : 0m,
                                Status = reader["Status"]?.ToString() ?? "Incoming"
                            });
                        }
                    }
                }
            }
            return result;
        }

        #endregion

        #region Menu & Text Helpers

        private static string GetBtn(string lang, string key)
        {
            bool isUk = lang != "en";
            switch (key)
            {
                case "Login": return isUk ? "🔐 Увійти" : "🔐 Login";
                case "Register": return isUk ? "🆕 Створити користувача" : "🆕 Create User";
                case "Language": return isUk ? "🌐 Змінити мову" : "🌐 Change Language";
                case "About": return isUk ? "ℹ️ Про нас" : "ℹ️ About Us";
                case "Main": return isUk ? "🏠 Головне меню" : "🏠 Main Menu";

                case "ViewParams": return isUk ? "📋 Переглянути параметри" : "📋 View Parameters";
                case "UpdateParams": return isUk ? "✏️ Змінити параметри" : "✏️ Update Parameters";
                case "OrderInfo": return isUk ? "📄 Інформація про замовлення" : "📄 Order Information";
                case "Order": return isUk ? "⛷️ Замовити" : "⛷️ Order";
                case "Logout": return isUk ? "🚪 Вийти з акаунту" : "🚪 Logout";

                case "Yes": return isUk ? "✅ Так" : "✅ Yes";
                case "No": return isUk ? "❌ Ні" : "❌ No";
                case "OwnEquip": return isUk ? "🎿 Власне спорядження" : "🎿 Own Equipment";
                case "RentEquip": return isUk ? "🏂 Прокат спорядження (+800 грн)" : "🏂 Rent Equipment (+800 UAH)";
                default: return key;
            }
        }

        private static bool MatchesBtn(string text, string lang, string key)
        {
            return text == GetBtn(lang, key) || text == GetBtn("uk", key) || text == GetBtn("en", key);
        }

        private static string FormatSkillDisplay(string skill, string lang)
        {
            bool isUk = lang != "en";
            switch ((skill ?? "Beginner").Trim().ToLower())
            {
                case "intermediate":
                case "середній":
                    return isUk ? "Середній" : "Intermediate";
                case "professional":
                case "професіонал":
                    return isUk ? "Професіонал" : "Professional";
                default:
                    return isUk ? "Початківець" : "Beginner";
            }
        }

        private static string NormalizeSkillFromInput(string input)
        {
            string s = (input ?? "").Trim().ToLower();
            if (s == "2" || s.Contains("inter") || s.Contains("серед")) return "Intermediate";
            if (s == "3" || s.Contains("pro") || s.Contains("проф")) return "Professional";
            return "Beginner";
        }

        private static string FormatOrderStatus(string status, string lang)
        {
            bool isUk = lang != "en";
            if (status == "Booked" || status == "Заброньовано")
                return isUk ? "✅ Підтверджено (активне)" : "✅ Confirmed (active)";
            return isUk ? "⏳ Очікує підтвердження адміна" : "⏳ Waiting for admin confirmation";
        }

        private static decimal CalculateDraftTotal(DraftOrder draft)
        {
            decimal total = 0m;
            int days = Math.Max(1, draft.Days);

            total += days * 1000m;
            if (!draft.OwnEquipment) total += 800m;
            if (draft.HotelRoom) total += days * 2500m;
            if (draft.Breakfast) total += days * 300m;
            if (draft.Sauna) total += 500m;

            return total;
        }

        private static async Task SendGuestMenu(ITelegramBotClient bot, long chatId, string lang, string messageText, CancellationToken ct)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { GetBtn(lang, "Login"), GetBtn(lang, "Register") },
                new KeyboardButton[] { GetBtn(lang, "Language"), GetBtn(lang, "About") },
                new KeyboardButton[] { GetBtn(lang, "Main") }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendMessage(chatId, messageText, parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: ct);
        }

        private static async Task SendAccountMenu(ITelegramBotClient bot, long chatId, string lang, string messageText, CancellationToken ct)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { GetBtn(lang, "ViewParams"), GetBtn(lang, "UpdateParams") },
                new KeyboardButton[] { GetBtn(lang, "Order"), GetBtn(lang, "OrderInfo") },
                new KeyboardButton[] { GetBtn(lang, "Logout"), GetBtn(lang, "Language") }
            })
            {
                ResizeKeyboard = true
            };

            await bot.SendMessage(chatId, messageText, parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: ct);
        }

        private static async Task SendYesNoKeyboard(ITelegramBotClient bot, long chatId, string lang, string prompt, CancellationToken ct)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { GetBtn(lang, "Yes"), GetBtn(lang, "No") }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await bot.SendMessage(chatId, prompt, parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: ct);
        }

        #endregion

        #region Update & Callback Handling

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
                        await bot.SendMessage(
                            chatId,
                            "Повідомлення не може бути порожнім або без символів.",
                            cancellationToken: cancellationToken
                        );
                        return;
                    }

                    var session = Sessions.GetOrAdd(chatId, id => new BotSession { ChatId = id, Language = "uk" });
                    string lang = session.Language ?? "uk";
                    bool isUk = lang != "en";

                    if (text == "/start" || text == LocalizationManager.Get(lang, "CmdStart") || MatchesBtn(text, lang, "Main"))
                    {
                        session.Step = "None";
                        session.PendingUsername = null;

                        if (session.LoggedInAccountId.HasValue)
                        {
                            var acc = GetAccountById(session.LoggedInAccountId.Value);
                            if (acc != null)
                            {
                                await SendAccountMenu(
                                    bot, chatId, lang,
                                    isUk
                                        ? $"🏠 *Головне меню акаунту ({acc.Username})*\nОберіть потрібну дію:"
                                        : $"🏠 *Account Main Menu ({acc.Username})*\nSelect an action:",
                                    cancellationToken
                                );
                                return;
                            }
                            session.LoggedInAccountId = null;
                        }

                        await SendGuestMenu(
                            bot, chatId, lang,
                            isUk
                                ? "🏔️ *Вітаємо у SkiHotel!*\nУвійдіть у свій акаунт або створіть новий, щоб керувати параметрами та бронювати:"
                                : "🏔️ *Welcome to SkiHotel!*\nLog in to your account or create a new one to manage parameters and book:",
                            cancellationToken
                        );
                        return;
                    }

                    if (MatchesBtn(text, lang, "Language") || LocalizationManager.IsMenuKey(lang, "MenuLanguage", text))
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
                            text: LocalizationManager.Get(lang, "ChooseLanguagePrompt"),
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cancellationToken
                        );
                        return;
                    }

                    if (MatchesBtn(text, lang, "About") || LocalizationManager.IsMenuKey(lang, "MenuAbout", text))
                    {
                        await bot.SendMessage(
                            chatId,
                            LocalizationManager.Get(lang, "AboutText"),
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                        return;
                    }

                    if (text == "/admin" || text == LocalizationManager.Get(lang, "CmdAdmin"))
                    {
                        if (AdminChatIds.Contains(chatId))
                        {
                            int totalBotUsers = GetBotAccountsCount();
                            await bot.SendMessage(
                                chatId,
                                isUk
                                    ? $"📈 *Статистика SkiHotel:*\n\nЗареєстрованих акаунтів у боті: *{totalBotUsers}*"
                                    : $"📈 *SkiHotel Statistics:*\n\nRegistered bot accounts: *{totalBotUsers}*",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                        }
                        else
                        {
                            await bot.SendMessage(chatId, LocalizationManager.Get(lang, "AdminAccessDenied"), cancellationToken: cancellationToken);
                        }
                        return;
                    }

                    if (session.Step != "None")
                    {
                        await HandleStepInputAsync(bot, session, text, cancellationToken);
                        return;
                    }

                    if (!session.LoggedInAccountId.HasValue)
                    {
                        if (MatchesBtn(text, lang, "Register"))
                        {
                            session.Step = "RegUsername";
                            session.PendingUsername = null;
                            await bot.SendMessage(
                                chatId,
                                isUk
                                    ? "🆕 *Створення нового користувача*\n\nВведіть бажаний *логін* (мінімум 3 символи, латиниця/цифри):"
                                    : "🆕 *Create New User*\n\nEnter desired *username* (at least 3 characters, letters/digits):",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                            return;
                        }

                        if (MatchesBtn(text, lang, "Login"))
                        {
                            session.Step = "LoginUsername";
                            session.PendingUsername = null;
                            await bot.SendMessage(
                                chatId,
                                isUk
                                    ? "🔐 *Вхід в акаунт*\n\nВведіть ваш *логін*:"
                                    : "🔐 *Account Login*\n\nEnter your *username*:",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                            return;
                        }

                        await SendGuestMenu(
                            bot, chatId, lang,
                            isUk
                                ? "Будь ласка, натисніть *«Увійти»* або *«Створити користувача»*."
                                : "Please press *«Login»* or *«Create User»*.",
                            cancellationToken
                        );
                        return;
                    }

                    BotAccount account = GetAccountById(session.LoggedInAccountId.Value);
                    if (account == null)
                    {
                        session.LoggedInAccountId = null;
                        await SendGuestMenu(
                            bot, chatId, lang,
                            isUk ? "Акаунт не знайдено. Увійдіть знову." : "Account not found. Please log in again.",
                            cancellationToken
                        );
                        return;
                    }

                    account.LastChatId = chatId;
                    UpdateBotAccount(account);

                    if (MatchesBtn(text, lang, "ViewParams"))
                    {
                        var recentOrders = GetRecentOrdersForAccount(account);
                        string ordersBlock;
                        if (recentOrders.Count == 0)
                        {
                            ordersBlock = isUk ? "Бронювань ще немає." : "No bookings yet.";
                        }
                        else
                        {
                            ordersBlock = string.Join("\n", recentOrders.Select(o =>
                                $"• #{o.Id} ({o.SkiPassDays} дн., {o.TotalPrice} грн) — {FormatOrderStatus(o.Status, lang)}"
                            ));
                        }

                        string notSpec = isUk ? "Не вказано" : "Not specified";
                        string profileText = isUk
                            ? $"📋 *Ваш профіль (`{account.Username}`):*\n\n" +
                              $"• Ім'я: {(string.IsNullOrWhiteSpace(account.FullName) ? notSpec : account.FullName)}\n" +
                              $"• Телефон: {(string.IsNullOrWhiteSpace(account.Phone) ? notSpec : account.Phone)}\n" +
                              $"• Email: {(string.IsNullOrWhiteSpace(account.Email) ? notSpec : account.Email)}\n" +
                              $"• Зріст: *{account.Height}* см\n" +
                              $"• Вага: *{account.Weight}* кг\n" +
                              $"• Розмір взуття: *{account.ShoeSize}*\n" +
                              $"• Рівень катання: *{FormatSkillDisplay(account.SkillLevel, lang)}*\n\n" +
                              $"🧾 *Останні бронювання:*\n{ordersBlock}"
                            : $"📋 *Your Profile (`{account.Username}`):*\n\n" +
                              $"• Name: {(string.IsNullOrWhiteSpace(account.FullName) ? notSpec : account.FullName)}\n" +
                              $"• Phone: {(string.IsNullOrWhiteSpace(account.Phone) ? notSpec : account.Phone)}\n" +
                              $"• Email: {(string.IsNullOrWhiteSpace(account.Email) ? notSpec : account.Email)}\n" +
                              $"• Height: *{account.Height}* cm\n" +
                              $"• Weight: *{account.Weight}* kg\n" +
                              $"• Shoe size: *{account.ShoeSize}*\n" +
                              $"• Skill level: *{FormatSkillDisplay(account.SkillLevel, lang)}*\n\n" +
                              $"🧾 *Recent Bookings:*\n{ordersBlock}";

                        await bot.SendMessage(chatId, profileText, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        return;
                    }

                    if (MatchesBtn(text, lang, "UpdateParams"))
                    {
                        session.Step = "EditFullName";
                        await bot.SendMessage(
                            chatId,
                            isUk
                                ? "✏️ *Оновлення параметрів*\n\nВведіть ваше *Ім'я та Прізвище*:"
                                : "✏️ *Update Parameters*\n\nEnter your *Full Name*:",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                        return;
                    }

                    if (MatchesBtn(text, lang, "OrderInfo"))
                    {
                        string infoText = isUk
                            ? "📄 *Інформація про замовлення та оплату в SkiHotel*\n\n" +
                              "1️⃣ *Як працює бронювання:* Ви вказуєте кількість днів, чи потрібне спорядження з прокату, а також додаткові опції (готель, сніданок, сауна). Ваші параметри зросту, ваги та розміру взуття підтягуються з профілю.\n" +
                              "2️⃣ *Тарифи:*\n" +
                              "   • Скі-пас: *1000 грн / день*\n" +
                              "   • Прокат повного комплекту: *800 грн* (якщо немає власного спорядження)\n" +
                              "   • Номер у готелі: *2500 грн / день*\n" +
                              "   • Сніданок: *300 грн / день*\n" +
                              "   • Сауна: *500 грн*\n" +
                              "3️⃣ *Оплата та підтвердження:* Після створення заявки ви отримаєте реквізити та суму. Ви можете внести передплату (від 30%) або повну суму. Замовлення переходить у статус *підтвердженого* після перевірки адміністратором у системі SkiHotel, після чого вам прийде сповіщення в цей чат."
                            : "📄 *Booking & Payment Information at SkiHotel*\n\n" +
                              "1️⃣ *How booking works:* You choose the number of days, whether you need rental equipment, and optional extras (hotel room, breakfast, sauna). Your height, weight, and shoe size are taken from your profile.\n" +
                              "2️⃣ *Rates:*\n" +
                              "   • Ski pass: *1000 UAH / day*\n" +
                              "   • Full rental set: *800 UAH* (if not using own equipment)\n" +
                              "   • Hotel room: *2500 UAH / day*\n" +
                              "   • Breakfast: *300 UAH / day*\n" +
                              "   • Sauna: *500 UAH*\n" +
                              "3️⃣ *Payment & confirmation:* After submitting, you receive payment details and total amount. You can pay a deposit (from 30%) or full amount. Once confirmed by the administrator in SkiHotel, your booking becomes active and you get a notification here.";

                        await bot.SendMessage(chatId, infoText, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                        return;
                    }

                    if (MatchesBtn(text, lang, "Order"))
                    {
                        if (LastOrderTimeByChat.TryGetValue(chatId, out DateTime lastOrderAt))
                        {
                            double elapsedSeconds = (DateTime.Now - lastOrderAt).TotalSeconds;
                            if (elapsedSeconds < OrderCooldownSeconds)
                            {
                                int waitLeft = (int)Math.Ceiling(OrderCooldownSeconds - elapsedSeconds);
                                await bot.SendMessage(
                                    chatId,
                                    isUk
                                        ? $"⏳ Зачекайте ще *{waitLeft} сек.* перед створенням наступного замовлення (захист від повторних заявок)."
                                        : $"⏳ Please wait *{waitLeft} sec.* before placing another order (anti-spam protection).",
                                    parseMode: ParseMode.Markdown,
                                    cancellationToken: cancellationToken
                                );
                                return;
                            }
                        }

                        if (account.Height <= 0 || account.Weight <= 0 || account.ShoeSize <= 0)
                        {
                            await bot.SendMessage(
                                chatId,
                                isUk
                                    ? "⚠️ *Ваші параметри ще не заповнені (дорівнюють 0)!*\n\nСпочатку натисніть *«Змінити параметри»* та вкажіть коректні зріст, вагу і розмір взуття."
                                    : "⚠️ *Your parameters are not set yet (equal to 0)!*\n\nPlease press *«Update Parameters»* first and set your height, weight, and shoe size.",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken
                            );
                            return;
                        }

                        session.Draft = new DraftOrder();
                        session.Step = "OrderDays";
                        await bot.SendMessage(
                            chatId,
                            isUk
                                ? "📅 *Нове замовлення (крок 1/5)*\n\nВведіть кількість днів (від *1* до *14*):"
                                : "📅 *New Order (step 1/5)*\n\nEnter number of days (from *1* to *14*):",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                        return;
                    }

                    if (MatchesBtn(text, lang, "Logout"))
                    {
                        session.Step = "ConfirmLogout";
                        await SendYesNoKeyboard(
                            bot, chatId, lang,
                            isUk
                                ? "🚪 *Ви дійсно хочете вийти з акаунту?*"
                                : "🚪 *Are you sure you want to log out of your account?*",
                            cancellationToken
                        );
                        return;
                    }

                    await SendAccountMenu(
                        bot, chatId, lang,
                        isUk ? "Оберіть дію з меню нижче:" : "Select an action from the menu below:",
                        cancellationToken
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task HandleStepInputAsync(ITelegramBotClient bot, BotSession session, string text, CancellationToken ct)
        {
            long chatId = session.ChatId;
            string lang = session.Language ?? "uk";
            bool isUk = lang != "en";

            switch (session.Step)
            {
                case "RegUsername":
                    {
                        string username = text.Trim();
                        if (username.Length < 3 || username.Length > 40 || username.Any(char.IsWhiteSpace))
                        {
                            await bot.SendMessage(
                                chatId,
                                isUk
                                    ? "❌ Логін має містити від 3 до 40 символів без пробілів. Спробуйте ще раз:"
                                    : "❌ Username must be 3 to 40 characters without spaces. Try again:",
                                cancellationToken: ct
                            );
                            return;
                        }

                        if (GetAccountByUsername(username) != null)
                        {
                            await bot.SendMessage(
                                chatId,
                                isUk
                                    ? "❌ Такий логін уже зайнятий. Введіть інший логін:"
                                    : "❌ This username is already taken. Enter another username:",
                                cancellationToken: ct
                            );
                            return;
                        }

                        session.PendingUsername = username;
                        session.Step = "RegPassword";
                        await bot.SendMessage(
                            chatId,
                            isUk
                                ? "🔑 Введіть простий *пароль* для вашого акаунту (мінімум 3 символи):"
                                : "🔑 Enter a *password* for your account (at least 3 characters):",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct
                        );
                        break;
                    }

                case "RegPassword":
                    {
                        if (text.Length < 3)
                        {
                            await bot.SendMessage(
                                chatId,
                                isUk
                                    ? "❌ Пароль має містити мінімум 3 символи. Введіть пароль ще раз:"
                                    : "❌ Password must be at least 3 characters. Enter password again:",
                                cancellationToken: ct
                            );
                            return;
                        }

                        string hash = SecurityHelper.HashPassword(text);
                        var created = CreateBotAccount(session.PendingUsername, hash, lang, chatId);

                        session.Step = "None";
                        session.PendingUsername = null;

                        await SendGuestMenu(
                            bot, chatId, lang,
                            isUk
                                ? $"✅ *Акаунт `{created.Username}` успішно створено!*\nПочаткові параметри (зріст, вага, розмір взуття) встановлено в *0*.\n\nТепер увійдіть в акаунт через кнопку *«Увійти»*."
                                : $"✅ *Account `{created.Username}` successfully created!*\nInitial parameters (height, weight, shoe size) are set to *0*.\n\nNow please log in using *«Login»*.",
                            ct
                        );
                        break;
                    }

                case "LoginUsername":
                    {
                        var acc = GetAccountByUsername(text.Trim());
                        if (acc == null)
                        {
                            session.Step = "None";
                            await SendGuestMenu(
                                bot, chatId, lang,
                                isUk
                                    ? "❌ Користувача з таким логіном не знайдено. Перевірте логін або натисніть *«Створити користувача»*."
                                    : "❌ User with this username was not found. Check username or press *«Create User»*.",
                                ct
                            );
                            return;
                        }

                        session.PendingUsername = acc.Username;
                        session.Step = "LoginPassword";
                        await bot.SendMessage(
                            chatId,
                            isUk ? "🔑 Введіть *пароль*:" : "🔑 Enter *password*:",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct
                        );
                        break;
                    }

                case "LoginPassword":
                    {
                        var acc = GetAccountByUsername(session.PendingUsername ?? "");
                        session.PendingUsername = null;

                        if (acc == null || acc.PasswordHash != SecurityHelper.HashPassword(text))
                        {
                            session.Step = "None";
                            await SendGuestMenu(
                                bot, chatId, lang,
                                isUk
                                    ? "❌ Невірний логін або пароль. Спробуйте увійти знову."
                                    : "❌ Invalid username or password. Please try logging in again.",
                                ct
                            );
                            return;
                        }

                        acc.LastChatId = chatId;
                        UpdateBotAccount(acc);

                        session.LoggedInAccountId = acc.Id;
                        session.Step = "None";

                        await SendAccountMenu(
                            bot, chatId, lang,
                            isUk
                                ? $"🎉 *Вітаємо у вашому акаунті, `{acc.Username}`!*\nОберіть потрібну дію з меню:"
                                : $"🎉 *Welcome to your account, `{acc.Username}`!*\nChoose an action from the menu:",
                            ct
                        );
                        break;
                    }

                case "ConfirmLogout":
                    {
                        if (MatchesBtn(text, lang, "Yes"))
                        {
                            session.LoggedInAccountId = null;
                            session.Step = "None";
                            await SendGuestMenu(
                                bot, chatId, lang,
                                isUk
                                    ? "👋 Ви вийшли з акаунту. До зустрічі!"
                                    : "👋 You have logged out of your account. See you soon!",
                                ct
                            );
                        }
                        else if (MatchesBtn(text, lang, "No"))
                        {
                            session.Step = "None";
                            await SendAccountMenu(
                                bot, chatId, lang,
                                isUk ? "Вихід скасовано. Ви залишаєтесь в акаунті." : "Logout cancelled. You remain logged in.",
                                ct
                            );
                        }
                        else
                        {
                            await SendYesNoKeyboard(
                                bot, chatId, lang,
                                isUk ? "Оберіть *«Так»* або *«Ні»*:" : "Please choose *«Yes»* or *«No»*:",
                                ct
                            );
                        }
                        break;
                    }

                case "EditFullName":
                    {
                        session.Draft.PendingFullName = text.Trim();
                        session.Step = "EditPhone";
                        await bot.SendMessage(
                            chatId,
                            isUk ? "📞 Введіть ваш *Номер телефону*:" : "📞 Enter your *Phone Number*:",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct
                        );
                        break;
                    }

                case "EditPhone":
                    {
                        session.Draft.PendingPhone = text.Trim();
                        session.Step = "EditEmail";
                        await bot.SendMessage(
                            chatId,
                            isUk ? "📧 Введіть ваш *Email*:" : "📧 Enter your *Email*:",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct
                        );
                        break;
                    }

                case "EditEmail":
                    {
                        session.Draft.PendingEmail = text.Trim();
                        session.Step = "EditHeight";
                        await bot.SendMessage(
                            chatId,
                            isUk ? "📏 Вкажіть ваш *Зріст (см)* числом (наприклад, 175):" : "📏 Enter your *Height (cm)* as a number (e.g., 175):",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct
                        );
                        break;
                    }

                case "EditHeight":
                    {
                        if (!int.TryParse(text, out int h) || h < 80 || h > 240)
                        {
                            await bot.SendMessage(
                                chatId,
                                isUk ? "❌ Введіть коректний зріст числом (80–240 см):" : "❌ Enter a valid height as a number (80–240 cm):",
                                cancellationToken: ct
                            );
                            return;
                        }
                        session.Draft.PendingHeight = h;
                        session.Step = "EditWeight";
                        await bot.SendMessage(
                            chatId,
                            isUk ? "⚖️ Вкажіть вашу *Вагу (кг)* числом (наприклад, 70):" : "⚖️ Enter your *Weight (kg)* as a number (e.g., 70):",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct
                        );
                        break;
                    }

                case "EditWeight":
                    {
                        if (!int.TryParse(text, out int w) || w < 20 || w > 220)
                        {
                            await bot.SendMessage(
                                chatId,
                                isUk ? "❌ Введіть коректну вагу числом (20–220 кг):" : "❌ Enter a valid weight as a number (20–220 kg):",
                                cancellationToken: ct
                            );
                            return;
                        }
                        session.Draft.PendingWeight = w;
                        session.Step = "EditShoeSize";
                        await bot.SendMessage(
                            chatId,
                            isUk ? "👟 Вкажіть ваш *Розмір взуття* числом (наприклад, 42):" : "👟 Enter your *Shoe Size* as a number (e.g., 42):",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: ct
                        );
                        break;
                    }

                case "EditShoeSize":
                    {
                        if (!int.TryParse(text, out int s) || s < 25 || s > 52)
                        {
                            await bot.SendMessage(
                                chatId,
                                isUk ? "❌ Введіть коректний розмір взуття числом (25–52):" : "❌ Enter a valid shoe size as a number (25–52):",
                                cancellationToken: ct
                            );
                            return;
                        }
                        session.Draft.PendingShoeSize = s;
                        session.Step = "EditSkill";

                        var skillKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                        new KeyboardButton[]
                        {
                            isUk ? "Початківець" : "Beginner",
                            isUk ? "Середній" : "Intermediate",
                            isUk ? "Професіонал" : "Professional"
                        }
                    })
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true
                        };

                        await bot.SendMessage(
                            chatId,
                            isUk ? "⛷️ Оберіть ваш *Рівень катання*:" : "⛷️ Choose your *Skill Level*:",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: skillKeyboard,
                            cancellationToken: ct
                        );
                        break;
                    }

                case "EditSkill":
                    {
                        if (!session.LoggedInAccountId.HasValue)
                        {
                            session.Step = "None";
                            return;
                        }

                        var acc = GetAccountById(session.LoggedInAccountId.Value);
                        if (acc != null)
                        {
                            acc.FullName = session.Draft.PendingFullName ?? acc.FullName;
                            acc.Phone = session.Draft.PendingPhone ?? acc.Phone;
                            acc.Email = session.Draft.PendingEmail ?? acc.Email;
                            acc.Height = session.Draft.PendingHeight;
                            acc.Weight = session.Draft.PendingWeight;
                            acc.ShoeSize = session.Draft.PendingShoeSize;
                            acc.SkillLevel = NormalizeSkillFromInput(text);
                            acc.LastChatId = chatId;
                            UpdateBotAccount(acc);
                        }

                        session.Step = "None";
                        await SendAccountMenu(
                            bot, chatId, lang,
                            isUk
                                ? "✅ *Параметри успішно збережено в базі даних!*"
                                : "✅ *Parameters successfully saved to the database!*",
                            ct
                        );
                        break;
                    }

                case "OrderDays":
                    {
                        if (!int.TryParse(text, out int days) || days < 1 || days > 14)
                        {
                            await bot.SendMessage(
                                chatId,
                                isUk ? "❌ Введіть число від *1* до *14*:" : "❌ Enter a number from *1* to *14*:",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: ct
                            );
                            return;
                        }

                        session.Draft.Days = days;
                        session.Step = "OrderEquipment";

                        var eqKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                        new KeyboardButton[] { GetBtn(lang, "RentEquip"), GetBtn(lang, "OwnEquip") }
                    })
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true
                        };

                        await bot.SendMessage(
                            chatId,
                            isUk
                                ? "🎿 *Крок 2/5 — Спорядження*\nОберіть варіант:"
                                : "🎿 *Step 2/5 — Equipment*\nChoose an option:",
                            parseMode: ParseMode.Markdown,
                            replyMarkup: eqKeyboard,
                            cancellationToken: ct
                        );
                        break;
                    }

                case "OrderEquipment":
                    {
                        if (text == GetBtn(lang, "OwnEquip") || text == GetBtn("uk", "OwnEquip") || text == GetBtn("en", "OwnEquip"))
                        {
                            session.Draft.OwnEquipment = true;
                        }
                        else if (text == GetBtn(lang, "RentEquip") || text == GetBtn("uk", "RentEquip") || text == GetBtn("en", "RentEquip"))
                        {
                            session.Draft.OwnEquipment = false;
                        }
                        else
                        {
                            await bot.SendMessage(
                                chatId,
                                isUk ? "Оберіть варіант спорядження з кнопок нижче." : "Choose an equipment option from the buttons below.",
                                cancellationToken: ct
                            );
                            return;
                        }

                        session.Step = "OrderHotel";
                        await SendYesNoKeyboard(
                            bot, chatId, lang,
                            isUk
                                ? "🏨 *Крок 3/5 — Номер у готелі (+2500 грн/день)?*"
                                : "🏨 *Step 3/5 — Hotel room (+2500 UAH/day)?*",
                            ct
                        );
                        break;
                    }

                case "OrderHotel":
                    {
                        if (!MatchesBtn(text, lang, "Yes") && !MatchesBtn(text, lang, "No"))
                        {
                            await SendYesNoKeyboard(bot, chatId, lang, isUk ? "Оберіть *Так* або *Ні*:" : "Choose *Yes* or *No*:", ct);
                            return;
                        }

                        session.Draft.HotelRoom = MatchesBtn(text, lang, "Yes");
                        session.Step = "OrderBreakfast";
                        await SendYesNoKeyboard(
                            bot, chatId, lang,
                            isUk
                                ? "🍳 *Крок 4/5 — Сніданок (+300 грн/день)?*"
                                : "🍳 *Step 4/5 — Breakfast (+300 UAH/day)?*",
                            ct
                        );
                        break;
                    }

                case "OrderBreakfast":
                    {
                        if (!MatchesBtn(text, lang, "Yes") && !MatchesBtn(text, lang, "No"))
                        {
                            await SendYesNoKeyboard(bot, chatId, lang, isUk ? "Оберіть *Так* або *Ні*:" : "Choose *Yes* or *No*:", ct);
                            return;
                        }

                        session.Draft.Breakfast = MatchesBtn(text, lang, "Yes");
                        session.Step = "OrderSauna";
                        await SendYesNoKeyboard(
                            bot, chatId, lang,
                            isUk
                                ? "🔥 *Крок 5/5 — Сауна (+500 грн)?*"
                                : "🔥 *Step 5/5 — Sauna (+500 UAH)?*",
                            ct
                        );
                        break;
                    }

                case "OrderSauna":
                    {
                        if (!MatchesBtn(text, lang, "Yes") && !MatchesBtn(text, lang, "No"))
                        {
                            await SendYesNoKeyboard(bot, chatId, lang, isUk ? "Оберіть *Так* або *Ні*:" : "Choose *Yes* or *No*:", ct);
                            return;
                        }

                        session.Draft.Sauna = MatchesBtn(text, lang, "Yes");
                        session.Draft.TotalPrice = CalculateDraftTotal(session.Draft);
                        session.Step = "OrderConfirm";

                        string yesStr = isUk ? "Так" : "Yes";
                        string noStr = isUk ? "Ні" : "No";
                        string eqStr = session.Draft.OwnEquipment
                            ? (isUk ? "Власне" : "Own")
                            : (isUk ? "Прокат" : "Rental");

                        string summary = isUk
                            ? $"🧾 *Підтвердження замовлення*\n\n" +
                              $"• Днів: *{session.Draft.Days}*\n" +
                              $"• Спорядження: *{eqStr}*\n" +
                              $"• Готель: *{(session.Draft.HotelRoom ? yesStr : noStr)}*\n" +
                              $"• Сніданок: *{(session.Draft.Breakfast ? yesStr : noStr)}*\n" +
                              $"• Сауна: *{(session.Draft.Sauna ? yesStr : noStr)}*\n" +
                              $"• До сплати: *{session.Draft.TotalPrice} грн*\n\n" +
                              $"*Ви дійсно хочете замовити цей ордер?*"
                            : $"🧾 *Order Confirmation*\n\n" +
                              $"• Days: *{session.Draft.Days}*\n" +
                              $"• Equipment: *{eqStr}*\n" +
                              $"• Hotel: *{(session.Draft.HotelRoom ? yesStr : noStr)}*\n" +
                              $"• Breakfast: *{(session.Draft.Breakfast ? yesStr : noStr)}*\n" +
                              $"• Sauna: *{(session.Draft.Sauna ? yesStr : noStr)}*\n" +
                              $"• Total: *{session.Draft.TotalPrice} UAH*\n\n" +
                              $"*Do you really want to place this order?*";

                        await SendYesNoKeyboard(bot, chatId, lang, summary, ct);
                        break;
                    }

                case "OrderConfirm":
                    {
                        if (MatchesBtn(text, lang, "No"))
                        {
                            session.Step = "None";
                            await SendAccountMenu(
                                bot, chatId, lang,
                                isUk ? "❌ Замовлення скасовано." : "❌ Order cancelled.",
                                ct
                            );
                            return;
                        }

                        if (!MatchesBtn(text, lang, "Yes"))
                        {
                            await SendYesNoKeyboard(
                                bot, chatId, lang,
                                isUk ? "Оберіть *«Так»* для підтвердження або *«Ні»* для скасування:" : "Choose *«Yes»* to confirm or *«No»* to cancel:",
                                ct
                            );
                            return;
                        }

                        if (!session.LoggedInAccountId.HasValue)
                        {
                            session.Step = "None";
                            return;
                        }

                        var acc = GetAccountById(session.LoggedInAccountId.Value);
                        if (acc == null)
                        {
                            session.LoggedInAccountId = null;
                            session.Step = "None";
                            return;
                        }

                        acc.LastChatId = chatId;
                        UpdateBotAccount(acc);

                        string clientName = string.IsNullOrWhiteSpace(acc.FullName) ? acc.Username : acc.FullName.Trim();
                        string phone = string.IsNullOrWhiteSpace(acc.Phone) ? "+380000000000" : acc.Phone.Trim();
                        string email = string.IsNullOrWhiteSpace(acc.Email) ? $"{acc.Username.ToLower()}@skihotel.local" : acc.Email.Trim();
                        string skill = NormalizeSkillFromInput(acc.SkillLevel);

                        Order order = new Order
                        {
                            ClientName = clientName,
                            Phone = phone,
                            Email = email,
                            HasEquipment = session.Draft.OwnEquipment ? "Yes" : "No",
                            Weight = acc.Weight + " kg",
                            Height = acc.Height + " cm",
                            ShoeSize = acc.ShoeSize.ToString(),
                            SkillLevel = skill,
                            SkiPassDays = session.Draft.Days,
                            HotelRoom = session.Draft.HotelRoom ? "Yes" : "No",
                            Breakfast = session.Draft.Breakfast,
                            Sauna = session.Draft.Sauna,
                            TotalPrice = session.Draft.TotalPrice,
                            Status = "Incoming",
                            BotChatId = chatId
                        };

                        if (session.Draft.OwnEquipment)
                        {
                            order.Skis = "Own";
                            order.Boots = "Own";
                            order.Poles = "Own";
                        }
                        else
                        {
                            order.Skis = EquipmentService.GetSkiLength(acc.Height, skill);
                            order.Boots = EquipmentService.GetBootSize(acc.ShoeSize);
                            order.Poles = EquipmentService.GetPoleLength(acc.Height);
                        }

                        OrderService.SaveOrder(order);
                        LastOrderTimeByChat[chatId] = DateTime.Now;
                        session.Step = "None";

                        await SendNewOrderNotificationAsync(order);

                        decimal depositAmount = Math.Round(order.TotalPrice * 0.30m, 2);

                        string paymentMessage = isUk
                            ? $"✅ *Заявку #{order.Id} успішно надіслано!*\n\n" +
                              $"💳 *Реквізити для оплати / передплати:*\n" +
                              $"• IBAN: `UA213052990000026007015598412`\n" +
                              $"• Отримувач: *ТОВ «СКІ ГОТЕЛЬ КАРПАТИ»*\n" +
                              $"• ЄДРПОУ: `44918273`\n" +
                              $"• Призначення платежу: *Бронювання #{order.Id} ({order.ClientName})*\n\n" +
                              $"💰 *Сума до сплати:* *{order.TotalPrice} грн* (або передплата від *{depositAmount} грн*)\n\n" +
                              $"⏳ *Очікуйте підтвердження:* після здійснення оплати адміністратор перевірить замовлення в системі SkiHotel та підтвердить його. Ви отримаєте сповіщення тут одразу після підтвердження.\n\n" +
                              $"🔒 Кнопка замовлення тимчасово заблокована на *{OrderCooldownSeconds} сек.* від повторних натискань."
                            : $"✅ *Booking request #{order.Id} submitted!*\n\n" +
                              $"💳 *Payment / Deposit details:*\n" +
                              $"• IBAN: `UA213052990000026007015598412`\n" +
                              $"• Recipient: *SKI HOTEL CARPATHIANS LLC*\n" +
                              $"• Tax ID: `44918273`\n" +
                              $"• Reference: *Booking #{order.Id} ({order.ClientName})*\n\n" +
                              $"💰 *Amount due:* *{order.TotalPrice} UAH* (or deposit from *{depositAmount} UAH*)\n\n" +
                              $"⏳ *Waiting for confirmation:* once paid, the administrator will review and confirm your booking in SkiHotel, and you will receive a notification here.\n\n" +
                              $"🔒 The order button is temporarily locked for *{OrderCooldownSeconds} sec.* against duplicate submissions.";

                        await SendAccountMenu(bot, chatId, lang, paymentMessage, ct);
                        break;
                    }
            }
        }

        #endregion

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

            var session = Sessions.GetOrAdd(chatId, id => new BotSession { ChatId = id, Language = "uk" });

            if (callbackQuery.Data.StartsWith("lang_"))
            {
                string selectedLang = callbackQuery.Data.Replace("lang_", "");
                if (selectedLang != "uk" && selectedLang != "en") selectedLang = "uk";

                session.Language = selectedLang;

                if (session.LoggedInAccountId.HasValue)
                {
                    var acc = GetAccountById(session.LoggedInAccountId.Value);
                    if (acc != null)
                    {
                        acc.Language = selectedLang;
                        acc.LastChatId = chatId;
                        UpdateBotAccount(acc);
                    }
                }

                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
                await bot.EditMessageReplyMarkup(chatId, messageId, replyMarkup: null, cancellationToken: cancellationToken);

                string langMsg = selectedLang == "en"
                    ? "Language successfully changed to English! 🇬🇧"
                    : "Мову успішно змінено на Українську! 🇺🇦";

                await bot.SendMessage(chatId, langMsg, cancellationToken: cancellationToken);

                if (session.LoggedInAccountId.HasValue)
                {
                    await SendAccountMenu(
                        bot, chatId, selectedLang,
                        selectedLang == "en" ? "Select an action from the menu:" : "Оберіть дію з меню:",
                        cancellationToken
                    );
                }
                else
                {
                    await SendGuestMenu(
                        bot, chatId, selectedLang,
                        selectedLang == "en" ? "Select an action from the menu:" : "Оберіть дію з меню:",
                        cancellationToken
                    );
                }
            }
        }

        private static Task HandleErrorAsync(
            ITelegramBotClient bot,
            Exception exception,
            HandleErrorSource source,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("========== TELEGRAM BOT ERROR ==========");
            Console.WriteLine($"Source: {source}");
            Console.WriteLine($"Error: {exception}");
            Console.WriteLine("========================================");
            Console.WriteLine(exception.ToString());
            return Task.CompletedTask;
        }
    }
}