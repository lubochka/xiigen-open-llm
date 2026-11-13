// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace LlmFromScratch.Tests;

using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;
using TrainableRouter;
using TrainableRouter.Training;

public sealed class LocalQwenCpuAuthoringBindingTests
{
    [Fact]
    public async Task SelectedCpuProfile_BindsFullBodyDeadlineAndConsumer_RefusesDriftWithoutChangingLegacy()
    {
        // One asset-free deterministic property; no native tokenizer, Ollama daemon, training or exam material is used.
        var cpu = LocalQwenOllamaInferenceProfileAuthority.CpuAuthoring;
        Assert.Equal("c1973891d1414f24b8625668446f5b33ffddeb433acb9b15daedc00d52d8aea5", cpu.ConfigurationSha256);
        Assert.Equal("b6b088d7fbee1bece702df1448e309b8a88335178a5bc84b2376bd68699453bf", LocalQwenOllamaInferenceProfileAuthority.Authoring.ConfigurationSha256);
        Assert.Equal("00be8f07e2f5d5d6e4a8c3a389a643dbcd4a1b37af4112432c9872ffd137b1cc", LocalQwenOllamaInferenceProfileAuthority.Evaluation.ConfigurationSha256);
        Assert.False(LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(cpu));
        Assert.Equal(LocalQwenOllamaInferenceProfileAuthority.Authoring, ReadProfile(null, null).Value);
        Assert.Equal(cpu, ReadProfile(cpu.ProfileId, cpu.ConfigurationSha256).Value);
        foreach (var pair in new[] { ("unknown", cpu.ConfigurationSha256), (cpu.ProfileId, new string('0', 64)), (cpu.ProfileId, (string?)null), ((string?)null, cpu.ConfigurationSha256) })
            Assert.False(ReadProfile(pair.Item1, pair.Item2).IsSuccess);
        Assert.False(Configuration(cpu with { ConfigurationSha256 = new string('0', 64) }).IsSuccess);

        var configured = Configuration(cpu).Value!;
        var legacy = Configuration(null).Value!;
        Assert.Equal(LocalQwenOllamaInferenceProfileAuthority.Authoring, legacy.AuthoringProfile);
        const string prompt = "{\"task\":\"Explain a lawful synthetic C# null check\"}";
        var inputTokens = 1536;
        string? counted = null;
        OperationResult<int[]> Count(string rendered, CancellationToken token) { token.ThrowIfCancellationRequested(); counted = rendered; return OperationResult<int[]>.Ok(new int[inputTokens]); }
        var prepared = LocalQwenOllamaTeacherInferenceClient.PreparedAuthoringRequest.Prepare(prompt, new string('a', 64), "SyntheticTest", configured, null, Count, CancellationToken.None).Value!;
        Assert.Equal("SyntheticTest", prepared.TokenizerOrigin);
        Assert.Equal(cpu, prepared.Profile);
        Assert.Equal(512, prepared.OutputReserve);
        Assert.Equal(2048, prepared.InputTokens + prepared.OutputReserve);
        Assert.False(prepared.Matches(new string('a', 64), legacy));
        Assert.True(prepared.Matches(new string('a', 64), configured));
        var handler = new Handler(unloaded: true);
        TimeSpan? deadline = null;
        using var client = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(configured, handler, true,
            deadlineFactory: t => { deadline = t; return new CancellationTokenSource(); }, syntheticCpuTokenCountForTesting: Count).Value!;
        using var owned = ContinuousLearningTp1AuthoringComposition.OwnForTesting(client);
        Assert.Equal(cpu, owned.AuthoringProfile);
        var permit = new Permit();
        var result = await owned.InferAsync(prompt, permit);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(cpu, result.Value!.InferenceProfile);
        Assert.Equal(TimeSpan.FromSeconds(600), deadline);
        Assert.True(permit.CallIssued);
        Assert.Equal(6, handler.Calls);
        Assert.Equal(prepared.CopyBody(), Encoding.UTF8.GetBytes(handler.ChatBody!));
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.RenderRestrictedSingleUser(prompt), counted);
        using (var document = JsonDocument.Parse(handler.ChatBody!))
        {
            var r = document.RootElement; var o = r.GetProperty("options");
            Assert.Equal(prompt, r.GetProperty("messages")[0].GetProperty("content").GetString());
            Assert.False(r.GetProperty("stream").GetBoolean()); Assert.False(r.GetProperty("think").GetBoolean());
            Assert.False(r.GetProperty("truncate").GetBoolean()); Assert.False(r.GetProperty("shift").GetBoolean());
            Assert.Equal(0, r.GetProperty("keep_alive").GetInt32());
            Assert.Equal(2048, o.GetProperty("num_ctx").GetInt32()); Assert.Equal(512, o.GetProperty("num_predict").GetInt32());
            Assert.Equal(2, o.GetProperty("num_thread").GetInt32()); Assert.Equal(16, o.GetProperty("num_batch").GetInt32()); Assert.Equal(0, o.GetProperty("num_gpu").GetInt32());
            Assert.Equal(0, o.GetProperty("temperature").GetDouble()); Assert.Equal(0, o.GetProperty("seed").GetInt32());
        }
        inputTokens++;
        var overflowPermit = new Permit(); var overflow = await owned.InferAsync(prompt, overflowPermit);
        Assert.Equal(ConfiguredQwenVocabularyLease.BudgetExceeded, overflow.ErrorCode);
        Assert.False(overflowPermit.CallIssued); Assert.Equal(6, handler.Calls);
        Assert.False(LocalQwenOllamaTeacherInferenceClient.CreateForTesting(configured, new Handler(), true, requestTimeout: TimeSpan.FromSeconds(30)).IsSuccess);

        // The actual authoring consumer must reject a legacy factory or forged context before any HTTP call.
        var legacyHandler = new Handler();
        TimeSpan? legacyDeadline = null;
        using var legacyClient = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(legacy, legacyHandler, true,
            deadlineFactory: t => { legacyDeadline = t; return new CancellationTokenSource(); }).Value!;
        var context = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateProductionContext(configured, () => OperationResult<ITrainingTeacherInferenceClient>.Ok(legacyClient), 1);
        Assert.True(context.Validate().IsSuccess);
        Assert.False((context with { InferenceProfile = cpu with { ConfigurationSha256 = new string('0', 64) } }).Validate().IsSuccess);
        Assert.False((CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateProductionContext(legacy, () => OperationResult<ITrainingTeacherInferenceClient>.Ok(legacyClient), 1) with { InferenceProfile = cpu }).Validate().IsSuccess);
        var row = new RoutingDecisionRecord("synthetic-run", "synthetic-request", "Explain obj?.Name ?? fallback.\r\nPreserve e\u0301 and the complete task.", "synthetic-null", "lawful", 1,
            [new UnitScoringResult("wrong", "unit", "csharp", 0, 0)], "Respect null propagation", "Wrong null law", DateTimeOffset.UnixEpoch);
        var source = CodeDocumentationRepositoryOwnedDpoAuthoringSource.Create([row], CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference, "synthetic-source", false).Value!;
        var preparedRequest = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(source, context);
        Assert.True(preparedRequest.IsSuccess, preparedRequest.ErrorMessage);
        var request = preparedRequest.Value!;
        // VSTest runs from the compiled output directory; resolve the existing project root without creating a fake repository.
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var addressResult = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateDefaultAddress(repositoryRoot);
        Assert.True(addressResult.IsSuccess, addressResult.ErrorMessage);
        var address = addressResult.Value!;
        var mismatch = await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthorPairsAsync(address, request, context);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherProvenanceMismatchErrorCode, mismatch.ErrorCode); Assert.Equal(0, legacyHandler.Calls);

        // R1: the reverse mismatch uses the actual strict producer and actual CPU client passport, before any Infer/permit/HTTP effect.
        var rejectedCpuTransport = new Handler(unloaded: true);
        using var rejectedCpuClient = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(configured, rejectedCpuTransport, true,
            syntheticCpuTokenCountForTesting: (_, _) => OperationResult<int[]>.Ok(new int[100])).Value!;
        using var reverseAdapter = new Handler { ForwardInferenceClient = rejectedCpuClient };
        var reverseContext = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateProductionContext(legacy, () => OperationResult<ITrainingTeacherInferenceClient>.Ok(reverseAdapter), 1);
        var reverseRequest = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PrepareRequest(source, reverseContext).Value!;
        var reverseMismatch = await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthorPairsAsync(address, reverseRequest, reverseContext, includeApprovedTask: true);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherProvenanceMismatchErrorCode, reverseMismatch.ErrorCode);
        Assert.Equal(1, reverseAdapter.PassportReads); Assert.Equal(0, reverseAdapter.InferCalls); Assert.Null(reverseAdapter.LastPermit); Assert.Equal(0, rejectedCpuTransport.Calls);
        using var unboundAdapter = new Handler();
        var unboundContext = context with { CreateTeacherClient = () => OperationResult<ITrainingTeacherInferenceClient>.Ok(unboundAdapter) };
        var unboundCpu = await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthorPairsAsync(address, request, unboundContext, includeApprovedTask: true);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherProvenanceMismatchErrorCode, unboundCpu.ErrorCode);
        Assert.Equal(1, unboundAdapter.PassportReads); Assert.Equal(0, unboundAdapter.InferCalls); Assert.Null(unboundAdapter.LastPermit);
        foreach (var admittedLegacyPassport in new[] { (TrainingTeacherInferenceProfileEvidence?)null, LocalQwenOllamaInferenceProfileAuthority.Authoring })
        {
            using var legacyAdapter = new Handler(declaredProfile: admittedLegacyPassport);
            var compatibleContext = reverseContext with { CreateTeacherClient = () => OperationResult<ITrainingTeacherInferenceClient>.Ok(legacyAdapter) };
            var admitted = await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthorPairsAsync(address, reverseRequest, compatibleContext, includeApprovedTask: true);
            Assert.NotEqual(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeacherProvenanceMismatchErrorCode, admitted.ErrorCode);
            Assert.Equal(1, legacyAdapter.PassportReads); Assert.Equal(1, legacyAdapter.InferCalls); Assert.True(legacyAdapter.LastPermit!.CallIssued);
        }

        // R2: call both real DesignConversion entrances with valid existing synthetic fixtures and their actual reservations.
        var chosen = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(htmlSuffix: "cpu-guard-chosen");
        var rejected = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(canPersistTrainingCandidate: false,
            jobId: "cpu-guard-rejected-job", requestId: "cpu-guard-rejected-request", outputId: "cpu-guard-rejected-output",
            candidateId: "cpu-guard-rejected-candidate", verdictId: "cpu-guard-rejected-verdict", score: 0.4, passed: false, htmlSuffix: "cpu-guard-rejected");
        Assert.True(DesignConversionDpoTeacherAttestationRuntime.BuildCanonicalPromptForTesting(chosen, rejected).IsSuccess);
        var submitted = chosen.SubmittedRequest;
        var currentRequest = new DesignConversionInternalEvaluateRequest(chosen.JobId, submitted.RequestId, submitted.SourceKind,
            submitted.Html, submitted.Css, submitted.DesignTreePayload, submitted.OriginalTreePayload, submitted.Screenshots,
            submitted.DesignRenders, submitted.Budget, [], [], new Dictionary<string, string>(), submitted.Provenance);
        var designRequest = new DesignConversionCandidateUpdateRequest("cpu-guard-loop", 1, currentRequest, chosen, chosen.VisualQaBoundaryTrace.TierPolicy, []);
        foreach (var declaration in new[] { cpu, LocalQwenOllamaInferenceProfileAuthority.Evaluation, cpu with { ProfileId = "unknown" }, LocalQwenOllamaInferenceProfileAuthority.Authoring with { ConfigurationSha256 = new string('0', 64) } })
        {
            using var designAdapter = new Handler(declaredProfile: declaration) { ForwardInferenceClient = declaration == cpu ? rejectedCpuClient : null };
            using var designRuntime = new DesignConversionDpoTeacherAttestationRuntime(designAdapter, configured.ModelId, configured.ExpectedOllamaManifestDigest, configured.ExpectedOllamaModelLayerDigest);
            var authorReservation = DesignConversionDpoTeacherCallReservation.Create(1, 0).Value!;
            var authorRefused = await designRuntime.AuthorCandidateAsync(designRequest, authorReservation);
            Assert.Equal(DesignConversionDpoTeacherAttestationRuntime.InferenceProfileInvalidErrorCode, authorRefused.ErrorCode);
            Assert.False(authorReservation.CallIssued); Assert.Equal(0, authorReservation.ActualObservedCallCount);
            var attestationReservation = DesignConversionDpoTeacherCallReservation.Create(1, 0).Value!;
            var attestationRefused = await designRuntime.AttestAsync(chosen, rejected, attestationReservation);
            Assert.Equal(DesignConversionDpoTeacherAttestationRuntime.InferenceProfileInvalidErrorCode, attestationRefused.ErrorCode);
            Assert.False(attestationReservation.CallIssued); Assert.Equal(0, attestationReservation.ActualObservedCallCount);
            Assert.Equal(2, designAdapter.PassportReads); Assert.Equal(0, designAdapter.InferCalls); Assert.Null(designAdapter.LastPermit); Assert.Equal(0, rejectedCpuTransport.Calls);
        }
        foreach (var admittedLegacyPassport in new[] { (TrainingTeacherInferenceProfileEvidence?)null, LocalQwenOllamaInferenceProfileAuthority.Authoring })
        {
            using var designAdapter = new Handler(declaredProfile: admittedLegacyPassport);
            using var designRuntime = new DesignConversionDpoTeacherAttestationRuntime(designAdapter, configured.ModelId, configured.ExpectedOllamaManifestDigest, configured.ExpectedOllamaModelLayerDigest);
            var authorReservation = DesignConversionDpoTeacherCallReservation.Create(1, 0).Value!;
            Assert.Equal(DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode, (await designRuntime.AuthorCandidateAsync(designRequest, authorReservation)).ErrorCode);
            Assert.True(authorReservation.CallIssued); Assert.Equal(1, authorReservation.ActualObservedCallCount);
            var attestationReservation = DesignConversionDpoTeacherCallReservation.Create(1, 0).Value!;
            Assert.Equal(DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode, (await designRuntime.AttestAsync(chosen, rejected, attestationReservation)).ErrorCode);
            Assert.True(attestationReservation.CallIssued); Assert.Equal(1, attestationReservation.ActualObservedCallCount);
            Assert.Equal(2, designAdapter.PassportReads); Assert.Equal(2, designAdapter.InferCalls);
        }

        // Exercise the real strict single-pair producer, rather than proving only an independently hand-typed JSON request.
        var producerHandler = new Handler(unloaded: true);
        using var producerClient = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(configured, producerHandler, true,
            syntheticCpuTokenCountForTesting: (_, _) => OperationResult<int[]>.Ok(new int[100])).Value!;
        var producerContext = context with { CreateTeacherClient = () => OperationResult<ITrainingTeacherInferenceClient>.Ok(producerClient) };
        var produced = await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthorPairsAsync(address, request, producerContext, includeApprovedTask: true);
        Assert.True(produced.IsSuccess, produced.ErrorMessage); Assert.Single(produced.Value!); Assert.Equal(6, producerHandler.Calls);
        using (var producedBody = JsonDocument.Parse(producerHandler.ChatBody!))
        using (var producedPrompt = JsonDocument.Parse(producedBody.RootElement.GetProperty("messages")[0].GetProperty("content").GetString()!))
        {
            Assert.Equal(row.Prompt, producedPrompt.RootElement.GetProperty("approved_task").GetString());
            var completeContext = producedPrompt.RootElement.GetProperty("authoring_context").GetString()!;
            Assert.Contains("Return only JSON with string fields: chosen, rejected, rejected_reason.", completeContext);
            Assert.Contains("paid_reference=Route to lawful. Respect null propagation", completeContext);
            Assert.Contains("weak_candidate=Route to wrong. Wrong null law", completeContext);
        }
        var bulkHandler = new Handler(unloaded: true);
        using var bulkClient = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(configured, bulkHandler, true,
            syntheticCpuTokenCountForTesting: (_, _) => OperationResult<int[]>.Ok(new int[100])).Value!;
        var bulkContext = context with { CreateTeacherClient = () => OperationResult<ITrainingTeacherInferenceClient>.Ok(bulkClient) };
        var plaintext = await CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.AuthorPairsAsync(address, request, bulkContext, includeApprovedTask: false);
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.PromptInvalidErrorCode, plaintext.ErrorCode); Assert.Equal(0, bulkHandler.Calls);

        var validate = typeof(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring).GetMethod("ValidateTeacherProvenance", BindingFlags.NonPublic | BindingFlags.Static)!;
        var domain = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CSharpDomainProfile;
        Assert.True(((OperationResult<bool>)validate.Invoke(null, [result.Value, request.RequestIdentity, domain, cpu])!).IsSuccess);
        Assert.False(((OperationResult<bool>)validate.Invoke(null, [result.Value with { InferenceProfile = legacy.AuthoringProfile }, request.RequestIdentity, domain, cpu])!).IsSuccess);

        // Separate fresh clients preserve exact legacy/evaluation body bytes, deadlines and unloaded-model refusal.
        using var stable = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(legacy, new Handler(), true,
            deadlineFactory: t => { legacyDeadline = t; return new CancellationTokenSource(); }).Value!;
        Assert.True((await stable.InferAsync("legacy", new Permit())).IsSuccess); Assert.Equal(TimeSpan.FromSeconds(30), legacyDeadline);
        var build = typeof(LocalQwenOllamaTeacherInferenceClient).GetMethod("BuildRequestBody", BindingFlags.NonPublic | BindingFlags.Static)!;
        byte[] Body(LocalQwenOllamaInferenceProfileDescriptor p) => ((OperationResult<byte[]>)build.Invoke(null, [configured.ModelId, "legacy", p, false])!).Value!;
        var prefix = "{\"model\":\"qwen3.5:9b-q4_K_M\",\"stream\":false,\"messages\":[{\"role\":\"user\",\"content\":\"legacy\"}],\"format\":\"json\",";
        Assert.Equal(prefix + "\"options\":{\"num_ctx\":8192,\"temperature\":0,\"seed\":0},\"keep_alive\":\"5m\"}", Encoding.UTF8.GetString(Body(LocalQwenOllamaInferenceProfileAuthority.AuthoringDescriptor)));
        Assert.Equal(prefix + "\"think\":false,\"options\":{\"num_ctx\":8192,\"temperature\":0,\"seed\":0,\"num_predict\":256},\"keep_alive\":\"5m\"}", Encoding.UTF8.GetString(Body(LocalQwenOllamaInferenceProfileAuthority.EvaluationDescriptor)));
        using var evaluator = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(configured, new Handler(), true,
            deadlineFactory: t => { deadline = t; return new CancellationTokenSource(); }).Value!;
        var eval = await evaluator.EvaluateAsync("legacy", new Permit());
        Assert.True(eval.IsSuccess, eval.ErrorMessage); Assert.Equal(LocalQwenOllamaInferenceProfileAuthority.Evaluation, eval.Value!.InferenceProfile); Assert.Equal(TimeSpan.FromSeconds(120), deadline);
        using var unloadedLegacy = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(legacy, new Handler(unloaded: true), true).Value!;
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ModelIdentityUnavailableErrorCode, (await unloadedLegacy.InferAsync("legacy", new Permit())).ErrorCode);
        using var missingPreflight = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(configured, new Handler(missingTags: true), true, syntheticCpuTokenCountForTesting: (_, _) => OperationResult<int[]>.Ok([0])).Value!;
        var missingPermit = new Permit(); Assert.False((await missingPreflight.InferAsync(prompt, missingPermit)).IsSuccess); Assert.False(missingPermit.CallIssued);
    }

    private static OperationResult<TrainingTeacherInferenceProfileEvidence> ReadProfile(string? id, string? sha)
        => ContinuousLearningModelServerConfigurationLoader.ReadAuthoringProfile(n => n == ContinuousLearningModelServerConfigurationLoader.AuthoringProfileIdEnvironmentName ? id : sha);
    private static OperationResult<ContinuousLearningTp1DpoConfiguration> Configuration(TrainingTeacherInferenceProfileEvidence? p)
        => ContinuousLearningTp1DpoConfiguration.Create("qwen3.5:9b-q4_K_M", ConfiguredQwenVocabularyLease.ManifestSha, ConfiguredQwenVocabularyLease.ModelSha, 1, "sha256:" + new string('a', 64), "sha256:" + new string('b', 64), new byte[32], authoringProfile: p);
    private sealed class Permit : ITrainingTeacherCallPermit, ILegalTeacherCallPermit
    {
        public bool CallIssued { get; private set; }
        public OperationResult<int> TryIssue() { if (CallIssued) return OperationResult<int>.Fail("Already issued", "ALREADY_ISSUED"); CallIssued = true; return OperationResult<int>.Ok(1); }
    }
    private sealed class Handler(bool unloaded = false, bool missingTags = false, TrainingTeacherInferenceProfileEvidence? declaredProfile = null) : HttpMessageHandler, ITrainingTeacherInferenceClient
    {
        internal int Calls { get; private set; }
        internal string? ChatBody { get; private set; }
        internal ITrainingTeacherInferenceClient? ForwardInferenceClient { get; init; }
        internal int PassportReads { get; private set; }
        internal int InferCalls { get; private set; }
        internal ITrainingTeacherCallPermit? LastPermit { get; private set; }
        public TrainingTeacherInferenceProfileEvidence? AuthoringProfile { get { PassportReads++; return ForwardInferenceClient?.AuthoringProfile ?? declaredProfile; } }
        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(string prompt, ITrainingTeacherCallPermit callPermit, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested(); InferCalls++; LastPermit = callPermit;
            if (ForwardInferenceClient is not null) return ForwardInferenceClient.InferAsync(prompt, callPermit, cancellationToken);
            var issued = callPermit.TryIssue();
            return Task.FromResult(issued.IsSuccess
                ? OperationResult<TrainingTeacherInferenceResult>.Fail("Synthetic unavailable adapter", DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode)
                : issued.MapError<TrainingTeacherInferenceResult>());
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++;
            var identity = new { name = "qwen3.5:9b-q4_K_M", model = "qwen3.5:9b-q4_K_M", digest = ConfiguredQwenVocabularyLease.ManifestSha };
            object body;
            switch (request.RequestUri!.AbsolutePath)
            {
                case "/api/chat": ChatBody = await request.Content!.ReadAsStringAsync(token); body = new { model = identity.model, done = true, message = new { role = "assistant", content = "{\"chosen\":\"Synthetic lawful null explanation\",\"rejected\":\"Synthetic wrong null explanation\",\"rejected_reason\":\"Wrong null propagation\"}" } }; break;
                case "/api/show": body = new { modelfile = "FROM C:\\synthetic\\blobs\\sha256-" + ConfiguredQwenVocabularyLease.ModelSha }; break;
                case "/api/ps" when unloaded: body = new { models = Array.Empty<object>() }; break;
                case "/api/tags" when missingTags: body = new { models = Array.Empty<object>() }; break;
                default: body = new { models = new[] { identity } }; break;
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };
        }
    }
}
