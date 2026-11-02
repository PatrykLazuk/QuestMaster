namespace QuestMaster.Rules;

// One die in a roll's result: the face it shows and whether it counts toward the total (D2.4).
public sealed record RolledDie(int Face, bool IsKept);
