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
                    while (reader.Read())
                    {
                        Order order = new Order
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            ClientName = reader["ClientName"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Email = reader["Email"].ToString(),
                            HasEquipment = reader["HasEquipment"].ToString(),
                            Weight = reader["Weight"].ToString(),
                            Height = reader["Height"].ToString(),
                            ShoeSize = reader["ShoeSize"].ToString(),
                            SkillLevel = reader["SkillLevel"].ToString(),
                            Skis = reader["Skis"].ToString(),
                            Boots = reader["Boots"].ToString(),
                            Poles = reader["Poles"].ToString(),
                            SkiPassDays = Convert.ToInt32(reader["SkiPassDays"]),
                            HotelRoom = reader["HotelRoom"].ToString(),
                            Breakfast = Convert.ToBoolean(reader["Breakfast"]),
                            Sauna = Convert.ToBoolean(reader["Sauna"]),
                            TotalPrice = Convert.ToDecimal(reader["TotalPrice"]),
                            Status = reader["Status"].ToString()
                        };
                        orders.Add(order);
                    }
                }
            }

            return orders;
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
                    (CreatedAt, ClientName, Phone, Email, HasEquipment, Weight, Height, ShoeSize, SkillLevel, Skis, Boots, Poles, SkiPassDays, HotelRoom, Breakfast, Sauna, TotalPrice, Status) 
                    VALUES 
                    (@CreatedAt, @ClientName, @Phone, @Email, @HasEquipment, @Weight, @Height, @ShoeSize, @SkillLevel, @Skis, @Boots, @Poles, @SkiPassDays, @HotelRoom, @Breakfast, @Sauna, @TotalPrice, @Status);
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