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
    private static readonly Guid SchemaId = new("7D08197B-14B3-4DBA-9EC9-3CA7C223EC06");

    public PreviewImageEStorage() : base(SchemaId, VendorId, SchemaName, new Dictionary<string, Type>
                                        {
                                            { PreviewImageFieldName, typeof(IList<byte>) }
                                        })
    {
    }

    public void Attach(Element element, Stream data)
    {
        var memoryStream = new MemoryStream();
        data.CopyTo(memoryStream);

        base.Attach(element, PreviewImageFieldName, (IList<byte>)memoryStream.ToArray());
    }

    public virtual void Detach(Element element)
    {
        base.Detach(element, PreviewImageFieldName);
    }


    public bool TryGet(Element element, [NotNullWhen(true)] out MemoryStream? data)
    {
        base.TryGet(element, PreviewImageFieldName, out IList<byte>? binaryData);

        data = binaryData != null ? new MemoryStream(binaryData.ToArray()) : null;

        return data != null;
    }

}
