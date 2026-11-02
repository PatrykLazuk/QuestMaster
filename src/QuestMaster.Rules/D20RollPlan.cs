namespace QuestMaster.Rules;

// A d20 roll in normal mode: one d20 plus a modifier (SRD p.6).
// The domain plans and resolves, the server provides the faces (ADR-0003).
public sealed record D20RollPlan(Modifier Modifier)
{
    public DiceNeeded DiceNeeded => throw new NotImplementedException();

    public D20RollResult Resolve(IReadOnlyList<int> faces)
    {
        throw new NotImplementedException();
    }
}
