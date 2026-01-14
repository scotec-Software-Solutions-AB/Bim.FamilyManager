using System.IO;
using OpenMcdf;

namespace Bim.FamilyManager.Base.Logic;

public class ViewImageWriter
{
    //public static void WritePreviewImage(PreviewImageEStorage eStorage, Stream documentStream, Stream imageStream)
    public static void WritePreviewImage(Stream documentStream, Stream imageStream)
    {
        using var root = RootStorage.Open(documentStream, StorageModeFlags.LeaveOpen);

        WriteToFamilyManagerFolder(root, documentStream, imageStream);
        WriteToRevitPreview(root, documentStream, imageStream);

        root.Flush(true);
    }

    private static void WriteToFamilyManagerFolder(RootStorage root, Stream documentStream, Stream imageStream)
    {
        if (!root.TryOpenStorage("BIM.FamilyManager", out var infoStorage))
        {
            infoStorage = root.CreateStorage("BIM.FamilyManager");
        }

        // Delete current family info if it exists.
        infoStorage.Delete("FamilyPreviewImage");

        // Add the new family info.
        var stream = infoStorage.CreateStream("FamilyPreviewImage");

        imageStream.CopyTo(stream);
        stream.Flush();

        root.Flush(true);

        documentStream.Position = 0;
    }

    private static void WriteToRevitPreview(RootStorage root, Stream documentStream, Stream imageStream)
    {
        // TODO: Need to write pre and post fix.
        return;
        root.Delete("RevitPreview4.0");

        // Add the new family info.
        var stream = root.CreateStream("RevitPreview4.0");

        imageStream.CopyTo(stream);
        stream.Flush();

        root.Flush(true);

        documentStream.Position = 0;
    }
}
