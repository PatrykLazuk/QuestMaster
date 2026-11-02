namespace QuestMaster.Rules;

// The number added to a d20 roll, derived from an ability score (SRD p.6). It can be negative.
public sealed record Modifier(int Value);
