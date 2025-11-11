using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Bim.FamilyManager.Source.Directory.Logic;

/// <summary>
///     Provides utility methods for creating backup copies of files and checking folder write permissions.
/// </summary>
/// <remarks>
///     This class is designed to assist with file management tasks, such as creating versioned backups
///     of files and verifying if the current user has write access to a specific folder. It is used
///     internally within the application to ensure data safety and integrity during file operations.
/// </remarks>
public class FileBackupHelper
{
    /// <summary>
    ///     Creates a backup of the specified file by generating a versioned copy in the same directory.
    /// </summary>
    /// <param name="filePath">The full path of the file to back up.</param>
    /// <exception cref="System.ArgumentException">Thrown when the <paramref name="filePath" /> is null or empty.</exception>
    /// <exception cref="System.IO.FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <remarks>
    ///     The backup file is created with a versioned name format, such as "MyFile.0001.extension".
    ///     If the directory containing the file does not allow write access, the method exits without creating a backup.
    /// </remarks>
    public static void CreateBackup(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException(@"File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);

        // Regex to match backup files with the format: MyFile.0001.rfa
        var pattern = $@"^{Regex.Escape(fileNameWithoutExtension)}\.\d{{4}}{Regex.Escape(extension)}$";

        // Find existing backup files that match the pattern
        var existingBackups = System.IO.Directory.GetFiles(directory)
                                    .Where(f => Regex.IsMatch(Path.GetFileName(f), pattern, RegexOptions.IgnoreCase))
                                    .Select(f => int.Parse(Path.GetFileName(f)
                                                               .Substring(fileNameWithoutExtension.Length + 1, 4)))
                                    .OrderByDescending(n => n);

        // Determine the next backup number
        var nextBackupNumber = existingBackups.FirstOrDefault() + 1;

        // Format the backup file name with exactly 4 digits
        var backupFileName = $"{fileNameWithoutExtension}.{nextBackupNumber:D4}{extension}";
        var backupFilePath = Path.Combine(directory, backupFileName);

        // Copy the file to create the backup
        File.Copy(filePath, backupFilePath);
    }

    /// <summary>
    ///     Determines whether the current user has write access to the specified folder.
    /// </summary>
    /// <param name="folderPath">The path of the folder to check for write permissions.</param>
    /// <returns>
    ///     <c>true</c> if the current user has write access to the folder; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method evaluates the access control list (ACL) of the specified folder to determine
    ///     if the current user or any of their groups have write permissions. If an error occurs
    ///     during the evaluation, the method will return <c>false</c>.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="folderPath" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     Thrown if <paramref name="folderPath" /> is an empty string or contains invalid characters.
    /// </exception>
    /// <exception cref="System.UnauthorizedAccessException">
    ///     Thrown if the caller does not have the required permission to access the folder's ACL.
    /// </exception>
    /// <exception cref="System.IO.DirectoryNotFoundException">
    ///     Thrown if the specified folder does not exist.
    /// </exception>
    public static bool CanWriteToFolder(string folderPath)
    {
        try
        {
            // Get the directory information
            var directoryInfo = new DirectoryInfo(folderPath);

            // Get the access control list (ACL) for the directory
            var directorySecurity = directoryInfo.GetAccessControl();

            // Get the current user's identity
            var currentUser = WindowsIdentity.GetCurrent();
            var currentPrincipal = new WindowsPrincipal(currentUser);

            // Get the access rules for the directory
            var accessRules = directorySecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));
            foreach (FileSystemAccessRule rule in accessRules)
            {
                // Check if the rule applies to the current user or their groups
                if (currentUser.User!.Equals(rule.IdentityReference) ||
                    currentPrincipal.IsInRole((SecurityIdentifier)rule.IdentityReference))
                {
                    // Check if the rule grants write access
                    if ((rule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write)
                    {
                        return rule.AccessControlType == AccessControlType.Allow;
                    }
                }
            }

            // If no rules explicitly allow write-access, return false
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine($@"Error checking folder access: {e.Message}");
            return false;
        }
    }
}
