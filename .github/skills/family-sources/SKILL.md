---
name: family-sources
description: Use for directory and Azure Blob family-source implementations, configuration, browsing, filtering, downloading and caching.
---
# Family source development

Start by inspecting the actual source contracts, provider implementations, view models and settings schemas; maintain their extension points and backwards compatibility.

## Directory sources

- Handle local and UNC paths, missing/disconnected locations, access denied, malformed files and large recursive trees.
- Respect existing filters and folder-exclusion configuration where implemented; do not invent configuration keys.
- Avoid blocking WPF UI during enumeration; honor cancellation and cleanly report source errors.

## Azure Blob sources

- Reuse configured Azure client and authentication behavior. Never expose credentials or connection strings in logs or tests.
- Account for transient errors, cancellation, pagination/listing, missing blobs and incomplete downloads. Manage local temporary files and streams deterministically.

## Shared contracts and UI

- Preserve identifiers, settings serialization, family metadata and preview behavior when changing providers.
- Isolate I/O from Revit API loading; use existing dispatch mechanisms for host operations.
- Add tests around enumeration, filtering, unavailable sources and settings migration where practical.
