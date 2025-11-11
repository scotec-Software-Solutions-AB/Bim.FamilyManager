using System.Diagnostics.CodeAnalysis;
using Scotec.Revit.RevitFamily;
using System.IO;

namespace Bim.FamilyManager.Abstractions;

/// <summary>
///     Represents a Revit family, which includes metadata, file information, preview image, and associated symbols.
/// </summary>
/// <remarks>
///     A Revit family is a collection of elements with a common set of properties, parameters, and graphical
///     representation.
///     This interface provides access to the family name, file path, preview image, and its symbols.
/// </remarks>
public interface IRevitFamily
{
    bool IsInitialized { get; }

    /// <summary>
    ///     Gets the name of the Revit family.
    /// </summary>
    /// <remarks>
    ///     The name typically corresponds to the file name of the Revit family without its extension.
    ///     It serves as a unique identifier for the family within the context of the application.
    /// </remarks>
    string Name { get; }

    /// <summary>
    ///     Gets a stream containing the preview image of the Revit family.
    /// </summary>
    /// <remarks>
    ///     The preview image provides a visual representation of the Revit family.
    ///     The stream returned is a copy of the original stream, ensuring the original data remains unaltered.
    /// </remarks>
    /// <returns>
    ///     A <see cref="Stream" /> containing the preview image of the Revit family.
    /// </returns>
    Stream? Preview { get; }

    /// <summary>
    ///     Gets the collection of family symbols (types) defined within the Revit family.
    /// </summary>
    /// <value>
    ///     A list of <see cref="IRevitFamilySymbol" /> instances representing the specific variations or types of the Revit
    ///     family.
    /// </value>
    /// <remarks>
    ///     Each element in the collection represents a distinct type within the Revit family, characterized by its own set of
    ///     parameters and properties.
    /// </remarks>
    IList<IRevitFamilySymbol> FamilySymbols { get; }

    /// <summary>
    ///     Gets the binary content of the Revit family file.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the raw binary data of the Revit family file.
    ///     It ensures that the family is fully initialized before returning the content.
    /// </remarks>
    Stream Family { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the Revit family is currently loaded in the active Revit document.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the family is loaded in the document; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property is used to track the state of the Revit family within the active document.
    ///     It is updated based on changes in the document and can be used to determine the availability
    ///     of the family for operations within the Revit environment.
    /// </remarks>
    bool IsLoadedInDocument { get; set; }

    /// <summary>
    ///     Initializes the <see cref="IRevitFamily" /> instance, ensuring it is ready for use.
    /// </summary>
    /// <remarks>
    ///     This method performs the necessary setup for the <see cref="IRevitFamily" /> instance.
    ///     If the instance is already initialized, the method returns immediately.
    ///     During initialization, the family data is loaded, and the <see cref="Initialized" /> event is triggered
    ///     upon successful completion. Any errors encountered during initialization are logged.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs during the initialization process. The error is logged, and the exception is rethrown.
    /// </exception>
    /// <example>
    ///     To initialize an <see cref="IRevitFamily" /> instance:
    ///     <code>
    /// var revitFamily = new RevitFamily("FamilyName", familyInfo, saveAction, logger);
    /// revitFamily.Initialize();
    /// </code>
    /// </example>
    void Initialize();

    /// <summary>
    ///     Occurs when the <see cref="IRevitFamily" /> instance has been successfully initialized.
    /// </summary>
    /// <remarks>
    ///     This event is triggered after the <see cref="Initialize" /> method completes successfully.
    ///     It indicates that the <see cref="IRevitFamily" /> instance is ready for use, with all necessary
    ///     data loaded and prepared. Subscribers to this event can perform additional actions or updates
    ///     in response to the initialization.
    /// </remarks>
    /// <example>
    ///     To handle the <see cref="Initialized" /> event:
    ///     <code>
    /// var revitFamily = new RevitFamily("FamilyName", familyInfo, saveAction, logger);
    /// revitFamily.Initialized += (sender, args) =>
    /// {
    ///     Console.WriteLine("Revit family has been initialized.");
    /// };
    /// revitFamily.Initialize();
    /// </code>
    /// </example>
    public event EventHandler? Initialized;

    /// <summary>
    ///     Occurs when the <see cref="IsLoadedInDocument" /> property of the <see cref="IRevitFamily" /> changes.
    /// </summary>
    /// <remarks>
    ///     This event is triggered whenever the state of the <see cref="IsLoadedInDocument" /> property is modified,
    ///     indicating whether the Revit family is currently loaded in a document.
    /// </remarks>
    /// <example>
    ///     To handle the <see cref="LoadedInDocumentChanged" /> event:
    ///     <code>
    /// var revitFamily = new RevitFamily("FamilyName", familyInfo, saveAction, logger);
    /// revitFamily.LoadedInDocumentChanged += (sender, args) =>
    /// {
    ///     Console.WriteLine("The loaded state of the family has changed.");
    /// };
    /// </code>
    /// </example>
    event EventHandler<EventArgs> LoadedInDocumentChanged;

    /// <summary>
    ///     Saves the current Revit family to the specified stream.
    /// </summary>
    /// <param name="stream">
    ///     The <see cref="Stream" /> to which the Revit family data will be written.
    /// </param>
    /// <remarks>
    ///     This method serializes the Revit family data and writes it to the provided stream.
    ///     It ensures that the family data is saved in a format suitable for later retrieval or use.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="stream" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the Revit family is not properly initialized or cannot be saved.
    /// </exception>
    void SaveToSource(Stream stream);

    /// <summary>
    /// Attempts to retrieve specific information associated with the Revit family by its name.
    /// </summary>
    /// <typeparam name="TInfo">
    /// The type of the information to retrieve. This must be a reference type.
    /// </typeparam>
    /// <param name="name">
    /// The name of the information to retrieve.
    /// </param>
    /// <param name="info">
    /// When this method returns, contains the information of type <typeparamref name="TInfo"/> if found; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the information was successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is useful for accessing metadata or other specific details related to the Revit family.
    /// </remarks>
    bool TryGetFamilyInfo<TInfo>(string name, [NotNullWhen(true)]out TInfo? info) where TInfo : class;

    /// <summary>
    /// Applies an update to the Revit family by replacing its current information with the provided
    /// <paramref name="familyInfo" />.
    /// </summary>
    /// <param name="familyInfo">
    /// The new <see cref="RevitFamilyInfo" /> containing updated data for the Revit family.
    /// </param>
    /// <remarks>
    /// This method resets the initialization state of the Revit family and clears the cached family symbols.
    /// After applying the update, the <see cref="Initialize" /> method is called to reinitialize the family with the new data.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="familyInfo" /> is <c>null</c>.
    /// </exception>
    void ApplyUpdate(RevitFamilyInfo familyInfo);

    public delegate IRevitFamily Factory(string name, RevitFamilyInfo familyInfo, Action<IRevitFamily, Stream> saveAction);

    public string Product { get; }
    public string ProductVersion { get; }
    public DateTime Updated { get; }

}
