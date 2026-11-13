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

public sealed class NegativeDpoGeneratorRepositoryOwnedDpoDatasetTests
    : IDisposable
{
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
        ExactSealedDataset_ReusesDiskBeforeCreatorAndFeedsReopenedPairsDownstream()
    {
        var repositoryRoot = NewRepositoryRoot("repo-exact");
        var artifactRoot = NewRoot("artifacts-exact");
        var batchId = "negative-dpo-generator-repository-owned-exact-hit";
        var sourcePairs = BuildProjectPairs(batchId, "exact-hit-reopened");
        var creatorOrderPairs = sourcePairs.Reverse().ToArray();
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("exact-hit", sourcePairs);
        var sealedPairs = CreateRepositoryPairs(
            address,
            identity,
            creatorOrderPairs);
        var seal = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address,
            identity,
            sealedPairs);
        Assert.True(seal.IsSuccess, seal.ErrorMessage);

        var creatorFactoryCalls = 0;
        var creatorCalls = 0;
        var context = CreateContext(
            repositoryRoot,
            identity,
            async (request, ct) =>
            {
                creatorFactoryCalls++;
                creatorCalls++;
                await Task.Yield();
                return OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>
                    .Fail(
                        $"Creator must not run for exact hit {request.BatchId}.",
                        "NEGATIVE_DPO_TEST_CREATOR_UNEXPECTED");
            });
        var modelContext = CreateModelContext(
            repositoryRoot,
            "exact-hit",
            allowColdStart: true,
            "authorised first test checkpoint for exact hit");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations).RunRepositoryOwnedBatchAsync(
            context,
            modelContext,
            artifactRoot,
            batchId,
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset,
            result.Value!.RepositoryOwnedDpoDataset!.Source);
        Assert.Equal(0, creatorFactoryCalls);
        Assert.Equal(0, creatorCalls);
        var downstreamHash =
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .ComputeProjectPairsSha256(result.Value.TrainingBatch.Pairs);
        Assert.Equal(
            downstreamHash,
            result.Value.RepositoryOwnedDpoDataset.ReopenedProjectPairsSha256);
        Assert.NotEqual(
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .ComputeProjectPairsSha256(creatorOrderPairs),
            result.Value.RepositoryOwnedDpoDataset.ReopenedProjectPairsSha256);
        Assert.Contains(
            result.Value.TrainingBatch.Pairs,
            pair => pair.Chosen.Contains(
                "exact-hit-reopened",
                StringComparison.Ordinal));
        AssertDownstreamSawReopenedMarker(observations, "exact-hit-reopened");
        AssertRepositoryOwnedAdmissionsMatchReopenedRows(
            observations,
            result.Value.RepositoryOwnedDpoDataset.ReopenedRepositoryPairs);
    }

    [Fact]
    public async Task
        CleanMiss_CreatesOnceSealsCanonicalFilesAndFeedsReopenedHashDownstream()
    {
        var repositoryRoot = NewRepositoryRoot("repo-clean-miss");
        var artifactRoot = NewRoot("artifacts-clean-miss");
        var batchId = "negative-dpo-generator-repository-owned-clean-miss";
        var sourcePairs = BuildProjectPairs(batchId, "clean-miss-reopened");
        var creatorOrderPairs = sourcePairs.Reverse().ToArray();
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("clean-miss", sourcePairs);
        var creatorFactoryCalls = 0;
        var creatorCalls = 0;
        var context = CreateContext(
            repositoryRoot,
            identity,
            (request, ct) =>
            {
                creatorFactoryCalls++;
                creatorCalls++;
                Assert.Equal(batchId, request.BatchId);
                Assert.Equal(50, request.ExpectedPairCount);
                Assert.Equal(address.ManifestPath, request.Address.ManifestPath);
                return Task.FromResult(OperationResult<
                    IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(
                    CreateRepositoryPairs(
                        request.Address,
                        request.RequestIdentity,
                        creatorOrderPairs)));
            });
        var modelContext = CreateModelContext(
            repositoryRoot,
            "clean-miss",
            allowColdStart: true,
            "authorised first test checkpoint for clean miss");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations).RunRepositoryOwnedBatchAsync(
            context,
            modelContext,
            artifactRoot,
            batchId,
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, creatorFactoryCalls);
        Assert.Equal(1, creatorCalls);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            result.Value!.RepositoryOwnedDpoDataset!.Source);
        var downstreamHash =
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .ComputeProjectPairsSha256(result.Value.TrainingBatch.Pairs);
        Assert.Equal(
            downstreamHash,
            result.Value.RepositoryOwnedDpoDataset.ReopenedProjectPairsSha256);
        Assert.NotEqual(
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .ComputeProjectPairsSha256(creatorOrderPairs),
            result.Value.RepositoryOwnedDpoDataset.ReopenedProjectPairsSha256);
        Assert.True(File.Exists(address.ManifestPath));
        Assert.Equal(
            Path.Combine(
                repositoryRoot,
                "training-data",
                "dpo",
                "dpo-dataset-creation",
                "negative-dpo-generator",
                "v1",
                "manifest.json"),
            address.ManifestPath);

        var reopened = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
            address,
            identity);
        Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
        Assert.Equal(50, reopened.Value!.Manifest.PairCount);
        Assert.All(reopened.Value.Manifest.OrderedPairs, pair =>
        {
            Assert.StartsWith(
                "training-data/dpo/dpo-dataset-creation/" +
                "negative-dpo-generator/v1/pairs/",
                pair.RepositoryRelativePairFilePath,
                StringComparison.Ordinal);
            Assert.True(File.Exists(Path.Combine(
                repositoryRoot,
                pair.RepositoryRelativePairFilePath.Replace(
                    '/',
                    Path.DirectorySeparatorChar))));
        });
        AssertDownstreamSawReopenedMarker(observations, "clean-miss-reopened");
        AssertRepositoryOwnedAdmissionsMatchReopenedRows(
            observations,
            result.Value.RepositoryOwnedDpoDataset.ReopenedRepositoryPairs);
    }

    [Fact]
    public async Task CorruptManifest_FailsClosedBeforeCreatorAndDownstream()
    {
        var repositoryRoot = NewRepositoryRoot("repo-corrupt");
        var artifactRoot = NewRoot("artifacts-corrupt");
        var sourcePairs = BuildProjectPairs(
            "negative-dpo-generator-repository-owned-corrupt",
            "corrupt");
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("corrupt", sourcePairs);
        Directory.CreateDirectory(address.DatasetRoot);
        await File.WriteAllTextAsync(
            address.ManifestPath,
            "{ not valid json",
            Encoding.UTF8);
        var creatorCalls = 0;
        var modelContext = CreateModelContext(
            repositoryRoot,
            "corrupt",
            allowColdStart: true,
            "unused because DPO manifest is corrupt");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations).RunRepositoryOwnedBatchAsync(
            CreateContext(
                repositoryRoot,
                identity,
                (_, _) =>
                {
                    creatorCalls++;
                    return Task.FromResult(OperationResult<
                        IReadOnlyList<ContinuousLearning.DpoPair>>.Ok([]));
                }),
            modelContext,
            artifactRoot,
            "negative-dpo-generator-repository-owned-corrupt",
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
        AssertNoDownstreamArtifacts(artifactRoot, observations);
    }

    [Fact]
    public async Task RequestIdentityMismatch_FailsClosedBeforeCreatorAndDownstream()
    {
        var repositoryRoot = NewRepositoryRoot("repo-mismatch");
        var artifactRoot = NewRoot("artifacts-mismatch");
        var sourcePairs = BuildProjectPairs(
            "negative-dpo-generator-repository-owned-mismatch",
            "mismatch");
        var address = CreateAddress(repositoryRoot);
        var persistedIdentity = CreateRequestIdentity(
            "mismatch-persisted",
            sourcePairs);
        var requestedIdentity = CreateRequestIdentity(
            "mismatch-requested",
            sourcePairs);
        var seal = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address,
            persistedIdentity,
            CreateRepositoryPairs(address, persistedIdentity, sourcePairs));
        Assert.True(seal.IsSuccess, seal.ErrorMessage);
        var creatorCalls = 0;
        var modelContext = CreateModelContext(
            repositoryRoot,
            "identity-mismatch",
            allowColdStart: true,
            "unused because DPO request identity mismatches");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations).RunRepositoryOwnedBatchAsync(
            CreateContext(
                repositoryRoot,
                requestedIdentity,
                (_, _) =>
                {
                    creatorCalls++;
                    return Task.FromResult(OperationResult<
                        IReadOnlyList<ContinuousLearning.DpoPair>>.Ok([]));
                }),
            modelContext,
            artifactRoot,
            "negative-dpo-generator-repository-owned-mismatch",
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer
                .RequestIdentityMismatchErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
        AssertNoDownstreamArtifacts(artifactRoot, observations);
    }

    [Fact]
    public async Task PartialDataset_FailsClosedBeforeCreatorAndDownstream()
    {
        var repositoryRoot = NewRepositoryRoot("repo-partial");
        var artifactRoot = NewRoot("artifacts-partial");
        var sourcePairs = BuildProjectPairs(
            "negative-dpo-generator-repository-owned-partial",
            "partial");
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("partial", sourcePairs);
        Directory.CreateDirectory(address.PairsRoot);
        await File.WriteAllTextAsync(
            Path.Combine(address.PairsRoot, "orphan.dpo-pair.json"),
            "{}",
            Encoding.UTF8);
        var creatorCalls = 0;
        var modelContext = CreateModelContext(
            repositoryRoot,
            "partial",
            allowColdStart: true,
            "unused because DPO dataset is partial");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations).RunRepositoryOwnedBatchAsync(
            CreateContext(
                repositoryRoot,
                identity,
                (_, _) =>
                {
                    creatorCalls++;
                    return Task.FromResult(OperationResult<
                        IReadOnlyList<ContinuousLearning.DpoPair>>.Ok([]));
                }),
            modelContext,
            artifactRoot,
            "negative-dpo-generator-repository-owned-partial",
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(0, creatorCalls);
        AssertNoDownstreamArtifacts(artifactRoot, observations);
    }

    [Fact]
    public async Task CreatorFailure_LeavesNoAcceptedPartialDatasetAndNoDownstream()
    {
        var repositoryRoot = NewRepositoryRoot("repo-creator-failure");
        var artifactRoot = NewRoot("artifacts-creator-failure");
        var sourcePairs = BuildProjectPairs(
            "negative-dpo-generator-repository-owned-creator-failure",
            "creator-failure");
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("creator-failure", sourcePairs);
        var creatorCalls = 0;
        var modelContext = CreateModelContext(
            repositoryRoot,
            "creator-failure",
            allowColdStart: true,
            "unused because DPO creator fails");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations).RunRepositoryOwnedBatchAsync(
            CreateContext(
                repositoryRoot,
                identity,
                async (request, ct) =>
                {
                    creatorCalls++;
                    Directory.CreateDirectory(request.Address.PairsRoot);
                    await File.WriteAllTextAsync(
                        Path.Combine(
                            request.Address.PairsRoot,
                            "partial.dpo-pair.json"),
                        "{}",
                        Encoding.UTF8,
                        ct);
                    return OperationResult<
                        IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                        "Synthetic creator failure.",
                        "NEGATIVE_DPO_TEST_CREATOR_FAILED");
                }),
            modelContext,
            artifactRoot,
            "negative-dpo-generator-repository-owned-creator-failure",
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal("NEGATIVE_DPO_TEST_CREATOR_FAILED", result.ErrorCode);
        Assert.Equal(1, creatorCalls);
        Assert.False(File.Exists(address.ManifestPath));
        Assert.False(Directory.Exists(address.DatasetRoot));
        AssertNoDownstreamArtifacts(artifactRoot, observations);
    }

    [Fact]
    public async Task CreatorCancellation_LeavesNoAcceptedPartialDatasetAndNoDownstream()
    {
        var repositoryRoot = NewRepositoryRoot("repo-creator-cancel");
        var artifactRoot = NewRoot("artifacts-creator-cancel");
        var sourcePairs = BuildProjectPairs(
            "negative-dpo-generator-repository-owned-creator-cancel",
            "creator-cancel");
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("creator-cancel", sourcePairs);
        var creatorCalls = 0;
        var modelContext = CreateModelContext(
            repositoryRoot,
            "creator-cancel",
            allowColdStart: true,
            "unused because DPO creator cancellation wins");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations).RunRepositoryOwnedBatchAsync(
            CreateContext(
                repositoryRoot,
                identity,
                async (request, ct) =>
                {
                    creatorCalls++;
                    Directory.CreateDirectory(request.Address.PairsRoot);
                    await File.WriteAllTextAsync(
                        Path.Combine(
                            request.Address.PairsRoot,
                            "partial.dpo-pair.json"),
                        "{}",
                        Encoding.UTF8,
                        ct);
                    throw new OperationCanceledException(
                        "Synthetic creator cancellation.",
                        ct);
                }),
            modelContext,
            artifactRoot,
            "negative-dpo-generator-repository-owned-creator-cancel",
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolver.CancelledErrorCode,
            result.ErrorCode);
        Assert.Equal(1, creatorCalls);
        Assert.False(File.Exists(address.ManifestPath));
        Assert.False(Directory.Exists(address.DatasetRoot));
        AssertNoDownstreamArtifacts(artifactRoot, observations);
    }

    [Theory]
    [InlineData(49)]
    [InlineData(51)]
    public async Task CreatorRowCountMismatch_FailsBeforeSealAndDownstream(
        int rowCount)
    {
        var repositoryRoot = NewRepositoryRoot(
            $"repo-creator-row-count-{rowCount}");
        var artifactRoot = NewRoot($"artifacts-creator-row-count-{rowCount}");
        var batchId =
            $"negative-dpo-generator-repository-owned-row-count-{rowCount}";
        var sourcePairs = BuildProjectPairs(batchId, $"row-count-{rowCount}");
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity($"row-count-{rowCount}",
            sourcePairs);
        var creatorCalls = 0;
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();
        var returnedPairs = SelectCreatorRows(sourcePairs, rowCount);
        var modelContext = CreateModelContext(
            repositoryRoot,
            $"row-count-{rowCount}",
            allowColdStart: true,
            "unused because DPO creator returns wrong row count");

        var result = await CreatePipeline(observations).RunRepositoryOwnedBatchAsync(
            CreateContext(
                repositoryRoot,
                identity,
                request =>
                {
                    creatorCalls++;
                    return CreateRepositoryPairs(
                        request.Address,
                        request.RequestIdentity,
                        returnedPairs);
                }),
            modelContext,
            artifactRoot,
            batchId,
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .BatchSizeErrorCode,
            result.ErrorCode);
        Assert.Equal(1, creatorCalls);
        Assert.False(File.Exists(address.ManifestPath));
        Assert.False(Directory.Exists(address.DatasetRoot));
        AssertNoDownstreamArtifacts(artifactRoot, observations);
    }

    [Theory]
    [InlineData(
        DpoPairSideAuthorKind.DeterministicGenerator,
        DpoPairSideAuthorKind.DeterministicGenerator)]
    [InlineData(
        DpoPairSideAuthorKind.PaidExternalModel,
        DpoPairSideAuthorKind.OpenTeacher)]
    public async Task
        UnsafeCreatorAuthorship_FailsBeforeMetricMutationAndArtifacts(
            DpoPairSideAuthorKind chosenAuthor,
            DpoPairSideAuthorKind rejectedAuthor)
    {
        var repositoryRoot = NewRepositoryRoot("repo-unsafe-authorship");
        var artifactRoot = NewRoot("artifacts-unsafe-authorship");
        var batchId =
            "negative-dpo-generator-repository-owned-unsafe-authorship";
        var sourcePairs = BuildProjectPairs(batchId, "unsafe-authorship");
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity(
            $"unsafe-authorship-{chosenAuthor}-{rejectedAuthor}",
            sourcePairs);
        var creatorCalls = 0;
        var modelContext = CreateModelContext(
            repositoryRoot,
            $"unsafe-authorship-{chosenAuthor}-{rejectedAuthor}",
            allowColdStart: true,
            "unused because DPO authorship is unsafe");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations).RunRepositoryOwnedBatchAsync(
            CreateContext(
                repositoryRoot,
                identity,
                async (request, _) =>
                {
                    creatorCalls++;
                    await Task.Yield();
                    return TryCreateRepositoryPairs(
                        request.Address,
                        request.RequestIdentity,
                        sourcePairs,
                        chosenAuthor,
                        rejectedAuthor);
                }),
            modelContext,
            artifactRoot,
            batchId,
            "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(1, creatorCalls);
        Assert.False(File.Exists(address.ManifestPath));
        Assert.False(Directory.Exists(address.DatasetRoot));
        AssertNoDownstreamArtifacts(artifactRoot, observations);
    }

    [Fact]
    public async Task
        RepositoryOwnedMetricRunner_ProvenanceMismatchFailsBeforeStepManyAndArtifacts()
    {
        var repositoryRoot = NewRepositoryRoot("repo-metric-mismatch");
        var artifactRoot = NewRoot("artifacts-metric-mismatch");
        var batchId =
            "negative-dpo-generator-repository-owned-metric-mismatch";
        var sourcePairs = BuildProjectPairs(batchId, "metric-mismatch");
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("metric-mismatch", sourcePairs);
        var repositoryPairs = CreateRepositoryPairs(
            address,
            identity,
            sourcePairs).ToArray();
        var sealedDataset = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address,
            identity,
            repositoryPairs);
        Assert.True(sealedDataset.IsSuccess, sealedDataset.ErrorMessage);
        var modelContext = CreateModelContext(
            repositoryRoot,
            "metric-mismatch",
            allowColdStart: true,
            "unused because reopened row authorship is mismatched");
        var differentIdentity = CreateRequestIdentity(
            "metric-mismatch-different",
            sourcePairs);
        repositoryPairs[0] = repositoryPairs[0] with
        {
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                FakeOpenTeacherProvenance(differentIdentity, 999))
        };
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();
        var metricRunner = CreateProjectWikiDpoMetricRunner(observations);

        var result =
            await metricRunner.TrainAndEvaluateRepositoryOwnedNegativeDpoAsync(
                NegativeDpoGeneratorTrainingRunner.UnitId,
                batchId,
                sourcePairs.Select(ToMetricPairForTesting).ToArray(),
                address,
                new RepositoryOwnedDpoDatasetManifestIdentity(
                    address.ManifestRelativePath,
                    sealedDataset.Value!.ManifestSha256),
                repositoryPairs,
                modelContext,
                BuildProjectPairs($"{batchId}-heldout", "heldout")
                    .Select(ToMetricPairForTesting)
                    .ToArray(),
                artifactRoot);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ProjectWikiDpoMetricRunner
                .RepositoryOwnedNegativeDpoAuthorshipErrorCode,
            result.ErrorCode);
        Assert.Empty(observations);
        Assert.False(Directory.Exists(Path.Combine(artifactRoot, "dpo-runs")));
    }

    [Fact]
    public async Task OrchestratorNegativeDpoRoute_UsesRepositoryOwnedContext()
    {
        var repositoryRoot = NewRepositoryRoot("repo-orchestrator");
        var artifactRoot = NewRoot("artifacts-orchestrator");
        var batchId = "negative-dpo-generator-repository-owned-orchestrator";
        var sourcePairs = BuildProjectPairs(batchId, "orchestrator-reopened");
        var address = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("orchestrator", sourcePairs);
        var creatorCalls = 0;
        var context = CreateContext(
            repositoryRoot,
            identity,
            request =>
            {
                creatorCalls++;
                return CreateRepositoryPairs(
                    request.Address,
                    request.RequestIdentity,
                    sourcePairs);
            });
        var modelContext = CreateModelContext(
            repositoryRoot,
            "orchestrator",
            allowColdStart: true,
            "authorised first test checkpoint for orchestrator route");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();
        var orchestrator = new ProjectWikiTrainingOrchestrator(
            negativeDpo: CreatePipeline(observations));

        var result = await orchestrator.RunWithRepositoryOwnedNegativeDpoAsync(
            "Train negative DPO rejected side for project wiki.",
            artifactRoot,
            context,
            modelContext,
            batchId,
            "https://models.local");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ProjectWikiRequestRouteKind.NegativeDpoTraining,
            result.Value!.RoutingDecision.RouteKind);
        Assert.Equal(50, result.Value.TrainingRowCount);
        Assert.Equal(1, creatorCalls);
        var evidence = result.Value.RepositoryOwnedDpoDatasetEvidence;
        Assert.NotNull(evidence);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset,
            evidence!.Source);
        Assert.Equal(address.ManifestRelativePath,
            evidence.ManifestRelativePath);
        var reopened = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
            address,
            identity);
        Assert.True(reopened.IsSuccess, reopened.ErrorMessage);
        Assert.Equal(
            reopened.Value!.Manifest.ManifestSha256,
            evidence.ManifestSha256);
        Assert.Equal(
            ComputeReopenedProjectPairsSha256(address, reopened.Value.Pairs),
            evidence.ReopenedProjectPairsSha256);
        Assert.NotNull(result.Value.RepositoryOwnedModelCheckpointEvidence);
        Assert.Equal(
            address.ManifestRelativePath,
            result.Value.RepositoryOwnedModelCheckpointEvidence!
                .DpoManifestRelativePath);
        AssertDownstreamSawReopenedMarker(observations, "orchestrator-reopened");
        AssertRepositoryOwnedAdmissionsMatchReopenedRows(
            observations,
            reopened.Value.Pairs);
    }

    [Fact]
    public async Task
        OrchestratorNegativeDpoRoute_WithoutRepositoryContextFailsClosed()
    {
        var artifactRoot = NewRoot("artifacts-orchestrator-no-context");
        var orchestrator = new ProjectWikiTrainingOrchestrator();

        var result = await orchestrator.RunAsync(
            "Train negative DPO rejected side for project wiki.",
            artifactRoot,
            "negative-dpo-generator-repository-owned-no-context",
            "https://models.local",
            learningDataRoot: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "NEGATIVE_DPO_REPOSITORY_CONTEXT_REQUIRED",
            result.ErrorCode);
        Assert.False(Directory.Exists(Path.Combine(artifactRoot, "dpo-runs")));
    }

    [Fact]
    public async Task
        RepositoryOwnedModelCheckpoint_ExplicitColdStartPublishesStepOneThenSecondRunContinuesToStepTwo()
    {
        var repositoryRoot = NewRepositoryRoot("repo-model-lifecycle");
        var firstArtifactRoot = NewRoot("artifacts-model-lifecycle-1");
        var secondArtifactRoot = NewRoot("artifacts-model-lifecycle-2");
        var batchId =
            "negative-dpo-generator-repository-owned-model-lifecycle";
        var sourcePairs = BuildProjectPairs(batchId, "model-lifecycle");
        var dpoAddress = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("model-lifecycle", sourcePairs);
        var creatorCalls = 0;
        var dpoContext = CreateContext(
            repositoryRoot,
            identity,
            request =>
            {
                creatorCalls++;
                return CreateRepositoryPairs(
                    request.Address,
                    request.RequestIdentity,
                    sourcePairs);
            });
        var coldStartReason =
            "authorised first repository checkpoint for lifecycle test";
        var modelContext = CreateModelContext(
            repositoryRoot,
            "model-lifecycle",
            allowColdStart: true,
            coldStartReason);
        var firstObservations =
            new List<ProjectWikiDpoMetricBatchObservation>();
        var pipeline = CreatePipeline(firstObservations);

        var first = await pipeline.RunRepositoryOwnedBatchAsync(
            dpoContext,
            modelContext,
            firstArtifactRoot,
            batchId,
            "https://models.local");

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(1, creatorCalls);
        var firstEvidence =
            first.Value!.RepositoryOwnedModelCheckpointEvidence;
        Assert.NotNull(firstEvidence);
        Assert.Equal("cold_start_explicit", firstEvidence!.StartMode);
        Assert.Equal(1, firstEvidence.PublishedStep);
        Assert.Equal(coldStartReason, firstEvidence.ColdStartReason);
        Assert.Null(firstEvidence.ParentStep);
        Assert.Equal(
            dpoAddress.ManifestRelativePath,
            firstEvidence.DpoManifestRelativePath);
        Assert.Equal(
            first.Value.RepositoryOwnedDpoDataset!.ManifestSha256,
            firstEvidence.DpoManifestSha256);
        var stepOneAddress = GetModelStepAddress(modelContext, 1);
        Assert.Equal(stepOneAddress.ManifestPath,
            firstEvidence.PublishedManifestPath);
        Assert.Equal(stepOneAddress.WeightsPath,
            firstEvidence.PublishedWeightsPath);
        var openedStepOne =
            RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
                stepOneAddress);
        Assert.True(openedStepOne.IsSuccess, openedStepOne.ErrorMessage);
        Assert.Equal(
            firstEvidence.PublishedWeightsSha256,
            openedStepOne.Value!.WeightsSha256);
        var stepOneHashBeforeSecondRun =
            ComputeFileSha256(stepOneAddress.WeightsPath);

        var secondObservations =
            new List<ProjectWikiDpoMetricBatchObservation>();
        pipeline = CreatePipeline(secondObservations);
        var second = await pipeline.RunRepositoryOwnedBatchAsync(
            dpoContext,
            modelContext,
            secondArtifactRoot,
            batchId,
            "https://models.local");

        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(1, creatorCalls);
        var secondEvidence =
            second.Value!.RepositoryOwnedModelCheckpointEvidence;
        Assert.NotNull(secondEvidence);
        Assert.Equal(
            "continued_from_latest_compatible_parent",
            secondEvidence!.StartMode);
        Assert.Equal(2, secondEvidence.PublishedStep);
        Assert.Equal(1, secondEvidence.ParentStep);
        Assert.Equal(
            stepOneAddress.ManifestRelativePath,
            secondEvidence.ParentManifestRelativePath);
        Assert.Equal(
            firstEvidence.PublishedWeightsSha256,
            secondEvidence.ParentWeightsSha256);
        Assert.Equal(
            "loaded_repository_owned_parent_step_000001",
            second.Value.DpoMetrics!.PolicyInitialization);
        Assert.Equal(
            "loaded_repository_owned_parent_step_000001",
            second.Value.DpoMetrics.ReferenceInitialization);
        Assert.Equal(
            stepOneHashBeforeSecondRun,
            ComputeFileSha256(stepOneAddress.WeightsPath));

        var stepTwoAddress = GetModelStepAddress(modelContext, 2);
        var openedStepTwo =
            RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
                stepTwoAddress);
        Assert.True(openedStepTwo.IsSuccess, openedStepTwo.ErrorMessage);
        Assert.Equal(2, openedStepTwo.Value!.Manifest.Step);
        Assert.Equal(
            stepOneAddress.ManifestRelativePath,
            openedStepTwo.Value.Manifest.ParentCheckpoint!
                .RepositoryRelativeManifestPath);
        Assert.Equal(
            firstEvidence.PublishedWeightsSha256,
            openedStepTwo.Value.Manifest.ParentCheckpoint.CheckpointSha256);
        Assert.Equal(
            secondEvidence.PublishedWeightsSha256,
            openedStepTwo.Value.WeightsSha256);

        var latest = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                repositoryRoot,
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                    .DomainSlug,
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                modelContext.Request!.Address.ModelIdSlug,
                modelContext.Request.Address.ModelVersion,
                modelContext.Request.RuntimeCompatibility);
        Assert.True(latest.IsSuccess, latest.ErrorMessage);
        using (latest.Value)
        {
            Assert.Equal(2, latest.Value!.Manifest.Step);
            Assert.Equal(
                secondEvidence.PublishedWeightsSha256,
                latest.Value.WeightsSha256);
        }
    }

    [Fact]
    public async Task
        RepositoryOwnedModelCheckpoint_MissingTreeWithoutColdStartReasonFailsBeforeStepManyAndArtifacts()
    {
        var repositoryRoot = NewRepositoryRoot("repo-model-cold-denied");
        var artifactRoot = NewRoot("artifacts-model-cold-denied");
        var batchId =
            "negative-dpo-generator-repository-owned-model-cold-denied";
        var sourcePairs = BuildProjectPairs(batchId, "model-cold-denied");
        var dpoAddress = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("model-cold-denied", sourcePairs);
        var sealedDpo = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            dpoAddress,
            identity,
            CreateRepositoryPairs(dpoAddress, identity, sourcePairs));
        Assert.True(sealedDpo.IsSuccess, sealedDpo.ErrorMessage);
        var dpoContext = CreateContext(
            repositoryRoot,
            identity,
            request => throw new InvalidOperationException(
                $"Creator must not run for sealed DPO {request.BatchId}."));
        var modelContext = CreateModelContext(
            repositoryRoot,
            "model-cold-denied",
            allowColdStart: false,
            coldStartReason: null);
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations)
            .RunRepositoryOwnedBatchAsync(
                dpoContext,
                modelContext,
                artifactRoot,
                batchId,
                "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ProjectWikiDpoMetricRunner
                .RepositoryOwnedModelCheckpointColdStartNotAuthorizedErrorCode,
            result.ErrorCode);
        Assert.Empty(observations);
        Assert.False(Directory.Exists(Path.Combine(artifactRoot, "dpo-runs")));
        var stepOneAddress = GetModelStepAddress(modelContext, 1);
        Assert.False(File.Exists(stepOneAddress.ManifestPath));
        Assert.False(File.Exists(stepOneAddress.WeightsPath));
    }

    [Theory]
    [InlineData("corrupt")]
    [InlineData("incompatible")]
    [InlineData("ambiguous")]
    public async Task
        RepositoryOwnedModelCheckpoint_UnsafeLatestParentFailsBeforeStepManyAndDoesNotColdStart(
            string caseName)
    {
        var repositoryRoot =
            NewRepositoryRoot("repo-model-unsafe-parent-" + caseName);
        var artifactRoot = NewRoot("artifacts-model-unsafe-parent-" + caseName);
        var batchId =
            "negative-dpo-generator-repository-owned-model-unsafe-parent-" +
            caseName;
        var sourcePairs = BuildProjectPairs(batchId, "model-unsafe-parent");
        var dpoAddress = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity(
            "model-unsafe-parent-" + caseName,
            sourcePairs);
        var sealedDpo = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            dpoAddress,
            identity,
            CreateRepositoryPairs(dpoAddress, identity, sourcePairs));
        Assert.True(sealedDpo.IsSuccess, sealedDpo.ErrorMessage);
        var dpoContext = CreateContext(
            repositoryRoot,
            identity,
            request => throw new InvalidOperationException(
                $"Creator must not run for sealed DPO {request.BatchId}."));
        var modelContext = CreateModelContext(
            repositoryRoot,
            "model-unsafe-parent-" + caseName,
            allowColdStart: true,
            coldStartReason:
                "allowed only for a clean missing tree, never unsafe parents");
        var expectedError = caseName switch
        {
            "corrupt" => RepositoryOwnedModelCheckpointLoader
                .CorruptCandidateErrorCode,
            "incompatible" => RepositoryOwnedModelCheckpointLoader
                .IncompatibleCandidateErrorCode,
            _ => RepositoryOwnedModelCheckpointLoader.AmbiguousCandidateErrorCode
        };

        if (caseName == "corrupt")
        {
            var stepOne = SealRepositoryOwnedModelCheckpoint(
                modelContext,
                dpoAddress,
                sealedDpo.Value!,
                step: 1,
                compatibility: modelContext.Request!.RuntimeCompatibility);
            File.AppendAllText(
                stepOne.WeightsPath,
                "tampered after manifest seal",
                Encoding.UTF8);
        }
        else if (caseName == "incompatible")
        {
            _ = SealRepositoryOwnedModelCheckpoint(
                modelContext,
                dpoAddress,
                sealedDpo.Value!,
                step: 1,
                compatibility:
                    CreateModelRuntimeCompatibility(
                        "model-unsafe-parent-different-runtime"));
        }
        else
        {
            _ = SealRepositoryOwnedModelCheckpoint(
                modelContext,
                dpoAddress,
                sealedDpo.Value!,
                step: 1,
                compatibility: modelContext.Request!.RuntimeCompatibility);
            var checkpointsRoot = Path.GetDirectoryName(
                GetModelStepAddress(modelContext, 1).CheckpointRoot)!;
            Directory.CreateDirectory(Path.Combine(checkpointsRoot, "1"));
        }

        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        var result = await CreatePipeline(observations)
            .RunRepositoryOwnedBatchAsync(
                dpoContext,
                modelContext,
                artifactRoot,
                batchId,
                "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError, result.ErrorCode);
        Assert.Null(result.Value);
        AssertNoDownstreamArtifacts(artifactRoot, observations);
        var stepTwoAddress = GetModelStepAddress(modelContext, 2);
        Assert.False(File.Exists(stepTwoAddress.ManifestPath));
        Assert.False(File.Exists(stepTwoAddress.WeightsPath));
    }

    [Fact]
    public async Task
        RepositoryOwnedModelCheckpoint_DpoManifestHashMismatchFailsBeforeStepManyAndArtifacts()
    {
        var repositoryRoot = NewRepositoryRoot("repo-model-dpo-mismatch");
        var artifactRoot = NewRoot("artifacts-model-dpo-mismatch");
        var batchId =
            "negative-dpo-generator-repository-owned-model-dpo-mismatch";
        var sourcePairs = BuildProjectPairs(batchId, "model-dpo-mismatch");
        var dpoAddress = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity(
            "model-dpo-mismatch",
            sourcePairs);
        var repositoryPairs = CreateRepositoryPairs(
            dpoAddress,
            identity,
            sourcePairs);
        var sealedDpo = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            dpoAddress,
            identity,
            repositoryPairs);
        Assert.True(sealedDpo.IsSuccess, sealedDpo.ErrorMessage);
        var modelContext = CreateModelContext(
            repositoryRoot,
            "model-dpo-mismatch",
            allowColdStart: true,
            "unused because the reopened DPO manifest hash is wrong");
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();
        var metricRunner = CreateProjectWikiDpoMetricRunner(observations);
        var wrongIdentity = new RepositoryOwnedDpoDatasetManifestIdentity(
            dpoAddress.ManifestRelativePath,
            Sha256Hex("wrong reopened DPO manifest hash"));

        var result =
            await metricRunner.TrainAndEvaluateRepositoryOwnedNegativeDpoAsync(
                NegativeDpoGeneratorTrainingRunner.UnitId,
                batchId,
                sourcePairs.Select(ToMetricPairForTesting).ToArray(),
                dpoAddress,
                wrongIdentity,
                repositoryPairs,
                modelContext,
                BuildProjectPairs($"{batchId}-heldout", "heldout")
                    .Select(ToMetricPairForTesting)
                    .ToArray(),
                artifactRoot);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ProjectWikiDpoMetricRunner
                .RepositoryOwnedModelCheckpointDpoManifestMismatchErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        AssertNoDownstreamArtifacts(artifactRoot, observations);
        var stepOneAddress = GetModelStepAddress(modelContext, 1);
        Assert.False(File.Exists(stepOneAddress.ManifestPath));
        Assert.False(File.Exists(stepOneAddress.WeightsPath));
    }

    [Fact]
    public async Task
        RepositoryOwnedModelCheckpoint_CancellationAfterTokenizationFailsBeforeStepManyAndChildCheckpoint()
    {
        var repositoryRoot = NewRepositoryRoot("repo-model-cancel-before-mutation");
        var artifactRoot = NewRoot("artifacts-model-cancel-before-mutation");
        var batchId =
            "negative-dpo-generator-repository-owned-model-cancel";
        var sourcePairs = BuildProjectPairs(batchId, "model-cancel");
        var dpoAddress = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity("model-cancel", sourcePairs);
        var repositoryPairs = CreateRepositoryPairs(
            dpoAddress,
            identity,
            sourcePairs);
        var sealedDpo = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            dpoAddress,
            identity,
            repositoryPairs);
        Assert.True(sealedDpo.IsSuccess, sealedDpo.ErrorMessage);
        var modelContext = CreateModelContext(
            repositoryRoot,
            "model-cancel",
            allowColdStart: true,
            "unused because cancellation happens before trainer mutation");
        using var cancellation = new CancellationTokenSource();
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();
        var metricRunner = CreateProjectWikiDpoMetricRunner(observations,
            observation =>
            {
                if (string.Equals(
                        observation.Stage,
                        ProjectWikiDpoMetricRunner
                            .TrainingTokenizationObservationStage,
                        StringComparison.Ordinal))
                {
                    // Человеческий смысл проверки: отмена может прийти после
                    // чтения DPO с диска, но до обучения. В этот момент нельзя
                    // ни делать StepMany, ни печатать успешный checkpoint.
                    cancellation.Cancel();
                }
            });

        var result =
            await metricRunner.TrainAndEvaluateRepositoryOwnedNegativeDpoAsync(
                NegativeDpoGeneratorTrainingRunner.UnitId,
                batchId,
                sourcePairs.Select(ToMetricPairForTesting).ToArray(),
                dpoAddress,
                new RepositoryOwnedDpoDatasetManifestIdentity(
                    dpoAddress.ManifestRelativePath,
                    sealedDpo.Value!.ManifestSha256),
                repositoryPairs,
                modelContext,
                BuildProjectPairs($"{batchId}-heldout", "heldout")
                    .Select(ToMetricPairForTesting)
                    .ToArray(),
                artifactRoot,
                cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_METRIC_CANCELLED", result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Contains(
            observations,
            observation => string.Equals(
                observation.Stage,
                ProjectWikiDpoMetricRunner.TrainingTokenizationObservationStage,
                StringComparison.Ordinal));
        Assert.DoesNotContain(
            observations,
            observation => string.Equals(
                observation.Stage,
                ProjectWikiDpoMetricRunner.StepManyTrainingObservationStage,
                StringComparison.Ordinal));
        Assert.False(Directory.Exists(Path.Combine(artifactRoot, "dpo-runs")));
        var stepOneAddress = GetModelStepAddress(modelContext, 1);
        Assert.False(File.Exists(stepOneAddress.ManifestPath));
        Assert.False(File.Exists(stepOneAddress.WeightsPath));
    }

    [Fact]
    public async Task
        RepositoryOwnedModelCheckpoint_CancellationAfterStepManyFailsBeforeCanonicalPublication()
    {
        var repositoryRoot = NewRepositoryRoot("repo-model-cancel-after-stepmany");
        var artifactRoot = NewRoot("artifacts-model-cancel-after-stepmany");
        var batchId =
            "negative-dpo-generator-repository-owned-model-cancel-after-step";
        var sourcePairs = BuildProjectPairs(batchId, "model-cancel-after-step");
        var dpoAddress = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity(
            "model-cancel-after-step",
            sourcePairs);
        var repositoryPairs = CreateRepositoryPairs(
            dpoAddress,
            identity,
            sourcePairs);
        var sealedDpo = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            dpoAddress,
            identity,
            repositoryPairs);
        Assert.True(sealedDpo.IsSuccess, sealedDpo.ErrorMessage);
        var modelContext = CreateModelContext(
            repositoryRoot,
            "model-cancel-after-step",
            allowColdStart: true,
            "unused because cancellation happens before canonical seal");
        using var cancellation = new CancellationTokenSource();
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();
        var metricRunner = CreateProjectWikiDpoMetricRunner(observations,
            observation =>
            {
                if (string.Equals(
                        observation.Stage,
                        ProjectWikiDpoMetricRunner
                            .StepManyTrainingObservationStage,
                        StringComparison.Ordinal))
                {
                    // Человеческий смысл проверки: callback срабатывает прямо
                    // перед StepMany; сам StepMany всё ещё должен успеть
                    // изменить только scratch checkpoint, а затем route обязан
                    // заметить отмену и не запечатывать canonical child.
                    cancellation.Cancel();
                }
            });

        var result =
            await metricRunner.TrainAndEvaluateRepositoryOwnedNegativeDpoAsync(
                NegativeDpoGeneratorTrainingRunner.UnitId,
                batchId,
                sourcePairs.Select(ToMetricPairForTesting).ToArray(),
                dpoAddress,
                new RepositoryOwnedDpoDatasetManifestIdentity(
                    dpoAddress.ManifestRelativePath,
                    sealedDpo.Value!.ManifestSha256),
                repositoryPairs,
                modelContext,
                BuildProjectPairs($"{batchId}-heldout", "heldout")
                    .Select(ToMetricPairForTesting)
                    .ToArray(),
                artifactRoot,
                cancellation.Token);

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_METRIC_CANCELLED", result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Contains(
            observations,
            observation => string.Equals(
                observation.Stage,
                ProjectWikiDpoMetricRunner.StepManyTrainingObservationStage,
                StringComparison.Ordinal));
        var stepOneAddress = GetModelStepAddress(modelContext, 1);
        Assert.False(File.Exists(stepOneAddress.ManifestPath));
        Assert.False(File.Exists(stepOneAddress.WeightsPath));
        Assert.False(Directory.Exists(stepOneAddress.CheckpointRoot));
        Assert.DoesNotContain(
            EnumerateRelativeFiles(repositoryRoot),
            path => path.Contains(
                "policy_step_",
                StringComparison.OrdinalIgnoreCase) ||
                path.Contains(
                    "metrics_step_",
                    StringComparison.OrdinalIgnoreCase) ||
                path.Contains(
                    ".dpo_step_",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task
        RepositoryOwnedModelCheckpoint_OutputInsideNextCheckpointFailsBeforeStepManyAndChildArtifacts()
    {
        var repositoryRoot = NewRepositoryRoot("repo-model-output-boundary");
        var batchId =
            "negative-dpo-generator-repository-owned-model-output-boundary";
        var sourcePairs = BuildProjectPairs(batchId, "model-output-boundary");
        var dpoAddress = CreateAddress(repositoryRoot);
        var identity = CreateRequestIdentity(
            "model-output-boundary",
            sourcePairs);
        var sealedDpo = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            dpoAddress,
            identity,
            CreateRepositoryPairs(dpoAddress, identity, sourcePairs));
        Assert.True(sealedDpo.IsSuccess, sealedDpo.ErrorMessage);
        var dpoContext = CreateContext(
            repositoryRoot,
            identity,
            request => throw new InvalidOperationException(
                $"Creator must not run for sealed DPO {request.BatchId}."));
        var modelContext = CreateModelContext(
            repositoryRoot,
            "model-output-boundary",
            allowColdStart: true,
            "boundary test starts only if output is outside checkpoint");
        var stepOneAddress = GetModelStepAddress(modelContext, 1);
        var observations = new List<ProjectWikiDpoMetricBatchObservation>();

        // Человеческий смысл проверки: если рабочие отчёты пишутся внутрь
        // будущего чекпоинта, публикация модели обязана остановиться до
        // изменения весов, иначе временные файлы станут частью модели.
        var result = await CreatePipeline(observations)
            .RunRepositoryOwnedBatchAsync(
                dpoContext,
                modelContext,
                stepOneAddress.CheckpointRoot,
                batchId,
                "https://models.local");

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ProjectWikiDpoMetricRunner
                .RepositoryOwnedModelCheckpointOutputOverlapsCallerCheckpointDirectoryErrorCode,
            result.ErrorCode);
        Assert.Null(result.Value);
        Assert.DoesNotContain(
            observations,
            observation => string.Equals(
                observation.Stage,
                ProjectWikiDpoMetricRunner.StepManyTrainingObservationStage,
                StringComparison.Ordinal));
        Assert.False(Directory.Exists(Path.Combine(
            stepOneAddress.CheckpointRoot,
            "dpo-runs")));
        Assert.False(File.Exists(stepOneAddress.ManifestPath));
        Assert.False(File.Exists(stepOneAddress.WeightsPath));
    }

    [Fact]
    public void MappingRoundTripsStrictCarrierWithOpenTeacherSideAndRejectsUnsafeAuthors()
    {
        var repositoryRoot = NewRepositoryRoot("repo-roundtrip");
        var address = CreateAddress(repositoryRoot);
        var sourcePair = BuildProjectPairs(
            "negative-dpo-generator-repository-owned-roundtrip",
            "roundtrip")[7];
        var identity = CreateRequestIdentity("roundtrip", [sourcePair]);

        var mapped = NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
            .MapProjectPairToRepositoryPair(
                address,
                sourcePair,
                FakeOpenTeacherProvenance(identity, 7),
                DpoPairSideAuthorKind.DeterministicGenerator,
                DpoPairSideAuthorKind.OpenTeacher);

        Assert.True(mapped.IsSuccess, mapped.ErrorMessage);
        Assert.Equal(
            DpoPairSideAuthorKind.DeterministicGenerator,
            mapped.Value!.ChosenAuthorship!.AuthorKind);
        Assert.Equal(
            DpoPairSideAuthorKind.OpenTeacher,
            mapped.Value.RejectedAuthorship!.AuthorKind);
        Assert.Null(mapped.Value.ChosenAuthorship.OpenTeacherPassport);
        Assert.Equal(
            mapped.Value.TeacherProvenance,
            mapped.Value.RejectedAuthorship.OpenTeacherPassport);
        Assert.DoesNotContain(sourcePair.Chosen, mapped.Value.Prompt,
            StringComparison.Ordinal);
        Assert.DoesNotContain(sourcePair.Rejected, mapped.Value.Prompt,
            StringComparison.Ordinal);

        var roundTrip = NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
            .MapRepositoryPairToProjectPair(address, mapped.Value);

        Assert.True(roundTrip.IsSuccess, roundTrip.ErrorMessage);
        Assert.Equal(sourcePair.PairId, roundTrip.Value!.PairId);
        Assert.Equal(
            sourcePair.DocumentationUnitId,
            roundTrip.Value.DocumentationUnitId);
        Assert.Equal(sourcePair.A, roundTrip.Value.A);
        Assert.Equal(sourcePair.Chosen, roundTrip.Value.Chosen);
        Assert.Equal(sourcePair.Rejected, roundTrip.Value.Rejected);
        Assert.Equal(sourcePair.NegativeType, roundTrip.Value.NegativeType);
        Assert.Equal(sourcePair.EvidenceIds, roundTrip.Value.EvidenceIds);

        var deterministicOnly =
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .MapProjectPairToRepositoryPair(
                    address,
                    sourcePair,
                    FakeOpenTeacherProvenance(identity, 8),
                    DpoPairSideAuthorKind.DeterministicGenerator,
                    DpoPairSideAuthorKind.DeterministicGenerator);

        Assert.False(deterministicOnly.IsSuccess);
        Assert.Equal(
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .InvalidErrorCode,
            deterministicOnly.ErrorCode);

        var paidSide = NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
            .MapProjectPairToRepositoryPair(
                address,
                sourcePair,
                FakeOpenTeacherProvenance(identity, 9),
                DpoPairSideAuthorKind.PaidExternalModel,
                DpoPairSideAuthorKind.OpenTeacher);

        Assert.False(paidSide.IsSuccess);
        Assert.Equal(
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .InvalidErrorCode,
            paidSide.ErrorCode);
    }

    private string NewRoot(string kind)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "negative-dpo-generator-repository-owned-tests",
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
            "negative dpo generator repository-owned fixture solution",
            Encoding.UTF8);
        Directory.CreateDirectory(
            Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
        return repositoryRoot;
    }

    private static RepositoryOwnedDpoDatasetAddress CreateAddress(
        string repositoryRoot)
    {
        var address = NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
            .CreateAddress(repositoryRoot);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext
        CreateContext(
            string repositoryRoot,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            Func<NegativeDpoGeneratorRepositoryOwnedDpoDatasetCreationRequest,
                CancellationToken,
                Task<OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>>>
                createPairsAsync)
    {
        var context = NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
            .CreateContext(
                repositoryRoot,
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                    .DefaultDatasetVersion,
                identity,
                createPairsAsync);
        Assert.True(context.IsSuccess, context.ErrorMessage);
        return context.Value!;
    }

    private static NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext
        CreateContext(
            string repositoryRoot,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            Func<NegativeDpoGeneratorRepositoryOwnedDpoDatasetCreationRequest,
                IReadOnlyList<ContinuousLearning.DpoPair>> createPairs)
        => CreateContext(
            repositoryRoot,
            identity,
            (request, _) => Task.FromResult(OperationResult<
                IReadOnlyList<ContinuousLearning.DpoPair>>.Ok(
                createPairs(request))));

    private static NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
        CreateModelContext(
            string repositoryRoot,
            string scope,
            bool allowColdStart,
            string? coldStartReason)
    {
        var context = NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
            .CreateModelCheckpointContext(
                repositoryRoot,
                "negative-dpo-generator-test-" +
                Sha256Hex("model-id\n" + scope)[..12],
                "v" + Sha256Hex("model-version\n" + scope)[..12],
                CreateModelRuntimeCompatibility(scope),
                allowColdStart,
                coldStartReason,
                [
                    "negative-dpo-generator-model-test-" +
                    Sha256Hex("model-provenance\n" + scope)[..16]
                ]);
        Assert.True(context.IsSuccess, context.ErrorMessage);
        return context.Value!;
    }

    private static RepositoryOwnedModelRuntimeCompatibility
        CreateModelRuntimeCompatibility(string scope)
        => new(
            ConfigSha256: Sha256Hex("project-wiki-model-config\n" + scope),
            TopologySha256:
                Sha256Hex("project-wiki-model-topology\n" + scope),
            TokenizerSha256:
                Sha256Hex("project-wiki-model-tokenizer\n" + scope),
            TrainerSha256:
                Sha256Hex("project-wiki-dpo-trainer\n" + scope),
            SchemaSha256:
                Sha256Hex("project-wiki-model-schema\n" + scope),
            CompatibilityKey: "negative-dpo-generator-" +
                Sha256Hex("compatibility-key\n" + scope)[..12]);

    private static RepositoryOwnedModelCheckpointAddress GetModelStepAddress(
        NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext context,
        int step)
    {
        var address = context.Request!.Address.CreateStepAddress(step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static RepositoryOwnedModelCheckpointAddress
        SealRepositoryOwnedModelCheckpoint(
            NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                modelContext,
            RepositoryOwnedDpoDatasetAddress dpoAddress,
            RepositoryOwnedDpoDatasetManifest sealedDpo,
            int step,
            RepositoryOwnedModelRuntimeCompatibility compatibility)
    {
        var address = GetModelStepAddress(modelContext, step);
        using var rawWeights = new MemoryStream(Encoding.UTF8.GetBytes(
            "negative dpo repository-owned parent checkpoint fixture\n" +
            $"{modelContext.Request!.Address.ModelIdSlug}\n" +
            $"{modelContext.Request.Address.ModelVersion}\n" +
            $"{step}\n"));
        var seal = RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
            address,
            rawWeights,
            new RepositoryOwnedModelCheckpointManifestRequest(
                ParentCheckpoint: null,
                ColdStartReason:
                    "test fixture parent checkpoint for failure matrix",
                DpoDatasetManifest:
                    new RepositoryOwnedDpoDatasetManifestIdentity(
                        dpoAddress.ManifestRelativePath,
                        sealedDpo.ManifestSha256),
                RuntimeCompatibility: compatibility,
                CreatedAtUtc: DateTimeOffset.UnixEpoch.AddSeconds(step),
                ProvenanceRefs:
                [
                    "negative-dpo-generator-model-parent-fixture-" +
                    Sha256Hex("model-parent-fixture\n" + step)[..16]
                ]));
        Assert.True(seal.IsSuccess, seal.ErrorMessage);
        return address;
    }

    private static ProjectDocumentationDpoPair[] BuildProjectPairs(
        string batchId,
        string reopenedMarker)
    {
        var built = new NegativeDpoGeneratorBatchBuilder()
            .BuildBatchAsync(batchId)
            .GetAwaiter()
            .GetResult();
        Assert.True(built.IsSuccess, built.ErrorMessage);
        Assert.Equal(50, built.Value!.Pairs.Length);
        return built.Value.Pairs
            .Select((pair, index) => pair with
            {
                Chosen =
                    $"{pair.Chosen}\nSEALED_REOPENED_MARKER " +
                    $"{reopenedMarker} {index:00}"
            })
            .ToArray();
    }

    private static ProjectDocumentationDpoPair[] SelectCreatorRows(
        ProjectDocumentationDpoPair[] sourcePairs,
        int rowCount)
    {
        if (rowCount <= sourcePairs.Length)
            return sourcePairs.Take(rowCount).ToArray();

        return
        [
            .. sourcePairs,
            .. sourcePairs
                .Take(rowCount - sourcePairs.Length)
                .Select((pair, index) => pair with
                {
                    PairId = $"{pair.PairId}-extra-{index:00}",
                    Chosen = $"{pair.Chosen}\nEXTRA_PRESEAL_ROW {index:00}",
                    Rejected = $"{pair.Rejected}\nEXTRA_PRESEAL_ROW {index:00}",
                    EvidenceIds = [.. pair.EvidenceIds, $"extra-{index:00}"]
                })
        ];
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity CreateRequestIdentity(
        string scope,
        IReadOnlyList<ProjectDocumentationDpoPair> pairs)
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
                    "negative-dpo-generator-test-generation",
                    "v1",
                    Sha256Hex("generation-policy\n" + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "negative-dpo-generator-test-creator",
                    "v1",
                    Sha256Hex("creator-policy\n" + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private static IReadOnlyList<ContinuousLearning.DpoPair>
        CreateRepositoryPairs(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            IReadOnlyList<ProjectDocumentationDpoPair> projectPairs,
            DpoPairSideAuthorKind chosenAuthor =
                DpoPairSideAuthorKind.DeterministicGenerator,
            DpoPairSideAuthorKind rejectedAuthor =
                DpoPairSideAuthorKind.OpenTeacher)
        => projectPairs.Select((pair, index) =>
        {
            var mapped =
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                    .MapProjectPairToRepositoryPair(
                        address,
                        pair,
                        FakeOpenTeacherProvenance(identity, index),
                        chosenAuthor,
                        rejectedAuthor);
            Assert.True(mapped.IsSuccess, mapped.ErrorMessage);
            return mapped.Value!;
        }).ToArray();

    private static OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>
        TryCreateRepositoryPairs(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            IReadOnlyList<ProjectDocumentationDpoPair> projectPairs,
            DpoPairSideAuthorKind chosenAuthor,
            DpoPairSideAuthorKind rejectedAuthor)
    {
        var pairs = new List<ContinuousLearning.DpoPair>();
        for (var index = 0; index < projectPairs.Count; index++)
        {
            var mapped =
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                    .MapProjectPairToRepositoryPair(
                        address,
                        projectPairs[index],
                        FakeOpenTeacherProvenance(identity, index),
                        chosenAuthor,
                        rejectedAuthor);
            if (!mapped.IsSuccess || mapped.Value is null)
            {
                return mapped.MapError<
                    IReadOnlyList<ContinuousLearning.DpoPair>>();
            }

            pairs.Add(mapped.Value);
        }

        return OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>
            .Ok(pairs.ToArray());
    }

    private static TrainingTeacherProvenance FakeOpenTeacherProvenance(
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        int index)
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
                $"negative-dpo-generator-test-open-teacher-{index:000}",
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

    private static NegativeDpoGeneratorTrainingPipeline CreatePipeline(
        List<ProjectWikiDpoMetricBatchObservation> observations)
        => new(dpoMetricRunner: CreateProjectWikiDpoMetricRunner(
            observations));

    private static ProjectWikiDpoMetricRunner CreateProjectWikiDpoMetricRunner(
        List<ProjectWikiDpoMetricBatchObservation> observations,
        Action<ProjectWikiDpoMetricBatchObservation>? afterObservation = null)
        => new(
            CreateProjectWikiLegalTeacherAdmissionAuthority(),
            epochCount: 1,
            learningRate: 5e-2f,
            vocabSize: 128,
            contextLen: 16,
            hiddenDim: 16,
            numLayers: 1,
            numHeads: 4,
            numKvHeads: 1,
            ffnHiddenDim: 32,
            deterministicFreshInitialization: true,
            batchObserverForTesting: observation =>
            {
                observations.Add(observation);
                afterObservation?.Invoke(observation);
            });

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
        if (!configuration.IsSuccess || configuration.Value is null)
        {
            throw new InvalidOperationException(
                "ProjectWiki legal-teacher test configuration admission " +
                $"failed: {configuration.ErrorCode}");
        }

        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration.Value,
                static () => true);
    }

    private static void AssertDownstreamSawReopenedMarker(
        IEnumerable<ProjectWikiDpoMetricBatchObservation> observations,
        string marker)
    {
        var trainingTokenization = observations
            .Where(observation => string.Equals(
                observation.Stage,
                ProjectWikiDpoMetricRunner.TrainingTokenizationObservationStage,
                StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(50, trainingTokenization.Length);
        Assert.Contains(
            trainingTokenization,
            observation => observation.Pair.Chosen.Contains(
                marker,
                StringComparison.Ordinal));
    }

    private static void AssertRepositoryOwnedAdmissionsMatchReopenedRows(
        IEnumerable<ProjectWikiDpoMetricBatchObservation> observations,
        IReadOnlyList<ContinuousLearning.DpoPair> reopenedRepositoryPairs)
    {
        var trainingTokenization = observations
            .Where(observation => string.Equals(
                observation.Stage,
                ProjectWikiDpoMetricRunner.TrainingTokenizationObservationStage,
                StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(50, trainingTokenization.Length);
        Assert.Equal(50, reopenedRepositoryPairs.Count);

        for (var index = 0; index < trainingTokenization.Length; index++)
        {
            AssertRepositoryOwnedAdmissionMatchesPair(
                trainingTokenization[index],
                reopenedRepositoryPairs[index]);
        }

        var stepMany = observations
            .Where(observation => string.Equals(
                observation.Stage,
                ProjectWikiDpoMetricRunner.StepManyTrainingObservationStage,
                StringComparison.Ordinal))
            .ToArray();
        Assert.Equal(50, stepMany.Length);
        for (var index = 0; index < stepMany.Length; index++)
        {
            AssertRepositoryOwnedAdmissionMatchesPair(
                stepMany[index],
                reopenedRepositoryPairs[index]);
        }
    }

    private static void AssertRepositoryOwnedAdmissionMatchesPair(
        ProjectWikiDpoMetricBatchObservation observation,
        ContinuousLearning.DpoPair repositoryPair)
    {
        Assert.Equal(repositoryPair, observation.RepositoryOwnedTrainingPair);
        Assert.NotNull(observation.Batch.Admission);
        Assert.NotNull(repositoryPair.TeacherProvenance);
        Assert.NotNull(repositoryPair.ChosenAuthorship);
        Assert.NotNull(repositoryPair.RejectedAuthorship);
        Assert.Equal(
            repositoryPair.ChosenAuthorship!.AuthorKind,
            observation.Batch.Admission!.ChosenAuthor);
        Assert.Equal(
            repositoryPair.RejectedAuthorship!.AuthorKind,
            observation.Batch.Admission.RejectedAuthor);
        Assert.True(
            observation.Batch.Admission.ChosenAuthor is
                DpoPairSideAuthorKind.OpenTeacher ||
            observation.Batch.Admission.RejectedAuthor is
                DpoPairSideAuthorKind.OpenTeacher);
        Assert.False(
            observation.Batch.Admission.ChosenAuthor is
                DpoPairSideAuthorKind.DeterministicGenerator &&
            observation.Batch.Admission.RejectedAuthor is
                DpoPairSideAuthorKind.DeterministicGenerator);
        var passport = repositoryPair.ChosenAuthorship.AuthorKind is
            DpoPairSideAuthorKind.OpenTeacher
            ? repositoryPair.ChosenAuthorship.OpenTeacherPassport
            : repositoryPair.RejectedAuthorship.OpenTeacherPassport;
        Assert.Equal(repositoryPair.TeacherProvenance, passport);
        Assert.Equal(64,
            repositoryPair.TeacherProvenance!.TrainingContentSha256.Length);
    }

    private static void AssertNoDownstreamArtifacts(
        string artifactRoot,
        List<ProjectWikiDpoMetricBatchObservation> observations)
    {
        Assert.Empty(observations);
        Assert.False(Directory.Exists(Path.Combine(artifactRoot, "dpo-runs")));
    }

    private static string[] EnumerateRelativeFiles(string root)
        => Directory.Exists(root)
            ? Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(root, path))
                .ToArray()
            : [];

    private static string ComputeReopenedProjectPairsSha256(
        RepositoryOwnedDpoDatasetAddress address,
        IReadOnlyList<ContinuousLearning.DpoPair> repositoryPairs)
    {
        var projectPairs = repositoryPairs.Select(pair =>
        {
            var mapped = NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                .MapRepositoryPairToProjectPair(address, pair);
            Assert.True(mapped.IsSuccess, mapped.ErrorMessage);
            return mapped.Value!;
        }).ToArray();

        return NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
            .ComputeProjectPairsSha256(projectPairs);
    }

    private static ProjectWikiDpoMetricPair ToMetricPairForTesting(
        ProjectDocumentationDpoPair pair)
        => new(
            pair.PairId,
            pair.A,
            $"{pair.Chosen}\n{BuildChosenSignalForTesting(pair.NegativeType)}",
            $"{pair.Rejected}\n{BuildRejectedSignalForTesting(pair.NegativeType)}");

    private static string BuildChosenSignalForTesting(string negativeType)
        => negativeType switch
        {
            "missing_location" => "PASS_NEGATIVE_DPO GOOD_REJECTED_CONTRAST SOURCE_LOCATION_PRESENT SOURCE_PATH_AND_LINE_PRESENT source_path_and_line_present: true wrong_source_location_not_from_code_trace: false",
            _ => "PASS_NEGATIVE_DPO GOOD_REJECTED_CONTRAST SPECIFIC_WRONG_REASON EVIDENCE_GROUNDED_WRONG_ANNOTATION"
        };

    private static string BuildRejectedSignalForTesting(string negativeType)
        => negativeType switch
        {
            "missing_location" => "FAIL_NEGATIVE_DPO MISSING_LOCATION MISSING_LOCATION MISSING_LOCATION WRONG_SOURCE_LOCATION WRONG_SOURCE_LOCATION wrong_source_location_not_from_code_trace: true source_path_and_line_present: false",
            _ => ""
        };

    private static string Sha256Hex(string text)
        => Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    private static string ComputeFileSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
