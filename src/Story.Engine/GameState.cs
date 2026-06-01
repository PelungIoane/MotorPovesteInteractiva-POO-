using Story.Model;
namespace Story.Engine;

public sealed class GameState
{
    public string CurrentBlockId { get; internal set; } = string.Empty;
    public Dictionary<string, int> Values { get; } = [];

    public static GameState Create(StoryDefinition story)
    {
        GameState state = new() { CurrentBlockId = story.StartBlockId };
        foreach (StatePropertyDefinition property in story.Properties)
            state.Values[property.Key] = property.Initial;
        return state;
    }

    public SavedGame ToSave(string title) => new()
    {
        StoryTitle = title,
        CurrentBlockId = CurrentBlockId,
        Values = new Dictionary<string, int>(Values)
    };
}
