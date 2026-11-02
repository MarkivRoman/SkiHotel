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
                if (!block.Contains("Client:") && !block.Contains("Клієнт:"))
                {
                    continue;
                }

                Order order = new Order();
                order.Id = ReadInt(block, "ID:", "ID:", 0);

                string dateText = ReadValue(block, "Date:", "Дата:");
                if (DateTime.TryParse(dateText, out DateTime createdAt))
                {
                    order.CreatedAt = createdAt;
                }

                order.ClientName = ReadValue(block, "Client:", "Клієнт:");
                order.Phone = ReadValue(block, "Phone:", "Телефон:");
                order.Email = ReadValue(block, "Email:", "Email:");
                order.HasEquipment = ReadValue(block, "Own equipment:", "Власне спорядження:");
                order.Weight = ReadValue(block, "Weight:", "Вага:");
                order.Height = ReadValue(block, "Height:", "Зріст:");
                order.ShoeSize = ReadValue(block, "Shoe size:", "Розмір взуття:");

                string level = ReadValue(block, "Skill level:", "Рівень:");
                order.SkillLevel = level;
                order.Level = level;

                order.Skis = ReadValue(block, "Skis:", "Лижі:");
                order.Boots = ReadValue(block, "Boots:", "Черевики:");
                order.Poles = ReadValue(block, "Poles:", "Палиці:");

                int days = ReadInt(block, "Ski pass days:", "Днів абонемента:", 1);
                order.SkiPassDays = days;
                order.Days = days;

                order.HotelRoom = ReadValue(block, "Hotel room:", "Номер у готелі:");

                string breakfastStr = ReadValue(block, "Breakfast:", "Сніданок:");
                order.Breakfast = (breakfastStr == "Yes" || breakfastStr == "Так" || breakfastStr == "True");

                string saunaStr = ReadValue(block, "Sauna:", "Сауна:");
                order.Sauna = (saunaStr == "Yes" || saunaStr == "Так" || saunaStr == "True");

                string costStr = ReadValue(block, "Cost:", "Вартість:");
                order.Cost = costStr;
                if (decimal.TryParse(costStr.Replace(" UAH", "").Replace(" грн", "").Trim(), out decimal price))
                {
                    order.TotalPrice = price;
                }

                string status = ReadValue(block, "Status:", "Статус:");
                order.Status = string.IsNullOrWhiteSpace(status) ? "Booked" : status;

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
                    writer.WriteLine("ORDER");
                    writer.WriteLine("ID: " + order.Id);
                    writer.WriteLine("Date: " + order.CreatedAt.ToString("dd.MM.yyyy HH:mm:ss"));
                    writer.WriteLine("Client: " + order.ClientName);
                    writer.WriteLine("Phone: " + order.Phone);
                    writer.WriteLine("Email: " + order.Email);
                    writer.WriteLine("Own equipment: " + order.HasEquipment);
                    writer.WriteLine("Weight: " + order.Weight);
                    writer.WriteLine("Height: " + order.Height);
                    writer.WriteLine("Shoe size: " + order.ShoeSize);
                    writer.WriteLine("Skill level: " + order.SkillLevel);
                    writer.WriteLine("Skis: " + order.Skis);
                    writer.WriteLine("Boots: " + order.Boots);
                    writer.WriteLine("Poles: " + order.Poles);
                    writer.WriteLine("Ski pass days: " + order.SkiPassDays);
                    writer.WriteLine("Hotel room: " + order.HotelRoom);
                    writer.WriteLine("Breakfast: " + (order.Breakfast ? "Yes" : "No"));
                    writer.WriteLine("Sauna: " + (order.Sauna ? "Yes" : "No"));
                    writer.WriteLine("Cost: " + order.Cost);
                    writer.WriteLine("Status: " + order.Status);
                    writer.WriteLine();
                }
            }
        }

        private static string ReadValue(string block, params string[] labels)
        {
            string[] lines = block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                foreach (string label in labels)
                {
                    if (trimmedLine.StartsWith(label, StringComparison.OrdinalIgnoreCase))
                    {
                        return trimmedLine.Substring(label.Length).Trim();
                    }
                }
            }
            return "";
        }

        private static int ReadInt(string block, string labelEn, string labelUa, int defaultValue)
        {
            string value = ReadValue(block, labelEn, labelUa);
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}