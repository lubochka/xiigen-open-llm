// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Buffers.Binary;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;
using DpoTrainerPairAdmission = DPO.Models.DpoTrainerPairAdmission;
using LoRA;
using TrainingLoop;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

public sealed class IncrementalTrainerDpoWiringTests
{
    private const int SessionMutationPermitLiveCheck = 2;
    private const int SingleExampleDpoDirectoryPermitLiveCheck = 6;
    private const int SingleExampleSupervisedPermitLiveCheck = 9;
    private const int SingleExampleFinalAdapterPermitLiveCheck = 10;
    private const string RepositoryOwnedDomainSlug = "bug";
    private const string RepositoryOwnedDesignPatternSlug =
        "incremental-trainer-dpo-wiring";
    private const string RepositoryOwnedModelIdSlug = "qwen3-5-9b";
    private const string RepositoryOwnedModelVersion = "v0001";
    private const string RepositoryOwnedContinuationChildModeEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_CHILD_MODE";
    private const string RepositoryOwnedContinuationChildScenarioEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_CHILD_SCENARIO";
    private const string RepositoryOwnedContinuationRepositoryRootEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_REPOSITORY_ROOT";
    private const string RepositoryOwnedContinuationFixtureRootEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_FIXTURE_ROOT";
    private const string RepositoryOwnedContinuationBaseCheckpointEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_BASE_CHECKPOINT";
    private const string RepositoryOwnedContinuationTokenizerEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_TOKENIZER";
    private const string RepositoryOwnedContinuationRetainedRootEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_RETAINED_ROOT";
    private const string RepositoryOwnedContinuationResultEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_RESULT";
    private const string RepositoryOwnedContinuationModelConfigEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_MODEL_CONFIG_JSON64";
    private const string RepositoryOwnedContinuationTrainingConfigEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_TRAINING_CONFIG_JSON64";
    private const string RepositoryOwnedContinuationLoraConfigEnvironmentName =
        "XIIGEN_INCREMENTAL_DPO_VSTEST_LORA_CONFIG_JSON64";
    private const string RepositoryOwnedContinuationAcceptedScenario =
        "accepted-continuation";
    private const string RepositoryOwnedContinuationMissingLoraScenario =
        "missing-explicit-lora-config";
    private static readonly string[] RepositoryOwnedContinuationChildEnvironmentNames =
    [
        RepositoryOwnedContinuationChildModeEnvironmentName,
        RepositoryOwnedContinuationChildScenarioEnvironmentName,
        RepositoryOwnedContinuationRepositoryRootEnvironmentName,
        RepositoryOwnedContinuationFixtureRootEnvironmentName,
        RepositoryOwnedContinuationBaseCheckpointEnvironmentName,
        RepositoryOwnedContinuationTokenizerEnvironmentName,
        RepositoryOwnedContinuationRetainedRootEnvironmentName,
        RepositoryOwnedContinuationResultEnvironmentName,
        RepositoryOwnedContinuationModelConfigEnvironmentName,
        RepositoryOwnedContinuationTrainingConfigEnvironmentName,
        RepositoryOwnedContinuationLoraConfigEnvironmentName,
    ];

    private static readonly IReadOnlyDictionary<ushort, OpCode> IlOpCodes =
        typeof(OpCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(static field => field.FieldType == typeof(OpCode))
            .Select(static field => (OpCode)field.GetValue(null)!)
            .GroupBy(static opCode => unchecked((ushort)opCode.Value))
            .ToDictionary(static group => group.Key, static group => group.First());

    [Fact]
    public async Task NonAdmittedPairs_FailBeforeSessionExecutorOrFilesystemMutation()
    {
        var cases = new (TrainingTeacherProvenance? Provenance, string ErrorCode)[]
        {
            (null, LegalTrainingTeacherAdmissionAuthority.ProvenanceRequiredErrorCode),
            (QwenProvenance("closed-request") with
            {
                ModelId = "gpt-5",
                ClosedProprietaryModel = true,
            }, LegalTrainingTeacherAdmissionAuthority.ClosedProprietaryDeniedErrorCode),
            (QwenProvenance("unlisted-request") with
            {
                ModelId = "qwen-unlisted",
            }, LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode),
            (QwenProvenance("manifest-mismatch") with
            {
                ModelVersion = string.Concat('e', new string('d', 63)),
            }, LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode),
            (QwenProvenance("layer-mismatch") with
            {
                ModelArtifactDigest = string.Concat('b', new string('c', 63)),
            }, LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode),
        };

        foreach (var (provenance, expectedErrorCode) in cases)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                $"continuous-learning-dpo-rejection-{Guid.NewGuid():N}");
            var factory = new CountingDpoSessionFactory();
            var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
            var trainer = new IncrementalTrainer(
                new NullSessionStorageCL(),
                factory,
                CreateAuthority(),
                executor,
                root);

            var result = await trainer.TrainAsync(
                "BUG",
                [Example()],
                [Pair(provenance)]);

            Assert.False(result.IsSuccess);
            Assert.Equal(expectedErrorCode, result.ErrorCode);
            Assert.Equal(0, factory.CreateCount);
            Assert.Equal(0, executor.CallCount);
            Assert.False(Directory.Exists(root));
            Assert.DoesNotContain(new string('d', 64), result.ErrorMessage);
            Assert.DoesNotContain(new string('c', 64), result.ErrorMessage);
            if (provenance is not null)
            {
                Assert.DoesNotContain(provenance.ModelVersion, result.ErrorMessage);
                if (provenance.ModelArtifactDigest is not null)
                {
                    Assert.DoesNotContain(
                        provenance.ModelArtifactDigest,
                        result.ErrorMessage);
                }
            }
        }
    }

    [Fact]
    public async Task PairAdmissionPrecedesTransactionRequirementWithoutMutation()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-dpo-order-{Guid.NewGuid():N}");
        var factory = new CountingDpoSessionFactory();
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            factory,
            CreateAuthority(),
            executor,
            root);
        var malformed = new DpoPair(
            string.Empty,
            "chosen",
            "rejected",
            "BUG",
            DateTimeOffset.UnixEpoch)
        {
            GovernanceDecision = Governance(),
        };

        var malformedResult = await trainer.TrainAsync(
            "BUG",
            [Example()],
            [malformed]);
        var admittedWithoutTransaction = await trainer.TrainAsync(
            "BUG",
            [Example()],
            [Pair(QwenProvenance("transaction-required"))]);

        Assert.Equal("INVALID_DPO_PAIR", malformedResult.ErrorCode);
        Assert.Equal(
            "DPO_TRANSACTION_REQUIRED",
            admittedWithoutTransaction.ErrorCode);
        Assert.Equal(0, factory.CreateCount);
        Assert.Equal(0, executor.CallCount);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public async Task InvalidGovernancePrecedesUnavailableCapability_ButValidPairRequiresCapability()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-capability-order-{Guid.NewGuid():N}");
        var factory = new CountingDpoSessionFactory();
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        ServerOwnedLegalTrainingTeacherAdmissionAuthority?[] capabilities =
        [
            null,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Unavailable(),
        ];
        foreach (var capability in capabilities)
        {
            var trainer = new IncrementalTrainer(
                new NullSessionStorageCL(),
                factory,
                capability,
                executor,
                root);

            var governanceRejected = await trainer.TrainAsync(
                "BUG",
                [Example()],
                [Pair(QwenProvenance("governance-first"), governance: null)]);
            var capabilityRejected = await trainer.TrainAsync(
                "BUG",
                [Example()],
                [Pair(QwenProvenance("capability-required"))]);

            Assert.Equal(
                TrainingDataGovernanceAdmission.RequiredErrorCode,
                governanceRejected.ErrorCode);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
                capabilityRejected.ErrorCode);
        }
        Assert.Equal(0, factory.CreateCount);
        Assert.Equal(0, executor.CallCount);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public async Task PersistedLocalQwenOllamaPair_AfterRestart_ReachesDpoSeamExactlyOnce()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var pairDirectory = Path.Combine(fixture.RootDirectory, "dpo-pairs");
        var provenance = QwenProvenance("local-qwen-ollama-request");
        var feedback = new FeedbackEvent(
            "fix the bug",
            "bad response",
            "good response",
            FeedbackSignal.Edited,
            "BUG")
        {
            TeacherProvenance = provenance,
            GovernanceDecision = Governance(),
        };
        var firstRouter = new FeedbackRouter(
            new NullSessionStorageCL(),
            new AcceptingDpoCache(),
            pairDirectory);
        var persisted = firstRouter.Route(feedback);
        Assert.True(persisted.IsSuccess, persisted.ErrorMessage);

        var restartedRouter = new FeedbackRouter(
            new NullSessionStorageCL(),
            new AcceptingDpoCache(),
            pairDirectory);
        var loaded = restartedRouter.GetDpoPairs();
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var loadedPair = Assert.Single(loaded.Value!);
        var loadedProvenance = Assert.IsType<TrainingTeacherProvenance>(
            loadedPair.TeacherProvenance);
        Assert.Equal(provenance.SourceKind, loadedProvenance.SourceKind);
        Assert.Equal(provenance.Family, loadedProvenance.Family);
        Assert.Equal(provenance.AccessMode, loadedProvenance.AccessMode);
        Assert.Equal(provenance.HostingProvider, loadedProvenance.HostingProvider);
        Assert.Equal(provenance.ModelId, loadedProvenance.ModelId);
        Assert.Equal(provenance.ModelVersion, loadedProvenance.ModelVersion);
        Assert.Equal(provenance.ModelLicense, loadedProvenance.ModelLicense);
        Assert.Equal(
            provenance.ProviderTermsChecked,
            loadedProvenance.ProviderTermsChecked);
        Assert.Equal(
            provenance.ProviderTermsAllowTraining,
            loadedProvenance.ProviderTermsAllowTraining);
        Assert.Equal(
            provenance.ClosedProprietaryModel,
            loadedProvenance.ClosedProprietaryModel);
        Assert.Equal(
            provenance.TeacherOutputUsedForTraining,
            loadedProvenance.TeacherOutputUsedForTraining);
        Assert.Equal(provenance.RequestId, loadedProvenance.RequestId);
        Assert.Equal(provenance.InvocationCount, loadedProvenance.InvocationCount);
        Assert.Equal(
            provenance.ModelArtifactDigestKind,
            loadedProvenance.ModelArtifactDigestKind);
        Assert.Equal(
            provenance.ModelArtifactDigest,
            loadedProvenance.ModelArtifactDigest);
        Assert.True(TrainingSourceProvenance.IsCanonicalSha256(
            loadedProvenance.TrainingContentSha256));
        var projected = DpoPairTrainingContentAuthority.Project([loadedPair]);
        Assert.True(projected.IsSuccess, projected.ErrorMessage);
        Assert.Equal(
            projected.Value!.Rows[0].Sha256,
            loadedProvenance.TrainingContentSha256);
        Assert.Equal(Governance(), loadedPair.GovernanceDecision);

        var coordinator = (IDpoPairTransactionCoordinator)restartedRouter;
        var prepared = coordinator.PrepareDpoTransaction(["BUG"]);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            prepared.Value);
        var reserved = transaction.Read();
        Assert.True(reserved.IsSuccess, reserved.ErrorMessage);
        var exactPairs = reserved.Value!.Pairs
            .Select(static value => value.Pair)
            .ToArray();
        var exactPair = Assert.Single(exactPairs);

        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Failure);
        var factory = new RecordingDpoSessionFactory(fixture.CreateFactory());
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            factory,
            CreateAuthority(),
            executor,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "fix bug", "return fixed")],
                exactPairs,
                transaction,
                CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("SYNTHETIC_DPO_FAILURE", result.ErrorCode);
        Assert.Equal(1, factory.CreateCount);
        Assert.Equal(1, executor.CallCount);
        Assert.Equal("BUG", executor.LastRequest!.Domain);
        Assert.Same(exactPair, Assert.Single(executor.LastRequest.Pairs));
        Assert.Equal(
            CreateRawAuthority().PolicyIdentityRef,
            executor.LastRequest.LegalTeacherAdmissionLease.Admission
                .PolicyIdentityRef);
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));

        var failedTransaction = transaction.Read();
        Assert.True(failedTransaction.IsSuccess, failedTransaction.ErrorMessage);
        Assert.True(failedTransaction.Value!.MutationStarted);
        Assert.Equal(
            FileDpoPairStore.DpoTransactionDomainState.MutationStarted,
            Assert.Single(failedTransaction.Value.Domains).State);
        var recovery = coordinator.PrepareDpoTransaction(["BUG"]);
        Assert.True(recovery.IsSuccess, recovery.ErrorMessage);
        Assert.Equal(
            transaction.TransactionId,
            Assert.IsType<FileDpoPairStore.TransactionHandle>(recovery.Value)
                .TransactionId);
    }

    [Fact]
    public async Task DurableDpoReference_IsIndependentFrozenExactPolicySnapshot()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "exact-reference-request");
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Failure);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            CreateAuthority(),
            executor,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "fix bug", "return fixed")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.Equal("SYNTHETIC_DPO_FAILURE", result.ErrorCode);
        var request = Assert.IsType<DpoTrainingExecutionRequest>(
            executor.LastRequest);
        Assert.NotSame(request.PolicyModel, request.ReferenceModel);
        var policy = request.PolicyModel.GetCheckpointParameterEntries();
        var reference = request.ReferenceModel.GetCheckpointParameterEntries();
        Assert.Equal(policy.Count, reference.Count);
        for (var index = 0; index < policy.Count; index++)
        {
            Assert.Equal(policy[index].Key, reference[index].Key);
            Assert.Equal(
                policy[index].Parameter.Value.Shape.Dimensions,
                reference[index].Parameter.Value.Shape.Dimensions);
            Assert.Equal(
                policy[index].Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits),
                reference[index].Parameter.Value.Data
                    .Select(BitConverter.SingleToInt32Bits));
            Assert.False(reference[index].Parameter.Value.RequiresGrad);
        }
    }

    [Fact]
    public async Task DurableDpoReferenceMutation_FailsClosedBeforePublication()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "mutated-reference-request");
        RecordingDpoExecutor? executor = null;
        executor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success,
            afterMutation: () =>
                executor!.LastRequest!.ReferenceModel
                    .GetCheckpointParameterEntries()[0]
                    .Parameter.Value.Data[0] += 1f);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            CreateAuthority(),
            executor,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "fix bug", "return fixed")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_REFERENCE_MODEL_MUTATION_DETECTED", result.ErrorCode);
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.lora")));
    }

    [Fact]
    public async Task EmptyPairs_PreserveSupervisedPathWithoutDpoExecution()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            CreateAuthority(),
            executor,
            fixture.AdapterDirectory);

        var result = await trainer.TrainAsync(
            "BUG",
            [fixture.Example("BUG", "prompt", "response")],
            []);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(0, executor.CallCount);
    }

    [Fact]
    public async Task OversizedAndPreCancelledBatches_NeverCreateSessionOrInvokeDpo()
    {
        var oversizedFactory = new CountingDpoSessionFactory();
        var oversizedExecutor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var oversizedTrainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            oversizedFactory,
            CreateAuthority(),
            oversizedExecutor);
        var oversizedPairs = Enumerable.Range(
                0,
                LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1)
            .Select(index => Pair(QwenProvenance($"request-{index}")))
            .ToArray();

        var oversized = await oversizedTrainer.TrainAsync(
            "BUG",
            [Example()],
            oversizedPairs);

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.BatchLimitExceededErrorCode,
            oversized.ErrorCode);
        Assert.Equal(0, oversizedFactory.CreateCount);
        Assert.Equal(0, oversizedExecutor.CallCount);

        var cancelledFactory = new CountingDpoSessionFactory();
        var cancelledExecutor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var cancelledTrainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            cancelledFactory,
            CreateAuthority(),
            cancelledExecutor);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            cancelledTrainer.TrainAsync(
                "BUG",
                [Example()],
                [Pair(QwenProvenance("cancelled-request"))],
                cancellation.Token));
        Assert.Equal(0, cancelledFactory.CreateCount);
        Assert.Equal(0, cancelledExecutor.CallCount);
    }

    [Theory]
    [InlineData(DpoExecutorBehavior.Failure, "SYNTHETIC_DPO_FAILURE")]
    [InlineData(DpoExecutorBehavior.MissingArtifact, "DPO_EXECUTION_ARTIFACT_INCOMPLETE")]
    public async Task DpoFailureOrMissingArtifact_CannotPublishFalseSuccess(
        DpoExecutorBehavior behavior,
        string expectedErrorCode)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            $"failure-request-{behavior}",
            uniqueStoreSuffix: $"failure-{behavior}");
        var executor = new RecordingDpoExecutor(behavior);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            CreateAuthority(),
            executor,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Equal(1, executor.CallCount);
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));
        Assert.False(transaction.ReleaseIfUnmutated().Value);
        Assert.Equal(
            FileDpoPairStore.DpoTransactionDomainState.MutationStarted,
            Assert.Single(transaction.Read().Value!.Domains).State);
    }

    [Fact]
    public void PublicTrainContract_HasNoPerCallPolicyInjection_AndProductionCompositionOwnsStableAdapter()
    {
        var trainMethod = typeof(IncrementalTrainer).GetMethod(
            nameof(IncrementalTrainer.TrainAsync),
            BindingFlags.Instance | BindingFlags.Public)!;
        Assert.DoesNotContain(
            trainMethod.GetParameters(),
            parameter => parameter.ParameterType == typeof(LegalTrainingTeacherAdmissionAuthority)
                         || parameter.ParameterType == typeof(IDpoTrainingExecutor)
                         || parameter.Name!.Contains("allowlist", StringComparison.OrdinalIgnoreCase)
                         || parameter.Name.Contains("salt", StringComparison.OrdinalIgnoreCase)
                         || parameter.Name.Contains("policy", StringComparison.OrdinalIgnoreCase));

        var configuration = CreateTp1Configuration();
        var composition = ContinuousLearningProductionComposition.CreateTp1Trainer(
            CreateAuthority(),
            new NullSessionStorageCL(),
            new CountingDpoSessionFactory(),
            configuration);
        Assert.True(composition.IsSuccess, composition.ErrorMessage);
        Assert.Equal(
            StableDpoTrainingExecutor.DefaultDpoStepCount,
            configuration.DpoMaxStepCount);
        var production = composition.Value!;
        var executorField = typeof(IncrementalTrainer).GetField(
            "_dpoTrainingExecutor",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.IsType<StableDpoTrainingExecutor>(executorField.GetValue(production));
        Assert.DoesNotContain(
            typeof(IncrementalTrainer).GetConstructors(BindingFlags.Instance | BindingFlags.Public),
            constructor => constructor.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(LegalTrainingTeacherAdmissionAuthority)));
        Assert.DoesNotContain(
            typeof(IncrementalTrainer).GetConstructors(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            constructor => constructor.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(LegalTrainingTeacherAdmissionAuthority)));
        Assert.DoesNotContain(
            typeof(IncrementalTrainer).GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            field => field.FieldType == typeof(LegalTrainingTeacherAdmissionAuthority));
        Assert.Contains(
            typeof(IncrementalTrainer).GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic),
            field => field.FieldType ==
                typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority));
    }

    [Fact]
    public void W154_ProductionCompositionNamesServerOwnedDpoAuthoritySeparatelyFromTeacherPolicyBudgetAuthority()
    {
        var rawAuthorityFactories = typeof(ContinuousLearningTp1DpoConfiguration)
            .GetMethods(
                BindingFlags.Instance |
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic)
            .Where(static method =>
                method.ReturnType == typeof(
                    OperationResult<LegalTrainingTeacherAdmissionAuthority>))
            .ToArray();

        Assert.Contains(
            rawAuthorityFactories,
            method => method.Name == "CreateTeacherPolicyBudgetAuthority");
        Assert.DoesNotContain(
            rawAuthorityFactories,
            method => method.Name == "CreateAuthority");

        var productionComposition = File.ReadAllText(FindRepositoryFile(
            "14.ContinuousLearning",
            "Training",
            "ContinuousLearningProductionComposition.cs"));
        var localQwenClient = File.ReadAllText(FindRepositoryFile(
            "14.ContinuousLearning",
            "Training",
            "LocalQwenOllamaTeacherInferenceClient.cs"));

        Assert.DoesNotContain(
            "CreateAuthority(",
            productionComposition,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "CreateAuthority(",
            localQwenClient,
            StringComparison.Ordinal);
        Assert.Contains(
            "CreateTeacherPolicyBudgetAuthority(",
            productionComposition,
            StringComparison.Ordinal);
        Assert.Contains(
            ".CreateTeacherPolicyBudgetAuthority()",
            localQwenClient,
            StringComparison.Ordinal);

        var productionFactory = typeof(ContinuousLearningProductionComposition)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method =>
                method.Name == nameof(
                    ContinuousLearningProductionComposition.CreateTp1Trainer) &&
                method.GetParameters().Length == 6 &&
                method.GetParameters()[0].ParameterType ==
                typeof(ServerOwnedLegalTrainingTeacherAdmissionAuthority));
        Assert.DoesNotContain(
            productionFactory.GetParameters(),
            parameter => parameter.ParameterType ==
                typeof(LegalTrainingTeacherAdmissionAuthority));
        Assert.DoesNotContain(
            typeof(ContinuousLearningProductionComposition)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method =>
                    method.Name == nameof(
                        ContinuousLearningProductionComposition.CreateTp1Trainer)),
            method => method.GetParameters().Any(parameter =>
                parameter.ParameterType ==
                typeof(LegalTrainingTeacherAdmissionAuthority)));
    }

    [Fact]
    public void ObsoleteTp1Factory_IsZeroTouchFailClosed()
    {
        var legacy = typeof(ContinuousLearningProductionComposition).GetMethod(
            nameof(ContinuousLearningProductionComposition.CreateTp1Trainer),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            [
                typeof(ISessionStorage),
                typeof(IIncrementalTrainingSessionFactory),
                typeof(ContinuousLearningTp1DpoConfiguration),
                typeof(string),
                typeof(LoraConfig)
            ],
            modifiers: null);
        Assert.NotNull(legacy);
        Assert.NotNull(legacy!.GetCustomAttribute<ObsoleteAttribute>());

        var result = Assert.IsType<OperationResult<IncrementalTrainer>>(
            legacy.Invoke(null, [null, null, null, null, null]));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public async Task ConfiguredDpoStepCount_ReachesIncrementalTrainerExecutorRequest()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "step-two-request",
            uniqueStoreSuffix: "configured-step-count");
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            CreateAuthority(),
            executor,
            fixture.AdapterDirectory,
            dpoMaxStepCount: 2);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, executor.CallCount);
        Assert.Equal(2, executor.LastRequest!.DpoStepCount);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(3, false)]
    public async Task DpoArtifactAcceptance_UsesActualStepWithinConfiguredMaximum(
        int actualDpoStepCount,
        bool expectedSuccess)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            $"actual-step-{actualDpoStepCount}",
            uniqueStoreSuffix: $"actual-step-{actualDpoStepCount}");
        var executor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success,
            actualDpoStepCount);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            CreateAuthority(),
            executor,
            fixture.AdapterDirectory,
            dpoMaxStepCount: 2);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.Equal(expectedSuccess, result.IsSuccess);
        if (!expectedSuccess)
            Assert.Equal("DPO_EXECUTION_ARTIFACT_INCOMPLETE", result.ErrorCode);
        Assert.Equal(1, executor.CallCount);
        Assert.Equal(
            DpoTrainer.BuildPolicyCheckpointFileName(actualDpoStepCount),
            Path.GetFileName(executor.LastArtifact!.PolicyCheckpointPath));
        Assert.Equal(
            DpoTrainer.BuildMetricsFileName(actualDpoStepCount),
            Path.GetFileName(executor.LastArtifact.MetricsPath));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));
        var snapshot = transaction.Read();
        Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
        var domain = Assert.Single(snapshot.Value!.Domains);
        Assert.Equal(
            expectedSuccess
                ? FileDpoPairStore.DpoTransactionDomainState.CandidateReady
                : FileDpoPairStore.DpoTransactionDomainState.MutationStarted,
            domain.State);
    }

    [Fact]
    public async Task Pipeline_RoutesDurablePairToItsDomainExactlyOnce()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var router = CreateDurableRouter(
            fixture,
            [("BUG", "pipeline-request")],
            "pipeline-route");
        var trainer = new PipelineRecordingTrainer();
        var pipeline = new ContinuousLearningPipeline(
            new NullSessionStorageCL(),
            new FixedDpoIndexer([Example()]),
            new AcceptingDpoCache(),
            router,
            trainer,
            new UnusedDpoRegistry());

        var result = await pipeline.RunCycleAsync("project");

        Assert.Equal("SYNTHETIC_TRAINING_STOP", result.ErrorCode);
        var call = Assert.Single(trainer.Calls);
        Assert.Equal("BUG", call.Domain);
        Assert.Equal(
            "pipeline-request",
            Assert.Single(call.Pairs).TeacherProvenance!.RequestId);
    }

    [Fact]
    public async Task Pipeline_RejectsUnroutableDurablePairWithoutSilentDrop()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var router = CreateDurableRouter(
            fixture,
            [("CODE_Q", "orphan-request")],
            "pipeline-unroutable");
        var trainer = new PipelineRecordingTrainer();
        var pipeline = new ContinuousLearningPipeline(
            new NullSessionStorageCL(),
            new FixedDpoIndexer([Example()]),
            new AcceptingDpoCache(),
            router,
            trainer,
            new UnusedDpoRegistry());

        var result = await pipeline.RunCycleAsync("project");

        Assert.Equal("SYNTHETIC_TRAINING_STOP", result.ErrorCode);
        var call = Assert.Single(trainer.Calls);
        Assert.Equal("BUG", call.Domain);
        Assert.NotEmpty(call.Examples);
        Assert.All(call.Examples, example => Assert.Equal("BUG", example.Domain));
        Assert.Empty(call.Pairs);
        var pending = router.GetDpoPairs();
        Assert.True(pending.IsSuccess, pending.ErrorMessage);
        var orphan = Assert.Single(pending.Value!);
        Assert.Equal("CODE_Q", orphan.Domain);
        Assert.Equal("orphan-request", orphan.TeacherProvenance!.RequestId);
    }

    [Fact]
    public void DpoAndFeedbackRecords_PreserveFiveArgumentConstructionAndDeconstruction()
    {
        var capturedAt = DateTimeOffset.UtcNow;
        var pair = new DpoPair("p", "c", "r", "BUG", capturedAt)
        {
            TeacherProvenance = QwenProvenance("abi-pair"),
            GovernanceDecision = Governance(),
        };
        var feedback = new FeedbackEvent(
            "p",
            "r",
            "c",
            FeedbackSignal.Edited,
            "BUG")
        {
            TeacherProvenance = QwenProvenance("abi-feedback"),
            GovernanceDecision = Governance(),
        };

        pair.Deconstruct(out var prompt, out var chosen, out var rejected, out var domain, out var at);
        feedback.Deconstruct(
            out var feedbackPrompt,
            out var response,
            out var correction,
            out var signal,
            out var feedbackDomain);

        Assert.Equal(("p", "c", "r", "BUG", capturedAt), (prompt, chosen, rejected, domain, at));
        Assert.Equal(("p", "r", "c", FeedbackSignal.Edited, "BUG"),
            (feedbackPrompt, response, correction, signal, feedbackDomain));
        Assert.NotNull(typeof(DpoPair).GetConstructor(
            [typeof(string), typeof(string), typeof(string), typeof(string), typeof(DateTimeOffset)]));
        Assert.NotNull(typeof(FeedbackEvent).GetConstructor(
            [typeof(string), typeof(string), typeof(string), typeof(FeedbackSignal), typeof(string)]));
    }

    [Fact]
    public async Task Tp1Composition_RejectsNonQwenLocalOllamaApacheAndInvalidGovernanceBeforeSession()
    {
        var cases = new (TrainingTeacherProvenance Provenance, TrainingDataGovernanceDecision? Governance, string Code)[]
        {
            (QwenProvenance("family") with { Family = OpenWeightTeacherFamily.Mistral },
                Governance(), LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode),
            (QwenProvenance("access") with { AccessMode = TrainingTeacherAccessMode.OnlineHostedOpenWeight },
                Governance(), LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode),
            (QwenProvenance("provider") with { HostingProvider = TrainingTeacherHostingProvider.DashScope },
                Governance(), LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode),
            (QwenProvenance("license") with { ModelLicense = TrainingTeacherModelLicense.MIT },
                Governance(), LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode),
            (QwenProvenance("governance-missing"), null,
                TrainingDataGovernanceAdmission.RequiredErrorCode),
            (QwenProvenance("license-denied"), Governance(licenseAccepted: false),
                TrainingDataGovernanceAdmission.LicenseDeniedErrorCode),
            (QwenProvenance("quarantined"), Governance(isQuarantined: true),
                TrainingDataGovernanceAdmission.QuarantinedErrorCode),
            (QwenProvenance("evidence-missing"), Governance(evidenceRefs: []),
                TrainingDataGovernanceAdmission.EvidenceInvalidErrorCode),
        };

        foreach (var (provenance, governance, expectedCode) in cases)
        {
            var factory = new CountingDpoSessionFactory();
            var composition = ContinuousLearningProductionComposition.CreateTp1Trainer(
                CreateAuthority(),
                new NullSessionStorageCL(),
                factory,
                CreateTp1Configuration());
            Assert.True(composition.IsSuccess, composition.ErrorMessage);

            var result = await composition.Value!.TrainAsync(
                "BUG",
                [Example()],
                [Pair(provenance, governance)]);

            Assert.Equal(expectedCode, result.ErrorCode);
            Assert.Equal(0, factory.CreateCount);
        }
    }

    [Fact]
    public void BoundedSnapshot_NeverReadsCount_AndNormalizesEveryNonFatalEnumerationPhase()
    {
        var valid = new AdversarialDpoList(
            [Pair(QwenProvenance("count-not-read"))],
            AdversarialDpoPhase.CountThrows);
        var captured = DpoPairBatchSnapshot.Capture(valid);
        Assert.True(captured.IsSuccess, captured.ErrorMessage);
        Assert.Single(captured.Value!);
        Assert.Equal(1, valid.DisposeCount);

        foreach (var phase in new[]
                 {
                     AdversarialDpoPhase.GetEnumeratorThrows,
                     AdversarialDpoPhase.MoveNextThrows,
                     AdversarialDpoPhase.CurrentThrows,
                     AdversarialDpoPhase.DisposeThrows,
                 })
        {
            var hostile = new AdversarialDpoList(
                [Pair(QwenProvenance($"phase-{phase}"))],
                phase);
            var result = DpoPairBatchSnapshot.Capture(hostile);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.EnumerationFailedErrorCode,
                result.ErrorCode);
            Assert.Equal(
                phase == AdversarialDpoPhase.GetEnumeratorThrows ? 0 : 1,
                hostile.DisposeCount);
        }

        var countLiarSource = new AdversarialDpoList(
            Enumerable.Range(
                    0,
                    LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1)
                .Select(index => Pair(QwenProvenance($"liar-{index}")))
                .ToArray(),
            AdversarialDpoPhase.CountLies);
        var countLiar = DpoPairBatchSnapshot.Capture(countLiarSource);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.BatchLimitExceededErrorCode,
            countLiar.ErrorCode);
        Assert.Equal(1, countLiarSource.DisposeCount);
    }

    [Fact]
    public async Task HostileBatchRejection_HasNoSessionExecutorOrFilesystemEffects()
    {
        foreach (var phase in new[]
                 {
                     AdversarialDpoPhase.GetEnumeratorThrows,
                     AdversarialDpoPhase.MoveNextThrows,
                     AdversarialDpoPhase.CurrentThrows,
                     AdversarialDpoPhase.DisposeThrows,
                 })
        {
            var root = Path.Combine(Path.GetTempPath(), $"dpo-hostile-{Guid.NewGuid():N}");
            var factory = new CountingDpoSessionFactory();
            var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
            var trainer = new IncrementalTrainer(
                new NullSessionStorageCL(),
                factory,
                CreateAuthority(),
                executor,
                root);

            var result = await trainer.TrainAsync(
                "BUG",
                [Example()],
                new AdversarialDpoList(
                    [Pair(QwenProvenance($"reject-{phase}"))],
                    phase));

            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.EnumerationFailedErrorCode,
                result.ErrorCode);
            Assert.Equal(0, factory.CreateCount);
            Assert.Equal(0, executor.CallCount);
            Assert.False(Directory.Exists(root));
        }

        var liarRoot = Path.Combine(Path.GetTempPath(), $"dpo-liar-{Guid.NewGuid():N}");
        var liarFactory = new CountingDpoSessionFactory();
        var liarExecutor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var liarTrainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            liarFactory,
            CreateAuthority(),
            liarExecutor,
            liarRoot);
        var liarResult = await liarTrainer.TrainAsync(
            "BUG",
            [Example()],
            new AdversarialDpoList(
                Enumerable.Range(
                        0,
                        LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1)
                    .Select(index => Pair(QwenProvenance($"side-effect-liar-{index}")))
                    .ToArray(),
                AdversarialDpoPhase.CountLies));
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.BatchLimitExceededErrorCode,
            liarResult.ErrorCode);
        Assert.Equal(0, liarFactory.CreateCount);
        Assert.Equal(0, liarExecutor.CallCount);
        Assert.False(Directory.Exists(liarRoot));
    }

    [Fact]
    public void BoundedSnapshot_CancellationAndFatalGraphsEscape()
    {
        Assert.Throws<OperationCanceledException>(() => DpoPairBatchSnapshot.Capture(
            new AdversarialDpoList([], AdversarialDpoPhase.GetEnumeratorThrows,
                new OperationCanceledException("synthetic"))));
        Assert.Throws<OutOfMemoryException>(() => DpoPairBatchSnapshot.Capture(
            new AdversarialDpoList([], AdversarialDpoPhase.GetEnumeratorThrows,
                new OutOfMemoryException("synthetic"))));
    }

    [Theory]
    [InlineData(nameof(AdversarialDpoPhase.MoveNextThrows), false)]
    [InlineData(nameof(AdversarialDpoPhase.MoveNextThrows), true)]
    [InlineData(nameof(AdversarialDpoPhase.CurrentThrows), false)]
    [InlineData(nameof(AdversarialDpoPhase.CurrentThrows), true)]
    public async Task FatalOrCancelledEnumeration_DisposesOnceBeforeEscapingWithoutSideEffects(
        string phaseName,
        bool fatal)
    {
        var phase = Enum.Parse<AdversarialDpoPhase>(phaseName);
        var root = Path.Combine(Path.GetTempPath(), $"dpo-fatal-enumeration-{Guid.NewGuid():N}");
        var factory = new CountingDpoSessionFactory();
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            factory,
            CreateAuthority(),
            executor,
            root);
        Exception primary = fatal
            ? new OutOfMemoryException("synthetic-fatal-primary")
            : new OperationCanceledException("synthetic-cancelled-primary");
        var pairs = new AdversarialDpoList(
            [Pair(QwenProvenance($"dispose-{phase}-{fatal}"))],
            phase,
            primary);

        var thrown = await Record.ExceptionAsync(() => trainer.TrainAsync(
            "BUG",
            [Example()],
            pairs));

        Assert.Same(primary, thrown);
        Assert.Equal(1, pairs.DisposeCount);
        Assert.Equal(0, factory.CreateCount);
        Assert.Equal(0, executor.CallCount);
        Assert.False(Directory.Exists(root));
    }

    [Theory]
    [InlineData(nameof(AdversarialDpoPhase.MoveNextThrows), false, false)]
    [InlineData(nameof(AdversarialDpoPhase.MoveNextThrows), false, true)]
    [InlineData(nameof(AdversarialDpoPhase.MoveNextThrows), true, false)]
    [InlineData(nameof(AdversarialDpoPhase.MoveNextThrows), true, true)]
    [InlineData(nameof(AdversarialDpoPhase.CurrentThrows), false, false)]
    [InlineData(nameof(AdversarialDpoPhase.CurrentThrows), false, true)]
    [InlineData(nameof(AdversarialDpoPhase.CurrentThrows), true, false)]
    [InlineData(nameof(AdversarialDpoPhase.CurrentThrows), true, true)]
    public async Task FatalPrimaryAndFatalDispose_AggregateInOrderWithoutSideEffects(
        string phaseName,
        bool primaryFatal,
        bool cleanupFatal)
    {
        var phase = Enum.Parse<AdversarialDpoPhase>(phaseName);
        var root = Path.Combine(Path.GetTempPath(), $"dpo-dual-fatal-{Guid.NewGuid():N}");
        var factory = new CountingDpoSessionFactory();
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            factory,
            CreateAuthority(),
            executor,
            root);
        Exception primary = primaryFatal
            ? new OutOfMemoryException("synthetic-fatal-primary")
            : new OperationCanceledException("synthetic-cancelled-primary");
        Exception cleanup = cleanupFatal
            ? new OutOfMemoryException("synthetic-fatal-cleanup")
            : new OperationCanceledException("synthetic-cancelled-cleanup");
        var pairs = new AdversarialDpoList(
            [Pair(QwenProvenance($"dual-fatal-{phase}-{primaryFatal}-{cleanupFatal}"))],
            phase,
            primary,
            cleanup);

        var thrown = await Record.ExceptionAsync(() => trainer.TrainAsync(
            "BUG",
            [Example()],
            pairs));

        var aggregate = Assert.IsType<AggregateException>(thrown);
        Assert.Collection(
            aggregate.InnerExceptions,
            first => Assert.Same(primary, first),
            second => Assert.Same(cleanup, second));
        Assert.Contains(
            phase == AdversarialDpoPhase.MoveNextThrows ? "MoveNext" : "Current",
            primary.StackTrace!);
        Assert.Contains("Dispose", cleanup.StackTrace!);
        Assert.Equal(1, pairs.DisposeCount);
        Assert.Equal(0, factory.CreateCount);
        Assert.Equal(0, executor.CallCount);
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void BoundedSnapshot_PreservesFatalPrecedenceAcrossDisposeFailures()
    {
        var ordinaryPrimary = new InvalidOperationException("ordinary-primary");
        var ordinaryDispose = new InvalidOperationException("ordinary-dispose");
        var bothOrdinary = new AdversarialDpoList(
            [Pair(QwenProvenance("ordinary-primary-ordinary-dispose"))],
            AdversarialDpoPhase.MoveNextThrows,
            ordinaryPrimary,
            ordinaryDispose);

        var ordinaryResult = DpoPairBatchSnapshot.Capture(bothOrdinary);

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.EnumerationFailedErrorCode,
            ordinaryResult.ErrorCode);
        Assert.Equal(1, bothOrdinary.DisposeCount);

        var fatalPrimary = new OutOfMemoryException("fatal-primary");
        var fatalThenOrdinary = new AdversarialDpoList(
            [Pair(QwenProvenance("fatal-primary-ordinary-dispose"))],
            AdversarialDpoPhase.CurrentThrows,
            fatalPrimary,
            new InvalidOperationException("ordinary-dispose"));

        Assert.Same(
            fatalPrimary,
            Record.Exception(() => DpoPairBatchSnapshot.Capture(fatalThenOrdinary)));
        Assert.Equal(1, fatalThenOrdinary.DisposeCount);

        var fatalDispose = new OutOfMemoryException("fatal-dispose");
        var ordinaryThenFatal = new AdversarialDpoList(
            [Pair(QwenProvenance("ordinary-primary-fatal-dispose"))],
            AdversarialDpoPhase.MoveNextThrows,
            new InvalidOperationException("ordinary-primary"),
            fatalDispose);

        Assert.Same(
            fatalDispose,
            Record.Exception(() => DpoPairBatchSnapshot.Capture(ordinaryThenFatal)));
        Assert.Equal(1, ordinaryThenFatal.DisposeCount);
    }

    [Fact]
    public async Task AdmittedFakeDpoAndFakeSupervisedStep_CreateDurableCandidateWithoutFinalRegistryPublication()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "fake-success",
            uniqueStoreSuffix: "fake-success");
        var dpoExecutor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            CreateAuthority(),
            dpoExecutor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "p", "r")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, dpoExecutor.CallCount);
        Assert.Equal(1, supervised.CallCount);
        Assert.True(File.Exists(result.Value!.AdapterPath));
        if (OperatingSystem.IsLinux())
        {
            // На Linux этот тест проверяет конечные права промежуточного
            // кандидата обученных весов: читать и писать файл может только
            // владелец. То, что права задаются сразу при создании файла,
            // видно из production-механизма в защищённом хранилище, а не из
            // одного этого утверждения теста.
            Assert.Equal(
                SecureLocalFileBoundary.OwnerFileMode,
                File.GetUnixFileMode(result.Value.AdapterPath));
            Assert.True(SecureLocalFileBoundary.HasOwnerOnlyFileMode(
                result.Value.AdapterPath));
        }
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));
        Assert.Equal(
            FileDpoPairStore.DpoTransactionDomainState.CandidateReady,
            Assert.Single(transaction.Read().Value!.Domains).State);
    }

    [Fact]
    public async Task RepositoryOwnedResumedTraining_AcceptedUpdatePublishesSealedChildAndFailedUpdateDoesNot()
    {
        using (var acceptedFixture = new TinyTrainingFixtureCL())
        {
            var accepted = CreateRepositoryOwnedResumedTrainingSetup(
                acceptedFixture,
                "accepted");
            var (transaction, pairs) =
                PrepareDurableTransactionFromRepositoryOwnedDataset(
                    accepted.CurrentDpoDatasetAddress,
                    RepositoryOwnedDomainSlug);
            var dpoExecutor = new RecordingDpoExecutor(
                DpoExecutorBehavior.Success);
            var supervised = new MutatingSupervisedStepExecutor();
            var trainer = new IncrementalTrainer(
                new NullSessionStorageCL(),
                accepted.Factory,
                CreateAuthority(),
                dpoExecutor,
                supervised,
                acceptedFixture.AdapterDirectory);

            Assert.False(File.Exists(accepted.ChildAddress.WeightsPath));
            Assert.False(File.Exists(accepted.ChildAddress.ManifestPath));

            var result = await ((ITransactionalIncrementalTrainer)trainer)
                .TrainTransactionAsync(
                    RepositoryOwnedDomainSlug,
                    [acceptedFixture.Example(
                        RepositoryOwnedDomainSlug,
                        "p",
                        "r")],
                    pairs,
                    transaction,
                    CancellationToken.None);

            Assert.True(result.IsSuccess,
                $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.Equal(1, accepted.FactoryCreateCount());
            Assert.Equal(1, dpoExecutor.CallCount);
            Assert.Equal(1, supervised.CallCount);
            Assert.True(File.Exists(accepted.ChildAddress.WeightsPath));
            Assert.True(File.Exists(accepted.ChildAddress.ManifestPath));
            var opened = RepositoryOwnedModelCheckpointManifestSealer
                .OpenSealed(accepted.ChildAddress);
            Assert.True(opened.IsSuccess, opened.ErrorMessage);
            var child = opened.Value!.Manifest;
            Assert.Equal(2, child.Step);
            Assert.EndsWith(
                "checkpoints/000002/manifest.json",
                child.RepositoryRelativeManifestPath,
                StringComparison.Ordinal);
            Assert.NotNull(child.ParentCheckpoint);
            var parentIdentity = child.ParentCheckpoint!;
            Assert.Equal(1, parentIdentity.Step);
            Assert.EndsWith(
                "checkpoints/000001/manifest.json",
                parentIdentity.RepositoryRelativeManifestPath,
                StringComparison.Ordinal);
            Assert.Equal(
                accepted.ParentManifest.RepositoryRelativeManifestPath,
                parentIdentity.RepositoryRelativeManifestPath);
            Assert.Equal(
                accepted.ParentManifest.WeightsSha256,
                parentIdentity.CheckpointSha256);
            Assert.Equal(
                accepted.ParentManifest.WeightsByteLength,
                parentIdentity.CheckpointByteLength);
            var expectedCurrentDpoDatasetManifest =
                Assert.IsType<RepositoryOwnedDpoDatasetManifestIdentity>(
                    accepted.CurrentDpoDatasetManifest);
            Assert.NotEqual(
                accepted.ParentDpoDatasetManifest,
                child.DpoDatasetManifest);
            Assert.Equal(
                expectedCurrentDpoDatasetManifest,
                child.DpoDatasetManifest);
            Assert.Contains(
                "incremental-trainer-accepted-update",
                child.ProvenanceRefs);
            Assert.Contains(
                $"training-source-manifest-sha256:{result.Value!.Provenance.TrainingSources.Sha256}",
                child.ProvenanceRefs);
        }

        using (var failedFixture = new TinyTrainingFixtureCL())
        {
            var failed = CreateRepositoryOwnedResumedTrainingSetup(
                failedFixture,
                "failed");
            var (transaction, pairs) =
                PrepareDurableTransactionFromRepositoryOwnedDataset(
                    failed.CurrentDpoDatasetAddress,
                    RepositoryOwnedDomainSlug);
            var dpoExecutor = new RecordingDpoExecutor(
                DpoExecutorBehavior.Success);
            var supervised = new NonFiniteSupervisedStepExecutor();
            var trainer = new IncrementalTrainer(
                new NullSessionStorageCL(),
                failed.Factory,
                CreateAuthority(),
                dpoExecutor,
                supervised,
                failedFixture.AdapterDirectory);

            var result = await ((ITransactionalIncrementalTrainer)trainer)
                .TrainTransactionAsync(
                    RepositoryOwnedDomainSlug,
                    [failedFixture.Example(
                        RepositoryOwnedDomainSlug,
                        "p",
                        "r")],
                    pairs,
                    transaction,
                    CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                "SUPERVISED_ADAPTER_TRAINING_FAILED",
                result.ErrorCode);
            Assert.Equal(1, failed.FactoryCreateCount());
            Assert.Equal(1, dpoExecutor.CallCount);
            Assert.Equal(1, supervised.CallCount);
            Assert.False(File.Exists(failed.ChildAddress.WeightsPath));
            Assert.False(File.Exists(failed.ChildAddress.ManifestPath));
        }
    }

    [Fact]
    public async Task RepositoryOwnedColdStartTraining_AcceptedUpdatePublishesSealedStepOne()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryOwnedSentinelRoot(
            fixture,
            "cold-start-step-one");
        var runtimeCompatibility = RepositoryOwnedRuntimeCompatibility();
        var currentDpo = SealRepositoryOwnedDpoDataset(
            repositoryRoot,
            "v0001");
        const string reason =
            "operator approved first repository checkpoint from retained base";
        var startRequest =
            new RepositoryOwnedIncrementalTrainingSessionStartRequest(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                runtimeCompatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                    reason),
                CurrentDpoDatasetManifest: currentDpo.Manifest);
        var stepOneAddress = RequireSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                step: 1));
        var repositoryLoadCount = 0;
        var retainedBaseOpenCount = 0;
        using var retainedAccess = CreateRetainedFileAccess(
            fixture,
            "cold-start-step-one");
        using var retainedFactory = RequireSuccess(
            RetainedCheckpointBackedIncrementalTrainingSessionFactory.Bind(
                retainedAccess,
                new NullSessionStorageCL(),
                fixture.ModelConfig,
                fixture.TrainingConfig,
                afterRetainedArtifactOpenForTesting: role =>
                {
                    if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                        retainedBaseOpenCount++;
                },
                loadRepositoryCheckpoint: request =>
                {
                    repositoryLoadCount++;
                    Assert.Same(startRequest, request);
                    return OperationResult<RepositoryOwnedModelCheckpointLoadResult>
                        .Fail(
                            "No repository-owned checkpoint tree exists yet.",
                            RepositoryOwnedModelCheckpointLoader
                                .NoCheckpointTreeErrorCode);
                },
                resolveRepositoryStartRequest: domain =>
                {
                    Assert.Equal(RepositoryOwnedDomainSlug, domain);
                    return OperationResult<
                        RepositoryOwnedIncrementalTrainingSessionStartRequest>
                        .Ok(startRequest);
                }));
        var recordingFactory =
            new PreTrainingModelRecordingSessionFactory(retainedFactory);
        var (transaction, pairs) =
            PrepareDurableTransactionFromRepositoryOwnedDataset(
                currentDpo.Address,
                RepositoryOwnedDomainSlug);
        var dpoExecutor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            recordingFactory,
            CreateAuthority(),
            dpoExecutor,
            supervised,
            fixture.AdapterDirectory);

        Assert.False(File.Exists(stepOneAddress.WeightsPath));
        Assert.False(File.Exists(stepOneAddress.ManifestPath));

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                RepositoryOwnedDomainSlug,
                [fixture.Example(RepositoryOwnedDomainSlug, "p", "r")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.True(result.IsSuccess,
            $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, recordingFactory.CreateCount);
        Assert.Equal(1, repositoryLoadCount);
        Assert.Equal(1, retainedBaseOpenCount);
        Assert.Equal(1, dpoExecutor.CallCount);
        Assert.Equal(1, supervised.CallCount);
        var metadata = Assert.IsType<IncrementalTrainingCheckpointStartMetadata>(
            recordingFactory.CreatedCheckpointStart);
        Assert.Equal(
            IncrementalTrainingCheckpointStartSource
                .RetainedBaseCheckpointColdStart,
            metadata.Source);
        Assert.Same(startRequest, metadata.RepositoryStartRequest);
        Assert.Null(metadata.ResumeParentCheckpoint);
        var coldStart = Assert.IsType<IncrementalTrainingColdStartMetadata>(
            metadata.ColdStart);
        Assert.Equal(reason, coldStart.HumanReason);
        Assert.Same(startRequest, coldStart.RepositoryStartRequest);
        Assert.True(File.Exists(stepOneAddress.WeightsPath));
        Assert.True(File.Exists(stepOneAddress.ManifestPath));

        var opened = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(stepOneAddress);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        var stepOne = opened.Value!.Manifest;
        Assert.Equal(1, stepOne.Step);
        Assert.EndsWith(
            "checkpoints/000001/manifest.json",
            stepOne.RepositoryRelativeManifestPath,
            StringComparison.Ordinal);
        Assert.Null(stepOne.ParentCheckpoint);
        Assert.Equal(reason, stepOne.ColdStartReason);
        Assert.Equal(currentDpo.Manifest, stepOne.DpoDatasetManifest);

        var loaded = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                startRequest.RepositoryRoot,
                startRequest.DomainSlug,
                startRequest.DesignPatternSlug,
                startRequest.ModelIdSlug,
                startRequest.ModelVersion,
                startRequest.RuntimeCompatibility);
        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(1, loaded.Value!.Manifest.Step);
        Assert.Null(loaded.Value.Manifest.ParentCheckpoint);
        Assert.Equal(reason, loaded.Value.Manifest.ColdStartReason);
        Assert.Equal(currentDpo.Manifest, loaded.Value.Manifest.DpoDatasetManifest);
        loaded.Value.Dispose();
    }

    [Fact]
    public async Task RepositoryOwnedColdStartTraining_ExistingAdapterCandidateFinishesStepOnePublication()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var repositoryRoot = CreateRepositoryOwnedSentinelRoot(
            fixture,
            "cold-start-existing-adapter");
        var runtimeCompatibility = RepositoryOwnedRuntimeCompatibility();
        var currentDpo = SealRepositoryOwnedDpoDataset(
            repositoryRoot,
            "v0001");
        var example = fixture.Example(
            RepositoryOwnedDomainSlug,
            "p",
            "r");
        var trainingExamples = new[] { example };
        var expectedTrainingSources = RequireSuccess(
            TrainingSourceProvenance.BuildManifest(
                trainingExamples.Select(static value => value.SourceId)));
        var expectedTrainingExampleBatchSha256 =
            BuildExpectedTrainingExampleBatchSha256(trainingExamples);
        const string reason =
            "operator approved first repository checkpoint from existing adapter guard";
        var startRequest =
            new RepositoryOwnedIncrementalTrainingSessionStartRequest(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                runtimeCompatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                    reason),
                CurrentDpoDatasetManifest: currentDpo.Manifest);
        var stepOneAddress = RequireSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                step: 1));
        var repositoryLoadCount = 0;
        var retainedBaseOpenCount = 0;
        using var retainedAccess = CreateRetainedFileAccess(
            fixture,
            "cold-start-existing-adapter");
        using var retainedFactory = RequireSuccess(
            RetainedCheckpointBackedIncrementalTrainingSessionFactory.Bind(
                retainedAccess,
                new NullSessionStorageCL(),
                fixture.ModelConfig,
                fixture.TrainingConfig,
                afterRetainedArtifactOpenForTesting: role =>
                {
                    if (role == ContinuousLearningRetainedArtifactRole.BaseCheckpoint)
                        retainedBaseOpenCount++;
                },
                loadRepositoryCheckpoint: request =>
                {
                    repositoryLoadCount++;
                    Assert.Same(startRequest, request);
                    return OperationResult<RepositoryOwnedModelCheckpointLoadResult>
                        .Fail(
                            "No repository-owned checkpoint tree exists yet.",
                            RepositoryOwnedModelCheckpointLoader
                                .NoCheckpointTreeErrorCode);
                },
                resolveRepositoryStartRequest: domain =>
                {
                    Assert.Equal(RepositoryOwnedDomainSlug, domain);
                    return OperationResult<
                        RepositoryOwnedIncrementalTrainingSessionStartRequest>
                        .Ok(startRequest);
                }));
        var recordingFactory =
            new PreTrainingModelRecordingSessionFactory(retainedFactory);
        var (transaction, pairs) =
            PrepareDurableTransactionFromRepositoryOwnedDataset(
                currentDpo.Address,
                RepositoryOwnedDomainSlug);
        var dpoExecutor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var repositoryTrainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            recordingFactory,
            CreateAuthority(),
            dpoExecutor,
            supervised,
            fixture.AdapterDirectory);

        // WHY: this is a two-run mutation-sensitive regression. The first run
        // must pass DPO + supervised mutation and persist the durable adapter
        // candidate before this artificial full-checkpoint collision fails
        // publication; otherwise the retry would not exercise ExistingArtifact.
        Directory.CreateDirectory(stepOneAddress.WeightsPath);
        Assert.True(Directory.Exists(stepOneAddress.WeightsPath));
        Assert.False(File.Exists(stepOneAddress.ManifestPath));

        var first = await ((ITransactionalIncrementalTrainer)repositoryTrainer)
            .TrainTransactionAsync(
                RepositoryOwnedDomainSlug,
                [example],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.False(first.IsSuccess);
        Assert.Equal(1, recordingFactory.CreateCount);
        Assert.Equal(1, repositoryLoadCount);
        Assert.Equal(1, retainedBaseOpenCount);
        Assert.Equal(1, dpoExecutor.CallCount);
        Assert.Equal(1, supervised.CallCount);
        var firstSnapshot = transaction.Read();
        Assert.True(firstSnapshot.IsSuccess, firstSnapshot.ErrorMessage);
        var firstDomain = Assert.Single(firstSnapshot.Value!.Domains);
        var firstSession = Assert.Single(recordingFactory.CreatedSessions);
        Assert.Null(firstSession.ParentAdapterCheckpoint);
        var firstSeed = ReadCandidateSeedJson(
            currentDpo.Address,
            transaction.TransactionId,
            RepositoryOwnedDomainSlug);
        AssertColdStartCandidateSeed(
            firstSeed,
            expectedTrainingSources,
            firstSession,
            trainingExamples.Length,
            expectedTrainingExampleBatchSha256);
        Assert.Equal(
            FileDpoPairStore.DpoTransactionDomainState.CandidateReady,
            firstDomain.State);
        Assert.Equal(trainingExamples.Length, firstDomain.TrainingExamples);
        Assert.NotNull(firstDomain.Artifact);
        var firstArtifact = firstDomain.Artifact!;
        Assert.Equal(
            expectedTrainingSources.Sha256,
            firstArtifact.Provenance.TrainingSources.Sha256);
        Assert.Equal(
            expectedTrainingSources.SourceIds,
            firstArtifact.Provenance.TrainingSources.SourceIds);
        Assert.Equal(
            firstSession.BaseCheckpointSha256,
            firstArtifact.Provenance.BaseCheckpointSha256);
        Assert.Equal(
            firstSession.TokenizerSha256,
            firstArtifact.Provenance.TokenizerSha256);
        Assert.Equal(firstSession.ModelIdentity, firstArtifact.Provenance.Model);
        AssertLoraIdentity(
            TrainingSourceProvenance.Identify(IncrementalTrainer.DefaultConfig),
            firstArtifact.Provenance.Lora);
        Assert.True(File.Exists(firstArtifact.AdapterPath));
        Assert.False(File.Exists(stepOneAddress.WeightsPath));
        Assert.True(Directory.Exists(stepOneAddress.WeightsPath));
        Assert.False(File.Exists(stepOneAddress.ManifestPath));

        var release = transaction.ReleaseIfUnmutated();
        Assert.True(release.IsSuccess, release.ErrorMessage);
        Assert.False(release.Value);
        Directory.Delete(stepOneAddress.WeightsPath, recursive: true);
        Assert.False(Directory.Exists(stepOneAddress.WeightsPath));

        var (retryTransaction, retryPairs) =
            PrepareDurableTransactionFromRepositoryOwnedDataset(
                currentDpo.Address,
                RepositoryOwnedDomainSlug);
        Assert.Equal(transaction.TransactionId, retryTransaction.TransactionId);
        var retryDpoExecutor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success);
        var retrySupervised = new MutatingSupervisedStepExecutor();
        var retryTrainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            recordingFactory,
            CreateAuthority(),
            retryDpoExecutor,
            retrySupervised,
            fixture.AdapterDirectory);

        // WHY: the retry reopens the same durable transaction, so
        // PrepareCandidate returns ExistingArtifact. Repository-owned cold
        // start still owes checkpoint 000001 and must publish it from the same
        // durable adapter bytes instead of running DPO/supervised mutation a
        // second time.
        var retry = await ((ITransactionalIncrementalTrainer)retryTrainer)
            .TrainTransactionAsync(
                RepositoryOwnedDomainSlug,
                [example],
                retryPairs,
                retryTransaction,
                CancellationToken.None);

        Assert.Equal(2, recordingFactory.CreatedSessions.Count);
        var retrySession = recordingFactory.CreatedSessions[1];
        Assert.Null(retrySession.ParentAdapterCheckpoint);
        Assert.Equal(
            firstSession.BaseCheckpointSha256,
            retrySession.BaseCheckpointSha256);
        Assert.Equal(
            firstSession.TokenizerSha256,
            retrySession.TokenizerSha256);
        Assert.Equal(firstSession.ModelIdentity, retrySession.ModelIdentity);
        AssertSameColdStartCheckpointStart(
            firstSession.CheckpointStart,
            retrySession.CheckpointStart,
            startRequest,
            reason);
        var retrySeed = ReadCandidateSeedJson(
            currentDpo.Address,
            retryTransaction.TransactionId,
            RepositoryOwnedDomainSlug);
        AssertColdStartCandidateSeed(
            retrySeed,
            expectedTrainingSources,
            retrySession,
            trainingExamples.Length,
            expectedTrainingExampleBatchSha256);
        Assert.True(retry.IsSuccess,
            $"{retry.ErrorCode}: {retry.ErrorMessage}");
        Assert.Equal(firstArtifact.AdapterPath, retry.Value!.AdapterPath);
        Assert.Equal(
            firstArtifact.Provenance.AdapterSha256,
            retry.Value.Provenance.AdapterSha256);
        Assert.Equal(2, recordingFactory.CreateCount);
        Assert.Equal(2, repositoryLoadCount);
        Assert.Equal(2, retainedBaseOpenCount);
        Assert.Equal(0, retryDpoExecutor.CallCount);
        Assert.Equal(0, retrySupervised.CallCount);
        Assert.True(File.Exists(stepOneAddress.WeightsPath));
        Assert.False(Directory.Exists(stepOneAddress.WeightsPath));
        Assert.True(File.Exists(stepOneAddress.ManifestPath));
        var openedRetry = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(stepOneAddress);
        Assert.True(openedRetry.IsSuccess, openedRetry.ErrorMessage);
        var retryStepOne = openedRetry.Value!.Manifest;
        Assert.Equal(1, retryStepOne.Step);
        Assert.Null(retryStepOne.ParentCheckpoint);
        Assert.Equal(reason, retryStepOne.ColdStartReason);
        Assert.Equal(currentDpo.Manifest, retryStepOne.DpoDatasetManifest);
        var retryStepOneWeights = File.ReadAllBytes(stepOneAddress.WeightsPath);
        Assert.Equal(
            retryStepOne.WeightsSha256,
            Sha256Hex(retryStepOneWeights));
        Assert.Equal(
            retryStepOne.WeightsByteLength,
            retryStepOneWeights.Length);
        Assert.NotNull(recordingFactory.CreatedModelBytes);
        Assert.NotEqual(
            Sha256Hex(recordingFactory.CreatedModelBytes!),
            retryStepOne.WeightsSha256);
        var loadedRetry = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                startRequest.RepositoryRoot,
                startRequest.DomainSlug,
                startRequest.DesignPatternSlug,
                startRequest.ModelIdSlug,
                startRequest.ModelVersion,
                startRequest.RuntimeCompatibility);
        Assert.True(loadedRetry.IsSuccess, loadedRetry.ErrorMessage);
        Assert.Equal(1, loadedRetry.Value!.Manifest.Step);
        Assert.Equal(
            retryStepOne.ManifestSha256,
            loadedRetry.Value.Manifest.ManifestSha256);
        loadedRetry.Value.Dispose();
    }

    [Fact]
    public async Task RepositoryOwnedResumedTraining_FreshFactoryReloadsPublished000002AndPublishes000003()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var first = CreateRepositoryOwnedResumedTrainingSetup(
            fixture,
            "fresh-factory-reload");
        var (firstTransaction, firstPairs) =
            PrepareDurableTransactionFromRepositoryOwnedDataset(
                first.CurrentDpoDatasetAddress,
                RepositoryOwnedDomainSlug);
        var firstDpoExecutor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success);
        var firstSupervised = new MutatingSupervisedStepExecutor();
        var firstTrainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            first.Factory,
            CreateAuthority(),
            firstDpoExecutor,
            firstSupervised,
            fixture.AdapterDirectory);

        var firstResult = await ((ITransactionalIncrementalTrainer)firstTrainer)
            .TrainTransactionAsync(
                RepositoryOwnedDomainSlug,
                [fixture.Example(RepositoryOwnedDomainSlug, "p", "r")],
                firstPairs,
                firstTransaction,
                CancellationToken.None);

        Assert.True(firstResult.IsSuccess,
            $"{firstResult.ErrorCode}: {firstResult.ErrorMessage}");
        Assert.Equal(1, first.FactoryCreateCount());
        Assert.Equal(1, firstDpoExecutor.CallCount);
        Assert.Equal(1, firstSupervised.CallCount);
        var openedStepTwo = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(first.ChildAddress);
        Assert.True(openedStepTwo.IsSuccess, openedStepTwo.ErrorMessage);
        var stepTwo = openedStepTwo.Value!.Manifest;
        Assert.Equal(2, stepTwo.Step);
        Assert.EndsWith(
            "checkpoints/000002/manifest.json",
            stepTwo.RepositoryRelativeManifestPath,
            StringComparison.Ordinal);
        var stepTwoWeights = File.ReadAllBytes(first.ChildAddress.WeightsPath);
        Assert.Equal(stepTwo.WeightsSha256, Sha256Hex(stepTwoWeights));
        Assert.Equal(stepTwo.WeightsByteLength, stepTwoWeights.Length);

        var currentDpo = SealRepositoryOwnedDpoDataset(
            first.RepositoryRoot,
            "v0003");
        var stepThreeAddress = RequireSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                first.RepositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                step: 3));
        var secondStartRequest =
            new RepositoryOwnedIncrementalTrainingSessionStartRequest(
                first.RepositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                first.RuntimeCompatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
                CurrentDpoDatasetManifest: currentDpo.Manifest);
        byte[]? loadedRepositoryCheckpointBytes = null;
        RepositoryOwnedModelCheckpointManifest? selectedRepositoryManifest = null;
        var repositoryLoadRequests =
            new List<RepositoryOwnedIncrementalTrainingSessionStartRequest>();
        var retainedBaseOpenCount = 0;
        using var retainedAccess = CreateRetainedFileAccess(
            fixture,
            "fresh-factory-reload");
        using var retainedFactory = RequireSuccess(
            RetainedCheckpointBackedIncrementalTrainingSessionFactory.Bind(
                retainedAccess,
                new NullSessionStorageCL(),
                fixture.ModelConfig,
                fixture.TrainingConfig,
                afterRetainedArtifactOpenForTesting: role =>
                {
                    if (role == ContinuousLearningRetainedArtifactRole
                            .BaseCheckpoint)
                    {
                        retainedBaseOpenCount++;
                    }
                },
                loadRepositoryCheckpoint: request =>
                {
                    repositoryLoadRequests.Add(request);
                    var loaded = new RepositoryOwnedModelCheckpointLoader()
                        .LoadLatestCompatible(
                            request.RepositoryRoot,
                            request.DomainSlug,
                            request.DesignPatternSlug,
                            request.ModelIdSlug,
                            request.ModelVersion,
                            request.RuntimeCompatibility);
                    if (loaded.IsSuccess && loaded.Value is not null)
                    {
                        selectedRepositoryManifest = loaded.Value.Manifest;
                        loadedRepositoryCheckpointBytes =
                            ReadAndRewind(loaded.Value.Weights);
                    }

                    return loaded;
                },
                resolveRepositoryStartRequest: domain =>
                {
                    Assert.Equal(RepositoryOwnedDomainSlug, domain);
                    return OperationResult<
                        RepositoryOwnedIncrementalTrainingSessionStartRequest>
                        .Ok(secondStartRequest);
                },
                repositoryCheckpointAdapterConfig:
                    IncrementalTrainer.DefaultConfig));
        var recordingFactory = new PreTrainingModelRecordingSessionFactory(
            retainedFactory);
        var (secondTransaction, secondPairs) =
            PrepareDurableTransactionFromRepositoryOwnedDataset(
                currentDpo.Address,
                RepositoryOwnedDomainSlug);
        var secondDpoExecutor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success);
        var secondSupervised = new MutatingSupervisedStepExecutor();
        var secondTrainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            recordingFactory,
            CreateAuthority(),
            secondDpoExecutor,
            secondSupervised,
            fixture.AdapterDirectory);

        Assert.False(File.Exists(stepThreeAddress.WeightsPath));
        Assert.False(File.Exists(stepThreeAddress.ManifestPath));
        var secondResult = await ((ITransactionalIncrementalTrainer)secondTrainer)
            .TrainTransactionAsync(
                RepositoryOwnedDomainSlug,
                [fixture.Example(RepositoryOwnedDomainSlug, "p2", "r2")],
                secondPairs,
                secondTransaction,
                CancellationToken.None);

        Assert.True(secondResult.IsSuccess,
            $"{secondResult.ErrorCode}: {secondResult.ErrorMessage}");
        Assert.Equal(1, recordingFactory.CreateCount);
        Assert.Equal(1, secondDpoExecutor.CallCount);
        Assert.Equal(1, secondSupervised.CallCount);
        Assert.Equal(0, retainedBaseOpenCount);
        Assert.Equal(secondStartRequest, Assert.Single(repositoryLoadRequests));
        var selected = Assert.IsType<RepositoryOwnedModelCheckpointManifest>(
            selectedRepositoryManifest);
        Assert.Equal(2, selected.Step);
        Assert.Equal(
            stepTwo.RepositoryRelativeManifestPath,
            selected.RepositoryRelativeManifestPath);
        Assert.Equal(stepTwo.ManifestSha256, selected.ManifestSha256);
        Assert.Equal(stepTwo.WeightsSha256, selected.WeightsSha256);
        Assert.Equal(stepTwoWeights, loadedRepositoryCheckpointBytes);
        Assert.Equal(
            stepTwo.WeightsSha256,
            recordingFactory.CreatedBaseCheckpointSha256);
        Assert.Equal(stepTwoWeights, recordingFactory.CreatedModelBytes);
        var startMetadata =
            Assert.IsType<IncrementalTrainingCheckpointStartMetadata>(
                recordingFactory.CreatedCheckpointStart);
        Assert.Equal(
            IncrementalTrainingCheckpointStartSource
                .RepositoryOwnedModelCheckpoint,
            startMetadata.Source);
        Assert.Null(startMetadata.ColdStart);
        Assert.Equal(stepTwo.ManifestSha256,
            startMetadata.ResumeParentCheckpoint!.ManifestSha256);

        Assert.True(File.Exists(stepThreeAddress.WeightsPath));
        Assert.True(File.Exists(stepThreeAddress.ManifestPath));
        var openedStepThree = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(stepThreeAddress);
        Assert.True(openedStepThree.IsSuccess, openedStepThree.ErrorMessage);
        var stepThree = openedStepThree.Value!.Manifest;
        Assert.Equal(3, stepThree.Step);
        Assert.EndsWith(
            "checkpoints/000003/manifest.json",
            stepThree.RepositoryRelativeManifestPath,
            StringComparison.Ordinal);
        var parentIdentity = Assert.IsType<
            RepositoryOwnedModelCheckpointParentIdentity>(
            stepThree.ParentCheckpoint);
        Assert.Equal(2, parentIdentity.Step);
        Assert.EndsWith(
            "checkpoints/000002/manifest.json",
            parentIdentity.RepositoryRelativeManifestPath,
            StringComparison.Ordinal);
        Assert.Equal(
            stepTwo.RepositoryRelativeManifestPath,
            parentIdentity.RepositoryRelativeManifestPath);
        Assert.Equal(stepTwo.WeightsSha256, parentIdentity.CheckpointSha256);
        Assert.Equal(
            stepTwo.WeightsByteLength,
            parentIdentity.CheckpointByteLength);
        Assert.Equal(currentDpo.Manifest, stepThree.DpoDatasetManifest);
        Assert.EndsWith(
            "training-data/dpo/bug/incremental-trainer-dpo-wiring/v0003/manifest.json",
            stepThree.DpoDatasetManifest.RepositoryRelativeManifestPath,
            StringComparison.Ordinal);
        Assert.NotEqual(
            first.ParentDpoDatasetManifest,
            stepThree.DpoDatasetManifest);
        Assert.NotEqual(stepTwo.DpoDatasetManifest, stepThree.DpoDatasetManifest);
        var stepThreeWeights = File.ReadAllBytes(stepThreeAddress.WeightsPath);
        Assert.Equal(stepThree.WeightsSha256, Sha256Hex(stepThreeWeights));
        Assert.NotEqual(stepTwoWeights, stepThreeWeights);
    }

    [Fact(Timeout = 180000)]
    public async Task RepositoryOwnedResumedTraining_VstestChildProcessReloadsPublished000002AndPublishes000003()
    {
        if (string.Equals(
                Environment.GetEnvironmentVariable(
                    RepositoryOwnedContinuationChildModeEnvironmentName),
                "1",
                StringComparison.Ordinal))
        {
            await RunRepositoryOwnedContinuationVstestChildAsync();
            return;
        }

        var explicitLoraConfig = CustomOneProjectionLoraConfig();

        using (var acceptedFixture = new TinyTrainingFixtureCL())
        {
            var published = await PublishRepositoryOwnedStepTwoAsync(
                acceptedFixture,
                "vstest-child-process",
                explicitLoraConfig);
            var baseCheckpointSha256BeforeChild =
                Sha256Hex(File.ReadAllBytes(acceptedFixture.BaseCheckpointPath));
            var outsideRepositoryBeforeChild =
                SnapshotFixtureEntriesOutsideArtifactRepository(
                    acceptedFixture.RootDirectory,
                    published.RepositoryRoot);
            var currentDpo = SealRepositoryOwnedDpoDataset(
                published.RepositoryRoot,
                "v0003");
            var stepThreeAddress = RequireSuccess(
                RepositoryOwnedTrainingArtifactLayout
                    .CreateModelCheckpointAddress(
                        published.RepositoryRoot,
                        RepositoryOwnedDomainSlug,
                        RepositoryOwnedDesignPatternSlug,
                        RepositoryOwnedModelIdSlug,
                        RepositoryOwnedModelVersion,
                        step: 3));
            var resultPath = Path.Combine(
                acceptedFixture.RootDirectory,
                "child-results",
                "accepted-continuation.json");

            // WHY: the same-process fresh-factory test proves a new object
            // graph, but not loss of managed memory. This child crosses a real
            // `dotnet vstest` process boundary, so checkpoint bytes, model
            // instances and delegates cannot cross except as sealed files plus
            // serializable path/configuration identity.
            using var child = StartRepositoryOwnedContinuationVstestChild(
                acceptedFixture,
                published.RepositoryRoot,
                resultPath,
                RepositoryOwnedContinuationAcceptedScenario,
                explicitLoraConfig);
            var childResult = await WaitForRepositoryOwnedContinuationChildAsync(
                child,
                resultPath,
                TimeSpan.FromSeconds(150));

            Assert.True(childResult.IsSuccess, childResult.ErrorMessage);
            Assert.Equal(
                RepositoryOwnedContinuationAcceptedScenario,
                childResult.Scenario);
            Assert.Equal(1, childResult.FactoryCreateCount);
            Assert.Equal(1, childResult.RepositoryLoadRequestCount);
            Assert.Equal(1, childResult.DpoExecutorCallCount);
            Assert.Equal(1, childResult.SupervisedCallCount);
            Assert.Equal(0, childResult.RetainedBaseOpenCount);
            Assert.True(childResult.RepositoryLoadRequestMatchedStartRequest);
            Assert.Equal(
                IncrementalTrainingCheckpointStartSource
                    .RepositoryOwnedModelCheckpoint.ToString(),
                childResult.CheckpointStartSource);
            Assert.Equal(
                published.StepTwoManifest.ManifestSha256,
                childResult.ResumeParentManifestSha256);
            Assert.Equal(2, childResult.LoadedParentStep);
            Assert.Equal(
                published.StepTwoManifest.ManifestSha256,
                childResult.LoadedParentManifestSha256);
            Assert.Equal(
                published.StepTwoManifest.WeightsSha256,
                childResult.LoadedParentWeightsSha256);
            Assert.Equal(
                published.StepTwoManifest.WeightsByteLength,
                childResult.LoadedParentWeightsByteLength);
            Assert.Equal(
                published.StepTwoManifest.WeightsSha256,
                childResult.CreatedBaseCheckpointSha256);
            Assert.Equal(
                published.StepTwoManifest.WeightsSha256,
                childResult.CreatedModelSha256);
            Assert.Equal(
                baseCheckpointSha256BeforeChild,
                Sha256Hex(File.ReadAllBytes(acceptedFixture.BaseCheckpointPath)));
            AssertNoUnexpectedOutOfRepositoryChildArtifacts(
                outsideRepositoryBeforeChild,
                acceptedFixture.RootDirectory,
                published.RepositoryRoot,
                resultPath);

            Assert.True(File.Exists(stepThreeAddress.WeightsPath));
            Assert.True(File.Exists(stepThreeAddress.ManifestPath));
            var openedStepThree = RepositoryOwnedModelCheckpointManifestSealer
                .OpenSealed(stepThreeAddress);
            Assert.True(openedStepThree.IsSuccess, openedStepThree.ErrorMessage);
            var stepThree = openedStepThree.Value!.Manifest;
            Assert.Equal(3, stepThree.Step);
            Assert.Equal(
                childResult.StepThreeManifestSha256,
                stepThree.ManifestSha256);
            Assert.EndsWith(
                "checkpoints/000003/manifest.json",
                stepThree.RepositoryRelativeManifestPath,
                StringComparison.Ordinal);
            var parentIdentity = Assert.IsType<
                RepositoryOwnedModelCheckpointParentIdentity>(
                stepThree.ParentCheckpoint);
            Assert.Equal(2, parentIdentity.Step);
            Assert.Equal(
                published.StepTwoManifest.RepositoryRelativeManifestPath,
                parentIdentity.RepositoryRelativeManifestPath);
            Assert.Equal(
                published.StepTwoManifest.WeightsSha256,
                parentIdentity.CheckpointSha256);
            Assert.Equal(
                published.StepTwoManifest.WeightsByteLength,
                parentIdentity.CheckpointByteLength);
            Assert.Equal(currentDpo.Manifest, stepThree.DpoDatasetManifest);
            Assert.Equal(
                currentDpo.Manifest.ManifestSha256,
                stepThree.DpoDatasetManifest.ManifestSha256);
            Assert.EndsWith(
                "training-data/dpo/bug/incremental-trainer-dpo-wiring/v0003/manifest.json",
                stepThree.DpoDatasetManifest.RepositoryRelativeManifestPath,
                StringComparison.Ordinal);
            var stepThreeWeights = File.ReadAllBytes(
                stepThreeAddress.WeightsPath);
            Assert.Equal(stepThree.WeightsSha256, Sha256Hex(stepThreeWeights));
            Assert.Equal(
                childResult.StepThreeWeightsSha256,
                stepThree.WeightsSha256);
            Assert.NotEqual(published.StepTwoWeights, stepThreeWeights);
        }

        using (var missingLoraFixture = new TinyTrainingFixtureCL())
        {
            var published = await PublishRepositoryOwnedStepTwoAsync(
                missingLoraFixture,
                "vstest-child-missing-lora",
                explicitLoraConfig);
            var baseCheckpointSha256BeforeChild =
                Sha256Hex(File.ReadAllBytes(missingLoraFixture.BaseCheckpointPath));
            var outsideRepositoryBeforeChild =
                SnapshotFixtureEntriesOutsideArtifactRepository(
                    missingLoraFixture.RootDirectory,
                    published.RepositoryRoot);
            _ = SealRepositoryOwnedDpoDataset(
                published.RepositoryRoot,
                "v0003");
            var stepThreeAddress = RequireSuccess(
                RepositoryOwnedTrainingArtifactLayout
                    .CreateModelCheckpointAddress(
                        published.RepositoryRoot,
                        RepositoryOwnedDomainSlug,
                        RepositoryOwnedDesignPatternSlug,
                        RepositoryOwnedModelIdSlug,
                        RepositoryOwnedModelVersion,
                        step: 3));
            var resultPath = Path.Combine(
                missingLoraFixture.RootDirectory,
                "child-results",
                "missing-lora-config.json");

            using var child = StartRepositoryOwnedContinuationVstestChild(
                missingLoraFixture,
                published.RepositoryRoot,
                resultPath,
                RepositoryOwnedContinuationMissingLoraScenario,
                loraConfig: null);
            var childResult = await WaitForRepositoryOwnedContinuationChildAsync(
                child,
                resultPath,
                TimeSpan.FromSeconds(150));

            Assert.False(childResult.IsSuccess);
            Assert.Equal(
                RepositoryOwnedContinuationMissingLoraScenario,
                childResult.Scenario);
            Assert.Equal("TRAINING_SESSION_CREATION_FAILED",
                childResult.ErrorCode);
            Assert.Equal(1, childResult.FactoryCreateCount);
            Assert.Equal(1, childResult.RepositoryLoadRequestCount);
            Assert.Equal(0, childResult.DpoExecutorCallCount);
            Assert.Equal(0, childResult.SupervisedCallCount);
            Assert.Equal(0, childResult.RetainedBaseOpenCount);
            Assert.True(childResult.RepositoryLoadRequestMatchedStartRequest);
            Assert.Equal(2, childResult.LoadedParentStep);
            Assert.Equal(
                published.StepTwoManifest.ManifestSha256,
                childResult.LoadedParentManifestSha256);
            Assert.Equal(
                published.StepTwoManifest.WeightsSha256,
                childResult.LoadedParentWeightsSha256);
            Assert.Null(childResult.CreatedBaseCheckpointSha256);
            Assert.Null(childResult.CreatedModelSha256);
            Assert.False(childResult.StepThreeExists);
            Assert.False(File.Exists(stepThreeAddress.WeightsPath));
            Assert.False(File.Exists(stepThreeAddress.ManifestPath));
            Assert.Equal(
                baseCheckpointSha256BeforeChild,
                Sha256Hex(File.ReadAllBytes(missingLoraFixture.BaseCheckpointPath)));
            AssertNoUnexpectedOutOfRepositoryChildArtifacts(
                outsideRepositoryBeforeChild,
                missingLoraFixture.RootDirectory,
                published.RepositoryRoot,
                resultPath);
        }
    }

    [Fact]
    public async Task RepositoryOwnedResumedTraining_AdapterBearingChildWithoutExplicitConfigDoesNotRetry()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var published = await PublishRepositoryOwnedStepTwoAsync(
            fixture,
            "no-explicit-retry-config");
        var currentDpo = SealRepositoryOwnedDpoDataset(
            published.RepositoryRoot,
            "v0003");
        var startRequest = CreateRepositoryOwnedContinuationRequest(
            published,
            currentDpo.Manifest);
        var repositoryLoadCount = 0;
        var retainedBaseOpenCount = 0;
        using var retainedAccess = CreateRetainedFileAccess(
            fixture,
            "no-explicit-retry-config");
        using var retainedFactory = RequireSuccess(
            RetainedCheckpointBackedIncrementalTrainingSessionFactory.Bind(
                retainedAccess,
                new NullSessionStorageCL(),
                fixture.ModelConfig,
                fixture.TrainingConfig,
                afterRetainedArtifactOpenForTesting: role =>
                {
                    if (role == ContinuousLearningRetainedArtifactRole
                            .BaseCheckpoint)
                    {
                        retainedBaseOpenCount++;
                    }
                },
                loadRepositoryCheckpoint: request =>
                {
                    repositoryLoadCount++;
                    return new RepositoryOwnedModelCheckpointLoader()
                        .LoadLatestCompatible(
                            request.RepositoryRoot,
                            request.DomainSlug,
                            request.DesignPatternSlug,
                            request.ModelIdSlug,
                            request.ModelVersion,
                            request.RuntimeCompatibility);
                },
                resolveRepositoryStartRequest: domain =>
                {
                    Assert.Equal(RepositoryOwnedDomainSlug, domain);
                    return OperationResult<
                        RepositoryOwnedIncrementalTrainingSessionStartRequest>
                        .Ok(startRequest);
                }));

        var result = retainedFactory.Create(RepositoryOwnedDomainSlug);

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAINING_SESSION_CREATION_FAILED", result.ErrorCode);
        Assert.Null(result.Value);
        Assert.Equal(1, repositoryLoadCount);
        Assert.Equal(0, retainedBaseOpenCount);
    }

    [Fact]
    public async Task RepositoryOwnedResumedTraining_CustomExplicitConfigReloadsSameAdapterTopology()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var explicitConfig = CustomOneProjectionLoraConfig();
        var published = await PublishRepositoryOwnedStepTwoAsync(
            fixture,
            "custom-explicit-retry-config",
            explicitConfig);
        var currentDpo = SealRepositoryOwnedDpoDataset(
            published.RepositoryRoot,
            "v0003");
        var startRequest = CreateRepositoryOwnedContinuationRequest(
            published,
            currentDpo.Manifest);
        var repositoryLoadCount = 0;
        var retainedBaseOpenCount = 0;
        using var retainedAccess = CreateRetainedFileAccess(
            fixture,
            "custom-explicit-retry-config");
        using var retainedFactory = RequireSuccess(
            RetainedCheckpointBackedIncrementalTrainingSessionFactory.Bind(
                retainedAccess,
                new NullSessionStorageCL(),
                fixture.ModelConfig,
                fixture.TrainingConfig,
                afterRetainedArtifactOpenForTesting: role =>
                {
                    if (role == ContinuousLearningRetainedArtifactRole
                            .BaseCheckpoint)
                    {
                        retainedBaseOpenCount++;
                    }
                },
                loadRepositoryCheckpoint: request =>
                {
                    repositoryLoadCount++;
                    return new RepositoryOwnedModelCheckpointLoader()
                        .LoadLatestCompatible(
                            request.RepositoryRoot,
                            request.DomainSlug,
                            request.DesignPatternSlug,
                            request.ModelIdSlug,
                            request.ModelVersion,
                            request.RuntimeCompatibility);
                },
                resolveRepositoryStartRequest: domain =>
                {
                    Assert.Equal(RepositoryOwnedDomainSlug, domain);
                    return OperationResult<
                        RepositoryOwnedIncrementalTrainingSessionStartRequest>
                        .Ok(startRequest);
                },
                repositoryCheckpointAdapterConfig: explicitConfig));

        var result = retainedFactory.Create(RepositoryOwnedDomainSlug);

        Assert.True(result.IsSuccess,
            $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, repositoryLoadCount);
        Assert.Equal(0, retainedBaseOpenCount);
        Assert.Equal(
            published.StepTwoManifest.WeightsSha256,
            result.Value!.BaseCheckpointSha256);
        using var reserialized = new MemoryStream();
        result.Value.Model.SaveCheckpointToStream(reserialized);
        Assert.Equal(published.StepTwoWeights, reserialized.ToArray());
    }

    [Fact]
    public async Task RepositoryOwnedResumedTraining_PublicTrainAsyncRequiresTransactionBeforeTrainingAndChildPublication()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var setup = CreateRepositoryOwnedResumedTrainingSetup(
            fixture,
            "public-nontransactional");
        var dpoExecutor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            setup.Factory,
            CreateAuthority(),
            dpoExecutor,
            supervised,
            fixture.AdapterDirectory);

        Assert.False(File.Exists(setup.ChildAddress.WeightsPath));
        Assert.False(File.Exists(setup.ChildAddress.ManifestPath));

        var result = await trainer.TrainAsync(
            RepositoryOwnedDomainSlug,
            [fixture.Example(
                RepositoryOwnedDomainSlug,
                "p",
                "r")],
            [],
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_TRANSACTION_REQUIRED", result.ErrorCode);
        Assert.Contains(
            "repository-owned training session requires a durable DPO transaction",
            result.ErrorMessage,
            StringComparison.Ordinal);
        Assert.Equal(1, setup.FactoryCreateCount());
        // WHY: the public TrainAsync path can materialize the resumed session,
        // but it must fail before any DPO/supervised mutation or filesystem
        // publication because it cannot call CompleteCandidate.
        Assert.Equal(0, dpoExecutor.CallCount);
        Assert.Equal(0, supervised.CallCount);
        Assert.False(File.Exists(setup.ChildAddress.WeightsPath));
        Assert.False(File.Exists(setup.ChildAddress.ManifestPath));
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterDirectory,
            $"{RepositoryOwnedDomainSlug}.lora")));
    }

    [Fact]
    public async Task RepositoryOwnedCurrentDpoDatasetPairMismatch_FailsBeforeTrainingAndChildPublication()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var setup = CreateRepositoryOwnedResumedTrainingSetup(
            fixture,
            "current-pair-mismatch");
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "repository-child-current-pair-mismatch",
            domain: RepositoryOwnedDomainSlug,
            uniqueStoreSuffix: "repository-child-current-pair-mismatch");
        var dpoExecutor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            setup.Factory,
            CreateAuthority(),
            dpoExecutor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                RepositoryOwnedDomainSlug,
                [fixture.Example(
                    RepositoryOwnedDomainSlug,
                    "p",
                    "r")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedIncrementalTrainingCheckpointPublisher
                .InvalidErrorCode,
            result.ErrorCode);
        Assert.Contains(
            "does not match the accepted DPO pairs",
            result.ErrorMessage,
            StringComparison.Ordinal);
        Assert.Equal(1, setup.FactoryCreateCount());
        Assert.Equal(0, dpoExecutor.CallCount);
        Assert.Equal(0, supervised.CallCount);
        Assert.False(File.Exists(setup.ChildAddress.WeightsPath));
        Assert.False(File.Exists(setup.ChildAddress.ManifestPath));
    }

    [Fact]
    public async Task RepositoryOwnedResumedTraining_WithoutSealedCurrentDpoDatasetDoesNotPublishChild()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var setup = CreateRepositoryOwnedResumedTrainingSetup(
            fixture,
            "missing-current",
            sealCurrentDpoDataset: false);
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "repository-child-missing-current",
            domain: RepositoryOwnedDomainSlug,
            uniqueStoreSuffix: "repository-child-missing-current");
        var dpoExecutor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            setup.Factory,
            CreateAuthority(),
            dpoExecutor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                RepositoryOwnedDomainSlug,
                [fixture.Example(
                    RepositoryOwnedDomainSlug,
                    "p",
                    "r")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedIncrementalTrainingCheckpointPublisher
                .InvalidErrorCode,
            result.ErrorCode);
        Assert.Contains(
            "sealed repository-owned DPO dataset for the current update",
            result.ErrorMessage,
            StringComparison.Ordinal);
        Assert.Equal(1, setup.FactoryCreateCount());
        Assert.Equal(0, dpoExecutor.CallCount);
        Assert.Equal(0, supervised.CallCount);
        Assert.False(File.Exists(setup.ChildAddress.WeightsPath));
        Assert.False(File.Exists(setup.ChildAddress.ManifestPath));
    }

    [Fact]
    public void RepositoryOwnedIncrementalTrainingSessionStartRequest_RetainsSevenArgumentConstructionAndDeconstruction()
    {
        var runtimeCompatibility = RepositoryOwnedRuntimeCompatibility();
        var coldStartPolicy =
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied;
        var request = new RepositoryOwnedIncrementalTrainingSessionStartRequest(
            "repository-root",
            "domain",
            "design-pattern",
            "model-id",
            "model-version",
            runtimeCompatibility,
            coldStartPolicy);

        var (
            repositoryRoot,
            domainSlug,
            designPatternSlug,
            modelIdSlug,
            modelVersion,
            deconstructedRuntimeCompatibility,
            deconstructedColdStartPolicy) = request;

        Assert.Null(request.CurrentDpoDatasetManifest);
        Assert.Equal("repository-root", repositoryRoot);
        Assert.Equal("domain", domainSlug);
        Assert.Equal("design-pattern", designPatternSlug);
        Assert.Equal("model-id", modelIdSlug);
        Assert.Equal("model-version", modelVersion);
        Assert.Equal(
            runtimeCompatibility,
            deconstructedRuntimeCompatibility);
        Assert.Equal(coldStartPolicy, deconstructedColdStartPolicy);

        var publicConstructors = typeof(
                RepositoryOwnedIncrementalTrainingSessionStartRequest)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        Assert.Contains(
            publicConstructors,
            constructor => constructor.GetParameters().Length == 7);
        Assert.Contains(
            typeof(RepositoryOwnedIncrementalTrainingSessionStartRequest)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance),
            method => StringComparer.Ordinal.Equals(
                          method.Name,
                          nameof(
                              RepositoryOwnedIncrementalTrainingSessionStartRequest
                                  .Deconstruct))
                      && method.GetParameters().Length == 7);
    }

    [Fact]
    public async Task RevocationBeforeSessionFactoryPreventsSessionAndFilesystemMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "revoke-before-session",
            uniqueStoreSuffix: "revoke-before-session");
        var factory = new RecordingDpoSessionFactory(fixture.CreateFactory());
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var authority = CreateAuthorityRevokingAtLiveCheck(
            SessionMutationPermitLiveCheck);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            factory,
            authority,
            executor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
        Assert.Equal(0, factory.CreateCount);
        Assert.Equal(0, executor.CallCount);
        Assert.Equal(0, supervised.CallCount);
        Assert.False(Directory.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.dpo")));
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.lora")));
        var released = transaction.ReleaseIfUnmutated();
        Assert.True(released.IsSuccess, released.ErrorMessage);
        Assert.True(released.Value);
        var recoveredRouter = CreateDurableRouter(
            fixture,
            [],
            "revoke-before-session");
        var recovered = ((IDpoPairTransactionCoordinator)recoveredRouter)
            .PrepareDpoTransaction(["BUG"]);
        Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
        Assert.Single(recovered.Value!.Read().Value!.Pairs);
    }

    [Fact]
    public async Task RevocationAndCancellationAfterSessionCreatePreventsAllPreparationAndTrainingMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "revoke-and-cancel-after-session",
            uniqueStoreSuffix: "revoke-after-session");
        using var cancellation = new CancellationTokenSource();
        var authority = CreateAuthority();
        var factory = new RecordingDpoSessionFactory(
            fixture.CreateFactory(),
            afterCreate: () =>
            {
                _ = authority.BeginRevocation();
                cancellation.Cancel();
            });
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            factory,
            authority,
            executor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                cancellation.Token)
            .WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(1, factory.CreateCount);
        var createdSession = Assert.IsType<IncrementalTrainingSession>(
            factory.LastSession);
        Assert.Null(createdSession.Model.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.Null(createdSession.Model.GetAttachedProjectionAdapter(0, "v_proj"));
        Assert.Equal(0, executor.CallCount);
        Assert.Equal(0, supervised.CallCount);
        Assert.False(Directory.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.dpo")));
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.lora")));
    }

    [Fact]
    public async Task RevocationBeforeDpoDirectoryCreationLeavesNoDpoOrRunDirectory()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "revoke-before-dpo-directory",
            uniqueStoreSuffix: "revoke-dpo-directory");
        var factory = new RecordingDpoSessionFactory(fixture.CreateFactory());
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var authority = CreateAuthorityRevokingAtLiveCheck(
            SingleExampleDpoDirectoryPermitLiveCheck);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            factory,
            authority,
            executor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
        Assert.Equal(1, factory.CreateCount);
        Assert.Equal(0, executor.CallCount);
        var domainDpoDirectory = Path.Combine(
            fixture.AdapterDirectory,
            "BUG.dpo");
        Assert.False(Directory.Exists(domainDpoDirectory));
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.lora")));
    }

    [Fact]
    public async Task RevocationAndCancellationAfterDpoMutationPreventsSupervisedAndFinalAdapterMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "revoke-and-cancel-after-dpo",
            uniqueStoreSuffix: "revoke-after-dpo");
        using var cancellation = new CancellationTokenSource();
        var authority = CreateAuthority();
        var executor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success,
            afterMutation: () =>
            {
                _ = authority.BeginRevocation();
                cancellation.Cancel();
            });
        var supervised = new MutatingSupervisedStepExecutor();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            authority,
            executor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                cancellation.Token)
            .WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(1, executor.CallCount);
        Assert.Equal(0, supervised.CallCount);
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.lora")));
    }

    [Fact]
    public async Task RevocationBeforeSupervisedUpdateAllowsNoLaterAdapterMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "revoke-before-supervised",
            uniqueStoreSuffix: "revoke-supervised");
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var authority = CreateAuthorityRevokingAtLiveCheck(
            SingleExampleSupervisedPermitLiveCheck);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            authority,
            executor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
        Assert.Equal(1, executor.CallCount);
        Assert.Equal(0, supervised.CallCount);
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.lora")));
    }

    [Fact]
    public async Task RevocationBeforeFinalAdapterTransactionCreatesNoAdapterFile()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "revoke-before-final-adapter",
            uniqueStoreSuffix: "revoke-final-adapter");
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var authority = CreateAuthorityRevokingAtLiveCheck(
            SingleExampleFinalAdapterPermitLiveCheck);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            authority,
            executor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
        Assert.Equal(1, executor.CallCount);
        Assert.Equal(0, supervised.CallCount);
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.lora")));
    }

    [Fact]
    public async Task RevocationBeatsCancellationAtFinalAdapterBoundary()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "revoke-and-cancel-final",
            uniqueStoreSuffix: "revoke-cancel-final");
        using var cancellation = new CancellationTokenSource();
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var authority = CreateAuthorityRevokingAtLiveCheck(
            SingleExampleFinalAdapterPermitLiveCheck,
            cancellation.Cancel);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            authority,
            executor,
            supervised,
            fixture.AdapterDirectory);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                cancellation.Token);

        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.False(File.Exists(Path.Combine(
            fixture.AdapterDirectory,
            "BUG.lora")));
    }

    [Fact]
    public async Task DpoExecutionsUseFreshPerRunDirectoriesAndPreserveEarlierArtifactPairs()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            CreateAuthority(),
            executor,
            new MutatingSupervisedStepExecutor(),
            fixture.AdapterDirectory);

        var (firstTransaction, firstPairs) = PrepareDurableTransaction(
            fixture,
            "first-run",
            uniqueStoreSuffix: "first-run");
        var first = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "first", "response")],
                firstPairs,
                firstTransaction,
                CancellationToken.None);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        var firstRequest = Assert.Single(executor.Requests);
        var firstPolicyPath = Path.Combine(
            firstRequest.CheckpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(firstRequest.DpoStepCount));
        var firstMetricsPath = Path.Combine(
            firstRequest.CheckpointDirectory,
            DpoTrainer.BuildMetricsFileName(firstRequest.DpoStepCount));
        var firstPolicyBytes = File.ReadAllBytes(firstPolicyPath);
        var firstMetricsBytes = File.ReadAllBytes(firstMetricsPath);

        var (secondTransaction, secondPairs) = PrepareDurableTransaction(
            fixture,
            "second-run",
            uniqueStoreSuffix: "second-run");
        var second = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "second", "response")],
                secondPairs,
                secondTransaction,
                CancellationToken.None);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        var secondRequest = executor.Requests[1];

        var restartedExecutor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        var restartedTrainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            CreateAuthority(),
            restartedExecutor,
            new MutatingSupervisedStepExecutor(),
            fixture.AdapterDirectory);
        var (thirdTransaction, thirdPairs) = PrepareDurableTransaction(
            fixture,
            "restarted-run",
            uniqueStoreSuffix: "third-run");
        var third = await ((ITransactionalIncrementalTrainer)restartedTrainer)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "third", "response")],
                thirdPairs,
                thirdTransaction,
                CancellationToken.None);
        Assert.True(third.IsSuccess, third.ErrorMessage);
        var thirdRequest = Assert.Single(restartedExecutor.Requests);

        var runDirectories = new[]
        {
            firstRequest.CheckpointDirectory,
            secondRequest.CheckpointDirectory,
            thirdRequest.CheckpointDirectory,
        };
        Assert.Equal(3, runDirectories.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        var domainRoot = Path.GetFullPath(Path.Combine(fixture.AdapterDirectory, "BUG.dpo"));
        Assert.All(runDirectories, runDirectory =>
        {
            Assert.Equal(domainRoot, Path.GetDirectoryName(runDirectory));
            Assert.StartsWith("run-", Path.GetFileName(runDirectory), StringComparison.Ordinal);
            Assert.True(File.Exists(Path.Combine(
                runDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(
                    StableDpoTrainingExecutor.DefaultDpoStepCount))));
            Assert.True(File.Exists(Path.Combine(
                runDirectory,
                DpoTrainer.BuildMetricsFileName(
                    StableDpoTrainingExecutor.DefaultDpoStepCount))));
        });
        Assert.Equal(firstPolicyBytes, File.ReadAllBytes(firstPolicyPath));
        Assert.Equal(firstMetricsBytes, File.ReadAllBytes(firstMetricsPath));
    }

    [Fact]
    public async Task DpoDomainReparsePointIsRejectedBeforeExecutorInvocation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var outsideDirectory = Path.Combine(
            Path.GetTempPath(),
            $"dpo-checkpoint-reparse-outside-{Guid.NewGuid():N}");
        var domainDirectory = Path.Combine(fixture.AdapterDirectory, "BUG.dpo");
        var verified = false;
        try
        {
            Directory.CreateDirectory(outsideDirectory);
            CreateDirectoryReparseLink(domainDirectory, outsideDirectory);

            var (transaction, pairs) = PrepareDurableTransaction(
                fixture,
                "dpo-domain-reparse",
                uniqueStoreSuffix: "reparse");
            var executor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
            var trainer = new IncrementalTrainer(
                new NullSessionStorageCL(),
                fixture.CreateFactory(),
                CreateAuthority(),
                executor,
                new MutatingSupervisedStepExecutor(),
                fixture.AdapterDirectory);

            var result = await ((ITransactionalIncrementalTrainer)trainer)
                .TrainTransactionAsync(
                    "BUG",
                    [fixture.Example("BUG", "prompt", "response")],
                    pairs,
                    transaction,
                    CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal("DPO_EXECUTION_ARTIFACT_INCOMPLETE", result.ErrorCode);
            Assert.Equal(0, executor.CallCount);
            Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));
            Assert.Empty(Directory.EnumerateFileSystemEntries(outsideDirectory));
            verified = true;
        }
        finally
        {
            if (Directory.Exists(domainDirectory))
                Directory.Delete(domainDirectory);
            if (verified && Directory.Exists(outsideDirectory))
                Directory.Delete(outsideDirectory, recursive: true);
        }
    }

    private static void CreateDirectoryReparseLink(string linkPath, string targetPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
            return;
        }

        using var process = System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                ArgumentList =
                {
                    "/d",
                    "/c",
                    "mklink",
                    "/J",
                    linkPath,
                    targetPath,
                },
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }) ?? throw new InvalidOperationException("Could not start the junction creation process.");
        var exited = process.WaitForExit(10_000);
        if (!exited)
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit();
        }
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        Assert.True(
            exited && process.ExitCode == 0,
            $"Could not create a Windows junction. stdout={standardOutput}; stderr={standardError}");
        Assert.True(
            (File.GetAttributes(linkPath) & FileAttributes.ReparsePoint) != 0,
            "The created DPO directory junction must be observable as a reparse point.");
    }

    [Fact]
    public void StableDpoAdapter_PureMappingPreservesOwnedRequestWithoutExecution()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var session = fixture.CreateFactory().Create("BUG").Value!;
        var reference = new LlmModel(session.Model.Config);
        var pair = Pair(QwenProvenance("mapping"));
        var authority = CreateAuthority();
        var lease = authority.AcquireTrainingLease(
            [pair.TeacherProvenance]).Value!;
        var request = new DpoTrainingExecutionRequest(
            "BUG",
            [pair],
            session.Model,
            reference,
            session.Tokenizer,
            IncrementalTrainer.DefaultConfig,
            Path.Combine(fixture.RootDirectory, "never-executed"),
            lease,
            DpoStepCount: 2);

        var mapped = StableDpoTrainingExecutor.BuildLogicalRequest(request);

        Assert.Same(session.Model, mapped.Input.PolicyModel);
        Assert.Same(reference, mapped.Input.ReferenceModel);
        Assert.NotSame(mapped.Input.PolicyModel, mapped.Input.ReferenceModel);
        Assert.Same(session.Tokenizer, mapped.Input.Tokenizer);
        Assert.Same(request.LoraConfig, mapped.Input.LoraConfig);
        Assert.Equal(2, mapped.Input.MaxStepCount);
        var preference = Assert.Single(mapped.Input.Preferences);
        Assert.Equal((pair.Prompt, pair.Chosen, pair.Rejected),
            (preference.Prompt, preference.Chosen, preference.Rejected));
        Assert.Equal(pair.ChosenAuthorship!.AuthorKind,
            preference.ChosenAuthorKind);
        Assert.Equal(pair.RejectedAuthorship!.AuthorKind,
            preference.RejectedAuthorKind);
        Assert.Equal(
            lease.Admission.PolicyIdentityRef,
            mapped.Metadata["legal_teacher_policy_ref"]);
        Assert.NotNull(mapped.Input.MutationAdmission);
        Assert.NotNull(mapped.Input.PreparedExecution);
        var preparedInput = Assert.Single(
            mapped.Input.PreparedExecution!.OptimizerInputs);
        Assert.Equal((pair.Prompt, pair.Chosen, pair.Rejected),
            (preparedInput.Prompt, preparedInput.Chosen,
                preparedInput.Rejected));
        var admission = Assert.IsType<DpoTrainerPairAdmission>(
            preparedInput.Batch.Batch.Admission);
        Assert.Equal(pair.ChosenAuthorship.AuthorKind,
            admission.ChosenAuthor);
        Assert.Equal(pair.RejectedAuthorship.AuthorKind,
            admission.RejectedAuthor);
        Assert.Equal(request.CheckpointDirectory, mapped.Input.CheckpointDirectory);
    }

    [Fact]
    public async Task StableDpoExecutor_ProductionRequestAndMutationSinkSharePreparedInputs()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var request = CreateExecutionRequest(
            fixture,
            dpoMaxStepCount: 1,
            requestId: "stable-prepared-execution");
        LogicalDpoTrainingRequest? observedRequest = null;
        IReadOnlyList<LogicalDpoPreparedOptimizerInput>? observedInputs = null;
        IReadOnlyList<DpoVectorBatch>? observedBatches = null;
        var priorRequestObserver = StableDpoTrainingExecutor
            .LogicalRequestObserverForTesting;
        var priorSinkObserver = LogicalDpoTrainingUnit
            .OptimizerMutationSinkObserverForTesting;

        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                value => observedRequest = value;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                (inputs, batches) =>
                {
                    observedInputs = inputs;
                    observedBatches = batches;
                };

            var result = await new StableDpoTrainingExecutor()
                .ExecuteAsync(request);

            Assert.True(result.IsSuccess,
                $"{result.ErrorCode}: {result.ErrorMessage}");
            Assert.NotNull(observedRequest?.PreparedExecution);
            var preparedInput = Assert.Single(
                observedRequest!.PreparedExecution!.OptimizerInputs);
            Assert.Same(preparedInput, Assert.Single(observedInputs!));
            Assert.Same(preparedInput.Batch, Assert.Single(observedBatches!));
            var preference = Assert.Single(observedRequest.Preferences);
            var sourcePair = Assert.Single(request.Pairs);
            Assert.Equal(sourcePair.ChosenAuthorship!.AuthorKind,
                preference.ChosenAuthorKind);
            Assert.Equal(sourcePair.RejectedAuthorship!.AuthorKind,
                preference.RejectedAuthorKind);
            var admission = Assert.IsType<DpoTrainerPairAdmission>(
                preparedInput.Batch.Batch.Admission);
            Assert.Equal(sourcePair.ChosenAuthorship.AuthorKind,
                admission.ChosenAuthor);
            Assert.Equal(sourcePair.RejectedAuthorship.AuthorKind,
                admission.RejectedAuthor);
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                priorRequestObserver;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                priorSinkObserver;
        }
    }

    [Fact]
    public async Task StableDpoExecutor_V4AuthorlessPairStopsBeforeLogicalRequestOrOptimizer()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var session = fixture.CreateFactory().Create("BUG").Value!;
        var richPair = Pair(QwenProvenance("v4-authorless"));
        var legacyPair = richPair with
        {
            ChosenAuthorship = null,
            RejectedAuthorship = null
        };
        var content = DpoPairTrainingContentAuthority.Project([legacyPair])
            .Value!;
        var lease = CreateAuthority().AcquireTrainingLease(
            content,
            [legacyPair.TeacherProvenance]).Value!;
        var request = new DpoTrainingExecutionRequest(
            "BUG",
            [legacyPair],
            session.Model,
            new LlmModel(session.Model.Config),
            session.Tokenizer,
            IncrementalTrainer.DefaultConfig,
            Path.Combine(fixture.RootDirectory, "dpo-v4-authorless"),
            lease,
            DpoStepCount: 1);
        var logicalRequestObserved = false;
        var optimizerObserved = false;
        var priorRequestObserver = StableDpoTrainingExecutor
            .LogicalRequestObserverForTesting;
        var priorSinkObserver = LogicalDpoTrainingUnit
            .OptimizerMutationSinkObserverForTesting;

        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                _ => logicalRequestObserved = true;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                (_, _) => optimizerObserved = true;

            var result = await new StableDpoTrainingExecutor()
                .ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                LogicalDpoTrainingUnit.PreferenceBoundsInvalidErrorCode,
                result.ErrorCode);
            Assert.False(logicalRequestObserved);
            Assert.False(optimizerObserved);
            Assert.False(Directory.Exists(request.CheckpointDirectory));
        }
        finally
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting =
                priorRequestObserver;
            LogicalDpoTrainingUnit.OptimizerMutationSinkObserverForTesting =
                priorSinkObserver;
        }
    }

    [Fact]
    public async Task StableDpoExecutor_EarlyActualStepUsesActualArtifactNames()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var logicalUnit = new FixedStepLogicalDpoTrainingUnit(actualDpoStepCount: 1);
        var executor = new StableDpoTrainingExecutor(logicalUnit);
        var request = CreateExecutionRequest(
            fixture,
            dpoMaxStepCount: 2,
            requestId: "stable-early-convergence");

        var result = await executor.ExecuteAsync(request);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(2, logicalUnit.LastRequest!.Input.MaxStepCount);
        Assert.Equal(1, result.Value!.DpoStepCount);
        Assert.Equal(
            DpoTrainer.BuildPolicyCheckpointFileName(1),
            Path.GetFileName(result.Value.PolicyCheckpointPath));
        Assert.Equal(
            DpoTrainer.BuildMetricsFileName(1),
            Path.GetFileName(result.Value.MetricsPath));
    }

    [Fact]
    public async Task StableDpoExecutor_RevocationAndCancellationAfterMutationPreservesExactRevokedError()
    {
        using var fixture = new TinyTrainingFixtureCL();
        using var cancellation = new CancellationTokenSource();
        var authority = CreateAuthority();
        var logicalUnit = new FixedStepLogicalDpoTrainingUnit(
            actualDpoStepCount: 1,
            afterMutation: () =>
            {
                _ = authority.BeginRevocation();
                cancellation.Cancel();
            });
        var executor = new StableDpoTrainingExecutor(logicalUnit);
        var request = CreateExecutionRequest(
            fixture,
            dpoMaxStepCount: 1,
            requestId: "stable-final-revocation",
            authority);

        var result = await executor.ExecuteAsync(
                request,
                cancellation.Token)
            .WaitAsync(TimeSpan.FromSeconds(10));

        Assert.False(result.IsSuccess);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task StableDpoExecutor_RejectsActualStepOutsideConfiguredMaximum(
        int actualDpoStepCount)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var executor = new StableDpoTrainingExecutor(
            new FixedStepLogicalDpoTrainingUnit(actualDpoStepCount));
        var request = CreateExecutionRequest(
            fixture,
            dpoMaxStepCount: 2,
            requestId: $"stable-invalid-actual-{actualDpoStepCount}");

        var result = await executor.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("DPO_EXECUTION_INCOMPLETE", result.ErrorCode);
    }

    [Fact]
    public async Task ProductionTp1Composition_ConfiguredStepTwoPublishesStepTwoDpoArtifacts()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var (transaction, pairs) = PrepareDurableTransaction(
            fixture,
            "production-step-two",
            uniqueStoreSuffix: "production-step-two");
        var configuration = CreateTp1Configuration(dpoMaxStepCount: 2);
        var composition = ContinuousLearningProductionComposition.CreateTp1Trainer(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .IssueFromAdmittedConfiguration(
                    configuration,
                    static () => true),
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            configuration,
            fixture.AdapterDirectory,
            loraConfig: IncrementalTrainer.DefaultConfig with { Dropout = 0f });
        Assert.True(composition.IsSuccess, composition.ErrorMessage);

        var result = await ((ITransactionalIncrementalTrainer)composition.Value!)
            .TrainTransactionAsync(
                "BUG",
                [fixture.Example("BUG", "prompt", "response")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var dpoRunDirectory = Assert.Single(Directory.GetDirectories(
            Path.Combine(fixture.AdapterDirectory, "BUG.dpo")));
        Assert.True(File.Exists(Path.Combine(
            dpoRunDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));
        Assert.True(File.Exists(Path.Combine(
            dpoRunDirectory,
            DpoTrainer.BuildMetricsFileName(1))));
        Assert.True(File.Exists(Path.Combine(
            dpoRunDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(2))));
        Assert.True(File.Exists(Path.Combine(
            dpoRunDirectory,
            DpoTrainer.BuildMetricsFileName(2))));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));
        var snapshot = transaction.Read();
        Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
        Assert.Equal(
            FileDpoPairStore.DpoTransactionDomainState.CandidateReady,
            Assert.Single(snapshot.Value!.Domains).State);
    }

    [Fact]
    public void StableLogicalDpoCatchBoundary_AllFourCatchesCompileAsHelperFilters()
    {
        Assert.True(LogicalDpoExceptionBoundary.ShouldNormalize(
            new InvalidOperationException("ordinary")));
        Assert.False(LogicalDpoExceptionBoundary.ShouldNormalize(
            new OperationCanceledException("cancel")));
        Assert.False(LogicalDpoExceptionBoundary.ShouldNormalize(
            new OutOfMemoryException("fatal")));
        Assert.False(LogicalDpoExceptionBoundary.ShouldNormalize(
            new AggregateException(new InvalidOperationException(), new OperationCanceledException())));

        AssertCompiledHelperFilters(
            "ExecuteWithExclusivePolicyLeaseAsync",
            // Seven current normalization boundaries include the seeded/RNG
            // preparation branches; the async wrapper itself has no filters.
            expectedFilterCount: 7);
        AssertCompiledHelperFilters(
            "OpenAndValidateParentArtifacts",
            expectedFilterCount: 1);
    }

    private static void AssertCompiledHelperFilters(
        string methodName,
        int expectedFilterCount)
    {
        var method = typeof(LogicalDpoTrainingUnit).GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var helper = typeof(LogicalDpoExceptionBoundary).GetMethod(
            nameof(LogicalDpoExceptionBoundary.ShouldNormalize),
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(helper);
        // Inspect the methods that execute the catches, including emitted
        // async MoveNext and the named local ExecuteCore, rather than the stub.
        var executableMethods = new List<MethodInfo> { method! };
        var stateMachine = method!.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
        if (stateMachine is not null)
            executableMethods.Add(stateMachine.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic)!);
        executableMethods.AddRange(DescendantTypes(typeof(LogicalDpoTrainingUnit))
            .Prepend(typeof(LogicalDpoTrainingUnit))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Where(candidate => candidate.Name.Contains($"<{methodName}>", StringComparison.Ordinal) &&
                candidate.Name.Contains("ExecuteCore", StringComparison.Ordinal)));
        var filters = executableMethods.Distinct()
            .SelectMany(candidate => candidate.GetMethodBody()!.ExceptionHandlingClauses
                .Where(clause => clause.Flags == ExceptionHandlingClauseOptions.Filter)
                .Select(clause => (Method: candidate, Clause: clause)))
            .ToArray();

        Assert.Equal(expectedFilterCount, filters.Length);
        Assert.All(filters, entry => Assert.True(
            ContainsResolvedBoundaryCall(
                entry.Method,
                entry.Method.GetMethodBody()!.GetILAsByteArray()!,
                entry.Clause.FilterOffset,
                entry.Clause.HandlerOffset,
                helper!),
            $"Filter IL in {methodName} does not call {helper!.Name}."));
    }

    private static IEnumerable<Type> DescendantTypes(Type root)
    {
        foreach (var child in root.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            yield return child;
            foreach (var descendant in DescendantTypes(child)) yield return descendant;
        }
    }

    private static bool ContainsResolvedBoundaryCall(
        MethodInfo containingMethod,
        byte[] il,
        int startOffset,
        int endOffset,
        MethodInfo expectedBoundary)
    {
        var offset = startOffset;
        while (offset < endOffset)
        {
            var opCode = ReadOpCode(il, ref offset, endOffset);
            var operandOffset = offset;
            var operandSize = GetOperandSize(
                opCode.OperandType,
                il,
                operandOffset,
                endOffset);
            var operandEnd = checked(operandOffset + operandSize);
            Assert.True(
                operandEnd <= endOffset && operandEnd <= il.Length,
                $"Malformed IL operand for {opCode.Name} at {operandOffset}.");

            if ((opCode == OpCodes.Call || opCode == OpCodes.Callvirt)
                && operandSize == sizeof(int))
            {
                var token = BitConverter.ToInt32(il, operandOffset);
                var resolved = containingMethod.Module.ResolveMethod(
                    token,
                    containingMethod.DeclaringType?.GetGenericArguments(),
                    containingMethod.GetGenericArguments());
                if (Equals(resolved, expectedBoundary))
                    return true;
            }

            offset = operandEnd;
        }

        Assert.Equal(endOffset, offset);
        return false;
    }

    private static OpCode ReadOpCode(
        byte[] il,
        ref int offset,
        int endOffset)
    {
        Assert.True(
            offset < endOffset && offset < il.Length,
            $"Missing IL opcode at {offset}.");
        ushort value = il[offset++];
        if (value == 0xFE)
        {
            Assert.True(
                offset < endOffset && offset < il.Length,
                $"Truncated two-byte IL opcode at {offset - 1}.");
            value = (ushort)(0xFE00 | il[offset++]);
        }

        Assert.True(IlOpCodes.TryGetValue(value, out var opCode),
            $"Unknown IL opcode 0x{value:X4} at {offset - 1}.");
        return opCode;
    }

    private static int GetOperandSize(
        OperandType operandType,
        byte[] il,
        int operandOffset,
        int endOffset)
        => operandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or
                OperandType.ShortInlineI or
                OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineBrTarget or
                OperandType.InlineField or
                OperandType.InlineI or
                OperandType.InlineMethod or
                OperandType.InlineSig or
                OperandType.InlineString or
                OperandType.InlineTok or
                OperandType.InlineType or
                OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => GetInlineSwitchOperandSize(
                il,
                operandOffset,
                endOffset),
            _ => throw new InvalidOperationException(
                $"Unsupported IL operand type {operandType}."),
        };

    private static int GetInlineSwitchOperandSize(
        byte[] il,
        int operandOffset,
        int endOffset)
    {
        Assert.True(
            operandOffset + sizeof(int) <= endOffset
            && operandOffset + sizeof(int) <= il.Length,
            $"Truncated InlineSwitch count at {operandOffset}.");
        var targetCount = BitConverter.ToInt32(il, operandOffset);
        Assert.True(targetCount >= 0, $"Negative InlineSwitch count at {operandOffset}.");
        return checked(sizeof(int) + (targetCount * sizeof(int)));
    }

    [Fact]
    public async Task Pipeline_MultiDomainRoutesEveryPairExactlyOnceWithoutDrop()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var router = CreateDurableRouter(
            fixture,
            [("BUG", "multi-bug"), ("CODE_Q", "multi-code")],
            "pipeline-multi");
        var trainer = new SuccessfulPipelineTrainer();
        var registry = new MemoryDpoRegistry();
        var pipeline = new ContinuousLearningPipeline(
            new NullSessionStorageCL(),
            new FixedDpoIndexer([
                fixture.Example("BUG", "bug prompt", "bug response"),
                fixture.Example("CODE_Q", "code prompt", "code response")]),
            new AcceptingDpoCache(),
            router,
            trainer,
            registry);

        var result = await pipeline.RunCycleAsync("project");

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(2, trainer.Calls.Count);
        Assert.Equal(
            "multi-bug",
            Assert.Single(trainer.Calls.Single(call => call.Domain == "BUG").Pairs)
                .TeacherProvenance!.RequestId);
        Assert.Equal(
            "multi-code",
            Assert.Single(trainer.Calls.Single(call => call.Domain == "CODE_Q").Pairs)
                .TeacherProvenance!.RequestId);
        Assert.Equal(2, result.Value!.Count);
        var pending = router.GetDpoPairs();
        Assert.True(pending.IsSuccess, pending.ErrorMessage);
        Assert.Empty(pending.Value!);
    }

    [Fact]
    public async Task Pipeline_UsesExactReservedSnapshotBeforeTrainerPartitioning()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var router = CreateDurableRouter(
            fixture,
            Enumerable.Range(0, 2)
                .Select(index => ("BUG", $"pipeline-overflow-{index}"))
                .ToArray(),
            "pipeline-overflow");
        var trainer = new PipelineRecordingTrainer();
        var pipeline = new ContinuousLearningPipeline(
            new NullSessionStorageCL(),
            new FixedDpoIndexer([Example()]),
            new AcceptingDpoCache(),
            router,
            trainer,
            new UnusedDpoRegistry());

        var result = await pipeline.RunCycleAsync("project");

        Assert.Equal("SYNTHETIC_TRAINING_STOP", result.ErrorCode);
        var call = Assert.Single(trainer.Calls);
        Assert.Equal(2, call.Pairs.Count);
    }

    [Fact]
    public async Task Pipeline_TransactionCapDrainsExactSnapshotAndLeavesRemainderReservable()
    {
        Assert.Equal(1024, FileDpoPairStore.MaximumTransactionPairCount);
        using var fixture = new TinyTrainingFixtureCL();
        var allRequestIds = Enumerable.Range(0, 3)
            .Select(index => $"pipeline-cap-{index}")
            .ToHashSet(StringComparer.Ordinal);
        var router = CreateDurableRouter(
            fixture,
            allRequestIds.Select(requestId => ("BUG", requestId)).ToArray(),
            "pipeline-cap-boundary",
            maximumTransactionPairCountForTesting: 2);
        var trainer = new SuccessfulPipelineTrainer();
        var registry = new MemoryDpoRegistry();
        var pipeline = new ContinuousLearningPipeline(
            new NullSessionStorageCL(),
            new FixedDpoIndexer([
                fixture.Example("BUG", "cap prompt", "cap response")]),
            new AcceptingDpoCache(),
            router,
            trainer,
            registry);

        var result = await pipeline.RunCycleAsync("project");

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var firstCall = Assert.Single(trainer.Calls);
        var firstRequestIds = firstCall.Pairs
            .Select(pair => pair.TeacherProvenance!.RequestId)
            .ToArray();
        Assert.Equal(2, firstRequestIds.Length);
        Assert.Equal(2, firstRequestIds.Distinct(StringComparer.Ordinal).Count());
        Assert.All(firstRequestIds, requestId => Assert.Contains(requestId, allRequestIds));

        var secondResult = await pipeline.RunCycleAsync("project");

        Assert.True(
            secondResult.IsSuccess,
            $"{secondResult.ErrorCode}: {secondResult.ErrorMessage}");
        Assert.Equal(2, trainer.Calls.Count);
        var secondCall = trainer.Calls[1];
        var remainingRequestId = Assert.Single(secondCall.Pairs)
            .TeacherProvenance!.RequestId;
        Assert.DoesNotContain(remainingRequestId, firstRequestIds);
        var observedRequestIds = firstRequestIds
            .Append(remainingRequestId)
            .ToArray();
        Assert.Equal(3, observedRequestIds.Distinct(StringComparer.Ordinal).Count());
        Assert.True(allRequestIds.SetEquals(observedRequestIds));
        var pending = router.GetDpoPairs();
        Assert.True(pending.IsSuccess, pending.ErrorMessage);
        Assert.Empty(pending.Value!);
    }

    private static TrainingExample Example()
        => new("prompt", "response", "BUG", 1d, DateTimeOffset.UtcNow);

    private static TrainingExample Example(string domain)
        => new($"prompt-{domain}", "response", domain, 1d, DateTimeOffset.UtcNow);

    private sealed record RepositoryOwnedDpoDatasetFixture(
        RepositoryOwnedDpoDatasetAddress Address,
        RepositoryOwnedDpoDatasetManifestIdentity Manifest);

    private sealed record PublishedRepositoryOwnedStepTwo(
        string RepositoryRoot,
        RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
        RepositoryOwnedModelCheckpointAddress StepTwoAddress,
        RepositoryOwnedModelCheckpointManifest StepTwoManifest,
        byte[] StepTwoWeights);

    private sealed record RepositoryOwnedContinuationVstestChildResult(
        bool IsSuccess,
        string Scenario,
        string? ErrorCode,
        string? ErrorMessage,
        int FactoryCreateCount,
        int RepositoryLoadRequestCount,
        int DpoExecutorCallCount,
        int SupervisedCallCount,
        int RetainedBaseOpenCount,
        bool RepositoryLoadRequestMatchedStartRequest,
        string? CheckpointStartSource,
        string? ResumeParentManifestSha256,
        int? LoadedParentStep,
        string? LoadedParentManifestSha256,
        string? LoadedParentWeightsSha256,
        long? LoadedParentWeightsByteLength,
        string? CreatedBaseCheckpointSha256,
        string? CreatedModelSha256,
        bool StepThreeExists,
        string? StepThreeManifestSha256,
        string? StepThreeWeightsSha256);

    private static async Task<PublishedRepositoryOwnedStepTwo>
        PublishRepositoryOwnedStepTwoAsync(
            TinyTrainingFixtureCL fixture,
            string uniqueSuffix,
            LoraConfig? loraConfig = null)
    {
        var setup = CreateRepositoryOwnedResumedTrainingSetup(
            fixture,
            uniqueSuffix);
        var (transaction, pairs) =
            PrepareDurableTransactionFromRepositoryOwnedDataset(
                setup.CurrentDpoDatasetAddress,
                RepositoryOwnedDomainSlug);
        var dpoExecutor = new RecordingDpoExecutor(
            DpoExecutorBehavior.Success);
        var supervised = new MutatingSupervisedStepExecutor();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            setup.Factory,
            CreateAuthority(),
            dpoExecutor,
            supervised,
            fixture.AdapterDirectory,
            loraConfig: loraConfig);

        var result = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                RepositoryOwnedDomainSlug,
                [fixture.Example(RepositoryOwnedDomainSlug, "p", "r")],
                pairs,
                transaction,
                CancellationToken.None);

        Assert.True(result.IsSuccess,
            $"{result.ErrorCode}: {result.ErrorMessage}");
        Assert.Equal(1, setup.FactoryCreateCount());
        Assert.Equal(1, dpoExecutor.CallCount);
        Assert.Equal(1, supervised.CallCount);
        var openedStepTwo = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(setup.ChildAddress);
        Assert.True(openedStepTwo.IsSuccess, openedStepTwo.ErrorMessage);
        var stepTwo = openedStepTwo.Value!.Manifest;
        Assert.Equal(2, stepTwo.Step);
        var stepTwoWeights = File.ReadAllBytes(setup.ChildAddress.WeightsPath);
        Assert.Equal(stepTwo.WeightsSha256, Sha256Hex(stepTwoWeights));
        Assert.Equal(stepTwo.WeightsByteLength, stepTwoWeights.Length);
        return new PublishedRepositoryOwnedStepTwo(
            setup.RepositoryRoot,
            setup.RuntimeCompatibility,
            setup.ChildAddress,
            stepTwo,
            stepTwoWeights);
    }

    private static async Task RunRepositoryOwnedContinuationVstestChildAsync()
    {
        // The result path is the only child-created file allowed outside the
        // temporary repository. The child writes it atomically so the parent
        // reads a complete protocol packet only after the process exits.
        var resultPath = Environment.GetEnvironmentVariable(
            RepositoryOwnedContinuationResultEnvironmentName);
        if (string.IsNullOrWhiteSpace(resultPath))
        {
            throw new InvalidOperationException(
                $"Missing child environment {RepositoryOwnedContinuationResultEnvironmentName}.");
        }

        try
        {
            var result =
                await ExecuteRepositoryOwnedContinuationVstestChildAsync();
            await WriteRepositoryOwnedContinuationChildResultAsync(
                resultPath,
                result);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            var scenario = Environment.GetEnvironmentVariable(
                RepositoryOwnedContinuationChildScenarioEnvironmentName)
                           ?? "unknown";
            await WriteRepositoryOwnedContinuationChildResultAsync(
                resultPath,
                new RepositoryOwnedContinuationVstestChildResult(
                    IsSuccess: false,
                    scenario,
                    "CHILD_UNHANDLED_EXCEPTION",
                    exception.ToString(),
                    FactoryCreateCount: 0,
                    RepositoryLoadRequestCount: 0,
                    DpoExecutorCallCount: 0,
                    SupervisedCallCount: 0,
                    RetainedBaseOpenCount: 0,
                    RepositoryLoadRequestMatchedStartRequest: false,
                    CheckpointStartSource: null,
                    ResumeParentManifestSha256: null,
                    LoadedParentStep: null,
                    LoadedParentManifestSha256: null,
                    LoadedParentWeightsSha256: null,
                    LoadedParentWeightsByteLength: null,
                    CreatedBaseCheckpointSha256: null,
                    CreatedModelSha256: null,
                    StepThreeExists: false,
                    StepThreeManifestSha256: null,
                    StepThreeWeightsSha256: null));
        }
        finally
        {
            foreach (var environmentName
                     in RepositoryOwnedContinuationChildEnvironmentNames)
            {
                Environment.SetEnvironmentVariable(environmentName, null);
            }
        }
    }

    private static async Task<RepositoryOwnedContinuationVstestChildResult>
        ExecuteRepositoryOwnedContinuationVstestChildAsync()
    {
        var scenario = RequiredRepositoryOwnedContinuationChildEnvironment(
            RepositoryOwnedContinuationChildScenarioEnvironmentName);
        var repositoryRoot = RequiredRepositoryOwnedContinuationChildEnvironment(
            RepositoryOwnedContinuationRepositoryRootEnvironmentName);
        var fixtureRoot = RequiredRepositoryOwnedContinuationChildEnvironment(
            RepositoryOwnedContinuationFixtureRootEnvironmentName);
        var baseCheckpointPath =
            RequiredRepositoryOwnedContinuationChildEnvironment(
                RepositoryOwnedContinuationBaseCheckpointEnvironmentName);
        var tokenizerPath = RequiredRepositoryOwnedContinuationChildEnvironment(
            RepositoryOwnedContinuationTokenizerEnvironmentName);
        var retainedRoot = RequiredRepositoryOwnedContinuationChildEnvironment(
            RepositoryOwnedContinuationRetainedRootEnvironmentName);
        var resultPath = RequiredRepositoryOwnedContinuationChildEnvironment(
            RepositoryOwnedContinuationResultEnvironmentName);
        ValidateRepositoryOwnedContinuationChildPaths(
            fixtureRoot,
            repositoryRoot,
            baseCheckpointPath,
            tokenizerPath,
            retainedRoot,
            resultPath);

        var modelConfig =
            DecodeRepositoryOwnedContinuationChildJson<ModelConfig>(
                RequiredRepositoryOwnedContinuationChildEnvironment(
                    RepositoryOwnedContinuationModelConfigEnvironmentName));
        var trainingConfig =
            DecodeRepositoryOwnedContinuationChildJson<TrainingConfig>(
                RequiredRepositoryOwnedContinuationChildEnvironment(
                    RepositoryOwnedContinuationTrainingConfigEnvironmentName));
        var loraConfig =
            DecodeOptionalRepositoryOwnedContinuationChildLoraConfig(scenario);

        var currentDpoAddress = RequireSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                "v0003"));
        var openedCurrentDpo = RequireSuccess(
            RepositoryOwnedDpoDatasetManifestSealer
                .OpenSealedForManifestIntegrity(currentDpoAddress));
        var currentDpoManifest =
            new RepositoryOwnedDpoDatasetManifestIdentity(
                currentDpoAddress.ManifestRelativePath,
                openedCurrentDpo.Manifest.ManifestSha256);
        var startRequest =
            new RepositoryOwnedIncrementalTrainingSessionStartRequest(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                RepositoryOwnedRuntimeCompatibility(),
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
                currentDpoManifest);

        var factoryCreateCount = 0;
        var repositoryLoadRequestCount = 0;
        var retainedBaseOpenCount = 0;
        var repositoryLoadRequestMatchedStartRequest = false;
        RepositoryOwnedModelCheckpointLoadResult? lastLoad = null;
        PreTrainingModelRecordingSessionFactory? recordingFactory = null;
        RecordingDpoExecutor? dpoExecutor = null;
        MutatingSupervisedStepExecutor? supervised = null;
        OperationResult<TrainedAdapterArtifact> trainingResult;

        using var retainedAccess =
            CreateRepositoryOwnedContinuationRetainedFileAccess(
                baseCheckpointPath,
                tokenizerPath,
                retainedRoot);
        var retainedFactoryResult =
            RetainedCheckpointBackedIncrementalTrainingSessionFactory.Bind(
                retainedAccess,
                new NullSessionStorageCL(),
                modelConfig,
                trainingConfig,
                afterRetainedArtifactOpenForTesting: role =>
                {
                    if (role == ContinuousLearningRetainedArtifactRole
                            .BaseCheckpoint)
                    {
                        retainedBaseOpenCount++;
                    }
                },
                loadRepositoryCheckpoint: request =>
                {
                    repositoryLoadRequestCount++;
                    repositoryLoadRequestMatchedStartRequest =
                        request == startRequest;
                    var loaded = new RepositoryOwnedModelCheckpointLoader()
                        .LoadLatestCompatible(
                            request.RepositoryRoot,
                            request.DomainSlug,
                            request.DesignPatternSlug,
                            request.ModelIdSlug,
                            request.ModelVersion,
                            request.RuntimeCompatibility);
                    if (loaded.IsSuccess && loaded.Value is not null)
                        lastLoad = loaded.Value;
                    return loaded;
                },
                resolveRepositoryStartRequest: domain =>
                    StringComparer.Ordinal.Equals(
                        domain,
                        RepositoryOwnedDomainSlug)
                        ? OperationResult<
                            RepositoryOwnedIncrementalTrainingSessionStartRequest>
                            .Ok(startRequest)
                        : OperationResult<
                            RepositoryOwnedIncrementalTrainingSessionStartRequest>
                            .Fail(
                                "Unexpected repository-owned continuation domain.",
                                "UNEXPECTED_DOMAIN"),
                repositoryCheckpointAdapterConfig: loraConfig);

        if (!retainedFactoryResult.IsSuccess
            || retainedFactoryResult.Value is null)
        {
            return BuildRepositoryOwnedContinuationChildResult(
                scenario,
                retainedFactoryResult.IsSuccess,
                retainedFactoryResult.ErrorCode,
                retainedFactoryResult.ErrorMessage,
                factoryCreateCount,
                repositoryLoadRequestCount,
                dpoExecutor,
                supervised,
                retainedBaseOpenCount,
                repositoryLoadRequestMatchedStartRequest,
                lastLoad,
                recordingFactory,
                repositoryRoot);
        }

        using var retainedFactory = retainedFactoryResult.Value;
        recordingFactory =
            new PreTrainingModelRecordingSessionFactory(retainedFactory);
        dpoExecutor = new RecordingDpoExecutor(DpoExecutorBehavior.Success);
        supervised = new MutatingSupervisedStepExecutor();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            recordingFactory,
            CreateAuthority(),
            dpoExecutor,
            supervised,
            Path.Combine(retainedRoot, "trainer-artifacts"),
            loraConfig: loraConfig);
        var (transaction, pairs) =
            PrepareDurableTransactionFromRepositoryOwnedDataset(
                currentDpoAddress,
                RepositoryOwnedDomainSlug);

        // The only payload crossing the process boundary is environment text:
        // absolute paths plus JSON config identity. The child reopens sealed
        // repository bytes, prepares its own transaction, and constructs the
        // production retained factory/trainer graph in this process.
        trainingResult = await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                RepositoryOwnedDomainSlug,
                [ProvenanceTestFactory.WithSource(
                    new TrainingExample(
                        "p",
                        "r",
                        RepositoryOwnedDomainSlug,
                        1d,
                        DateTimeOffset.UtcNow),
                    "repository-owned-vstest-child")],
                pairs,
                transaction,
                CancellationToken.None);
        factoryCreateCount = recordingFactory.CreateCount;

        return BuildRepositoryOwnedContinuationChildResult(
            scenario,
            trainingResult.IsSuccess,
            trainingResult.ErrorCode,
            trainingResult.ErrorMessage,
            factoryCreateCount,
            repositoryLoadRequestCount,
            dpoExecutor,
            supervised,
            retainedBaseOpenCount,
            repositoryLoadRequestMatchedStartRequest,
            lastLoad,
            recordingFactory,
            repositoryRoot);
    }

    private static Process StartRepositoryOwnedContinuationVstestChild(
        TinyTrainingFixtureCL fixture,
        string repositoryRoot,
        string resultPath,
        string scenario,
        LoraConfig? loraConfig)
    {
        ValidateRepositoryOwnedContinuationParentPaths(
            fixture.RootDirectory,
            repositoryRoot,
            resultPath);
        var retainedRoot = Path.GetFullPath(Path.Combine(
            repositoryRoot,
            "training-artifacts",
            "vstest-child-retained",
            scenario,
            Guid.NewGuid().ToString("N")));
        var trainingConfig = fixture.TrainingConfig with
        {
            CheckpointDir = Path.Combine(retainedRoot, "training-checkpoints"),
        };

        var startInfo =
            new ProcessStartInfo(ResolveDotnetHostForRepositoryOwnedChildTest())
            {
                WorkingDirectory =
                    Path.GetDirectoryName(FindRepositoryFile(
                        "LlmFromScratch.sln"))!,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
        startInfo.ArgumentList.Add("vstest");
        startInfo.ArgumentList.Add(
            ResolveCurrentRepositoryOwnedContinuationTestAssemblyPath());
        startInfo.ArgumentList.Add(
            "--TestCaseFilter:FullyQualifiedName~RepositoryOwnedResumedTraining_VstestChildProcessReloadsPublished000002AndPublishes000003");
        startInfo.ArgumentList.Add("--Logger:Console;Verbosity=minimal");

        foreach (var environmentName
                 in RepositoryOwnedContinuationChildEnvironmentNames)
        {
            startInfo.Environment.Remove(environmentName);
        }

        startInfo.Environment[
            RepositoryOwnedContinuationChildModeEnvironmentName] = "1";
        startInfo.Environment[
            RepositoryOwnedContinuationChildScenarioEnvironmentName] = scenario;
        startInfo.Environment[
            RepositoryOwnedContinuationRepositoryRootEnvironmentName] =
            repositoryRoot;
        startInfo.Environment[
            RepositoryOwnedContinuationFixtureRootEnvironmentName] =
            fixture.RootDirectory;
        startInfo.Environment[
            RepositoryOwnedContinuationBaseCheckpointEnvironmentName] =
            fixture.BaseCheckpointPath;
        startInfo.Environment[
            RepositoryOwnedContinuationTokenizerEnvironmentName] =
            fixture.TokenizerPath;
        startInfo.Environment[
            RepositoryOwnedContinuationRetainedRootEnvironmentName] =
            retainedRoot;
        startInfo.Environment[
            RepositoryOwnedContinuationResultEnvironmentName] = resultPath;
        startInfo.Environment[
            RepositoryOwnedContinuationModelConfigEnvironmentName] =
            EncodeRepositoryOwnedContinuationChildJson(fixture.ModelConfig);
        startInfo.Environment[
            RepositoryOwnedContinuationTrainingConfigEnvironmentName] =
            EncodeRepositoryOwnedContinuationChildJson(trainingConfig);
        if (loraConfig is not null)
        {
            startInfo.Environment[
                RepositoryOwnedContinuationLoraConfigEnvironmentName] =
                EncodeRepositoryOwnedContinuationChildJson(loraConfig);
        }

        var process = Process.Start(startInfo);
        Assert.NotNull(process);
        return process;
    }

    private static async Task<RepositoryOwnedContinuationVstestChildResult>
        WaitForRepositoryOwnedContinuationChildAsync(
            Process process,
            string resultPath,
            TimeSpan timeout)
    {
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        using var timeoutSource = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }

            await process.WaitForExitAsync(CancellationToken.None);
            var timeoutStdout = await stdoutTask;
            var timeoutStderr = await stderrTask;
            Assert.Fail(
                $"Repository-owned continuation child timed out after {timeout}. stdout={timeoutStdout}; stderr={timeoutStderr}");
        }

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        Assert.Equal(0, process.ExitCode);
        Assert.True(
            File.Exists(resultPath),
            $"Repository-owned continuation child exited without result. stdout={stdout}; stderr={stderr}");
        try
        {
            var json = await File.ReadAllTextAsync(resultPath);
            var result = JsonSerializer.Deserialize<
                RepositoryOwnedContinuationVstestChildResult>(json);
            Assert.NotNull(result);
            return result!;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            throw new InvalidOperationException(
                $"Repository-owned continuation child result was unreadable. stdout={stdout}; stderr={stderr}",
                exception);
        }
    }

    private static IReadOnlySet<string>
        SnapshotFixtureEntriesOutsideArtifactRepository(
            string fixtureRoot,
            string repositoryRoot)
    {
        var fixtureFullPath = Path.GetFullPath(fixtureRoot);
        var repositoryFullPath = Path.GetFullPath(repositoryRoot);
        Assert.True(
            IsSameOrDescendantPath(repositoryFullPath, fixtureFullPath),
            "The repository-owned fixture root must stay inside the test fixture root.");
        if (!Directory.Exists(fixtureFullPath))
            return new HashSet<string>(StringComparer.Ordinal);

        var entries = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in Directory.EnumerateFileSystemEntries(
                     fixtureFullPath,
                     "*",
                     SearchOption.AllDirectories))
        {
            var fullPath = Path.GetFullPath(entry);
            if (IsSameOrDescendantPath(fullPath, repositoryFullPath))
                continue;
            entries.Add(NormalizeFixtureRelativePath(
                fixtureFullPath,
                fullPath,
                Directory.Exists(fullPath)));
        }

        return entries;
    }

    private static void AssertNoUnexpectedOutOfRepositoryChildArtifacts(
        IReadOnlySet<string> before,
        string fixtureRoot,
        string repositoryRoot,
        string allowedResultPath)
    {
        ValidateRepositoryOwnedContinuationParentPaths(
            fixtureRoot,
            repositoryRoot,
            allowedResultPath);
        var after = SnapshotFixtureEntriesOutsideArtifactRepository(
            fixtureRoot,
            repositoryRoot);
        var allowed = BuildAllowedRepositoryOwnedContinuationResultEntries(
            fixtureRoot,
            allowedResultPath);
        var unexpected = after
            .Except(before, StringComparer.Ordinal)
            .Except(allowed, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var removed = before
            .Except(after, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Empty(unexpected);
        Assert.Empty(removed);
    }

    private static RepositoryOwnedContinuationVstestChildResult
        BuildRepositoryOwnedContinuationChildResult(
            string scenario,
            bool isSuccess,
            string? errorCode,
            string? errorMessage,
            int factoryCreateCount,
            int repositoryLoadRequestCount,
            object? dpoExecutor,
            object? supervised,
            int retainedBaseOpenCount,
            bool repositoryLoadRequestMatchedStartRequest,
            RepositoryOwnedModelCheckpointLoadResult? lastLoad,
            object? recordingFactory,
            string repositoryRoot)
    {
        var recordingDpoExecutor = (RecordingDpoExecutor?)dpoExecutor;
        var supervisedExecutor = (MutatingSupervisedStepExecutor?)supervised;
        var preTrainingRecordingFactory =
            (PreTrainingModelRecordingSessionFactory?)recordingFactory;
        var stepThreeAddress = RequireSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                step: 3));
        var stepThreeWeightsExists = File.Exists(stepThreeAddress.WeightsPath);
        var stepThreeManifestExists = File.Exists(stepThreeAddress.ManifestPath);
        var stepThreeExists =
            stepThreeWeightsExists || stepThreeManifestExists;
        string? stepThreeManifestSha256 = null;
        string? stepThreeWeightsSha256 = null;
        if (stepThreeWeightsExists)
            stepThreeWeightsSha256 =
                Sha256Hex(File.ReadAllBytes(stepThreeAddress.WeightsPath));
        if (stepThreeWeightsExists && stepThreeManifestExists)
        {
            var openedStepThree =
                RepositoryOwnedModelCheckpointManifestSealer
                    .OpenSealed(stepThreeAddress);
            if (openedStepThree.IsSuccess && openedStepThree.Value is not null)
                stepThreeManifestSha256 =
                    openedStepThree.Value.Manifest.ManifestSha256;
        }

        return new RepositoryOwnedContinuationVstestChildResult(
            isSuccess,
            scenario,
            errorCode,
            errorMessage,
            factoryCreateCount,
            repositoryLoadRequestCount,
            recordingDpoExecutor?.CallCount ?? 0,
            supervisedExecutor?.CallCount ?? 0,
            retainedBaseOpenCount,
            repositoryLoadRequestMatchedStartRequest,
            preTrainingRecordingFactory?.CreatedCheckpointStart?.Source.ToString(),
            preTrainingRecordingFactory?.CreatedCheckpointStart
                ?.ResumeParentCheckpoint
                ?.ManifestSha256,
            lastLoad?.Manifest.Step,
            lastLoad?.Manifest.ManifestSha256,
            lastLoad?.WeightsSha256,
            lastLoad?.WeightsByteLength,
            preTrainingRecordingFactory?.CreatedBaseCheckpointSha256,
            preTrainingRecordingFactory?.CreatedModelBytes is null
                ? null
                : Sha256Hex(preTrainingRecordingFactory.CreatedModelBytes),
            stepThreeExists,
            stepThreeManifestSha256,
            stepThreeWeightsSha256);
    }

    private static ServerOwnedContinuousLearningFileAccess
        CreateRepositoryOwnedContinuationRetainedFileAccess(
            string baseCheckpointPath,
            string tokenizerPath,
            string retainedRoot)
    {
        var adapterRoot = Path.Combine(retainedRoot, "adapters");
        var cacheRoot = Path.Combine(retainedRoot, "cache");
        var registryRoot = Path.Combine(retainedRoot, "registry");
        var dpoPairRoot = Path.Combine(retainedRoot, "dpo-pairs");
        Directory.CreateDirectory(adapterRoot);
        Directory.CreateDirectory(cacheRoot);
        Directory.CreateDirectory(registryRoot);
        Directory.CreateDirectory(dpoPairRoot);
        return ServerOwnedContinuousLearningFileAccess.Acquire(
            baseCheckpointPath,
            tokenizerPath,
            adapterRoot,
            cacheRoot,
            registryRoot,
            dpoPairRoot,
            options: null,
            onRetainedObjectTrustLost: static () =>
                throw new InvalidOperationException(
                    "Repository-owned continuation child retained paths must stay stable."));
    }

    private static LoraConfig?
        DecodeOptionalRepositoryOwnedContinuationChildLoraConfig(
            string scenario)
    {
        var encoded = Environment.GetEnvironmentVariable(
            RepositoryOwnedContinuationLoraConfigEnvironmentName);
        if (string.Equals(
                scenario,
                RepositoryOwnedContinuationMissingLoraScenario,
                StringComparison.Ordinal))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(encoded))
        {
            throw new InvalidOperationException(
                $"Scenario {scenario} requires explicit LoRA config JSON.");
        }

        return DecodeRepositoryOwnedContinuationChildJson<LoraConfig>(
            encoded);
    }

    private static string RequiredRepositoryOwnedContinuationChildEnvironment(
        string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing child environment {name}.");
        }

        return value;
    }

    private static string EncodeRepositoryOwnedContinuationChildJson<T>(
        T value)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(value)));

    private static T DecodeRepositoryOwnedContinuationChildJson<T>(
        string encoded)
        => JsonSerializer.Deserialize<T>(
               Encoding.UTF8.GetString(Convert.FromBase64String(encoded)))
           ?? throw new InvalidOperationException(
               $"Could not decode {typeof(T).Name} child JSON.");

    private static async Task WriteRepositoryOwnedContinuationChildResultAsync(
        string resultPath,
        RepositoryOwnedContinuationVstestChildResult result)
    {
        var fullResultPath = Path.GetFullPath(resultPath);
        var resultDirectory = Path.GetDirectoryName(fullResultPath);
        if (string.IsNullOrWhiteSpace(resultDirectory))
            throw new InvalidOperationException("Child result path is invalid.");
        Directory.CreateDirectory(resultDirectory);
        var temporaryPath = Path.Combine(
            resultDirectory,
            $".{Path.GetFileName(fullResultPath)}.{Guid.NewGuid():N}.tmp");
        await File.WriteAllTextAsync(
            temporaryPath,
            JsonSerializer.Serialize(
                result,
                new JsonSerializerOptions { WriteIndented = true }),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        File.Move(temporaryPath, fullResultPath, overwrite: true);
    }

    private static void ValidateRepositoryOwnedContinuationParentPaths(
        string fixtureRoot,
        string repositoryRoot,
        string resultPath)
    {
        var fixtureFullPath = Path.GetFullPath(fixtureRoot);
        var repositoryFullPath = Path.GetFullPath(repositoryRoot);
        var resultFullPath = Path.GetFullPath(resultPath);
        Assert.True(
            IsSameOrDescendantPath(repositoryFullPath, fixtureFullPath),
            "The temporary artifact repository must be inside the test fixture root.");
        Assert.True(
            IsSameOrDescendantPath(resultFullPath, fixtureFullPath),
            "The child result must stay inside the test fixture root.");
        Assert.False(
            IsSameOrDescendantPath(resultFullPath, repositoryFullPath),
            "The explicitly allowed child result is outside the artifact repository.");
    }

    private static void ValidateRepositoryOwnedContinuationChildPaths(
        string fixtureRoot,
        string repositoryRoot,
        string baseCheckpointPath,
        string tokenizerPath,
        string retainedRoot,
        string resultPath)
    {
        ValidateRepositoryOwnedContinuationParentPaths(
            fixtureRoot,
            repositoryRoot,
            resultPath);
        var fixtureFullPath = Path.GetFullPath(fixtureRoot);
        var repositoryFullPath = Path.GetFullPath(repositoryRoot);
        var baseFullPath = Path.GetFullPath(baseCheckpointPath);
        var tokenizerFullPath = Path.GetFullPath(tokenizerPath);
        var retainedFullPath = Path.GetFullPath(retainedRoot);
        if (!File.Exists(baseFullPath))
        {
            throw new FileNotFoundException(
                "Repository-owned continuation child base checkpoint is missing.",
                baseFullPath);
        }

        if (!File.Exists(tokenizerFullPath))
        {
            throw new FileNotFoundException(
                "Repository-owned continuation child tokenizer is missing.",
                tokenizerFullPath);
        }

        if (!IsSameOrDescendantPath(baseFullPath, fixtureFullPath)
            || !IsSameOrDescendantPath(tokenizerFullPath, fixtureFullPath)
            || !IsSameOrDescendantPath(retainedFullPath, repositoryFullPath))
        {
            throw new InvalidOperationException(
                "Repository-owned continuation child paths are outside their bounded roots.");
        }
    }

    private static IReadOnlySet<string>
        BuildAllowedRepositoryOwnedContinuationResultEntries(
            string fixtureRoot,
            string resultPath)
    {
        var fixtureFullPath = Path.GetFullPath(fixtureRoot);
        var resultFullPath = Path.GetFullPath(resultPath);
        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            NormalizeFixtureRelativePath(
                fixtureFullPath,
                resultFullPath,
                isDirectory: false),
        };
        var current = new DirectoryInfo(
            Path.GetDirectoryName(resultFullPath)!);
        while (current is not null
               && !PathsEqual(current.FullName, fixtureFullPath))
        {
            allowed.Add(NormalizeFixtureRelativePath(
                fixtureFullPath,
                current.FullName,
                isDirectory: true));
            current = current.Parent;
        }

        return allowed;
    }

    private static string NormalizeFixtureRelativePath(
        string fixtureRoot,
        string fullPath,
        bool isDirectory)
    {
        var relative = Path
            .GetRelativePath(fixtureRoot, fullPath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
        return isDirectory ? $"{relative}/" : relative;
    }

    private static bool IsSameOrDescendantPath(string candidate, string root)
    {
        var candidateFullPath = Path.GetFullPath(candidate);
        var rootFullPath = Path.GetFullPath(root);
        if (PathsEqual(candidateFullPath, rootFullPath))
            return true;
        var relative = Path.GetRelativePath(rootFullPath, candidateFullPath);
        return !string.IsNullOrWhiteSpace(relative)
               && !string.Equals(relative, ".", StringComparison.Ordinal)
               && !relative.StartsWith("..", StringComparison.Ordinal)
               && !Path.IsPathRooted(relative);
    }

    private static bool PathsEqual(string first, string second)
        => StringComparerForCurrentFileSystem.Equals(
            Path.GetFullPath(first).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar),
            Path.GetFullPath(second).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar));

    private static StringComparer StringComparerForCurrentFileSystem
        => OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private static string ResolveCurrentRepositoryOwnedContinuationTestAssemblyPath()
    {
        var assemblyLocation =
            typeof(IncrementalTrainerDpoWiringTests).Assembly.Location;
        if (!string.IsNullOrWhiteSpace(assemblyLocation)
            && File.Exists(assemblyLocation))
        {
            return assemblyLocation;
        }

        var candidate = Path.Combine(
            AppContext.BaseDirectory,
            "LlmFromScratch.Tests.dll");
        if (File.Exists(candidate))
            return candidate;

        throw new FileNotFoundException(
            "Could not resolve the current test assembly for repository-owned continuation child vstest.");
    }

    private static string ResolveDotnetHostForRepositoryOwnedChildTest()
    {
        foreach (var environmentName in new[] { "DOTNET_HOST_PATH", "DOTNET_EXE" })
        {
            var configuredHost = Environment.GetEnvironmentVariable(
                environmentName);
            if (!string.IsNullOrWhiteSpace(configuredHost)
                && File.Exists(configuredHost))
            {
                return configuredHost;
            }
        }

        return OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
    }

    private static RepositoryOwnedIncrementalTrainingSessionStartRequest
        CreateRepositoryOwnedContinuationRequest(
            PublishedRepositoryOwnedStepTwo published,
            RepositoryOwnedDpoDatasetManifestIdentity currentDpoDatasetManifest)
        => new(
            published.RepositoryRoot,
            RepositoryOwnedDomainSlug,
            RepositoryOwnedDesignPatternSlug,
            RepositoryOwnedModelIdSlug,
            RepositoryOwnedModelVersion,
            published.RuntimeCompatibility,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
            CurrentDpoDatasetManifest: currentDpoDatasetManifest);

    private static LoraConfig CustomOneProjectionLoraConfig()
        => new()
        {
            Rank = 2,
            Alpha = 4f,
            Dropout = 0f,
            TargetModules = ["q_proj"],
        };

    private static (
        IIncrementalTrainingSessionFactory Factory,
        Func<int> FactoryCreateCount,
        RepositoryOwnedModelCheckpointManifest ParentManifest,
        RepositoryOwnedModelCheckpointAddress ChildAddress,
        RepositoryOwnedDpoDatasetManifestIdentity ParentDpoDatasetManifest,
        RepositoryOwnedDpoDatasetManifestIdentity? CurrentDpoDatasetManifest,
        RepositoryOwnedDpoDatasetAddress CurrentDpoDatasetAddress,
        string RepositoryRoot,
        RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
        RepositoryOwnedIncrementalTrainingSessionStartRequest StartRequest)
        CreateRepositoryOwnedResumedTrainingSetup(
            TinyTrainingFixtureCL fixture,
            string uniqueSuffix,
            bool sealCurrentDpoDataset = true)
    {
        var repositoryRoot = CreateRepositoryOwnedSentinelRoot(
            fixture,
            uniqueSuffix);
        var runtimeCompatibility = RepositoryOwnedRuntimeCompatibility();
        var parentDpoDataset = SealRepositoryOwnedDpoDataset(
            repositoryRoot,
            "v0001");
        var currentDpoDatasetAddress = RequireSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                "v0002"));
        var currentDpoDatasetManifest = sealCurrentDpoDataset
            ? SealRepositoryOwnedDpoDataset(currentDpoDatasetAddress)
            : null;
        var parentAddress = RequireSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                step: 1));
        var parentManifest = SealRepositoryOwnedParentCheckpoint(
            fixture,
            parentAddress,
            parentDpoDataset.Manifest,
            runtimeCompatibility);
        var childAddress = RequireSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                RepositoryOwnedModelIdSlug,
                RepositoryOwnedModelVersion,
                step: 2));
        var startRequest = new RepositoryOwnedIncrementalTrainingSessionStartRequest(
            repositoryRoot,
            RepositoryOwnedDomainSlug,
            RepositoryOwnedDesignPatternSlug,
            RepositoryOwnedModelIdSlug,
            RepositoryOwnedModelVersion,
            runtimeCompatibility,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
            CurrentDpoDatasetManifest: currentDpoDatasetManifest?.Manifest);

        var factory = new RepositoryOwnedCheckpointDpoSessionFactory(
            fixture,
            parentAddress,
            parentManifest,
            startRequest);

        return (
            factory,
            () => factory.CreateCount,
            parentManifest,
            childAddress,
            parentDpoDataset.Manifest,
            currentDpoDatasetManifest?.Manifest,
            currentDpoDatasetAddress,
            repositoryRoot,
            runtimeCompatibility,
            startRequest);
    }

    private static string CreateRepositoryOwnedSentinelRoot(
        TinyTrainingFixtureCL fixture,
        string uniqueSuffix)
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            fixture.RootDirectory,
            $"repository-owned-{uniqueSuffix}"));
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "Microsoft Visual Studio Solution File, Format Version 12.00");
        Directory.CreateDirectory(Path.Combine(
            repositoryRoot,
            "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
        return repositoryRoot;
    }

    private static RepositoryOwnedDpoDatasetFixture
        SealRepositoryOwnedDpoDataset(
            string repositoryRoot,
            string datasetVersion)
    {
        var address = RequireSuccess(
            RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
                repositoryRoot,
                RepositoryOwnedDomainSlug,
                RepositoryOwnedDesignPatternSlug,
                datasetVersion));
        return SealRepositoryOwnedDpoDataset(address);
    }

    private static RepositoryOwnedDpoDatasetFixture SealRepositoryOwnedDpoDataset(
        RepositoryOwnedDpoDatasetAddress address)
    {
        var provenance = QwenProvenance(
            $"repository-owned-dpo-{address.DatasetVersion}");
        var pair = BindPair(new DpoPair(
            "repository owned parent prompt",
            "repository owned accepted answer",
            "repository owned rejected answer",
            RepositoryOwnedDomainSlug,
            DateTimeOffset.UnixEpoch.AddSeconds(17))
        {
            TeacherProvenance = provenance,
            GovernanceDecision = Governance(),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.User),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                provenance),
        });
        var manifest = RequireSuccess(
            RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
                address,
                RepositoryOwnedDpoRequestIdentity(),
                [pair]));
        return new RepositoryOwnedDpoDatasetFixture(
            address,
            new RepositoryOwnedDpoDatasetManifestIdentity(
                address.ManifestRelativePath,
                manifest.ManifestSha256));
    }

    private static RepositoryOwnedModelCheckpointManifest
        SealRepositoryOwnedParentCheckpoint(
            TinyTrainingFixtureCL fixture,
            RepositoryOwnedModelCheckpointAddress parentAddress,
            RepositoryOwnedDpoDatasetManifestIdentity dpoDatasetManifest,
            RepositoryOwnedModelRuntimeCompatibility runtimeCompatibility)
    {
        using var weights = File.OpenRead(fixture.BaseCheckpointPath);
        return RequireSuccess(
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                parentAddress,
                weights,
                new RepositoryOwnedModelCheckpointManifestRequest(
                    ParentCheckpoint: null,
                    ColdStartReason:
                    "test parent checkpoint before incremental trainer update",
                    DpoDatasetManifest: dpoDatasetManifest,
                    RuntimeCompatibility: runtimeCompatibility,
                    CreatedAtUtc: DateTimeOffset.UnixEpoch,
                    ProvenanceRefs:
                    [
                        "incremental-trainer-dpo-wiring-test-parent",
                    ])));
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity
        RepositoryOwnedDpoRequestIdentity()
        => RequireSuccess(
            RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity(
                [Sha256Hex(Encoding.UTF8.GetBytes(
                    "paid reference answer omitted from fixture pairs"))],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen3.5:9b",
                    new string('d', 64),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    new string('c', 64)),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "incremental-trainer-dpo-wiring",
                    "v1",
                    new string('4', 64)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "incremental-trainer-dpo-wiring-tests",
                    "v1",
                    new string('5', 64))));

    private static RepositoryOwnedModelRuntimeCompatibility
        RepositoryOwnedRuntimeCompatibility()
        => new(
            ConfigSha256: new string('6', 64),
            TopologySha256: new string('7', 64),
            TokenizerSha256: new string('8', 64),
            TrainerSha256: new string('9', 64),
            SchemaSha256: new string('a', 64),
            CompatibilityKey: "incremental-trainer-dpo-wiring-v1");

    private static T RequireSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static JsonElement ReadCandidateSeedJson(
        RepositoryOwnedDpoDatasetAddress address,
        string transactionId,
        string domain)
    {
        var transactionPath = Path.Combine(
            address.PairStoreRoot,
            $"{transactionId}.dpo-transaction.json");
        using var document = JsonDocument.Parse(File.ReadAllBytes(transactionPath));
        foreach (var domainElement in document.RootElement
                     .GetProperty("domains")
                     .EnumerateArray())
        {
            if (StringComparer.Ordinal.Equals(
                    domainElement.GetProperty("domain").GetString(),
                    domain))
            {
                var seed = domainElement.GetProperty("seed");
                Assert.NotEqual(JsonValueKind.Null, seed.ValueKind);
                return seed.Clone();
            }
        }

        throw new InvalidOperationException(
            $"Missing DPO transaction domain seed for {domain}.");
    }

    private static void AssertColdStartCandidateSeed(
        JsonElement seed,
        TrainingSourceManifest expectedTrainingSources,
        IncrementalTrainingSession session,
        int expectedTrainingExamples,
        string expectedTrainingExampleBatchSha256)
    {
        var trainingSources = seed.GetProperty("trainingSources");
        Assert.Equal(
            expectedTrainingSources.SourceIds,
            trainingSources.GetProperty("sourceIds")
                .EnumerateArray()
                .Select(static value => value.GetString()!)
                .ToArray());
        Assert.Equal(
            expectedTrainingSources.Sha256,
            trainingSources.GetProperty("sha256").GetString());
        Assert.Equal(
            FileDpoPairStore.NoParentAdapterIdentity,
            seed.GetProperty("parentAdapterIdentity").GetString());
        Assert.Equal(
            session.BaseCheckpointSha256,
            seed.GetProperty("baseCheckpointSha256").GetString());
        Assert.Equal(
            session.TokenizerSha256,
            seed.GetProperty("tokenizerSha256").GetString());
        AssertModelIdentity(
            session.ModelIdentity,
            seed.GetProperty("model"));
        AssertLoraIdentity(
            TrainingSourceProvenance.Identify(IncrementalTrainer.DefaultConfig),
            seed.GetProperty("lora"));
        Assert.Equal(
            expectedTrainingExamples,
            seed.GetProperty("trainingExamples").GetInt32());
        Assert.Equal(
            expectedTrainingExampleBatchSha256,
            seed.GetProperty("trainingExampleBatchSha256").GetString());
    }

    private static void AssertSameColdStartCheckpointStart(
        IncrementalTrainingCheckpointStartMetadata? first,
        IncrementalTrainingCheckpointStartMetadata? retry,
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest,
        string reason)
    {
        Assert.NotNull(first);
        Assert.NotNull(retry);
        Assert.Equal(first.Source, retry.Source);
        Assert.Equal(
            IncrementalTrainingCheckpointStartSource
                .RetainedBaseCheckpointColdStart,
            retry.Source);
        Assert.Null(first.ResumeParentCheckpoint);
        Assert.Null(retry.ResumeParentCheckpoint);
        Assert.Same(startRequest, first.RepositoryStartRequest);
        Assert.Same(startRequest, retry.RepositoryStartRequest);
        Assert.NotNull(first.ColdStart);
        Assert.NotNull(retry.ColdStart);
        Assert.Equal(reason, first.ColdStart.HumanReason);
        Assert.Equal(reason, retry.ColdStart.HumanReason);
        Assert.Same(startRequest, first.ColdStart.RepositoryStartRequest);
        Assert.Same(startRequest, retry.ColdStart.RepositoryStartRequest);
    }

    private static void AssertModelIdentity(
        ModelConfigurationIdentity expected,
        JsonElement actual)
    {
        Assert.Equal(expected.VocabSize, actual.GetProperty("vocabSize").GetInt32());
        Assert.Equal(expected.ContextLen, actual.GetProperty("contextLen").GetInt32());
        Assert.Equal(expected.HiddenDim, actual.GetProperty("hiddenDim").GetInt32());
        Assert.Equal(expected.NumLayers, actual.GetProperty("numLayers").GetInt32());
        Assert.Equal(expected.NumHeads, actual.GetProperty("numHeads").GetInt32());
        Assert.Equal(expected.NumKvHeads, actual.GetProperty("numKvHeads").GetInt32());
        Assert.Equal(expected.FfnHiddenDim, actual.GetProperty("ffnHiddenDim").GetInt32());
        Assert.Equal(expected.RopeTheta, actual.GetProperty("ropeTheta").GetSingle());
        Assert.Equal(expected.RmsNormEps, actual.GetProperty("rmsNormEps").GetSingle());
        Assert.Equal(expected.Sha256, actual.GetProperty("sha256").GetString());
    }

    private static void AssertLoraIdentity(
        LoraConfigurationIdentity expected,
        LoraConfigurationIdentity actual)
    {
        Assert.Equal(expected.Rank, actual.Rank);
        Assert.Equal(expected.Alpha, actual.Alpha);
        Assert.Equal(expected.Scaling, actual.Scaling);
        Assert.Equal(expected.Dropout, actual.Dropout);
        Assert.Equal(expected.OrderedTargetModules, actual.OrderedTargetModules);
        Assert.Equal(expected.Sha256, actual.Sha256);
    }

    private static void AssertLoraIdentity(
        LoraConfigurationIdentity expected,
        JsonElement actual)
    {
        Assert.Equal(expected.Rank, actual.GetProperty("rank").GetInt32());
        Assert.Equal(expected.Alpha, actual.GetProperty("alpha").GetSingle());
        Assert.Equal(expected.Scaling, actual.GetProperty("scaling").GetSingle());
        Assert.Equal(expected.Dropout, actual.GetProperty("dropout").GetSingle());
        Assert.Equal(
            expected.OrderedTargetModules,
            actual.GetProperty("orderedTargetModules")
                .EnumerateArray()
                .Select(static value => value.GetString()!)
                .ToArray());
        Assert.Equal(expected.Sha256, actual.GetProperty("sha256").GetString());
    }

    private static string BuildExpectedTrainingExampleBatchSha256(
        IReadOnlyList<TrainingExample> examples)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendExpectedInt32(hash, examples.Count);
        AppendExpectedString(hash, "xiigen.dpo-supervised-example-batch.v1");
        foreach (var example in examples)
        {
            AppendExpectedString(hash, example.Prompt);
            AppendExpectedString(hash, example.Response);
            AppendExpectedString(hash, example.Domain);
            AppendExpectedString(hash, example.SourceId);
            AppendExpectedInt64(
                hash,
                BitConverter.DoubleToInt64Bits(example.Weight));
            AppendExpectedInt64(hash, example.CapturedAt.Ticks);
            AppendExpectedInt64(hash, example.CapturedAt.Offset.Ticks);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendExpectedString(
        IncrementalHash hash,
        string? value)
    {
        if (value is null)
        {
            AppendExpectedInt32(hash, -1);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        AppendExpectedInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendExpectedInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static void AppendExpectedInt64(IncrementalHash hash, long value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static byte[] ReadAndRewind(Stream stream)
    {
        stream.Position = 0;
        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        stream.Position = 0;
        return copy.ToArray();
    }

    private static ServerOwnedContinuousLearningFileAccess
        CreateRetainedFileAccess(
            TinyTrainingFixtureCL fixture,
            string uniqueSuffix)
    {
        var retainedRoot = Path.Combine(
            fixture.RootDirectory,
            $"retained-{uniqueSuffix}");
        var adapterRoot = Path.Combine(retainedRoot, "adapters");
        var cacheRoot = Path.Combine(retainedRoot, "cache");
        var registryRoot = Path.Combine(retainedRoot, "registry");
        var dpoPairRoot = Path.Combine(retainedRoot, "dpo-pairs");
        Directory.CreateDirectory(adapterRoot);
        Directory.CreateDirectory(cacheRoot);
        Directory.CreateDirectory(registryRoot);
        Directory.CreateDirectory(dpoPairRoot);
        return ServerOwnedContinuousLearningFileAccess.Acquire(
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath,
            adapterRoot,
            cacheRoot,
            registryRoot,
            dpoPairRoot,
            options: null,
            onRetainedObjectTrustLost: static () =>
                throw new InvalidOperationException(
                    "Retained test artifacts must stay stable."));
    }

    private static DpoPair Pair(TrainingTeacherProvenance? provenance)
        => BindPair(new DpoPair(
            "prompt",
            "chosen",
            "rejected",
            "BUG",
            DateTimeOffset.UtcNow)
        {
            TeacherProvenance = provenance,
            GovernanceDecision = Governance(),
            ChosenAuthorship = OpenTeacherAuthorship(provenance),
            RejectedAuthorship = LocalModelAuthorship(provenance),
        });

    private static DpoPair Pair(
        TrainingTeacherProvenance? provenance,
        TrainingDataGovernanceDecision? governance,
        string domain = "BUG")
        => BindPair(new DpoPair(
            "prompt",
            "chosen",
            "rejected",
            domain,
            DateTimeOffset.UtcNow)
        {
            TeacherProvenance = provenance,
            GovernanceDecision = governance,
            ChosenAuthorship = OpenTeacherAuthorship(provenance),
            RejectedAuthorship = LocalModelAuthorship(provenance),
        });

    private static DpoPairSideAuthorship? OpenTeacherAuthorship(
        TrainingTeacherProvenance? provenance)
    {
        // Невалидный паспорт остаётся данными для проверки допуска тренера, а не
        // становится доверенной ссылкой на сторону OpenTeacher уже в тестовой заготовке.
        if (!DpoPairSideAuthorshipPolicy
                .IsPermittedOpenTeacherPassportReference(provenance))
        {
            return null;
        }

        // Валидный паспорт получает обычную расширенную ссылку на авторство, чтобы
        // позитивные сценарии продолжали проверять связку стороны пары и
        // паспорта учителя, а не бедную трёхполевую пару.
        return new DpoPairSideAuthorship(
            DpoPairSideAuthorKind.OpenTeacher,
            provenance);
    }

    private static DpoPairSideAuthorship? LocalModelAuthorship(
        TrainingTeacherProvenance? provenance)
        => provenance is null
            ? null
            : new DpoPairSideAuthorship(DpoPairSideAuthorKind.LocalModel);

    private static DpoPair BindPair(DpoPair pair)
        => pair.TeacherProvenance is null
            ? pair
            : DpoPairTrainingContentAuthority.Bind(pair).Value!;

    private static FeedbackRouter CreateDurableRouter(
        TinyTrainingFixtureCL fixture,
        IReadOnlyList<(string Domain, string RequestId)> rows,
        string uniqueStoreSuffix,
        int? maximumTransactionPairCountForTesting = null)
    {
        var router = new FeedbackRouter(
            new NullSessionStorageCL(),
            new AcceptingDpoCache(),
            Path.Combine(
                fixture.RootDirectory,
                $"durable-dpo-pairs-{uniqueStoreSuffix}"),
            FileDpoPairStore.DefaultMaximumRecordCount,
            maximumTransactionPairCountForTesting:
                maximumTransactionPairCountForTesting);
        foreach (var (domain, requestId) in rows)
        {
            var routed = router.Route(new FeedbackEvent(
                $"fix the {domain} {requestId}",
                $"bad response {requestId}",
                $"good response {requestId}",
                FeedbackSignal.Edited,
                domain)
            {
                TeacherProvenance = QwenProvenance(requestId),
                GovernanceDecision = Governance(),
            });
            Assert.True(routed.IsSuccess, routed.ErrorMessage);
        }
        return router;
    }

    private static (
        FileDpoPairStore.TransactionHandle Transaction,
        IReadOnlyList<DpoPair> Pairs) PrepareDurableTransaction(
            TinyTrainingFixtureCL fixture,
            string requestId,
            string domain = "BUG",
            string? uniqueStoreSuffix = null)
    {
        var router = new FeedbackRouter(
            new NullSessionStorageCL(),
            new AcceptingDpoCache(),
            Path.Combine(
                fixture.RootDirectory,
                uniqueStoreSuffix is null
                    ? "durable-dpo-pairs"
                    : $"durable-dpo-pairs-{uniqueStoreSuffix}"));
        var feedback = new FeedbackEvent(
            $"fix the {domain}",
            $"bad response {requestId}",
            $"good response {requestId}",
            FeedbackSignal.Edited,
            domain)
        {
            TeacherProvenance = QwenProvenance(requestId),
            GovernanceDecision = Governance(),
        };
        var routed = router.Route(feedback);
        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        var prepared = ((IDpoPairTransactionCoordinator)router)
            .PrepareDpoTransaction([domain]);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            prepared.Value);
        var snapshot = transaction.Read();
        Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
        return (
            transaction,
            snapshot.Value!.Pairs.Select(static value => value.Pair).ToArray());
    }

    private static (
        FileDpoPairStore.TransactionHandle Transaction,
        IReadOnlyList<DpoPair> Pairs)
        PrepareDurableTransactionFromRepositoryOwnedDataset(
            RepositoryOwnedDpoDatasetAddress address,
            string domain)
    {
        var router = new FeedbackRouter(
            new NullSessionStorageCL(),
            new AcceptingDpoCache(),
            address.PairStoreRoot);
        var prepared = ((IDpoPairTransactionCoordinator)router)
            .PrepareDpoTransaction([domain]);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        var transaction = Assert.IsType<FileDpoPairStore.TransactionHandle>(
            prepared.Value);
        var snapshot = transaction.Read();
        Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
        return (
            transaction,
            snapshot.Value!.Pairs.Select(static value => value.Pair).ToArray());
    }

    private static TrainingDataGovernanceDecision Governance(
        bool licenseAccepted = true,
        bool isQuarantined = false,
        IEnumerable<string>? evidenceRefs = null)
        => new(
            licenseAccepted,
            isQuarantined,
            evidenceRefs ?? [$"sha256:{new string('a', 64)}"]);

    private static DpoTrainingExecutionRequest CreateExecutionRequest(
        TinyTrainingFixtureCL fixture,
        int dpoMaxStepCount,
        string requestId,
        ServerOwnedLegalTrainingTeacherAdmissionAuthority? authority = null)
    {
        var session = fixture.CreateFactory().Create("BUG").Value!;
        var pair = Pair(QwenProvenance(requestId));
        var content = DpoPairTrainingContentAuthority.Project([pair]).Value!;
        var lease = (authority ?? CreateAuthority()).AcquireTrainingLease(
            content,
            [pair.TeacherProvenance]).Value!;
        return new DpoTrainingExecutionRequest(
            "BUG",
            [pair],
            session.Model,
            new LlmModel(session.Model.Config),
            session.Tokenizer,
            // This wiring fixture does not exercise dropout/RNG continuation.
            // Keep production defaults unchanged and satisfy the existing gate.
            IncrementalTrainer.DefaultConfig with { Dropout = 0f },
            Path.Combine(fixture.RootDirectory, $"dpo-{requestId}"),
            lease,
            dpoMaxStepCount);
    }

    private static ContinuousLearningTp1DpoConfiguration CreateTp1Configuration(
        int dpoMaxStepCount = StableDpoTrainingExecutor.DefaultDpoStepCount)
    {
        var result = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray(),
            dpoMaxStepCount);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    [Fact]
    public void EvidenceFreeTp1ConfigurationCreateSignature_FailsClosed()
    {
        var legacy = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            1,
            Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray());

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            legacy.ErrorCode);
        Assert.Null(legacy.Value);
        Assert.Contains(
            typeof(ContinuousLearningTp1DpoConfiguration).GetMethods(),
            method => method.Name == nameof(ContinuousLearningTp1DpoConfiguration.Create)
                      && method.GetParameters().Length == 4);
    }

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority CreateAuthority(
        Func<bool>? isAdmissionLive = null)
    {
        var capability = ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .IssueFromAdmittedConfiguration(
                    CreateTp1Configuration(),
                    isAdmissionLive ?? (static () => true));
        Assert.True(capability.IsAvailable);
        return capability;
    }

    internal static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateAuthorityForPipelineTest()
        => CreateAuthority();

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateAuthorityRevokingAtLiveCheck(
            int liveCheckToRevoke,
            Action? beforeRevocation = null)
    {
        ServerOwnedLegalTrainingTeacherAdmissionAuthority authority = null!;
        var liveCheckCount = 0;
        authority = CreateAuthority(() =>
        {
            liveCheckCount++;
            if (liveCheckCount == liveCheckToRevoke)
            {
                beforeRevocation?.Invoke();
                _ = authority.BeginRevocation();
            }
            return true;
        });
        return authority;
    }

    private static LegalTrainingTeacherAdmissionAuthority CreateRawAuthority()
    {
        var result = LegalTrainingTeacherAdmissionAuthority.Create(
            [new LegalTrainingTeacherPolicyEntry(
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                "qwen3.5:9b",
                new string('d', 64),
                TrainingTeacherModelLicense.Apache20,
                $"sha256:{new string('a', 64)}",
                $"sha256:{new string('b', 64)}")
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = new string('c', 64)
            }],
            maxTeacherCalls: LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static string FindRepositoryFile(params string[] relativePath)
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
                var candidate = Path.Combine(
                    [current.FullName, .. relativePath]);
                if (File.Exists(candidate))
                    return candidate;
                current = current.Parent;
            }
        }

        throw new FileNotFoundException(
            $"Could not find repository file '{Path.Combine(relativePath)}'.");
    }

    private static TrainingTeacherProvenance QwenProvenance(string requestId)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5:9b",
            new string('d', 64),
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = new string('c', 64)
        };
}

public enum DpoExecutorBehavior
{
    Success,
    Failure,
    MissingArtifact,
}

file sealed class RecordingDpoExecutor : IDpoTrainingExecutor
{
    private readonly DpoExecutorBehavior _behavior;
    private readonly int? _actualDpoStepCount;
    private readonly Action? _afterMutation;

    public RecordingDpoExecutor(
        DpoExecutorBehavior behavior,
        int? actualDpoStepCount = null,
        Action? afterMutation = null)
    {
        _behavior = behavior;
        _actualDpoStepCount = actualDpoStepCount;
        _afterMutation = afterMutation;
    }

    public int CallCount { get; private set; }
    public DpoTrainingExecutionRequest? LastRequest { get; private set; }
    public DpoTrainingExecutionArtifact? LastArtifact { get; private set; }
    public List<DpoTrainingExecutionRequest> Requests { get; } = [];

    public Task<OperationResult<DpoTrainingExecutionArtifact>> ExecuteAsync(
        DpoTrainingExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        LastRequest = request;
        Requests.Add(request);
        if (_behavior == DpoExecutorBehavior.Failure)
        {
            return Task.FromResult(OperationResult<DpoTrainingExecutionArtifact>.Fail(
                "Synthetic DPO failure.",
                "SYNTHETIC_DPO_FAILURE"));
        }

        var permit = request.LegalTeacherAdmissionLease.AcquireMutationPermit();
        if (!permit.IsSuccess || permit.Value is null)
        {
            return Task.FromResult(
                permit.MapError<DpoTrainingExecutionArtifact>());
        }

        using (permit.Value)
        {
            var actualDpoStepCount = _actualDpoStepCount ?? request.DpoStepCount;
            var policyPath = Path.Combine(
                request.CheckpointDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(actualDpoStepCount));
            var metricsPath = Path.Combine(
                request.CheckpointDirectory,
                DpoTrainer.BuildMetricsFileName(actualDpoStepCount));
            if (_behavior == DpoExecutorBehavior.Success)
            {
                Directory.CreateDirectory(request.CheckpointDirectory);
                File.WriteAllBytes(policyPath, [1]);
                File.WriteAllBytes(metricsPath, [1]);
            }

            LastArtifact = new DpoTrainingExecutionArtifact(
                request.Pairs.Count,
                actualDpoStepCount,
                policyPath,
                metricsPath,
                request.LegalTeacherAdmissionLease.Admission.PolicyIdentityRef);
        }
        _afterMutation?.Invoke();
        return Task.FromResult(OperationResult<DpoTrainingExecutionArtifact>.Ok(LastArtifact));
    }
}

file sealed class FixedStepLogicalDpoTrainingUnit
    : ILogicalUnit<LogicalDpoTrainingRequest, LogicalDpoTrainingResult>
{
    private readonly int _actualDpoStepCount;
    private readonly Action? _afterMutation;

    public FixedStepLogicalDpoTrainingUnit(
        int actualDpoStepCount,
        Action? afterMutation = null)
    {
        _actualDpoStepCount = actualDpoStepCount;
        _afterMutation = afterMutation;
    }

    public LogicalUnitRequest<LogicalDpoTrainingRequest>? LastRequest { get; private set; }

    public Task<OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>> ExecuteAsync(
        LogicalUnitRequest<LogicalDpoTrainingRequest> request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastRequest = request;
        var permit = request.Input.MutationAdmission?.AcquireMutationPermit()
                     ?? OperationResult<IDisposable>.Fail(
                         "A server-issued DPO mutation admission capability is required.",
                         LogicalDpoMutationAdmissionCapability.RequiredErrorCode);
        if (!permit.IsSuccess || permit.Value is null)
        {
            return Task.FromResult(
                permit.MapError<LogicalUnitResult<LogicalDpoTrainingResult>>());
        }

        string policyPath;
        string metricsPath;
        // This is synthetic publication state for the fixed-step test double,
        // not evidence that its one-byte weights contain a trained model.
        var optimizerState = new TrainingLoop.Optimizers.AdamWPortableState(
            TrainingLoop.Optimizers.AdamWPortableState.CurrentFormatVersion,
            Math.Max(0, _actualDpoStepCount),
            request.Input.PolicyModel.GetCheckpointParameterEntries().Select(entry =>
                new TrainingLoop.Optimizers.AdamWPortableParameterState(entry.Key,
                    entry.Parameter.Value.Shape.Dimensions,
                    new float[entry.Parameter.Value.Data.Length],
                    new float[entry.Parameter.Value.Data.Length])).ToArray());
        var stateBytes = optimizerState.ToBytes();
        var optimizerIdentity = new LogicalDpoTrainingOptimizerStateIdentity(
            optimizerState.FormatVersion, optimizerState.Step,
            Convert.ToHexString(SHA256.HashData(stateBytes)).ToLowerInvariant(), stateBytes.LongLength);
        using (permit.Value)
        {
            Directory.CreateDirectory(request.Input.CheckpointDirectory);
            policyPath = Path.Combine(
                request.Input.CheckpointDirectory,
                DpoTrainer.BuildPolicyCheckpointFileName(_actualDpoStepCount));
            metricsPath = Path.Combine(
                request.Input.CheckpointDirectory,
                DpoTrainer.BuildMetricsFileName(_actualDpoStepCount));
            File.WriteAllBytes(policyPath, [1]);
            File.WriteAllBytes(metricsPath, [1]);
            if (_actualDpoStepCount > 0 && _actualDpoStepCount <= request.Input.MaxStepCount)
                request.Input.PersistPublicationBundle?.Invoke(new DpoTrainingPublicationBundle(
                    optimizerState, null, optimizerState.Step, _actualDpoStepCount));
        }
        _afterMutation?.Invoke();
        var output = new LogicalDpoTrainingResult(
            request.Input.Preferences.Length,
            _actualDpoStepCount,
            LoraAdapterCount: 1,
            LastLoss: 0.1f,
            LastAccuracy: 1f,
            TrainingTimeMs: 1,
            [metricsPath],
            ["11.DPO:DpoTrainer"],
            optimizerState,
            optimizerIdentity);
        var result = new LogicalUnitResult<LogicalDpoTrainingResult>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            output,
            Confidence: 1,
            PredicateIds: ["LEARNING", "DPO"],
            DomainTags: [LogicalDpoTrainingUnit.DomainTag],
            EvidenceIds: [metricsPath],
            Warnings: []);
        return Task.FromResult(
            OperationResult<LogicalUnitResult<LogicalDpoTrainingResult>>.Ok(result));
    }

    private sealed class NoopTestMutationPermit : IDisposable
    {
        internal static readonly NoopTestMutationPermit Instance = new();

        public void Dispose()
        {
        }
    }
}

file sealed class CountingDpoSessionFactory : IIncrementalTrainingSessionFactory
{
    public int CreateCount { get; private set; }

    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        CreateCount++;
        return OperationResult<IncrementalTrainingSession>.Fail(
            "Session creation was not expected.",
            "UNEXPECTED_SESSION_CREATION");
    }
}

file sealed class RecordingDpoSessionFactory : IIncrementalTrainingSessionFactory
{
    private readonly IIncrementalTrainingSessionFactory _inner;
    private readonly Action? _afterCreate;

    public RecordingDpoSessionFactory(
        IIncrementalTrainingSessionFactory inner,
        Action? afterCreate = null)
    {
        _inner = inner;
        _afterCreate = afterCreate;
    }

    public int CreateCount { get; private set; }
    public IncrementalTrainingSession? LastSession { get; private set; }

    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        CreateCount++;
        var result = _inner.Create(domain);
        LastSession = result.IsSuccess ? result.Value : null;
        _afterCreate?.Invoke();
        return result;
    }
}

file sealed class PreTrainingModelRecordingSessionFactory
    : IIncrementalTrainingSessionFactory
{
    private readonly IIncrementalTrainingSessionFactory _inner;

    public PreTrainingModelRecordingSessionFactory(
        IIncrementalTrainingSessionFactory inner)
        => _inner = inner;

    public int CreateCount { get; private set; }
    public List<IncrementalTrainingSession> CreatedSessions { get; } = [];
    public byte[]? CreatedModelBytes { get; private set; }
    public string? CreatedBaseCheckpointSha256 { get; private set; }
    public IncrementalTrainingCheckpointStartMetadata? CreatedCheckpointStart
    {
        get;
        private set;
    }

    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        CreateCount++;
        var result = _inner.Create(domain);
        if (result.IsSuccess && result.Value is not null)
        {
            CreatedSessions.Add(result.Value);
            using var modelBytes = new MemoryStream();
            result.Value.Model.SaveCheckpointToStream(modelBytes);
            CreatedModelBytes = modelBytes.ToArray();
            CreatedBaseCheckpointSha256 = result.Value.BaseCheckpointSha256;
            CreatedCheckpointStart = result.Value.CheckpointStart;
        }

        return result;
    }
}

file sealed class RepositoryOwnedCheckpointDpoSessionFactory
    : IIncrementalTrainingSessionFactory
{
    private readonly TinyTrainingFixtureCL _fixture;
    private readonly RepositoryOwnedModelCheckpointAddress _parentAddress;
    private readonly RepositoryOwnedModelCheckpointManifest _parentManifest;
    private readonly RepositoryOwnedIncrementalTrainingSessionStartRequest
        _startRequest;

    public RepositoryOwnedCheckpointDpoSessionFactory(
        TinyTrainingFixtureCL fixture,
        RepositoryOwnedModelCheckpointAddress parentAddress,
        RepositoryOwnedModelCheckpointManifest parentManifest,
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest)
    {
        _fixture = fixture;
        _parentAddress = parentAddress;
        _parentManifest = parentManifest;
        _startRequest = startRequest;
    }

    public int CreateCount { get; private set; }

    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        _ = domain;
        CreateCount++;
        return IncrementalTrainingSessionBuilder.Build(
            _fixture.ModelConfig,
            _fixture.TrainingConfig,
            () => File.OpenRead(_parentAddress.WeightsPath),
            () => File.OpenRead(_fixture.TokenizerPath),
            parentAdapterCheckpoint: null,
            checkpointStart: IncrementalTrainingCheckpointStartMetadata
                .FromRepository(_parentManifest, _startRequest));
    }
}

file sealed class AcceptingDpoCache : IDomainDatasetCache
{
    public OperationResult<bool> Add(TrainingExample example)
        => OperationResult<bool>.Ok(true);

    public OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain)
        => OperationResult<IReadOnlyList<TrainingExample>>.Ok([]);

    public OperationResult<int> TotalCount()
        => OperationResult<int>.Ok(0);

    public Task<OperationResult<bool>> FlushAsync()
        => Task.FromResult(OperationResult<bool>.Ok(true));
}

file sealed class NullSessionStorageCL : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail(
            "Not persisted.",
            "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
}

file sealed class FixedDpoIndexer : IProjectIndexer
{
    private readonly IReadOnlyList<TrainingExample> _examples;

    public FixedDpoIndexer(IReadOnlyList<TrainingExample> examples)
        => _examples = examples;

    public OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath)
        => OperationResult<IReadOnlyList<TrainingExample>>.Ok(_examples);
}

file sealed class PipelineRecordingTrainer : IIncrementalTrainer,
    ITransactionalIncrementalTrainer
{
    public List<(
        string Domain,
        IReadOnlyList<TrainingExample> Examples,
        IReadOnlyList<DpoPair> Pairs)> Calls { get; } = [];

    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
    {
        Calls.Add((domain, examples, dpoPairs));
        return Task.FromResult(OperationResult<TrainedAdapterArtifact>.Fail(
            "Synthetic stop after routing proof.",
            "SYNTHETIC_TRAINING_STOP"));
    }

    public Task<OperationResult<TrainedAdapterArtifact>> TrainTransactionAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        FileDpoPairStore.TransactionHandle transaction,
        CancellationToken cancellationToken = default)
    {
        var snapshot = transaction.Read();
        Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
        var expectedPairs = snapshot.Value!.Pairs
            .Where(value => StringComparer.Ordinal.Equals(value.Pair.Domain, domain))
            .Select(static value => value.Pair)
            .ToArray();
        Assert.Equal(expectedPairs.Length, dpoPairs.Count);
        for (var index = 0; index < expectedPairs.Length; index++)
            Assert.Equal(expectedPairs[index], dpoPairs[index]);
        return TrainAsync(domain, examples, dpoPairs, cancellationToken);
    }
}

file sealed class UnusedDpoRegistry : ContinuousLearning.IDomainModelRegistry
{
    public OperationResult<bool> Register(ModelAdapter adapter)
        => OperationResult<bool>.Fail("Unused.", "UNUSED");

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
        => OperationResult<ModelAdapter?>.Fail("Unused.", "UNUSED");

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok([]);
}

file sealed class MutatingSupervisedStepExecutor : ISupervisedTrainingStepExecutor
{
    public int CallCount { get; private set; }

    public float Execute(
        IncrementalTrainingSession session,
        IReadOnlyList<int> tokenIds,
        float weight,
        IReadOnlyList<TensorCore.Layers.Parameter> adapterParameters)
    {
        CallCount++;
        var parameter = Assert.Single(adapterParameters.Take(1));
        parameter.Value.Data[0] += 0.125f;
        return 0.5f;
    }
}

file sealed class NonFiniteSupervisedStepExecutor : ISupervisedTrainingStepExecutor
{
    public int CallCount { get; private set; }

    public float Execute(
        IncrementalTrainingSession session,
        IReadOnlyList<int> tokenIds,
        float weight,
        IReadOnlyList<TensorCore.Layers.Parameter> adapterParameters)
    {
        _ = session;
        _ = tokenIds;
        _ = weight;
        _ = adapterParameters;
        CallCount++;
        return float.NaN;
    }
}

file enum AdversarialDpoPhase
{
    CountThrows,
    CountLies,
    GetEnumeratorThrows,
    MoveNextThrows,
    CurrentThrows,
    DisposeThrows,
}

file sealed class AdversarialDpoList : IReadOnlyList<DpoPair>
{
    private readonly DpoPair[] _items;
    private readonly AdversarialDpoPhase _phase;
    private readonly Exception _exception;
    private readonly Exception? _disposeException;

    public AdversarialDpoList(
        DpoPair[] items,
        AdversarialDpoPhase phase,
        Exception? exception = null,
        Exception? disposeException = null)
    {
        _items = items;
        _phase = phase;
        _exception = exception ?? new InvalidOperationException($"synthetic-{phase}");
        _disposeException = disposeException
                            ?? (phase == AdversarialDpoPhase.DisposeThrows
                                ? _exception
                                : null);
    }

    public int DisposeCount { get; private set; }

    public int Count
        => _phase == AdversarialDpoPhase.CountThrows
            ? throw _exception
            : _phase == AdversarialDpoPhase.CountLies
                ? 0
                : _items.Length;

    public DpoPair this[int index] => _items[index];

    public IEnumerator<DpoPair> GetEnumerator()
        => _phase == AdversarialDpoPhase.GetEnumeratorThrows
            ? throw _exception
            : new Enumerator(
                _items,
                _phase,
                _exception,
                _disposeException,
                () => DisposeCount++);

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    private sealed class Enumerator : IEnumerator<DpoPair>
    {
        private readonly DpoPair[] _items;
        private readonly AdversarialDpoPhase _phase;
        private readonly Exception _exception;
        private readonly Exception? _disposeException;
        private readonly Action _onDispose;
        private int _index = -1;

        public Enumerator(
            DpoPair[] items,
            AdversarialDpoPhase phase,
            Exception exception,
            Exception? disposeException,
            Action onDispose)
        {
            _items = items;
            _phase = phase;
            _exception = exception;
            _disposeException = disposeException;
            _onDispose = onDispose;
        }

        public DpoPair Current
            => _phase == AdversarialDpoPhase.CurrentThrows
                ? throw _exception
                : _items[_index];

        object System.Collections.IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_phase == AdversarialDpoPhase.MoveNextThrows)
                throw _exception;
            _index++;
            return _index < _items.Length;
        }

        public void Reset() => _index = -1;

        public void Dispose()
        {
            _onDispose();
            if (_disposeException is not null)
                throw _disposeException;
        }
    }
}

file sealed class SuccessfulPipelineTrainer : IIncrementalTrainer,
    ITransactionalIncrementalTrainer
{
    public List<(string Domain, IReadOnlyList<DpoPair> Pairs)> Calls { get; } = [];

    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResult<TrainedAdapterArtifact>.Fail(
            "A durable transaction is required by this test trainer.",
            "DPO_TRANSACTION_REQUIRED"));

    public async Task<OperationResult<TrainedAdapterArtifact>> TrainTransactionAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        FileDpoPairStore.TransactionHandle transaction,
        CancellationToken cancellationToken = default)
    {
        var snapshot = transaction.Read();
        Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
        var expectedPairs = snapshot.Value!.Pairs
            .Where(value => StringComparer.Ordinal.Equals(value.Pair.Domain, domain))
            .Select(static value => value.Pair)
            .ToArray();
        Assert.Equal(expectedPairs.Length, dpoPairs.Count);
        for (var index = 0; index < expectedPairs.Length; index++)
            Assert.Equal(expectedPairs[index], dpoPairs[index]);

        using var fixture = new TinyTrainingFixtureCL();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageCL(),
            fixture.CreateFactory(),
            IncrementalTrainerDpoWiringTests.CreateAuthorityForPipelineTest(),
            new RecordingDpoExecutor(DpoExecutorBehavior.Success),
            new MutatingSupervisedStepExecutor(),
            fixture.AdapterDirectory);
        Calls.Add((domain, dpoPairs));
        return await ((ITransactionalIncrementalTrainer)trainer)
            .TrainTransactionAsync(
                domain,
                examples,
                dpoPairs,
                transaction,
                cancellationToken);
    }
}

file sealed class MemoryDpoRegistry : ContinuousLearning.IDomainModelRegistry,
    IDpoRegistryPublicationTransaction
{
    private readonly Dictionary<string, ModelAdapter> _adapters =
        new(StringComparer.Ordinal);

    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        _adapters[adapter.Domain] = adapter;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
        => OperationResult<ModelAdapter?>.Ok(
            _adapters.TryGetValue(domain, out var adapter) ? adapter : null);

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok(
            _adapters.Values.ToArray());

    public OperationResult<IReadOnlyList<ModelAdapter>> PublishBatchAndCommit(
        IReadOnlyList<ModelAdapter> adapters,
        Func<IReadOnlyList<ModelAdapter>, OperationResult<bool>> commit)
    {
        foreach (var adapter in adapters)
            _adapters[adapter.Domain] = adapter;
        var committed = commit(adapters);
        if (!committed.IsSuccess || committed.Value is not true)
            return committed.MapError<IReadOnlyList<ModelAdapter>>();
        return OperationResult<IReadOnlyList<ModelAdapter>>.Ok(adapters);
    }
}
