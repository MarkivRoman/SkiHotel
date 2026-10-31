using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using SkiHotel.Services;
using SkiHotel.Models;

namespace SkiHotel.Forms
{
    public partial class ReviewsForm : Form
    {
        private FlowLayoutPanel panelReviewsContainer = new FlowLayoutPanel();
        private Label lblAverage = new Label();

        // Кнопки фільтрації
        private Button btnFilterAll = new Button();
        private Button btnFilterGood = new Button();
        private Button btnFilterBad = new Button();

        private List<Review> allReviews = new List<Review>();
        private string currentFilter = "ALL"; // "ALL", "GOOD", "BAD"

        public ReviewsForm()
        {
            // Налаштування вікна
            Text = "Відгуки клієнтів — SKI HOTEL";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(950, 600);
            BackColor = Color.FromArgb(240, 245, 250);
            Font = new Font("Segoe UI", 10);

            // Верхня панель (статистика та фільтри)
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White,
                Padding = new Padding(25, 0, 25, 0)
            };

            Label title = new Label
            {
                Text = "Відгуки клієнтів",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 90),
                Location = new Point(25, 12),
                AutoSize = true
            };

            // Блок розрахунку середньої оцінки
            lblAverage.Location = new Point(25, 55);
            lblAverage.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblAverage.ForeColor = Color.FromArgb(30, 100, 70);
            lblAverage.AutoSize = true;

            // Кнопки фільтрів справа
            btnFilterAll = CreateFilterButton("Всі", 350, true);
            btnFilterGood = CreateFilterButton("🟢 Добрі (4-5)", 500, false);
            btnFilterBad = CreateFilterButton("🔴 Погані (1-2)", 650, false);

            btnFilterAll.Click += (s, e) => SetFilter("ALL", btnFilterAll);
            btnFilterGood.Click += (s, e) => SetFilter("GOOD", btnFilterGood);
            btnFilterBad.Click += (s, e) => SetFilter("BAD", btnFilterBad);

            topPanel.Controls.Add(title);
            topPanel.Controls.Add(lblAverage);
            topPanel.Controls.Add(btnFilterAll);
            topPanel.Controls.Add(btnFilterGood);
            topPanel.Controls.Add(btnFilterBad);

            // Контейнер для карток відгуків
            panelReviewsContainer.Dock = DockStyle.Fill;
            panelReviewsContainer.AutoScroll = true;
            panelReviewsContainer.BackColor = Color.FromArgb(240, 245, 250);
            panelReviewsContainer.Padding = new Padding(20);

            Controls.Add(panelReviewsContainer);
            Controls.Add(topPanel);

            LoadReviews();
        }

        private Button CreateFilterButton(string text, int xPos, bool active)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(xPos, 35),
                Size = new Size(135, 36),
                Font = new Font("Segoe UI", 10, active ? FontStyle.Bold : FontStyle.Regular),
                BackColor = active ? Color.FromArgb(30, 60, 90) : Color.FromArgb(235, 242, 248),
                ForeColor = active ? Color.White : Color.FromArgb(30, 60, 90),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void SetFilter(string filterType, Button clickedButton)
        {
            currentFilter = filterType;
            UpdateFilterButtonStyles(clickedButton);
            DisplayFilteredReviews();
        }

        private void UpdateFilterButtonStyles(Button activeBtn)
        {
            Button[] buttons = { btnFilterAll, btnFilterGood, btnFilterBad };
            foreach (var b in buttons)
            {
                if (b == activeBtn)
                {
                    b.BackColor = Color.FromArgb(30, 60, 90);
                    b.ForeColor = Color.White;
                    b.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                else
                {
                    b.BackColor = Color.FromArgb(235, 242, 248);
                    b.ForeColor = Color.FromArgb(30, 60, 90);
                    b.Font = new Font("Segoe UI", 10, FontStyle.Regular);
                }
            }
        }

        private void LoadReviews()
        {
            allReviews = ReviewService.GetAllReviews();

            if (allReviews == null || allReviews.Count == 0)
            {
                lblAverage.Text = "Середня оцінка: немає даних";
            }
            else
            {
                double avgScore = allReviews.Average(r => r.Rating);
                double percentage = (avgScore / 5.0) * 100.0;
                lblAverage.Text = $"Загальна середня оцінка: {avgScore:0.1} / 5  ({percentage:0.0}%)";
            }

            DisplayFilteredReviews();
        }

        private void DisplayFilteredReviews()
        {
            panelReviewsContainer.Controls.Clear();

            var filtered = allReviews;
            if (currentFilter == "GOOD")
            {
                filtered = allReviews.Where(r => r.Rating >= 4).ToList();
            }
            else if (currentFilter == "BAD")
            {
                filtered = allReviews.Where(r => r.Rating <= 2).ToList();
            }

            if (filtered == null || filtered.Count == 0)
            {
                Label lblEmpty = new Label
                {
                    Text = "За цим фільтром немає відгуків.",
                    Font = new Font("Segoe UI", 13, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(30, 30)
                };
                panelReviewsContainer.Controls.Add(lblEmpty);
                return;
            }

            foreach (var review in filtered)
            {
                panelReviewsContainer.Controls.Add(CreateReviewCard(review));
            }
        }

        private Panel CreateReviewCard(Review review)
        {
            Color cardBg = Color.White;
            Color borderColor = Color.FromArgb(210, 220, 230);
            Color avatarBg = Color.FromArgb(50, 100, 150);

            if (review.Rating >= 4)
            {
                cardBg = Color.FromArgb(235, 250, 238);
                borderColor = Color.FromArgb(160, 220, 175);
                avatarBg = Color.FromArgb(40, 140, 60);
            }
            else if (review.Rating == 3)
            {
                cardBg = Color.FromArgb(255, 253, 230);
                borderColor = Color.FromArgb(230, 220, 140);
                avatarBg = Color.FromArgb(180, 140, 0);
            }
            else
            {
                cardBg = Color.FromArgb(255, 235, 235);
                borderColor = Color.FromArgb(240, 170, 170);
                avatarBg = Color.FromArgb(200, 50, 50);
            }

            Panel card = new Panel
            {
                Width = 860,
                Height = 110,
                BackColor = cardBg,
                Margin = new Padding(0, 0, 0, 15)
            };

            card.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(borderColor, 1.5f))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            // Кругла іконка-аватарка з першою літерою імені клієнта
            PictureBox avatarBox = new PictureBox
            {
                Size = new Size(45, 45),
                Location = new Point(20, 18),
                BackColor = Color.Transparent
            };

            avatarBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush brush = new SolidBrush(avatarBg))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, 44, 44);
                }

                string initial = string.IsNullOrEmpty(review.ClientName) ? "?" : review.ClientName.Substring(0, 1).ToUpper();
                using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, initial, font, new Rectangle(0, 0, 45, 45), Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };

            // Нік клієнта
            Label lblName = new Label
            {
                Text = review.ClientName,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 50, 70),
                Location = new Point(78, 16),
                AutoSize = true
            };

            // Зірочки оцінки
            string stars = new string('★', review.Rating) + new string('☆', 5 - review.Rating);
            Label lblStars = new Label
            {
                Text = $"{stars} ({review.Rating}/5)",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = review.Rating >= 4 ? Color.FromArgb(40, 140, 60) : (review.Rating == 3 ? Color.FromArgb(160, 120, 0) : Color.FromArgb(190, 40, 40)),
                Location = new Point(260, 17),
                AutoSize = true
            };

            // Дата маленьким шрифтом у куточку
            Label lblDate = new Label
            {
                Text = review.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.Gray,
                Location = new Point(700, 18),
                AutoSize = true
            };

            // Текст коментаря
            Label lblComment = new Label
            {
                Text = string.IsNullOrWhiteSpace(review.Comment) ? "Без коментаря" : $"\"{review.Comment}\"",
                Font = new Font("Segoe UI", 11, FontStyle.Italic),
                ForeColor = Color.FromArgb(50, 60, 70),
                Location = new Point(78, 56),
                Size = new Size(760, 40)
            };

            card.Controls.Add(avatarBox);
            card.Controls.Add(lblName);
            card.Controls.Add(lblStars);
            card.Controls.Add(lblDate);
            card.Controls.Add(lblComment);

            return card;
        }

        private void ReviewsForm_Load(object sender, EventArgs e)
        {

        }
    }
}