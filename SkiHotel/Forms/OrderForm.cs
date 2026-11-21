using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SkiHotel.Models;
using SkiHotel.Services;

namespace SkiHotel.Forms
{
    public partial class OrderForm : Form
    {
        private Label lblHeaderTitle;
        private Label lblClientCardTitle;
        private Label lblPassCardTitle;
        private Label lblEquipmentCardTitle;

        private Label lblName;
        private Label lblPhone;
        private Label lblEmail;

        private Label lblDays;
        private Label lblWeight;
        private Label lblHeight;
        private Label lblShoeSize;
        private Label lblSkill;

        private TextBox txtName;
        private TextBox txtPhone;
        private TextBox txtEmail;

        private CheckBox chkOwnEquipment;
        private Panel grpEquipment;

        private NumericUpDown nudWeight;
        private NumericUpDown nudHeight;
        private NumericUpDown nudShoeSize;

        private ComboBox cmbSkill;
        private NumericUpDown nudDays;

        private CheckBox chkHotel;
        private CheckBox chkBreakfast;
        private CheckBox chkSauna;

        private Label lblSkis;
        private Label lblBoots;
        private Label lblPoles;
        private Label lblTotal;

        private Button btnCalculate;
        private Button btnBook;

        private PictureBox avatarBox;
        private decimal currentTotal = 0;
        private bool _isBookingSubmitted = false;
        private bool _isCalculated = false;

        public OrderForm()
        {
            InitializeComponent();
            CreateOrderForm();
            LocalizationManager.LanguageChanged += ApplyLanguage;
            this.FormClosed += (s, e) => LocalizationManager.LanguageChanged -= ApplyLanguage;
            ApplyLanguage();
        }

        private void CreateOrderForm()
        {
            BackColor = Color.FromArgb(240, 245, 250);
            Font = new Font("Segoe UI", 10);

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };

            headerPanel.Paint += (s, e) => {
                using (Pen p = new Pen(Color.FromArgb(210, 220, 230), 1))
                {
                    e.Graphics.DrawLine(p, 0, headerPanel.Height - 1, headerPanel.Width, headerPanel.Height - 1);
                }
            };

            lblHeaderTitle = new Label
            {
                Text = "New Order and Booking Creation",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 90),
                Location = new Point(20, 16),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblHeaderTitle);
            Controls.Add(headerPanel);

            Panel mainPanel = new Panel
            {
                Location = new Point(15, 80),
                Size = new Size(800, 480),
                BackColor = Color.Transparent,
                AutoScroll = true
            };
            Controls.Add(mainPanel);

            Panel clientPanel = CreateCardPanel(new Point(10, 10), new Size(380, 175), "Client Personal Data", out lblClientCardTitle);

            avatarBox = new PictureBox
            {
                Size = new Size(35, 35),
                Location = new Point(325, 12),
                BackColor = Color.Transparent
            };
            avatarBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 60, 90)))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, 34, 34);
                }

                string nameText = txtName != null ? txtName.Text.Trim() : "";
                string initial = string.IsNullOrEmpty(nameText) ? "?" : nameText.Substring(0, 1).ToUpper();
                using (Font font = new Font("Segoe UI", 12, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, initial, font, new Rectangle(0, 0, 35, 35), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };
            clientPanel.Controls.Add(avatarBox);

            lblName = AddLabel(clientPanel, "Full Name:", new Point(15, 38));
            txtName = new TextBox { Location = new Point(125, 35), Width = 230, Font = new Font("Segoe UI", 10) };
            txtName.TextChanged += (s, e) => avatarBox.Invalidate();
            clientPanel.Controls.Add(txtName);

            lblPhone = AddLabel(clientPanel, "Phone:", new Point(15, 76));
            txtPhone = new TextBox { Location = new Point(125, 73), Width = 230, Font = new Font("Segoe UI", 10) };
            clientPanel.Controls.Add(txtPhone);

            lblEmail = AddLabel(clientPanel, "Email:", new Point(15, 114));
            txtEmail = new TextBox { Location = new Point(125, 111), Width = 230, Font = new Font("Segoe UI", 10) };
            clientPanel.Controls.Add(txtEmail);

            mainPanel.Controls.Add(clientPanel);

            Panel passPanel = CreateCardPanel(new Point(10, 195), new Size(380, 175), "Accommodation and Skipass", out lblPassCardTitle);

            lblDays = AddLabel(passPanel, "Number of Days:", new Point(15, 38));
            nudDays = new NumericUpDown { Location = new Point(165, 35), Width = 90, Minimum = 1, Maximum = 14, Value = 3, Font = new Font("Segoe UI", 10) };
            passPanel.Controls.Add(nudDays);

            chkHotel = new CheckBox { Text = "Hotel room", Location = new Point(15, 72), Width = 250, Font = new Font("Segoe UI", 10) };
            passPanel.Controls.Add(chkHotel);

            chkBreakfast = new CheckBox { Text = "Breakfast (+300 UAH/day)", Location = new Point(15, 103), Width = 280, Font = new Font("Segoe UI", 10) };
            passPanel.Controls.Add(chkBreakfast);

            chkSauna = new CheckBox { Text = "Sauna (+500 UAH)", Location = new Point(15, 134), Width = 250, Font = new Font("Segoe UI", 10) };
            passPanel.Controls.Add(chkSauna);

            mainPanel.Controls.Add(passPanel);

            grpEquipment = CreateCardPanel(new Point(400, 10), new Size(390, 360), "Equipment Parameters", out lblEquipmentCardTitle);

            chkOwnEquipment = new CheckBox
            {
                Text = "Client has own equipment",
                Location = new Point(15, 35),
                Width = 340,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 140, 60)
            };
            chkOwnEquipment.CheckedChanged += ChkOwnEquipment_CheckedChanged;
            grpEquipment.Controls.Add(chkOwnEquipment);

            lblWeight = AddLabel(grpEquipment, "Weight (kg):", new Point(15, 72));
            nudWeight = new NumericUpDown { Location = new Point(170, 69), Width = 95, Minimum = 20, Maximum = 200, Value = 70, Font = new Font("Segoe UI", 10) };
            grpEquipment.Controls.Add(nudWeight);

            lblHeight = AddLabel(grpEquipment, "Height (cm):", new Point(15, 107));
            nudHeight = new NumericUpDown { Location = new Point(170, 104), Width = 95, Minimum = 100, Maximum = 230, Value = 175, Font = new Font("Segoe UI", 10) };
            grpEquipment.Controls.Add(nudHeight);

            lblShoeSize = AddLabel(grpEquipment, "Shoe Size:", new Point(15, 142));
            nudShoeSize = new NumericUpDown { Location = new Point(170, 139), Width = 95, Minimum = 30, Maximum = 50, Value = 42, Font = new Font("Segoe UI", 10) };
            grpEquipment.Controls.Add(nudShoeSize);

            lblSkill = AddLabel(grpEquipment, "Skill Level:", new Point(15, 177));
            cmbSkill = new ComboBox
            {
                Location = new Point(170, 174),
                Width = 175,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbSkill.Items.AddRange(new string[] { "Beginner", "Intermediate", "Professional" });
            cmbSkill.SelectedIndex = 0;
            grpEquipment.Controls.Add(cmbSkill);

            Panel resultPanel = new Panel
            {
                Location = new Point(15, 212),
                Size = new Size(355, 125),
                BackColor = Color.FromArgb(245, 248, 252)
            };
            resultPanel.Paint += (s, e) =>
            {
                using (Pen p = new Pen(Color.FromArgb(210, 220, 235), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, resultPanel.Width - 1, resultPanel.Height - 1);
                }
            };

            lblSkis = CreateResultLabel("Skis: not selected", new Point(12, 14));
            lblBoots = CreateResultLabel("Boots: not selected", new Point(12, 44));
            lblPoles = CreateResultLabel("Poles: not selected", new Point(12, 74));

            resultPanel.Controls.Add(lblSkis);
            resultPanel.Controls.Add(lblBoots);
            resultPanel.Controls.Add(lblPoles);
            grpEquipment.Controls.Add(resultPanel);

            mainPanel.Controls.Add(grpEquipment);

            Panel bottomPanel = new Panel
            {
                Location = new Point(10, 380),
                Size = new Size(780, 90),
                BackColor = Color.White
            };
            bottomPanel.Paint += (s, e) => {
                using (Pen p = new Pen(Color.FromArgb(210, 220, 230), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, bottomPanel.Width - 1, bottomPanel.Height - 1);
                }
            };

            lblTotal = new Label
            {
                Text = "Total cost: 0 UAH",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 100, 70),
                Location = new Point(15, 30),
                AutoSize = true
            };
            bottomPanel.Controls.Add(lblTotal);

            btnCalculate = new Button
            {
                Text = "CALCULATE",
                Location = new Point(410, 25),
                Size = new Size(165, 40),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 60, 90),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCalculate.FlatAppearance.BorderSize = 0;
            btnCalculate.Click += BtnCalculate_Click;
            bottomPanel.Controls.Add(btnCalculate);

            btnBook = new Button
            {
                Text = "BOOK",
                Location = new Point(585, 25),
                Size = new Size(165, 40),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 140, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnBook.FlatAppearance.BorderSize = 0;
            btnBook.Click += BtnBook_Click;
            bottomPanel.Controls.Add(btnBook);

            mainPanel.Controls.Add(bottomPanel);
        }

        private string GetSelectedSkillEnglish()
        {
            switch (cmbSkill.SelectedIndex)
            {
                case 1: return "Intermediate";
                case 2: return "Professional";
                default: return "Beginner";
            }
        }

        private void ApplyLanguage()
        {
            bool isUk = LocalizationManager.AppLanguage == "uk";

            lblHeaderTitle.Text = isUk ? "Нове замовлення та бронювання" : "New Order and Booking Creation";
            lblClientCardTitle.Text = isUk ? "Особисті дані клієнта" : "Client Personal Data";
            lblPassCardTitle.Text = isUk ? "Проживання та скі-пас" : "Accommodation and Skipass";
            lblEquipmentCardTitle.Text = isUk ? "Параметри спорядження" : "Equipment Parameters";

            lblName.Text = isUk ? "ПІБ клієнта:" : "Full Name:";
            lblPhone.Text = isUk ? "Телефон:" : "Phone:";
            lblEmail.Text = "Email:";

            lblDays.Text = isUk ? "Кількість днів:" : "Number of Days:";
            chkHotel.Text = isUk ? "Номер у готелі" : "Hotel room";
            chkBreakfast.Text = isUk ? "Сніданок (+300 грн/день)" : "Breakfast (+300 UAH/day)";
            chkSauna.Text = isUk ? "Сауна (+500 грн)" : "Sauna (+500 UAH)";

            chkOwnEquipment.Text = isUk ? "Клієнт має власне спорядження" : "Client has own equipment";
            lblWeight.Text = isUk ? "Вага (кг):" : "Weight (kg):";
            lblHeight.Text = isUk ? "Зріст (см):" : "Height (cm):";
            lblShoeSize.Text = isUk ? "Розмір взуття:" : "Shoe Size:";
            lblSkill.Text = isUk ? "Рівень катання:" : "Skill Level:";

            int selectedSkillIdx = cmbSkill.SelectedIndex;
            cmbSkill.Items.Clear();
            if (isUk)
            {
                cmbSkill.Items.AddRange(new string[] { "Початківець", "Середній", "Професіонал" });
            }
            else
            {
                cmbSkill.Items.AddRange(new string[] { "Beginner", "Intermediate", "Professional" });
            }
            cmbSkill.SelectedIndex = selectedSkillIdx >= 0 ? selectedSkillIdx : 0;

            UpdateEquipmentLabels();

            lblTotal.Text = isUk
                ? $"Загальна вартість: {currentTotal} грн"
                : $"Total cost: {currentTotal} UAH";

            btnCalculate.Text = isUk ? "РОЗРАХУВАТИ" : "CALCULATE";

            if (_isBookingSubmitted)
            {
                btnBook.Text = isUk ? "ВІДПРАВЛЕНО" : "SENT";
            }
            else
            {
                btnBook.Text = isUk ? "ЗАБРОНЮВАТИ" : "BOOK";
            }
        }

        private void UpdateEquipmentLabels()
        {
            bool isUk = LocalizationManager.AppLanguage == "uk";

            if (chkOwnEquipment.Checked)
            {
                lblSkis.Text = isUk ? "Лижі: Власні" : "Skis: Own";
                lblBoots.Text = isUk ? "Черевики: Власні" : "Boots: Own";
                lblPoles.Text = isUk ? "Палиці: Власні" : "Poles: Own";
            }
            else if (_isCalculated)
            {
                string skillEn = GetSelectedSkillEnglish();
                string skiLength = EquipmentService.GetSkiLength((double)nudHeight.Value, skillEn);
                string poleLength = EquipmentService.GetPoleLength((double)nudHeight.Value);
                string bootSize = EquipmentService.GetBootSize((int)nudShoeSize.Value);

                lblSkis.Text = isUk ? $"Лижі: {skiLength}" : $"Skis: {skiLength}";
                lblBoots.Text = isUk ? $"Черевики: {bootSize}" : $"Boots: {bootSize}";
                lblPoles.Text = isUk ? $"Палиці: {poleLength}" : $"Poles: {poleLength}";
            }
            else
            {
                lblSkis.Text = isUk ? "Лижі: не обрано" : "Skis: not selected";
                lblBoots.Text = isUk ? "Черевики: не обрано" : "Boots: not selected";
                lblPoles.Text = isUk ? "Палиці: не обрано" : "Poles: not selected";
            }
        }

        private Panel CreateCardPanel(Point location, Size size, string titleText, out Label titleLabel)
        {
            Panel card = new Panel
            {
                Location = location,
                Size = size,
                BackColor = Color.White
            };

            card.Paint += (s, e) => {
                using (Pen p = new Pen(Color.FromArgb(210, 220, 230), 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            titleLabel = new Label
            {
                Text = titleText,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 90),
                Location = new Point(15, 12),
                AutoSize = true
            };
            card.Controls.Add(titleLabel);

            return card;
        }

        private Label AddLabel(Control parent, string text, Point location)
        {
            Label label = new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 90, 100)
            };
            parent.Controls.Add(label);
            return label;
        }

        private Label CreateResultLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 70)
            };
        }

        private void ChkOwnEquipment_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = !chkOwnEquipment.Checked;
            nudWeight.Enabled = enabled;
            nudHeight.Enabled = enabled;
            nudShoeSize.Enabled = enabled;
            cmbSkill.Enabled = enabled;
            _isCalculated = false;

            UpdateEquipmentLabels();
        }

        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return Regex.IsMatch(name.Trim(), @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ'\-\s]{2,50}$");
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            if (phone.Any(char.IsLetter))
                return false;

            string cleaned = Regex.Replace(phone.Trim(), @"[\s\-\(\)]", "");
            return Regex.IsMatch(cleaned, @"^(\+?38)?0\d{9}$");
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");
        }

        private bool ValidateClientInputs()
        {
            bool isUk = LocalizationManager.AppLanguage == "uk";

            if (!IsValidName(txtName.Text))
            {
                MessageBox.Show(
                    isUk
                        ? "Введіть коректне ПІБ клієнта (тільки літери, мінімум 2 символи, без цифр)."
                        : "Please enter a valid Full Name (letters only, at least 2 characters, no digits or symbols).",
                    isUk ? "Помилка валідації" : "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                txtName.Focus();
                return false;
            }

            if (!IsValidPhone(txtPhone.Text))
            {
                MessageBox.Show(
                    isUk
                        ? "Введіть коректний номер телефону (наприклад, +380XXXXXXXXX або 0XXXXXXXXX)."
                        : "Please enter a valid phone number (e.g., +380XXXXXXXXX or 0XXXXXXXXX).",
                    isUk ? "Помилка валідації" : "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                txtPhone.Focus();
                return false;
            }

            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show(
                    isUk
                        ? "Введіть коректну електронну пошту (наприклад, client@gmail.com)."
                        : "Please enter a valid Email address (e.g., client@gmail.com).",
                    isUk ? "Помилка валідації" : "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                txtEmail.Focus();
                return false;
            }

            return true;
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            if (!ValidateClientInputs())
                return;

            bool isUk = LocalizationManager.AppLanguage == "uk";
            currentTotal = 0;
            int days = (int)nudDays.Value;

            currentTotal += days * 1000;

            if (!chkOwnEquipment.Checked)
            {
                currentTotal += 800;
            }

            if (chkHotel.Checked) currentTotal += days * 2500;
            if (chkBreakfast.Checked) currentTotal += days * 300;
            if (chkSauna.Checked) currentTotal += 500;

            _isCalculated = true;
            UpdateEquipmentLabels();

            lblTotal.Text = isUk
                ? $"Загальна вартість: {currentTotal} грн"
                : $"Total cost: {currentTotal} UAH";

            if (!_isBookingSubmitted)
            {
                btnBook.Enabled = true;
            }
        }

        private async void BtnBook_Click(object sender, EventArgs e)
        {
            if (_isBookingSubmitted)
                return;

            if (!ValidateClientInputs())
                return;

            bool isUk = LocalizationManager.AppLanguage == "uk";

            if (currentTotal <= 0)
            {
                MessageBox.Show(
                    isUk ? "Спочатку розрахуйте вартість." : "Calculate the cost first.",
                    isUk ? "Увага" : "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            _isBookingSubmitted = true;
            btnBook.Enabled = false;
            btnBook.Text = isUk ? "ВІДПРАВКА..." : "SENDING...";

            try
            {
                string skill = GetSelectedSkillEnglish();

                Order order = new Order
                {
                    ClientName = txtName.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    HasEquipment = chkOwnEquipment.Checked ? "Yes" : "No",
                    Weight = nudWeight.Value.ToString() + " kg",
                    Height = nudHeight.Value.ToString() + " cm",
                    ShoeSize = nudShoeSize.Value.ToString(),
                    SkillLevel = skill,
                    SkiPassDays = (int)nudDays.Value,
                    HotelRoom = chkHotel.Checked ? "Yes" : "No",
                    Breakfast = chkBreakfast.Checked,
                    Sauna = chkSauna.Checked,
                    TotalPrice = currentTotal,
                    Status = "Incoming"
                };

                if (chkOwnEquipment.Checked)
                {
                    order.Skis = "Own";
                    order.Boots = "Own";
                    order.Poles = "Own";
                }
                else
                {
                    order.Skis = EquipmentService.GetSkiLength((double)nudHeight.Value, skill);
                    order.Boots = EquipmentService.GetBootSize((int)nudShoeSize.Value);
                    order.Poles = EquipmentService.GetPoleLength((double)nudHeight.Value);
                }

                OrderService.SaveOrder(order);
                await TelegramBotHandler.SendNewOrderNotificationAsync(order);

                btnBook.Text = isUk ? "ВІДПРАВЛЕНО" : "SENT";

                MessageBox.Show(
                    isUk
                        ? $"Бронювання успішно оформлено та додано до системи!\n\nКлієнт: {order.ClientName}\nСума: {order.TotalPrice} грн\nСтатус: Очікує підтвердження"
                        : $"Booking successfully submitted and added to the system!\n\nClient: {order.ClientName}\nAmount: {order.TotalPrice} UAH\nStatus: Incoming (waiting for admin confirmation)",
                    isUk ? "Успішно" : "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                Close();
            }
            catch (Exception ex)
            {
                _isBookingSubmitted = false;
                btnBook.Enabled = true;
                btnBook.Text = isUk ? "ЗАБРОНЮВАТИ" : "BOOK";
                MessageBox.Show(
                    isUk ? $"Помилка при оформленні замовлення: {ex.Message}" : $"Failed to submit booking: {ex.Message}",
                    isUk ? "Помилка" : "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void OrderForm_Load(object sender, EventArgs e) { }
    }
}