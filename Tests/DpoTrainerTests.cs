// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Layers;
using TransformerModel;
using TransformerModel.Config;
using TransformerModel.Layers;

namespace LlmFromScratch.Tests;

// Unit tests for DpoTrainer — step output, metric correctness, atomic checkpoint.
// Behavioral assertion gate: DpoMetrics fields are checked for concrete properties,
// not just "no exception thrown".
public class DpoTrainerTests : IDisposable
{
    // Minimal model config: tiny vocab, 1 layer, 16-dim — creates fast in tests
    private static readonly ModelConfig SmallCfg = new()
    {
        VocabSize    = 64,
        ContextLen   = 8,
        HiddenDim    = 16,
        NumLayers    = 1,
        NumHeads     = 2,
        NumKvHeads   = 1,
        FfnHiddenDim = 32
    };

    private readonly string _checkpointDir = Path.Combine(Path.GetTempPath(), $"dpo-test-{Guid.NewGuid()}");

    public void Dispose()
    {
        if (Directory.Exists(_checkpointDir))
            Directory.Delete(_checkpointDir, recursive: true);
    }

    // Build a minimal admitted DpoBatch with the given token IDs (all 1 batch × seqLen)
    private static DpoBatch MakeBatch(int[] chosenIds, int[] rejectedIds)
        => MakeBatch(chosenIds, rejectedIds, CreatePairAdmission());

    private static DpoBatch MakeBatchWithoutAdmission(
        int[] chosenIds,
        int[] rejectedIds)
        => MakeBatch(chosenIds, rejectedIds, admission: null);

    private static DpoBatch MakeBatchWithAdmission(
        int[] chosenIds,
        int[] rejectedIds,
        DpoTrainerPairAdmission admission)
        => MakeBatch(chosenIds, rejectedIds, admission);

    private static DpoBatch MakeBatch(
        int[] chosenIds,
        int[] rejectedIds,
        DpoTrainerPairAdmission? admission)
    {
        float[] cIds  = chosenIds.Select(i => (float)i).ToArray();
        float[] rIds  = rejectedIds.Select(i => (float)i).ToArray();
        float[] cMask = new float[chosenIds.Length];
        float[] rMask = new float[rejectedIds.Length];
        Array.Fill(cMask,  1f);
        Array.Fill(rMask,  1f);

        return new DpoBatch
        {
            ChosenIds    = new Tensor(cIds,  new TensorShape(1, chosenIds.Length)),
            RejectedIds  = new Tensor(rIds,  new TensorShape(1, rejectedIds.Length)),
            ChosenMask   = new Tensor(cMask, new TensorShape(1, chosenIds.Length)),
            RejectedMask = new Tensor(rMask, new TensorShape(1, rejectedIds.Length)),
            Admission = admission
        };
    }

    private static DpoTrainerPairAdmission CreatePairAdmission(
        DpoPairSideAuthorKind chosenAuthor = DpoPairSideAuthorKind.User,
        DpoPairSideAuthorKind rejectedAuthor = DpoPairSideAuthorKind.OpenTeacher)
        => DpoTrainerPairCustoms
            .CreateAdmitted(
                "trainer-test prompt",
                "trainer-test chosen",
                "trainer-test rejected",
                chosenAuthor,
                rejectedAuthor,
                "dpo-trainer-tests")
            .Admission;

    [Fact]
    public void AdmittedPublicConstructorStillCreatesDirectoryEagerly()
    {
        Assert.False(Directory.Exists(_checkpointDir));
        var lease = new TestDpoMutationAdmissionLease();
        _ = CreateAdmittedTrainer(new LlmModel(SmallCfg), new LlmModel(SmallCfg),
            _checkpointDir, mutationLease: lease);
        Assert.Equal(1, lease.PermitRequests);
        Assert.True(Directory.Exists(_checkpointDir));
        Assert.Empty(Directory.GetFileSystemEntries(_checkpointDir));
    }

    [Fact]
    public void PreparedFactoryDefersOutputThroughImportUntilFirstAdmittedStep()
    {
        var sourcePolicy = new LlmModel(SmallCfg);
        var sourceReference = new LlmModel(SmallCfg);
        SetDeterministicWeights(sourcePolicy);
        CopyWeights(sourcePolicy, sourceReference);
        var source = CreateAdmittedTrainer(sourcePolicy, sourceReference, _checkpointDir);
        source.Step(MakeBatch([1, 2, 3], [1, 4, 3]), 1);
        var optimizer = source.ExportOptimizerState();
        Assert.NotEmpty(optimizer.Entries);
        Assert.Equal(1, optimizer.Step);
        var policy = new LlmModel(SmallCfg);
        policy.LoadCheckpoint(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(1)));
        var reference = new LlmModel(SmallCfg);
        CopyWeights(policy, reference);
        var output = Path.Combine(_checkpointDir, "prepared");
        var permitLive = true;
        var lease = new TestDpoMutationAdmissionLease(() =>
        {
            Assert.False(Directory.Exists(output));
            if (!permitLive)
                throw new DpoMutationAdmissionException("refused before output", "DPO_TEST_MUTATION_PERMIT_REVOKED");
            return new CancellationTokenSource();
        });
        var reservations = 0;
        var trainer = DpoTrainer.CreateForPreparedContinuation(policy, reference, output,
            DpoMutationAdmissionCapability.Issue(lease), DpoTrainer.DefaultLearningRate,
            new DpoTrainerPublicationObserver
            {
                AfterReservationAcquired = () =>
                {
                    reservations++;
                    Assert.True(Directory.Exists(output));
                }
            }, null, null);
        Assert.Equal(1, lease.PermitRequests);
        Assert.False(Directory.Exists(output));
        Assert.True(trainer.TryRestoreContinuationState(null, optimizer));
        Assert.Equal(optimizer.ToBytes(), trainer.ExportOptimizerState().ToBytes());
        Assert.False(Directory.Exists(output));
        permitLive = false;
        var refusal = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.StepVector([new DpoVectorBatch(MakeBatch([1, 2, 3], [1, 4, 3]), [1f])], 2));
        Assert.Equal("DPO_TEST_MUTATION_PERMIT_REVOKED", refusal.ErrorCode);
        Assert.False(Directory.Exists(output));
        Assert.Equal(0, reservations);
        Assert.Equal(optimizer.ToBytes(), trainer.ExportOptimizerState().ToBytes());
        permitLive = true;
        var metrics = trainer.StepVector([new DpoVectorBatch(MakeBatch([1, 2, 3], [1, 4, 3]), [1f])], 2);
        Assert.Equal(2, metrics.Step);
        Assert.Equal(2, trainer.ExportOptimizerState().Step);
        Assert.Equal(3, lease.PermitRequests);
        Assert.Equal(1, reservations);
        Assert.True(File.Exists(Path.Combine(output, DpoTrainer.BuildPolicyCheckpointFileName(2))));
        Assert.True(File.Exists(Path.Combine(output, DpoTrainer.BuildMetricsFileName(2))));
    }

    // DPO convergence monitor contracts

    [Fact]
    public void DpoConvergenceMonitor_MaxThresholdWindowOneStopsAfterTwoStableComparisons()
    {
        var monitor = new DpoConvergenceMonitor(float.MaxValue, windowSteps: 1);

        Assert.False(monitor.ShouldStop(0.9f));
        Assert.False(monitor.ShouldStop(0.8f));
        Assert.True(monitor.ShouldStop(0.7f));
        Assert.Equal(3, monitor.StepsTaken);
    }

    [Fact]
    public void DpoConvergenceMonitor_UnstableComparisonResetsStableWindowCount()
    {
        var monitor = new DpoConvergenceMonitor(threshold: 0.01f, windowSteps: 1);

        Assert.False(monitor.ShouldStop(0.5f));
        Assert.False(monitor.ShouldStop(0.495f));
        Assert.False(monitor.ShouldStop(0.4f));
        Assert.False(monitor.ShouldStop(0.395f));
        Assert.True(monitor.ShouldStop(0.394f));
    }

    [Fact]
    public void DpoConvergenceMonitor_StepsTakenAccumulatesNormalizedWindowSize()
    {
        var monitor = new DpoConvergenceMonitor(threshold: 0.01f, windowSteps: 25);

        monitor.ShouldStop(0.5f);
        monitor.ShouldStop(0.4f);
        monitor.ShouldStop(0.3f);

        Assert.Equal(75, monitor.StepsTaken);

        var normalized = new DpoConvergenceMonitor(threshold: -1f, windowSteps: 0);
        normalized.ShouldStop(0.5f);
        normalized.ShouldStop(0.5f);
        Assert.Equal(2, normalized.StepsTaken);
        Assert.False(normalized.ShouldStop(0.5f));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DpoMetrics field contracts
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Step_WithoutMutationAdmissionFailsBeforeMutationOrArtifactCreation()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var trainer = new DpoTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir);

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 99));

        Assert.Equal(
            DpoMutationAdmissionCapability.RequiredErrorCode,
            exception.ErrorCode);
        AssertModelBitsEqual(before, policy);
        Assert.False(Directory.Exists(_checkpointDir));
    }

    [Fact]
    public void Step_MissingPairAdmissionFailsBeforePermitMutationOrArtifactCreation()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.Step(MakeBatchWithoutAdmission([1, 2, 3], [1, 4, 3]), step: 112));

        Assert.Equal(DpoTrainerPairAdmission.MissingErrorCode, exception.ErrorCode);
        Assert.Equal(0, lease.PermitRequests);
        AssertModelBitsEqual(before, policy);
        AssertNoPublishedArtifacts(_checkpointDir);
    }

    [Fact]
    public void StepMany_MissingPairAdmissionFailsBeforePermitMutationOrArtifactCreation()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.StepMany(
                [MakeBatchWithoutAdmission([1, 2, 3], [1, 4, 3])],
                step: 113));

        Assert.Equal(DpoTrainerPairAdmission.MissingErrorCode, exception.ErrorCode);
        Assert.Equal(0, lease.PermitRequests);
        AssertModelBitsEqual(before, policy);
        AssertNoPublishedArtifacts(_checkpointDir);
    }

    [Fact]
    public void StepVector_MissingPairAdmissionFailsBeforePermitMutationOrArtifactCreation()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.StepVector(
                [new DpoVectorBatch(
                    MakeBatchWithoutAdmission([1, 2, 3], [1, 4, 3]),
                    [1f, 0f])],
                step: 114));

        Assert.Equal(DpoTrainerPairAdmission.MissingErrorCode, exception.ErrorCode);
        Assert.Equal(0, lease.PermitRequests);
        AssertModelBitsEqual(before, policy);
        AssertNoPublishedArtifacts(_checkpointDir);
    }

    [Fact]
    public void Step_PaidChosenPairAdmissionFailsBeforePermitMutationOrArtifactCreation()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.Step(
                MakeBatchWithAdmission(
                    [1, 2, 3],
                    [1, 4, 3],
                    CreatePairAdmission(
                        DpoPairSideAuthorKind.PaidExternalModel,
                        DpoPairSideAuthorKind.User)),
                step: 115));

        Assert.Equal(
            DpoTrainerPairAdmission.PaidExternalModelSideErrorCode,
            exception.ErrorCode);
        Assert.Equal(0, lease.PermitRequests);
        AssertModelBitsEqual(before, policy);
        AssertNoPublishedArtifacts(_checkpointDir);
    }

    [Fact]
    public void StepVector_PaidRejectedPairAdmissionFailsBeforePermitMutationOrArtifactCreation()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.StepVector(
                [new DpoVectorBatch(
                    MakeBatchWithAdmission(
                        [1, 2, 3],
                        [1, 4, 3],
                        CreatePairAdmission(
                            DpoPairSideAuthorKind.OpenTeacher,
                            DpoPairSideAuthorKind.PaidExternalModel)),
                    [1f, 0f])],
                step: 116));

        Assert.Equal(
            DpoTrainerPairAdmission.PaidExternalModelSideErrorCode,
            exception.ErrorCode);
        Assert.Equal(0, lease.PermitRequests);
        AssertModelBitsEqual(before, policy);
        AssertNoPublishedArtifacts(_checkpointDir);
    }

    [Fact]
    public void Step_WithLegalPairAdmissionKeepsExistingMutationPath()
    {
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir);

        var metrics = trainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 117);

        Assert.Equal(117, metrics.Step);
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(117))));
    }

    [Fact]
    public void StepMany_WithLegalPairAdmissionKeepsExistingMutationPath()
    {
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir);

        var metrics = trainer.StepMany(
            [
                MakeBatch([1, 2, 3], [1, 4, 3]),
                MakeBatch([1, 3, 5], [1, 6, 5])
            ],
            step: 118);

        Assert.Equal(118, metrics.Step);
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(118))));
    }

    [Fact]
    public void StepVector_WithLegalPairAdmissionKeepsExistingMutationPath()
    {
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir);

        var metrics = trainer.StepVector(
            [
                new DpoVectorBatch(MakeBatch([1, 2, 3], [1, 4, 3]), [1f, 0f]),
                new DpoVectorBatch(MakeBatch([1, 3, 5], [1, 6, 5]), [0f, 1f])
            ],
            step: 119);

        Assert.Equal(119, metrics.Step);
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(119))));
    }

    [Fact]
    public void StepMany_ValidThenMissingBatchPrevalidatesWholeCollectionBeforeMutation()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.StepMany(
                [
                    MakeBatch([1, 2, 3], [1, 4, 3]),
                    MakeBatchWithoutAdmission([1, 3, 5], [1, 6, 5])
                ],
                step: 120));

        Assert.Equal(DpoTrainerPairAdmission.MissingErrorCode, exception.ErrorCode);
        Assert.Equal(0, lease.PermitRequests);
        AssertModelBitsEqual(before, policy);
        AssertNoPublishedArtifacts(_checkpointDir);
    }

    [Fact]
    public void StepVector_ValidThenMissingBatchPrevalidatesWholeCollectionBeforeMutation()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.StepVector(
                [
                    new DpoVectorBatch(MakeBatch([1, 2, 3], [1, 4, 3]), [1f, 0f]),
                    new DpoVectorBatch(MakeBatchWithoutAdmission([1, 3, 5], [1, 6, 5]), [0f, 1f])
                ],
                step: 132));

        Assert.Equal(DpoTrainerPairAdmission.MissingErrorCode, exception.ErrorCode);
        Assert.Equal(0, lease.PermitRequests);
        AssertModelBitsEqual(before, policy);
        AssertNoPublishedArtifacts(_checkpointDir);
    }

    [Fact]
    public void PairAdmissionFailurePrecedesObservablePermitRequest()
    {
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        _ = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.StepMany(
                [
                    MakeBatch([1, 2, 3], [1, 4, 3]),
                    MakeBatchWithAdmission(
                        [1, 3, 5],
                        [1, 6, 5],
                        CreatePairAdmission(
                            DpoPairSideAuthorKind.OpenTeacher,
                            DpoPairSideAuthorKind.PaidExternalModel))
                ],
                step: 121));

        Assert.Equal(0, lease.PermitRequests);
    }

    [Fact]
    public void Step_WithLegalPairAdmissionButNoMutationCapabilityKeepsOldMutationDeniedBehavior()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var trainer = new DpoTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir);

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 122));

        Assert.Equal(
            DpoMutationAdmissionCapability.RequiredErrorCode,
            exception.ErrorCode);
        AssertModelBitsEqual(before, policy);
        Assert.False(Directory.Exists(_checkpointDir));
    }

    [Fact]
    public void Step_WithLegalPairAdmissionButRevokedPermitKeepsOldMutationDeniedBehavior()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var permitLive = true;
        var lease = new TestDpoMutationAdmissionLease(() =>
        {
            if (permitLive)
                return new CancellationTokenSource();
            throw new DpoMutationAdmissionException(
                "Synthetic revoked DPO mutation permit.",
                "DPO_TEST_MUTATION_PERMIT_REVOKED");
        });
        var trainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        permitLive = false;
        lease.ResetPermitRequests();

        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 123));

        Assert.Equal("DPO_TEST_MUTATION_PERMIT_REVOKED", exception.ErrorCode);
        Assert.Equal(1, lease.PermitRequests);
        AssertModelBitsEqual(before, policy);
        AssertNoPublishedArtifacts(_checkpointDir);
    }

    [Fact]
    public void EvaluateMany_AcceptsMissingPairAdmissionWithoutMutationCapability()
    {
        var trainer = new DpoTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir);

        var metrics = trainer.EvaluateMany(
            [MakeBatchWithoutAdmission([1, 2, 3], [1, 4, 3])],
            step: 124);

        Assert.Equal(124, metrics.Step);
        Assert.False(Directory.Exists(_checkpointDir));
    }

    [Fact]
    public void EvaluateMany_AcceptsPaidPairAdmissionWithoutMutationCapability()
    {
        var trainer = new DpoTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir);

        var metrics = trainer.EvaluateMany(
            [
                MakeBatchWithAdmission(
                    [1, 2, 3],
                    [1, 4, 3],
                    CreatePairAdmission(
                        DpoPairSideAuthorKind.PaidExternalModel,
                        DpoPairSideAuthorKind.User))
            ],
            step: 125);

        Assert.Equal(125, metrics.Step);
        Assert.False(Directory.Exists(_checkpointDir));
    }

    [Fact]
    public void StepMany_EmptyCollectionKeepsArgumentExceptionBeforePermitRequest()
    {
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        Assert.Throws<ArgumentException>(() => trainer.StepMany([], step: 126));

        Assert.Equal(0, lease.PermitRequests);
    }

    [Fact]
    public void StepVector_EmptyCollectionKeepsArgumentExceptionBeforePermitRequest()
    {
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        Assert.Throws<ArgumentException>(() => trainer.StepVector([], step: 127));

        Assert.Equal(0, lease.PermitRequests);
    }

    [Fact]
    public void Step_NullBatchFailsBeforePermitRequest()
    {
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        Assert.Throws<ArgumentNullException>(() => trainer.Step(null!, step: 128));

        Assert.Equal(0, lease.PermitRequests);
    }

    [Fact]
    public void StepMany_NullCollectionKeepsNullReferenceBeforePermitRequest()
    {
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        Assert.Throws<NullReferenceException>(() =>
            trainer.StepMany(null!, step: 129));

        Assert.Equal(0, lease.PermitRequests);
    }

    [Fact]
    public void StepVector_NullCollectionKeepsNullReferenceBeforePermitRequest()
    {
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        Assert.Throws<NullReferenceException>(() =>
            trainer.StepVector(null!, step: 130));

        Assert.Equal(0, lease.PermitRequests);
    }

    [Fact]
    public void StepVector_NullNestedBatchFailsBeforePermitRequest()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var lease = new TestDpoMutationAdmissionLease();
        var trainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            mutationLease: lease);
        lease.ResetPermitRequests();

        // null! здесь намеренно моделирует runtime legacy/interop ошибку:
        // nested Batch должен упасть на prevalidation, а не после permit.
        var exception = Assert.Throws<ArgumentNullException>(() =>
            trainer.StepVector(
                [new DpoVectorBatch(null!, [1f])],
                step: 133));

        Assert.Equal("batch", exception.ParamName);
        Assert.Equal(0, lease.PermitRequests);
        AssertModelBitsEqual(before, policy);
        AssertNoPublishedArtifacts(_checkpointDir);
    }

    [Fact]
    public void ExternalHistoricalExperimentRunnerRemainsAuthorlessAndStopsAtTrainerDoor()
    {
        var source = File.ReadAllText(Path.Combine(
            ResolveRepoRoot(),
            "experiments-v2",
            "INTERNAL-TRAIN-READY-V191",
            "runner",
            "InternalTrainReadyDpoRunner",
            "Program.cs"));
        var trainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir);

        // Исторический runner остается authorless: чинить его здесь нельзя,
        // поэтому первый реальный train-call упирается в дверь тренера.
        Assert.Contains("trainer.StepMany(dpoBatches", source, StringComparison.Ordinal);
        Assert.Contains("return new DpoBatch", source, StringComparison.Ordinal);
        Assert.DoesNotContain("DpoTrainerPairCustoms", source, StringComparison.Ordinal);
        var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
            trainer.StepMany(
                [MakeBatchWithoutAdmission([1, 2, 3], [1, 4, 3])],
                step: 131));
        Assert.Equal(DpoTrainerPairAdmission.MissingErrorCode, exception.ErrorCode);
    }

    [Fact]
    public void Step_ReturnsMetricsWithCorrectStep()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3, 4 }, new[] { 1, 5, 6, 4 });

        var metrics = trainer.Step(batch, step: 7);

        Assert.Equal(7, metrics.Step);
    }

    [Fact]
    public void Step_LossIsPositive()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3 }, new[] { 1, 5, 3 });

        var metrics = trainer.Step(batch, step: 0);

        // DPO loss = -log σ(margin) which is always in (0, log2] for any real margin
        Assert.True(metrics.Loss > 0f, $"Loss should be positive, got {metrics.Loss}");
        Assert.False(float.IsNaN(metrics.Loss), "Loss must not be NaN");
    }

    [Fact]
    public void Step_AccuracyIsBinary()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3 }, new[] { 1, 5, 3 });

        var metrics = trainer.Step(batch, step: 0);

        // Accuracy is 1.0 if margin>0 else 0.0 — never anything else
        Assert.True(metrics.Accuracy is 0f or 1f,
            $"Accuracy must be 0 or 1, got {metrics.Accuracy}");
    }

    [Fact]
    public void Step_RewardMargin_MatchesChosenMinusRejected()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 2, 3 }, new[] { 5, 6 });

        var metrics = trainer.Step(batch, step: 0);

        float expected = metrics.ChosenReward - metrics.RejectedReward;
        Assert.Equal(expected, metrics.RewardMargin, precision: 5);
    }

    [Fact]
    public void Step_AccuracyConsistentWithMargin()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3 }, new[] { 4, 5, 6 });

        var metrics = trainer.Step(batch, step: 0);

        // Accuracy = 1 iff margin > 0 (by definition in DpoTrainer)
        if (metrics.RewardMargin > 0f)
            Assert.Equal(1f, metrics.Accuracy);
        else
            Assert.Equal(0f, metrics.Accuracy);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Atomic checkpoint (DNA rule 8 + DNA rule 7)
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Step_WritesMetricsFileAtomically()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2 }, new[] { 3, 4 });

        trainer.Step(batch, step: 42);

        // Metrics file must exist, .tmp must be gone
        string metricsPath = Path.Combine(_checkpointDir, "dpo_step_000042.json");
        Assert.True(File.Exists(metricsPath), "Metrics file must be written");
        Assert.False(File.Exists(metricsPath + ".tmp"), ".tmp file must not be left behind");
    }

    [Fact]
    public void Step_MetricsFileContainsValidJson()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2 }, new[] { 3, 4 });

        var metrics = trainer.Step(batch, step: 1);
        string json = File.ReadAllText(Path.Combine(_checkpointDir, "dpo_step_000001.json"));

        // Must deserialize without error and match the returned metrics
        var loaded = System.Text.Json.JsonSerializer.Deserialize<DpoMetrics>(json)!;
        Assert.Equal(metrics.Step, loaded.Step);
        Assert.Equal(metrics.Loss, loaded.Loss, precision: 5);
    }

    [Fact]
    public void Step_WritesLoadablePolicyCheckpointAtomically()
    {
        var model = new LlmModel(SmallCfg);
        var refMdl = new LlmModel(SmallCfg);
        SetDeterministicWeights(model);
        CopyWeights(model, refMdl);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch = MakeBatch(new[] { 1, 2, 3, 4 }, new[] { 1, 5, 6, 4 });

        trainer.Step(batch, step: 42);

        var checkpointPath = Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(42));
        Assert.True(File.Exists(checkpointPath), "Policy checkpoint must be written by the real DPO trainer.");
        Assert.False(File.Exists(checkpointPath + ".tmp"), ".tmp policy checkpoint must not be left behind.");

        var loaded = new LlmModel(SmallCfg);
        loaded.LoadCheckpoint(checkpointPath);

        AssertModelParametersEqual(model, loaded);
    }

    [Fact]
    public void Step_ExistingArtifactPairFailsBeforeMutationAndPreservesPublishedBytes()
    {
        var model = new LlmModel(SmallCfg);
        var reference = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, reference, _checkpointDir);
        var batch = MakeBatch([1, 2, 3], [1, 4, 3]);

        trainer.Step(batch, step: 101);
        var policyPath = Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(101));
        var metricsPath = Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(101));
        var policyBytes = File.ReadAllBytes(policyPath);
        var metricsBytes = File.ReadAllBytes(metricsPath);
        var parametersBeforeRejectedStep = SnapshotModelBits(model);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            trainer.Step(batch, step: 101));

        Assert.StartsWith(DpoTrainer.CheckpointCollisionErrorCode, exception.Message);
        AssertModelBitsEqual(parametersBeforeRejectedStep, model);
        Assert.Equal(policyBytes, File.ReadAllBytes(policyPath));
        Assert.Equal(metricsBytes, File.ReadAllBytes(metricsPath));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Step_LonePreexistingArtifactFailsBeforeMutation(bool policyOnly)
    {
        var model = new LlmModel(SmallCfg);
        var reference = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, reference, _checkpointDir);
        var batch = MakeBatch([1, 2, 3], [1, 4, 3]);
        var step = policyOnly ? 102 : 103;
        var existingPath = Path.Combine(
            _checkpointDir,
            policyOnly
                ? DpoTrainer.BuildPolicyCheckpointFileName(step)
                : DpoTrainer.BuildMetricsFileName(step));
        File.WriteAllBytes(existingPath, [7, 8, 9]);
        var parametersBeforeRejectedStep = SnapshotModelBits(model);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            trainer.Step(batch, step));

        Assert.StartsWith(DpoTrainer.CheckpointIncompleteErrorCode, exception.Message);
        AssertModelBitsEqual(parametersBeforeRejectedStep, model);
        Assert.Equal([7, 8, 9], File.ReadAllBytes(existingPath));
    }

    [Fact]
    public void Step_ReusesReleasedPersistentLockWithoutTreatingItAsAPoisonFile()
    {
        Directory.CreateDirectory(_checkpointDir);
        var lockPath = Path.Combine(
            _checkpointDir,
            ".dpo_step_000104.lock");
        File.WriteAllBytes(lockPath, [1]);
        var firstTrainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir);

        firstTrainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 104);
        var secondTrainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir);
        secondTrainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 108);

        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(104))));
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(104))));
        Assert.True(File.Exists(lockPath));
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(108))));
    }

    [Fact]
    public void Step_MetricsCommitCollisionDoesNotDeleteForeignPolicyReplacement()
    {
        var model = new LlmModel(SmallCfg);
        var policyPath = Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(105));
        var metricsPath = Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(105));

        byte[] foreignMetrics = [3, 1, 4];
        byte[] foreignStaging = [5, 9, 2];
        byte[]? byteIdenticalForeignPolicy = null;
        string? replacedStagingPath = null;
        var trainer = CreateAdmittedTrainer(
            model,
            new LlmModel(SmallCfg),
            _checkpointDir,
            learningRate: 1e-5f,
            new DpoTrainerPublicationObserver
            {
                AfterPolicyFinalMove = () =>
                {
                    byteIdenticalForeignPolicy = File.ReadAllBytes(policyPath);
                    File.Delete(policyPath);
                    File.WriteAllBytes(policyPath, byteIdenticalForeignPolicy);
                    replacedStagingPath = Assert.Single(
                        Directory.EnumerateFiles(_checkpointDir, "*.staging"));
                    File.Delete(replacedStagingPath);
                    File.WriteAllBytes(replacedStagingPath, foreignStaging);
                    File.WriteAllBytes(metricsPath, foreignMetrics);
                }
            });

        var exception = Assert.Throws<IOException>(() =>
        {
            trainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 105);
        });

        Assert.NotEmpty(exception.Message);
        Assert.Equal(byteIdenticalForeignPolicy, File.ReadAllBytes(policyPath));
        Assert.Equal(foreignMetrics, File.ReadAllBytes(metricsPath));
        Assert.Equal(foreignStaging, File.ReadAllBytes(replacedStagingPath!));
        Assert.Single(Directory.EnumerateFiles(_checkpointDir, "*.staging"));
        AssertTrainerOwnedReservationReleased(_checkpointDir);

        var retryPolicy = new LlmModel(SmallCfg);
        var retryBefore = SnapshotModelBits(retryPolicy);
        var retry = CreateAdmittedTrainer(retryPolicy, new LlmModel(SmallCfg), _checkpointDir);
        var retryException = Assert.Throws<InvalidOperationException>(() =>
            retry.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 105));
        Assert.StartsWith(DpoTrainer.CheckpointCollisionErrorCode, retryException.Message);
        AssertModelBitsEqual(retryBefore, retryPolicy);
    }

    [Fact]
    public void Step_MetricsCommitFailureLeavesIncompleteEvidenceAndRejectsRetryBeforeMutation()
    {
        var model = new LlmModel(SmallCfg);
        var policyPath = Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(107));
        var metricsPath = Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(107));
        var trainer = CreateAdmittedTrainer(
            model,
            new LlmModel(SmallCfg),
            _checkpointDir,
            learningRate: 1e-5f,
            new DpoTrainerPublicationObserver
            {
                AfterPolicyFinalMove = () =>
                    throw new IOException("synthetic metrics commit failure")
            });

        var publicationFailure = Assert.Throws<IOException>(() =>
        {
            trainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 107);
        });

        Assert.Equal("synthetic metrics commit failure", publicationFailure.Message);
        Assert.True(File.Exists(policyPath));
        Assert.False(File.Exists(metricsPath));
        Assert.Single(Directory.EnumerateFiles(_checkpointDir, "*.staging"));
        AssertTrainerOwnedReservationReleased(_checkpointDir);

        var retryPolicy = new LlmModel(SmallCfg);
        var retryBefore = SnapshotModelBits(retryPolicy);
        var retry = CreateAdmittedTrainer(retryPolicy, new LlmModel(SmallCfg), _checkpointDir);
        var retryException = Assert.Throws<InvalidOperationException>(() =>
            retry.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 107));
        Assert.StartsWith(DpoTrainer.CheckpointIncompleteErrorCode, retryException.Message);
        AssertModelBitsEqual(retryBefore, retryPolicy);
    }

    [Fact]
    public void Step_FailedPublicationBeforeFinalNameRestoresPolicyLoRAAndOptimizerForCleanRetry()
    {
        var failedPolicy = new LlmModel(SmallCfg);
        var failedReference = new LlmModel(SmallCfg);
        SetDeterministicWeights(failedPolicy);
        CopyWeights(failedPolicy, failedReference);
        _ = LoraApplicator.Apply(
            failedPolicy,
            new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0f, TargetModules = ["q_proj"] });
        var controlPolicy = new LlmModel(SmallCfg);
        var controlReference = new LlmModel(SmallCfg);
        _ = LoraApplicator.Apply(
            controlPolicy,
            new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0f, TargetModules = ["q_proj"] });
        CopyWeights(failedPolicy, controlPolicy);
        CopyWeights(failedReference, controlReference);
        var firstBatch = MakeBatch([1, 2, 3, 4], [1, 5, 6, 4]);
        var secondBatch = MakeBatch([1, 7, 8, 4], [1, 9, 10, 4]);
        var failure = new IOException("synthetic pre-final publication failure");
        var failSecondPublication = false;
        var failingTrainer = CreateAdmittedTrainer(
            failedPolicy,
            failedReference,
            _checkpointDir,
            learningRate: 1e-2f,
            new DpoTrainerPublicationObserver
            {
                ArtifactOwnershipProven = _ => { },
                BeforePolicySerialization = _ =>
                {
                    if (failSecondPublication)
                        throw failure;
                }
            });
        var controlDir = Path.Combine(_checkpointDir, "control");
        var controlTrainer = CreateAdmittedTrainer(
            controlPolicy,
            controlReference,
            controlDir,
            learningRate: 1e-2f);

        failingTrainer.Step(firstBatch, step: 1);
        controlTrainer.Step(firstBatch, step: 1);
        var beforeFailedSecondAttempt = SnapshotModelBits(failedPolicy);
        controlTrainer.Step(secondBatch, step: 2);

        failSecondPublication = true;
        var observed = Assert.Throws<IOException>(() =>
            failingTrainer.Step(secondBatch, step: 2));

        Assert.Same(failure, observed);
        AssertModelBitsEqual(beforeFailedSecondAttempt, failedPolicy);
        Assert.False(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(2))));
        Assert.False(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(2))));
        Assert.Empty(Directory.EnumerateFiles(_checkpointDir, "*.staging"));

        failSecondPublication = false;
        failingTrainer.Step(secondBatch, step: 2);

        AssertModelParametersEqual(controlPolicy, failedPolicy);
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(2))));
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(2))));
    }

    [Fact]
    public void Step_CompletedPublicationReservationReleaseFailureDoesNotRollbackPolicy()
    {
        var observedPolicy = new LlmModel(SmallCfg);
        var observedReference = new LlmModel(SmallCfg);
        SetDeterministicWeights(observedPolicy);
        CopyWeights(observedPolicy, observedReference);
        var controlPolicy = new LlmModel(SmallCfg);
        var controlReference = new LlmModel(SmallCfg);
        CopyWeights(observedPolicy, controlPolicy);
        CopyWeights(observedReference, controlReference);
        var batch = MakeBatch([1, 2, 3, 4], [1, 5, 6, 4]);
        var failingTrainer = CreateAdmittedTrainer(
            observedPolicy,
            observedReference,
            _checkpointDir,
            learningRate: 1e-2f,
            new DpoTrainerPublicationObserver
            {
                BeforeReservationUnlockForTesting = () =>
                    throw new IOException("synthetic reservation release failure")
            });
        var controlTrainer = CreateAdmittedTrainer(
            controlPolicy,
            controlReference,
            Path.Combine(_checkpointDir, "control"),
            learningRate: 1e-2f);

        var exception = Assert.Throws<IOException>(() =>
            failingTrainer.Step(batch, step: 4));
        controlTrainer.Step(batch, step: 4);

        Assert.StartsWith(
            DpoTrainer.CheckpointReservationReleaseFailedErrorCode,
            exception.Message);
        Assert.Equal(
            "synthetic reservation release failure",
            exception.InnerException!.Message);
        AssertModelParametersEqual(controlPolicy, observedPolicy);
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(4))));
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(4))));
    }

    [Fact]
    public async Task ConcurrentSameStep_AllowsAtMostOnePublicationAndDoesNotMutateTheRejectedPolicy()
    {
        var winnerPolicy = new LlmModel(SmallCfg);
        var loserPolicy = new LlmModel(SmallCfg);
        var loserBefore = SnapshotModelBits(loserPolicy);
        var reserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var winnerTrainer = CreateAdmittedTrainer(
            winnerPolicy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            learningRate: 1e-5f,
            new DpoTrainerPublicationObserver
            {
                AfterReservationAcquired = () =>
                {
                    reserved.TrySetResult();
                    release.Task.GetAwaiter().GetResult();
                }
            });
        var loserTrainer = CreateAdmittedTrainer(
            loserPolicy,
            new LlmModel(SmallCfg),
            _checkpointDir);

        var winner = Task.Run(() =>
        {
            winnerTrainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 106);
        });
        await reserved.Task;

        var rejected = Assert.Throws<InvalidOperationException>(() =>
            loserTrainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 106));
        Assert.StartsWith(DpoTrainer.CheckpointReservationActiveErrorCode, rejected.Message);
        AssertModelBitsEqual(loserBefore, loserPolicy);

        release.TrySetResult();
        await winner;
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(106))));
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(106))));
        Assert.Empty(Directory.EnumerateFiles(_checkpointDir, "*.staging"));
        AssertTrainerOwnedReservationReleased(_checkpointDir);
    }

    [Fact]
    public async Task ConcurrentDifferentStepRejectsOverlapOnSamePolicyBeforeReservation()
    {
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        var reserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstTrainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir,
            learningRate: 1e-5f,
            new DpoTrainerPublicationObserver
            {
                AfterReservationAcquired = () =>
                {
                    reserved.TrySetResult();
                    release.Task.GetAwaiter().GetResult();
                }
            });
        var secondTrainer = CreateAdmittedTrainer(
            policy,
            new LlmModel(SmallCfg),
            _checkpointDir);

        var first = Task.Run(() =>
            firstTrainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 112));
        try
        {
            await reserved.Task.WaitAsync(TimeSpan.FromSeconds(20));

            var exception = Assert.Throws<InvalidOperationException>(() =>
                secondTrainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 113));

            Assert.StartsWith(DpoTrainer.PolicyMutationActiveErrorCode, exception.Message);
            AssertModelBitsEqual(before, policy);
            Assert.False(File.Exists(Path.Combine(
                _checkpointDir,
                ".dpo_step_000113.lock")));
            Assert.False(File.Exists(Path.Combine(
                _checkpointDir,
                DpoTrainer.BuildPolicyCheckpointFileName(113))));
            Assert.False(File.Exists(Path.Combine(
                _checkpointDir,
                DpoTrainer.BuildMetricsFileName(113))));
        }
        finally
        {
            release.TrySetResult();
        }

        await first.WaitAsync(TimeSpan.FromSeconds(20));
    }

    [Fact]
    public async Task ProcessLocalReservationGateRejectsOverlapBeforeTheFilesystemLockIsAcquired()
    {
        var gateEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var firstTrainer = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir,
            learningRate: 1e-5f,
            new DpoTrainerPublicationObserver
            {
                AfterProcessLocalGateAcquired = () =>
                {
                    gateEntered.TrySetResult();
                    release.Task.GetAwaiter().GetResult();
                }
            });
        var secondPolicy = new LlmModel(SmallCfg);
        var secondBefore = SnapshotModelBits(secondPolicy);
        var secondTrainer = CreateAdmittedTrainer(
            secondPolicy,
            new LlmModel(SmallCfg),
            _checkpointDir);

        var first = Task.Run(() =>
            firstTrainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 109));
        await gateEntered.Task;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            secondTrainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 109));
        Assert.StartsWith(DpoTrainer.CheckpointReservationActiveErrorCode, exception.Message);
        AssertModelBitsEqual(secondBefore, secondPolicy);

        release.TrySetResult();
        await first;
    }

    [Fact]
    public void FilesystemByteRangeLockRejectsExternalHolderBeforePolicyMutation()
    {
        if (OperatingSystem.IsMacOS())
            return;

        Directory.CreateDirectory(_checkpointDir);
        var lockPath = Path.Combine(_checkpointDir, ".dpo_step_000110.lock");
        var policy = new LlmModel(SmallCfg);
        var before = SnapshotModelBits(policy);
        using (var externalLock = new FileStream(
                   lockPath,
                   FileMode.OpenOrCreate,
                   FileAccess.ReadWrite,
                   FileShare.ReadWrite | FileShare.Delete))
        {
            externalLock.Lock(0, 1);
            var trainer = CreateAdmittedTrainer(policy, new LlmModel(SmallCfg), _checkpointDir);
            var exception = Assert.Throws<InvalidOperationException>(() =>
                trainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 110));
            Assert.StartsWith(DpoTrainer.CheckpointReservationActiveErrorCode, exception.Message);
            AssertModelBitsEqual(before, policy);
            externalLock.Unlock(0, 1);
        }

        var recovered = CreateAdmittedTrainer(policy, new LlmModel(SmallCfg), _checkpointDir);
        recovered.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 110);
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(110))));
    }

    [Fact]
    public void InstanceScopedPublicationObserverDoesNotLeakToAnotherTrainer()
    {
        var observed = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir,
            learningRate: 1e-5f,
            new DpoTrainerPublicationObserver
            {
                AfterReservationAcquired = () =>
                    throw new InvalidOperationException("observer-scoped-failure")
            });

        var observedException = Assert.Throws<InvalidOperationException>(() =>
            observed.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 111));
        Assert.Equal("observer-scoped-failure", observedException.Message);

        var unobserved = CreateAdmittedTrainer(
            new LlmModel(SmallCfg),
            new LlmModel(SmallCfg),
            _checkpointDir);
        unobserved.Step(MakeBatch([1, 2, 3], [1, 4, 3]), step: 111);
        Assert.True(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildMetricsFileName(111))));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Multiple steps — model updates
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void MultipleSteps_DoNotThrow()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2, 3 }, new[] { 4, 5, 6 });

        var ex = Record.Exception(() =>
        {
            for (int i = 0; i < 3; i++)
                trainer.Step(batch, step: i);
        });

        Assert.Null(ex);
    }

    [Fact]
    public void MultipleSteps_EachWritesOwnMetricsFile()
    {
        var model   = new LlmModel(SmallCfg);
        var refMdl  = new LlmModel(SmallCfg);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir);
        var batch   = MakeBatch(new[] { 1, 2 }, new[] { 3, 4 });

        for (int i = 0; i < 3; i++)
            trainer.Step(batch, step: i);

        // One metrics file per step
        var files = Directory.GetFiles(_checkpointDir, "*.json");
        Assert.Equal(3, files.Length);
    }

    [Fact]
    public void StepMany_WithCopiedReferenceImprovesRewardMarginOnRepeatedPreference()
    {
        var model = new LlmModel(SmallCfg);
        var refMdl = new LlmModel(SmallCfg);
        SetDeterministicWeights(model);
        CopyWeights(model, refMdl);
        var trainer = CreateAdmittedTrainer(model, refMdl, _checkpointDir, learningRate: 5e-2f);
        var batch = MakeBatch(
            new[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            new[] { 1, 9, 10, 11, 12, 13, 14, 8 });

        var initial = trainer.EvaluateMany([batch], step: 0);
        for (var step = 1; step <= 40; step++)
            trainer.StepMany([batch], step);
        var trained = trainer.EvaluateMany([batch], step: 41);

        Assert.Equal(0f, initial.RewardMargin, precision: 5);
        Assert.True(
            trained.RewardMargin > initial.RewardMargin + 1e-5f,
            $"Expected reward margin to improve from {initial.RewardMargin}, got {trained.RewardMargin}.");
    }

    [Fact]
    public void DpoTrainer_StepUsesEachPolicyTrainingGraphExactlyOnce()
    {
        var policy = new LlmModel(SmallCfg);
        var reference = new LlmModel(SmallCfg);
        RecordingAdapter? recorder = null;
        policy.ExecuteAdapterMutation(lease =>
        {
            var state = lease.InspectProjection(0, "q_proj");
            recorder = new RecordingAdapter(state.BaseProjection);
            lease.PopulateAdapterPair(0, "q_proj", recorder);
            return 0;
        });
        foreach (var parameter in policy.BaseParameters)
            parameter.Value.RequiresGrad = false;
        var trainer = CreateAdmittedTrainer(policy, reference, _checkpointDir);

        _ = trainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), 70);

        Assert.Equal([true, true], recorder!.ObservedModes);
        Assert.DoesNotContain(false, recorder.ObservedModes);
    }

    [Fact]
    public void DpoTrainer_EvaluateManyKeepsPolicyScoringInInferenceMode()
    {
        var policy = new LlmModel(SmallCfg);
        var reference = new LlmModel(SmallCfg);
        RecordingAdapter? recorder = null;
        policy.ExecuteAdapterMutation(lease =>
        {
            var state = lease.InspectProjection(0, "q_proj");
            recorder = new RecordingAdapter(state.BaseProjection);
            lease.PopulateAdapterPair(0, "q_proj", recorder);
            return 0;
        });
        var trainer = new DpoTrainer(policy, reference, _checkpointDir);

        _ = trainer.EvaluateMany([MakeBatch([1, 2, 3], [1, 4, 3])], 71);

        Assert.Equal([false, false], recorder!.ObservedModes);
    }

    [Fact]
    public void DpoTrainer_StaleFrozenBaseGradientCannotMoveBaseWhileNamedAdapterBChanges()
    {
        var policy = new LlmModel(SmallCfg);
        var reference = new LlmModel(SmallCfg);
        SetDeterministicWeights(policy);
        CopyWeights(policy, reference);
        var adapter = Assert.Single(LoraApplicator.Apply(
            policy,
            new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj"] }));
        var adapterA = adapter.AdapterParameters[0].Parameter;
        var adapterB = adapter.AdapterParameters[1].Parameter;
        Array.Fill(adapterA.Value.Data, 0.25f);
        Array.Clear(adapterB.Value.Data);
        var namedB = Assert.Single(
            policy.GetCheckpointParameterEntries(),
            entry => entry.Key == "lora.block_0000.q_proj.lora_b");
        Assert.Same(adapterB, namedB.Parameter);

        var staleBase = policy.BaseParameters[2];
        staleBase.Value.Grad = new Tensor(
            Enumerable.Repeat(3f, staleBase.Value.Data.Length).ToArray(),
            staleBase.Value.Shape);
        var baseBefore = policy.BaseParameters
            .Select(parameter => parameter.Value.Data.ToArray())
            .ToArray();
        var bBefore = adapterB.Value.Data.ToArray();
        var trainer = CreateAdmittedTrainer(policy, reference, _checkpointDir, learningRate: 1e-2f);

        _ = trainer.Step(MakeBatch([1, 2, 3, 4], [1, 5, 6, 4]), 71);

        for (var index = 0; index < policy.BaseParameters.Count; index++)
            Assert.Equal(baseBefore[index], policy.BaseParameters[index].Value.Data);
        var bDelta = adapterB.Value.Data
            .Zip(bBefore, (after, before) => after - before)
            .ToArray();
        Assert.All(adapterB.Value.Data, value => Assert.True(float.IsFinite(value)));
        Assert.All(bDelta, value => Assert.True(float.IsFinite(value)));
        Assert.Contains(bDelta, value => MathF.Abs(value) > 1e-8f);
    }

    [Fact]
    public void DpoTrainer_AllThreeMutatingApisFilterStaleFrozenGradient()
    {
        VerifyStaleGradientFilter((trainer, batch) => trainer.Step(batch, 80));
        VerifyStaleGradientFilter((trainer, batch) => trainer.StepMany([batch, batch], 81));
        VerifyStaleGradientFilter((trainer, batch) =>
            trainer.StepVector([new DpoVectorBatch(batch, [1f, 0f], 1f)], 82));
    }

    [Fact]
    public void DpoTrainer_AdapterCheckpointIsKeyedAndLoadsIntoIdenticalAttachment()
    {
        var policy = new LlmModel(SmallCfg);
        var reference = new LlmModel(SmallCfg);
        _ = LoraApplicator.Apply(
            policy,
            new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] });
        var trainer = CreateAdmittedTrainer(policy, reference, _checkpointDir);

        _ = trainer.Step(MakeBatch([1, 2, 3], [1, 4, 3]), 90);
        var path = Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(90));
        using (var reader = new BinaryReader(File.OpenRead(path)))
        {
            Assert.Equal(unchecked((int)0xC14F5241), reader.ReadInt32());
            Assert.Equal(2, reader.ReadInt32());
            Assert.Equal(1, reader.ReadInt32());
            Assert.Equal(32, reader.ReadInt32());
        }

        var loaded = new LlmModel(SmallCfg);
        _ = LoraApplicator.Apply(
            loaded,
            new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] });
        loaded.LoadCheckpoint(path);
        AssertModelParametersEqual(policy, loaded);
    }

    private void VerifyStaleGradientFilter(Action<DpoTrainer, DpoBatch> execute)
    {
        var policy = new LlmModel(SmallCfg);
        var reference = new LlmModel(SmallCfg);
        _ = LoraApplicator.Apply(
            policy,
            new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj"] });
        var staleBase = policy.BaseParameters[2];
        staleBase.Value.Grad = new Tensor(
            Enumerable.Repeat(9f, staleBase.Value.Data.Length).ToArray(),
            staleBase.Value.Shape);
        var before = staleBase.Value.Data.ToArray();
        var trainer = CreateAdmittedTrainer(policy, reference, _checkpointDir);

        execute(trainer, MakeBatch([1, 2, 3], [1, 4, 3]));

        Assert.Equal(before, staleBase.Value.Data);
    }

    private static DpoTrainer CreateAdmittedTrainer(
        LlmModel policy,
        LlmModel reference,
        string checkpointDir,
        float learningRate = 1e-5f,
        DpoTrainerPublicationObserver? publicationObserver = null,
        TestDpoMutationAdmissionLease? mutationLease = null)
    {
        return new DpoTrainer(
            policy,
            reference,
            checkpointDir,
            DpoMutationAdmissionCapability.Issue(
                mutationLease ?? new TestDpoMutationAdmissionLease()),
            learningRate,
            publicationObserver);
    }

    private static void AssertTrainerOwnedReservationReleased(
        string checkpointDir)
    {
        var reservationFiles = Directory
            .EnumerateFiles(checkpointDir, "*.lock")
            .ToArray();
        if (OperatingSystem.IsWindows())
            Assert.Empty(reservationFiles);
        else
            Assert.Single(reservationFiles);
    }

    private static void AssertNoPublishedArtifacts(string checkpointDir)
    {
        if (!Directory.Exists(checkpointDir))
            return;

        Assert.Empty(Directory.EnumerateFiles(
            checkpointDir,
            "*.bin",
            SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(
            checkpointDir,
            "*.json",
            SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(
            checkpointDir,
            "*.staging",
            SearchOption.AllDirectories));
    }

    private static string ResolveRepoRoot()
    {
        var candidate = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."));
        if (Directory.Exists(Path.Combine(candidate, "11.DPO")) &&
            Directory.Exists(Path.Combine(candidate, "experiments-v2")))
        {
            return candidate;
        }

        throw new InvalidOperationException(
            "DPO trainer tests cannot resolve repository root.");
    }

    private sealed class TestDpoMutationAdmissionLease(
        Func<IDisposable?>? acquireMutationPermit = null)
        : IDpoMutationAdmissionIssuer
    {
        private readonly Func<IDisposable?> _acquireMutationPermit =
            acquireMutationPermit ?? (static () => new CancellationTokenSource());

        public int PermitRequests { get; private set; }

        public void ResetPermitRequests()
            => PermitRequests = 0;

        IDisposable IDpoMutationAdmissionIssuer
            .AcquireDpoTrainerMutationPermit()
        {
            PermitRequests++;
            return _acquireMutationPermit()!;
        }

        public void Dispose()
        {
        }
    }

    private static void AssertModelParametersEqual(LlmModel expected, LlmModel actual)
    {
        var expectedParameters = expected.Parameters().ToArray();
        var actualParameters = actual.Parameters().ToArray();
        Assert.Equal(expectedParameters.Length, actualParameters.Length);

        for (var parameterIndex = 0; parameterIndex < expectedParameters.Length; parameterIndex++)
        {
            var expectedData = expectedParameters[parameterIndex].Value.Data;
            var actualData = actualParameters[parameterIndex].Value.Data;
            Assert.Equal(expectedData.Length, actualData.Length);

            for (var valueIndex = 0; valueIndex < expectedData.Length; valueIndex++)
                Assert.Equal(expectedData[valueIndex], actualData[valueIndex]);
        }
    }

    private static int[][] SnapshotModelBits(LlmModel model)
        => model.Parameters()
            .Select(parameter => parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();

    private static void AssertModelBitsEqual(int[][] expected, LlmModel actual)
    {
        var current = SnapshotModelBits(actual);
        Assert.Equal(expected.Length, current.Length);
        for (var index = 0; index < expected.Length; index++)
            Assert.Equal(expected[index], current[index]);
    }

    private static void SetDeterministicWeights(LlmModel model)
    {
        var rng = new Random(42);
        var parameters = model.Parameters().ToArray();
        FillNormal(parameters[0].Value.Data, rng, std: 0.02f);
        FillOnes(parameters[1].Value.Data);
        FillKaiming(parameters[2].Value.Data, rng, parameters[2].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[3].Value.Data, rng, parameters[3].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[4].Value.Data, rng, parameters[4].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[5].Value.Data, rng, parameters[5].Value.Shape.Dimensions[1]);
        FillOnes(parameters[6].Value.Data);
        FillKaiming(parameters[7].Value.Data, rng, parameters[7].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[8].Value.Data, rng, parameters[8].Value.Shape.Dimensions[1]);
        FillKaiming(parameters[9].Value.Data, rng, parameters[9].Value.Shape.Dimensions[1]);
        FillOnes(parameters[10].Value.Data);
        FillKaiming(parameters[11].Value.Data, rng, parameters[11].Value.Shape.Dimensions[1]);
    }

    private static void FillKaiming(float[] data, Random rng, int fanIn)
    {
        var bound = MathF.Sqrt(6.0f / fanIn);
        for (var i = 0; i < data.Length; i++)
            data[i] = (float)(rng.NextDouble() * 2 * bound - bound);
    }

    private static void FillNormal(float[] data, Random rng, float std)
    {
        for (var i = 0; i < data.Length; i += 2)
        {
            var u1 = 1.0 - rng.NextDouble();
            var u2 = 1.0 - rng.NextDouble();
            var r = Math.Sqrt(-2.0 * Math.Log(u1));
            var theta = 2.0 * Math.PI * u2;

            data[i] = (float)(r * Math.Cos(theta) * std);
            if (i + 1 < data.Length)
                data[i + 1] = (float)(r * Math.Sin(theta) * std);
        }
    }

    private static void FillOnes(float[] data)
    {
        Array.Fill(data, 1.0f);
    }

    private static void CopyWeights(LlmModel source, LlmModel target)
    {
        var sourceParameters = source.Parameters().ToArray();
        var targetParameters = target.Parameters().ToArray();
        for (var parameterIndex = 0; parameterIndex < Math.Min(sourceParameters.Length, targetParameters.Length); parameterIndex++)
        {
            var sourceData = sourceParameters[parameterIndex].Value.Data;
            var targetData = targetParameters[parameterIndex].Value.Data;
            Array.Copy(sourceData, targetData, Math.Min(sourceData.Length, targetData.Length));
        }
    }

    private sealed class RecordingAdapter : Module, IAttentionProjectionAdapter
    {
        public Linear BaseProjection { get; }
        public Module RegistrationModule => this;
        public IReadOnlyList<(string Suffix, Parameter Parameter)> AdapterParameters => [];
        public List<bool> ObservedModes { get; } = [];

        public RecordingAdapter(Linear baseProjection)
        {
            BaseProjection = baseProjection;
        }

        public Tensor Forward(Tensor input, bool training)
        {
            ObservedModes.Add(training);
            return BaseProjection.Forward(input);
        }
    }
}
