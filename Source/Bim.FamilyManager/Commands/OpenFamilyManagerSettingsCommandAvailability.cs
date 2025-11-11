using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit;
using Scotec.Revit.Isolation;

namespace Bim.FamilyManager.Commands;

[RevitCommandAvailabilityIsolation(ContextName = "Bim.FamilyManager")]

public class OpenFamilyManagerSettingsCommandAvailability : RevitCommandAvailability
{
    protected override bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories, IServiceProvider services)
    {
        return true;
    }
}
