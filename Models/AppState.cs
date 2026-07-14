namespace DSOCompanion.Models;

public sealed class AppState
{
    public List<CharacterProfile> Characters { get; set; } = [];
    public Guid? ActiveCharacterId { get; set; }
}
