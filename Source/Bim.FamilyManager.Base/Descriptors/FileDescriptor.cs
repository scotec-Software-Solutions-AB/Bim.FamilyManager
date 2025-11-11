using Bim.FamilyManager.Abstractions.Descriptors;

namespace Bim.FamilyManager.Base.Descriptors;

public class FileDescriptor : ItemDescriptor, IFileDescriptor
{
    public string Version { get; set; } = string.Empty;
}
