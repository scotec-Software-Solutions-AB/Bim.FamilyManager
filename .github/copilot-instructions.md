<!--
SPDX-FileCopyrightText: Copyright © 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright © 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->
# GitHub Copilot instructions — Bim.FamilyManager

Work in the existing repository, not an imagined application. Inspect relevant source, project files and tests before making changes; favor focused diffs. The repository is an open-source Autodesk Revit family-library add-in, not a database-backed document management system.

## Product and codebase

- Support the Revit versions actually configured by the projects and build pipeline (the published README describes 2025, 2026 and 2027); do not assume a single runtime or Revit API surface.
- Use the existing C#/.NET, WPF and MVVM patterns. Reuse current abstractions and DI registration instead of introducing competing frameworks.
- Main functional boundaries include Revit integration, abstractions, core family management, UI (standard/modern), directory sources, Azure Blob Storage sources, configuration and MSI packaging. Verify real names and behavior from code.
- Family discovery, filtering, metadata, previews and loading must work with existing source contracts and should remain independent from UI concerns where practical.

## Revit correctness

- Invoke Revit API operations only in supported Revit execution contexts; never invoke them directly from arbitrary background tasks or UI callbacks.
- Respect transaction requirements, document validity and host lifecycle. Keep I/O and network operations off the Revit UI thread when safe; marshal Revit work back through existing mechanisms.
- Check API and target-framework differences per Revit version; do not silently change registration, manifests, or startup behavior.

## Family sources and data handling

- Directory sources can use local and UNC paths; handle missing/inaccessible directories and recursive traversal without freezing the UI.
- Azure sources use configured authentication and storage APIs; never log credentials, connection strings, tokens, or secret values.
- Do not assume a family is local until the source has made its file available. Avoid leaving half-written temporary files and preserve source configuration compatibility.
- Follow existing YAML/configuration conventions (including folder exclusions if implemented) rather than inventing schemas.

## Code quality

- Preserve existing public contracts, serialized settings and migration behavior unless the task explicitly requests changes.
- Use clear async/cancellation patterns for I/O; avoid accessing WPF-bound collections from worker threads.
- Use structured diagnostic logging where supported; do not log sensitive document or tenant details.
- Catch exceptions when you can recover, translate, clean up or add actionable context; do not swallow them.
- Use meaningful XML documentation for non-obvious public APIs, including Revit context and resource ownership requirements. Keep Markdown and example code aligned with the actual implementation.

## Testing, building and communication

- Follow existing test framework and naming conventions; do not invent a test project or require a particular test framework without checking.
- Unit-test source enumeration, filtering and configuration logic using temporary directories and mocked service boundaries as appropriate. Avoid requiring Revit or Azure for ordinary unit tests.
- Build/test the changed projects and affected Revit target variants when tooling is available. Report exactly what was and was not executed.
- Explain key risks, compatibility effects and changes concisely. Do not claim tests or packaging were verified without evidence.

## Installer-specific work

- Existing installer authoring uses WixSharp and WiX Toolset 4. Consult `.github/skills/wixsharp-installer/SKILL.md` and installer source for changes to MSI behavior.
- Preserve per-user installation semantics, multi-Revit-version selection, upgrades and maintenance unless requirements explicitly change. Treat ARP/Modify visibility separately from MSI maintenance behavior.
