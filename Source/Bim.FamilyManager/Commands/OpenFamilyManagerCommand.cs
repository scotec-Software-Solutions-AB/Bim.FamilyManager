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
        NoTransaction = true;
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

    /// <summary>
    ///     Executes the command to open the Family Manager dockable pane in Autodesk Revit.
    /// </summary>
    /// <param name="commandData">
    ///     An object that provides access to the Revit application and its associated data.
    /// </param>
    /// <param name="services">
    ///     A service provider that supplies additional services required for command execution.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.UI.Result" /> value indicating the success or failure of the command execution.
    /// </returns>
    /// <remarks>
    ///     This method activates and displays the dockable pane associated with the Family Manager,
    ///     allowing users to manage Revit families. The command is executed manually and does not
    ///     require a transaction.
    /// </remarks>
    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        var dockableWindow = commandData.Application.GetDockablePane(new DockablePaneId(Constants.PaneId));
        dockableWindow.Show();

        return Result.Succeeded;
    }
}
