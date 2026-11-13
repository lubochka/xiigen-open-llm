# N27-A — Ранее обученная TranslationMachine теряет статус пригодности после пакета без новых уроков

**Дата:** 2026-10-16. **Статус:** `CODE_SEAM_CONFIRMED / CAUSAL_QUALITY_NOT_RUN / RP01_STUDENT_CAUSE_NOT_PROVED / DRAFT_FOR_INDEPENDENT_REVIEW`. **Source snapshot:** `lubochka/llm_mvp_core@8c7d41611aecba98d3d1bbdbf30d69bd784949ae`; исследуемый файл `27.TranslationDomain/TranslationMachine.cs`. Публикуется **только** в `xiigen-planning@llm_plan3/.../possibleDifs`. Никакой core-код, веса, данные, checkpoints и закрытый каталог core docs не изменялись.

## Коротко: новый эффект, не повторяющий счётчик `RowsTrained`

Если структурная машина уже выучила полезный перевод, последующий **пакет, содержащий только held-out examples** (или не имеющий ни одного распознанного обучаемого Unit), сбрасывает `_hasStructuredTraining` в `false`. Память KNN и карты переводов при этом не очищаются. Однако публичный `EvaluateAsync` затем **не выполняет проверку перевода вообще**, возвращает accuracy=0 и отрицательный gate. В результате успешный learner может ошибочно выглядеть утратившим качество без ухудшения его прогнозов.

Это отличается от уже опубликованного [N17 silent Unit dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md), который описывает `RowsTrained` при неизвестном получателе. **Новая гипотеза — non-monotonic readiness gate после корректного предшествующего обучения, в том числе при entirely-held-out batch, и его сохранение через checkpoint.**

## Первичная кодовая цепочка (строки с привязкой к pinned commit)

1. [`TrainStructuredBatchAsync` L89–103](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L89-L103) проверяет лишь `batch.Examples.Count > 0`; не требует хотя бы одного `!example.HeldOut`.
2. [L105–123](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L105-L123): `trainedUnits` создаётся заново **для каждого вызова**, цикл ограничен `Where(example => !example.HeldOut)`, неизвестные `UnitId` молча пропускаются. При only-held-out циклов `TrainOnExample` ровно 0.
3. [L124–145](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L124-L145): при наличии held-out `AccuracyAfter = MeasureHeldOutAccuracy(heldOut)`, независимо от готовности; затем **`_hasStructuredTraining = trainedUnits.Count > 0`**, version инкрементируется и TrainingHistory получает запись с `RowsTrained=0`, даже когда ранее обученная память не очищена.
4. [`EvaluateAsync` L148–200](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L148-L200): `heldOutAccuracyMeasured = _hasStructuredTraining`; при `false` блок, вызывающий `TranslateForEvaluationAsync`, не исполняется; `accuracy` остаётся 0; `PassesGate=false`, `GateNote` помечает `legacy scaffold held_out_accuracy_measured=false`. Это **не результат проверки переводов**, а результат флага.
5. [`ExportCheckpoint` L282–302](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L282-L302) сохраняет `HasStructuredTraining`; [`ImportCheckpoint` L339–359](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L339-L359) восстанавливает его вместе с реальной KNN/Unit памятью. Следовательно, ошибочно сброшенный статус переживает штатное full-checkpoint восстановление.
6. [`TranslateForEvaluationAsync` L686–704](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L686-L704) сейчас hardcodes RU→EN. Поэтому первую микрооценку нужно ограничить направлением RU_EN либо использовать отдельный ordinary consumer EN_RU: не перепутать этот **уже известный** direction gap с новым readiness дефектом.

**Непосредственно доказано синтаксисом и control flow:** B (held-out-only) может сбросить историческую readiness без обучения и очистки памяти. **Не доказано:** какой реальный orchestration caller подаёт B, вызвал ли он такое состояние в RP01, насколько это влияет на пользовательскую правильность. RP01 C#500/ENRU500 вызывает другой путь `CodeDocumentationSimulationDpoExperiment → DpoTrainer/LoRA → TextGenerator`; новая проблема не объясняет автоматически 0/3 и 0/4.

## Различающий микроопыт N27A: train A → no-train B → тот же consumer

**Tier 1, без новых DPO и без внешних вызовов.** На свежей `new TranslationMachine()` использовать два исходно валидных **структурных** учебных урока из разрешённого existing seed для RU_EN; не генерировать новые chosen/rejected. Test set заранее отделить по source family от train. Повторяем один и тот же `LearningEvaluationSet<TranslationDomain>` после каждого вмешательства.

| Плечо | Действие | Что измерять |
|---|---|---|
| C0 | fresh machine, ещё не обучена | ordinary inference и честное NOT_MEASURED; baseline |
| A | один `TrainStructuredBatchAsync` c recognized, `HeldOut=false` rows | `PromotedUnitIds`, `RowsTrained`, `HasStructuredTraining`, KNN/map counts, `EvaluateAsync` + ordinary answer |
| B | **та же обученная machine** + `TrainStructuredBatchAsync` только с `HeldOut=true` из неизменного test set | те же 6 полей, `EvaluateAsync` и ordinary answer на **точно тех же input** |
| C1 | `LoadCheckpointAsync` из сохранённого **после B** полного checkpoint в новую машину | `HasStructuredTraining`, KNN/map counts, `EvaluateAsync`, ordinary answer |
| C2 | fresh A копия, **без** B | повтор того же `EvaluateAsync` для контрольной стабильности |

**Первичный фальсификатор:** если B отвергается admission на entire-held-out либо `_hasStructuredTraining` не сбрасывается, гипотеза в данной pinned версии опровергнута. **Source-derived expected:** после A `HasStructuredTraining=true`; после B `false` и `EvaluateAsync` возвращает 0/negative gate, при сохранении KNN/Unit state. Если проверочный end-user output стабилен, заключение — **ложный отрицательный сигнал контроля качества**, а не ухудшение генерации. Если независимый реальный consumer меняет корректный ответ — зарегистрировать отдельную причинную цепочку (не предполагать её заранее).

**Метрика и порог:** главное `false_negative_quality_gate = 1`, если A проходит свой обоснованный eval, B без изменения входа/весов памяти оценивается как 0 исключительно из-за флага; `same_input_correctness_delta` и abstention — вторичные. `No gain` по ответам не уменьшает важность ошибки quality gate, но запрещает говорить «DPO/LoRA сломались».

## Ремонт только после native-фальсификатора

Разнести 3 понятия: (1) когда-либо обучено распознаваемое состояние Unit; (2) сколько Unit реально обновлено **этим** batch; (3) проведена ли оценка на настоящем heldout. Для empty/no-op batch не стирать persisted readiness; `RowsTrained` и продвижение считать по реальным mutation receipts; ошибка invalid/empty training semantics должна быть явной. Не заменять реальное качество ещё одним булевым флагом. В конфигурациях RP01 Student новый патч Unit не заменяет проверку B0/SFT→DPO.

**Review:** архитектурное самообследование и source-review выполнены автором текста; **независимое CODEX/astra 6 ultracode review НЕ вызывалось**. Эксперимент НЕ запускался. Ответственность за CODEX lesson selection/quality и внешнюю китайскую модель для BOTH DPO sides сохраняется, но этот опыт не создаёт новых DPO.
