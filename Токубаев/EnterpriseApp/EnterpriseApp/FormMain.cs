using System;
using System.Drawing;
using System.Windows.Forms;

public partial class FormMain : Form
{
    private ReportService _reportService;
    private int currentPage = 1;
    private int pageSize = 3;

    // Элементы управления
    private TabControl tabControl1;
    private TabPage tabPage1, tabPage2, tabPage3, tabPage4;

    // Вкладка 1
    private DataGridView dgv1;
    private ComboBox cmbCategory;
    private Button btnFilter;

    // Вкладка 2
    private DataGridView dgv2;
    private Label lblAudit;

    // Вкладка 3
    private DataGridView dgv3;
    private Button btnExcept, btnIntersect, btnUnion;

    // Вкладка 4
    private DataGridView dgv4;
    private Button btnPrev, btnNext;
    private Label lblPageInfo;

    public FormMain()
    {
        DatabaseInitializer.Initialize();
        _reportService = new ReportService();

        BuildUI();
        LoadData();
    }

    private void BuildUI()
    {
        this.Text = "Комплексный анализ данных LINQ";
        this.Width = 900;
        this.Height = 600;

        tabControl1 = new TabControl { Dock = DockStyle.Fill };
        tabPage1 = new TabPage("Реестр продаж");
        tabPage2 = new TabPage("Группировка и Агрегация");
        tabPage3 = new TabPage("Анализ каналов");
        tabPage4 = new TabPage("Постраничный просмотр");

        // --- Вкладка 1 ---
        cmbCategory = new ComboBox { Left = 20, Top = 20, Width = 150 };
        cmbCategory.Items.Add("Все");
        foreach (var c in _reportService.GetCategories()) cmbCategory.Items.Add(c.Name);
        cmbCategory.SelectedIndex = 0;

        btnFilter = new Button { Text = "Применить фильтр", Left = 190, Top = 18, Width = 130 };
        btnFilter.Click += (s, e) => LoadTab1Data();

        dgv1 = new DataGridView { Left = 20, Top = 60, Width = 830, Height = 450, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        tabPage1.Controls.Add(cmbCategory);
        tabPage1.Controls.Add(btnFilter);
        tabPage1.Controls.Add(dgv1);

        // --- Вкладка 2 ---
        dgv2 = new DataGridView { Left = 20, Top = 20, Width = 830, Height = 380, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        lblAudit = new Label { Left = 20, Top = 420, Width = 830, Height = 40, Font = new Font("Arial", 10, FontStyle.Bold) };
        tabPage2.Controls.Add(dgv2);
        tabPage2.Controls.Add(lblAudit);

        // --- Вкладка 3 ---
        btnExcept = new Button { Text = "Online Only (Except)", Left = 20, Top = 20, Width = 160 };
        btnIntersect = new Button { Text = "Общие (Intersect)", Left = 190, Top = 20, Width = 150 };
        btnUnion = new Button { Text = "Все уникальные (Union)", Left = 350, Top = 20, Width = 160 };

        btnExcept.Click += (s, e) => dgv3.DataSource = _reportService.GetSetProducts("Except");
        btnIntersect.Click += (s, e) => dgv3.DataSource = _reportService.GetSetProducts("Intersect");
        btnUnion.Click += (s, e) => dgv3.DataSource = _reportService.GetSetProducts("Union");

        dgv3 = new DataGridView { Left = 20, Top = 60, Width = 830, Height = 450, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        tabPage3.Controls.Add(btnExcept);
        tabPage3.Controls.Add(btnIntersect);
        tabPage3.Controls.Add(btnUnion);
        tabPage3.Controls.Add(dgv3);

        // --- Вкладка 4 ---
        dgv4 = new DataGridView { Left = 20, Top = 20, Width = 830, Height = 400, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        btnPrev = new Button { Text = "Назад", Left = 20, Top = 440, Width = 100 };
        btnNext = new Button { Text = "Вперед", Left = 130, Top = 440, Width = 100 };
        lblPageInfo = new Label { Left = 250, Top = 445, Width = 200, Height = 30 };

        btnPrev.Click += (s, e) => { if (currentPage > 1) { currentPage--; LoadTabPage4(); } };
        btnNext.Click += (s, e) => { currentPage++; LoadTabPage4(); };

        tabPage4.Controls.Add(dgv4);
        tabPage4.Controls.Add(btnPrev);
        tabPage4.Controls.Add(btnNext);
        tabPage4.Controls.Add(lblPageInfo);

        // Добавление вкладок
        tabControl1.TabPages.Add(tabPage1);
        tabControl1.TabPages.Add(tabPage2);
        tabControl1.TabPages.Add(tabPage3);
        tabControl1.TabPages.Add(tabPage4);

        this.Controls.Add(tabControl1);
    }

    private void LoadData()
    {
        LoadTab1Data();
        LoadTab2Data();
        LoadTabPage4();
    }

    private void LoadTab1Data()
    {
        string cat = cmbCategory.SelectedItem?.ToString() ?? "Все";
        dgv1.DataSource = _reportService.GetFilteredAndSortedReports(cat, "", 1, 100);
    }

    private void LoadTab2Data()
    {
        var data = _reportService.GetGroupedAnalytics();
        dgv2.DataSource = data.groupedData;
        lblAudit.Text = data.auditString;
    }

    private void LoadTabPage4()
    {
        var data = _reportService.GetFilteredAndSortedReports("Все", "", currentPage, pageSize);
        dgv4.DataSource = data;
        lblPageInfo.Text = $"Страница {currentPage}";
    }
}