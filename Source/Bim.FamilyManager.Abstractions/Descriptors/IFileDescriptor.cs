namespace Bim.FamilyManager.Abstractions.Descriptors;

public interface IFileDescriptor : IItemDescriptor
{
    string Version { get; set; }
}
