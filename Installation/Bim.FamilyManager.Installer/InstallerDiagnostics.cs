using System;
using System.IO;

namespace Bim.FamilyManager.Installer
{
    internal static class InstallerDiagnostics
    {
        internal static string ErrorLogPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "scotec",
            "BIM.FamilyManager",
            "Installer.log");

        internal static bool TryWriteErrorLog(Exception exception)
        {
            try
            {
                var path = ErrorLogPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.AppendAllText(
                    path,
                    $"[{DateTimeOffset.Now:O}]\r\n{exception}\r\n\r\n");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}