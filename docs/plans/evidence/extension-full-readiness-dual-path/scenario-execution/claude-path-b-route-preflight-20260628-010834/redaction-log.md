# Redaction log

## Scope

This folder contains a route preflight only. No scenario prompt was entered or submitted.

## Redactions and avoided captures

- Raw Claude window content was not persisted.
- No full Claude screenshot was saved.
- `claude-window-redacted-geometry.png` is a generated redacted geometry image. It masks the content area and is not route-proof screenshot evidence.
- Non-allowlisted window titles were redacted before persistence.
- UIA control names were redacted unless they were generic Claude/login/send labels or XIIGen/Smart Skills route terms.
- No password, PIN, token, account data, raw chat content, private prompt draft, or model response was captured.

## Reason

The local Claude app/window exists, but the correct XIIGen/Smart Skills thread/project/extension route is not proven. Saving raw chat content would add privacy risk without proving the route.
