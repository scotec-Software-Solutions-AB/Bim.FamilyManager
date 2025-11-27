using Bim.FamilyManager.Abstractions.Descriptors;

namespace Bim.FamilyManager.Base.Descriptors;

public abstract class ItemDescriptor : IItemDescriptor
{
    public IList<LocalizedName> LocalizedNames { get; set; } = new List<LocalizedName>();
    public string ImagePath { get; set; } = string.Empty;

    public string GetName(string languageCode)
    {
        return LocalizedNames.FirstOrDefault(n => n.Language == languageCode)?.Name
               ?? LocalizedNames.FirstOrDefault()?.Name
               ?? string.Empty;
    }
}
