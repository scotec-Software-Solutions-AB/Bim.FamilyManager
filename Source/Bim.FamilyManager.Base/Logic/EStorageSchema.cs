using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Bim.FamilyManager.Abstractions;

namespace Bim.FamilyManager.Base.Logic;

public abstract class EStorageSchema
{
    private readonly Schema _schema;
    private readonly string _schemaDataFieldName;
    private readonly Guid _schemaId;

    private readonly string _schemaName;
    private readonly string _vendorId;

    protected EStorageSchema(Guid schemaId, string vendorId, string schemaName, string schemaDataFieldName)
    {
        _schemaId = schemaId;
        _vendorId = vendorId;
        _schemaName = schemaName;
        _schemaDataFieldName = schemaDataFieldName;

        _schema ??= GetSchema() ?? CreateSchema();
    }

    public void Attach<TData>(Element element, TData data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data);
        var entity = new Entity(_schema);
        entity.Set<IList<byte>>(_schemaDataFieldName, bytes);
        element.SetEntity(entity);
    }

    public void Detach(Element element)
    {
        var entity = element.GetEntity(_schema);
        entity?.Clear(_schemaDataFieldName);
    }

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

    private Schema? GetSchema()
    {
        return Schema.ListSchemas()
                     .FirstOrDefault(item => item.GUID == _schemaId);
    }
}
