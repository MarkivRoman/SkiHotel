using System;
using System.Drawing;
using System.Windows.Forms;
using SkiHotel.Models;
using SkiHotel.Services;

namespace SkiHotel.Forms
{
    public partial class OrderForm : Form
    {
        private TextBox txtName;
        private TextBox txtPhone;
        private TextBox txtEmail;

        private CheckBox chkOwnEquipment;
        private GroupBox grpEquipment;

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

        private decimal currentTotal = 0;

        public OrderForm()
        {
            InitializeComponent();
            CreateOrderForm();
        }

        private void CreateOrderForm()
        {
            Text = "Нове замовлення - SKI HOTEL";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(850, 800);
            MinimumSize = new Size(850, 800);
            BackColor = Color.FromArgb(235, 242, 248);
            Font = new Font("Segoe UI", 10);

            Label title = new Label
            {
                Text = "Нове замовлення",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 90),
                Location = new Point(30, 20),
                AutoSize = true
            };
            Controls.Add(title);

            // Дані клієнта
            GroupBox clientGroup = new GroupBox
            {
                Text = "Дані клієнта",
                Location = new Point(30, 75),
                Size = new Size(370, 190)
            };
            Controls.Add(clientGroup);

            AddLabel(clientGroup, "Ім'я та прізвище:", new Point(15, 35));
            txtName = new TextBox { Location = new Point(155, 32), Width = 190 };
            clientGroup.Controls.Add(txtName);

            AddLabel(clientGroup, "Телефон:", new Point(15, 80));
            txtPhone = new TextBox { Location = new Point(155, 77), Width = 190 };
            clientGroup.Controls.Add(txtPhone);

            AddLabel(clientGroup, "Email:", new Point(15, 125));
            txtEmail = new TextBox { Location = new Point(155, 122), Width = 190 };
            clientGroup.Controls.Add(txtEmail);

            // Спорядження
            grpEquipment = new GroupBox
            {
                Text = "Параметри для підбору спорядження",
                Location = new Point(420, 75),
                Size = new Size(380, 300)
            };
            Controls.Add(grpEquipment);

            chkOwnEquipment = new CheckBox
            {
                Text = "Клієнт має власне спорядження",
                Location = new Point(15, 30),
                Width = 330
            };
            chkOwnEquipment.CheckedChanged += ChkOwnEquipment_CheckedChanged;
            grpEquipment.Controls.Add(chkOwnEquipment);

            AddLabel(grpEquipment, "Вага (кг):", new Point(15, 75));
            nudWeight = new NumericUpDown { Location = new Point(180, 72), Width = 100, Minimum = 20, Maximum = 200, Value = 70 };
            grpEquipment.Controls.Add(nudWeight);

            AddLabel(grpEquipment, "Зріст (см):", new Point(15, 115));
            nudHeight = new NumericUpDown { Location = new Point(180, 112), Width = 100, Minimum = 100, Maximum = 230, Value = 175 };
            grpEquipment.Controls.Add(nudHeight);

            AddLabel(grpEquipment, "Розмір взуття:", new Point(15, 155));
            nudShoeSize = new NumericUpDown { Location = new Point(180, 152), Width = 100, Minimum = 30, Maximum = 55, Value = 42 };
            grpEquipment.Controls.Add(nudShoeSize);

            AddLabel(grpEquipment, "Рівень катання:", new Point(15, 195));
            cmbSkill = new ComboBox
            {
                Location = new Point(180, 192),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSkill.Items.AddRange(new string[] { "Початківець", "Середній", "Професіонал" });
            cmbSkill.SelectedIndex = 0;
            grpEquipment.Controls.Add(cmbSkill);

            lblSkis = CreateResultLabel("Лижі: не підібрано", new Point(15, 230));
            lblBoots = CreateResultLabel("Черевики: не підібрано", new Point(15, 250));
            lblPoles = CreateResultLabel("Палки: не підібрано", new Point(15, 270));

            grpEquipment.Controls.Add(lblSkis);
            grpEquipment.Controls.Add(lblBoots);
            grpEquipment.Controls.Add(lblPoles);

            // Абонемент
            GroupBox passGroup = new GroupBox
            {
                Text = "Абонемент та готель",
                Location = new Point(30, 285),
                Size = new Size(370, 250)
            };
            Controls.Add(passGroup);

            AddLabel(passGroup, "Кількість днів:", new Point(15, 35));
            nudDays = new NumericUpDown { Location = new Point(180, 32), Width = 100, Minimum = 1, Maximum = 7, Value = 1 };
            passGroup.Controls.Add(nudDays);

            chkHotel = new CheckBox { Text = "Забронювати номер у готелі", Location = new Point(15, 80), Width = 300 };
            passGroup.Controls.Add(chkHotel);

            chkBreakfast = new CheckBox { Text = "Сніданок", Location = new Point(15, 120) };
            passGroup.Controls.Add(chkBreakfast);

            chkSauna = new CheckBox { Text = "Сауна", Location = new Point(15, 160) };
            passGroup.Controls.Add(chkSauna);

            // Розрахунок
            btnCalculate = new Button
            {
                Text = "РОЗРАХУВАТИ ВАРТІСТЬ",
                Location = new Point(30, 560),
                Size = new Size(370, 55),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.White
            };
            btnCalculate.Click += BtnCalculate_Click;
            Controls.Add(btnCalculate);

            lblTotal = new Label
            {
                Text = "Вартість: 0 грн",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 100, 70),
                Location = new Point(430, 400),
                AutoSize = true
            };
            Controls.Add(lblTotal);

            btnBook = new Button
            {
                Text = "ГОТОВО — ЗАБРОНЮВАТИ",
                Location = new Point(430, 560),
                Size = new Size(370, 55),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(190, 230, 200),
                Enabled = false
            };
            btnBook.Click += BtnBook_Click;
            Controls.Add(btnBook);
        }

        private void AddLabel(Control parent, string text, Point location)
        {
            Label label = new Label { Text = text, Location = location, AutoSize = true };
            parent.Controls.Add(label);
        }

        private Label CreateResultLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Italic)
            };
        }

        private void ChkOwnEquipment_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = !chkOwnEquipment.Checked;
            nudWeight.Enabled = enabled;
            nudHeight.Enabled = enabled;
            nudShoeSize.Enabled = enabled;
            cmbSkill.Enabled = enabled;

            if (!enabled)
            {
                lblSkis.Text = "Лижі: власні";
                lblBoots.Text = "Черевики: власні";
                lblPoles.Text = "Палки: власні";
            }
            else
            {
                lblSkis.Text = "Лижі: не підібрано";
                lblBoots.Text = "Черевики: не підібрано";
                lblPoles.Text = "Палки: не підібрано";
            }
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введіть ім'я та прізвище клієнта.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text) && string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Введіть телефон або електронну пошту клієнта.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentTotal = 0;
            int days = (int)nudDays.Value;

            currentTotal += days * 1000;

            if (!chkOwnEquipment.Checked)
            {
                string skiLength = EquipmentService.GetSkiLength((double)nudHeight.Value, cmbSkill.SelectedItem?.ToString() ?? "Початківець");
                string poleLength = EquipmentService.GetPoleLength((double)nudHeight.Value);
                string bootSize = EquipmentService.GetBootSize((int)nudShoeSize.Value);

                lblSkis.Text = $"Лижі: {skiLength}";
                lblBoots.Text = $"Черевики: {bootSize}";
                lblPoles.Text = $"Палки: {poleLength}";

                currentTotal += 800;
            }
            else
            {
                lblSkis.Text = "Лижі: власні";
                lblBoots.Text = "Черевики: власні";
                lblPoles.Text = "Палки: власні";
            }

            if (chkHotel.Checked) currentTotal += days * 2500;
            if (chkBreakfast.Checked) currentTotal += days * 300;
            if (chkSauna.Checked) currentTotal += 500;

            lblTotal.Text = $"Вартість бронювання: {currentTotal} грн";
            btnBook.Enabled = true;
        }

        private void BtnBook_Click(object sender, EventArgs e)
        {
            if (currentTotal <= 0)
            {
                MessageBox.Show("Спочатку розрахуйте вартість.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string skill = cmbSkill.SelectedItem?.ToString() ?? "Початківець";

            Order order = new Order
            {
                ClientName = txtName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                HasEquipment = chkOwnEquipment.Checked ? "Так" : "Ні",
                Weight = nudWeight.Value.ToString() + " кг",
                Height = nudHeight.Value.ToString() + " см",
                ShoeSize = nudShoeSize.Value.ToString(),
                SkillLevel = skill,
                SkiPassDays = (int)nudDays.Value,
                HotelRoom = chkHotel.Checked ? "Так" : "Ні",
                Breakfast = chkBreakfast.Checked,
                Sauna = chkSauna.Checked,
                TotalPrice = currentTotal,
                Status = "Вхідні" // Нові заявки спочатку падають у вхідні
            };

            if (chkOwnEquipment.Checked)
            {
                order.Skis = "Власні";
                order.Boots = "Власні";
                order.Poles = "Власні";
            }
            else
            {
                order.Skis = EquipmentService.GetSkiLength((double)nudHeight.Value, skill);
                order.Boots = EquipmentService.GetBootSize((int)nudShoeSize.Value);
                order.Poles = EquipmentService.GetPoleLength((double)nudHeight.Value);
            }

            OrderService.SaveOrder(order);

            MessageBox.Show(
                "Замовлення успішно надіслано!\n\n" +
                $"Клієнт: {order.ClientName}\n" +
                $"Сума: {order.TotalPrice} грн\n" +
                "Статус: Вхідні (очікує підтвердження адміном)",
                "Бронювання успішне",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            Close();
        }

        private void OrderForm_Load(object sender, EventArgs e) { }
    }
}