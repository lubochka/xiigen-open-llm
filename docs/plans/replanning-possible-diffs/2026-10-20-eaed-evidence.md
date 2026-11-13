# Eaеd: первичные доказательства и согласование новых Tensor-рецептов

20 октября 2026. Worker `/root/rp01_primary_evidence`, фаза READ_ONLY_RESEARCH. Изменяется только этот Markdown в разрешённой папке. Эксперименты, build/test/Train/Forward/Encode, binary loaders и git mutations не выполняются; source/runner/weights/raw не меняются. Это evidence-документ текущего чтения, не новый результат обучения.

## Текущий снимок и приоритет источников

Локальный `C:/Projects/llm_mvp_core` HEAD при чтении — `6e3e8864933e35fccdb4f21369c9912fa195aaf6`. Incoming пакет находится в `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments`; его окончательный core827 — более ранний снимок. В уже прочитанном live документном состоянии LF сохранён, №01–07 не повторяются, NEXT08. **Первоначальное промежуточное назначение eaed** было38ID: GRAD35→FROZEN36→IDENTITY37→KN38, семь сохранённых/31будущий. Последующий d7 и финальный readback ниже заменяют его на40/7/33. Это назначение документов, не запуск.

Attachment `C:/Users/lubao/.codex/attachments/22ca37e7-ce41-4cae-8272-a4b2bc62a4bf/Pasted text.txt`:8–10,73–135 — pull53cb→eaed,62 новых paths=61 пакет+общий pointer. Attachment `C:/Users/lubao/.codex/attachments/ebafc613-16ad-495a-b593-62274b65646c/Pasted text.txt`:3–4 описывает прежний53cb/47files; его :152 NEXT05 и :15836ID — исторические claims, не свежая очередь eaed. Повторная доставка summary не создаёт нового эксперимента или независимого review.

## Первичные Tensor-факторы

| Фактор | Первичное наблюдение | Отличие и граница |
|---|---|---|
| GRAD-ACCUM-COPY-01 | `C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs`:984–997: при первом `target.Grad==null` на :990 создаётся внешний `ToArray`; `Tensor.cs`:39–46 передаёт его в `TensorStorage.cs`:21–23 с ещё одним `ToArray`. | B снимает только внешний duplicate; RequiresGrad guard, `+=`, defensive storage и `Tensor.Backward`:99 остаются. Это не LF initial-gradient copy. Измеренного выигрыша нового фактора нет. |
| FROZEN-BROADCAST-MAP-01 | `TensorOps.cs`:168–184 выделяет/пишет `mappedBIndices` всегда; чтение на :199/:207 только в closure под :188 `a.RequiresGrad || b.RequiresGrad`. | B early-return для обоих false сохраняет `FlatToIndices`:174, multiply, shape/cancellation и весь grad path. Это не RMS-COORD, который меняет координатные allocations. Измеренного выигрыша нет. |

Полные Git blob идентичности pin883/HEAD проверены read-only: TensorOps `e1a20dca84d76ee85d0c559bd496c1fa3c96ac8b`, Tensor `a2569eefe378fb0a841cbb2137c6a8ac001bdb3a`, TensorStorage `22aec3ba03b756530001ae786acbdce28af90104`, RmsNorm `246bbc4781d4c06d3b71f617d51aec8860ac39f2`. Это не проверка ABI DLL.

Учебная причинная цепь в `C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs`:879/882 — reference forwards,885/888 policy forwards,901/904 backward,921 один ApplyOptimizer. `BackwardLogProbs`:1120 вызывает logits.Backward; TensorOps backward вызывает AccumulateGrad. `14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs`:112 замораживает independent reference,129–161 создаёт actual q/v recipient/profile/legal authority/lease и non-null RNG callbacks. `03.TransformerModel/LlmModel.cs`:226–239 проходит blocks/finalNorm, `Layers/RmsNorm.cs`:36–40 вызывает MultiplyBroadcast. Теоретические payload sums не равны measured allocations/peak.

Каждый новый ID: одна simulation-пара f06-008, один StepVector/arm, всего2updates/4policy TRAIN forwards/4reference forwards/4backwards и отдельно4policy DIAG_TRAIN forwards. Full-response chosen panel с shifted rows и исходными masks — самостоятельный новый readout двух новых ID. Он не приписывается старому LF, где сохранён только first-token сигнал. StepVector return — PRE_UPDATE, не post-update target quality. No EXAM/Generate.

## Замыкание будущей сборочной границы

Pin `883496bca18113bf37e0afe683c75b05a0075048`: git ls-tree фиксирует114 `.cs`:13 TensorCore,6 TrainingLoop,14 DPO,81 ContinuousLearning, без bin/obj. Это состав source closure, не114 semantic code reviews. Четыре исходных csproj прочитаны полностью. Target net9, nullable/implicit. Имена assembly следуют именам проектов TensorCore/TrainingLoop/DPO/ContinuousLearning; explicit AssemblyVersion/RootNamespace overrides в этих проектах и root Directory.Build не найдены. Фактическая identity/ABI binary pack не проверялась.

Сохранить IVT из `11.DPO/DPO.csproj`:19–27 (ContinuousLearning, CapabilityTraining, LlmFromScratch.Tests), `14.ContinuousLearning/ContinuousLearning.csproj`:19–30 (42.ProjectWikiDomain,27.TranslationDomain,TrainableRouter,LlmFromScratch.Tests) и исходные .cs assembly attributes: TensorCore/AssemblyInfo.cs:7; TrainingLoop/Trainer.cs:13; ContinuousLearning/Feedback/FeedbackRouter.cs:6–7, Feedback/SecureLocalFileBoundary.cs:6–7, Training/CodeDocumentationSimulationDpoExperiment.cs:17, Training/ContinuousLearningModelServerComposition.cs:6, Training/LocalQwenOllamaTeacherInferenceClient.cs:6, Training/RepositoryOwnedDpoRandomStateFile.cs:12. Новый friend для TensorLearningRunner не нужен: bridge собирается внутри ContinuousLearning.

Полная историческая source-copy/capability композиция прочитана из локального Git blob `883496…:docs/plans/replanning-possible-diffs/experimets/cards/N62-neural-update-costs.md`:141–244. Live N62 уже retired; ссылку incoming contract:48/152 нельзя превратить в поручение искать старые141–244 в текущем49-строчном файле. Parent встраивает фиксированную композицию в новый shared27 contract.

Конкретные native inputs: `C:/Projects/llm_mvp_core/config/code-documentation/generation-v1.json` фактически прочитан; SHA256 `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40` совпал. StudentV629/H512/L4/H8/KV2/FFN1408, q/v rank8/alpha16/dropout0, seed20261014. `25.ModelServer/ContinuousLearning/CodeDocumentationGenerationConfiguration.cs`:60–63 читает этот exact path, :110–114 — соседний `tokenizer.json`; :103–107 задаёт Denied cold-start. Native loader `tools/Rp01SimulationDpo/Program.cs`:307–332 проверяет configuration pin, PrepareStart, manifest Step1/no-parent/pins/55tensors/39base; две независимые загрузки policy/reference; weights path = repo+verified `ParentManifest.RepositoryRelativeWeightsPath`. Это заданное разрешение пути, не поиск модели исполнителем. Payload weights здесь не открывался.

Final custody: simulation owner:194–207 ValidateFullCheckpointArtifact→rewind→CreateReadbackModel→LoadCheckpoint; optimizer OpenExisting сравнивается по bytes с live, RNG OpenAndValidate→RestoreRandomState. Различать исходный B0, trained live policy и fresh reloaded candidate; frozen reference отдельно. Чтение optimizer bytes не доказывает новое обучение после восстановления. Старый combined guard changedLoRA==0/readback failure не должен обнулять отдельные observations.

## Применяемые parent уточнения

- `C:/xg/AGC1` и FBM1 заменяются назначенными `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/AGC1` и FBM1; внутри src/host/results. Текущий worker папки не создаёт.
- Stale6completed/NEXTLF заменяются saved7/NEXT08; новые два ID самостоятельны, старые N62 не повторяются.
- Incoming protected-registry запрет относится к другому аудитору. В нашем authorized scope обычный полезный candidate регистрируется в required-changes.md; немедленный STOP/NEXT выше оформления.
- Factor→manifest→build, one host claim до build, один владелец results/study, arm claim до load/Train. Host study — blueprint; не второй result study. Existing/unknown не обходится сменой root/attempt.

Первоначально это был промежуточный раздел. Заключительное покрытие пакета eaed и границы проверки записаны ниже; новая порция d7 рассматривается отдельно и не расширяет эти числа.

## Заключительное чтение eaed — 2026-10-20

Прочитаны все 32 Markdown пакета по смыслу, все 29 JSON полностью декодированы и структурно обойдены (49 634 узла JSON, ошибок разбора нет). Это не 32 независимых review и не семантическая проверка всех файлов, перечисленных в больших manifests. Таблица CARD-BY-CARD-COVERAGE прочитана как историческое свидетельство автора; повторяющийся дословный суффикс семейства сокращался при выводе, все неповторяющиеся строки рассмотрены. Ни один связанный в таблице источник не считается прочитанным только из-за наличия ссылки.

Корень входов: `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-20-learning-evidence-and-next-experiments`. Ни этот пакет, ни его исторические команды не являются разрешением на runtime. Publication attachment ebafc613 относится к прежнему 53cb/36-ID состоянию, а не является новой квитанцией eaed: текущий статус берётся из живого canonical индекса и сохранённых результатов.

### Дополнительные подтверждённые цепи и ограниченные пробелы

1. Native broker не имеет whitelist конкретных AGC1/FBM1 ID. [ServerOwnedLegalTrainingTeacherAdmissionAuthority.cs:872](C:/Projects/llm_mvp_core/14.ContinuousLearning/Core/ServerOwnedLegalTrainingTeacherAdmissionAuthority.cs:872) принимает CodeDocumentation content через настоящий lease; строки 930–938 допускают 1–256 ASCII letters/digits/`-_.:`. Поэтому `GRAD-ACCUM-COPY-01:A`, `:B`, `FROZEN-BROADCAST-MAP-01:A`, `:B` удовлетворяют именно этому source-предикату. Это не runtime проверка выдачи capability. Scope hash выводится из RunId; provenance — из этого scope и content. Новый ID не требует bypass валидатора.
2. В выбранной цепи отдельный native paired-study store не найден: старый экспериментальный [Prepare-Study.ps1:69](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N62-R1/Prepare-Study.ps1:69) писал study metadata; native ownership касается lease, checkpoint/optimizer/RNG. Нельзя приписывать новому bridge несуществующий store API. Старый [N62RunRequest:21](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/N62-R1/Common/ContinuousLearning/N62ExperimentBridge.cs:21) задаёт ExperimentId, Arm, RepositoryRoot, StudyRoot, ArmOutputDirectory, InputPath, InputSha256, ExpectedPairCount, ConfigSha256, B0WeightsSha256, B0ManifestSha256, AuthoritySaltPath. Для новых опытов InputPath назначен `<Workspace>/host/train.jsonl`, salt — `<Workspace>/host/authority-salt.bin`; StudyRoot=`<Workspace>/results`. Старый Program:42–48 смотрел в StudyRoot, поэтому его нельзя копировать без этой явной адаптации. Соль producer [Prepare-Study.ps1:57](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/N62-R1/Prepare-Study.ps1:57) — один случайный массив 32 bytes, общий arms, с length check. Это authority salt, не seed Student.
3. **E-METRIC-PHASE — отдельное условие известной stopping-семьи, не новый измеренный успех.** [LlmIngredientLogicalUnits.cs:1378](C:/Projects/llm_mvp_core/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:1378) выполняет StepVector, затем на 1388 передаёт `lastMetrics.Loss` в monitor. DpoTrainer вычисляет его до ApplyOptimizer (891–896 против 921). [P78:144](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P78-loss-plateau.md:144) меняет window1→5; [P89:153](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P89-density-stop.md:153) меняет weighted→normalized. Ни один из этих факторов не сравнивает pre/post одну и ту же weighted objective при fixed window/threshold. Для такого отдельного контраста post readout нужен в обеих руках, с явным бюджетом; возвращённые старые scalars не after. Точные model/tokenizer/lessons/host для этого generated-capability caller входной источник не закрепляет: author-owned NEEDS_INPUT, не исполнительский поиск и не подмена RP01 simulation.
4. **HIST-E07-GLOBAL-01 — условное накопительное обучение.** Current [E07StructuralContextToken.cs:275](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs:275): `_trans`/`_bigram` живут в объекте, local globalNextCounts пересоздаётся на 285, `_mostFrequentNext` заменяется на 307; Predict fallback читает его на 322. Источник `history/02-conditional-ngram-continuation.md:15–19` сам ограничивает вывод: старый Run создаёт новый predictor и вызывает Train единожды. Поэтому это не объяснение исторических отрицательных E07 и не текущий RP01 blocker. Предложенный основной budget 4 Train/30 Predict в источнике не включает неопределённый дополнительный бюджет обратного порядка/single/empty controls; active recipe пока не замкнут и в очередь не добавляется.
5. **P54 не получает семь вымышленных опытов.** Current P54:21,266–308 уже снимает OUT_OF_LEARNING_SCOPE и сохраняет NEEDS_INPUT_FOR_RECIPE для конкретных learned contents/tokenizer/query/mandatory/semantic contracts. Историческое требование исправить blanket запрет закрыто ранее. Восстановленный reader связан с обучением, но наличие старого success не удостоверяет все семь нужных состояний.
6. `history/04-unmentioned-source-crosswalk.md:59–80` различает ещё три узких lead: phase-only condition выше; OrderedPhraseSupport/v1 перед exact/phrase/word/kNN шире прежнего SourceSpanFrames; положительная correction Verifier при chosen.Pass=true требует установленного caller. Последний не объявлен native RP01 фактором. Нулевой LoRA delta после committed update записывается наблюдением; source-owner объединял его с final readback guard, но отсутствие delta не оправдывает дополнительный update. Эти различия переданы родителю для адресных P/N60 примечаний, без нового host.

### Семантический ledger всех Markdown eaed

Каждое имя ниже относительно указанного полного корня. «Прочитан» означает текст этого документа, а не повторное исполнение или независимую проверку всех его ссылок.

| Файл | Что учтено |
|---|---|
| README.md | Исторические счётчики/pins, две новые Tensor-гипотезы; current queue не берётся из старого NEXT. |
| 01-learning-mechanisms-and-rag.md | Разделены Unit memory, native DPO, DPO→subunits, learned readers; blanket «нет Train значит нет learning» неприменим. |
| 02-results-and-candidates.md | Сохранены полезные измерения рядом с регрессиями; allocated/peak/time не взаимозаменяемы; новые Tensor gains не заявлены. |
| 03-coverage-and-limits.md | 747/334/470 — scope исходного аудита, не наши новые reviews; 116 crosswalk не готовые recipes. |
| 04-execution-registry.md | 38-ID proposal интегрируется по ID, saved N62 не переигрывается, readers последние. |
| 05-executor-contract.md | External roots/protected-write scope принадлежат прежней сессии; canonical пути/owner/claim имеют текущий приоритет. |
| 06-concurrent-reconciliation.md | Историческое согласование 53cb, не новый runtime; старые output assignments сохранены как происхождение. |
| 07-core669-status-and-results.md | R2 saved supersedes старый NEXT; current HEAD свежее669. |
| 08-final-core827.md | EOPT saved и labels/custody limits; saved LF текущего workspace дополнительно исключает прежний NEXT LF. |
| source-and-review.md | Чужие read/review receipts не присвоены worker; меняющиеся snapshot counts атрибутированы. |
| candidates/N62-R1.md | Старый next R2 исторический; allocation gain/GC regressions сохранены, 4.9x не causal. |
| learning/GRAD-ACCUM-COPY-01.md | Один внешний ToArray в AccumulateGrad, defensive storage copy остаётся; отдельный фактор от LF. |
| learning/FROZEN-BROADCAST-MAP-01.md | No-grad branch до map allocation; FlatToIndices/grad branch остаются, отдельный фактор RMS-семьи. |
| learning/TENSOR-LEARNING-EXECUTION-CONTRACT.md | Один fixed pair/step arm, четыре дополнительных full-response DIAG; четыре source-copy libraries, без вызова старого main. |
| learning/learning-evidence.md | Primary causal-chain и bounded novelty; глобальная новизна не заявлена. |
| learning/CORE669-DELTA.md | Target Tensor blobs стабильны; moving docs не меняют pinned native source. |
| experiments/CARD-BY-CARD-COVERAGE.md | Все строки исторического ledger рассмотрены; 153 linked cards не объявлены 153 новыми reviews. |
| experiments/CONCURRENT-MERGE-ADVICE.md | Старые roots/order superseded текущими карточками; не альтернативный runnable маршрут. |
| experiments/FINAL-CONCURRENT-MAPPING-REVIEW.md | Review старого mapping, собственный current check отдельно. |
| experiments/EXPERIMENT-AUDIT-CURRENT.md | Fourteen-rule/card checks исходного автора; training scope и missing recipe не смешаны. |
| experiments/CURRENT669-N62-R2-READBACK.md | Два completed update/arm; returned loss pre-update; final panel отсутствует, custody gap сохранён. |
| experiments/FINAL827-EOPT-READBACK.md | Measured allocations/peak/GC, stale R1 labels и source-manifest limit; saved result не повторяется. |
| experiments/N62-R1-RESULT-CORRIGENDUM.md | Сняты причинный4.9x, «нет улучшений» и «нет регрессий»; 16 изменённых tensors не подтверждают навыка. |
| experiments/EXECUTION-CONFLICT-OVERLAY.md | Исторический R2 wrapper/8DIAG не применяются ретроактивно; claim owner один, не второй prepare. |
| experiments/INDEPENDENT-PACK-REVIEW.md | Чужие R01–03 исправления/35-ID draft сохранены как история; build/ABI не проверены. |
| history/01-history-evidence.md | E09/E14/E22 положительные и отрицательные записи сохранены; two zero E22 classes; unmatched matrix donor не опровергает structural kNN transfer. |
| history/02-conditional-ngram-continuation.md | Mixed accumulation horizon E07 conditional; single-run не затронут; дополнительные control budgets не замкнуты. |
| history/03-p54-ng01-semantic-verdict.md | Исторические P54/NG01 document gaps уже исправлены; exact int-pattern factor не generic C2 duplicate. |
| history/04-unmentioned-source-crosswalk.md | Exact phase/positive correction/OrderedPhraseSupport distinctions; broad prior mentions не полное покрытие. |
| history/05-concurrent-primary-reconciliation.md | 27ae structural kNN→production имеет receipt; original RP01 margin success и readback document provenance отдельно от нового runtime. |
| history/06-core669-gap-status.md | P54/NG01 closed document presence, remaining author-owned inputs; P78/P89 phase factor отдельно. |
| history/07-core827-output-delta.md | AST/BR01 internal roots, ROPE candidate.md correction; old mapped paths не запускаются. |

### Targeted readback новых canonical Tensor docs

Прочитаны новые GRAD/FROZEN cards полностью и общий27 полностью (194 строки на момент чтения), после передачи source closure родителю. Верные ограничения: genuine StepVector/lease, 2 updates +4 DIAG на каждый ID, original TensorStorage, pinned114-source composition, explicit early SDK path imports, final readback третьей модели, runtime not materialized. Переданы три точных остатка: явно поставить FROZEN branch **до mappedBIndices allocation**; убрать подразумеваемый «paired-store» и закрепить actual request/RunId predicate; включить authority-salt.bin в host file list и использовать host inputs вместо старого StudyRoot сочетания. Новый d7 steering требует40-ID routing и FROZEN→VT; это обновление очереди родителя, не увеличение двух Tensor budget. Последующая правка этих мест не считается проверенной до отдельного readback.

**Граница результата:** работоспособность host/SDK/ABI/dependency binaries/B0 payload, actual encoding, native permissions, численный parity и выигрыши новых опытов не проверены. Binary checkpoint не загружались. Runtime действий worker — 0; product/git mutations — 0. Записан только этот evidence Markdown.

## Полный byte inventory eaed

SHA256 ниже относится к полностью прочитанным исходным bytes. Markdown review scope описан выше; JSON — полный parse/структурный обход, не все транзитивные ссылки. Файлы перечислены относительно полного source root выше.

| Файл | Bytes | SHA256 | Read scope |
|---|---:|---|---|
| 01-learning-mechanisms-and-rag.md | 41895 | `51f00b1523f8e9d7b030dfc1297e7cf8a730357a8b43bdb4e73158156858959e` | full Markdown semantic read |
| 02-results-and-candidates.md | 18265 | `1665c2f1fb2543952e3424da5b22123d2c423e62d35ecaab82735c0defcc1947` | full Markdown semantic read |
| 03-coverage-and-limits.md | 14945 | `f48b232b0b0fcb8fba890c2187e608b8fe14d560ce2ba16285901ed2a014438e` | full Markdown semantic read |
| 04-execution-registry.md | 22437 | `7712908b54b283548729d00a3a1b5ef41d9e5eb08820a07f6946033f152d0d36` | full Markdown semantic read |
| 05-executor-contract.md | 28077 | `0e783c486f1a810b2d482d2cc3434d3c4951d43974264094b512ad7e7733b38a` | full Markdown semantic read |
| 06-concurrent-reconciliation.md | 7780 | `3caa50be712ab1faf11930e114a76d7cfadc1e5c1777b1357b6809fac523e7ba` | full Markdown semantic read |
| 07-core669-status-and-results.md | 8016 | `0b092f1c8dce6929f47b0e47fadf61131d90338ad370389cf7c0371ff8800018` | full Markdown semantic read |
| 08-final-core827.md | 8209 | `bfc11397bf70fa6a211ececc18384a1092da9c398cc54b57a7e5bca15db63e5b` | full Markdown semantic read |
| candidates/N62-R1.md | 2592 | `5ded5e2e055652b759627feb801efd6db26f406dbbc55ed45dd932cb8c904333` | full Markdown semantic read |
| coverage/additional-linked-sources.json | 499 | `c63c5c6c4c10a56398d7c0d0a3aec2d19fd5723bbdd8cba39b5589643e6d1ff3` | full bytes + JSON parse/structural traversal |
| coverage/branches-inspected.json | 4008 | `9622097d09ae1968fe8a5a80106e79dd19e679500f580348638b0bbb78476479` | full bytes + JSON parse/structural traversal |
| coverage/concurrent-source-manifest.json | 14822 | `57c7dbea8a8235d93eaec1e6347124aef479842dcbfb07d23c76e6f1eb4d65f9` | full bytes + JSON parse/structural traversal |
| coverage/core669-concurrent-change-receipt.json | 65525 | `06e2a63b699efc1447e866dd72f92dcf0602e22273e7b71fb5cb30fc64cf8de4` | full bytes + JSON parse/structural traversal |
| coverage/core669-result-source-identities.json | 5076 | `28649fc45de94b3e14b162dc975a029c54a1748cde89dd7a4da4e8167250ee7b` | full bytes + JSON parse/structural traversal |
| coverage/execution-projection.json | 64619 | `92aec1f78712e362393da21c4df7bb6c28aa6353bba0a707317e1abaf746424e` | full bytes + JSON parse/structural traversal |
| coverage/final-document-validation.json | 1891 | `31177842ad13fad9b05422d11d361d33875d25b014898629e08520720be20030` | full bytes + JSON parse/structural traversal |
| coverage/planning-source-index.json | 312044 | `3f1a60ffaa16b9bb01bb7c5376e294a46f20ebcd2f5291ea76c497974334eec5` | full bytes + JSON parse/structural traversal |
| coverage/rag-source-manifest.json | 22468 | `be595edf0a4d9b62791e716c93032bfc520a7e495b91c08ed88073079f4e7540` | full bytes + JSON parse/structural traversal |
| coverage/repository-content-manifest.json | 613107 | `e4abc97389526034e53b19d9e13ff21bb2d7127e2c19fffb0535777273c46e45` | full bytes + JSON parse/structural traversal |
| coverage/subcase-links.json | 116654 | `28bc0031799aca7e5c53a2aac42fbdd5cceedfe2267319e1e9e8b0f244f5fd62` | full bytes + JSON parse/structural traversal |
| experiments/CARD-BY-CARD-COVERAGE.md | 74796 | `2eea38260c463d0ced268811fe649823cb5fab255b6bd47c5eeb4a03bb0d9a28` | full Markdown semantic read |
| experiments/card-policy-matrix-current.json | 137474 | `3cef6e860b44aaf720f68c003ee6bdfdb6d76f8f2c1ee0c68e03ccbc11dc5521` | full bytes + JSON parse/structural traversal |
| experiments/cards-inventory-current.json | 35454 | `c9cb7f46f20f1b9289c441b3b36835579483df959ee4d87dd6358e087f0bb884` | full bytes + JSON parse/structural traversal |
| experiments/CONCURRENT-MERGE-ADVICE.md | 5555 | `4881fafd58ed116f72e30991a3a1ab2b7d1c9f12a48279851136bafb433327a3` | full Markdown semantic read |
| experiments/concurrent-merged-registry.json | 73325 | `7e1a3e8ddf8c8b4d3664b2375b8183620167b0a32be4728818c8651fe2336e4f` | full bytes + JSON parse/structural traversal |
| experiments/core669-R2-recalculated.json | 2371 | `59f8f115461515a60b9780b72e9f936f92aa4bdb87ede40aef4fb836834b46e4` | full bytes + JSON parse/structural traversal |
| experiments/current-queue.json | 17689 | `dfca9620321f09f0a50729c32aac88b35ac749519b265eca87c15d9b108b44f2` | full bytes + JSON parse/structural traversal |
| experiments/CURRENT669-N62-R2-READBACK.md | 10481 | `191c29602195ad7ade3bf06035e0bad26484d472ed87d90273f172dfe03fa685` | full Markdown semantic read |
| experiments/EXECUTION-CONFLICT-OVERLAY.md | 16518 | `ce61b2b00052cf7fb4c23d9704241609d9d22b95b426d465bdabca12b68082d5` | full Markdown semantic read |
| experiments/EXPERIMENT-AUDIT-CURRENT.md | 19879 | `f9d063e5b0cc82e7c2ecf3e471f985f28649c0abdd4a6903e9554cf49938f552` | full Markdown semantic read |
| experiments/FINAL-CONCURRENT-MAPPING-REVIEW.md | 5466 | `e588ff1f707c42047ad783f1f8dd363d59d35b2a7322698eb7518b871cb9dfe4` | full Markdown semantic read |
| experiments/FINAL827-EOPT-METRICS.json | 6378 | `e01743fd6c231a7f9c560a272084486784364d4960f27413ee7919443d323343` | full bytes + JSON parse/structural traversal |
| experiments/FINAL827-EOPT-READBACK.md | 13072 | `52f43891f8875f04783835c0825259e24b39b4cc4fa9052bf8f075b9a0ad2471` | full Markdown semantic read |
| experiments/INDEPENDENT-PACK-REVIEW.md | 19564 | `579bd921f3e1b8f4d195a8a6c245f9547d1cd892a189f0728d2f06a9a55a94c7` | full Markdown semantic read |
| experiments/independent-root-mapping-review.json | 699 | `78fd216ee2d6e73fc8edfa864d204427a22ee427f4a8384d866f0196a04f9503` | full bytes + JSON parse/structural traversal |
| experiments/measured-results-recalculated.json | 4849 | `6bce367f23c526a1795d503ffb1883b9d016938c3e9877e59cfd43d9bb67702e` | full bytes + JSON parse/structural traversal |
| experiments/N62-R1-RESULT-CORRIGENDUM.md | 14828 | `e7fa54afe36051cc518aa0dc3e15b8e1fa62232aae1a480ef2c131e2940754c3` | full Markdown semantic read |
| history/01-history-evidence.md | 19283 | `0235ca53d6c82d2b199772663fb4458e722b5a5a73eeb9248aa22a06622295a4` | full Markdown semantic read |
| history/02-conditional-ngram-continuation.md | 18456 | `6f6b793ea0083672e7cf49b7b7817ac7d40d213f4a27ebdbec7efb7983b89f0a` | full Markdown semantic read |
| history/03-p54-ng01-semantic-verdict.md | 13736 | `b91d2ed8356df0092b89ac2aaa78fe1ca21bb1793e82dc6b4e3b6f9a95aacdbd` | full Markdown semantic read |
| history/04-unmentioned-source-crosswalk.md | 26381 | `b8df318aaacd0e9c09087254e951c07a2ba10296d0d1a07c2d71de3d1e197141` | full Markdown semantic read |
| history/05-concurrent-primary-reconciliation.md | 11811 | `43899b3d12526d3b5a0ab0ab3b395232ce787989caa23cf88eb62fe6cbc91a32` | full Markdown semantic read |
| history/06-core669-gap-status.md | 20232 | `7b43c49f8729c49e8c7538f365548971c355bbb0d34b2cad35c3cf45f72b3b48` | full Markdown semantic read |
| history/07-core827-output-delta.md | 12387 | `34e616620220351ee3df166f2777be6278614b1d64a47e0b3cb531518eb8668c` | full Markdown semantic read |
| history/evidence/core669-gap-source-receipt.json | 717 | `b8c22dada383df52a8732ed99816400812f79533452956a33653ea5c985c97c5` | full bytes + JSON parse/structural traversal |
| history/evidence/core827-changed-output-assignments.json | 6616 | `ed1624560a901341d339003e2966403e04e4ed0a1e3256a0497b3e6d4e46beba` | full bytes + JSON parse/structural traversal |
| history/evidence/core827-source-receipt.json | 1320 | `c419819b4fba10d41761ae146c930fdfcffdc26ccff8e995b69751dac46f05a3` | full bytes + JSON parse/structural traversal |
| history/evidence/current-history-source-manifest.json | 30073 | `e6f7b3d315c16b10c42d0fa7c5265d68f1a407c1319690df5a531f653f885239` | full bytes + JSON parse/structural traversal |
| history/evidence/early-history-receipts.json | 4726 | `9a46eae2c6e6e8d850dbc1543edeab8ed9cf78fa0fb0b3fe82023d3720dbe614` | full bytes + JSON parse/structural traversal |
| history/evidence/ng01-core669-output-mapping.json | 7853 | `1f707ed8835f9fbe3f9485f186a54e68ff69026ed32d998ce3861d88217b355c` | full bytes + JSON parse/structural traversal |
| history/evidence/original-rp01-primary-summaries.json | 12861 | `034c5b17666eb70fcdb43c8b6cd2591f93c9e7fcbda4960473a3254a7d4fd3a1` | full bytes + JSON parse/structural traversal |
| history/evidence/sibling-source-blobs.json | 580 | `16a082f9756768c70fef1121b2757992424cf7b64ffe9beee37b4b2e986b42ff` | full bytes + JSON parse/structural traversal |
| history/evidence/unmentioned-source-crosswalk-116.json | 525697 | `abdd3475a17fcdf93d173f552813b71d990ec22c64b1f793f324a573eb969d79` | full bytes + JSON parse/structural traversal |
| learning/CORE669-DELTA.md | 6214 | `ed0b235016f140ff38d91547d5b91c2b41e3ab4ec1119b50034bfd745b33a7e2` | full Markdown semantic read |
| learning/FROZEN-BROADCAST-MAP-01.md | 18778 | `9cc85202c898530a889b34fb7f5c04faa0e28f6e6dfebee57f2992593038b0fc` | full Markdown semantic read |
| learning/GRAD-ACCUM-COPY-01.md | 17012 | `f9777d8ba305abc88244996f7bb0ee0921be05fcc254e3e9dbdff6fe8054e82a` | full Markdown semantic read |
| learning/learning-evidence.md | 17666 | `800d2658794ccd4ede3e5e51b7c266a3b0af49b3ec28d35157d214b94bc9b981` | full Markdown semantic read |
| learning/learning-novelty.json | 237841 | `f1010e1b0e8ec4c8afb1b4861dcb62c1482096cbc3f3a7922460fe15831050bd` | full bytes + JSON parse/structural traversal |
| learning/TENSOR-LEARNING-EXECUTION-CONTRACT.md | 30109 | `be7f813c675ae7c9d781a3f120479ac10cc744d0516076d5d86dca2e34a94e6e` | full Markdown semantic read |
| README.md | 11567 | `3c16ee055f15e855ea64b10f0322065fdefb8d56b6b3091cd07b0d679620150f` | full Markdown semantic read |
| source-and-review.md | 11204 | `765c96a52d978f1a946cc8bf8197812eecbdc6f4a66c972aee1cb207d1f355e1` | full Markdown semantic read |
| C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/00-LEARNING-EVIDENCE-AND-NEXT-EXPERIMENTS-2026-10-20.md | 2261 | `13a79e01acd61590d07fbde43ce3572d588a890d0944a976c87ec3348452f72a` | full text semantic read |
| C:/Users/lubao/.codex/attachments/22ca37e7-ce41-4cae-8272-a4b2bc62a4bf/Pasted text.txt | 14570 | `9f6c0f2256676e4f6eafcc27432631c67539e8b97126bee0a4516b9e853373af` | full text semantic read |
| C:/Users/lubao/.codex/attachments/ebafc613-16ad-495a-b593-62274b65646c/Pasted text.txt | 29408 | `b8a6336213285741dcbea76e029d8c5d83be08ff301f19b7b72374fcbf2aaecd` | full text semantic read |

## Финальный readback eaed после d7

Последовательно прочитан current shared27 целиком (221 строка), FROZEN card/prompt и конкретные закрывающие правки. `experimets/cards/FROZEN-BROADCAST-MAP-01.md:18` теперь ставит branch строго до allocation mappedBIndices. `experimets/27-tensor-learning-execution-contract.md:59,195–218` закрепляет host/train.jsonl,32-byte host salt, все13request fields, actual native run-ID predicate, одного experimental study owner; выдуманный native paired-store отсутствует. Строки162–177 содержат114-source closure, ABI boundary, assigned loader и fresh third-model readback. Ничего из этого не вызывалось.

Финальный FROZEN:8 теперь40/7/33,NEXT37VT→RMS38→IDENTITY39→KN40; общий NEXT08. Ранее перечисленные адресные замечания закрыты содержимым текущих документов. Дополнительная d7-порция и её ограничения — [отдельный evidence](2026-10-20-d7-evidence.md). Это один адресный worker review, не независимые повторные циклы. Runtime0, source/product/Git mutations0. Изменялись только два evidence Markdown.