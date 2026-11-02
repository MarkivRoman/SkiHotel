using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        public OrderForm()
        {
            InitializeComponent();
            CreateOrderForm(); // Викликаємо створення елементів тут
        }

        private void CreateOrderForm()
        {
            BackColor = Color.FromArgb(240, 245, 250);
            Font = new Font("Segoe UI", 10);

            // Top header panel
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

            Label title = new Label
            {
                Text = "New Order and Booking Creation",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 90),
                Location = new Point(20, 16),
                AutoSize = true
            };
            headerPanel.Controls.Add(title);
            Controls.Add(headerPanel);

            // Main container
            Panel mainPanel = new Panel
            {
                Location = new Point(15, 80),
                Size = new Size(800, 480),
                BackColor = Color.Transparent,
                AutoScroll = true
            };
            Controls.Add(mainPanel);

            // ================= 1. CLIENT DATA =================
            Panel clientPanel = CreateCardPanel(new Point(10, 10), new Size(380, 175), "Client Personal Data");

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

                string nameText = txtName != null ? txtName.Text : "";
                string initial = string.IsNullOrEmpty(nameText) ? "?" : nameText.Substring(0, 1).ToUpper();
                using (Font font = new Font("Segoe UI", 12, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, initial, font, new Rectangle(0, 0, 35, 35), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };
            clientPanel.Controls.Add(avatarBox);

            AddLabel(clientPanel, "Full Name:", new Point(15, 38));
            txtName = new TextBox { Location = new Point(125, 35), Width = 230, Font = new Font("Segoe UI", 10) };
            txtName.TextChanged += (s, e) => avatarBox.Invalidate();
            clientPanel.Controls.Add(txtName);

            AddLabel(clientPanel, "Phone:", new Point(15, 76));
            txtPhone = new TextBox { Location = new Point(125, 73), Width = 230, Font = new Font("Segoe UI", 10) };
            clientPanel.Controls.Add(txtPhone);

            AddLabel(clientPanel, "Email:", new Point(15, 114));
            txtEmail = new TextBox { Location = new Point(125, 111), Width = 230, Font = new Font("Segoe UI", 10) };
            clientPanel.Controls.Add(txtEmail);

            mainPanel.Controls.Add(clientPanel);

            // ================= 2. SKIPASS AND HOTEL =================
            Panel passPanel = CreateCardPanel(new Point(10, 195), new Size(380, 175), "Accommodation and Skipass");

            AddLabel(passPanel, "Number of Days:", new Point(15, 38));
            nudDays = new NumericUpDown { Location = new Point(165, 35), Width = 90, Minimum = 1, Maximum = 14, Value = 3, Font = new Font("Segoe UI", 10) };
            passPanel.Controls.Add(nudDays);

            chkHotel = new CheckBox { Text = "Hotel room", Location = new Point(15, 72), Width = 180, Font = new Font("Segoe UI", 10) };
            passPanel.Controls.Add(chkHotel);

            chkBreakfast = new CheckBox { Text = "Breakfast (+300 UAH/day)", Location = new Point(15, 103), Width = 210, Font = new Font("Segoe UI", 10) };
            passPanel.Controls.Add(chkBreakfast);

            chkSauna = new CheckBox { Text = "Sauna (+500 UAH)", Location = new Point(15, 134), Width = 180, Font = new Font("Segoe UI", 10) };
            passPanel.Controls.Add(chkSauna);

            mainPanel.Controls.Add(passPanel);

            // ================= 3. EQUIPMENT =================
            grpEquipment = CreateCardPanel(new Point(400, 10), new Size(390, 360), "Equipment Parameters");

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

            AddLabel(grpEquipment, "Weight (kg):", new Point(15, 72));
            nudWeight = new NumericUpDown { Location = new Point(170, 69), Width = 95, Minimum = 20, Maximum = 200, Value = 70, Font = new Font("Segoe UI", 10) };
            grpEquipment.Controls.Add(nudWeight);

            AddLabel(grpEquipment, "Height (cm):", new Point(15, 107));
            nudHeight = new NumericUpDown { Location = new Point(170, 104), Width = 95, Minimum = 100, Maximum = 230, Value = 175, Font = new Font("Segoe UI", 10) };
            grpEquipment.Controls.Add(nudHeight);

            AddLabel(grpEquipment, "Shoe Size:", new Point(15, 142));
            nudShoeSize = new NumericUpDown { Location = new Point(170, 139), Width = 95, Minimum = 30, Maximum = 50, Value = 42, Font = new Font("Segoe UI", 10) };
            grpEquipment.Controls.Add(nudShoeSize);

            AddLabel(grpEquipment, "Skill Level:", new Point(15, 177));
            cmbSkill = new ComboBox
            {
                Location = new Point(170, 174),
                Width = 175,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbSkill.Items.AddRange(new string[] { "Beginner", "Intermediate", "Professional", "Початківець", "Середній", "Професіонал" });
            cmbSkill.SelectedIndex = 0;
            grpEquipment.Controls.Add(cmbSkill);

            // Equipment result output block
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

            // ================= 4. BOTTOM PANEL: CALCULATION & BUTTONS =================
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

        private Panel CreateCardPanel(Point location, Size size, string titleText)
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

            Label lblTitle = new Label
            {
                Text = titleText,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 90),
                Location = new Point(15, 12),
                AutoSize = true
            };
            card.Controls.Add(lblTitle);

            return card;
        }

        private void AddLabel(Control parent, string text, Point location)
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

            if (!enabled)
            {
                lblSkis.Text = "Skis: Own";
                lblBoots.Text = "Boots: Own";
                lblPoles.Text = "Poles: Own";
            }
            else
            {
                lblSkis.Text = "Skis: not selected";
                lblBoots.Text = "Boots: not selected";
                lblPoles.Text = "Poles: not selected";
            }
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter client's full name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text) && string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please enter client's phone or email.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentTotal = 0;
            int days = (int)nudDays.Value;

            currentTotal += days * 1000;

            if (!chkOwnEquipment.Checked)
            {
                string skiLength = EquipmentService.GetSkiLength((double)nudHeight.Value, cmbSkill.SelectedItem?.ToString() ?? "Beginner");
                string poleLength = EquipmentService.GetPoleLength((double)nudHeight.Value);
                string bootSize = EquipmentService.GetBootSize((int)nudShoeSize.Value);

                lblSkis.Text = $"Skis: {skiLength}";
                lblBoots.Text = $"Boots: {bootSize}";
                lblPoles.Text = $"Poles: {poleLength}";

                currentTotal += 800;
            }
            else
            {
                lblSkis.Text = "Skis: Own";
                lblBoots.Text = "Boots: Own";
                lblPoles.Text = "Poles: Own";
            }

            if (chkHotel.Checked) currentTotal += days * 2500;
            if (chkBreakfast.Checked) currentTotal += days * 300;
            if (chkSauna.Checked) currentTotal += 500;

            lblTotal.Text = $"Total cost: {currentTotal} UAH";
            btnBook.Enabled = true;
        }

        private void BtnBook_Click(object sender, EventArgs e)
        {
            if (currentTotal <= 0)
            {
                MessageBox.Show("Calculate the cost first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string skill = cmbSkill.SelectedItem?.ToString() ?? "Beginner";

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

            MessageBox.Show(
                "Booking successfully submitted and added to the system!\n\n" +
                $"Client: {order.ClientName}\n" +
                $"Amount: {order.TotalPrice} UAH\n" +
                "Status: Incoming (waiting for admin confirmation)",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            Close();
        }

        private void OrderForm_Load(object sender, EventArgs e) { }
    }
}