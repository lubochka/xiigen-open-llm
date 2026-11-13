# Codex Pass108 - Training Intake Boundary

claim_scope: Implemented a narrow local DesignConversion training-intake boundary that validates an approved sanitized DPO handoff artifact and records an intake request for future ILearningMachine training.
evidence_scope: Code changes, focused tests, direct readbacks, and hygiene checks in the local DesignConversion domain/API/tests/evidence files only.
count_claimed: 1 bounded implementation pass; 1 focused dotnet test run.
independent_evidence_count: 1 current focused dotnet test run passed.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: no for the narrow local intake boundary after verification; yes for actual training, checkpoint mutation, trained weights, model upload, public host/API, production DB, held-out proof, source-disjoint proof, mergeability, and full product readiness.

## Summary

Pass108 adds a local training-intake request boundary for Pass107 approved DesignConversion DPO handoff artifacts. It accepts only sanitized handoff artifacts with approved candidates, reviewer/reason metadata, and required handoff-only/not-trained boundary metadata. Accepted handoffs are recorded to a local JSONL intake queue for future ILearningMachine training.

This pass does not run training, mutate checkpoints, produce weights, upload a model, call an external provider, add a public host/API, use a production DB, or claim model/product readiness.

## Files Written

Pass108 touched these files:

- `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs`
- `44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs`
- `Tests/DesignConversionThinClientJobRuntimeTests.cs`
- `Tests/DesignConversionDpoStorageTests.cs`
- `docs/evidence/design-conversion/codex-pass108-training-intake-boundary.md`

Current path-scoped status also shows cumulative dirty allowed DesignConversion files from prior passes:

- `44.DesignConversionDomain/DesignConversionDpoGenerator.cs`
- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs`
- `Tests/DesignConversionDpoUsefulnessApiTests.cs`
- `Tests/DesignConversionLocalFullLoopE2eTests.cs`

Pass108 did not edit `claude-*`, anything under `C:\Users\lubao\Documents\xiigen`, git internals, or `grep.exe.stackdump`.

## Files Read

- `AGENTS.md`
- `docs/SESSION-START-PROMPT.md`
- `docs/skills/planning-session-startup-SKILL.md`
- `docs/skills/architect-behavior-classifier-SKILL.md`
- `docs/skills/authority-requirement-binding-SKILL.md`
- `docs/skills/reference-inventory-and-source-map-SKILL.md`
- `docs/skills/phase-planning-SKILL.md`
- `docs/skills/plan-review-SKILL.md`
- `docs/skills/goal-delivery-completeness-SKILL.md`
- `docs/skills/claim-verification-SKILL.md`
- `docs/skills/audit-protocol-SKILL.md`
- `docs/skills/specificity-calibration-SKILL.md`
- `docs/skills/output-skepticism-SKILL.md`
- `docs/skills/reconnaissance-gate-SKILL.md`
- `docs/skills/trainable-architecture-decision-SKILL.md`
- `docs/skills/no-product-decisions-SKILL.md`
- `docs/skills/output-readability-gate-SKILL.md`
- `docs/skills/qa-session-type-SKILL.md`
- `docs/skills/html-css-render-qa-automation-GUIDE.md`
- `docs/skills/generated-code-review-SKILL.md`
- `docs/skills/implementation-integrity-SKILL.md`
- `docs/skills/verification-before-completion-SKILL.md`
- `docs/evidence/design-conversion/codex-pass107-approved-dpo-handoff-export.md`
- `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs`
- `44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs`
- `44.DesignConversionDomain/DesignConversionDpoGenerator.cs`
- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs`
- `Tests/DesignConversionThinClientJobRuntimeTests.cs`
- `Tests/DesignConversionDpoUsefulnessApiTests.cs`
- `Tests/DesignConversionDpoStorageTests.cs`
- `Tests/DesignConversionLocalFullLoopE2eTests.cs`

## Skill Load Status

Every named required skill/context file was found and read by direct repo file read/open commands in this turn.

- SK-SESSION: `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, `docs/skills/planning-session-startup-SKILL.md` found/read.
- SK-INTENT: `docs/skills/architect-behavior-classifier-SKILL.md` found/read.
- SK-PLAN: `docs/skills/authority-requirement-binding-SKILL.md`, `docs/skills/reference-inventory-and-source-map-SKILL.md`, `docs/skills/phase-planning-SKILL.md` found/read.
- SK-REVIEW: `docs/skills/plan-review-SKILL.md`, `docs/skills/goal-delivery-completeness-SKILL.md`, `docs/skills/claim-verification-SKILL.md`, `docs/skills/audit-protocol-SKILL.md`, `docs/skills/specificity-calibration-SKILL.md`, `docs/skills/output-skepticism-SKILL.md`, `docs/skills/reconnaissance-gate-SKILL.md` found/read.
- SK-ARCH: `docs/skills/trainable-architecture-decision-SKILL.md`, `docs/skills/no-product-decisions-SKILL.md`, `docs/skills/output-readability-gate-SKILL.md` found/read.
- SK-QA/CODE: `docs/skills/qa-session-type-SKILL.md`, `docs/skills/html-css-render-qa-automation-GUIDE.md`, `docs/skills/generated-code-review-SKILL.md`, `docs/skills/implementation-integrity-SKILL.md`, `docs/skills/verification-before-completion-SKILL.md` found/read.
- Missing optional file impact: none; every named optional file was present.

## Implementation Summary

- Added `DesignConversionTrainingIntakeCandidateRef` and `DesignConversionTrainingIntakeRecord`.
- Added `IDesignConversionLocalArtifactDb.CreateTrainingIntakeAsync(...)` and `ListTrainingIntakeRequestsAsync(...)`.
- Added `JsonlDesignConversionLocalArtifactDb` validation and local JSONL queue persistence for accepted training-intake requests.
- Added local API delegation through `DesignConversionDpoUsefulnessApi.CreateTrainingIntakeAsync(...)` and `ListTrainingIntakeRequestsAsync(...)`.
- The intake record contains intake id/ref, source handoff artifact id/ref, approved and accepted candidate counts, candidate ids/hashes, requested timestamp, requester/reason, and boundary fields: `training_intake_only=true`, `training_not_started=true`, `not_trained=true`, `weights_not_produced=true`, `checkpoint_not_mutated=true`, `model_upload_not_ready=true`, `product_not_ready=true`, `held_out_not_proven=true`, and `source_disjoint_not_proven=true`.
- Empty approved handoff returns a zero-intake result with `no_approved_candidates` and does not enqueue a JSONL intake record.
- Missing required handoff boundary metadata, raw/private markers, missing reviewer/reason/timestamp, and missing candidate hashes are rejected.

## Tests Run

Command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionThinClientJobRuntimeTests|FullyQualifiedName~DesignConversionDpoUsefulnessApiTests|FullyQualifiedName~DesignConversionDpoStorageTests|FullyQualifiedName~DesignConversionLocalFullLoopE2eTests" --no-restore --logger "console;verbosity=minimal"
```

Result:

```text
Passed! - Failed: 0, Passed: 19, Skipped: 0, Total: 19
```

## Changed Code Readback

Readback command:

```powershell
rg -n "DesignConversionTrainingIntakeCandidateRef|DesignConversionTrainingIntakeRecord|CreateTrainingIntakeAsync|ListTrainingIntakeRequestsAsync|TrainingIntakeSchemaVersion|ValidateTrainingIntakeHandoff|BuildTrainingIntakeRecord|ContainsForbiddenTrainingIntakePayload|training_intake_only|training_not_started|checkpoint_not_mutated|model_upload_not_ready" 44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs 44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs Tests/DesignConversionThinClientJobRuntimeTests.cs Tests/DesignConversionDpoStorageTests.cs docs/evidence/design-conversion/codex-pass108-training-intake-boundary.md
```

Readback found:

- `DesignConversionTrainingIntakeCandidateRef` and `DesignConversionTrainingIntakeRecord`.
- `IDesignConversionLocalArtifactDb.CreateTrainingIntakeAsync(...)` and `ListTrainingIntakeRequestsAsync(...)`.
- `DesignConversionDpoUsefulnessApi.CreateTrainingIntakeAsync(...)` and `ListTrainingIntakeRequestsAsync(...)`.
- `TrainingIntakeSchemaVersion`, `ValidateTrainingIntakeHandoff(...)`, `BuildTrainingIntakeRecord(...)`, and `ContainsForbiddenTrainingIntakePayload(...)`.
- Runtime-flow test coverage for API intake after approval and handoff export.
- Storage test coverage for accepted intake, zero-intake, bad-boundary rejection, raw/private rejection, and missing-reviewer rejection.
- Evidence-file markers for `training_intake_only`, `training_not_started`, `checkpoint_not_mutated`, and `model_upload_not_ready`.

## Verification Results

- Focused dotnet test command: passed, 19 tests.
- Line-anchored conflict marker scan over changed Pass108 files: no matches.
- Mojibake marker scan over changed Pass108 files using the configured marker set from `verification_required`: no matches.
- `git diff --check` over tracked allowed modified files: no output.
- Path-scoped git status over allowed files listed only allowed DesignConversion domain/test/evidence files.

Current path-scoped status output:

```text
 M 44.DesignConversionDomain/DesignConversionDpoGenerator.cs
 M 44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs
 M Tests/DesignConversionDpoStorageTests.cs
 M Tests/DesignConversionThinClientJobRuntimeTests.cs
?? 44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs
?? 44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs
?? Tests/DesignConversionDpoUsefulnessApiTests.cs
?? Tests/DesignConversionLocalFullLoopE2eTests.cs
?? docs/evidence/design-conversion/codex-pass108-training-intake-boundary.md
```

## Mental Simulation Result

Runtime creates a local DPO candidate as review-pending. Before approval, Pass107 handoff export returns zero approved rows, and Pass108 intake returns zero accepted candidates without enqueueing an intake record or claiming readiness. A local reviewer approves the candidate with reviewer ref, reason, and timestamp. Pass107 export emits a sanitized approved handoff with hashes/refs only. Pass108 intake validates the handoff boundaries and records a local intake request for future ILearningMachine training. The recorded request preserves approval/usefulness traceability and states that training is not started, no checkpoint was mutated, no weights were produced, no model upload is ready, and product readiness is false.

Bad-boundary, raw/private, and missing-approval handoffs are rejected before an intake record is written.

## Closed Claim

The narrow local approved DesignConversion DPO handoff training-intake boundary is implemented and covered by focused tests.

## Not Closed Claims

- No real model training implemented or run.
- No trained weights produced.
- No checkpoint mutation implemented.
- No model upload implemented.
- No public host/API verified.
- No external visual provider call added.
- No production DB added.
- No held-out proof added.
- No source-disjoint proof added.
- No mergeability or git operation performed.
- No full product readiness claimed.
