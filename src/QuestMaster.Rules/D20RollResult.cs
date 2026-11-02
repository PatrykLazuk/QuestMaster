namespace QuestMaster.Rules;

// The outcome of a d20 roll. It keeps every die and marks the kept one, for the roll record (D2.4).
// A class, not a record: a record would compare the list of dice by reference, not by content.
public sealed class D20RollResult
{
    internal D20RollResult(IReadOnlyList<RolledDie> dice, Modifier modifier, int total)
    {
        Dice = dice;
        Modifier = modifier;
        Total = total;
    }

    public IReadOnlyList<RolledDie> Dice { get; }

    public RolledDie KeptDie => throw new NotImplementedException();

    public Modifier Modifier { get; }

    public int Total { get; }
}
