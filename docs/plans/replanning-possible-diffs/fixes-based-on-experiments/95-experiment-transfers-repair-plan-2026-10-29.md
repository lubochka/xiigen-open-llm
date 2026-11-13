# 95. Адресный план исправления переносов и постановок — 29 октября 2026

<!-- transfer-entry-reload -->
**Вход исполнителя.** [R42](01-registry.md#work-29-w1). Это отдельное чтение результатов. Для него достаточно сохранённых входов и указанного ниже проверяющего. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Текущий остаток29-D3/29-W1 —4 ноября

Первые ремонты embedding/warmup и их историческая приёмка сохранены в103. Старые инструкции ниже о первичной реализации относятся к29 октября. **Новая подтверждённая проблема — файлы двух S1**, а не отсутствие алгоритма: восстановить original по[155§2](codex-155-postq003-addressed-repair-plan-2026-11-03.md) и[157](codex-157-postq003-executor-context-repair-2026-11-04.md). Владельцы прежние:29-D3 / [план88, раздел Embedding](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) дляP11,29-W1/§1 иR42 дляP12. Ремонт не повторяет SFT, не меняет manifest и не закрывает FIX-MC.

**Передать исполнителю:** «Восстанови exact S1 из lineage конкретного донора. Сверь hash/bytes и назначенный native readback отдельным тестом; старые EmbeddingLearningReachabilityFixture и WarmupFirstSupervisedUpdateTests сначала обучают. Не используй их как Train0-проверку. Сборка, discovery, технические тесты и обоснованная адаптация входят в этот назначенный ремонт по46. Недоступные original bytes обозначь явно; новый Train не разрешён. Сохрани принятый алгоритм, historical результаты и неполноту private schedule counter P12. Результат — сохранность/пригодность файла; новый прирост точности не заявляй. После R42 перейти кR47, если прямое назначение не задаёт другой ID».

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски TOK/STATE/RUN:** Разделить unknown ID, EOS и допустимость UTF-8 bytes. Lawful многобайтовый поток сохраняется, неизвестный ID/незавершённый terminal не исчезает молча; проверить ordinary caller и обе назначенные greedy/stochastic ветви, а не только новый overload. Проверить полный typed learned payload и применимые RNG/tokenizer/index/maps, состояние сразу после вмешательства и отдельный fresh-load только по назначенному API. Положительный старый ответ сохраняется; внешняя мутация/чужая identity должна обнаруживаться. Count/hash сами не доказывают восстановление. Привязать source/config/inputs к фактически потреблённым child bytes и effective MSBuild instances; late mutation против unchanged — разные controls. Parent terminal/exit не заменяется child Passed. Такой структурный guard не требует выдуманного model Train.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


## Цель и граница

План закрывает находки [аудита 94](audit-experiment-transfers-2026-10-29.md): сохраняет полезные обученные результаты, исправляет конкретные ошибки продукта и делает приёмку других карточек чувствительной к тем же ошибкам. Это план реализации, не отчёт о выполнении будущих native-тестов.

Основание: core `0c0228cdf5b1d0348b173c87f9e999b0af13253c`, planning `3b7537c267e45002ebab4f44845fe4ce92f204f9`. При применении сначала сверить diff с реальным текущим HEAD. Не сбрасывать чужие изменения и не накладывать патч вслепую на другой код. В рабочем каталоге пользователя корень core — `C:\Projects\llm_mvp_core`; planning определяется по фактическому checkout, а не по придуманному локальному пути. Команды ниже выполняются из проверенного корня core.

Общий порядок: восстановить исполнимость обучения там, где оно блокируется; исправить нарушение состояния/чтения; доказать native consumer; затем оценивать новые факторы. Независимые sampler, KNN и cache файлы можно исправлять отдельно. Общие проектные файлы/сборка и изменения одного файла координируются последовательно. Все исходные attempts сохраняются; новый run получает свободный suffix и конкретный бюджет. Q024/P23 остаётся следующим PRIMARY, пока канонический индекс не обновлён другим результатом; это отдельная очередь от ремонтных строк ниже.

## 1. Первый полезный SFT-шаг — закончить уже назначенный перенос

В нынешнем `04.TrainingLoop/Trainer.cs::TrainSupervisedNextTokenStep` первый warmup LR=0 вычисляется после Forward/Backward, затем шаг отвергается. Реализовать узкий [88/R29-W1](https://github.com/lubochka/llm_mvp_core/blob/0c0228cdf5b1d0348b173c87f9e999b0af13253c/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/88-gradient-warmup-transfer-repair-plan-2026-10-28.md). `CosineWithWarmup` глобально не смещать; default Warmup0 не менять. Допустимость LR проверяется до модели, сохраняя очищение градиентов и неизменность weights/moments/RNG/counters при отказе.

Состояние: `_shared-learned-starts/h64-rms-row/v1/` с исходными weights/optimizer/RNG/config; load-compatible tokenizer — `P1201/src/byte-identity-tokenizer.json`. В обе руки импортируются AdamW moments и step400; отдельный SFT scheduler начинается 0. Уроки T1,T2,T1,T2, warmup4/total8/maxLR0.001/minLR0.0001/decay0. Сравниваются нынешний native кандидат и отдельно измеренная политика P12 только для step0. Учитывать 32 модельных вызова по существующему 88; bootstrap0. Не использовать новый optimizer0 и не считать восстановление hash полноценным restore API приватного scheduler.

Приёмка: четыре разрешённых обновления; LR/loss/weights/moments и четыре ответа по ID совпадают с назначенной политикой; недопустимый LR даёт 0 Forward/0 Backward/0mutation. Сохранить небольшой S1 один раз по будущему пути из 88. Исторические 2/4 и ухудшение E1 не переименовываются в accuracy gain.

Команда после реализации: `dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter FullyQualifiedName~WarmupFirstSupervisedUpdateTests`. Новый класс пока планируемый; zero tests не PASS. Build≤600s, адресный runtime≤120s; при превышении записать конкретный отказ и продолжить разбор причины, а не запускать ещё один процесс без проверки первого.

## 2. Запретить токены нулевой массы в верхнем хвосте sampler

**Файлы:** `09.Inference/Sampler.cs`; `Tests/SamplerTests.cs`; boundary cases в `Tests/ExperimentFixes/P08SamplerCompositionTests.cs` и независимом reference при необходимости. Существующие r=0 tests оставить.

Алгоритм: один раз вычислить конечную положительную сумму допустимых probability values в double; сделать один RNG draw; умножить его на эту сумму; обойти те же значения со строгим `<`. Обнаруженные отрицательные/nonfinite вероятности и неположительная/nonfinite общая масса дают `InvalidOperationException` с отдельным стабильным сообщением `SAMPLER_INVALID_PROBABILITY_MASS` до RNG. Защитный fallback возвращает только последний positive index. Не исправлять это возвратом 0 или argmax: они меняют назначенное распределение. Нельзя переводить softmax в другую температуру или выполнять второй draw.

Входы: сохранённые T1/E1 logits P0801, topK2/topP1, near-one `Math.BitDecrement(1d)`; простой `[2,0,-5]`; ноль, .5 и соседние representable границы; uniform/temperature0 controls; masked last vocabulary ID; mass slightly above/below1. Expected support берётся из независимой композиции, не из вызова продукта. Недопустимые массы проверять через доступный узкий internal probability seam, если текущий публичный вход не позволяет чисто задать их; не добавлять публичный API ради теста.

Доза новых tail-regression cases:0 Train/0 model Forward/0 Backward/0 optimizer и не более 64 Sample calls. Seed progression должен сохранить ровно один NextDouble для допустимого stochastic вызова и 0 для greedy/invalid preflight. Native-тест исходного дефекта должен падать до правки и проходить после. Изменение редких пограничных seeded ответов описать, не скрывать ослабленным assert.

Назначить новый отдельный класс `SamplerUpperTailRegressionTests` и запускать `dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter FullyQualifiedName~SamplerUpperTailRegressionTests`; build≤600s/runtime≤120s. Zero tests не PASS. Если для сохранения прежних r=0/composition contracts нужен запуск существующих тестов, перечислить точные методы и отдельно учесть их Sample/model calls до запуска: они не входят в ≤64 и 0 model Forward нового класса. Весь `SamplerTests`/composition suite этой командой не запускается. Не повторять 8800 исторических resource calls: этот ремонт не снимает memory HOLD композиции K→p и не доказывает ускорение.

## 3. Замкнуть владение metadata у TransformerEmbedding KNN

**Файлы:** `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`; tests в `Tests/KnnDomainPrincipleLearningTests.cs` или отдельном адресном классе с тем же действительным fixture. Владельцы — [74](https://github.com/lubochka/llm_mvp_core/blob/0c0228cdf5b1d0348b173c87f9e999b0af13253c/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/74-knn-domain-constructor-transfer.md) и конкретные in-process string consumers.

При `Train` до помещения в map захватить owned copies PredicateIds/DomainTags. Null/malformed carrier отвергать через существующий OperationResult до публикации нового Unit; при обязательном наличии этих массивов вернуть код `INVALID_TRAINING_EXAMPLE`, без частично возвращённого экземпляра. При `ExecuteAsync` выдавать новые copies; shared native/ranked view сохраняет одну неизменяемую внутреннюю память. Для текущего string Output отдельный сериализатор не нужен. Mutable generic Output не обещать поддержанным без названного фактического типа и симметричной snapshot policy.

Приёмка: после одного захвата 1-row memory исходный caller меняет обе непустые metadata arrays; затем caller меняет arrays первого результата. Следующие native/ranked ответы сохраняют literal original tags/predicates и example ID, без Train/restore после атаки. Меняются copies, а не «ожидаемый» объект из того же alias. Проверить null input по envelope отдельно. Успех только ingress-copy при оставшемся egress alias не принят.

Бюджет адресной 1-row проверки: один существующий BuildTokenizer BPE Train на фиксированном корпусе с vocab320, один обычный KNN Train/одно предъявление/одна запись индекса, шесть Execute;0 SFT/0 DPO. Модель — ограниченный existing test fixture; новый bootstrap нейросети не назначается. Это маленькая контрактная проверка, не новый опыт качества и не повтор большого KDP обучения. Более широкие исторические RU/HE/OR exact-answer регрессии остаются своими существующими work orders.

## 4. Очистить reference cache после принятого restore того же DPO owner

**Файлы:** `11.DPO/DpoTrainer.cs::TryRestoreContinuationState`, `Tests/DpoReferenceScoreCacheTests.cs`; существующий proof-bearing образец в `DpoTrainerOptimizerStateHandoffTests`.

Очистить private cache в успешной ветви после завершения разрешённого импорта и до true. Сохранить admission/continuation proof, неизменность frozen reference и существующую область синхронизации. Pre-admission rejection до мутации не меняет прогретое состояние; failure после начала restore использует прежний rollback с его очисткой. Отдельно уточнить в 13 эту фазовую границу вместо буквального blanket «любая ошибка очищает всё».

Проверка создаёт proof для действительно допустимого saved snapshot до прогрева; тот же owner делает StepVector двух пар, затем второй шаг с hit; принимает свой разрешённый snapshot; третий StepVector требует два misses и 4reference Forward. Null-proof helper существующих cache-тестов для этой ветви не годится. Нельзя заменять owner новым экземпляром или отключать proof.

Бюджет конкретного сценария: один подготовительный BPE Train("fixture",256), затем 3 StepVector,6 предъявлений пар, reference8/policy12/Backward12,3 optimizer updates по текущему control flow. До запуска сверить дополнительные диагностические вызовы fixture и записать отдельно; не подмешивать их к этой формуле. Counter assertions используют дельты. Typed RNG entries/state/cursor, optimizer bindings/moments/step и policy/reference сравниваются по полям. ToString не oracle. Удаление только successful-restore Clear должно ломать тест.

Добавить один сценарий `AcceptedContinuationRestore_OnWarmedSameOwner_RequiresReferenceMiss` и запускать `dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --filter FullyQualifiedName=LlmFromScratch.Tests.DpoReferenceScoreCacheTests.AcceptedContinuationRestore_OnWarmedSameOwner_RequiresReferenceMiss`; build≤600s/runtime≤120s. Zero tests не PASS. Указанный бюджет относится только этому методу; полный существующий cache suite требует отдельного учёта и здесь не назначен. Не повторять H512/1000-step historical preparation; это lifecycle regression с ограниченной дозой, без утверждения нового quality gain.

## 5. Завершить две уже назначенные реальные consumer-проверки

**Sampler с actual trained logits:** исполнить [92/R30-S1](https://github.com/lubochka/llm_mvp_core/blob/0c0228cdf5b1d0348b173c87f9e999b0af13253c/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/92-positive-transfer-followup-plan-2026-10-28.md). Загрузить P0801 S1 и передавать в native Sample logits, полученные из него, а не savedRow.Logits. Сохранённые logits только oracle. Четыре строки дают 4 Generate/4 prefill/4 Sample,0decode/0 Backward/0 updates. Сверить 256 logits по назначенному допуску и output IDs; не увеличивать допуск после ошибки. Memory HOLD остаётся.

**Learned cache chunks:** исполнить 92/R30-C1 в существующем `CacheAwareForwardTests.RestoredTrainedStateOfTheReadingExperiment_EmptyCachePrefillMatchesOrdinaryForward`. Использовать именно P0901 S1, четыре 21-token prompt, windows7/7/7, absolute offsets0/7/14. Всего 4 ordinary+4 empty+12 chunked=20 Forward,0 Train. Сравнить каждый slice и final output; не подменять ошибочный исторический E1 правильным gold. Fresh cache у каждой строки. Эти две проверки не сливаются в одно виртуально выполненное e2e доказательство.

**Приоритет:** после исправления соответствующего native пути; не менять LlmModel до выявления конкретного дефекта. Source hashes и runtime receipts записать в новые R31 consumer directories. Filters/лимиты существующего 92 сохраняются.

## 6. Подготовить достижимую проверку pruning без повторного BPE

Current P20 attempt остаётся recorded/HOLD. В его amended карточке уже находится отдельная доучебная проверка достижимости. Вход: A-trained tokenizer/index/state и Arms[].Vectors/After[].QueryVector из P2001 metrics; source SHA и checksum каждого input фиксируются. Сохраняются 8 ID/order, m2/efConstruction64/efSearch32/seed42 одинаково A/B. Две структуры строятся из frozen learned vectors, отличается только защита рёбер.

Существующий P2001 `ExperimentStateAdapter` умеет только Save и пишет `RestoreClaimed=false`; обычный `Train` заново обучает BPE и жёстко создаёт m8. Поэтому создать в новом runner отдельный partial factory `RestoreReindexedExperimentState` для того же экспериментального `KnnLogicalUnit`: десериализовать A-trained `state.json` и `tokenizer.json`, скопировать сохранённый ID→example/output mapping и переданный selector; построить новый `SeededHnswIndex(m:2, efConstruction:64, efSearch:32, seed:42, protectEdges:armFlag)` ровно восемью Add сохранённых vectors в исходном порядке; вызвать существующий private constructor Unit с новым index. Обычный `Train` и подмена M в старом `index.json` не используются. До readout сверить ID/order/vector bytes и mapping с источником. Обычный Execute каждой руки строит query прежним tokenizer; captured query сравнивается с закреплённым After.QueryVector, который служит только oracle. Factory/diff включаются в source/build manifest; это новый диагностический adapter, а не существующий native restore.

Измерить ReversePruneDecisions и именно предотвращённые удаления; registered-protection-count не подходит. При нулевом trigger не делать вывод о пользе защиты. Сначала независимый exact scan и обычные answers той же памяти; отдельно Count/degrees/reachability/serialized bytes. Это 0 нового BPE,0 изменений обученных векторов;16 Add для реконструкции и 24 обычных query плюс 24 exact scans/192oracle distance comparisons по фиксированной панели. Внутренние расстояния HNSW при построении/поиске считаются отдельно, а не объявляются нулевыми. Контроль не называется новым самостоятельным опытом обучения.

Если нужен последующий причинный опыт качества при обучении, назначить отдельный полный recipe с реальным учебным переходом, сохранённым S0 и nonzero readout; не запускать его автоматически из этого документа и не менять N/m/seed после просмотра результата. Для m8 необходим минимум 18 узлов; это не достаточная гарантия.

## 7. Довести shape preflight до допуска valid и invalid входов

Владелец P21. Сохранённый S0 используется в двух независимых clones. На clone1 guard ON отвергает прежний 63d bad до изменения Count/IDs/graph и сохраняет все 12 обычных ответов. На clone2 guard ON принимает один 64d vector с новым `valid-next` ID. После успешного Add Count=9, IDs содержат valid-next. Для index membership один раз вызвать `Search(validVector, topK:9)` при efSearch≥9 и требовать valid-next в result set. В bounded Unit adapter клонировать сохранённый example u1 под новым ExampleId=valid-next и поместить в mapping после успешного Add. Независимым typed readback проверить новый ID и закреплённые output/metadata. Одного index.Add без mapping для Unit недостаточно.

Фиксировать входной vector bytes из S0.u1, соответствующий ему output и новый уникальный ID до запуска. Обычный Unit query с Input/metadata u1 сохраняет ожидаемый output; выбранный ID фиксируется в receipt. Среди совпадающих vectors top1 может законно остаться u1: одинаковый output не доказывает чтение именно valid-next. Consumer-proof нового ID потребует отдельного контраста с заранее выбранным различимым query/vector, а здесь не заявляется. Не менять tie-policy, которая не является фактором. Контроли reject-all и disabled-guard должны проваливаться по разным причинам. NaN/Inf/alias/mid-Add rollback не объединять с доказанным shape-guard статусом.

Основной сценарий:0 нового BPE/0 bootstrap,2 restore,1 invalidAdd attempt+1 validAdd;12 post-fault Unit queries+1 ordinary valid query и 1 index Search(topK9). Два одноразовых mutation-контроля учитываются отдельно: reject-all выполняет только valid case до ожидаемого провала acceptance assert, disabled-guard — только invalid case до ожидаемого провала rejection/no-mutation assert. Каждый добавляет 1 restore и 1 Add attempt; остановка на ожидаемом assert без дополнительных query-панелей. Итого назначено 4 restore и 4 Add attempts, без новых BPE/учебных переходов. Если control неожиданно не падает, тест нечувствителен; дозу и fixture не увеличивать. Это acceptance regression на обученном состоянии; нового quality acquisition не заявлять. Исторический runtime не повторять ради report.

## 8. Зафиксировать native/adapted границу RNG

В P22 passport называть C controlled cold-cursor SplitMix64, не native legacy. Stream equality, graph equality и answer correctness — отдельные axes. Общее 6→12 от новых уроков не записывать в B-exclusive delta.

Перед product переносом owner обязан выбрать schema/version и владельца RNG: per-index новый stream либо осознанно совместимый shared stream. Текущий native System.Random не имеет доказанной побитовой миграции в SplitMix64. До выбора и проверки такого договора общий native transfer остаётся HOLD. Независимые interleaved indices, continued Add, Save/Load и отказ на чужой версии — назначаемые acceptance cases. Этот пункт не выдаётся за готовый recipe нового алгоритма: это явно названное архитектурное решение до допуска изменения RNG.

## 9. Привязать следующие результаты к реально запущенным bytes

Использовать существующий пример P05Q01 source-snapshot/build-binding. Для следующего создаваемого runner P23/P24/P25: manifest до launch включает пути и SHA256 Program/src/csproj, linked native source и dependency/assembly identities, native diff относительно pinned base, command/args, config/tokenizer/dataset/state hashes. После build сохранить hashes запускаемой DLL и runtime dependencies; сверить перед единственным launch и сохранить run ID в receipt/metrics. Это 0 model calls и не повод переобучить старый опыт.

HEAD родителя не идентифицирует незафиксированный runner. Позднее изменение source после run создаёт новую версию manifest, не переписывает историческое свидетельство. LFS pointer проверяется по формату до JSON/binary load; при недоступном payload статус конкретно missing payload, а не «файл пуст» и не fallback на новую базу.

## 10. Закрытие ремонта и движение дальше

Для каждой строки: concrete diff → адресный native тест с ненулевым discovery → источник результата → owner card и при необходимости index. Пройденные поля и открытые границы независимы. Если последний процесс ещё жив, ждать тот же handle с bounded wait; не запускать duplicate. Прямая команда пользователя STOP/NEXT сохраняет приоритет.

P05 writer уже исправлен; не повторять закрытый R29-Q1/Q2. P05Q01 full-answer0/6 сохраняет цель OPEN. Его 400 SFT не повторять из-за красивого нового отчёта: следующий учебный recipe обязан объяснить одну новую причинную переменную и пригодный saved state. KDP descriptor уже frozen; отдельно остаются configured restore/actual answers, новые metadata границы и ProductHost. Действующие 69 resource/admission constraints сохраняются; неизвестные ресурсные оси не становятся нулевыми.

## Что реализовано документным пакетом сейчас

Уточнены current/historical паспорта, common-контракты, адресные owners и будущие карточки; подготовлены audit94, repair95 и applicability96. Это не объявление native фиксов выполненными. Пакет включает unified patches и проверку их применимости к материализованным base-файлам. Ветка GitHub этим пакетом не изменена.
