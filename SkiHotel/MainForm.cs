using System;
using System.Drawing;
using System.Windows.Forms;
using SkiHotel.Forms;

namespace SkiHotel
{
    public partial class MainForm : Form
    {
        private Button btnNewOrder;
        private Button btnClients;
        private Button btnEquipment;
        private Button btnStatistics;
        private Button btnExit;

        private Label lblTitle;
        private Label lblSubtitle;

        public MainForm()
        {
            InitializeComponent();

            CreateMainMenu();
        }

        private void CreateMainMenu()
        {
            // Налаштування головного вікна
            this.Text = "SKI HOTEL";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(800, 500);
            this.BackColor = Color.FromArgb(235, 242, 248);

            // =========================
            // ЗАГОЛОВОК
            // =========================

            lblTitle = new Label();

            lblTitle.Text = "SKI HOTEL";
            lblTitle.Font = new Font(
                "Segoe UI",
                28,
                FontStyle.Bold
            );

            lblTitle.ForeColor = Color.FromArgb(30, 60, 90);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(40, 35);

            this.Controls.Add(lblTitle);

            // =========================
            // ПІДЗАГОЛОВОК
            // =========================

            lblSubtitle = new Label();

            lblSubtitle.Text =
                "Система керування відпочинком на гірськолижному курорті";

            lblSubtitle.Font = new Font(
                "Segoe UI",
                11,
                FontStyle.Regular
            );

            lblSubtitle.ForeColor =
                Color.FromArgb(80, 100, 115);

            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(45, 85);

            this.Controls.Add(lblSubtitle);

            // =========================
            // КНОПКИ
            // =========================

            btnNewOrder = CreateMenuButton(
                "НОВЕ ЗАМОВЛЕННЯ",
                new Point(60, 160)
            );

            btnClients = CreateMenuButton(
                "КЛІЄНТИ",
                new Point(460, 160)
            );

            btnEquipment = CreateMenuButton(
                "СПОРЯДЖЕННЯ",
                new Point(60, 280)
            );

            btnStatistics = CreateMenuButton(
                "СТАТИСТИКА",
                new Point(460, 280)
            );

            btnExit = CreateMenuButton(
                "ВИХІД",
                new Point(260, 410)
            );

            // Додаємо кнопки на форму

            this.Controls.Add(btnNewOrder);
            this.Controls.Add(btnClients);
            this.Controls.Add(btnEquipment);
            this.Controls.Add(btnStatistics);
            this.Controls.Add(btnExit);

            // =========================
            // ПОДІЇ КНОПОК
            // =========================

            btnNewOrder.Click += BtnNewOrder_Click;
            btnClients.Click += BtnClients_Click;
            btnEquipment.Click += BtnEquipment_Click;
            btnStatistics.Click += BtnStatistics_Click;
            btnExit.Click += BtnExit_Click;
        }

        // =========================
        // СТВОРЕННЯ КНОПКИ
        // =========================

        private Button CreateMenuButton(
            string text,
            Point location)
        {
            Button button = new Button();

            button.Text = text;

            button.Size = new Size(320, 85);
            button.Location = location;

            button.Font = new Font(
                "Segoe UI",
                12,
                FontStyle.Bold
            );

            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(30, 60, 90);

            button.FlatStyle = FlatStyle.Flat;

            button.FlatAppearance.BorderSize = 1;

            button.FlatAppearance.BorderColor =
                Color.FromArgb(190, 205, 215);

            button.Cursor = Cursors.Hand;

            return button;
        }

        // =========================
        // НОВЕ ЗАМОВЛЕННЯ
        // =========================

        private void BtnNewOrder_Click(
     object sender,
     EventArgs e)
        {
            using (OrderForm orderForm = new OrderForm())
            {
                orderForm.ShowDialog();
            }
        }

        // =========================
        // КЛІЄНТИ
        // =========================

        private void BtnClients_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "Тут буде список клієнтів та замовлень.",
                "Клієнти",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // =========================
        // СПОРЯДЖЕННЯ
        // =========================

        private void BtnEquipment_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "Тут буде інформація про лижі, черевики та палки.",
                "Спорядження",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // =========================
        // СТАТИСТИКА
        // =========================

        private void BtnStatistics_Click(
            object sender,
            EventArgs e)
        {
            MessageBox.Show(
                "Тут буде статистика готелю.",
                "Статистика",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // =========================
        // ВИХІД
        // =========================

        private void BtnExit_Click(
            object sender,
            EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}