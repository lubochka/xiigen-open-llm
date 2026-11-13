# FIX-N63C: дополнение по измеренному удержанию исправленного ответа

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски LEARN:** Сохранить назначенные lesson/RowId и before→factor→after по независимому gold; margin/hash/update не заменяют правильный ответ. Положительный контроль удерживает прежний правильный ответ, отрицательный показывает потерянную/неусвоенную строку; gained/lost TRAIN и EXAM раздельно.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


Дата: 2026-10-23. Это дополнение существующего **FIX-N63C**, а не новый FIX-ID. Основная продуктовая правка, точное сопоставление пары и восстановление транзакции остаются в [плане 17](../fixes-based-on-experiments/17-dpo-sft-correction-collision.md). Приоритет задаёт [актуальный реестр](01-registry.md); номер старой экспериментальной очереди здесь не является маршрутом исполнения.

Сейчас выполнены только чтение источников и запись этого плана. Реализация, сборка, тесты, обучение и повтор опыта не запускались. Будущая реализация наследует [контракт переноса и исполнения](02-native-transfer-and-test-repair.md). Сохранённые N63C и N63C-PROBE не становятся повторно исполняемыми от появления этого документа.

## 1. Что добавилось к плану 17

Новый источник — [results-03/n63c-probe.json](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results-03/n63c-probe.json), [его report.md](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results-03/report.md:44) и [progress.log](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results-03/progress.log). Они опубликованы в `0e437c2887618ec389b8011ad8632c4d428a3c95`. При чтении текущий HEAD был `916c93cc96a674fd65d82b382c5277532fc5b717`. Git-команды записи этим исследованием не выполнялись.

План 17 использовал другой сохранённый опыт: 600 шагов подготовки, затем по одному DPO и 100 SFT на плечо; TRAIN после SFT был A=20/23, B=21/23. Новый результат нельзя дописать к старому как продолжение одной траектории. В `results-03` подготовка составила 60 SFT, затем по одному DPO и 20 SFT на плечо. Получилось A=20/23, B=22/23. Новое основание — сохранение всех **ранее приобретённых 22 токенов**, а не освоение целого ответа.

| Измерение нового опыта | Общий обученный старт | После DPO, A и B | A после устаревшего SFT | B после исправленного SFT |
|---|---:|---:|---:|---:|
| TRAIN T1: правильные токены | 22/23 | 22/23 | 20/23 | 22/23 |
| T1: средняя NLL chosen | 0,5733602 | 0,5728762 | 1,1680962 | 0,6226763 |
| T1: средний перевес chosen над rejected | +0,8254118 | +0,8356108 | −0,6125487 | +0,8769186 |
| EXAM Q1: правильные токены | 0/29 | 0/29 | 0/29 | 0/29 |
| Q1: средняя NLL chosen | 6,7388034 | 6,7392793 | 6,2907543 | 6,7964600 |
| EXAM Q2: правильные токены | 2/21 | 2/21 | 2/21 | 1/21 |
| Q2: средняя NLL chosen | 6,4019790 | 6,4020770 | 6,0767740 | 6,7142390 |

Полного правильного ответа T1 нет ни до, ни после воздействия. B лучше A по удержанию TRAIN и знаку предпочтения, но его TRAIN NLL хуже собственного обученного старта; Q2 теряет один правильный токен, обе EXAM NLL ухудшаются. A улучшает EXAM NLL, одновременно разрушая предпочтение T1. Это ограниченный положительный результат по одной обученной строке с явными регрессиями, достаточный для уточнения плана кандидата; обобщение не доказано, разрешение на перенос в продукт отсюда не следует.

Сохранённый расход: 60 подготовительных SFT + 2 DPO + 40 SFT плеч, 42 диагностических Forward, 0 Generate и внешних учителей. Это 102 обновления; 42 не включает тренировочные Forward. Время всего опыта 129,196996 с, пик working set 1 206 616 064 байта. Время и allocations A/B наблюдались, но изменение текста урока здесь не изолирует вычислительную оптимизацию; отдельного ресурсного FIX из них не выводится.

## 2. Первичный код и границы вывода

[N63CollisionProbe.cs:54](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:54) задаёт `60 / 0.003 / 20 / 0.003`: предел и LR подготовки, число и LR SFT плеча. Blob текущего файла — `95bbf8a7f8ae67bc028d4906837304a76bfaea40`. В отличие от старого результата, эти константы согласуются с `results-03`. Это проверка исходника и receipt, а не восстановленная аттестация исполнявшейся DLL: полного source/binary binding нового процесса в прочитанном наборе нет.

- [LoadModels:477–509](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:477) проверяет конфигурацию, B0 manifest/weights, `InitialBaselineNoUpdate`, 55 именованных параметров и 39 базовых; policy/reference создаются отдельно. [RunArm:264](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:264) затем загружает приобретённые policy weights, сохраняя B0 reference.
- [RunArm:289–345](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:289) проходит simulation authority, lease, mutation capability и настоящий `DpoTrainer.CreateForProjectLora(...).StepVector`. Совпавшие DPO-метрики подтверждают одинаковые наблюдаемые показания, но сами по себе не доказывают побайтное равенство всего состояния двух плеч.
- [RunArm:350–375](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:350) создаёт свежий AdamW, делает 20 настоящих `Trainer.TrainSupervisedNextTokenStep`, оценивает панель и сохраняет `after-sft.bin`. Здесь нет сохранения финальных SFT moments/RNG. DPO checkpoint step1 имеет отдельные native optimizer/RNG; они не заменяют финальное SFT-состояние.
- [Подготовка:124](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:124) и [SFT:357](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:357) кодируют `Prompt + LF + Response`. [DpoExactPairMaterial.cs:21](C:/Projects/llm_mvp_core/11.DPO/Models/DpoExactPairMaterial.cs:21), строки 72–78 и 171–176, задаёт `prompt-response-segments-no-separator-v2`: отдельное кодирование сегментов и конкатенацию без LF/EOS. [SideScore:434–449](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs:434) считает targets через response mask. Поэтому малая full-sequence SFT loss 0,00084102585 не означает полный правильный DPO-scored ответ. Framing — известное различие контекстов, не доказанная причина последнего неверного токена.
- [DPO checkpoint A](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results-03/A_stale_rejected_text/checkpoints/dpo_step_000001.json) и [B](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results-03/B_corrected_text/checkpoints/dpo_step_000001.json) содержат native `RewardMargin=1.3741179`, objective `response-sum-softplus-signal-weighted-v3`. Это не средний readout margin +0,8356108 из таблицы; их нельзя взаимозаменять.

Фраза отчёта о невозможности обучения за четыре шага относится к наблюдавшейся исходной попытке, а не ко всем learners или бюджетам. Его слова «одинаковые числа — значит восстановлено одно состояние» сильнее доступной аттестации. Здесь эти ограничения сохранены, исходные raw/report не переписаны.

## 3. Что переносить в продукт, а что уже предусмотрено

Новых правил сопоставления не требуется. Наследуются разделы 2–3 плана 17: только точное `Ordinal` совпадение domain/prompt и `SFT.Response == admittedPair.Rejected`; единственное допустимое chosen; неоднозначность — отказ до mutation. Не менять DPO→SFT, веса урока, порядок/число примеров, LoRA, токенизатор или framing одновременно с фактором. Новый receipt не разрешает подменять simulation настоящей product authority и не отменяет зависимостей реестра.

Адреса будущей правки остаются прежними:

1. `C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/DpoSupervisedCorrectionProjection.cs` — запланированный internal helper, пока отсутствует. Он возвращает immutable projection и типизированные lineage bindings; старый indexed source не перезаписывается.
2. [IncrementalTrainer.cs:385–432](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/IncrementalTrainer.cs:385) — после настоящего admission, до `tokenSequences`, manifest/hash и `CandidateProvenanceSeed` передать проекцию во все связанные consumers. Сейчас строка 402 кодирует исходный `Prompt + LF + Response`. Исправление только runner или кэша не переносит механизм в продукт.
3. Версионирование `BuildTrainingExampleBatchSha256` и durable candidate seed, `FileDpoPairStore` transaction snapshot/Prepare — строго по плану 17. v1 не переинтерпретировать как v2; восстановленная binding определяет тот же материал без повторных DPO/SFT. Новое измерение не создаёт ещё одну схему admission или восстановления.

Математический смысл сохраняется: DPO увеличивает относительное предпочтение chosen относительно rejected, а следующий SFT на rejected минимизирует `−log p(rejected | prompt)` и может направлять градиент против предыдущей коррекции. Замена только совпавшего stale target устраняет эту конкретную конкуренцию целевых текстов. Это не теорема монотонности NLL, качества EXAM или совместимости градиентов для любых пар; наблюдаемые регрессии B это ограничение показывают.

## 4. Точное дополнение будущей учебной проверки

Этот раздел заменяет donor и расход **новой проверки переноса** из раздела 4 плана 17. Исторические результаты не меняются. Не запускать старый N63CollisionProbe, не повторять 60/600 шагов подготовки и не создавать другой seed/fixture при несовпадении.

**Данные.** Побайтно перенести [correction-collision.json](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-ingress-and-retention-review/ingress/fixtures/correction-collision.json>) в будущий `C:/Projects/llm_mvp_core/Tests/Fixtures/ExperimentFixes/v1/N63C/correction-collision.json`. SHA256 исходных bytes: `fcf2e07aa283ad40622b7c05eee738d0c490c5c2448f686fde935775ecff1992`. TRAIN только `// file: Abs.cs`, chosen `static int Abs(int n)=>n<0?-n:n;`, rejected `static int Abs(int n)=>n<0?n:-n;`. Q1 `Magnitude.cs` и Q2 `Sign.cs` — только EXAM. Метка fixture `CODE_Q` не заменяет native runtime-domain `code-documentation`; authored simulation не является product legal-teacher receipt.

**Сохранённый старт.** Назначен [results-03/authority-acquired.bin](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N63C-PROBE/results-03/authority-acquired.bin), 47 899 519 байт. Опубликованный Git LFS oid: `sha256:59810def905c4ead2f22985c5e1d525f59feb45590256eca8b492a293206d44f`. Это проверенный текст pointer, а не вычисленный сейчас digest working binary. Будущий preflight сверяет полные bytes и typed schema; нынешнее исследование бинарник не загружало и ABI не проверяло.

Модель H512/V629/C2048/L4/Q8/KV2/FF1408, LoRA q/v rank8, alpha16, dropout0. Конфигурация `C:/Projects/llm_mvp_core/config/code-documentation/generation-v1.json`, SHA256 `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`. Нативный `CodeDocumentationGenerationConfiguration.ReadVerified` и `CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart` используются с теми же declared inputs и guards, что в LoadModels; B0 manifest `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`, weights `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`.

Перед загрузкой donor вызвать [typed validator](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:911): `LlmModel.ValidateFullCheckpointArtifact(stream, cfg, new FullCheckpointLoraTopology(8, new[] { "q_proj", "v_proj" }), maximumArtifactBytes: 47899519)`; требуются `IsSuccess && Value`, затем отдельный stream/load. Base-only overload для этих LoRA weights не подходит. После native установки совместимых adapters загрузить donor policy; не инициализировать параметры после load. Reference остаётся B0. Не регистрировать этот simulation donor как canonical accepted parent.

Это новая, явно заданная эпоха из **weights-only** старта: DPO optimizer fresh step0, SFT AdamW fresh step0 в обеих руках. Исторические SFT moments не импортируются и не выдумываются. Одинаковый начальный RNG получают из двух native B0 `PrepareStart` с одной `AdapterSeedAuthority`; после загрузки donor и до первого score сохранить `CaptureRandomState` с identity, вычисленными через `LoraRandomStateIdentity` из этих config/seeds, и восстановить тот же снимок обеим рукам через `RestoreRandomState`. Снимок маркируется новым начальным step0, а не восстановленным RNG 60-го SFT шага.

| Будущий расход | A | B | Всего |
|---|---:|---:|---:|
| Новая подготовка/дополнительные warmup updates | 0 | 0 | 0 |
| Native DPO StepVector на T1 | 1 | 1 | 2 |
| SFT на T1 после DPO | 20 | 20 | 40 |
| Панели: до, после DPO, после SFT; T1/Q1/Q2 × chosen/rejected | 18 Forward | 18 Forward | 36 Forward |
| Тренировочные Forward: DPO 4 + SFT 20 | 24 | 24 | 48 |
| Все Forward / Backward | 42 / 22 | 42 / 22 | **84 / 44** |

Итого **42 updates**, 84 Forward, 44 Backward, 0 Generate/external calls. Числа тренировочных проходов — расчёт заданной native композиции, не счётчик нового запуска: [DpoTrainer.cs:878–904](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:878) делает по два reference/policy Forward и два Backward на пару, [Trainer.cs:117](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:117) — один Forward на supervised step. DPO LR 1e−5 (`DpoTrainer.DefaultLearningRate`, строка 363), один `StepVector` с chosen/rejected и неизменным reference; SFT LR=min=max0.003, batch1, seq2048, accumulation1, warmup0, clip1, AdamW(.9,.95,decay.01,eps1e−8), TotalSteps20. A использует stale response; B — результат продуктовой exact projection. Все остальные входы идентичны. Полные суммы включают обе независимые before-панели; дополнительных проверок модели ради отчёта нет.

До воздействия должны совпасть входные identities, все веса/RNG и native readouts A/B, T1 denominator23/correct22. Уже назначенные before-панели являются проверкой пригодности; никаких дополнительных Forward ради неё. При несовпадении разбирать custody, материал и framing, без обучения до нужного ответа. После полного фиксированного A/B любое полезное изменение правильных токенов, NLL или предпочтения фиксируется своей осью с регрессиями рядом. Исторические `20/23` и `22/23` после воздействия — ориентир evidence, не обещанный exact oracle или повод повторять тест. Whole-answer objective остаётся открытой при неполном ответе.

## 5. Тесты, адресные команды и восстановление без повторного обучения

Для новой проверки назначен файл `C:/Projects/llm_mvp_core/Tests/ExperimentFixes/DpoSftCollisionLearningTests.cs`, namespace `LlmFromScratch.Tests.ExperimentFixes`. Это выбранное размещение ранее запланированного класса из 17, а не второй одноимённый класс. Два Fact: `CaptureLearning` и `CompareCompletedReceipts`. Первый выполняет только своё плечо из таблицы; второй читает завершённые A/B receipts и делает 0 Train/Forward/Generate.

Общая подготовка теста перед A может извлечь native seam без изменения текста урока: baseline helper возвращает original/v1, B реализует projection/v2 по 17. Тест B вызывает этот продуктовый helper, а не повторяет правило замены в fixture runner. Независимый wiring spy доказывает, что `IncrementalTrainer` передаёт projected IDs в `ISupervisedTrainingStepExecutor`; микротест learner и spy нельзя вместе назвать измеренным production end-to-end обучением.

Новые/уточняемые проверки без дополнительного model fit:

- `DpoSftCorrectionProjectionTests`: exact collision, already chosen, иной prompt/domain, empty pairs, ambiguous chosen, неизменность порядка/веса/исходного ID; v1 bytes без коррекции и v2 binding с exact transaction pair ID. Expected material строится из frozen fixture, не из результата проверяемого helper.
- `IncrementalTrainer_ConsumesProjectedTokensBeforeBinding`: spy сверяет именно `Encode(prompt + LF + chosen)` и hash/provenance проекции. DPO-material остаётся отдельным no-separator material. Тест не должен «исправить» framing вместе с response-фактором.
- `CorrectionCandidate_RecoversWithoutSecondMutation`: сохранённые v1/v2 bindings и states повторно читаются без второго DPO/SFT. CandidatePrepared/CandidateReady и гонка snapshot/Prepare сохраняют условия плана 17.
- `CompareCompletedReceipts`: считает token-level и whole exact отдельно, выводит все Q1/Q2 регрессии, проверяет бюджет и identities. Capture сохраняет полный terminal state, уже имеющиеся metrics/qualification и ошибки записи независимо; COMPLETE receipt записывается последним. Ошибка formatter/validation не разрешает повторить capture.

Существующие `EmptyPairs_PreserveSupervisedPathWithoutDpoExecution`, reference-mutation и pipeline-domain guards из [IncrementalTrainerDpoWiringTests.cs](C:/Projects/llm_mvp_core/Tests/IncrementalTrainerDpoWiringTests.cs:441) сохраняются. В частности, EmptyPairs вызывает настоящий `TrainAsync`: ноль DPO не означает ноль SFT. Они **не назначены бесплатным дополнением** к бюджету 42/84/44. Старый широкий filter из 17:111 не применять для этого ограниченного capture; отдельные существующие guards требуют собственного заранее учтённого расхода по 02, без приписывания им неисполненных падений.

Будущий общий `Tests.Support/ExperimentTransfer/Run-NativeAB.ps1` по 02 получает existing PlanId `FIX-N63C`, stage A/B и назначенные `XIIGEN_FBE_*`; отдельный launcher не создавать. Output: `C:/Projects/llm_mvp_core/artifacts/fbe/FIX-N63C/attempt-0001/`, `learning/A`, `learning/B`, `contracts/B`, `report.ru.md`. Внутри соответствующей стадии после одного Rebuild выполняются только точные Fact:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --list-tests --filter "FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.DpoSftCollisionLearningTests.CaptureLearning"
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.DpoSftCollisionLearningTests.CaptureLearning"
# Только после completed A и completed B, один раз, без обучения:
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName=LlmFromScratch.Tests.ExperimentFixes.DpoSftCollisionLearningTests.CompareCompletedReceipts"
```

Это команды будущих ещё не реализованных тестов, не утверждение их наличия или текущей работоспособности. Discovery должен найти ровно один Fact. Rebuild ≤15 минут, один capture ≤5 минут, сравнение ≤1 минуты — пределы наблюдения по 02, не причина менять steps или автоматически считать процесс завершённым. Completed A той же пары повторно используется для B. Чужой owner, неизвестный исход или частичный capture запрещают новую выдачу; живой handle продолжают ждать, а пригодное сохранённое состояние используют для восстановления записи/отчёта без нового обучения. Автоматический attempt-0002 не назначен.

Исполнитель — фактическая модель будущей сессии по умолчанию, без override. STOP/NEXT выполняется немедленно по прямому поручению: не дописывать прежний отчёт/manifest, не повторять consumed опыт. Штатный bounded wait относится к тому же handle; нет обещания автоуведомления вместо ожидания, дополнительного фонового monitor или искусственной работы. Отчёт на русском по 02 разделяет native learning, wiring, source custody, частичные ответы и регрессии.

## 6. Охват этой документной проверки

Содержательно прочитаны новый report/progress/run log, соответствующие sections native Probe и IncrementalTrainer, framing/scoring anchors, relevant sections плана 17 и контракта 02. Новый JSON разобран целиком как структура; сравнивались acquisition, панели, оба arms, budget и limitations. Прочитаны оба step1 JSON и Git LFS pointers трёх сохранённых weights. Fixture прочитан и SHA256 проверен. Существующий тест EmptyPairs рассмотрен адресно; весь тестовый корпус не проверялся.

Бинарные checkpoints не загружались и не валидировались native API; источник исполнявшейся DLL отдельно не аттестован. Никаких новых опытов, тестов или независимых runtime reviews здесь нет. Добавлен только этот Markdown: новое измерение удержания, точный donor и меньшая source-backed учебная матрица для уже существующего FIX-N63C. Продуктовая проекция и durable recovery пока остаются планом.
