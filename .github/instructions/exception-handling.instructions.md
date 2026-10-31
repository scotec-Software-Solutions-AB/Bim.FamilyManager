---
description: Error handling for Revit, filesystem, Azure, and UI operations.
applyTo: "**/*.cs"
---
# Exception handling

- Keep failures actionable; preserve original exceptions when wrapping and never silently swallow failures.
- Handle inaccessible/missing paths, invalid family files, unavailable networks and Azure errors at suitable boundaries; avoid crashing unrelated sources.
- Respect cancellation without treating user cancellation as an unexpected failure.
- Avoid leaving incomplete downloads or configuration writes; maintain Revit document/transaction integrity.
- Do not mandate an exception hierarchy unless it exists in the affected code.
