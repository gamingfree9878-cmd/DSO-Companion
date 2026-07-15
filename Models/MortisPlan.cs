namespace DSOCompanion.Models;

public sealed class MortisPlan
{
    public int TargetBones { get; set; } = 100000;
    public int CurrentBones { get; set; }
    public int DaysLeft { get; set; } = 7;
    public decimal HoursPerDay { get; set; } = 3;
    public decimal MinutesPerRun { get; set; } = 4;
    public int AndermantPerEntry { get; set; }
    public int ElixirCostPerRun { get; set; }
    public int MortisCoins { get; set; }
    public List<MortisActivity> Activities { get; set; } = [];

    public int PlannedBones => Activities.Sum(x => x.TotalBones);
    public int FinalBones => CurrentBones + PlannedBones;
    public int MissingBones => Math.Max(0, TargetBones - FinalBones);
    public int ExcessBones => Math.Max(0, FinalBones - TargetBones);
    public int TotalRuns => Activities.Sum(x => x.Entries);
    public int TotalEntries => Activities.Sum(x => x.Entries);
    public decimal TotalHours => TotalRuns * MinutesPerRun / 60m;
    public decimal TimeBudgetHours => DaysLeft * HoursPerDay;
    public decimal BonesPerDay => DaysLeft > 0 ? MissingBones / (decimal)DaysLeft : MissingBones;
    public long AndermantCost => (long)TotalEntries * AndermantPerEntry;
    public long ElixirCost => (long)TotalRuns * ElixirCostPerRun;
}
