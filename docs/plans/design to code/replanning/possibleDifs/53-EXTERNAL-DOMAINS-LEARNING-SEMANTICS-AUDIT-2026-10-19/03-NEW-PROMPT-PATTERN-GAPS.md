# PT-01 / PT-02 — память PromptToTask может терять полезный ответ после правильного дополнения

19 октября 2026. **PT-02 — новая source-bound гипотеза; PT-01 — новая конкретная локализация прежнего открытого duplicate lead. Влияние на качество RP01 не измерено.** [Финальная граница новизны](08-FINAL-CORE-DOCS-NOVELTY-CLARIFICATION.md). Обе относятся к сохранённой памяти паттернов и её потребителю. Это отдельный текущий механизм: в `external_domains@3036a01…` одноимённого модуля нет. Поэтому эти пункты идут после [восстановления подтверждённых донорских функций](00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md), а не объявляются потерянными исправлениями старого neural trainer.

## Достижимость и смысл слова «обученный»

`PromptToTaskMachine.Translate` загружает descriptor, создаёт `PromptToTaskTaskPatternUnit` из его `LearnedPatterns` и использует выбранный паттерн для `StructuredTaskContract`. Однако сам selector явно называется baseline nearest-signal-overlap, а `SemanticQualityClaimed` остаётся `false`. Наличие поля `LearnedPatterns` не доказывает, что его содержимое получено реальным DPO или что конечная задача выполнена правильно. Здесь проверяется более узкое свойство: **сохраняется ли полезное решение уже принятой памяти при повторном дополнении и при извлечении признаков запроса**. [Machine:117–215][translate] [Unit:159–230][decision]

В API обычный путь существует: `XiigenExtensionApiService.Process` вызывает `PromptToTaskMachine.Translate`, затем передаёт `StructuredTask` в `OrchestrationReasoningMachine.Plan`. Для эффекта на этой первой границе нужен загруженный PromptToTask state именно у выбранного экземпляра. Успех всех соседних assets не является условием самого вызова Translate; для проверки полного ответа состояние остальных потребителей, конечно, тоже должно быть работоспособным. Новый N52 обсуждает другую проблему — загрузку OrchestrationReasoning и общий readiness. [API:122–139][api-call] [N52: admission/readiness][prior52]

**Общий приоритет:** P1 для соответствующего PromptToTask-пути при наличии памяти; условный дополнительный риск относительно isolated RP01. Прямого пути из `CodeDocumentationSimulationDpoExperiment` к этим классам не установлено. При пустой памяти, другом маршруте или отсутствии вызывающего приложения этот механизм не объясняет наблюдаемое ухудшение.

## PT-01. Повтор правильного знания превращается в неоднозначность

### Причинная цепочка

| Шаг | Точное поведение | Следствие |
|---|---|---|
| Дополнение памяти | `ContinueWithAcceptedPattern` добавляет паттерн через `Concat([learnedPattern])` и устанавливает новое состояние | Одинаковое содержимое с другим PatternId остаётся отдельным соседом. [262–305][append] |
| Поиск | Ранжируются отдельные patterns, затем берутся первый и второй | Семантически одинаковые ответы конкурируют друг с другом. [233–249][nearest] |
| Score | `Confidence × matchedSignals / patternSignals` | Две одинаковые записи при одном запросе получают одинаковый балл. [507–534][score] |
| Условие готовности | `topTwoMargin = top.confidence − second.confidence`; margin ниже threshold даёт NotReady | При defaults `MinimumTopTwoMargin=0.01` точный повтор обнуляет margin. [100–128][defaults] [175–206][decision] |
| Потребитель | На NotReady машина пробует generic baseline, иначе возвращает no matching learned pattern | Паттерн с конкретными требованиями и запретами больше не выбран. [135–180][translate] |

Минимальное свидетельство: память содержит P1 с сигналами `translation, verify`, confidence 0.9 и полным решением «проверить перевод с данными требованиями». На подходящем запросе единственный top имеет margin 0.9. Добавляем P2 с другим ID, теми же входными сигналами, **всеми** теми же полями результата, SplitId и confidence. Оба score равны 0.9; новый margin — **0**. Качество первого знания не ухудшилось, противоположного ответа не добавлено, но ready меняется на not-ready.

Это конкретнее общего тезиса «дубликаты мешают KNN»: один принятый правильный повтор меняет **решение о наличии достаточного знания**. Число знаний выросло, а путь к прежнему полезному ответу исчез.

### Как отказ может скрыться в штатном API

Если обычный запрос содержит API context, машина может вернуть `baseline_internal_universal_task` вместо not-ready. `BuildPromptContextFields` ставит `xiigen_method` и `raw_prompt_redacted`; `BuildGenericRuntimeSignals` превращает их совместное наличие в `extension_api_boundary_present`. Вместе с ещё одним generic signal это разрешает fallback. Поэтому нужно сохранять не только success/status, но и реальные `TaskType`, требования, ограничения, deliverables и запрещённые действия. [Fallback:461–527][fallback] [Runtime signals:530–561][runtime-signals] [API context:1092–1110][api-context]

Например, общий корректно оформленный ответ может остаться, но конкретное ограничение принятого паттерна «только проверить, не изменять» исчезнуть из выбранного задания. Это **source-predicted следствие**, а не записанный результат native API в этой работе. Fallback не притворяется семантически проверенным: код явно оставляет соответствующие quality-флаги false.

### Минимальный эксперимент EXP-PT-01

**Первая стадия — 0 optimizer updates.** Один сохранённый PromptToTask state, один настоящий `ContinueWithAcceptedPattern`, четыре запроса: подходящий исходному знанию; его новая формулировка; запрос к независимому паттерну; намеренно конфликтующее решение. Для каждого выполнить A с нынешним append и B с узкой идемпотентностью для точного семантического повтора. Затем сохранить оба состояния и повторить те же четыре запроса в новом процессе: **4 запроса × 2 плеча × 2 состояния процесса = 16 Translate**. Входная память, request context и downstream состояние фиксированы.

Четыре строки manifest — это точные `(caseId, stateHash, request)`, а не четыре запроса к одному неизменному набору конкурентов. Первые три используют positive-repeat state P1+P2; для четвёртой независимо создать frozen negative-control state P1+P3, где P3 имеет те же LearnedSignals/Confidence, но действительно другой полный semantic payload. Конфликт возникает из этого состояния, не из смены текста запроса. В обоих плечах четвёртый case получает соответствующий conflict state; бюджет 16 Translate сохраняется.

B не сравнивает лишь TaskType. Совпасть должны нормализованные LearnedSignals, все ExplicitUserRequirements/Constraints/Deliverables/ForbiddenActions/RequiredHosts/EvidenceNeeds, SplitId и Confidence. IDs и accepted provenance сохраняются отдельно; реальный конфликт остаётся конфликтом. Если порядок какого-либо списка имеет предметный смысл, его нельзя нормализовать как множество. Не понижать общий ambiguity threshold ради fixture.

Сначала записать matched signals, top2 IDs/scores, margin, selected pattern и полный emitted task. Затем на том же API проверить, дошли ли оба требования до обычного планировщика. Если нужен реальный learner для получения памяти, это отдельный подготовительный опыт; вручную созданный descriptor не считать прошедшим обучение.

**Доменные панели:** две близкие задачи проверки C# документации и две проверки перевода EN→RU; в каждой есть явно зафиксированный запрет на редактирование. Сначала один домен; второй нужен для проверки применимости, а не смешивается в общий score. Практический малый сигнал — B сохраняет все прежние требования в positive-repeat случае и не разрешает конфликт ни в одном negative control. Изменение только pattern count или SHA не считается пользой.

**Опровержение/сужение:** production admission уже исключает такие повторные patterns до этого метода; текущий consumer не использует эту память; thresholds у реального состояния отключают данное условие; native повтор не меняет решение; либо A и B дают одинаковое предметно верное задание. Необходимость общей агрегации близких, но не идентичных паттернов этим опытом не доказывается.

## PT-02. Признак missing_gates создаётся из слова gate и даже из delegated

### Доказательство в исходнике

`ExtractPromptSignals` собирает слова и добавляет `BuildUniversalSignalAliases`. В aliases есть как специфическое `"missing gate" → "missing_gates"`, так и общее **`"gate" → "missing_gates"`**. `AddAliasIf` применяет `normalizedPrompt.Contains(phrase, StringComparison.Ordinal)` без границы слова. [344–366][extract] [448–504][aliases]

Извлечение относится к `AuthorityTexts` после отделения evidence-only блоков. Для случая нужен обычный текущий instruction или корректный `current_unquoted_user_instruction`; не утверждается, что любой процитированный старый текст автоматически получает authority. [BuildQuery / quarantine:327–410][authority]

Таким образом, «The gate is complete» получает признак отсутствующего gate. Слова `delegate` и `delegated` тоже содержат подстроку `gate`. Это уже не только потеря отрицания или порядка слов: **fixed feature extractor сам добавляет конкретное ошибочное смысловое утверждение до обращения к памяти**. Последующий nearest selector использует этот признак как обычный сигнал. [BuildQuery / matching][nearest] [Score][score]

Сконструированная память содержит два независимых паттерна: `complete` с confidence 0.9 → review completed work; `missing_gates` с confidence 0.95 → repair missing gate. Для четырёх фраз о завершённом gate/review нынешняя формула выбирает второй. Узкое B-вмешательство удаляет только generic mapping `gate`, сохраняя специфический `missing gate`; правильный паттерн снова выигрывает. Контроли «Repair the missing gate» и «The documentation is complete» не меняются.

### Что действительно воспроизведено

[Python probe](evidence/prompt/prompt_pattern_probe.py) читает точные anchors трёх C# источников, извлекает актуальные AddAliasIf mappings и воспроизводит только scoring/margin для ASCII fixtures. [Receipt](evidence/prompt/prompt_pattern_result.json) сохраняет hashes источников, все входы, scores и статусы.

| Проверка | A | Узкий B | Статус |
|---|---|---|---|
| PT-01, одинаковое полезное знание с другим ID | Margin 0, not-ready | Прежнее решение сохранено | Source-bound arithmetic |
| PT-01, действительно конфликтующий output | Not-ready | Not-ready | Отрицательный контроль |
| PT-02, 4 положительных + 2 контрольных ASCII fixtures | 2/6 совпадений с заданным вручную expected action | 6/6 | **Не метрика обученной модели** |

Native C#, обучение, восстановление checkpoint и независимая доменная оценка в этом probe не выполнялись. Action в Python — сокращённый carrier полного решения; он не является реализацией всех C# contract validations. Машинный fallback разобран по source, а не эмулирован как весь API. Нельзя называть 2/6→6/6 измеренным приростом качества RP01.

### Минимальный эксперимент EXP-PT-02

**Механическая стадия:** реальный Unit на шести приведённых ASCII fixtures и четырёх дополнительных контролях, A и B, одинаковый accepted state: **20 Decide**, 0 updates. Дополнительные случаи — `gate is absent`, `no missing gate`, одна русская и одна другая не-ASCII фраза. Первые два проверяют возможную потерю recall в B и остаточную ложную активацию специфического alias; они не включены в выполненные 2/6→6/6. Записать полный extracted signal set, matched patterns и task. Для Unicode не полагаться на Python/.NET parity.

**Предметная стадия:** по четыре новых запроса в документации и оркестрации — gate завершён, gate отсутствует, действие делегировано, состояние неизвестно. Независимая рубрика оценивает сохранение исходного смысла, допустимость repair и запрет лишнего изменения. Сначала один домен: 4 × 2 плеча = 8 API обращений. Модель, память, thresholds и downstream state одинаковы; только один alias отличается. Для перевода это релевантно лишь если текст проходит через PromptToTask; standalone TranslationMachine не объявляется затронутым.

**Опровержение/сужение:** реальные принятые patterns никогда не используют `missing_gates`; upstream предоставляет другие запросы; признаки не попадают в фактический selector; current A уже выбирает правильное действие на слепой панели; либо специфический mapping необходим для корректных выражений и B ухудшает их. Более точный semantic extractor тогда исследуется отдельно. Нельзя убрать все fixed правила на основании одного плохого alias.

## Новизна и следующий владелец

В первоначальном корпусе 661 Markdown отсутствовали PromptToTask и оба точных механизма. В свежих 10 добавленных документах planning появился PromptToTask как соседний asset N52, но не `Concat → duplicate top2 margin` и не `gate → missing_gates`. Граница новизны и полный поисковый ledger — [методология](06-SOURCES-NOVELTY-AND-REVIEW.md). После этого этапа в core `358af351…` обнаружен содержательный generic duplicate/margin lead с `OPEN_RECIPIENT`. Поэтому PT-01 учитывается как новая локализация owner/trigger/consumer, а не новая общая идея; см. [финальную главу 08](08-FINAL-CORE-DOCS-NOVELTY-CLARIFICATION.md). Абсолютная новизна во всех ветках и чужих непубликованных черновиках не заявляется.

Следующий исполнитель получает один выбранный EXP-PT с этим документом и pinned source, сначала подтверждает выбранный state/consumer, затем меняет одну границу в изолированной исследовательской копии. Результат должен различать `MECHANISM_REPRODUCED`, `NO_GAIN`, `IMPROVE`, `REGRESS` и `UNREACHABLE_IN_SELECTED_RUN`. Без измеренного доменного ответа статус learning quality остаётся `NOT_MEASURED`.

[translate]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L117-L215
[append]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L262-L305
[decision]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L159-L230
[defaults]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L100-L128
[nearest]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L233-L249
[score]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L507-L534
[fallback]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L461-L527
[runtime-signals]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L530-L561
[api-call]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L122-L139
[api-context]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1092-L1110
[extract]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L344-L366
[authority]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L327-L410
[aliases]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L448-L504
[prior52]: https://github.com/lubochka/xiigen-planning/blob/02532653562a5f1e27fd75b1b03d4ae23e930e2f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/52-ORCHESTRATION-REASONING-CHECKPOINT-AND-ADMISSION-GAP-2026-10-19.md
