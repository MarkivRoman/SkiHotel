using System;

namespace SkiHotel.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = "";
        public int Rating { get; set; } // від 0 до 5
        public string Comment { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}