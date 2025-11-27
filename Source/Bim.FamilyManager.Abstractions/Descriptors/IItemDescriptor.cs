namespace Bim.FamilyManager.Abstractions.Descriptors;

public interface IItemDescriptor
{
    IList<LocalizedName> LocalizedNames { get; set; }

    string ImagePath { get; set; }

    string GetName(string languageCode);
}
