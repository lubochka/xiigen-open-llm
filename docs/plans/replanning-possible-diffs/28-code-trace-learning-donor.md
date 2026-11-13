# Code-trace после backward-fix: что сохранено об обучении

Промежуточный исследовательский пакет, 2026-10-16. Фаза — READ_ONLY_RESEARCH: прочитаны сохранённые данные и исходники; новые обучение, inference, тесты и сборки не запускались. Этот файл не завершает общий поиск полезных прежних механизмов.

Исследуются ровно два исторических каталога внутри `43.LearningLocalWikiData/artifacts/runner-metrics/`:

- **A:** `2026-05-15-code-trace-batch-001-after-backward-fix`, DPO run `code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515135722`.
- **B:** `2026-05-15-code-trace-batch-001-50-optimizer-steps`, DPO run `code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515143246`.

Из нынешнего дерева прочитаны сохранённые JSON, а не выполнены повторные оценки. HEAD при чтении — `259c61769c3c66c9353f70d3dd569da19682067b`. Сопоставление с историческим source snapshot и получателями продолжается ниже; дата в имени run сама по себе не аттестует использованную runtime-сборку.

## КТ-01. Положительный предметный отчёт и neural preference — разные измерения

Оба `runner-reports/code-trace-builder-flow-batch-001-code-trace-builder-unit-section22.json` сохраняют `weighted_accuracy: 1` и `CODE_TRACE_BUILDER_GATE_PASSED`, одновременно `completion_claim_allowed: false`, `exact_evidence_accuracy: UNKNOWN`, `hallucination_rate: UNKNOWN`. Neural поля этого же отчёта существенно уже общего verdict:

| Сохранённое измерение | A | B |
|---|---:|---:|
| Учебных / контрольных строк DPO | 50 / 50 | 50 / 50 |
| Элементов `batchSummaries` | 20 | 50 |
| `finalAccuracy` | 0,62 | 0,6600000113248825 |
| `heldOutAccuracy` | 0,6200000047683716 | 0,5600000023841858 |
| `heldOutRewardMargin` | 0,0019463715143501759 | 0,0034502565395087004 |

Источник чисел — соответствующие `dpo-runs/code-trace-builder-unit/<run>/project-wiki-dpo-metric-summary.json`. Это положительные сохранённые margin и предметная метрика, но ещё не причинная before/after пара. Не следует ни приписывать 100% свободной neural generation, ни объявлять полезность предметного runner опровергнутой. Точные определения, состав примеров и mutable state проверяются отдельно.

## КТ-02. Что удалось установить о получателе изменения весов

Продолжение исследования выполнил `/root/rp01_code_trace_astra` (Astra ultra). В этой части прочитаны сохранённые bytes обоих `checkpoints/policy_final.bin`; модель для проверки не создавалась и forward не вызывался. Формат разобран как число тензоров, затем rank, dimensions и float32 little-endian — согласно историческому `03.TransformerModel/LlmModel.cs:70–106` в commit `89dc7a6adc2d6faebc8fd4476bb9409a61aea624`.

У обоих файлов 12 тензоров и 1 188 744 bytes; разбор закончился точно на конце файла. Сравнение даёт:

| Область checkpoint | Между сохранёнными A и B |
|---|---|
| Тензоры 0–10, всего 166 080 float | Побайтово одинаковы |
| Тензор 11, форма `[2048,64]`, 131 072 float | Отличаются 131 071 значений; максимальная абсолютная разность `0.03309109807014465` |
| Полный SHA256 A | `8cabd7bceeccdb934f2e946b971745e71a426a060411b1498aeb6d832a199628` |
| Полный SHA256 B | `6669ddc75d8416508b7478f782fe8b9e7687ad873aee46c45f75a29db7f9afab` |

Исторический порядок регистрации в `LlmModel.cs:27–38` — embedding, block, final norm, `lm_head`; `00.TensorCore/Layers/Module.cs:22–29` обходит зарегистрированные параметры и дочерние модули. Поэтому последний тензор соответствует **output head**. В прочитанной модели нет установленных LoRA: runner создаёт обычный `LlmModel` с vocabulary 2048, hidden 64, одним слоем, четырьмя Q-heads и одним KV-head (`ProjectWikiDpoMetricRunner.cs:85–97`). Поля LoRA rank 2 и dimensions 4096/96/2 layers в опубликованном manifest относятся к другому, JSON-артефакту; они не описывают найденный neural binary.

Сохранённое различие head — положительное свидетельство того, что нейронные результаты здесь не сводятся к одинаковому файлу с новым именем. Оно согласуется с реально изменяемым выходным преобразованием. Но **A→B не является установленным продолжением одного обучения**: в обоих каталогах среди binary checkpoints найден только `policy_final.bin`; нет исходного policy, `reference_base.bin` или before-checkpoint. Начальные head и reference этих конкретных запусков восстановить из прочитанных артефактов нельзя. Не утверждаем, что все 131 071 различий возникли исключительно от дополнительных optimizer steps, или что A был начальным checkpoint B.

Исторический код объясняет возможный получатель: `DpoTrainer.cs:99–110` вызывает backward и AdamW по параметрам policy; `RmsNorm.cs:37–60` возвращает новые Tensor из готовых данных без связи backward; `Tensor.cs:34–41` по умолчанию задаёт `requiresGrad=false`; `AdamW.cs:42–44` пропускает параметр без gradient. `lm_head` после final norm остаётся обучаемым. Это source-supported объяснение одинакового backbone и различающегося head, **не аттестация фактически запущенной сборки**. Обширное разбирательство ProjectWiki query-planning из пакета 25 здесь не повторяется.

Для RP01 это точный кандидат различия: в [пакете 12](12-student-update-to-generation-path.md), С-01/С-03, получатели — q/v-LoRA, а 39 base-тензоров, включая `lm_head`, остаются неизменными. Переносить из этого сравнения следует гипотезу о роли обучаемого выходного преобразования, а не старые численные head-веса: их vocabulary/представление не совпадают с текущим BPE. Причина плохой генерации RP01 этим сравнением не доказана.

## КТ-03. Это не эксперимент «те же данные, больше обучения»

Оба `dpo-pairs.jsonl` разобраны целиком как 100 сохранённых записей: строки 1–50 — train, 51–100 — heldout по ID. Во всех 100 позициях между A и B совпадают `id`, `prompt`, `chosen`. В каждой половине **изменены 30 rejected из 50**: по 10 типов `wrong_caller`, `invented_callee`, `incomplete_trace`. Первые 20 пар каждой половины (`missing_callee`, `missing_value_flow`) совпадают полностью.

| Raw bytes `dpo-pairs.jsonl` | SHA256 |
|---|---|
| A | `e682d791a8d823c69683e0105d0ed4b9049db65b67d6c6f6b83a0f1e3f78f11a` |
| B | `a550412b4ae3ca69898a3b9297856caaba0e05551b943e33f25fa050269307dd` |

Оба manifest при этом имеют одинаковый `sourceDatasetSha256=1BC58B776651A6DD941377825DB78A442287E03B6898971D6DDF2BEAB61DA9F8`. Источник объясняет почему: `CodeTraceBuilderManifestPublisher.cs:130–134` хеширует сериализованные **result.Cases**, а не raw training/heldout тексты. Равный manifest hash не доказывает равные учебные данные. Аналогично `trainingStepCount=50` в обоих manifest берётся из `TrainingRowCount` (`:103`), а не из числа optimizer updates; точный счёт шагов здесь следует читать по DPO summaries.

Содержание изменения предметно полезно. Например, train pair-031 в A добавляет `Invented.Service->Invented.Repository`, тогда как B сохраняет реальную трассу и добавляет правдоподобный вызов `CampaignController3.SaveCampaign->AuditTrailService.Record` и ложный `auditTicket->returned`. Pair-041 в A оставляет один метод; B сохраняет методы и локальный переход к Normalize, но теряет Persist/Repository. Такие rejected лучше локализуют **неверную зависимость** и **обрыв пути значения**, хотя сами по себе ещё не доказывают приобретение этого навыка моделью. В обеих версиях rejected также содержит явный `[WRONG: ...]`; маркер и его объяснение входят в neural answer, создавая альтернативный поверхностный признак.

Из `batchSummaries` отдельно пересчитаны веса агрегатов. A содержит 20 записей, повторяющих размеры 16/16/16/2; B — 50 записей по 5. В обоих случаях сумма `pairCount` = **250 предъявлений**: по пять проходов через 50 сохранённых train-пар. Это не 20 против 50 эпох и не 50 новых уникальных примеров за каждый шаг. Различаются размер minibatch, число updates и 60% rejected, а исходная идентичность policy/reference не сохранена.

Поэтому `heldOutAccuracy 0.62→0.56` и `heldOutRewardMargin 0.001946→0.003450` нельзя использовать как чистое улучшение либо ухудшение из-за числа шагов. Для неизменившихся 20 heldout в B есть итог 12/20: 5/10 `missing_callee` и 7/10 `missing_value_flow`. В A отсутствует `dpo-pair-diagnostics.jsonl`, поэтому сопоставимый A-result именно на этих 20 строках из архива не установлен. Новое оценивание не проводилось.

## КТ-04. Что означают accuracy и положительный margin

В историческом `11.DPO/DpoTrainer.cs:13,135–165,226–260` margin вычисляется как `0.1 × ((policyChosen − referenceChosen) − (policyRejected − referenceRejected))`; каждая log-probability усреднена по masked answer-позициям. Accuracy — доля пар с margin строго больше нуля. Это направление **относительного предпочтения к reference**, а не доля свободно сгенерированных верных трасс. Положительный margin не требует даже абсолютного `policyChosen > policyRejected`.

Название `finalAccuracy` также требует осторожности. Пересчёт сохранённых `batchSummaries` точно воспроизводит `finalLoss`, `finalAccuracy`, `finalRewardMargin` как средние по всем историческим шагам с весом `pairCount`. Это агрегат траектории, а не отдельный итоговый экзамен на всех train-строках. В B дополнительно сохранены per-pair diagnostics после обучения: train accuracy 33/50, mean margin `0.005150831276550889`; heldout accuracy 28/50, mean margin `0.0034502564650028943`. Незначительное различие последнего числа со summary соответствует разным порядкам float-усреднения, а не новому прогону.

Сохранённые окна повторного предъявления одних train-пар дают важное контрдоказательство идее сильного роста:

| Run и сохранённое окно | Train accuracy | Mean margin | Mean loss |
|---|---:|---:|---:|
| A, первое окно 50 предъявлений | 0.62 | 0.003608470634 | 0.691382565498 |
| A, пятое окно 50 предъявлений | 0.62 | 0.003586877771 | 0.691393721104 |
| B, первое окно 50 предъявлений | ≈0.66 | 0.005459753086 | 0.690441530943 |
| B, пятое окно 50 предъявлений | ≈0.66 | 0.005187852774 | 0.690577536821 |

Все пять окон внутри каждого run сохраняют ту же accuracy, а средний margin монотонно слегка снижается. Это арифметика уже сохранённых step-отчётов, не пять новых экспериментов. Такая траектория не подтверждает полезное улучшение neural preference от дальнейшего обучения даже на train. Она не отменяет наличие конечных положительных margins и различающегося head; она ограничивает причинный вывод.

B позволяет увидеть предметные различия:

| Ошибка в heldout B | Положительных margins / 10 | Mean margin |
|---|---:|---:|
| Пропущен callee | 5 | −0.000479221344 |
| Пропущен путь значения | 7 | 0.006890535448 |
| Неверный caller | 6 | 0.007342748530 |
| Выдуман callee | 6 | 0.005811748840 |
| Неполная трасса | 4 | −0.002314529149 |

Данные показывают, где относительное предпочтение уже благоприятно и где нет. Они не содержат независимого pre-training результата, поэтому 7/10 нельзя назвать улучшением с меньшего исходного значения. Отдельно в первом сохранённом step A margin уже `0.00405192375`, B — `0.01208400633`; следовательно нельзя молча подставить нулевой стартовый margin на основании более позднего исходника, копирующего policy в reference.

## КТ-05. Предметный ответ действительно строится, но другим механизмом

В прочитанной цепи источник `weighted_accuracy=1` — минимальное из трёх recall в `ProjectWikiTrainingOrchestrator.cs:172–201` исторического snapshot. `CodeTraceBuilderTrainingRunner.cs` в нынешнем дереве, строки 49–72 и 124–130, для каждой heldout-строки вызывает `_builder.Analyze(sourceText)` и сравнивает множества ожидаемых методов, call-edges и value-flows. Непустой expected оценивается по recall; лишние actual-элементы этой формулой отдельно не штрафуются. `wrongAnnotationRate=1` означает наличие `[WRONG:` в данных, а не успешное распознавание ошибки.

`CodeTraceBuilderUnit.cs:30–43,46–124` извлекает методы, ищет имена вызовов, аргументы и использование результата через regex/правила. Его output — конкретные символы и переходы, пригодные как источник фактов для документации. `confidence=0.94` назначается правилом при наличии методов и call-edges (`:34`), а не калибруется по нейронному ответу. В `TrainAndEvaluateAsync` нет обновления этого extractor по trainingRows: train нужны для проверки размера и контрактов пар. Такой детерминированный инструмент может быть полезен независимо от DPO; наличие обучения в другом месте pipeline не делает его output neural generation.

Есть предел независимости gold: исторический `CodeTraceBuilderDpoGenerator.cs:22–36` вызывает тот же `_builder.Analyze` и кладёт его Methods/CallEdges/ValueFlows в ожидаемые поля. Потом runner сравнивает extractor с производными того же extractor. В архиве сохранён успех на 50 шаблонных heldout-сниппетах с другими доменными именами; независимая ручная или compiler-backed правильность всех expected здесь не установлена. Это полезная проверка согласованности трассы, но недостаточная оценка полноты понимания произвольного C#.

Связка артефактов прослежена непосредственно:

1. Исторический `CodeTraceBuilderTrainingPipeline.cs:46–84` сначала получает детерминированный `run`, затем отдельно выполняет DPO metrics и публикует **`run.Value`**. Параметр neural model/checkpoint в publisher не передаётся.
2. `CodeTraceBuilderManifestPublisher.cs:59–70` нынешнего дерева сериализует training result в `policy.bin` как JSON `kind=code_trace_builder_policy_state`, а в `tokenizer.json` пишет четыре имени полей. Содержимое обоих архивных `policy.bin` и `tokenizer.json` прочитано и соответствует этому формату.
3. Нейронный `policy_final.bin` лежит в `dpo-runs/.../checkpoints/`; manifest указывает на **другой** `code-trace-builder-unit/.../policy.bin`. Наличие manifest, `Trusted` и успешной регистрации не доказывает загрузку neural state в генератор ответа.

В рассмотренных непосредственных callers собственную предметную трассу производит `Analyze`, а neural model используется для DPO scoring. Потребитель, который загружает именно эти neural binaries и выдаёт из них связный свободный code-trace answer, в этой ограниченной цепи не найден; сохранённых свободных outputs также нет. Это не утверждение об отсутствии любого такого пути во всём репозитории. Для RP01 правильный перенос здесь — проверить явное подключение структурной трассы к финальному ответу, если такой composite serving выбран; это самостоятельная гипотеза, а не доказанный ремонт LoRA-generation.

## КТ-06. Представление токенов не переносится как готовая языковая способность

В историческом `ProjectWikiDpoMetricRunner.cs:300–338,356–387` текст дробится на слова, приводится к нижнему регистру, пунктуация вне `_`/`-` теряется; слова хешируются в 2047 ненулевых ID. Prompt ограничен четвертью context 192, для длинного answer применяется head/tail отбор (`:341–353`). Это полезное компактное представление для сравнительного score, но не обратимый tokenizer исходного кода или текста. Даже без доказательства конкретной hash-коллизии потеря регистра и пунктуации уже не позволяет восстановить исходную строку однозначно.

Архивный `tokenizer.json` содержит лишь `tokenizer_kind=code-trace-source-fields` и поля `source_text`, `methods`, `call_edges`, `value_flows`; это не таблица vocabulary и не decoder от neural ID к словам. В прочитанной паре artifacts нет собственного text-decoding пути. Проверки обратимости запуском tokenizer не выполнялись: вывод основан на исходнике и содержимом сохранённого JSON.

Точная версия этого tokenizer в A/B остаётся неаттестованной. Сохранённые B diagnostics называют `ChosenTokenCount=17` и `RejectedTokenCount=24` у pair-001; в историческом `BuildPairDiagnostic` эти поля считаются через `ToHashSet`, то есть отражают **уникальные слова**, а не полный causal token sequence. Не следует сравнивать их напрямую с RP01 BPE context length. Нынешние exact BPE/response-target свойства RP01 уже разобраны в пакетах [12](12-student-update-to-generation-path.md) и [14](14-data-quality-and-evaluation-alignment.md); старый lossy hashing не является улучшением, которое нужно механически возвращать.

## КТ-07. Что можно взять из этих запусков для дальнейшего исследования

**Конкретный полезный материал есть:** source-grounded представление методов, caller/callee и пути значения; отрицательные примеры по пяти разным смысловым ошибкам; отдельная диагностика слабых типов; сохранённый отличающийся output head. Это кандидаты для обучения и проверки объяснения C#, особенно когда «правильно» означает показать, откуда пришло значение и куда ушло, а не только воспроизвести общий префикс ответа.

**Полезное приобретение свободной нейронной генерации здесь не установлено.** До/после одного checkpoint не сохранено, 60% контрольных rejected изменены, within-run margins слегка ухудшаются, предметный output строит extractor, reversible decoding отсутствует в прочитанном пути. Поэтому два run нельзя включать в перечень доказанных прежних улучшений free generation. Также нельзя из этих ограничений вывести, что структурный extractor или весь прежний проект был бесполезен.

Минимальный будущий причинный дизайн — **только предложение, не запущен и не разрешён этим документом**:

- После отдельного прямого разрешения пользователя более дешёвый исполнитель берёт один зафиксированный initial checkpoint/reference/tokenizer и неизменные 20 train-пар плюс 10 независимых heldout: по 4/2 примера каждого из пяти типов ошибок. Предлагаемый потолок — 10 updates на обучаемую ветвь, один seed; это диагностический малый дизайн, не доказательство статистической устойчивости. Сохраняются исходные веса и один и тот же heldout; никаких замен rejected между сравниваемыми состояниями.
- Первая проверка изолирует recipient: no-update control, разрешённая q/v-LoRA и отдельно разрешённый output-head recipient при одинаковых данных и фиксированном бюджете. Старый head не переносится через несовместимый vocabulary; сравнивается принцип обучения выходного слоя на текущем представлении. Расширение recipient требует явного разрешения как отдельный эксперимент.
- Измеряются named tensor deltas, before/after preference на тех же парах и свободный ответ на новых небольших исходниках. Для последних независимо проверяются реальные methods/calls/value-flows, включая лишние и выдуманные связи. `[WRONG:]` не должен быть единственным различием между правильным и ошибочным ответом; independent gold не создаётся тем же оцениваемым extractor.
- Проверка полезности deterministic trace как входа final answer проводится отдельным последующим сравнением с фиксированными нейронными весами. Не смешивать её с recipient-изменением: тогда станет видно, пришёл ли выигрыш от обученных весов или от дополнительной фактической трассы.

Это дизайн для различения причин, а не готовый executor, экспериментальный результат или требование немедленно менять продукт. Предложенные число updates, command и compute budget должны быть закреплены в будущем разрешённом work order до запуска.

## КТ-08. Источники, границы покрытия и следующий открытый источник

Первичные записи A: [summary](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15-code-trace-batch-001-after-backward-fix/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515135722/project-wiki-dpo-metric-summary.json), [100 raw pairs](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15-code-trace-batch-001-after-backward-fix/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515135722/dpo-pairs.jsonl), [binary](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15-code-trace-batch-001-after-backward-fix/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515135722/checkpoints/policy_final.bin).

Первичные записи B: [summary](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15-code-trace-batch-001-50-optimizer-steps/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515143246/project-wiki-dpo-metric-summary.json), [100 raw pairs](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15-code-trace-batch-001-50-optimizer-steps/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515143246/dpo-pairs.jsonl), [100 pair diagnostics](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15-code-trace-batch-001-50-optimizer-steps/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515143246/dpo-pair-diagnostics.jsonl), [binary](../../../43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15-code-trace-batch-001-50-optimizer-steps/dpo-runs/code-trace-builder-unit/code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515143246/checkpoints/policy_final.bin).

Точно прочитаны JSON metrics, pair contents, B diagnostics, оба полных neural binaries, оба manifest/tokenizer, оба section22 reports; у JSON `policy.bin` просмотрено содержимое с форматом и case-полями. Все 20/50 step summaries использованы арифметически; отдельные `dpo_step_*.json` не перечитаны целиком поштучно. Полные model/trainer/optimizer/normalization классы не заявляются прочитанными там, где извлекались адресные окна. В частности, для metric runner прочитаны initialization, update/evaluation, diagnostics, tokenizer и serialization-result участки; для DPO — update, score и backward. Это один исследовательский пакет, не 70 новых запусков, циклов или независимых ревью.

Артефакты внесены в git commit `dd22eb10bb4148e3124ffcbb97cf11a64dd3d5bc` от 2026-05-24; это установлено по `git log -- <два каталога>`. Исторический source snapshot `89dc7a6adc2d6faebc8fd4476bb9409a61aea624` от 2026-05-20 прочитан через `git show`. Он уже содержит варианты, которых нет в двух run: сохранение reference checkpoint и дополнительные поля summary; его generator также отличается от сохранённых A/B rejected. Следовательно **source-import SHA не является runtime-binary attestation**. Точная сборка, полный начальный neural state и исторический decode consumer остаются пробелами. Ни git checkout, ни build для восстановления не выполнялись.

При завершающем чтении repo HEAD — `d5c2409b24292d361593b8262b0c54997b15f539`; он изменился извне относительно HEAD, указанного в начале пакета. Исторические source-выводы выше привязаны к явному SHA, а bytes A/B — к указанным hashes. Продолжение изменило только этот Markdown; product code, training data, weights и runtime state не менялись. Общий индекс/учебный лог ведёт parent.

**Skill-load boundary:** найдены и адресно прочитаны `AGENTS.md`, `CLAUDE.md`, `architect-behavior-classifier`, `planning-session-startup`, `plan-review`, `generated-code-review`, `prompt-patch-authoring`, `output-readability-gate`, `human-readable-architecture-audit` под `docs/skills`; это targeted excerpts по scope, evidence, recipient, causal isolation и русскому объяснению, не полная загрузка многостраничных файлов. Прочитан `RTK.md`; применён Context Mode для анализа сохранённых файлов. Один большой Context Mode вызов с indexing intent был остановлен после отсутствия ответа; дальнейшие короткие `ctx_execute` без indexing завершались. Ошибки чтения отсутствующего A diagnostics и неверно предположенных source-путей исправлены на действительные пути; runtime экспериментов не было.

**Следующий точный источник для parent:** `43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-19-code-trace-builder-baseline-before-remediation/`. При этом продолжении подтверждено только имя каталога, его содержимое не прочитано. Предметный вопрос следующего bounded поиска — есть ли там сохранённые initial/reference и сопоставимый heldout, позволяющие отделить полезный learned change от изменения rejected. Название `baseline` само по себе ничего из этого не доказывает. Пакет 28 завершает только разбор двух порученных run; общее исследование полезных прежних механизмов остаётся открытым.

