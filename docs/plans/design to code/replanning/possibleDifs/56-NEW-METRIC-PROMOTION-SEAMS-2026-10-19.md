# N56 — новые, узкие source-seams: неверные cost meters и «обучен» ≠ «опубликован»

**Дата** 19.10.2026 · `SOURCE_CONFIRMED` для конкретных выражений C#; `PRODUCT_IMPACT_UNPROVEN`, `NATIVE_AB_NOT_RUN`. **Новых измерений качества здесь нет.** Основа: текущая ветка `llm_mvp_core@codex/dpo-implementation`, blob `TranslationMachine.cs=fd0e736de3739236b4dde2d6b9d000f5ebae8416`, `LearningMachineContracts.cs=7f605d49762e3fc0c1b4d4ca490596bc9668cdd5` при чтении. Перепроверить blob/HEAD перед native запуском. Запись **только** `xiigen-planning/.../possibleDifs`.

## N56-M1 · Метрика CPU объявлена, но измерено настенное время

**Что код реально делает:** [`TranslationMachine.TrainStructuredBatchAsync` L100–144](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L100-L145) берёт `startMs = Environment.TickCount64`, а затем:
```csharp
CpuMs = Environment.TickCount64 - startMs,
ElapsedMs = Environment.TickCount64 - startMs,
```
**SOURCE_CONFIRMED:** оба поля получают разность wall-clock tick counter, **`CpuMs` не измеряет CPU time**. Из двух значений нельзя построить загрузку CPU, efficiency CPU, сравнить потребление циклов двух алгоритмов или корректную конкуренцию с сетью/await. При подсчёте времени короткого batch TickCount64 имеет ограниченное разрешение; отдельного `Stopwatch` здесь нет.

**Риск:** планировщик может ошибочно приписать снижение `CpuMs` сокращению вычислительной работы при том же wall latency и обучаемых update, хотя это **ошибка интерпретации**, не автоматическая потеря веса/качества. Недоказано, что RP01 neural-run вызывает этот `TranslationMachine` метод или что policy выбирала варианты именно по `CpuMs`.

**Prior art boundary:** protected [03-metrics-and-reporting](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/03-metrics-and-reporting.md) требует отдельно учесть elapsed и стоимость реальной работы, но просмотренный [полный source claim-ledger](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/04-source-claim-ledger.md) и master index **не содержали** точного `Environment.TickCount64 -> CpuMs` seam. «Новое» ограничено этими конкретными source expressions, не метриками вообще.

**Мини-контроль:** 1 реальный неизменённый EN→RU `TranslationLearningBatch`, два **последовательных измерения** одного исполнения через тот же профиль (без A/B качества). A = returned `CpuMs/ElapsedMs`; B = внешний `Stopwatch.Elapsed` и `Process.TotalProcessorTime` deltas вокруг того же вызова, вызванного один раз. Для холостого warmed call и batch без updates также логировать actual `TrainOnExample` count. Ожидать `CpuMs≈wall`, не считать равенство `CpuMs=CPU`. Это метрологическая C-проверка, **0 новых сравнений качества**. Фальсификация: вызывающий wrapper перезаписывает `CpuMs` перед использованием, или данный путь не читается человеком/ранжировщиком. Для причинного выигрыша надо отдельно повторить A/B алгоритмов на одинаковых held-out заданиях.

## N56-M2 · Показатель «PeakRamMb» — один снимок managed heap, не пик процесса

**Source:** те же [L134–144](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L134-L145):
```csharp
PeakRamMb = GC.GetTotalMemory(forceFullCollection: false) / (1024 * 1024)
```
`GC.GetTotalMemory(false)` возвращает **оценку управляемой памяти в момент вызова**, не сохраняет пиковое значение за batch, не включает полноту RSS/native allocations/VRAM. Название `PeakRamMb` в `LearningBatchResult` может вводить в заблуждение при сравнении kNN индекса, нейросетевой LoRA и буферов decoder. Это источник **недостоверного resource trade-off**, а не свидетельство утечки/деградации. Наличие GPU/VRAM для данного конкретного TranslationMachine не утверждается.

**Малый probe:** один native batch, один Process sampler до/во время/после с максимальной фиксируемой частотой, `Process.PeakWorkingSet64` либо sampled `WorkingSet64`, `GC.GetTotalMemory` до/после, `GC.GetTotalAllocatedBytes` при применимости. A = публичный field, B = независимый процессный пик; не складывать heap и RSS как две независимые части. Два взаимно отличных профиля: 2 малых lessons vs 2 длинных lessons, **одинаковое число updates**; это проверка измерителя, не обучения. Если отдельный counter уже измеряет peak и используется downstream, назвать его явно и снять конфликт отчёта.

**Prior art:** protected [методика четырёх состояний](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/01-method-and-report.md) уже правильно требует RAM peak в экспериментальном отчёте; новый вопрос — **доверяет ли runner этому конкретному полю** и как его проверить. Не дублировать P119 (LR/beta/batch).

## N56-M3 · `PromotedUnitIds` является локальным набором touched Units, без доказательства публикации в 25.ModelServer

**Source:** [L103–145](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L103-L145) накапливает `trainedUnits` после `trainer.TrainOnExample`; в конце отдаёт
```csharp
PromotedUnitIds: trainedUnits.OrderBy(StringComparer.Ordinal).ToArray()
```
Этот метод **не вызывает publish / server registry / activate** и не получает acknowledgement `25.ModelServer`. А [контракт `LearningBatchResult`](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/00.Core/LearningMachineContracts.cs#L45-L85) описывает `PromotedUnitIds` как IDs checkpoints, promoted to `25.ModelServer`. Таким образом **прямой метод гарантирует «trainer был вызван», но не «сервер принял checkpoint»**.

**Риск для качества:** модуль, который считает `PromotedUnitIds.Length>0` фактом принятия learned state, может показать успешное обучение там, где после cold start ordinary consumer загружает прежнюю модель. Для доказательства риска **нужен настоящий downstream caller, читающий поле без receipt публикации**. Не утверждается, что его нашли. Этот seam отдельный от уже описанного [N17 silent dispatch](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md): N17 показывает false `RowsTrained` при незнакомом UnitId; здесь выбран **распознанный, действительно обученный** Unit, и сравнение касается последующего принятия сервером.

**Малый causal probe (перевод EN→RU, другой профиль отдельно):** обучить ровно два валидных structured lessons в процессе P125; залогировать `PromotedUnitIds`, actual Unit mutation, server registry/checkpoint manifest SHA, active serving pointer, ordinary response in-process и **new process after cold restore**. A = native `TrainStructuredBatchAsync` без явно показанной публикации; B = тот же trained source state, но исполняется *существующий законный* export/publish + validated activation, если реальный caller предусмотрен; freeze corpus+gold+semantic tokenizer. Основная метрика: `correct/4` для **одинаковых независимых четырёх tasks** in-process vs cold process; secondary `publisher receipt exists`, active SHA, time and memory. 4×2×2 consumer answers максимум16, ровно 2 урока; не считать оба arms равнозначными, если B требует отдельного API. **Фальсификация:** настоящий serving caller не использует `PromotedUnitIds` либо автоматически публикует state за пределами данного метода с проверяемым acknowledgement.

## N56-M4 · CPU/RAM/promoted + accuracy: риск ложной «скорости обучения» от метрик и принятия

Три независимых seam не нужно исправлять одним патчем. Сначала **инструментация без изменения обучения** (M1), затем **измерение памяти без изменения обучения** (M2), затем **доказательство публикации и совпадения ordinary consumer после restore** (M3); по одному фактору. Только если downstream consumer использует эти поля для выбора модели, gate или policy, перейти к отдельной настоящей причинной оценке предметного качества.

**Критические контрдоказательства/дедупликация:** `_accuracyAfterLastBatch` contamination уже подробно доказана в [N55](55-NOVEL-DIRECTION-EVAL-AND-METRIC-STATE-2026-10-19.md), `_hasStructuredTraining` reset — в [N27](27-NEW-STRUCTURED-READINESS-RESET-AFTER-NOOP-BATCH-2026-10-16.md), ложные `RowsTrained` — в [N17](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md). Ни одна из них не названа N56-new.

**Статусы сегодня:** `M1/M2 SOURCE_CONFIRMED, TRUE_MEASUREMENT_NOT_RUN`; `M3 SOURCE_CONFIRMED_LOCAL_NON-PUBLISH, REAL_SERVER_CONSUMER_NOT_TRACED`; `RP01_NEURAL_ROOT_CAUSE=OPEN`. Нельзя объявлять эти три причины установленной regression между ветками: требуется gold/current вызов одного потребителя и одинаковый экзамен.

[Эксперименты, оформленные по доменам](56-CAUSAL-MICROEXPERIMENTS-BY-DOMAIN-2026-10-19.md) · [проверка read-only покрытия](56-PROTECTED-EXPERIMENT-VALIDITY-2026-10-19.md).
