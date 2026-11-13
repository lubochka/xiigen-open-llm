# Проверка плана92: версия, замечания и граница принятия

Дата: 28.10.2026. Независимый от автора91/92 исследователь: `/root/p08_transfer_audit`, единственный worker Astra ultra. Фаза **READ_ONLY_RESEARCH + этот Markdown**. Проверены документы91/92, адресные места85 и относящиеся к их поручениям исходники/сохранённые входы. Новые tests/builds/training/inference не выполнялись; продукт, fixtures, raw и документы родителя этим worker не изменялись.

**Вывод:** два конкретных замечания к исходному candidate устранены в прочитанной финальной версии92. В проверенной области оставшихся блокирующих ошибок постановки не найдено. Это принятие ограниченного будущего плана, **не runtime PASS, не выполненный перенос и не разрешение запуска**. Общий P08 остаётся HOLD по памяти.

## Зафиксированные версии

| Документ | Прочитанная версия | Строк / байт | SHA256 |
|---|---|---:|---|
| [92](../92-positive-transfer-followup-plan-2026-10-28.md) | Исходный candidate, до двух уточнений | 75 / 22688 | `037a6f5bb78272d883b30b247fb731b4442ad9e834a698f4d856dc84de1b6518` |
| [92](../92-positive-transfer-followup-plan-2026-10-28.md) | **Финальная принятая постановка** | **77 / 23971** | **`3de119f448175a7c78080fb9251faaa8158c231e219d0cca8127e37c46f326bc`** |
| [91](../91-positive-transfer-followup-audit-2026-10-28.md) | Полностью прочитан; hash не изменился между чтениями | 51 / 14543 | `8d0999d4895182dc759cf0da4ec4c6f22077f3faeecb63df6309ed80e2a4092d` |

Финальный92 прочитан целиком после подтверждения записи автором. Повторное чтение исправлений — продолжение **одного** независимого review, не второй аудит или отдельный цикл. Вывод не распространяется автоматически на следующую редакцию.85 прочитан адресно для дедупа исполненных стадий и отмены повторного назначения8800Sample; полный повторный аудит85 не выполнялся.

## Два замечания и их фактическое закрытие

1. **Разные JSON keys.** Исходный92:20 предлагал связывать записи «по row_id» без различения файлов. Реальный reader [P08SamplerCompositionTests.cs:321–339](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/P08SamplerCompositionTests.cs:321) читает `after.json: rows[].id`, но `arms.json: model_samples[].row_id`. Финальный92:20 теперь называет оба точных пути и arm. Ошибка будущего reader предотвращена; данные и доза не изменены.
2. **E51 raw нет в checkout.** Оба May11 JSON по историческим путям возвращали `exists=false`. Они доступны как объекты локального commit `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`, а не как готовые test fixtures. Исходный92:64 не определял доставку этих входов. Финальный92:64 назначает два неизменяемых будущих файла, точный source blob, размер/SHA до parsing и BLOCKED_SOURCE без fetch/latest/BuildEvidence fallback. Это достаточный адресный маршрут входов; fixtures этой работой не создавались.

Независимое чтение через `git show <commit>:<path>` вернуло exit0 для обоих raw. Хеши считались по исходным bytes, включая UTF-8 BOM `EF BB BF`, без преобразования перевода строк:

| Источник внутри commit f6032aff… | Размер | SHA256 | Назначенный будущий test fixture |
|---|---:|---|---|
| `experiments-v2/E51A-CODE-CONTEXT-PIPELINE/results/run-2026-05-11-194251.json` | 16657 | `dd423917e21b533fe8d64120041393e25c804429107ef522b485d5f3c901c8d3` | `Tests/Fixtures/ExperimentEvidence/E51/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/E51A.json` |
| `experiments-v2/E51B-DYNAMIC-FLOW-MINI-E2E/results/run-2026-05-11-194252.json` | 14651 | `73bafb92ca2e2f8aaebea98ca7aee68b60003f023ec794fdd9321cbc967fae87` | `Tests/Fixtures/ExperimentEvidence/E51/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/E51B.json` |

## Что проверено по каждому пункту

| Пункт92 | Source/evidence сверка | Граница принятия |
|---|---|---|
| R30-S1, строки16–26 | [Текущий generator Theory:62–117](C:/Projects/llm_mvp_core/Tests/ExperimentFixes/P08SamplerCompositionTests.cs:62) действительно создаёт carrier model и подставляет `savedRow.Logits` в Sampler:93. [Consumer restore:271–300](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/consumer-completion/Program.cs:271) содержит real config/base/LoRA/LoadCheckpoint/RestoreRandomState. Все четыре after prompt имеют21 ID и256 logits. | Замена ровно четырёх существующих cases: **4Generate/4prefill/4Sample,0decode/Backward/updates**. Native Sampler получает actual callback logits; saved logits — только oracle. Допуск фиксирован заранее. Нет повторения восьми старых A/B consumers, нового SFT или constructor-only доказательства. |
| R30-C1, строки30–38 | [Learned Fact:300–443](C:/Projects/llm_mvp_core/Tests/CacheAwareForwardTests.cs:300) уже восстанавливает P0901 S1; читает gold-oracle/arms-logits:387–405; выполняет ordinary:408 и empty-cache:413 для каждой из четырёх строк. [AssertLogitWindowClose:550–576](C:/Projects/llm_mvp_core/Tests/CacheAwareForwardTests.cs:550) использует abs2e−5. | Три окна7/7/7, offsets0/7/14 и отдельный пустой cache на строку добавляют12Forward. **4ordinary+4empty+12chunked=20Forward**,0Sample/Generate/Backward/updates. Старые21504=4×21×256 comparisons сохраняются. Не нужен второй ordinary baseline; P0801 не заменяет provenance P0901 даже при равенстве байтов. |
| R30-S0, строки42–44 | [UniformToken:252–262](C:/Projects/llm_mvp_core/Tests/TextGeneratorSeededSamplingTests.cs:252) действительно использует `randomValue <= accumulatedProbability`. Для uniform4 правильные полуоткрытые интервалы имеют ids `floor(4r)` при0≤r<1. | Pure-oracle Theory вызывает только helper: **0Forward/0Sample/0updates**. Точные границы0/.25/.5/.75 и соседние representable значения — различающий контроль. Полный generator suite не спрятан в этом бюджете. |
| R30-R1, строки48–52 | [Consumer:304–309](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/consumer-completion/Program.cs:304) читает/импортирует optimizer404, после работы экспорт/проверка даёт404; receipt сохраняет `parent_lineage.optimizer_step_after=404` и `optimizer_step_after_completion=404`. `_updates` не инкрементируется, а `calls.backward_calls` — literal0:509–510. [Старая correction](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/consumer-completion-0001/evidence-boundary-correction.md:140) уже объясняет различие. | `optimizer_step_delta=0` из404→404 допустим как наблюдённый delta, а не счёт каждого когда-либо вызванного optimizer. Backward остаётся structural/unknown без выдуманного наблюдателя. Pure projection/readback не требуют нового host run. Нулевая доза — проверка существующего receipt, не новая программа без обучения. |
| R30-E1, строки58–68 | Сопоставлено с [предыдущим source packet](2026-10-28-positive-candidate-delta-source-audit.md), включая обе E51 WriteEvidence поверхности и generic ResultWriter/Runner. Новые fixture hashes выше совпадают с92:64. | E51-specific projection сохраняет observed качества, null+reason для synthetic values; generic legacy export явно отказывает `E51_QUALIFICATION_REQUIRED` до PASSED/NextAction/progress/ladder. `Core/ResultTypes.cs` и unrelated IDs не меняются. Чистый Evidence overload и internal ready-result seam исключают Dispatch/BuildEvidence/Train из адресного test class. **0Train/0model calls/0updates**; нынешние DynamicFlowE51Tests с живым BuildEvidence не входят в этот бюджет. |

Для R30-R1 важно сохранённое различие имён: `optimizer_step_before=400` в parent_lineage относится к исходному обучению, а serving BEFORE — **parent `optimizer_step_after=404`**. План92:50 правильно требует404→404 и source каждого значения; future reader не должен вычислять404−400 как дозу serving-проверки. Это уточнение использования существующей записи, не новый открытый дефект плана.

## Входы состояния и допуск

Проверено существование обоих actual BPE snapshots, donor generation.json, P0901 gold-oracle.json/arms-logits.json, `Tests/LlmFromScratch.Tests.csproj` и `Tests/ExperimentFixes/P08SamplerCompositionReference.cs`. Для P0801 и P0901 отдельно пересчитаны SHA трёх S1 файлов; во всех шести проверках digest совпал с **собственным** `metrics.json.learning.saved_state`:

| Файл | Размер каждого файла | SHA256 |
|---|---:|---|
| S1.weights.bin | 320620 | `d4817625ffcd3d5cb5939d2793731881393dd814050f0d9fff6f0585647b1e78` |
| S1.optimizer.bin | 7384 | `7b35c9cf47f601f76d98e6cb8e4fc34a870ef35a5fd1ccc84c2e321f7d458aff` |
| S1.rng.json | 740 | `fd5ebc3cc4be30392246ed07eed86c10dd97b1320a3e3ddf6a8e555c228d206e` |

Это проверка байтов и provenance, не вызов LoadCheckpoint, не inference и не доказательство runtime-совместимости будущего теста. Recipe обеих родительских попыток содержит T1,T2,T1,T2 и400→404; будущие проверки являются их продолжением, а не новой PRIMARY с нулём обучения. Новая стадия при missing/corrupt state должна остановиться с BLOCKED_STATE, не обучать замену.

В91:7 и92:10 корректно удержано **HOLD P08**: `(25872−10120)/10120·100 =155.6521739%` allocations наT2/E1/E2. Это не CPU-only ухудшение; критерий общего допуска не выполнен. T1 отдельный показатель25936 и средняя парная разность15803.2 не подменяют друг друга. Указано, что8800Sample уже измерены в203ba60, нового повторения нет. В этой проверке вся ресурсная статистика и измерительный host заново не переаудированы; подтверждается согласованность постановки/арифметики и сохранение ограничения, а не новое performance measurement.

91 явно отличает сохранённые журналы чужого исполнения от нового запуска аудитором.92:74 отделяет56owner docs/62ID от62 code audits и не добавляет FIX-ID. Матрица93 и все56 owner blocks **не проверялись этим review**; ссылка на них не расширяет его область. Полный тестовый корпус, другие карточки, native алгоритмы и прочие ветки повторно не аудированы.

## Финальная граница принятия

Финальная версия92 с SHA **`3de119f448175a7c78080fb9251faaa8158c231e219d0cca8127e37c46f326bc`** согласована как ограниченное поручение: точные target methods, состояние, budget, writer routes и адресные тестовые filters заданы; два найденных препятствия reader/input устранены. Новых подтверждённых ошибок постановки в этой области не осталось. Любое дальнейшее изменение source/дозы требует проверки фактического diff и расхода; этот документ не обещает будущий зелёный runtime и не снимает P08 HOLD.

Изменён только этот Markdown. Продуктовых изменений или runtime PASS в настоящей работе нет. После записи прочитаны финальные91/92 и этот review:37 локальных Markdown-ссылок, отсутствующих файлов0. Это проверка разрешения путей, не повторный source audit каждой ссылки. SHA92 остался указанным выше.
