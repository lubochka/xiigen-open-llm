# ГЛАВА 0 — восстановить успешные обучаемые способности ДО масштабирования RP01

**19.10.2026 · новый адресный сравнительный аудит · SOURCE_CONFIRMED / ANALYTIC_PROXY / NEW_NATIVE_TRAINING_NOT_RUN.**

Эта глава ставит **проверенную историческую способность и её реального получателя** перед очередным большим DPO/LoRA запуском. Пользовательский критерий: после нового урока обычный потребитель действительно лучше решает новые задачи, в том числе после нового процесса/восстановления. Старый проектный Wiki и соседние `language_translate`/`PrepareToGPL` **не считаются одной и той же моделью**. Нельзя объединить их проценты в единую метрику RP01.

## Приоритет восстановления (верх списка)

| Очередь | Исторический положительный механизм | Что подтверждено и что НЕ подтверждено | Минимальное действие |
|---|---|---|---|
| **P0-W1** | **`local_wiki_planning`: Project Wiki request routing** | Архивный реальный пакет 50 train/50 heldout: structural route accuracy **1.0**, confidence **0.9337**; отдельный neural DPO heldout **0.88**, final train accuracy **0.64**, **publication=null**, предупреждение о недостаточных 0.92. Не доказано, что опубликованный neural checkpoint обслуживал обычный wiki-запрос | **Вернуть сохранение prompt + chosen + rejected в neural DPO для wiki routing**, отдельно проверить subject-level маршруты (не только rejected type), затем проверить фактическое применение обученного router после восстановления |
| **P0-W2** | **`local_wiki_planning`: CodeTraceBuilder — обучающие исходники и конкретные следы вызовов** | На исходной ветке `ToDpoMetricPair` передавал `pair.A, pair.Chosen, pair.Rejected`; current заменил всё на условные маркеры defect type. Историческая положительная structural trace не доказывает neural code generation | Вернуть конкретные кодовые входы/исходы в учебный recipient; проверить изменение трассы на независимом C# методе и у обычного consumer |
| **P0-W3** | **`local_wiki_planning`: DPO checkpoint continuation vs fresh deterministic default** | Исторический run043 хранит `policyInitialization`/`referenceInitialization` из предыдущей кампании; current default `ProjectWikiRequestRoutingTrainingPipeline` конструирует `ProjectWikiDpoMetricRunner(deterministicFreshInitialization:true)` без parent policy/reference. Это **source-only risk**, не измеренный эффект полезности | Проверить exact parent→next policy identity на двух batch и old retention, прежде чем менять optimizer/LoRA; [конкретная N49-RESET-01](49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/06-NEW-CONTINUATION-RESET-POLICY-REFERENCE.md) |
| **P0-T1** | **`PrepareToGPL` / translation memory, kNN, typed units и cold restore** | Соседними аудитами зафиксированы специализированные positive receipts 11/24→19/24, 24/24 cold-restored; они относятся к конкретным корпусам/памяти, а не RP01 q/v LoRA | Адресный перенос подходящих typed memory/kNN/flow функций с проверкой направлений, доменного контекста, 4→6 schema и новой сессии |
| **P0-C1** | **`language_translate` E09 structural KNN** | Архивные top1 0.2413→0.3507, k5, 4 492 train pairs, **несовпадающие знаменатели validation**, поэтому число нельзя объявлять причинным эффектом | Matched file-family-disjoint heldout, идентичные метки AST, сравнить реального C# consumer |
| **P0-T2** | **`language_translate` E26 контекст слова** | Архивные 0.40→1.00 на **15 фиксированных примерах**; старое словарное POS/context правило не доказывает изменение нейронных весов | Контекстное разрешение многозначных слов, новые перефразы, отделить deterministic baseline от learned kNN/Unit |
| **P0-R1** | **`language_translate` E43 replay** | Архив old retention 0.3226→0.3978 (old465/new414/replay177), **не LoRA continuation proof** | Одно и то же состояние, старые/новые непересекающиеся семейства, фиксированный бюджет с replay и без него |
| **P1** | E22 domain router, E51A code-context, E09/E26 специализированные признаки | Положительные receipts предметные и неодинаково применимы: AST — C#, POS/order — язык, law-aware retrieval — структурные задачи | Только когда соответствующий домен вызывает нужный recipient; не включать все функции во все домены |

**Граница новизны:** старые P0-T1/C1/T2/R1 и generic потеря входного контекста уже исследованы в соседних документах и **не выдаются за новые открытия**. НОВОЕ адресное сопоставление именно `local_wiki_planning`→`codex/dpo-implementation` для request-router и CodeTraceBuilder зафиксировано в [пакете 49](49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/README.md). Исторический маршрут `WikiTraceAggregator`, где раньше также теряли конкретные трассы, уже зафиксирован в защищённом [исследовании 76](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md); **этот каталог read-only**.

## Почему приоритет W1/W2 выше нового большого пакета LoRA

RP01 с random B0 (vocab=629, 4 блока, LoRA q/v) изменил 16 LoRA тензоров и DPO preference margin, но не улучшил свободные ответы: C# **0/3→0/3**, EN↔RU **0/4→0/4**. Это говорит о недостаточности имеющегося свидетельства полезного навыка, **но не доказывает**, что потеря wiki DPO является причиной именно этих RP01 нулевых результатов: у них другие получатель, темы и consumer.

В пределах W1/W2 доказано более узкое: **в текущем neural DPO-подготовителе часть входа уничтожается ещё до tokenizer/LoRA**, а значит более мощная LoRA не может восстановить отсутствующие свойства конкретного учебного примера. Вернуть данные нужно *до* обсуждения ранга LoRA; затем проверить, что улучшение реально дошло до normal consumer.

## Короткие проверяемые опыты (не выполнены)

1. **W1 / Wiki routing / 4 входа, 2 нужных маршрута, одинаковый rejected type:** старый exact-pair projection против текущего type-only. Результат-граница: B-model input должен отличаться между различными prompt; текущий типовой projection совпадает. Затем 8–16 разрешённых пар, фиксированные initial weights, unseen перефразы, route accuracy и контраст chosen/rejected; без изменения production gate.
2. **W2 / C# trace / 2 разных метода с одним defect type:** прежний DPO содержит имена вызовов и return edges; текущий не содержит. Потом ограниченное обучение, source-disjoint C# methods, precision/recall независимого компиляторного/экспертного gold, проверка обычного CodeTrace consumer.
3. **W3 / Wiki publication & restore / 1 существующий пакет:** проверить `semanticQualityClaimed=false` и `PublishAllowed` у реального `ProjectWikiRequestRoutingTrainingRunner`; не обходить guard. Подготовить независимую смысловую экспертизу и только после прохождения проверить новый процесс/locator.
4. **Сопутствующие P0 translation / C# structural / replay:** следовать уже существующим парным протоколам по доменам, не делать Roslyn feature обязательной для EN→RU.

Полные критерии входов, контрольных случаев, нулевых гипотез и falsifiers — [49/03](49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/03-FAST-DOMAIN-EXPERIMENTS.md). На момент фиксации: **новые native L/U проверки NOT_RUN**.

## Прямые источники

- [Исторический Wiki training pipeline](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L109-L116) и [текущий type-only pipeline](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs#L131-L168)
- [Исторический C# code-trace projection](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L119-L120) и [текущий type-only](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs#L124-L129)
- [Исторический Wiki router run043 с результатами, парами и warning](https://github.com/lubochka/llm_mvp_core/blob/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-20-project-wiki-request-router-trainable-route-calibrated-043-dpo/project-wiki-request-router-weak-route-replay-trainable-route-calibrated-043-result.json)
- [Действующий RP01 diagnostic](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md), [исторические E09/E26/E43](44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md), [translation recovery](44-RECOVERY-FIRST-EVIDENCE-AND-PRIORITY-2026-10-19.md)

**Область изменения:** только эта папка `xiigen-planning/.../possibleDifs/**`. Product code, source branch, frozen outcomes, защищённый исследовательский каталог core — **не изменялись**.
