using System.Diagnostics.CodeAnalysis;
using System.IO;
using Scotec.Revit.RevitFamily;

namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Defines the contract for a Revit family in Bim.FamilyManager.
/// </summary>
/// <remarks>
///     Provides access to metadata, file information, preview image, and associated symbols for a Revit family.
///     A Revit family is a collection of elements with shared properties, parameters, and graphical representation.
/// </remarks>
public interface IRevitFamily
{
    /// <summary>
    ///     Delegate for creating an <see cref="IRevitFamily" /> instance.
    /// </summary>
    /// <param name="name">The name of the Revit family.</param>
    /// <param name="familyInfo">The <see cref="RevitFamilyInfo" /> containing metadata and configuration.</param>
    /// <param name="saveAction">The action used to save the family to a stream.</param>
    /// <returns>An instance of <see cref="IRevitFamily" />.</returns>
    public delegate IRevitFamily Factory(string name, RevitFamilyInfo familyInfo, Action<IRevitFamily, Stream> saveAction);

    /// <summary>
    ///     Gets a value indicating whether the Revit family is initialized and ready for use.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    ///     Gets the name of the Revit family.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the family name, typically corresponding to the file name without extension.
    /// </value>
    string Name { get; }

    /// <summary>
    ///     Gets a stream containing the preview image of the Revit family.
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> containing the preview image, or <c>null</c> if unavailable.
    /// </value>
    /// <remarks>
    ///     The returned stream is a copy of the original to ensure data integrity.
    /// </remarks>
    Stream? Preview { get; }

    /// <summary>
    ///     Gets the collection of family symbols (types) defined within the Revit family.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IRevitFamilySymbol" /> instances representing the specific variations or types.
    /// </value>
    IList<IRevitFamilySymbol> FamilySymbols { get; }

    /// <summary>
    ///     Gets the binary content of the Revit family file.
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> containing the raw binary data of the family file.
    /// </value>
    Stream Family { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the Revit family is currently loaded in the active Revit document.
    /// </summary>
    /// <value>
    ///     <c>true</c> if loaded in the document; otherwise, <c>false</c>.
    /// </value>
    bool IsLoadedInDocument { get; set; }

    /// <summary>
    ///     Gets the product name associated with the Revit family.
    /// </summary>
    string Product { get; }

    /// <summary>
    ///     Gets the product version associated with the Revit family.
    /// </summary>
    string ProductVersion { get; }

    /// <summary>
    ///     Gets the date and time when the Revit family was last updated.
    /// </summary>
    DateTime Updated { get; }

    /// <summary>
    ///     Initializes the <see cref="IRevitFamily" /> instance.
    /// </summary>
    /// <remarks>
    ///     Loads family data and triggers the <see cref="Initialized" /> event upon successful completion.
    ///     If already initialized, returns immediately.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs during initialization.
    /// </exception>
    void Initialize();

    /// <summary>
    ///     Occurs when the <see cref="IRevitFamily" /> instance has been successfully initialized.
    /// </summary>
    /// <remarks>
    ///     Triggered after <see cref="Initialize" /> completes successfully.
    ///     Subscribers can perform actions in response to initialization.
    /// </remarks>
    public event EventHandler? Initialized;

    /// <summary>
    ///     Occurs when the <see cref="IsLoadedInDocument" /> property changes.
    /// </summary>
    /// <remarks>
    ///     Triggered whenever the loaded state of the family changes.
    /// </remarks>
    event EventHandler<EventArgs> LoadedInDocumentChanged;

    /// <summary>
    ///     Saves the current Revit family to the specified stream.
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to which the family data will be written.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="stream" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the family is not properly initialized or cannot be saved.
    /// </exception>
    void SaveToSource(Stream stream);

    /// <summary>
    ///     Attempts to retrieve specific information associated with the Revit family by its name.
    /// </summary>
    /// <typeparam name="TInfo">The type of information to retrieve (reference type).</typeparam>
    /// <param name="name">The name of the information to retrieve.</param>
    /// <param name="info">
    ///     When this method returns, contains the information of type <typeparamref name="TInfo" /> if found;
    ///     otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the information was successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetFamilyInfo<TInfo>(string name, [NotNullWhen(true)] out TInfo? info) where TInfo : class;

    /// <summary>
    ///     Applies an update to the Revit family by replacing its current information with the provided
    ///     <paramref name="familyInfo" />.
    /// </summary>
    /// <param name="familyInfo">The new <see cref="RevitFamilyInfo" /> containing updated data.</param>
    /// <remarks>
    ///     Resets the initialization state and clears cached symbols. Calls <see cref="Initialize" /> to reinitialize with new
    ///     data.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="familyInfo" /> is <c>null</c>.
    /// </exception>
    void ApplyUpdate(RevitFamilyInfo familyInfo);

    /// <summary>
    ///     Gets a stream containing the preview image for a specific family type (symbol) within the Revit family.
    /// </summary>
    /// <param name="typeName">
    ///     The name of the family type (symbol) for which to retrieve the preview image.
    /// </param>
    /// <returns>
    ///     A <see cref="Stream" /> containing the preview image for the specified type, or <c>null</c> if no preview is
    ///     available.
    /// </returns>
    /// <remarks>
    ///     The returned stream is positioned at the beginning and represents a copy of the preview image data for the
    ///     requested type.
    ///     If a type-specific preview is not available, a general family preview may be returned instead.
    ///     The caller is responsible for disposing the returned stream.
    /// </remarks>
    Stream? GetTypePreview(string typeName);
}
