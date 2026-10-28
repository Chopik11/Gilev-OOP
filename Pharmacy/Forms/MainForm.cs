using System;
using System.Drawing;
using System.Windows.Forms;
using Pharmacy.DB;

namespace Pharmacy.Forms
{
    public class MainForm : Form
    {
        private Panel _sidebar;
        private Panel _contentPanel;
        private Button _btnMedicines;
        private Button _btnSales;
        private Label _lblTitle;
        private Panel _welcomePanel;

        public MainForm()
        {
            InitializeComponent();
            ShowWelcome();
        }

        private void InitializeComponent()
        {
            this.Text = "Аптека — учёт медикаментов";
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.Font = new Font("Segoe UI", 9f);

            // ── Sidebar ──────────────────────────────────────────
            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(30, 55, 90)
            };

            var logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(20, 40, 70)
            };
            var lblLogo = new Label
            {
                Text = "💊  Аптека",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            logoPanel.Controls.Add(lblLogo);

            _btnMedicines = CreateSidebarButton("💊  Лекарства", 0);
            _btnMedicines.Click += (s, e) => OpenMedicines();

            _btnSales = CreateSidebarButton("🛒  Продажи", 1);
            _btnSales.Click += (s, e) => OpenSales();

            var btnSeparator = new Panel
            {
                Height = 1,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 90, 130),
                Margin = new Padding(10, 0, 10, 0)
            };

            var lblVersion = new Label
            {
                Text = "v1.0.0",
                ForeColor = Color.FromArgb(120, 150, 185),
                Font = new Font("Segoe UI", 8f),
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _sidebar.Controls.Add(lblVersion);
            _sidebar.Controls.Add(_btnSales);
            _sidebar.Controls.Add(_btnMedicines);
            _sidebar.Controls.Add(btnSeparator);
            _sidebar.Controls.Add(logoPanel);

            // ── Header ───────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White
            };
            header.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 225, 235), 1),
                    0, header.Height - 1, header.Width, header.Height - 1);
            };

            _lblTitle = new Label
            {
                Text = "Главная",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 55, 90),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };
            header.Controls.Add(_lblTitle);

            // ── Content Panel ────────────────────────────────────
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            var mainArea = new Panel { Dock = DockStyle.Fill };
            mainArea.Controls.Add(_contentPanel);
            mainArea.Controls.Add(header);

            this.Controls.Add(mainArea);
            this.Controls.Add(_sidebar);
        }

        private Button CreateSidebarButton(string text, int index)
        {
            return new Button
            {
                Text = text,
                ForeColor = Color.FromArgb(190, 210, 235),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Dock = DockStyle.Top,
                Height = 46,
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private void SetActiveButton(Button active)
        {
            foreach (Control c in _sidebar.Controls)
            {
                if (c is Button btn)
                {
                    btn.BackColor = Color.Transparent;
                    btn.ForeColor = Color.FromArgb(190, 210, 235);
                }
            }
            active.BackColor = Color.FromArgb(50, 85, 130);
            active.ForeColor = Color.White;
        }

        private void ShowWelcome()
        {
            _lblTitle.Text = "Добро пожаловать";
            _contentPanel.Controls.Clear();

            _welcomePanel = new Panel { Dock = DockStyle.Fill };

            var lblWelcome = new Label
            {
                Text = "Система учёта медикаментов",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 55, 90),
                AutoSize = true,
                Location = new Point(40, 60)
            };

            var lblSub = new Label
            {
                Text = "Выберите раздел в меню слева для начала работы",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.FromArgb(100, 120, 150),
                AutoSize = true,
                Location = new Point(40, 110)
            };

            // Quick stats cards
            int expiring = GetExpiringCount();
            int total    = GetMedicineCount();

            var card1 = CreateStatCard("📦 Всего лекарств",  total.ToString(),    Color.FromArgb(66, 133, 244), new Point(40, 170));
            var card2 = CreateStatCard("⚠️ Истекает срок",   expiring.ToString(), Color.FromArgb(234, 67, 53),  new Point(260, 170));

            _welcomePanel.Controls.Add(lblWelcome);
            _welcomePanel.Controls.Add(lblSub);
            _welcomePanel.Controls.Add(card1);
            _welcomePanel.Controls.Add(card2);

            _contentPanel.Controls.Add(_welcomePanel);
        }

        private Panel CreateStatCard(string title, string value, Color accent, Point location)
        {
            var card = new Panel
            {
                Size = new Size(190, 100),
                Location = location,
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(accent), 0, 0, 5, card.Height);
                ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle,
                    Color.FromArgb(220, 225, 235), ButtonBorderStyle.Solid);
            };

            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(18, 12)
            };
            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(100, 120, 150),
                AutoSize = true,
                Location = new Point(18, 60)
            };
            card.Controls.Add(lblVal);
            card.Controls.Add(lblTitle);
            return card;
        }

        private int GetMedicineCount()
        {
            try
            {
                var result = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Medicines");
                return Convert.ToInt32(result);
            }
            catch { return 0; }
        }

        private int GetExpiringCount()
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM Medicines WHERE expiry_date <= @d";
                var p = new[] { new System.Data.SQLite.SQLiteParameter("@d", DateTime.Today.AddDays(30).ToString("yyyy-MM-dd")) };
                var result = DatabaseHelper.ExecuteScalar(sql, p);
                return Convert.ToInt32(result);
            }
            catch { return 0; }
        }

        private void OpenMedicines()
        {
            SetActiveButton(_btnMedicines);
            _lblTitle.Text = "Лекарства";
            _contentPanel.Controls.Clear();

            var form = new MedicinesControl();
            form.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(form);
        }

        private void OpenSales()
        {
            SetActiveButton(_btnSales);
            _lblTitle.Text = "Продажи";
            _contentPanel.Controls.Clear();

            var form = new SalesControl();
            form.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(form);
        }
    }
}
