# Codex Recon — Эксперименты E01-E50 (core docs): метрики, SCALE_UP, DPO/flow evidence

Назначение: evidence для ПЛАНА `ILearningDynamicGraph` (авто-построение DPO для
ILearningUnit/ILearningFlow/ILearningOrchestrator/ILearningDynamicGraph). Это не
разрешение на реализацию. Только реально найденные пути.

Под-вопрос рекона: **E01-E50 — результаты обучающих экспериментов, метрики,
флаги SCALE_UP.**

---

## 1. Что это и где (точные ключевые пути)

- `C:/Projects/llm_mvp_core/docs/EXPERIMENT-RESULTS-MAP.md` — главный реестр
  результатов E01-E50. Шапка: 59 прогонов (включая V2-варианты) — 30 PASSED,
  5 SCALE_UP, 27 FAILED. Обновлён 2026-05-01.
- `C:/Projects/llm_mvp_core/docs/skills/experiment-design-SKILL.md` — правила
  Tier 1/2/3, эскалации и определение SCALE_UP ("needs more data, algorithm
  correct"). Строки 13-27, 60.
- `C:/Projects/llm_mvp_core/docs/plans/test hypothesys/small experiment/experiment-result-schema-v1.json`
  — JSON-схема результата одного микро-эксперимента (baseline vs improved):
  блок из 5 метрик (`prediction_error`, `time_ms`, `memory_peak_mb`,
  `cpu_avg_pct`, `disk_write_bytes`), `delta`, `verdict.passed`,
  `verdict.next_action` enum (PROMOTE_TO_NEXT_TIER / TUNE_AND_RETRY /
  ABANDON_APPROACH / NEEDS_HUMAN_REVIEW).
- `C:/Projects/llm_mvp_core/docs/projects/experiments-v2-runners.md` — карта
  runner-ов `experiments-v2/*/runner/*` (DPO-тренер, KNN, teaching, held-out,
  comparison) с разделом anti-determinism/anti-false-done. Самый релевантный
  для DPO-части плана.
- `C:/Projects/llm_mvp_core/docs/plans/E51-DUAL-TRACK-EXPERIMENT-STRATEGY-v1.md`
  и `C:/Projects/llm_mvp_core/docs/plans/DYNAMIC-FLOW-MINI-E2E-EXPERIMENT-PLAN-v1_0.md`
  — самые свежие планы экспериментов; E51B/E51-DYNAMIC-FLOW-MINI-E2E — ближайший
  к dynamic-graph прообраз (станции, арбитры как hard gate, knowledge-edge →
  learning packet, KNN как память а не авторитет).
- Реестры планов экспериментов E01-E07 (V2): `docs/plans/test hypothesys/small
  experiment/V2-extracted/plans/E0x/...`. Инфраструктура: `34-experiments.md`,
  `35-experiments-v2.md` в `docs/projects/`.

---

## 2. Как устроено / работает (кратко)

- **Метрики (схема v1).** Каждый эксперимент = одна пара baseline vs improved.
  Меряются 5 групп: ошибка предсказания (top1/top5 accuracy, absolute_error,
  metric_type: token_prediction|ngram_hit_rate|bm25_recall|perplexity), время
  (ms, per-token), пиковая память, CPU %, дисковые записи. `delta.goal_met=true`
  только если ВСЕ метрики улучшились на >=5%. `verdict.passed=true` = улучшение
  >=5%.
- **PASSED (валидные для прод, MAP §"Validated for production use").**
  Примеры значимых для графа: E04 BM25 +100% recall@3; E09 K-NN structural
  +45% top1; E18 PatternClassifier +15% top1 / 95% domain; E19 Complexity router
  +18%; E32 EpisodeDecomposer +11% count; E34V2 Hybrid BM25+K-NN +8% hit@1;
  E35 DependencyDetector +30% (исп. в 15.OrchestratorModel и 22); E39
  QualityOrchestrator +20% check_f1; E40V2 TrustScorer +50%; E49
  OrchestratorExpander +44% dependency_accuracy (17.DynamicDomains, 22); E50
  DomainGraphClosure +75% final_edge_delta (17.DynamicDomains).
- **SCALE_UP (5 шт., нужно больше данных, алгоритм корректен).** MAP §"Scale-up
  required": E14 ContextAction K-NN (данные слишком разделимы на Tier 3); E20
  StackDetector (синтетика 100% тривиальна на Tier 3). Skill добавляет, что
  bigram-семейство (E28-E31, E44, E45) помечается SCALE_UP, т.к. bigram не даёт
  градиентного сигнала. Правило Tier-эскалации: FAILED на Tier 2 ≠ гипотеза
  неверна → эскалация на Tier 3 перед финальным вердиктом.
- **FAILED — архитектурная находка (27 шт.).** Ключевые уроки для графа:
  E06 PageRank MRR -22% (BM25 проще и лучше); E07 структурный контекст-токен
  -13% (шум в K-NN); E13 method K-NN 0% (методы слишком коротки для эмбеддинга);
  E24 AttributedRouter -37% (атрибуция повышает шум); E33 StopSignalDetector,
  E36 FeedbackCorrector, E37 MergerSynthesizer, E45/V2 CurriculumOrganizer, E46
  ConfidenceGate, E48 DomainBootstrapper — 0% на тривиально-решаемых/слишком
  разделимых данных; E47 ModelHubSearcher -8% (точный tag-match бьёт BM25).
- **Критичные архитектурные находки (MAP §"Key…").** E20: StackDetector ДОЛЖЕН
  идти до E15-нормализации (иначе маркеры уничтожаются). E44: max_weight =
  total_examples/(num_classes × rare_examples), константа ×10 вызывает забывание.
  E40: StratifiedSplit обязателен при фиксированном seed. E34V2: BM25+K-NN fusion
  бьёт каждый по-отдельности (D-10 в DECISIONS-LOCKED).
- **Runner-ы (experiments-v2-runners.md).** Консольные обёртки читают
  JSONL/CLI/hard-coded пути, вызывают библиотеки проекта (`26.TrainableRouter`,
  `35.ExperimentsV2`, `18.CapabilityTraining`, `11.DPO`, `03.TransformerModel`),
  пишут summary JSON/JSONL/MD/checkpoint. Среди них: `InternalTrainReadyDpoRunner`
  (DPO StepMany + policy checkpoints), `E13B…` (KnnLogicalUnit.Train),
  `Teaching20RowsCurrentRunner` / `XiigenSessionBatchedTeachingRunner`
  (route → knowledge-edge branches → `DefaultLogicalCapabilityTrainer
  .TrainAndRegisterAsync` → flow/unit registries), `ProjectWikiBaseHeldoutEvaluator`
  (загрузка checkpoint, preference/contract metrics).

---

## 3. Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- **DPO.** Готовый референс пайплайна DPO в проекте: `InternalTrainReadyDpoRunner`
  (`experiments-v2/INTERNAL-TRAIN-READY-V191/...`) — нормализация пар
  prompt/chosen/rejected → `DpoBatch` → `trainer.StepMany` → периодический
  `policy.SaveCheckpoint`. План графа по авто-построению DPO может опираться на
  этот формат пар и checkpoint-механику как на существующий образец; но по
  правилам doc-а DPO-метрика и checkpoint != продуктовая готовность без
  manifest/locator/fresh-load/use/continue.
- **Граф / оркестрация / станции.** E50 DomainGraphClosure (+75% final_edge) и
  E49 OrchestratorExpander (+44% dependency) — прямые «графовые/оркестраторные»
  PASSED-результаты (17.DynamicDomains, 22). E35 DependencyDetector (+30%) питает
  15.OrchestratorModel. План DynamicGraph (узлы/рёбра, fan-out/fan-in,
  авторитет per-edge) имеет здесь эмпирическую опору для рёбер графа.
- **Ближайший прообраз dynamic-flow.** `DYNAMIC-FLOW-MINI-E2E` (E51B): один
  логический путь для обучения и runtime (тот же компилятор), локальная
  K-NN-топология выбирает станцию, арбитры — hard gate авторитета, knowledge-edge
  → context pack + learning packet. Главное правило для графа:
  **«K-NN can choose memory. Arbiters decide acceptance»** — память (KNN) не есть
  авторитет; авторитет — у арбитров/гейтов. Это напрямую отображается на дизайн
  «deterministic-vs-trainable authority per edge» в плане DynamicGraph.
- **Измерение.** Схема v1 даёт готовый контракт метрик/verdict/next_action для
  любого узла-эксперимента графа; e2e-метрика E51B — `e2e_trace_success_rate`
  (held-out + challenge), требование 6/6 (5/6 не принимается, данные крошечные) —
  образец строгого gate для обучаемого графа.
- **Teacher-provenance.** В этих core-docs/экспериментах teacher-provenance
  (teacher_access_mode, model_id, license, closed_proprietary_model) НЕ покрыт.
  E51B явно запрещает звонок внешнему поставщику в v1 (симулированный ответ для
  knowledge-edge). Это пробел, который план графа по DPO обязан закрыть отдельно
  (см. §5 и HOW-TO-USE-SKILLS триггер DPO/distillation teacher provenance).

---

## 4. Классификация (уровень ILearningMachine vs детерминистический хелпер)

Ссылки на правила: HOW-TO-USE-SKILLS.md строки 327-347 (триггеры
trainable-architecture-decision); experiments-v2-runners.md §"Anti-false-done и
anti-determinism"; CLAUDE.md «Allowed trainable architecture pattern gate».

- **Документы как таковые (MAP, schema, runners.md, планы E51)** — это
  evidence/документация, не код и не уровень иерархии. Классификации уровня не
  требуют; служат источником метрик/находок/контрактов для плана.
- **K-NN (E04/E09/E14/E34V2, `KnnLogicalUnit`, `StructuralKnnPredictor`,
  `BruteForceIndex`)** — по правилам скила это retrieval/nearest-neighbour
  механизм. Сам по себе он **внутренний детерминистический хелпер / память**,
  НЕ архитектурный уровень. В плане графа допустим только как реализация-деталь
  внутри разрешённого уровня (например внутри `ILogicalUnit`), и НЕ может быть
  финальным авторитетом выбора (правило E51B: «KNN = память, арбитры =
  авторитет»; CLAUDE.md: scorer/selector/KNN/Jaccard/BM25 не являются
  architecture patterns).
- **BM25 / route classifier / pattern classifier (E04/E18/E19/E22/E24)** —
  детерминистические scorer/classifier/router. По CLAUDE.md и R4 (No hidden
  determinism) НЕ заменяют обучаемый уровень; в графе — helper/baseline/eval-only,
  если только активная авторитетность не у обучаемого `ILogicalUnit`/
  `ILearningMachine`.
- **DPO-тренер (`InternalTrainReadyDpoRunner` → `trainer.StepMany`,
  policy checkpoints; `03.TransformerModel`, `11.DPO`)** — это **обучающий
  механизм** (кандидат на обвязку обучаемого уровня), но runner сам по себе —
  evidence-only. Чтобы считаться частью проекта как trainable unit, по CLAUDE.md
  «Trainable Unit pattern» нужны: регистрация flow-adapter в `15.OrchestratorModel
  /Logical/`, публикация checkpoint через `25.ModelServer`/`IModelRepository`,
  manifest с checkpoint-ссылками, обнаружение через `IRemoteUnitLocator`,
  runner-report с метриками, и lifecycle (user/cache → repo/common → backup/
  export/import/share → continue-training). Без этого — «не часть проекта».
- **`DefaultLogicalCapabilityTrainer.TrainAndRegisterAsync` +
  topology/flow/unit registries (`InMemoryLogical*Registry`)** — это путь
  обучения+регистрации обучаемых **ILogicalUnit/ILogicalFlow** (уровни 2-3
  иерархии ILearningMachine). Релевантно как существующий механизм, на который
  опирается «teaching» в графе. Но in-memory registry + hard-coded source =
  evidence по конкретному прогону, не доказательство уровня
  `ILearningMachine<TDomain>` (уровень 5) и не runtime продукта.
- **Арбитры E51B (`identity_consistency`, `token_safety`, `verification_order`)
  и E39 QualityOrchestrator / E46 ConfidenceGate** — в эксперименте описаны как
  «simple deterministic judges» (гейты авторитета). Это допустимые
  детерминистические **гейты/арбитры**, НЕ обучаемый уровень; в графе их роль —
  hard-gate authority per-edge, помеченный как deterministic.

Итого для плана графа: единственные кандидаты на уровни иерархии — обучаемые
unit/flow через `DefaultLogicalCapabilityTrainer`/DPO-обвязку с полным
lifecycle; всё остальное (KNN, BM25, classifier, арбитры, runner-ы) — helper /
gate / evidence и не может выдаваться за обученную семантику.

---

## 5. Пробелы / что доисследовать

1. **Teacher-provenance для DPO-графа НЕ покрыт** в этих core-docs (нет полей
   teacher_access_mode/model_id/license/closed_proprietary_model). E51B запрещает
   внешнего поставщика в v1. План DynamicGraph по авто-DPO обязан определить
   источник «учителя»/chosen-rejected и провенанс отдельно. Доисследовать:
   `docs/skills/dpo-triple-format-SKILL.md`,
   `docs/skills/trainable-architecture-decision-SKILL.md`,
   `docs/DECISIONS-LOCKED.md` (A-LM-*).
2. **MAP датирован 2026-05-01; baseline тестов в DYNAMIC-FLOW plan = 877/877**,
   тогда как CLAUDE.md заявляет ≥1488. Возможен дрейф. Доисследовать актуальный
   статус E51A/E51B/E51-DYNAMIC-FLOW (есть ли записанные результаты в
   `experiments-v2/E51*/results/`) — это вне прочитанной области.
3. **Иерархия ILearningMachine vs «ILearningDynamicGraph».** В прочитанных
   core-docs термин `ILearningDynamicGraph` не встречается; иерархия описана как
   Level 5..1 (ILearningMachine → … → ILearningMachinePair) в `00.Core` (CLAUDE.md).
   Доисследовать: `docs/COMPONENT-INTERFACES.md`, `docs/ARCHITECTURE.md §9`,
   `00.Core/LearningMachineContracts.cs` — как DynamicGraph ложится на уровни/как
   именуются узлы-рёбра (отдельная под-задача рекона по коду).
4. **Реальные тела runner-ов и контрактов KNN/DPO** (`KnnLogicalUnit`,
   `DpoTrainer.StepMany`, `DefaultLogicalCapabilityTrainer`) не читались — только
   doc-карта с цитатами. Точные сигнатуры/типизированное состояние/checkpoint-формат
   — отдельная под-задача рекона по коду (`26.TrainableRouter`, `11.DPO`,
   `18.CapabilityTraining`, `35.ExperimentsV2`).
5. **HOW-TO-USE-SKILLS.md прочитан частично** (стр. 1-433 из 782). Триггеры
   классификации и DPO-provenance найдены; остальная половина (детальные gate-ы)
   не вычитана — при необходимости доисследовать вторую половину файла.
