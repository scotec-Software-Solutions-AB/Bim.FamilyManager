namespace Bim.FamilyManager.Core.Abstractions;

/// <summary>
///     An <see cref="IFamilyNameFilter" /> that accepts every family name unconditionally.
/// </summary>
/// <remarks>
///     Use <see cref="Instance" /> wherever no name filtering is required, following the
///     Null Object pattern to avoid <see langword="null" /> checks at call sites and inside
///     family source implementations.
/// </remarks>
public sealed class NullFamilyNameFilter : IFamilyNameFilter
{
    private NullFamilyNameFilter() { }

    /// <summary>
    ///     Gets the singleton instance of <see cref="NullFamilyNameFilter" />.
    /// </summary>
    public static IFamilyNameFilter Instance { get; } = new NullFamilyNameFilter();

    /// <inheritdoc />
    /// <returns>Always <see langword="true" />.</returns>
    public bool IsMatch(string familyName) => true;
}
