# Проверка ремонта R2–R4: численные пути и полнота доказательств

27 октября 2026. Исполнитель: исследователь `/root/repair_numerical_audit`. Фаза — **READ_ONLY_RESEARCH плюс этот Markdown**. Product code, тесты, веса и runtime state этим исследователем не изменялись; команды изменения Git index/веток, builds/tests/experiments/inference/training не выполнялись. Ниже описаны логические контрпримеры, а не выполненные mutation runs. После записи файла Git показал для него `AM`; происхождение index-изменения не проверялось и не приписывается исследователю.

**Результат:** прежние ошибки области ATS и отдельного training core SVD исправлены в прочитанном исходнике. Найдены два конкретных дефекта доказательства R4: сравнение RNG через `ToString()` не читает содержимое потоков; общий тест CS/RRD/MB не выполняет назначенную восстановленную учебную программу и не сравнивает конечное состояние с независимым эталоном. Это не доказательство новой ошибки самих численных ядер.

## 1. Идентичность и ширина проверки

Начальный и повторно прочитанный HEAD: `455398b082ddd1a88e6eb6ffe1daba56ea289611`. Прочитан фактический C# diff `4a7839d4c0b` (ATS), `f9694321424` (SVD), `dc8eb29bd3a` (LC), `76c3b027280` и `d847a109cc5` (общая qualification). Для `f04ba4dfe89`, `850b767904b`, `9f4e1bd2df2` проверены identities, список изменённых файлов и актуальное содержимое соответствующих разделов [66](../66-r4-row-evidence-2026-10-27.md). Последние три изменения — документация, не новые численные реализации.

В конце чтения `git diff --name-only HEAD --` для разобранных product/test файлов ATS, SVD, LC, общей qualification, RNG, Trainer и двух дополнительных parent-findings вернул пустой список. Это связывает приведённые строки с указанным HEAD; не является утверждением о чистоте всего checkout. Чужие текущие изменения не присваиваются этому исследователю.

Прочитаны актуальные `AGENTS.md`/`CLAUDE.md` адресными фрагментами, включая first-screen правила, и адресные части шести назначенных навыков: architect-behavior-classifier, planning-session-startup, plan-review, generated-code-review, output-readability-gate, human-readable-architecture-audit. `human-communication-GLOSSARY.md` прочитан целиком. **Полная загрузка больших governance/skill файлов не заявляется.** Применён Context Mode; его файл прочитан целиком. Новых субагентов исследователь не создавал.

Исходные finding/acceptance прочитаны в [64](../64-transfer-execution-audit-2026-10-27.md), [65](../65-transfer-repair-plan-2026-10-27.md), [57](../57-native-transfer-contract-gaps.md), владельцах [LC/13](../13-dpo-reference-cache.md), [RRD/44](../44-rms-row-reduce.md), [ATS/45](../45-attention-transpose-strides.md) и [прежнем41](../../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md). Не все исторические строки этих больших документов перечитывались полностью.

## 2. N1 — RNG LC сравнивается без содержимого потоков

**Приоритет P1. Тип: доказанный дефект тестового oracle, не обнаруженная порча генератора в продукте.**

Точный target: [Tests/DpoReferenceScoreCacheTests.cs:209](C:/Projects/llm_mvp_core/Tests/DpoReferenceScoreCacheTests.cs:209), строки209–211:

```csharp
Assert.Equal(
    plain.Fixture.Policy.CaptureRandomState(plain.Topology, plain.Identity, 2).ToString(),
    cached.Fixture.Policy.CaptureRandomState(cached.Topology, cached.Identity, 2).ToString());
```

[LlmModelRandomStateSnapshot](C:/Projects/llm_mvp_core/03.TransformerModel/Randomness/LlmModelRandomState.cs:29), строки29–39, — record без собственного `ToString`. Его `Entries` имеет тип `IReadOnlyList<LlmModelRandomStateEntry>`. [LlmModel.CaptureRandomState](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:297), строки297–317, передаёт туда обычный `List<LlmModelRandomStateEntry>`. Стандартное строковое представление record выводит строковое представление этой коллекции, а не каждую entry. Поля `AdapterKey`, `StreamDomain`, `InitializationSeed`, `DropoutSeed`, `ConsumedCursor` из строк18–23 носителя этим assertion не сравниваются.

**Логический контрпример, не запускался.** В двух snapshots сохранить одинаковые format/algorithm/topology/configuration/step и обычный `List` того же типа; во втором заменить лишь `Entries[0].ConsumedCursor` с0 на1. Строковое сравнение внешнего snapshot не отличает их. Можно так же изменить seed или stream entry. Это показывает дефект сравнения независимо от того, случается ли такая порча в текущем продукте.

Дополнительная граница: fixture устанавливает `Dropout=0` ([тест:241](C:/Projects/llm_mvp_core/Tests/DpoReferenceScoreCacheTests.cs:241)); [Dropout.Forward:56](C:/Projects/llm_mvp_core/00.TensorCore/Layers/Dropout.cs:56) тогда возвращает input, не выбирая маску. Следовательно, это не runtime-доказательство продолжения ненулевого потока dropout. **Dropout0 — допустимый назначенный режим LC**, менять его ради основного сравнения не требуется: owner13:85 прямо требует сверять RNG/state identity и при dropout0.

**Будущая точная правка.** В этом Fact заменить два `ToString()` на сравнение format, algorithm, topology/configuration identity, randomStateStep, количества и каждой упорядоченной entry. Уже существующий правильный образец находится в [DpoTrainerOptimizerStateHandoffTests.cs:418](C:/Projects/llm_mvp_core/Tests/DpoTrainerOptimizerStateHandoffTests.cs:418), строки418–427. Для чувствительности добавить чистую проверку сравнивателя: одинаковый snapshot принимается, изменение только одного cursor/seed/stream отклоняется. Она не требует нового fit/Forward. Если нужен отдельный stochastic continuation, использовать уже назначенный LC continuation contract с собственной учтённой ценой, не молча расширять main fixture.

**Приёмка.** Изменение одной entry при прежних остальных полях обязательно обнаруживается; независимые снимки с одинаковым содержимым проходят. В отчёте отдельно названы проверка identity при dropout0 и наличие/отсутствие опыта с ненулевыми draws. До правки строка66:21 «состояние генератора случайных чисел» подтверждена лишь внешними scalar-полями, а не полным состоянием потоков.

**Что LC действительно усилил.** Новый Fact:164–211 сравнивает все перечисленные значения policy между capacity0 и2, frozen reference с началом, `Step==2`, байты exported AdamW, и требует движения policy. Это содержательное усиление прежних четырёх метрик. Пропуск последнего optimizer call с уменьшением Step обнаруживается прямо assertion. Оно не отменяется N1. Основной H512 restored-start/readout contract владельца13 и отрицательные lifecycle/authority/rollback оси остаются отдельной незакрытой работой; tiny fixture из этого Fact не является их заменой.

## 3. N2 — общий CS/RRD/MB тест подменяет восстановленную учебную квалификацию и не охраняет полный terminal state

**Приоритет P1. Тип: доказанный дефект qualification/oracle и завышенное закрытие оси в66. Численная ошибка CS/RRD/MB этим не установлена.**

Targets: [RestoredStateLearningQualificationTests.cs:32](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/RestoredStateLearningQualificationTests.cs:32), строки32–75,112–133,149–159,171–190,201–214; [66:591](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/66-r4-row-evidence-2026-10-27.md:591), строки591–605,635–648,653–655.

| Назначено владельцами41/44 и подтверждено historical raw | Сделано новым тестом |
|---|---|
| V256/C64/H64/L1/Q4/KV2/FF176; q/v LoRA rank4/alpha8 | V64/C16/H16/L2/Q2/KV1/FF32; plain model без LoRA |
| Готовый `h64-rms-row/v1` для RRD/MB; отдельный `h64-causal-scores/v1` для CS | `SavedState()` каждый раз создаёт свежую модель и получает массив из `Random(4242)`; `Restored()` копирует float[] вручную |
| Native checkpoint load, keyed AdamW step400 и family RNG/identity | Ни checkpoint load, ни optimizer import, ни RNG restore; свежий AdamW step0 |
| Приобретённый TRAIN T1 уже верен; T1,T2,T1,T2; LR0.001; step400→404 | Четыре новых массива ID; LR0.05; step0→4; все параметры trainable |
| A/B для одного фактора; полный keyed terminal state, frozen state и readout | Одна текущая реализация выполняется повторно; Final и Optimizer сравниваются лишь между этими повторами |

Дословное назначение [прежнего41:72](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md:72): «Восстановить keyed AdamW moments/step400 и RNG этой семьи; не искать другого донора, не обнулять optimizer и не повторять400updates». Строки74–76 фиксируют порядок, LR и бюджет. Владелец44:3 отдельно говорит, что scalar oracle не заменяет эти восстановленные четыре обновления. Это **не только отсутствующая historical панель**, поскольку отсутствует сам назначенный trained continuation input.

### 3.1. Нет acquired-before, который можно сохранить

Gold последнего токена Lessons:34,35,36,37. Закреплённые before:42,37,43,46; after:22,22,22,22. Для собственного readout теста это **0/4→0/4**. Комментарий теста:208–211 честно отрицает приобретение и называет постоянный ответ; документ66:615–618 также признаёт отсутствие усвоения. Однако этим невозможно обосновать закрытие назначения «восстановить уже приобретённое и проверить сохранение». Движение неверных ответов и параметров доказывает update, не retention.

**Логический контрпример, не запускался.** Ошибка `LoadCheckpoint`, `TryImportPortableState` или `RestoreRandomState` может полностью сломать загрузку реального donor; эти три Fact не вызывают ни один из этих путей и остаются такими же. Это не предположение о наличии ошибки loader — это точная граница теста.

### 3.2. Даже внутри нового fixture полный terminal state не закреплён

`Assert.NotEqual(saved, run.Final)` доказывает только некоторое движение. Равенство `first.Final == second.Final` доказывает повторяемость одной и той же реализации. Четыре pinned losses измеряются до соответствующих updates: [Trainer.cs:117](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:117), строки117–147; после четвёртого update остаются только четыре argmax.

**Точный логический контрпример, не запускался.** После четвёртого `optimizer.Step` детерминированно прибавить1 к одному значению input embedding токена63. Ни один lesson/readout не содержит token63. В модели input embedding и output head создаются отдельно ([LlmModel.cs:104](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:104), строки104–109), поэтому такая порча не меняет эти logits или argmax. Все четыре losses уже вычислены; AdamW Step/moments не меняются; веса всё ещё отличаются от saved; оба повторных запуска получают одинаково испорченный Final. **Все три новых Fact сохранят свои assertions**, хотя один terminal parameter неверен. Это проверяемая логика кода, не результат выполненного mutation run и не заявление о прохождении всей тестовой suite.

У проверки «моменты не пустые» есть ещё более простой false-positive: bytes fresh step0 сравниваются с bytes step4 (:157–159), а [AdamWPortableState.ToBytes:152](C:/Projects/llm_mvp_core/04.TrainingLoop/Optimizers/AdamWPortableState.cs:152) сериализует Step независимо от entries. Если exported state теряет все entries, но сохраняет Step4, inequality с freshStep0 остаётся истинной, повторные bytes совпадают, losses/weights прежние. Таким assertion нельзя доказать наличие m/v. Существующие отдельные optimizer tests могут ловить часть подобной порчи; это не делает данный Fact самостоятельным full-state oracle.

### 3.3. Повторяемые moments не являются снимком RNG

В :185–190 и66:638–642 из повторяемости optimizer bytes делается вывод о генераторе. Здесь нет прикреплённых LoRA adapters, native capture/restore и сравнения их потоков. Даже в stochastic-пути неиспользованный лишний draw в конце программы способен изменить будущую последовательность, не меняя уже вычисленные веса/moments. Следовательно, равенство их bytes не влечёт равенство RNG cursor. Текущий plain deterministic smoke не доказывает stochastic continuation.

**Будущая точная правка.**

1. Сохранить полезный tiny numerical smoke с честным именем/комментарием и границей «повторяемость текущего plain-model step». Убрать из его описания и66 утверждение, что он закрывает assigned restored-learning/full-state/RNG qualification.
2. Реализовать ранее назначенный native fixture владельцев41/44: точные existing family bundles, native keyed checkpoint/AdamW/RNG restore, проверка trained T1 до updates; при missing/incompatible donor завершать отказом, не менять donor, LR, модель или бюджет и не запускать подготовку.
3. Разделять native A/B одной дельты и B/B repeatability. Сравнивать terminal keyed shapes/values/RequiresGrad, frozen snapshots, ожидаемый набор optimizer keys, обе moment arrays и Step, RNG entries. Для pure resource factor нужен независимый A receipt или независимая старая композиция, а не ещё один запуск B. Сохранить ранее назначенные 4updates/4BEFORE/4AFTER на руку; не выдавать этот документ за разрешение их запускать.
4. Предусмотреть sensitivity к порче одного terminal веса при неизменных losses/argmax, к утрате одной moment entry при правильном Step и к изменению только RNG cursor. Такие будущие контрпримеры следует учитывать отдельно; не прятать дополнительные учебные запуски внутри прежнего бюджета.

**Приёмка.** Native загрузка проверяет exact family/identity и optimizer400; T1 уже верен; назначенная программа заканчивается на404. Состояние A/B совпадает по независимому эталону, а каждый из трёх перечисленных дефектов обнаруживается соответствующим сравнением. Широкое усвоение правила не требуется: historical donor имел1/2TRAIN и constant-label limitation. Требуется сохранить уже наблюдавшийся acquired case и отделить этот результат от0→0.

## 4. Что исправлено по R2 и R3

### R2 / ATS: прежняя ошибка области исправлена в источнике

`4a7839d4c0b` заменяет общую reuse-ветвь на [TensorOps.Transpose:866](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:866): `rank==4` и нормализованные оси1↔2. Адрес назначения `((b*cSize+c)*aSize+a)*width+d` соответствует прямой перестановке этих осей; иной rank/пара осей возвращены в generic mapping с прежней выделяемой координатой. Identity/2D и inverse backward сохранены. Так устранён конкретный mismatch F3 старого аудита, а не просто изменён комментарий.

Тесты [AttentionTransposeStrideTests](C:/Projects/llm_mvp_core/Tests/AttentionTransposeStrideTests.cs:39) сохраняют независимый coordinate-walk oracle, оба порядка/отрицательные оси, bitwise round-trip. Новые случаи :131–223 охватывают накопление двух градиентов с неодинаковым upstream, отмену после progress с неизменным input и rank5 fallback. Это прочитанные assertions, не заново выполненные PASS.

Открыто: назначенная native learned-state/resource A/B qualification владельца45. Историческая экономия AT01 не является новым измерением исправленного HEAD; отсутствие этой qualification не основание снова переписывать правильную узкую формулу.

### R3 / SVD: настоящий Run использует общий training core

`f9694321424` удаляет inline counts→PPMI→SVD из [E10SvdPmiEmbedding.Run:103](C:/Projects/llm_mvp_core/34.Experiments/Experiments/E10SvdPmiEmbedding.cs:103); :109 вызывает `FitFixedTraining(train)`, :117 явно вызывает `ArgmaxDirected`. Split и ID-cap остаются в Run:75–80. Source similarity сохранён отдельным reader:226–227; right factor сохраняется в :206–207. Прежний F4 устранён в реальном consumer.

Новый [SvdDirectedTargetReadingTests:94](C:/Projects/llm_mvp_core/Tests/SvdDirectedTargetReadingTests.cs:94) вызывает настоящий Run ровно на назначенных500 IDs `i%3` и независимо требует100% у Bigram и directed SVD. Эта циклическая цель чувствительна к возврату симметричного reader; исходные три label-cycle cases и similarity contract сохранены. Runtime PASS этого consumer в данном аудите не воспроизводился.

Открыто: historical пятицелевая before600/after2400 программа57§6, её self-controls и native receipts. Трицикл не заменяет эту панель. Сама common-core правка не доказывает перенос на другой SVD-потребитель или обучение neural RP01.

## 5. Проверенные исторические данные и границы mutation-свидетельств

Прочитаны JSON-данные A/B, а не только prose reports. Сравнение ниже — обработка уже сохранённых файлов, без запуска model code.

| Источник | Что проверено в содержимом | Граница |
|---|---|---|
| [AT01 A](../../../../../experiments-v2/RP01-MICRO/AT01/a001/results/A/code-sign.json), [B](../../../../../experiments-v2/RP01-MICRO/AT01/a001/results/B/code-sign.json) | Сохранённые `parameters_final` и `optimizer_final` A/B совпадают как JSON-значения; losses `[1.1465623,1.2090048,1.1422932,1.2038769]` одинаковы; allocated13640016→12457296; donor optimizer400 | Это старый host/pin66947785…, не native qualification текущего ATS |
| [RD01 A](../../../../../experiments-v2/RP01-MICRO/RD01/a001/results/A/code-sign.json), [B](../../../../../experiments-v2/RP01-MICRO/RD01/a001/results/B/code-sign.json) | Losses и terminal trainable/optimizer SHA совпадают; allocated13640016→12984816; before T1 «+» верен, T2 ожидает«−», выдаёт«+» | RD raw здесь содержит state hashes; их равенство не заменяется заявлением, что все actual массивы заново восстановлены |
| [CS01 A](../../../../../experiments-v2/RP01-MICRO/CS01/a001/results/A/result.json), [B](../../../../../experiments-v2/RP01-MICRO/CS01/a001/results/B/result.json) | Native restore receipt:16keys, optimizer400; JSON terminal weights/optimizer records и online-loss bits A/B совпадают, final404; T1/E2 верны до/после, T2/E1 неверны | State descriptions/serialized moment data сравнивались; live restore не выполнялся |
| [MB01 A](../../../../../experiments-v2/RP01-MICRO/MB01/a001/results/A/code-sign.json), [B](../../../../../experiments-v2/RP01-MICRO/MB01/a001/results/B/code-sign.json) | Terminal records/loss bits совпадают. Фактические A/B файлы `S1.weights.bin`320620B, `S1.optimizer.bin`7384B и `S1.rng.json`761B имеют одинаковые bytes; это hydrated файлы, не LFS pointer | Это реальное сравнение сохранённых bytes, но не проверка совместимости их загрузки текущим HEAD |
| [SVD01 metrics](../../../../../experiments-v2/RP01-MICRO/SVD01/results/attempt-0001/metrics.json) | A-after2/5 и directed0/3, B-after5/5 и directed3/3; self-controls2/2; before3/5 и directed1/3 в обеих руках; payload120000→240000B | Проверено содержимое сохранённого опыта; новые fits не выполнялись |
| [LC pair-summary](../../../../../experiments-v2/RP01-MICRO/N61-REF-CACHE/results/pair-summary.json) | Recorded acquired IDs после1000 preparation; after A/B46/46 и44/44, одинаковые NLL/margins | Этот файл — уже составленный summary. Full terminal policy/AdamW/RNG в нём не доказаны; owner13 признаёт weights-only start и отсутствие исторических m/v/RNG |

Исторические AT/RD/CS/MB данные подтверждают **ненулевой acquired-before T1**, но не правило знака: TRAIN1/2 совместим с константой. Поэтому новый0/4→0/4 smoke нельзя признать той же квалификацией. Это не требование поднять quality floor до2/2 или переучить donor.

66 сообщает: LC skip-last-step mutation; CS perturbation `[1,0]`; RRD/MB multiplier1.0001; отдельные numerical perturbations общего теста и skip-last-update. У этих сообщений нет ссылок на retained raw/TRX. В переданном пользователем пакете, по сообщению родителя, были subjects коммитов без raw-путей. Адресный инвентарь `TestResults`, `.tmp`, `.codex-test-temp` не дал numerical repair log27.10. **Это author-reported mutation results, не независимо проверенные здесь запуски; отсутствие найденных файлов не доказывает, что автор не запускал их.** Поиск всей файловой системы не проводился.

Связь CS caller подтверждается исходником [CausalSelfAttention.cs:280](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:280); rectangular WithPast:310 остаётся MatMul. Предложение «старый consumer остался в текущем источнике» не подтвердилось. Но порча результата текущего `CausalScores` и возврат consumer на старый dense kernel — разные контрпримеры: numerical равенство двух модельных путей само по себе не доказывает, что потеря ресурсной оптимизации обязательно провалит тест. Это граница coverage, а не новая найденная ошибка текущего caller.

## 6. Ограниченная независимая проверка двух находок родителя

По отдельному bounded поручению родителя прочитаны названные тесты и owners. Ниже — review его находок, не собственная новая серия runtime-проверок.

### IM — body oracle проверяет только маршрут: подтверждено, P1

[OrchestrationPlanTests.cs:266](C:/Projects/llm_mvp_core/Tests/OrchestrationPlanTests.cs:266), строки266–298, утверждает доставку тела после restore, но проверяет `flow_ref=FLOW-33` и отсутствие других FlowRef. [TrainedRouteComposerUnit:19](C:/Projects/llm_mvp_core/42.ProjectWikiDomain/TrainedRouteComposerUnit.cs:19) создаёт этот header из `grounding.FlowRef`; независимо :34 вызывает `SerializeChosen`, а :86–88 добавляет chosen FlowRef/title/PlanText.

**Контрпример, не запускался:** убрать только `chosen.PlanText` из SerializeChosen:88. Header, выбранный route, title и отсутствие чужих IDs сохраняются; тело исчезает; новый Fact остаётся удовлетворённым. Это точнее мутации «заменить всё тело чужим», которую наличие чужого ID действительно может обнаружить.

**Будущая правка:** в этом Fact закрепить независимые содержательные строки/шаги ожидаемого `Chosen.PlanText` из training source для FLOW-33; при необходимости — отдельные два payload одной FlowRef разных версий. Проверять восстановленный body, а не только headers. **Приёмка:** удаление только PlanText при целых IDs проваливает body assertion, исходный consumer проходит. Фактическая порча production body не установлена.

### BPE — отношение1×/8× не защищает no-match-copy: подтверждено, P2

[BpeWeightedChunkTests.cs:222](C:/Projects/llm_mvp_core/Tests/BpeWeightedChunkTests.cs:222), строки222–255, считает расходы Train одного и восьми повторов и требует `eight < once*1.5`. [BpeTokenizer.cs:599](C:/Projects/llm_mvp_core/01.Tokenizer/BpeTokenizer.cs:599), строки599–603, вызывает MergeChunk по weighted representatives; no-match :632–633 возвращает исходный список. Адресное чтение и поиск `Same/ReferenceEquals/MergeChunk/GetMethod/Reflection` по полному содержимому `BpeNoMatchCopyTests.cs` и `BpeWeightedChunkTests.cs` не обнаружили reference-identity assertion этого результата; полное построчное чтение обоих файлов не заявляется.

**Контрпример, не запускался:** вернуть `new List<int>(ids)` вместо `ids` только при no-match. Learned vocabulary/hash/IDs остаются прежними. При одинаковых representatives и merge-проходах добавленная ценаΔ приходится на обе стороны1×/8×. Если R≥S>0, `(R+Δ)/(S+Δ)≤R/S`: относительный threshold становится слабее. Корпус повторяет те же word chunks; разница количества одиночных newline не создаёт новых соседних пар. Даже без обещания точного числаΔ этот тест предназначен для BW grouping и не устанавливает бюджет конкретного FIX-BPE no-copy path.

**Будущая правка:** добавить узкий no-match ownership oracle для существующего private MergeChunk через контролируемый test adapter/reflection: отсутствие пары возвращает тот же список и не меняет данные; наличие пары — нужные values и отдельное владение. Это ноль нового обучения. Сохранить отдельный BW1×/8× тест и historical tokenizer SHA; не подменять одно другим. **Приёмка:** единственная мутация `return new List<int>(ids)` проваливает no-copy oracle при прежнем semantic gold. Наличие самой оптимизации в продукте не оспаривается, устойчивое ускорение не измерено.

## 7. Дальнейшее ограниченное действие

Родитель может перенести N1/N2 и подтверждённые IM/BPE coverage gaps в общий ремонтный план с точными владельцами. R2/R3 надо закрывать по прежним конкретным source-findings, сохраняя открытые native historical/resource программы. Новые численные product deltas по этому review не обоснованы.

Этот документ — исследовательский deliverable. Ни тестовый ремонт, ни новая qualification, ни предложенные negative controls им не выполнены и не разрешены автоматически. Для запуска требуется прямое поручение пользователя в рамках фиксированного бюджета.

Проверка честности перед выдачей:

```text
claim_scope: 2 numerical evidence findings; narrow R2/R3 source closure; review of 2 parent findings
evidence_scope: actual source/diffs and retained historical data; no new runtime runs
count_claimed: 1 worker packet, not N passes
independent_evidence_count: source, specified commits and retained files named above; not test cycles
subagent_id_or_NONE: /root/repair_numerical_audit; no nested subagent
downgrade_needed: full native qualification and mutation-run success are not claimed
```
