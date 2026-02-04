using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Bim.FamilyManager.Base.Logic;

public class Helper
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
    /// <remarks>
    ///     This method initializes a new <see cref="BitmapImage" />, sets its <see cref="BitmapImage.UriSource" /> to the
    ///     provided URI,
    ///     and completes the initialization process. It is used to load images for display in the application.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if the <paramref name="uri" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the image resource cannot be loaded from the specified <paramref name="uri" />.
    /// </exception>
    public static BitmapImage LoadImage(Uri uri)
    {
        // Create a BitmapImage and set its UriSource
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = uri;
        bitmapImage.EndInit();
        return bitmapImage;
    }

    public static ImageSource CreateBitmapFromStream(Stream preview, Color? transparentColor)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = preview;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze(); // Freeze the bitmap for thread safety

        if (transparentColor is null)
        {
            return bitmap;
        }

        // Create a WriteableBitmap to modify the pixels
        var writeableBitmap = new WriteableBitmap(bitmap);

        // Get the pixel data
        var width = writeableBitmap.PixelWidth;
        var height = writeableBitmap.PixelHeight;
        var stride = width * (writeableBitmap.Format.BitsPerPixel / 8);
        var pixels = new byte[height * stride];
        writeableBitmap.CopyPixels(pixels, stride, 0);

        // Iterate through the pixels and make the white background transparent
        for (var i = 0; i < pixels.Length; i += 4)
        {
            var blue = pixels[i];
            var green = pixels[i + 1];
            var red = pixels[i + 2];
            var alpha = pixels[i + 3];

            // Check if the pixel matches the transparent color
            if (red == transparentColor.Value.R && green == transparentColor.Value.G && blue == transparentColor.Value.B)
            {
                // Set alpha to 0 (transparent)
                pixels[i + 3] = 0;
            }
        }

        // Write the modified pixels back to the WriteableBitmap
        writeableBitmap.WritePixels(
            new Int32Rect(0, 0, width, height),
            pixels,
            stride,
            0);

        writeableBitmap.Freeze();
        return writeableBitmap;
    }
}
