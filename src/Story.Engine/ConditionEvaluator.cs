using Story.Model;
namespace Story.Engine;

public static class ConditionEvaluator
{
    public static bool Evaluate(ConditionDefinition? condition, IReadOnlyDictionary<string, int> values)
    {
        if (condition is null) return true;
        return condition.Type switch
        {
            ConditionType.And => condition.Conditions.All(item => Evaluate(item, values)),
            ConditionType.Or => condition.Conditions.Any(item => Evaluate(item, values)),
            _ => EvaluateComparison(condition, values)
        };
    }

    private static bool EvaluateComparison(ConditionDefinition condition, IReadOnlyDictionary<string, int> values)
    {
        if (string.IsNullOrWhiteSpace(condition.Property) || !values.TryGetValue(condition.Property, out int actual))
            return false;
        return condition.Operator switch
        {
            ComparisonOperator.Less => actual < condition.Value,
            ComparisonOperator.LessOrEqual => actual <= condition.Value,
            ComparisonOperator.Greater => actual > condition.Value,
            ComparisonOperator.GreaterOrEqual => actual >= condition.Value,
            ComparisonOperator.Equal => actual == condition.Value,
            ComparisonOperator.NotEqual => actual != condition.Value,
            _ => false
        };
    }
}
