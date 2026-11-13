# DPO-TOKENIZER-SNAPSHOT-01: точная граница native source-copy

**DESIGNED_NOT_RUN.** Это документационная спецификация; перечисленные проекты, bridge и runner ещё не реализованы. Текущее поручение их не создаёт и не запускает. Карточка [DPO-TOKENIZER-SNAPSHOT-01.md](DPO-TOKENIZER-SNAPSHOT-01.md) задаёт единственный фактор, две учебные строки, B0 и измерения.

## Копируемые библиотеки и assembly identity

Источник — `lubochka/llm_mvp_core@ab583032562d6656de9dd5ca6fa0a325fd4b1f30`; канонический checkout только для чтения — `C:/Projects/llm_mvp_core`. Пакет — **C:/xg/ts01**. Три **полные** библиотеки скопированы в будущий design потому, что preparation находится в CapabilityTraining, материал — в DPO, а native issuers — в ContinuousLearning:

| Исходный проект | Полный набор C# | Назначенный проект копии |
|---|---:|---|
| `11.DPO/DPO.csproj` | 14 файлов | `C:/xg/ts01/lib/DPO.csproj`, AssemblyName DPO |
| `18.CapabilityTraining/CapabilityTraining.csproj` | 13 файлов | `C:/xg/ts01/lib/CapabilityTraining.csproj`, AssemblyName CapabilityTraining |
| `14.ContinuousLearning/ContinuousLearning.csproj` | 81 файл | `C:/xg/ts01/lib/ContinuousLearning.csproj`, AssemblyName ContinuousLearning |

Полный список 108 source blobs — [source-copy-manifest.json](source-copy-manifest.json). Все upstream файлы сохраняют имена, namespace и license headers в `common/DPO`, `common/CapabilityTraining`, `common/ContinuousLearning`. Ни одного `Compile Include` прямо в original checkout. Не копировать один DpoTrainer в другой executable и не давать executable имя friend library.

Три library projects используют .NET9, Nullable/ImplicitUsings как исходные проекты. AssemblyName/AssemblyVersion/Culture/PublicKeyToken копий должны совпасть с реально доступными исходными DLL; исходные friend declarations DPO и ContinuousLearning сохраняются, новые friend grants не добавляются. Недоступная strong-name signing identity — конкретная ошибка подготовки; подделка запрещена.

`Variant=A|B` выбирает только свои replacements для `DpoExactPairMaterial.cs` и `LlmIngredientLogicalUnits.cs`; исходные версии этих двух файлов исключаются из common Compile items, чтобы не получить duplicate type. Всё остальное одинаково. Никакой прочий source diff не входит в B. Common bridge — новый `C:/xg/ts01/bridge/TokenizerSnapshotExperimentBridge.cs`, компилируемый **в ContinuousLearning**. Host `C:/xg/ts01/Runner.csproj` с AssemblyName `TokenizerSnapshotRunner` содержит только `Program.cs` и normal ProjectReference на три копии.

Пути MSBuild абсолютные: у host `OutputPath=C:/xg/ts01/bin/$(Variant)/Release/net9.0/`; у library projects `OutputPath=C:/xg/ts01/libbin/$(Variant)/$(MSBuildProjectName)/Release/net9.0/`. Все четыре проекта задают `AppendTargetFrameworkToOutputPath=false` и `AppendRuntimeIdentifierToOutputPath=false`, иначе получится дополнительный net9.0 и конечные команды не найдут DLL. RuntimeIdentifier пустой, Configuration=Release; неподдерживаемые RID/Variant/Configuration — ошибка подготовки. `BaseIntermediateOutputPath` и `MSBuildProjectExtensionsPath` равны `C:/xg/ts01/obj/$(Variant)/$(MSBuildProjectName)/`, задаются до SDK imports. ImportDirectoryBuildProps/ImportDirectoryBuildTargets отключены. ProjectReference сохраняет Variant и копирует свои библиотеки в назначенный host output; output одного library project не делится с другим.

### Заранее выбранный binary dependency boundary

Ровно `C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/bin/Release/net9.0/`. Пакет копии ссылается обычными Reference/HintPath на следующие неизменные DLL; **TrainingLoop здесь не меняется**, в отличие от старого N62 host.

```text
27.TranslationDomain.dll; 44.DesignConversionDomain.dll;
AgentAlgorithms.dll; AgentInfrastructure.dll; AutonomousLearning.dll;
Core.dll; DatasetPipeline.dll; DynamicDomains.dll; Embeddings.dll;
Inference.dll; LoRA.dll; Microsoft.CodeAnalysis.CSharp.dll; Microsoft.CodeAnalysis.dll;
Microsoft.Data.Sqlite.dll; ModelServer.dll; SemanticCompiler.dll;
SemanticCompiler29.dll; SemanticCompilerAdapter.dll; SQLitePCLRaw.batteries_v2.dll;
SQLitePCLRaw.core.dll; SQLitePCLRaw.provider.e_sqlite3.dll; TensorCore.dll;
Tokenizer.dll; TrainableRouter.dll; TrainingLoop.dll; TransformerModel.dll; VectorSearch.dll
```

Нельзя ссылаться на исходные **DPO.dll, CapabilityTraining.dll, ContinuousLearning.dll** или Rp01SimulationDpo.exe/dll. Первые три замещают полные copy projects. Исходные executable deps/runtimeconfig не копируются как конфигурация нового host. Никаких product ProjectReference, package upgrade, solution rebuild или fallback на другой bin.

При однократной подготовке dependency-manifest фиксирует file paths/SHA/PE assembly identity. После ResolveReferences все `ReferencePath` и `ReferenceCopyLocalPaths` для трёх заменяемых identities должны указывать только на output соответствующего local ProjectReference нужного Variant. Старый transitive DLL — `PREPARATION_FAILED_ASSEMBLY_BOUNDARY`. Не чинить через AssemblyResolve/reflection после старта. Фактическая доступность этих Windows DLL, ABI и custody B0 в текущем документационном проходе не проверялась.

## Однофакторная patch specification

### DPO/Models/DpoExactPairMaterial.cs

В B ввести **internal opaque** `FrozenTokenizerSnapshot` с private readonly BpeTokenizer clone, private readonly canonical hash и vocab size. Ни property возвращающего BpeTokenizer, ни Save/Train mutator у carrier нет. Закрытый snapshot создаёт только существующая exact-material область DPO через исходные Save→Load→ComputeStateSha256. Это новый API **копии**, не уже существующий product API.

Сохранить исходную CreateBatch(BpeTokenizer, prompt, chosen, rejected, admission) для независимых callers: она продолжает capture на один вызов. Добавить internal overload с `ref FrozenTokenizerSnapshot? invocationSnapshot`. Он в исходном порядке выполняет CheckText и chosen≠rejected; только после этих guard делает lazy capture при null. Encoding, lossless checks, encoded inequality, DpoExactPairMaterial construction, detached tensors/masks, Validate выполняются заново для каждой строки. Snapshot не содержит IDs/answers/admission прошлого примера и не становится межсессионным cache.

### CapabilityTraining/LlmIngredientLogicalUnits.cs

В B/PrepareExecution локально определить nullable invocation snapshot; lifetime заканчивается с возвратом этого вызова. Legacy route прежний. Передать его по ref только через private BuildBatch к exact overload.

BuildBatch сначала сохраняет defined authors и `DpoTrainerPairCustoms.CreateAdmitted`, затем **исходный** `BatchTokenizationObserver.Value?.Invoke`. Если observer активен на этой строке, использовать исходный per-pair CreateBatch и **сбросить invocationSnapshot**: следующая строка не должна reuse состояние до потенциальной мутации через observer. Если observer отсутствует, вызвать новый overload. Snapshot создаётся на первом действительно дошедшем до CreateBatch valid exact row, а не до per-pair guard.

Никаких перемещений guard, external callback в DPO encoder, снятия capacity/authorship проверки, глобального Dictionary cache или изменения material version. Два source replacements B содержат только этот reuse; helper refactor общий для A/B не должен нагрузить A дополнительным capture/serialization. Инструментировать Save/Load capture count одинаково, ровно в реальной ветке создания snapshot.

Ожидаемая эквивалентность относится к owner-exclusive неизменяемому tokenizer. При произвольной внешней одновременной мутации старый код мог видеть разные snapshots в разных парах; новый захватывает одну. Это явно ограниченный контракт кандидата, не доказанное поведение всех внешних callers. В будущий prod перенос нельзя молча заявлять глобальную equivalence.

## Тонкий bridge: настоящий learner и исходные authority calls

Назначенный новый тип копии: `ContinuousLearning.TokenizerSnapshotExperimentBridge`, один public Run для immutable request с fixed ID/arm/input/output. Он компилируется в настоящей копии ContinuousLearning и использует существующие internal issuers, без reflection или новых permits. Program только проверяет exact CLI/compiled Variant и вызывает bridge. Не исполнять `CodeDocumentationSimulationDpoExperiment.Run`: его существующие main/calibration guards допускают 500/1 rows, а наш опыт — отдельный finite caller из двух строк.

Порядок bridge уже определён:

1. `CodeDocumentationSimulationDpoExperiment.Inspect(inputPath, fixtureSha, 2, SimulationDpoDomain.CodeDocumentation)`. Native duplicate/tie/text/provenance checks прежние.
2. Перенести в общий bridge **только** ReadConfiguration/LoadModels из `tools/Rp01SimulationDpo/Program.cs:307–332`. Root `C:/Projects/llm_mvp_core`. `ModelServer.CodeDocumentationGenerationConfiguration.ReadVerified` получает функцию env lookup: ENABLED=true, CONFIG_SHA256 из карточки, остальные null; глобальный environment не меняется. Два `TrainableRouter.Training.CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart` дают policy и отдельный reference. Все B0/Step1/no-parent/InitialBaselineNoUpdate/55/39 guards сохраняются. Checkpoint path берётся из manifest; произвольный файл не ищется.
3. Reference параметры RequiresGrad=false. `TrainingDataGovernanceDecision(true,false,["sha256:"+fixtureSha])`; исходные rich DpoPair с domain code-documentation и обеими `DpoPairSideAuthorKind.DeterministicGenerator`. `TrainingDataGovernanceAdmission.Validate` → `DpoPairTrainingContentAuthority.Project`. Никаких external teacher licences/passports не выдумывать.
4. Создать две LogicalDpoPreferenceExample из тех же трёх текстов и typed authors. Отмерить **один** `LogicalDpoPreparedBatchMaterializer.Prepare(preferences, tokenizer)`. Этот результат передать в training, не пересоздавать через вторую materialization. `DpoExactPairMaterial.CaptureAndValidate` для двух результатов выполняет исходную validation; его стоимость отдельная от MeasurePrepare.
5. Получить существующие attached q/v `LoraLinear`; `DpoProjectLoraRecipient.Capture`; `DpoExecutionProfile.Capture`. `LegalTrainingTeacherAdmissionAuthority.Create([],0,RandomNumberGenerator.GetBytes(32))` → `ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue` с исходными Validate/Seal/Verify callbacks → `AcquireCodeDocumentationTrainingLease(content, "DPO-TOKENIZER-SNAPSHOT-01-0001:"+arm)` → `IssueDpoTrainerMutationAdmissionCapability`. Non-null capability передаётся тренеру. `live=false` в finally и Dispose lease остаются.
6. RNG topology/loraIdentity берутся из `LoraRandomStateIdentity` исходного config/seeds. Приватный `<arm>/checkpoints` через SecureLocalFileBoundary. Observer сохраняет ownership receipts и в BeforePolicyFinalMove вызывает исходные RepositoryOwnedDpoOptimizerStateFile.WriteAndReadBack / RepositoryOwnedDpoRandomStateFile.WriteCreateNew; null RNG — отказ. Не менять DPO transactional publication/rollback.
7. Создать **один** `DpoTrainer.CreateForProjectLora(policy,reference,checkpointRoot,mutation,recipient,profile,null,observer,captureRng,restoreRng)` на arm. Null — именно continuation proof исходной B0; mutation non-null, оба RNG callbacks non-null. Ни нового optimizer, ни подмены softplus.
8. Записать A0/B0: named policy/reference bits, initial optimizer.Export bytes с step0, RNG. Для шага0 не вызывать native optimizer-file writer, который предназначен для положительных checkpoints. Прямо перед mutation записать issued, проверить optimizer.Step==0 и вызвать **ровно один** `trainer.StepVector(batches,1,timing)` с двумя rows. Только returned committed call отмечается completed.
9. Reopen private checkpoint через LlmModel.ValidateFullCheckpointArtifact/LoadCheckpoint в третьем Load из того же B0, optimizer OpenExisting, random OpenAndValidate/RestoreRandomState. Сохранить A1/B1, equality saved/live, unchanged base/reference. Не Evaluate/Generate. Нулевой LoRA delta записывать честно как observed no-change; он сам не является основанием признать вызванный optimizer несуществующим. Строгий guard нулевого delta из большого owner:205–207 в bridge не копируется; это одинаковое observation правило обоих arms, не treatment.
10. Из recorded JSON сформировать сравнение и закончить. Другие методы/домены не исполняются только потому, что их DLL присутствуют в boundary.

## Конечный CLI, попытки и отсутствие автоматического продолжения

Будущие файлы: `Runner.csproj`, `Program.cs`, три lib projects, common источники/bridge, A/B replacements, `in/train.jsonl`, source/dependency manifests. Результаты — `C:/xg/ts01/r/0001/{A,B}/`; общий `report.md`, `comparison.json`, `candidate.md` при useful gain. Максимальная вложенность не включает длинное имя репозитория/эксперимента.

Будущий `run-pair` с exact CLI из карточки сначала создаёт единый `r/0001/intent.json` с factor/source/fixture/B0/config/optimizer identities, A→B, two rows/one update per arm, noRetry=true. После этого запускает A и B child один раз; `run-arm --arm A|B --output <exact arm dir>` — private finite subcommand этого же host. Каждый arm отмечает issued/completed create-new. Controller не обучает и не создаёт третий arm.

Существующий intent/arm/issued marker означает `ATTEMPT_EXISTS_NO_REPLAY`. Нельзя обойти отказ новым RunId, удалить каталог, переписать старый status или повторить «для отчёта». Ошибка before optimizer → training not started; прерывание issued без completed → effect UNKNOWN до анализа существующего журнала, без нового update. Raw/partial artifacts сохраняются. Перезапуск с новой attempt identity требует отдельного прямого поручения; текущая документационная стадия не создаёт никакую попытку.

Если binary/source/config path не существует, отсутствие фиксируется в report подготовки, а не превращается в поиск альтернативного проекта, checkpoint или изменение product. Build касается только четырёх локальных проектов после будущей реализации. Полный CI, test repair, production promotion и protected-folder writes не входят в опыт.

