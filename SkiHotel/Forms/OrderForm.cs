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
            // ==========================
            // НАЛАШТУВАННЯ ВІКНА
            // ==========================

            Text = "Нове замовлення - SKI HOTEL";

            StartPosition = FormStartPosition.CenterScreen;

            Size = new Size(850, 800);

            MinimumSize = new Size(850, 800);

            BackColor = Color.FromArgb(235, 242, 248);

            Font = new Font("Segoe UI", 10);

            // ==========================
            // ЗАГОЛОВОК
            // ==========================

            Label title = new Label();

            title.Text = "Нове замовлення";

            title.Font = new Font(
                "Segoe UI",
                24,
                FontStyle.Bold
            );

            title.ForeColor =
                Color.FromArgb(30, 60, 90);

            title.Location =
                new Point(30, 20);

            title.AutoSize = true;

            Controls.Add(title);

            // ==========================
            // ДАНІ КЛІЄНТА
            // ==========================

            GroupBox clientGroup = new GroupBox();

            clientGroup.Text = "Дані клієнта";

            clientGroup.Location =
                new Point(30, 75);

            clientGroup.Size =
                new Size(370, 190);

            Controls.Add(clientGroup);

            AddLabel(
                clientGroup,
                "Ім'я та прізвище:",
                new Point(15, 35)
            );

            txtName = new TextBox();

            txtName.Location =
                new Point(155, 32);

            txtName.Width = 190;

            clientGroup.Controls.Add(txtName);

            AddLabel(
                clientGroup,
                "Телефон:",
                new Point(15, 80)
            );

            txtPhone = new TextBox();

            txtPhone.Location =
                new Point(155, 77);

            txtPhone.Width = 190;

            clientGroup.Controls.Add(txtPhone);

            AddLabel(
                clientGroup,
                "Email:",
                new Point(15, 125)
            );

            txtEmail = new TextBox();

            txtEmail.Location =
                new Point(155, 122);

            txtEmail.Width = 190;

            clientGroup.Controls.Add(txtEmail);

            // ==========================
            // СПОРЯДЖЕННЯ
            // ==========================

            grpEquipment = new GroupBox();

            grpEquipment.Text =
                "Параметри для підбору спорядження";

            grpEquipment.Location =
                new Point(420, 75);

            grpEquipment.Size =
                new Size(380, 300);

            Controls.Add(grpEquipment);

            chkOwnEquipment = new CheckBox();

            chkOwnEquipment.Text =
                "Клієнт має власне спорядження";

            chkOwnEquipment.Location =
                new Point(15, 30);

            chkOwnEquipment.Width = 330;

            chkOwnEquipment.CheckedChanged +=
                ChkOwnEquipment_CheckedChanged;

            grpEquipment.Controls.Add(
                chkOwnEquipment
            );

            // Вага

            AddLabel(
                grpEquipment,
                "Вага (кг):",
                new Point(15, 75)
            );

            nudWeight = new NumericUpDown();

            nudWeight.Location =
                new Point(180, 72);

            nudWeight.Width = 100;

            nudWeight.Minimum = 20;
            nudWeight.Maximum = 200;
            nudWeight.Value = 70;

            grpEquipment.Controls.Add(
                nudWeight
            );

            // Зріст

            AddLabel(
                grpEquipment,
                "Зріст (см):",
                new Point(15, 115)
            );

            nudHeight = new NumericUpDown();

            nudHeight.Location =
                new Point(180, 112);

            nudHeight.Width = 100;

            nudHeight.Minimum = 100;
            nudHeight.Maximum = 230;
            nudHeight.Value = 175;

            grpEquipment.Controls.Add(
                nudHeight
            );

            // Розмір взуття

            AddLabel(
                grpEquipment,
                "Розмір взуття:",
                new Point(15, 155)
            );

            nudShoeSize = new NumericUpDown();

            nudShoeSize.Location =
                new Point(180, 152);

            nudShoeSize.Width = 100;

            nudShoeSize.Minimum = 30;
            nudShoeSize.Maximum = 55;
            nudShoeSize.Value = 42;

            grpEquipment.Controls.Add(
                nudShoeSize
            );

            // Рівень

            AddLabel(
                grpEquipment,
                "Рівень катання:",
                new Point(15, 195)
            );

            cmbSkill = new ComboBox();

            cmbSkill.Location =
                new Point(180, 192);

            cmbSkill.Width = 160;

            cmbSkill.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cmbSkill.Items.AddRange(
                new string[]
                {
                    "Початківець",
                    "Середній",
                    "Професіонал"
                });

            cmbSkill.SelectedIndex = 0;

            grpEquipment.Controls.Add(
                cmbSkill
            );

            // Результат підбору

            lblSkis = CreateResultLabel(
                "Лижі: не підібрано",
                new Point(15, 230)
            );

            lblBoots = CreateResultLabel(
                "Черевики: не підібрано",
                new Point(15, 250)
            );

            lblPoles = CreateResultLabel(
                "Палки: не підібрано",
                new Point(15, 270)
            );

            grpEquipment.Controls.Add(lblSkis);
            grpEquipment.Controls.Add(lblBoots);
            grpEquipment.Controls.Add(lblPoles);

            // ==========================
            // АБОНЕМЕНТ
            // ==========================

            GroupBox passGroup = new GroupBox();

            passGroup.Text =
                "Абонемент та готель";

            passGroup.Location =
                new Point(30, 285);

            passGroup.Size =
                new Size(370, 250);

            Controls.Add(passGroup);

            AddLabel(
                passGroup,
                "Кількість днів:",
                new Point(15, 35)
            );

            nudDays = new NumericUpDown();

            nudDays.Location =
                new Point(180, 32);

            nudDays.Width = 100;

            nudDays.Minimum = 1;
            nudDays.Maximum = 7;
            nudDays.Value = 1;

            passGroup.Controls.Add(nudDays);

            chkHotel = new CheckBox();

            chkHotel.Text =
                "Забронювати номер у готелі";

            chkHotel.Location =
                new Point(15, 80);

            chkHotel.Width = 300;

            passGroup.Controls.Add(chkHotel);

            chkBreakfast = new CheckBox();

            chkBreakfast.Text =
                "Сніданок";

            chkBreakfast.Location =
                new Point(15, 120);

            passGroup.Controls.Add(chkBreakfast);

            chkSauna = new CheckBox();

            chkSauna.Text =
                "Сауна";

            chkSauna.Location =
                new Point(15, 160);

            passGroup.Controls.Add(chkSauna);

            // ==========================
            // РОЗРАХУНОК
            // ==========================

            btnCalculate = new Button();

            btnCalculate.Text =
                "РОЗРАХУВАТИ ВАРТІСТЬ";

            btnCalculate.Location =
                new Point(30, 560);

            btnCalculate.Size =
                new Size(370, 55);

            btnCalculate.Font =
                new Font(
                    "Segoe UI",
                    11,
                    FontStyle.Bold
                );

            btnCalculate.BackColor =
                Color.White;

            btnCalculate.Click +=
                BtnCalculate_Click;

            Controls.Add(btnCalculate);

            // ==========================
            // СУМА
            // ==========================

            lblTotal = new Label();

            lblTotal.Text =
                "Вартість: 0 грн";

            lblTotal.Font =
                new Font(
                    "Segoe UI",
                    18,
                    FontStyle.Bold
                );

            lblTotal.ForeColor =
                Color.FromArgb(30, 100, 70);

            lblTotal.Location =
                new Point(430, 400);

            lblTotal.AutoSize = true;

            Controls.Add(lblTotal);

            // ==========================
            // ЗАБРОНЮВАТИ
            // ==========================

            btnBook = new Button();

            btnBook.Text =
                "ГОТОВО — ЗАБРОНЮВАТИ";

            btnBook.Location =
                new Point(430, 560);

            btnBook.Size =
                new Size(370, 55);

            btnBook.Font =
                new Font(
                    "Segoe UI",
                    12,
                    FontStyle.Bold
                );

            btnBook.BackColor =
                Color.FromArgb(190, 230, 200);

            btnBook.Enabled = false;

            btnBook.Click +=
                BtnBook_Click;

            Controls.Add(btnBook);
        }

        private void AddLabel(
            Control parent,
            string text,
            Point location)
        {
            Label label = new Label();

            label.Text = text;

            label.Location = location;

            label.AutoSize = true;

            parent.Controls.Add(label);
        }

        private Label CreateResultLabel(
            string text,
            Point location)
        {
            Label label = new Label();

            label.Text = text;

            label.Location = location;

            label.AutoSize = true;

            label.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Italic
                );

            return label;
        }

        private void ChkOwnEquipment_CheckedChanged(
            object sender,
            EventArgs e)
        {
            bool enabled =
                !chkOwnEquipment.Checked;

            nudWeight.Enabled = enabled;
            nudHeight.Enabled = enabled;
            nudShoeSize.Enabled = enabled;
            cmbSkill.Enabled = enabled;

            if (!enabled)
            {
                lblSkis.Text =
                    "Лижі: власні";

                lblBoots.Text =
                    "Черевики: власні";

                lblPoles.Text =
                    "Палки: власні";
            }
            else
            {
                lblSkis.Text =
                    "Лижі: не підібрано";

                lblBoots.Text =
                    "Черевики: не підібрано";

                lblPoles.Text =
                    "Палки: не підібрано";
            }
        }

        private void BtnCalculate_Click(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                txtName.Text))
            {
                MessageBox.Show(
                    "Введіть ім'я та прізвище клієнта.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtName.Focus();

                return;
            }

            if (string.IsNullOrWhiteSpace(
                txtPhone.Text) &&
                string.IsNullOrWhiteSpace(
                    txtEmail.Text))
            {
                MessageBox.Show(
                    "Введіть телефон або електронну пошту клієнта.",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            currentTotal = 0;

            int days =
                (int)nudDays.Value;

            // Абонемент
            currentTotal += days * 1000;

            // Спорядження
            if (!chkOwnEquipment.Checked)
            {
                string skiLength =
                    EquipmentService.GetSkiLength(
                        (double)nudHeight.Value,
                        cmbSkill.SelectedItem?.ToString()
                        ?? "Початківець"
                    );

                string poleLength =
                    EquipmentService.GetPoleLength(
                        (double)nudHeight.Value
                    );

                string bootSize =
                    EquipmentService.GetBootSize(
                        (int)nudShoeSize.Value
                    );

                lblSkis.Text =
                    $"Лижі: {skiLength}";

                lblBoots.Text =
                    $"Черевики: {bootSize}";

                lblPoles.Text =
                    $"Палки: {poleLength}";

                // Умовна ціна комплекту
                currentTotal += 800;
            }
            else
            {
                lblSkis.Text =
                    "Лижі: власні";

                lblBoots.Text =
                    "Черевики: власні";

                lblPoles.Text =
                    "Палки: власні";
            }

            // Готель
            if (chkHotel.Checked)
            {
                currentTotal += days * 2500;
            }

            // Сніданок
            if (chkBreakfast.Checked)
            {
                currentTotal += days * 300;
            }

            // Сауна
            if (chkSauna.Checked)
            {
                currentTotal += 500;
            }

            lblTotal.Text =
                $"Вартість бронювання: {currentTotal} грн";

            btnBook.Enabled = true;
        }

        private void BtnBook_Click(
            object sender,
            EventArgs e)
        {
            // Перевіряємо, чи вже був розрахунок
            if (currentTotal <= 0)
            {
                MessageBox.Show(
                    "Спочатку розрахуйте вартість.",
                    "Увага",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string skill =
                cmbSkill.SelectedItem?.ToString()
                ?? "Початківець";

            Order order = new Order();

            order.ClientName =
                txtName.Text.Trim();

            order.Phone =
                txtPhone.Text.Trim();

            order.Email =
                txtEmail.Text.Trim();

            order.HasOwnEquipment =
                chkOwnEquipment.Checked;

            order.Weight =
                (double)nudWeight.Value;

            order.Height =
                (double)nudHeight.Value;

            order.ShoeSize =
                (int)nudShoeSize.Value;

            order.SkillLevel =
                skill;

            if (chkOwnEquipment.Checked)
            {
                order.Skis = "Власні";
                order.Boots = "Власні";
                order.Poles = "Власні";
            }
            else
            {
                order.Skis =
                    EquipmentService.GetSkiLength(
                        (double)nudHeight.Value,
                        skill
                    );

                order.Boots =
                    EquipmentService.GetBootSize(
                        (int)nudShoeSize.Value
                    );

                order.Poles =
                    EquipmentService.GetPoleLength(
                        (double)nudHeight.Value
                    );
            }

            order.SkiPassDays =
                (int)nudDays.Value;

            order.HotelRoom =
                chkHotel.Checked;

            order.Breakfast =
                chkBreakfast.Checked;

            order.Sauna =
                chkSauna.Checked;

            order.TotalPrice =
                currentTotal;

            order.Status =
                "Заброньовано";

            OrderService.SaveOrder(order);

            MessageBox.Show(
                "Замовлення успішно заброньовано!\n\n" +
                $"Клієнт: {order.ClientName}\n" +
                $"Сума: {order.TotalPrice} грн\n" +
                "Статус: Заброньовано",
                "Бронювання успішне",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            Close();
        }

        private void OrderForm_Load(object sender, EventArgs e)
        {

        }
    }
}