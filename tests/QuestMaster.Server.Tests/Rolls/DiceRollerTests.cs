using QuestMaster.Rules;
using QuestMaster.Server.Rolls;

using Xunit;

namespace QuestMaster.Server.Tests.Rolls;

// Smoke tests of the production dice roller (ADR-0013, Decision 4). Whether the faces are fair
// is checked separately by DiceUniformityTests.
public class DiceRollerTests
{
    [Fact]
    public void Rolls_as_many_dice_as_needed_with_faces_in_range()
    {
        var roller = new DiceRoller();

        var faces = roller.Roll(new DiceNeeded(Count: 3, Sides: 6));

        Assert.Equal(3, faces.Count);
        Assert.All(faces, face => Assert.InRange(face, 1, 6));
    }

    // Catches an off-by-one in the range, such as faces 0-19 or 1-19.
    // A fair d20 misses some face in 1,000 rolls with probability at most 20 x 0.95^1000, about 10^-21.
    [Fact]
    public void A_thousand_d20_rolls_show_every_face_from_1_to_20_and_nothing_else()
    {
        var roller = new DiceRoller();

        var faces = roller.Roll(new DiceNeeded(Count: 1_000, Sides: 20));

        Assert.Equal(Enumerable.Range(1, 20), faces.Distinct().Order());
    }
}
