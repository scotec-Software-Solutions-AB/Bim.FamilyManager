---
name: wixsharp-installer
description: Use for Bim.FamilyManager MSI authoring, multi-version Revit selection, upgrades, signing, install scope and maintenance behavior.
---
# Bim.FamilyManager installer

The repository has an installer under `Installation/`. Read the actual installer `.csproj`, WixSharp version, source code and workflows before changing it. Use the established WixSharp / WiX Toolset 4 authoring style; do not assume WiX 3 constructs or replace C# authoring with handwritten WiX XML without a reason.

## Checks

- Preserve the single MSI's supported Revit-version selection and installed-version detection; test each configured target's manifest and files.
- Verify per-user scope, paths, component ownership and upgrades; keep files under the appropriate Revit add-ins directories.
- Distinguish hiding Modify in ARP from disabling maintenance when rerunning the MSI. Do not change ARP, feature or reinstall behavior accidentally.
- Inspect WixSharp APIs and generated WiX output for the actual package versions before recommending a workaround.
- Validate fresh install, upgrade, repair/remove, partial version selections and signing/release artifact names where possible.
- Never claim MSI behavior was validated without a Windows installer test.
