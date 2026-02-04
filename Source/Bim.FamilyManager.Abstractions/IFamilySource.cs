using System.IO;

namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Defines the contract for a source of Revit families in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides access to the source's name, associated folders, preview image, type, and events for error and reload
///     notifications.
///     Supports reloading the source to reflect changes and saving family data.
/// </remarks>
public interface IFamilySource
{
    /// <summary>
    ///     Gets the name of the Revit family source.
    /// </summary>
    /// <remarks>
    ///     Uniquely identifies the source and can be used for display or organizational purposes.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets a stream containing the preview image of the family source.
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> representing the preview image, or <c>null</c> if no preview is available.
    /// </value>
    Stream? Preview { get; }

    /// <summary>
    ///     Gets the type identifier of the family source.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the type of the source (e.g., "Directory", "Database", "AzureStorage").
    /// </value>
    string Type { get; }

    /// <summary>
    ///     Gets the collection of folders associated with the Revit family source.
    /// </summary>
    /// <value>
    ///     An <see cref="IAsyncEnumerable{IFolder}" /> representing all top-level folders within the source.
    /// </value>
    /// <remarks>
    ///     Each folder represents a hierarchical structure that can contain subfolders and Revit families.
    /// </remarks>
    IAsyncEnumerable<IFolder> GetFoldersAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Occurs when an error is encountered in the family source.
    /// </summary>
    /// <value>
    ///     An event providing <see cref="FamilySourceErrorEventArgs" /> with error details and the affected family source.
    /// </value>
    event EventHandler<FamilySourceErrorEventArgs> Error;

    /// <summary>
    ///     Reloads the family source, updating its state to reflect any changes to the underlying data or structure.
    /// </summary>
    /// <remarks>
    ///     Used to refresh the source after external modifications. May trigger the <see cref="Reloaded" /> event.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown if called on a disposed instance.
    /// </exception>
    void Reload();

    /// <summary>
    ///     Occurs when the family source has been reloaded.
    /// </summary>
    /// <remarks>
    ///     Triggered after <see cref="Reload" /> is called, indicating the source has been refreshed.
    ///     Subscribers can update UI or reload dependent data in response.
    /// </remarks>
    event EventHandler<EventArgs> Reloaded;
}

/// <summary>
///     Provides error details for family source operations in Bim.FamilyManager.
/// </summary>
public class FamilySourceErrorEventArgs
{
    /// <summary>
    ///     Initializes a new instance of <see cref="FamilySourceErrorEventArgs" />.
    /// </summary>
    /// <param name="hasError">Indicates whether an error occurred.</param>
    /// <param name="familySource">The family source where the error occurred.</param>
    public FamilySourceErrorEventArgs(bool hasError, IFamilySource familySource)
    {
        HasError = hasError;
        FamilySource = familySource;
    }

    /// <summary>
    ///     Gets a value indicating whether an error occurred.
    /// </summary>
    public bool HasError { get; }

    /// <summary>
    ///     Gets the family source associated with the error.
    /// </summary>
    public IFamilySource FamilySource { get; }
}
