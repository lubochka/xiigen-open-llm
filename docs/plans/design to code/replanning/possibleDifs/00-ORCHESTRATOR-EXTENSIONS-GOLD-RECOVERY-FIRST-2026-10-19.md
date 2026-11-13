# ГЛАВА 0 — orchestrator-extensions: восстановить измеренные способности, а не имитировать успех тестами

Дата: 19.10.2026. Статус: **SOURCE-VERIFIED / HISTORICAL-RECEIPTS; NATIVE RESTORE и QUALITY A/B NOT_RUN**. Авторизованная область: только документы xiigen-planning/llm_plan3/replanning/possibleDifs. **Ничего не менять** в llm_mvp_core/docs/plans/replanning-possible-diffs. Эта глава дополняет, но не отменяет ветки-доноры external_domains, language_translate, local_wiki_planning, prepareToGPL.

## 0.1. Главный эталон и две разные формы «успеха»

- Эталон: [orchestrator-extensions@b293045bd89b1eb96d3e32fedd341f1a4170adbc](https://github.com/lubochka/llm_mvp_core/tree/b293045bd89b1eb96d3e32fedd341f1a4170adbc).
- Проверяемый recipient: [codex/dpo-implementation@44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76](https://github.com/lubochka/llm_mvp_core/tree/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76).
- Первый пакет: [RP01](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/). Из его frozen diagnostic: C#500 изменил LoRA параметры и предпочтительный margin на 0.00122753, но free answers 0/3→0/3; ENRU500 0/4→0/4. Это не эквивалентно успешной исторической гибридной системе.
- **Значение слова «эталон».** Не называть все сохранённые старые PASSED результатами нейросетевого обучения. Ветка содержит независимые обучаемые юниты, retrieval, replay, структурные функции и детерминистическую композицию. E53 записывает e2e_orchestration_success=true при live_external_calls=0; это подтверждение конкретной схемы и ограничений, не прироста качества LoRA или end-user neural generation.

Результаты из исторической ветки, подлежащие восстановлению **только внутри собственных доменов**:

| Donor evidence | Реальное измерение | Домен и ограничение |
| --- | --- | --- |
| [E43 replay retention receipt](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json) | 32.26% без replay → 39.78% с replay; 465 старых, 414 новых, 177 replay | Continual learning / forgetting; не эффект DPO LoRA |
| [E49 expand receipt](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/results/E49-ORCHESTRATOR-EXPANDER/results/run-2026-05-01-001.json) | dependency_accuracy 0.60→0.8667; 3 новых домена, 5 известных | Расширение графа; маленький Tier 1 |
| [E50 closure receipt](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/results/E50-DOMAIN-GRAPH-CLOSURE/results/run-2026-05-01-001.json) | final_edge_delta 0.2883→0.0714; здесь **меньше лучше** | Графовая сходимость, не качество текста |
| [E35 dependency Tier1 receipt](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/results/E35-DEPENDENCY-DETECTOR/results/run-2026-05-01-001.json) | 0.00→0.10 dependency_accuracy на 50 парах | Не заменять этим значение +30% из общей карты другого tier |
| [E53 orchestration proof](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/experiments-v2/E53-HIGH-LEVEL-ORCHESTRATION-PROOF/results/run-2026-05-04-131603.md) | lost_edge_count=0, dependency_violation_count=0, learning_gap_packet_count=4, 0 external calls | One complex onboarding scenario; synthetic/deterministic, not neural proof |
| [Learning model index donor](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/models/index.json) | OrchestrationReasoning / PromptToTask / SkillSelection listed | Asset metadata, NOT an end-to-end model quality score |

## 0.2. P0: подтверждённый разрыв при загрузке обученного состояния

**NEW-GOLD-OR-01 (SOURCE VERIFIED).** Работавший на эталонной архитектуре OrchestrationReasoning checkpoint сохранён в **schemaVersion=1**. В текущем core загрузчик перешёл к **schemaVersion=2** и прямо запрещает чтение v1 без provenance rebuild. Репозиторий при этом содержит прежний checkpoint и manifest версии 1, без соответствующего v2 admission proof. API при провале загрузки оставляет новую пустую машину, записывает ошибку и снижает all_common_assets_loaded.

Цепочка доказательств:
1. [Donor OrchestrationReasoningMachine.cs:111](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L109-L116): CheckpointSchemaVersion=1 и прежняя LoadCheckpointAsync.
2. [Current OrchestrationReasoningMachine.cs:132, 971-980, 1028-1069](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/OrchestrationReasoningMachine.cs#L971-L980): CheckpointSchemaVersion=2; schema-v1 checkpoint/manifest явно возвращает ProvenanceRebuildRequiredCode.
3. [Current committed manifest](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/models/orchestration-reasoning/orchestration-reasoning-v1/manifest.json): schemaVersion=1, learnedPatternCount=15, dpoPairCount=8.
4. [Current named checkpoint](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/models/orchestration-reasoning/orchestration-reasoning-v1/orchestration-reasoning-v1-checkpoint.json): schemaVersion=1, 15 learnedPatterns, 11 acceptedRefIds. Соседний checkpoint.json содержит лишь 4 learnedPatterns/4 acceptedRefIds. **Нельзя мигрировать «первый найденный json» или усреднить счётчики 15/11/8.**
5. [Current XiigenExtensionApiService.cs:1427-1450](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1427-L1450): LoadCheckpointAsync; failure → asset_loaded=false, error ref, warning, не восстановленный OrchestrationReasoningMachine; all_common_assets_loaded=(pttLoaded && orLoaded && skillLoaded).

**Статус:** доказана статическая несовместимость поставляемого v1 common asset и v2 loader при штатном пути; **не проводился runtime replay на конкретном Windows PC и не измерено изменение accuracy**. Не удалять новые проверки: восстановить законное содержимое/источники, создать полноценный **v2 provenance/admission proof**, сохранить семантику 15 паттернов и без права переименовать старый hash как новый.

**NEW-GOLD-OR-02 (SOURCE VERIFIED metadata; IMPACT UNKNOWN).** [Gold models/index.json](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/models/index.json) содержит orchestration-reasoning-v1; [current index](https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/models/index.json) **не содержит**. Это риск discoverability/registry. Однако конкретный XiigenExtensionApiService обращается к фиксированному пути, а НЕ выбирает модель из этого index. Поэтому удаление записи само по себе НЕ считается доказанной причиной API неуспеха; непосредственная причина для данного пути — schema/admission incompatibility. Кроме того, **старый index checkpointSha256 не совпадает с поздним manifest checkpointSha256**: metadata index нельзя считать единственным источником truth без reconcile.

**NEW-GOLD-OR-03 (HIGH-RISK MIGRATION HYPOTHESIS).** Перенос только 8 DPO rows или 4-pattern checkpoint в recipient может потерять продолженную память из 15 learnedPatterns и 11 acceptedRefIds, хотя file/hash/loader gate станет зелёным. Проверить semantic identity patternId, required signals, scenarioRef, confidence, matched outcomes и source refs до/после. Количества не обязаны быть равны, поскольку это разные сущности; не объявлять «лишние» паттерны ошибкой без provenance graph.

**NEW-GOLD-OR-04 (CASCADING AVAILABILITY HYPOTHESIS).** Один failed OrchestrationReasoning common asset делает aggregate all_common_assets_loaded=false даже при успешных PromptToTask/SkillSelection. Проверить влияние именно на потребителей readiness/quality через отдельные статусы компонентов. Снижение aggregate-флага не доказывает по себе ухудшение ответа.

Полный отчёт с evidence: [52-ORCHESTRATION-REASONING-CHECKPOINT-AND-ADMISSION-GAP-2026-10-19.md](52-ORCHESTRATION-REASONING-CHECKPOINT-AND-ADMISSION-GAP-2026-10-19.md).

## 0.3. Другие релевантные пробелы из ранее выполненного аудита: не терять и не дублировать

| Возможное отклонение | Первичные/другие отчёты | Доменная специфика |
| --- | --- | --- |
| Студент без языкового pretraining, DPO margin без полезных ответов; chosen/rejected, mask, gradient | [02-DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [03-DPO](03-DPO-PAIR-AND-GRADIENT.md), [RP01 diagnostic](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-CAUSAL-LEARNING-DIAGNOSTIC-PACK.md) | EN/RU и code generation; нельзя путать с retrieval |
| LoRA recipient/target/rank/optimizer, векторизация, token budget и EOS | [03-LORA](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [04-BASE](04-BASE-LORA-CHECKPOINT.md), [10 inference](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md) | Neural Student; отдельно от trainable pattern unit |
| E07/E13 отрицательные исходы матрицы/базовой метрики → полезные feature в kNN | [12 matrix→kNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [12 provenance](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md) | AST/синтаксис C#; POS и порядок в EN/RU; нельзя включать глобально |
| BM25+kNN, admissibility, ranking/fusion, replay, некорректная выдача | [04-KNN](04-KNN-HYBRID-RETRIEVAL.md), [16 tier-balance](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md), [20 fusion](20-MICRO-FUSION-CAUSAL-EXPERIMENT-AND-CODEX-WORK-ORDER.md) | Wiki/code/translation: разные feature channels и пороги |
| DPO→Unit/Flow many:many/async graph, обучение адресата не связано с обычным потребителем | [05 units](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [06 graph](06-UNITS-FLOWS-GRAPH.md), [18 consumer](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md) | Orchestration, docs→code, Wiki и IDE; typed linking |
| Restore, replay and cold serving; old learners missing consumer | [06 restore](06-CHECKPOINT-RESTORE-ADOPTION.md), [11 historic](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md), [27 replay](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) | Долгая память; correctness и utility раздельно |
| Successful domain donors с разной evidence quality | [00 external](00-EXTERNAL-DOMAINS-SUCCESS-RESTORATION-P0-2026-10-19.md), [00 wiki](00-00-LOCAL-WIKI-GOLDEN-FIRST-2026-10-19.md), [00 translate](00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md) | НЕ переносить оценку одного домена на другой |

## 0.4. Первая очередь восстановительных экспериментов

| Порядок | Опыт | Основной outcome | Статус |
| --- | --- | --- | --- |
| **GOLD-OR-A0 (P0)** | v1 gold vs current unchanged vs v2 **legit provenance-rebuilt** vs current empty baseline; common API, cold restore | LearnedStateLoaded, сохранность всех 15 pattern IDs / 11 accepted refs, zero false semantic claims, unseen orchestration quality | NOT_RUN |
| **GOLD-OR-A1 (P0)** | E53-like unseen graphs с конфликтах/зависимостях, graph ablation | edge recall, invalid parallel, abstention on missing evidence, quality of 5-section answer | NOT_RUN |
| **GOLD-OR-A2 (P1)** | E43-like past/current replay mixed, compare correct cache and no replay | old-domain retention vs new-domain adaptation after cold restart | NOT_RUN |
| **GOLD-OR-A3 (P1)** | Domain kNN ablation: AST order/code; POS grammar/translation; graph edges/orchestrator; lexicon BM25/Wiki | error type per domain, effect size, no cross-domain drop | NOT_RUN |

Для каждого arm требуются одинаковые source-disjoint prompts, frozen evaluator и отделение unit accuracy от consumer answer quality; тесты Windows/GitHub CI не являются acceptance для этих опытов. [Пошаговые микропробы и acceptance](52-ORCHESTRATOR-GOLD-DOMAIN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md).

## 0.5. Граница выводов

- **Источник/код подтверждает** различие форматов, отказ v1, разные количества паттернов, donor receipts, отсутствие записи в index и запуск fallback в API.
- **Не подтверждено запуском** реальное падение качества именно по причине формата, точный способ v2 миграции, численная причинность на heldout, перенос работоспособности всей ветки одним исправлением.
- **Не делать:** отключить проверки provenance, переписать 15 паттернов случайными данными, перенести неизмеренные обещания E53 на DPO/LoRA, скрыть model-not-loaded за зелёным тестом, использовать один AST/POS score для всех доменов.

