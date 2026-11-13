# Pass155 DPO Approval And Training-Intake Boundary

claim_scope: One bounded Pass155 local product slice for loop-created DesignConversion DPO candidates: usefulness/readback visibility, unapproved training exclusion, explicit approval handoff, and local training/upload readiness gates. No external upload, deployment, full product readiness, git publish action, or external provider call is claimed.
evidence_scope: Current code and focused tests in `C:\Projects\llm_mvp_core`; Pass145/147/153/154 were context only. Production change was limited to usefulness API row boundary metadata no-raw/no-private flags. Test proof was added to the existing Pass154 loop-created candidate test.
count_claimed: 1 bounded Pass155 proof packet; 0 sub-agent passes; 0 external uploads; 0 full-product-readiness claims.
independent_evidence_count: 4 current focused passing test command records plus direct source/test readback.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
requested_model: gpt-5.5
requested_reasoning_effort: xhigh

skill_load_status:
- `AGENTS.md`: found/read by whole-file scan; lines=6913; sha12=1783F2465B18; active gates included FULL_ACCESS_NO_PERMISSION_PROMPT_GATE, no dishonest claims, approval/training verification markers.
- `docs/SESSION-START-PROMPT.md`: found/read by whole-file scan; lines=621; sha12=450961E74C8E.
- `docs/skills/planning-session-startup-SKILL.md`: found/read by whole-file scan; lines=2041; sha12=9889656CCE52.
- `docs/skills/architect-behavior-classifier-SKILL.md`: found/read by whole-file scan; lines=1469; sha12=02FD255413A2.
- `docs/skills/plan-review-SKILL.md`: found/read by whole-file scan; lines=3747; sha12=AF9764553AB1.
- `docs/skills/claim-verification-SKILL.md`: found/read by whole-file scan; lines=766; sha12=B230CB92DFD3.
- `docs/skills/trainable-architecture-decision-SKILL.md`: found/read by whole-file scan; lines=738; sha12=3A751518E20A.
- `docs/skills/no-product-decisions-SKILL.md`: found/read by whole-file scan; lines=159; sha12=7009F0B51642.
- `docs/skills/qa-session-type-SKILL.md`: found/read by whole-file scan; lines=231; sha12=114DA97E4FC8.
- `docs/skills/generated-code-review-SKILL.md`: found/read by whole-file scan; lines=643; sha12=89A5587A76B1.
- `docs/skills/test-integrity-SKILL.md`: found/read by whole-file scan; lines=776; sha12=FD545554BC74.
- `docs/skills/implementation-integrity-SKILL.md`: found/read by whole-file scan; lines=794; sha12=18C33DD8C4D7.
- `docs/skills/verification-before-completion-SKILL.md`: found/read by whole-file scan; lines=696; sha12=BDEC446CFC62.
- `docs/skills/bug-to-tests-SKILL.md`: found/read by whole-file scan; lines=202; sha12=491C82ABF212.
- `docs/skills/self-verification-SKILL.md`: found/read by whole-file scan; lines=192; sha12=1A14F71DD7D6.
- `docs/skills/agent-output-format-SKILL.md`: found/read by whole-file scan; lines=498; sha12=22D9E15316D7.

active_task_sentence_source:
- Current unquoted Luba message beginning `CURRENT TASK: Pass155 product slice...`.
- Current unquoted constraints beginning `NOT THE TASK`, `allowed_files`, `forbidden_files`, and `verification_required`.

pre_fix_diagnosis:
- Existing production code already had local artifact DB append/list, `QueryApprovedTrainingCandidatesAsync`, runtime-created approval, approved handoff export, training intake creation, preflight, and training run gate paths.
- Existing production code already excluded review-pending loop-created candidates from approved training query/export.
- Gap found while adding proof: user-facing usefulness rows did not include row-level `raw_html_included=false`, `raw_css_included=false`, `raw_design_tree_included=false`, `raw_user_private_data_included=false`, and `provider_payload_included=false`. The response envelope had no-raw flags, but each row needed explicit row metadata for the readback boundary.
- Regression caught and repaired: a temporary prompt-literal row key violated existing no-literal leakage assertions because the key itself contained a forbidden marker. It was removed.

files_changed:
- `44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs`: added row-level no-raw/no-private boundary metadata flags to DPO usefulness API rows.
- `Tests/DesignConversionImprovementLoopCoordinatorTests.cs`: extended the Pass154 loop-created candidate test to prove Pass155 usefulness/readback, approval, training-intake, and gate behavior.
- `docs/evidence/design-conversion/codex-pass155-dpo-approval-training-intake-boundary.md`: this evidence packet.

commands_run:
- `rg` memory/context and repo discovery commands for DPO storage/usefulness/training terms.
- Whole-file required skill load scan using `Get-Content -Raw` with line counts and SHA prefixes.
- Targeted source/test readbacks for `DesignConversionLocalArtifactDb`, `DesignConversionDpoUsefulnessApi`, `DesignConversionDpoStorageTests`, `DesignConversionImprovementLoopCoordinatorTests`, and training measurement gate code.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionImprovementLoopCoordinatorTests.RunAsync_WithLocalArtifactDbPersistsFinalLoopCandidateForDpoSourceWithoutRawPrivatePayload" --no-restore`
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAsync_StartsLocalLifecycleWithRealWeightsWithoutUploadClaims|FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAsync_BlocksUploadWhenMeasurementGateFailsAfterRealWeights|FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAsync_AllowsLocalUploadReadinessOnlyAfterMeasurementAndPromotionGatePass" --no-restore`
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionDpoUsefulnessApiTests|FullyQualifiedName~DesignConversionModelServerDpoUsefulnessTests" --no-restore`

tests_run:
- Pass: `DesignConversionImprovementLoopCoordinatorTests.RunAsync_WithLocalArtifactDbPersistsFinalLoopCandidateForDpoSourceWithoutRawPrivatePayload` after latest repair. Result: Passed 1, Failed 0.
- Pass: `DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAsync_StartsLocalLifecycleWithRealWeightsWithoutUploadClaims`.
- Pass: `DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAsync_BlocksUploadWhenMeasurementGateFailsAfterRealWeights`.
- Pass: `DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAsync_AllowsLocalUploadReadinessOnlyAfterMeasurementAndPromotionGatePass`.
- Pass: `DesignConversionDpoUsefulnessApiTests` and `DesignConversionModelServerDpoUsefulnessTests`. Result: Passed 6, Failed 0.

usefulness_readback_proof:
- The loop-created candidate is visible before approval through `DesignConversionDpoUsefulnessApi.QueryAsync` with one row, `ReviewStatus=runtime-candidate-review-pending`, `ApprovalStatus=pending`, `ReviewRequired=true`, `TrainReady=false`, `ProductNotReady=true`, `ProductionDpo=false`, and `ModelReadinessClaimed=false`.
- The same loop-created local artifact is visible through `QueryLocalArtifactReadbackAsync` with one DPO candidate, review pending, approval pending, training-candidate-only, product-not-ready, and no model readiness claim.
- Usefulness row metadata now explicitly includes false no-raw/no-private row flags for HTML, CSS, design tree, user private data, and provider payload.

approval_training_gate_proof:
- Before approval, `QueryApprovedTrainingCandidatesAsync` returned empty and approved handoff export had `ApprovedCandidateCount=0`.
- After explicit local approval with reviewer and reason, `QueryApprovedTrainingCandidatesAsync` returned exactly the approved loop-created training record and approved handoff export returned one handoff-only row.
- Creating training intake from that approved handoff produced an intake record, but the single loop-created candidate remained blocked before training by `source_disjoint_blocked_same_source`; `RunTrainingIntakeAsync` did not invoke the runner and reported no training start, no checkpoint mutation, no weights, and no model upload readiness.
- Existing focused gate tests prove the broader approved-runtime path: real weights without measurement stay upload-blocked, failing measurement stays upload-blocked, and local upload readiness appears only after measurement plus promotion policy pass. This is local readiness only; no external upload was attempted or claimed.

raw_private_boundary_proof:
- The extended loop test checks persisted JSONL and serialized usefulness/readback/approval/handoff/intake/run envelopes do not contain the private HTML/CSS/tree markers, final improved raw HTML/CSS strings, a private Windows user-path marker, or a data-URI marker.
- DPO usefulness API and ModelServer route regression tests also passed after the row-level metadata patch, including existing no-raw-payload assertions.

verification_results:
- Focused tests listed above passed after the final patch.
- Direct source readback showed `DesignConversionDpoUsefulnessApi.ToApiRow` row metadata now includes no-raw/no-private false flags and does not include the forbidden prompt-literal marker.
- Direct test readback showed the loop proof covers pending visibility, unapproved exclusion, approval, approved handoff/intake, source-disjoint blocker, and raw/private absence.
- Exact identity-line count: anchored search for the required identity line returned `1`.
- Conflict-marker scan over touched files returned no matches.
- Mojibake marker scan over touched files returned no matches for the configured marker set.
- `git diff --check -- <touched files>` returned exit code 0 with no output. Because the edited source/test files are untracked in this dirty worktree, this is a hygiene check only, not a tracked mergeability proof.

mergeability_status_or_not_checked: Not checked as full mergeability. The worktree was already very dirty, and the two edited source/test files are untracked by git in this worktree, so `git diff` does not show their patch. No git commit, branch, push, checkout, reset, or clean was run.

blockers:
- No blocker for this bounded Pass155 local approval/readback/training-intake boundary after current tests.
- The loop-created single-candidate path remains correctly blocked from actual training start by source-disjoint preflight until a separate source-disjoint held-out/train split exists.
- External upload/deployment/full product readiness remains not proven and not claimed.

downgrade_needed: No for the bounded Pass155 local boundary after tests and readback; yes for any broader external upload, deployment, full product readiness, production DB, or multi-candidate source-disjoint loop-readiness claim.
next_recommended_pass: If continuing, add a separate source-disjoint loop-created multi-candidate/held-out proof before claiming loop-created candidates can start training beyond the single-candidate blocker.
