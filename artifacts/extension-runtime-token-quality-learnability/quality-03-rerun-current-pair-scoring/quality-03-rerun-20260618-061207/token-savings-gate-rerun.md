# Token-savings gate rerun

Run: `quality-03-rerun-20260618-061207`

Global status: `CLOSED_GLOBALLY_AND_PER_ROW`.

- rows: 54
- allowed rows: 0
- baseline total tokens: 3189
- adaptive total tokens: 3237
- delta adaptive-minus-baseline: 48
- reduction rows, still claim-closed: 0
- equal rows, claim-closed: 42
- increase rows, claim-closed: 12

Blockers:
- STRICT_QUALITY_NON_REGRESSION_NOT_PASSED: fresh scorer shows limited no-regression, but strict source-disjoint/no-leakage/no-memorization/ablation controls remain inconclusive.
- HANDLER_LEVEL_ONLY_NOT_LIVE_HOST: TOKEN-03/TOKEN-04 are direct XiigenExtensionApiService handler/core-boundary traces, not live Claude/Cursor/Codex UI/chat send-message pairs.
- QSG28_STAGED_TEST_ONLY_TARGET_LIMITATION: adaptive rows use staged/test-only QSG28 target locator, not production locator/checkpoint promotion evidence.
- ORCHESTRATION_REASONING_COMMON_FALLBACK_LIMITATION: adaptive OrchestrationReasoning user/staged asset did not load first and fell back to repo/common.
- SOURCE_DISJOINT_SPLIT_NOT_PROVABLE: current scenario metadata lacks enough source_ref/doc_id/license/train-vs-held-out partition evidence for strict quality pass.
- NO_LEAKAGE_NO_MEMORIZATION_CONTROLS_INCONCLUSIVE: current QUALITY-01/02 controls document the gap but do not close it.
- NO_GLOBAL_TOKEN_REDUCTION_OBSERVED: adaptive total tokens minus baseline total tokens = 48.

Per-row status: every row has `token_savings_claim_allowed=false` and `token_savings_claimed=false`.
