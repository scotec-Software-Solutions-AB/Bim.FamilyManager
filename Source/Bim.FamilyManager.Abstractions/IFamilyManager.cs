using System.Diagnostics.CodeAnalysis;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Represents an interface for managing Revit families and their associated operations.
/// </summary>
/// <remarks>
///     This interface provides methods and properties to manage Revit families, including searching,
///     editing, registering, and retrieving families. It also supports reloading family data and
///     notifying subscribers when the reload operation is completed.
/// </remarks>
public interface IFamilyManager
{
    /// <summary>
    ///     Gets a collection of family sources that provide access to Revit families.
    /// </summary>
    /// <remarks>
    ///     Each family source represents a repository or location containing Revit families.
    ///     This property allows enumeration of all available family sources, enabling operations
    ///     such as searching, editing, or reloading families within these sources.
    /// </remarks>
    IEnumerable<IFamilySource> FamilySources { get; }

    Task ReloadAsync();

    /// <summary>
    ///     Occurs when the family manager has completed reloading its data.
    /// </summary>
    /// <remarks>
    ///     This event is triggered after the <see cref="Reload" /> method is called and the reloading process is finished.
    ///     Subscribers can use this event to perform actions that depend on the updated family data.
    /// </remarks>
    event EventHandler<EventArgs> Reloaded;

    /// <summary>
    ///     Asynchronously searches for Revit families within the specified folder that match the given search pattern.
    /// </summary>
    /// <param name="folder">
    ///     The folder in which to search for Revit families. This folder can include subfolders and families.
    /// </param>
    /// <param name="searchPattern">
    ///     The search pattern used to filter Revit families. This can include wildcards or specific naming criteria.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of
    ///     <see cref="IRevitFamily" /> objects that match the search criteria within the specified folder.
    /// </returns>
    /// <remarks>
    ///     This method allows for efficient searching of Revit families based on a folder hierarchy and a search pattern.
    ///     It is commonly used to locate specific families in large datasets.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="folder" /> or <paramref name="searchPattern" /> is <c>null</c>.
    /// </exception>
    IAsyncEnumerable<IRevitFamily> SearchRevitFamiliesAsync(IFolder folder, string searchPattern, CancellationToken cancellationToken);

    /// <summary>
    ///     Opens the specified Revit family for editing.
    /// </summary>
    /// <param name="revitFamily">
    ///     The <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" /> instance representing the Revit family to
    ///     be edited.
    /// </param>
    /// <remarks>
    ///     This method ensures that the provided Revit family is initialized before attempting to open it for editing.
    ///     If the family is not already open in the Revit application, it will be loaded and activated.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when the <paramref name="revitFamily" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the specified Revit family is not initialized.
    /// </exception>
    void EditFamily(IRevitFamily revitFamily);

    /// <summary>
    ///     Registers a Revit family with the family manager.
    /// </summary>
    /// <param name="family">
    ///     The <see cref="IRevitFamily" /> instance representing the Revit family to be registered.
    /// </param>
    /// <remarks>
    ///     This method adds or updates the specified Revit family in the internal cache.
    ///     If a family with the same name already exists, it will be updated with the provided instance.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="family" /> parameter is <c>null</c>.
    /// </exception>
    void RegisterRevitFamily(IRevitFamily family);

    /// <summary>
    ///     Attempts to retrieve a Revit family by its name.
    /// </summary>
    /// <param name="familyName">
    ///     The name of the Revit family to retrieve. This parameter is case-sensitive.
    /// </param>
    /// <param name="family">
    ///     When this method returns, contains the <see cref="IRevitFamily" /> instance if found; otherwise, <c>null</c>.
    ///     This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the Revit family with the specified name exists and was successfully retrieved; otherwise,
    ///     <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method is useful for checking the existence of a Revit family and retrieving its details in a single
    ///     operation.
    /// </remarks>
    bool TryGetRevitFamily(string familyName, [MaybeNullWhen(false)] out IRevitFamily family);

    /// <summary>
    ///     Loads a Revit family into the specified document.
    /// </summary>
    /// <param name="revitFamily">
    ///     The <see cref="IRevitFamily" /> instance representing the family to be loaded.
    /// </param>
    /// <param name="document">
    ///     The <see cref="Document" /> instance representing the active Revit document where the family will be loaded.
    /// </param>
    /// <param name="family"></param>
    /// <returns>
    ///     The loaded <see cref="IRevitFamily" /> instance from the Revit document.
    /// </returns>
    /// <remarks>
    ///     This method ensures that the specified Revit family is loaded into the provided document.
    ///     If the family is already loaded, it may return the existing instance.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="revitFamily" /> or <paramref name="document" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the family cannot be loaded into the document due to an invalid state or other constraints.
    /// </exception>
    bool TryLoadFamily(IRevitFamily revitFamily, Document document, [NotNullWhen(true)] out Family? family);

    bool TryLoadFamilySymbol(IRevitFamilySymbol revitFamilySymbol, Document document, [NotNullWhen(true)] out FamilySymbol? familySymbol);

    /// <summary>
    ///     Attempts to load the specified Revit family into the currently active document.
    /// </summary>
    /// <param name="revitFamily">
    ///     The <see cref="IRevitFamily" /> instance representing the Revit family to be loaded.
    /// </param>
    /// <param name="family">
    ///     When this method returns, contains the loaded <see cref="Autodesk.Revit.DB.Family" /> instance,
    ///     if the operation was successful; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the Revit family was successfully loaded into the active document; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method assumes that there is an active document available in the current context.
    ///     If no active document is present, the method may throw an exception.
    /// </remarks>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if there is no active document available.
    /// </exception>
    bool TryLoadFamilyIntoActiveDocument(IRevitFamily revitFamily, [NotNullWhen(true)] out Family? family);

    /// <summary>
    ///     Removes the specified Revit family from the active document.
    /// </summary>
    /// <param name="family">
    ///     The <see cref="IRevitFamily" /> instance representing the Revit family to be removed.
    /// </param>
    /// <remarks>
    ///     This method is used to remove a loaded Revit family from the currently active document in Revit.
    ///     Ensure that the specified family is currently loaded in the active document before calling this method.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when the <paramref name="family" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the specified family is not loaded in the active document.
    /// </exception>
    void RemoveFamilyFromActiveDocument(IRevitFamily family);

    void CreatePreviewImage(UIApplication application, View view);
}
