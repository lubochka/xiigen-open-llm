# MICRO-N34 — E43: проверить причину положительного replay на одинаковых данных

<!-- CODEX-POST125-PLANNING:START -->
## Адресное уточнение после опытов P118–P125 — 3 ноября 2026

Применение к этой карточке: **`PAIR-TUPLE-AND-ORIGIN`**. Источник ошибки проверен на core `9d4d79d27f18066ce3a346fac2a5eea156304f62`; это профилактическое усиление соответствующей границы, не утверждение о новом исполнении исторического опыта этой карточки.

Профилактика по исправлению E43 и оставшейся неоднозначности составного ключа project/file.

**Хороший пример и действие проверяющего:** Оба конца пары выводятся из фактического одного потока проекта и файла с позициями. Ключ проекта и файла остаётся структурированной парой; строка для показа не используется как идентичность.

**Плохой пример и требуемый отказ:** Две допустимые пары строк (project="a :: b", file="c") и (project="a", file="b :: c") не должны сливаться в один ключ. Отдельно перестановка project при прежнем тексте/числе пар обязана обнаруживаться проверяющим происхождения.

**Предел вывода:** Новые законные примеры не добавляются в TRAIN ради контроля словаря. Сохранять собственную панель и разделять эффект границы файла от повторного предъявления уроков.

**Добавить к передаваемому заданию:** выполнить вышеописанную проверку через того же рабочего потребителя и общий finalizer. Сначала полный честный пакет → принятие, затем ровно одна порча → отказ с конкретным полем. Использовать существующие результаты; этот аудит не назначает Train, новый PRIMARY, изменение gold или увеличение cap. Происхождение файла, actual state и исполнявшаяся сборка называются отдельно.

[Сводка сверки](codex-2026-11-03-POST125-TRANSFER-AND-EXPERIMENT-AUDIT.md) · [владелец в рабочем репозитории](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P91-method-pair-split.md). Нижние исходные данные, результаты, команды и исторические выводы сохранены.
<!-- CODEX-POST125-PLANNING:END -->

<!-- POST117-CURRENT-2026-11-03:START -->
## Актуальный вход после P117

[текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) фиксирует P116 как уже исполненный узкий контраст границы; он не исполняет весь factorial протокол ниже. Исторический 0.3226→0.3978 и новый 3→0 seams сохраняются с разными владельцами и границами. Документ не назначает повтор P116 или очередной PRIMARY.

Перед отдельно разрешённой причинной попыткой зафиксировать stable lawful pair IDs и replay-кратности: одинакового seed для разных списков недостаточно. Общий ledger включает диагностические экземпляры памяти, replay и все физические попытки. Источник обоих концов проверяется по trusted project/file/position/content; итоговый false/missing контроль даёт отказ. Null receipt или неизвестное состояние дерева процессов не допускают завершённый verdict. Текст, raw и дозы старого опыта не меняются; чистый ремонт измерителя и учёта выполняется до новых запусков.
<!-- POST117-CURRENT-2026-11-03:END -->


**Цель человека:** выяснить, действительно ли исторический механизм запоминал переносимые предметные закономерности, или часть успеха была связана с искусственными парами между файлами и повторным предъявлением уже виденных биграмм. **Продолжительность рабочего объёма:** один узкий локальный .NET runner/experiment, без Windows CI, без нового model-server и без переобучения основного LLM. Это **проект**; native запуск в ходе данного исследования НЕ выполнялся.

**Связанные доказательства:** [N34 source review](34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md); [E43 исходник](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L16-L105); [E09 аналогичный, но другой механизм](32-NEW-E09-CROSS-FILE-TRANSITIONS-AND-IGNORED-SOURCE-2026-10-16.md). Зафиксированный historical E43 Tier2 = 0.3226→0.3978 **не** используется как ожидаемое значение нового запуска.

## 0. Заранее зафиксировать что проверяется

- **EXPERIMENT ID:** `MICRO-N34-E43-BND-01`.
- **Baseline:** существующее извлечение единым списком и `Zip` поверх всех файлов, `E43ReplayBuffer.ExtractPairs`.
- **Enhanced:** внутрифайловое извлечение `perFile.SelectMany(predicates.Zip(predicates.Skip(1)))`, без внешних переходов.
- **Primary metric:** `retention_accuracy` на **одной и той же** независимой old-domain панели реальных предикатных переходов в никогда не обучавшихся `source_file_id`. Одинаковая evaluation функция для всех arm.
- **Secondary:** `boundary_pair_count`, `duplicate_pair_key_rate`, `retention_delta_vs_zero_replay`, `old_base_retention`, `stability_by_file_order`, `law_correctness_on_holdout` при наличии domain oracle, CPU и время.
- **Область:** исторический CODE_Q/SECURITY bigram + `LogicExtractor`; **не** ENRU/LoRA и **не** общая предметная пригодность LLM.
- **Фактор:** только формирование пар на границе файла. Ratio replay отдельный, заранее фиксируемый фактор 0% и 30%; не менять tokenizer, dictionary, validation evaluator и source files между arms.
- **Tier 1:** для проверки кода 4–8 искусственных файлов, 12–40 токенов — обнаруживает pseudo transitions, **не меряет обобщение**.
- **Tier 2 (минимум для предметной гипотезы):** ~16–30 допустимых исходных файлов с исходными path и source-family ID; минимум 4 old train families, 2+ old heldout families и 2+ new train families; если material меньше — `INSUFFICIENT_VALIDATION`, не `FAILED_HYPOTHESIS`.

## 1. Прозрачный пример ожидаемого результата

Есть два файла A: `[NULL_CHECK, RETURN]` и B: `[SQL_READ, SAVE]`. Текущий `ExtractPairs` даёт три перехода:
`NULL_CHECK→RETURN` (из A), **`RETURN→SQL_READ`** (между файлами; не было в исходной задаче), `SQL_READ→SAVE` (из B).
Контроль B даёт **два** перехода и не придумывает связь RETURN→SQL_READ. Если это действительно важная ошибка учебного материала, downstream качество на неизменных, ранее не виденных файлах должно отличаться. Если улучшения нет — source-разрыв найден, но **влияние на качество не подтверждено**.

## 2. Исполнитель: подготовка source и изоляция

**Действующий вход исполнителя:** прочитать полную карточку,120§1/40§2/42/44 и нынешний срез; сначала разобрать saved P116, все физические попытки и source-derived65 Learn/run. Новый PRIMARY, factorial contrast, перестановки файлов или ratio trials не запускаются из этой секции автоматически. Следующие шаги относятся лишь к отдельно назначенному опыту с заранее зафиксированным объёмом; gold/raw/дозы P116 остаются прежними. Ошибка JSON или отчёта сначала исправляется на сохранённых данных.

1. Прикрепить `core_sha` (не просто имя ветки), `source_manifest_sha`, `code_sha`, `experiment_id`; хранить исходный `Path` и `source_family` у всех файлов.
2. **До** обучения разбить *файлы*, не отдельные переходы, на `oldTrainFiles`, `oldHeldoutFiles`, `newTrainFiles`, `newHeldoutFiles`. Зафиксировать 3–5 preset splits с разными seed и стратификацией по домену/проекту; никакая пара из holdout не может участвовать в trainer/replay.
3. Запустить `LogicExtractor.Extract(text)` **один раз** на source-файлах. Сохранить последовательности `source_file_id→[predicate ints]`. Не менять состав токенов/версию LogicExtractor в ходе опыта; одинаковые token IDs across arms.
4. Сформировать `currentConcatPairs` и `withinFilePairs` из одинакового train корпуса. В диагностике сохранять project/file/position/hash обоих концов, `left_predicate`, `right_predicate`, `boundary=` и кратности. Проверять оба конца по trusted streams, а не по их самообъявлению. Lawful replay заранее связывается со стабильными IDs пар и кратностями, не с индексами списков разной длины; один seed это равенство не доказывает. `source_file_id` и project служат проверке происхождения, не добавляются в predictor без отдельного фактора.
5. Отложенные true пары извлекать **внутри holdout файла** для всех arm. Следить, чтобы ни один holdout source family не проник через дешёвую проверку «похожая формулировка».

## 3. Четыре ветки одного factorial contrast

| Arm | Пары из train | Replay старого | Что сравниваем |
| --- | --- | ---: | --- |
| A0 | current concatenated/Zip | 0% | baseline без replay |
| A1 | current concatenated/Zip | 30% | прежний положительный механизм |
| B0 | only within-file | 0% | убрали boundary, без replay |
| B1 | only within-file | 30% | изолировали replay без invented transitions |

`A1−A0` и `B1−B0` сравниваются **на одинаковых frozen oldHoldoutPairs**. `B1−B0` является искомой устойчивостью 30% replay при очищенном источнике. `A1−B1` сам по себе недостаточен: объёмы train-массива различаются; сохранить counts и выполнить дополнительный capacity-matched subsample при крупном эффекте.

**Важное разграничение:** допустимая метрика — heldout top1 на биграммах. Не выдавать её за accuracy C# documentation generation; для последующего продуктового решения потребуется **новый** перевод/Flow/LLM consumer experiment.

## 4. Негативные и причинные контроли

- **File order:** переставить файлы *только внутри* oldTrain или newTrain, не меняя их принадлежности; повторить A0/A1/B0/B1, fixed RNG для sampling. В B полученные `(prev,next)` пары должны быть неизменны с точностью до порядка; в A boundary-pairs могут смениться. Если итоговый gain «плавает» с порядком — существенный аргумент против прежней интерпретации.
- **Exact-pair overlap:** сохранить `fraction(heldout pairs exactly present in replayOldTrain or newTrain)`, отдельную `nonoverlap_accuracy` и её знаменатель. Если почти всё improvement лишь на повторных биграммах — это retention/memorization, не cross-family generalization.
- **0/10/30% ratio:** 10% разрешено только во втором этапе и на прежнем holdout; не выбирать «лучший» ratio по результату закрытой панели. Позитивное отличие 30% от нуля само по себе не доказывает оптимальности 30%.
- **Shuffled labels:** на диагностической копии сломать связи old transitions, но оставить числа pairs/weights. Если «улучшение» сохраняется вопреки разрушенной структуре, перепроверить метрику и data leak.
- **Per-project limits:** если домен имеет синтаксическое дерево (C#/TS), хранить `fileId`, `AST parent` / role в будущих кандидатах; **нельзя переносить** этот признак в RU/EN текст без отдельного исследования POS/word order.

## 5. Результат и обязательная честность

Каждый arm пишет: `sourceManifestSha, oldTrainFilesSha, heldoutFilesSha, extractedPredicateSeqSha, pairIDs, trainCount, replayCount, boundaryCount, overlapCount, accuracyNumerator, accuracyDenominator, accuracyNonOverlapNumerator, accuracyNonOverlapDenominator, trainingMs, evaluationMs, executableCommit, verdict`.

Пример правильного ответа после выполнения: «На 18 независимых C# файлах B1 сохранил 7/12 правильных old-holdout переходов, B0 — 5/12; разница +2/12 наблюдается на paired panel, нулевая гипотеза пока не исключена из-за малого материала». **Это шаблон, а не якобы полученные числа.**

Если baseline и enhanced равны или результаты нестабильны, решение `HYPOTHESIS_NOT_SUPPORTED_AT_THIS_TIER`. Если `old_base_retention==0`, retention experiment `NOT_INFORMATIVE`; сначала добиться хотя бы одного честно измеренного old skill. При слабой статистике `SCALE_UP_REQUIRED`, а не переписывать заранее цель или объявлять `FAILED`.

## 6. Границы полномочий и техническая реализация

Операции выполняются только в отдельно назначенном срезе и изолированном runner/feature branch. Текущий ремонт отчёта, ledger, parser и finalizer использует сохранённые данные без обучения. Если назначен новый причинный опыт, локальный `ExtractPairsPerFile` сохраняет исходную функцию и тот же `BuildBigram/Evaluate`; actual diagnostic/scientific/replay Learn и все упавшие попытки входят в общий расход. Null/неверная схема receipt, unknown дерево процессов и любой missing/false обязательный control дают отказ; это предварительные чистые проверки, не повод повторить модель. Дозы small fixture/tier2 и дальнейшие варианты задаёт само назначение, не успешность предыдущего шага. Без GitHub Windows CI и переписывания production model. До реализации проверить [experiment-design](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md), [code-examination](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/code-examination-SKILL.md) и настоящего caller/consumer.

## 7. Что НЕ является доказательством

- `passed:true` старого Tier2 JSON не подтверждает перенос в RP01.
- Изменение скалярного `retention_accuracy` при **разных** oldVal не является paired эффектом.
- Source-only proof `Zip` не подтверждает, что именно он испортил конкретный пользовательский ответ.
- Успешное восстановление checkpoint или 0 падений dotnet tests не подтверждают качество на новых доменах.

**Результат сегодняшней работы:** подготовлены protocol и [малый воспроизводимый source-proxy](34-E43-FILE-BOUNDARY-SOURCE-PROXY.mjs), native .NET consumer/learning **НЕ ЗАПУСКАЛСЯ**.
