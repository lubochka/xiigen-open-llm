// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Net;
using System.Reflection;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;

public sealed class LocalQwenOllamaTeacherInferenceClientTests
{
    private const string ModelId = "qwen3.5:9b";
    private const string Output = "bounded teacher output";
    private static readonly string ManifestDigest = new('d', 64);
    private static readonly string ModelLayerDigest = new('c', 64);

    [Fact]
    public async Task InferAsync_UsesExactSerializedSixRequestProtocol()
    {
        const string prompt = "one user prompt";
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync(prompt, permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var inferenceProfile =
            LocalQwenOllamaInferenceProfileAuthority.AuthoringDescriptor;
        Assert.True(permit.CallIssued);
        Assert.Equal(Output, result.Value!.Output);
        Assert.Equal(ManifestDigest, result.Value.TrainingTeacherProvenance.ModelVersion);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            result.Value.TrainingTeacherProvenance.ModelArtifactDigestKind);
        Assert.Equal(
            ModelLayerDigest,
            result.Value.TrainingTeacherProvenance.ModelArtifactDigest);
        Assert.Equal(
            inferenceProfile.Evidence,
            result.Value.InferenceProfile);
        Assert.Equal(
            new[]
            {
                "GET http://127.0.0.1:11434/api/tags",
                "POST http://127.0.0.1:11434/api/show",
                "POST http://127.0.0.1:11434/api/chat",
                "GET http://127.0.0.1:11434/api/ps",
                "GET http://127.0.0.1:11434/api/tags",
                "POST http://127.0.0.1:11434/api/show"
            },
            handler.Requests.Select(static request => request.MethodAndUri));
        Assert.All(
            handler.Requests.Where(static request => request.Method == HttpMethod.Get),
            static request => Assert.Null(request.Body));
        Assert.Equal(2, handler.Requests.Count(
            static request => request.Uri.AbsolutePath == "/api/show"));

        var show = handler.Requests[1];
        Assert.Equal("application/json", show.ContentType);
        Assert.Equal("utf-8", show.CharSet);
        using (var showBody = JsonDocument.Parse(show.Body!))
            Assert.Equal(ModelId, showBody.RootElement.GetProperty("model").GetString());

        var chat = handler.Requests[2];
        Assert.Equal("application/json", chat.ContentType);
        Assert.Equal("utf-8", chat.CharSet);
        Assert.Equal(
            "{\"model\":\"qwen3.5:9b\",\"stream\":false,\"messages\":[{\"role\":\"user\",\"content\":\"one user prompt\"}],\"format\":\"json\",\"options\":{\"num_ctx\":8192,\"temperature\":0,\"seed\":0},\"keep_alive\":\"5m\"}",
            chat.Body);
        using var body = JsonDocument.Parse(chat.Body!);
        var root = body.RootElement;
        Assert.Equal(6, root.EnumerateObject().Count());
        Assert.Equal(ModelId, root.GetProperty("model").GetString());
        Assert.False(root.GetProperty("stream").GetBoolean());
        Assert.Equal(
            LocalQwenOllamaRequestFormatMode.ExplicitIdentity,
            inferenceProfile.RequestFormatMode);
        Assert.Equal(
            inferenceProfile.FormatIdentity,
            root.GetProperty("format").GetString());
        var options = root.GetProperty("options");
        Assert.Equal(inferenceProfile.NumContext, options.GetProperty("num_ctx").GetInt32());
        Assert.Equal(inferenceProfile.Temperature, options.GetProperty("temperature").GetDouble());
        Assert.Equal(inferenceProfile.Seed, options.GetProperty("seed").GetInt32());
        Assert.Equal("5m", root.GetProperty("keep_alive").GetString());
        var message = Assert.Single(root.GetProperty("messages").EnumerateArray());
        Assert.Equal("user", message.GetProperty("role").GetString());
        Assert.Equal(prompt, message.GetProperty("content").GetString());
    }

    [Fact]
    public async Task EvaluateAsync_UsesBoundedStructuredRequestAndReturnsEvaluationOnlyProvenance()
    {
        const string arbitraryText = "arbitrary evaluation text; not JSON";
        const string ownerTask = "Explain \"{ task }\"\nnext line\r\nlast line";
        const string referenceAnswer = "Closed \"{ reference }\"\nnext line\r\nlast line";
        const string studentAnswer = "Student \"{ answer }\"\nnext line\r\nlast line";
        var referenceProvenance = PaidReferenceProvenance.Create(
            "synthetic-paid-provider", "synthetic-paid-model", "v1", "synthetic-formatter-reference",
            DateTimeOffset.UnixEpoch, FrozenReferenceEntry.ComputeNormalizedTextSha256(referenceAnswer));
        Assert.True(referenceProvenance.IsSuccess, referenceProvenance.ErrorMessage);
        var stored = FrozenReferenceEntry.Create(ownerTask, referenceAnswer, referenceProvenance.Value!,
            "csharp", "code-documentation", FrozenReferencePartition.HeldOut,
            FrozenReferenceEntry.ComputeNormalizedTextSha256("synthetic-formatter-source"));
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
        var entry = stored.Value!;
        var prompt = CodeDocumentationSavedModelAssessmentService.BuildCheckerPrompt(entry, studentAnswer);
        var handler = new ScriptedHandler((request, cancellationToken) =>
            request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                ? Task.FromResult(JsonResponse(ChatJson(content: arbitraryText)))
                : DefaultResponseAsync(request, cancellationToken));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.EvaluateAsync(prompt, permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var inferenceProfile =
            LocalQwenOllamaInferenceProfileAuthority.EvaluationDescriptor;
        Assert.Equal(arbitraryText, result.Value!.Output);
        Assert.Equal(
            inferenceProfile.Evidence,
            result.Value.InferenceProfile);
        Assert.True(permit.CallIssued);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(
            new[]
            {
                "GET http://127.0.0.1:11434/api/tags",
                "POST http://127.0.0.1:11434/api/show",
                "POST http://127.0.0.1:11434/api/chat",
                "GET http://127.0.0.1:11434/api/ps",
                "GET http://127.0.0.1:11434/api/tags",
                "POST http://127.0.0.1:11434/api/show"
            },
            handler.Requests.Select(static request => request.MethodAndUri));

        var chat = handler.Requests[2];
        Assert.Equal(
            "{\"model\":\"qwen3.5:9b\",\"stream\":false,\"messages\":[{\"role\":\"user\",\"content\":" +
                JsonSerializer.Serialize(prompt) +
                "}],\"format\":\"json\",\"think\":false,\"options\":{\"num_ctx\":8192,\"temperature\":0,\"seed\":0,\"num_predict\":256},\"keep_alive\":\"5m\"}",
            chat.Body);
        Assert.Equal(
            LocalQwenOllamaRequestFormatMode.ExplicitIdentity,
            inferenceProfile.RequestFormatMode);
        using (var body = JsonDocument.Parse(chat.Body!))
        {
            var message = Assert.Single(body.RootElement.GetProperty("messages").EnumerateArray());
            Assert.Equal("user", message.GetProperty("role").GetString());
            var sentPrompt = message.GetProperty("content").GetString()!;
            Assert.Equal(prompt, sentPrompt);
            using var payload = JsonDocument.Parse(sentPrompt[sentPrompt.IndexOf('{')..]);
            Assert.Equal(entry.EntryId, payload.RootElement.GetProperty("caseId").GetString());
            Assert.Equal(entry.OwnerInput, payload.RootElement.GetProperty("ownerTask").GetString());
            Assert.Equal(studentAnswer, payload.RootElement.GetProperty("studentAnswer").GetString());
            Assert.Equal(entry.PaidModelReferenceAnswer, payload.RootElement.GetProperty("closedReferenceAnswer").GetString());
            Assert.Equal("json", body.RootElement.GetProperty("format").GetString());
            Assert.False(body.RootElement.GetProperty("think").GetBoolean());
            var options = body.RootElement.GetProperty("options");
            Assert.Equal(
                inferenceProfile.NumContext,
                options.GetProperty("num_ctx").GetInt32());
            Assert.Equal(
                inferenceProfile.Temperature,
                options.GetProperty("temperature").GetDouble());
            Assert.Equal(
                inferenceProfile.Seed,
                options.GetProperty("seed").GetInt32());
            Assert.Equal(
                inferenceProfile.NumPredict,
                options.GetProperty("num_predict").GetInt32());
        }
        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.DefaultEvaluationRequestTimeout,
            inferenceProfile.RequestTimeout);

        var provenance = result.Value.TrainingTeacherProvenance;
        Assert.False(provenance.TeacherOutputUsedForTraining);
        var authority = CreateConfiguration().CreateTeacherPolicyBudgetAuthority().Value!;
        Assert.True(
            authority.ValidateLocalEvaluationOutput(provenance).IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.TermsNotApprovedErrorCode,
            authority.ValidateBatch([provenance]).ErrorCode);
        var invalidPermit = new TestCallPermit();
        var invalid = await client.EvaluateAsync(prompt + "\n", invalidPermit); // A literal control outside the serialized payload still fails before transport and call issuance.
        Assert.False(invalid.IsSuccess);
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.PromptInvalidErrorCode, invalid.ErrorCode);
        Assert.Equal(6, handler.Requests.Count);
        Assert.False(invalidPermit.CallIssued);
        Assert.Equal(0, invalidPermit.IssueCount);
        Assert.Equal(1, permit.IssueCount);
    }

    [Fact]
    public void InferenceProfiles_UseByteExactGoldenCodecAndRejectNearMisses()
    {
        const string authoringDigest =
            "b6b088d7fbee1bece702df1448e309b8a88335178a5bc84b2376bd68699453bf";
        const string evaluationDigest =
            "00be8f07e2f5d5d6e4a8c3a389a643dbcd4a1b37af4112432c9872ffd137b1cc";
        var authoring = LocalQwenOllamaInferenceProfileAuthority.Authoring;

        Assert.Equal("qwen35-local-authoring-v1", authoring.ProfileId);
        Assert.Equal(authoringDigest, authoring.ConfigurationSha256);
        Assert.Equal(
            "qwen35-local-evaluation-v1",
            LocalQwenOllamaInferenceProfileAuthority.Evaluation.ProfileId);
        Assert.Equal(
            evaluationDigest,
            LocalQwenOllamaInferenceProfileAuthority.Evaluation.ConfigurationSha256);
        Assert.Equal(
            evaluationDigest,
            LocalQwenOllamaInferenceProfileAuthority.CreateEvaluationForTesting(
                "qwen35-local-evaluation-v1",
                "evaluation_only",
                "json",
                "8192",
                "0",
                "0",
                think: false,
                numPredict: 256,
                TimeSpan.FromSeconds(120)).ConfigurationSha256);
        Assert.NotEqual(
            evaluationDigest,
            LocalQwenOllamaInferenceProfileAuthority.CreateEvaluationForTesting(
                "qwen35-local-evaluation-v1",
                "evaluation_only",
                "json",
                "8192",
                "0",
                "0",
                think: true,
                numPredict: 256,
                TimeSpan.FromSeconds(120)).ConfigurationSha256);
        Assert.NotEqual(
            evaluationDigest,
            LocalQwenOllamaInferenceProfileAuthority.CreateEvaluationForTesting(
                "qwen35-local-evaluation-v1",
                "evaluation_only",
                "json",
                "8192",
                "0",
                "0",
                think: false,
                numPredict: 255,
                TimeSpan.FromSeconds(120)).ConfigurationSha256);
        Assert.NotEqual(
            evaluationDigest,
            LocalQwenOllamaInferenceProfileAuthority.CreateEvaluationForTesting(
                "qwen35-local-evaluation-v1",
                "evaluation_only",
                "json",
                "8192",
                "0",
                "0",
                think: false,
                numPredict: 256,
                TimeSpan.FromSeconds(119)).ConfigurationSha256);
        Assert.Equal(
            authoringDigest,
            HashProfile(
                "xiigen:local-qwen-ollama-inference-profile:v1\0",
                bigEndian: true,
                reorderPurpose: false));
        Assert.NotEqual(
            authoringDigest,
            HashProfile(
                "xiigen:local-qwen-ollama-inference-profile:v1",
                bigEndian: true,
                reorderPurpose: false));
        Assert.NotEqual(
            authoringDigest,
            HashProfile(
                "xiigen:local-qwen-ollama-inference-profile:v1\0",
                bigEndian: false,
                reorderPurpose: false));
        Assert.NotEqual(
            authoringDigest,
            HashProfile(
                "xiigen:local-qwen-ollama-inference-profile:v1\0",
                bigEndian: true,
                reorderPurpose: true));
        Assert.NotEqual(
            authoringDigest,
            LocalQwenOllamaInferenceProfileAuthority.CreateForTesting(
                "qwen35-local-authoring-v2",
                "training_authoring",
                "json",
                "8192",
                "0",
                "0").ConfigurationSha256);
        Assert.NotEqual(
            authoringDigest,
            LocalQwenOllamaInferenceProfileAuthority.CreateForTesting(
                "qwen35-local-authoring-v1",
                "evaluation_only",
                "json",
                "8192",
                "0",
                "0").ConfigurationSha256);
        Assert.False(LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(
            new(authoring.ProfileId, authoringDigest.ToUpperInvariant())));
        Assert.False(LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(
            new(authoring.ProfileId, $"sha256:{authoringDigest}")));
    }

    [Fact]
    public async Task InferAsync_SerializesInstalledMixedCaseOllamaTagByteForByte()
    {
        const string installedModelId = "qwen3.5:9b-q4_K_M";
        const string hostedAlias = "Qwen/Qwen3.5-9B";
        var handler = new ScriptedHandler((request, cancellationToken) =>
            request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                ? Task.FromResult(JsonResponse(ChatJson(modelId: installedModelId)))
                : request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ShowEndpoint
                    ? Task.FromResult(JsonResponse(ShowJson(ModelLayerDigest)))
                : Task.FromResult(JsonResponse(IdentityJson(
                    ModelEntry(installedModelId, ManifestDigest)))));
        var permit = new TestCallPermit();
        using var client = CreateClient(
            handler,
            configuration: CreateConfiguration(installedModelId));

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var chat = handler.Requests[2];
        Assert.Contains(
            "\"model\":\"qwen3.5:9b-q4_K_M\"",
            chat.Body,
            StringComparison.Ordinal);
        Assert.DoesNotContain(hostedAlias, chat.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("qwen3.5:9b-q4_k_m", chat.Body, StringComparison.Ordinal);
        using var body = JsonDocument.Parse(chat.Body!);
        Assert.Equal(
            installedModelId,
            body.RootElement.GetProperty("model").GetString());
        Assert.Equal(installedModelId, result.Value!.TrainingTeacherProvenance.ModelId);
    }

    [Fact]
    public async Task InferAsync_AcceptsOneExactTargetAmongUnrelatedValidEntries()
    {
        var unrelated = ModelEntry("Mistral:Latest", new string('a', 64));
        var exact = ModelEntry(ModelId, ManifestDigest);
        var handler = new ScriptedHandler((request, cancellationToken) =>
            request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                ? Task.FromResult(JsonResponse(ChatJson()))
                : request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ShowEndpoint
                    ? Task.FromResult(JsonResponse(ShowJson(ModelLayerDigest)))
                : Task.FromResult(JsonResponse(IdentityJson(unrelated, exact))));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(permit.CallIssued);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Theory]
    [MemberData(nameof(InvalidManifestDigests))]
    public void Configuration_RejectsNonCanonicalManifestDigest(string digest)
    {
        var result = ContinuousLearningTp1DpoConfiguration.Create(
            ModelId,
            digest,
            ModelLayerDigest,
            maxTeacherCalls: 1,
            EvidenceRef('a'),
            EvidenceRef('b'),
            AuditSalt());

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            result.ErrorCode);
    }

    public static IEnumerable<object[]> InvalidManifestDigests()
    {
        yield return [string.Empty];
        yield return [new string('a', 63)];
        yield return [new string('a', 65)];
        yield return [new string('A', 64)];
        yield return ["sha256:" + new string('a', 64)];
        yield return [new string('a', 63) + " "];
        yield return [new string('g', 64)];
    }

    [Theory]
    [MemberData(nameof(InvalidManifestDigests))]
    public void Configuration_RejectsNonCanonicalModelLayerDigest(string digest)
    {
        var result = ContinuousLearningTp1DpoConfiguration.Create(
            ModelId,
            ManifestDigest,
            digest,
            maxTeacherCalls: 1,
            EvidenceRef('a'),
            EvidenceRef('b'),
            AuditSalt());

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void Configuration_LegacyOverloadCannotOmitModelLayerDigest()
    {
        var result = ContinuousLearningTp1DpoConfiguration.Create(
            ModelId,
            ManifestDigest,
            maxTeacherCalls: 1,
            EvidenceRef('a'),
            EvidenceRef('b'),
            AuditSalt());

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            result.ErrorCode);
    }

    [Fact]
    public void Configuration_AdmitsInstalledMixedCaseOllamaTag()
    {
        const string installedModelId = "qwen3.5:9b-q4_K_M";
        var result = ContinuousLearningTp1DpoConfiguration.Create(
            installedModelId,
            ManifestDigest,
            ModelLayerDigest,
            maxTeacherCalls: 1,
            EvidenceRef('a'),
            EvidenceRef('b'),
            AuditSalt());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(installedModelId, result.Value!.ModelId);
        Assert.Equal(ManifestDigest, result.Value.ExpectedOllamaManifestDigest);
        Assert.Equal(ModelLayerDigest, result.Value.ExpectedOllamaModelLayerDigest);

        var authority = result.Value.CreateTeacherPolicyBudgetAuthority();
        Assert.True(authority.IsSuccess, authority.ErrorMessage);
        var admission = authority.Value!.ValidateBatch(
        [
            new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                installedModelId,
                ManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                RequestId: "installed-mixed-case-tag",
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = ModelLayerDigest
            }
        ]);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        Assert.Equal(1, admission.Value!.ExternalTeacherInvocationCount);
    }

    [Theory]
    [MemberData(nameof(OllamaModelIdGrammarCases))]
    public void Configuration_OllamaModelIdGrammarAdmitsSafeIdsAndRejectsUnsafeIds(
        string modelId,
        bool expectedSuccess)
    {
        Assert.Equal(
            expectedSuccess,
            ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(modelId));

        var result = ContinuousLearningTp1DpoConfiguration.Create(
            modelId,
            ManifestDigest,
            ModelLayerDigest,
            maxTeacherCalls: 1,
            EvidenceRef('a'),
            EvidenceRef('b'),
            AuditSalt());

        Assert.Equal(expectedSuccess, result.IsSuccess);
        if (!expectedSuccess)
        {
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
                result.ErrorCode);
            return;
        }

        Assert.Equal(modelId, result.Value!.ModelId);
        var authority = result.Value.CreateTeacherPolicyBudgetAuthority();
        Assert.True(authority.IsSuccess, authority.ErrorMessage);
    }

    public static IEnumerable<object[]> OllamaModelIdGrammarCases()
    {
        yield return ["qwen3.5:9b", true];
        yield return ["qwen-test:9b", true];
        yield return ["qwen/qwen3.5:Q4.K_M-1", true];
        yield return ["qwen3.5:9b-q4_K_M", true];
        yield return ["Qwen3.5:9b-q4_K_M", false];
        yield return ["qwen/Model:9b", false];
        yield return ["qwen3.5", false];
        yield return ["qwen3.5:", false];
        yield return ["qwen3.5:9b:q4_K_M", false];
        yield return [" qwen3.5:9b", false];
        yield return ["qwen3.5:9b ", false];
        yield return ["qwen3.5:9b\u0001", false];
        yield return ["qwen model:9b", false];
        yield return ["qwen\\model:9b", false];
        yield return ["qwen3.5?download=1:9b", false];
        yield return ["qwen3.5#fragment:9b", false];
        yield return ["https://qwen3.5:9b", false];
        yield return ["qwen/../qwen3.5:9b", false];
        yield return ["qwen/./qwen3.5:9b", false];
        yield return ["qwen//qwen3.5:9b", false];
        yield return ["/qwen3.5:9b", false];
        yield return ["qwen3.5/:9b", false];
        yield return ["mistral-test:7b", false];
        yield return ["deepseek-r1:7b", false];
        yield return ["qwen/openai-private:latest", false];
        yield return ["qwen/claude-compatible:latest", false];
    }

    [Theory]
    [InlineData("missing", LocalQwenOllamaTeacherInferenceClient.ModelIdentityUnavailableErrorCode)]
    [InlineData("duplicate", LocalQwenOllamaTeacherInferenceClient.ModelIdentityMismatchErrorCode)]
    [InlineData("conflicting", LocalQwenOllamaTeacherInferenceClient.ModelIdentityMismatchErrorCode)]
    [InlineData("wrong-digest", LocalQwenOllamaTeacherInferenceClient.ModelIdentityMismatchErrorCode)]
    [InlineData("malformed-json", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("wrong-content-type", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("non-success", LocalQwenOllamaTeacherInferenceClient.ModelIdentityUnavailableErrorCode)]
    [InlineData("oversize", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("too-deep", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("too-many", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("duplicate-critical", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    public async Task InferAsync_PreIdentityFailuresSpendZeroAndSuppressChat(
        string scenario,
        string expectedErrorCode)
    {
        var handler = new ScriptedHandler((request, cancellationToken) =>
            Task.FromResult(PreIdentityFailureResponse(scenario)));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.False(permit.CallIssued);
        Assert.Single(handler.Requests);
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.TagsEndpoint, handler.Requests[0].Uri);
    }

    [Theory]
    [InlineData("missing", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("duplicate", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("uppercase", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("malformed", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("ambiguous", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("commented-only", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("trailing-token", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("non-blob-absolute", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("blobs-not-immediate-parent", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("uppercase-blobs-parent", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("near-miss-blobs-parent", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("mixed-forward-backslash", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("mixed-backslash-forward", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("unc-blobs-as-server", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("unc-extra-leading-slash", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("unc-missing-share", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("unc-empty-component", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("unc-dot-path-component", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("unc-dotdot-share", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("unc-device-question", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("unc-device-dot", LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    [InlineData("wrong-layer", LocalQwenOllamaTeacherInferenceClient.ModelIdentityMismatchErrorCode)]
    public async Task InferAsync_PreShowIdentityFailuresSpendZeroAndSuppressChat(
        string scenario,
        string expectedErrorCode)
    {
        var handler = new ScriptedHandler((request, cancellationToken) =>
            request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ShowEndpoint
                ? Task.FromResult(ShowFailureResponse(scenario))
                : DefaultResponseAsync(request, cancellationToken));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.False(permit.CallIssued);
        Assert.Equal(
            new[] { "/api/tags", "/api/show" },
            handler.Requests.Select(static request => request.Uri.AbsolutePath));
        Assert.DoesNotContain(
            handler.Requests,
            static request => request.Uri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint);
    }

    [Theory]
    [InlineData("C:/Users/test/.ollama/models/blobs/sha256-")]
    [InlineData("/home/test/.ollama/models/blobs/sha256-")]
    [InlineData("\\\\server\\share\\blobs\\sha256-")]
    [InlineData("\\\\server\\ollama\\models\\blobs\\sha256-")]
    public async Task InferAsync_AcceptsCanonicalAbsoluteBlobLocatorForms(
        string locatorPrefix)
    {
        var handler = new ScriptedHandler((request, cancellationToken) =>
            request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ShowEndpoint
                ? Task.FromResult(JsonResponse(ShowJson(
                    ModelLayerDigest,
                    $"FROM {locatorPrefix}{ModelLayerDigest}")))
                : DefaultResponseAsync(request, cancellationToken));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(permit.CallIssued);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_CorrectLayerConfigurationDoesNotAuthorizeWrongManifest()
    {
        var handler = new ScriptedHandler((request, cancellationToken) =>
            request.RequestUri == LocalQwenOllamaTeacherInferenceClient.TagsEndpoint
                ? Task.FromResult(JsonResponse(IdentityJson(
                    ModelEntry(ModelId, new string('e', 64)))))
                : DefaultResponseAsync(request, cancellationToken));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.ModelIdentityMismatchErrorCode,
            result.ErrorCode);
        Assert.False(permit.CallIssued);
        Assert.Single(handler.Requests);
        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.TagsEndpoint,
            handler.Requests[0].Uri);
    }

    [Fact]
    public async Task InferAsync_PostShowLayerChangeSpendsOneAndWithholdsOutput()
    {
        var showCount = 0;
        var handler = new ScriptedHandler((request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ShowEndpoint)
            {
                var digest = Interlocked.Increment(ref showCount) == 1
                    ? ModelLayerDigest
                    : new string('e', 64);
                return Task.FromResult(JsonResponse(ShowJson(digest)));
            }
            return DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.ModelIdentityChangedErrorCode,
            result.ErrorCode);
        Assert.True(permit.CallIssued);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(6, handler.Requests.Count);
        Assert.Null(result.Value);
    }

    [Theory]
    [InlineData("ps-missing", 4, LocalQwenOllamaTeacherInferenceClient.ModelIdentityUnavailableErrorCode)]
    [InlineData("ps-mutated", 4, LocalQwenOllamaTeacherInferenceClient.ModelIdentityChangedErrorCode)]
    [InlineData("tags-mutated", 5, LocalQwenOllamaTeacherInferenceClient.ModelIdentityChangedErrorCode)]
    [InlineData("post-malformed", 4, LocalQwenOllamaTeacherInferenceClient.ModelIdentityResponseInvalidErrorCode)]
    public async Task InferAsync_PostIdentityFailureSpendsOneAndWithholdsOutput(
        string scenario,
        int expectedRequestCount,
        string expectedErrorCode)
    {
        var tagObservationCount = 0;
        var handler = new ScriptedHandler((request, cancellationToken) =>
        {
            var index = request.RequestUri!.AbsolutePath;
            if (index == "/api/chat")
                return Task.FromResult(JsonResponse(ChatJson()));
            if (index == "/api/show")
                return Task.FromResult(JsonResponse(ShowJson(ModelLayerDigest)));
            if (index == "/api/ps")
            {
                return Task.FromResult(scenario switch
                {
                    "ps-missing" => JsonResponse(IdentityJson(
                        ModelEntry("mistral:latest", new string('a', 64)))),
                    "ps-mutated" => JsonResponse(IdentityJson(
                        ModelEntry(ModelId, new string('e', 64)))),
                    "post-malformed" => JsonResponse("{"),
                    _ => JsonResponse(IdentityJson(ModelEntry(ModelId, ManifestDigest)))
                });
            }
            if (index == "/api/tags"
                && Interlocked.Increment(ref tagObservationCount) == 2
                && scenario == "tags-mutated")
            {
                return Task.FromResult(JsonResponse(IdentityJson(
                    ModelEntry(ModelId, new string('e', 64)))));
            }
            return Task.FromResult(JsonResponse(IdentityJson(
                ModelEntry(ModelId, ManifestDigest))));
        });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.True(permit.CallIssued);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(expectedRequestCount, handler.Requests.Count);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task InferAsync_ChatFailureSpendsOneAndRunsNoPostProbe()
    {
        var handler = new ScriptedHandler((request, cancellationToken) =>
            Task.FromResult(request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                ? JsonResponse("{}", HttpStatusCode.BadGateway)
                : request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ShowEndpoint
                    ? JsonResponse(ShowJson(ModelLayerDigest))
                : JsonResponse(IdentityJson(ModelEntry(ModelId, ManifestDigest)))));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.True(permit.CallIssued);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_RejectedPermitSuppressesChatAndPostProbes()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new TestCallPermit(allowIssue: false);
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.False(result.IsSuccess);
        Assert.Equal(TestCallPermit.RejectedErrorCode, result.ErrorCode);
        Assert.False(permit.CallIssued);
        AssertOnlyPrePermitIdentityProbes(handler.Requests);
    }

    [Fact]
    public async Task InferAsync_NullPermitResultFailsClosedWithoutChat()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new ProgrammablePermit(returnNull: true);
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.CallPermitRejectedErrorCode,
            result.ErrorCode);
        Assert.Equal(1, permit.TryIssueCount);
        Assert.Equal(0, permit.CallIssuedReadCount);
        AssertOnlyPrePermitIdentityProbes(handler.Requests);
    }

    [Fact]
    public async Task InferAsync_NonFatalThrowingTryIssueFailsClosedWithoutChat()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new ProgrammablePermit(
            tryIssueException: new InvalidOperationException("private permit detail"));
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.CallPermitRejectedErrorCode,
            result.ErrorCode);
        Assert.Equal(1, permit.TryIssueCount);
        Assert.Equal(0, permit.CallIssuedReadCount);
        AssertOnlyPrePermitIdentityProbes(handler.Requests);
    }

    [Fact]
    public async Task InferAsync_FatalThrowingTryIssuePassesThroughWithoutChat()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new ProgrammablePermit(
            tryIssueException: new OutOfMemoryException("fatal permit marker"));
        using var client = CreateClient(handler);

        await Assert.ThrowsAsync<OutOfMemoryException>(() =>
            client.InferAsync("prompt", permit));

        Assert.Equal(1, permit.TryIssueCount);
        Assert.Equal(0, permit.CallIssuedReadCount);
        AssertOnlyPrePermitIdentityProbes(handler.Requests);
    }

    [Fact]
    public async Task InferAsync_ThrowingCallIssuedGetterIsNotTouchedOnHotPath()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new ProgrammablePermit(throwFromCallIssued: true);
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, permit.TryIssueCount);
        Assert.Equal(0, permit.CallIssuedReadCount);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_SuccessIssuesPermitExactlyOnce()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_CallerCancellationBeforePrecheckSpendsZero()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new TestCallPermit();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var client = CreateClient(handler);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            client.InferAsync("prompt", permit, cancellation.Token));
        Assert.False(permit.CallIssued);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task InferAsync_CallerCancellationAfterPermitSpendsOne()
    {
        var chatEntered = NewSignal();
        var handler = new ScriptedHandler(async (request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint)
            {
                chatEntered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            return await DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        using var cancellation = new CancellationTokenSource();
        using var client = CreateClient(handler);

        var pending = client.InferAsync("prompt", permit, cancellation.Token);
        await chatEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.True(permit.CallIssued);
        Assert.Equal(1, permit.IssueCount);
    }

    [Theory]
    [InlineData(false, LocalQwenOllamaTeacherInferenceClient.TimeoutErrorCode, 0)]
    [InlineData(true, LocalQwenOllamaTeacherInferenceClient.TimeoutErrorCode, 1)]
    public async Task InferAsync_OneDeadlineCoversPrecheckAndPostPermit(
        bool blockChat,
        string expectedErrorCode,
        int expectedIssueCount)
    {
        var entered = NewSignal();
        var deadline = new ControlledDeadline();
        var handler = new ScriptedHandler(async (request, cancellationToken) =>
        {
            var shouldBlock = blockChat
                ? request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                : request.RequestUri == LocalQwenOllamaTeacherInferenceClient.TagsEndpoint;
            if (shouldBlock)
            {
                entered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            return await DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler, deadlineFactory: deadline.Create);

        var pending = client.InferAsync("prompt", permit);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        deadline.Trigger();
        var result = await pending;

        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Equal(expectedIssueCount, permit.IssueCount);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public async Task Dispose_CancelsWaiterOrHolderWithoutBlocking(
        bool blockChat,
        int expectedIssueCount)
    {
        var entered = NewSignal();
        var handler = new ScriptedHandler(async (request, cancellationToken) =>
        {
            var shouldBlock = blockChat
                ? request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                : request.RequestUri == LocalQwenOllamaTeacherInferenceClient.TagsEndpoint;
            if (shouldBlock)
            {
                entered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            return await DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        var client = CreateClient(handler);
        var pending = client.InferAsync("prompt", permit);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var dispose = Task.Run(client.Dispose);
        await dispose.WaitAsync(TimeSpan.FromSeconds(1));
        var result = await pending;

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.DisposedErrorCode, result.ErrorCode);
        Assert.Equal(expectedIssueCount, permit.IssueCount);
        Assert.Equal(1, handler.DisposeCount);
    }

    [Fact]
    public async Task InferAsync_ConcurrentSequencesNeverInterleave()
    {
        var firstChatEntered = NewSignal();
        var releaseFirstChat = NewSignal();
        var handler = new ScriptedHandler(async (request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                && !firstChatEntered.Task.IsCompleted)
            {
                firstChatEntered.TrySetResult();
                await releaseFirstChat.Task.WaitAsync(cancellationToken);
            }
            return await DefaultResponseAsync(request, cancellationToken);
        });
        var firstPermit = new TestCallPermit();
        var secondPermit = new TestCallPermit();
        using var client = CreateClient(handler);

        var first = client.InferAsync("first", firstPermit);
        await firstChatEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var second = client.InferAsync("second", secondPermit);
        await Task.Delay(50);
        Assert.Equal(3, handler.Requests.Count);
        releaseFirstChat.TrySetResult();
        var results = await Task.WhenAll(first, second);

        Assert.All(results, static result => Assert.True(result.IsSuccess, result.ErrorMessage));
        Assert.Equal(
            new[] { "/api/tags", "/api/show", "/api/chat", "/api/ps",
                    "/api/tags", "/api/show", "/api/tags", "/api/show",
                    "/api/chat", "/api/ps", "/api/tags", "/api/show" },
            handler.Requests.Select(static request => request.Uri.AbsolutePath));
    }

    [Fact]
    public async Task InferAsync_TransportDiagnosticsAreSanitizedAndNotRetried()
    {
        const string privatePrompt = "PRIVATE-prompt";
        const string privateBody = "PRIVATE-body";
        var handler = new ScriptedHandler((request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint)
                throw new HttpRequestException(privateBody + ModelId + ManifestDigest);
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ShowEndpoint)
                return Task.FromResult(JsonResponse(ShowJson(ModelLayerDigest)));
            return Task.FromResult(JsonResponse(IdentityJson(
                ModelEntry(ModelId, ManifestDigest))));
        });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync(privatePrompt, permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.TransportFailedErrorCode, result.ErrorCode);
        Assert.Equal(3, handler.Requests.Count);
        Assert.DoesNotContain(privatePrompt, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateBody, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(ModelId, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(ManifestDigest, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InferAsync_SuccessReturnsOutputAndExactLegalProvenance()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(Output, result.Value!.Output);
        var provenance = result.Value.TrainingTeacherProvenance;
        Assert.Equal(TrainingTeacherSourceKind.ExternalTeacherOutput, provenance.SourceKind);
        Assert.Equal(OpenWeightTeacherFamily.Qwen, provenance.Family);
        Assert.Equal(TrainingTeacherAccessMode.LocalHostedOpenWeight, provenance.AccessMode);
        Assert.Equal(TrainingTeacherHostingProvider.Ollama, provenance.HostingProvider);
        Assert.Equal(ModelId, provenance.ModelId);
        Assert.Equal(ManifestDigest, provenance.ModelVersion);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            provenance.ModelArtifactDigestKind);
        Assert.Equal(ModelLayerDigest, provenance.ModelArtifactDigest);
        Assert.Equal(TrainingTeacherModelLicense.Apache20, provenance.ModelLicense);
        Assert.False(provenance.ProviderTermsChecked);
        Assert.False(provenance.ProviderTermsAllowTraining);
        Assert.False(provenance.ClosedProprietaryModel);
        Assert.True(provenance.TeacherOutputUsedForTraining);
        Assert.Equal(1, provenance.InvocationCount);
        Assert.Matches("^[0-9a-f]{32}$", provenance.RequestId);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(6, handler.Requests.Count);

        var admission = CreateConfiguration().CreateTeacherPolicyBudgetAuthority().Value!
            .ValidateBatch([provenance]);
        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        Assert.Equal(1, admission.Value!.ExternalTeacherInvocationCount);
    }

    [Fact]
    public void Create_IllegalTeacherIdentityFailsBeforeAnyHandlerUse()
    {
        var handler = new ScriptedHandler((_, _) =>
            throw new InvalidOperationException("Handler must not run."));
        var configuration = ContinuousLearningTp1DpoConfiguration.Create(
            "openai-private-model:latest",
            ManifestDigest,
            ModelLayerDigest,
            1,
            EvidenceRef('a'),
            EvidenceRef('b'),
            AuditSalt());
        var nullConfigurationClient =
            LocalQwenOllamaTeacherInferenceClient.CreateForTesting(
                null!,
                handler,
                disposeHandler: false);

        Assert.False(configuration.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            configuration.ErrorCode);
        Assert.False(nullConfigurationClient.IsSuccess);
        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.ConfigurationInvalidErrorCode,
            nullConfigurationClient.ErrorCode);
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InferAsync_EmptyPromptFailsBeforeHandler(string prompt)
    {
        var handler = new ScriptedHandler((_, _) =>
            throw new InvalidOperationException("Handler must not run."));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync(prompt, permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.PromptInvalidErrorCode, result.ErrorCode);
        Assert.False(permit.CallIssued);
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData("\u0001")]
    [InlineData("safe\u007Ftext")]
    public async Task InferAsync_DisallowedPromptTextFailsBeforeHandler(string prompt)
    {
        var handler = new ScriptedHandler((_, _) =>
            throw new InvalidOperationException("Handler must not run."));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync(prompt, permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.PromptInvalidErrorCode, result.ErrorCode);
        Assert.False(permit.CallIssued);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task InferAsync_UnpairedSurrogateFailsBeforeHandler()
    {
        var handler = new ScriptedHandler((_, _) =>
            throw new InvalidOperationException("Handler must not run."));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);
        var prompt = new string('\uD800', 1);

        var result = await client.InferAsync(prompt, permit);

        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.PromptInvalidErrorCode,
            result.ErrorCode);
        Assert.False(permit.CallIssued);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task InferAsync_InvalidPromptDoesNotConsumeBudget()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var invalid = await client.InferAsync("embedded\u0009control", permit);
        var valid = await client.InferAsync("valid", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.PromptInvalidErrorCode, invalid.ErrorCode);
        Assert.True(valid.IsSuccess, valid.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_PromptLimitBoundarySucceeds()
    {
        var handler = new ScriptedHandler(DefaultResponseAsync);
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync(ExactPromptBoundary(), permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_PromptLimitPlusOneFailsBeforeHandler()
    {
        var handler = new ScriptedHandler((_, _) =>
            throw new InvalidOperationException("Handler must not run."));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync(ExactPromptBoundary() + "a", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.RequestTooLargeErrorCode, result.ErrorCode);
        Assert.False(permit.CallIssued);
        Assert.Empty(handler.Requests);
    }

    [Theory]
    [MemberData(nameof(InvalidResponseCases))]
    public async Task InferAsync_RejectsInvalidOllamaResponses(
        string _,
        string responseJson)
    {
        var handler = ChatResponseHandler(() => JsonResponse(responseJson));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
        Assert.Null(result.Value);
    }

    public static IEnumerable<object[]> InvalidResponseCases()
    {
        yield return ["malformed", "not-json"];
        yield return ["truncated", "{\"model\":\"qwen3.5:9b\""];
        yield return ["done-false", ChatJson(done: false)];
        yield return ["blank-content", ChatJson(content: "   ")];
        yield return ["wrong-role", ChatJson(role: "user")];
        yield return ["model-mismatch", ChatJson(modelId: "qwen-other:9b")];
        yield return ["wrong-types", "{\"model\":17,\"done\":\"true\",\"message\":[]}"];
        yield return
        [
            "duplicate-model",
            "{\"model\":\"qwen3.5:9b\",\"model\":\"qwen3.5:9b\"," +
            "\"done\":true,\"message\":{\"role\":\"assistant\",\"content\":\"ok\"}}"
        ];
        yield return ["extra-json", ChatJson() + "{}"];
        yield return
        [
            "trailing-comma",
            "{\"model\":\"qwen3.5:9b\",\"message\":{" +
            "\"role\":\"assistant\",\"content\":\"ok\"},\"done\":true,}"
        ];
    }

    [Fact]
    public async Task InferAsync_AcceptsExactMaximumJsonDepth()
    {
        var handler = ChatResponseHandler(() => JsonResponse(
            ChatJsonWithUnknownArrayDepth(
                LocalQwenOllamaTeacherInferenceClient.MaximumJsonDepth - 1)));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_RejectsMaximumJsonDepthPlusOne()
    {
        var handler = ChatResponseHandler(() => JsonResponse(
            ChatJsonWithUnknownArrayDepth(
                LocalQwenOllamaTeacherInferenceClient.MaximumJsonDepth)));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Redirect)]
    public async Task InferAsync_RejectsNonSuccessAndRedirectWithoutRetry(
        HttpStatusCode statusCode)
    {
        var handler = ChatResponseHandler(() => JsonResponse(ChatJson(), statusCode));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_RejectsMissingContentType()
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(ChatJson()));
        var handler = ChatResponseHandler(() =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_RejectsWrongContentType()
    {
        var handler = ChatResponseHandler(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(ChatJson(), Encoding.UTF8, "text/plain")
        });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_RejectsNonUtf8Charset()
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(ChatJson()));
        content.Headers.ContentType = new("application/json") { CharSet = "iso-8859-1" };
        var handler = ChatResponseHandler(() =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_RejectsInvalidResponseUtf8()
    {
        var handler = ChatResponseHandler(() => ByteResponse(new byte[] { 0xC3, 0x28 }));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_DeclaredResponseLimitBoundarySucceeds()
    {
        var handler = ChatResponseHandler(() => ByteResponse(PaddedChatResponse(
            LocalQwenOllamaTeacherInferenceClient.MaximumResponseBodyBytes)));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_DeclaredResponseLimitPlusOneFailsBeforeRead()
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(ChatJson()));
        content.Headers.ContentLength =
            LocalQwenOllamaTeacherInferenceClient.MaximumResponseBodyBytes + 1L;
        content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        var handler = ChatResponseHandler(() =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseTooLargeErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(-1)]
    public async Task InferAsync_RejectsDeclaredLengthMismatch(int delta)
    {
        var bytes = Encoding.UTF8.GetBytes(ChatJson());
        var handler = ChatResponseHandler(() =>
            ByteResponse(new DeclaredLengthContent(bytes, bytes.Length + delta)));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_StreamedResponseLimitBoundarySucceeds()
    {
        var handler = ChatResponseHandler(() => ByteResponse(new UnknownLengthContent(
            PaddedChatResponse(LocalQwenOllamaTeacherInferenceClient.MaximumResponseBodyBytes))));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_StreamedResponseLimitPlusOneFails()
    {
        var handler = ChatResponseHandler(() => ByteResponse(new UnknownLengthContent(
            PaddedChatResponse(LocalQwenOllamaTeacherInferenceClient.MaximumResponseBodyBytes + 1))));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseTooLargeErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_DisposesResponseStreamAfterRead()
    {
        var stream = new TrackingMemoryStream(Encoding.UTF8.GetBytes(ChatJson()));
        var handler = ChatResponseHandler(() => ByteResponse(new TrackingStreamContent(stream)));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(stream.IsDisposed);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(6, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_PreCancelledCallerTokenIsPropagatedBeforeHandler()
    {
        var handler = new ScriptedHandler((_, _) =>
            throw new InvalidOperationException("Handler must not run."));
        var permit = new TestCallPermit();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var client = CreateClient(handler);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.InferAsync("prompt", permit, cancellation.Token));

        Assert.False(permit.CallIssued);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task InferAsync_CallerCancellationDuringSendIsPropagated()
    {
        var chatEntered = NewSignal();
        var handler = new ScriptedHandler(async (request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint)
            {
                chatEntered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            return await DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        using var cancellation = new CancellationTokenSource();
        using var client = CreateClient(handler);

        var pending = client.InferAsync("prompt", permit, cancellation.Token);
        await chatEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task Dispose_ActiveRequestReturnsDisposedInsteadOfTimeout()
    {
        var chatEntered = NewSignal();
        var deadline = new ControlledDeadline();
        var handler = new ScriptedHandler(async (request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint)
            {
                chatEntered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            return await DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        var client = CreateClient(handler, deadlineFactory: deadline.Create);

        var pending = client.InferAsync("prompt", permit);
        await chatEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        client.Dispose();
        var result = await pending.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.DisposedErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task Dispose_DefersOwnedCleanupUntilBothRequestLeasesExit()
    {
        var entered = NewSignal();
        var cancellationObserved = NewSignal();
        var release = NewSignal();
        var handler = new ScriptedHandler(async (request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.TagsEndpoint)
            {
                entered.TrySetResult();
                using var registration = cancellationToken.Register(
                    () => cancellationObserved.TrySetResult());
                await cancellationObserved.Task;
                await release.Task;
                throw new OperationCanceledException(cancellationToken);
            }
            return await DefaultResponseAsync(request, cancellationToken);
        });
        var firstPermit = new TestCallPermit();
        var secondPermit = new TestCallPermit();
        var client = CreateClient(handler, disposeHandler: true);

        var first = client.InferAsync("first", firstPermit);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var second = client.InferAsync("second", secondPermit);
        client.Dispose();
        await cancellationObserved.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var secondResult = await second.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.DisposedErrorCode, secondResult.ErrorCode);
        Assert.Equal(0, handler.DisposeCount);
        Assert.False(firstPermit.CallIssued);
        Assert.False(secondPermit.CallIssued);

        release.TrySetResult();
        var firstResult = await first.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.DisposedErrorCode, firstResult.ErrorCode);
        Assert.Equal(1, handler.DisposeCount);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Dispose_DoesNotWaitForHandlerSynchronousPrefix()
    {
        var handler = new SynchronousPrefixHandler();
        var permit = new TestCallPermit();
        var client = CreateClient(handler, disposeHandler: true);
        var pending = Task.Run(() => client.InferAsync("prompt", permit));

        Assert.True(handler.Entered.Wait(TimeSpan.FromSeconds(2)));
        var dispose = Task.Run(client.Dispose);
        try
        {
            await dispose.WaitAsync(TimeSpan.FromSeconds(2));
            Assert.Equal(0, handler.DisposeCount);
            Assert.False(permit.CallIssued);
        }
        finally
        {
            handler.Release.Set();
        }

        var result = await pending.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.DisposedErrorCode, result.ErrorCode);
        Assert.Equal(1, handler.DisposeCount);
    }

    [Fact]
    public async Task InferAsync_CallerCancellationHasPriorityOverDisposal()
    {
        var chatEntered = NewSignal();
        var release = NewSignal();
        var handler = BlockingCancellationHandler(chatEntered, release);
        var permit = new TestCallPermit();
        var client = CreateClient(handler);
        using var caller = new CancellationTokenSource();

        var pending = client.InferAsync("prompt", permit, caller.Token);
        await chatEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        caller.Cancel();
        client.Dispose();
        release.TrySetResult();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_DisposalHasPriorityOverExpiredDeadline()
    {
        var deadline = new ControlledDeadline();
        var chatEntered = NewSignal();
        var cancellationObserved = NewSignal();
        var release = NewSignal();
        var handler = BlockingCancellationHandler(
            chatEntered,
            release,
            cancellationObserved);
        var permit = new TestCallPermit();
        var client = CreateClient(handler, deadlineFactory: deadline.Create);

        var pending = client.InferAsync("prompt", permit);
        await chatEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        deadline.Trigger();
        await cancellationObserved.Task.WaitAsync(TimeSpan.FromSeconds(2));
        client.Dispose();
        release.TrySetResult();
        var result = await pending.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.DisposedErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_CallerHasPriorityOverDisposalAndDeadline()
    {
        var deadline = new ControlledDeadline();
        var chatEntered = NewSignal();
        var cancellationObserved = NewSignal();
        var release = NewSignal();
        var handler = BlockingCancellationHandler(
            chatEntered,
            release,
            cancellationObserved);
        var permit = new TestCallPermit();
        var client = CreateClient(handler, deadlineFactory: deadline.Create);
        using var caller = new CancellationTokenSource();

        var pending = client.InferAsync("prompt", permit, caller.Token);
        await chatEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        deadline.Trigger();
        await cancellationObserved.Task.WaitAsync(TimeSpan.FromSeconds(2));
        caller.Cancel();
        client.Dispose();
        release.TrySetResult();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_InternalDeadlineReturnsSanitizedTimeout()
    {
        var deadline = new ControlledDeadline();
        var chatEntered = NewSignal();
        var handler = new ScriptedHandler(async (request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint)
            {
                chatEntered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            return await DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler, deadlineFactory: deadline.Create);

        var pending = client.InferAsync("prompt", permit);
        await chatEntered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        deadline.Trigger();
        var result = await pending.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.TimeoutErrorCode, result.ErrorCode);
        Assert.Equal("The local teacher request timed out.", result.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_TransportFailureIsSanitizedAndAttemptedOnce()
    {
        const string privateTransportText = "PRIVATE-transport-detail";
        var handler = new ScriptedHandler((request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint)
                throw new HttpRequestException(privateTransportText);
            return DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("PRIVATE-prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.TransportFailedErrorCode, result.ErrorCode);
        Assert.Equal("The local teacher request failed.", result.ErrorMessage);
        Assert.DoesNotContain(privateTransportText, result.ErrorMessage);
        Assert.DoesNotContain("PRIVATE-prompt", result.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_UnrelatedTransportCancellationIsSanitized()
    {
        var handler = new ScriptedHandler((request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint)
                throw new OperationCanceledException("unrelated cancellation");
            return DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.TransportFailedErrorCode, result.ErrorCode);
        Assert.Equal("The local teacher request failed.", result.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_FatalTransportExceptionPassesThrough()
    {
        var handler = new ScriptedHandler((request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint)
                throw new OutOfMemoryException("fatal marker");
            return DefaultResponseAsync(request, cancellationToken);
        });
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        await Assert.ThrowsAsync<OutOfMemoryException>(() =>
            client.InferAsync("prompt", permit));

        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_InvalidResponseDoesNotRetry()
    {
        var handler = ChatResponseHandler(() => JsonResponse("not-json"));
        var permit = new TestCallPermit();
        using var client = CreateClient(handler);

        var result = await client.InferAsync("prompt", permit);

        Assert.False(result.IsSuccess);
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.ResponseInvalidErrorCode, result.ErrorCode);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task InferAsync_ErrorsRedactPromptBodyModelAndRequestIdentity()
    {
        const string privatePrompt = "PRIVATE-prompt-marker";
        const string privateBody = "PRIVATE-response-body-marker";
        const string privateModel = "qwen-private-model-marker:9b";
        var privateDigest = new string('e', 64);
        var handler = new ScriptedHandler((request, _) => Task.FromResult(
            request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                ? JsonResponse(privateBody)
                : request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ShowEndpoint
                    ? JsonResponse(ShowJson(ModelLayerDigest))
                : JsonResponse(IdentityJson(ModelEntry(privateModel, privateDigest)))));
        var permit = new TestCallPermit();
        using var client = CreateClient(
            handler,
            configuration: CreateConfiguration(privateModel, privateDigest));

        var result = await client.InferAsync(privatePrompt, permit);

        Assert.False(result.IsSuccess);
        Assert.DoesNotContain(privatePrompt, result.ErrorMessage);
        Assert.DoesNotContain(privateBody, result.ErrorMessage);
        Assert.DoesNotContain(privateModel, result.ErrorMessage);
        Assert.DoesNotContain(privateDigest, result.ErrorMessage);
        Assert.Equal(1, permit.IssueCount);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public void CreateForTesting_DoesNotSendAndDoesNotOwnInjectedHandlerByDefault()
    {
        var handler = new ScriptedHandler((_, _) =>
            throw new InvalidOperationException("Handler must not run."));
        var created = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(
            CreateConfiguration(),
            handler,
            disposeHandler: false);

        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.Empty(handler.Requests);
        created.Value!.Dispose();
        created.Value.Dispose();
        Assert.Equal(0, handler.DisposeCount);
    }

    [Fact]
    public void Dispose_DisposesInjectedHandlerOnlyWhenOwnershipIsExplicit()
    {
        var unowned = new ScriptedHandler(DefaultResponseAsync);
        var owned = new ScriptedHandler(DefaultResponseAsync);
        var unownedClient = CreateClient(unowned, disposeHandler: false);
        var ownedClient = CreateClient(owned, disposeHandler: true);

        unownedClient.Dispose();
        ownedClient.Dispose();
        unownedClient.Dispose();
        ownedClient.Dispose();

        Assert.Equal(0, unowned.DisposeCount);
        Assert.Equal(1, owned.DisposeCount);
    }

    [Fact]
    public async Task InferAsync_AfterDisposeFailsWithoutHandlerUse()
    {
        var handler = new ScriptedHandler((_, _) =>
            throw new InvalidOperationException("Handler must not run."));
        var permit = new TestCallPermit();
        var client = CreateClient(handler);
        client.Dispose();

        var result = await client.InferAsync("prompt", permit);

        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.DisposedErrorCode, result.ErrorCode);
        Assert.False(permit.CallIssued);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public void SecureHandler_HasAllRequiredFixedNetworkFlags()
    {
        using var handler =
            LocalQwenOllamaTeacherInferenceClient.CreateSecureSocketsHttpHandler();

        Assert.False(handler.UseProxy);
        Assert.Null(handler.Proxy);
        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseCookies);
        Assert.Null(handler.Credentials);
        Assert.Null(handler.DefaultProxyCredentials);
        Assert.Equal(DecompressionMethods.None, handler.AutomaticDecompression);
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.DefaultConnectTimeout, handler.ConnectTimeout);
        Assert.Equal(LocalQwenOllamaTeacherInferenceClient.MaximumConnectionsPerServer, handler.MaxConnectionsPerServer);
        Assert.True(handler.PooledConnectionLifetime > TimeSpan.Zero);
        Assert.True(handler.PooledConnectionIdleTimeout > TimeSpan.Zero);
    }

    [Fact]
    public async Task EvaluateAsync_UsesSeparateTwoMinuteDeadlineWithoutChangingAuthoringDeadline()
    {
        var deadlines = new List<TimeSpan>();
        var handler = new ScriptedHandler(DefaultResponseAsync);
        using var client = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(
            CreateConfiguration(),
            handler,
            disposeHandler: true,
            requestTimeout: TimeSpan.FromSeconds(30),
            deadlineFactory: timeout =>
            {
                deadlines.Add(timeout);
                return new CancellationTokenSource();
            },
            evaluationRequestTimeout: TimeSpan.FromSeconds(120)).Value!;

        var authoring = await client.InferAsync("authoring", new TestCallPermit());
        var evaluation = await client.EvaluateAsync("evaluation", new TestCallPermit());

        Assert.True(authoring.IsSuccess, authoring.ErrorMessage);
        Assert.True(evaluation.IsSuccess, evaluation.ErrorMessage);
        Assert.Equal(
            [TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(120)],
            deadlines);
    }

    [Fact]
    public void InferenceSurface_RequiresSeparatePermitBoundTrainingAndEvaluationContracts()
    {
        var interfaceMethod = Assert.Single(
            typeof(ITrainingTeacherInferenceClient).GetMethods(),
            static method => method.Name == nameof(ITrainingTeacherInferenceClient.InferAsync));
        Assert.Equal(
            new[] { typeof(string), typeof(ITrainingTeacherCallPermit), typeof(CancellationToken) },
            interfaceMethod.GetParameters().Select(static parameter => parameter.ParameterType));
        Assert.DoesNotContain(
            typeof(LocalQwenOllamaTeacherInferenceClient).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            static method => method.Name == nameof(LocalQwenOllamaTeacherInferenceClient.InferAsync)
                && method.GetParameters().All(static parameter =>
                    parameter.ParameterType != typeof(ITrainingTeacherCallPermit)));

        var evaluationInterfaceMethod = Assert.Single(
            typeof(ILegalTeacherEvaluationInferenceClient).GetMethods(),
            static method => method.Name == nameof(ILegalTeacherEvaluationInferenceClient.EvaluateAsync));
        Assert.Equal(
            new[] { typeof(string), typeof(ILegalTeacherCallPermit), typeof(CancellationToken) },
            evaluationInterfaceMethod.GetParameters().Select(static parameter => parameter.ParameterType));
        Assert.Contains(
            typeof(ILegalTeacherEvaluationInferenceClient),
            typeof(LocalQwenOllamaTeacherInferenceClient).GetInterfaces());
        Assert.DoesNotContain(
            typeof(LocalQwenOllamaTeacherInferenceClient).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            static method => method.Name == nameof(LocalQwenOllamaTeacherInferenceClient.EvaluateAsync)
                && method.GetParameters().All(static parameter =>
                    parameter.ParameterType != typeof(ILegalTeacherCallPermit)));
        Assert.DoesNotContain(
            typeof(LocalQwenOllamaTeacherInferenceClient).GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            static method => (method.Name == nameof(LocalQwenOllamaTeacherInferenceClient.InferAsync)
                    || method.Name == nameof(LocalQwenOllamaTeacherInferenceClient.EvaluateAsync))
                && method.GetParameters().Any(static parameter =>
                    parameter.ParameterType == typeof(bool)
                    || parameter.ParameterType.IsEnum));
    }

    [Fact]
    public void SecureHandler_HasFixedLoopbackTransportFlags()
    {
        using var handler =
            LocalQwenOllamaTeacherInferenceClient.CreateSecureSocketsHttpHandler();

        Assert.False(handler.UseProxy);
        Assert.Null(handler.Proxy);
        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseCookies);
        Assert.Null(handler.Credentials);
        Assert.Null(handler.DefaultProxyCredentials);
        Assert.Equal(DecompressionMethods.None, handler.AutomaticDecompression);
        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.MaximumConnectionsPerServer,
            handler.MaxConnectionsPerServer);
    }

    private static string HashProfile(
        string domain,
        bool bigEndian,
        bool reorderPurpose)
    {
        var components = reorderPurpose
            ? new[]
            {
                domain,
                "profile_id", "qwen35-local-authoring-v1",
                "format", "json",
                "purpose", "training_authoring",
                "num_ctx", "8192",
                "temperature", "0",
                "seed", "0"
            }
            : new[]
            {
                domain,
                "profile_id", "qwen35-local-authoring-v1",
                "purpose", "training_authoring",
                "format", "json",
                "num_ctx", "8192",
                "temperature", "0",
                "seed", "0"
            };
        using var stream = new MemoryStream();
        Span<byte> length = stackalloc byte[sizeof(long)];
        foreach (var component in components)
        {
            var bytes = Encoding.UTF8.GetBytes(component);
            if (bigEndian)
                BinaryPrimitives.WriteInt64BigEndian(length, bytes.LongLength);
            else
                BinaryPrimitives.WriteInt64LittleEndian(length, bytes.LongLength);
            stream.Write(length);
            stream.Write(bytes);
        }
        return Convert.ToHexString(SHA256.HashData(stream.ToArray()))
            .ToLowerInvariant();
    }

    private static LocalQwenOllamaTeacherInferenceClient CreateClient(
        HttpMessageHandler handler,
        bool disposeHandler = true,
        Func<TimeSpan, CancellationTokenSource>? deadlineFactory = null,
        ContinuousLearningTp1DpoConfiguration? configuration = null)
    {
        var created = LocalQwenOllamaTeacherInferenceClient.CreateForTesting(
            configuration ?? CreateConfiguration(),
            handler,
            disposeHandler,
            requestTimeout: TimeSpan.FromSeconds(30),
            deadlineFactory);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }

    private static ContinuousLearningTp1DpoConfiguration CreateConfiguration(
        string modelId = ModelId,
        string? manifestDigest = null)
    {
        var result = ContinuousLearningTp1DpoConfiguration.Create(
            modelId,
            manifestDigest ?? ManifestDigest,
            ModelLayerDigest,
            maxTeacherCalls: 100,
            EvidenceRef('a'),
            EvidenceRef('b'),
            AuditSalt());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static byte[] AuditSalt()
        => Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray();

    private static string EvidenceRef(char value)
        => $"sha256:{new string(value, 64)}";

    private static string ExactPromptBoundary()
    {
        var prompt = new string('\u20AC', 21_845) + "a";
        Assert.Equal(
            LocalQwenOllamaTeacherInferenceClient.MaximumPromptUtf8Bytes,
            Encoding.UTF8.GetByteCount(prompt));
        return prompt;
    }

    private static void AssertOnlyPrePermitIdentityProbes(
        IReadOnlyList<RequestSnapshot> requests)
    {
        Assert.Collection(
            requests,
            tags =>
            {
                Assert.Equal(HttpMethod.Get, tags.Method);
                Assert.Equal(
                    LocalQwenOllamaTeacherInferenceClient.TagsEndpoint,
                    tags.Uri);
                Assert.Null(tags.Body);
            },
            show =>
            {
                Assert.Equal(HttpMethod.Post, show.Method);
                Assert.Equal(
                    LocalQwenOllamaTeacherInferenceClient.ShowEndpoint,
                    show.Uri);
                Assert.Equal("application/json", show.ContentType);
                Assert.Equal("utf-8", show.CharSet);
                using var body = JsonDocument.Parse(show.Body!);
                Assert.Equal(JsonValueKind.Object, body.RootElement.ValueKind);
                var property = Assert.Single(
                    body.RootElement.EnumerateObject());
                Assert.Equal("model", property.Name);
                Assert.Equal(JsonValueKind.String, property.Value.ValueKind);
                Assert.Equal(
                    ModelId,
                    property.Value.GetString());
            });
        Assert.DoesNotContain(
            requests,
            static request => request.Uri ==
                LocalQwenOllamaTeacherInferenceClient.ChatEndpoint);
        Assert.DoesNotContain(
            requests,
            static request => request.Uri ==
                LocalQwenOllamaTeacherInferenceClient.RunningModelsEndpoint);
    }

    private static Task<HttpResponseMessage> DefaultResponseAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        => Task.FromResult(request.RequestUri switch
        {
            var uri when uri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                => JsonResponse(ChatJson()),
            var uri when uri == LocalQwenOllamaTeacherInferenceClient.ShowEndpoint
                => JsonResponse(ShowJson(ModelLayerDigest)),
            _ => JsonResponse(IdentityJson(ModelEntry(ModelId, ManifestDigest)))
        });

    private static string ChatJson(
        string modelId = ModelId,
        string role = "assistant",
        string content = Output,
        bool done = true)
        => JsonSerializer.Serialize(new
        {
            model = modelId,
            message = new { role, content },
            done
        });

    private static string ChatJsonWithUnknownArrayDepth(int arrayDepth)
        => "{\"model\":\"qwen3.5:9b\",\"message\":{" +
           "\"role\":\"assistant\",\"content\":\"bounded teacher output\"}," +
           "\"done\":true,\"unknown\":" +
           new string('[', arrayDepth) + "0" + new string(']', arrayDepth) + "}";

    private static object ModelEntry(string modelId, string digest)
        => new { name = modelId, model = modelId, digest };

    private static string IdentityJson(params object[] models)
        => JsonSerializer.Serialize(new { models });

    private static string ShowJson(
        string modelLayerDigest,
        string? modelfile = null)
        => JsonSerializer.Serialize(new
        {
            modelfile = modelfile
                ?? $"FROM C:\\Users\\test\\.ollama\\models\\blobs\\sha256-{modelLayerDigest}\n"
        });

    private static ScriptedHandler ChatResponseHandler(
        Func<HttpResponseMessage> chatResponse)
        => new((request, cancellationToken) =>
            request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint
                ? Task.FromResult(chatResponse())
                : DefaultResponseAsync(request, cancellationToken));

    private static ScriptedHandler BlockingCancellationHandler(
        TaskCompletionSource chatEntered,
        TaskCompletionSource release,
        TaskCompletionSource? cancellationObserved = null)
        => new(async (request, cancellationToken) =>
        {
            if (request.RequestUri == LocalQwenOllamaTeacherInferenceClient.ChatEndpoint)
            {
                chatEntered.TrySetResult();
                using var registration = cancellationToken.Register(
                    () => cancellationObserved?.TrySetResult());
                await release.Task;
                throw new OperationCanceledException(cancellationToken);
            }
            return await DefaultResponseAsync(request, cancellationToken);
        });

    private static HttpResponseMessage PreIdentityFailureResponse(string scenario)
        => scenario switch
        {
            "missing" => JsonResponse(IdentityJson(
                ModelEntry("mistral:latest", new string('a', 64)))),
            "duplicate" => JsonResponse(IdentityJson(
                ModelEntry(ModelId, ManifestDigest),
                ModelEntry(ModelId, ManifestDigest))),
            "conflicting" => JsonResponse(JsonSerializer.Serialize(new
            {
                models = new[]
                {
                    new { name = ModelId, model = "qwen3.5:other", digest = ManifestDigest }
                }
            })),
            "wrong-digest" => JsonResponse(IdentityJson(
                ModelEntry(ModelId, new string('e', 64)))),
            "malformed-json" => JsonResponse("{"),
            "wrong-content-type" => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(IdentityJson(ModelEntry(ModelId, ManifestDigest)))
            },
            "non-success" => JsonResponse("{}", HttpStatusCode.ServiceUnavailable),
            "oversize" => ByteResponse(new byte[
                LocalQwenOllamaTeacherInferenceClient.MaximumResponseBodyBytes + 1]),
            "too-deep" => JsonResponse("{\"models\":[{\"name\":\"qwen3.5:9b\","
                + "\"model\":\"qwen3.5:9b\",\"digest\":\"" + ManifestDigest
                + "\",\"unknown\":" + new string('[', 20) + "0" + new string(']', 20)
                + "}]}"),
            "too-many" => JsonResponse(IdentityJson(
                Enumerable.Range(0, LocalQwenOllamaTeacherInferenceClient.MaximumModelEntries + 1)
                    .Select(index => ModelEntry($"mistral:{index}", new string('a', 64)))
                    .ToArray())),
            "duplicate-critical" => JsonResponse(
                "{\"models\":[{\"name\":\"qwen3.5:9b\","
                + "\"name\":\"qwen3.5:9b\",\"model\":\"qwen3.5:9b\","
                + "\"digest\":\"" + ManifestDigest + "\"}]}"),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

    private static HttpResponseMessage ShowFailureResponse(string scenario)
        => scenario switch
        {
            "missing" => JsonResponse("{}"),
            "duplicate" => JsonResponse(
                "{\"modelfile\":\"FROM C:\\\\one\\\\blobs\\\\sha256-" + ModelLayerDigest
                + "\",\"modelfile\":\"FROM C:\\\\two\\\\blobs\\\\sha256-" + ModelLayerDigest + "\"}"),
            "uppercase" => JsonResponse(ShowJson(ModelLayerDigest.ToUpperInvariant())),
            "malformed" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM relative/blobs/sha256-{ModelLayerDigest}")),
            "ambiguous" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM C:\\one\\blobs\\sha256-{ModelLayerDigest}\n"
                + $"FROM C:\\two\\blobs\\sha256-{ModelLayerDigest}")),
            "commented-only" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"# FROM C:\\one\\blobs\\sha256-{ModelLayerDigest}")),
            "trailing-token" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM C:\\one\\blobs\\sha256-{ModelLayerDigest} extra")),
            "non-blob-absolute" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM C:\\one\\sha256-{ModelLayerDigest}")),
            "blobs-not-immediate-parent" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM C:\\one\\blobs\\nested\\sha256-{ModelLayerDigest}")),
            "uppercase-blobs-parent" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM C:\\one\\BLOBS\\sha256-{ModelLayerDigest}")),
            "near-miss-blobs-parent" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM C:\\one\\blobs-near\\sha256-{ModelLayerDigest}")),
            "mixed-forward-backslash" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM C:/one/blobs\\sha256-{ModelLayerDigest}")),
            "mixed-backslash-forward" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM C:\\one\\blobs/sha256-{ModelLayerDigest}")),
            "unc-blobs-as-server" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM \\\\blobs\\sha256-{ModelLayerDigest}")),
            "unc-extra-leading-slash" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM \\\\\\blobs\\sha256-{ModelLayerDigest}")),
            "unc-missing-share" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM \\\\server\\blobs\\sha256-{ModelLayerDigest}")),
            "unc-empty-component" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM \\\\server\\\\share\\blobs\\sha256-{ModelLayerDigest}")),
            "unc-dot-path-component" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM \\\\server\\share\\.\\blobs\\sha256-{ModelLayerDigest}")),
            "unc-dotdot-share" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM \\\\server\\..\\blobs\\sha256-{ModelLayerDigest}")),
            "unc-device-question" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM \\\\?\\C:\\models\\blobs\\sha256-{ModelLayerDigest}")),
            "unc-device-dot" => JsonResponse(ShowJson(
                ModelLayerDigest,
                $"FROM \\\\.\\C:\\models\\blobs\\sha256-{ModelLayerDigest}")),
            "wrong-layer" => JsonResponse(ShowJson(new string('e', 64))),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

    private static HttpResponseMessage JsonResponse(
        string json,
        HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private static HttpResponseMessage ByteResponse(byte[] bytes)
    {
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private static HttpResponseMessage ByteResponse(HttpContent content)
    {
        content.Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
    }

    private static byte[] PaddedChatResponse(int totalBytes)
    {
        var json = Encoding.UTF8.GetBytes(ChatJson());
        Assert.True(json.Length <= totalBytes);
        var padded = Enumerable.Repeat((byte)' ', totalBytes).ToArray();
        json.CopyTo(padded, 0);
        return padded;
    }

    private static TaskCompletionSource NewSignal()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private sealed class TestCallPermit(bool allowIssue = true)
        : ITrainingTeacherCallPermit, ILegalTeacherCallPermit
    {
        internal const string RejectedErrorCode = "TEST_PERMIT_REJECTED";
        private int _issued;

        public bool CallIssued => Volatile.Read(ref _issued) != 0;
        public int IssueCount => Volatile.Read(ref _issued);

        public OperationResult<int> TryIssue()
        {
            if (!allowIssue)
            {
                return OperationResult<int>.Fail(
                    "Test call permit rejected.",
                    RejectedErrorCode);
            }
            return Interlocked.CompareExchange(ref _issued, 1, 0) == 0
                ? OperationResult<int>.Ok(1)
                : OperationResult<int>.Fail(
                    "Test call permit was already issued.",
                    RejectedErrorCode);
        }
    }

    private sealed class ProgrammablePermit(
        bool returnNull = false,
        bool throwFromCallIssued = false,
        Exception? tryIssueException = null)
        : ITrainingTeacherCallPermit
    {
        private int _tryIssueCount;
        private int _callIssuedReadCount;

        public bool CallIssued
        {
            get
            {
                Interlocked.Increment(ref _callIssuedReadCount);
                if (throwFromCallIssued)
                    throw new InvalidOperationException("getter must not be read");
                return Volatile.Read(ref _tryIssueCount) > 0;
            }
        }

        internal int TryIssueCount => Volatile.Read(ref _tryIssueCount);
        internal int CallIssuedReadCount =>
            Volatile.Read(ref _callIssuedReadCount);

        public OperationResult<int> TryIssue()
        {
            Interlocked.Increment(ref _tryIssueCount);
            if (tryIssueException is not null)
                throw tryIssueException;
            if (returnNull)
                return null!;
            return OperationResult<int>.Ok(1);
        }
    }

    private sealed record RequestSnapshot(
        HttpMethod Method,
        Uri Uri,
        string? Body,
        string? ContentType,
        string? CharSet)
    {
        internal string MethodAndUri => $"{Method.Method} {Uri.AbsoluteUri}";
    }

    private sealed class ScriptedHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
        : HttpMessageHandler
    {
        private readonly object _gate = new();
        private readonly List<RequestSnapshot> _requests = [];
        private int _disposeCount;

        internal IReadOnlyList<RequestSnapshot> Requests
        {
            get
            {
                lock (_gate)
                    return _requests.ToArray();
            }
        }

        internal int DisposeCount => Volatile.Read(ref _disposeCount);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            lock (_gate)
            {
                _requests.Add(new RequestSnapshot(
                    request.Method,
                    request.RequestUri!,
                    body,
                    request.Content?.Headers.ContentType?.MediaType,
                    request.Content?.Headers.ContentType?.CharSet));
            }
            return await send(request, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Interlocked.Increment(ref _disposeCount);
            base.Dispose(disposing);
        }
    }

    private sealed class SynchronousPrefixHandler : HttpMessageHandler
    {
        private int _disposeCount;

        internal ManualResetEventSlim Entered { get; } = new(initialState: false);
        internal ManualResetEventSlim Release { get; } = new(initialState: false);
        internal int DisposeCount => Volatile.Read(ref _disposeCount);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Entered.Set();
            Release.Wait();
            return Task.FromCanceled<HttpResponseMessage>(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Interlocked.Increment(ref _disposeCount);
                Entered.Dispose();
                Release.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    private sealed class DeclaredLengthContent(byte[] bytes, long declaredLength)
        : HttpContent
    {
        protected override bool TryComputeLength(out long length)
        {
            length = declaredLength;
            return true;
        }

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
            => stream.WriteAsync(bytes).AsTask();

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context,
            CancellationToken cancellationToken)
            => stream.WriteAsync(bytes, cancellationToken).AsTask();

        protected override Task<Stream> CreateContentReadStreamAsync()
            => Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));

        protected override Task<Stream> CreateContentReadStreamAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
        }
    }

    private sealed class TrackingStreamContent(TrackingMemoryStream stream)
        : HttpContent
    {
        protected override bool TryComputeLength(out long length)
        {
            length = stream.Length;
            return true;
        }

        protected override Task SerializeToStreamAsync(
            Stream target,
            TransportContext? context)
            => stream.CopyToAsync(target);

        protected override Task SerializeToStreamAsync(
            Stream target,
            TransportContext? context,
            CancellationToken cancellationToken)
            => stream.CopyToAsync(target, cancellationToken);

        protected override Task<Stream> CreateContentReadStreamAsync()
            => Task.FromResult<Stream>(stream);

        protected override Task<Stream> CreateContentReadStreamAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Stream>(stream);
        }
    }

    private sealed class TrackingMemoryStream(byte[] bytes)
        : MemoryStream(bytes, writable: false)
    {
        internal bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class UnknownLengthContent(byte[] bytes) : HttpContent
    {
        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context)
            => stream.WriteAsync(bytes).AsTask();

        protected override Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context,
            CancellationToken cancellationToken)
            => stream.WriteAsync(bytes, cancellationToken).AsTask();

        protected override Task<Stream> CreateContentReadStreamAsync()
            => Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));

        protected override Task<Stream> CreateContentReadStreamAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
        }
    }

    private sealed class ControlledDeadline
    {
        private CancellationTokenSource? _source;

        internal CancellationTokenSource Create(TimeSpan timeout)
        {
            var source = new CancellationTokenSource();
            Assert.Null(Interlocked.CompareExchange(ref _source, source, null));
            return source;
        }

        internal void Trigger()
        {
            var source = Volatile.Read(ref _source);
            Assert.NotNull(source);
            source.Cancel();
        }
    }
}
