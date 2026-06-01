using Story.Model;
namespace Story.Engine;

public sealed class StoryValidator
{
    public IReadOnlyList<string> Validate(StoryDefinition story, Func<string, bool>? assetExists = null)
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(story.Title)) errors.Add("Titlul povestii este obligatoriu.");
        if (story.Blocks.Count == 0) errors.Add("Povestea trebuie sa contina cel putin un bloc.");

        HashSet<string> ids = [];
        foreach (StoryBlock block in story.Blocks)
        {
            if (string.IsNullOrWhiteSpace(block.Id)) errors.Add("Un bloc nu are identificator.");
            else if (!ids.Add(block.Id)) errors.Add($"Identificator de bloc duplicat: {block.Id}.");
        }
        if (!ids.Contains(story.StartBlockId)) errors.Add("Blocul de start nu exista.");

        HashSet<string> propertyKeys = [];
        foreach (StatePropertyDefinition property in story.Properties)
        {
            if (!propertyKeys.Add(property.Key)) errors.Add($"Proprietate duplicata: {property.Key}.");
            if (property.Min > property.Initial || property.Initial > property.Max)
                errors.Add($"Limite invalide pentru proprietatea {property.Key}.");
            ValidateRedirect(property.OnMinBlockId, property.Key, ids, errors);
            ValidateRedirect(property.OnMaxBlockId, property.Key, ids, errors);
        }

        foreach (StoryBlock block in story.Blocks)
        {
            if (!string.IsNullOrWhiteSpace(block.BackgroundImage) && assetExists is not null && !assetExists(block.BackgroundImage))
                errors.Add($"Imagine inexistenta pentru blocul {block.Id}: {block.BackgroundImage}.");
            foreach (DecisionDefinition decision in block.Decisions)
            {
                if (!ids.Contains(decision.TargetBlockId)) errors.Add($"Destinatie inexistenta: {decision.TargetBlockId}.");
                foreach (EffectDefinition effect in decision.Effects)
                    if (!propertyKeys.Contains(effect.Property)) errors.Add($"Efect pe proprietate inexistenta: {effect.Property}.");
                ValidateCondition(decision.Condition, propertyKeys, errors);
            }
        }
        return errors;
    }

    private static void ValidateRedirect(string? id, string property, HashSet<string> ids, List<string> errors)
    {
        if (!string.IsNullOrWhiteSpace(id) && !ids.Contains(id))
            errors.Add($"Redirectionarea pentru {property} indica un bloc inexistent: {id}.");
    }

    private static void ValidateCondition(ConditionDefinition? condition, HashSet<string> properties, List<string> errors)
    {
        if (condition is null) return;
        if (condition.Type == ConditionType.Comparison && (condition.Property is null || !properties.Contains(condition.Property)))
            errors.Add("O conditie foloseste o proprietate inexistenta.");
        foreach (ConditionDefinition child in condition.Conditions) ValidateCondition(child, properties, errors);
    }
}
