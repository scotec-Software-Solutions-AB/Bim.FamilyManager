using System.IO;
using Autodesk.Revit.DB;

namespace Bim.FamilyManager.Core.Abstractions;

/// <summary>
///     Defines the contract for exporting a Revit view as a PNG image stream.
/// </summary>
public interface IViewImageExporter
{
    /// <summary>
    ///     Exports the specified Revit <paramref name="view" /> from <paramref name="document" /> as a PNG
    ///     <see cref="Stream" />.
    /// </summary>
    /// <param name="document">The Revit document containing the view.</param>
    /// <param name="view">The view to export.</param>
    /// <param name="pixelSize">The pixel dimension of the exported image. Defaults to 256.</param>
    /// <returns>A <see cref="Stream" /> containing the PNG image data, positioned at the beginning.</returns>
    Stream ExportViewPng(Document document, View view, int pixelSize = 256);
}
