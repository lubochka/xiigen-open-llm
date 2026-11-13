# CORE_EVIDENCE_RESIDUE_RESOLVER - 2026-06-28

## Honesty Guard

claim_scope:
- Bounded core evidence/residue resolver packet for `C:\Projects\llm_mvp_core`.
- Created a classification-only resolver packet for dirty/generated/untracked evidence/residue observed by read-only status summary and the existing handoff/preflight reports.
- No cleanup/delete/move/stash/restore, staging, commit, push, branch, PR, test, build, browser, runtime, external AI, secret, archive, or content-redaction action was executed.

evidence_scope:
- Read `codex-preflight-resolution-sweep-2026-06-28.md`.
- Read `codex-designconversion-completion-handoff-2026-06-28.md`.
- Read `codex-designconversion-handoff-index-2026-06-28.md`.
- Ran read-only handoff-folder listing.
- Ran read-only git status summary in `C:\Projects\llm_mvp_core`.
- Checked that this resolver packet path did not exist before writing.
- Did not inspect file contents inside untracked evidence trees, root trace JSON files, package binaries, browser cache/user-data files, secrets, runtime state, tests, builds, PRs, or external services.

count_claimed:
- resolver packets created: 1
- mutating residue actions executed: 0
- cleanup/delete/move/stash/restore actions executed: 0
- commit/stage/push/branch/PR actions executed: 0
- tests/builds/browser/runtime/external AI actions executed: 0

independent_evidence_count:
- 6 current-turn evidence records used: handoff-folder listing, preflight report read, completion handoff read, handoff index read, read-only git status summary, target-path existence check.

subagent_id_or_NONE:
- CORE_EVIDENCE_RESIDUE_RESOLVER

downgrade_needed:
- Yes for any broader claim. The evidence supports only evidence vs residue classification and conservative resolver routing. It does not prove clean repo, product completion, merge readiness, trace sensitivity safety, package provenance, runtime readiness, visual fidelity, or safe cleanup/commit eligibility.

## Required Literals

- `CORE_EVIDENCE_RESIDUE_RESOLVER`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `no cleanup/delete/move/stash/restore`
- `evidence vs residue`

## Source Boundary

Active write scope:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-core-evidence-residue-resolver-2026-06-28.md`

Read sources used:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-preflight-resolution-sweep-2026-06-28.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-completion-handoff-2026-06-28.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-handoff-index-2026-06-28.md`
- read-only git status summary in `C:\Projects\llm_mvp_core`

This packet is a resolver/classification document only. It does not authorize or perform any residue mutation.

## Current Read-Only Git Status Summary

Observed from read-only `git status --porcelain=v1 --untracked-files=all` summary in `C:\Projects\llm_mvp_core`:

| Bucket | Count |
| --- | ---: |
| Total dirty/untracked status entries | 6921 |
| Tracked modified entries | 2 |
| Untracked entries | 6919 |

Status-code summary:

| Status | Count |
| --- | ---: |
| ` M` | 2 |
| `??` | 6919 |

Top-level status summary:

| Top-level path | Count | Initial classification |
| --- | ---: | --- |
| `artifacts` | 2678 | keep as evidence until explicit selector decides archive/commit/defer |
| `docs` | 4175 | mixed: 2 tracked plan/state modifications plus 4172 scenario evidence entries; include relevant handoff/index references only after scoped document work order |
| `evidence` | 64 | keep as evidence; handoff/index already references saved browser proof boundary |
| `tools` | 1 | ignore as local residue or quarantine until exact selector identifies path and action |
| `xiigen-codex-request.json` | 1 | quarantine pending sensitivity selector |
| `xiigen-codex-response.json` | 1 | quarantine pending sensitivity selector |
| `xiigen-codex-trace.json` | 1 | quarantine pending sensitivity selector |

Category summary from the same read-only status pass:

| Category | Count | Initial resolver classification |
| --- | ---: | --- |
| `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-*` tracked state/review files | 2 | commit blocked pending policy |
| `artifacts/extension-runtime-token-quality-learnability/*` | 2678 | keep as evidence; include in handoff/index only by explicit scoped index update |
| `docs/plans/evidence/extension-full-readiness-dual-path/*` | 4172 | keep as evidence if tied to scenario proof; ignore as local residue only after explicit selector |
| other dirty/untracked paths | 69 | mixed evidence/residue; default quarantine or defer until exact path/category selector exists |

Boundary:
- The summary counts are current read-only status evidence only.
- The packet does not inspect the contents of any listed files.
- The packet does not prove that these files are safe to publish, commit, delete, archive, or ignore.

## Resolver Categories

### 1. Keep As Evidence

Classification:
- Evidence trees under `artifacts/extension-runtime-token-quality-learnability/*`.
- Scenario evidence trees under `docs/plans/evidence/extension-full-readiness-dual-path/*`.
- Saved browser proof paths already named by the handoff/index under `evidence/run/flow-platform-bridge-20260628-161923/browser-visual-proof`.

Reason:
- Existing preflight/handoff/index language treats these as evidence candidates or prior saved-output proof boundaries.
- Evidence status does not mean commit-ready, product-ready, or sensitive-safe.

Allowed now:
- Preserve in place.
- Reference in future handoff/index only through a bounded document/index work order.

Blocked:
- Commit, cleanup, archive, delete, move, stash, restore, publish, or sensitivity inspection without explicit selector and policy.

Default:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` for every non-default action.

### 2. Include In Handoff/Index

Classification:
- Existing handoff/index already includes the preflight resolver and broad owner-decision ledger.
- This new packet should be included in future handoff/index only by a separate bounded handoff-index update worker, because this worker's write scope is only this file.

Reason:
- The index is the document/RAG inclusion mechanism, but this work order forbids writing any file except this resolver packet.

Allowed now:
- Record the future work order below.

Blocked:
- Direct edit of `codex-designconversion-handoff-index-2026-06-28.md` or handoff report by this worker.

Default:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` for index mutation until write scope is granted.

### 3. Ignore As Local Residue

Classification:
- Potential local residue includes tool/cache/user-data/status-only entries not proven to be meaningful evidence by the status summary alone.
- The single `tools` top-level status entry and miscellaneous `root_other` entries are not safe to ignore solely from this packet.

Reason:
- Ignoring requires exact path/category selector and owner policy.
- The status summary is metadata only and did not inspect contents or ownership.

Allowed now:
- Mark as candidate local residue only.

Blocked:
- Adding ignore rules, deleting, moving, restoring, stashing, or excluding from future accounting.

Default:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` until exact selector exists.

### 4. Cleanup Blocked Pending Luba Selector

Classification:
- All cleanup-like actions for evidence, generated artifacts, cache/user-data, tool residue, and root trace files are blocked.

Reason:
- The preflight sweep explicitly keeps cleanup/delete/move/stash/restore-style actions blocked until exact selectors and policies are supplied.
- This packet has no cleanup selector, no backup policy, no archive target, no redaction policy, and no owner approval.

Blocked actions:
- `no cleanup/delete/move/stash/restore`
- no archive/copy
- no generated-output deletion
- no cache deletion
- no trace-file deletion
- no fixture refresh/restore
- no broad repo dirt cleanup

Default:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

### 5. Commit Blocked Pending Policy

Classification:
- The 2 tracked modified docs/plans state/review files are commit-blocked pending policy.
- All 6919 untracked entries are commit-blocked pending evidence selection, sensitivity review, provenance policy, and commit message/push policy.

Reason:
- The status summary alone cannot decide commit scope or safety.
- Existing handoff/preflight text requires Luba decisions before commit/cleanup/archive/waive/defer actions.

Blocked:
- stage, commit, push, branch mutation, PR mutation.

Default:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

### 6. Quarantine

Classification:
- Root trace JSON files are quarantined pending sensitivity policy:
  - `xiigen-codex-request.json`
  - `xiigen-codex-response.json`
  - `xiigen-codex-trace.json`
- Any browser/user-data/cache/log files that may contain local session or telemetry residue are quarantine candidates until an exact selector and no-secret inspection policy exists.
- Miscellaneous unclassified `root_other` entries are quarantine/defer by default, not safe-ignore by default.

Reason:
- This worker did not inspect sensitive contents and did not use secrets.
- Trace files and host/user-data artifacts can contain request, response, local state, auth/session, telemetry, or environment details.

Blocked:
- content printing, redaction, archive, commit, cleanup/delete/move/stash/restore, or publication without explicit sensitivity and no-secret-print policy.

Default:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

## Evidence Vs Residue Decision Matrix

| Bucket | Evidence value | Residue risk | Default resolver action |
| --- | --- | --- | --- |
| `artifacts/extension-runtime-token-quality-learnability/*` | High: likely execution/probe evidence packets and command outputs. | Medium: may contain generated outputs, logs, and tool-specific traces. | Keep as evidence; commit/archive blocked pending selector. |
| `docs/plans/evidence/extension-full-readiness-dual-path/*` | High if tied to scenario proof. | High: includes large local run/user-data/cache/log trees. | Keep as evidence or quarantine by subcategory; cleanup blocked pending selector. |
| `evidence/run/flow-platform-bridge-20260628-161923/*` | High for saved-output browser proof boundary already indexed. | Medium: not full runtime proof and may include generated assets. | Keep as evidence; include existing handoff references only. |
| tracked docs/plans state/review files | Medium: governance/state record value. | Medium: may be stale or owned by another workflow. | Commit blocked pending policy; do not restore. |
| root `xiigen-codex-*.json` traces | Unknown: may be useful trace evidence. | High: uninspected sensitivity risk. | Quarantine pending sensitivity selector. |
| tool/cache/user-data/log residue | Unknown to medium. | High: local environment/session residue possible. | Quarantine or ignore only after exact selector; no cleanup now. |

## Explicit Blocker Rules

Default blocker:
- Any cleanup/delete/move/stash/restore/commit/archive/publish/readiness-upgrade action defaults to `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` unless Luba supplies the exact selector, action, target paths, safety policy, and verification evidence requirements.

No selector exists in this work order for:
- cleaning generated artifacts;
- deleting cache/user-data/log residue;
- moving evidence to an archive;
- staging or committing evidence;
- restoring tracked files;
- stashing local residue;
- printing or inspecting root trace contents;
- updating handoff/index;
- running tests/builds/browser/runtime;
- mutating branches, PRs, or remotes.

Therefore:
- `no cleanup/delete/move/stash/restore`
- no stage/commit/push
- no archive/copy
- no sensitivity inspection
- no index/handoff mutation by this worker

## Future Bounded Work Orders

1. `bounded-core-residue-selector-worker`
   - Purpose: convert this resolver into exact Luba decision options.
   - Read scope: this resolver, preflight sweep, handoff/index, and read-only git status summary.
   - Write scope: one codex-prefixed selector packet in the handoff folder.
   - Must output exact category selectors for keep evidence, include in handoff/index, ignore local residue, cleanup, commit, quarantine, or defer.
   - Must not mutate repo files, git state, PRs, branches, tests, builds, runtime, browser, external AI, or secrets.

2. `bounded-core-evidence-index-update-worker`
   - Purpose: include this resolver packet in the handoff/index if Luba authorizes document/index updates.
   - Write scope: only explicitly named handoff/index files.
   - Must add this packet path and preserve downgraded evidence boundaries.
   - Must not stage, commit, push, clean, archive, delete, or inspect sensitive trace contents.

3. `bounded-core-trace-sensitivity-worker`
   - Purpose: decide root `xiigen-codex-*.json` handling only after Luba supplies a no-secret-print and sensitivity policy.
   - Allowed actions only if explicitly selected: metadata-only list, redacted inspection, archive outside repo, commit after redaction, cleanup/delete, waive, or defer.
   - Default remains quarantine and `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

4. `bounded-evidence-archive-or-commit-scope-worker`
   - Purpose: prepare a scoped manifest for evidence trees selected by Luba.
   - Must separate evidence packets, command outputs, screenshots, user-data/cache/logs, package/binary artifacts, and traces.
   - Must not archive, stage, commit, delete, move, or restore until a later execution worker receives exact approval.

5. `bounded-local-residue-cleanup-decision-worker`
   - Purpose: prepare cleanup options and consequences for local residue categories.
   - Must include backup/rollback implications and exact paths.
   - Must stop before cleanup/delete/move/stash/restore unless Luba approves exact selector and policy.

6. `bounded-generated-artifact-policy-worker`
   - Purpose: classify generated artifacts into commit-worthy evidence, local run residue, package/binary artifacts, or quarantine.
   - Must not rely on filenames alone for publish safety.
   - Must not run tests/builds/runtime or inspect secrets.

## Final Boundary

This packet resolves only the classification layer for `evidence vs residue`.

It does not claim:
- clean repository state;
- product completion;
- merge readiness;
- full runtime readiness;
- full visual proof;
- external AI judge success;
- trace sensitivity safety;
- package provenance;
- commit readiness;
- cleanup readiness;
- handoff/index inclusion of this new packet.

Any future non-default action remains:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-core-evidence-residue-resolver-2026-06-28.md`

verification:
- Created this packet only.
- Post-write verification still required by the worker report.
