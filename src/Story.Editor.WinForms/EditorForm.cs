using Story.Engine;
using Story.Model;
using Story.Persistence;
namespace Story.Editor.WinForms;

public sealed class EditorForm : Form
{
    private readonly StoryPackageRepository _repository = new();
    private readonly string _assetsFolder = Path.Combine(Path.GetTempPath(), "StoryEditor_" + Guid.NewGuid().ToString("N"));
    private StoryDefinition _story = NewStory();
    private readonly TextBox _title = new() { Width = 310 };
    private readonly TextBox _startBlock = new() { Width = 210 };
    private readonly ListBox _blocks = new() { Dock = DockStyle.Fill };
    private readonly TextBox _blockId = new() { Width = 250 };
    private readonly RichTextBox _blockText = new() { Width = 560, Height = 110 };
    private readonly CheckBox _isEnding = new() { Text = "Bloc final" };
    private readonly TextBox _background = new() { Width = 350 };
    private readonly DataGridView _decisions = new() { Width = 620, Height = 170, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly DataGridView _properties = new() { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private readonly ListBox _validation = new() { Dock = DockStyle.Bottom, Height = 90 };
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly TabPage _propertiesTab = new("Proprietati stare");

    public EditorForm()
    {
        Directory.CreateDirectory(Path.Combine(_assetsFolder, "images"));
        Text = "Story Editor - Motor de poveste interactiva"; Width = 1150; Height = 760; StartPosition = FormStartPosition.CenterScreen;
        ToolStrip toolbar = BuildToolbar(); Controls.Add(toolbar);
        SplitContainer split = new() { Dock = DockStyle.Fill, SplitterDistance = 240 };
        split.Panel1.Controls.Add(BuildNavigation()); split.Panel2.Controls.Add(BuildEditorTabs());
        Controls.Add(split); Controls.SetChildIndex(toolbar, 0);
        RefreshAll();
    }

    private ToolStrip BuildToolbar()
    {
        ToolStrip bar = new() { Dock = DockStyle.Top };
        bar.Items.Add("Nou", null, (_, _) => { _story = NewStory(); RefreshAll(); });
        bar.Items.Add("Deschide ZIP", null, (_, _) => OpenPackage());
        bar.Items.Add("Salveaza ZIP", null, (_, _) => SavePackage());
        bar.Items.Add(new ToolStripSeparator());
        bar.Items.Add("Valideaza", null, (_, _) => ValidateStory());
        bar.Items.Add(new ToolStripSeparator());
        bar.Items.Add("Proprietati stare", null, (_, _) => _tabs.SelectedTab = _propertiesTab);
        return bar;
    }
    private Control BuildNavigation()
    {
        Panel panel = new() { Dock = DockStyle.Fill, Padding = new Padding(8) };
        Label label = new() { Text = "Blocuri", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
        FlowLayoutPanel commands = new() { Dock = DockStyle.Bottom, Height = 72, FlowDirection = FlowDirection.LeftToRight };
        Button add = new() { Text = "Adauga" }; Button delete = new() { Text = "Sterge" };
        add.Click += (_, _) => AddBlock(); delete.Click += (_, _) => DeleteBlock(); commands.Controls.Add(add); commands.Controls.Add(delete);
        _blocks.SelectedIndexChanged += (_, _) => LoadSelectedBlock(); panel.Controls.Add(_blocks); panel.Controls.Add(commands); panel.Controls.Add(label); return panel;
    }
    private Control BuildEditorTabs()
    {
        _tabs.TabPages.Add(BuildStoryTab()); _tabs.TabPages.Add(BuildBlockTab()); _tabs.TabPages.Add(BuildPropertiesTab());
        Panel panel = new() { Dock = DockStyle.Fill }; panel.Controls.Add(_tabs); panel.Controls.Add(_validation); return panel;
    }
    private TabPage BuildStoryTab()
    {
        TabPage tab = new("Poveste"); FlowLayoutPanel panel = ColumnPanel();
        panel.Controls.Add(Row("Titlu:", _title)); panel.Controls.Add(Row("Bloc de start:", _startBlock));
        Button apply = new() { Text = "Aplica metadate", Width = 150 }; apply.Click += (_, _) => { _story.Title = _title.Text.Trim(); _story.StartBlockId = _startBlock.Text.Trim(); ValidateStory(); };
        panel.Controls.Add(Row(string.Empty, apply)); tab.Controls.Add(panel); return tab;
    }
    private TabPage BuildBlockTab()
    {
        TabPage tab = new("Bloc selectat"); FlowLayoutPanel panel = ColumnPanel();
        panel.Controls.Add(Row("Identificator:", _blockId)); panel.Controls.Add(new Label { Text = "Text narativ:", AutoSize = true }); panel.Controls.Add(_blockText);
        Button pickBackground = new() { Text = "Alege..." }; pickBackground.Click += (_, _) => { string? asset = ImportImage(); if (!string.IsNullOrWhiteSpace(asset)) _background.Text = asset; };
        panel.Controls.Add(Row("Fundal relativ:", _background, pickBackground)); panel.Controls.Add(_isEnding);
        Button apply = new() { Text = "Actualizeaza bloc" }; apply.Click += (_, _) => UpdateBlock(); panel.Controls.Add(Row(string.Empty, apply));
        panel.Controls.Add(new Label { Text = "Decizii:", AutoSize = true });
        _decisions.Columns.Add("Text", "Text"); _decisions.Columns.Add("Target", "Tinta"); _decisions.Columns.Add("Condition", "Conditie");
        panel.Controls.Add(_decisions);
        Button add = new() { Text = "Adauga decizie" }; Button edit = new() { Text = "Modifica" }; Button delete = new() { Text = "Sterge" };
        add.Click += (_, _) => EditDecision(null); edit.Click += (_, _) => EditSelectedDecision(); delete.Click += (_, _) => DeleteSelectedDecision(); panel.Controls.Add(Row(string.Empty, add, edit, delete));
        tab.Controls.Add(panel); return tab;
    }
    private TabPage BuildPropertiesTab()
    {
        _properties.Columns.Add("Key", "Cheie"); _properties.Columns.Add("HudLabel", "Eticheta HUD"); _properties.Columns.Add("Min", "Min"); _properties.Columns.Add("Max", "Max"); _properties.Columns.Add("Initial", "Initial"); _properties.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Visible", HeaderText = "HUD" }); _properties.Columns.Add("Order", "Ordine"); _properties.Columns.Add("OnMin", "Bloc la minim"); _properties.Columns.Add("OnMax", "Bloc la maxim");
        Label hint = new() { Dock = DockStyle.Top, Height = 34, Text = "Definiti proprietatile care controleaza povestea si sunt afisate in HUD (Viata, Energie, Bani etc.).", Padding = new Padding(8) };
        FlowLayoutPanel buttons = new() { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(5) };
        Button add = new() { Text = "Adauga proprietate", AutoSize = true };
        Button delete = new() { Text = "Sterge selectia", AutoSize = true };
        Button apply = new() { Text = "Aplica proprietatile", AutoSize = true };
        add.Click += (_, _) => AddPropertyRow(); delete.Click += (_, _) => DeleteSelectedProperty(); apply.Click += (_, _) => ApplyProperties();
        buttons.Controls.Add(add); buttons.Controls.Add(delete); buttons.Controls.Add(apply);
        _propertiesTab.Controls.Add(_properties); _propertiesTab.Controls.Add(hint); _propertiesTab.Controls.Add(buttons);
        return _propertiesTab;
    }
    private static FlowLayoutPanel ColumnPanel() => new() { Dock = DockStyle.Fill, Padding = new Padding(16), FlowDirection = FlowDirection.TopDown, AutoScroll = true, WrapContents = false };
    private static Control Row(string label, params Control[] controls) { FlowLayoutPanel row = new() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight }; row.Controls.Add(new Label { Text = label, Width = 105, Padding = new Padding(0, 6, 0, 0) }); row.Controls.AddRange(controls); return row; }

    private void RefreshAll()
    {
        _title.Text = _story.Title; _startBlock.Text = _story.StartBlockId; _blocks.DataSource = null; _blocks.DataSource = _story.Blocks; RefreshProperties(); if (_blocks.Items.Count > 0) _blocks.SelectedIndex = 0; ValidateStory();
    }
    private void LoadSelectedBlock()
    {
        if (_blocks.SelectedItem is not StoryBlock block) return;
        _blockId.Text = block.Id; _blockText.Text = block.Text; _isEnding.Checked = block.IsEnding; _background.Text = block.BackgroundImage ?? string.Empty;
        RefreshDecisions(block);
    }
    private void AddBlock()
    {
        string id = "block." + (_story.Blocks.Count + 1); _story.Blocks.Add(new StoryBlock { Id = id, Text = "Text narativ nou." }); _blocks.DataSource = null; _blocks.DataSource = _story.Blocks; _blocks.SelectedIndex = _story.Blocks.Count - 1;
    }
    private void DeleteBlock() { if (_blocks.SelectedItem is StoryBlock block) { _story.Blocks.Remove(block); RefreshAll(); } }
    private void UpdateBlock()
    {
        if (_blocks.SelectedItem is not StoryBlock block) return;
        block.Id = _blockId.Text.Trim(); block.Text = _blockText.Text; block.IsEnding = _isEnding.Checked; block.BackgroundImage = string.IsNullOrWhiteSpace(_background.Text) ? null : _background.Text.Trim(); RefreshAll();
    }
    private void RefreshDecisions(StoryBlock block)
    {
        _decisions.Rows.Clear(); foreach (DecisionDefinition decision in block.Decisions) _decisions.Rows.Add(decision.Text, decision.TargetBlockId, decision.Condition?.Type.ToString() ?? "-");
    }
    private void EditDecision(DecisionDefinition? source)
    {
        if (_blocks.SelectedItem is not StoryBlock block) return; using DecisionDialog dialog = new(source, ImportImage); if (dialog.ShowDialog(this) != DialogResult.OK) return;
        if (source is null) block.Decisions.Add(dialog.Decision); else { int index = block.Decisions.IndexOf(source); block.Decisions[index] = dialog.Decision; }
        RefreshDecisions(block);
    }
    private void EditSelectedDecision() { if (_blocks.SelectedItem is StoryBlock block && _decisions.CurrentRow is not null && _decisions.CurrentRow.Index < block.Decisions.Count) EditDecision(block.Decisions[_decisions.CurrentRow.Index]); }
    private void DeleteSelectedDecision() { if (_blocks.SelectedItem is StoryBlock block && _decisions.CurrentRow is not null && _decisions.CurrentRow.Index < block.Decisions.Count) { block.Decisions.RemoveAt(_decisions.CurrentRow.Index); RefreshDecisions(block); } }
    private void RefreshProperties() { _properties.Rows.Clear(); foreach (StatePropertyDefinition p in _story.Properties) _properties.Rows.Add(p.Key, p.HudLabel, p.Min, p.Max, p.Initial, p.VisibleInHud, p.HudOrder, p.OnMinBlockId, p.OnMaxBlockId); }
    private void AddPropertyRow()
    {
        string key = "story.property" + (_properties.Rows.Cast<DataGridViewRow>().Count(row => !row.IsNewRow) + 1);
        _properties.Rows.Add(key, "Proprietate noua", 0, 100, 0, true, _properties.Rows.Count + 1, string.Empty, string.Empty);
    }
    private void DeleteSelectedProperty()
    {
        foreach (DataGridViewRow row in _properties.SelectedRows)
            if (!row.IsNewRow) _properties.Rows.Remove(row);
        ApplyProperties();
    }
    private void ApplyProperties()
    {
        List<StatePropertyDefinition> list = [];
        foreach (DataGridViewRow row in _properties.Rows)
        {
            if (row.IsNewRow || row.Cells[0].Value is null) continue;
            if (!int.TryParse(row.Cells[2].Value?.ToString(), out int min) || !int.TryParse(row.Cells[3].Value?.ToString(), out int max) || !int.TryParse(row.Cells[4].Value?.ToString(), out int initial) || !int.TryParse(row.Cells[6].Value?.ToString(), out int order)) { MessageBox.Show("Valorile numerice ale proprietatii nu sunt valide."); return; }
            list.Add(new StatePropertyDefinition { Key = row.Cells[0].Value!.ToString()!, HudLabel = row.Cells[1].Value?.ToString() ?? string.Empty, Min = min, Max = max, Initial = initial, VisibleInHud = Convert.ToBoolean(row.Cells[5].Value ?? false), HudOrder = order, OnMinBlockId = row.Cells[7].Value?.ToString(), OnMaxBlockId = row.Cells[8].Value?.ToString() });
        }
        _story.Properties = list; ValidateStory();
    }
    private void ValidateStory() { _validation.DataSource = null; IReadOnlyList<string> errors = new StoryValidator().Validate(_story); _validation.DataSource = errors.Count == 0 ? new[] { "Validare reusita: povestea este coerenta." } : errors; }
    private void OpenPackage()
    {
        using OpenFileDialog dialog = new() { Filter = "Pachet poveste (*.zip)|*.zip" }; if (dialog.ShowDialog() != DialogResult.OK) return;
        using StoryPackage package = _repository.Load(dialog.FileName); _story = package.Story; CopyAssets(package.WorkingDirectory); RefreshAll();
    }
    private void SavePackage()
    {
        ApplyProperties(); ValidateStory(); if (_validation.Items.Count > 0 && !_validation.Items[0]!.ToString()!.StartsWith("Validare reusita")) { MessageBox.Show("Corectati erorile inainte de salvare."); return; }
        using SaveFileDialog dialog = new() { Filter = "Pachet poveste (*.zip)|*.zip", FileName = "story.zip" }; if (dialog.ShowDialog() == DialogResult.OK) _repository.Save(dialog.FileName, _story, _assetsFolder);
    }

    private string? ImportImage()
    {
        using OpenFileDialog dialog = new() { Filter = "Imagini (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp" };
        if (dialog.ShowDialog() != DialogResult.OK) return null;
        string fileName = Path.GetFileName(dialog.FileName);
        string destination = Path.Combine(_assetsFolder, "images", fileName);
        File.Copy(dialog.FileName, destination, true);
        return "images/" + fileName;
    }
    private void CopyAssets(string sourceFolder)
    {
        string sourceImages = Path.Combine(sourceFolder, "images");
        if (!Directory.Exists(sourceImages)) return;
        foreach (string file in Directory.GetFiles(sourceImages))
            File.Copy(file, Path.Combine(_assetsFolder, "images", Path.GetFileName(file)), true);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { try { if (Directory.Exists(_assetsFolder)) Directory.Delete(_assetsFolder, true); } catch { } }
        base.Dispose(disposing);
    }
    private static StoryDefinition NewStory() => new() { Title = "Poveste noua", StartBlockId = "intro.start", Blocks = [new StoryBlock { Id = "intro.start", Text = "Inceputul povestii." }], Properties = [new StatePropertyDefinition { Key = "player.life", HudLabel = "Viata", Min = 0, Max = 100, Initial = 100, VisibleInHud = true, HudOrder = 1 }] };
}
