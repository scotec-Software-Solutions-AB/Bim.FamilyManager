using System.IO;
using System.Windows;
using Bim.FamilyManager.Abstractions;

namespace Bim.FamilyManager.Base.Logic;

/// <summary>
///     Represents a folder in the Revit Family Manager hierarchy.
/// </summary>
/// <remarks>
///     A folder can contain subfolders and Revit families. It provides access to its name, path,
///     and collections of subfolders and families. This class supports lazy-loading for efficient
///     retrieval of hierarchical data.
/// </remarks>
public class Folder : IFolder
{
    private static readonly Stream PreviewStream;
    private readonly Func<IEnumerable<IRevitFamily>> _families;
    private readonly Func<IEnumerable<IFolder>> _subFolders;

    /// <summary>
    ///     Initializes static members of the <see cref="Folder" /> class.
    /// </summary>
    /// <remarks>
    ///     This static constructor loads the preview image resource for the <see cref="Folder" /> class.
    ///     The preview image is used to visually represent a folder in the Revit Family Manager hierarchy.
    /// </remarks>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when the resource URI is null or empty.
    /// </exception>
    /// <exception cref="System.IO.FileNotFoundException">
    ///     Thrown when the specified resource cannot be found.
    /// </exception>
    static Folder()
    {
        const string packUri = "pack://application:,,,/Bim.FamilyManager.Base;component/Resources/Images/Folder_128x128.png";

        PreviewStream = LoadResourceAsStream(packUri);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Folder" /> class.
    /// </summary>
    /// <param name="name">
    ///     The name of the folder.
    /// </param>
    /// <param name="subFolders">
    ///     A function to lazily load the subfolders of this folder.
    /// </param>
    /// <param name="families">
    ///     A function to lazily load the Revit families contained in this folder.
    /// </param>
    /// <remarks>
    ///     This constructor enables the creation of a folder instance with lazy-loading capabilities,
    ///     allowing efficient access to hierarchical data such as subfolders and families.
    /// </remarks>
    public Folder(string name, Func<IEnumerable<IFolder>> subFolders, Func<IEnumerable<IRevitFamily>> families)
    {
        _subFolders = subFolders;
        _families = families;
        Name = name;
    }

    /// <summary>
    ///     Gets the name of the folder.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the folder.
    /// </value>
    public string Name { get; }

    /// <summary>
    ///     Gets the collection of subfolders contained within this folder.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{IFolder}" /> representing the subfolders.
    /// </value>
    public IEnumerable<IFolder> Subfolders => _subFolders();

    /// <summary>
    ///     Gets the collection of Revit families contained within this folder.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{IRevitFamily}" /> representing the Revit families.
    /// </value>
    public IEnumerable<IRevitFamily> Families => _families();

    /// <summary>
    ///     Gets the preview image stream associated with this folder.
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> containing the preview image for the folder.
    /// </value>
    /// <remarks>
    ///     The preview image provides a visual representation of the folder in the UI.
    ///     The stream's position is reset to 0 on each access.
    /// </remarks>
    public Stream Preview
    {
        get
        {
            PreviewStream.Position = 0;
            return PreviewStream;
        }
    }

    /// <summary>
    ///     Loads a resource as a <see cref="Stream" /> from the specified pack URI.
    /// </summary>
    /// <param name="packUri">
    ///     The pack URI of the resource to be loaded. This must be a valid, non-empty URI string.
    /// </param>
    /// <returns>
    ///     A <see cref="Stream" /> containing the resource data.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when the <paramref name="packUri" /> is null or empty.
    /// </exception>
    /// <exception cref="System.IO.FileNotFoundException">
    ///     Thrown when the resource specified by the <paramref name="packUri" /> cannot be found.
    /// </exception>
    /// <remarks>
    ///     This method retrieves a resource from the application's resources using the provided pack URI.
    ///     The resource is read into a <see cref="MemoryStream" /> to allow for efficient and reusable access.
    /// </remarks>
    private static Stream LoadResourceAsStream(string packUri)
    {
        // TODO: Move this method to helper class.
        if (string.IsNullOrEmpty(packUri))
        {
            throw new ArgumentException("Pack URI cannot be null or empty.", nameof(packUri));
        }

        // Get the resource stream
        var resourceInfo = Application.GetResourceStream(new Uri(packUri, UriKind.RelativeOrAbsolute));
        if (resourceInfo == null)
        {
            throw new FileNotFoundException($"Resource not found at {packUri}");
        }

        using var resourceStream = resourceInfo.Stream;
        var memoryStream = new MemoryStream();
        resourceStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }
}
