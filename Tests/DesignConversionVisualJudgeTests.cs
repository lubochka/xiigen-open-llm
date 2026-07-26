// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionVisualJudgeTests
{
    [Fact]
    public async Task NoConfiguredVisualJudge_ReturnsExplicitBlockerWithoutExternalCall()
    {
        var judge = new NoConfiguredVisualJudge();

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.IsBlocked);
        Assert.Equal(NoConfiguredVisualJudge.Status, result.Value.Status);
        Assert.False(result.Value.Provider.ExternalCallExecuted);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Equal(0.0, result.Value.Verdict.Score);
        Assert.Contains(NoConfiguredVisualJudge.EvidenceMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, result.Value.EvidenceRefs);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker,
            result.Value.Metadata["provider_required_reason"]);
    }

    [Fact]
    public async Task DefaultLocalVisualJudge_ReturnsMetadataVerdictWithoutExternalCall()
    {
        var judge = new DefaultLocalDesignConversionVisualJudge();

        var result = await judge.JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
        {
            ["comparison_verdict_score"] = "0.91",
            ["comparison_verdict_passed"] = bool.TrueString,
            ["comparison_defect_categories"] = string.Empty
        }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.IsBlocked);
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.StatusOk, result.Value.Status);
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.ProviderName, result.Value.Provider.ProviderName);
        Assert.False(result.Value.Provider.ExternalCallExecuted);
        Assert.Equal(0m, result.Value.EstimatedCostUsd);
        Assert.True(result.Value.Verdict.Passed);
        Assert.Equal(0.91, result.Value.Verdict.Score);
        Assert.Empty(result.Value.Verdict.Defects);
        Assert.Contains(DefaultLocalDesignConversionVisualJudge.EvidenceMarker, result.Value.EvidenceRefs);
        Assert.Contains(DefaultLocalDesignConversionVisualJudge.ExternalProviderNotUsedMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain("external_ai_visual_judge_executed:true", result.Value.EvidenceRefs);
        Assert.Equal(bool.FalseString, result.Value.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
            result.Value.Metadata["provider_boundary"]);
    }

    [Fact]
    public async Task DefaultLocalVisualJudge_NoComparisonMeasurementFailsClosedWithSafeEvidence()
    {
        var judge = new DefaultLocalDesignConversionVisualJudge();
        const string rawHtml = "<main data-secret=\"private-html\">hidden</main>";
        const string rawCss = ".private { color: red; }";
        const string rawTree = "{\"private\":\"tree\"}";
        const string rawPath = @"C:\sandbox\llm_mvp_core\private\source.html";
        const string rawSecret = "safe-test-secret-not-for-results";

        var result = await judge.JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
        {
            ["html"] = rawHtml,
            ["css"] = rawCss,
            ["tree_payload"] = rawTree,
            ["source_path"] = rawPath,
            ["api_key"] = rawSecret
        }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.IsBlocked);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Equal(0.0, result.Value.Verdict.Score);
        var defect = Assert.Single(result.Value.Verdict.Defects);
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.ComparisonMeasurementMissingCategory, defect.Category);
        Assert.Equal("blocker", defect.Severity);
        Assert.Equal(bool.FalseString, result.Value.Metadata["comparison_measurement_present"]);
        Assert.Equal("none", result.Value.Metadata["comparison_measurement_source"]);
        Assert.Equal(
            DefaultLocalDesignConversionVisualJudge.ComparisonMeasurementMissingStatus,
            result.Value.Metadata["comparison_measurement_status"]);
        Assert.Contains(
            $"local_visual_labeler_comparison_measurement_present={bool.FalseString}",
            result.Value.EvidenceRefs);
        Assert.Contains(
            DefaultLocalDesignConversionVisualJudge.ComparisonMeasurementMissingEvidenceMarker,
            result.Value.EvidenceRefs);
        Assert.Contains(
            $"local_visual_labeler_defect:{DefaultLocalDesignConversionVisualJudge.ComparisonMeasurementMissingCategory}",
            result.Value.EvidenceRefs);
        Assert.DoesNotContain("external_ai_visual_judge_executed:true", result.Value.EvidenceRefs);

        var serialized = JsonSerializer.Serialize(result.Value);
        AssertSerializedDoesNotContain(
            serialized,
            rawHtml,
            rawCss,
            rawTree,
            rawPath,
            rawSecret,
            @"C:\sandbox\llm_mvp_core\private");
    }

    [Fact]
    public async Task DefaultLocalVisualJudge_InvalidComparisonScoreFailsClosedAsMissingMeasurement()
    {
        var judge = new DefaultLocalDesignConversionVisualJudge();

        var result = await judge.JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
        {
            ["comparison_verdict_score"] = "not-a-number",
            ["comparison_score"] = "also-not-a-number",
            ["visual_distance"] = "distance-not-a-number",
            ["comparison_verdict_passed"] = bool.TrueString
        }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.Verdict.Passed);
        Assert.Equal(0.0, result.Value.Verdict.Score);
        var defect = Assert.Single(result.Value.Verdict.Defects);
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.ComparisonMeasurementMissingCategory, defect.Category);
        Assert.Equal("blocker", defect.Severity);
        Assert.Equal(bool.FalseString, result.Value.Metadata["comparison_measurement_present"]);
        Assert.Equal(
            DefaultLocalDesignConversionVisualJudge.ComparisonMeasurementMissingStatus,
            result.Value.Metadata["comparison_measurement_status"]);
        Assert.Contains(
            DefaultLocalDesignConversionVisualJudge.ComparisonMeasurementMissingEvidenceMarker,
            result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task DefaultLocalVisualJudge_ValidComparisonScorePreservesPassBehavior()
    {
        var judge = new DefaultLocalDesignConversionVisualJudge();

        var result = await judge.JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
        {
            ["comparison_verdict_score"] = "not-a-number",
            ["comparison_score"] = "0.93",
            ["comparison_verdict_passed"] = bool.TrueString,
            ["comparison_defect_categories"] = "spacing"
        }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.IsBlocked);
        Assert.True(result.Value.Verdict.Passed);
        Assert.Equal(0.93, result.Value.Verdict.Score);
        Assert.Empty(result.Value.Verdict.Defects);
        Assert.Equal(bool.TrueString, result.Value.Metadata["comparison_measurement_present"]);
        Assert.Equal("comparison_score", result.Value.Metadata["comparison_measurement_source"]);
        Assert.Equal("present", result.Value.Metadata["comparison_measurement_status"]);
        Assert.DoesNotContain(
            DefaultLocalDesignConversionVisualJudge.ComparisonMeasurementMissingEvidenceMarker,
            result.Value.EvidenceRefs);
        Assert.DoesNotContain(
            $"local_visual_labeler_defect:{DefaultLocalDesignConversionVisualJudge.ComparisonMeasurementMissingCategory}",
            result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task DefaultLocalVisualJudge_LabelsResolutionDiffAndComparisonDefectsDeterministically()
    {
        var judge = new DefaultLocalDesignConversionVisualJudge();

        var result = await judge.JudgeAsync(CreateRequest(
            diffUri: null,
            metadata: new Dictionary<string, string>
            {
                ["comparison_verdict_score"] = "0.52",
                ["comparison_verdict_passed"] = bool.FalseString,
                ["comparison_defect_categories"] = "spacing|color contrast"
            },
            actualResolution: new RenderQaResolution("mobile", 390, 844, 3.0)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.IsBlocked);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Equal(0.50, result.Value.Verdict.Score);
        Assert.Equal(
            [
                "viewport_resolution_mismatch",
                "device_scale_factor_mismatch",
                "diff_artifact_missing",
                "comparison_spacing",
                "comparison_color_contrast"
            ],
            result.Value.Verdict.Defects.Select(defect => defect.Category).ToArray());
        Assert.Contains(result.Value.EvidenceRefs, evidence => evidence == "local_visual_labeler_defect:comparison_spacing");
        Assert.Contains(result.Value.EvidenceRefs, evidence => evidence == "local_visual_labeler_defect:comparison_color_contrast");
        Assert.All(result.Value.Verdict.Defects, defect =>
        {
            Assert.Contains("expected-shot", defect.EvidenceImageIds);
            Assert.Contains("actual-render", defect.EvidenceImageIds);
        });
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_ParsesFakeProviderVerdictDefectsAndMetadata()
    {
        const string requestSecret = "visual-request-secret-threshold";
        const string rawPayload = "<main data-private=\"visual-threshold\">secret</main>";
        var handler = new FakeVisualJudgeHandler("""
            {
              "judge_result_id": "fake-result-001",
              "verdict_id": "fake-verdict-001",
              "score": 0.88,
              "passed": true,
              "model_id": "fake-vision-model",
              "cost_estimate_usd": 98765.4321,
              "tokens": { "input": 120, "output": 24, "total": 144 },
              "evidence_refs": [ "fake-provider:evidence" ],
              "defects": [
                {
                  "id": "defect-spacing-001",
                  "category": "spacing",
                  "severity": "minor",
                  "description": "Button spacing is slightly tight.",
                  "x": 18,
                  "y": 24,
                  "width": 64,
                  "height": 20,
                  "evidence_image_ids": [ "expected-shot", "actual-render" ]
                }
              ]
            }
            """);
        var judge = CreateHttpJudge(handler);

        var result = await judge.JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
        {
            ["quality_threshold"] = "0.90",
            ["api_key"] = requestSecret,
            ["html"] = rawPayload
        }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        Assert.NotNull(handler.CapturedBody);
        Assert.DoesNotContain(requestSecret, handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPayload, handler.CapturedBody, StringComparison.Ordinal);
        using (var payloadDocument = JsonDocument.Parse(handler.CapturedBody!))
        {
            var payload = payloadDocument.RootElement;
            Assert.Equal(0.9, payload.GetProperty("quality_threshold").GetDouble());
            Assert.Equal(
                "required boolean diagnostic; server recomputes final Passed from score and quality_threshold",
                payload.GetProperty("response_schema").GetProperty("passed").GetString());
            Assert.Equal(
                "0..1 server acceptance threshold; provider passed is diagnostic only",
                payload.GetProperty("response_schema").GetProperty("quality_threshold").GetString());
        }

        Assert.False(result.Value!.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusOk, result.Value.Status);
        Assert.True(result.Value.Provider.ExternalCallExecuted);
        Assert.Equal(
            bool.FalseString,
            result.Value.Provider.Metadata[DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker]);
        Assert.Equal("fake-provider", result.Value.Provider.ProviderName);
        Assert.Equal("fake-vision-model", result.Value.Provider.ModelId);
        Assert.Equal("fake-verdict-001", result.Value.Verdict.VerdictId);
        Assert.Equal(0.88, result.Value.Verdict.Score);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Single(result.Value.Verdict.Defects);
        Assert.Equal("spacing", result.Value.Verdict.Defects[0].Category);
        Assert.Equal(0.0042m, result.Value.EstimatedCostUsd);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            result.Value.Metadata["cost_source"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["provider_reported_passed"]);
        Assert.Equal("0.9", result.Value.Metadata["required_quality_threshold"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["server_quality_threshold_passed"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["provider_server_passed_mismatch"]);
        Assert.Equal("server_quality_threshold", result.Value.Metadata["passed_authority"]);
        Assert.Equal(bool.TrueString, result.Value.Provider.Metadata["provider_reported_passed"]);
        Assert.Equal("0.9", result.Value.Provider.Metadata["required_quality_threshold"]);
        Assert.Equal(bool.FalseString, result.Value.Provider.Metadata["server_quality_threshold_passed"]);
        Assert.Equal(bool.TrueString, result.Value.Provider.Metadata["provider_server_passed_mismatch"]);
        Assert.Equal("server_quality_threshold", result.Value.Provider.Metadata["passed_authority"]);
        Assert.Contains("provider_reported_passed:True", result.Value.EvidenceRefs);
        Assert.Contains("required_quality_threshold:0.9", result.Value.EvidenceRefs);
        Assert.Contains("server_quality_threshold_passed:False", result.Value.EvidenceRefs);
        Assert.Contains("provider_server_passed_mismatch:True", result.Value.EvidenceRefs);
        Assert.Contains("passed_authority:server_quality_threshold", result.Value.EvidenceRefs);
        Assert.Equal(bool.TrueString, result.Value.Metadata["provider_reported_cost_present"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["provider_reported_cost_value_included"]);
        Assert.Equal(bool.TrueString, result.Value.Provider.Metadata["server_owned_pricing_configured"]);
        Assert.Equal(144, result.Value.TokenMetadata["total"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["visual_ai_executed"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["provider_response_identity_verified"]);
        Assert.Equal(
            result.Value.Metadata["provider_response_identity_verified"],
            result.Value.Provider.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            result.Value.Metadata["provider_response_schema_verified"],
            result.Value.Provider.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Contains("external_ai_visual_judge_executed:true", result.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(NoConfiguredVisualJudge.EvidenceMarker, result.Value.EvidenceRefs);
        var serializedResult = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain("98765.4321", serializedResult, StringComparison.Ordinal);
        Assert.DoesNotContain(requestSecret, serializedResult, StringComparison.Ordinal);
        Assert.DoesNotContain(rawPayload, serializedResult, StringComparison.Ordinal);
        Assert.DoesNotContain("fake-result-001", serializedResult, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_Retries429OnceAndCanSucceed()
    {
        var retryDelay = new RecordingRetryDelay();
        var handler = new SequenceVisualJudgeHandler(
            () => CreateStatusResponse(HttpStatusCode.TooManyRequests),
            CreateSuccessfulVisualJudgeResponse);
        var judge = CreateHttpJudgeWithRetryDelay(handler, retryDelay.DelayAsync);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        Assert.Single(retryDelay.Delays);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusOk, result.Value!.Status);
        Assert.Equal(0.0084m, result.Value.EstimatedCostUsd);
        Assert.Equal(
            "2",
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal(
            "0.0084",
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.All(handler.IdempotencyKeyValues, values => Assert.Equal(["judge-request-001"], values));
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_UsesRetryAfterDelayFor429WithoutSleeping()
    {
        var retryAfter = TimeSpan.FromMilliseconds(375);
        var retryDelay = new RecordingRetryDelay();
        var handler = new SequenceVisualJudgeHandler(
            () => CreateStatusResponse(HttpStatusCode.TooManyRequests, retryAfter),
            () => CreateStatusResponse(
                HttpStatusCode.InternalServerError,
                body: "private vendor error body token=terminal-secret"));
        var judge = CreateHttpJudgeWithRetryDelay(handler, retryDelay.DelayAsync);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        Assert.Equal([retryAfter], retryDelay.Delays);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked, result.Value!.Status);
        Assert.True(result.Value.IsBlocked);
        Assert.True(result.Value.Provider.ExternalCallExecuted);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Equal(0.0, result.Value.Verdict.Score);
        Assert.Equal(0.0084m, result.Value.EstimatedCostUsd);
        Assert.Equal(
            "2",
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            "500",
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(
            bool.FalseString,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.FalseString,
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.FalseString,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            bool.FalseString,
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Contains("external_ai_visual_judge_executed:true", result.Value.EvidenceRefs);
        Assert.DoesNotContain("98765", JsonSerializer.Serialize(result.Value), StringComparison.Ordinal);
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(result.Value),
            "private vendor error body",
            "terminal-secret");
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_ClampsHttpDateRetryAfterWithoutSleeping()
    {
        var retryDelay = new RecordingRetryDelay();
        var handler = new SequenceVisualJudgeHandler(
            () =>
            {
                var response = CreateStatusResponse(HttpStatusCode.TooManyRequests);
                response.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddMinutes(1));
                return response;
            },
            () => CreateStatusResponse(HttpStatusCode.InternalServerError));
        var judge = CreateHttpJudgeWithRetryDelay(handler, retryDelay.DelayAsync);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked, result.Value!.Status);
        Assert.True(result.Value.IsBlocked);
        Assert.Equal("500", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey]);
        Assert.Equal(2, handler.CallCount);
        Assert.Equal([TimeSpan.FromSeconds(2)], retryDelay.Delays);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_UsesFallbackDelayForTransient5xxWithoutSleeping()
    {
        var retryDelay = new RecordingRetryDelay();
        var handler = new SequenceVisualJudgeHandler(
            () => CreateStatusResponse(HttpStatusCode.InternalServerError),
            CreateSuccessfulVisualJudgeResponse);
        var judge = CreateHttpJudgeWithRetryDelay(handler, retryDelay.DelayAsync);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        Assert.Equal([TimeSpan.FromMilliseconds(100)], retryDelay.Delays);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_CancellationDuringRetryWaitPreventsSecondCall()
    {
        using var cts = new CancellationTokenSource();
        var retryDelay = new RecordingRetryDelay((_, cancellationToken) =>
        {
            cts.Cancel();
            return Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        });
        var handler = new SequenceVisualJudgeHandler(
            () => CreateStatusResponse(HttpStatusCode.TooManyRequests, TimeSpan.FromMilliseconds(25)),
            CreateSuccessfulVisualJudgeResponse);
        var judge = CreateHttpJudgeWithRetryDelay(handler, retryDelay.DelayAsync);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => judge.JudgeAsync(CreateRequest(), cts.Token));

        Assert.Equal(1, handler.CallCount);
        Assert.Single(retryDelay.Delays);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_CallerCancellationDuringSendStillThrows()
    {
        using var cts = new CancellationTokenSource();
        var handler = new DelayingVisualJudgeHandler(TimeSpan.FromSeconds(5), cts.Cancel);
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-judge.test/v1/judge"),
                ProviderName: "fake-provider",
                ModelId: "fake-vision-model",
                ApiKey: "safe-test-key",
                ServerOwnedCostPerRequestUsd: 0.0042m),
            new HttpClient(handler),
            DefaultPngFixture.Value.Resolver);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => judge.JudgeAsync(CreateRequest(), cts.Token));

        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_TransientRetryCountRemainsBounded()
    {
        var retryDelay = new RecordingRetryDelay();
        var handler = new SequenceVisualJudgeHandler(
            () => CreateStatusResponse(HttpStatusCode.TooManyRequests),
            () => CreateStatusResponse(HttpStatusCode.TooManyRequests),
            CreateSuccessfulVisualJudgeResponse);
        var judge = CreateHttpJudgeWithRetryDelay(handler, retryDelay.DelayAsync);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked, result.Value!.Status);
        Assert.True(result.Value.IsBlocked);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Equal(0.0084m, result.Value.EstimatedCostUsd);
        Assert.Equal("2", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal("429", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey]);
        Assert.Equal(2, handler.CallCount);
        Assert.Single(retryDelay.Delays);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_SendTimeoutAfterAttemptReturnsBlockedTransportCostReceiptWithoutLeaks()
    {
        const string apiKey = "safe-timeout-key-not-for-result";
        const string privateMetadata = "private-timeout-detail-not-for-result";
        const string endpoint = "https://visual-judge.example/v1/judge";
        const string endpointHost = "visual-judge.example";
        var handler = new DelayingVisualJudgeHandler(TimeSpan.FromSeconds(5));
        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(50)
        };
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri(endpoint),
                ProviderName: "openai",
                ModelId: "gpt-vision-real",
                ApiKey: apiKey,
                ServerOwnedCostPerRequestUsd: 0.0042m),
            httpClient,
            DefaultPngFixture.Value.Resolver);

        var result = await judge.JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
        {
            ["api_key"] = privateMetadata
        }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        Assert.True(result.Value!.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusTransportBlocked, result.Value.Status);
        Assert.True(result.Value.Provider.ExternalCallExecuted);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Equal(0.0, result.Value.Verdict.Score);
        Assert.Equal(0.0042m, result.Value.EstimatedCostUsd);
        Assert.Empty(result.Value.TokenMetadata);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureRequestTimeoutCode,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey]);
        Assert.Equal("1", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal("0.0042", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal("0.0042", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            result.Value.Metadata["cost_source"]);
        AssertTransportAttemptedWithoutVisualAiExecution(result.Value);
        Assert.False(result.Value.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey));
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.StatusTransportBlocked, result.Value.EvidenceRefs);
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.TransportFailureMarker, result.Value.EvidenceRefs);
        Assert.Contains(
            $"transport_failure_code:{ExternalHttpDesignConversionVisualJudge.TransportFailureRequestTimeoutCode}",
            result.Value.EvidenceRefs);
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(result.Value),
            apiKey,
            privateMetadata,
            endpoint,
            endpointHost,
            "/v1/judge",
            nameof(OperationCanceledException),
            "EXTERNAL_VISUAL_JUDGE_TIMEOUT");
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_FinalHttpRequestExceptionReturnsBlockedTransportCostReceiptWithoutLeaks()
    {
        const string firstPrivateMessage = "private-transport-secret token=transport-secret-one";
        const string secondPrivateMessage = "private-transport-secret token=transport-secret-two";
        var retryDelay = new RecordingRetryDelay();
        var handler = new SequenceVisualJudgeHandler(
            () => throw new HttpRequestException(firstPrivateMessage),
            () => throw new HttpRequestException(secondPrivateMessage));
        var judge = CreateHttpJudgeWithRetryDelay(handler, retryDelay.DelayAsync);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        Assert.Single(retryDelay.Delays);
        Assert.True(result.Value!.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusTransportBlocked, result.Value.Status);
        Assert.True(result.Value.Provider.ExternalCallExecuted);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Equal(0.0, result.Value.Verdict.Score);
        Assert.Equal(0.0084m, result.Value.EstimatedCostUsd);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureHttpRequestFailedCode,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey]);
        Assert.Equal("2", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal("0.0042", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal("0.0084", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Equal(
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey],
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            result.Value.Metadata["cost_source"]);
        Assert.Equal(result.Value.Metadata["cost_source"], result.Value.Provider.Metadata["cost_source"]);
        AssertTransportAttemptedWithoutVisualAiExecution(result.Value);
        Assert.False(result.Value.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey));
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.TransportFailureMarker, result.Value.EvidenceRefs);
        Assert.Contains(
            $"transport_failure_code:{ExternalHttpDesignConversionVisualJudge.TransportFailureHttpRequestFailedCode}",
            result.Value.EvidenceRefs);
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(result.Value),
            firstPrivateMessage,
            secondPrivateMessage,
            "transport-secret-one",
            "transport-secret-two",
            nameof(HttpRequestException));
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_RetrySuccessWithoutServerOwnedPriceLeavesCostUnknown()
    {
        var retryDelay = new RecordingRetryDelay();
        var handler = new SequenceVisualJudgeHandler(
            () => CreateStatusResponse(HttpStatusCode.TooManyRequests),
            CreateSuccessfulVisualJudgeResponse);
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-judge.test/v1/judge"),
                ProviderName: "fake-provider",
                ModelId: "fake-vision-model",
                ApiKey: "safe-test-key"),
            new HttpClient(handler),
            retryDelay.DelayAsync,
            DefaultPngFixture.Value.Resolver);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, handler.CallCount);
        Assert.Null(result.Value!.EstimatedCostUsd);
        Assert.Equal(
            "2",
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced,
            result.Value.Metadata["cost_source"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced,
            result.Value.Provider.Metadata["cost_source"]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingUnknown,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_ProviderReportedCostWithoutServerOwnedPriceLeavesCostUnpriced()
    {
        var handler = new FakeVisualJudgeHandler("""
            {
              "judge_result_id": "fake-result-unpriced",
              "verdict_id": "fake-verdict-unpriced",
              "score": 0.89,
              "passed": true,
              "model_id": "fake-vision-model",
              "estimated_cost_usd": 0.0042,
              "tokens": { "input": 12, "output": 3, "total": 15 },
              "defects": []
            }
            """);
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-judge.test/v1/judge"),
                ProviderName: "fake-provider",
                ModelId: "fake-vision-model",
                ApiKey: "safe-test-key"),
            new HttpClient(handler),
            DefaultPngFixture.Value.Resolver);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        Assert.Null(result.Value!.EstimatedCostUsd);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced,
            result.Value.Metadata["cost_source"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["server_owned_pricing_configured"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["server_owned_pricing_valid"]);
        Assert.Equal("unconfigured", result.Value.Metadata["server_owned_pricing_status"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["provider_reported_cost_present"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["provider_reported_cost_value_included"]);
        Assert.Equal(15, result.Value.TokenMetadata["total"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced,
            result.Value.Provider.Metadata["cost_source"]);
        Assert.DoesNotContain("0.0042", JsonSerializer.Serialize(result.Value), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_DirectNegativeServerOwnedPriceIsInvalidUnpricedAtRuntime()
    {
        var handler = new FakeVisualJudgeHandler("""
            {
              "score": 0.91,
              "passed": true,
              "model_id": "fake-vision-model",
              "tokens": { "total": 7 },
              "defects": []
            }
            """);
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-judge.test/v1/judge"),
                ProviderName: "fake-provider",
                ModelId: "fake-vision-model",
                ApiKey: "safe-test-key",
                ServerOwnedCostPerRequestUsd: -0.01m),
            new HttpClient(handler),
            DefaultPngFixture.Value.Resolver);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Null(result.Value!.EstimatedCostUsd);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced,
            result.Value.Metadata["cost_source"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["server_owned_pricing_configured"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["server_owned_pricing_valid"]);
        Assert.Equal("invalid", result.Value.Metadata["server_owned_pricing_status"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["provider_reported_cost_present"]);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_TestProviderOnlyValidConfigReturnsBlockedNoNetworkWithoutLeaks()
    {
        var secret = "safe-test-provider-key-" + Guid.NewGuid().ToString("N");
        var endpoint = "https://visual-judge.example/v1/judge?api_key=endpoint-secret";
        var handler = new FakeVisualJudgeHandler("""
            {
              "score": 1.0,
              "passed": true,
              "defects": []
            }
            """);
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri(endpoint),
                ProviderName: "fake-provider",
                ModelId: "fake-vision-model",
                ApiKey: secret,
                Metadata: new Dictionary<string, string>
                {
                    [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString,
                    ["test_provider"] = "yes"
                }),
            new HttpClient(handler));

        var result = await judge.JudgeAsync(CreateRequest(
            expectedUri: @"C:\sandbox\llm_mvp_core\private\expected.png",
            actualUri: "file:///C:/sandbox/llm_mvp_core/private/actual.png"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, handler.CallCount);
        Assert.True(result.Value!.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.TestProviderNoNetworkStatus, result.Value.Status);
        Assert.False(result.Value.Provider.ExternalCallExecuted);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Equal(0.0, result.Value.Verdict.Score);
        Assert.Null(result.Value.EstimatedCostUsd);
        Assert.Empty(result.Value.TokenMetadata);
        Assert.Equal("not_called", result.Value.Provider.EndpointHost);
        Assert.Equal(bool.FalseString, result.Value.Provider.Metadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Value.Provider.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, result.Value.Provider.Metadata["visual_ai_executed"]);
        Assert.Equal(bool.FalseString, result.Value.Provider.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.FalseString, result.Value.Provider.Metadata["provider_response_schema_verified"]);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.TestProviderNoNetworkReason, result.Value.Metadata["reason"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, result.Value.Metadata["visual_ai_executed"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata[DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, result.Value.EvidenceRefs);
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.TestProviderNoNetworkEvidenceMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain("external_ai_visual_judge_executed:true", result.Value.EvidenceRefs);

        var serialized = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(endpoint, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("visual-judge.example", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("endpoint-secret", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(@"C:\sandbox\llm_mvp_core\private", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("file:///C:/sandbox/llm_mvp_core/private", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_SanitizesMetadataWithAllowlistAndValueScanning()
    {
        const string apiKey = "safe-test-key-not-for-logs";
        const string requestSecret = "very-secret-value";
        const string normalizedCollisionSecret = "normalized-collision-secret";
        const string neutralBearer = "Bearer neutral-secret-token-value";
        const string commonCredentialPrefix = "sk-proj-visualjudge-opaque-secret-0123456789";
        const string sensitiveSuffixOpaqueValue = "opaque-private-key-material-without-assignment";
        const string requestJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkZXNpZ24iLCJzZWNyZXQiOiJyZXEifQ.signaturesegment0123456789";
        const string providerSecretAssignment = "api_key=provider-secret-value";
        const string privateKeyMarker = "private-key-sentinel";
        const string providerUnknownRaw = "provider unknown innocuous raw should not leave";
        const string requestUnknownRaw = "request unknown innocuous raw should not leave";
        const string expectedUnknownRaw = "expected image unknown raw should not leave";
        const string renderedUnknownRaw = "rendered image unknown raw should not leave";
        const string rawHtml = "<main>private html</main>";
        const string rawCss = ".private{color:red}";
        const string rawTree = "{\"private\":\"tree\"}";
        const string genericMarkupSentinel = "generic-markup-sentinel";
        const string genericCss = "body{margin:0}";
        const string unknownCrowdSentinel = "unknown-crowd-raw-sentinel";
        const string expectedRawHtml = "<section>expected private html</section>";
        const string renderedRawCss = ".actual-private{}";
        const string localSourcePath = @"C:\sandbox\llm_mvp_core\private\source.png";
        const string localExpectedPath = @"C:\sandbox\llm_mvp_core\private\expected.png";
        const string localRenderedPath = @"C:\sandbox\llm_mvp_core\private\actual.png";
        const string uriSecret = "uri-secret-value";
        var genericMarkup = $"<p>{new string('x', 420)}{genericMarkupSentinel}</p>";
        var privateKeyPayload = $"-----BEGIN PRIVATE KEY-----\n{privateKeyMarker}\n-----END PRIVATE KEY-----";
        var handler = new FakeVisualJudgeHandler("""
            {
              "verdict_id": "fake-verdict-redaction",
              "score": 0.91,
              "passed": true,
              "defects": []
            }
            """);
        var judge = CreateRealHttpJudge(
            handler,
            apiKey: apiKey,
            metadata: new Dictionary<string, string>
            {
                ["external_visual_provider_config_complete"] = bool.TrueString,
                ["external_visual_provider_image_payload_mode"] = ExternalHttpDesignConversionVisualJudge.PayloadMode,
                ["server_owned_pricing_configured"] = bool.TrueString,
                ["primary_api_key_setting"] = "DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_API_KEY",
                ["design_token_provider_spacing"] = "8px",
                ["design_token_provider_secret"] = providerSecretAssignment,
                ["provider_unknown_note"] = providerUnknownRaw
            });
        var requestMetadata = new Dictionary<string, string>
        {
            ["api_key"] = requestSecret,
            ["api-key"] = normalizedCollisionSecret,
            ["html"] = rawHtml,
            ["css"] = rawCss,
            ["tree_payload"] = rawTree,
            ["user_id"] = "luba-private-user",
            ["source_path"] = localSourcePath,
            ["quality_threshold"] = "0.90",
            ["job_id"] = "job-visual-001",
            ["render_result_id"] = "render-result-001",
            ["comparison_request_id"] = "comparison-request-001",
            ["comparison_artifact_id"] = "comparison-artifact-001",
            ["visual_judge_index"] = "1",
            ["qa_data_role"] = "input_evidence_only",
            ["comparison_verdict_score"] = "0.91",
            ["comparison_verdict_passed"] = bool.TrueString,
            ["comparison_defect_count"] = "2",
            ["comparison_metric_visual_distance"] = "0.12",
            ["comparison_metric_invalid_text"] = "not-a-finite-metric",
            ["comparison_metric_nan"] = "NaN",
            ["comparison_metric_infinity"] = "Infinity",
            ["visual_distance"] = "0.12",
            ["viewport_width"] = "1440",
            ["html_sha256"] = "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
            ["source_artifact_ref"] = "artifact-ref:request-source",
            ["design_token_primary_color"] = "#3366ff",
            ["design_token_secondary_color"] = commonCredentialPrefix,
            ["design_token_private_key"] = sensitiveSuffixOpaqueValue,
            ["design-token-radius-card"] = "8px",
            ["design_token_radius_card"] = "10px",
            ["status"] = neutralBearer,
            ["provider_name"] = commonCredentialPrefix,
            ["provider_model"] = requestJwt,
            ["config_intake_source"] = privateKeyPayload,
            ["cost_source"] = providerSecretAssignment,
            ["comparison_boundary"] = genericMarkup,
            ["render_artifact_kind"] = genericCss,
            ["safe_note"] = requestUnknownRaw,
            ["callback_url"] = $"https://visual.example/callback?access_token={uriSecret}",
            ["profile_ref"] = "https://user:password@visual.example/resource"
        };
        for (var index = 0; index < 80; index++)
            requestMetadata[$"aaa_unknown_{index:D3}"] = $"{unknownCrowdSentinel}-{index:D3}";

        var request = CreateRequest(
            diffUri: @"C:\sandbox\llm_mvp_core\private\diff.png",
            metadata: requestMetadata,
            expectedMetadata: new Dictionary<string, string>
            {
                ["raw_html"] = expectedRawHtml,
                ["browser_screenshot_path"] = localExpectedPath,
                ["source_artifact_id"] = "expected-artifact-001",
                ["source_artifact_kind"] = "figma_export",
                ["source_artifact_ref"] = "artifact-ref:expected",
                ["expected_sha256_verified"] = bool.TrueString,
                ["png_width"] = "32",
                ["design_token_expected_spacing"] = "12px",
                ["design_token_expected_secret"] = "secret=expected-secret-value",
                ["image_hash_note"] = expectedUnknownRaw
            },
            actualMetadata: new Dictionary<string, string>
            {
                ["raw_css"] = renderedRawCss,
                ["local_path"] = localRenderedPath,
                ["rendered_sha256_verified"] = bool.TrueString,
                ["artifact_path_ref"] = "local-png://design-conversion/run/desktop/abcdef0123456789",
                ["png_height"] = "24",
                ["design_token_render_radius"] = "4px",
                ["design_token_render_secret"] = "password=rendered-secret-value",
                ["render_hash_note"] = renderedUnknownRaw
            });

        var result = await judge.JudgeAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(handler.CapturedBody);
        AssertSerializedDoesNotContain(
            handler.CapturedBody!,
            requestSecret,
            normalizedCollisionSecret,
            apiKey,
            neutralBearer,
            commonCredentialPrefix,
            sensitiveSuffixOpaqueValue,
            requestJwt,
            providerSecretAssignment,
            privateKeyMarker,
            "BEGIN PRIVATE KEY",
            providerUnknownRaw,
            requestUnknownRaw,
            expectedUnknownRaw,
            renderedUnknownRaw,
            rawHtml,
            rawCss,
            rawTree,
            genericMarkup,
            genericMarkupSentinel,
            genericCss,
            unknownCrowdSentinel,
            "luba-private-user",
            expectedRawHtml,
            renderedRawCss,
            localSourcePath,
            localExpectedPath,
            localRenderedPath,
            uriSecret,
            "user:password");
        Assert.DoesNotContain(@"C:\", handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("file:///C:/", handler.CapturedBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("artifact-ref:diff-artifact", handler.CapturedBody, StringComparison.Ordinal);
        Assert.Contains("#3366ff", handler.CapturedBody, StringComparison.Ordinal);
        Assert.Contains("12px", handler.CapturedBody, StringComparison.Ordinal);
        Assert.Contains("4px", handler.CapturedBody, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(handler.CapturedBody!);
        var body = document.RootElement;
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.PayloadMode, body.GetProperty("image_payload_mode").GetString());
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.RedactionPolicy,
            body.GetProperty("redaction").GetProperty("policy").GetString());

        var metadata = body.GetProperty("metadata");
        Assert.Equal("[redacted]", metadata.GetProperty("api_key").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("html").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("css").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("tree_payload").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("user_id").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("source_path").GetString());
        Assert.False(metadata.TryGetProperty("safe_note", out _));
        Assert.False(metadata.TryGetProperty("callback_url", out _));
        Assert.False(metadata.TryGetProperty("design_token_private_key", out _));
        Assert.False(metadata.TryGetProperty("aaa_unknown_000", out _));
        Assert.Equal("[redacted]", metadata.GetProperty("profile_ref").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("status").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("provider_name").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("provider_model").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("config_intake_source").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("cost_source").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("comparison_boundary").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("render_artifact_kind").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("design_token_secondary_color").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("design_token_radius_card").GetString());
        Assert.Equal("#3366ff", metadata.GetProperty("design_token_primary_color").GetString());
        Assert.Equal("0.90", metadata.GetProperty("quality_threshold").GetString());
        Assert.Equal("0.91", metadata.GetProperty("comparison_verdict_score").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("comparison_verdict_passed").GetString());
        Assert.Equal("2", metadata.GetProperty("comparison_defect_count").GetString());
        Assert.Equal("0.12", metadata.GetProperty("comparison_metric_visual_distance").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("comparison_metric_invalid_text").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("comparison_metric_nan").GetString());
        Assert.Equal("[redacted]", metadata.GetProperty("comparison_metric_infinity").GetString());
        Assert.Equal("0.12", metadata.GetProperty("visual_distance").GetString());
        Assert.Equal("1440", metadata.GetProperty("viewport_width").GetString());
        Assert.Equal("sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", metadata.GetProperty("html_sha256").GetString());
        Assert.Equal("artifact-ref:request-source", metadata.GetProperty("source_artifact_ref").GetString());
        Assert.InRange(metadata.EnumerateObject().Count(), 1, 64);

        var providerMetadata = body.GetProperty("provider_metadata");
        Assert.Equal(bool.TrueString, providerMetadata.GetProperty("external_visual_provider_config_complete").GetString());
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.PayloadMode, providerMetadata.GetProperty("external_visual_provider_image_payload_mode").GetString());
        Assert.Equal(bool.TrueString, providerMetadata.GetProperty("server_owned_pricing_configured").GetString());
        Assert.Equal("DESIGN_CONVERSION_EXTERNAL_VISUAL_JUDGE_API_KEY", providerMetadata.GetProperty("primary_api_key_setting").GetString());
        Assert.Equal("8px", providerMetadata.GetProperty("design_token_provider_spacing").GetString());
        Assert.False(providerMetadata.TryGetProperty("design_token_provider_secret", out _));
        Assert.False(providerMetadata.TryGetProperty("provider_unknown_note", out _));

        var expectedMetadata = body.GetProperty("expected_screenshot").GetProperty("metadata");
        Assert.Equal("[redacted]", expectedMetadata.GetProperty("raw_html").GetString());
        Assert.Equal("[redacted]", expectedMetadata.GetProperty("browser_screenshot_path").GetString());
        Assert.False(expectedMetadata.TryGetProperty("design_token_expected_secret", out _));
        Assert.False(expectedMetadata.TryGetProperty("image_hash_note", out _));
        Assert.Equal("expected-artifact-001", expectedMetadata.GetProperty("source_artifact_id").GetString());
        Assert.Equal("figma_export", expectedMetadata.GetProperty("source_artifact_kind").GetString());
        Assert.Equal("artifact-ref:expected", expectedMetadata.GetProperty("source_artifact_ref").GetString());
        Assert.Equal(bool.TrueString, expectedMetadata.GetProperty("expected_sha256_verified").GetString());
        Assert.Equal("32", expectedMetadata.GetProperty("png_width").GetString());
        Assert.Equal("12px", expectedMetadata.GetProperty("design_token_expected_spacing").GetString());

        var renderedMetadata = body.GetProperty("rendered_image").GetProperty("metadata");
        Assert.Equal("[redacted]", renderedMetadata.GetProperty("raw_css").GetString());
        Assert.Equal("[redacted]", renderedMetadata.GetProperty("local_path").GetString());
        Assert.False(renderedMetadata.TryGetProperty("design_token_render_secret", out _));
        Assert.False(renderedMetadata.TryGetProperty("render_hash_note", out _));
        Assert.Equal(bool.TrueString, renderedMetadata.GetProperty("rendered_sha256_verified").GetString());
        Assert.Equal("local-png://design-conversion/run/desktop/abcdef0123456789", renderedMetadata.GetProperty("artifact_path_ref").GetString());
        Assert.Equal("24", renderedMetadata.GetProperty("png_height").GetString());
        Assert.Equal("4px", renderedMetadata.GetProperty("design_token_render_radius").GetString());
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_SendsInlinePngBytesByteIdenticalWithoutLocalPaths()
    {
        var fixture = CreatePngFixture();
        var handler = new FakeVisualJudgeHandler("""
            {
              "verdict_id": "fake-verdict-inline-png",
              "score": 0.93,
              "passed": true,
              "defects": []
            }
            """);
        var judge = DesignConversionExternalVisualProviderConfigIntake.TryCreateVisualJudge(
            new DesignConversionExternalVisualProviderConfigInput(
                Endpoint: "https://visual-judge.example/v1/judge",
                ProviderName: "openai",
                ModelId: "gpt-vision-real",
                ApiKey: "safe-test-key",
                ServerOwnedImageArtifactRoot: fixture.Root),
            handler);
        Assert.True(judge.IsSuccess, judge.ErrorMessage);

        var result = await judge.Value!.JudgeAsync(CreateRequest(fixture: fixture));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        Assert.NotNull(handler.CapturedBody);
        AssertSerializedDoesNotContain(handler.CapturedBody!, fixture.Root, "file://", @"C:\");

        using var document = JsonDocument.Parse(handler.CapturedBody!);
        var body = document.RootElement;
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.PayloadMode,
            body.GetProperty("image_payload_mode").GetString());
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes,
            body.GetProperty("image_payload_limits").GetProperty("per_image_max_bytes").GetInt64());
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.MaxInlinePngTotalBytes,
            body.GetProperty("image_payload_limits").GetProperty("total_max_bytes").GetInt64());
        Assert.True(ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes > 64L * 1024L);
        Assert.Equal(25L * 1024L * 1024L, ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes);
        Assert.Equal(
            checked(ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes * 2L),
            ExternalHttpDesignConversionVisualJudge.MaxInlinePngTotalBytes);
        Assert.True(ExternalHttpDesignConversionVisualJudge.MaxRequestMetadataEnvelopeBytes > 0);
        AssertInlinePngContent(
            body.GetProperty("expected_screenshot"),
            fixture.ExpectedBytes,
            fixture.ExpectedSha256);
        AssertInlinePngContent(
            body.GetProperty("rendered_image"),
            fixture.ActualBytes,
            fixture.ActualSha256);

        var requestPayloadDigest = "sha256:" + Sha256Text(handler.CapturedBody!);
        Assert.Equal(
            requestPayloadDigest,
            Assert.Single(handler.CapturedHeaderValues(ExternalHttpDesignConversionVisualJudge.RequestPayloadSha256HeaderName)));
        Assert.Equal(
            requestPayloadDigest,
            result.Value!.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestPayloadSha256Key]);
    }

    [Theory]
    [InlineData("sha_mismatch", "EXTERNAL_VISUAL_JUDGE_IMAGE_SHA_MISMATCH")]
    [InlineData("non_png", "EXTERNAL_VISUAL_JUDGE_IMAGE_PNG_INVALID")]
    [InlineData("oversize", "EXTERNAL_VISUAL_JUDGE_IMAGE_BYTES_OUT_OF_BOUNDS")]
    [InlineData("unresolvable", "EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE")]
    public async Task ExternalHttpVisualJudge_PreDispatchImageContentFailuresDoNotCallVendor(
        string scenario,
        string expectedErrorCode)
    {
        var fixture = CreatePngFixture();
        var handler = new FakeVisualJudgeHandler("""
            {
              "verdict_id": "should-not-be-called",
              "score": 1.0,
              "passed": true,
              "defects": []
            }
            """);
        var judge = CreateRealHttpJudge(handler, resolver: fixture.Resolver);
        var request = BuildInvalidImageContentRequest(fixture, scenario);

        var result = await judge.JudgeAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedErrorCode, result.ErrorCode);
        Assert.Equal(0, handler.CallCount);
        Assert.Null(handler.CapturedBody);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_TrustedInlinePngContentAboveFormer64KiBLimitIsTransportedByteIdentical()
    {
        var expectedBytes = CreateLargePngContractBytes(72 * 1024, seed: 11);
        var actualBytes = CreateLargePngContractBytes(80 * 1024, seed: 29);
        var expectedSha = Sha256Bytes(expectedBytes);
        var actualSha = Sha256Bytes(actualBytes);
        var expectedPathMarker = $"/tmp/private-large-expected-{expectedSha}.png";
        var actualPathMarker = $"/tmp/private-large-actual-{actualSha}.png";
        var handler = new FakeVisualJudgeHandler("""
            {
              "verdict_id": "large-inline-contract-verdict",
              "score": 0.96,
              "passed": true,
              "defects": []
            }
            """);
        var resolver = new StaticVisualJudgeImageContentResolver(
            expectedSha,
            expectedBytes,
            actualSha,
            actualBytes);
        var judge = CreateRealHttpJudge(handler, resolver: resolver);

        var result = await judge.JudgeAsync(CreateRequest(
            expectedUri: expectedPathMarker,
            actualUri: actualPathMarker,
            expectedSha256: $"sha256:{expectedSha}",
            actualSha256: $"sha256:{actualSha}"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        Assert.NotNull(handler.CapturedBody);
        Assert.DoesNotContain(expectedPathMarker, handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(actualPathMarker, handler.CapturedBody, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(handler.CapturedBody!);
        var body = document.RootElement;
        AssertInlinePngContent(body.GetProperty("expected_screenshot"), expectedBytes, expectedSha);
        AssertInlinePngContent(body.GetProperty("rendered_image"), actualBytes, actualSha);
        Assert.True(expectedBytes.Length > 64 * 1024);
        Assert.True(actualBytes.Length > 64 * 1024);
        Assert.True(expectedBytes.Length < ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes);
        Assert.True(actualBytes.Length < ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_DirectResolverPathThroughReparseAncestorFailsBeforeVendorCall()
    {
        var fixture = CreatePngFixture();
        var outsideRoot = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "visual-judge-reparse-outside",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outsideRoot);
        var outsidePath = Path.Combine(outsideRoot, "outside.png");
        File.WriteAllBytes(outsidePath, fixture.ExpectedBytes);
        var outsideSha = Sha256Bytes(fixture.ExpectedBytes);
        var linkPath = Path.Combine(fixture.Root, "linked-outside");
        var handler = new FakeVisualJudgeHandler("""
            {
              "verdict_id": "should-not-be-called",
              "score": 1.0,
              "passed": true,
              "defects": []
            }
            """);

        try
        {
            try
            {
                Directory.CreateSymbolicLink(linkPath, outsideRoot);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
            {
                Assert.Equal(0, handler.CallCount);
                Assert.Null(handler.CapturedBody);
                return;
            }

            var linkedImagePath = Path.Combine(linkPath, "outside.png");
            var judge = CreateRealHttpJudge(
                handler,
                resolver: new ServerOwnedLocalPngComparisonProvider(
                    fixture.Root,
                    maxArtifactBytes: ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes));

            var result = await judge.JudgeAsync(CreateRequest(
                fixture: fixture,
                expectedUri: new Uri(linkedImagePath).AbsoluteUri,
                expectedSha256: $"sha256:{outsideSha}"));

            Assert.False(result.IsSuccess);
            Assert.Equal("EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE", result.ErrorCode);
            Assert.Equal(0, handler.CallCount);
            Assert.Null(handler.CapturedBody);
        }
        finally
        {
            TryDeleteDirectory(linkPath);
            TryDeleteDirectory(outsideRoot);
        }
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_ReparseArtifactRootFailsBeforeVendorCall()
    {
        var linkRoot = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "visual-judge-reparse-root",
            Guid.NewGuid().ToString("N"));
        var targetRoot = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "visual-judge-reparse-root-target",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.GetDirectoryName(linkRoot)!);
        Directory.CreateDirectory(targetRoot);
        var rendered = RenderPngArtifact(
            targetRoot,
            "root-reparse-expected",
            "root-reparse-expected",
            "<main class=\"expected\">root reparse expected</main>",
            ".expected{color:#123456;background:#f8fafc}",
            new RenderQaResolution("desktop", 32, 24, 1.0));
        var relativeImagePath = Path.GetRelativePath(targetRoot, new Uri(rendered.Image.Uri).LocalPath);
        var linkedImagePath = Path.Combine(linkRoot, relativeImagePath);
        var handler = new FakeVisualJudgeHandler("""
            {
              "verdict_id": "should-not-be-called",
              "score": 1.0,
              "passed": true,
              "defects": []
            }
            """);

        try
        {
            try
            {
                Directory.CreateSymbolicLink(linkRoot, targetRoot);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
            {
                Assert.Equal(0, handler.CallCount);
                Assert.Null(handler.CapturedBody);
                return;
            }

            var judge = CreateRealHttpJudge(
                handler,
                resolver: new ServerOwnedLocalPngComparisonProvider(
                    linkRoot,
                    maxArtifactBytes: ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes));

            var result = await judge.JudgeAsync(CreateRequest(
                expectedUri: new Uri(linkedImagePath).AbsoluteUri,
                expectedSha256: $"sha256:{rendered.Sha256}"));

            Assert.False(result.IsSuccess);
            Assert.Equal("EXTERNAL_VISUAL_JUDGE_IMAGE_UNRESOLVABLE", result.ErrorCode);
            Assert.Equal(0, handler.CallCount);
            Assert.Null(handler.CapturedBody);
        }
        finally
        {
            TryDeleteDirectory(linkRoot);
            TryDeleteDirectory(targetRoot);
        }
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_TerminalHttpReceiptBindsExactRequestPayloadDigest()
    {
        var handler = new FakeVisualJudgeHandler(
            "private terminal body should not leak",
            HttpStatusCode.InternalServerError);
        var judge = CreateHttpJudge(handler);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.MaxServerOwnedHttpAttemptCount, handler.CallCount);
        Assert.True(result.Value!.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked, result.Value.Status);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.MaxServerOwnedHttpAttemptCount.ToString(),
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        AssertPayloadDigestReceiptMatchesCapturedRequest(result.Value!, handler);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_MissingProviderConfigReturnsProviderRequiredWithoutHandlerCall()
    {
        var handler = new FakeVisualJudgeHandler("""
            {
              "score": 1.0,
              "passed": true,
              "defects": []
            }
            """);
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-judge.test/v1/judge"),
                ProviderName: "fake-provider",
                ModelId: "fake-vision-model",
                ApiKey: null,
                Metadata: new Dictionary<string, string>
                {
                    [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString
                }),
            new HttpClient(handler));

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, handler.CallCount);
        Assert.True(result.Value!.IsBlocked);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, result.Value.Status);
        Assert.False(result.Value.Provider.ExternalCallExecuted);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, result.Value.EvidenceRefs);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker,
            result.Value.Metadata["provider_required_reason"]);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_RejectsLoopbackHttpBeforeHandlerAndRedactsEndpointAndKey()
    {
        var secret = "safe-loopback-http-key-" + Guid.NewGuid().ToString("N");
        const string rawEndpoint = "http://127.0.0.1:43187/v1/judge";
        var handler = new FakeVisualJudgeHandler("""
            {
              "score": 1.0,
              "passed": true,
              "defects": []
            }
            """);
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri(rawEndpoint),
                ProviderName: "openai",
                ModelId: "gpt-vision-real",
                ApiKey: secret),
            new HttpClient(handler));

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("EXTERNAL_VISUAL_JUDGE_ENDPOINT_SCHEME_FORBIDDEN", result.ErrorCode);
        Assert.Equal(0, handler.CallCount);
        Assert.Null(result.Value);

        var serialized = JsonSerializer.Serialize(new
        {
            result.ErrorCode,
            result.ErrorMessage,
            result.Value
        });
        Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(rawEndpoint, serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("127.0.0.1", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("43187", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("loopback HTTP is allowed", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_RejectsLocalPrivateHttpsBeforeHandlerAndRedactsEndpointAndKey()
    {
        var secret = "safe-private-https-key-" + Guid.NewGuid().ToString("N");
        var invalidCases = new[]
        {
            new
            {
                Endpoint = "https://localhost/v1/judge",
                Forbidden = new[] { "https://localhost/v1/judge", "localhost" }
            },
            new
            {
                Endpoint = "https://api.localhost/v1/judge",
                Forbidden = new[] { "https://api.localhost/v1/judge", "api.localhost", "localhost" }
            },
            new
            {
                Endpoint = "https://visual.local/v1/judge",
                Forbidden = new[] { "https://visual.local/v1/judge", "visual.local" }
            },
            new
            {
                Endpoint = "https://127.0.0.1/v1/judge",
                Forbidden = new[] { "https://127.0.0.1/v1/judge", "127.0.0.1" }
            },
            new
            {
                Endpoint = "https://10.0.0.1/v1/judge",
                Forbidden = new[] { "https://10.0.0.1/v1/judge", "10.0.0.1" }
            },
            new
            {
                Endpoint = "https://169.254.169.254/latest/meta-data",
                Forbidden = new[] { "https://169.254.169.254/latest/meta-data", "169.254.169.254", "meta-data" }
            },
            new
            {
                Endpoint = "https://192.168.1.1/v1/judge",
                Forbidden = new[] { "https://192.168.1.1/v1/judge", "192.168.1.1" }
            },
            new
            {
                Endpoint = "https://[::1]/v1/judge",
                Forbidden = new[] { "https://[::1]/v1/judge", "[::1]", "::1" }
            },
            new
            {
                Endpoint = "https://[fc00::1]/v1/judge",
                Forbidden = new[] { "https://[fc00::1]/v1/judge", "[fc00::1]", "fc00::1" }
            },
            new
            {
                Endpoint = "https://[fe80::1]/v1/judge",
                Forbidden = new[] { "https://[fe80::1]/v1/judge", "[fe80::1]", "fe80::1" }
            }
        };

        foreach (var testCase in invalidCases)
        {
            var handler = new FakeVisualJudgeHandler("""
                {
                  "score": 1.0,
                  "passed": true,
                  "defects": []
                }
                """);
            var judge = new ExternalHttpDesignConversionVisualJudge(
                new DesignConversionExternalVisualJudgeOptions(
                    Endpoint: new Uri(testCase.Endpoint),
                    ProviderName: "openai",
                    ModelId: "gpt-vision-real",
                    ApiKey: secret),
                new HttpClient(handler));

            var result = await judge.JudgeAsync(CreateRequest());

            Assert.False(result.IsSuccess);
            Assert.Equal("EXTERNAL_VISUAL_JUDGE_ENDPOINT_DESTINATION_FORBIDDEN", result.ErrorCode);
            Assert.Equal(0, handler.CallCount);
            Assert.Null(result.Value);

            var serialized = JsonSerializer.Serialize(new
            {
                result.ErrorCode,
                result.ErrorMessage,
                result.Value
            });
            Assert.DoesNotContain(secret, serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(testCase.Endpoint, serialized, StringComparison.OrdinalIgnoreCase);
            foreach (var fragment in testCase.Forbidden)
                Assert.DoesNotContain(fragment, serialized, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExternalVisualJudgeSecureNetwork_PrivateOnlyDnsAliasFailsClosedWithoutConnector()
    {
        const string host = "private-only.visual-judge.test";
        const string privateAddress = "10.8.0.4";
        var connectorCalls = 0;

        var result = await DesignConversionExternalVisualJudgeNetwork.ConnectPinnedAsync(
            new DnsEndPoint(host, 9443),
            (_, _) => Task.FromResult(OperationResult<IReadOnlyList<IPAddress>>.Ok([IPAddress.Parse(privateAddress)])),
            (_, _, _) =>
            {
                connectorCalls++;
                return ValueTask.FromResult<Stream>(new MemoryStream());
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionExternalVisualJudgeNetwork.DestinationBlockedErrorCode, result.ErrorCode);
        Assert.Equal(0, connectorCalls);
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(new { result.ErrorCode, result.ErrorMessage, result.Value }),
            host,
            privateAddress,
            "9443");
    }

    [Fact]
    public async Task ExternalVisualJudgeSecureNetwork_MixedPublicPrivateDnsFailsClosedWithoutConnector()
    {
        const string host = "mixed-answer.visual-judge.test";
        const string publicAddress = "93.184.216.34";
        const string privateAddress = "192.168.1.7";
        var connectorCalls = 0;

        var result = await DesignConversionExternalVisualJudgeNetwork.ConnectPinnedAsync(
            new DnsEndPoint(host, 443),
            (_, _) => Task.FromResult(OperationResult<IReadOnlyList<IPAddress>>.Ok(
                [IPAddress.Parse(publicAddress), IPAddress.Parse(privateAddress)])),
            (_, _, _) =>
            {
                connectorCalls++;
                return ValueTask.FromResult<Stream>(new MemoryStream());
            });

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionExternalVisualJudgeNetwork.DestinationBlockedErrorCode, result.ErrorCode);
        Assert.Equal(0, connectorCalls);
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(new { result.ErrorCode, result.ErrorMessage, result.Value }),
            host,
            publicAddress,
            privateAddress,
            "443");
    }

    [Fact]
    public async Task ExternalVisualJudgeSecureNetwork_PublicOnlyDnsConnectsPinnedAddressAndOriginalPort()
    {
        const string host = "public-only.visual-judge.test";
        const string publicAddress = "93.184.216.34";
        var expectedAddress = IPAddress.Parse(publicAddress);
        IPAddress? capturedAddress = null;
        var capturedPort = 0;
        var connectorCalls = 0;
        var harmlessStream = new MemoryStream();

        var result = await DesignConversionExternalVisualJudgeNetwork.ConnectPinnedAsync(
            new DnsEndPoint(host, 9443),
            (_, _) => Task.FromResult(OperationResult<IReadOnlyList<IPAddress>>.Ok([expectedAddress])),
            (address, port, _) =>
            {
                connectorCalls++;
                capturedAddress = address;
                capturedPort = port;
                return ValueTask.FromResult<Stream>(harmlessStream);
            });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, connectorCalls);
        Assert.Equal(expectedAddress, capturedAddress);
        Assert.Equal(9443, capturedPort);
        Assert.Same(harmlessStream, result.Value);
        result.Value!.Dispose();
    }

    [Fact]
    public async Task ExternalVisualJudgeSecureNetwork_DnsFailureAndEmptyAnswersAreSanitizedAndFailClosed()
    {
        const string failureHost = "dns-failure.visual-judge.test";
        const string emptyHost = "dns-empty.visual-judge.test";
        const string rawDnsException = "raw DNS failure for dns-failure.visual-judge.test 93.184.216.34";
        var connectorCalls = 0;
        Task<OperationResult<IReadOnlyList<IPAddress>>> failingResolver(string host, CancellationToken cancellationToken)
            => throw new InvalidOperationException(rawDnsException);

        var failure = await DesignConversionExternalVisualJudgeNetwork.ConnectPinnedAsync(
            new DnsEndPoint(failureHost, 9443),
            failingResolver,
            (_, _, _) =>
            {
                connectorCalls++;
                return ValueTask.FromResult<Stream>(new MemoryStream());
            });

        var empty = await DesignConversionExternalVisualJudgeNetwork.ConnectPinnedAsync(
            new DnsEndPoint(emptyHost, 9443),
            (_, _) => Task.FromResult(OperationResult<IReadOnlyList<IPAddress>>.Ok([])),
            (_, _, _) =>
            {
                connectorCalls++;
                return ValueTask.FromResult<Stream>(new MemoryStream());
            });

        Assert.False(failure.IsSuccess);
        Assert.Equal(DesignConversionExternalVisualJudgeNetwork.DnsResolutionFailedErrorCode, failure.ErrorCode);
        Assert.False(empty.IsSuccess);
        Assert.Equal(DesignConversionExternalVisualJudgeNetwork.DnsEmptyErrorCode, empty.ErrorCode);
        Assert.Equal(0, connectorCalls);
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(new { failure.ErrorCode, failure.ErrorMessage, failure.Value }),
            failureHost,
            "93.184.216.34",
            rawDnsException,
            "9443");
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(new { empty.ErrorCode, empty.ErrorMessage, empty.Value }),
            emptyHost,
            "9443");
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_SecureNetworkFailureReturnsTransportBlockedWithoutIpPortApiKeyLeak()
    {
        var secret = "safe-network-redaction-key-" + Guid.NewGuid().ToString("N");
        const string host = "redaction-network.visual-judge.test";
        const string endpoint = "https://redaction-network.visual-judge.test:9443/v1/judge";
        const string privateAddress = "10.12.13.14";
        var connectorCalls = 0;
        using var httpClient = DesignConversionExternalVisualJudgeNetwork.CreateSecureHttpClient(
            (_, _) => Task.FromResult(OperationResult<IReadOnlyList<IPAddress>>.Ok([IPAddress.Parse(privateAddress)])),
            (_, _, _) =>
            {
                connectorCalls++;
                return ValueTask.FromResult<Stream>(new MemoryStream());
            });
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri(endpoint),
                ProviderName: "openai",
                ModelId: "gpt-vision-real",
                ApiKey: secret),
            httpClient,
            DefaultPngFixture.Value.Resolver);

        var result = await judge.JudgeAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusTransportBlocked, result.Value.Status);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureHttpRequestFailedCode,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey]);
        Assert.Equal("2", result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Null(result.Value.EstimatedCostUsd);
        Assert.Equal(0, connectorCalls);
        AssertTransportAttemptedWithoutVisualAiExecution(result.Value);
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(result.Value),
            secret,
            host,
            endpoint,
            privateAddress,
            "9443",
            "/v1/judge");
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_SendsBoundedInlinePngPayloadIdentityIdempotencyAndMapsStructuredResponse()
    {
        var fixture = CreatePngFixture();
        var secret = "pass-visual-http-secret-" + Guid.NewGuid().ToString("N");
        const string posixDiffPath = "/tmp/private-diff.png";
        const string posixMetadataPath = "/tmp/private-metadata.png";
        const string relativeMetadataPath = "../private/metadata.png";
        const string dotRelativeMetadataPath = "./private/rendered.png";
        const string safeArtifactRef = "artifact://safe/reference.png";
        const string safeHttpsRef = "https://cdn.example.test/safe/reference.png";
        const string credentialedRefSecret = "credentialed-ref-secret";
        const string encodedCredentialRefSecret = "encoded-credential-ref-secret";
        const string arbitraryPlainRef = "arbitrary-plain-reference-value";
        var handler = new FakeVisualJudgeHandler("""
            {
              "judge_request_id": "judge-request-001",
              "request_id": "design-request-001",
              "provider": "openai",
              "judge_result_id": "real-http-result-001",
              "verdict_id": "real-http-verdict-001",
              "score": 0.72,
              "passed": false,
              "confidence": 0.64,
              "model_id": "gpt-vision-real",
              "cost_estimate_usd": 0.0123,
              "tokens": { "input": 180, "output": 36, "total": 216 },
              "evidence_refs": [ "real-provider:evidence" ],
              "issues": [
                {
                  "id": "issue-layout-001",
                  "category": "layout",
                  "severity": "major",
                  "description": "Rendered spacing differs from source design.",
                  "x": 12,
                  "y": 16,
                  "width": 80,
                  "height": 24,
                  "evidence_image_ids": [ "expected-shot", "actual-render" ]
                }
              ]
            }
            """);
        var judge = CreateRealHttpJudge(handler, "openai", "gpt-vision-real", secret, resolver: fixture.Resolver);

        var result = await judge.JudgeAsync(CreateRequest(
            fixture: fixture,
            diffUri: posixDiffPath,
            metadata: new Dictionary<string, string>
            {
                ["safe_trace"] = "pass-visual-http",
                ["api_key"] = "private-metadata-secret",
                ["relative_asset_ref"] = relativeMetadataPath,
                ["safe_artifact_ref"] = safeArtifactRef,
                ["safe_https_ref"] = safeHttpsRef,
                ["unsafe_https_ref"] = $"https://cdn.example.test/private/reference.png?api_key={credentialedRefSecret}",
                ["encoded_https_ref"] = $"https://cdn.example.test/private/reference.png?access%5Ftoken={encodedCredentialRefSecret}",
                ["plain_text_ref"] = arbitraryPlainRef
            },
            expectedMetadata: new Dictionary<string, string>
            {
                ["posix_asset_ref"] = posixMetadataPath
            },
            actualMetadata: new Dictionary<string, string>
            {
                ["dot_relative_asset_ref"] = dotRelativeMetadataPath
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal("https://visual-judge.example/v1/judge", handler.CapturedRequestUri);
        Assert.Equal($"Bearer {secret}", Assert.Single(handler.CapturedHeaderValues("Authorization")));
        Assert.Equal("judge-request-001", Assert.Single(handler.CapturedHeaderValues("Idempotency-Key")));
        Assert.NotNull(handler.CapturedBody);
        Assert.DoesNotContain(secret, handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("private-metadata-secret", handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(posixDiffPath, handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(posixMetadataPath, handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(relativeMetadataPath, handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(dotRelativeMetadataPath, handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(credentialedRefSecret, handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(encodedCredentialRefSecret, handler.CapturedBody, StringComparison.Ordinal);
        Assert.DoesNotContain(arbitraryPlainRef, handler.CapturedBody, StringComparison.Ordinal);
        Assert.Contains(safeArtifactRef, handler.CapturedBody, StringComparison.Ordinal);
        Assert.Contains(safeHttpsRef, handler.CapturedBody, StringComparison.Ordinal);

        using var bodyDocument = JsonDocument.Parse(handler.CapturedBody!);
        var body = bodyDocument.RootElement;
        Assert.Equal("judge-request-001", body.GetProperty("judge_request_id").GetString());
        Assert.Equal("design-request-001", body.GetProperty("request_id").GetString());
        Assert.Equal("openai", body.GetProperty("provider").GetString());
        Assert.Equal("gpt-vision-real", body.GetProperty("model").GetString());
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.PayloadMode, body.GetProperty("image_payload_mode").GetString());
        Assert.Equal("expected-shot", body.GetProperty("expected_screenshot").GetProperty("image_id").GetString());
        Assert.Equal("actual-render", body.GetProperty("rendered_image").GetProperty("image_id").GetString());
        Assert.Equal("artifact-ref:expected-shot", body.GetProperty("expected_screenshot").GetProperty("uri").GetString());
        Assert.Equal("artifact-ref:actual-render", body.GetProperty("rendered_image").GetProperty("uri").GetString());
        Assert.Equal("artifact-ref:diff-artifact", body.GetProperty("diff_artifact").GetProperty("uri").GetString());
        Assert.Equal(fixture.Expected.Sha256, body.GetProperty("expected_screenshot").GetProperty("sha256").GetString());
        Assert.Equal(fixture.Actual.Sha256, body.GetProperty("rendered_image").GetProperty("sha256").GetString());
        AssertInlinePngContent(body.GetProperty("expected_screenshot"), fixture.ExpectedBytes, fixture.ExpectedSha256);
        AssertInlinePngContent(body.GetProperty("rendered_image"), fixture.ActualBytes, fixture.ActualSha256);
        Assert.Equal("[redacted]", body.GetProperty("metadata").GetProperty("api_key").GetString());
        Assert.Equal("artifact-ref:relative_asset_ref", body.GetProperty("metadata").GetProperty("relative_asset_ref").GetString());
        Assert.Equal(safeArtifactRef, body.GetProperty("metadata").GetProperty("safe_artifact_ref").GetString());
        Assert.Equal(safeHttpsRef, body.GetProperty("metadata").GetProperty("safe_https_ref").GetString());
        Assert.Equal("[redacted]", body.GetProperty("metadata").GetProperty("unsafe_https_ref").GetString());
        Assert.Equal("[redacted]", body.GetProperty("metadata").GetProperty("encoded_https_ref").GetString());
        Assert.Equal("[redacted]", body.GetProperty("metadata").GetProperty("plain_text_ref").GetString());
        Assert.Equal(
            "artifact-ref:posix_asset_ref",
            body.GetProperty("expected_screenshot").GetProperty("metadata").GetProperty("posix_asset_ref").GetString());
        Assert.Equal(
            "artifact-ref:dot_relative_asset_ref",
            body.GetProperty("rendered_image").GetProperty("metadata").GetProperty("dot_relative_asset_ref").GetString());

        Assert.False(result.Value!.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusOk, result.Value.Status);
        Assert.True(result.Value.Provider.ExternalCallExecuted);
        Assert.Equal("judge-request-001:server-owned-visual-result", result.Value.JudgeResultId);
        Assert.Equal("openai", result.Value.Provider.ProviderName);
        Assert.Equal("gpt-vision-real", result.Value.Provider.ModelId);
        Assert.Equal("real-http-verdict-001", result.Value.Verdict.VerdictId);
        Assert.Equal(0.72, result.Value.Verdict.Score);
        Assert.False(result.Value.Verdict.Passed);
        Assert.Equal("layout", Assert.Single(result.Value.Verdict.Defects).Category);
        Assert.Equal("0.64", result.Value.Metadata["confidence"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.TrueString, result.Value.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            result.Value.JudgeResultId,
            result.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey]);
        Assert.StartsWith("sha256:", result.Value.Metadata["provider_raw_visual_result_id_sha256"], StringComparison.Ordinal);
        var serializedResult = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(secret, serializedResult, StringComparison.Ordinal);
        Assert.DoesNotContain("real-http-result-001", serializedResult, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_KnownContentLengthOversizeReturnsResponsePolicyBlockWithCostReceipt()
    {
        const string apiKey = "safe-known-oversize-key-not-for-result";
        const string privateMetadata = "private-known-oversize-metadata-not-for-result";
        const string privateOptionsMetadata = "private-known-oversize-options-not-for-result";
        const string endpoint = "https://visual-judge.example:9443/v1/judge?api_key=endpoint-secret";
        const string bodyPrefix = "private-known-oversize-provider-body";
        var handler = new FakeVisualJudgeHandler(bodyPrefix + new string('x', 300_000));

        var result = await CreateRealHttpJudge(
                handler,
                apiKey: apiKey,
                serverOwnedCostPerRequestUsd: 0.0042m,
                endpoint: endpoint,
                metadata: new Dictionary<string, string>
                {
                    ["private_options_metadata"] = privateOptionsMetadata
                })
            .JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
            {
                ["api_key"] = privateMetadata
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        AssertResponsePolicyBlockedWithReceipt(
            result.Value!,
            expectedEstimatedCostUsd: 0.0042m,
            expectedCostPerAttemptUsd: "0.0042",
            expectedTotalCostUsd: "0.0042",
            expectedCostAccounting: ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt,
            expectedCostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            expectedPricingStatus: "configured");
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(result.Value),
            apiKey,
            privateMetadata,
            privateOptionsMetadata,
            endpoint,
            "visual-judge.example",
            "9443",
            "/v1/judge",
            "endpoint-secret",
            bodyPrefix,
            "EXTERNAL_VISUAL_JUDGE_RESPONSE_TOO_LARGE");
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_UnknownLengthStreamedOversizeReturnsResponsePolicyBlockWithCostReceipt()
    {
        const string apiKey = "safe-streamed-oversize-key-not-for-result";
        const string privateMetadata = "private-streamed-oversize-metadata-not-for-result";
        const string privateOptionsMetadata = "private-streamed-oversize-options-not-for-result";
        const string endpoint = "https://visual-judge.example:9443/v1/judge?api_key=endpoint-secret";
        const string bodyPrefix = "private-streamed-oversize-provider-body";
        var bodyBytes = Encoding.UTF8.GetBytes(bodyPrefix + new string('y', 300_000));
        var handler = new ResponseContentVisualJudgeHandler(
            new StreamBackedHttpContent(() => new MemoryStream(bodyBytes, writable: false)));

        var result = await CreateRealHttpJudge(
                handler,
                apiKey: apiKey,
                serverOwnedCostPerRequestUsd: 0.0042m,
                endpoint: endpoint,
                metadata: new Dictionary<string, string>
                {
                    ["private_options_metadata"] = privateOptionsMetadata
                })
            .JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
            {
                ["api_key"] = privateMetadata
            }));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        AssertResponsePolicyBlockedWithReceipt(
            result.Value!,
            expectedEstimatedCostUsd: 0.0042m,
            expectedCostPerAttemptUsd: "0.0042",
            expectedTotalCostUsd: "0.0042",
            expectedCostAccounting: ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt,
            expectedCostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            expectedPricingStatus: "configured");
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(result.Value),
            apiKey,
            privateMetadata,
            privateOptionsMetadata,
            endpoint,
            "visual-judge.example",
            "9443",
            "/v1/judge",
            "endpoint-secret",
            bodyPrefix,
            "EXTERNAL_VISUAL_JUDGE_RESPONSE_TOO_LARGE");
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_ResponsePolicyBlockMissingOrInvalidPriceLeavesCostUnpricedWithAttempt()
    {
        var cases = new[]
        {
            new ResponsePolicyPricingCase(null, "unconfigured"),
            new ResponsePolicyPricingCase(-0.01m, "invalid")
        };

        foreach (var testCase in cases)
        {
            var handler = new FakeVisualJudgeHandler("private-unpriced-oversize-body" + new string('z', 300_000));

            var result = await CreateRealHttpJudge(
                    handler,
                    serverOwnedCostPerRequestUsd: testCase.ServerOwnedCostPerRequestUsd)
                .JudgeAsync(CreateRequest());

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, handler.CallCount);
            AssertResponsePolicyBlockedWithReceipt(
                result.Value!,
                expectedEstimatedCostUsd: null,
                expectedCostPerAttemptUsd: string.Empty,
                expectedTotalCostUsd: string.Empty,
                expectedCostAccounting: ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingUnknown,
                expectedCostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced,
                expectedPricingStatus: testCase.ExpectedPricingStatus);
        }
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_FailsClosedForMalformedTimeoutAndIdentityMismatch()
    {
        var (malformed, malformedHandler) = await JudgeValidationResponseAsync("{ not-json");
        AssertResponseValidationBlockedWithReceipt(
            malformed,
            malformedHandler,
            ExternalHttpDesignConversionVisualJudge.ResponseValidationInvalidJsonCode,
            "{ not-json",
            "not-json",
            "JsonException");

        var timeoutClient = new HttpClient(new DelayingVisualJudgeHandler(TimeSpan.FromSeconds(5)))
        {
            Timeout = TimeSpan.FromMilliseconds(50)
        };
        var timeoutJudge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-judge.example/v1/judge"),
                ProviderName: "openai",
                ModelId: "gpt-vision-real",
                ApiKey: "safe-test-key"),
            timeoutClient,
            DefaultPngFixture.Value.Resolver);
        var timeout = await timeoutJudge.JudgeAsync(CreateRequest());
        Assert.True(timeout.IsSuccess, timeout.ErrorMessage);
        Assert.True(timeout.Value!.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusTransportBlocked, timeout.Value.Status);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureRequestTimeoutCode,
            timeout.Value.Metadata[ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey]);
        Assert.Equal("1", timeout.Value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Null(timeout.Value.EstimatedCostUsd);
        AssertTransportAttemptedWithoutVisualAiExecution(timeout.Value);

        var (requestMismatch, requestMismatchHandler) = await JudgeValidationResponseAsync("""
            {
              "judge_request_id": "private-other-judge-request",
              "request_id": "private-other-design-request",
              "provider": "openai",
              "model_id": "gpt-vision-real",
              "tokens": { "private_token_secret": 7 },
              "score": 0.91,
              "passed": true,
              "issues": []
            }
            """);
        AssertResponseValidationBlockedWithReceipt(
            requestMismatch,
            requestMismatchHandler,
            ExternalHttpDesignConversionVisualJudge.ResponseValidationIdentityMismatchCode,
            "private-other-judge-request",
            "private-other-design-request",
            "private_token_secret");

        var (providerMismatch, providerMismatchHandler) = await JudgeValidationResponseAsync("""
            {
              "judge_request_id": "judge-request-001",
              "request_id": "design-request-001",
              "provider": "private-provider-gemini",
              "model_id": "gpt-vision-real",
              "tokens": { "private_provider_token": 8 },
              "score": 0.91,
              "passed": true,
              "issues": []
            }
            """);
        AssertResponseValidationBlockedWithReceipt(
            providerMismatch,
            providerMismatchHandler,
            ExternalHttpDesignConversionVisualJudge.ResponseValidationProviderMismatchCode,
            "private-provider-gemini",
            "private_provider_token");

        var (modelMismatch, modelMismatchHandler) = await JudgeValidationResponseAsync("""
            {
              "judge_request_id": "judge-request-001",
              "request_id": "design-request-001",
              "provider": "openai",
              "model_id": "private-other-vision-model",
              "tokens": { "private_model_token": 9 },
              "score": 0.91,
              "passed": true,
              "issues": []
            }
            """);
        AssertResponseValidationBlockedWithReceipt(
            modelMismatch,
            modelMismatchHandler,
            ExternalHttpDesignConversionVisualJudge.ResponseValidationModelMismatchCode,
            "private-other-vision-model",
            "private_model_token");
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_ResponseValidationSchemaInvalidReturnsBlockedCostReceiptWithoutLeaks()
    {
        var cases = new[]
        {
            new
            {
                Body = """
                    {
                      "judge_request_id": "judge-request-001",
                      "request_id": "design-request-001",
                      "provider": "openai",
                      "model_id": "gpt-vision-real",
                      "passed": true,
                      "issues": [],
                      "tokens": { "private_missing_score_token": 1 }
                    }
                    """,
                Forbidden = new[] { "private_missing_score_token" }
            },
            new
            {
                Body = """
                    {
                      "judge_request_id": "judge-request-001",
                      "request_id": "design-request-001",
                      "provider": "openai",
                      "model_id": "gpt-vision-real",
                      "score": "private-invalid-score",
                      "passed": true,
                      "issues": []
                    }
                    """,
                Forbidden = new[] { "private-invalid-score" }
            },
            new
            {
                Body = """
                    {
                      "judge_request_id": "judge-request-001",
                      "request_id": "design-request-001",
                      "provider": "openai",
                      "model_id": "gpt-vision-real",
                      "score": 0.91,
                      "issues": [],
                      "tokens": { "private_missing_passed_token": 2 }
                    }
                    """,
                Forbidden = new[] { "private_missing_passed_token" }
            },
            new
            {
                Body = """
                    {
                      "judge_request_id": "judge-request-001",
                      "request_id": "design-request-001",
                      "provider": "openai",
                      "model_id": "gpt-vision-real",
                      "score": 0.91,
                      "passed": "private-not-bool",
                      "issues": []
                    }
                    """,
                Forbidden = new[] { "private-not-bool" }
            },
            new
            {
                Body = """
                    {
                      "judge_request_id": "judge-request-001",
                      "request_id": "design-request-001",
                      "provider": "openai",
                      "model_id": "gpt-vision-real",
                      "score": 0.91,
                      "passed": true,
                      "tokens": { "private_missing_issues_token": 3 }
                    }
                    """,
                Forbidden = new[] { "private_missing_issues_token" }
            },
            new
            {
                Body = """
                    {
                      "judge_request_id": "judge-request-001",
                      "request_id": "design-request-001",
                      "provider": "openai",
                      "model_id": "gpt-vision-real",
                      "score": 0.91,
                      "passed": true,
                      "defects": "private-not-array"
                    }
                    """,
                Forbidden = new[] { "private-not-array" }
            }
        };

        foreach (var testCase in cases)
        {
            var (result, handler) = await JudgeValidationResponseAsync(testCase.Body);

            AssertResponseValidationBlockedWithReceipt(
                result,
                handler,
                ExternalHttpDesignConversionVisualJudge.ResponseValidationSchemaInvalidCode,
                testCase.Forbidden);
        }
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_ResponseValidationMissingOrInvalidPriceLeavesCostUnpricedWithAttempt()
    {
        var cases = new[]
        {
            new ResponsePolicyPricingCase(null, "unconfigured"),
            new ResponsePolicyPricingCase(-0.01m, "invalid")
        };

        foreach (var testCase in cases)
        {
            var (result, handler) = await JudgeValidationResponseAsync(
                """
                {
                  "judge_request_id": "judge-request-001",
                  "request_id": "design-request-001",
                  "provider": "openai",
                  "model_id": "gpt-vision-real",
                  "score": "private-invalid-score-for-unpriced-validation",
                  "passed": true,
                  "issues": []
                }
                """,
                testCase.ServerOwnedCostPerRequestUsd);

            AssertResponseValidationBlockedWithReceipt(
                result,
                handler,
                ExternalHttpDesignConversionVisualJudge.ResponseValidationSchemaInvalidCode,
                expectedEstimatedCostUsd: null,
                expectedCostPerAttemptUsd: string.Empty,
                expectedTotalCostUsd: string.Empty,
                expectedCostAccounting: ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingUnknown,
                expectedCostSource: DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceUnknownUnpriced,
                expectedPricingStatus: testCase.ExpectedPricingStatus,
                forbiddenFragments: new[] { "private-invalid-score-for-unpriced-validation" });
        }
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_ResponseBodyReadTimesOutAfterHeadersReturnsBlockedResponseReadCostReceipt()
    {
        const string apiKey = "safe-body-timeout-key-not-for-result";
        const string privateMetadata = "private-body-timeout-metadata-not-for-result";
        const string endpoint = "https://visual-judge.example/v1/judge";
        const string endpointHost = "visual-judge.example";
        var handler = new ResponseContentVisualJudgeHandler(
            new StreamBackedHttpContent(() => new DelayingReadStream(TimeSpan.FromSeconds(5))));
        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(75)
        };
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri(endpoint),
                ProviderName: "openai",
                ModelId: "gpt-vision-real",
                ApiKey: apiKey,
                ServerOwnedCostPerRequestUsd: 0.0042m),
            httpClient,
            DefaultPngFixture.Value.Resolver);

        var stopwatch = Stopwatch.StartNew();
        var result = await judge.JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
        {
            ["api_key"] = privateMetadata
        }));
        stopwatch.Stop();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        Assert.True(result.Value!.IsBlocked);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.StatusResponseReadBlocked,
            result.Value.Status);
        Assert.Equal(0.0042m, result.Value.EstimatedCostUsd);
        AssertResponseReadBlockedWithReceipt(
            result.Value,
            ExternalHttpDesignConversionVisualJudge.ResponseReadFailureTimeoutCode,
            expectedTotalCostUsd: "0.0042");
        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(2),
            $"Expected bounded body timeout, actual elapsed time was {stopwatch.Elapsed}.");
        AssertSerializedDoesNotContain(
            JsonSerializer.Serialize(result.Value),
            apiKey,
            privateMetadata,
            endpoint,
            endpointHost,
            "/v1/judge",
            nameof(OperationCanceledException),
            "EXTERNAL_VISUAL_JUDGE_TIMEOUT");
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_ResponseBodyReadFailuresReturnBlockedResponseReadCostReceiptWithoutLeaks()
    {
        const string apiKey = "safe-body-read-key-not-for-result";
        var cases = new[]
        {
            new ExceptionReadCase(
                new HttpRequestException("private-read-secret https://visual-judge.example/private?token=body-secret"),
                "private-read-secret",
                "body-secret"),
            new ExceptionReadCase(
                new IOException("private-io-secret https://visual-judge.example/private?token=io-secret"),
                "private-io-secret",
                "io-secret")
        };

        foreach (var testCase in cases)
        {
            var handler = new ResponseContentVisualJudgeHandler(
                new StreamBackedHttpContent(() => new ThrowingReadStream(testCase.Exception)));
            var result = await CreateRealHttpJudge(
                    handler,
                    apiKey: apiKey,
                    serverOwnedCostPerRequestUsd: 0.0042m)
                .JudgeAsync(CreateRequest());

            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(1, handler.CallCount);
            Assert.True(result.Value!.IsBlocked);
            Assert.Equal(
                ExternalHttpDesignConversionVisualJudge.StatusResponseReadBlocked,
                result.Value.Status);
            Assert.Equal(0.0042m, result.Value.EstimatedCostUsd);
            AssertResponseReadBlockedWithReceipt(
                result.Value,
                ExternalHttpDesignConversionVisualJudge.ResponseReadFailureExceptionCode,
                expectedTotalCostUsd: "0.0042");
            AssertSerializedDoesNotContain(
                JsonSerializer.Serialize(result.Value),
                apiKey,
                testCase.PrivateMessageFragment,
                testCase.PrivateTokenFragment,
                "visual-judge.example/private",
                testCase.Exception.GetType().Name,
                "EXTERNAL_VISUAL_JUDGE_RESPONSE_READ_FAILED");
        }
    }

    [Fact]
    public async Task ExternalHttpVisualJudge_CallerCancellationDuringResponseBodyReadPropagates()
    {
        var handler = new ResponseContentVisualJudgeHandler(
            new StreamBackedHttpContent(() => new DelayingReadStream(TimeSpan.FromSeconds(5))));
        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        var judge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-judge.example/v1/judge"),
                ProviderName: "openai",
                ModelId: "gpt-vision-real",
                ApiKey: "safe-test-key",
                ServerOwnedCostPerRequestUsd: 0.0042m),
            httpClient,
            DefaultPngFixture.Value.Resolver);
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(75));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => judge.JudgeAsync(CreateRequest(), cancellation.Token));
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task VisualJudgeResult_CanFeedComparisonAndDpoVisualVerdictModel()
    {
        var handler = new FakeVisualJudgeHandler("""
            {
              "verdict_id": "fake-verdict-dpo-feed",
              "score": 0.96,
              "passed": true,
              "defects": [],
              "evidence_refs": [ "fake-provider:dpo-feed" ]
            }
            """);
        var request = CreateRequest();
        var result = await CreateHttpJudge(handler).JudgeAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var comparison = result.Value!.ToComparisonArtifact(request);
        var qaResult = new RenderQaResultRecord(
            ResultId: "qa-result-from-visual-judge",
            RequestId: request.RequestId,
            Expected: comparison.Expected,
            Actual: comparison.Actual,
            Verdict: comparison.Verdict,
            Metrics: comparison.Metrics,
            Warnings: []);
        var chosen = CreateImprovementOutput("chosen-output", comparison.Verdict, accepted: true, qaResult);
        var rejected = CreateImprovementOutput(
            "rejected-output",
            new VisualVerdict(
                VerdictId: "rejected-verdict",
                Score: 0.22,
                Defects:
                [
                    new VisualDefect(
                        DefectId: "rejected-defect",
                        Category: "layout",
                        Severity: "major",
                        Description: "Rejected output has visible layout mismatch.",
                        X: 10,
                        Y: 10,
                        Width: 50,
                        Height: 20,
                        EvidenceImageIds: ["actual-render"])
                ],
                EvidenceRefs: ["rejected:evidence"],
                Passed: false),
            accepted: false,
            qaResult);
        var generator = new DesignConversionDpoGenerator();

        var dpo = generator.BuildTrainReadyRecords(
        [
            new DesignConversionStoredDpoExample(
                ExampleId: "visual-judge-dpo-example",
                UnitId: DesignConversionDpoUnits.CriticUnit,
                InternalUnitStatePayload: "{\"schema\":\"visual-judge-state-v1\"}",
                InternalVectorPayload: [0.96, 0.22, 0.74],
                SourceStructurePayload: "{\"nodes\":[\"frame\",\"button\"]}",
                Chosen: chosen,
                Rejected: rejected,
                TieBreakerPayload: null,
                UsefulnessScore: 0.82,
                NegativeQualityReason: "Rejected has a visible layout mismatch confirmed by visual judge evidence.",
                NegativeReasonTaxonomy: DesignConversionNegativeReasonTaxonomy.SemanticLayoutHierarchy,
                Provenance: new DesignConversionDpoProvenance(
                    SourceRequestId: request.RequestId,
                    SourceKind: DesignConversionSourceKind.Figma.ToString(),
                    SourceManifestRef: "artifact://visual-judge/source-manifest.json",
                    ReviewerRef: "external-visual-judge:fake-provider",
                    CreatedAtUtc: DateTimeOffset.UnixEpoch,
                    Metadata: new Dictionary<string, string>()),
                License: new DesignConversionLicenseScope(
                    LicenseId: "test-license",
                    RightsScope: "unit-test",
                    AllowsTraining: true,
                    ContainsUserPrivateData: false,
                    Metadata: new Dictionary<string, string>()),
                TeacherEvidenceRefs: result.Value.EvidenceRefs,
                Metadata: new Dictionary<string, string>())
        ]);

        Assert.Equal("fake-verdict-dpo-feed", comparison.Verdict.VerdictId);
        Assert.Equal(0.96, qaResult.Verdict.Score);
        Assert.True(dpo.IsSuccess, dpo.ErrorMessage);
        Assert.Single(dpo.Value!.AcceptedRecords);
        Assert.Empty(dpo.Value.QuarantinedRecords);
        Assert.DoesNotContain("fake-provider:dpo-feed", dpo.Value.AcceptedRecords[0].Quarantine.EvidenceRefs);
        Assert.Contains(
            dpo.Value.AcceptedRecords[0].Quarantine.EvidenceRefs,
            evidenceRef => evidenceRef.StartsWith("dcm-dpo-evidence-", StringComparison.Ordinal));
    }

    private static readonly Lazy<VisualJudgePngFixture> DefaultPngFixture = new(CreatePngFixture);

    private static ExternalHttpDesignConversionVisualJudge CreateHttpJudge(
        FakeVisualJudgeHandler handler,
        string apiKey = "safe-test-key",
        decimal? serverOwnedCostPerRequestUsd = 0.0042m,
        IDesignConversionVisualJudgeImageContentResolver? resolver = null)
        => new(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-judge.test/v1/judge"),
                ProviderName: "fake-provider",
                ModelId: "fake-vision-model",
                ApiKey: apiKey,
                ServerOwnedCostPerRequestUsd: serverOwnedCostPerRequestUsd),
            new HttpClient(handler),
            resolver ?? DefaultPngFixture.Value.Resolver);

    private static ExternalHttpDesignConversionVisualJudge CreateRealHttpJudge(
        HttpMessageHandler handler,
        string providerName = "openai",
        string modelId = "gpt-vision-real",
        string apiKey = "safe-test-key",
        decimal? serverOwnedCostPerRequestUsd = 0.0123m,
        string endpoint = "https://visual-judge.example/v1/judge",
        IReadOnlyDictionary<string, string>? metadata = null,
        IDesignConversionVisualJudgeImageContentResolver? resolver = null)
        => new(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri(endpoint),
                ProviderName: providerName,
                ModelId: modelId,
                ApiKey: apiKey,
                Metadata: metadata,
                ServerOwnedCostPerRequestUsd: serverOwnedCostPerRequestUsd),
            new HttpClient(handler),
            resolver ?? DefaultPngFixture.Value.Resolver);

    private static ExternalHttpDesignConversionVisualJudge CreateHttpJudgeWithRetryDelay(
        HttpMessageHandler handler,
        Func<TimeSpan, CancellationToken, Task> retryDelayAsync,
        IDesignConversionVisualJudgeImageContentResolver? resolver = null)
        => new(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-judge.test/v1/judge"),
                ProviderName: "fake-provider",
                ModelId: "fake-vision-model",
                ApiKey: "safe-test-key",
                ServerOwnedCostPerRequestUsd: 0.0042m),
            new HttpClient(handler),
            retryDelayAsync,
            resolver ?? DefaultPngFixture.Value.Resolver);

    private static HttpResponseMessage CreateStatusResponse(
        HttpStatusCode statusCode,
        TimeSpan? retryAfter = null,
        string? body = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (retryAfter.HasValue)
            response.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter.Value);
        if (body is not null)
            response.Content = new StringContent(body, Encoding.UTF8, "text/plain");

        return response;
    }

    private static HttpResponseMessage CreateSuccessfulVisualJudgeResponse()
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                {
                  "judge_request_id": "judge-request-001",
                  "request_id": "design-request-001",
                  "provider": "fake-provider",
                  "model_id": "fake-vision-model",
                  "score": 0.91,
                  "passed": true,
                  "tokens": { "total": 7 },
                  "defects": []
                }
                """, Encoding.UTF8, "application/json")
        };

    private static DesignConversionVisualJudgeRequest CreateRequest(
        string? expectedUri = null,
        string? actualUri = null,
        string? diffUri = "artifact://diff.png",
        IReadOnlyDictionary<string, string>? metadata = null,
        IReadOnlyDictionary<string, string>? expectedMetadata = null,
        IReadOnlyDictionary<string, string>? actualMetadata = null,
        RenderQaResolution? expectedResolution = null,
        RenderQaResolution? actualResolution = null,
        string? expectedSha256 = null,
        string? actualSha256 = null,
        VisualJudgePngFixture? fixture = null)
    {
        fixture ??= DefaultPngFixture.Value;
        var resolution = expectedResolution ?? fixture.Expected.Resolution;
        return new DesignConversionVisualJudgeRequest(
            JudgeRequestId: "judge-request-001",
            RequestId: "design-request-001",
            ExpectedScreenshot: fixture.Expected with
            {
                Resolution = resolution,
                Uri = expectedUri ?? fixture.Expected.Uri,
                Sha256 = expectedSha256 ?? fixture.Expected.Sha256,
                Metadata = expectedMetadata ?? new Dictionary<string, string>()
            },
            RenderedImage: fixture.Actual with
            {
                Resolution = actualResolution ?? resolution,
                Uri = actualUri ?? fixture.Actual.Uri,
                Sha256 = actualSha256 ?? fixture.Actual.Sha256,
                Metadata = actualMetadata ?? new Dictionary<string, string>()
            },
            DiffArtifactUri: diffUri,
            DiffArtifactSha256: "sha256:diff",
            Metadata: metadata ?? new Dictionary<string, string>());
    }

    private static VisualJudgePngFixture CreatePngFixture()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "llm_mvp_core",
            "visual-judge-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var resolution = new RenderQaResolution("desktop", 32, 24, 1.0);
        var expected = RenderPngArtifact(
            root,
            "expected-shot",
            "expected",
            "<main class=\"expected\">reference</main>",
            ".expected{display:grid;color:#123456;background:#f8fafc}",
            resolution);
        var actual = RenderPngArtifact(
            root,
            "actual-render",
            "actual",
            "<main class=\"actual\">rendered</main>",
            ".actual{display:flex;color:#654321;background:#ffffff}",
            resolution);

        return new VisualJudgePngFixture(
            Root: root,
            Expected: expected.Image with { ImageId = "expected-shot", Label = "expected" },
            Actual: actual.Image with { ImageId = "actual-render", Label = "actual" },
            ExpectedBytes: expected.Bytes,
            ActualBytes: actual.Bytes,
            ExpectedSha256: expected.Sha256,
            ActualSha256: actual.Sha256,
            Resolver: new ServerOwnedLocalPngComparisonProvider(
                root,
                maxArtifactBytes: ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes));
    }

    private static (RenderQaImageRecord Image, byte[] Bytes, string Sha256) RenderPngArtifact(
        string root,
        string renderRequestId,
        string requestIdSuffix,
        string html,
        string css,
        RenderQaResolution resolution)
    {
        var render = new DeterministicLocalDesignConversionPngRenderProvider(root)
            .RenderAsync(new DesignConversionRenderRequest(
                RenderRequestId: renderRequestId,
                RequestId: $"visual-judge-test-{requestIdSuffix}",
                Html: html,
                Css: css,
                Resolution: resolution,
                Metadata: new Dictionary<string, string>()))
            .GetAwaiter()
            .GetResult();

        Assert.True(render.IsSuccess, render.ErrorMessage);
        var image = render.Value!.Image;
        var bytes = File.ReadAllBytes(new Uri(image.Uri).LocalPath);
        var sha256 = Sha256Bytes(bytes);
        Assert.Equal($"sha256:{sha256}", image.Sha256);
        return (image, bytes, sha256);
    }

    private static DesignConversionVisualJudgeRequest BuildInvalidImageContentRequest(
        VisualJudgePngFixture fixture,
        string scenario)
    {
        return scenario switch
        {
            "sha_mismatch" => CreateRequest(
                fixture: fixture,
                expectedSha256: "sha256:" + new string('a', 64)),
            "non_png" => CreateRequestForFixtureBytes(
                fixture,
                Encoding.UTF8.GetBytes("not a png"),
                expectedFileName: "not-a-png.bin"),
            "oversize" => CreateRequestForFixtureBytes(
                fixture,
                CreateOversizedBytes(),
                expectedFileName: "oversize.png"),
            "unresolvable" => CreateRequest(
                fixture: fixture,
                expectedUri: "local-png-ref:sha256:" + new string('b', 64),
                expectedSha256: "sha256:" + new string('b', 64)),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown image content failure scenario.")
        };
    }

    private static DesignConversionVisualJudgeRequest CreateRequestForFixtureBytes(
        VisualJudgePngFixture fixture,
        byte[] bytes,
        string expectedFileName)
    {
        var path = Path.Combine(fixture.Root, $"{Guid.NewGuid():N}-{expectedFileName}");
        File.WriteAllBytes(path, bytes);
        return CreateRequest(
            fixture: fixture,
            expectedUri: new Uri(path).AbsoluteUri,
            expectedSha256: "sha256:" + Sha256Bytes(bytes));
    }

    private static byte[] CreateOversizedBytes()
    {
        var bytes = new byte[(int)ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes + 1];
        bytes[0] = 137;
        bytes[1] = 80;
        bytes[2] = 78;
        bytes[3] = 71;
        bytes[4] = 13;
        bytes[5] = 10;
        bytes[6] = 26;
        bytes[7] = 10;
        return bytes;
    }

    private static byte[] CreateLargePngContractBytes(int length, byte seed)
    {
        var bytes = new byte[length];
        bytes[0] = 137;
        bytes[1] = 80;
        bytes[2] = 78;
        bytes[3] = 71;
        bytes[4] = 13;
        bytes[5] = 10;
        bytes[6] = 26;
        bytes[7] = 10;
        for (var i = 8; i < bytes.Length; i++)
            bytes[i] = (byte)(seed + i);
        return bytes;
    }

    private static void AssertInlinePngContent(
        JsonElement image,
        byte[] expectedBytes,
        string expectedSha256)
    {
        Assert.StartsWith("artifact-ref:", image.GetProperty("uri").GetString(), StringComparison.Ordinal);
        var content = image.GetProperty("content");
        Assert.Equal("image/png", content.GetProperty("media_type").GetString());
        Assert.Equal("base64", content.GetProperty("encoding").GetString());
        Assert.Equal("sha256:" + expectedSha256, content.GetProperty("sha256").GetString());
        Assert.Equal(expectedBytes.LongLength, content.GetProperty("byte_length").GetInt64());
        var decoded = Convert.FromBase64String(content.GetProperty("base64").GetString()!);
        Assert.Equal(expectedBytes, decoded);
    }

    private static void AssertPayloadDigestReceiptMatchesCapturedRequest(
        DesignConversionVisualJudgeResult result,
        FakeVisualJudgeHandler handler)
    {
        Assert.NotNull(handler.CapturedBody);
        var expectedPayloadDigest = "sha256:" + Sha256Text(handler.CapturedBody!);
        Assert.Equal(
            expectedPayloadDigest,
            Assert.Single(handler.CapturedHeaderValues(ExternalHttpDesignConversionVisualJudge.RequestPayloadSha256HeaderName)));
        Assert.Equal(
            expectedPayloadDigest,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestPayloadSha256Key]);
        Assert.Equal(
            expectedPayloadDigest,
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestPayloadSha256Key]);
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return;

            var attributes = File.GetAttributes(path);
            Directory.Delete(path, recursive: (attributes & FileAttributes.ReparsePoint) == 0);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    private static string Sha256Bytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string Sha256Text(string value)
        => Sha256Bytes(Encoding.UTF8.GetBytes(value));

    private sealed record VisualJudgePngFixture(
        string Root,
        RenderQaImageRecord Expected,
        RenderQaImageRecord Actual,
        byte[] ExpectedBytes,
        byte[] ActualBytes,
        string ExpectedSha256,
        string ActualSha256,
        IDesignConversionVisualJudgeImageContentResolver Resolver);

    private sealed class StaticVisualJudgeImageContentResolver(
        string expectedSha256,
        byte[] expectedBytes,
        string actualSha256,
        byte[] actualBytes) : IDesignConversionVisualJudgeImageContentResolver
    {
        public Task<OperationResult<DesignConversionVisualJudgeImageContent>> ResolveAsync(
            RenderQaImageRecord image,
            string role,
            long maxBytes,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                    "DesignConversion visual judge image content resolution cancelled.",
                    "CANCELLED"));
            }

            var isRendered = string.Equals(role, "rendered_image", StringComparison.Ordinal);
            var bytes = isRendered ? actualBytes : expectedBytes;
            var sha256 = isRendered ? actualSha256 : expectedSha256;
            if (bytes.LongLength > maxBytes)
            {
                return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Fail(
                    "DesignConversion test image content exceeded max bytes.",
                    "EXTERNAL_VISUAL_JUDGE_IMAGE_BYTES_OUT_OF_BOUNDS"));
            }

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Ok(
                new DesignConversionVisualJudgeImageContent("image/png", sha256, bytes)));
        }
    }

    private static DesignConversionImprovementOutput CreateImprovementOutput(
        string outputId,
        VisualVerdict verdict,
        bool accepted,
        RenderQaResultRecord qaResult)
        => new(
            OutputId: outputId,
            CandidateId: $"{outputId}:candidate",
            Html: accepted ? "<main class=\"card improved\">Hello</main>" : "<main>Hello</main>",
            Css: accepted ? ".card{display:grid;gap:12px;}" : "main{display:block;}",
            Verdict: verdict,
            QaResults: [qaResult],
            EvidenceRefs: [$"{outputId}:evidence", .. verdict.EvidenceRefs],
            Accepted: accepted);

    private static void AssertSerializedDoesNotContain(string serialized, params string[] forbiddenFragments)
    {
        foreach (var fragment in forbiddenFragments)
            Assert.DoesNotContain(fragment, serialized, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<(OperationResult<DesignConversionVisualJudgeResult> Result, FakeVisualJudgeHandler Handler)>
        JudgeValidationResponseAsync(
            string responseJson,
            decimal? serverOwnedCostPerRequestUsd = 0.0123m)
    {
        var handler = new FakeVisualJudgeHandler(responseJson);
        var judge = CreateRealHttpJudge(
            handler,
            apiKey: "validation-api-key-not-for-result",
            serverOwnedCostPerRequestUsd: serverOwnedCostPerRequestUsd,
            endpoint: "https://visual-judge.example:9443/v1/judge",
            metadata: new Dictionary<string, string>
            {
                ["api_key"] = "private-options-api-key-not-for-result",
                ["private_options_metadata"] = "private-options-metadata-not-for-result"
            });

        var result = await judge.JudgeAsync(CreateRequest(metadata: new Dictionary<string, string>
        {
            ["api_key"] = "private-request-api-key-not-for-result",
            ["private_request_metadata"] = "private-request-metadata-not-for-result"
        }));

        return (result, handler);
    }

    private static void AssertResponseValidationBlockedWithReceipt(
        OperationResult<DesignConversionVisualJudgeResult> result,
        FakeVisualJudgeHandler handler,
        string expectedValidationCode,
        params string[] forbiddenFragments)
        => AssertResponseValidationBlockedWithReceipt(
            result,
            handler,
            expectedValidationCode,
            0.0123m,
            "0.0123",
            "0.0123",
            ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt,
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            "configured",
            forbiddenFragments);

    private static void AssertResponseValidationBlockedWithReceipt(
        OperationResult<DesignConversionVisualJudgeResult> result,
        FakeVisualJudgeHandler handler,
        string expectedValidationCode,
        decimal? expectedEstimatedCostUsd,
        string expectedCostPerAttemptUsd,
        string expectedTotalCostUsd,
        string expectedCostAccounting,
        string expectedCostSource,
        string expectedPricingStatus,
        params string[] forbiddenFragments)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        var value = result.Value!;
        Assert.True(value.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusResponseValidationBlocked, value.Status);
        Assert.True(value.Provider.ExternalCallExecuted);
        Assert.False(value.Verdict.Passed);
        Assert.Equal(0.0, value.Verdict.Score);
        Assert.Empty(value.TokenMetadata);
        Assert.Equal(expectedEstimatedCostUsd, value.EstimatedCostUsd);

        var defect = Assert.Single(value.Verdict.Defects);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.ResponseValidationFailureMarker, defect.Category);
        Assert.Equal("blocker", defect.Severity);

        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureEndpointHostPlaceholder,
            value.Provider.EndpointHost);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureEndpointHostPlaceholder,
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey]);
        Assert.Equal(bool.TrueString, value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        Assert.Equal(
            bool.TrueString,
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        AssertPayloadDigestReceiptMatchesCapturedRequest(value, handler);

        Assert.Equal(expectedValidationCode, value.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseValidationCodeKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseValidationCodeKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseValidationCodeKey]);
        Assert.Equal(bool.TrueString, value.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseValidationFailureMarker]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseValidationFailureMarker],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseValidationFailureMarker]);
        Assert.Equal("1", value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal(expectedCostPerAttemptUsd, value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal(expectedTotalCostUsd, value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(expectedCostAccounting, value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Equal(expectedCostSource, value.Metadata["cost_source"]);
        Assert.Equal(value.Metadata["cost_source"], value.Provider.Metadata["cost_source"]);
        Assert.Equal(expectedPricingStatus, value.Metadata["server_owned_pricing_status"]);
        Assert.Equal(
            value.Metadata["server_owned_pricing_status"],
            value.Provider.Metadata["server_owned_pricing_status"]);

        Assert.Equal(bool.TrueString, value.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(bool.TrueString, value.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(bool.TrueString, value.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
        Assert.Equal(bool.TrueString, value.Metadata["network_call_executed"]);
        Assert.Equal(value.Metadata["network_call_executed"], value.Provider.Metadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, value.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(
            value.Metadata["external_ai_visual_judge_executed"],
            value.Provider.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, value.Metadata["visual_ai_executed"]);
        Assert.Equal(value.Metadata["visual_ai_executed"], value.Provider.Metadata["visual_ai_executed"]);
        Assert.Equal(bool.FalseString, value.Metadata["provider_response_identity_verified"]);
        Assert.Equal(
            value.Metadata["provider_response_identity_verified"],
            value.Provider.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.FalseString, value.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            value.Metadata["provider_response_schema_verified"],
            value.Provider.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.FalseString,
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.FalseString,
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            value.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey],
            value.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);

        Assert.Equal(
            "external_response_received",
            value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(
            value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey],
            value.Provider.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(
            "response_validation_failure",
            value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.Equal(
            value.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey],
            value.Provider.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);

        Assert.False(value.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey));
        Assert.False(value.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey));
        Assert.False(value.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.ResponseReadFailureCodeKey));
        Assert.False(value.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.ResponsePolicyCodeKey));
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.StatusResponseValidationBlocked, value.EvidenceRefs);
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.ResponseValidationFailureMarker, value.EvidenceRefs);
        Assert.Contains($"response_validation_code:{expectedValidationCode}", value.EvidenceRefs);
        Assert.Contains("network_call_attempted:true", value.EvidenceRefs);
        Assert.Contains("network_call_execution_verified:true", value.EvidenceRefs);
        Assert.Contains("provider_response_received:true", value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, value.EvidenceRefs);
        Assert.DoesNotContain("external_ai_visual_judge_executed:true", value.EvidenceRefs);
        Assert.DoesNotContain("external_executed", value.Metadata.Values);
        Assert.DoesNotContain("external_executed", value.Provider.Metadata.Values);

        var validationForbiddenFragments = new[]
        {
            "validation-api-key-not-for-result",
            "private-options-api-key-not-for-result",
            "private-options-metadata-not-for-result",
            "private-request-api-key-not-for-result",
            "private-request-metadata-not-for-result",
            "https://visual-judge.example:9443/v1/judge",
            "visual-judge.example",
            "9443",
            "/v1/judge",
            "EXTERNAL_VISUAL_JUDGE_RESPONSE_INVALID",
            "EXTERNAL_VISUAL_JUDGE_RESPONSE_ID_MISMATCH",
            "EXTERNAL_VISUAL_JUDGE_RESPONSE_PROVIDER_MISMATCH",
            "EXTERNAL_VISUAL_JUDGE_RESPONSE_MODEL_MISMATCH",
            "EXTERNAL_VISUAL_JUDGE_RESPONSE_SCHEMA_INVALID"
        }.Concat(forbiddenFragments).ToArray();
        AssertSerializedDoesNotContain(JsonSerializer.Serialize(value), validationForbiddenFragments);
    }

    private static void AssertTransportAttemptedWithoutVisualAiExecution(
        DesignConversionVisualJudgeResult result)
    {
        Assert.True(result.Provider.ExternalCallExecuted);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureEndpointHostPlaceholder,
            result.Provider.EndpointHost);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureEndpointHostPlaceholder,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey]);
        Assert.Equal(
            bool.TrueString,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(bool.FalseString, result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(bool.FalseString, result.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
        Assert.Equal(bool.FalseString, result.Metadata["network_call_executed"]);
        Assert.Equal(result.Metadata["network_call_executed"], result.Provider.Metadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(
            result.Metadata["external_ai_visual_judge_executed"],
            result.Provider.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, result.Metadata["visual_ai_executed"]);
        Assert.Equal(result.Metadata["visual_ai_executed"], result.Provider.Metadata["visual_ai_executed"]);
        Assert.Equal(bool.FalseString, result.Metadata["provider_response_identity_verified"]);
        Assert.Equal(
            result.Metadata["provider_response_identity_verified"],
            result.Provider.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.FalseString, result.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            result.Metadata["provider_response_schema_verified"],
            result.Provider.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.FalseString,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.FalseString,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            "external_attempted",
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey],
            result.Provider.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(
            "transport_failure",
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.Equal(
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey],
            result.Provider.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.Contains("network_call_attempted:true", result.EvidenceRefs);
        Assert.Contains("network_call_execution_verified:false", result.EvidenceRefs);
        Assert.Contains("provider_response_received:false", result.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, result.EvidenceRefs);
        Assert.DoesNotContain("external_ai_visual_judge_executed:true", result.EvidenceRefs);
    }

    private static void AssertResponseReadBlockedWithReceipt(
        DesignConversionVisualJudgeResult result,
        string expectedFailureCode,
        string expectedTotalCostUsd)
    {
        Assert.True(result.Provider.ExternalCallExecuted);
        Assert.False(result.Verdict.Passed);
        Assert.Equal(0.0, result.Verdict.Score);
        Assert.Empty(result.TokenMetadata);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureEndpointHostPlaceholder,
            result.Provider.EndpointHost);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureEndpointHostPlaceholder,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey]);
        Assert.Equal(
            bool.TrueString,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(
            expectedFailureCode,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseReadFailureCodeKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseReadFailureCodeKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseReadFailureCodeKey]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseReadFailureMarker]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseReadFailureMarker],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ResponseReadFailureMarker]);
        Assert.Equal("1", result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal("0.0042", result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal(expectedTotalCostUsd, result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
        Assert.Equal(bool.TrueString, result.Metadata["network_call_executed"]);
        Assert.Equal(result.Metadata["network_call_executed"], result.Provider.Metadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(
            result.Metadata["external_ai_visual_judge_executed"],
            result.Provider.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, result.Metadata["visual_ai_executed"]);
        Assert.Equal(result.Metadata["visual_ai_executed"], result.Provider.Metadata["visual_ai_executed"]);
        Assert.Equal(bool.FalseString, result.Metadata["provider_response_identity_verified"]);
        Assert.Equal(
            result.Metadata["provider_response_identity_verified"],
            result.Provider.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.FalseString, result.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            result.Metadata["provider_response_schema_verified"],
            result.Provider.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.FalseString,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.FalseString,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            "external_response_received",
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey],
            result.Provider.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(
            "response_read_failure",
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.Equal(
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey],
            result.Provider.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.False(result.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey));
        Assert.False(result.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey));
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.StatusResponseReadBlocked, result.EvidenceRefs);
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.ResponseReadFailureMarker, result.EvidenceRefs);
        Assert.Contains($"response_read_failure_code:{expectedFailureCode}", result.EvidenceRefs);
        Assert.Contains("network_call_attempted:true", result.EvidenceRefs);
        Assert.Contains("network_call_execution_verified:true", result.EvidenceRefs);
        Assert.Contains("provider_response_received:true", result.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, result.EvidenceRefs);
        Assert.DoesNotContain("external_ai_visual_judge_executed:true", result.EvidenceRefs);
        Assert.DoesNotContain("external_executed", result.Metadata.Values);
        Assert.DoesNotContain("external_executed", result.Provider.Metadata.Values);
    }

    private static void AssertResponsePolicyBlockedWithReceipt(
        DesignConversionVisualJudgeResult result,
        decimal? expectedEstimatedCostUsd,
        string expectedCostPerAttemptUsd,
        string expectedTotalCostUsd,
        string expectedCostAccounting,
        string expectedCostSource,
        string expectedPricingStatus)
    {
        Assert.True(result.IsBlocked);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusResponsePolicyBlocked, result.Status);
        Assert.True(result.Provider.ExternalCallExecuted);
        Assert.False(result.Verdict.Passed);
        Assert.Equal(0.0, result.Verdict.Score);
        Assert.Empty(result.TokenMetadata);
        Assert.Equal(expectedEstimatedCostUsd, result.EstimatedCostUsd);
        var defect = Assert.Single(result.Verdict.Defects);
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.ResponsePolicyBlockMarker, defect.Category);
        Assert.Equal("blocker", defect.Severity);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureEndpointHostPlaceholder,
            result.Provider.EndpointHost);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.TransportFailureEndpointHostPlaceholder,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        Assert.Equal(
            bool.TrueString,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(
            bool.TrueString,
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ResponsePolicyResponseTooLargeCode,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ResponsePolicyCodeKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ResponsePolicyCodeKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ResponsePolicyCodeKey]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.ResponsePolicyBlockMarker]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ResponsePolicyBlockMarker],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ResponsePolicyBlockMarker]);
        Assert.Equal("1", result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal(expectedCostPerAttemptUsd, result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal(expectedTotalCostUsd, result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(expectedCostAccounting, result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Equal(expectedCostSource, result.Metadata["cost_source"]);
        Assert.Equal(result.Metadata["cost_source"], result.Provider.Metadata["cost_source"]);
        Assert.Equal(expectedPricingStatus, result.Metadata["server_owned_pricing_status"]);
        Assert.Equal(
            result.Metadata["server_owned_pricing_status"],
            result.Provider.Metadata["server_owned_pricing_status"]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(bool.TrueString, result.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
        Assert.Equal(bool.TrueString, result.Metadata["network_call_executed"]);
        Assert.Equal(result.Metadata["network_call_executed"], result.Provider.Metadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(
            result.Metadata["external_ai_visual_judge_executed"],
            result.Provider.Metadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, result.Metadata["visual_ai_executed"]);
        Assert.Equal(result.Metadata["visual_ai_executed"], result.Provider.Metadata["visual_ai_executed"]);
        Assert.Equal(bool.FalseString, result.Metadata["provider_response_identity_verified"]);
        Assert.Equal(
            result.Metadata["provider_response_identity_verified"],
            result.Provider.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.FalseString, result.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            result.Metadata["provider_response_schema_verified"],
            result.Provider.Metadata["provider_response_schema_verified"]);
        Assert.Equal(
            bool.FalseString,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(
            bool.FalseString,
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            result.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey],
            result.Provider.Metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(
            "external_response_received",
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey],
            result.Provider.Metadata[DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey]);
        Assert.Equal(
            "response_policy_blocked",
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.Equal(
            result.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey],
            result.Provider.Metadata[DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey]);
        Assert.False(result.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.TerminalHttpStatusCodeKey));
        Assert.False(result.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.TransportFailureCodeKey));
        Assert.False(result.Metadata.ContainsKey(ExternalHttpDesignConversionVisualJudge.ResponseReadFailureCodeKey));
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.StatusResponsePolicyBlocked, result.EvidenceRefs);
        Assert.Contains(ExternalHttpDesignConversionVisualJudge.ResponsePolicyBlockMarker, result.EvidenceRefs);
        Assert.Contains(
            $"response_policy_code:{ExternalHttpDesignConversionVisualJudge.ResponsePolicyResponseTooLargeCode}",
            result.EvidenceRefs);
        Assert.Contains("network_call_attempted:true", result.EvidenceRefs);
        Assert.Contains("network_call_execution_verified:true", result.EvidenceRefs);
        Assert.Contains("provider_response_received:true", result.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker, result.EvidenceRefs);
        Assert.DoesNotContain("external_ai_visual_judge_executed:true", result.EvidenceRefs);
        Assert.DoesNotContain(ExternalHttpDesignConversionVisualJudge.StatusResponseReadBlocked, result.EvidenceRefs);
        Assert.DoesNotContain(ExternalHttpDesignConversionVisualJudge.ResponseReadFailureMarker, result.EvidenceRefs);
        Assert.DoesNotContain("external_executed", result.Metadata.Values);
        Assert.DoesNotContain("external_executed", result.Provider.Metadata.Values);
    }

    private sealed class RecordingRetryDelay(Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync =
            delayAsync ?? ((_, _) => Task.CompletedTask);
        private readonly List<TimeSpan> _delays = [];

        public IReadOnlyList<TimeSpan> Delays => _delays;

        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            _delays.Add(delay);
            return _delayAsync(delay, cancellationToken);
        }
    }

    private sealed class SequenceVisualJudgeHandler(params Func<HttpResponseMessage>[] responses) : HttpMessageHandler
    {
        private readonly List<IReadOnlyList<string>> _idempotencyKeyValues = [];

        public int CallCount { get; private set; }
        public IReadOnlyList<IReadOnlyList<string>> IdempotencyKeyValues => _idempotencyKeyValues;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            _idempotencyKeyValues.Add(
                request.Headers.TryGetValues("Idempotency-Key", out var values)
                    ? values.ToArray()
                    : []);

            if (request.Content is not null)
                await request.Content.ReadAsStringAsync(cancellationToken);

            var responseIndex = CallCount - 1;
            if (responseIndex >= responses.Length)
                throw new InvalidOperationException("Unexpected visual judge HTTP call.");

            return responses[responseIndex]();
        }
    }

    private sealed class FakeVisualJudgeHandler(
        string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public int CallCount { get; private set; }
        public string? CapturedBody { get; private set; }
        public string? CapturedRequestUri { get; private set; }
        private Dictionary<string, IReadOnlyList<string>> CapturedHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<string> CapturedHeaderValues(string name)
            => CapturedHeaders.TryGetValue(name, out var values) ? values : [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            CapturedRequestUri = request.RequestUri?.ToString();
            CapturedHeaders.Clear();
            foreach (var header in request.Headers)
                CapturedHeaders[header.Key] = header.Value.ToArray();
            if (request.Content is not null)
            {
                foreach (var header in request.Content.Headers)
                    CapturedHeaders[header.Key] = header.Value.ToArray();
            }

            CapturedBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class ResponseContentVisualJudgeHandler(HttpContent responseContent) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            if (request.Content is not null)
                await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = responseContent
            };
        }
    }

    private sealed class StreamBackedHttpContent(Func<Stream> streamFactory) : HttpContent
    {
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            await using var source = streamFactory();
            await source.CopyToAsync(stream);
        }

        protected override async Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context,
            CancellationToken cancellationToken)
        {
            await using var source = streamFactory();
            await source.CopyToAsync(stream, cancellationToken);
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
            => Task.FromResult(streamFactory());

        protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
            => Task.FromResult(streamFactory());

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }

    private sealed class DelayingReadStream(TimeSpan delay) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            return 0;
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(delay, cancellationToken);
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private sealed class ThrowingReadStream(Exception exception) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
            => throw exception;

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
            => Task.FromException<int>(exception);

        public override ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
            => ValueTask.FromException<int>(exception);

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();
    }

    private sealed record ExceptionReadCase(
        Exception Exception,
        string PrivateMessageFragment,
        string PrivateTokenFragment);

    private sealed record ResponsePolicyPricingCase(
        decimal? ServerOwnedCostPerRequestUsd,
        string ExpectedPricingStatus);

    private sealed class DelayingVisualJudgeHandler(TimeSpan delay, Action? onSend = null) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            onSend?.Invoke();
            await Task.Delay(delay, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                      "score": 0.91,
                      "passed": true,
                      "issues": []
                    }
                    """, Encoding.UTF8, "application/json")
            };
        }
    }
}
