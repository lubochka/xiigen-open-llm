# FIX-RRD — суммировать последнюю ось RMSNorm без массивов координат

<!-- transfer-entry-reload -->
**Вход исполнителя.** [R08](01-registry.md#work-fix-rrd). Для принятой части сохранена граница применимости. Открывайте её расширение только по отдельному назначению. Перед каждым переносом и после восстановления контекста заново прочтите полностью агентские файлы, назначенные навыки, исправленный список Любы, текущий индекс и карточку. [Как применить прочитанное к этой работе](03-test-and-execution-contract.md#read-and-apply) · [Что взять сейчас](01-registry.md#executor-next).

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

Для FIX-RRD исходные данные, дозы, модель, доменная область и oracle владельца сохраняются. Это адресное условие будущей приёмки, не свидетельство уже пройденной проверки и не новый запуск. Источник ошибки и точные ремонты — [91/92](92-positive-transfer-followup-plan-2026-10-28.md).
<!-- END audit-91-applicability -->


<!-- BEGIN audit-84-applicability -->
## Уточнение исполнения по аудиту P02/P08/P09

Стоимость относится к точно названному окну. Контрпример P09: разность allocations четырёх arm windows, делённая на4, включает создание входов/cache/receipts и не является постоянной стоимостью одного Forward. Для этой оптимизации указать размер работы, warmup, что внутри/снаружи счётчиков и одинаковую обвязку A/B; не закреплять точные bytes по одному такому среднему. Проверять существующие tests и дополнять их learned fixture, не объявлять отсутствующими по одному отчёту.

Применимость: FIX-RRD. Основание и адресные проверки — [план85](85-reporting-sampling-cache-repair-plan-2026-10-27.md), [общий договор03](03-test-and-execution-contract.md). Это усиление постановки, не утверждение о наличии дефекта в каждом данном исходнике и не новый запуск. Исходный эксперимент/бюджет/статус допуска сохраняются; продолжение пропущенного этапа использует тот же обученный state, новая самостоятельная программа обязана содержать назначенное реальное обучение. При STOP/NEXT следовать прямому указанию пользователя, не продолжать прежнюю задачу.
<!-- END audit-84-applicability -->


<!-- BEGIN audit-87-applicability -->
## Приёмка и состояние обучения: аудит 28 октября

Полный ресурсный бюджет обязан включать обязательные диагностические вызовы и неудачные попытки, с отдельными единицами model/layer и optimizer-call/effective-update. Контрпример P11: 40 model Forward основной части +19 контрольных =59; 8 optimizer calls дают лишь4 изменения весов. Oracle/FD, заявленный обязательным, должен влиять на принятие результата, а не только выводиться в JSON. Равенство старому алгоритму подтверждать независимой проверкой, не комментарием в исходнике.

Применимость: FIX-RRD. Контроль отчёта: точная панель, настоящий учебный этап, терминальное завершение и выполненные обязательные проверки — отдельные поля; успех одной оси не равен общему допуску. Требования по [плану88](88-gradient-warmup-transfer-repair-plan-2026-10-28.md) добавляются к исходному рецепту, не меняют исторические результаты и не назначают повтор обучения. Проверки сохранённого receipt используют raw уже выполненного обучения; новая самостоятельная программа обязана содержать назначенный Train. Если нужен новый вызов модели, сначала явно назначить полный бюджет, а не скрывать его в диагностике. Это профилактическая применимость, не установленный дефект каждого данного исходника. STOP/NEXT и ожидание исходного process handle из карточки сохраняются.
<!-- END audit-87-applicability -->

<!-- BEGIN audit-76-specific-controls -->
## Уточнение по новому аудиту 27 октября: FIX-RRD

<!-- OWNER-CONTEXT-120 --> **Самопроверка выбранного раздела:** CORE по договору120 во входе файла; собственные source/caller/данные/доза и negative+positive этой строки обязательны. Старый завершённый результат не переоткрывать; при падении сохранить input/expected/actual/stack+SHA, не менять oracle ради зелёного. Примеры другого owner не добавляют проверки или Train к этому разделу.

Основание — [76](76-new-transfers-and-experiment-evidence-audit-2026-10-27.md), ремонт — [77](77-new-transfer-repair-plan-2026-10-27.md). Это защита от повторения проверенной ошибки другого переноса, **не утверждение о найденном дефекте реализации каждого ID**. Сохраняются собственные данные, алгоритм, критерии и лимиты этой карточки; дополнительные Train/Forward этой вставкой не назначены.

**M — метрика привязана к фактической стадии обучения.** Для обучающего loss/reward записать состояние, на котором он вычислен, отдельно от номера опубликованных после update весов. У DpoTrainer последняя StepMetrics — pre-update; не называть её оценкой конечного checkpoint. Пример P02: B уже стартовал с margin 0,002773 после SFT; отношение последних значений ×3,22 не равно отношению приростов DPO. Любое сравнение приобретения/ремонта использует собственное before каждого этапа и одинаковый reader. DPO Accuracy=(reward_margin>0) означает улучшение относительно reference, а не правильный ответ: T2 имеет policy chosen−rejected=−0,1827326 при положительном reward. TRAIN/EXAM correctness и сохранённые logP остаются отдельными полями. Если эта карточка сравнивает математически равные оптимизации, сохранить её A/B equality; не требовать искусственного роста точности и не добавлять post-update Forward, когда нужное чтение уже предусмотрено.

**W — отчёт должен воспроизводиться своим источником.** Не закрывать исправление только ручной заменой JSON/Markdown: проверить действующий writer, который создаст следующий отчёт. Attempted, completed updates, diagnostic/model calls и native memory fits различаются. Неизмеренный ресурс — null/NOT_MEASURED, измеренный ноль — отдельный исход. Контроль отказа проходит через реальный producer/обработчик результата; готовый hand-written JSON доказывает только формат. Состоявшееся обучение после ошибки отчёта не повторять. Unit-проверка producer без Train не является новым экспериментом; новая экспериментальная программа содержит назначенный учебный участок.
<!-- END audit-76-specific-controls -->

**Приоритетная поправка R4:** [FU-01/67](67-transfer-repair-followup-audit-2026-10-27.md), [U1/68](68-transfer-repair-followup-plan-2026-10-27.md). Random H16/0→0 из `RestoredStateLearningQualificationTests` не закрывает RD01. Нужен существующий `h64-rms-row/v1`, LoRA и optimizer400→404, полный keyed state/RNG A/B. Scalar/consumer проверки сохранить: неправильная доля одной строки должна ловиться несимметричным upstream, не только ones. Последний update может повредить параметр без изменения прежних losses — final state сравнивается с независимым arm/receipt, не только повтором B. Снимки берутся после назначенных шагов, не после дополнительного обучения. Fallback/keepDims остаются отдельными границами, не расширять fast path под прежним успехом.

**Уточнение исполнения, 27 октября 2026.** Код внесён в `416a64d7e0e`. Прямой scalar sum — независимый primitive oracle, но он не заменяет восстановленные четыре SFT updates и сравнение полного состояния из RD01. Проверить последнюю ось с keepDims=true и прежний fallback раздельно; не расширять условия ускоренной ветви под тем же experiment ID. Потерю градиента следует обнаруживать несимметричным upstream, а не одними ones.

Общие примеры правильной и ошибочной проверки — [контракт03§24](03-test-and-execution-contract.md#transfer-audit-20261027). Это дополнение плана; новые проверки сейчас не запускались.

Дата: 22 октября 2026, UTC. Статус: **план кандидата на основании успешного ресурсного опыта; продуктовый diff и новые тесты ещё не выполнялись**. Основание — RMS-ROW-REDUCE-01 / RD01, а не предположение об ускорении нормализации. Проверенные исходники/evidence: [ebaf7760a36d43172bd88317ff469edfd09a615b](https://github.com/lubochka/llm_mvp_core/commit/ebaf7760a36d43172bd88317ff469edfd09a615b). Очередь и следующий ID определяются [реестром](01-registry.md), а не номером этого файла.

**Решение:** добавить в приватный `TensorOps.SumAlongAxis` узкий путь для последней оси с сохранённой размерностью. Прямое чтение непрерывной строки заменяет вычисление/копирование координат для каждого слагаемого. В историческом опыте четыре настоящих native SFT-обновления из восстановленного learned S0 выделили **на 655 200 байт меньше, −4,804%**, при одинаковых losses, параметрах и optimizer. Ученик сохранил один приобретённый TRAIN-ответ, но не освоил правило знака: качество1/2 равно простому constant baseline. Именно расход обучения составляет измеренный успех; улучшение accuracy не заявляется.

## 1. Экспериментальная база и границы вывода

| Источник на закреплённом SHA | Что из него следует |
|---|---|
| [RD01 report](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/RD01/a001/results/report.md) | Один фактор, ресурсный выигрыш и ограничение «правило не освоено» |
| [Raw A code-sign](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/RD01/a001/results/A/code-sign.json), [raw B](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/RD01/a001/results/B/code-sign.json) | `train_cost`, `training_losses`, `training_panel_before/after`, EXAM, state hashes |
| [Исторический host](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/RD01/a001/Program.cs) | Восстановление S0/AdamW/RNG, четыре вызова native Trainer, factor только внутри training interval |
| [h64-rms-row/v1 bundle](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/bundle.json) и [generation](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/generation.json) | COMPLETE, qualified acquisition T1, законченные producer400 и точная конфигурация |

Историческая база host — `66947785d1318f7cbbc88118f3156cdc75001e16`. Положительный опыт использовал изолированную копию и experiment switch; этот план переводит его на настоящий продукт без такого переключателя. Существующий [FIX-RMS / 30](30-broadcast-coordinate-allocation.md) относится к `MultiplyBroadcast`; он не покрывает private reduction `SumAlongAxis`. [FIX-ATS / 45](45-attention-transpose-strides.md) меняет attention transpose, не эту редукцию. Общие веса похожих H64-семейств не позволяют подменить donor/RNG данной карточки.

| Метрика | A | B | Что установлено |
|---|---:|---:|---|
| Allocated bytes четырёх SFT calls | 13 640 016 | 12 984 816 | −655 200, −4,804% |
| TRAIN correct before→after | 1/2→1/2 | 1/2→1/2 | Приобретённый T1 сохранён; T2 неверен |
| EXAM correct before→after | 1/2→1/2 | 1/2→1/2 | E1 неверен, E2 верен; не доказательство правила |
| Optimizer step | 400→404 | 400→404 | Четыре реальных обновления |
| Online losses четырёх calls | 1,1465623;1,2090048;1,1422932;1,2038769 | Те же | Loss каждой call до её update, не post-update loss |
| Wall / CPU ms | 156,5045 /140,625 | 105,9158 /109,375 | Одно короткое наблюдение, причинное ускорение не установлено |
| GC0/1/2 | 2/1/0 | 2/1/0 | Без изменения |
| Peak working set bytes | 41 893 888 | 42 139 648 | +245 760, ухудшение сохранено |

После обучения совпадают declared trainable digest `37e94d41663ddd6a6df7c570c14ebb665d3b8b6fd4f947e22f2a6805ea387416` и optimizer digest `d48755488f778d1c841cdf93cdc447d4b7f1266c34223103b694d913b2cb80ff`; frozen state неизменён. В raw есть ненулевое изменение trainable state и импортированный optimizer400. Эти утверждения проверены по сохранённым JSON; новый native запуск/перехэширование binaries в данной работе не выполнялись.

Для T1 answer NLL1,3478641664→1,3276255042, rank1, ответ `+` верен; T2 NLL2,4280250332→2,4030496808, rank2, модель продолжает отвечать `+` вместо `−`. EXAM показывает противоположную постоянную склонность, а не усвоение вычитания. Улучшение NLL одинаково в A/B и не является эффектом RRD. **Любое измеренное улучшение полезного показателя считается успехом по этому показателю и профилю; рост accuracy не обязателен.** Нельзя превращать этот ресурсный успех в утверждение, что обучена арифметика или качество выше constant baseline.

## 2. Точный получатель и математическое доказательство

Получатель — [00.TensorCore/Core/TensorOps.cs:1020–1048, SumAlongAxis](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/00.TensorCore/Core/TensorOps.cs#L1020-L1048). Его вызывает [Mean:532–585](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/00.TensorCore/Core/TensorOps.cs#L532-L585). Native [RmsNorm.Forward:29–40](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/03.TransformerModel/Layers/RmsNorm.cs#L29-L40) использует `Mean(squared, axis:-1, keepDims:true)`. Этот путь находится внутри `LlmModel.Forward` настоящего [Trainer.TrainSupervisedNextTokenStep:67–137](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/04.TrainingLoop/Trainer.cs#L67-L137) перед CE/backward/AdamW.

Узкий product diff — в начале private SumAlongAxis добавить guard:

~~~csharp
axis == srcShape.Rank - 1
    && srcShape.Rank == dstShape.Rank
    && dstShape.Dimensions[axis] == 1
~~~

Внутри него один цикл по `dstFlat`, `axisSize = srcShape.Dimensions[axis]`, `start = dstFlat * axisSize`; scalar `float sum = 0f`, цикл `k=0..axisSize-1` и `sum += src[start + k]`. В начале каждой строки сохранить `ThrowIfCancellationRequested()`, внутри цикла — прежний guard `(k & 4095) == 0`. Записать `dst[dstFlat] = sum`, после всех строк вернуть управление. Вся исходная general branch ниже остаётся без изменений.

Не менять `Mean`/нормализацию negative axis, деление на размер оси, `BroadcastGradAlongAxis`, epsilon RMSNorm, тип float, порядок сложения, конструктор Tensor и shape contract. Не применять SIMD/parallel reduction или re-association: это уже другой численный фактор. Не переносить эту ветвь на middle axis либо keepDims=false без нового доказательства/опыта. При ранее принятом другом TensorOps-кандидате он должен быть идентичен в A/B; запрещено незаметно включить несколько оптимизаций.

Для row-major формы `[d₀,…,dₖ₋₁,w]` и сохранённой last axis с размером1 индекс выходной строки r задаёт элементы:

\[
\operatorname{FlatIndex}(i_0,\dots,i_{k-1},j)=r\,w+j,
\qquad
\mu_r=\frac{1}{w}\sum_{j=0}^{w-1}x_{rw+j}.
\]

Обе ветви читают одни и те же элементы в порядке j=0,1,…,w−1, начинают с `0f` и выполняют те же FP32 сложения; исходное `Mean` затем выполняет то же деление. Поэтому меняются только промежуточные integer-координаты. Производная `∂μᵣ/∂xᵣⱼ=1/w` остаётся той же; существующий backward не меняется. Для RMSNorm `r=(mean(x²)+ε)^(−1/2)` и `y=γ⊙x r` неизменность mean и последующего графа означает неизменность y и его производных при тех же входах. Это вывод для данного scalar кода, не утверждение о любой реализации RMSNorm.

Общее правило переноса градиента через композицию даёт [Proposition 9.4](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0312-0317-ch09-motor-taxonomy-and-related-work.md#proposition-9-4). Точная row-major адресация и сохранение FP32-порядка доказаны выше независимо; формулы LayerNorm из книги не подменяют RMSNorm native кода и не обосновывают измеренный ресурсный процент.

В профиле опыта hidden64/21 входная позиция/1 layer+final norm: три нормы за forward,21строка на норму. Старый путь создаёт для каждой строки одну координату и64её копии; в наблюдавшемся64-bit runtime массив3int занимает40байт. Для4updates оценка удаления `3 × 21 × (1 + 64) × 40 × 4 = 655 200` байт точно совпадает с raw дельтой. Такая проверка механизма усиливает приоритет плана, но не превращает размер объекта другого runtime в обещанный постоянный коэффициент.

**Известная граница проверки текущего кода:** `BroadcastGradAlongAxis` сохраняет full-rank `dstIdx` даже для reduced-rank output keepDims=false. По исходникам это отдельный риск backward; выполняемого падения здесь нет и успешного опыта его исправления не представлено. Поэтому fallback keepDims=false ниже проверяется по forward, а gradient-контракты — для keepDims=true, включая неизменённую middle axis. RRD не назначает расширение/починку этого отдельного механизма и не маскирует прежние ошибки изменением expected.

## 3. Восстановить h64-rms-row/v1, а не обучать новый старт

Планируемые общие файлы данной карточки: `Tests/ExperimentFixes/RmsRowStateFixture.cs` и `Tests/Fixtures/ExperimentFixes/v1/RmsRowReduce01/manifest.json`. В них находятся только native loader/data/receipt plumbing, без копии TensorOps/Trainer. Единственный donor — `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/`; producer400 уже завершён. Ни «почти такой же» H64 checkpoint, ни новое обучение до первого правильного ответа не назначены.

| Файл donor | SHA-256 | Bytes |
|---|---|---:|
| S0.weights.bin | `8721dd57b11fa61b11acf37c337611129891a1d3096ae0392a89455d6d58fdec` | 320 620 |
| S0.optimizer.bin | `539d11e925bbff676d9e5969d8c7bfb4326ff93aa1b93f934e8ff9bbbafc15d0` | 7 384 |
| S0.rng.json | `f1a52f59651ee91b78219a6e897efcbc94f69e9a1b6259e2675a69c9cd5db9cc` | 761 |
| preparation.json | `1515ff51ee2c13c25ff1f7910207d92c2df3d63637adb9fe60b71055aa3c8a6e` | 24 586 |
| generation.json | `1ccf772ac2a8802b5bcf0ed9beebb99cffbd27127898bc4f81ea181a8ea82d2c` | 850 |
| tokenizer.json | `65fbb5934ce9a9e997250d6c625408acb841fb20cb271977d328c21fd87de68a` | 819 |
| data.source.jsonl | `5fb81eabe5099672470a6bde4403809282481f4a1ec213846c3e63177e2fd514` | 121 |
| data.material.json | `0f20c602f16eb23e2ec3c5171068b04adf5178402ef951c5f34b8693f24d6f56` | 1 928 |

Сначала проверить actual bytes и bundle COMPLETE/learning_qualified/acquired_ids=[T1]. LFS pointer вместо bin, отсутствующий payload или несовпадение SHA — конкретный setup defect до Forward, без попытки подобрать другую модель. Большие donor-файлы не копировать по одному на каждый тест; manifest ссылается на общий immutable bundle.

Восстановление по native API, в явном порядке:

1. `Initializers.UseDeterministicSeed(62001UL)` → `new LlmModel`: V256/C64/H64/L1/Q4/KV2/FF176/RoPE10000/RMS ε1e−5. `LoraApplicator.ApplySeeded`: q_proj/v_proj,rank4,alpha8,dropout0,seed62002,stream **`rms-row-reduce-probe-v1`**.
2. `model.LoadCheckpoint(S0.weights.bin)`. Составить `AdamWParameterBinding` только для фактически прикреплённых q/v adapter parameters по reference identity, получить keys через `GetCheckpointParameterEntries`, отсортировать `StringComparer.Ordinal`. Не брать все параметры/не менять порядок.
3. Создать native `AdamW(beta1:0.9f,beta2:0.95f,weightDecay:0f,eps:1e-8f)`. `AdamWPortableState.FromBytes` → `TryImportPortableState(bindings, portable)`; отказ — ошибка, не старт с нулевыми моментами. Требовать `portable.Step==400` и полный импорт moments/keys.
4. RestoreRandomState с native `LlmModelRandomStateIdentity`: SHA256 UTF-8 literal `RD01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj` и SHA256 literal `RD01|alpha8|dropout0|seed62002|rms-row-reduce-probe-v1`. Не подставлять stream семейства attention/SILU и не переименовывать identity.
5. Создать native `Trainer` с BatchSize1/SeqLen64/Warmup0/TotalSteps404/MaxLr=MinLr0.001/GradClipNorm1/GradAccumSteps1/WeightDecay0. Его новый локальный scheduler допустим здесь потому, что LR постоянна; это не разрешение сбрасывать scheduler в опытах с warmup/decay. AdamW продолжает **400→404**.

### Назначенный набор без смешения тем

Тема — **программирование**, профиль C#/знак целочисленного вычитания; исторический domain key `programming-language-csharp` сохраняется как provenance. Это не обучение маршрутизатора, перевода или отдельной темы «математика».

| Раздел / ID | Exact prompt | Gold |
|---|---|---|
| TRAIN T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN T2 | `C# result sign: 4-9 =` | `-` |
| EXAM E1 | `C# result sign: 8-3 =` | `+` |
| EXAM E2 | `C# result sign: 3-8 =` | `-` |

Tokenization — UTF-8 byte IDs0..255, без BPE fit/Encode, BOM/BOS/EOS, newline или пробела между prompt и answer. Каждый TRAIN sequence —22bytes,21shifted next-token targets. Сохранить **full-sequence SFT** опыта, не вносить одновременно response-mask. Порядок четырёх calls —T1,T2,T1,T2, weight1. EXAM не участвует в обучении и не меняет расписание. Ответы `+`/`-` выводятся из независимого gold набора, не из ответа baseline или изменённого SumAlongAxis.

## 4. Native обучающий тест и ограниченный бюджет

Планируемый файл `Tests/ExperimentFixes/RmsRowReductionLearningTests.cs`, namespace `LlmFromScratch.Tests.ExperimentFixes`, один Fact **`RestoredSft_PreservesLearningState`**. Он восстанавливает назначенный S0, снимает4readouts before (T1,T2,E1,E2), делает4native `TrainSupervisedNextTokenStep`, снимает те же4readouts after. Никакого автоматического повышения steps до2/2, нового producer или смены датасета.

Квалификация requires retained acquired T1, correct TRAIN before≥1/2 и after≥1/2, записанный ошибочный T2, реальные optimizer400→404, ненулевое изменение adapter parameters и unchanged frozen parameters. Все4raw predictions/logits, gold NLL/rank, losses4, named trainable arrays и portable optimizer/RNG сохраняются. A/B сравниваются по значениям, а не только summary PASS. Исторические digests служат provenance; fresh A/B обязаны совпадать друг с другом на одном runtime, не подгоняться под старый serialized binary hash.

В историческом host factor был выключен на before/after readouts и включён только в training B. В native переносе B-код действует **также на diagnostic forward**; experiment flag не добавляется. Поэтому новый native контракт дополнительно требует A/B parity before и after, что проверяет более широкий реальный путь. Это отличается от схемы historical host явно, без приписывания историческому опыту не проведённой проверки.

Main resource envelope — ровно4синхронных TrainSupervisedNextTokenStep в одном test thread, включая штатные CE/backward/clipping/AdamW. Вне него находятся restore,8readouts, hashing, exports, JSON/report. Primary allocation — `GC.GetAllocatedBytesForCurrentThread`, как в историческом scalar host; можно снять также process-wide total на тех же границах, но не смешивать поля. Wall/CPU/GC/peak сохраняются рядом. После окна native state экспортируется без дополнительного forward.

**На руку:**4SFTupdates,4training forwards,4backward,8diagnostic forwards,4optimizer applications. **На A+B:**8updates,8training forwards,8backward,16diagnostic forwards. DPO/producer/Generate/external0. Переиспользование receipt для отчёта стоит0training; контрактные и существующие tiny tests ниже не выполняют learner/optimizer updates.

Learning/readout budget заканчивается после этой пары. Одно полезное улучшение — measured success по оси; отсутствие resource advantage в новом native B означает «не воспроизведён при переносе», не разрешение повторять до удачного числа. Семантическая parity обязательна для этого точного факторa; улучшение allocation не оправдывает повреждение обученного состояния. Timing из одной пары не превращается в flaky ускорительный CI-threshold.

## 5. Contract tests и действующие тесты, которые сохраняются

Планируемый `Tests/ExperimentFixes/RmsRowReductionContractTests.cs` содержит **шесть Facts**, каждый вызывает native `TensorOps.Mean`. Они защищают именно назначенную training operation, не считаются самостоятельными доказательствами обучения.

| Новый Fact | Вход / oracle |
|---|---|
| `LastAxisNegativeIndex_PreservesValuesAndGradients` | X[2,2,3]=1..12,axis−1,keepDims=true,requiresGrad=true. Shape[2,2,1],means[2,5,8,11]. Upstream[1,2,3,4] → каждая тройка dX равна соответствующему upstream/3 |
| `LastAxisWithoutKeepDims_PreservesForwardFallback` | Те же значения,axis2,keepDims=false,requiresGrad=false. Shape[2,2],means[2,5,8,11]; backward этой неизменённой ветви не назначен |
| `MiddleAxisKeepDims_PreservesValuesAndGradients` | X[2,2,3]=1..12,axis1,keepDims=true. Shape[2,1,3],means[2.5,3.5,4.5,8.5,9.5,10.5]. Upstream[1..6] → каждая из двух исходных строк batch получает соответствующий вектор upstream/2 |
| `FirstAxisWithoutKeepDims_PreservesForwardFallback` | X[2,2,3]=1..12,axis0,keepDims=false,requiresGrad=false. Shape[2,3],means[4,5,6,7,8,9] |
| `LeftToRightRounding_IsPreserved` | X[1,1,4]=[16777216,1,−16777216,1],axis−1,keepDims=true. Scalar FP32 sum1,mean0.25 (побитово),upstream8→dX=[2,2,2,2]. Tree/SIMD re-association этим не разрешается |
| `PreCancelledLastAxis_Throws` | X[1,1,4097], заранее отменённый token,axis−1,keepDims=true. OperationCanceledException, а не завершённая редукция |

Не создавать тест, который просто вызывает новый private loop и вычисляет expected его копией. Оракулы в таблице независимы, gradient1/w выведен математически. Целочисленные результаты forward сравниваются точно; dX при1/3 — тем же фиксированным аналитическим scale, без сравнения с изменённым методом. Contract cases используют native tensors, а не historical copied TensorOps.

**Пять existing tests**, без изменения assertions/tolerances:

| Полное имя после `LlmFromScratch.Tests.` | Причина сохранить |
|---|---|
| `TensorCoreTests.Mean_AlongLastAxis_ReducesDimension` | Действующий last-axis expected[2,3] |
| `TransformerModelTests.RmsNorm_ZeroInput_DoesNotThrow` | Поведение нулевого input и epsilon |
| `TransformerModelTests.RmsNorm_NormalisesEachVector_ApproximatelyToUnitRms` | Семантика native RmsNorm |
| `LoraTests.RmsNorm_BackwardMatchesReferenceForInputAndGamma` | Input/gamma backward, analytic/finite-difference oracle |
| `LoraTests.LoraAttention_AllProjectionBGradientsMatchFiniteDifferenceThroughFinalNorm` | Связь обновляемых адаптеров с конечной нормой |

Источники: [TensorCoreTests](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/TensorCoreTests.cs#L202-L216), [TransformerModelTests](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/TransformerModelTests.cs#L35-L81), [LoraTests](https://github.com/lubochka/llm_mvp_core/blob/ebaf7760a36d43172bd88317ff469edfd09a615b/Tests/LoraTests.cs#L795-L920). Текущая документная проверка **не запускала эти тесты и не установила свежие падения**. Если A зелёный/B красный, чинить guard/start/порядок/cancellation назначенной редукции; менять expected на actual или расширять tolerance запрещено. Если A уже красный, фиксировать ID/stack и точную классификацию по03; не начинать широкий solution repair.

Existing filter — OR пяти полных FQN через `FullyQualifiedName=`. Contracts filter `FullyQualifiedName~LlmFromScratch.Tests.ExperimentFixes.RmsRowReductionContractTests.`; learning filter `FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.RmsRowReductionLearningTests.RestoredSft_PreservesLearningState`. Discovery **5existing+6contracts+1learning=12 cases** на руку; mismatch — setup error. Единственная общая build prerequisite — [02§10](02-learning-state-and-fixtures.md#native-build-prerequisite), одинаковая в A/B, без удаления других failing tests.

## 6. Один canonical checkout, native A/B и отчёт

Договор [03§9](03-test-and-execution-contract.md#native-ab-transfer) применяется с PlanId **FIX-RRD**. Будущие helpers `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` и `Compare-NativeAB.ps1` **ещё не реализованы**. Сначала создать их и указанные tests/helper/small fixture в общем harness commit при неизменённом product A. `$baselineCommit` — фактический SHA этого commit. Canonical root `C:/Projects/llm_mvp_core`, branch `codex/dpo-implementation`; других checkout, test assembly copies, worktrees и branch switches нет.

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-RRD -Stage Baseline -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit
~~~

Только после terminal A вне runner внести назначенную last-axis ветвь SumAlongAxis, review/commit → `$candidateCommit`:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/Tests.Support/ExperimentTransfer/Run-NativeAB.ps1 -PlanId FIX-RRD -Stage Candidate -SourceRoot C:/Projects/llm_mvp_core -BaselineCommit $baselineCommit -CandidateCommit $candidateCommit
~~~

Stage на том же корне делает один `dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -t:Rebuild`, один discovery12cases и три отдельных `dotnet test -c Release --no-build` процесса: **existing → contracts → learning**. Итого2Rebuild/2discovery/6test processes; только learning выполняет4updates. B проверяет A ancestor B, точный product diff одного метода, неизменные tests/helper/fixtures/csproj и runtime/ISA. До model calls сохранить native TensorCore/TransformerModel/TrainingLoop assembly Location/SHA/MVID; загруженная A DLL в B — отдельный build defect.

Output `artifacts/fbe/FIX-RRD/attempt-0001/`. CreateNew `intent.json` до работы; B отдельно CreateNew `candidate-binding.json` с candidate SHA и digest A intent. Subpaths `baseline/A`, `after/B`, `contracts/A`, `contracts/B`, `learning/A`, `learning/B`; на process собственные ownership/status/TRX/stdout. Learning receipts: exact inputs/donor hashes, `native-assemblies.json`, `before.json`, `after.json`, `losses.json`, `train-cost.json`, named state arrays, portable optimizer/RNG, completed/not_reached counters. Report/comparison читают сохранённые receipts; `report.md` и `report-for-user.md` по-русски, без Train/Forward.

Build watchdog15min/stage, learning и остальные test processes5min/process; это deadline, не дополнительные steps. Уже completed stage используется как готовое evidence; живой owner ожидается; неизвестный/незавершённый owner — именованный defect. Не удалять output, не менять прошлые raw, не выбирать attempt-0002 автоматически. После ошибки сохранения отчёта допускается только чтение прежних raw для исправления formatter, без нового обучения.

## 7. Связь с E2E и область действия

Оптимизация относится к learner с row-major last-axis Mean, а не к теме как таковой. C# — измеренный профиль; для другого hidden size/числа слоёв/sequence length расход изменится. Доказательство численной эквивалентности применимо при том же guard; ресурсный процент и обучение нового домена переносить без опыта нельзя. AST/порядок предложений/KNN этой карточкой не меняются.

[FIX-E2E](20-prompt-domain-e2e.md) может после принятия кандидата использовать новый native TensorCore в уже назначенном выбранном learner. Router state и teacher authority остаются отдельными от H64 learned state; нельзя выдавать `C# result sign` S0 за router fixture. Существующий E2E plan отвечает за prompt→правильный domain→recipient→ответ, а эта карточка — за цену реального SFT-update внутри выбранного learner. Нового router training или E2E-прогона сверх назначенного бюджета RRD нет.

## 8. Готовый промпт исполнителю

~~~text
Действующий вход этого передаваемого задания (02.11.2026): прочитай целиком карточку C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/44-rms-row-reduce.md. Свежее полное чтение AGENTS/CLAUDE и основного стека навыков: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md, §1; дополнительные роли и результат их применения: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md, §2; проверка применимости и разбор падений: C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/codex-42-postmerge-executor-checks-2026-11-02.md. Короткие списки навыков ниже только дополняют этот вход. Выполняется лишь прямо назначенный пользователем ID/этап; старый императив «реализуй/запусти» не разрешает исполнение при документальном поручении. Данные, один фактор, модель, команды, бюджет и хорошие/плохие примеры берутся из этой полной карточки; нерешённое противоречие не разрешается подбором gold, нового обучения или чужого механизма.

Выполни только отдельно назначенный FIX-RRD; сейчас это документный план.
Default model текущей сессии; русский report.md и report-for-user.md под
artifacts/fbe/FIX-RRD/attempt-0001/. Прочитай AGENTS/CLAUDE,02/03 и эту карточку.
Skills: docs/skills/architect-behavior-classifier-SKILL.md,
trainable-architecture-decision-SKILL.md,phase-planning-SKILL.md,
plan-review-SKILL.md,code-examination-SKILL.md,test-integrity-SKILL.md,
experiment-design-SKILL.md,prompt-patch-authoring-SKILL.md,
human-communication-GLOSSARY.md; остальные обязательные роли перечислены03§4.
Один фактор: contiguous scalar last-axis SumAlongAxis при same-rank keepDims.
Не меняй Mean/backward/epsilon/float-order и не чини отдельный keepDims=false risk.
Существующий h64-rms-row/v1,полные weights+AdamW400+RNG;0bootstrap.
T1,T2,T1,T2;четыре SFT updates на руку,восемь diagnostics;exact data/config выше.
A до product edit,B после,один canonical root; никаких copied native assemblies.
Любой измеренный полезный выигрыш — успех по оси/профилю,accuracy расти не обязана.
Не называй1/2 освоенным правилом;покажи constant baseline и все regressions.
Один process/handle; жди terminal штатными wait-tools в том же ходе,
порциями≤60секунд. Не заканчивай раунд вместо ожидания;никаких blind polling,
дублирующих watcher loops,автоповторов или нового fit ради оформления отчёта.
STOP/NEXT немедленно прекращает прежний ID: запрещено дописывать его report,
тесты,коммит или cleanup. NEXT только поID реестра01;старые experiments не удаляй.
Новых DPO нет. Если их отдельно назначат: генератор — внешняя открытая китайская
модель;выбор в роли человека и quality оценка — CODEX с astra 6 ultracode.
Исполнитель default не выдаёт собственную проверку за исполнение этих ролей.
~~~

`exec_command` session ожидается через `write_stdin` того же session_id; yielded functions-cell — через `functions.wait` того же cell_id после реального yielded ответа. Нет вывода «процесс не работает» только потому, что output ещё не появился. При прямом NEXT не продолжать предыдущий ID ради его завершения. Текущая работа создала план по source/raw; **новые restore/build/test/training команды не запускались**.
