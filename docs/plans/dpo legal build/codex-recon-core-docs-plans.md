# Codex Recon — core/docs/plans (для плана ILearningDynamicGraph + DPO)

Рекон-аналитик (Opus 4.8). Только чтение + запись этого файла. Не код, не тесты, не git, не внешние модели. Это evidence для будущего плана, не разрешение на реализацию.

corpus = core; slug = core-docs-plans.

---

## 1. Что это и где (точные ключевые пути)

Область цели: `C:/Projects/llm_mvp_core/docs/plans` (большая, ~150+ файлов; глубоко прочитано 3–8 самых релевантных под вопрос про DPO/translation/orchestration/trainable/graph).

Самые релевантные планы (путь + 1 строка):

- `docs/plans/claude-ILEARNINGDYNAMICGRAPH-DPO-PLAN-v1.md` — ПРЯМОЕ ПОПАДАНИЕ: готовый план `ILearningDynamicGraph` + DPO для Unit/Flow/Orchestrator поверх `ILearningMachine` (Part A, source map S1–S12/RI-03..13, DPO/training/measurement guides, ordered_phase_queue, legal/provenance matrix). claude-* = read-only evidence, мне его нельзя править.
- `docs/plans/ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION-v1.md` — каноничный дизайн `ILearningMachine<TDomain>` и переход от детерминированной C#-оркестрации к обучаемой топологии; решения A-LM-01..A-LM-08.
- `docs/plans/DPO-KNOWLEDGE-MAP-v1.md` — governance-карта: какой trainable/adaptive домен какие DPO skills/guides/schema/taxonomy/validation обязан загрузить; "DPO later" при claim trainable readiness = провал.
- `docs/plans/TRAINABLE-ARCHITECTURE-AUDIT-v4.md` — свежий вердикт PASS_WITH_LIMITATIONS: machine делегирует решение в typed `ILogicalUnit` (а не скрытый private scorer); nearest/KNN остаётся честным baseline, semantic quality NOT claimed.
- `docs/plans/SKILL-SELECTION-DPO-NEGATIVE-STRATEGY-v1.md` — образец DPO negative taxonomy (SS-DPO-01..12), universal-vs-project-specific правило, row-level binding template, quality gates; static strategy, модель не обучает.
- `docs/plans/ORCHESTRATION-AND-PROMPT-TO-TASK-FULL-EXTENSIONS-PLAN-v3.md` — two-leg `ILearningMachinePair` (common+personal), correction/result-gap DPO в `.xiigen`, обязательность DPO-KNOWLEDGE-MAP; содержит анти-паттерн `_candidatePositionWeights` (позиционный счётчик, НЕ semantic selector).
- `docs/plans/ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md` — интегрированный trainable-orchestration план: `PromptFlowGroundingModel` владеет learned grounding end-to-end; `FlowReferenceDetector`/`FlowIndex` = build-time only; exact-prompt leakage обязан валить gate; модели exportable/importable/locatable/continue-trainable.

Смежные (по имени, не читались глубоко, для будущих pass): `TRANSLATION-CORPUS-SOURCING-PLAN-v1.md`, `TRANSLATION-UNIT-DPO-PAIR-AUTHORING-GUIDE-v1.md`, `TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md`, `GRAMMAR-RULE-DPO-REGISTRY-GUIDE-v1.md`, `ARCHITECT-BEHAVIOR-DPO-GUIDE-v1.md`, `TRAINABLE-ARCHITECTURE-AUDIT-v1..v3.md`, `DESIGN-CONVERSION-SERVER-ILEARNINGMACHINE-PLAN-v1.md`.

Скилы (ШАГ 1): `docs/HOW-TO-USE-SKILLS.md` существует (v27.5, 782 строки). Правила классификации взяты из триггер-таблицы: любой scorer/selector/classifier/router/KNN/dictionary/pattern-matcher → грузить `trainable-architecture-decision-SKILL.md`; graph/DynamicGraph trainable → + `human-readable-architecture-audit` + `codex-planning-quality-GUIDE` + `plan-review`, с требованием nodes/edges, input/output контрактов, many-to-many, fan-out/fan-in, async, и deterministic-vs-trainable authority per edge; любой DPO/distillation teacher → provenance-поля (teacher_access_mode, model_license, provider_terms_checked...), запрет Claude/OpenAI/Gemini как distillation teacher.

---

## 2. Как устроено / работает (кратко)

Иерархия (из ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION §2, подтверждена `00.Core/LearningMachineContracts.cs`, `00.Core/LogicalContracts.cs`):

- Level 5 `ILearningMachine<TDomain>` — единственный владелец TrainBatchAsync/EvaluateAsync/ExportAsync/ImportAsync/GetOrchestratorAsync.
- Level 4 `ILogicalOrchestrator` — runtime-координатор flow.
- Level 3 `ILogicalFlow<TInput>` — топология units.
- Level 2 `ILogicalUnit<TIn,TOut>` — execution-only ячейка (Train на ней ЗАПРЕЩЁН, A-LM-02).
- Level 1 `ILearningMachinePair<TPersonal,TGlobal>` — common + user-adaptive с анонимной синхронизацией.

Ключевой тезис claude-плана (§4.1): шестого trainable-уровня НЕТ. `ILearningDynamicGraph` — это не новый интерфейс, а `ILearningMachine<DynamicGraphDomain>`, чей `GetOrchestratorAsync()` возвращает `DynamicGraphLogicalOrchestrator`, который гоняет `DynamicGraphCandidateFlow[]` из learning-units (SourceEncoder → BranchProposal → Critic → Improver → ConfidenceLifecycle). Async: независимые ветки — параллельно, зависимые — в порядке графа (precedent `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs:444-568`).

DPO-цепочка: Critic → `DpoPairBuilder` (chosen/rejected + provenance) → `LegalProvenanceRegistry` → `DpoTrainer` (loss/reward margin/accuracy, `11.DPO/DpoTrainer.cs`). Train-batch shape прецедент 50 train + 50 held-out source-disjoint (`42.ProjectWikiDomain/CrossStackBusinessFlowGraphTrainingRunner.cs`, `27.TranslationDomain/TranslationMachine.cs`).

---

## 3. Релевантность к ILearningDynamicGraph

Очень высокая. Эти планы дают почти готовый каркас для целевого плана:

- Обучение/lifecycle: A-LM-01..08 фиксируют, что обучение только на `ILearningMachine`; precedent train/eval/checkpoint/fresh-load — `TranslationMachine`.
- Граф: closure/bridge subphases (`17.DynamicDomains/DynamicDomainGraph.cs`), topological sort/levels (`22.AgentAlgorithms/MultiAgent/DependencyGraph.cs`), storage/ranking (`07.GraphEngine` PageRank/KnowledgeGraph) — все классифицированы как детерминированные хелперы, не authority.
- Оркестрация: routing/concurrency/dependency/transition-signal паттерны уже есть в `AdaptiveLogicalOrchestrator`.
- DPO/измерение: схема пары, rejected-таксономия, held-out/leakage controls, метрики, legal/provenance.
- Teacher-provenance: claude-план R19/R20/§6.1 и HOW-TO-USE-SKILLS требуют provider terms/license/request_id; closed API (Claude/OpenAI/Gemini) запрещены как distillation teacher; Qwen Apache-2.0 — кандидат только после проверки.

---

## 4. Классификация (уровень vs детерминированный хелпер)

Правило-основа: HOW-TO-USE-SKILLS триггеры + `trainable-architecture-decision-SKILL.md` (G1 в claude-плане): scorer/selector/classifier/router/KNN/Jaccard/BM25/dictionary/counter/regex и private `TryDecide*`/`Choose*`/`Count*` — НЕ архитектурные паттерны, только implementation detail внутри разрешённого уровня.

| Механизм (из планов/кода) | Классификация | Почему / правило |
|---|---|---|
| `ILearningDynamicGraphMachine : ILearningMachine<DynamicGraphDomain>` (планируемый) | Разрешённый уровень — `ILearningMachine` | Владеет train/eval/export/import/orchestrator; единственный trainable authority после gates (A-LM-01/02) |
| `DynamicGraphLogicalOrchestrator` (планируемый) | Разрешённый — `ILogicalOrchestrator` | Runtime-координатор, не learner |
| `DynamicGraphCandidateFlow` (планируемый) | Разрешённый — `ILogicalFlow` | Топология units; сам не заявляет semantic quality |
| `SourceEncoder/BranchProposal/Critic/Improver/ConfidenceLifecycle Unit` (планируемые) | Разрешённый — `ILogicalUnit` | Typed execution-cells; обучаемое состояние грузят, но Train на них нельзя (A-LM-02) |
| `ILearningMachinePair` common+personal (v3) | Разрешённый — `ILearningMachinePair` | Нужны обе ноги: общая + user-adaptive с анонимной синхронизацией (A-LM-08) |
| `DomainGraphClosure`, `DependencyGraph.Sort`, `OrchestratorExpander` | Детерминированный хелпер | Упорядочивает/расширяет ветки; не semantic-quality authority (claude §4.3, §6.4) |
| `KnowledgeGraph`, `PageRankCalculator`, `GraphSerializer` | Детерминированный хелпер (storage/ranking) | Candidate gen / retrieval / diagnostics; PageRank нельзя заявлять как learned semantic authority |
| `DpoTrainer` метрики | Metric engine | Меряет loss/reward margin/accuracy; не доказательство product quality сам по себе |
| `_candidatePositionWeights[index]` (v3, строка ~398) | Детерминированный хелпер-ЛОВУШКА | Позиционный частотный счётчик, НЕ semantic selector; нельзя оставлять финальной authority (CLAUDE.md trainable-gate; HOW-TO triggers) |
| `FlowReferenceDetector` / `FlowIndex` / exact-prompt lookup (v20) | Детерминированный хелпер (build-time/eval only) | В production routing заменяется trainable `PromptFlowGroundingModel`; exact-prompt leakage обязан валить gate (v20 D1–D3) |
| nearest/KNN в `PromptToTaskTaskPatternUnit` (audit-v4) | Хелпер внутри разрешённого `ILogicalUnit` | Прозрачный baseline; явные маркеры `BASELINE_NOT_SEMANTIC_QUALITY`; semantic quality not claimed |

Принцип репараций (CLAUDE.md + skill): любую Dictionary/counter/KNN/scorer/private-decision нельзя «обернуть в unit» абстрактно — нужен per-field blueprint: typed state, training update algorithm, KNN/nearest сигнатуры и роль, source/license gate, positive/rejected/quarantine DPO примеры, lifecycle (user/cache → repo/common fallback → backup/export/import/share → continue-training from checkpoint).

---

## 5. Пробелы / что доисследовать

- claude-* план — read-only evidence; нельзя редактировать в этой роли. Если целевой артефакт = другой файл, использовать claude-план как образец, не как рабочий артефакт.
- Кодовые ссылки S1–S12 (LearningMachineContracts.cs, LogicalContracts.cs, AdaptiveLogicalOrchestrator.cs, DpoTrainer.cs, TranslationMachine.cs и т.д.) НЕ перечитаны напрямую в этой сессии — это требование 40-pass `llm_mvp_core` library pass (claude §8.2), пока packet-only.
- 20-pass external docs library и provider terms (Qwen/DeepSeek/Kimi/GLM/Mistral) — не выполнялись (нет интернета); legal/provenance matrix остаётся будущей фазой.
- Translation/grammar/architect DPO guides (раздел 1, «смежные») прочитаны только по имени — нужны для полноты DPO-таксономии и held-out правил.
- `claude-...-PLAN-v1.md` прочитан до строки 588 из 921 (хвост: разделы 9–15, финальные вопросы для Luba) — при использовании как образца дочитать остаток.
- HOW-TO-USE-SKILLS.md прочитан 1–433 из 782 + триггер-таблица; нижняя половина (детальные схемы скилов) не вычитана.
- Mojibake: v3 читается через Grep как UTF-8-каша (русский), но смысл восстановим; при цитировании брать через Read с корректной кодировкой.
