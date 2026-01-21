# Adding a Directory Family Source

A directory family source connects Bim.FamilyManager to a local or network folder containing Revit family files.

## Fields

- **Name**  
  *A user-friendly name for the source (e.g., "Local Library" or "Network Families").*  
  **Required**

- **Path**  
  *The full path to the folder containing your `.rfa` family files.*  
  **Required**  
  Example: `C:\BIM\RevitFamilies` or `\\Server\Shared\Families`

- **Include Subfolders**  
  *If enabled, Bim.FamilyManager will search for families in all subfolders of the specified path.*  
  Optional (default: enabled)

- **Active**  
  *Indicates whether this source is currently active and available in the manager.*  
  Optional (default: enabled)

- **Description**  
  *Optional notes or comments about the source (e.g., "Main office library").*  
  Optional

## Steps

1. Enter a unique name for the source.
2. Browse or type the folder path.
3. Enable or disable subfolder inclusion.
4. Optionally add a description.
5. Click "Save" or "Add" to register the source.

[Back to Managing Family Sources](Managing%20Family%20Sources.md)