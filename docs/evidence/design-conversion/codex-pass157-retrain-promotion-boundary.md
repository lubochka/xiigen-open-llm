# Codex Pass157 - T6/R11 retrain and local promotion boundary

claim_scope: One bounded Pass157 product slice for DesignConversion T6/R11 local retrain and local ModelServer promotion boundary.
evidence_scope: Current live-code inspection plus focused dotnet tests run in this turn; Pass144/150/153/155/156 and the repair guide were context only.
count_claimed: 1 bounded local product slice.
independent_evidence_count: 3 current test command outputs; this file is a summary artifact, not an extra test.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
requested_model: gpt-5.5
requested_reasoning_effort: xhigh
downgrade_needed: No for the bounded local retrain/local promotion boundary; yes for external cloud upload, production deployment, full product readiness, and full-suite status.

## skill_load_status

- AGENTS.md: found/read by direct Get-Content command; display was large but command completed.
- docs/SESSION-START-PROMPT.md: found/read by direct Get-Content command.
- docs/skills/planning-session-startup-SKILL.md: found/read by direct Get-Content command.
- architect-behavior-classifier-SKILL.md: found/read by direct Get-Content command.
- plan-review-SKILL.md: found/read by direct Get-Content command.
- claim-verification-SKILL.md: found/read by direct Get-Content command.
- trainable-architecture-decision-SKILL.md: found/read by direct Get-Content command.
- no-product-decisions-SKILL.md: found/read by direct Get-Content command.
- qa-session-type-SKILL.md: found/read by direct Get-Content command.
- generated-code-review-SKILL.md: found/read by direct Get-Content command.
- test-integrity-SKILL.md: found/read by direct Get-Content command.
- implementation-integrity-SKILL.md: found/read by direct Get-Content command.
- verification-before-completion-SKILL.md: found/read by direct Get-Content command.
- self-verification-SKILL.md: found/read by direct Get-Content command.
- agent-output-format-SKILL.md: found/read by direct Get-Content command.
- FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: active; no sandbox permissions, approval prompts, external provider calls, git publish, or destructive git commands were used.

## active_task_sentence_source

Latest unquoted Luba message in this thread: "Pass157 product slice: close T6/R11 from the repair guide: user/architect-requested retrain from approved DPO candidates plus local model promotion/upload-readiness boundary."

## pre_fix_diagnosis

- Repair guide T6/R11 context said retrain plus server upload/promotion was a missing scaffold, but current code already had several later local pieces.
- Current code already had approved-candidate handoff, training-intake preflight, source-disjoint gating, measurement gate handling, local checkpoint mutation evidence, local upload-readiness metadata, promoted-local model loading, and ModelServer promoted-local metadata.
- The gap was integrated: `RunTrainingIntakeAsync` ran a new in-process `DesignConversionMachine` and returned readiness evidence, but did not preserve a local promoted checkpoint/model object for ModelServer use.
- `44.DesignConversionDomain` still does not reference `25.ModelServer`; the implemented bridge keeps that dependency direction intact by returning a domain `DesignConversionPromotedLocalModel` which ModelServer can consume.

## files_changed

- `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs`: added `DesignConversionTrainingIntakeLocalPromotionResult`, `RunTrainingIntakeAndPromoteLocalAsync`, local-only promotion artifact writing, local-only promotion uploader, and boundary metadata for no external cloud upload.
- `44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs`: added API forwarder for `RunTrainingIntakeAndPromoteLocalAsync`.
- `Tests/DesignConversionDpoStorageTests.cs`: added Pass157 focused test and local ModelServer fixture helpers.
- `docs/evidence/design-conversion/codex-pass157-retrain-promotion-boundary.md`: this evidence packet.

## commands_run

- `rg` and `Get-Content` inspections for required skills, repair guide T6/R11, Pass144/150/153/155/156 evidence, training intake, training runner, training pipeline, promotion, promoted-local model, ModelServer API, and tests.
- `git status --short` read-only status checks.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionDpoStorageTests.Pass157_LocalArtifactDb_UserRequestedRetrainPromotesLocalModelServerCheckpointOnlyAfterGates"`
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAsync"`
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionModelLifecycleTests.Pass144_LocalLifecycle_TrainsPromotesLoadsAndModelServerUsesPromotedCheckpoint"`

## tests_run

- PASS: Pass157 focused test, 1 passed, 0 failed.
- PASS: nearby local-artifact-db training intake tests, 7 passed, 0 failed.
- PASS: Pass144 local lifecycle and ModelServer promoted checkpoint test, 1 passed, 0 failed.
- The test commands compiled the test project and dependencies. Existing unrelated nullable/analyzer warnings were emitted; no test failures.

## retrain_checkpoint_proof

- The Pass157 test creates two approved, source-disjoint runtime DPO candidates and packages them through `DesignConversionDpoUsefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync` plus `CreateTrainingIntakeAsync` with `requestedByRef = architect:pass157-local`.
- Failing measurement gate path proves training can produce local weights while promotion readiness stays blocked: `WeightsProduced = true`, `ModelUploadReady = false`, `LocalPromotionReady = false`, no publication, and no promoted local model.
- Passing measurement gate path proves the local chain: source manifest proven, source disjoint proven, training started, checkpoint mutated, checkpoint candidate created, real weights produced, measurement passed, promotion gate status `PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS`, and local upload-readiness status `upload_ready_local_promotion_gate_passed_no_external_upload`.
- The test saves a baseline checkpoint, then asserts the promoted checkpoint SHA differs from baseline, version is not `0.1.0`, and learned weight artifact/observation counts are greater than zero.

## promotion_gate_and_modelserver_proof

- The local promotion bridge writes local checkpoint/tokenizer/policy artifacts only after the existing measurement and promotion readiness checks pass.
- The bridge uses a local-only promotion ref with `local://design-conversion/promoted/` and records `external_cloud_upload_attempted = false` and `cloud_model_upload_attempted = false`.
- `DesignConversionPromotedLocalModelLoader.LoadAsync` reloads the promoted checkpoint and verifies fresh load plus real trained weights.
- The Pass157 test constructs `DesignConversionModelServerApi` with the promoted local model and verifies `LearnedInferenceApplied = true`, `ModelTrainingVerified = true`, `ModelUploadReady = false`, deployment false, product false, promoted model id/version in boundary metadata, and cloud upload attempted false.

## raw_private_boundary_proof

- The Pass157 test seeds raw/private HTML, CSS, local path markers, and exact prompt markers into the local artifact source record.
- The serialized training run is asserted not to contain `<main`, `.private-user`, `C:/Users/lubao`, `exact_prompt`, `private.png`, or `external-provider-payload`.
- ModelServer boundary metadata is asserted to report `raw_html_included = false`, `raw_css_included = false`, and `raw_user_private_data_included = false`, and is scanned for the same raw/private markers.

## verification_results

- Focused tests passed as listed above.
- External cloud upload was not attempted or claimed.
- No external AI/provider APIs were called.
- No git commit, push, branch, checkout, reset, clean, PR, or publish command was run.
- Full-suite status was not checked.

## mergeability_status_or_not_checked

Not checked. The repo already had a dirty worktree, including pre-existing untracked DesignConversion domain files; no mergeability or PR status is claimed.

## blockers

- External cloud upload/deployment remains not implemented and not claimed.
- Full product readiness remains not claimed.
- Full test suite remains not run.

## next_recommended_pass

If Luba wants to go beyond local readiness, run a separate pass for a real ModelServer/cloud promotion adapter contract with explicit external-call test doubles first, then only a real external upload if separately authorized and provider credentials/boundaries are available.
