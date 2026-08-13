using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Bim.FamilyManager.Installer
{
    /// <summary>
    /// Represents one Revit version entry in the version selector dialog.
    /// </summary>
    public class RevitVersionEntry : INotifyPropertyChanged
    {
        private bool _isSelectable;
        private bool _isSelected;

        /// <summary>The Revit year string, e.g. "2025".</summary>
        public string Year { get; set; } = string.Empty;

        /// <summary>True if this Revit version is detected as installed on the machine.</summary>
        public bool IsDetected { get; set; }

        /// <summary>True when this version may be selected in the current installer session.</summary>
        public bool IsSelectable
        {
            get => _isSelectable;
            set
            {
                if (_isSelectable == value) return;
                _isSelectable = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True if the user has selected this version for installation.
        /// Only meaningful when <see cref="IsDetected"/> is true.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string DisplayLabel => IsDetected
            ? $"Revit {Year}"
            : $"Revit {Year}  (not installed)";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// ViewModel for the Revit version selector dialog.
    /// Populated from registry detection; each entry carries its own selection state.
    /// </summary>
    public class RevitVersionSelectorViewModel
    {
        public List<RevitVersionEntry> Versions { get; } = new List<RevitVersionEntry>();

        public RevitVersionSelectorViewModel()
        {
            // Registry detection is best-effort — if it fails for any reason
            // (permissions, missing key) all versions show as not detected.
            HashSet<string> installedRevitVersions;
            HashSet<string> installedAddinVersions;
            try
            {
                installedRevitVersions = new HashSet<string>(Script.GetInstalledRevitVersions());
            }
            catch (Exception ex)
            {
                InstallerDiagnostics.TryWriteErrorLog(new InvalidOperationException(
                    "Revit version detection failed.", ex));
                installedRevitVersions = new HashSet<string>();
            }

            try
            {
                installedAddinVersions = new HashSet<string>(Script.GetInstalledAddinVersions());
            }
            catch (Exception ex)
            {
                InstallerDiagnostics.TryWriteErrorLog(new InvalidOperationException(
                    "Installed add-in detection failed.", ex));
                installedAddinVersions = new HashSet<string>();
            }

            var hasInstalledAddins = installedAddinVersions.Count > 0;

            foreach (var year in new[] { "2025", "2026", "2027" })
            {
                var isRevitDetected = installedRevitVersions.Contains(year);
                var isAddinInstalled = installedAddinVersions.Contains(year);
                Versions.Add(new RevitVersionEntry
                {
                    Year = year,
                    IsDetected = isRevitDetected,
                    IsSelectable = isRevitDetected || isAddinInstalled,
                    IsSelected = hasInstalledAddins ? isAddinInstalled : isRevitDetected
                });
            }
        }

        /// <summary>True when at least one detected version is selected.</summary>
        public bool IsSelectionValid => Versions.Any(v => v.IsSelectable && v.IsSelected);
    }
}
