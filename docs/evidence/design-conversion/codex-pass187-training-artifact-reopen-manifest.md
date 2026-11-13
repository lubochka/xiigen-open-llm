# Codex Pass187 - Training Artifact Reopen Manifest

claim_scope: Pass187 evidence-only repair for DesignToCode local artifact DB/training persistence reproducibility. This pass creates a consolidated reopen manifest from prior evidence so a future worker can identify the DB, DPO, training, and checkpoint evidence without rerunning the whole chain first. It does not create production DB data, train a model, run broad tests, edit product code, or claim ProductReady.

evidence_scope: Fresh reads of the required AGENTS/skill files; full reads of Pass101, Pass103, Pass153, Pass154, Pass157, Pass164R, Pass170, Pass171, and Pass179 artifacts; targeted reads of Pass185 and Pass186 count artifacts; current inventory/path checks under `docs/evidence/design-conversion/artifacts`; and this evidence manifest. No safe worktree/copy/source repo, `C:\Users\lubao\Documents\xiigen`, product code, test source, build output, git branch/ref, cloud/provider/deploy target, or external service was mutated.

count_claimed: 1 Pass187 evidence manifest artifact only. This is not a 50-pass completion claim, not a 20-fix/repair completion claim, not ProductReady, and not a product-code implementation pass.

independent_evidence_count: 5 evidence groups: required guidance reads; prior storage/training/pass artifact reads; artifact-directory and representative path checks; Pass185/Pass186 count-status reads; post-write verification commands. These are evidence groups, not cycles or independent passes.

subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

skill_load_status:

| Required file | Status in this pass | Applied boundary |
|---|---|---|
| `C:\Projects\llm_mvp_core\AGENTS.md` | found/read command completed; large visible output truncated by tool display | No overclaim, no invented verification, no permission prompts under full access, local evidence is not ProductReady. |
| `C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md` | found/read command completed; large visible output truncated by tool display | Required skill load before status, no ProductReady upgrade, evidence-only quoted context. |
| `C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md` | found/read command completed; large visible output truncated by tool display | Claims must stay inside read/run/edit evidence; schema or storage tests do not prove production training. |
| `C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md` | found/read | QA evidence stays bounded; DPO/training quality claims require exact evidence. |
| `C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md` | found/read | Trainable/code evidence needs numeric/runtime boundaries; no product-code review was claimed here. |
| `C:\Projects\llm_mvp_core\docs\skills\audit-protocol-SKILL.md` | found/read | Count-index snapshots are not completion; count status must be current and bounded. |

## Files Read

- `C:\Projects\llm_mvp_core\AGENTS.md`.
- `C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md`.
- `C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md`.
- `C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md`.
- `C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md`.
- `C:\Projects\llm_mvp_core\docs\skills\audit-protocol-SKILL.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass101-runtime-artifact-db-persistence.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass103-post-readback-product-queue-refresh.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass153-training-store-autowrite.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass154-loop-to-training-store.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass157-retrain-promotion-boundary.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass164R-loop-training-promotion.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass170-platform-flow-real-modelserver-allup.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass171-post-pass170-final-audit.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass179-post-caller-final-review.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass185-source-materialization-readiness-map.md`.
- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass186-spawned-worker-self-denial-repair.md`.
- Current inventory of `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\artifacts`.

## Files Changed

- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass187-training-artifact-reopen-manifest.md`.

## Commands Run

- `rg --files` discovery for required guidance, prior pass artifacts, DesignConversion artifact inventory, and possible existing reopen/manifest generators.
- `Get-Content -Raw` for the required guidance files and prior evidence artifacts listed above.
- `rg -n` targeted searches for `conversion-results.jsonl`, `conversion-results.index.json`, JSONL/index reopen behavior, DPO candidates, source-disjoint training, checkpoint promotion, fresh-load, and current count status.
- `Get-ChildItem -Recurse -File` under `docs/evidence/design-conversion/artifacts`.
- `Test-Path` representative checks for the artifact directory and expected DB filenames under the evidence artifact directory.
- Post-write field-count, conflict-marker, configured mojibake-marker, readback, and `git diff --check` verification commands.

## Manifest Summary

Prior passes prove local artifact/training persistence in bounded runtime/test paths, but before Pass187 there was no single reopen manifest in `docs/evidence/design-conversion` that told a future worker which artifact classes existed, which files to look for, and which files were only temp/test-time outputs. This file is the bounded evidence repair.

The evidence supports only this narrow statement:

`JsonlDesignConversionLocalArtifactDb`-backed local tests and runtime paths can write and reopen sanitized `conversion-results.jsonl` plus `conversion-results.index.json`; approved runtime-created DPO candidates can feed local source-disjoint train/held-out splitting and local promotion; promoted checkpoints can be fresh-loaded in local tests. The current checked-in evidence folder does not contain a reusable sample DB/checkpoint payload for future workers to reopen without either rerunning a focused producer test or adding a later sanitized sample artifact.

## Reopen Manifest

| Artifact class | Prior evidence that proves creation/open behavior | Persisted filename or locator from evidence | Current repo evidence/artifacts status | Future reopen instruction |
|---|---|---|---|---|
| Local artifact DB records | Pass101 proves runtime append and test read of DB files. Pass153 proves append/read reopen with two JSONL lines and index schema. Pass154 proves improvement-loop final candidate is appended and a fresh DB instance returns it. | `conversion-results.jsonl`; `conversion-results.index.json`; schema `design-conversion-local-artifact-db-v1`. | Expected filenames are not present under `docs/evidence/design-conversion/artifacts` in this pass. Prior evidence indicates the files were produced in focused test/temp paths, not preserved as checked-in evidence payloads. | To reopen actual rows, use the focused producer tests/commands from Pass153 or Pass154, or add a later sanitized sample DB artifact under `docs/evidence/design-conversion/artifacts` with explicit row count and index hash. |
| Runtime-created DPO candidate rows | Pass153 proves runtime autowrite includes runtime-created DPO candidate fields and later DPO construction source. Pass154 proves one loop-created DPO candidate is persisted with review-pending/product-not-ready metadata. | Stored inside local artifact DB records, not as a standalone checked-in `dpo-rows.jsonl` file in the evidence folder. | No standalone DesignConversion DPO JSONL file was found under the current evidence artifact directory. | Future worker should query the local artifact DB/review API after recreating or receiving a DB path; do not treat the manifest itself as DPO rows. |
| Approved training candidates and source-disjoint split | Pass157 proves two approved source-disjoint candidates can run local retrain/promotion boundary. Pass164R proves 100 approved loop-created candidates, 100 distinct source manifests/structures, 50 train records, 50 held-out records, and source-disjoint count gate success. | Candidate rows are local DB/training-intake objects in tests; key values include `ApprovedCandidateCount=100`, train/held-out `50/50`, `SourceDisjoint=true`, and `product_ready=false`. | No preserved 100-row DesignConversion training dataset file was found under `docs/evidence/design-conversion/artifacts`. | Future worker can use Pass164R test command as the producer route, then record the generated DB/training rows into a later artifact if persistent row inspection is required without rerun. |
| Local training run and promotion evidence | Pass157 proves checkpoint mutation, real weights, measurement pass, local promotion readiness, and fresh-load through `DesignConversionPromotedLocalModelLoader.LoadAsync`. Pass164R proves local promotion from 100 loop-created candidates and ModelServer use of promoted checkpoint. | Local-only promotion ref shape `local://design-conversion/promoted/`; checkpoint/tokenizer/policy artifacts are produced by the local promotion bridge in tests. | No reusable promoted DesignConversion checkpoint directory or checkpoint manifest was found under `docs/evidence/design-conversion/artifacts`. | Future worker should use Pass157 or Pass164R producer tests to materialize a temp promoted checkpoint, then capture a sanitized checkpoint manifest in a later evidence artifact if reopen-without-rerun is required. |
| Platform/FLOW all-up promoted checkpoint readback | Pass170 proves safe platform request to FLOW to real in-process ModelServer with browser-backed local render and fresh promoted checkpoint loaded during test. Pass171 and Pass179 summarize this as bounded-local/safe-worktree coverage, not production readiness. | Pass170 local test-created checkpoint; markers include `CheckpointFreshLoadVerified`, `CheckpointContainsRealTrainedWeights`, `LocalOnly`, and `local_promoted_checkpoint_loaded=True`. | The all-up checkpoint is test-time/local safe-worktree evidence; no portable checkpoint artifact is present in this repo evidence folder. | Future worker should start from Pass170 command if the all-up route must be reproved; do not infer production checkpoint availability from this manifest. |

## Producer Command Index

These commands are copied as evidence references from prior pass artifacts. They were not rerun in Pass187.

```text
Pass153 focused storage/runtime command:
dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_RoundTripsConversionRenderJudgeAndTrainingCandidateWithoutPrivateFields|FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_AppendsRecordsAndWritesSchemaVersionIndex|FullyQualifiedName~DesignConversionThinClientJobRuntimeTests.RunAsync_WithConfiguredLocalArtifactDbPersistsSanitizedCompletedEvaluation|FullyQualifiedName~DesignConversionThinClientJobRuntimeTests.QueryLocalArtifactReadbackAsync_ReturnsRuntimePersistedRecordAndCreatedDpoRows|FullyQualifiedName~DesignConversionLocalFullLoopE2eTests.LocalNonProviderLoop_SavesConversionDisplaysUsefulnessAndGatesTrainingRows" --no-restore --logger "console;verbosity=minimal"

Pass154 focused loop-to-store command:
dotnet test --filter "FullyQualifiedName~DesignConversionImprovementLoopCoordinatorTests.RunAsync_WithLocalArtifactDbPersistsFinalLoopCandidateForDpoSourceWithoutRawPrivatePayload"

Pass157 focused retrain/promotion command:
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionDpoStorageTests.Pass157_LocalArtifactDb_UserRequestedRetrainPromotesLocalModelServerCheckpointOnlyAfterGates"

Pass164R count-gate/promotion command:
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionModelServerImprovementLoopTests.Pass164R_LoopCreatedRuntimeCandidatesPassDefaultCountGatePromoteAndModelServerUsesCheckpoint" --logger "console;verbosity=minimal"

Pass170 all-up safe-worktree command:
C:\Projects\dotnet-sdk-10\dotnet.exe test C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75\tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj --filter FullyQualifiedName~SubmitExplicitBrowserBackedPromotedLocalCompositionInvokesRealFlowAndRealModelServerWithPromotedCheckpoint --logger "console;verbosity=minimal" -p:NuGetAudit=false
```

## Representative Path Verification

| Path checked | Status | Meaning |
|---|---|---|
| `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass101-runtime-artifact-db-persistence.md` | exists | Prior runtime DB append evidence is available. |
| `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass153-training-store-autowrite.md` | exists | Prior JSONL/index reopen evidence is available. |
| `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass154-loop-to-training-store.md` | exists | Prior loop-to-DB and DPO candidate evidence is available. |
| `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass157-retrain-promotion-boundary.md` | exists | Prior local retrain/promotion evidence is available. |
| `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass164R-loop-training-promotion.md` | exists | Prior 100-candidate source-disjoint count-gate/promotion evidence is available. |
| `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass170-platform-flow-real-modelserver-allup.md` | exists | Prior all-up promoted-checkpoint readback evidence is available. |
| `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\artifacts` | exists | Current evidence artifact directory exists. |
| `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\artifacts\conversion-results.jsonl` | missing | No reusable DB JSONL payload is currently preserved at that generic evidence path. |
| `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\artifacts\conversion-results.index.json` | missing | No reusable DB index payload is currently preserved at that generic evidence path. |

## Remaining Gaps

- This manifest is an index and boundary record, not the actual local artifact DB, DPO row file, training dataset, or promoted checkpoint.
- The prior evidence proves the DB/index can be produced and reopened in focused local tests, but the current evidence artifact directory does not preserve representative rows/checkpoint files for a future worker to open without rerunning a producer route.
- Production DB deployment, cloud model upload, public endpoint, external provider execution, production browser farm, CI, commit, push, merge, PR, and ProductReady remain not attempted and not proven.
- No tiny DesignConversion-specific reopen manifest generator was found in the narrow `docs`/`tools` search; no product code or test/data generator was edited or run.

## Verification Results

- Manifest readback showed the claim, skill-load table, reopen manifest, representative path table, count status, and verdict.
- Identity-key count: 1.
- Pass187 verdict-key count: 1.
- Line-anchored conflict-marker count: 0.
- ASCII/non-mojibake check: non-ASCII character count 0, so the configured mojibake markers are absent from this artifact.
- Representative path checks found the prior evidence files and artifact directory present, while the generic DB JSONL/index payload paths under the artifact directory are explicitly missing.
- `git diff --check -- docs/evidence/design-conversion/codex-pass187-training-artifact-reopen-manifest.md` exited 0 with no output.

current_count_status: After Pass186 accepted = 15/50 main sequential evidence passes and 11/20 strict fix/repair passes. If Pass187 is accepted = 16/50 main sequential evidence passes and 12/20 strict fix/repair passes. This artifact does not self-accept Pass187 and does not claim 50 done, 20 done, ProductReady, source materialization, CI, commit, push, merge, PR, deploy, cloud, provider execution, or production DB readiness.

pass187_verdict: PASS_TRAINING_ARTIFACT_REOPEN_MANIFEST_REPAIR

downgrade_needed: No downgrade for the narrow Pass187 evidence-manifest repair if post-write verification remains clean. Downgrade remains mandatory for any claim that a reusable DB/checkpoint payload exists in the evidence artifact directory, that production DB/cloud/deploy/provider readiness is proven, or that the 50/20 pass counts are complete.
