# Эксперименты после приёмки фразового пути: цена селектора и происхождение результата

Дата: 12 ноября 2026. Проверяемый core: `f089a7d55d359ff103d0cdd9ddfb26eee346a16b`; planning: `60a43da35df6452299134e1827014d6dc06e97e0`. Автор проверки: `/root/experiment_audit`, модель унаследована от родительской сессии; отдельная модель или независимый native runner не заявляются.

**Новый результат:** единственный правильный ответ P58 после коррекции принадлежит уже предъявленной corrective строке; полный выбранный test class оплачивает существенно больше обучения, чем основной Fact. Исправление этих границ сохраняет узкие положительные результаты и не превращает их в доказательство обобщения. Новые численные выводы получены пересчётом закреплённых данных и чтением текущего исходника, без запуска ученика.

## 1. Свежая проверка и её границы

В этой проверке материализованы и проверены по Git blob SHA **29 входных файлов**: 25 сохранённых JSON P48/P49/P50/P52/P58 и четыре текущих C# файла P49/P52/P58. Часть входов служит контрольной привязкой истории, а не отдельным повторно исследованным опытом. Выполнен новый [Python-пересчёт](https://github.com/lubochka/llm_mvp_core/blob/246446cc57ad3e0975abceeb926336a0f4c1b072/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-post-admission-experiments.py); [manifest](https://github.com/lubochka/llm_mvp_core/blob/246446cc57ad3e0975abceeb926336a0f4c1b072/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-post-admission-inputs.json) содержит точные исходные bytes/hashes, [результат](https://github.com/lubochka/llm_mvp_core/blob/246446cc57ad3e0975abceeb926336a0f4c1b072/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-post-admission-recomputed.json) — построчные выводы и условную цену каждого Fact. Скрипт отказывается от другого source blob и перезаписи уже существующего output.

Свежо пересчитаны paired outcomes P48, exposure и адрес единственного выигрыша P49, cold/constant/answer/address P52, corrected-item/untouched partition P58. Для P52/P58 рассчитана полная цена выбранного класса по методам и helper-вызовам, исключая комментарии и строки из подсчёта. **C# build, discovery, native tests, training, fresh state load и физические счётчики здесь не выполнялись.** Числа 16 и 2 ниже — исходные `[Fact]` declarations, не число обнаруженных или пройденных случаев. P50 raw только закреплены; новый отдельный P50 verdict не объявлен. WC01 и несогласованность loss/gradient сохранены как результаты прежнего аудита, без заявления нового воспроизведения.

После первоначального чтения обнаружена локальная ошибка материализации больших base64-файлов: разделение по символам до удаления переносов могло дать неполный блок. До расчётов все 29 файлов материализованы заново с нормализованной base64 и сверены по точным Git blob hashes. Это исправление локального чтения, не дефект репозитория; исторические raw не менялись.

## 2. P58: после corrective lesson EXAM меняет состав

Сохранённая попытка `P5801/results/attempt-0002` даёт следующую цепь:

| Фаза | Строка и наблюдение |
|---|---|
| После setup, до correction | `e1`: `Anna opened the window`; gold `Анна открыла окно`; actual `Анна открыла дверь`, adapter `executor-en_ru-ordinary-source-knn` |
| Выбор feedback | Первая неверная строка `e1` передаётся в `RunArm` как wrongExamSource/wrongExamGold/actualWrongAnswer |
| Обучение A/B/C | `BuildCorrectiveBatch` получает тот же source и gold; меняется исследуемая rejected-side интервенция |
| После correction | Во всех A/B/C только `e1` верна, adapter `executor-en_ru-exact-source`; `e2…e8` неверны |

Исходные данные: [before.json](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/experiments-v2/RP01-MICRO/P5801/results/attempt-0002/before.json), [arms.json](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/experiments-v2/RP01-MICRO/P5801/results/attempt-0002/arms.json). Текущий выбор строки — [test L670–692](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs#L670-L692); передача source/gold в builder и TrainBatch — [L530–550](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs#L530-L550).

Следовательно, прежние **1/8** остаются верной арифметикой общей панели, но после corrective training раскладываются на **1/1 corrected-item recall + 0/7 остальных непредъявленных строк**. Все три плеча имеют такое разложение. Это полезное локальное усвоение; оно не доказывает перенос на новые запросы. Оставшиеся семь не названы независимыми по source-family: такая независимость здесь отдельно не установлена. Пригодность rejected counterexample 0→1 также сохраняется как отдельная ось.

**Адресный ремонт измерения:** хранить на каждом readout `RowId`, фактический input/key, phase/arm/checkpoint и consumed lesson IDs; после feedback переклассифицировать обученную строку и эквивалентные алиасы. Сохранять неизменные Answer/Gold и прежнюю общую панель, дополнительно выводить corrected-item, untouched-input и отдельно установленный family-disjoint результат. Если untouched denominator нулевой, значение `UNMEASURED`, а не 0%.

**Различающая приёмка без нового Train:** в синтетическом ledger одной correction соответствуют 1 corrected и 7 untouched; иной RowId того же model-visible input не создаёт независимую задачу; замена только target не делает input новым; действительно новый input остаётся untouched. Это проверка учёта, а не новый результат обучения. Native обычный потребитель и свежая загрузка для нового учебного эффекта остаются отдельной назначенной проверкой.

## 3. P58: полный класс и основной Fact имеют разную цену

Текущий test source содержит 16 Facts. При **однократном полном успешном прохождении каждого**, без retries и ранних выходов, получается:

| Метод / группа | Unit lesson calls | Прямые output reads | Основание |
|---|---:|---:|---|
| `PreparedCorpusLineAndActualWrongOutputAreMeasuredOnBothAxesSeparately` | 105 | 56 | setup-probe 24; три плеча по 24+3; cold/setup/TRAIN controls/три EXAM |
| `TheSameRejectedSourceAlwaysGivesTheSameLessonAndOnlyTheFactorChangesIt` | 132 | 40 | setup-probe 24; четыре дополнительных плеча по 27; probe8 + arms32 |
| `ConstantAnswerCeilingIsAnalyticalAndStableAcrossRepeatedComputation` | 24 | 0 | отдельный TrainBatch для проверки независимости analytical ceiling |
| `ConstantAnswerCeilingIsNotZeroAndIsNotTheObservedColdPanelCount` | 0 | 8 | отдельное cold чтение панели |
| Остальные 12 Facts | 0 | 0 | нет learner calls в просмотренных телах этих Facts |
| **Весь класс** | **261** | **104** | сумма указанных методов |

Источники: [RunArm L505–550](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs#L505-L550), [repeat-control L1175–1205](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs#L1175-L1205), [ceiling controls L1343–1378](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs#L1343-L1378). Команда внутри отчёта [L1154](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs#L1154) выбирает весь класс.

105/56 не ошибка как subtotal main. Ошибка возникла бы при выдаче этого subtotal за цену полного class selection. **261/104 не приписывается историческому процессу:** нужны его selector, discovery, terminal, TRX и физический ledger. Ранний отказ может уменьшить исполненный объём; retries — увеличить. Один lesson здесь равен вызову `TrainOnExample` конкретной Unit; один output — прямой Tokenize→Execute. Это не 261 уникальная пара и не 104 полноценных TranslationMachine Flow.

Проверка `ComputeConstantAnswerCeiling` сама аналитическая; training-bearing Fact её независимости всё же тратит 24 lessons. Поэтому название метода или слово analytical не даёт нулевую цену всего теста.

## 4. Та же граница в P52

У P52 два Facts. Основной вызывает два плеча: **12 lessons / 12 outputs**. Повторный контроль вызывает ещё четыре плеча: **24 lessons / 16 outputs**. Однократный успешный весь class: **36 / 28**. Обратные чтения tokenizer в плечах — часть этих же output chains, поэтому они не добавлены ещё раз к 28.

См. [main L373–409](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs#L373-L409), [repeat-control L736–756](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs#L736-L756), [RunArm L180–325](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs#L180-L325).

Повторно подтверждён прежний открытый writer-дефект: cold0/4 сохраняется в поле constant ceiling; независимый histogram даёт2/4. Оба обученных плеча отвечают4/4, tense-address1/4→3/4. Это подтверждение прежнего остатка, а не новая находка и не причинное улучшение качества перевода.

**Приёмка общего бюджетного ремонта:** получить exclusive attempt owner до любого выбранного training-bearing Fact; зафиксировать FQN selection/discovery; счётчики run/FQN/stage/recipient обновлять по реальным операциям. Расширение selector с P58 main на весь class добавляет расчётные156 lessons/48 outputs; тот же шаг в P52 добавляет24/16. Искусственно добавленный setup вызов обязан появиться в фактическом ledger. При раннем отказе записываются достигнутые, а не плановые суммы. Чистые reporting/ceiling проверки не должны случайно захватывать Train из общей fixture или широкого фильтра.

## 5. P48/P49: уточнённый адрес эффекта

- **P48:** прежние12/16 у обоих подтверждены, но 6 ответов различаются: gains `exam-t1`, `train-f1-00`, `train-f1-02`; losses `exam-t3`, `train-f3-06`, `train-f3-08`. Равный aggregate не означает равное поведение; это три выигрыша и три потери. Назначенное weighted shoulder не подменяется mechanism control.
- **P49:** cold EXAM exposure =2 exact-pair seen +1 seen-input/new-target +1 unseen-input; warm+new =4+0+0. Единственная прибавка на группе нового проекта B — `c-f3-p2` (input3→target0), **уже предъявленная new-TRAIN пара**. Выигрыш7/8 против6/8 полезен на своей оси; он не является прибавкой unseen transfer. Negative control1→5 при TRAIN1→0 остаётся seen-input/new-target, хотя текущий textual overlap verdict называет такую ситуацию незнакомыми входами.

Данные и идентичности строк включены в JSON пересчёта. Текущий P49 helper [L248–268](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/experiments-v2/RP01-MICRO/P4901/src/FinitenessGovernedVerdict.cs#L248-L268) и его контроль [L998–1007](https://github.com/lubochka/llm_mvp_core/blob/f089a7d55d359ff103d0cdd9ddfb26eee346a16b/Tests/ExperimentFixes/P4901FinitenessGovernedWarmStartTests.cs#L998-L1007) сохраняют известную проблему словесной новизны. Новая точность этого дополнения — конкретный RowId выигрыша, а не повторное объявление старого дефекта.

## 6. Изменённые постановки

С сохранением всех исходных bytes добавлены датированные разделы в три planning-файла:

1. `codex-2026-11-12-audit-card-hardening.md` — снята применимость старого «ordinary caller не подключён» после b64ba198; очередь возвращена к владению попыткой/terminal/открытым измерителям.
2. `60-01-NEW-CURRICULUM-FEEDBACK-AND-UNIT-MEASUREMENT-SEAMS-2026-10-19.md` — конкретные261/104 и тесты selector-cost, достижимый partial ledger, exposure после correction и aliases.
3. `33-MICRO-RECIPIENT-AND-METRIC-QUALITY-EXPERIMENT-2026-10-16.md` — four separate recall/transfer denominators, paired gains/losses, запрет скрытого обучения в scorer-only блоках, сохранение малых полезных результатов.

Это ремонт постановок. Реализация runner/writer и новые native результаты данным документом не объявляются выполненными.


## Позднее дополнение 12 ноября: полная привязка пересчёта и новый F03

Первоначальная проверка выше и её ссылки на core `246446cc` сохраняются как история. После независимого review в [окончательный manifest](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-post-admission-inputs.json) добавлены три producer helper: теперь закреплены **32 входа — 25 raw JSON и 7 C# источников**. Множители уроков выводятся из этой проверяемой цепи. [Отдельный результат v2](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-post-admission-recomputed-v2.json) сохраняет численные выводы P48/P49/P52/P58; опубликованный v1 не переписан. Условная цена полного успешного однократного выбора остаётся P58 **261 Unit calls /104 direct reads**, P52 **36/28**; это по-прежнему не исторические actual counters и не native discovery.

Термин «остальные/untouched строки» в первоначальном изложении ограничивается **буквальным Source относительно одной corrective строки**. В v2 это выражено полями `other_literal_source_*` и явным `partition_rule`. Независимость aliases, нормализованных model keys и source-family не доказана. 1/1 на исправленном e1 через exact-source и 0/7 на остальных literal Source не переименовываются в измеренный перенос на независимый held-out набор.

Перед итоговой публикацией core продвинулся на `f95a96b`: общий sink уже получил claim/occupied-check/single-shot запись. [Поздняя source-проверка](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-late-ownership-review.md) сохраняет этот прогресс и уточняет остатки: получение sink обычным P58 после Train, запись после CompleteAttempt, case-sensitive containment и обход single-shot через выданный путь. [Проверка сохранённых TRX](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-late-ownership-evidence.md) подтверждает 14/14, 14/14 и reviewer13/13 в точном составе; 13 helper результатов не означают regression тринадцати ordinary callers.

Полный актуальный [аудит](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-audit.md) и [адресный план](https://github.com/lubochka/llm_mvp_core/blob/9dc7f71c775506ec7eb0d5186afec82ddada0444/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/post-admission/codex-repair-plan.md) опубликованы в core `9dc7f71c775506ec7eb0d5186afec82ddada0444`. Новые native тесты и обучение этой проверкой не выполнялись. Карточки N33/N60 сохраняют внесённые требования к exposure и полной цене selector; это дополнение уточняет evidence и актуальный источник следующей работы.
