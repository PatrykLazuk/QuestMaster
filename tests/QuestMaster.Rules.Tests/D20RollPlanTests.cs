using QuestMaster.Rules;

using Xunit;

namespace QuestMaster.Rules.Tests;

// Dice faces are plain data here: the domain never rolls, the server provides the faces (ADR-0003).
public class D20RollPlanTests
{
    public static TheoryData<int> EveryD20Face => new(Enumerable.Range(1, 20));

    [Fact]
    [Trait("SRD", "p.6")]
    public void Normal_roll_needs_one_d20()
    {
        var plan = new D20RollPlan(new Modifier(2));

        Assert.Equal(new DiceNeeded(Count: 1, Sides: 20), plan.DiceNeeded);
    }

    [Theory]
    [MemberData(nameof(EveryD20Face))]
    [Trait("SRD", "p.6")]
    public void Every_face_from_1_to_20_is_accepted_and_kept(int face)
    {
        var plan = new D20RollPlan(new Modifier(0));

        var result = plan.Resolve([face]);

        var die = Assert.Single(result.Dice);
        Assert.Equal(new RolledDie(face, IsKept: true), die);
        Assert.Equal(die, result.KeptDie);
        Assert.Equal(face, result.Total);
    }

    // A check has no lower limit on its total, so a low die with a negative modifier goes below zero.
    [Theory]
    [InlineData(17, 2, 19)]
    [InlineData(10, 0, 10)]
    [InlineData(8, -1, 7)]
    [InlineData(1, -5, -4)]
    [InlineData(20, 10, 30)]
    [Trait("SRD", "p.6")]
    public void Total_is_the_kept_die_plus_the_modifier(int face, int modifier, int expectedTotal)
    {
        var plan = new D20RollPlan(new Modifier(modifier));

        var result = plan.Resolve([face]);

        Assert.Equal(expectedTotal, result.Total);
        Assert.Equal(new Modifier(modifier), result.Modifier);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(21)]
    [InlineData(int.MaxValue)]
    public void Face_outside_1_to_20_is_rejected(int face)
    {
        var plan = new D20RollPlan(new Modifier(0));

        Assert.Throws<ArgumentOutOfRangeException>(() => plan.Resolve([face]));
    }

    [Theory]
    [InlineData(new int[] { })]
    [InlineData(new[] { 10, 12 })]
    public void Wrong_number_of_faces_is_rejected(int[] faces)
    {
        var plan = new D20RollPlan(new Modifier(0));

        Assert.Throws<ArgumentException>(() => plan.Resolve(faces));
    }
}
