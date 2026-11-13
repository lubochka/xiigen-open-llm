// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionMachineGradientBridgeTests
{
    private const string SourceHash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact]
    public async Task TrainBatch_ConfiguredBridgeUsesOnlyValidatedBridgeUpdatesAndSanitizedInput()
    {
        var bridge = new RecordingGradientBridge((request, _) => Task.FromResult(
            OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(new(
                request.BatchIdentityHash,
                request.DomainTag,
                request.InputMachineVersion,
                AverageRewardMargin: 0.42,
                Updates:
                [
                    new DesignConversionMachineGradientBridgeUpdate(
                        DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                        "source_kind:figma",
                        0.125)
                ]))));
        var machine = CreateBridgeMachine(bridge);
        var rawBatchId = @"C:\Users\bridge-owner\private\run-token=PA4-SECRET";
        var batch = BuildGradientBatch(rawBatchId);

        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, batch);

        Assert.True(train.IsSuccess, train.ErrorMessage);
        Assert.Equal(1, bridge.CallCount);
        var request = Assert.IsType<DesignConversionMachineGradientBridgeRequest>(bridge.LastRequest);
        var expectedBatchIdentityHash = IndependentlyHashBatchIdentity(rawBatchId);
        Assert.NotEqual(rawBatchId, request.BatchIdentityHash);
        Assert.Equal(expectedBatchIdentityHash, request.BatchIdentityHash);
        AssertCanonicalHash(request.BatchIdentityHash);
        Assert.Equal(DesignConversionMachine.DomainTag, request.DomainTag);
        Assert.Equal("0.1.0", request.InputMachineVersion);
        Assert.NotEmpty(request.SanitizedSignals);
        Assert.All(request.SanitizedSignals, signal =>
        {
            Assert.Equal(DesignConversionRuntimeScaffoldCriticUnit.UnitId, signal.CanonicalUnitId);
            Assert.False(string.IsNullOrWhiteSpace(signal.AllowedFeatureKey));
            Assert.True(double.IsFinite(signal.SignalValue));
        });
        var serializedRequest = JsonSerializer.Serialize(request);
        Assert.DoesNotContain("Users", serializedRequest, StringComparison.Ordinal);
        Assert.DoesNotContain("private", serializedRequest, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("run-token", serializedRequest, StringComparison.Ordinal);
        Assert.DoesNotContain("PA4-SECRET", serializedRequest, StringComparison.Ordinal);
        Assert.DoesNotContain("do-not-expose", serializedRequest, StringComparison.Ordinal);
        Assert.DoesNotContain("raw-source-id", serializedRequest, StringComparison.Ordinal);

        Assert.Equal(0.42, train.Value!.RewardMargin);
        var checkpoint = Export(machine);
        var persistedWeight = Assert.Single(checkpoint.CriticUnit.LearnedWeights ?? []);
        Assert.Equal("source_kind:figma", persistedWeight.FeatureKey);
        Assert.Equal(0.125, persistedWeight.Weight);
        Assert.Equal("gradient_bridge_update", persistedWeight.Derivation);
        Assert.Equal(
            [IndependentlyHashBatchIdentity("raw-source-id")],
            persistedWeight.SourceIdentityHashes);
        Assert.DoesNotContain(
            checkpoint.CriticUnit.LearnedWeights ?? [],
            weight => weight.FeatureKey.StartsWith("internal_vector:", StringComparison.Ordinal));
        Assert.DoesNotContain(
            checkpoint.CriticUnit.LearnedWeights ?? [],
            weight => string.Equals(weight.FeatureKey, "source_manifest:hash_diversity", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TrainBatch_ConfiguredBridgeAggregatesDuplicateSemanticSignalsByMeanIndependentOfRowOrder()
    {
        static RecordingGradientBridge CreateMeanEchoBridge()
            => new((request, _) =>
            {
                var meanSignal = Assert.Single(
                    request.SanitizedSignals,
                    signal => string.Equals(signal.AllowedFeatureKey, "internal_vector:mean", StringComparison.Ordinal));
                return Task.FromResult(OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(new(
                    request.BatchIdentityHash,
                    request.DomainTag,
                    request.InputMachineVersion,
                    AverageRewardMargin: 0.5,
                    Updates:
                    [
                        new DesignConversionMachineGradientBridgeUpdate(
                            meanSignal.CanonicalUnitId,
                            meanSignal.AllowedFeatureKey,
                            meanSignal.SignalValue)
                    ])));
            });

        var firstBridge = CreateMeanEchoBridge();
        var reversedBridge = CreateMeanEchoBridge();
        var firstMachine = CreateBridgeMachine(firstBridge);
        var reversedMachine = CreateBridgeMachine(reversedBridge);

        var firstTrain = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            firstMachine,
            BuildGradientAggregationBatch(reverseRowsAndCustody: false));
        var reversedTrain = await DesignConversionLegalTeacherCapabilityTestFactory.TrainMachineStrictAsync(
            reversedMachine,
            BuildGradientAggregationBatch(reverseRowsAndCustody: true));

        Assert.True(firstTrain.IsSuccess, firstTrain.ErrorMessage);
        Assert.True(reversedTrain.IsSuccess, reversedTrain.ErrorMessage);
        var firstRequest = Assert.IsType<DesignConversionMachineGradientBridgeRequest>(firstBridge.LastRequest);
        var reversedRequest = Assert.IsType<DesignConversionMachineGradientBridgeRequest>(reversedBridge.LastRequest);
        Assert.Equal(JsonSerializer.Serialize(firstRequest), JsonSerializer.Serialize(reversedRequest));
        Assert.Equal(
            ["internal_vector:dimension", "internal_vector:mean", "source_kind:figma"],
            firstRequest.SanitizedSignals.Select(signal => signal.AllowedFeatureKey).ToArray());
        Assert.Equal(3, firstRequest.SanitizedSignals.Length);
        var aggregatedMean = Assert.Single(
            firstRequest.SanitizedSignals,
            signal => string.Equals(signal.AllowedFeatureKey, "internal_vector:mean", StringComparison.Ordinal));
        Assert.Equal(0.5, aggregatedMean.SignalValue, precision: 6);
        Assert.DoesNotContain(
            typeof(DesignConversionMachineGradientBridgeSanitizedSignal).GetProperties(),
            property => property.Name.Contains("Identity", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("SourceId", StringComparison.OrdinalIgnoreCase));
        var serializedRequest = JsonSerializer.Serialize(firstRequest);
        Assert.DoesNotContain("custody-source-a", serializedRequest, StringComparison.Ordinal);
        Assert.DoesNotContain("custody-source-z", serializedRequest, StringComparison.Ordinal);

        var firstWeight = Assert.Single(Export(firstMachine).CriticUnit.LearnedWeights ?? []);
        var reversedWeight = Assert.Single(Export(reversedMachine).CriticUnit.LearnedWeights ?? []);
        Assert.Equal("internal_vector:mean", firstWeight.FeatureKey);
        Assert.Equal(0.5, firstWeight.Weight, precision: 6);
        Assert.Equal(firstWeight.FeatureKey, reversedWeight.FeatureKey);
        Assert.Equal(firstWeight.Weight, reversedWeight.Weight);
        Assert.Equal(firstWeight.Derivation, reversedWeight.Derivation);
        var expectedCustodyHashes = new[] { "custody-source-a", "custody-source-z" }
            .Select(IndependentlyHashBatchIdentity)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(expectedCustodyHashes, firstWeight.SourceIdentityHashes);
        Assert.Equal(expectedCustodyHashes, reversedWeight.SourceIdentityHashes);
    }

    [Theory]
    [MemberData(nameof(InvalidBridgeResultFactories))]
    public async Task TrainBatch_InvalidBridgeSuccessResultDoesNotMutate(
        string _,
        Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult> forgeResult)
    {
        var bridge = new RecordingGradientBridge((request, _) => Task.FromResult(
            OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(forgeResult(request))));
        var machine = CreateBridgeMachine(bridge);
        var before = Snapshot(machine);

        var result = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildGradientBatch("invalid-result"));

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeInvalidResultCode, result.ErrorCode);
        Assert.Equal("Gradient bridge result is invalid.", result.ErrorMessage);
        Assert.Equal(1, bridge.CallCount);
        Assert.Equal(before, Snapshot(machine));
    }

    [Fact]
    public async Task TrainBatch_BridgeFailureCancellationAndExceptionPreserveStateAndExposeNoExceptionDetail()
    {
        var returnedFailureBridge = new RecordingGradientBridge((_, _) => Task.FromResult(
            OperationResult<DesignConversionMachineGradientBridgeResult>.Fail(
                "C:\\private\\secret-gradient-input.json",
                "C:\\private\\SECRET_PROVIDER_FAILURE")));
        var returnedFailureMachine = CreateBridgeMachine(returnedFailureBridge);
        var returnedFailureBefore = Snapshot(returnedFailureMachine);

        var returnedFailure = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                returnedFailureMachine,
                BuildGradientBatch("returned-failure"));

        Assert.False(returnedFailure.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeFailedCode, returnedFailure.ErrorCode);
        Assert.Equal("Gradient bridge did not return a usable result.", returnedFailure.ErrorMessage);
        Assert.DoesNotContain("secret", returnedFailure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private", returnedFailure.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, returnedFailureBridge.CallCount);
        Assert.Equal(returnedFailureBefore, Snapshot(returnedFailureMachine));

        var returnedCancellationBridge = new RecordingGradientBridge((_, _) => Task.FromResult(
            OperationResult<DesignConversionMachineGradientBridgeResult>.Fail(
                "C:\\private\\secret-gradient-input.json",
                "CANCELLED")));
        var returnedCancellationMachine = CreateBridgeMachine(returnedCancellationBridge);
        var returnedCancellationBefore = Snapshot(returnedCancellationMachine);

        var returnedCancellation = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                returnedCancellationMachine,
                BuildGradientBatch("returned-cancellation"));

        Assert.False(returnedCancellation.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeFailedCode, returnedCancellation.ErrorCode);
        Assert.Equal("Gradient bridge did not return a usable result.", returnedCancellation.ErrorMessage);
        Assert.Equal(returnedCancellationBefore, Snapshot(returnedCancellationMachine));

        var throwingBridge = new RecordingGradientBridge((_, _) => Task.FromException<OperationResult<DesignConversionMachineGradientBridgeResult>>(
            new InvalidOperationException("C:\\private\\secret-gradient-input.json")));
        var throwingMachine = CreateBridgeMachine(throwingBridge);
        var throwingBefore = Snapshot(throwingMachine);

        var thrown = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                throwingMachine,
                BuildGradientBatch("throwing-bridge"));

        Assert.False(thrown.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeFailedCode, thrown.ErrorCode);
        Assert.Equal("Gradient bridge did not return a usable result.", thrown.ErrorMessage);
        Assert.DoesNotContain("secret", thrown.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, throwingBridge.CallCount);
        Assert.Equal(throwingBefore, Snapshot(throwingMachine));

        using var cancellation = new CancellationTokenSource();
        var cancellingBridge = new RecordingGradientBridge((request, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(
                ValidResult(request)));
        });
        var cancellingMachine = CreateBridgeMachine(cancellingBridge);
        var cancellingBefore = Snapshot(cancellingMachine);

        var cancelled = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                cancellingMachine,
                BuildGradientBatch("cancelled-bridge"),
                cancellation.Token);

        Assert.False(cancelled.IsSuccess);
        Assert.Equal("CANCELLED", cancelled.ErrorCode);
        Assert.Equal(1, cancellingBridge.CallCount);
        Assert.Equal(cancellingBefore, Snapshot(cancellingMachine));
    }

    [Fact]
    public async Task TrainBatch_WithoutBridgeRetainsDeterministicScaffoldBehavior()
    {
        var machine = new DesignConversionMachine();

        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                BuildGradientBatch("deterministic-scaffold"));

        Assert.True(train.IsSuccess, train.ErrorMessage);
        var checkpoint = Export(machine);
        Assert.Contains(
            checkpoint.CriticUnit.LearnedWeights ?? [],
            weight => string.Equals(weight.FeatureKey, "internal_vector:dimension", StringComparison.Ordinal));
        Assert.DoesNotContain(
            checkpoint.CriticUnit.LearnedWeights ?? [],
            weight => string.Equals(weight.FeatureKey, "source_manifest:hash_diversity", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TrainBatch_ConfiguredBridgeWithNoSanitizedSignalsFailsBeforeBridgeAndMutation()
    {
        var bridge = new RecordingGradientBridge((request, _) => Task.FromResult(
            OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(new(
                request.BatchIdentityHash,
                request.DomainTag,
                request.InputMachineVersion,
                0.1,
                [ValidUpdate(request)]))));
        var machine = CreateBridgeMachine(bridge);
        var before = Snapshot(machine);
        var emptySignalBatch = new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            "bridge-empty-signals",
            DesignConversionMachine.DomainTag,
            [new LearningTrainingItem(
                "row",
                "not-json",
                "{\"html\":\"<main><p>Complete conversion</p></main>\",\"css\":\"main{display:grid}\"}",
                "{\"html\":\"<main></main>\",\"css\":\"main{display:block}\"}")],
            ["raw-source-id"]);

        var result = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, emptySignalBatch);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeInputEmptyCode, result.ErrorCode);
        Assert.Equal(0, bridge.CallCount);
        Assert.Equal(before, Snapshot(machine));
    }

    [Fact]
    public async Task TrainBatch_ImmutableRequestBoundaryRejectsReplacementAuthorityWithoutMutation()
    {
        var bridge = new RecordingGradientBridge((request, _) =>
        {
            var observed = request.SanitizedSignals[0];
            var replacement = observed with { AllowedFeatureKey = "C:\\private\\replacement" };
            var outerView = (IList<DesignConversionMachineGradientBridgeSanitizedSignal>)request.SanitizedSignals;
            Assert.Throws<NotSupportedException>(() => outerView[0] = replacement);
            return Task.FromResult(OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(new(
                request.BatchIdentityHash,
                request.DomainTag,
                request.InputMachineVersion,
                0.5,
                [new DesignConversionMachineGradientBridgeUpdate(
                    replacement.CanonicalUnitId,
                    replacement.AllowedFeatureKey,
                    0.125)])));
        });
        var machine = CreateBridgeMachine(bridge);
        var before = Snapshot(machine);

        var result = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                BuildGradientBatch("immutable-request-alias"));

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeInvalidResultCode, result.ErrorCode);
        Assert.Equal(before, Snapshot(machine));
    }

    [Fact]
    public async Task TrainBatch_ImmutableResultConstructionSnapshotsBridgeOwnedArrayBeforeReturn()
    {
        var bridge = new RecordingGradientBridge((request, _) =>
        {
            var mutableUpdates = new[] { ValidUpdate(request) };
            var immutableUpdates = mutableUpdates.ToImmutableArray();
            mutableUpdates[0] = ValidUpdate(request) with { AllowedFeatureKey = "source_kind:other" };
            return Task.FromResult(OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(new(
                request.BatchIdentityHash,
                request.DomainTag,
                request.InputMachineVersion,
                0.5,
                immutableUpdates)));
        });
        var machine = CreateBridgeMachine(bridge);

        var result = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                BuildGradientBatch("immutable-result-alias"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var weight = Assert.Single(Export(machine).CriticUnit.LearnedWeights ?? []);
        Assert.Equal("source_kind:figma", weight.FeatureKey);
    }

    [Fact]
    public async Task TrainBatch_OptimisticBridgeCallsPermitOneCommitAndRejectStalePeer()
    {
        var bridge = new OptimisticGradientBridge();
        var machine = CreateBridgeMachine(bridge);

        var first = DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildGradientBatch("serialized-first"));
        await bridge.FirstInvocationEntered.Task;
        var second = DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildGradientBatch("serialized-second"));
        await bridge.SecondInvocationEntered.Task;
        var secondResult = await second;
        bridge.ReleaseFirstInvocation();
        var firstResult = await first;

        Assert.Equal(2, bridge.CallCount);
        Assert.Equal(2, bridge.MaxConcurrentInvocations);
        Assert.Equal(["0.1.0", "0.1.0"], bridge.ObservedVersions);
        Assert.True(secondResult.IsSuccess, secondResult.ErrorMessage);
        Assert.False(firstResult.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeVersionConflictCode, firstResult.ErrorCode);
        var checkpoint = Export(machine);
        Assert.Equal("0.1.1", checkpoint.Version);
        Assert.Single(checkpoint.TrainingHistory);
    }

    [Fact]
    public async Task TrainBatch_ReentrantBridgeCompletesWithoutDeadlockAndRejectsStaleOuterResult()
    {
        var bridge = new ReentrantGradientBridge();
        var machine = CreateBridgeMachine(bridge);
        bridge.Machine = machine;
        var outer = DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildGradientBatch("reentrant-outer"));

        var completed = await Task.WhenAny(outer, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.Same(outer, completed);
        var outerResult = await outer;

        Assert.NotNull(bridge.InnerResult);
        Assert.True(bridge.InnerResult!.IsSuccess, bridge.InnerResult.ErrorMessage);
        Assert.False(outerResult.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeVersionConflictCode, outerResult.ErrorCode);
        Assert.Single(Export(machine).TrainingHistory);
    }

    [Fact]
    public async Task TrainBatch_ImportCheckpointAtomicallyInvalidatesPausedBridgeResult()
    {
        var bridge = new PausingGradientBridge();
        var machine = CreateBridgeMachine(bridge);
        var train = DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildGradientBatch("bridge-import-race"));
        await bridge.InvocationEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var importedCheckpoint = Export(new DesignConversionMachine()) with
        {
            CriticUnit = Export(new DesignConversionMachine()).CriticUnit with
            {
                LearnedWeightsPresent = true,
                LearnedWeights =
                [
                    new DesignConversionLearnedWeightCheckpoint(
                        "source_kind:figma",
                        0.75,
                        3,
                        "imported-batch",
                        "imported",
                        [SourceHash],
                        [
                            DesignConversionMachine.LearnedWeightsPersistedEvidence,
                            DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence
                        ])
                ]
            }
        };
        var imported = machine.ImportCheckpoint(importedCheckpoint);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        var afterImport = Snapshot(machine);

        bridge.Release();
        var result = await train.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeVersionConflictCode, result.ErrorCode);
        Assert.Equal(afterImport, Snapshot(machine));
        var checkpoint = Export(machine);
        var importedWeight = Assert.Single(checkpoint.CriticUnit.LearnedWeights ?? []);
        Assert.Equal(0.75, importedWeight.Weight);
        Assert.Equal(3, importedWeight.ObservationCount);
        Assert.Empty(checkpoint.TrainingHistory);
    }

    [Fact]
    public async Task TrainBatch_ImportAsyncAtomicallyBindsCheckpointBeforeRejectingPausedBridgeResult()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"bridge-import-async-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var source = new DesignConversionMachine();
            var sourceExport = await source.SaveCheckpointAsync(Path.Combine(root, "checkpoint.json"));
            Assert.True(sourceExport.IsSuccess, sourceExport.ErrorMessage);
            var sourceMachineExport = sourceExport.Value!;

            var bridge = new PausingGradientBridge();
            var machine = CreateBridgeMachine(bridge);
            var train = DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(
                    machine,
                    BuildGradientBatch("bridge-import-async-race"));
            await bridge.InvocationEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var imported = await machine.ImportAsync(sourceMachineExport);
            Assert.True(imported.IsSuccess, imported.ErrorMessage);
            var boundExport = await machine.ExportAsync();
            Assert.True(boundExport.IsSuccess, boundExport.ErrorMessage);
            var afterImport = Snapshot(machine);

            bridge.Release();
            var result = await train.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.False(result.IsSuccess);
            Assert.Equal(DesignConversionMachine.GradientBridgeVersionConflictCode, result.ErrorCode);
            Assert.Equal(afterImport, Snapshot(machine));
            Assert.Equal(sourceMachineExport.CheckpointRef, boundExport.Value!.CheckpointRef);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveCheckpointAsync_EvaluationAfterAtomicCaptureRejectsStaleSaveWithoutBinding()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"save-evaluate-race-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var checkpointPath = Path.Combine(root, "checkpoint.json");
            var baselineMachine = new DesignConversionMachine();
            var baselineTrain = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(
                    baselineMachine,
                    BuildGradientBatch("baseline-state"));
            Assert.True(baselineTrain.IsSuccess, baselineTrain.ErrorMessage);
            var baselineSave = await baselineMachine.SaveCheckpointAsync(checkpointPath);
            Assert.True(baselineSave.IsSuccess, baselineSave.ErrorMessage);
            var baselineCheckpoint = Export(baselineMachine);
            var baselineTargetBytes = await File.ReadAllBytesAsync(checkpointPath);
            var baselineTargetHash = Convert.ToHexString(SHA256.HashData(baselineTargetBytes)).ToLowerInvariant();

            var machine = new DesignConversionMachine();
            var subjectFirstTrain = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(
                    machine,
                    BuildGradientBatch("subject-state-first"));
            var subjectSecondTrain = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(
                    machine,
                    BuildGradientBatch("subject-state-second"));
            Assert.True(subjectFirstTrain.IsSuccess, subjectFirstTrain.ErrorMessage);
            Assert.True(subjectSecondTrain.IsSuccess, subjectSecondTrain.ErrorMessage);
            var capturedSubjectCheckpoint = Export(machine);
            Assert.NotEqual(baselineCheckpoint.Version, capturedSubjectCheckpoint.Version);
            var captureEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseCapture = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var captureScope = DesignConversionMachine.UseCheckpointStateCaptureForCurrentAsyncFlow(
                async ct =>
                {
                    captureEntered.TrySetResult(true);
                    await releaseCapture.Task.WaitAsync(ct);
                });

            var save = machine.SaveCheckpointAsync(checkpointPath);
            await captureEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var evaluation = await machine.EvaluateAsync(new LearningEvaluationSet<DesignConversionDomain.DesignConversionDomain>(
                "save-evaluate-race-eval",
                DesignConversionMachine.DomainTag,
                [new LearningEvaluationItem(
                    "row",
                    $"{{\"unit_id\":\"{DesignConversionRuntimeScaffoldCriticUnit.UnitId}\",\"source_kind\":\"figma\"}}",
                    "{\"passed\":true}")]));
            Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);

            releaseCapture.TrySetResult(true);
            var saved = await save.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.False(saved.IsSuccess);
            Assert.Equal(DesignConversionMachine.ExportCheckpointVersionStaleCode, saved.ErrorCode);
            var targetBytesAfterStaleSave = await File.ReadAllBytesAsync(checkpointPath);
            Assert.Equal(baselineTargetBytes, targetBytesAfterStaleSave);
            Assert.Equal(baselineTargetHash, Convert.ToHexString(SHA256.HashData(targetBytesAfterStaleSave)).ToLowerInvariant());
            var loadedBaseline = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath);
            Assert.True(loadedBaseline.IsSuccess, loadedBaseline.ErrorMessage);
            Assert.Equal(baselineCheckpoint.Version, loadedBaseline.Value!.Checkpoint.Version);
            var baselineHistory = Assert.Single(baselineCheckpoint.TrainingHistory);
            var preservedHistory = Assert.Single(loadedBaseline.Value.Checkpoint.TrainingHistory);
            Assert.Equal(baselineHistory.BatchId, preservedHistory.BatchId);
            Assert.Equal(baselineHistory.RowsTrained, preservedHistory.RowsTrained);
            Assert.Equal(
                (baselineCheckpoint.CriticUnit.LearnedWeights ?? []).Count,
                (loadedBaseline.Value.Checkpoint.CriticUnit.LearnedWeights ?? []).Count);
            var baselineWeight = Assert.Single(
                baselineCheckpoint.CriticUnit.LearnedWeights ?? [],
                weight => string.Equals(weight.FeatureKey, "source_kind:figma", StringComparison.Ordinal));
            var preservedWeight = Assert.Single(
                loadedBaseline.Value.Checkpoint.CriticUnit.LearnedWeights ?? [],
                weight => string.Equals(weight.FeatureKey, "source_kind:figma", StringComparison.Ordinal));
            Assert.Equal(baselineWeight.FeatureKey, preservedWeight.FeatureKey);
            Assert.Equal(baselineWeight.Weight, preservedWeight.Weight);
            Assert.Equal(baselineWeight.ObservationCount, preservedWeight.ObservationCount);
            var boundExport = await machine.ExportAsync();
            Assert.False(boundExport.IsSuccess);
            Assert.Equal(DesignConversionMachine.ExportCheckpointNotBoundCode, boundExport.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveCheckpointAsync_CancellationBeforePersistenceMoveLeavesExistingTargetUntouched()
    {
        var root = Path.Combine(Path.GetTempPath(), "llm_mvp_core", $"save-cancellation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var checkpointPath = Path.Combine(root, "checkpoint.json");
            var baselineMachine = new DesignConversionMachine();
            var baselineSave = await baselineMachine.SaveCheckpointAsync(checkpointPath);
            Assert.True(baselineSave.IsSuccess, baselineSave.ErrorMessage);
            var baselineTargetBytes = await File.ReadAllBytesAsync(checkpointPath);

            var machine = new DesignConversionMachine();
            using var cancellation = new CancellationTokenSource();
            var captureEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseCapture = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var captureScope = DesignConversionMachine.UseCheckpointStateCaptureForCurrentAsyncFlow(
                async ct =>
                {
                    captureEntered.TrySetResult(true);
                    await releaseCapture.Task.WaitAsync(ct);
                });

            var save = machine.SaveCheckpointAsync(checkpointPath, cancellation.Token);
            await captureEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            cancellation.Cancel();
            var cancelled = await save.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.False(cancelled.IsSuccess);
            Assert.Equal("CANCELLED", cancelled.ErrorCode);
            Assert.Equal(baselineTargetBytes, await File.ReadAllBytesAsync(checkpointPath));
            var boundExport = await machine.ExportAsync();
            Assert.False(boundExport.IsSuccess);
            Assert.Equal(DesignConversionMachine.ExportCheckpointNotBoundCode, boundExport.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TrainBatch_PausedBridgeEvaluationExportAndRuntimeFlowObserveCoherentState()
    {
        var bridge = new PausingGradientBridge();
        var machine = CreateBridgeMachine(bridge);
        var train = DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildGradientBatch("bridge-read-race"));
        await bridge.InvocationEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));

        var evaluation = await machine.EvaluateAsync(new LearningEvaluationSet<DesignConversionDomain.DesignConversionDomain>(
            "bridge-read-race-eval",
            DesignConversionMachine.DomainTag,
            [new LearningEvaluationItem(
                "row",
                $"{{\"unit_id\":\"{DesignConversionRuntimeScaffoldCriticUnit.UnitId}\",\"source_kind\":\"figma\"}}",
                "{\"passed\":true}")]));
        var checkpoint = Export(machine);
        var runtimeFlow = machine.CreateRuntimeRoutingFlow();

        Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
        Assert.Empty(checkpoint.TrainingHistory);
        Assert.Empty(checkpoint.CriticUnit.LearnedWeights ?? []);
        Assert.Same(machine.Flow, runtimeFlow);

        bridge.Release();
        var result = await train.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeVersionConflictCode, result.ErrorCode);
        var finalCheckpoint = Export(machine);
        Assert.Empty(finalCheckpoint.TrainingHistory);
        Assert.Empty(finalCheckpoint.CriticUnit.LearnedWeights ?? []);
        Assert.Equal(checkpoint.AccuracyAfterLastBatch, finalCheckpoint.AccuracyAfterLastBatch);
    }

    [Fact]
    public async Task TrainBatch_NoBridgeCumulativePreflightRejectsSecondDeltaBeforeMutation()
    {
        var machine = new DesignConversionMachine();
        var baseline = Export(new DesignConversionMachine());
        var importedCheckpoint = baseline with
        {
            CriticUnit = baseline.CriticUnit with
            {
                LearnedWeightsPresent = true,
                LearnedWeights =
                [
                    new DesignConversionLearnedWeightCheckpoint(
                        "source_kind:figma",
                        0.125,
                        int.MaxValue - 1,
                        "imported-batch",
                        "imported",
                        [SourceHash],
                        [
                            DesignConversionMachine.LearnedWeightsPersistedEvidence,
                            DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence
                        ])
                ]
            }
        };
        Assert.True(machine.ImportCheckpoint(importedCheckpoint).IsSuccess);
        var before = Snapshot(machine);

        var result = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                BuildGradientBatchWithRows("cumulative-overflow", 2));

        Assert.False(result.IsSuccess);
        Assert.Equal("LEARNED_WEIGHT_NUMERIC_OVERFLOW", result.ErrorCode);
        Assert.Equal(before, Snapshot(machine));
    }

    [Fact]
    public async Task TrainBatch_MachineAttachesPrivateCanonicalCustodyHashesAfterBridgeValidation()
    {
        var bridge = new RecordingGradientBridge((request, _) => Task.FromResult(
            OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(ValidResult(request))));
        var machine = CreateBridgeMachine(bridge);

        var result = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                BuildGradientBatch("private-custody-attachment"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var weight = Assert.Single(Export(machine).CriticUnit.LearnedWeights ?? []);
        Assert.Equal(
            [IndependentlyHashBatchIdentity("raw-source-id")],
            weight.SourceIdentityHashes);
    }

    [Fact]
    public async Task TrainBatch_RejectsProjectedWeightedSumOverflowBeforeSecondMutation()
    {
        var bridge = new RecordingGradientBridge((request, _) => Task.FromResult(
            OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(
                ValidResult(request) with { Updates = [ValidUpdate(request, double.MaxValue)] })));
        var machine = CreateBridgeMachine(bridge);

        var first = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildGradientBatch("max-first"));
        var afterFirst = Snapshot(machine);
        var second = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildGradientBatch("max-second"));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.False(second.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeInvalidResultCode, second.ErrorCode);
        Assert.Equal(afterFirst, Snapshot(machine));
    }

    [Fact]
    public async Task TrainBatch_RejectsImportedMaximumObservationCountBeforeMutation()
    {
        var bridge = new RecordingGradientBridge((request, _) => Task.FromResult(
            OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(ValidResult(request))));
        var machine = CreateBridgeMachine(bridge);
        var baseline = Export(new DesignConversionMachine());
        var importedWeight = new DesignConversionLearnedWeightCheckpoint(
            "source_kind:figma",
            0.125,
            int.MaxValue,
            "imported-batch",
            "gradient_bridge_update",
            [SourceHash],
            [
                DesignConversionMachine.LearnedWeightsPersistedEvidence,
                DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence
            ]);
        var import = machine.ImportCheckpoint(baseline with
        {
            CriticUnit = baseline.CriticUnit with
            {
                LearnedWeightsPresent = true,
                LearnedWeights = [importedWeight]
            }
        });
        Assert.True(import.IsSuccess, import.ErrorMessage);
        var before = Snapshot(machine);

        var train = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(machine, BuildGradientBatch("max-observation"));

        Assert.False(train.IsSuccess);
        Assert.Equal(DesignConversionMachine.GradientBridgeInvalidResultCode, train.ErrorCode);
        Assert.Equal(1, bridge.CallCount);
        Assert.Equal(before, Snapshot(machine));
    }

    [Fact]
    public void ContractHasSanitizedSurfaceAndMachineRetainsLegacyConstructors()
    {
        var contractTypes = new[]
        {
            typeof(IDesignConversionMachineGradientBridge),
            typeof(DesignConversionMachineGradientBridgeRequest),
            typeof(DesignConversionMachineGradientBridgeSanitizedSignal),
            typeof(DesignConversionMachineGradientBridgeResult),
            typeof(DesignConversionMachineGradientBridgeUpdate)
        };
        var forbiddenFragments = new[] { "LlmModel", "Tensor", "Checkpoint", "Provider", "ModelId", "Path" };

        foreach (var type in contractTypes)
        {
            Assert.DoesNotContain(forbiddenFragments, fragment => type.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase));
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                Assert.DoesNotContain(forbiddenFragments, fragment => property.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase));
                AssertNoForbiddenSurfaceType(property.PropertyType, forbiddenFragments);
            }

            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                AssertNoForbiddenSurfaceType(method.ReturnType, forbiddenFragments);
                foreach (var parameter in method.GetParameters())
                {
                    Assert.DoesNotContain(forbiddenFragments, fragment => parameter.Name?.Contains(fragment, StringComparison.OrdinalIgnoreCase) == true);
                    AssertNoForbiddenSurfaceType(parameter.ParameterType, forbiddenFragments);
                }
            }
        }

        Assert.NotNull(typeof(DesignConversionMachine).GetConstructor(Type.EmptyTypes));
        Assert.NotNull(typeof(DesignConversionMachine).GetConstructor([typeof(DesignConversionRoutingFlow)]));
        Assert.Null(typeof(DesignConversionMachine).GetConstructor([typeof(IDesignConversionMachineGradientBridge)]));
        Assert.NotNull(typeof(DesignConversionMachine).GetConstructor(
            [typeof(DesignConversionRoutingFlow), typeof(IDesignConversionMachineGradientBridge)]));
        Assert.Equal(
            typeof(ImmutableArray<DesignConversionMachineGradientBridgeSanitizedSignal>),
            typeof(DesignConversionMachineGradientBridgeRequest).GetProperty(nameof(DesignConversionMachineGradientBridgeRequest.SanitizedSignals))!.PropertyType);
        Assert.DoesNotContain(
            typeof(DesignConversionMachineGradientBridgeSanitizedSignal).GetProperties(),
            property => property.Name.Contains("Identity", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(
            typeof(ImmutableArray<DesignConversionMachineGradientBridgeUpdate>),
            typeof(DesignConversionMachineGradientBridgeResult).GetProperty(nameof(DesignConversionMachineGradientBridgeResult.Updates))!.PropertyType);
        Assert.DoesNotContain(
            typeof(DesignConversionMachineGradientBridgeUpdate).GetProperties(),
            property => property.Name.Contains("Identity", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<object[]> InvalidBridgeResultFactories()
    {
        yield return ["wrong-batch-identity", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { BatchIdentityHash = "wrong-batch" })];
        yield return ["raw-batch-identity", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { BatchIdentityHash = @"C:\Users\forged\private\token=PA4-SECRET" })];
        yield return ["uppercase-batch-identity", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { BatchIdentityHash = request.BatchIdentityHash.ToUpperInvariant() })];
        yield return ["wrong-domain", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { DomainTag = "WrongDomain" })];
        yield return ["wrong-version", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { InputMachineVersion = "0.0.0" })];
        yield return ["nan-margin", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { AverageRewardMargin = double.NaN })];
        yield return ["infinite-margin", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { AverageRewardMargin = double.PositiveInfinity })];
        yield return ["empty-updates", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [] })];
        yield return ["bad-unit", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request) with { CanonicalUnitId = "other-unit" }] })];
        yield return ["bad-key", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request) with { AllowedFeatureKey = "usefulness_score" }] })];
        yield return ["private-path-key", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request) with { AllowedFeatureKey = "C:\\private\\secret" }] })];
        yield return ["control-key", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request) with { AllowedFeatureKey = "raw\ncontrol" }] })];
        yield return ["provider-looking-key", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request) with { AllowedFeatureKey = "provider_model_signal" }] })];
        yield return ["raw-arbitrary-key", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request) with { AllowedFeatureKey = "arbitrary_raw_value" }] })];
        yield return ["unobserved-normal-key", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request) with { AllowedFeatureKey = "source_kind:other" }] })];
        yield return ["nan-contribution", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request) with { Contribution = double.NaN }] })];
        yield return ["infinite-contribution", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request) with { Contribution = double.NegativeInfinity }] })];
        yield return ["duplicate-update", new Func<DesignConversionMachineGradientBridgeRequest, DesignConversionMachineGradientBridgeResult>(request => ValidResult(request) with { Updates = [ValidUpdate(request), ValidUpdate(request) with { Contribution = 0.25 }] })];
    }

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> BuildGradientBatch(string batchId)
        => new(
            batchId,
            DesignConversionMachine.DomainTag,
            [
                new LearningTrainingItem(
                    $"{batchId}:critic",
                    $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_manifest_hash\":\"sha256:{SourceHash}\",\"internal_vector\":[0.2,0.4],\"source_kind\":\"figma\",\"raw_secret\":\"do-not-expose\"}}",
                    "{\"html\":\"<main><section><h1>Converted design</h1></section></main>\",\"css\":\"main{display:grid}section{padding:1rem}\"}",
                    "{\"html\":\"<main><section></section></main>\",\"css\":\"main{display:block}section{padding:0}\"}")
            ],
            ["raw-source-id"]);

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> BuildGradientBatchWithRows(
        string batchId,
        int rowCount)
        => new(
            batchId,
            DesignConversionMachine.DomainTag,
            Enumerable.Range(0, rowCount)
                .Select(index => new LearningTrainingItem(
                    $"{batchId}:critic:{index}",
                    $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_manifest_hash\":\"sha256:{SourceHash}\",\"source_kind\":\"figma\"}}",
                    "{\"html\":\"<main><section><h1>Converted design</h1></section></main>\",\"css\":\"main{display:grid}section{padding:1rem}\"}",
                    "{\"html\":\"<main><section></section></main>\",\"css\":\"main{display:block}section{padding:0}\"}"))
                .ToArray(),
            Enumerable.Range(0, rowCount).Select(index => $"raw-source-id:{index}").ToArray());

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain> BuildGradientAggregationBatch(
        bool reverseRowsAndCustody)
    {
        var rows = new[]
        {
            new LearningTrainingItem(
                "semantic-row-low",
                $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_kind\":\"figma\",\"internal_vector\":[0.2,0.4]}}",
                "{\"html\":\"<main>chosen-low</main>\",\"css\":\"main{display:grid}\"}",
                "{\"html\":\"<main>rejected-low</main>\",\"css\":\"main{display:block}\"}"),
            new LearningTrainingItem(
                "semantic-row-high",
                $"{{\"unit_id\":\"{DesignConversionDpoUnits.CriticUnit}\",\"source_kind\":\"figma\",\"internal_vector\":[0.6,0.8]}}",
                "{\"html\":\"<main>chosen-high</main>\",\"css\":\"main{display:grid}\"}",
                "{\"html\":\"<main>rejected-high</main>\",\"css\":\"main{display:block}\"}")
        };
        var custody = new[] { "custody-source-z", "custody-source-a" };
        return new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            "semantic-aggregation-batch",
            DesignConversionMachine.DomainTag,
            reverseRowsAndCustody ? rows.Reverse().ToArray() : rows,
            reverseRowsAndCustody ? custody.Reverse().ToArray() : custody);
    }

    private static DesignConversionMachineGradientBridgeResult ValidResult(
        DesignConversionMachineGradientBridgeRequest request)
        => new(
            request.BatchIdentityHash,
            request.DomainTag,
            request.InputMachineVersion,
            0.5,
            [ValidUpdate(request)]);

    private static DesignConversionMachineGradientBridgeUpdate ValidUpdate(
        DesignConversionMachineGradientBridgeRequest request,
        double contribution = 0.125)
        => new(
            DesignConversionRuntimeScaffoldCriticUnit.UnitId,
            "source_kind:figma",
            contribution);

    private static DesignConversionMachine CreateBridgeMachine(IDesignConversionMachineGradientBridge bridge)
        => new(new DesignConversionRoutingFlow(), bridge);

    private static DesignConversionMachineCheckpoint Export(DesignConversionMachine machine)
    {
        var export = machine.ExportCheckpoint();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        return export.Value!;
    }

    private static string Snapshot(DesignConversionMachine machine)
        => JsonSerializer.Serialize(Export(machine) with { ExportedAtUtc = DateTimeOffset.UnixEpoch });

    private static void AssertCanonicalHash(string value)
    {
        Assert.Equal(64, value.Length);
        Assert.All(value, character => Assert.True(
            character is >= '0' and <= '9' or >= 'a' and <= 'f',
            $"Expected lowercase SHA-256 hexadecimal character, got '{character}'."));
    }

    private static string IndependentlyHashBatchIdentity(string rawBatchId)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawBatchId))).ToLowerInvariant();

    private static void AssertNoForbiddenSurfaceType(Type type, IReadOnlyList<string> forbiddenFragments)
    {
        Assert.DoesNotContain(forbiddenFragments, fragment => type.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase));
        foreach (var genericArgument in type.GetGenericArguments())
            AssertNoForbiddenSurfaceType(genericArgument, forbiddenFragments);
    }

    private sealed class RecordingGradientBridge(
        Func<DesignConversionMachineGradientBridgeRequest, CancellationToken, Task<OperationResult<DesignConversionMachineGradientBridgeResult>>> handler)
        : IDesignConversionMachineGradientBridge
    {
        public int CallCount { get; private set; }

        public DesignConversionMachineGradientBridgeRequest? LastRequest { get; private set; }

        public Task<OperationResult<DesignConversionMachineGradientBridgeResult>> ComputeUpdatesAsync(
            DesignConversionMachineGradientBridgeRequest request,
            CancellationToken ct = default)
        {
            CallCount++;
            LastRequest = request;
            return handler(request, ct);
        }
    }

    private sealed class PausingGradientBridge : IDesignConversionMachineGradientBridge
    {
        private readonly TaskCompletionSource<bool> _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> InvocationEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<OperationResult<DesignConversionMachineGradientBridgeResult>> ComputeUpdatesAsync(
            DesignConversionMachineGradientBridgeRequest request,
            CancellationToken ct = default)
        {
            InvocationEntered.TrySetResult(true);
            await _release.Task.WaitAsync(ct);
            return OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(ValidResult(request));
        }

        public void Release() => _release.TrySetResult(true);
    }

    private sealed class OptimisticGradientBridge : IDesignConversionMachineGradientBridge
    {
        private readonly object _sync = new();
        private readonly TaskCompletionSource<bool> _releaseFirst = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> FirstInvocationEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> SecondInvocationEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }

        public int MaxConcurrentInvocations { get; private set; }

        public List<string> ObservedVersions { get; } = [];

        private int ActiveInvocations { get; set; }

        public async Task<OperationResult<DesignConversionMachineGradientBridgeResult>> ComputeUpdatesAsync(
            DesignConversionMachineGradientBridgeRequest request,
            CancellationToken ct = default)
        {
            int callNumber;
            lock (_sync)
            {
                CallCount++;
                callNumber = CallCount;
                ActiveInvocations++;
                MaxConcurrentInvocations = Math.Max(MaxConcurrentInvocations, ActiveInvocations);
                ObservedVersions.Add(request.InputMachineVersion);
            }

            try
            {
                if (callNumber == 1)
                {
                    FirstInvocationEntered.TrySetResult(true);
                    await _releaseFirst.Task.WaitAsync(ct);
                }
                else
                {
                    SecondInvocationEntered.TrySetResult(true);
                }

                return OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(ValidResult(request));
            }
            finally
            {
                lock (_sync)
                    ActiveInvocations--;
            }
        }

        public void ReleaseFirstInvocation() => _releaseFirst.TrySetResult(true);
    }

    private sealed class ReentrantGradientBridge : IDesignConversionMachineGradientBridge
    {
        private int _callCount;

        public DesignConversionMachine? Machine { get; set; }

        public OperationResult<LearningBatchResult>? InnerResult { get; private set; }

        public async Task<OperationResult<DesignConversionMachineGradientBridgeResult>> ComputeUpdatesAsync(
            DesignConversionMachineGradientBridgeRequest request,
            CancellationToken ct = default)
        {
            if (Interlocked.Increment(ref _callCount) == 1)
            {
                var machine = Machine ?? throw new InvalidOperationException("Machine is required.");
                InnerResult = await DesignConversionLegalTeacherCapabilityTestFactory
                    .TrainMachineStrictAsync(
                        machine,
                        BuildGradientBatch("reentrant-inner"),
                        ct);
            }

            return OperationResult<DesignConversionMachineGradientBridgeResult>.Ok(ValidResult(request));
        }
    }
}
