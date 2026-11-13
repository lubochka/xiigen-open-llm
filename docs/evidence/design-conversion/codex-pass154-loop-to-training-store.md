# Pass154 loop-to-training-store evidence

claim_scope: Pass154 bounded product slice only: iterative QA/improvement loop result to local artifact DB autowrite and DPO-source readback.
evidence_scope: Current code inspection, implementation in the loop coordinator, focused tests, storage/thin-client regression probes, and readback/marker scans in this worktree.
count_claimed: 1 bounded Pass154 loop-to-training-store pass.
independent_evidence_count: 3 passing focused test commands plus this evidence readback/scan packet. One earlier target-test run failed on an over-specific hash expectation and was repaired before acceptance.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
requested_model: gpt-5.5
requested_reasoning_effort: xhigh

## skill_load_status

- C:\Projects\llm_mvp_core\AGENTS.md: found/read.
- C:\Projects\llm_mvp_core\docs\SESSION-START-PROMPT.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\architect-behavior-classifier-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\trainable-architecture-decision-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\no-product-decisions-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\test-integrity-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\implementation-integrity-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\verification-before-completion-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\bug-to-tests-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\self-verification-SKILL.md: found/read.
- C:\Projects\llm_mvp_core\docs\skills\agent-output-format-SKILL.md: found/read.
- FULL_ACCESS_NO_PERMISSION_PROMPT_GATE: active; no approval prompt requested; no sandbox_permissions used.

## active_task_sentence_source

Luba current unquoted message: "Pass154 product slice: close the remaining Pass153/T4 note: prove or add explicit improvement-loop-to-local-artifact-DB autowrite so the chain `iterative QA/improvement loop -> local training store -> DPO source` is locally wired."

## pre_fix_diagnosis

- Existing Pass153/current code proved thin-client single completed-evaluation autowrite through `DesignConversionThinClientJobRuntime.PersistCompletedEvaluationAsync`.
- Current `DesignConversionImprovementLoopCoordinator` had no `IDesignConversionLocalArtifactDb` dependency and no append path for `DesignConversionImprovementLoopResult`.
- Existing `JsonlDesignConversionLocalArtifactDb` and `DesignConversionDpoGenerator.BuildRuntimeCreatedTrainingCandidates` already supported sanitized runtime-created DPO candidates, review-pending/product-not-ready metadata, and raw-payload exclusion.
- Diagnosis: loop-to-DB was missing/partial, not already real.

## files_changed

- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionImprovementLoopCoordinator.cs
- C:\Projects\llm_mvp_core\Tests\DesignConversionImprovementLoopCoordinatorTests.cs
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass154-loop-to-training-store.md
- Worktree note: the two code/test files are currently untracked according to `git status --short`; plain `git diff` has no tracked diff for them.

## implementation_summary

- Added optional `IDesignConversionLocalArtifactDb` and clock dependencies to `DesignConversionImprovementLoopCoordinator`.
- Added terminal-result `CompleteAsync` path. If a local artifact DB is configured and a final evaluation exists, it builds runtime-created DPO candidates from the final loop evaluation and appends a `DesignConversionLocalArtifactDbRecord`.
- Added loop-specific metadata/evidence markers: `improvement_loop_local_artifact_db_autowrite`, `improvement_loop_local_training_store_dpo_source`, final/current/original request refs, final job/candidate refs, attempt count, review-pending/product-not-ready/training-candidate-only boundaries, and raw-payload false flags.
- The record is built through `DesignConversionLocalArtifactDbRecord.FromEvaluation` and normalized by `JsonlDesignConversionLocalArtifactDb`, so HTML/CSS/design tree/screenshot/provider payloads stay refs/hashes/metadata only.

## commands_run

- `rg -n "Pass15[234]|DesignConversion|local artifact|training store|JsonlDesignConversionLocalArtifactDb" C:\Users\lubao\.codex\memories\MEMORY.md`
- `git branch --show-current`
- `git status --short`
- Multiple focused `Get-Content`/`rg` inspections of coordinator, local artifact DB, thin-client runtime, DPO generator, DPO storage tests, and improvement-loop tests.
- `dotnet test --filter "FullyQualifiedName~DesignConversionImprovementLoopCoordinatorTests.RunAsync_WithLocalArtifactDbPersistsFinalLoopCandidateForDpoSourceWithoutRawPrivatePayload"`: first run failed on exact hash expectation; this was repaired.
- `dotnet test --filter "FullyQualifiedName~DesignConversionImprovementLoopCoordinatorTests.RunAsync_WithLocalArtifactDbPersistsFinalLoopCandidateForDpoSourceWithoutRawPrivatePayload"`: passed after repair.
- `dotnet test --filter "FullyQualifiedName~DesignConversionImprovementLoopCoordinatorTests"`: passed, 10 tests.
- `dotnet test --filter "FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_RoundTripsConversionRenderJudgeAndTrainingCandidateWithoutPrivateFields|FullyQualifiedName~DesignConversionThinClientJobRuntimeTests.RunAsync_WithConfiguredLocalArtifactDbPersistsSanitizedCompletedEvaluation"`: passed, 2 tests.
- `git status --short -- 44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs Tests/DesignConversionImprovementLoopCoordinatorTests.cs docs/evidence/design-conversion/codex-pass154-loop-to-training-store.md`
- `rg -n "LocalArtifactDbAutowriteMarker|CompleteAsync|BuildLocalArtifactDbMetadata|RunAsync_WithLocalArtifactDbPersistsFinalLoopCandidate" 44.DesignConversionDomain\DesignConversionImprovementLoopCoordinator.cs Tests\DesignConversionImprovementLoopCoordinatorTests.cs`
- Evidence-file readback with `Get-Content`.
- Exact identity-line count check: returned `1`.
- Conflict-marker scan on touched files: no matches.
- Mojibake-marker scan on touched files: no matches.
- Trailing-whitespace scan on touched files: no matches.
- `git diff --check -- 44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs Tests/DesignConversionImprovementLoopCoordinatorTests.cs docs/evidence/design-conversion/codex-pass154-loop-to-training-store.md`: clean. Note: code/test/evidence files are untracked, so this does not create a tracked patch claim.
- `git status --short -- ':(glob)claude-*'`: no output.

## tests_run

- New focused proof: `DesignConversionImprovementLoopCoordinatorTests.RunAsync_WithLocalArtifactDbPersistsFinalLoopCandidateForDpoSourceWithoutRawPrivatePayload`: passed after repair.
- Coordinator regression class: `DesignConversionImprovementLoopCoordinatorTests`: passed, 10 tests.
- Reused storage/runtime probes: `DesignConversionDpoStorageTests.LocalArtifactDb_RoundTripsConversionRenderJudgeAndTrainingCandidateWithoutPrivateFields` and `DesignConversionThinClientJobRuntimeTests.RunAsync_WithConfiguredLocalArtifactDbPersistsSanitizedCompletedEvaluation`: passed, 2 tests.

## loop_to_training_store_proof

- The new test runs the real `DesignConversionImprovementLoopCoordinator` through three attempts: original -> candidate-002 -> candidate-003.
- The configured `JsonlDesignConversionLocalArtifactDb` receives exactly one appended record.
- Reloading a fresh DB instance returns the stored record.
- Stored record proof:
  - `JobId` is `pass128-job:loop-attempt:2:loop-attempt:3`.
  - `RequestId` is `candidate-003-request`.
  - `final_candidate_from_loop` is `True`.
  - `attempt_count` is `3`.
  - conversion HTML/CSS refs are `dcm-html_result-*` and `dcm-css_result-*`.
  - conversion HTML/CSS hashes are derived from the final evaluation output and differ from the original request hashes.

## dpo_source_and_raw_boundary_proof

- The stored record contains one runtime-created DPO candidate.
- Candidate metadata remains review-pending/product-not-ready:
  - `runtime_created_dpo_candidate=True`
  - `review_status=runtime-candidate-review-pending`
  - `approved_for_training=False`
  - `product_not_ready=True`
- `QueryApprovedTrainingCandidatesAsync` returns empty, so the proof does not claim train-ready or production-DPO readiness.
- The persisted JSONL is checked for absence of original/private loop markers, first and final raw HTML/CSS strings, `C:/Users/lubao`, and `data:image`.
- The test asserts chosen/rejected DPO payloads are stored as `dcm-chosen_html-*` and `dcm-rejected_html-*` refs, not raw HTML.

## verification_results

- Focused Pass154 test: passed.
- Existing coordinator behavior: passed.
- Existing local artifact DB and thin-client persistence proofs: passed.
- Evidence-file readback: passed.
- Exact identity-line count: passed, count was 1.
- Conflict-marker scan: passed, no matches.
- Mojibake-marker scan: passed, no matches.
- Trailing-whitespace scan: passed, no matches.
- `git diff --check` on the path set: clean, with untracked-file caveat above.
- `claude-*` status check: no output for the root glob.
- No external provider/API calls were made.
- No git commit, push, branch, reset, checkout, clean, or PR action was run.
- Production DB deployment was not tested or claimed.

## mergeability_status_or_not_checked

Not checked as full mergeability. Focused compile/test commands passed for the touched area. Worktree has pre-existing unrelated modified/untracked files; this packet does not claim repository-wide clean state.

## blockers

None for this bounded Pass154 loop-to-local-artifact-DB slice.

## downgrade_needed

No downgrade for the bounded Pass154 claim after passing tests and readback. Downgrade remains for any broader claim: no full product readiness, production DB deployment, external provider path, or approved train-ready DPO handoff is claimed.

## next_recommended_pass

If Luba continues, run the next bounded pass on an approval/readback API route from loop-created local candidates to the existing DPO usefulness/training-intake UI/API, without claiming train-ready until explicit reviewer approval and measurement gates pass.
