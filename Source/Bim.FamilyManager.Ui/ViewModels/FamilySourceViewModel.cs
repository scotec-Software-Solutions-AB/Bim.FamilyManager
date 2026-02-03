using System.Windows.Media;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Base.Options;
using Microsoft.Extensions.Options;
using Scotec.Events.WeakEvents;

namespace Bim.FamilyManager.Ui.ViewModels;

/// <summary>
///     Represents the view model for a family source in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This class provides functionality to manage and interact with a family source, including its folders and selection
///     state.
/// </remarks>
public abstract class FamilySourceViewModel<TLayoutOptions> : FamilyManagerItemViewModel<TLayoutOptions>, IFamilySourceViewModel
    where TLayoutOptions : LayoutOptions
{
    private readonly IFamilySource _familySource;
    private IList<IFolderViewModel>? _folders;
    private IFolderViewModel? _selectedFolder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilySourceViewModel{TLayoutOptions}" /> class.
    /// </summary>
    /// <param name="familySource">
    ///     An instance of <see cref="IFamilySource" /> representing the family source to be managed by
    ///     this view model.
    /// </param>
    /// <param name="panelFactory">
    ///     A factory delegate for creating instances of <see cref="IFamilySourcePanelViewModel" /> to
    ///     represent the panel for the family source.
    /// </param>
    /// <param name="layoutOptions">
    ///     An <see cref="IOptionsMonitor{TLayoutOptions}" /> providing the current and updated layout
    ///     options.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the view model with the specified family source and panel factory, enabling interaction
    ///     with the family source's data and its associated panel.
    /// </remarks>
    protected FamilySourceViewModel(
        IFamilySource familySource,
        IFamilySourcePanelViewModel.Factory panelFactory,
        IOptionsMonitor<TLayoutOptions> layoutOptions)
        : base(layoutOptions)
    {
        _familySource = familySource;

        StaticWeakEventManager.AddWeakHandler(_familySource, nameof(IFamilySource.Reloaded), OnReloaded);
        Panel = panelFactory.Invoke(familySource);
        Preview = familySource.Preview is null ? null : GetPreviewImage(familySource.Preview);
    }

    /// <summary>
    ///     Gets the collection of folder view models associated with the family source.
    /// </summary>
    /// <remarks>
    ///     This property initializes the folder collection by transforming the folders from the family source
    ///     into <see cref="IFolderViewModel" /> instances using the provided factory. The collection is sorted
    ///     by folder name and the first folder is set as the selected folder by default.
    /// </remarks>
    /// <value>
    ///     A list of <see cref="IFolderViewModel" /> instances representing the folders in the family source.
    /// </value>
    public IList<IFolderViewModel>? Folders
    {
        get
        {
            if (_folders is null)
            {
                var folders = Task.Run(async () =>
                {
                    var folders = new List<IFolder>();
                    await foreach (var folder in _familySource.GetFoldersAsync(CancellationToken.None))
                    {
                        folders.Add(folder);
                    }

                    return folders.OrderBy(f => f.Name)
                                  .ToList();
                }).ConfigureAwait(true).GetAwaiter().GetResult();

                _folders = folders.Select(CreateFolderViewModel)
                                  .ToList();

                SelectedFolder = _folders?.FirstOrDefault();
            }

            return _folders;
        }
    }

    /// <summary>
    ///     Gets the name of the family source represented by this view model.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the family source.
    /// </value>
    /// <remarks>
    ///     This property retrieves the name from the underlying <see cref="IFamilySource" /> instance.
    /// </remarks>
    public override string Name => _familySource.Name;

    /// <summary>
    ///     Gets the preview image associated with the family source.
    /// </summary>
    /// <value>
    ///     An <see cref="ImageSource" /> representing the preview image, or <c>null</c> if unavailable.
    /// </value>
    public override ImageSource? Preview { get; }

    /// <summary>
    ///     Gets or sets the currently selected folder in the family source.
    /// </summary>
    /// <value>
    ///     The selected folder represented by an <see cref="IFolderViewModel" /> instance, or <c>null</c> if no folder is
    ///     selected.
    /// </value>
    /// <remarks>
    ///     This property is used to track the user's current folder selection within the family source.
    ///     Changes to this property will update the selection state and may trigger related UI updates.
    /// </remarks>
    public IFolderViewModel? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (_selectedFolder is not null)
            {
                _selectedFolder.IsSelected = false;
            }

            if (value is not null)
            {
                value.IsSelected = true;
            }

            SetProperty(ref _selectedFolder, value);
        }
    }

    /// <summary>
    ///     Gets the panel view model associated with the family source.
    /// </summary>
    /// <value>
    ///     An <see cref="IFamilySourcePanelViewModel" /> instance representing the panel, or <c>null</c> if not available.
    /// </value>
    public IFamilySourcePanelViewModel? Panel { get; }

    protected virtual void OnReloaded(IFamilySource? sender, EventArgs e)
    {
        _folders = null;
        SelectedFolder = null;
        OnPropertyChanged(nameof(Folders));
    }

    /// <summary>
    ///     Creates a folder view model for the specified folder.
    /// </summary>
    /// <param name="folder">The <see cref="IFolder" /> instance to create a view model for.</param>
    /// <returns>An <see cref="IFolderViewModel" /> representing the folder.</returns>
    protected abstract IFolderViewModel CreateFolderViewModel(IFolder folder);
}
