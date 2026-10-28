using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Pharmacy.DB;

namespace Pharmacy.Forms
{
    public class MedicinesControl : UserControl
    {
        private DataGridView _grid;
        private TextBox _txtSearch;
        private TextBox _txtName;
        private ComboBox _cmbCategory;
        private TextBox _txtPrice;
        private TextBox _txtStock;
        private DateTimePicker _dtExpiry;
        private Button _btnAdd;
        private Button _btnEdit;
        private Button _btnDelete;
        private Label _lblStatus;
        private int _selectedId = -1;

        public MedicinesControl()
        {
            InitializeComponent();
            LoadCategories();
            LoadMedicines();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(245, 247, 250);

            // ── Left: Grid + Search ──────────────────────────────
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 8, 0)
            };

            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.Transparent
            };
            var lblSearch = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI", 11f),
                AutoSize = false,
                Width = 30,
                Height = 32,
                Location = new Point(0, 5),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _txtSearch = new TextBox
            {
                Location = new Point(30, 8),
                Width = 340,
                Height = 28,
                Font = new Font("Segoe UI", 10f),
                BorderStyle = BorderStyle.FixedSingle
            };
            _txtSearch.TextChanged += (s, e) => LoadMedicines(_txtSearch.Text);
            searchPanel.Controls.Add(_txtSearch);
            searchPanel.Controls.Add(lblSearch);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 36,
                RowTemplate = { Height = 30 },
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(230, 235, 242),
                Font = new Font("Segoe UI", 9f)
            };
            _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 55, 90),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0)
            };
            _grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                Padding = new Padding(4, 0, 0, 0),
                SelectionBackColor = Color.FromArgb(200, 220, 245),
                SelectionForeColor = Color.FromArgb(20, 40, 80)
            };
            _grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 250, 255),
                SelectionBackColor = Color.FromArgb(200, 220, 245),
                SelectionForeColor = Color.FromArgb(20, 40, 80)
            };
            _grid.SelectionChanged += OnGridSelectionChanged;

            var gridCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(1)
            };
            gridCard.Paint += (s, e) => DrawCardBorder(e, gridCard);
            gridCard.Controls.Add(_grid);

            leftPanel.Controls.Add(gridCard);
            leftPanel.Controls.Add(searchPanel);

            // ── Right: Form ──────────────────────────────────────
            var rightPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 270,
                BackColor = Color.White,
                Padding = new Padding(16)
            };
            rightPanel.Paint += (s, e) => DrawCardBorder(e, rightPanel);

            int y = 16;
            int labelH = 18;
            int inputH = 28;
            int gap = 10;
            int sectionGap = 18;

            var lblForm = new Label
            {
                Text = "Данные лекарства",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 55, 90),
                Location = new Point(16, y),
                AutoSize = true
            };
            y += 30;

            var sep = new Panel
            {
                Location = new Point(16, y),
                Size = new Size(238, 1),
                BackColor = Color.FromArgb(220, 225, 235)
            };
            y += 12;

            _txtName = CreateInput(rightPanel, "Название", ref y, labelH, inputH, gap);
            _cmbCategory = new ComboBox
            {
                Location = new Point(16, y + labelH + 2),
                Width = 238,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f)
            };
            AddLabeledControl(rightPanel, "Категория", _cmbCategory, ref y, labelH, inputH, gap);
            _txtPrice = CreateInput(rightPanel, "Цена (₽)", ref y, labelH, inputH, gap);
            _txtStock = CreateInput(rightPanel, "Количество на складе", ref y, labelH, inputH, gap);

            var lblDate = new Label
            {
                Text = "Срок годности",
                Location = new Point(16, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 120, 150)
            };
            y += labelH + 2;
            _dtExpiry = new DateTimePicker
            {
                Location = new Point(16, y),
                Width = 238,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5f)
            };
            y += inputH + gap + sectionGap;
            rightPanel.Controls.Add(lblDate);
            rightPanel.Controls.Add(_dtExpiry);

            _btnAdd = CreateButton("➕  Добавить", Color.FromArgb(40, 167, 69), rightPanel, ref y);
            _btnAdd.Click += BtnAdd_Click;
            y += 6;
            _btnEdit = CreateButton("✏️  Сохранить изменения", Color.FromArgb(0, 123, 255), rightPanel, ref y);
            _btnEdit.Click += BtnEdit_Click;
            y += 6;
            _btnDelete = CreateButton("🗑️  Удалить", Color.FromArgb(220, 53, 69), rightPanel, ref y);
            _btnDelete.Click += BtnDelete_Click;

            _lblStatus = new Label
            {
                Location = new Point(16, y + 12),
                Size = new Size(238, 40),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 150, 100),
                TextAlign = ContentAlignment.TopLeft
            };
            rightPanel.Controls.Add(_lblStatus);

            rightPanel.Controls.Add(lblForm);
            rightPanel.Controls.Add(sep);

            // ── Assemble ─────────────────────────────────────────
            this.Controls.Add(leftPanel);
            this.Controls.Add(rightPanel);
        }

        private TextBox CreateInput(Panel parent, string label, ref int y, int labelH, int inputH, int gap)
        {
            var lbl = new Label
            {
                Text = label,
                Location = new Point(16, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 120, 150)
            };
            y += labelH + 2;
            var txt = new TextBox
            {
                Location = new Point(16, y),
                Width = 238,
                Height = inputH,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };
            y += inputH + gap + 4;
            parent.Controls.Add(lbl);
            parent.Controls.Add(txt);
            return txt;
        }

        private void AddLabeledControl(Panel parent, string label, Control ctrl, ref int y, int labelH, int inputH, int gap)
        {
            var lbl = new Label
            {
                Text = label,
                Location = new Point(16, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 120, 150)
            };
            ctrl.Location = new Point(16, y + labelH + 2);
            y += labelH + 2 + inputH + gap + 4;
            parent.Controls.Add(lbl);
            parent.Controls.Add(ctrl);
        }

        private Button CreateButton(string text, Color color, Panel parent, ref int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(16, y),
                Size = new Size(238, 34),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            y += 40;
            parent.Controls.Add(btn);
            return btn;
        }

        private void DrawCardBorder(PaintEventArgs e, Control c)
        {
            ControlPaint.DrawBorder(e.Graphics, c.ClientRectangle,
                Color.FromArgb(220, 225, 235), ButtonBorderStyle.Solid);
        }

        // ── Data ──────────────────────────────────────────────────

        private void LoadCategories()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery("SELECT id, name FROM Categories ORDER BY name");
                _cmbCategory.DisplayMember = "name";
                _cmbCategory.ValueMember = "id";
                _cmbCategory.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMedicines(string search = "")
        {
            try
            {
                string sql = @"
                    SELECT m.id,
                           m.name        AS 'Название',
                           c.name        AS 'Категория',
                           m.price       AS 'Цена (₽)',
                           m.stock       AS 'Остаток',
                           m.expiry_date AS 'Срок годности'
                    FROM   Medicines m
                    LEFT JOIN Categories c ON c.id = m.category_id
                    WHERE  m.name LIKE @s
                    ORDER  BY m.name";

                var p = new[] { new SQLiteParameter("@s", $"%{search}%") };
                var dt = DatabaseHelper.ExecuteQuery(sql, p);

                _grid.DataSource = dt;
                if (_grid.Columns.Contains("id"))
                    _grid.Columns["id"].Visible = false;

                ColorExpiredRows();
                _lblStatus.Text = $"Записей: {dt.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ColorExpiredRows()
        {
            if (!_grid.Columns.Contains("Срок годности")) return;
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                if (DateTime.TryParse(row.Cells["Срок годности"].Value?.ToString(), out DateTime exp))
                {
                    if (exp < DateTime.Today)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(180, 0, 0);
                    }
                    else if (exp <= DateTime.Today.AddDays(7))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 200);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 100, 0);
                    }
                }
            }
        }

        private void OnGridSelectionChanged(object sender, EventArgs e)
        {
            if (_grid.SelectedRows.Count == 0) { _selectedId = -1; return; }
            var row = _grid.SelectedRows[0];
            _selectedId = Convert.ToInt32(row.Cells["id"].Value);
            _txtName.Text = row.Cells["Название"].Value?.ToString();
            _txtPrice.Text = row.Cells["Цена (₽)"].Value?.ToString();
            _txtStock.Text = row.Cells["Остаток"].Value?.ToString();
            if (DateTime.TryParse(row.Cells["Срок годности"].Value?.ToString(), out DateTime d))
                _dtExpiry.Value = d;

            // Match category
            string catName = row.Cells["Категория"].Value?.ToString();

            DataTable table = (DataTable)_cmbCategory.DataSource;

            foreach (DataRow rowCat in table.Rows)
            {
                if (rowCat["name"].ToString() == catName)
                {
                    _cmbCategory.SelectedValue = rowCat["id"];
                    break;
                }
            }
        }

        private bool ValidateInput(out string name, out int catId, out decimal price, out int stock)
        {
            name = _txtName.Text.Trim();
            catId = -1; price = 0; stock = 0;

            if (string.IsNullOrEmpty(name))
            { SetStatus("⚠ Введите название лекарства.", false); return false; }

            if (_cmbCategory.SelectedValue == null)
            { SetStatus("⚠ Выберите категорию.", false); return false; }
            catId = Convert.ToInt32(_cmbCategory.SelectedValue);

            if (!decimal.TryParse(_txtPrice.Text.Replace(',', '.'), out price) || price < 0)
            { SetStatus("⚠ Некорректная цена.", false); return false; }

            if (!int.TryParse(_txtStock.Text, out stock) || stock < 0)
            { SetStatus("⚠ Некорректное количество.", false); return false; }

            return true;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out string name, out int catId, out decimal price, out int stock)) return;
            try
            {
                string sql = @"INSERT INTO Medicines (name, category_id, price, stock, expiry_date)
                               VALUES (@n, @c, @p, @s, @d)";
                var parms = new[]
                {
                    new SQLiteParameter("@n", name),
                    new SQLiteParameter("@c", catId),
                    new SQLiteParameter("@p", price),
                    new SQLiteParameter("@s", stock),
                    new SQLiteParameter("@d", _dtExpiry.Value.ToString("yyyy-MM-dd"))
                };
                DatabaseHelper.ExecuteNonQuery(sql, parms);
                LoadMedicines(_txtSearch.Text);
                ClearForm();
                SetStatus("✔ Лекарство добавлено.", true);
            }
            catch (Exception ex) { SetStatus("Ошибка: " + ex.Message, false); }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedId < 0) { SetStatus("⚠ Выберите запись в таблице.", false); return; }
            if (!ValidateInput(out string name, out int catId, out decimal price, out int stock)) return;
            try
            {
                string sql = @"UPDATE Medicines SET name=@n, category_id=@c, price=@p, stock=@s, expiry_date=@d
                               WHERE id=@id";
                var parms = new[]
                {
                    new SQLiteParameter("@n",  name),
                    new SQLiteParameter("@c",  catId),
                    new SQLiteParameter("@p",  price),
                    new SQLiteParameter("@s",  stock),
                    new SQLiteParameter("@d",  _dtExpiry.Value.ToString("yyyy-MM-dd")),
                    new SQLiteParameter("@id", _selectedId)
                };
                DatabaseHelper.ExecuteNonQuery(sql, parms);
                LoadMedicines(_txtSearch.Text);
                SetStatus("✔ Данные обновлены.", true);
            }
            catch (Exception ex) { SetStatus("Ошибка: " + ex.Message, false); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedId < 0) { SetStatus("⚠ Выберите запись.", false); return; }
            if (MessageBox.Show("Удалить выбранное лекарство?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Medicines WHERE id=@id",
                    new[] { new SQLiteParameter("@id", _selectedId) });
                _selectedId = -1;
                LoadMedicines(_txtSearch.Text);
                ClearForm();
                SetStatus("✔ Запись удалена.", true);
            }
            catch (Exception ex) { SetStatus("Ошибка: " + ex.Message, false); }
        }

        private void ClearForm()
        {
            _txtName.Clear(); _txtPrice.Clear(); _txtStock.Clear();
            _dtExpiry.Value = DateTime.Today;
            if (_cmbCategory.Items.Count > 0) _cmbCategory.SelectedIndex = 0;
            _selectedId = -1;
        }

        private void SetStatus(string msg, bool ok)
        {
            _lblStatus.Text = msg;
            _lblStatus.ForeColor = ok ? Color.FromArgb(40, 140, 60) : Color.FromArgb(190, 50, 50);
        }
    }
}
