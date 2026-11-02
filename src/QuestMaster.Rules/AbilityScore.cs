namespace QuestMaster.Rules;

// Red step of TDD (issue #8): the type exists so the tests compile, the behavior comes next.
public sealed record AbilityScore
{
    public AbilityScore(int value)
    {
        throw new NotImplementedException();
    }

    public int Value { get; }

    public Modifier Modifier => throw new NotImplementedException();
}
