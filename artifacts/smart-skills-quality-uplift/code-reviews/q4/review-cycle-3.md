# Q4 Pre-Commit Review/Fix Cycle 3 of 4

Status: PASS_WITH_REPAIR_ACCEPTED

Reviewed scope: Q4 OrchestrationReasoning changed-file set only: Q4 runner and reports, OrchestrationReasoning checkpoint/manifest/model index, orchestration-reasoning-expanded-v1 training data, material-quality report, review cycles 1-2, and state. No staging, commit, push, broad tests, dashboard/API, host/plugin/runtime, Q5, or Q8 work was performed.

## Finding

P1 - Runner still used held-out provenance rationale text as inference authority.

`artifacts/smart-skills-quality-uplift/q4/run-orchestration-reasoning-q4-01.ps1` no longer used held-out pair ids or expected scenario labels after cycles 1-2, but `New-HeldoutSignals` still included `ProvenanceRefs.chosen_quality_reason` and `ProvenanceRefs.rejected_quality_reason` in held-out feature text. Those fields are exact row rationale text about why the chosen answer is good and the rejected answer is wrong. For cycle 3, that was too close to using row construction metadata as answer authority.

## Fix

Removed `chosen_quality_reason` and `rejected_quality_reason` from `New-HeldoutSignals`. Held-out inference signals now come only from `NearMissStrategyRef` and `ProvenanceRefs.teaches_boundary`, while labels, ids, split metadata, source refs, taxonomy refs, source quality, filenames, paths, titles, BM25/static-control metadata, and exact rationale text stay out of held-out feature extraction.

Focused Q4 runner rerun was justified because runner logic changed and checkpoint, manifest, model index, fresh-load, continue, held-out, ablation, and training reports depend on the runner output/hash.

## Evidence

- Branch: `codex/chat-gpt-5.5`.
- Focused runner rerun: PASS.
- Held-out result after repair: 4/4, accuracy 1.0.
- Source-disjoint check: 8 train rows, 4 held-out rows, SourceManifestRef overlap 0.
- Current runner SHA-256: `c2b2e085442210cb1f72dddff33239495a2f77a1ea5bdd105210c6b5d2ec2afc`.
- Current checkpoint SHA-256: `64470dcb1f65b7cc0ae6727fcbd92a8e2cbe70487b9126278028bb47cb3c7361`.
- Current manifest SHA-256: `9018cee05cdb87fa51211ac18e96496e8a6b09f1b07886f493ef6abe11686d4b`.
- Current model index SHA-256: `0b4a28dd295b4a4c9641e6f788bdf6d029417644602f253aa245405e92c928a0`.
- Current training report SHA-256: `0d5bc195074da1692bf7de6394a3eaade9a9b77f4f2cdec77d1f5677dbe140a7`.
- Current held-out report SHA-256: `edfa96daf74a91ee87486f6ce7c3c7b91f5f63d5c1e1e915c762d689af785623`.
- Current ablation report SHA-256: `a0fdec4e6d0125f48d3355ff5e7370dc51a23a5a8d4099f446e8153fc9aaa868`.
- Current fresh-load report SHA-256: `8281d78e1845386b7fdc412328d2d3be093df1dd612165d2cccaba53fd27e239`.
- Current continue report SHA-256: `f1b372bac54b3b954cd04a5807c3d47d7034ad57d6b710eb94da19a8cf0ff4a1`.
- Training data SHA-256: `7cc7c94d3d1d160807eebc404a132342479eb74b50300de638c8c80ac24a95f7`.
- Split/schema/dataset-card SHA-256: `e9ae5934517b5f0eabf325526e3a2d1b364726ec8ed4598e394e11998aae99ea`, `e7b9b99812f67fea09d7e83ec630c22b7e85fdcf9069fced52fb992c72d1d260`, `126e47576bcaa9c716563ff3c92b639ddefb8789b9d950dd6033aba304cbefe7`.
- Manifest checkpoint hash matches actual checkpoint: PASS.
- Model index checkpoint/manifest hashes match actual files: PASS.
- Training, fresh-load, and continue reports point to the current checkpoint: PASS.
- Deterministic controls: no-trained 0.0, exact guide-title/path/filename 0.0, BM25/title surrogate 0.25, static route table 0.25; all below trained 1.0.
- `New-HeldoutSignals` contains none of `TrainingPairId`, `SourceRowId`, `ApprovedPairRef`, `ChosenScenarioRef`, `RejectedScenarioRef`, `SourceManifestRef`, `SplitRole`, `HeldOutSetId`, `ErrorCode`, `ErrorTaxonomyRef`, `source_quality`, `chosen_quality_reason`, `rejected_quality_reason`, filename/path/title/BM25 terms.
- Current checkpoint stores accepted train pair ids 001-008 only; held-out pair ids 009-012 are not accepted refs and no held-out ids/source refs are stored as learned authority.
- `semantic_quality_claimed=false` in checkpoint, manifest, model index, training report, held-out report, ablation report, fresh-load report, and continue report.

## Scope/Honesty Notes

This PASS is commit-readiness cycle 3 only. It proves bounded Q4 seed asset/report consistency after removing held-out provenance-rationale leakage from feature extraction. It does not claim full semantic quality, host/plugin/runtime readiness, dashboard/API evidence, Q5, Q8, or final plan completion.

Historical Q4 review JSON snapshots may still contain old hashes as historical evidence. Commit readiness for current files is superseded by this cycle-3 report and the updated state entry.

## Verification Run

- PowerShell JSON/JSONL parse checks over Q4 runner outputs, manifest, model index, checkpoint, training data, split, schema, and dataset card: PASS.
- PowerShell hash consistency checks over checkpoint, manifest, model index, training, held-out, ablation, fresh-load, continue, runner: PASS.
- Targeted runner function check: `New-HeldoutSignals` excludes pair ids, source refs, split fields, taxonomy/error-code fields, source-quality fields, answer-label refs, quality-rationale text, filename/path/title/BM25 terms.
- Deterministic controls remain degraded after repair.
- `git diff --check`: PASS.

## Verdict

PASS_WITH_REPAIR_ACCEPTED. Cycle 3 found and fixed a real Q4 defect: held-out exact provenance rationale text was still used as inference text after label refs were removed. Current Q4 commit candidate remains bounded seed asset/report readiness only with no semantic-quality overclaim.

Exact next action: `Q4_PRE_COMMIT_REVIEW_FIX_CYCLE_4_OF_4`.
