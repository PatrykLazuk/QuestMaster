using System.Security.Cryptography;

using QuestMaster.Rules;

namespace QuestMaster.Server.Rolls;

// Server dice (D7.1): a cryptographically secure generator, so nobody can predict or steer a roll.
// A concrete class for now. The interface, with a deterministic implementation for integration tests,
// comes with issue #24, where the first code uses it (ADR-0003, point 7; ADR-0014).
public sealed class DiceRoller
{
    public IReadOnlyList<int> Roll(DiceNeeded dice)
    {
        var faces = new int[dice.Count];
        for (var i = 0; i < dice.Count; i++)
        {
            // The upper bound is exclusive, so sides + 1 makes the highest face possible.
            faces[i] = RandomNumberGenerator.GetInt32(1, dice.Sides + 1);
        }

        return faces;
    }
}
