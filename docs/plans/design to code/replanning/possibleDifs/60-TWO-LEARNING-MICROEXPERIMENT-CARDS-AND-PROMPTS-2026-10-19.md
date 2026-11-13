# N60-C — Две условные карточки настоящего обучения и промпты исполнителям

**Дата:** 2026-10-19. **Статус:** DRAFT_ONLY, EXACT_USER_APPROVAL=PENDING, NEW_NATIVE_RUNS=0. Защищённый каталог llm_mvp_core/docs/plans/replanning-possible-diffs/** — READ ONLY. В этом документе НЕ утверждается, что N60 открыл прежде неизвестные элементарные дефекты: P52.8, P54, P56 уже описывают составные части; проверяется их прежде неизмеренная связка.

## Общее правило, обязательное для КАЖДОЙ карточки и промпта

Любое фактически измеренное полезное улучшение — УСПЕХ именно по этому показателю и предметной теме; рост accuracy НЕ обязателен. Все остальные показатели и УХУДШЕНИЯ включить рядом. Недопустимо назначать обязательные repeated runs/Tier3 только ради переименования успеха. Если measured subject НЕ участвует в обучении реального Unit/LoRA и тот же trained state НЕ читает ordinary consumer, это INVALID_AS_LEARNING_EXPERIMENT: НЕ ТРАТИТЬ ТОКЕНЫ. Frozen scoring, policy-only inference, offline metric, standalone restore или mock runner таким экспериментом не являются. До личного одобрения Любы полного текста и SHA точной версии карточки никакие опыты не выполнять.

## Карточка N60-E1 — Отрицательный пример обучает Verifier отвергать законную парафразу

**ExperimentId:** N60-E1-VERIFIER-NEGATIVE-ADMISSION. **Тема:** перевод. **Профиль:** EN→RU; RU→EN только отдельной авторизованной постановкой. **Цель:** не переносить ложные fail-уроки в память Verifier, не потеряв верные target constraints. **Один фактор:** admission отрицательного Verifier-урока при rejectedCandidate==chosen.

**Первичные источники:** [BuildGeneralBatch](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L65-L147); [BuildRejectedCandidate](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L268-L326); [Verifier.TrainOnExample + Verify](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L1037-L1175); [real caller dispatch](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L89-L144); [ordinary adapter](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L109-L146). Prior: [P52.8](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P52-semantic-annotation.md) and P56. Новое — проверяемая последовательность от false-negative к реально достигнутому learned rule и собственному ответу; влияние НЕ доказано.

**Получатель:** QualityVerifierUnit, обновляемый реальным TranslationMachine.TrainStructuredBatchAsync. Для обычного перевода используется TrainedTranslationAdapter через GetOrchestratorAsync.RunAsync. ExecutorUnit и tokenizer одинаково предварительно обучены или frozen в обеих ветвях; не сравнивать два разных Student.

**A0=B0:** два независимых, равных состояния реальных Units, с checkpoint SHA, одинаковая версия builder, source/target language, verifier rules и одинаковая обученная либо пустая Executor memory. До нового урока зафиксировать own outputs по панели, версии, sha и route.

**TRAIN:** 2–4 заранее одобренные пары. Одна с типом ошибки gender/case/tense, но без отдельной явной законной rejected строки; одна с честным отличающимся rejected; при необходимости ещё 1–2 проверяемых пары для настоящего source marker. TRAIN source, chosen и EXAM gold независимо оценены до опыта. Вход должен реально дать equality rejectedCandidate==chosen, иначе постановка неприменима и STOP. Никаких gold EXAM в Unit.

**A1 — baseline:** существующий materializer отправляет три records, включая неверный Verifier fail-example, в действительный TrainStructuredBatchAsync. **B1 — enhanced:** materializer в том же batch отвергает *только* Verifier false-negative record с кодом NO_DISTINCT_NEGATIVE; остальные Semantic/Executor records и настоящие Verifier negatives побайтно одинаковы. Accepted training rows различаются именно из-за единственного фильтра; это измеряемая стоимость, не ошибка дизайна. Training и learned state обновляются *на самом деле* в обоих arms.

**EXAM:** 4–8 независимых ранее неизвестных source-family запросов, парафразы и смысловые противоположности, включая отрицание/согласование, альтернативное корректное слово, полностью незнакомый source. Тот же ordinary consumer реально отвечает; сохранять raw candidate, actual verifier rules readback, Issues, Pass, adapter/verdict, отказ и финальный текст. Если consumer не читает изменённый learned rule либо оба checkpoint одинаковы — NO_CAUSAL_LINK, прекратить, не подменять опыт сценарием с direct Verify/mock.

**Метрики:** A0/A1/B0/B1 correct/all, correct/answered, coverage, wrong accepted, false rejected, retained prior correct, valid/invalid learned lessons, update/time, latency, CPU/RAM/VRAM и size of state, число и глубина Unit/Flow; neural total/trainable layers = 0. Сохранить gained/lost по ID, независимый gold, raw state и стоимость учителя.

**Фальсификатор:** нет отличающихся реально выученных правил или ordinary Flow никогда их не использует — гипотеза не подтвердилась. Изменение только числа написанных JSON records без recipient/memory/output НЕ является улучшением обучения.

**Правило успеха E1:** любое измеренное полезное улучшение — успех по его показателю и переводу, без роста accuracy. Параллельно показать все ухудшения, в особенности coverage, false accepted и teacher effort.

**ПРОМПТ ИСПОЛНИТЕЛЮ E1 (проект, не разрешение):**
> CURRENT TASK: провести строго N60-E1 только после личного одобрения Любы полного текста и SHA. Required skills: planning-session-startup, architect-behavior-classifier, phase-planning, plan-review, audit-protocol, code-examination, claim-verification, experiment-design, human-communication-GLOSSARY. Прочти указанные исходные файлы и callers перед модификацией. Сначала найди документированный реальный producer→TrainStructuredBatchAsync→Verifier learned rule→ordinary TrainedTranslationAdapter; без любого звена STOP/BLOCKED. Сравни A0/A1/B0/B1, реальные outputs и state с одним фактором false-negative admission, 2–4 одобренными уроками и 4–8 новых EXAM. Не изменяй protected и не запускай полный Windows CI/массовые 500-step runs. Любое реально измеренное улучшение полезной метрики считается успехом этой оси и темы даже без прироста accuracy; все прочие изменения, потери и стоимость записать рядом. Если измерение не участвует в процессе обучения, опыт НЕВАЛИДЕН: запрет расхода токенов. Никакие данные и запуск не разрешены до exact пользовательского review.

## Карточка N60-E2 — Обучаемые операторные слоты в памяти композиции EN→RU

**ExperimentId:** N60-E2-LEARNED-OPERATOR-SUPPORT. **Тема:** перевод. **Профиль:** EN→RU, технические команды. **Получатель:** ExecutorUnit phrase/word/exact learned memory via real TrainStructuredBatchAsync and ordinary TranslationRoutingFlow, не нейронный RP01 DPO. **Один фактор:** представление признаков источника, записываемое *при обучении* в state памяти и применяемое тем же learner при композиции.

**Прямые источники:** [положительный native 8-lesson report](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md); [raw after outputs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/after/raw-outputs.json); [phrase composer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L805-L860); [ordinary adapter](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationDirectionAdapters.cs#L109-L146). Исторические строки cc01 и cc04 потеряли отрицание/вопрос; обе имели adapterId executor-en_ru-phrase-map и confidence=0.9900. Не утверждать, что сработал verifier bypass: archived raw не содержит verification.Pass/Issues.

**A0/B0:** одинаковый fresh Unit checkpoint, текущий источник восьми уроков с сохранённым SHA и frozen 24-case EXAM. **A1:** те же 8 легальных уроков и обычная запись phrase/word memory. **B1:** те же 8 lessons, но при TrainOnExample записывается *из source TRAIN, а не EXAM gold* дополнительно версия и модель-supported input operator/role/order slots, связанная с собственными learned phrase mappings. Обычный Executor обязан читать **эти записанные при обучении slots**, чтобы отличать знакомые композиции и незнакомые смысловые операторы. Если в B меняется только frozen inference guard, нет real changed trained state/ordinary reader, то это НЕ learning experiment; STOP без costly run. Не вводить hardcoded if для cc01 или абстрактный AST для перевода; AST актуален для C#/SQL, здесь word order/polarity.

**Единственное концептуальное изменение:** representation/support-acquisition during learning; все остальное frozen — source/chosen, tokenizer base, verifier rules, thresholds и corpus admission. Если B требует второго независимого изменения policy, разделить в другой дизайн, иначе нарушено one-factor comparison. В отчёте указать, что learned operator marker не является доказательством полноценной семантики. Проверить cold import только как часть этого learning A/B, не самостоятельный restore run.

**EXAM:** прежняя 24-панель: 8 exact + 3 near + 8 recombinations + 5 edge с собственным independent gold; 4 новые семейно независимые фразы только по отдельному одобрению. Для каждого входа ordinary answer A0/A1/B0/B1, selected adapter, learned support available, correct/accepted, coverage, wrong accepted, recall. 8 recombinations — отдельная bucket, не 8 независимых смысловых законов; danger on cc01/cc04 вместе с ошибками/отказами.

**Бюджет:** 8 fixed lessons в каждом arm, одна небольшая native training call per arm, 24 коротких consumer calls на каждое after, readback states; без CI Windows, полного retrain, 500 DPO steps, synthetic oracle outputs. Выполнить только если профиль и state interface доказаны и exact card version письменно одобрена.

**Метрики:** correct/all, correct/answered, coverage, wrong-accepted/all, old skill retention, number of trained map entries, source feature bytes, training time, total wall, CPU/RAM/VRAM, target calls и teacher annotation minutes, per-query latency, changed Unit/Flow depth; neural trainable layers=0. Фактический gain by safety может сосуществовать с lost coverage и скоростью; все привести рядом. Числа 11/24,19/24,1→3 — ИСТОРИЧЕСКИЕ, не результат E2.

**Фальсификаторы:** B не изменяет memory при Train, не читает ту же memory в ordinary RunAsync, не отличает unseen operators или сохраняет те же неверно принятые ответы. Последний случай означает отсутствие улучшения на этой оси, не обязательно провал всех метрик.

**Правило успеха E2:** любое фактически измеренное полезное улучшение — успех по его показателю и переводу без обязательного роста accuracy. Обязательно одновременно показать другие изменения и ухудшения.

**ПРОМПТ ИСПОЛНИТЕЛЮ E2 (проект, не разрешение):**
> CURRENT TASK: при отдельном точном разрешении Любы провести N60-E2 на исторических восьми уроках EN→RU и frozen 24-case панели с сохранёнными источниками/SHA. Load skills: planning-session-startup, architect-behavior-classifier, phase-planning, plan-review, audit-protocol, code-examination, claim-verification, experiment-design, human-communication-GLOSSARY. Показать доказанную трассу TrainStructuredBatchAsync→ExecutorUnit learned state→ordinary RunAsync. A — старое представление урока, B — только одна versioned обучаемая проекция operator/role/order; сравнить A0/A1/B0/B1, raw ответы, state, gained/lost, latency/RAM/bytes и teacher effort. Никакого добавления в TRAIN новых отрицаний, вопроса или exact EXAM gold; это иной опыт N59-L01. No hidden hardcoded heuristic on cc01. Любое полезное измеренное улучшение является успехом по своей оси без требования роста accuracy, а все ухудшения записываются рядом. Если вмешательство не пишет и не читает реальное обучаемое состояние, опыт невалиден и запрещено тратить токены на его исполнение. Protected не менять, Windows suite не запускать, карточка DRAFT до review пользователем.

## Общая форма отчёта и reviewer gate

| ID и предметный gold, зафиксированный до | A0 own | A1 own | B0 own | B1 own | gained/lost + explanation |
|---|---|---|---|---|---|
| case 1 |  |  |  |  |  |
| case 2 |  |  |  |  |  |

| Ось | A до→после | B до→после | полезный gain по теме | потери и ограничение |
|---|---|---|---|---|
| correct/all; correct/answered; wrong-accepted; coverage | | | | |
| обучение sec/update; latency/query; time-to-usefulness | | | | |
| CPU/RAM/VRAM; learned-state/checkpoint bytes | | | | |
| lessons accepted, code/unit/flow depth; model total/trainable layers | | | | |
| ручные annotations, price/time, retention older lessons | | | | |

**Паспорт перед разрешением:** reviewed exact card SHA; разрешённый experimentId/profile; source commit pin; owner + actual reader route; утверждённые train/gold identities SHA; initial checkpoint; единственный фактор; exact budget; allowed files; forbidden files; owner executor/reviewer; stop controls; raw result directory вне protected; date + текст личного разрешения пользователя. Пока поля не установлены — NEEDS_INPUT, а не READY.

**Результат проверки:** новые карточки подготовлены, но ничего не обучено/не измерено в этой сессии.