using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Bim.FamilyManager.Base.Logic;

public static class ViewImageExporter
{
    public static Stream ExportViewPng(Document document, View view, int pixelSize = 1024)
    {
        // Use a transaction group to temporarily hide elements and then roll it back.
        // Within the transaction group, a transaction is used to perform the temporary hiding.
        // This transaction must be committed before generating the image, but the transaction group is rolled back afterward.
        // This approach allows modifications to the view before image creation without making permanent changes,
        // ensuring that the document is not marked as modified.
        var transactionGroup = new TransactionGroup(document, "Temp hide connectors");
        transactionGroup.Start();

        
        // Temporarily hide connectors (Family Editor connectors)
        using (var t = new Transaction(document, "Temp hide connectors"))
        {
            t.Start();

            var elementIds = GetFilteredElements(document, view).ToList();

            if (elementIds.Count > 0)
                view.HideElementsTemporary(elementIds);


            if (view.HasDetailLevel() && view.CanModifyDetailLevel())
            {
                view.DetailLevel = ViewDetailLevel.Fine;
            }

            if (view.HasDisplayStyle() && view.CanModifyDisplayStyle())
            {
                view.DisplayStyle = DisplayStyle.Realistic;
            }

            t.Commit();
        }

        // Export the view as image (NO transaction needed)
        // FilePath is a *base name*; Revit appends view name / index.
        var basePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));
        var opt = new ImageExportOptions
        {
            ExportRange = ExportRange.SetOfViews,
            FilePath = basePath,
            PixelSize = pixelSize,
            ZoomType = ZoomFitType.FitToPage,

            // Choose PNG for both hidden-line/wireframe and shaded/shadow cases
            HLRandWFViewsFileType = ImageFileType.PNG,
            ShadowViewsFileType = ImageFileType.PNG,
            ImageResolution = ImageResolution.DPI_72,
        };

        opt.SetViewsAndSheets(new List<ElementId> { view.Id });

        document.ExportImage(opt);

        // Revit can tell you the produced filename:
        var exportedFile = ImageExportOptions.GetFileName(document, view.Id);

        // Rollback the transaction group to undo all committed changes.
        transactionGroup.RollBack();


        using (var file = new FileStream(
                   basePath + exportedFile + ".png",
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.Read,
                   bufferSize: 4096,
                   options: FileOptions.DeleteOnClose))
        {
            var stream = new MemoryStream();
            file.CopyTo(stream);
            stream.Position = 0;

            return stream;
        }
    }

    //Todo: Refactor this method.
    private static IEnumerable<ElementId> GetFilteredElements(Document document, View view)
    {
        var classes = new List<Type>()
        {
            typeof(ConnectorElement), // Connectors
            typeof(SpatialElementCalculationPoint), // SpatialElement calculation points
            typeof(Dimension), // Dimensions
            typeof(ReferencePlane), // Reference planes
            typeof(TextNote), // Text notes
            typeof(CurveElement), // Reference lines are ModelCurves with IsReferenceLine == true
            typeof(GenericForm), // Void elements: GenericForm.IsSolid == false
        };
        var classFilter = new ElementMulticlassFilter(classes);

        var classElements = new FilteredElementCollector(document, view.Id)
                       .WherePasses(classFilter)
                       .WhereElementIsNotElementType()
                       .Select(e => e);

        foreach (var element in classElements)
        {
            if (element is CurveElement curveElement)
            {
                if (curveElement.LineStyle is GraphicsStyle graphicsStyle)
                {
                    var lineStyleCategory = graphicsStyle.GraphicsStyleCategory.BuiltInCategory;
                    if (lineStyleCategory is BuiltInCategory.OST_MechanicalEquipmentHiddenLines or BuiltInCategory.OST_InvisibleLines)
                    {
                        yield return element.Id;
                    }
                }

                // Reference lines (UI): ModelCurve with IsReferenceLine == true
                if (curveElement is ModelCurve { IsReferenceLine: true })
                {
                    yield return element.Id;
                }
                continue;
            }

            // Void elements (UI): GenericForm that is not solid
            if (element is GenericForm gf)
            {
                if (!gf.IsSolid)
                {
                    yield return element.Id;
                }

                continue;
            }

            // All other included classes (connectors, dims, ref planes, text, calc points, etc.)
            yield return element.Id;
        }

        var categories = new List<BuiltInCategory>
        {
            BuiltInCategory.OST_Lines
        };
        
        var categoryFilter = new ElementMulticategoryFilter(categories);

        var categoryElements = new FilteredElementCollector(document, view.Id)
                       .WherePasses(classFilter)
                       .WhereElementIsNotElementType()
                       .Select(e => e);

        foreach (var element in categoryElements)
        {
            if (element is CurveElement curveElement)
            {
                var lineStyleIds = curveElement.GetLineStyleIds();
            }
        }

    }
}