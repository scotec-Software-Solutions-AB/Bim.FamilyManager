using Bim.FamilyManager.Core.Abstractions.Descriptors;

namespace Bim.FamilyManager.Core.Descriptors;

public class FileDescriptor : ItemDescriptor, IFileDescriptor
{
    public string Version { get; set; } = string.Empty;
}
