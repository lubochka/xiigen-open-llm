# Codex Pass107 - Approved DPO Handoff Export

claim_scope: Implemented a narrow local export/handoff artifact for approved runtime-created DesignConversion DPO training candidates.
evidence_scope: Code changes, focused tests, readbacks, and hygiene checks in the local DesignConversion domain only.
count_claimed: 1 bounded implementation pass
independent_evidence_count: 1 focused dotnet test run passed
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: no for the narrow handoff export; yes for training, weights, model upload, public API, production DB, mergeability, and full product readiness.

## Summary

Pass107 adds an in-memory/domain handoff export for locally approved runtime-created DPO candidates. The export is handoff-only. It does not run training, mutate checkpoints, produce weights, upload a model, call an external provider, add a public host/API, use a production DB, or claim product readiness.

The export includes only approved local runtime candidates that satisfy all of these criteria:

- `runtime_created_dpo_candidate=true`
- `approval_status=approved`
- `approved_for_training=true`
- approved reviewer, reason, and timestamp are present
- training-candidate-only and product-not-ready boundaries remain true
- model/trained/public/production readiness claims remain false

Pending, unapproved, quarantined, non-runtime, missing-reviewer, and missing-reason rows are excluded from the handoff.

## Files Written

- `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs`
- `44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs`
- `Tests/DesignConversionThinClientJobRuntimeTests.cs`
- `Tests/DesignConversionDpoStorageTests.cs`
- `docs/evidence/design-conversion/codex-pass107-approved-dpo-handoff-export.md`

Current path-scoped status also includes cumulative dirty DesignConversion files from prior passes, if present in the worktree. This pass did not edit `claude-*`, `C:\Users\lubao\Documents\xiigen`, git internals, or `grep.exe.stackdump`.

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
- `docs/evidence/design-conversion/codex-pass106-local-dpo-candidate-approval-transition.md`
- `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs`
- `44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs`
- `44.DesignConversionDomain/DesignConversionDpoGenerator.cs`
- `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs`
- `Tests/DesignConversionThinClientJobRuntimeTests.cs`
- `Tests/DesignConversionDpoUsefulnessApiTests.cs`
- `Tests/DesignConversionDpoStorageTests.cs`
- `Tests/DesignConversionLocalFullLoopE2eTests.cs`

## Skill Load Status

Every named required skill/context file was found/read by direct repo file readback. Optional `output-readability-gate-SKILL.md`, `implementation-integrity-SKILL.md`, and `verification-before-completion-SKILL.md` were present and read, so there is no optional-file impact.

## Implementation Summary

- Added `DesignConversionApprovedDpoHandoffRow` and `DesignConversionApprovedDpoHandoffArtifact`.
- Added `IDesignConversionLocalArtifactDb.ExportApprovedRuntimeDpoCandidatesAsync(...)`.
- Added `DesignConversionDpoUsefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(...)`.
- Export rows include stable artifact id/ref, candidate ids, training record ids, usefulness score, approval metadata, source manifest ref, payload hashes, evidence refs, and boundary metadata.
- Export rows contain hashes/refs/metadata only. They do not include raw HTML, raw CSS, raw design tree payloads, local private paths, prompt text, user-private identifiers, or provider payloads.
- Export warnings and boundary metadata explicitly include `training_handoff_only`, `not_trained`, `weights_not_produced`, `model_upload_not_ready`, `held_out_not_proven`, and `source_disjoint_not_proven`.
- Empty export for pending-only candidates returns zero rows and warning `no_approved_candidates`.

## Tests Run

Command:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionThinClientJobRuntimeTests|FullyQualifiedName~DesignConversionDpoUsefulnessApiTests|FullyQualifiedName~DesignConversionDpoStorageTests|FullyQualifiedName~DesignConversionLocalFullLoopE2eTests" --no-restore --logger "console;verbosity=minimal"
```

Result:

```text
Passed! - Failed: 0, Passed: 18, Skipped: 0, Total: 18
```

## Changed Code Readback

Readback command:

```powershell
rg -n "ExportApprovedRuntimeDpoCandidatesAsync|DesignConversionApprovedDpoHandoff|ApprovedDpoHandoffSchemaVersion|IsApprovedRuntimeHandoffCandidate|BuildHandoffRow|BuildHandoffWarnings|BuildHandoffBoundaryMetadata|training_handoff_only|not_trained|weights_not_produced|model_upload_not_ready" .\44.DesignConversionDomain\DesignConversionLocalArtifactDb.cs .\44.DesignConversionDomain\DesignConversionDpoUsefulnessApi.cs .\Tests\DesignConversionThinClientJobRuntimeTests.cs .\Tests\DesignConversionDpoStorageTests.cs .\docs\evidence\design-conversion\codex-pass107-approved-dpo-handoff-export.md
```

Readback found:

- `DesignConversionApprovedDpoHandoffRow` and `DesignConversionApprovedDpoHandoffArtifact`.
- `IDesignConversionLocalArtifactDb.ExportApprovedRuntimeDpoCandidatesAsync(...)`.
- `DesignConversionDpoUsefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(...)`.
- `IsApprovedRuntimeHandoffCandidate(...)`, `BuildHandoffRow(...)`, `BuildHandoffWarnings(...)`, and `BuildHandoffBoundaryMetadata(...)`.
- Runtime-flow test coverage for pending-empty export and approved sanitized export.
- Storage-filter test coverage for pending, non-runtime, quarantined, missing-reviewer, and missing-reason exclusions.
- This evidence file's `training_handoff_only`, `not_trained`, `weights_not_produced`, and `model_upload_not_ready` boundaries.

## Verification Results

- Focused dotnet test command: passed, 18 tests.
- Line-anchored conflict marker scan over changed Pass107 files: no matches.
- Mojibake marker scan over changed Pass107 files using the configured corruption-marker set: no matches.
- `git diff --check` over tracked allowed modified files: no output.
- Path-scoped status over allowed files listed only allowed DesignConversion domain/test/evidence paths.

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
?? docs/evidence/design-conversion/codex-pass107-approved-dpo-handoff-export.md
```

Pass107 edited `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs`, `44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs`, `Tests/DesignConversionThinClientJobRuntimeTests.cs`, `Tests/DesignConversionDpoStorageTests.cs`, and this evidence file. The currently dirty/visible allowed files `44.DesignConversionDomain/DesignConversionDpoGenerator.cs`, `44.DesignConversionDomain/DesignConversionThinClientJobRuntime.cs`, `Tests/DesignConversionDpoUsefulnessApiTests.cs`, and `Tests/DesignConversionLocalFullLoopE2eTests.cs` were read as context but not edited by Pass107.

## Mental Simulation Result

Runtime creates a local DPO candidate and persists it as review-pending. Export before approval returns zero rows with `no_approved_candidates`; no training/upload/model readiness is claimed. A local reviewer approves the runtime-created candidate with reviewer ref, reason, and timestamp. Export then returns exactly that approved candidate as a sanitized handoff row with hashes/refs/metadata and boundary warnings. A future trainer can consume the ids and hashes later, but this artifact does not train, upload, or prove model/product readiness.

Invalid export cases were simulated by tests:

- pending runtime candidate: excluded
- non-runtime train-ready-looking candidate: excluded
- approved runtime candidate missing reviewer: excluded
- approved runtime candidate missing reason: excluded
- quarantined runtime candidate: excluded

## Closed Claim

The narrow local approved-runtime-DPO-candidate handoff export is implemented and covered by focused tests.

## Not Closed Claims

- No real model training implemented.
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
