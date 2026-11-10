using System;
using System.Data.SqlClient;

namespace SkiHotel
{
    public static class Database
    {
        // Головний рядок підключення до нашої бази SkiHotelDB
        private static string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=SkiHotelDB;Trusted_Connection=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            // Крок 1. Підключаємось до системної бази master, щоб гарантовано створити SkiHotelDB
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

            // Крок 2. Підключаємось до SkiHotelDB і створюємо таблицю Users
            using (var conn = GetConnection())
            {
                conn.Open();

                // Використовуємо надійну перевірку через sys.tables
                string createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                    BEGIN
                        CREATE TABLE Users (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Username NVARCHAR(100) UNIQUE,
                            PasswordHash NVARCHAR(256),
                            Role NVARCHAR(50)
                        )
                    END";

                using (var cmd = new SqlCommand(createTableQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Крок 3. Перевіряємо чи є користувачі, якщо таблиця порожня — додаємо їх
                string checkUsersQuery = "SELECT COUNT(*) FROM Users";
                using (var cmd = new SqlCommand(checkUsersQuery, conn))
                {
                    int count = (int)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        string passMarkiv = SecurityHelper.HashPassword("markiv123");
                        string passMuhalchuk = SecurityHelper.HashPassword("muhalchuk123");
                        string passArendach = SecurityHelper.HashPassword("arendach123");

                        string insertQuery = $@"
                            INSERT INTO Users (Username, PasswordHash, Role) VALUES ('Markiv', '{passMarkiv}', 'Admin');
                            INSERT INTO Users (Username, PasswordHash, Role) VALUES ('Muhalchuk', '{passMuhalchuk}', 'User');
                            INSERT INTO Users (Username, PasswordHash, Role) VALUES ('Arendach', '{passArendach}', 'User');
                        ";

                        using (var insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}