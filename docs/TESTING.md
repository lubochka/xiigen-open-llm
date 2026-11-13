# Тестирование

## Регрессия прав промежуточного кандидата обученных весов в транзакции обучения на предпочтениях

В транзакционном пути обучения на предпочтениях (DPO) регрессия проявлялась
так: тренер инкрементального обучения, `IncrementalTrainer`, сохранял
промежуточный файл кандидата обученных весов через сохранение адаптера по
строковому пути, `LlmModel.SaveAdapterCheckpoint(string)`. Этот путь выдавался
резервированием кандидата, `FileDpoPairStore.CandidateReservation`, но сам файл
создавался не защищённым хранилищем. Поэтому на Linux он мог получить права
доступа шире, чем «читать и писать может только владелец файла». Проверка
кандидата, `InspectCandidate`, честно отвергала такой файл через проверку
безопасных прав Unix, `HasSafeUnixFileMode`. Следующий шаг видел уже начатую
мутацию без принятого точного кандидата и возвращал
`DPO_TRANSACTION_RECOVERY_REQUIRED`.

Исправление: транзакционный тренер больше не создаёт промежуточный файл
кандидата обученных весов сам. Он передаёт модель в сохранение кандидата,
`CandidateReservation.SaveAdapterCheckpoint`, а защищённое хранилище пар,
`FileDpoPairStore`, под текущей арендой права мутировать домен создаёт файл
через поток с доступом только владельцу. После открытия хранилище проверяет,
что открытый файловый дескриптор ведёт именно к ожидаемому файлу, проверяет
права Linux, размер и формат сохранённых весов, а затем выполняет
принудительную запись на диск, `Flush(true)`. Это не ослабляет проверку
кандидата, `InspectCandidate`, проверку безопасных прав Unix,
`HasSafeUnixFileMode`, или поведение «требуется восстановление»,
`DPO_TRANSACTION_RECOVERY_REQUIRED`; нетранзакционный файл адаптера
`<domain>.lora` остаётся на прежнем пути.

Короткая проверка этого среза:

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~IncrementalTrainerDpoWiringTests.AdmittedFakeDpoAndFakeSupervisedStep_CreateDurableCandidateWithoutFinalRegistryPublication --no-restore --nologo
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~IncrementalTrainerDpoWiringTests.ConfiguredDpoStepCount_ReachesIncrementalTrainerExecutorRequest|FullyQualifiedName~IncrementalTrainerDpoWiringTests.DpoArtifactAcceptance_UsesActualStepWithinConfiguredMaximum|FullyQualifiedName~IncrementalTrainerDpoWiringTests.DpoExecutionsUseFreshPerRunDirectoriesAndPreserveEarlierArtifactPairs|FullyQualifiedName~IncrementalTrainerDpoWiringTests.Pipeline_MultiDomainRoutesEveryPairExactlyOnceWithoutDrop|FullyQualifiedName~IncrementalTrainerDpoWiringTests.Pipeline_TransactionCapDrainsExactSnapshotAndLeavesRemainderReservable|FullyQualifiedName~IncrementalTrainerDpoWiringTests.ProductionTp1Composition_ConfiguredStepTwoPublishesStepTwoDpoArtifacts" --no-restore --nologo
```

Зелёный результат доказывает только этот срез с промежуточным кандидатом
обученных весов и связанные пути связывания компонентов. Он не заменяет полный
набор тестов и не доказывает публикацию в итоговый реестр моделей.

## Cross-process проверка repository-owned DPO continuation

`IncrementalTrainerDpoWiringTests.RepositoryOwnedResumedTraining_VstestChildProcessReloadsPublished000002AndPublishes000003`
проверяет не ещё один in-process объектный граф, а настоящую границу процесса.
Родительская часть теста создаёт temporary repo-shaped root, запечатывает DPO
dataset и публикует LoRA-bearing checkpoint `000002`. Дочерний запуск стартует
как отдельный `dotnet vstest` process и получает только пути плюс serialized
model/training/LoRA configuration; bytes checkpoint-а, модели, delegates,
transaction и factory objects не могут прийти из памяти родителя.

В child process заново создаются production retained factory,
`RepositoryOwnedModelCheckpointLoader`, `IncrementalTrainer`, durable DPO
transaction и checkpoint publisher. Успешная ветка обязана загрузить точный
sealed `000002`, выполнить accepted tiny continuation и опубликовать sealed
`000003`. После выхода дочернего процесса родитель заново открывает `000003`,
хэширует weights и проверяет lineage: parent identity указывает на тот же
`000002`, а DPO identity указывает на sealed
`training-data/dpo/bug/incremental-trainer-dpo-wiring/v0003/manifest.json`.

Отрицательная ветка находится в том же xUnit fact: LoRA-bearing parent без
explicit LoRA config даёт `TRAINING_SESSION_CREATION_FAILED` до выполнения
trainer-а. Поэтому DPO executor и supervised executor имеют ноль вызовов,
legacy retained base checkpoint не открывается, и `000003/weights.bin` /
`000003/manifest.json` не создаются.

Focused-команда после Release build:

```powershell
dotnet vstest Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll --TestCaseFilter:"FullyQualifiedName~IncrementalTrainerDpoWiringTests.RepositoryOwnedResumedTraining_VstestChildProcessReloadsPublished000002AndPublishes000003"
```

Граница доказательства узкая: tiny deterministic fixtures, без real Chinese
model/teacher, без production DPO corpus, без real trained weights, без
model-quality claim и без fresh clone/Git LFS materialization. Тест пишет
repo-shaped `training-data`/`training-artifacts` только внутри temporary fixture,
не создаёт canonical repo artifacts и не является причиной добавлять ignore
rules: ничто под `training-artifacts` не должно быть скрыто git-игнорами.

## Восстановление оборванной публикации Translation DPO checkpoint

Восемь focused-проверок держат один человеческий контракт: если публикация
Translation DPO checkpoint оборвалась, повтор должен закончить тот же
repository-owned step, а не обучить следующий и не угадать чужие bytes.
`Run_RepositoryOwnedModelCheckpointPendingTrainerFilesSealWithoutRetraining`
создаёт trainer policy/metrics, orphan `weights.bin` с теми же bytes и
bound pending journal для первого checkpoint. Runner обязан допечатать manifest,
загрузить восстановленную policy, написать отчёт и удалить journal только после
успешного отчёта. `Run_RepositoryOwnedModelCheckpointPendingPublicationRecoversContinuationStepAfterParent`
добавляет sealed step 1 и оборванный step 2: recovery должен загрузить parent,
опубликовать step 2 и не создать step 3.

Две дополнительные TOCTOU-проверки закрывают окно «проверил path, закрыл,
потом открыл уже другой файл». `Run_RepositoryOwnedModelCheckpointPendingPublicationConsumesHeldParentWeights`
пытается заменить parent `weights.bin` после hash/length-проверки; runner
обязан либо получить отказ ОС на замену, либо всё равно загрузить уже удержанные
bytes. `Run_RepositoryOwnedModelCheckpointPendingPublicationConsumesHeldTrainerArtifacts`
так же пытается заменить trainer policy и metrics после проверки journal-bound
hash/length; sealer, JSON metrics reader и `LlmModel.LoadCheckpoint` должны
потребить тот же открытый stream, а не второй раз довериться path-у.

Отрицательные проверки защищают границу от самообмана. Foreign orphan weights
не совпадают с trainer policy — manifest не пишется. Trainer/weights без
pending journal — ambiguous state, runner не угадывает владельца bytes.
Tampered metrics после hash-binding — fail-closed, потому что journal больше не
соответствует trainer-файлу. Report-write retry моделирует падение после
`weights.bin`/`manifest.json`, но до JSON-отчёта: journal остаётся, повтор
переписывает только отчёт того же step и не начинает следующий checkpoint. Эти
тесты доказывают только маленький deterministic recovery-срез в temporary
fixture. Они не запускают Qwen, Ollama или реальное обучение, не создают
canonical production weights и не заменяют полный прогон всех translation tests.

Focused-команда:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointPendingTrainerFilesSealWithoutRetraining|FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointPendingPublicationRejectsForeignOrphanWeights|FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointPendingPublicationRejectsAmbiguousArtifactsWithoutJournal|FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointPendingPublicationRecoversContinuationStepAfterParent|FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointPendingPublicationConsumesHeldParentWeights|FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointPendingPublicationConsumesHeldTrainerArtifacts|FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointPendingPublicationRejectsTamperedMetricsBinding|FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointReportWriteRetryFinishesSamePublishedStep"
```

TOCTOU-команда для двух held-stream тестов:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointPendingPublicationConsumesHeldParentWeights|FullyQualifiedName~TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointPendingPublicationConsumesHeldTrainerArtifacts"
```

## Fresh-clone проверка repository-owned model checkpoint через Git LFS

`RepositoryOwnedTrainingArtifactLayoutTests.ModelCheckpointGitLfs_FreshCloneMaterializesWeightsAndLoaderOpensLatestCompatible`
проверяет последний кусок публикационной цепочки без реального обучения.
Тест создаёт только temporary source repo, bare remote и fresh clone; в рабочем
дереве проекта fixture weights не появляются. Временный repo получает тот же
LFS-атрибут, что canonical repo: `training-artifacts/models/**/weights.bin`
хранится через Git LFS, а соседний `manifest.json` остаётся обычным текстовым
файлом.

Зелёная проверка доказывает пять вещей: `weights.bin` получает LFS filter,
объект в Git commit является LFS pointer, локальный bare remote получает LFS
object, свежий clone материализует исходные маленькие bytes вместо pointer-а,
а `RepositoryOwnedModelCheckpointLoader.LoadLatestCompatible` открывает cloned
checkpoint как latest compatible по тем же runtime/DPO fingerprints. Если
`git lfs` недоступен, тест должен упасть с явной причиной prerequisite failure;
skip не считается доказательством.

Важная Unix-граница: Git и Git LFS переносят содержимое файлов, но не являются
custody-provisioner-ом для owner-only прав, которые требуют repository-owned
readers. Поэтому production primitive
`RepositoryOwnedTrainingArtifactCloneCustodyPreparer` запускается перед loader-ом
и нормализует права только в temporary clone и только для выбранных
repository-owned roots. Public `PrepareForStrictReaders` покрывает
`training-data/dpo`, `training-artifacts/models` и
`training-data/reference-corpora`; internal DPO-only/model+DPO contexts для
текущих DPO/model readers сохраняют старую узкую семантику и reference root не
трогают. Доказательство покрывает LFS materialization плюс открытие loader-ом
после обязательной production custody preparation; оно не доказывает готовую
deployment/startup automation вокруг этой primitive.

Focused-команда:

```bash
dotnet test /mnt/c/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj --no-build --nologo --filter FullyQualifiedName~RepositoryOwnedTrainingArtifactLayoutTests.ModelCheckpointGitLfs_FreshCloneMaterializesWeightsAndLoaderOpensLatestCompatible
```

Дополнительная команда для самой preparation boundary:

```bash
dotnet test /mnt/c/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj --no-build --nologo --filter FullyQualifiedName~RepositoryOwnedTrainingArtifactLayoutTests.CloneCustodyPreparer
```

Честная граница: bytes в этом тесте — маленькая deterministic fixture, не
real NegativeDpoGenerator weights, не Qwen/Ollama run и не production training.
Проверка не добавляет ignore rules: `training-artifacts/` остаётся целиком
Git-visible, включая lock/temp/log/TestResults/bin/obj и model weights.

## Cross-process проверка создания repository-owned DPO dataset

`RepositoryOwnedDpoDatasetManifestTests.AsyncResolver_GitCloneStyleTrackedLeaseVstestChildProcessesReserveOnceReuseDiskAndFailClosed`
проверяет отдельную вещь: не продолжение обученной модели, а создание и
переиспользование самого DPO-набора, который должен жить в репозитории. Родитель
создаёт temporary repo-shaped root и заранее кладёт в
`training-artifacts/.dpo-dataset-creation-leases/...` стабильный однобайтовый
lock так, как он мог бы прийти из обычного Git clone: каталоги с правами `0755`,
файл с правами `0644`. Это не ignored runtime-мусор; всё под
`training-artifacts` остаётся trackable.

Два дочерних `dotnet vstest` процесса одновременно запрашивают один и тот же
отсутствующий DPO dataset. Успешная ветка обязана вызвать creator ровно один раз,
запечатать DPO под `training-data/dpo/...`, а оба процесса обязаны открыть один
и тот же sealed manifest и тот же pair-file hash с диска. Затем третий отдельный
процесс повторяет запрос и должен получить reuse с нулём creator-вызовов. После
этого mismatch-сценарий с другой request identity обязан fail-closed: без
перезаписи, без повторной генерации и без появления второго DPO вместо уже
запечатанного.

Linux-часть теста дополнительно доказывает режим свежего clone: resolver
безопасно сужает права только у точной цепочки
`.dpo-dataset-creation-leases/<domain>/<design-pattern>` и у точного lock-файла,
через no-follow/open-handle путь. Корень `training-artifacts` не chmod'ится
целиком, потому что реальные DPO и checkpoints/weights должны оставаться
обычными репозиторными артефактами. Честная граница: creator в тесте
детерминированный и маленький; реальная китайская teacher model, качество DPO,
скорость обучения и точность модели этим тестом не проверяются.

Отдельный regression-тест
`RepositoryOwnedDpoDatasetManifestTests.Resolver_ExistingZeroByteCreationLeaseLockFailsClosedWithoutRewriteOrCreator`
проверяет повреждённый tracked lock: если файл уже существует, но пустой, resolver
не считает его «только что созданным», не записывает в него правильный байт, не
зовёт creator и не создаёт dataset. Это защищает правило «ничего под
`training-artifacts` не игнорируется»: раз lock версионируется, код обязан
безопасно отличать нормальный однобайтовый lock от повреждённого файла.

Focused-команда после Release build:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~Resolver_ExistingZeroByteCreationLeaseLockFailsClosedWithoutRewriteOrCreator|FullyQualifiedName~AsyncResolver_GitCloneStyleTrackedLeaseVstestChildProcessesReserveOnceReuseDiskAndFailClosed"
```

## Адресация и sealed open frozen reference corpora

`RepositoryOwnedTrainingArtifactLayoutTests.ReferenceCorpusAddress_*` проверяют
адресный контракт для замороженных эталонных корпусов:
путь обязан быть
`training-data/reference-corpora/<domainSlug>/<subjectSlug>/<corpusVersion>/`,
а manifest — соседним `manifest.json`. Проверки покрывают детерминированные
repo-relative paths, отказ fabricated address у `IsValidReferenceCorpusAddress`,
отказ unsafe slug без создания дерева, отказ repository root без sentinels,
отказ на observed reparse point и отказ на ordinary-file collision в
обязательной цепочке директорий.

`RepositoryOwnedTrainingArtifactLayoutTests.CloneCustodyPreparer_*Reference*`
проверяют custody-границу для этого root: public `PrepareForStrictReaders`
сужает существующий clone-like `training-data/reference-corpora`, сообщает
именно этот repo-relative root, повторный запуск идемпотентен, а symlink/hardlink
leaf внутри root fail-closed там, где platform helper реально создал такой
объект. Отдельный compact test фиксирует, что internal DPO-only и model+DPO
contexts не готовят clone-like reference root.

`RepositoryOwnedReferenceCorpusManifestTests` проверяет реализованный
disk-owned sealer/opener поверх этого адреса. Успешный сценарий пишет
`corpus.json`, затем `manifest.json`, зануляет in-memory corpus reference и
fresh-open восстанавливает эквивалентный `FrozenReferenceCorpus` с теми же
entry ids, train/held-out split и paid-reference prohibition flags. Negative
сценарии fail-closed покрывают tampered payload byte, manifest self-hash,
manifest identity, entry count и unsafe payload path escape. Отдельно проверены
идемпотентный повтор с теми же bytes, conflict для другой версии content под тем
же address и отказ address/corpus version mismatch без создания дерева.

`RepositoryOwnedTrainingArtifactLayoutTests.ReferenceMatchingCalibrationAddress_*`
проверяет deterministic адрес для persisted calibration numbers:
`training-data/reference-corpora/<domain>/<subject>/<corpusVersion>/calibrations/<ruleId>/<ruleIdentitySha256Hex>/<calibrationSetSha256Hex>/manifest.json`.
Unsafe slug/hash inputs fail before any `training-data` tree is created.

`RepositoryOwnedReferenceMatchingCalibrationManifestTests` проверяет новый
manifest-only sealer/opener. Успешный сценарий сначала seals linked
`FrozenReferenceCorpus`, затем пишет calibration manifest, reopens it, and checks
separate false-miss and false-hit IDs/rates. Negative сценарии fail-closed
покрывают manifest self-hash, linked-corpus path escape, corpus hash/count,
rule identity/version, known-set counts, false-miss and false-hit counts/rates/
id-list and result-hash tampering, known-positive/known-negative PairId
substitution, cross-set intersection and non-canonical rule description
whitespace, plus missing/tampered linked sealed corpus. Отдельно проверены
idempotent repeat, conflict when a different valid
result is written to the same address, and absence of raw paid/candidate answer
bytes in the calibration manifest.

The same test class now covers the domain-worker convenience entrypoint:
`EvaluatePersistAndSeal` evaluates the matching predicate, derives the exact
repository path and reopens the manifest at the same address. It also proves that
a wrong subject or missing linked sealed corpus does not call the predicate and
does not create a parallel manifest tree, and that predicate failure stops before
writing any calibration file.

`ReferenceMatchingRuleCalibrationTests` фиксирует `ResultSha256` canonical
schema v2: the hash includes the complete ordered known-positive and
known-negative PairId membership, not just denominators and false-id outcomes.

Focused-команда для layout/custody:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --filter "FullyQualifiedName~RepositoryOwnedTrainingArtifactLayoutTests.ReferenceCorpusAddress|FullyQualifiedName~RepositoryOwnedTrainingArtifactLayoutTests.ReferenceMatchingCalibrationAddress|FullyQualifiedName~RepositoryOwnedTrainingArtifactLayoutTests.GitIgnore_DoesNotHideRepositoryOwnedTrainingHomes"
```

Focused-команда для sealer/opener:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~RepositoryOwnedReferenceCorpusManifestTests|FullyQualifiedName~RepositoryOwnedReferenceMatchingCalibrationManifestTests" --no-restore --nologo --verbosity quiet
```

Честная граница: эти тесты создают только маленький deterministic
`FrozenReferenceCorpus` fixture в temporary repo-shaped root. Они доказывают
адресацию, fail-closed форму пути, public clone-custody для reference root,
Git-visible `corpus.json`/`manifest.json`, strict reopen и отсутствие
`.gitignore`-скрытия root. Они не создают настоящие paid-model answers, не
запускают Qwen/Ollama, teacher/model inference, DPO generation или training run,
не публикуют реальные corpus bytes в canonical checkout и не доказывают качество
будущего matching rule.

`LanguageTranslateReferenceMatchingRuleTests` проверяет первый предметный
translation adapter поверх generic frozen-reference calibration contract. Fixture
создаёт только in-memory fake paid provenance с явным test-only комментарием:
это не production paid corpus и не bytes под
`training-data/reference-corpora/**`. Focused evidence покрывает deterministic
identity для `language-translate`, scorer normalization/metric profiles,
threshold/policy, self-reference false miss 0, same-domain foreign answer false
hit 0 при strict policy, threshold/candidate mutation sensitivity,
fail-closed invalid/oversized/invalid-Unicode scorer input и generic rejection
чужого entry/reference hash до появления calibration numbers.

Focused-команда для translation reference matching rule:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --nologo --filter "FullyQualifiedName~LanguageTranslateReferenceMatchingRuleTests"
```

`DesignConversionReferenceMatchingRuleTests` проверяет design-to-code adapter
поверх generic frozen-reference calibration contract. Fixture создаёт только
in-memory fake paid provenance с явным test-only комментарием; это не production
paid corpus, не DPO row, не model call, не weights, не checkpoint и не training
artifact. Focused evidence покрывает deterministic identity для
`design-conversion`/`design-tree-to-markup`/`design-to-code`, binding policy
`candidate expected_image_id/SHA-256 == frozen paid_reference expected_image_id/SHA-256`,
`expected_markup_fingerprint` binding, local-PNG-only field-bound refs, no-match
для high-score decoy с foreign expected reference, mutation sensitivity по
expected SHA, markup fingerprint и visual/markup thresholds, fail-closed
self-comparison image id/SHA, missing markup, malformed/wrong-schema/extra-field
raw JSON, swapped roles, wrong subject/pattern, duplicate evidence refs,
NaN/infinity/out-of-range scores, copied-marker-only metadata echo, local-policy
rejection для external receipt marker, wrong-rule identity и wrong corpus subject
до появления metrics, и false-hit 0 через общий
`ReferenceMatchingRuleCalibrationEvaluator`.

Focused-команда для DesignConversion reference matching rule:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --nologo --filter "FullyQualifiedName~DesignConversionReferenceMatchingRuleTests"
```

`ProjectWikiCodeDocumentationReferenceMatchingRuleTests` проверяет ProjectWiki
frozen-reference adapter для C# documentation evidence ids,
`ProjectWikiAngularCodeDocumentationReferenceMatchingRuleTests` проверяет
отдельный Angular adapter, а
`ProjectWikiSqlCodeDocumentationReferenceMatchingRuleTests` проверяет отдельный
SQL adapter. Fixture создаёт только in-memory fake paid provenance; он не
публикует real paid corpus, не вызывает teacher/model, не генерирует DPO и не
запускает training. Тесты доказывают canonical payload shape `schema` →
`subject_id` → `answer` → `evidence_ids`, ordinal sorting, exact-set match при
другом порядке ids, no-match для missing и extra ids, fail-closed
duplicate/malformed/wrong-schema/unsafe subject/unsafe ids/oversized input,
empty-set semantics, wrong-rule identity rejection before calibration numbers,
corpus subject rejection before calibration numbers, same-domain foreign
evidence false-hit 0, mutation sensitivity и то, что `FromAnswer` берёт только
явный subject плюс typed `ProjectWikiAnswer.EvidenceIds`, не regex из prose.
Отдельные C#, Angular и SQL tests используют одинаковые evidence-id строки для
C#/Angular/SQL payloads, чтобы доказать: один общий DomainTag
`project-local-wiki` сам по себе больше не смешивает subjects. SQL tests
дополнительно проверяют, что `.sql`, `procedure:` и SQL-looking prose не
являются authority без typed subject.

Focused-команда для ProjectWiki C#/Angular/SQL reference matching rules:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --nologo --filter "FullyQualifiedName~ProjectWikiSqlCodeDocumentationReferenceMatchingRuleTests|FullyQualifiedName~ProjectWikiAngularCodeDocumentationReferenceMatchingRuleTests|FullyQualifiedName~ProjectWikiCodeDocumentationReferenceMatchingRuleTests"
```

Честная граница рядом с ProjectWiki answer composer: существующее поле
`ExactEvidenceAccuracy` в `ProjectWikiAnswerComposerTrainingRunner` остаётся
историческим subset gate — оно проверяет, что required evidence ids присутствуют
у ответа, но не запрещает лишние ids. Новый frozen-reference instrument
отдельный и stricter: candidate/reference payloads должны иметь exactly equal
evidence-id sets.

Batch routing рядом с этим срезом закреплён точными class-boundary terms:
`FrozenReferenceCorpusTests` и `ReferenceMatchingRuleCalibrationTests` идут в
существующей группе `teaching-translation-dpo`, потому что они проверяют generic
calibration contract для дальнейшего обучения/translation/DPO boundary, а
`RepositoryOwnedReferenceCorpusManifestTests` идёт в
`repository-owned-training-artifacts`, потому что это storage sealer/opener для
repository-owned frozen reference corpus bytes.

## Быстрый targeted-прогон по изменённым тестовым файлам

Если небольшая правка изменила один или несколько тестовых файлов, сначала
можно получить человеческое объяснение, какие основные группы затронуты:

```powershell
.\tools\test-batches\select-impacted-batches.ps1 -ChangedSince origin/codex/dpo-implementation
```

`-ChangedSince` подходит для уже зафиксированной дельты между коммитами. Для
грязного рабочего дерева передавайте конкретные файлы через `-ChangedPath` или
`-ChangedPathsFile`, потому что незакоммиченные изменения ещё не видны
`git diff --name-only <base> HEAD`.

Для нескольких грязных путей используйте `-ChangedPath path1,path2` или файл
со списком через `-ChangedPathsFile`. Запускалки отключают позиционные
параметры, чтобы второй путь не мог случайно стать `RepoRoot` или другим
служебным аргументом.

Переданные пути приводятся к полному пути внутри текущего репозитория:
повтор одного файла в другом регистре учитывается один раз, а путь за пределами
рабочей копии останавливает selector. Так targeted-прогон не сможет случайно
проверить “похожий” файл из соседней папки.

Та же логика встроена в PowerShell-запускалку. Она сама вычислит `-Only` из
живого C#-разбора тестовых классов и текущего списка групп, затем передаст в
WSL-runner только выбранные batch id:

```powershell
.\tools\test-batches\run-test-batches.ps1 -ChangedSince origin/codex/dpo-implementation -NoBuild -MaxLeaves 2
```

Тестовые C# harness-ы, которые напрямую запускают `.ps1`, используют общий
test-only resolver PowerShell. На Linux/WSL допустим только PowerShell Core из
`PATH`: сначала обычное Unix-имя `pwsh`, затем Windows-style имя `pwsh.exe`.
Второй вариант нужен для WSL: Codex runtime может добавить Windows PowerShell
Core в `PATH` именно как `pwsh.exe`, хотя это всё ещё новый PowerShell Core, а
не старый Windows PowerShell. Linux/WSL не должны уходить в
`powershell`/`powershell.exe`, потому что это Windows PowerShell 5 и он может
спрятать portability-дефект вместо честного красного теста. На Windows сначала
проверяется `%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe`,
затем `PATH` в порядке `powershell.exe`, `powershell`, `pwsh.exe`, `pwsh`.
Если shell не найден, тест падает явным `InvalidOperationException` со списком
проверенных кандидатов. Такая зелёная точечная проверка доказывает только
переносимый запуск соответствующих PowerShell-based harness-ов, а не общий
статус всего batch-runner-а или product suite.

Когда WSL запускает Windows `pwsh.exe`, test harness не вызывает `wslpath` в
отдельном процессе: он сам строит Windows-путь. `/mnt/<диск>/...` становится
обычным `C:\...`, а Linux-пути вроде `/tmp/...` становятся UNC-путями
`\\wsl.localhost\<дистрибутив>\...`; имя дистрибутива берётся только из
`WSL_DISTRO_NAME` и проверяется как безопасный UNC-сегмент. Если это имя
отсутствует или путь нельзя преобразовать без догадки, запуск падает явно.
Сгенерированный launcher первого локального этапа обучения применяет ту же
границу только к известным файловым значениям: `server_command_path`, значениям
после `--ready-file`, `--artifact-db-root`,
`--measurement-gate-thresholds-file` и шести переменным окружения с путями
checkpoint/tokenizer/adapter/cache/registry/DPO-pairs. Токены, salt, digest,
model id, refs и остальные непутевые значения остаются байт-в-байт теми же;
private JSON не переписывается во время запуска.

В extension-token-quality artifact JSON приватные ссылки могут сохранять
Windows-написание `C:\Projects\llm_mvp_core\...`; Linux/WSL не должен читать
такую строку как относительный файл. Producer доказательных файлов в этой
группе пишет устойчивую ссылку на текущий canonical repo, а не runtime-путь
конкретного WSL-запуска. Потребители artifact-ссылок используют общий
test-only resolver: refs текущего canonical repo переводятся в текущий
`RepoRoot`, relative refs под `artifacts/...` тоже читаются от `RepoRoot`, а
sibling-файлы вроде `request.json` выводятся только после resolution
response-файла. Ссылки на старый sibling `llm_mvp_core-DpoImplementation` и
внешние repo fail-closed через `InvalidOperationException`.

Tracked-доказательства этой группы должны быть идемпотентными: если код,
исходные данные и выбранный batch не изменились, повторный запуск не имеет права
переписывать committed JSON только из-за текущих часов или из-за того, что тест
стартовал из WSL. Для этого production-машины PromptToTask, OrchestrationReasoning
и SkillSelection по умолчанию сохраняют реальное UTC-время, но тестовые fixtures
передают фиксированный `savedAtUtc`. Все repo-owned поля `source_state_ref`,
`output_files` и loader diagnostics пишутся в едином виде
`C:\Projects\llm_mvp_core\...`; внешние пути остаются внешними и не маскируются.

То же правило действует для baseline-кандидата PromptToTask из обучающего
разбиения: тест создаёт committed checkpoint, manifest, lifecycle export и
отчёты с фиксированным `savedAtUtc`. Первый запуск после чистого checkout,
повторный запуск из Windows и повторный запуск из WSL должны оставлять один и
тот же канонический путь `C:\Projects\llm_mvp_core\...` и не менять SHA-256
tracked-доказательств, если не менялись входные строки или код построения
кандидата.
Проверка такого слайса: запустить `extension-token-quality`, снять SHA-256 всех
tracked-файлов под `artifacts/extension-runtime-token-quality-learnability`, затем
запустить тот же batch ещё раз и убедиться, что второй прогон не изменил ни один
байт tracked-доказательств.

Зелёный результат такого среза доказывает только переносимость чтения текущих
artifact refs и защиту от старого sibling checkout. Он не означает, что уже
починены следующие отдельные красные проверки этой же группы: пользовательский
экран улучшений всё ещё должен реально показать adaptive task/plan/skills.

### Self-contained bundle для OrchestrationReasoning checkpoint

`OrchestrationReasoningMachine.SaveCheckpointAsync` публикует не одиночный JSON
и не только тройку `checkpoint + manifest + source-admission-proof`. Живой
переносимый артефакт — это весь каталог checkpoint bundle: рядом с checkpoint и
manifest лежит proof, а под bundle-local source-material refs лежат копии
payload, source manifest, admission ledger и admission evidence. Checkpoint и
proof должны ссылаться только на эти относительные bundle-local пути; временный
corpus, из которого состояние было обучено, после сохранения не является частью
доверенной загрузки.

Поэтому portability-тест копирует или переносит весь каталог bundle. Копирование
только `*.json` triple без sibling proof file и без реальных
`source-materials/<n>/payload.jsonl`,
`source-materials/<n>/manifest.json`,
`source-materials/<n>/source-admission-ledger.json`,
`source-materials/<n>/source-admission-evidence.json` не доказывает
переносимость и должно fail-closed на fresh load. При fresh load proof заново
открывает bundle-local source materials, сверяет SHA-256 payload/manifest/ledger/
evidence с receipt и заново собирает learned-state/admission; rooted
Windows/Unix/UNC refs, `..` traversal и несовпадение persisted manifest refs с
proof refs должны останавливаться до использования окружающего checkout или cwd.
Сохранение публикует `payload + source manifest + ledger + evidence + proof +
checkpoint + manifest` одной транзакцией: при ошибке или отмене новые файлы и
пустые каталоги bundle, созданные только этой попыткой, откатываются вместе.

### Переносимый hash-контракт для TOKEN-04 scored JSON projection

TOKEN-04 `output_hash` считается по узкому indented JSON projection ответа, а
не по полному response-файлу. У `System.Text.Json` indented newline зависит от
платформы: Windows даёт CRLF, Linux/WSL — LF. Поэтому одинаковый JSON мог
получать разные SHA-256 при пересчёте в WSL, хотя содержательно projection не
менялся.

Контракт для scored projection теперь явный: перед `output_hash` все реальные
переводы строк внутри projection нормализуются в canonical CRLF. Это сохраняет
исторические committed hashes TOKEN-04 и делает генерацию/проверку одинаковой
на Windows и Linux/WSL. Файловые артефакты, которые уже пишутся как UTF-8 без
BOM с LF, остаются отдельным файловым контрактом; CRLF относится именно к
строке projection, которая хешируется.

Генерация capture/input rows для adaptive-effect-gap-02:

```powershell
pwsh ./tools/token-04-rerun-after-quality-gap-repair-01.ps1 -RunId "token-04-rerun-after-adaptive-effect-gap-02" -PhaseId "TOKEN-04-RERUN-AFTER-ADAPTIVE-EFFECT-GAP-02" -OutputPrefix "token-04-rerun-after-adaptive-effect-gap-02" -UserAdaptiveAssetRoot "artifacts/extension-runtime-token-quality-learnability/adaptive-effect-gap-02/user-cache"
```

Scoring generated input rows:

```powershell
pwsh ./tools/token-quality-full-host-scorer.ps1 -InputPath "artifacts/extension-runtime-token-quality-learnability/token-04-rerun-after-adaptive-effect-gap-02-quality-scoring-inputs.jsonl" -PairInputPath "artifacts/extension-runtime-token-quality-learnability/token-04-rerun-after-adaptive-effect-gap-02-fresh-pair-results.jsonl" -PhaseId "TOKEN-04-SCORE-AFTER-ADAPTIVE-EFFECT-GAP-02" -OutputPrefix "token-04-score-after-adaptive-effect-gap-02" -NextBoundedWorkOrder "QUALITY-SEMANTIC-GATES-03: adaptive effect is now visible, but quality gates and semantic-quality gates must still be repaired/evaluated before token savings or semantic claims."
```

Минимальная WSL-проверка этого контракта:

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~ExtensionToken04QualityScoringArtifactsTests.ScoredJsonProjectionHash_CanonicalizesLfAndCrlf --no-restore --nologo
dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~ExtensionToken04QualityScoringArtifactsTests.Token04ScoreAfterAdaptiveEffectGap02Artifacts_ShowAdaptiveEffectButNoQualityOrTokenClaims --no-restore --nologo
```

Эти проверки доказывают только переносимый projection hash и повторную
проверяемость текущего 108-row artifact. Они не доказывают token savings,
semantic quality, learnability, training, checkpoint promotion, locator update
или final extension readiness.

### Узкая переносимость чтения host source для проверки quality semantic gate 19

Проверка quality semantic gate 19 читает source/launcher файлы трёх известных
host-репозиториев только через отдельный test-only whitelist: Claude, Cursor и
Codex host adapters. Этот whitelist содержит ровно шесть canonical Windows
refs и переносит их под runtime-родителя текущего `RepoRoot`, поэтому тот же
тест читает `C:\Projects\...` на Windows и соответствующий runtime-путь в
Linux/WSL без hardcode `/mnt/c`.

Эта переносимость относится только к inspection test-у. Она не расширяет общий
artifact resolver `ExtensionRuntimeTokenQualityArtifactPaths`, не разрешает
чтение произвольных sibling repo и не меняет product-поведение `TargetLocator`.
Новые host paths добавляются только явной правкой whitelist-а и должны
fail-closed останавливаться, если путь пустой, relative, traversal, относится к
current core repo, unknown host repo или не входит в эти шесть файлов.

### Переносимое чтение source roots для классификатора этапов разработки (`WDA-01`)

Тесты классификатора этапов разработки (`WDA-01`) валидируют, что
source-ledger rows указывают на существующие exact source paths/files во
внешней библиотеке пользователя и в текущем репозитории, а scoring использует
семантические поля source-ledger row. Эта граница не читает document contents
для scoring и не доказывает content-backed classification; доведение
классификатора до content-backed classification остаётся отдельным продуктовым
долгом. Файлы нельзя копировать в `Tests/`, подменять фикстурами или пропускать
на Linux/WSL: если root не найден или в нём нет точных required files, тест
должен fail-closed с сообщением, какие candidates проверялись и каких файлов не
хватило.

Порядок root resolution:

1. Repository source root: сначала `XIIGEN_WDA01_REPOSITORY_ROOT`, иначе поиск
   вверх от `AppContext.BaseDirectory` до solution marker `LlmFromScratch.sln`.
   Root принимается только если в нём есть solution marker и два source files,
   которые используются held-out rows.
2. Orchestrator experiment source root: сначала непустой
   `XIIGEN_WDA01_ORCHESTRATOR_EXPERIMENT_ROOT`; если он задан и неверен,
   resolver fail-closed и не откатывается к legacy. Legacy override
   `XIIGEN_ORCHESTRATOR_EXPERIMENT_ROOT` используется только когда новый env
   отсутствует или пуст. Если оба env отсутствуют/пусты, используется
   Documents-backed library `Documents/xiigen/llm mvp/orchestrator experiment`.
3. External source root: сначала `XIIGEN_WDA01_EXTERNAL_SOURCE_ROOT`, иначе
   Documents-backed library
   `Documents/xiigen/llm mvp/external-sources-training-sets/wave-1/CB4-stage-3-pairs`.

На Windows Documents root берётся из `Environment.SpecialFolder.MyDocuments`.
На Linux/WSL resolver дополнительно проверяет native mount
`/mnt/c/Users/<profile>/Documents/...`: он перечисляет только один уровень
профилей под `/mnt/c/Users`, не сканирует диски рекурсивно и не вызывает
`wslpath`. Если больше одного профиля даёт валидные required files, resolver
fail-closed с `WDA01_AMBIGUOUS_SOURCE_ROOT`, перечисляет найденные roots и просит
задать точный env override. Если env override содержит drive-qualified Windows path вроде
`C:\Users\...\Documents\...`, resolver пробует его как исходную строку и затем
как `/mnt/<drive>/...`, чтобы Linux не воспринимал Windows path как filename.

### Переносимые проверки реестра обучающих адаптеров

Часть проверок `DomainModelRegistry` специально остаётся Windows-only, потому
что она доказывает именно Windows share/handle oracle: один процесс держит
дескриптор файла, а второй путь записи должен fail-closed до публикации. Такой
сценарий нельзя честно перенести на Linux через `FileShare.None`, потому что
Linux не делает те же share-запреты обязательными для соседнего файлового
открытия.

Переносимый lock-контракт на обеих операционных системах доказывает соседняя
межпроцессная проверка: отдельный helper-процесс держит byte-range lock
`.registry.json.lock`, а реестр должен дождаться таймаута, ничего не
опубликовать и затем восстановиться после освобождения lock-а. Именно эта
проверка является общей для Windows и Linux.

Rollback-проверки не используют Windows FileShare как блокер. Они проходят
через internal test seam прямо перед реальным восстановлением старого
`registry.json`: seam бросает `IOException`, publication уже успела оставить
backup или zero-byte absence marker, а следующий запуск реестра обязан поднять
предыдущее состояние и убрать recovery-мусор. В production этот seam всегда
`null`, поэтому рабочее поведение не меняется.

Проверка нечитаемого обычного файла сначала смотрит на способность платформы.
Windows использует эксклюзивный handle. Linux допускается только если probe с
`chmod 000` действительно получает `UnauthorizedAccessException`; privileged
Linux/WSL, где процесс всё равно читает файл, честно получает skip с понятной
причиной. Это лучше, чем зелёный результат от `FileShare.None`, который на
Linux не доказывает нечитаемость обычного файла.

Минимальная WSL-проверка после изменения root resolution:

```bash
cd /mnt/c/Projects/llm_mvp_core
dotnet build Tests/LlmFromScratch.Tests.csproj --no-restore --nologo
dotnet vstest Tests/bin/Debug/net9.0/LlmFromScratch.Tests.dll --TestCaseFilter:"FullyQualifiedName=LlmFromScratch.Tests.WdaOrchestrationExperimentTests.WDA01_DocumentClassification_ClassifiesAllCanonicalSubPhases|FullyQualifiedName=LlmFromScratch.Tests.WdaOrchestrationExperimentTests.WDA01_Wave2CenterBatch001HeldOut_ClassifiesAllRows|FullyQualifiedName=LlmFromScratch.Tests.WdaOrchestrationExperimentTests.WDA01_SourceRootResolver_TranslatesWindowsOverrideToWslMount|FullyQualifiedName=LlmFromScratch.Tests.WdaOrchestrationExperimentTests.WDA01_SourceRootResolver_DiscoversMountedWindowsDocumentsProfileOnLinux|FullyQualifiedName=LlmFromScratch.Tests.WdaOrchestrationExperimentTests.WDA01_SourceRootResolver_RejectsInvalidNewOverrideBeforeLegacy|FullyQualifiedName=LlmFromScratch.Tests.WdaOrchestrationExperimentTests.WDA01_SourceRootResolver_FailsClosedWhenTwoMountedWindowsProfilesAreValid"
```

Для test-only активации кандидата проверки активного product API target binding
(quality semantic gate 17, QSG-17) недостаточно, чтобы файлы checkpoint/manifest
просто существовали, парсились как JSON и имели совпавшие хэши. Кандидат,
созданный проверкой target/rollback/locator candidate set (quality semantic gate
16, QSG-16), считается безопасным для такой локальной активации только если
реальные production loaders приняли все три машины: PromptToTask,
OrchestrationReasoning и SkillSelection. Это важно из-за строгих контрактов:
OrchestrationReasoning требует текущий schema/provenance proof, который можно
rehydrate из source materials; source refs в proof являются переносимыми
относительными путями только внутри дерева checkpoint directory и при загрузке
fail-closed отклоняют пустые, rooted Windows/Unix/UNC и `..` traversal refs до
чтения source files, чтобы candidate asset subtree можно было переносить без cwd
fallback. SkillSelection
требует `checkpointSha256` / `checkpoint_sha256` в manifest и сверяет его с
фактическим checkpoint. Наличие файла, валидного JSON или заранее записанного
hash само по себе не доказывает, что product API сможет загрузить candidate
assets.

Перед QSG-17 test-only activation locator evidence должен пройти строгую
проверку входных refs. Persisted locator обязан содержать ровно три уникальные
машины — PromptToTask, OrchestrationReasoning и SkillSelection — и для каждой
строки обязательны `proposed_target_ref`, `proposed_manifest_ref`,
`target_sha256`, `rollback_ref` и `rollback_sha256`. SHA должны быть 64-символьным
hex, refs должны быть только repo-relative: без rooted Windows/Unix путей, без
`..` traversal, после resolution оставаться внутри точных target/rollback
artifact roots и совпадать с canonical checkpoint/manifest paths из QSG-16
`BuildMachineAssets`. QSG-17 не имеет права достраивать отсутствующий manifest
ref или синтезировать missing SHA из текущего файла: плохое evidence падает до
production loader и host-shaped probes с человеческой диагностикой, а безопасная
активация по-прежнему сверяет persisted hashes с фактическими файлами.
Если сам product API возвращает failed `OperationResult` или пустой response,
QSG-17 также fail-closed записывает диагностическую host runtime row с error
code/message и не разыменовывает response до `IsSuccess`.

`RepositoryModelIndexIntegrityGuardTests` checks the shared `models/index.json`
trust boundary directly. The focused cases cover the live repository index,
one-character manifest/checkpoint hash mutations, missing files, path escape,
duplicate `model_id`, manifest-only rows, and reparse-point traversal where the
platform allows creating the link. The consumer regressions live next to their
domains: `Phase08CommonUserTwoLegLifecycleTests` proves a manifest-only
orchestration registry row is not active, and `TranslationModelAssetTests`
proves a corrupt unrelated existing row blocks publish before `models/index.json`
is rewritten.

Targeted command example after building `Tests/LlmFromScratch.Tests.csproj`:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --no-build --filter "FullyQualifiedName~RepositoryModelIndexIntegrityGuardTests|FullyQualifiedName~Phase08CommonUserTwoLegLifecycleTests|FullyQualifiedName~TranslationModelAssetTests"
```

Для adaptive-effect-gap-02 orchestration cache исходные материалы допуска лежат
внутри самого checkpoint bundle:
`orchestration-reasoning/orchestration-reasoning-v1/source-admission/pattern-*`.
Это не косметика путей, а часть проверки переносимости: checkpoint и manifest
хранят только bundle-local refs, а proof-файл рядом с checkpoint заново читает
payload, manifest, ledger и evidence из этой же папки. Если в таком ref
появляется абсолютный Windows/Unix путь, старый sibling checkout или `..`,
загрузчик должен остановиться до чтения файла.

Та же проверка обязательна для target package
`adaptive-learnability-01/user-cache-continued`, который пересобирает
`ExtensionQualitySemanticGates16CandidateProbeTests`. Для
OrchestrationReasoning каждый source pattern передаёт в trainer не абсолютный
`manifestPath`, а bundle-local `learnedStateManifestRef` вида
`source-admission/pattern-01/manifest.json`. После генерации тест напрямую
читает `checkpoint.json`, `manifest.json` и `checkpoint.source-admission-proof.json`:
composite `learnedState.manifestId` должен состоять из тех же `manifest_ref`,
которые записаны в proof, через `;delta:`. Абсолютные пути вроде `/mnt/c/...`
или `C:\...` в этом поле запрещены, потому что пакет должен переживать перенос
между checkout-ами и операционными системами; production loader правильно
сравнивает checkpoint с proof и fail-closed отклоняет несовпадение.

Узкая проверка этого сценария запускается из WSL в корне
`/mnt/c/Projects/llm_mvp_core`, без restore:

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~ExtensionQualitySemanticGates16CandidateProbeTests --no-restore --nologo
dotnet test Tests/LlmFromScratch.Tests.csproj --filter FullyQualifiedName~ExtensionQualitySemanticGates17ProductRuntimeVerificationTests --no-restore --no-build --nologo
git diff --check
```

Если второй запуск не может использовать уже собранные binaries, допустимо
повторить его без `--no-build`, но не нужно запускать full suite или старый
широкий batch: этот кейс доказывается узкой парой генератор target package →
product-runtime verification.

Обычный тест `ExtensionAdaptiveEffectGap02Tests` не имеет права обновлять
committed fixture. Он создаёт временные owned assets, проверяет API-эффект и в
конце сравнивает снимок защищённых файлов репозитория до и после запуска.
Намеренное обновление committed fixture делается только как отдельная
разработческая операция: временно включить bounded copy helper, один раз
пересобрать `artifacts/extension-runtime-token-quality-learnability/adaptive-effect-gap-02`
через production `OrchestrationReasoningTrainer.MaterializeAsync` и
`OrchestrationReasoningMachine.SaveCheckpointAsync`, затем удалить write-capable
helper до коммита. В финальном коде теста не должно оставаться env-switch,
который может писать в репозиторий при обычном запуске.

Для правок самой инфраструктуры батчей PowerShell-обёртка сначала запускает
обязательный короткий страж, а уже потом переходит к выбранным основным
группам. Если нужен только быстрый сигнал о том, что основной список групп,
слабый снимок и guard не разошлись, добавьте `-FocusedObligationsOnly`: тогда
тяжелая основная группа не стартует.

Если такой автоматический запуск получает положительный `-MaxLeaves`, этот
лимит общий для наборов обязательных проверок (`focused manifest`) и основных групп. PowerShell
передаёт первому дочернему запуску текущий остаток, после завершения читает
прирост `leaf-journal.tsv` и только потом решает, можно ли запускать следующий
manifest или основную группу. Уже закрытые leaf-и при resume не тратят новый
остаток. Если остаток стал нулём, следующий дочерний процесс не запускается:
ноль не передаётся как `--max-leaves 0`, потому что для bash-запуска это
означает безлимитный режим.

При явном `-RunRoot` короткие обязательные результаты пишутся в стабильную
соседнюю папку `RunRoot-focused-obligations\<manifest>`. Повтор той же команды
продолжает этот же focused-каталог, а основной `RunRoot` остаётся отдельным и
не смешивает свои сводки с короткими leaf-журналами.

Если положительный `-MaxLeaves` задан без явного `-RunRoot`, PowerShell-обёртка
сама выбирает один основной каталог `TestResults/batches-<slice>/run-<UTC>Z`,
печатает его как путь для продолжения и передаёт его Linux-запуску. Так
wrapper видит прирост `leaf-journal.tsv` основного дочернего процесса и не
печатает ложный остаток после реально выполненной порции.

`TestResults` — это рабочая папка доказательств, а не часть исходного кода.
Новые результаты, журналы, последовательности и дампы зависаний должны жить
только там или в другой уже исключённой папке результата. Старые файлы под
`Tests/TestResults` сняты с отслеживания: они могут оставаться на локальном
диске как историческое evidence, но новый коммит не должен снова добавлять их
в индекс.

Если нужно проверить одну маленькую группу внутри уже выбранной основной
группы, укажите её через `--leaf-id`. Сначала выберите ровно одну основную
группу через `--only`, затем укажите, например, `--leaf-id leaf-005`. Команда
строит полный план маленьких групп для этой основной группы, запускает только
выбранную порцию, а остальные порции оставляет открытыми. Такой каталог
результатов проверяется как неполный (`-AllowIncomplete`) и не может называться
полной проверкой всей группы.

Для проверки выбора без WSL используйте `-PlanOnly` вместе с `-ChangedPath` или
`-ChangedSince`. В этом режиме wrapper печатает человеческие названия выбранных
групп, исполнимый `-Only` и короткие обязательные проверки, затем
останавливается до запуска тестов.

Это первая быстрая проверка свежего изменения, а не заявление full-suite green:
зелёный targeted-прогон говорит только о выбранных батчах. Если selector не
нашёл безопасной автоматической карты для production-файла, документа,
удалённого файла или C#-файла с неоднозначным тестовым атрибутом, он
останавливается и просит явный `-Only` либо focused manifest, не возвращая
частичный список.

Selector больше не использует `Tests/TestSuiteBatchLegacySnapshot.txt` как
карту владельцев тестов. Этот слабый снимок может быть пустым; он остаётся
проверяемым контрольным списком старых классов без собственного точного
маршрута, но выбранные группы берутся из текущего `batches.tsv`, имени
тестового класса и тестовой сборки. Старый параметр `SnapshotPath` сохранён для
совместимости команд и не определяет маршрут. Если один исходник подключён в две сборки, например
`Tests/DesignConversionModelActivationRegistryTests.cs`, запускалка выбирает
оба нужных адреса: основную группу конверсии дизайна и отдельную группу
включения модели.

Для автоматического выбора нужен PowerShell 7 с доступным встроенным Roslyn
(`Microsoft.CodeAnalysis*.dll`). Если parser недоступен или метод помечен
атрибутом, который нельзя надёжно связать с xUnit, выбор останавливается и
требует ручной `-Only`: частичная зелёная проверка хуже честного отказа.

Запускатель батчей выбирает методы через владельца класса. Он снимает полный
список тестов конкретной сборки, назначает каждый метод первой подходящей
строке `batches.tsv` по имени класса и только потом передаёт VSTest точные имена
методов. Большой точный фильтр передаётся через файл настроек (`.runsettings`),
поэтому длинные группы не зависят от лимита аргументов Linux. Проверяющий
скрипт сверяет результаты с сохранённым полным списком тестов и ловит как
пропущенные методы, так и чужие методы в закрытой группе.

Подготовка полного списка тестов также ограничена по времени: используется
предел маленькой порции (`--leaf-timeout`), а рядом с кэшем сохраняются команда,
время начала и конца, длительность, stdout/stderr и exit code. Если этот шаг
завис, запускатель возвращает ошибку и не принимает старый список методов.

Защита от зависшего тестового процесса проверяется отдельной короткой
самопроверкой без продуктовых тестов. Она создаёт синтетический VSTest, который
пишет частичный результат и зависает, после чего запускатель обязан оборвать его
внешним лимитом, сохранить stdout/stderr, команду, `blame-hang.runsettings` и
записать состояние маленькой группы как `LEAF_TIMED_OUT`, а не как зелёный
результат из старого файла.

```bash
bash tools/test-batches/run-test-batches.sh \
  --no-build \
  --self-check-hang-guard \
  --run-root "TestResults/hang-guard-self-check-$(date -u +%Y%m%dT%H%M%SZ)"
```

Эта команда доказывает только работу защиты от зависания и записи evidence. Она
не заменяет полный пакетный прогон и не означает, что продуктовые тесты зелёные.

Самопроверка выбора по классу не запускает .NET-тесты:

```bash
bash tools/test-batches/run-test-batches.sh \
  --no-build \
  --self-check-class-first-selection \
  --run-root "TestResults/class-first-selection-self-check-$(date -u +%Y%m%dT%H%M%SZ)"
```

## Узкая проверка восстановления журнала публикации, который разбивается на отдельные файлы

Тест `LocalArtifactDb_ForcedRotationPreservesReplayRepairListAndRollback`
теперь предназначен для реального запуска в Linux, а не для тихого успешного
выхода до основной логики. Он проверяет восстановление списка локальных
публикаций после сбоя при записи этого списка, повтор без нового создания
модели или отправки результата и откат к предыдущей активной локальной модели.
Этот тест не доказывает качество внешней модели, обучение на полном корпусе
или зелёный статус всего набора тестов.

Узкий фильтр для ручной проверки:

```bash
env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT \
  dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build \
  --filter "FullyQualifiedName=DesignConversionDpoStorageTests.LocalArtifactDb_ForcedRotationPreservesReplayRepairListAndRollback"
```

## Узкая проверка переноса журнала публикации локальной модели при заполнении

Две проверки журнала публикации локальной модели выполняются в Linux/WSL, а не
выходят тихо до основной логики. Первая проверяет перенос пары файлов, когда
журнал операций уже заполнен, а файл проекции ещё пустой. Вторая проверяет
обратный случай: заполнен только файл проекции, но новое поколение должно быть
общим для обеих сторон журнала. Соседние проверки поведения при блокировке
файла этой правкой не изменяются и в команду ниже не включены.

Ограниченная команда после сборки тестового проекта:

```bash
env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT \
  dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build \
  --filter "FullyQualifiedName=DesignConversionDpoStorageTests.LocalPromotionJournalSet_OperationPressureRotatesPairWithZeroByteProjection|FullyQualifiedName=DesignConversionDpoStorageTests.LocalPromotionJournalSet_ProjectionOnlyPressureRotatesCoordinatedGeneration"
```

Зелёный результат этой команды доказывает только выполнение двух сценариев
переноса при заполнении журнала. Он не заменяет полный набор тестов проекта и
не закрывает поведение при блокировке файла.

## Фокусная проверка паспорта шума замера

Срез паспорта шума проверяется точечно, без заявления full-suite green. Базовая команда для локальной сборки:

```powershell
dotnet build Tests\LlmFromScratch.Tests.csproj -c Release --nologo
```

Фокусный срез запускается через exact-method manifest. Он намеренно не
использует substring `FullyQualifiedName~`: паспорт шума теперь должен
доказать не только set/rows/mean, но и личность прибора — scorer id, scorer
version, normalization profile и metric profile.

```powershell
$runRoot = "TestResults\measurement-noise-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\measurement-noise-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\measurement-noise-focused.tsv -RunRoot $runRoot -Focused
```

Если нужно вручную сузить только xUnit discovery, допустим короткий ad-hoc
фильтр, но он не заменяет manifest-bound evidence от runner/verifier:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --no-build --nologo --filter "FullyQualifiedName~MeasurementNoisePolicyTests|FullyQualifiedName~DesignConversionG3NoiseFloorTests|FullyQualifiedName~DesignConversionMeasurementGateTests|FullyQualifiedName~ContinuousLearningTp1EvaluationScoringRunnerTests|FullyQualifiedName~ContinuousLearningTp1EvaluationReviewRunnerTests|FullyQualifiedName~DesignConversionG3AcceptancePolicyTests"
```

Ожидаемая смысловая матрица: одинаковые repeated scores дают sample stddev `0`, но не нулевой floor из `2 * sqrt(p * (1 - p) / N)`; improvement принимается только строго выше floor; равенство floor и score внутри полосы measurement gate не проходят как доказанный выигрыш; отсутствие калибровки остаётся статусом `difference_not_proven_no_calibration`; TP1 review до появления настоящего passport-bound calibration принимает только `MeasurementNoiseFloor = null` и ровно этот статус; чужой или несогласованный паспорт, включая чужие scorer id/version/normalization/metric profile, даёт fail-closed отказ.

### Совместимость отпечатка измерения при добавлении паспорта шума

Канонический отпечаток имеет две явные версии. Запрос, в котором нет ни
ожидаемой личности скорера, ни паспорта шума, по-прежнему сериализуется точными
историческими байтами схемы `design-conversion-caller-admission-measurement-v1`:
маркеры отсутствующих новых полей туда не добавляются. Поэтому существующим
записям без калибровки миграция не нужна, а старые независимые эталоны хеша
продолжают проверять тот же контракт.

Наличие ожидаемой личности или паспорта включает схему
`design-conversion-caller-admission-measurement-v2`. В ней присутствуют оба
маркера, полная ожидаемая личность и, когда он есть, полный паспорт со всеми
повторными оценками в исходном порядке. Ожидаемая личность без паспорта —
допустимое честное описание ещё не откалиброванного прибора, но все пять её
полей проверяются теми же ограничениями длины и безопасного алфавита, что и в
паспорте. Паспорт без ожидаемой личности отвергается до вычисления хеша.

Промежуточный ошибочный вариант «имя v1 плюс новые поля паспорта» не считается
синонимом ни одной версии. Если такой отпечаток успел попасть во временный
артефакт, его нельзя переписать по одному старому хешу: запись пересоздаётся из
полного исходного запроса. Целевой manifest дополнительно запускает три
сквозных независимых эталона старого формата; их ожидаемые значения менять ради
зелёного результата запрещено.

## Фокусная проверка переносимости TP1 evaluation runner

Переносимые unit-тесты `ContinuousLearningTp1EvaluationRunnerTests` должны
явно подменять только публикационную оболочку на `FakePublicationBoundary`.
Это не ослабляет production runner: `EvaluationRunnerDependencies.Production`
по-прежнему создаёт `WindowsEvaluationPublicationBoundary`, а запуск без
test seam на Linux/WSL должен оставаться отказом
`TP1_EVALUATION_PLATFORM_UNSUPPORTED`.

Смысл среза: fake-client проверки на WSL доходят до своих бизнес-вердиктов, а
не падают раньше на production Windows gate. Ошибки должны оставаться
конкретными: invalid provenance, permit unused/double, blank output, teacher
failure, cancellation и capability-invalid guard для клиента, который ошибочно
открывает training capability. Успешная fake-публикация дополнительно
проверяет, что staging не протёк, а failure-пути не выполняли unintended
publish.

```powershell
dotnet build Tests\LlmFromScratch.Tests.csproj -c Release --nologo
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --no-build --nologo --filter "FullyQualifiedName=LlmFromScratch.Tests.ContinuousLearningTp1EvaluationRunnerTests.SuccessfulRun_PublishesOnlyObservationAndManifestWithNegativeInvariants|FullyQualifiedName=LlmFromScratch.Tests.ContinuousLearningTp1EvaluationRunnerTests.InvalidProvenance_AbortsWithoutFinalDirectory|FullyQualifiedName=LlmFromScratch.Tests.ContinuousLearningTp1EvaluationRunnerTests.ProvenanceClaimingMultipleInvocations_AbortsWithoutPublication|FullyQualifiedName=LlmFromScratch.Tests.ContinuousLearningTp1EvaluationRunnerTests.MissingOrDoublePermitIssue_AbortsWithoutPublication|FullyQualifiedName=LlmFromScratch.Tests.ContinuousLearningTp1EvaluationRunnerTests.BlankOutput_AbortsWithoutFinalDirectory|FullyQualifiedName=LlmFromScratch.Tests.ContinuousLearningTp1EvaluationRunnerTests.MidRunFailure_CleansStagingAndDoesNotPublish|FullyQualifiedName=LlmFromScratch.Tests.ContinuousLearningTp1EvaluationRunnerTests.Cancellation_CleansStagingAndDoesNotPublish|FullyQualifiedName=LlmFromScratch.Tests.ContinuousLearningTp1EvaluationRunnerTests.DualEvaluationAndTrainingClient_IsRejectedBeforeEvaluation"
```

Manifest-bound evidence для portability-среза использует exact-method manifest:

```powershell
$runRoot = "TestResults\tp1-evaluation-runner-portability-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\tp1-evaluation-runner-portability-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\tp1-evaluation-runner-portability-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна manifest-строка, 8 exact method FQN и
минимум 9 физических xUnit rows. Эта проверка не является full-suite green и не
заменяет отдельные Windows-only проверки production publication boundary.

## Фокусная проверка TP1 ceiling и паспорта шума review-пакета

TP1 local-teacher ceiling принимает из review-пакета только честный паспорт шума
без калибровки: `measurement_noise_floor = null` и
`measurement_noise_status = difference_not_proven_no_calibration`. Это не
повышает доверие, не даёт `PASS`, TP1 exit, TP2 или DPO-ready. Любой non-null
floor, сериализованный статус `improved_beyond_measurement_noise`
(доказанное улучшение), `regressed_beyond_measurement_noise` (доказанное
ухудшение), `indistinguishable_within_measurement_noise` (разность внутри пола
шума), `measurement_noise_passport_invalid` (паспорт невалиден), любой другой
статус и любое новое лишнее поле внутри `runs[]` должны падать fail-closed до
публикации.

```powershell
dotnet build Tests\LlmFromScratch.Tests.csproj -c Release --nologo
$runRoot = "TestResults\tp1-local-teacher-ceiling-noise-passport-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\tp1-local-teacher-ceiling-noise-passport-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\tp1-local-teacher-ceiling-noise-passport-focused.tsv -RunRoot $runRoot -Focused
```

## Фокусная проверка допуска пары DPO

Срез допуска пары проверяет контракт происхождения, таможню-фабрику,
nullable slot в `DpoBatch`, admission-aware `DpoDataset` loader и guard прямого
создания бедной `DPO.Models.DpoPair`. Это не full suite: текущие
translation/Wiki/DesignConversion/Capability scopes доводят общий счёт product
bridges до `5/5`, а отдельный trainer-door срез ниже закрывает protected
trainer entrances как `3/3`. Legacy batch без admission теперь читается и
оценивается, но mutating `Step`/`StepMany`/`StepVector` падают с typed
`DPO_PAIR_ADMISSION_MISSING` до mutation permit.

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~DpoTrainerPairAdmissionTests|FullyQualifiedName~PoorDpoPairConstructionGuardTests"
```

Manifest-bound evidence запускается без `-NoBuild`, чтобы runner сам собрал
актуальные `DPO.dll` и `LlmFromScratch.Tests.dll`, а затем verifier закрепил
focused scope и запрет full-suite claim:

```powershell
$runRoot = "TestResults\dpo-pair-admission-contract-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\dpo-pair-admission-contract-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\dpo-pair-admission-contract-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: корневой `state.txt` = `FINISHED`,
`verification_scope=FOCUSED`, `full_suite_claim_allowed=false`, одна строка
manifest, 25 exact FQN и 25 физических xUnit rows, непустые DLL
`11.DPO/bin/Release/net9.0/DPO.dll` и
`Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll`. Смысловой статус:
dataset prerequisite готов, legacy rows имеют `Admission=null`, complete
author/issuer rows с exact-case `Prompt`/`Chosen`/`Rejected` создают admission
через customs, public `Rows` не дает downcast-мутировать private snapshot,
duplicate/wrong-case core/admission поля падают закрыто, paid side readable but
not trainable. Roslyn semantic guard фиксирует customs как единственный legal
product-training construction site; translation held-out остается explicit
evaluation, а external experiment считается historical prohibited/non-product
site и не входит в product allowlist. Self-fixture guard-а покрывает
unqualified, qualified, `global::`, type alias, namespace alias,
cross-file global alias из отдельного source file, target-typed return/local и
target-typed property initializer; actual repo census компилирует все scanned
source trees одним semantic compilation, поэтому product file не обязан
содержать текст `DpoPair` рядом с target-typed `new()`. Каждая
`BaseObjectCreationExpressionSyntax` проверяется semantic model-ом, а DPO-like
syntax с unresolved/null/error type падает fail-fast вместо false negative.
Этот manifest остается bridge/contract evidence; trainer door доказывает
отдельный focused manifest ниже.

## Проверка карантина старых DPO-артефактов

Проверка карантина отвечает на простой человеческий вопрос: все старые
трёхполевые пары и старые seed-run каталоги записаны как историческая база, и
ни один такой файл не может незаметно снова стать материалом обучения. Поэтому
она сканирует исторические `dpo-runs` внутри исходных артефактов проекта,
например `43.LearningLocalWikiData/artifacts/.../dpo-runs/.../dpo-pairs.jsonl`.

Корневые `Results` и `TestResults` не являются источником обучающих пар. Это
рабочие папки доказательств текущих запусков, там могут оставаться служебные
каталоги с ограниченными правами. Скан карантина обязан отсечь только
`Results/...` и `TestResults/...` от корня репозитория: закрытая папка
результатов теста не должна ронять проверку manifest и не должна попадать в
список исторических DPO-артефактов. Вложенные папки с теми же именами не
скрываются: `docs/plans/Results/...`, `artifacts/Results/...` или
`training-data/.../Results/...` могут быть настоящей исторической базой, и
manifest обязан увидеть лежащие там `dpo-runs`.
Исторические `artifacts` при этом не исключаются, потому что именно в них лежит
проверяемая база. Старый корневой `evidence/run` тоже считается runtime-выходом,
но вложенные папки `evidence` внутри документации или training-data не
скрываются: если там появится исторический `dpo-runs`, manifest обязан увидеть
и описать его.
Обычный каталог проекта нельзя пропускать молча: ошибка чтения вне служебных
runtime-папок означает красную проверку, потому что иначе настоящий старый
обучающий файл может исчезнуть из обхода без записи в карантин. Хеши jsonl
считаются как канонический UTF-8 текст без учёта служебного BOM и с переводами
строк `LF`, чтобы один и тот же git-файл проверялся одинаково из Windows- и
Linux-рабочих копий.

Узкая проверка этого договора:

```powershell
wsl.exe -d Ubuntu-24.04 --cd /mnt/c/Projects/llm_mvp_core --exec /usr/bin/timeout 180s dotnet vstest Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll --TestCaseFilter:"FullyQualifiedName~LegacyDpoArtifactQuarantineManifestTests"
```

## Фокусная проверка двери DPO trainer pair admission

Этот срез проверяет последнюю fail-closed дверь непосредственно в
`DpoTrainer`: все mutating входы `Step`, `StepMany` и `StepVector` требуют
passport до запроса server mutation permit, optimizer/RNG mutation и публикации
checkpoint/artifacts. Missing admission дает typed
`DPO_PAIR_ADMISSION_MISSING`; `PaidExternalModel` на chosen или rejected стороне
дает typed `DPO_PAIR_ADMISSION_PAID_MODEL_SIDE`. `StepMany` и `StepVector`
сначала проверяют всю коллекцию, поэтому valid-first/bad-later не меняет модель
и не создает artifacts. `EvaluateMany` остается read-only и принимает missing
или paid admission.

```powershell
$runRoot = "TestResults\dpo-trainer-pair-admission-door-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\dpo-trainer-pair-admission-door-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\dpo-trainer-pair-admission-door-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна строка manifest, 23 exact FQN и 23
физические xUnit rows. Смысловой статус: protected trainer entrances `3/3`,
legacy `DpoDataset` batch остается `Admission=null`, vector valid-first/bad-later
и null nested batch падают до permit, external historical experiment runner не
патчится на mint admission и также останавливается на trainer door. Валидный passport не заменяет
`DpoMutationAdmissionCapability`: отсутствие или revoked permit сохраняет старый
mutation-denied отказ без изменения весов.

## Фокусная проверка rich pair authorship и FileDpoPairStore v5

Этот срез проверяет авторство сторон у rich continuous-learning DPO pair,
строгую v5-персистентность в `FileDpoPairStore` и DesignConversion single/atomic
intake projection до durable pair store. Capability bridge и ModelServer handoff
теперь потребляют эти typed author kinds downstream: общий счёт admission bridges
стал `5/5`, а protected trainer entrances закрыты отдельным trainer-door
manifest как `3/3`.

Targeted локальная проверка:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~ContinuousLearningDpoPairAuthorshipPersistenceTests|FullyQualifiedName~DesignConversionDualTargetTeacherExternalConsumptionTests.LocalArtifactPairProjection|FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAsync_PersistsApprovedPairsThroughDurableStoreBeforeRunner|FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAsync_ExactDualTargetTeacherAggregate_PersistsOnlyOrderedTrainChildrenBeforeCoordinatorFailure"
```

Manifest-bound evidence запускается без `-NoBuild`: runner сам собирает
актуальные Release DLL, пишет физический TRX и закрепляет focused scope.
Verifier запускается с тем же manifest и `-Focused`.

```powershell
$runRoot = "TestResults\rich-pair-authorship-store-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\rich-pair-authorship-store-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\rich-pair-authorship-store-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна строка manifest, 19 exact method FQN и 45
физических xUnit rows. Разница намеренная: tamper-theory разворачивается в три
случая — one-sided authorship, undefined enum и forged OpenTeacher passport;
две forged deterministic-no-external theory разворачиваются по 9 классов
противоречий каждая для persistence и transaction gates; две unsafe RequestId
theory разворачиваются по 5 классов — empty, whitespace-only, trimmed, control
char и overlong. Проверка доказывает constructor immutability/passport rules,
router mappings, permitted OpenTeacher side, rejection of unknown/contradictory
events, external provenance-to-side linkage, exact deterministic-no-external
provenance and safe RequestId for non-OpenTeacher v5 authors, v5 restart/roundtrip,
identity mutation by author,
v4 readable-but-not-training-ready legacy, atomic group child validation и
DesignConversion single/atomic intake projection without v4 fallback.

Storage-local regression keeps two separate external-teacher fixtures: passport
and `OpenTeacher`/`LocalModel` side authorship without
`independent_preference_proven=true` is still rejected as
`DESIGN_CONVERSION_DPO_PAIR_AUTHORSHIP_MISSING_OR_INVALID`, while the
allowlisted ordinary local-promotion fixture adds that independent-preference
proof but does not set the dual-target corpus marker
`runtime_rejected_independently_evaluated=true`. Так single-pair bridge не
обходит rich authorship contract и одновременно не притворяется corpus-admission
путём, который проверяется отдельными atomic external-consumption тестами.

Порядок стражей в закрытом исполнителе с правовой властью зафиксирован отдельно:
абсолютный правовой запрет на использование выхода закрытой проприетарной модели
как цели обучения проверяется РАНЬШЕ действительности паспорта пары.
Предполётная правовая проверка собирает различные паспорта учителя у строк
обучения и судит их тем же примитивом правовой власти, что и совмещённая
предполётная проверка, поэтому отказ остаётся опознаваем как правовой
(`LEGAL_TEACHER_CLOSED_PROPRIETARY_DENIED`, `LEGAL_TEACHER_NOT_ALLOWLISTED`), а не
как отказ по неисправной записи
(`DESIGN_CONVERSION_DPO_PAIR_AUTHORSHIP_MISSING_OR_INVALID`). Строго
детерминированный путь без внешнего учителя в правовую партию не попадает и
проходит как прежде. Порядок закреплён проверкой
`DpoTrainerPolicyCheckpointBridge_ClosedProprietaryTeacherDenialPrecedesAllowlistFailure`.


Сверочные модели доказательства неизменности продолжения мост больше не строит
сам. Он просит их у внедрённого поставщика через вход «отдать сверочный набор»
узкого договора продолжения, а отпечаток ожидаемого состояния по-прежнему
считает у себя. Это восстанавливает правило 2 проекта («сначала фабрика»):
единственным источником языковых моделей для моста стал внедрённый поставщик, и
создание модели в файле моста больше не встречается ни разу. Путь продолжения на
поставщике без договора продолжения, а также поставщик договора продолжения,
который сверочный вход не переопределил, отказывают закрытым кодом
`DESIGN_CONVERSION_DPO_POLICY_BASE_VERIFICATION_MODEL_SET_UNAVAILABLE`.

Сверочный набор — расходный: в него загружается допущенная контрольная точка, и
обучаемые модели он не затрагивает. Поэтому он приходит отдельным входом, а не
повторным обращением за набором обучаемых моделей: счётчик обращений к
поставщику этим входом не увеличивается, у сверочных обращений свой счётчик.
Наблюдение, которое важно для будущих проверок: роль модели (политика, опорная,
свежая загрузка) влияет на посев канонной топологии малых поправок, но **не
выживает загрузку контрольной точки** — после загрузки состояние трёх ролей
определяется самой контрольной точкой. Поэтому сравнение ролей имеет смысл
только до загрузки и только под детерминированным посевом; именно так устроена
проверка `ProviderSuppliedVerificationSetMatchesBridgeOwnedConstructionByteForByte`.
Отказ приведения типа и отказ по умолчанию закреплены проверкой
`ContinuationVerificationSetIsFailClosedWithoutContinuationContract`.

Граница подделки происхождения состояния выполнения расширена ровно на один
серверный факт. Детектор заявки об обученном выводе устроен как запрет по
умолчанию: любая правдивая метка происхождения, которой нет в списке серверных
фактов без права на власть, считается заявкой. Метка «репозиторный договор
обучающих пар действителен» внесена в этот список, потому что гейт её проверки
относится только к ВКЛЮЧЁННЫМ маршрутам: у отключённого договора отклонять
нечего, и `Ok(true)` отвечает на вопрос «проверка отклонила договор?», а не
заявляет, что договор существует. Режим несёт парная метка «включён», и она
правдиво False. Заявкой об обученности эта метка не является.

Почему внесение не стало дырой подделки, и это закреплено постоянной проверкой
`SubmitAndEvaluateAsync_ClientMetadataCannotForgeServerOwnedRepositoryOwnedDpoValidity`:
присланное клиентом в происхождение состояния выполнения не попадает вовсе — ни
голым именем серверного факта, ни под пространством имён запроса. Проверка
различает владельца значения намеренно: клиент присылает значение,
ПРОТИВОПОЛОЖНОЕ тому, что вычислит сервер, и в происхождении обязано остаться
серверное. Чувствительность доказана мутацией: при убранной записи в списке
краснеют обе проверки — и эта, и проверка обучаемого потока исполнения.
Для `IncrementalTrainerDpoWiringTests` действует тот же разрыв обязанностей:
структурно плохой паспорт учителя остаётся внутри пары как входные данные для
проверки допуска тренера, но доверенный объект авторства стороны `OpenTeacher`
создаётся только для паспорта, который уже проходит production-контракт ссылки
на открытого учителя. Поэтому отрицательные тесты плохих паспортов доходят до
границы тренера и не падают раньше времени внутри тестовой заготовки.
Independent-preference proof is immutable payload-custody governance, so the
fixture stamps it before custody binding; reviewer approval evidence remains a
separate mutable transition that the local artifact DB later records and
rebinds without changing teacher content.

## Фокусная проверка допуска пары перевода

Этот срез проверяет только Prompt 10 translation substage 2/5: handoff выдает
admission из валидного паспорта строки, bridge переносит его в training
`DpoBatch`, held-out evaluation batch остается без admission, а raw
Prompt/Chosen/Rejected нельзя менять после выдачи паспорта. Это не full suite:
исторически этот manifest был substage 2/5; текущая суммарная картина после
Wiki, DesignConversion и Capability — `5/5` product bridges with admission.
Protected trainer entrances проверяются отдельным trainer-door manifest как
`3/3`.

Targeted локальная проверка:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~TranslationDpoTrainingHandoffTests|FullyQualifiedName~TranslationDpoTrainerBridgeTests|FullyQualifiedName~TranslationHandoffPerSideAuthorshipTests"
```

Manifest-bound evidence запускается без `-NoBuild`: runner сам собирает
актуальные DLL, пишет физический TRX и закрепляет focused scope. Verifier должен
запускаться с тем же manifest и `-Focused`.

```powershell
$runRoot = "TestResults\translation-pair-admission-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\translation-pair-admission-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\translation-pair-admission-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна строка manifest, 38 физических xUnit rows
для translation handoff/bridge/authorship, admission issuer ровно
`TranslationDpoTrainingHandoffBuilder`, обе стороны `User`, fingerprint по raw
pair, held-out `DpoBatch.Admission = null`.

## Фокусная проверка допуска пары Wiki

Этот срез проверяет только ProjectWiki bridge substage 3/5: DPO metric runner
выдаёт admission для training `DpoBatch`, которые идут в `DpoTrainer.StepMany`,
а held-out tokenization и pair diagnostics остаются evaluation-only с
`DpoBatch.Admission = null`. Обе стороны Wiki-пары сейчас допускаются как
`DeterministicGenerator`, потому что текущие 11 product pipeline converters
строят `ProjectWikiDpoMetricPair` из deterministic seeds/templates/signals, без
User/Local/Open/Paid source side. Это не full suite: исторически Wiki был
substage 3/5; после DesignConversion и Capability общий счёт — `5/5` product
bridges with admission; protected trainer entrances проверяются отдельным
trainer-door manifest как `3/3`.

Минимальный test seam: `BuildTrainingBatchPreviewForTesting`,
`BuildEvaluationBatchPreviewForTesting` и `batchObserverForTesting` показывают,
какой `DpoBatch` построен для preview/tokenization/StepMany/diagnostics; они не
меняют product routing и нужны только для проверки, что training admission не
утекает в evaluation path.

Targeted локальная проверка:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~ProjectWikiDpoMetricRunnerPairAdmissionTests"
```

Manifest-bound evidence запускается без `-NoBuild`: runner сам собирает
актуальные DLL, пишет физический TRX и закрепляет focused scope. Verifier должен
запускаться с тем же manifest и `-Focused`.

```powershell
$runRoot = "TestResults\project-wiki-pair-admission-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\project-wiki-pair-admission-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\project-wiki-pair-admission-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна строка manifest, 4 физических xUnit rows,
admission issuer ровно `ProjectWikiDpoMetricRunner`, обе стороны
`DeterministicGenerator`, fingerprint по raw `Prompt`/`Chosen`/`Rejected` без
tokenization prefix `Answer:`, held-out и diagnostics `DpoBatch.Admission =
null`.

## Фокусная проверка CodeDocumentation server-owned DPO training lease broker

Этот срез проверяет только серверную дверь допуска мутации для DPO-обучения
документации кода. `IServerOwnedDpoTrainingLeaseBroker` принимает точный
`DpoTrainingContentBatch` и bounded canonical `trainingRunId`, а
`ServerOwnedLegalTrainingTeacherAdmissionAuthority` выдаёт lease через тот же
server-owned deterministic/bound `AcquireTrainingLease` путь, но в отдельном
namespace `code-documentation-dpo`. Невалидные run id должны падать закрыто с
существующим `LEGAL_TEACHER_TRAINING_CONTENT_INVALID` до выдачи lease.

Проверка доказывает только admission door: distinct namespace, exact content
identity, невозможность применить lease к изменённому batch через существующий
content-binding seam, а также отказ mutation permit после dispose/revocation.
Она не создаёт DPO, не тренирует модель, не загружает и не публикует checkpoint,
не пишет отчёты и не трогает `training-artifacts/**`.

Targeted локальная проверка:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore --nologo --filter "FullyQualifiedName~CodeDocumentationServerOwnedDpoTrainingLeaseBrokerTests"
```

## Фокусная проверка CodeDocumentation server-owned DPO training executor

Этот срез проверяет публичный consumer над lease broker:
`CodeDocumentationServerOwnedDpoTrainingExecutor` валидирует bounded request до
мутации/файлов, отклоняет alias `ReferenceEquals(policy, reference)` до lease и
LoRA topology mutation, rebuild-ит `RepositoryOwnedModelCheckpointAddress`
через layout, один раз захватывает caller-owned pair input через
`DpoPairBatchSnapshot.Capture`, получает CodeDocumentation lease, а затем
передаёт тот же immutable snapshot в существующий
`StableDpoTrainingExecutor`/`11.DPO:DpoTrainer`.

Позитивный тест использует tiny in-process `LlmModel`, reference cloned
bit-for-bit from the starting policy, fake repository-shaped temp root outside
live checkout, один валидный DPO step, SHA-256 policy до/после и непустые final
`dpo_policy_step_000001.bin` / `dpo_step_000001.json` внутри sealed
`ArtifactAddress.CheckpointRoot`. Мутационный list тест доказывает, что broker
может заменить caller-owned элемент на другой authorless row после получения
content, но training всё равно использует captured original snapshot, а content
identity совпадает с original, не replacement. Негативные тесты доказывают, что
refusing broker и same policy/reference object останавливаются до
изменения весов и до создания checkpoint directory.

Отдельный positive adversarial тест разделяет два числа, которые нельзя
смешивать. Repository checkpoint step — это место модели в цепочке сохранённых
весов (`000002` = второй checkpoint). DPO step count — это сколько optimizer
шагов выполняет один запуск. Поэтому тест с адресом checkpoint `000002` и
`DpoStepCount = 1` должен проходить: trainer пишет
`dpo_policy_step_000001.bin` и `dpo_step_000001.json` прямо в root checkpoint
`000002`, а не требует trainer step `2`. Это защищает продолжение обучения из
сохранённой модели вместо старта заново. Это не доказывает
CodeDocumentation runner wiring, production corpus quality или final weights
publication lifecycle; эти связи проверяются отдельным lifecycle-тестом ниже.

`CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests`
закрывает focused lifecycle для документации кода. Positive test создаёт
маленький repo-shaped fixture, запечатывает DPO dataset в `training-data/dpo`,
запечатывает parent model checkpoint `000001` в `training-artifacts/models`,
затем запускает координатор. Проверка принимает только такой результат: DPO
открыт обратно из repository manifest, policy загружена через START, один
DPO-шаг выполнен через server-owned lease, child checkpoint `000002` опубликован
как `weights.bin` + `manifest.json`, а следующий START с тем же LoRA-описанием
открывает этот checkpoint с диска и видит уже step `000002`.
Negative test подменяет пары в памяти runner-а при старом receipt и должен
упасть до training/publication: child `weights.bin` и `manifest.json` не
появляются. Отдельный negative test ломает receipt так, будто процесс принёс
паспорт DPO без упорядоченного списка pair-файлов; координатор должен вернуть
понятную ошибку до START/training/publication, а child checkpoint не должен
появиться. Это доказывает lifecycle и защиту от подмены или поломанного
описания DPO-пар на fixture байтах; оно не запускает реальный Qwen и не
заявляет качество обученной модели.

Targeted локальная проверка:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore --nologo --filter "FullyQualifiedName~CodeDocumentationServerOwnedDpoTrainingExecutorTests"
dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore --nologo --filter "FullyQualifiedName~CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests"
```

## Фокусная проверка общего remote-транспорта ниже ModelServer authority

Этот срез проверяет не обучение и не публикацию модели, а направление
зависимостей. Общие типы remote manifest/locator/proxy лежат в
`21.AgentInfrastructure/ModelServer/*`, чтобы `26.TrainableRouter` мог
маршрутизировать локальные bootstrap-запросы без ссылки на исполняемый проект
`25.ModelServer`. Сам ModelServer продолжает владеть admission и публикацией
checkpoint-ов; общий transport не становится второй властью обучения.

Главный guard —
`DistributedModelArchitectureTests.RemoteRoutingTransportLivesBelowModelServerAuthority`.
Он читает реальные `.csproj` и проверяет три вещи: `26.TrainableRouter` больше
не ссылается на `25.ModelServer`, оба проекта используют общий
`21.AgentInfrastructure`, а четыре перенесённых source-файла существуют в
нижнем слое. Остальные проверки этого же класса подтверждают, что прежние
`RemoteUnitManifest`, `InMemoryRemoteUnitLocator` и `RemoteLogicalUnitProxy`
по-прежнему исполняют тот же manifest/lookup/proxy контракт.

Targeted локальная проверка:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore --nologo --filter "FullyQualifiedName~DistributedModelArchitectureTests"
```

## Фокусная проверка честности seed CrossStackBusinessFlowGraph

Этот срез проверяет, что deterministic seed для сквозного бизнес-графа остаётся
cold-start baseline, а не основанием для semantic `Trusted` publication.
Генератор обязан строить полную матрицу 10 доменов x 5 rejected-типов без
скрытого round index и без `Take(50)`. Независимый validator заново считает
wrong-annotation rate и matrix cells из raw rows, сверяет internal provenance
proof, split-role и SHA-256 exact ordered rows; публичный `SourceKind` остаётся
только описанием и сам не даёт provenance.

Отдельная проверка отсутствия обрезки теперь проходит через тот же production
seed/batch path, но с тестовым private seam на 11 явных доменов: 11 доменов x 5
rejected-типов дают 55 строк. Это пересекает прежнюю границу 50 строк, поэтому
регрессия вида `Take(50)` ломает row count и полную matrix-cell проверку, а не
маскируется ровно 50 строками. Этот тест не доказывает смысловое качество
ответов, не создаёт настоящий DPO-набор и не запускает обучение.

```powershell
$runRoot = "TestResults\cross-stack-flow-seed-honesty-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\cross-stack-flow-seed-honesty-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\cross-stack-flow-seed-honesty-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна строка manifest, 16 exact method FQN и 16
физических xUnit rows. Смысловой статус: batch имеет
`SourceKind=DeterministicSeed`, `IndependentlyEvaluated=false` до validator-а;
pipeline валидирует train и held-out отдельно, пишет generator self-score
отдельно от validator recomputed rate, возвращает baseline/no semantic
publication и не вызывает publisher. Разрешённые side effects — только
training/evaluation baseline artifacts: raw train/held-out rows, independent
validation report, DPO metrics/diagnostics/checkpoints и versioned
`BaselineOnlyNotPromotable` disposition с allowlist/fingerprints. Все эти
baseline writes идут через temp-file в той же папке и replace; successful run не
оставляет `.tmp` files и сверяет exact allowlist (`cross_stack_batch_root`:
пять batch files + вложенный baseline-only `dpo-runs/**`,
`project_wiki_dpo_metric_run_root`: metric JSONL/diagnostics/summary/checkpoints
и reservation sidecars `checkpoints/.dpo_step_*.lock`).
Это не semantic authority.
Publisher дополнительно отказывает null/forged/replayed authority,
`DeterministicSeed`, `Unknown` и unaccepted candidates до создания trusted
files, upload или registry register. Так как importer настоящего
`RealRepositoryCorpus` пока отсутствует, live semantic authority minter для
этого unit тоже отсутствует; public publisher route без opaque authority
отказной, и positive trusted path в тесте не фабрикуется.

## Фокусная проверка допуска пары DesignConversion

Этот срез проверяет только Prompt 10 DesignConversion substage 4/5:
teacher-authored generated side мапится в `OpenTeacher`, incumbent side — в
`LocalModel`, а independent preference direction может развернуть chosen/rejected
без вывода авторства из score. Standalone и evaluated training batch получают
`DpoBatch.Admission` с fingerprint по raw `Prompt`/`Chosen`/`Rejected`; held-out
packing и runtime inference остаются без trainer admission. Candidate-only
authoring rows остаются quarantine/no-train-authority даже при server-owned route
binding.

Для сценария, где один запрос открытого учителя используется сразу для
обучающей и отложенной для проверки записи, тестовые данные должны задавать
происхождение обеих сторон пары до расчёта защитных контрольных сумм содержимого
записи. Паспорт вызова учителя говорит, какой внешний учитель работал, но не
заменяет ответ на вопрос, кто написал выбранную сторону и кто написал
отклонённую сторону. В этом положительном примере выбранная сторона принадлежит
открытому учителю, отклонённая — локальной модели; затем подготовка данных
пересобирает канонический вид записи.

Эта узкая проверка не закрывает пять сценариев хранилища и публикации новой
версии модели, где не хватает доказательства независимого сравнения. Их нельзя
чинить простой установкой флага «независимое сравнение подтверждено»: таким
записям нужен валидный источник этого доказательства. Защитные проверки с
неизвестным или противоречивым происхождением остаются отказными и не должны
попадать в создание обучающей партии.

Это не full suite: текущий общий статус Prompt 10 после Capability scope —
`5/5` product bridges with admission. Запуск ниже остается bridge evidence и
сам по себе не доказывает DPO trainer door; дверь тренера закрывает отдельный
focused manifest как `3/3`.

Targeted локальная проверка:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~DesignConversionDpoPairAdmissionBridgeTests|FullyQualifiedName~DesignConversionDpoPolicyRepresentationTests"
```

Manifest-bound evidence запускается без `-NoBuild`: runner сам собирает
актуальные DLL, пишет физический TRX и закрепляет focused scope. Verifier должен
запускаться с тем же manifest и `-Focused`.

```powershell
$runRoot = "TestResults\design-conversion-pair-admission-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-pair-admission-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-pair-admission-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна строка manifest, 15 exact FQN и 20
физических xUnit rows для DesignConversion pair admission + двух canonical
policy representation seams. Разница намеренная: один exact FQN —
`EvaluatedComposite_TrainabilityFailuresStopBeforeOptimizerLeaseAndFileAuthority`
— является parameterized theory и разворачивается в шесть trainability cases
(train/heldout × `AllowsTraining=false`, `ContainsUserPrivateData=true`,
quarantined=true). Admission issuer ровно
`DesignConversionDpoPolicyCheckpointBridge`, generated teacher side =
`OpenTeacher`, incumbent side = `LocalModel`, deterministic fallback =
`DeterministicGenerator`/`DeterministicGenerator`, fingerprint по raw pair,
held-out/runtime inference `DpoBatch.Admission = null`.

## Контракт и реальный canary независимого локального судьи Qwen

Контрактный пакет не обращается к модели. Он закрепляет точное тело запроса
evaluation-only профиля (`format=json`, `think=false`, `num_ctx=8192`,
`temperature=0`, `seed=0`, `num_predict=256`), отдельный предел 120 секунд,
fingerprint профиля, неизменность 30-секундного teacher-authoring запроса,
строгий parser, fail-closed ошибки и одноразовый receipt. В manifest две строки
и 24 физические проверки; это focused evidence, а не полный прогон и не
доказательство качества ответа модели.

```powershell
$runRoot = "TestResults\design-conversion-local-independent-qwen-judge-contract"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-local-independent-qwen-judge-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-local-independent-qwen-judge-focused.tsv -RunRoot $runRoot -Focused
```

Отдельный Windows canary является настоящим quality oracle. Он выполняет четыре
вызова одной и той же качественной пары: хороший вариант дважды находится на
стороне `generated`, затем дважды на стороне `incumbent`. Оценочные metadata у
сторон одинаковы, поэтому решение можно принять только по HTML/CSS. Все четыре
ответа обязаны выбрать один и тот же хороший контент, а направление обязано
смениться после перестановки. Каждый вызов сохраняет `temperature=0`, `seed=0`,
`think=false`, предел 256 токенов и максимум 120 секунд. Timeout, malformed JSON,
неизвестное поле или несогласованный выбор при включённом canary — падение, не
skip. Тест не передаёт receipt генератору обучающих пар и не вызывает тренер.

Перед запуском оператор явно задаёт `XIIGEN_RUN_LOCAL_QWEN_JUDGE_CANARY=1` и
три production-пина: `XIIGEN_CONTINUOUS_LEARNING_TP1_MODEL_ID`,
`XIIGEN_CONTINUOUS_LEARNING_TP1_OLLAMA_MANIFEST_DIGEST` и
`XIIGEN_CONTINUOUS_LEARNING_TP1_OLLAMA_MODEL_LAYER_DIGEST`. Без флага или вне
Windows тест получает discovery-time `Skip` через локальный `FactAttribute`: в
TRX это честный `NotExecuted`, без runtime `$XunitDynamicSkip$` failure. При
приёмке с флагом на Windows требуется ровно 1 из 1 прошедшая строка и ни
одного skip; отсутствие production-пинов, timeout, malformed JSON или неверный
выбор Qwen должны быть `Failed`, а не skip.

Эта одна xUnit-строка оборачивает четыре реальные production-попытки. До
отдельного Windows-запуска с opt-in и пинами модели их принято 0 из 4, поэтому
runtime quality остаётся красной/недоказанной границей даже при зелёном
контрактном пакете 24 из 24. Добавление класса в
`TestSuiteBatchLegacySnapshot.txt` не повышает этот статус: inventory membership
только гарантирует, что canary не потеряется при планировании батчей.

Старый TRX, созданный до discovery-time skip, verifier читает узко: только
точный FQN
`DesignConversionLocalIndependentQwenRuntimeCanaryTests.LocalQwenJudge_FourCallsChooseSameQualityAndReverseDirectionOnSwap`
и только сохранённое сообщение с префиксом `$XunitDynamicSkip$WINDOWS_ONLY_RUNTIME:`
нормализуются в `NotExecuted`. Эта историческая нормализация зависит от
записанного evidence, а не от сегодняшней переменной окружения verifier:
один и тот же старый TRX обязан дать одинаковые counts при env unset и env=1.
Любой другой FQN, другой `$XunitDynamicSkip$...` prefix и любой реальный quality
failure остаются красными. Свежая Windows-приёмка с
`XIIGEN_RUN_LOCAL_QWEN_JUDGE_CANARY=1` не нормализуется через старый TRX:
она обязана реально пройти 1/1 или упасть.

```powershell
$env:XIIGEN_RUN_LOCAL_QWEN_JUDGE_CANARY = "1"
$runRoot = "TestResults\design-conversion-local-independent-qwen-judge-canary"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-local-independent-qwen-judge-canary.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-local-independent-qwen-judge-canary.tsv -RunRoot $runRoot -Focused
```

Smart App Control `0x800711C7` блокирует только Windows runtime lane и не делает
его зелёным. Контрактный пакет в таком случае продолжается в WSL; реальный
canary остаётся недоказанным, пока четыре production-вызова не завершатся.

## Фокусная проверка admission для мутации DesignConversion DPO checkpoint

Этот срез проверяет Prompt 12 checkpoint-mutation quarantine: legacy/current
read-only inspection остаётся разрешённой, но continuation/mutable DPO base
требует live `DesignConversionDpoCheckpointMutationAdmission`. Admission
создаётся только verifier factory после current contract validation и сверки
checkpoint SHA-256, byte length, file/relative identity, training-context schema,
legal-teacher commitment fingerprint, base checkpoint SHA и source/training
manifest SHA. Bridge больше не принимает admission от provider-returned
`ModelSet`: promoted-parent создаёт отдельный hidden bridge-proof one-shot
source и отдельный provider-transfer one-shot source, bridge сам строит
expected fingerprint фактических policy/reference/fresh-load весов из
admitted checkpoint A и отвергает custom provider, который возвращает
public-contract-compatible, но чужое mutable состояние.

Тестовый `ContractPair` также не хранит байты checkpoint в публичном или
сериализуемом графе. Фактический снимок передаётся helper'у отдельно: verifier
получает фактический номер DPO-шага отдельным аргументом, один раз вычисляет из
байтов SHA-256 и длину, выводит canonical имя файла из этого независимого шага
и только после совпадения с договором создаёт admission. Отдельная копия того
же неизменяемого снимка выдаётся bridge как одноразовый proof-source. Поэтому
родитель и ребёнок используют шаг прямо из своего bridge-result, повреждённый
и обрезанный checkpoint — исходный фактический шаг, а согласованная подмена
`DpoStepCount` + имени файла + artifact-set binding не может аттестовать себя.

Targeted локальная проверка:

```powershell
dotnet vstest Tests\bin\Release\net9.0\LlmFromScratch.Tests.dll --TestCaseFilter:"FullyQualifiedName=DesignConversionCheckpointBackedDpoPolicyBaseProviderTests.CreateContinuedModelSetAsync_MissingAdmissionFailsBeforeOpeningAnySource|FullyQualifiedName=DesignConversionCheckpointBackedDpoPolicyBaseProviderTests.CreateContinuedModelSetAsync_AdmissionMismatchFailsTypedBeforeSource|FullyQualifiedName=DesignConversionDpoPolicyContinuationTests.ContinuationProviderForeignActualWeightsFailBridgeOwnedProofBeforeArtifacts|FullyQualifiedName=DesignConversionDpoStorageTests.EvaluatedBridge_TypedContinuationProviderForeignActualWeightsFailsBeforeAnyRealPermitOrArtifacts|FullyQualifiedName=DesignConversionDpoPolicyContinuationTests.ContractSchemaMatrix_AllowsLegacyInspectionButQuarantinesMutationContinuation|FullyQualifiedName=DesignConversionDpoPromotedParentContinuationTests.PromotedParentContinuationUsesSeparateProofAndProviderSourcesEndToEnd|FullyQualifiedName=DesignConversionDpoPolicyContinuationTests.ColdBridgePublicationContinuesThroughRealChildAndGrandchildLineage|FullyQualifiedName=DesignConversionDpoPolicyContinuationTests.ContractTamperingFailsBeforeOpeningCheckpointSource|FullyQualifiedName=DesignConversionDpoPolicyContinuationTests.CoordinatedStepFileAndBindingTamperingCannotMintAdmission|FullyQualifiedName=DesignConversionDpoPolicyContinuationTests.ContractsBindCanonicalArtifactSetAndSerializeNoCheckpointOrRawPath|FullyQualifiedName=DesignConversionDpoPolicyContinuationTests.MalformedTruncatedOversizedAndMutatedStreamsAreRejected"
```

Manifest-bound evidence запускается без `-NoBuild`, чтобы runner собрал
актуальные DLL, написал физический TRX и закрепил focused scope:

```powershell
$runRoot = "TestResults\design-conversion-checkpoint-mutation-admission-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-checkpoint-mutation-admission-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-checkpoint-mutation-admission-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна manifest строка, 15 exact method FQN и
не менее 22 physical xUnit rows. Срез доказывает только quarantine/admission boundary,
включая bridge-owned actual-state mismatch, zero-mutation-permit custom-provider
attack в standalone path and typed evaluated continuation path, plus
post-validation first real permit ordering, фактическую custody родительского и
дочернего checkpoint и отсутствие raw checkpoint bytes в сериализуемой паре;
он не заменяет полный
DesignConversion или solution test suite.

## Фокусная проверка endpoint codec загрузчика активированного checkpoint

Этот срез проверяет адрес unit при загрузке активированного
DesignConversion checkpoint из repository manifest. Валидный endpoint — это
ровно `/models/` плюс `Uri.EscapeDataString(expectedUnitId)`, сравнение
`Ordinal`. Важно: encoded identity не является файловым путём. Unit id вроде
`a/b`, `a\b` и literal `a%2Fb` остаются разными допустимыми identity только
когда slash/backslash/percent находятся внутри одного canonical
percent-encoded сегмента.

Отказы fail-closed: query, fragment, absolute URI, authority-relative URI,
wrong prefix, raw extra segment, raw backslash, raw traversal, decoded
`.`/`..` traversal components, malformed `%`, lowercase/noncanonical percent
form, alternate percent spelling and double-identity mismatch. Remote URL
собирается только после отдельной проверки server base URL и endpoint codec; на
сам `/models/<encoded identity>` не применяется общий decoded filesystem path
sanitizer.

```powershell
$runRoot = "TestResults\design-conversion-loader-endpoint-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-loader-endpoint-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-loader-endpoint-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна manifest строка, 6 exact method FQN и
минимум 40 physical xUnit rows после theory expansion. Срез не исправляет и не
доказывает uploader remote manifest hash; uploader проверяется отдельно.

## Фокусная проверка hash-контракта uploader DesignConversion repository

Этот срез проверяет только uploader публикации DesignConversion model repository.
Важный legacy-контракт: поле `ManifestSha256` в `RemoteUnitManifest` означает
SHA-256 фактически загруженных payload bytes по `PolicyCheckpointPath`, а не
отдельный JSON manifest. Для non-eval это те же bytes свежезагруженного runtime
checkpoint; для evaluated DPO загружаются policy bytes, а hash runtime checkpoint
остаётся отдельным `RuntimeProvenanceSha256`. Поэтому fake/local repository и
`ValidateUploadedManifest` должны сравнивать remote `ManifestSha256` с
upload-manifest `ManifestSha256`, который равен policy payload SHA.

```powershell
dotnet build Tests\LlmFromScratch.Tests.csproj -c Release --nologo
$runRoot = "TestResults\design-conversion-model-repository-promotion-uploader-focused"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-model-repository-promotion-uploader-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-model-repository-promotion-uploader-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна manifest строка, 4 exact method FQN и 4
physical xUnit rows. Две mutation проверки обязаны падать закрыто до load,
register и locate, если remote repository вернул `ManifestSha256` не от тех
payload bytes. Этот срез не является full-suite green и не расширяет контракт
`RemoteUnitManifest` отдельными remote `PolicyCheckpointSha256` /
`RuntimeProvenanceSha256` полями.

## Фокусная проверка допуска пары Capability

Этот срез закрывает Prompt 10 Capability/ModelServer bridge как пятый product
bridge. Он проверяет, что `LogicalDpoTrainingUnit` выдаёт admission по raw
`Prompt`/`Chosen`/`Rejected` до `Compact`, требует обе стороны авторства до
tokenizer/optimizer, а current и sealed replay rows получают
`DeterministicGenerator`/`DeterministicGenerator`. Raw compatibility overload в
`AutonomousLearningReplayMixer` остаётся authorless и поэтому не становится
training-ready. ContinuousLearning v5 переносит `OpenTeacher`/`LocalModel` из
rich pair, v4 authorless row падает до logical request/optimizer.
DesignConversion ModelServer handoff копирует directed author kinds из record;
runtime/held-out paths не получают training admission.

Targeted локальная проверка:

```powershell
$capabilityPairAdmissionFilter = @(
    "FullyQualifiedName=LlmFromScratch.Tests.LlmIngredientLogicalUnitTests.LogicalDpoTrainingUnit_ExecutesLoraAndDpoThroughLogicalUnit",
    "FullyQualifiedName=LlmFromScratch.Tests.LlmIngredientLogicalUnitTests.LogicalDpoTrainingUnit_BatchesMultiplePreferencesIntoOneOptimizerStep",
    "FullyQualifiedName=LlmFromScratch.Tests.LlmIngredientLogicalUnitTests.LogicalDpoTrainingUnit_PrepareExecution_MintsAdmissionFromRawPreferenceStrings",
    "FullyQualifiedName=LlmFromScratch.Tests.LlmIngredientLogicalUnitTests.LogicalDpoTrainingUnit_RejectsMissingOneSidedAndUndefinedAuthorsBeforeTokenizerOrOptimizer",
    "FullyQualifiedName=LlmFromScratch.Tests.LlmIngredientLogicalUnitTests.LogicalDpoPreferenceExample_ProductCreationSiteCensus_StaysAtSeven",
    "FullyQualifiedName=LlmFromScratch.Tests.LlmIngredientLogicalUnitTests.LogicalDpoTrainingUnit_RejectsAdversarialRowsBeforePermitOrMutation",
    "FullyQualifiedName=LlmFromScratch.Tests.LlmIngredientLogicalUnitTests.LogicalDpoTrainingUnit_UsesRetainedTokenizerAfterCallerMutationFollowingRevalidation",
    "FullyQualifiedName=LlmFromScratch.Tests.CapabilityResultTranslatorAndDeeperModelTests.LogicalDpoTrainingUnit_UsesSameLivePolicyAndReapplyIsIdempotent",
    "FullyQualifiedName=LlmFromScratch.Tests.AutonomousLearningReplayMixerTests.BuildPreferences_MixesReplayWithoutConstructingProject16ReplayBuffer",
    "FullyQualifiedName=LlmFromScratch.Tests.AutonomousLearningReplayMixerTests.BuildPreferences_DeletedReplayReceiptCannotDowngradeToLegacyReplay",
    "FullyQualifiedName=LlmFromScratch.Tests.AutonomousLearningReplayMixerTests.BuildPreferences_RawCompatibilityOverloadKeepsAuthorKindsAbsent",
    "FullyQualifiedName=LlmFromScratch.Tests.DefaultLogicalCapabilityTrainerTests.DefaultLogicalCapabilityTrainer_UsesOneLeaseForExactCanonicalBatchAndAllMutations",
    "FullyQualifiedName=LlmFromScratch.Tests.DefaultLogicalCapabilityTrainerTests.DefaultLogicalCapabilityTrainer_AdmissionBatchExactlyMatchesIndependentlyReconstructedCurrentAndReplayRows",
    "FullyQualifiedName=LlmFromScratch.Tests.IncrementalTrainerDpoWiringTests.StableDpoAdapter_PureMappingPreservesOwnedRequestWithoutExecution",
    "FullyQualifiedName=LlmFromScratch.Tests.IncrementalTrainerDpoWiringTests.StableDpoExecutor_ProductionRequestAndMutationSinkSharePreparedInputs",
    "FullyQualifiedName=LlmFromScratch.Tests.IncrementalTrainerDpoWiringTests.StableDpoExecutor_V4AuthorlessPairStopsBeforeLogicalRequestOrOptimizer",
    "FullyQualifiedName=DesignConversionModelServerApiTests.DpoTrainingHandoff_DeepSnapshotsCallerCollectionsBeforeAuthorityLease",
    "FullyQualifiedName=DesignConversionModelServerApiTests.DesignConversionDpoTrainingHandoff_MissingPairAuthorsFailBeforeProvider"
) -join "|"
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --nologo --filter $capabilityPairAdmissionFilter
```

Manifest-bound evidence запускается без `-NoBuild`: runner сам собирает
актуальные DLL, пишет физический TRX и закрепляет focused scope. Verifier должен
запускаться с тем же manifest и `-Focused`.

```powershell
$runRoot = "TestResults\capability-pair-admission-focused-p10-20260828-001"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\capability-pair-admission-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\capability-pair-admission-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый scope: `verification_scope=FOCUSED`,
`full_suite_claim_allowed=false`, одна строка manifest, 18 exact FQN и 18
физических xUnit rows. Семь admission/fail-closed rows доказывают raw
fingerprint/issuer, missing/one-sided/undefined author fail-before-tokenizer,
raw helper authorless, старую live-policy synthetic пару с явными
`DeterministicGenerator`/`DeterministicGenerator`, v4 authorless
fail-before-optimizer, ModelServer missing authors fail before provider и
product creation-site census exactly five. Остальные rows — touched regressions
для current/replay deterministic admission, v5 rich author copy, ModelServer
successful handoff copy и existing logical DPO execution. Два ModelServer
метода остаются в global xUnit namespace как
`DesignConversionModelServerApiTests.*`; это текущий `--list-tests` readback, а
не сокращённый фильтр. Это не full suite и не самостоятельный protected trainer
enforcement run; trainer-door focused manifest закрывает `Step`/`StepMany`/
`StepVector` как `3/3`.

## Изоляция DesignConversion model-server и machine-тестов

Тесты локальной DPO-публикации model-server и прямые тесты `DesignConversionMachine`
используют один in-process transformer/DPO runtime. Поэтому проверка качества не
должна зависеть от того, какой соседний тест первым построил `LlmModel` и
сдвинул process-wide инициализацию весов. Тестовый DPO provider создаёт
policy/reference/fresh-load base models внутри явного deterministic seed scope,
а route-level regression отдельно строит несколько “шумовых” моделей перед
локальной публикацией.

Это правило не ослабляет качество. Если обученная модель набрала меньше
требуемого порога, локальная публикация остаётся неготовой; тестовая изоляция
только гарантирует, что один и тот же сценарий получает один и тот же старт.

Две проверки модуля переноса обученных весов в модель для запуска уточнены.
Если перед последней записью прежний файл подменён другим физическим файлом с
тем же содержимым, откат не может доказать, что исходный файл сохранён. Поэтому
модуль сообщает, что состояние публикации неизвестно, а не выдаёт обычную ошибку
записи. Если два параллельных исполнителя по очереди записывают точно одно
содержимое, второй запуск безопасно повторяет первый. Попытка заменить
существующие данные отличающимся содержимым по-прежнему проверяется отдельно.
Эти проверки относятся только к подмене файла и порядку записи; они не
доказывают, что прошли все тесты проекта.

## Чтение старых форматов обучающих пар в защищённом каталоге Linux

Тесты `DpoPairSchemaV2Tests` проверяют, что уже сохранённые записи схемы 1 и
исторические записи схемы 2 читаются побайтно: идентификатор пары, отсутствие новых
метаданных и буквальный JSON не переписываются ради текущей версии. Эти записи
создаются вручную, минуя обычный писатель, поэтому на Linux временный каталог
фикстуры сразу должен быть приватным для владельца. Файл пары остаётся доступен
только владельцу, как и раньше.

Это не ослабляет защиту хранилища. Если каталог уже существует с небезопасными
правами, `FileDpoPairStore` должен отказать в доступе и не менять или чинить
права за вызывающий код. Поэтому рядом с чтением старых форматов выполняются
четыре Linux-проверки безопасности корня, файла блокировки и канонической записи; они
подтверждают, что небезопасный каталог не превращается в опубликованные данные.

## Сохранение и повтор двух связанных обучающих пар

`DpoPairAtomicGroupPersistenceTests` проверяет пару связанных записей как одну
атомарную группу: оборванный временный файл не должен раскрывать одну из двух
связанных записей, старый журнал повтора должен резервировать всю группу, а
завершённая транзакция должна оставить одну запись журнала, запрещающую
повторную обработку родительского запроса. Часть этих сценариев вручную создаёт
временный файл, журнал или сохранённые параметры модели, минуя обычный
продуктовый писатель.

На Linux такие тестовые файлы тоже должны создаваться через защищённую файловую
границу с правами владельца. Иначе тест проверяет случайные права временного
каталога теста, а не сохранение двух связанных обучающих пар и не повтор после
перезапуска. При этом ожидаемые байты, хеши, идентификаторы группы и правила
допуска не меняются: небезопасные каталоги и файлы по-прежнему проверяются
отдельными Linux-сценариями отказа.

## Восстановление завершённой операции обучения и прерванной очистки журнала

`DpoPairTransactionRegressionTests` проверяет одиночную обучающую пару в трёх
сценариях: уже опубликованная операция должна пережить перезапуск, прерванная
очистка журнала должна сходиться при повторе, а старый журнал повтора схемы 1
должен по-прежнему резервировать исходный запрос. Эти сценарии специально пишут
исторический журнал и сохранённые параметры модели вручную, чтобы проверить
чтение старых форматов, а не текущий путь записи.

На Linux такие временные файлы теста создаются с доступом только владельцу.
Файл сохранённых параметров модели записывается в уже открытый защищённый поток
и закрывается до начала изменения данных, но резервирование кандидата
удерживается до завершения операции. Это сохраняет те же байты, хеши и ожидания
транзакции, но не позволяет случайным правам временного файла подменить
проверку восстановления.

## Повторный допуск корпуса через одно native-хранилище обучающих пар

После переноса хранилища обучающих пар с Windows-диска в native-файловую
систему WSL тест повторного запуска не должен вручную собирать старый путь
`<artifact-root>/training-intake-dpo-pairs`. Фактический каталог берётся из
`TrainingIntakeDpoPairStoreRootResolutionForTesting`. Тест сравнивает весь
паспорт привязки до и после повторного открытия базы: путь, manifest,
fingerprint артефактного корня и физические поля device/inode/effective user.
Так совпадение одной строки пути нельзя ошибочно принять за отсутствие
split-brain между двумя хранилищами.

Linux-only manifest
`tools/test-batches/design-conversion-training-intake-dpo-pair-store-replay-focused.tsv`
запускает ровно три метода: обычный повтор после перезапуска, конкурентный
повтор и отказ при исчезновении уже привязанного native-каталога. В WSL
дополнительно проверяется существование manifest и отсутствие записей в старом
каталоге на `/mnt/c`.

```powershell
$runRoot = Join-Path $PWD "TestResults\design-conversion-training-intake-dpo-pair-store-replay-focused-$(Get-Date -Format 'yyyyMMddTHHmmssZ')"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-training-intake-dpo-pair-store-replay-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\design-conversion-training-intake-dpo-pair-store-replay-focused.tsv -RunRoot $runRoot -Focused
```

Ожидаемый результат — три выполненных и прошедших теста без исключений. Этот
узкий прогон доказывает только единство pair-store и отсутствие повторной
машинной мутации; он не заменяет исходную группу тестов и полный прогон решения.

## Публикация manifest по способности платформы, а не по имени ОС

`TeachingRegistryPersistenceTests.ManifestIdentityCustody_CapabilityContractPublishesOrFailsBeforeMutation`
защищает контракт публикации manifest для обученной способности. Исторически
проверка ожидала отказ на любой не-Windows системе, потому что безопасная
опека файловой личности тогда была только Windows-веткой. После добавления
Linux x64 пути этот смысл изменился: поддерживаемая платформа обязана
безопасно опубликовать manifest и прочитать его через inventory, а
неподдерживаемая платформа обязана вернуть прежний стабильный код отказа до
создания каталога или файла. Старое строковое имя кода отказа сохранено ради
совместимости журналов, но тест больше не трактует его как запрет Linux.

Если меняется публичный конструктор `JsonlDesignConversionLocalArtifactDb`,
проверка должна покрывать два поколения входов одновременно. Прежние три
отражаемые подписи заканчиваются `provider=null` и нужны старым двоичным и
отражательным вызовам. Нынешние три подписи дополнительно принимают скалярные
настройки хранилища обучающих пар через именованный аргумент
`trainingIntakeDpoPairStoreOptions`. Приоритет перегрузки должен оставлять
вызовы без этих настроек на прежнем входе, а вызовы с настройками — на нынешнем;
оба пути обязаны приходить в одну общую инициализацию.

Для контракта отражения метода `Program.CreateLocalLoopbackRouteRegistry` тест
закрепляет такой порядок параметров по именам: `artifactDb`,
`dpoPolicyBaseProvider`, `measurementGateThresholdConfiguration`, затем
`trainingIntakeDpoPairStoreOptions`. Настройка порога измерительной проверки
должна оставаться третьим параметром, а настройка хранилища обучающих пар —
четвертым. Оба параметра необязательные, допускают `null` и имеют значение по
умолчанию `null`; поэтому старый вызов с тремя позиционными аргументами
по-прежнему передает третьим аргументом именно порог. Проверка «последний
параметр — порог» для `Program.CreateLocalLoopbackRouteRegistry` неверна.
Такой строгий контракт остается только у метода
`Program.CreateLocalLoopbackServiceComposition`.

Минимальный ручной фильтр для такого изменения:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --no-build --nologo --filter "FullyQualifiedName=DesignConversionDpoStorageTests.LocalArtifactDb_PublicConstructorSurface_PreservesQuotaPrefixesWithOptionalProviderTail|FullyQualifiedName=DesignConversionDpoStorageTests.LocalArtifactDb_ExplicitUploaderNull_BindsToLegacyUploaderConstructor|FullyQualifiedName=DesignConversionDpoStorageTests.LocalArtifactDb_PublicConstructors_BindLegacyPrefixesAndNamedOptions|FullyQualifiedName=Tests.DesignConversionTrainingIntakeDpoPairStoreRootResolverTests.ProductConstructor_ExplicitNativeRoot_UsesResolverWithoutInjectedPairStore"
```

## Общая подготовка данных для проверки допуска обучающих пар дизайна-в-код

Тесты допуска корпуса используют общий вспомогательный метод подготовки
обучающей пары из `DesignConversionLocalIndependentPreferenceEvaluatorTests`.
Вызывать его нужно напрямую, а не через поиск и вызов по строковому имени:
компилятор проверяет переданные аргументы при сборке. Иначе тест может упасть
на `TargetParameterCountException` ещё до проверки допуска корпуса, хранилища и
отсутствия изменений модели без разрешения.

Сценарии локальной публикации, которые проверяют журнал, ограничение его размера,
срок действия серверного разрешения и очистку после отзыва, используют заранее
заданные проверочные пары. Вспомогательная функция отдельно проверяет
сохранность и происхождение содержимого, разные источники и то, что для этих
проверочных данных отдельный допуск не требуется. Поэтому тесты доходят до участка, где
проверяются журнал, разрешение и отзыв, без ослабления защиты для обучающих пар.

Для пар, которым нужен отдельный допуск набора данных, локальная публикация
получила только внутренний вход. Он принимает внутренний запрос с
подтверждением допуска из того же процесса, снова сверяет текущие записи,
доказательство происхождения, подтверждение независимого сравнения и
сохранённый окончательный результат при повторном запросе. Публичная точка входа
без такого подтверждения по-прежнему останавливает эти данные до исполнителя и
до изменения модели.

Исторически этот внутренний путь останавливался после одной серверной внешней
транзакции для уже допущенной группы: мост локальной публикации ещё не умел
принять подтверждённую группу. Текущий срез закрывает именно этот разрыв: после
зафиксированной внешней транзакции локальный хвост получает дочерние строки из
хранилища, продолжает модель локально, пишет local-only receipt и возвращает
durable local-ready record. Это всё ещё не означает удалённую загрузку на
model-server и не означает автоматическую активацию модели в production.

Сценарий внешнего учителя с разрешением остаётся отдельной открытой проблемой.
Старый положительный тест в `Tests/DesignConversionDpoStorageTests.cs`
(`LocalArtifactDb_RunTrainingIntakeAndPromoteLocalAsync_UsesInjectedLegalTeacherAuthorityForAllowlistedExternalTeacher`)
проверен и сейчас падает на неверном авторстве обычной проверочной пары. Запись
допущенного набора данных и передача полномочия в мост публикации весов — это
следующие разрывы, а не непосредственная причина первого отказа. Это не
доказательство качества настоящей модели Qwen и не обучение рабочей модели.

Отдельные отрицательные проверки удерживают границу безопасности: кандидат без
подтверждённого независимого сравнения остаётся в карантине, валидная пара без
внутреннего подтверждения допуска не проходит публичный вход локальной
публикации, а повторный запрос не возвращает сохранённый результат, если
подменены подтверждение независимого сравнения или доказательство происхождения.

Повреждённая форма внутреннего запроса тоже должна закрываться управляемым
отказом до исполнителя: нет пары, нет подтверждения независимого сравнения, нет
доказательства происхождения, отсутствует список обучающих или контрольных
строк, либо строка внутри прежнего объекта заменена на `null`. Такой отказ
возвращает код невалидного допуска и не создаёт журнал локальной публикации,
каталог вывода или файлы атомарной внешней транзакции.

Безопасная узкая повторная проверка запускается из корня этой рабочей копии
после сборки. Она берёт внутренний успешный путь, его повтор, старые публичные
отказы без допуска, клонированное поддельное доказательство происхождения и
подмену строки внутри того же самого объекта доказательства происхождения. Один
метод является параметризованным тестом и разворачивается в семь физических
строк результата, поэтому ожидаемый итог узкого запуска — 9 имён методов и 15
физических строк результата.

```powershell
$filter = "FullyQualifiedName=Tests.DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_CommitsExternalConsumptionOnceAndExactReplayHasNoNewEffects|FullyQualifiedName=Tests.DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_TamperedReplayReceiptCannotReturnCachedTerminal|FullyQualifiedName=Tests.DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_InvalidSourceProofFailsBeforeRunnerOrArtifacts|FullyQualifiedName=Tests.DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_SameObjectSourceProofMutationFailsBeforeRunnerOrCachedReplay|FullyQualifiedName=Tests.DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_MalformedRequestShapeFailsInvalidBeforeRunnerOrArtifacts|FullyQualifiedName=Tests.DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_NonCorpusIntakeRequestFailsBeforeOrdinaryPromotion|FullyQualifiedName=Tests.DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithoutCorpusAdmission_RejectsValidDualTargetTeacherPartitionBeforeExternalConsumptionOrMutation|FullyQualifiedName=Tests.DesignConversionDpoCorpusAdmissionTests.LocalPromotionIdempotentReplay_RevalidatesCorpusAdmissionRequirementBeforeReturningCachedSuccess|FullyQualifiedName=Tests.DesignConversionDpoCorpusAdmissionTests.RealTrainingIntake_AdmitsOnceAfterStoredRevalidationAndRestartReplayHasZeroEffects"
wsl.exe -d Ubuntu-24.04 --cd /mnt/c/Projects/llm_mvp_core --exec /usr/bin/env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT /usr/bin/timeout 180s dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --nologo --filter $filter
```

Этот прогон доказывает работоспособность подготовки тестовых данных и входа в
проверку продукта. Он не доказывает качество обучения, не делает внешний
сценарий учителя зелёным и не заменяет все проверки модуля дизайна-в-код.

## Фактический каталог хранилища пар при атомарной записи дизайна-в-код

Проверки атомарной записи обучающих пар должны смотреть не на исторический путь
под корнем артефактов, а на сохранённое решение базы о каталоге. В WSL
защищённое хранилище может находиться в отдельном каталоге внутри Linux,
поэтому тест читает `PairStoreRootDirectory` из текущей базы и уже там проверяет
отсутствие одиночных, групповых и транзакционных файлов, одну запись внешнего
потребления, журнал повторов и запрет повторного изменения данных.

Если вместо построчного журнала локального продвижения появляется каталог,
результат зависит от платформенного способа открытия файла: Linux отвергает
такую подмену как нарушение вида файла, а Windows возвращает обычный отказ чтения
через системное открытие «только обычный файл». В проверяемом сценарии сбой
создаётся на границе запуска: успевает сохраниться одна запись о потреблении
обучающей группы, но локальная модель не публикуется. Повтор с тем же ключом
должен останавливаться ранним отказом восстановления до наблюдателей и вызова
исполнителя: он не вызывает поставщика модели и не меняет сохранённые байты.
Короткая проверка трёх точных сценариев в этом месте доказывает только эту
защиту восстановления после сбоя; она не доказывает повторное обучение весов
на старой версии и не заменяет полный набор проверок модуля дизайна-в-код.

## POSIX retained custody для Continuous Learning

Linux/WSL-проверки `ServerOwnedContinuousLearningFileAccessTests` защищают не
строку пути, а удержанную файловую личность. Обязательные регрессии: успешный
happy path с независимым `openat`-чтением base checkpoint, отказ от final
symlink, sticky loss после delete/recreate артефакта, sticky loss после замены
root-каталога и отказ от group-writable terminal root.

Эти тесты Linux x64-only: другой Unix не должен автоматически наследовать
константы `openat`, `O_NOFOLLOW` и layout `stat`. Windows-тесты остаются
отдельными, потому что они доказывают более сильную handle/share-mode
гарантию, а не advisory-lock модель Linux.

После POSIX retained проверки отдельно прогоняются affected model-server groups
и capability-классы `ServerOwnedContinuousLearningCacheRootCapabilityTests`,
`ServerOwnedContinuousLearningAdapterRootCapabilityTests` и
`ServerOwnedContinuousLearningRegistryRootCapabilityTests`. Эта полоса нужна,
чтобы поймать не только допуск base checkpoint, но и честную Linux-границу для
cache/adapter/registry: retained directory descriptor, no-follow opens,
fail-closed exact delete/replace, create-only adapter/registry publication и
запрет fallback на строковые пути.

Capability-полоса намеренно разделена по платформам. Windows-классы остаются
источником handle/share-mode/delete-pending oracle и запускаются через project
host, потому что cross-process owner-lock helper берёт `DOTNET_HOST_PATH` из
`dotnet test`; прямой `dotnet vstest <dll>` на Windows запускает helper через
`testhost` и может дать инфраструктурный `testhost.runtimeconfig.json` failure.
Linux/WSL запускает direct DLL и выполняет POSIX facts вместе с platform-free
public-contract checks; только старые Windows handle/share-mode/delete-pending
assertions должны быть discovery-time xUnit skip. Runtime `SkipException` здесь
не подходит: VSTest 17.12 показывает его как failure с `$XunitDynamicSkip$`.

Для durable mutation lock есть отдельная focused-приёмка:
`tools/test-batches/design-conversion-durable-mutation-lock-focused.tsv`.
Она проверяет не “Windows delete-share везде”, а переносимое ожидание sidecar
lease: Windows contention — только `32/33`, Linux contention — только наша
нормализация errno `11` внутри acquire-пути sidecar-а, неизвестный
`IOException` не запускает retry-цикл. Это важно для Linux/WSL: занятость может
прийти как от .NET-открытия файла, так и от явного `flock`, но только внутри
`DesignConversionDeterministicMutationLock`. Сценарии отмены обязаны доказать
отсутствие новых байтов, journal/cache мутаций и повторов до освобождения lease;
checkpoint writer-lock сценарии проверяют точный конфликт без записи
checkpoint/temp. Destination/index/journal read-only oracles остаются отдельными
проверками и не считаются доказательством этого lease-контракта.

### Честный статус Windows-only проверок в полном WSL-прогоне

Если тест доказывает Windows handle-relative семантику, Windows file-identity
custody или Windows retained capability, он не должен падать в WSL только
потому, что платформа другая. Правильный статус для такого теста вне Windows —
`NotExecuted`: тест виден в отчёте, но честно помечен как требующий Windows.

Для этого у Windows-only семейств используются локальные `Fact`/`Theory`
атрибуты, которые выставляют `Skip` во время обнаружения тестов. Метод
`RequireWindows()` остаётся внутри тела как второй предохранитель: если тест
случайно вызвали в обход xUnit discovery, он всё равно не начнёт доказывать
Windows-контракт на Linux. Но один только `Assert.True(OperatingSystem.IsWindows())`
не подходит для полного WSL-прогона: он превращает границу платформы в красный
дефект продукта и мешает видеть настоящие падения.

Первый исправленный срез 26.08.2026 переводит на такой статус четыре семейства:
retained registry publication, canonical semantic-compiler cache, retained
checkpoint-backed training session factory и server-owned continuous-learning
file access. Windows-прогон этих тестов остаётся обязательным: `NotExecuted` в
WSL не означает “проверка пройдена”, он означает “эту часть нужно доказывать на
Windows-раннере”.

Приёмочная проверка этого среза запускает ровно эти четыре семейства, чтобы
убедиться в двух вещах: Windows-only строки видимы в discovery и получают
`NotExecuted`, а переносимые Linux/POSIX строки в тех же файлах продолжают
исполняться.

```powershell
wsl -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet vstest Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll /TestCaseFilter:"FullyQualifiedName~CanonicalSemanticCompilerCacheTests|FullyQualifiedName~RetainedCheckpointBackedIncrementalTrainingSessionFactoryTests|FullyQualifiedName~RetainedDomainModelRegistryTests|FullyQualifiedName~ServerOwnedContinuousLearningFileAccessTests" /Logger:"trx;LogFileName=windows-only-status-slice.trx"'
```

Контрольное свидетельство среза 26.08.2026 после Linux-сборки: `Failed 0`,
`Passed 13`, `Skipped 97`, `Total 110`; все 97 строк `NotExecuted` несут
причину `WINDOWS_ONLY_CAPABILITY_ORACLE`. Это не постоянный норматив полного
прогона: в новых прогонах считать нужно фактические строки `UnitTestResult`,
а не поле шапки TRX.

Срез 31.08.2026 для локальной базы артефактов добавляет ту же честную форму к
четырём проверкам открытых файлов Windows. Сценарии читаются так:

1. Повторное добавление использует готовый индекс и не перечитывает
   заблокированный журнал.
2. Отказ записи в заблокированный журнал не добавляет новую запись в индекс.
3. Журнал и индекс сохраняются при запрете замены файла, а затем та же операция
   успешно проходит повторной попыткой.
4. Подтверждение сохраняется при заблокированном индексе, а повторный запрос
   восстанавливает индекс.

После этих человеческих сценариев команда ниже перечисляет точные имена
методов, чтобы проверяющий запускал только нужные строки тестов. На Linux/WSL
четыре Windows-проверки должны давать `NotExecuted` с понятной причиной, а два
соседних переносимых теста ротации журнала публикации должны реально
выполняться.

```powershell
wsl.exe -d Ubuntu-24.04 --cd /mnt/c/Projects/llm_mvp_core --exec env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT timeout 180s dotnet test /mnt/c/Projects/llm_mvp_core/Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll --filter 'FullyQualifiedName=DesignConversionDpoStorageTests.LocalArtifactDb_SteadyStateAppendUsesCachedIndexWithoutRereadingRecordsAcrossInstances|FullyQualifiedName=DesignConversionDpoStorageTests.LocalArtifactDb_AppendFailureDoesNotAdvanceIndexOrExposePhantomRecord|FullyQualifiedName=DesignConversionDpoStorageTests.LocalArtifactDb_ApprovalRewriteFailureKeepsAuthorityAndIndexAndAllowsRetry|FullyQualifiedName=DesignConversionDpoStorageTests.LocalArtifactDb_ApprovalRewriteLockedIndexReturnsDeferredSuccessAndReplayHealsIndex|FullyQualifiedName=DesignConversionDpoStorageTests.LocalPromotionJournalSet_OperationPressureRotatesPairWithZeroByteProjection|FullyQualifiedName=DesignConversionDpoStorageTests.LocalPromotionJournalSet_ProjectionOnlyPressureRotatesCoordinatedGeneration' --logger 'trx;LogFileName=windows-open-file-status-slice.trx' --results-directory /mnt/c/Projects/llm_mvp_core/TestResults/<run-root>
```

Приёмка этого среза считает физические строки `UnitTestResult`: ожидается 2
`Passed` для переносимых соседей и 4 `NotExecuted` для четырёх проверок
открытых файлов Windows. Это не доказывает исполнение этих четырёх методов на
Windows; для этого нужен отдельный запуск в Windows тем же точным набором.

Для сравнения с полным опубликованным ccda-прогоном нельзя переносить старые
фиксированные числа из предыдущих carrier'ов. Актуальная pre-diff база по двум
test assembly перед этим срезом: `11308` total, `10820` passed, `396` failed и
`92` NotExecuted по фактическим строкам `UnitTestResult`. Следующий полный
прогон пересчитывается заново; focused-свидетельство `97` skipped выше
доказывает только эти четыре семейства и не заменяет полный Windows-lane долг.

Полный batch-прогон на срезе `f91d6c36` читается как product RED: RunRoot
`TestResults/batches-full-f91d6c36-20260829T193803Z` имеет `FINISHED`, projected
current rows `18`, raw journal rows `19`, total `11571`, passed `11188`, failed
`124`, not executed `259`, evidence problems `0`, но full-suite green всё равно
ложен из-за реальных failed/not-executed строк. Лишняя raw-строка — это
superseded `LEAF_INCOMPLETE`, а не второй batch.

Фактическая матрица для среза 26.08.2026:

```powershell
dotnet build Tests\LlmFromScratch.Tests.csproj -c Release --nologo
```

Ожидание: `Build succeeded`, `0 Error(s)`.

```powershell
wsl -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet build 14.ContinuousLearning/ContinuousLearning.csproj -c Release -nr:false'
```

Ожидание: `Build succeeded`, `0 Error(s)`. Для fresh WSL test DLL выполняется
тот же test-project build в Linux:

```powershell
wsl -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet build Tests/LlmFromScratch.Tests.csproj -c Release -nr:false'
```

Ожидание: `Build succeeded`, `0 Error(s)`. Возможны старые analyzer/platform
warnings из unrelated projects; они не являются POSIX acceptance gate, пока не
стали ошибками. Ссылка test-project на `43.SmartSkillsDomain.Dashboard`
закреплена на `net9.0`, чтобы Linux SDK не пытался импортировать
`Microsoft.NET.Sdk.WindowsDesktop` для `net9.0-windows`.

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --no-build --nologo --filter "FullyQualifiedName~ServerOwnedContinuousLearningFileAccessTests|FullyQualifiedName~ServerOwnedContinuousLearningCacheRootCapabilityTests|FullyQualifiedName~ServerOwnedContinuousLearningAdapterRootCapabilityTests|FullyQualifiedName~ServerOwnedContinuousLearningRegistryRootCapabilityTests"
```

Ожидание на Windows: `Passed 124`, `Skipped 24`, `Failed 0`,
`Total 148`. Skipped здесь — только POSIX-only tests.

```powershell
wsl -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet vstest Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll --TestCaseFilter:"FullyQualifiedName~ServerOwnedContinuousLearningCacheRootCapabilityTests|FullyQualifiedName~ServerOwnedContinuousLearningAdapterRootCapabilityTests|FullyQualifiedName~ServerOwnedContinuousLearningRegistryRootCapabilityTests"'
```

Ожидание в WSL Ubuntu 24.04/.NET 9: `Passed 28`, `Skipped 75`,
`Failed 0`, `Total 103`. Passed здесь — Linux x64 public capability checks для
no-follow symlink/ELOOP rejection, POSIX fail-closed exact delete/replace,
deterministic substitution-window survival, adapter direct create-only publish,
registry direct create-only first publication, root delete/recreate sticky loss,
unsafe owner-lock mode и hardlink rejection плюс platform-free
adapter/registry contract checks. Skipped — только Windows-only capability
oracle, который не считается passed; в TRX это `NotExecuted 75`.

```powershell
wsl -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet vstest Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll --TestCaseFilter:"FullyQualifiedName~Posix"'
```

Ожидание: `Passed 24`, `Skipped 0`, `Failed 0`, `Total 24`
— 7 прямых retained file-access POSIX tests и 17 POSIX capability tests.

```powershell
wsl -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet vstest Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll --TestCaseFilter:"FullyQualifiedName~ContinuousLearningModelServerCompositionTests|FullyQualifiedName~ContinuousLearningModelServerReadinessEndpointTests"'
```

Ожидание для affected model-server regression: `Passed 214`, `Skipped 0`,
`Failed 0`, `Total 214`.

### Батчевый полный прогон с защитой от зависания

Полный прогон больше не должен зависеть от одного трёхчасового процесса. Для
этого добавлен раннер `tools/test-batches/run-test-batches.sh` и PowerShell
обёртка `tools/test-batches/run-test-batches.ps1`. Они читают лестницу
`tools/test-batches/batches.tsv`, запускают батчи по одному через прямой
`dotnet vstest` по уже собранной DLL и сразу после каждого батча пишут
корневые маркеры `state.txt`, `exit.txt`, строку в `journal.tsv` и отдельную
папку попытки `attempt-NNNN/`. Внутри попытки лежат её собственные
`result.trx`, `state.txt`, `exit.txt`, `stdout.log`, `stderr.log`,
`effective-filter.txt` и `blame-hang.runsettings`.

Перед первым батчем runner собирает все исполняемые файлы, которые тесты потом
используют как готовые артефакты: основную тестовую сборку, сборку включения
модели конверсии дизайна и локальный серверный инструмент
`tools/DesignConversionLiveLocalServer`. Это важно для честного красного
сигнала: проверки локального сервера должны падать по поведению продукта, а не
по тому, что `DesignConversionLiveLocalServer.dll` не был подготовлен перед
запуском батчей. Ключ `--no-build` оставляет эту ответственность вызывающему.

Если последняя страховочная строка со звёздочкой (`*`) для конкретной тестовой
сборки стала реально пустой, запускатель не должен превращать это в зелёный
файл результата (`TRX`). Он пишет отдельное состояние `EMPTY_VERIFIED`,
сохраняет полный список методов сборки и доказательство, что все методы уже
назначены более ранним группам, но не создаёт попытку запуска тестов через
VSTest и не запускает пустой фильтр или `*`. В сводке такая строка считается
отдельно от прошедших тестов: физических результатов (`UnitTestResult`) у неё
ноль. Проверяющий скрипт принимает это только для последней остаточной строки
основного списка групп и отвергает именованную пустоту, произвольный список
групп или выбор отдельных методов (`custom`/`focused`), устаревшую квитанцию,
пустой или неуспешно полученный список тестов (`ListTests`), изменение
сохранённого списка и новый метод, который должен был попасть в остаток.

Короткая проверка этой ветви запускается без сборки и без полного набора тестов:

```powershell
wsl -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && bash tools/test-batches/run-test-batches.sh --no-build --self-check-empty-residual --run-root "TestResults/empty-residual-self-check-$(date -u +%Y%m%dT%H%M%SZ)"'
```

Для самопроверки проверяющего скрипта можно вызвать только его новый маршрут:

```powershell
C:\Users\lubao\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\powershell\pwsh.exe -NoLogo -NoProfile -File tools\test-batches\verify-test-batches.ps1 -SelfCheckEmptyResidual
```

Ready/health JSON локального сервера публикует `sourceIdentity` только как
безопасную идентичность запущенного кода. Фактический `sourceCommit` берётся из
assembly metadata, затем из informational version, затем из `git rev-parse HEAD`
текущего working directory; если git недоступен, поле остаётся честно
неполным, а сервер не падает. `expectedSourceCommit` из
`--expected-source-commit` или переменной окружения — только ожидание для
сравнения и никогда не становится fallback-источником actual commit. Публичные
ready/health поля не должны включать stderr git, локальные пути репозитория,
пути DLL, пользовательские каталоги или `file://` URI.

Browser/PNG readiness локального сервера доказывается двумя разными слоями.
`browserRendererRuntimeAvailable` означает только то, что server-owned resolver
нашёл Chromium-family executable; это ещё не зелёный рендер. Для
machine-backed local обязательный startup proof публикуется отдельно:
`browserRendererPngProofRequired`, `browserRendererPngProofAttempted`,
`browserRendererPngProofValid`, `browserRendererPngProofStartupStatus`,
`browserRendererPngProofNonReadyCode`, счётчики expected/observed viewport и
число materialized PNG evidence. Зелёный PNG proof требует реальных PNG-байтов,
проверенной PNG-signature, sha256, размера viewport и nonblank pixels; отсутствие
браузера, crash Chromium или невозможность записать render-artifacts остаются
`bounded_non_ready`, а не `productReady=true`.

Перед browser-зависимым прогоном можно проверить среду так:

```powershell
Get-Command chromium,chromium-browser,google-chrome,google-chrome-stable,chrome,msedge -ErrorAction SilentlyContinue | Select-Object Name,Source
Test-Path 'C:\Program Files\Google\Chrome\Application\chrome.exe'
wsl -d Ubuntu-24.04 -- bash -lc 'command -v chromium || command -v chromium-browser || command -v google-chrome || command -v google-chrome-stable || true'
```

Для live-local проверки используйте focused test или runner с TRX, не
подменяющий браузер заглушкой. Если браузер найден, но PNG materialization не
прошла, ожидаемый публичный итог — `browserRendererPngProofValid=false`,
`browserRendererPngProofStartupStatus=bounded_non_ready` и конкретный
`browserRendererPngProofNonReadyCode`, например
`MACHINE_BACKED_API_PNG_MATERIALIZATION_FAILED`.
В WSL/контейнерном запуске от пользователя `root` Chromium-family runtime
требует `--no-sandbox`; live-local provider добавляет его только в этом
Linux-root случае, а сетевой контур остаётся закрытым через strict CSP и
`--host-resolver-rules=MAP * ~NOTFOUND`.
Публичные response/status JSON для этого же среза остаются refs/hash-only:
Linux temp paths вроде `/tmp/design-conversion-live-local-server-tests/...`
из checkpoint/runtime warnings хешируются тем же leakage guard, потому что
зелёный PNG proof не должен раскрывать локальную файловую топологию testhost.

Корневой `result.trx` появляется только после проверки текущей попытки:
файл должен быть непустым, должен парситься как TRX и должен содержать
фактические строки `UnitTestResult`. Поэтому старый результат в корне батча
больше не может закрыть новый оборванный запуск: если `current-attempt.txt`
указывает на попытку без `result.trx`, verifier считает батч незакрытым.
Проверка выполняется полноценным XML-парсером через стандартную библиотеку
Python 3 в WSL: runner требует корректный XML, хотя бы одну строку
`UnitTestResult`, числовые `Counters` (`total`, `executed`, `passed`, `failed`,
`notExecuted`) и точное совпадение счётчиков с фактическими outcome-строками.
Поддерживаются только `Passed`, `Failed` и `NotExecuted`: неизвестный исход
закрывается как недействительный. Обязательно выполняются равенства
`executed = Passed + Failed` и `total = executed + NotExecuted`; подробные
семейства `error`, `timeout`, `aborted` и подобные обязаны быть числовыми
нулями. В честных TRX от VSTest служебный атрибут `Counters.notExecuted` бывает
нулём даже при физических строках `NotExecuted`, поэтому для него допустимы
только ноль или точное физическое число; каноническая часть суммы берётся из
самих строк. Все остальные расхождения остаются битым результатом.
Файл, который только текстом похож на TRX, остаётся `INVALID_RESULT` и не
копируется в корень батча.

Минимальное число результатов (`min_rows`) из `tools/test-batches/batches.tsv`
закрывается только физическими строками `UnitTestResult`. Для обычного режима
это строки корневого TRX текущей попытки; для группы из небольших порций
(`leaf-pack`) — сумма TRX закрытых порций (`leaf`), а не оценка из
`leaf-plan.tsv` и не числа из `leaf-journal.tsv` или корневого `journal.tsv`.
Если закрытый зелёный или красный результат ниже порога, запускатель (`runner`)
записывает `INVALID_TOO_FEW_ROWS`, а проверяющий скрипт (`verifier`) возвращает
ошибку даже при `-AllowIncomplete`. Этот флаг нужен только для честного
незавершённого каталога результатов (`RunRoot`) после
`MaxLeaves`/`MaxElapsedMinutes`; он не отмывает старый зелёный результат ниже
минимума. Явные `NotExecuted` входят в `total`, но не прибавляются к `passed`.

### Сводчик уже собранных батчей

После батчевого запуска используйте переносимый сводчик:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\summarize-test-batches.ps1 `
  -RunRoot .\TestResults\batches-<slice>\run-<utc> `
  -AllowIncomplete
```

Он не запускает тесты повторно. Его задача — прочитать уже созданный
`journal.tsv`, закрытые `result.trx` и, если батч делился на маленькие порции,
`leaf-journal.tsv`; затем записать два человеческих файла рядом с результатом:
`summary.md` и `notrun-report.md`. Это отдельный слой поверх запускателя:
запускатель сохраняет попытки, проверяющий скрипт ловит битую структуру, а
сводчик отвечает человеку «сколько всего, сколько прошло, сколько упало, что не
исполнялось».

Сводчик считает физические строки `UnitTestResult`, а не шапку `Counters`.
Это важно для WSL-прогонов: в некоторых TRX шапка сообщает ноль
`notExecuted`, хотя сами строки результата честно помечены как `NotExecuted`.
Также сводчик не склеивает одинаковые имена тестов из разных сборок: если две
сборки дали две физические строки с одним `testName`, это две строки результата
до отдельного архитектурного решения о дедупликации. Таймаут маленькой порции
без читаемого TRX попадает в `notrun-report.md` отдельной строкой с количеством,
а не превращается в зелёный батч.

Быстрая самопроверка сводчика:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\summarize-test-batches.ps1 -SelfCheck
```

#### Дополнение 12.11.2026: происхождение чисел и отказ по недоказанной полноте

Это дополнение к разделу выше; прежние формулировки остаются в силе.

Сводчик называет происхождение каждого числа. Прежде число, взятое из журнала
при отсутствующем `result.trx`, складывалось с числами, прочитанными из
настоящих файлов результатов, и после сложения их было не различить — журнал без
сырого файла превращался в «тесты пройдены полностью». Теперь в `summary.md`
есть строка `Происхождение чисел: TRX | JOURNAL_ONLY | MIXED | NONE` (последнее —
«в итоге нет ни одного числа»; это обычный исход у испорченной подтверждённо
пустой группы, чей заявленный общий счёт в итог не импортируется), два счётчика
(`заявлено журналом без сырого файла результатов` и `заявлено ИСПОЛНЕННЫМИ без
сырого подтверждения (NOT_PROVEN_RAW)`), а в таблице батчей — колонки
`Происхождение` и `Не доказано сырым`. Строка `Подтверждённой полнотой этот
итог НЕ является...` печатается только при ненулевом счёте непроверенных.

Батч с объявленным и отсутствующим `result.trx` больше не считается прочитанным
и переводит прогон в незакрытое состояние; счётчик поэтому переименован в
`Батчей с прочитанным и подтверждённым результатом`. Платформенный пропуск и
подтверждённо пустая группа при честной строке журнала заявляют ноль исполненных
проверок и этим гейтом не затронуты.

Все четыре ветви с журнальными числами читают заявленных исполненными из
журнальной строки. Поэтому строка пропуска или пустой группы, заявляющая
исполненные проверки, называется предупреждением с именем батча и числом, не
попадает в подтверждённые и даёт код возврата `66`. У подтверждённо пустой
группы заявленный общий счёт в итог намеренно не импортируется — ловит ложь счёт
заявленных исполненными, а не арифметика, поэтому у строки `Заявлено
ИСПОЛНЕННЫМИ без сырого подтверждения` нет приставки «из них».

Код возврата `66` означает «в итоге есть проверки, заявленные исполненными без
сырого файла результатов». Он не снимается флагом `-AllowIncomplete` и отличается
от кода `65` (остались незакрытые батчи), чтобы вызывающий различал «каталог не
дочитан» и «числа не доказаны». Внешний вывод дополнен строкой
`provenance=... journalOnly=... notProvenRaw=...`.

**Уточнение к смыслу `-AllowIncomplete` (см. выше).** Этот флаг по-прежнему
означает только «читаю честно незавершённый каталог результатов» и по-прежнему
не отмывает старый зелёный результат ниже минимума. Дополнительно теперь прямо
сказано: он не разрешает принять числа **без доказательства**. Недоказанная
полнота — отдельная беда со своим кодом возврата `66`, и флаг её не подавляет.

Самопроверка создаёт временный синтетический каталог под игнорируемым
`TestResults/`, проверяет ловушки «ошибочная шапка TRX» и «одинаковое имя теста
в двух сборках», затем удаляет только свой точный каталог. Она не является
полным прогоном проекта.

Самопроверка нижнего порога запускается из WSL без сборки:

```bash
bash tools/test-batches/run-test-batches.sh \
  --no-build \
  --self-check-min-rows \
  --run-root "TestResults/min-rows-self-check-$(date -u +%Y%m%dT%H%M%SZ)"
```

Красный батч считается закрытым результатом: он дал файл результата и точные
числа падений. Незакрытым считается только батч без доказательства: например,
оборванный по пределу времени без `result.trx` или запуск, который оставил
только состояние `STARTED`. Это различие важно для обучения: красный результат
показывает, что чинить дальше, а незакрытый результат не даёт права двигать
замок полной проверки.

Каждая команда прогона несёт три уровня защиты. `blame-hang.runsettings` с
пределом десять минут нужен, чтобы назвать зависший тест по имени. Внутренний
`TestSessionTimeout` в том же runsettings ограничивает саму сессию VSTest.
Внешний `timeout --kill-after=60s` ограничивает процесс-хозяин, если зависает
не тест, а сам запускатель. Такой прямой маршрут выбран потому, что `dotnet
test --no-build` в WSL может завершиться с кодом `0` и не оставить `TRX`, то
есть дать недействительный результат. Папка результата всегда находится под
`TestResults/`, потому что она исключена из контроля версий; старый путь
`evidence/run/` также исключён и не используется для новых прогонов.

Превышение времени хранится как счётчик батча в `timeout-count.txt`. Первый
timeout даёт состояние `RETRY_REQUIRED_TIMEOUT`: это означает “повторить тот же
батч”, без автоматического деления. Второй timeout при
`split_on_repeat_timeout=true` даёт состояние `SPLIT_REQUIRED` и файл
`split-plan.tsv`. Если фильтр можно развести автоматически, в плане будут две
дочерние строки `A` и `B` с непересекающимися положительными терминами и
явными исключениями. Если фильтр не содержит минимум двух положительных
терминов, runner пишет честную строку `manual` вместо заявления об
автоматическом делении. Verifier не принимает слова плана на веру: он заново
перечисляет текущие тестовые классы исходного батча и обеих дочерних строк,
печатает отдельно число положительных терминов и число реально найденных
классов, затем требует нулевое пересечение, нулевые потери и нулевые лишние
классы. Поэтому запись «4 термина» никогда не выдается за «4 класса» без
фактического перечисления исходников.

Пример запуска двух дешёвых батчей из Windows:

```powershell
$runRoot = "C:\Projects\llm_mvp_core\TestResults\batches-smoke\run-manual"
tools\test-batches\run-test-batches.ps1 -Only "extension-token-quality,platform-capability-oracles" -RunRoot $runRoot
```

Пример запуска из WSL:

```bash
tools/test-batches/run-test-batches.sh --only extension-token-quality,platform-capability-oracles --run-root TestResults/batches-smoke/run-manual
```

Повтор той же команды с тем же `RunRoot` — это не новый прогон, а
возобновление. Запускалка читает `state.txt` каждого батча и пропускает только
закрытые батчи. Закрытый зелёный или красный батч обязан иметь непустой TRX из
текущей попытки, а не просто старый файл в корне. Если процесса уже нет, но
текущая попытка не дала файла результата или дала пустой/непарсимый файл,
состояние становится `INVALID_NO_TRX` и такой батч выполняется заново при
следующем запуске. Поэтому “процесс вернул ноль” больше не означает “батч
готов”: готовность доказывает только непустой и разобранный файл результата.

Большой батч теперь дополнительно режется на последовательные leaf'ы. Батч
остаётся человеческой единицей плана (`design-conversion-local-host`,
`core-m-z-and-remainder` и так далее), а leaf — техническая порция внутри него,
которая сохраняет прогресс чаще. Режим по умолчанию — `pack`: runner свежим
`dotnet vstest /ListTests` перечисляет методы текущей DLL под effective-filter и
группирует cases одной theory в один method FQN. Неделимый unit по умолчанию —
`class:<FQN>`: сам по себе `[Collection(..., DisableParallelization = true)]`
является scheduling-меткой, а не доказательством общего состояния, потому что
leaf'ы всё равно выполняются последовательно и без параллельных процессов. В
unit `collection:<name>` runner объединяет классы только когда исходный
`CollectionDefinition` реально привязан к `ICollectionFixture<...>` или когда
существующий manifest `tools/test-batches/leaf-shared-collections.tsv` явно
помечает коллекцию как shared-state. Если в исходниках видна fixture-привязка,
но имя `CollectionDefinition` нельзя разрешить, планирование останавливается
fail-closed: лучше не запустить leaf, чем незаметно разрезать общий fixture.
Цель 35–40 физических строк и предел 60 строк применяются только между такими
целыми units: runner не имеет права разрезать один class или настоящий shared
collection ради красивого размера. Если один неделимый unit больше предела, он
попадает в отдельный oversized leaf с причиной в `leaf-oversized-units.tsv`.
Диагностический режим `method-diagnostic` делает один leaf на метод и нужен
только явно, когда operator сознательно разбирает повторно зависший oversized
pack; `off` возвращает старое поведение “один батч — один VSTest”.

Класс `DesignConversionLiveLocalServerTests` — единственное каноническое
исключение из обычной упаковки локального хоста: он измеренно слишком велик
для одной порции, поэтому живёт в группе
`design-conversion-live-local-server-method-diagnostic` и запускается по одному
методу. Это не расширяет `method-diagnostic` на все классы
`design-conversion-local-host`: остальные серверные классы остаются в обычной
упаковке. Если у этого класса или его xUnit-коллекции появится настоящий общий
ресурс тестов (`IClassFixture<...>`, `ICollectionFixture<...>` или запись в
`leaf-shared-collections.tsv`), планирование новой группы должно остановиться
fail-closed, а не молча разрезать общий ресурс. Для этого одного маршрута
source-map не пытается быть полноценным C#-компилятором: любой список базовых
классов или интерфейсов у точного класса, общий объект текущей коллекции,
полученный через наследование или псевдоним типа, такой же список в любой
partial-части definition-класса этой коллекции, другая коллекция или
отсутствующее описание текущей коллекции считаются небезопасными для
автоматического запуска по методам.

Класс тестов хранилища DPO (`DesignConversionDpoStorageTests`) остаётся в своей
существующей группе (`design-conversion-dpo-storage`), но внутри этой группы он
делится на небольшие последовательные порции по методам (`class-method-pack`).
Остальные классы той же группы не делятся по методам: они по-прежнему идут
целыми классовыми или коллекционными единицами. В историческом прогоне
2026-08-29 этот класс дал 430 физических проверок и занял 443.68 секунды в
одной порции; один метод подтверждения многих кандидатов для обучения
(`LocalArtifactDb_BatchApprovalApprovesManyRuntimeCandidatesWithOneAuthorityRewrite`)
занял 252.166 секунды. Поэтому сценарий запуска выносит именно этот метод в отдельную
порцию, а остальные методы складывает небольшими группами. Эти числа — только
историческая справка: текущее число методов и физических проверок всегда берём
из свежих файлов `leaf-discovery.tsv`, `leaf-methods.tsv` и
`leaf-pack-report.tsv`. При неполной карте исходников или появлении общих
ресурсов, атрибутов класса либо наследования разделение запрещено; это
проверяется во всех частях класса. Одноимённый класс из другого пространства
имён не считается доказательством, что можно делить этот глобальный класс.

Leaf-фильтры всегда точные: `FullyQualifiedName=...`, объединённые через `|`.
Substring-фильтры внутри leaf запрещены, чтобы метод `Foo.Bar` не забрал
`Foo.BarExtended`. Закрытый leaf дополнительно сверяет TRX membership: verifier и
runner читают физические `UnitTestResult`, связывают `testId` с
`UnitTest/TestMethod className+name` и проверяют, что каждая строка принадлежит
точному method set leaf'а, без чужих/пропавших/лишних rows. План leaf'ов привязан
к fingerprint: текущий `HEAD`, SHA-256 тестовой DLL, SHA-256 runner'а и verifier'а,
SHA-256 `batches.tsv`, selected effective-filter, операционная система, версия и
детали .NET/runtime/testhost, leaf timeout/session/hang/max-limit настройки,
SHA-256 фактического runsettings XML, SHA-256 discovery seed и SHA-256
`leaf-source-collections.tsv` плюс SHA-256 `leaf-source-exact-bindings.tsv`.
При несовпадении fingerprint runner
останавливается fail-closed: старый leaf-plan нельзя возобновить на другой DLL,
другом runner'е/verifier'е, другой source-map class/collection или с другими
runtime настройками.

Отмена публикации результата обучения проверяется отдельным коротким набором
Linux-тестов: контрольные файлы весов, созданные текущим запуском, должны быть
удалены, а чужой файл с теми же байтами под тем же именем должен остаться. Эта
проверка не заменяет полную проверку хранилища и не означает исправность всего
проекта.

Evidence внутри leaf-run root:

```text
verification-scope.tsv       ← CANONICAL/FOCUSED область и запрет full-suite claim для focused
batch-01-design-conversion-live-local-server-method-diagnostic/
    leaf-source-collections.tsv ← source-карта class/collection/shared fixture
    leaf-source-exact-bindings.tsv ← точная связь класса с файлами исходников для узких исключений
    leaf-discovery.tsv          ← текущие method FQN и оценка физических строк
    leaf-fingerprint.tsv        ← привязка плана к HEAD/DLL/runner/batches.tsv
    leaf-plan.tsv               ← стабильные leaf packs и exact filters
    leaf-methods.tsv            ← каждый discovered method ровно в одном leaf + class/unit
    leaf-pack-report.tsv        ← class/unit/shared-collection split count и pack summary
    leaf-oversized-units.tsv    ← неделимые units больше hard cap
    leaf-runsettings.xml        ← runsettings XML, чей SHA входит в fingerprint
    leaf-testhost-files.tsv     ← testhost/TestPlatform файлы, чей SHA входит в fingerprint
    leaf-journal.tsv            ← строка на каждую leaf-попытку
    leaf-001/
        current-attempt.txt
        attempt-0001/
            result.trx
            state.txt
            exit.txt
            stdout.log
            stderr.log
```

Повтор команды с тем же `RunRoot` пропускает только закрытые leaf'ы
`COMPLETED_GREEN` и `COMPLETED_RED`; leaf со `STARTED`, `RUNNING`,
`LEAF_TIMED_OUT` или недействительным TRX выполняется заново, не перетирая
доказательство соседей. Красный leaf не останавливает остальные leaf'ы — это
продуктовый сигнал, а не инфраструктурный обрыв. Если остановили запуск по
`MaxLeaves` или `MaxElapsedMinutes`, корень получает состояние `INCOMPLETE`,
а не `FINISHED`; такой partial run verifier принимает только с явным
`-AllowIncomplete`, и он никогда не считается зелёным или красным полным
прогоном.

Родительский каталог группы из небольших порций (`leaf-pack`) не пропускается
как готовая группа (`batch`) до входа в `run_leaf_batch`: иначе старый план мог
бы миновать сверку отпечатка (`fingerprint`) текущего кода, DLL, фильтра и
настроек. Повтор внутри `run_leaf_batch` пропускает только уже закрытые порции
(`leaf`), а итоговый минимум всё равно пересчитывает по их TRX.

Возобновление делается той же командой и тем же `RunRoot`. `journal.tsv`
остаётся append-only журналом событий: строка `LEAF_INCOMPLETE` от первого
прохода не удаляется, а последующая terminal-строка того же batch становится
текущей проекцией. Runner summary и verifier печатают projected counts по
последней строке каждого batch в порядке manifest, а raw event count выводят
отдельно как `run_root_raw_journal_rows`. Если resume дошёл до `FINISHED`,
старый `incomplete-reason.txt` удаляется атомарно, чтобы вчерашняя причина
обрыва не читалась как текущий статус.

Хороший пример: один batch сначала записал `LEAF_INCOMPLETE`, затем при повторе
той же команды закрылся `COMPLETED_GREEN`; projected rows = 1, raw journal rows
= 2, итог берёт terminal counts. Плохой пример: две разные terminal-строки
одного batch с несовпадающими counts/result path; verifier обязан считать это
конфликтом evidence, а не молча выбирать последнюю строку.

На текущих локальных группах конверсии дизайна exact числа всегда надо брать из
свежего plan-only, потому что они зависят от текущей DLL. После выделения
медленного серверного класса первая группа
`design-conversion-live-local-server-method-diagnostic` содержит один класс
`DesignConversionLiveLocalServerTests`: свежий план показал 46 методов, 46
последовательных порций и 60 оценочных строк, причём каждая порция содержит
ровно один метод. Старая группа `design-conversion-local-host` остаётся обычной
упакованной группой для остальных классов локального хоста: свежий план показал
214 методов, 7 порций и 321 оценочную строку. Эти числа не являются контрактом
для будущей DLL: после изменений тестов нужно снова запускать plan-only и
читать свежие `leaf-pack-report.tsv` и `leaf-oversized-units.tsv`.

Пример plan-only инвентаризации медленного серверного класса:

```powershell
$runRoot = ".\TestResults\leaf-plan-design-conversion-live-local-server"
tools\test-batches\run-test-batches.ps1 -Only "design-conversion-live-local-server-method-diagnostic" -RunRoot $runRoot -PlanOnly
```

Пример короткого bounded запуска одной серверной порции:

```powershell
tools\test-batches\run-test-batches.ps1 -Only "design-conversion-live-local-server-method-diagnostic" -RunRoot $runRoot -MaxLeaves 1 -MaxElapsedMinutes 25
tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Для старой группы локального хоста пример остаётся обычным `pack`-планом:

```powershell
$runRoot = ".\TestResults\leaf-plan-design-conversion-local-host"
tools\test-batches\run-test-batches.ps1 -Only "design-conversion-local-host" -RunRoot $runRoot -PlanOnly -LeafMode pack
```

Короткий ограниченный запуск двух первых маленьких порций группы хранилища:

```powershell
$runRoot = ".\TestResults\batches-design-conversion-dpo-storage-method-pack"
tools\test-batches\run-test-batches.ps1 -Only "design-conversion-dpo-storage" -RunRoot $runRoot -NoBuild -MaxLeaves 2
tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Если после свежей проверки плана нужна не первая, а конкретная маленькая
группа, используйте команду Linux ниже и сохраните тот же каталог результатов
для проверки:

```bash
bash tools/test-batches/run-test-batches.sh \
  --only design-conversion-dpo-storage \
  --run-root TestResults/batches-design-conversion-dpo-storage-method-pack \
  --no-build \
  --leaf-id leaf-005 \
  --max-leaves 1
```

Такой запуск проверяет только уже выбранные порции и их журналы. Он не
измеряет общую длительность всей группы хранилища, не запускает весь
исторический набор из 430 физических строк и не является полной проверкой
проекта. Все варианты параметризованного теста выполняются вместе; если их
больше лимита, создаётся отдельная большая порция с записью в
`leaf-oversized-units.tsv`, а сам метод не режется пополам.

При каждом запуске создаётся `plan.tsv`. Это сухой план без догадок: для всех
строк лестницы показан исходный фильтр и фактический `effective_filter`,
который уйдёт в VSTest. Даже если через `--only` выбран поздний остаточный
батч, ранние строки той же DLL всё равно добавляют свои исключения. Поэтому
остаточный батч основной сборки не превращается обратно в wildcard `*`, а
забирает только ещё не назначенные классы.

Лестница батчей теперь проверяется отдельной командой:

```powershell
tools\test-batches\verify-test-batches.ps1
```

Она печатает человеческие счётчики: сколько публичных тестовых классов осталось
без батча, сколько классов получили дублирующее назначение, сколько
неделимых серийных групп разорвано и учтена ли отдельная Windows-сборка
настольной панели. Дополнительно verifier проверяет, что поздний остаточный
батч имеет реальные исключения, а не `*`, и выполняет маленькую самопроверку
attempt-логики: старый корневой TRX плюс новая незавершённая попытка должны
считаться проблемой, а не готовым батчем. Вторая самопроверка создает
синтаксически правильный XML с одной `Passed`-строкой и ложным `executed=0`:
и runner, и verifier обязаны отвергнуть его как противоречивый результат.

Чтобы эта проверка не тратила время на один и тот же обход списка групп для
каждого точного правила, проверяющий скрипт один раз строит карту «класс и
сборка → первая группа» для текущего основного списка. Карта живёт только внутри
одного запуска. Если правило вызывается с изменёнными строками, без нужного ключа
или с произвольным списком, проверяющий скрипт возвращается к свежему расчёту
назначения по первому совпадению; поэтому потерянный класс не прячется, а чужую
сборку, похожее имя класса и такое же короткое имя в другом пространстве имён
нельзя ошибочно включить в целевую группу.

Тот же проверяющий скрипт заранее разбирает фильтры строк списка групп на
положительные и отрицательные условия и повторно использует их в главном цикле.
Это ускорение действует только внутри одного запуска и только при точном
совпадении исходной строки фильтра; изменённая строка, включая изменение
регистра имени оператора, проверяется свежим старым путём. Непонятная ветка
фильтра остаётся ленивой: совпавшая ранняя ветка проходит, а достижимая
неподдержанная ветка даёт ошибку, как до ускорения.

Короткая самопроверка этого ускорения не запускает тесты .NET:

```powershell
pwsh -NoProfile -File .\tools\test-batches\verify-test-batches.ps1 -SelfCheckPreparedFilterParsing
```

Быстрая самопроверка этого маршрута запускается без сборки:

```powershell
pwsh -NoProfile -File .\tools\test-batches\verify-test-batches.ps1 -SelfCheckExactHumanBatchAssignmentMap
```

Любое нарушение возвращает ненулевой код выхода. Если нужно проверить уже
созданную папку прогона, передайте её явно:

```powershell
tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot
```

Для короткой приёмки checkpoint-а есть отдельный focused-режим. Он не заменяет
каноническую лестницу из текущего `batches.tsv`: focused допускает только custom manifest с
одной или несколькими строками и только точные method FQN вида
`FullyQualifiedName=Class.Method`, соединённые через `|`. Substring
`FullyQualifiedName~`, wildcard `*`, отрицания, пустые термы и повторы методов
отклоняются fail-closed. Runner пишет `verification-scope.tsv`, а leaf
fingerprint несёт `verification_scope=FOCUSED` и
`full_suite_claim_allowed=false`; verifier в этом режиме проверяет evidence
самих выбранных методов, но не делает global all-class assignment и никогда не
печатает full-suite green.
Дополнительная проверка также привязана к текущему списку (`focused manifest`): набор `batch_id`, SHA256
файла списка, `selected_filter` в `leaf-fingerprint.tsv` и объединение
`leaf-methods.tsv` должны ровно совпасть с exact-методами текущего
`BatchesFile`. Если после прогона заменить manifest на другой валидный
exact-список, verifier обязан завершиться красно, даже когда старый TRX сам по
себе парсится.

Хороший пример focused-проверки:

```powershell
$manifest = "$runRoot\public-path-sanitizer-batches.tsv"
tools\test-batches\run-test-batches.ps1 -BatchesFile $manifest -Only "public-path-sanitizer-checkpoint" -RunRoot $runRoot -LeafMode pack -Focused
tools\test-batches\verify-test-batches.ps1 -BatchesFile $manifest -RunRoot $runRoot -Focused
```

Плохие примеры, которые должны завершаться с ошибкой:

```powershell
tools\test-batches\run-test-batches.ps1 -BatchesFile $manifest -RunRoot $runRoot
tools\test-batches\verify-test-batches.ps1 -BatchesFile $manifest -RunRoot $runRoot
tools\test-batches\verify-test-batches.ps1 -Focused
```

Первый и второй примеры пытаются выдать exact-method focused manifest за
обычный canonical-прогон; третий включает focused на дефолтной canonical
лестнице. Старые корни без `verification-scope.tsv` остаются читаемыми:
обычная 18-строчная лестница получает метку `CANONICAL_LEGACY`, а старый
focused-корень принимается только с явными `-Focused` и custom manifest как
`FOCUSED_LEGACY`.

Для `RunRoot` verifier строгий: корневое состояние должно быть `FINISHED`,
`journal.tsv` не должен быть пустым, каждая папка `batch-*` должна иметь строку
журнала, а текущий статус берётся из последней строки batch в порядке manifest.
Состояния `RUNNING`/`STARTED`/неизвестные запрещены, закрытые красные и зелёные
батчи обязаны иметь парсимый TRX с фактическими строками результата, а числа в
текущей строке журнала должны совпадать с TRX. Платформенный пропуск принимается
только как явный `SKIPPED_PLATFORM` с причиной, числом неисполненных строк и
без фиктивного зелёного результата; focused verifier в этом единственном случае
не требует leaf/TRX, а сверяет `total` с текущим manifest `min_rows`; raw строки
журнала остаются видимыми отдельным счётчиком.

Отдельная сборка настольной панели наблюдения (`Tests.ApiDashboardWpf`) целится
в Windows и содержит девять проверок. В Linux/WSL она не удаляется из плана:
батч `api-dashboard-wpf-windows` получает состояние `SKIPPED_PLATFORM`, пишет
причину в свою папку и учитывает девять строк как не исполнявшиеся. Это честная
граница среды, а не зелёный результат и не пропажа батча.

Эта Windows-группа теперь имеет точный class-boundary фильтр
`FullyQualifiedName~ApiDashboardWindowInteractiveTests.` вместо wildcard. В
исходниках сейчас есть ровно один public test-класс этой сборки,
`ApiDashboardWindowInteractiveTests`, и девять `[Fact]`-методов; поэтому
`min_rows=9` остаётся прежним. Завершающая точка делает фильтр границей класса:
он покрывает `ApiDashboardWindowInteractiveTests` и методы вида
`ApiDashboardWindowInteractiveTests.SomeFact`, но не захватывает будущий
`ApiDashboardWindowInteractiveTestsLookalike`. Такой lookalike-класс должен
получить отдельный человеческий маршрут, а не попасть в WPF batch случайно.
После этой правки canonical TSV содержал 24 группы: 23 с явным
`FullyQualifiedName~...` сигналом и один wildcard-остаток
`core-m-z-and-remainder`. Active weak snapshot сжался до восьми строк; следующий
server-owned replay/passport срез сжимает остаток ещё раз.
Это source-only доказательство карты, а не доказательство Windows/WPF runtime:
окно и automation по-прежнему требуют отдельного Windows-запуска.

Группа `server-owned-replay-manifest-attestation` отделяет один класс
`LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTests`.
В исходнике сейчас три тестовых метода и пять физических xUnit-строк: теория
конфигурации HMAC даёт три строки, а два факта проверяют точную replay
attestation и passport исполняемого manifest. В `batches.tsv` используется
class-boundary фильтр
`FullyQualifiedName~LlmFromScratch.Tests.ServerOwnedLogicalCapabilityReplayVerificationCompositionTests.`:
точка после имени класса нужна для runtime `Class.Method`, но она не захватывает
будущий `ServerOwnedLogicalCapabilityReplayVerificationCompositionTestsLookalike`.
После этого сжатия canonical TSV содержал 25 групп: 24 строки с явным
`FullyQualifiedName~...` сигналом и один wildcard-остаток
`core-m-z-and-remainder`; следующий Phase 08 lifecycle срез сжимает остаток
ещё раз.

Группа `phase08-common-user-two-leg-lifecycle` отделяет один класс
`LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTests`. В исходнике сейчас
четыре `[Fact]`-метода; `min_rows=3` остаётся нижней границей для targeted-прогона.
TSV использует class-boundary фильтр
`FullyQualifiedName~LlmFromScratch.Tests.Phase08CommonUserTwoLegLifecycleTests.`:
точка нужна, чтобы VSTest совпал с `Class.Method`, но будущий
`Phase08CommonUserTwoLegLifecycleTestsLookalike` не попал в эту короткую
группу. После предыдущего Phase 08-сжатия исторически было 26 групп и шесть
слабых строк. После singleton-среза исторически стало 27 групп: 26
именованных и одна остаточная. Срез структуры потоков и иерархии исправления
результата исторически довёл лестницу до 28 групп: 27 именованных и одна
остаточная. Три группы записи пар, выполнения графа и защищённого каталога
сохраняемых моделей довели лестницу до 31 группы: 30 именованных и одной
остаточной. Следующее выделение медленного локального сервера довело текущий
список до 32 групп: 31 именованной и одной остаточной, а перечень классов без
собственного точного маршрута — до пустого активного снимка.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-phase08-common-user-two-leg-lifecycle"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "phase08-common-user-two-leg-lifecycle" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Такой targeted-запуск доказывает только Phase 08 portable profile/share/
continue/rollback группу и не является full-suite green.

Группа `singleton-flow-node-identity` отделяет один класс
`LlmFromScratch.Tests.SingletonIdentityTests` из `UniversalFlowSingletonTests.cs`.
Четыре проверки подтверждают, что повторное создание схемы работы возвращает те
же объекты потоков и узлов и не добавляет повторные регистрации. Их отдельный
запуск не проверяет весь проект. Соседние классы
`ConcurrentExecutionIsolationTests` и `DeduplicationTests` из того же файла
остаются в своих группах. Фильтр
`FullyQualifiedName~LlmFromScratch.Tests.SingletonIdentityTests.` указан как
технический пример после человеческого описания: завершающая точка отделяет имя
нужного класса от похожих имён вроде `SingletonIdentityTestsLookalike`. На
момент singleton-среза было 27 групп: 26 именованных и одна остаточная; это
исторический счёт после следующего выделения. Срез структуры потоков и
иерархии исправления результата исторически довёл лестницу до 28 групп: 27
именованных и одна остаточная. Текущие три группы записи пар, выполнения
графа и защищённого каталога сохраняемых моделей довели лестницу до 31
группы: 30 именованных и одной остаточной. Следующее выделение медленного
локального сервера довело текущий список до 32 групп: 31 именованной и одной
остаточной. Перечень классов без собственного точного маршрута теперь пуст;
это не удаляет страховочную остаточную строку.
Исходный перечень классов без точного маршрута остаётся неизменным: добавлять
в него новые классы нельзя.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-singleton-flow-node-identity"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "singleton-flow-node-identity" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Такой отдельный запуск доказывает только общие объекты потоков и узлов и
отсутствие повторной регистрации; состояние всего проекта он не закрывает.

Группа `logical-topology-and-result-gap` отделяет два оставшихся лёгких класса
проверки логической структуры: `LlmFromScratch.Tests.TopologyMutationFlowTests`
и `ResultGapLogicalHierarchyTests`. Первый класс содержит шестнадцать `[Fact]`
о мутациях структуры потоков, второй — четыре `[Fact]` о связи узла, потока и
оркестратора при исправлении результата. Поэтому `min_rows=20` — это
двадцать физических проверок, а не два класса. TSV использует фильтр с
границей полного имени класса
`FullyQualifiedName~LlmFromScratch.Tests.TopologyMutationFlowTests.|FullyQualifiedName~ResultGapLogicalHierarchyTests.`:
завершающая точка нужна фильтру запуска VSTest, который видит методы как
`Class.Method`, и не захватывает будущие
`TopologyMutationFlowTestsLookalike` или `ResultGapLogicalHierarchyTestsLookalike`.
На момент этого сжатия исторически было 28 групп: 27 именованных и одна
остаточная. Перечень классов без собственного точного маршрута содержал три
старые строки; неизменяемый исходный перечень слабых классов не меняется.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-logical-topology-and-result-gap"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "logical-topology-and-result-gap" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Этот отдельный запуск подтверждает только структуру потоков и иерархию
исправления результата; он не подтверждает прохождение всех тестов проекта.

Группа `result-gap-training-pair-writer` отделяет класс
`ResultGapPhase07DpoWriterTests`, который записывает обезличенные обучающие
пары и строки исправлений. В классе шесть обычных тестов, поэтому `min_rows=6`
означает шесть выполнений, а не качество обучения. Фильтр
`FullyQualifiedName~ResultGapPhase07DpoWriterTests.` заканчивается точкой:
она удерживает запуск на полном имени класса и не захватывает похожее имя.
В TSV эта точная строка стоит ранним порядком 11, сразу после
`design-conversion-remainder-n-z`, то есть до широких `dynamic-graph`,
`teaching-translation-dpo` и `orchestration-skills-wiki`: запускалка проверяет
группы по первому совпадению и не успевает забрать её в широкий маршрут по
слову из имени метода.

Короткий запуск записи пар:

```powershell
$runRoot = ".\TestResults\batches-result-gap-training-pair-writer"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "result-gap-training-pair-writer" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Этот отдельный запуск подтверждает только маршрут записи обезличенных пар и
исправлений; он не запускает обучение модели и не доказывает качество обучения.

Группа `single-combination-graph-runner` отделяет класс
`SingleCombinationGraphCandidateRunnerTests`, который проверяет выполнение
одного варианта графа, отмену и превышение времени. Четырнадцать обычных
тестов и два теста с двумя наборами входных данных дают 18 выполнений, поэтому
`min_rows=18`. Фильтр
`FullyQualifiedName~SingleCombinationGraphCandidateRunnerTests.` тоже
заканчивается точкой и удерживает запуск на полном имени класса.
Эта точная строка стоит ранним порядком 12, до широких графовых, обучающих и
групп оркестрации. Поэтому все 18 строк выполнения тестов остаются в своей
группе,
включая метод
`PublicSurface_IsOneAsyncCandidateMethodWithoutMultiPlanOrchestrationOrTrainingAuthority`,
где слово `Orchestration` находится только в имени метода, а не в имени класса.

Короткий запуск выполнения графа:

```powershell
$runRoot = ".\TestResults\batches-single-combination-graph-runner"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "single-combination-graph-runner" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Группа `retained-domain-model-registry` отделяет класс
`RetainedDomainModelRegistryTests`, который проверяет защищённый каталог
сохраняемых моделей. В Linux короткий запуск даёт 42 строки результата: три
переносимые строки выполняются, а 39 Windows-only строк честно учитываются как
NotExecuted. Это не зелёная подмена и не пропуск всей группы. Фильтр
`FullyQualifiedName~RetainedDomainModelRegistryTests.` заканчивается точкой,
поэтому будущий `RetainedDomainModelRegistryTestsLookalike` не расширит группу
молча.

Короткий запуск защищённого каталога вместе с остатком:

```powershell
$runRoot = ".\TestResults\batches-retained-domain-model-registry"
pwsh -NoProfile -File .\tools\test-batches\run-test-batches.ps1 -Only "retained-domain-model-registry,core-m-z-and-remainder" -RunRoot $runRoot -LeafMode pack -NoBuild
pwsh -NoProfile -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

После выделения медленного локального сервера всего 32 физические runner-группы:
31 именованная и одна остаточная. Поверх них в самом
`tools/test-batches/batches.tsv` хранится проверяемая logical ladder версии
`test-suite-batch-ladder-v1`: семь правил prompt 249, шестнадцать плановых
логических батчей и mapping каждой physical row ровно в один logical batch.
Это один carrier, а не второй список запуска. Runner продолжает выполнять
физические строки, а `TestSuiteBatchInventoryGuardTests` и
`verify-test-batches.ps1` падают, если logical metadata потеряла batch,
сослалась на неизвестную physical row, записала дубль или спрятала
Windows-only WPF сборку не в logical batch 16.

При расширении лестницы сначала выбирайте человеческий logical batch по семи
правилам в шапке `batches.tsv`, затем добавляйте или уточняйте physical row.
Если новый тест поднимает процесс, сервер, браузер или иной общий runtime
ресурс, обязанность автора — дать ему существующую или новую
группу-запрет параллельности; иначе first-match классификация идёт по
discovered assembly + class FQN. Теории и data rows влияют на runtime-вес и
TRX-счётчики, но не дают одному discovered class owner несколько batch owners.

Активный
перечень классов без собственного точного маршрута пуст; остаточная строка
остаётся страховкой для будущих новых классов и должна подтверждаться как
пустая отдельным состоянием, а не пустым запуском тестов.

`LanguageTranslateReferenceMatchingRuleTests` адресуется существующей строкой
`core-g-l` рядом с `LanguageTranslateHeldOutScorerTests` через точный
`FullyQualifiedName~LlmFromScratch.Tests.LanguageTranslateReferenceMatchingRuleTests.`, поэтому новый
класс не расширяет wildcard-остаток и не требует второго batch canon.
`ProjectWikiCodeDocumentationReferenceMatchingRuleTests`,
`ProjectWikiAngularCodeDocumentationReferenceMatchingRuleTests` и
`ProjectWikiSqlCodeDocumentationReferenceMatchingRuleTests` адресуются
существующей ProjectWiki-группой `orchestration-skills-wiki`, потому что их fully
qualified class names содержат `ProjectWiki`, а эта строка имеет фильтр
`FullyQualifiedName~ProjectWiki` до wildcard-остатка. Поэтому для этого среза
`tools/test-batches/batches.tsv` не меняется.

`OrchestrationReferenceMatchingRuleTests` проверяет только новый code-level
frozen-reference measurement adapter в
`26.TrainableRouter/Training/OrchestrationReferenceMatchingRule.cs`. Фокусные
тесты создают in-memory fake paid provenance для
`FrozenReferenceCorpus` fixture, но не создают production paid corpus, DPO rows,
teacher calls, model weights, checkpoints или training artifacts. Положительный
case доказывает, что prose descriptions не являются authority: совпадение
строится по exact bindings (`reference_case_id`, `scenario_id`,
`source_entity_fingerprint`), step-id coverage, ordered step sequence,
DAG/topologically-forward dependency edge set, phase-tag set и step-to-phase
assignment. Отрицательные cases мутируют role/envelope, bindings, order,
missing/extra steps, dependencies, phase tags, duplicate ids/orders/edges,
missing endpoints, self-edge, two-node cycle, longer cycle, backward edge,
unknown fields и arbitrary high-score property; отдельный raw JSON case
проверяет, что identical invalid cyclic candidate/reference всё равно даёт
fail-closed `false`. Это не тест `OrchestrationAcceptanceGateEvaluator`:
текущий acceptance gate остаётся отдельным self-referential механизмом и не
promoted в authority для frozen reference.

Фокусный запуск нового adapter:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore --nologo --filter "FullyQualifiedName~OrchestrationReferenceMatchingRuleTests"
```

`DpoPairReferencePreferenceMatchingRuleTests` проверяет только generic
детерминированный измеритель “chosen side closer to frozen reference than
rejected side” в
`14.ContinuousLearning/Core/DpoPairReferencePreferenceMatchingRule.cs`. Тесты
используют in-memory fake paid provenance и простой test-only subject predicate,
чтобы не дублировать Translation/DesignConversion/ProjectWiki/Orchestration
matchers и не вызывать teacher/model/training/DPO generation. Положительный
case требует `chosen=true` и `rejected=false`; mutation cases покрывают
chosen/rejected swap, both-match tie, neither-match, paid reference copied into
either side, `PaidExternalModel` authorship in either admission side,
prompt/owner-input mismatch, domain/subject/rule-identity mismatch, predicate
failure/cancellation, missing rich side authorship, admission author kind
mismatch at the same content fingerprint, deterministic result surface without
Score/Distance properties, and result-hash binding to the exact
`DpoTrainerPairAdmission.ContentFingerprintSha256` without storing raw
Prompt/Chosen/Rejected bytes or the paid answer.

Фокусный запуск нового preference adapter:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --no-restore --nologo --filter "FullyQualifiedName~DpoPairReferencePreferenceMatchingRuleTests"
```

Отдельный xUnit guard `TestSuiteBatchInventoryGuardTests` проверяет не TRX
последнего прогона, а саму карту тестовой лестницы. Он читает
`tools/test-batches/batches.tsv`, находит три живые тестовые сборки, включая
Windows-only `Tests.ApiDashboardWpf`, строит first-match назначения классов и
сравнивает слабую часть результата с
`Tests/TestSuiteBatchLegacySnapshot.txt`.
Runner отвечает за запуск строк и запись evidence, verifier отвечает за
валидность RunRoot/TRX, а inventory guard ловит drift до запуска: новую сборку
без строки манифеста, класс, случайно попавший в wildcard-остаток, разорванную
неделимую группу или ручной exact-method фильтр внутри canonical TSV.

Активный snapshot не refresh'ится автоматически и больше не является полной
описью текущих публичных тестовых классов. После выделения групп
оркестрации, маршрутизации, границ подключения продукта, тестовой
инфраструктуры, включения модели, WPF панели, server-owned replay/passport,
Phase 08 lifecycle, singleton-проверок общих потоков/узлов, структуры
потоков/иерархии исправления результата, записи пар, выполнения графа и
защищённого каталога сохраняемых моделей перечень классов без собственного
точного маршрута пуст.
Он хранит только старые слабые
классы, которые пока
попадают в wildcard-остаток без явного `FullyQualifiedName~...` правила.
Неизменяемый baseline этих старых слабых строк лежит в
`Tests/TestSuiteBatchLegacyWeakBaseline.txt`: активный snapshot может только
сжиматься, когда класс получает сильный маршрут. Новый слабый класс не
добавляется в snapshot; правильная починка — дать ему человеческий префикс в
`batches.tsv` или осознанно выбрать существующую группу. Тело baseline
дополнительно закреплено внешним SHA-256 якорем в `TestSuiteBatchManifest`,
поэтому одновременное добавление нового слабого класса в active snapshot и
baseline отклоняется даже с правильно пересчитанными header-хешами. Hash
считается по LF-канону тела и не зависит от CRLF/LF checkout. Тесты сами эти
файлы не переписывают. Для короткой проверки logical ladder есть focused manifest
`tools/test-batches/test-suite-batch-inventory-guard-focused.tsv`: он перечисляет
только три exact acceptance method FQN для logical-ladder guard. Старый широкий
inventory diagnostic pack запускается отдельно и не смешивается с acceptance
этого среза. Этот focused manifest не даёт права заявлять прохождение всех
тестов проекта.

Минимальная проверка logical ladder не запускает полный suite:

```powershell
dotnet build Tests\LlmFromScratch.Tests.csproj -c Release --nologo
$runRoot = Join-Path $PWD ("TestResults\test-batch-ladder-guard-" + (Get-Date -Format "yyyyMMddTHHmmssZ"))
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -BatchesFile .\tools\test-batches\test-suite-batch-inventory-guard-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused -NoBuild
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -BatchesFile .\tools\test-batches\test-suite-batch-inventory-guard-focused.tsv -RunRoot $runRoot -Focused
```

Для canonical discovery-only проверки полного inventory без выполнения тестов
запускайте только verifier на основном `batches.tsv`; он читает source/test
inventory, строит first-match classification и печатает счётчики missing,
duplicate и unmapped logical rows:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1
```

Focused run root — временное evidence под игнорируемым `TestResults/`. После
чтения `summary.md`, `journal.tsv`, focused `result.trx` и вывода verifier
удаляйте только точные дочерние папки, созданные этой проверкой
(`test-batch-ladder-*`). Не удаляйте весь `TestResults/` и не переносите эти
папки в tracked дерево.

Группа `training-entrypoints-and-admission` вынесена в canonical TSV отдельно от
wildcard-остатка. Она ловит шесть входных классов обучения:
`DpoTrainerPairAdmissionTests`, `LegacyDpoArtifactQuarantineManifestTests`,
`ModelConfigTests`, `PretrainingRunnerTests`, `SchedulerTests` и
`TrainerTests`. Это быстрый маршрут для ближайших правок обучения: падение в
этой группе показывает проблему допуска пары, карантина старых DPO-артефактов,
конфига модели, базового тренера, предобучения или планировщика, а не шум
большого остаточного batch.

Группа `model-core-math-tokenizer` вынесена в canonical TSV отдельно от
wildcard-остатка. Она ловит шестнадцать классов математики модели, тензоров,
sampling, tokenizer и transformer, чтобы поломки вычислительного ядра модели
проверялись отдельным понятным маршрутом, а не вместе с несвязанными остатками
по буквам M-Z.
В TSV эта группа перечислена полными именами классов: `MachineLearningTests`,
`MatrixAlgorithmsTests`, `MeanPoolingTests`, `NpyReaderTests`,
`NpyWriterTests`, `P08SamplerCompositionTests`,
`RotaryEmbeddingPositionOffsetTests`, `SamplerTests`,
`SimdMatMulTests`, `TensorCoreTests`, `TensorOpsCausalMaskWithPastTests`,
`TextGeneratorSeededSamplingTests`, `TokenizerEdgeCaseTests`, `TokenizerTests`,
`TransformerKnnTokenizerE15Tests` и `TransformerModelTests`. Это намеренно не
короткие слова `Tokenizer`, `Transformer`, `Tensor` или `Npy`: runtime-фильтр
VSTest смотрит на полное имя метода и мог бы случайно захватить метод чужого
класса.

Шестнадцатый класс этой группы — проверки последовательных отсечений при выборе
следующего знака (`P08SamplerCompositionTests` в папке `Tests/ExperimentFixes`).
Он попал сюда, потому что проверяет ту же математику выбора знака, что и
`SamplerTests`: сначала маска по числу кандидатов, затем отсечение по сумме
вероятностей уже по отобранным кандидатам. Явный полный префикс в TSV добавлен
сразу вместе с классом: новый класс без человеческого адреса провалил бы опись
групп тестов (`new_test_class_without_strong_batch_signal`), а добавлять такой
класс в активный снимок слабых классов запрещено правилами самого снимка.

Группа `semantic-compiler-search-vector` вынесена в canonical TSV отдельно от
wildcard-остатка. Она ловит десять классов семантической компиляции, поиска и
vector search: `OssEquivalentsTests`, `OssEquivalentsV4Tests`,
`PatternFusionTests`, `RoslynCodeAnalyzerTests`, `SearchEngineTests`,
`SemanticCompiler29Tests`, `SemanticCompilerTests`, `SemanticNlpTests`,
`VectorSearchEdgeCaseTests` и `VectorSearchTests`. Это быстрый маршрут для
правок поиска и семантического компилятора: он показывает проблему в этой
поверхности отдельно от общего `core-m-z-and-remainder`. В TSV используются
полные имена классов с namespace `LlmFromScratch.Tests`, чтобы не поймать
похожий метод чужого класса. Этот batch не равен полному зелёному suite:
после него всё равно нужен canonical summary по всем группам.

Группа `orchestration-routing-product-boundaries` вынесена в canonical TSV
отдельно от wildcard-остатка. Она ловит семь классов пути подключения продукта:
`MultiSubPhaseRouterLearnedAuthorityTests`, `OrchestratorModelTests`,
`PluginBoundaryTests`, `PromptRoutingInputCompilerUnitTests`,
`ReflectionWiringTests`, `SolutionProductProjectRegistrationTests` и
`ThinClientApiContractTests`. Эти проверки покрывают путь “prompt -> compiled
request -> route choice -> orchestrator/model -> reflection wiring ->
plugin/thin-client boundary” коротким отдельным сигналом. В TSV используются
полные class-level `FullyQualifiedName~...` термы; `ProjectTaskOrchestrationPipelineTests`
сюда не входит, потому что уже получает сильный маршрут в группе
`orchestration-skills-wiki` раньше по first-match лестнице.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-orchestration-routing-product-boundaries"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "orchestration-routing-product-boundaries" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Такой targeted-прогон полезен для быстрой диагностики routing/wiring/plugin
поверхности, но не даёт права писать “вся тестовая проверка зелёная”. Полный
статус дерева появляется только из сводки по всем тридцати группам.

Группа `test-infrastructure-evidence-measurement` отделяет пять классов, которые
следят за честностью самой тестовой машины: `CrossStackFlowSeedHonestyTests`,
`MeasurementNoisePolicyTests`, `TestOwnedOutputRootTests`,
`TestSuiteBatchInventoryGuardTests` и `WorkLeftRagEvidenceIndexTests`. Здесь
проверяются изоляция временных файлов, RAG-опись, политика шума измерений и
guard деления тестов. Они больше не прячутся в wildcard-остатке, поэтому
поломка batch-инструмента получает короткий адрес.

Короткий запуск новой группы:

```powershell
$runRoot = ".\TestResults\batches-test-infrastructure-evidence-measurement"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "test-infrastructure-evidence-measurement" -RunRoot $runRoot
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Этот targeted-прогон нужен для быстрой проверки инфраструктуры тестов и
измерений. Он не является full-suite green и не говорит о состоянии всех
проектных тестов.

Короткий запуск server-owned replay/passport группы:

```powershell
$runRoot = ".\TestResults\batches-server-owned-replay-manifest-attestation"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "server-owned-replay-manifest-attestation" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Этот targeted-прогон доказывает только серверную replay/passport группу; он не
является full-suite green и не закрывает оставшиеся классы wildcard-остатка.

Группа `model-activation` отделяет шесть классов отдельной сборки включения
модели конверсии дизайна:
`DesignConversionActivatedModelRepositoryHostRuntimeTests`,
`DesignConversionExternalVisualAiReceiptTrustTests`,
`DesignConversionHumanApprovalTests`,
`DesignConversionImprovementLoopSemanticsTests`,
`DesignConversionModelActivationRegistryTests` и
`DesignConversionModelActivationRouteRuntimeTests`. В `batches.tsv` эти классы
записаны class-boundary термами с точкой после имени класса, например
`FullyQualifiedName~DesignConversionHumanApprovalTests.`. Это не короткий
substring: runtime-фильтр VSTest совпадает с `Class.Method`, но не совпадает с
будущим `ClassLookalike.Method`. Новый похожий класс останется вне
`model-activation` и будет требовать отдельный человеческий маршрут, а не
молча менять время прогона.

Короткий запуск группы включения модели:

```powershell
$runRoot = ".\TestResults\batches-model-activation"
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\run-test-batches.ps1 -Only "model-activation" -RunRoot $runRoot -LeafMode pack
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\test-batches\verify-test-batches.ps1 -RunRoot $runRoot -AllowIncomplete
```

Этот targeted-прогон доказывает только отдельную сборку включения модели; он
не является full-suite green и не проверяет production-модули обучения.

### Refs-only включение готовой локальной DPO-модели

Команда `activate-local-promotion` проверяет безопасный путь включения уже
готовой локальной promotion-записи. Внешний caller не приносит
`DesignConversionPromotedModelActivationRecord`, `Publication`, `Candidate` или
filesystem paths. Он передаёт только tenant/auth/idempotency/expected generation
и selector из безопасных скаляров:

```json
{
  "action": "activate-local-promotion",
  "tenantId": "tenant-design-conversion-route-alpha",
  "requestedByRef": "operator:local-model-lifecycle",
  "reason": "activate already reviewed local promotion",
  "idempotencyKey": "activate-local:<stable-operation-id>",
  "expectedGeneration": 0,
  "localPromotionSelector": {
    "promotionUploadRef": "local://design-conversion/promoted/<manifest-id>",
    "promotedModelId": "<promoted-model-id>",
    "version": "<exact-model-version>",
    "checkpointSha256": "<64 lowercase hex sha256>"
  }
}
```

Смысл границы: private checkpoint/artifact paths остаются только в
server-owned durable local-promotion journal. API читает
`ListTrainingIntakeLocalPromotionRecordsAsync`, фильтрует ровно один ready
record того же tenant-а по точному selector-у, проверяет, что эта строка является
current, и только затем отдаёт полный внутренний activation record в существующий
`DesignConversionModelActivationRegistry.ActivateAsync`. `promote-local` от
этого не становится auto-activation.

Хороший пример: selector содержит `promotionUploadRef`, `promotedModelId`,
точную `version` и canonical `checkpointSha256`; ответ возвращает refs/hash
(`checkpointPathRef`) и `productReady=false`.

Плохой пример: caller присылает serialized `Candidate`, `Publication`, абсолютный
checkpoint path или выбирает только по `modelId/version` без promotion ref и SHA.
Такой запрос должен падать до mutation.

Фокусная проверка нового среза:

```powershell
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet test Tests/DesignConversion.ModelActivation.Tests/DesignConversion.ModelActivation.Tests.csproj -c Release --filter "FullyQualifiedName~ModelActivationRoute_ActivateLocalPromotion"'
```

Этот claim честно доказывает только refs-only activation route с тестовыми
durable local-promotion records. Он не доказывает полный `model-activation`
batch, DPO trainer, cloud upload/deploy, public HTTP hosting или production
готовность.

Живой файл `tools/test-batches/leaf-mode-overrides.tsv` сейчас задаёт три
точечных режима разбиения. Для
`design-conversion-live-local-server-method-diagnostic` это нужно из-за
измеренно слишком крупного серверного класса: каждый метод получает собственную
порцию, файл результата и точный `FullyQualifiedName=...` фильтр. Для
`test-infrastructure-evidence-measurement` это нужно потому, что
`TestSuiteBatchInventoryGuardTests` сам проверяет тяжёлые мутации manifest,
снимка и verifier-а: один зависший метод теперь оставляет долг только по своей
порции, а уже прошедшие RAG- и measurement-методы сохраняют отдельные журналы,
файлы результата и fingerprint. При этом `tools/test-batches/batches.tsv`
по-прежнему назначает целые классы, чтобы статическая опись не потеряла класс и
не дала дубль. Для `design-conversion-dpo-storage` режим `class-method-pack`
делит по методам только глобальный класс `DesignConversionDpoStorageTests` и
изолирует исторически медленный метод подтверждения кандидатов, но не даёт
этому режиму перейти на соседние классы или другие группы. При продолжении
прерванного прогона сверяются режим, файл настроек и соответствие классов
исходникам; результаты прежнего разбиения к новому плану не подходят.

Для guard-методов, которые внутри одного теста проверяют несколько мутаций
одного списка групп, снимок описи надо строить один раз и передавать во все
проверки. Повторный полный обход на каждую мутацию превращает один leaf в серию
тяжёлых сканирований: при зависании такой метод теряет возобновляемость и может
сорвать весь диагностический прогон.

Для короткой проверки можно добавлять `-MaxLeaves` или `-MaxElapsedMinutes`.
Это честный ограниченный набор: verifier с `-AllowIncomplete` сохранит уже
закрытые leaf-и и явно покажет, сколько осталось. Такой набор помогает
передаче проверки между заходами, но не заменяет полный прогон выбранной группы
и тем более не является зелёным полным прогоном всех тестов.

`-MaxLeaves` считает новые попытки выполнения порций (`leaf`) суммарно за один
вызов запускалки (`runner`), даже если выбранный список переходит через
несколько групп. Например при лимите 3 первый заход может выполнить две порции
первой группы и одну порцию следующей; третья группа не начнёт лишний тест до
следующего продолжения тем же `RunRoot`. Повтор той же команды с тем же
`RunRoot` получает новый бюджет, пропускает уже закрытые порции и продолжает с
первой открытой. `-MaxLeaves 0` означает отсутствие ограничения по количеству
порций.

В PowerShell-обёртке положительный `-MaxLeaves` совместим только с leaf-режимами
`pack` и `method-diagnostic`. Режим `-LeafMode off` не пишет `leaf-journal.tsv`,
поэтому обёртка отказывает такой комбинации до WSL; безлимитный
`-LeafMode off -MaxLeaves 0` остаётся обычным запуском.

`-MaxElapsedMinutes` проверяется перед запуском следующей порции. Он не обещает
жёстко остановить общее время команды ровно на этой минуте и не обрывает уже
стартовавший VSTest посередине; для отдельной попытки остаются свои таймауты.
Лимиты записываются в `verification-scope.tsv` как честный контекст текущего
вызова, но контрольный отпечаток плана (`fingerprint`) продолжает зависеть
только от смысла плана: override/mode/filter/source-map/test DLL/runsettings.
Поэтому смена бюджета разрешена, а смена override или фильтра по-прежнему
отказывается fail-closed.

При автоматическом запуске PowerShell-обёртки этот временной предел пока
отсчитывается отдельно в каждом дочернем процессе. Общее время всей команды
может быть больше; в данном исправлении общим стал только лимит количества
порций (`MaxLeaves`).

Быстрая самопроверка межгруппового счётчика использует синтетический `dotnet`,
который имитирует перечисление тестов (`ListTests`) и файлы результатов (`TRX`);
она не запускает продуктовые тесты:

```bash
bash tools/test-batches/run-test-batches.sh --no-build --self-check-invocation-budget --run-root "TestResults/invocation-budget-self-check-$(date -u +%Y%m%dT%H%M%SZ)"
```

Отдельная самопроверка PowerShell-обёртки не запускает продуктовые .NET тесты:
она выполняет управляемую подмену выбора групп и запуска Linux (`fake selector`,
`fake WSL`), проверяет общий бюджет между дополнительными и основными дочерними
процессами, стабильное продолжение дополнительной проверки, отказ от плохого
доказательства и то, что stdout дочернего процесса не становится кодом выхода:

```powershell
pwsh -NoProfile -File .\tools\test-batches\self-check-wrapper-budget.ps1 -EvidenceRoot .\TestResults\wrapper-budget-self-check-<date>
```

Для аудита resume рядом пишется `RunRoot/run-invocations.tsv`. Это не
fingerprint и не условие совместимости плана, а append-only история вызовов:
runner сначала проверяет существующую цепочку, затем дописывает ровно одну
новую строку. Первая строка хранит бюджет первого захода, вторая — бюджет
следующего, и так далее. Каждая строка содержит порядковый invocation-id,
UTC-время, выбранные группы, запрошенный и фактический leaf-режим, `MaxLeaves`,
`MaxElapsedMinutes`, hash всех предыдущих строк и hash самой строки. Verifier
проверяет эту цепочку и связь с `journal.tsv`, поэтому отсутствующий или
испорченный header, удаление раннего захода, нарушение последовательности,
дубликат id, текст вместо числового бюджета или подмена старой строки
становится отдельным красным evidence, а не тихой потерей истории.

В этой группе RAG-опись обязана работать и под Windows, и из WSL. Product-loader
для evidence-index переводит Windows-абсолютный путь вида `C:\...` в
`/mnt/c/...` до любого склеивания путей, поэтому отказ не маскируется
относительным Linux-хвостом. Тестовые источники WorkLeft сначала проверяются в
живом каталоге сессии, затем в `archive`, куда старые файлы были перенесены
после консолидации 25.08.2026; ожидаемые hash при этом не меняются.

## Фокусная проверка карантина старых DPO-артефактов

Этот срез проверяет, что старые трехполевые DPO-наборы и посевные каталоги
`dpo-runs` остаются только исторической базой холодного старта. Проверка не
обучает модель и не удаляет файлы: она сверяет manifest, два JSONL-реестра,
SHA-256 и число непустых строк по живому репозиторию.

Живой карантин:
`training-data/legacy-dpo-artifact-quarantine/2026-08-30/manifest.json`.
На 30.08.2026 он покрывает 61 файл
`experiments-v2/INTERNAL-TRAIN-READY-V191/results/**/dpo-pairs-normalized.jsonl`
с 37 647 строками и 439 файлов `dpo-runs/**/dpo-pairs.jsonl` с 43 550
строками; рядом учтены 7 held-out файлов с 350 строками. Все записи имеют
`BASELINE_ONLY_COLD_START_FOREST`, `training_allowed=false` и
`semantic_publication_allowed=false`.

Проверка скриптом:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\training-artifacts\verify-legacy-dpo-artifact-quarantine.ps1
```

Фокусный тест:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj -c Release --nologo --filter "FullyQualifiedName~LlmFromScratch.Tests.LegacyDpoArtifactQuarantineManifestTests"
```

Этот scope доказывает только карантин исторических DPO-артефактов. Он не
является полным прогоном, не запускает обучение и не разрешает смысловую
публикацию старых пар.

`DesignConversionLocalIndependentQwenRuntimeCanaryTests` остаётся в общей
inventory, но не в слабом snapshot: по first-match лестнице он получает сильный
сигнал `DesignConversionL` и попадает в DesignConversion-остаток от A до M. Это
членство не принимает реальные ответы Qwen и не меняет границу качества:
contract evidence остаётся отдельным слоем 24 из 24, а runtime canary без
Windows opt-in, модели и трёх production-пинов остаётся 0 из 4 принятых
реальных вызовов. В Linux/WSL это честный Skip/NotExecuted, не зелёный и не
повод удалить canary из inventory.

Focused GitHub Actions workflow пишет evidence так же явно, пусть и без
батчевой лестницы: каждый focused `dotnet test` получает собственную папку
`TestResults/focused-verification/<human-name>/` с `result.trx`, `output.log`,
`state.txt` и `exit.txt`. Шесть focused test шагов помечены
`continue-on-error: true`, чтобы красный тест не остановил сбор доказательств
для следующих тестов. Финальный `upload-artifact` запускается с `if: always()`
и прикладывает все TRX/log/state/exit файлы, которые успели появиться. Уже
после upload отдельный aggregation gate читает все шесть папок и делает job
красным, если хотя бы одна папка отсутствует, не содержит непустой TRX, имеет
не `COMPLETED_GREEN` или ненулевой exit. Если GitHub убил шаг по timeout,
`state.txt` останется `RUNNING`, и финальный gate честно покажет незакрытый
тест вместо молчаливой потери evidence.

## Переносимая проверка отложенной публикации производного индекса

Файл `conversion-results.jsonl` остаётся авторитетным журналом результатов
Design-to-Code. Файл `conversion-results.index.json` — производный ускоритель
чтения: его можно построить заново из журнала, поэтому сбой его публикации
после успешной записи JSONL возвращает честный успешный результат с признаками
«индекс отложен» и «межфайловая атомарность не заявлена». Старые байты индекса
при этом не меняются, временный `*.write.tmp` удаляется, а следующая запись
восстанавливает индекс по всему авторитетному журналу.

Не следует имитировать этот сбой удержанием destination-файла через
`FileShare`: Windows и Linux/WSL по-разному разрешают замену открытого файла.
Три отрицательных теста используют внутренний callback непосредственно перед
атомарным `File.Move` и выбрасывают `IOException` ровно один раз. Callback по
умолчанию равен `null`, не расширяет публичный API и не меняет production-
классификацию `IOException` или `UnauthorizedAccessException`.

Короткая проверка привязана к двум строкам manifest: три отрицательных сценария
и пять ближайших положительных сценариев роста, восстановления и отмены.

```powershell
$runRoot = Join-Path $PWD ("TestResults\design-conversion-derived-index-deferred-focused-" + (Get-Date -Format "yyyyMMddTHHmmssZ"))
tools\test-batches\run-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-derived-index-deferred-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused -NoBuild
tools\test-batches\verify-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-derived-index-deferred-focused.tsv -RunRoot $runRoot -Focused
```

Этот focused-прогон ожидает не менее восьми физических xUnit-строк без
пропусков. Он доказывает только границу авторитетного журнала и производного
индекса; право объявлять весь набор тестов зелёным остаётся `false`.

## Переносимый отказ записи авторитетного JSONL-журнала

Отказ самой записи `conversion-results.jsonl` отличается от отказа производного
индекса: операция возвращает типизированную безопасную ошибку, а исходные байты
журнала и индекса остаются неизменными. Удерживать журнал с `FileShare.Read` для
такой проверки нельзя: Windows запрещает конкурирующую запись, а Linux/WSL может
её разрешить. Тест использует уже существующий внутренний callback прямо перед
открытием authority-файла и выбрасывает `IOException` ровно один раз. Следующая
запись тем же экземпляром должна завершиться за ограниченное время, что
доказывает освобождение gate; свежий reader и физические строки подтверждают
единственную запись без дубля, а временных `*.tmp` файлов не остаётся.

Узкий manifest содержит изменённый отрицательный тест и четыре уникальных
положительных соседа: обычные первый/второй append, ожидание mutation lease,
идемпотентный повтор с восстановлением индекса и отмену без мутации.

```powershell
$runRoot = Join-Path $PWD ("TestResults\design-conversion-authority-jsonl-write-failure-focused-" + (Get-Date -Format "yyyyMMddTHHmmssZ"))
tools\test-batches\run-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-authority-jsonl-write-failure-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused -NoBuild
tools\test-batches\verify-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-authority-jsonl-write-failure-focused.tsv -RunRoot $runRoot -Focused
```

Ожидается не менее пяти физических xUnit-строк, без падений и пропусков. Этот
прогон доказывает только отказ и восстановление записи авторитетного журнала;
он не является полным прогоном тестов.

## Переносимый отказ публикации журналов обучения

Журналы заявок `training-intake-requests.jsonl` и запусков
`training-intake-runs.jsonl` публикуются общей статической операцией
copy-on-write. Внутренний тестовый callback вызывается после `FlushAsync` и
`Flush(flushToDisk: true)`, после проверки отмены, но непосредственно перед
`File.Replace` или `File.Move`. В production callback равен `null`; он получает
только вид журнала (`TrainingIntakeRequests` или `TrainingIntakeRuns`) и не
получает путь, JSON либо ключ идемпотентности. Поэтому шов не становится новым
каналом утечки данных и не меняет существующую классификацию `IOException` и
`UnauthorizedAccessException`.

Три отрицательных теста больше не полагаются на `FileShare`: такая блокировка
останавливает замену файла в Windows, но может не остановить её в Linux/WSL.
Одноразовый callback через `Interlocked` выбрасывает `IOException` ровно на
первой попытке публикации. Тесты проверяют старые байты журнала, отсутствие
`*.copyonwrite.tmp`, стабильную безопасную ошибку без пути и приватных маркеров,
освобождение gate и единственную запись после повтора тем же экземпляром.

Сначала сборка в разрешённой WSL-среде и точечная проверка трёх отрицательных
методов:

```powershell
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet build Tests/LlmFromScratch.Tests.csproj -c Release --nologo'
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName=DesignConversionLocalArtifactDbTrainingIntakeDurabilityTests.CreateTrainingIntakeAsync_InjectedPublicationFailureAfterTempFlush_FailsClosedPreservesBytesAndRetryAppendsOnce|FullyQualifiedName=DesignConversionLocalArtifactDbTrainingIntakeDurabilityTests.CreateTrainingIntakeAsync_WriteFailure_ReturnsStableSafeErrorWithoutPathTempExceptionTenantKeyOrPrivateMarker|FullyQualifiedName=DesignConversionLocalArtifactDbTrainingRunDurabilityTests.RunTrainingIntakeAsync_InjectedPublicationFailureAfterTempFlush_FailsSafelyPreservesBytesAndRetryAppendsOnce"'
```

Каноническая короткая проверка добавляет четыре положительных соседних
сценария и привязывает результат к исходникам, DLL и manifest:

```powershell
$runRoot = Join-Path $PWD ("TestResults\design-conversion-training-journal-publication-failure-focused-" + (Get-Date -Format "yyyyMMddTHHmmssZ"))
tools\test-batches\run-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-training-journal-publication-failure-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused -NoBuild
tools\test-batches\verify-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-training-journal-publication-failure-focused.tsv -RunRoot $runRoot -Focused
```

Ожидается ровно семь физических xUnit-строк без падений и пропусков, а verifier
должен вернуть код `0`. Эта проверка доказывает только границу публикации двух
журналов и четыре соседних контракта; она не является полным прогоном и не даёт
права объявлять весь набор тестов зелёным.

## Переносимый отказ публикации журнала локального продвижения

Журнал локального продвижения сначала публикует ограниченный durable pending,
а уже затем собирает и атомарно заменяет целевой JSONL-файл. Внутренний тестовый
callback расположен после проверки отмены и непосредственно перед публикацией
pending-файла, внутри существующей границы обработки ошибок. В production он
равен `null`, не получает путь, запись журнала или идентификаторы операции и не
меняет `CancellationToken.None`, fsync, recovery, фильтры исключений либо порядок
pending → временный файл → замена. Одноразовый `IOException` поэтому одинаково
воспроизводит отказ в Windows и Linux/WSL, не полагаясь на разные правила
`FileShare`.

Два отрицательных теста проверяют неизменность целевых байтов, точную стабильную
ошибку без пути и приватных маркеров, отсутствие pending/temp/stage-следов и
успешный повтор тем же объектом базы за десять секунд. Повтор вызывает callback
второй раз и добавляет ровно одну запись без дубля. Четыре положительных соседа
защищают обычные две записи и replay, ожидание общей mutation lease для
продвижения и отката и единственный откат после незавершённого продвижения.

Сборка тестового проекта и точечная проверка двух отрицательных методов:

```powershell
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet build Tests/LlmFromScratch.Tests.csproj -c Release --nologo'
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName=DesignConversionLocalArtifactDbTrainingLocalPromotionDurabilityTests.RunTrainingIntakeAndPromoteLocalAsync_InjectedPendingJournalPublicationFailure_FailsSafelyPreservesBytesAndRetryAppendsOnce|FullyQualifiedName=DesignConversionLocalArtifactDbTrainingLocalPromotionDurabilityTests.RollbackTrainingIntakeLocalPromotionAsync_InjectedPendingJournalPublicationFailure_FailsSafelyPreservesBytesAndRetryAppendsOnce"'
```

Канонический короткий пакет привязывает шесть точных методов к исходникам, DLL
и manifest:

```powershell
$runRoot = Join-Path $PWD ("TestResults\design-conversion-local-promotion-journal-publication-failure-focused-" + (Get-Date -Format "yyyyMMddTHHmmssZ"))
tools\test-batches\run-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-local-promotion-journal-publication-failure-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused -NoBuild
tools\test-batches\verify-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-local-promotion-journal-publication-failure-focused.tsv -RunRoot $runRoot -Focused
```

Ожидается ровно шесть физических xUnit-строк без падений и пропусков, а verifier
должен вернуть код `0`. Весь затронутый класс проверяется отдельно ограниченным
запуском:

```powershell
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && timeout 600s dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~DesignConversionLocalArtifactDbTrainingLocalPromotionDurabilityTests"'
```

Контрольный запуск этого среза выполнил 29 физических строк: 29 прошли, падений
и пропусков нет. Ни один из этих прогонов не является полной проверкой всего
набора тестов и не даёт права объявлять его зелёным.

## Переносимый отказ публикации реестра активации модели

Реестр активации сначала создаёт новый временный файл, записывает JSON без BOM,
выполняет асинхронный flush и принудительный flush на диск, закрывает файл и
только затем атомарно заменяет действующий снимок. Внутренний тестовый callback
расположен между надёжной записью временного файла и заменой. В production он
равен `null`, не получает путь, JSON, tenant или ключ операции и потому не
расширяет поверхность утечки.

Отрицательный тест вводит одноразовый `IOException` через `Interlocked` и не
полагается на отличающиеся между Windows и Linux правила read-only/FileShare.
Он проверяет точный безопасный код и текст ошибки без приватного маркера,
неизменные старые байты, одну старую durable-операцию, отсутствие временных
файлов и прежнюю версию как в тёплом кэше, так и в новом экземпляре реестра.
Повтор той же команды тем же экземпляром должен завершиться за десять секунд,
вызвать callback второй раз, опубликовать поколение 2 и оставить ровно две
durable-операции без дубля.

Сначала собирается тестовый проект и запускается точный отрицательный метод в
разрешённой WSL-среде. Затем канонический короткий пакет добавляет четыре
положительных соседних сценария и привязывает результат к исходникам, DLL и
manifest:

```powershell
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet build Tests/LlmFromScratch.Tests.csproj -c Release --nologo'
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build --filter "FullyQualifiedName=DesignConversionModelActivationRegistryTests.ActivateAsync_InjectedPublicationFailureAfterTempFlush_PreservesCachedAndDurableSnapshotAndRetryPublishesOnce"'
$runRoot = Join-Path $PWD ("TestResults\design-conversion-model-activation-store-publication-failure-focused-" + (Get-Date -Format "yyyyMMddTHHmmssZ"))
tools\test-batches\run-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-model-activation-store-publication-failure-focused.tsv -RunRoot $runRoot -LeafMode pack -Focused -NoBuild
tools\test-batches\verify-test-batches.ps1 -BatchesFile tools\test-batches\design-conversion-model-activation-store-publication-failure-focused.tsv -RunRoot $runRoot -Focused
```

Ожидается ровно пять физических xUnit-строк без падений и пропусков, а verifier
должен вернуть код `0`. Весь класс реестра проверяется отдельным ограниченным
запуском. Эти проверки не являются полным прогоном всего набора тестов и не
дают права объявлять его зелёным.

Контрольный запуск среза выполнил пять физических строк: 5 прошли, падений и
пропусков нет, verifier вернул код `0`. Отдельный запуск всего класса реестра
выполнил 73 строки: 73 прошли, падений и пропусков нет.

## Сохранение обученных весов при удержанном открытом файле

Этот срез защищает сохранение весов и метрик после измерения точности
Design-to-Code на Linux/WSL поверх `/mnt/c`. Ошибка была не в очистке: обычный
`.NET File.Move` переносил временный файл в итоговое имя, но пока второй
открытый файл ещё удерживался для доказательства владения, итоговое имя было
видно в каталоге и одновременно не открывалось для чтения. Под
«физическим файловым объектом» здесь понимается тот же файл на диске, а не
новое имя с похожими байтами. Контрольная сумма — это SHA-256, которым тесты
проверяют, что содержимое не подменилось. Наблюдатель — обработчик этапов
сохранения, через который мост подтверждает владение файлами; тесты
дополнительно наблюдают его работу.

После Release-сборки запускаются две короткие порции: первая порция из пяти
точных методов проверяет три исходных падения сохранения измеренной точности и
две защиты файловой публикации; вторая порция из двух методов отдельно
проверяет обычный тренер без наблюдателя и существующую защиту от чужого
hardlink-подменённого итогового файла:

```powershell
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT timeout 240s dotnet build Tests/LlmFromScratch.Tests.csproj -c Release --nologo'
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT timeout 180s dotnet test Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll --filter "FullyQualifiedName=DesignConversionDpoStorageTests.EvaluatedBridge_PersistsHeldOutAccuracyIntoRuntimeCheckpointExportMachineAndReload|FullyQualifiedName=DesignConversionDpoStorageTests.ImportedEvaluatedRuntimeEvidence_IsSnapshotAndClearsAfterTrainBatch|FullyQualifiedName=DesignConversionDpoStorageTests.ImportedEvaluatedRuntimeEvidence_ClearsAfterSuccessfulEvaluateAsync|FullyQualifiedName=DesignConversionDpoStorageTests.DpoTrainer_LinuxNoReplacePublicationKeepsFinalOpenWithRetainedStagingProof|FullyQualifiedName=DesignConversionDpoStorageTests.DpoTrainer_LinuxExistingFinalDestinationFailsClosedAndPreservesForeignBytes"'
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT timeout 180s dotnet test Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll --filter "FullyQualifiedName=DesignConversionDpoStorageTests.DpoTrainer_LinuxNoObserverPublicationUsesRealProofAndPublishesPolicyAndMetrics|FullyQualifiedName=DesignConversionDpoStorageTests.DpoTrainer_ForeignHardLinkedFinalReplacementIsRejectedAndPreserved"'
```

Физический TRX/XML нужно проверять по полным именам методов и outcome каждой
строки; заголовок `Passed!` сам по себе не является достаточным доказательством.
Семь строк в двух порциях не являются полным прогоном всего класса или всего
набора тестов.

## Разрешённый внешний учитель и честное авторство сторон пары

> Пометка 2026-09-01 (исполнитель): exact dual-target external-teacher
> aggregate теперь разделяет две обязанности. Внешний teacher request
> списывается и коммитится ровно один раз, только чтобы закрепить эталонные
> пары. После этого ordinary local runner получает store-minted child records и
> обязан реально продолжить локальную модель; успешный путь принимается только
> когда continue-training probe доказывает, что checkpoint изменился после fresh
> load. Это не флаг готовности и не повторное потребление внешнего учителя.
> Bridge после store-owned committed external consumption не удерживает parent
> lease для composite и не списывает повторно исходный teacher request: bridge
> получает два store-minted child паспорта из committed transaction/group
> identities и реального custody payload. Train partition для bridge сохраняет
> порядок prepared batches, но каждую matched строку материализует через
> проверенный custody secret; final resolved records больше не считаются уже
> раскрытыми.
> Для committed external promotion bridge получает отдельный store-minted
> transfer-row override: ordinary evaluated packing берёт materialized
> `TrainRecords`, а identity/source sets сверяются с consumed partition.
> Positive storage helper переводит один heldout child в тот же
> review-pending lifecycle, что и train children: сначала Save пишет
> публичную строку без сырого payload, затем approval mint-ит persisted
> custody evidence, и bridge раскрывает heldout уже из storage evidence,
> а не из caller in-memory row. Helper не собирает custody вручную:
> после runtime-approved metadata и artifact proof refs он переиспользует
> canonical `AttachOptimizerPayloadCustody` с исходными real html/css.
> Committed external local-promotion path принимает replay с уже записанным
> Intent, но не пропускает валидацию state-machine: Prepare/Intent доводятся
> перед pair-store/result gates, а Prepared/Committed остаются только в
> `StorePromoteLocalIdempotencyResultAsync`.
> Prepared journal hashes также должны опираться на pair-store rows:
> committed external path пишет store-minted transfer pairs через тот же
> `PersistFinalTrainingIntakeDpoPairsBeforeRunner`, не подставляя caller
> originals и не пропуская pair-store evidence.
> Partition-проверки после runner больше не используют один старый снимок для
> всех границ. Admitted сравнение берёт frozen snapshot, проверка перед bridge
> и проверка перед публикацией заново читают training-intake records; для
> committed external path заменяются только две train-строки на store-minted
> transfer rows, а held-out экзамен остаётся fresh-read. Если held-out строка
> меняется после runner, локальная публикация должна закрыто остановиться до
> upload/activation.
> Focused exact-run должен проверять две разные вещи: внешний эталон потреблён
> один раз, а replay больше не вызывает ни внешний teacher request, ни локальное
> продолжение обучения; при этом первый запуск обязан иметь
> `CheckpointMutated=true`, verified continue-training и promoted checkpoint,
> отличный от checkpoint внешней транзакции.

Этот срез защищает два ранних барьера вокруг локально размещённого открытого
учителя.

Первая группа проверок относится к внутреннему допуску корпуса. Сначала запрос
с правильным подтверждением сравнения, но с неверными настройками сохранения
обученной модели, должен остановиться до трёх действий: до расходования
одноразовой квитанции допуска, до запуска обучающей машины и до вызова
поставщика базовой политики. После этого повторный запрос с той же квитанцией
и исправленными настройками должен пройти весь локальный хвост: одна
серверная внешняя транзакция закрепляет эталонную пару, затем обычный
локальный runner продолжает модель на store-minted child-парах, bridge
сохраняет checkpoint, пишет local-only receipt, проверяет fresh-load и
возвращает durable local-ready model record. Это доказывает, что плохой
запрос не сжёг одноразовый допуск, а хороший запрос больше не застревает после
external-consumption commit.

Этот bridge проверяет четыре человеческих сценария. Успешный сценарий:
допущенный корпус сохраняет train/held-out partition, custody commitment,
child identities и source-proof identities; две обучающие строки становятся
store-owned child transfer rows, а held-out строки остаются экзаменом, не
становятся training-grade случайно. Повтор того же запроса возвращает
сохранённый результат без второго внешнего списания, без новой локальной
мутации, без новой публикации, без новых journal/pair files. Сценарий
неверных настроек останавливается до receipt, runner, output directory,
publication и machine mutation. Сценарий подменённой квитанции не имеет права
вернуть cached terminal result. Сценарий non-corpus request не должен падать в
обычный promote-local route.

Ограничение claim: зелёные проверки ниже доказывают только этот локальный
bridge и соседний generic committed-external local-continuation хвост. Они не
доказывают external AI/eval/cloud execution, удалённую загрузку на model
server, автоматическую активацию модели для production traffic, полный
solution build на WSL или весь test suite. Публикация в этом срезе остаётся
local-only receipt: manifest явно не заявляет remote upload или model-server
promotion.

Вторая группа относится к обычным строкам с открытым учителем. Паспорт учителя
говорит, какая модель участвовала, но не говорит, кто написал каждую сторону
пары. Поэтому вспомогательная функция тестов теперь добавляет явное авторство
до привязки скрытого содержимого пары к хэшам. При этом строка со статусом
«ожидает проверки» без отдельного независимого доказательства предпочтения
всё ещё не допускается в тренерский вход. Соседний успешный тест проверяет
отдельный путь измерения и сохранения модели: авторство и разрешение учителя
проверяются для обучающих и отложенных проверочных примеров.

Фактический Debug-прогон этого bridge-среза в WSL выполнялся такими командами:

```bash
cd /mnt/c/Projects/llm_mvp_core
timeout 240s dotnet build --no-restore
timeout 240s dotnet test Tests/LlmFromScratch.Tests.csproj --no-restore --filter 'FullyQualifiedName~DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_CommitsExternalConsumptionOnceAndExactReplayHasNoNewEffects'
timeout 360s dotnet test Tests/LlmFromScratch.Tests.csproj --no-build --filter 'FullyQualifiedName~DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_CommitsExternalConsumptionOnceAndExactReplayHasNoNewEffects|FullyQualifiedName~DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_InvalidBridgeOptionsFailBeforeCorpusReceiptOrMachine|FullyQualifiedName~DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_TamperedReplayReceiptCannotReturnCachedTerminal|FullyQualifiedName~DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_NonCorpusIntakeRequestFailsBeforeOrdinaryPromotion'
timeout 360s dotnet test Tests/LlmFromScratch.Tests.csproj --no-build --filter 'FullyQualifiedName~DesignConversionDpoStorageTests.LocalArtifactDb_RunTrainingIntakeAndPromoteLocalAsync_ExactDualTargetTeacherAggregate'
```

Результаты этого Debug-прогона: solution build вернул код `1` только из-за
шести WSL-ошибок отсутствующего WindowsDesktop SDK в desktop-проектах; при
этом `44.DesignConversionDomain` и `Tests/LlmFromScratch.Tests.csproj`
скомпилировались. Primary corpus replay test прошёл `1/1`. Corpus-neighbor
набор прошёл `9/9`: это четыре named tests, где invalid bridge options
разворачиваются в шесть physical theory rows. Generic committed-external
local-continuation набор прошёл `5/5`.

Короткая проверка после Release-сборки состоит из двух отдельных групп тестов:

```powershell
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT timeout 240s dotnet build Tests/LlmFromScratch.Tests.csproj -c Release --nologo'
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT timeout 180s dotnet test Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll --no-build --filter "FullyQualifiedName~DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_InvalidBridgeOptionsFailBeforeCorpusReceiptOrMachine|FullyQualifiedName~DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_CommitsExternalConsumptionOnceAndExactReplayHasNoNewEffects|FullyQualifiedName~DesignConversionDpoCorpusAdmissionTests.LocalPromotionWithCorpusAdmission_TamperedReplayReceiptCannotReturnCachedTerminal"'
wsl.exe -d Ubuntu-24.04 -- bash -lc 'cd /mnt/c/Projects/llm_mvp_core && env -u PROJECT_WIKI_TRAINING_ARTIFACT_ROOT timeout 180s dotnet test Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll --no-build --filter "FullyQualifiedName~DesignConversionDpoStorageTests.BuildOrdinaryExternalTeacherApprovedRuntimeRecord_CarriesOpenTeacherSideAuthorshipBeforeIntake|FullyQualifiedName~DesignConversionDpoStorageTests.EvaluatedBridge_ValidExternalTeacherAcrossTrainAndHeldOutSucceedsAndDeduplicatesRequest"'
```

Эти короткие команды проверяют только выбранные границы. Закрытый теперь мост
от допущенного корпуса открытого учителя до долговечной локальной публикации
проверяется отдельными тестами повторного запуска после сбоя и отмены. Они не
запускают облачную загрузку, активацию на сервере моделей, весь набор тестов или
проверку Windows.

## Старый журнал порога качества и переносимые переводы строк

Проверка старого повтора локальной публикации модели вручную пересобирает
журнал операций, чтобы убрать из результата только версию правила порога
качества. Это должно оставлять журнал внутренне валидным: повтор обязан дойти
до сравнения правил порога и остановиться именно там, не запуская обучение,
публикацию или активацию модели.

Рабочий писатель журнала операций всегда завершает каждую запись CRLF-строкой.
Поэтому тесты, которые вручную меняют записи этого журнала, тоже должны писать
CRLF. Если на Linux использовать обычную запись строк с LF, читатель справедливо
считает файл повреждённым и возвращает отказ состояния операции раньше нужной
проверки. Такой результат не доказывает поведение старого валидного журнала —
он доказывает только защиту от битого файла.

Короткая проверка этого среза состоит из трёх смысловых точек: старый валидный
журнал без версии правила должен упасть на конфликте правила порога; текущий
журнал с версией правила должен повторяться без нового обучения; намеренно
испорченная цепочка операций должна по-прежнему останавливаться как повреждённый
журнал операций.

## Удержание артефактов публикации во время отложенной загрузки

Проверка отложенной загрузки модели должна учитывать разную семантику файловых
имён на Windows и Linux. На Windows открытый файл с узким режимом совместного
доступа блокирует удаление, запись и переименование, поэтому тест прямо
проверяет, что внешняя атака на имя файла не проходит, пока загрузчик стоит на
паузе.

На Linux открытый дескриптор не запрещает `unlink` или `rename` имени в
каталоге. Если тест сам удалит путь из manifest-а, он разрушит вход загрузчика и
получит ошибку отсутствующего файла, а не доказательство удержания артефакта.
Поэтому Linux-ветка этой проверки не делает разрушительных операций с именами:
она проверяет, что все ожидаемые пути существуют и байты не изменились во время
паузы. Защиты Linux от подмены физического файлового объекта проверяются
отдельными identity-bound тестами тренера и файлового моста.

Отдельная Linux-проверка ограничения длины имени ожидает `ArgumentException`.
Это не ослабляет fail-closed поведение хранилища: имя длиннее 255 байт UTF-8
останавливается до системного вызова как неверный входной leaf-компонент, тогда
как повреждённые или подменённые сохранённые файлы по-прежнему проверяются
отдельными сценариями отказа хранилища.

В том же малом листе есть проверка уборки после нефатальной первичной ошибки и
фатальной ошибки освобождения резерва. Текущая Linux-реализация уже умеет
удалить принадлежащий попытке временный файл через удержанный дескриптор, поэтому
повтор после такой ошибки должен доходить до успешной публикации шага, а не
закреплять старую коллизию временного файла.
