using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Resources;
using Microsoft.Extensions.DependencyInjection;
using Scotec.Revit;
using Scotec.Revit.Isolation;

namespace Bim.FamilyManager.Commands;

[RevitCommandIsolation(ContextName = "Bim.FamilyManager")]
[Transaction(TransactionMode.Manual)]
public class CreatePreviewImageCommand : RevitCommand
{
    public CreatePreviewImageCommand()
    {
        NoTransaction = true;
    }

    protected override string CommandName => StringResources.Command_OpenFamilyManager_Name;

    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        var familyManager = services.GetRequiredService<IFamilyManager>();
        
        var view = commandData.View;
        var application = commandData.Application;

        familyManager.CreatePreviewImage(application, view);


        return Result.Succeeded;
    }
}
