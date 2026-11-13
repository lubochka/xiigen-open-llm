# N27-B — Mutability alias между checkpoint cache и обучаемой машиной: грязное состояние после неудачного сохранения

**16 октября 2026 · `SOURCE_CONFIRMED_MUTABLE_ALIAS / CONDITIONAL_STALE_STATE / NATIVE_CAUSAL_PROBE_NOT_RUN / DRAFT`.** Pinned repository: `lubochka/llm_mvp_core@8c7d41611aecba98d3d1bbdbf30d69bd784949ae`, `codex/dpo-implementation`. Только документация `xiigen-planning@llm_plan3/.../possibleDifs`. core protected docs, training state и test code не изменялись.

## Объяснение без подмены понятий

**Checkpoint на диске и checkpoint, повторно полученный из RAM-кеша, могут перестать описывать одну версию обучения, хотя оба выглядят успешно «загруженными».** Это возникает не потому, что serializer теряет поля: объект `TranslationMachine`, положенный в `TranslationCheckpointCache`, является **изменяемой живой машиной**. Кеш возвращает **тот же экземпляр**, и следующая порция Unit-learning меняет его прямо в кеше **до** записи новой версии на диск. Если этот цикл сорвался перед успешным Save, кеш способен выдать состояние *после незафиксированного обучения* вместе со старым cursor из disk checkpoint.

Это отдельная гипотеза от опубликованных [N23 non-atomic Unit training](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md) и [06 restore/adoption](06-CHECKPOINT-RESTORE-ADOPTION.md): там обсуждается целостность batch/checkpoint и использование состояния; здесь — **aliasing mutable object inside cache across runs and crash boundary**. Переобучение DPO Student RP01 этим не доказывается: описанный путь относится к Tatoeba/TranslationMachine structured Unit learning.

## Прямые доказательства, source → caller → consumer

| Шаг | Свидетельство | Следствие |
|---|---|---|
| 1. Тип объекта | [`TranslationCheckpointCache.cs` L14–24](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationCheckpointCache.cs#L14-L24): `Dictionary<CacheKey, TranslationMachineLoadResult>` | Кеш содержит loaded result, внутри которого живая `Machine`, не checkpoint DTO/immutable snapshot |
| 2. Alias на выдаче | [L28–38](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationCheckpointCache.cs#L28-L38): `_cache.TryGetValue(key, out result)` | Без клонирования/десериализации возвращается ссылка на тот же результат |
| 3. Alias на записи | [L41–50](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationCheckpointCache.cs#L41-L50): `_cache[...]=loadResult` | `Store` не делает snapshot; key = абсолютный путь + schema=1 + `LastWriteTimeUtc.Ticks` + `Length` |
| 4. Resume | [`TatoebaPilotLoopRunner.cs` L184–211](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L184-L211): `loadedCheckpoint=cachedResult`; `machine=loadedCheckpoint.Machine` | Active learner — ровно кешированный mutable instance |
| 5. Обучение до commit | [L444–475](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L444-L475) и [L747–771](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L747-L771): `TrainRowsAsync(machine, ...)` / `TrainStructuredBatchAsync` предшествуют `SaveLoopCheckpointAsync`; также source rollover [L224–251](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L224-L251) | До успешного Save исходный RAM object уже изменён |
| 6. Invalidate только после save | [L642–669](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TatoebaPilotLoopRunner.cs#L642-L669): `SaveCheckpointAsync` → при success `Cache.Invalidate` → `Cache.Store(new TranslationMachineLoadResult(machine,loopState))` | При exception/failed save до invalidate старая key-входная запись всё ещё ссылается на изменённый объект |
| 7. Реальная mutation | [`TranslationMachine.TrainStructuredBatchAsync` L107–145](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L107-L145) вызывает `trainer.TrainOnExample`; [`ExportCheckpoint` L282–300](https://github.com/lubochka/llm_mvp_core/blob/8c7d41611aecba98d3d1bbdbf30d69bd784949ae/27.TranslationDomain/TranslationMachine.cs#L282-L300) сохраняет KNN, maps, Unit rules, identities | Диск и кеш могут содержать разные наборы выученных уроков/курсоров |

### Проверенный логикой контрпример с 2 последовательными вызовами

1. На диске checkpoint C0: `KNN=K0`, `cursor=10`. Он загружен, а `Cache.Store(C0)` хранит объект M0.
2. Вызов с тем же cache получает M0 без disk-load, затем добавляет законную обучающую запись B: состояние стало `KNN=K1`, где `K1≠K0`.
3. Сохранение C1 срывается (например, контролируемый отказ записи в *тестовую* папку). `Cache.Invalidate` не вызывается, disk=C0, но объект M0 в кеше уже содержит K1.
4. Следующий вызов с cache при неизменном stat файла видит старый `cursor=10` и новый `KNN=K1`; без cache открывает disk=C0/K0. Это нарушение expected restart equivalence. При последующем повторе учебной B-строки возможна двойная запись, но её фактическое влияние/идемпотентность надо измерить отдельно.

**Доказаны из кода aliasing и расположение мутации/сохранения; сценарий потери write — условие, а не заявление о реальном инциденте.** Отдельный риск: ключ кеша использует mtime+length без hash, поэтому same-size rewrite с сохранённым mtime не распознаётся, но это самостоятельная условная эксплуатационная грань, не подтверждённая практическим прогоном.

## Микроопыт N27B — 1 checkpoint, 1 cache, 1 новый разрешённый Unit lesson

**Tier 1, без нейронной тренировки, без исходных продукционных путей.** Исполнять только в изолированной временной test directory с уже утверждёнными training examples. Исходный C0 checkpoint сделать штатным `SaveCheckpointAsync` и проверить его `LoadCheckpointAsync` до создания cache.

- **A — disk baseline:** `TranslationMachine.LoadCheckpointAsync(C0)`, сохранить `ExportCheckpoint` summary: `KnnStore.Entries.Count`, entries checksum, `TrainingHistory`, `StructuredTrainingIdentities`, versions, example-ID set и ordinary heldout translations.
- **B — cache alias:** `Cache.Store(path, loaded C0)`; `TryGet(path)` дважды; проверить `ReferenceEquals(hit1.Machine,hit2.Machine)=true` как *механическую* метрику.
- **C — uncommitted mutation:** вызвать `TrainStructuredBatchAsync(B)` на `hit1.Machine`, **не** вызывать Save; снова `Cache.TryGet(path)`; отдельно загрузить C0 прямо с диска. Записать `cache-state-digest != disk-state-digest` при одинаковом file-stamp. Два fresh heldout correct answers — для видимости эффекта в ordinary reader, но не подменять общую accuracy одного кейса.
- **D — crash/recovery simulation:** вызвать stateful runner с fault injection **после training, до successful Save**, убедиться, что `TryGet` выдаёт изменённый object, а `Cache=null` — старый disk checkpoint. Не удалять и не портить prod snapshots.
- **E — успешный Save control:** штатно сохранить, инвалидировать кеш и вновь загрузить из диска; expected cache/disk digest parity.

**Primary observable:** mismatch cache/disk full learner-state digest после отвергнутого commit при одинаковом path/file-stamp. **Secondary:** числа новых пар, ordinary accepted/correct/incorrect/abstain и replay counts, а не 'checkpoint loaded=true'. Фальсификация: actual tested `TryGet` получает полноценный глубокий клон, либо runtime гарантированно откатывает `Machine` при *любой* ошибке до `Invalidate` (проверить на полном caller). Если end-user consumer не зависит от cache, фиксировать только fault/restart integrity risk.

## Исправление после подтверждения

Основной invariant: **cache hit представляет только committed full-state checkpoint с известным digest/version, не текущий mutable learner**. Возможные варианты: кешировать immutable checkpoint DTO и всегда строить fresh machine на cache hit; copy-on-write перед training, commit cache atomically лишь после successful disk save; или строго исключить cache на mutation path. Сверить optimizer/LoRA и Unit checkpoints отдельно: не переносить из TranslationMachine ошибку на другой механизм восстановления без trace.

**Роли и права:** законные DPO chosen/rejected в иных экспериментах создаёт внешняя открытая китайская модель; выбор уроков CODEX (`astra 6 ultracode`); отдельная оценка unchanged DPO pairs также CODEX (`astra 6 ultracode`). Этот микроопыт ничего из этого не вызывает. Independent reviewer/cold native experiment — **NOT_RUN**.
