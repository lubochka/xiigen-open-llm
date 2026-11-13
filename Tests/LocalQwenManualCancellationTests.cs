// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace LlmFromScratch.Tests;

using System.Net;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using TrainableRouter.Training;

public sealed class LocalQwenManualCancellationTests
{
    [Fact]
    public async Task ExplicitManualProfiles_KeepExactConsumersAndCaps_WithoutTimer_AndHonorCallerAndDispose()
    {
        // Synthetic engineering transport only: no Ollama, native model, training or exam inputs.
        var author = LocalQwenOllamaInferenceProfileAuthority.ManualCpuAuthoring;
        var evaluator = LocalQwenOllamaInferenceProfileAuthority.ManualEvaluation;
        Assert.Equal("c3e56109f94eae51c9c5439e4be1477cbdf8578cbb42ff700ab165c9b7b26d77", author.ConfigurationSha256);
        Assert.Equal("05a98ca23c020836c7c14dc7ee26c02030783c88f4286eb869d6f1a5de884575", evaluator.ConfigurationSha256);
        var configuration = Configuration(author, evaluator).Value!;
        var legacy = Configuration(null, null).Value!;
        Assert.Equal("b6b088d7fbee1bece702df1448e309b8a88335178a5bc84b2376bd68699453bf", legacy.AuthoringProfile.ConfigurationSha256);
        Assert.Equal("00be8f07e2f5d5d6e4a8c3a389a643dbcd4a1b37af4112432c9872ffd137b1cc", legacy.EvaluationProfile.ConfigurationSha256);
        Assert.Equal("c1973891d1414f24b8625668446f5b33ffddeb433acb9b15daedc00d52d8aea5", LocalQwenOllamaInferenceProfileAuthority.CpuAuthoring.ConfigurationSha256);
        Assert.NotEqual(LocalQwenOllamaInferenceProfileAuthority.CpuAuthoring, author);
        Assert.NotEqual(legacy.EvaluationProfile, evaluator);
        Assert.False(LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(author));
        Assert.False(LocalQwenOllamaInferenceProfileAuthority.IsCanonicalCpuAuthoring(author));
        Assert.False(Configuration(evaluator, author).IsSuccess);
        Assert.False(Configuration(author with { ConfigurationSha256 = new string('0', 64) }, evaluator).IsSuccess);
        Assert.False(Configuration(author, evaluator with { ConfigurationSha256 = new string('0', 64) }).IsSuccess);
        Assert.Equal(120, configuration.MaxTeacherCalls);
        Assert.Equal(1, configuration.DpoMaxStepCount);

        foreach (var evaluation in new[] { false, true })
        {
            var selected = evaluation ? evaluator : author;
            OperationResult<TrainingTeacherInferenceProfileEvidence> Read(string? id, string? sha)
                => evaluation
                    ? ContinuousLearningModelServerConfigurationLoader.ReadEvaluationProfile(n => n == ContinuousLearningModelServerConfigurationLoader.EvaluationProfileIdEnvironmentName ? id : sha)
                    : ContinuousLearningModelServerConfigurationLoader.ReadAuthoringProfile(n => n == ContinuousLearningModelServerConfigurationLoader.AuthoringProfileIdEnvironmentName ? id : sha);
            Assert.Equal(evaluation ? legacy.EvaluationProfile : legacy.AuthoringProfile, Read(null, null).Value);
            Assert.Equal(selected, Read(selected.ProfileId, selected.ConfigurationSha256).Value);
            Assert.False(Read(selected.ProfileId, null).IsSuccess);
            Assert.False(Read(null, selected.ConfigurationSha256).IsSuccess);
            Assert.False(Read(selected.ProfileId, new string('0', 64)).IsSuccess);
            Assert.False(Read("unknown", selected.ConfigurationSha256).IsSuccess);
        }

        var context = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateProductionContext(configuration,
            () => OperationResult<ITrainingTeacherInferenceClient>.Fail("No effects in this check", "SYNTHETIC"), 1);
        Assert.True(context.Validate().IsSuccess);
        Assert.False((context with { InferenceProfile = LocalQwenOllamaInferenceProfileAuthority.CpuAuthoring }).Validate().IsSuccess);
        var legacyContext = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.CreateProductionContext(legacy,
            context.CreateTeacherClient, 1);
        Assert.False((legacyContext with { InferenceProfile = author }).Validate().IsSuccess);
        Assert.Equal(Timeout.InfiniteTimeSpan, CodeDocumentationSavedModelAssessmentCalibrationService.ResolveMaximumDuration(null, configuration));
        Assert.Equal(TimeSpan.FromMinutes(5), CodeDocumentationSavedModelAssessmentCalibrationService.ResolveMaximumDuration(null, legacy));
        Assert.True(CodeDocumentationSavedModelAssessmentCalibrationService.IsValidMaximumDuration(Timeout.InfiniteTimeSpan, configuration));
        Assert.False(CodeDocumentationSavedModelAssessmentCalibrationService.IsValidMaximumDuration(Timeout.InfiniteTimeSpan, legacy));
        foreach (var invalid in new[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(-2), TimeSpan.FromMinutes(-1) })
            Assert.False(CodeDocumentationSavedModelAssessmentCalibrationService.IsValidMaximumDuration(invalid, configuration));
        var scorer = CodeDocumentationSavedModelAssessmentCalibrationService.BuildCurrentScorerIdentity("csharp", configuration.ModelId, configuration.ExpectedOllamaManifestDigest, configuration);
        Assert.NotEqual(scorer, CodeDocumentationSavedModelAssessmentCalibrationService.BuildCurrentScorerIdentity("csharp", legacy.ModelId, legacy.ExpectedOllamaManifestDigest));

        const string prompt = "{\"task\":\"Synthetic complete prompt\"}";
        var handler = new Handler();
        using (var client = Client(configuration, handler))
        using (var authoring = ContinuousLearningTp1AuthoringComposition.OwnForTesting(client))
        using (var evaluation = ContinuousLearningTp1EvaluationComposition.OwnForTesting(client))
        {
            Assert.Equal(author, authoring.AuthoringProfile);
            Assert.Equal(evaluator, evaluation.EvaluationProfile);
            var authored = await authoring.InferAsync(prompt, new Permit());
            Assert.True(authored.IsSuccess, authored.ErrorMessage);
            Assert.Equal(author, authored.Value!.InferenceProfile);
            using (var body = JsonDocument.Parse(handler.ChatBody!))
            {
                var root = body.RootElement; var options = root.GetProperty("options");
                Assert.Equal(prompt, root.GetProperty("messages")[0].GetProperty("content").GetString());
                Assert.Equal(2048, options.GetProperty("num_ctx").GetInt32());
                Assert.Equal(512, options.GetProperty("num_predict").GetInt32());
                Assert.Equal(2, options.GetProperty("num_thread").GetInt32());
                Assert.Equal(16, options.GetProperty("num_batch").GetInt32());
                Assert.Equal(0, options.GetProperty("num_gpu").GetInt32());
                Assert.False(root.GetProperty("truncate").GetBoolean());
                Assert.False(root.GetProperty("shift").GetBoolean());
            }
            var evaluated = await evaluation.EvaluateAsync("synthetic public evaluation", new Permit());
            Assert.True(evaluated.IsSuccess, evaluated.ErrorMessage);
            Assert.Equal(evaluator, evaluated.Value!.InferenceProfile);
            Assert.False(evaluated.Value.TrainingTeacherProvenance.TeacherOutputUsedForTraining);
            Assert.False(evaluated.Value.TrainingTeacherProvenance.ProviderTermsAllowTraining);
            using var evalBody = JsonDocument.Parse(handler.ChatBody!);
            Assert.Equal(8192, evalBody.RootElement.GetProperty("options").GetProperty("num_ctx").GetInt32());
            Assert.Equal(256, evalBody.RootElement.GetProperty("options").GetProperty("num_predict").GetInt32());
        }
        Assert.True(handler.Disposed);

        foreach (var evaluation in new[] { false, true })
        foreach (var dispose in new[] { false, true })
        {
            var waitingHandler = new Handler { WaitForCancellation = true };
            using var client = Client(configuration, waitingHandler);
            using var caller = new CancellationTokenSource();
            var permit = new Permit();
            Task pending = evaluation
                ? client.EvaluateAsync("synthetic public evaluation", permit, caller.Token)
                : client.InferAsync(prompt, permit, caller.Token);
            await waitingHandler.ChatEntered.Task;
            Assert.False(pending.IsCompleted);
            Assert.True(permit.CallIssued);
            if (dispose)
            {
                client.Dispose();
                var code = evaluation
                    ? (await (Task<OperationResult<TeacherEvaluationInferenceResult>>)pending).ErrorCode
                    : (await (Task<OperationResult<TrainingTeacherInferenceResult>>)pending).ErrorCode;
                Assert.Equal(LocalQwenOllamaTeacherInferenceClient.DisposedErrorCode, code);
            }
            else
            {
                caller.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
                client.Dispose();
            }
            Assert.True(waitingHandler.CancellationObserved);
            Assert.True(waitingHandler.Disposed);
        }
    }

    private static OperationResult<ContinuousLearningTp1DpoConfiguration> Configuration(
        TrainingTeacherInferenceProfileEvidence? author, TrainingTeacherInferenceProfileEvidence? evaluation)
        => ContinuousLearningTp1DpoConfiguration.Create("qwen3.5:9b-q4_K_M", ConfiguredQwenVocabularyLease.ManifestSha,
            ConfiguredQwenVocabularyLease.ModelSha, 120, "sha256:" + new string('a', 64), "sha256:" + new string('b', 64),
            new byte[32], dpoMaxStepCount: 1, authoringProfile: author, evaluationProfile: evaluation);

    private static LocalQwenOllamaTeacherInferenceClient Client(ContinuousLearningTp1DpoConfiguration configuration, Handler handler)
        => LocalQwenOllamaTeacherInferenceClient.CreateForTesting(configuration, handler, true,
            deadlineFactory: _ => throw new InvalidOperationException("Manual profile must not create a deadline timer"),
            syntheticCpuTokenCountForTesting: (_, token) => { token.ThrowIfCancellationRequested(); return OperationResult<int[]>.Ok(new int[1536]); }).Value!;

    private sealed class Permit : ITrainingTeacherCallPermit, ILegalTeacherCallPermit
    {
        public bool CallIssued { get; private set; }
        public OperationResult<int> TryIssue()
        {
            if (CallIssued) return OperationResult<int>.Fail("Already issued", "ALREADY_ISSUED");
            CallIssued = true;
            return OperationResult<int>.Ok(1);
        }
    }

    private sealed class Handler : HttpMessageHandler
    {
        internal bool WaitForCancellation { get; init; }
        internal TaskCompletionSource ChatEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal string? ChatBody { get; private set; }
        internal bool CancellationObserved { get; private set; }
        internal bool Disposed { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var identity = new { name = "qwen3.5:9b-q4_K_M", model = "qwen3.5:9b-q4_K_M", digest = ConfiguredQwenVocabularyLease.ManifestSha };
            object body;
            switch (request.RequestUri!.AbsolutePath)
            {
                case "/api/chat":
                    ChatBody = await request.Content!.ReadAsStringAsync(token);
                    ChatEntered.TrySetResult();
                    if (WaitForCancellation)
                    {
                        try { await Task.Delay(Timeout.Infinite, token); }
                        catch (OperationCanceledException) { CancellationObserved = true; throw; }
                    }
                    body = new { model = identity.model, done = true, message = new { role = "assistant", content = "synthetic output" } };
                    break;
                case "/api/show": body = new { modelfile = "FROM C:\\synthetic\\blobs\\sha256-" + ConfiguredQwenVocabularyLease.ModelSha }; break;
                default: body = new { models = new[] { identity } }; break;
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };
        }

        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
