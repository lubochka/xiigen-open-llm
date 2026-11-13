# Redaction log

Screenshot captured: target-window.png.
Visual inspection: shows Cursor login/signup gate only. No password, PIN, 2FA, API key, browser auth page, email address, or token is visible.

No credentials were entered. No user/main Cursor profile storage was used for launch; command-line inventory shows test-only storage under this evidence folder.

Sanitization update: generated runtime storage directories `cursor-user-data/` and `cursor-extensions/` were removed before commit. The retained evidence records only summarized test-only launch paths; see `runtime-storage-manifest.json`.
