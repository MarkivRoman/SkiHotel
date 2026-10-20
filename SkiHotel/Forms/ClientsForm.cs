
using System;
using System.Drawing;
using System.Windows.Forms;
using SkiHotel.Services;

namespace SkiHotel.Forms
{
    public partial class ClientsForm : Form
    {
        private readonly DataGridView grid = new DataGridView();

        public ClientsForm()
        {
            // Налаштування вікна
            Text = "Клієнти — SKI HOTEL";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 500);
            BackColor = Color.FromArgb(235, 242, 248);
            Font = new Font("Segoe UI", 10);

            // Заголовок
            Label title = new Label
            {
                Text = "Клієнти та замовлення",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 90),
                Dock = DockStyle.Top,
                Height = 65,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };

            // Таблиця
            grid.Dock = DockStyle.Fill;
            grid.Visible = true;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoGenerateColumns = true;
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.DisplayedCells;
            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.ColumnHeadersHeight = 40;
            grid.EnableHeadersVisualStyles = false;

            grid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(30, 60, 90);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(240, 246, 250);

            // Створюємо колонки, щоб таблиця була видима
            // навіть за відсутності замовлень
            grid.Columns.Add("Id", "ID");
            grid.Columns.Add("CreatedAt", "Дата замовлення");
            grid.Columns.Add("ClientName", "Ім'я та прізвище");
            grid.Columns.Add("Phone", "Телефон");
            grid.Columns.Add("Email", "Email");
            grid.Columns.Add("HasOwnEquipment", "Власне спорядження");
            grid.Columns.Add("Weight", "Вага, кг");
            grid.Columns.Add("Height", "Зріст, см");
            grid.Columns.Add("ShoeSize", "Розмір взуття");
            grid.Columns.Add("SkillLevel", "Рівень катання");
            grid.Columns.Add("Skis", "Лижі");
            grid.Columns.Add("Boots", "Черевики");
            grid.Columns.Add("Poles", "Палиці");
            grid.Columns.Add("SkiPassDays", "Днів абонемента");
            grid.Columns.Add("HotelRoom", "Номер у готелі");
            grid.Columns.Add("Breakfast", "Сніданок");
            grid.Columns.Add("Sauna", "Сауна");
            grid.Columns.Add("TotalPrice", "Вартість, грн");
            grid.Columns.Add("Status", "Статус");

            // Додаємо елементи у правильному порядку
            Controls.Add(grid);
            Controls.Add(title);

            // Завантажуємо дані
            LoadClients();
        }

        private void LoadClients()
        {
            try
            {
                var orders = OrderService.GetAllOrders();

                // Якщо замовлень немає, залишаємо порожню таблицю
                if (orders == null)
                    return;

                grid.Rows.Clear();

                foreach (var order in orders)
                {
                    grid.Rows.Add(
                        order.Id,
                        order.CreatedAt,
                        order.ClientName,
                        order.Phone,
                        order.Email,
                        order.HasOwnEquipment,
                        order.Weight,
                        order.Height,
                        order.ShoeSize,
                        order.SkillLevel,
                        order.Skis,
                        order.Boots,
                        order.Poles,
                        order.SkiPassDays,
                        order.HotelRoom,
                        order.Breakfast,
                        order.Sauna,
                        order.TotalPrice,
                        order.Status
                    );
                }

                if (grid.Columns["CreatedAt"] != null)
                {
                    grid.Columns["CreatedAt"].DefaultCellStyle.Format =
                        "dd.MM.yyyy HH:mm";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не вдалося завантажити замовлення.\n\n" + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ClientsForm_Load(object sender, EventArgs e)
        {
        }
    }
}
