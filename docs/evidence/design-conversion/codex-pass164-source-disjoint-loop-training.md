# Codex Pass164 - Source-disjoint loop-created DPO training proof

claim_scope: Pass164 focused local proof that loop-created/runtime DesignConversion DPO candidates from two distinct source manifests/source structures can pass source-manifest/source-disjoint training-intake preflight and reach the training runner boundary, while a single-source runtime candidate remains blocked. No full product readiness, cloud upload, deployment, external provider call, commit, branch, or Pass165 learned-improver correction-choice is claimed.
evidence_scope: Current-turn reads of project governance markers, required skill gate sections, Pass155/157/162/163 evidence, Claude DesignToCode evidence searches, current DesignConversion local artifact DB/DPO/preflight/runtime loop code, one focused test addition in `Tests/DesignConversionModelServerImprovementLoopTests.cs`, focused dotnet test output, and test-file `git diff --check`.
count_claimed: 1
independent_evidence_count: 2
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: no for the bounded Pass164 source-disjoint local preflight/training-runner boundary after the focused test; yes for full product readiness, cloud/deploy, production DB readiness, external provider readiness, browser-backed rendering, Pass165 learned-improver correction-choice, and real local promotion from this two-record run.
pass164_verdict: PASS

## skill_load_status

- SK-SESSION: found/read_task_relevant_sections: `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, `docs/skills/planning-session-startup-SKILL.md`.
- SK-INTENT: found/read_task_relevant_sections: `docs/skills/architect-behavior-classifier-SKILL.md`.
- SK-PLAN: found/read_task_relevant_sections: `docs/skills/authority-requirement-binding-SKILL.md`, `docs/skills/reference-inventory-and-source-map-SKILL.md`, `docs/skills/phase-planning-SKILL.md`, `docs/skills/codex-planning-quality-GUIDE.md`.
- SK-REVIEW: found/read_task_relevant_sections: `docs/skills/plan-review-SKILL.md`, `docs/skills/goal-delivery-completeness-SKILL.md`, `docs/skills/claim-verification-SKILL.md`, `docs/skills/audit-protocol-SKILL.md`, `docs/skills/specificity-calibration-SKILL.md`, `docs/skills/output-skepticism-SKILL.md`, `docs/skills/reconnaissance-gate-SKILL.md`.
- SK-ARCH: found/read_task_relevant_sections: `docs/skills/trainable-architecture-decision-SKILL.md`, `docs/skills/no-product-decisions-SKILL.md`, `docs/skills/output-readability-gate-SKILL.md`.
- SK-QA/CODE: found/read_task_relevant_sections: `docs/skills/qa-session-type-SKILL.md`, `docs/skills/generated-code-review-SKILL.md`, `docs/skills/test-integrity-SKILL.md`, `docs/skills/e2e-test-matrix-SKILL.md`, `docs/skills/implementation-integrity-SKILL.md`, `docs/skills/verification-before-completion-SKILL.md`, `docs/skills/bug-to-tests-SKILL.md`, `docs/skills/test-failure-triage-SKILL.md`, `docs/skills/self-verification-SKILL.md`, `docs/skills/agent-output-format-SKILL.md`.

## files_read

- `AGENTS.md`; `docs/SESSION-START-PROMPT.md`; required skill files listed above.
- `docs/evidence/design-conversion/codex-pass155-dpo-approval-training-intake-boundary.md`.
- `docs/evidence/design-conversion/codex-pass157-retrain-promotion-boundary.md`.
- `docs/evidence/design-conversion/codex-pass162-product-gap-audit.md`.
- `docs/evidence/design-conversion/codex-pass163-same-scenario-local-chain.md`.
- Existing Pass146-Pass161 evidence was searched for DPO/usefulness/training/source-disjoint/promotion boundaries.
- Read-only Claude evidence searched: `claude-SCAFFOLD-TO-TRAINED-FEATURE-PLAN-2026-06-29.md`, `claude-FIX-GUIDE-scaffold-to-implemented-v1.md`, `claude-AUTONOMY-EXTENSIONS-skills-guides-agents-v1.md`.
- Current implementation/test files read: `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs`, `44.DesignConversionDomain/DesignConversionDpoGenerator.cs`, `44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs`, `Tests/DesignConversionDpoStorageTests.cs`, `Tests/DesignConversionModelServerImprovementLoopTests.cs`.

## files_changed

- `Tests/DesignConversionModelServerImprovementLoopTests.cs`: added `Pass164_SourceDisjointLoopCreatedRuntimeCandidatesPassTrainingIntakeBoundary`, `CreatePass164Package`, and `AssertRuntimeQualityCreatedDpoPair`.
- `docs/evidence/design-conversion/codex-pass164-source-disjoint-loop-training.md`: this evidence file.
- No production code was changed; the existing source-manifest/source-disjoint gate was not weakened.

## commands_run

- `rg -n ... C:\Users\lubao\.codex\memories\MEMORY.md` for relevant prior governance memory.
- `rg --files -g "AGENTS.md" -g "*SKILL.md" -g "*GUIDE.md" -g "SESSION-START-PROMPT.md"` to locate the required local stack.
- `rg`/`Select-String`/`Get-Content` focused reads for skills, evidence, current code, and current tests.
- `dotnet test Tests\Tests.csproj --filter "FullyQualifiedName~DesignConversionModelServerImprovementLoopTests.Pass164_SourceDisjointLoopCreatedRuntimeCandidatesPassTrainingIntakeBoundary" --logger "console;verbosity=minimal"` failed before build because `Tests\Tests.csproj` does not exist; this is not proof.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionModelServerImprovementLoopTests.Pass164_SourceDisjointLoopCreatedRuntimeCandidatesPassTrainingIntakeBoundary" --logger "console;verbosity=minimal"` passed before quality-signal tightening: Failed 0, Passed 1, Total 1.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionModelServerImprovementLoopTests.Pass164_SourceDisjointLoopCreatedRuntimeCandidatesPassTrainingIntakeBoundary" --logger "console;verbosity=minimal"` passed after quality-signal tightening: Failed 0, Passed 1, Total 1. Existing unrelated warnings were emitted from other tests.
- `git diff --check -- Tests\DesignConversionModelServerImprovementLoopTests.cs` returned exit code 0 with no output.

## source_disjoint_trace

- Negative control: first loop-created runtime package `pass164-source-a-request` used source manifest `sha256:pass164-source-a-design-tree`. Its runtime candidate started review-pending and unapproved, then was explicitly approved through `ApproveRuntimeCreatedDpoCandidateAsync`.
- Single-source intake from only that approved runtime candidate stayed blocked: `MayStartTraining=false`, `SourceManifestStatus=proven`, `SourceDisjointStatus=blocked_same_source`, blocker `source_disjoint_blocked_same_source`, `source_manifest_candidate_count=1`, `source_disjoint_train_source_count=0`, `source_disjoint_heldout_source_count=1`, `model_upload_ready=false`, `product_ready=false`.
- Source-disjoint pass scenario: second loop-created runtime package `pass164-source-b-request` used source manifest `sha256:pass164-source-b-design-tree`; it was generated through the same local improvement-loop/runtime artifact DB path and explicitly approved.
- Handoff after both approvals had `ApprovedCandidateCount=2`, two distinct `SourceManifestRef` values, and two distinct `SourceStructureHash` values. The test asserts disjointness is not faked by changing only request ids.
- Intake after both approvals had two distinct `SourceIdentityHash` values. Preflight returned `MayStartTraining=true`, `SourceManifestStatus=proven`, `SourceDisjointStatus=proven`, no block reasons, `source_manifest_candidate_count=2`, `source_manifest_hash_count=2`, `source_disjoint_train_source_count=1`, `source_disjoint_heldout_source_count=1`, `source_disjoint_overlap_count=0`, `training_started=false`, `weights_produced=false`, `model_upload_ready=false`, `product_ready=false`.
- Training run boundary after source-disjoint preflight invoked the runner with `ResolvedTrainingRecordCount=2`, `TrainingRunnerInvoked=true`, `SourceManifestStatus=proven`, `SourceDisjointStatus=proven`. The runner then stopped at the existing dataset-count boundary: `TrainingExecutionStatus=runner_invoked_training_blocked_dataset_count_gate`, `RunnerBlockedReason=dataset_count_gate_not_passed`. This proves the intake/preflight boundary without hiding count-gate or product-readiness limits.
- Quality-pair assertions: each runtime DPO record has chosen accepted, rejected not accepted, distinct chosen/rejected candidate ids, chosen visual score greater than rejected visual score, positive usefulness, runtime-created/review-required/approval-required metadata, and no quarantine.

## verification_results

- Focused Pass164 test passed after final assertions: `Failed: 0, Passed: 1, Skipped: 0, Total: 1`.
- Same-source/single-source blocker remains active inside the new focused test through `source_disjoint_blocked_same_source`.
- Source-disjoint loop-created/runtime candidate path reaches training runner boundary without product readiness: `product_ready=false`, `model_upload_ready=false`, `promotion_upload_attempted=false`.
- No production files were modified.
- Test-file whitespace check passed with no output: `git diff --check -- Tests\DesignConversionModelServerImprovementLoopTests.cs`.

## mental_simulation_result

The Pass164 path starts with local ModelServer improvement-loop packages, not synthetic hand-built DPO rows. Each package produces a runtime-created DPO candidate through `DesignConversionImprovementLoopCoordinator` and `DesignConversionDpoGenerator.BuildRuntimeCreatedTrainingCandidates`, persists it in the local JSONL artifact DB, and exposes it as review-pending/training-candidate-only/product-not-ready. The first approved candidate alone cannot create a train/held-out split and is blocked by the existing same-source gate. Adding the second approved runtime candidate from a different source manifest and source structure gives two source identity hashes; the preflight partitions one source into held-out and one into train with zero overlap, then allows runner invocation. The runner is still blocked by the dataset count gate, so the proof stops at the training intake/preflight/runner boundary and does not claim local promotion, cloud upload, deployment, or product readiness.

## remaining_gaps_after_pass164

- The default two-record Pass164 run reaches the runner but does not pass the dataset count gate; enough source-disjoint approved records or an explicit count-gate test hook would be needed for a real local retrain/promotion claim.
- Pass165 learned-improver correction-choice is not implemented or claimed.
- Pass166 browser-backed rendering is not implemented or claimed.
- Cloud upload, deployment, production DB readiness, and full product readiness remain not proven.

next_recommended_pass: Pass165 learned-improver correction-choice, or a separate count-gate/local-promotion pass if Luba wants the source-disjoint loop-created path to proceed beyond runner invocation.
