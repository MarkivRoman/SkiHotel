using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SkiHotel.Models;

namespace SkiHotel.Services
{
    public static class ReviewService
    {
        public static void SaveReview(Review review)
        {
            review.CreatedAt = DateTime.Now;

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = @"
                    INSERT INTO Reviews (CreatedAt, ClientName, Rating, Comment) 
                    VALUES (@CreatedAt, @ClientName, @Rating, @Comment);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CreatedAt", review.CreatedAt);
                    cmd.Parameters.AddWithValue("@ClientName", review.ClientName ?? "");
                    cmd.Parameters.AddWithValue("@Rating", review.Rating);
                    cmd.Parameters.AddWithValue("@Comment", review.Comment ?? "");

                    object newId = cmd.ExecuteScalar();
                    if (newId != null)
                    {
                        review.Id = Convert.ToInt32(newId);
                    }
                }
            }
        }

        public static List<Review> GetAllReviews()
        {
            List<Review> reviews = new List<Review>();

            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Reviews ORDER BY Id ASC";
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Review review = new Review
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            ClientName = reader["ClientName"].ToString(),
                            Rating = Convert.ToInt32(reader["Rating"]),
                            Comment = reader["Comment"].ToString()
                        };
                        reviews.Add(review);
                    }
                }
            }

            return reviews;
        }
    }
}