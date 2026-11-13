# N60-01 — две новые уточнённые гипотезы динамики выбора уроков и отдельная ловушка метрики Unit

<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-N60LEDGER:START -->
## Расход и достижимый отрицательный исход — уточнение 11 ноября 2026

Конкретизация уже добавленного EV-P58, без смены H60-A/B и без нового запуска. Physical ledger хранит owner/stage, source rows, batch calls, предъявления каждой Unit, optimizer updates, EXAM/TRAIN readouts, save/load и external calls. Setup-probe, A/B, instrument control, mutations и другие Facts — отдельные строки. Отказ не заполняет будущие счётчики запланированными числами.

Healthy main P58: cold — 16 Unit-chain readouts; setup-probe — 24 Unit lessons и 16 readouts; A/B/C — по 27 lessons и 8 readouts. Сумма 105/56 — source-derived subtotal одного Fact, не число уникальных пар, не полные TranslationMachine Flow, не цена класса. No wrong row после установки даёт NO_CONTRAST, 0 corrective calls и фактический setup/control расход.

Negative cases проходят **executor→sink→outer terminal**: отсутствие acquisition, unequal-start, instrument failure, no wrong row, late assertion, missing TRX. Сначала сохранить partial raw/status/reason, затем завершить внешний путь. Сериализация заранее созданного invalid object не доказывает достижимость записи. Пригодный отрицательный научный исход не превращается в INVALID, сломанный измеритель не поддерживает гипотезу.

Root/owner выделяется до preparation/Train. Bounded exit 0 и journal-only сводка без raw не подтверждают полноту native run. Lifecycle проверяется искусственным child без модели; научный повтор получает отдельный явный расход. [Core договор](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/03-test-and-execution-contract.md).
<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-N60LEDGER:END -->


<!-- XIIGEN-AUDIT-2026-11-11-INSERT -->
<!-- AUDIT-2026-11-11-EV-P58:START -->
## Уточнение feedback evidence и полного расхода — 11 ноября 2026

H60-A/B ниже сохраняют собственные temporal-policy гипотезы. Последующий P58 на core `009bf226174441fa0fbf0336e74ad2f2b7174d92` иллюстрирует три разные границы: current source уже считает constant ceiling1/8 отдельно от cold0/8 и доказывает setup learning по TRAIN0/8→8/8; пригодность counterexample0→1 не дала различия A/B state или ответов; ранние asserts main Fact всё ещё могут прервать запись invalid terminal receipt. Старый ceiling-дефект не назначать заново как открытый.

Для ожидаемой трассы feedback→lesson→native state→ordinary output добавить сохранение actual wrong row и terminal-документа на no-gain, unequal-start, instrument-failure и no-errors ветках; правильный serializer сам по себе не доказывает достижимость записи. В учёт включать каждый физический setup/probe/control. У P58 attempt2 derived total105 Unit lesson calls=24+3×24+3×3 и56 наблюдений цепочки Tokenize→Execute дополняют прежние24/3 на плечо. Это расход одного main Fact, не105 уникальных пар, не56 полных TranslationMachine Flow. Для полного class/attempt/lifetime105 — лишь нижняя граница: другие Facts тоже обучают, общий расход требует selector/TRX. Подробности — [дополнение11.11](2026-11-11-P48-P58-EXPERIMENT-MEASUREMENT-ADDENDUM.md). Пересчёт read-only, свежего native PASS нет; H60-A/B этим уточнением не запускаются.
<!-- AUDIT-2026-11-11-EV-P58:END -->


Дата: 19.10.2026. Источник: [TatoebaPilotLoopRunner.cs @f50b710](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TatoebaPilotLoopRunner.cs). Статусы: SOURCE_CONFIRMED для конкретной последовательности строк; HYPOTHESIS для эффекта полезного предметного обучения; TRAINING_AB=NOT_RUN, RP01_DIRECT_CAUSALITY=NOT_PROVEN. Это **не** дефект LoRA Student и не источник права менять protected core. Исторический донор PrepareToGPL и C# kNN разобраны в [N60-00](60-00-RP01-SUCCESS-LINEAGE-AND-GAP-ATLAS-2026-10-19.md).

## H60-A — «лучший когда-либо score» делает допуск уроков нечувствительным к ухудшению обучения

### 1. Прямая цепь к настоящему обучению

1. [Цикл 403–419](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L403-L419): решение о promotion принимается, затем измеряется текущий RuntimeProductAccuracy, но переменная обновляется через Math.Max(bestObservedRuntimeProductAccuracy, report.RuntimeProductAccuracy). После падения качества historical best остаётся прежним.
2. [Threshold 796–803](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L796-L803): MinimumNewPatternRate = 1-best; MaximumSeenTokenRate = best. При большом историческом best порог нужной новизны НИЖЕ, а допускаемая доля уже известных токенов ВЫШЕ.
3. [ShouldPromote 773–779](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L773-L779): условие NewPatternRate > 1-best И SeenTokenRate < best; оба требования с ростом best становятся мягче.
4. [Учебный consumer 444–454 и 747–770](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L444-L454): выбранные строки попадают в TrainRowsAsync, BuildGeneralBatch и **реальный TranslationMachine.TrainStructuredBatchAsync**; промотированные lessons меняют память и ordinary Flow. Это **не** только замена внешней метрики.

### 2. Математически проверяемый контрпример — НЕ результат теста

Если historical best был 0.90, а настоящий текущий score снизился до 0.50, кандидаты с NewPatternRate=0.40, SeenTokenRate=0.70 проходят **старый** допуск (>0.10, <0.90) и не проходят допуск на актуальном score (>0.50, <0.50). Это демонстрация различия функции на согласованных значениях, **не измеренный случай репозитория**. Условия появления такого кандидата на реальных корпусах и полезность отбора ещё не доказаны.

### 3. Отличие от ранее найденных тем (dedup)

- [P58.4 protected](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P58-feedback-learning.md) и [N23 scorer experiment](24-MICRO-TATOEBA-QUALITY-GATE-EXPERIMENT.md) меняют **сам ProductMatch scorer** при тех же thresholds; настоящая H60-A меняет **временной источник значения accuracy (historical best vs latest real score)** при одном и том же scorer.
- [P65 history admission](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P65-history-admission.md) меняет Jaccard/sequence guard, но не исторический score→две novelty inequalities.
- [N59 third-step](59-03-NEW-INTERACTION-HYPOTHESES-AND-FAST-LEARNING-CARDS-2026-10-19.md) измеряет долговременное сохранение законов при новых lessons, не конкретную форму управляемого schedule.

**Новизна узкого пересечения относительно изученных prior docs — кандидат, не абсолютное заявление**: в сотнях документов могли быть дополнительные формулировки сходного эффекта. Репликационная карточка не должна выдавать прежнюю абстрактную идею feedback gating за новое открытие.

### 4. Границы

Это может быть **намеренный curriculum**: после одной сильной performance отметки система сознательно допускает менее новый материал. Отсюда НЕ следует ошибка: ухудшение качества не всегда означает, что строгий admission улучшит обучение. Нужно сравнить реальное распределение следующих уроков и *после применения* — ordinary Flow. Профиль: перевод EN→RU и RU→EN отдельно. C# AST/SQL сюда без своего текущего Tatoeba trainer не включать. Перед экспериментом доказать, что historical score и latest score действительно расходятся на frozen DEV, а выбранные lesson families способны менять Unit.

## H60-B — обучение выбирается до оценки текущих кандидатных уроков: межцикловая задержка политики

Прямая source цепь:
- В [loop L403–415](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L403-L415) promoteToTraining вычисляется ДО RunCycleAsync.
- В [RunCycleAsync L697–744](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L697-L744) читаются ordinary outputs, вычисляется RuntimeProductAccuracy и формируется report.
- Затем [loop L417–454](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L417-L454) best обновляется и только потом программа обучает выбранным ранее batch (если не выполнен PassesTarget).

**Гипотеза:** при резком сдвиге качества результат нового «экзамена» может требовать иной curriculum admission уже на ТЕКУЩЕМ batch, но это станет видно только после предварительного решения. А/B вопрос независим от A: *когда* оценка обновляет решение (до или после scoring), даже если правило выбора historical best оставить одинаковым в обоих плечах. В отличие от source bug, это может оказаться корректной online policy (запрет смотреть exam score до выбора lesson нужен от утечки), поэтому не считать новообнаруженным нарушением без контракта. Правильный вариант B может использовать лишь **разрешённую учебную feedback DEV-панель**, не SEALED экзамен; вся оценка на EXAM остаётся полностью вне обучения и не влияет на решение. Исполнитель сначала доказывает законность данных B, иначе маркирует H60-B NOT_ELIGIBLE и не запускает.

**Отличие от H60-A:** A — historical maximum не убывает даже между циклами; B — временная задержка одного конкретного решения, при замороженной самой формуле best. Нельзя менять обе оси в одном сравнении и приписывать улучшение одной.

## S60-M — отдельная ловушка внутреннего измерения: обученный SemanticTokenizer bypass в unit-only accuracy

В [ExecutorUnit.MeasureAccuracy](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TranslationTrainedUnits.cs#L470-L500) создаётся **new TranslationSemanticTokenizer()** и вызывается Execute напрямую. В ordinary [TrainedTranslationAdapter.TranslateAsync](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TranslationDirectionAdapters.cs#L115-L140) Executor получает результат от *обученного* SemanticTokenizerUnit. Следовательно, Unit-only internal exact-match measure и ordinary Flow могут расходиться при ненулевых trained corrections.

**STATUS:** source-confidence C; расхождение quality зависит от конкретных lessons. В protected [P57 semantic measurement](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P57-semantic-measurement.md), P106/P51 уже покрыто много похожих несоответствий; этот cross-method owner не объявляется безусловно новым. **Запрещён отдельный эксперимент «изменим только формулу и измерим accuracy», потому что это НЕ обучение**. Сначала найти реальный consumer этой Unit метрики, определяющий следующий TrainOnExample/admission; если такого нет — только SOURCE_NOTE, без token-consuming learning experiment. Не подменять данные эмпирической оценки результатом scalar ExportAsync, который обучённые state не восстанавливает.

## S60-C — дополнительные ограничения измерителя, НЕ новые гипотезы обучения

- [TranslationProductHeldOutRunner PassesGate](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L214-L260) требует внутреннюю прибавку accuracy >=.02 и ещё несколько внешних порогов. Это **продуктовый gate**, не корректное определение результата исследования пользователя. Реально измеренная полезная экономия RAM/времени/уроков — успех по своей оси даже если PassesGate=false, с явным учётом ухудшений. Источник эффекта на **следующую** native тренировку данным кодом пока не доказан — никакого отдельного scorer-only опыта.
- [TranslationMachine.TrainBatchAsync](https://github.com/lubochka/llm_mvp_core/blob/f50b710a5eaa6a8f0468b07d5b2510342a1de7ba/27.TranslationDomain/TranslationMachine.cs#L55-L87) increment .01 без Unit update — ранее установлено P58.3; не повторять как новое.
- Any historical improvement (включая «FAILED» <5%) сохраняется как факт улучшения соответствующей полезной метрики/домена, без автоматического права переносить вывод на иной Unit.

## Ожидаемое доказательство для A/B и условия опровержения

Полная карточка и промпт — [N60-02](60-02-REAL-LEARNING-MICROEXPERIMENTS-2026-10-19.md). Должна быть трасса prior score → выбранные candidate lesson IDs → actual TrainRowsAsync → writes конкретного Executor/Semantic/Verifier → ordinary Flow before/after. Если admission различается, но изменённый набор lessons не доходит до нового trained state, либо не меняет ни одного полезного учебного показателя — quality effect НЕ ДОКАЗАН. Если current score не падает относительно best, H60-A нерелевантна данному прогону. Если изменение очередности использует SEALED exam в обучении, H60-B невалидна, не запускать.

<!-- CODEX-POST-ADMISSION-20261112-N60:START -->
## Конкретизация цены выбранного класса и судьбы EXAM после feedback — 12 ноября 2026

Основание: пересчёт P58 `attempt-0002` и source `Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs` на core `f089a7d55d359ff103d0cdd9ddfb26eee346a16b`. Это уточнение измерения и наряда; H60-A/B не запускались. Сводка и воспроизводимый расчёт связаны в [дополнении](codex-2026-11-12-post-admission-experiment-review.md).

**Материал меняет статус после обратной связи.** В P58 выбранная ошибочная `e1` = `Anna opened the window` → `Анна открыла окно` становится corrective lesson. После него именно `e1` даёт единственный правильный ответ всех A/B/C через exact-source. Полная панель 1/8 остаётся историческим наблюдением, но новое разложение обязательно: corrected-item recall 1/1; untouched rows `e2…e8` — 0/7. Это не отменяет полезное усвоение `e1` и не квалифицирует оставшиеся семь как source-family-disjoint без отдельной проверки.

Для H60-A/B закрепить четыре поля на каждом readout: `RowId`, фактический model-visible input/key, `exposure_before_read`, список consumed lesson IDs. Состояния допуска: предъявленный exact input/target; знакомый input с новым target; ещё не предъявленный input. Отдельный признак фиксирует использование примера для feedback/подбора, даже если Train ещё не вызван. После корректирующего урока обработанная строка и её эквивалентные алиасы больше не входят в untouched denominator. Прежний score не переписывается: публикуется новый срез на конкретной фазе и неизменном остатке.

Приёмка измерителя без нового обучения: добавить synthetic correction для одной строки восьмёрки — число untouched становится 7, corrected-item 1; изменить только gold при прежнем input — новизна input не растёт; дать два RowId одному фактическому input — обе записи помечаются как уже предъявленные, а число независимых задач не увеличивается. При пустом untouched наборе выводить `UNMEASURED` и denominator 0, а не «0% качества». Положительный контроль оставляет действительно новый input в untouched. Изменение принадлежности строк не должно менять сами сохранённые Answer/Gold.

**Цена относится к точному селектору.** В текущем P58 16 Facts. При прохождении каждого ровно один раз до конца main требует 105 lessons/56 outputs; повторный контроль `TheSameRejectedSourceAlwaysGivesTheSameLessonAndOnlyTheFactorChangesIt` — 132/40; проверка независимости analytical ceiling — 24/0; cold-ceiling контроль — 0/8; остальные — 0/0. Итого 261/104. Это условная source-арифметика, а не наблюдённый счётчик. Ранний отказ, другой фильтр, retries и parallelization требуют собственных фактических счётчиков. Урок означает вызов `TrainOnExample` единицы; output означает одну прямую цепочку Tokenize→Execute, без заявления полного TranslationMachine Flow.

До запуска наряд обязан содержать FQN selector, discovery list, стоимость **каждого** выбранного training-bearing Fact и общий лимит. Native ledger привязывает calls к run/FQN/stage/recipient; control, setup, instrument, retries не исчезают из суммы при одинаковых state hashes. Контроль заявленный «без обучения» либо действительно не вызывает Train, либо получает отдельную явно названную учебную дозу. Проверка сборщика/ledger с fake child не запускает learner и не считается измерением обучения. Суммы 261/104 — ожидаемый контроль для именно этого среза и успешного полного класса, не постоянные лимиты будущего кода.

Проверка полноты: расширить selector с main до всего класса — предрасчёт и фактический ledger должны показать дополнительные 156 lessons/48 outputs; скрытый setup-call обязан изменить фактическую сумму и нарушить сверку бюджета. Если main откажет до конца, ledger сохраняет только достигнутые операции и внешний terminal, без заполнения плановыми 261. Прежде чем native запуск допустим, владелец attempt должен быть получен для всех выбранных training-bearing Facts.
<!-- CODEX-POST-ADMISSION-20261112-N60:END -->
