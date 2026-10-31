---
description: Dependency injection and service lifetime guidance for C# source and UI.
applyTo: "**/*.cs"
---
# Dependency injection

Use existing registration and resolution conventions found in the repository. Prefer constructor injection for services. Avoid service locators and static mutable service state. Match lifetimes to source providers, UI windows and Revit application/document contexts; do not retain Revit document objects beyond their validity. Dispose owned resources and avoid double-disposing injected instances. Do not assume that the repository uses ASP.NET Core request scopes.
