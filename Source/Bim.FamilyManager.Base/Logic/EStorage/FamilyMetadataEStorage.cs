using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Bim.FamilyManager.Base.Logic.EStorage;

/// <summary>
///     Represents a serializable EStorage schema for storing family metadata in Revit elements.
/// </summary>
public class FamilyMetadataEStorage : EStorageSchema
{
    protected const string FamilyMetadataFieldName = "FamilyMetadata";
    
    private const string SchemaName = "EStorage.Bim.FamilyManager.FamilyMetadata";

    /// <summary>
    ///     The vendor identifier for the schema.
    /// </summary>
    private const string VendorId = "BIM.FamilyManager";

    /// <summary>
    ///     The unique identifier for the schema.
    /// </summary>
    private static readonly Guid SchemaId = new("9D245BBE-229B-41DA-88CC-F052FC7DB891");

    /// <summary>
    ///     The name of the schema.
    /// </summary>
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyMetadataEStorage" /> class.
    /// </summary>
    public FamilyMetadataEStorage() : base(SchemaId, VendorId, SchemaName, new Dictionary<string, Type>
                                        {
                                            { FamilyMetadataFieldName, typeof(byte[]) }
                                        })
    {
    }

    public void Attach(Element element, FamilyMetadata data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data);

        base.Attach(element, FamilyMetadataFieldName, bytes);
    }

    public virtual void Detach(Element element)
    {
        base.Detach(element, FamilyMetadataFieldName);
    }


    public bool TryGet(Element element, [NotNullWhen(true)] out FamilyMetadata? data)
    {
        base.TryGet(element, FamilyMetadataFieldName, out byte[]? binaryData);
        data = binaryData != null ? JsonSerializer.Deserialize<FamilyMetadata>(new ReadOnlySpan<byte>(binaryData)) : null;
        
        return data != null;
    }
}
