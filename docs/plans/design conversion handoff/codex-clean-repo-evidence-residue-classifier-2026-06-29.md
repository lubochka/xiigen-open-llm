# CLEAN_REPO_EVIDENCE_RESIDUE_CLASSIFIER - 2026-06-29

## Простое русское резюме

Правильная рабочая папка для текущего DesignToCode workflow - это `C:\Projects\llm_mvp_core_DesignToCode_worktree`.
В этой папке `DesignToCode worktree` сейчас выглядит чистым по read-only `git status --porcelain=v1 --untracked-files=all`: найдено `0` dirty/untracked entries.

Старая/оригинальная папка `C:\Projects\llm_mvp_core` не является чистой рабочей папкой для этого workflow. Там есть `original folder local residue`: read-only статус показывает `6926` entries, из них `6919` untracked, `4` deleted-status entries и `3` modified-status entries. Основная масса находится в `docs`, `artifacts` и `evidence`; это похоже на смесь доказательств, generated run outputs, trace files и локального residue, а не на текущую чистую DesignToCode рабочую папку.

Вывод: clean-repo evidence для текущего workflow относится только к `DesignToCode worktree`. Грязь в оригинальной папке надо считать отдельным локальным хвостом/evidence residue и не использовать как доказательство, что правильная DesignToCode папка грязная.

Этот отчет ничего не чистит. `no cleanup/revert/stash/restore`.

`Proof readiness items are tasks, not stop blockers`: оставшиеся proof/readiness пункты надо вести как будущие bounded work orders, а не как повод сейчас чистить репозиторий вслепую.

## Honesty Guard

claim_scope:
- Bounded read-only/doc-only clean-repo/evidence-residue classifier for the corrected DesignToCode workflow.
- Created one classifier file only.
- No cleanup, delete, move, stash, restore, revert, stage, commit, push, branch, PR, test, build, browser, runtime, external AI, secret, or repo mutation was performed.

evidence_scope:
- Read `codex-fresh-human-full-chain-audit-2026-06-29.md`.
- Read prior evidence/residue reports:
  - `codex-core-evidence-residue-resolver-2026-06-28.md`
  - `codex-core-evidence-dirt-manifest-2026-06-28.md`
  - `codex-figma-fixture-residue-resolver-2026-06-28.md`
  - `codex-wiki-figma-residue-classification-2026-06-28.md`
- Ran read-only git status/status-summary commands in:
  - `C:\Projects\llm_mvp_core`
  - `C:\Projects\llm_mvp_core_DesignToCode_worktree`
- Did not inspect contents of evidence trees, trace JSON files, browser/user-data/cache folders, generated packages, secrets, tests, builds, runtime, browser, PRs, or external services.

count_claimed:
- classifier reports created: 1
- files changed by this worker: 1
- cleanup/revert/stash/restore actions executed: 0
- repo/git/PR/test/build/runtime/browser/external-AI mutations executed: 0

independent_evidence_count:
- 12 evidence records before write: 5 report reads, 5 read-only status/status-summary commands, 1 filename search, 1 target-path existence check.

subagent_id_or_NONE:
- CLEAN_REPO_EVIDENCE_RESIDUE_CLASSIFIER

downgrade_needed:
- Yes for any broader claim. The evidence supports only current read-only status classification and document routing. It does not prove product completion, PR readiness, full merge readiness, runtime/browser readiness, visual fidelity, external AI judge success, trace sensitivity safety, commit readiness, or cleanup readiness.

## Current Folder Distinction

| Folder | Role in corrected workflow | Current read-only status evidence | Classification |
| --- | --- | --- | --- |
| `C:\Projects\llm_mvp_core_DesignToCode_worktree` | Correct working folder for DesignToCode workflow | `git status --porcelain=v1 --untracked-files=all` returned `TOTAL 0`; branch line from short status was `## DesignToCode` | Clean for current workflow evidence |
| `C:\Projects\llm_mvp_core` | Original folder, not the corrected DesignToCode worktree | `TOTAL 6926`: `?? 6919`, ` D 4`, ` M 3`; top-level: `docs 4175`, `artifacts 2678`, `evidence 64`, plus small root/tool/local entries | `original folder local residue`; needs selector before any cleanup |

Important boundary:
- The original folder being dirty does not invalidate the clean `DesignToCode worktree`.
- The clean `DesignToCode worktree` does not authorize cleanup of the original folder.
- Status metadata alone does not prove whether any untracked item is safe to publish, archive, ignore, or delete.

## Residue Classification

### Keep Evidence

Keep in place as evidence until Luba chooses a later selector:

| Bucket | Current evidence | Why keep now | Boundary |
| --- | --- | --- | --- |
| `docs/plans/evidence/extension-full-readiness-dual-path/*` | Current status bucket under `docs`; prior manifest classified thousands of entries as scenario evidence | Likely scenario proof packets, logs, screenshots, and planning evidence | Keep does not mean commit-ready or sensitivity-safe |
| `artifacts/extension-runtime-token-quality-learnability/*` | Current status bucket under `artifacts`; prior reports classify this as execution/proof evidence | Likely execution, host, token-quality, UI, and readiness evidence | Keep does not mean product-ready |
| `evidence/run/*` | Current status bucket `evidence 64`; prior manifest names flow/browser proof evidence | Saved proof boundary and run output evidence | Saved/browser proof is not full runtime or source-design fidelity proof |
| Handoff reports already read | Existing reports preserve classification history | They are the audit trail for why cleanup is not automatic | A report row is not a completed test/review cycle |

Default action now:
- preserve in place
- no stage/commit/push
- no archive/copy/delete/move
- no cleanup/revert/stash/restore

### Local Residue Needing Luba Decision

These need a Luba selector before action:

| Bucket | Current evidence | Decision needed |
| --- | --- | --- |
| `AGENTS.md` in original folder | Current original-folder status includes one modified root policy file | Preserve, compare, move through authorized governance process, or leave as local residue |
| tracked docs/plans state/review files in original folder | Current status has tracked modified entries; prior manifest identified plan/state/review files | Commit as evidence, defer, or handle through a scoped policy worker |
| root `xiigen-codex-request.json`, `xiigen-codex-response.json`, `xiigen-codex-trace.json` | Current status has three root JSON entries; prior reports classify trace/request/response files as sensitivity-quarantine | Redacted inspect, archive outside repo, commit after policy, waive, or approved cleanup |
| local worktree/status metadata entries in original folder | Current status has 4 deleted-status entries in local worktree metadata area | Owner decision before restore/cleanup; this classifier does not inspect or touch those files |
| `tools` single status entry | Current compact status shows one top-level `tools` entry | Exact selector must decide evidence, residue, quarantine, or ignore |

Default action now:
- decision needed
- no cleanup/revert/stash/restore
- no content printing of sensitive trace files
- no governance/policy mutation by this worker

### Safe To Ignore For Current DesignToCode Clean-Repo Claim

Safe to ignore only for the narrow question: "is the corrected DesignToCode worktree clean right now?"

| Item | Why safe to ignore for that narrow claim | What is not safe |
| --- | --- | --- |
| Dirt in `C:\Projects\llm_mvp_core` | It is the original folder, not `C:\Projects\llm_mvp_core_DesignToCode_worktree` | Not safe to delete, restore, stash, or call irrelevant forever |
| Older residue reports from 2026-06-28 | They explain classification history and do not change the current `DesignToCode worktree` status | Not safe to treat as final cleanup approval |
| Proof-readiness gaps from fresh audit | They are future tasks, not proof that the worktree is dirty | Not safe to claim product completion |

Default action now:
- ignore for the clean DesignToCode worktree status claim only
- keep in evidence accounting
- no cleanup/revert/stash/restore

### Needs Future Cleanup Selector

These are not cleanup-ready now. They need future bounded selector work after Luba chooses policy:

| Future selector target | Needed selector | Why |
| --- | --- | --- |
| Evidence trees under `docs/plans/evidence` | keep evidence vs commit evidence vs archive outside repo vs approved cleanup | Large mixed trees may include logs, screenshots, user-data, and generated files |
| Evidence trees under `artifacts` | commit/archive/defer selector plus sensitivity/provenance review | Evidence value is likely high, but publish safety was not checked |
| Root trace JSON files | sensitivity/no-secret policy and exact action selector | Request/response/trace content was not inspected |
| Browser/cache/user-data/log outputs | no-secret policy plus archive/delete/defer selector | May contain local/session/environment residue |
| Original folder tracked modifications | owner/governance selector | Could be meaningful governance/state work or local residue |
| Local worktree metadata statuses | owner/worktree selector | Status-only evidence is not enough for restore/delete decisions |

Default action now:
- `needs future cleanup selector`
- no cleanup/revert/stash/restore

## Future Bounded Work Orders After Luba Selector

1. `bounded-original-folder-residue-selector-worker`
   - Purpose: turn `original folder local residue` into exact Luba options.
   - Read scope: this classifier, fresh audit, prior residue reports, read-only status in `C:\Projects\llm_mvp_core`.
   - Write scope: one codex-prefixed selector packet in the handoff folder.
   - Output: exact categories for keep evidence, archive outside repo, commit candidate, ignore for current workflow, quarantine, or cleanup candidate.
   - Forbidden: cleanup/revert/stash/restore, stage, commit, push, branch/PR mutation, tests/builds/browser/runtime, external AI, secrets.

2. `bounded-trace-sensitivity-decision-worker`
   - Purpose: decide handling for `xiigen-codex-*.json`.
   - Preconditions: Luba supplies no-secret-print/redaction policy.
   - Output: redacted-inspect/archive/commit-after-policy/waive/cleanup decision packet.
   - Forbidden by default: printing raw sensitive contents, cleanup/revert/stash/restore, stage, commit, push.

3. `bounded-evidence-archive-or-commit-scope-worker`
   - Purpose: prepare commit/archive candidates for `docs/plans/evidence`, `artifacts`, and `evidence/run`.
   - Output: manifest separating screenshots, logs, traces, package/binary artifacts, cache/user-data, and durable proof packets.
   - Forbidden until a later execution order: archive, delete, move, stage, commit, push.

4. `bounded-cleanup-execution-worker-after-selector`
   - Purpose: perform cleanup only after Luba provides exact selector, paths, backup/rollback policy, and verification commands.
   - Required first step: re-run read-only status and compare with this classifier because the residue may change.
   - Allowed actions: only the exact selected cleanup action.
   - Forbidden unless explicitly selected: cleanup/revert/stash/restore.

5. `bounded-proof-readiness-task-planner`
   - Purpose: convert proof-readiness gaps into tasks, not stop blockers.
   - Must preserve literal rule: `Proof readiness items are tasks, not stop blockers`.
   - Output: exact task queue for runtime/browser, visual, merge/readiness, package/trace/corpora/fixture follow-up.
   - Forbidden: running tests/builds/browser/runtime/external AI unless separately authorized.

## Verification

Post-write verification required:
- File exists at `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-clean-repo-evidence-residue-classifier-2026-06-29.md`.
- Required literals are present:
  - `CLEAN_REPO_EVIDENCE_RESIDUE_CLASSIFIER`
  - `DesignToCode worktree`
  - `original folder local residue`
  - `no cleanup/revert/stash/restore`
  - `Proof readiness items are tasks, not stop blockers`

Class C yes-no:
- yes, for cleanup/commit/archive decisions that require Luba selector.
- no, for the narrow current clean-worktree evidence: the `DesignToCode worktree` status was directly checked and returned `0` entries.

## Final Boundary

This classifier says only:
- the corrected `DesignToCode worktree` is clean by current read-only status;
- the original folder has local evidence/residue;
- cleanup is not authorized;
- future cleanup needs exact Luba selector work.

It does not say:
- product is complete;
- PR is ready;
- merge is complete;
- runtime/browser proof passed;
- visual fidelity is proven;
- traces are sensitivity-safe;
- evidence is commit-ready;
- original folder may be cleaned now.
