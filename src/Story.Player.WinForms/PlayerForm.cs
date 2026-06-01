using Story.Engine;
using Story.Model;
using Story.Persistence;
namespace Story.Player.WinForms;

public sealed class PlayerForm : Form
{
    private readonly Label _title = new() { Dock = DockStyle.Top, Height = 42, Font = new Font("Segoe UI", 18, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
    private readonly FlowLayoutPanel _hud = new() { Dock = DockStyle.Top, Height = 42, Padding = new Padding(8), BackColor = Color.FromArgb(235, 240, 248) };
    private readonly PictureBox _image = new() { Dock = DockStyle.Top, Height = 270, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(28, 35, 48) };
    private readonly RichTextBox _text = new() { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Segoe UI", 12), BorderStyle = BorderStyle.None, Padding = new Padding(16) };
    private readonly FlowLayoutPanel _decisions = new() { Dock = DockStyle.Bottom, Height = 106, Padding = new Padding(10), FlowDirection = FlowDirection.TopDown, AutoScroll = true };
    private readonly ToolStripStatusLabel _status = new("Deschideti o poveste pentru a incepe.");
    private readonly StoryPackageRepository _packages = new();
    private readonly SaveGameRepository _saves = new();
    private StoryPackage? _package;
    private GameEngine? _engine;

    public PlayerForm()
    {
        Text = "Story Player - Motor de poveste interactiva";
        Width = 980; Height = 700; StartPosition = FormStartPosition.CenterScreen;
        MainMenuStrip = BuildMenu();
        Controls.Add(_text); Controls.Add(_decisions); Controls.Add(_image); Controls.Add(_hud); Controls.Add(_title);
        Controls.Add(new StatusStrip { Items = { _status } });
        Controls.Add(MainMenuStrip);
    }

    private MenuStrip BuildMenu()
    {
        MenuStrip menu = new();
        ToolStripMenuItem file = new("Fisier");
        file.DropDownItems.Add("Deschide poveste...", null, (_, _) => OpenStory());
        file.DropDownItems.Add("Salveaza progres...", null, (_, _) => SaveProgress());
        file.DropDownItems.Add("Incarca progres...", null, (_, _) => LoadProgress());
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("Iesire", null, (_, _) => Close());
        ToolStripMenuItem game = new("Poveste");
        game.DropDownItems.Add("Restart", null, (_, _) => RestartStory());
        menu.Items.Add(file); menu.Items.Add(game);
        return menu;
    }

    private void OpenStory()
    {
        using OpenFileDialog dialog = new() { Filter = "Pachet poveste (*.zip)|*.zip", Title = "Deschide o poveste" };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        try
        {
            _package?.Dispose();
            _package = _packages.Load(dialog.FileName);
            _engine = new GameEngine(_package.Story);
            RenderBlock();
            _status.Text = "Poveste incarcata: " + _package.Story.Title;
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Pachet invalid", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void RenderBlock()
    {
        if (_engine is null) return;
        StoryBlock block = _engine.CurrentBlock;
        _title.Text = _engine.Story.Title + " - " + block.Id;
        _text.Text = block.Text;
        _hud.Controls.Clear();
        foreach (StatePropertyDefinition property in _engine.Story.Properties.Where(p => p.VisibleInHud).OrderBy(p => p.HudOrder))
            _hud.Controls.Add(new Label { AutoSize = true, Padding = new Padding(8, 6, 8, 6), Text = $"{property.HudLabel}: {_engine.State.Values[property.Key]}" });
        _image.Image?.Dispose(); _image.Image = null;
        if (_package is not null && !string.IsNullOrWhiteSpace(block.BackgroundImage) && _package.AssetExists(block.BackgroundImage))
            using (Image source = Image.FromFile(_package.AssetPath(block.BackgroundImage))) _image.Image = new Bitmap(source);
        _decisions.Controls.Clear();
        if (block.IsEnding)
        {
            _decisions.Controls.Add(new Label { AutoSize = true, Text = "Finalul povestii. Alegeti Restart pentru a juca din nou.", Padding = new Padding(8) });
            return;
        }
        foreach (DecisionDefinition decision in _engine.AvailableDecisions())
        {
            Button button = new() { Text = decision.Text, AutoSize = true, Height = 36, Padding = new Padding(10, 4, 10, 4), Tag = decision };
            button.Click += (_, _) => { _engine.Choose((DecisionDefinition)button.Tag!); RenderBlock(); };
            _decisions.Controls.Add(button);
        }
    }

    private void RestartStory() { if (_engine is null) return; _engine.Restart(); RenderBlock(); _status.Text = "Poveste repornita."; }
    private void SaveProgress()
    {
        if (_engine is null) return;
        using SaveFileDialog dialog = new() { Filter = "Salvare poveste (*.json)|*.json" };
        if (dialog.ShowDialog() == DialogResult.OK) { _saves.Save(dialog.FileName, _engine.State.ToSave(_engine.Story.Title)); _status.Text = "Progres salvat."; }
    }
    private void LoadProgress()
    {
        if (_engine is null) return;
        using OpenFileDialog dialog = new() { Filter = "Salvare poveste (*.json)|*.json" };
        if (dialog.ShowDialog() == DialogResult.OK) { _engine.Restore(_saves.Load(dialog.FileName)); RenderBlock(); _status.Text = "Progres incarcat."; }
    }
    protected override void Dispose(bool disposing) { if (disposing) _package?.Dispose(); base.Dispose(disposing); }
}
