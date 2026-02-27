using Autodesk.Internal.InfoCenter;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Bim.FamilyManager.Abstractions;
using Bim.FamilyManager.Ui.Resources;
using Microsoft.Extensions.DependencyInjection;
using Scotec.Revit;
using Scotec.Revit.Isolation;

namespace Bim.FamilyManager.Commands;

[RevitCommandIsolation(ContextName = "Bim.FamilyManager")]
[RevitTransactionMode(Mode = RevitTransactionMode.None)]
public class CreatePreviewImageCommand : RevitCommand
{
    public CreatePreviewImageCommand()
    {
    }

    protected override string CommandName => StringResources.Command_CreatePreviewImages_Name;

    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        var familyManager = services.GetRequiredService<IFamilyManager>();

        var view = commandData.View;
        var application = commandData.Application;

        familyManager.CreatePreviewImage(application, view);

        var notification = StringResources.Command_CreatePreviewImages_Notification_Success;
        var result = new ResultItem
        {
            Title = notification,
            Category = StringResources.FamilyManager_Name,
            IsNew = true,
            Timestamp = DateTime.Now,
            Type = ResultType.Unknown
        };

        ComponentManager.InfoCenterPaletteManager.ShowBalloon(result);

        return Result.Succeeded;
    }
}
