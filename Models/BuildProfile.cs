namespace DSOCompanion.Models;

public sealed class BuildProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Boss-Build";
    public List<EquipmentSlot> Equipment { get; set; } = [];
}
