using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Bim.FamilyManager.Abstractions;

namespace Bim.FamilyManager.Base.Logic.EStorage;

/// <summary>
///     Abstract base class for managing Revit Extensible Storage schemas.
///     Provides methods to attach, detach, and retrieve custom data from Revit elements using a schema.
/// </summary>
public abstract class EStorageSchema
{
    private readonly Schema _schema;
    private readonly string _schemaDataFieldName;
    private readonly Guid _schemaId;

    private readonly string _schemaName;
    private readonly string _vendorId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EStorageSchema" /> class.
    /// </summary>
    /// <param name="schemaId">The unique identifier for the schema.</param>
    /// <param name="vendorId">The vendor ID for the schema.</param>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="schemaDataFieldName">The name of the data field in the schema.</param>
    protected EStorageSchema(Guid schemaId, string vendorId, string schemaName, string schemaDataFieldName)
    {
        _schemaId = schemaId;
        _vendorId = vendorId;
        _schemaName = schemaName;
        _schemaDataFieldName = schemaDataFieldName;

        _schema ??= GetSchema() ?? CreateSchema();
    }

    /// <summary>
    ///     Attaches the specified data to the given Revit element using the schema.
    /// </summary>
    /// <typeparam name="TData">The type of data to attach.</typeparam>
    /// <param name="element">The Revit element to attach data to.</param>
    /// <param name="data">The data to attach.</param>
    public void Attach<TData>(Element element, TData data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data);
        var entity = new Entity(_schema);
        entity.Set<IList<byte>>(_schemaDataFieldName, bytes);
        element.SetEntity(entity);
    }

    /// <summary>
    ///     Detaches the schema data from the specified Revit element.
    /// </summary>
    /// <param name="element">The Revit element to detach data from.</param>
    public void Detach(Element element)
    {
        var entity = element.GetEntity(_schema);
        entity?.Clear(_schemaDataFieldName);
    }

    /// <summary>
    ///     Attempts to retrieve the schema data from the specified Revit element.
    /// </summary>
    /// <typeparam name="TData">The type of data to retrieve.</typeparam>
    /// <param name="element">The Revit element to retrieve data from.</param>
    /// <param name="data">When this method returns, contains the retrieved data if successful; otherwise, the default value.</param>
    /// <returns><c>true</c> if the data was successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGet<TData>(Element element, [NotNullWhen(true)] out TData? data)
    {
        data = default;
        var entity = element.GetEntity(_schema);

        if (entity == null || !entity.IsValid() || entity.SchemaGUID != _schemaId)
        {
            return false;
        }

        var dataBytes = entity.Get<IList<byte>>(_schema.GetField(_schemaDataFieldName)).ToArray();
        data = JsonSerializer.Deserialize<TData>(new ReadOnlySpan<byte>(dataBytes));

        return data != null;
    }

    /// <summary>
    ///     Creates a new schema with the specified configuration.
    /// </summary>
    /// <returns>The created <see cref="Schema" /> instance.</returns>
    private Schema CreateSchema()
    {
        var schemaBuilder = new SchemaBuilder(_schemaId);

        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
        schemaBuilder.SetVendorId(_vendorId);
        schemaBuilder.SetSchemaName(_schemaName);
        schemaBuilder.AddArrayField(_schemaDataFieldName, typeof(byte));
        schemaBuilder.SetApplicationGUID(Constants.ApplicationId);

        return schemaBuilder.Finish();
    }

    /// <summary>
    ///     Retrieves the schema instance if it exists.
    /// </summary>
    /// <returns>The existing <see cref="Schema" /> instance, or <c>null</c> if not found.</returns>
    private Schema? GetSchema()
    {
        return Schema.ListSchemas()
                     .FirstOrDefault(item => item.GUID == _schemaId);
    }
}
