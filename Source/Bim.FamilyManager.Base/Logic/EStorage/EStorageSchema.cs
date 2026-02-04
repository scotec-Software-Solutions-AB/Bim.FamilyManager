using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
    private static readonly MethodInfo SetMethodInfo;
    private static readonly MethodInfo GetMethodInfo;
    private readonly IDictionary<string, Type> _fields;

    private readonly Schema _schema;
    private readonly Guid _schemaId;

    private readonly string _schemaName;
    private readonly string _vendorId;

    static EStorageSchema()
    {
        GetMethodInfo = typeof(Entity).GetMethod("Get", [typeof(string)])!;
        SetMethodInfo = typeof(Entity).GetMethods()
                                      .Where(m => m is { Name: "Set", IsGenericMethod: true })
                                      .FirstOrDefault(m =>
                                      {
                                          var parameters = m.GetParameters();
                                          // Ensure the method has exactly two parameters: string and T (generic)
                                          return parameters.Length == 2 &&
                                                 parameters[0].ParameterType == typeof(string) &&
                                                 parameters[1].ParameterType.IsGenericParameter;
                                      })!;
    }

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
    ///     Detaches the schema data from the specified Revit element.
    /// </summary>
    /// <param name="element">The Revit element to detach data from.</param>
    public virtual void Detach(Element element /*, string fieldName*/)
    {
        //var entity = element.GetEntity(_schema);
        element.DeleteEntity(_schema);
        //entity?.Clear(fieldName);
    }

    protected virtual void Attach(Element element, IDictionary<string, object> data)
    {
        var entity = new Entity(_schema);

        foreach (var (fieldName, value) in data)
        {
            var genericMethod = SetMethodInfo.MakeGenericMethod(_fields[fieldName]);
            genericMethod.Invoke(entity, [fieldName, value]);
        }

        element.SetEntity(entity);
    }

    protected virtual bool TryGet(Element element, [NotNullWhen(true)] out IDictionary<string, object>? dataDictionary)
    {
        dataDictionary = null;
        var entity = element.GetEntity(_schema);

        if (entity == null || !entity.IsValid() || entity.SchemaGUID != _schemaId)
        {
            return false;
        }

        dataDictionary = new Dictionary<string, object>();
        foreach (var (fieldName, fieldType) in _fields)
        {
            var method = GetMethodInfo.MakeGenericMethod(fieldType);

            var value = method.Invoke(entity, [fieldName]);
            if (value is not null)
            {
                dataDictionary[fieldName] = value;
            }
        }

        return true;
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
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];
                schemaBuilder.AddMapField(fieldName, keyType, valueType);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var elemType = type.GetGenericArguments()[0];
                schemaBuilder.AddArrayField(fieldName, elemType); // IList<byte> -> byte
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
