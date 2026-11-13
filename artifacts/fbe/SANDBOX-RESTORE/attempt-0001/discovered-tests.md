# Какие проверки обращаются к каталогу испытаний в репозитории

Дата: 2026-11-09. Каталог испытаний — это папка `.codex-test-temp` в корне
репозитория, куда проверки кладут свои временные хранилища вместо системной
временной папки.

Поиск выполнен по всем файлам проверок (`C:\Projects\llm_mvp_core\Tests\*.cs`)
по двум признакам: буквальное упоминание имени каталога (`.codex-test-temp`) и
вызов общей оснастки, которая собирает путь внутри него
(`CreateHandleBoundTempRoot`). Найдено 28 обращений в 9 файлах.

## Общая оснастка, которая собирает путь

`Tests\DesignConversionPostEvaluationEligibilityTestFixture.cs`, строки 11-19,
метод `CreateHandleBoundTempRoot(scope)`. Он склеивает
`<корень репозитория>/.codex-test-temp/<область>-<уникальный номер>`. Имя
области обрезается до трёх знаков (строки 446-457, метод
`SanitizePathSegment`), поэтому папки выглядят как `pos-<32 знака>`, а не как
полное человеческое имя области. Сам метод только считает путь и ничего не
создаёт: папку создаёт вызывающая проверка через `Directory.CreateDirectory`.

## Проверки, которые действительно создают папки (создающие)

### Класс допуска корпуса обучающих пар (DesignConversionDpoCorpusAdmissionTests)

Создаёт папку в общей оснастке `CreateFixture` (строки 3739-3743:
`CreateHandleBoundTempRoot` плюс `Directory.CreateDirectory`). Эту оснастку
вызывают 25 проверок класса. Прямые упоминания: строки 108, 618, 2289, 3742.

Важно для итога: оснастка одноразовая (`using var fixture = CreateFixture()`),
а её освобождение `Dispose` (строки 4103-4106) вызывает удаление с повторами
`DeleteDirectoryWithRetry` (строки 4109-4122), то есть
`Directory.Delete(root, recursive: true)`. Удаляется только собственная папка
прогона внутри каталога испытаний; сам каталог испытаний не удаляется.
Поэтому эти проверки каталог создают, но свои папки за собой убирают.

Проверка, прямо требующая каталог в репозитории:
`CandidatePayloadCustodyStore_RepoRootTempRootPreparesCommitsAndResolves`
(строки 101-127). Она утверждает, что корень хранилища кандидатов лежит внутри
каталога испытаний репозитория (строки 114-116) и НЕ уходит в системную
временную папку (строки 117-119).

### Класс допуска к обучению после оценки (DesignConversionPostEvaluationTrainingEligibilityTests)

Создаёт папки через собственную оснастку `NewRoot` (строки 2028-2031). Её
вызывают 14 проверок. В этом файле нет ни одного `Directory.Delete` и нет
освобождения `Dispose`, то есть папки остаются на диске после прогона. Это
единственный из найденных классов, который гарантированно оставляет видимое
содержимое.

Полный список этих 14 проверок:
ExactCommittedMatchingState_AllowsEveryLocalArtifactConsumerCategory;
AppendFailure_LeavesPreparedStateProvisionalAndCentrallyIneligible;
FixtureReplayWithSameIdsButDifferentPayloadCannotCommitOrRebindOriginalDurableRow;
LocalReadbackSnapshot_UsesOneInventoryAndPreservesRecordEligibilityPairing;
MeasurementSnapshot_UsesOneInventoryAndIncludesOnlyPositionallyEligibleRecord;
TrainingIntakeResolution_BatchesDistinctRecordsOnceForDuplicateAndMissingRefs;
ConsumerSnapshots_FailClosedWhenInventoryFailsAndOnCancellation;
DerivedIndexCountRefreshesAfterStateOnlyEligibilityTransition;
StableEligibilitySnapshot_DoesNotRewriteCurrentDerivedIndex;
IncompleteOrNonDurableShapes_AreDeniedFailClosed;
MissingMetadata_IsDeniedAcrossApprovalQueryExportDisplayIndexMeasurementAndReadback;
RebuiltIndex_DoesNotCountProvisionalOrQuarantinedCandidatesAsApproved;
QueueAfterExport_DeletedStateFailsBeforeIntakeJournalAppend;
QueuedWhileCommitted_DeletedBeforeRunBlocksTrainerDependency.

### Класс хранилища предпочтений (DesignConversionDpoStorageTests)

15 прямых вызовов оснастки из проверок с собственными именами (строки 2538,
2618, 2686, 2739, 2799, 2889, 2915, 3058, 7919, 26534, 28897, 29365, 29472,
30713, 31276). В файле 203 вызова удаления каталога и 5 освобождений
`Dispose`, то есть значительная часть прогонов убирает за собой.

### Класс долговечности прогона обучения (DesignConversionLocalArtifactDbTrainingRunDurabilityTests)

Создаёт папки через оснастку `CreateTempRoot` (строка 1368), её вызывают 16
проверок; есть одно удаление каталога (строка 1373).

### Класс хранилища состояния после оценки (DesignConversionPostEvaluationStateStoreTests)

Строка 326 — проверка с набором входных данных
`WriteAsync_RepoRootV9fsUnsafeNamespaceEntryFailsClosed`; строка 3470 —
оснастка `NewRepoRootStateRoot`, её вызывают 6 проверок. Есть два удаления
(строки 3568, 3570).

### Класс распознавателя корня хранилища пар (DesignConversionTrainingIntakeDpoPairStoreRootResolverTests)

Оснастка `NewArtifactRoot` (строки 640-646) создаёт папку. Её вызывают 15
внутренних вспомогательных методов, прямых вызовов из проверок с именами нет.

## Проверки, которые каталог только исключают из обхода (не создающие)

Утверждение родителя подтверждено, а не опровергнуто.

`Tests\RepositorySourceScanner.cs`, строка 15: имя каталога стоит в списке
исключаемых имён папок при обходе репозитория. Создания нет.

`Tests\LegacyDpoArtifactQuarantineManifestTests.cs`, строка 436: сравнение
имени папки с именем каталога испытаний внутри метода `IsRuntimeDirectory`,
то есть распознавание служебной папки. Создания нет.
