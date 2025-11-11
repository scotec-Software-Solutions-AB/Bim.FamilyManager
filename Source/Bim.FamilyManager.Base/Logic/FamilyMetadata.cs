namespace Bim.FamilyManager.Base.Logic;

public class FamilyMetadata
{
    public string? Description { get; set; }

    public required Version Version { get; set; }

    public required DateTime LastModified { get; set; }

    public required string ModifiedBy { get; set; }
}
