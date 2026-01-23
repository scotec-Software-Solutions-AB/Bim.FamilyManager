using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit;
using Scotec.Revit.Isolation;

namespace Bim.FamilyManager.Ui.Commands;

[RevitCommandAvailabilityIsolation(ContextName = "Bim.FamilyManager")]
public class CreatePreviewImageCommandAvailability : RevitCommandAvailability
{
    protected override bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories, IServiceProvider services)
    {
        var document = applicationData.ActiveUIDocument.Document;
        var view = applicationData.ActiveUIDocument.ActiveView;
        if (view is null)
        {
            return false;
        }

        if (document.IsFamilyDocument)
        {
            var settings = document.GetDocumentPreviewSettings();
            return settings.IsViewIdValidForPreview(view.Id);
        }

        return false;
    }
}
