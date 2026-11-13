# N21 — Новый source audit: учебные противоположности исчезают ДО обучения; отдельный риск wrong-direction Unit

**16.10.2026 · pinned core:** lubochka/llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5, branch codex/dpo-implementation. **Статус:** CODE_CONFIRMED conditional behavior + EXECUTED_JS_SHADOW, NATIVE_DOTNET_NOT_RUN, USER_QUALITY_NOT_MEASURED, CODEX/ASTRA_INDEPENDENT_REVIEW_NOT_PERFORMED. Этот конкретный механизм обнаружен после проверки уже опубликованных DIF-01..44, 14, 16, 18 и параллельного N20, но полнота отрицательного поиска по всем другим агентам не доказана.

## Главное наблюдение на человеческом языке

В одном из **действительно вызываемых** путей обучения переводческих Unit есть защита от повторного обучения на похожих примерах. Она удаляет из строки все знаки операций перед сравнением с историей и измеряет похожесть наборов оставшихся слов. Для двух учебных задач «проверь x == null» и «проверь x != null» получаются **одинаковые** наборы слов. Вторая, противоположная задача ошибочно называется дубликатом. Значит, алгоритм, который должен изучить обе стороны семантического закона, **получает только одну**, хотя обе созданы как разные учебные случаи.

Это не «LoRA плохо обучается», не потеря знаков после DPO/в exact memory (она описана раньше). Это **ранний отказ допустить законный контрастный урок в датасет Unit** — до TrainOnExample и до индексации KNN. Даже идеальный оптимизатор не может выучить отклонённый материал.

Схема без кода:

    История H: "проверь x == null"    →  {проверь, x, null}
    Новый N:  "проверь x != null"    →  {проверь, x, null}
                                      Jaccard = 1.00
                                      порог 0.70 → N ИСКЛЮЧЁН
    Ожидалось: разные законные операторы == / != требуют сохранить оба урока.

## N21.01 — P0 для Batch029 Unit / источник и условия проявления

1. **Извлечение признаков:** [TranslationStructuredBatchBuilder.cs L194–255](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L194-L255). TokenSetForSimilarity делает lowercase, Split только по whitespace, затем char.IsLetterOrDigit для каждого символа и ToHashSet. Символы ==, !=, >=, <=, <, >, ! **не остаются ни отдельным токеном, ни feature**. FilterByTrainingHistoryUniqueness отбрасывает новые пары при Jaccard >= 0.70 к любому историческому A-side.
2. **Реальный вызывающий маршрут:** [TranslationBatch029Runner.cs L76–110](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L76-L110). Загружаются corpusPairs, и **если corpusPairs.Count > 0**, trainPairs фильтруются относительно corpusPairs.ASide. Затем [строки 126–159](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L126-L159) превращают именно оставшиеся записи в структурный Unit batch; пустой split-batch пропускается. Поэтому это не невызванный helper: минимум одна production/research callsite с реальным эффектом.
3. **Отдельный контрпример для оценки:** [TranslationBatch029Runner.cs L387–445](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L387-L445) использует **вторую реализацию той же потери операторов** (TokenizeForSimilarity) при проверке train↔heldout. Для пары с разными операторами similarity=1.0 и при пороге >0.70 будет возвращён TRAIN_HELDOUT_SIMILARITY_OVERLAP [строки 120–124](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L120-L124). При этом предварительная проверка буквального совпадения NormalizeForOverlap смотрит полную строку со знаками и может эти два примера пропустить. Так система способна **либо не допустить учебную противоположность, либо запретить измерить её на heldout**. Второе — защита от data leakage со слишком грубой метрикой, не доказательство того, что true heldout должен всегда допускать near-duplicates.

**Область доказательства:** ру-английский Batch029 structural translation research path с corpus history; это **не** прямой путь RP01 CodeDocumentationSimulationDpoExperiment exact-v2, и вызов этих фильтров из main500/ENRU500 **не установлен**. Возможное ухудшение качества именно RP01 — H, а не C.

## N21.02 — выполненный короткий механизмный контроль (JS-прокси)

Кодовая формула повторена только для ASCII + русских букв, с Unicode буквенно-цифровой фильтрацией. [Runner](21-MICRO-LAW-ANTIPAIRS-NOVELTY-PROXY.mjs), [raw results](21-MICRO-LAW-ANTIPAIRS-NOVELTY-PROXY-RESULT.json). Локально Node v22 исполнил 6 случаев, asserts=true:

| Пара разных источников | Current Jaccard | Current retained? | Символо-aware Jaccard | Символо-aware retained? |
| --- | ---: | --- | ---: | --- |
| x == null / x != null | 1.000 | **нет** | 0.600 | да |
| i < 10 / i > 10 | 1.000 | **нет** | 0.600 | да |
| total >= limit / total <= limit | 1.000 | **нет** | 0.600 | да |
| (!valid) / (valid) | 1.000 | **нет** | 0.667 | да |
| одинаковая строка / точно такая же | 1.000 | нет (правильно) | 1.000 | нет |
| проверь x / удали x при том же == null | 0.500 | да | 0.600 | да |

Один фактор в прокси: **сохранить явный typed operator token** в fingerprint; те же исходные строки и threshold=0.70. Это не предлагает «разрешить все near-duplicates»: отрицательный exact-duplicate контроль продолжает исключаться. JS повторяет алгоритм, но **не** вызывает реальный C# helper, не измеряет semantic gain Student и не является реальным DPO.

## N21.03 — отдельный source-confirmed wrong-direction Unit при формировании frequency/quote lessons

[TranslationFrequencyQuoteMap.cs L38–94](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L38-L94) использует entry.SourceLang для semantic tokenizer и ExecutorInput, **но независимо от SourceLang всегда назначает TranslationUnitIds.ExecutorRuEn для executor lesson**. При SourceLang="en" и английском source/русском target ожидаемый UnitId для такого двухъязычного задания — executor-en_ru, а фактический — executor-ru_en. [TranslationUnitContracts.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationUnitContracts.cs#L86-L110) определяет оба отдельных ID; [TranslationMachine.cs L714–785](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationMachine.cs#L714-L785) создаёт два разных ExecutorUnit. Поэтому ошибочный UnitId **валиден** (не будет отброшен как unknown N17), и выбранный урок может уйти к **другому языковому получателю**.

Условие: FrequencyQuoteEntry с SourceLang="en" действительно передаётся BuildBatch и затем в TrainStructuredBatchAsync; поиск конкретного caller/реальных en-входов в истории здесь не подтвердил исполнение. Возможен намеренный RU_EN-only контракт генератора; текущая сигнатура не ограничивает SourceLang. Следующее действие — два entry (ru/en) → BuildBatch → проверить UnitIds → подходящие реальные machine trainer state/output на холодном restore. **Не объявлять дефектом RP01 ENRU500 без caller trace.**

## Почему это новые механизмы, а не повторение уже описанного

- Ранее [N16 operator identity](16-MICRO-OPERATOR-IDENTITY-CAUSAL-EXPERIMENT-2026-10-16.md) обнаружил схлопывание **ключа exact memory во время записи/поиска**. N21.01 показывает, что противоположный урок вообще **не доходит** до trainable Unit. Продуктовая починка только NormalizeMapKey N16 не спасёт контрастные уроки, убранные upstream.
- Ранее [N17 unknown UnitID](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md) нашёл **пропущенный** неправильный ID. N21.03 — **существующий, но чужой** направлению ID: report может выглядеть как успешное обучение другого Unit. Это другой failure class.
- Ранее [N16 hardcoded RU_EN evaluator](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md) затрагивал eval direction. N21.03 затрагивает **формирование training material до evaluator**.
- Две сходные копии loss-of-operator fingerprints внутри Batch029 (admission и heldout-contamination) — разные точки влияния; не удваивать их как две независимые причины одной и той же формулы.

## Короткий настоящий эксперимент и план исправления

[21 — исполнимый .NET work order и условия причинной проверки качества](21-MICRO-OPERATOR-AWARE-ADMISSION-AND-UNIT-DIRECTION-EXPERIMENT.md).

1. **Native L1 (минимум без model training):** вызвать actual public TranslationStructuredBatchBuilder.ComputeTokenOverlap и FilterByTrainingHistoryUniqueness на 4 противоположных строках + 2 контролях, выводить retained IDs. Отдельно опробовать actual TranslationBatch029Runner private contamination comparer/conditional runner with fixture, не менять score по ожиданию. При наличии другой нормировки результаты переобозначить.
2. **Tiny quality L2:** две копии одного frozen TranslationMachine/одни source-disjoint реальные RU_EN cases, A=current admission, B=operator-aware admission и неизменённые learner/scorer/holdout. Тренировать максимум 8 lawful lessons, 8–12 blind heldout; считать actual Unit state hashes и ordinary Flow translations. Отрицательный shuffled-label контроль при одинаковом составе принятых уроков; **если изменения лишь в числе строк, это evidence исправления отбора, не доказанный quality gain**.
3. **Direction L3:** две FrequencyQuoteEntry (ru/en) и таблица ожидаемого/фактического ID + direct Unit behavior; отдельный causal test только при подтверждённом runtime caller.
4. **Не менять** historical RP01 thresholds/weights/records и не запускать Windows CI. Не принимать новые DPO без правильного Teacher/CODEX admission.

## Роли, которые нельзя перепутать

**Человеческий выбор учебного случая выполняет CODEX на GPT-6 Astra 6 UltraCode.** **Обе окончательные DPO-стороны chosen/rejected создаёт только разрешённая внешняя открытая китайская модель.** **Оценку качества точной полученной пары отдельно выполняет CODEX на GPT-6 Astra 6 UltraCode.** Независимая оценка обученного Student/Unit после update — четвёртая отдельная функция. Здешние 6 synthetic fixture НЕ являются разрешёнными DPO и не претендуют на реальные модельные вызовы.

**Статус:** N21.01 фактический потерянный различающий признак в реальном caller — C; N21.03 unconditional constant UnitId in builder — C, actual EN→RU occurrence — U; Node shadow — EXECUTED; native .NET — NOT_RUN; measured downstream learning gain — NOT_MEASURED; independent CODEX/Astra code and pair review — NOT_PERFORMED.


## Cross-branch provenance (добавлено после первичного аудита)

Прямо проверены соседние ветки: исторический [DesignToCode@8f20ceb8a9bfbadab2ad309226c47dd76e714548](https://github.com/lubochka/llm_mvp_core/tree/8f20ceb8a9bfbadab2ad309226c47dd76e714548), DesignToCode-clean, codex/dpo-implementation-s04 и действующий codex/dpo-implementation. В DesignToCode [TranslationStructuredBatchBuilder.cs:234–247](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L234-L247) loss-of-operator TokenSetForSimilarity **текстово совпадает** с действующей реализацией; в [TranslationFrequencyQuoteMap.cs:62–63](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L62-L63) постоянный ExecutorRuEn тоже совпадает. Это унаследованные архитектурные риски, **а не доказательство регрессии, введённой именно RP01**. В language_translate и external_domains оба файла отсутствуют; отсутствие файла в соседней ветке не доказывает, что нужный алгоритм раньше был успешнее, если архитектура этого домена в той ветке ещё не существовала. Прямой caller Batch029 подтверждён в текущей версии, historical successful same-panel parity и native outcomes не выполнены.

**Итог по новизне:** новый для просмотренного возможного causal ledger **failure stage** (отбор контрастных уроков + запрет law-holdout), не новосозданный кодовый дефект и не заявка на эксклюзивное обнаружение среди всех исследователей.
