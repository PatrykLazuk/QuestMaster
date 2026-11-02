namespace QuestMaster.Rules;

// A d20 roll in normal mode: one d20 plus a modifier (SRD p.6).
// The domain plans and resolves, the server provides the faces (ADR-0003).
public sealed record D20RollPlan(Modifier Modifier)
{
    public DiceNeeded DiceNeeded { get; } = new(Count: 1, Sides: 20);

    // Invalid faces mean a bug in the server's dice code, so they throw instead of returning a rejection.
    // Manual dice (M2) bring faces typed by a player, and the form of the rejection is revisited with ADR-0007.
    public D20RollResult Resolve(IReadOnlyList<int> faces)
    {
        if (faces.Count != DiceNeeded.Count)
        {
            throw new ArgumentException($"This roll needs {DiceNeeded.Count} d{DiceNeeded.Sides}, but got {faces.Count} faces.", nameof(faces));
        }

        foreach (var face in faces)
        {
            if (face < 1 || face > DiceNeeded.Sides)
            {
                throw new ArgumentOutOfRangeException(nameof(faces), face, $"A d{DiceNeeded.Sides} face must be between 1 and {DiceNeeded.Sides}.");
            }
        }

        // Normal mode rolls a single die, so that die is the kept one.
        var keptFace = faces[0];
        var dice = new List<RolledDie> { new(keptFace, IsKept: true) };

        return new D20RollResult(dice, Modifier, total: keptFace + Modifier.Value);
    }
}
