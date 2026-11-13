# 30. Убрать поэлементные массивы координат из MultiplyBroadcast

<!-- transfer-entry-reload -->
**Вход исполнителя.** [Q38](01-registry.md#work-fix-rms). Для общего включения действует измеренное ограничение этой строки. Возврат к работе — по записанному условию для её домена. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

## Уточнение допуска по показателям — 3 ноября 2026

**FIX-RMS: Приостановить общий перенос.** Вошёл по памяти, но скорость обучения ухудшилась >5%; причина тайминга пока не доказана.

- Точность: Учебных и экзаменационных по 1/2 → 1/2; веса A/B совпали.
- Полезный результат: Выделения 13 642 856 → 12 352 616 Б (−9,46%).
- Цена и ограничения: Время 78,6 → 91,5 мс (+16,41% по округлённым данным; raw +16,46%); пик +0,42%, CPU +20%.
- [Полное сравнение и статус кода](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#benefit-fix-rms).

Этот вывод относится к названным ID, а не ко всем механизмам документа. Старый текст плана и опыта сохранён как доказательство, но его общий статус «к переносу / разрешён» не переопределяет это уточнение. Ближайшее действие — разбор уже сохранённых данных и конкретного регресса; новое обучение, повтор PRIMARY и переключение product default этим дополнением не назначаются. Для отдельного логического ремонта назвать нарушенный контракт и сохранить отрицательные контроли, а не переносить весь эксперимент под словом «bugfix».


<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски CORE:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения.

**Ожидания от теста.** Используйте данные этого ID и обычный путь, указанный в его действующей строке. Исходные доказательства сохраняются. Дополнительную диагностику из прежнего плана открывайте при конкретном расхождении. Для учебного результата запишите, что было до урока и что стало после. Техническую проверку формата или маршрута так и назовите.

**Проверка исполнителя.** Соберите затронутый проект и выполните назначенный малый сценарий на своей сборке. Укажите вызванный метод, вход и фактический результат. Для обученного поведения сравните смысл ответа и выбранного получателя. Для ресурса измерьте названную величину за тот же вызов. Если проверка упала, сохраните вход, ожидание и ошибку. Исполнитель может исправить дефект теста по независимому критерию и повторить эту проверку. Отпечаток нужен при разборе происхождения файла или сборки. Дождитесь завершения запущенной проверки. Затем переходите по текущей очереди. Право выполнить перенос определяется действующим назначением.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN audit-91-applicability -->
## Дополнение28.10: фактический выход и происхождение показаний

Для ресурсного переноса сравнивать actual native операции на том же обученном состоянии и в одинаковом окне. Managed allocations, peak и CPU различаются; прогрев/дрейф показываются по блокам. P08 продемонстрировал полезный gold-mass эффект при allocations+155,65%: наличие quality gain и кода в продукте не отменяет HOLD по памяти. Константа счётчика не является наблюдением.

Для FIX-RMS исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-RMS. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-RMS. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-RMS

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Граница полного state oracle, 27 октября 2026.** Собственный h64-rms-coordinate/v1 с LoRA/AdamW400→404 обязателен; random H16 с равными0→0 ответами и соседний h64-rms-row/v1 не закрывают эту карточку. После четвёртого update сравниваются с независимым A все named parameters, включая не задействованный последним readout параметр, moments/step и typed RNG entries. Удаление одного RNG entry или изменение одного float вне текущих logits обязано нарушить comparator. Это конкретизация существующего snapshot/compare §4 и [68, U1/U2](68-transfer-repair-followup-plan-2026-10-27.md), без новых Forward/Train. Прямой gradient oracle сохраняется отдельно: правильная адресация shape не доказывает восстановленный навык.

ID: **FIX-RMS**. Статус: **план переноса RMS-COORD-ALLOC-01 / RMS01**, только документация по `7a9fa58d55127ae88e93cd5d71c24a579f730ca3`. Тема **язык программирования C#**, профиль знака результата целочисленного вычитания; measured consumer — SFT с LoRA и RMSNorm. Следующий ID определяется [реестром](01-registry.md), не номером исторического опыта.

## 1. Измеренный успех, ухудшения и предел навыка

Первичные источники: [A/code-sign.json](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/RMS01/a001/results/A/code-sign.json), [B/code-sign.json](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/RMS01/a001/results/B/code-sign.json), [report](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/RMS01/a001/results/report.md), [one-factor.diff](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/RMS01/a001/one-factor.diff).

| Четыре настоящих SFT updates | A | B | Изменение |
|---|---:|---:|---|
| Allocated bytes | 13 642 856 | 12 352 616 | **−1 290 240, −9,4573%** |
| Wall | 78,5594 ms | 91,4881 ms | **+12,9287 ms, +16,4572%** |
| CPU | 78,125 ms | 93,75 ms | **+15,625 ms, +20%** |
| Peak working set процесса | 42 233 856 bytes | 42 409 984 bytes | **+176 128 bytes** |
| GC gen0/1/2 | 2/1/0 | 2/1/0 | Без изменения |
| Optimizer | 400→404 | 400→404 | Восстановленное продолжение, четыре записи весов |
| TRAIN / EXAM correct | 1/2→1/2 / 1/2→1/2 | 1/2→1/2 / 1/2→1/2 | A/B совпали, роста accuracy нет |

Одинаковы losses `[1.1465623,1.2090048,1.1422932,1.2038769]`, конечные adapter values (digest `006a44c8482de9274aa166110451b5eeee4992074c69735a6e0786572c59db13`), frozen base не изменился. TRAIN NLL T1 `1.3479→1.3276`, T2 `2.428→2.403` меняются одинаково в A/B; это не выигрыш B по качеству.

Старт исторически усвоил T1, но **не весь навык**: TRAIN T1 верен, T2 неверен (оба предсказания «+»); EXAM E1 неверен, E2 верен (оба «−»). Сохранить поштучные ответы и constant baselines: 1/2 объясняется постоянной меткой внутри каждой панели. Не объявлять 50% доказательством различения знаков или обобщения. Это полезный ресурсный результат при реально изменяющихся весах и сохранении ранее приобретённого T1, не `0→0`.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и профилю; рост accuracy не обязателен.** Allocation gain позволяет добавить кандидат несмотря на wall/CPU/peak regressions. Один короткий A→B не устанавливает универсальный runtime gain; перенос в другие формы тензоров требует контрактной совместимости ниже, а доменного quality gain вне code-sign не заявлено.

## 2. Единственный production recipient

Изменить [00.TensorCore/Core/TensorOps.cs:148–214](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/00.TensorCore/Core/TensorOps.cs#L148-L214), **только расчёт bFlat внутри forward-цикла MultiplyBroadcast**. Сейчас каждая позиция создаёт `int[]` через `FlatToIndices(flat,a.Shape)`. Предписанный arithmetic mapping:

~~~csharp
var aDims = a.Shape.Dimensions;
var bDims = b.Shape.Dimensions;
int bFlat = 0, remaining = flat, bStride = 1;
for (int dimension = b.Shape.Rank - 1; dimension >= 0; dimension--)
{
    int bDim = bDims[dimension];
    int aDim = aDims[rankOffset + dimension];
    int coordinate = remaining % aDim;
    remaining /= aDim;
    if (bDim != 1) bFlat += coordinate * bStride;
    bStride *= bDim;
}
~~~

Сохранить right-aligned asymmetric broadcast, прежние проверки b.Shape.Rank≤a.Shape.Rank и каждой размерности, массив **`mappedBIndices`** (используется backward), data multiplication, graph parents, backward обеих сторон и порядок накопления. Cancellation по `flat & 4095` сохраняется, включая flat0. Не удалять `FlatToIndices` из других методов и не менять RmsNorm eps/math. Экспериментальный `ExperimentUseArithmeticBroadcastIndex` в продукт не переносится.

Настоящий потребитель: [RmsNorm.Forward:29–40](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/03.TransformerModel/Layers/RmsNorm.cs#L29-L40) вызывает MultiplyBroadcast дважды — нормированная активация и gamma; далее block/final norm → `LlmModel` → [Trainer.TrainSupervisedNextTokenStep:67–145](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/04.TrainingLoop/Trainer.cs#L67-L145) → loss/backward/clip/AdamW. Эту цепь проверяет learning test, отдельный synthetic timing не назначен.

## 3. Восстановление вместо нового обучения

Future fixture: `Tests/Fixtures/ExperimentFixes/v1/RmsCoordinate01/{manifest.json,train.jsonl,exam.jsonl,historical-A.json,historical-B.json}`; loader [02](02-learning-state-and-fixtures.md#new-resource-fixtures). Donor: `experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/`, готовые weights + optimizer400 + RNG. Producer400 не запускать. Отсутствие bytes/LFS/hash/keys — точный `missing_or_incompatible_fixture`, без поиска другого checkpoint, уменьшения модели или повторной подготовки.

Параметры [Program.BuildModel:487–507](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/RMS01/a001/Program.cs#L487-L507): V256/C64/H64/L1/Q4/KV2/FF176/RoPE10000/RMS eps1e−5; Initializers seed62001; q_proj/v_proj LoRA rank4/alpha8/dropout0, root62002, stream `rms-coordinate-probe-v1`.

Construct model → apply LoRA → load S0.weights → restore RNG → bind adapters by checkpoint key sorted `StringComparer.Ordinal` → import AdamW portable state. AdamW `(beta1=.9,beta2=.95,weightDecay=0,eps=1e-8)`; step строго400. Bindings qA256/qB256/vA256/vB128. Точные RNG identity — SHA UTF8 строк, не произвольно выбранный helper:

~~~text
RMS01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj
RMS01|alpha8|dropout0|seed62002|rms-coordinate-probe-v1
~~~

Создать Trainer: BatchSize1, SeqLen64, WarmupSteps0, TotalSteps404, MaxLr=MinLr=.001, GradAccumSteps1, GradClipNorm1, WeightDecay0. Использовать `TrainSupervisedNextTokenStep(model, optimizer, tokenIds, weight: 1f)`. Scheduler не восстановлен: новый Trainer допустим здесь из-за постоянного LR, не основание заявлять общее продолжение schedule.

| ID | Роль | Prompt | Answer |
|---|---|---|---|
| T1 | TRAIN | `C# result sign: 9-4 =` | `+` |
| T2 | TRAIN | `C# result sign: 4-9 =` | `-` |
| E1 | EXAM | `C# result sign: 8-3 =` | `+` |
| E2 | EXAM | `C# result sign: 3-8 =` | `-` |

Точные UTF8 bytes prompt+answer, без BOS/EOS/newline. Prompt21/answer1; loss — полный shifted sequence на **21 target**, не response-only. Порядок updates **T1,T2,T1,T2**. EXAM не обучается. Readout — один forward prompt-only, argmax последнего логита из256 с прежним tie rule, NLL/rank gold; не Generate. FIX-MS не подмешивается. Public portable optimizer import для fixture не является переносом production lifecycle FIX-MC и не требует внедрить MC.

## 4. Native learning coverage и бюджет

Новые planned files: `Tests/ExperimentFixes/BroadcastCoordinateLearningTests.cs` (один Fact `RestoredSft_PreservesLearningState`), `Tests/ExperimentFixes/BroadcastCoordinateContractTests.cs`, общий runner03. A/B — две native сборки одного harness, только выбранный mapping отличается; никаких archive assemblies или runtime flags.

Один основной Fact на плечо:

1. Проверить donor и происхождение native TensorOps/Trainer/LlmModel до первого forward, восстановить описанный S0.
2. Один before panel: T1,T2,E1,E2, четыре forwards; квалификация T1 correct и точные начальные state identities обязательны.
3. Четыре SFT steps в указанном порядке; сохранить losses, actual optimizer step, adapter/base state. Измерять только этот цикл синхронно на одном потоке: `GC.GetAllocatedBytesForCurrentThread`, wall, CPU, GC; не включать load/readout/export.
4. Один after panel тех же четырёх строк; snapshot weights/optimizer/RNG после измерения. Сравнить с A: одинаковые predictions/NLL/losses, именованные weights и optimizer state; nonzero adapter change, frozen base unchanged, step404.
5. Сохранить отдельные before/after/metrics receipts один раз и использовать их для всех assertions.

В историческом host обе before-панели читались старой ветвью, а B включался перед updates. В native B новый kernel действует и на before. Это осознанная дополнительная проверка совместимости: initial A/B readouts обязаны совпасть. Не добавлять переключатель старого алгоритма в продукт ради имитации host.

Бюджет **на плечо**: 4 training forwards, 4 backward, 4 optimizer updates, 8 diagnostic forwards; для A+B —8/8/8/16. 0 producer updates, 0 DPO, 0 Generate, 0 нейронных warmup steps. JIT/порядок процессов фиксируются в receipt; никаких дополнительных training steps для «прогрева». Финальные bytes сохраняются в attempt/arm и могут читаться последующими assertions без Train, но не подменяют общий donor в другом опыте.

Постоянная CI проверка утверждает actual updates, exact parity и retained T1. Resource comparison показывает новые allocation/wall/CPU/peak deltas; исторические −9,4573% не жёсткий порог другой сборки. Для claim о сохранении allocation gain требуется фактическое `B<A` на назначенном native pair. Если gain не воспроизвёлся, результат так и записать; не запускать повтор до удобного числа и не стирать исторический успех.

## 5. Тесты, которые сохраняются, и точный ремонт

Свежих test failures не установлено. Запланированы только A baseline/B after для:

| Существующий method в LlmFromScratch.Tests | Проверяемый контракт |
|---|---|
| `LoraTests.TensorOps_MultiplyBroadcastBackwardReducesPerRowScale` | Gradients строкового масштаба |
| `LoraTests.TensorOps_MultiplyBroadcastBackwardReducesGammaAcrossRows` | Накопление gamma по строкам |
| `LoraTests.RmsNorm_BackwardMatchesReferenceForInputAndGamma` | Вход и gamma относительно независимого reference |
| `LoraTests.LoraAttention_AllProjectionBGradientsMatchFiniteDifferenceThroughFinalNorm` | Конечный norm в реальном LoRA/attention gradient path |

Сохранить существующие shape/zero-input tests в `TransformerModelTests`; отдельный их запуск не нужен без нового риска. В новом contract class один Theory `CoordinatesAndGradients_MatchIndependentReference`, **шесть именованных cases**: A[2,3,4,5]/B[1,4,1] (rank offset), A[2,3,4]/B[1] (scalar-like), A[2,3]/B[2,3] (same shape), недопустимый rank A[2,3]/B[1,2,3], несовместимая размерность A[2,3]/B[2,2], заранее отменённый token с A[4097]/B[1]. Для всех массивов в row-major порядке заданы a[i]=(i-5)*0.25f, b[j]=(j+1)*0.5f, upstream[i]=((i%7)-3)*0.125f. Case IDs: rank-offset, singleton, same-shape, rank-reject, dimension-reject, cancel-pre. Положительные cases проверяют forward и обе gradients независимым медленным reference по координатам; не повторять arithmetic mapping как oracle. Cancellation проверяет прежний Throw до первого элемента; не утверждать измеренную отзывчивость на середине цикла.

Ни shape cases, ни finite-difference controls не являются отдельным обучающим экспериментом; 0 optimizer updates, стоимость вне learning interval. Они нужны потому, что этот общий tensor метод вызывается настоящим learner с разными формами.

Падение в B при зелёном A означает ошибку переноса: чинить bFlat/right alignment/gradient mapping только в назначенном участке, сохранять прежние expected и tolerance. Изменять gamma/epsilon, понижать gate, вводить Skip или скрывать Tests из Compile запрещено. При общем A/B build failure действуют только точная предпосылка [02 §10](02-learning-state-and-fixtures.md#native-build-prerequisite) и классификация [03 §2](03-test-and-execution-contract.md). Нет обязательного требования, чтобы здоровые controls падали до исправления.

## 6. Исполнение и E2E

Единственный корень — `C:/Projects/llm_mvp_core`, ветка `codex/dpo-implementation`; attempt — `artifacts/fbe/FIX-RMS/attempt-0001/` внутри неё. [Общий контракт 03§9](03-test-and-execution-contract.md#native-ab-transfer) задаёт последовательные Baseline/Candidate. Сначала common harness/tests/fixtures фиксируются на неизменённом native A как `$baselineCommit`. Worktree, sibling/copy, checkout/reset и переключение ветки запрещены. После реализации запланированного runner выполняется только первая стадия:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-RMS -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

Baseline исполняет existing filter → шесть contract cases → один learning Fact A; terminal A receipts сохраняются до product edit. Затем человек или назначенный исполнитель в том же корне меняет только arithmetic coordinates, проводит review и фиксирует `$candidateCommit`. Test/helper/fixture bytes не меняются. Только после этого:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-RMS -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Candidate исполняет existing filter → те же шесть contract cases → один learning Fact B и читает сохранённые A/B receipts, не перезапускает A. В каждой стадии перед `--no-build` выполняется `Rebuild` назначенного Tests проекта по03§9; в паспорт руки входят native assembly Location/SHA-256/MVID и source SHA, чтобы исключить stale DLL общего каталога. Отсутствующие завершённые A receipts блокируют Candidate без Train; автоматические retry/cleanup и новый output не назначены.

Runner сохраняет прежние число test processes, шесть contract cases на руку и по одному основному Fact на native arm, не historical CLI `prepare-shared` или `run-arm`. Learning deadline5 минут на process, максимум2. Штатный wait03 удерживает текущий ход до terminal/deadline, порции≤60секунд; отсутствие output не объявляется отсутствием запуска.

Связь с [FIX-E2E §9](20-prompt-domain-e2e.md#resource-transfer-e2e) — сохранить настоящий recipient, tokenizer/model identity и изоляцию domain state на прежних запросах. Code-sign не добавляется в каталог как новый обученный routing topic. Две локальные SFT руки не повторяются в routing suite; новых E2E updates0. Выигрыш памяти RMSNorm не доказывает улучшение routing или знака вычитания.

## 7. Промпт исполнителю

~~~text
Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/30-broadcast-coordinate-allocation.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.

Выполни только отдельно назначенный FIX-RMS, текущая стадия до назначения — документация.
Модель текущая default; язык report.md/report-for-user.md русский.
Skills в docs/skills: architect-behavior-classifier-SKILL.md, phase-planning-SKILL.md,
plan-review-SKILL.md, code-examination-SKILL.md, test-integrity-SKILL.md,
prompt-patch-authoring-SKILL.md, human-communication-GLOSSARY.md, bug-to-tests-SKILL.md.
Прочитай AGENTS/CLAUDE, 02/03 и эту карточку. Пути, данные, seeds и бюджеты заданы.
WRONG: снова producer400, reset optimizer0, response-only loss, смешать EXAM в TRAIN,
удалить mappedBIndices/все FlatToIndices, перенести runtime experiment flag.
CORRECT: S0+AdamW400+RNG, fixed T1,T2,T1,T2, native B mapping и exact learning parity.
В canonical C:/Projects/llm_mvp_core сначала Stage Baseline и terminal A receipts,
затем назначенный product diff/review/commit, затем Stage Candidate без повторного A.
Каждая стадия Rebuild и native DLL SHA/MVID; no worktree/checkout/reset/branch switch.
Успех — любое измеренное полезное улучшение по профилю, accuracy расти не обязана;
wall/CPU/peak regressions и частичный T1 skill показать явно. 0→0 не proof.
Один process/handle, wait в том же ходе до terminal/deadline; не повтор/новый polling.
STOP/NEXT прекращает прежнюю карточку немедленно: ни отчёта, ни tests, commit, cleanup.
NEXT — точный ID из01; старые опыты и результаты не удалять.
~~~

Отчёт по03 сохраняет restoration identities, fact/test IDs, before→after каждой строки, constant baselines, actual updates и resource deltas. Новые tests/helper пока не созданы, model load/обучение здесь не запускались. Кандидат остаётся решением пользователя о переносе, а не объявлением production gate пройденным.
