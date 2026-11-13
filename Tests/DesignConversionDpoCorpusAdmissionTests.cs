// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace Tests;

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;
using Xunit;

public sealed class DesignConversionDpoCorpusAdmissionTests
{
    [Theory]
    [InlineData(true, true, true, false, false, false, true)]
    [InlineData(false, false, false, true, true, true, true)]
    [InlineData(true, true, false, false, false, true, false)]
    [InlineData(true, false, true, false, true, false, false)]
    [InlineData(false, true, true, true, false, false, false)]
    [InlineData(true, false, false, false, false, false, false)]
    [InlineData(false, false, false, false, false, false, false)]
    public void CorpusAdmissionRequirementPolicy_RequiresAllThreeMarkersOnOneRecordAndAnyQualifyingRecord(
        bool firstAuthored,
        bool firstPreferenceProven,
        bool firstRejectedEvaluated,
        bool secondAuthored,
        bool secondPreferenceProven,
        bool secondRejectedEvaluated,
        bool expected)
    {
        var records = new[]
        {
            CreateCorpusRequirementPolicyRecord(
                "corpus-requirement-first",
                firstAuthored,
                firstPreferenceProven,
                firstRejectedEvaluated),
            CreateCorpusRequirementPolicyRecord(
                "corpus-requirement-second",
                secondAuthored,
                secondPreferenceProven,
                secondRejectedEvaluated)
        };
        var identitiesBefore = records
            .Select(DesignConversionDpoDatasetBuilder.ComputeRecordIdentityHash)
            .ToArray();
        var metadataBefore = records
            .Select(static record => JsonSerializer.Serialize(
                record.Provenance.Metadata.OrderBy(
                    static pair => pair.Key,
                    StringComparer.Ordinal)))
            .ToArray();

        var requiresAdmission = DesignConversionDpoCorpusAdmissionRequirementPolicy
            .RequiresAdmission(records);

        Assert.Equal(expected, requiresAdmission);
        Assert.Equal(
            identitiesBefore,
            records.Select(DesignConversionDpoDatasetBuilder.ComputeRecordIdentityHash));
        Assert.Equal(
            metadataBefore,
            records.Select(static record => JsonSerializer.Serialize(
                record.Provenance.Metadata.OrderBy(
                    static pair => pair.Key,
                    StringComparer.Ordinal))));
    }

    [Fact]
    public void CorpusAdmissionAuthority_IsInternalOpaqueAndOneShotByConstruction()
    {
        var authority = typeof(DesignConversionDpoCorpusAdmissionAuthority);
        var admission = typeof(DesignConversionDpoCorpusAdmission);

        Assert.False(authority.IsPublic);
        Assert.False(admission.IsPublic);
        Assert.False(admission.IsSerializable);
        Assert.Empty(admission.GetCustomAttributes<JsonConverterAttribute>());
        Assert.DoesNotContain(
            admission.GetProperties(BindingFlags.Instance | BindingFlags.Public),
            static property => property.PropertyType == typeof(string));

        var consume = Assert.Single(admission.GetMethods(
            BindingFlags.Instance | BindingFlags.NonPublic),
            static method => method.Name == "TryConsumeForPipeline");
        Assert.Equal(typeof(bool), consume.ReturnType);
        Assert.Null(typeof(DesignConversionTrainingRunner).GetMethod(
            "RunAdmittedCorpusAsync",
            BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.Null(typeof(DesignConversionTrainingPipeline).GetMethod(
            "RunAdmittedCorpusValidatedAsync",
            BindingFlags.Instance | BindingFlags.NonPublic));
    }

    [Fact]
    public void CandidatePayloadCustodyStore_RepoRootTempRootPreparesCommitsAndResolves()
    {
        using var fixture = CreateFixture();
        var fullRoot = Path.GetFullPath(fixture.Root);
        var repositoryRoot = FindRepositoryRootFromContainedPath(fullRoot);
        var tempCarrier = Path.GetFullPath(
            Path.Combine(repositoryRoot, ".codex-test-temp"));
        var relativeToCarrier = Path.GetRelativePath(tempCarrier, fullRoot);
        var systemTemp = Path.GetFullPath(Path.GetTempPath());

        Assert.NotEmpty(fixture.Records);
        Assert.NotNull(fixture.Capability);
        Assert.True(
            IsUnderDirectory(fullRoot, tempCarrier),
            $"{fullRoot} must stay under repository temp carrier {tempCarrier}.");
        Assert.False(
            IsUnderDirectory(fullRoot, systemTemp),
            $"{fullRoot} must not be routed through system temp {systemTemp}.");
        Assert.False(Path.IsPathFullyQualified(relativeToCarrier));
        Assert.NotEqual(".", relativeToCarrier);
        Assert.NotEqual("..", relativeToCarrier);
        Assert.False(
            relativeToCarrier.StartsWith(
                ".." + Path.DirectorySeparatorChar,
                StringComparison.Ordinal));
    }

    [Fact]
    public void ExactAdmission_ProjectsOnlyDerivativeRowsAndPreservesOriginals()
    {
        using var fixture = CreateFixture();
        var originalHashes = fixture.Records
            .Select(DesignConversionDpoDatasetBuilder.ComputeRecordIdentityHash)
            .ToArray();

        var issued = fixture.Db.IssueDpoCorpusAdmission(
            fixture.Records,
            fixture.Capability,
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var duplicateIssue = fixture.Db.IssueDpoCorpusAdmission(
            fixture.Records,
            fixture.Capability,
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        Assert.False(duplicateIssue.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.ReplayErrorCode,
            duplicateIssue.ErrorCode);
        Assert.All(fixture.Records, static record =>
        {
            Assert.False(record.Provenance.Metadata.ContainsKey(
                "runtime_teacher_selected_preference"));
            Assert.Equal(bool.FalseString, record.Provenance.Metadata["training_grade"]);
            Assert.Equal(bool.TrueString, record.Provenance.Metadata["not_training_grade"]);
        });

        var projected = new DesignConversionDpoDatasetBuilder()
            .BuildAdmittedCorpus(issued.Value!);
        Assert.True(projected.IsSuccess, projected.ErrorMessage);
        Assert.Equal(2, projected.Value!.Dataset.TrainRows.Count);
        Assert.NotEmpty(projected.Value.Dataset.HeldOutRows);
        Assert.All(projected.Value.Dataset.TrainRows, static row =>
        {
            Assert.True(row.ValidationOutcome.TrainingGrade);
            Assert.False(row.ValidationOutcome.NotTrainingGrade);
            Assert.Equal(bool.TrueString,
                row.Metadata["runtime_teacher_authored_candidate"]);
            Assert.Equal(bool.FalseString,
                row.Metadata["runtime_teacher_selected_preference"]);
            Assert.Equal("server_owned_one_shot",
                row.Metadata["corpus_admission_authority"]);
            Assert.Equal(bool.TrueString, row.Metadata["product_not_ready"]);
        });
        Assert.All(projected.Value.Dataset.HeldOutRows,
            static row => Assert.False(row.ValidationOutcome.TrainingGrade));
        Assert.All(projected.Value.Dataset.ToJsonlLines(), static line =>
        {
            using var json = JsonDocument.Parse(line);
            Assert.Equal(5, json.RootElement.EnumerateObject().Count());
            Assert.DoesNotContain("evaluator", line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("reviewer", line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("result", line, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("audit", line, StringComparison.OrdinalIgnoreCase);
        });

        Assert.Equal(
            originalHashes,
            fixture.Records
                .Select(DesignConversionDpoDatasetBuilder.ComputeRecordIdentityHash));
        Assert.All(projected.Value.Records, static record => Assert.Equal(
            bool.FalseString,
            record.Provenance.Metadata["runtime_teacher_selected_preference"]));
        for (var index = 0; index < fixture.Records.Count; index++)
            Assert.NotSame(fixture.Records[index], projected.Value.Records[index]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void AdmittedCorpusProjection_NormalizesHostileCallerOptionsButPreservesRunGateInputs(
        int hostileBatchSize)
    {
        using var fixture = CreateFixture();
        var issued = fixture.Db.IssueDpoCorpusAdmission(
            fixture.Records,
            fixture.Capability,
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);
        var measurementGate =
            BuildPassingMeasurementGateRequest("hostile-caller-options-gate");
        var bridgeOptions = new DesignConversionDpoPolicyCheckpointBridgeOptions(
            MaxBridgePreferenceCount: 7,
            DpoStepCount: 3)
        {
            DpoConvergenceThreshold = 0.123f,
            DpoConvergenceWindowSteps = 4
        };
        var hostileOptions = new DesignConversionTrainingPipelineOptions(
            RunId: "caller-controlled-hostile-run-id",
            BatchSize: hostileBatchSize,
            CallMachineLifecycleHook: false,
            RequireDatasetCountGate: false,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: "caller-controlled-hostile-dataset",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 0,
                HeldOutRecordFraction: 0.99,
                Preflight: DesignConversionDpoDatasetPreflight
                    .LocalFixtureTrainingCandidateOnly(
                        "caller-controlled-hostile-preflight")),
            MeasurementGateRequest: measurementGate,
            DpoPolicyBridgeOptions: bridgeOptions);

        var projected = new DesignConversionDpoDatasetBuilder()
            .BuildAdmittedCorpus(issued.Value!, hostileOptions);

        Assert.True(projected.IsSuccess, projected.ErrorMessage);
        var options = projected.Value!.Options;
        Assert.Equal(
            $"corpus-admission:{issued.Value!.ReceiptSha256[7..23]}",
            options.RunId);
        Assert.Equal(2, options.BatchSize);
        Assert.NotEqual(hostileBatchSize, options.BatchSize);
        Assert.True(options.CallMachineLifecycleHook);
        Assert.True(options.RequireDatasetCountGate);
        Assert.Same(measurementGate, options.MeasurementGateRequest);
        Assert.Equal(bridgeOptions, options.DpoPolicyBridgeOptions);
        Assert.NotNull(options.DatasetOptions);
        Assert.Equal(projected.Value.Dataset.DatasetId, options.DatasetOptions!.DatasetId);
        Assert.Equal(
            projected.Value.Dataset.Manifest.CountGate.RequiredTrainRecords,
            options.DatasetOptions.MinimumTrainRecords);
        Assert.Equal(
            projected.Value.Dataset.Manifest.CountGate.RequiredHeldOutRecords,
            options.DatasetOptions.MinimumHeldOutRecords);
        Assert.Equal(0.2, options.DatasetOptions.HeldOutRecordFraction);
        Assert.Same(
            projected.Value.Dataset.Manifest.Preflight,
            options.DatasetOptions.Preflight);
        Assert.DoesNotContain(
            "caller-controlled-hostile",
            JsonSerializer.Serialize(
                options,
                DesignConversionCheckpointJsonOptions.Standard),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task NoCorpusAuthority_CountGateProducesZeroMachineEffects()
    {
        using var fixture = CreateFixture();
        var effects = 0;
        var previous = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref effects);
        try
        {
            var options = new DesignConversionTrainingPipelineOptions(
                "corpus-no-authority",
                BatchSize: 2,
                CallMachineLifecycleHook: true,
                RequireDatasetCountGate: true,
                DatasetOptions: new DesignConversionDpoDatasetOptions(
                    "corpus-no-authority",
                    2,
                    0,
                    0.0,
                    Preflight: DesignConversionDpoDatasetPreflight
                        .LocalFixtureTrainingCandidateOnly()));
            var result = await new DesignConversionTrainingPipeline().RunAsync(
                CreateLegalAuthority(fixture.Pair),
                new DesignConversionMachine(),
                fixture.Records,
                options);

            Assert.True(fixture.SourceProof.Manifest.CountGate.Passed);
            if (result.IsSuccess)
            {
                Assert.NotNull(result.Value);
                Assert.False(result.Value!.MachineLifecycleHookAttempted);
            }
            else
            {
                Assert.Equal(
                    DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
                    result.ErrorCode);
            }

            Assert.Equal(0, Volatile.Read(ref effects));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previous;
        }
    }

    [Fact]
    public async Task ConcurrentCorpusReplay_AllowsExactlyOneMachineMutation()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var options = BuildCorpusPromotionOptions("concurrent-corpus-replay");
        var firstDb = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            legalTeacherAdmissionAuthority: CreateLegalAuthority(fixture.Pair));
        var secondDb = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            legalTeacherAdmissionAuthority: CreateLegalAuthority(fixture.Pair));

        var effects = 0;
        var previous = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref effects);
        try
        {
            var runs = await Task.WhenAll(
                firstDb.RunTrainingIntakeWithCorpusAdmissionAsync(
                    intake.IntakeId,
                    request,
                    options),
                secondDb.RunTrainingIntakeWithCorpusAdmissionAsync(
                    intake.IntakeId,
                    request,
                    options));

            Assert.Single(runs, static result => result.IsSuccess);
            var rejected = Assert.Single(runs, static result => !result.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.ReplayErrorCode,
                rejected.ErrorCode);
            Assert.Equal(1, Volatile.Read(ref effects));
            var pairStoreResolution = firstDb
                .TrainingIntakeDpoPairStoreRootResolutionForTesting;
            // Две конкурентные БД должны договориться о том же native root:
            // иначе replay мог бы скрыть split-brain между двумя pair stores.
            Assert.Equal(
                pairStoreResolution.PairStoreRootDirectory,
                secondDb
                    .TrainingIntakeDpoPairStoreRootResolutionForTesting
                    .PairStoreRootDirectory);
            if (pairStoreResolution.UsesNativeWslStateRoot)
            {
                // В WSL пары больше не пишутся на /mnt/c: manifest в artifact
                // root только связывает canonical project identity с native root.
                Assert.False(IsUnderDirectory(
                    pairStoreResolution.PairStoreRootDirectory,
                    fixture.Root));
                Assert.True(File.Exists(pairStoreResolution.ManifestPath));
                Assert.Empty(EnumeratePairStoreEntries(
                    pairStoreResolution.LegacyPairStoreRootDirectory));
            }

            var pairRoot = pairStoreResolution.PairStoreRootDirectory;
            Assert.Empty(Directory.EnumerateFiles(
                pairRoot,
                "*.dpo-pair.json",
                SearchOption.TopDirectoryOnly));
            Assert.Empty(Directory.EnumerateFiles(
                pairRoot,
                "*.dpo-pair-group.json",
                SearchOption.TopDirectoryOnly));
            Assert.Empty(Directory.EnumerateFiles(
                pairRoot,
                "*.dpo-transaction.json",
                SearchOption.TopDirectoryOnly));
            Assert.Single(Directory.EnumerateFiles(
                pairRoot,
                "*.dpo-external-consumption.json",
                SearchOption.TopDirectoryOnly));
            Assert.Single(File.ReadLines(Path.Combine(
                fixture.Root,
                "training-intake-runs.jsonl")));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previous;
        }
    }

    [Fact]
    public void DurableReceiptReservation_BlocksFreshDatabaseInstanceReplay()
    {
        using var fixture = CreateFixture();
        var issued = fixture.Db.IssueDpoCorpusAdmission(
            fixture.Records,
            fixture.Capability,
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        Assert.True(issued.IsSuccess, issued.ErrorMessage);

        var reopened = new JsonlDesignConversionLocalArtifactDb(fixture.Root);
        var replay = reopened.IssueDpoCorpusAdmission(
            fixture.Records,
            fixture.Capability,
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);

        Assert.False(replay.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.ReplayErrorCode,
            replay.ErrorCode);
    }

    [Fact]
    public async Task RealTrainingIntake_AdmitsOnceAfterStoredRevalidationAndRestartReplayHasZeroEffects()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var options = BuildCorpusPromotionOptions("real-corpus-replay");
        var effects = 0;
        var previous = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref effects);
        try
        {
            var noAdmissionDb = new JsonlDesignConversionLocalArtifactDb(
                fixture.Root,
                legalTeacherAdmissionAuthority: CreateLegalAuthority(fixture.Pair));
            var noAdmission = await noAdmissionDb.RunTrainingIntakeAsync(
                intake.IntakeId,
                options);
            Assert.False(noAdmission.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
                noAdmission.ErrorCode);
            Assert.Equal(0, Volatile.Read(ref effects));

            var first = await fixture.Db.RunTrainingIntakeWithCorpusAdmissionAsync(
                intake.IntakeId,
                request,
                options);
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(first.Value!.TrainingRunnerInvoked);
            Assert.True(first.Value.TrainingStarted);
            Assert.Equal(1, Volatile.Read(ref effects));
            var pairStoreResolution = fixture.Db
                .TrainingIntakeDpoPairStoreRootResolutionForTesting;
            var pairRoot = pairStoreResolution.PairStoreRootDirectory;
            if (pairStoreResolution.UsesNativeWslStateRoot)
            {
                // В WSL обучающие пары принадлежат native-хранилищу, а не
                // временному каталогу артефактов на Windows-диске.
                Assert.False(IsUnderDirectory(pairRoot, fixture.Root));
                Assert.True(File.Exists(pairStoreResolution.ManifestPath));
                Assert.Empty(EnumeratePairStoreEntries(
                    pairStoreResolution.LegacyPairStoreRootDirectory));
            }
            Assert.Empty(Directory.EnumerateFiles(
                pairRoot,
                "*.dpo-pair.json",
                SearchOption.TopDirectoryOnly));
            Assert.Empty(Directory.EnumerateFiles(
                pairRoot,
                "*.dpo-pair-group.json",
                SearchOption.TopDirectoryOnly));
            Assert.Empty(Directory.EnumerateFiles(
                pairRoot,
                "*.dpo-transaction.json",
                SearchOption.TopDirectoryOnly));
            Assert.Single(Directory.EnumerateFiles(
                pairRoot,
                "*.dpo-external-consumption.json",
                SearchOption.TopDirectoryOnly));

            var reopened = new JsonlDesignConversionLocalArtifactDb(
                fixture.Root,
                legalTeacherAdmissionAuthority: CreateLegalAuthority(fixture.Pair));
            var reopenedPairStoreResolution = reopened
                .TrainingIntakeDpoPairStoreRootResolutionForTesting;
            // Повторный запуск обязан вернуть тот же полный паспорт каталога:
            // совпадения одной строки пути недостаточно для защиты от split-brain.
            Assert.Equal(pairRoot, reopenedPairStoreResolution.PairStoreRootDirectory);
            Assert.Equal(pairStoreResolution, reopenedPairStoreResolution);
            var replay = await reopened.RunTrainingIntakeWithCorpusAdmissionAsync(
                intake.IntakeId,
                request,
                options);
            Assert.False(replay.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.ReplayErrorCode,
                replay.ErrorCode);
            Assert.Equal(1, Volatile.Read(ref effects));
            Assert.Single(Directory.EnumerateFiles(
                pairRoot,
                "*.dpo-external-consumption.json",
                SearchOption.TopDirectoryOnly));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previous;
        }
    }

    [Fact]
    public async Task RunWithoutCorpusAdmission_MultiSourceMultiPairTeacherAuthoredCandidatesRequireCorpusBeforeDuplicateRequestIds()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var options = BuildDefaultTrainingIntakeRunOptions(intake) with
        {
            MeasurementGateRequest = BuildPassingMeasurementGateRequest(
                "multi-source-multi-pair-corpus-before-duplicate-gate")
        };
        var expandedPreview = ExpandRuntimeCorpusCandidatesForSecondSource(
            fixture.Records.Concat(fixture.WitnessRecords).ToArray());
        var previewCorpusRecords = expandedPreview
            .Where(IsCorpusRequirementRecord)
            .ToArray();
        Assert.Equal(4, previewCorpusRecords.Length);
        Assert.Equal(
            2,
            previewCorpusRecords
                .Select(static record => record.Provenance.SourceManifestRef)
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.Contains(
            previewCorpusRecords
                .GroupBy(
                    static record => record.TeacherProvenance?.RequestId,
                    StringComparer.Ordinal),
            static group => !string.IsNullOrWhiteSpace(group.Key) &&
                            group.Count() > 2);

        var filesBefore = SnapshotPersistedFiles(fixture.Root);
        var runnerCount = 0;
        var machineMutationCount = 0;
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        OperationResult<DesignConversionTrainingIntakeRunResult> result;
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(
                fixture.Root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref runnerCount),
                legalTeacherAdmissionAuthority: fixture.LegalAuthority,
                runTrainingIntakeResolvedRecordsTransformerForTesting:
                ExpandRuntimeCorpusCandidatesForSecondSource);
            result = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options);
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, Volatile.Read(ref runnerCount));
        Assert.Equal(0, Volatile.Read(ref machineMutationCount));
        Assert.Equal(filesBefore, SnapshotPersistedFiles(fixture.Root));
        AssertNoTrainingPairOrExternalConsumptionArtifacts(fixture.Root);
    }

    [Fact]
    public async Task RunWithoutCorpusAdmission_OrdinaryNonCorpusDuplicateRequestIdsStillFailDuplicateValidation()
    {
        var root = DesignConversionPostEvaluationEligibilityTestFixture
            .CreateHandleBoundTempRoot("dcm-corpus-admission-ordinary-duplicate");
        Directory.CreateDirectory(root);
        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateOrdinaryApprovedRuntimeTrainingIntakeForTestingAsync(
                setupDb,
                "ordinary-non-corpus-duplicate");
            var options = BuildDefaultTrainingIntakeRunOptions(intake) with
            {
                MeasurementGateRequest = BuildPassingMeasurementGateRequest(
                    "ordinary-non-corpus-duplicate-gate")
            };
            var filesBefore = SnapshotPersistedFiles(root);
            var runnerCount = 0;
            var machineMutationCount = 0;
            var previousMutationObserver = DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting;
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                _ => Interlocked.Increment(ref machineMutationCount);
            OperationResult<DesignConversionTrainingIntakeRunResult> result;
            try
            {
                var db = new JsonlDesignConversionLocalArtifactDb(
                    root,
                    createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                    createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                    runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                        Interlocked.Increment(ref runnerCount),
                    legalTeacherAdmissionAuthority:
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                    runTrainingIntakeResolvedRecordsTransformerForTesting:
                    ForceOrdinaryDuplicateRequestIdWithoutCorpusMarkers);
                result = await db.RunTrainingIntakeAsync(
                    intake.IntakeId,
                    options);
            }
            finally
            {
                DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                    previousMutationObserver;
            }

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.Equal(filesBefore, SnapshotPersistedFiles(root));
            AssertNoTrainingPairOrExternalConsumptionArtifacts(root);
        }
        finally
        {
            DeleteDirectoryWithRetry(root);
        }
    }

    [Fact]
    public async Task LocalPromotionWithoutCorpusAdmission_RejectsValidDualTargetTeacherPartitionBeforeExternalConsumptionOrMutation()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var outputDirectory = Path.Combine(fixture.Root, "promotion-without-corpus-admission");
        var options = BuildCorpusPromotionOptions("promotion-without-corpus-admission");
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = "tenant:promotion-without-corpus",
            ["training_intake_idempotency_key_ref"] =
                "idempotency:promotion-without-corpus"
        };
        var runnerCount = 0;
        var providerCount = 0;
        var publicationCount = 0;
        var activationCount = 0;
        var machineMutationCount = 0;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            Interlocked.Increment(ref providerCount);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet);
        });
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting: () =>
                Interlocked.Increment(ref publicationCount),
            runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting: () =>
                Interlocked.Increment(ref activationCount),
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var filesBefore = SnapshotPersistedFiles(fixture.Root);
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> result;
        try
        {
            result = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
                boundary,
                () => db.RunTrainingIntakeAndPromoteLocalAsync(
                    intake.IntakeId,
                    outputDirectory,
                    options));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, Volatile.Read(ref runnerCount));
        Assert.Equal(0, Volatile.Read(ref providerCount));
        Assert.Equal(0, Volatile.Read(ref publicationCount));
        Assert.Equal(0, Volatile.Read(ref activationCount));
        Assert.Equal(0, Volatile.Read(ref machineMutationCount));
        Assert.False(Directory.Exists(outputDirectory));
        Assert.False(File.Exists(Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionFileName)));
        Assert.Equal(filesBefore, SnapshotPersistedFiles(fixture.Root));
        AssertNoTrainingPairOrExternalConsumptionArtifacts(fixture.Root);
    }

    [Fact]
    public async Task LocalPromotionIdempotentReplay_RevalidatesCorpusAdmissionRequirementBeforeReturningCachedSuccess()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var outputDirectory = Path.Combine(fixture.Root, "authorityless-replay-output");
        var options = BuildCorpusPromotionOptions("authorityless-replay");
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = "tenant:corpus-replay",
            ["training_intake_idempotency_key_ref"] = "idempotency:corpus-replay"
        };
        var cached = BuildCachedPromotionReplay(
            intake,
            options,
            outputDirectory,
            boundary);
        var journalPath = Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionFileName);
        File.WriteAllText(
            journalPath,
            JsonSerializer.Serialize(
                cached,
                new JsonSerializerOptions(DesignConversionCheckpointJsonOptions.Standard)
                {
                    WriteIndented = false
                }) + Environment.NewLine,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var journalBytesBefore = File.ReadAllBytes(journalPath);
        var runnerCount = 0;
        var providerCount = 0;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            Interlocked.Increment(ref providerCount);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(modelSet);
        });
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);

        var result = await DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
            boundary,
            () => db.RunTrainingIntakeAndPromoteLocalAsync(
                intake.IntakeId,
                outputDirectory,
                options));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, Volatile.Read(ref runnerCount));
        Assert.Equal(0, Volatile.Read(ref providerCount));
        Assert.False(Directory.Exists(outputDirectory));
        Assert.Equal(journalBytesBefore, File.ReadAllBytes(journalPath));
        Assert.Single(File.ReadLines(journalPath));
        AssertNoTrainingPairOrExternalConsumptionArtifacts(fixture.Root);
    }

    [Theory]
    [InlineData("max-preference-zero")]
    [InlineData("max-preference-too-large")]
    [InlineData("dpo-steps-zero")]
    [InlineData("dpo-steps-too-large")]
    [InlineData("negative-convergence-threshold")]
    [InlineData("zero-convergence-window")]
    public async Task LocalPromotionWithCorpusAdmission_InvalidBridgeOptionsFailBeforeCorpusReceiptOrMachine(
        string invalidCase)
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var outputDirectory = Path.Combine(
            fixture.Root,
            "promotion-invalid-bridge-options-" + invalidCase);
        var invalidOptions = BuildCorpusPromotionOptions(
            "promotion-invalid-bridge-options-" + invalidCase) with
        {
            DpoPolicyBridgeOptions =
                BuildInvalidCorpusPromotionBridgeOptions(invalidCase)
        };
        var validOptions = BuildCorpusPromotionOptions(
            "promotion-valid-after-invalid-bridge-options-" + invalidCase) with
        {
            DpoPolicyBridgeOptions = BuildValidCorpusPromotionBridgeOptions()
        };
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] =
                "tenant:invalid-bridge-options-" + invalidCase,
            ["training_intake_idempotency_key_ref"] =
                "idempotency:invalid-bridge-options-" + invalidCase
        };
        var providerCount = 0;
        var runnerCount = 0;
        var promotionRunnerBoundary = 0;
        var publicationCount = 0;
        var activationCount = 0;
        var machineMutationCount = 0;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            Interlocked.Increment(ref providerCount);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                modelSet);
        });
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting:
                () => Interlocked.Increment(ref publicationCount),
            runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting:
                () => Interlocked.Increment(ref activationCount),
            runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting:
                () => Interlocked.Increment(ref promotionRunnerBoundary),
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var filesBefore = SnapshotPersistedFiles(fixture.Root);
        var pairStoreBefore = SnapshotPairStoreFiles(db);
        var journalPath = Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionFileName);
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        try
        {
            // Ошибка настроек должна закрыться после проверки формы запроса,
            // но до квитанции допуска корпуса, запуска обучения и мутации машины.
            var invalid = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        request,
                        invalidOptions));

            Assert.False(invalid.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_DPO_POLICY_BRIDGE_OPTIONS_INVALID",
                invalid.ErrorCode);
            Assert.Null(invalid.Value);
            Assert.Equal(0, Volatile.Read(ref providerCount));
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(0, Volatile.Read(ref promotionRunnerBoundary));
            Assert.Equal(0, Volatile.Read(ref publicationCount));
            Assert.Equal(0, Volatile.Read(ref activationCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(journalPath));
            Assert.Equal(filesBefore, SnapshotPersistedFiles(fixture.Root));
            Assert.Equal(pairStoreBefore, SnapshotPairStoreFiles(db));
            AssertNoTrainingPairOrExternalConsumptionArtifacts(fixture.Root);

            // С той же квитанцией допуска повторяем уже с валидными
            // настройками: одноразовый допуск не был израсходован невалидным
            // запросом.
            var valid = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        request,
                        validOptions));

            Assert.True(valid.IsSuccess, valid.ErrorMessage);
            AssertLocalPromotionReadyAfterAdmittedExternalConsumption(
                valid.Value!,
                request);
            Assert.Equal(1, Volatile.Read(ref providerCount));
            Assert.Equal(2, Volatile.Read(ref runnerCount));
            Assert.Equal(2, Volatile.Read(ref promotionRunnerBoundary));
            Assert.Equal(1, Volatile.Read(ref publicationCount));
            Assert.Equal(1, Volatile.Read(ref activationCount));
            Assert.Equal(4, Volatile.Read(ref machineMutationCount));
            Assert.True(Directory.Exists(outputDirectory));
            Assert.Single(File.ReadLines(journalPath));
            AssertSingleExternalConsumptionAndLocalContinuationPairs(db);
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }
    }

    [Fact]
    public async Task LocalPromotionWithCorpusAdmission_CommitsExternalConsumptionOnceAndExactReplayHasNoNewEffects()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var outputDirectory = Path.Combine(
            fixture.Root,
            "promotion-with-internal-corpus-admission");
        var options = BuildCorpusPromotionOptions(
            "promotion-with-internal-corpus-admission");
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] =
                "tenant:promotion-with-internal-corpus",
            ["training_intake_idempotency_key_ref"] =
                "idempotency:promotion-with-internal-corpus"
        };
        var providerCount = 0;
        var runnerCount = 0;
        var promotionRunnerBoundary = 0;
        var publicationCount = 0;
        var activationCount = 0;
        var machineMutationCount = 0;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            Interlocked.Increment(ref providerCount);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                modelSet);
        });
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting:
                () => Interlocked.Increment(ref publicationCount),
            runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting:
                () => Interlocked.Increment(ref activationCount),
            runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting:
                () => Interlocked.Increment(ref promotionRunnerBoundary),
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        try
        {
            // Проверяем внутренний путь: он допускает набор данных, выполняет
            // одну серверную внешнюю транзакцию, а затем продолжает уже
            // существующий local-only promotion tail без внешней загрузки.
            var first = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        request,
                        options));

            Assert.True(
                first.IsSuccess,
                string.Join(
                    " | ",
                    first.ErrorCode,
                    first.ErrorMessage,
                    $"provider={Volatile.Read(ref providerCount)}",
                    $"runner={Volatile.Read(ref runnerCount)}",
                    $"promotionRunner={Volatile.Read(ref promotionRunnerBoundary)}",
                    $"publication={Volatile.Read(ref publicationCount)}",
                    $"activation={Volatile.Read(ref activationCount)}",
                    $"machineMutation={Volatile.Read(ref machineMutationCount)}"));
            AssertLocalPromotionReadyAfterAdmittedExternalConsumption(
                first.Value!,
                request);
            Assert.Equal(1, Volatile.Read(ref providerCount));
            Assert.Equal(2, Volatile.Read(ref runnerCount));
            Assert.Equal(2, Volatile.Read(ref promotionRunnerBoundary));
            Assert.Equal(1, Volatile.Read(ref publicationCount));
            Assert.Equal(1, Volatile.Read(ref activationCount));
            Assert.Equal(4, Volatile.Read(ref machineMutationCount));
            Assert.True(Directory.Exists(outputDirectory));
            var pairSnapshotAfterFirst =
                AssertSingleExternalConsumptionAndLocalContinuationPairs(db);
            var journalPath = Path.Combine(
                fixture.Root,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeLocalPromotionFileName);
            var journalBytesAfterFirst = File.ReadAllBytes(journalPath);
            var persisted = JsonSerializer.Deserialize<
                DesignConversionTrainingIntakeLocalPromotionResult>(
                    Assert.Single(File.ReadLines(journalPath)),
                    DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(persisted);
            AssertLocalPromotionReadyAfterAdmittedExternalConsumption(
                persisted!,
                request);

            // Точный повтор должен перечитать текущие доказательства и вернуть
            // сохранённый результат без второй машинной мутации и без нового файла.
            var replay = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        request,
                        options));

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            AssertLocalPromotionReadyAfterAdmittedExternalConsumption(
                replay.Value!,
                request);
            Assert.Equal(
                bool.TrueString,
                replay.Value.BoundaryMetadata[
                    "training_intake_idempotency_replay_hit"]);
            Assert.Equal(
                bool.FalseString,
                replay.Value.BoundaryMetadata[
                    "training_intake_idempotency_created_new_side_effect"]);
            Assert.Equal(1, Volatile.Read(ref providerCount));
            Assert.Equal(2, Volatile.Read(ref runnerCount));
            Assert.Equal(2, Volatile.Read(ref promotionRunnerBoundary));
            Assert.Equal(1, Volatile.Read(ref publicationCount));
            Assert.Equal(1, Volatile.Read(ref activationCount));
            Assert.Equal(4, Volatile.Read(ref machineMutationCount));
            Assert.Equal(journalBytesAfterFirst, File.ReadAllBytes(journalPath));
            Assert.Single(File.ReadLines(journalPath));
            Assert.Equal(pairSnapshotAfterFirst, SnapshotPairStoreFiles(db));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }
    }

    [Fact]
    public async Task LocalPromotionWithCorpusAdmission_CrashAfterExternalCommitExactReplayRecoversWithoutSecondExternalConsumption()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var outputDirectory = Path.Combine(
            fixture.Root,
            "promotion-crash-after-external-commit");
        var options = BuildCorpusPromotionOptions(
            "promotion-crash-after-external-commit");
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] =
                "tenant:crash-after-external-commit",
            ["training_intake_idempotency_key_ref"] =
                "idempotency:crash-after-external-commit"
        };
        var providerCount = 0;
        var runnerCount = 0;
        var promotionRunnerBoundary = 0;
        var publicationCount = 0;
        var activationCount = 0;
        var machineMutationCount = 0;
        var committedIdentityValidationCount = 0;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            Interlocked.Increment(ref providerCount);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                modelSet);
        });
        var crashingDb = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting:
                () => Interlocked.Increment(ref publicationCount),
            runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting:
                () => Interlocked.Increment(ref activationCount),
            runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting:
                () => Interlocked.Increment(ref promotionRunnerBoundary),
            runTrainingIntakeLocalPromotionAfterCommittedExternalIdentityValidationObserverForTesting:
                () =>
                {
                    Interlocked.Increment(ref committedIdentityValidationCount);
                    throw new InvalidOperationException(
                        "test crash after committed external identity");
                },
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        Exception? crash;
        try
        {
            crash = await Record.ExceptionAsync(async () =>
                await DesignConversionTrainingIntakeRouteAdmissionContext
                    .RunAsync(
                        boundary,
                        () => crashingDb.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                            intake.IntakeId,
                            outputDirectory,
                            request,
                            options)));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.IsType<InvalidOperationException>(crash);
        Assert.Equal(1, Volatile.Read(ref committedIdentityValidationCount));
        Assert.Equal(0, Volatile.Read(ref providerCount));
        Assert.Equal(1, Volatile.Read(ref runnerCount));
        Assert.Equal(1, Volatile.Read(ref promotionRunnerBoundary));
        Assert.Equal(0, Volatile.Read(ref publicationCount));
        Assert.Equal(0, Volatile.Read(ref activationCount));
        var mutationCountAfterCrash = Volatile.Read(ref machineMutationCount);
        Assert.True(
            mutationCountAfterCrash > 0,
            "После фиксации внешней транзакции до падения уже должна быть хотя бы одна мутация: иначе тест не проверяет реальное окно восстановления.");
        Assert.False(File.Exists(Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionOperationFileName)));
        Assert.False(File.Exists(Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionFileName)));
        AssertSingleCorpusAdmissionReservation(fixture.Root);
        AssertSingleExternalConsumptionOnly(crashingDb);

        var replayPhases = new List<
            JsonlDesignConversionLocalArtifactDb.LocalPromotionOperationPhase>();
        var replayDb = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting:
                () => Interlocked.Increment(ref publicationCount),
            runTrainingIntakeLocalPromotionActivationInvokedObserverForTesting:
                () => Interlocked.Increment(ref activationCount),
            runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting:
                () => Interlocked.Increment(ref promotionRunnerBoundary),
            runTrainingIntakeLocalPromotionAfterCommittedExternalIdentityValidationObserverForTesting:
                () => Interlocked.Increment(ref committedIdentityValidationCount),
            runTrainingIntakeLocalPromotionOperationPhaseObserverForTesting:
                replayPhases.Add,
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>
            replay;
        try
        {
            replay = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => replayDb.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        request,
                        options));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        AssertLocalPromotionReadyAfterAdmittedExternalConsumption(
            replay.Value!,
            request);
        Assert.Equal(1, Volatile.Read(ref providerCount));
        Assert.Equal(2, Volatile.Read(ref runnerCount));
        Assert.Equal(2, Volatile.Read(ref promotionRunnerBoundary));
        Assert.Equal(1, Volatile.Read(ref publicationCount));
        Assert.Equal(1, Volatile.Read(ref activationCount));
        Assert.True(
            Volatile.Read(ref machineMutationCount) > mutationCountAfterCrash,
            "Точный повтор должен продолжить локальный хвост и дать новую локальную мутацию, но без второго списания внешнего учителя.");
        Assert.Equal(2, Volatile.Read(ref committedIdentityValidationCount));
        Assert.Equal(
            [
                JsonlDesignConversionLocalArtifactDb
                    .LocalPromotionOperationPhase.Prepare,
                JsonlDesignConversionLocalArtifactDb
                    .LocalPromotionOperationPhase.Intent,
                JsonlDesignConversionLocalArtifactDb
                    .LocalPromotionOperationPhase.Prepared,
                JsonlDesignConversionLocalArtifactDb
                    .LocalPromotionOperationPhase.Committed
            ],
            replayPhases);
        AssertSingleCorpusAdmissionReservation(fixture.Root);
        AssertSingleExternalConsumptionAndLocalContinuationPairs(replayDb);
        Assert.Single(File.ReadLines(Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionFileName)));
    }

    [Fact]
    public async Task LocalPromotionWithCorpusAdmission_CancellationAfterExternalCommitExactReplayRecoversWithoutSecondExternalConsumption()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var outputDirectory = Path.Combine(
            fixture.Root,
            "promotion-cancel-after-external-commit");
        var options = BuildCorpusPromotionOptions(
            "promotion-cancel-after-external-commit");
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] =
                "tenant:cancel-after-external-commit",
            ["training_intake_idempotency_key_ref"] =
                "idempotency:cancel-after-external-commit"
        };
        using var cts = new CancellationTokenSource();
        var runnerCount = 0;
        var providerCount = 0;
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            runTrainingIntakeLocalPromotionAfterCommittedExternalIdentityValidationObserverForTesting:
                cts.Cancel,
            dpoPolicyBaseProvider: new DesignConversionDpoPolicyBaseTestProvider(
                modelSet =>
                {
                    Interlocked.Increment(ref providerCount);
                    return OperationResult<DesignConversionDpoPolicyBaseModelSet>
                        .Ok(modelSet);
                }),
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);

        var cancelled = await DesignConversionTrainingIntakeRouteAdmissionContext
            .RunAsync(
                boundary,
                () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                    intake.IntakeId,
                    outputDirectory,
                    request,
                    options,
                    cts.Token));

        Assert.False(cancelled.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPromotedParentContinuation.CancelledErrorCode,
            cancelled.ErrorCode);
        Assert.Equal(0, Volatile.Read(ref providerCount));
        Assert.Equal(1, Volatile.Read(ref runnerCount));
        AssertSingleCorpusAdmissionReservation(fixture.Root);
        AssertSingleExternalConsumptionOnly(db);

        var replayDb = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            dpoPolicyBaseProvider: new DesignConversionDpoPolicyBaseTestProvider(
                modelSet =>
                {
                    Interlocked.Increment(ref providerCount);
                    return OperationResult<DesignConversionDpoPolicyBaseModelSet>
                        .Ok(modelSet);
                }),
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var replay = await DesignConversionTrainingIntakeRouteAdmissionContext
            .RunAsync(
                boundary,
                () => replayDb.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                    intake.IntakeId,
                    outputDirectory,
                    request,
                    options));

        Assert.True(replay.IsSuccess, replay.ErrorMessage);
        AssertLocalPromotionReadyAfterAdmittedExternalConsumption(
            replay.Value!,
            request);
        Assert.Equal(1, Volatile.Read(ref providerCount));
        Assert.Equal(2, Volatile.Read(ref runnerCount));
        AssertSingleCorpusAdmissionReservation(fixture.Root);
        AssertSingleExternalConsumptionAndLocalContinuationPairs(replayDb);
    }

    [Theory]
    [InlineData("tampered-reservation")]
    [InlineData("duplicate-reservation")]
    [InlineData("conflicting-duplicate-reservation")]
    [InlineData("tampered-external-record")]
    [InlineData("tampered-replay-ledger")]
    [InlineData("duplicate-replay-ledger")]
    [InlineData("changed-source-proof")]
    [InlineData("changed-options")]
    [InlineData("changed-current-snapshot")]
    [InlineData("reservation-only-no-committed-carrier")]
    public async Task LocalPromotionWithCorpusAdmission_CrashReplayFailsClosedWhenDurableCarrierOrRequestChanges(
        string mismatchCase)
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var outputDirectory = Path.Combine(
            fixture.Root,
            "promotion-crash-replay-fail-" + mismatchCase);
        var options = BuildCorpusPromotionOptions(
            "promotion-crash-replay-fail-" + mismatchCase);
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] =
                "tenant:crash-replay-fail-" + mismatchCase,
            ["training_intake_idempotency_key_ref"] =
                "idempotency:crash-replay-fail-" + mismatchCase
        };
        var runnerCount = 0;
        var providerCount = 0;
        var machineMutationCount = 0;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            Interlocked.Increment(ref providerCount);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                modelSet);
        });
        var retainTransaction = mismatchCase.StartsWith(
            "retained-transaction-",
            StringComparison.Ordinal);
        var pairStoreRoot = new JsonlDesignConversionLocalArtifactDb(
                fixture.Root,
                legalTeacherAdmissionAuthority: fixture.LegalAuthority)
            .TrainingIntakeDpoPairStoreRootResolutionForTesting
            .PairStoreRootDirectory;
        var injectedPairStore = retainTransaction
            ? new FileDpoPairStore(
                pairStoreRoot,
                afterReplayLedgerWrittenBeforePairCompactionForTesting:
                    () => throw new InvalidOperationException(
                        "test stop after replay ledger before transaction compaction"))
            : null;
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            runTrainingIntakeLocalPromotionRunnerBeforeInvokeObserverForTesting:
                string.Equals(
                    mismatchCase,
                    "reservation-only-no-committed-carrier",
                    StringComparison.Ordinal)
                    ? () => throw new InvalidOperationException(
                        "test crash before committed external carrier")
                    : null,
            runTrainingIntakeLocalPromotionAfterCommittedExternalIdentityValidationObserverForTesting:
                retainTransaction || string.Equals(
                    mismatchCase,
                    "reservation-only-no-committed-carrier",
                    StringComparison.Ordinal)
                    ? null
                    : () => throw new InvalidOperationException(
                        "test crash after committed external carrier"),
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority,
            trainingIntakeDpoPairStoreForTesting: injectedPairStore);
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        try
        {
            var crash = await Record.ExceptionAsync(async () =>
                await DesignConversionTrainingIntakeRouteAdmissionContext
                    .RunAsync(
                        boundary,
                        () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                            intake.IntakeId,
                            outputDirectory,
                            request,
                            options)));
            if (retainTransaction)
                Assert.Null(crash);
            else
                Assert.IsType<InvalidOperationException>(crash);
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        AssertSingleCorpusAdmissionReservation(fixture.Root);
        if (string.Equals(
                mismatchCase,
                "reservation-only-no-committed-carrier",
                StringComparison.Ordinal))
        {
            Assert.Empty(Directory.EnumerateFiles(
                db.TrainingIntakeDpoPairStoreRootResolutionForTesting
                    .PairStoreRootDirectory,
                "*.dpo-external-consumption.json",
                SearchOption.TopDirectoryOnly));
        }
        else if (!retainTransaction)
        {
            AssertSingleExternalConsumptionOnly(db);
        }
        else
        {
            Assert.True(File.Exists(Path.Combine(
                pairStoreRoot,
                ".dpo-replay-ledger.json")));
            Assert.Single(Directory.EnumerateFiles(
                pairStoreRoot,
                "*.dpo-transaction.json",
                SearchOption.TopDirectoryOnly));
        }

        var replayRequest = request;
        var replayOptions = options;
        Func<IReadOnlyList<DesignConversionDpoTrainingRecord>,
            IReadOnlyList<DesignConversionDpoTrainingRecord>>?
            replayRecordsTransformer = null;
        switch (mismatchCase)
        {
            case "tampered-reservation":
                TamperCorpusAdmissionReservation(
                    fixture.Root,
                    "SourceProofSha256",
                    Sha("tampered-corpus-reservation"));
                break;
            case "duplicate-reservation":
                DuplicateCorpusAdmissionReservation(
                    fixture.Root,
                    conflicting: false);
                break;
            case "conflicting-duplicate-reservation":
                DuplicateCorpusAdmissionReservation(
                    fixture.Root,
                    conflicting: true);
                break;
            case "tampered-external-record":
                TamperSingleExternalConsumption(
                    db,
                    "ResultSha256",
                    Sha("tampered-external-consumption-result"));
                break;
            case "tampered-replay-ledger":
                MutateReplayLedger(
                    db,
                    duplicateMatchingEntry: false,
                    tamperResult: true);
                break;
            case "duplicate-replay-ledger":
                MutateReplayLedger(
                    db,
                    duplicateMatchingEntry: true,
                    tamperResult: false);
                break;
            case "retained-transaction-tampered-replay-ledger":
                MutateReplayLedger(
                    db,
                    duplicateMatchingEntry: false,
                    tamperResult: true,
                    pairStoreRoot);
                break;
            case "retained-transaction-duplicate-replay-ledger":
                MutateReplayLedger(
                    db,
                    duplicateMatchingEntry: true,
                    tamperResult: false,
                    pairStoreRoot);
                break;
            case "changed-source-proof":
                var trainRows = fixture.SourceProof.TrainRows.ToArray();
                trainRows[0] = trainRows[0] with
                {
                    Chosen = trainRows[0].Chosen + " tampered"
                };
                replayRequest = new DesignConversionDpoCorpusAdmissionRequest(
                    fixture.Pair,
                    fixture.Receipt,
                    fixture.SourceProof with
                    {
                        TrainRows = trainRows
                    },
                    fixture.ObservedAtUtc);
                break;
            case "changed-options":
                replayOptions = options with
                {
                    DpoPolicyBridgeOptions =
                        new DesignConversionDpoPolicyCheckpointBridgeOptions(
                            MaxBridgePreferenceCount: 2,
                            DpoStepCount: 2)
                        {
                            DpoConvergenceThreshold = 0f,
                            DpoConvergenceWindowSteps = 1
                        }
                };
                break;
            case "changed-current-snapshot":
                replayRecordsTransformer = records =>
                    records.Take(records.Count - 1).ToArray();
                break;
            case "reservation-only-no-committed-carrier":
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(mismatchCase),
                    mismatchCase,
                    "Unknown mismatch case.");
        }

        var pairSnapshotBeforeReplay = SnapshotPairStoreFiles(
            db,
            retainTransaction ? pairStoreRoot : null);
        var runnerCountBeforeReplay = Volatile.Read(ref runnerCount);
        var mutationCountBeforeReplay = Volatile.Read(ref machineMutationCount);
        var providerCountBeforeReplay = Volatile.Read(ref providerCount);
        var replayDb = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority,
            trainingIntakeDpoPairStoreForTesting: retainTransaction
                ? new FileDpoPairStore(pairStoreRoot)
                : null,
            runTrainingIntakeResolvedRecordsTransformerForTesting:
                replayRecordsTransformer);
        previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>
            replay;
        try
        {
            replay = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => replayDb.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        replayRequest,
                        replayOptions));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.False(replay.IsSuccess);
        Assert.Null(replay.Value);
        Assert.Equal(providerCountBeforeReplay, Volatile.Read(ref providerCount));
        Assert.Equal(runnerCountBeforeReplay, Volatile.Read(ref runnerCount));
        Assert.Equal(mutationCountBeforeReplay,
            Volatile.Read(ref machineMutationCount));
        Assert.False(File.Exists(Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionFileName)));
        Assert.Equal(
            pairSnapshotBeforeReplay,
            SnapshotPairStoreFiles(
                replayDb,
                retainTransaction ? pairStoreRoot : null));
    }

    [Theory]
    [InlineData("retained-transaction-tampered-replay-ledger")]
    [InlineData("retained-transaction-duplicate-replay-ledger")]
    public Task LocalPromotionWithCorpusAdmission_RetainedCommittedTransactionAndReplayLedgerMismatchFailsClosed(
        string mismatchCase)
        => LocalPromotionWithCorpusAdmission_CrashReplayFailsClosedWhenDurableCarrierOrRequestChanges(
            mismatchCase);

    [Fact]
    public async Task LocalPromotionWithCorpusAdmission_TamperedReplayReceiptCannotReturnCachedTerminal()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var outputDirectory = Path.Combine(
            fixture.Root,
            "promotion-tampered-corpus-replay");
        var options = BuildCorpusPromotionOptions("promotion-tampered-corpus-replay");
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = "tenant:tampered-corpus-replay",
            ["training_intake_idempotency_key_ref"] =
                "idempotency:tampered-corpus-replay"
        };
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            dpoPolicyBaseProvider: new DesignConversionDpoPolicyBaseTestProvider(),
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var first = await DesignConversionTrainingIntakeRouteAdmissionContext
            .RunAsync(
                boundary,
                () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                    intake.IntakeId,
                    outputDirectory,
                    request,
                    options));
        Assert.True(first.IsSuccess, first.ErrorMessage);
        var journalPath = Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionFileName);
        var journalBytesAfterFirst = File.ReadAllBytes(journalPath);
        var pairSnapshotAfterFirst = SnapshotPairStoreFiles(db);
        var runnerCount = 0;
        var providerCount = 0;
        var machineMutationCount = 0;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            Interlocked.Increment(ref providerCount);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                modelSet);
        });
        var replayDb = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var tamperedRequest = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt with
            {
                ScorerOutputSha256 = Sha("tampered-replay-scorer-output")
            },
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> replay;
        try
        {
            // Сохранённый окончательный результат не разрешает любой похожий
            // запрос: подтверждение независимого сравнения проверяется заново
            // перед возвратом результата.
            replay = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => replayDb.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        tamperedRequest,
                        options));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.False(replay.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
            replay.ErrorCode);
        Assert.Null(replay.Value);
        Assert.Equal(0, Volatile.Read(ref runnerCount));
        Assert.Equal(0, Volatile.Read(ref providerCount));
        Assert.Equal(0, Volatile.Read(ref machineMutationCount));
        Assert.Equal(journalBytesAfterFirst, File.ReadAllBytes(journalPath));
        Assert.Equal(pairSnapshotAfterFirst, SnapshotPairStoreFiles(replayDb));
    }

    [Fact]
    public async Task LocalPromotionWithCorpusAdmission_InvalidSourceProofFailsBeforeRunnerOrArtifacts()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        // Копия доказательства происхождения не получает допуск, выданный
        // построителем исходному набору, даже при похожих полях и отпечатках
        // записей.
        var tamperedTrainRows = fixture.SourceProof.TrainRows
            .Select(static row => row)
            .ToArray();
        tamperedTrainRows[0] = tamperedTrainRows[0] with
        {
            Chosen = tamperedTrainRows[0].Chosen + " tampered"
        };
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof with
            {
                TrainRows = tamperedTrainRows
            },
            fixture.ObservedAtUtc);
        var outputDirectory = Path.Combine(
            fixture.Root,
            "promotion-invalid-source-proof");
        var options = BuildCorpusPromotionOptions("promotion-invalid-source-proof");
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = "tenant:invalid-source-proof",
            ["training_intake_idempotency_key_ref"] =
                "idempotency:invalid-source-proof"
        };
        var runnerCount = 0;
        var providerCount = 0;
        var publicationCount = 0;
        var machineMutationCount = 0;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            Interlocked.Increment(ref providerCount);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                modelSet);
        });
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting:
                () => Interlocked.Increment(ref publicationCount),
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var filesBefore = SnapshotPersistedFiles(fixture.Root);
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> result;
        try
        {
            // Запрос с подтверждением для другого набора нельзя направлять в
            // обычную публикацию: принадлежность подтверждения проверяется до
            // любых изменений.
            result = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        request,
                        options));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, Volatile.Read(ref runnerCount));
        Assert.Equal(0, Volatile.Read(ref providerCount));
        Assert.Equal(0, Volatile.Read(ref publicationCount));
        Assert.Equal(0, Volatile.Read(ref machineMutationCount));
        Assert.False(Directory.Exists(outputDirectory));
        Assert.False(File.Exists(Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionFileName)));
        Assert.Equal(filesBefore, SnapshotPersistedFiles(fixture.Root));
        AssertNoTrainingPairOrExternalConsumptionArtifacts(fixture.Root);
    }

    [Fact]
    public async Task LocalPromotionWithCorpusAdmission_SameObjectSourceProofMutationFailsBeforeRunnerOrCachedReplay()
    {
        using (var fixture = CreateFixture())
        {
            var intake = await SeedRealTrainingIntakeAsync(fixture);
            MutateFirstSourceProofTrainRowInPlace(
                fixture.SourceProof,
                "first-run");
            var request = new DesignConversionDpoCorpusAdmissionRequest(
                fixture.Pair,
                fixture.Receipt,
                fixture.SourceProof,
                fixture.ObservedAtUtc);
            var outputDirectory = Path.Combine(
                fixture.Root,
                "promotion-same-object-source-proof-first-run");
            var options = BuildCorpusPromotionOptions(
                "promotion-same-object-source-proof-first-run");
            var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] =
                    "tenant:same-object-source-proof-first-run",
                ["training_intake_idempotency_key_ref"] =
                    "idempotency:same-object-source-proof-first-run"
            };
            var runnerCount = 0;
            var providerCount = 0;
            var publicationCount = 0;
            var machineMutationCount = 0;
            var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
            {
                Interlocked.Increment(ref providerCount);
                return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                    modelSet);
            });
            var db = new JsonlDesignConversionLocalArtifactDb(
                fixture.Root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref runnerCount),
                runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting:
                    () => Interlocked.Increment(ref publicationCount),
                dpoPolicyBaseProvider: provider,
                legalTeacherAdmissionAuthority: fixture.LegalAuthority);
            var filesBefore = SnapshotPersistedFiles(fixture.Root);
            var previousMutationObserver = DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting;
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                _ => Interlocked.Increment(ref machineMutationCount);
            OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> result;
            try
            {
                // Объект доказательства остался тем же, но строка внутри
                // массива изменилась: защита от повторного запуска проверяет
                // само содержимое, а не только ссылку и отпечаток идентичности
                // записи.
                result = await DesignConversionTrainingIntakeRouteAdmissionContext
                    .RunAsync(
                        boundary,
                        () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                            intake.IntakeId,
                            outputDirectory,
                            request,
                            options));
            }
            finally
            {
                DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                    previousMutationObserver;
            }

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
                result.ErrorCode);
            Assert.Null(result.Value);
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(0, Volatile.Read(ref providerCount));
            Assert.Equal(0, Volatile.Read(ref publicationCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.False(Directory.Exists(outputDirectory));
            Assert.False(File.Exists(Path.Combine(
                fixture.Root,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeLocalPromotionFileName)));
            Assert.Equal(filesBefore, SnapshotPersistedFiles(fixture.Root));
            AssertNoTrainingPairOrExternalConsumptionArtifacts(fixture.Root);
        }

        using (var fixture = CreateFixture())
        {
            var intake = await SeedRealTrainingIntakeAsync(fixture);
            var request = new DesignConversionDpoCorpusAdmissionRequest(
                fixture.Pair,
                fixture.Receipt,
                fixture.SourceProof,
                fixture.ObservedAtUtc);
            var outputDirectory = Path.Combine(
                fixture.Root,
                "promotion-same-object-source-proof-cached-replay");
            var options = BuildCorpusPromotionOptions(
                "promotion-same-object-source-proof-cached-replay");
            var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] =
                    "tenant:same-object-source-proof-cached-replay",
                ["training_intake_idempotency_key_ref"] =
                    "idempotency:same-object-source-proof-cached-replay"
            };
            var firstDb = new JsonlDesignConversionLocalArtifactDb(
                fixture.Root,
                dpoPolicyBaseProvider:
                    new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
                        OperationResult<DesignConversionDpoPolicyBaseModelSet>
                            .Ok(modelSet)),
                legalTeacherAdmissionAuthority: fixture.LegalAuthority);
            var first = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => firstDb.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        request,
                        options));
            Assert.True(first.IsSuccess, first.ErrorMessage);
            AssertLocalPromotionReadyAfterAdmittedExternalConsumption(
                first.Value!,
                request);
            var journalPath = Path.Combine(
                fixture.Root,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeLocalPromotionFileName);
            var journalBytesAfterFirst = File.ReadAllBytes(journalPath);
            var pairSnapshotAfterFirst = SnapshotPairStoreFiles(firstDb);

            MutateFirstSourceProofTrainRowInPlace(
                fixture.SourceProof,
                "cached-replay");
            var runnerCount = 0;
            var providerCount = 0;
            var machineMutationCount = 0;
            var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
            {
                Interlocked.Increment(ref providerCount);
                return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                    modelSet);
            });
            var replayDb = new JsonlDesignConversionLocalArtifactDb(
                fixture.Root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref runnerCount),
                dpoPolicyBaseProvider: provider,
                legalTeacherAdmissionAuthority: fixture.LegalAuthority);
            var previousMutationObserver = DesignConversionTrainingPipeline
                .OptimizerMutationSinkObserverForTesting;
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                _ => Interlocked.Increment(ref machineMutationCount);
            OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> replay;
            try
            {
                // Сохранённый окончательный результат не заменяет повторную
                // проверку доказательства происхождения перед возвратом
                // сохранённого результата.
                replay = await DesignConversionTrainingIntakeRouteAdmissionContext
                    .RunAsync(
                        boundary,
                        () => replayDb.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                            intake.IntakeId,
                            outputDirectory,
                            request,
                            options));
            }
            finally
            {
                DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                    previousMutationObserver;
            }

            Assert.False(replay.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
                replay.ErrorCode);
            Assert.Null(replay.Value);
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(0, Volatile.Read(ref providerCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.Equal(journalBytesAfterFirst, File.ReadAllBytes(journalPath));
            Assert.Equal(pairSnapshotAfterFirst, SnapshotPairStoreFiles(replayDb));
            Assert.Single(File.ReadLines(journalPath));
        }
    }

    [Theory]
    [InlineData("null-proof")]
    [InlineData("null-train-rows")]
    [InlineData("null-held-out-rows")]
    [InlineData("same-object-train-row-null")]
    [InlineData("same-object-held-out-row-null")]
    [InlineData("null-pair")]
    [InlineData("null-receipt")]
    public async Task LocalPromotionWithCorpusAdmission_MalformedRequestShapeFailsInvalidBeforeRunnerOrArtifacts(
        string malformedCase)
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = BuildMalformedCorpusAdmissionRequest(
            fixture,
            malformedCase);
        var outputDirectory = Path.Combine(
            fixture.Root,
            "promotion-malformed-corpus-admission-" + malformedCase);
        var options = BuildCorpusPromotionOptions(
            "promotion-malformed-corpus-admission-" + malformedCase);
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] =
                "tenant:malformed-corpus-admission-" + malformedCase,
            ["training_intake_idempotency_key_ref"] =
                "idempotency:malformed-corpus-admission-" + malformedCase
        };
        var runnerCount = 0;
        var providerCount = 0;
        var publicationCount = 0;
        var machineMutationCount = 0;
        var provider = new DesignConversionDpoPolicyBaseTestProvider(modelSet =>
        {
            Interlocked.Increment(ref providerCount);
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                modelSet);
        });
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                Interlocked.Increment(ref runnerCount),
            runTrainingIntakeLocalPromotionBeforePublicationObserverForTesting:
                () => Interlocked.Increment(ref publicationCount),
            dpoPolicyBaseProvider: provider,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var filesBefore = SnapshotPersistedFiles(fixture.Root);
        var pairStoreBefore = SnapshotPairStoreFiles(db);
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> result;
        try
        {
            // Повреждённая форма внутреннего запроса должна закрыться
            // управляемым отказом до подготовки исполнителя и до изменения файлов.
            result = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        request,
                        options));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, Volatile.Read(ref runnerCount));
        Assert.Equal(0, Volatile.Read(ref providerCount));
        Assert.Equal(0, Volatile.Read(ref publicationCount));
        Assert.Equal(0, Volatile.Read(ref machineMutationCount));
        Assert.False(Directory.Exists(outputDirectory));
        Assert.False(File.Exists(Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionFileName)));
        Assert.Equal(filesBefore, SnapshotPersistedFiles(fixture.Root));
        Assert.Equal(pairStoreBefore, SnapshotPairStoreFiles(db));
        AssertNoTrainingPairOrExternalConsumptionArtifacts(fixture.Root);
    }

    [Fact]
    public async Task LocalPromotionWithCorpusAdmission_NonCorpusIntakeRequestFailsBeforeOrdinaryPromotion()
    {
        using var fixture = CreateFixture();
        var db = new JsonlDesignConversionLocalArtifactDb(
            fixture.Root,
            legalTeacherAdmissionAuthority: fixture.LegalAuthority);
        var intake = await CreateOrdinaryApprovedRuntimeTrainingIntakeForTestingAsync(
            db,
            "non-corpus-local-promotion-request");
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var outputDirectory = Path.Combine(
            fixture.Root,
            "promotion-non-corpus-request");
        var options = BuildCorpusPromotionOptions("promotion-non-corpus-request");
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = "tenant:non-corpus-request",
            ["training_intake_idempotency_key_ref"] =
                "idempotency:non-corpus-request"
        };
        var filesBefore = SnapshotPersistedFiles(fixture.Root);
        var machineMutationCount = 0;
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        OperationResult<DesignConversionTrainingIntakeLocalPromotionResult> result;
        try
        {
            result = await DesignConversionTrainingIntakeRouteAdmissionContext
                .RunAsync(
                    boundary,
                    () => db.RunTrainingIntakeAndPromoteLocalWithCorpusAdmissionAsync(
                        intake.IntakeId,
                        outputDirectory,
                        request,
                        options));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, Volatile.Read(ref machineMutationCount));
        Assert.False(Directory.Exists(outputDirectory));
        Assert.False(File.Exists(Path.Combine(
            fixture.Root,
            JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionFileName)));
        Assert.Equal(filesBefore, SnapshotPersistedFiles(fixture.Root));
    }

    [Fact]
    public async Task RunIdempotentReplay_RevalidatesCorpusAdmissionRequirementBeforeReturningCachedSuccess()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var options = BuildDefaultTrainingIntakeRunOptions(intake) with
        {
            MeasurementGateRequest =
                BuildPassingMeasurementGateRequest("authorityless-run-replay-gate")
        };
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = "tenant:corpus-run-replay",
            ["training_intake_idempotency_key_ref"] = "idempotency:corpus-run-replay"
        };
        var cached = BuildCachedRunReplay(intake, options, boundary);
        var journalPath = Path.Combine(fixture.Root, "training-intake-runs.jsonl");
        File.WriteAllText(
            journalPath,
            JsonSerializer.Serialize(
                cached,
                new JsonSerializerOptions(DesignConversionCheckpointJsonOptions.Standard)
                {
                    WriteIndented = false
                }) + Environment.NewLine,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        var journalBytesBefore = File.ReadAllBytes(journalPath);
        var filesBefore = SnapshotPersistedFiles(fixture.Root);
        var runnerCount = 0;
        var machineMutationCount = 0;
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        OperationResult<DesignConversionTrainingIntakeRunResult> result;
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(
                fixture.Root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref runnerCount),
                legalTeacherAdmissionAuthority: fixture.LegalAuthority);
            result = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: boundary);
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(0, Volatile.Read(ref runnerCount));
        Assert.Equal(0, Volatile.Read(ref machineMutationCount));
        Assert.Equal(journalBytesBefore, File.ReadAllBytes(journalPath));
        Assert.Single(File.ReadLines(journalPath));
        Assert.Equal(filesBefore, SnapshotPersistedFiles(fixture.Root));
        AssertNoTrainingPairOrExternalConsumptionArtifacts(fixture.Root);
    }

    [Fact]
    public async Task RunIdempotentReplay_OrdinaryNonCorpusCachedReplayRemainsCompatible()
    {
        var root = DesignConversionPostEvaluationEligibilityTestFixture
            .CreateHandleBoundTempRoot("dcm-corpus-admission-tests");
        Directory.CreateDirectory(root);
        try
        {
            var setupDb = new JsonlDesignConversionLocalArtifactDb(root);
            var intake = await CreateOrdinaryApprovedRuntimeTrainingIntakeForTestingAsync(
                setupDb,
                "ordinary-run-replay");
            var options = BuildDefaultTrainingIntakeRunOptions(intake) with
            {
                MeasurementGateRequest =
                    BuildPassingMeasurementGateRequest("ordinary-run-replay-gate")
            };
            var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] = "tenant:ordinary-run-replay",
                ["training_intake_idempotency_key_ref"] =
                    "idempotency:ordinary-run-replay"
            };
            var cached = BuildCachedRunReplay(intake, options, boundary);
            var journalPath = Path.Combine(root, "training-intake-runs.jsonl");
            File.WriteAllText(
                journalPath,
                JsonSerializer.Serialize(
                    cached,
                    new JsonSerializerOptions(DesignConversionCheckpointJsonOptions.Standard)
                    {
                        WriteIndented = false
                    }) + Environment.NewLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var journalBytesBefore = File.ReadAllBytes(journalPath);
            var runnerCount = 0;
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref runnerCount),
                legalTeacherAdmissionAuthority:
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());

            var replay = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: boundary);

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.Equal(bool.TrueString,
                replay.Value!.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
            Assert.Equal(bool.FalseString,
                replay.Value.BoundaryMetadata[
                    "training_intake_idempotency_created_new_side_effect"]);
            Assert.True(replay.Value.BoundaryMetadata.ContainsKey(
                "training_intake_run_current_records_snapshot_sha256"));
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(journalBytesBefore, File.ReadAllBytes(journalPath));
            Assert.Single(File.ReadLines(journalPath));
        }
        finally
        {
            DeleteDirectoryWithRetry(root);
        }
    }

    [Fact]
    public async Task RunIdempotentReplay_WithCurrentCorpusAdmissionReplaysOnlyForMatchingReceiptAndSnapshot()
    {
        using var fixture = CreateFixture();
        var intake = await SeedRealTrainingIntakeAsync(fixture);
        var request = new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        var options = BuildDefaultTrainingIntakeRunOptions(intake) with
        {
            MeasurementGateRequest =
                BuildPassingMeasurementGateRequest("lawful-run-replay-gate")
        };
        var boundary = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = "tenant:lawful-corpus-run-replay",
            ["training_intake_idempotency_key_ref"] = "idempotency:lawful-corpus-run-replay"
        };
        var runnerCount = 0;
        var machineMutationCount = 0;
        var previousMutationObserver = DesignConversionTrainingPipeline
            .OptimizerMutationSinkObserverForTesting;
        DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
            _ => Interlocked.Increment(ref machineMutationCount);
        try
        {
            var db = new JsonlDesignConversionLocalArtifactDb(
                fixture.Root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting: () =>
                    Interlocked.Increment(ref runnerCount),
                legalTeacherAdmissionAuthority: fixture.LegalAuthority);
            var journalPath = Path.Combine(fixture.Root, "training-intake-runs.jsonl");
            var corpusReplayMetadata =
                await BuildCurrentCorpusReplayBoundaryMetadataAsync(
                    db,
                    intake,
                    request,
                    options,
                    boundary);
            var cached = BuildCachedRunReplay(
                intake,
                options,
                boundary,
                corpusReplayMetadata);
            File.WriteAllText(
                journalPath,
                JsonSerializer.Serialize(
                    cached,
                    new JsonSerializerOptions(DesignConversionCheckpointJsonOptions.Standard)
                    {
                        WriteIndented = false
                    }) + Environment.NewLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var linesBeforeReplay = File.ReadLines(journalPath)
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            var persisted = JsonSerializer.Deserialize<DesignConversionTrainingIntakeRunResult>(
                Assert.Single(linesBeforeReplay),
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(persisted);
            Assert.True(persisted!.BoundaryMetadata.ContainsKey(
                "training_intake_run_current_records_snapshot_sha256"));
            Assert.True(persisted.BoundaryMetadata.ContainsKey(
                "training_intake_run_current_options_snapshot_sha256"));
            Assert.Equal(
                request.Receipt.ReceiptSha256,
                persisted.BoundaryMetadata["corpus_admission_receipt_sha256"]);
            Assert.False(string.IsNullOrWhiteSpace(
                persisted.BoundaryMetadata["corpus_admission_request_binding_sha256"]));
            Assert.False(string.IsNullOrWhiteSpace(
                persisted.BoundaryMetadata["corpus_admission_source_proof_sha256"]));

            var replay = await db.RunTrainingIntakeWithCorpusAdmissionAsync(
                intake.IntakeId,
                request,
                options,
                admissionBoundaryMetadata: boundary);
            var linesAfterReplay = File.ReadLines(journalPath)
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.Equal(bool.TrueString,
                replay.Value!.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
            Assert.Equal(bool.FalseString,
                replay.Value.BoundaryMetadata["training_intake_idempotency_created_new_side_effect"]);
            Assert.Equal(
                persisted.BoundaryMetadata["training_intake_run_current_records_snapshot_sha256"],
                replay.Value.BoundaryMetadata["training_intake_run_current_records_snapshot_sha256"]);
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.Equal(linesBeforeReplay, linesAfterReplay);

            var staleReceiptRequest = new DesignConversionDpoCorpusAdmissionRequest(
                fixture.Pair,
                fixture.Receipt with { ReceiptSha256 = Sha("stale-receipt") },
                fixture.SourceProof,
                fixture.ObservedAtUtc);
            var staleReceiptReplay =
                await db.RunTrainingIntakeWithCorpusAdmissionAsync(
                    intake.IntakeId,
                    staleReceiptRequest,
                    options,
                    admissionBoundaryMetadata: boundary);

            Assert.False(staleReceiptReplay.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
                staleReceiptReplay.ErrorCode);
            Assert.Null(staleReceiptReplay.Value);
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.Equal(linesBeforeReplay, File.ReadLines(journalPath)
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .ToArray());
            Assert.Single(File.ReadLines(journalPath));

            var staleSourceProofRequest = new DesignConversionDpoCorpusAdmissionRequest(
                fixture.Pair,
                fixture.Receipt,
                fixture.SourceProof with
                {
                    DatasetId = fixture.SourceProof.DatasetId + "-stale"
                },
                fixture.ObservedAtUtc);
            var staleSourceProofReplay =
                await db.RunTrainingIntakeWithCorpusAdmissionAsync(
                    intake.IntakeId,
                    staleSourceProofRequest,
                    options,
                    admissionBoundaryMetadata: boundary);

            Assert.False(staleSourceProofReplay.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
                staleSourceProofReplay.ErrorCode);
            Assert.Null(staleSourceProofReplay.Value);
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.Equal(linesBeforeReplay, File.ReadLines(journalPath)
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .ToArray());
            Assert.Single(File.ReadLines(journalPath));

            var missingSnapshotMetadata = new Dictionary<string, string>(
                corpusReplayMetadata,
                StringComparer.Ordinal);
            missingSnapshotMetadata.Remove(
                "training_intake_run_current_records_snapshot_sha256");
            var missingSnapshotCached = BuildCachedRunReplay(
                intake,
                options,
                boundary,
                missingSnapshotMetadata);
            File.WriteAllText(
                journalPath,
                JsonSerializer.Serialize(
                    missingSnapshotCached,
                    new JsonSerializerOptions(DesignConversionCheckpointJsonOptions.Standard)
                    {
                        WriteIndented = false
                    }) + Environment.NewLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var missingSnapshotReplay =
                await db.RunTrainingIntakeWithCorpusAdmissionAsync(
                    intake.IntakeId,
                    request,
                    options,
                    admissionBoundaryMetadata: boundary);

            Assert.False(missingSnapshotReplay.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
                missingSnapshotReplay.ErrorCode);
            Assert.Null(missingSnapshotReplay.Value);
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.Single(File.ReadLines(journalPath));

            var staleSnapshotMetadata = new Dictionary<string, string>(
                corpusReplayMetadata,
                StringComparer.Ordinal)
            {
                ["training_intake_run_current_records_snapshot_sha256"] =
                    Sha("stale-current-record-snapshot")
            };
            var staleSnapshotCached = BuildCachedRunReplay(
                intake,
                options,
                boundary,
                staleSnapshotMetadata);
            File.WriteAllText(
                journalPath,
                JsonSerializer.Serialize(
                    staleSnapshotCached,
                    new JsonSerializerOptions(DesignConversionCheckpointJsonOptions.Standard)
                    {
                        WriteIndented = false
                    }) + Environment.NewLine,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var staleSnapshotReplay =
                await db.RunTrainingIntakeWithCorpusAdmissionAsync(
                    intake.IntakeId,
                    request,
                    options,
                    admissionBoundaryMetadata: boundary);

            Assert.False(staleSnapshotReplay.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
                staleSnapshotReplay.ErrorCode);
            Assert.Null(staleSnapshotReplay.Value);
            Assert.Equal(0, Volatile.Read(ref runnerCount));
            Assert.Equal(0, Volatile.Read(ref machineMutationCount));
            Assert.Single(File.ReadLines(journalPath));
        }
        finally
        {
            DesignConversionTrainingPipeline.OptimizerMutationSinkObserverForTesting =
                previousMutationObserver;
        }
    }

    [Fact]
    public void WrongRootOrderCustodyOrReceipt_CannotMint()
    {
        using var fixture = CreateFixture();
        var reversed = fixture.Db.IssueDpoCorpusAdmission(
            fixture.Records.Reverse().ToArray(),
            fixture.Capability,
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        Assert.False(reversed.IsSuccess);

        var noCustody = fixture.Db.IssueDpoCorpusAdmission(
            fixture.Records,
            null!,
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        Assert.False(noCustody.IsSuccess);

        var copiedReceipt = fixture.Db.IssueDpoCorpusAdmission(
            fixture.Records,
            fixture.Capability,
            fixture.Pair,
            fixture.Receipt with { ReceiptSha256 = Sha("copied-receipt") },
            fixture.SourceProof,
            fixture.ObservedAtUtc);
        Assert.False(copiedReceipt.IsSuccess);

        var otherRoot = Path.Combine(fixture.Root, "other-artifact-root");
        Directory.CreateDirectory(otherRoot);
        var wrongRoot = new JsonlDesignConversionLocalArtifactDb(otherRoot)
            .IssueDpoCorpusAdmission(
                fixture.Records,
                fixture.Capability,
                fixture.Pair,
                fixture.Receipt,
                fixture.SourceProof,
                fixture.ObservedAtUtc);
        Assert.False(wrongRoot.IsSuccess);
    }

    [Fact]
    public void EvaluatorOrG3Result_IsNotAnAdmissionAuthorityParameter()
    {
        var issue = Assert.Single(
            typeof(DesignConversionDpoCorpusAdmissionAuthority).GetMethods(
                BindingFlags.Instance | BindingFlags.NonPublic),
            static method => method.Name == "Issue");
        Assert.DoesNotContain(issue.GetParameters(), static parameter =>
            parameter.ParameterType.Name.Contains("G3", StringComparison.Ordinal) ||
            parameter.ParameterType.Name.Contains("Evaluator", StringComparison.Ordinal) ||
            parameter.ParameterType.Name.Contains("Result", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PreStampedSelectionMarker_CannotMintCorpusAuthority(bool selected)
    {
        using var fixture = CreateFixture();
        var poisoned = fixture.Records.Select(record => record with
        {
            Provenance = record.Provenance with
            {
                Metadata = new Dictionary<string, string>(
                    record.Provenance.Metadata,
                    StringComparer.Ordinal)
                {
                    ["runtime_teacher_selected_preference"] = selected.ToString()
                }
            }
        }).ToArray();

        var issued = fixture.Db.IssueDpoCorpusAdmission(
            poisoned,
            fixture.Capability,
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);

        Assert.False(issued.IsSuccess);
        Assert.Equal(
            selected
                ? DesignConversionDpoCorpusAdmissionAuthority.ForbiddenSelfLabelErrorCode
                : DesignConversionDpoCorpusAdmissionAuthority.InvalidErrorCode,
            issued.ErrorCode);
    }

    [Fact]
    public void MutableTrainingGradeMetadata_NeverMintsAnOrdinaryDatasetRow()
    {
        using var fixture = CreateFixture();
        var forged = fixture.Records.Select(record => record with
        {
            Provenance = record.Provenance with
            {
                Metadata = new Dictionary<string, string>(
                    record.Provenance.Metadata,
                    StringComparer.Ordinal)
                {
                    ["training_grade"] = bool.TrueString,
                    ["not_training_grade"] = bool.FalseString
                }
            }
        }).Concat(fixture.WitnessRecords).ToArray();
        var preflight = fixture.SourceProof.Manifest.Preflight with
        {
            ProductReady = true,
            TrainingCandidateOnly = false
        };
        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            forged,
            new DesignConversionDpoDatasetOptions(
                "forged-grade",
                2,
                1,
                0.2,
                Preflight: preflight));

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.All(dataset.Value!.Rows,
            static row => Assert.False(row.ValidationOutcome.TrainingGrade));
    }

    private static DesignConversionDpoTrainingRecord CreateCorpusRequirementPolicyRecord(
        string id,
        bool authored,
        bool preferenceProven,
        bool rejectedEvaluated)
    {
        var built = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
        [
            DesignConversionDpoGeneratorTests.CreateExample(
                id,
                DesignConversionDpoUnits.CriticUnit)
        ]);
        Assert.True(built.IsSuccess, built.ErrorMessage);
        var record = Assert.Single(built.Value!.AcceptedRecords);
        return record with
        {
            Provenance = record.Provenance with
            {
                Metadata = new Dictionary<string, string>(
                    record.Provenance.Metadata,
                    StringComparer.Ordinal)
                {
                    ["runtime_teacher_authored_candidate"] = authored.ToString(),
                    ["independent_preference_proven"] = preferenceProven.ToString(),
                    ["runtime_rejected_independently_evaluated"] = rejectedEvaluated.ToString()
                }
            }
        };
    }

    private static DesignConversionTrainingPipelineOptions BuildCorpusPromotionOptions(
        string runId)
        => new(
            runId,
            BatchSize: 2,
            CallMachineLifecycleHook: true,
            RequireDatasetCountGate: true,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                runId,
                MinimumTrainRecords: 2,
                MinimumHeldOutRecords: 1,
                HeldOutRecordFraction: 0.2,
                Preflight: DesignConversionDpoDatasetPreflight
                    .LocalFixtureTrainingCandidateOnly(runId)),
            MeasurementGateRequest: BuildPassingMeasurementGateRequest(
                $"{runId}-gate"));

    private static DesignConversionDpoPolicyCheckpointBridgeOptions
        BuildInvalidCorpusPromotionBridgeOptions(string invalidCase)
        => invalidCase switch
        {
            "max-preference-zero" =>
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 0,
                    DpoStepCount: 1),
            "max-preference-too-large" =>
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount:
                        DesignConversionDpoPolicyCheckpointBridgeOptions
                            .MaxAllowedBridgePreferenceCount + 1,
                    DpoStepCount: 1),
            "dpo-steps-zero" =>
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 1,
                    DpoStepCount: 0),
            "dpo-steps-too-large" =>
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 1,
                    DpoStepCount:
                        DesignConversionDpoPolicyCheckpointBridgeOptions
                            .MaxAllowedDpoStepCount + 1),
            "negative-convergence-threshold" =>
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 1,
                    DpoStepCount: 1)
                {
                    DpoConvergenceThreshold = -1f
                },
            "zero-convergence-window" =>
                new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 1,
                    DpoStepCount: 1)
                {
                    DpoConvergenceWindowSteps = 0
                },
            _ => throw new ArgumentOutOfRangeException(
                nameof(invalidCase),
                invalidCase,
                "Unknown invalid DPO policy bridge options case.")
        };

    private static DesignConversionDpoPolicyCheckpointBridgeOptions
        BuildValidCorpusPromotionBridgeOptions()
        => new(
            MaxBridgePreferenceCount: 2,
            DpoStepCount: 1)
        {
            DpoConvergenceThreshold = 0f,
            DpoConvergenceWindowSteps = 1
        };

    private static DesignConversionMeasurementGateRequest BuildPassingMeasurementGateRequest(
        string gateId)
        => new(
            GateId: gateId,
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 1,
                HeldOutQualityScore: 0.94,
                RuntimeMs: 320,
                CostUsd: 0.25m,
                PeakRamMb: 256,
                EvidenceRefs: [$"ref:{gateId}:heldout:pass"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: true,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs: [$"ref:{gateId}:integrity:pass"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionDpoCorpusAdmissionRequest
        BuildMalformedCorpusAdmissionRequest(
            Fixture fixture,
            string malformedCase)
    {
        return malformedCase switch
        {
            "null-proof" => new DesignConversionDpoCorpusAdmissionRequest(
                fixture.Pair,
                fixture.Receipt,
                null!,
                fixture.ObservedAtUtc),
            "null-train-rows" => new DesignConversionDpoCorpusAdmissionRequest(
                fixture.Pair,
                fixture.Receipt,
                fixture.SourceProof with { TrainRows = null! },
                fixture.ObservedAtUtc),
            "null-held-out-rows" => new DesignConversionDpoCorpusAdmissionRequest(
                fixture.Pair,
                fixture.Receipt,
                fixture.SourceProof with { HeldOutRows = null! },
                fixture.ObservedAtUtc),
            "same-object-train-row-null" => BuildSameObjectNullRowRequest(
                fixture,
                trainRow: true),
            "same-object-held-out-row-null" => BuildSameObjectNullRowRequest(
                fixture,
                trainRow: false),
            "null-pair" => new DesignConversionDpoCorpusAdmissionRequest(
                null!,
                fixture.Receipt,
                fixture.SourceProof,
                fixture.ObservedAtUtc),
            "null-receipt" => new DesignConversionDpoCorpusAdmissionRequest(
                fixture.Pair,
                null!,
                fixture.SourceProof,
                fixture.ObservedAtUtc),
            _ => throw new ArgumentOutOfRangeException(
                nameof(malformedCase),
                malformedCase,
                "Unknown malformed corpus admission request shape.")
        };
    }

    private static DesignConversionDpoCorpusAdmissionRequest
        BuildSameObjectNullRowRequest(
            Fixture fixture,
            bool trainRow)
    {
        if (trainRow)
        {
            var trainRows = Assert.IsType<DesignConversionDpoDatasetRow[]>(
                fixture.SourceProof.TrainRows);
            trainRows[0] = null!;
        }
        else
        {
            var heldOutRows = Assert.IsType<DesignConversionDpoDatasetRow[]>(
                fixture.SourceProof.HeldOutRows);
            heldOutRows[0] = null!;
        }

        // Повреждается список внутри исходного объекта, чтобы проверить форму
        // входа до того, как построитель сверит сохранённый отпечаток набора.
        return new DesignConversionDpoCorpusAdmissionRequest(
            fixture.Pair,
            fixture.Receipt,
            fixture.SourceProof,
            fixture.ObservedAtUtc);
    }

    private static void MutateFirstSourceProofTrainRowInPlace(
        DesignConversionDpoDataset sourceProof,
        string suffix)
    {
        var liveTrainRows =
            Assert.IsType<DesignConversionDpoDatasetRow[]>(
                sourceProof.TrainRows);
        var originalRow = liveTrainRows[0];
        // Подмена сохраняет объект доказательства и отпечаток идентичности
        // записи; обнаружить её позволяет пересчёт полного отпечатка содержимого.
        liveTrainRows[0] = originalRow with
        {
            Chosen = originalRow.Chosen + " tampered-" + suffix
        };
    }

    private static string[] SnapshotPersistedFiles(string root)
        => Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".mutation.lock", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .Select(path => string.Join(
                "|",
                Path.GetRelativePath(root, path),
                Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))))
            .ToArray();

    private static void AssertNoTrainingPairOrExternalConsumptionArtifacts(string root)
    {
        var pairRoot = Path.Combine(
            root,
            JsonlDesignConversionLocalArtifactDb.TrainingIntakeDpoPairStoreDirectoryName);
        if (!Directory.Exists(pairRoot))
            return;

        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-pair.json",
            SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-pair-group.json",
            SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-transaction.json",
            SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-external-consumption.json",
            SearchOption.AllDirectories));
    }

    private static string[] AssertSingleCorpusAdmissionReservation(string root)
    {
        var journalPath = Path.Combine(
            root,
            "design-conversion-dpo-corpus-admission-reservations.v1.jsonl");
        Assert.True(File.Exists(journalPath));
        var rows = File.ReadAllLines(journalPath);
        Assert.Single(rows);
        Assert.Contains("ReceiptSha256", rows[0], StringComparison.Ordinal);
        Assert.Contains("SourceProofSha256", rows[0], StringComparison.Ordinal);
        Assert.Contains(
            "AggregateCommitmentSha256",
            rows[0],
            StringComparison.Ordinal);
        Assert.Contains(
            "RequestBindingSha256",
            rows[0],
            StringComparison.Ordinal);
        return rows;
    }

    private static void TamperCorpusAdmissionReservation(
        string root,
        string propertyName,
        string replacementValue)
    {
        var journalPath = Path.Combine(
            root,
            "design-conversion-dpo-corpus-admission-reservations.v1.jsonl");
        var rows = AssertSingleCorpusAdmissionReservation(root);
        File.WriteAllText(
            journalPath,
            ReplaceJsonStringProperty(
                rows[0],
                propertyName,
                replacementValue) + Environment.NewLine,
            Encoding.UTF8);
    }

    private static void DuplicateCorpusAdmissionReservation(
        string root,
        bool conflicting)
    {
        var journalPath = Path.Combine(
            root,
            "design-conversion-dpo-corpus-admission-reservations.v1.jsonl");
        var rows = AssertSingleCorpusAdmissionReservation(root);
        var duplicate = conflicting
            ? ReplaceJsonStringProperty(
                rows[0],
                "SourceProofSha256",
                Sha("conflicting-duplicate-corpus-reservation"))
            : rows[0];
        File.AppendAllText(
            journalPath,
            duplicate + Environment.NewLine,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void MutateReplayLedger(
        JsonlDesignConversionLocalArtifactDb db,
        bool duplicateMatchingEntry,
        bool tamperResult,
        string? pairStoreRootOverride = null)
    {
        var pairRoot = pairStoreRootOverride ?? db
            .TrainingIntakeDpoPairStoreRootResolutionForTesting
            .PairStoreRootDirectory;
        var ledgerPath = Path.Combine(pairRoot, ".dpo-replay-ledger.json");
        var ledger = JsonNode.Parse(File.ReadAllText(ledgerPath, Encoding.UTF8))!
            .AsObject();
        var entries = ledger["entries"]!.AsArray();
        var entry = Assert.Single(entries)!.AsObject();
        if (duplicateMatchingEntry)
            entries.Add(entry.DeepClone());
        if (tamperResult)
        {
            entry["externalConsumption"]!["resultSha256"] =
                Sha("tampered-retained-replay-result");
        }

        // Хеш считается по той же компактной канонической JSON-форме, но с
        // пустым полем собственного хеша: так тест создаёт валидный по форме,
        // однако неоднозначный или противоречивый долговечный журнал.
        ledger["recordSha256"] = string.Empty;
        var hashInput = Encoding.UTF8.GetBytes(ledger.ToJsonString());
        ledger["recordSha256"] = Convert.ToHexString(SHA256.HashData(hashInput))
            .ToLowerInvariant();
        File.WriteAllText(
            ledgerPath,
            ledger.ToJsonString(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void TamperSingleExternalConsumption(
        JsonlDesignConversionLocalArtifactDb db,
        string propertyName,
        string replacementValue)
    {
        var pairRoot = db.TrainingIntakeDpoPairStoreRootResolutionForTesting
            .PairStoreRootDirectory;
        var external = Assert.Single(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-external-consumption.json",
            SearchOption.TopDirectoryOnly));
        var original = File.ReadAllText(external, Encoding.UTF8);
        File.WriteAllText(
            external,
            ReplaceJsonStringProperty(
                original,
                propertyName,
                replacementValue),
            Encoding.UTF8);
    }

    private static string ReplaceJsonStringProperty(
        string json,
        string propertyName,
        string replacementValue)
    {
        using var document = JsonDocument.Parse(json);
        var property = document.RootElement
            .EnumerateObject()
            .Single(candidate => string.Equals(
                candidate.Name,
                propertyName,
                StringComparison.OrdinalIgnoreCase));
        var original = property.Value.GetString();
        Assert.False(string.IsNullOrWhiteSpace(original));
        var changed = json.Replace(
            JsonSerializer.Serialize(original),
            JsonSerializer.Serialize(replacementValue),
            StringComparison.Ordinal);
        Assert.NotEqual(json, changed);
        return changed;
    }

    private static string[] AssertSingleExternalConsumptionOnly(
        JsonlDesignConversionLocalArtifactDb db)
    {
        var pairStoreResolution =
            db.TrainingIntakeDpoPairStoreRootResolutionForTesting;
        if (pairStoreResolution.UsesNativeWslStateRoot)
        {
            Assert.False(IsUnderDirectory(
                pairStoreResolution.PairStoreRootDirectory,
                db.AllowedArtifactRootDirectory));
            Assert.True(File.Exists(pairStoreResolution.ManifestPath));
            Assert.Empty(EnumeratePairStoreEntries(
                pairStoreResolution.LegacyPairStoreRootDirectory));
        }

        var pairRoot = pairStoreResolution.PairStoreRootDirectory;
        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-pair.json",
            SearchOption.TopDirectoryOnly));
        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-pair-group.json",
            SearchOption.TopDirectoryOnly));
        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-transaction.json",
            SearchOption.TopDirectoryOnly));
        Assert.Single(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-external-consumption.json",
            SearchOption.TopDirectoryOnly));
        return SnapshotPairStoreFiles(db);
    }

    private static string[] SnapshotPairStoreFiles(
        JsonlDesignConversionLocalArtifactDb db,
        string? pairStoreRootOverride = null)
    {
        var pairRoot = pairStoreRootOverride ?? db
            .TrainingIntakeDpoPairStoreRootResolutionForTesting
            .PairStoreRootDirectory;
        return Directory.Exists(pairRoot)
            ? Directory.EnumerateFiles(pairRoot, "*", SearchOption.TopDirectoryOnly)
                .Order(StringComparer.Ordinal)
                .Select(path => string.Join(
                    "|",
                    Path.GetRelativePath(pairRoot, path),
                    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))))
                .ToArray()
            : [];
    }

    private static string[] AssertSingleExternalConsumptionAndLocalContinuationPairs(
        JsonlDesignConversionLocalArtifactDb db)
    {
        var pairStoreResolution = db
            .TrainingIntakeDpoPairStoreRootResolutionForTesting;
        if (pairStoreResolution.UsesNativeWslStateRoot)
        {
            Assert.False(IsUnderDirectory(
                pairStoreResolution.PairStoreRootDirectory,
                pairStoreResolution.LegacyPairStoreRootDirectory));
            Assert.True(File.Exists(pairStoreResolution.ManifestPath));
            Assert.Empty(EnumeratePairStoreEntries(
                pairStoreResolution.LegacyPairStoreRootDirectory));
        }

        var pairRoot = pairStoreResolution.PairStoreRootDirectory;
        Assert.Equal(2, Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-pair.json",
            SearchOption.TopDirectoryOnly).Count());
        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-pair-group.json",
            SearchOption.TopDirectoryOnly));
        Assert.Empty(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-transaction.json",
            SearchOption.TopDirectoryOnly));
        Assert.Single(Directory.EnumerateFiles(
            pairRoot,
            "*.dpo-external-consumption.json",
            SearchOption.TopDirectoryOnly));
        return SnapshotPairStoreFiles(db);
    }

    private static void AssertLocalPromotionReadyAfterAdmittedExternalConsumption(
        DesignConversionTrainingIntakeLocalPromotionResult result,
        DesignConversionDpoCorpusAdmissionRequest request)
    {
        Assert.True(result.TrainingRun.TrainingRunnerInvoked);
        Assert.True(result.TrainingRun.TrainingStarted);
        Assert.True(result.LocalPromotionReady);
        Assert.True(result.TrainingRun.ModelUploadReady);
        Assert.NotNull(result.Publication);
        Assert.NotNull(result.PromotedLocalModel);
        Assert.True(File.Exists(result.PromotedCheckpointPath));
        Assert.True(result.PromotedLocalModel!.LocalOnly);
        Assert.True(result.PromotedLocalModel.CheckpointFreshLoadVerified);
        Assert.True(result.PromotedLocalModel.CheckpointContainsRealTrainedWeights);
        Assert.True(result.PromotedLocalModel.PromotionManifestClaimed);
        Assert.Equal(
            result.PromotedLocalModel.CheckpointSha256,
            Convert.ToHexString(
                SHA256.HashData(File.ReadAllBytes(result.PromotedCheckpointPath)))
                .ToLowerInvariant());
        Assert.Equal(
            DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
            result.Publication!.PublicationKind);
        Assert.Equal(
            DesignConversionPromotionPublicationStatus.NotUploaded,
            result.Publication.PublicationStatus);
        Assert.False(result.Publication.Manifest.ModelServerUploadAttempted);
        Assert.False(result.Publication.Manifest.ModelServerPromotionClaimed);
        Assert.True(result.Publication.Manifest.LocalPromotionReceiptWritten);
        Assert.Equal(
            "local_receipt_only",
            result.Publication.Manifest.PromotionReceiptKind);
        Assert.StartsWith(
            DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix,
            result.Publication.Manifest.PromotionUploadRef,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "local_promotion_blocked_dual_target_teacher_external_consumption_bridge_contract_unavailable",
            result.BlockReasons);
        Assert.DoesNotContain(
            "DESIGN_CONVERSION_DUAL_TARGET_TEACHER_EXTERNAL_CONSUMPTION_COMMITTED_LOCAL_PROMOTION_BRIDGE_UNAVAILABLE",
            result.EvidenceRefs);
        Assert.Contains(
            "DESIGN_CONVERSION_TRAINING_INTAKE_LOCAL_PROMOTION_READY",
            result.EvidenceRefs);
        Assert.Equal(
            bool.TrueString,
            result.BoundaryMetadata[
                "dual_target_teacher_external_consumption_committed"]);
        Assert.Equal(
            "Committed",
            result.BoundaryMetadata[
                "dual_target_teacher_external_consumption_state"]);
        Assert.Equal(
            bool.TrueString,
            result.BoundaryMetadata[
                "dual_target_teacher_external_server_owned_commit_v1"]);
        Assert.Equal(
            "committed_store_minted_child_transfer_to_composite_bridge",
            result.BoundaryMetadata[
                "dual_target_teacher_external_local_promotion_bridge_contract"]);
        Assert.Equal(
            bool.TrueString,
            result.BoundaryMetadata[
                "training_intake_promote_local_no_retrain_claimed"]);
        Assert.Equal(
            bool.FalseString,
            result.BoundaryMetadata[
                "training_intake_promote_local_retry_may_repeat_training"]);
        Assert.Equal(
            request.Receipt.ReceiptSha256,
            result.BoundaryMetadata["corpus_admission_receipt_sha256"]);
        Assert.False(string.IsNullOrWhiteSpace(
            result.BoundaryMetadata[
                "corpus_admission_request_binding_sha256"]));
        Assert.False(string.IsNullOrWhiteSpace(
            result.BoundaryMetadata[
                "corpus_admission_source_proof_sha256"]));
        Assert.False(string.IsNullOrWhiteSpace(
            result.BoundaryMetadata[
                "training_intake_promote_local_current_records_snapshot_sha256"]));
        Assert.False(string.IsNullOrWhiteSpace(
            result.BoundaryMetadata[
                "training_intake_promote_local_current_options_snapshot_sha256"]));
        Assert.DoesNotContain(
            "dual_target_teacher_external_injected_by_admission",
            result.BoundaryMetadata.Keys);
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord>
        ExpandRuntimeCorpusCandidatesForSecondSource(
            IReadOnlyList<DesignConversionDpoTrainingRecord> records)
    {
        var expanded = new List<DesignConversionDpoTrainingRecord>(
            records.Count * 2);
        foreach (var record in records)
        {
            expanded.Add(record);
            if (IsCorpusRequirementRecord(record))
            {
                expanded.Add(CloneCorpusRecordForSecondSource(record));
            }
        }

        return expanded;
    }

    private static DesignConversionDpoTrainingRecord CloneCorpusRecordForSecondSource(
        DesignConversionDpoTrainingRecord record)
    {
        var originalReceipt = ReadMetadata(
            record.Provenance.Metadata,
            "independent_preference_receipt_sha256");
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            ["multi_source_multi_pair_regression"] = bool.TrueString,
            ["independent_preference_receipt_sha256"] =
                Sha(originalReceipt + ":second-source")
        };

        return record with
        {
            RecordId = record.RecordId + "-second-source",
            SourceStructurePayload =
                record.SourceStructurePayload + "\nsecond-source",
            Provenance = record.Provenance with
            {
                SourceRequestId =
                    record.Provenance.SourceRequestId + "-second-source",
                SourceManifestRef =
                    record.Provenance.SourceManifestRef + "-second-source",
                Metadata = metadata
            }
        };
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord>
        ForceOrdinaryDuplicateRequestIdWithoutCorpusMarkers(
            IReadOnlyList<DesignConversionDpoTrainingRecord> records)
    {
        var duplicateRequestId = records
            .Select(static record => record.TeacherProvenance?.RequestId)
            .FirstOrDefault(static requestId =>
                !string.IsNullOrWhiteSpace(requestId));
        if (string.IsNullOrWhiteSpace(duplicateRequestId))
            return records;

        return records
            .Select(record => record.TeacherProvenance is null
                ? RemoveCorpusRequirementMarkers(record)
                : RemoveCorpusRequirementMarkers(record) with
                {
                    TeacherProvenance = record.TeacherProvenance with
                    {
                        RequestId = duplicateRequestId
                    }
                })
            .ToArray();
    }

    private static DesignConversionDpoTrainingRecord RemoveCorpusRequirementMarkers(
        DesignConversionDpoTrainingRecord record)
    {
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal);
        metadata.Remove("runtime_teacher_authored_candidate");
        metadata.Remove("independent_preference_proven");
        metadata.Remove("runtime_rejected_independently_evaluated");
        metadata.Remove("runtime_teacher_selected_preference");

        return record with
        {
            Provenance = record.Provenance with { Metadata = metadata }
        };
    }

    private static bool IsCorpusRequirementRecord(
        DesignConversionDpoTrainingRecord record)
        => IsTrue(ReadMetadata(
               record.Provenance.Metadata,
               "runtime_teacher_authored_candidate")) &&
           IsTrue(ReadMetadata(
               record.Provenance.Metadata,
               "independent_preference_proven")) &&
           IsTrue(ReadMetadata(
               record.Provenance.Metadata,
               "runtime_rejected_independently_evaluated"));

    private static string ReadMetadata(
        IReadOnlyDictionary<string, string> metadata,
        string key)
        => metadata.TryGetValue(key, out var value)
            ? value ?? string.Empty
            : string.Empty;

    private static bool IsTrue(string value)
        => bool.TryParse(value, out var parsed) && parsed;

    private static DesignConversionTrainingIntakeLocalPromotionResult BuildCachedPromotionReplay(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options,
        string outputDirectory,
        IReadOnlyDictionary<string, string> boundary)
    {
        var run = new DesignConversionTrainingIntakeRunResult(
            RunId: options.RunId,
            TrainingIntakeId: intake.IntakeId,
            PreflightId: "cached-corpus-replay-preflight",
            AcceptedCandidateCount: intake.AcceptedCandidateCount,
            ResolvedTrainingRecordCount: intake.AcceptedCandidateCount,
            MayStartTraining: true,
            SourceManifestStatus: "proven",
            SourceDisjointStatus: "proven",
            TrainingRunnerInvoked: true,
            LocalLifecycleHookAttempted: true,
            TrainingStarted: true,
            CheckpointMutated: true,
            CheckpointCandidateCreated: true,
            WeightsProduced: true,
            ModelUploadReady: false,
            TrainingExecutionBoundary: "cached_test_replay",
            TrainingExecutionStatus: "completed",
            RunnerBlockedReason: string.Empty,
            CheckpointStatus: "created",
            CheckpointMutationStatus: "mutated",
            WeightProofStatus: "proven",
            MeasurementGateStatus: "passed",
            PromotionGateStatus: "passed",
            UploadReadinessStatus: "local_only",
            PromotionBlockedReasons: [],
            RunnerEvidence: null,
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal));
        var unmarked = new DesignConversionTrainingIntakeLocalPromotionResult(
            TrainingRun: run,
            LocalPromotionReady: true,
            ExternalCloudUploadAttempted: false,
            PromotionOutputDirectory: outputDirectory,
            PromotedCheckpointPath: Path.Combine(outputDirectory, "cached.bin"),
            Publication: null,
            PromotedLocalModel: null,
            EvidenceRefs: [],
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata: new Dictionary<string, string>(StringComparer.Ordinal));
        var marker = typeof(JsonlDesignConversionLocalArtifactDb).GetMethod(
            "MarkPromoteLocalIdempotencyBoundary",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(marker);
        return Assert.IsType<DesignConversionTrainingIntakeLocalPromotionResult>(
            marker!.Invoke(
                null,
                [
                    unmarked,
                    intake,
                    options,
                    outputDirectory,
                    boundary,
                    false,
                    true,
                    false
                ]));
    }

    private static DesignConversionTrainingIntakeRunResult BuildCachedRunReplay(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options,
        IReadOnlyDictionary<string, string> boundary,
        IReadOnlyDictionary<string, string>? additionalMetadata = null)
    {
        var metadata = new Dictionary<string, string>(boundary, StringComparer.Ordinal)
        {
            ["training_intake_admission_boundary_scope"] =
                "route_admission_plus_local_run_replay_cache_no_auth_token_no_full_tenant_isolation",
            ["training_intake_run_replay_cache_boundary"] =
                "tenant_id_idempotency_key_same_intake_options_run_replay_cache_v1",
            ["training_intake_replay_cache_checked"] = bool.TrueString,
            ["training_intake_idempotency_replay_hit"] = bool.FalseString,
            ["training_intake_idempotency_created_new_side_effect"] = bool.TrueString,
            ["training_intake_idempotent_replay_result"] = bool.FalseString,
            ["training_intake_idempotency_side_effect_boundary"] =
                "run_training_intake_append_only_once",
            ["training_intake_run_idempotency_payload_hash"] =
                BuildRunTrainingIntakeIdempotencyPayloadHash(intake, options),
            ["training_intake_run_recorded_at_utc"] =
                DateTimeOffset.UtcNow.ToString("O")
        };
        foreach (var pair in additionalMetadata ??
                 new Dictionary<string, string>(StringComparer.Ordinal))
        {
            metadata[pair.Key] = pair.Value;
        }

        return new DesignConversionTrainingIntakeRunResult(
            RunId: options.RunId,
            TrainingIntakeId: intake.IntakeId,
            PreflightId: "cached-corpus-run-replay-preflight",
            AcceptedCandidateCount: intake.AcceptedCandidateCount,
            ResolvedTrainingRecordCount: intake.AcceptedCandidateCount,
            MayStartTraining: true,
            SourceManifestStatus: "proven",
            SourceDisjointStatus: "proven",
            TrainingRunnerInvoked: true,
            LocalLifecycleHookAttempted: true,
            TrainingStarted: true,
            CheckpointMutated: true,
            CheckpointCandidateCreated: true,
            WeightsProduced: true,
            ModelUploadReady: false,
            TrainingExecutionBoundary: "cached_test_replay",
            TrainingExecutionStatus: "completed",
            RunnerBlockedReason: string.Empty,
            CheckpointStatus: "created",
            CheckpointMutationStatus: "mutated",
            WeightProofStatus: "proven",
            MeasurementGateStatus: "passed",
            PromotionGateStatus: "passed",
            UploadReadinessStatus: "local_only",
            PromotionBlockedReasons: [],
            RunnerEvidence: null,
            Warnings: [],
            BlockReasons: [],
            BoundaryMetadata: metadata);
    }

    private static async Task<IReadOnlyDictionary<string, string>>
        BuildCurrentCorpusReplayBoundaryMetadataAsync(
            JsonlDesignConversionLocalArtifactDb db,
            DesignConversionTrainingIntakeRecord intake,
            DesignConversionDpoCorpusAdmissionRequest request,
            DesignConversionTrainingPipelineOptions options,
            IReadOnlyDictionary<string, string> boundary)
    {
        var preflight = await db.RunTrainingIntakePreflightAsync(intake.IntakeId);
        Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
        var resolved = await ResolveTrainingIntakeRecordsForTestingAsync(db, intake);
        var recordsSnapshot =
            DesignConversionTrainingPipeline.SnapshotTrainingRecords(resolved.Records);
        Assert.True(recordsSnapshot.IsSuccess, recordsSnapshot.ErrorMessage);
        var optionsSnapshot = DesignConversionTrainingPipeline.SnapshotOptions(options);
        Assert.True(optionsSnapshot.IsSuccess, optionsSnapshot.ErrorMessage);
        var currentMetadata = new Dictionary<string, string>(
            boundary,
            StringComparer.Ordinal)
        {
            ["training_intake_run_current_records_snapshot_sha256"] =
                BuildTrainingRecordsSnapshotSha256(recordsSnapshot.Value!),
            ["training_intake_run_current_options_snapshot_sha256"] =
                DesignConversionDpoGenerator.Sha256Ref(
                    JsonSerializer.Serialize(optionsSnapshot.Value!))
        };
        var requestBinding = await BuildCorpusAdmissionRequestBindingForTestingAsync(
            db,
            intake,
            preflight.Value!,
            optionsSnapshot.Value!,
            currentMetadata,
            recordsSnapshot.Value!,
            resolved.CustodyEvidence);
        var sourceProofSha256 = BuildCurrentCorpusSourceProofSha256(
            request,
            recordsSnapshot.Value!);

        currentMetadata["corpus_admission_request_binding_sha256"] = requestBinding;
        currentMetadata["corpus_admission_receipt_sha256"] =
            request.Receipt.ReceiptSha256;
        currentMetadata["corpus_admission_source_proof_sha256"] =
            sourceProofSha256;
        currentMetadata["corpus_admission_reservation"] =
            "durable_fail_closed_before_mutation";
        return currentMetadata;
    }

    private static async Task<(
        IReadOnlyList<DesignConversionDpoTrainingRecord> Records,
        IReadOnlyList<DesignConversionCandidatePayloadCustodyStore.CommittedEvidence>
            CustodyEvidence)> ResolveTrainingIntakeRecordsForTestingAsync(
            JsonlDesignConversionLocalArtifactDb db,
            DesignConversionTrainingIntakeRecord intake)
    {
        var method = typeof(JsonlDesignConversionLocalArtifactDb).GetMethod(
            "ResolveTrainingIntakeRecordsAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(
            method!.Invoke(db, [intake, CancellationToken.None]));
        await task;
        var result = task.GetType().GetProperty("Result")!.GetValue(task)!;
        var isSuccess = (bool)result.GetType()
            .GetProperty("IsSuccess")!
            .GetValue(result)!;
        var errorMessage = (string?)result.GetType()
            .GetProperty("ErrorMessage")!
            .GetValue(result);
        Assert.True(isSuccess, errorMessage);
        var value = result.GetType().GetProperty("Value")!.GetValue(result)!;
        var records = Assert.IsAssignableFrom<
            IReadOnlyList<DesignConversionDpoTrainingRecord>>(
            value.GetType().GetProperty("Records")!.GetValue(value));
        var custodyEvidence = Assert.IsAssignableFrom<IReadOnlyList<
            DesignConversionCandidatePayloadCustodyStore.CommittedEvidence>>(
            value.GetType().GetProperty("CustodyEvidence")!.GetValue(value));

        return (records, custodyEvidence);
    }

    private static async Task<DesignConversionTrainingIntakeRecord>
        CreateOrdinaryApprovedRuntimeTrainingIntakeForTestingAsync(
            JsonlDesignConversionLocalArtifactDb db,
            string id)
    {
        var method = typeof(DesignConversionDpoStorageTests).GetMethod(
            "CreateOrdinaryApprovedRuntimeTrainingIntakeAsync",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types:
            [
                typeof(JsonlDesignConversionLocalArtifactDb),
                typeof(string)
            ],
            modifiers: null);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(
            method!.Invoke(null, [db, id]));
        await task;
        var fixture = task.GetType().GetProperty("Result")!.GetValue(task)!;
        return Assert.IsType<DesignConversionTrainingIntakeRecord>(
            fixture.GetType().GetProperty("Intake")!.GetValue(fixture));
    }

    private static async Task<string>
        BuildCorpusAdmissionRequestBindingForTestingAsync(
            JsonlDesignConversionLocalArtifactDb db,
            DesignConversionTrainingIntakeRecord intake,
            DesignConversionTrainingIntakePreflightResult preflight,
            DesignConversionTrainingPipelineOptions options,
            IReadOnlyDictionary<string, string> boundary,
            IReadOnlyList<DesignConversionDpoTrainingRecord> records,
            IReadOnlyList<DesignConversionCandidatePayloadCustodyStore.CommittedEvidence>
                custodyEvidence)
    {
        var method = typeof(JsonlDesignConversionLocalArtifactDb).GetMethod(
            "BuildCorpusAdmissionRequestBindingAsync",
            BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        var task = Assert.IsAssignableFrom<Task>(
            method!.Invoke(
                db,
                [
                    intake,
                    preflight,
                    options,
                    boundary,
                    records,
                    custodyEvidence,
                    CancellationToken.None
                ]));
        await task;
        var result = Assert.IsType<OperationResult<string>>(
            task.GetType().GetProperty("Result")!.GetValue(task));
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static string BuildCurrentCorpusSourceProofSha256(
        DesignConversionDpoCorpusAdmissionRequest request,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records)
    {
        var snapshotByIdentity = records.ToDictionary(
            DesignConversionDpoDatasetBuilder.ComputeRecordIdentityHash,
            StringComparer.Ordinal);
        var orderedPair = request.CanonicalSourceProof.TrainRows
            .Select(row => snapshotByIdentity[row.RecordIdentityHash])
            .OrderBy(static record => string.Equals(
                    record.UnitId,
                    DesignConversionDpoUnits.CriticUnit,
                    StringComparison.Ordinal)
                ? 0
                : 1)
            .ToArray();
        Assert.True(DesignConversionDpoDatasetBuilder.TryAuthorizeCanonicalSourceProof(
            request.CanonicalSourceProof,
            orderedPair,
            out var sourceProofSha256));
        return sourceProofSha256;
    }

    private static string BuildTrainingRecordsSnapshotSha256(
        IReadOnlyList<DesignConversionDpoTrainingRecord> records)
        => DesignConversionDpoGenerator.Sha256Ref(string.Join(
            "|",
            records
                .Select(DesignConversionDpoDatasetBuilder.ComputeRecordIdentityHash)
                .Order(StringComparer.Ordinal)));

    private static string BuildRunTrainingIntakeIdempotencyPayloadHash(
        DesignConversionTrainingIntakeRecord intake,
        DesignConversionTrainingPipelineOptions options)
    {
        var method = typeof(JsonlDesignConversionLocalArtifactDb).GetMethod(
            "BuildRunTrainingIntakeIdempotencyPayloadHash",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsType<string>(method!.Invoke(null, [intake, options]));
    }

    private static DesignConversionTrainingPipelineOptions
        BuildDefaultTrainingIntakeRunOptions(
            DesignConversionTrainingIntakeRecord intake)
    {
        var method = typeof(JsonlDesignConversionLocalArtifactDb).GetMethod(
            "BuildDefaultTrainingIntakeRunOptions",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsType<DesignConversionTrainingPipelineOptions>(
            method!.Invoke(null, [intake]));
    }

    private static string FindRepositoryRootFromContainedPath(string path)
    {
        var current = new DirectoryInfo(Path.GetFullPath(path));
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate repository root above corpus admission fixture root.");
    }

    private static bool IsUnderDirectory(string candidate, string parent)
    {
        var fullCandidate = EnsureTrailingSeparator(Path.GetFullPath(candidate));
        var fullParent = EnsureTrailingSeparator(Path.GetFullPath(parent));
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return fullCandidate.Length > fullParent.Length &&
               fullCandidate.StartsWith(fullParent, comparison);
    }

    private static string EnsureTrailingSeparator(string path)
        => Path.EndsInDirectorySeparator(path)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static IReadOnlyList<string> EnumeratePairStoreEntries(string root)
        => Directory.Exists(root)
            ? Directory.EnumerateFileSystemEntries(root).ToArray()
            : [];

    internal static Fixture CreateFixture()
    {
        var root = DesignConversionPostEvaluationEligibilityTestFixture
            .CreateHandleBoundTempRoot("dcm-corpus-admission-tests");
        Directory.CreateDirectory(root);
        var observedAtUtc = DateTimeOffset.UtcNow;
        var pair = CreatePair("corpus-" + Guid.NewGuid().ToString("N"));
        var db = new JsonlDesignConversionLocalArtifactDb(
            root,
            legalTeacherAdmissionAuthority: CreateLegalAuthority(pair));
        var rootBinding = DesignConversionIndependentPreferenceEvidenceAssociation
            .BuildArtifactRootBinding(db);
        var receipt = DesignConversionIndependentPreferenceEvidenceTestIssuer.Issue(
            pair,
            rootBinding,
            observedAtUtc);
        var built = new DesignConversionDpoGenerator()
            .BuildRuntimeTeacherAuthoredTrainingCandidates(
                [pair],
                [receipt],
                rootBinding,
                observedAtUtc);
        Assert.True(built.IsSuccess, built.ErrorMessage);
        Assert.Equal(2, built.Value!.AcceptedRecords.Count);
        var approved = built.Value.AcceptedRecords
            .Select(record => WithRuntimeArtifactProofRefs(
                record,
                $"corpus-{record.UnitId}"))
            .Select(record =>
        {
            var reviewerRef = Sha("corpus-reviewer");
            var metadata = new Dictionary<string, string>(
                record.Provenance.Metadata,
                StringComparer.Ordinal)
            {
                ["approval_status"] = "approved",
                ["review_status"] = JsonlDesignConversionLocalArtifactDb
                    .RuntimeCandidateReviewApprovedStatus,
                ["approved_for_training"] = bool.TrueString,
                ["approved_by_ref"] = reviewerRef,
                ["approval_reason"] = "independent_pair_approved",
                ["approved_at_utc"] = observedAtUtc.ToString("O")
            };
            var licenseMetadata = new Dictionary<string, string>(
                record.License.Metadata,
                StringComparer.Ordinal)
            {
                ["approval_status"] = "approved",
                ["review_status"] = JsonlDesignConversionLocalArtifactDb
                    .RuntimeCandidateReviewApprovedStatus,
                ["approved_for_training"] = bool.TrueString,
                ["approved_by_ref"] = reviewerRef,
                ["approval_reason"] = "independent_pair_approved",
                ["approved_at_utc"] = observedAtUtc.ToString("O")
            };
            return record with
            {
                Provenance = record.Provenance with
                {
                    ReviewerRef = reviewerRef,
                    Metadata = metadata
                },
                License = record.License with { Metadata = licenseMetadata }
            };
        }).OrderBy(static record =>
            string.Equals(record.UnitId, DesignConversionDpoUnits.CriticUnit,
                StringComparison.Ordinal) ? 0 : 1).ToArray();

        var entries = approved.Select(static record =>
        {
            var secret = Assert.IsType<DesignConversionDpoPayloadCustodySecret>(
                record.PayloadCustodySecret);
            return new DesignConversionCandidatePayloadCustodyEntry(
                record.RecordId,
                secret.TrainingRecordBindingSha256,
                secret.GovernanceSha256,
                secret.PromptSha256,
                secret.ChosenResponseSha256,
                secret.RejectedResponseSha256,
                secret);
        }).ToArray();
        var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
            "authority:corpus-admission",
            Sha("corpus-authority"),
            entries);
        Assert.True(draft.IsSuccess, draft.ErrorMessage);
        var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
            draft.Value!,
            Sha("corpus-authority-fingerprint"));
        Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
        var store = new DesignConversionCandidatePayloadCustodyStore(
            Path.Combine(root, "corpus-custody"));
        var prepared = store.Prepare(draft.Value!, commitment.Value!);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var evidence = store.Commit(prepared.Value!);
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);
        var envelope = store.ResolveSecretEnvelope(evidence.Value!);
        Assert.True(envelope.IsSuccess, envelope.ErrorMessage);
        approved = approved.Select(record =>
        {
            var entry = DesignConversionCandidatePayloadCustodyStore.FindExactEntry(
                envelope.Value!,
                evidence.Value!,
                record.PayloadCustodySecret!.TrainingRecordBindingSha256);
            Assert.NotNull(entry);
            return record with { PayloadCustodySecret = entry!.Secret };
        }).ToArray();
        var capability = DesignConversionCandidatePayloadCustodyStore
            .MintTrainingCapability([evidence.Value!]);
        Assert.True(capability.IsSuccess, capability.ErrorMessage);

        Assert.All(approved, record => Assert.True(
            DesignConversionTrainingMeasurementGatePolicy
                .IsRecordAllowedForTrainingCandidate(record),
            string.Join(",", DesignConversionTrainingMeasurementGatePolicy
                .GetRecordBlockReasons(record))));
        var pairRecordHashes = approved
            .Select(DesignConversionDpoDatasetBuilder.ComputeRecordIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        DesignConversionDpoTrainingRecord? witness = null;
        DesignConversionDpoDataset? sourceProof = null;
        for (var attempt = 0; attempt < 10_000 && sourceProof is null; attempt++)
        {
            var example = DesignConversionDpoGeneratorTests.CreateExample(
                Sha("corpus-heldout-" + attempt),
                DesignConversionDpoUnits.CriticUnit) with
            {
                Provenance = DesignConversionDpoGeneratorTests.CreateExample(
                    Sha("corpus-heldout-" + attempt),
                    DesignConversionDpoUnits.CriticUnit).Provenance with
                {
                    SourceManifestRef = $"manifest://corpus-heldout/{attempt}",
                    Metadata = new Dictionary<string, string>
                    {
                        ["split"] = "heldout"
                    }
                },
                TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                    "corpus-heldout")
            };
            var candidate = new DesignConversionDpoGenerator()
                .BuildTrainReadyRecords([example]);
            Assert.True(candidate.IsSuccess, candidate.ErrorMessage);
            var nextWitness = WithRuntimeArtifactProofRefs(
                Assert.Single(candidate.Value!.AcceptedRecords),
                $"corpus-heldout-{attempt}");
            var witnessReviewerRef = Sha("corpus-heldout-reviewer");
            nextWitness = nextWitness with
            {
                Provenance = nextWitness.Provenance with
                {
                    ReviewerRef = witnessReviewerRef,
                    Metadata = new Dictionary<string, string>(
                        nextWitness.Provenance.Metadata,
                        StringComparer.Ordinal)
                    {
                        ["runtime_created_dpo_candidate"] = bool.TrueString,
                        ["review_required"] = bool.TrueString,
                        ["approval_required"] = bool.TrueString,
                        ["approval_status"] = "approved",
                        ["review_status"] = JsonlDesignConversionLocalArtifactDb
                            .RuntimeCandidateReviewApprovedStatus,
                        ["approved_for_training"] = bool.TrueString,
                        ["approved_by_ref"] = witnessReviewerRef,
                        ["approval_reason"] = "heldout_witness_approved",
                        ["approved_at_utc"] = observedAtUtc.ToString("O")
                    }
                },
                License = nextWitness.License with
                {
                    Metadata = new Dictionary<string, string>(
                        nextWitness.License.Metadata,
                        StringComparer.Ordinal)
                    {
                        ["approval_status"] = "approved",
                        ["review_status"] = JsonlDesignConversionLocalArtifactDb
                            .RuntimeCandidateReviewApprovedStatus,
                        ["approved_for_training"] = bool.TrueString,
                        ["approved_by_ref"] = witnessReviewerRef,
                        ["approval_reason"] = "heldout_witness_approved",
                        ["approved_at_utc"] = observedAtUtc.ToString("O")
                    }
                }
            };
            var proof = new DesignConversionDpoDatasetBuilder().BuildDataset(
                approved.Append(nextWitness).ToArray(),
                new DesignConversionDpoDatasetOptions(
                    "corpus-source-proof",
                    2,
                    1,
                    0.2,
                    Preflight: DesignConversionDpoDatasetPreflight
                        .LocalFixtureTrainingCandidateOnly(
                            "corpus-source-proof-fixture")));
            Assert.True(proof.IsSuccess, proof.ErrorMessage);
            if (proof.Value!.Manifest.CountGate.Passed &&
                proof.Value.TrainRows.Count == 2 &&
                proof.Value.TrainRows
                    .Select(static row => row.RecordIdentityHash)
                    .ToHashSet(StringComparer.Ordinal)
                    .SetEquals(pairRecordHashes))
            {
                witness = nextWitness;
                sourceProof = proof.Value;
            }
        }
        Assert.NotNull(witness);
        Assert.NotNull(sourceProof);

        return new Fixture(
            root,
            db,
            pair,
            receipt,
            approved,
            [witness!],
            capability.Value!,
            sourceProof!,
            observedAtUtc,
            CreateLegalAuthority(pair));
    }

    internal static async Task<DesignConversionTrainingIntakeRecord>
        SeedRealTrainingIntakeAsync(Fixture fixture)
    {
        var method = typeof(DesignConversionDpoStorageTests).GetMethod(
            "BuildArtifactRecord",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        var pendingPair = fixture.Records.Select(record =>
        {
            var provenanceMetadata = new Dictionary<string, string>(
                record.Provenance.Metadata,
                StringComparer.Ordinal)
            {
                ["approval_status"] = "pending",
                ["review_status"] = JsonlDesignConversionLocalArtifactDb
                    .RuntimeCandidateReviewPendingStatus,
                ["approved_for_training"] = bool.FalseString,
                ["review_required"] = bool.TrueString,
                ["approval_required"] = bool.TrueString
            };
            provenanceMetadata.Remove("approved_by_ref");
            provenanceMetadata.Remove("approval_reason");
            provenanceMetadata.Remove("approved_at_utc");
            var licenseMetadata = new Dictionary<string, string>(
                record.License.Metadata,
                StringComparer.Ordinal)
            {
                ["approval_status"] = "pending",
                ["review_status"] = JsonlDesignConversionLocalArtifactDb
                    .RuntimeCandidateReviewPendingStatus,
                ["approved_for_training"] = bool.FalseString
            };
            licenseMetadata.Remove("approved_by_ref");
            licenseMetadata.Remove("approval_reason");
            licenseMetadata.Remove("approved_at_utc");
            return record with
            {
                Provenance = record.Provenance with
                {
                    Metadata = provenanceMetadata
                },
                License = record.License with { Metadata = licenseMetadata }
            };
        }).ToArray();
        var records = pendingPair.Concat(fixture.WitnessRecords).ToArray();
        var localRecord = Assert.IsType<DesignConversionLocalArtifactDbRecord>(
            method!.Invoke(null,
            [
                "corpus-real-intake-record",
                "corpus-real-intake-job",
                "corpus-real-intake-request",
                DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                records,
                "<main>corpus admission test</main>",
                ".corpus-admission-test{}"
            ]));
        localRecord = localRecord with
        {
            SanitizedSourceRefs = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source_manifest_ref"] = "manifest://corpus-real-intake",
                ["source_kind"] = "figma",
                ["design_tree_payload_ref"] = Sha("corpus-design-tree"),
                ["original_tree_payload_ref"] = Sha("corpus-original-tree"),
                ["source_artifact:expected_desktop"] = Sha("corpus-expected"),
                ["source_artifact:actual_desktop"] = Sha("corpus-actual")
            }
        };
        var stored = await DesignConversionPostEvaluationEligibilityTestFixture
            .AppendEligibleRecordAsync(fixture.Db, localRecord);
        foreach (var record in fixture.Records)
        {
            var approval = await fixture.Db.ApproveRuntimeCreatedDpoCandidateAsync(
                stored.RecordId,
                record.RecordId,
                record.Provenance.Metadata["approved_by_ref"],
                record.Provenance.Metadata["approval_reason"],
                fixture.ObservedAtUtc);
            Assert.True(approval.IsSuccess, approval.ErrorMessage);
        }
        var handoff = await fixture.Db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(JobId: "corpus-real-intake-job"),
            fixture.ObservedAtUtc);
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(3, handoff.Value!.Rows.Count);
        var intake = await fixture.Db.CreateTrainingIntakeAsync(
            handoff.Value,
            "requester:corpus-admission",
            "exact server-owned corpus admission",
            fixture.ObservedAtUtc);
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        return intake.Value!;
    }

    private static DesignConversionTeacherAuthoredEvaluationPair CreatePair(string suffix)
    {
        // Прямой вызов сохраняет прежние настройки тестовой пары.
        // Компилятор проверяет аргументы общего вспомогательного метода, поэтому
        // несовместимое изменение обнаружится при сборке, а не до проверки допуска обучающих пар.
        return DesignConversionLocalIndependentPreferenceEvaluatorTests.CreatePair(
            suffix,
            generatedIsGood: true,
            neutralizeQualityMetadata: false);
    }

    private static DesignConversionDpoTrainingRecord WithRuntimeArtifactProofRefs(
        DesignConversionDpoTrainingRecord record,
        string id)
    {
        var method = typeof(DesignConversionDpoStorageTests).GetMethod(
            "WithRuntimeArtifactProofRefs",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return Assert.IsType<DesignConversionDpoTrainingRecord>(
            method!.Invoke(null, [record, id]));
    }

    private static string Sha(string value)
        => "sha256:" + Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateLegalAuthority(DesignConversionTeacherAuthoredEvaluationPair pair)
        => DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(
            modelId: pair.AuthoringEvidence.Provenance.ModelId,
            expectedOllamaManifestDigest:
                pair.AuthoringEvidence.Provenance.ModelVersion,
            expectedOllamaModelLayerDigest:
                pair.AuthoringEvidence.Provenance.ModelArtifactDigest);

    internal sealed record Fixture(
        string Root,
        JsonlDesignConversionLocalArtifactDb Db,
        DesignConversionTeacherAuthoredEvaluationPair Pair,
        DesignConversionIndependentPreferenceEvidence Receipt,
        IReadOnlyList<DesignConversionDpoTrainingRecord> Records,
        IReadOnlyList<DesignConversionDpoTrainingRecord> WitnessRecords,
        DesignConversionCandidatePayloadCustodyStore.TrainingCapability Capability,
        DesignConversionDpoDataset SourceProof,
        DateTimeOffset ObservedAtUtc,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority LegalAuthority) : IDisposable
    {
        public void Dispose()
        {
            DeleteDirectoryWithRetry(Root);
        }
    }

    private static void DeleteDirectoryWithRetry(string root)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 4)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(50);
            }
            catch (UnauthorizedAccessException) when (attempt < 4)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Thread.Sleep(50);
            }
        }
    }
}
