using System.Diagnostics.CodeAnalysis;
using Autodesk.Revit.DB;

namespace Bim.FamilyManager.Base.Logic;

public class SerializableEStorage : EStorageSchema
{
    private const string SchemaDataFieldName = "FamilyMetadata";
    private const string SchemaName = "SerializableEStorage";
    private const string VendorId = "BIM-FamilyManager";
    private static readonly Guid SchemaId = new("9D245BBE-229B-41DA-88CC-F052FC7DB891");

    public SerializableEStorage()
        : base(SchemaId, VendorId, SchemaName, SchemaDataFieldName)
    {
    }
}

public static class EStorageExtensions
{
    private static readonly SerializableEStorage SerializableEStorage = new();

    public static void AttachData<TData>(this Element element, TData data)
    {
        SerializableEStorage.Attach(element, data);
    }

    public static void DetachData(this Element element)
    {
        SerializableEStorage.Detach(element);
    }

    public static bool TryGetData<TData>(this Element element, [NotNullWhen(true)] out TData? data)
    {
        return SerializableEStorage.TryGet(element, out data);
    }
}