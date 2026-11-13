# N29-A — новый проверяемый разрыв: метка из rejected становится частью адреса *правильного* учебного примера

<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-N29:START -->
## Адресный эффект, качество и baseline — уточнение 11 ноября 2026

Сверены P52/F06 и correction F09 на core 55cfd9d00bbebdeae04da7c505b84d0b1cb9b747; N29 не объявляется новым доказанным ухудшением качества. В P52 attempt-0002 writer и JSON называют cold 0/4 пределом постоянного ответа. Настоящий answer ceiling 2/4, tense ceiling 2/4. Правильное значение другой попытки не исправляет этот носитель. Поправка содержит hash и JSON pointer; raw неизменен. Oracle выводится из gold тем же scorer, независимо от cold.

Адресное чтение 1/4→3/4 и ответы 4/4→4/4 — разные оси. Нельзя назвать их улучшением перевода или стереть полезный адресный результат. Сохранить actual route: full/prefix/structure, exact-source correction и факт достижения изменённого reader. Positive reached-consumer и negative masking control предъявлять раздельно. Вариант с новой target при прежнем input не является source-disjoint.

Teacher annotation остаётся provenance/target, пока не показан source-derived runtime producer до ответа. Перенос тега из rejected в chosen не решает его недоступность для нового пользовательского input. По каждой label указать данные learner и данные runtime; равенство сохранённых fields не доказывает отсутствие leakage.

Actual source-echo может быть реальным ответом, а третий текст, отличный от source и gold, — выдуманным. Для product correction проектируется server-owned issued/verification registry с lookup canonical payload и атомарным claim доставки до Train; произвольный typed record caller недостаточен. Это не доказательство DPO-эффекта нынешнего Executor.

Baseline/overlap исправляются по сохранённым данным с Train=0. Новый source-disjoint опыт требует собственного паспорта и расхода; старый бюджет этой вставкой не расширяется. [Core аудит](https://github.com/lubochka/llm_mvp_core/blob/d9fc526b61a11312baefef5b1246064552bc4c85/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-11/2026-11-11-experiment-transfer-audit.md).
<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-N29:END -->


## Сверка P52 — 30.10.2026

На core `cbf3b1d834cbf28baa2bb13baeb1bddc17e21019` P52 подтвердил адресную часть: A0/2,B2/2,mutation0/2 по нужным пометкам. Точный текст4/4 во всех обученных плечах, включая copy-off/mutation. Это **KEY_SHAPE_ONLY / NO_CONTRAST по переводу**, совместимо с исходным предупреждением о маскировке другими путями. Native builder всё ещё читает rule tags из rejected; экспериментальная копия не закрывает product transfer.

Следующий перенос разделяет source-derived runtime features и teacher provenance. Замена rejected на chosen также не проходит приёмку, если этот признак невозможно получить до runtime ответа. Нужны actual route trace, положительный reached-consumer пример и отрицательный masking control; новое обучение этим дополнением не назначается. Полный аудит и адресный план — в мосте30.10 рядом с этим файлом.

**16.10.2026 · источник core `codex/dpo-implementation@d5c2409b24292d361593b8262b0c54997b15f539`.** Статус: **SOURCE-CONFIRMED MECHANISM / NATIVE-C# NOT_RUN / USER-QUALITY CAUSALITY OPEN / independent CODEX–Astra review NOT_RUN**. Направление: structured Translation Units + KNN, **не** непосредственный RP01 neural DPO/LoRA. Создано только в `xiigen-planning/.../possibleDifs`. Закрытый для изменений `llm_mvp_core/docs/plans/replanning-possible-diffs` не редактировался.

## 1. Понятно человеку

Программа берёт правильный перевод `chosen`, но адрес для его запоминания делает частично из объясняющей аннотации *ошибочного* ответа `rejected`: `[RULE: ...]`. В новом пользовательском предложении этой учительской аннотации нет. Получается ситуация: **правильный урок сохранён по метке, которую машина умеет воспроизвести для исходной строки, но не обязана уметь вычислить для нового предложения того же смысла**.

Это не утверждение, что KNN сломан у каждого запроса. На exact-source запросе обученная `_signatureCorrections` и `_exactSourceMap` часто полностью маскируют дефект; в `prefix`/`structure` buckets сохраняются альтернативные адреса, и сосед тоже может находиться. Нужно измерить именно долю новых запросов, которая реально доходит до этого уровня, и разные KNN маршруты.

## 2. Первичные исходники и точная цепочка

1. [`TranslationStructuredBatchBuilder.BuildGeneralBatch`, строки 89–111](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L89-L111): базовая `sequence.Signature` от `pair.ASide` затем изменяется `signature = AppendRuleTags(signature, BuildRuleTags(pair.Rejected));`. Таким образом feature зависит от *rejected side*, не от свойств исходного input, доступных обычному runtime.
2. [`BuildRuleTags` и `AppendRuleTags`, строки 433–460](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L433-L460) и [`RuleTagRegex`, строки 555–557](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L553-L557): из `rejected` достаются `[RULE: arbitrary-text]`, нормализуются до `RULE:...` и добавляются к `SemanticSignature`.
3. [`BuildGeneralBatch`, строки 106–142](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L106-L142): та же изменённая `signature` уходит в три разных учебных примера: SemanticTokenizer, Executor и QualityVerifier. Значит это не просто метаданные отчёта.
4. [`SemanticTokenizerUnit.TrainOnExample`, строки 123–142](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L123-L142) сохраняет выбранную сигнатуру по точному исходному тексту; [`ExecutorUnit.TrainOnExample`, строки 404–424](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L424) записывает эту сигнатуру как полный KNN bucket. В отличие от неё дополнительные prefix/structure строятся заново из `new TranslationSemanticTokenizer()`.
5. [`TranslationSemanticTokenizer`, строки 110–155](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationSemanticTokenizer.cs#L110-L155) вычисляет runtime-теги из source/POS/русской грамматики, **не читает `rejected` и не создаёт произвольный `RULE:` по комментариям учителя**. На *незнакомом* тексте у `SemanticTokenizerUnit` ещё нет exact correction.
6. [`TranslationKnnStore.FindNearestCascade`, строки 239–295](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationUnitContracts.cs#L239-L295) ищет совпадение full signature, затем prefix, затем structure; общий component-overlap `FindNearest` в cascade **не используется**. [`QualityVerifierUnit`, строки 1051–1139](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationTrainedUnits.cs#L1051-L1139) тоже может хранить `requiredTermsBySignature`, привязанные к изменённому адресу.

**Конструктивный контрпример (предсказание по коду, C# НЕ запускался):**

- TRAIN source `"Проверь журнал ошибок"`, chosen `"Check the error log"`, rejected содержит разрешённое `[RULE: preserve_log_noun]`.
- Ключ Unit в `TranslationLearningExample.SemanticSignature` должен содержать `RULE:PRESERVE_LOG_NOUN`. Обученный semantic Unit может воспроизвести его только для исходной точной строки.
- Новый unseen source `"Проверь ошибочный журнал"` строит подпись без такого тега, если в речи пользователя его нет и эта строка ранее не обучалась.
- Подсистемы prefix/structure могут всё-таки найти пример; **проверить реальный trace и quality — обязательно**, нельзя заявлять NO_HIT без выполнения.

## 3. Историческая и причинная граница

Это **унаследованная особенность**, а не найденный diff, введённый RP01: [`DesignToCode` та же строка `BuildRuleTags(pair.Rejected)`](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L90-L113) подтверждена просмотром ветки. `language_translate` содержит другие ранние механизмы; этот конкретный builder в ней не найден, поэтому её нельзя использовать как якобы подтверждённый positive A/B.

Есть уже найденные смежные причины:
- [N21 self-label / `|PAIR:id`](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md): искусственный chosen tokenizer.
- [N26 language key + RuleTags rebuild](26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md): потеря *вычислимых из source* `RU:...` tags при восстановлении prefix.
- [N28 upstream/downstream key drift](28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md): обученный upstream extractor отличается от вновь созданного bootstrap extractor.

**Узкая новая гипотеза N29-A** — *учительская annotation от rejected попадает в позитивный lookup key*, хотя этот признак на новых inputs объективно отсутствует. Её нужно проверять отдельно от N26 и N28. Не смешивать с neural RP01 0/4; причинное влияние на результат RP01 **NOT_ESTABLISHED**.

## 4. Быстрый фальсификатор и домены

Эксперимент [N29-MICRO](29-MICRO-REJECTED-KEY-AND-WORD-ORDER-CAUSAL-EXPERIMENT-2026-10-16.md), **Arm A**: две валидные копии одной свежей structured pair (в разных isolated machines), одинаковые source/chosen и train identity, но один rejected несёт `[RULE: ...]`; сравнить `BuiltExample.SemanticSignature`, Tokenizer exact-correction, KNN keys и endpoint на **8 новых source-disjoint** парафразах. Не менять gold/teacher-авторство; это диагностика через synthetic test fixture, не production DPO admission. Затем отдельно one-factor shadow *annotation metadata only*, без изменения будущего consumer или LoRA.

Предсказание механизма: запись полного KNN ключа зависит от аннотации rejected; base tokenizer нового input — нет. **Фальсификация quality-эффекта:** если full/prefix/structure coverage и ordinary Flow ответы одинаково корректны, гипотеза качества понижается до `KEY_SHAPE_ONLY`, не чинить бесполезную ветку.

**Применимость:** технические инструкции RU→EN/EN→RU с `[RULE:]`; доменные случаи, где преподавательский law должен сначала стать воспроизводимым semantic feature. **Не применять как общее правило** к чистому neural DPO, AST-kNN без данного builder, либо unit без rejected annotation.

**Skill-рамка:** architect (реальный learner и consumer), code examination (source→calls→consumers), planning/plan-review (bounded A/B), prompt engineering (никакой учительский label в gold), human-readable architecture audit (объяснение разрыва человеку), experiment-design и claim-verification. Только документация, без product code mutation и без вызова отдельного CODEX/Astra reviewer.
