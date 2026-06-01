namespace Story.Model;

public sealed class StoryDefinition
{
    public string Title { get; set; } = "Poveste noua";
    public string StartBlockId { get; set; } = string.Empty;
    public List<StatePropertyDefinition> Properties { get; set; } = [];
    public List<StoryBlock> Blocks { get; set; } = [];
}

public sealed class StatePropertyDefinition
{
    public string Key { get; set; } = string.Empty;
    public string HudLabel { get; set; } = string.Empty;
    public int Min { get; set; }
    public int Max { get; set; } = 100;
    public int Initial { get; set; }
    public bool VisibleInHud { get; set; } = true;
    public int HudOrder { get; set; }
    public string? OnMinBlockId { get; set; }
    public string? OnMaxBlockId { get; set; }
}

public sealed class StoryBlock
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsEnding { get; set; }
    public string? BackgroundImage { get; set; }
    public List<DecisionDefinition> Decisions { get; set; } = [];
    public override string ToString() => Id;
}

public sealed class DecisionDefinition
{
    public string Text { get; set; } = string.Empty;
    public string TargetBlockId { get; set; } = string.Empty;
    public string? IconPath { get; set; }
    public ConditionDefinition? Condition { get; set; }
    public List<EffectDefinition> Effects { get; set; } = [];
}

public sealed class EffectDefinition
{
    public EffectType Type { get; set; } = EffectType.Add;
    public string Property { get; set; } = string.Empty;
    public int Value { get; set; }
}

public enum EffectType { Add, Set }
public enum ConditionType { Comparison, And, Or }
public enum ComparisonOperator { Less, LessOrEqual, Greater, GreaterOrEqual, Equal, NotEqual }

public sealed class ConditionDefinition
{
    public ConditionType Type { get; set; } = ConditionType.Comparison;
    public string? Property { get; set; }
    public ComparisonOperator Operator { get; set; } = ComparisonOperator.Equal;
    public int Value { get; set; }
    public List<ConditionDefinition> Conditions { get; set; } = [];
}

public sealed class SavedGame
{
    public string StoryTitle { get; set; } = string.Empty;
    public string CurrentBlockId { get; set; } = string.Empty;
    public Dictionary<string, int> Values { get; set; } = [];
}
