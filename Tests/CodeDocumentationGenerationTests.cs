using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using AgentInfrastructure;
using ContinuousLearning;
using Inference;
using LoRA;
using ModelServer;
using Tokenizer;
using TrainableRouter.Training;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

// Synthetic saved-state integration, not a learning-quality experiment.
public sealed class CodeDocumentationGenerationTests
{
    [Fact]
    public void InitialControls_RefuseExistingCandidateBeforeLockedPayloadsOrSnapshotEffects()
    {
        using var fixture = new Fixture(); // Existing synthetic fixture is engineering evidence, never the live Student parent.
        var request = fixture.Start with { ColdStartPolicy = RepositoryOwnedIncrementalTrainingColdStartPolicy.Authorize("explicit synthetic guard probe"),
            BaseInitializationSeed = 20261014, LoraConfig = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
            AdapterSeedAuthority = new LoraPairRandomSeedAuthority(20261014, "guard-policy", "guard-reference") };
        const string relative = ".kiro/xiigen-slices/work/results/RP01.03A-initial-control-binding-20261014/private/initial.weights.bin";
        var snapshotPath = Path.Combine(fixture.Root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetDirectoryName(snapshotPath))!);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Path.GetDirectoryName(snapshotPath)!);
        var manifestPath = Path.Combine(fixture.Root, fixture.Manifest.RepositoryRelativeManifestPath);
        var dpoPath = Path.Combine(fixture.Root, fixture.Manifest.DpoDatasetManifest.RepositoryRelativeManifestPath);
        var payloadPaths = new[] { manifestPath, dpoPath, fixture.WeightsPath };
        var payloadHashes = payloadPaths.Select(path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))).ToArray();
        var locks = payloadPaths.Select(path => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)).ToArray();
        var priorObserver = RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting;
        var custodyPreparations = 0;
        RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting = _ => custodyPreparations++;
        try
        {
            foreach (var path in payloadPaths) // On this native host each accepted payload is demonstrably unreadable by another opener.
                Assert.Throws<IOException>(() => { using var forbidden = File.OpenRead(path); });
            var error = RepositoryOwnedModelCheckpointLoader.InitialControlCheckpointPresentErrorCode;
            var owner = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(request, initialOnly: true);
            Assert.False(owner.IsSuccess); Assert.Equal(error, owner.ErrorCode); Assert.Null(owner.Value);
            var generation = new CodeDocumentationGenerationService(new(request, fixture.Tokenizer, new(4, 0, 0, 1, 7, 1024)));
            var input = new CodeDocumentationGenerationInput("guard-only", request.DomainSlug, "fixture input");
            AssertRefusal(generation.GenerateInitialControl(input));
            Assert.False(File.Exists(snapshotPath)); // CreateNew must not precede START admission.
            var retainedBytes = Encoding.UTF8.GetBytes("synthetic retained snapshot sentinel");
            using (var snapshot = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(snapshotPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.None)) snapshot.Write(retainedBytes);
            AssertRefusal(generation.GenerateRetainedInitialControl(input));
            Assert.Equal(retainedBytes, File.ReadAllBytes(snapshotPath));
            Assert.Equal(0, custodyPreparations); // Both controls stop before custody traversal and every accepted payload owner.
            var ordinary = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(request.RepositoryRoot, request.DomainSlug,
                request.DesignPatternSlug, request.ModelIdSlug, request.ModelVersion, request.RuntimeCompatibility);
            Assert.False(ordinary.IsSuccess); Assert.NotEqual(error, ordinary.ErrorCode); // Legacy default still takes its ordinary strict-reader branch.
            Console.WriteLine($"initial START and BOTH controls: {error}; custody preparations before ordinary default=0; no prediction; new snapshot absent; retained sentinel unchanged; accepted payload open locks=3");

            void AssertRefusal(OperationResult<CodeDocumentationInitialControlReceipt> result)
            {
                Assert.True(result.IsSuccess, result.ErrorMessage); Assert.NotNull(result.Value);
                Assert.Equal(error, result.Value.Generation.ErrorCode); Assert.Null(result.Value.Generation.Text);
                Assert.Null(result.Value.Generation.Source); Assert.Null(result.Value.Generation.GeneratedTokenCount);
                Assert.Null(result.Value.Before); Assert.Null(result.Value.After);
                Assert.Equal(0, result.Value.Generation.GenerationMilliseconds);
            }
        }
        finally
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting = priorObserver;
            foreach (var stream in locks) stream.Dispose();
        }
        Assert.Equal(payloadHashes, payloadPaths.Select(path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))).ToArray());
    }

    [Fact]
    public async Task OrdinaryColdRequest_ReturnsActualTextFromRepositoryPublishedCheckpointAndExactInput()
    {
        using var fixture = new Fixture();
        await using var app = Program.CreateLocalLoopbackHostCore([], fixture.ReadEnvironment, fixture.Root);
        app.Urls.Clear(); app.Urls.Add("http://127.0.0.1:0");
        await app.StartAsync();
        using var client = new HttpClient { BaseAddress = Program.ResolveStartedLoopbackAddress(app) };
        var x = "  C#\tπ\r\n";
        var input = new CodeDocumentationGenerationInput("ordinary-1", fixture.Start.DomainSlug, x);
        using var response = await client.PostAsJsonAsync(CodeDocumentationGenerationEndpoint.EndpointPath, input);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = (await response.Content.ReadFromJsonAsync<CodeDocumentationGenerationResult>())!;
        var expected = new TextGenerator(fixture.Model, fixture.Tokenizer).GenerateDetailed(x, 4, 0, 0, 1, 7, null, CancellationToken.None);
        Assert.Equal(expected.Text, result.Text);
        Assert.Equal(input.RequestId, result.RequestId);
        Assert.Equal(CodeDocumentationStudentGeneration.InputSha256(x), result.InputSha256);
        Assert.Equal("repository_published_checkpoint", result.Source!.SourceKind);
        Assert.Equal(fixture.Manifest.ManifestSha256, result.Source.ManifestSha256);
        Assert.Equal(fixture.Manifest.WeightsSha256, result.Source.WeightsSha256);
        Assert.Equal(fixture.Tokenizer.ComputeStateSha256(), result.TokenizerSha256);
        Assert.Equal(fixture.Tokenizer.Encode(x).Count, result.PromptTokenCount);
        Assert.Equal(4, result.GeneratedTokenCount); Assert.Equal("LENGTH_LIMIT", result.StopReason);
        Assert.Null(result.ErrorCode); Assert.True(result.ElapsedMilliseconds >= result.GenerationMilliseconds);
        Assert.Equal(CodeDocumentationStudentGeneration.FramingVersion, result.FramingVersion);
        // Internal facade parity; this does not mint an assessment-owner receipt.
        var privateReceipt = CodeDocumentationStudentGeneration.GeneratePrivate(fixture.Model, fixture.Tokenizer, x,
            new(4, 0, 0, 1, 7, 1, 1024), CancellationToken.None);
        Assert.Equal(result.Text, privateReceipt.Text); Assert.Equal(result.GeneratedTokenCount, privateReceipt.GeneratedTokenCount);
        await app.StopAsync();
    }

    [Fact]
    public async Task CorruptPublishedSourceAndClientPrivateSelector_AreTypedRefusals()
    {
        using var fixture = new Fixture();
        await using var app = Program.CreateLocalLoopbackHostCore([], fixture.ReadEnvironment, fixture.Root);
        app.Urls.Clear(); app.Urls.Add("http://127.0.0.1:0"); await app.StartAsync();
        using var client = new HttpClient { BaseAddress = Program.ResolveStartedLoopbackAddress(app) };
        using var privateSelector = await client.PostAsync(CodeDocumentationGenerationEndpoint.EndpointPath,
            new StringContent("{\"requestId\":\"bad\",\"domainSlug\":\"csharp-code-documentation\",\"ownerInput\":\"x\",\"trialPath\":\"private/weights.bin\"}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, privateSelector.StatusCode);
        Assert.Contains(CodeDocumentationStudentGeneration.InvalidErrorCode, await privateSelector.Content.ReadAsStringAsync());
        var bytes = File.ReadAllBytes(fixture.WeightsPath); bytes[^1] ^= 1; File.WriteAllBytes(fixture.WeightsPath, bytes);
        using var corrupt = await client.PostAsJsonAsync(CodeDocumentationGenerationEndpoint.EndpointPath,
            new CodeDocumentationGenerationInput("corrupt", fixture.Start.DomainSlug, "x"));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, corrupt.StatusCode);
        var result = (await corrupt.Content.ReadFromJsonAsync<CodeDocumentationGenerationResult>())!;
        Assert.NotNull(result.ErrorCode); Assert.Equal("ERROR", result.StopReason);
        Assert.Null(result.Text); Assert.Null(result.Source); Assert.Null(result.GeneratedTokenCount);
        Assert.Equal(bytes, File.ReadAllBytes(fixture.WeightsPath)); // No fresh fallback or checkpoint rewrite.
        await app.StopAsync();

        await using var disabled = Program.CreateLocalLoopbackHostCore([], _ => null, fixture.Root);
        disabled.Urls.Clear(); disabled.Urls.Add("http://127.0.0.1:0"); await disabled.StartAsync();
        using var disabledClient = new HttpClient { BaseAddress = Program.ResolveStartedLoopbackAddress(disabled) };
        using var disabledResponse = await disabledClient.PostAsJsonAsync(CodeDocumentationGenerationEndpoint.EndpointPath,
            new CodeDocumentationGenerationInput("disabled", fixture.Start.DomainSlug, "x"));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, disabledResponse.StatusCode);
        Assert.Contains(CodeDocumentationGenerationConfiguration.NotConfigured, await disabledResponse.Content.ReadAsStringAsync());
        await disabled.StopAsync();

        // A server artifact is also strict: private selectors cannot become configuration authority.
        fixture.WriteConfiguration(Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(fixture.ConfigBytes)[..^1] + ",\"trialPath\":\"private/weights.bin\"}"));
        await using var invalid = Program.CreateLocalLoopbackHostCore([], fixture.ReadEnvironment, fixture.Root);
        invalid.Urls.Clear(); invalid.Urls.Add("http://127.0.0.1:0"); await invalid.StartAsync();
        using var invalidClient = new HttpClient { BaseAddress = Program.ResolveStartedLoopbackAddress(invalid) };
        using var invalidResponse = await invalidClient.PostAsJsonAsync(CodeDocumentationGenerationEndpoint.EndpointPath,
            new CodeDocumentationGenerationInput("invalid-config", fixture.Start.DomainSlug, "x"));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, invalidResponse.StatusCode);
        Assert.Contains(CodeDocumentationGenerationConfiguration.Invalid, await invalidResponse.Content.ReadAsStringAsync());
        Assert.Equal(bytes, File.ReadAllBytes(fixture.WeightsPath));
        await invalid.StopAsync();
    }

    private sealed class Fixture : IDisposable
    {
        internal string Root { get; } = Path.Combine(Path.GetTempPath(), "ordinary-code-doc-" + Guid.NewGuid().ToString("N"));
        internal BpeTokenizer Tokenizer { get; } = new();
        internal LlmModel Model { get; }
        internal CodeDocumentationRepositoryOwnedModelCheckpointStartRequest Start { get; }
        internal RepositoryOwnedModelCheckpointManifest Manifest { get; }
        internal string WeightsPath { get; }
        internal byte[] ConfigBytes { get; private set; } = [];
        private string ConfigHash = string.Empty;
        internal string? ReadEnvironment(string name) => name switch
        {
            CodeDocumentationGenerationConfiguration.EnabledEnvironmentName => "true",
            CodeDocumentationGenerationConfiguration.HashEnvironmentName => ConfigHash,
            _ => null
        };
        internal Fixture()
        {
            Directory.CreateDirectory(Root); Directory.CreateDirectory(Path.Combine(Root, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Root, "11.DPO")); File.WriteAllText(Path.Combine(Root, "LlmFromScratch.sln"), "");
            Tokenizer.Train("fixture", 256);
            var config = new ModelConfig { VocabSize = Tokenizer.VocabSize, ContextLen = 64, HiddenDim = 8,
                NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 16 };
            Model = new(config);
            foreach (var parameter in Model.Parameters())
                for (var i = 0; i < parameter.Value.Data.Length; i++) parameter.Value.Data[i] = 0.02f * (i % 7 + 1);
            var hash = new string('a', 64);
            var compatibility = new RepositoryOwnedModelRuntimeCompatibility(TrainingSourceProvenance.Identify(config).Sha256,
                hash, Tokenizer.ComputeStateSha256(), hash, hash, "ordinary-generation-fixture");
            var lesson = CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinatorTests.SealRepositoryOwnedDpo(
                Root, "ordinary-generation", "v1", "x", "yes", "no");
            Start = new(Root, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainSlug,
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.DesignPatternSlug, "code-documentation-llm", "v1",
                config, compatibility, RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied);
            var address = RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(Root, Start.DomainSlug,
                Start.DesignPatternSlug, Start.ModelIdSlug, Start.ModelVersion, 1).Value!;
            var sealedModel = RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(address, Model.SaveCheckpointToStream,
                new(null, "synthetic initial state", lesson.Identity, compatibility, DateTimeOffset.UnixEpoch, ["test:ordinary-generation"]));
            Assert.True(sealedModel.IsSuccess, sealedModel.ErrorMessage); Manifest = sealedModel.Value!;
            WeightsPath = address.WeightsPath;
            var directory = Path.Combine(Root, "config", "code-documentation");
            Directory.CreateDirectory(Path.Combine(Root, "config"));
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(directory);
            using (var tokenizerFile = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(Path.Combine(directory, "tokenizer.json"),
                FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.None))
                Tokenizer.Save(tokenizerFile);
            WriteConfiguration(JsonSerializer.SerializeToUtf8Bytes(new
            {
                version = CodeDocumentationGenerationConfiguration.Version,
                domainSlug = Start.DomainSlug, designPatternSlug = Start.DesignPatternSlug,
                modelIdSlug = Start.ModelIdSlug, modelVersion = Start.ModelVersion,
                // All nine writable fields; no computed HeadDim/KvGroupSize are serialized.
                modelConfig = new { config.VocabSize, config.ContextLen, config.HiddenDim, config.NumLayers,
                    config.NumHeads, config.NumKvHeads, config.FfnHiddenDim, config.RopeTheta, config.RmsNormEps },
                runtimeCompatibility = compatibility,
                settings = new CodeDocumentationStudentGenerationSettings(4, 0, 0, 1, 7, 1024)
            }, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }
        internal void WriteConfiguration(byte[] bytes)
        {
            var path = Path.Combine(Root, "config", "code-documentation", "generation-v1.json");
            using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path,
                File.Exists(path) ? FileMode.Open : FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.None);
            stream.SetLength(0); stream.Write(bytes); stream.Flush();
            ConfigBytes = bytes;
            ConfigHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        }
        public void Dispose() { if (Directory.Exists(Root)) Directory.Delete(Root, true); }
    }
}
