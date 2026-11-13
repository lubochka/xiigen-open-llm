# 49/04 · Источники, дедупликация, инженерное ревью, конкретное поручение исполнителю

19 октября 2026. Цель: проверяемая **дельта относительно реально работавшей ветки**, а не ещё одна общая программа DPO.

## 1. Что прочитано и чем обоснованы выводы

**Прямые source files:** `42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs` old/new; `CodeTraceBuilderTrainingPipeline.cs` old/new; `ProjectWikiRequestRoutingDpoGenerator.cs` new; `ProjectWikiRequestRoutingTrainingRunner.cs` new; `ProjectWikiRequestRoutingTrainableModel.cs` old/new; `ProjectWikiRequestRouterUnit.cs` new; `ProjectWikiTrainingOrchestrator.cs` new; `ProjectWikiTrainingFlowAdapter.cs` old/new. Для контрпримеров по области проверены `WikiContextRoutingTrainingPipeline`, `ProjectWikiQueryPlanningTrainingPipeline`, `ProjectWikiAnswerComposerTrainingPipeline` old/new. Фиксированные refs donor `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` и current `a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`; у двух сравниваемых branch/current исходный `ProjectWikiRequestRoutingTrainingPipeline` blob проверен (донор `55484cd1...`, current `1de590d8...`).

**Архивные реальные данные:** [router train/heldout run043 JSON](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-20-project-wiki-request-router-trainable-route-calibrated-043-dpo/project-wiki-request-router-weak-route-replay-trainable-route-calibrated-043-result.json), включая пары, score, предупреждение и publication null; [Wiki migration report](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/reports/PROJECT-WIKI-TRAINING-ASSET-MIGRATION-REPORT.md), [registry block](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/registry/TRAINING-CONTINUATION-BLOCK.json), [base wiki cumulative batch report](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/base-wiki-runs/session-01-batch-005/base-wiki-batch-summary.json): base wiki `PAIRSETS_READY_CUMULATIVE_DPO_RUNNER_PENDING` не объявлен trained proof. [Source-equivalent projected counts](projection-read-only-receipt.json) — аналитика с честной границей.

**RP01 и RAG:** [RP01 causal diagnostic](../../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md); [RP01 current code map](../../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md); [WorkLeft Master RAG](../../../09.09.26WorkLeft-RAG-MASTER-INDEX.md), [исходное задание](../../../original%20task.md); [DPO legal RAG](../../../09.09.26WorkLeft-RAG-01-dpo-legal-build.md), [DTC RAG](../../../09.09.26WorkLeft-RAG-02-design-to-code.md). RAG индекс направляет поиск, а не подтверждает runtime.

**Skills прочитаны из core `docs/skills` в current branch:** `architectural-decision-testing-SKILL.md`, `intent-to-plan-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `claim-verification-SKILL.md`, `planning-dpo-authoring-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-communication-GLOSSARY.md`. Правила: до гипотезы — поиск caller/consumer; одна переменная на эксперимент; evidence-bound claims; домен как тема; позитивный результат не равно accepted checkpoint; не допускать synthetic teacher attribution; без ослабления safety.

## 2. Дедупликация с двумя прежними каталогами

**Planning `possibleDifs`:** адресно изучены master `00-MASTER-SYNTHESIS-AND-CONFLICTS.md`, `00-LANGUAGE-TRANSLATE-FIRST-PRIORITY-2026-10-19.md`, `44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md`, `44-RECOVERY-FIRST-EVIDENCE-AND-PRIORITY-2026-10-19.md`, `45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md`, пакеты `47` и `48` (в т.ч. новые гипотезы, KNN, neural/unit internals). По каталогу имён также проведена инвентаризация большого множества гипотез разных агентов. **Не заявляется полный построчный аудит 400+ файлов.**

**Protected core `docs/plans/replanning-possible-diffs`:** прочитаны релевантные `76-wiki-trace-donor-input-loss-and-checkpoint-identity.md` (предыдущая потеря WikiTrace A/Chosen/Rejected, **другой code path**), `28-code-trace-learning-donor.md` и `39-code-trace-remediation-lineage.md` (historical trace self-gold, neural checkpoint mismatch), `81-validation-program.md` (раздельно S/C/L/U). Инвентаризировались вложенные материалы `experimets/**` без модификации. **Ни один файл protected каталога не создавался и не менялся.**

| Механизм | Уже существовал | Новая конкретная дельта N49 |
|---|---|---|
| Generic input context lost | Да, `WikiTrace` в protected `76`; DPO prompt erasure в других docs | **OLD/NEW Wiki Request Router ToDpoMetricPair** с ошибочным `CODE_TRACE_DPO`, six-to-one route labels |
| Generic DPO pair collapse | Да, общий N38 input insensitivity, E16 contrast, SFT/stop | **50 реальных old wiki prompts → 4 current projected triples**, split overlap, шестимаршрутная коллизия |
| C# trace static-vs-neural mismatch | Да, protected `28`/`39` | **Прямое различие old/new CodeTraceBuilder DPO bridge** `pair.A/chosen/rejected` → defect-only |
| Checkpoint/adoption not proven | Да, общие audits | **Конкретная современная конъюнкция** `semanticQualityClaimed=false` → `PublishAllowed=false` в Wiki router, совместно с default static orchestrator |
| Historic E09/E26/E43 gains | Уже описаны | **Не переименованы в новые discoveries**, сохранены в «Глава 0» первыми |
| New gradient/AdamW/LoRA details | N48 и protected experiments | **Не дублируются**: исправление матриц после утраты `pair.Prompt` не вернёт исходное информационное содержание |

Новизна формулируется **на уровне конкретного метода и нового old/new сопоставления**, а не «никто никогда не находил подобную ошибку». Данный audit не выполнял exhaustive file-by-file scan всех соседних веток и закрытых бинарников; отсутствие доказательств не означает доказательства отсутствия.

## 3. Ревью: потенциальные ошибочные заявления и как исправлены

| Неверный быстрый вывод | Дисциплинированное уточнение |
|---|---|
| «В успешном доноре DPO router идеально выучился» | Structural route=1.0 на 50 heldout; **neural DPO heldout=.88**, `publication=null`; проверена отдельность recipients |
| «В текущем DPO ровно 4 уникальных обучающих строки» | **Не доказано**: 50→4 рассчитано на архивном корпусе **в текущей проекции**, новый `metricTrain` нужен для фактического подсчёта |
| «Любой CODE_TRACE_DPO marker = неисправность любой модели» | Маркер допустим для обучения defect-type detector, но не эквивалентен route/code-content conditioned DPO |
| «Защитный publication gate надо удалить» | `semanticQualityClaimed=false` — безопасный отказ без независимого semantic evidence; нужен настоящий assessor и consumer |
| «RP01 C#0/3 объяснено нами» | **Не доказано**. RP01 random q/v LoRA — другой recipient, а найденные швы wiki domain-specific |
| «Добавим Roslyn KNN для всех доменов» | Только C#/code structure, для language EN↔RU — POS/order/morphology, для orchestration — typed task routing |
| «Результат нового текста со stale checkpoint = обучение» | Нужно отличить training recipients, per-step grads/state, cold re-open, actual ordinary consumer |

## 4. Команда следующему исполнителю CODEX/Kiro: ограниченный быстрый цикл

**Role:** executor-researcher под архитектурной и независимой проверкой. **Allowed writable** только код экспериментальных локальных веток после отдельного допуска и `xiigen-planning/possibleDifs` для протоколов/отчётов. Текущий аудит не разрешает переписывать production в core; `core/docs/plans/replanning-possible-diffs/**` неизменяем. **Read:** exact old/current source files из §1, RP01 diag, WorkLeft/RAG + active skills. **Focus:** Wiki Router N49-01, затем CodeTrace N49-03, затем N49-02; **not-the-task:** massive RP01 500 batch, CI Windows gates, cosmetic reports, generic LoRA rank tuning.

**Work Order WRK49-01 (C/S, первоочередной):**
- Входы: 4 independent wiki-route fixture prompts из [03](03-FAST-DOMAIN-EXPERIMENTS.md); old/current `ToDpoMetricPair`; origin `ProjectWikiRequestRoutingDpoGenerator`; исторический JSON run043. 
- Действия: instrument current `metricTrain`/`metricHeldOut` **не меняя data authority**, сериализовать модельно-видимые трио без PairId, зафиксировать hash+distinct count+cross-split duplicates+target-label collisions, сравнить со старой `Prompt/Chosen/Rejected` проекцией. Не делать новых label assumptions.
- Выход: `WRK49-01-PAIR-PROJECTION-REPORT.md/json` с exact current SHA, sample IDs/fields, cardinalities, data source authority, falsifiers. 
- Good: «На actual current batch 50 entries → N distinct effective triples; class wrong_training_unit contains K target labels; restored learned route quality UNKNOWN.» Bad: «50 уникальных DPO подтверждены 50 pairId», «архивную проекцию объявили live result».
- Acceptance: всё модельно-видимое содержимое и duplicate semantics доказаны, происхождение source, отсутствие недопустимого teacher call/маскировки. Независимый reviewer читает исходник и raw trace заново, а не только итог автора.

**Work Order WRK49-02 (L/C, только после WRK49-01):**
- Вход: frozen dataset с реально допущенными 8–16 пар, fixed student/optimizer/tokenizer, две проекции различаются только pair bridge.
- Outputs: `WRK49-02-NEURAL-CAUSAL-RESULT.md/json`; numerical chosen/rejected margin **и** independent task accuracy, per-route confusion, at least one unseen law-sensitive case; 2 seeds если возможно.
- Acceptance: source conditioned улучшает новый маршрут без wrong-route regression и без leakage; если нет — гипотеза downgrade, никакого blanket rollback всех изменений.

**Work Order WRK49-03 (U/restore):**
- Reconcile `ProjectWikiRequestRoutingTrainingRunner` constant semantic flag, `PublishAllowed`, `ProjectWikiRequestRoutingTrainableModel.FromSnapshot/Route`, обычный `ProjectWikiTrainingOrchestrator` path и resolver.
- Выход `WRK49-03-CONSUMER-RESTORE.md/json`: fresh-process result; checkpoint sha+unit id+gate evidence, actual downstream consumed state; если current code сознательно запрещает publication, написать BLOCKED_BY_VALID_GATE, а не обходить.
- Acceptance: честная граница; сохранённый trained route влияет на обычный ответ и выдерживает новый процесс **или** явно отсутствует и даёт конкретный минимальный development slice.

**Ревью:** независимый читатель не подписывает REVIEW_PASS без snapshot commit и микроскопического звена generator→DPO projection→recipient→checkpoint→ordinary consumer; не называет выполненным native эксперимент, который был только спроектирован. Если есть источник семейства other agent с такой же exact method-level цепочкой, объединить alias вместо искусственной новой нумерации.

## 5. Итоговый статус

`SOURCE_CONFIRMED: N49-01 N49-03 N49-02(code)`. `ANALYTIC_PROXY_VERIFIED: old-50-projected-current-4`. `NATIVE_LEARNING_QUALITY_NOT_RUN`. `ORDINARY_COLD_RESTORE_NOT_RUN`. `MULTI-BRANCH_COVERAGE_PARTIAL`. `RP01_ROOT_CAUSE_UNRESOLVED`.

Это открытый технический материал с конкретными фальсифицируемыми work orders, **не READY-продукт, не доказанное исправление и не новое обучение**.


## 6. Параллельная проверка других агентов на 19.10.2026 (post-publication collision review)

В ходе работы в этот же каталог параллельно опубликованы два peer-пакета: [49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY](../49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY-2026-10-19/README.md) и [49-LOCAL-WIKI-GOLDEN-RECOVERY](../49-LOCAL-WIKI-GOLDEN-RECOVERY-2026-10-19/02-NEW-DIFFS-AND-DOMAIN-SCOPE.md). Их конкретные главы просмотрены **после** первоначального N49 source-аудита. Новизна уточняется, чтобы пользователь не получил несколько «уникальных» авторств одного и того же факта.

- Наши `N49-01` (Wiki router defect-only), `N49-03` (CodeTrace defect-only), `N49-02` (semantic gate/static consumer) совпадают по основному механизму с peer `D49-01..04` и частью `G-W02`. **Это независимые подтверждения одного и того же source gap, не три дополнительных новых открытия сверх остальных агентов.**
- Наш опыт «исторические 50+50 router043 → четыре rejectedType-класса при current projection» является **отдельной числовой аналитической проекцией другого run043**, в peer `01-DONOR-RECEIPTS...` обсуждаются также более поздние архивы 24 мая. Наша работа не отменяет важность их CodeTrace 064 (DPO .92, published), QueryPlanning134 (DPO 1.0, но uniform chosen/rejected) и Router092 (DPO .66, not published). Выбор 043 — независимый дополнительный пример, не «последний лучший donor».
- Peer [G-W01/G-W05/G-W06/G-W08](../49-LOCAL-WIKI-GOLDEN-RECOVERY-2026-10-19/02-NEW-DIFFS-AND-DOMAIN-SCOPE.md) отдельно исследует **потерянную ClientServerBusinessFlow специализацию**, codec восстановленных label names/order, confidence clamp и исходный negation guard. Эти открытия принадлежат peer-пакету; здесь мы на них **не претендуем**.
- **Отдельная дополнительная гипотеза именно этого пакета:** [N49-RESET-01](06-NEW-CONTINUATION-RESET-POLICY-REFERENCE.md) — current default `deterministicFreshInitialization:true` без родительских policy/reference, тогда как реальный донорский 043 содержит pointers к предыдущей кампании. В просмотренных конкретных peer N49/G-W02 файлах этого method-level paired finding не было; глобальная тема «не продолжили checkpoint» уже известна в каталоге 44. Novelty строго **узкая**, а effect on quality ещё **NOT_RUN**.

**Итог сравнения:** сохранить адресные подтверждения, не считать число файлов числом разных причин. Первые тесты в очереди теперь **CodeTrace064 exact content** (peer positive donor), **router043 source-equivalent reduction**, **N49-RESET-01 multi-batch continuity**, затем consumer/gate. Обе ветки и peer каталоги остаются read-only; ничего из них этим review не редактировалось.
