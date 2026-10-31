---
description: Test and validation conventions for Bim.FamilyManager.
applyTo: "**/*Tests*/**/*.cs"
---
# Testing

- Follow existing test framework, naming, and project patterns; do not assume a specific framework without checking.
- Test source enumeration, filtering, settings changes, and error paths with deterministic temporary directory fixtures where possible.
- Mock external Azure and Revit boundaries where appropriate; separate tests requiring a running Revit host or cloud resources.
- Verify cancellation, missing directories, network faults and any regression scenario tied to a fix.
- Do not claim cross-version compatibility without building/running the relevant variants.
