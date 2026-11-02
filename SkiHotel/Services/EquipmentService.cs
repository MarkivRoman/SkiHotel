using System;
namespace SkiHotel.Services
{
    public static class EquipmentService
    {
        public static string GetSkiLength(double height, string skillLevel)
        {
            double length;
            switch (skillLevel)
            {
                case "Beginner":
                case "Початківець":
                    length = height - 15;
                    break;
                case "Intermediate":
                case "Середній":
                    length = height - 10;
                    break;
                case "Professional":
                case "Професіонал":
                    length = height;
                    break;
                default:
                    length = height - 10;
                    break;
            }
            return $"{length:0} cm";
        }
        public static string GetPoleLength(double height)
        {
            return $"{height * 0.68:0} cm";
        }
        public static string GetBootSize(int shoeSize)
        {
            return $"size {shoeSize}";
        }
    }
}