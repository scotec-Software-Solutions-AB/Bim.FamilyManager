using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using WixSharp;
using ExitDialog = WixSharp.UI.WPF.ExitDialog;
using MaintenanceTypeDialog = WixSharp.UI.WPF.MaintenanceTypeDialog;
using ProgressDialog = WixSharp.UI.WPF.ProgressDialog;
using WelcomeDialog = WixSharp.UI.WPF.WelcomeDialog;

namespace Bim.FamilyManager.Installer
{
    public class Script
    {
        // Supported Revit versions bundled in this installer.
        private static readonly string[] SupportedRevitVersions = { "2025", "2026", "2027" };

        public static void Main(string[] args)
        {
            List<Feature> features;
            var dirs = BuildDirs(GetPublishRoot(), out features);

            var project = new ManagedProject("BIM.FamilyManager", dirs.ToArray<WixObject>())
            {
                GUID = Guid.NewGuid(),
                Platform = Platform.x64,
                UpgradeCode = new Guid("1A2B3C4D-5E6F-7A8B-9C0D-1E2F3A4B5C6D"),
                Version = GetProductVersion(),
                ControlPanelInfo = GetProductInfo(),
                MajorUpgrade = MajorUpgrade.Default,
                BackgroundImage = @"Resources\Icons\BackgroundImage.png",
                BannerImage = @"Resources\Icons\BannerImage.png",
            };

            // WixSharp's maintenance dialog records the Repair choice in MODIFY_ACTION.
            // Translate it to the standard MSI property before costing so all installed
            // features are actually reinstalled with the authored REINSTALLMODE.
            project.Actions = new WixSharp.Action[]
            {
                new SetPropertyAction(
                    "REINSTALL",
                    "ALL",
                    Return.check,
                    When.Before,
                    Step.CostInitialize,
                    new Condition("Installed AND MODIFY_ACTION=\"Repair\""))
            };

            var ui = project.ManagedUI = new ManagedUI();

            // WiX4: inject Package/@Scope="perUser" so the engine installs per-user.
            // This sets MSIINSTALLPERUSER without it being treated as a restricted property.
            // WixSharp.Project.InstallScope is obsolete and broken in WiX4.
            project.WixSourceGenerated += doc =>
            {
                var ns = doc.Root.Name.Namespace;
                var pkg = doc.Root.Descendants(ns + "Package").FirstOrDefault();
                if (pkg != null)
                    pkg.SetAttributeValue("Scope", "perUser");
            };
            ui.InstallDialogs.Add<WelcomeDialog>()
              .Add<CustomDialogWith<RevitVersionSelectorControl>>()
              .Add<ProgressDialog>()
              .Add<ExitDialog>();

            ui.ModifyDialogs.Add<MaintenanceTypeDialog>()
              .Add<CustomDialogWith<RevitVersionSelectorControl>>()
              .Add<ProgressDialog>()
              .Add<ExitDialog>();

            project.Features = features.ToArray();

            var msiPath = GetMsiOutputPath();
            Compiler.BuildMsi(project, msiPath);
        }

        /// <summary>
        /// Builds one Dir per Revit version, each with files tagged to their Feature.
        /// Features are returned via the out parameter so they can be registered
        /// on the project separately — the Project constructor does not accept Feature objects.
        /// </summary>
        private static List<Dir> BuildDirs(string publishRoot, out List<Feature> features)
        {
            features = new List<Feature>();
            var dirs = new List<Dir>();

            foreach (var year in SupportedRevitVersions)
            {
                var feature = new Feature($"Revit {year}", $"Install BIM.FamilyManager for Autodesk Revit {year}.")
                {
                    Id = new Id($"Feature_Revit{year}")
                };

                // WixSharp maps %AppData% to the WiX AppDataFolder property.
                // Using [AppDataFolder] literally in the path is treated as a directory name — not a property reference.
                var revitAddinsPath = $@"%AppData%\Autodesk\Revit\Addins\{year}";
                var versionPublishRoot = Path.Combine(publishRoot, year);

                var dir = new Dir(new Id($"INSTALLDIR_{year}"), revitAddinsPath,
                    new WixSharp.File(new Id($"AddinFile_{year}"), Path.Combine(versionPublishRoot, "Bim.FamilyManager.addin"))
                    {
                        Feature = feature
                    },
                    new Dir(new Id($"BinDir_{year}"), "Bim.FamilyManager",
                        new Files(Path.Combine(versionPublishRoot, @"Bim.FamilyManager\*.*"))
                        {
                            Feature = feature
                        }));

                features.Add(feature);
                dirs.Add(dir);
            }

            return dirs;
        }

        private static string GetPublishRoot()
        {
            var publishRoot = Environment.GetEnvironmentVariable("BIM_FAMILYMANAGER_PUBLISH_ROOT");
            if (string.IsNullOrWhiteSpace(publishRoot))
            {
                throw new InvalidOperationException(
                    "BIM_FAMILYMANAGER_PUBLISH_ROOT must point to the Publish directory before building the MSI.");
            }

            publishRoot = Path.GetFullPath(publishRoot);
            if (!Directory.Exists(publishRoot))
                throw new DirectoryNotFoundException($"Publish directory not found: {publishRoot}");

            return publishRoot;
        }

        private static string GetMsiOutputPath()
        {
            var outputPath = Environment.GetEnvironmentVariable("BIM_FAMILYMANAGER_MSI_PATH");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                throw new InvalidOperationException(
                    "BIM_FAMILYMANAGER_MSI_PATH must specify the output MSI path.");
            }

            outputPath = Path.GetFullPath(outputPath);
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputDirectory))
                throw new InvalidOperationException($"Invalid MSI output path: {outputPath}");

            Directory.CreateDirectory(outputDirectory);
            return outputPath;
        }

        private static ProductInfo GetProductInfo()
        {
            return new ProductInfo
            {
                Comments = "Revit add-in for managing and standardizing Revit families.",
                HelpLink = "https://github.com/scotec-Software-Solutions-AB/Bim.FamilyManager/issues",
                UrlInfoAbout = "https://www.scotec.com/bimfamilymanager",
                UrlUpdateInfo = "https://github.com/scotec-Software-Solutions-AB/Bim.FamilyManager/releases",
                Manufacturer = "scotec",
                ProductIcon = @"Resources\Icons\Logo.ico"
            };
        }

        private static Version GetProductVersion()
        {
            var semver = Environment.GetEnvironmentVariable("PkgVersion") ?? "0.1.0-local";
            var version = semver.Split('-', '+')[0];
            var versionParts = version.Split('.');

            if (versionParts.Length != 3 || !Version.TryParse(version, out var parsedVersion))
            {
                throw new InvalidOperationException(
                    "Invalid version format in PkgVersion environment variable. Expected format: Major.Minor.Build (e.g., 1.0.0)");
            }

            // WIX1148: MSI major version must be < 256. If a year-based version like 2025.0.0 is used
            // the major component is reduced to its last two digits (e.g. 25.0.0).
            if (parsedVersion.Minor < 0 || parsedVersion.Minor > 255 ||
                parsedVersion.Build < 0 || parsedVersion.Build > 65535)
            {
                throw new InvalidOperationException(
                    "Invalid MSI version in PkgVersion environment variable. Minor must be <= 255 and build must be <= 65535.");
            }

            return parsedVersion.Major >= 256
                ? new Version(parsedVersion.Major % 100, parsedVersion.Minor, parsedVersion.Build)
                : parsedVersion;
        }

        /// <summary>
        /// Returns the Revit versions from <see cref="SupportedRevitVersions"/> that are
        /// detected as installed on the current machine via the Autodesk registry keys.
        /// </summary>
        public static IEnumerable<string> GetInstalledRevitVersions()
        {
            // Autodesk registers installed Revit versions under this key.
            const string revitRegistryBase = @"SOFTWARE\Autodesk\Revit";

            using (var baseKey = Registry.LocalMachine.OpenSubKey(revitRegistryBase))
            {
                if (baseKey == null)
                    yield break;

                foreach (var year in SupportedRevitVersions)
                {
                    // Each year has a subkey named e.g. "Autodesk Revit 2025"
                    var subKeyName = baseKey.GetSubKeyNames()
                        .FirstOrDefault(k => k.Contains(year));

                    if (subKeyName != null)
                        yield return year;
                }
            }
        }

        /// <summary>
        /// Returns the supported Revit versions for which the per-user
        /// BIM.FamilyManager add-in manifest currently exists.
        /// </summary>
        public static IEnumerable<string> GetInstalledAddinVersions()
        {
            var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            foreach (var year in SupportedRevitVersions)
            {
                var manifestPath = Path.Combine(
                    applicationData,
                    "Autodesk",
                    "Revit",
                    "Addins",
                    year,
                    "Bim.FamilyManager.addin");

                if (System.IO.File.Exists(manifestPath))
                    yield return year;
            }
        }
    }
}
