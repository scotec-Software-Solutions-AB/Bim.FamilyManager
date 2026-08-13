using Bim.FamilyManager.Core.Abstractions;

namespace Bim.FamilyManager.Core.Logic;

/// <summary>
///     An <see cref="IFamilyNameFilter" /> that accepts family names containing
///     a fixed pattern using a case-insensitive ordinal comparison.
/// </summary>
public sealed class ContainsFamilyNameFilter : IFamilyNameFilter
{
    private readonly string _pattern;

    /// <summary>
    ///     Initializes a new instance of <see cref="ContainsFamilyNameFilter" />.
    /// </summary>
    /// <param name="pattern">The non-empty substring to search for.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="pattern" /> is <see langword="null" /> or empty.
    /// </exception>
    public ContainsFamilyNameFilter(string pattern)
    {
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        _pattern = pattern;
    }

    /// <inheritdoc />
    public bool IsMatch(string familyName)
        => familyName.Contains(_pattern, StringComparison.OrdinalIgnoreCase);
}
