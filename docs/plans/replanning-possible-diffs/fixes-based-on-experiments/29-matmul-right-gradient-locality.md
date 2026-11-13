# 29. Локальность вычисления правого градиента MatMul

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — численная приёмка не доверяет NaN

Общий численный comparator проверяет finite expected/actual/tolerance и ошибки до итогового сравнения. Positive lawful zero/finite gradient проходит; negative NaN/Infinity или одна finite coordinate corruption отклоняется даже при прежнем argmax/norm. Переполнение в вычислении ошибки также invalid, tolerance не расширять после наблюдения.

P110 проверил forward обученной probe projection через experimental adapter, не этот arbitrary backward path. Перенос kernel в существующий ordinary owner требует собственной полной gradient/update parity; равенство 16 классов и wall speedup не заменяют её. Учебный бюджет карточки остаётся своим; чистые array controls не добавляют model calls.

Основание: [адресный план 135](codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски CORE:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-GB исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-GB. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-GB. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-GB

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

ID: **FIX-GB**. Статус: **план переноса GRADB-LOCALITY-01 / KGB**, только документация. Проверенный срез — `7a9fa58d55127ae88e93cd5d71c24a579f730ca3`; завершённые результаты добавлены commit `6f0762405678bd45f8869dd63beb3af526a228e9`. Порядок и следующий ID — только [реестр](01-registry.md). Тема: **документация программ**, профиль C# / объяснение порядка аргументов при вычитании.

## 1. Что успешно измерено

Первичные источники: [A/metrics.json](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/KGB/results/A/metrics.json), [B/metrics.json](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/KGB/results/B/metrics.json), [report](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/KGB/results/report.md), [candidate](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/KGB/results/candidate.md). Старые `A.blocked.json/B.blocked.json` относятся к прежнему отказу до обучения; их не удалять и не смешивать с завершёнными metrics.

| Показатель | A | B | Вывод |
|---|---:|---:|---|
| Время gradB, update 1 | 558,9294 ms | 209,0994 ms | **−62,5893%** |
| Время gradB, update 2 | 570,4808 ms | 224,3966 ms | **−60,6654%** |
| Весь измеренный учебный цикл | 26 999,8673 ms | 18 495,4858 ms | Сопутствующее наблюдение одного A→B |
| CPU цикла | 23 796,875 ms | 17 750 ms | Сопутствующее наблюдение |
| Allocated bytes цикла | 3 765 261 568 | 3 765 294 024 | **Ухудшение +32 456 bytes** |
| Peak working set процесса | 1 552 293 888 bytes | 965 988 352 bytes | Не объём самого gradB |
| TRAIN D-T1 / D-T2, до и после | 33/33 / 33/33 | 33/33 / 33/33 | Полные teacher-forced response sequences сохранены |
| DPO optimizer | 0→2 | 0→2 | Два настоящих обновления в каждом плече |

В каждом update измерены 46 gradB calls и 123 420 672 multiply-add. Суммарная экономия вложенного участка — 695,9142 ms; она не объясняет всё изменение wall цикла на 8,5 секунды. Не приписывать весь wall/peak gain одному фактору. По одному процессу на плечо, статистическая устойчивость не исследована.

Loss по двум шагам `0.3575644 / 0.3298104`, reward margin `1.1646881 / 1.2564831` и финальные параметры совпали A/B. Это результаты шага до применения соответствующего update, не after accuracy. Adapter delta L2 ненулевой в обоих шагах. Конечный file SHA `554ac64bb4ce644d7c6d7102123081a17b967e2d8a6574703332710da3870546`; adapter-value digest `c804e021c00269bba4fc9cc109f86e6f18002afd551915efdf6cf904f0993121`. EXAM и свободная генерация не измерялись.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и профилю; рост accuracy не обязателен.** Здесь успех — время участвующего в обучении gradB при сохранённом приобретённом навыке. Ухудшения показываются рядом. Новый план не требует повторить consumed KGB и не выдаёт исторический процент за обязательный порог CI.

**Исправление интерпретации бюджета:** [bridge:339–345](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/35.ExperimentsV2/Experiments/Rp01Micro/KGB/Common/ContinuousLearning/GradBExperimentBridge.cs#L339-L345) записал `backwards=batches.Count*4`. Native [DpoTrainer:900–905](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/11.DPO/DpoTrainer.cs#L900-L905) делает два backward на batch, и raw timing подтверждает `Backward.CompletedCalls=2` в каждом шаге. Итого **4 backward на плечо**, не 8. Исходный raw сохраняется; будущий receipt использует фактический timing.

## 2. Куда внедрять

Единственный алгоритмический product diff — [00.TensorCore/Core/TensorOps.cs:679–698](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/00.TensorCore/Core/TensorOps.cs#L679-L698), ветвь `b.RequiresGrad` метода `MatMul`.

Изменить порядок циклов `batch → inner → column → row` на `batch → row → inner → column`; перед внутренним `column` один раз прочитать `aValue = a.Data[aOffset + row*aCols + inner]`. Накопление остаётся:

~~~csharp
gradB[bOffset + inner * bCols + column] +=
    aValue * result.Grad.Data[resultOffset + row * bCols + column];
~~~

Для каждой ячейки B последовательность суммирования по batch/row сохраняется. Сохранить `bBatch = batch % bBatchSize`, offsets, shape, `AccumulateGrad`, gradA, forward SIMD и cancellation. Не добавлять parallel reduction, новую формулу, транспонирование/кэш весов или экспериментальную `GradBExperimentTelemetry` в production.

Путь участия: [LoraLinear.Forward:112–125](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/10.LoRA/LoraLinear.cs#L112-L125) → q/v projections в attention → `LlmModel.Forward` → `DpoTrainer.StepVector` → backward → AdamW adapter update. Это вычисление настоящего обучения, не отдельный benchmark умножения.

FIX-LC, FIX-PD и FIX-TS меняют другие участки. Их заранее принятые изменения, если есть, одинаковы в обеих руках и записаны в baseline SHA. Не внедрять их ради FIX-GB, не менять padding, objective, материализацию, reference или LoRA. После изменения общей базы новое resource число не называется точным воспроизведением процента KGB.

### 2.1. Математическое обновление: тот же полный градиент и проверяемые условия

[Новая доказательная вставка исходной карточки на858e984](https://github.com/lubochka/llm_mvp_core/blob/858e98452abdcbca43971bb00f749417ea659991/docs/plans/replanning-possible-diffs/experimets/cards/GRADB-LOCALITY-01.md#book-math-gradb-locality) ссылается на [Proposition9.22 и Theorem9.23, PDF334](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#proposition-9-22) и [продолжение доказательства, PDF335](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#theorem-9-23-continued). Для C=AB, A размера m×k и B размера k×n получаем G_B=AᵀG, где G=dL/dC. Это основание сохранения вычисляемого градиента; ускорение установлено отдельными KGB measurements§1, а не этой формулой.

**Применение к фактическому native broadcast.** Для хранимой B-матрицы q код суммирует `(G_B[q])[r,j] = Σ по b с b % bBatchSize = q Σ по i A[b,i,r]·G[b,i,j]`. В [исходнике TensorOps:679–698 на858e984](https://github.com/lubochka/llm_mvp_core/blob/858e98452abdcbca43971bb00f749417ea659991/00.TensorCore/Core/TensorOps.cs#L679-L698) A и предлагаемый B посещают слагаемые **каждой отдельной ячейки** в прежнем порядке batch/row; меняется лишь чередование работы с разными ячейками. Поэтому сохраняются формула, offsets и порядок float accumulation при той же dtype/runtime/ISA. Алгебраическое равенство над вещественными числами само по себе не разрешает переставлять reduction, включать parallel/FMA-реализацию или ослаблять числовой oracle.

Условия доказательства проверяют **уже назначенные**, а не новые тесты§5: `RightGradient_NonUniformUpstreamMatchesAnalyticalResult` — формулу AᵀG; `BothGradients_PreserveAccumulation` — добавление к прежнему градиенту; `BatchedRightGradient_PreservesBatchMapping` и существующий broadcast finite-difference — offsets/reduction. Основной native learning receipt§4 проверяет сохранённый33/33 и реальные adapter/optimizer updates. Новые fits, counters, thresholds и kernel benchmarks не добавляются.

[Remark9.24](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#remark-9-24) отдельно ограничивает вывод о LoRA: локальная формула/оценка ранга не доказывает успех конкретного дообучения. FIX-GB не вводит low-rank approximation, не меняет LoRA rank и не сокращает число уроков. Формула MatMul не зависит от темы, но полезный эффект измерен именно на C# code-documentation S0; другие домены этим не квалифицированы. Математика подкрепляет существующий приоритет FIX-GB→FIX-NH; предшествующий ID берётся из актуального реестра01, без нового ID и без повторного KGB.

## 3. Один готовый старт и два урока

Будущие малые файлы: `Tests/Fixtures/ExperimentFixes/v1/GradBLocality01/{manifest.json,train.jsonl,historical-A.json,historical-B.json}`. Loader — общий планируемый `Tests/ExperimentFixes/ExperimentFixtureLoader.cs`. Паспорт состояния и хэши — [02 §11](02-learning-state-and-fixtures.md#new-resource-fixtures). Большие bytes читаются read-only из исходного репозитория, не копируются в каждую попытку.

Семья **`_shared-learned-starts/h512-dt-segmented/v1`**, уже использованная FIX-TS, не `h512-dt-joint`, не LC donor. Модель `rp01-csharp-small/v1`: V629/C2048/H512/L4/heads8/KV2/FFN1408; q/v LoRA rank8/alpha16/dropout0; 16 adapter tensors, 53 248 trainable elements. Точные topology/tokenizer/RNG identity проверяются до forward.

Порядок восстановления повторяет [native loader в bridge](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/35.ExperimentsV2/Experiments/Rp01Micro/KGB/Common/ContinuousLearning/GradBExperimentBridge.cs#L581-L611): независимо создать policy/reference через `CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart` из B0; заморозить reference; **только policy** загрузить S0.weights и S0 RNG. DPO optimizer свежий step0. S0.optimizer — исторический SFT1000, в DPO не импортируется. Reference не превращать в S0. Готовый producer1000 повторяется **0 раз**. DPO использует native `ProductionOptimizerSettings` этого среза: learningRate1e-5, weightDecay0.01, beta/epsilon из штатного AdamW; сохранить полный settings record и требовать его равенства A/B. KL beta0.1, native teaching vector/weight из одного и того же Materializer, без ручного override. Менять defaults ради совпадения числа запрещено.

Префикс prompt — точная строка с завершающим пробелом: `Explain in one short English sentence what the method returns. Describe argument order. Code: `. После неё без разделителя — Code из таблицы.

| ID | Code | Chosen | Rejected |
|---|---|---|---|
| D-T1 | `int Delta(int first,int second){return first-second;}` | `Returns the first argument minus the second argument.` | `Returns the second argument minus the first argument.` |
| D-T2 | `int Delta(int first,int second){return second-first;}` | `Returns the second argument minus the first argument.` | `Returns the first argument minus the second argument.` |

Материализация — native exact path из [bridge Materialize](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/35.ExperimentsV2/Experiments/Rp01Micro/KGB/Common/ContinuousLearning/GradBExperimentBridge.cs), metadata `dataset_version=n62-csharp-subtraction-v1`; не подставлять metadata TS01 только из-за общего S0. Длины prompt91/chosen124/rejected124/padded124; 33 response targets. Digests: D-T1 `7b96136c03f8d04797f3c9b6e93775468fb09b1d4065657eb535aac4d19199a3`, D-T2 `c82e8b8eccdaf4081efff76c502710475ebf05644ee45a34e58e2705469929d6`. Разница digest/length блокирует шаг до обучения, а не «лечится» изменением expected.

## 4. Постоянный native обучающий тест

Новые файлы, **пока не реализованы**:

- `Tests/ExperimentFixes/GradBLocalityLearningTests.cs`, один Fact `RestoredDpoPair_PreservesLearningState`;
- `Tests/ExperimentFixes/GradBLocalityContractTests.cs`;
- общий двухэтапный runner из [03 §9](03-test-and-execution-contract.md#native-ab-transfer).

Оба плеча используют обычные native ProjectReference в одном canonical checkout. Сначала исходный kernel A создаёт законченный learning receipt; затем выбранный product diff и Rebuild дают B из изменённого `TensorCore.Core.TensorOps`, не KGB assembly. В обеих руках один и тот же test source и один runtime/ISA/config. Старые A receipts остаются read-only, отдельная рабочая копия или production feature flag не создаются.

В native Tests уже есть подходящий test-scoped доступ: [DpoStepVectorTimingTests:212–220,241–245](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/Tests/DpoStepVectorTimingTests.cs#L212-L245). Использовать существующий private TestLease pattern с `DpoMutationAdmissionCapability.Issue`, `DpoProjectLoraRecipient.Capture`, `DpoExecutionProfile.Capture` и `CreateForProjectLora`; не добавлять friend assembly/public bypass или копировать experiment authority issuer. Это native core learning test, **не** доказательство production admission, publication или закрытие governance/E2E требований FIX-TS.

CheckpointDir тестового trainer — `<stageOutput>/state/dpo`, уникальный для руки; StepVector сам пишет policy/metrics. Для чтения использовать `DpoTrainer.BuildPolicyCheckpointFileName(step)` и `BuildMetricsFileName(step)`, не придумывать имена для native codecs. Отдельные optimizer/RNG snapshots test harness сохраняет после измерения своим test-state serializer, не выдаёт их за canonical product publication. Existing native publication внутри StepVector сохраняется.

Один Fact на плечо:

1. Проверить bytes/identity/loaded native assemblies до первого model call; восстановить S0/B0 и fresh DPO0.
2. Два teacher-forced readout TRAIN; ожидаются 33/33 и exact=true на каждой строке. Иной результат — несовместимый старт, не новый producer.
3. `StepVector` с одним batch D-T1, затем одним D-T2. Сохранить реальные `DpoStepVectorTiming`, loss/margin, optimizer step и adapter deltas после каждого шага.
4. Ещё два TRAIN readout. Сохранить полный weights и keyed optimizer/RNG state в test artifacts; источники read-only. Параметры reference/base не меняются, adapter delta ненулевой, optimizer2.
5. Внешнее сравнение двух receipts: точное совпадение carriers, readouts, loss/margin, именованных float values и optimizer tensors/step в том же runtime. Не сравнивать JSON с timestamp/path как математическое состояние. Незначительное численное расхождение не устранять ослаблением oracle: выяснить изменившийся порядок накопления/сборку.

Измерять wall/CPU/allocated вокруг двух настоящих `StepVector`, исключая внешний restore/readout/hash/export и контрактные тесты; штатная policy/metrics publication внутри StepVector остаётся частью измерения. Внутри шага не добавлять observer с дополнительным model forward. Сохранять scope измерения явно: он уже исторического цикла с диагностическими снимками, поэтому абсолютные bytes/ms между старым host и native test несопоставимы напрямую. Исторический участок −62% не измеряется этим новым whole-step timing и не становится CI порогом. CI проверяет обучение/parity; будущий receipt честно показывает все resource deltas, включая отсутствие выигрыша.

**Бюджет основного A+B:** 4 DPO updates, 16 training forwards (8 reference + 8 policy), 8 backward, 8 diagnostic TRAIN forwards, 4 optimizer applications; 0 SFT/producer/EXAM/Generate. На плечо — половина. Один receipt на плечо используется всеми assertions; отдельный тест не повторяет обучение ради другого показателя.

## 5. Существующие тесты, новый контракт и ремонт падений

Свежие падения не измерялись. Сначала A baseline по точным methods ниже, затем те же methods в B; политика [03 §2](03-test-and-execution-contract.md) обязательна. Namespace везде `LlmFromScratch.Tests`.

| Существующий method | Смысл и действие при падении |
|---|---|
| `TensorCoreTests.MatMul_2x2_MatchesPyTorchReference` | Forward не меняется; прежний expected сохранить |
| `TensorCoreTests.MatMul_IncompatibleDimensions_Throws` | Проверки shape сохранить |
| `TensorCoreTests.Backward_MatMul_GradMatchesPyTorchReference` | Сохраняет gradA; в текущем тесте B не trainable, поэтому он один **не покрывает gradB** |
| `LoraTests.TensorOps_MatMulBroadcastBackwardReducesAllLeadingMatrices` | Сохранить finite-difference oracle для broadcast/reduction |
| `LoraTests.LoraAttention_AllProjectionBGradientsMatchFiniteDifferenceThroughFinalNorm` | Сохранить настоящий q/v/attention/final-norm gradient path |

В новом contract class назначить ровно три Fact:

| Новый method | Полностью заданный вход и независимый expected |
|---|---|
| `RightGradient_NonUniformUpstreamMatchesAnalyticalResult` | A=[[1,2,3],[4,5,6]], RequiresGrad=false; B=[[2,-1],[0,3],[-2,4]], RequiresGrad=true; upstream G=[[1,2],[3,4]]. Expected gradB=[[13,18],[17,24],[21,30]] |
| `BothGradients_PreserveAccumulation` | Те же A/B/G, обе стороны RequiresGrad=true. После одного fresh output backward gradA=[[0,6,6],[2,12,10]], gradB как выше. Второй fresh MatMul output с теми же leaves и G, без ZeroGrad: оба gradients ровно удваиваются |
| `BatchedRightGradient_PreservesBatchMapping` | A[2,2,3]: batches [[1,2,3],[4,5,6]] / [[7,8,9],[10,11,12]]; B[2,3,2]: [[2,-1],[0,3],[-2,4]] / [[1,2],[-1,0],[3,-2]]; G[2,2,2]: [[1,2],[3,4]] / [[-1,1],[2,-2]]. Обе стороны trainable. Expected gradB: прежняя первая матрица / [[13,-13],[14,-14],[15,-15]]; gradA: прежняя первая / [[1,1,-5],[-2,-2,10]] |

Existing shared-B broadcast finite-difference test сохраняется. Новые три contract Fact — точная целочисленная арифметика, 0 optimizer updates; они не обучают модель и не заменяют основной learning receipt. Oracle задан независимо от нового порядка циклов.


Если B падает на прежнем независимом expected — чинить indexing/accumulation в назначенной ветви MatMul. Не менять tolerances, accuracy gate, данные, ключи optimizer или expected под actual. Если A тоже падает — сохранить baseline diagnostic и его принадлежность native path; не чинить весь solution. Общая единственная разрешённая предпосылка сборки — [02 §10](02-learning-state-and-fixtures.md#native-build-prerequisite).

## 6. Две стадии будущего переноса и связь с E2E

Единственная рабочая поверхность — `C:/Projects/llm_mvp_core`, ветка `codex/dpo-implementation`; вывод — `artifacts/fbe/FIX-GB/attempt-0001/` внутри неё. [Общий контракт 03§9](03-test-and-execution-contract.md#native-ab-transfer) задаёт две последовательные стадии. Сначала назначенный исполнитель создаёт и фиксирует common test/helper/fixture harness на неизменённом native алгоритме A, записывает его SHA как `$baselineCommit` и выполняет только Baseline. Worktree, sibling/copy, checkout/reset и переключение ветки запрещены; runner не материализует другой checkout.

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-GB -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

Baseline выполняет существующие tests → назначенные contract tests → одну learning руку A и сохраняет её terminal receipts **до** изменения кода. Затем только человек или назначенный исполнитель в том же корне вносит выбранный gradB diff, проверяет его и фиксирует candidate commit; `$candidateCommit` — полученный SHA. Test/helper/fixture bytes остаются прежними. После этого выполняется вторая стадия:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-GB -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Candidate выполняет существующие tests → те же contract tests → одну learning руку B, затем читает готовые A/B receipts; повтор A или откат к baseline не допускается. В каждой стадии перед `--no-build` выполняется `Rebuild` назначенного Tests проекта по03§9. Loaded native assembly Location/SHA-256/MVID и source SHA сохраняются отдельно для A/B: общий каталог не позволяет принять старую DLL за новый код. При отсутствии завершённых A receipts Candidate останавливается без обучения, без автоматического retry/cleanup или нового output.

Это **планируемый** runner, не существующая команда для немедленного запуска. Значения SHA выдаёт назначенный подготовительный этап03, не выбираются поиском. Он запускает только перечисленные methods и новые classes, проверяет discovery и сохраняет baseline/after/learning receipts. Число процессов, тестов и учебный бюджет прежние. Ограничение одного learning process —15 минут, не более двух; отсутствие terminal не разрешает повтор. Ожидание —03 §5, тот же handle в том же ходе.

[FIX-E2E §9](20-prompt-domain-e2e.md#resource-transfer-e2e) сохраняет уже выбранный learned selector, три темы и запросы. Матрица весов и это code-documentation состояние не являются checkpoint маршрутизатора. Положительный neural тест выполняется здесь один раз; E2E получает только assertions изоляции существующих recipients, 0 дополнительных training updates. Скорость gradB не объявляется улучшением выбора домена.

## 7. Промпт исполнителю и закрытие

~~~text
Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/29-matmul-right-gradient-locality.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.

Выполни только назначенный FIX-GB; текущий документ сам не разрешает перенос.
Модель: текущая default без override. Отчёт: русский.
Прочитай AGENTS.md/CLAUDE.md; docs/skills/architect-behavior-classifier-SKILL.md,
phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md,
test-integrity-SKILL.md, prompt-patch-authoring-SKILL.md,
human-communication-GLOSSARY.md, bug-to-tests-SKILL.md; общие 02/03 и эту карточку.
Меняй только gradB traversal TensorOps.MatMul; test/helper/fixture адреса заданы выше.
WRONG: повторить KGB/producer1000, импортировать SFT moments в DPO, reference=S0,
назвать 8 backwards правильным budget, перенести telemetry или ослабить oracle.
CORRECT: native A/B из одного harness, policy=S0/reference=B0, fresh DPO0,
2 updates/4 backwards на плечо, сохранённый TRAIN и все resource regressions.
В canonical C:/Projects/llm_mvp_core сначала Stage Baseline и сохранённые A receipts,
затем назначенный product diff/review/commit, затем Stage Candidate без повторного A.
Перед каждой рукой Rebuild и native DLL SHA/MVID; no worktree/checkout/reset/branch switch.
Любое полезное измеренное улучшение — успех по метрике и профилю; рост accuracy не обязателен.
0→0 не подтверждает обучение. Отсутствующий donor — точный blocked input, не новый поиск/fit.
Один запуск — один handle, штатный wait в этом ходе до terminal/deadline; не polling loop/повтор.
STOP/NEXT немедленно: нельзя дописывать старый отчёт, тест, commit или cleanup.
NEXT берётся по точному ID из01; все прежние результаты сохранить, ничего не удалять.
~~~

Будущий русский `report.md` — в указанной attempt-папке, по шаблону03: source/assembly/fixture identities, restore и фактический бюджет; A/B before→after; дельты времени, CPU, памяти и качества; независимые controls; точный diff и оставшиеся ограничения. `report-for-user.md` кратко отделяет исторический успех gradB от нового результата переноса. Документный кандидат не считается production PASS до назначенной реализации и её фактической проверки.


