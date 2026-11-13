# CURSOR-EXT-READINESS-01 Evidence Packet

subagent_id_or_nickname: `CURSOR-EXT-READINESS-01`

Status: `PARTIAL`.

The evidence supports only Cursor local adapter/core boundary and syntax evidence for this Row23-based smoke. It does not prove live Cursor UI, full Cursor extension readiness, token savings, quality improvement, learnability, training, backup/export/import/share/continue-training, rollback/target promotion, or final three-extension readiness.

## What Passed

- `node --check scripts/local-runtime-helper.js`: exit code 0.
- `node --check dist/extension.js`: exit code 0.
- Row23-based `scripts/install-smoke.ps1` report: `adapterFunctionalityPassed=true`.
- Install-smoke debug-off and debug-on cases both report `passed=true` and `core_exit_code=0`.
- Debug-on case reports exact redacted debug shape.
- Both install-smoke cases report raw prompt absent from display/response/trace.

## What Did Not Pass To Full Readiness

- Real Cursor/VS Code Extension Host or UI Path B was not run.
- Signed-in/session preflight was not run.
- Same-scenario Path A plus Path B comparison was not run.
- Token, quality, learnability/DPO lifecycle, backup/export/import/share/continue-training, rollback/target safety, documentation, and final dual-path scenario coverage were not run.

## Command Notes

The first runner command timed out after producing install-smoke artifacts. I stopped the exact runner process after verifying its command line. Because of that timeout, the install-smoke process exit code was not captured in `commands-run.json`; the install-smoke pass claim is limited to its generated report fields and per-case `core_exit_code=0`.

## Next Bounded Worker Scope

`CURSOR-EXT-PATH-B-RUNTIME-01`: run the real Cursor/VS Code Extension Host/UI same-scenario Path B proof with session/auth preflight, exact clicks/typing/expected visible state, screenshots/window inventory/redaction log, same Row23 or parent-approved Cursor scenario input, and correlation to the local adapter/core artifacts. No Claude/Codex execution and no readiness claim unless all named gates pass.
