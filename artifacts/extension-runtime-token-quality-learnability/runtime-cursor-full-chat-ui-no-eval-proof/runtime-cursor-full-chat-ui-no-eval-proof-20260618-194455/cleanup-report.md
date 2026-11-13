# Cursor proof cleanup report

Run: runtime-cursor-full-chat-ui-no-eval-proof-20260618-194455

## Decision

Kept a small commit-safe evidence packet and removed the raw generated Cursor user-data/cache/runtime profile. The proof remains locked / partial; cleanup does not upgrade it to clean Cursor UI proof.

## Removed

- Roots: cursor-user-data-startprocess, cursor-user-data, cursor-extensions-dir
- Files removed: 76
- Bytes removed: 5308650
- Manifest retained: discarded-user-data-manifest.json

Rationale: these files are generated Cursor profile/cache/local-storage/history/log artifacts. They are noisy, not necessary to prove the blocked/partial result, and had local auth/debug/profile-risk indicators. Raw content was not preserved; the manifest keeps only relative path, byte size, SHA256, and rationale.

## Kept Evidence

- proof-packet.json, checks-report.json, changed-files.txt
- report-ru.md, review-report.md, review-report.json
- cleanup-report.md, cleanup-report.json, discarded-user-data-manifest.json
- Redacted cursor-extension-host-startprocess.stdout.log and cursor-extension-host-startprocess.stderr.log
- Empty direct-run logs cursor-extension-host.stdout.log and cursor-extension-host.stderr.log
- cursor-runtime-proof-harness.js
- cursor-extensions-dir-startprocess/extensions.json
- extension-host-evidence/*.json

## Redaction

Kept host logs were redacted for local Windows user profile paths and Cursor machine/device identifiers. The removed raw user-data profile had pre-cleanup indicators: secret_word=20, cursor_auth=392, machine_ids=21, user_profile=713.

## Boundary

No git add/commit/push, no runtime/eval/external AI/model/training, no source/test/skill/governance/plan edits, and no next phase execution.