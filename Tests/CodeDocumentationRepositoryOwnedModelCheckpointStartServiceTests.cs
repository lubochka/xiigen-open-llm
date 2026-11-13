// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using ContinuousLearning;
using TrainableRouter.Training;
using TransformerModel;
using TransformerModel.Config;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

// These tests prove the START boundary as a product contract, not just as
// construction smoke: saved weights must come back exactly, empty lineage needs
// a human reason, and unsafe saved state must fail instead of silently starting
// from a blank model.
public sealed class CodeDocumentationRepositoryOwnedModelCheckpointStartServiceTests
{
    [Fact]
    public void PrepareStart_LoadsExactSavedLlmModelStateFromRepositoryOwnedCheckpoint()
    {
        // Arrange: create an isolated repository-shaped root so the shared
        // repository-owned layout validates sentinels and paths without touching
        // the real project tree.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-start-loads-exact-state");
        try
        {
            // Arrange: bind START to one exact ModelConfig and one matching
            // runtime compatibility fingerprint, catching drift between config
            // identity and manifest compatibility.
            var modelConfig = CreateModelConfig();
            var compatibility = CreateCompatibility(modelConfig, "exact-load");
            var request = CreateStartRequest(
                repositoryRoot,
                modelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);

            // Arrange: write deterministic non-default weights so a bug that
            // returns a newly initialized model cannot pass by accident.
            var parentModel = new LlmModel(modelConfig);
            FillModelWithDeterministicWeights(parentModel, seed: 0.25f);
            var parentSnapshot = SnapshotBaseWeights(parentModel);

            // Arrange: model checkpoint manifests must reference a sealed
            // Direct Preference Optimization (DPO) lineage, so the fixture
            // creates the smallest governed dataset identity.
            var dpoIdentity = CreateSealedCodeDocumentationDpoIdentity(
                repositoryRoot);
            var manifest = SealModelCheckpoint(
                repositoryRoot,
                parentModel,
                compatibility,
                dpoIdentity,
                step: 1,
                parent: null);
            Assert.Equal(1, manifest.Step);

            // Act: cold start is denied here, so any success must come from
            // repository-owned saved weights and not from an empty fresh model.
            var started =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);

            // Assert: parent manifest present and cold-start reason absent prove
            // the service followed the repository-load branch.
            Assert.True(started.IsSuccess, started.ErrorMessage);
            Assert.NotNull(started.Value!.ParentManifest);
            Assert.Null(started.Value.ColdStartReason);
            Assert.Equal(
                manifest.ManifestSha256,
                started.Value.ParentManifest!.ManifestSha256);

            // Assert: evidence refs prove repository discovery, full checkpoint
            // validation, and LlmModel.LoadCheckpoint import all ran.
            Assert.Contains(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .LoadedFromRepositoryEvidence,
                started.Value.EvidenceRefs);
            Assert.Contains(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .FullCheckpointValidatedEvidence,
                started.Value.EvidenceRefs);
            Assert.Contains(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .LlmModelLoadCheckpointCalledEvidence,
                started.Value.EvidenceRefs);

            // Assert: exact parameter equality catches skipped imports, tensor
            // ordering mistakes, and accidental default-weight success.
            AssertBaseWeightsEqual(
                parentSnapshot,
                SnapshotBaseWeights(started.Value.Model));
        }
        finally
        {
            // Cleanup: remove only this test's isolated fixture root; no real
            // repository training artifact is created or retained.
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void LoadTrainingAttemptCheckpoint_ReopensSavedLessonWeightsFromDisk()
    {
        // Arrange: this fixture represents an interrupted lesson. The trainer
        // already wrote the policy checkpoint into the repository-owned attempt
        // folder, but the quality decision has not finished yet.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-start-reopens-attempt");
        try
        {
            var modelConfig = CreateModelConfig();
            var request = CreateStartRequest(
                repositoryRoot,
                modelConfig,
                CreateCompatibility(modelConfig, "attempt-reopen"),
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var attempt = CreateTrainingAttemptAddress(
                repositoryRoot,
                "code-doc-model-start:reopen-saved-attempt");
            Directory.CreateDirectory(attempt.AttemptRoot);

            // Arrange: save non-default lesson weights as a real full LlmModel
            // checkpoint. A loader that secretly uses a fresh model or the
            // previous in-memory object cannot reproduce this snapshot.
            var lessonModel = new LlmModel(modelConfig);
            FillModelWithDeterministicWeights(lessonModel, seed: 2.75f);
            var expectedWeights = SnapshotBaseWeights(lessonModel);
            var checkpointPath = Path.Combine(
                attempt.AttemptRoot,
                DPO.DpoTrainer.BuildPolicyCheckpointFileName(1));
            using (var stream = File.Create(checkpointPath))
            {
                lessonModel.SaveCheckpointToStream(stream);
            }

            // Act: reopen the checkpoint through the production boundary used
            // before quality evaluation and publication resume.
            var loaded =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .LoadTrainingAttemptCheckpoint(
                        request,
                        attempt,
                        dpoStepCount: 1);

            // Assert: the returned model comes from the saved attempt file and
            // carries evidence that disk reopening and hash verification ran.
            Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
            Assert.NotNull(loaded.Value);
            Assert.Equal(checkpointPath, loaded.Value!.WeightsPath);
            Assert.Equal(
                ComputeFileSha256(checkpointPath),
                loaded.Value.WeightsSha256);
            Assert.Equal(new FileInfo(checkpointPath).Length,
                loaded.Value.WeightsByteLength);
            Assert.EndsWith(
                "/attempts/" + attempt.TrainingRunIdSha256 + "/" +
                DPO.DpoTrainer.BuildPolicyCheckpointFileName(1),
                loaded.Value.RepositoryRelativeWeightsPath,
                StringComparison.Ordinal);
            Assert.Contains(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .TrainingAttemptCheckpointReopenedEvidence,
                loaded.Value.EvidenceRefs);
            Assert.Contains(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .TrainingAttemptCheckpointHashVerifiedEvidence,
                loaded.Value.EvidenceRefs);
            AssertBaseWeightsEqual(
                expectedWeights,
                SnapshotBaseWeights(loaded.Value.Model));
        }
        finally
        {
            // Cleanup: remove only this isolated fixture repository. The test
            // proves production layout behavior without leaving training files
            // on the developer drive.
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void LoadTrainingAttemptCheckpoint_FailsClosedWhenLessonWeightsAreMissing()
    {
        // Arrange: an existing attempt address with no policy checkpoint should
        // not quietly create a fresh model or continue to quality evaluation.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-start-missing-attempt");
        try
        {
            var modelConfig = CreateModelConfig();
            var request = CreateStartRequest(
                repositoryRoot,
                modelConfig,
                CreateCompatibility(modelConfig, "attempt-missing"),
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var attempt = CreateTrainingAttemptAddress(
                repositoryRoot,
                "code-doc-model-start:missing-saved-attempt");
            Directory.CreateDirectory(attempt.AttemptRoot);

            // Act: no saved lesson weights exist, so the resume path must stop.
            var loaded =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .LoadTrainingAttemptCheckpoint(
                        request,
                        attempt,
                        dpoStepCount: 1);

            // Assert: failure here is the product guarantee that an interrupted
            // lesson is not replaced by a fake fresh model.
            Assert.False(loaded.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .TrainingAttemptCheckpointInvalidErrorCode,
                loaded.ErrorCode);
        }
        finally
        {
            // Cleanup: remove only the isolated missing-checkpoint fixture.
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void PrepareStart_AllowsCleanColdStartOnlyWithExplicitHumanReason()
    {
        // Arrange: create a valid but empty model tree target; the absence is
        // clean because no checkpoint step has been sealed for this identity.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-start-cold-start");
        try
        {
            // Arrange: the human reason is the auditable root of the first model
            // lineage and guards against quiet accidental cold starts.
            var modelConfig = CreateModelConfig();
            var compatibility = CreateCompatibility(modelConfig, "cold-start");
            const string reason =
                "Luba approved the first CodeDocumentation LlmModel checkpoint lineage for this empty repository model tree.";
            var request = CreateStartRequest(
                repositoryRoot,
                modelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize(
                    reason));

            // Act: with no checkpoint present, this should succeed only through
            // the explicit cold-start authorization path.
            var started =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);

            // Assert: no parent manifest plus the exact reason proves this is a
            // declared lineage root rather than saved-model continuation.
            Assert.True(started.IsSuccess, started.ErrorMessage);
            Assert.NotNull(started.Value!.Model);
            Assert.Null(started.Value.ParentManifest);
            Assert.Equal(reason, started.Value.ColdStartReason);

            // Assert: downstream evidence must label the start as cold-started
            // so it cannot be overreported as loaded from repository weights.
            Assert.Contains(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .ColdStartAuthorizedEvidence,
                started.Value.EvidenceRefs);
        }
        finally
        {
            // Cleanup: remove the isolated fixture root after the cold-start
            // branch has been proven.
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void PrepareStart_DeniesCleanColdStartWhenHumanReasonIsMissing()
    {
        // Arrange: use a valid empty target but deny cold start, proving that a
        // clean miss alone is not permission to return a blank model.
        var repositoryRoot = CreateRepositoryRoot(
            "code-doc-model-start-denies-cold-start");
        try
        {
            // Arrange: compatibility is valid, so the expected failure is the
            // missing human reason rather than malformed request identity.
            var modelConfig = CreateModelConfig();
            var compatibility = CreateCompatibility(modelConfig, "denied");
            var request = CreateStartRequest(
                repositoryRoot,
                modelConfig,
                compatibility,
                RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);

            // Act: START sees clean absence and must reject the empty-model path.
            var started =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(request);

            // Assert: this stable error code catches regressions that silently
            // convert missing repository weights into a fresh model.
            Assert.False(started.IsSuccess);
            Assert.Equal(
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .ColdStartNotAuthorizedErrorCode,
                started.ErrorCode);
        }
        finally
        {
            // Cleanup: remove only the denied-start fixture root.
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void PrepareStart_FailsClosedForIncompatibleOrCorruptCheckpointEvenWhenColdStartIsAuthorized()
    {
        // Arrange: use separate fixture repositories so incompatible-manifest
        // behavior and corrupt-payload behavior cannot mask each other.
        var incompatibleRepositoryRoot = CreateRepositoryRoot(
            "code-doc-model-start-incompatible");
        var corruptRepositoryRoot = CreateRepositoryRoot(
            "code-doc-model-start-corrupt");
        try
        {
            // Arrange: one runtime compatibility matches sealed bytes; the
            // second asks for a different topology/key and should be rejected by
            // loader compatibility checks before this service can cold-start.
            var modelConfig = CreateModelConfig();
            var compatibleRuntime =
                CreateCompatibility(modelConfig, "compatible");
            var incompatibleRuntime = compatibleRuntime with
            {
                TopologySha256 = Hex('9'),
                CompatibilityKey = "code-documentation-incompatible"
            };
            var parentModel = new LlmModel(modelConfig);
            FillModelWithDeterministicWeights(parentModel, seed: 1.25f);

            // Arrange: seal valid bytes under the compatible identity, then ask
            // START for incompatible runtime identity to prove conflict is not a
            // clean missing lineage.
            SealModelCheckpoint(
                incompatibleRepositoryRoot,
                parentModel,
                compatibleRuntime,
                CreateSealedCodeDocumentationDpoIdentity(
                    incompatibleRepositoryRoot),
                step: 1,
                parent: null);

            // Act: a human cold-start reason must not override incompatible
            // sealed repository state.
            var incompatibleStart =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(CreateStartRequest(
                        incompatibleRepositoryRoot,
                        modelConfig,
                        incompatibleRuntime,
                        RepositoryOwnedIncrementalTrainingColdStartPolicy
                            .Authorize("Human reason must not override incompatible sealed bytes.")));

            // Assert: preserving the loader's incompatible code tells callers to
            // repair compatibility instead of adding a cold-start reason.
            Assert.False(incompatibleStart.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointLoader
                    .IncompatibleCandidateErrorCode,
                incompatibleStart.ErrorCode);

            // Arrange: seal another valid checkpoint and then tamper only the
            // weight bytes, simulating disk corruption after manifest seal.
            var corruptManifest = SealModelCheckpoint(
                corruptRepositoryRoot,
                parentModel,
                compatibleRuntime,
                CreateSealedCodeDocumentationDpoIdentity(corruptRepositoryRoot),
                step: 1,
                parent: null);
            File.WriteAllText(
                Path.Combine(
                    corruptRepositoryRoot,
                    corruptManifest.RepositoryRelativeWeightsPath),
                "not a sealed full LlmModel checkpoint anymore");

            // Act: the shared loader should detect the hash/manifest mismatch
            // before this service validates or imports the payload.
            var corruptStart =
                CodeDocumentationRepositoryOwnedModelCheckpointStartService
                    .PrepareStart(CreateStartRequest(
                        corruptRepositoryRoot,
                        modelConfig,
                        compatibleRuntime,
                        RepositoryOwnedIncrementalTrainingColdStartPolicy
                            .Authorize("Human reason must not override corrupt sealed bytes.")));

            // Assert: corrupt repository-owned state fails closed even when a
            // cold-start reason is available on the request.
            Assert.False(corruptStart.IsSuccess);
            Assert.Equal(
                RepositoryOwnedModelCheckpointLoader.CorruptCandidateErrorCode,
                corruptStart.ErrorCode);
        }
        finally
        {
            // Cleanup: remove both isolated fixture roots; the real repository's
            // training-artifacts directory is not touched by this test.
            DeleteDirectoryIfExists(incompatibleRepositoryRoot);
            DeleteDirectoryIfExists(corruptRepositoryRoot);
        }
    }

    // Builds the public START request used by these tests. Keeping identity
    // fields centralized makes domain, model id, version, and compatibility
    // drift visible instead of hiding it across several test bodies.
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

    // Seals full LlmModel checkpoint bytes into the repository-owned model tree.
    // The helper uses the same manifest sealer that production START reads, so
    // tests cover real saved-to-loaded compatibility rather than mock bytes.
    private static RepositoryOwnedModelCheckpointManifest SealModelCheckpoint(
        string repositoryRoot,
        LlmModel model,
        RepositoryOwnedModelRuntimeCompatibility compatibility,
        RepositoryOwnedDpoDatasetManifestIdentity dpoIdentity,
        int step,
        RepositoryOwnedModelCheckpointManifest? parent)
    {
        // The checkpoint address must match the START request identity exactly;
        // otherwise the loader would correctly report a clean miss.
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

        // The manifest records either the root cold-start reason or exact parent
        // checkpoint identity, which catches lineage regressions in future
        // continuation slices.
        var manifestRequest =
            new RepositoryOwnedModelCheckpointManifestRequest(
                ParentCheckpoint: parent is null
                    ? null
                    : new RepositoryOwnedModelCheckpointParentIdentity(
                        parent.RepositoryRelativeManifestPath,
                        parent.Step,
                        parent.WeightsSha256,
                        parent.WeightsByteLength),
                ColdStartReason: parent is null
                    ? "first CodeDocumentation repository-owned checkpoint"
                    : string.Empty,
                DpoDatasetManifest: dpoIdentity,
                RuntimeCompatibility: compatibility,
                CreatedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(step),
                ProvenanceRefs:
                [
                    "test:code-documentation-repository-owned-model-start"
                ]);

        // PersistAndSeal writes a full model checkpoint plus a sibling manifest;
        // SaveCheckpointToStream ensures these bytes are full LlmModel weights,
        // not adapter-only or placeholder content.
        var sealedManifest =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address.Value!,
                model.SaveCheckpointToStream,
                manifestRequest);
        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorMessage);
        return sealedManifest.Value!;
    }

    // Creates the minimal sealed Direct Preference Optimization (DPO) dataset
    // identity that a repository-owned model checkpoint manifest must reference.
    // The test does not train; it only satisfies the manifest lineage contract.
    private static RepositoryOwnedDpoDatasetManifestIdentity
        CreateSealedCodeDocumentationDpoIdentity(string repositoryRoot)
    {
        // The dataset address shares the same CodeDocumentation domain and
        // design-pattern constants as the model checkpoint identity.
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

        // The request identity binds reference hashes, teacher identity,
        // generation policy, and creator policy without calling a real teacher.
        var identity = CreateRepositoryOwnedDpoRequestIdentity(
            "code-documentation-model-start");

        // The sealed pair is intentionally tiny but fully governed so the model
        // manifest's DPO pointer can be verified by shared sealer/loader code.
        var pair = CreateRepositoryOwnedDpoPair(
            identity,
            "code-documentation-model-start");
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address.Value!,
            identity,
            [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);

        return new RepositoryOwnedDpoDatasetManifestIdentity(
            address.Value!.ManifestRelativePath,
            manifest.Value!.ManifestSha256);
    }

    // Builds deterministic repository-owned DPO request identity metadata. Fixed
    // text-derived hashes keep the fixture reproducible while still exercising
    // production Secure Hash Algorithm 256-bit (SHA-256) validation.
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
                    "code-documentation-model-start",
                    "v1",
                    Sha256Hex("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "code-documentation-model-start-tests",
                    "v1",
                    Sha256Hex("creator policy " + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    // Builds one governed DPO pair with teacher-authored chosen text and
    // local-model rejected text. This catches manifest validation regressions
    // around authorship, license, and governance metadata.
    private static ContinuousLearning.DpoPair CreateRepositoryOwnedDpoPair(
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        string scope)
    {
        // The teacher passport mirrors an admitted local Qwen-style open-weight
        // teacher but remains fixture metadata only; no external inference runs.
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
            "repository-owned-checkpoint-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

        // Binding through content authority proves the pair is governed before
        // it is sealed into repository-owned DPO fixture data.
        var bound = DpoPairTrainingContentAuthority.Bind(new ContinuousLearning.DpoPair(
            "Document a C# service method for " + scope,
            "Explain the method purpose, runtime assumptions, dependencies and side effects with bottom-up caller/callee evidence.",
            "Summarize the code vaguely. [WRONG: missing caller/callee evidence and runtime assumptions.]",
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs: ["sha256:" + Sha256Hex("governance " + scope)]),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.LocalModel)
        });
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    // Tiny but valid Transformer configuration keeps the focused tests fast
    // while still producing a real full LlmModel checkpoint schema.
    private static ModelConfig CreateModelConfig()
        => new()
        {
            VocabSize = 16,
            ContextLen = 8,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 2,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
            RopeTheta = 10_000f,
            RmsNormEps = 1e-5f
        };

    // Runtime compatibility is tied to the exact ModelConfig hash and uses
    // deterministic placeholder hashes for other runtime surfaces because this
    // slice tests START selection, not tokenizer or trainer content.
    private static RepositoryOwnedModelRuntimeCompatibility
        CreateCompatibility(ModelConfig config, string keySuffix)
        => new(
            ConfigSha256: TrainingSourceProvenance.Identify(config).Sha256,
            TopologySha256: Hex('b'),
            TokenizerSha256: Hex('c'),
            TrainerSha256: Hex('e'),
            SchemaSha256: Hex('f'),
            CompatibilityKey: "code-documentation-" + keySuffix);

    // Writes deterministic non-default values into every base parameter. This
    // creates a strong oracle for exact state restoration after LoadCheckpoint.
    private static void FillModelWithDeterministicWeights(
        LlmModel model,
        float seed)
    {
        // Each parameter gets a distinct offset so swapped parameter order,
        // skipped parameters, and partial copy bugs are visible.
        var parameterIndex = 0;
        foreach (var parameter in model.BaseParameters)
        {
            // Each scalar inside the parameter gets a distinct fractional offset
            // so tensor element order is tested too.
            var data = parameter.Value.Data;
            for (var valueIndex = 0; valueIndex < data.Length; valueIndex++)
            {
                // Human-readable fixture: each slot gets a unique exact float,
                // so a fresh-load bug cannot hide behind default initialization.
                data[valueIndex] = seed + parameterIndex + (valueIndex * 0.125f);
            }

            parameterIndex++;
        }
    }

    // Captures only base weights because this START slice loads full base model
    // checkpoints and does not attach or test adapter checkpoint state.
    private static float[][] SnapshotBaseWeights(LlmModel model)
        => model.BaseParameters
            .Select(static parameter => parameter.Value.Data.ToArray())
            .ToArray();

    // Compares every parameter tensor exactly. The expected values come from
    // the saved parent snapshot, not from production output under test.
    private static void AssertBaseWeightsEqual(
        IReadOnlyList<float[]> expected,
        IReadOnlyList<float[]> actual)
    {
        // Count mismatch means the checkpoint was loaded into the wrong model
        // shape or failed to restore the full base parameter set.
        Assert.Equal(expected.Count, actual.Count);
        for (var parameterIndex = 0;
             parameterIndex < expected.Count;
             parameterIndex++)
        {
            // Per-parameter equality catches byte order, tensor order, and
            // missing LoadCheckpoint regressions.
            Assert.Equal(expected[parameterIndex], actual[parameterIndex]);
        }
    }

    // Creates the smallest repository-shaped root accepted by the shared
    // repository-owned layout. It is a temp fixture, not a real worktree,
    // checkpoint artifact, or production training-data directory.
    private static string CreateRepositoryRoot(string scope)
    {
        // The unique suffix prevents cross-test reuse of sealed manifests.
        var root = Path.Combine(
            Path.GetTempPath(),
            scope + "-" + Guid.NewGuid().ToString("N"));

        // These sentinels are what RepositoryOwnedTrainingArtifactLayout uses to
        // recognize a valid llm_mvp_core repository without shelling out to Git.
        Directory.CreateDirectory(root);
        File.WriteAllText(
            Path.Combine(root, "LlmFromScratch.sln"),
            "fixture solution sentinel");
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        return root;
    }

    private static RepositoryOwnedModelTrainingAttemptAddress
        CreateTrainingAttemptAddress(
            string repositoryRoot,
            string trainingRunId)
    {
        var attempt = RepositoryOwnedTrainingArtifactLayout
            .CreateModelTrainingAttemptAddress(
                repositoryRoot,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug,
                "code-documentation-llm",
                "v1",
                trainingRunId);
        Assert.True(attempt.IsSuccess, attempt.ErrorMessage);
        return attempt.Value!;
    }

    private static string ComputeFileSha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))
            .ToLowerInvariant();

    // Produces canonical lower-case hexadecimal strings for manifest fields
    // where the actual hash content is not the tested behavior.
    private static string Hex(char value) => new(value, 64);

    // Computes deterministic fixture SHA-256 values from stable text so
    // identities remain reproducible and pass production hash validation.
    private static string Sha256Hex(string value)
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    // Best-effort cleanup is limited to the unique temp repository root created
    // by this test file. Cleanup failures are swallowed so they do not hide the
    // product assertion that already ran.
    private static void DeleteDirectoryIfExists(string path)
    {
        try
        {
            // Removing the temp root prevents sealed fixture checkpoints and DPO
            // rows from becoming persistent artifacts on the developer machine.
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Test cleanup only: repository-owned production code under test
            // already wrote only into this per-test temporary repository root.
        }
    }
}
