namespace DSOCompanion.Models;

public sealed class LicensePayload
{
    public string LicenseId { get; set; } = "";
    public string Owner { get; set; } = "";
    public string HardwareId { get; set; } = "";
    public string LicenseType { get; set; } = "Lifetime";
    public DateTime IssuedUtc { get; set; }
    public DateTime? ExpiresUtc { get; set; }
}
