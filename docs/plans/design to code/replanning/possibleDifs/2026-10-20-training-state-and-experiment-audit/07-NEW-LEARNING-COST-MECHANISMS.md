# Новые ограниченные гипотезы внутри фактического обучения

Дата: 20 октября 2026. Core: `62bcc8f2368840e22d2ea98ef16d9bd526ad9a39`. Planning: `fb9aadf69e0669d36b820d61f76fe3ecc8697a7d`.

**Найдены два конкретных механизма, для которых в проверенном корпусе нет отдельного малого A/B опыта. Они могут уменьшить стоимость обучения; причинность ухудшения предметного качества пока не установлена.** Подготовлены изолированные копии настоящего C# learner и точные команды. Обучение, сборка, CI и продуктовые исправления в этой работе не выполнялись. SDK `dotnet` в среде отсутствует, поэтому статус scaffold — `MATERIALIZED_NOT_COMPILED_NOT_RUN`, а не «сборка прошла».

| ID | Проверяемое отличие | Реальное место в обучении | Приоритет и статус |
|---|---|---|---|
| `NEURAL-FROZEN-TRANSPOSE-01` | Повторная материализация транспонированной неизменной base-матрицы при каждом `Linear.Forward` | Native SFT → Transformer → frozen Linear → native backward → AdamW; тот же Linear вызывается в DPO | P1 после устранения блокировок TRAIN. Механизм подтверждён исходниками; эффект не измерен |
| `BPE-NO-MATCH-COPY-01` | `MergeChunk` выделяет и копирует список даже при отсутствии очередной пары | Native `BpeTokenizer.Train` внутри `KnnLogicalUnit.Train`; тот же MergeChunk используется при Encode учебного материала | P1 стоимости подготовки/обучения токенизатора. Эффект не измерен; accuracy KNN/NN не заявляется |

Любое фактически измеренное улучшение полезного показателя является успехом **по этому показателю и предметному профилю**. Рост accuracy не обязателен. Ухудшения остальных показателей показываются рядом. Ресурсный выигрыш при изменившихся ответах сохраняется в отчёте, но не называется эквивалентным ускорением и сам по себе не разрешает перенос в основной код.

## 1. Повторный transpose frozen base внутри LoRA TRAIN

### Что доказано кодом

1. [`Linear.Forward`, строки 41–60][linear] всегда вызывает `TensorOps.Transpose(Weight.Value, 0, 1, ...)`; ветви для `RequiresGrad == false` нет. Метод не `virtual`. Подкласс с одноимённым методом не перехватит вызовы в существующих полях типа `Linear`.
2. [`Transpose`, строки 721–779][transpose] создаёт массив размером с матрицу и переносит значения. Затем [`Tensor(float[], ...)`, строки 40–48][tensor] вызывает [`TensorStorage(float[])`, строки 20–23][storage], который копирует переданный массив через `ToArray()`.
3. [`LoraLinear.Forward`, строки 112–125][lora-forward] начинает с `_base.Forward(x, ...)`. Заморозка base запрещает обновление её весов, но не устраняет повторную транспозицию. Обучаемые A/B отдельно транспонируются каждый раз; их кэшировать данным изменением нельзя.
4. Обычный Transformer forward вызывает Q/K/V/O projections ([attention, 138–170][attention]), три FFN Linears ([SwiGLU, 40–51][ffn]) и LM head ([LlmModel, 219–243][model]). Таким образом, затраты существуют и в неадаптированных замороженных проекциях, а не только внутри двух LoRA slots.
5. [`TrainSupervisedNextTokenStep`, 62–150][sft] действительно выполняет forward, CE backward, clipping и `AdamW.Step`. [`DpoTrainer`, 878–923][dpo] отдельно проводит reference/policy forwards, backward и optimizer. Это зависимость от настоящего обучения, не самостоятельный serving benchmark.

Из исходников следует арифметическая оценка: повторный transpose матрицы из `N` FP32 значений создаёт примерно `8N` байт **полезной нагрузки двух float-массивов**, не считая объектов, shapes и градиентов. Это расчёт по коду, не результат профилирования. Кэш сохраняет примерно `4N` байт значений на время жизни матрицы и поэтому может увеличить удерживаемую память.

### Единственное вмешательство

В экспериментальной копии `Linear.cs` B запоминает результат transpose только для frozen Weight. A исполняет исходное тело через общую ветвь инструментации. Оба плеча компилируются из одного host; численная математика, порядок MatMul, LoRA A/B, исходные веса, dropout, clipping и optimizer одинаковы. Создание кэша оплачивается **внутри первого измеряемого TRAIN**, до него B не прогревается.

Кэш действует только для одного экземпляра модели с неизменной base. В этом опыте нет restore, merge LoRA, изменения base или конкурентного использования. Для будущего production-кандидата потребуется явная инвалидация при загрузке/изменении base или версия весов; один `RequiresGrad == false` не доказывает, что публичный `Weight.Value.Data` никто не меняет. Нельзя распространять этот короткий опыт на recovery без отдельного доказательства.

### Что проверяет маленький опыт

Один Transformer block, два LoRA slots, четыре настоящих SFT updates в A и четыре в B. Предмет: знак целочисленного вычитания в C#; два урока и два контрольных выражения с другими числами. Основные показатели — выделенные байты, время и CPU непосредственно четырёх TRAIN шагов. A0/A1/B0/B1 показывают реальные последний логит, NLL целевого байта и точность на двух EXAM. Hash обучаемых весов и optimizer moments после TRAIN подтверждает, изменилось ли фактическое состояние, и позволяет сравнить плечи.

Это намеренно native SFT/LoRA, **не DPO-эксперимент**. Исходники доказывают применимость причины к общему Linear в DPO, но измеренные здесь величины нельзя выдавать за DPO latency, DPO quality или качество всего RP01 Flow. Полноценный DPO caller и его admission в этом опыте не заменяются имитацией.

## 2. Копирование BPE-чанка без совпадения пары

### Что доказано кодом

1. [`BpeTokenizer.Train`, 42–68][bpe-train] учит словарь: считает пары, выбирает merge, добавляет токен и вызывает `MergeAll`.
2. [`MergeAll` и `MergeChunk`, 540–565][bpe-merge] обходят каждый chunk на каждом merge. `MergeChunk` сразу создаёт `new List<int>(ids.Count)`, даже если проверяемая пара ни разу не встречается. В таком случае содержимое выходного списка совпадает с входным.
3. [`ApplyMerges`, 568–574][bpe-apply] вызывает тот же метод для каждой ранее выученной merge-пары при Encode.
4. Это фактический путь обучения существующего logical Unit: [`KnnLogicalUnit.Train`, 132–153][knn-train] включает BPE.Train на текстах уроков, а затем Encode/Vectorize этих уроков перед записью в HNSW. Время и allocated bytes входят в native training manifest, строки 156–178.
5. Для neural DPO тот же tokenizer участвует в подготовке принятой пары: [`DpoExactPairMaterial.CreateBatch`, 64–87][dpo-material] фиксирует tokenizer и кодирует prompt/chosen/rejected. Данный опыт не меняет это доверенное место построения batch.

### Единственное вмешательство

В B выходной список создаётся при **первом реальном merge**. Тогда один раз копируется неизменный префикс и продолжается тот же левосторонний неперекрывающийся merge. Если пары нет, метод возвращает исходный список; этот метод и его callers не мутируют переданный список. Не меняются frequencies, tie policy, порядок слияний, token IDs, RegexSplitter, словарь, размер словаря, нормализация и Unicode. Отсутствие копирования не означает отказ от просмотра chunk; выигрыша времени заранее не обещаем.

### Что проверяет маленький опыт

Два отдельных предметных корпуса: тексты документации к C# и парные EN→RU предложения. В каждом A и B реально учат native BPE от byte-vocab до целевого словаря 320. Четыре независимых native процесса, без KNN/NN обучения. По каждому домену: A0/B0 — vocabulary256; A1/B1 — результат одного вызова native Train на одинаковом корпусе. Измеряемые TRAIN cost и merge/state deltas принадлежат **обучению токенизатора**, которое прямо вызывается в продуктивном `KnnLogicalUnit.Train`.

EXAM использует выученный tokenizer и показывает token IDs/count и точность round-trip, а не качество перевода/документации. Полная идентичность канонического tokenizer state и выходных token IDs ожидается и проверяется. Сокращение allocated bytes допустимо признать успехом по стоимости обучения BPE при нулевой разнице accuracy — accuracy KNN/NN здесь вообще не измеряется. Опыт нельзя переименовать в успешное обучение переводу.

## 3. Сверка с существующими документами и защищёнными карточками

Проверены 577 Markdown в текущем `possibleDifs` и 286 Markdown в `docs/plans/replanning-possible-diffs`; manifest корпуса и точные поисковые совпадения сохранены в [журнале поиска](evidence/neural-tokenizer-novelty.json). Поиск конкретного frozen transpose cache не дал совпадений. Идентификаторы `MergeChunk`/`ApplyMerges` найдены только в N21 и N50, посвящённых другой причине. Это ограниченная новизна относительно проверенного корпуса, не заявление о мировой оригинальности.

| Существующее покрытие | Содержание | Отношение к новому опыту |
|---|---|---|
| [P09, cached forward][p09] | KV-cache, prefill/append/absolute position | Другая память и другой участок. Новый опыт сохраняет обычный TRAIN forward и лишь транспонированную immutable base matrix |
| [P110, numeric matrix cost][p110] | MatVec numeric implementation, sparsity threshold, стоимость consumer | Родственная тема стоимости; нет данного lifetime/transpose фактора, ни назначенного native TRAIN A/B |
| [P17, tokenizer refit][p17] | Повторное обучение и согласование tokenizer/index | Новый опыт сохраняет learned dictionary и index representation неизменными |
| [P98, tokenizer capacity][p98] | Влияние размера vocabulary на representation/sequence cost | В новом опыте target size и фактически выученный state одинаковы |
| [N21 BPE vocab drift][n21] | Порядок уроков/добавление постороннего урока меняют vocab | Корпус и порядок здесь фиксированы, меняется только создание списка при no-match |
| [N50 seed budget][n50] | Технические seed строки занимают merge budget | Корпус, budget, частоты и merge order здесь сохраняются |
| Недавние DPO pad, reference-score reuse, softmax scratch | Длина последовательности, устранение reference forwards, массивы softmax | Не включены как новые гипотезы; никакой из этих факторов не меняется |

**Вывод покрытия:** наличие общей P09/P110/P17/P98 не является готовым экспериментом для этих двух конкретных причин. Ни их валидационный запуск, ни результаты нельзя считать уже имеющимися. Новые additive карточки находятся в `experiments/linear-transpose/` и `experiments/bpe-copy/`; защищённая папка не менялась.

## 4. Подготовленные исполняемые материалы и их граница

Каждый packet содержит `README.md`, `EXECUTOR-PROMPT.md`, `scaffold/prepare.py`, `scaffold/run.py`, `scaffold/compare.py`, literal host `scaffold/35.ExperimentsV2/Experiments/Rp01Micro/<ID>/Runner.csproj` и подготовленные исходники в `scaffold/experiments-v2/RP01-MICRO/<ID>/`.

Neural closure — 36 canonical `.cs` плюс один Program; BPE closure — два canonical `.cs` плюс Program. Нет `ProjectReference` на крупные проекты, существующий dispatcher и product tests. `prepare.py` читает `--core-root` без записи, сверяет каждый Git blob SHA, копирует только нужные файлы в planning scaffold и меняет один механизм. Сохраняет source hashes, hash Program и `one-factor.diff`. Исходники подготовлены; компиляция не выполнялась из-за отсутствия SDK. Host не называется существующей командой основного core-репозитория.

Результаты будущего исполнения назначены только в новом planning-пакете: `results/NEURAL-FROZEN-TRANSPOSE-01/A|B/` и `results/BPE-NO-MATCH-COPY-01/A|B/`. Полный core, `docs/plans/replanning-possible-diffs`, предыдущие experiments/results, модели и веса не изменяются. Повторное исполнение не перезаписывает существующий результат.

## 5. Применённые навыки и роли

Применены принципы указанных пользователем навыков из `docs/skills`: architect behavior classification — явная связь state/output/caller; planning startup и plan review — один фактор, конкретные пути, разрешённые границы и проверяемые исходы; code examination/generated code review — доказательство текущими bytes и method call chain; prompt patch authoring — буквальный исполнительский промпт без поиска места и сценария; human communication glossary/readability — различение предмета, механизма и направления перевода. Наличие навыка не вводит новое личное разрешение для каждого ID/SHA и не разрешает менять запрошенную документную работу на запуск обучения.

Результат статического аудита отделён от результата эксперимента: факт лишнего копирования доказан исходником; полезный эффект и его размер должны быть измерены; влияние на качество RP01 и причинность прежнего ухудшения остаются непроверенными.

[linear]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/00.TensorCore/Layers/Linear.cs#L41-L60
[transpose]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/00.TensorCore/Core/TensorOps.cs#L721-L779
[tensor]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/00.TensorCore/Core/Tensor.cs#L40-L48
[storage]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/00.TensorCore/Core/TensorStorage.cs#L20-L23
[lora-forward]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/10.LoRA/LoraLinear.cs#L112-L125
[attention]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/03.TransformerModel/Layers/CausalSelfAttention.cs#L138-L170
[ffn]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/03.TransformerModel/Layers/SwiGLUFeedForward.cs#L40-L51
[model]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/03.TransformerModel/LlmModel.cs#L219-L243
[sft]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/04.TrainingLoop/Trainer.cs#L62-L150
[dpo]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/11.DPO/DpoTrainer.cs#L878-L923
[bpe-train]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/01.Tokenizer/BpeTokenizer.cs#L42-L68
[bpe-merge]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/01.Tokenizer/BpeTokenizer.cs#L540-L565
[bpe-apply]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/01.Tokenizer/BpeTokenizer.cs#L568-L574
[knn-train]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L132-L178
[dpo-material]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/11.DPO/Models/DpoExactPairMaterial.cs#L64-L87
[p09]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P09-cached-forward.md#L67-L91
[p110]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P110-numeric-matrix-cost.md#L65-L95
[p17]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P17-tokenizer-refit.md
[p98]: https://github.com/lubochka/llm_mvp_core/blob/62bcc8f2368840e22d2ea98ef16d9bd526ad9a39/docs/plans/replanning-possible-diffs/experimets/cards/P98-tokenizer-capacity.md
[n21]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/21-INDEPENDENT-BPE-RETRAIN-VOCAB-DRIFT-2026-10-16.md
[n50]: https://github.com/lubochka/xiigen-planning/blob/fb9aadf69e0669d36b820d61f76fe3ecc8697a7d/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/50-LOCAL-WIKI-FEEDBACK-AND-BPE-BUDGET-2026-10-19/02-NEW-BPE-SEED-BUDGET.md

## Поздний срез перед публикацией

Исходные 577 planning / 286 protected Markdown относятся к закреплённому срезу выше. Поздняя адресная сверка root описана в [06-CONCURRENT-UPDATES.md](06-CONCURRENT-UPDATES.md): core `7c57183670ccaf554a47c0a4f44cabadde8c303d` сохраняет native исходники этих двух факторов; новые OCT20-R1, OCT20-R2 и E-OPT-STEP на planning `64aa3c75e378ddf32af517ab139f17dea1f0ed15` относятся к RequireValid comparison, BackwardLogProbs buffer и optimizer export. Они не дублируют выбранные transpose/no-match-copy вмешательства. Наша новизна остаётся ограниченной проверенным корпусом, включая эту адресную позднюю дельту.
