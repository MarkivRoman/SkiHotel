using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SkiHotel.Models;

namespace SkiHotel.Services
{
    public static class OrderService
    {
        public static List<Order> GetAllOrders()
        {
            List<Order> orders = new List<Order>();

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Orders ORDER BY Id ASC";
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    int botChatIdOrdinal = reader.GetOrdinal("BotChatId");

                    while (reader.Read())
                    {
                        Order order = new Order
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            ClientName = reader["ClientName"]?.ToString() ?? "",
                            Phone = reader["Phone"]?.ToString() ?? "",
                            Email = reader["Email"]?.ToString() ?? "",
                            HasEquipment = reader["HasEquipment"]?.ToString() ?? "No",
                            Weight = reader["Weight"]?.ToString() ?? "",
                            Height = reader["Height"]?.ToString() ?? "",
                            ShoeSize = reader["ShoeSize"]?.ToString() ?? "",
                            SkillLevel = reader["SkillLevel"]?.ToString() ?? "",
                            Skis = reader["Skis"]?.ToString() ?? "",
                            Boots = reader["Boots"]?.ToString() ?? "",
                            Poles = reader["Poles"]?.ToString() ?? "",
                            SkiPassDays = reader["SkiPassDays"] != DBNull.Value ? Convert.ToInt32(reader["SkiPassDays"]) : 1,
                            HotelRoom = reader["HotelRoom"]?.ToString() ?? "No",
                            Breakfast = reader["Breakfast"] != DBNull.Value && Convert.ToBoolean(reader["Breakfast"]),
                            Sauna = reader["Sauna"] != DBNull.Value && Convert.ToBoolean(reader["Sauna"]),
                            TotalPrice = reader["TotalPrice"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPrice"]) : 0m,
                            Status = reader["Status"]?.ToString() ?? "Booked",
                            BotChatId = reader.IsDBNull(botChatIdOrdinal) ? (long?)null : Convert.ToInt64(reader["BotChatId"])
                        };
                        orders.Add(order);
                    }
                }
            }

            return orders;
        }

        public static Order GetOrderById(int orderId)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Orders WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", orderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;

                        int botChatIdOrdinal = reader.GetOrdinal("BotChatId");

                        return new Order
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            ClientName = reader["ClientName"]?.ToString() ?? "",
                            Phone = reader["Phone"]?.ToString() ?? "",
                            Email = reader["Email"]?.ToString() ?? "",
                            HasEquipment = reader["HasEquipment"]?.ToString() ?? "No",
                            Weight = reader["Weight"]?.ToString() ?? "",
                            Height = reader["Height"]?.ToString() ?? "",
                            ShoeSize = reader["ShoeSize"]?.ToString() ?? "",
                            SkillLevel = reader["SkillLevel"]?.ToString() ?? "",
                            Skis = reader["Skis"]?.ToString() ?? "",
                            Boots = reader["Boots"]?.ToString() ?? "",
                            Poles = reader["Poles"]?.ToString() ?? "",
                            SkiPassDays = reader["SkiPassDays"] != DBNull.Value ? Convert.ToInt32(reader["SkiPassDays"]) : 1,
                            HotelRoom = reader["HotelRoom"]?.ToString() ?? "No",
                            Breakfast = reader["Breakfast"] != DBNull.Value && Convert.ToBoolean(reader["Breakfast"]),
                            Sauna = reader["Sauna"] != DBNull.Value && Convert.ToBoolean(reader["Sauna"]),
                            TotalPrice = reader["TotalPrice"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPrice"]) : 0m,
                            Status = reader["Status"]?.ToString() ?? "Booked",
                            BotChatId = reader.IsDBNull(botChatIdOrdinal) ? (long?)null : Convert.ToInt64(reader["BotChatId"])
                        };
                    }
                }
            }
        }

        public static void SaveOrder(Order order)
        {
            order.CreatedAt = DateTime.Now;
            if (string.IsNullOrWhiteSpace(order.Status))
            {
                order.Status = "Booked";
            }

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = @"
                    INSERT INTO Orders 
                    (CreatedAt, ClientName, Phone, Email, HasEquipment, Weight, Height, ShoeSize, SkillLevel, Skis, Boots, Poles, SkiPassDays, HotelRoom, Breakfast, Sauna, TotalPrice, Status, BotChatId) 
                    VALUES 
                    (@CreatedAt, @ClientName, @Phone, @Email, @HasEquipment, @Weight, @Height, @ShoeSize, @SkillLevel, @Skis, @Boots, @Poles, @SkiPassDays, @HotelRoom, @Breakfast, @Sauna, @TotalPrice, @Status, @BotChatId);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CreatedAt", order.CreatedAt);
                    cmd.Parameters.AddWithValue("@ClientName", order.ClientName ?? "");
                    cmd.Parameters.AddWithValue("@Phone", order.Phone ?? "");
                    cmd.Parameters.AddWithValue("@Email", order.Email ?? "");
                    cmd.Parameters.AddWithValue("@HasEquipment", order.HasEquipment ?? "No");
                    cmd.Parameters.AddWithValue("@Weight", order.Weight ?? "");
                    cmd.Parameters.AddWithValue("@Height", order.Height ?? "");
                    cmd.Parameters.AddWithValue("@ShoeSize", order.ShoeSize ?? "");
                    cmd.Parameters.AddWithValue("@SkillLevel", order.SkillLevel ?? "");
                    cmd.Parameters.AddWithValue("@Skis", order.Skis ?? "");
                    cmd.Parameters.AddWithValue("@Boots", order.Boots ?? "");
                    cmd.Parameters.AddWithValue("@Poles", order.Poles ?? "");
                    cmd.Parameters.AddWithValue("@SkiPassDays", order.SkiPassDays);
                    cmd.Parameters.AddWithValue("@HotelRoom", order.HotelRoom ?? "No");
                    cmd.Parameters.AddWithValue("@Breakfast", order.Breakfast);
                    cmd.Parameters.AddWithValue("@Sauna", order.Sauna);
                    cmd.Parameters.AddWithValue("@TotalPrice", order.TotalPrice);
                    cmd.Parameters.AddWithValue("@Status", order.Status);
                    cmd.Parameters.AddWithValue("@BotChatId", (object)order.BotChatId ?? DBNull.Value);

                    object newId = cmd.ExecuteScalar();
                    if (newId != null)
                    {
                        order.Id = Convert.ToInt32(newId);
                    }
                }
            }
        }

        public static void UpdateOrderStatus(int orderId, string newStatus)
        {
            Order existing = GetOrderById(orderId);

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = "UPDATE Orders SET Status = @Status WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@Id", orderId);
                    cmd.ExecuteNonQuery();
                }
            }

            if (existing != null && newStatus == "Booked" && existing.BotChatId.HasValue)
            {
                existing.Status = newStatus;
                _ = TelegramBotHandler.SendOrderConfirmedNotificationAsync(existing);
            }
        }

        public static void DeleteOrder(int orderId)
        {
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM Orders WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", orderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}