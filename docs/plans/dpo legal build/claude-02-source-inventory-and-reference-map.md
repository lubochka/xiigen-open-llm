---
artifact_type: source_inventory_reference_map
authority_source: latest unquoted Luba work order for DPO legal build library
status: first_substantial_recon_pass_done_full_audit_future
last_updated: 2026-06-30
recon_pass: B-21 (R12-recon-libs) — pass 1 of N, honest counter in §"RECON v1 — Gate B"
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
---

# Source Inventory And Reference Map

## Collection Boundary

Collected now means targeted evidence read or referenced for library planning. Future means planned collection still required. This file does not claim internet research, provider terms review, model license review, 20-pass external docs, or 40-pass core-doc library completion.

## Direct Evidence Read Now

| source_id | path | planning use |
|---|---|---|
| P-001 | `C:\Projects\llm_mvp_core\docs\plans\claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` | Existing plan evidence, source map, future phase framing |
| C-001 | `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs` | `LearningBatch`, `LearningTrainingItem`, `LearningEvaluationSet`, `LearningBatchResult`, `ILearningMachine`, `ILearningMachinePair` |
| C-002 | `C:\Projects\llm_mvp_core\00.Core\LogicalContracts.cs` | `ILogicalUnit`, `ILogicalFlow`, `ILogicalOrchestrator`, transition signals, context packets |
| C-003 | `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs` | Adaptive routing, branch execution, transition follow-up, capability trainer boundary |
| C-004 | `C:\Projects\llm_mvp_core\17.DynamicDomains\DynamicDomainGraph.cs` | Graph closure, bridge subphase expansion |
| C-005 | `C:\Projects\llm_mvp_core\22.AgentAlgorithms\MultiAgent\DependencyGraph.cs` | Topological levels, sequential dependency detection, parallel level execution model |
| C-006 | `C:\Projects\llm_mvp_core\11.DPO\Models\DpoPair.cs` | Minimal DPO pair shape: prompt, chosen, rejected |
| C-007 | `C:\Projects\llm_mvp_core\11.DPO\DpoTrainer.cs` | DPO loss, reward margin, accuracy, metrics persistence |
| C-008 | `C:\Projects\llm_mvp_core\26.TrainableRouter\Training\RoutingDpoPairBuilder.cs` | Routing DPO pair precedent |
| C-009 | `C:\Projects\llm_mvp_core\26.TrainableRouter\Training\RoutingTrainingRunner.cs` | Train and evaluate runner precedent |
| C-010 | `C:\Projects\llm_mvp_core\27.TranslationDomain\TranslationMachine.cs` | `ILearningMachine` lifecycle precedent with training history, evaluation, export, import, checkpoint boundary |
| C-011 | `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\UniversalNineNodeDevelopmentFlow.cs` | `ILogicalFlow` and `ILogicalUnit` execution sequence with transition signals |

## Repository Reference Categories

| category | current status | future collection work |
|---|---|---|
| Core contracts | Targeted paths read now | Full 40-pass core-doc library remains future |
| Logical orchestration | Main orchestrator and nine-node flow read now | More flow variants and tests remain future |
| Dynamic graph helpers | `DynamicDomainGraph` read now | Graph engine, PageRank, serialization, project wiki graph domain remain future |
| DPO components | DPO pair and trainer read now | All DPO generators, runners, metrics, and source registry tests remain future |
| Translation machine precedent | One machine implementation read now | Other `ILearningMachine` implementations remain future |
| Planning and review skills | Existing plan references recorded | Skill and guide audit remains future unless reauthorized |

## Xiigen Documentation Roots

| root | current status | planning relevance |
|---|---|---|
| `C:\Users\lubao\Documents\xiigen\llm mvp` | Target folder lives here | Library storage and local xiigen planning context |
| `C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build` | Populated now by this work | Current active library |
| `C:\Users\lubao\Documents\xiigen\llm mvp\extensions` | Evidence root only if later read | Product boundary, local/offline policy, extension targets |
| `C:\Users\lubao\Documents\xiigen\llm mvp\translation_guidance_fixed` | Evidence root only if later read | Translation DPO provenance and source policy precedent |
| `C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2` | Evidence root only if later read | External source registry and legal evidence precedent |

## Legal And Provider Evidence

| item | current status | required future gate |
|---|---|---|
| Provider terms for closed APIs | not collected now | terms_snapshot_gate before any training or judging use |
| Model licenses for open-weight candidates | not collected now | license_snapshot_gate before candidate acceptance |
| Hosted API output-use rights | not collected now | closed_api_distillation_block unless explicit terms allow |
| Data retention and privacy terms | not collected now | privacy_retention_gate before hosted use |
| Geography and export constraints | not collected now | legal_review_gate before provider activation |

## RECON v1 — содержательная библиотека знаний (B-21 / R12-recon-libs, первый существенный проход)

> **Человеческим языком (по-русски).** Это первый СОДЕРЖАТЕЛЬНЫЙ проход рекона:
> per-document/implementation строки по двум корням — `llm mvp` (домены данных/доказательств)
> и `llm_mvp_core` (подпроекты-исходники), с колонками «документ/локация / краткое описание /
> зачем-когда». Named REFERENCES (R-NN ниже) привязаны к точным путям. Каждый путь проверен
> на диске 2026-06-30 (`ls`/`sed`; см. Gate B). Это НЕ заявление о «>=40 проходах»: см.
> честный счётчик прохода в конце раздела. Это первый существенный проход — помечен как такой.

### A. `llm_mvp_core` — подпроекты-исходники (named references R-CORE-NN)

| ref | локация (проверено) | краткое описание | зачем/когда нужно |
|---|---|---|---|
| R-CORE-00a | `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs` | `LearningBatch`(SourceIds :26), `LearningTrainingItem`(:30), `LearningBatchResult`, `ILearningMachine`, `ILearningMachinePair`(:245) | КОГДА: проектируешь GraphDpoPair/provenance/pair — это контракт-канон без правок (Q2/Q3/B-15-split) |
| R-CORE-00b | `C:\Projects\llm_mvp_core\00.Core\LogicalContracts.cs` | `ILogicalUnit/ILogicalFlow/ILogicalOrchestrator`, transition signals, context packets | КОГДА: размещаешь узел/поток/оркестратор графа по уровням L2/L3/L4 |
| R-CORE-07 | `C:\Projects\llm_mvp_core\07.GraphEngine\` | Граф + PageRank + сериализация (соседний к 08) | КОГДА: нужна графовая infra; помни — это baseline, не доказательство обучения |
| R-CORE-08 | `C:\Projects\llm_mvp_core\08.GraphRag\` (Indexer.cs, LocalSearch.cs) | Graph-RAG: индекс узлов → векторы → ближайшие узлы; оркеструет 05/06/07 | КОГДА: ищешь прецедент извлечения/композиции графовых паттернов; источник находок R-17 (контаминация sonnet-*) |
| R-CORE-11 | `C:\Projects\llm_mvp_core\11.DPO\DpoTrainer.cs`(margin :44, avg RewardMargin ≈:119) + `Models\DpoPair.cs` | DPO-loss, reward-margin, accuracy, persist метрик; минимальная форма пары | КОГДА: любой training-update обучаемого узла — сдвиг весов ТОЛЬКО через этот движок (R4), число Q30 отсюда |
| R-CORE-15 | `C:\Projects\llm_mvp_core\15.OrchestratorModel\Logical\` | Регистрация flow-адаптеров, маршрутизация | КОГДА: регистрируешь flow-адаптер обучаемого узла (Trainable Unit pattern) |
| R-CORE-17 | `C:\Projects\llm_mvp_core\17.DynamicDomains\DynamicDomainGraph.cs` | Graph closure, bridge subphase expansion (детерминированная) | КОГДА: нужен ablation-baseline порядка; НЕ выдавать за learned |
| R-CORE-18 | `C:\Projects\llm_mvp_core\18.CapabilityTraining\` | Тренировка capability (граница trainer) | КОГДА: проектируешь lifecycle обучения узла поверх capability-trainer |
| R-CORE-21 | `C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs`(MaxAdaptiveBranchCount=8 :19) + `UniversalNineNodeDevelopmentFlow.cs` | Adaptive routing, потолок веток; flow как упорядоченная цепочка units | КОГДА: потолок N комбинаций (Q16: дефолт 4 / потолок 8); образец ILogicalFlow |
| R-CORE-22 | `C:\Projects\llm_mvp_core\22.AgentAlgorithms\MultiAgent\DependencyGraph.cs` | Топологические уровни, parallel/sequential detection | КОГДА: разделяешь параллельные/последовательные ветки; порядок ≠ авторитет (B-9) |
| R-CORE-25 | `C:\Projects\llm_mvp_core\25.ModelServer\` (`IModelRepository`) | Публикация checkpoint, манифест | КОГДА: common-нога публикует принятый checkpoint (Q6, Trainable Unit pattern) |
| R-CORE-26 | `C:\Projects\llm_mvp_core\26.TrainableRouter\Training\RoutingDpoPairBuilder.cs` + `RoutingTrainingRunner.cs` | Прецедент строителя DPO-пар + train/evaluate runner | КОГДА: строишь DPO-builder (блок 07) и runner-контракт с метриками/held-out |
| R-CORE-27 | `C:\Projects\llm_mvp_core\27.TranslationDomain\TranslationMachine.cs` (RewardMargin литерал :70/:187, SchemaVersion:1 :277, Save/Load) | Полный lifecycle ILearningMachine: история, eval, export/import, checkpoint | КОГДА: эталон lifecycle + находка R-19 (литерал 0.03 НЕ копировать); образец B-18 export/import |
| R-CORE-42 | `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\` | Bootstrap ILogicalUnit-обёртки, prompt-to-task | КОГДА: образец «обёртка статической логики в ILogicalUnit как старт DPO» |
| R-CORE-43 | `C:\Projects\llm_mvp_core\43.LearningLocalWikiData\` / `43.SmartSkillsDomain\` | Local-wiki/skills данные обучения | КОГДА: навигация для доменов local-wiki / skills (E.3-bis) |

### B. `llm mvp` — домены данных и доказательств (named references R-MVP-NN)

| ref | локация (проверено) | краткое описание | зачем/когда нужно |
|---|---|---|---|
| R-MVP-LT | `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\` | Корпуса/наборы перевода | КОГДА: пилот №1 (есть доказанный эталон margin/lifecycle, блоки 10/11) |
| R-MVP-EXT | `C:\Users\lubao\Documents\xiigen\llm mvp\extensions\` | Хост-расширения (Claude/Cursor/Codex) | КОГДА: домен extensions; навигация covered ссылкой (рекон есть) |
| R-MVP-DOC | `C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\` | Наборы documentation-learning | КОГДА: домен documentation-learning; covered ссылкой |
| R-MVP-SM | `C:\Users\lubao\Documents\xiigen\llm mvp\Small model racional\` | Small-model материалы | КОГДА: домен small-model; предусловие — дотянуть строку-цель в карту блока 14 |
| R-MVP-LW | `C:\Users\lubao\Documents\xiigen\llm mvp\local wiki learning\` | Local-wiki наборы | КОГДА: домен local-wiki; предусловие — выделить отдельную навигационную строку-цель |
| R-MVP-DC | `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion server\` | figma→html+css сервер/наборы | КОГДА: host-mapping B-19 (фасад xiigen-platform → FLOW-31), client-выходы Q38 QUARANTINE |
| R-MVP-EXTSRC | `C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\` | Внешние источники + legal-прецедент | КОГДА: legal-гейт teacher/judge; прецедент provider-terms/license |
| R-MVP-TG | `C:\Users\lubao\Documents\xiigen\llm mvp\translation_guidance_fixed\` | DPO-provenance перевода | КОГДА: образец provenance-политики и формата пар |
| R-MVP-IL2 | `C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\` | Внутренние наборы (train-ready) | КОГДА: находка R-18 (расхождение указателей V192/V309) — назвать канонический перед обучением |
| R-MVP-LS | `C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\` | First-party learning-sets | КОГДА: дефолт источника X v1 для cold-start (common-нога, R5/Q6, B-1) |

### C. Codex recon-база как навигационный слой (evidence-only, НЕ редактируется)

> Реальный disk-счёт проверен 2026-06-30: **40** `codex-recon-core-*.md` + **23**
> `codex-recon-mvp-*.md` + **2** `codex-knowledge-library-*.md`. Эти файлы — evidence-only
> навигация бенчмарка по тем же путям; мои named references выше привязаны к **диску**, а не
> к рекон-файлам. Графово-важные кросс-ссылки: `codex-recon-core-08-graphrag.md`,
> `codex-recon-core-07-graph.md`, `codex-recon-core-11-dpo.md`, `codex-recon-core-27-translation.md`,
> `codex-recon-mvp-lang-translate.md`, `codex-recon-mvp-internal-learn-2.md`.

### RECON v1 — Gate B (формат + честный счётчик прохода)

| метрика прохода | значение (pass 1) | определение / правило |
|---|---|---|
| `pass_format` | per-document/implementation row: `ref \| локация(проверено) \| краткое описание \| зачем-когда` | формат строки библиотеки зафиксирован как Gate B; будущие проходы дополняют ЭТОТ формат |
| `core_refs_anchored` | **15** (R-CORE-*) | named references по подпроектам `llm_mvp_core`, привязанные к путям (C6-crossfile арифметика: реальный счёт строк R-CORE = 15 — R-CORE-00a/00b/07/08/11/15/17/18/21/22/25/26/27/42/43; прежнее «14» исправлено по факту таблицы §A) |
| `mvp_refs_anchored` | 10 (R-MVP-*) | named references по доменам `llm mvp`, привязанные к путям (R-MVP-LT/EXT/DOC/SM/LW/DC/EXTSRC/TG/IL2/LS = 10) |
| `line_anchors_verified` | 12 (в `claude-03` §RECON) | точные file:line, проверенные на диске |
| `paths_existence_checked` | true | все локализованные пути проверены `ls`/`sed` 2026-06-30 |
| `pass_count_claimed` | **1 (первый существенный проход)** | НЕ заявляется ">=40 проходов"; «1 файл = 1 проход» запрещено (count-инфляция) |
| `full_per_file_audit` | future_not_done | полный per-file аудит ВСЕХ подпроектов/доменов = будущая работа |
| Gate B (закрытие pass 1) | `core_refs_anchored>=15 AND mvp_refs_anchored>=10 AND line_anchors_verified>=12 AND paths_existence_checked==true AND pass_count_claimed honest(==1)` | PASS для pass 1 (порог `>=15` синхронизирован с фактическим числом строк R-CORE = 15) |

## Будущие проходы (после pass 1)

| phase | status | output expected |
|---|---|---|
| RECON pass 2..N (per-file deep) | future_not_done | per-file строки для оставшихся подпроектов/доменов в ТОМ ЖЕ формате Gate B |
| 20-pass external-doc library | future_not_done | Provider terms, model cards, license snapshots, source registries, legal summaries |
| Provider terms verification | future_not_done | Terms snapshot refs and allowed-use matrix |
| Model candidate verification | future_not_done | Accepted, rejected, or conditional candidate table |
| Implementation source audit | future_not_done | Exact class and interface change plan after Luba approval |
