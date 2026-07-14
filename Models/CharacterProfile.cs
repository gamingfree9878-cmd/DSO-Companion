namespace DSOCompanion.Models;

public sealed class CharacterProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Mein Charakter";
    public string CharacterClass { get; set; } = "Zirkelmagier";
    public int Level { get; set; } = 100;
    public string Server { get; set; } = "Heredur";
    public List<BuildProfile> Builds { get; set; } = [];
    public Guid? ActiveBuildId { get; set; }
}
