# QUALITY-SEMANTIC-ROW12-HELDOUT-ABLATION-FRESHLOAD-01

Вердикт: PARTIAL_NEEDS_WORK.

Полная semantic quality для Row12/P01 universal orchestration family не доказана. Текущий bounded run дал только частичную evidence: часть held-out/source-disjoint и ablation checks проходит, но fresh-load и continue-training preconditions не закрыты.

## Текущий focused run

Команда была focused, без broad test run:

```text
dotnet test Tests\LlmFromScratch.Tests.csproj -p:IsTestProject=true --filter "FullyQualifiedName~PromptToTaskPhase04UniversalContractTests|FullyQualifiedName~ApiDashboardSourceDisjointGuardTests|FullyQualifiedName~SmartSkillsSemanticQualityP11FreshLoadTests|FullyQualifiedName~SmartSkillsSemanticQualityP12ContinueTrainingTests|FullyQualifiedName~OrchestrationReasoningP9EvalAblationTests|FullyQualifiedName~OrchestrationReasoningP9BEvalAblationRerunTests" --no-restore --logger "console;verbosity=normal"
```

Результат:

- Total tests: 14
- Passed: 12
- Failed: 2
- Failed P11/P12:
  - `SmartSkillsSemanticQualityP11FreshLoadTests.P11FreshLoadsComponentCheckpointsThroughProjectPaths`
  - `SmartSkillsSemanticQualityP12ContinueTrainingTests.P12RecordsNoOpContinueAndPreservesHeldOutBehavior`
- Failure reason: `Manifest checkpoint ref mismatch` on PromptToTask.

## Что это значит

Current evidence supports only partial preconditions. It does not support a full semantic quality claim.

- PromptToTask: narrow held-out/source-id-family ablation evidence exists from the current focused run, but P11/P12 are blocked by PromptToTask manifest checkpoint ref mismatch.
- OrchestrationReasoning: P9/P9B held-out/ablation and fresh-load-style component evidence passed in the current focused run.
- SkillSelection: source-disjoint runtime eval is missing. Existing P22 evidence says accepted rows = 0 and true source-disjoint runtime side-by-side comparison did not run.

## Exact blockers

- Fresh-load precondition is not clean because current P11 failed.
- Continue-training precondition is not clean because current P12 failed.
- SkillSelection has held-out dataset/material, but no accepted true source-disjoint runtime eval; P22 accepted rows = 0.
- No unified Row12/P01 gate currently proves PromptToTask + OrchestrationReasoning + SkillSelection on the same held-out scenario family.
- No current local/user adaptive continue-training proof with backup/export/import and rollback was run in this bounded packet.

## Next bounded work orders

1. `PTT-P11-P12-MANIFEST-PORTABILITY-FIX-01`: repair PromptToTask candidate manifest/checkpoint references for the sync workspace, then rerun only P11/P12 focused tests with `-p:IsTestProject=true`. No product code edits, no checkpoint promotion.

2. `SKILLSELECTION-TRUE-SOURCE-DISJOINT-RUNTIME-EVAL-01`: create/run accepted SkillSelection runtime eval rows with source/pair/id disjointness, candidate-order controls, filename/path/title controls, exact prompt replay controls, and no training use. Required current target: replace P22 gap where accepted rows = 0.

3. `ROW12-P01-UNIFIED-HELDOUT-ABLATION-GATE-01`: create a same-scenario Row12/P01 gate across PromptToTask, OrchestrationReasoning, and SkillSelection with no marker leakage, no exact prompt memorization, and no candidate-position final authority.

4. `ROW12-P01-FRESHLOAD-CONTINUE-ROLLBACK-01`: after P11/P12 repair, run local/test-only fresh-load and continue-training with backup/export/import and rollback. Do not mutate production/common checkpoints.

## Non-claims

- product_readiness_claimed: false
- semantic_quality_claimed: false
- token_savings_claimed: false
- full_semantic_quality_proven: false
- checkpoint_promotion_claimed: false
- commit_or_push_run: false

## Honesty block

claim_scope: bounded repair packet only, based on already collected/logged evidence.
evidence_scope: existing command logs/evidence from the prior focused run; no new broad search or tests in this repair.
count_claimed: 14 tests executed in prior focused run; 12 passed; 2 failed; 2 packet files created/repaired in this turn.
independent_evidence_count: prior command logs plus this exact file creation and verification.
subagent_id_or_NONE: bounded-sub-agent-worker-current-session
downgrade_needed: yes; evidence supports PARTIAL_NEEDS_WORK only.
