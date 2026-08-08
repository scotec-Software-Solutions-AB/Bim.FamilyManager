using System.IO;
using System.Text;
using Bim.FamilyManager.Core.Abstractions.Descriptors;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Bim.FamilyManager.Core.Descriptors;

/// <summary>
///     Serializes and deserializes family descriptor objects to and from YAML format.
/// </summary>
public class DescriptorYamlSerializer : IDescriptorYamlSerializer
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
                                                   .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                   .Build();

    private readonly ISerializer _serializer = new SerializerBuilder()
                                               .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                               .Build();

    /// <inheritdoc />
    public T Deserialize<T>(string yaml) where T : IItemDescriptor
    {
        return _deserializer.Deserialize<T>(yaml);
    }

    /// <inheritdoc />
    public string Serialize<T>(T descriptor) where T : IItemDescriptor
    {
        return _serializer.Serialize(descriptor);
    }

    /// <inheritdoc />
    public T DeserializeFromStream<T>(Stream stream) where T : IItemDescriptor
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var yaml = reader.ReadToEnd();
        return Deserialize<T>(yaml);
    }

    /// <inheritdoc />
    public void SerializeToStream<T>(T descriptor, Stream stream) where T : IItemDescriptor
    {
        var yaml = Serialize(descriptor);
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(yaml);
        writer.Flush();
        stream.Position = 0;
    }
}
