# P12 / Q013: нулевой первый LR, действительный update и граница переноса

Дата: **28 октября 2026**. Исследователь: `/root/p08_transfer_audit`, Astra ultra. Фаза: **READ_ONLY_RESEARCH + этот Markdown**. В текущей работе не запускались build, test, inference, обучение, experiment entrypoint или изменение product/runtime state. Числа ниже прочитаны из сохранённой попытки; дополнительные проценты и суммы вычислены из JSON без вызовов модели. Это один адресный аудит P12, не повторная проверка всего каталога и не новый эксперимент.

## 1. Результат и текущий статус переноса

Подтверждено: прямой `Trainer.TrainSupervisedNextTokenStep` при fresh scheduler step0, warmup4, maxLR=.001 выполняет Forward, Backward и clipping, затем отвергает LR=0 **до AdamW**. В сохранённой A-попытке веса, моменты, optimizer step400, SFT step0 и RNG не изменились. B заменяет только первое значение расписания на .00025, затем возвращает исходное расписание и делает четыре настоящих обновления. Это полезное подтверждение механизма обучения и исправимого противоречия между scheduler и SFT contract. Новое правильное правило знака не приобретено: ответы остаются2/4, T2 и E1 ошибочны.

**В основной код этот вариант B не перенесён.** Локально прочитан commit `f44195d55ea681eeaf3e215e4d13293f0207fe2a` — «Опыт тринадцатый: первый урок при нулевой скорости разогрева выполняется целиком и выбрасывается, а одной правки нулевого шага хватает, чтобы он дошёл до весов». Его25 файлов — P1201 и `required-changes.md`; product Trainer/scheduler/optimizer в delta отсутствуют. Наблюдённый HEAD очереди — `b47df140494fbcb0389c3aebb314c74bbeeef9d6`, runtime source HEAD в raw — `47c914705e16113f2752160fe0baba6ac085629d`. Remote publication этим аудитом не проверялась.

Важная область применимости: штатная incremental factory уже задаёт `WarmupSteps=0` перед созданием Trainer. Поэтому опыт не доказывает, что обычное incremental обучение сейчас теряет первый урок. Для pretraining нулевой LR разрешён и AdamW всё равно обновляет моменты/счётчик: утверждение «урок целиком выброшен» к этому пути неприменимо.

## 2. Первичные источники и идентичность

Все ссылки ниже ведут в canonical repository. H=`experiments-v2/RP01-MICRO/P1201`, R=`H/results/attempt-0001`.

| Что проверено | Точный источник |
|---|---|
| Паспорт PRIMARY и один фактор | [P12 card:4](../../experimets/cards/P12-optimizer-boundaries.md), строки8–18 |
| Сохранённый человеческий вывод | [report.md:31](../../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/report.md), 119–190,195–318 |
| Начальный state, scheduler, фактический reader | [before.json:9](../../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/before.json), schedule89, panel142 |
| A отказ и state parity; B четыре updates; oracle | [arms.json:7](../../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/arms.json), state50/57, B75, oracle171/187 |
| Actual after | [after.json:2](../../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/after.json), B строки156/194/232/270 |
| Budget, loss, полные panels, ресурсы | [metrics.json:2](../../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/metrics.json), panels88+, costs после708 |
| Итоговая квитанция | [status.json:8](../../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/status.json) |
| Runtime source manifest | [source-hashes.json:3](../../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/source-hashes.json) |
| Native путь и настоящий B-вызов | [Program.cs:617](../../../../../experiments-v2/RP01-MICRO/P1201/Program.cs),683–710; [NativeAccess.cs:42](../../../../../experiments-v2/RP01-MICRO/P1201/src/NativeAccess.cs) |

Текущие native bytes отдельно пересчитаны SHA256 и совпали с runtime manifest:

- `04.TrainingLoop/Trainer.cs`: `3d82a7edd6afa6303e09f8a4cdd7a20abc05d457ac2dc33ea41b27aba761fb60`.
- `04.TrainingLoop/Schedulers/CosineWithWarmup.cs`: `3a1b6d7e9d1dca2bccb1665980db24a1962066bc908a36598f610b31e06ecfab`.
- `04.TrainingLoop/Optimizers/AdamW.cs`: `4ff288aef879a110dbdbc0da356d5839f1087e219d128a06cb987bf251abe3ea`.

Текущий `P1201/Program.cs` не идентичен исполнявшимся bytes: raw SHA `22c15eeb9a2ae132d11bd30a41a9889f416d165b386749710365307bdd814ad0`, текущий `4eefc85e84a45e1a5670ec66a5fb87f8e7a07eee380c3480a8399080e107c8cf`. LF/CRLF это не объясняет. Working-tree delta этого файла отсутствует; он внесён тем же f441. **Причина post-run изменения задокументирована:** [run-log-encoding-note.md:65](../../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/run-log-encoding-note.md) сообщает новый UTF-8 file writer и отсутствие повторного build/run. Нельзя называть опубликованный host побайтово тем же исполнявшимся host или считать проверенным build его исправленного logging. Текущие line anchors Program — текущий source; исторические действия дополнительно подтверждаются raw. Полная реконструкция прежних112107 bytes здесь не выполнялась.

Отдельная уже закрытая документальная поправка не является новой находкой: [source-snapshot-compare-correction.md:18](../../../../../experiments-v2/RP01-MICRO/P1201/results/attempt-0001/source-snapshot-compare-correction.md) объясняет `differing_count=5` вместо1;17 файлов,16 совпали, один TensorOps отличается. Raw не переписан, новый запуск не нужен. Git-blob prefix из этого документа не путать с SHA256 файла.

## 3. Что обучалось и что показал consumer

S0 — `_shared-learned-starts/h64-rms-row/v1`, weights SHA `8721dd57b11fa61b11acf37c337611129891a1d3096ae0392a89455d6d58fdec`; импортированы AdamW moments и step400, RNG. A/B начинают отдельный **scheduler epoch0**. Это не заявление о восстановлении произвольного scheduler из донорского checkpoint. V256/C64/H64/L1/Q4/KV2/FF176, q/v LoRA rank4 alpha8 dropout0,896 trainable scalars. AdamW β1=.9, β2=.95, eps1e−8, decay0 — назначенная SFT-конфигурация, не ошибочный перенос старого DPO-default.

TRAIN/retention: T1 `C# result sign: 9-4 =`→`+`, T2 `C# result sign: 4-9 =`→`-`; EXAM: E1 `8-3`→`+`, E2 `3-8`→`-` с тем же префиксом. Каждая учебная строка22 tokens; native loss обучает все21 shifted targets. LF не добавлен. Native [Trainer.cs:102](../../../../../04.TrainingLoop/Trainer.cs) строит targets для всей последовательности,125 делит gradient на sequenceLength. Позиция ответа в raw — readout annotation, **не loss mask**. Поэтому сохраняющаяся в [P12 card:10](../../experimets/cards/P12-optimizer-boundaries.md) фраза `exact response masks` нуждается в том же уточнении, которое84–86 уже внесли для других H64-карт. Нельзя ради подписи менять objective оплаченного опыта. Фраза card12 «SFT LR=.001 constant» также противоречит локальному schedule8/14; локальный PRIMARY override следует назвать явным.

Настоящий consumer — [Program.cs:514](../../../../../experiments-v2/RP01-MICRO/P1201/Program.cs): `TextGenerator.GenerateDetailed`, maxNewTokens1, temperature0, topK0, topP1, verifiedEOS=null. Gold не передаётся генератору. LogP измеряется отдельным Forward на том же prompt. Все16 generations завершены LENGTH_LIMIT с1 token. Это исходный односимвольный reader, не post-hoc обрезка длинного ответа до gold.

| Строка | Верный знак / actual до→после B | P(gold) до→после B | Относительное изменение P(gold) | Margin gold−other до→после |
|---|---|---|---:|---|
| T1 | `+` / `+`→`+` | .2597945464→.2609583965 | +.447989% |1.861236334→1.863307476 |
| T2 | `-` / `+`→`+` | .0882108742→.0896576151 | +1.640094% |−.198426247→−.179077625 |
| E1 | `+` / `-`→`-` | .0473513544→.0472204946 | **−.276359%** |−.478856564→−.494457483 |
| E2 | `-` / `-`→`-` | .0782724805→.0794011292 | +1.441948% |.386182547→.401283979 |

Верных2/4→2/4; TRAIN1/2→1/2, EXAM1/2→1/2. T1 до и после действительно освоен в этом reader, T2 не освоен. E1 deterioration сохранён; unchanged aggregate его не отменяет. Проценты вероятности получены как `100*expm1(logP_after−logP_before)`, не как относительный процент отрицательного логарифма. У E1 margin уменьшился на.0156009197; масштаб ухудшения относительно |before|≈3.258%. Signed margin не превращать автоматически в успех/неуспех по проценту: на T2 он вырос≈9.751%, но знак всё ещё отрицателен.

Report:231–238 сам сохраняет отсутствие accuracy gain и E1. Его фразу «четыре урока — слишком маленькая доза» корректнее читать как гипотезу причины: альтернативная доза не проверялась. Направленное изменение трёх margins не доказывает освоение условного арифметического правила.

## 4. Причина и математика: что именно проверил B

[CosineWithWarmup.cs:28](../../../../../04.TrainingLoop/Schedulers/CosineWithWarmup.cs): при `0≤s<W`, `lr(s)=maxLR*s/W`. При W4 на шаге0 —0. [Trainer.cs:117](../../../../../04.TrainingLoop/Trainer.cs) делает Forward,125 Backward,126 clip,128 вычисляет LR и129–132 отклоняет `<=0`. AdamW вызывается135; `_supervisedStep++` лишь145. Поэтому A не дошёл ни до optimizer-step, ни до продвижения scheduler. Повтор того же вызова без изменения конфигурации/состояния по этому control flow опять встретит step0; повторный запуск здесь не выполнялся.

[FirstStepRateSchedules.cs:38](../../../../../experiments-v2/RP01-MICRO/P1201/src/FirstStepRateSchedules.cs) временно ставит native constant scheduler max=min=.00025; [Program.cs:683](../../../../../experiments-v2/RP01-MICRO/P1201/Program.cs) заменяет приватный scheduler только для B index0,689 вызывает настоящий Trainer,695–699 возвращает исходный объект. Проверенный ряд B: **[.00025,.00025,.0005,.00075]**. Это отличается от глобального `(s+1)/W` или SFT `GetLr(s+1)`: обе такие правки дают **[.00025,.0005,.00075,.001]**. Общий shift не является механическим переносом доказанного фактора; он меняет следующие updates и pretraining либо весь SFT warmup.

Существующая A-попытка:0/896 изменённых scalars, optimizer400→400, scheduler0→0, одинаковые weights/moments/RNG hashes без скрытого restore. B:4 updates, optimizer400→404, scheduler0→4,896/896 scalars изменены, deltaL2=.01747221544, maxAbs=.00190778077. A имеет1 Forward/Backward, B4; это не matched-work timing experiment.

Native [AdamW.cs:72](../../../../../04.TrainingLoop/Optimizers/AdamW.cs) сначала увеличивает t,96–97 обновляет m/v, затем103–106 меняет веса:

`m'=β1*m+(1−β1)g`, `v'=β2*v+(1−β2)g²`, `w'=w−η*(λ*w + mHat/(sqrt(vHat)+eps))`.

Oracle [AdamWStepOracle.cs:88](../../../../../experiments-v2/RP01-MICRO/P1201/src/AdamWStepOracle.cs) восстанавливает g из m/m', проверяет v', затем ожидаемый w'. В raw все четыре steps имеют0 weight/moment mismatches. На step0 candidate rates0/.00025/.0005/.001 дают704/0/704/832 weight mismatches. Это независимая арифметическая проверка **применённой дозы и AdamW-consistency**, а не независимый gradient/finite-difference oracle: g получен из optimizer moments. Заявлять по ней правильность полной производной нельзя.

**Почему pretraining другой:** [Trainer.cs:240](../../../../../04.TrainingLoop/Trainer.cs) разрешает0 (`<0`, а не `<=0`) и251 вызывает AdamW. При η=0 веса не движутся, однако m/v/t меняются. Они влияют на следующие updates. Убирать такой шаг как «пустой» без отдельного контракта нельзя.

## 5. Реальные пробелы и то, что дефектом пока не является

1. **Подтверждённое позднее обнаружение invalid LR.** Невалидность определима до model call, но проверяется после дорогой работы. Будущий минимальный target — `Trainer.TrainSupervisedNextTokenStep`, LR resolve/validate до выделения inputs и Forward. Это сохраняет отказ и экономит выброшенную работу; оно само по себе не заставляет warmup-SFT учиться. Не выдавать два изменения — timing отказа и положительный first LR — за один A/B factor.

2. **Подтверждённый transfer mismatch в предлагаемом общем решении.** Report303–315 обсуждает global shift/SFT shift; реально испытана только step0 replacement. Для точного native переноса разумный узкий кандидат: SFT-only first-step policy при W>0, s=0 и корректном положительном maxLR, η=maxLR/W; s≥1 использует прежнее расписание. Default W0 сохраняется; нулевой/NaN maxLR не «лечится» положительной константой. Обобщение с W4 на иной W — кандидат с boundary controls, не наблюдённый результат. Не выбирать max(minLR,η) молча: это ещё один фактор.

3. **Подтверждённая граница consumer.** [IncrementalTrainingSession.cs:403](../../../../../14.ContinuousLearning/Training/IncrementalTrainingSession.cs) содержит комментарий о non-zero first step и `trainingConfig with { WarmupSteps=0 }`, затем Trainer/AdamW. [ProductionSupervisedTrainingStepExecutor:19](../../../../../14.ContinuousLearning/Training/SupervisedTrainingExecution.cs) действительно зовёт Trainer. Этот действующий обход следует сохранить и покрыть тестом конфигурации. Удалять его на основании P12 нельзя. P12 не повод менять FIX-MC schedulerPolicy=NewSession или DPO.

4. **Подтверждённый ограниченный state claim.** A полностью сохраняет перечисленные численные состояния при dropout0; gradient buffers после refusal уже выделены и очищены. Это не byte-identical process state, не отсутствие allocations и не доказанная rollback-гарантия для dropout>0. Forward с random layers потенциально меняет RNG до позднего отказа; такой режим в P12 не измерен. Ранний отказ устраняет именно этот путь, но rollback для любых поздних ошибок — отдельная задача.

5. **Адресный пробел writer, не опровержение текущих raw.** [Program.cs:959](../../../../../experiments-v2/RP01-MICRO/P1201/Program.cs) выводит learningValidity из weight movement, внешнего bundleQualified и reference hash. Фактический qualified BEFORE и полнота after-panel не являются аргументами этого условия. В raw здесь T1 before/after правильный, поэтому результат не равен P05-ошибке0→0. Следующий writer должен отдельно принимать observed qualified BEFORE, actual updates и полный AFTER, сохранять learned/retained/lost IDs. **1→0 остаётся валидно измеренной регрессией**, а не автоматически invalid experiment; решение transfer/goal отделяется от validity. Нового общего FIX дублирующего80 не нужно: это конкретный адрес того же writer-контракта.

6. **Часть объявленных guards не участвует в terminal closure.** `guardArmABackwardRan`/`guardArmAPanelUnchanged` пишутся в guards793/796, но не входят в hypothesisHolds956–957; per-step oracle results после первого тоже не входят в guardDoseVerified783. Для узкой гипотезы первого update all-step oracle не обязательный критерий; для заявления «все дозы проверены» и «Backward состоялся» эти признаки надо явно включить в соответствующий claim receipt. В текущем raw они true/0 mismatches; ложный actual PASS не обнаружен. Проверка должна мутировать копию saved receipt и вызвать настоящий builder, без нового Forward.

7. **Ресурсы A/B не измерены, универсальный допуск не установлен.** Whole-program wall1823.6761ms, CPU1187.5ms, peak59990016B, allocated87557024B. Они не дают arm-specific delta. Ни полезный update, ни CPU exception не разрешают считать RAM/load-save/disk регрессии отсутствующими. A1/B4 — заранее заданные разные work counts.

8. **Нет сохранённого P12 S1 для повторного consumer.** В after только panel/final hashes/change counts; snapshot arrays/moments существовали в памяти, наружу записаны summaries. В проверенном attempt directory нет checkpoint/S1 files; Program writer не сохраняет конечные weights/optimizer/RNG. Нельзя восстановить модель из SHA или oracle summaries. Повторный readout с «P12 S1» требует реально найденного и проверенного артефакта; его наличие не подтверждено. Исторический run не повторять для отчётной арифметики. При отдельно разрешённом native parity validation сохранить новый конечный state один раз и переиспользовать далее.

## 6. Полный расход и критерий69

Raw честно разделяет5 training Forward,5 Backward,0A+4B updates,16 generations,16 scoring Forward. Native [TextGenerator.cs:203](../../../../../09.Inference/TextGenerator.cs) имеет один prefill ForwardWithCache;234–235 при maxNewTokens1 выходит до decode238. Поэтому производный полный счёт: **37 model Forward =5training+16prefill+16scoring**,5 Backward,16 greedy Sample,105 учебных token presentations. Guard queries23/state captures12 дополнительных model calls не делают. Это статически сверенный полный учёт, не новый instrumented runtime counter.

По [69:28](../69-transfer-admission-and-priority-2026-10-27.md): полезные вероятностные улучшения здесь <5%, accuracy не изменился; E1 regression видна и <5% по P(gold).0→4 updates нельзя превращать в бесконечный процент. Для явной логической несовместимости scheduler/SFT действует отдельное основание ремонта. General-default admission по ресурсам пока **NOT_MEASURED**, а не PASS. CPU-time exemption касается только CPU; RAM, allocations, disk/checkpoint bytes, load/save, accuracy и остальные защищаемые показатели остаются на контроле. Wall time не переименовывается в CPU. Маленькие положительные изменения сохраняются; отсутствие большого accuracy gain не отменяет механизм и результат попытки.

## 7. Существующее покрытие, точные будущие проверки и бюджет

Проверено чтением, **не запуском**:

| Existing test | Что уже проверяет / недостающая граница |
|---|---|
| [SchedulerTests.cs:13](../../../../../Tests/SchedulerTests.cs) `Warmup_StartsNearZeroAndRisesToMaxLr` | Только `step0<step50<step100=max`; exact first-step policy не закреплена |
| [SchedulerTests.cs:27](../../../../../Tests/SchedulerTests.cs) `Decay_DropsBelowMaxLrAfterWarmup` | Decay/end bound, не SFT integration |
| [ContinuousLearningTests.cs:2071](../../../../../Tests/ContinuousLearningTests.cs) `Trainer_SupervisedNextTokenStep_InvalidLearningRate_DoesNotUpdateParameters` | W0, max=min0/NaN, no-dropout, weights unchanged; не workload/moments/counters/RNG |
| [TrainerTests.cs:272](../../../../../Tests/TrainerTests.cs) `SupervisedSteps_DoNotAdvancePretrainingCounterOrScheduler` | Раздельные counters/trajectory; fixture W0. Сохранить, не переписать expected под global shift |
| [ContinuousLearningTests.cs:2035](../../../../../Tests/ContinuousLearningTests.cs) token/weight boundaries | Входные границы уже существуют; расширять адресно |

Будущий work order после прямого разрешения реализации, сейчас **не выполнялся**:

- Сначала отдельная source-only/writer стадия: сделать correction receipt по существующим before/arms/after с0model calls/updates; это обработка оплаченной попытки, не новый эксперимент без обучения. Вынести настоящий status builder под unit tests и проверить observed BEFORE mismatch, отсутствующий/дублированный AFTER, regression1→0 с valid comparison и blocked transfer, отдельный false backward guard, late-step oracle mismatch. Сохранять raw attempt, его ID и parent reference. Logging/source mismatch пометить, не переписать recorded runtime SHA текущим.
- Изменение раннего отказа проверять в существующем invalid-LR test:0/NaN/±Infinity и отрицательный LR; входы валидны; snapshots weights/moments/optimizer/scheduler/RNG до/после, отсутствие вновь созданных Grad при initially-null и сохранность заранее заданных gradients по явно выбранному contract. Для dropout>0 RNG before/after — отдельный контроль, не утверждение об уже обнаруженной порче. Guard tests не являются новым самостоятельным учебным PRIMARY.
- Native first-step policy — собственный узкий метод/ветка внутри Trainer; public API и product reflection seam не требуются. Экспериментальный reflection helper исторического P1201 не переносить в продукт. В новых `Tests/ExperimentFixes/P12WarmupFirstUpdateTests.cs` закрепить W=0/1/4, s=0/1/W−1/W/total, invalid maxLR, min=max constant, раздельные SFT/pretraining epochs. Для W4 ожидается именно [.00025,.00025,.0005,.00075], не скрытый глобальный shift. Existing pretraining behavior при lr0 отдельно сохраняет moments/counter progression; AdamW.Step не переписывать.
- **Один предложенный конечный native transfer-validation**, если отдельно разрешён: общий сохранённый S0, два независимых restore, native candidate и test-only reference композиция исходного step0 override, по4 настоящих full-sequence SFT T1,T2,T1,T2. Оба имеют одинаковые импортированные moments400/RNG и schedule epoch0; W4/total8/max.001/min.0001, decay0/clip из точного donor config. Это проверка нового native переноса, не повтор original A failure ради красивой метрики. Actual BEFORE обязателен; regression AFTER сохраняется и блокирует перенос при соответствующем пороге.
- Бюджет этой **предложенной новой**, ещё не назначенной на запуск программы:8 updates/8training Forward/8Backward, общий before-panel4generation+4scoring и два after-panel по4generation+4scoring. Итого **32model Forward=8training+12prefill+12scoring**,12greedy Sample,168training token presentations. Никаких дополнительных qualification sweeps/seed retries. Build≤600s, runtime≤1200s,total≤1800s; append-only receipt с source/config/data/tokenizer/state hashes. Guard-model calls, если добавятся, заранее назвать и включить в новый бюджет, не прятать в «controls». Сохранить S1 обеих рук, typed optimizer/RNG и квалификацию; последующие readout/resource проверки — явно учтённое продолжение этой обученной попытки. Original P1201 raw не дописывать заново.
- Совпадение candidate/reference weights/moments и readout с объявленным tolerance проверяет перенос. Ресурсы измерять вокруг одинаковых stage boundaries: CPU/wall/allocations отдельно, process peak не выдавать за изолированный per-arm peak. Если без нового отдельного процесса peak attribution невозможна — оставить unknown. Не назначать extra repetitions ради снижения шума без отдельного бюджета.

## 8. Применимость к текущим планам

- [21 / FIX-MC](../21-sft-optimizer-continuity.md),113–114 уже различает optimizer continuation и session-local scheduler, `schedulerPolicy=NewSession`; P12 не заменяет этот договор и не является новым доказательством scheduler resume.
- [23 / FIX-MS](../23-sft-response-mask.md) и [62 / framing](../62-supervised-framing-and-learning-readout.md): добавить P12 к адресам donor objective/readout-mask drift; не менять full-sequence учебный цель оплаченного run.
- [80 / P05 writer repair](../80-followup-repair-plan-2026-10-27.md): переиспользовать разделение real training, qualification, outcomes, transfer admission; не возвращать правило «weight delta значит освоенный навык». Одновременно не объявлять валидную измеренную регрессию invalid лишь из-за ошибки после обучения.
- [84](../84-reporting-sampling-cache-transfer-audit-2026-10-27.md), [85](../85-reporting-sampling-cache-repair-plan-2026-10-27.md), [86](../86-reporting-sampling-cache-card-coverage-2026-10-27.md): reuse только реально сохранённых S1, все implicit model calls включены в расход, constructor/adapter не доказательство consumer. P12 отличается отсутствием saved S1; P08 continuation нельзя скопировать буквально.
- Существующие планы с Warmup0/constant LR (например30 и17) не требуют global schedule migration; guard явного effective config до training полезен им всем. В проверенных01/21/85 отдельный владелец first-warmup-step fix не найден; прежде нового FIX-ID родитель связывает эту находку со своим новым планом, не прячет изменение scheduler в optimizer-continuity.

Граница чтения: primary report прочитан полностью, raw JSON разобраны по перечисленным sections, native scheduler/AdamW/SFT/pretraining и существующие tests прочитаны в адресных диапазонах; governance/skills — целевые выдержки, не полный reload тысяч строк. Непрочитанные соседние варианты P12, все остальные catalog raw и полный behavioral diff исторических host bytes не заявлены проверенными. Новых runtime результатов нет.
