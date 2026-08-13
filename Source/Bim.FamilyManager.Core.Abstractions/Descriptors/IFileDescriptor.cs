namespace Bim.FamilyManager.Core.Abstractions.Descriptors;

public interface IFileDescriptor : IItemDescriptor
{
    string Version { get; set; }
}
