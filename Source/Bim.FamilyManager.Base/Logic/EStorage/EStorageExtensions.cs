using System.Diagnostics.CodeAnalysis;
using Autodesk.Revit.DB;

namespace Bim.FamilyManager.Base.Logic.EStorage;

/// <summary>
///     Provides extension methods for attaching, detaching, and retrieving serializable EStorage data from Revit elements.
/// </summary>
public static class EStorageExtensions
{
    //private static readonly FamilyMetadataEStorage SerializableEStorage = new();

    /// <summary>
    ///     Declares extension methods for the <see cref="Autodesk.Revit.DB.Element" /> type, enabling EStorage operations such
    ///     as attaching, detaching, and retrieving serializable data.
    /// </summary>
    /// <param name="element">The Revit element instance to extend with EStorage functionality.</param>
    extension(Element element)
    {
        /// <summary>
        ///     Attaches the specified data to this Revit element using the serializable EStorage schema.
        /// </summary>
        /// <typeparam name="TData">The type of data to attach.</typeparam>
        /// <param name="data">The data to attach to the element.</param>
        public void AttachData<TData>(TData data)
        {
            SerializableEStorage.Attach(element, data);
        }

        /// <summary>
        ///     Detaches the serializable EStorage data from this Revit element.
        /// </summary>
        public void DetachData()
        {
            SerializableEStorage.Detach(element);
        }

        /// <summary>
        ///     Attempts to retrieve the serializable EStorage data from this Revit element.
        /// </summary>
        /// <typeparam name="TData">The type of data to retrieve.</typeparam>
        /// <param name="data">When this method returns, contains the retrieved data if available; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if the data was successfully retrieved; otherwise, <c>false</c>.</returns>
        public bool TryGetData<TData>([NotNullWhen(true)] out TData? data)
        {
            return SerializableEStorage.TryGet(element, out data);
        }
    }
}
