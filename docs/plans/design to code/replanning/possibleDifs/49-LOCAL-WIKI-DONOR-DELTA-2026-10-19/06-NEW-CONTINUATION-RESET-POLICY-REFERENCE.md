# N49-RESET-01 — текущий Wiki Router может начинать каждый DPO batch с одинаковой новой модели вместо продолжения обучения

**19 октября 2026. НОВАЯ source-specific гипотеза восстановленного донорского механизма. CODE_CONFIRMED + ARCHIVAL_CONTINUATION_RECEIPT. Native before/after semantic quality NOT_RUN.**

## 1. Короткое наблюдение

В историческом `local_wiki_planning` пакете request-router `...weak-route-replay-trainable-route-calibrated-043` отдельные поля `dpoMetrics.policyInitialization` и `dpoMetrics.referenceInitialization` **не равны fresh/random**: они указывают на `policy_final.bin` и `reference_base.bin` **предыдущей** кампании `...weak-doc-validation-trace-from021-gentle-034`. Это реальная сохранённая запись **заявленного checkpoint continuation**. Она не аттестует полностью бинарники, optimizer state, actual path readability сегодня, но доказывает, что старый pipeline имел и использовал путь передачи конкретного предыдущего policy/reference в отдельный DPO runner.

В текущем `codex/dpo-implementation` **конструктор по умолчанию** `ProjectWikiRequestRoutingTrainingPipeline.cs:17-30` создаёт:

```csharp
_dpoMetricRunner = dpoMetricRunner ??
    new ProjectWikiDpoMetricRunner(
        epochCount: 8,
        learningRate: 1e-3f,
        deterministicFreshInitialization: true);
```

[Прямой код](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L17-L30).

Этот конструктор **не получает** `initialPolicyCheckpointPath`, `initialReferenceCheckpointPath` или repository-owned model context. Соответствующий `ProjectWikiDpoMetricRunner.cs:501-537` при таком сочетании:
- создаёт `new LlmModel(config)` для `policy` и `reference`;
- при отсутствии обоих начальных checkpoint вызывает `InitializeDeterministicWeights(policy, "project-wiki-dpo-metric:" + unitId)`, устанавливает `policyInitialization="deterministic_fresh_policy"`;
- копирует новые веса в reference `CopyWeights(policy, reference)`, задаёт `referenceInitialization="copied_from_deterministic_fresh_policy"`.

[Точный блок initialization](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L488-L545).

**Source-level вывод:** в текущем default Wiki request-routing DPO metric path отсутствует продолжение policy/reference из прошлого batch; старт детерминированно привязан к `unitId`, **не к предыдущему checkpoint и не к предыдущей учебной истории**. Если две кампании используют этот default path, вторая начинается с такого же type of fresh initial policy, а не с адаптированного состояния первой. Ранний DPO gain может быть **сброшен** между 50-строчными batch-ами независимо от качества новых DPO данных. Это отличается от RP01 `C#500→ENRU500` и от `LoRA` (другой runner/получатель).

## 2. Нюанс старого кода: не назвать всё «регрессией из-за flag»

И в прежней реализации старый `ProjectWikiDpoMetricRunner` конструктор без специально переданных параметров начинал с fresh policy; там поддерживались `initialPolicyCheckpointPath` и `initialReferenceCheckpointPath`. Значит **сам по себе текущий default fresh не доказывает, что прежний default автоматически продолжал обучение**. Точный обнаруженный разрыв — между **фактически сохранённым историческим continuation-вызовом** и **нынешним типовым вызовом** без передачи parent. В иных current callers можно вручную передать отдельный runner с checkpoint paths или войти через repository-owned `Start`; нельзя утверждать, что в системе вообще невозможно continuation.

Current runner `:488-499` имеет отдельную ветку `repositoryOwnedModels`, где policy/reference заранее подготовлены; эту законную альтернативу проверить, не обходить её. [Точное место](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L488-L499).

## 3. Чем отличается новая гипотеза от соседних аудитов

- [44/08](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md) уже обсуждает optimizer AdamW C#→ENRU. [44/19](../44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md) — нейронный checkpoint в общем. Это **известные проблемы**.
- Другие недавние пакеты `49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY` и `49-LOCAL-WIKI-GOLDEN-RECOVERY` подтвердили reset source data, router gate и index semantics; проверенные их method-level гипотезы **не описывали конкретный current `deterministicFreshInitialization:true` default + previous-policy/reference lineage из 043**.
- Новая узкая гипотеза **N49-RESET-01**: 50-строчный Wiki router, даже при разрешённых новых парах и правильном DPO objective, повторно учится **с нуля**, а не продолжает предыдущее learned policy, если orchestration не передаёт ни parent, ни repository-owned model context. Это **другой failure mode** и иной адресат, чем generic RP01 optimizer reset.

## 4. Маленькая проверка C — без expensive training

**Вход:** текущий код `ProjectWikiRequestRoutingTrainingPipeline` default construction и один `ProjectWikiDpoMetricRunner` через разрешённый тестовый harness; исторический DPO summary from 043. Не менять живой code, не трогать protected docs.

**C0:** создать default runner дважды для разных новых batch IDs с **одним и тем же unitId**, выписать internal initial policy/reference identities и `policyInitialization` (желательно до первого optimizer update), подтвердить exact hash/logits. Ожидаем: `deterministic_fresh_policy`, одинаковый initial-policy hash при неизменных config/unitId (проверить actual seed implementation, если другой global noise — отметить). Даже если два начальных хэша оказались разными, **parent checkpoint не передан**, следовательно это всё равно *не continuation*.

**C1:** создать authorized continuation (только с правильными исходными checkpoint+reference, admitted new pair IDs и manifest matching) и выписать `policyInitialization`/ `referenceInitialization`. Ожидаем: адреса принятого exact parent, не `deterministic_fresh_policy`. Любую невозможность разрешённого продолжения отметить `BLOCKED_BY_VALID_AUTHORITY`, а не обходить.

**Мини-обучение L** только после C: при лимите runner на 50 использовать два кратких разрешённых 50-row batch (не притворяться, что текущий runner легально принимает 8). Две arms имеют одинаковый pair order/optimizer/epoch budget и gold families:
- A — исторический continuation policy/reference → new batch.
- B — новый fresh deterministic → тот же new batch.

Заранее выбрать 12 независимых wiki route prompts из старых и новых семейства (без exact overlap после projection). Метрики `old-route retention`, `new-route acquisition`, `absolute chosen logP`, `DPO heldout preference`, `actual learned route consumer`, время/память. Повторять с одинаковым RNG в случае разрешённой source runtime.

**U** только при изменении полезного результата: новый процесс загружает именно checkpoint A/B, normal route исполнителя; сравнить `learned disabled`, `learned restored`. Если ни один ordinary caller его не использует, результат — trainer-only и adoption gap.

**Falsifier:** если реальный current Wiki routing orchestrator уже всегда поставляет exact parent+reference или `repositoryOwnedModels` во все intended live runs, а native evidence демонстрирует сохранённую old+new способность между batch, то source default branch существует, но **не является причиной production degradation**. Если continuation не приносит пользы после independent gold, снижать priority.

## 5. Дополнительное взаимодействие с N49-01

Два независимых ограничения могут накладываться:

```text
current DPO pair bridge: exact Prompt/Chosen/Rejected
  -> defect-type-only tokens          [N49-01]
  -> DPO metric runner from fresh     [N49-RESET-01]
  -> new isolated checkpoint/report
  -> PublishAllowed false / ordinary router still static [N49-02]
```

Нельзя «починить» downstream publication gate до подтверждения качественного входа и разрешённого continuation; нельзя объявлять fail-closed gate ошибкой.

**Источники:** [old runner:30–45,109–137](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L30-L45); [старый run043 с `dpoMetrics.policyInitialization`/`referenceInitialization`](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-20-project-wiki-request-router-trainable-route-calibrated-043-dpo/project-wiki-request-router-weak-route-replay-trainable-route-calibrated-043-result.json); [new runner:90–117](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L90-L117); [new runner initialization:488–545](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiDpoMetricRunner.cs#L488-L545).

**Статус:** source VERIFIED, donor receipt VERIFIED, no new native run, causal impact UNKNOWN. В protected core каталоге ничего не менялось.
