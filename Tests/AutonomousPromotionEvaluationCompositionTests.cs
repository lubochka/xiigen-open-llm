// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutonomousLearning;
using ContinuousLearning;
using LoRA;
using ModelServer.AutonomousPromotion;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

[Collection(DesignConversionLocalHostTestCollectionNames.CollectionName)]
public sealed class AutonomousPromotionEvaluationCompositionTests
{
    [Fact]
    public async Task DefaultModelServerRoute_FailsClosedWithoutEvaluationConfiguration()
    {
        await using var app = ModelServer.Program.CreateLocalLoopbackHost([]);
        await app.StartAsync();
        try
        {
            using var client = CreateClient(app);
            using var httpResponse = await client.PostAsJsonAsync(
                AutonomousPromotionEvaluationEndpoint.EndpointPath,
                new AutonomousPromotionEvaluationRequest("CODE_Q"));
            var response = await httpResponse.Content
                .ReadFromJsonAsync<AutonomousPromotionEvaluationResponse>();

            Assert.Equal(HttpStatusCode.ServiceUnavailable, httpResponse.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.IsSuccess);
            Assert.False(response.PromotionReady);
            Assert.False(response.ProductReady);
            Assert.False(response.PerformsTraining);
            Assert.False(response.EvaluatorEvidenceAvailable);
            Assert.Equal(
                "AUTONOMOUS_PROMOTION_EVALUATION_NOT_CONFIGURED",
                response.ErrorCode);
            Assert.Equal("False", GetHeader(httpResponse, "x-autonomous-promotion-performs-training"));
            Assert.Equal("False", GetHeader(httpResponse, "x-autonomous-promotion-product-ready"));
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task OptInModelServerRoute_UsesRealHeldOutArtifactsWithoutMutatingThem()
    {
        using var fixture = new PromotionEvaluationFixture();
        var configurationPath = fixture.WriteConfiguration();
        var before = fixture.CaptureArtifactHashes();
        await using var app = ModelServer.Program.CreateLocalLoopbackHost(
        [
            AutonomousPromotionEvaluationConfigurationLoader.ConfigurationArgumentName,
            configurationPath,
            ModelServer.Program.ReadyFileArgumentName,
            fixture.ReadyFilePath,
        ]);
        await app.StartAsync();
        try
        {
            using var ready = JsonDocument.Parse(await File.ReadAllTextAsync(fixture.ReadyFilePath));
            Assert.True(
                ready.RootElement
                    .GetProperty("autonomousPromotionEvaluationConfigured")
                    .GetBoolean());
            Assert.True(
                ready.RootElement
                    .GetProperty("autonomousPromotionEvaluationRouteMapped")
                    .GetBoolean());
            Assert.False(
                ready.RootElement
                    .GetProperty("autonomousPromotionEvaluationPerformsTraining")
                    .GetBoolean());
            Assert.False(
                ready.RootElement
                    .GetProperty("autonomousPromotionEvaluationProductReady")
                    .GetBoolean());

            using var client = CreateClient(app);
            using var httpResponse = await client.PostAsJsonAsync(
                AutonomousPromotionEvaluationEndpoint.EndpointPath,
                new AutonomousPromotionEvaluationRequest(PromotionEvaluationFixture.Domain));
            var response = await httpResponse.Content
                .ReadFromJsonAsync<AutonomousPromotionEvaluationResponse>();

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
            Assert.NotNull(response);
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.True(response.EvaluatorEvidenceAvailable);
            Assert.False(response.ProductReady);
            Assert.False(response.PerformsTraining);
            Assert.True(response.PromotionReady);
            Assert.Equal(0.0, response.BaselineAccuracy);
            Assert.Equal(1.0, response.CandidateAccuracy);
            Assert.Equal(fixture.BaseCheckpointSha256, response.Artifacts!.BaseCheckpointSha256);
            Assert.Equal(fixture.TokenizerSha256, response.Artifacts.TokenizerSha256);
            Assert.Equal(fixture.ModelIdentity.Sha256, response.Artifacts.ModelConfigurationSha256);
            Assert.Equal(fixture.AdapterSha256, response.AdapterSha256);
            Assert.Equal(fixture.Manifest.Sha256, response.TrainingSourceManifestSha256);
            Assert.Equal(before, fixture.CaptureArtifactHashes());
            Assert.Equal("False", GetHeader(httpResponse, "x-autonomous-promotion-performs-training"));
            Assert.Equal("False", GetHeader(httpResponse, "x-autonomous-promotion-product-ready"));
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task OptInModelServerRoute_AdapterIdentityMismatchCannotAuthorizePromotion()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        var configurationPath = fixture.WriteConfiguration(
            adapterSha256Override: new string('f', 64));
        await using var app = ModelServer.Program.CreateLocalLoopbackHost(
        [
            AutonomousPromotionEvaluationConfigurationLoader.ConfigurationArgumentName,
            configurationPath,
        ]);
        await app.StartAsync();
        try
        {
            using var client = CreateClient(app);
            using var httpResponse = await client.PostAsJsonAsync(
                AutonomousPromotionEvaluationEndpoint.EndpointPath,
                new AutonomousPromotionEvaluationRequest(PromotionEvaluationFixture.Domain));
            var response = await httpResponse.Content
                .ReadFromJsonAsync<AutonomousPromotionEvaluationResponse>();

            Assert.Equal(HttpStatusCode.UnprocessableEntity, httpResponse.StatusCode);
            Assert.NotNull(response);
            Assert.False(response.IsSuccess);
            Assert.False(response.PromotionReady);
            Assert.False(response.ProductReady);
            Assert.False(response.PerformsTraining);
            Assert.False(response.EvaluatorEvidenceAvailable);
            Assert.Equal("ADAPTER_PROVENANCE_MISMATCH", response.ErrorCode);
            Assert.Equal(before, fixture.CaptureArtifactHashes());
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public void Composition_ExactTrainingSourceAndHeldOutCaseLimits_AreAccepted()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        var result = AutonomousPromotionEvaluationCompositionRoot.Create(
            fixture.CreateConfiguration(
                limits: new HeldOutEvaluationLimits(
                    MaxTrainingSources: 1,
                    MaxCases: 1)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        Assert.Equal(before, fixture.CaptureArtifactHashes());
    }

    [Fact]
    public void Composition_OverTrainingSourceLimit_FailsBeforeManifestOrArtifactAccess()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        var sourceIds = new BoundedProbeReadOnlyList<string>(
            ["not-canonical-source-1", "not-canonical-source-2"],
            maximumMoveNextCalls: 2);
        var overLimitManifest = new TrainingSourceManifest(sourceIds, "not-a-manifest-hash");
        var result = AutonomousPromotionEvaluationCompositionRoot.Create(
            fixture.CreateConfiguration(
                baseCheckpointPath: fixture.MissingArtifactPath,
                tokenizerPath: fixture.MissingArtifactPath,
                adapterRoot: fixture.MissingArtifactPath,
                plannedTrainingSources: overLimitManifest,
                limits: new HeldOutEvaluationLimits(
                    MaxTrainingSources: 1,
                    MaxCases: 1)));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("TRAINING_SOURCE_LIMIT_EXCEEDED", result.ErrorCode);
        Assert.Equal(2, sourceIds.MoveNextCalls);
        Assert.Equal(before, fixture.CaptureArtifactHashes());
    }

    [Fact]
    public void Composition_OverHeldOutCaseLimit_FailsBeforeEvaluatorOrArtifactAccess()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        var cases = new BoundedProbeReadOnlyList<HeldOutEvaluationCase>(
            [fixture.HeldOutCase, fixture.HeldOutCase with { CaseId = "second-case" }],
            maximumMoveNextCalls: 2);
        var result = AutonomousPromotionEvaluationCompositionRoot.Create(
            fixture.CreateConfiguration(
                baseCheckpointPath: fixture.MissingArtifactPath,
                tokenizerPath: fixture.MissingArtifactPath,
                adapterRoot: fixture.MissingArtifactPath,
                heldOutCases: cases,
                limits: new HeldOutEvaluationLimits(
                    MaxTrainingSources: 1,
                    MaxCases: 1)));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("HELD_OUT_CASE_LIMIT_EXCEEDED", result.ErrorCode);
        Assert.Equal(2, cases.MoveNextCalls);
        Assert.Equal(before, fixture.CaptureArtifactHashes());
    }

    [Fact]
    public async Task KnownLengthOversizedRequest_Returns413WithoutArtifactMutation()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        await using var app = ModelServer.Program.CreateLocalLoopbackHost(
        [
            AutonomousPromotionEvaluationConfigurationLoader.ConfigurationArgumentName,
            fixture.WriteConfiguration(),
        ]);
        await app.StartAsync();
        try
        {
            using var client = CreateClient(app);
            using var content = new ByteArrayContent(
                new byte[AutonomousPromotionEvaluationEndpoint.MaximumRequestBytes + 1]);
            content.Headers.ContentType = new("application/json");
            using var httpResponse = await client.PostAsync(
                AutonomousPromotionEvaluationEndpoint.EndpointPath,
                content);

            await AssertTooLargeResponseAsync(httpResponse);
            Assert.Equal(before, fixture.CaptureArtifactHashes());
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public async Task ChunkedOversizedRequest_Returns413WithoutArtifactMutation()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        await using var app = ModelServer.Program.CreateLocalLoopbackHost(
        [
            AutonomousPromotionEvaluationConfigurationLoader.ConfigurationArgumentName,
            fixture.WriteConfiguration(),
        ]);
        await app.StartAsync();
        try
        {
            using var client = CreateClient(app);
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                AutonomousPromotionEvaluationEndpoint.EndpointPath)
            {
                Content = new UnknownLengthContent(
                    new byte[AutonomousPromotionEvaluationEndpoint.MaximumRequestBytes + 1]),
            };
            request.Headers.TransferEncodingChunked = true;
            Assert.Null(request.Content.Headers.ContentLength);
            Assert.True(request.Headers.TransferEncodingChunked);
            using var httpResponse = await client.SendAsync(request);

            await AssertTooLargeResponseAsync(httpResponse);
            Assert.Equal(before, fixture.CaptureArtifactHashes());
        }
        finally
        {
            await app.StopAsync();
        }
    }

    [Fact]
    public void ConfigurationLoader_CliOverridesEnvironmentAndIgnoresUnrelatedHostArguments()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        var configurationPath = fixture.WriteConfiguration();
        var endpoint = AutonomousPromotionEvaluationConfigurationLoader.ResolveEndpoint(
        [
            "--urls",
            "http://0.0.0.0:54321",
            AutonomousPromotionEvaluationConfigurationLoader.ConfigurationArgumentName,
            configurationPath,
            ModelServer.Program.ReadyFileArgumentName,
            fixture.ReadyFilePath,
        ],
        name => StringComparer.Ordinal.Equals(
            name,
            AutonomousPromotionEvaluationConfigurationLoader.ConfigurationEnvironmentName)
                ? fixture.MissingArtifactPath
                : null);

        Assert.True(endpoint.IsConfigured);
        Assert.Equal(before, fixture.CaptureArtifactHashes());
    }

    [Fact]
    public void ConfigurationLoader_DuplicateCliOption_IsRejected()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        var configurationPath = fixture.WriteConfiguration();

        var exception = Assert.Throws<ArgumentException>(() =>
            AutonomousPromotionEvaluationConfigurationLoader.ResolveEndpoint(
            [
                AutonomousPromotionEvaluationConfigurationLoader.ConfigurationArgumentName,
                configurationPath,
                AutonomousPromotionEvaluationConfigurationLoader.ConfigurationArgumentName
                    + "=" + configurationPath,
            ],
            _ => null));

        Assert.Contains("more than once", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, fixture.CaptureArtifactHashes());
    }

    [Fact]
    public void Composition_AdapterOutsideConfiguredRoot_FailsClosedWithoutMutation()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        var allowedRoot = Path.GetFullPath(Path.Combine(fixture.RootDirectory, "allowed-adapters"));
        Directory.CreateDirectory(allowedRoot);

        var result = AutonomousPromotionEvaluationCompositionRoot.Create(
            fixture.CreateConfiguration(adapterRoot: allowedRoot));

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_PATH_OUTSIDE_ROOT", result.ErrorCode);
        Assert.Equal(before, fixture.CaptureArtifactHashes());
    }

    [Fact]
    public void Composition_NonCanonicalAdapterPath_FailsClosedWithoutMutation()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        var nonCanonicalAdapterPath = Path.Combine(
            fixture.RootDirectory,
            ".",
            Path.GetFileName(fixture.AdapterPath));

        var result = AutonomousPromotionEvaluationCompositionRoot.Create(
            fixture.CreateConfiguration(adapterPath: nonCanonicalAdapterPath));

        Assert.False(result.IsSuccess);
        Assert.Equal("AUTONOMOUS_PROMOTION_CONFIGURATION_INVALID", result.ErrorCode);
        Assert.Equal(before, fixture.CaptureArtifactHashes());
    }

    [Fact]
    public void Composition_ReparseAdapterRoot_FailsClosedWithoutMutation()
    {
        using var fixture = new PromotionEvaluationFixture();
        var before = fixture.CaptureArtifactHashes();
        var realRoot = Path.GetFullPath(Path.Combine(fixture.RootDirectory, "real-adapter-root"));
        var linkedRoot = Path.GetFullPath(Path.Combine(fixture.RootDirectory, "linked-adapter-root"));
        Directory.CreateDirectory(realRoot);
        File.Copy(fixture.AdapterPath, Path.Combine(realRoot, Path.GetFileName(fixture.AdapterPath)));
        CreateDirectoryReparseLink(linkedRoot, realRoot);
        try
        {
            var result = AutonomousPromotionEvaluationCompositionRoot.Create(
                fixture.CreateConfiguration(
                    adapterRoot: linkedRoot,
                    adapterPath: Path.Combine(linkedRoot, Path.GetFileName(fixture.AdapterPath))));

            Assert.False(result.IsSuccess);
            Assert.Equal("AUTONOMOUS_PROMOTION_CONFIGURATION_INVALID", result.ErrorCode);
            Assert.Equal(before, fixture.CaptureArtifactHashes());
        }
        finally
        {
            if (Directory.Exists(linkedRoot))
                Directory.Delete(linkedRoot);
        }
    }

    private static HttpClient CreateClient(Microsoft.AspNetCore.Builder.WebApplication app)
        => new()
        {
            BaseAddress = ModelServer.Program.ResolveStartedLoopbackAddress(app),
            Timeout = TimeSpan.FromSeconds(30),
        };

    private static string GetHeader(HttpResponseMessage response, string name)
        => Assert.Single(response.Headers.GetValues(name));

    private static async Task AssertTooLargeResponseAsync(HttpResponseMessage httpResponse)
    {
        var response = await httpResponse.Content
            .ReadFromJsonAsync<AutonomousPromotionEvaluationResponse>();
        Assert.Equal((HttpStatusCode)413, httpResponse.StatusCode);
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.False(response.PromotionReady);
        Assert.False(response.ProductReady);
        Assert.False(response.PerformsTraining);
        Assert.False(response.EvaluatorEvidenceAvailable);
        Assert.Equal("AUTONOMOUS_PROMOTION_REQUEST_TOO_LARGE", response.ErrorCode);
        Assert.Equal("False", GetHeader(httpResponse, "x-autonomous-promotion-promotion-ready"));
        Assert.Equal("False", GetHeader(httpResponse, "x-autonomous-promotion-product-ready"));
        Assert.Equal("False", GetHeader(httpResponse, "x-autonomous-promotion-performs-training"));
    }

    private static void CreateDirectoryReparseLink(string linkPath, string targetPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
            return;
        }

        using var process = System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                ArgumentList =
                {
                    "/d",
                    "/c",
                    "mklink",
                    "/J",
                    linkPath,
                    targetPath,
                },
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }) ?? throw new InvalidOperationException("Could not start the junction creation process.");
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
        Assert.True(
            (File.GetAttributes(linkPath) & FileAttributes.ReparsePoint) != 0,
            "The created adapter-root junction must be observable as a reparse point.");
    }

    private sealed class PromotionEvaluationFixture : IDisposable
    {
        public const string Domain = "CODE_Q";

        public string RootDirectory { get; }
        public string BaseCheckpointPath { get; }
        public string TokenizerPath { get; }
        public string AdapterPath { get; }
        public string ReadyFilePath { get; }
        public string MissingArtifactPath { get; }
        public ModelConfig ModelConfig { get; } = new()
        {
            VocabSize = 256,
            ContextLen = 32,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 2,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
        };
        public LoraConfig LoraConfig { get; } = new()
        {
            Rank = 1,
            Alpha = 1,
            Dropout = 0,
            TargetModules = ["q_proj"],
        };
        public TrainingSourceManifest Manifest { get; }
        public string BaseCheckpointSha256 { get; }
        public string TokenizerSha256 { get; }
        public string AdapterSha256 { get; }
        public ModelConfigurationIdentity ModelIdentity { get; }
        public HeldOutEvaluationCase HeldOutCase { get; }

        public PromotionEvaluationFixture()
        {
            RootDirectory = Path.GetFullPath(Path.Combine(
                Path.GetTempPath(),
                $"autonomous-promotion-evaluation-{Guid.NewGuid():N}"));
            Directory.CreateDirectory(RootDirectory);
            BaseCheckpointPath = Path.GetFullPath(Path.Combine(RootDirectory, "base.bin"));
            TokenizerPath = Path.GetFullPath(Path.Combine(RootDirectory, "tokenizer.json"));
            AdapterPath = Path.GetFullPath(Path.Combine(RootDirectory, "candidate.lora"));
            ReadyFilePath = Path.GetFullPath(Path.Combine(RootDirectory, "modelserver-ready.json"));
            MissingArtifactPath = Path.GetFullPath(Path.Combine(RootDirectory, "missing-artifact"));

            new LlmModel(ModelConfig).SaveCheckpoint(BaseCheckpointPath);
            var tokenizer = new BpeTokenizer();
            tokenizer.Train("held out tokenizer corpus prompt response", 256);
            tokenizer.Save(TokenizerPath);

            var canonicalLora = LoraApplicator.ValidateAndCanonicalize(LoraConfig);
            HeldOutEvaluationCase? separatingCase = null;
            foreach (var (initial, increment) in new[]
                     {
                         (0.125f, 0.0005f),
                         (-0.25f, 0.001f),
                         (0.75f, -0.0015f),
                         (-1.25f, 0.002f),
                     })
            {
                WriteAdapter(canonicalLora, initial, increment);
                separatingCase = FindCaseWhereAdapterChangesPrediction(canonicalLora);
                if (separatingCase is not null)
                    break;
            }
            HeldOutCase = separatingCase
                ?? throw new InvalidOperationException(
                    "The production-composition test fixture could not create a prediction-separating adapter.");

            Manifest = TrainingSourceProvenance.BuildManifest(
            [
                TrainingSourceProvenance.CreateSourceId(
                    "training/source",
                    "private training bytes"),
            ]).Value!;
            BaseCheckpointSha256 = Hash(BaseCheckpointPath);
            TokenizerSha256 = Hash(TokenizerPath);
            AdapterSha256 = Hash(AdapterPath);
            ModelIdentity = TrainingSourceProvenance.Identify(ModelConfig);
        }

        public string WriteConfiguration(string? adapterSha256Override = null)
        {
            var configuration = CreateConfiguration(
                adapterSha256Override: adapterSha256Override);
            var path = Path.GetFullPath(Path.Combine(RootDirectory, "evaluation-config.json"));
            File.WriteAllText(path, JsonSerializer.Serialize(configuration));
            return path;
        }

        public AutonomousPromotionEvaluationConfiguration CreateConfiguration(
            string? baseCheckpointPath = null,
            string? tokenizerPath = null,
            string? adapterRoot = null,
            string? adapterPath = null,
            string? adapterSha256Override = null,
            TrainingSourceManifest? plannedTrainingSources = null,
            IReadOnlyList<HeldOutEvaluationCase>? heldOutCases = null,
            HeldOutEvaluationLimits? limits = null)
        {
            var canonicalLora = LoraApplicator.ValidateAndCanonicalize(LoraConfig);
            var appliedManifest = plannedTrainingSources ?? Manifest;
            var provenance = new ModelAdapterProvenance(
                appliedManifest,
                adapterSha256Override ?? AdapterSha256,
                BaseCheckpointSha256,
                TokenizerSha256,
                ModelIdentity,
                TrainingSourceProvenance.Identify(canonicalLora));
            var candidate = new ModelAdapter(
                Domain,
                adapterPath ?? AdapterPath,
                appliedManifest.SourceIds.Count,
                DateTimeOffset.UtcNow,
                provenance);
            return new AutonomousPromotionEvaluationConfiguration(
                ModelConfig,
                baseCheckpointPath ?? BaseCheckpointPath,
                tokenizerPath ?? TokenizerPath,
                adapterRoot ?? RootDirectory,
                canonicalLora,
                [
                    new AutonomousPromotionEvaluationTargetConfiguration(
                        Domain,
                        candidate,
                        appliedManifest,
                        heldOutCases ?? [HeldOutCase]),
                ],
                limits);
        }

        public string[] CaptureArtifactHashes()
            => [Hash(BaseCheckpointPath), Hash(TokenizerPath), Hash(AdapterPath)];

        public void Dispose()
        {
            if (Directory.Exists(RootDirectory))
                Directory.Delete(RootDirectory, recursive: true);
        }

        private static string Hash(string path)
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);
            return TrainingSourceProvenance.Sha256(stream);
        }

        private void WriteAdapter(
            LoraConfig canonicalLora,
            float initialAdapterValue,
            float adapterValueIncrement)
        {
            var candidate = new LlmModel(ModelConfig);
            candidate.LoadCheckpoint(BaseCheckpointPath);
            var adapters = LoraApplicator.Apply(candidate, canonicalLora);
            var value = initialAdapterValue;
            foreach (var (_, parameter) in adapters.SelectMany(
                         static adapter => adapter.AdapterParameters))
            {
                for (var index = 0; index < parameter.Value.Data.Length; index++)
                {
                    parameter.Value.Data[index] = value;
                    value += adapterValueIncrement;
                }
            }
            candidate.SaveAdapterCheckpoint(AdapterPath);
        }

        private HeldOutEvaluationCase? FindCaseWhereAdapterChangesPrediction(
            LoraConfig canonicalLora)
        {
            var tokenizer = BpeTokenizer.Load(TokenizerPath);
            var baseline = new LlmModel(ModelConfig);
            baseline.LoadCheckpoint(BaseCheckpointPath);
            var candidate = new LlmModel(ModelConfig);
            candidate.LoadCheckpoint(BaseCheckpointPath);
            LoraApplicator.Apply(candidate, canonicalLora);
            candidate.LoadAdapterCheckpoint(AdapterPath);

            for (var character = 33; character <= 126; character++)
            {
                var prompt = ((char)character).ToString();
                var prefix = tokenizer.Encode(string.Concat(prompt, "\n"));
                var baselinePrediction = PredictNext(baseline, prefix);
                var candidatePrediction = PredictNext(candidate, prefix);
                if (candidatePrediction is < 33 or > 126
                    || candidatePrediction == baselinePrediction)
                {
                    continue;
                }

                return new HeldOutEvaluationCase(
                    "adapter-forward-case",
                    TrainingSourceProvenance.CreateSourceId(
                        "held/source",
                        "source-disjoint held bytes"),
                    prompt,
                    ((char)candidatePrediction).ToString());
            }

            return null;
        }

        private static int PredictNext(LlmModel model, IReadOnlyList<int> prefix)
        {
            var input = new Tensor(
                prefix.Select(static token => (float)token).ToArray(),
                new TensorShape(1, prefix.Count));
            var logits = model.Forward(input, training: false);
            var offset = (prefix.Count - 1) * model.Config.VocabSize;
            var best = 0;
            for (var token = 1; token < model.Config.VocabSize; token++)
            {
                if (logits.Data[offset + token] > logits.Data[offset + best])
                    best = token;
            }
            return best;
        }
    }

    private sealed class UnknownLengthContent(byte[] payload) : HttpContent
    {
        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
            => stream.WriteAsync(payload).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private sealed class BoundedProbeReadOnlyList<T>(
        IReadOnlyList<T> values,
        int maximumMoveNextCalls) : IReadOnlyList<T>
    {
        public int MoveNextCalls { get; private set; }
        public int Count => values.Count;
        public T this[int index] => values[index];

        public IEnumerator<T> GetEnumerator()
        {
            for (var index = 0; index < values.Count; index++)
            {
                RecordMoveNext();
                yield return values[index];
            }
            RecordMoveNext();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();

        private void RecordMoveNext()
        {
            MoveNextCalls++;
            if (MoveNextCalls > maximumMoveNextCalls)
            {
                throw new InvalidOperationException(
                    "The bounded sequence was enumerated beyond the configured limit plus one.");
            }
        }
    }
}
