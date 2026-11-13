# N35 · Новая связанная гипотеза: потерянное отрицание в word-map превращается в ответ с confidence 0.99

<!-- CODEX-POST125-PLANNING:START -->
## Адресное уточнение после опытов P118–P125 — 3 ноября 2026

Применение к этой карточке: **`RESTORE-ACTUAL-CHECKPOINT`**. Источник ошибки проверен на core `9d4d79d27f18066ce3a346fac2a5eea156304f62`; это профилактическое усиление соответствующей границы, не утверждение о новом исполнении исторического опыта этой карточки.

P12501 уже исполнен: 0 → 19/24, после загрузки 19/24; новый процесс и 24/24 буквального совпадения подтверждаются сохранёнными файлами. Это отдельный состоявшийся прогон на тех же уроках и панели P2502; он не разрешает повторять P2502 и не является карточкой P125-executor-memory-learning.

**Хороший пример и действие проверяющего:** Читать существующие checkpoint, memory-trained/restored, source-provenance и 72 ответа. Один и тот же независимый сравниватель проверяет exact key с Topic, phrase key и KNN key/target, версии, полный состав с кратностью записей, происхождение и значимый порядок, затем обычные ответы. Все обязательные результаты должны влиять на reader-status, общий outcome и exit.

**Плохой пример и требуемый отказ:** Поменять только Topic одной exact-записи или KNN target при прежних Source/Target соседних записей и числе 48: тот же независимый сравниватель должен отказать. Проверка наличия строки target где-либо в JSON не подходит.

**Предел вывода:** Не переучивать память. 19/24 — результат именно использованной сборки; source-provenance не подтверждает исполнение свежего HEAD. Восемь уроков не содержат отрицаний и вопросов: способность к ним не измерена. Наблюдённые cc01/cc04 и одинаковая уверенность 0,99 остаются фактами; снятую с очереди продуктовую правку по отрицанию не возвращать.

**Добавить к передаваемому заданию:** выполнить вышеописанную проверку через того же рабочего потребителя и общий finalizer. Сначала полный честный пакет → принятие, затем ровно одна порча → отказ с конкретным полем. Использовать существующие результаты; этот аудит не назначает Train, новый PRIMARY, изменение gold или увеличение cap. Происхождение файла, actual state и исполнявшаяся сборка называются отдельно.

[Сводка сверки](codex-2026-11-03-POST125-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [владелец в рабочем репозитории](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P25-cold-restore.md). Нижние исходные данные, результаты, команды и исторические выводы сохранены.
<!-- CODEX-POST125-PLANNING:END -->

**Дата:** 16.10.2026. **Статус:** `SOURCE-CONFIRMED CONDITION CHAIN / FORMULA-ONLY COUNTEREXAMPLE / NATIVE DOTNET NOT_RUN / SEMANTIC GAIN NOT_MEASURED / CODEX ASTRA REVIEW NOT_RUN`.
**Срез исходников:** [llm_mvp_core@216a1f3d6af3f40ceef081dc168b07e4ce3934f9](https://github.com/lubochka/llm_mvp_core/tree/216a1f3d6af3f40ceef081dc168b07e4ce3934f9), `codex/dpo-implementation`. Результат записан **только** в `xiigen-planning@llm_plan3/replanning/possibleDifs`; защищённый каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` НЕ ИЗМЕНЯТЬ.

## 0. Для человека — что может происходить

Переводчик выучил два полезных слова, но не выучил «never/not». В новом предложении он пропускает неизвестное отрицание, строит утвердительный перевод по двум известным словам, а затем проверяющий без выученных правил выдаёт `Score=1.0`. Маршрут поднимает уверенность **с 0.50 до 0.99** и возвращает перевод как `TRANSLATED`. Таким образом, **добавление частичного полезного урока** может превратить прежний честный Bootstrap/fallback в уверенное, но семантически обратное утверждение. Это не доказанный исход реальных RP01 задач, а проверяемая цепочка, состоящая из нескольких настоящих веток кода.

**Важная граница:** это `TranslationMachine → ExecutorUnit/Verifier → TranslationRoutingFlow`, НЕ прямой `RP01 Student DPO → q/v LoRA → TextGenerator`. Нельзя считать эту гипотезу объяснением neural C#500 `0/3→0/3` или EN→RU500 `0/4→0/4`, пока не доказан фактический recipient/consumer.

## 1. Source evidence: все звенья соединены

| Звено | Прямое доказательство в core | Обусловленное последствие |
| --- | --- | --- |
| Legitimate lesson writes standalone word maps | [ExecutorUnit.TrainOnExample L404–457](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L457) | Можно обучить `delete→удалить`, `records→записи` из допускаемого урока; необученное `never` не имеет соответствия |
| Partial word composition deletes unknown source tokens | [TryComposeFromWordMappings L366–401](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationTrainedUnits.cs#L366-L401) | Нет добавления неизвестного слова в результат и нет обязательной проверки полярности. `mappedTokens >=2`, `mappedRatio>=0.50` достаточно |
| Map result preempts KNN, constant confidence | [ExecutorUnit.Execute L264–329](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L329) | При удачной word composition возвращается `Confidence=0.50`, после exact и phrase, но **до** nearest-KNN. Улучшение KNN не исправит уже принятый неверный map |
| No learned verifier rules ⇒ no issues ⇒ PASS and score 1.0 | [QualityVerifierUnit.Verify L1044–1092](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationTrainedUnits.cs#L1044-L1092) | Пустая память верификатора — не доказательство верного перевода, но код возвращает `Score=1.0`, `Pass=true` |
| Confidence laundering inside ordinary adapter | [TrainedTranslationAdapter L112–141](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationDirectionAdapters.cs#L112-L141) | При `candidate.Confidence>=.5` и `verification.Pass` возвращается `Min(.99,Max(.50,1))=.99` |
| Downstream flow announces success | [TranslationRoutingFlow L77–128](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/27.TranslationDomain/TranslationRoutingFlow.cs#L77-L128) | `passed` зависит от положительного confidence и non-bootstrap AdapterId, **не** от law verdict; `OverallVerdict=TRANSLATED`, `EnhancedPrimary=.99` |

**Контрпример на уровне формул, не native receipt:** EN→RU source `never delete records`; корректный смысл запрещает удаление, но известны лишь `delete→удалить` и `records→записи`. Если source токенизируется в три translatable tokens и нет точной/фразовой пары, то `2/3=0.667>=0.50`; потенциальный текст `удалить записи` теряет отрицание. Непотренированный verifier имеет 0 rules, score 1; расчёт confidence даёт `min(.99,max(.50,1))=.99`. **На реальном `TranslationSemanticTokenizer` пример/выбор branch следует проверить, не подменять предпосылку результатом.**

## 2. Почему N35 — самостоятельный проверяемый *составной* эффект, а не переименование старых находок

Смежные доказательства уже есть, считать их авторов первичными:
- [N16-08 — фиксированные confidence и coverage не равны learning](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md).
- [N27 — негирование/порядок в KNN](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md).
- [N26 — Verifier игнорирует полярность, хотя совпадают требуемые слова](26-NEW-DIFFERENCES-ROLE-POLARITY-AND-DIRECTION-METRIC-2026-10-16.md).
- [N23 — накопление конфликтующих verifier constraints](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md).

**Узкая новая гипотеза N35:** взаимодействие *partial word-map admission* × *отсутствие полярности у неизвестных токенов* × *пустой verifier=совершенная оценка* × *max() confidence* × *product `TRANSLATED`*. Уже отмеченный KNN negation failure сам по себе НЕ объясняет этот маршрут: word-map выполняется ДО KNN. Эта карточка не претендует на доказанную абсолютную новизну среди всех 262 документов и соседних агентов без машинного полнотекстового сравнения.

## 3. Доменные границы и семантические законы

- **P1, EN→RU и RU→EN:** отрицание, запрет, запретительный императив, `never`, `not`, `without`, `no`; особенно критичны словосочетания, где две другие лексемы уже выучены. Для RU→EN применимость зависит от идентичности source tokens и пригодности target guard.
- **P1, перевод программных инструкций / SQL-синтаксиса:** «do not drop table», `NOT EXISTS`, `IS NOT NULL`. Нельзя переносить plain word-map на SQL без AST/operator-aware law: нарушение `NOT` меняет смысл. **Это перспективное отдельное доменное исследование, не утверждение что SQL production вызывает TranslationMachine.**
- **C#/Angular и Figma→HTML/CSS:** не переносить English-Russian negation check механически. Для C#/SQL нужен AST и неизменность операторов; для Figma — структурная/геометрическая эквивалентность, атрибуты, вложенность. Гипотеза N35 не универсальна.

## 4. Короткое причинное опровержение

[Рядом составлен `35-MICRO-NEGATION-WORDMAP-CONFIDENCE-FACTORIAL-2026-10-16.md`](35-MICRO-NEGATION-WORDMAP-CONFIDENCE-FACTORIAL-2026-10-16.md).

- **A (current):** 1–3 train-only урока с двумя полезными word mappings, до 12 source-family-disjoint запросов. В отчёте сохранить exact tokenizer dump; `ExecutorCandidate(Text,Confidence,AdapterId)`, `Verification(Pass,Score,Issues)`, `Flow(OverallVerdict,EnhancedPrimary)`. Если нет branch `-wordmap` — именно данный причинный след НЕ достигнут.
- **B (only one factor):** при неизменных уроках и состояниях, запрещать optimistic result, если source содержит **контрактно-критичный неизвестный negator**; выбрать abstain или дополнительный доказанный mapping. Измерить correctness и coverage на той же панели.
- **C (only confidence):** без изменения текста и Verifier pass, для пустого наборa правил вместо score=1 объявлять `NOT_ASSESSED` и не увеличивать `candidate.Confidence`; проверить калибровку и заодно убедиться, что неверный ответ всё ещё может быть возвращён (один confidence-fix не равен law-fix).
- **D (2×2):** вместе, чтобы отделить основной и interaction effect. Обязательно реальные incorrect→correct pairs и correct→refuse regressions.

**Falsifier:** N35 quality impact отклоняется, если не удаётся получить обычный word-map consumer с потерянной полярностью из **легальных** train-only уроков, если независимый экзаменатор не находит ухудшения, или если специализированный gate одинаково ухудшает качество/coverage. Истинность формулы `.99` не доказывает внедрённого улучшения.

## 5. Authority, ограничения и текущий статус

Согласно [WorkLeft-02](../../09.09.26WorkLeft-02-teaching-chinese-model.md) и прямому требованию пользователя: **CODEX на Astra 6 UltraCode исполняет человеческую роль выбора учебного случая, отдельно проверяет точную DPO-пару, и независимый CODEX/Astra 6 UltraCode оценивает ответ Student. Разрешённая ВНЕШНЯЯ ОТКРЫТАЯ КИТАЙСКАЯ МОДЕЛЬ генерирует ОБЕ стороны DPO: chosen И rejected.** Детерминированные hand-crafted fixtures здесь — ТОЛЬКО diagnostic controls, не финальный Teacher DPO. Фактического вызова этих моделей, native .NET, Student обучения, независимого CODEX review и product improvement в данной работе **не было**.

Следствие для приёмки: `SOURCE_CONFIRMED` — цепочка условий кода; `QUALITY_EFFECT` — **HYPOTHESIS**; `ACCEPTED_REPAIR` — **NO**. Не менять протокол RL/DPO, checkpoint или production code по одной формульной реплике.
