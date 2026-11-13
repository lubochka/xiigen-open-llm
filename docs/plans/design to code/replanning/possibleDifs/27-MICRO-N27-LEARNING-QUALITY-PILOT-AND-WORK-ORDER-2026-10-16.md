# MICRO-N27 — минимальный причинный опыт качества обучения: Verifier с отрицательными и положительными ответами

**Дата:** 16.10.2026. **Состояние:** `READY_AS_PROTOCOL / NOT_RUN / DRAFT`. Этот опыт **проверяет надёжность оценки обучаемого Unit**, а не утверждает улучшение RP01 Student. Target core: `llm_mvp_core@8c7d41611aecba98d3d1bbdbf30d69bd784949ae`; исследовательский результат хранится только в `xiigen-planning/.../possibleDifs`.

## Суть одним предложением

Если проверять выученный перевод только на заведомо неправильных ответах, валидатор, который **отказывает всегда**, может получить 100% «точности», хотя не умеет пропускать правильные ответы. Нужен один быстрый контраст: **тот же обученный Verifier, те же источники, но две полярности ответа**.

## Основания и границы

- [`TranslationStructuredBatchBuilder.cs#L128-L141`](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L128-L141): `BuildGeneralBatch(heldOut:true)` строит для Verifier *по одной* отрицательной паре `CandidateText=rejectedCandidate; expected.Pass=false`.
- [`TranslationTrainedUnits.cs#L1148-L1170`](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationTrainedUnits.cs#L1148-L1170): `QualityVerifierUnit.MeasureAccuracy` сравнивает только `Pass`, жёстко семантизирует source с `"ru"`; full correctness и language scope не измеряет.
- [`TranslationDirectionAdapters.cs#L117-L139`](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L139): ordinary TrainedTranslationAdapter использует `Verifier.Verify` для принятия реального ответа, с отдельным исключением phrase-map. Это даёт обоснование проверять *consumer effect*, а не только счётчик PASS.
- [Новые source findings N27-C](27-NEW-UNIT-HELDOUT-VERIFIER-TOKENIZER-METRIC-BIAS-2026-10-16.md) и [N23 learned verifier conflicts](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md).

## Ограниченный CODEX work order

**Владелец выполнения:** CODEX с запрошенной `astra 6 ultracode` для выбора вместо человека и независимой смысловой оценки. Если текущий runtime не предоставляет это имя, записать реальное `model_id` и `NOT_AVAILABLE` — не выдавать другую модель за Astra. Для этого эксперимента **новые DPO не создаются**; если они понадобятся позднее, внешняя открытая китайская модель авторствует одновременно `chosen` и `rejected`, а отдельный CODEX на `astra 6 ultracode` судит неизменённые пары.

**Inputs:** 4 источника RU→EN из уже разрешённого обучающего набора, которые CODEX подтвердил как **четыре различные смысловые семьи**. Отдельные 4 закрытые от обучения validation source из других семей. Для каждого validation source составить два кандидата: корректный перевод `GOOD` и минимально ошибочный `BAD`, с подтверждённым независимым gold `Pass=true/false`. Не использовать ответы heldout для построения выученных rules, dictionaries или tuning; frozen tokenizer, Unit state и decoder.

**Два плеча на ОДНОМ и том же состоянии Unit:**

| Arm | Test rows | Что ожидать от корректной метрики |
|---|---|---|
| A: actual-shaped | 4 `BAD` (held-out record текущего builder) | Только specificity / true-negative rate; **не считать общей accuracy** |
| B: balanced | те же 4 `BAD` + те же source с 4 `GOOD` | sensitivity, specificity, balanced accuracy, false-reject и false-accept |
| C: control | фиктивный классификатор `always Pass=false` на тех же 8 records | На A proxy 4/4, на B 4/8; **это проверка качества тестового протокола, не модельный результат** |

Сравнить `QualityVerifierUnit.MeasureAccuracy(arm A)` и отдельно `Verify(candidate, originalSourceTokens)` на всех 8 cases, затем аккуратно подсчитать confusion matrix, не меняя модель. `B` можно собирать test-only DTO вручную на базе настоящих gold; **не передавать heldout через `TrainStructuredBatchAsync` в обученную машину**, пока не проверен [N27-A readiness reset](27-NEW-STRUCTURED-READINESS-RESET-AFTER-NOOP-BATCH-2026-10-16.md). Лучше вызвать `MeasureAccuracy`/ `Verify` напрямую на отдельной fresh loaded Unit/translation snapshot.

**Особый контроль:** `MeasureAccuracy` принудительно использует русский tokenizer; RU_EN выбран, чтобы избежать ложной межъязыковой причины. Направления EN_RU/HE_EN/ES_EN проверить отдельным 2×2 после RU_EN, меняя только исходный язык. Исключение phrase-map в ordinary adapter измерить отдельно; не считать `Verifier.Pass=false` эквивалентом product rejection при phrase-map override.

## Решение по результатам

- **CONFIRMED_METRIC_BIAS:** always-fail проходит A и проваливает positive-class, B различает этот failure. Этот вывод об обязательности balanced test можно доказать и формулой; реальное поведение Unit отдельно.
- **REAL_UNIT_FALSE_REJECT:** обученный Unit отклоняет independently-approved GOOD; необходимо указать case, source family, required learned rule, candidate, inferred language и actual consumer path.
- **NO_OBSERVED_GAIN:** A/B дают одинаковые корректные ответы, либо расхождение только в reporting; запрет заявлять quality regression/gain.
- **INVALID_EVIDENCE:** gold зависит от builder/bootstrap, повторяется обучающая семья или потеряна actual Unit state identity.
- **INCONCLUSIVE:** нет CODEX approved gold, Unit context/pair authorship недоступны.

**Минимальные сдаваемые файлы после native выполнения:** `n27-micro-inputs.json` с source-family hashes без приватных raw, `n27-micro-predictions.json` (source-ID, GOOD/BAD, Pass, learned rule, route), `n27-micro-confusion.json`, `n27-micro-verdict.md`. Ни один из них **не создан и не выдается за выполненную проверку** сейчас. Порог: 0 принятых критических смысловых ошибок, любые `false-reject` и `false-accept` перечислить без усреднения; 8 случаев — smoke/causal evidence, не статистическое доказательство генерализации.

**Дополнительно в следующем отдельном малом опыте:** [N27-B cache/disk compare](27-NEW-MUTABLE-TRANSLATION-CHECKPOINT-CACHE-ALIAS-2026-10-16.md) и N27-A heldout-only score suppression; на них не переносить вердикт этого эксперимента. **Windows CI и 500 DPO pairs не требуются** для диагностики.
