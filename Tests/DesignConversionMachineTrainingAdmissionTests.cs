// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using DPO.Models;
using TensorCore.Core;

public sealed class DesignConversionMachineTrainingAdmissionTests
{
    private const string Chosen =
        "{\"html\":\"<main>chosen</main>\",\"css\":\"main{color:green}\"}";
    private const string Rejected =
        "{\"html\":\"<main>rejected</main>\",\"css\":\"main{color:red}\"}";

    [Fact]
    public async Task MatchingFinalBatchAndLease_MintsAndMutatesThroughAdmittedEntry()
    {
        var fixture = CreateFixture("matching");
        var entries = new List<string>();
        var previous = DesignConversionMachine.TrainingEntryObserverForTesting;
        DesignConversionMachine.TrainingEntryObserverForTesting = entries.Add;
        try
        {
            var admission = DesignConversionMachineTrainingAdmission.Mint(
                fixture.Final,
                fixture.Batch,
                fixture.Lease);

            Assert.True(admission.IsSuccess, admission.ErrorMessage);
            var result = await new DesignConversionMachine()
                .TrainAdmittedBatchAsync(admission.Value!);

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal("matching:batch", result.Value!.BatchId);
            Assert.Equal(["admitted"], entries);
        }
        finally
        {
            DesignConversionMachine.TrainingEntryObserverForTesting = previous;
            fixture.Lease.Dispose();
        }
    }

    [Fact]
    public void ContentALease_CannotMintContentB()
    {
        var first = CreateFixture("content-a");
        var second = CreateFinal(
            "content-b",
            [CreateItem("content-b:1", Rejected, Chosen)]);
        try
        {
            var result = DesignConversionMachineTrainingAdmission.Mint(
                second,
                CreateBatch("content-b", second.Inputs),
                first.Lease);

            AssertInvalid(result);
        }
        finally
        {
            first.Lease.Dispose();
        }
    }

    [Fact]
    public void ProvenanceOnlyLease_CannotMintMachineAdmission()
    {
        var final = CreateFinal(
            "provenance-only",
            [CreateItem("provenance-only:1", Chosen, Rejected)]);
        var leaseResult = DesignConversionLegalTeacherCapabilityTestFactory
            .CreateAdmitted()
            .AcquireTrainingLease(final.Provenance);
        Assert.True(leaseResult.IsSuccess, leaseResult.ErrorMessage);
        using var lease = leaseResult.Value!;

        var result = DesignConversionMachineTrainingAdmission.Mint(
            final,
            CreateBatch("provenance-only", final.Inputs),
            lease);

        AssertInvalid(result);
    }

    [Fact]
    public void SubstitutedReorderedAndNonReferenceItems_FailMint()
    {
        var final = CreateFinal(
            "reference",
            [
                CreateItem("reference:1", Chosen, Rejected),
                CreateItem(
                    "reference:2",
                    "{\"html\":\"<main>two</main>\",\"css\":\"main{}\"}",
                    Rejected)
            ]);
        using var lease = AcquireLease(final);

        var copy = final.Inputs[0] with { };
        AssertInvalid(DesignConversionMachineTrainingAdmission.Mint(
            final,
            CreateBatch("copy", [copy]),
            lease));
        AssertInvalid(DesignConversionMachineTrainingAdmission.Mint(
            final,
            CreateBatch("reordered", [final.Inputs[1], final.Inputs[0]]),
            lease));
        AssertInvalid(DesignConversionMachineTrainingAdmission.Mint(
            final,
            CreateBatch("substituted", [final.Inputs[0], copy]),
            lease));
    }

    [Fact]
    public async Task CallerListReplacementAfterMint_DoesNotChangeConsumedSnapshot()
    {
        var final = CreateFinal(
            "alias",
            [CreateItem("alias:1", Chosen, Rejected)]);
        using var lease = AcquireLease(final);
        var selected = new[] { final.Inputs[0] };
        var sources = new[] { "source-before" };
        var batch = new LearningBatch<DesignConversionDomain>(
            "alias:batch",
            DesignConversionMachine.DomainTag,
            selected,
            sources);
        var admission = DesignConversionMachineTrainingAdmission.Mint(
            final,
            batch,
            lease);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);

        selected[0] = CreateItem("alias:replacement", Rejected, Chosen);
        sources[0] = "source-after";

        Assert.Equal("alias:1", admission.Value!.Batch.TrainingItems[0].ItemId);
        Assert.Equal("source-before", admission.Value.Batch.SourceIds[0]);
        var result = await new DesignConversionMachine()
            .TrainAdmittedBatchAsync(admission.Value);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("alias:batch", result.Value!.BatchId);
    }

    [Fact]
    public async Task DisposedLeaseAfterMint_PreventsMachineMutation()
    {
        var fixture = CreateFixture("disposed");
        var admission = DesignConversionMachineTrainingAdmission.Mint(
            fixture.Final,
            fixture.Batch,
            fixture.Lease);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        fixture.Lease.Dispose();

        var result = await new DesignConversionMachine()
            .TrainAdmittedBatchAsync(admission.Value!);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void LabelBearingNeutralCommitment_FailsCarrierMintThroughSideContract()
    {
        const string labelled =
            "{\"html\":\"<main>x</main>\",\"css\":\"main{}\",\"accepted\":true}";
        var item = CreateItem("label:1", labelled, Rejected);
        var neutral = BoundedDpoTrainingContentProjector.Project(
        [
            new DpoTrainingContentRowInput(
                item.Prompt,
                item.ChosenResponse,
                item.RejectedResponse,
                [],
                "neutral-test-commitment")
        ]);
        Assert.True(neutral.IsSuccess, neutral.ErrorMessage);
        var provenance = TrainingTeacherProvenance.Deterministic("label:request")
            with
            {
                TrainingContentSha256 = neutral.Value!.Rows[0].Sha256
            };
        var final = new DesignConversionFinalOptimizerBatch<LearningTrainingItem>(
            [item],
            [provenance],
            neutral.Value);
        using var lease = AcquireLease(final);

        var result = DesignConversionMachineTrainingAdmission.Mint(
            final,
            CreateBatch("label", final.Inputs),
            lease);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoOptimizerSideContract.InvalidErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void ProductionCallsites_UseAdmittedEntryWithoutPublicFallback()
    {
        var root = FindRepoRoot();
        var pipeline = File.ReadAllText(Path.Combine(
            root,
            "44.DesignConversionDomain",
            "DesignConversionTrainingPipeline.cs"));
        var continuation = File.ReadAllText(Path.Combine(
            root,
            "44.DesignConversionDomain",
            "DesignConversionLocalContinueTrainingProbe.cs"));
        var modelServer = File.ReadAllText(Path.Combine(
            root,
            "25.ModelServer",
            "DesignConversion",
            "DesignConversionDpoTrainingHandoff.cs"));

        Assert.Contains("machine.TrainAdmittedBatchAsync(", pipeline);
        Assert.DoesNotContain("machine.TrainBatchAsync(", pipeline);
        Assert.Contains(
            "VerifyFreshLoadedCheckpointAdmittedAsync(",
            pipeline);
        Assert.Contains("TrainAdmittedBatchAsync(", continuation);
        Assert.DoesNotContain(".TrainBatchAsync(", continuation);
        Assert.Contains("machine.TrainAdmittedBatchAsync(", modelServer);
        Assert.DoesNotContain(
            "machine.TrainBatchAsync(\r\n                    preparedRuntimeBatch",
            modelServer,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task PublicEntryFailsClosedWithoutStateChangeOrObserver()
    {
        var entries = new List<string>();
        var previous = DesignConversionMachine.TrainingEntryObserverForTesting;
        DesignConversionMachine.TrainingEntryObserverForTesting = entries.Add;
        try
        {
            var machine = new DesignConversionMachine();
            var result = await machine.TrainBatchAsync(
                CreateBatch(
                    "public-denied",
                    [CreateItem("public-denied:1", Chosen, Rejected)]));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority
                    .ConfigurationRequiredErrorCode,
                result.ErrorCode);
            Assert.Empty(entries);
            Assert.Empty(machine.ExportCheckpoint().Value!.TrainingHistory);
        }
        finally
        {
            DesignConversionMachine.TrainingEntryObserverForTesting = previous;
        }
    }

    [Fact]
    public async Task PublicLocalContinueEntryFailsClosedBeforeAnyProbeWork()
    {
        var result = await DesignConversionLocalContinueTrainingProbe
            .VerifyFreshLoadedCheckpointAsync(
                freshLoadProbe: null!,
                continuationBatch: null!,
                heldOutRecordIdentityHashes: [],
                continuedCheckpointPath: string.Empty,
                datasetCountGatePassed: false);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .ConfigurationRequiredErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task StrictAuthorityExecutorMutatesValidCanonicalSides()
    {
        var machine = new DesignConversionMachine();
        var result = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                CreateBatch(
                    "strict-valid",
                    [CreateItem("strict-valid:1", Chosen, Rejected)]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("strict-valid:batch", result.Value!.BatchId);
        Assert.Single(machine.ExportCheckpoint().Value!.TrainingHistory);
    }

    [Fact]
    public async Task StrictAuthorityExecutorRejectsInvalidSidesWithoutMutation()
    {
        const string labelled =
            "{\"html\":\"<main>x</main>\",\"css\":\"main{}\",\"accepted\":true}";
        var machine = new DesignConversionMachine();
        var result = await DesignConversionLegalTeacherCapabilityTestFactory
            .TrainMachineStrictAsync(
                machine,
                CreateBatch(
                    "strict-invalid",
                    [CreateItem("strict-invalid:1", labelled, Rejected)]));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            DesignConversionDpoOptimizerSideContract.InvalidErrorCode,
            result.ErrorCode);
        Assert.Empty(machine.ExportCheckpoint().Value!.TrainingHistory);
    }

    [Fact]
    public async Task CompositeParentLease_DerivesExactPipelineBindingAndAdmittedMutation()
    {
        using var fixture = CreateCompositeFixture("composite-success");

        var direct = DesignConversionPipelineTrainingBinding.MintExact(
            fixture.Pipeline,
            fixture.ParentLease);
        AssertInvalidBinding(direct);

        var binding = DesignConversionPipelineTrainingBinding.MintComposite(
            fixture.Commitment,
            fixture.Pipeline,
            fixture.Bridge,
            fixture.ParentLease);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        var admission = DesignConversionMachineTrainingAdmission.Mint(
            fixture.Pipeline,
            CreateBatch("composite-success", fixture.Pipeline.Inputs),
            binding.Value);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);

        var result = await new DesignConversionMachine()
            .TrainAdmittedBatchAsync(admission.Value!);

        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    [Fact]
    public void CompositeBinding_RejectsSameTextCopiedReorderedAndSubstitutedPipelineChildren()
    {
        using var fixture = CreateCompositeFixture(
            "composite-refs",
            itemCount: 2,
            includeBridge: false);
        var copiedItems = fixture.Pipeline.Inputs
            .Select(static item => item with { })
            .ToArray();
        var copiedFinal = CreateFinal("composite-refs-copy", copiedItems);
        var reorderedFinal = CreateFinal(
            "composite-refs-reordered",
            copiedItems.Reverse().ToArray());
        var substitutedFinal = CreateFinal(
            "composite-refs-substituted",
            [fixture.Pipeline.Inputs[0], copiedItems[1]]);

        AssertInvalidBinding(DesignConversionPipelineTrainingBinding
            .MintComposite(
                fixture.Commitment,
                copiedFinal,
                bridgeFinalOptimizer: null,
                fixture.ParentLease));
        AssertInvalidBinding(DesignConversionPipelineTrainingBinding
            .MintComposite(
                fixture.Commitment,
                reorderedFinal,
                bridgeFinalOptimizer: null,
                fixture.ParentLease));
        AssertInvalidBinding(DesignConversionPipelineTrainingBinding
            .MintComposite(
                fixture.Commitment,
                substitutedFinal,
                bridgeFinalOptimizer: null,
                fixture.ParentLease));
    }

    [Fact]
    public void CompositeBinding_RejectsPipelineIdentityOrContextDriftAfterParentLease()
    {
        using var fixture = CreateCompositeFixture(
            "composite-drift",
            includeBridge: false);
        var item = fixture.Pipeline.Inputs[0];
        var changedIdentity = CreateFinal(
            "composite-drift-id",
            [item with { ItemId = item.ItemId + ":changed" }]);
        var changedPrompt = CreateFinal(
            "composite-drift-prompt",
            [item with { Prompt = item.Prompt + ":changed" }]);

        AssertInvalidBinding(DesignConversionPipelineTrainingBinding
            .MintComposite(
                fixture.Commitment,
                changedIdentity,
                bridgeFinalOptimizer: null,
                fixture.ParentLease));
        AssertInvalidBinding(DesignConversionPipelineTrainingBinding
            .MintComposite(
                fixture.Commitment,
                changedPrompt,
                bridgeFinalOptimizer: null,
                fixture.ParentLease));
    }

    [Fact]
    public void CompositeBinding_RejectsChangedPairGovernanceAndDifferentBridgeChild()
    {
        using var fixture = CreateCompositeFixture("composite-parent-drift");
        var changedPair = fixture.Pair with
        {
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                ["sha256:" + new string('e', 64)])
        };
        var rebound = DpoPairTrainingContentAuthority.Bind(changedPair);
        Assert.True(rebound.IsSuccess, rebound.ErrorMessage);
        var changedCommitment = DesignConversionDpoTrainingContentAuthority
            .ProjectCompositeOptimizerInputs(
                fixture.Pipeline,
                [rebound.Value!],
                fixture.Bridge);
        Assert.True(changedCommitment.IsSuccess, changedCommitment.ErrorMessage);
        AssertInvalidBinding(DesignConversionPipelineTrainingBinding
            .MintComposite(
                changedCommitment.Value,
                fixture.Pipeline,
                fixture.Bridge,
                fixture.ParentLease));

        var otherBridge = ReprojectBridge(fixture.Bridge!);
        AssertInvalidBinding(DesignConversionPipelineTrainingBinding
            .MintComposite(
                fixture.Commitment,
                fixture.Pipeline,
                otherBridge,
                fixture.ParentLease));
    }

    [Fact]
    public void CompositeBinding_DisposedParentBlocksBindingAndAlreadyDerivedPermit()
    {
        var fixture = CreateCompositeFixture("composite-revoked");
        var binding = DesignConversionPipelineTrainingBinding.MintComposite(
            fixture.Commitment,
            fixture.Pipeline,
            fixture.Bridge,
            fixture.ParentLease);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        fixture.ParentLease.Dispose();

        AssertInvalidBinding(DesignConversionPipelineTrainingBinding
            .MintComposite(
                fixture.Commitment,
                fixture.Pipeline,
                fixture.Bridge,
                fixture.ParentLease));
        var permit = binding.Value!.AcquireMutationPermit();
        Assert.False(permit.IsSuccess);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            permit.ErrorCode);
        fixture.Dispose();
    }

    [Theory]
    [InlineData("teaching_vector")]
    [InlineData("chosen_ids")]
    [InlineData("chosen_mask")]
    [InlineData("rejected_ids")]
    [InlineData("rejected_mask")]
    public void CompositeBinding_LivePermitRejectsPostMintBridgeArrayDrift(
        string mutation)
    {
        using var fixture = CreateCompositeFixture(
            $"composite-live-drift-{mutation}");
        var binding = DesignConversionPipelineTrainingBinding.MintComposite(
            fixture.Commitment,
            fixture.Pipeline,
            fixture.Bridge,
            fixture.ParentLease);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        var vectorBatch = fixture.Bridge!.Inputs[0].VectorBatch;
        switch (mutation)
        {
            case "teaching_vector":
                vectorBatch.TeachingVector[0] += 1f;
                break;
            case "chosen_ids":
                vectorBatch.Batch.ChosenIds.Data[0] += 1f;
                break;
            case "chosen_mask":
                vectorBatch.Batch.ChosenMask.Data[0] = 0f;
                break;
            case "rejected_ids":
                vectorBatch.Batch.RejectedIds.Data[0] += 1f;
                break;
            case "rejected_mask":
                vectorBatch.Batch.RejectedMask.Data[0] = 0f;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation));
        }

        var permit = binding.Value!.AcquireMutationPermit();

        Assert.False(permit.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode,
            permit.ErrorCode);
    }

    private static Fixture CreateFixture(string id)
    {
        var final = CreateFinal(
            id,
            [CreateItem($"{id}:1", Chosen, Rejected)]);
        var lease = AcquireLease(final);
        return new Fixture(final, CreateBatch(id, final.Inputs), lease);
    }

    private static CompositeFixture CreateCompositeFixture(
        string id,
        int itemCount = 1,
        bool includeBridge = true)
    {
        var items = Enumerable.Range(0, itemCount)
            .Select(index => CreateItem(
                $"{id}:{index}",
                Chosen,
                index % 2 == 0
                    ? Rejected
                    : "{\"html\":\"<main>other</main>\",\"css\":\"main{}\"}"))
            .ToArray();
        var pipeline = CreateFinal(id, items);
        var pair = new ContinuousLearning.DpoPair(
            items[0].Prompt,
            items[0].ChosenResponse,
            items[0].RejectedResponse,
            DesignConversionMachine.DomainTag,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = pipeline.Provenance[0],
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                ["sha256:" + new string('c', 64)])
        };
        var pairs = new ContinuousLearning.DpoPair[itemCount];
        for (var index = 0; index < itemCount; index++)
        {
            var source = index == 0
                ? pair
                : pair with
                {
                    Prompt = items[index].Prompt,
                    Chosen = items[index].ChosenResponse,
                    Rejected = items[index].RejectedResponse,
                    TeacherProvenance = pipeline.Provenance[index]
                };
            var bound = DpoPairTrainingContentAuthority.Bind(source);
            Assert.True(bound.IsSuccess, bound.ErrorMessage);
            pairs[index] = bound.Value!;
        }

        DesignConversionFinalOptimizerBatch<
            DesignConversionDpoEvaluatedPackedBatch>? bridge = null;
        if (includeBridge)
            bridge = CreateBridge(pipeline.Provenance[0]!);
        var commitment = DesignConversionDpoTrainingContentAuthority
            .ProjectCompositeOptimizerInputs(pipeline, pairs, bridge);
        Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
        var lease = DesignConversionLegalTeacherCapabilityTestFactory
            .CreateAdmitted()
            .AcquireTrainingLease(
                commitment.Value!.FinalOptimizer.Content,
                commitment.Value.FinalOptimizer.Provenance);
        Assert.True(lease.IsSuccess, lease.ErrorMessage);
        return new CompositeFixture(
            pipeline,
            pairs[0],
            bridge,
            commitment.Value,
            lease.Value!);
    }

    private static DesignConversionFinalOptimizerBatch<
        DesignConversionDpoEvaluatedPackedBatch> CreateBridge(
            TrainingTeacherProvenance provenance)
    {
        var packed = new DesignConversionDpoEvaluatedPackedBatch(
            new DpoVectorBatch(
                new DpoBatch
                {
                    ChosenIds = new Tensor([0f], new TensorShape(1, 1)),
                    RejectedIds = new Tensor([0f], new TensorShape(1, 1)),
                    ChosenMask = new Tensor([1f], new TensorShape(1, 1)),
                    RejectedMask = new Tensor([1f], new TensorShape(1, 1))
                },
                [0.25f],
                0.75f),
            new string('a', 64),
            new string('b', 64),
            "bridge-prompt",
            Chosen,
            Rejected,
            provenance);
        var projected = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                [packed],
                [provenance],
                static value => value.Prompt,
                static value => value.Chosen,
                static value => value.Rejected,
                static value => value.VectorBatch.TeachingVector,
                static value => value.VectorBatch.Weight,
                static _ => "composite-bridge-test");
        Assert.True(projected.IsSuccess, projected.ErrorMessage);
        return projected.Value!;
    }

    private static DesignConversionFinalOptimizerBatch<
        DesignConversionDpoEvaluatedPackedBatch> ReprojectBridge(
            DesignConversionFinalOptimizerBatch<
                DesignConversionDpoEvaluatedPackedBatch> bridge)
    {
        var projected = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                bridge.Inputs,
                bridge.Provenance,
                static value => value.Prompt,
                static value => value.Chosen,
                static value => value.Rejected,
                static value => value.VectorBatch.TeachingVector,
                static value => value.VectorBatch.Weight,
                static _ => "composite-bridge-test");
        Assert.True(projected.IsSuccess, projected.ErrorMessage);
        return projected.Value!;
    }

    private static DesignConversionFinalOptimizerBatch<LearningTrainingItem>
        CreateFinal(string id, IReadOnlyList<LearningTrainingItem> items)
    {
        var projected = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                items,
                items.Select((_, index) =>
                    (TrainingTeacherProvenance?)
                    TrainingTeacherProvenance.Deterministic(
                        $"{id}:request:{index}")).ToArray(),
                static item => item.Prompt,
                static item => item.ChosenResponse,
                static item => item.RejectedResponse,
                static _ => Array.Empty<float>(),
                static _ => null,
                static item => $"machine-admission-test|item={item.ItemId}");
        Assert.True(projected.IsSuccess, projected.ErrorMessage);
        return projected.Value!;
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionLease AcquireLease(
        DesignConversionFinalOptimizerBatch<LearningTrainingItem> final)
    {
        var result = DesignConversionLegalTeacherCapabilityTestFactory
            .CreateAdmitted()
            .AcquireTrainingLease(final.Content, final.Provenance);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static LearningTrainingItem CreateItem(
        string id,
        string chosen,
        string rejected)
        => new(id, $"prompt:{id}", chosen, rejected);

    private static LearningBatch<DesignConversionDomain> CreateBatch(
        string id,
        IReadOnlyList<LearningTrainingItem> items)
        => new(
            $"{id}:batch",
            DesignConversionMachine.DomainTag,
            items,
            [$"source:{id}"]);

    private static void AssertInvalid(
        AgentInfrastructure.OperationResult<
            DesignConversionMachineTrainingAdmission> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode,
            result.ErrorCode);
    }

    private static void AssertInvalidBinding(
        AgentInfrastructure.OperationResult<
            DesignConversionPipelineTrainingBinding> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode,
            result.ErrorCode);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }

    private sealed record Fixture(
        DesignConversionFinalOptimizerBatch<LearningTrainingItem> Final,
        LearningBatch<DesignConversionDomain> Batch,
        ServerOwnedLegalTrainingTeacherAdmissionLease Lease);

    private sealed record CompositeFixture(
        DesignConversionFinalOptimizerBatch<LearningTrainingItem> Pipeline,
        ContinuousLearning.DpoPair Pair,
        DesignConversionFinalOptimizerBatch<
            DesignConversionDpoEvaluatedPackedBatch>? Bridge,
        DesignConversionDpoTrainingContentAuthority
            .CompositeOptimizerCommitment Commitment,
        ServerOwnedLegalTrainingTeacherAdmissionLease ParentLease)
        : IDisposable
    {
        public void Dispose() => ParentLease.Dispose();
    }
}
