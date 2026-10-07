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
                writer.WriteLine("========================================");
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

            // Поки що метод залишаємо порожнім.
            // Пізніше, коли будемо робити DataGridView,
            // зробимо нормальне читання замовлень.

            return orders;
        }
    }
}