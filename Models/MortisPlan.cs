namespace DSOCompanion.Models;

public sealed class MortisPlan
{
    public int TargetBones { get; set; } = 100000;
    public int CurrentBones { get; set; }

    public int AndermantPerEntry { get; set; }
    public int MortisCoins { get; set; }

    // Mortis-Knochenelixier: Ein Buff hält immer 15 Minuten.
    // Der Nutzer trägt ein, wie viele Mortis-Runs geplant sind
    // und wie viele Runs er durchschnittlich in einem Elixier schafft.
    public int ElixirPlannedRuns { get; set; }
    public int RunsPerElixir { get; set; } = 3;

    public List<MortisActivity> Activities { get; set; } = [];

    public int PlannedBones => Activities.Sum(x => x.TotalBones);
    public int FinalBones => CurrentBones + PlannedBones;
    public int MissingBones => Math.Max(0, TargetBones - FinalBones);
    public int ExcessBones => Math.Max(0, FinalBones - TargetBones);
    public int TotalRuns => Activities.Sum(x => x.Runs);
    public int TotalEntries => Activities.Sum(x => x.Entries);
    public long AndermantCost => (long)TotalEntries * AndermantPerEntry;

    public int RequiredElixirs =>
        ElixirPlannedRuns <= 0
            ? 0
            : RunsPerElixir <= 0
                ? ElixirPlannedRuns
                : (int)Math.Ceiling(ElixirPlannedRuns / (decimal)RunsPerElixir);
}
