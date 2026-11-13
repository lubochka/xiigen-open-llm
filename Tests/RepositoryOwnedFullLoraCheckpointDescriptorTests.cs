// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
using System.Security.Cryptography;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using LoRA;
using ModelServer;
using Tokenizer;
using TrainableRouter.Training;
using TransformerModel;
using TransformerModel.Config;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

namespace LlmFromScratch.Tests;

public sealed class RepositoryOwnedFullLoraCheckpointDescriptorTests
{
    [Fact]
    public async Task DeclaredInitialFullLora_UsesStrictRootAndLoadsAllTensorsIntoIndependentFrozenBaseClones()
    {
        // Engineering serialization fixture only: same55 named slots, tiny dimensions,
        // no inference/optimizer/Teacher and no real B0 model construction.
        var root = Path.Combine(Path.GetTempPath(), "xiigen-full-lora-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "LlmFromScratch.sln"), "synthetic fixture sentinel");
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        try
        {
            var dpo = CreateSealedSyntheticDataset(root);
            var tokenizer = new BpeTokenizer(); tokenizer.Train("synthetic startup boundary only", 256);
            var config = new ModelConfig { VocabSize = 256, ContextLen = 128, HiddenDim = 16,
                NumLayers = 4, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 32 };
            var lora = new LoraConfig { Rank = 8, Alpha = 16, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
            var seed = new LoraPairRandomSeedAuthority(20261014, "rp01-policy", "rp01-reference");
            var runtime = new RepositoryOwnedModelRuntimeCompatibility(Hex('a'),
                LoraRandomStateIdentity.ComputeTopologySha256(config, lora, seed), tokenizer.ComputeStateSha256(),
                Hex('c'), Hex('d'), "synthetic-full-lora");
            var start = new CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(root,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug, "synthetic-root", "v1",
                config, runtime, RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
                LoraConfig: lora, AdapterSeedAuthority: seed, BaseInitializationSeed: seed.RootInitializationSeed);
            var generation = new CodeDocumentationGenerationConfigurationResult(
                new(start, tokenizer, new(8, 0, 0, 1, 1, 1024)), null);
            var address = Success(RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(root,
                start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, start.ModelVersion, 1));
            var model = new LlmModel(config);
            LoraApplicator.ApplySeeded(model, lora, seed.RootInitializationSeed, seed.PolicyDropoutStreamDomain);
            var valueIndex = 0;
            foreach (var parameter in model.Parameters())
                for (var index = 0; index < parameter.Value.Data.Length; index++)
                    parameter.Value.Data[index] = (++valueIndex) / 1000f;
            Assert.Equal(55, model.GetCheckpointParameterEntries().Count);
            var sourcePath = Path.Combine(root, "synthetic-full.bin"); model.SaveCheckpoint(sourcePath);
            var originalBytes = File.ReadAllBytes(sourcePath);
            using var original = new MemoryStream(originalBytes, writable: false);
            var parent = Success(RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(address, original,
                new(null, "synthetic initial state only", dpo, runtime, DateTimeOffset.UnixEpoch, ["test:full-lora-root"])
                    { InitialBaselineNoUpdate = true }));
            var selectors = new Dictionary<string, string?>
            {
                [CodeDocumentationInitialTrainingConfiguration.ModeEnvironmentName] = CodeDocumentationInitialTrainingConfiguration.FirstRootUpdateMode,
                [CodeDocumentationInitialTrainingConfiguration.ReasonEnvironmentName] = "Synthetic engineering fixture only; exact saved root first-update selector.",
                [CodeDocumentationInitialTrainingConfiguration.ParentEnvironmentName] = parent.ManifestSha256
            };
            var initial = CodeDocumentationInitialTrainingConfiguration.Resolve(name => selectors.GetValueOrDefault(name), generation);
            var declared = Assert.IsType<RepositoryOwnedFullLoraCheckpointDescriptor>(Success(initial.BindCheckpointForStartup()));
            Assert.Equal(parent.ManifestSha256, declared.ManifestSha256);
            Assert.Equal(parent.WeightsSha256, declared.WeightsSha256);
            Assert.True(Success(declared.MatchesModelConfig(config)));
            var wrongShape = config with { ContextLen = config.ContextLen + 1 };
            Assert.False(declared.MatchesModelConfig(wrongShape).IsSuccess);
            Assert.False(declared.CreateModelForLoad(wrongShape).IsSuccess);
            using (var retained = new MemoryStream(originalBytes, writable: false))
                Assert.True(Success(declared.ValidateArtifact(retained, config, originalBytes.Length)));
            var changed = originalBytes.ToArray(); changed[^1] ^= 1;
            using (var wrong = new MemoryStream(changed, writable: false))
            {
                var refused = declared.ValidateArtifact(wrong, config, changed.Length);
                Assert.False(refused.IsSuccess);
                Assert.Equal("REPOSITORY_OWNED_FULL_LORA_CHECKPOINT_IDENTITY_MISMATCH", refused.ErrorCode);
            }
            var source = new FixtureSource(originalBytes);
            var options = new DesignConversionDpoPolicyBaseProviderOptions(seed.RootInitializationSeed, Hex('f'))
                { InitialFullCheckpoint = declared };
            var provider = Success(DesignConversionCheckpointBackedDpoPolicyBaseProvider.CreateWithOptions(
                source, config, "synthetic-root", "v1", parent.WeightsSha256, options));
            var models = Success(await provider.CreateModelSetAsync(CancellationToken.None));
            var loadedIndex = 0;
            foreach (var loaded in new[] { models.Policy, models.Reference, models.FreshLoadTarget })
            {
                var path = Path.Combine(root, $"loaded-{loadedIndex++}.bin"); loaded.SaveCheckpoint(path);
                Assert.Equal(originalBytes, File.ReadAllBytes(path));
                Assert.All(loaded.BaseParameters, parameter => Assert.False(parameter.Value.RequiresGrad));
            }
            Assert.NotSame(models.Policy.BaseParameters[0].Value.Data, models.Reference.BaseParameters[0].Value.Data);
            Assert.NotSame(models.Policy.BaseParameters[0].Value.Data, models.FreshLoadTarget.BaseParameters[0].Value.Data);
            var reference = models.Reference.Parameters().SelectMany(p => p.Value.Data).ToArray();
            var fresh = models.FreshLoadTarget.Parameters().SelectMany(p => p.Value.Data).ToArray();
            models.Policy.Parameters().Last().Value.Data[0] += 1f;
            Assert.Equal(reference, models.Reference.Parameters().SelectMany(p => p.Value.Data));
            Assert.Equal(fresh, models.FreshLoadTarget.Parameters().SelectMany(p => p.Value.Data));
            var undeclared = Success(DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                source, config, "synthetic-root", "v1", parent.WeightsSha256));
            Assert.False((await undeclared.CreateModelSetAsync(CancellationToken.None)).IsSuccess);
            Assert.False(DesignConversionCheckpointBackedDpoPolicyBaseProvider.CreateWithOptions(
                source, config, "synthetic-root", "v1", Hex('0'), options).IsSuccess);
            var wrongStart = start with { LoraConfig = lora with { Rank = 4 } };
            var wrongGeneration = generation with { Options = generation.Options! with { StartRequest = wrongStart } };
            Assert.False(CodeDocumentationInitialTrainingConfiguration.Resolve(name => selectors.GetValueOrDefault(name), wrongGeneration)
                .BindCheckpointForStartup().IsSuccess);
            Assert.Null(Success(CodeDocumentationInitialTrainingConfiguration.Resolve(_ => null, generation).BindCheckpointForStartup()));
            selectors[CodeDocumentationInitialTrainingConfiguration.ParentEnvironmentName] = Hex('0');
            Assert.True(initial.BindCheckpointForStartup().IsSuccess); // The captured valid selector is immutable.
            Assert.False(CodeDocumentationInitialTrainingConfiguration.Resolve(name => selectors.GetValueOrDefault(name), generation)
                .BindCheckpointForStartup().IsSuccess);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static RepositoryOwnedDpoDatasetManifestIdentity CreateSealedSyntheticDataset(string root)
    {
        var address = Success(RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(root,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DefaultDatasetVersion));
        var identity = Success(RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity([Hex('a')],
            new(OpenWeightTeacherFamily.Qwen.ToString(), "qwen-synthetic-test:9b", Hex('c'),
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind, Hex('d')),
            new("synthetic-full-lora", "v1", Hex('b')), new("synthetic-full-lora", "v1", Hex('e'))));
        var passport = new TrainingTeacherProvenance(TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen, TrainingTeacherAccessMode.LocalHostedOpenWeight, TrainingTeacherHostingProvider.Ollama,
            identity.OpenTeacher.ModelId, identity.OpenTeacher.ModelVersion, TrainingTeacherModelLicense.Apache20,
            false, false, false, true, "synthetic-fixture-no-inference", 1)
            { ModelArtifactDigestKind = identity.OpenTeacher.ModelArtifactDigestKind, ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256 };
        var pair = Success(DpoPairTrainingContentAuthority.Bind(new DpoPair("Synthetic fixture question", "Synthetic chosen explanation",
            "Synthetic rejected explanation", CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug, DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(true, false, ["sha256:" + Hex('a')]),
            ChosenAuthorship = new DpoPairSideAuthorship(DpoPairSideAuthorKind.OpenTeacher, passport),
            RejectedAuthorship = new DpoPairSideAuthorship(DpoPairSideAuthorKind.LocalModel)
        }));
        var manifest = Success(RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(address, identity, [pair]));
        return new(address.ManifestRelativePath, manifest.ManifestSha256);
    }

    private sealed class FixtureSource(byte[] bytes) : IDesignConversionDpoPolicyCheckpointSource
    {
        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(CancellationToken cancellationToken)
            => Task.FromResult(OperationResult<Stream>.Ok(new MemoryStream(bytes, writable: false)));
    }
    private static string Hex(char value) => new(value, 64);
    private static T Success<T>(OperationResult<T> result)
    { Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage); return result.Value!; }
}
