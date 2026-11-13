# Ф-02 · E35/E36: два одноимённых seed превратили validation в отбор по классу; E35 «KNN» не обучался

**16 октября 2026 года. SOURCE_CONFIRMED + ARCHIVE_CONFIRMED + PYTHON_PROXY_CONFIRMED; NATIVE_DOTNET_NOT_RUN; INDEPENDENT_CODEX_REVIEW_PENDING.** Core audited `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`. Это новое дополнение к [10 — E40V2 исторический перенос](10-historical-function-transfer-and-learning.md) и [12 — текущие BPE/KNN gaps](12-bpe-retrain-identity-and-consumer-causal-gap.md), а не замена или правка старых receipts.

Полный адресный отчёт для RAG/planning: [23-NEW-E35-E36-COUPLED-RNG-AND-FALSE-KNN-PASS](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/23-NEW-E35-E36-COUPLED-RNG-AND-FALSE-KNN-PASS-2026-10-16.md> "Исторический ref llm_plan3; локальный файл может отличаться"). Выполняемый контролируемый [Python-proxy](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/23-E35-E36-COUPLED-SEED-SPLIT-PROXY.py> "Исторический ref llm_plan3; локальный файл может отличаться"). Он не заменяет native case trace.

## Структурная причина и почему это не просто «малый validation»

В [E35.Run:18–32](<C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E35DependencyDetector.cs:18> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L18-L32; локальный файл может отличаться"):
- dataset generation: `GenerateDependencyPairs(seed:42)`;
- splitting: **новый** `Random(42)`, `shuffled=pairs.OrderBy(_ => rng.Next())`, последние 20% идут в val.

В [E35.GenerateDependencyPairs:72–103](<C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E35DependencyDetector.cs:72> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L72-L103; локальный файл может отличаться") генератор на каждую строку делает один `rng.Next(14)`, где template indices 0..7 последовательно класс SEQUENTIAL, 8..13 класс PARALLEL.

Следовательно, оба `Random(42)` получают на строке i одинаковый исходный PRNG draw: `Next(14)` выбирает template index, `Next()` возвращает базовый величинный sort key. Больший ключ означает больший template index. Отбор top20% по sort key означает целенаправленную выборку верхних шаблонов, которые сгруппированы по одному классу. Состав классов не независим от split, даже если исходный код выглядит как случайное перемешивание.

[E36.Run:20–34](<C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs:20> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L20-L34; локальный файл может отличаться") повторяет тот же паттерн. [E36.GeneratedTemplates:80–113](<C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs:80> "Исторический ref 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, L80-L113; локальный файл может отличаться") упорядочены по целевому компоненту: 32 (4), 33 (3), 34 (3), 35 (3), 37 (3), поэтому val отбирает преимущественно последние две категории.

СОГЛАСОВАНИЕ С ИСХОДНЫМИ АРХИВНЫМИ ЧИСЛАМИ:

| Исторический файл | val и состав | Baseline | Enhanced | записанный verdict |
| --- | --- | ---: | ---: | --- |
| [E35 Tier1](<C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E35-DEPENDENCY-DETECTOR/results/run-2026-04-30-001.json>) | 10/10 PARALLEL | 0% | 10% | PASSED |
| [E35 Tier2](<C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E35-DEPENDENCY-DETECTOR/results/run-2026-04-30-002.json>) | 20/20 PARALLEL | 0% | 30% | PASSED |
| [E36 Tier1](<C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E36-FEEDBACK-CORRECTOR/results/run-2026-04-30-001.json>) | 8/8, только две категории | 0% | 25% | PASSED |
| [E36 Tier2](<C:/Projects/llm_mvp_core/35.ExperimentsV2/results/E36-FEEDBACK-CORRECTOR/results/run-2026-05-01-002.json>) | 16/16, только две категории | 0% | 0% | FAILED |

Локально **выполненный Python-proxy** классической seeded System.Random последовательности построил: E35 Tier1 PARALLEL10, Tier2 PARALLEL20; E36 Tier1 component35×4+component37×4, Tier2 component35×11+component37×5. Эти цифры совместимы с сохранёнными `parallels` и `component_coverage=.4`, но **native .NET replay E36 per-row ещё не выполнен**. Точное утверждение: исходник и архив показывают связанный split и искусственный class skew; causal remap качества после исправления split требует отдельного запуска.

## Ещё одна новая причина ложной интерпретации: E35 не является KNN-learner

[E35 header](<C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E35DependencyDetector.cs>) объявляет `Enhanced: K-NN on predicate overlap + produces/consumes signal`. Но [`IsSequential(a,b)`](<C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E35DependencyDetector.cs>) — чистая функция `CollapseString + (CREATE/UPDATE && GET) || overlap>=2`, не имеющая train set, neighbor index, state update или learned label. Enhanced score считает лишь эту deterministic heuristic на `val`. Сохранённый `PASSED` поэтому не свидетельствует обучения и не может служить baseline «раньше KNN учился на E35».

Наоборот, [E36 Train+top3](<C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E36FeedbackCorrector.cs>) действительно создаёт `trainK` из feedback/TargetComponent labels, считает `TagDist`, выбирает `Take(3)` и делает majority vote. Это реальная **train-example-dependent** процедура, но её бывший `PASSED` узкой панели нельзя переносить на новые корректировки или текущий generic KNN. Нынешний [`KnnLogicalUnit`](<C:/Projects/llm_mvp_core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs>) при `k=3` берёт только `hits[0]`, а не E36 majority vote. Точное происхождение алгоритма между версиями **USER_REPORTED / LINEAGE_UNPROVEN** — требуется Git/consumer trace; не объявлять совпадение имён доказанным port.

## MICRO-E35-E36-SPLIT-01 — ближайший дешёвый эксперимент

**Цель:** отдельно доказать, что исторический verdict вызван biased split, и проверить, сохраняется ли какая-либо полезная learned KNN способность E36 после исправления отбора.

**Input:** те же 14 шаблонов E35 и 16 E36, тот же generator seed42, непоменянный код `IsSequential/TagDist`, k3, classes и gold. **A:** исторический split с seed42 для sorting; **B:** independent shuffle seed43, зафиксирован до просмотра результатов; **C:** stratified fixed validation по всем меткам. Для честной проверки обобщения отдельный `D`: disjoint template families/synonyms, не используемые при подборе кандидатов; это другой вопрос, не смешивать в одновременной A/B.

**L0, no training changes:** native .NET вывести для A/B/C `template-id, source-class, shuffle-key, train/val flag`, source/code SHA. Требовать:
- A воспроизводит `E35 val all PARALLEL` и `E36 only components 35/37`; иначе объявить `PROXY_NATIVE_MISMATCH`, не строить выводы на Python.
- B/C имеют SEQUENTIAL/PARALLEL для E35, все пять labels для E36; если нет, split invalid.
- Для E35 обучающий corpus не участвует в `IsSequential` даже при shuffle/удалении train rows.
- Для E36 отрицательный контроль `shuffle trained labels while frozen tags` должен менять predictions; иначе скрытое deterministic совпадение.

**L1, paired evaluation:** baseline/enhanced на **одних и тех же B/C queries**. Метрики E35 `balanced_acc, per-class recall, false_PARALLEL critical scheduling`; E36 `5-class confusion, macro-F1, per-class recall, train-match/source-family leakage`. Учитывать всего 14/16 уникальных шаблонов, не заявлять широкую generalization. Явно различать `learning_from_labels` и `hardcoded feature heuristic`. Никогда не использовать архивные 0→30% как выигрыш калиброванной нейросети RP01.

**Decision:** если B/C ломают старый PASS, маркировать `HISTORICAL_GATE_INVALID_FOR_LEARNING`, не стирать архив. Если E36 сохраняет положительный paired effect, это правомерный кандидат для отдельного learned Unit/KNN consumer. Если E35 heuristic помогает, сохранить как deterministic feature/rule с законной authority, **не** маркировать обучаемым KNN. Если quality на C неопределенна — `UNDERPOWERED`, готовить Tier2 source-family experiment, не многодневные CI.

## Код, план, prompt и человеческая роль

Сначала прочитать `docs/skills/architect-behavior-classifier-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `generated-code-review-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md` и `conversation-intent-and-self-reflection-SKILL.md`. Разрешён один isolated read-only/test-only runner; запрещена правка historical receipts, frozen RP01/LoRA, ProductHost и CI. Публикация результата как `ACCEPT` только после независимой проверки exact SHA, команды/exit-code и blinded semantic gold.

**Человеческую роль (выбор задач, обучение/quality DPO суд) выполняет CODEX с Astra 6 UltraCode. Внешняя открытая разрешённая китайская модель авторствует обе DPO стороны — chosen и rejected. Оценка их качества — также CODEX/Astra.** Этот микроопыт не создаёт новых DPO, поэтому teacher не вызывается. CODEX оценивает class-labeled gold и источник результата. Во время текущего документарного аудита CODEX/Astra не вызывались и native тесты не запускались.

**Return packet:** `claim_scope, evidence_scope, source_sha, fixture_sha, same-panel case_ids, per-class distributions, baseline_enhanced_correct, false_PARALLEL, E36 confusion, negative_label_control, test_command, native_exit_status, quality_change_after_split, independent_review, unresolved_causality, verdict`. Документ готов как **DRAFT**, не как подтверждённое решение RP01.
