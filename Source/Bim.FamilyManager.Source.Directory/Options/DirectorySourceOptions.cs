using Bim.FamilyManager.Base.Options;
using Bim.FamilyManager.Source.Directory.Logic;

namespace Bim.FamilyManager.Source.Directory.Options;

/// <summary>
///     Represents the configuration options for a directory-based family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This class extends <see cref="FamilySourceOptions" /> to provide
///     specific properties and functionality for managing families stored in a directory structure.
/// </remarks>
[FamilySourceOptions(OptionsName = "DirectorySource")]
public class DirectorySourceOptions : FamilySourceOptions
{
    /// <summary>
    ///     Gets or sets the file system path to the directory containing the Revit families.
    /// </summary>
    /// <remarks>
    ///     This property specifies the root directory from which Revit families are loaded.
    ///     It is used by the <see cref="DirectorySource" />
    ///     to locate and manage families stored in a directory structure.
    /// </remarks>
    public string Path { get; set; } = string.Empty;
}
