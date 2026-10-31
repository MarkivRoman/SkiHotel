using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SkiHotel.Models;

namespace SkiHotel.Services
{
    public static class ReviewService
    {
        private static readonly string DataFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        private static readonly string FilePath =
            Path.Combine(DataFolder, "reviews.txt");

        private const string Separator = "========================================";

        public static void SaveReview(Review review)
        {
            Directory.CreateDirectory(DataFolder);

            List<Review> reviews = GetAllReviews();

            review.Id = reviews.Count == 0 ? 1 : reviews.Max(x => x.Id) + 1;
            review.CreatedAt = DateTime.Now;

            using (StreamWriter writer = new StreamWriter(FilePath, true))
            {
                writer.WriteLine(Separator);
                writer.WriteLine("ВІДГУК");
                writer.WriteLine("ID: " + review.Id);
                writer.WriteLine("Дата: " + review.CreatedAt);
                writer.WriteLine("Клієнт: " + review.ClientName);
                writer.WriteLine("Оцінка: " + review.Rating + "/5");
                writer.WriteLine("Коментар: " + review.Comment);
                writer.WriteLine();
            }
        }

        public static List<Review> GetAllReviews()
        {
            List<Review> reviews = new List<Review>();

            if (!File.Exists(FilePath))
            {
                return reviews;
            }

            string content = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(content))
            {
                return reviews;
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

                Review review = new Review();
                review.Id = ReadInt(block, "ID:", 0);

                string dateText = ReadValue(block, "Дата:");
                if (DateTime.TryParse(dateText, out DateTime createdAt))
                {
                    review.CreatedAt = createdAt;
                }

                review.ClientName = ReadValue(block, "Клієнт:");
                review.Rating = ReadInt(block, "Оцінка:", 5);
                review.Comment = ReadValue(block, "Коментар:");

                reviews.Add(review);
            }

            return reviews.OrderBy(x => x.Id).ToList();
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
            if (value.Contains("/"))
            {
                value = value.Split('/')[0].Trim();
            }

            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}