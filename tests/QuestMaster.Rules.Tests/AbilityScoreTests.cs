using QuestMaster.Rules;

using Xunit;

namespace QuestMaster.Rules.Tests;

public class AbilityScoreTests
{
    public static TheoryData<int> EveryValidScore => new(Enumerable.Range(1, 30));

    [Theory]
    [MemberData(nameof(EveryValidScore))]
    [Trait("SRD", "p.6")]
    public void Score_from_1_to_30_is_accepted(int value)
    {
        var score = new AbilityScore(value);

        Assert.Equal(value, score.Value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(31)]
    [InlineData(int.MaxValue)]
    [Trait("SRD", "p.6")]
    public void Score_outside_1_to_30_is_rejected(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AbilityScore(value));
    }

    // The expected modifiers are copied from the SRD table, not computed with a formula,
    // so a wrong formula in the code cannot also hide in the test.
    [Theory]
    [InlineData(1, -5)]
    [InlineData(2, -4)]
    [InlineData(3, -4)]
    [InlineData(4, -3)]
    [InlineData(5, -3)]
    [InlineData(6, -2)]
    [InlineData(7, -2)]
    [InlineData(8, -1)]
    [InlineData(9, -1)]
    [InlineData(10, 0)]
    [InlineData(11, 0)]
    [InlineData(12, 1)]
    [InlineData(13, 1)]
    [InlineData(14, 2)]
    [InlineData(15, 2)]
    [InlineData(16, 3)]
    [InlineData(17, 3)]
    [InlineData(18, 4)]
    [InlineData(19, 4)]
    [InlineData(20, 5)]
    [InlineData(21, 5)]
    [InlineData(22, 6)]
    [InlineData(23, 6)]
    [InlineData(24, 7)]
    [InlineData(25, 7)]
    [InlineData(26, 8)]
    [InlineData(27, 8)]
    [InlineData(28, 9)]
    [InlineData(29, 9)]
    [InlineData(30, 10)]
    [Trait("SRD", "p.6, p.187")]
    public void Modifier_matches_the_srd_table_for_every_score(int value, int expectedModifier)
    {
        var score = new AbilityScore(value);

        Assert.Equal(new Modifier(expectedModifier), score.Modifier);
    }

    [Theory]
    [InlineData(1, -5)]
    [InlineData(8, -1)]
    [InlineData(9, -1)]
    [InlineData(10, 0)]
    [InlineData(11, 0)]
    [InlineData(15, 2)]
    [InlineData(20, 5)]
    [InlineData(30, 10)]
    [Trait("GS", "GS-01")]
    [Trait("SRD", "p.6, p.187")]
    public void GS01_sample_scores_have_the_modifiers_from_the_srd_table(int value, int expectedModifier)
    {
        var score = new AbilityScore(value);

        Assert.Equal(new Modifier(expectedModifier), score.Modifier);
    }
}
