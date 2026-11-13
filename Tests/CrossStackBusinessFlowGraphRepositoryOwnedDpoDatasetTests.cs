// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using ContinuousLearning;
using DPO.Models;
using ProjectWikiDomain;

public sealed class CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTests
    : IDisposable
{
    private static readonly string[] SimulatedTrainingDomains =
    [
        "campaign",
        "client",
        "diet",
        "media",
        "task",
        "game",
        "profile",
        "report",
        "notification",
        "booking"
    ];

    private static readonly string[] SimulatedHeldOutDomains =
    [
        "invoice",
        "refund",
        "shipment",
        "audit",
        "tenant",
        "license",
        "subscription",
        "cart",
        "checkout",
        "message"
    ];

    private static readonly string[] SimulatedRejectedTypes =
    [
        "ui_api_only",
        "server_sql_only",
        "procedure_without_result_shape",
        "separate_repositories",
        "incomplete_cross_stack_business_flow"
    ];

    private readonly List<string> _roots = [];

    public void Dispose()
    {
        foreach (var root in _roots)
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task
        ExactSealedDataset_ReusesDiskBeforeCreatorAcrossFreshContexts()
    {
        var repositoryRoot = NewRepositoryRoot("repo-exact-hit");
        var batchId = "cross-stack-repository-owned-exact-hit";
        var sourcePairs = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: false,
            "exact-hit-reopened");
        var creatorOrderPairs = sourcePairs.Reverse().ToArray();
        var address = CreateAddress(repositoryRoot, "v1-train");
        var identity = CreateRequestIdentity("exact-hit", sourcePairs);
        var seal = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address,
            identity,
            CreateRepositoryPairs(
                address,
                identity,
                DpoBatchSplitRole.Training,
                creatorOrderPairs));
        Assert.True(seal.IsSuccess, seal.ErrorMessage);

        var creatorCalls = 0;
        var freshContext = CreateContext(
            repositoryRoot,
            "v1-train",
            DpoBatchSplitRole.Training,
            identity,
            (_, _) =>
            {
                creatorCalls++;
                return Task.FromResult(OperationResult<
                    IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                    "Creator must not run on exact sealed hit.",
                    "CROSS_STACK_TEST_CREATOR_UNEXPECTED"));
            });

        var resolved =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(freshContext, batchId);

        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            resolved.Value!.Source);
        Assert.Equal(0, creatorCalls);
        Assert.Equal(
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ComputeProjectPairsSha256(resolved.Value.Batch.Pairs),
            resolved.Value.ReopenedProjectPairsSha256);
        Assert.NotEqual(
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ComputeProjectPairsSha256(creatorOrderPairs),
            resolved.Value.ReopenedProjectPairsSha256);
        Assert.Contains(
            resolved.Value.Batch.Pairs,
            pair => pair.Chosen.Contains(
                "exact-hit-reopened",
                StringComparison.Ordinal));
        Assert.Equal(DpoBatchSourceKind.RealRepositoryCorpus,
            resolved.Value.Batch.SourceKind);
        Assert.Equal(DpoBatchSplitRole.Training,
            resolved.Value.Batch.SplitRole);
    }

    [Fact]
    public async Task
        CleanMiss_CallsCreatorOnceSealsCanonicalFilesThenReopensWithoutCreator()
    {
        var repositoryRoot = NewRepositoryRoot("repo-clean-miss");
        var batchId = "cross-stack-repository-owned-clean-miss";
        var sourcePairs = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: false,
            "clean-miss-reopened");
        var creatorOrderPairs = sourcePairs.Reverse().ToArray();
        var address = CreateAddress(repositoryRoot, "v1-train");
        var identity = CreateRequestIdentity("clean-miss", sourcePairs);
        var creatorCalls = 0;
        var context = CreateContext(
            repositoryRoot,
            "v1-train",
            DpoBatchSplitRole.Training,
            identity,
            request =>
            {
                creatorCalls++;
                Assert.Equal(batchId, request.BatchId);
                Assert.Equal(50, request.ExpectedPairCount);
                Assert.Equal(DpoBatchSplitRole.Training, request.SplitRole);
                Assert.Equal(address.ManifestPath, request.Address.ManifestPath);
                return CreateRepositoryPairs(
                    request.Address,
                    request.RequestIdentity,
                    request.SplitRole,
                    creatorOrderPairs);
            });

        var first =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(context, batchId);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(1, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            first.Value!.Source);
        Assert.True(File.Exists(address.ManifestPath));
        Assert.Equal(
            Path.Combine(
                repositoryRoot,
                "training-data",
                "dpo",
                "cross-stack-business-flow-graph",
                "ilearning-unit",
                "v1-train",
                "manifest.json"),
            address.ManifestPath);
        Assert.All(first.Value.ReopenedRepositoryPairs, pair =>
            Assert.Equal(
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                    .DomainSlug,
                pair.Domain));

        var secondContext = CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
            .CreateContext(
                repositoryRoot,
                "v1-train",
                DpoBatchSplitRole.Training,
                identity);
        Assert.True(secondContext.IsSuccess, secondContext.ErrorMessage);
        var second =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(secondContext.Value!, batchId);

        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            second.Value!.Source);
        Assert.Equal(1, creatorCalls);
        Assert.Equal(first.Value.ReopenedProjectPairsSha256,
            second.Value.ReopenedProjectPairsSha256);
    }

    [Fact]
    public async Task MissingCreatorOnCleanMiss_FailsClosed()
    {
        var repositoryRoot = NewRepositoryRoot("repo-missing-creator");
        var batchId = "cross-stack-repository-owned-missing-creator";
        var sourcePairs = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: false,
            "missing-creator");
        var identity = CreateRequestIdentity("missing-creator", sourcePairs);
        var context = CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
            .CreateContext(
                repositoryRoot,
                "v1-train",
                DpoBatchSplitRole.Training,
                identity);
        Assert.True(context.IsSuccess, context.ErrorMessage);

        var result =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(context.Value!, batchId);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepositoryOwnedDpoDatasetResolver.MissingErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task CorruptManifest_FailsClosedBeforeCreator()
    {
        var repositoryRoot = NewRepositoryRoot("repo-corrupt");
        var batchId = "cross-stack-repository-owned-corrupt";
        var sourcePairs = BuildSimulatedOpenTeacherProjectPairs(batchId, heldOut: false, "corrupt");
        var address = CreateAddress(repositoryRoot, "v1-train");
        var identity = CreateRequestIdentity("corrupt", sourcePairs);
        Directory.CreateDirectory(address.DatasetRoot);
        await File.WriteAllTextAsync(
            address.ManifestPath,
            "{ not valid json",
            Encoding.UTF8);
        var creatorCalls = 0;

        var result =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(
                    CreateContext(
                        repositoryRoot,
                        "v1-train",
                        DpoBatchSplitRole.Training,
                        identity,
                        (_, _) =>
                        {
                            creatorCalls++;
                            return Task.FromResult(OperationResult<
                                IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(
                                []));
                        }),
                    batchId);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
    }

    [Fact]
    public async Task RequestIdentityMismatch_FailsClosedBeforeCreator()
    {
        var repositoryRoot = NewRepositoryRoot("repo-mismatch");
        var batchId = "cross-stack-repository-owned-mismatch";
        var sourcePairs = BuildSimulatedOpenTeacherProjectPairs(batchId, heldOut: false, "mismatch");
        var address = CreateAddress(repositoryRoot, "v1-train");
        var persistedIdentity =
            CreateRequestIdentity("mismatch-persisted", sourcePairs);
        var requestedIdentity =
            CreateRequestIdentity("mismatch-requested", sourcePairs);
        var seal = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address,
            persistedIdentity,
            CreateRepositoryPairs(
                address,
                persistedIdentity,
                DpoBatchSplitRole.Training,
                sourcePairs));
        Assert.True(seal.IsSuccess, seal.ErrorMessage);
        var creatorCalls = 0;

        var result =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(
                    CreateContext(
                        repositoryRoot,
                        "v1-train",
                        DpoBatchSplitRole.Training,
                        requestedIdentity,
                        (_, _) =>
                        {
                            creatorCalls++;
                            return Task.FromResult(OperationResult<
                                IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(
                                []));
                        }),
                    batchId);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer
                .RequestIdentityMismatchErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
    }

    [Fact]
    public async Task PartialDataset_FailsClosedBeforeCreator()
    {
        var repositoryRoot = NewRepositoryRoot("repo-partial");
        var batchId = "cross-stack-repository-owned-partial";
        var sourcePairs = BuildSimulatedOpenTeacherProjectPairs(batchId, heldOut: false, "partial");
        var address = CreateAddress(repositoryRoot, "v1-train");
        var identity = CreateRequestIdentity("partial", sourcePairs);
        Directory.CreateDirectory(address.PairsRoot);
        await File.WriteAllTextAsync(
            Path.Combine(address.PairsRoot, "orphan.dpo-pair.json"),
            "{}",
            Encoding.UTF8);
        var creatorCalls = 0;

        var result =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(
                    CreateContext(
                        repositoryRoot,
                        "v1-train",
                        DpoBatchSplitRole.Training,
                        identity,
                        (_, _) =>
                        {
                            creatorCalls++;
                            return Task.FromResult(OperationResult<
                                IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(
                                []));
                        }),
                    batchId);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
    }

    [Fact]
    public async Task Cancellation_FailsBeforeCreator()
    {
        var repositoryRoot = NewRepositoryRoot("repo-cancel");
        var batchId = "cross-stack-repository-owned-cancel";
        var sourcePairs = BuildSimulatedOpenTeacherProjectPairs(batchId, heldOut: false, "cancel");
        var identity = CreateRequestIdentity("cancel", sourcePairs);
        var creatorCalls = 0;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(
                    CreateContext(
                        repositoryRoot,
                        "v1-train",
                        DpoBatchSplitRole.Training,
                        identity,
                        (_, _) =>
                        {
                            creatorCalls++;
                            return Task.FromResult(OperationResult<
                                IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(
                                []));
                        }),
                    batchId,
                    cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal(RepositoryOwnedDpoDatasetResolver.CancelledErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
    }

    [Fact]
    public async Task PipelineRepositoryOwnedRoute_UsesDistinctReopenedSplits()
    {
        var repositoryRoot = NewRepositoryRoot("repo-pipeline");
        var artifactRoot = NewRoot("artifacts-pipeline");
        var batchId = "cross-stack-repository-owned-pipeline";
        var trainingPairs = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: false,
            "pipeline-train-reopened");
        var heldOutPairs = BuildSimulatedOpenTeacherProjectPairs(
            $"{batchId}-heldout",
            heldOut: true,
            "pipeline-heldout-reopened");
        var trainingIdentity = CreateRequestIdentity(
            "pipeline-train",
            trainingPairs);
        var heldOutIdentity = CreateRequestIdentity(
            "pipeline-heldout",
            heldOutPairs);
        var trainingCreatorCalls = 0;
        var heldOutCreatorCalls = 0;
        var trainingContext = CreateContext(
            repositoryRoot,
            "v1-train",
            DpoBatchSplitRole.Training,
            trainingIdentity,
            request =>
            {
                trainingCreatorCalls++;
                return CreateRepositoryPairs(
                    request.Address,
                    request.RequestIdentity,
                    request.SplitRole,
                    trainingPairs.Reverse().ToArray());
            });
        var heldOutContext = CreateContext(
            repositoryRoot,
            "v1-heldout",
            DpoBatchSplitRole.HeldOut,
            heldOutIdentity,
            request =>
            {
                heldOutCreatorCalls++;
                return CreateRepositoryPairs(
                    request.Address,
                    request.RequestIdentity,
                    request.SplitRole,
                    heldOutPairs.Reverse().ToArray());
            });
        var pipeline = new CrossStackBusinessFlowGraphTrainingPipeline(
            dpoMetricRunner: CreateFastMetricRunner(
                CreateProjectWikiLegalTeacherAdmissionAuthority()));

        var result = await pipeline.RunRepositoryOwnedBatchAsync(
            trainingContext,
            heldOutContext,
            artifactRoot,
            batchId,
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, trainingCreatorCalls);
        Assert.Equal(1, heldOutCreatorCalls);
        Assert.NotNull(result.Value!.RepositoryOwnedTrainingDpoDataset);
        Assert.NotNull(result.Value.RepositoryOwnedHeldOutDpoDataset);
        Assert.Equal(DpoBatchSourceKind.RealRepositoryCorpus,
            result.Value.TrainingBatch.SourceKind);
        Assert.Equal(DpoBatchSourceKind.RealRepositoryCorpus,
            result.Value.HeldOutBatch.SourceKind);
        Assert.Equal(DpoBatchSplitRole.Training,
            result.Value.TrainingBatch.SplitRole);
        Assert.Equal(DpoBatchSplitRole.HeldOut,
            result.Value.HeldOutBatch.SplitRole);
        Assert.NotEqual(
            result.Value.RepositoryOwnedTrainingDpoDataset!.ManifestRelativePath,
            result.Value.RepositoryOwnedHeldOutDpoDataset!.ManifestRelativePath);
        Assert.Equal(
            trainingIdentity.RequestIdentitySha256,
            result.Value.RepositoryOwnedTrainingDpoDataset.RequestIdentity
                .RequestIdentitySha256);
        Assert.Equal(
            heldOutIdentity.RequestIdentitySha256,
            result.Value.RepositoryOwnedHeldOutDpoDataset.RequestIdentity
                .RequestIdentitySha256);
        Assert.NotEqual(
            result.Value.RepositoryOwnedTrainingDpoDataset.RequestIdentity
                .RequestIdentitySha256,
            result.Value.RepositoryOwnedHeldOutDpoDataset.RequestIdentity
                .RequestIdentitySha256);
        Assert.DoesNotContain(
            result.Value.HeldOutBatch.Pairs.Select(pair => pair.PairId),
            result.Value.TrainingBatch.Pairs
                .Select(pair => pair.PairId)
                .ToHashSet(StringComparer.Ordinal)
                .Contains);
        Assert.Contains(
            result.Value.TrainingBatch.Pairs,
            pair => pair.Chosen.Contains(
                "pipeline-train-reopened",
                StringComparison.Ordinal));
        Assert.Contains(
            result.Value.HeldOutBatch.Pairs,
            pair => pair.Chosen.Contains(
                "pipeline-heldout-reopened",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task
        PipelineRepositoryOwnedRoute_OpensHeldOutBeforeTrainingValidationFail()
    {
        var repositoryRoot = NewRepositoryRoot("repo-pipeline-validation-order");
        var artifactRoot = NewRoot("artifacts-validation-order");
        var batchId = "cross-stack-repository-owned-validation-order";
        var invalidTrainingPairs = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: true,
            "training-reopened-but-crossstack-invalid");
        var heldOutPairs = BuildSimulatedOpenTeacherProjectPairs(
            $"{batchId}-heldout",
            heldOut: true,
            "heldout-reopened-before-training-validation");
        var trainingIdentity = CreateRequestIdentity(
            "validation-order-train",
            invalidTrainingPairs);
        var heldOutIdentity = CreateRequestIdentity(
            "validation-order-heldout",
            heldOutPairs);
        var trainingCreatorCalls = 0;
        var heldOutCreatorCalls = 0;
        var trainingContext = CreateContext(
            repositoryRoot,
            "v1-validation-order-train",
            DpoBatchSplitRole.Training,
            trainingIdentity,
            request =>
            {
                trainingCreatorCalls++;
                return CreateRepositoryPairs(
                    request.Address,
                    request.RequestIdentity,
                    request.SplitRole,
                    invalidTrainingPairs);
            });
        var heldOutContext = CreateContext(
            repositoryRoot,
            "v1-validation-order-heldout",
            DpoBatchSplitRole.HeldOut,
            heldOutIdentity,
            request =>
            {
                heldOutCreatorCalls++;
                return CreateRepositoryPairs(
                    request.Address,
                    request.RequestIdentity,
                    request.SplitRole,
                    heldOutPairs);
            });
        var pipeline = new CrossStackBusinessFlowGraphTrainingPipeline(
            dpoMetricRunner: CreateFastMetricRunner());

        var result = await pipeline.RunRepositoryOwnedBatchAsync(
            trainingContext,
            heldOutContext,
            artifactRoot,
            batchId,
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal("CROSS_STACK_SEED_MATRIX_INCOMPLETE", result.ErrorCode);
        Assert.Equal(1, trainingCreatorCalls);
        Assert.Equal(1, heldOutCreatorCalls);
        Assert.True(File.Exists(trainingContext.Address.ManifestPath));
        Assert.True(File.Exists(heldOutContext.Address.ManifestPath));
        Assert.False(Directory.Exists(Path.Combine(artifactRoot, "dpo-runs")));
    }

    [Fact]
    public async Task PipelineRepositoryOwnedRoute_RejectsAliasedSplitContext()
    {
        var repositoryRoot = NewRepositoryRoot("repo-alias");
        var artifactRoot = NewRoot("artifacts-alias");
        var batchId = "cross-stack-repository-owned-alias";
        var sourcePairs = BuildSimulatedOpenTeacherProjectPairs(batchId, heldOut: false, "alias");
        var identity = CreateRequestIdentity("alias", sourcePairs);
        var context = CreateContext(
            repositoryRoot,
            "v1-train",
            DpoBatchSplitRole.Training,
            identity,
            request => CreateRepositoryPairs(
                request.Address,
                request.RequestIdentity,
                request.SplitRole,
                sourcePairs));
        var pipeline = new CrossStackBusinessFlowGraphTrainingPipeline(
            dpoMetricRunner: CreateFastMetricRunner());

        var result = await pipeline.RunRepositoryOwnedBatchAsync(
            context,
            context,
            artifactRoot,
            batchId,
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "CROSS_STACK_REPOSITORY_OWNED_DPO_SPLIT_CONTEXT_MISMATCH",
            result.ErrorCode);
        Assert.False(Directory.Exists(Path.Combine(artifactRoot, "dpo-runs")));
    }

    [Fact]
    public async Task
        PipelineRepositoryOwnedRoute_RejectsSameRequestIdentityAtDifferentAddressesBeforeArtifacts()
    {
        var repositoryRoot = NewRepositoryRoot("repo-same-request-identity");
        var artifactRoot = NewRoot("artifacts-same-request-identity");
        var batchId = "cross-stack-repository-owned-same-request-identity";
        var trainingPairs = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: false,
            "same-request-training");
        var heldOutPairs = BuildSimulatedOpenTeacherProjectPairs(
            $"{batchId}-heldout",
            heldOut: true,
            "same-request-heldout");
        var sharedIdentity = CreateRequestIdentity(
            "same-request-identity",
            trainingPairs);
        var trainingCreatorCalls = 0;
        var heldOutCreatorCalls = 0;
        var trainingContext = CreateContext(
            repositoryRoot,
            "v1-same-request-train",
            DpoBatchSplitRole.Training,
            sharedIdentity,
            request =>
            {
                trainingCreatorCalls++;
                return CreateRepositoryPairs(
                    request.Address,
                    request.RequestIdentity,
                    request.SplitRole,
                    trainingPairs);
            });
        var heldOutContext = CreateContext(
            repositoryRoot,
            "v2-same-request-heldout",
            DpoBatchSplitRole.HeldOut,
            sharedIdentity,
            request =>
            {
                heldOutCreatorCalls++;
                return CreateRepositoryPairs(
                    request.Address,
                    request.RequestIdentity,
                    request.SplitRole,
                    heldOutPairs);
            });
        var pipeline = new CrossStackBusinessFlowGraphTrainingPipeline(
            dpoMetricRunner: CreateFastMetricRunner());

        var result = await pipeline.RunRepositoryOwnedBatchAsync(
            trainingContext,
            heldOutContext,
            artifactRoot,
            batchId,
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "CROSS_STACK_REPOSITORY_OWNED_DPO_SPLIT_REQUEST_IDENTITY_LEAK",
            result.ErrorCode);
        Assert.NotEqual(
            trainingContext.Address.ManifestPath,
            heldOutContext.Address.ManifestPath);
        Assert.Equal(
            trainingContext.RequestIdentity.RequestIdentitySha256,
            heldOutContext.RequestIdentity.RequestIdentitySha256);
        Assert.Equal(0, trainingCreatorCalls);
        Assert.Equal(0, heldOutCreatorCalls);
        Assert.False(File.Exists(trainingContext.Address.ManifestPath));
        Assert.False(File.Exists(heldOutContext.Address.ManifestPath));
        Assert.False(Directory.Exists(Path.Combine(artifactRoot, "dpo-runs")));
    }

    [Theory]
    [InlineData(
        DpoPairSideAuthorKind.DeterministicGenerator,
        DpoPairSideAuthorKind.OpenTeacher)]
    [InlineData(
        DpoPairSideAuthorKind.OpenTeacher,
        DpoPairSideAuthorKind.DeterministicGenerator)]
    [InlineData(
        DpoPairSideAuthorKind.DeterministicGenerator,
        DpoPairSideAuthorKind.DeterministicGenerator)]
    [InlineData(
        DpoPairSideAuthorKind.User,
        DpoPairSideAuthorKind.OpenTeacher)]
    [InlineData(
        DpoPairSideAuthorKind.OpenTeacher,
        DpoPairSideAuthorKind.User)]
    [InlineData(
        DpoPairSideAuthorKind.LocalModel,
        DpoPairSideAuthorKind.OpenTeacher)]
    [InlineData(
        DpoPairSideAuthorKind.OpenTeacher,
        DpoPairSideAuthorKind.LocalModel)]
    [InlineData(
        DpoPairSideAuthorKind.PaidExternalModel,
        DpoPairSideAuthorKind.OpenTeacher)]
    [InlineData(
        DpoPairSideAuthorKind.OpenTeacher,
        DpoPairSideAuthorKind.PaidExternalModel)]
    public void NonOpenTeacherSideCannotMintRepositoryOwnedCorpusRows(
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor)
    {
        var repositoryRoot = NewRepositoryRoot("repo-deterministic-rejected");
        var batchId = "cross-stack-repository-owned-deterministic-rejected";
        var projectPair = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: false,
            "deterministic-rejected")[0];
        var address = CreateAddress(repositoryRoot, "v1-train");
        var identity = CreateRequestIdentity(
            "non-open-teacher-authorship",
            [projectPair]);

        var mapped =
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .MapProjectPairToRepositoryPair(
                    address,
                    DpoBatchSplitRole.Training,
                    projectPair,
                    SimulatedOpenTeacherProvenance(identity, 0),
                    chosenAuthor,
                    rejectedAuthor);

        Assert.False(mapped.IsSuccess);
        Assert.Equal(
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .InvalidErrorCode,
            mapped.ErrorCode);
        Assert.False(File.Exists(address.ManifestPath));
        Assert.DoesNotContain(
            EnumerateRelativeFiles(repositoryRoot),
            IsRepositoryOwnedDpoFile);
        Assert.DoesNotContain(
            EnumerateRelativeFiles(repositoryRoot),
            IsRepositoryOwnedCreationLease);
    }

    [Fact]
    public void DeterministicTeacherProvenanceCannotMintRepositoryOwnedCorpusRows()
    {
        var repositoryRoot = NewRepositoryRoot("repo-deterministic-provenance");
        var batchId = "cross-stack-repository-owned-deterministic-provenance";
        var projectPair = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: false,
            "deterministic-provenance")[0];
        var address = CreateAddress(repositoryRoot, "v1-train");

        var mapped =
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .MapProjectPairToRepositoryPair(
                    address,
                    DpoBatchSplitRole.Training,
                    projectPair,
                    TrainingTeacherProvenance.Deterministic(
                        "cross-stack-deterministic-fixture"),
                    DpoPairSideAuthorKind.OpenTeacher,
                    DpoPairSideAuthorKind.OpenTeacher);

        Assert.False(mapped.IsSuccess);
        Assert.Equal(
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .InvalidErrorCode,
            mapped.ErrorCode);
        Assert.False(File.Exists(address.ManifestPath));
        Assert.DoesNotContain(
            EnumerateRelativeFiles(repositoryRoot),
            IsRepositoryOwnedDpoFile);
        Assert.DoesNotContain(
            EnumerateRelativeFiles(repositoryRoot),
            IsRepositoryOwnedCreationLease);
    }

    [Theory]
    [InlineData(
        DpoPairSideAuthorKind.DeterministicGenerator,
        DpoPairSideAuthorKind.OpenTeacher)]
    [InlineData(
        DpoPairSideAuthorKind.OpenTeacher,
        DpoPairSideAuthorKind.DeterministicGenerator)]
    public async Task
        ExistingSealedDataset_WithDeterministicSide_FailsProjectAdmissionWithoutCreatorOrNewFiles(
            DpoPairSideAuthorKind chosenAuthor,
            DpoPairSideAuthorKind rejectedAuthor)
    {
        var repositoryRoot = NewRepositoryRoot("repo-reopened-mixed-author");
        var batchId = "cross-stack-repository-owned-reopened-mixed-author";
        var fixturePairs = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: false,
            "reopened-mixed-author");
        var address = CreateAddress(repositoryRoot, "v1-train");
        var identity = CreateRequestIdentity(
            "reopened-mixed-author",
            fixturePairs);
        var admittedRows = CreateRepositoryPairs(
            address,
            identity,
            DpoBatchSplitRole.Training,
            fixturePairs);

        var mixedRows = admittedRows
            .Select((pair, index) => index == 0
                ? pair with
                {
                    ChosenAuthorship = CreateStoredSideAuthorship(
                        chosenAuthor,
                        pair.TeacherProvenance!),
                    RejectedAuthorship = CreateStoredSideAuthorship(
                        rejectedAuthor,
                        pair.TeacherProvenance!)
                }
                : pair)
            .ToArray();
        var seal = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address,
            identity,
            mixedRows);
        Assert.True(seal.IsSuccess, seal.ErrorMessage);
        var beforeResolve = EnumerateRelativeFiles(repositoryRoot)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var creatorCalls = 0;

        var context = CreateContext(
            repositoryRoot,
            "v1-train",
            DpoBatchSplitRole.Training,
            identity,
            (_, _) =>
            {
                creatorCalls++;
                return Task.FromResult(OperationResult<
                    IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                    "Creator must not run for an existing sealed deterministic-side rejection.",
                    "CROSS_STACK_TEST_CREATOR_UNEXPECTED"));
            });

        var result =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(context, batchId);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
        Assert.Equal(
            beforeResolve,
            EnumerateRelativeFiles(repositoryRoot)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    [Fact]
    public async Task CleanMiss_WritesOnlyRepositoryOwnedDpoFilesAndCreationLease()
    {
        var repositoryRoot = NewRepositoryRoot("repo-write-scope");
        var before = EnumerateRelativeFiles(repositoryRoot)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var batchId = "cross-stack-repository-owned-write-scope";
        var sourcePairs = BuildSimulatedOpenTeacherProjectPairs(
            batchId,
            heldOut: false,
            "write-scope");
        var identity = CreateRequestIdentity("write-scope", sourcePairs);

        var result =
            await CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .ResolveBatchAsync(
                    CreateContext(
                        repositoryRoot,
                        "v1-train",
                        DpoBatchSplitRole.Training,
                        identity,
                        request => CreateRepositoryPairs(
                            request.Address,
                            request.RequestIdentity,
                            request.SplitRole,
                            sourcePairs)),
                    batchId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var after = EnumerateRelativeFiles(repositoryRoot)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        after.ExceptWith(before);
        Assert.NotEmpty(after);
        Assert.All(after, path =>
        {
            // WHY: a clean miss owns two repository paths: the durable dataset
            // bytes under training-data/dpo and the persistent one-byte
            // creation lease under training-artifacts. Allowing exactly those
            // roots proves the resolver did not spill teacher/model/training
            // outputs beside the sealed DPO corpus.
            Assert.True(
                IsRepositoryOwnedDpoFile(path) ||
                IsRepositoryOwnedCreationLease(path),
                path);
        });
    }

    private string NewRoot(string kind)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "cross-stack-repository-owned-tests",
            $"{kind}-{Guid.NewGuid():N}");
        _roots.Add(root);
        return Path.GetFullPath(root);
    }

    private string NewRepositoryRoot(string kind)
    {
        var repositoryRoot = NewRoot(kind);
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "cross-stack repository-owned fixture solution",
            Encoding.UTF8);
        Directory.CreateDirectory(
            Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
        return repositoryRoot;
    }

    private static RepositoryOwnedDpoDatasetAddress CreateAddress(
        string repositoryRoot,
        string datasetVersion)
    {
        var address =
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .CreateAddress(repositoryRoot, datasetVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext
        CreateContext(
            string repositoryRoot,
            string datasetVersion,
            DpoBatchSplitRole splitRole,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            Func<CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetCreationRequest,
                CancellationToken,
                Task<OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>>>
                createPairsAsync)
    {
        var context =
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                .CreateContext(
                    repositoryRoot,
                    datasetVersion,
                    splitRole,
                    identity,
                    createPairsAsync);
        Assert.True(context.IsSuccess, context.ErrorMessage);
        return context.Value!;
    }

    private static CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext
        CreateContext(
            string repositoryRoot,
            string datasetVersion,
            DpoBatchSplitRole splitRole,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            Func<CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetCreationRequest,
                IReadOnlyList<ContinuousLearning.DpoPair>> createPairs)
        => CreateContext(
            repositoryRoot,
            datasetVersion,
            splitRole,
            identity,
            (request, _) => Task.FromResult(OperationResult<
                IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(
                createPairs(request))));

    private static CrossStackBusinessFlowGraphDpoPair[] BuildSimulatedOpenTeacherProjectPairs(
        string batchId,
        bool heldOut,
        string reopenedMarker)
    {
        // WHY: this is an explicit simulated open-teacher fixture. It proves
        // repository-owned wiring only; it is not production authorship proof
        // and deliberately does not call the deterministic production seed
        // generator that remains baseline/eval-only.
        var unit = new CrossStackBusinessFlowGraphUnit();
        var domains = heldOut
            ? SimulatedHeldOutDomains
            : SimulatedTrainingDomains;
        var rows = new List<CrossStackBusinessFlowGraphDpoPair>(
            domains.Length * SimulatedRejectedTypes.Length);
        foreach (var domain in domains)
        {
            foreach (var rejectedType in SimulatedRejectedTypes)
            {
                var index = rows.Count;
                var cellSlug = Slug(rejectedType);
                var marker = $"flow:{domain}:{cellSlug}";
                var projects = new[]
                {
                    $"angular-{domain}-{cellSlug}",
                    $"dotnet-{domain}-{cellSlug}",
                    $"sql-{domain}-{cellSlug}"
                };
                var boundary = new SystemBoundaryMapResult(
                    $"system-{domain}-{cellSlug}",
                    projects,
                    [
                        new SystemBoundaryEdge(
                            $"edge-client-api-{domain}-{cellSlug}",
                            projects[0],
                            projects[1],
                            "client_to_server_api",
                            [marker, $"api:/{domain}/{cellSlug}"],
                            [
                                $"ev-{domain}-{cellSlug}-client",
                                $"ev-{domain}-{cellSlug}-api"
                            ],
                            0.97,
                            "known"),
                        new SystemBoundaryEdge(
                            $"edge-server-sql-{domain}-{cellSlug}",
                            projects[1],
                            projects[2],
                            "database_boundary",
                            [
                                marker,
                                $"sp:{domain}_save_{cellSlug}",
                                $"result:{domain}_summary_{cellSlug}",
                                $"ui:{domain}_state_{cellSlug}"
                            ],
                            [
                                $"ev-{domain}-{cellSlug}-server",
                                $"ev-{domain}-{cellSlug}-sql",
                                $"ev-{domain}-{cellSlug}-result"
                            ],
                            0.96,
                            "known")
                    ],
                    [],
                    [
                        $"confirm DTO mapping for {domain}",
                        $"confirm UI state consumer for {domain}"
                    ],
                    0.94,
                    []);
                var graphId = $"simulated-open-teacher-graph-{domain}-{cellSlug}";
                var graph = unit.Build(graphId, boundary);
                Assert.True(graph.IsSuccess, graph.ErrorMessage);
                var evidenceIds = graph.Value!.Nodes
                    .SelectMany(node => node.EvidenceIds)
                    .Concat(graph.Value.Edges.SelectMany(edge => edge.EvidenceIds))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                rows.Add(new CrossStackBusinessFlowGraphDpoPair(
                    $"{batchId}-pair-{index + 1:000}",
                    boundary,
                    "Simulated open-teacher CrossStack fixture prompt: " +
                    $"connect Angular, .NET, and SQL for {domain}/{rejectedType}.",
                    "SIMULATED_OPEN_TEACHER_ACCEPTED_COMPLETE_GRAPH " +
                    "PASS_TRUE COMPLETE_CROSS_STACK_PATH " +
                    $"system_id: {boundary.SystemId}\n" +
                    $"SEALED_REOPENED_MARKER {reopenedMarker} {index:00}\n" +
                    "client_to_stored_procedure_paths: " +
                    string.Join("; ", graph.Value.ClientToStoredProcedurePaths),
                    "SIMULATED_OPEN_TEACHER_REJECTED_INCOMPLETE_GRAPH " +
                    "FAIL_FALSE MISSING_CROSS_STACK_PATH " +
                    $"system_id: {boundary.SystemId}\n" +
                    $"[WRONG: {rejectedType} - simulated teacher fixture " +
                    "omits part of the Angular to .NET to SQL flow.]",
                    rejectedType,
                    graph.Value.ClientToStoredProcedurePaths,
                    evidenceIds));
            }
        }

        Assert.Equal(50, rows.Count);
        return rows.ToArray();
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity CreateRequestIdentity(
        string scope,
        IReadOnlyList<CrossStackBusinessFlowGraphDpoPair> pairs)
    {
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                pairs
                    .Select((pair, index) => Sha256Hex(
                        "paid-reference-fingerprint-only\n" +
                        $"{scope}\n{index}\n{pair.PairId}"))
                    .ToArray(),
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen3.5:9b",
                    new string('d', 64),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    new string('c', 64)),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "cross-stack-business-flow-graph-test-generation",
                    "v1",
                    Sha256Hex("generation-policy\n" + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "cross-stack-business-flow-graph-test-creator",
                    "v1",
                    Sha256Hex("creator-policy\n" + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private static IReadOnlyList<ContinuousLearning.DpoPair>
        CreateRepositoryPairs(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            DpoBatchSplitRole splitRole,
            IReadOnlyList<CrossStackBusinessFlowGraphDpoPair> projectPairs,
            DpoPairSideAuthorKind chosenAuthor =
                DpoPairSideAuthorKind.OpenTeacher,
            DpoPairSideAuthorKind rejectedAuthor =
                DpoPairSideAuthorKind.OpenTeacher)
        => projectPairs.Select((pair, index) =>
        {
            var mapped =
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                    .MapProjectPairToRepositoryPair(
                        address,
                        splitRole,
                        pair,
                        SimulatedOpenTeacherProvenance(identity, index),
                        chosenAuthor,
                        rejectedAuthor);
            Assert.True(mapped.IsSuccess, mapped.ErrorMessage);
            return mapped.Value!;
        }).ToArray();

    private static DpoPairSideAuthorship CreateStoredSideAuthorship(
        DpoPairSideAuthorKind author,
        TrainingTeacherProvenance teacherProvenance)
        => author is DpoPairSideAuthorKind.OpenTeacher
            ? new DpoPairSideAuthorship(author, teacherProvenance)
            : new DpoPairSideAuthorship(author);

    private static TrainingTeacherProvenance SimulatedOpenTeacherProvenance(
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        int index)
        // WHY: tests provide a typed simulated Qwen/Ollama teacher passport so
        // the repository-owned bridge exercises production provenance wiring
        // without claiming that a live teacher/model was called.
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            identity.OpenTeacher.ModelId,
            identity.OpenTeacher.ModelVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            RequestId:
                $"cross-stack-test-open-teacher-{index:000}",
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

    private static string Slug(string value)
        => value.Replace('_', '-');

    private static ProjectWikiDpoMetricRunner CreateFastMetricRunner(
        ServerOwnedLegalTrainingTeacherAdmissionAuthority?
            legalTeacherAdmissionAuthority = null)
        // WHY: the repository-owned pipeline test runs the existing tiny local
        // DPO metric path. It still needs the same server-owned legal admission
        // capability as older metric tests, but this helper never calls a live
        // Qwen, Ollama, or external teacher.
        => legalTeacherAdmissionAuthority is null
            ? new(
                epochCount: 1,
                learningRate: 5e-2f,
                vocabSize: 128,
                contextLen: 16,
                hiddenDim: 16,
                numLayers: 1,
                numHeads: 4,
                numKvHeads: 1,
                ffnHiddenDim: 32,
                deterministicFreshInitialization: true)
            : new(
                legalTeacherAdmissionAuthority,
                epochCount: 1,
                learningRate: 5e-2f,
                vocabSize: 128,
                contextLen: 16,
                hiddenDim: 16,
                numLayers: 1,
                numHeads: 4,
                numKvHeads: 1,
                ffnHiddenDim: 32,
                deterministicFreshInitialization: true);

    private static string[] EnumerateRelativeFiles(string root)
        => Directory.Exists(root)
            ? Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(root, path))
                .ToArray()
            : [];

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateProjectWikiLegalTeacherAdmissionAuthority()
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => checked((byte)value))
                .ToArray());
        Assert.True(configuration.IsSuccess, configuration.ErrorMessage);

        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration.Value!,
                static () => true);
    }

    private static bool IsRepositoryOwnedDpoFile(string path)
        => path.StartsWith(
            "training-data" + Path.DirectorySeparatorChar + "dpo" +
            Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase);

    private static bool IsRepositoryOwnedCreationLease(string path)
        => path.StartsWith(
               "training-artifacts" + Path.DirectorySeparatorChar +
               ".dpo-dataset-creation-leases" +
               Path.DirectorySeparatorChar,
               StringComparison.OrdinalIgnoreCase) &&
           path.EndsWith(".lock", StringComparison.OrdinalIgnoreCase);

    private static string Sha256Hex(string text)
        => Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}
