using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Bim.FamilyManager.Resources;
using Bim.FamilyManager.Ui.Views.Settings;
using Microsoft.Extensions.DependencyInjection;
using Scotec.Revit;
using Scotec.Revit.Isolation;

namespace Bim.FamilyManager.Commands;

/// <summary>
///     Represents a command to open the Family Manager settings within Autodesk Revit.
/// </summary>
/// <remarks>
///     This command is responsible for displaying the settings interface of the Family Manager,
///     allowing users to configure and manage Revit families effectively. It is executed manually
///     and operates in an isolated context to avoid conflicts with other assemblies.
/// </remarks>
[RevitCommandIsolation(ContextName = "Bim.FamilyManager")]
[RevitTransactionMode(Mode = RevitTransactionMode.ReadOnly)]
public class OpenFamilyManagerSettingsCommand : RevitCommand
{
    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="Bim.FamilyManager.Commands.OpenFamilyManagerSettingsCommand" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor configures the command to operate in an isolated context without requiring a transaction.
    /// </remarks>
    public OpenFamilyManagerSettingsCommand()
    {
    }

    /// <summary>
    ///     Gets the name of the command to open the Family Manager settings.
    /// </summary>
    /// <remarks>
    ///     This property retrieves a localized string representing the name of the command,
    ///     which is displayed in the user interface of Autodesk Revit. The value is sourced
    ///     from the <see cref="Bim.FamilyManager.Resources.StringResources.Command_OpenFamilyManagerSettings_Name" />
    ///     resource.
    /// </remarks>
    protected override string CommandName => StringResources.Command_OpenFamilyManagerSettings_Name;

    protected Result OnExecute(SettingsManagerWindow.Factory settingsWindowFactory)
    {
        var window = settingsWindowFactory();

        window.ShowDialog();

        return Result.Succeeded;
    }
}
