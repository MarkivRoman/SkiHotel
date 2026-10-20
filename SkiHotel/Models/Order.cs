
using System;
namespace SkiHotel.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public bool HasOwnEquipment { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public int ShoeSize { get; set; }
        public string SkillLevel { get; set; } = "";
        public string Skis { get; set; } = "";
        public string Boots { get; set; } = "";
        public string Poles { get; set; } = "";
        public int SkiPassDays { get; set; }
        public bool HotelRoom { get; set; }
        public bool Breakfast { get; set; }
        public bool Sauna { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Заброньовано";
        public DateTime CreatedAt { get; set; }
    }
}
