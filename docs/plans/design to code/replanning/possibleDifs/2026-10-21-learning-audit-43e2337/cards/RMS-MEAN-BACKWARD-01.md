# RMS-MEAN-BACKWARD-01 — цена обратного Mean в настоящем LoRA-обучении

**Повтор уже сохранённого результата запрещён.** Сначала прочитать назначенные receipts/outputs; пригодный результат использовать автоматически без вопроса «reuse или retrain». Отсутствие полного снимка не разрешает новый warm-up, перенос другого donor либо восстановление missing native moments/RNG по догадке. Consumed опыт не получает дополнительных model calls.

Дата: 21.10.2026. Исходный core43e2337d68dad0e4ab8d7fbb0cea38406b592b61. **Только документный кандидат. Runner и новое A/B ещё не созданы/не запущены; улучшение не измерено.**

| Поле реестра | Значение |
|---|---|
| ID | RMS-MEAN-BACKWARD-01 |
| Статус | DESIGNED / NOT_RUN / DEPENDS_ON_QUALIFIED_S0 |
| Тема | C# арифметика; знак целочисленной разности |
| Learning recipient | Native q/v-LoRA + AdamW, один Transformer block |
| Проверяемый фактор | Адресация последней оси в TensorOps.BroadcastGradAlongAxis |
| Перед началом | Учебные блокеры программы разобраны; RD01-S0 уже квалифицирован |
| Приоритет | После direct learning blockers и FILE-STATIC-RECEIVER-01; до ATTN-SOFTMAX-COPY-01 |
| Следующий ID | ATTN-SOFTMAX-COPY-01 по реестру; при поручении только этой карточки — END |
| Protected tree | docs/plans/replanning-possible-diffs — только чтение, никаких изменений |

[Общий контракт исполнения](../06-execution-contract.md) обязателен вместе с этой карточкой. Текущий документ не меняет очередь защищённого индекса. Порядок новой документной программы берётся из её собственного реестра по ID, а не по историческому номеру. Следующий ID — адрес следующего поручения, не разрешение выполнить его автоматически.

## Один вопрос опыта

Уменьшатся ли allocations настоящего SFT-update из уже обученного общего checkpoint, если заменить два массива координат на целочисленный адрес при last-axis Mean backward, сохранив приобретённые ответы и численное состояние?

## Доказанный участок: два массива на каждый элемент градиента

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

В экспериментальной копии `TensorOps` добавить `internal static bool ExperimentFastMeanBackward = false`. Program перед A0/B0 явно устанавливает false, перед четырьмя TRAIN calls — `arm == "B"`, после TRAIN сбрасывает false до readout/save. A никогда не включает фактор. Статическая переменная — только instrumentation host, не новая product option.

Только в экспериментальной копии `TensorOps.BroadcastGradAlongAxis` добавить ветвь, охраняемую `ExperimentFastMeanBackward`, для:
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

Окончательная ширина поиска старых документов должна браться из [dedup receipt](../evidence/learning-code-dedup.json), а не из числа строк этой таблицы. Наличие нового точного фактора не означает отсутствия близкого перефразированного предложения во всех ветках.

## Исполнение малого A/B после готового donor


### Реестр и зависимость

Статус: **DESIGNED / NOT_RUN / DEPENDS_ON_QUALIFIED_S0**. Назначать после решения приоритетных учебных блокеров и после получения квалифицированного общего состояния RD01. Source-only находка не получает `SUCCESS` и не добавляется как измеренное улучшение.

Реестр должен различать `depends_on=h64-rms-row/v1:COMPLETE_QUALIFIED_SFT_STEP400_FULL_STATE` и `next_id=ATTN-SOFTMAX-COPY-01`: зависимость не разрешает исполнителю запускать RD01 или продолжать уже остановленный опыт. Прямое указание Любы NEXT/STOP всегда немедленно прекращает прежнюю работу. Нельзя четырежды продолжать старую карточку, дописывать её необязательные файлы или выполнять новый train после NEXT.

### Общий обученный before: один назначенный donor

Единственный источник:
`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1`.

Полный donor contract обновлён по [core0cf3634/33](https://github.com/lubochka/llm_mvp_core/blob/0cf3634ce03d811498ec66e317e5a80ae84488e7/docs/plans/replanning-possible-diffs/experimets/33-shared-learned-starts.md): `S0.weights.bin`, `S0.optimizer.bin`, `S0.rng.json`, `preparation.json`, `generation.json`, `tokenizer.json`, `data.source.jsonl`, `data.material.json`, `authority.json`, `source-manifest.json`, `bundle.json`. Только COMPLETE/qualified bundle этой family допустим. Статус — `FUTURE_NOT_MATERIALIZED`; scripts — `DESIGNED_NOT_IMPLEMENTED_NOT_RUN`. Прежний `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/RD01/a001/results/common` — только original source назначенного экспорта, не запасной donor. Источник фактора остаётся pinned43e2337; все36 native files совместимы с0cf3634 по [проверке](../evidence/late-shared-start-delta.json).

**В этой новой карточке common TRAIN=0.** Не обучать ещё400 шагов, не брать другой checkpoint, не заменять восстановление одинаковым seed, не брать `RESTORE-MINIMAL-SET` только потому, что там есть файлы. Если named donor ещё не создан/не квалифицирован, эксперимент остаётся ожидающим конкретную зависимость; это не включает поручение consumer на Prepare/Export; общая подготовка остаётся отдельным назначением по контракту33. Никаких автоматических retry/a002/alternate donor.

До запуска arms автор проверяет receipt: `learning_qualified=true`, actual400 updates, точная config/data/stream и хотя бы один приобретённый **полный однотокенный ответ**: W0 false→S0 true на T1 или T2. Состояние с1/2 показывает приобретение named answer, но не превосходит constant-label1/2 и не доказывает общий закон знака. Оба learned IDs и ограничения обязаны быть в отчёте. Если нужен вывод о различении двух знаков, нужен2/2; исполнитель не увеличивает budget до этого результата.

Donor original source был закреплён на66947785d1318f7cbbc88118f3156cdc75001e16. **Все36 файлов его native source closure имеют те же Git blob SHA на текущем43e2337d.** Это проверено по Git tree metadata, перечень — [source closure36](../evidence/learning-code-source-closure.json). Проверка совместимости исходников не доказывает наличие donor bytes.

Private identity для обеих arms: `topologySha = lowercase SHA256(UTF8("RD01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj"))`; `loraConfigurationSha = lowercase SHA256(UTF8("RD01|alpha8|dropout0|seed62002|rms-row-reduce-probe-v1"))`. Без BOM и конечного LF. Не заменять префикс RD01 именем MB01/AC01 и не перемаркировать несовпадающий старый snapshot.

### Место, модель и запуск

- Единственный будущий host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MB01/a001`.
- Results: тот же host + `/results`; A/B raw: `results/A/code-sign.json`, `results/B/code-sign.json`; русский отчёт: `results/report.md`.
- Модель исполнителя: модель назначенной сессии по умолчанию, без override. Внешний teacher/judge не нужен.
- Student: V256/Context64/H64/L1/Q4/KV2/FF176/Rope10000/RmsEps1e-5; q/v-LoRA rank4/alpha8/dropout0; base seed62001, adapter seed62002, stream `rms-row-reduce-probe-v1`, **тот же stream donor**, не новая строка по ID этого опыта.
- Native source: ровно36 файлов собственного [source closure36](../evidence/learning-code-source-closure.json), current pin43e2337d. Получать bytes `git show <pin>:<path>` через argv без shell; проверить Git blob SHA до изменения копии. Только копия TensorOps получает фактор. Product files/старые результаты/protected docs read-only.
- `Runner.csproj`: net9.0 Exe, AssemblyName `RMS-MEAN-BACKWARD-01`, compile36 явных src files+`Program.cs`; external Directory.Build.props/targets выключены. Bin/obj только под MB01/a001, пути задаются до SDK imports. Без solution build/широких tests/установки иного SDK.
- Будущие файлы host: `Program.cs`, `Runner.csproj`, `run.py`, `src/`, `source-copy-manifest.json`, `one-factor.diff`. Этот аудит их не создаёт и не объявляет CLI проверенной.

Единственная будущая внешняя команда **после подготовки именно этого host и квалификации donor**:

```powershell
Set-Location 'C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/MB01/a001'
C:/Python314/python.exe ./run.py
```

Точный build-контракт `Runner.csproj`: свойства назначаются **до явного импорта Sdk.props**: `ImportDirectoryBuildProps=false`, `ImportDirectoryBuildTargets=false`, `EnableDefaultCompileItems=false`, `BaseIntermediateOutputPath=obj/`, `MSBuildProjectExtensionsPath=obj/`, `OutputPath=bin/`, `AppendTargetFrameworkToOutputPath=false`, `AppendRuntimeIdentifierToOutputPath=false`. После него объявить `TargetFramework=net9.0`, `OutputType=Exe`, `ImplicitUsings=enable`, AssemblyName из этой карточки, явные36 Compile+Program.cs и импорт Sdk.targets. Wrapper выполняет argv `dotnet build Runner.csproj -c Release --nologo --verbosity minimal` с cwd именно host; поэтому DLL лежит ровно в `bin/<AssemblyName>.dll`, как в командах ниже. Сборка не включает посторонние cs-файлы и production targets.

Runner без аргументов: фиксированный root от `__file__`; сначала root `execution.claim` create-new, затем results/create-new и одна Release build. После проверки donor и build выполнить последовательно:

```text
dotnet bin/RMS-MEAN-BACKWARD-01.dll A code-sign results/A/code-sign.json
dotnet bin/RMS-MEAN-BACKWARD-01.dll B code-sign results/B/code-sign.json
```

Ненулевой exit code A не разрешает начинать B; существующие claim/results/bin/obj не удаляются и не обходятся подбором другого aNN. Сохранить build.stdout/stderr и отдельные arm stdout/stderr. `execution.json`: стадия, issued/terminal, actual exit, complete/unknown counts; отсутствующее измерение=null, не0.

### Что точно делает Program

1. Проверить `bundle.json`: family=h64-rms-row/version1/status=COMPLETE, qualification и SHA256/length остальных10 файлов; сохранить read-only receipt. Сверить source/config/данные и фактические native веса/m/v/RNG. Ничего не писать обратно в bundle; дополнительных Forward/Train для этой проверки нет.
2. Создать policy через native constructor+ApplySeeded с указанной topology, затем загрузить `S0.weights.bin`. После Load повторно инициализировать adapter запрещено.
3. Сформировать `AdamWParameterBinding` из native `GetCheckpointParameterEntries()` и фактических attached q/v Parameter references, ordinal keys. Не придумывать имена по числу параметров. `AdamW(.9,.95,0,1e-8)`; `AdamWPortableState.FromBytes` и `TryImportPortableState` должны успешно восстановить step400 и все key/shape/m/v bits. Повторный Export сравнить с прочитанным portable snapshot.
4. `S0.rng.json` читать `JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>`. Это private JSON полного native snapshot, не файл native DPO writer. `RestoreRandomState(snapshot,new LlmModelRandomStateIdentity(topologySha,loraConfigurationSha))`; Capture сверяет все stream/cursor/seed/step fields. Dropout0 не разрешает скрытую замену topology.
5. Сверить все загруженные named weight bits/shapes, frozen hash, optimizer и RNG. A0/B0 должны точно совпадать. Создать `Trainer` с Warmup0, MaxLr=MinLr.001, TotalSteps404, Batch1/Seq64/GradAccum1/Clip1. Новый счётчик Trainer0 допустим здесь только из-за constantLR; native AdamW остаётся400.
6. Перед TRAIN снять одним и тем же Observer панель T1,T2,E1,E2, training:false, prompt-only last logits. Затем **четыре** native `TrainSupervisedNextTokenStep`: T1,T2,T1,T2, weight1. После них снять ту же панель. B-factor включён только для B TRAIN; readout forward не содержит нового backward.
7. Из четырёх real updates сохранить online loss, все post-update named weights, optimizer step404, все m/v bits и статистику затрат. Сохранить zero/finiteness/changed-state flags; числовые прочтения после TRAIN не добавляют forward/backward.
8. Observer: UTF-8 bytes Prompt (без Answer), `model.Forward(training:false)`, последние256 logits; predicted ID = максимальный logit, ties=min token ID; gold rank=1+число logits строго выше gold. NLL logsumexp считать одинаковым методом из одних logits A/B. `exact = predicted == byte(Answer)`. Full answer здесь1 byte, не первый токен длинного текста.

### Данные, scope и бюджет

| Split | ID | Prompt буквально | Answer |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Тема — C# арифметика, задача — знак разности. Это не уроки разных предметных доменов; два знака — два класса одной задачи. TRAIN целиком `Prompt+Answer`, без newline/BOS/EOS; UTF-8 byte IDs0…255, BPE Train/Encode не вызываются. Native objective остаётся full-sequence SFT. Тождественная/переименованная схема EXAM ограничивает перенос: не доказывать общий арифметический навык.

Полный новый runtime budget после готового donor: **8 optimizer updates / 8 training forward / 8 backward / 16 diagnostic forward**, всего24model forward. На каждом arm4 updates,8 readouts. Common retraining0, Generate0, DPO0, teacher0, finite-difference0, warmup0, retries0, дополнительные seeds0. Зафиксированный общий deadline600s: build≤180s, каждое arm≤120s, чтение/summary≤60s, оставшееся — подготовка. Это operational budget, не гарантия успешной сборки/обучения в любой среде.

### Метрики, успех и регрессии

Измеряется стоимость четырёх **native TRAIN calls**, включая forward/backward/clipping/AdamW. Build, restore, readout, сериализация и hashes вне cost timer. Primary resource — `GC.GetAllocatedBytesForCurrentThread`; это текущий managed thread, не общий RAM. Также elapsed Stopwatch, Process CPU с Refresh, GC0/1/2, peak working set процесса (отдельно от allocated bytes).

Сохранить A0→A1, B0→B1 и difference-in-differences на **одинаковых** readout rows; не сравнивать online loss разных уроков вместо before/after. Отдельно TRAIN exact/NLL/rank, EXAM exact/NLL/rank; learned answer retention. Целевой контроль эквивалентности — побитовые A/B конечные weights, m/v, step и panel logits, finite values; если есть расхождение, сохранить значения, не округлять его доPASS. Материальная регрессия одной оси не скрывает измеренное улучшение другой, но ограничивает статус семантической нейтральности.

**Любое измеренное улучшение полезной метрики в валидном учебном сравнении считается успехом именно этой метрики/темы; роста accuracy не требуется.** Никакого обязательного5%/10% порога. Если A/B incomplete или learned before не доказан — обучение невалидно, cost cannot close learning objective. Wall time одного фиксированного A→B чувствителен к порядку/JIT/load; не выдавать его за устойчивое ускорение всех моделей. Additional rerun не следует автоматически из шума.

### Ожидание, отчёт и остановка

До launch обнаружить реальный tool launch и wait/resume в текущем каталоге и прочитать схему. Одна выдача entry, затем same-handle bounded blocking/event wait до terminal или исходного deadline. Yield/RPC timeout, отсутствие output и пустой файл не доказывают остановку. Не заканчивать ход со словами «жду, сообщу потом», не перезапускать entry, не заводить background monitor/watchdog/summary script и не использовать быстрый polling вместо ожидания. При противоречии tool responses проверить именно исходный handle/PID+start и известные raw; неизвестные updates оставитьnull. Руководство: [docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md](https://github.com/lubochka/llm_mvp_core/blob/43e2337d68dad0e4ab8d7fbb0cea38406b592b61/docs/plans/replanning-possible-diffs/experimets/31-tool-discovery-and-wait.md). В среде с `exec` running cell используется wait того же cell; имя чужого Kiro tool не выдумывать.

STOP/NEXT Любы — немедленно: не продолжать прежний эксперимент ради отчёта, косметики, проверки или публикации. После обычного завершения один русский `results/report.md`: «Что изменили»; «Обученный before»; «A0/A1/B0/B1»; «Цена TRAIN»; «Улучшения и ухудшения»; «Границы и кандидат». При полезном измеренном эффекте создать `results/candidate.md` с оригинальным методом, единственным diff, метрикой, темой, ограничениями и ссылками raw. Решение о переносе в product остаётся за Любой; ни production patch, ни исправление product tests/CI не входит в этот опыт.

Skills будущего исполнителя закреплены на core snapshot: `docs/skills/architect-behavior-classifier-SKILL.md`, `planning-session-startup-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `capability-measurement-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `output-readability-gate-SKILL.md`, `human-communication-GLOSSARY.md`. Прямые пользовательские правила о малом опыте, любой полезной метрике и отсутствии prod changes имеют приоритет над историческими thresholds/full production pipeline.


Итоговый `results/result.json` объединяет уже сохранённые A/B числовые arrays, hashes и actual counters; для него запрещены новые model calls. Отчёт `results/report.md` — до700слов плюс таблица, язык русский; отдельный `results/candidate.md` только при полезном измеренном эффекте.

Сравнение0→0 правильных учебных ответов не является валидным учебным результатом: сдвиг margin/loss его не исправляет. При отсутствии назначенного qualified before — остановка до TRAIN; никакого самостоятельного добора уроков, нового warm-up или другого donor.
