using System;
using System.Data.SqlClient;
using System.IO;
using SkiHotel.Models;

namespace SkiHotel
{
    public static class Database
    {
        private static string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=SkiHotelDB;Trusted_Connection=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            // 1. Створення бази даних SkiHotelDB через master
            string masterConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;";
            using (var masterConn = new SqlConnection(masterConnectionString))
            {
                masterConn.Open();
                string createDbQuery = "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SkiHotelDB') CREATE DATABASE SkiHotelDB;";
                using (var cmd = new SqlCommand(createDbQuery, masterConn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // 2. Створення всіх таблиць у SkiHotelDB
            using (var conn = GetConnection())
            {
                conn.Open();

                // Таблиця Users
                string createUsersTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                    BEGIN
                        CREATE TABLE Users (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Username NVARCHAR(100) UNIQUE,
                            PasswordHash NVARCHAR(256),
                            Role NVARCHAR(50)
                        )
                    END";
                ExecuteNonQuery(conn, createUsersTable);

                // Таблиця Orders (повна відповідність полям orders.txt)
                string createOrdersTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
                    BEGIN
                        CREATE TABLE Orders (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            CreatedAt DATETIME NOT NULL,
                            ClientName NVARCHAR(200),
                            Phone NVARCHAR(100),
                            Email NVARCHAR(150),
                            HasEquipment NVARCHAR(50),
                            Weight NVARCHAR(50),
                            Height NVARCHAR(50),
                            ShoeSize NVARCHAR(50),
                            SkillLevel NVARCHAR(100),
                            Skis NVARCHAR(100),
                            Boots NVARCHAR(100),
                            Poles NVARCHAR(100),
                            SkiPassDays INT,
                            HotelRoom NVARCHAR(50),
                            Breakfast BIT,
                            Sauna BIT,
                            TotalPrice DECIMAL(18, 2),
                            Status NVARCHAR(50)
                        )
                    END";
                ExecuteNonQuery(conn, createOrdersTable);

                // Таблиця Reviews (повна відповідність полям reviews.txt)
                string createReviewsTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reviews')
                    BEGIN
                        CREATE TABLE Reviews (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            CreatedAt DATETIME NOT NULL,
                            ClientName NVARCHAR(200),
                            Rating INT,
                            Comment NVARCHAR(MAX)
                        )
                    END";
                ExecuteNonQuery(conn, createReviewsTable);

                // Таблиця EquipmentInventory (повна відповідність структурі equipment.txt)
                string createEquipmentTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EquipmentInventory')
                    BEGIN
                        CREATE TABLE EquipmentInventory (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Category NVARCHAR(150),
                            SubCategory NVARCHAR(150),
                            ItemName NVARCHAR(200),
                            Quantity INT
                        )
                    END";
                ExecuteNonQuery(conn, createEquipmentTable);

                // 3. Початкове заповнення користувачів
                SeedUsers(conn);

                // 4. Одноразова міграція даних зі старих .txt файлів (якщо таблиці порожні)
                MigrateLegacyEquipment(conn);
                MigrateLegacyOrders(conn);
                MigrateLegacyReviews(conn);
            }
        }

        private static void ExecuteNonQuery(SqlConnection conn, string query)
        {
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void SeedUsers(SqlConnection conn)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users", conn))
            {
                if ((int)cmd.ExecuteScalar() == 0)
                {
                    string passMarkiv = SecurityHelper.HashPassword("markiv123");
                    string passMuhalchuk = SecurityHelper.HashPassword("muhalchuk123");
                    string passArendach = SecurityHelper.HashPassword("arendach123");

                    string insertQuery = $@"
                        INSERT INTO Users (Username, PasswordHash, Role) VALUES ('Markiv', '{passMarkiv}', 'Admin');
                        INSERT INTO Users (Username, PasswordHash, Role) VALUES ('Muhalchuk', '{passMuhalchuk}', 'User');
                        INSERT INTO Users (Username, PasswordHash, Role) VALUES ('Arendach', '{passArendach}', 'User');
                    ";
                    ExecuteNonQuery(conn, insertQuery);
                }
            }
        }

        private static void MigrateLegacyEquipment(SqlConnection conn)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM EquipmentInventory", conn))
            {
                if ((int)cmd.ExecuteScalar() > 0) return;
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "equipment.txt");
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split('|');
                    if (parts.Length >= 4 && int.TryParse(parts[3].Trim(), out int qty))
                    {
                        string query = "INSERT INTO EquipmentInventory (Category, SubCategory, ItemName, Quantity) VALUES (@c, @s, @n, @q)";
                        using (var insertCmd = new SqlCommand(query, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@c", parts[0].Trim());
                            insertCmd.Parameters.AddWithValue("@s", parts[1].Trim());
                            insertCmd.Parameters.AddWithValue("@n", parts[2].Trim());
                            insertCmd.Parameters.AddWithValue("@q", qty);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            else
            {
                // Мінімальний шаблон, якщо файлу не було
                string query = "INSERT INTO EquipmentInventory (Category, SubCategory, ItemName, Quantity) VALUES ('Skies and Poles', 'Beginner', 'Sample Ski Item', 10)";
                ExecuteNonQuery(conn, query);
            }
        }

        private static void MigrateLegacyOrders(SqlConnection conn)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Orders", conn))
            {
                if ((int)cmd.ExecuteScalar() > 0) return;
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "orders.txt");
            if (!File.Exists(filePath)) return;

            string content = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(content)) return;

            string[] blocks = content.Split(new[] { "========================================" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string block in blocks)
            {
                if (!block.Contains("Client:") && !block.Contains("Клієнт:")) continue;

                DateTime createdAt = DateTime.TryParse(ReadLegacyValue(block, "Date:", "Дата:"), out DateTime dt) ? dt : DateTime.Now;
                string client = ReadLegacyValue(block, "Client:", "Клієнт:");
                string phone = ReadLegacyValue(block, "Phone:", "Телефон:");
                string email = ReadLegacyValue(block, "Email:", "Email:");
                string hasEq = ReadLegacyValue(block, "Own equipment:", "Власне спорядження:");
                string weight = ReadLegacyValue(block, "Weight:", "Вага:");
                string height = ReadLegacyValue(block, "Height:", "Зріст:");
                string shoe = ReadLegacyValue(block, "Shoe size:", "Розмір взуття:");
                string level = ReadLegacyValue(block, "Skill level:", "Рівень:");
                string skis = ReadLegacyValue(block, "Skis:", "Лижі:");
                string boots = ReadLegacyValue(block, "Boots:", "Черевики:");
                string poles = ReadLegacyValue(block, "Poles:", "Палиці:");
                int days = int.TryParse(ReadLegacyValue(block, "Ski pass days:", "Днів абонемента:"), out int d) ? d : 1;
                string room = ReadLegacyValue(block, "Hotel room:", "Номер у готелі:");
                string brStr = ReadLegacyValue(block, "Breakfast:", "Сніданок:");
                bool breakfast = (brStr == "Yes" || brStr == "Так" || brStr == "True");
                string saunaStr = ReadLegacyValue(block, "Sauna:", "Сауна:");
                bool sauna = (saunaStr == "Yes" || saunaStr == "Так" || saunaStr == "True");
                string costStr = ReadLegacyValue(block, "Cost:", "Вартість:").Replace(" UAH", "").Replace(" грн", "").Trim();
                decimal price = decimal.TryParse(costStr, out decimal p) ? p : 0;
                string status = ReadLegacyValue(block, "Status:", "Статус:");
                if (string.IsNullOrWhiteSpace(status)) status = "Booked";

                string query = @"INSERT INTO Orders 
                    (CreatedAt, ClientName, Phone, Email, HasEquipment, Weight, Height, ShoeSize, SkillLevel, Skis, Boots, Poles, SkiPassDays, HotelRoom, Breakfast, Sauna, TotalPrice, Status) 
                    VALUES (@CreatedAt, @ClientName, @Phone, @Email, @HasEquipment, @Weight, @Height, @ShoeSize, @SkillLevel, @Skis, @Boots, @Poles, @SkiPassDays, @HotelRoom, @Breakfast, @Sauna, @TotalPrice, @Status)";

                using (var insertCmd = new SqlCommand(query, conn))
                {
                    insertCmd.Parameters.AddWithValue("@CreatedAt", createdAt);
                    insertCmd.Parameters.AddWithValue("@ClientName", client);
                    insertCmd.Parameters.AddWithValue("@Phone", phone);
                    insertCmd.Parameters.AddWithValue("@Email", email);
                    insertCmd.Parameters.AddWithValue("@HasEquipment", hasEq);
                    insertCmd.Parameters.AddWithValue("@Weight", weight);
                    insertCmd.Parameters.AddWithValue("@Height", height);
                    insertCmd.Parameters.AddWithValue("@ShoeSize", shoe);
                    insertCmd.Parameters.AddWithValue("@SkillLevel", level);
                    insertCmd.Parameters.AddWithValue("@Skis", skis);
                    insertCmd.Parameters.AddWithValue("@Boots", boots);
                    insertCmd.Parameters.AddWithValue("@Poles", poles);
                    insertCmd.Parameters.AddWithValue("@SkiPassDays", days);
                    insertCmd.Parameters.AddWithValue("@HotelRoom", room);
                    insertCmd.Parameters.AddWithValue("@Breakfast", breakfast);
                    insertCmd.Parameters.AddWithValue("@Sauna", sauna);
                    insertCmd.Parameters.AddWithValue("@TotalPrice", price);
                    insertCmd.Parameters.AddWithValue("@Status", status);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private static void MigrateLegacyReviews(SqlConnection conn)
        {
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Reviews", conn))
            {
                if ((int)cmd.ExecuteScalar() > 0) return;
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "reviews.txt");
            if (!File.Exists(filePath)) return;

            string content = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(content)) return;

            string[] blocks = content.Split(new[] { "========================================" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string block in blocks)
            {
                if (!block.Contains("Client:") && !block.Contains("Клієнт:")) continue;

                DateTime createdAt = DateTime.TryParse(ReadLegacyValue(block, "Date:", "Дата:"), out DateTime dt) ? dt : DateTime.Now;
                string client = ReadLegacyValue(block, "Client:", "Клієнт:");
                string ratingStr = ReadLegacyValue(block, "Rating:", "Оцінка:");
                if (ratingStr.Contains("/")) ratingStr = ratingStr.Split('/')[0].Trim();
                int rating = int.TryParse(ratingStr, out int r) ? r : 5;
                string comment = ReadLegacyValue(block, "Comment:", "Коментар:");

                string query = "INSERT INTO Reviews (CreatedAt, ClientName, Rating, Comment) VALUES (@CreatedAt, @ClientName, @Rating, @Comment)";
                using (var insertCmd = new SqlCommand(query, conn))
                {
                    insertCmd.Parameters.AddWithValue("@CreatedAt", createdAt);
                    insertCmd.Parameters.AddWithValue("@ClientName", client);
                    insertCmd.Parameters.AddWithValue("@Rating", rating);
                    insertCmd.Parameters.AddWithValue("@Comment", comment);
                    insertCmd.ExecuteNonQuery();
                }
            }
        }

        private static string ReadLegacyValue(string block, params string[] labels)
        {
            string[] lines = block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                foreach (string label in labels)
                {
                    if (trimmed.StartsWith(label, StringComparison.OrdinalIgnoreCase))
                        return trimmed.Substring(label.Length).Trim();
                }
            }
            return "";
        }
    }
}