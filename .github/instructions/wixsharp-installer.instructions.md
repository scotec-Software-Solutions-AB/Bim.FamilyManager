---
description: Rules for authoring and maintaining the Bim.FamilyManager installer with WixSharp and WiX Toolset 4.
applyTo: "**/Bim.FamilyManager.Installer/**"
---

# WixSharp / WiX 4 installer instructions

- This repository uses **WixSharp with WiX Toolset 4**. Do not introduce WiX 3 syntax, WiX 3 command-line tooling, or WiX 3-only APIs.
- Treat the existing WixSharp C# authoring as the source of truth. Do not replace it with handwritten `.wxs` authoring unless explicitly requested.
- Before changing installer code, inspect the installer `.csproj`, package versions, shared build properties, existing installer source, and relevant build/release workflow. Do not assume a WixSharp API exists in the version used by this repository.
- Prefer the smallest change that preserves the existing installer architecture and behavior.
- Do not add a new NuGet package, WiX extension, bootstrapper, custom action, or external tool unless it is necessary for the requested behavior.
- Prefer standard Windows Installer functionality over custom actions.
- Preserve per-user installation semantics unless the task explicitly requires a different installation scope. Do not introduce elevation or machine-wide writes accidentally.
- Preserve the existing installation locations and Revit add-in registration strategy unless the task explicitly changes them.
- Revit 2025, 2026, and 2027 are independently selectable installation targets. Model version-specific payloads as MSI features/components so users can add or remove supported Revit versions later through maintenance/Modify.
- Changes must not break maintenance mode, repair, uninstall, or upgrades.
- If Modify is intended to be available, ensure `ARPNOMODIFY` is absent from the final MSI rather than merely assigning it a false-looking string value. Verify the generated MSI when UI helpers or WixSharp abstractions may inject ARP properties.
- Keep component identity and upgrade behavior stable. Never casually change component GUID strategy, UpgradeCode, feature IDs, directory IDs, or other MSI identities.
- Do not place the same physical resource into multiple unrelated components unless the Windows Installer component rules are satisfied.
- Shared files must not be removed when another selected Revit-version feature still requires them.
- Version-specific files, manifests, configuration, and registry data must be owned by the corresponding Revit feature whenever practical.
- Do not hardcode developer-machine paths. Resolve paths from repository/build context or existing build properties.
- Prefer deterministic installer output. Avoid environment-dependent behavior that is not already part of the build.
- Never commit generated WiX intermediate files unless this repository already treats them as source artifacts or the user explicitly asks for them.
- For troubleshooting, inspect the generated WiX source and MSI tables rather than guessing what WixSharp emitted.
- When an installer behavior depends on Windows Installer semantics, verify against Microsoft Windows Installer documentation. When it depends on WixSharp, prefer the WixSharp repository/wiki and the API actually referenced by this solution. For WiX 4 schema/tooling behavior, prefer official WiX Toolset documentation.
- After edits, build the installer using the repository's existing build path. Fix warnings/errors caused by the change and report any validation step that could not be performed.
- Do not claim that an MSI property, feature, component, custom action, or WixSharp setting behaves a certain way unless it is supported by the generated WiX/MSI or authoritative documentation.
