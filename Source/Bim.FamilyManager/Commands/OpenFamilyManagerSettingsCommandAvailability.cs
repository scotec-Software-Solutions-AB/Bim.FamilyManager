using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit;
using Scotec.Revit.Isolation;

namespace Bim.FamilyManager.Commands;

/// <summary>
///     Determines the availability of the OpenFamilyManagerSettings command in the Bim.FamilyManager context.
/// </summary>
/// <remarks>
///     This class is used by Revit to check whether the Family Manager Settings command should be enabled.
///     The command is always available, as <see cref="IsCommandAvailable" /> returns <c>true</c> regardless of the
///     application state or selected categories.
/// </remarks>
[RevitCommandAvailabilityIsolation(ContextName = "Bim.FamilyManager")]
public class OpenFamilyManagerSettingsCommandAvailability : RevitCommandAvailability
{
    /// <summary>
    ///     Checks if the OpenFamilyManagerSettings command is available.
    /// </summary>
    /// <param name="applicationData">The current <see cref="UIApplication" /> instance.</param>
    /// <param name="selectedCategories">The set of selected <see cref="Category" /> objects.</param>
    /// <param name="services">The service provider for dependency resolution.</param>
    /// <returns>
    ///     Always returns <c>true</c>, indicating the command is available in all contexts.
    /// </returns>
    protected override bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories, IServiceProvider services)
    {
        return true;
    }
}
