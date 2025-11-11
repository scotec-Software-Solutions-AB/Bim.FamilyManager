using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Point = System.Drawing.Point;

namespace Bim.FamilyManager.Ui;

/// <summary>
///     Provides interop functionality for interacting with Windows API and managing window-related operations.
/// </summary>
/// <remarks>
///     This class includes methods for retrieving window rectangles and mouse positions, leveraging Windows API calls.
///     It is designed for internal use within the application and is not intended for external consumption.
/// </remarks>
public class Interop
{
    /// <summary>
    ///     Retrieves the bounding rectangle of the specified WPF <see cref="Window" />.
    /// </summary>
    /// <param name="window">
    ///     The <see cref="Window" /> instance for which the rectangle is to be retrieved.
    /// </param>
    /// <param name="rectangle">
    ///     When this method returns, contains the bounding rectangle of the specified window,
    ///     represented as a <see cref="System.Drawing.Rectangle" />. This parameter is passed uninitialized.
    /// </param>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the window rectangle cannot be retrieved.
    /// </exception>
    /// <remarks>
    ///     This method uses Windows API calls to retrieve the rectangle of the specified window.
    ///     It is intended for internal use within the application.
    /// </remarks>
    public static void GetWindowRect(Window window, out Rectangle rectangle)
    {
        if (PInvoke.GetWindowRect(GetWindowHandle(window), out var rect))
        {
            rectangle = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
            return;
        }

        throw new InvalidOperationException("Unable to retrieve window rectangle.");
    }

    /// <summary>
    ///     Retrieves the current position of the mouse cursor on the screen.
    /// </summary>
    /// <param name="position">
    ///     When this method returns, contains the <see cref="System.Drawing.Point" /> representing the current mouse cursor
    ///     position.
    /// </param>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the mouse position cannot be retrieved.
    /// </exception>
    /// <remarks>
    ///     This method uses Windows API calls to determine the mouse cursor's position.
    /// </remarks>
    public static void GetMousePosition(out Point position)
    {
        if (PInvoke.GetCursorPos(out var point))
        {
            position = new Point(point.X, point.Y);
            return;
        }

        throw new InvalidOperationException("Unable to retrieve mouse position.");
    }

    /// <summary>
    ///     Retrieves the handle of the specified WPF <see cref="Window" />.
    /// </summary>
    /// <param name="window">
    ///     The <see cref="Window" /> instance for which the handle is to be retrieved.
    /// </param>
    /// <returns>
    ///     A <see cref="Windows.Win32.Foundation.HWND" /> representing the handle of the specified window.
    /// </returns>
    /// <remarks>
    ///     This method utilizes the <see cref="WindowInteropHelper" /> to obtain the handle of the specified WPF window.
    ///     It is intended for internal use within the application.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when the provided <paramref name="window" /> is <c>null</c>.
    /// </exception>
    private static HWND GetWindowHandle(Window window)
    {
        var windowInteropHelper = new WindowInteropHelper(window);
        return (HWND)windowInteropHelper.Handle;
    }
}
