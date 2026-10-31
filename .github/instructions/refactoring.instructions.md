---
description: Safe refactoring of the C# application and UI.
applyTo: "**/*.cs"
---
# Refactoring

- Preserve observable behavior, public APIs, configuration formats, Revit context/transaction semantics, and supported versions unless change is explicitly requested.
- Refactor in small steps; avoid mixing cleanup with functional fixes.
- Verify WPF bindings, DI wiring and source-provider behavior after moving or renaming code.
- Update tests and relevant XML/Markdown documentation when contracts or behavior change.
