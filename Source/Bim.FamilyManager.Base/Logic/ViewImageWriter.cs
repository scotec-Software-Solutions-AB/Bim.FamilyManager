using OpenMcdf;
using System;
using System.IO;
using System.Text.Json;
using Autodesk.Revit.DB;
using Bim.FamilyManager.Base.Logic.EStorage;

namespace Bim.FamilyManager.Base.Logic
{
    public class ViewImageWriter
    {
        public static void WritePreviewImage(PreviewImageEStorage eStorage, Document document, Stream imageStream)
        {
            
            //using (var root = RootStorage.Open(documentStream, StorageModeFlags.LeaveOpen))
            //{
            //    if (!root.TryOpenStorage("BIM.FamilyManager", out var infoStorage))
            //    {
            //        infoStorage = root.CreateStorage("BIM.FamilyManager");
            //    }

            //    // Delete current family info if it exists.
            //    infoStorage.Delete("RevitPreview4.0");

            //    // Add the new family info.
            //    var stream = infoStorage.CreateStream("RevitPreview4.0");
            //    imageStream.CopyTo(stream);
            //    stream.Flush();

            //    root.Flush(true);
            //}
            
//            documentStream.Position = 0;
        }
    }
}
