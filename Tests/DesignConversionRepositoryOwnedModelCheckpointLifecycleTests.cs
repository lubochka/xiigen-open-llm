// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

public sealed class DesignConversionRepositoryOwnedModelCheckpointLifecycleTests
{
    [Fact]
    public async Task LoadTrainAndPublishNext_ContinuesDesignConversionFromRepositoryOwnedCheckpoint()
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-repository-model-lifecycle-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var target = new DesignConversionRepositoryOwnedModelCheckpointRequest(
                repositoryRoot,
                DomainSlug: "figma-html-css",
                DesignPatternSlug: "design-to-code",
                ModelIdSlug: "qwen3-5-9b",
                ModelVersion: "v0001",
                RuntimeCompatibility: CreateCompatibility("design-html-css"),
                AllowColdStart: true,
                ColdStartReason:
                "first accepted DesignConversion repository checkpoint",
                ProvenanceRefs: ["test:design-conversion-lifecycle"]);
            var dpoIdentity =
                CreateSealedDesignConversionDpoDatasetIdentity(repositoryRoot);

            var parentStart =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PrepareStartAsync(
                        new DesignConversionRepositoryOwnedModelCheckpointContext(
                            true,
                            target));
            Assert.True(parentStart.IsSuccess, parentStart.ErrorMessage);
            Assert.Null(parentStart.Value!.ParentManifest);
            Assert.Equal(target.ColdStartReason, parentStart.Value.ColdStartReason);

            var parentMachine = parentStart.Value.Machine;
            var parentTraining = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(
                    parentMachine,
                    CreateBatch("parent-batch", "parent"));
            Assert.True(parentTraining.IsSuccess, parentTraining.ErrorMessage);

            var parentPublished =
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PublishNext(
                        new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                            target,
                            parentStart.Value,
                            parentMachine,
                            dpoIdentity,
                            DateTimeOffset.UnixEpoch,
                            ["test:parent-design-conversion-dpo"],
                            ColdStartReason: target.ColdStartReason));
            Assert.True(parentPublished.IsSuccess, parentPublished.ErrorMessage);
            Assert.Equal(1, parentPublished.Value!.Manifest.Step);
            Assert.Equal(
                "training-artifacts/models/figma-html-css/design-to-code/qwen3-5-9b/v0001/checkpoints/000001/weights.bin",
                parentPublished.Value.Manifest.RepositoryRelativeWeightsPath);
            Assert.Contains(
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .ColdStartPublishedEvidence,
                parentPublished.Value.EvidenceRefs);

            var loadedParent =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .LoadLatestCompatibleAsync(target);
            Assert.True(loadedParent.IsSuccess, loadedParent.ErrorMessage);
            Assert.Equal(1, loadedParent.Value!.Manifest.Step);
            Assert.Equal(
                parentPublished.Value.Manifest.ManifestSha256,
                loadedParent.Value.Manifest.ManifestSha256);
            Assert.Contains(
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .LoadedFromRepositoryEvidence,
                loadedParent.Value.EvidenceRefs);
            Assert.Single(
                loadedParent.Value.Machine.ExportCheckpoint()
                    .Value!
                    .TrainingHistory);

            var childStart =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PrepareStartAsync(
                        new DesignConversionRepositoryOwnedModelCheckpointContext(
                            true,
                            target));
            Assert.True(childStart.IsSuccess, childStart.ErrorMessage);
            Assert.Equal(
                loadedParent.Value.Manifest.RepositoryRelativeManifestPath,
                childStart.Value!.ParentManifest!.RepositoryRelativeManifestPath);

            var continuedTraining = await DesignConversionLegalTeacherCapabilityTestFactory
                .TrainMachineStrictAsync(
                    childStart.Value.Machine,
                    CreateBatch("child-batch", "child"));
            Assert.True(
                continuedTraining.IsSuccess,
                continuedTraining.ErrorMessage);

            var childPublished =
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PublishNext(
                        new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                            target,
                            childStart.Value,
                            childStart.Value.Machine,
                            dpoIdentity,
                            DateTimeOffset.UnixEpoch.AddMinutes(1),
                            ["test:child-design-conversion-dpo"],
                            ParentCheckpoint: childStart.Value.ParentManifest));
            Assert.True(childPublished.IsSuccess, childPublished.ErrorMessage);
            Assert.Equal(2, childPublished.Value!.Manifest.Step);
            Assert.NotNull(childPublished.Value.Manifest.ParentCheckpoint);
            Assert.Equal(
                loadedParent.Value.Manifest.RepositoryRelativeManifestPath,
                childPublished.Value.Manifest.ParentCheckpoint!
                    .RepositoryRelativeManifestPath);
            Assert.Equal(
                loadedParent.Value.Manifest.WeightsSha256,
                childPublished.Value.Manifest.ParentCheckpoint.CheckpointSha256);
            Assert.Equal(
                dpoIdentity,
                childPublished.Value.Manifest.DpoDatasetManifest);
            Assert.Contains(
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .ContinuedFromParentEvidence,
                childPublished.Value.EvidenceRefs);

            var loadedChild =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .LoadLatestCompatibleAsync(target);
            Assert.True(loadedChild.IsSuccess, loadedChild.ErrorMessage);
            Assert.Equal(2, loadedChild.Value!.Manifest.Step);
            var childCheckpoint = loadedChild.Value.Machine.ExportCheckpoint().Value!;
            Assert.Equal(
                ["parent-batch", "child-batch"],
                childCheckpoint.TrainingHistory
                    .Select(entry => entry.BatchId)
                    .ToArray());
            Assert.Equal(
                "training-artifacts/models/figma-html-css/design-to-code/qwen3-5-9b/v0001/checkpoints/000002/manifest.json",
                loadedChild.Value.Manifest.RepositoryRelativeManifestPath);
            Assert.True(
                File.Exists(Path.Combine(
                    repositoryRoot,
                    loadedChild.Value.Manifest.RepositoryRelativeWeightsPath)));
            Assert.True(
                File.Exists(Path.Combine(
                    repositoryRoot,
                    loadedChild.Value.Manifest.RepositoryRelativeManifestPath)));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void PublishNext_RefusesColdStartWithoutPreparedRepositoryStart()
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-repository-model-direct-cold-start-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var target = new DesignConversionRepositoryOwnedModelCheckpointRequest(
                repositoryRoot,
                DomainSlug: "figma-html-css",
                DesignPatternSlug: "design-to-code",
                ModelIdSlug: "qwen3-5-9b",
                ModelVersion: "v0001",
                RuntimeCompatibility: CreateCompatibility("direct-cold-start"),
                AllowColdStart: true,
                ColdStartReason:
                "first accepted DesignConversion repository checkpoint",
                ProvenanceRefs: ["test:direct-cold-start"]);
            var dpoIdentity =
                CreateSealedDesignConversionDpoDatasetIdentity(repositoryRoot);

            // WHY: a direct first-step publish would let a caller skip the
            // disk-first repository probe and invent a cold-start reason. The
            // low-level publisher must require the prepared start returned by
            // the lifecycle loader.
            var published =
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PublishNext(
                        new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                            target,
                            null!,
                            new DesignConversionMachine(),
                            dpoIdentity,
                            DateTimeOffset.UnixEpoch,
                            ["test:direct-cold-start"],
                            ColdStartReason: target.ColdStartReason));

            Assert.False(published.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .InvalidErrorCode,
                published.ErrorCode);
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task PublishNext_RefusesPreparedColdStartForDifferentTarget()
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-repository-model-target-replay-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var target = new DesignConversionRepositoryOwnedModelCheckpointRequest(
                repositoryRoot,
                DomainSlug: "figma-html-css",
                DesignPatternSlug: "design-to-code",
                ModelIdSlug: "qwen3-5-9b",
                ModelVersion: "v0001",
                RuntimeCompatibility: CreateCompatibility("target-replay-a"),
                AllowColdStart: true,
                ColdStartReason:
                "first accepted DesignConversion repository checkpoint",
                ProvenanceRefs: ["test:target-replay-a"]);
            var prepared =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PrepareStartAsync(
                        new DesignConversionRepositoryOwnedModelCheckpointContext(
                            true,
                            target));
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var training =
                await DesignConversionLegalTeacherCapabilityTestFactory
                    .TrainMachineStrictAsync(
                        prepared.Value!.Machine,
                        CreateBatch("target-replay-batch", "target replay"));
            Assert.True(training.IsSuccess, training.ErrorMessage);

            var otherTarget = target with
            {
                ModelIdSlug = "qwen3-5-10b",
                RuntimeCompatibility = CreateCompatibility("target-replay-b"),
                ProvenanceRefs = ["test:target-replay-b"]
            };

            // WHY: a prepared cold start proves that one exact repository
            // model tree was empty. Replaying it into another tree would hide
            // where the first learned weights really belong.
            var published =
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PublishNext(
                        new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                            otherTarget,
                            prepared.Value,
                            prepared.Value.Machine,
                            CreateSealedDesignConversionDpoDatasetIdentity(
                                repositoryRoot),
                            DateTimeOffset.UnixEpoch,
                            ["test:target-replay"],
                            ColdStartReason: otherTarget.ColdStartReason));

            Assert.False(published.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .InvalidErrorCode,
                published.ErrorCode);
            Assert.False(Directory.Exists(Path.Combine(
                repositoryRoot,
                "training-artifacts",
                "models",
                "figma-html-css",
                "design-to-code",
                "qwen3-5-10b")));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task PublishNext_RefusesPreparedColdStartForDifferentMachine()
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-repository-model-machine-replay-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var target = new DesignConversionRepositoryOwnedModelCheckpointRequest(
                repositoryRoot,
                DomainSlug: "figma-html-css",
                DesignPatternSlug: "design-to-code",
                ModelIdSlug: "qwen3-5-9b",
                ModelVersion: "v0001",
                RuntimeCompatibility: CreateCompatibility("machine-replay"),
                AllowColdStart: true,
                ColdStartReason:
                "first accepted DesignConversion repository checkpoint",
                ProvenanceRefs: ["test:machine-replay"]);
            var prepared =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PrepareStartAsync(
                        new DesignConversionRepositoryOwnedModelCheckpointContext(
                            true,
                            target));
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var training =
                await DesignConversionLegalTeacherCapabilityTestFactory
                    .TrainMachineStrictAsync(
                        prepared.Value!.Machine,
                        CreateBatch("machine-replay-batch", "machine replay"));
            Assert.True(training.IsSuccess, training.ErrorMessage);

            // WHY: the loader's machine is the object that was admitted for
            // mutation. A fresh caller-created machine must not borrow that
            // proof and publish unrelated weights.
            var published =
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PublishNext(
                        new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                            target,
                            prepared.Value,
                            new DesignConversionMachine(),
                            CreateSealedDesignConversionDpoDatasetIdentity(
                                repositoryRoot),
                            DateTimeOffset.UnixEpoch,
                            ["test:machine-replay"],
                            ColdStartReason: target.ColdStartReason));

            Assert.False(published.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .InvalidErrorCode,
                published.ErrorCode);
            Assert.False(Directory.Exists(Path.Combine(
                repositoryRoot,
                "training-artifacts",
                "models",
                "figma-html-css",
                "design-to-code",
                "qwen3-5-9b")));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public async Task PublishNext_RefusesResumeWhenParentDiffersFromPreparedStart()
    {
        var repositoryRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-repository-model-parent-replay-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var target = new DesignConversionRepositoryOwnedModelCheckpointRequest(
                repositoryRoot,
                DomainSlug: "figma-html-css",
                DesignPatternSlug: "design-to-code",
                ModelIdSlug: "qwen3-5-9b",
                ModelVersion: "v0001",
                RuntimeCompatibility: CreateCompatibility("parent-replay"),
                AllowColdStart: true,
                ColdStartReason:
                "first accepted DesignConversion repository checkpoint",
                ProvenanceRefs: ["test:parent-replay"]);
            var dpoIdentity =
                CreateSealedDesignConversionDpoDatasetIdentity(repositoryRoot);
            var parentStart =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PrepareStartAsync(
                        new DesignConversionRepositoryOwnedModelCheckpointContext(
                            true,
                            target));
            Assert.True(parentStart.IsSuccess, parentStart.ErrorMessage);
            var parentTraining =
                await DesignConversionLegalTeacherCapabilityTestFactory
                    .TrainMachineStrictAsync(
                        parentStart.Value!.Machine,
                        CreateBatch("parent-replay-parent", "parent replay"));
            Assert.True(parentTraining.IsSuccess, parentTraining.ErrorMessage);
            var parentPublished =
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PublishNext(
                        new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                            target,
                            parentStart.Value,
                            parentStart.Value.Machine,
                            dpoIdentity,
                            DateTimeOffset.UnixEpoch,
                            ["test:parent-replay-parent"],
                            ColdStartReason: target.ColdStartReason));
            Assert.True(parentPublished.IsSuccess, parentPublished.ErrorMessage);

            var childStart =
                await DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PrepareStartAsync(
                        new DesignConversionRepositoryOwnedModelCheckpointContext(
                            true,
                            target));
            Assert.True(childStart.IsSuccess, childStart.ErrorMessage);
            var childTraining =
                await DesignConversionLegalTeacherCapabilityTestFactory
                    .TrainMachineStrictAsync(
                        childStart.Value!.Machine,
                        CreateBatch("parent-replay-child", "child replay"));
            Assert.True(childTraining.IsSuccess, childTraining.ErrorMessage);
            var wrongParent = parentPublished.Value!.Manifest with
            {
                Step = parentPublished.Value.Manifest.Step + 7
            };

            // WHY: a child checkpoint lineage is only honest when the supplied
            // parent is exactly the parent loaded by PrepareStartAsync.
            var childPublished =
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .PublishNext(
                        new DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
                            target,
                            childStart.Value,
                            childStart.Value.Machine,
                            dpoIdentity,
                            DateTimeOffset.UnixEpoch.AddMinutes(1),
                            ["test:parent-replay-child"],
                            ParentCheckpoint: wrongParent));

            Assert.False(childPublished.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedModelCheckpointLifecycle
                    .InvalidErrorCode,
                childPublished.ErrorCode);
            Assert.False(File.Exists(Path.Combine(
                repositoryRoot,
                "training-artifacts",
                "models",
                "figma-html-css",
                "design-to-code",
                "qwen3-5-9b",
                "v0001",
                "checkpoints",
                "000002",
                "manifest.json")));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    private static LearningBatch<DesignConversionDomain.DesignConversionDomain>
        CreateBatch(string batchId, string label)
        => new(
            BatchId: batchId,
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: $"{batchId}-item-001",
                    Prompt: $"figma frame for {label}",
                    ChosenResponse: SerializeOptimizerSide(
                        $"<main class=\"screen\"><section>{label} chosen</section></main>",
                        ".screen{display:grid;gap:16px;}"),
                    RejectedResponse: SerializeOptimizerSide(
                        $"<main class=\"screen\"><section>{label} rejected</section></main>",
                        ".screen{display:block;margin:0;}"))
            ],
            SourceIds: [$"source:{batchId}"]);

    private static RepositoryOwnedModelRuntimeCompatibility
        CreateCompatibility(string label)
        => new(
            ConfigSha256: Hex('a'),
            TopologySha256: Hex('b'),
            TokenizerSha256: Hex('c'),
            TrainerSha256: Hex('e'),
            SchemaSha256: Hex('f'),
            CompatibilityKey: "design-conversion-" + label);

    private static string SerializeOptimizerSide(string html, string css)
        => JsonSerializer.Serialize(new { html, css });

    private static RepositoryOwnedDpoDatasetManifestIdentity
        CreateSealedDesignConversionDpoDatasetIdentity(string repositoryRoot)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repositoryRoot,
                "figma-html-css",
                "design-to-code",
                "v0001");
        Assert.True(address.IsSuccess, address.ErrorMessage);

        var requestIdentity = CreateRepositoryOwnedDpoRequestIdentity(
            "design-conversion-model-checkpoint-lifecycle");
        var pair = CreateRepositoryOwnedDpoPair(
            requestIdentity,
            "design-conversion-model-checkpoint-lifecycle");
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address.Value!,
            requestIdentity,
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
                [Sha256Hex("paid DesignConversion reference " + scope)],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen-test:9b",
                    Hex('c'),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    Hex('d')),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "design-conversion-model-checkpoint-lifecycle",
                    "v1",
                    Sha256Hex("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "design-conversion-model-checkpoint-lifecycle-tests",
                    "v1",
                    Sha256Hex("creator policy " + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private static DpoPair CreateRepositoryOwnedDpoPair(
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
            "repository-owned-checkpoint-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

        var bound = DpoPairTrainingContentAuthority.Bind(new DpoPair(
            "convert Figma tree to HTML and CSS for " + scope,
            SerializeOptimizerSide(
                "<main class=\"card\"><h1>open teacher</h1></main>",
                ".card{display:grid;gap:1rem;}"),
            SerializeOptimizerSide(
                "<main><h1>local candidate</h1></main>",
                "main{display:block;}"),
            "figma-html-css",
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

    private static string Hex(char value) => new(value, 64);

    private static string Sha256Hex(string value)
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static void CreateRepositorySentinels(string repositoryRoot)
    {
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "fixture solution sentinel");
        Directory.CreateDirectory(
            Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
    }

    private static void DeleteDirectoryIfExists(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Test cleanup only: the repository-owned lifecycle under test has
            // already written into this per-test temporary repository root.
        }
    }
}
