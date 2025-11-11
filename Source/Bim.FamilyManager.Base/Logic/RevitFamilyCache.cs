using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Bim.FamilyManager.Abstractions;

namespace Bim.FamilyManager.Base.Logic;

/// <summary>
///     Represents a cache for managing Revit family instances.
/// </summary>
/// <remarks>
///     This class provides functionality to store, retrieve, and update Revit family instances.
///     It ensures thread-safe operations using a concurrent dictionary and allows for custom actions
///     to be executed when a family is added or updated in the cache.
/// </remarks>
/// <seealso cref="Bim.FamilyManager.Abstractions.IRevitFamily" />
public class RevitFamilyCache
{
    private readonly Action<IRevitFamily> _addedAction;
    private readonly ConcurrentDictionary<string, IRevitFamily> _familyCache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitFamilyCache" /> class.
    /// </summary>
    /// <param name="addedAction">
    ///     An action to be executed when a new <see cref="IRevitFamily" /> is added to the cache.
    ///     This action can be used to perform additional processing or initialization of the family.
    /// </param>
    /// <remarks>
    ///     The <see cref="RevitFamilyCache" /> class provides a mechanism to manage Revit family instances efficiently.
    ///     This constructor allows specifying a custom action to handle newly added families.
    /// </remarks>
    public RevitFamilyCache(Action<IRevitFamily> addedAction)
    {
        _addedAction = addedAction;
    }

    /// <summary>
    ///     Gets the collection of Revit families stored in the cache.
    /// </summary>
    /// <value>
    ///     An <see cref="IEnumerable{T}" /> of <see cref="Bim.FamilyManager.Abstractions.IRevitFamily" />
    ///     representing the families currently stored in the cache.
    /// </value>
    /// <remarks>
    ///     This property provides access to all Revit families managed by the cache.
    ///     The collection is thread-safe and reflects the current state of the cache.
    /// </remarks>
    public IEnumerable<IRevitFamily> Families => _familyCache.Values;

    /// <summary>
    ///     Attempts to retrieve a Revit family from the cache by its name.
    /// </summary>
    /// <param name="name">
    ///     The name of the Revit family to retrieve.
    /// </param>
    /// <param name="family">
    ///     When this method returns, contains the <see cref="IRevitFamily" /> instance associated with the specified name,
    ///     if the name is found; otherwise, <see langword="null" />. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the Revit family with the specified name exists in the cache; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <remarks>
    ///     This method checks the internal cache for a Revit family with the specified name. If found, the corresponding
    ///     <see cref="IRevitFamily" /> instance is returned via the <paramref name="family" /> parameter.
    /// </remarks>
    public bool TryGetFamily(string name, [MaybeNullWhen(false)] out IRevitFamily family)
    {
        return _familyCache.TryGetValue(name, out family);
    }

    /// <summary>
    ///     Adds a new Revit family to the cache or updates an existing one.
    /// </summary>
    /// <param name="name">The unique name of the Revit family to add or update.</param>
    /// <param name="family">The <see cref="IRevitFamily" /> instance to be added or updated in the cache.</param>
    /// <param name="updateValueFactory">
    ///     A factory method that provides a mechanism to update the existing family in the cache.
    ///     This function takes the family name and the existing family instance as parameters and returns the updated family.
    /// </param>
    /// <remarks>
    ///     This method ensures thread-safe addition or update of Revit families in the cache.
    ///     If a family with the specified name already exists, the <paramref name="updateValueFactory" /> is used to determine
    ///     the updated value.
    ///     Additionally, the action specified during the initialization of the <see cref="RevitFamilyCache" /> is invoked for
    ///     the added or updated family.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="name" />, <paramref name="family" />, or <paramref name="updateValueFactory" /> is
    ///     <c>null</c>.
    /// </exception>
    public void AddOrUpdateFamily(string name, IRevitFamily family, Func<string, IRevitFamily, IRevitFamily> updateValueFactory)
    {
        _familyCache.AddOrUpdate(name, family, updateValueFactory);
        _addedAction.Invoke(family);
    }

    /// <summary>
    ///     Clears all entries from the Revit family cache.
    /// </summary>
    /// <remarks>
    ///     This method removes all cached <see cref="IRevitFamily" /> instances from the internal storage.
    ///     It is typically used to reset the cache, ensuring that no previously stored families remain.
    /// </remarks>
    public void Clear()
    {
        _familyCache.Clear();
    }
}
