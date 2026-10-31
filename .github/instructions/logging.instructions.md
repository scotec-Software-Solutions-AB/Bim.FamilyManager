---
description: Diagnostic logging without leaking credentials or sensitive family data.
applyTo: "**/*.cs"
---
# Logging

- Follow the logging library and patterns already present; prefer structured contextual messages where supported.
- Include operation and source identity when useful, but never log Azure credentials, connection strings, tokens, secret values or raw authorization headers.
- Avoid logging large BIM payloads or full family content. Log failures at the boundary responsible for handling them, not repeatedly along the call chain.
