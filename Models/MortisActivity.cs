namespace DSOCompanion.Models;

public sealed class MortisActivity
{
    public string Name { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public int BonesPerRun { get; set; }
    public int Entries { get; set; }

    public int TotalBones => BonesPerRun * Entries;
    public string DisplayName =>
        Difficulty == "-" || string.IsNullOrWhiteSpace(Difficulty)
            ? Name
            : $"{Name}\n{Difficulty}";
}
