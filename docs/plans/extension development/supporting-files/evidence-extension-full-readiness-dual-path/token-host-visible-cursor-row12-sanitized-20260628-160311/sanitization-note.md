# Sanitization Note

This folder is a commit-safe replacement for:

`C:/Projects/llm_mvp_core/docs/plans/evidence/extension-full-readiness-dual-path/token-host-visible-cursor-row12-20260628-155302/`

It preserves compact blocker evidence only:

- `README.md`
- `result.json`
- `blocker-details.json`
- `command-log.md`
- `redaction-note.md`
- `cursor-cmd-stdout.log`
- `cursor-cmd-stderr.log`
- generated request JSON
- generated core-response JSON

Excluded from this sanitized copy:

- `cursor-user-data*` runtime profiles
- `Cache`
- `Code Cache`
- `GPUCache`
- `Local Storage`
- `Network`
- Cursor logs trees
- `machineid`
- `sentry` queue data
- runtime profile data and large generated binaries
- extension/profile directories not needed for compact blocker evidence
- `row12-host-visible-command-runner.test.js`, because this evidence folder should preserve results, not carry harness code

The blocker conclusion is intentionally preserved. The result status remains:

`BLOCKED_CURSOR_HOST_VISIBLE_HARNESS_FAILED`

Exact next bounded action:

Implement repo-internal Cursor Extension Host row12 runner under `C:/Projects/cursor-xiigen-smart-skills/test/host-qa/...` and configure it to write compact trace artifacts only.
