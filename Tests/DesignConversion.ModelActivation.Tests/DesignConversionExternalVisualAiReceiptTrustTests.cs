// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionExternalVisualAiReceiptTrustTests
{
    private const string ProductionTrustedKey = "render_evidence_production_trusted";
    private const string ApprovalEvidenceAllowedKey = "approval_evidence_allowed";

    [Fact]
    public async Task ExternalVisualAiComparison_RealLoopbackHttpReceipt_CanonicalizesTrustedThroughInternalService()
    {
        var root = DesignConversionExternalVisualAiReceiptTestHarness.NewRoot(nameof(ExternalVisualAiComparison_RealLoopbackHttpReceipt_CanonicalizesTrustedThroughInternalService));
        await using var server = await LoopbackVisualJudgeServer.StartAsync(score: 0.93);
        var service = DesignConversionExternalVisualAiReceiptTestHarness.CreateBrowserService(
            root,
            DesignConversionExternalVisualAiReceiptTestHarness.CreateExternalHttpComparisonProvider(server, root));

        var result = await service.EvaluateAsync(
            DesignConversionExternalVisualAiReceiptTestHarness.CreateInternalRequest(
                "external-loopback-trusted",
                root));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, server.RequestCount);
        var observedRequest = Assert.Single(server.RequestBodies);
        Assert.Contains("\"judge_request_id\"", observedRequest, StringComparison.Ordinal);
        var comparison = Assert.Single(result.Value!.ComparisonArtifacts);
        Assert.Equal(0.93, comparison.Verdict.Score, precision: 6);
        Assert.True(comparison.Verdict.Passed);
        Assert.Equal(bool.TrueString, comparison.Metadata[DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey]);
        Assert.Equal(bool.TrueString, comparison.Metadata[ProductionTrustedKey]);
        Assert.Equal(bool.TrueString, comparison.Metadata[ApprovalEvidenceAllowedKey]);
        Assert.Equal(bool.TrueString, comparison.Metadata["visual_comparison_proven"]);
        Assert.Equal(
            DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification,
            comparison.Metadata["comparison_evidence_trust_classification"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["server_owned_external_visual_ai_receipt_validated"]);
        Assert.Equal("valid", comparison.Metadata["server_owned_external_visual_ai_receipt_validation_reason"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["product_ready"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["dpo_eligible"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["model_weights_published"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["model_training_verified"]);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);
    }

    public static IEnumerable<object[]> InvalidExternalReceiptMutationCases()
    {
        yield return InvalidReceiptCase("missing_receipt_marker", ReceiptMutationKind.MissingReceiptMarker, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("partial_receipt_version", ReceiptMutationKind.PartialReceiptVersion, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("missing_receipt_id", ReceiptMutationKind.MissingReceiptId, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_job_id_mismatch", ReceiptMutationKind.ReceiptJobIdMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_request_id_mismatch", ReceiptMutationKind.ReceiptRequestIdMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_root_request_id_mismatch", ReceiptMutationKind.ReceiptRootRequestIdMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_provider_request_id_mismatch", ReceiptMutationKind.ReceiptProviderRequestIdMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_comparison_request_id_mismatch", ReceiptMutationKind.ReceiptComparisonRequestIdMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_visual_result_id_mismatch", ReceiptMutationKind.ReceiptVisualResultIdMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("artifact_comparison_artifact_id_mismatch", ReceiptMutationKind.ArtifactComparisonArtifactIdMismatch, "comparison_artifact_id_mismatch");
        yield return InvalidReceiptCase("receipt_comparison_artifact_id_mismatch", ReceiptMutationKind.ReceiptComparisonArtifactIdMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_provider_name_mismatch", ReceiptMutationKind.ReceiptProviderNameMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_provider_model_mismatch", ReceiptMutationKind.ReceiptProviderModelMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_endpoint_host_mismatch", ReceiptMutationKind.ReceiptEndpointHostMismatch, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("fake_client_marker", ReceiptMutationKind.FakeClientMarker, "external_execution_markers_invalid");
        yield return InvalidReceiptCase("test_only_marker", ReceiptMutationKind.TestOnlyMarker, "external_execution_markers_invalid");
        yield return InvalidReceiptCase("network_call_false", ReceiptMutationKind.NetworkCallFalse, "external_execution_markers_invalid");
        yield return InvalidReceiptCase("external_call_false", ReceiptMutationKind.ExternalCallFalse, "external_execution_markers_invalid");
        yield return InvalidReceiptCase("response_identity_false", ReceiptMutationKind.ResponseIdentityFalse, "response_verification_markers_invalid");
        yield return InvalidReceiptCase("response_schema_false", ReceiptMutationKind.ResponseSchemaFalse, "response_verification_markers_invalid");
        yield return InvalidReceiptCase("receipt_external_call_false", ReceiptMutationKind.ReceiptExternalCallFalse, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_response_identity_false", ReceiptMutationKind.ReceiptResponseIdentityFalse, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("receipt_response_schema_false", ReceiptMutationKind.ReceiptResponseSchemaFalse, "server_call_receipt_invalid");
        yield return InvalidReceiptCase("terminal_status", ReceiptMutationKind.TerminalStatus, "external_visual_status_not_success");
        yield return InvalidReceiptCase("blocked_status", ReceiptMutationKind.BlockedStatus, "external_visual_result_blocked");
        yield return InvalidReceiptCase("expected_image_ref_mismatch", ReceiptMutationKind.ExpectedImageRefMismatch, "comparison_image_refs_mismatch");
        yield return InvalidReceiptCase("actual_image_ref_mismatch", ReceiptMutationKind.ActualImageRefMismatch, "comparison_image_refs_mismatch");
    }

    [Theory]
    [MemberData(nameof(InvalidExternalReceiptMutationCases))]
    public async Task ExternalVisualAiComparison_InvalidReceiptMutation_IsUntrustedZeroedAndEvidenceBound(
        string caseName,
        ReceiptMutationKind mutation,
        string expectedReason)
    {
        Assert.False(string.IsNullOrWhiteSpace(caseName));
        await using var server = await LoopbackVisualJudgeServer.StartAsync(score: 0.94);
        const string neutralBaselineName = "invalid-receipt-mutation-baseline";
        var root = DesignConversionExternalVisualAiReceiptTestHarness.NewRoot(neutralBaselineName);
        var provider = DesignConversionExternalVisualAiReceiptTestHarness.CreateExternalHttpComparisonProvider(server, root);
        var request = DesignConversionExternalVisualAiReceiptTestHarness.CreateDirectComparisonRequest(
            neutralBaselineName,
            root);
        var artifact = await CreateTrustedExternalVisualAiComparisonArtifactAsync(provider, request);
        var mutated = MutateReceiptArtifact(request, artifact, mutation);

        var canonicalized = DesignConversionComparisonEvidenceTrustBoundary.Canonicalize(
            provider,
            request,
            mutated);

        Assert.True(canonicalized.IsSuccess, canonicalized.ErrorMessage);
        var canonicalizedArtifact = canonicalized.Value!;
        AssertInvalidExternalReceiptCanonicalization(canonicalizedArtifact, expectedReason);
        if (mutation is ReceiptMutationKind.ExpectedImageRefMismatch or ReceiptMutationKind.ActualImageRefMismatch)
        {
            Assert.Equal(bool.TrueString, canonicalizedArtifact.Metadata["comparison_image_refs_rebound_by_server"]);
            Assert.Contains("UNTRUSTED_COMPARISON_PROVIDER_IMAGE_REFS_REBOUND_BY_SERVER", canonicalizedArtifact.EvidenceRefs);
            Assert.Equal(request.Expected, canonicalizedArtifact.Expected);
            Assert.Equal(request.Actual, canonicalizedArtifact.Actual);
        }
    }

    [Fact]
    public async Task ExternalVisualAiComparison_InjectedForgedJudgeReceipt_RemainsUntrustedAndZeroedThroughInternalService()
    {
        var root = DesignConversionExternalVisualAiReceiptTestHarness.NewRoot(nameof(ExternalVisualAiComparison_InjectedForgedJudgeReceipt_RemainsUntrustedAndZeroedThroughInternalService));
        var forgedProvider = new DesignConversionExternalVisualAiComparisonProvider(new ForgedReceiptVisualJudge());
        var downstreamJudge = new RecordingDownstreamVisualJudge();
        var service = DesignConversionExternalVisualAiReceiptTestHarness.CreateBrowserService(
            root,
            forgedProvider,
            downstreamJudge);

        var result = await service.EvaluateAsync(
            DesignConversionExternalVisualAiReceiptTestHarness.CreateInternalRequest(
                "external-forged-judge-zeroed",
                root));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var comparison = Assert.Single(result.Value!.ComparisonArtifacts);
        Assert.Equal(0.0, comparison.Verdict.Score);
        Assert.False(comparison.Verdict.Passed);
        Assert.Equal(bool.FalseString, comparison.Metadata[ProductionTrustedKey]);
        Assert.Equal(bool.FalseString, comparison.Metadata[ApprovalEvidenceAllowedKey]);
        Assert.Equal(bool.TrueString, comparison.Metadata[DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey]);
        Assert.Equal(bool.FalseString, comparison.Metadata["visual_comparison_proven"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["server_owned_external_visual_ai_receipt_validated"]);
        Assert.Equal(
            "provider_does_not_own_external_http_visual_judge",
            comparison.Metadata["server_owned_external_visual_ai_receipt_validation_reason"]);
        Assert.Equal(1.0, comparison.Metrics["comparison_score_zeroed_by_trust_boundary"]);
        Assert.Equal(1.0, comparison.Metrics["comparison_pass_zeroed_by_trust_boundary"]);

        var downstreamRequest = Assert.Single(downstreamJudge.Requests);
        Assert.Equal("0", downstreamRequest.Metadata["comparison_verdict_score"]);
        Assert.Equal(bool.FalseString, downstreamRequest.Metadata["comparison_verdict_passed"]);
        var downstreamResult = Assert.Single(result.Value.VisualJudgeResults);
        Assert.Equal(0.0, downstreamResult.Verdict.Score);
        Assert.False(downstreamResult.Verdict.Passed);
        var qa = Assert.Single(result.Value.QaResults);
        Assert.Equal(0.0, qa.Verdict.Score);
        Assert.False(qa.Verdict.Passed);
    }

    private static object[] InvalidReceiptCase(
        string caseName,
        ReceiptMutationKind mutation,
        string expectedReason)
        => [caseName, mutation, expectedReason];

    private static async Task<DesignConversionQaComparisonArtifact> CreateTrustedExternalVisualAiComparisonArtifactAsync(
        DesignConversionExternalVisualAiComparisonProvider provider,
        DesignConversionQaComparisonRequest request)
    {
        var comparison = await provider.CompareAsync(request);
        Assert.True(comparison.IsSuccess, comparison.ErrorMessage);

        var canonicalized = DesignConversionComparisonEvidenceTrustBoundary.Canonicalize(
            provider,
            request,
            comparison.Value!);
        Assert.True(canonicalized.IsSuccess, canonicalized.ErrorMessage);
        Assert.Equal(
            DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification,
            canonicalized.Value!.Metadata[DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey]);
        Assert.Equal(bool.TrueString, canonicalized.Value.Metadata[ProductionTrustedKey]);
        Assert.Equal(bool.TrueString, canonicalized.Value.Metadata[ApprovalEvidenceAllowedKey]);
        Assert.Equal(bool.TrueString, canonicalized.Value.Metadata["visual_comparison_proven"]);

        return comparison.Value!;
    }

    private static DesignConversionQaComparisonArtifact MutateReceiptArtifact(
        DesignConversionQaComparisonRequest request,
        DesignConversionQaComparisonArtifact artifact,
        ReceiptMutationKind mutation)
    {
        var metadata = new Dictionary<string, string>(artifact.Metadata, StringComparer.Ordinal);
        var updated = artifact;
        switch (mutation)
        {
            case ReceiptMutationKind.MissingReceiptMarker:
                metadata.Remove(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey);
                break;
            case ReceiptMutationKind.PartialReceiptVersion:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey] = "design_conversion_visual_provider_call_receipt_v0";
                break;
            case ReceiptMutationKind.MissingReceiptId:
                metadata.Remove(ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey);
                break;
            case ReceiptMutationKind.ReceiptJobIdMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey] = $"{request.ComparisonRequestId}:forged-job";
                break;
            case ReceiptMutationKind.ReceiptRequestIdMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey] = $"{request.RequestId}:forged-request";
                break;
            case ReceiptMutationKind.ReceiptRootRequestIdMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey] = $"{request.RequestId}:forged-root";
                break;
            case ReceiptMutationKind.ReceiptProviderRequestIdMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] = $"{request.ComparisonRequestId}:forged-provider-request";
                break;
            case ReceiptMutationKind.ReceiptComparisonRequestIdMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey] = $"{request.ComparisonRequestId}:forged-comparison";
                break;
            case ReceiptMutationKind.ReceiptVisualResultIdMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey] = $"{request.ComparisonRequestId}:forged-visual-result";
                break;
            case ReceiptMutationKind.ArtifactComparisonArtifactIdMismatch:
                updated = updated with { ArtifactId = $"{artifact.ArtifactId}:forged" };
                break;
            case ReceiptMutationKind.ReceiptComparisonArtifactIdMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey] = $"{artifact.ArtifactId}:forged-receipt";
                break;
            case ReceiptMutationKind.ReceiptProviderNameMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey] = "anthropic";
                break;
            case ReceiptMutationKind.ReceiptProviderModelMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey] = "gpt-forged-model";
                break;
            case ReceiptMutationKind.ReceiptEndpointHostMismatch:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey] = "forged.visual-ai-receipt.test";
                break;
            case ReceiptMutationKind.FakeClientMarker:
                metadata["fake_client"] = bool.TrueString;
                break;
            case ReceiptMutationKind.TestOnlyMarker:
                metadata[DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.TrueString;
                break;
            case ReceiptMutationKind.NetworkCallFalse:
                metadata["network_call_executed"] = bool.FalseString;
                break;
            case ReceiptMutationKind.ExternalCallFalse:
                metadata["real_external_provider_executed"] = bool.FalseString;
                break;
            case ReceiptMutationKind.ResponseIdentityFalse:
                metadata["provider_response_identity_verified"] = bool.FalseString;
                break;
            case ReceiptMutationKind.ResponseSchemaFalse:
                metadata["provider_response_schema_verified"] = bool.FalseString;
                break;
            case ReceiptMutationKind.ReceiptExternalCallFalse:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey] = bool.FalseString;
                break;
            case ReceiptMutationKind.ReceiptResponseIdentityFalse:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey] = bool.FalseString;
                break;
            case ReceiptMutationKind.ReceiptResponseSchemaFalse:
                metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey] = bool.FalseString;
                break;
            case ReceiptMutationKind.TerminalStatus:
                metadata["status"] = ExternalHttpDesignConversionVisualJudge.StatusHttpBlocked;
                break;
            case ReceiptMutationKind.BlockedStatus:
                metadata["external_visual_ai_blocked"] = bool.TrueString;
                metadata["blocked_reason"] = DesignConversionExternalVisualAiComparisonProvider.TerminalHttpNonSuccessReason;
                break;
            case ReceiptMutationKind.ExpectedImageRefMismatch:
                updated = updated with
                {
                    Expected = updated.Expected with
                    {
                        ImageId = $"{updated.Expected.ImageId}:forged"
                    }
                };
                break;
            case ReceiptMutationKind.ActualImageRefMismatch:
                updated = updated with
                {
                    Actual = updated.Actual with
                    {
                        Sha256 = DesignConversionExternalVisualAiReceiptTestHarness.ShaRef($"{updated.Actual.ImageId}:forged")
                    }
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation), mutation, "Unknown receipt mutation.");
        }

        return updated with { Metadata = metadata };
    }

    private static void AssertInvalidExternalReceiptCanonicalization(
        DesignConversionQaComparisonArtifact comparison,
        string expectedReason)
    {
        Assert.Equal(0.0, comparison.Verdict.Score);
        Assert.False(comparison.Verdict.Passed);
        Assert.Equal(
            DesignConversionComparisonEvidenceTrustBoundary.UntrustedInjectedClassification,
            comparison.Metadata[DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey]);
        Assert.Equal(bool.FalseString, comparison.Metadata[ProductionTrustedKey]);
        Assert.Equal(bool.FalseString, comparison.Metadata[ApprovalEvidenceAllowedKey]);
        Assert.Equal(bool.FalseString, comparison.Metadata["visual_comparison_proven"]);
        Assert.Equal(bool.FalseString, comparison.Metadata["server_owned_external_visual_ai_receipt_validated"]);
        Assert.Equal(expectedReason, comparison.Metadata["server_owned_external_visual_ai_receipt_validation_reason"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["comparison_score_zeroed_by_trust_boundary"]);
        Assert.Equal(bool.TrueString, comparison.Metadata["comparison_pass_zeroed_by_trust_boundary"]);
        Assert.Equal(1.0, comparison.Metrics["comparison_score_zeroed_by_trust_boundary"]);
        Assert.Equal(1.0, comparison.Metrics["comparison_pass_zeroed_by_trust_boundary"]);
        Assert.Contains(DesignConversionExternalVisualAiReceiptValidator.InvalidEvidenceRef, comparison.EvidenceRefs);
        Assert.Contains($"server_owned_external_visual_ai_receipt_invalid:{expectedReason}", comparison.EvidenceRefs);
        Assert.Contains("EXTERNAL_VISUAL_AI_COMPARISON_SCORE_ZEROED_BEFORE_DOWNSTREAM_JUDGE", comparison.Verdict.EvidenceRefs);
    }

    public enum ReceiptMutationKind
    {
        MissingReceiptMarker,
        PartialReceiptVersion,
        MissingReceiptId,
        ReceiptJobIdMismatch,
        ReceiptRequestIdMismatch,
        ReceiptRootRequestIdMismatch,
        ReceiptProviderRequestIdMismatch,
        ReceiptComparisonRequestIdMismatch,
        ReceiptVisualResultIdMismatch,
        ArtifactComparisonArtifactIdMismatch,
        ReceiptComparisonArtifactIdMismatch,
        ReceiptProviderNameMismatch,
        ReceiptProviderModelMismatch,
        ReceiptEndpointHostMismatch,
        FakeClientMarker,
        TestOnlyMarker,
        NetworkCallFalse,
        ExternalCallFalse,
        ResponseIdentityFalse,
        ResponseSchemaFalse,
        ReceiptExternalCallFalse,
        ReceiptResponseIdentityFalse,
        ReceiptResponseSchemaFalse,
        TerminalStatus,
        BlockedStatus,
        ExpectedImageRefMismatch,
        ActualImageRefMismatch
    }
}

internal static class DesignConversionExternalVisualAiReceiptTestHarness
{
    public static string NewRoot(string name)
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "xiigen-external-visual-ai-receipt",
            SafeId(name),
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    public static DesignConversionExternalVisualAiComparisonProvider CreateExternalHttpComparisonProvider(
        LoopbackVisualJudgeServer server,
        string artifactRoot)
    {
        var options = new DesignConversionExternalVisualJudgeOptions(
            Endpoint: server.Endpoint,
            ProviderName: "openai",
            ModelId: "gpt-visual-loopback",
            ApiKey: "test-loopback-key",
            Metadata: new Dictionary<string, string>
            {
                ["provider_payload_included"] = bool.FalseString,
                ["secret_value_included"] = bool.FalseString
            },
            ServerOwnedCostPerRequestUsd: 0.01m);
        var judge = new ExternalHttpDesignConversionVisualJudge(
            options,
            server.CreateHttpClient(),
            new ServerOwnedLocalPngComparisonProvider(
                artifactRoot,
                maxArtifactBytes: ExternalHttpDesignConversionVisualJudge.MaxInlinePngImageBytes));
        return new DesignConversionExternalVisualAiComparisonProvider(judge);
    }

    public static DesignConversionInternalService CreateBrowserService(
        string artifactRoot,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge? downstreamJudge = null)
        => new(
            new BrowserBackedDesignConversionRenderProvider(new ReceiptBrowserAdapter(artifactRoot)),
            comparisonProvider,
            downstreamJudge ?? new DefaultLocalDesignConversionVisualJudge(),
            routingFlow: null);

    public static DesignConversionQaComparisonRequest CreateDirectComparisonRequest(
        string name,
        string artifactRoot)
    {
        var safeName = SafeId(name);
        var jobId = $"external-ai-receipt-job-{safeName}-{Guid.NewGuid():N}";
        var requestId = $"external-ai-receipt-request-{safeName}-{Guid.NewGuid():N}";
        var resolution = new RenderQaResolution($"source-{safeName}-desktop", 640, 360, 1.0);
        var expected = CreateDirectImageRecord(
            artifactRoot,
            $"expected-{safeName}",
            "expected-design-render",
            resolution,
            $"<main class=\"expected\">expected:{safeName}</main>",
            ".expected{display:grid;min-height:100vh;background:#f8fafc;color:#111827}");
        var actual = CreateDirectImageRecord(
            artifactRoot,
            $"actual-{safeName}",
            "rendered-candidate",
            resolution,
            $"<main class=\"actual\">actual:{safeName}</main>",
            ".actual{display:grid;min-height:100vh;background:#e2e8f0;color:#0f172a}");

        return new DesignConversionQaComparisonRequest(
            ComparisonRequestId: $"{jobId}:comparison-request:1",
            RequestId: requestId,
            Expected: expected,
            Actual: actual,
            Metadata: new Dictionary<string, string>
            {
                ["job_id"] = jobId,
                ["render_result_id"] = $"{jobId}:render-result:1",
                ["comparison_index"] = "1",
                ["comparison"] = "external_visual_ai_provider",
                ["qa_data_role"] = "input_evidence_only"
            });
    }

    public static DesignConversionInternalEvaluateRequest CreateInternalRequest(
        string name,
        string artifactRoot)
    {
        var safeName = SafeId(name);
        var resolutions = new[]
        {
            new RenderQaResolution($"source-{safeName}-desktop", 640, 360, 1.0)
        };
        var screenshots = resolutions
            .Select((resolution, index) => CreateScreenshotArtifact(artifactRoot, safeName, resolution, index + 1))
            .ToArray();

        return new DesignConversionInternalEvaluateRequest(
            JobId: $"external-ai-receipt-job-{safeName}-{Guid.NewGuid():N}",
            RequestId: $"external-ai-receipt-request-{safeName}-{Guid.NewGuid():N}",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: $"<main class=\"receipt-test\">{safeName}</main>",
            Css: ".receipt-test{display:grid;min-height:100vh;background:#f8fafc;color:#111827}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: $"tree-{safeName}",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\",\"name\":\"receipt-test\"}",
                Sha256: ShaRef($"tree-{safeName}"),
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: $"original-tree-{safeName}",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\",\"name\":\"receipt-original\"}",
                Sha256: ShaRef($"original-tree-{safeName}"),
                Metadata: new Dictionary<string, string>()),
            Screenshots: screenshots,
            DesignRenders: [],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "external-ai-receipt-test-user",
                Tier: "bounded-test",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>()),
            RenderResolutions: resolutions,
            AiTeacherEvidenceRefs: [],
            DeterministicEvidenceRefs: new Dictionary<string, string>(),
            Provenance: new Dictionary<string, string>());
    }

    public static IReadOnlyDictionary<string, string> BuildPlausibleExternalReceiptMetadata(
        string providerRequestId,
        string requestId,
        string comparisonRequestId)
    {
        var jobId = ResolveJobId(comparisonRequestId);
        var visualResultId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
            providerRequestId,
            comparisonRequestId);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["provider_kind"] = DesignConversionExternalVisualAiComparisonProvider.ProviderKind,
            ["provider_name"] = "openai",
            ["provider_model"] = "gpt-visual-loopback",
            ["provider_endpoint_host"] = LoopbackVisualJudgeServer.EndpointHost,
            ["external_visual_ai_configured"] = bool.TrueString,
            ["external_visual_ai_executed"] = bool.TrueString,
            ["visual_ai_provider_contract_executed"] = bool.TrueString,
            ["real_external_provider_executed"] = bool.TrueString,
            ["network_call_executed"] = bool.TrueString,
            ["fake_client"] = bool.FalseString,
            [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = bool.FalseString,
            ["external_visual_ai_blocked"] = bool.FalseString,
            ["blocked_reason"] = "none",
            ["provider_response_identity_verified"] = bool.TrueString,
            ["provider_response_schema_verified"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString,
            ["status"] = ExternalHttpDesignConversionVisualJudge.StatusOk,
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderKey] = "openai",
            [DesignConversionVisualAiProviderSelectionPolicy.SelectedProviderStatusKey] = "executed",
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderModeKey] = "external_executed",
            [DesignConversionVisualAiProviderSelectionPolicy.ProviderFallbackStatusKey] = "not_needed",
            [DesignConversionVisualAiProviderSelectionPolicy.CandidateProvidersKey] = "claude,openai,gemini",
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProviderCountKey] = "2",
            [DesignConversionVisualAiProviderSelectionPolicy.UnavailableProvidersKey] = "claude,gemini",
            [DesignConversionVisualAiProviderSelectionPolicy.RealCloudProviderClaimedKey] = bool.FalseString,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey] = bool.TrueString,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey] = ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey] = $"{providerRequestId}:server-visual-provider-call-receipt",
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey] = jobId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey] = providerRequestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey] = requestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey] = requestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey] = providerRequestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey] = comparisonRequestId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey] = visualResultId,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey] = "openai",
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey] = "gpt-visual-loopback",
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey] = LoopbackVisualJudgeServer.EndpointHost,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey] = bool.TrueString,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey] = bool.TrueString,
            [ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey] = bool.TrueString
        };
    }

    internal static string SafeId(string value)
        => new(value
            .Select(character => char.IsAsciiLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-')
            .ToArray());

    internal static string ShaRef(string value)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static RenderQaImageRecord CreateImageRecord(
        string imageId,
        string label,
        RenderQaResolution resolution,
        string uri,
        string sha256)
        => new(
            ImageId: imageId,
            Label: label,
            Resolution: resolution,
            Uri: uri,
            Sha256: sha256,
            Metadata: new Dictionary<string, string>
            {
                ["media_type"] = "image/png",
                ["content_type"] = "image/png",
                ["png_width"] = resolution.Width.ToString(CultureInfo.InvariantCulture),
                ["png_height"] = resolution.Height.ToString(CultureInfo.InvariantCulture),
                ["viewport_width"] = resolution.Width.ToString(CultureInfo.InvariantCulture),
                ["viewport_height"] = resolution.Height.ToString(CultureInfo.InvariantCulture),
                ["viewport_id"] = resolution.ResolutionId
            });

    private static RenderQaImageRecord CreateDirectImageRecord(
        string artifactRoot,
        string imageId,
        string label,
        RenderQaResolution resolution,
        string html,
        string css)
    {
        var render = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot)
            .RenderAsync(new DesignConversionRenderRequest(
                RenderRequestId: $"{imageId}:direct-receipt-fixture",
                RequestId: $"{imageId}:direct-receipt-request",
                Html: html,
                Css: css,
                Resolution: resolution,
                Metadata: new Dictionary<string, string>()))
            .GetAwaiter()
            .GetResult();
        if (!render.IsSuccess)
            throw new InvalidOperationException(render.ErrorMessage);

        return CreateImageRecord(
            imageId,
            label,
            resolution,
            render.Value!.Image.Sha256,
            render.Value!.Image.Sha256);
    }

    private static string ResolveJobId(string comparisonRequestId)
    {
        var marker = ":comparison-request:";
        var index = comparisonRequestId.IndexOf(marker, StringComparison.Ordinal);
        return index <= 0
            ? comparisonRequestId
            : comparisonRequestId[..index];
    }

    private static DesignRenderArtifact CreateScreenshotArtifact(
        string artifactRoot,
        string safeName,
        RenderQaResolution resolution,
        int index)
    {
        var render = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot)
            .RenderAsync(new DesignConversionRenderRequest(
                RenderRequestId: $"target-{safeName}-{index}",
                RequestId: $"target-{safeName}",
                Html: $"<main>{safeName}:{resolution.ResolutionId}</main>",
                Css: "main{display:grid;min-height:100vh;background:#f8fafc;color:#111827}",
                Resolution: resolution,
                Metadata: new Dictionary<string, string>()))
            .GetAwaiter()
            .GetResult();
        if (!render.IsSuccess)
            throw new InvalidOperationException(render.ErrorMessage);

        var image = render.Value!.Image;
        return new DesignRenderArtifact(
            ArtifactId: $"target-{safeName}-{index}",
            Kind: "screenshot",
            Width: resolution.Width,
            Height: resolution.Height,
            Uri: image.Sha256,
            Sha256: image.Sha256,
            Metadata: new Dictionary<string, string>
            {
                ["media_type"] = "image/png",
                ["content_type"] = "image/png",
                ["png_width"] = resolution.Width.ToString(CultureInfo.InvariantCulture),
                ["png_height"] = resolution.Height.ToString(CultureInfo.InvariantCulture),
                ["viewport_width"] = resolution.Width.ToString(CultureInfo.InvariantCulture),
                ["viewport_height"] = resolution.Height.ToString(CultureInfo.InvariantCulture),
                ["viewport_id"] = resolution.ResolutionId
            });
    }

    private sealed class ReceiptBrowserAdapter(string artifactRoot) : IDesignConversionBrowserProcessAdapter
    {
        private readonly string _artifactRoot = artifactRoot;

        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            var physicalResolution = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
                request.Resolution,
                "RECEIPT_BROWSER_CAPTURE_RESOLUTION_INVALID",
                "External visual AI receipt browser adapter");
            if (!physicalResolution.IsSuccess)
                return physicalResolution.MapError<DesignConversionBrowserCaptureResult>();

            var render = await new DeterministicLocalDesignConversionPngRenderProvider(_artifactRoot)
                .RenderAsync(
                    new DesignConversionRenderRequest(
                        RenderRequestId: $"{request.CaptureRequestId}:receipt-browser-adapter-png",
                        RequestId: request.RequestId,
                        Html: request.DocumentHtml,
                        Css: "body{margin:0}.receipt-test{background:#f8fafc;color:#111827}",
                        Resolution: new RenderQaResolution(
                            $"{request.Resolution.ResolutionId}:physical-png",
                            physicalResolution.Value!.PhysicalWidth,
                            physicalResolution.Value.PhysicalHeight,
                            1.0),
                        Metadata: request.Metadata),
                    cancellationToken);
            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(new DesignConversionBrowserCaptureResult(
                ScreenshotUri: render.Value!.Image.Uri,
                ScreenshotSha256: render.Value.Image.Sha256["sha256:".Length..],
                ElapsedMs: 3,
                Metadata: new Dictionary<string, string>
                {
                    ["browser_adapter_kind"] = "external-visual-ai-receipt-browser-adapter",
                    ["browser_process_boundary"] = "adapter_supplied_inspectable_png_file"
                },
                Warnings: []));
        }
    }
}

internal sealed class ForgedReceiptVisualJudge : IDesignConversionVisualJudge
{
    public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
        DesignConversionVisualJudgeRequest request,
        CancellationToken cancellationToken = default)
    {
        var comparisonRequestId = request.Metadata.TryGetValue("comparison_request_id", out var value)
            ? value
            : request.JudgeRequestId;
        var judgeResultId = ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
            request.JudgeRequestId,
            comparisonRequestId);
        var metadata = DesignConversionExternalVisualAiReceiptTestHarness.BuildPlausibleExternalReceiptMetadata(
            request.JudgeRequestId,
            request.RequestId,
            comparisonRequestId);
        var evidenceRefs = new[]
        {
            "external_ai_visual_judge_executed:true",
            "network_call_executed:true",
            "real_external_provider_executed:true",
            $"{ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey}:true"
        };
        var verdict = new VisualVerdict(
            VerdictId: $"{request.JudgeRequestId}:forged-external-visual-judge",
            Score: 0.94,
            Defects: [],
            EvidenceRefs: evidenceRefs,
            Passed: true);

        return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
            JudgeResultId: judgeResultId,
            JudgeRequestId: request.JudgeRequestId,
            Status: ExternalHttpDesignConversionVisualJudge.StatusOk,
            IsBlocked: false,
            Verdict: verdict,
            Provider: new DesignConversionVisualJudgeProviderMetadata(
                ProviderName: "openai",
                EndpointHost: LoopbackVisualJudgeServer.EndpointHost,
                ModelId: "gpt-visual-loopback",
                ExternalCallExecuted: true,
                Metadata: metadata),
            EstimatedCostUsd: 0.01m,
            TokenMetadata: new Dictionary<string, double>(),
            EvidenceRefs: evidenceRefs,
            Metadata: metadata)));
    }
}

internal sealed class RecordingDownstreamVisualJudge : IDesignConversionVisualJudge
{
    private readonly DefaultLocalDesignConversionVisualJudge _inner = new();

    public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

    public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
        DesignConversionVisualJudgeRequest request,
        CancellationToken cancellationToken = default)
    {
        Requests.Add(request);
        return _inner.JudgeAsync(request, cancellationToken);
    }
}

internal sealed class LoopbackVisualJudgeServer : IAsyncDisposable
{
    public const string EndpointHost = "visual-ai-receipt.test";

    private readonly TcpListener _listener;
    private readonly X509Certificate2 _certificate;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _acceptLoop;
    private readonly ConcurrentBag<string> _requestBodies = [];
    private readonly ConcurrentBag<HttpClient> _clients = [];
    private readonly double _score;

    private int _requestCount;

    private LoopbackVisualJudgeServer(
        TcpListener listener,
        X509Certificate2 certificate,
        double score)
    {
        _listener = listener;
        _certificate = certificate;
        _score = score;
        var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        Endpoint = new Uri($"https://{EndpointHost}:{port}/visual-judge");
        _acceptLoop = Task.Run(AcceptLoopAsync);
    }

    public Uri Endpoint { get; }

    public int RequestCount => Volatile.Read(ref _requestCount);

    public IReadOnlyList<string> RequestBodies => _requestBodies.ToArray();

    public static Task<LoopbackVisualJudgeServer> StartAsync(double score)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return Task.FromResult(new LoopbackVisualJudgeServer(listener, CreateCertificate(), score));
    }

    public HttpClient CreateHttpClient()
    {
        var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (_, cancellationToken) =>
            {
                var client = new TcpClient(AddressFamily.InterNetwork);
                await client.ConnectAsync(IPAddress.Loopback, port, cancellationToken);
                return client.GetStream();
            },
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (_, _, _, _) => true
            }
        };
        var client = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        _clients.Add(client);
        return client;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in _clients)
            client.Dispose();

        _cts.Cancel();
        _listener.Stop();
        try
        {
            await _acceptLoop.WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch (Exception)
        {
            // Test cleanup should not hide the assertion that already ran.
        }

        _cts.Dispose();
        _certificate.Dispose();
    }

    private async Task AcceptLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            TcpClient? client = null;
            try
            {
                client = await _listener.AcceptTcpClientAsync(_cts.Token);
                _ = Task.Run(() => HandleClientAsync(client, _cts.Token));
            }
            catch (OperationCanceledException)
            {
                client?.Dispose();
                break;
            }
            catch (ObjectDisposedException)
            {
                client?.Dispose();
                break;
            }
            catch
            {
                client?.Dispose();
                if (_cts.IsCancellationRequested)
                    break;
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using var clientOwner = client;
        await using var ssl = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);
        await ssl.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
        {
            ServerCertificate = _certificate,
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            ClientCertificateRequired = false
        }, cancellationToken);

        var requestText = await ReadHttpRequestAsync(ssl, cancellationToken);
        var body = ExtractBody(requestText);
        _requestBodies.Add(body);
        Interlocked.Increment(ref _requestCount);

        var responseJson = BuildResponseJson(body);
        var responseBody = Encoding.UTF8.GetBytes(responseJson);
        var responseHeaders = Encoding.ASCII.GetBytes(string.Join(
            "\r\n",
            "HTTP/1.1 200 OK",
            "Content-Type: application/json",
            $"Content-Length: {responseBody.Length.ToString(CultureInfo.InvariantCulture)}",
            "Connection: close",
            "",
            ""));
        await ssl.WriteAsync(responseHeaders, cancellationToken);
        await ssl.WriteAsync(responseBody, cancellationToken);
        await ssl.FlushAsync(cancellationToken);
    }

    private string BuildResponseJson(string requestBody)
    {
        using var document = JsonDocument.Parse(requestBody);
        var root = document.RootElement;
        var judgeRequestId = root.GetProperty("judge_request_id").GetString() ?? string.Empty;
        var requestId = root.GetProperty("request_id").GetString() ?? string.Empty;
        var provider = root.GetProperty("provider").GetString() ?? "openai";
        var model = root.GetProperty("model").GetString() ?? "gpt-visual-loopback";
        return JsonSerializer.Serialize(new
        {
            judge_request_id = judgeRequestId,
            request_id = requestId,
            provider,
            provider_name = provider,
            model_id = model,
            score = _score,
            passed = _score >= 0.85,
            defects = Array.Empty<object>(),
            evidence_refs = new[]
            {
                "loopback_visual_judge_http_observed",
                "loopback_visual_judge_refs_only"
            },
            confidence = 0.99,
            tokens = new { prompt = 7, completion = 3 }
        });
    }

    private static async Task<string> ReadHttpRequestAsync(SslStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var bytes = new List<byte>();
        var headerEnd = -1;
        while (headerEnd < 0)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read <= 0)
                break;
            bytes.AddRange(buffer.AsSpan(0, read).ToArray());
            headerEnd = IndexOfHeaderEnd(bytes);
        }

        var contentLength = ReadContentLength(bytes, headerEnd);
        var bodyBytesRead = Math.Max(0, bytes.Count - (headerEnd + 4));
        while (bodyBytesRead < contentLength)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read <= 0)
                break;
            bytes.AddRange(buffer.AsSpan(0, read).ToArray());
            bodyBytesRead += read;
        }

        return Encoding.UTF8.GetString(bytes.ToArray());
    }

    private static string ExtractBody(string requestText)
    {
        var index = requestText.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        return index < 0 ? string.Empty : requestText[(index + 4)..];
    }

    private static int ReadContentLength(List<byte> bytes, int headerEnd)
    {
        if (headerEnd < 0)
            return 0;

        var headerText = Encoding.ASCII.GetString(bytes.Take(headerEnd).ToArray());
        foreach (var line in headerText.Split("\r\n"))
        {
            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
                continue;
            if (string.Equals(line[..separator], "Content-Length", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(line[(separator + 1)..].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }
        }

        return 0;
    }

    private static int IndexOfHeaderEnd(List<byte> bytes)
    {
        for (var index = 3; index < bytes.Count; index++)
        {
            if (bytes[index - 3] == '\r' &&
                bytes[index - 2] == '\n' &&
                bytes[index - 1] == '\r' &&
                bytes[index] == '\n')
            {
                return index - 3;
            }
        }

        return -1;
    }

    private static X509Certificate2 CreateCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            $"CN={EndpointHost}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        var san = new SubjectAlternativeNameBuilder();
        san.AddDnsName(EndpointHost);
        request.CertificateExtensions.Add(san.Build());
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        using var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        return X509CertificateLoader.LoadPkcs12(cert.Export(X509ContentType.Pfx), password: null);
    }
}
