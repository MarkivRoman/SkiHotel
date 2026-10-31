using System;
using System.Collections.Generic;
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
                if (DateTime.TryParse(dateText, out DateTime createdAt))
                {
                    order.CreatedAt = createdAt;
                }

                order.ClientName = ReadValue(block, "Клієнт:");
                order.Phone = ReadValue(block, "Телефон:");
                order.Email = ReadValue(block, "Email:");
                order.HasEquipment = ReadValue(block, "Власне спорядження:");
                order.Weight = ReadValue(block, "Вага:");
                order.Height = ReadValue(block, "Зріст:");
                order.ShoeSize = ReadValue(block, "Розмір взуття:");

                string level = ReadValue(block, "Рівень:");
                order.SkillLevel = level;
                order.Level = level;

                order.Skis = ReadValue(block, "Лижі:");
                order.Boots = ReadValue(block, "Черевики:");
                order.Poles = ReadValue(block, "Палиці:");

                int days = ReadInt(block, "Днів абонемента:", 1);
                order.SkiPassDays = days;
                order.Days = days;

                order.HotelRoom = ReadValue(block, "Номер у готелі:");

                string breakfastStr = ReadValue(block, "Сніданок:");
                order.Breakfast = (breakfastStr == "Так" || breakfastStr == "True");

                string saunaStr = ReadValue(block, "Сауна:");
                order.Sauna = (saunaStr == "Так" || saunaStr == "True");

                string costStr = ReadValue(block, "Вартість:");
                order.Cost = costStr;
                if (decimal.TryParse(costStr.Replace(" грн", "").Trim(), out decimal price))
                {
                    order.TotalPrice = price;
                }

                string status = ReadValue(block, "Статус:");
                order.Status = string.IsNullOrWhiteSpace(status) ? "Заброньовано" : status;

                orders.Add(order);
            }

            return orders.OrderBy(x => x.Id).ToList();
        }

        public static void SaveOrder(Order order)
        {
            var orders = GetAllOrders();
            order.Id = orders.Count > 0 ? orders.Max(o => o.Id) + 1 : 1;
            order.CreatedAt = DateTime.Now;
            orders.Add(order);
            SaveAllOrders(orders);
        }

        public static void UpdateOrderStatus(int orderId, string newStatus)
        {
            var orders = GetAllOrders();
            var target = orders.FirstOrDefault(o => o.Id == orderId);
            if (target != null)
            {
                target.Status = newStatus;
                SaveAllOrders(orders);
            }
        }

        public static void DeleteOrder(int orderId)
        {
            var orders = GetAllOrders();
            var target = orders.FirstOrDefault(o => o.Id == orderId);
            if (target != null)
            {
                orders.Remove(target);
                SaveAllOrders(orders);
            }
        }

        private static void SaveAllOrders(List<Order> orders)
        {
            Directory.CreateDirectory(DataFolder);

            using (StreamWriter writer = new StreamWriter(FilePath, false))
            {
                foreach (var order in orders)
                {
                    writer.WriteLine(Separator);
                    writer.WriteLine("ЗАМОВЛЕННЯ");
                    writer.WriteLine("ID: " + order.Id);
                    writer.WriteLine("Дата: " + order.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss"));
                    writer.WriteLine("Клієнт: " + order.ClientName);
                    writer.WriteLine("Телефон: " + order.Phone);
                    writer.WriteLine("Email: " + order.Email);
                    writer.WriteLine("Власне спорядження: " + order.HasEquipment);
                    writer.WriteLine("Вага: " + order.Weight);
                    writer.WriteLine("Зріст: " + order.Height);
                    writer.WriteLine("Розмір взуття: " + order.ShoeSize);
                    writer.WriteLine("Рівень: " + order.SkillLevel);
                    writer.WriteLine("Лижі: " + order.Skis);
                    writer.WriteLine("Черевики: " + order.Boots);
                    writer.WriteLine("Палиці: " + order.Poles);
                    writer.WriteLine("Днів абонемента: " + order.SkiPassDays);
                    writer.WriteLine("Номер у готелі: " + order.HotelRoom);
                    writer.WriteLine("Сніданок: " + (order.Breakfast ? "Так" : "Ні"));
                    writer.WriteLine("Сауна: " + (order.Sauna ? "Так" : "Ні"));
                    writer.WriteLine("Вартість: " + order.Cost);
                    writer.WriteLine("Статус: " + order.Status);
                    writer.WriteLine();
                }
            }
        }

        private static string ReadValue(string block, string label)
        {
            string[] lines = block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(label, StringComparison.OrdinalIgnoreCase))
                {
                    return trimmedLine.Substring(label.Length).Trim();
                }
            }
            return "";
        }

        private static int ReadInt(string block, string label, int defaultValue)
        {
            string value = ReadValue(block, label);
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}