---
description: Bim.FamilyManager layering and compatibility rules.
applyTo: "**/*.cs"
---
# Architecture

- Check actual project boundaries before adding dependencies: add-in host, abstractions, base/core, WPF views and view models, directory and Azure Storage source providers.
- Keep source enumeration, loading and settings independent of presentation when possible. Do not couple reusable source contracts to WPF or Revit API types unnecessarily.
- Keep host commands thin and execute Revit operations only in supported contexts. Preserve transactions and document validity.
- Prefer existing DI and extension points; avoid introducing new layers for a single-use change.
- Preserve public contracts, settings formats and compatibility with configured Revit versions. Explain and test breaking changes.
