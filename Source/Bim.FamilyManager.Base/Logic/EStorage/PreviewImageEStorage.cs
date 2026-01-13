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


    private const string SchemaName = "EStorage.Bim.FamilyManager.PreviewImage";

    /// <summary>
    ///     The vendor identifier for the schema.
    /// </summary>
    private const string VendorId = "BIM.FamilyManager";

    /// <summary>
    ///     The unique identifier for the schema.
    /// </summary>
    private static readonly Guid SchemaId = new("2C9DA88A-E3F7-4224-8358-208A70DBA79B");

    public PreviewImageEStorage() : base(SchemaId, VendorId, SchemaName, new Dictionary<string, Type>
                                        {
                                            { PreviewImageFieldName, typeof(byte[]) }
                                        })
    {
    }

    public void Attach(Element element, Stream data)
    {
        var memoryStream = new MemoryStream();
        data.CopyTo(memoryStream);

        base.Attach(element, PreviewImageFieldName, memoryStream.ToArray());
    }

    public virtual void Detach(Element element)
    {
        base.Detach(element, PreviewImageFieldName);
    }


    public bool TryGet(Element element, [NotNullWhen(true)] out MemoryStream? data)
    {
        base.TryGet(element, PreviewImageFieldName, out byte[]? binaryData);

        data = binaryData != null ? new MemoryStream(binaryData) : null;

        return data != null;
    }

}
