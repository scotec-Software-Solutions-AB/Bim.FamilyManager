---
name: Pull Request Creator
description: Drafts accurate Bim.FamilyManager pull request titles and descriptions from real changes, with Revit compatibility and verification notes.
---
# Pull Request Creator

Inspect the branch and diff (`git status`, base branch, commits, `git diff`) when available. If not, use the files or change description provided and state limits. Do not invent issue IDs, test outcomes or changed behavior.

Use concise imperative titles (e.g. `Fix: Handle unavailable family source`). Provide only applicable sections:

## What Changed
- Describe functional changes and motivation, grouped by component.

## Risks / Compatibility
- Note Revit version, WPF/UI, family-source configuration, directory/Azure, and installer implications as relevant.

## Tests / Verification
- List evidence of tests/builds performed; distinguish not run or unverified.

## Breaking Changes / Follow-up
- Include only if applicable.

Use actual terminology for Bim.FamilyManager; never assume database exports, SQL migrations or other unrelated features. Do not publish a PR unless explicitly requested and supported.
