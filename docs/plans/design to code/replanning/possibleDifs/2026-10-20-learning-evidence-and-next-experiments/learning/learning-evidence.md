# Дополнительные расходы настоящего обучения: что новое и что уже покрыто

20 октября 2026. Только доказательная документация. Никакого нового обучения, исполнения C#, изменения production или защищённого каталога этим проходом не было.

## Вывод

Найден один новый узкий кандидат и одно новое уточнение уже известной семьи RMS: лишняя копия при первом накоплении градиента и массив обратных broadcast-индексов в полностью замороженном forward. Оба участка входят в настоящий DPO-шаг; предполагаемый полезный эффект — меньше выделений памяти за одинаковое обучение. Влияние на значения ответов и фактическая экономия **не измерены**. Они не объявляются причинами плохих ответов RP01 и не получают статус успешного эксперимента до запуска.

| Кандидат | Конкретный участок | Новизна относительно проверенного корпуса | Состояние |
|---|---|---|---|
| [GRAD-ACCUM-COPY-01 — одна защитная копия первого вклада градиента](GRAD-ACCUM-COPY-01.md) | `TensorOps.AccumulateGrad`, только `target.Grad == null` | В просмотренных текстах нет отдельного такого A/B. Известный LF-03 меняет `Tensor.Backward(initialGradient)`, другую строку и другой срок жизни массива | Код подтверждён; опыт не запускался |
| [FROZEN-BROADCAST-MAP-01 — не хранить backward-индексы при отсутствии gradient graph](FROZEN-BROADCAST-MAP-01.md) | `TensorOps.MultiplyBroadcast`, только оба `RequiresGrad == false` | Уточнение семьи RMS. Известный RMS-COORD-ALLOC-01 меняет `FlatToIndices`, явно сохраняя `mappedBIndices`; здесь координатный алгоритм не меняется | Код подтверждён; опыт не запускался |

Предметная тема первого ограниченного сравнения — **документация C#**. Обучающийся получатель — Student с q/v-LoRA. Копии массивов не являются новыми доменами, архитектурными Unit или детерминистическими ответчиками. Для перевода или другой темы применимость возможна только при том же tensor/DPO пути; это не перенос результата на TranslationKnnStore или семантические таблицы.

## Подтверждённые учебные цепочки

Для первого кандидата: [DpoTrainer.StepVectorCore:832–943][step] → [BackwardLogProbs:1072–1121][backward] → [Tensor.Backward(Tensor):94–103][tensor-back] → backward-функции [MatMul:646–699][matmul] и других TensorOps → [AccumulateGrad:984–997][accum] → общий AdamW update. `AccumulateGrad` сначала исполняет `gradToAdd.Data.ToArray()`, а [Tensor:39–46][tensor-ctor] и [TensorStorage:21–24][storage] ещё раз копируют переданный массив. B убирает только первую из этих двух копий. Последующие `+=` не меняются. Это отличается от удаления копии начального logits-gradient перед запуском графа в LF-03.

Для второго: тот же [StepVectorCore:879–889][reference] реально вычисляет frozen-reference log-probs обеих сторон до policy update. [Simulation owner:110–112][freeze] замораживает отдельную reference-модель. [LlmModel.Forward:219–242][model] проходит блоки и final norm; [TransformerBlock:46–64][block] вызывает два RmsNorm; [RmsNorm:29–40][rms] вызывает два MultiplyBroadcast. [MultiplyBroadcast:167–184][broadcast-map] выделяет `mappedBIndices` и пишет его всегда; [187–210][broadcast-back] читает его только внутри gradient graph. В полностью замороженном reference эти индексы не используются после forward. Это необходимое вычисление DPO reference, а не посторонний inference benchmark.

## Снятая новизна

Первоначальный поиск по старому кэшу не нашёл per-element `FlatToIndices` allocations. После полного актуального снимка найден [RMS-COORD-ALLOC-01][prior-rms], в точности описывающий этот фактор. Новая карточка с таким же вмешательством **не создаётся**. Вторая новая карточка ссылается на эту семью и не присваивает её открытие.

Также не объявляются новыми: AdamW scale cancellation/возраст параметра/переполнение второго момента; LoRA A-scale/gauge; DPO padding; scratch `expVals`; initial-gradient copy LF-03; frozen Linear transpose; перестановка циклов `gradB`; потеря response masks или недостижимый исторический autograd. Эти темы уже присутствуют в existing possibleDifs и/или protected-карточках. Текущий source сохраняет live LoRA, `(oneHot-softmax)` и вызов `logits.Backward`; старый исторический обрыв не является желательным алгоритмом.

## Граница покрытия защищёнными экспериментами

На проверенном core `883496bca18113bf37e0afe683c75b05a0075048` protected-каталог содержит [LF-03 внутри N62][prior-lf] и [RMS-COORD-ALLOC-01][core-rms]. Они не проверяют два новых точных вмешательства. Факт описания широкой категории Tensor/LoRA не означает наличие эксперимента для конкретной строки. Поэтому статус этих двух строк — **подготовлены для добавления, физически не добавлены в защищённый каталог**. Запрет пользователя соблюдён. Полные карточки, промпты и [точный контракт исполнения](TENSOR-LEARNING-EXECUTION-CONTRACT.md) находятся в данном пакете possibleDifs.

## Приоритет и результат

Текущий следующий — ещё не завершённый LF-03, исходный №07 по [актуальному реестру](../04-execution-registry.md); исходные №01–06 уже имеют сохранённые результаты и повтор не назначен. [Единый реестр пакета](../04-execution-registry.md) задаёт: **исходные №07–15 → №17–31 → ROPE-TRAIN-TABLE-01 → AST-SIB-CACHE-01 → NG01 → BPE-REALIZED-MERGE-COUNT-01 → GRAD-ACCUM-COPY-01 → FROZEN-BROADCAST-MAP-01 → IDENTITY-M1-01 (исходный №16) → KN-ROUTER-LEARNED-KEY-01 (исходный №32) → END**. Всего 38 строк: 6 завершены, 32 ожидают. GRAD NEXT — FROZEN; FROZEN NEXT — IDENTITY-M1-01. ID стабильны, исходный номер хранится отдельно. Эти файлы не создают собственную конкурирующую очередь. Если указан конкретный ID — выполнить только его в порученной фазе. Документный аудит сам обучение не разрешает и не выполняет.

Любое измеренное улучшение полезной метрики — успех именно этой метрики и предметной темы; рост точности не обязателен. Конечные веса/optimizer/RNG, TRAIN NLL/rank/teacher-forced accuracy и все ухудшения показываются рядом. Уменьшение theoretical payload не выдаётся за measured RAM. Один A→B без повторов не устанавливает устойчивость wall-time/CPU.

## Чтение навыков и источников

Применены: архитектурная граница и evidence-layer, code-examination с upstream/downstream, learning-path-audit, plan-review с одним фактором, prompt-patch-authoring, human-communication glossary. Прямой текущий запрет изменения production/protected и требование малых экспериментальных копий выше старых требований полного CI/рефакторинга/merge. Порог успеха из старого promptops-cycle не применяется: действует правило пользователя о любом полезном изменении метрики.

Идентичность source проверена через GitHub fetch_file по core SHA, а не по памяти. `TensorOps.cs` Git blob `e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b`, `Tensor.cs` `a2569eefe378fb0a841cbb2137c6a8ac001bdb3a`, `TensorStorage.cs` `22aec3ba03b756530001ae786acbdce28af90104`, `RmsNorm.cs` `246bbc4781d4c06d3b71f617d51aec8860ac39f2`, `DpoTrainer.cs` `0a7fd4d088fd37824aa213ddf0c6189675fe7dc2`. Дедупликация выполнена по полностью материализованному possibleDifs на planning SHA `6bc4b6fba8f14870f5dc4966cf5a344754962d66`; сырой поиск и ссылки на ближайшие предшественники — [learning-novelty.json](learning-novelty.json). Поиск ограничивает утверждение о новизне этим корпусом и датой; он не доказывает отсутствие идеи в любой истории проекта.

## Дополнение после concurrent planning 53cb

На planning `53cbcabd3926cda714bab428d294eb666301e9e4` адресно и содержательно сверены пять документов: [ROPE-TRAIN-TABLE-01][concurrent-rope], [AST-SIB-CACHE-01][concurrent-ast], [SKNN-SIBLING-PREP-01][concurrent-sknn], [BPE-REALIZED-MERGE-COUNT-01][concurrent-bpe] и [12-source-and-novelty-ledger.md][concurrent-ledger]. Дополнительно root выполнил буквальный поиск по всем 53 добавленным или изменённым файлам. Точное удаление внешней копии первого `AccumulateGrad` и отказ от no-grad `mappedBIndices` не обнаружены.

Содержательное различие сохраняется: ROPE повторно использует training cos/sin; AST кэширует sibling-признаки Structural KNN; SKNN уже помечен `REFERENCE_ONLY / NO_SEPARATE_RUN` как альтернативный рецепт AST; BPE меняет цель выбора merges. **GRAD остаётся отдельным новым фактором в проверенном корпусе. FROZEN остаётся отдельным уточнением известной RMS-семьи:** прежний RMS-COORD меняет `FlatToIndices`, а FROZEN сохраняет его и меняет только backward-map при отсутствии графа. Чужие ID и результаты сохраняются; SKNN не становится дополнительным запуском.

Это дополнение к первоначальному 747-файловому planning-снимку, не заявление о повторном ручном прочтении всех новых файлов или всей истории. Исходный `learning-novelty.json`, его inventory и SHA не переписывались. Core доказательств остаётся `883496bca18113bf37e0afe683c75b05a0075048`; новые measured результаты не появились.

[concurrent-rope]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/06-ROPE-TRAIN-TABLE-01.md#L15-L22
[concurrent-ast]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md#L49-L55
[concurrent-sknn]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/06-SKNN-SIBLING-PREP-01.md#L1-L7
[concurrent-bpe]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/08-BPE-REALIZED-MERGE-COUNT-01.md#L13-L43
[concurrent-ledger]: https://github.com/lubochka/xiigen-planning/blob/53cbcabd3926cda714bab428d294eb666301e9e4/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-delta-audit-883496/12-source-and-novelty-ledger.md#L40-L70

[step]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L832-L943
[backward]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L1072-L1121
[tensor-back]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/Tensor.cs#L94-L103
[matmul]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L646-L699
[accum]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L984-L997
[tensor-ctor]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/Tensor.cs#L39-L46
[storage]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorStorage.cs#L21-L24
[reference]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L879-L889
[freeze]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L110-L112
[model]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/03.TransformerModel/LlmModel.cs#L219-L242
[block]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/03.TransformerModel/TransformerBlock.cs#L46-L64
[rms]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/03.TransformerModel/Layers/RmsNorm.cs#L29-L40
[broadcast-map]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L167-L184
[broadcast-back]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/00.TensorCore/Core/TensorOps.cs#L187-L210
[prior-rms]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-causal-delta-audit/RMS-COORD-ALLOC-01-CARD.md
[prior-lf]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#queue-lf03
[core-rms]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/RMS-COORD-ALLOC-01.md
[queue]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md

**Уточнение core669:** N62-R2 уже завершён, повтор запрещён; NEXT общей программы LF-03. Source-copy этих двух факторов остаётся на883: [оба Tensor-файла идентичны669, R2 меняет другой механизм](CORE669-DELTA.md).

**Финальный статус827:** №01–06 completed, NEXT=LF-03; [реестр](../04-execution-registry.md). TensorOps/TensorStorage на827 имеют те же blobs, что883/669; заданный source-copy pin883 не меняется.
