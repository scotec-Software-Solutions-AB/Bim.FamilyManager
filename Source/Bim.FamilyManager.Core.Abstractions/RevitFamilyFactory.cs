using System.IO;
using Scotec.Revit.RevitFamily;

namespace Bim.FamilyManager.Core.Abstractions;

/// <summary>
///     Represents a factory delegate for creating <see cref="IRevitFamily" /> instances.
/// </summary>
/// <param name="name">The name of the Revit family.</param>
/// <param name="familyInfo">The <see cref="RevitFamilyInfo" /> containing metadata and configuration.</param>
/// <param name="saveAction">The action used to save the family to a stream.</param>
/// <returns>A new <see cref="IRevitFamily" /> instance.</returns>
public delegate IRevitFamily RevitFamilyFactory(string name, RevitFamilyInfo familyInfo, Action<IRevitFamily, Stream> saveAction);
