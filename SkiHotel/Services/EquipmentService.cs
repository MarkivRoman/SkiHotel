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
                case "Початківець":
                    length = height - 15;
                    break;

                case "Середній":
                    length = height - 10;
                    break;

                case "Професіонал":
                    length = height;
                    break;

                default:
                    length = height - 10;
                    break;
            }

            return $"{length:0} см";
        }

        public static string GetPoleLength(double height)
        {
            double length = height * 0.68;

            return $"{length:0} см";
        }

        public static string GetBootSize(int shoeSize)
        {
            return $"розмір {shoeSize}";
        }
    }
}