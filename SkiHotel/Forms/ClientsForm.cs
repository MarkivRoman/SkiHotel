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

        private TabPage tabActive = new TabPage("Active Bookings");
        private TabPage tabIncoming = new TabPage("Incoming Requests (Waiting for Payment)");

        private FlowLayoutPanel panelActiveContainer = new FlowLayoutPanel();
        private FlowLayoutPanel panelIncomingContainer = new FlowLayoutPanel();

        private Label title = new Label();
        private Label lblCapacity = new Label();
        private const int MaxCapacity = 20;

        public ClientsForm()
        {
            Text = "Booking Management — SKI HOTEL";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1000, 650);
            BackColor = Color.FromArgb(240, 245, 250);
            Font = new Font("Segoe UI", 10);

            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.White,
                Padding = new Padding(25, 0, 25, 0)
            };

            title = new Label
            {
                Text = "Client and Booking Management Panel",
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

            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 11, FontStyle.Bold);

            SetupContainer(panelActiveContainer);
            SetupContainer(panelIncomingContainer);

            tabActive.Controls.Add(panelActiveContainer);
            tabIncoming.Controls.Add(panelIncomingContainer);

            tabControl.Controls.Add(tabActive);
            tabControl.Controls.Add(tabIncoming);

            Controls.Add(tabControl);
            Controls.Add(topPanel);

            LocalizationManager.LanguageChanged += ApplyLanguage;
            this.FormClosed += (s, e) => LocalizationManager.LanguageChanged -= ApplyLanguage;

            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            bool isUk = LocalizationManager.AppLanguage == "uk";

            Text = isUk ? "Управління бронюваннями — SKI HOTEL" : "Booking Management — SKI HOTEL";
            title.Text = isUk ? "Панель управління клієнтами та бронюваннями" : "Client and Booking Management Panel";
            tabActive.Text = isUk ? "Активні бронювання" : "Active Bookings";
            tabIncoming.Text = isUk ? "Вхідні заявки (Очікують оплати)" : "Incoming Requests (Waiting for Payment)";

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
            bool isUk = LocalizationManager.AppLanguage == "uk";
            var allOrders = OrderService.GetAllOrders();

            var activeOrders = allOrders.Where(o => o.Status == "Booked" || o.Status == "Заброньовано" || string.IsNullOrEmpty(o.Status)).ToList();
            var incomingOrders = allOrders.Where(o => o.Status == "Incoming" || o.Status == "Waiting for Payment" || o.Status == "Вхідні" || o.Status == "Очікує оплати").ToList();

            int activeCount = activeOrders.Count;
            int freeSpots = Math.Max(0, MaxCapacity - activeCount);
            lblCapacity.Text = isUk
                ? $"Вільних місць: {freeSpots} з {MaxCapacity} (Зайнято: {activeCount})"
                : $"Free spots: {freeSpots} of {MaxCapacity} (Occupied: {activeCount})";

            FillContainer(panelActiveContainer, activeOrders, true);
            FillContainer(panelIncomingContainer, incomingOrders, false);
        }

        private void FillContainer(FlowLayoutPanel panel, List<Order> orders, bool isActiveTab)
        {
            bool isUk = LocalizationManager.AppLanguage == "uk";
            panel.Controls.Clear();

            if (orders == null || orders.Count == 0)
            {
                Label lblEmpty = new Label
                {
                    Text = isUk ? "Список порожній." : "List is empty.",
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

        private string TranslateYesNo(string value, bool isUk)
        {
            if (!isUk) return value;
            if (value == "Yes") return "Так";
            if (value == "No") return "Ні";
            return value;
        }

        private Panel CreateOrderCard(Order order, bool isActiveTab)
        {
            bool isUk = LocalizationManager.AppLanguage == "uk";

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
                Text = isUk
                    ? $"Телефон: {order.Phone}  |  Email: {order.Email}"
                    : $"Phone: {order.Phone}  |  Email: {order.Email}",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 110, 120),
                Location = new Point(87, 45),
                AutoSize = true
            };

            string equipText = TranslateYesNo(order.HasEquipment, isUk);
            string hotelText = TranslateYesNo(order.HotelRoom, isUk);
            string details = isUk
                ? $"Скі-пас: {order.Days} дн. | Власне спорядження: {equipText} | Готель: {hotelText} | Вартість: {order.TotalPrice} грн"
                : $"Pass: {order.Days} days | Equipment: {order.HasEquipment} | Hotel: {order.HotelRoom} | Cost: {order.Cost}";

            Label lblDetails = new Label
            {
                Text = details,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(50, 60, 70),
                Location = new Point(87, 75),
                AutoSize = true
            };

            Label lblDate = new Label
            {
                Text = isUk
                    ? $"Створено: {order.CreatedAt:dd.MM.yyyy HH:mm}"
                    : $"Requested at: {order.CreatedAt:dd.MM.yyyy HH:mm}",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(650, 18),
                AutoSize = true
            };

            if (!isActiveTab)
            {
                Button btnApprove = new Button
                {
                    Text = isUk ? "Підтвердити" : "Approve",
                    Location = new Point(635, 105),
                    Size = new Size(130, 32),
                    BackColor = Color.FromArgb(40, 140, 60),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnApprove.FlatAppearance.BorderSize = 0;
                btnApprove.Click += (s, e) =>
                {
                    int currentActive = OrderService.GetAllOrders().Where(o => o.Status == "Booked" || o.Status == "Заброньовано" || string.IsNullOrEmpty(o.Status)).Count();
                    if (currentActive >= MaxCapacity)
                    {
                        MessageBox.Show(
                            isUk ? "Досягнуто ліміт у 20 вільних місць! Неможливо підтвердити нове бронювання." : "The limit of 20 free spots has been reached! Cannot approve new booking.",
                            isUk ? "Ліміт місць" : "Capacity Limit",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return;
                    }

                    OrderService.UpdateOrderStatus(order.Id, "Booked");
                    LoadData();
                };
                card.Controls.Add(btnApprove);
            }

            string deleteBtnText = isActiveTab
                ? (isUk ? "Видалити" : "Delete")
                : (isUk ? "Відхилити" : "Reject");

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
                string question = isUk
                    ? $"Ви впевнені, що хочете {(isActiveTab ? "видалити" : "відхилити")} замовлення клієнта {order.ClientName}?"
                    : $"Are you sure you want to {(isActiveTab ? "delete" : "reject")} the order for client {order.ClientName}?";

                var result = MessageBox.Show(
                    question,
                    isUk ? "Підтвердження" : "Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
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