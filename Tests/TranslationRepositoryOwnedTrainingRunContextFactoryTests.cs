namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using ContinuousLearning;
using DPO.Models;
using TranslationDomain;

public sealed class TranslationRepositoryOwnedTrainingRunContextFactoryTests
    : IDisposable
{
    private static readonly Encoding Utf8NoBom =
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly string _tempRoot = Path.Combine(
        Path.GetTempPath(),
        $"translation-run-context-factory-{Guid.NewGuid():N}");

    public TranslationRepositoryOwnedTrainingRunContextFactoryTests()
        => Directory.CreateDirectory(_tempRoot);

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    [Fact]
    public void Create_ValidSealedDatasetYieldsEnabledContextsWithExactIdentity()
    {
        var fixture = CreateSealedDataset("valid-exact-identity");
        var before = SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot);
        var spec = CreateSpec(fixture, "valid-exact-identity");

        var result = TranslationRepositoryOwnedTrainingRunContextFactory
            .Create(spec);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var contexts = result.Value!;
        Assert.True(contexts.DpoDatasetContext.Enabled);
        Assert.True(contexts.ModelCheckpointContext.Enabled);
        AssertRequestIdentityEqual(
            fixture.Manifest.RequestIdentity,
            contexts.DpoDatasetContext.RequestIdentity);
        Assert.Equal(
            fixture.Address.ManifestRelativePath,
            contexts.DpoDatasetContext.Address!.ManifestRelativePath);
        Assert.Equal(
            fixture.Address.RepositoryRoot,
            contexts.ModelCheckpointContext.Request!.Address.RepositoryRoot);
        Assert.Equal(
            fixture.Address.DomainSlug,
            contexts.ModelCheckpointContext.Request.Address.DomainSlug);
        Assert.Equal(
            fixture.Address.DesignPatternSlug,
            contexts.ModelCheckpointContext.Request.Address.DesignPatternSlug);
        Assert.Equal(
            spec.ModelIdSlug,
            contexts.ModelCheckpointContext.Request.Address.ModelIdSlug);
        Assert.Equal(
            spec.ModelVersion,
            contexts.ModelCheckpointContext.Request.Address.ModelVersion);

        var stepOne = contexts.ModelCheckpointContext.Request.Address
            .CreateStepAddress(1);
        Assert.True(stepOne.IsSuccess, stepOne.ErrorMessage);
        Assert.Equal(
            $"training-artifacts/models/{fixture.Address.DomainSlug}/{fixture.Address.DesignPatternSlug}/{spec.ModelIdSlug}/{spec.ModelVersion}/checkpoints/000001",
            stepOne.Value!.CheckpointRootRelativePath);
        Assert.Equal(
            before,
            SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot));
    }

    [Fact]
    public void Create_MissingSealedDatasetFailsAndCreatesNoArtifactDirectories()
    {
        var repositoryRoot = CreateRepositoryOwnedFixtureRoot(
            "missing-sealed-dataset");
        var spec = CreateSpec(
            repositoryRoot,
            "translation-domain",
            "dpo-train-evaluate",
            "v" + ComputeTextSha256("missing-sealed-dataset")[..12],
            "translation-policy-" +
            ComputeTextSha256("model-missing-sealed-dataset")[..12],
            "v" + ComputeTextSha256("model-version-missing-sealed-dataset")[..12],
            "missing-sealed-dataset");
        var before = SnapshotRelativeFileSystemEntries(repositoryRoot);

        var result = TranslationRepositoryOwnedTrainingRunContextFactory
            .Create(spec);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationRepositoryOwnedTrainingRunContextFactory
                .DpoDatasetUnavailableErrorCode,
            result.ErrorCode);
        Assert.DoesNotContain(
            repositoryRoot,
            result.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(repositoryRoot));
        Assert.False(Directory.Exists(Path.Combine(repositoryRoot, "training-data")));
        Assert.False(Directory.Exists(Path.Combine(repositoryRoot, "training-artifacts")));
    }

    [Fact]
    public void Create_CleanMissCarriesLazyQwenAuthoringWithoutCreatingClientOrFiles()
    {
        var repositoryRoot = CreateRepositoryOwnedFixtureRoot(
            "clean-miss-lazy-authoring");
        var spec = CreateSpec(
            repositoryRoot,
            "translation-domain",
            "dpo-train-evaluate",
            "v" + ComputeTextSha256("clean-miss-lazy-authoring")[..12],
            "translation-policy-" +
            ComputeTextSha256("model-clean-miss-lazy-authoring")[..12],
            "v" + ComputeTextSha256(
                "model-version-clean-miss-lazy-authoring")[..12],
            "clean-miss-lazy-authoring");
        var identity = CreateRepositoryOwnedDpoRequestIdentity(
            "clean-miss-lazy-authoring");
        var clientFactoryCalls = 0;
        var authoring = new
            TranslationRepositoryOwnedDpoDatasetAuthoringContext(
                () =>
                {
                    clientFactoryCalls++;
                    return OperationResult<ITrainingTeacherInferenceClient>.Fail(
                        "The test client must remain lazy.",
                        "TEST_CLIENT_MUST_REMAIN_LAZY");
                },
                identity.OpenTeacher,
                identity.GenerationPolicy,
                identity.Creator);
        var before = SnapshotRelativeFileSystemEntries(repositoryRoot);

        var result = TranslationRepositoryOwnedTrainingRunContextFactory
            .Create(spec with { CleanMissAuthoringContext = authoring });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Null(result.Value!.DpoDatasetContext.RequestIdentity);
        Assert.Same(
            authoring,
            result.Value.DpoDatasetContext.CleanMissAuthoringContext);
        Assert.Equal(0, clientFactoryCalls);
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(repositoryRoot));
        Assert.False(Directory.Exists(Path.Combine(
            repositoryRoot,
            "training-data")));
        Assert.False(Directory.Exists(Path.Combine(
            repositoryRoot,
            "training-artifacts")));
    }

    [Fact]
    public void Create_CorruptManifestFailsClosed()
    {
        var fixture = CreateSealedDataset("corrupt-manifest");
        File.WriteAllText(
            fixture.Address.ManifestPath,
            "{ not valid repository-owned manifest json }\n",
            Utf8NoBom);
        var before = SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot);

        var result = TranslationRepositoryOwnedTrainingRunContextFactory
            .Create(CreateSpec(fixture, "corrupt-manifest"));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationRepositoryOwnedTrainingRunContextFactory
                .DpoDatasetUnavailableErrorCode,
            result.ErrorCode);
        Assert.DoesNotContain(
            fixture.RepositoryRoot,
            result.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot));
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models")));
    }

    [Fact]
    public void Create_SelfHashedManifestWithInvalidRequestIdentityFailsClosed()
    {
        var fixture = CreateSealedDataset("invalid-request-identity");
        var invalidManifest = fixture.Manifest with
        {
            RequestIdentity = fixture.Manifest.RequestIdentity with
            {
                RequestIdentitySha256 = new string('0', 64)
            },
            ManifestSha256 = string.Empty
        };
        invalidManifest = invalidManifest with
        {
            ManifestSha256 = ComputeSha256(
                WriteCanonicalManifestBytes(invalidManifest))
        };
        File.WriteAllBytes(
            fixture.Address.ManifestPath,
            WriteCanonicalManifestBytes(invalidManifest));
        var before = SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot);

        var result = TranslationRepositoryOwnedTrainingRunContextFactory
            .Create(CreateSpec(fixture, "invalid-request-identity"));

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationRepositoryOwnedTrainingRunContextFactory
                .DpoDatasetUnavailableErrorCode,
            result.ErrorCode);
        Assert.DoesNotContain(
            fixture.RepositoryRoot,
            result.ErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot));
    }

    [Theory]
    [InlineData("runtime-compatibility")]
    [InlineData("cold-start")]
    [InlineData("provenance")]
    public void Create_InvalidModelPolicyFailsAfterDatasetOpenWithoutModelArtifacts(
        string invalidPart)
    {
        var fixture = CreateSealedDataset("invalid-model-" + invalidPart);
        var spec = CreateSpec(fixture, "invalid-model-" + invalidPart);
        spec = invalidPart switch
        {
            "runtime-compatibility" => spec with
            {
                RuntimeCompatibility = spec.RuntimeCompatibility with
                {
                    ConfigSha256 = new string('A', 64)
                }
            },
            "cold-start" => spec with
            {
                AllowColdStart = true,
                ColdStartReason = null
            },
            "provenance" => spec with
            {
                ProvenanceRefs = []
            },
            _ => spec
        };
        var before = SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot);

        var result = TranslationRepositoryOwnedTrainingRunContextFactory
            .Create(spec);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            TranslationRepositoryOwnedModelCheckpointContext.InvalidErrorCode,
            result.ErrorCode);
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot));
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models")));
    }

    [Fact]
    public void Create_ValidSealedDatasetDoesNotGenerateDpoOrModelOutputFiles()
    {
        var fixture = CreateSealedDataset("no-generated-artifacts");
        var before = SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot);

        var result = TranslationRepositoryOwnedTrainingRunContextFactory
            .Create(CreateSpec(fixture, "no-generated-artifacts"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(before, SnapshotRelativeFileSystemEntries(fixture.RepositoryRoot));
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            "models")));
        Assert.False(Directory.Exists(Path.Combine(
            fixture.RepositoryRoot,
            "training-artifacts",
            ".dpo-dataset-creation-leases")));
    }

    private SealedDatasetFixture CreateSealedDataset(string scope)
    {
        var repositoryRoot = CreateRepositoryOwnedFixtureRoot(scope);
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            repositoryRoot,
            "translation-domain",
            "dpo-train-evaluate",
            "v" + ComputeTextSha256(scope)[..12]);
        Assert.True(address.IsSuccess, address.ErrorMessage);
        var identity = CreateRepositoryOwnedDpoRequestIdentity(scope);
        var manifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
            address.Value!,
            identity,
            [CreateRepositoryOwnedDpoPair(address.Value!, identity, scope)]);
        Assert.True(manifest.IsSuccess, manifest.ErrorMessage);
        return new SealedDatasetFixture(
            repositoryRoot,
            address.Value!,
            manifest.Value!);
    }

    private string CreateRepositoryOwnedFixtureRoot(string scope)
    {
        var root = Path.Combine(
            _tempRoot,
            scope + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        File.WriteAllText(
            Path.Combine(root, "LlmFromScratch.sln"),
            "repository-owned training run context fixture\n",
            Utf8NoBom);
        return root;
    }

    private static TranslationRepositoryOwnedTrainingRunContextSpec CreateSpec(
        SealedDatasetFixture fixture,
        string scope)
        => CreateSpec(
            fixture.RepositoryRoot,
            fixture.Address.DomainSlug,
            fixture.Address.DesignPatternSlug,
            fixture.Address.DatasetVersion,
            "translation-policy-" + ComputeTextSha256("model-" + scope)[..12],
            "v" + ComputeTextSha256("model-version-" + scope)[..12],
            scope);

    private static TranslationRepositoryOwnedTrainingRunContextSpec CreateSpec(
        string repositoryRoot,
        string domainSlug,
        string designPatternSlug,
        string datasetVersion,
        string modelIdSlug,
        string modelVersion,
        string scope)
        => new(
            RepositoryRoot: repositoryRoot,
            DomainSlug: domainSlug,
            DesignPatternSlug: designPatternSlug,
            DatasetVersion: datasetVersion,
            ModelIdSlug: modelIdSlug,
            ModelVersion: modelVersion,
            RuntimeCompatibility:
                CreateRepositoryOwnedModelRuntimeCompatibility(scope),
            AllowColdStart: true,
            ColdStartReason:
                "first run allowed after sealed DPO dataset was opened",
            ProvenanceRefs:
            [
                "translation-context-factory-" +
                ComputeTextSha256("provenance-" + scope)[..16]
            ]);

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
                    "translation-context-factory",
                    "v1",
                    ComputeTextSha256("generation-policy-" + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "translation-context-factory",
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
            "translation-context-factory-" +
            ComputeTextSha256("teacher-request-" + scope),
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };
        var pair = new ContinuousLearning.DpoPair(
            Prompt: "Translate deterministic source text for " + scope,
            Chosen: "deterministic chosen translation for " + scope,
            Rejected: "deterministic rejected translation for " + scope,
            Domain: address.DomainSlug,
            CapturedAt: DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs:
                [
                    "sha256:" + ComputeTextSha256("governance-" + scope)
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

    private static IReadOnlyList<string> SnapshotRelativeFileSystemEntries(
        string root)
        => Directory
            .EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static void AssertRequestIdentityEqual(
        RepositoryOwnedDpoDatasetRequestIdentity expected,
        RepositoryOwnedDpoDatasetRequestIdentity? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Schema, actual!.Schema);
        Assert.Equal(
            expected.PaidReferenceAnswers.AnswerCount,
            actual.PaidReferenceAnswers.AnswerCount);
        Assert.Equal(
            expected.PaidReferenceAnswers.OrderedAnswerSha256,
            actual.PaidReferenceAnswers.OrderedAnswerSha256);
        Assert.Equal(
            expected.PaidReferenceAnswers.CanonicalListSha256,
            actual.PaidReferenceAnswers.CanonicalListSha256);
        Assert.Equal(expected.OpenTeacher, actual.OpenTeacher);
        Assert.Equal(expected.GenerationPolicy, actual.GenerationPolicy);
        Assert.Equal(expected.Creator, actual.Creator);
        Assert.Equal(
            expected.RequestIdentitySha256,
            actual.RequestIdentitySha256);
    }

    private static byte[] WriteCanonicalManifestBytes(
        RepositoryOwnedDpoDatasetManifest manifest)
    {
        var method = typeof(RepositoryOwnedDpoDatasetManifestSealer).GetMethod(
            "WriteCanonicalManifestBytes",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (byte[])method!.Invoke(null, [manifest])!;
    }

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string ComputeTextSha256(string value)
        => ComputeSha256(Utf8NoBom.GetBytes(value));

    private sealed record SealedDatasetFixture(
        string RepositoryRoot,
        RepositoryOwnedDpoDatasetAddress Address,
        RepositoryOwnedDpoDatasetManifest Manifest);
}
