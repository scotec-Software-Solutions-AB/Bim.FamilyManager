using System.IO;
using System.Text.RegularExpressions;
using Autodesk.Internal.InfoCenter;
using Autodesk.Windows;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Base.Logic;
using Bim.FamilyManager.Source.Directory.Options;
using Bim.FamilyManager.Ui.Resources;
using Microsoft.Extensions.Logging;
using Scotec.Revit.RevitFamily;
using Folder = Bim.FamilyManager.Base.Logic.Folder;

namespace Bim.FamilyManager.Source.Directory.Logic;

/// <summary>
///     Represents a source for managing Revit families stored in a directory structure.
/// </summary>
/// <remarks>
///     This class provides functionality to load and manage Revit families from a specified directory.
///     It extends the <see cref="FamilySource{TOptions}" /> class
///     and uses <see cref="DirectorySourceOptions" /> to configure the directory path.
/// </remarks>
public sealed class DirectorySource : FamilySource<DirectorySourceOptions>
{
    /// <summary>
    ///     A factory delegate for creating instances of <see cref="DirectorySource" />.
    /// </summary>
    /// <param name="options">
    ///     The <see cref="DirectorySourceOptions" /> used to configure the created instance.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="DirectorySource" /> configured with the specified options.
    /// </returns>
    public delegate DirectorySource Factory(DirectorySourceOptions options);

    private static readonly Regex BackupRegex = new(@"\.\d{4}\.rfa$", RegexOptions.Compiled);
    private static readonly Stream PreviewStream;
    private readonly ILogger<DirectorySource> _logger;
    private readonly string _rootPath;
    private IEnumerable<IFolder>? _folders;

    /// <summary>
    ///     Initializes static members of the <see cref="DirectorySource" /> class.
    /// </summary>
    /// <remarks>
    ///     This static constructor is responsible for initializing the <see cref="PreviewStream" /> field
    ///     by loading a resource stream from a predefined URI.
    /// </remarks>
    static DirectorySource()
    {
        const string packUri =
            "pack://application:,,,/Bim.FamilyManager.Source.Directory;component/Resources/Images/FamilySourceStorageNetwork_128x128.png";

        PreviewStream = LoadResourceAsStream(packUri);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Bim.FamilyManager.Source.Directory.Logic.DirectorySource" />
    ///     class.
    /// </summary>
    /// <param name="options">
    ///     The <see cref="Bim.FamilyManager.Source.Directory.Options.DirectorySourceOptions" /> that specifies the
    ///     configuration for the directory source.
    /// </param>
    /// <param name="familyManager">
    ///     An implementation of the <see cref="Bim.FamilyManager.Abstractions.IFamilyManager" /> interface used to
    ///     manage Revit families.
    /// </param>
    /// <param name="familyFactory">
    ///     A factory delegate for creating instances of <see cref="Bim.FamilyManager.Base.Logic.RevitFamily" />.
    /// </param>
    /// <param name="logger">
    ///     An instance of
    ///     <see cref="Microsoft.Extensions.Logging.ILogger{Bim.FamilyManager.Source.Directory.Logic.DirectorySource}" />
    ///     used for logging.
    /// </param>
    /// <remarks>
    ///     This constructor initializes the directory source with the specified options, family manager, and family factory.
    ///     It also sets the root path for the directory source based on the provided options.
    /// </remarks>
    public DirectorySource(DirectorySourceOptions options, IFamilyManager familyManager, IRevitFamily.Factory familyFactory,
                           ILogger<DirectorySource> logger)
        : base(options, familyManager, familyFactory)
    {
        _logger = logger;
        _rootPath = Options.Path;
    }

    /// <summary>
    ///     Gets the collection of folders representing the directory structure for Revit families.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{T}" /> of <see cref="IFolder" /> representing the folders
    ///     containing Revit families.
    /// </value>
    /// <remarks>
    ///     This property lazily initializes the collection of folders by scanning the root directory
    ///     specified during the creation of the <see cref="DirectorySource" /> instance. The folders
    ///     are retrieved using the <c>GetFolders</c> method.
    /// </remarks>
    public override IEnumerable<IFolder> Folders => _folders ??= GetFolders(_rootPath);

    /// <summary>
    ///     Gets a stream that provides a preview of the Revit family data.
    /// </summary>
    /// <remarks>
    ///     This property overrides <see cref="FamilySource{TOptions}.Preview" /> to return a stream
    ///     positioned at the beginning, allowing for reading the preview data from the start.
    /// </remarks>
    /// <returns>
    ///     A <see cref="Stream" /> object containing the preview data for the Revit family.
    /// </returns>
    public override Stream Preview
    {
        get
        {
            PreviewStream.Position = 0;
            return PreviewStream;
        }
    }

    /// <summary>
    ///     Releases the resources used by the <see cref="DirectorySource" /> instance.
    /// </summary>
    /// <param name="disposing">
    ///     A boolean value indicating whether the method is being called explicitly
    ///     (true) or by the garbage collector (false).
    /// </param>
    /// <remarks>
    ///     This method overrides <see cref="FamilySource{TOptions}.Dispose(bool)" /> to ensure that
    ///     any resources specific to the <see cref="DirectorySource" /> class are properly released.
    ///     When <paramref name="disposing" /> is true, it releases both managed and unmanaged resources.
    ///     When false, it releases only unmanaged resources.
    /// </remarks>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }

    /// <summary>
    ///     Handles the reload operation for the <see cref="DirectorySource" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method overrides <see cref="FamilySource{TOptions}.OnReload" /> to reset the internal folder structure,
    ///     ensuring that all folders are reinitialized during the next operation.
    /// </remarks>
    protected override void OnReload()
    {
        // Reset all folders to force reinitialization.
        _folders = null;
    }

    /// <summary>
    ///     Retrieves the collection of folders within the specified directory.
    /// </summary>
    /// <param name="directory">
    ///     The path of the directory to search for subfolders.
    /// </param>
    /// <returns>
    ///     An enumerable collection of <see cref="Bim.FamilyManager.Abstractions.IFolder" /> objects
    ///     representing the subfolders within the specified directory.
    /// </returns>
    /// <remarks>
    ///     This method searches the given directory for subdirectories and creates instances of
    ///     <see cref="Base.Logic.Folder" /> for each subdirectory. Each folder includes
    ///     lazy-loaded collections of its subfolders and Revit families.
    /// </remarks>
    /// <exception cref="System.IO.DirectoryNotFoundException">
    ///     Thrown if the specified directory does not exist.
    /// </exception>
    /// <exception cref="System.UnauthorizedAccessException">
    ///     Thrown if the application does not have the required permissions to access the directory.
    /// </exception>
    private IEnumerable<IFolder> GetFolders(string directory)
    {
        return System.IO.Directory.GetDirectories(directory, "", SearchOption.TopDirectoryOnly)
                     .Select(IFolder (d) => new Folder(Path.GetFileName(d), () => GetFolders(d), () => GetFamilies(d)));
    }

    /// <summary>
    ///     Saves the specified Revit family to the provided file path using the given stream.
    /// </summary>
    /// <param name="family">The <see cref="RevitFamily" /> instance representing the family to be saved.</param>
    /// <param name="stream">The <see cref="Stream" /> containing the family data to be written to the file.</param>
    /// <param name="path">The file path where the family should be saved.</param>
    /// <remarks>
    ///     This method ensures that a backup of the existing file is created before overwriting it.
    ///     It uses <see cref="FileBackupHelper" /> to check if the folder is writable and to create the backup.
    ///     After saving, the method updates the family with the new family information.
    /// </remarks>
    private void SaveFamily(IRevitFamily family, Stream stream, string path)
    {
        _logger.LogInformation($"Save family to the family source. Source: {Name}, Family: {family.Name}, Path: {path}");

        var notification = string.Empty;
        try
        {
            if (!FileBackupHelper.CanWriteToFolder(path))
            {
                notification = StringResources.DirectorySource_Message_Save_Error;

                _logger.LogWarning("Failed to save the family to the specified family source. The current user does not have the necessary access rights.");
                return;
            }

            FileBackupHelper.CreateBackup(path);
            using (var file = File.OpenWrite(path))
            {
                stream.CopyTo(file);
            }

            family.ApplyUpdate(CreateFamilyInfo(path));
            notification = StringResources.DirectorySource_Message_Save_Success;

            _logger.LogInformation("The family was successfully saved to the family source.");
        }
        catch (Exception e)
        {
            notification = StringResources.DirectorySource_Message_Save_Error;

            _logger.LogError(e, "Failed to save the family to the specified family source.");
        }
        finally
        {
            var result = new ResultItem
            {
                Title = notification,
                Category = StringResources.FamilyManager_Name,
                IsNew = true,
                Timestamp = DateTime.Now,
                Type = ResultType.Unknown
            };

            ComponentManager.InfoCenterPaletteManager.ShowBalloon(result);
        }
    }

    /// <summary>
    ///     Retrieves a list of Revit families from the specified directory.
    /// </summary>
    /// <param name="directory">The directory path to search for Revit family files.</param>
    /// <returns>
    ///     A list of <see cref="IRevitFamily" /> objects representing the Revit families found in the directory.
    /// </returns>
    /// <remarks>
    ///     This method searches for Revit family files (*.rfa) in the specified directory, excluding backup files
    ///     that match the predefined backup file naming pattern. If a family is already managed by the
    ///     <see cref="Bim.FamilyManager.Abstractions.IFamilyManager" />, it is reused; otherwise, a new
    ///     family is created and initialized.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="directory" /> is null.
    /// </exception>
    /// <exception cref="System.IO.DirectoryNotFoundException">
    ///     Thrown if the specified <paramref name="directory" /> does not exist.
    /// </exception>
    private List<IRevitFamily> GetFamilies(string directory)
    {
        var familyFiles = System.IO.Directory.GetFiles(directory, "*.rfa", SearchOption.TopDirectoryOnly)
                                .Where(filePath => !BackupRegex.IsMatch(filePath))
                                .Select(filePath => filePath)
                                .ToList();
        var descriptionFiles = System.IO.Directory.GetFiles(directory, "*.yaml", SearchOption.TopDirectoryOnly);

        var sets = GroupFamiliesAndDescriptions(familyFiles, descriptionFiles);

        var families = sets.Select(set =>
                           {
                               var familyName = set.Name;
                               if (FamilyManager.TryGetRevitFamily(familyName, out var family))
                               {
                                   return family;
                               }

                               return CreateRevitFamily(familyName, CreateFamilyInfo(set.FamilyFile),
                                   (revitFamily, stream) => SaveFamily(revitFamily, stream, set.FamilyFile));
                           })
                           .ToList();

        return families;
    }

    /// <summary>
    ///     Creates a new instance of a Revit family and registers it with the family manager.
    /// </summary>
    /// <param name="familyName">The name of the Revit family to be created.</param>
    /// <param name="familyInfo">The metadata and information associated with the Revit family.</param>
    /// <param name="saveAction">
    ///     An action that defines how the Revit family should be saved, taking the Revit family instance
    ///     and a stream as parameters.
    /// </param>
    /// <returns>
    ///     An instance of <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> representing the created
    ///     Revit family.
    /// </returns>
    /// <remarks>
    ///     This method utilizes the family factory to create a new Revit family instance based on the provided
    ///     name and metadata. The created family is then registered with the family manager to ensure it is
    ///     tracked and managed appropriately.
    /// </remarks>
    private IRevitFamily CreateRevitFamily(string familyName, RevitFamilyInfo familyInfo, Action<IRevitFamily, Stream> saveAction)
    {
        var family = FamilyFactory(familyName, familyInfo, saveAction);
        FamilyManager.RegisterRevitFamily(family);

        return family;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="RevitFamilyInfo" /> for the specified file path.
    /// </summary>
    /// <param name="path">The file path of the Revit family file.</param>
    /// <returns>A <see cref="RevitFamilyInfo" /> object containing metadata and stream-loading logic for the specified file.</returns>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when the <paramref name="path" /> is null, empty, or consists only of
    ///     white-space characters.
    /// </exception>
    /// <exception cref="System.IO.FileNotFoundException">
    ///     Thrown when the file specified by <paramref name="path" /> does not
    ///     exist.
    /// </exception>
    /// <remarks>
    ///     This method validates the provided file path, ensures the file exists, and creates a <see cref="RevitFamilyInfo" />
    ///     instance.
    ///     The <see cref="RevitFamilyInfo" /> object includes a delegate for loading the file stream, which reads the file
    ///     into memory
    ///     and returns a <see cref="MemoryStream" /> for further processing.
    /// </remarks>
    private RevitFamilyInfo CreateFamilyInfo(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(@"Path cannot be null or empty.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The specified file was not found: {path}");
        }

        // RevitFamilyInfo attempts to retrieve additional information from the "BIM.FamilyManager" storage, if it is present within the family file.
        var familyInfo = CreateFamilyInfo(LoadFileStream);

        return familyInfo;

        Stream LoadFileStream()
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);

            memoryStream.Position = 0;
            return memoryStream;
        }
    }

    /// <summary>
    /// Groups Revit family files and their corresponding description files into a list of tuples.
    /// </summary>
    /// <param name="familyFiles">
    /// A collection of file paths representing Revit family files (*.rfa).
    /// </param>
    /// <param name="descriptionFiles">
    /// A collection of file paths representing description files (*.yaml).
    /// </param>
    /// <returns>
    /// A list of tuples where each tuple contains the name of the family, the path to the family file,
    /// and the path to the corresponding description file (if available).
    /// </returns>
    /// <remarks>
    /// This method matches Revit family files with their corresponding description files based on
    /// their filenames (excluding extensions). If a matching description file is not found for a
    /// family file, the description file path in the tuple will be <c>null</c>.
    /// </remarks>
    private static List<(string Name, string FamilyFile, string? DescriptionFile)> GroupFamiliesAndDescriptions(
        IEnumerable<string> familyFiles, IEnumerable<string> descriptionFiles)
    {
        // Build a lookup for .fm files by filename without extension
        var fmLookup = descriptionFiles
            .ToDictionary(
                fm => Path.GetFileNameWithoutExtension(fm)!,
                fm => fm,
                StringComparer.OrdinalIgnoreCase);

        var set = familyFiles
                  .Select(rfa =>
                  {
                      var name = Path.GetFileNameWithoutExtension(rfa);
                      fmLookup.TryGetValue(name, out var fmFile);
                      return (Name: name, FamilyFile: rfa, DescriptionFile: fmFile);
                  })
                  .ToList();

        return set;
    }
}
