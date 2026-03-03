# Introduction

**Bim.FamilyManager** is an add-in for **Autodesk® Revit® 2025 and newer** that centralizes how you *store, find, preview, and load* Revit family files (`.rfa`) into your projects.

Instead of relying on a single “office library” folder, Bim.FamilyManager can work with **multiple family sources** at the same time—e.g. departmental folders on a file server plus a curated library in **Azure Blob Storage**. Each source can be enabled/disabled and configured independently.

## What is a “family source”?

A **family source** is a configured connection to a location that contains family files:

- **Directory source**: a local, network, or shared folder path
- **Azure Storage source**: an Azure Blob container + optional virtual root path, authenticated through Azure AD (interactive sign‑in)

Sources appear in the UI as named entries with a **Source** value (path / root path). Only **active** sources are used when browsing/searching families.

## Goals

Bim.FamilyManager aims to:

- Provide a **single place** to manage multiple family libraries
- Make families easier to **discover** through consistent browsing/searching
- Support **cloud-based libraries** without changing the Revit workflow
- Keep configuration **per user** (so each user can point to the sources they need)

## Quick start

1. Open Revit and start **Bim.FamilyManager** from the Revit ribbon.
2. Open **Settings → Family Sources**.
3. Add one or more sources (Directory / Azure Storage).
4. Click **Save** in the settings dialog.
5. Return to the main UI and browse/search/load families from your sources.

## Documentation map

- [Managing Family Sources](Managing%20Family%20Sources.md)
- [Adding a Directory Family Source](Adding%20a%20Directory%20Family%20Source.md)
- [Adding an Azure Family Source](Adding%20an%20Azure%20Family%20Source.md)

