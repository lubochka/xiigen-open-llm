# Pass229 Full Suite Rerun Evidence

pass229_verdict: NEEDS_FIX_FULL_SUITE_FAILED
full_test_command: dotnet test Tests\LlmFromScratch.Tests.csproj --logger "console;verbosity=minimal"
full_test_result: Failed - Failed: 22, Passed: 2317, Skipped: 0, Total: 2339, Duration: 24 m 48 s - LlmFromScratch.Tests.dll (net9.0)
remaining_failure_count: 22
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

## Claim Scope

claim_scope: Pass229 reran the requested full local test suite after Pass225-Pass228 repairs and recorded the remaining failures.
evidence_scope: Evidence includes required local guidance read/hash receipts, Pass224-Pass228 evidence reads, pre-run branch/status and cached-index snapshot, one full `dotnet test` run, post-run branch/status and cached-index snapshot, and this NEEDS_FIX artifact.
count_claimed: 1 full-suite rerun; 0 full-suite passes; 0 repairs; 0 ProductReady, CI, cloud, deploy, public endpoint, commit, stage, push, PR, merge, rebase, checkout, reset, clean, stash, or external-provider claims.
independent_evidence_count: 1 full-suite execution record plus setup/status/readback evidence records.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: Yes. The evidence supports only a local full-suite failure report and NEEDS_FIX queue.

## Skill Load Status

- AGENTS.md: found/read from disk by PowerShell `Get-Content -Raw`; sha256 prefix A8F6EB15692F727C; gate markers found for no dishonest claims, overclaim, NEEDS_FIX, ProductReady, sandbox permissions, UNKNOWN_IN_SUBAGENT, subagent_id_or_NONE, verification, and claim/evidence scope.
- docs\SESSION-START-PROMPT.md: found/read from disk; sha256 prefix 450961E74C8E9DB4.
- docs\skills\planning-session-startup-SKILL.md: found/read from disk; sha256 prefix C21D82E6CC29995F.
- docs\skills\claim-verification-SKILL.md: found/read from disk; sha256 prefix F6D84C0F3815367C.
- docs\skills\audit-protocol-SKILL.md: found/read from disk; sha256 prefix FF0C6898DEBD3BF2.
- docs\skills\goal-delivery-completeness-SKILL.md: found/read from disk; sha256 prefix 2012CCFF230463C0.
- docs\skills\verification-before-completion-SKILL.md: found/read from disk; sha256 prefix BDEC446CFC62417B.
- docs\skills\test-integrity-SKILL.md: found/read from disk; sha256 prefix FD545554BC746E78.
- docs\skills\generated-code-review-SKILL.md: found/read from disk; sha256 prefix CEB6CF6BBFFF4121.
- docs\skills\implementation-integrity-SKILL.md: found/read from disk; sha256 prefix 18C33DD8C4D74770.
- docs\skills\code-examination-SKILL.md: found/read from disk; sha256 prefix 38DF19AB6D80A0EF.
- docs\skills\qa-session-type-SKILL.md: found/read from disk; sha256 prefix BF5544598D4E358A.

No nested sub-agent was used by this worker.

## Files Read

- AGENTS.md.
- docs\SESSION-START-PROMPT.md.
- docs\skills\planning-session-startup-SKILL.md.
- docs\skills\claim-verification-SKILL.md.
- docs\skills\audit-protocol-SKILL.md.
- docs\skills\goal-delivery-completeness-SKILL.md.
- docs\skills\verification-before-completion-SKILL.md.
- docs\skills\test-integrity-SKILL.md.
- docs\skills\generated-code-review-SKILL.md.
- docs\skills\implementation-integrity-SKILL.md.
- docs\skills\code-examination-SKILL.md.
- docs\skills\qa-session-type-SKILL.md.
- docs\evidence\design-conversion\codex-pass224-full-test-suite.md.
- docs\evidence\design-conversion\codex-pass225-qsg-state-oom-repair.md.
- docs\evidence\design-conversion\codex-pass226-qsg13-17-19-repair.md.
- docs\evidence\design-conversion\codex-pass227-remaining-giant-state-patterns.md.
- docs\evidence\design-conversion\codex-pass228-qsg09-semantic-status.md.

## Branch And Status Snapshots

- Pre-run branch: DesignToCode-clean.
- Pre-run relation: `## DesignToCode-clean...origin/DesignToCode-clean [ahead 2]`.
- Pre-run status count: total 546, staged 0, unstaged 234, untracked 312, conflicted 0.
- Pre-run cached index entries: 0.
- Post-run branch: DesignToCode-clean.
- Post-run relation: `## DesignToCode-clean...origin/DesignToCode-clean [ahead 2]`.
- Post-run status count before this artifact write: total 548, staged 0, unstaged 234, untracked 314, conflicted 0.
- Post-run cached index entries before this artifact write: 0.

## Full Suite Failures

The full suite failed with 22 failing tests:

1. Tests.ExtensionAdaptiveEffectGap02Tests.ExtensionApi_UsesScenarioAlignedUserAdaptiveAssets_AndChangesToken01OutputsWithoutClaims.
   Representative error: `Assert.Contains()` failure. Collection contained `architecture-audit`, `documentation-sync`, and `planning-dpo-authoring`; missing `trainable-unit-design`. Source line: Tests\ExtensionAdaptiveEffectGap02Tests.cs:89.
2. ExtensionQualitySemanticGates10NoOverclaimTests.QualitySemanticGates10Artifacts_AddGraduationCriteriaAndNoOverclaimGuard.
   Representative error: `Assert.Equal()` expected `-1038`, actual `-1032`. Source line: Tests\ExtensionQualitySemanticGates10NoOverclaimTests.cs:376.
3. ExtensionQualitySemanticGates15PromotionReadinessTests.QualitySemanticGates15Artifacts_RecordPromotionReviewAndKeepClaimsClosed.
   Representative error: `DirectoryNotFoundException` for `C:\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-14-report.json`. Source line: Tests\ExtensionQualitySemanticGates15PromotionReadinessTests.cs:41.
4. Tests.ExtensionAdaptiveLearnability01Tests.ExtensionApi_LoadsUserAdaptiveAssetsFirst_AndProvesPortableContinueTrainingLifecycle.
   Representative error: `Assert.Equal()` expected string `true`, actual `false`. Source line: Tests\ExtensionAdaptiveLearnability01Tests.cs:38.
5. ExtensionQualitySemanticGates14ClosureTests.QualitySemanticGates14Artifacts_OpenOnlyBoundedSemanticGraduation.
   Representative error: `DirectoryNotFoundException` for `C:\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-07-report.json`. Source line: Tests\ExtensionQualitySemanticGates14ClosureTests.cs:47.
6. ExtensionQualitySemanticGates19TargetLocatorPlumbingTests.QualitySemanticGates19_VerifiesTargetLocatorPlumbingAndKeepsReadinessClosed.
   Representative error: `DirectoryNotFoundException` for `C:\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-17-test-only-active-locator.json`. Source line: Tests\ExtensionQualitySemanticGates19TargetLocatorPlumbingTests.cs:51.
7. ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates05Artifacts_BuildSemanticProofFrameworkForExtensionApiOutputs.
   Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-04-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:526.
8. ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates06Artifacts_AddLeakageAuthorityControlsWithoutSemanticOverclaim.
   Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-04-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:781.
9. ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates03Artifacts_PassAdaptiveRuntimeRowsWithoutSemanticOverclaim.
   Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-03-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:333.
10. ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates09Artifacts_RepairBaselinePromptToTaskAndOrchestrationRuntimeRowsWithoutReadinessOverclaim.
    Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-04-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:1839.
11. ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates07Artifacts_AddSourceDisjointEvalAndAblationControlsWithoutSemanticOverclaim.
    Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-04-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:1145.
12. ExtensionToken04QualityScoringArtifactsTests.Token04ScoreAfterAdaptiveEffectGap02Artifacts_ShowAdaptiveEffectButNoQualityOrTokenClaims.
    Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\token-04-rerun-after-adaptive-effect-gap-02-quality-scoring-inputs.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:248.
13. ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates08Artifacts_AddSourceDisjointContinueTrainingProofWithoutCommonReadinessOverclaim.
    Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-04-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:1504.
14. ExtensionToken04QualityScoringArtifactsTests.QualitySemanticGates04Artifacts_RepairCommonSkillSelectionButKeepClaimsClosed.
    Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-04-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:427.
15. ExtensionToken04QualityScoringArtifactsTests.Token04RerunWithAdaptiveLearnabilityArtifacts_ShowAdaptiveAssetsLoadedButNoOutputEffect.
    Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\token-04-rerun-with-adaptive-learnability-01-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:172.
16. ExtensionToken04QualityScoringArtifactsTests.Token04QualityScoringArtifacts_HaveExpectedCountsAndNoClaims.
    Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\token-04-quality-scoring-full-hosts-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:25.
17. ExtensionToken04QualityScoringArtifactsTests.Token04RerunAfterQualityGapRepairArtifacts_HaveFreshCountsAndNoClaims.
    Representative error: missing expected artifact `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability\token-04-rerun-after-quality-gap-repair-01-row-scores.jsonl`. Source line: Tests\ExtensionToken04QualityScoringArtifactsTests.cs:91.
18. ExtensionQualitySemanticGates16CandidateProbeTests.QualitySemanticGates16Artifacts_CreateCandidateSetAndExactProbeRecordsWithoutPromotion.
    Representative error: `DirectoryNotFoundException` for `C:\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-15-report.json`. Source line: Tests\ExtensionQualitySemanticGates16CandidateProbeTests.cs:38.
19. DesignConversionTrainingPipelineTests.RunAsync_LocalFreshLoadProbeUsesTempPathAndDoesNotWriteRepoArtifacts.
    Representative error: `Assert.Equal()` collection diff at position 7385. Source line: Tests\DesignConversionTrainingPipelineTests.cs:618.
20. ExtensionQualitySemanticGates18LiveHostTargetRollbackClosureTests.QualitySemanticGates18_RecordsLiveHostTargetBlockersAndKeepsRollbackClosed.
    Representative error: `DirectoryNotFoundException` for `C:\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-17-report.json`. Source line: Tests\ExtensionQualitySemanticGates18LiveHostTargetRollbackClosureTests.cs:46.
21. LlmFromScratch.Tests.DpoTrainerTests.StepMany_WithCopiedReferenceImprovesRewardMarginOnRepeatedPreference.
    Representative error: expected reward margin to improve from `0`, got `0`. Source line: Tests\DpoTrainerTests.cs:222.
22. ExtensionQualitySemanticGates17ProductRuntimeVerificationTests.QualitySemanticGates17_ActivatesQsg16LocatorCandidateInTestOnlyProductApiRuntime.
    Representative error: `DirectoryNotFoundException` for `C:\artifacts\extension-runtime-token-quality-learnability\quality-semantic-gates-16-report.json`. Source line: Tests\ExtensionQualitySemanticGates17ProductRuntimeVerificationTests.cs:51.

## Triage Verdict

No repair was attempted in Pass229 because 22 failures remain. That exceeds the requested 1-3 obvious local failure threshold and includes several failure families:

- Extension artifact root/path mismatch or generation-order failures involving `C:\artifacts\extension-runtime-token-quality-learnability` and `C:\Projects\llm_mvp_core\..\..\..\..\artifacts\extension-runtime-token-quality-learnability`.
- Adaptive learnability/effect-gap assertion failures.
- QSG10 score threshold drift.
- DesignConversion local fresh-load artifact snapshot drift.
- DPO trainer reward-margin assertion failure.

## Commands Run

- `rg --files | Select-String -Pattern ...` to discover required guidance and prior evidence files.
- Required file size/read receipt commands for guidance and evidence files.
- `Get-Content -Raw` reads for Pass224-Pass228 evidence artifacts.
- Required guidance read/hash/marker receipt commands.
- Pre-run branch/status/cached-index snapshot command.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --logger "console;verbosity=minimal"`.
- Post-run branch/status/cached-index snapshot command.

## Files Changed

- docs\evidence\design-conversion\codex-pass229-full-suite-needs-fix.md.

No source/test repair files were edited in Pass229.

## Artifact Verification

artifact_verification: required markers present; conflict_marker_count=0; mojibake_marker_count=0; trailing_whitespace_count=0; cached_index_entries=0; artifact_status=untracked.

## Remaining NEEDS_FIX Or External Blockers

remaining_NEEDS_FIX_or_external_blockers: Pass230 should be a bounded failure-family triage and repair pass focused first on extension artifact root/path authority plus generation-order repair for Token04/QSG14-QSG19/adaptive artifacts, then rerun the affected focused filters. If extension artifact-path repair clears the artifact-missing cluster, subsequent bounded passes should address any remaining adaptive assertion, QSG10 threshold drift, DesignConversion snapshot drift, and DPO reward-margin failure separately.

accepted_bounded_verdict: FULL_SUITE_RERUN_FAILED_NEEDS_FIX_LOCAL_ONLY_NOT_PRODUCTREADY_NOT_CI_NOT_CLOUD_NOT_DEPLOY_NOT_PUBLIC_ENDPOINT.
