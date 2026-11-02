namespace QuestMaster.Rules;

// The dice a planned roll needs, such as 1 × d20. The server rolls exactly these
// and passes the faces back to the plan to resolve (ADR-0003).
public sealed record DiceNeeded(int Count, int Sides);
