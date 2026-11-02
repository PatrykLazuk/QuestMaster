namespace QuestMaster.Rules;

// One of a creature's six ability scores, such as Strength 15 (SRD p.6).
// A class, not a struct: a struct always has a default value of 0, which is not a valid score.
public sealed record AbilityScore
{
    public const int Minimum = 1;
    public const int Maximum = 30;

    public AbilityScore(int value)
    {
        if (value < Minimum || value > Maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"An ability score must be between {Minimum} and {Maximum}.");
        }

        Value = value;
    }

    public int Value { get; }

    // SRD p.6: subtract 10 and divide by 2; p.187: every division rounds down.
    // Plain integer division would be wrong here: C# truncates toward zero, so (9 - 10) / 2 is 0 instead of -1.
    public Modifier Modifier => new((int)Math.Floor((Value - 10) / 2.0));
}
