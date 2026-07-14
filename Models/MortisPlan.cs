namespace DSOCompanion.Models;

public sealed class MortisPlan
{
    public int TargetBones { get; set; } = 99_999;
    public int CurrentBones { get; set; }
    public int DaysLeft { get; set; } = 7;
    public decimal HoursPerDay { get; set; } = 3;
    public decimal MinutesPerRun { get; set; } = 4;
    public List<MortisActivity> Activities { get; set; } = [];

    public int PlannedBones => Activities.Sum(x => x.TotalBones);
    public int FinalBones => CurrentBones + PlannedBones;
    public int MissingBones => Math.Max(0, TargetBones - FinalBones);
    public int TotalRuns => Activities.Sum(x => x.Runs);
    public int TotalEntries => Activities.Sum(x => x.Entries);
    public decimal TotalHours => TotalRuns * MinutesPerRun / 60m;
    public decimal TimeBudgetHours => DaysLeft * HoursPerDay;
    public decimal BonesPerDay => DaysLeft > 0 ? MissingBones / (decimal)DaysLeft : MissingBones;
}
