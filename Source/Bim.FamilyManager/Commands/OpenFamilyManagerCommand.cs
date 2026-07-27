using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Resources;
using Scotec.Revit;
using Scotec.Revit.Isolation;

namespace Bim.FamilyManager.Commands;

/// <summary>
///     Defines a command that opens the Family Manager interface within Autodesk Revit.
/// </summary>
/// <remarks>
///     The command activates and displays the dockable pane associated with the Family Manager,
///     enabling users to manage Revit families efficiently.
///     This command is designed to be executed manually by the user.
///     It runs in its own <see cref="System.Runtime.Loader.AssemblyLoadContext" /> to ensure isolation,
///     preventing potential conflicts with other assemblies.
/// </remarks>
[RevitCommandIsolation(ContextName = "Bim.FamilyManager")]
[RevitTransactionMode(Mode = RevitTransactionMode.ReadOnly)]
public class OpenFamilyManagerCommand : RevitCommand
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Bim.FamilyManager.Commands.OpenFamilyManagerCommand" />
    ///     class.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the command to run without requiring a transaction,
    ///     ensuring that it operates in a read-only mode within Autodesk Revit.
    /// </remarks>
    public OpenFamilyManagerCommand()
    {
    }

    /// <summary>
    ///     Gets the name of the command that opens the Family Manager interface.
    /// </summary>
    /// <value>
    ///     A localized string representing the name of the command, retrieved from the resource file.
    /// </value>
    /// <remarks>
    ///     This property provides the display name of the command as defined in the resource file.
    ///     It is used to identify the command in the user interface.
    /// </remarks>
    protected override string CommandName => StringResources.Command_OpenFamilyManager_Name;

    protected Result OnExecute(UIControlledApplication uiApplication)
    {
        var dockableWindow = uiApplication.GetDockablePane(new DockablePaneId(Constants.PaneId));
        dockableWindow.Show();

        return Result.Succeeded;
    }
}
