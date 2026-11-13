namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using TransformerModel;
using TransformerModel.Config;
using TranslationDomain;

public sealed class TranslationDpoTrainEvaluateRunnerTests : IDisposable
{
    private const string SeedFileName = "language-translate-seed-v1.jsonl";
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly string _tempRoot = Path.Combine(
        Path.GetTempPath(),
        $"translation-dpo-train-evaluate-runner-{Guid.NewGuid():N}");
    private readonly string _checkpointDir;
    private readonly List<string> _directoryLinksToRemove = new();

    public TranslationDpoTrainEvaluateRunnerTests()
    {
        Directory.CreateDirectory(_tempRoot);
        _checkpointDir = Path.Combine(_tempRoot, "checkpoints");
    }

    public void Dispose()
    {
        foreach (var directoryLink in _directoryLinksToRemove.AsEnumerable().Reverse())
            RemoveDirectoryLinkIfPresent(directoryLink);

        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Fact]
    public void BrokerlessEntryPoints_AreObsoleteWarningsAndBrokeredEntryPointsRemainSupported()
    {
        var runnerType = typeof(TranslationDpoTrainEvaluateRunner);
        var brokerlessRun = runnerType.GetMethod(
            nameof(TranslationDpoTrainEvaluateRunner.Run),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types:
            [
                typeof(string),
                typeof(DpoTrainer),
                typeof(TranslationDpoTrainEvaluateRunnerOptions)
            ],
            modifiers: null)!;
        var brokeredRun = runnerType.GetMethod(
            nameof(TranslationDpoTrainEvaluateRunner.Run),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types:
            [
                typeof(string),
                typeof(Func<LlmModel>),
                typeof(Func<LlmModel>),
                typeof(Func<LlmModel>),
                typeof(IServerOwnedDpoTrainingLeaseBroker),
                typeof(TranslationDpoTrainEvaluateRunnerOptions)
            ],
            modifiers: null)!;
        var legacyTwoModelBrokeredRun = runnerType.GetMethod(
            nameof(TranslationDpoTrainEvaluateRunner.Run),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types:
            [
                typeof(string),
                typeof(Func<LlmModel>),
                typeof(Func<LlmModel>),
                typeof(IServerOwnedDpoTrainingLeaseBroker),
                typeof(TranslationDpoTrainEvaluateRunnerOptions)
            ],
            modifiers: null)!;
        var brokerlessLifecycle = runnerType.GetMethod(
            nameof(TranslationDpoTrainEvaluateRunner.RunFreshLoadContinueEvaluate),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types:
            [
                typeof(string),
                typeof(TranslationDpoTrainEvaluateRunReport),
                typeof(Func<LlmModel>),
                typeof(Func<LlmModel>),
                typeof(TranslationDpoCheckpointLifecycleRunnerOptions)
            ],
            modifiers: null)!;
        var brokeredLifecycle = runnerType.GetMethod(
            nameof(TranslationDpoTrainEvaluateRunner.RunFreshLoadContinueEvaluate),
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types:
            [
                typeof(string),
                typeof(TranslationDpoTrainEvaluateRunReport),
                typeof(Func<LlmModel>),
                typeof(Func<LlmModel>),
                typeof(IServerOwnedDpoTrainingLeaseBroker),
                typeof(TranslationDpoCheckpointLifecycleRunnerOptions)
            ],
            modifiers: null)!;

        var runObsolete = Assert.Single(
            brokerlessRun.GetCustomAttributes<ObsoleteAttribute>());
        var lifecycleObsolete = Assert.Single(
            brokerlessLifecycle.GetCustomAttributes<ObsoleteAttribute>());
        Assert.False(runObsolete.IsError);
        Assert.False(lifecycleObsolete.IsError);
        Assert.Contains(
            nameof(IServerOwnedDpoTrainingLeaseBroker),
            runObsolete.Message,
            StringComparison.Ordinal);
        Assert.Contains(
            nameof(IServerOwnedDpoTrainingLeaseBroker),
            lifecycleObsolete.Message,
            StringComparison.Ordinal);
        Assert.Contains("fails closed", runObsolete.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fails closed", lifecycleObsolete.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(brokeredRun.GetCustomAttributes<ObsoleteAttribute>());
        Assert.Empty(brokeredLifecycle.GetCustomAttributes<ObsoleteAttribute>());
        Assert.NotNull(legacyTwoModelBrokeredRun.GetCustomAttribute<ObsoleteAttribute>());
        Assert.Null(runnerType.GetCustomAttribute<ObsoleteAttribute>());

        var coordinatorSource = File.ReadAllText(Path.Combine(
                GetRepoRoot(),
                "27.TranslationDomain.Tools",
                "TranslationDpoModelServerCoordinator.cs"))
            .Replace("\r\n", "\n");
        Assert.Contains(
            "TranslationDpoTrainEvaluateRunner.RunAsync(\n                options.SeedCorpusPath,\n                models.TakeInitialPolicy,\n                models.TakeInitialReference,\n                models.TakeFreshLoadTarget,\n                broker,",
            coordinatorSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "options.RepositoryOwnedModelCheckpointContext),\n                cancellationToken)",
            coordinatorSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "models.TakePreparedFreshLoadTargetForContinuation,\n                    models.TakeContinuationReference,\n                    broker,\n                    new TranslationDpoCheckpointLifecycleRunnerOptions(",
            coordinatorSource,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Run_ExecutesRealTrainThenHeldOutEvaluateAndWritesParseableMetricsReport()
    {
        var outputPath = Path.Combine(_tempRoot, "reports", "translation-dpo-run-report.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            broker,
            BuildOptions(outputPath));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.True(File.Exists(outputPath));
        Assert.Equal(TranslationDpoTrainEvaluateRunner.Pipeline, report.Pipeline);
        Assert.Equal(16, report.SourceCorpusRowCount);
        Assert.Equal(12, report.TrainSourceRowCount);
        Assert.Equal(4, report.HeldOutSourceRowCount);
        Assert.True(report.TrainingDataArtifactEvidence.LegalCommonDpoManifestValidated);
        Assert.Equal(TranslationTrainingDataContract.ManifestRepoPath, report.TrainingDataArtifactEvidence.ManifestRepoPath);
        Assert.Equal(12, report.TrainingDataArtifactEvidence.CorpusRowsCount);
        Assert.Equal(12, report.TrainingDataArtifactEvidence.ManifestTrainingPairsCount);
        Assert.Equal("ACCEPT", report.TrainingDataArtifactEvidence.LegalVerdict);
        Assert.True(report.TrainingDataArtifactEvidence.CommonTrainReady);
        Assert.True(report.TrainingDataArtifactEvidence.DpoTrainReady);
        Assert.True(File.Exists(report.TrainingDataArtifactEvidence.ManifestPath));
        Assert.True(File.Exists(report.TrainingDataArtifactEvidence.CorpusPath));
        Assert.Equal(12, report.TrainPairCount);
        Assert.Equal(4, report.HeldOutCount);
        Assert.Equal(4, report.HeldOutExcludedFromTrainingCount);
        Assert.Equal(12, report.TrainExcludedFromHeldOutEvaluationCount);
        Assert.False(float.IsNaN(report.TrainMetrics.Loss));
        Assert.False(float.IsInfinity(report.TrainMetrics.Loss));
        Assert.False(float.IsNaN(report.HeldOutMetrics.Loss));
        Assert.False(float.IsInfinity(report.HeldOutMetrics.Loss));
        Assert.True(report.TrainerMetricsWritten);
        Assert.True(File.Exists(report.TrainerMetricsPath));
        Assert.True(report.CheckpointWritten);
        Assert.True(File.Exists(report.CheckpointPath));
        Assert.NotNull(report.ReproducibilityEvidence);
        Assert.True(DpoTrainingReproducibilityContextContract.IsValidEvidence(
            report.ReproducibilityEvidence));
        Assert.NotNull(report.SeededLoraTopologyEvidence);
        Assert.True(DpoTrainingSeededLoraTopologyContract.TryValidateApplicationEvidence(
            report.SeededLoraTopologyEvidence,
            report.ReproducibilityEvidence!.Context,
            out var topologyEvidenceReason),
            topologyEvidenceReason);
        Assert.Equal(
            ComputeSha256(report.CheckpointPath!),
            report.PolicyCheckpointSha256);
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000002.json")));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(2))));
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.SemanticQualityReadinessClaimed);
        Assert.Contains("semantic-quality readiness remains closed", report.SemanticQualityReadinessClosedReason, StringComparison.Ordinal);
        Assert.True(report.LegalProvenanceValidated);
        Assert.True(report.ClosedTeacherRejected);
        Assert.False(report.HeldOutRowsPassedToStepMany);
        Assert.False(report.TrainRowsUsedForHeldOutEvaluation);
        Assert.Equal(TranslationDpoTrainerBridge.TrainerCallPath, report.TrainerCallPath);
        Assert.Equal(TranslationDpoTrainerBridge.EvaluationCallPath, report.EvaluationCallPath);
        Assert.Empty(report.TrainedPairIds.Intersect(report.HeldOutEvaluationPairIds, StringComparer.Ordinal));
        Assert.Empty(report.HeldOutEvaluationPairIds.Intersect(report.ExcludedTrainPairIds, StringComparer.Ordinal));
        Assert.NotNull(broker.LastTrainingContent);
        Assert.Equal(12, broker.LastTrainingContent!.Rows.Count);
        Assert.All(
            broker.LastTrainingContent.Rows,
            row =>
            {
                Assert.StartsWith(
                    "translation-dpo-final-token-tensors-v2\n",
                    row.Context,
                    StringComparison.Ordinal);
                Assert.StartsWith("chosen_ids;f32;shape=", row.Prompt, StringComparison.Ordinal);
                Assert.StartsWith("chosen_mask;f32;shape=", row.Chosen, StringComparison.Ordinal);
                Assert.StartsWith("rejected_ids;f32;shape=", row.Rejected, StringComparison.Ordinal);
                Assert.Contains("\nrejected_mask;f32;shape=", row.Rejected, StringComparison.Ordinal);
                Assert.Contains("vocab_size=512", row.Context, StringComparison.Ordinal);
                Assert.Contains("context_len=64", row.Context, StringComparison.Ordinal);
                Assert.DoesNotContain("project-wiki", row.Context, StringComparison.Ordinal);
            });
        Assert.StartsWith("translation-initial-", broker.LastTrainingRunId, StringComparison.Ordinal);

        using var reportJson = JsonDocument.Parse(File.ReadAllText(outputPath, Utf8NoBom));
        Assert.Equal(12, reportJson.RootElement.GetProperty("train_pair_count").GetInt32());
        Assert.Equal(4, reportJson.RootElement.GetProperty("held_out_count").GetInt32());
        Assert.False(reportJson.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.False(reportJson.RootElement.GetProperty("semantic_quality_readiness_claimed").GetBoolean());
        Assert.True(reportJson.RootElement.GetProperty("trainer_metrics_written").GetBoolean());
        Assert.True(reportJson.RootElement.GetProperty("checkpoint_written").GetBoolean());
        Assert.Equal(1, reportJson.RootElement.GetProperty("train_metrics").GetProperty("step").GetInt32());
        Assert.Equal(2, reportJson.RootElement.GetProperty("held_out_metrics").GetProperty("step").GetInt32());
        var artifactEvidence = reportJson.RootElement.GetProperty("training_data_artifact_evidence");
        Assert.True(artifactEvidence.GetProperty("legal_common_dpo_manifest_validated").GetBoolean());
        Assert.Equal(12, artifactEvidence.GetProperty("manifest_training_pairs_count").GetInt32());
        Assert.Equal(report.TrainingDataArtifactEvidence.CorpusSha256, artifactEvidence.GetProperty("corpus_sha256").GetString());
        var reproducibilityEvidence = reportJson.RootElement.GetProperty("reproducibility_evidence");
        Assert.Equal(
            report.ReproducibilityEvidence!.ContextSha256,
            reproducibilityEvidence.GetProperty("context_sha256").GetString());
        Assert.Equal(
            report.PolicyCheckpointSha256,
            reportJson.RootElement.GetProperty("policy_checkpoint_sha256").GetString());
        Assert.Equal(
            report.SeededLoraTopologyEvidence.TopologySha256,
            reportJson.RootElement
                .GetProperty("seeded_lora_topology_evidence")
                .GetProperty("topology_sha256")
                .GetString());

        using var trainerMetricsJson = JsonDocument.Parse(File.ReadAllText(report.TrainerMetricsPath!, Utf8NoBom));
        Assert.Equal(1, trainerMetricsJson.RootElement.GetProperty("Step").GetInt32());
    }

    [Fact]
    public void Run_ConsumesRepoLocalTrainingDataArtifactAndKeepsSemanticReadinessClosed()
    {
        var outputPath = Path.Combine(_tempRoot, "reports", "translation-dpo-repo-artifact-run-report.json");
        var repoRoot = GetRepoRoot();

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(outputPath, trainingDataArtifactRoot: repoRoot));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.True(report.TrainingDataArtifactEvidence.LegalCommonDpoManifestValidated);
        Assert.Equal(repoRoot, report.TrainingDataArtifactEvidence.ArtifactRoot);
        Assert.Equal(TranslationTrainingDataContract.ManifestRepoPath, report.TrainingDataArtifactEvidence.ManifestRepoPath);
        Assert.Equal(12, report.TrainingDataArtifactEvidence.CorpusRowsCount);
        Assert.Equal(12, report.TrainingDataArtifactEvidence.ManifestTrainingPairsCount);
        Assert.Equal(12, report.TrainPairCount);
        Assert.Equal(4, report.HeldOutCount);
        Assert.True(report.TrainingDataArtifactEvidence.CommonTrainReady);
        Assert.True(report.TrainingDataArtifactEvidence.DpoTrainReady);
        Assert.Equal("ACCEPT", report.TrainingDataArtifactEvidence.LegalVerdict);
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.SemanticQualityReadinessClaimed);
        Assert.False(report.HeldOutRowsPassedToStepMany);
        Assert.False(report.TrainRowsUsedForHeldOutEvaluation);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public void Run_RepositoryOwnedExactSealedHitReusesTrainingPairsWithoutLegacyArtifactCreation()
    {
        var identity = PrepareRepositoryOwnedRequestIdentityFromRepoSeed(
            "exact-hit",
            maxPairsToAuthor: 2);
        var context = CreateRepositoryOwnedDpoDatasetContext(
            "exact-hit",
            identity);
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            context,
            CreateRepositoryOwnedDpoPairs(context, "exact-hit", count: 2));
        var missingArtifactRoot = Path.Combine(
            _tempRoot,
            "missing-artifact-root-exact-hit");
        var checkpointDirectory = Path.Combine(
            _tempRoot,
            "repository-owned-exact-hit-checkpoints");
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "repository-owned-exact-hit.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var teacherFactoryCalls = 0;
        context = context with
        {
            CleanMissAuthoringContext = CreateAuthoringContext(
                context.RequestIdentity!,
                () =>
                {
                    teacherFactoryCalls++;
                    return OperationResult<ITrainingTeacherInferenceClient>.Ok(
                        new RecordingTranslationDpoTeacher(
                            identity));
                },
                maxPairsToAuthor: 2)
        };

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            broker,
            BuildOptions(outputPath, checkpointDirectory, missingArtifactRoot)
                with
                {
                    RepositoryOwnedDpoDatasetContext = context
                });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedDpoTrainingPairsPipeline,
            report.Pipeline);
        Assert.NotNull(report.RepositoryOwnedDpoDatasetEvidence);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset.ToString(),
            report.RepositoryOwnedDpoDatasetEvidence!.ResolutionSource);
        Assert.False(
            report.RepositoryOwnedDpoDatasetEvidence
                .LegacyTrainingDataArtifactLoaded);
        Assert.Equal(
            sealedDataset.Dataset.Manifest.ManifestSha256,
            report.RepositoryOwnedDpoDatasetEvidence.ManifestSha256);
        Assert.Equal(2, report.TrainPairCount);
        Assert.Equal(2, report.TrainSourceRowCount);
        Assert.Equal(2, broker.LastTrainingContent!.Rows.Count);
        Assert.Equal(0, teacherFactoryCalls);
        Assert.False(Directory.Exists(missingArtifactRoot));
        Assert.True(File.Exists(report.CheckpointPath));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointColdStartRequiresReasonAndPublishesFirstSealedCheckpoint()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-cold-start");
        SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-cold-start",
                count: 2));
        var deniedModelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-cold-start",
            allowColdStart: false);
        var deniedCheckpointDirectory = Path.Combine(
            _tempRoot,
            "model-cold-start-denied-checkpoints");
        var deniedOutputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-cold-start-denied.json");

        var denied = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(deniedOutputPath, deniedCheckpointDirectory) with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = deniedModelContext
            });

        Assert.False(denied.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointColdStartNotAuthorizedErrorCode,
            denied.ErrorCode);
        Assert.False(File.Exists(deniedOutputPath));
        Assert.False(File.Exists(Path.Combine(
            deniedCheckpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));

        var coldStartReason =
            "first translation dpo model checkpoint for this exact temp fixture identity";
        var allowedModelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-cold-start",
            allowColdStart: true,
            coldStartReason: coldStartReason);
        var decoyCheckpointDirectory = Path.Combine(
            _tempRoot,
            "arbitrary-decoy-checkpoint-authority");
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-cold-start-allowed.json");

        var allowed = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(outputPath, decoyCheckpointDirectory) with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = allowedModelContext
            });

        Assert.True(allowed.IsSuccess, allowed.ErrorMessage);
        var report = allowed.Value!;
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedDpoAndModelCheckpointPipeline,
            report.Pipeline);
        Assert.NotNull(report.RepositoryOwnedModelCheckpointEvidence);
        var evidence = report.RepositoryOwnedModelCheckpointEvidence!;
        Assert.Equal(
            RepositoryOwnedTrainingArtifactLayout.ModelArtifactRootRelativePath,
            evidence.Authority);
        Assert.Equal("cold_start_explicit", evidence.StartMode);
        Assert.Equal(1, evidence.PublishedStep);
        Assert.Null(evidence.ParentStep);
        Assert.Equal(coldStartReason, evidence.ColdStartReason);
        Assert.StartsWith(
            dpoContext.Address!.RepositoryRoot,
            evidence.PublishedManifestPath,
            StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(evidence.PublishedWeightsPath));
        Assert.True(File.Exists(evidence.PublishedManifestPath));
        Assert.False(Directory.Exists(decoyCheckpointDirectory));

        var stepOneAddress = allowedModelContext.Request!.Address
            .CreateStepAddress(1);
        Assert.True(stepOneAddress.IsSuccess, stepOneAddress.ErrorMessage);
        var expectedTrainerPolicyPath = Path.Combine(
            stepOneAddress.Value!.CheckpointRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var expectedTrainerMetricsPath = Path.Combine(
            stepOneAddress.Value.CheckpointRoot,
            DpoTrainer.BuildMetricsFileName(1));
        Assert.Equal(expectedTrainerPolicyPath, report.CheckpointPath);
        Assert.Equal(expectedTrainerMetricsPath, report.TrainerMetricsPath);
        Assert.True(File.Exists(report.CheckpointPath));
        Assert.True(File.Exists(report.TrainerMetricsPath));
        Assert.StartsWith(
            stepOneAddress.Value.CheckpointRoot,
            report.CheckpointPath!,
            StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith(
            stepOneAddress.Value.CheckpointRoot,
            report.TrainerMetricsPath!,
            StringComparison.OrdinalIgnoreCase);
        var opened = RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
            stepOneAddress.Value);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Null(opened.Value!.Manifest.ParentCheckpoint);
        Assert.Equal(coldStartReason, opened.Value.Manifest.ColdStartReason);
        Assert.Equal(
            dpoContext.Address.ManifestRelativePath,
            opened.Value.Manifest.DpoDatasetManifest
                .RepositoryRelativeManifestPath);
        Assert.Equal(
            opened.Value.Manifest.WeightsSha256,
            evidence.PublishedWeightsSha256);
        Assert.Equal(
            ComputeSha256(report.CheckpointPath!),
            evidence.PublishedWeightsSha256);
        Assert.Equal(
            ComputeSha256(report.CheckpointPath!),
            ComputeSha256(evidence.PublishedWeightsPath));
        Assert.Equal(
            ComputeSha256(report.CheckpointPath!),
            report.PolicyCheckpointSha256);
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointRejectsReportInsideCallerCheckpointDirectoryBeforeMutation()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-output-overlap");
        _ = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-output-overlap",
                count: 2));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-output-overlap",
            allowColdStart: true,
            coldStartReason:
                "output overlap fixture would otherwise publish first model checkpoint");
        var externalDecoyCheckpointDirectory = Path.Combine(
            _tempRoot,
            "model-output-overlap-decoy-checkpoints");
        var outputPath = Path.Combine(
            externalDecoyCheckpointDirectory,
            "report.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var factoryCallCount = 0;

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () =>
            {
                factoryCallCount++;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCallCount++;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCallCount++;
                return new LlmModel(BuildTinyConfig());
            },
            broker,
            BuildOptions(
                    outputPath,
                    externalDecoyCheckpointDirectory,
                    Path.Combine(
                        _tempRoot,
                        "missing-artifact-root-for-output-overlap"))
                with
                {
                    RepositoryOwnedDpoDatasetContext = dpoContext,
                    RepositoryOwnedModelCheckpointContext = modelContext
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointOutputOverlapsCallerCheckpointDirectoryErrorCode,
            result.ErrorCode);
        Assert.Equal(0, factoryCallCount);
        Assert.Null(broker.LastTrainingContent);
        Assert.Empty(broker.TrainingRunIds);
        Assert.False(File.Exists(outputPath));
        Assert.False(Directory.Exists(externalDecoyCheckpointDirectory));
        var nextStep = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(nextStep.IsSuccess, nextStep.ErrorMessage);
        Assert.False(Directory.Exists(nextStep.Value!.CheckpointRoot));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointRejectsReportThroughDirectoryLinkToCallerCheckpointDirectoryBeforeMutation()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-output-link-overlap");
        _ = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-output-link-overlap",
                count: 2));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-output-link-overlap",
            allowColdStart: true,
            coldStartReason:
                "directory-link output fixture must fail before model checkpoint publication");
        var callerCheckpointDirectory = Path.Combine(
            _tempRoot,
            "model-output-link-target-checkpoints");
        Directory.CreateDirectory(callerCheckpointDirectory);
        var linkParent = Path.Combine(
            _tempRoot,
            "model-output-link-alias-parent");
        Directory.CreateDirectory(linkParent);
        var outputAlias = Path.Combine(
            linkParent,
            "lexically-outside-checkpoint-alias");

        // The alias is lexically outside the caller checkpoint directory but
        // physically routes report writes into it; the runner must not follow
        // the link to accept that relationship.
        CreateRequiredDirectoryLink(outputAlias, callerCheckpointDirectory);
        var outputPath = Path.Combine(outputAlias, "report.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var factoryCallCount = 0;

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () =>
            {
                factoryCallCount++;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCallCount++;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCallCount++;
                return new LlmModel(BuildTinyConfig());
            },
            broker,
            BuildOptions(
                    outputPath,
                    callerCheckpointDirectory,
                    Path.Combine(
                        _tempRoot,
                        "missing-artifact-root-for-output-link-overlap"))
                with
                {
                    RepositoryOwnedDpoDatasetContext = dpoContext,
                    RepositoryOwnedModelCheckpointContext = modelContext
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointOutputOrCallerCheckpointDirectoryReparsePointErrorCode,
            result.ErrorCode);
        Assert.Equal(0, factoryCallCount);
        Assert.Null(broker.LastTrainingContent);
        Assert.Empty(broker.TrainingRunIds);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(Path.Combine(
            callerCheckpointDirectory,
            "report.json")));
        var nextStep = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(nextStep.IsSuccess, nextStep.ErrorMessage);
        Assert.False(Directory.Exists(nextStep.Value!.CheckpointRoot));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointRejectsCallerCheckpointDirectoryThroughDirectoryLinkBeforeMutation()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-checkpoint-link-chain");
        _ = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-checkpoint-link-chain",
                count: 2));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-checkpoint-link-chain",
            allowColdStart: true,
            coldStartReason:
                "checkpoint-directory link fixture must fail before training");
        var checkpointAliasTarget = Path.Combine(
            _tempRoot,
            "model-checkpoint-link-target");
        Directory.CreateDirectory(checkpointAliasTarget);
        var checkpointAlias = Path.Combine(
            _tempRoot,
            "model-checkpoint-link-alias");

        // Any existing link in the caller checkpoint directory chain makes the
        // output/checkpoint boundary physically unprovable even when the report
        // path itself is ordinary.
        CreateRequiredDirectoryLink(checkpointAlias, checkpointAliasTarget);
        var callerCheckpointDirectory = Path.Combine(
            checkpointAlias,
            "legacy-checkpoints");
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "checkpoint-link-chain-report.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var factoryCallCount = 0;

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () =>
            {
                factoryCallCount++;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCallCount++;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCallCount++;
                return new LlmModel(BuildTinyConfig());
            },
            broker,
            BuildOptions(
                    outputPath,
                    callerCheckpointDirectory,
                    Path.Combine(
                        _tempRoot,
                        "missing-artifact-root-for-checkpoint-link-chain"))
                with
                {
                    RepositoryOwnedDpoDatasetContext = dpoContext,
                    RepositoryOwnedModelCheckpointContext = modelContext
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointOutputOrCallerCheckpointDirectoryReparsePointErrorCode,
            result.ErrorCode);
        Assert.Equal(0, factoryCallCount);
        Assert.Null(broker.LastTrainingContent);
        Assert.Empty(broker.TrainingRunIds);
        Assert.False(File.Exists(outputPath));
        Assert.False(Directory.Exists(Path.Combine(
            checkpointAliasTarget,
            "legacy-checkpoints")));
        var nextStep = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(nextStep.IsSuccess, nextStep.ErrorMessage);
        Assert.False(Directory.Exists(nextStep.Value!.CheckpointRoot));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointAllowsReportInSiblingPrefixDirectory()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-output-sibling-prefix");
        _ = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-output-sibling-prefix",
                count: 2));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-output-sibling-prefix",
            allowColdStart: true,
            coldStartReason:
                "sibling prefix fixture has no prior repository-owned parent");
        var callerCheckpointDirectory = Path.Combine(
            _tempRoot,
            "model-output-sibling-checkpoints");
        var siblingPrefixOutputPath = Path.Combine(
            _tempRoot,
            "model-output-sibling-checkpoints-other",
            "report.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            broker,
            BuildOptions(
                    siblingPrefixOutputPath,
                    callerCheckpointDirectory,
                    Path.Combine(
                        _tempRoot,
                        "missing-artifact-root-for-output-sibling-prefix"))
                with
                {
                    RepositoryOwnedDpoDatasetContext = dpoContext,
                    RepositoryOwnedModelCheckpointContext = modelContext
                });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(File.Exists(siblingPrefixOutputPath));
        Assert.False(Directory.Exists(callerCheckpointDirectory));
        Assert.Single(broker.TrainingRunIds);
        Assert.NotNull(result.Value!.RepositoryOwnedModelCheckpointEvidence);
        var nextStep = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(nextStep.IsSuccess, nextStep.ErrorMessage);
        Assert.Equal(
            Path.Combine(
                nextStep.Value!.CheckpointRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1)),
            result.Value.CheckpointPath);
        Assert.True(File.Exists(result.Value.CheckpointPath));
        Assert.True(File.Exists(result.Value.TrainerMetricsPath));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointTwoSequentialRunsReuseSameDecoyAndPublishNextSteps()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-sequential-enabled");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-sequential-enabled",
                count: 2));
        var coldStartReason =
            "first sequential translation dpo model checkpoint for temp fixture";
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-sequential-enabled",
            allowColdStart: true,
            coldStartReason: coldStartReason);
        var sharedDecoyCheckpointDirectory = Path.Combine(
            _tempRoot,
            "model-sequential-enabled-decoy");
        var sharedOptions = BuildOptions(
                Path.Combine(
                    _tempRoot,
                    "reports",
                    "model-sequential-enabled.json"),
                sharedDecoyCheckpointDirectory,
                Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-sequential-enabled"))
            with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            };

        var first = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            sharedOptions);
        var second = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            sharedOptions);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.False(Directory.Exists(sharedDecoyCheckpointDirectory));
        Assert.Equal(
            sealedDataset.Dataset.Manifest.ManifestSha256,
            first.Value!.RepositoryOwnedDpoDatasetEvidence!.ManifestSha256);
        Assert.Equal(
            sealedDataset.Dataset.Manifest.ManifestSha256,
            second.Value!.RepositoryOwnedDpoDatasetEvidence!.ManifestSha256);

        var firstEvidence = first.Value.RepositoryOwnedModelCheckpointEvidence!;
        var secondEvidence = second.Value.RepositoryOwnedModelCheckpointEvidence!;
        Assert.Equal("cold_start_explicit", firstEvidence.StartMode);
        Assert.Equal(1, firstEvidence.PublishedStep);
        Assert.Null(firstEvidence.ParentStep);
        Assert.Equal(coldStartReason, firstEvidence.ColdStartReason);
        Assert.Equal(
            "continued_from_latest_compatible_parent",
            secondEvidence.StartMode);
        Assert.Equal(2, secondEvidence.PublishedStep);
        Assert.Equal(1, secondEvidence.ParentStep);
        Assert.Equal(
            firstEvidence.PublishedManifestRelativePath,
            secondEvidence.ParentManifestRelativePath);
        Assert.Equal(
            firstEvidence.PublishedWeightsSha256,
            secondEvidence.ParentWeightsSha256);
        Assert.Equal(
            firstEvidence.PublishedWeightsByteLength,
            secondEvidence.ParentWeightsByteLength);

        var stepOneAddress = modelContext.Request!.Address.CreateStepAddress(1);
        var stepTwoAddress = modelContext.Request.Address.CreateStepAddress(2);
        Assert.True(stepOneAddress.IsSuccess, stepOneAddress.ErrorMessage);
        Assert.True(stepTwoAddress.IsSuccess, stepTwoAddress.ErrorMessage);
        Assert.Equal(
            Path.Combine(
                stepOneAddress.Value!.CheckpointRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1)),
            first.Value.CheckpointPath);
        Assert.Equal(
            Path.Combine(
                stepTwoAddress.Value!.CheckpointRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1)),
            second.Value.CheckpointPath);
        Assert.Equal(
            Path.Combine(
                stepOneAddress.Value.CheckpointRoot,
                DpoTrainer.BuildMetricsFileName(1)),
            first.Value.TrainerMetricsPath);
        Assert.Equal(
            Path.Combine(
                stepTwoAddress.Value.CheckpointRoot,
                DpoTrainer.BuildMetricsFileName(1)),
            second.Value.TrainerMetricsPath);
        Assert.Equal(
            ComputeSha256(first.Value.CheckpointPath!),
            firstEvidence.PublishedWeightsSha256);
        Assert.Equal(
            ComputeSha256(second.Value.CheckpointPath!),
            secondEvidence.PublishedWeightsSha256);
        Assert.Equal(
            ComputeSha256(second.Value.CheckpointPath!),
            ComputeSha256(secondEvidence.PublishedWeightsPath));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointLoadsLatestCompatibleParentAndPublishesContinuationLineage()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-continuation");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(dpoContext, "model-continuation"));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-continuation",
            allowColdStart: false);
        var stepOne = SealRepositoryOwnedModelCheckpoint(
            modelContext,
            dpoContext,
            sealedDataset,
            step: 1,
            parent: null,
            coldStartReason: "seed parent for continuation fixture",
            rootSeed: 222UL);
        var stepTwo = SealRepositoryOwnedModelCheckpoint(
            modelContext,
            dpoContext,
            sealedDataset,
            step: 2,
            parent: ToTestParentIdentity(stepOne),
            coldStartReason: string.Empty,
            rootSeed: 333UL);
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-continuation.json");

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(outputPath, Path.Combine(
                    _tempRoot,
                    "model-continuation-decoy-checkpoints"),
                trainingDataArtifactRoot: Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-exact-dpo-hit")) with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var evidence = Assert.IsType<
            TranslationRepositoryOwnedModelCheckpointRunEvidence>(
            result.Value!.RepositoryOwnedModelCheckpointEvidence);
        Assert.Equal(
            "continued_from_latest_compatible_parent",
            evidence.StartMode);
        Assert.Equal(2, evidence.ParentStep);
        Assert.Equal(stepTwo.RepositoryRelativeManifestPath,
            evidence.ParentManifestRelativePath);
        Assert.Equal(stepTwo.WeightsSha256, evidence.ParentWeightsSha256);
        Assert.Equal(stepTwo.WeightsByteLength, evidence.ParentWeightsByteLength);
        Assert.Equal(3, evidence.PublishedStep);
        Assert.Null(evidence.ColdStartReason);

        var stepThreeAddress = modelContext.Request!.Address.CreateStepAddress(
            3);
        Assert.True(stepThreeAddress.IsSuccess, stepThreeAddress.ErrorMessage);
        Assert.False(Directory.Exists(Path.Combine(
            _tempRoot,
            "model-continuation-decoy-checkpoints")));
        Assert.Equal(
            Path.Combine(
                stepThreeAddress.Value!.CheckpointRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(1)),
            result.Value!.CheckpointPath);
        Assert.Equal(
            Path.Combine(
                stepThreeAddress.Value.CheckpointRoot,
                DpoTrainer.BuildMetricsFileName(1)),
            result.Value.TrainerMetricsPath);
        var opened = RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
            stepThreeAddress.Value);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Equal(3, opened.Value!.Manifest.Step);
        Assert.NotNull(opened.Value.Manifest.ParentCheckpoint);
        Assert.Equal(
            stepTwo.RepositoryRelativeManifestPath,
            opened.Value.Manifest.ParentCheckpoint!
                .RepositoryRelativeManifestPath);
        Assert.Equal(stepTwo.WeightsSha256,
            opened.Value.Manifest.ParentCheckpoint.CheckpointSha256);
        Assert.Equal(
            sealedDataset.Dataset.Manifest.ManifestSha256,
            opened.Value.Manifest.DpoDatasetManifest.ManifestSha256);
        Assert.Equal(
            ComputeSha256(result.Value.CheckpointPath!),
            evidence.PublishedWeightsSha256);
        Assert.Equal(
            ComputeSha256(result.Value.CheckpointPath!),
            ComputeSha256(evidence.PublishedWeightsPath));
        Assert.Equal(
            ComputeSha256(result.Value.CheckpointPath!),
            result.Value.PolicyCheckpointSha256);
    }

    [Theory]
    [InlineData("corrupt")]
    [InlineData("incompatible")]
    [InlineData("ambiguous")]
    public void Run_RepositoryOwnedModelCheckpointUnsafeParentsFailBeforeTraining(
        string caseName)
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-unsafe-" + caseName);
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-unsafe-" + caseName,
                count: 2));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-unsafe-" + caseName,
            allowColdStart: true,
            coldStartReason:
                "allowed only for exact missing tree, not unsafe parents");
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
            var manifest = SealRepositoryOwnedModelCheckpoint(
                modelContext,
                dpoContext,
                sealedDataset,
                step: 1,
                parent: null,
                coldStartReason: "corrupt fixture parent",
                rootSeed: 444UL);
            var address = modelContext.Request!.Address.CreateStepAddress(1);
            Assert.True(address.IsSuccess, address.ErrorMessage);
            File.AppendAllText(address.Value!.WeightsPath, "tampered", Utf8NoBom);
            Assert.NotEqual(
                manifest.WeightsSha256,
                ComputeSha256(address.Value.WeightsPath));
        }
        else if (caseName == "incompatible")
        {
            var incompatible = modelContext with
            {
                Request = modelContext.Request! with
                {
                    RuntimeCompatibility =
                        CreateRepositoryOwnedModelRuntimeCompatibility(
                            "different-runtime")
                }
            };
            _ = SealRepositoryOwnedModelCheckpoint(
                incompatible,
                dpoContext,
                sealedDataset,
                step: 1,
                parent: null,
                coldStartReason: "incompatible fixture parent",
                rootSeed: 555UL);
        }
        else
        {
            _ = SealRepositoryOwnedModelCheckpoint(
                modelContext,
                dpoContext,
                sealedDataset,
                step: 1,
                parent: null,
                coldStartReason: "ambiguous fixture parent",
                rootSeed: 666UL);
            var ambiguousStepRoot = Path.Combine(
                modelContext.Request!.Address.RepositoryRoot,
                "training-artifacts",
                "models",
                modelContext.Request.Address.DomainSlug,
                modelContext.Request.Address.DesignPatternSlug,
                modelContext.Request.Address.ModelIdSlug,
                modelContext.Request.Address.ModelVersion,
                "checkpoints",
                "1");
            Directory.CreateDirectory(ambiguousStepRoot);
        }

        var checkpointDirectory = Path.Combine(
            _tempRoot,
            "model-unsafe-" + caseName + "-checkpoints");
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-unsafe-" + caseName + ".json");

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(outputPath, checkpointDirectory, Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-unsafe-" + caseName)) with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError, result.ErrorCode);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(Path.Combine(
            checkpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointInvalidParentBytesFailBeforeTrainerMutation()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-invalid-parent-bytes");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-invalid-parent-bytes",
                count: 2));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-invalid-parent-bytes",
            allowColdStart: false);
        SealRepositoryOwnedRawModelCheckpoint(
            modelContext,
            dpoContext,
            sealedDataset,
            step: 1,
            parent: null,
            coldStartReason: "invalid parent bytes fixture");
        var checkpointDirectory = Path.Combine(
            _tempRoot,
            "model-invalid-parent-bytes-checkpoints");
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-invalid-parent-bytes.json");

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(outputPath, checkpointDirectory, Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-invalid-parent")) with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointPolicyLoadFailedErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(Path.Combine(
            checkpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointPublicationFailureDoesNotWriteSuccessReport()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-publication-failure");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-publication-failure",
                count: 2));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-publication-failure",
            allowColdStart: true,
            coldStartReason:
                "publication failure fixture requires explicit cold start")
            with
            {
                Request = CreateRepositoryOwnedModelCheckpointRequest(
                    dpoContext,
                    "model-publication-failure-mismatched-pattern",
                    allowColdStart: true,
                    coldStartReason:
                        "publication failure fixture requires explicit cold start",
                    designPatternSlug: "other-design-pattern")
            };
        var checkpointDirectory = Path.Combine(
            _tempRoot,
            "model-publication-failure-checkpoints");
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-publication-failure.json");

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(outputPath, checkpointDirectory, Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-publication-failure")) with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(outputPath));
        Assert.False(Directory.Exists(checkpointDirectory));
        var attemptedStep = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(attemptedStep.IsSuccess, attemptedStep.ErrorMessage);
        Assert.True(File.Exists(Path.Combine(
            attemptedStep.Value!.CheckpointRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));
        Assert.True(File.Exists(Path.Combine(
            attemptedStep.Value.CheckpointRoot,
            DpoTrainer.BuildMetricsFileName(1))));
        Assert.False(File.Exists(attemptedStep.Value.WeightsPath));
        Assert.False(File.Exists(attemptedStep.Value.ManifestPath));
        var reopenedAttempt = RepositoryOwnedModelCheckpointManifestSealer
            .OpenSealed(attemptedStep.Value);
        Assert.False(reopenedAttempt.IsSuccess);
        var loaderAttempt = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                modelContext.Request.Address.RepositoryRoot,
                modelContext.Request.Address.DomainSlug,
                modelContext.Request.Address.DesignPatternSlug,
                modelContext.Request.Address.ModelIdSlug,
                modelContext.Request.Address.ModelVersion,
                modelContext.Request.RuntimeCompatibility);
        Assert.False(loaderAttempt.IsSuccess);
        Assert.Equal(
            sealedDataset.Dataset.Manifest.ManifestSha256,
            RepositoryOwnedDpoDatasetManifestSealer
                .OpenSealedForManifestIntegrity(dpoContext.Address!)
                .Value!.Manifest.ManifestSha256);
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointPendingTrainerFilesSealWithoutRetraining()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-pending-publication-recovery");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-pending-publication-recovery",
                count: 2));
        var coldStartReason =
            "pending publication recovery fixture starts from an explicit clean repository miss";
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-pending-publication-recovery",
            allowColdStart: true,
            coldStartReason: coldStartReason);
        var stepAddress = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(stepAddress.IsSuccess, stepAddress.ErrorMessage);
        CreateRepositoryOwnedCheckpointRootForTest(
            stepAddress.Value!.CheckpointRoot);

        var trainerPolicyPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var trainerMetricsPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildMetricsFileName(1));
        CreateSeededPolicyModel(777UL).SaveCheckpoint(trainerPolicyPath);
        var recoveredMetrics = new DpoMetrics
        {
            Step = 1,
            Loss = 0.125f,
            ChosenReward = 0.75f,
            RejectedReward = 0.25f,
            RewardMargin = 0.5f,
            Accuracy = 1.0f
        };
        File.WriteAllText(
            trainerMetricsPath,
            JsonSerializer.Serialize(recoveredMetrics),
            Utf8NoBom);
        File.Copy(trainerPolicyPath, stepAddress.Value.WeightsPath);
        var trainerPolicyShaBeforeRecovery = ComputeSha256(trainerPolicyPath);

        var pendingJournalPath =
            WriteRepositoryOwnedModelCheckpointPendingJournalForTest(
                modelContext,
                dpoContext,
                sealedDataset,
                stepAddress.Value,
                trainerPolicyPath,
                trainerMetricsPath,
                coldStartReason);
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-pending-publication-recovery.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            broker,
            BuildOptions(outputPath, Path.Combine(
                    _tempRoot,
                    "model-pending-publication-recovery-decoy-checkpoints"),
                trainingDataArtifactRoot: Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-pending-publication-recovery"))
            with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(File.Exists(outputPath));
        Assert.False(File.Exists(pendingJournalPath));
        Assert.False(Directory.Exists(Path.Combine(
            _tempRoot,
            "model-pending-publication-recovery-decoy-checkpoints")));
        Assert.Equal(trainerPolicyPath, result.Value!.CheckpointPath);
        Assert.Equal(trainerMetricsPath, result.Value.TrainerMetricsPath);
        Assert.Equal(recoveredMetrics.Step, result.Value.TrainMetrics.Step);
        Assert.Equal(recoveredMetrics.Loss, result.Value.TrainMetrics.Loss);
        Assert.Equal(
            recoveredMetrics.ChosenReward,
            result.Value.TrainMetrics.ChosenReward);
        Assert.Equal(
            recoveredMetrics.RejectedReward,
            result.Value.TrainMetrics.RejectedReward);
        Assert.Equal(
            recoveredMetrics.RewardMargin,
            result.Value.TrainMetrics.RewardMargin);
        Assert.Equal(
            recoveredMetrics.Accuracy,
            result.Value.TrainMetrics.Accuracy);
        Assert.Contains(
            "recovered-from-pending-repository-checkpoint",
            result.Value.TrainerCallPath,
            StringComparison.Ordinal);

        var opened = RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
            stepAddress.Value);
        Assert.True(opened.IsSuccess, opened.ErrorMessage);
        Assert.Equal(1, opened.Value!.Manifest.Step);
        Assert.Equal(
            trainerPolicyShaBeforeRecovery,
            opened.Value.Manifest.WeightsSha256);
        Assert.Equal(
            trainerPolicyShaBeforeRecovery,
            ComputeSha256(stepAddress.Value.WeightsPath));
        Assert.Equal(
            sealedDataset.Dataset.Manifest.ManifestSha256,
            opened.Value.Manifest.DpoDatasetManifest.ManifestSha256);
        Assert.Contains(
            "trainer-step:1",
            opened.Value.Manifest.ProvenanceRefs);
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointPendingPublicationRejectsForeignOrphanWeights()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-pending-publication-foreign-orphan");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-pending-publication-foreign-orphan",
                count: 2));
        var coldStartReason =
            "pending publication foreign orphan fixture starts from an explicit clean repository miss";
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-pending-publication-foreign-orphan",
            allowColdStart: true,
            coldStartReason: coldStartReason);
        var stepAddress = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(stepAddress.IsSuccess, stepAddress.ErrorMessage);
        CreateRepositoryOwnedCheckpointRootForTest(
            stepAddress.Value!.CheckpointRoot);

        var trainerPolicyPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var trainerMetricsPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildMetricsFileName(1));
        CreateSeededPolicyModel(778UL).SaveCheckpoint(trainerPolicyPath);
        CreateSeededPolicyModel(779UL).SaveCheckpoint(stepAddress.Value.WeightsPath);
        File.WriteAllText(
            trainerMetricsPath,
            JsonSerializer.Serialize(new DpoMetrics
            {
                Step = 1,
                Loss = 0.25f,
                ChosenReward = 0.5f,
                RejectedReward = 0.1f,
                RewardMargin = 0.4f,
                Accuracy = 1.0f
            }),
            Utf8NoBom);
        var pendingJournalPath =
            WriteRepositoryOwnedModelCheckpointPendingJournalForTest(
                modelContext,
                dpoContext,
                sealedDataset,
                stepAddress.Value,
                trainerPolicyPath,
                trainerMetricsPath,
                coldStartReason);
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-pending-publication-foreign-orphan.json");

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(outputPath, Path.Combine(
                    _tempRoot,
                    "model-pending-publication-foreign-orphan-decoy-checkpoints"),
                trainingDataArtifactRoot: Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-pending-publication-foreign-orphan"))
            with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode,
            result.ErrorCode);
        Assert.True(File.Exists(pendingJournalPath));
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(stepAddress.Value.ManifestPath));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointPendingPublicationRejectsAmbiguousArtifactsWithoutJournal()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-pending-publication-no-journal");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-pending-publication-no-journal",
                count: 2));
        var coldStartReason =
            "pending publication no journal fixture starts from an explicit clean repository miss";
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-pending-publication-no-journal",
            allowColdStart: true,
            coldStartReason: coldStartReason);
        var stepAddress = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(stepAddress.IsSuccess, stepAddress.ErrorMessage);
        CreateRepositoryOwnedCheckpointRootForTest(
            stepAddress.Value!.CheckpointRoot);

        var trainerPolicyPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var trainerMetricsPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildMetricsFileName(1));
        CreateSeededPolicyModel(780UL).SaveCheckpoint(trainerPolicyPath);
        File.Copy(trainerPolicyPath, stepAddress.Value.WeightsPath);
        File.WriteAllText(
            trainerMetricsPath,
            JsonSerializer.Serialize(new DpoMetrics
            {
                Step = 1,
                Loss = 0.2f,
                ChosenReward = 0.7f,
                RejectedReward = 0.3f,
                RewardMargin = 0.4f,
                Accuracy = 1.0f
            }),
            Utf8NoBom);
        var policyShaBeforeRun = ComputeSha256(trainerPolicyPath);
        var weightsShaBeforeRun = ComputeSha256(stepAddress.Value.WeightsPath);
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-pending-publication-no-journal.json");

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(outputPath, Path.Combine(
                    _tempRoot,
                    "model-pending-publication-no-journal-decoy-checkpoints"),
                trainingDataArtifactRoot: Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-pending-publication-no-journal"))
            with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointPendingPublicationInvalidErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(stepAddress.Value.ManifestPath));
        Assert.Equal(policyShaBeforeRun, ComputeSha256(trainerPolicyPath));
        Assert.Equal(weightsShaBeforeRun, ComputeSha256(stepAddress.Value.WeightsPath));
        Assert.Equal(
            sealedDataset.Dataset.Manifest.ManifestSha256,
            RepositoryOwnedDpoDatasetManifestSealer
                .OpenSealedForManifestIntegrity(dpoContext.Address!)
                .Value!.Manifest.ManifestSha256);
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointPendingPublicationRecoversContinuationStepAfterParent()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-pending-publication-step-two");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-pending-publication-step-two",
                count: 2));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-pending-publication-step-two",
            allowColdStart: false);
        var stepOne = SealRepositoryOwnedModelCheckpoint(
            modelContext,
            dpoContext,
            sealedDataset,
            step: 1,
            parent: null,
            coldStartReason: "seed parent for pending continuation recovery",
            rootSeed: 781UL);
        var parent = ToTestParentIdentity(stepOne);
        var stepTwoAddress = modelContext.Request!.Address.CreateStepAddress(2);
        Assert.True(stepTwoAddress.IsSuccess, stepTwoAddress.ErrorMessage);
        CreateRepositoryOwnedCheckpointRootForTest(
            stepTwoAddress.Value!.CheckpointRoot);

        var trainerPolicyPath = Path.Combine(
            stepTwoAddress.Value.CheckpointRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var trainerMetricsPath = Path.Combine(
            stepTwoAddress.Value.CheckpointRoot,
            DpoTrainer.BuildMetricsFileName(1));
        CreateSeededPolicyModel(782UL).SaveCheckpoint(trainerPolicyPath);
        File.Copy(trainerPolicyPath, stepTwoAddress.Value.WeightsPath);
        var recoveredMetrics = new DpoMetrics
        {
            Step = 1,
            Loss = 0.12f,
            ChosenReward = 0.8f,
            RejectedReward = 0.2f,
            RewardMargin = 0.6f,
            Accuracy = 1.0f
        };
        File.WriteAllText(
            trainerMetricsPath,
            JsonSerializer.Serialize(recoveredMetrics),
            Utf8NoBom);
        var pendingJournalPath =
            WriteRepositoryOwnedModelCheckpointPendingJournalForTest(
                modelContext,
                dpoContext,
                sealedDataset,
                stepTwoAddress.Value,
                trainerPolicyPath,
                trainerMetricsPath,
                coldStartReason: string.Empty,
                parent: parent);
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-pending-publication-step-two.json");

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(outputPath, Path.Combine(
                    _tempRoot,
                    "model-pending-publication-step-two-decoy-checkpoints"),
                trainingDataArtifactRoot: Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-pending-publication-step-two"))
            with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(File.Exists(pendingJournalPath));
        var evidence = result.Value!.RepositoryOwnedModelCheckpointEvidence!;
        Assert.Equal("continued_from_latest_compatible_parent",
            evidence.StartMode);
        Assert.Equal(1, evidence.ParentStep);
        Assert.Equal(2, evidence.PublishedStep);
        Assert.Equal(stepOne.RepositoryRelativeManifestPath,
            evidence.ParentManifestRelativePath);
        Assert.Equal(stepOne.WeightsSha256, evidence.ParentWeightsSha256);
        Assert.False(Directory.Exists(
            modelContext.Request.Address.CreateStepAddress(3).Value!
                .CheckpointRoot));
        Assert.Equal(recoveredMetrics.Accuracy,
            result.Value.TrainMetrics.Accuracy);
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointPendingPublicationConsumesHeldParentWeights()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-pending-publication-parent-held");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-pending-publication-parent-held",
                count: 2));
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-pending-publication-parent-held",
            allowColdStart: false);
        var stepOne = SealRepositoryOwnedModelCheckpoint(
            modelContext,
            dpoContext,
            sealedDataset,
            step: 1,
            parent: null,
            coldStartReason:
            "seed parent for held-stream continuation recovery",
            rootSeed: 791UL);
        var stepOneAddress = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(stepOneAddress.IsSuccess, stepOneAddress.ErrorMessage);
        var stepTwoAddress = modelContext.Request.Address.CreateStepAddress(2);
        Assert.True(stepTwoAddress.IsSuccess, stepTwoAddress.ErrorMessage);
        CreateRepositoryOwnedCheckpointRootForTest(
            stepTwoAddress.Value!.CheckpointRoot);

        var trainerPolicyPath = Path.Combine(
            stepTwoAddress.Value.CheckpointRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var trainerMetricsPath = Path.Combine(
            stepTwoAddress.Value.CheckpointRoot,
            DpoTrainer.BuildMetricsFileName(1));
        CreateSeededPolicyModel(792UL).SaveCheckpoint(trainerPolicyPath);
        File.WriteAllText(
            trainerMetricsPath,
            JsonSerializer.Serialize(new DpoMetrics
            {
                Step = 1,
                Loss = 0.11f,
                ChosenReward = 0.7f,
                RejectedReward = 0.2f,
                RewardMargin = 0.5f,
                Accuracy = 1.0f
            }),
            Utf8NoBom);
        var pendingJournalPath =
            WriteRepositoryOwnedModelCheckpointPendingJournalForTest(
                modelContext,
                dpoContext,
                sealedDataset,
                stepTwoAddress.Value,
                trainerPolicyPath,
                trainerMetricsPath,
                coldStartReason: string.Empty,
                parent: ToTestParentIdentity(stepOne));
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-pending-publication-parent-held.json");
        var parentSwapAttempts = 0;
        var parentSwapRejected = 0;
        var parentSwapSucceeded = 0;

        try
        {
            SetRepositoryOwnedModelCheckpointHeldFileValidationProbe(
                (role, path) =>
                {
                    if (!string.Equals(
                            role,
                            "repository-owned parent checkpoint weights",
                            StringComparison.Ordinal) ||
                        !PathsEqual(path, stepOneAddress.Value!.WeightsPath))
                    {
                        return;
                    }

                    parentSwapAttempts++;
                    if (TryReplaceFileWithBytes(
                            path,
                            Utf8NoBom.GetBytes(
                                "not a transformer checkpoint after parent validation\n")))
                    {
                        parentSwapSucceeded++;
                    }
                    else
                    {
                        parentSwapRejected++;
                    }
                });

            var result = TranslationDpoTrainEvaluateRunner.Run(
                GetRepoSeedDataPath(),
                () => new LlmModel(BuildTinyConfig()),
                () => new LlmModel(BuildTinyConfig()),
                () => new LlmModel(BuildTinyConfig()),
                CreateAuthority(),
                BuildOptions(outputPath, Path.Combine(
                        _tempRoot,
                        "model-pending-publication-parent-held-decoy"),
                    trainingDataArtifactRoot: Path.Combine(
                        _tempRoot,
                        "missing-artifact-root-for-parent-held"))
                with
                {
                    RepositoryOwnedDpoDatasetContext = dpoContext,
                    RepositoryOwnedModelCheckpointContext = modelContext
                });

            Assert.True(parentSwapAttempts > 0);
            Assert.True(parentSwapSucceeded > 0 || parentSwapRejected > 0);
            if (parentSwapSucceeded > 0)
            {
                // Если ОС разрешила заменить parent path, runner не должен
                // продолжать публикацию поверх испорченной lineage. Это честный
                // fail-closed исход того же TOCTOU-теста.
                Assert.False(result.IsSuccess);
                Assert.Equal(
                    RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
                    result.ErrorCode);
                Assert.True(File.Exists(pendingJournalPath));
                Assert.False(File.Exists(stepTwoAddress.Value.ManifestPath));
            }
            else
            {
                Assert.True(result.IsSuccess, result.ErrorMessage);
                Assert.False(File.Exists(pendingJournalPath));
                Assert.Equal(2,
                    result.Value!.RepositoryOwnedModelCheckpointEvidence!
                        .PublishedStep);
            }
        }
        finally
        {
            SetRepositoryOwnedModelCheckpointHeldFileValidationProbe(null);
        }
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointPendingPublicationConsumesHeldTrainerArtifacts()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-pending-publication-trainer-held");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-pending-publication-trainer-held",
                count: 2));
        var coldStartReason =
            "pending publication trainer held-stream fixture starts clean";
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-pending-publication-trainer-held",
            allowColdStart: true,
            coldStartReason: coldStartReason);
        var stepAddress = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(stepAddress.IsSuccess, stepAddress.ErrorMessage);
        CreateRepositoryOwnedCheckpointRootForTest(
            stepAddress.Value!.CheckpointRoot);

        var trainerPolicyPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var trainerMetricsPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildMetricsFileName(1));
        CreateSeededPolicyModel(793UL).SaveCheckpoint(trainerPolicyPath);
        var originalMetrics = new DpoMetrics
        {
            Step = 1,
            Loss = 0.21f,
            ChosenReward = 0.8f,
            RejectedReward = 0.1f,
            RewardMargin = 0.7f,
            Accuracy = 1.0f
        };
        File.WriteAllText(
            trainerMetricsPath,
            JsonSerializer.Serialize(originalMetrics),
            Utf8NoBom);
        var originalPolicySha256 = ComputeSha256(trainerPolicyPath);
        var pendingJournalPath =
            WriteRepositoryOwnedModelCheckpointPendingJournalForTest(
                modelContext,
                dpoContext,
                sealedDataset,
                stepAddress.Value,
                trainerPolicyPath,
                trainerMetricsPath,
                coldStartReason);
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-pending-publication-trainer-held.json");
        var trainerSwapAttempts = 0;
        var trainerSwapRejected = 0;
        var trainerSwapSucceeded = 0;

        try
        {
            SetRepositoryOwnedModelCheckpointHeldFileValidationProbe(
                (role, path) =>
                {
                    if (string.Equals(
                            role,
                            "repository-owned recovered trainer policy checkpoint",
                            StringComparison.Ordinal))
                    {
                        trainerSwapAttempts++;
                        if (TryReplaceFileWithBytes(
                                path,
                                Utf8NoBom.GetBytes(
                                    "not a transformer checkpoint after policy validation\n")))
                        {
                            trainerSwapSucceeded++;
                        }
                        else
                        {
                            trainerSwapRejected++;
                        }
                    }

                    if (string.Equals(
                            role,
                            "repository-owned recovered trainer metrics",
                            StringComparison.Ordinal))
                    {
                        trainerSwapAttempts++;
                        var tamperedMetrics = JsonSerializer.Serialize(
                            originalMetrics with { Step = 99 });
                        if (TryReplaceFileWithBytes(
                                path,
                                Utf8NoBom.GetBytes(tamperedMetrics)))
                        {
                            trainerSwapSucceeded++;
                        }
                        else
                        {
                            trainerSwapRejected++;
                        }
                    }
                });

            var result = TranslationDpoTrainEvaluateRunner.Run(
                GetRepoSeedDataPath(),
                () => new LlmModel(BuildTinyConfig()),
                () => new LlmModel(BuildTinyConfig()),
                () => new LlmModel(BuildTinyConfig()),
                CreateAuthority(),
                BuildOptions(outputPath, Path.Combine(
                        _tempRoot,
                        "model-pending-publication-trainer-held-decoy"),
                    trainingDataArtifactRoot: Path.Combine(
                        _tempRoot,
                        "missing-artifact-root-for-trainer-held"))
                with
                {
                    RepositoryOwnedDpoDatasetContext = dpoContext,
                    RepositoryOwnedModelCheckpointContext = modelContext
                });

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.False(File.Exists(pendingJournalPath));
            Assert.Equal(originalMetrics.Step, result.Value!.TrainMetrics.Step);
            Assert.Equal(originalMetrics.Loss, result.Value.TrainMetrics.Loss);
            Assert.Equal(
                originalPolicySha256,
                result.Value.RepositoryOwnedModelCheckpointEvidence!
                    .PublishedWeightsSha256);
            Assert.True(trainerSwapAttempts >= 2);
            Assert.True(trainerSwapSucceeded > 0 || trainerSwapRejected > 0);
        }
        finally
        {
            SetRepositoryOwnedModelCheckpointHeldFileValidationProbe(null);
        }
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointPendingPublicationRejectsTamperedMetricsBinding()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-pending-publication-metrics-tamper");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-pending-publication-metrics-tamper",
                count: 2));
        var coldStartReason =
            "pending publication metrics tamper fixture starts clean";
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-pending-publication-metrics-tamper",
            allowColdStart: true,
            coldStartReason: coldStartReason);
        var stepAddress = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(stepAddress.IsSuccess, stepAddress.ErrorMessage);
        CreateRepositoryOwnedCheckpointRootForTest(
            stepAddress.Value!.CheckpointRoot);

        var trainerPolicyPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(1));
        var trainerMetricsPath = Path.Combine(
            stepAddress.Value.CheckpointRoot,
            DpoTrainer.BuildMetricsFileName(1));
        CreateSeededPolicyModel(783UL).SaveCheckpoint(trainerPolicyPath);
        File.Copy(trainerPolicyPath, stepAddress.Value.WeightsPath);
        File.WriteAllText(
            trainerMetricsPath,
            JsonSerializer.Serialize(new DpoMetrics
            {
                Step = 1,
                Loss = 0.3f,
                ChosenReward = 0.6f,
                RejectedReward = 0.2f,
                RewardMargin = 0.4f,
                Accuracy = 0.9f
            }),
            Utf8NoBom);
        var pendingJournalPath =
            WriteRepositoryOwnedModelCheckpointPendingJournalForTest(
                modelContext,
                dpoContext,
                sealedDataset,
                stepAddress.Value,
                trainerPolicyPath,
                trainerMetricsPath,
                coldStartReason);
        File.WriteAllText(
            trainerMetricsPath,
            JsonSerializer.Serialize(new DpoMetrics
            {
                Step = 1,
                Loss = 9.9f,
                ChosenReward = 9.9f,
                RejectedReward = 0.0f,
                RewardMargin = 9.9f,
                Accuracy = 0.0f
            }),
            Utf8NoBom);

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(Path.Combine(
                    _tempRoot,
                    "reports",
                    "model-pending-publication-metrics-tamper.json"),
                Path.Combine(
                    _tempRoot,
                    "model-pending-publication-metrics-tamper-decoy"),
                trainingDataArtifactRoot: Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-pending-publication-metrics-tamper"))
            with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointPendingPublicationConflictErrorCode,
            result.ErrorCode);
        Assert.True(File.Exists(pendingJournalPath));
        Assert.False(File.Exists(stepAddress.Value.ManifestPath));
    }

    [Fact]
    public void Run_RepositoryOwnedModelCheckpointReportWriteRetryFinishesSamePublishedStep()
    {
        var dpoContext = CreateRepositoryOwnedDpoDatasetContext(
            "model-pending-publication-report-retry");
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            dpoContext,
            CreateRepositoryOwnedDpoPairs(
                dpoContext,
                "model-pending-publication-report-retry",
                count: 2));
        var coldStartReason =
            "pending publication report retry fixture starts clean";
        var modelContext = CreateRepositoryOwnedModelCheckpointContext(
            dpoContext,
            "model-pending-publication-report-retry",
            allowColdStart: true,
            coldStartReason: coldStartReason);
        var blockedOutputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-pending-publication-report-retry-blocked.json");
        Directory.CreateDirectory(Path.GetDirectoryName(blockedOutputPath)!);
        Directory.CreateDirectory(blockedOutputPath + ".tmp");

        var failedReportWrite = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(blockedOutputPath, Path.Combine(
                    _tempRoot,
                    "model-pending-publication-report-retry-decoy-first"),
                trainingDataArtifactRoot: Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-pending-publication-report-retry-first"))
            with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.False(failedReportWrite.IsSuccess);
        Assert.Equal("REPORT_WRITE_FAILED", failedReportWrite.ErrorCode);
        var stepOneAddress = modelContext.Request!.Address.CreateStepAddress(1);
        Assert.True(stepOneAddress.IsSuccess, stepOneAddress.ErrorMessage);
        var pendingJournalPath = Path.Combine(
            stepOneAddress.Value!.CheckpointRoot,
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointPendingPublicationFileName);
        Assert.True(File.Exists(stepOneAddress.Value.ManifestPath));
        Assert.True(File.Exists(pendingJournalPath));

        var retryOutputPath = Path.Combine(
            _tempRoot,
            "reports",
            "model-pending-publication-report-retry.json");
        var retry = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(retryOutputPath, Path.Combine(
                    _tempRoot,
                    "model-pending-publication-report-retry-decoy-second"),
                trainingDataArtifactRoot: Path.Combine(
                    _tempRoot,
                    "missing-artifact-root-for-pending-publication-report-retry-second"))
            with
            {
                RepositoryOwnedDpoDatasetContext = dpoContext,
                RepositoryOwnedModelCheckpointContext = modelContext
            });

        Assert.True(retry.IsSuccess, retry.ErrorMessage);
        Assert.True(File.Exists(retryOutputPath));
        Assert.False(File.Exists(pendingJournalPath));
        Assert.Equal(1,
            retry.Value!.RepositoryOwnedModelCheckpointEvidence!.PublishedStep);
        Assert.Null(retry.Value.RepositoryOwnedModelCheckpointEvidence.ParentStep);
        var stepTwoAddress = modelContext.Request.Address.CreateStepAddress(2);
        Assert.True(stepTwoAddress.IsSuccess, stepTwoAddress.ErrorMessage);
        Assert.False(File.Exists(stepTwoAddress.Value!.ManifestPath));
        Assert.False(Directory.Exists(Path.Combine(
            _tempRoot,
            "model-pending-publication-report-retry-decoy-second")));
    }

    [Theory]
    [InlineData("training-artifacts/.dpo-dataset-creation-leases/example.lock")]
    [InlineData("training-artifacts/models/translation-domain/dpo-train-evaluate/translation-policy/v0001/checkpoints/000001/weights.bin")]
    [InlineData("training-artifacts/models/translation-domain/dpo-train-evaluate/translation-policy/v0001/checkpoints/000001/manifest.json")]
    [InlineData("training-artifacts/datasets/translation-domain/dpo-train-evaluate/pairs.jsonl")]
    [InlineData("training-artifacts/arbitrary/.hidden")]
    [InlineData("training-artifacts/arbitrary/file.tmp")]
    [InlineData("training-artifacts/arbitrary/file.log")]
    [InlineData("training-artifacts/arbitrary/file.lock")]
    [InlineData("training-artifacts/arbitrary/file.stackdump")]
    [InlineData("training-artifacts/domain/bin/weights.bin")]
    [InlineData("training-artifacts/domain/obj/manifest.json")]
    [InlineData("training-artifacts/domain/TestResults/result.trx")]
    [InlineData("training-artifacts/domain/NuGet/Migrations/state.json")]
    public void RepositoryOwnedTrainingArtifactsRemainVisibleToGitForTranslationDpo(
        string repositoryRelativePath)
    {
        AssertRepositoryPathIsVisibleToGit(GetRepoRoot(), repositoryRelativePath);
    }

    [Fact]
    public void BuildRepositoryOwnedHandoff_PreservesTeacherGovernanceAndSideAuthorship()
    {
        var handoff = BuildRepositoryOwnedHandoffForValidation(
            "passport-positive");
        var record = Assert.Single(handoff.TrainingPairs);
        var passport = record.RepositoryOwnedPassport;

        Assert.NotNull(record.Admission);
        Assert.NotNull(passport);
        Assert.Equal(
            DpoPairSideAuthorKind.OpenTeacher,
            record.Admission!.ChosenAuthor);
        Assert.Equal(
            DpoPairSideAuthorKind.LocalModel,
            record.Admission.RejectedAuthor);
        Assert.Equal(
            DpoPairSideAuthorKind.OpenTeacher,
            passport!.ChosenAuthorship.AuthorKind);
        Assert.Equal(
            DpoPairSideAuthorKind.LocalModel,
            passport.RejectedAuthorship.AuthorKind);
        Assert.Equal(
            passport.TeacherProvenance,
            passport.ChosenAuthorship.OpenTeacherPassport);
        Assert.Null(passport.RejectedAuthorship.OpenTeacherPassport);
        Assert.Equal(
            passport.TeacherProvenance.SourceKind.ToString(),
            record.TeacherProvenance.SourceType);
        Assert.Equal(
            $"repository-owned-open-teacher:{passport.TeacherProvenance.RequestId}",
            record.TeacherProvenance.Author);
        Assert.Equal(
            passport.TeacherProvenance.HostingProvider!.Value.ToString(),
            record.TeacherProvenance.ModelProvider);
        Assert.Equal(
            passport.TeacherProvenance.ModelId,
            record.TeacherProvenance.ModelName);
        Assert.False(record.TeacherProvenance.ExternalApiUsed);
        Assert.False(record.TeacherProvenance.ClosedProprietaryTeacherUsed);

        var snapshot = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            handoff,
            BuildSettings());

        Assert.True(snapshot.IsSuccess, snapshot.ErrorMessage);
        Assert.Equal(1, snapshot.Value!.BatchBuild.TrainPairCount);
    }

    [Fact]
    public void BuildMutationBatchSnapshot_RepositoryOwnedMissingPassportFailsBeforeTrainerMutation()
    {
        var handoff = BuildRepositoryOwnedHandoffForValidation(
            "passport-missing");
        var record = Assert.Single(handoff.TrainingPairs);
        var mutated = handoff with
        {
            TrainingPairs =
            [
                record with
                {
                    RepositoryOwnedPassport = null
                }
            ]
        };

        var snapshot = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            mutated,
            BuildSettings());

        Assert.False(snapshot.IsSuccess);
        Assert.Equal("DPO_PAIR_REPOSITORY_PASSPORT_MISSING", snapshot.ErrorCode);
    }

    [Fact]
    public void BuildMutationBatchSnapshot_RepositoryOwnedChosenAuthorMutationFailsBeforeTrainerMutation()
    {
        var handoff = BuildRepositoryOwnedHandoffForValidation(
            "chosen-author-mutation");
        var record = Assert.Single(handoff.TrainingPairs);
        var mutatedAdmission = DpoTrainerPairCustoms.CreateAdmitted(
            record.TrainerPair.Prompt,
            record.TrainerPair.Chosen,
            record.TrainerPair.Rejected,
            DpoPairSideAuthorKind.LocalModel,
            record.Admission!.RejectedAuthor,
            nameof(TranslationDpoTrainingHandoffBuilder));
        var mutated = handoff with
        {
            TrainingPairs =
            [
                record with
                {
                    Admission = mutatedAdmission.Admission
                }
            ]
        };

        var snapshot = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            mutated,
            BuildSettings());

        Assert.False(snapshot.IsSuccess);
        Assert.Equal(
            "DPO_PAIR_ADMISSION_TRANSLATION_AUTHOR_MISMATCH",
            snapshot.ErrorCode);
    }

    [Fact]
    public void BuildMutationBatchSnapshot_RepositoryOwnedRejectedAuthorMutationFailsBeforeTrainerMutation()
    {
        var handoff = BuildRepositoryOwnedHandoffForValidation(
            "rejected-author-mutation");
        var record = Assert.Single(handoff.TrainingPairs);
        var mutatedAdmission = DpoTrainerPairCustoms.CreateAdmitted(
            record.TrainerPair.Prompt,
            record.TrainerPair.Chosen,
            record.TrainerPair.Rejected,
            record.Admission!.ChosenAuthor,
            DpoPairSideAuthorKind.OpenTeacher,
            nameof(TranslationDpoTrainingHandoffBuilder));
        var mutated = handoff with
        {
            TrainingPairs =
            [
                record with
                {
                    Admission = mutatedAdmission.Admission
                }
            ]
        };

        var snapshot = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            mutated,
            BuildSettings());

        Assert.False(snapshot.IsSuccess);
        Assert.Equal(
            "DPO_PAIR_ADMISSION_TRANSLATION_AUTHOR_MISMATCH",
            snapshot.ErrorCode);
    }

    [Fact]
    public void BuildMutationBatchSnapshot_RepositoryOwnedTeacherMutationFailsBeforeTrainerMutation()
    {
        var handoff = BuildRepositoryOwnedHandoffForValidation(
            "teacher-mutation");
        var record = Assert.Single(handoff.TrainingPairs);
        var passport = record.RepositoryOwnedPassport!;
        var mutatedTeacher = passport.TeacherProvenance with
        {
            RequestId = passport.TeacherProvenance.RequestId + "-mutated"
        };
        var mutated = handoff with
        {
            TrainingPairs =
            [
                record with
                {
                    TeacherProvenance = record.TeacherProvenance with
                    {
                        Author =
                            $"repository-owned-open-teacher:{mutatedTeacher.RequestId}"
                    },
                    RepositoryOwnedPassport = passport with
                    {
                        TeacherProvenance = mutatedTeacher
                    }
                }
            ]
        };

        var snapshot = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            mutated,
            BuildSettings());

        Assert.False(snapshot.IsSuccess);
        Assert.Equal(
            "DPO_PAIR_REPOSITORY_PASSPORT_AUTHORSHIP_MISMATCH",
            snapshot.ErrorCode);
    }

    [Fact]
    public void BuildMutationBatchSnapshot_RepositoryOwnedGovernanceMutationFailsBeforeTrainerMutation()
    {
        var handoff = BuildRepositoryOwnedHandoffForValidation(
            "governance-mutation");
        var record = Assert.Single(handoff.TrainingPairs);
        var passport = record.RepositoryOwnedPassport!;
        var mutated = handoff with
        {
            TrainingPairs =
            [
                record with
                {
                    RepositoryOwnedPassport = passport with
                    {
                        GovernanceDecision = new TrainingDataGovernanceDecision(
                            licenseAccepted: false,
                            isQuarantined: false,
                            passport.GovernanceDecision.EvidenceRefs)
                    }
                }
            ]
        };

        var snapshot = TranslationDpoTrainerBridge.BuildMutationBatchSnapshot(
            mutated,
            BuildSettings());

        Assert.False(snapshot.IsSuccess);
        Assert.Equal("TRAINING_DATA_LICENSE_NOT_ACCEPTED", snapshot.ErrorCode);
    }

    [Fact]
    public void PrepareRequest_RepositoryOwnedRequestIdentityBindsEveryTeacherVisibleSourceField()
    {
        var authoring = CreateAuthoringContext(
            CreateRepositoryOwnedDpoRequestIdentity(
                "source-row-identity-fields"),
            static () => throw new InvalidOperationException(
                "PrepareRequest must not create a teacher client."));
        var row = CreateTeacherVisibleSourceRow("base");
        var baseline = PrepareRepositoryOwnedRequestIdentitySha256(
            [row],
            authoring);
        var mutations = new Dictionary<string, TranslationTrainReadyPair>
        {
            ["id"] = row with { Id = row.Id + "-changed" },
            ["source"] = row with { Source = row.Source + "-changed" },
            ["direction"] = row with { Direction = "RU_EN" },
            ["prompt"] = row with { Prompt = row.Prompt + " Keep tone formal." },
            ["input"] = row with { ASide = row.ASide + " Extra source sentence." },
            ["paid reference"] = row with { Chosen = row.Chosen + " Extra paid detail." },
            ["weak candidate"] = row with { Rejected = row.Rejected + " Extra weak detail." }
        };

        foreach (var mutation in mutations)
        {
            var changed = PrepareRepositoryOwnedRequestIdentitySha256(
                [mutation.Value],
                authoring);
            Assert.NotEqual(baseline, changed);
        }
    }

    [Fact]
    public void PrepareRequest_RepositoryOwnedRequestIdentityIsStableAndOrderSensitive()
    {
        var authoring = CreateAuthoringContext(
            CreateRepositoryOwnedDpoRequestIdentity(
                "source-row-identity-order"),
            static () => throw new InvalidOperationException(
                "PrepareRequest must not create a teacher client."));
        var firstRow = CreateTeacherVisibleSourceRow("first");
        var secondRow = CreateTeacherVisibleSourceRow("second");

        var first = PrepareRepositoryOwnedRequestIdentitySha256(
            [firstRow, secondRow],
            authoring);
        var repeat = PrepareRepositoryOwnedRequestIdentitySha256(
            [firstRow, secondRow],
            authoring);
        var reversed = PrepareRepositoryOwnedRequestIdentitySha256(
            [secondRow, firstRow],
            authoring);

        Assert.Equal(first, repeat);
        Assert.NotEqual(first, reversed);
    }

    [Fact]
    public async Task Run_RepositoryOwnedCleanMissingCreatesOnceThenReusesIdenticalSealedTrainingData()
    {
        var identity = PrepareRepositoryOwnedRequestIdentityFromRepoSeed(
            "clean-miss");
        var teacher = new RecordingTranslationDpoTeacher(identity);
        var teacherFactoryCalls = 0;
        var context = CreateCleanMissRepositoryOwnedDpoDatasetContext(
            "clean-miss",
            CreateAuthoringContext(
                identity,
                () =>
                {
                    teacherFactoryCalls++;
                    return OperationResult<ITrainingTeacherInferenceClient>.Ok(
                        teacher);
                }));
        var artifactRoot = Path.Combine(
            _tempRoot,
            "missing-artifact-root-clean-miss");
        var firstCheckpointDirectory = Path.Combine(
            _tempRoot,
            "repository-owned-clean-miss-checkpoints-first");
        var firstOutputPath = Path.Combine(
            _tempRoot,
            "reports",
            "repository-owned-clean-miss-first.json");
        var firstBroker = new RecordingLeaseBroker(CreateAuthority());

        var first = await TranslationDpoTrainEvaluateRunner.RunAsync(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            firstBroker,
            BuildOptions(firstOutputPath, firstCheckpointDirectory, artifactRoot)
                with
                {
                    RepositoryOwnedDpoDatasetContext = context
                });

        Assert.True(first.IsSuccess, first.ErrorMessage);
        var firstReport = first.Value!;
        Assert.NotNull(firstReport.RepositoryOwnedDpoDatasetEvidence);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .NewlyPersistedRepositoryDataset.ToString(),
            firstReport.RepositoryOwnedDpoDatasetEvidence!.ResolutionSource);
        Assert.False(
            firstReport.RepositoryOwnedDpoDatasetEvidence
                .LegacyTrainingDataArtifactLoaded);
        Assert.Equal(1, teacherFactoryCalls);
        Assert.Equal(firstReport.TrainPairCount, teacher.CallCount);
        var firstContentRows = firstBroker.LastTrainingContent!.Rows
            .Select(row => row.Sha256)
            .ToArray();
        var sealedIdentity = RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestIdentityOnly(context.Address!);
        Assert.True(sealedIdentity.IsSuccess, sealedIdentity.ErrorMessage);
        var firstSealed = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
            context.Address!,
            sealedIdentity.Value!.RequestIdentity);
        Assert.True(firstSealed.IsSuccess, firstSealed.ErrorMessage);
        Assert.All(firstSealed.Value!.Pairs, pair =>
        {
            Assert.StartsWith("open-qwen-chosen-", pair.Chosen);
            Assert.StartsWith("open-qwen-rejected-", pair.Rejected);
            Assert.DoesNotContain(
                "paid reference",
                pair.Chosen,
                StringComparison.OrdinalIgnoreCase);
        });
        var firstPairFiles = firstSealed.Value.Manifest.OrderedPairs
            .Select(pair => (
                pair.RepositoryRelativePairFilePath,
                pair.PairFileSha256))
            .ToArray();

        var secondCheckpointDirectory = Path.Combine(
            _tempRoot,
            "repository-owned-clean-miss-checkpoints-second");
        var secondOutputPath = Path.Combine(
            _tempRoot,
            "reports",
            "repository-owned-clean-miss-second.json");
        var secondBroker = new RecordingLeaseBroker(CreateAuthority());

        var second = await TranslationDpoTrainEvaluateRunner.RunAsync(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            secondBroker,
            BuildOptions(
                    secondOutputPath,
                    secondCheckpointDirectory,
                    artifactRoot)
                with
                {
                    RepositoryOwnedDpoDatasetContext = context
                });

        Assert.True(second.IsSuccess, second.ErrorMessage);
        var secondReport = second.Value!;
        Assert.NotNull(secondReport.RepositoryOwnedDpoDatasetEvidence);
        Assert.Equal(
            RepositoryOwnedDpoDatasetResolutionSource
                .ExistingSealedRepositoryDataset.ToString(),
            secondReport.RepositoryOwnedDpoDatasetEvidence!.ResolutionSource);
        Assert.False(
            secondReport.RepositoryOwnedDpoDatasetEvidence
                .LegacyTrainingDataArtifactLoaded);
        Assert.Equal(
            firstReport.RepositoryOwnedDpoDatasetEvidence.ManifestSha256,
            secondReport.RepositoryOwnedDpoDatasetEvidence.ManifestSha256);
        Assert.Equal(firstReport.TrainedPairIds, secondReport.TrainedPairIds);
        Assert.Equal(
            firstContentRows,
            secondBroker.LastTrainingContent!.Rows
                .Select(row => row.Sha256)
                .ToArray());
        Assert.Equal(1, teacherFactoryCalls);
        Assert.Equal(firstReport.TrainPairCount, teacher.CallCount);
        var secondSealed = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
            context.Address!,
            sealedIdentity.Value.RequestIdentity);
        Assert.True(secondSealed.IsSuccess, secondSealed.ErrorMessage);
        Assert.Equal(
            firstPairFiles,
            secondSealed.Value!.Manifest.OrderedPairs
                .Select(pair => (
                    pair.RepositoryRelativePairFilePath,
                    pair.PairFileSha256))
                .ToArray());
    }

    [Fact]
    public async Task Run_RepositoryOwnedCleanMissRejectsStaleSealedDatasetWhenCurrentInputChanges()
    {
        var identity = CreateRepositoryOwnedDpoRequestIdentity(
            "clean-miss-stale-source");
        var teacher = new RecordingTranslationDpoTeacher(identity);
        var teacherFactoryCalls = 0;
        var context = CreateCleanMissRepositoryOwnedDpoDatasetContext(
            "clean-miss-stale-source",
            CreateAuthoringContext(
                identity,
                () =>
                {
                    teacherFactoryCalls++;
                    return OperationResult<ITrainingTeacherInferenceClient>.Ok(
                        teacher);
                }));
        var artifactRoot = CreateTrainingDataArtifactRoot();
        var firstBroker = new RecordingLeaseBroker(CreateAuthority());

        var first = await TranslationDpoTrainEvaluateRunner.RunAsync(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            firstBroker,
            BuildOptions(
                    Path.Combine(
                        _tempRoot,
                        "reports",
                        "repository-owned-clean-miss-stale-first.json"),
                    Path.Combine(
                        _tempRoot,
                        "repository-owned-clean-miss-stale-first-checkpoints"),
                    artifactRoot)
                with
                {
                    RepositoryOwnedDpoDatasetContext = context
                });

        Assert.True(first.IsSuccess, first.ErrorMessage);
        var teacherCallsAfterFirstRun = teacher.CallCount;
        var firstManifestBytes = File.ReadAllBytes(context.Address!.ManifestPath);
        var changedSeedCorpusPath = WriteMutatedCorpus(nodes =>
        {
            var trainRow = nodes.First(IsTrainRow);
            trainRow["input_text"] =
                trainRow["input_text"]!.GetValue<string>() +
                " Current source input changed while the paid answer stayed the same.";
        });
        var changedArtifactRoot =
            CreateTrainingDataArtifactRoot(changedSeedCorpusPath);
        var secondOutputPath = Path.Combine(
            _tempRoot,
            "reports",
            "repository-owned-clean-miss-stale-second.json");
        var secondCheckpointDirectory = Path.Combine(
            _tempRoot,
            "repository-owned-clean-miss-stale-second-checkpoints");
        var secondBroker = new RecordingLeaseBroker(CreateAuthority());

        var second = await TranslationDpoTrainEvaluateRunner.RunAsync(
            changedSeedCorpusPath,
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            secondBroker,
            BuildOptions(
                    secondOutputPath,
                    secondCheckpointDirectory,
                    changedArtifactRoot)
                with
                {
                    RepositoryOwnedDpoDatasetContext = context
                });

        Assert.False(second.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer
                .RequestIdentityMismatchErrorCode,
            second.ErrorCode);
        Assert.Equal(1, teacherFactoryCalls);
        Assert.Equal(teacherCallsAfterFirstRun, teacher.CallCount);
        Assert.Null(secondBroker.LastTrainingContent);
        Assert.Equal(firstManifestBytes, File.ReadAllBytes(context.Address.ManifestPath));
        Assert.False(File.Exists(secondOutputPath));
        Assert.False(Directory.Exists(secondCheckpointDirectory));
    }

    [Theory]
    [InlineData("paid-verbatim")]
    [InlineData("malformed-json")]
    [InlineData("wrong-provenance")]
    public async Task Run_RepositoryOwnedCleanMissRejectsUnsafeTeacherOutputWithoutSealing(
        string failureMode)
    {
        var identity = CreateRepositoryOwnedDpoRequestIdentity(
            "authoring-failure-" + failureMode);
        var teacher = new RecordingTranslationDpoTeacher(identity, failureMode);
        var context = CreateCleanMissRepositoryOwnedDpoDatasetContext(
            "authoring-failure-" + failureMode,
            CreateAuthoringContext(
                identity,
                () => OperationResult<ITrainingTeacherInferenceClient>.Ok(
                    teacher)));
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "authoring-failure-" + failureMode + ".json");
        var checkpointDirectory = Path.Combine(
            _tempRoot,
            "authoring-failure-checkpoints-" + failureMode);

        var result = await TranslationDpoTrainEvaluateRunner.RunAsync(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(),
            BuildOptions(
                    outputPath,
                    checkpointDirectory,
                    CreateTrainingDataArtifactRoot())
                with
                {
                    RepositoryOwnedDpoDatasetContext = context
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(1, teacher.CallCount);
        Assert.False(File.Exists(context.Address!.ManifestPath));
        Assert.False(File.Exists(outputPath));
        Assert.False(Directory.Exists(checkpointDirectory));
    }

    [Fact]
    public void Run_RepositoryOwnedIdentityMismatchStopsBeforeCheckpointWriteAndTrainerMutation()
    {
        var context = CreateRepositoryOwnedDpoDatasetContext("mismatch-source");
        SealRepositoryOwnedDpoDataset(
            context,
            CreateRepositoryOwnedDpoPairs(context, "mismatch-source", count: 2));
        var mismatchedContext = context with
        {
            RequestIdentity = CreateRepositoryOwnedDpoRequestIdentity(
                "mismatch-request")
        };
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "repository-owned-mismatch.json");
        var checkpointDirectory = Path.Combine(
            _tempRoot,
            "repository-owned-mismatch-checkpoints");
        var broker = new RecordingLeaseBroker(CreateAuthority());

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            broker,
            BuildOptions(
                    outputPath,
                    checkpointDirectory,
                    Path.Combine(_tempRoot, "missing-artifact-mismatch"))
                with
                {
                    RepositoryOwnedDpoDatasetContext = mismatchedContext
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer
                .RequestIdentityMismatchErrorCode,
            result.ErrorCode);
        Assert.Null(broker.LastTrainingContent);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(Path.Combine(
            checkpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public void Run_RepositoryOwnedCorruptionStopsBeforeCheckpointWriteAndTrainerMutation()
    {
        var context = CreateRepositoryOwnedDpoDatasetContext("corruption");
        SealRepositoryOwnedDpoDataset(
            context,
            CreateRepositoryOwnedDpoPairs(context, "corruption", count: 2));
        var firstPairFile = Directory
            .EnumerateFiles(context.Address!.PairsRoot, "*.json")
            .Order(StringComparer.Ordinal)
            .First();
        File.AppendAllText(firstPairFile, "\n{\"corrupt\":true}\n", Utf8NoBom);
        var outputPath = Path.Combine(
            _tempRoot,
            "reports",
            "repository-owned-corruption.json");
        var checkpointDirectory = Path.Combine(
            _tempRoot,
            "repository-owned-corruption-checkpoints");
        var broker = new RecordingLeaseBroker(CreateAuthority());

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            broker,
            BuildOptions(
                    outputPath,
                    checkpointDirectory,
                    Path.Combine(_tempRoot, "missing-artifact-corruption"))
                with
                {
                    RepositoryOwnedDpoDatasetContext = context
                });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode,
            result.ErrorCode);
        Assert.Null(broker.LastTrainingContent);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(Path.Combine(
            checkpointDirectory,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public void RunFreshLoadContinueEvaluate_LoadsRunnerCheckpointAndKeepsHeldOutEvalOnly()
    {
        var initialOutputPath = Path.Combine(_tempRoot, "reports", "translation-dpo-run-report.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var referenceModel = new LlmModel(BuildTinyConfig());
        var freshLoadTarget = new LlmModel(BuildTinyConfig());
        var initial = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => referenceModel,
            () => freshLoadTarget,
            broker,
            BuildOptions(initialOutputPath));
        Assert.True(initial.IsSuccess, initial.ErrorMessage);
        Assert.True(initial.Value!.CheckpointWritten);
        Assert.True(File.Exists(initial.Value.CheckpointPath));

        var continuationDir = Path.Combine(_tempRoot, "continued-checkpoints");
        var lifecycleOutputPath = Path.Combine(_tempRoot, "reports", "translation-dpo-lifecycle-report.json");

        var lifecycle = TranslationDpoTrainEvaluateRunner.RunFreshLoadContinueEvaluate(
            GetRepoSeedDataPath(),
            initial.Value,
            () => freshLoadTarget,
            () => referenceModel,
            broker,
            BuildLifecycleOptions(lifecycleOutputPath, continuationDir));

        Assert.True(lifecycle.IsSuccess, lifecycle.ErrorMessage);
        var report = lifecycle.Value!;
        Assert.True(File.Exists(lifecycleOutputPath));
        Assert.Equal(TranslationDpoTrainEvaluateRunner.CheckpointLifecyclePipeline, report.Pipeline);
        Assert.Equal(TranslationDpoTrainEvaluateRunner.TrainerLoadCallPath, report.TrainerLoadCallPath);
        Assert.True(report.CheckpointFileUsed);
        Assert.True(report.LoadedOrResumed);
        Assert.True(report.PolicyCheckpointLoaded);
        Assert.False(report.ReferenceModelRestoredFromCheckpoint);
        Assert.Contains("policy model only", report.ReferenceModelBoundary, StringComparison.Ordinal);
        Assert.Equal(initial.Value.CheckpointPath, report.LoadedCheckpointPath);
        Assert.NotEqual(initial.Value.CheckpointPath, report.ContinuationCheckpointPath);
        Assert.NotNull(report.ReproducibilityEvidence);
        Assert.Equal(
            initial.Value.ReproducibilityEvidence!.ContextSha256,
            report.ReproducibilityEvidence!.ContextSha256);
        Assert.NotNull(report.SeededLoraTopologyEvidence);
        Assert.Equal(
            initial.Value.SeededLoraTopologyEvidence!.TopologySha256,
            report.SeededLoraTopologyEvidence!.TopologySha256);
        Assert.Equal(
            initial.Value.PolicyCheckpointSha256,
            report.LoadedPolicyCheckpointSha256);
        Assert.True(report.ContinuationCheckpointWritten);
        Assert.True(File.Exists(report.ContinuationCheckpointPath));
        Assert.Equal(
            ComputeSha256(report.ContinuationCheckpointPath!),
            report.ContinuationPolicyCheckpointSha256);
        Assert.True(report.ContinuationTrainerMetricsWritten);
        Assert.True(File.Exists(report.ContinuationTrainerMetricsPath));
        Assert.False(File.Exists(Path.Combine(continuationDir, "dpo_step_000003.json")));
        Assert.False(File.Exists(Path.Combine(continuationDir, DpoTrainer.BuildPolicyCheckpointFileName(3))));
        Assert.True(File.Exists(Path.Combine(continuationDir, "dpo_step_000004.json")));
        Assert.True(File.Exists(Path.Combine(continuationDir, DpoTrainer.BuildPolicyCheckpointFileName(4))));
        Assert.False(File.Exists(Path.Combine(continuationDir, "dpo_step_000005.json")));
        Assert.False(File.Exists(Path.Combine(continuationDir, DpoTrainer.BuildPolicyCheckpointFileName(5))));
        Assert.Equal(16, report.SourceCorpusRowCount);
        Assert.Equal(12, report.TrainSourceRowCount);
        Assert.Equal(4, report.HeldOutSourceRowCount);
        Assert.Equal(12, report.ContinueTrainPairCount);
        Assert.Equal(4, report.HeldOutCount);
        Assert.Equal(4, report.HeldOutExcludedFromContinuationTrainingCount);
        Assert.Equal(12, report.TrainExcludedFromFreshEvaluationCount);
        Assert.Empty(report.ContinuedTrainingPairIds.Intersect(report.FreshHeldOutEvaluationPairIds, StringComparer.Ordinal));
        Assert.Empty(report.ContinuedTrainingPairIds.Intersect(report.PostHeldOutEvaluationPairIds, StringComparer.Ordinal));
        Assert.Empty(report.PostHeldOutEvaluationPairIds.Intersect(report.ExcludedTrainPairIds, StringComparer.Ordinal));
        Assert.False(float.IsNaN(report.FreshLoadedHeldOutMetrics.Loss));
        Assert.False(float.IsInfinity(report.FreshLoadedHeldOutMetrics.Loss));
        Assert.False(float.IsNaN(report.ContinueTrainMetrics.Loss));
        Assert.False(float.IsInfinity(report.ContinueTrainMetrics.Loss));
        Assert.False(float.IsNaN(report.PostContinueHeldOutMetrics.Loss));
        Assert.False(float.IsInfinity(report.PostContinueHeldOutMetrics.Loss));
        Assert.False(report.HeldOutRowsPassedToContinueTraining);
        Assert.False(report.TrainRowsUsedForFreshEvaluation);
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.SemanticQualityReadinessClaimed);
        Assert.Equal(2, broker.TrainingRunIds.Count);
        Assert.StartsWith("translation-initial-", broker.TrainingRunIds[0], StringComparison.Ordinal);
        Assert.StartsWith("translation-continuation-", broker.TrainingRunIds[1], StringComparison.Ordinal);
        Assert.NotEqual(broker.TrainingRunIds[0], broker.TrainingRunIds[1]);
        Assert.Contains("semantic-quality readiness remains closed", report.SemanticQualityReadinessClosedReason, StringComparison.Ordinal);

        using var lifecycleJson = JsonDocument.Parse(File.ReadAllText(lifecycleOutputPath, Utf8NoBom));
        Assert.True(lifecycleJson.RootElement.GetProperty("loaded_or_resumed").GetBoolean());
        Assert.True(lifecycleJson.RootElement.GetProperty("policy_checkpoint_loaded").GetBoolean());
        Assert.False(lifecycleJson.RootElement.GetProperty("reference_model_restored_from_checkpoint").GetBoolean());
        Assert.False(lifecycleJson.RootElement.GetProperty("semantic_quality_claimed").GetBoolean());
        Assert.Equal(initial.Value.CheckpointPath, lifecycleJson.RootElement.GetProperty("loaded_checkpoint_path").GetString());
        Assert.Equal(
            initial.Value.ReproducibilityEvidence!.ContextSha256,
            lifecycleJson.RootElement
                .GetProperty("reproducibility_evidence")
                .GetProperty("context_sha256")
                .GetString());
        Assert.Equal(
            initial.Value.PolicyCheckpointSha256,
            lifecycleJson.RootElement
                .GetProperty("loaded_policy_checkpoint_sha256")
                .GetString());
        Assert.Equal(4, lifecycleJson.RootElement.GetProperty("held_out_excluded_from_continuation_training_count").GetInt32());
    }

    [Fact]
    public void Run_SameServerContextWritesByteIdenticalFirstPolicyCheckpointAndSeedChangeChangesIt()
    {
        var policyBase = SaveBaseCheckpoint("policy-base.bin");
        var referenceBase = SaveBaseCheckpoint("reference-base.bin");
        var freshLoadBase = SaveBaseCheckpoint("fresh-load-base.bin");

        var first = RunInitialWithBaseCheckpoints(
            rootSeed: 111UL,
            outputName: "same-seed-first.json",
            checkpointDirectoryName: "same-seed-first-checkpoints",
            policyBase,
            referenceBase,
            freshLoadBase);
        var second = RunInitialWithBaseCheckpoints(
            rootSeed: 111UL,
            outputName: "same-seed-second.json",
            checkpointDirectoryName: "same-seed-second-checkpoints",
            policyBase,
            referenceBase,
            freshLoadBase);
        var changedSeed = RunInitialWithBaseCheckpoints(
            rootSeed: 222UL,
            outputName: "changed-seed.json",
            checkpointDirectoryName: "changed-seed-checkpoints",
            policyBase,
            referenceBase,
            freshLoadBase);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.True(changedSeed.IsSuccess, changedSeed.ErrorMessage);
        Assert.Equal(
            ComputeSha256(first.Value!.CheckpointPath!),
            first.Value.PolicyCheckpointSha256);
        Assert.Equal(
            first.Value.PolicyCheckpointSha256,
            second.Value!.PolicyCheckpointSha256);
        Assert.NotEqual(
            first.Value.PolicyCheckpointSha256,
            changedSeed.Value!.PolicyCheckpointSha256);
    }

    [Fact]
    public void RunFreshLoadContinueEvaluate_UnseededFreshLoadFailsBeforeContinuationLeaseAndArtifacts()
    {
        var initialOutputPath = Path.Combine(
            _tempRoot,
            "reports",
            "initial-before-unseeded-fresh-load.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var referenceModel = new LlmModel(BuildTinyConfig());
        var preparedFreshLoad = new LlmModel(BuildTinyConfig());
        var initial = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => referenceModel,
            () => preparedFreshLoad,
            broker,
            BuildOptions(initialOutputPath));
        Assert.True(initial.IsSuccess, initial.ErrorMessage);

        var continuationDir = Path.Combine(
            _tempRoot,
            "continued-unseeded-fresh-load");
        var lifecycleOutputPath = Path.Combine(
            _tempRoot,
            "reports",
            "unseeded-fresh-load-lifecycle.json");
        var lifecycle = TranslationDpoTrainEvaluateRunner.RunFreshLoadContinueEvaluate(
            GetRepoSeedDataPath(),
            initial.Value!,
            () => new LlmModel(BuildTinyConfig()),
            () => referenceModel,
            broker,
            BuildLifecycleOptions(lifecycleOutputPath, continuationDir));

        Assert.False(lifecycle.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .SeededLoraTopologyPreparationFailedErrorCode,
            lifecycle.ErrorCode);
        Assert.Single(broker.TrainingRunIds);
        Assert.False(File.Exists(lifecycleOutputPath));
        Assert.False(Directory.Exists(continuationDir));
    }

    [Fact]
    public void Run_MissingReproducibilityContextFailsBeforeFactoriesBrokerAndArtifacts()
    {
        var outputPath = Path.Combine(_tempRoot, "reports", "missing-reproducibility.json");
        var policyFactoryCalled = false;
        var referenceFactoryCalled = false;
        var freshLoadFactoryCalled = false;
        var broker = new RecordingLeaseBroker(CreateAuthority());

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () =>
            {
                policyFactoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                referenceFactoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                freshLoadFactoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            broker,
            BuildOptions(outputPath) with { ReproducibilityContext = null });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner.ReproducibilityContextRequiredErrorCode,
            result.ErrorCode);
        Assert.False(policyFactoryCalled);
        Assert.False(referenceFactoryCalled);
        Assert.False(freshLoadFactoryCalled);
        Assert.Null(broker.LastTrainingContent);
        Assert.False(File.Exists(outputPath));
        Assert.False(Directory.Exists(_checkpointDir));
    }

    [Fact]
    public void Run_InvalidReproducibilityContextFailsBeforeFactoriesBrokerAndArtifacts()
    {
        var outputPath = Path.Combine(_tempRoot, "reports", "invalid-reproducibility.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var factoryCalled = false;

        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () =>
            {
                factoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            broker,
            BuildOptions(outputPath) with
            {
                ReproducibilityContext =
                    BuildReproducibilityContext() with
                    {
                        ReferenceStreamDomain = "translation.policy"
                    }
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner.ReproducibilityContextInvalidErrorCode,
            result.ErrorCode);
        Assert.False(factoryCalled);
        Assert.Null(broker.LastTrainingContent);
        Assert.False(File.Exists(outputPath));
        Assert.False(Directory.Exists(_checkpointDir));
    }

    [Fact]
    public void RunFreshLoadContinueEvaluate_MissingSourceEvidenceFailsBeforeFactoriesBrokerAndArtifacts()
    {
        var initialOutputPath = Path.Combine(_tempRoot, "reports", "initial-before-missing-evidence.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var initial = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            broker,
            BuildOptions(initialOutputPath));
        Assert.True(initial.IsSuccess, initial.ErrorMessage);

        var factoryCalled = false;
        var continuationDir = Path.Combine(_tempRoot, "continued-missing-evidence");
        var lifecycleOutputPath = Path.Combine(_tempRoot, "reports", "missing-source-evidence.json");
        var lifecycle = TranslationDpoTrainEvaluateRunner.RunFreshLoadContinueEvaluate(
            GetRepoSeedDataPath(),
            initial.Value! with { ReproducibilityEvidence = null },
            () =>
            {
                factoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            broker,
            BuildLifecycleOptions(lifecycleOutputPath, continuationDir));

        Assert.False(lifecycle.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner.ReproducibilityEvidenceRequiredErrorCode,
            lifecycle.ErrorCode);
        Assert.False(factoryCalled);
        Assert.Single(broker.TrainingRunIds);
        Assert.False(File.Exists(lifecycleOutputPath));
        Assert.False(Directory.Exists(continuationDir));
    }

    [Fact]
    public void RunFreshLoadContinueEvaluate_MismatchedContextFailsBeforeFactoriesBrokerAndArtifacts()
    {
        var initialOutputPath = Path.Combine(_tempRoot, "reports", "initial-before-mismatch.json");
        var broker = new RecordingLeaseBroker(CreateAuthority());
        var initial = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            broker,
            BuildOptions(initialOutputPath));
        Assert.True(initial.IsSuccess, initial.ErrorMessage);

        var factoryCalled = false;
        var continuationDir = Path.Combine(_tempRoot, "continued-mismatch");
        var lifecycleOutputPath = Path.Combine(_tempRoot, "reports", "mismatched-context.json");
        var lifecycle = TranslationDpoTrainEvaluateRunner.RunFreshLoadContinueEvaluate(
            GetRepoSeedDataPath(),
            initial.Value!,
            () =>
            {
                factoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            () =>
            {
                factoryCalled = true;
                return new LlmModel(BuildTinyConfig());
            },
            broker,
            BuildLifecycleOptions(lifecycleOutputPath, continuationDir) with
            {
                ReproducibilityContext = DpoTrainingReproducibilityContext.CreateCurrent(
                    987654321UL,
                    new string('e', 64),
                    BuildSeededLoraTopology(),
                    "translation.policy",
                    "translation.reference",
                    "translation.fresh-load")
            });

        Assert.False(lifecycle.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner.ReproducibilityEvidenceMismatchErrorCode,
            lifecycle.ErrorCode);
        Assert.False(factoryCalled);
        Assert.Single(broker.TrainingRunIds);
        Assert.False(File.Exists(lifecycleOutputPath));
        Assert.False(Directory.Exists(continuationDir));
    }

    [Fact]
    public void Run_WithoutBrokerFailsBeforeMutationOrCheckpointPublication()
    {
        var outputPath = Path.Combine(_tempRoot, "reports", "no-broker.json");

#pragma warning disable CS0618 // Intentional compatibility coverage for the brokerless fail-closed overload.
        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            BuildTrainer(),
            BuildOptions(outputPath));
#pragma warning restore CS0618

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .MutationAdmissionBrokerRequiredErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(Path.Combine(
            _checkpointDir,
            DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public void RunFreshLoadContinueEvaluate_WithoutBrokerFailsBeforeInputInspectionOrPublication()
    {
        var outputPath = Path.Combine(_tempRoot, "reports", "no-broker-lifecycle.json");
        var continuationDirectory = Path.Combine(_tempRoot, "no-broker-continuation");
        var policyFactoryCalled = false;
        var referenceFactoryCalled = false;

#pragma warning disable CS0618 // Intentional compatibility coverage for the brokerless fail-closed overload.
        var result = TranslationDpoTrainEvaluateRunner.RunFreshLoadContinueEvaluate(
            Path.Combine(_tempRoot, "missing-private-seed.jsonl"),
            null!,
            () =>
            {
                policyFactoryCalled = true;
                throw new InvalidOperationException("Policy factory must not be inspected.");
            },
            () =>
            {
                referenceFactoryCalled = true;
                throw new InvalidOperationException("Reference factory must not be inspected.");
            },
            BuildLifecycleOptions(outputPath, continuationDirectory));
#pragma warning restore CS0618

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationDpoTrainEvaluateRunner
                .MutationAdmissionBrokerRequiredErrorCode,
            result.ErrorCode);
        Assert.False(policyFactoryCalled);
        Assert.False(referenceFactoryCalled);
        Assert.False(File.Exists(outputPath));
        Assert.False(Directory.Exists(continuationDirectory));
    }

    [Fact]
    public void Run_InactiveServerOwnedBrokerFailsClosedBeforeMutation()
    {
        var outputPath = Path.Combine(_tempRoot, "reports", "inactive-broker.json");
        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            CreateAuthority(static () => false),
            BuildOptions(outputPath));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(outputPath));
        Assert.False(Directory.Exists(_checkpointDir));
    }

    [Fact]
    public void Run_RevokedTranslationLeaseFailsClosedBeforeTrainerConstruction()
    {
        var outputPath = Path.Combine(_tempRoot, "reports", "revoked-lease.json");
        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            () => new LlmModel(BuildTinyConfig()),
            new RevokeLeaseOnAcquireBroker(CreateAuthority()),
            BuildOptions(outputPath));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority
                .AdmissionRevokedErrorCode,
            result.ErrorCode);
        Assert.False(File.Exists(outputPath));
        Assert.False(Directory.Exists(_checkpointDir));
    }

    [Fact]
    public void Run_RejectsHeldOutLeakageBeforeTrainingAndWritesNoArtifacts()
    {
        var contaminatedCorpusPath = WriteMutatedCorpus(nodes =>
        {
            var trainInput = nodes.First(IsTrainRow)["input_text"]!.GetValue<string>();
            nodes.First(IsHeldOutRow)["input_text"] = trainInput;
        });
        var outputPath = Path.Combine(_tempRoot, "reports", "leakage-report.json");

#pragma warning disable CS0618 // Intentional legacy input-validation coverage for the brokerless overload.
        var result = TranslationDpoTrainEvaluateRunner.Run(
            contaminatedCorpusPath,
            BuildTrainer(),
            BuildOptions(outputPath));
#pragma warning restore CS0618

        Assert.False(result.IsSuccess);
        Assert.Equal("SEED_CORPUS_LOAD_FAILED", result.ErrorCode);
        Assert.Contains("held-out row", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000001.json")));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public void Run_RejectsIllegalProvenanceAndSemanticQualityClaimBeforeTraining()
    {
        var closedTeacherPath = WriteMutatedCorpus(nodes =>
        {
            var provenance = nodes.First(IsTrainRow)["teacher_provenance"]!.AsObject();
            provenance["closed_proprietary_teacher_used"] = true;
        });
        var semanticClaimPath = WriteMutatedCorpus(nodes =>
        {
            nodes.First(IsTrainRow)["semantic_quality_claimed"] = true;
        });

#pragma warning disable CS0618 // Intentional legacy input-validation coverage for the brokerless overload.
        var closedTeacher = TranslationDpoTrainEvaluateRunner.Run(
            closedTeacherPath,
            BuildTrainer(),
            BuildOptions(Path.Combine(_tempRoot, "reports", "closed-teacher.json")));
        var semanticClaim = TranslationDpoTrainEvaluateRunner.Run(
            semanticClaimPath,
            BuildTrainer(),
            BuildOptions(Path.Combine(_tempRoot, "reports", "semantic-claim.json")));
#pragma warning restore CS0618

        Assert.False(closedTeacher.IsSuccess);
        Assert.Equal("SEED_CORPUS_LOAD_FAILED", closedTeacher.ErrorCode);
        Assert.Contains("closed", closedTeacher.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(semanticClaim.IsSuccess);
        Assert.Equal("SEED_CORPUS_LOAD_FAILED", semanticClaim.ErrorCode);
        Assert.Contains("semantic_quality_claimed", semanticClaim.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000001.json")));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public void Run_RejectsInvalidTrainingDataArtifactManifestBeforeTraining()
    {
        var artifactRoot = CreateTrainingDataArtifactRoot();
        RemoveTrainingDataManifestField(artifactRoot, "legal_verdict");
        var outputPath = Path.Combine(_tempRoot, "reports", "invalid-training-data-artifact.json");

#pragma warning disable CS0618 // Intentional legacy input-validation coverage for the brokerless overload.
        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            BuildTrainer(),
            BuildOptions(outputPath, trainingDataArtifactRoot: artifactRoot));
#pragma warning restore CS0618

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAINING_DATA_MANIFEST_FIELD_MISSING", result.ErrorCode);
        Assert.Contains("legal_verdict", result.ErrorMessage, StringComparison.Ordinal);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000001.json")));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Theory]
    [InlineData("common_train_ready")]
    [InlineData("dpo_train_ready")]
    public void Run_RejectsStaleTrainingDataArtifactReadinessValuesBeforeTraining(string fieldName)
    {
        var artifactRoot = CreateTrainingDataArtifactRoot();
        SetTrainingDataManifestField(artifactRoot, fieldName, false);
        var outputPath = Path.Combine(_tempRoot, "reports", $"stale-{fieldName}.json");

#pragma warning disable CS0618 // Intentional legacy input-validation coverage for the brokerless overload.
        var result = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            BuildTrainer(),
            BuildOptions(outputPath, trainingDataArtifactRoot: artifactRoot));
#pragma warning restore CS0618

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAINING_DATA_MANIFEST_INVALID", result.ErrorCode);
        Assert.Contains("shared legal/common/DPO contract", result.ErrorMessage, StringComparison.Ordinal);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000001.json")));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public void Run_RequiresSafeOutputPathAndBlocksRepositoryArtifactsByDefault()
    {
#pragma warning disable CS0618 // Intentional legacy path-validation coverage for the brokerless overload.
        var requiredOutput = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            BuildTrainer(),
            BuildOptions(null!));
        var relativeOutput = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            BuildTrainer(),
            BuildOptions("relative-runner-output.json"));
        var repoOutputPath = Path.Combine(
            GetRepoRoot(),
            $"translation-dpo-runner-should-not-exist-{Guid.NewGuid():N}.json");

        var repoOutput = TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            BuildTrainer(),
            BuildOptions(repoOutputPath));
#pragma warning restore CS0618

        Assert.False(requiredOutput.IsSuccess);
        Assert.Equal("OUTPUT_PATH_REQUIRED", requiredOutput.ErrorCode);
        Assert.False(relativeOutput.IsSuccess);
        Assert.Equal("OUTPUT_PATH_MUST_BE_ABSOLUTE", relativeOutput.ErrorCode);
        Assert.False(repoOutput.IsSuccess);
        Assert.Equal("REPOSITORY_OUTPUT_BLOCKED", repoOutput.ErrorCode);
        Assert.False(File.Exists(repoOutputPath));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, "dpo_step_000001.json")));
        Assert.False(File.Exists(Path.Combine(_checkpointDir, DpoTrainer.BuildPolicyCheckpointFileName(1))));
    }

    [Fact]
    public void Run_BlocksRepositoryArtifactsWhenSeedCorpusIsOutsideRepository()
    {
        var externalSeedPath = CopySeedCorpusOutsideRepository();
        var repoRoot = GetRepoRoot();
        var repoOutputPath = Path.Combine(
            repoRoot,
            $"translation-dpo-external-seed-output-{Guid.NewGuid():N}.json");
        var repoCheckpointDirectory = Path.Combine(
            repoRoot,
            $"translation-dpo-external-seed-checkpoints-{Guid.NewGuid():N}");
        var safeOutputPath = Path.Combine(
            _tempRoot,
            "reports",
            "external-seed-safe-output.json");

#pragma warning disable CS0618 // Intentional legacy path-validation coverage for the brokerless overload.
        var repoOutput = TranslationDpoTrainEvaluateRunner.Run(
            externalSeedPath,
            BuildTrainer(),
            BuildOptions(repoOutputPath));
        var repoCheckpoint = TranslationDpoTrainEvaluateRunner.Run(
            externalSeedPath,
            BuildTrainer(),
            BuildOptions(safeOutputPath, repoCheckpointDirectory));
#pragma warning restore CS0618

        Assert.False(repoOutput.IsSuccess);
        Assert.Equal("REPOSITORY_OUTPUT_BLOCKED", repoOutput.ErrorCode);
        Assert.False(File.Exists(repoOutputPath));
        Assert.False(repoCheckpoint.IsSuccess);
        Assert.Equal("REPOSITORY_OUTPUT_BLOCKED", repoCheckpoint.ErrorCode);
        Assert.False(File.Exists(safeOutputPath));
        Assert.False(Directory.Exists(repoCheckpointDirectory));
    }

    private TranslationRepositoryOwnedDpoDatasetContext
        CreateRepositoryOwnedDpoDatasetContext(
            string scope,
            RepositoryOwnedDpoDatasetRequestIdentity? requestIdentity = null)
    {
        var repositoryRoot = CreateRepositoryOwnedFixtureRoot(scope);
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            repositoryRoot,
            "translation-domain",
            "dpo-train-evaluate",
            "v" + ComputeTextSha256(scope)[..12]);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return new TranslationRepositoryOwnedDpoDatasetContext(
            Enabled: true,
            Address: address.Value,
            RequestIdentity: requestIdentity ??
                             CreateRepositoryOwnedDpoRequestIdentity(scope));
    }

    private TranslationRepositoryOwnedDpoDatasetContext
        CreateCleanMissRepositoryOwnedDpoDatasetContext(
            string scope,
            TranslationRepositoryOwnedDpoDatasetAuthoringContext authoring)
    {
        var repositoryRoot = CreateRepositoryOwnedFixtureRoot(scope);
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            repositoryRoot,
            "translation-domain",
            "dpo-train-evaluate",
            "v" + ComputeTextSha256(scope)[..12]);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return new TranslationRepositoryOwnedDpoDatasetContext(
            Enabled: true,
            Address: address.Value,
            RequestIdentity: null,
            CleanMissAuthoringContext: authoring);
    }

    private static TranslationRepositoryOwnedDpoDatasetAuthoringContext
        CreateAuthoringContext(
            RepositoryOwnedDpoDatasetRequestIdentity identity,
            Func<OperationResult<ITrainingTeacherInferenceClient>>
                createTeacherClient,
            int maxPairsToAuthor = 50)
        => new(
            createTeacherClient,
            identity.OpenTeacher,
            identity.GenerationPolicy,
            identity.Creator,
            MaxPairsToAuthor: maxPairsToAuthor);

    private RepositoryOwnedDpoDatasetRequestIdentity
        PrepareRepositoryOwnedRequestIdentityFromRepoSeed(
            string scope,
            int maxPairsToAuthor = 50)
    {
        var seedIdentity = CreateRepositoryOwnedDpoRequestIdentity(scope);
        var authoring = CreateAuthoringContext(
            seedIdentity,
            () => OperationResult<ITrainingTeacherInferenceClient>.Fail(
                "This test helper prepares request identity only and must not create a teacher client.",
                "TEST_TEACHER_FACTORY_NOT_USED"),
            maxPairsToAuthor);
        var rows = TranslationSeedCorpusLoader
            .Load(GetRepoSeedDataPath())
            .ToTrainReadyPairs(TranslationSeedCorpusSplit.Train);
        var prepared = TranslationRepositoryOwnedDpoDatasetAuthoring
            .PrepareRequest(rows, authoring);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        return prepared.Value!.RequestIdentity;
    }

    private static string PrepareRepositoryOwnedRequestIdentitySha256(
        IReadOnlyList<TranslationTrainReadyPair> rows,
        TranslationRepositoryOwnedDpoDatasetAuthoringContext authoring)
    {
        var prepared = TranslationRepositoryOwnedDpoDatasetAuthoring
            .PrepareRequest(rows, authoring);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
        return prepared.Value!.RequestIdentity.RequestIdentitySha256;
    }

    private static TranslationTrainReadyPair CreateTeacherVisibleSourceRow(
        string scope)
        => new(
            Id: "teacher-visible-row-" + scope,
            Source: "teacher-visible-source-" + scope,
            Direction: "EN_RU",
            PairFamily: "teacher-visible-source-fields",
            Prompt: "Translate the source sentence into Russian.",
            ASide: "The demo starts at nine.",
            Chosen: "Демо начинается в девять.",
            Rejected: "Демо заканчивается в девять.",
            ValidationOutcome: "train_ready",
            ValidationErrors: []);

    private string CreateRepositoryOwnedFixtureRoot(string scope)
    {
        var root = Path.Combine(
            _tempRoot,
            "repository-owned-fixtures",
            scope + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        File.WriteAllText(
            Path.Combine(root, "LlmFromScratch.sln"),
            "repository-owned test fixture\n",
            Utf8NoBom);
        return root;
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
                    "translation-dpo-train-evaluate",
                    "v1",
                    ComputeTextSha256("generation-policy-" + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "translation-dpo-train-evaluate",
                    "v1",
                    ComputeTextSha256("creator-policy-" + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private IReadOnlyList<ContinuousLearning.DpoPair>
        CreateRepositoryOwnedDpoPairs(
            TranslationRepositoryOwnedDpoDatasetContext context,
            string scope,
            int? count = null)
    {
        var trainReadyPairs = TranslationSeedCorpusLoader
            .Load(GetRepoSeedDataPath())
            .ToTrainReadyPairs(TranslationSeedCorpusSplit.Train);
        if (count is { } requestedCount)
            trainReadyPairs = trainReadyPairs.Take(requestedCount).ToArray();

        return trainReadyPairs
            .Select((pair, index) => CreateRepositoryOwnedDpoPair(
                context,
                scope,
                pair,
                index))
            .ToArray();
    }

    private static ContinuousLearning.DpoPair CreateRepositoryOwnedDpoPair(
        TranslationRepositoryOwnedDpoDatasetContext context,
        string scope,
        TranslationTrainReadyPair pair,
        int ordinal)
    {
        var identity = context.RequestIdentity!;
        var address = context.Address!;
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
            "translation-test-dpo-" + ComputeTextSha256(
                string.Join(
                    "\n",
                    scope,
                    ordinal.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    pair.Id,
                    identity.RequestIdentitySha256)),
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };
        var commonPair = new ContinuousLearning.DpoPair(
            Prompt: $"{pair.Prompt}\nInput: {pair.ASide}",
            Chosen: pair.Chosen,
            Rejected: pair.Rejected,
            Domain: address.DomainSlug,
            CapturedAt: DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs:
                [
                    "sha256:" + ComputeTextSha256("governance-" + scope),
                    "sha256:" + ComputeTextSha256("translation-pair-" + pair.Id)
                ]),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.LocalModel)
        };

        var bound = DpoPairTrainingContentAuthority.Bind(commonPair);
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    private TranslationRepositoryOwnedModelCheckpointContext
        CreateRepositoryOwnedModelCheckpointContext(
            TranslationRepositoryOwnedDpoDatasetContext dpoContext,
            string scope,
            bool allowColdStart,
            string? coldStartReason = null)
        => new(
            Enabled: true,
            Request: CreateRepositoryOwnedModelCheckpointRequest(
                dpoContext,
                scope,
                allowColdStart,
                coldStartReason));

    private TranslationRepositoryOwnedModelCheckpointRequest
        CreateRepositoryOwnedModelCheckpointRequest(
            TranslationRepositoryOwnedDpoDatasetContext dpoContext,
            string scope,
            bool allowColdStart,
            string? coldStartReason = null,
            string? designPatternSlug = null,
            RepositoryOwnedModelRuntimeCompatibility?
                runtimeCompatibility = null)
    {
        var dpoAddress = dpoContext.Address!;
        var request = new TranslationRepositoryOwnedModelCheckpointRequest(
            Address: new TranslationRepositoryOwnedModelCheckpointAddress(
                dpoAddress.RepositoryRoot,
                dpoAddress.DomainSlug,
                designPatternSlug ?? dpoAddress.DesignPatternSlug,
                "translation-policy-" +
                ComputeTextSha256("model-" + scope)[..12],
                "v" + ComputeTextSha256("model-version-" + scope)[..12]),
            RuntimeCompatibility: runtimeCompatibility ??
                CreateRepositoryOwnedModelRuntimeCompatibility(scope),
            AllowColdStart: allowColdStart,
            ColdStartReason: coldStartReason,
            ProvenanceRefs:
            [
                "translation-dpo-test-" +
                ComputeTextSha256("model-provenance-" + scope)[..16]
            ]);
        var validation = request.Validate();
        Assert.True(validation.IsSuccess, validation.ErrorMessage);
        return request;
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

    private RepositoryOwnedModelCheckpointManifest
        SealRepositoryOwnedModelCheckpoint(
            TranslationRepositoryOwnedModelCheckpointContext modelContext,
            TranslationRepositoryOwnedDpoDatasetContext dpoContext,
            RepositoryOwnedDpoDatasetResolution sealedDataset,
            int step,
            RepositoryOwnedModelCheckpointParentIdentity? parent,
            string coldStartReason,
            ulong rootSeed)
    {
        var model = CreateSeededPolicyModel(rootSeed);
        var address = modelContext.Request!.Address.CreateStepAddress(step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var sealedCheckpoint =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address.Value!,
                model.SaveCheckpointToStream,
                CreateRepositoryOwnedModelCheckpointManifestRequest(
                    modelContext,
                    dpoContext,
                    sealedDataset,
                    parent,
                    coldStartReason,
                    step));
        Assert.True(sealedCheckpoint.IsSuccess, sealedCheckpoint.ErrorMessage);
        return sealedCheckpoint.Value!;
    }

    private void SealRepositoryOwnedRawModelCheckpoint(
        TranslationRepositoryOwnedModelCheckpointContext modelContext,
        TranslationRepositoryOwnedDpoDatasetContext dpoContext,
        RepositoryOwnedDpoDatasetResolution sealedDataset,
        int step,
        RepositoryOwnedModelCheckpointParentIdentity? parent,
        string coldStartReason)
    {
        using var rawWeights = new MemoryStream(
            Utf8NoBom.GetBytes("not a transformer checkpoint\n"));
        var address = modelContext.Request!.Address.CreateStepAddress(step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var sealedCheckpoint =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address.Value!,
                rawWeights,
                CreateRepositoryOwnedModelCheckpointManifestRequest(
                    modelContext,
                    dpoContext,
                    sealedDataset,
                    parent,
                    coldStartReason,
                    step));
        Assert.True(sealedCheckpoint.IsSuccess, sealedCheckpoint.ErrorMessage);
    }

    private static void CreateRepositoryOwnedCheckpointRootForTest(
        string checkpointRoot)
    {
        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(checkpointRoot);
            return;
        }

        // The production sealer refuses broad Unix permissions. These recovery
        // fixtures create an interrupted step by hand, so the fixture must give
        // that step the same owner-only custody as a real repository-owned
        // checkpoint root before recovery is allowed to trust it.
        Directory.CreateDirectory(
            checkpointRoot,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute);
    }

    private static string WriteRepositoryOwnedModelCheckpointPendingJournalForTest(
        TranslationRepositoryOwnedModelCheckpointContext modelContext,
        TranslationRepositoryOwnedDpoDatasetContext dpoContext,
        RepositoryOwnedDpoDatasetResolution sealedDataset,
        RepositoryOwnedModelCheckpointAddress address,
        string trainerPolicyPath,
        string trainerMetricsPath,
        string coldStartReason,
        RepositoryOwnedModelCheckpointParentIdentity? parent = null,
        int trainerStep = 1,
        bool bindTrainerArtifacts = true)
    {
        var dpoIdentity = new RepositoryOwnedDpoDatasetManifestIdentity(
            dpoContext.Address!.ManifestRelativePath,
            sealedDataset.Dataset.Manifest.ManifestSha256);
        var provenanceRefs =
            BuildRepositoryOwnedModelCheckpointProvenanceRefsForTest(
                modelContext.Request!,
                dpoIdentity,
                trainerStep,
                parentStep: parent?.Step);
        var createdAtUtc = DateTimeOffset.Parse(
            "2026-08-26T14:15:16Z",
            System.Globalization.CultureInfo.InvariantCulture);
        var policyFileInfo = bindTrainerArtifacts
            ? new FileInfo(trainerPolicyPath)
            : null;
        var metricsFileInfo = bindTrainerArtifacts
            ? new FileInfo(trainerMetricsPath)
            : null;
        var journal = new
        {
            Schema =
                "translation-repository-owned-model-checkpoint-pending-publication-v1",
            Runner = nameof(TranslationDpoTrainEvaluateRunner),
            RepositoryRelativeCheckpointDirectory =
                address.CheckpointRootRelativePath,
            RepositoryRelativeWeightsPath = address.WeightsRelativePath,
            RepositoryRelativeManifestPath = address.ManifestRelativePath,
            DomainSlug = address.DomainSlug,
            DesignPatternSlug = address.DesignPatternSlug,
            ModelIdSlug = address.ModelIdSlug,
            ModelVersion = address.ModelVersion,
            Step = address.Step,
            ParentManifestRelativePath =
                parent?.RepositoryRelativeManifestPath,
            ParentStep = parent?.Step,
            ParentWeightsSha256 = parent?.CheckpointSha256,
            ParentWeightsByteLength = parent?.CheckpointByteLength,
            ColdStartReason = parent is null ? coldStartReason : string.Empty,
            DpoDatasetManifest = dpoIdentity,
            RuntimeCompatibility = modelContext.Request!.RuntimeCompatibility,
            TrainerStep = trainerStep,
            TrainerPolicyFileName = Path.GetFileName(trainerPolicyPath),
            TrainerMetricsFileName = Path.GetFileName(trainerMetricsPath),
            TrainerPolicySha256 = bindTrainerArtifacts
                ? ComputeSha256(trainerPolicyPath)
                : null,
            TrainerPolicyByteLength = policyFileInfo?.Length,
            TrainerMetricsSha256 = bindTrainerArtifacts
                ? ComputeSha256(trainerMetricsPath)
                : null,
            TrainerMetricsByteLength = metricsFileInfo?.Length,
            CreatedAtUtc = createdAtUtc,
            ProvenanceRefs = provenanceRefs
        };
        var json = JsonSerializer.Serialize(
            journal,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = true
            }) + Environment.NewLine;
        var journalPath = Path.Combine(
            address.CheckpointRoot,
            TranslationDpoTrainEvaluateRunner
                .RepositoryOwnedModelCheckpointPendingPublicationFileName);
        File.WriteAllText(journalPath, json, Utf8NoBom);
        return journalPath;
    }

    private static IReadOnlyList<string>
        BuildRepositoryOwnedModelCheckpointProvenanceRefsForTest(
            TranslationRepositoryOwnedModelCheckpointRequest request,
            RepositoryOwnedDpoDatasetManifestIdentity dpoIdentity,
            int trainerStep,
            int? parentStep)
        => Array.AsReadOnly(request.ProvenanceRefs
            .Concat([
                "translation-dpo-train-evaluate-runner",
                "trainer-step:" +
                trainerStep.ToString(
                    System.Globalization.CultureInfo.InvariantCulture),
                "dpo-manifest:" +
                dpoIdentity.RepositoryRelativeManifestPath,
                parentStep is null
                    ? "model-start:cold"
                    : "model-start:parent-step-" +
                      parentStep.Value.ToString(
                          System.Globalization.CultureInfo.InvariantCulture)
            ])
            .Distinct(StringComparer.Ordinal)
            .ToArray());

    private static RepositoryOwnedModelCheckpointManifestRequest
        CreateRepositoryOwnedModelCheckpointManifestRequest(
            TranslationRepositoryOwnedModelCheckpointContext modelContext,
            TranslationRepositoryOwnedDpoDatasetContext dpoContext,
            RepositoryOwnedDpoDatasetResolution sealedDataset,
            RepositoryOwnedModelCheckpointParentIdentity? parent,
            string coldStartReason,
            int step)
        => new(
            ParentCheckpoint: parent,
            ColdStartReason: parent is null ? coldStartReason : string.Empty,
            DpoDatasetManifest: new RepositoryOwnedDpoDatasetManifestIdentity(
                dpoContext.Address!.ManifestRelativePath,
                sealedDataset.Dataset.Manifest.ManifestSha256),
            RuntimeCompatibility: modelContext.Request!.RuntimeCompatibility,
            CreatedAtUtc: DateTimeOffset.Parse("2026-06-02T00:00:00Z"),
            ProvenanceRefs:
            [
                "translation-dpo-model-checkpoint-test-" +
                ComputeTextSha256(
                    string.Join(
                        "\n",
                        coldStartReason,
                        step.ToString(System.Globalization.CultureInfo
                            .InvariantCulture)))[..16]
            ]);

    private static RepositoryOwnedModelCheckpointParentIdentity
        ToTestParentIdentity(RepositoryOwnedModelCheckpointManifest manifest)
        => new(
            manifest.RepositoryRelativeManifestPath,
            manifest.Step,
            manifest.WeightsSha256,
            manifest.WeightsByteLength);

    private static LlmModel CreateSeededPolicyModel(ulong rootSeed)
    {
        var policy = new LlmModel(BuildTinyConfig());
        var reference = new LlmModel(BuildTinyConfig());
        var freshLoad = new LlmModel(BuildTinyConfig());
        _ = DpoTrainingSeededLoraTopologyPreparer.ApplyFreshSeededTriple(
            policy,
            reference,
            freshLoad,
            BuildReproducibilityContext(rootSeed));
        return policy;
    }

    private static void AssertRepositoryPathIsVisibleToGit(
        string repositoryRoot,
        string repositoryRelativePath)
    {
        var startInfo = new ProcessStartInfo(
            "git",
            "check-ignore --no-index -- \"" +
            repositoryRelativePath.Replace("\"", "\\\"") + "\"")
        {
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        var stdout = process!.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.Equal(1, process.ExitCode);
        Assert.True(
            string.IsNullOrWhiteSpace(stdout),
            "Expected no git ignore match, but got stdout: " + stdout);
        Assert.True(
            string.IsNullOrWhiteSpace(stderr),
            "Expected no git ignore stderr, but got stderr: " + stderr);
    }

    private static void SetRepositoryOwnedModelCheckpointHeldFileValidationProbe(
        Action<string, string>? probe)
    {
        // Эта ручка нужна только тестам мутации: production API не должен
        // получать публичный переключатель ради проверки гонки файлов.
        var property = typeof(TranslationDpoTrainEvaluateRunner).GetProperty(
            "RepositoryOwnedModelCheckpointHeldFileValidationProbe",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(property);
        property!.SetValue(null, probe);
    }

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static bool TryReplaceFileWithBytes(string path, byte[] bytes)
    {
        var temporaryPath = Path.Combine(
            Path.GetDirectoryName(path)!,
            "." + Path.GetFileName(path) + "." +
            Guid.NewGuid().ToString("N") + ".swap-test.tmp");
        try
        {
            // Тест честно пробует подменить именно path после callback-а.
            // Если ОС держит файл закрытым от записи — это тоже безопасный
            // исход: production-код сохранил удержанный handle.
            File.WriteAllBytes(temporaryPath, bytes);
            File.Move(temporaryPath, path, overwrite: true);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        finally
        {
            try
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            catch (Exception exception)
                when (exception is IOException or UnauthorizedAccessException)
            {
            }
        }
    }

    private void CreateRequiredDirectoryLink(
        string linkPath,
        string targetPath)
    {
        if (OperatingSystem.IsWindows())
        {
            CreateRequiredWindowsDirectoryJunction(linkPath, targetPath);
        }
        else
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
        }

        _directoryLinksToRemove.Add(linkPath);
        Assert.True(
            IsObservableDirectoryLink(linkPath),
            "The test fixture must create an observable directory link, junction, or reparse point; otherwise it would not prove the fail-closed boundary.");
    }

    private static void CreateRequiredWindowsDirectoryJunction(
        string linkPath,
        string targetPath)
    {
        using var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "cmd.exe",
                ArgumentList =
                {
                    "/d",
                    "/c",
                    "mklink",
                    "/J",
                    linkPath,
                    targetPath
                },
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }) ?? throw new InvalidOperationException(
                "Could not start the Windows junction creation process.");
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
    }

    private static void RemoveDirectoryLinkIfPresent(string linkPath)
    {
        try
        {
            _ = File.GetAttributes(linkPath);
        }
        catch (FileNotFoundException)
        {
            return;
        }
        catch (DirectoryNotFoundException)
        {
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            using var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    ArgumentList =
                    {
                        "/d",
                        "/c",
                        "rmdir",
                        linkPath
                    },
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
            if (process is not null)
            {
                var exited = process.WaitForExit(10_000);
                if (!exited)
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit();
                }

                if (exited && process.ExitCode == 0)
                    return;
            }
        }

        Directory.Delete(linkPath);
    }

    private static bool IsObservableDirectoryLink(string linkPath)
    {
        try
        {
            var attributes = File.GetAttributes(linkPath);
            return (attributes & FileAttributes.ReparsePoint) != 0 ||
                new DirectoryInfo(linkPath).LinkTarget is not null;
        }
        catch (Exception exception)
            when (exception is IOException
                or NotSupportedException
                or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private TranslationDpoTrainingHandoff BuildRepositoryOwnedHandoffForValidation(
        string scope)
    {
        var context = CreateRepositoryOwnedDpoDatasetContext(scope);
        var sealedDataset = SealRepositoryOwnedDpoDataset(
            context,
            CreateRepositoryOwnedDpoPairs(context, scope, count: 1));
        var heldOutRows = TranslationSeedCorpusLoader
            .Load(GetRepoSeedDataPath())
            .HeldOutRows
            .ToArray();

        return TranslationDpoTrainingHandoffBuilder
            .BuildRepositoryOwnedDpoTrainHandoff(
                "repository-owned-validation-" + scope,
                context.Address!.ManifestRelativePath,
                sealedDataset.Dataset.Pairs,
                heldOutRows);
    }

    private static RepositoryOwnedDpoDatasetResolution
        SealRepositoryOwnedDpoDataset(
            TranslationRepositoryOwnedDpoDatasetContext context,
            IReadOnlyList<ContinuousLearning.DpoPair> pairs)
    {
        var sealedDataset = RepositoryOwnedDpoDatasetResolver
            .OpenExistingOrCreateAndSeal(
                context.Address!,
                context.RequestIdentity!,
                _ => OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>
                    .Ok(pairs));
        Assert.True(sealedDataset.IsSuccess, sealedDataset.ErrorMessage);
        return sealedDataset.Value!;
    }

    private TranslationDpoTrainEvaluateRunnerOptions BuildOptions(
        string outputPath,
        string? checkpointDirectory = null,
        string? trainingDataArtifactRoot = null)
        => new(
            OutputPath: outputPath,
            CheckpointDirectory: checkpointDirectory ?? _checkpointDir,
            TrainingDataArtifactRoot: trainingDataArtifactRoot ?? CreateTrainingDataArtifactRoot(),
            TrainStep: 1,
            HeldOutEvaluationStep: 2,
            TokenizationSettings: BuildSettings(),
            ReproducibilityContext: BuildReproducibilityContext());

    private TranslationDpoCheckpointLifecycleRunnerOptions BuildLifecycleOptions(
        string outputPath,
        string continuationCheckpointDirectory)
        => new(
            OutputPath: outputPath,
            ContinuationCheckpointDirectory: continuationCheckpointDirectory,
            FreshLoadEvaluationStep: 3,
            ContinueTrainStep: 4,
            PostContinueEvaluationStep: 5,
            TokenizationSettings: BuildSettings(),
            ReproducibilityContext: BuildReproducibilityContext());

    private OperationResult<TranslationDpoTrainEvaluateRunReport>
        RunInitialWithBaseCheckpoints(
            ulong rootSeed,
            string outputName,
            string checkpointDirectoryName,
            string policyBaseCheckpoint,
            string referenceBaseCheckpoint,
            string freshLoadBaseCheckpoint)
    {
        var outputPath = Path.Combine(_tempRoot, "reports", outputName);
        var checkpointDirectory = Path.Combine(
            _tempRoot,
            checkpointDirectoryName);
        return TranslationDpoTrainEvaluateRunner.Run(
            GetRepoSeedDataPath(),
            LoadBaseModelFactory(policyBaseCheckpoint),
            LoadBaseModelFactory(referenceBaseCheckpoint),
            LoadBaseModelFactory(freshLoadBaseCheckpoint),
            CreateAuthority(),
            BuildOptions(outputPath, checkpointDirectory) with
            {
                ReproducibilityContext =
                    BuildReproducibilityContext(rootSeed)
            });
    }

    private string SaveBaseCheckpoint(string fileName)
    {
        var path = Path.Combine(_tempRoot, "base-models", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        new LlmModel(BuildTinyConfig()).SaveCheckpoint(path);
        return path;
    }

    private static Func<LlmModel> LoadBaseModelFactory(string checkpointPath)
        => () =>
        {
            var model = new LlmModel(BuildTinyConfig());
            model.LoadCheckpoint(checkpointPath);
            return model;
        };

    private static DpoTrainingReproducibilityContext BuildReproducibilityContext(
        ulong rootSeed = 123456789UL)
        => DpoTrainingReproducibilityContext.CreateCurrent(
            rootSeed,
            new string('e', 64),
            BuildSeededLoraTopology(),
            "translation.policy",
            "translation.reference",
            "translation.fresh-load");

    private static DpoTrainingSeededLoraTopology BuildSeededLoraTopology()
        => new(2, 4f, 0f, ["q_proj", "v_proj"]);

    private DpoTrainer BuildTrainer()
        => new(
            new LlmModel(BuildTinyConfig()),
            new LlmModel(BuildTinyConfig()),
            _checkpointDir);

    private static ServerOwnedLegalTrainingTeacherAdmissionAuthority
        CreateAuthority(Func<bool>? isAdmissionLive = null)
    {
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "qwen3.5:9b",
            new string('d', 64),
            new string('c', 64),
            16,
            $"sha256:{new string('a', 64)}",
            $"sha256:{new string('b', 64)}",
            Enumerable.Range(1, 32)
                .Select(static value => (byte)value)
                .ToArray()).Value!;
        return ServerOwnedLegalTrainingTeacherAdmissionAuthority
            .IssueFromAdmittedConfiguration(
                configuration,
                isAdmissionLive ?? (() => true));
    }

    private sealed class RecordingLeaseBroker(
        IServerOwnedDpoTrainingLeaseBroker inner)
        : IServerOwnedDpoTrainingLeaseBroker
    {
        public DpoTrainingContentBatch? LastTrainingContent { get; private set; }
        public string? LastTrainingRunId { get; private set; }
        public List<string> TrainingRunIds { get; } = [];

        public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
            AcquireTranslationTrainingLease(
                DpoTrainingContentBatch trainingContent,
                string trainingRunId)
        {
            LastTrainingContent = trainingContent;
            LastTrainingRunId = trainingRunId;
            TrainingRunIds.Add(trainingRunId);
            return inner.AcquireTranslationTrainingLease(
                trainingContent,
                trainingRunId);
        }
    }

    private sealed class RecordingTranslationDpoTeacher(
        RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity,
        string? failureMode = null)
        : ITrainingTeacherInferenceClient
    {
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
            {
                return Task.FromResult(
                    issued.MapError<TrainingTeacherInferenceResult>());
            }

            var ordinal = Interlocked.Increment(ref _callCount);
            var paidReference = prompt.Split('\n')
                .Single(line => line.StartsWith(
                    "paid_reference=",
                    StringComparison.Ordinal))["paid_reference=".Length..];
            var output = failureMode switch
            {
                "paid-verbatim" => JsonSerializer.Serialize(new
                {
                    chosen = paidReference,
                    rejected = "open-qwen-rejected-" + ordinal,
                    rejected_reason = "The answer omits a required detail."
                }),
                "malformed-json" => "not-json",
                _ => JsonSerializer.Serialize(new
                {
                    chosen = "open-qwen-chosen-" + ordinal,
                    rejected = "open-qwen-rejected-" + ordinal,
                    rejected_reason = "The answer omits a required detail."
                })
            };
            var teacherIdentity = expectedIdentity.OpenTeacher;
            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                failureMode == "wrong-provenance"
                    ? "qwen-wrong:9b"
                    : teacherIdentity.ModelId,
                teacherIdentity.ModelVersion,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                "translation-authoring-test-" + ordinal,
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    teacherIdentity.ModelArtifactDigestKind,
                ModelArtifactDigest = teacherIdentity.ModelArtifactSha256
            };
            return Task.FromResult(
                OperationResult<TrainingTeacherInferenceResult>.Ok(
                    new TrainingTeacherInferenceResult(output, provenance)
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority.Authoring
                    }));
        }

        public void Dispose()
        {
            // The fake owns no transport. Keeping disposal a no-op lets the
            // test prove that a repeat hit does not create or call it again.
        }
    }

    private sealed class RevokeLeaseOnAcquireBroker(
        IServerOwnedDpoTrainingLeaseBroker inner)
        : IServerOwnedDpoTrainingLeaseBroker
    {
        public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>
            AcquireTranslationTrainingLease(
                DpoTrainingContentBatch trainingContent,
                string trainingRunId)
        {
            var result = inner.AcquireTranslationTrainingLease(
                trainingContent,
                trainingRunId);
            result.Value?.Dispose();
            return result;
        }
    }

    private static TranslationDpoTokenizationSettings BuildSettings()
        => new(VocabSize: 512, ContextLength: 64);

    private static ModelConfig BuildTinyConfig()
        => new()
        {
            VocabSize = BuildSettings().VocabSize,
            ContextLen = BuildSettings().ContextLength,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };

    private string WriteMutatedCorpus(Action<JsonObject[]> mutate)
    {
        var nodes = File.ReadAllLines(GetRepoSeedDataPath(), Utf8NoBom)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonNode.Parse(line)!.AsObject())
            .ToArray();
        mutate(nodes);

        var path = Path.Combine(_tempRoot, $"seed-corpus-{Guid.NewGuid():N}.jsonl");
        var jsonl = string.Join("\n", nodes.Select(node => node.ToJsonString(JsonOptions))) + "\n";
        File.WriteAllText(path, jsonl, Utf8NoBom);
        return path;
    }

    private string CopySeedCorpusOutsideRepository()
    {
        var directory = Path.Combine(_tempRoot, "external-seed");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, SeedFileName);
        File.Copy(GetRepoSeedDataPath(), path, overwrite: true);
        return path;
    }

    private string CreateTrainingDataArtifactRoot(string? seedCorpusPath = null)
    {
        var root = Path.Combine(_tempRoot, "training-data-artifacts", Guid.NewGuid().ToString("N"));
        var corpus = TranslationSeedCorpusLoader.Load(
            seedCorpusPath ?? GetRepoSeedDataPath());
        var export = TranslationTrainingDataArtifactStore.ExportTrainReadyPairsAsync(
                root,
                corpus.ToTrainReadyPairs(TranslationSeedCorpusSplit.Train),
                DateTimeOffset.Parse("2026-06-02T00:00:00Z"))
            .GetAwaiter()
            .GetResult();

        Assert.True(export.IsSuccess, export.ErrorMessage);
        return root;
    }

    private static void RemoveTrainingDataManifestField(string artifactRoot, string fieldName)
    {
        var manifestPath = Path.Combine(
            artifactRoot,
            TranslationTrainingDataContract.ManifestRepoPath.Replace('/', Path.DirectorySeparatorChar));
        var manifestJson = File.ReadAllText(manifestPath, Utf8NoBom);
        var node = JsonNode.Parse(manifestJson)!.AsObject();
        Assert.True(node.Remove(fieldName), $"Expected generated manifest to contain {fieldName} before negative mutation.");
        File.WriteAllText(
            manifestPath,
            node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n",
            Utf8NoBom);
    }

    private static void SetTrainingDataManifestField(string artifactRoot, string fieldName, bool value)
    {
        var manifestPath = Path.Combine(
            artifactRoot,
            TranslationTrainingDataContract.ManifestRepoPath.Replace('/', Path.DirectorySeparatorChar));
        var manifestJson = File.ReadAllText(manifestPath, Utf8NoBom);
        var node = JsonNode.Parse(manifestJson)!.AsObject();
        Assert.True(node.ContainsKey(fieldName), $"Expected generated manifest to contain {fieldName} before negative mutation.");
        node[fieldName] = value;
        File.WriteAllText(
            manifestPath,
            node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n",
            Utf8NoBom);
    }

    private static bool IsTrainRow(JsonObject row)
        => string.Equals(row["split"]!.GetValue<string>(), "train", StringComparison.Ordinal);

    private static bool IsHeldOutRow(JsonObject row)
        => string.Equals(row["split"]!.GetValue<string>(), "held_out", StringComparison.Ordinal);

    private static string GetRepoSeedDataPath()
        => Path.Combine(GetRepoRoot(), "27.TranslationDomain", "SeedData", SeedFileName);

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string ComputeTextSha256(string value)
        => Convert
            .ToHexString(SHA256.HashData(Utf8NoBom.GetBytes(value)))
            .ToLowerInvariant();

    private static string GetRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root with LlmFromScratch.sln was not found.");
    }
}
