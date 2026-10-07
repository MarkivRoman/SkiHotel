using System;

namespace SkiHotel.Models
{
    public class Order
    {
        // Ідентифікатор замовлення
        public int Id { get; set; }

        // Дані клієнта
        public string ClientName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";

        // Дані лижника
        public bool HasOwnEquipment { get; set; }

        public double Weight { get; set; }
        public double Height { get; set; }
        public int ShoeSize { get; set; }

        public string SkillLevel { get; set; } = "";

        // Підібране спорядження
        public string Skis { get; set; } = "";
        public string Boots { get; set; } = "";
        public string Poles { get; set; } = "";

        // Абонемент
        public int SkiPassDays { get; set; }

        // Готель
        public bool HotelRoom { get; set; }

        // Додаткові послуги
        public bool Breakfast { get; set; }
        public bool Sauna { get; set; }

        // Вартість
        public decimal TotalPrice { get; set; }

        // Статус
        public string Status { get; set; } = "Заброньовано";

        // Дата створення
        public DateTime CreatedAt { get; set; }
    }
}