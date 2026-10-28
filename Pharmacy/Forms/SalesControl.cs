using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Pharmacy.DB;

namespace Pharmacy.Forms
{
    public class SalesControl : UserControl
    {
        // ── Cart item ────────────────────────────────────────────
        private class CartItem
        {
            public int    MedicineId { get; set; }
            public string Name       { get; set; }
            public int    Quantity   { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total => Quantity * UnitPrice;
        }

        private readonly List<CartItem> _cart = new List<CartItem>();

        private ComboBox         _cmbMedicine;
        private NumericUpDown    _numQty;
        private DataGridView     _cartGrid;
        private DataGridView     _historyGrid;
        private Label            _lblTotal;
        private Button           _btnAddToCart;
        private Button           _btnRemove;
        private Button           _btnCheckout;

        public SalesControl()
        {
            InitializeComponent();
            LoadMedicinesCombo();
            LoadSalesHistory();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(245, 247, 250);

            // ── Left: Cart ───────────────────────────────────────
            var cartPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 8, 0)
            };

            // Product selection row
            var selPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = Color.White
            };
            selPanel.Paint += (s, e) => DrawBorder(e, selPanel);

            var lblMed = new Label
            {
                Text = "Лекарство:",
                Location = new Point(10, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };
            _cmbMedicine = new ComboBox
            {
                Location = new Point(90, 10),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5f)
            };
            var lblQty = new Label
            {
                Text = "Кол-во:",
                Location = new Point(382, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };
            _numQty = new NumericUpDown
            {
                Location = new Point(432, 10),
                Width = 65,
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Font = new Font("Segoe UI", 9.5f)
            };
            _btnAddToCart = new Button
            {
                Text = "➕ В корзину",
                Location = new Point(508, 8),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnAddToCart.FlatAppearance.BorderSize = 0;
            _btnAddToCart.Click += BtnAddToCart_Click;

            selPanel.Controls.AddRange(new Control[] { lblMed, _cmbMedicine, lblQty, _numQty, _btnAddToCart });

            // Cart grid
            _cartGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 28 },
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(230, 235, 242),
                Font = new Font("Segoe UI", 9f)
            };
            _cartGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 55, 90),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(6, 0, 0, 0)
            };
            _cartGrid.DefaultCellStyle = new DataGridViewCellStyle
            {
                Padding = new Padding(4, 0, 0, 0),
                SelectionBackColor = Color.FromArgb(200, 220, 245),
                SelectionForeColor = Color.FromArgb(20, 40, 80)
            };

            var cartGridWrap = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            cartGridWrap.Paint += (s, e) => DrawBorder(e, cartGridWrap);
            cartGridWrap.Controls.Add(_cartGrid);

            // Bottom bar
            var bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Color.White
            };
            bottomBar.Paint += (s, e) => DrawBorder(e, bottomBar);

            _lblTotal = new Label
            {
                Text = "Итого: 0,00 ₽",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 55, 90),
                Location = new Point(14, 10),
                AutoSize = true
            };
            _btnRemove = new Button
            {
                Text = "🗑 Удалить позицию",
                Size = new Size(160, 34),
                Location = new Point(0, 9),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            _btnRemove.FlatAppearance.BorderSize = 0;
            _btnRemove.Click += BtnRemove_Click;

            _btnCheckout = new Button
            {
                Text = "✔ Оформить продажу",
                Size = new Size(180, 34),
                Location = new Point(0, 9),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            _btnCheckout.FlatAppearance.BorderSize = 0;
            _btnCheckout.Click += BtnCheckout_Click;

            // Position buttons from right
            bottomBar.Resize += (s, e) =>
            {
                _btnCheckout.Left = bottomBar.Width - _btnCheckout.Width - 10;
                _btnRemove.Left  = _btnCheckout.Left - _btnRemove.Width - 8;
            };
            bottomBar.Controls.AddRange(new Control[] { _lblTotal, _btnRemove, _btnCheckout });

            cartPanel.Controls.Add(cartGridWrap);
            cartPanel.Controls.Add(bottomBar);
            cartPanel.Controls.Add(selPanel);

            // ── Right: History ───────────────────────────────────
            var historyPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 320,
                BackColor = Color.White
            };
            historyPanel.Paint += (s, e) => DrawBorder(e, historyPanel);

            var lblHist = new Label
            {
                Text = "История продаж",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 55, 90),
                Dock = DockStyle.Top,
                Height = 38,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            _historyGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 32,
                RowTemplate = { Height = 26 },
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(230, 235, 242),
                Font = new Font("Segoe UI", 9f)
            };
            _historyGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(60, 90, 130),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Padding = new Padding(4, 0, 0, 0)
            };
            _historyGrid.DefaultCellStyle = new DataGridViewCellStyle
            {
                Padding = new Padding(4, 0, 0, 0),
                SelectionBackColor = Color.FromArgb(200, 220, 245),
                SelectionForeColor = Color.FromArgb(20, 40, 80)
            };

            historyPanel.Controls.Add(_historyGrid);
            historyPanel.Controls.Add(lblHist);

            this.Controls.Add(cartPanel);
            this.Controls.Add(historyPanel);

            SetupCartColumns();
        }

        private void DrawBorder(PaintEventArgs e, Control c)
        {
            ControlPaint.DrawBorder(e.Graphics, c.ClientRectangle,
                Color.FromArgb(220, 225, 235), ButtonBorderStyle.Solid);
        }

        private void SetupCartColumns()
        {
            _cartGrid.Columns.Clear();
            _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name",      HeaderText = "Лекарство",   FillWeight = 50 });
            _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qty",       HeaderText = "Кол-во",      FillWeight = 15 });
            _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "Цена",        FillWeight = 17 });
            _cartGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total",     HeaderText = "Сумма",       FillWeight = 18 });
        }

        private void LoadMedicinesCombo()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery("SELECT id, name || ' (' || stock || ' шт.)' AS label FROM Medicines WHERE stock > 0 ORDER BY name");
                _cmbMedicine.DisplayMember = "label";
                _cmbMedicine.ValueMember = "id";
                _cmbMedicine.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки лекарств: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSalesHistory()
        {
            try
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT id AS '№', substr(date,1,10) AS 'Дата', printf('%.2f ₽', total) AS 'Сумма' FROM Sales ORDER BY id DESC LIMIT 50");
                _historyGrid.DataSource = dt;
            }
            catch { }
        }

        private void RefreshCartGrid()
        {
            _cartGrid.Rows.Clear();
            decimal total = 0;
            foreach (var item in _cart)
            {
                _cartGrid.Rows.Add(item.Name, item.Quantity,
                    item.UnitPrice.ToString("F2") + " ₽",
                    item.Total.ToString("F2") + " ₽");
                total += item.Total;
            }
            _lblTotal.Text = $"Итого: {total:F2} ₽";
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (_cmbMedicine.SelectedValue == null) return;
            int medId = Convert.ToInt32(_cmbMedicine.SelectedValue);
            int qty = (int)_numQty.Value;

            // Get medicine details
            var dt = DatabaseHelper.ExecuteQuery(
                "SELECT name, price, stock FROM Medicines WHERE id=@id",
                new[] { new SQLiteParameter("@id", medId) });

            if (dt.Rows.Count == 0) return;

            string name  = dt.Rows[0]["name"].ToString();
            decimal price = Convert.ToDecimal(dt.Rows[0]["price"]);
            int stock = Convert.ToInt32(dt.Rows[0]["stock"]);

            // Check existing in cart
            int alreadyInCart = 0;
            CartItem existing = null;
            foreach (var ci in _cart)
                if (ci.MedicineId == medId) { alreadyInCart = ci.Quantity; existing = ci; break; }

            if (alreadyInCart + qty > stock)
            {
                MessageBox.Show($"Недостаточно на складе. Доступно: {stock - alreadyInCart} шт.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (existing != null)
                existing.Quantity += qty;
            else
                _cart.Add(new CartItem { MedicineId = medId, Name = name, Quantity = qty, UnitPrice = price });

            RefreshCartGrid();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (_cartGrid.SelectedRows.Count == 0) return;
            int idx = _cartGrid.SelectedRows[0].Index;
            if (idx >= 0 && idx < _cart.Count)
            {
                _cart.RemoveAt(idx);
                RefreshCartGrid();
            }
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста. Добавьте товары.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal total = 0;
            foreach (var item in _cart) total += item.Total;

            if (MessageBox.Show($"Оформить продажу на сумму {total:F2} ₽?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                using (var tx = conn.BeginTransaction())
                {
                    // Insert Sale
                    var cmdSale = new SQLiteCommand(
                        "INSERT INTO Sales (date, total) VALUES (@d, @t); SELECT last_insert_rowid();", conn, tx);
                    cmdSale.Parameters.AddWithValue("@d", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmdSale.Parameters.AddWithValue("@t", total);
                    long saleId = (long)cmdSale.ExecuteScalar();

                    foreach (var item in _cart)
                    {
                        // Insert SaleItem
                        var cmdItem = new SQLiteCommand(
                            "INSERT INTO SaleItems (sale_id, medicine_id, quantity, price) VALUES (@s, @m, @q, @p)", conn, tx);
                        cmdItem.Parameters.AddWithValue("@s", saleId);
                        cmdItem.Parameters.AddWithValue("@m", item.MedicineId);
                        cmdItem.Parameters.AddWithValue("@q", item.Quantity);
                        cmdItem.Parameters.AddWithValue("@p", item.UnitPrice);
                        cmdItem.ExecuteNonQuery();

                        // Reduce stock
                        var cmdStock = new SQLiteCommand(
                            "UPDATE Medicines SET stock = stock - @q WHERE id = @m", conn, tx);
                        cmdStock.Parameters.AddWithValue("@q", item.Quantity);
                        cmdStock.Parameters.AddWithValue("@m", item.MedicineId);
                        cmdStock.ExecuteNonQuery();
                    }

                    tx.Commit();
                }

                MessageBox.Show($"✔ Продажа оформлена!\nСумма: {total:F2} ₽", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                _cart.Clear();
                RefreshCartGrid();
                LoadMedicinesCombo();
                LoadSalesHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
