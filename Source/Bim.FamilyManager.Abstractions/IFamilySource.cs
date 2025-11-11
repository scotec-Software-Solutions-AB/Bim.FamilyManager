using System.IO;

namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Represents a source of Revit families, providing access to its name, associated folders, and functionality
///     for reloading and saving families.
/// </summary>
public interface IFamilySource
{
    /// <summary>
    ///     Gets the name of the Revit family source.
    /// </summary>
    /// <remarks>
    ///     The name uniquely identifies the source of Revit families and can be used for display
    ///     or organizational purposes.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets the collection of folders associated with the Revit family source.
    /// </summary>
    /// <remarks>
    ///     Each folder represents a hierarchical structure that can contain subfolders and Revit families.
    ///     This property provides access to all top-level folders within the source.
    /// </remarks>
    IEnumerable<IFolder> Folders { get; }

    public Stream? Preview { get; }

    public event EventHandler<FamilySourceErrorEventArgs> Error;

    /// <summary>
    ///     Reloads the family source, ensuring that any changes to the underlying data or structure
    ///     are reflected in the current instance.
    /// </summary>
    /// <remarks>
    ///     This method is typically used to refresh the state of the family source, such as after
    ///     external modifications to the source data. Implementations may also trigger the
    ///     <see cref="Reloaded" /> event to notify subscribers of the reload operation.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown if the method is called on an instance that has already been disposed.
    /// </exception>
    void Reload();

    /// <summary>
    ///     Occurs when the family source has been reloaded.
    /// </summary>
    /// <remarks>
    ///     This event is triggered after the <see cref="Reload" /> method is called, indicating that
    ///     the state of the family source has been refreshed. Subscribers can use this event to
    ///     perform actions in response to the reload operation, such as updating UI elements or
    ///     reloading dependent data.
    /// </remarks>
    public event EventHandler<EventArgs> Reloaded;
    
    public string Type { get; }
}

public class FamilySourceErrorEventArgs
{
    public FamilySourceErrorEventArgs(bool hasError, IFamilySource familySource)
    {
        HasError = hasError;
        FamilySource = familySource;
    }

    public bool HasError { get; }
    
    public IFamilySource FamilySource { get; }
}
