using System.Text.Json;
using System.Text.Json.Serialization;
using Bim.FamilyManager.Abstractions.Options;

namespace Bim.FamilyManager.Base.Settings;

/// <summary>
///     Provides a custom JSON converter for handling serialization and deserialization of
///     <see cref="Abstractions.Options.IFamilySourceOptions" /> objects.
/// </summary>
/// <remarks>
///     This converter supports multiple implementations of
///     <see cref="Abstractions.Options.IFamilySourceOptions" />,
///     such as "DirectorySource" and "DatabaseSource". The converter determines the specific type of the object
///     based on the "Type" property in the JSON structure and deserializes it accordingly.
/// </remarks>
public class JsonFamilySourceOptionsConverter : JsonConverter<IFamilySourceOptions>
{
    private readonly Dictionary<string, Type> _sourceOptionsTypes;

    public JsonFamilySourceOptionsConverter(Dictionary<string, Type> sourceOptionsTypes)
    {
        _sourceOptionsTypes = sourceOptionsTypes;
    }

    /// <summary>
    ///     Reads and deserializes a JSON representation of an
    ///     <see cref="Abstractions.Options.IFamilySourceOptions" /> object.
    /// </summary>
    /// <param name="reader">
    ///     The <see cref="Utf8JsonReader" /> to read the JSON data from.
    /// </param>
    /// <param name="typeToConvert">
    ///     The type of the object to convert. This parameter is not used in this implementation.
    /// </param>
    /// <param name="options">
    ///     The <see cref="JsonSerializerOptions" /> to use for deserialization.
    /// </param>
    /// <returns>
    ///     An instance of <see cref="Abstractions.Options.IFamilySourceOptions" /> deserialized from the
    ///     JSON data.
    /// </returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the "Type" property in the JSON data specifies an unsupported source type.
    /// </exception>
    /// <remarks>
    ///     This method determines the specific implementation of
    ///     <see cref="Abstractions.Options.IFamilySourceOptions" />
    ///     to instantiate based on the "Type" property in the JSON structure. Supported types include "DirectorySource" and
    ///     "DatabaseSource".
    /// </remarks>
    public override IFamilySourceOptions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        var type = root.GetProperty("Type").GetString()!;

        if (_sourceOptionsTypes.TryGetValue(type, out var familySourceOptionsType))
        {
            return (IFamilySourceOptions)JsonSerializer.Deserialize(root.GetRawText(), familySourceOptionsType, options)!;
        }

        throw new NotSupportedException($"Source type '{type}' is not supported.");
    }

    /// <summary>
    ///     Writes a JSON representation of an <see cref="Abstractions.Options.IFamilySourceOptions" />
    ///     object.
    /// </summary>
    /// <param name="writer">
    ///     The <see cref="Utf8JsonWriter" /> to write the JSON data to.
    /// </param>
    /// <param name="value">
    ///     The <see cref="Abstractions.Options.IFamilySourceOptions" /> object to serialize.
    /// </param>
    /// <param name="options">
    ///     The <see cref="JsonSerializerOptions" /> to use for serialization.
    /// </param>
    /// <remarks>
    ///     This method serializes the provided <see cref="Abstractions.Options.IFamilySourceOptions" />
    ///     object
    ///     into its JSON representation, including its specific implementation type.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when the <paramref name="writer" /> or <paramref name="value" /> is <c>null</c>.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, IFamilySourceOptions value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
