# Codex Pass113 - Training Execution Semantics

claim_scope: Narrow Pass113 implementation/repair for local DesignConversion training-intake run semantics.
evidence_scope: Current-turn code inspection, targeted implementation, focused DesignConversion test execution, conflict/mojibake/diff scans, and required readback commands listed below.
count_claimed: 1 narrow Pass113 packet, pending parent reconciliation.
independent_evidence_count: 9 current-turn command/readback records used for this narrow pass.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
subagent_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true
downgrade_needed: no for the narrow Pass113 semantics after tests and scans pass; yes for any full-product, real-weight, or upload-readiness claim.

## Skill Load Status

- AGENTS.md: found/read.
- docs/SESSION-START-PROMPT.md: found/read.
- docs/skills/planning-session-startup-SKILL.md: found/read.
- docs/skills/architect-behavior-classifier-SKILL.md: found/read.
- docs/skills/authority-requirement-binding-SKILL.md: found/read.
- docs/skills/reference-inventory-and-source-map-SKILL.md: found/read.
- docs/skills/phase-planning-SKILL.md: found/read.
- docs/skills/plan-review-SKILL.md: found/read.
- docs/skills/goal-delivery-completeness-SKILL.md: found/read.
- docs/skills/claim-verification-SKILL.md: found/read.
- docs/skills/audit-protocol-SKILL.md: found/read.
- docs/skills/specificity-calibration-SKILL.md: found/read.
- docs/skills/output-skepticism-SKILL.md: found/read.
- docs/skills/reconnaissance-gate-SKILL.md: found/read.
- docs/skills/trainable-architecture-decision-SKILL.md: found/read.
- docs/skills/no-product-decisions-SKILL.md: found/read.
- docs/skills/output-readability-gate-SKILL.md: found/read.
- docs/skills/implementation-doc-uml-SKILL.md: found/read.
- docs/skills/implementation-doc-algorithms-SKILL.md: found/read.
- docs/skills/implementation-doc-code-refs-SKILL.md: found/read.
- docs/skills/implementation-doc-status-SKILL.md: found/read.
- docs/skills/implementation-doc-hierarchy-SKILL.md: found/read.

## Files Read

- 44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs
- 44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs
- 44.DesignConversionDomain/DesignConversionTrainingRunner.cs
- 44.DesignConversionDomain/DesignConversionTrainingPipeline.cs
- 44.DesignConversionDomain/DesignConversionMachine.cs
- 44.DesignConversionDomain/DesignConversionLocalFreshLoadProbe.cs
- 44.DesignConversionDomain/DesignConversionLocalContinueTrainingProbe.cs
- 44.DesignConversionDomain/DesignConversionPromotion.cs
- 44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs
- Tests/DesignConversionDpoStorageTests.cs
- Tests/DesignConversionDpoUsefulnessApiTests.cs
- Tests/DesignConversionThinClientJobRuntimeTests.cs
- Tests/DesignConversionLocalFullLoopE2eTests.cs
- Tests/DesignConversionTrainingPipelineTests.cs
- docs/evidence/design-conversion/codex-pass111-intake-to-training-run-bridge.md
- docs/evidence/design-conversion/codex-pass112-source-manifest-disjoint-proof.md

## Files Changed

- 44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs
- Tests/DesignConversionDpoStorageTests.cs
- docs/evidence/design-conversion/codex-pass113-training-execution-semantics.md

## Commands Run

- `rg -n "Pass113|DesignConversion|Pass111|Pass112|TrainingStarted|CheckpointMutated|RunTrainingIntakeAsync" C:\Users\lubao\.codex\memories\MEMORY.md`
- Skill/context reads with `Get-Content`/`rg` against the required local files.
- `rg -n "DesignConversionTrainingIntakeRunnerEvidence\(|new DesignConversionTrainingIntakeRunnerEvidence|TrainingExecutionStatus|CheckpointMutationStatus|WeightProofStatus" 44.DesignConversionDomain Tests`
- `tool_search` for subagent/delegation tooling; no callable sub-agent tool was exposed.
- `dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionThinClientJobRuntimeTests|FullyQualifiedName~DesignConversionDpoUsefulnessApiTests|FullyQualifiedName~DesignConversionDpoStorageTests|FullyQualifiedName~DesignConversionLocalFullLoopE2eTests|FullyQualifiedName~DesignConversionTraining" --no-restore --logger "console;verbosity=minimal"`
- `rg -n "^(<<<<<<<|=======|>>>>>>>)" -- 44.DesignConversionDomain\DesignConversionLocalArtifactDb.cs Tests\DesignConversionDpoStorageTests.cs docs\evidence\design-conversion\codex-pass113-training-execution-semantics.md`
- `rg -n <configured mojibake marker set> -- 44.DesignConversionDomain\DesignConversionLocalArtifactDb.cs Tests\DesignConversionDpoStorageTests.cs docs\evidence\design-conversion\codex-pass113-training-execution-semantics.md`
- `git diff --check -- 44.DesignConversionDomain\DesignConversionLocalArtifactDb.cs Tests\DesignConversionDpoStorageTests.cs docs\evidence\design-conversion\codex-pass113-training-execution-semantics.md`
- `rg -n "subagent_id_or_NONE|subagent_identity_self_check|RunnerInvoked|TrainingStarted|TrainingExecutionStatus|Checkpoint|Weights|ModelUploadReady" -- 44.DesignConversionDomain\DesignConversionLocalArtifactDb.cs Tests\DesignConversionDpoStorageTests.cs docs\evidence\design-conversion\codex-pass113-training-execution-semantics.md`

## Tests Result

- First focused test run built successfully but failed one new assertion because run warnings inherited stale preflight-only markers.
- After repair, the required focused test command passed: Failed 0, Passed 38, Skipped 0, Total 38.

## Implementation Summary

- `DesignConversionTrainingIntakeRunnerEvidence` now records `TrainingStarted`, `TrainingExecutionStatus`, `RunnerBlockedReason`, `CheckpointMutationStatus`, `CheckpointMutationEvidenceProven`, and `WeightProofStatus`.
- `DesignConversionTrainingIntakeRunResult` now exposes `TrainingExecutionStatus`, `RunnerBlockedReason`, `CheckpointStatus`, `CheckpointMutationStatus`, and `WeightProofStatus`.
- `TrainingStarted` is no longer inferred from `RunTrainingIntakeAsync` or from `RealTrainingRunClaimed`; it is true only when the runner entered the local lifecycle path, the pipeline attempted the machine lifecycle hook, the dataset count gate passed, and a local lifecycle batch result exists.
- Missing or same-source Pass112 source proof still blocks before runner invocation.
- Distinct-source intake can now distinguish `RunnerInvoked=true` with `TrainingStarted=false` when the runner is blocked by the dataset count gate.
- A controlled local test can now prove `TrainingStarted=true` for the local scaffold lifecycle while keeping `CheckpointContainsRealTrainedWeights=false`, `WeightsProduced=false`, and `ModelUploadReady=false`.
- Checkpoint mutation is recorded only from the local continue-training probe evidence: source checkpoint unchanged, continuation batch appended, no replay batch id, and training history count increased in the continued checkpoint report.
- Run warnings now remove stale preflight-only markers after runner invocation, training start, or checkpoint mutation evidence supersedes them.

## Mental Simulation Result

Approved distinct-source intake goes through preflight and may reach the runner. With default count gates, the runner is invoked but training is blocked by the dataset count gate, so `RunnerInvoked=true`, `TrainingStarted=false`, `TrainingExecutionStatus=runner_invoked_training_blocked_dataset_count_gate`, and `RunnerBlockedReason=dataset_count_gate_not_passed`.

With controlled local options that satisfy the count gate, the runner enters the existing local lifecycle scaffold. The machine records lifecycle training history and the fresh-load/continue probe produces checkpoint mutation metadata, so `TrainingStarted=true`, `TrainingExecutionStatus=local_lifecycle_training_started_scaffold_only`, `CheckpointMutated=true`, and `CheckpointMutationStatus=local_continue_checkpoint_mutated_metadata_only`. The machine checkpoint still exports `LearnedWeightsPresent=false`, so `CheckpointContainsRealTrainedWeights=false`, `WeightsProduced=false`, `WeightProofStatus=real_trained_weights_not_proven`, and `ModelUploadReady=false`.

## Boundaries Not Proven

- Real learned weights are not proven.
- Upload readiness is not proven.
- Full product completion is not proven.
- Server API, UI, deployment, external provider execution, and production database readiness are not proven.
- The local checkpoint mutation evidence is metadata/history continuity evidence from the existing continue-training probe; it is not a real learned-weights claim.

## Remaining Queue

- Integrate or prove a real DesignConversion trainer that mutates learned weights.
- Persist and promote a checkpoint only after fresh-load and learned-weight evidence exists.
- Add upload readiness only behind a separate proven promotion/upload gate.
- Continue server/API/UI/full-product work separately.

## Post-Write Verification

- conflict_marker_scan: passed; no conflict markers found in the changed files.
- mojibake_marker_scan: passed; no configured mojibake markers found in the changed files.
- git_diff_check: passed; `git diff --check` exited 0 with no output for the changed files.
- required_readback: passed; readback found `subagent_id_or_NONE`, `subagent_identity_self_check`, `RunnerInvoked`, `TrainingStarted`, `TrainingExecutionStatus`, `Checkpoint`, `Weights`, and `ModelUploadReady` in the packet, domain file, and tests.

## Packet Self Check

- This packet uses `subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT`, not `NONE`.
- subagent_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true.
- The packet does not claim full product completion, real learned weights, or upload readiness.
- The packet does not bypass the Pass112 source-disjoint gate.
