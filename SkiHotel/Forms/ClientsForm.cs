using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using SkiHotel.Models;
using SkiHotel.Services;

namespace SkiHotel.Forms
{
    public partial class ClientsForm : Form
    {
        private TabControl tabControl = new TabControl();

        // Вкладки
        private TabPage tabActive = new TabPage("Активні бронювання");
        private TabPage tabIncoming = new TabPage("Вхідні заявки (Очікують оплати)");

        private FlowLayoutPanel panelActiveContainer = new FlowLayoutPanel();
        private FlowLayoutPanel panelIncomingContainer = new FlowLayoutPanel();

        private Label lblCapacity = new Label();
        private const int MaxCapacity = 20;

        public ClientsForm()
        {
            Text = "Керування бронюваннями — SKI HOTEL";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);
            BackColor = Color.FromArgb(240, 245, 250);
            Font = new Font("Segoe UI", 10);

            // Верхня панель зі заголовком та лічильником місць
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.White,
                Padding = new Padding(25, 0, 25, 0)
            };

            Label title = new Label
            {
                Text = "Панель керування клієнтами та бронями",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 90),
                Location = new Point(25, 20),
                AutoSize = true
            };

            lblCapacity.Location = new Point(650, 24);
            lblCapacity.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblCapacity.ForeColor = Color.FromArgb(30, 100, 70);
            lblCapacity.AutoSize = true;

            topPanel.Controls.Add(title);
            topPanel.Controls.Add(lblCapacity);

            // Налаштування TabControl (вкладок)
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            // Налаштування контейнерів всередині вкладок
            SetupContainer(panelActiveContainer);
            SetupContainer(panelIncomingContainer);

            tabActive.Controls.Add(panelActiveContainer);
            tabIncoming.Controls.Add(panelIncomingContainer);

            tabControl.Controls.Add(tabActive);
            tabControl.Controls.Add(tabIncoming);

            Controls.Add(tabControl);
            Controls.Add(topPanel);

            LoadData();
        }

        private void SetupContainer(FlowLayoutPanel panel)
        {
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.BackColor = Color.FromArgb(240, 245, 250);
            panel.Padding = new Padding(20);
        }

        private void LoadData()
        {
            var allOrders = OrderService.GetAllOrders();

            // Фільтруємо за статусами
            var activeOrders = allOrders.Where(o => o.Status == "Заброньовано" || string.IsNullOrEmpty(o.Status)).ToList();
            var incomingOrders = allOrders.Where(o => o.Status == "Вхідні" || o.Status == "Очікує оплати").ToList();

            // Оновлюємо індикатор вільних місць (максимум 20)
            int activeCount = activeOrders.Count;
            int freeSpots = Math.Max(0, MaxCapacity - activeCount);
            lblCapacity.Text = $"Вільних місць: {freeSpots} з {MaxCapacity} (Зайнято: {activeCount})";

            // Заповнюємо активні
            FillContainer(panelActiveContainer, activeOrders, true);
            // Заповнюємо вхідні
            FillContainer(panelIncomingContainer, incomingOrders, false);
        }

        private void FillContainer(FlowLayoutPanel panel, List<Order> orders, bool isActiveTab)
        {
            panel.Controls.Clear();

            if (orders == null || orders.Count == 0)
            {
                Label lblEmpty = new Label
                {
                    Text = "Список порожній.",
                    Font = new Font("Segoe UI", 13, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(30, 30)
                };
                panel.Controls.Add(lblEmpty);
                return;
            }

            foreach (var order in orders)
            {
                panel.Controls.Add(CreateOrderCard(order, isActiveTab));
            }
        }

        private Panel CreateOrderCard(Order order, bool isActiveTab)
        {
            Panel card = new Panel
            {
                Width = 900,
                Height = 150,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15)
            };

            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.FromArgb(210, 220, 230), 1.5f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            // Кругла аватарка з ініціалом
            PictureBox avatarBox = new PictureBox
            {
                Size = new Size(50, 50),
                Location = new Point(20, 20),
                BackColor = Color.Transparent
            };

            avatarBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 60, 90)))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, 49, 49);
                }

                string initial = string.IsNullOrEmpty(order.ClientName) ? "?" : order.ClientName.Substring(0, 1).ToUpper();
                using (Font font = new Font("Segoe UI", 18, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, initial, font, new Rectangle(0, 0, 50, 50), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            // ПІБ клієнта та контакти
            Label lblName = new Label
            {
                Text = order.ClientName,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 70),
                Location = new Point(85, 15),
                AutoSize = true
            };

            Label lblContacts = new Label
            {
                Text = $"Телефон: {order.Phone}  |  Email: {order.Email}",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 110, 120),
                Location = new Point(87, 45),
                AutoSize = true
            };

            // Деталі бронювання
            string details = $"Абонемент: {order.Days} дн. | Спорядження: {order.HasEquipment} | Готель: {order.HotelRoom} | Вартість: {order.Cost}";
            Label lblDetails = new Label
            {
                Text = details,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(50, 60, 70),
                Location = new Point(87, 75),
                AutoSize = true
            };

            // Дата заявки
            Label lblDate = new Label
            {
                Text = $"Заявка від: {order.CreatedAt:dd.MM.yyyy HH:mm}",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(650, 18),
                AutoSize = true
            };

            // Кнопка Ухвалити (тільки для вхідних)
            if (!isActiveTab)
            {
                Button btnApprove = new Button
                {
                    Text = "Ухвалити",
                    Location = new Point(650, 105),
                    Size = new Size(115, 32),
                    BackColor = Color.FromArgb(40, 140, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnApprove.FlatAppearance.BorderSize = 0;
                btnApprove.Click += (s, e) =>
                {
                    int currentActive = OrderService.GetAllOrders().Where(o => o.Status == "Заброньовано" || string.IsNullOrEmpty(o.Status)).Count();
                    if (currentActive >= MaxCapacity)
                    {
                        MessageBox.Show("Досягнуто ліміт у 20 вільних місць! Неможливо ухвалити нове бронювання.", "Ліміт місць", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    OrderService.UpdateOrderStatus(order.Id, "Заброньовано");
                    LoadData();
                };
                card.Controls.Add(btnApprove);
            }

            // Кнопка Видалити / Відхилити
            string deleteBtnText = isActiveTab ? "Видалити" : "Відхилити";
            Button btnDelete = new Button
            {
                Text = deleteBtnText,
                Location = new Point(775, 105),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += (s, e) =>
            {
                var result = MessageBox.Show($"Ви дійсно хочете {(isActiveTab ? "видалити" : "відхилити")} замовлення клієнта {order.ClientName}?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    OrderService.DeleteOrder(order.Id);
                    LoadData();
                }
            };

            card.Controls.Add(avatarBox);
            card.Controls.Add(lblName);
            card.Controls.Add(lblContacts);
            card.Controls.Add(lblDetails);
            card.Controls.Add(lblDate);
            card.Controls.Add(btnDelete);

            return card;
        }

        private void ClientsForm_Load(object sender, EventArgs e)
        {

        }
    }

}