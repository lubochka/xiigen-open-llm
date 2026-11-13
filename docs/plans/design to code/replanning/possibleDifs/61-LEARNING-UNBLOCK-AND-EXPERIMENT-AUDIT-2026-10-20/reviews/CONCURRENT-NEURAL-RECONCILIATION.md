# Сверка новых параллельных neural-пакетов

20.10.2026. Planning `fb9aadf69e0669d36b820d61f76fe3ecc8697a7d`; последняя core-дельта `a8222d1b15745fe6207f36cd294fd3fb61c01d3b` меняет только protected документы. **READ_ONLY_RECONCILIATION; новые model/build/train/scorer calls — 0.**

## Результат для новизны

| Наш или входящий предмет | Новое совпадение | Итоговая классификация |
|---|---|---|
| **Наш N-HEAD-01: selected LM-head rows после полного Transformer** | В новых neural MD нет сокращения выходной проекции по scored positions. Incoming PAD меняет полную вычисляемую длину Transformer; LC меняет reuse reference scores между шагами. | **Остаётся NEW_WITHIN_REVIEWED_CORPUS**, с расширением поиска до этой параллельной дельты; не мировая novelty и не измеренный speedup. |
| **Наш N61-RESULT: точный `!Any(adapter hash changed)` в owner** | Ни exception `SIMULATION_DPO_EFFECT_OR_READBACK_INVALID`, ни конкретный final guard/neutral-status split в новых 46 changed-file patches не найдены. У incoming LC есть общее требование изменённого LoRA, но оно не анализирует этот guard. | **Остаётся новой конкретной находкой owner**, связанной с давно известным правилом допустимого нулевого эффекта. Не причина старой RP01 accuracy. |
| **Наш PAD blueprint** | Incoming [03-NEURAL][paired] описывает тот же N60/23 PAD1/PAD2,2 шага на arm, 4 updates, 0 Generate и valid IDs+mask после full validation. | **EXACT SAME FACTOR / RELATED IMPLEMENTATION REFINEMENT.** Не считать ещё одним новым опытом или новым механизмом. |
| **Incoming LC-REF-CACHE** | [Reference-score reuse][cache] кеширует две immutable reference log-probabilities на пару между повторными actual updates. Это не selected head, PAD, KV decoding cache, P09, P105 или BM25 P115. | **Отдельная опубликованная другим исполнителем гипотеза. Добавить crosswalk с авторством источника; не присваивать нашему аудиту.** |
| **Incoming paired-study anti-replay** | Один mode+TRAIN hash блокирует второе arm независимо от RunId. Incoming предлагает общий study ID с двумя заранее связанными arm journals. Наш PAD уже разделяет два фиксированных modes, сохраняя markers. | **Совпадающая граница, более подробный дизайн общей identity**. Дополнение к подготовке caller, не новая причина деградации и не самостоятельный обучающий A/B. |
| **Incoming FP32 cancellation → double totals** | Incoming03 явно ссылается на старый PG-09 и требует сохранять double через subtraction, а не только sum. | **KNOWN / REFINEMENT**, не новый номер. Два коротких PAD урока не гарантируют нужного contrast. |

## Что добавить в protected crosswalk, не меняя protected папку

Предлагаемая строка: **`LC-REF-CACHE → отдельной прямой protected карточки в проверенном срезе не найдено → supplemental possibleDifs design → PREPARABLE / NOT_RUN`**. Ближайшие P09/P105/P115 и активные D04/23 — related, не exact coverage. Новая core-дельта содержит общие правила исполнения, а не реализацию такого cache factor.

Реальный повторяющий caller проверен независимо через GitHub на исходном core `bf9b543`: `18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1347` сохраняет `batches=executionBatches`; цикл1352 и `trainer.StepVector(batches,step)`1378 используют их повторно. Request465 default MaxStepCount1, пределы586–588 от1 до5; `windowSteps=3` при двух шагах не достигает convergence check1388. Это действительный learning path. В current DPO на879/882 reference вызывается заново; результаты входят в reward/margin и backward.

**У LC своё расписание:** обе пары на каждом из двух шагов,2 updates / 4 presentations на arm. Для A ожидается 8 training reference forwards, для B 4; оба имеют 8 policy forwards и 8 backward. A+B — 4 updates, 8 pair presentations, 16 policy forwards, 16 backwards, 12 training reference forwards при ожидаемых hits. Это статический счёт, не измерение. Incoming также явно назначает 4 `EvaluateMany` до/после на этих 2 парах, дополнительно 16 reference + 16 policy forwards вне training timer. Их нельзя скрыть в PAD budget, где Evaluate 0.

**Главное ограничение:** current RP01 main500 предъявляет уникальные строки один раз, поэтому reuse не обещает экономию там; при отсутствии hits возможны только накладные расходы. Нельзя выдавать LC за исправление уже завершённого one-pass main500. Механизм не использует AST/word-order эвристику и не доказывает свободное владение языком.

**Уточнение к incoming LC перед исполнением:** source01 шаг 4 требует «Показать, что actual LoRA изменилась», а prompt03 связывает валидность с «сохранённым изменённым LoRA состоянием». Это следует читать/исправить как **измерить actual ΔLoRA, включая честный ноль**, при подтверждённом выполнении обязательных градиентных операций и optimizer. Ненулевая дельта не обязательна для ресурсного успеха. Отсутствующий gradient edge или пропущенный backward не становится допустимым zero-effect. Наш конкретный N61-RESULT guard остаётся отдельным source finding.

## Что принять из incoming03 как уточнение подготовки

Общий study manifest полезно связать с factor, source, TRAIN, config/tokenizer, B0/reference, initial optimizer settings/state/RNG и отображением arm→build/treatment. Нынешний анти-replay нельзя обойти новым RunId, сменой source-строки или удалением marker. Наши два строгих modes уже разделяют arms; общий manifest добавляет явное доказательство, что они составляют заранее объявленный один A/B. Этот вариант требует проектирования копии owner, не ослабления существующего admission.

Incoming optional scoring дает точную нормировку: primary heldout NLL — равновесное по четырём случаям среднее `−chosenMean`; рядом token counts, reconstructed sum, token-weighted NLL и normalized margin. Это полезное уточнение **только явно выбранного расширения**. Суммы из mean×count не независимая high-precision accumulation; current StepVector metrics вычислены до optimizer и не являются final-state evaluation. В основной ресурсный PAD/HEAD scoring не добавлять.

## Последнее правило исполнения имеет приоритет

Новая шапка на core `a822…` требует сначала точную экспериментальную копию исходного механизма в 34.Experiments/35.ExperimentsV2 и ограниченный runner в `experiments-v2/RP01-MICRO/<ID>/`; original code read-only. Старые слова incoming и наших черновиков «изменить original» теперь означают адрес будущего переноса после результата и отдельного решения Любы. Никаких новых blanket approvals, fake source copies или второго имитирующего learner. В текущем аудите остаются только документы; protected output paths не становятся разрешением писать в защищённую папку вопреки прямому текущему запрету пользователя.

## Граница и доказательства сверки

Из `audit_state/planning-final-concurrent-compare.json` извлечены 7 полных добавленных объектов указанного neural scope: 4 Markdown, selection JSON, 2 fixture JSONL; каждый восстановленный текст сверился с Git blob SHA. Это read-only source mirrors уже опубликованных материалов, не созданные runtime datasets. Scoped reading охватило механизмы, counters, ready-state и executor contracts. По всем 46 changed-file patches дополнительно выполнен поиск точных guard/status и selected-head терминов. Это не новое полное чтение всего планирования; raw score/model execution не выполнялся.

[cache]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/dpo/01-reference-score-reuse.md
[prompt]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/dpo/03-executor-prompt.md
[reconcile]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/dpo/04-current-head-reconciliation.md
[paired]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-FIRST-AUDIT-2026-10-20/03-NEURAL-LEARNING-AND-MICRO-CALLER.md
