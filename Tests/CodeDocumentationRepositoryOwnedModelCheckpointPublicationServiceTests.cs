// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using ModelServer;
using Tokenizer;
using TrainableRouter.Training;
using TransformerModel;
using TransformerModel.Config;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

// Эти focused-тесты доказывают именно сохранение модели в репозиторий: START
// дал модель, publication записал ее полные bytes в training-artifacts/models,
// а следующий START загрузил уже дочерний checkpoint. Реальное обучение и Qwen
// здесь намеренно не запускаются.
public sealed class CodeDocumentationRepositoryOwnedModelCheckpointPublicationServiceTests
{
    [Fact]
    public void Bootstrap_ExplicitStageAndEligibleSavedDpoCreateOnlyStepOneAndDeniedReload()
    {
        // All effects are confined to this synthetic repo. These are lifecycle guards, not learned-quality measurements.
        var root = CreateRepositoryRoot("code-doc-bootstrap-synthetic");
        try
        {
            var tokenizer = new BpeTokenizer();
            tokenizer.Train("synthetic bootstrap fixture", 256);
            var config = CreateModelConfig() with { VocabSize = tokenizer.VocabSize };
            var runtime = CreateCompatibility(config, "bootstrap") with { TokenizerSha256 = tokenizer.ComputeStateSha256() };
            var profile = CreateStartRequest(root, config, runtime, RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var directory = Path.Combine(root, "config", "code-documentation");
            Directory.CreateDirectory(Path.Combine(root, "config"));
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(directory);
            using (var file = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(Path.Combine(directory, "tokenizer.json"),
                FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.None)) tokenizer.Save(file);
            var configBytes = JsonSerializer.SerializeToUtf8Bytes(new
            {
                version = CodeDocumentationGenerationConfiguration.Version,
                domainSlug = profile.DomainSlug, designPatternSlug = profile.DesignPatternSlug,
                modelIdSlug = profile.ModelIdSlug, modelVersion = profile.ModelVersion,
                modelConfig = new { config.VocabSize, config.ContextLen, config.HiddenDim, config.NumLayers,
                    config.NumHeads, config.NumKvHeads, config.FfnHiddenDim, config.RopeTheta, config.RmsNormEps },
                runtimeCompatibility = runtime, settings = new CodeDocumentationStudentGenerationSettings(4, 0, 0, 1, 7, 1024)
            }, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            using (var file = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(Path.Combine(directory, "generation-v1.json"),
                FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.None)) file.Write(configBytes);
            var lesson = CreateSealedCodeDocumentationDpoIdentity(root, "bootstrap-synthetic");
            var version = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DefaultDatasetVersion;
            var arguments = new[] { CodeDocumentationBaselineBootstrapCommand.CommandArgument,
                CodeDocumentationBaselineBootstrapCommand.DatasetVersionArgument, version,
                CodeDocumentationBaselineBootstrapCommand.DatasetHashArgument, lesson.ManifestSha256 };
            var environment = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                [CodeDocumentationBaselineBootstrapCommand.EnabledEnvironmentName] = "true",
                [Program.RepositoryOwnedTrainingEnabledEnvironmentName] = "true",
                [Program.RepositoryOwnedTrainingAllowColdStartEnvironmentName] = "true",
                [Program.RepositoryOwnedTrainingColdStartReasonEnvironmentName] = "synthetic isolated bootstrap stage explicitly permitted",
                [Program.RepositoryOwnedTrainingProvenanceRefsEnvironmentName] = "test:synthetic-bootstrap-no-training",
                [CodeDocumentationGenerationConfiguration.EnabledEnvironmentName] = "true",
                [CodeDocumentationGenerationConfiguration.HashEnvironmentName] = Sha256(configBytes)
            };
            string? Read(string name) => environment.GetValueOrDefault(name);
            CodeDocumentationBaselineBootstrapResult Run(string[]? selected = null)
                => CodeDocumentationBaselineBootstrapCommand.RunSyntheticFixtureForTesting(selected ?? arguments, Read, root);
            var modelAddress = CreateModelCheckpointAddress(root, 1);
            void AssertNoModelOrJournal()
            {
                Assert.False(File.Exists(modelAddress.ManifestPath));
                Assert.False(File.Exists(modelAddress.WeightsPath));
                var models = Path.Combine(root, "training-artifacts", "models");
                Assert.True(!Directory.Exists(models) || !Directory.EnumerateFiles(models, "*", SearchOption.AllDirectories).Any());
            }

            Assert.False(CodeDocumentationBaselineBootstrapCommand.IsRequested(["--ready-file", "host-ready.json"]));
            foreach (var key in new[] { CodeDocumentationBaselineBootstrapCommand.EnabledEnvironmentName,
                Program.RepositoryOwnedTrainingEnabledEnvironmentName, Program.RepositoryOwnedTrainingAllowColdStartEnvironmentName })
            {
                environment[key] = null;
                Assert.Equal("BOOTSTRAP_STAGE_NOT_AUTHORIZED", Run().ErrorCode);
                AssertNoModelOrJournal(); environment[key] = "true";
            }
            foreach (var invalid in new[] { arguments.Concat(["--unknown"]).ToArray(),
                arguments.Concat([CodeDocumentationBaselineBootstrapCommand.CommandArgument]).ToArray(),
                new[] { CodeDocumentationBaselineBootstrapCommand.CommandArgument } })
            {
                Assert.Equal("BOOTSTRAP_ARGUMENTS_INVALID", Run(invalid).ErrorCode); AssertNoModelOrJournal();
            }
            var stale = arguments.ToArray(); stale[^1] = Hex('a');
            Assert.Equal("BOOTSTRAP_DPO_IDENTITY_MISMATCH", Run(stale).ErrorCode); AssertNoModelOrJournal();

            // Reuse the actual dataset and existing admission owner; a sealed hash alone cannot admit licensing/quarantine.
            foreach (var (suffix, governance, expected) in new[]
            {
                ("license", new TrainingDataGovernanceDecision(false, false, new[] { "sha256:" + Hex('a') }), "TRAINING_DATA_LICENSE_NOT_ACCEPTED"),
                ("quarantine", new TrainingDataGovernanceDecision(true, true, new[] { "sha256:" + Hex('a') }), "TRAINING_DATA_QUARANTINED")
            })
            {
                var invalidAddress = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(root, profile.DomainSlug,
                    profile.DesignPatternSlug, "v-" + suffix).Value!;
                var identity = CreateRepositoryOwnedDpoRequestIdentity("bootstrap-" + suffix);
                var pair = CreateRepositoryOwnedDpoPair(identity, "bootstrap-" + suffix) with { GovernanceDecision = governance };
                var sealedInvalid = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(invalidAddress, identity, [pair]);
                Assert.True(sealedInvalid.IsSuccess, sealedInvalid.ErrorMessage);
                var invalidArguments = arguments.ToArray(); invalidArguments[2] = "v-" + suffix;
                invalidArguments[4] = sealedInvalid.Value!.ManifestSha256;
                Assert.Equal(expected, Run(invalidArguments).ErrorCode); AssertNoModelOrJournal();
            }
            var dpoAddress = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(root, profile.DomainSlug, profile.DesignPatternSlug, version).Value!;
            var opened = RepositoryOwnedDpoDatasetManifestSealer.OpenManifestIdentityOnly(dpoAddress).Value!;
            var pairPath = Path.Combine(root, opened.Manifest.OrderedPairs[0].RepositoryRelativePairFilePath.Replace('/', Path.DirectorySeparatorChar));
            var savedPair = File.ReadAllBytes(pairPath);
            File.WriteAllBytes(pairPath, savedPair.Concat(new byte[] { 32 }).ToArray());
            Assert.Equal("REFUSED", Run().Status); AssertNoModelOrJournal();
            File.WriteAllBytes(pairPath, savedPair);

            using (var cancelled = new CancellationTokenSource())
            {
                cancelled.Cancel();
                var cancelledResult = CodeDocumentationBaselineBootstrapCommand.RunSyntheticFixtureForTesting(arguments, Read, root, cancelled.Token);
                Assert.Equal("REFUSED", cancelledResult.Status);
                Assert.Equal("BOOTSTRAP_CANCELLED", cancelledResult.ErrorCode);
                AssertNoModelOrJournal();
            }
            var initialOwnerRequest = profile with { ColdStartPolicy = RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize("synthetic owner boundary") };
            var beforeSealFailure = CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.PublishInitialBaseline(
                initialOwnerRequest, lesson, DateTimeOffset.UnixEpoch, []);
            Assert.False(beforeSealFailure.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.InvalidErrorCode, beforeSealFailure.ErrorCode);
            AssertNoModelOrJournal();
            var uncertainOwner = CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.PublishInitialBaseline(
                initialOwnerRequest, lesson with { ManifestSha256 = Hex('a') }, DateTimeOffset.UnixEpoch, ["test:owner-seal-boundary"]);
            Assert.True(uncertainOwner.IsSuccess, uncertainOwner.ErrorMessage);
            Assert.IsType<CodeDocumentationInitialBaselinePublicationOutcome.ReconciliationRequired>(uncertainOwner.Value);
            AssertNoModelOrJournal();
            var result = Run();
            Assert.Equal("INITIAL_BASELINE_CREATED", result.Status);
            Assert.Equal(1, result.Publication!.Step); Assert.Null(result.Publication.ParentCheckpoint);
            Assert.Equal(lesson, result.Publication.DpoDatasetManifest);
            Assert.Equal(runtime, result.Publication.RuntimeCompatibility);
            var fresh = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(profile);
            Assert.True(fresh.IsSuccess, fresh.ErrorMessage);
            Assert.Equal(result.Publication.ManifestSha256, fresh.Value!.ParentManifest!.ManifestSha256);
            Assert.Equal(RepositoryOwnedModelCheckpointManifestSealer.InitialBaselineSchema, fresh.Value.ParentManifest.Schema);
            Assert.Equal(RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate, fresh.Value.ParentManifest.DpoApplication);
            Assert.Equal(result.Publication.WeightsSha256, Sha256(SaveModelBytes(fresh.Value.Model)));
            Assert.Null(fresh.Value.ColdStartReason);
            var beforeRepeat = File.ReadAllBytes(modelAddress.ManifestPath);
            var genuineManifestText = Encoding.UTF8.GetString(beforeRepeat);
            // Проверяем настоящие canonical self hashes: отказ должен зависеть от роли/схемы, а не только от повреждённого JSON.
            foreach (var mutate in new Func<string, string>[]
            {
                text => text.Replace("\"dpoApplication\":1", "\"dpoApplication\":2", StringComparison.Ordinal),
                text => text.Replace(",\"dpoApplication\":1", "", StringComparison.Ordinal),
                text => text.Replace(RepositoryOwnedModelCheckpointManifestSealer.InitialBaselineSchema,
                    RepositoryOwnedModelCheckpointManifestSealer.CurrentSchema, StringComparison.Ordinal)
            })
            {
                var invalid = mutate(genuineManifestText);
                var signature = "\"manifestSha256\":\"" + result.Publication.ManifestSha256 + "\"";
                var unsigned = invalid.Replace(signature, "\"manifestSha256\":\"\"", StringComparison.Ordinal);
                invalid = invalid.Replace(signature, "\"manifestSha256\":\"" + Sha256(Encoding.UTF8.GetBytes(unsigned)) + "\"", StringComparison.Ordinal);
                File.WriteAllText(modelAddress.ManifestPath, invalid, new UTF8Encoding(false));
                Assert.False(RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(modelAddress).IsSuccess);
                Assert.False(CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(profile).IsSuccess);
                File.WriteAllBytes(modelAddress.ManifestPath, beforeRepeat);
            }
            var changedWithoutSeal = genuineManifestText.Replace("\"dpoApplication\":1", "\"dpoApplication\":2", StringComparison.Ordinal);
            File.WriteAllText(modelAddress.ManifestPath, changedWithoutSeal, new UTF8Encoding(false));
            Assert.False(RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(modelAddress).IsSuccess);
            File.WriteAllBytes(modelAddress.ManifestPath, beforeRepeat);
            var baselineRequest = profile with { ColdStartPolicy = RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize("synthetic existing-parent readback") };
            var alreadyPresent = CodeDocumentationRepositoryOwnedModelCheckpointPublicationService.PublishInitialBaseline(
                baselineRequest, lesson, DateTimeOffset.UnixEpoch, ["test:existing-parent-readback"]);
            Assert.True(alreadyPresent.IsSuccess, alreadyPresent.ErrorMessage);
            Assert.IsType<CodeDocumentationInitialBaselinePublicationOutcome.AlreadyPresent>(alreadyPresent.Value);
            Assert.Equal("ALREADY_PRESENT", Run().Status);
            Assert.Equal(beforeRepeat, File.ReadAllBytes(modelAddress.ManifestPath));
            Assert.False(File.Exists(CreateModelCheckpointAddress(root, 2).ManifestPath));
            Assert.False(File.Exists(CreateModelCheckpointAddress(root, 2).WeightsPath));
            Assert.Equal(savedPair, File.ReadAllBytes(pairPath));
            Assert.DoesNotContain(Directory.EnumerateFiles(Path.Combine(root, "training-artifacts", "models"), "*", SearchOption.AllDirectories),
                path => path.Contains("training-attempt", StringComparison.Ordinal) || path.Contains("journal", StringComparison.Ordinal));
        }
        finally { DeleteDirectoryIfExists(root); }
    }

    [Fact]
    public void Publish_ParentStartWritesChildCheckpointAndNextStartLoadsChild()
    {
        // Arrange: изолированный repo-shaped root дает настоящие canonical
        // пути `training-data` и `training-artifacts`, но не трогает рабочий
        // репозиторий разработчика.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-publish-parent");
        try
        {
            var config = CreateModelConfig();
            var compatibility = CreateCompatibility(config, "parent-child");
            var request = CreateStartRequest(
                repositoryRoot,
                config,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot,
                "parent-child");

            // Arrange: parent checkpoint уже существует на диске, поэтому
            // publication обязан писать продолжение step 000002, а не новый
            // холодный step 000001.
            var parentModel = new LlmModel(config);
            FillModelWithDeterministicWeights(parentModel, seed: 0.25f);
            var parentManifest = SealModelCheckpoint(
                repositoryRoot,
                parentModel,
                compatibility,
                dpoIdentity,
                step: 1,
                parent: null,
                coldReason: "first CodeDocumentation publication test parent");

            var started =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(started.IsSuccess, started.ErrorMessage);
            Assert.Equal(
                parentManifest.ManifestSha256,
                started.Value!.ParentManifest!.ManifestSha256);

            // Arrange: это маленькая deterministic мутация вместо реального
            // training loop. Она делает child bytes отличимыми от parent bytes.
            FillModelWithDeterministicWeights(started.Value.Model, seed: 4.5f);
            var expectedChildSnapshot = SnapshotBaseWeights(started.Value.Model);
            var expectedChildBytes = SaveModelBytes(started.Value.Model);
            var expectedChildSha256 = Sha256(expectedChildBytes);

            // Act: publication пишет child checkpoint через shared sealer и
            // сразу открывает его обратно.
            var published =
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .Publish(
                        request,
                        started.Value,
                        dpoIdentity,
                        DateTimeOffset.UnixEpoch.AddHours(2),
                        ["test:code-documentation-publication-parent-child"]);

            // Assert: step, lineage, paths, hashes and length come from the
            // reopened manifest, not from an optimistic in-memory status.
            Assert.True(published.IsSuccess, published.ErrorMessage);
            Assert.Equal(2, published.Value!.Step);
            Assert.NotNull(published.Value.ParentCheckpoint);
            Assert.Equal(1, published.Value.ParentCheckpoint!.Step);
            Assert.Null(published.Value.ColdStartReason);
            Assert.Equal(dpoIdentity, published.Value.DpoDatasetManifest);
            Assert.Equal(expectedChildSha256, published.Value.WeightsSha256);
            Assert.Equal(expectedChildBytes.Length, published.Value.WeightsByteLength);
            Assert.True(File.Exists(published.Value.WeightsPath));
            Assert.True(File.Exists(published.Value.ManifestPath));
            Assert.Contains(
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .ReopenedAfterPublishEvidence,
                published.Value.EvidenceRefs);

            // Assert: следующий START с denied cold-start должен загрузить
            // child step 000002 с диска; fresh empty model здесь пройти не может.
            var nextStart =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(nextStart.IsSuccess, nextStart.ErrorMessage);
            Assert.Equal(2, nextStart.Value!.ParentManifest!.Step);
            AssertBaseWeightsEqual(
                expectedChildSnapshot,
                SnapshotBaseWeights(nextStart.Value.Model));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Publish_ExplicitColdStartWritesStepOneAndDeniedNextStartLoadsIt()
    {
        // Arrange: нет parent checkpoint, поэтому единственный допустимый путь
        // публикации — точная человеческая причина холодного старта из request.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-publish-cold");
        try
        {
            var config = CreateModelConfig();
            var compatibility = CreateCompatibility(config, "cold-step-one");
            const string coldReason =
                "Luba approved the first CodeDocumentation model checkpoint for this repository-owned lineage.";
            var coldRequest = CreateStartRequest(
                repositoryRoot,
                config,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                    coldReason));
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot,
                "cold-step-one");
            var started =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(coldRequest);
            Assert.True(started.IsSuccess, started.ErrorMessage);
            Assert.Null(started.Value!.ParentManifest);
            Assert.Equal(coldReason, started.Value.ColdStartReason);

            // Arrange: deterministic weights make the first saved checkpoint a
            // real model snapshot, not a manifest-only placeholder.
            FillModelWithDeterministicWeights(started.Value.Model, seed: 7.25f);
            var expectedSnapshot = SnapshotBaseWeights(started.Value.Model);

            // Act: publication must seal step 000001 and keep the cold reason
            // as the explicit root of lineage.
            var published =
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .Publish(
                        coldRequest,
                        started.Value,
                        dpoIdentity,
                        DateTimeOffset.UnixEpoch.AddHours(4),
                        ["test:code-documentation-publication-cold"]);

            Assert.True(published.IsSuccess, published.ErrorMessage);
            Assert.Equal(1, published.Value!.Step);
            Assert.Null(published.Value.ParentCheckpoint);
            Assert.Equal(coldReason, published.Value.ColdStartReason);

            // Assert: after step 000001 exists, a denied-cold request must load
            // repository bytes. This catches regressions where training starts
            // from scratch every run.
            var deniedColdRequest = CreateStartRequest(
                repositoryRoot,
                config,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var nextStart =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(deniedColdRequest);
            Assert.True(nextStart.IsSuccess, nextStart.ErrorMessage);
            Assert.Equal(1, nextStart.Value!.ParentManifest!.Step);
            Assert.Null(nextStart.Value.ColdStartReason);
            AssertBaseWeightsEqual(
                expectedSnapshot,
                SnapshotBaseWeights(nextStart.Value.Model));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Publish_MismatchedStartModelConfigFailsBeforeAcceptedManifest()
    {
        // Arrange: START was made for one ModelConfig, but publication is asked
        // to save it under another request fingerprint. That is exactly the
        // kind of hidden lineage mismatch this service must stop before write.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-publish-mismatch");
        try
        {
            var startConfig = CreateModelConfig();
            var publishConfig = CreateModelConfig(hiddenDim: 12, heads: 3);
            var startCompatibility =
                CreateCompatibility(startConfig, "mismatch-start");
            var publishCompatibility =
                CreateCompatibility(publishConfig, "mismatch-publish");
            const string coldReason =
                "Luba approved the first CodeDocumentation mismatch fixture start.";
            var startRequest = CreateStartRequest(
                repositoryRoot,
                startConfig,
                startCompatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                    coldReason));
            var started =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(startRequest);
            Assert.True(started.IsSuccess, started.ErrorMessage);

            var mismatchedPublishRequest = CreateStartRequest(
                repositoryRoot,
                publishConfig,
                publishCompatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                    coldReason));
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot,
                "mismatch");
            var rejectedStep = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    repositoryRoot,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .CSharpDomainSlug,
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                        .DesignPatternSlug,
                    "code-documentation-llm",
                    "v1",
                    step: 1);
            Assert.True(rejectedStep.IsSuccess, rejectedStep.ErrorMessage);

            // Act: service should reject before PersistAndSeal receives bytes.
            var rejected =
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .Publish(
                        mismatchedPublishRequest,
                        started.Value!,
                        dpoIdentity,
                        DateTimeOffset.UnixEpoch.AddHours(6),
                        ["test:code-documentation-publication-mismatch"]);

            // Assert: no accepted weights/manifest are left behind for the
            // wrong model identity, so the next START cannot load poisoned data.
            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .InvalidErrorCode,
                rejected.ErrorCode);
            Assert.False(File.Exists(rejectedStep.Value!.WeightsPath));
            Assert.False(File.Exists(rejectedStep.Value.ManifestPath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Publish_MaxParentStepFailsClosedWithoutOverflowOrModelTree()
    {
        // Arrange: parent step at Int32.MaxValue is a poisoned lineage edge.
        // The service must return a stable failure instead of throwing and
        // leaving the caller unsure whether repository bytes were touched.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-publish-max-step");
        try
        {
            var config = CreateModelConfig();
            var compatibility = CreateCompatibility(config, "max-step");
            var request = CreateStartRequest(
                repositoryRoot,
                config,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot,
                "max-step");
            var start = new CodeDocumentationRepositoryOwnedModelCheckpointStart(
                new LlmModel(config),
                CreateSyntheticParentManifest(
                    request,
                    compatibility,
                    step: int.MaxValue),
                ColdStartReason: null,
                EvidenceRefs: ["test:synthetic-max-step-parent"],
                Diagnostics: []);
            var modelRoot = RepositoryOwnedTrainingArtifactLayout
                .ResolveModelArtifactRoot(repositoryRoot);
            Assert.True(modelRoot.IsSuccess, modelRoot.ErrorMessage);

            // Act: no OverflowException should escape this call.
            var rejected =
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .Publish(
                        request,
                        start,
                        dpoIdentity,
                        DateTimeOffset.UnixEpoch.AddHours(8),
                        ["test:code-documentation-publication-max-step"]);

            // Assert: the result is a normal fail-closed publication-invalid
            // answer, and no model checkpoint tree was created.
            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .InvalidErrorCode,
                rejected.ErrorCode);
            Assert.False(Directory.Exists(modelRoot.Value!.ModelArtifactRoot));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Publish_TamperedParentAfterStartFailsBeforeChildManifest()
    {
        // Arrange: START reads a sealed parent, then the parent weight bytes are
        // corrupted before publication. The child must not be accepted against
        // stale in-memory parent evidence.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-publish-tampered-parent");
        try
        {
            var config = CreateModelConfig();
            var compatibility = CreateCompatibility(config, "tampered-parent");
            var request = CreateStartRequest(
                repositoryRoot,
                config,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot,
                "tampered-parent");
            var parentModel = new LlmModel(config);
            FillModelWithDeterministicWeights(parentModel, seed: 2.25f);
            SealModelCheckpoint(
                repositoryRoot,
                parentModel,
                compatibility,
                dpoIdentity,
                step: 1,
                parent: null,
                coldReason: "first parent before tamper");
            var started =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(started.IsSuccess, started.ErrorMessage);
            FillModelWithDeterministicWeights(started.Value!.Model, seed: 3.5f);
            File.WriteAllText(
                Path.Combine(
                    repositoryRoot,
                    started.Value.ParentManifest!
                        .RepositoryRelativeWeightsPath),
                "corrupted parent bytes after START");
            var childAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 2);

            // Act: shared sealer reopens parent lineage before staging child
            // bytes, so this corruption must stop publication.
            var rejected =
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .Publish(
                        request,
                        started.Value,
                        dpoIdentity,
                        DateTimeOffset.UnixEpoch.AddHours(10),
                        ["test:code-documentation-publication-parent-tamper"]);

            // Assert: child step has no accepted manifest or staging leftovers.
            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
                rejected.ErrorCode);
            AssertNoAcceptedCheckpoint(childAddress);
            AssertNoOwnedStagingFilesRemain(childAddress);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Publish_TamperedDpoManifestAfterStartFailsBeforeStepOneManifest()
    {
        // Arrange: START is a valid explicit cold start, but the DPO manifest
        // identity is damaged before publication. Model weights may not be
        // sealed unless the referenced DPO dataset still reopens cleanly.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-publish-tampered-dpo");
        try
        {
            var config = CreateModelConfig();
            var compatibility = CreateCompatibility(config, "tampered-dpo");
            const string coldReason =
                "Luba approved cold start for the tampered DPO fixture.";
            var request = CreateStartRequest(
                repositoryRoot,
                config,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                    coldReason));
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot,
                "tampered-dpo");
            var started =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(started.IsSuccess, started.ErrorMessage);
            var dpoManifestPath = Path.Combine(
                repositoryRoot,
                dpoIdentity.RepositoryRelativeManifestPath.Replace(
                    '/',
                    Path.DirectorySeparatorChar));
            File.WriteAllText(dpoManifestPath, "{ not-json");
            var stepOne = CreateModelCheckpointAddress(repositoryRoot, step: 1);

            // Act: DPO revalidation happens inside the shared manifest sealer
            // before any accepted model checkpoint appears.
            var rejected =
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .Publish(
                        request,
                        started.Value!,
                        dpoIdentity,
                        DateTimeOffset.UnixEpoch.AddHours(12),
                        ["test:code-documentation-publication-dpo-tamper"]);

            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointManifestSealer.InvalidErrorCode,
                rejected.ErrorCode);
            AssertNoAcceptedCheckpoint(stepOne);
            AssertNoOwnedStagingFilesRemain(stepOne);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Publish_ColdStartReasonMismatchFailsBeforeStepOneManifest()
    {
        // Arrange: the model is a cold-start model, but the START result carries
        // a different reason than the request. That mismatch must not become a
        // lineage root in the repository.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-publish-cold-mismatch");
        try
        {
            var config = CreateModelConfig();
            var compatibility = CreateCompatibility(config, "cold-mismatch");
            const string requestReason =
                "Luba approved the real cold-start reason.";
            var request = CreateStartRequest(
                repositoryRoot,
                config,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                    requestReason));
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot,
                "cold-mismatch");
            var wrongStart =
                new CodeDocumentationRepositoryOwnedModelCheckpointStart(
                    new LlmModel(config),
                    ParentManifest: null,
                    ColdStartReason:
                    "A different cold-start reason must not be accepted.",
                    EvidenceRefs: ["test:wrong-cold-reason"],
                    Diagnostics: []);
            var stepOne = CreateModelCheckpointAddress(repositoryRoot, step: 1);

            var rejected =
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .Publish(
                        request,
                        wrongStart,
                        dpoIdentity,
                        DateTimeOffset.UnixEpoch.AddHours(14),
                        ["test:code-documentation-publication-cold-mismatch"]);

            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .InvalidErrorCode,
                rejected.ErrorCode);
            AssertNoAcceptedCheckpoint(stepOne);
            AssertNoOwnedStagingFilesRemain(stepOne);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Publish_DestinationConflictPreservesOrphanWeightsAndDeletesStaging()
    {
        // Arrange: a child step already contains incompatible orphan weights but
        // no manifest. Publication must refuse to bless them and must delete its
        // own staging file after the conflict.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-publish-conflict");
        try
        {
            var config = CreateModelConfig();
            var compatibility = CreateCompatibility(config, "conflict");
            var request = CreateStartRequest(
                repositoryRoot,
                config,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot,
                "conflict");
            var parentModel = new LlmModel(config);
            FillModelWithDeterministicWeights(parentModel, seed: 5.25f);
            SealModelCheckpoint(
                repositoryRoot,
                parentModel,
                compatibility,
                dpoIdentity,
                step: 1,
                parent: null,
                coldReason: "first parent before conflict");
            var started =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);
            Assert.True(started.IsSuccess, started.ErrorMessage);
            FillModelWithDeterministicWeights(started.Value!.Model, seed: 6.5f);
            var childAddress = CreateModelCheckpointAddress(
                repositoryRoot,
                step: 2);
            Directory.CreateDirectory(childAddress.CheckpointRoot);
            if (!OperatingSystem.IsWindows())
            {
                // Linux sealer accepts only owner-only checkpoint roots. The
                // test wants the later orphan-weights conflict, not an earlier
                // directory-mode refusal.
                File.SetUnixFileMode(
                    childAddress.CheckpointRoot,
                    UnixFileMode.UserRead |
                    UnixFileMode.UserWrite |
                    UnixFileMode.UserExecute);
            }

            var orphanBytes = Encoding.UTF8.GetBytes(
                "orphan bytes that must not become a sealed checkpoint");
            File.WriteAllBytes(childAddress.WeightsPath, orphanBytes);

            // Act: PersistAndSeal stages the new full checkpoint, notices the
            // incompatible orphan weights, returns conflict and removes staging.
            var rejected =
                CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                    .Publish(
                        request,
                        started.Value,
                        dpoIdentity,
                        DateTimeOffset.UnixEpoch.AddHours(16),
                        ["test:code-documentation-publication-conflict"]);

            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointManifestSealer.ConflictErrorCode,
                rejected.ErrorCode);
            Assert.Equal(orphanBytes, File.ReadAllBytes(childAddress.WeightsPath));
            Assert.False(File.Exists(childAddress.ManifestPath));
            AssertNoOwnedStagingFilesRemain(childAddress);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    private static CodeDocumentationRepositoryOwnedModelCheckpointStartRequest
        CreateStartRequest(
            string repositoryRoot,
            ModelConfig modelConfig,
            RepositoryOwnedModelRuntimeCompatibility compatibility,
            RepositoryOwnedIncrementalTrainingColdStartPolicy coldStartPolicy)
        => new(
            repositoryRoot,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                .DesignPatternSlug,
            ModelIdSlug: "code-documentation-llm",
            ModelVersion: "v1",
            modelConfig,
            compatibility,
            coldStartPolicy);

    private static RepositoryOwnedModelCheckpointAddress
        CreateModelCheckpointAddress(
            string repositoryRoot,
            int step)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                "code-documentation-llm",
                "v1",
                step);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static RepositoryOwnedModelCheckpointManifest
        CreateSyntheticParentManifest(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            RepositoryOwnedModelRuntimeCompatibility compatibility,
            int step)
    {
        var relativeRoot =
            $"training-artifacts/models/{request.DomainSlug}/{request.DesignPatternSlug}/{request.ModelIdSlug}/{request.ModelVersion}/checkpoints/{step.ToString("D6")}";
        var relativeWeights = relativeRoot + "/weights.bin";
        var relativeManifest = relativeRoot + "/manifest.json";
        return new RepositoryOwnedModelCheckpointManifest(
            RepositoryOwnedModelCheckpointManifestSealer.CurrentSchema,
            RepositoryOwnedModelCheckpointManifestSealer.CurrentContext,
            request.DomainSlug,
            request.DesignPatternSlug,
            request.ModelIdSlug,
            request.ModelVersion,
            step,
            relativeRoot,
            relativeWeights,
            relativeManifest,
            WeightsSha256: Hex('a'),
            WeightsByteLength: 16,
            ParentCheckpoint: null,
            ColdStartReason: "synthetic max-step parent fixture",
            DpoDatasetManifest: new RepositoryOwnedDpoDatasetManifestIdentity(
                $"training-data/dpo/{request.DomainSlug}/{request.DesignPatternSlug}/v1/manifest.json",
                Hex('b')),
            RuntimeCompatibility: compatibility,
            CreatedAtUtc: DateTimeOffset.UnixEpoch.ToString("O"),
            ProvenanceRefs: ["test:synthetic-parent"],
            ManifestSha256: Hex('c'));
    }

    private static RepositoryOwnedModelCheckpointManifest SealModelCheckpoint(
        string repositoryRoot,
        LlmModel model,
        RepositoryOwnedModelRuntimeCompatibility compatibility,
        RepositoryOwnedDpoDatasetManifestIdentity dpoIdentity,
        int step,
        RepositoryOwnedModelCheckpointManifest? parent,
        string coldReason)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                "code-documentation-llm",
                "v1",
                step);
        Assert.True(address.IsSuccess, address.ErrorMessage);

        // Parent identity is rebuilt from the sealed parent manifest, so tests
        // exercise the same exact lineage contract as the publication service.
        var manifestRequest = new RepositoryOwnedModelCheckpointManifestRequest(
            ParentCheckpoint: parent is null
                ? null
                : new RepositoryOwnedModelCheckpointParentIdentity(
                    parent.RepositoryRelativeManifestPath,
                    parent.Step,
                    parent.WeightsSha256,
                    parent.WeightsByteLength),
            ColdStartReason: parent is null ? coldReason : string.Empty,
            DpoDatasetManifest: dpoIdentity,
            RuntimeCompatibility: compatibility,
            CreatedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(step),
            ProvenanceRefs:
            [
                "test:code-documentation-publication-parent-fixture"
            ]);

        var sealedManifest =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address.Value!,
                model.SaveCheckpointToStream,
                manifestRequest);
        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);
        return sealedManifest.Value!;
    }

    private static void AssertNoAcceptedCheckpoint(
        RepositoryOwnedModelCheckpointAddress address)
    {
        // No manifest means the rejected step cannot be selected as a sealed
        // checkpoint by the next START, even if a directory exists.
        Assert.False(File.Exists(address.ManifestPath));
    }

    private static void AssertNoOwnedStagingFilesRemain(
        RepositoryOwnedModelCheckpointAddress address)
    {
        // The shared sealer owns `.weights.*.tmp` and `.manifest.*.tmp` staging
        // files. Rejections may leave pre-existing orphan bytes, but must not
        // leave these transient publication files behind.
        if (!Directory.Exists(address.CheckpointRoot))
            return;

        var stagingFiles = Directory
            .EnumerateFiles(
                address.CheckpointRoot,
                "*",
                SearchOption.AllDirectories)
            .Where(static path =>
            {
                var name = Path.GetFileName(path);
                return name.EndsWith(".tmp", StringComparison.Ordinal) &&
                       (name.StartsWith(".weights.", StringComparison.Ordinal) ||
                        name.StartsWith(".manifest.", StringComparison.Ordinal));
            })
            .ToArray();
        Assert.Empty(stagingFiles);
    }

    private static RepositoryOwnedDpoDatasetManifestIdentity
        CreateSealedCodeDocumentationDpoIdentity(
            string repositoryRoot,
            string scope)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DefaultDatasetVersion);
        Assert.True(address.IsSuccess, address.ErrorMessage);

        var identity = CreateRepositoryOwnedDpoRequestIdentity(scope);
        var pair = CreateRepositoryOwnedDpoPair(identity, scope);
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address.Value!,
            identity,
            [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);

        return new RepositoryOwnedDpoDatasetManifestIdentity(
            address.Value!.ManifestRelativePath,
            manifest.Value!.ManifestSha256);
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CreateRepositoryOwnedDpoRequestIdentity(string scope)
    {
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [Sha256Hex("paid CodeDocumentation reference " + scope)],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen-code-doc-test:9b",
                    Hex('c'),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    Hex('d')),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "code-documentation-publication",
                    "v1",
                    Sha256Hex("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "code-documentation-publication-tests",
                    "v1",
                    Sha256Hex("creator policy " + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private static ContinuousLearning.DpoPair CreateRepositoryOwnedDpoPair(
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
            "repository-owned-publication-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

        var bound = DpoPairTrainingContentAuthority.Bind(
            new ContinuousLearning.DpoPair(
                "Document a C# service method for " + scope,
                "Explain purpose, assumptions, data flow, side effects and caller evidence in human language.",
                "Say that it documents code. [WRONG: missing evidence and side effects.]",
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                DateTimeOffset.UnixEpoch)
            {
                TeacherProvenance = passport,
                GovernanceDecision = new TrainingDataGovernanceDecision(
                    licenseAccepted: true,
                    isQuarantined: false,
                    evidenceRefs:
                    [
                        "sha256:" + Sha256Hex("governance " + scope)
                    ]),
                ChosenAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.OpenTeacher,
                    passport),
                RejectedAuthorship = new DpoPairSideAuthorship(
                    DpoPairSideAuthorKind.LocalModel)
            });
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    private static ModelConfig CreateModelConfig(
        int hiddenDim = 8,
        int heads = 2)
        => new()
        {
            VocabSize = 16,
            ContextLen = 8,
            HiddenDim = hiddenDim,
            NumLayers = 1,
            NumHeads = heads,
            NumKvHeads = 1,
            FfnHiddenDim = hiddenDim * 2,
            RopeTheta = 10_000f,
            RmsNormEps = 1e-5f
        };

    private static RepositoryOwnedModelRuntimeCompatibility
        CreateCompatibility(ModelConfig config, string keySuffix)
        => new(
            ConfigSha256: TrainingSourceProvenance.Identify(config).Sha256,
            TopologySha256: Hex('b'),
            TokenizerSha256: Hex('c'),
            TrainerSha256: Hex('e'),
            SchemaSha256: Hex('f'),
            CompatibilityKey: "code-documentation-" + keySuffix);

    private static void FillModelWithDeterministicWeights(
        LlmModel model,
        float seed)
    {
        var parameterIndex = 0;
        foreach (var parameter in model.BaseParameters)
        {
            var data = parameter.Value.Data;
            for (var valueIndex = 0; valueIndex < data.Length; valueIndex++)
            {
                // Every scalar becomes a readable unique value; if the next
                // START loads default weights, equality with this snapshot fails.
                data[valueIndex] = seed + parameterIndex + (valueIndex * 0.125f);
            }

            parameterIndex++;
        }
    }

    private static byte[] SaveModelBytes(LlmModel model)
    {
        using var stream = new MemoryStream();
        model.SaveCheckpointToStream(stream);
        return stream.ToArray();
    }

    private static float[][] SnapshotBaseWeights(LlmModel model)
        => model.BaseParameters
            .Select(static parameter => parameter.Value.Data.ToArray())
            .ToArray();

    private static void AssertBaseWeightsEqual(
        IReadOnlyList<float[]> expected,
        IReadOnlyList<float[]> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var parameterIndex = 0;
             parameterIndex < expected.Count;
             parameterIndex++)
        {
            Assert.Equal(expected[parameterIndex], actual[parameterIndex]);
        }
    }

    private static string CreateRepositoryRoot(string scope)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            scope + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(
            Path.Combine(root, "LlmFromScratch.sln"),
            "fixture solution sentinel");
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        return root;
    }

    private static string Hex(char value) => new(value, 64);

    private static string Sha256Hex(string value)
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static void DeleteDirectoryIfExists(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Cleanup is deliberately best-effort and limited to this test's
            // unique temp repository root; it must not hide assertion failures.
        }
    }
}
