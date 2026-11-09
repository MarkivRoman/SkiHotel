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
            // Window setup
            this.Text = "SKI HOTEL";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(800, 500);
            this.BackColor = Color.FromArgb(235, 242, 248);

            // =========================
            // TITLE
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
            // SUBTITLE
            // =========================

            lblSubtitle = new Label();

            lblSubtitle.Text =
                "Ski resort vacation management system";

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
            // BUTTONS
            // =========================

            btnNewOrder = CreateMenuButton(
                "NEW ORDER",
                new Point(60, 160)
            );

            btnClients = CreateMenuButton(
                "CLIENTS",
                new Point(460, 160)
            );

            btnEquipment = CreateMenuButton(
                "EQUIPMENT",
                new Point(60, 280)
            );

            btnStatistics = CreateMenuButton(
                "STATISTICS",
                new Point(460, 280)
            );

            btnExit = CreateMenuButton(
                "EXIT",
                new Point(260, 410)
            );

            // Add buttons to the form

            this.Controls.Add(btnNewOrder);
            this.Controls.Add(btnClients);
            this.Controls.Add(btnEquipment);
            this.Controls.Add(btnStatistics);
            this.Controls.Add(btnExit);

            // =========================
            // BUTTON EVENTS
            // =========================

            btnNewOrder.Click += BtnNewOrder_Click;
            btnClients.Click += BtnClients_Click;
            btnEquipment.Click += BtnEquipment_Click;
            btnStatistics.Click += BtnStatistics_Click;
            btnExit.Click += BtnExit_Click;
        }

        // =========================
        // CREATE BUTTON
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
        // NEW ORDER
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
        // CLIENTS
        // =========================

        private void BtnClients_Click(object sender, EventArgs e)
        {
            using (ClientsForm form = new ClientsForm())
            {
                form.ShowDialog(this);
            }
        }


        // =========================
        // EQUIPMENT
        // =========================

        // =========================
        // EQUIPMENT
        // =========================

        private void BtnEquipment_Click(
            object sender,
            EventArgs e)
        {
            using (EquipmentForm equipmentForm = new EquipmentForm())
            {
                equipmentForm.ShowDialog(this);
            }
        }

        // =========================
        // STATISTICS (REVIEWS)
        // =========================

        private void BtnStatistics_Click(
            object sender,
            EventArgs e)
        {
            // Open our client reviews and statistics form from file
            using (ReviewsForm reviewsForm = new ReviewsForm())
            {
                reviewsForm.ShowDialog(this);
            }
        }

        // =========================
        // EXIT
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