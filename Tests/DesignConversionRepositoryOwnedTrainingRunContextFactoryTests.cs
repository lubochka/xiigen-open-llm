// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

[Collection(RepoArtifactSnapshotCollectionNames.CollectionName)]
public sealed class DesignConversionRepositoryOwnedTrainingRunContextFactoryTests
{
    [Fact]
    public void Create_SealedDpoDatasetBuildsEnabledContextsWithoutModelWrites()
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "design-conversion-context-factory-success");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var sealedDataset = CreateSealedDesignConversionDpoDataset(
                repositoryRoot,
                "context-factory-success");
            var provenanceRefs = new List<string>
            {
                "test:design-conversion-context-factory-success"
            };

            var result =
                DesignConversionRepositoryOwnedTrainingRunContextFactory.Create(
                    CreateSpec(repositoryRoot, provenanceRefs));

            Assert.True(result.IsSuccess, result.ErrorMessage);
            var contexts = result.Value!;
            Assert.True(contexts.DpoTransactionContext.Enabled);
            AssertDpoAddressEqual(
                sealedDataset.Address,
                contexts.DpoTransactionContext.Address!);
            AssertRequestIdentityEqual(
                sealedDataset.RequestIdentity,
                contexts.DpoTransactionContext.RequestIdentity);
            Assert.True(
                contexts.DpoTransactionContext.Validate().IsSuccess);
            Assert.True(contexts.ModelCheckpointContext.Enabled);
            Assert.True(contexts.ModelCheckpointContext.Validate().IsSuccess);
            Assert.NotNull(contexts.ModelCheckpointContext.Request);
            Assert.Equal(
                Path.GetFullPath(repositoryRoot),
                contexts.ModelCheckpointContext.Request!.RepositoryRoot);
            Assert.Equal(
                "figma-html-css",
                contexts.ModelCheckpointContext.Request.DomainSlug);
            Assert.Equal(
                "design-to-code",
                contexts.ModelCheckpointContext.Request.DesignPatternSlug);
            Assert.Equal(
                "qwen3-5-9b",
                contexts.ModelCheckpointContext.Request.ModelIdSlug);
            Assert.Equal(
                CreateCompatibility("context-factory"),
                contexts.ModelCheckpointContext.Request.RuntimeCompatibility);
            Assert.True(contexts.ModelCheckpointContext.Request.AllowColdStart);
            Assert.Equal(
                "first design-to-code checkpoint after sealed DPO",
                contexts.ModelCheckpointContext.Request.ColdStartReason);
            Assert.Equal(
                ["test:design-conversion-context-factory-success"],
                contexts.ModelCheckpointContext.Request.ProvenanceRefs);

            provenanceRefs.Add("test:mutated-after-create");
            Assert.Equal(
                ["test:design-conversion-context-factory-success"],
                contexts.ModelCheckpointContext.Request.ProvenanceRefs);
            Assert.False(
                Directory.Exists(ModelArtifactRoot(repositoryRoot)));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Create_MissingDpoManifestFailsClosedWithoutModelDirectory()
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "design-conversion-context-factory-missing");
        try
        {
            CreateRepositorySentinels(repositoryRoot);

            var result =
                DesignConversionRepositoryOwnedTrainingRunContextFactory.Create(
                    CreateSpec(repositoryRoot));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedTrainingRunContextFactory
                    .DpoDatasetUnavailableErrorCode,
                result.ErrorCode);
            Assert.False(
                Directory.Exists(ModelArtifactRoot(repositoryRoot)));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Create_CorruptDpoManifestFailsClosedWithoutModelDirectory()
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "design-conversion-context-factory-corrupt-manifest");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var address = CreateDpoDatasetAddress(repositoryRoot);
            Directory.CreateDirectory(Path.GetDirectoryName(address.ManifestPath)!);
            File.WriteAllText(
                address.ManifestPath,
                "{ this is not a sealed DPO manifest",
                Encoding.UTF8);

            var result =
                DesignConversionRepositoryOwnedTrainingRunContextFactory.Create(
                    CreateSpec(repositoryRoot));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedTrainingRunContextFactory
                    .DpoDatasetUnavailableErrorCode,
                result.ErrorCode);
            Assert.False(
                Directory.Exists(ModelArtifactRoot(repositoryRoot)));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Create_MissingPairFileFailsClosedInsteadOfRecreatingDpo()
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "design-conversion-context-factory-partial-dpo");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var sealedDataset = CreateSealedDesignConversionDpoDataset(
                repositoryRoot,
                "context-factory-partial");
            var pair = Assert.Single(sealedDataset.Manifest.OrderedPairs);
            File.Delete(Path.Combine(
                repositoryRoot,
                pair.RepositoryRelativePairFilePath));

            var result =
                DesignConversionRepositoryOwnedTrainingRunContextFactory.Create(
                    CreateSpec(repositoryRoot));

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedTrainingRunContextFactory
                    .DpoDatasetUnavailableErrorCode,
                result.ErrorCode);
            Assert.False(
                Directory.Exists(ModelArtifactRoot(repositoryRoot)));
            Assert.True(File.Exists(sealedDataset.Address.ManifestPath));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Create_InvalidModelSlugFailsClosedAfterSealedDpoValidation()
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "design-conversion-context-factory-invalid-model");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            CreateSealedDesignConversionDpoDataset(
                repositoryRoot,
                "context-factory-invalid-model");

            var result =
                DesignConversionRepositoryOwnedTrainingRunContextFactory.Create(
                    CreateSpec(repositoryRoot) with
                    {
                        ModelIdSlug = "../qwen3-5-9b"
                    });

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedTrainingRunContextFactory
                    .InvalidErrorCode,
                result.ErrorCode);
            Assert.False(
                Directory.Exists(ModelArtifactRoot(repositoryRoot)));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    private sealed record SealedDpoDatasetFixture(
        RepositoryOwnedDpoDatasetAddress Address,
        RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
        RepositoryOwnedDpoDatasetManifest Manifest);

    private static DesignConversionRepositoryOwnedTrainingRunContextSpec
        CreateSpec(
            string repositoryRoot,
            IReadOnlyList<string>? provenanceRefs = null)
        => new(
            repositoryRoot,
            DomainSlug: "figma-html-css",
            DesignPatternSlug: "design-to-code",
            DatasetVersion: "v0001",
            ModelIdSlug: "qwen3-5-9b",
            ModelVersion: "v0001",
            RuntimeCompatibility: CreateCompatibility("context-factory"),
            AllowColdStart: true,
            ColdStartReason: "first design-to-code checkpoint after sealed DPO",
            ProvenanceRefs:
            provenanceRefs ??
            ["test:design-conversion-context-factory"]);

    private static SealedDpoDatasetFixture
        CreateSealedDesignConversionDpoDataset(
            string repositoryRoot,
            string scope)
    {
        var address = CreateDpoDatasetAddress(repositoryRoot);
        var requestIdentity = CreateRepositoryOwnedDpoRequestIdentity(scope);
        var pair = CreateRepositoryOwnedDpoPair(requestIdentity, scope);
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address,
            requestIdentity,
            [pair]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        return new SealedDpoDatasetFixture(
            address,
            requestIdentity,
            manifest.Value!);
    }

    private static RepositoryOwnedDpoDatasetAddress CreateDpoDatasetAddress(
        string repositoryRoot)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repositoryRoot,
                "figma-html-css",
                "design-to-code",
                "v0001");
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static void AssertDpoAddressEqual(
        RepositoryOwnedDpoDatasetAddress expected,
        RepositoryOwnedDpoDatasetAddress actual)
    {
        Assert.Equal(expected.RepositoryRoot, actual.RepositoryRoot);
        Assert.Equal(expected.DpoArtifactRoot, actual.DpoArtifactRoot);
        Assert.Equal(expected.DomainSlug, actual.DomainSlug);
        Assert.Equal(expected.DesignPatternSlug, actual.DesignPatternSlug);
        Assert.Equal(expected.DatasetVersion, actual.DatasetVersion);
        Assert.Equal(expected.DatasetRoot, actual.DatasetRoot);
        Assert.Equal(expected.PairStoreRoot, actual.PairStoreRoot);
        Assert.Equal(expected.PairsRoot, actual.PairsRoot);
        Assert.Equal(expected.ManifestPath, actual.ManifestPath);
        Assert.Equal(
            expected.DatasetRootRelativePath,
            actual.DatasetRootRelativePath);
        Assert.Equal(
            expected.PairStoreRootRelativePath,
            actual.PairStoreRootRelativePath);
        Assert.Equal(
            expected.PairsRootRelativePath,
            actual.PairsRootRelativePath);
        Assert.Equal(expected.ManifestRelativePath, actual.ManifestRelativePath);
    }

    private static void AssertRequestIdentityEqual(
        RepositoryOwnedDpoDatasetRequestIdentity expected,
        RepositoryOwnedDpoDatasetRequestIdentity? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Schema, actual!.Schema);
        Assert.Equal(
            expected.RequestIdentitySha256,
            actual.RequestIdentitySha256);
        Assert.Equal(
            expected.PaidReferenceAnswers.CanonicalListSha256,
            actual.PaidReferenceAnswers.CanonicalListSha256);
        Assert.Equal(
            expected.PaidReferenceAnswers.OrderedAnswerSha256,
            actual.PaidReferenceAnswers.OrderedAnswerSha256);
        Assert.Equal(expected.OpenTeacher, actual.OpenTeacher);
        Assert.Equal(expected.GenerationPolicy, actual.GenerationPolicy);
        Assert.Equal(expected.Creator, actual.Creator);
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CreateRepositoryOwnedDpoRequestIdentity(string scope)
    {
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [HashIdentity("paid DesignConversion reference " + scope)],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    "qwen-test:9b",
                    HexForTest('c'),
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    HexForTest('d')),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "design-conversion-context-factory",
                    "v1",
                    HashIdentity("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "design-conversion-context-factory-tests",
                    "v1",
                    HashIdentity("creator policy " + scope)));
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
            "repository-owned-context-factory-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };

        var bound = DpoPairTrainingContentAuthority.Bind(new DpoPair(
            "convert Figma tree to HTML and CSS for " + scope,
            SerializeRepositoryModelSide(
                "<main class=\"card\"><h1>open teacher</h1></main>",
                ".card{display:grid;gap:1rem;}"),
            SerializeRepositoryModelSide(
                "<main><h1>local candidate</h1></main>",
                "main{display:block;}"),
            "figma-html-css",
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs: ["sha256:" + HashIdentity("governance " + scope)]),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DpoPairSideAuthorKind.LocalModel)
        });
        Assert.True(bound.IsSuccess, bound.ErrorMessage);
        return bound.Value!;
    }

    private static RepositoryOwnedModelRuntimeCompatibility
        CreateCompatibility(string label)
        => new(
            ConfigSha256: HexForTest('a'),
            TopologySha256: HexForTest('b'),
            TokenizerSha256: HexForTest('c'),
            TrainerSha256: HexForTest('d'),
            SchemaSha256: HexForTest('e'),
            CompatibilityKey: "design-conversion-" + label);

    private static string SerializeRepositoryModelSide(string html, string css)
        => JsonSerializer.Serialize(new { html, css });

    private static string HexForTest(char value) => new(value, 64);

    private static string HashIdentity(string value)
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string CreateRepositoryFixtureRoot(string slug)
        => Path.Combine(
            Path.GetTempPath(),
            slug + "-" + Guid.NewGuid().ToString("N"));

    private static string ModelArtifactRoot(string repositoryRoot)
        => Path.Combine(repositoryRoot, "training-artifacts", "models");

    private static void CreateRepositorySentinels(string repositoryRoot)
    {
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "DesignConversion repository-owned context factory fixture");
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
            // These are per-test throwaway repository roots. A cleanup issue
            // must not make the production repository look dirty or hide the
            // assertion that just ran.
        }
    }
}
