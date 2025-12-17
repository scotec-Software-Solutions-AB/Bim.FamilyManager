using System.IO;
using System.Windows;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.Options;
using Bim.FamilyManager.Base.Options;
using Scotec.Revit.RevitFamily;

namespace Bim.FamilyManager.Base.Logic;

/// <summary>
///     Represents an abstract base class for managing Revit family sources with specific options.
/// </summary>
/// <typeparam name="TOptions">
///     The type of options used to configure the family source. Must inherit from <see cref="FamilySourceOptions" />.
/// </typeparam>
/// <remarks>
///     This class provides foundational functionality for managing Revit family sources, including
///     loading, saving, and reloading families. It also supports event handling for reload operations
///     and implements the <see cref="IDisposable" /> interface for resource management.
///     Derived classes must define a delegate factory in the following form:
///     <code>public delegate DerivedSource Factory(DerivedSourceOptions options);</code>
///     While there are no direct references to the delegate, the FamilyManager accesses it using reflection.
/// </remarks>
public abstract class FamilySource<TOptions> : IFamilySource, IDisposable where TOptions : IFamilySourceOptions
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySource{TOptions}" /> class with the specified family manager,
    ///     options, and family factory.
    /// </summary>
    /// <param name="familyManager">
    ///     The <see cref="IFamilyManager" /> instance responsible for managing Revit families.
    /// </param>
    /// <param name="options">
    ///     The options of type <typeparamref name="TOptions" /> used to configure the family source.
    /// </param>
    /// <param name="familyFactory">
    ///     A factory delegate for creating instances of <see cref="RevitFamily" />.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the family source by associating it with a family manager, applying the provided options,
    ///     and initializing the family factory. It also assigns the name of the family source based on the provided options.
    /// </remarks>
    protected FamilySource(TOptions options, IFamilyManager familyManager, IRevitFamily.Factory familyFactory)
    {
        Options = options;
        FamilyManager = familyManager;
        FamilyFactory = familyFactory;

        Name = options.Name;
    }

    /// <summary>
    ///     Gets the error message associated with the most recent error event.
    /// </summary>
    /// <value>
    ///     A string containing the error message, or <c>null</c> if no error has occurred.
    /// </value>
    /// <remarks>
    ///     This property is set when an error is raised using the <see cref="RaiseError(Exception)" /> or
    ///     <see cref="RaiseError(string)" /> methods. It provides details about the error for diagnostic purposes.
    /// </remarks>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    ///     Gets the instance of <see cref="IFamilyManager" /> associated with this family source.
    /// </summary>
    /// <remarks>
    ///     The <see cref="IFamilyManager" /> provides functionality for managing Revit families,
    ///     including operations such as searching, editing, and registering families.
    ///     This property is initialized through the constructor and is used internally
    ///     to interact with the family management system.
    /// </remarks>
    protected IFamilyManager FamilyManager { get; }

    /// <summary>
    ///     Gets the options used to configure the family source.
    /// </summary>
    /// <remarks>
    ///     The options are of type <typeparamref name="TOptions" />, which must inherit from
    ///     <see cref="FamilySourceOptions" />. These options define specific configurations
    ///     for the family source, such as its name or other relevant settings.
    /// </remarks>
    protected TOptions Options { get; }

    /// <summary>
    ///     Gets the factory method used to create instances of <see cref="RevitFamily" />.
    /// </summary>
    /// <remarks>
    ///     This property provides a delegate that facilitates the creation of <see cref="RevitFamily" /> objects.
    ///     It is typically used to instantiate families with specific configurations, such as name, family information,
    ///     and save actions. The factory method ensures consistency in the creation process and integrates with
    ///     the <see cref="IFamilyManager" /> for registration.
    /// </remarks>
    protected IRevitFamily.Factory FamilyFactory { get; }

    /// <summary>
    ///     Releases the resources used by the <see cref="FamilySource{TOptions}" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method is part of the implementation of the <see cref="IDisposable" /> interface. It ensures that
    ///     both managed and unmanaged resources are properly released when the object is no longer needed.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Gets the name of the family source.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the family source.
    /// </value>
    /// <remarks>
    ///     This property provides a unique identifier or descriptive name for the family source,
    ///     which can be used for display purposes or to differentiate between multiple sources.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    ///     Gets the collection of folders associated with the family source.
    /// </summary>
    /// <value>
    ///     An enumerable collection of <see cref="IFolder" /> objects representing the folders
    ///     managed by this family source.
    /// </value>
    /// <remarks>
    ///     This property provides access to the hierarchical structure of folders within the family source.
    ///     Each folder may contain subfolders and families. The implementation of this property is specific
    ///     to the derived class and determines how the folders are retrieved or constructed.
    /// </remarks>
    public abstract IAsyncEnumerable<IFolder> GetFoldersAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Reloads the family source, triggering any necessary updates and notifying subscribers of the reload event.
    /// </summary>
    /// <remarks>
    ///     This method invokes the <see cref="OnReload" /> method to perform the actual reload logic
    ///     and then raises the <see cref="Reloaded" /> event to notify listeners that the reload operation has completed.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown if the method is called on a disposed instance of the family source.
    /// </exception>
    public void Reload()
    {
        OnReload();
        Reloaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Occurs when the family source has been reloaded.
    /// </summary>
    /// <remarks>
    ///     This event is triggered after the <see cref="Reload" /> method is called and the reload operation
    ///     is completed. Subscribers can use this event to perform any actions required after the family
    ///     source has been refreshed.
    /// </remarks>
    public event EventHandler<EventArgs>? Reloaded;

    /// <summary>
    ///     Gets the type of the family source.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the type of the family source, as defined by the associated
    ///     <see cref="IFamilySourceOptions" />.
    /// </value>
    /// <remarks>
    ///     This property retrieves the <c>Type</c> value from the <see cref="Options" /> of the family source.
    /// </remarks>
    public string Type => Options.Type;

    /// <summary>
    ///     Gets a stream that provides a preview of the family data.
    /// </summary>
    /// <remarks>
    ///     The returned <see cref="Stream" /> may contain data specific to the implementation of the family source.
    ///     It is the caller's responsibility to ensure proper disposal of the stream after use.
    /// </remarks>
    /// <returns>
    ///     A <see cref="Stream" /> object containing the preview data, or <c>null</c> if no preview is available.
    /// </returns>
    public abstract Stream? Preview { get; }

    /// <summary>
    ///     Occurs when an error is raised in the family source.
    /// </summary>
    /// <remarks>
    ///     This event is triggered to notify subscribers about an error that has occurred within the family source.
    ///     The event provides details about the error through a <see cref="FamilySourceErrorEventArgs" /> instance.
    /// </remarks>
    public event EventHandler<FamilySourceErrorEventArgs>? Error;

    /// <summary>
    ///     Raises an error event with the specified exception details.
    /// </summary>
    /// <param name="e">The exception containing details about the error.</param>
    /// <remarks>
    ///     This method sets the <see cref="ErrorMessage" /> property to the exception's message
    ///     and triggers the <see cref="Error" /> event to notify subscribers about the error.
    /// </remarks>
    protected void RaiseError(Exception e)
    {
        RaiseError(e.Message);
    }

    /// <summary>
    ///     Raises an error event with the specified error message.
    /// </summary>
    /// <param name="message">
    ///     The error message to be associated with the error event.
    /// </param>
    /// <remarks>
    ///     This method sets the <see cref="ErrorMessage" /> property to the provided message and triggers the
    ///     <see cref="Error" /> event to notify subscribers about the error.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="message" /> is <c>null</c>.
    /// </exception>
    protected void RaiseError(string message)
    {
        ErrorMessage = message;
        RaiseError();
    }

    /// <summary>
    ///     Raises an error event to notify subscribers about an error in the family source.
    /// </summary>
    /// <remarks>
    ///     This method triggers the <see cref="Error" /> event, providing details about the error
    ///     through a <see cref="FamilySourceErrorEventArgs" /> instance. It is typically called
    ///     internally when an error occurs during operations within the family source.
    /// </remarks>
    protected void RaiseError()
    {
        Error?.Invoke(this, new FamilySourceErrorEventArgs(true, this));
    }

    /// <summary>
    ///     Resets the current error state for the family source and raises the <see cref="Error" /> event
    ///     to indicate that the error has been cleared.
    /// </summary>
    /// <remarks>
    ///     This method is typically called to clear any previously raised errors and notify subscribers
    ///     that the family source is no longer in an error state.
    /// </remarks>
    protected void ResetError()
    {
        Error?.Invoke(this, new FamilySourceErrorEventArgs(false, this));
    }

    /// <summary>
    ///     Executes the logic required to reload the family source.
    /// </summary>
    /// <remarks>
    ///     This method is called internally by the <see cref="Reload" /> method to perform
    ///     the specific operations necessary for reloading the family source. Derived classes
    ///     must implement this method to define their custom reload behavior.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown if the method is invoked on a disposed instance of the family source.
    /// </exception>
    protected abstract void OnReload();

    /// <summary>
    ///     Releases the resources used by the <see cref="FamilySource{TOptions}" /> class.
    /// </summary>
    /// <param name="disposing">
    ///     A boolean value indicating whether the method is being called explicitly
    ///     (true) or by the garbage collector (false).
    /// </param>
    /// <remarks>
    ///     This method is called by the <see cref="Dispose()" /> method and the finalizer.
    ///     When <paramref name="disposing" /> is true, it releases both managed and unmanaged resources.
    ///     When false, it releases only unmanaged resources.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
        }
    }

    /// <summary>
    ///     Loads a resource from the application's resources as a stream using the specified pack URI.
    /// </summary>
    /// <param name="packUri">
    ///     The pack URI of the resource to load. This should be a valid pack URI pointing to an embedded resource
    ///     within the application's resources.
    /// </param>
    /// <returns>
    ///     A <see cref="Stream" /> containing the resource data. The returned stream is a memory stream containing
    ///     a copy of the resource's content, positioned at the beginning.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    ///     Thrown if <paramref name="packUri" /> is null or empty.
    /// </exception>
    /// <exception cref="System.IO.FileNotFoundException">
    ///     Thrown if the resource cannot be found at the specified <paramref name="packUri" />.
    /// </exception>
    /// <remarks>
    ///     This method is intended for use in WPF applications where resources are embedded and accessed via pack URIs.
    ///     It retrieves the resource as a stream, copies its contents into a new <see cref="MemoryStream" />, and returns
    ///     the memory stream positioned at the beginning. The caller is responsible for disposing the returned stream.
    /// </remarks>
    protected static Stream LoadResourceAsStream(string packUri)
    {
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

    /// <summary>
    ///     Creates a new instance of <see cref="RevitFamilyInfo" /> using the provided file stream loader.
    /// </summary>
    /// <param name="fileStreamLoader">
    ///     A function that returns a <see cref="Stream" /> for loading the family file.
    /// </param>
    /// <returns>
    ///     A <see cref="RevitFamilyInfo" /> object containing information about the Revit family.
    /// </returns>
    /// <remarks>
    ///     The method attempts to retrieve additional information from the "BIM.FamilyManager" storage
    ///     if it is present within the family file.
    /// </remarks>
    protected RevitFamilyInfo CreateFamilyInfo(Func<Stream> fileStreamLoader)
    {
        // RevitFamilyInfo attempts to retrieve additional information from the "BIM.FamilyManager" storage, if it is present within the family file.
        var familyInfo = new RevitFamilyInfo(fileStreamLoader, "BIM.FamilyManager");
        return familyInfo;
    }
}
