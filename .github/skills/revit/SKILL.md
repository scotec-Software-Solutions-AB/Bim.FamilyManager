---
name: revit
description: Revit API context, family discovery/loading and compatibility in Bim.FamilyManager.
---
# Revit family management

Read relevant existing add-in integrations and source contracts before implementing.

- Revit API calls must occur only in supported contexts; external events and commands have different lifecycle constraints. Never run host API calls in arbitrary `Task.Run` work.
- Keep transaction boundaries explicit for model changes; identify the target document, handle closure/invalid documents, and avoid holding Revit elements across unsupported contexts.
- Account for family loading semantics, name collisions and types where the existing code provides such behavior.
- Keep disk/Azure enumeration and previews away from Revit API logic; marshal resulting requests back through existing host mechanisms.
- Respect configured Revit 2025/2026/2027 API/runtime differences rather than assuming one build works everywhere.
- Test host-independent logic separately; note when manual Revit validation is needed.
