namespace Bim.FamilyManager.Base.Options;

/// <summary>
///     Specifies layout options for a class in the Revit Family Manager.
/// </summary>
/// <remarks>
///     This attribute is used to associate a class with specific layout options by defining an options name.
///     It is applied to classes and is not inherited by derived classes.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LayoutOptionsAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the name of the layout options associated with the class.
    /// </summary>
    /// <remarks>
    ///     This property is used to uniquely identify the layout options for a class.
    ///     It is primarily utilized in scenarios where layout configurations need to be
    ///     retrieved or managed dynamically, such as in the <see cref="SettingsManager.GetLayoutOptionTypes" /> method.
    /// </remarks>
    public required string OptionsName { get; set; }
}
