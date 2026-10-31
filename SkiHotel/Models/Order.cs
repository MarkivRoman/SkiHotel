using System;

namespace SkiHotel.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClientName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";

        // Поля у вигляді рядків для файлу та нової форми
        public string HasEquipment { get; set; } = "Ні";
        public string Weight { get; set; } = "";
        public string Height { get; set; } = "";
        public string ShoeSize { get; set; } = "";

        public string SkillLevel { get; set; } = "";
        public string Level { get => SkillLevel; set => SkillLevel = value; }

        public int SkiPassDays { get; set; }
        public int Days { get => SkiPassDays; set => SkiPassDays = value; }

        public string HotelRoom { get; set; } = "Ні";
        public bool Breakfast { get; set; }
        public bool Sauna { get; set; }

        public decimal TotalPrice { get; set; }
        public string Cost
        {
            get => TotalPrice > 0 ? TotalPrice.ToString("0") + " грн" : "";
            set
            {
                if (!string.IsNullOrEmpty(value) && decimal.TryParse(value.Replace(" грн", "").Trim(), out decimal val))
                    TotalPrice = val;
            }
        }

        public string Skis { get; set; } = "";
        public string Boots { get; set; } = "";
        public string Poles { get; set; } = "";

        public string Status { get; set; } = "Заброньовано";
    }
}