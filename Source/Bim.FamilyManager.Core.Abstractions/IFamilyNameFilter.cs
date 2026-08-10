namespace Bim.FamilyManager.Core.Abstractions;

/// <summary>
///     Defines a filter that tests whether a family name satisfies a condition.
/// </summary>
/// <remarks>
///     Pass an <see cref="IFamilyNameFilter" /> to
///     <see cref="IFolder.GetFamiliesAsync(bool, IFamilyNameFilter, CancellationToken)" />
///     to have family sources skip non-matching entries before allocating
///     <see cref="IRevitFamily" /> instances.
///     Use <see cref="NullFamilyNameFilter.Instance" /> where no filtering is required.
/// </remarks>
public interface IFamilyNameFilter
{
    /// <summary>
    ///     Returns <see langword="true" /> when <paramref name="familyName" /> satisfies this filter.
    /// </summary>
    /// <param name="familyName">The family name to test.</param>
    bool IsMatch(string familyName);
}
