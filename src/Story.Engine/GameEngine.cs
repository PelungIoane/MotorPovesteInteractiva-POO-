using Story.Model;
namespace Story.Engine;

public sealed class GameEngine
{
    private readonly Dictionary<string, StoryBlock> _blocks;
    public StoryDefinition Story { get; }
    public GameState State { get; private set; }
    public StoryBlock CurrentBlock => _blocks[State.CurrentBlockId];

    public GameEngine(StoryDefinition story)
    {
        Story = story;
        _blocks = story.Blocks.ToDictionary(block => block.Id, StringComparer.OrdinalIgnoreCase);
        State = GameState.Create(story);
    }

    public void Restart() => State = GameState.Create(Story);

    public IReadOnlyList<DecisionDefinition> AvailableDecisions() => CurrentBlock.Decisions
        .Where(decision => ConditionEvaluator.Evaluate(decision.Condition, State.Values)).ToList();

    public void Choose(DecisionDefinition decision)
    {
        if (!AvailableDecisions().Contains(decision))
            throw new InvalidOperationException("Decizia nu este disponibila in starea curenta.");

        foreach (EffectDefinition effect in decision.Effects)
            ApplyEffect(effect);

        string? redirect = EvaluateAutomaticRedirect();
        State.CurrentBlockId = redirect ?? decision.TargetBlockId;
    }

    public void Restore(SavedGame savedGame)
    {
        if (!_blocks.ContainsKey(savedGame.CurrentBlockId))
            throw new InvalidDataException("Salvarea contine un bloc inexistent.");
        State = GameState.Create(Story);
        State.CurrentBlockId = savedGame.CurrentBlockId;
        foreach ((string key, int value) in savedGame.Values)
            if (State.Values.ContainsKey(key)) State.Values[key] = value;
    }

    private void ApplyEffect(EffectDefinition effect)
    {
        StatePropertyDefinition definition = Story.Properties.First(p => p.Key == effect.Property);
        int current = State.Values[effect.Property];
        int next = effect.Type == EffectType.Set ? effect.Value : current + effect.Value;
        State.Values[effect.Property] = Math.Clamp(next, definition.Min, definition.Max);
    }

    private string? EvaluateAutomaticRedirect()
    {
        foreach (StatePropertyDefinition definition in Story.Properties)
        {
            int value = State.Values[definition.Key];
            if (value == definition.Min && !string.IsNullOrWhiteSpace(definition.OnMinBlockId))
                return definition.OnMinBlockId;
            if (value == definition.Max && !string.IsNullOrWhiteSpace(definition.OnMaxBlockId))
                return definition.OnMaxBlockId;
        }
        return null;
    }
}
