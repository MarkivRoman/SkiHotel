using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkiHotel
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Запускаємо Telegram-бот у фоновому потоці
            Task.Run(() => TelegramBotHandler.StartBot());

            // Запускаємо головне вікно Windows Forms
            Application.Run(new MainForm());
        }
    }
}