namespace Bim.FamilyManager.Core.Abstractions.Options;

/// <summary>
///     Represents a factory delegate for creating <see cref="IFamilySourceOptions" /> instances
///     resolved by a unique string key from the dependency injection container.
/// </summary>
/// <param name="key">A unique string key identifying the family source options to create.</param>
/// <returns>An instance of <see cref="IFamilySourceOptions" /> corresponding to the specified key.</returns>
public delegate IFamilySourceOptions FamilySourceOptionsFactory(string key);
