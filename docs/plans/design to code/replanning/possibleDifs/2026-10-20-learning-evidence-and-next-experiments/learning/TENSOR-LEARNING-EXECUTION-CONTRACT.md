# Точный договор для двух дополнительных tensor-экспериментов

Это спецификация подготовки, **не созданный и не проверенный runner**. Текущая работа документальная. Будущий исполнитель получает одну конкретную карточку, этот договор и её встроенный промпт; другой проект, модель, данные или место результатов выбирать не нужно.

## 1. Порядок, модель и немедленный переход

Запуск задаёт [единый реестр пакета](../04-execution-registry.md), без отдельной конкурирующей очереди этих файлов. Текущий следующий — LF-03, исходный №07 в [protected-индексе][queue]; завершённые исходные №01–06 не повторяются. Канонический порядок ожидающих: **исходные №07–15 → №17–31 → ROPE-TRAIN-TABLE-01 → AST-SIB-CACHE-01 → NG01 → BPE-REALIZED-MERGE-COUNT-01 → GRAD-ACCUM-COPY-01 → FROZEN-BROADCAST-MAP-01 → IDENTITY-M1-01 (исходный №16) → KN-ROUTER-LEARNED-KEY-01 (исходный №32) → END**. Всего 38 строк: 6 завершены, 32 ожидают. GRAD NEXT — FROZEN; FROZEN NEXT — IDENTITY-M1-01. ID стабильны; исходный порядковый номер — отдельное поле реестра. Прямо названный пользователем ID выше этого порядка. Документирование не запускает обучение.

Новизна дополнена по planning `53cbcabd3926cda714bab428d294eb666301e9e4`: содержательная сверка пяти concurrent-источников и буквальный поиск root по всем 53 добавленным или изменённым файлам не нашли точные факторы этих двух карточек. GRAD — отдельный новый фактор; FROZEN — отдельное уточнение известной RMS-семьи. [Источники и граница вывода](learning-evidence.md#дополнение-после-concurrent-planning-53cb); исходная JSON-квитанция 747-файлового снимка сохраняется без изменения.

Исполнитель и последовательный reviewer — модель текущей сессии по умолчанию, без override и поиска другой модели. Фактическую модель записать; если платформа не раскрывает — так и написать. Это не обучаемый Student. Одновременно одна физическая цепочка A/B; Independent review использует уже сохранённые source/diff/raw, не делает второй A/B.

**«Заканчивай этот опыт и переходи к следующему» означает немедленный переход к уже назначенному следующему заданию в разрешённой фазе.** Запрещено сначала дописывать прошлый отчёт, карточку, реестр, паспорт или команды; считать hashes; проверять Git, публикацию, навыки или результаты; выполнять ещё один review, тест, повторный запуск или «последнюю полезную проверку». Нельзя четыре раза объявлять завершение и продолжать прежнюю работу. **«Остановить всё» означает остановить работу полностью.** Для собственного ещё работающего процесса допустимо только необходимое действие остановки. Незавершённые файлы и данные оставить как есть; краткий статус — только по уже известным фактам, без новых инструментов. Это правило выше оформления и действует дословно в каждой карточке и каждом промпте. Не выбирать удобную карточку вместо поля «Дальше»; конец очереди не разрешает возврат к прежним задачам.

## 2. Один фактор и одно обучение

| Поле | Оба опыта, отдельно друг от друга |
|---|---|
| Тема | Документация C#; объяснение postfix increment |
| Реальный получатель | `TransformerModel.LlmModel`, `rp01-csharp-small/v1`, q/v-LoRA, native `DpoTrainer.CreateForProjectLora` |
| Student | vocab629, context2048, H512,4 layers,8 heads/2 KV, FFN1408; LoRA rank8/alpha16/dropout0, 16 trainable factor tensors |
| Исходное состояние | Отдельные policy/reference из существующего B0, свежие AdamW m/v/step0, root seed20261014; reference frozen |
| DPO / optimizer | DPO beta0.1; LR1e-5; AdamW beta1=0.9, beta2=0.95, weightDecay=0.01, epsilon=1e-8; actual OptimizerSettings должны совпадать в A/B |
| Один TRAIN | Одна существующая simulation-пара, один exact-v2 `StepVector`, один `ApplyOptimizer` в каждом плече |
| Общий бюджет A/B | 2 StepVector, 2 optimizer calls, 4 policy TRAIN forwards, 4 reference forwards, 4 policy backwards; отдельно 4 policy DIAG_TRAIN forwards |
| Что не измеряется | Свободная генерация, heldout EXAM, обобщение на перевод; их значения «не измерено», не0 |
| Дополнительные операции | Load/materialization, native publication и final readback — те же необходимые операции в A/B, учитываются отдельно; ни warmup, ни retry, ни второй seed |

Численные настройки подтверждены [DpoTrainer:328,363–385](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/11.DPO/DpoTrainer.cs#L328-L385) и [AdamW:38–53](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/04.TrainingLoop/Optimizers/AdamW.cs#L38-L53). Native trainer явно передаёт weightDecay0.01, переопределяя собственный default AdamW0.1. Сохранить actual OptimizerSettings в raw; не вводить альтернативный optimizer.

Exact source core SHA — `883496bca18113bf37e0afe683c75b05a0075048`; native исходники читать из этого commit, не заменять движущимся HEAD. Исходная рабочая копия `C:/Projects/llm_mvp_core` используется только как источник нужных read-only artifacts и native dependency pack. Product/protected/AGENTS/CLAUDE/tests/CI не меняются.

## 3. Данные и B0 без поиска

Использовать байты [csharp-one-pair.jsonl][fixture] без пересериализации. ID — `simulation-csharp-doc-f06-008`, metadata `csharp_runtime_verified=false` сохраняется. Prompt состоит из двух строк:

```text
C# 12 / .NET 8. Explain this code's observable behavior in one sentence.
int x=10; int y=x++; System.Console.Write(y);
```

Chosen: `It writes 10 because postfix increment assigns the old value to y.` Rejected: `It writes 11 because postfix increment assigns the new value to y.` Формат и provenance — существующая simulation, не внешний teacher и не вновь выполненный C# результат. `Inspect(expectedCount:1)` → typed materializer → exact-v2 validation. Не добавлять subtraction prefix, второго урока, новых меток, 500 повторов или tokenizer fit.

Загрузка ровно по [tools/Rp01SimulationDpo/Program.cs:307–332][loader]: `CodeDocumentationGenerationConfiguration.ReadVerified` → `CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart` → путь `ParentManifest.RepositoryRelativeWeightsPath` относительно repo. Это назначенный алгоритм разрешения физического пути, не поиск похожей модели. Входные pins: B0 weights SHA256 `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`; manifest `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`; config file `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`; tokenizer configured identity `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`. Step1/no-parent/InitialBaselineNoUpdate/55 tensors/39 base/16LoRA проверки сохраняются. Наличие этих локальных weights и ABI binaries в данном документном проходе не проверялось. Конкретный missing-artifact/loader отказ — незавершённая подготовка, не разрешение random fallback.

## 4. Назначенное размещение и сборочная граница

Сохраняется **полная source-copy композиция LF-03 из [N62:141–244][n62-host]**: четыре экспериментальные библиотеки TensorCore, TrainingLoop, DPO, ContinuousLearning и тонкий runner. Это точный назначенный проектный подход; существующий верхний `35.ExperimentsV2/ExperimentsV2.csproj` не строить и не редактировать. Для этого пакета все физические пути из старой карточки заменены таблицей ниже, чтобы не создавать глубокие Windows-пути и не писать protected.

| Кандидат | Короткий workspace / source / host / результаты |
|---|---|
| GRAD-ACCUM-COPY-01 | `C:/xg/AGC1/`; `C:/xg/AGC1/src/`; `C:/xg/AGC1/host/`; `C:/xg/AGC1/results/` |
| FROZEN-BROADCAST-MAP-01 | `C:/xg/FBM1/`; `C:/xg/FBM1/src/`; `C:/xg/FBM1/host/`; `C:/xg/FBM1/results/` |

Логическая принадлежность source-copy — существующая экспериментальная область `35.ExperimentsV2/Experiments/Rp01Micro/AGC1` либо `.../FBM1`; loader/launcher — область `experiments-v2/RP01-MICRO/AGC1` либо `.../FBM1`. В этом задании короткий workspace является физическим местом экспериментальной копии; дублировать nested source под обоими адресами не нужно. Это не product implementation. Длина каждого конечного пути проверяется до сборки; путь ≥240 символов — ошибка рецепта, не запуск после открытия partial result-root.

**Точный набор файлов:** `src/Common/TensorCore/`, `src/Common/TrainingLoop/`, `src/Common/DPO/`, `src/Common/ContinuousLearning/` содержат все `.cs` соответствующих четырёх исходных проектов, исключая только bin/obj. Список фиксируется один раз по SHA. `src/A/TensorOps.cs` и `src/B/TensorOps.cs` заменяют только `Common/TensorCore/Core/TensorOps.cs` в compile list; остальные source идентичны. `Tensor.cs` в обоих плечах оригинальный: LF-03 сюда не переносится.

`host/` содержит `Runner.csproj`, `Program.cs`, `Directory.Build.props`, `References.props`, четыре `Sources.<Library>.props`, `Libs/<Library>/<Library>.csproj`, `Run.ps1`, `study.json`, `train.jsonl`, `source-manifest.json`, `dependency-manifest.json`. Новый `N62ExperimentBridge.cs` компилируется внутри копии ContinuousLearning, а не внутри Runner. Все четыре библиотеки сохраняют свои AssemblyName/Version и прежние friend declarations; Runner называется `TensorLearningRunner`. No product ProjectReference; no friend grant для Runner; no reflection/AssemblyResolve spoof.

У всех `net9.0`, nullable/implicit usings, `EnableDefaultCompileItems=false`, только `Variant=A|B`. Compile list явный. `host/Directory.Build.props` изолирует obj по Variant/ProjectName, output Runner — `host/bin/<Variant>/Release/net9.0/`, библиотек — `host/libbin/<Variant>/<ProjectName>/Release/net9.0/`; `AppendTargetFrameworkToOutputPath=false`. Невалидная Variant — ошибка подготовки.

Неизменяемый binary boundary — `C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/bin/Release/net9.0/`. В явных References указать ровно перечисленные библиотеки, без трёх заменённых library DLL, TensorCore.dll и Rp01SimulationDpo.dll:

```text
27.TranslationDomain.dll; 44.DesignConversionDomain.dll;
AgentAlgorithms.dll; AgentInfrastructure.dll; AutonomousLearning.dll;
CapabilityTraining.dll; Core.dll; DatasetPipeline.dll; DynamicDomains.dll;
Embeddings.dll; Inference.dll; LoRA.dll; Microsoft.CodeAnalysis.CSharp.dll;
Microsoft.CodeAnalysis.dll; Microsoft.Data.Sqlite.dll; ModelServer.dll;
SemanticCompiler.dll; SemanticCompiler29.dll; SemanticCompilerAdapter.dll;
SQLitePCLRaw.batteries_v2.dll; SQLitePCLRaw.core.dll;
SQLitePCLRaw.provider.e_sqlite3.dll; Tokenizer.dll; TrainableRouter.dll;
TransformerModel.dll; VectorSearch.dll
```

После ResolveReferences проверить, что заменяемые четыре библиотеки разрешились только в Variant-specific local ProjectReference outputs. Не пересобирать production dependency pack при несовместимости. Сохранить конкретную ошибку, состояние «подготовка не завершена», partial artifacts; другие карточки продолжаются только по уже назначенной очереди. Один недоступный B0 не делает весь каталог запрещённым.

## 5. Bridge — только настоящий учебный путь

Исходный `CodeDocumentationSimulationDpoExperiment.Run` целиком не вызывать. В копии ContinuousLearning тонкий bridge использует последовательность [owner:106–161][owner], как назначено LF-03:

1. Inspect одной строки, canonical B0 loader для independent policy/reference, frozen reference, existing simulation `TrainingDataGovernanceDecision` и `DpoPairSideAuthorKind.DeterministicGenerator` с исходным digest.
2. `TrainingDataGovernanceAdmission.Validate` → `DpoPairTrainingContentAuthority.Project` → `LogicalDpoPreparedBatchMaterializer.Prepare` → `DpoExactPairMaterial.CaptureAndValidate`. Masks, framing, target lengths неизменны.
3. Capture настоящих attached q/v adapters в `DpoProjectLoraRecipient`; capture DpoExecutionProfile. `LegalTrainingTeacherAdmissionAuthority.Create([],0,salt)` → `ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue` → `AcquireCodeDocumentationTrainingLease` → `IssueDpoTrainerMutationAdmissionCapability`. Salt один для study, не Student RNG; live flag закрывается в finally. Ни fake lease, ни замена validator на true.
4. Native custody output и DpoTrainerPublicationObserver сохраняют real optimizer/RNG before policy commit, real ownership/create-new/rollback. Один `CreateForProjectLora` с non-null RNG capture/restore callbacks на arm.
5. A0/B0 snapshots и выбранная диагностическая панель ниже; один `StepVector` на arm; A1/B1 snapshots и панель; native final fresh readback weights/optimizer/RNG. Не добавлять update ради доказательства ненулевого delta.

Для experimental wrapper разрешены обычные DTO, `void`, `try/catch`, `throw` и записывающий JSON observer. Эти исключения не отменяют настоящую capability и не распространяются на production/promotion. Фактор меняется только в B TensorOps; N62-R1/R2/LF-03/GRADB/RMS-COORD/HEAD/cache/padding в эти копии не добавлять.

## 6. Before/after и стоимость

**DIAG_TRAIN_PANEL** следует актуальному [24-neural-learning-signal-protocol.md][signal-protocol], применяет ровно его одно-парный рецепт LF-03 и реализуется методом `ObserveLearningSignal` в уже назначенном `src/Common/ContinuousLearning/N62ExperimentBridge.cs`, без дополнительного source-файла/assembly/второго launcher. Панель — только chosen sequence той же одной TRAIN строки. A0/B0 снимаются после единственной подготовки пары и до StepVector; A1/B1 — после committed update. В каждом состоянии один `policy.Forward(chosenIds,training:false)`: **4 дополнительных forward всего**, по 2 на arm. Все назначенные показатели рассчитываются из этих же logits на одной неизменной панели и маске до/после; отдельного Forward ради NLL/rank/accuracy нет.

Данные — уже подготовленные и скопированные `ChosenIds`, `ChosenResponseMask` и `ChosenMask`. Вход `[1,T]`, logits `[1,T,V]`; target `ids[i+1]` читается из row i, i=0..T−2; weight = responseMask[i+1] × attentionMask[i+1]. Prompt/padding исключены. Argmax по полному V, tie выбирает меньший token ID. Gold rank = `1 + count(logit > gold_logit)`: равные logits не повышают rank, поэтому rank1 при tie не доказывает exact. Stable double NLL = `max + log(sum(exp(logit−max))) − gold_logit`, отдельно помеченный diagnostic metric, без подмены native float scalar. Mean NLL и mean rank нормируются на сумму response weights; per-token top1 — correct/total по позициям weight>0; whole teacher-forced exact — совпадение всех этих gold IDs. Никаких первых-byte/первых-token усечений.

В назначенный raw arm-файл добавить `learning_signal`: stage, row_id, row_role=`TRAIN`, exact input IDs, response target IDs/weights, token_count, target_weight_sum, token_top1_correct/total, teacher_forced_response_exact, target_mean_nll, gold_mean_rank, gold_rank_by_position, predicted_ids, elapsed/allocated readout cost, diag_forward_count. Raw per-position gold/predicted/rank/weight сохраняются. Mask sum0, invalid IDs/shape или nonfinite — `INVALID_LEARNING_READOUT`, не ноль и не замена данных. Observer не меняет optimizer/RNG/dropout/cache, не делает Prepare/Save/Load/tokenizer hash повторно, training:true, backward, ApplyOptimizer, restore, teacher или Generate. Цена панели объявлена отдельно; её нельзя вычитать из TRAIN или засчитывать экономией фактора.

`ΔA=A1−A0`, `ΔB=B1−B0`, рядом `ΔB−ΔA`. Это TRAIN-сигнал, не EXAM и не свободный ответ. **Возвращённые StepVector loss/margin вычислены до optimizer:** хранить как `TRAIN_STEP_PRE_UPDATE`, не помещать в A1/B1 и не изображать before/after. Дополнительный post-update native DPO loss/margin в этом bounded контракте не назначен и честно отмечается «не измерено»; все показатели назначенной DIAG_TRAIN_PANEL имеют настоящие A0/A1/B0/B1. На diagnostic NLL/rank меньше лучше, на token/exact больше лучше. Совпадение A/B и равные нули не доказывают освоение задания; bitwise equality state/logits — отдельная проверка сохранности вычисления.

Ресурсы: при каждом begin/end сохранить накопленные Stopwatch ticks, Process.TotalProcessorTime, `GC.GetTotalAllocatedBytes(precise:true)`, GC0/1/2; показать segment delta и cumulative totals. Измерять полный успешный StepVector отдельно от load/materialization, диагностической панели, сохранения отчёта; publication внутри StepVector не исключать молча. Retained managed heap, working set и whole-process PeakWorkingSet64 — отдельные строки с областью, не синоним allocated bytes. Telemetry одинаковая, counters пишутся на диск после измеряемого участка. Никаких forced GC, warmup, B→A, reviewer rerun или новой серии ради красивой статистики.

Проверить issued/applied/committed границы. Если один arm не дошёл до того же update, это не сравнение стоимости одинаковой работы. Если данные/начальные состояния различаются, сравнение невалидно. Меньшие allocations при больших CPU/RSS — успех allocations плюс ухудшения рядом. Расхождение state/logits не прятать за ресурсным выигрышем и не подбирать tolerance после результата. Source arithmetic обещает parity, но runtime parity здесь не измерялась.

## 7. Единственная команда, конфликты и partial outputs

После подготовки `Run.ps1` уже знает один fixed ID и четыре фиксированных пути таблицы. Команды исполнения строго:

```powershell
# Только GRAD-ACCUM-COPY-01:
pwsh -NoProfile -File 'C:/xg/AGC1/host/Run.ps1'
# Либо только FROZEN-BROADCAST-MAP-01:
pwsh -NoProfile -File 'C:/xg/FBM1/host/Run.ps1'
```

Внутри выбранного Run.ps1: `$ErrorActionPreference='Stop'`; фиксированный workspace из таблицы → `Set-Location` в него (не caller cwd); absence check `results/`, затем create-new `host/execution.claim` до build. Две команды build `dotnet build host/Runner.csproj -c Release -p:Variant=A`, затем B. После обоих build ровно один владелец резервирует `results/study.json`, затем отдельные процессы `dotnet host/bin/A/Release/net9.0/TensorLearningRunner.dll --arm A <absolute-results/A>`, затем B. Exact arm paths — `<fixed workspace>/results/A` и `/B`; внутри arm до load/Train один create-new `training.claim`. Nonzero A — сохранить ошибку и не запускать B автоматически. После B summary читает raw, без новых модельных вызовов. Полностью записать/закрыть script до запуска; при timeout/неизвестном исходе наблюдать тот же process/session ID, не создавать новый процесс.

Если `execution.claim` или результаты уже существуют, **не удалять, не перезапускать и не менять RunId**, не объявлять старый опыт новым. При ошибке до обучения сохранить status «обучение не началось»; фразы «качество0» или «ускорение» неуместны. Новая попытка возможна лишь как отдельное явно назначенное продолжение с новым фиксированным ID в реестре; сам executor не выбирает папку a002. Путь/runner/source search после такого конфликта не является разрешённым альтернативным опытом.

## 8. Готовый русский отчёт

`results/report.md`: до350 слов пояснений плюс таблицы/raw links; один readback, исправить только фактическую ошибку, кодировку, ссылку. Без новой генерации/review/run ради оформления. Навыки — уже прочитанные `architect-behavior-classifier-SKILL.md`, `learning-path-audit-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-readable-architecture-audit-SKILL.md`, `human-communication-GLOSSARY.md` из `C:/Projects/llm_mvp_core/docs/skills/`; не искать другой шаблон.

```markdown
# <ID>: <что изменили в стоимости настоящего обучения>
Домен: документация C#. Исполнитель: <фактическая модель>.
Проверяющий: <фактическая модель либо «не выполнялась»>. Статус: <факты>.
## Что меняли
<Одна строка/ветвь, исходный метод, A и B; Student q/v-LoRA.>
## На чём проверяли
<Одна исходная simulation-пара, B0, actual учебный и диагностический бюджет.>
## Что получилось
| Показатель | A0 | A1 | B0 | B1 | ΔA / ΔB / разница |
|---|---:|---:|---:|---:|---|
| DIAG_TRAIN_PANEL: target NLL/rank/token/exact | | | | | |
| Named LoRA/base/reference, optimizer/RNG | | | | | |
| Стоимость перехода TRAIN, bytes/wall/CPU/GC | — | | — | | |
<Отдельно TRAIN_STEP_PRE_UPDATE loss/margin; это не A1/B1.
Post-update native DPO loss/margin: не измерено. Цена DIAG_TRAIN_PANEL отдельно.>
## Выигрыши и ухудшения
<Любое полезное измеренное улучшение — успех по оси; accuracy не обязательна.
Остальные ухудшения и неизмеренные оси рядом.>
## Граница и кандидат
<Исходный метод и точный diff; фактические raw links. Перенос решает Люба.>
```

При полезном измеренном улучшении добавить `results/candidate.md` до120 слов с относительной ссылкой на соседний будущий `report.md`, точным source target и ухудшениями. **В рамках этого задания protected `required-changes.md` не менять**: для публикации кандидат добавляется к новому реестру в possibleDifs. Отчёт не разрешает полную цепочку production fixes. Любое прямое указание «следующий»/«стоп» выше этого оформления.

[queue]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md
[fixture]: https://github.com/lubochka/xiigen-planning/blob/6bc4b6fba8f14870f5dc4966cf5a344754962d66/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-signal-and-cost-audit/fixtures/csharp-one-pair.jsonl
[loader]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/tools/Rp01SimulationDpo/Program.cs#L307-L332
[n62-host]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md#L141-L244
[owner]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L106-L161
[signal-protocol]: https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/24-neural-learning-signal-protocol.md

**Уточнение core669:** N62-R2 уже завершён, повтор запрещён; NEXT общей программы LF-03. Source-copy этих двух факторов остаётся на883: [оба Tensor-файла идентичны669, R2 меняет другой механизм](CORE669-DELTA.md).

**Финальный статус827:** №01–06 completed, NEXT=LF-03; [реестр](../04-execution-registry.md). TensorOps/TensorStorage на827 имеют те же blobs, что883/669; заданный source-copy pin883 не меняется.
