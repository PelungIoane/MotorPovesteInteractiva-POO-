using System.Text.Json;
using System.Text.Json.Serialization;
using Story.Model;
namespace Story.Editor.WinForms;

public sealed class DecisionDialog : Form
{
    private readonly Func<string?>? _pickIcon;
    private readonly TextBox _text = new() { Width = 360 };
    private readonly TextBox _target = new() { Width = 260 };
    private readonly TextBox _icon = new() { Width = 260 };
    private readonly TextBox _condition = new() { Width = 520, Height = 100, Multiline = true, ScrollBars = ScrollBars.Vertical };
    private readonly DataGridView _effects = new() { Width = 520, Height = 150, AllowUserToAddRows = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    public DecisionDefinition Decision { get; private set; }

    public DecisionDialog(DecisionDefinition? source = null, Func<string?>? pickIcon = null)
    {
        _pickIcon = pickIcon;
        Decision = source ?? new DecisionDefinition();
        Text = "Editare decizie"; Width = 590; Height = 520; StartPosition = FormStartPosition.CenterParent;
        _effects.Columns.Add("Property", "Proprietate"); _effects.Columns.Add("Type", "Tip (Add/Set)"); _effects.Columns.Add("Value", "Valoare");
        FlowLayoutPanel panel = new() { Dock = DockStyle.Fill, Padding = new Padding(12), FlowDirection = FlowDirection.TopDown, AutoScroll = true };
        Button chooseIcon = new() { Text = "Alege..." }; chooseIcon.Click += (_, _) => { string? asset = _pickIcon?.Invoke(); if (!string.IsNullOrWhiteSpace(asset)) _icon.Text = asset; };
        panel.Controls.Add(Row("Text decizie:", _text)); panel.Controls.Add(Row("Bloc tinta:", _target)); panel.Controls.Add(Row("Iconita relativa:", _icon, chooseIcon));
        panel.Controls.Add(new Label { Text = "Conditie JSON optionala:", AutoSize = true }); panel.Controls.Add(_condition);
        panel.Controls.Add(new Label { Text = "Efecte:", AutoSize = true }); panel.Controls.Add(_effects);
        Button ok = new() { Text = "OK", Width = 100 }; Button cancel = new() { Text = "Anuleaza", Width = 100, DialogResult = DialogResult.Cancel };
        ok.Click += (_, _) => SaveAndClose(); panel.Controls.Add(Row(string.Empty, ok, cancel)); Controls.Add(panel); AcceptButton = ok; CancelButton = cancel;
        LoadDecision();
    }
    private static Control Row(string label, params Control[] controls) { FlowLayoutPanel row = new() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight }; row.Controls.Add(new Label { Text = label, Width = 115, Padding = new Padding(0, 6, 0, 0) }); row.Controls.AddRange(controls); return row; }
    private void LoadDecision()
    {
        _text.Text = Decision.Text; _target.Text = Decision.TargetBlockId; _icon.Text = Decision.IconPath ?? string.Empty;
        if (Decision.Condition is not null) _condition.Text = JsonSerializer.Serialize(Decision.Condition, JsonOptions());
        foreach (EffectDefinition effect in Decision.Effects) _effects.Rows.Add(effect.Property, effect.Type, effect.Value);
    }
    private void SaveAndClose()
    {
        if (string.IsNullOrWhiteSpace(_text.Text) || string.IsNullOrWhiteSpace(_target.Text)) { MessageBox.Show("Textul si blocul tinta sunt obligatorii."); return; }
        ConditionDefinition? condition = null;
        if (!string.IsNullOrWhiteSpace(_condition.Text))
        {
            try { condition = JsonSerializer.Deserialize<ConditionDefinition>(_condition.Text, JsonOptions()); }
            catch (JsonException ex) { MessageBox.Show("Conditia JSON nu este valida: " + ex.Message); return; }
        }
        List<EffectDefinition> effects = [];
        foreach (DataGridViewRow row in _effects.Rows)
        {
            if (row.IsNewRow || row.Cells[0].Value is null) continue;
            if (!Enum.TryParse(row.Cells[1].Value?.ToString(), true, out EffectType type) || !int.TryParse(row.Cells[2].Value?.ToString(), out int value)) { MessageBox.Show("Efect invalid."); return; }
            effects.Add(new EffectDefinition { Property = row.Cells[0].Value!.ToString()!, Type = type, Value = value });
        }
        Decision = new DecisionDefinition { Text = _text.Text.Trim(), TargetBlockId = _target.Text.Trim(), IconPath = string.IsNullOrWhiteSpace(_icon.Text) ? null : _icon.Text.Trim(), Condition = condition, Effects = effects };
        DialogResult = DialogResult.OK; Close();
    }
    private static JsonSerializerOptions JsonOptions() => new() { WriteIndented = true, Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } };
}
