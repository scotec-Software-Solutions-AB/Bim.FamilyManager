using System.Windows.Media;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Abstractions.ViewModels;
using Bim.FamilyManager.Base.Options;
using Microsoft.Extensions.Options;

namespace Bim.FamilyManager.Ui.ViewModels;

/// <summary>
///     Represents the view model for a folder in the Revit Family Manager application.
/// </summary>
/// <remarks>
///     This class provides properties and commands to interact with folder data, such as its name and path.
///     It is designed to work with the <see cref="IFolder" /> abstraction and integrates with WPF commands for UI
///     interaction.
/// </remarks>
public abstract class FolderViewModel<TLayoutOptions> : FamilyManagerItemViewModel<TLayoutOptions>, IFolderViewModel
    where TLayoutOptions : LayoutOptions
{
    private IEnumerable<IFamilyViewModel>? _families;
    private bool _isExpanded;
    private IEnumerable<IFolderViewModel>? _subfolders;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FolderViewModel{TLayoutOptions}" /> class with the specified folder
    ///     and layout options.
    /// </summary>
    /// <param name="folder">
    ///     An instance of <see cref="IFolder" /> representing the folder data to be encapsulated by the view model.
    /// </param>
    /// <param name="layoutOptions">
    ///     The <see cref="IOptionsMonitor{TLayoutOptions}" /> providing the current and updated layout options.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the <see cref="FolderViewModel{TLayoutOptions}" /> with the provided folder data and
    ///     layout options, enabling
    ///     interaction with folder properties and preview image. It integrates with the <see cref="IFolder" /> abstraction and
    ///     supports dynamic creation of subfolder and family view models.
    /// </remarks>
    protected FolderViewModel(IFolder folder, IOptionsMonitor<TLayoutOptions> layoutOptions)
        : base(layoutOptions)
    {
        Folder = folder;
        Preview = folder.Preview is null ? null : GetPreviewImage(folder.Preview);
    }

    /// <summary>
    ///     Gets the name of the folder represented by this view model.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the folder.
    /// </value>
    /// <remarks>
    ///     This property retrieves the name of the folder from the associated <see cref="IFolder" /> instance.
    ///     It is commonly used for display purposes in the UI.
    /// </remarks>
    public override string Name => Folder.Name;

    /// <summary>
    ///     Gets the preview image associated with the folder.
    /// </summary>
    /// <value>
    ///     An <see cref="ImageSource" /> representing the preview image of the folder, or <c>null</c> if no preview is
    ///     available.
    /// </value>
    /// <remarks>
    ///     The preview image is typically used to visually represent the folder in the user interface.
    ///     It is loaded from the underlying <see cref="IFolder" /> instance.
    /// </remarks>
    public override ImageSource? Preview { get; }

    /// <summary>
    ///     Gets the folder associated with this view model.
    /// </summary>
    /// <value>
    ///     The <see cref="IFolder" /> instance representing the folder's data.
    /// </value>
    /// <remarks>
    ///     This property provides access to the underlying <see cref="IFolder" /> instance,
    ///     which represents the folder's data, including its name, path, subfolders, and families.
    /// </remarks>
    public IFolder Folder { get; }

    /// <summary>
    ///     Gets the collection of subfolders associated with the current folder.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{IFolderViewModel}" /> representing the subfolders, or <c>null</c> if there are no
    ///     subfolders.
    /// </value>
    /// <remarks>
    ///     This property retrieves subfolder data from the underlying <see cref="IFolder" /> abstraction
    ///     and creates corresponding <see cref="FolderViewModel{TLayoutOptions}" /> instances using the provided factory.
    ///     GetSubfoldersAsync are loaded only when the folder is expanded or selected for performance reasons.
    /// </remarks>
    public IEnumerable<IFolderViewModel>? Subfolders
    {
        get
        {
            if (IsExpanded || IsSelected)
            {
                if (_subfolders is null)
                {
                    var subfolders = Task.Run(async () =>
                    {
                        var subfolders = new List<IFolder>();
                        await foreach (var subfolder in Folder.GetSubfoldersAsync(CancellationToken.None))
                        {
                            subfolders.Add(subfolder);
                        }

                        return subfolders.OrderBy(f => f.Name)
                                         .ToList();
                    }).ConfigureAwait(true).GetAwaiter().GetResult();

                    _subfolders = subfolders.Select(CreateSubfolderViewModel)
                                            .ToList();
                }

                return _subfolders;
            }

            return null;
        }
    }
    // Performance: Do not load the families if the folder is not expanded.

    /// <summary>
    ///     Gets the collection of families associated with the folder represented by this view model.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="IFamilyViewModel" /> objects representing the families in the folder, or <c>null</c> if
    ///     the folder is not expanded.
    /// </value>
    /// <remarks>
    ///     This property dynamically loads and returns the families when the folder is expanded or selected. If the folder is
    ///     not expanded,
    ///     the property returns <c>null</c>. The families are represented as instances of <see cref="IFamilyViewModel" />.
    /// </remarks>
    public IEnumerable<IFamilyViewModel>? Families
    {
        get
        {
            // Performance: Do not load the families if the folder is not expanded.
            if (IsExpanded || IsSelected)
            {
                if (_families is null)
                {
                    var families = Task.Run(async () =>
                    {
                        var families = new List<IRevitFamily>();
                        await foreach (var family in Folder.GetFamiliesAsync(true, CancellationToken.None))
                        {
                            families.Add(family);
                        }

                        return families.OrderBy(f => f.Name)
                                       .ToList();
                        
                        // Probably performace problem. UI blocking.
                    }).ConfigureAwait(true).GetAwaiter().GetResult();
                    _families = families.Select(CreateFamilyViewModel)
                                        .ToList();
                }
            }

            return _families;
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the folder is expanded in the UI.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the folder is expanded; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     When the folder is expanded, its associated families are loaded and displayed.
    ///     Changing this property triggers the <see cref="Families" /> property to update, ensuring
    ///     that the families are loaded only when needed. This property is typically bound to a UI element to control
    ///     expansion state.
    /// </remarks>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            SetProperty(ref _isExpanded, value);
            if (_families is null)
            {
                // First time expanded. Notify property change to ensure families are loaded.
                OnPropertyChanged(nameof(Families));
            }
        }
    }

    protected abstract IFolderViewModel CreateSubfolderViewModel(IFolder subfolder);

    protected abstract IFamilyViewModel CreateFamilyViewModel(IRevitFamily family);
}
