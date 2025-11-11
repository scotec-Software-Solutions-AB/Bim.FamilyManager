namespace Bim.FamilyManager.Abstractions.Settings;

/// <summary>
///     Represents an attribute used to define a settings section for a class.
/// </summary>
/// <remarks>
///     This attribute is applied to classes to associate them with a specific settings section.
///     The section name is specified during the attribute's initialization and can be accessed
///     via the <see cref="SectionName" /> property.
/// </remarks>
/// <example>
///     The following example demonstrates how to use the <see cref="SettingsSectionAttribute" />:
///     <code>
/// [SettingsSection("ExampleSection")]
/// public class ExampleOptions
/// {
///     public string Option { get; set; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SettingsSectionAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsSectionAttribute" /> class with the specified section name.
    /// </summary>
    /// <param name="sectionName">
    ///     The name of the settings section associated with the class. This value is used to identify
    ///     and group related settings.
    /// </param>
    /// <remarks>
    ///     The <paramref name="sectionName" /> parameter is required and must be a non-empty string.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="sectionName" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when <paramref name="sectionName" /> is an empty string.
    /// </exception>
    public SettingsSectionAttribute(string sectionName)
    {
        SectionName = sectionName;
    }

    /// <summary>
    ///     Gets the name of the settings section associated with the attribute.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the settings section.
    /// </value>
    /// <remarks>
    ///     This property is initialized through the constructor of the <see cref="SettingsSectionAttribute" /> class
    ///     and provides a way to retrieve the section name specified during the attribute's creation.
    /// </remarks>
    public string SectionName { get; }
}
