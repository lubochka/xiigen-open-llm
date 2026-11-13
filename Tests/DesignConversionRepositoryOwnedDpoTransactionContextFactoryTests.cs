// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;

[Collection(RepoArtifactSnapshotCollectionNames.CollectionName)]
public sealed class DesignConversionRepositoryOwnedDpoTransactionContextFactoryTests
{
    [Fact]
    public void FactoryConfigurationAndMaterialRemainInternalAuthorityBoundary()
    {
        Assert.True(
            typeof(DesignConversionRepositoryOwnedDpoTransactionContextFactory)
                .IsNotPublic);
        Assert.True(
            typeof(DesignConversionRepositoryOwnedDpoTransactionContextFactoryConfiguration)
                .IsNotPublic);
        Assert.True(typeof(DesignConversionPaidReferenceMaterial).IsNotPublic);
    }

    [Fact]
    public void Create_SameHtmlCssBuildsDeterministicIdentityAndCanonicalAddress()
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "dcm-dpo-identity-factory-deterministic");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var first = CreateContext(repositoryRoot);
            var second = CreateContext(repositoryRoot);

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(second.IsSuccess, second.ErrorMessage);
            Assert.True(first.Value!.Validate().IsSuccess);
            Assert.True(second.Value!.Validate().IsSuccess);
            Assert.Equal(
                first.Value.RequestIdentity!.RequestIdentitySha256,
                second.Value!.RequestIdentity!.RequestIdentitySha256);
            Assert.Equal(
                first.Value.RequestIdentity.PaidReferenceAnswers
                    .CanonicalListSha256,
                second.Value.RequestIdentity.PaidReferenceAnswers
                    .CanonicalListSha256);
            Assert.Equal(
                NormalizeRepoRelative(
                    "training-data/dpo/figma-html-css/design-to-code/v0001/manifest.json"),
                first.Value.Address!.ManifestRelativePath);
            Assert.Equal(
                Path.GetFullPath(repositoryRoot),
                first.Value.Address.RepositoryRoot);
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-data")));
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-artifacts")));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Fact]
    public void Create_PaidReferenceContentChangeChangesReferenceHashAndRequestIdentity()
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "dcm-dpo-identity-factory-content-change");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var first = CreateContext(repositoryRoot);
            var changed = CreateContext(
                repositoryRoot,
                html: "<main class=\"paid\"><h1>Changed</h1></main>");

            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.True(changed.IsSuccess, changed.ErrorMessage);
            Assert.NotEqual(
                first.Value!.RequestIdentity!.PaidReferenceAnswers
                    .OrderedAnswerSha256.Single(),
                changed.Value!.RequestIdentity!.PaidReferenceAnswers
                    .OrderedAnswerSha256.Single());
            Assert.NotEqual(
                first.Value.RequestIdentity.RequestIdentitySha256,
                changed.Value.RequestIdentity.RequestIdentitySha256);
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-data")));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("relative-root")]
    [InlineData("foreign-root")]
    [InlineData("foreign-normalized-root")]
    public void Create_ForeignOrRelativeAssertedRepositoryRootFailsClosed(
        string caseName)
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "dcm-dpo-identity-factory-root");
        var foreignRoot = CreateRepositoryFixtureRoot(
            "dcm-dpo-identity-factory-foreign-root");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            Directory.CreateDirectory(foreignRoot);
            var suppliedRoot = caseName switch
            {
                "relative-root" => ".",
                "foreign-root" => foreignRoot,
                "foreign-normalized-root" => Path.Combine(
                    foreignRoot,
                    "..",
                    Path.GetFileName(foreignRoot)),
                _ => throw new InvalidOperationException(caseName)
            };

            var result = CreateContext(
                repositoryRoot,
                assertedRepositoryRoot: suppliedRoot);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .InvalidErrorCode,
                result.ErrorCode);
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-data")));
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-artifacts")));
            if (caseName == "foreign-root")
            {
                Assert.False(Directory.Exists(
                    Path.Combine(foreignRoot, "training-data")));
                Assert.False(Directory.Exists(
                    Path.Combine(foreignRoot, "training-artifacts")));
            }
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
            DeleteDirectoryIfExists(foreignRoot);
        }
    }

    [Theory]
    [InlineData("domain")]
    [InlineData("pattern")]
    [InlineData("dataset-version")]
    public void Create_InvalidDomainPatternOrDatasetVersionFailsClosed(
        string field)
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "dcm-dpo-identity-factory-slug");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var configuration = CreateConfiguration(repositoryRoot) with
            {
                DomainSlug = field == "domain"
                    ? "translation"
                    : DesignConversionRepositoryOwnedDpoTransactionContextFactory
                        .FixedDomainSlug,
                DesignPatternSlug = field == "pattern"
                    ? "design/escape"
                    : DesignConversionRepositoryOwnedDpoTransactionContextFactory
                        .FixedDesignPatternSlug,
                DatasetVersion = field == "dataset-version"
                    ? "../v0001"
                    : "v0001"
            };

            var result =
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .Create(configuration);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .InvalidErrorCode,
                result.ErrorCode);
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-data")));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("non-qwen")]
    [InlineData("non-local-ollama")]
    [InlineData("closed-teacher")]
    [InlineData("missing-artifact-hash")]
    public void Create_InvalidTeacherIdentityFailsClosed(string mutation)
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "dcm-dpo-identity-factory-teacher");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var teacher = CreateOpenTeacher() with
            {
                Family = mutation == "non-qwen"
                    ? OpenWeightTeacherFamily.Mistral.ToString()
                    : OpenWeightTeacherFamily.Qwen.ToString(),
                ModelId = mutation switch
                {
                    "non-local-ollama" => "qwen",
                    "closed-teacher" => "openai/qwen:9b",
                    _ => "qwen3.5:9b"
                },
                ModelArtifactSha256 = mutation == "missing-artifact-hash"
                    ? string.Empty
                    : HexForTest('d')
            };

            var result = CreateContext(
                repositoryRoot,
                openTeacher: teacher);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .InvalidErrorCode,
                result.ErrorCode);
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-data")));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    [Theory]
    [InlineData("policy-hash")]
    [InlineData("creator-hash")]
    [InlineData("empty-html")]
    public void Create_InvalidPolicyCreatorOrPaidMaterialFailsClosed(
        string mutation)
    {
        var repositoryRoot = CreateRepositoryFixtureRoot(
            "dcm-dpo-identity-factory-policy");
        try
        {
            CreateRepositorySentinels(repositoryRoot);
            var configuration = CreateConfiguration(
                repositoryRoot,
                html: mutation == "empty-html"
                    ? string.Empty
                    : "<main class=\"paid\"><h1>Paid</h1></main>") with
            {
                GenerationPolicy = mutation == "policy-hash"
                    ? CreateGenerationPolicy() with { ConfigSha256 = "missing" }
                    : CreateGenerationPolicy(),
                Creator = mutation == "creator-hash"
                    ? CreateCreator() with { EffectiveConfigSha256 = "missing" }
                    : CreateCreator()
            };

            var result =
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .Create(configuration);

            Assert.False(result.IsSuccess);
            Assert.Equal(
                DesignConversionRepositoryOwnedDpoTransactionContextFactory
                    .InvalidErrorCode,
                result.ErrorCode);
            Assert.False(Directory.Exists(
                Path.Combine(repositoryRoot, "training-data")));
        }
        finally
        {
            DeleteDirectoryIfExists(repositoryRoot);
        }
    }

    private static OperationResult<DesignConversionRepositoryOwnedDpoTransactionContext>
        CreateContext(
            string repositoryRoot,
            string html = "<main class=\"paid\"><h1>Paid</h1></main>",
            string css = ".paid{display:grid;gap:16px;}",
            RepositoryOwnedDpoOpenTeacherIdentity? openTeacher = null,
            string? assertedRepositoryRoot = null)
        => DesignConversionRepositoryOwnedDpoTransactionContextFactory.Create(
            CreateConfiguration(
                repositoryRoot,
                html,
                css,
                openTeacher,
                assertedRepositoryRoot));

    private static DesignConversionRepositoryOwnedDpoTransactionContextFactoryConfiguration
        CreateConfiguration(
            string repositoryRoot,
            string html = "<main class=\"paid\"><h1>Paid</h1></main>",
            string css = ".paid{display:grid;gap:16px;}",
            RepositoryOwnedDpoOpenTeacherIdentity? openTeacher = null,
            string? assertedRepositoryRoot = null)
    {
        var material =
            DesignConversionPaidReferenceMaterial.Create(
                html,
                css);
        return new DesignConversionRepositoryOwnedDpoTransactionContextFactoryConfiguration(
            repositoryRoot,
            assertedRepositoryRoot ?? repositoryRoot,
            DesignConversionRepositoryOwnedDpoTransactionContextFactory
                .FixedDomainSlug,
            DesignConversionRepositoryOwnedDpoTransactionContextFactory
                .FixedDesignPatternSlug,
            DatasetVersion: "v0001",
            openTeacher ?? CreateOpenTeacher(),
            CreateGenerationPolicy(),
            CreateCreator(),
            material.IsSuccess ? [material.Value!] : []);
    }

    private static RepositoryOwnedDpoOpenTeacherIdentity CreateOpenTeacher()
        => new(
            OpenWeightTeacherFamily.Qwen.ToString(),
            "qwen3.5:9b",
            HexForTest('c'),
            LegalTrainingTeacherAdmissionAuthority
                .OllamaModelLayerSha256DigestKind,
            HexForTest('d'));

    private static RepositoryOwnedDpoGenerationPolicyIdentity
        CreateGenerationPolicy()
        => new(
            "design-conversion-dpo-identity-factory",
            "v1",
            HashIdentity("generation policy"));

    private static RepositoryOwnedDpoCreatorIdentity CreateCreator()
        => new(
            "design-conversion-dpo-identity-factory-tests",
            "v1",
            HashIdentity("creator config"));

    private static string CreateRepositoryFixtureRoot(string slug)
        => Path.Combine(
            Path.GetTempPath(),
            slug + "-" + Guid.NewGuid().ToString("N"));

    private static void CreateRepositorySentinels(string repositoryRoot)
    {
        Directory.CreateDirectory(repositoryRoot);
        File.WriteAllText(
            Path.Combine(repositoryRoot, "LlmFromScratch.sln"),
            "DesignConversion repository-owned DPO identity factory fixture",
            Encoding.UTF8);
        Directory.CreateDirectory(
            Path.Combine(repositoryRoot, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, "11.DPO"));
    }

    private static string NormalizeRepoRelative(string path)
        => path.Replace('\\', '/');

    private static string HexForTest(char value) => new(value, 64);

    private static string HashIdentity(string value)
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static void DeleteDirectoryIfExists(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Test-репозитории одноразовые; сбой cleanup не должен скрыть
            // проверку, что сама factory ничего не записала на диск.
        }
    }
}
