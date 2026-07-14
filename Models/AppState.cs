namespace DSOCompanion.Models;

public sealed class AppState
{
    public List<CharacterProfile> Profiles { get; set; } = [];
    public Guid? ActiveProfileId { get; set; }
    public MortisPlan Mortis { get; set; } = new();
}
