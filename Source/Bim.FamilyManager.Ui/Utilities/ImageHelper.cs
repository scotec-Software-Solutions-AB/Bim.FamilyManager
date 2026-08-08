using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bim.FamilyManager.Ui.Utilities;

/// <summary>
///     Provides utility methods for loading and converting images for display in the WPF UI.
/// </summary>
internal static class ImageHelper
{
    /// <summary>
    ///     Loads an image from the specified URI and returns it as a <see cref="BitmapImage" />.
    /// </summary>
    /// <param name="uri">
    ///     The <see cref="Uri" /> of the image to be loaded. This must be an absolute URI pointing to the image resource.
    /// </param>
    /// <returns>
    ///     A <see cref="BitmapImage" /> instance representing the loaded image.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="uri" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the image resource cannot be loaded from the specified <paramref name="uri" />.
    /// </exception>
    public static BitmapImage LoadImage(Uri uri)
    {
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = uri;
        bitmapImage.EndInit();
        return bitmapImage;
    }

    /// <summary>
    ///     Creates an <see cref="ImageSource" /> from the provided stream, optionally replacing a specific color with
    ///     transparency.
    /// </summary>
    /// <param name="preview">The <see cref="Stream" /> containing the raw image data.</param>
    /// <param name="transparentColor">
    ///     An optional <see cref="Color" /> whose pixels will be made fully transparent. If <c>null</c>, no transparency
    ///     processing is applied.
    /// </param>
    /// <returns>
    ///     A frozen <see cref="ImageSource" /> ready for use on any thread.
    /// </returns>
    public static ImageSource CreateBitmapFromStream(Stream preview, Color? transparentColor)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = preview;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();

        if (transparentColor is null)
        {
            return bitmap;
        }

        var writeableBitmap = new WriteableBitmap(bitmap);

        var width = writeableBitmap.PixelWidth;
        var height = writeableBitmap.PixelHeight;
        var stride = width * (writeableBitmap.Format.BitsPerPixel / 8);
        var pixels = new byte[height * stride];
        writeableBitmap.CopyPixels(pixels, stride, 0);

        for (var i = 0; i < pixels.Length; i += 4)
        {
            var blue = pixels[i];
            var green = pixels[i + 1];
            var red = pixels[i + 2];

            if (red == transparentColor.Value.R && green == transparentColor.Value.G && blue == transparentColor.Value.B)
            {
                pixels[i + 3] = 0;
            }
        }

        writeableBitmap.WritePixels(
            new Int32Rect(0, 0, width, height),
            pixels,
            stride,
            0);

        writeableBitmap.Freeze();
        return writeableBitmap;
    }
}
