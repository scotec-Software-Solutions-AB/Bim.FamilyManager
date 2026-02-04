using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Autodesk.Revit.DB;

namespace Bim.FamilyManager.Base.Logic.EStorage;

/// <summary>
///     Represents a serializable EStorage schema for storing family metadata in Revit elements.
/// </summary>
public class FamilyMetadataEStorage : EStorageSchema
{
    protected const string FamilyMetadataFieldName = "FamilyMetadata";

    private const string SchemaName = "Bim_FamilyManager_FamilyMetadata_V1";

    /// <summary>
    ///     The vendor identifier for the schema.
    /// </summary>
    private const string VendorId = "BIM.FamilyManager";

    /// <summary>
    ///     The unique identifier for the schema.
    /// </summary>
    private static readonly Guid SchemaId = new("7DAED877-211A-41B8-BEF4-2CEE567D0C01");

    /// <summary>
    ///     The name of the schema.
    /// </summary>
    /// <summary>
    ///     Initializes a new instance of the <see cref="FamilyMetadataEStorage" /> class.
    /// </summary>
    public FamilyMetadataEStorage() : base(SchemaId, VendorId, SchemaName, new Dictionary<string, Type>
    {
        { FamilyMetadataFieldName, typeof(IList<byte>) }
    })
    {
    }

    public void Attach(Element element, FamilyMetadata data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data);

        Attach(element, new Dictionary<string, object>
        {
            { FamilyMetadataFieldName, bytes }
        });
    }

    public override void Detach(Element element)
    {
        base.Detach(element);
    }

    public bool TryGet(Element element, [NotNullWhen(true)] out FamilyMetadata? data)
    {
        data = null;
        if (base.TryGet(element, out var dataDictionary))
        {
            if (dataDictionary.TryGetValue(FamilyMetadataFieldName, out var value) && value is IList<byte> binaryData)
            {
                data = JsonSerializer.Deserialize<FamilyMetadata>(binaryData.ToArray());
            }
        }

        return data != null;
    }
}
