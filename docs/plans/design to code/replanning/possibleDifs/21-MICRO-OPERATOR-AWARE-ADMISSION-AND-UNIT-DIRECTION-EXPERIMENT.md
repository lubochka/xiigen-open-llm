# MICRO-21 — быстрый эксперимент «исчезнувшая противоположность»: native C# и проверка изменения качества

**Дата:** 16.10.2026. **Статус:** спецификация; JS-прокси выполнен, native C# и обучение не выполнялись. Source: llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5. Корпус RP01 / исходные DPO / существующие веса остаются неизменными.

## Цель и single-variable контракт

Проверить, что осмысленно новый пример с противоположным оператором (== против !=, >= против <=, true против !true) отбрасывается **до вызова обучаемого Unit** потому, что novelty filter принимает его за старую запись. Проверить отдельно, что при сохранении операторов обе задачи попадают в Unit и **реальное качество** на новых задачах возрастает, а не только счётчик обученных строк.

**Baseline A:** TranslationStructuredBatchBuilder.FilterByTrainingHistoryUniqueness с неизменёнными Tok​enSetForSimilarity / Jaccard threshold 0.70.

**Treatment B:** только экспериментальная тестовая функция строит тот же fingerprint, но добавляет operator features (например op:==, op:!=, op:>=, op:<=, op:!, op:<, op:>); **тот же threshold 0.70**, те же строки/разделение/Unit/другие параметры. Это не обещанный production fix: versioning, domain-specific polarity, parens/type/scope и semantic equivalence требуют отдельного design review.

**Primary mechanism metric L1:** admitted_critical_anti_pairs / four synthetic antonym contrasts. **Primary application metric L2:** на 8–12 frozen independent lawful cases — paired exact law correctness обычного TranslationRoutingFlow after cold re-open. Вспомогательные: retained lesson IDs, new memory/state digest, lost-law critical errors, other-domain retention, wall/CPU/peak memory.

## L0 — выполненная JS shadow-проба, без модели

Ресурсы [runner](21-MICRO-LAW-ANTIPAIRS-NOVELTY-PROXY.mjs) · [raw JSON](21-MICRO-LAW-ANTIPAIRS-NOVELTY-PROXY-RESULT.json) хранят начальную воспроизводимую формулу. Текущий JS baseline отбрасывает **4/4** synthetic law-opposites с similarity=1.0; operator-aware вариант оставляет 4/4 (similarity 0.6/0.667). Exact duplicate остаётся отброшен; явно иной глагол допускается в обоих режимах. Фактический Node script executed locally; native .NET **NOT_RUN**. Пользовательские ответы не генерировались.

## L1 — минимальный настоящий native C# тест, ДО обучения

**Именно existing public API, не скопированная формула:**

1. Создать в test-only проекте четыре пары history→new A-side: «проверь x == null»→«проверь x != null», «проверь i < 10»→«проверь i > 10», «проверь total >= limit»→«проверь total <= limit», «проверь (!valid)»→«проверь (valid)».
2. Создать два controls: exact duplicate (должен отбрасываться); unrelated verb «проверь»→«удали» (должен сохраняться). Передать текст точно, без normalization вне Product code.
3. Для каждой пары вызвать actual TranslationStructuredBatchBuilder.ComputeTokenOverlap(history, proposed) — ожидается 1.0 на четырёх операторных контрастах. Создать TranslationTrainReadyPair c Direction="RU_EN", ValidationOutcome="train_ready", пустыми ValidationErrors и валидными, но локальными test-only output fields. Вызвать **actual** FilterByTrainingHistoryUniqueness([row], [history]) и получить 0 на четырёх противоположностях. Assertions привязать к настоящему пути и версии исходника.
4. Test-only B строит typed operator fingerprint, не меняя порог; требует retained=4/4, exact duplicate=0, unrelated verb=1. Контроль абстракции: не переписывать C# оригинал лишь ради pass; сохранить bounded diff, delta hashes, команды, actual output и worst-case exception.
5. **Отдельно** воспроизвести FindSimilarityContamination через минимальный Batch029 runner fixture либо точечно через test-only вызов private method с reflect; expected train↔heldout operator antonym получает 1.0 и может приводить к TRAIN_HELDOUT_SIMILARITY_OVERLAP. Это не разрешение убирать полноценную train/heldout contamination защиту — только получить точное доказательство ложноположительного случая.

**Pass L1:** native path exhibits specific loss and B admits contrast without admitting duplicate. **Fail:** native current code behaves otherwise, source path pin drift/hidden normalization, proxy was incomplete. При отсутствии установленного dotnet — честный NOT_RUN с missing-tool и источниками, не считать JS native.

## L2 — 8 учебных записей и 12 независимых случаев (не 500 DPO)

Материал: 4 законные противоположности для двух семейств, два контекстно-несущих варианта каждой, максимум 8 TRAIN; 4 DEV для выбора строго подходящего heldout family и предзаданных правил; 12 SEALED HELDOUT из непересекающихся source/family, включая 4 контрастных операторных пары и 4 обычных controls. CODEX/Astra judgement законов вслепую к метке arm, без просмотра candidate rank.

**Обязательное управление причинностью:**
- До обучения заморозить identical starting TranslationMachine checkpoint/state, actual unit registry, tokenizer, KNN index and deterministic fallback settings. A/B получают **одни и те же** исходные законные примеры и отдельное однофакторное решение фильтра, без других изменений.
- Зафиксировать exact переданные BuildGeneralBatch training examples, counts after dedup, UnitIds, training method calls, expected vs actual state revisions, CRC/SHA и конкретный ordinary Flow, которые читают learned state. Не выдавать случайно оставшийся +0.01 synthetic TrainBatchAsync за обучение — нужен TrainStructuredBatchAsync.
- Обе ветви после обучения сохраняются и холодно восстанавливаются с настоящими Unit checkpoints; сравнить исходный пользовательский запрос через ordinary TranslationRoutingFlow, не лишь MeasureUnitCoverage или tokenizer direct.
- Negative N повторяет B с переставленными chosen outputs при тех же admitted examples; улучшение N должно исчезнуть. No-update parent контроль оценить на том же frozen panel.
- Основной эффект: количество B-only-lawful minus A-only-lawful, critical wrong counts, retained pre-existing old law. Если меняется только admitted/RowsTrained без semantic improvement — MECHANISM_ONLY. 8–12 случаев = exploratory, не 95% generalization или RP01 recovery. Если ничего не меняется — NO_OBSERVED_QUALITY_GAIN.

## L3 — отдельная направленность FrequencyQuote Unit (без обучения достаточно для первого шага)

Создать две FrequencyQuoteEntry с одинаковым семантическим уровнем: одна SourceLang="ru" с русским FullSourceSentence и английским FullTargetSentence, другая SourceLang="en" с английским источником и русским target. Вызвать actual FrequencyQuoteMapBatchBuilder.BuildBatch:

- RU->EN expected UnitId executor-ru_en, actual executor-ru_en.
- EN->RU expected UnitId executor-en_ru, **source code predicts actual executor-ru_en** (требуется native подтверждение).
- Проверить actual runtime caller: только если этот builder реально используется с en-source, проходить вперёд до TranslationMachine.TrainStructuredBatchAsync, _unitTrainers registry, конкретного wrong-direction state и ordinary translation result. Без обнаруженного caller — SOURCE_CONDITIONAL, не production bug.
- После native guard test-only B маршрутизирует по явной проверенной direction, метрикой является правильный ID и state owner. Изменение конечного перевода — отдельное качество, не подразумевается успешным UnitId.

## CODEX executable work order: минимальная программа действий

**CURRENT TASK:** выполнить L1 и L3 native тесты сначала; только при подтверждённом реальном mismatch и наличии 8 lawful lessons — L2. Цель: быстро обнаружить missing learning signal до Unit и определить actual learner/consumer.

**NOT THE TASK:** 500 DPO, new teacher calls, дорогостоящие Windows Actions, произвольная перепись KNN/LoRA, изменение canonical RP01 checkpoints и frozen results, широкая версия production fix без review.

**REQUIRED SKILLS:** docs/skills/architect-behavior-classifier-SKILL.md; phase-planning-SKILL.md; plan-review-SKILL.md; code-examination-SKILL.md; generated-code-review-SKILL.md; human-readable-architecture-audit-SKILL.md; prompt-patch-authoring-SKILL.md; experiment-design-SKILL.md; learning-path-audit-SKILL.md; claim-verification-SKILL.md; output-skepticism-SKILL.md. Выполнить architecture/code source review, план проверки, независимый review конкретного changed diff и readback публикации, не подменять каждый pass формальной строкой.

**SOURCE EVIDENCE MAP:** source filter [TranslationStructuredBatchBuilder.cs:194–255](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L194-L255); actual caller [TranslationBatch029Runner.cs:98–159](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L98-L159); contamination [TranslationBatch029Runner.cs:387–445](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationBatch029Runner.cs#L387-L445); wrong-direction builder [TranslationFrequencyQuoteMap.cs:38–75](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/27.TranslationDomain/TranslationFrequencyQuoteMap.cs#L38-L75). Core snapshot must match, else independent re-read & downgrade.

**ALLOWED:** read-only evidence, local scoped test-only harness/fixtures, narrow proposed test-only scorer/tokenizer callback, planning artifacts in possibleDifs, independent review, authorized commit/push/readback. **FORBIDDEN:** mutate canonical existing raw DPO/weights/heldout, train unaudited Chinese-teacher pairs, claim CODEX/Astra has judged when it has not, accept synthetic case as DPO, force-push or overwrite concurrent README changes, use production quality score for dev tuning.

**RETURN PACKET** with exact: core/planning head SHA; actual commands and UTC timestamp; each source/input hash; before/after retained train and heldout IDs; native vs shadow status; per-case gold law+output+failing reason; actual Unit state hash before/after cold import; no-update/shuffled output controls; any cost/time/CPU/RAM; reviewer identity/independent evidence bound to candidate SHA; published artifact SHA; conclusion from IMPROVE / REGRESS / MECHANISM_ONLY / INCONCLUSIVE / INVALID_COMPARISON; remaining work and actual ordinary consumer provenance.

## Governing DPO roles

**CODEX с GPT-6 Astra 6 UltraCode выбирает учебные задачи от имени человека. Разрешённая внешняя открытая китайская модель создаёт BOTH exact chosen и exact rejected для настоящих DPO. CODEX с GPT-6 Astra 6 UltraCode отдельно оценивает полученную пару.** Post-update evaluation Student выполняется отдельно. В этом микротесте используются только synthetic Unit fixtures; ни роль внешней Teacher, ни реальное DPO авторство ими не имитируются. Независимого CODEX/Astra ревью этой спецификации пока НЕ БЫЛО.

## Stop / приёмка

**Преждевременно останавливать расширение**, если native actual builder опроверг гипотезу, нужный caller не используется, learner не вызывает обучаемый Unit, heldout оказался невалидным либо замер случайно объединил изменение фильтра с model weights/seed/consumer. No-operator-contrast admission + B positive law correctness on matched ordinary Flow → предварительная поддержка причины Unit quality; не доказывает RP01 Student. Пока native+heldout отсутствуют, допустимый итог этой работы: **source-confirmed probable gap and a completed cheap JS causal mechanism check**, не «исправлено обучение».
