using System;
using System.Data.SqlClient;
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

        private Label lblAvatar;
        private Label lblUserStatus;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblSecurityFooter;

        private string currentUsername = "";
        private string currentRole = "";

        public MainForm()
        {
            InitializeComponent();
            Database.InitializeDatabase();
            CreateMainMenu();
        }

        private void CreateMainMenu()
        {
            this.Text = "SKI HOTEL — Terminal Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(880, 620);
            this.MinimumSize = new Size(800, 550);
            this.BackColor = Color.FromArgb(242, 246, 250);
            this.MaximizeBox = false;

            // ==========================================
            // ЛІВА ПАНЕЛЬ (Повністю темно-синя вертикальна смуга)
            // ==========================================
            Panel leftPanel = new Panel()
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(20, 45, 75)
            };
            this.Controls.Add(leftPanel);

            // ==========================================
            // ПРАВА ПАНЕЛЬ (ОСНОВНИЙ КОНТЕНТ)
            // ==========================================

            // Кружечок з першою літерою (Аватарка у верхньому правому/центральному блоці)
            lblAvatar = new Label()
            {
                Size = new Size(50, 50),
                Location = new Point(230, 25),
                BackColor = Color.FromArgb(30, 135, 225),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "M"
            };
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, lblAvatar.Width, lblAvatar.Height);
            lblAvatar.Region = new Region(path);
            this.Controls.Add(lblAvatar);

            // Інформація про користувача зверху
            lblUserStatus = new Label()
            {
                Location = new Point(290, 32),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Text = "🔓 Logged in as: Loading..."
            };
            this.Controls.Add(lblUserStatus);

            // Заголовок системи
            lblTitle = new Label()
            {
                Text = "SKI HOTEL",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 45, 75),
                AutoSize = true,
                Location = new Point(228, 85)
            };
            this.Controls.Add(lblTitle);

            // Підзаголовок
            lblSubtitle = new Label()
            {
                Text = "Ski resort management terminal system",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 115, 130),
                AutoSize = true,
                Location = new Point(232, 125)
            };
            this.Controls.Add(lblSubtitle);

            // ==========================================
            // КНОПКИ В СТОВПЧИК (СПРАВА)
            // ==========================================
            int startX = 230;
            int startY = 175;
            int btnWidth = 580;
            int btnHeight = 55;
            int spacing = 15;

            btnNewOrder = CreateMenuButton("NEW ORDER", new Point(startX, startY));
            btnClients = CreateMenuButton("CLIENTS", new Point(startX, startY + (btnHeight + spacing) * 1));
            btnEquipment = CreateMenuButton("EQUIPMENT", new Point(startX, startY + (btnHeight + spacing) * 2));
            btnStatistics = CreateMenuButton("STATISTICS", new Point(startX, startY + (btnHeight + spacing) * 3));
            btnExit = CreateMenuButton("EXIT SYSTEM", new Point(startX, startY + (btnHeight + spacing) * 4));

            this.Controls.Add(btnNewOrder);
            this.Controls.Add(btnClients);
            this.Controls.Add(btnEquipment);
            this.Controls.Add(btnStatistics);
            this.Controls.Add(btnExit);

            btnNewOrder.Click += BtnNewOrder_Click;
            btnClients.Click += BtnClients_Click;
            btnEquipment.Click += BtnEquipment_Click;
            btnStatistics.Click += BtnStatistics_Click;
            btnExit.Click += BtnExit_Click;

            // ==========================================
            // ДРІБНИЙ ТЕКСТ В НИЗУ (БЕЗПЕКА ТА ШИФРУВАННЯ)
            // ==========================================
            lblSecurityFooter = new Label()
            {
                Text = "Secured with SHA-256 database encryption & LocalDB protocol — Terminal v2.4",
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = Color.FromArgb(140, 155, 170),
                AutoSize = true,
                Location = new Point(230, 545)
            };
            this.Controls.Add(lblSecurityFooter);

            SetButtonsAccess(false);
        }

        private Button CreateMenuButton(string text, Point location)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(580, 55);
            button.Location = location;
            button.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            button.BackColor = Color.White;
            button.ForeColor = Color.FromArgb(20, 45, 75);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(205, 215, 225);
            button.Cursor = Cursors.Hand;
            return button;
        }

        private void SetButtonsAccess(bool access)
        {
            btnNewOrder.Enabled = access;
            btnClients.Enabled = access;
            btnEquipment.Enabled = access;
            btnStatistics.Enabled = access;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ShowLoginDialog();
        }

        // Вікно авторизації, яке ми робили раніше
        private void ShowLoginDialog()
        {
            Form loginForm = new Form()
            {
                Width = 520,
                Height = 320,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Ski Hotel — Authorization",
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(245, 248, 252)
            };

            Panel leftPanel = new Panel()
            {
                Dock = DockStyle.Left,
                Width = 150,
                BackColor = Color.FromArgb(20, 45, 75)
            };

            Label lblDialogAvatar = new Label()
            {
                Size = new Size(70, 70),
                Location = new Point(40, 95),
                BackColor = Color.FromArgb(40, 80, 125),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "M"
            };
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, lblDialogAvatar.Width, lblDialogAvatar.Height);
            lblDialogAvatar.Region = new Region(path);

            leftPanel.Controls.Add(lblDialogAvatar);
            loginForm.Controls.Add(leftPanel);

            Label lblHeader = new Label()
            {
                Left = 175,
                Top = 20,
                Text = "Ski Hotel Terminal Authorization",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 45, 75),
                AutoSize = true
            };

            Label lblSubHeader = new Label()
            {
                Left = 175,
                Top = 45,
                Text = "Please select account and enter security key",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(110, 125, 140),
                AutoSize = true
            };

            Label lblUser = new Label()
            {
                Left = 175,
                Top = 85,
                Text = "Select User Profile:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 70, 90),
                AutoSize = true
            };

            ComboBox cmbUser = new ComboBox()
            {
                Left = 175,
                Top = 110,
                Width = 290,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5f)
            };
            cmbUser.Items.AddRange(new object[] { "Markiv", "Muhalchuk", "Arendach" });
            cmbUser.SelectedIndex = 0;

            cmbUser.SelectedIndexChanged += (s, e) =>
            {
                if (cmbUser.SelectedItem != null)
                {
                    lblDialogAvatar.Text = cmbUser.SelectedItem.ToString().Substring(0, 1).ToUpper();
                }
            };

            Label lblPass = new Label()
            {
                Left = 175,
                Top = 150,
                Text = "Password:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 70, 90),
                AutoSize = true
            };

            TextBox txtPass = new TextBox()
            {
                Left = 175,
                Top = 175,
                Width = 290,
                PasswordChar = '*',
                Font = new Font("Segoe UI", 10.5f)
            };

            Button btnLogin = new Button()
            {
                Text = "AUTHORIZE",
                Left = 175,
                Top = 220,
                Width = 290,
                Height = 40,
                BackColor = Color.FromArgb(30, 135, 225),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            };
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += (sender, args) => { loginForm.Close(); };

            loginForm.Controls.Add(lblHeader);
            loginForm.Controls.Add(lblSubHeader);
            loginForm.Controls.Add(lblUser);
            loginForm.Controls.Add(cmbUser);
            loginForm.Controls.Add(lblPass);
            loginForm.Controls.Add(txtPass);
            loginForm.Controls.Add(btnLogin);
            loginForm.AcceptButton = btnLogin;

            loginForm.ShowDialog(this);

            string inputUser = cmbUser.SelectedItem != null ? cmbUser.SelectedItem.ToString() : "";
            string inputPassHash = SecurityHelper.HashPassword(txtPass.Text);

            bool success = false;
            using (var conn = Database.GetConnection())
            {
                conn.Open();
                string query = "SELECT Role FROM Users WHERE Username = @u AND PasswordHash = @p";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@u", inputUser);
                    cmd.Parameters.AddWithValue("@p", inputPassHash);

                    var roleResult = cmd.ExecuteScalar();
                    if (roleResult != null)
                    {
                        currentUsername = inputUser;
                        currentRole = roleResult.ToString();
                        success = true;
                    }
                }
            }

            if (success)
            {
                SetButtonsAccess(true);
                // Оновлюємо кружечок та статус у головному меню під обраного користувача
                lblAvatar.Text = currentUsername.Substring(0, 1).ToUpper();
                lblUserStatus.Text = $"Logged in as: {currentUsername} ({currentRole})";
            }
            else
            {
                MessageBox.Show("Incorrect password! Access denied.", "Authorization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void BtnNewOrder_Click(object sender, EventArgs e)
        {
            using (OrderForm orderForm = new OrderForm())
            {
                orderForm.ShowDialog();
            }
        }

        private void BtnClients_Click(object sender, EventArgs e)
        {
            using (ClientsForm form = new ClientsForm())
            {
                form.ShowDialog(this);
            }
        }

        private void BtnEquipment_Click(object sender, EventArgs e)
        {
            using (EquipmentForm equipmentForm = new EquipmentForm())
            {
                equipmentForm.ShowDialog(this);
            }
        }

        private void BtnStatistics_Click(object sender, EventArgs e)
        {
            using (ReviewsForm reviewsForm = new ReviewsForm())
            {
                reviewsForm.ShowDialog(this);
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}