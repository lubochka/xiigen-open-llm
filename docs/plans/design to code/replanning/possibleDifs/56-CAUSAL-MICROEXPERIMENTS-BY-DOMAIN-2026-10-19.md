# N56 — малые причинные эксперименты по доменам, независимым получателям и состоянию до/после

**Дата:** 2026-10-19 · `EXPERIMENT_DESIGN_ONLY / NATIVE_NEW_RUNS_NOT_PERFORMED`. Существующие успешные 18-Oct receipts **учитываются отдельно**. Защищённые программы `llm_mvp_core/docs/plans/replanning-possible-diffs/**` только читаем, не изменяем. Этот документ — полное bounded дополнение под новые seam, а **не команда запускать всё**.

## Общая приёмка — обязательна для каждого отдельного опыта

Человеческий вопрос; **предметная тема** (домен); конкретный язык/направление/задача; именованный обучаемый recipient; реальный consumer; одна меняемая переменная; frozen source+binary+model/tokenizer/seed; pre-registered 2–4 TRAIN и 4–8 ранее не виденных EXAM; gold независимо от chosen и model; A0, A1, B0, B1; actual update и state hash; реальные outputs всех четырёх состояний; время выполнения, train/response operations, RAM/VRAM, размер checkpoints, total/trainable tensors и Unit/Flow levels, если измеримы. Если нет recipient/consumer, **`NOT_RUN / MISSING_CALLER`**, не нулевое качество.

Основная оценка `Q = correct semantic obligations / applicable obligations`; дополнительно `whole-correct / N`, критические нарушения (отрицание, роли, нарушенная структура), отказ, retained старые навыки, response token budget, latency. Каждая panel и её веса фиксируются **до** обучения. `G_A=Q(A1)-Q(A0)`, `G_B=Q(B1)-Q(B0)`, `ΔG=G_B-G_A` уместно только с одинаковым gold, входом, consumer/scorer и явно сопоставимыми arms. Не считать изменение loss, DPO margin, KNN hit count или checkpoint SHA достаточным успехом обучения.

Нативный вход/выход всегда сохранять на диске с записью `core_head, planning_head, actual_binary_hash, input_sha, train_exams_sha, before_after_state_hash, caller, seed, raw_prompts, raw_answers, score_by_obligation, runtime_cost, independent_review, status`. Для источника, расходящегося между ветками, вывод о регрессии допустим только после изоляции source-diff при прочих условиях одинаковых.

## X56-T1 · Перевод EN→RU: восстановить реальный успешный memory, проверить перенос через cold restore

**Вопрос:** выигрывает ли ordinary Executor от typed learned phrase/word/KNN, а не от заранее загруженной словарной подсказки?

**Основание:** [native P125](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/README.md) достиг 0/24→19/24; P25 сохранил 19/24. Сначала по SHA восстановить их checkpoint, не пересоздавать «похожее» обучение.

- **Recipient:** `TranslationMachine.TrainStructuredBatchAsync` → `ExecutorUnit.TrainOnExample` → exact/phrase/word/KNN memory.
- **Consumer:** тот же `GetOrchestratorAsync().RunAsync` в ordinary EN→RU Flow; отдельный cold restored process с этим же Flow.
- **TRAIN:** в качестве малого отличающего пилота две осмысленные пары с явными typed mappings, например `validate the file`→`проверь файл`, `delete the cache`→`удали кэш`; это **новый проект набора**, не утверждение о готовых legal pairs.
- **EXAM (до просмотра ответов):** `delete the file`, `validate the cache`, одно отрицание `do not delete the file`, перестановка роли `the file deletes the cache`. Допустимые переводы/допуски фиксирует независимая рубрика, строки обучения в EXAM не включаются.
- **A:** тот же checkpoint без новых structured lessons (контроль).
- **B:** его независимая копия + точно два разрешённых typed lessons. Остальные map seed, tokenizer, decoder, KNN topK/distance неизменны.
- **Budget:** 2 lessons, 4 EXAM × (A0/A1/B0/B1) = до16 ordinary responses; +4 B1 cold-process answers только на дополнительно не входящих в train EXAM. Отдельно record `UnitId`, mappings/hash count, selected candidate, acceptance/rejection и final text.
- **Primary:** (ΔG) по четырём смысловым требованиям, `whole-correct /4`, `critical_wrong /4`; secondary cold parity 4/4 exact response/state (не значимое обучение после рестарта), elapsed/RAM. `PASS_LOCAL` только при лучшем предметном ответе или полезной стоимости с явными tradeoffs, а не факте вставки словаря. EN→HE/ES, RU→EN — отдельные независимые профили, не вывод из EN→RU.

**Контроль против ложного KNN успеха:** обнулить обучаемую память в одной *только отдельной* диагностической копии, не менять scoring, сравнить exact/near/composition и абстенцию; не приписывать одно изменение всем KNN-функциям. **Статус нового X56:** NOT_RUN, сохранённый donor P125 already native.

## X56-N1 · Документация C#: DPO/LoRA действительно меняет объяснение порядка аргументов?

**Основание:** [protected small D](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md), [RP01 existing CLI source-map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md), [native trainer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L103-L170).

**Открытый реальный blocker:** текущий runner принимает калибровку1/пакет500, **двухпарного `micro-paired` admit/CLI нет**. Создание его — отдельная короткая wiring задача, не разрешение ослабить governance, прогнать 500 копий пар или подменить Student teacher. До неё статус `MISSING_BOUND_TWO_PAIR_CALLER`.

**Recipient:** фактические project-LoRA `LoraLinear` A/B weights, AdamW state. **Consumer:** `CodeDocumentationStudentGeneration.Generate` с тем же decoder/seed+settings, независимо frozen reference and saved control. **TRAIN** (ровно два разных закона, без EXAM answers): `Subtract(a,b)` means `a-b` against wrong `b-a`; `Validate();Save();` documents validate-before-save against swapped step; обе пары должны быть допустимыми provenance guards, не synthetic mixing hidden teacher. **EXAM**: `Subtract(total,discount)`, `Subtract(left,right)`, `ValidateProfile();SaveProfile();`, `ValidateCache();PersistCache();` — четыре новых source families, gold фиксируется по C# semantics.

**A:** frozen B0 Student, evaluation before and after sham no-update. **B:** same B0 with 2 actual DPO updates, same policy/reference/tokenizer/LoRA topology and exact masks. Один фактор — применение обучения; если исследуют different rank/β, нужен *другой* A/B. Не делать greedy-decoder отличием между arms.

**Primary:** count of four independent descriptions respecting operand/order and no reversed law, `ΔG`; secondary signed chosen/rejected logprobs, parameter delta per named LoRA tensor, optimizer actual steps, prompt sensitivity (counterfactual source), time/update+RAM. Отдельно проверить former learned tasks для forgetting. Один `margin>0` без source-conditioned Student output ≠ gain. `B` 0/4→0/4 — честный отрицательный native результат, а не автоматический отказ от всей LoRA. **Статус:** DESIGN_ONLY / MISSING_CALLER / NOT_RUN.

## X56-C1 · Язык программирования C#: синтаксическая роль как доменно-специфический признак kNN

**Основание:** [protected small C](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/02-programming.md); `StructuralKnnPredictor` использует `SyntacticRole` при distance, facade требует **>=10 train pairs**, поэтому «2 произвольных pair» может не достичь текущего consumer.

**Recipient:** `StructuralKnnPredictor._training`; **consumer:** `PredictStructuralNextTokens` в C# `SemanticCompilerFacade`, не свободная генерация кода. **Input:** 10 lawful AST transitions (из двух исходных корректных C# семейств) с annotated operand role, 4 новых independent AST transitions в другой вложенности, роли gold из Roslyn AST.

**A:** текущий fixed-distance без роли через явно выбранный controlled ablation; **B:** тот же decoder/pairs/k/kNN и одна `SyntacticRole` feature enabled. Код изменения только feature/distance, не train corpus, family split, parser или `NodeTypeId % 200` одновременно. Два варианта должны иметь тот же source→feature preprocessing, кроме данного feature. **До/после:** пустая/обученная память A0/A1/B0/B1, 4*4 structural predictions; macro accuracy по AST families, роль operand/next-kind и ambiguity collisions отдельно. Измерить actual candidate vote IDs, latency, rows/memory.

**Не переносить как универсальный выигрыш:** AST role feature уместна для структурного C#, но для естественного перевода вместо Roslyn требуют typed word order/subject/object и свои gold. На SQL используется собственный parser и schema — нельзя автоматически включить AST feature. **Статус:** DESIGN_ONLY / native fixture NOT_RUN.

## X56-T2 · Перевод: неверный evaluator vs неправильное обучение — две разные гипотезы

**Это повторяемый prior art N55, а не новое открытие N56.** [Source N55](55-NOVEL-DIRECTION-EVAL-AND-METRIC-STATE-2026-10-19.md), [protected 17 additions](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/17-additions-direction-evaluation-and-policy.md).

**Recipient меняется?** Нет, frozen checkpoint, 0 updates. **Consumer:** actual `TranslationMachine.EvaluateAsync` and ordinary Flow. **A:** helper всегда `ru/en/general`; **B:** typed source/target из independently frozen row, остальные параметры идентичны. Четыре примера EN→RU и RU→EN (открытое/закрытое с отрицанием), по2 на направление; holdout не входит в train. Два Evaluate wrapper calls дают8 nested ordinary Flow responses; 4 direct typed ordinary Flow control calls; итого **12 реальных переводов, не 12 +2 «wrapper»**. Поля `request_direction, actual_direction, UnitId, raw_text, ordinal_score, semantic_score, old/new checkpoint hash`. **Гипотеза опровергается** если production RP01 scorer не вызывает этот helper; ошибочный маршрут тогда всё ещё source seam, но не объясняет neural RP01 degradation. Typed per-item caller **ещё не реализован**. **Статус:** PROPOSED / MISSING_TYPED_CALLER / NOT_RUN.

## X56-T3 · Перевод: `PromotedUnitIds` vs реальная серверная публикация learned state (НОВАЯ узкая гипотеза)

[Прямой source/новизна N56-M3](56-NEW-METRIC-PROMOTION-SEAMS-2026-10-19.md).

**Домен:** перевод, EN→RU; **recipients:** Executor learned memory → `25.ModelServer` accepted deployment (если caller существует). **A:** train2 в private `TranslationMachine`, затем ordinary in-process answer, **без подтверждённой server publish stage**. **B:** абсолютно те же source learned weights, sanctioned export/publish/registry switch с подписанным receipt (это исследуемый фактор). **EXAM:** 4 независимых перевода и независимая rubric, до/после развертывания, ин-процесс vs **fresh process restored deployed**. Score = correct/4, lost-skill count, serving checkpoint SHA vs private trained SHA, publisher acknowledged bool, steps and full cost. Отдельный контроль: clean B0 served pointer вообще не меняется. Не объявлять B реализованным без найденного API публикации. **Статус:** SOURCE_CONFIRMED_METHOD_ONLY / PRODUCTION_CALLER_OPEN / NOT_RUN.

## X56-Q1 и X56-Q2 · Валидность стоимости (0 neural updates), один показатель за опыт

В новой [N56 metric-delta](56-NEW-METRIC-PROMOTION-SEAMS-2026-10-19.md) приведены обе formulas. **Q1** одна native training request, не обучающий A/B: compare returned `CpuMs` to independent `Stopwatch` + `Process.TotalProcessorTime` deltas, raw ms/calls; ответ модели не меняется; `wall≠CPU` фиксируется. **Q2** тот же canonical workload: reported `PeakRamMb` vs sampled actual process `PeakWorkingSet`/managed allocations and optional VRAM, separately, with external sampler. Ни Q1, ни Q2 **не закрывают degradation hypothesis**, если consumer accuracy не сравнили. Минимум одна shadow measurement, no product/learning changes. **Статус:** SOURCE_CONFIRMED_METRIC_SEAM / NATIVE_METER_VALIDATION_NOT_RUN.

## X56-K1 · Перенос «неудачной матрицы» в полезную kNN функцию без смешивания механизмов

**Исторический факт:** в документах E09/E25/E26/E43 и N33–N55 есть успехи отдельных features даже при негативном итоговом experiment. **Проверяемая гипотеза:** часть признаков выгодна как `kNN distance/eligibility/vote`, но не как постоянная обучаемая матрица. **Разбить на две самостоятельные процедуры:**
1. **Программирование/C# AST:** same StructuralKnn training pairs + 4 heldout; A baseline fixed distance, B только исторически полезная роль/структура в kNN distance. Перенос полезности **уже частично покрыт X56-C1** — не повторять дважды.
2. **Перевод:** same typed EN→RU lessons + 4 heldout; A baseline kNN scoring; B только одно свойство `source token order/subject-object/negation`, представленное *в обеих train/query* и привязанное к consumer. Если признаки у tokenizer недоступны, `MISSING_CONSUMER`, не подмена их статическим oracle. Отрицательный контроль: punctuation/case variants и иной домен. Вывод ограничить EN→RU, не обобщать на code AST/SQL.

Оба эксперимента pre-register: `correct/4, critical-role/inversion-errors, topK selection, latency, RAM`; 0 новых neural updates, отдельный memory Train. Нельзя менять одновременно vectorizer, LoRA rank, distance и activation, а также выдавать прежние E26 `BuildDefault` 40→100% за усвоение двух уроков. **Статус:** DESIGN_ONLY.

## X56-X1 · Переход от локальной победы к многодоменной устойчивости (не gate для малых опытов)

После **хотя бы одного** положительного актуального local A/B закрепить точные `checkpoint/Flow` identity и проверить прежние навыки C# AST, перевод EN→RU, документацию, оркестрацию **каждый на своём настоящем получателе**, где consumer существует. Независимые 4-case панели *по профилю*, family-disjoint training/exam; фиксировать `retained correct, newly learned correct, interference/critical wrong, resource cost` до/после. **Не складывать** accuracy C# AST kind prediction, neural text, memory exact translation и planner mask в один процент. Профиль без consumer записать `NOT_MEASURED`, не 0. Удешевить до minimum meaningful, повышать panel только после наблюдаемого delta/неопределённости.

## Шаблон возвращаемого пакета исполнителя

```yaml
experiment_id: X56-...
scope: domain / profile / recipient / consumer
status: DESIGNED | BLOCKED_MISSING_CALLER | NATIVE_RUN | REVIEW_ACCEPT | NO_GAIN | INCONCLUSIVE
pin: { core_sha: ..., planning_sha: ..., executable_sha: ..., checkpoint_sha: ... }
hypothesis_one_factor: ...
train: { ids: [], count: 0, hash: ... }
exam: { ids: [], count: 0, rubric_precommitted_sha: ... }
A0: { raw_results_path: ..., quality: ..., learner_state_sha: ... }
A1: { raw_results_path: ..., quality: ..., learner_state_sha: ... }
B0: { raw_results_path: ..., quality: ..., learner_state_sha: ... }
B1: { raw_results_path: ..., quality: ..., learner_state_sha: ... }
actual_updates: { A: 0, B: 0, named_parameter_or_memory_deltas: ... }
receipt: { caller_trace: ..., consumer_trace: ..., seed: ..., runtime_seconds: ..., cpu_seconds: ..., ram_peak_mib: ..., tokens_per_second: ..., trainable_weights: ..., unit_flow_levels: ... }
analysis: { acquired: ..., lost: ..., abstained: ..., fatal_semantic_errors: ..., changed_other_factors: [], native_or_proxy: ... }
review: { reviewer: ..., independent_reproduction: ..., accept_or_reject: ..., limits: ... }
```

**GOOD:** «После двух native memory lessons обычный EN→RU Flow на новых case IDs: 1/4→3/4; cold consumer тоже3/4; no unintended negation flips; trace показывает нужный Executor и stored SHA».

**BAD:** «DPO loss уменьшился / изменились веса / kNN вернул соседа / обещано 95%, следовательно всё обучение восстановлено».

**STOP / falsification:** Точный путь не обучает нужную единицу, авторская тестовая разметка попала в Student input, after scoring на старом/другом consumer, failed-to-load checkpoint, разные checkpoint roots или задача не имеет независимого gold. Прекратить приписывать такой run quality-delta, записать причину и минимальную поправку к caller. Иные законные остановки safety/governance сохраняются; этот экспериментальный документ их не отменяет.

[Базовый synthesis](56-INDEPENDENT-LEARNING-GOLD-AND-RP01-AUDIT-2026-10-19.md) · [метрические гипотезы](56-NEW-METRIC-PROMOTION-SEAMS-2026-10-19.md) · [read-only coverage](56-PROTECTED-EXPERIMENT-VALIDITY-2026-10-19.md).
