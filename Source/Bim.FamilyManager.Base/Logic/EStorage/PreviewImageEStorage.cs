using Autodesk.Revit.DB;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace Bim.FamilyManager.Base.Logic.EStorage;

/// <summary>
///     Represents a serializable EStorage schema for storing family metadata in Revit elements.
/// </summary>
public class PreviewImageEStorage : EStorageSchema
{
    protected const string PreviewImageFieldName = "PreviewImage";


    private const string SchemaName = "Bim_FamilyManager_PreviewImage_V1";

    /// <summary>
    ///     The vendor identifier for the schema.
    /// </summary>
    private const string VendorId = "BIM.FamilyManager";

    /// <summary>
    ///     The unique identifier for the schema.
    /// </summary>
    private static readonly Guid SchemaId = new("E179ABFC-840A-4AC2-B9F9-47DF6BE6E1EF");

    public PreviewImageEStorage() : base(SchemaId, VendorId, SchemaName, new Dictionary<string, Type>
                                        {
                                            { PreviewImageFieldName, typeof(IDictionary<string, string>) }
                                        })
    {
    }

    public void Attach(Element element, IDictionary<string, Stream> data)
    {
        var base64Data = data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ConvertStreamToBase64());
        
        base.Attach(element, PreviewImageFieldName, (IDictionary<string, string>)base64Data);
    }

    public virtual void Detach(Element element)
    {
        base.Detach(element, PreviewImageFieldName);
    }


    public bool TryGet(Element element, [NotNullWhen(true)] out IDictionary<string, Stream>? data)
    {
        if (base.TryGet(element, PreviewImageFieldName, out IDictionary<string, string>? base64Data))
        {
            data = base64Data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ConvertBase64ToStream());
            return true;
        }

        data = null;
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