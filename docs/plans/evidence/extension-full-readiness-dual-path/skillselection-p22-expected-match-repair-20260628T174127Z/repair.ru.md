# P22 expected-match repair

## Что было не доказано

До ремонта P22 доказывал source-disjoint/anti-leakage controls, но не доказывал runtime expected match: в последнем разобранном baseline было `accepted_rows=2`, `common_expected_match_count=0`, `candidate_expected_match_count=0`.

## Какие labels были неправильными

Старые runtime expected labels были `e2e-test-matrix` и `no-product-decisions`. Это holdout source skill ids и хорошие sentinels для проверки утечки source/path/title/id, но они не были поддержанными trained runtime expected labels для common checkpoint. Поэтому runtime выбирал другие поддержанные skills, а expected-match оставался нулевым.

Для строки `P22-SS-RUNTIME-E2E-MATRIX-001` human-correct runtime expected label заменен на `test-integrity`, потому что prompt просит матрицу сценариев и проверочное покрытие success/idempotency/delay/recovery/boundary/security. `e2e-test-matrix` оставлен как `source_leakage_controls` sentinel.

Для строки `P22-SS-RUNTIME-NO-PRODUCT-DECISION-001` human-correct runtime expected label заменен на `claim-verification`, потому что prompt просит не утверждать/не выбирать user-visible direction без проверки claims. `no-product-decisions` оставлен как `source_leakage_controls` sentinel.

## Что конкретно изменено

В `Tests/SmartSkillsSemanticQualityP22SourceDisjointRuntimeEvalTests.cs` runtime expected labels отделены от source-leakage sentinel labels:

- `ExpectedSelectedSkillIds` теперь проверяет human-correct runtime match.
- новый `SourceLeakageControlSkillIds` хранит holdout source skill ids для path/filename/title/source-id leakage controls.
- leakage controls больше не используют runtime expected labels как sentinel.
- добавлена проверка, что `SourceSkillDocId` остается в `SourceLeakageControlSkillIds`.

## Цифры до/после

До:

- `accepted_rows=2`
- `common_expected_match_count=0`
- `candidate_expected_match_count=0`
- anti-leakage controls были true
- `training_run=false`
- `checkpoint_mutated=false`
- `runtime_promotion_performed=false`
- `semantic_quality_claimed=false`

После fresh run:

- `accepted_rows=2`
- `common_expected_match_count=2`
- `candidate_expected_match_count=0`
- `filenamePathTitleControlsPassed=true`
- `sourceIdPairIdControlsPassed=true`
- all listed no-leakage controls true
- `training_run=false`
- `checkpoint_mutated=false`
- `runtime_promotion_performed=false`
- `semantic_quality_claimed=false`

## Какие тесты прошли

Focused P22 test passed:

`dotnet test Tests\LlmFromScratch.Tests.csproj -p:IsTestProject=true --filter "FullyQualifiedName~SmartSkillsSemanticQualityP22SourceDisjointRuntimeEvalTests" --no-restore --logger "console;verbosity=normal"`

`git diff --check` passed with exit code 0.

## Что все еще НЕ заявлено

Не заявлено: full semantic quality, SkillSelection product readiness, host UI/API readiness, training run, checkpoint mutation, checkpoint promotion, locator update, or product readiness.
