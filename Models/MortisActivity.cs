namespace DSOCompanion.Models;

public sealed class MortisActivity
{
    public string Name { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public int BonesPerRun { get; set; }
    public int Entries { get; set; }
    public int Runs { get; set; }

    public int TotalBones => BonesPerRun * Runs;
}
