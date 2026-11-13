# N23 — Independent causal-gap audit: Tatoeba evaluation, lexical induction, checkpoint cache

**Дата:** 2026-10-16. **Класс:** SOURCE_CONFIRMED (для точных методов) + SYNTHETIC_PROXY_EXECUTED (для 12 сценариев) + HUMAN_QUALITY_NOT_MEASURED + NATIVE_NOT_RUN + CODEX_ASTRA_REVIEW_NOT_RUN.

**Core pin:** [0eccbdc3c4efb90adf1d699806cbfa9f5610998d](https://github.com/lubochka/llm_mvp_core/tree/0eccbdc3c4efb90adf1d699806cbfa9f5610998d). **Не приписывать этим находкам причину RP01 Student DPO 0/3 и 0/4:** расследован `TatoebaPilotLoopRunner → TranslationMachine structured training → TranslationRoutingFlow`, другой учебный и evaluation-путь. Это новая отдельная область риска, а не повтор исходных DIF-01…44.

## 1. Сводка для архитектора

| ID | Приоритет | Наблюдаемое в коде | Что потенциально теряет пользователь | Evidence boundary |
|---|---|---|---|---|
| N23-01 | P0 для Tatoeba promotion, НЕ RP01 Student | Метрика `ProductMatch` пропускает потерю отрицания, числа, сущности, действия, добавленные слова | Запрещённый или ложный перевод считается изученным правильно; завышается сигнал для promotion | SOURCE + synthetic JS; .NET и независимый judge не выполнены |
| N23-02 | P1 conditional EN_HE | `TargetScriptOk` рассчитан только на RU, все не-RU проверяются как Latin | Правильный Hebrew-only ответ отвергается | SOURCE + synthetic JS; реальный EN_HE pilot invocation не доказан |
| N23-03 | P1 | Индукция словаря/фраз из совместной встречаемости без word/phrase alignment | Unit получает неправильное соответствие source→target, даже при верном sentence pair | SOURCE; полезность/реальные ошибки ещё не измерены |
| N23-04 | P2 conditional | Cache-hit по `(path, schema, mtimeTicks, length)`, без content SHA | При внешней подмене checkpoint с сохранением размера/mtime возможен stale model | SOURCE; штатная запись через runner инвалидирует cache, это существенное смягчение |

## 2. N23-01 — ложноположительный успех как потеря обученного качества

**Настоящий source:** [BuildRowResult:1439–1463](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L1439-L1463), [TokenSimilarity/HasExtraNegationJunk/TargetScriptOk:1465–1523](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L1465-L1523), [metric to promotion:704–744, 405–420](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L704-L744).

Реальная формула: `ProductMatch = scriptOk && !bootstrap && !copied && !extraNegation && (exact || similarity >= .50)`. Значение `similarity` есть recall МНОЖЕСТВА содержательных слов ожидаемого ответа: `|ExpectedContent ∩ ActualContent| / |ExpectedContent|`. Это **не precision, не BLEU и не предметная корректность**. `ContentWords` пропускает однобуквенные слова и цифры, а `not` и `не` включены в stop words. `HasExtraNegationJunk` ловит только `actualNegations > expectedNegations`, но не `actualNegations < expectedNegations`.

**Проверяемые GOOD/BAD/EDGE:**
- RU→EN: Expected “Do not open the door.” vs Actual “Open the door.”: content recall 1, no extra-negation → ProductMatch true, но закон отрицания нарушен.
- EN→RU: Expected “Не открывайте дверь.” vs Actual “Открывайте дверь.”: recall 1, тоже false-positive.
- EN→RU: Expected “Передайте отчёт Марии сегодня.” vs Actual “Передайте отчёт Ольге сегодня.”: ожидаемые content `передайте/отчёт/марии/сегодня`; три из четырёх совпадают → 0.75, wrong recipient принят.
- Expected “Отправьте 5 писем.” vs Actual “Отправьте 50 писем.”: числовое изменение игнорируется.
- Expected “Доступ запрещён.” vs Actual “Доступ разрешён.”: content recall 0.5, изменение разрешения принято.
- Expected “Кошка спит у окна.” vs Actual “Кошка спит у окна. Дом горит.”: лишний вредный tail не штрафуется (односторонний recall).
- GOOD: точный перевод с правильными действием, получателем, числом, отрицанием и target script должен остаться принят. EDGE: латиница в имени/коде, случай с пустым content set, смешанные языки, нормализация, пограничный порог 0.50.

**Зафиксированный ограниченный опыт без запуска обучающей модели:** [JS source-aligned proxy](23-MICRO-SCORER-LAW-PROXY.mjs) воспроизводит перечисленные операции на 12 специально созданных synthetic-сценариях; [JSON](23-MICRO-SCORER-LAW-PROXY-RESULT.json). При source-aligned вычислении: 11/12 `ProductMatch=true`, 4/12 вручную размечены `oracleCorrect=true`; **8 false-accept, 1 false-reject, 3 true-accept, 0 true-reject**. Эти доли НЕ являются частотой проблем в реальных данных, performance/gain или независимой оценкой CODEX. Это намеренно adversarial micro-fixture для контрпримера. Нативный .NET вызов исходного метода ещё не выполнялся.

**Кausal chain:** если этот scorer оценивает реальные outputs и `RuntimeProductAccuracy` участвует в `BestRuntimeProductAccuracy`, `PassesTarget` и policy promotion thresholds, ошибочный score может направлять loop не туда (ложный success, откладывание обучения или выбор неправильно принятого checkpoint). Прямой эффект на качество требует A/B train + matched heldout после доказанной native parity; пока только механизм ошибки оценки.

## 3. N23-02 — EN_HE target script

[TargetScriptOk:1498–1509](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L1498-L1509) возвращает RU-case `hasCyrillic && !hasLatin`, иначе `hasLatin && !hasCyrillic`. [TargetLanguage:1536–1546](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L1536-L1546) явно разрешает `EN_HE → he`. Для `expected=actual="שלום"` `exact=true`, но `scriptOk=false` → `ProductMatch=false`. Подтверждено формулой/JS, не подтверждён actual EN_HE pilot production caller. Не путать с сохранёнными EN→RU качественными тестами RP01.

## 4. N23-03 — co-occurrence lexicon ≠ alignment

[InduceLexicon:1162–1247](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L1162-L1247): для каждого предложения все `sourceWords` перемножаются со всеми `targetWords`, после чего считается support/confidence/specificity и выбирается наибольший score; это совместная встречаемость внутри предложения, **не выравнивание пары слов по смыслу**. Условия `MinimumLexiconSupport=3`, `confidence=.60`, `specificity=.45`, margin=.15 ограничивают шум, но не доказывают переводную правильность.

[InducePhraseMappings:1249–1328](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L1249-L1328) также ранжирует target ngrams по co-occurrence; [TrainRowsAsync:746–773](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L746-L773) реально передаёт выведенные отображения через `ToTrainReadyPair→TranslationStructuredBatchBuilder→TrainStructuredBatchAsync`. Ошибка **возможна**, но не доказана для существующего корпуса.

**Узкий причинный тест:** 12–20 lawful parallel sentences с нарочно сконструированной корреляцией двух source-token и одного target-token; frozen aligned gold от отдельного CODEX judge; train/heldout family-disjoint. В сравнении `lexicon on` vs `lexicon disabled`, phrase mappings фиксировать; затем отдельный тест только для phrase. Измерить wrong word association / wrong target / exact-and-law-correct translations, отдельно coverage. Запрещено одновременно менять сам корпус, токенизатор и ранжирование.

## 5. N23-04 — in-process checkpoint object alias и file-stamp cache

[TranslationCheckpointCache:15–65](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TranslationCheckpointCache.cs#L15-L65) использует `AbsolutePath/SchemaVersion/LastWriteUtcTicks/FileLength`, хранит не immutable state, а `TranslationMachineLoadResult`, включающий ссылку на `Machine`. [Runner resume:183–211](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L183-L211) возвращает cache hit без перечитывания. Если внешний writer заменит файл другими байтами с идентичным size+mtime, cache может вернуть старую модель. Отдельно изменения живого `Machine` по общему object reference могут не соответствовать сохранённому состоянию. **Ограничение:** штатный [SaveLoopCheckpointAsync:642–669](https://github.com/lubochka/llm_mvp_core/blob/0eccbdc3c4efb90adf1d699806cbfa9f5610998d/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L642-L669) вызывает `Invalidate` и `Store` после сохранения; это снимает обычный сценарий stale cache, но не доказывает внешние/другие caller cases. Низкий приоритет, не кандидат немедленного RP01 repair.

## 6. Отличие от чужих записей и ближайшее действие

Поименованные проверки: [N21 actor/negation *admission*](21-MICRO-CAUSAL-LEARNING-QUALITY-PILOT.md) теряет противоположный lesson **до** обучения; [N20 KNN nonlocal rerank](20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md) меняет выбор stored output **после** обучения; этот N23 проверяет **качество решения, которое признают выученным**. Это самостоятельный слой, но общий causal result RP01 пока не установлен. Уже существующие DIF-11/16 о metric ≠ quality являются родительскими общими сигналами, не отдельной новой причиной. При дублировании другой записью приоритет у точного первичного кода, не у даты документа.

Дальше исполнить только [MICRO-N23 native+matched quality protocol](24-MICRO-TATOEBA-QUALITY-GATE-EXPERIMENT.md); НЕ запускать дорогую Student DPO только ради подтверждения этой формулы. Исходные данные и фикстуры не переписывать. Найти actual invoking direction/consumer перед исправлением. CODEX/Astra 6 UltraCode делает человеческий выбор учебной задачи/получателя и **независимую оценку DPO качества**; разрешённая внешняя открытая китайская модель создаёт **оба** chosen/rejected. Синтетическая ручная метка этого proxy не заменяет evaluator. Новый реальный CODEX/Astra review exact SHA: **NOT_PERFORMED**.

**Публикация:** документ исследовательский, авторское source review ≠ независимый архитектурный review. В продукте ничего не исправлялось; .NET/Windows CI не выполнялись.

## Addendum N23-03A — конкретное ложное соответствие из ЧЕТЫРЁХ правильных предложений

Независимый *source-aligned* контроль формулы `InduceLexicon` (JavaScript, **НЕ C# native**) использует четыре допустимых EN→RU учебных предложения:

| English source | Russian target |
| --- | --- |
| dog barks here | собака лает здесь |
| dog barks loudly | собака громко лает |
| dog barks daily | собака лает ежедневно |
| dog barks softly | пёс тихо лает |

`dog` встречается в 4/4 строк. `лает` встречается в 4/4 русских ответов, `собака` — в 3/4. По настоящей формуле `confidence × specificity` получаются `dog→лает: 1.00` и `dog→собака: 0.75`. Оба проходят support≥3, confidence≥.60, specificity≥.45; отрыв 0.25 превышает `MinimumLexiconScoreMargin=.15`, поэтому выбранный по score target — **`лает`**, неверное соответствие существительного с глаголом. Здесь нет ложного teacher sentence: все четыре предложения сами по себе корректны.

Воспроизведение: [4-row script](23-MICRO-LEXICON-COOCCURRENCE-PROXY.mjs) и [raw JSON](23-MICRO-LEXICON-COOCCURRENCE-RESULT.json), запуск из каталога `possibleDifs`: `node 23-MICRO-LEXICON-COOCCURRENCE-PROXY.mjs repro.json`. **Этот результат подтверждает механизм ошибочного sub-Unit label construction на синтетической выборке, но не демонстрирует фактическое ложное downstream translation:** mapping может быть отвергнут последующим consumer/переопределён другими правилами. Следующий gate — вызвать оригинальный C# `InduceLexicon` на тех же четырёх `TatoebaPrivateSentencePair`, проверить emitted mapping, затем `ToTrainReadyPair→BuildGeneralBatch→TrainStructuredBatchAsync→TranslationRoutingFlow` и отдельную blind noun/verb correctness panel. Сравнить выключение ТОЛЬКО inducted lexicon при неизменной phrase mapping. Не утверждать эффект на RP01 StepVector/LoRA.
