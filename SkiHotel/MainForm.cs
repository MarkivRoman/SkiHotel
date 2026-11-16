using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SkiHotel.Forms;

namespace SkiHotel
{
    public partial class MainForm : Form
    {
        // Верхня панель навігації (завжди зверху для всіх форм)
        private Panel topNavBar;
        private Button btnBackToMenu;
        private Button btnLanguage;
        private ContextMenuStrip langMenu;
        private Button btnRefresh;
        private Button btnSwitchUser;
        private Button btnTopExit;

        // Робоча область під верхньою панеллю
        private Panel workspacePanel;

        // Панель самого Головного Меню
        private Panel mainMenuContainer;
        private Form activeChildForm = null;

        // Елементи Головного Меню
        private Button btnNewOrder;
        private Button btnClients;
        private Button btnEquipment;
        private Button btnStatistics;

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
            SetupShellAndMenu();
        }

        private void SetupShellAndMenu()
        {
            // Налаштування головного вікна: по центру, на весь екран
            this.Text = "SKI HOTEL — Enterprise Terminal";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1024, 700);
            this.BackColor = Color.FromArgb(242, 246, 250);

            // ========================================================
            // 1. ВЕРХНЯ ПАНЕЛЬ НАВІГАЦІЇ (Завжди зверху для всіх форм)
            // ========================================================
            topNavBar = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.FromArgb(12, 28, 48) // Глибокий темний фон панелі
            };
            this.Controls.Add(topNavBar);

            // Кнопка "Повернутись на головне меню" (зліва)
            btnBackToMenu = new Button()
            {
                Text = "←  Back to Main Menu",
                Location = new Point(20, 11),
                Size = new Size(215, 36),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(25, 110, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false // На головному меню прихована, з'являється у дочірніх формах
            };
            btnBackToMenu.FlatAppearance.BorderSize = 0;
            btnBackToMenu.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 130, 210);
            btnBackToMenu.Click += (s, e) => ReturnToMainMenu();
            topNavBar.Controls.Add(btnBackToMenu);

            // Контейнер для правих кнопок із фіксованою шириною та чіткими відступами
            Panel rightButtonsPanel = new Panel()
            {
                Size = new Size(560, 58),
                Location = new Point(topNavBar.Width - 560, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.Transparent
            };
            topNavBar.Controls.Add(rightButtonsPanel);

            // 1. Кнопка зміни мови (з випадаючим меню-заглушкою)
            langMenu = new ContextMenuStrip();
            langMenu.Items.Add("🇺🇦 Українська мова");
            langMenu.Items.Add("🇬🇧 English language");

            btnLanguage = CreateNavButton("🌐 Language ▾", Color.FromArgb(20, 42, 68), new Point(10, 11), 135);
            btnLanguage.Click += (s, e) =>
            {
                langMenu.Show(btnLanguage, new Point(0, btnLanguage.Height));
            };
            rightButtonsPanel.Controls.Add(btnLanguage);

            // 2. Кнопка Оновити (візуальне оновлення)
            btnRefresh = CreateNavButton("🔄 Refresh", Color.FromArgb(20, 42, 68), new Point(157, 11), 115);
            btnRefresh.Click += (s, e) =>
            {
                this.Refresh();
                if (activeChildForm != null) activeChildForm.Refresh();

                string oldText = btnRefresh.Text;
                btnRefresh.Text = "✓ Updated";
                Timer t = new Timer() { Interval = 800 };
                t.Tick += (ts, te) => { btnRefresh.Text = oldText; t.Stop(); t.Dispose(); };
                t.Start();
            };
            rightButtonsPanel.Controls.Add(btnRefresh);

            // 3. Кнопка Змінити користувача (з фігуркою людини)
            btnSwitchUser = CreateNavButton("👤 Switch User", Color.FromArgb(20, 42, 68), new Point(284, 11), 150);
            btnSwitchUser.Click += (s, e) =>
            {
                ReturnToMainMenu();
                SetButtonsAccess(false);
                ShowLoginDialog();
            };
            rightButtonsPanel.Controls.Add(btnSwitchUser);

            // 4. Червона кнопка Виходу (темно-бордова, підсвічується червоним)
            btnTopExit = CreateNavButton("✕ Exit", Color.FromArgb(165, 32, 32), new Point(446, 11), 95);
            btnTopExit.FlatAppearance.BorderColor = Color.FromArgb(200, 50, 50);
            btnTopExit.FlatAppearance.MouseOverBackColor = Color.FromArgb(205, 40, 40);
            btnTopExit.Click += (s, e) => Application.Exit();
            rightButtonsPanel.Controls.Add(btnTopExit);

            // ========================================================
            // 2. РОБОЧА ОБЛАСТЬ ПІД ВЕРХНЬОЮ ПАНЕЛЛЮ
            // ========================================================
            workspacePanel = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(242, 246, 250)
            };
            this.Controls.Add(workspacePanel);
            workspacePanel.BringToFront();

            // ========================================================
            // 3. КОНТЕЙНЕР ГОЛОВНОГО МЕНЮ (Вставляється у workspacePanel)
            // ========================================================
            mainMenuContainer = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(242, 246, 250)
            };
            workspacePanel.Controls.Add(mainMenuContainer);

            // Ліва темно-синя смуга
            Panel leftBlueStripe = new Panel()
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(20, 45, 75)
            };
            mainMenuContainer.Controls.Add(leftBlueStripe);

            // Права частина головного меню
            Panel menuContentArea = new Panel()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40)
            };
            mainMenuContainer.Controls.Add(menuContentArea);
            menuContentArea.BringToFront();

            // Аватарка з першою літерою
            lblAvatar = new Label()
            {
                Size = new Size(55, 55),
                Location = new Point(40, 30),
                BackColor = Color.FromArgb(30, 135, 225),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "M"
            };
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, lblAvatar.Width, lblAvatar.Height);
            lblAvatar.Region = new Region(path);
            menuContentArea.Controls.Add(lblAvatar);

            // Статус користувача
            lblUserStatus = new Label()
            {
                Location = new Point(110, 43),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Text = "Locked: Waiting for authorization..."
            };
            menuContentArea.Controls.Add(lblUserStatus);

            // Заголовок
            lblTitle = new Label()
            {
                Text = "SKI HOTEL",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 45, 75),
                AutoSize = true,
                Location = new Point(38, 105)
            };
            menuContentArea.Controls.Add(lblTitle);

            // Підзаголовок
            lblSubtitle = new Label()
            {
                Text = "Ski resort management terminal system",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 115, 130),
                AutoSize = true,
                Location = new Point(42, 155)
            };
            menuContentArea.Controls.Add(lblSubtitle);

            // Кнопки в стовпчик
            int startX = 40;
            int startY = 210;
            int btnHeight = 65;
            int spacing = 18;

            btnNewOrder = CreateMenuButton("NEW ORDER", new Point(startX, startY));
            btnClients = CreateMenuButton("CLIENTS", new Point(startX, startY + (btnHeight + spacing) * 1));
            btnEquipment = CreateMenuButton("EQUIPMENT", new Point(startX, startY + (btnHeight + spacing) * 2));
            btnStatistics = CreateMenuButton("STATISTICS", new Point(startX, startY + (btnHeight + spacing) * 3));

            menuContentArea.Controls.Add(btnNewOrder);
            menuContentArea.Controls.Add(btnClients);
            menuContentArea.Controls.Add(btnEquipment);
            menuContentArea.Controls.Add(btnStatistics);

            // Події відкриття форм НА МІСЦІ головного меню
            btnNewOrder.Click += (s, e) => OpenFormInWorkspace(new OrderForm());
            btnClients.Click += (s, e) => OpenFormInWorkspace(new ClientsForm());
            btnEquipment.Click += (s, e) => OpenFormInWorkspace(new EquipmentForm());
            btnStatistics.Click += (s, e) => OpenFormInWorkspace(new ReviewsForm());

            // Текст безпеки в самому низу
            lblSecurityFooter = new Label()
            {
                Text = "Secured with SHA-256 database encryption & LocalDB protocol — Terminal v2.4",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(140, 155, 170),
                AutoSize = true,
                Dock = DockStyle.Bottom
            };
            menuContentArea.Controls.Add(lblSecurityFooter);

            SetButtonsAccess(false);
        }

        // Метод для відкриття будь-якої форми прямо замість головного меню
        private void OpenFormInWorkspace(Form childForm)
        {
            if (activeChildForm != null)
            {
                activeChildForm.Close();
            }

            activeChildForm = childForm;

            // Ховаємо головне меню
            mainMenuContainer.Visible = false;

            // Показуємо кнопку "Повернутись на головне меню" зверху
            btnBackToMenu.Visible = true;

            // Налаштовуємо дочірню форму, щоб вона вбудувалася без рамки на всю робочу область
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            // Якщо форма закриється зсередини (наприклад, через this.Close()), повертаємо головне меню
            childForm.FormClosed += (s, args) => ReturnToMainMenu();

            workspacePanel.Controls.Add(childForm);
            childForm.BringToFront();
            childForm.Show();
        }

        // Метод повернення до головного меню
        private void ReturnToMainMenu()
        {
            if (activeChildForm != null)
            {
                Form temp = activeChildForm;
                activeChildForm = null; // Захист від повторного виклику при FormClosed
                temp.Close();
                temp.Dispose();
            }

            btnBackToMenu.Visible = false;
            mainMenuContainer.Visible = true;
            mainMenuContainer.BringToFront();
        }

        private Button CreateNavButton(string text, Color bgColor, Point location, int width)
        {
            Button btn = new Button()
            {
                Text = text,
                Location = location,
                Size = new Size(width, 36),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = bgColor,
                ForeColor = Color.FromArgb(230, 238, 245),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Color.FromArgb(42, 72, 108);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(32, 62, 98);
            return btn;
        }

        private Button CreateMenuButton(string text, Point location)
        {
            Button button = new Button()
            {
                Text = text,
                Size = new Size(650, 65),
                Location = location,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(20, 45, 75),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(205, 215, 225);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 242, 250);
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
            GraphicsPath path = new GraphicsPath();
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
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += (sender, args) => { loginForm.Close(); };

            loginForm.Controls.Add(lblHeader);
            loginForm.Controls.Add(lblSubHeader);
            loginForm.Controls.Add(lblUser);
            loginForm.Controls.Add(cmbUser);
            loginForm.Controls.Add(lblPass);
            loginForm.Controls.Add(txtPass);
            loginForm.Controls.Add(btnLogin);
            loginForm.AcceptButton = btnLogin;

            DialogResult result = loginForm.ShowDialog(this);

            // Якщо користувач закрив вікно авторизації на хрестик
            if (result != DialogResult.OK && string.IsNullOrEmpty(currentUsername))
            {
                Application.Exit();
                return;
            }

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
                lblAvatar.Text = currentUsername.Substring(0, 1).ToUpper();
                lblUserStatus.Text = $"Logged in as: {currentUsername} ({currentRole})";
            }
            else
            {
                MessageBox.Show("Incorrect password! Access denied.", "Authorization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (string.IsNullOrEmpty(currentUsername))
                {
                    Application.Exit();
                }
                else
                {
                    ShowLoginDialog();
                }
            }
        }
    }
}