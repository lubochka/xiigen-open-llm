# REVIEW_FIX_INTERNAL: source metadata fill 20260618-065930

Verdict: PASS_WITH_QUARANTINED_UNTRACKED_SCOPE.

The previous FAIL was a false stop-gate for commit safety. The source-metadata-fill package itself is internally consistent after one small state repair, and the only dirty item outside the metadata-fill phase is the known quarantined old Cursor user-data folder:

`artifacts/extension-runtime-token-quality-learnability/qsg21-live-runtime-target-probes/cursor-extension-host/cursor-user-data/`

This path is outside my allowed write scope and must remain unstaged, unmoved, undeleted, and not deeply inspected in this phase. Its existence alone is not a real blocker for the exact metadata-fill phase commit because the safe staging command names only the state file and the metadata-fill run folder.

## Counts Verified

- Scenario coverage: 12/12 scenarios.
- Pair-row coverage: 54/54 pair rows.
- Source/doc candidate resolved: 11/12 scenarios and 51/54 pair rows.
- Missing source/doc candidate: `TQ-002-prompt-task-interrupt`, 3/54 pair rows.
- License proven: 0/12 scenarios and 0/54 pair rows.
- Train/held-out partition proven: 0/12 scenarios and 0/54 pair rows.
- Full strict metadata resolved: 0/12 scenarios and 0/54 pair rows.
- Strict QUALITY-03 controls: NOT READY.

## Hard Checks

- Deliverables exist: PASS.
- JSON/JSONL parse: PASS via Node parser. `candidate-reference-matches.jsonl` has 12 rows; `source-metadata-filled-manifest.jsonl` has 66 rows, covering 12 scenarios plus 54 pair rows.
- No false truthy claims in reviewed JSON/state: PASS.
- Unknown licenses and missing train/held-out partitions are explicit blockers: PASS.
- Matched references are local path-like references with no URL/web refs and no missing path-like refs: PASS.
- Universal-vs-stack-specific routing: PASS. Common training rows = 0; host labels are matrix coverage only.
- State human readability: PASS_WITH_REPAIR_ACCEPTED. Repaired stale human-readable `done` entry that pointed at `SOURCE-DISJOINT-ABLATION` instead of this metadata-fill prep package.
- `git diff --check`: PASS.
- No forbidden staged old Cursor user-data changes: PASS_WITH_QUARANTINED_UNTRACKED_SCOPE. The old Cursor user-data folder is a known quarantined untracked path outside the exact staging scope; it must remain unstaged and unmodified.

## Safe Parent Commit Instructions

Exact staging command:

```powershell
git add -- docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json artifacts/extension-runtime-token-quality-learnability/quality-controls-source-metadata-fill/quality-controls-source-metadata-fill-20260618-065930/
```

After staging, run:

```powershell
git diff --cached --name-only
```

The staged set must contain only `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json` plus files under `artifacts/extension-runtime-token-quality-learnability/quality-controls-source-metadata-fill/quality-controls-source-metadata-fill-20260618-065930/`. The quarantined `artifacts/extension-runtime-token-quality-learnability/qsg21-live-runtime-target-probes/cursor-extension-host/cursor-user-data/` path may still appear in `git status --short` as untracked and must not be staged.

`git add --dry-run` for the exact command listed only the state file and files under the metadata-fill run folder. It did not list `cursor-user-data/`.

## Repairs Made

- `artifacts/extension-runtime-token-quality-learnability/quality-controls-source-metadata-fill/quality-controls-source-metadata-fill-20260618-065930/review-source-metadata-fill-20260618-065930.json`
  - Reclassified the old cursor-user-data untracked folder from blocker to known quarantined untracked scope.
  - Recorded exact safe parent staging command and expected unstaged quarantine path.
- `artifacts/extension-runtime-token-quality-learnability/quality-controls-source-metadata-fill/quality-controls-source-metadata-fill-20260618-065930/review-source-metadata-fill-20260618-065930.md`
  - Updated this human-readable review verdict and next action to match exact scoped commit safety.
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`
  - Updated `updated_at_local`.
  - Replaced the stale `human_readable_status.done` entry with the current source-metadata-fill run and exact 12/12, 54/54, 11/12, 51/54, TQ-002, license, partition and strict-control status.

No source code, model, checkpoint, locator, training, plan, or Cursor user-data files were edited.

## Next Bounded Action

Parent may commit/push the metadata-fill phase using the exact scoped staging command above, leaving:

`artifacts/extension-runtime-token-quality-learnability/qsg21-live-runtime-target-probes/cursor-extension-host/cursor-user-data/`

unstaged and unmodified.

After that commit/push, the next product phase remains: fill missing approved source/license/train-vs-held-out refs and required no-leakage/no-memorization/ablation evidence before strict QUALITY-03 controls rerun.
