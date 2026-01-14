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
    private readonly Guid _schemaId;

    private readonly string _schemaName;
    private readonly IDictionary<string, Type> _fields;
    private readonly string _vendorId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EStorageSchema" /> class.
    /// </summary>
    protected EStorageSchema(Guid schemaId, string vendorId, string schemaName, Dictionary<string, Type> fields)
    {
        _schemaId = schemaId;
        _vendorId = vendorId;
        _schemaName = schemaName;
        _fields = fields;

        _schema ??= GetSchema() ?? CreateSchema();
    }

    /// <summary>
    ///     Attaches the specified data to the given Revit element using the schema.
    /// </summary>
    /// <typeparam name="TData">The type of data to attach.</typeparam>
    /// <param name="element">The Revit element to attach data to.</param>
    /// <param name="data">The data to attach.</param>
    public virtual void Attach<TData>(Element element, string fieldName, TData data)
    {
        var entity = new Entity(_schema);
        
        entity.Set(fieldName, data);
        element.SetEntity(entity);
    }

    /// <summary>
    ///     Detaches the schema data from the specified Revit element.
    /// </summary>
    /// <param name="element">The Revit element to detach data from.</param>
    public virtual void Detach(Element element, string fieldName)
    {
        var entity = element.GetEntity(_schema);
        entity?.Clear(fieldName);
    }

    /// <summary>
    ///     Attempts to retrieve the schema data from the specified Revit element.
    /// </summary>
    /// <typeparam name="TData">The type of data to retrieve.</typeparam>
    /// <param name="element">The Revit element to retrieve data from.</param>
    /// <param name="data">When this method returns, contains the retrieved data if successful; otherwise, the default value.</param>
    /// <returns><c>true</c> if the data was successfully retrieved; otherwise, <c>false</c>.</returns>
    public virtual bool TryGet<TData>(Element element, string fieldName, [NotNullWhen(true)] out TData? data)
    {
        data = default;
        var entity = element.GetEntity(_schema);

        if (entity == null || !entity.IsValid() || entity.SchemaGUID != _schemaId)
        {
            return false;
        }

        data = entity.Get<TData>(_schema.GetField(fieldName));
        
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
        
        foreach (var field in _fields)
        {
            var fieldName = field.Key;
            var type = field.Value;
            if (type.IsArray)
            {
                schemaBuilder.AddArrayField(fieldName, type.GetElementType());
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var elemType = type.GetGenericArguments()[0];
                schemaBuilder.AddArrayField(fieldName, elemType);   // IList<byte> -> byte
            }
            else
            {
                schemaBuilder.AddSimpleField(fieldName, type);
            }
        }
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
