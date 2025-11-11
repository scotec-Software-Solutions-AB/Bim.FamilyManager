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
}
