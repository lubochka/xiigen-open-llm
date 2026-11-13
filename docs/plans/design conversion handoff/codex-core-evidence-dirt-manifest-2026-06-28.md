# Codex Core Evidence Dirt Manifest - 2026-06-28

claim_scope:
- Created an inspectable manifest for the exact llm_mvp_core dirt categories named in the work order.
- No cleanup, delete, stage, commit, push, or repo-file edit was performed.

evidence_scope:
- Original manifest: ran `git status --short --untracked-files=all` against only the requested pathspec categories.
- Original manifest: ran `git diff --name-status` against only the requested pathspec categories.
- Reconciled `evidence/run/flow-platform-bridge-20260628-161923` on 2026-06-28 by targeted `git status --short --untracked-files=all` and recursive listing only.
- Reconciled exact cache file `tools/extension-full-readiness-dual-path/__pycache__/validate_phase02_evidence_contract.cpython-313.pyc` on 2026-06-28 by `Test-Path` and exact-path `git status --short` only; the file was absent and exact-path git status output was empty.
- Used status/diff/listing summaries only. I did not inspect file contents, run tests, or validate whether evidence files are safe to publish.

count_claimed: 6891 reconciled matching dirty entries; bridge target category count is directly verified as 12; exact pyc row was removed after targeted absence/status checks; whole-manifest total is the prior 6892 minus that stale single cache entry only.
independent_evidence_count: 2 original read-only git evidence commands plus 2 targeted bridge reconciliation inspections (`status --short --untracked-files=all`, recursive listing) plus 2 exact pyc reconciliation inspections (`Test-Path`, exact-path `git status --short`)
subagent_id_or_NONE: bounded-core-evidence-archive-waive-manifest-worker; latest targeted pyc reconciliation by bounded-core-manifest-pyc-reconciliation-worker
downgrade_needed: yes; evidence supports classification and next-action recommendations only, not file-content safety or product readiness.

Generated at: 2026-06-28T18:16:08+03:00
Repository inspected: `C:\Projects\llm_mvp_core`
Manifest output: `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-core-evidence-dirt-manifest-2026-06-28.md`

## Summary

Total matching dirty entries: 6891

Tracked diffs found: 2

Untracked entries found: 6889

Reconciliation note: the whole-manifest totals above were not produced by a fresh all-category recount. They reconcile the prior manifest total with the directly verified bridge category increase from 7 to 12 entries, then remove one stale cache entry after exact-path absence/status checks.

Cache-file reconciliation boundary: the deleted cache-file evidence proves only that `tools/extension-full-readiness-dual-path/__pycache__/validate_phase02_evidence_contract.cpython-313.pyc` is absent and has no exact-path git status output. It does not prove broad cache cleanup, absence of other cache files, or repo cleanliness.

No category should be acted on now without Luba. Conservative reason: the worker was authorized to create this manifest only, and several categories may contain evidence payloads, traces, logs, screenshots, user-data caches, or request/response files whose publication/deletion policy was not verified by content inspection.

## Category Manifest

| Category | Count | Status counts | Representative paths | Classification | Recommended next bounded action | Safe to act now without Luba |
|---|---:|---|---|---|---|---|
| `docs/plans/evidence/extension-full-readiness-dual-path` | 4170 | `??`: 4170 | `docs/plans/evidence/extension-full-readiness-dual-path/next-scenario-selection-after-row28-20260627-012724/next-scenario-selection-after-row28.json`<br>`docs/plans/evidence/extension-full-readiness-dual-path/non-ui-claude-adapter-entrypoint-proof-20260627T050437Z/command.txt`<br>`docs/plans/evidence/extension-full-readiness-dual-path/non-ui-claude-adapter-entrypoint-proof-20260627T050437Z/stdout.log`<br>`docs/plans/evidence/extension-full-readiness-dual-path/plan-readiness-status-20260627-023854/plan-readiness-status.md` | Evidence archive, with possible generated cache/user-data mixed into evidence folders | Archive doc now via this manifest; next ask Luba whether to commit as evidence archive, waive, or approve a cleanup plan for generated caches only | No |
| `artifacts/extension-runtime-token-quality-learnability` | 2678 | `??`: 2678 | `artifacts/extension-runtime-token-quality-learnability/claude-ext-readiness-01/CLAUDE-EXT-READINESS-01-evidence-packet.json`<br>`artifacts/extension-runtime-token-quality-learnability/claude-ext-readiness-01/install-smoke/install-smoke-report.json`<br>`artifacts/extension-runtime-token-quality-learnability/claude-ext-readiness-01/install-smoke/npm-install-smoke.stderr.txt`<br>`artifacts/extension-runtime-token-quality-learnability/claude-ext-readiness-01/install-smoke/phase40-claude-check-set-1.json` | Evidence archive and commit candidate, pending owner policy | Commit only after Luba approves scope/redaction policy; otherwise archive doc or waive | No |
| `evidence/run/*flow47*` | 26 | `??`: 26 | `evidence/run/flow47-live-proof-20260628-160245/cleanup.txt`<br>`evidence/run/flow47-live-proof-20260628-160245/flow.stderr.log`<br>`evidence/run/flow47-live-proof-20260628-160245/flow.stdout.log`<br>`evidence/run/flow47-live-proof-20260628-160245/health.json` | Evidence archive and likely narrow commit candidate | Commit only if Luba confirms Flow47 live-proof run evidence belongs in repo history; otherwise archive doc or waive | No |
| `evidence/run/flow-platform-bridge-20260628-161923` | 12 | `??`: 12 | `evidence/run/flow-platform-bridge-20260628-161923/bridge-summary.json`<br>`evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/browser-visual-proof-report.json`<br>`evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/desktop-1440x900.png`<br>`evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/dom-text.txt`<br>`evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/mobile-390x844.png`<br>`evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/render-saved-html-css-proof.js`<br>`evidence/run/flow-platform-bridge-20260628-161923/flow.stderr.log`<br>`evidence/run/flow-platform-bridge-20260628-161923/platform-final-status-body.json`<br>`evidence/run/flow-platform-bridge-20260628-161923/platform.stdout.log` | Evidence archive and likely narrow commit candidate. Browser-proof files are evidence archive candidates only; boundary: saved HTML/CSS browser render evidence, not full runtime/source design fidelity/external AI proof. | Commit only if Luba confirms bridge evidence belongs in repo history; otherwise archive doc or waive. No cleanup or commit without Luba approval. | No |
| `xiigen-codex-*.json` | 3 | `??`: 3 | `xiigen-codex-request.json`<br>`xiigen-codex-response.json`<br>`xiigen-codex-trace.json` | Evidence archive, but hard blocker requiring Luba before commit because request/response/trace content was not inspected for sensitivity | Hard blocker question: should these root request/response/trace JSON files be committed, archived outside repo, or waived/cleaned with approval? | No |
| `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-*.json*` | 2 | ` M`: 2 | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`<br>`docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl` | Commit candidate, tracked plan/state edits | Commit only after Luba confirms these tracked state/review edits are part of the intended evidence chain | No |

## Tracked Diff Summary

Only path-level diff status was inspected. Diff contents were not inspected.

| Status | Path | Category |
|---|---|---|
| `M` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json` | Commit candidate |
| `M` | `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl` | Commit candidate |

## Browser-Proof Reconciliation

Target category: `evidence/run/flow-platform-bridge-20260628-161923`

Current verified untracked file count: 12

Browser-proof generated files explicitly observed:

| File | Classification | Evidence boundary |
|---|---|---|
| `evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/browser-visual-proof-report.json` | Evidence archive candidate | Saved HTML/CSS browser render only; not full runtime/source design fidelity/external AI proof |
| `evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/desktop-1440x900.png` | Evidence archive candidate | Saved HTML/CSS browser render only; not full runtime/source design fidelity/external AI proof |
| `evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/mobile-390x844.png` | Evidence archive candidate | Saved HTML/CSS browser render only; not full runtime/source design fidelity/external AI proof |
| `evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/dom-text.txt` | Evidence archive candidate | Saved HTML/CSS browser render only; not full runtime/source design fidelity/external AI proof |
| `evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof/render-saved-html-css-proof.js` | Evidence archive candidate | Saved HTML/CSS browser render only; not full runtime/source design fidelity/external AI proof |

No cleanup, deletion, staging, commit, or push is recommended or authorized without Luba approval.

## Recommendations

1. Do not clean or delete anything from this worker's evidence alone.
2. Treat evidence directories as archive/commit candidates only after Luba confirms what should enter repo history.
3. Treat root `xiigen-codex-*.json` files as requiring a Luba decision before commit because request/response/trace content was not inspected.
4. Do not list `tools/extension-full-readiness-dual-path/__pycache__/validate_phase02_evidence_contract.cpython-313.pyc` as present in the dirty manifest; targeted evidence only shows that single cache file is absent.
5. If the next bounded worker is authorized, it should produce either a commit-scope manifest or a cleanup-scope manifest, not mix both actions.

must_stop_after_report=true
