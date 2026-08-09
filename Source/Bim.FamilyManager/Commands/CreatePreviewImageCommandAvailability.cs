using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit;
using Scotec.Revit.Isolation;

namespace Bim.FamilyManager.Commands;

[RevitCommandAvailabilityIsolation(ContextName = "Bim.FamilyManager")]
public class CreatePreviewImageCommandAvailability : RevitCommandAvailability
{
    [RevitCommandAvailabilityCheck]
    protected bool IsCommandAvailable(UIApplication applicationData)
    {
        var view = applicationData.ActiveUIDocument?.ActiveView;
        if (view is null)
        {
            return false;
        }

        var document = applicationData.ActiveUIDocument?.Document;
        if (document?.IsFamilyDocument == true)
        {
            var settings = document.GetDocumentPreviewSettings();
            return settings.IsViewIdValidForPreview(view.Id);
        }

        return false;
    }
}
