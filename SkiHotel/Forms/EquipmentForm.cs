using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SkiHotel
{
    public partial class EquipmentForm : Form
    {
        private TabControl tabControlMain;
        private string dataFilePath;
        private Dictionary<string, TextBox> quantityInputs = new Dictionary<string, TextBox>();

        public EquipmentForm()
        {
            InitializeComponent();
            InitEquipmentSystem();
        }

        private void InitEquipmentSystem()
        {
            // Шлях до Data/equipment.txt
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            dataFilePath = Path.Combine(dir, "equipment.txt");

            // Якщо файлу немає, створюємо з мінімальним шаблоном, щоб структура не була пустою
            if (!File.Exists(dataFilePath))
            {
                File.WriteAllText(dataFilePath, "Skies and Poles | Beginner | Sample Ski Item | 10\n");
            }

            LoadAndBuildUI();
        }

        private void LoadAndBuildUI()
        
        {
            // Очищаємо попередні контроли, якщо відбулося оновлення
            foreach (Control c in this.Controls.Cast<Control>().ToList())
            {
                if (c is TabControl || (c is Panel && c.Dock == DockStyle.Bottom))
                    this.Controls.Remove(c);
            }
            quantityInputs.Clear();

            tabControlMain = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10, FontStyle.Regular) };

            // Читаємо дані ВИКЛЮЧНО з текстового файлу
            var lines = File.ReadAllLines(dataFilePath);
            var inventory = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');
                if (parts.Length >= 4)
                {
                    string cat = parts[0].Trim();
                    string sub = parts[1].Trim();
                    string name = parts[2].Trim();
                    if (int.TryParse(parts[3].Trim(), out int qty))
                    {
                        if (!inventory.ContainsKey(cat)) inventory[cat] = new Dictionary<string, Dictionary<string, int>>();
                        if (!inventory[cat].ContainsKey(sub)) inventory[cat][sub] = new Dictionary<string, int>();
                        inventory[cat][sub][name] = qty;
                    }
                }
            }

            // Динамічне створення вкладок на основі категорій з файлу
            foreach (var catPair in inventory)
            {
                string categoryName = catPair.Key;
                TabPage catTab = new TabPage(categoryName);

                TabControl subTabControl = new TabControl { Dock = DockStyle.Fill };
                foreach (var subPair in catPair.Value)
                {
                    TabPage subPage = new TabPage(subPair.Key);
                    subPage.Controls.Add(CreateItemsTable(categoryName, subPair.Key, subPair.Value));
                    subTabControl.TabPages.Add(subPage);
                }
                catTab.Controls.Add(subTabControl);
                tabControlMain.TabPages.Add(catTab);
            }

            this.Controls.Add(tabControlMain);

            // Нижня панель із кнопкою збереження
            Panel panelBottom = new Panel { Height = 55, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(240, 240, 240) };
            Button btnSave = new Button
            {
                Text = "Save Changes to File",
                Width = 200,
                Height = 35,
                Location = new Point(15, 10),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            panelBottom.Controls.Add(btnSave);
            this.Controls.Add(panelBottom);
        }

        private Control CreateItemsTable(string category, string subCategory, Dictionary<string, int> items)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            TableLayoutPanel table = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 3,
                Dock = DockStyle.Top,
                Padding = new Padding(15)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));

            table.Controls.Add(new Label { Text = "Item Name", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true }, 0, 0);
            table.Controls.Add(new Label { Text = "Quantity", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true }, 1, 0);
            table.Controls.Add(new Label { Text = "Quick Action", Font = new Font("Segoe UI", 9, FontStyle.Bold), AutoSize = true }, 2, 0);

            int row = 1;
            foreach (var item in items)
            {
                string uniqueKey = $"{category}|{subCategory}|{item.Key}";

                Label lblName = new Label { Text = item.Key, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) };
                TextBox txtQty = new TextBox { Text = item.Value.ToString(), Width = 70, TextAlign = HorizontalAlignment.Center, Font = new Font("Segoe UI", 10) };

                quantityInputs[uniqueKey] = txtQty;

                Button btnPlus = new Button { Text = "+1", Width = 45, Height = 26, Tag = txtQty, FlatStyle = FlatStyle.Flat };
                btnPlus.Click += (s, e) => {
                    var tb = (TextBox)((Button)s).Tag;
                    if (int.TryParse(tb.Text, out int v)) tb.Text = (v + 1).ToString();
                };

                table.Controls.Add(lblName, 0, row);
                table.Controls.Add(txtQty, 1, row);
                table.Controls.Add(btnPlus, 2, row);
                row++;
            }

            panel.Controls.Add(table);
            return panel;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var newLines = new List<string>();
            foreach (var kvp in quantityInputs)
            {
                string[] parts = kvp.Key.Split('|');
                if (parts.Length == 3)
                {
                    string cat = parts[0];
                    string sub = parts[1];
                    string name = parts[2];
                    string qty = kvp.Value.Text;
                    newLines.Add($"{cat} | {sub} | {name} | {qty}");
                }
            }
            File.WriteAllLines(dataFilePath, newLines);
            MessageBox.Show("Inventory successfully updated and saved to equipment.txt!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadAndBuildUI(); // Перезавантажуємо інтерфейс з оновленого файлу
        }

        private void EquipmentForm_Load(object sender, EventArgs e)
        {

        }
    }
}