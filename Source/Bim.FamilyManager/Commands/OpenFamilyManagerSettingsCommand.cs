using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Bim.FamilyManager.Resources;
using Bim.FamilyManager.Ui.Views.Settings;
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
[Transaction(TransactionMode.Manual)]
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
        NoTransaction = true;
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

    /// <summary>
    ///     Executes the command to open the Family Manager settings window in Autodesk Revit.
    /// </summary>
    /// <param name="commandData">
    ///     Provides access to the Revit application and its associated data.
    /// </param>
    /// <param name="services">
    ///     A service provider that supplies additional services required for the execution of the command.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.UI.Result" /> indicating the outcome of the command execution.
    /// </returns>
    /// <remarks>
    ///     This method retrieves the factory for creating the
    ///     <see cref="Bim.FamilyManager.Ui.Views.Settings.SettingsManagerWindow" />
    ///     and displays the settings window as a modal dialog. The command is executed manually and does not require a
    ///     transaction.
    /// </remarks>
    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        var settingsWindowFactory = services.GetRequiredService<SettingsManagerWindow.Factory>();
        var window = settingsWindowFactory();

        window.ShowDialog();

        return Result.Succeeded;
    }
}
