# Проверка нейронного тракта: новые кандидаты и границы доказательств

> **Позднее уточнение источников:** основной аудит ниже относится к core `43e2337`. После его публикации появился `0cf3634`: LC-REF-CACHE уже имеет сохранённые результаты, общий реестр содержит53 ID, а для S0 назначены canonical shared families. Для текущего статуса и путей приоритет имеет [дополнение09](09-late-core-0cf3634-update.md); прежние выводы оставлены как датированный срез, а не текущая инструкция повторять опыт.

Дата: 21.10.2026. Core snapshot: `43e2337d68dad0e4ab8d7fbb0cea38406b592b61`. Результат этой работы — чтение источников и проектирование ограниченного опыта; native сборок, обучения и новых измеренных результатов **нет**. Production и защищённая папка `docs/plans/replanning-possible-diffs` не менялись.

## Вывод

**Один сильный новый ресурсный кандидат: лишние массивы координат в обратном проходе RMSNorm.** Участок прямо участвует в SFT/LoRA и DPO/LoRA, поэтому это допустимый учебный эксперимент. Возможная польза — меньше выделений памяти на настоящий update. Из источника не следует улучшение точности, и эта находка сама по себе не объясняет наблюдавшуюся потерю предметного качества.

Второй, менее сильный кандидат — лишний начальный проход копирования внутри `TensorOps.Softmax`. Он не уменьшает число выделенных массивов и требует измерения времени внутри обучения; на малой модели эффект может не обнаружиться. Его следует держать ниже первого кандидата, без автоматического дополнительного запуска.

Новых подтверждённых дефектов качества DPO/LoRA/restore по проверенному тракту не найдено. Ранее описанные разрывы градиента, SFT/DPO collision, состояние оптимизатора, EOS, vector scalar weighting и token framing не переименованы в новые открытия.

## 1. RMS-MEAN-BACKWARD-01: два массива на каждый элемент градиента

### Что подтверждено источником

| Звено | Точный источник | Что он доказывает |
|---|---|---|
| Реальный native learner | [04.TrainingLoop/Trainer.cs L102-145](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/04.TrainingLoop/Trainer.cs#L102-L145) | `Forward(training:true) → BackwardLoss → ClipGradNorm → AdamW.Step`; фактор попадает внутрь настоящего обучения |
| Model owner | [03.TransformerModel/LlmModel.cs L219-241](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/03.TransformerModel/LlmModel.cs#L219-L241) | Блоки модели и завершающий `_finalNorm` стоят перед `_lmHead` в том же graph |
| Нормализация после обучаемого attention | [03.TransformerModel/TransformerBlock.cs L46-64](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/03.TransformerModel/TransformerBlock.cs#L46-L64) | `norm2` получает residual после attention с q/v-LoRA |
| Нужная форма Mean | [03.TransformerModel/Layers/RmsNorm.cs L29-40](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/03.TransformerModel/Layers/RmsNorm.cs#L29-L40) | `Pow(x,2) → Mean(axis:-1,keepDims:true)`; это последняя сохранённая ось |
| Обратный вызов Mean | [00.TensorCore/Core/TensorOps.cs L569-582](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/00.TensorCore/Core/TensorOps.cs#L569-L582) | `BroadcastGradAlongAxis` вызывается только при `x.RequiresGrad`, с прежним `1.0f / axisSize` |
| Источник двух массивов | [00.TensorCore/Core/TensorOps.cs L1008-1017](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/00.TensorCore/Core/TensorOps.cs#L1008-L1017); [00.TensorCore/Core/TensorOps.cs L1049-1060](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/00.TensorCore/Core/TensorOps.cs#L1049-L1060) | Для каждого srcFlat создаётся новый `int[rank]`, затем его `ToArray()`; обе копии используются только для вычисления адреса |
| Layout/dtype | [00.TensorCore/Core/TensorShape.cs L25-58](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/00.TensorCore/Core/TensorShape.cs#L25-L58); [00.TensorCore/Core/Tensor.cs L15-16](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/00.TensorCore/Core/Tensor.cs#L15-L16) | Непрерывный row-major layout и `float[]`; последний stride равен1 |
| Связь с LoRA | [10.LoRA/LoraLinear.cs L112-125](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/10.LoRA/LoraLinear.cs#L112-L125); [03.TransformerModel/Layers/CausalSelfAttention.cs L130-168](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/03.TransformerModel/Layers/CausalSelfAttention.cs#L130-L168) | Native q/v adapter находится в том же forward; input-gradient после attention требуется даже при frozen base |

У малой назначенной модели `B1,T21,D64,L1` (22 UTF-8 byte IDs дают21 next-token input) две Mean-ноды имеют backward: `norm2` и `finalNorm`. Первый `norm1` до q/v при frozen embeddings/base не объявляется trainable. Таким образом, источник предписывает **2 × 1 × 21 × 64 × 2 = 5 376 массивов int[3] на один update** в этом участке; за4 updates одного плеча —21 504. Это аналитический подсчёт создаваемых объектов, **не измерение CLR allocations/bytes/time**. Фактическое число вызовов и стоимость фиксируются в настоящем обучении.

### Предлагаемое единственное изменение

Только в экспериментальной копии `TensorOps.BroadcastGradAlongAxis` добавить ветвь для:
`axis == gradInShape.Rank - 1`, одинакового rank входа/выхода, последнего размера `gradOutShape == 1` и одинаковых предыдущих размеров.

```csharp
int axisSize = gradInShape.Dimensions[axis];
for (int srcFlat = 0; srcFlat < gradInShape.TotalElements; srcFlat++)
    gradIn[srcFlat] = gradOut[srcFlat / axisSize] * scale;
return;
```

Ветвь A выполняет исходный код. Ветвь B выполняет этот адресный shortcut; для всех остальных форм — исходный fallback. `scale`, порядок scalar multiply, `AccumulateGrad`, Tensor copies и численная цепь не меняются. При last-axis keepDims=true прежний многомерный адрес строго равен `srcFlat / axisSize`; здесь нет sum/reassociation. Побитовое совпадение учебного state ожидается из формулы, но не объявлено измеренным.

Нельзя попутно менять `SumAlongAxis`, `MultiplyBroadcast`, `Pow`, `RSqrt`, кэш transpose, AdamW, маску SFT, набор и dtype. Нельзя запускать standalone Mean benchmark вместо native updates.

### Почему это отдельная гипотеза

| Предыдущая работа | Отличие |
|---|---|
| [docs/plans/replanning-possible-diffs/experimets/cards/RMS-ROW-REDUCE-01.md](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/experimets/cards/RMS-ROW-REDUCE-01.md) | Меняет **forward `SumAlongAxis`**. Её §3 прямо требует не менять `BroadcastGradAlongAxis`. В текущем новом A/B эта оптимизация не включается |
| [docs/plans/replanning-possible-diffs/experimets/cards/RMS-COORD-ALLOC-01.md](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/experimets/cards/RMS-COORD-ALLOC-01.md) | Меняет координатную адресацию **forward `MultiplyBroadcast`**, не обратный Mean |
| [docs/plans/replanning-possible-diffs/experimets/cards/FROZEN-BROADCAST-MAP-01.md](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/experimets/cards/FROZEN-BROADCAST-MAP-01.md) | Не строит mapping при обоих frozen inputs. Здесь input действительно требует gradient |
| [docs/plans/replanning-possible-diffs/experimets/cards/GRAD-ACCUM-COPY-01.md](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/experimets/cards/GRAD-ACCUM-COPY-01.md) | Изменяет владение первой gradient copy, не адресацию Mean |
| [docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/12-additions-wiki-sql-knn-tensor.md L222-235](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/12-additions-wiki-sql-knn-tensor.md#L222-L235) | Известная correctness-проблема `Mean(keepDims:false)` без найденного RP01 caller. Здесь `keepDims:true`, живой caller и ресурсный фактор |

Финальный поиск охватил994 Markdown possibleDifs и374 защищённых Markdown (1368/1368 original Git blobs проверены), точные запросы — в [dedup receipt](evidence/learning-code-dedup.json). Это полнотекстовый поиск, не1368ручных review. Наличие нового точного фактора не означает отсутствия близкого перефразированного предложения во всех ветках.

## 2. Исполнительские рецепты

Окончательные рецепты после независимого review находятся в [RMS-MEAN-BACKWARD-01](cards/RMS-MEAN-BACKWARD-01.md) и [ATTN-SOFTMAX-COPY-01](cards/ATTN-SOFTMAX-COPY-01.md). Они назначают явный runtime switch, exact build/output paths, один qualified RD01 donor и по24forward/8updates без common retraining. Эти карточки, общий контракт и registry имеют приоритет над ранними черновыми именами host/следующего ID. Обученных новых результатов нет.

## 3. Второй кандидат: ATTN-SOFTMAX-COPY-01

**Статус: LOW_PRIORITY_DESIGNED / NOT_RUN / DEPENDS_ON_QUALIFIED_S0; самостоятельная карточка описана, runtime не назначен.** Не выдавать второй runtime entry автоматически.

[00.TensorCore/Core/TensorOps.cs L793-848](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/00.TensorCore/Core/TensorOps.cs#L793-L848) сначала копирует каждый x.Data элемент в resultData (797–802), затем читает resultData при max scan и exponentiation. `CausalSelfAttention.ScaledDotProduct` вызывает этот TensorOps.Softmax в обычном attention forward. Это не `DpoTrainer.BackwardLogProbs` и не N62-R2 scratch.

Одно предполагаемое изменение: удалить copy-loop, max scan читать `x.Data[idx]`, exponentiation писать `resultData[idx]=MathF.Exp(x.Data[idx]-maxVal)`. Сохранить shape/index iteration, expSum order, final normalization, native backward, массив результата и Tensor defensive copy. Existing cancellation checks в max/exp/normalize сохранить; для общего метода отдельно учесть cadence, не объявлять concurrency/cancellation-equivalence доказанной по математике. В назначенном synchronous SFT нет конкурентной мутации x.

Source-level экономия — один read+write проход по attention scores `B×heads×T×T`; не сокращение количества float[] и не заявленное уменьшение allocated bytes. В tiny H64/L1/T21 это1 764 elements на forward, что может быть незаметно на фоне matmul. Гипотеза: уменьшение CPU/elapsed native update без изменения learned state. При отсутствующем measured gain — честный NO_OBSERVED_GAIN, а не запуск большего бюджета по инициативе исполнителя.

При будущем назначении этой карточки использовать тот же конкретный квалифицированный RD01 donor, **самостоятельные** A/B из S0, тот же24forward/8update recipe, иной отдельный host `AC01/a001`, фактор RMS выключен. Нельзя сравнивать её с уже обученным B после RMS, потому что тогда меняется starting state. После RMS-MEAN-BACKWARD-01 следует ATTN-SOFTMAX-COPY-01, затем END. Отдельные готовые документные карточки сохранены рядом; их порядок не разрешает автоматическое исполнение.

Отличие от [docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md) / N62-R2: там один локальный exp scratch в DPO backward, здесь устранение входного copy-pass в general attention softmax forward. Прямое ухудшение accuracy данным источником не доказано.

## 4. Не выдавать известные или неподходящие риски за новые результаты

| Проверенный lead | Решение |
|---|---|
| Повторный warmup SFT каждую сессию | Отозвано: current `IncrementalTrainingSession.Build` явно задаёт WarmupSteps=0; нельзя приписывать этот failure |
| Fresh AdamW / scheduler при новой session | Известно в SFT-state/restore документах; новая независимая находка отсутствует |
| `Mean(keepDims:false)` / MatMul необычные broadcast формы | Для текущего RP01 нужный caller не найден; не запускать synthetic-only learner ради активации |
| Нечётный RoPE head dimension | Conditional source-risk; не найден actual профиль с odd HeadDim. Не записывать причиной текущего обучения |
| Дробная response mask DPO | Current backward умножает на mask; исторический mismatch нельзя переносить на текущий код |
| Frozen base через LoRA якобы разрывает градиент | Source contra-evidence: propagation к adapter leaves сохранён; математическая композиция не объявляется численно проверенной без runtime |
| Translation phrase index / unknown tokens / static rule priority | Известно из старого корпуса; parent ведёт отдельную предметную ветку, здесь не повторяется |
| RESTORE-MINIMAL-SET как доказанный full-answer donor | Raw показывает TRAIN correct tokens3/40, full exact responses0/2, EXAM0. Не подходит для заявлений о ненулевом полном ответе |

Последний вывод опирается на [experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json): weights restored bit-exact и optimizer step1000 действительно заявлены raw, но это не превращает `TrainRowsWithAnyCorrectResponseToken=2` в две правильные программы. После200 продолжений NLL улучшается на TRAIN, CONTROL NLL ухудшается5.807963→5.857194; все эти оси должны оставаться раздельными.

## Проверка полноты этого пакета

Файлы доказательств: [source evidence](evidence/learning-code-evidence.json), [source closure](evidence/learning-code-source-closure.json), [dedup receipt](evidence/learning-code-dedup.json). Новизна ограничена прочитанным и полнотекстово проверенным corpus snapshot; метаданные SHA позволяют восстановить точные исходники. Никакой source-level или аналитический факт не назван результатом native обучения.

