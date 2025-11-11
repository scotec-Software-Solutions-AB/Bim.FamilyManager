namespace Bim.FamilyManager.Abstractions.Descriptors;

public class LocalizedName
{
    public string Language { get; set; } = string.Empty; // e.g. "en-US"
    public string Name { get; set; } = string.Empty;
}
