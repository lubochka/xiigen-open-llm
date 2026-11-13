# Согласование peer-пакета eaed: два самостоятельных фактора и единая очередь40

Дата: 20 октября 2026. Это документная сверка; native сборки, эксперименты, обучение и тесты не запускались. Изменены только файлы этого отчёта в `work/`. Protected, production и опубликованные peer-документы не изменены.

**Результат:** GRAD-ACCUM-COPY-01 и FROZEN-BROADCAST-MAP-01 не дублируют LF-03, GRADB-LOCALITY-01, RMS-COORD-ALLOC-01, ATTN-TRANSPOSE-STRIDES-01 или JSON-OPTIONS-REUSE-01. В обоих реестрах38 ID; пересечение36; объединение **40 различных ID**. Это вычисление по буквальным ID, дополненное сравнением конкретных методов и B-diff, а не суммирование названий похожих оптимизаций.

Источники: planning peer [eaed843a][peer-registry], parent `53cbcabd3926cda714bab428d294eb666301e9e4`; исходный peer статус core `827a1acdbef95d220886e54f9d36bce0b405c8cd`. Все10 прочитанных peer-файлов побайтно совпали с Git blob SHA; receipts и полный merged список40 находятся в `planning-eaed-peer-delta.json`.

## 1. Дедупликация по месту изменения

| ID | Точный фактор | Почему это другой эксперимент |
|---|---|---|
| LF-03 | `Tensor.Backward(Tensor initialGradient)`: удалить внешний `initialGradient.Data.ToArray()`, сохранить defensive copy TensorStorage | Копирование **начального** logits-gradient перед обходом графа; в назначенном DPO два policy backward. Не изменяет приём вкладов промежуточными узлами |
| GRAD-ACCUM-COPY-01 | `TensorOps.AccumulateGrad`, ветвь `target.Grad == null`: `new Tensor(gradToAdd.Data.ToArray(),shape)` → `new Tensor(gradToAdd.Data,shape)` | Копирование первого вклада **в каждом достигнутом producer**, включая промежуточные узлы. Начальный Backward остаётся исходным. Фактор и его число достижений не совпадают с LF-03 |
| GRADB-LOCALITY-01 | `TensorOps.MatMul`, правый adjoint: порядок циклов вычисления `AᵀG` | Меняет локальность скалярного вычисления правого градиента; сохраняет массив результата и AccumulateGrad. Не убирает ни одну из указанных копий |
| RMS-COORD-ALLOC-01 | Устранить per-element `FlatToIndices` coordinate arrays | Старый фактор сохраняет mappedBIndices; это другой массив, guard и lifetime |
| FROZEN-BROADCAST-MAP-01 | `TensorOps.MultiplyBroadcast`: при обоих `RequiresGrad=false` не создавать/заполнять `mappedBIndices` | Ненужное backward-отображение замороженного forward. `FlatToIndices`, float multiply, grad-ветвь и storage copy остаются исходными; не RMS-COORD |
| ATTN-TRANSPOSE-STRIDES-01 | `TensorOps.Transpose`: rank4 axes1↔2, прямая арифметика смещений вместо coordinate arrays | Другой оператор, guard и actual attention forward/backward. Frozen broadcast map не трогает |
| JSON-OPTIONS-REUSE-01 | `ExecutorUnit.TrainOnExample`: lifetime JsonSerializerOptions с тем же converter | Native EN→RU memory training; TensorCore/autograd не меняет |

Доказательства peer: [GRAD diff и границы L9–27][grad-factor], [его сравнение LF-03 L57–68][grad-prior], [FROZEN diff L9–41][frozen-factor], [его сравнение RMS L73–85][frozen-prior]. Исходный [AccumulateGrad L984–997][accum-source] дополнительно прочитан локально на current-source snapshot; строка соответствует peer. [Protected LF-03 L331–353][lf03] явно меняет другой метод. [GRADB L40–57][gradb] прямо сохраняет AccumulateGrad и исключает LF-03.

**Граница вывода:** подтверждено отсутствие exact duplicate среди названных семейств и сравниваемых пакетов. Это не утверждение абсолютной новизны во всех ветках/разговорах. GRAD-U не является отдельно установленным ID; дополнительного запуска или псевдоalias на это имя не создаётся.

## 2. Точные passports двух peer-факторов

Обе карточки и их inline prompts прочитаны; общий [tensor-договор][tensor-contract] является обязательной частью каждой. **Статус обоих: DESIGNED_NOT_RUN, runner/source-copy ещё не созданы.** Наличие строки команды не означает существующего или проверенного launcher.

| Поле | GRAD-ACCUM-COPY-01 | FROZEN-BROADCAST-MAP-01 |
|---|---|---|
| Physical root | `C:/xg/AGC1` | `C:/xg/FBM1` |
| Source | `C:/xg/AGC1/src` | `C:/xg/FBM1/src` |
| Host | `C:/xg/AGC1/host` | `C:/xg/FBM1/host` |
| Project | `C:/xg/AGC1/host/Runner.csproj` | `C:/xg/FBM1/host/Runner.csproj` |
| Results | `C:/xg/AGC1/results` | `C:/xg/FBM1/results` |
| Report | `C:/xg/AGC1/results/report.md` | `C:/xg/FBM1/results/report.md` |
| Candidate | `C:/xg/AGC1/results/candidate.md` | `C:/xg/FBM1/results/candidate.md` |
| Единственный entry после подготовки | `pwsh -NoProfile -File 'C:/xg/AGC1/host/Run.ps1'` | `pwsh -NoProfile -File 'C:/xg/FBM1/host/Run.ps1'` |
| Claim до build | `C:/xg/AGC1/host/execution.claim` | `C:/xg/FBM1/host/execution.claim` |
| Arm outputs | `results/A`, `results/B` под AGC1 | `results/A`, `results/B` под FBM1 |
| Arm claim до load/Train | `results/A/training.claim`, `results/B/training.claim` | Те же относительные пути под FBM1 |
| Card/prompt | [карточка][grad-card], [inline prompt L53–55][grad-prompt] | [карточка][frozen-card], [inline prompt L69–71][frozen-prompt] |

Source pin обоих — `883496bca18113bf37e0afe683c75b05a0075048`, не latest HEAD. По peer final827 tensor blobs unchanged883→669→827: TensorOps `e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b`, TensorStorage `22aec3ba03b756530001ae786acbdce28af90104`. Здесь это pin/receipt peer, не повторная аттестация всех binaries. [Final827 L40–44][peer-latest]

Состав: полные source-copy TensorCore/TrainingLoop/DPO/ContinuousLearning, thin executable `TensorLearningRunner`, bridge внутри ContinuousLearning; `src/A/TensorOps.cs` и `src/B/TensorOps.cs` вместо common TensorOps. Tensor.cs не менять. `net9.0`, explicit compile list, `Variant=A|B`, без product ProjectReference. Точные26 внешних DLL перечислены в tensor-договоре L63–75; ни product dependency pack, ни полный35.ExperimentsV2 автоматически не пересобирать.

Run.ps1 сначала фиксирует свой working directory, проверяет отсутствие results и create-new host claim. Затем build A, build B, единственный owner reserves study; процессы A→B:
`dotnet host/bin/A/Release/net9.0/TensorLearningRunner.dll --arm A <absolute-results/A>`, аналогично B. Nonzero A прекращает chain, B автоматически не начинается. Существующие claims/results не удаляются и не обходятся новым a002. Статус missing B0/ABI/build до learner — «обучение не началось». [Точная chain L105–118][tensor-run]

### Student, data и бюджет

Student: `rp01-csharp-small/v1`, vocab629/context2048/H512/4 layers/8 heads/2 KV/FFN1408; q/v-LoRA rank8 alpha16 dropout0,16 trainable tensors. Policy/reference загружаются независимо из canonical B0, reference frozen; fresh AdamW m/v/step0, root seed20261014. DPO beta0.1, LR1e-5, AdamW beta1=.9/beta2=.95/weightDecay=.01/epsilon1e-8. Source governance/lease/materializer/recipient/rollback сохраняются настоящими. [Tensor §2–5][tensor-contract]

Одна неизменная fixture `simulation-csharp-doc-f06-008`: postfix `int x=10; int y=x++; System.Console.Write(y);`; chosen «It writes10…old value», rejected «It writes11…new value» — точные bytes существующего JSONL, без пересериализации; `csharp_runtime_verified=false` остаётся. B0 loader и четыре pins перечислены в JSON metadata и contract L35–44.

**На каждый отдельный ID:** A1update+B1update =2 StepVector/2 optimizer calls,4 policy TRAIN forwards,4 reference forwards,4 policy backwards. Отдельно4 DIAG_TRAIN policy forwards, по одному на A0/A1/B0/B1. Generate/EXAM/teacher/retry/warmup=0. Эти бюджеты не складываются с LF-03, ATTN или JSON внутри одного B.

Из той же chosen TRAIN panel: target NLL, full-vocab gold rank, token top1 и whole teacher-forced exact. Response/attention masks и IDs неизменны. `ΔA=A1−A0`, `ΔB=B1−B0`, рядом difference-in-differences. StepVector return помечается PRE_UPDATE, не выдаётся за A1/B1. Native post-update DPO margin/loss не добавляются и отмечаются «не измерено».

Peer gold rank =1+число строго больших logits; ties не повышают rank, argmax выбирает меньший ID. Это явно объявленный competition rank и **не тот же показатель**, что ordinal rank с lower-ID tie-break в нашем ATTN. Сам peer предупреждает, что rank1 при tie не доказывает exact. Нельзя склеивать эти rank-колонки как одинаковую метрику; менять source-рецепт для согласования без отдельной причины не требуется. [Tensor L93–103][tensor-metrics]

Resource counters — полный успешный StepVector отдельно от load, DIAG, оформления; publication внутри StepVector не исключается. CPU/wall/allocated/GC, managed heap/RSS/whole-process peak различаются. Любое измеренное полезное улучшение остаётся успехом своей оси/домена, рядом все регрессии. A→B без repeats не устанавливает причинную долю wall/CPU. State mismatch сохраняет ресурсное наблюдение, но не подтверждает эквивалентность.

## 3. Merged очередь и обязательный NEXT override

Счёт проверен: peer38, наш38, common36, peer-only2, ours-only2. Первые34 совпадают **в том же порядке**, последние2 общие readers также совпадают. Назначенное root объединение:

| Позиция | ID | NEXT |
|---:|---|---|
| 01–34 | Прежние36 core ID без двух последних readers; исходный порядок сохранён | Строка34 BPE-REALIZED → GRAD |
| 35 | GRAD-ACCUM-COPY-01 | FROZEN-BROADCAST-MAP-01 |
| 36 | FROZEN-BROADCAST-MAP-01 | **ATTN-TRANSPOSE-STRIDES-01** |
| 37 | ATTN-TRANSPOSE-STRIDES-01 | JSON-OPTIONS-REUSE-01 |
| 38 | JSON-OPTIONS-REUSE-01 | IDENTITY-M1-01 |
| 39 | IDENTITY-M1-01 | KN-ROUTER-LEARNED-KEY-01 |
| 40 | KN-ROUTER-LEARNED-KEY-01 | END |

**Обязательное дополнительное правило:** старое peer `FROZEN NEXT=IDENTITY` в его card, inline prompt и tensor-contract отменяется **только для объединённой программы**. Effective NEXT после FROZEN здесь =ATTN. Остальные source paths, data и бюджет FROZEN прежние; peer-файлы сохраняются как исторические документы без переписывания.

На peer core827 было6 completed/32 pending при38ID; при union40 это6/34. **Root сообщил последующее завершение LF-03 и назначил latest-status override:7completed/33pending, NEXT=N63-INGRESS-LENGTH.** Сведения о позднем LF запуске в этом узком peer-аудите не перечитывались и не выдаются за собственный fresh readback. JSON разделяет source827 и root status override. Root связывает поздний статус со своими actual LF receipts; старый peer NEXT=LF-03 не разрешает повтор.

## 4. History02/03/07: согласование, без новых запусков

**HIST-E07-GLOBAL-01** — условный новый кодовый случай: `_trans/_bigram` накапливаются между Train, `globalNextCounts` локален, fallback определяется последним batch. Однако проверенный E07 создаёт объект и обучает один раз; repeated-Train caller для RP01 не установлен. Поэтому правильный статус — вне очереди, не объяснение опубликованной E07/RP01 неудачи. Его типизированный BpeId fixture не является измеренным C# BPE/Roslyn quality. Это согласуется с нашим разграничением исторического наблюдения, actual learning и недоказанной matrix→KNN причинности. Условный ID не добавляется41-м. [History02 L5–27,35–54][hist02]

**P54/NG01.** History03 — исторический883 receipt: P54 семь уже известных reader подслучаев, не семь новых гипотез; чтение learned state может законно влиять на результат/оценку обучения без искусственного дополнительного writer. Но конкретные learned checkpoint/inputs/host для семи не закреплены. Peer header/latest827 явно сообщает, что blanket ban уже снят669 и NG01 protected recipe появился. Не переносить старый текст «ban действует / NG01 отсутствует» в текущий вывод. NG01 остаётся одним exact int-relational-pattern label фактором внутри известной N48-семьи, не дубль generic receiver C2 и не второй запуск. Это соответствует нашему current audit. [History03 L3–13,19–52][hist03], [Latest827 L40–44][peer-latest]

**History07 output назначения** совпадают с нашим827 mapping: AST01 results/attempt-0001/**candidate-fix.md** и Run-Assigned; BR01 r/0001/**candidate.md** и Run-Assigned; ROPE RPE01/a001/results/**candidate.md**, Python314 `./run.py`. У ROPE peer исправил свою старую ошибку проекции, source assignment не менялся. NG01 current protected host/output669 сохранились827; менялись owner/queue абзацы. Никакой из этих переносов не даёт право удалить legacy claim или повторить завершённое. [History07 L13–66][hist07]

**E-OPT attribution:** peer latest сохраняет measured−467944 allocated bytes и GC улучшения, рядом peak+204546048; raw labels всё ещё R1-style. Это согласуется с нашим provenance-addendum: labels conflict не доказывает отсутствие обучения или исполнение именноR1. Наш дополнительный source/binary разбор не должен исчезнуть при импорте peer; status remains completed/candidate, без повторного запуска ради красивого raw. [Latest827 L5–25][peer-latest]

## 5. Граница завершения

Никаких новых измеренных побед эти2 карточки не содержат; оба — кандидаты на будущий опыт. Source-code доказательство достижимого обучающего участка не заменяет measured outcome. Для текущей публикации достаточно сохранить peer источники,40ID union,33pending latest override, exact outputs и FROZEN→ATTN. Исторические условные варианты не раздувают активную очередь.

[peer-registry]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/04-execution-registry.md#L3-L48
[grad-card]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/GRAD-ACCUM-COPY-01.md
[grad-factor]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/GRAD-ACCUM-COPY-01.md#L9-L27
[grad-prior]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/GRAD-ACCUM-COPY-01.md#L57-L68
[grad-prompt]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/GRAD-ACCUM-COPY-01.md#L53-L55
[frozen-card]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/FROZEN-BROADCAST-MAP-01.md
[frozen-factor]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/FROZEN-BROADCAST-MAP-01.md#L9-L41
[frozen-prior]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/FROZEN-BROADCAST-MAP-01.md#L73-L85
[frozen-prompt]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/FROZEN-BROADCAST-MAP-01.md#L69-L71
[tensor-contract]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/TENSOR-LEARNING-EXECUTION-CONTRACT.md
[tensor-run]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/TENSOR-LEARNING-EXECUTION-CONTRACT.md#L105-L118
[tensor-metrics]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/learning/TENSOR-LEARNING-EXECUTION-CONTRACT.md#L93-L103
[accum-source]: https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/00.TensorCore/Core/TensorOps.cs#L984-L997
[lf03]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L331-L353
[gradb]: https://github.com/lubochka/llm_mvp_core/blob/827a1acdbef95d220886e54f9d36bce0b405c8cd/docs/plans/replanning-possible-diffs/experimets/cards/GRADB-LOCALITY-01.md#L40-L57
[hist02]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/history/02-conditional-ngram-continuation.md
[hist03]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/history/03-p54-ng01-semantic-verdict.md
[hist07]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/history/07-core827-output-delta.md
[peer-latest]: https://github.com/lubochka/xiigen-planning/blob/eaed843ad2b063fbb003f073deb69da19f0cb135/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments/08-final-core827.md
