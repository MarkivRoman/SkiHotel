using System;
using System.Windows.Forms;

namespace SkiHotel
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Гарантовано створюємо БД та всі таблиці до запуску бота і форм
            Database.InitializeDatabase();

            TelegramBotHandler.StartBot();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}