namespace Bim.FamilyManager.Core.Abstractions.Options;

/// <summary>
///     Represents a factory delegate for creating <see cref="ILayoutOptions" /> instances
///     resolved by a unique string key from the dependency injection container.
/// </summary>
/// <param name="key">A unique string key identifying the layout options to create.</param>
/// <returns>An instance of <see cref="ILayoutOptions" /> corresponding to the specified key.</returns>
public delegate ILayoutOptions LayoutOptionsFactory(string key);
