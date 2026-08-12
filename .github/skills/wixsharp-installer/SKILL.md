---
name: wixsharp-installer
description: Design, implement, review, debug, and validate Windows Installer packages authored with WixSharp and WiX Toolset 4. Use for MSI features, Revit-version selection, maintenance/Modify, upgrades, install scope, ARP behavior, files/directories, components, custom actions, signing, generated WiX inspection, and installer build failures.
compatibility: Visual Studio 2026 with GitHub Copilot Agent mode; repository uses WixSharp and WiX Toolset 4 on Windows.
---

# WixSharp Installer Skill

Use this skill for installer work in this repository.

## 1. Establish the actual installer context first

Before proposing or applying a change:

1. Find the installer project and its `.csproj`.
2. Read the relevant WixSharp authoring files.
3. Read `Directory.Packages.props`, `Directory.Build.props`, or equivalent files that determine package versions and build properties.
4. Check the existing CI/release workflow if the task can affect packaging, versioning, signing, or produced artifacts.
5. Determine the exact WixSharp version and confirm the project is using the WiX 4 toolchain.
6. Search the repository for existing patterns before inventing new ones.

Do not infer the available WixSharp API from memory when the installed package or repository source can answer the question.

## 2. Technology baseline

Use these assumptions unless the repository says otherwise:

- Authoring language: C#
- Installer abstraction: WixSharp
- Underlying toolset: WiX Toolset 4
- Package type: MSI
- Application: Autodesk Revit add-in
- Supported Revit targets: 2025, 2026, and 2027
- Installation scope: per user
- Users must be able to modify the installed Revit-version selection later

Never fall back to WiX 3 examples such as old schema namespaces or `candle.exe` / `light.exe` workflows when solving a WiX 4 task.

## 3. WixSharp first, generated WiX second

Prefer a supported WixSharp API when one exists.

Use generated `.wxs` as a diagnostic representation:

- inspect it when WixSharp behavior is unclear;
- use `Compiler.PreserveTempFiles` or an existing repository mechanism when appropriate;
- compare emitted properties, features, components, conditions, sequences, and directories with the intended MSI behavior;
- do not make permanent manual edits to generated `.wxs` unless explicitly requested.

If WixSharp cannot express a required WiX 4 construct cleanly, use the repository's existing WixSharp XML-processing/event extension mechanism before redesigning the installer.

## 4. Revit version features

Treat Revit support as independently selectable MSI features.

Desired behavior:

- Revit 2025 can be installed or omitted.
- Revit 2026 can be installed or omitted.
- Revit 2027 can be installed or omitted.
- Maintenance mode can later add a previously omitted version.
- Maintenance mode can later remove an installed version.
- Removing one Revit version must not damage another selected version.

When changing feature design:

1. Identify each version-specific payload.
2. Identify truly shared payload.
3. Ensure each component has a clear owner.
4. Avoid cross-feature component ownership that makes feature removal ambiguous.
5. Keep feature identifiers stable across compatible upgrades.
6. Verify the maintenance UI exposes feature selection when Modify is supported.

Do not duplicate shared files merely to make feature authoring convenient unless that is already an intentional repository design.

## 5. Per-user installation

Preserve per-user behavior.

Check for accidental machine-wide behavior including:

- Program Files writes;
- HKLM registry writes;
- machine-wide environment changes;
- services;
- privileged custom actions;
- installation actions that require elevation.

For Revit add-in registration, preserve the repository's existing per-user registration/path strategy. Do not change it merely because another Revit installer example uses a machine-wide location.

If an action requires elevation, stop and explain the architectural consequence before implementing it.

## 6. Add/Remove Programs and Modify

Distinguish between:

- whether the product is registered in ARP;
- whether Modify is displayed;
- whether the MSI actually contains selectable features and a maintenance UI capable of changing them.

For a product that should support Modify:

- do not intentionally set `ARPNOMODIFY`;
- remember that for boolean-like MSI ARP properties, presence can be significant, so do not assume a textual value such as `"0"` or `"False"` is equivalent to absence;
- inspect WixSharp UI helpers because they can inject ARP properties independently of `ControlPanelInfo`;
- verify the final MSI `Property` table when behavior does not match source intent;
- verify the maintenance dialog sequence includes feature selection if the user must change installed Revit versions.

A visible Modify button alone is not sufficient. Test that feature state changes actually install/remove the correct components.

## 7. Components and Windows Installer identity

Treat component identity as persistent installer state.

Before changing component layout or IDs, determine whether the change affects an already released MSI.

Preserve these unless there is a deliberate upgrade design:

- `UpgradeCode`;
- feature IDs;
- directory IDs when externally significant;
- component GUIDs / component identity strategy;
- component key paths.

Follow Windows Installer component rules. In particular:

- one component should represent one atomic installation unit;
- do not move a resource between components casually across product versions;
- avoid multiple components owning the same file/path;
- make registry/file key paths deliberate;
- make shared resources safe when features are independently removed.

When uncertain, inspect the generated MSI/component table rather than relying only on the WixSharp object graph.

## 8. Upgrade behavior

Before editing upgrade logic, determine the current strategy:

- first install;
- repair;
- maintenance change;
- minor update, if used;
- major upgrade;
- downgrade;
- same-version reinstall.

Keep the product's stable `UpgradeCode`.

Do not change `ProductCode`, package identity, or major-upgrade scheduling rules without understanding the current release strategy.

When changing files/components in a released installer, reason about both a clean install and an upgrade from the previous public version.

## 9. Custom actions

Prefer standard MSI tables and WixSharp/WiX authoring over custom actions.

Use a custom action only when standard MSI behavior cannot perform the task.

For every custom action, identify:

- installation sequence;
- execute vs UI sequence;
- immediate vs deferred;
- impersonated vs non-impersonated context;
- install, repair, modify, upgrade, uninstall conditions;
- rollback requirements;
- whether it is safe for per-user installation.

Never use a custom action merely to copy files, create normal registry values, or perform another task MSI already models declaratively.

## 10. Conditions

Write conditions around MSI state, not assumptions about UI flow.

When conditions involve feature actions, installed state, upgrades, uninstall, or repair:

1. identify the relevant MSI properties/states;
2. consider silent installation as well as interactive UI;
3. consider initial install and maintenance separately;
4. verify generated condition placement/sequence.

Avoid conditions that only work because a particular dialog happened to set a property.

## 11. Files, paths, and Revit content

Do not hardcode local checkout paths or build-machine absolute paths.

Use existing source-base/build properties.

Keep these concepts separate:

- build-time source path;
- MSI target directory;
- Revit `.addin` manifest location;
- binary payload location;
- user configuration/data location.

If the installer references Autodesk/Revit content already installed on the machine, do not package or redistribute that content unless explicitly required and licensed.

## 12. Signing

If the repository has signing configured:

1. build binaries;
2. sign publisher-owned binaries;
3. create the MSI;
4. sign the MSI;
5. verify signatures.

Do not alter third-party signed binaries.

Do not introduce certificate secrets, passwords, tokens, or PFX files into source control.

If signing is not configured, do not invent credentials or silently create a self-signed production workflow.

## 13. Debugging build failures

For a WixSharp/WiX build failure:

1. capture the first meaningful error, not only the final MSBuild exit code;
2. identify whether failure occurs in C# compilation, WixSharp generation, WiX 4 compilation/linking, MSI validation, signing, or post-build logic;
3. inspect generated `.wxs` when the failure is after WixSharp generation;
4. verify required WiX extensions are available and match the WiX 4 toolchain;
5. verify package/tool versions from the project;
6. apply the smallest correction;
7. rebuild.

Do not recommend reinstalling WiX or changing global tool versions before checking how this repository provisions the toolchain.

## 14. Validation after a change

Use the repository's existing commands where available.

At minimum validate what is relevant:

- installer project builds;
- MSI is produced;
- expected Revit features exist;
- generated WiX contains expected feature/component/property authoring;
- `ARPNOMODIFY` is absent when Modify must be enabled;
- initial feature selection behaves correctly;
- maintenance can add a Revit version;
- maintenance can remove a Revit version;
- repair does not lose configuration unexpectedly;
- uninstall removes only installer-owned resources;
- upgrade from the prior supported installer works, when the change affects upgrade behavior.

If full MSI installation testing cannot be automated in the current environment, state exactly what still requires manual verification.

## 15. Source hierarchy for technical answers

When behavior is uncertain, prefer evidence in this order:

1. the current repository and generated MSI/WiX;
2. the WixSharp version/API actually referenced by the project;
3. official WixSharp repository/wiki;
4. official WiX Toolset 4 documentation;
5. Microsoft Windows Installer documentation.

Avoid generic blog posts and WiX 3 answers unless they explain Windows Installer behavior that is still applicable and you explicitly translate it to WiX 4/WixSharp.

## 16. Change style

- Make focused changes.
- Preserve existing naming and structure.
- Avoid unrelated refactoring.
- Explain MSI semantic changes in comments only when the reason would otherwise be non-obvious.
- Do not add speculative compatibility code.
- Build after editing.
- Report the files changed and the validation performed.
