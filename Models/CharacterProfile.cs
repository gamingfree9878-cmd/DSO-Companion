namespace DSOCompanion.Models;

public sealed class CharacterProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Mein Charakter";
    public string CharacterClass { get; set; } = "Zirkelmagier";
    public int Level { get; set; } = 100;
    public string Server { get; set; } = "Heredur";
    public string Notes { get; set; } = "";
    public List<BuildProfile> Builds { get; set; } = [];
    public Guid? ActiveBuildId { get; set; }

    public int OwnedGemDust { get; set; }
    public int OwnedGold { get; set; }
    public List<GemCollection> Gems { get; set; } = [];

    public int OwnedRuneDust { get; set; }
    public int OwnedRuneGold { get; set; }
    public List<RuneCollection> Runes { get; set; } = [];
    public MortisPlan Mortis { get; set; } = new();
}
