---
description: XML docs and Markdown documentation conventions.
applyTo: "**/*.{cs,md}"
---
# Documentation

- Document relevant public contracts and non-obvious behavior, not mere restatements of names.
- Use correct `summary`, `param`, `returns`, `exception`, and `remarks` tags as applicable. Mention Revit API context, transactions, resource ownership and cancellation where material.
- Keep README and `Documentation/` content accurate for configured Revit versions and family source types.
- Ensure Markdown examples use real methods, option names and paths from this repository; never invent APIs.
- Preserve existing tone, formatting, and headings unless reorganizing is requested.
