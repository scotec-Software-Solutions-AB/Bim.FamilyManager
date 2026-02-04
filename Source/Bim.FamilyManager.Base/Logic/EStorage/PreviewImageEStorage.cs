using System.Diagnostics.CodeAnalysis;
using System.IO;
using Autodesk.Revit.DB;

namespace Bim.FamilyManager.Base.Logic.EStorage;

/// <summary>
///     Represents a serializable EStorage schema for storing family metadata in Revit elements.
/// </summary>
public class PreviewImageEStorage : EStorageSchema
{
    protected const string FieldNameFamilyPreviewImageName = "FamilyPreviewImageName";
    protected const string FieldNameTypePreviewImages = "TypePreviewImages";

    private const string SchemaName = "Bim_FamilyManager_PreviewImages_V1";

    /// <summary>
    ///     The vendor identifier for the schema.
    /// </summary>
    private const string VendorId = "BIM.FamilyManager";

    /// <summary>
    ///     The unique identifier for the schema.
    /// </summary>
    private static readonly Guid SchemaId = new("1F9E97BA-765D-43B8-9100-6FDE3FE3114A");

    public PreviewImageEStorage() : base(SchemaId, VendorId, SchemaName, new Dictionary<string, Type>
    {
        { FieldNameFamilyPreviewImageName, typeof(string) },
        { FieldNameTypePreviewImages, typeof(IDictionary<string, string>) }
    })
    {
    }

    public void Attach(Element element, string familyPreviewImageName, IDictionary<string, Stream> typePreviews)
    {
        var base64Data = typePreviews.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ConvertStreamToBase64());

        var dataDictionary = new Dictionary<string, object>
        {
            { FieldNameFamilyPreviewImageName, familyPreviewImageName },
            { FieldNameTypePreviewImages, base64Data }
        };

        Attach(element, dataDictionary);
    }

    public override void Detach(Element element)
    {
        base.Detach(element);
    }

    public bool TryGet(Element element, [NotNullWhen(true)] out string? familyPreviewImageName,
                       [NotNullWhen(true)] out IDictionary<string, Stream>? typePreviews)
    {
        if (base.TryGet(element, out var dataDictionary))
        {
            if (dataDictionary.TryGetValue(FieldNameFamilyPreviewImageName, out var familyPreviewNameImageObject) &&
                familyPreviewNameImageObject is string familyPreviewImageNameValue
                && dataDictionary.TryGetValue(FieldNameTypePreviewImages, out var base64DataObject) &&
                base64DataObject is IDictionary<string, string> base64Data)
            {
                familyPreviewImageName = familyPreviewImageNameValue;
                typePreviews = base64Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ConvertBase64ToStream());
                return true;
            }
        }

        familyPreviewImageName = null;
        typePreviews = null;
        return false;
    }
}

public static class Base64Extensions
{
    public static string ConvertStreamToBase64(this Stream stream)
    {
        if (stream is not MemoryStream memoryStream)
        {
            memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static Stream ConvertBase64ToStream(this string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        var stream = new MemoryStream(bytes);

        stream.Position = 0;

        return stream;
    }
}
