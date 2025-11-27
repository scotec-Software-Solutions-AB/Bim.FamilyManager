namespace Bim.FamilyManager.Base.Logic.EStorage;

/// <summary>
///     Represents a serializable EStorage schema for storing family metadata in Revit elements.
/// </summary>
public class SerializableEStorage : EStorageSchema
{
    /// <summary>
    ///     The name of the data field used to store family metadata.
    /// </summary>
    private const string SchemaDataFieldName = "FamilyMetadata";

    /// <summary>
    ///     The name of the schema.
    /// </summary>
    private const string SchemaName = "SerializableEStorage";

    /// <summary>
    ///     The vendor identifier for the schema.
    /// </summary>
    private const string VendorId = "BIM-FamilyManager";

    /// <summary>
    ///     The unique identifier for the schema.
    /// </summary>
    private static readonly Guid SchemaId = new("9D245BBE-229B-41DA-88CC-F052FC7DB891");

    /// <summary>
    ///     Initializes a new instance of the <see cref="SerializableEStorage" /> class.
    /// </summary>
    public SerializableEStorage()
        : base(SchemaId, VendorId, SchemaName, SchemaDataFieldName)
    {
    }
}
