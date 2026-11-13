// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DPO;
using DPO.Models;
using DesignConversionDomain;
using ModelServer.DesignConversion;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using TranslationDomain;
using TranslationDomainTools;
using TrainingLoop;
using TranslationToolsProgram = TranslationDomainTools.Program;

[Collection(ModelServerContinuousLearningEnvironmentCollection.Name)]
public sealed class TranslationDpoModelServerCoordinatorTests
{
    [Fact]
    public async Task Cli_DefaultAndHelpRemainNonTraining()
    {
        var root = CreateTempRoot();
        var output = Path.Combine(root, "must-not-exist.json");
        try
        {
            var noArgs = await InvokeProgramAsync([]);
            var help = await InvokeProgramAsync(
                ["train-translation-dpo", "--help", "--output", output]);

            Assert.Equal(2, noArgs.ExitCode);
            Assert.Equal(0, help.ExitCode);
            Assert.Contains("train-translation-dpo", noArgs.Error);
            Assert.Contains("train-translation-dpo", help.Error);
            Assert.False(File.Exists(output));
            Assert.Empty(Directory.EnumerateFileSystemEntries(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Cli_InactiveConfigurationFailsClosedWithoutTrainingArtifactsAndSanitizesPaths()
    {
        using var fixture = new CoordinatorFixture();
        using var environment = fixture.ApplyEnvironment(
            enabled: false,
            trainingActivationRequested: false);
        var output = Path.Combine(fixture.Root, "private-output-marker.json");
        var checkpoint = Path.Combine(fixture.Root, "private-checkpoint-marker");
        var secretPathMarker = "private-secret-path-marker";
        var invocation = await InvokeProgramAsync(
        [
            "train-translation-dpo",
            "--seed", Path.Combine(fixture.Root, secretPathMarker, "seed.jsonl"),
            "--output", output,
            "--checkpoint-dir", checkpoint,
            "--training-data-artifact-root", fixture.Root,
        ]);

        Assert.Equal(3, invocation.ExitCode);
        Assert.Contains(
            TranslationDpoModelServerCoordinator.TrainingInactiveErrorCode,
            invocation.Error,
            StringComparison.Ordinal);
        Assert.DoesNotContain(output, invocation.Error, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(checkpoint, invocation.Error, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(secretPathMarker, invocation.Error, StringComparison.Ordinal);
        Assert.False(File.Exists(output));
        Assert.False(Directory.Exists(checkpoint));
    }

    [Fact]
    public async Task Cli_ActiveHostUsesExactServerBrokerAndProducesInitialRunnerArtifacts()
    {
        using var fixture = new CoordinatorFixture();
        using var environment = fixture.ApplyEnvironment(
            enabled: true,
            trainingActivationRequested: true);
        var options = fixture.CreateOptions(includeContinuation: false);

        var gatewayInvocationCount = 0;
        ProgramInvocation invocation;
        DesignConversionRetainedDpoPolicyBaseProviderGateway
            .InvocationLeaseAcquiredObserverForTesting = () =>
            {
                Interlocked.Increment(ref gatewayInvocationCount);
                return Task.CompletedTask;
            };
        try
        {
            invocation = await InvokeProgramAsync(
            [
                "train-translation-dpo",
                "--seed", options.SeedCorpusPath,
                "--output", options.OutputPath,
                "--checkpoint-dir", options.CheckpointDirectory,
                "--training-data-artifact-root", options.TrainingDataArtifactRoot,
            ]);
        }
        finally
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = null;
        }

        Assert.True(
            invocation.ExitCode == 0,
            "Expected ModelServer-owned CLI training to pass, but it failed."
            + Environment.NewLine
            + "stderr:"
            + Environment.NewLine
            + invocation.Error
            + Environment.NewLine
            + "stdout:"
            + Environment.NewLine
            + invocation.Output);
        Assert.Equal(1, gatewayInvocationCount);
        Assert.True(string.IsNullOrWhiteSpace(invocation.Error));
        using var output = JsonDocument.Parse(invocation.Output);
        var report = output.RootElement;
        Assert.True(report.GetProperty("passed").GetBoolean());
        Assert.True(report.GetProperty(
            "server_authority_liveness_verified").GetBoolean());
        Assert.True(report.GetProperty(
            "exact_server_authority_identity_verified").GetBoolean());
        Assert.True(report.GetProperty("model_set_created_once").GetBoolean());
        Assert.True(report.GetProperty("initial_training_completed").GetBoolean());
        Assert.False(report.GetProperty("continuation_requested").GetBoolean());
        Assert.False(report.GetProperty(
            "continuation_training_completed").GetBoolean());
        Assert.Equal(JsonValueKind.Null, report.GetProperty("continuation").ValueKind);
        var initial = report.GetProperty("initial");
        Assert.True(initial.GetProperty("checkpoint_written").GetBoolean());
        Assert.True(initial.GetProperty("trainer_metrics_written").GetBoolean());
        var reproducibilityEvidence = initial.GetProperty("reproducibility_evidence");
        Assert.Equal(
            0UL,
            reproducibilityEvidence
                .GetProperty("context")
                .GetProperty("root_initialization_seed")
                .GetUInt64());
        Assert.Equal(
            fixture.ExpectedTrainingConfigurationFingerprint,
            reproducibilityEvidence
                .GetProperty("context")
                .GetProperty("training_configuration_fingerprint")
                .GetString());
        Assert.True(File.Exists(options.OutputPath));
        Assert.True(File.Exists(initial.GetProperty("checkpoint_path").GetString()));
        Assert.True(File.Exists(initial.GetProperty("trainer_metrics_path").GetString()));
        Assert.False(report.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(report.GetProperty("product_ready_claimed").GetBoolean());
        Assert.Equal(
            TranslationDpoModelServerCoordinator.HostBoundary,
            report.GetProperty("host_boundary").GetString());
    }

    [Fact]
    public async Task Cli_RepositoryOwnedModeReusesSealedDpoAndPublishesModelUnderRepository()
    {
        using var fixture = new CoordinatorFixture();
        using var environment = fixture.ApplyEnvironment(
            enabled: true,
            trainingActivationRequested: true);
        var options = fixture.CreateOptions(includeContinuation: false);
        var dataset = fixture.SealRepositoryOwnedDpoDataset(
            "cli-repository-owned");

        var invocation = await InvokeProgramAsync(
        [
            "train-translation-dpo",
            "--seed", options.SeedCorpusPath,
            "--output", options.OutputPath,
            "--checkpoint-dir", options.CheckpointDirectory,
            "--training-data-artifact-root", options.TrainingDataArtifactRoot,
            "--repository-owned-training-root", fixture.RepositoryRoot,
            "--repository-owned-dpo-domain", dataset.Address.DomainSlug,
            "--repository-owned-dpo-design-pattern",
                dataset.Address.DesignPatternSlug,
            "--repository-owned-dpo-dataset-version",
                dataset.Address.DatasetVersion,
            "--repository-owned-model-id", "translation-qwen-local",
            "--repository-owned-model-version", "v1",
            "--repository-owned-runtime-config-sha256",
                dataset.RuntimeCompatibility.ConfigSha256,
            "--repository-owned-runtime-topology-sha256",
                dataset.RuntimeCompatibility.TopologySha256,
            "--repository-owned-runtime-tokenizer-sha256",
                dataset.RuntimeCompatibility.TokenizerSha256,
            "--repository-owned-runtime-trainer-sha256",
                dataset.RuntimeCompatibility.TrainerSha256,
            "--repository-owned-runtime-schema-sha256",
                dataset.RuntimeCompatibility.SchemaSha256,
            "--repository-owned-runtime-compatibility-key",
                dataset.RuntimeCompatibility.CompatibilityKey,
            "--repository-owned-model-cold-start-reason",
                "first repository-owned Translation checkpoint after sealed DPO dataset opened",
            "--repository-owned-provenance-ref",
                "translation-dpo-cli-test"
        ]);

        Assert.True(
            invocation.ExitCode == 0,
            "Expected repository-owned CLI training to pass, but it failed."
            + Environment.NewLine
            + "stderr:"
            + Environment.NewLine
            + invocation.Error
            + Environment.NewLine
            + "stdout:"
            + Environment.NewLine
            + invocation.Output);
        Assert.True(string.IsNullOrWhiteSpace(invocation.Error));
        using var output = JsonDocument.Parse(invocation.Output);
        var initial = output.RootElement.GetProperty("initial");
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedDpoAndModelCheckpointPipeline,
            initial.GetProperty("pipeline").GetString());
        Assert.Equal(
            JsonValueKind.Object,
            initial.GetProperty("repository_owned_dpo_dataset_evidence")
                .ValueKind);
        var modelEvidence = initial.GetProperty(
            "repository_owned_model_checkpoint_evidence");
        Assert.Equal(JsonValueKind.Object, modelEvidence.ValueKind);
        Assert.Equal(
            "cold_start_explicit",
            modelEvidence.GetProperty("start_mode").GetString());
        Assert.Contains(
            "training-artifacts/models/translation/learning-unit/translation-qwen-local/v1/checkpoints/000001/weights.bin",
            modelEvidence.GetProperty("published_weights_relative_path")
                .GetString(),
            StringComparison.Ordinal);
        Assert.True(File.Exists(modelEvidence
            .GetProperty("published_weights_path")
            .GetString()));
        Assert.True(File.Exists(modelEvidence
            .GetProperty("published_manifest_path")
            .GetString()));
        Assert.False(File.Exists(Path.Combine(
            options.CheckpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public async Task ModelSetGuard_RejectsSecondAttemptBeforeUnderlyingProviderCall()
    {
        var inner = new CountingFailingPolicyBaseProvider();
        var guard = new SingleInvocationDpoPolicyBaseModelSetProvider(inner);

        var first = await guard.CreateModelSetAsync(CancellationToken.None);
        var second = await guard.CreateModelSetAsync(CancellationToken.None);

        Assert.False(first.IsSuccess);
        Assert.Equal(CountingFailingPolicyBaseProvider.ErrorCode, first.ErrorCode);
        Assert.False(second.IsSuccess);
        Assert.Equal(
            SingleInvocationDpoPolicyBaseModelSetProvider
                .AlreadyRequestedErrorCode,
            second.ErrorCode);
        Assert.Equal(1, inner.InvocationCount);
        Assert.Equal(1, guard.ObservedInvocationCount);
    }

    [Fact]
    public async Task ActiveHost_RunnerFailureDrainsAndSameCustodyCanRunImmediately()
    {
        using var fixture = new CoordinatorFixture();
        using var environment = fixture.ApplyEnvironment(
            enabled: true,
            trainingActivationRequested: true);
        var validOptions = fixture.CreateOptions(includeContinuation: false);
        var missingSeed = Path.Combine(
            fixture.Root,
            "private-missing-seed-marker.jsonl");
        var invalidOptions = validOptions with { SeedCorpusPath = missingSeed };
        var gatewayInvocationCount = 0;

        OperationResult<TranslationDpoModelServerCoordinatorReport> failed;
        OperationResult<TranslationDpoModelServerCoordinatorReport> recovered;
        var reportAbsentAfterFailure = false;
        var checkpointsAbsentAfterFailure = false;
        var gatewayInvocationsAfterFailure = -1;
        DesignConversionRetainedDpoPolicyBaseProviderGateway
            .InvocationLeaseAcquiredObserverForTesting = () =>
            {
                Interlocked.Increment(ref gatewayInvocationCount);
                return Task.CompletedTask;
            };
        try
        {
            failed = await TranslationDpoModelServerCoordinator.RunAsync(
                invalidOptions);
            reportAbsentAfterFailure = !File.Exists(validOptions.OutputPath);
            checkpointsAbsentAfterFailure =
                !Directory.Exists(validOptions.CheckpointDirectory);
            gatewayInvocationsAfterFailure =
                Volatile.Read(ref gatewayInvocationCount);
            recovered = await TranslationDpoModelServerCoordinator.RunAsync(
                validOptions);
        }
        finally
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = null;
        }

        Assert.False(failed.IsSuccess);
        Assert.Equal(
            TranslationDpoModelServerCoordinator.InitialTrainingFailedErrorCode,
            failed.ErrorCode);
        Assert.DoesNotContain(
            missingSeed,
            failed.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            fixture.Root,
            failed.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.True(reportAbsentAfterFailure);
        Assert.True(checkpointsAbsentAfterFailure);
        Assert.Equal(1, gatewayInvocationsAfterFailure);
        Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
        Assert.True(recovered.Value!.ExactServerAuthorityIdentityVerified);
        Assert.True(recovered.Value.ModelSetCreatedOnce);
        Assert.True(File.Exists(validOptions.OutputPath));
        Assert.True(File.Exists(recovered.Value.Initial.CheckpointPath));
        Assert.Equal(2, gatewayInvocationCount);
    }

    [Fact]
    public async Task PreCancelledRunCreatesNoArtifactsAndSameCustodyCanRun()
    {
        using var fixture = new CoordinatorFixture();
        using var environment = fixture.ApplyEnvironment(
            enabled: true,
            trainingActivationRequested: true);
        var options = fixture.CreateOptions(includeContinuation: false);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var gatewayInvocationCount = 0;

        OperationResult<TranslationDpoModelServerCoordinatorReport> cancelled;
        OperationResult<TranslationDpoModelServerCoordinatorReport> recovered;
        var reportAbsentAfterCancellation = false;
        var checkpointsAbsentAfterCancellation = false;
        var gatewayInvocationsAfterCancellation = -1;
        DesignConversionRetainedDpoPolicyBaseProviderGateway
            .InvocationLeaseAcquiredObserverForTesting = () =>
            {
                Interlocked.Increment(ref gatewayInvocationCount);
                return Task.CompletedTask;
            };
        try
        {
            cancelled = await TranslationDpoModelServerCoordinator.RunAsync(
                options,
                cancellation.Token);
            reportAbsentAfterCancellation = !File.Exists(options.OutputPath);
            checkpointsAbsentAfterCancellation =
                !Directory.Exists(options.CheckpointDirectory);
            gatewayInvocationsAfterCancellation =
                Volatile.Read(ref gatewayInvocationCount);
            recovered = await TranslationDpoModelServerCoordinator.RunAsync(
                options);
        }
        finally
        {
            DesignConversionRetainedDpoPolicyBaseProviderGateway
                .InvocationLeaseAcquiredObserverForTesting = null;
        }

        Assert.False(cancelled.IsSuccess);
        Assert.Equal(
            TranslationDpoModelServerCoordinator.CancelledErrorCode,
            cancelled.ErrorCode);
        Assert.DoesNotContain(
            fixture.Root,
            cancelled.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.True(reportAbsentAfterCancellation);
        Assert.True(checkpointsAbsentAfterCancellation);
        Assert.Equal(0, gatewayInvocationsAfterCancellation);
        Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
        Assert.True(recovered.Value!.ExactServerAuthorityIdentityVerified);
        Assert.True(File.Exists(options.OutputPath));
        Assert.True(File.Exists(recovered.Value.Initial.CheckpointPath));
        Assert.Equal(1, gatewayInvocationCount);
    }

    [Fact]
    public async Task ActiveHost_ContinuationFreshLoadsAndProducesContinuedArtifacts()
    {
        using var fixture = new CoordinatorFixture();
        using var environment = fixture.ApplyEnvironment(
            enabled: true,
            trainingActivationRequested: true);
        var options = fixture.CreateOptions(includeContinuation: true);

        var result = await TranslationDpoModelServerCoordinator.RunAsync(options);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = Assert.IsType<TranslationDpoModelServerCoordinatorReport>(
            result.Value);
        Assert.True(report.ExactServerAuthorityIdentityVerified);
        Assert.True(report.ModelSetCreatedOnce);
        Assert.True(DpoTrainingReproducibilityContextContract.IsValidEvidence(
            report.Initial.ReproducibilityEvidence));
        Assert.True(report.ContinuationRequested);
        Assert.True(report.ContinuationTrainingCompleted);
        var continuation = Assert.IsType<TranslationDpoCheckpointLifecycleRunReport>(
            report.Continuation);
        Assert.Equal(
            report.Initial.ReproducibilityEvidence!.ContextSha256,
            continuation.ReproducibilityEvidence!.ContextSha256);
        Assert.True(continuation.CheckpointFileUsed);
        Assert.True(continuation.PolicyCheckpointLoaded);
        Assert.True(continuation.LoadedOrResumed);
        Assert.True(continuation.ContinuationCheckpointWritten);
        Assert.True(continuation.ContinuationTrainerMetricsWritten);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner.TrainerLoadCallPath,
            continuation.TrainerLoadCallPath);
        Assert.Contains(
            "TransformerModel.LlmModel.LoadCheckpoint",
            continuation.TrainerLoadCallPath,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            string.Concat("DPO.DpoTrainer.", "Load", "Policy", "Checkpoint"),
            continuation.TrainerLoadCallPath,
            StringComparison.Ordinal);
        Assert.True(File.Exists(options.ContinuationOutputPath));
        Assert.True(File.Exists(continuation.ContinuationCheckpointPath));
        Assert.True(File.Exists(continuation.ContinuationTrainerMetricsPath));
        Assert.NotEqual(
            report.Initial.CheckpointPath,
            continuation.ContinuationCheckpointPath);
    }

    [Fact]
    public void ProjectGraph_HasOuterToolReferencesTo25And27WithoutReverseReference()
    {
        var repo = FindRepositoryRoot();
        var toolsProject = File.ReadAllText(Path.Combine(
            repo,
            "27.TranslationDomain.Tools",
            "27.TranslationDomain.Tools.csproj"));
        var modelServerProject = File.ReadAllText(Path.Combine(
            repo,
            "25.ModelServer",
            "ModelServer.csproj"));

        Assert.Contains("..\\25.ModelServer\\ModelServer.csproj", toolsProject);
        Assert.Contains("..\\27.TranslationDomain\\27.TranslationDomain.csproj", toolsProject);
        Assert.DoesNotContain("27.TranslationDomain", modelServerProject);
    }

    [Fact]
    public async Task Cli_StrictParserRejectsUnknownDuplicateAndPartialContinuationWithoutPaths()
    {
        var root = CreateTempRoot();
        try
        {
            var required = new[]
            {
                "train-translation-dpo",
                "--seed", Path.Combine(root, "secret-seed.jsonl"),
                "--output", Path.Combine(root, "secret-output.json"),
                "--checkpoint-dir", Path.Combine(root, "secret-checkpoint"),
                "--training-data-artifact-root", root,
            };
            var unknown = await InvokeProgramAsync(
                [.. required, "--unknown", "secret-value"]);
            var duplicate = await InvokeProgramAsync(
                [.. required, "--seed", Path.Combine(root, "other.jsonl")]);
            var partialContinuation = await InvokeProgramAsync(
                [.. required, "--continuation-output", Path.Combine(root, "continued.json")]);

            foreach (var invocation in new[] { unknown, duplicate, partialContinuation })
            {
                Assert.Equal(2, invocation.ExitCode);
                Assert.Contains(
                    TranslationDpoModelServerCoordinator.InvalidArgumentsErrorCode,
                    invocation.Error,
                    StringComparison.Ordinal);
                Assert.DoesNotContain(root, invocation.Error, StringComparison.OrdinalIgnoreCase);
            }
            Assert.Empty(Directory.EnumerateFileSystemEntries(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ModelSetReproducibilityAdapter_RejectsMissingAndTamperedServerContextBeforeRunner()
    {
        var missing = TranslationDpoModelServerCoordinator
            .AdaptServerReproducibilityContextForTesting(null);
        var tampered = TranslationDpoModelServerCoordinator
            .AdaptServerReproducibilityContextForTesting(
                DesignConversionDpoPolicyRandomContextContract
                    .CreateCurrent(0UL, new string('f', 64)) with
                {
                    ReferenceDropoutStreamDomain =
                        DesignConversionDpoPolicyRandomContextContract
                            .PolicyDropoutStreamDomain
                });

        Assert.False(missing.IsSuccess);
        Assert.Equal(
            TranslationDpoModelServerCoordinator
                .ReproducibilityContextUnavailableErrorCode,
            missing.ErrorCode);
        Assert.False(tampered.IsSuccess);
        Assert.Equal(
            TranslationDpoModelServerCoordinator
                .ReproducibilityContextInvalidErrorCode,
            tampered.ErrorCode);
    }

    private static async Task<ProgramInvocation> InvokeProgramAsync(string[] args)
    {
        var previousOut = Console.Out;
        var previousError = Console.Error;
        using var output = new StringWriter();
        using var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);
        try
        {
            var exitCode = await TranslationToolsProgram.Main(args);
            return new ProgramInvocation(
                exitCode,
                output.ToString(),
                error.ToString());
        }
        finally
        {
            Console.SetOut(previousOut);
            Console.SetError(previousError);
        }
    }

    private static string CreateTempRoot()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"translation-dpo-coordinator-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException();
    }

    private sealed record ProgramInvocation(
        int ExitCode,
        string Output,
        string Error);

    private sealed class CountingFailingPolicyBaseProvider
        : IDesignConversionDpoPolicyBaseProvider
    {
        internal const string ErrorCode = "TEST_INNER_PROVIDER_FAILURE";
        internal int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(CancellationToken cancellationToken)
        {
            InvocationCount++;
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    "Deterministic test provider failure.",
                    ErrorCode));
        }
    }

    private sealed class CoordinatorFixture : IDisposable
    {
        private readonly Dictionary<string, string> _environment;

        internal CoordinatorFixture()
        {
            Root = CreateTempRoot();
            BaseCheckpoint = Path.Combine(Root, "base.bin");
            TokenizerPath = Path.Combine(Root, "tokenizer.bin");
            AdapterRoot = Path.Combine(Root, "adapter");
            CacheRoot = Path.Combine(Root, "cache");
            RegistryRoot = Path.Combine(Root, "registry");
            RepositoryRoot = Path.Combine(Root, "repository-root");
            CreateRepositorySentinels(RepositoryRoot);
            PairRoot = Path.Combine(
                RepositoryRoot,
                "training-data",
                "dpo",
                "translation",
                "learning-unit",
                "v0001",
                "pairs");
            ModelConfiguration = new ModelConfig
            {
                VocabSize = 256,
                ContextLen = 8,
                HiddenDim = 8,
                NumLayers = 1,
                NumHeads = 1,
                NumKvHeads = 1,
                FfnHiddenDim = 16,
                RopeTheta = 10000f,
                RmsNormEps = 1e-5f,
            };
            new LlmModel(ModelConfiguration).SaveCheckpoint(BaseCheckpoint);
            var tokenizer = new BpeTokenizer();
            tokenizer.Train(
                "translation dpo modelserver coordinator fixture",
                ModelConfiguration.VocabSize);
            tokenizer.Save(TokenizerPath);
            Directory.CreateDirectory(AdapterRoot);
            Directory.CreateDirectory(CacheRoot);
            Directory.CreateDirectory(RegistryRoot);
            Directory.CreateDirectory(PairRoot);

            _environment = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                    = bool.TrueString,
                [ContinuousLearningModelServerConfigurationLoader.ModelIdEnvironmentName]
                    = "qwen3.5:9b",
                [ContinuousLearningModelServerConfigurationLoader.OllamaManifestDigestEnvironmentName]
                    = new string('d', 64),
                [ContinuousLearningModelServerConfigurationLoader.OllamaModelLayerDigestEnvironmentName]
                    = new string('c', 64),
                [ContinuousLearningModelServerConfigurationLoader.BaseModelIdEnvironmentName]
                    = "registry/qwen3.5-base-9b",
                [ContinuousLearningModelServerConfigurationLoader.BaseModelRevisionEnvironmentName]
                    = "base-checkpoint-coordinator-r1",
                [ContinuousLearningModelServerConfigurationLoader.MaxTeacherCallsEnvironmentName]
                    = "100",
                [ContinuousLearningModelServerConfigurationLoader.AuditSaltEnvironmentName]
                    = Convert.ToBase64String(Enumerable.Range(1, 32)
                        .Select(static value => checked((byte)value))
                        .ToArray()),
                [ContinuousLearningModelServerConfigurationLoader.LocalRetentionPolicyRefEnvironmentName]
                    = $"sha256:{new string('a', 64)}",
                [ContinuousLearningModelServerConfigurationLoader.OutputLicenseRefEnvironmentName]
                    = $"sha256:{new string('b', 64)}",
                [ContinuousLearningModelServerConfigurationLoader.BaseCheckpointEnvironmentName]
                    = BaseCheckpoint,
                [ContinuousLearningModelServerConfigurationLoader.TokenizerEnvironmentName]
                    = TokenizerPath,
                [ContinuousLearningModelServerConfigurationLoader.AdapterRootEnvironmentName]
                    = AdapterRoot,
                [ContinuousLearningModelServerConfigurationLoader.CacheRootEnvironmentName]
                    = CacheRoot,
                [ContinuousLearningModelServerConfigurationLoader.RegistryRootEnvironmentName]
                    = RegistryRoot,
                [ContinuousLearningModelServerConfigurationLoader.RepositoryRootEnvironmentName]
                    = RepositoryRoot,
                [ContinuousLearningModelServerConfigurationLoader.DpoPairRootEnvironmentName]
                    = PairRoot,
                [ContinuousLearningModelServerConfigurationLoader.RepositoryModelIdEnvironmentName]
                    = "translation-qwen-local",
                [ContinuousLearningModelServerConfigurationLoader.RepositoryModelVersionEnvironmentName]
                    = "v1",
                [ContinuousLearningModelServerConfigurationLoader.RuntimeConfigSha256EnvironmentName]
                    = ComputeTextSha256("fixture-runtime-config"),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeTopologySha256EnvironmentName]
                    = ComputeTextSha256("fixture-runtime-topology"),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeTokenizerSha256EnvironmentName]
                    = ComputeTextSha256("fixture-runtime-tokenizer"),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeTrainerSha256EnvironmentName]
                    = ComputeTextSha256("fixture-runtime-trainer"),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeSchemaSha256EnvironmentName]
                    = ComputeTextSha256("fixture-runtime-schema"),
                [ContinuousLearningModelServerConfigurationLoader.RuntimeCompatibilityKeyEnvironmentName]
                    = "translation-dpo-fixture-runtime",
                [ContinuousLearningModelServerConfigurationLoader.ModelVocabSizeEnvironmentName]
                    = "256",
                [ContinuousLearningModelServerConfigurationLoader.ModelContextLenEnvironmentName]
                    = "8",
                [ContinuousLearningModelServerConfigurationLoader.ModelHiddenDimEnvironmentName]
                    = "8",
                [ContinuousLearningModelServerConfigurationLoader.ModelNumLayersEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.ModelNumHeadsEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.ModelNumKvHeadsEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.ModelFfnHiddenDimEnvironmentName]
                    = "16",
                [ContinuousLearningModelServerConfigurationLoader.ModelRopeThetaEnvironmentName]
                    = "10000",
                [ContinuousLearningModelServerConfigurationLoader.ModelRmsNormEpsEnvironmentName]
                    = "1E-05",
                [ContinuousLearningModelServerConfigurationLoader.TrainingBatchSizeEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingSeqLenEnvironmentName]
                    = "8",
                [ContinuousLearningModelServerConfigurationLoader.TrainingMaxLrEnvironmentName]
                    = "0.01",
                [ContinuousLearningModelServerConfigurationLoader.TrainingMinLrEnvironmentName]
                    = "0.001",
                [ContinuousLearningModelServerConfigurationLoader.TrainingWarmupStepsEnvironmentName]
                    = "0",
                [ContinuousLearningModelServerConfigurationLoader.TrainingTotalStepsEnvironmentName]
                    = "10",
                [ContinuousLearningModelServerConfigurationLoader.TrainingGradAccumStepsEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingGradClipNormEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingWeightDecayEnvironmentName]
                    = "0",
                [ContinuousLearningModelServerConfigurationLoader.TrainingCheckpointEveryEnvironmentName]
                    = "1",
                [ContinuousLearningModelServerConfigurationLoader.TrainingLogEveryEnvironmentName]
                    = "1",
            };
        }

        internal string Root { get; }
        internal string BaseCheckpoint { get; }
        internal string TokenizerPath { get; }
        internal string AdapterRoot { get; }
        internal string CacheRoot { get; }
        internal string RegistryRoot { get; }
        internal string RepositoryRoot { get; }
        internal string PairRoot { get; }
        internal ModelConfig ModelConfiguration { get; }
        internal string ExpectedTrainingConfigurationFingerprint =>
            ContinuousLearningTrainingConfigurationFingerprint.Compute(
                new TrainingConfig
                {
                    BatchSize = 1,
                    SeqLen = 8,
                    MaxLr = 0.01f,
                    MinLr = 0.001f,
                    WarmupSteps = 0,
                    TotalSteps = 10,
                    GradAccumSteps = 1,
                    GradClipNorm = 1,
                    WeightDecay = 0,
                    CheckpointEvery = 1,
                    LogEvery = 1,
                    RootInitializationSeed = 0UL
                });

        internal TranslationDpoModelServerCoordinatorOptions CreateOptions(
            bool includeContinuation)
        {
            var repo = FindRepositoryRoot();
            return new TranslationDpoModelServerCoordinatorOptions(
                Path.Combine(
                    repo,
                    "27.TranslationDomain",
                    "SeedData",
                    "language-translate-seed-v1.jsonl"),
                Path.Combine(Root, "reports", "initial.json"),
                Path.Combine(Root, "initial-checkpoints"),
                repo,
                includeContinuation
                    ? Path.Combine(Root, "reports", "continuation.json")
                    : null,
                includeContinuation
                    ? Path.Combine(Root, "continuation-checkpoints")
                    : null);
        }

        internal SealedRepositoryOwnedDpoDataset SealRepositoryOwnedDpoDataset(
            string scope)
        {
            var address = RepositoryOwnedTrainingArtifactLayout
                .CreateDpoDatasetAddress(
                    RepositoryRoot,
                    "translation",
                    "learning-unit",
                    "v0001");
            Assert.True(address.IsSuccess, address.ErrorMessage);

            var requestIdentity = CreateRepositoryOwnedDpoRequestIdentity(
                scope);
            // WHY: the CLI test must exercise a real train/evaluate run, not a
            // single-row shortcut. Two deterministic pairs keep the fixture tiny
            // while giving the trainer enough data to publish the repository
            // checkpoint through the same path used by production runs.
            var manifest = RepositoryOwnedDpoDatasetManifestSealer
                .PersistAndSeal(
                    address.Value!,
                    requestIdentity,
                    [
                        CreateRepositoryOwnedDpoPair(
                            address.Value!,
                            requestIdentity,
                            scope + "-first"),
                        CreateRepositoryOwnedDpoPair(
                            address.Value!,
                            requestIdentity,
                            scope + "-second")
                    ]);
            Assert.True(manifest.IsSuccess, manifest.ErrorMessage);

            return new SealedRepositoryOwnedDpoDataset(
                address.Value!,
                manifest.Value!,
                CreateRepositoryOwnedModelRuntimeCompatibility(scope));
        }

        internal IDisposable ApplyEnvironment(
            bool enabled,
            bool trainingActivationRequested)
        {
            var values = new Dictionary<string, string>(
                _environment,
                StringComparer.Ordinal)
            {
                [ContinuousLearningModelServerConfigurationLoader.EnabledEnvironmentName]
                    = enabled.ToString(),
                [ContinuousLearningModelServerConfigurationLoader.TrainingActivationEnabledEnvironmentName]
                    = trainingActivationRequested.ToString(),
            };
            if (trainingActivationRequested)
            {
                values[ContinuousLearningModelServerConfigurationLoader
                    .TrainingRootInitializationSeedEnvironmentName] = "0";
            }

            return new EnvironmentScope(values);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }

        private static void CreateRepositorySentinels(string repositoryRoot)
        {
            Directory.CreateDirectory(repositoryRoot);
            File.WriteAllText(
                Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
                "fixture solution sentinel");
            Directory.CreateDirectory(Path.Combine(repositoryRoot, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
        }

        private static RepositoryOwnedDpoDatasetRequestIdentity
            CreateRepositoryOwnedDpoRequestIdentity(string scope)
        {
            var identity = RepositoryOwnedDpoDatasetManifestSealer
                .CreateRequestIdentity(
                    [ComputeTextSha256("paid-reference-" + scope)],
                    new RepositoryOwnedDpoOpenTeacherIdentity(
                        OpenWeightTeacherFamily.Qwen.ToString(),
                        "qwen-test:9b",
                        new string('c', 64),
                        LegalTrainingTeacherAdmissionAuthority
                            .OllamaModelLayerSha256DigestKind,
                        new string('d', 64)),
                    new RepositoryOwnedDpoGenerationPolicyIdentity(
                        "translation-domain-tools",
                        "v1",
                        ComputeTextSha256("generation-policy-" + scope)),
                    new RepositoryOwnedDpoCreatorIdentity(
                        "translation-domain-tools",
                        "v1",
                        ComputeTextSha256("creator-policy-" + scope)));
            Assert.True(identity.IsSuccess, identity.ErrorMessage);
            return identity.Value!;
        }

        private static RepositoryOwnedModelRuntimeCompatibility
            CreateRepositoryOwnedModelRuntimeCompatibility(string scope)
            => new(
                ConfigSha256: ComputeTextSha256("model-config-" + scope),
                TopologySha256: ComputeTextSha256("model-topology-" + scope),
                TokenizerSha256: ComputeTextSha256("model-tokenizer-" + scope),
                TrainerSha256: ComputeTextSha256("model-trainer-" + scope),
                SchemaSha256: ComputeTextSha256("model-schema-" + scope),
                CompatibilityKey: "translation-dpo-" +
                    ComputeTextSha256("compatibility-key-" + scope)[..12]);

        private static ContinuousLearning.DpoPair CreateRepositoryOwnedDpoPair(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            string scope)
        {
            var passport = new TrainingTeacherProvenance(
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
                "translation-domain-tools-" +
                ComputeTextSha256("teacher-request-" + scope),
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    identity.OpenTeacher.ModelArtifactDigestKind,
                ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
            };

            var pair = new ContinuousLearning.DpoPair(
                Prompt: "Translate from en to ru.\nInput: deterministic source text for " + scope,
                Chosen: "детерминированный правильный перевод для " + scope,
                Rejected: "WRONG[fixture]: deterministic rejected translation for " + scope,
                Domain: address.DomainSlug,
                CapturedAt: DateTimeOffset.UnixEpoch)
            {
                TeacherProvenance = passport,
                GovernanceDecision = new TrainingDataGovernanceDecision(
                    licenseAccepted: true,
                    isQuarantined: false,
                    evidenceRefs:
                    [
                        "sha256:" + ComputeTextSha256(
                            "governance-" + scope)
                    ]),
                ChosenAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.OpenTeacher,
                    passport),
                RejectedAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.LocalModel)
            };
            var bound = DpoPairTrainingContentAuthority.Bind(pair);
            Assert.True(bound.IsSuccess, bound.ErrorMessage);
            return bound.Value!;
        }

        private static string ComputeTextSha256(string text)
            => Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    private sealed record SealedRepositoryOwnedDpoDataset(
        RepositoryOwnedDpoDatasetAddress Address,
        RepositoryOwnedDpoDatasetManifest Manifest,
        RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility);

    private sealed class EnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> _prior =
            new(StringComparer.Ordinal);

        internal EnvironmentScope(IReadOnlyDictionary<string, string> values)
        {
            foreach (var (name, value) in values)
            {
                _prior[name] = Environment.GetEnvironmentVariable(name);
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        public void Dispose()
        {
            foreach (var (name, value) in _prior)
                Environment.SetEnvironmentVariable(name, value);
        }
    }
}
