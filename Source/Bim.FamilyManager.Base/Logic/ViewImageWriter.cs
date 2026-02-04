using System.IO;
using OpenMcdf;

namespace Bim.FamilyManager.Base.Logic;

public class ViewImageWriter
{
    //public static void WritePreviewImage(PreviewImageEStorage eStorage, Stream documentStream, Stream imageStream)
    public static void WritePreviewImages(Stream documentStream, string familyPreviewImageName, IDictionary<string, Stream> typePreviewImageStreams)
    {
        if (!typePreviewImageStreams.Any())
        {
            return;
        }

        using var root = RootStorage.Open(documentStream, StorageModeFlags.LeaveOpen);
        root.Delete("Previews");

        WriteRevitFamilyPreview(root, documentStream, typePreviewImageStreams[familyPreviewImageName]);

        if (!root.TryOpenStorage("BIM.FamilyManager", out var familyManagerStorage))
        {
            familyManagerStorage = root.CreateStorage("BIM.FamilyManager");
        }

        // Remove all previously inserted preview images.
        familyManagerStorage.Delete("PreviewImages");

        var previewStorage = familyManagerStorage.CreateStorage("PreviewImages");

        WriteFamilyPreview(previewStorage, documentStream, typePreviewImageStreams[familyPreviewImageName]);
        WriteTypePreviews(previewStorage, documentStream, typePreviewImageStreams);

        root.Flush(true);
    }

    //FamilyPreviewImage
    private static void WriteFamilyPreview(Storage storage, Stream documentStream, Stream previewImageStream)
    {
        // Remove all previously inserted preview images.
        storage.Delete("FamilyPreviewImage");

        BuildPreviewImageStream(storage, "FamilyPreviewImage", previewImageStream);
        documentStream.Position = 0;
    }

    private static void BuildPreviewImageStream(Storage storage, string name, Stream stream)
    {
        using var previewStream = storage.CreateStream(name);
        stream.CopyTo(previewStream);
        previewStream.Flush();
        stream.Position = 0;
    }

    private static void WriteTypePreviews(Storage storage, Stream documentStream, IDictionary<string, Stream> typePreviewImageStreams)
    {
        foreach (var (name, stream) in typePreviewImageStreams)
        {
            BuildPreviewImageStream(storage, name, stream);
        }

        documentStream.Position = 0;
    }

    private static void WriteRevitFamilyPreview(RootStorage root, Stream documentStream, Stream imageStream)
    {
        // TODO: Need to write pre and post fix. This is not needed for the family manager.
#if false        
        root.Delete("RevitPreview4.0");

        // Add the new family info.
        var stream = root.CreateStream("RevitPreview4.0");

        imageStream.CopyTo(stream);
        stream.Flush();

        root.Flush(true);

        imageStream.Position = 0;
        documentStream.Position = 0;
#endif
    }
}
