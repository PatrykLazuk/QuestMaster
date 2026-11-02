using QuestMaster.Rules;
using QuestMaster.Server.Rolls;

using Xunit;

namespace QuestMaster.Server.Tests.Rolls;

// D7.4 and ADR-0013, Decision 5: a chi-square goodness-of-fit test on the production d20.
//
// How the threshold was derived:
// - 200,000 rolls of a fair d20 give an expected count of 10,000 per face.
// - The statistic is the sum over the 20 faces of (observed - expected)^2 / expected.
//   For a fair die it follows a chi-square distribution with 20 - 1 = 19 degrees of freedom.
// - The chance that a fair die exceeds a threshold x is P(chi-square > x) = Q(19/2, x/2),
//   where Q is the regularized upper incomplete gamma function.
// - P(chi-square > 63.68) = 9.99 x 10^-7 is the limit the SPEC allows (below 1 in 10^6 runs).
//   P(chi-square > 69.78) = 1.00 x 10^-7 is the stricter threshold ADR-0013 chose.
// - Computed on 2026-10-28 with a hand-written incomplete gamma function and cross-checked by numerical
//   integration of the chi-square density (ADR-0013). Recomputed on 2026-11-02 for issue #9 with the series
//   for the lower incomplete gamma function: 9.989 x 10^-7 and 1.0006 x 10^-7.
//
// What it catches: gross mistakes such as a wrong range or mapping. A "20" that comes up 6% of the time
// instead of 5% is detected practically always. Fine bias is not, and relies on the .NET generator instead.
//
// A failure is a signal to investigate, not to re-run: this test has no automatic retry.
public class DiceUniformityTests
{
    private const int Sides = 20;
    private const int RollCount = 200_000;
    private const double ExpectedCountPerFace = (double)RollCount / Sides;
    private const double FailureThreshold = 69.78;

    [Fact]
    public void D20_faces_are_uniform_by_a_chi_square_test()
    {
        var roller = new DiceRoller();

        var faces = roller.Roll(new DiceNeeded(Count: RollCount, Sides: Sides));

        var countPerFace = new int[Sides + 1];
        foreach (var face in faces)
        {
            countPerFace[face]++;
        }

        var chiSquare = 0.0;
        for (var face = 1; face <= Sides; face++)
        {
            var difference = countPerFace[face] - ExpectedCountPerFace;
            chiSquare += difference * difference / ExpectedCountPerFace;
        }

        Assert.True(
            chiSquare <= FailureThreshold,
            $"Chi-square {chiSquare:F2} is above {FailureThreshold}. A fair d20 does this once in 10^7 runs, so investigate, do not re-run.");
    }
}
