# Q4 Pre-Commit Review/Fix Cycle 1 of 4

Status: PASS_WITH_REPAIR_ACCEPTED

Reviewed scope: Q4 OrchestrationReasoning changed-file set only: Q4 runner and reports, OrchestrationReasoning checkpoint/manifest/model index, orchestration-reasoning-expanded-v1 training data, material-quality report, and state. No staging, commit, push, broad tests, dashboard/API, host/plugin/runtime, Q5, or Q8 work was performed.

## Finding

P1 - Runner held-out prediction used held-out pair ids as authority.

`artifacts/smart-skills-quality-uplift/q4/run-orchestration-reasoning-q4-01.ps1` built held-out semantic signals with a `switch ($row.TrainingPairId)` and static-control rows with an exact `or-expanded-v1-pair-009` check. That violated the Q4 review requirement that runner logic must not use held-out id, pair id, source ref, filename, path, or guide title lookup as trained-result authority.

## Fix

Repaired the Q4 runner so held-out prediction signals are derived from semantic row fields: `ChosenScenarioRef`, `RejectedScenarioRef`, `NearMissStrategyRef`, and `ProvenanceRefs` boundary/quality reasons. The runner now:

- removes the `TrainingPairId` switch from `New-HeldoutSignals`;
- uses `$row.ChosenScenarioRef` only as the expected label for scoring;
- changes static/BM25 controls to taxonomy-level deterministic controls instead of exact pair-id controls;
- adds a universal bounded-subagent evidence-review aggregate pattern;
- keeps pair-009 repair as the reusable source-disjoint split-readiness aggregate pattern, not exact row memory.

Focused runner rerun was justified because checkpoint, manifest, model index, fresh-load, continue, held-out, ablation, and training reports depend on the runner output/hash.

## Evidence

- Branch: `codex/chat-gpt-5.5`.
- Focused runner rerun: PASS.
- Held-out result: 4/4, accuracy 1.0.
- Source-disjoint check: 8 train rows, 4 held-out rows, SourceManifestRef overlap 0.
- Current checkpoint SHA-256: `cf05df63ea868843756470428d74645b6f085dba874d6546b091da1df28b457a`.
- Current manifest SHA-256: `5f0acde3f7bb13dfd3a1e8040761dcef4397bd812d4970afce63bd6234b2cf43`.
- Current model index SHA-256: `3c6a528001110e8b1494b5e4dbcc249f367c5b0e23562dfaf23726e8d5599684`.
- Manifest checkpoint hash matches actual checkpoint: PASS.
- Model index checkpoint/manifest hashes match actual files: PASS.
- Training, fresh-load, and continue reports point to the current checkpoint: PASS.
- Deterministic controls: no-trained 0.0, exact guide-title/path/filename 0.0, BM25/title surrogate 0.25, static route table 0.25; all below trained 1.0.
- Checkpoint contains `or-expanded-v1-pair-009`: false.
- Checkpoint contains pair-009 held-out source markers: false.
- `semantic_quality_claimed=false` in checkpoint, manifest, model index, training report, held-out report, ablation report, fresh-load report, and continue report.

## Scope/Honesty Notes

This PASS is commit-readiness cycle 1 only. It proves bounded Q4 seed asset/report consistency after the runner authority repair. It does not claim full semantic quality, host/plugin/runtime readiness, dashboard/API evidence, Q5, Q8, or final plan completion.

Historical Q4-02/Q4-03 review JSON snapshots may still contain their original old hash fields as historical evidence. Commit readiness for current files is superseded by this cycle-1 report and the updated state entry.

## Verification Run

- PowerShell JSON/JSONL/hash checks over Q4 runner outputs, manifest, model index, checkpoint, training data, split, schema, and dataset card: PASS.
- Targeted runner text check: no `switch ($row.TrainingPairId)` held-out prediction authority remains.
- Targeted runner text check: expected scoring uses `$row.ChosenScenarioRef`.
- Targeted runner text check: static control no longer keys on `or-expanded-v1-pair-009`.
- Targeted `rg` still finds `or-expanded-v1-pair-009` only as expected row/report/review evidence, not as runner prediction authority.
- `git diff --check`: PASS.

## Verdict

PASS_WITH_REPAIR_ACCEPTED. Cycle 1 found and fixed a real Q4 defect. Current Q4 commit candidate remains bounded seed asset/report readiness only with no semantic-quality overclaim.

Exact next action: `Q4_PRE_COMMIT_REVIEW_FIX_CYCLE_2_OF_4`.
