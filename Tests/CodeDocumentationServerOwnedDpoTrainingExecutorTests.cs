// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;
using LoRA;
using TransformerModel;
using TransformerModel.Config;
using TrainingLoop.Optimizers;

namespace LlmFromScratch.Tests;

public sealed class CodeDocumentationServerOwnedDpoTrainingExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_RealTinyModelMutationPublishesDpoTrainerFinalArtifactsUnderSealedAttempt()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var fakeRepositoryRoot = CreateFakeRepositoryRoot();
        var exactMaterialObserved = false;
        LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = (_, batches) =>
        {
            var batch = Assert.Single(batches).Batch;
            Assert.Equal(DPO.Models.DpoExactPairMaterial.Version, batch.MaterialVersion);
            var material = Assert.IsType<DPO.Models.DpoExactPairMaterial>(batch.ExactMaterial);
            Assert.Equal(Pair().Prompt, material.Prompt);
            Assert.Equal(Pair().Chosen, material.Chosen);
            Assert.Equal(Pair().Rejected, material.Rejected);
            Assert.All(batch.ChosenResponseMask!.Data.Take(material.PromptLength),
                value => Assert.Equal(0f, value));
            Assert.Equal(1f, batch.ChosenResponseMask.Data[material.PromptLength]);
            Assert.Equal(1f, batch.RejectedResponseMask!.Data[material.RejectedLength - 1]);
            exactMaterialObserved = true;
        };
        try
        {
            var trainingRunId = "code-doc-run:real-tiny-mutation";
            var address = CreateAddress(fakeRepositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var beforeSha256 = Sha256OfModel(session.Model);
            var request = CreateRequest(
                session.Model,
                session.Tokenizer,
                address,
                CreateAcceptingBroker(),
                trainingRunId: trainingRunId,
                dpoStepCount: 1);

            var result = await CodeDocumentationServerOwnedDpoTrainingExecutor
                .ExecuteAsync(request)
                .WaitAsync(TimeSpan.FromSeconds(30));

            Assert.True(result.IsSuccess,
                $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.NotNull(result.Value);
            Assert.True(exactMaterialObserved);
            var afterSha256 = Sha256OfModel(session.Model);
            Assert.NotEqual(beforeSha256, afterSha256);
            Assert.Equal(1, result.Value!.ProcessedPairCount);
            Assert.Equal(1, result.Value.DpoStepCount);
            Assert.Equal(
                Path.Combine(
                    address.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(1)),
                result.Value.PolicyCheckpointPath);
            Assert.Equal(
                Path.Combine(
                    address.AttemptRoot,
                    DpoTrainer.BuildMetricsFileName(1)),
                result.Value.MetricsPath);
            Assert.True(new FileInfo(result.Value.PolicyCheckpointPath).Length > 0);
            Assert.True(new FileInfo(result.Value.MetricsPath).Length > 0);
            Assert.Equal(
                "policy/code-documentation-server-owned-dpo-test",
                result.Value.LegalTeacherPolicyIdentityRef);
        }
        finally
        {
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting = null;
            // WHY: repository-owned layout validation needs repo sentinels, but
            // this fixture repository is intentionally temporary and outside
            // the live checkout, so cleanup cannot erase product source.
            DeleteDirectoryIfExists(fakeRepositoryRoot);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ExistingAttemptRefusesBeforeSecondWeightMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var fakeRepositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:duplicate-attempt";
            var address = CreateAddress(fakeRepositoryRoot, trainingRunId);
            var firstSession = fixture.CreateFactory().Create("CODE").Value!;
            var first = await CodeDocumentationServerOwnedDpoTrainingExecutor
                .ExecuteAsync(
                    CreateRequest(
                        firstSession.Model,
                        firstSession.Tokenizer,
                        address,
                        CreateAcceptingBroker(),
                        trainingRunId,
                        dpoStepCount: 1))
                .WaitAsync(TimeSpan.FromSeconds(30));
            Assert.True(first.IsSuccess, $"{first.ErrorCode}: {first.ErrorMessage}");
            Assert.True(File.Exists(address.AttemptClaimPath));
            Assert.True(File.Exists(first.Value!.PolicyCheckpointPath));
            var claimSha256BeforeRetry = ComputeFileSha256(address.AttemptClaimPath);
            var policySha256BeforeRetry =
                ComputeFileSha256(first.Value.PolicyCheckpointPath);
            var metricsSha256BeforeRetry =
                ComputeFileSha256(first.Value.MetricsPath);

            var secondSession = fixture.CreateFactory().Create("CODE").Value!;
            var beforeSecondSha256 = Sha256OfModel(secondSession.Model);
            var trainerReached = false;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        trainerReached = true;
                    };

                var second = await CodeDocumentationServerOwnedDpoTrainingExecutor
                    .ExecuteAsync(
                        CreateRequest(
                            secondSession.Model,
                            secondSession.Tokenizer,
                            address,
                            CreateAcceptingBroker(),
                            trainingRunId,
                            dpoStepCount: 1))
                    .WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(second.IsSuccess);
                Assert.Equal(
                    CodeDocumentationServerOwnedDpoTrainingExecutor
                        .TrainingAttemptAlreadyExistsErrorCode,
                    second.ErrorCode);
                Assert.False(trainerReached);
                Assert.Equal(beforeSecondSha256, Sha256OfModel(secondSession.Model));
                Assert.Equal(
                    claimSha256BeforeRetry,
                    ComputeFileSha256(address.AttemptClaimPath));
                Assert.Equal(
                    policySha256BeforeRetry,
                    ComputeFileSha256(first.Value.PolicyCheckpointPath));
                Assert.Equal(
                    metricsSha256BeforeRetry,
                    ComputeFileSha256(first.Value.MetricsPath));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(fakeRepositoryRoot);
        }
    }

    [Fact]
    public async Task ExecuteAsync_RefusingBrokerFailsBeforeWeightOrFileMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var fakeRepositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:refusing-broker";
            var address = CreateAddress(fakeRepositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var beforeSha256 = Sha256OfModel(session.Model);
            var broker = new RefusingCodeDocumentationLeaseBroker();
            var request = CreateRequest(
                session.Model,
                session.Tokenizer,
                address,
                broker,
                trainingRunId: trainingRunId,
                dpoStepCount: 1);

            var result = await CodeDocumentationServerOwnedDpoTrainingExecutor
                .ExecuteAsync(request)
                .WaitAsync(TimeSpan.FromSeconds(10));

            Assert.False(result.IsSuccess);
            Assert.Equal(RefusingCodeDocumentationLeaseBroker.ErrorCode,
                result.ErrorCode);
            Assert.Equal(1, broker.CodeDocumentationCallCount);
            Assert.NotNull(broker.LastTrainingContent);
            Assert.Equal(beforeSha256, Sha256OfModel(session.Model));
            Assert.False(Directory.Exists(address.AttemptRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(fakeRepositoryRoot);
        }
    }

    [Fact]
    public async Task ExecuteAsync_MutableCallerListCannotChangeExactLeaseOrTrainerSnapshotAfterProjection()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var fakeRepositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:mutable-caller-list";
            var address = CreateAddress(fakeRepositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var original = Pair();
            var replacement = AuthorlessReplacementPair();
            var mutablePairs = new List<ContinuousLearning.DpoPair> { original };
            var originalContent = DpoPairTrainingContentAuthority
                .Project([original]).Value!;
            var replacementContent = DpoPairTrainingContentAuthority
                .Project([replacement]).Value!;
            var broker = new MutatingCodeDocumentationLeaseBroker(
                mutablePairs,
                replacement);
            var request = CreateRequest(
                session.Model,
                session.Tokenizer,
                address,
                broker,
                trainingRunId: trainingRunId,
                dpoStepCount: 1,
                pairs: mutablePairs);

            var result = await CodeDocumentationServerOwnedDpoTrainingExecutor
                .ExecuteAsync(request)
                .WaitAsync(TimeSpan.FromSeconds(30));

            Assert.True(result.IsSuccess,
                $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.Same(replacement, mutablePairs[0]);
            Assert.Equal(1, broker.CodeDocumentationCallCount);
            Assert.NotNull(broker.LastTrainingContent);
            Assert.Equal(
                originalContent.Sha256,
                broker.LastTrainingContent!.Sha256);
            Assert.NotEqual(
                replacementContent.Sha256,
                broker.LastTrainingContent.Sha256);
            Assert.Equal(
                Path.Combine(
                    address.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(1)),
                result.Value!.PolicyCheckpointPath);
            Assert.True(new FileInfo(result.Value.PolicyCheckpointPath).Length > 0);
        }
        finally
        {
            DeleteDirectoryIfExists(fakeRepositoryRoot);
        }
    }

    [Fact]
    public async Task ExecuteAsync_SingleDpoStepWritesTrainerFilesUnderAttemptNotNumericCheckpoint()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var fakeRepositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:single-dpo-step-attempt";
            var address = CreateAddress(fakeRepositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var beforeSha256 = Sha256OfModel(session.Model);
            var broker = CreateAcceptingBroker();
            var request = CreateRequest(
                session.Model,
                session.Tokenizer,
                address,
                broker,
                trainingRunId: trainingRunId,
                dpoStepCount: 1);

            var result = await CodeDocumentationServerOwnedDpoTrainingExecutor
                .ExecuteAsync(request)
                .WaitAsync(TimeSpan.FromSeconds(30));

            Assert.True(result.IsSuccess,
                $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.NotNull(result.Value);
            Assert.Contains(
                "/attempts/",
                address.AttemptRootRelativePath,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "/checkpoints/",
                address.AttemptRootRelativePath,
                StringComparison.Ordinal);
            Assert.NotEqual(beforeSha256, Sha256OfModel(session.Model));
            Assert.Equal(1, result.Value!.ProcessedPairCount);
            Assert.Equal(1, result.Value.DpoStepCount);
            Assert.Equal(
                Path.Combine(
                    address.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(1)),
                result.Value.PolicyCheckpointPath);
            Assert.Equal(
                Path.Combine(
                    address.AttemptRoot,
                    DpoTrainer.BuildMetricsFileName(1)),
                result.Value.MetricsPath);
            Assert.DoesNotContain(
                DpoTrainer.BuildPolicyCheckpointFileName(2),
                result.Value.PolicyCheckpointPath,
                StringComparison.Ordinal);
            Assert.True(new FileInfo(result.Value.PolicyCheckpointPath).Length > 0);
            Assert.True(new FileInfo(result.Value.MetricsPath).Length > 0);
        }
        finally
        {
            DeleteDirectoryIfExists(fakeRepositoryRoot);
        }
    }

    [Fact]
    public async Task ExecuteAsync_SamePolicyAndReferenceObjectFailsBeforeBrokerWeightOrFileMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var fakeRepositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:same-policy-reference";
            var address = CreateAddress(fakeRepositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var beforeSha256 = Sha256OfModel(session.Model);
            var broker = new RefusingCodeDocumentationLeaseBroker();
            var request = CreateRequest(
                session.Model,
                session.Tokenizer,
                address,
                broker,
                trainingRunId: trainingRunId,
                dpoStepCount: 1,
                referenceModel: session.Model);

            var result = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                .ExecuteAsync(request)
                .WaitAsync(TimeSpan.FromSeconds(10));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority
                    .TrainingContentInvalidErrorCode,
                result.ErrorCode);
            Assert.Equal(0, broker.CodeDocumentationCallCount);
            Assert.Equal(beforeSha256, Sha256OfModel(session.Model));
            Assert.False(Directory.Exists(address.AttemptRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(fakeRepositoryRoot);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ExecuteAsync_NonNullEmptyOptimizerStateFailsClosed(int step)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var fakeRepositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:empty-optimizer-state-" + step;
            var address = CreateAddress(fakeRepositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var beforeSha256 = Sha256OfModel(session.Model);
            var trainerReached = false;
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
            var invalidState = new AdamWPortableState(
                AdamWPortableState.CurrentFormatVersion,
                step,
                Array.Empty<AdamWPortableParameterState>());

            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        trainerReached = true;
                    };

                var result = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                    .ExecuteAsync(
                        CreateRequest(
                            session.Model,
                            session.Tokenizer,
                            address,
                            CreateAcceptingBroker(),
                            trainingRunId,
                            dpoStepCount: 1) with
                        {
                            OptimizerStateToRestore = invalidState,
                            OptimizerStateColdStartAuthorization = null
                        })
                    .WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(result.IsSuccess);
                Assert.Equal(
                    RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode,
                    result.ErrorCode);
                Assert.False(trainerReached);
                Assert.Equal(beforeSha256, Sha256OfModel(session.Model));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(fakeRepositoryRoot);
        }
    }

    [Fact]
    public async Task ExecuteAsync_PreExistingOptimizerStateSurvivesFailedCreateNewPublication()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var fakeRepositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:pre-existing-optimizer-state";
            var address = CreateAddress(fakeRepositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var statePath = Path.Combine(
                address.AttemptRoot,
                RepositoryOwnedDpoOptimizerStateFile.BuildFileName(1));
            var preexistingStateBytes = new byte[] { 0x2a };
            var priorObserver =
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting;

            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                            statePath,
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.None,
                            FileOptions.WriteThrough);
                        stream.Write(
                            preexistingStateBytes,
                            0,
                            preexistingStateBytes.Length);
                        stream.Flush(flushToDisk: true);
                    };

                var result = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                    .ExecuteAsync(
                        CreateRequest(
                            session.Model,
                            session.Tokenizer,
                            address,
                            CreateAcceptingBroker(),
                            trainingRunId,
                            dpoStepCount: 1))
                    .WaitAsync(TimeSpan.FromSeconds(20));

                Assert.False(result.IsSuccess);
                Assert.Equal("DPO_EXECUTION_FAILED", result.ErrorCode);
                Assert.False(File.Exists(Path.Combine(
                    address.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(1))));
                Assert.False(File.Exists(Path.Combine(
                    address.AttemptRoot,
                    DpoTrainer.BuildMetricsFileName(1))));
                Assert.True(File.Exists(statePath));
                Assert.Equal(
                    Convert.ToHexString(SHA256.HashData(preexistingStateBytes))
                        .ToLowerInvariant(),
                    ComputeFileSha256(statePath));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(fakeRepositoryRoot);
        }
    }

    [Fact]
    public async Task RejectsMissingRandomStateBeforeMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:missing-random-state";
            var address = CreateAddress(repositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var beforeSha256 = Sha256OfModel(session.Model);
            var reached = false;
            var request = CreateDropoutRequest(
                session.Model,
                session.Tokenizer,
                address,
                trainingRunId) with
            {
                RandomStateColdStartAuthorization = null
            };
            var priorObserver = StableDpoTrainingExecutor
                .LogicalRequestObserverForTesting;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    _ => reached = true;
                var result = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                    .ExecuteAsync(request)
                    .WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "DPO_RANDOM_STATE_COLD_START_AUTHORIZATION_REQUIRED",
                    result.ErrorCode);
                Assert.False(reached);
                Assert.Equal(beforeSha256, Sha256OfModel(session.Model));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("hash")]
    [InlineData("step")]
    [InlineData("topology")]
    public async Task RejectsRandomStateHashLengthStepAndTopologyMismatch(
        string mismatch)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:random-mismatch-" + mismatch;
            var address = CreateAddress(repositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var beforeSha256 = Sha256OfModel(session.Model);
            var authority = new LoraPairRandomSeedAuthority(
                71UL,
                "policy.step",
                "reference.step");
            var loraConfig = DropoutConfig();
            var snapshot = CreateRandomSnapshot(
                session.Model.Config,
                loraConfig,
                authority,
                mismatch);
            var invalidOptimizer = new AdamWPortableState(
                AdamWPortableState.CurrentFormatVersion,
                1,
                [new AdamWPortableParameterState("not-a-policy-key", [1], [0f], [0f])]);
            var request = CreateDropoutRequest(
                session.Model,
                session.Tokenizer,
                address,
                trainingRunId) with
            {
                OptimizerStateToRestore = invalidOptimizer,
                OptimizerStateColdStartAuthorization = null,
                RandomStateToRestore = snapshot,
                RandomStateColdStartAuthorization = null
            };

            var result = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                .ExecuteAsync(request)
                .WaitAsync(TimeSpan.FromSeconds(10));

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_RANDOM_STATE_IDENTITY_MISMATCH", result.ErrorCode);
            Assert.Equal(beforeSha256, Sha256OfModel(session.Model));
            Assert.False(File.Exists(Path.Combine(
                address.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1))));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task WritesOptimizerAndRandomStateWithSameGlobalStep()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:random-and-optimizer-step";
            var address = CreateAddress(repositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var result = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                .ExecuteAsync(
                    CreateDropoutRequest(
                        session.Model,
                        session.Tokenizer,
                        address,
                        trainingRunId))
                .WaitAsync(TimeSpan.FromSeconds(10));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.NotNull(result.Value);
            Assert.Equal(
                result.Value!.OptimizerStateStep,
                result.Value.RandomStateStep);
            Assert.True(result.Value.RandomStateRequired);
            Assert.Equal(1, result.Value.OptimizerStateStep);
            Assert.True(File.Exists(result.Value.OptimizerStatePath));
            Assert.True(File.Exists(result.Value.RandomStatePath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task DeletesOnlyTransactionOwnedRandomStateAfterCallbackFailure()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:random-callback-failure";
            var address = CreateAddress(repositoryRoot, trainingRunId);
            var session = fixture.CreateFactory().Create("CODE").Value!;
            var sentinelPath = Path.Combine(address.AttemptRoot, "sentinel.bin");
            var priorObserver = StableDpoTrainingExecutor
                .LogicalRequestObserverForTesting;
            var priorFailure =
                CodeDocumentationServerOwnedDpoTrainingExecutor
                    .ForcePostPublicationValidationFailureForTesting;
            try
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    logicalRequest =>
                    {
                        priorObserver?.Invoke(logicalRequest);
                        Directory.CreateDirectory(address.AttemptRoot);
                        File.WriteAllBytes(sentinelPath, [0x55, 0x66]);
                    };
                CodeDocumentationServerOwnedDpoTrainingExecutor
                        .ForcePostPublicationValidationFailureForTesting =
                    _ => true;

                var result = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                    .ExecuteAsync(
                        CreateDropoutRequest(
                            session.Model,
                            session.Tokenizer,
                            address,
                            trainingRunId))
                    .WaitAsync(TimeSpan.FromSeconds(10));

                Assert.False(result.IsSuccess);
                Assert.Equal(
                    "CODE_DOCUMENTATION_DPO_POST_PUBLICATION_VALIDATION_FAILED",
                    result.ErrorCode);
                Assert.True(File.Exists(sentinelPath));
                Assert.False(File.Exists(Path.Combine(
                    address.AttemptRoot,
                    RepositoryOwnedDpoRandomStateFile.BuildFileName(1))));
            }
            finally
            {
                StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                    priorObserver;
                CodeDocumentationServerOwnedDpoTrainingExecutor
                        .ForcePostPublicationValidationFailureForTesting =
                    priorFailure;
            }
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void CreateModelTrainingAttemptAddress_HashesRunIdAndRejectsRawRunIdPath()
    {
        var fakeRepositoryRoot = CreateFakeRepositoryRoot();
        try
        {
            var trainingRunId = "code-doc-run:hash.Raw_ok-1";
            var address = CreateAddress(fakeRepositoryRoot, trainingRunId);
            var expectedRunIdSha256 = Convert.ToHexString(
                    SHA256.HashData(
                        System.Text.Encoding.UTF8.GetBytes(trainingRunId)))
                .ToLowerInvariant();

            Assert.Equal(trainingRunId, address.TrainingRunId);
            Assert.Equal(expectedRunIdSha256, address.TrainingRunIdSha256);
            Assert.EndsWith(
                "training-artifacts/models/code-documentation/server-owned-dpo-training/tiny-model/v0001/attempts/" +
                expectedRunIdSha256,
                address.AttemptRootRelativePath,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                trainingRunId,
                address.AttemptRootRelativePath,
                StringComparison.Ordinal);
            Assert.False(RepositoryOwnedTrainingArtifactLayout
                .TryParseModelCheckpointStepSlug(
                    expectedRunIdSha256,
                    out _));
            Assert.True(RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptAddress(address));

            var invalid = RepositoryOwnedTrainingArtifactLayout
                .CreateModelTrainingAttemptAddress(
                    fakeRepositoryRoot,
                    "code-documentation",
                    "server-owned-dpo-training",
                    "tiny-model",
                    "v0001",
                    "raw/run-id");
            Assert.False(invalid.IsSuccess);
        }
        finally
        {
            DeleteDirectoryIfExists(fakeRepositoryRoot);
        }
    }

    private static CodeDocumentationServerOwnedDpoTrainingRequest CreateDropoutRequest(
        LlmModel policy,
        Tokenizer.BpeTokenizer tokenizer,
        RepositoryOwnedModelTrainingAttemptAddress address,
        string trainingRunId)
    {
        var authority = new LoraPairRandomSeedAuthority(
            71UL,
            "policy.step",
            "reference.step");
        return CreateRequest(
                policy,
                tokenizer,
                address,
                CreateAcceptingBroker(),
                trainingRunId,
                dpoStepCount: 1) with
        {
            LoraConfig = DropoutConfig(),
            AdapterSeedAuthority = authority,
            SeededTopologyPreparedByRepositoryStart = false,
            RandomStateColdStartAuthorization =
                new DpoRandomStateColdStartAuthorization(
                    "executor-test",
                    "bounded dropout cold start",
                    authority.RootInitializationSeed),
        };
    }

    private static LoraConfig DropoutConfig()
        => new()
        {
            Rank = 2,
            Alpha = 4,
            Dropout = 0.25f,
            TargetModules = ["q_proj"]
        };

    private static LlmModelRandomStateSnapshot CreateRandomSnapshot(
        ModelConfig modelConfig,
        LoraConfig loraConfig,
        LoraPairRandomSeedAuthority authority,
        string mismatch)
    {
        var topologyHash = LoraRandomStateIdentity.ComputeTopologySha256(
            modelConfig,
            loraConfig,
            authority);
        var loraHash = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
            loraConfig,
            authority);
        var seeds = LoraAdapterRandomSeedDerivation.Derive(
            authority.RootInitializationSeed,
            0,
            "q_proj",
            authority.PolicyDropoutStreamDomain);
        return new LlmModelRandomStateSnapshot(
            LlmModelRandomStateSnapshot.CurrentFormatVersion,
            LlmModelRandomStateSnapshot.CurrentAlgorithm,
            [new LlmModelRandomStateEntry(
                "lora.block_0000.q_proj",
                authority.PolicyDropoutStreamDomain,
                seeds.InitializationSeed,
                seeds.DropoutSeed,
                0UL)],
            mismatch == "topology" ? "bad" : topologyHash,
            mismatch == "hash" ? "bad" : loraHash,
            mismatch == "step" ? 2 : 1);
    }

    private static CodeDocumentationServerOwnedDpoTrainingRequest CreateRequest(
        LlmModel policy,
        Tokenizer.BpeTokenizer tokenizer,
        RepositoryOwnedModelTrainingAttemptAddress address,
        IServerOwnedDpoTrainingLeaseBroker broker,
        string trainingRunId,
        int dpoStepCount,
        IReadOnlyList<ContinuousLearning.DpoPair>? pairs = null,
        LlmModel? referenceModel = null,
        bool authorizeOptimizerColdStart = true)
        => new(
            pairs ?? [Pair()],
            policy,
            referenceModel ?? CloneModel(policy),
            tokenizer,
            new LoraConfig
            {
                Rank = 2,
                Alpha = 4,
                Dropout = 0,
                TargetModules = ["q_proj", "v_proj"]
            },
            address,
            broker,
            trainingRunId,
            dpoStepCount,
            OptimizerStateColdStartAuthorization:
                authorizeOptimizerColdStart
                    ? CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization
                        .Authorize("bounded executor test starts from an admitted root")
                    : null);

    private static ContinuousLearning.DpoPair Pair()
        => new(
            // A complete, synthetic C# law contrast fits the fixture's actual
            // context under separate prompt/response BPE. The exact path must
            // reject overflow instead of silently shortening judged material.
            "C# null?.Length?",
            "Returns null.",
            "Returns 0.",
            "CODE",
            DateTimeOffset.UtcNow)
        {
            // WHY: StableDpoTrainingExecutor intentionally rejects legacy
            // authorless pairs; this public doorway must prove it carries typed
            // side authorship into the existing DpoTrainer admission bridge.
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.DeterministicGenerator),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.DeterministicGenerator)
        };

    private static ContinuousLearning.DpoPair AuthorlessReplacementPair()
        => new(
            "Document a replacement caller-owned path.",
            "Replacement chosen must not reach trainer.",
            "Replacement rejected must not reach trainer.",
            "CODE",
            DateTimeOffset.UtcNow);

    private static IServerOwnedDpoTrainingLeaseBroker CreateAcceptingBroker()
        => (IServerOwnedDpoTrainingLeaseBroker)
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                _ => OperationResult<LegalTrainingTeacherAdmission>.Fail(
                    "CodeDocumentation executor must use exact content-bound leases.",
                    "UNBOUND_VALIDATOR_CALLED"),
                static () => true,
                concurrencyObserverForTesting: null,
                sealReplay: null,
                verifyReplay: null,
                sealExecutableManifest: null,
                verifyExecutableManifest: null,
                validateBoundBatch: (_, _) =>
                    OperationResult<LegalTrainingTeacherAdmission>.Ok(
                        new LegalTrainingTeacherAdmission(
                            [],
                            ExternalTeacherInvocationCount: 0,
                            PolicyIdentityRef:
                            "policy/code-documentation-server-owned-dpo-test",
                            EvidenceRefs: [])));

    private static RepositoryOwnedModelTrainingAttemptAddress CreateAddress(
        string fakeRepositoryRoot,
        string trainingRunId)
    {
        var result = RepositoryOwnedTrainingArtifactLayout
            .CreateModelTrainingAttemptAddress(
                fakeRepositoryRoot,
                "code-documentation",
                "server-owned-dpo-training",
                "tiny-model",
                "v0001",
                trainingRunId);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private static string CreateFakeRepositoryRoot()
    {
        var liveRoot = FindLiveRepositoryRoot();
        // Exercise real owner-only native storage. A Windows checkout mounted
        // by WSL can report 0777 regardless of the requested Unix creation mode.
        var fixtureParent = Path.Combine(Path.GetTempPath(), "code-doc-server-owned-dpo-tests");
        var root = Path.Combine(
            fixtureParent,
            $"code-doc-server-owned-dpo-repo-{Guid.NewGuid():N}");
        // A fresh direct child of this dedicated fixture directory is allowed;
        // it must remain outside every live repository source/model root.
        Assert.True(PathComparer.Equals(Path.GetDirectoryName(root), fixtureParent));
        Assert.True(IsInsideDirectory(root, fixtureParent));
        Assert.False(PathComparer.Equals(root, liveRoot));
        Assert.False(Directory.Exists(root));
        Directory.CreateDirectory(root);
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
            File.WriteAllText(Path.Combine(root, "LlmFromScratch.sln"), string.Empty);
            return root;
        }
        catch
        {
            DeleteDirectoryIfExists(root);
            throw;
        }
    }

    private static string FindLiveRepositoryRoot()
    {
        foreach (var start in new[]
                 {
                     Directory.GetCurrentDirectory(),
                     AppContext.BaseDirectory
                 })
        {
            var current = new DirectoryInfo(start);
            while (current is not null)
            {
                if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")) &&
                    Directory.Exists(Path.Combine(current.FullName, "14.ContinuousLearning")) &&
                    Directory.Exists(Path.Combine(current.FullName, "11.DPO")))
                {
                    return current.FullName;
                }
                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException(
            "Could not locate the live LlmFromScratch repository root.");
    }

    private static bool IsInsideDirectory(string candidate, string root)
    {
        var normalizedCandidate = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(candidate));
        var normalizedRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(root));
        if (PathComparer.Equals(normalizedCandidate, normalizedRoot))
            return true;
        return normalizedCandidate.StartsWith(
            normalizedRoot + Path.DirectorySeparatorChar,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
    }

    private static string Sha256OfModel(LlmModel model)
    {
        using var stream = new MemoryStream();
        model.SaveCheckpointToStream(stream);
        return Convert.ToHexString(SHA256.HashData(stream.ToArray()))
            .ToLowerInvariant();
    }

    private static string ComputeFileSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static LlmModel CloneModel(LlmModel source)
    {
        using var stream = new MemoryStream();
        source.SaveCheckpointToStream(stream);
        stream.Position = 0;
        var clone = new LlmModel(source.Config);
        clone.LoadCheckpoint(stream);
        return clone;
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        var canonical = Path.GetFullPath(path);
        var fixtureParent = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "code-doc-server-owned-dpo-tests"));
        if (!PathComparer.Equals(Path.GetDirectoryName(canonical), fixtureParent) ||
            !Path.GetFileName(canonical).StartsWith("code-doc-server-owned-dpo-repo-", StringComparison.Ordinal))
            throw new InvalidOperationException("Executor fixture cleanup escaped its dedicated root.");
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }

    private static StringComparer PathComparer { get; }
        = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
}

file sealed class RefusingCodeDocumentationLeaseBroker
    : IServerOwnedDpoTrainingLeaseBroker
{
    public const string ErrorCode = "REFUSING_CODE_DOCUMENTATION_LEASE";

    public int CodeDocumentationCallCount { get; private set; }
    public DpoTrainingContentBatch? LastTrainingContent { get; private set; }

    public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireTranslationTrainingLease(
            DpoTrainingContentBatch trainingContent,
            string trainingRunId)
        => OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
            "Translation DPO lease is not part of this test.",
            "UNEXPECTED_TRANSLATION_LEASE");

    public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireCodeDocumentationTrainingLease(
            DpoTrainingContentBatch trainingContent,
            string trainingRunId)
    {
        CodeDocumentationCallCount++;
        LastTrainingContent = trainingContent;
        return OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
            .Fail(
                "Synthetic broker refusal.",
                ErrorCode);
    }
}

file sealed class MutatingCodeDocumentationLeaseBroker
    : IServerOwnedDpoTrainingLeaseBroker
{
    private readonly IList<ContinuousLearning.DpoPair> _callerOwnedPairs;
    private readonly ContinuousLearning.DpoPair _replacement;
    private readonly IServerOwnedDpoTrainingLeaseBroker _inner;

    public MutatingCodeDocumentationLeaseBroker(
        IList<ContinuousLearning.DpoPair> callerOwnedPairs,
        ContinuousLearning.DpoPair replacement)
    {
        _callerOwnedPairs = callerOwnedPairs;
        _replacement = replacement;
        _inner = CreateInner();
    }

    public int CodeDocumentationCallCount { get; private set; }
    public DpoTrainingContentBatch? LastTrainingContent { get; private set; }

    public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireTranslationTrainingLease(
            DpoTrainingContentBatch trainingContent,
            string trainingRunId)
        => OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail(
            "Translation DPO lease is not part of this test.",
            "UNEXPECTED_TRANSLATION_LEASE");

    public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
        AcquireCodeDocumentationTrainingLease(
            DpoTrainingContentBatch trainingContent,
            string trainingRunId)
    {
        CodeDocumentationCallCount++;
        LastTrainingContent = trainingContent;

        // WHY: this simulates a hostile caller mutating its list after the
        // public executor already projected lease content. The executor must
        // continue with its captured snapshot, not reopen the caller list.
        _callerOwnedPairs[0] = _replacement;
        return _inner.AcquireCodeDocumentationTrainingLease(
            trainingContent,
            trainingRunId);
    }

    private static IServerOwnedDpoTrainingLeaseBroker CreateInner()
        => (IServerOwnedDpoTrainingLeaseBroker)
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                _ => OperationResult<LegalTrainingTeacherAdmission>.Fail(
                    "CodeDocumentation executor must use exact content-bound leases.",
                    "UNBOUND_VALIDATOR_CALLED"),
                static () => true,
                concurrencyObserverForTesting: null,
                sealReplay: null,
                verifyReplay: null,
                sealExecutableManifest: null,
                verifyExecutableManifest: null,
                validateBoundBatch: (_, _) =>
                    OperationResult<LegalTrainingTeacherAdmission>.Ok(
                        new LegalTrainingTeacherAdmission(
                            [],
                            ExternalTeacherInvocationCount: 0,
                            PolicyIdentityRef:
                            "policy/code-documentation-server-owned-dpo-test",
                            EvidenceRefs: [])));
}
