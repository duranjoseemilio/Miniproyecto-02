using FacturacionLogistica.Domain;
using FacturacionLogistica.Services;
namespace FacturacionLogistica.UI;

public sealed class FormPrincipal : Form
{
    readonly Configuracion _config;
    readonly PaqueteService _service;
    readonly CsvService _csv = new();
    readonly IdiomaService _lang = new();
    ResultadoLote? _resultado;
    bool _en;

    readonly ComboBox idioma = new();
    readonly TextBox entrada = new(), detalle = new();
    readonly Button procesar = new(), limpiar = new(), exportar = new(), historial = new();
    readonly DataGridView grid = new(), gridErrores = new();
    readonly Label lblEntrada = new(), lblTotal = new(), lblEstado = new();
    readonly GroupBox grupo = new();
    readonly TabControl tabs = new();

    readonly Color navy = Color.FromArgb(36, 59, 83);
    readonly Color teal = Color.FromArgb(40, 125, 142);
    readonly Color green = Color.FromArgb(39, 132, 91);
    readonly Color amber = Color.FromArgb(197, 138, 50);
    readonly Color canvas = Color.FromArgb(244, 247, 250);

    public FormPrincipal(Configuracion c, PaqueteService s)
    {
        _config = c; _service = s;
        Build(); Events(); ApplyLanguage(); ApplyTheme();
    }

    void Build()
    {
        Width = 1320; Height = 850; MinimumSize = new(1050, 700);
        StartPosition = FormStartPosition.CenterScreen; BackColor = canvas;
        Font = new Font("Segoe UI", 9F);

        var top = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = navy, Padding = new Padding(14, 8, 14, 8) };
        var brand = new Label { Text = "FACTURACIÓN LOGÍSTICA", ForeColor = Color.White, Font = new Font("Segoe UI", 13F, FontStyle.Bold), AutoSize = true, Location = new Point(14, 16) };
        var languageLabel = new Label { Text = "Idioma:", ForeColor = Color.White, AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        idioma.DropDownStyle = ComboBoxStyle.DropDownList; idioma.Items.AddRange(new object[] { "Español", "English" }); idioma.SelectedIndex = 0; idioma.Width = 125; idioma.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        top.Controls.Add(brand); top.Controls.Add(languageLabel); top.Controls.Add(idioma);
        top.Resize += (_, _) => { idioma.Location = new Point(top.ClientSize.Width - idioma.Width - 14, 15); languageLabel.Location = new Point(idioma.Left - 72, 18); };
        Controls.Add(top);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(14), BackColor = canvas };
        layout.ColumnStyles.Add(new(SizeType.Percent, 70)); layout.ColumnStyles.Add(new(SizeType.Percent, 30));
        layout.RowStyles.Add(new(SizeType.Absolute, 190)); layout.RowStyles.Add(new(SizeType.Percent, 100)); layout.RowStyles.Add(new(SizeType.Absolute, 48));

        var cap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.White };
        lblEntrada.Dock = DockStyle.Top; lblEntrada.Height = 28; lblEntrada.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        entrada.Multiline = true; entrada.ScrollBars = ScrollBars.Vertical; entrada.Dock = DockStyle.Fill; entrada.Font = new Font("Consolas", 10F); entrada.BorderStyle = BorderStyle.FixedSingle;
        var acciones = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 5, 0, 0) };
        foreach (var b in new[] { procesar, limpiar, exportar, historial }) { b.AutoSize = true; b.Height = 32; b.Padding = new Padding(12, 3, 12, 3); b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0; b.Font = new Font("Segoe UI", 9F, FontStyle.Bold); b.Cursor = Cursors.Hand; acciones.Controls.Add(b); }
        cap.Controls.Add(entrada); cap.Controls.Add(acciones); cap.Controls.Add(lblEntrada); layout.Controls.Add(cap, 0, 0); layout.SetColumnSpan(cap, 2);

        tabs.Dock = DockStyle.Fill; tabs.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        var tabValidos = new TabPage("Paquetes procesados") { BackColor = Color.White, Padding = new Padding(6) };
        var tabErrores = new TabPage("Errores de validación") { BackColor = Color.White, Padding = new Padding(6) };
        ConfigureGrid(grid); ConfigureGrid(gridErrores); tabValidos.Controls.Add(grid); tabErrores.Controls.Add(gridErrores); tabs.TabPages.Add(tabValidos); tabs.TabPages.Add(tabErrores); layout.Controls.Add(tabs, 0, 1);

        grupo.Dock = DockStyle.Fill; grupo.Padding = new Padding(10); grupo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        detalle.Dock = DockStyle.Fill; detalle.Multiline = true; detalle.ReadOnly = true; detalle.ScrollBars = ScrollBars.Vertical; detalle.BorderStyle = BorderStyle.None; detalle.BackColor = Color.White; detalle.Font = new Font("Consolas", 10F);
        grupo.Controls.Add(detalle); layout.Controls.Add(grupo, 1, 1);

        var foot = new Panel { Dock = DockStyle.Fill, BackColor = navy, Padding = new Padding(10, 5, 10, 5) };
        lblTotal.Dock = DockStyle.Right; lblTotal.Width = 300; lblTotal.TextAlign = ContentAlignment.MiddleRight;
        lblEstado.Dock = DockStyle.Fill; lblEstado.TextAlign = ContentAlignment.MiddleLeft;
        foot.Controls.Add(lblEstado); foot.Controls.Add(lblTotal); layout.Controls.Add(foot, 0, 2); layout.SetColumnSpan(foot, 2);
        Controls.Add(layout); top.BringToFront();
    }

    static void ConfigureGrid(DataGridView dgv)
    {
        dgv.Dock = DockStyle.Fill; dgv.ReadOnly = true; dgv.AllowUserToAddRows = false; dgv.AllowUserToDeleteRows = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect; dgv.MultiSelect = false; dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.RowHeadersVisible = false; dgv.BorderStyle = BorderStyle.None; dgv.BackgroundColor = Color.White; dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersHeight = 38; dgv.RowTemplate.Height = 30; dgv.GridColor = Color.FromArgb(226, 232, 238);
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 238, 240); dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 50, 65);
        dgv.DefaultCellStyle.Padding = new Padding(5, 2, 5, 2); dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 251);
    }

    void ApplyTheme()
    {
        procesar.BackColor = teal; procesar.ForeColor = Color.White;
        limpiar.BackColor = Color.FromArgb(230, 235, 240); limpiar.ForeColor = navy;
        exportar.BackColor = green; exportar.ForeColor = Color.White;
        historial.BackColor = amber; historial.ForeColor = Color.White;
        foreach (var dgv in new[] { grid, gridErrores }) { dgv.ColumnHeadersDefaultCellStyle.BackColor = navy; dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold); dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F); }
        gridErrores.DefaultCellStyle.BackColor = Color.FromArgb(255, 249, 248);
        gridErrores.DefaultCellStyle.SelectionBackColor = Color.FromArgb(250, 225, 222);
        gridErrores.DefaultCellStyle.SelectionForeColor = Color.FromArgb(100, 35, 35);
    }

    void Events()
    {
        procesar.Click += ProcessClick;
        limpiar.Click += (_, _) => { entrada.Clear(); grid.DataSource = null; gridErrores.DataSource = null; detalle.Clear(); lblTotal.Text = ""; lblEstado.Text = ""; _resultado = null; tabs.SelectedIndex = 0; };
        exportar.Click += ExportClick; historial.Click += HistoryClick;
        idioma.SelectedIndexChanged += (_, _) => { _en = idioma.SelectedIndex == 1; ApplyLanguage(); };
        grid.CellClick += GridClick;
    }

    void ProcessClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(entrada.Text)) { MessageBox.Show(_lang.T("empty", _en)); return; }
        try
        {
            _resultado = _service.Procesar(entrada.Text, _config);
            ShowPackages(_resultado.Paquetes); ShowErrors(_resultado.Errores);
            lblTotal.Text = $"{_lang.T("total", _en)} ${_resultado.TotalLote:N2}";
            lblEstado.Text = $"{_lang.T("valid", _en)}: {_resultado.Paquetes.Length} | {_lang.T("invalid", _en)}: {_resultado.Errores.Length}";
            if (_resultado.Errores.Length > 0)
            {
                tabs.SelectedIndex = 1;
                MessageBox.Show($"{_resultado.Errores.Length} {_lang.T("invalidFound", _en)}. {_lang.T("errorsTabHint", _en)}", _lang.T("errors", _en), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else { tabs.SelectedIndex = 0; MessageBox.Show(_lang.T("done", _en), Text, MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }
        catch (Exception ex) { MessageBox.Show(_lang.T("processError", _en) + ": " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    void ShowPackages(Paquete[] ps)
    {
        grid.DataSource = ps.Select(p => new { p.Codigo, p.Cliente, p.Zona, p.PesoKg, p.ValorDeclarado, Subtotal = p.Desglose.SubtotalPeso, Recargo = p.Desglose.RecargoZona, Penalizacion = p.Desglose.PenalizacionExcesoPeso, Seguro = p.Desglose.Seguro, IVA = p.Desglose.Iva, Total = p.Desglose.Total }).ToList();
        TranslateColumns();
        if (grid.Columns.Contains("Total"))
        {
            grid.Columns["Total"].DefaultCellStyle.BackColor = Color.FromArgb(230, 244, 236);
            grid.Columns["Total"].DefaultCellStyle.ForeColor = Color.FromArgb(30, 105, 68);
            grid.Columns["Total"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }

    void ShowErrors(ErrorRegistro[] errors)
    {
        var rows = errors.Select(error =>
        {
            var fields = error.TextoOriginal.Split(';');
            var code = fields.Length > 0 ? fields[0].Trim() : "";
            return new { Linea = error.NumeroLinea, Codigo = string.IsNullOrWhiteSpace(code) ? "(sin código)" : code, RegistroOriginal = error.TextoOriginal, Motivo = TranslateReason(error.Motivo) };
        }).ToList();
        gridErrores.DataSource = rows;
        if (gridErrores.Columns.Count >= 4)
        {
            gridErrores.Columns[0].HeaderText = _lang.T("line", _en); gridErrores.Columns[1].HeaderText = _lang.T("code", _en);
            gridErrores.Columns[2].HeaderText = _lang.T("original", _en); gridErrores.Columns[3].HeaderText = _lang.T("reason", _en);
            gridErrores.Columns[2].FillWeight = 150; gridErrores.Columns[3].FillWeight = 130;
            gridErrores.Columns[3].DefaultCellStyle.ForeColor = Color.FromArgb(150, 45, 45);
            gridErrores.Columns[3].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }

    void GridClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _resultado is null || e.RowIndex >= _resultado.Paquetes.Length) return;
        var p = _resultado.Paquetes[e.RowIndex];
        detalle.Text = $"{_lang.T("code", _en)}: {p.Codigo}\r\n{_lang.T("client", _en)}: {p.Cliente}\r\n{_lang.T("zone", _en)}: {p.Zona}\r\n{_lang.T("weight", _en)}: {p.PesoKg:N2}\r\n{_lang.T("declared", _en)}: ${p.ValorDeclarado:N2}\r\n\r\n{_lang.T("subtotal", _en)}: ${p.Desglose.SubtotalPeso:N2}\r\n{_lang.T("fee", _en)}: ${p.Desglose.RecargoZona:N2}\r\n{_lang.T("penalty", _en)}: ${p.Desglose.PenalizacionExcesoPeso:N2}\r\n{_lang.T("insurance", _en)}: ${p.Desglose.Seguro:N2}\r\n{_lang.T("fixed", _en)}: ${p.Desglose.CostoFijo:N2}\r\n{_lang.T("tax", _en)}: ${p.Desglose.Iva:N2}\r\n{_lang.T("grand", _en)}: ${p.Desglose.Total:N2}";
    }

    void ExportClick(object? sender, EventArgs e)
    {
        if (_resultado is null || _resultado.Paquetes.Length == 0) { MessageBox.Show(_lang.T("nodata", _en)); return; }
        using var d = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", DefaultExt = "csv", AddExtension = true, FileName = $"Auditoria_{DateTime.Now:yyyyMMdd_HHmm}.csv" };
        if (d.ShowDialog() != DialogResult.OK) return;
        try { _csv.Exportar(d.FileName, _resultado.Paquetes); MessageBox.Show(_lang.T("exported", _en)); }
        catch (Exception ex) { MessageBox.Show(_lang.T("exportError", _en) + ": " + ex.Message); }
    }

    void HistoryClick(object? sender, EventArgs e)
    {
        try { var ps = _service.Historial().ToArray(); _resultado = new() { Paquetes = ps }; ShowPackages(ps); gridErrores.DataSource = null; detalle.Clear(); lblEstado.Text = $"{_lang.T("history", _en)}: {ps.Length}"; lblTotal.Text = $"{_lang.T("records", _en)}: {ps.Length}"; tabs.SelectedIndex = 0; }
        catch (Exception ex) { MessageBox.Show(_lang.T("historyError", _en) + ": " + ex.Message); }
    }

    string TranslateReason(string reason)
    {
        if (!_en) return reason;
        return reason.Replace("Línea vacía.", "Empty line.").Replace("Se esperaban 5 campos y llegaron", "Expected 5 fields but received")
            .Replace("Código inválido: use PACK-xxx o COD-xxx.", "Invalid code: use PACK-xxx or COD-xxx.").Replace("Cliente vacío.", "Customer is empty.")
            .Replace("Zona inválida: use Zona A, Zona B o Zona C.", "Invalid zone: use Zone A, Zone B or Zone C.")
            .Replace("Peso no numérico; use punto decimal.", "Weight is not numeric; use a decimal point.")
            .Replace("Valor declarado no numérico; use punto decimal.", "Declared value is not numeric; use a decimal point.")
            .Replace("El peso debe ser mayor que cero.", "Weight must be greater than zero.").Replace("Valor declarado negativo.", "Declared value cannot be negative.");
    }

    void ApplyLanguage()
    {
        Text = _lang.T("title", _en); lblEntrada.Text = _lang.T("input", _en); procesar.Text = _lang.T("process", _en);
        limpiar.Text = _lang.T("clear", _en); exportar.Text = _lang.T("export", _en); historial.Text = _lang.T("history", _en); grupo.Text = _lang.T("detail", _en);
        tabs.TabPages[0].Text = _lang.T("validTab", _en); tabs.TabPages[1].Text = _lang.T("errorsTab", _en);
        if (grid.Columns.Count > 0) TranslateColumns();
        if (_resultado is not null && _resultado.Errores.Length > 0) ShowErrors(_resultado.Errores);
    }

    void TranslateColumns()
    {
        string[] keys = { "code", "client", "zone", "weight", "declared", "subtotal", "fee", "penalty", "insurance", "tax", "grand" };
        for (int i = 0; i < keys.Length && i < grid.Columns.Count; i++) grid.Columns[i].HeaderText = _lang.T(keys[i], _en);
    }
}
