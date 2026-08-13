using System.IO;
using Bim.FamilyManager.Core.Abstractions.Descriptors;

namespace Bim.FamilyManager.Core.Abstractions.Descriptors;

/// <summary>
///     Defines serialization and deserialization operations for family descriptor objects using YAML format.
/// </summary>
public interface IDescriptorYamlSerializer
{
    /// <summary>
    ///     Deserializes a YAML string into a descriptor of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The descriptor type. Must implement <see cref="IItemDescriptor" />.</typeparam>
    /// <param name="yaml">The YAML string to deserialize.</param>
    /// <returns>A deserialized instance of <typeparamref name="T" />.</returns>
    T Deserialize<T>(string yaml) where T : IItemDescriptor;

    /// <summary>
    ///     Serializes a descriptor object into a YAML string.
    /// </summary>
    /// <typeparam name="T">The descriptor type. Must implement <see cref="IItemDescriptor" />.</typeparam>
    /// <param name="descriptor">The descriptor to serialize.</param>
    /// <returns>A YAML string representing the descriptor.</returns>
    string Serialize<T>(T descriptor) where T : IItemDescriptor;

    /// <summary>
    ///     Deserializes a YAML-encoded stream into a descriptor of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The descriptor type. Must implement <see cref="IItemDescriptor" />.</typeparam>
    /// <param name="stream">The stream containing YAML content.</param>
    /// <returns>A deserialized instance of <typeparamref name="T" />.</returns>
    T DeserializeFromStream<T>(Stream stream) where T : IItemDescriptor;

    /// <summary>
    ///     Serializes a descriptor object and writes it to the given stream as YAML.
    /// </summary>
    /// <typeparam name="T">The descriptor type. Must implement <see cref="IItemDescriptor" />.</typeparam>
    /// <param name="descriptor">The descriptor to serialize.</param>
    /// <param name="stream">The target stream. Stream position is reset to 0 after writing.</param>
    void SerializeToStream<T>(T descriptor, Stream stream) where T : IItemDescriptor;
}
