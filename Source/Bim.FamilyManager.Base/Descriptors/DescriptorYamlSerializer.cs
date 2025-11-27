using System.IO;
using System.Text;
using Bim.FamilyManager.Abstractions.Descriptors;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Bim.FamilyManager.Base.Descriptors;

public static class DescriptorYamlSerializer
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
                                                         .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                         .Build();

    private static readonly ISerializer Serializer = new SerializerBuilder()
                                                     .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                     .Build();

    public static T Deserialize<T>(string yaml) where T : IItemDescriptor
    {
        return Deserializer.Deserialize<T>(yaml);
    }

    public static string Serialize<T>(T descriptor) where T : IItemDescriptor
    {
        return Serializer.Serialize(descriptor);
    }

    public static T DeserializeFromStream<T>(Stream stream) where T : IItemDescriptor
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var yaml = reader.ReadToEnd();
        return Deserialize<T>(yaml);
    }

    public static void SerializeToStream<T>(T descriptor, Stream stream) where T : IItemDescriptor
    {
        var yaml = Serialize(descriptor);
        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(yaml);
        writer.Flush();
        stream.Position = 0;
    }
}
