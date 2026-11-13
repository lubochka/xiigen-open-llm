# N33 — два новых узких falsifier: статистика Unit-экзамена и договор длины ответа Student

**2026-10-16.** `SOURCE-CONFIRMED CONTRACT SEAMS` / `CAUSAL IMPACT NOT PROVEN` / `NATIVE TEST NOT RUN` / `NO NEW MODEL TRAINING`. Current primary C# source SHA: [32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7](https://github.com/lubochka/llm_mvp_core/tree/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7). Protected core docs READ ONLY; new material lives only in planning/possibleDifs.

## Короткое объяснение

Оценка «насколько хорошо обучилась система» сегодня может менять смысл, когда переходим от **балла за Unit** к **доле правильно отвеченных пользовательских запросов** или от **сырого Student decoder** к **сервису с ограничением длины ответа**. При этом веса модели и выученные словари могут остаться абсолютно прежними. Если сравнить показатели с различными знаменателями или разной политикой выдачи, полезное обучение будет выглядеть как провал — либо наоборот.

Это **новые N33 узкие проверяемые расхождения в процедуре измерения**, а не утверждение о повреждении DPO-градиента. Они не объясняют автоматически исторические `RP01 C#0/3` и `ENRU0/4`, если в конкретном frozen экзамене не использовались соответствующие пути.

## N33-A: macro mean по UnitId вместо request-weighted accuracy

### Source observation

- [TranslationMachine.cs L124–145](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/27.TranslationDomain/TranslationMachine.cs#L124-L145): после структурного обучения при наличии held-out записывается `accuracyAfter = MeasureHeldOutAccuracy(heldOut)`. То есть это может стать сохранённым `_accuracyAfterLastBatch` и полем `LearningBatchResult.AccuracyAfter`.
- [TranslationMachine.cs L662–672](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/27.TranslationDomain/TranslationMachine.cs#L662-L672):

    `heldOut.GroupBy(UnitId).Select(group => trainer.MeasureAccuracy(group)).Average()`.

- [TranslationTrainedUnits.cs L461–490](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/27.TranslationDomain/TranslationTrainedUnits.cs#L461-L490): Executor `MeasureAccuracy` считает количество точных совпадений среди held-out своего Unit. Другие Unit имеют отдельные реализации/метрики, а не обязательно ту же смысловую задачу.
- [TranslationMachine.cs L148–200](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/27.TranslationDomain/TranslationMachine.cs#L148-L200): отдельный `EvaluateAsync` оценивает реальные `EvaluationItems` по строгому сравнению ответа и ожидаемой строки. Этот путь **другой**; direction в нём RU→EN hardcoded, что уже задокументировано другим агентом. Не отождествлять две метрики.

### New quantified counterexample (derived from source; NOT a measured run)

Дано 10 held-out случаев с корректной независимой разметкой: Unit X = 1 пример и 1 правильный, Unit Y = 9 примеров и 0 правильных.

| Indicator | Calculation | Value |
|---|---|---|
| Existing `MeasureHeldOutAccuracy` | `mean(1/1, 0/9)` | **0.50** |
| Request-weighted accuracy | `(1+0)/(1+9)` | **0.10** |
| Wrong requests / user impact | `9/10` | **90% wrong** |

Оба числа арифметически честные, но описывают **разные вопросы**. Если принять `0.50` за request-level teaching success, девять неправильных ответов будут скрыты. Обратная ситуация тоже возможна: плохо работающий редкий диагностический Unit снижает макро-балл, хотя большинство живых пользовательских запросов правильны.

**Conditional impact hypothesis N33-A1:** выбор checkpoint/скейлинга или анализ «новое обучение ухудшило/улучшило перевод» может зависеть от макро-среднего, а не от реальной доли правильных переводов. Для причинности надо найти actual caller, который принимает/отклоняет модель по `AccuracyAfter`. Если такого caller нет, status downgrade до `MEASUREMENT_ONLY`, а не product regression.

**Conditional impact hypothesis N33-A2:** метрики разных Unit могут семантически несовместимы (проверка RuleTag, точный перевод и обнаружение плохого кандидата) — даже `micro` смешивать их в один accuracy некорректно. Самостоятельный реальный экзамен Flow является отдельным обязательным верхним уровнем. N27 уже выявил negative-only Verifier и языковой bias; N33 фокусируется **не на том, что label неправильный, а на взвешивании экзаменационных заданий**.

**What is NOT novel:** `MeasureUnitCoverage` без held-out жёстко предпочитает RU_EN, и `EvaluateAsync` hardcodes RU_EN; это уже установлено в [N26 role/direction metric](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md). N33 не переименовывает этот известный баг.

### Domain specificity

Особенно важно для multi-unit translation RU/EN/HE/ES с неравными объёмами примеров, графов и разных типов ошибок; для единственного Unit с единым examiner macro и micro совпадают. C# AST KNN `E09` и RP01 direct TextGenerator **не вызывают этот метод в показанных training chains**. Таким образом это не заявленная общая причина `0/3`.

## N33-B: два потребителя одного Student не одинаково реализуют MaxAnswerChars

### Source observation: no max-character contract in raw RP01 Evaluate consumer

1. [CodeDocumentationStudentGeneration.cs L12–13, 43–75](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L12-L75) включает `MaxAnswerChars` в `CodeDocumentationStudentGenerationSettings` и в `SettingsSha256` через сериализацию полного record. В `Generate` проверяется только `MaxAnswerChars > 0`, после чего вызывается `TextGenerator.GenerateDetailed` с `MaxNewTokens`, temperature, TopK/TopP/seed, **без MaxAnswerChars**.
2. [TextGenerator.cs L212–250](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/09.Inference/TextGenerator.cs#L212-L250) заканчивает генерацию по EOS (если он объявлен) либо лимиту токенов. В этом detailed-вызыве `verifiedEosToken:null`; после Decode возвращает исходный полный `generatedText`.
3. [CodeDocumentationSimulationDpoEvaluation.cs L90–97](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs#L90-L97) сохраняет `generation` **непосредственно** от `CodeDocumentationStudentGeneration.Generate`, не через ограничивающий `SavedModelAssessmentService`.
4. Иной consumer: [CodeDocumentationSavedModelAssessmentService.cs L548–577](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs#L548-L577) вызывает тот же генератор, но **после получения текста** отвергает ответ при `answer.Length > settings.MaxAnswerChars`; при включённом capture до отказа записывает полный raw ответ.

**Precisely observed seam:** Смена одного `MaxAnswerChars` при достаточном `MaxNewTokens` может поменять reported `SettingsSha256`, но *не* сырую генерацию в simulation runner. Тот же ответ может считаться «полученным» там и «ошибкой превышения лимита» в assessment service. **Это контрактное отличие consumers, не доказательство того, что любой из них реализован неправильно.** Комментарий самого генератора явно оставляет enforcement потребителю.

### New falsifiable hypotheses

- **N33-B1 / P1 observability:** сравнение двух RP01 free-generation receipts как «different decoder conditions» по `SettingsSha256` вводит в заблуждение, если отличие только `MaxAnswerChars`; декодер не получил изменения и не обязан изменить raw tokens.
- **N33-B2 / P1 conditional quality gate:** при переносе evaluation result из simulation runner в SavedModelAssessment длинный, но семантически правильный вывод может быть отказом `GenerationFailed`, тогда как в другом consumer он попадёт в независимый экзамен. Возможна обратная проблема: длинная бессмысленная выдача не ограничивается simulation budget по символам.
- **N33-B3 / not established:** исторический frozen C# 0/3 может быть связан с этим стыком **только если** конкретные файлы `generation-NNNN.json`, длины output, договор `MaxAnswerChars` и правило рубрики показывают, что проблема была именно в output-boundary. Ныне подтверждения нет. Нельзя менять старые оценки или считать угадывание сокращением «обучением».

### Domain specificity

Только paths через `CodeDocumentationStudentGeneration` и `CodeDocumentationSimulationDpoEvaluation`, в частности C# documentation. ENRU simulation имеет делегат `TranslationGeneration` и требует **отдельной** проверки его реализации; не переносить этот вывод на ENRU автоматически. Ни структурная матрица, ни KNN, ни LoRA optimizer не затронуты.

## Negative evidence, triage and reviewer obligations

- **No generic "LoRA broken" assertion:** current DPO exact path has observed nonzero q/v changes and restored checkpoint. Independent gradient probe remains open (see [protected 41](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/41-current-dpo-gradient-chain-source-review.md)).
- **No "every failing test is a learning failure" assertion:** N33-A and N33-B concern *measurement* and *consumer contract*. To show a **real quality regression**, demonstrate changed blinded domain-law correctness on the *same prompt and frozen learned state* while changing only relevant recipient/consumer treatment.
- **No "100% of possibleDifs reviewed" assertion:** prior 200+ files and sibling histories are larger than this bounded audit; note duplication if another owner of the exact macro-denominator counterexample is later found.
- **One variable, one accepted conclusion:** first a no-training parity/control proof; then ONE lawful small training recipient and free-answer task to see whether reporting divergence masks measurable learning. [Bounded executable work order](33-MICRO-RECIPIENT-AND-METRIC-QUALITY-EXPERIMENT-2026-10-16.md).
- **Skills/review:** `architect-behavior-classifier` locates owner/caller; `code-examination` reads upstream/downstream; `plan-self-validation` / `plan-review` reject circular exam; `experiment-design` pins independent stratified heldout; `prompt-patch-authoring` freezes exactly one factor; `conversation-intent-and-self-reflection` plus `claim-verification` prevent overstating source as native learning.
