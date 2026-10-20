using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SkiHotel.Models;

namespace SkiHotel.Services
{
    public static class OrderService
    {
        private static readonly string DataFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        private static readonly string FilePath =
            Path.Combine(DataFolder, "orders.txt");

        private const string Separator = "========================================";

        public static void SaveOrder(Order order)
        {
            Directory.CreateDirectory(DataFolder);

            List<Order> orders = GetAllOrders();

            order.Id = orders.Count == 0
                ? 1
                : orders.Max(x => x.Id) + 1;

            order.CreatedAt = DateTime.Now;

            using (StreamWriter writer = new StreamWriter(FilePath, true))
            {
                writer.WriteLine(Separator);
                writer.WriteLine("ЗАМОВЛЕННЯ");
                writer.WriteLine("ID: " + order.Id);
                writer.WriteLine("Дата: " + order.CreatedAt);
                writer.WriteLine("Клієнт: " + order.ClientName);
                writer.WriteLine("Телефон: " + order.Phone);
                writer.WriteLine("Email: " + order.Email);
                writer.WriteLine("Власне спорядження: " +
                    (order.HasOwnEquipment ? "Так" : "Ні"));

                if (!order.HasOwnEquipment)
                {
                    writer.WriteLine("Вага: " + order.Weight + " кг");
                    writer.WriteLine("Зріст: " + order.Height + " см");
                    writer.WriteLine("Розмір взуття: " + order.ShoeSize);
                    writer.WriteLine("Рівень: " + order.SkillLevel);
                    writer.WriteLine("Лижі: " + order.Skis);
                    writer.WriteLine("Черевики: " + order.Boots);
                    writer.WriteLine("Палиці: " + order.Poles);
                }

                writer.WriteLine("Днів абонемента: " + order.SkiPassDays);
                writer.WriteLine("Номер у готелі: " +
                    (order.HotelRoom ? "Так" : "Ні"));
                writer.WriteLine("Сніданок: " +
                    (order.Breakfast ? "Так" : "Ні"));
                writer.WriteLine("Сауна: " +
                    (order.Sauna ? "Так" : "Ні"));
                writer.WriteLine("Вартість: " + order.TotalPrice + " грн");
                writer.WriteLine("Статус: " + order.Status);
                writer.WriteLine();
            }
        }

        public static List<Order> GetAllOrders()
        {
            List<Order> orders = new List<Order>();

            if (!File.Exists(FilePath))
            {
                return orders;
            }

            string content = File.ReadAllText(FilePath);

            if (string.IsNullOrWhiteSpace(content))
            {
                return orders;
            }

            string[] blocks = content.Split(
                new string[] { Separator },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string block in blocks)
            {
                if (!block.Contains("Клієнт:"))
                {
                    continue;
                }

                Order order = new Order();

                order.Id = ReadInt(block, "ID:", 0);

                string dateText = ReadValue(block, "Дата:");
                DateTime createdAt;

                if (DateTime.TryParse(dateText, out createdAt))
                {
                    order.CreatedAt = createdAt;
                }

                order.ClientName = ReadValue(block, "Клієнт:");
                order.Phone = ReadValue(block, "Телефон:");
                order.Email = ReadValue(block, "Email:");

                order.HasOwnEquipment =
                    ReadValue(block, "Власне спорядження:") == "Так";

                order.Weight = ReadDouble(block, "Вага:", 0);
                order.Height = ReadDouble(block, "Зріст:", 0);
                order.ShoeSize = ReadInt(block, "Розмір взуття:", 0);
                order.SkillLevel = ReadValue(block, "Рівень:");

                order.Skis = ReadValue(block, "Лижі:");
                order.Boots = ReadValue(block, "Черевики:");
                order.Poles = ReadValue(block, "Палиці:");

                order.SkiPassDays =
                    ReadInt(block, "Днів абонемента:", 0);

                order.HotelRoom =
                    ReadValue(block, "Номер у готелі:") == "Так";

                order.Breakfast =
                    ReadValue(block, "Сніданок:") == "Так";

                order.Sauna =
                    ReadValue(block, "Сауна:") == "Так";

                order.TotalPrice = ReadDecimal(block, "Вартість:", 0);

                order.Status = ReadValue(block, "Статус:");

                if (string.IsNullOrWhiteSpace(order.Status))
                {
                    order.Status = "Заброньовано";
                }

                orders.Add(order);
            }

            return orders.OrderBy(x => x.Id).ToList();
        }

        private static string ReadValue(string block, string label)
        {
            string[] lines = block.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.None);

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith(
                    label,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return trimmedLine.Substring(label.Length).Trim();
                }
            }

            return "";
        }

        private static int ReadInt(
            string block,
            string label,
            int defaultValue)
        {
            string value = ReadValue(block, label);
            int result;

            if (int.TryParse(value, out result))
            {
                return result;
            }

            return defaultValue;
        }

        private static double ReadDouble(
            string block,
            string label,
            double defaultValue)
        {
            string value = ReadValue(block, label);
            value = value.Replace("кг", "").Replace("см", "").Trim();

            double result;

            if (double.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.CurrentCulture,
                out result))
            {
                return result;
            }

            if (double.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out result))
            {
                return result;
            }

            return defaultValue;
        }

        private static decimal ReadDecimal(
            string block,
            string label,
            decimal defaultValue)
        {
            string value = ReadValue(block, label);
            value = value.Replace("грн", "").Trim();

            decimal result;

            if (decimal.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.CurrentCulture,
                out result))
            {
                return result;
            }

            if (decimal.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out result))
            {
                return result;
            }

            return defaultValue;
        }
    }
}