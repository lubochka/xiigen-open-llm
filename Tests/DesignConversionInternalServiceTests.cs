// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionInternalServiceTests
{
    private const string FakeableVisualJudgeDeterministicEvidence =
        "deterministic-input-evidence:visual_judge_provider=optional_injected_fakeable";

    private const string ProviderRequiredVisualJudgeDeterministicEvidence =
        "deterministic-input-evidence:visual_judge_provider=provider_required_not_configured";

    [Fact]
    public async Task EvaluateAsync_OmittedVisualJudgeFailsClosedWithProviderRequiredSignal()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider);

        var result = await service.EvaluateAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("design-job-001", result.Value!.JobId);
        Assert.Equal("design-request-internal-001", result.Value.SubmittedRequest.RequestId);
        Assert.Equal(DesignConversionSourceKind.Figma, result.Value.SubmittedRequest.SourceKind);
        Assert.Equal("<main class=\"card\">Hello</main>", result.Value.SubmittedRequest.Html);
        Assert.Equal(".card{display:grid;}", result.Value.SubmittedRequest.Css);
        Assert.Equal("design-job-001", result.Value.SubmittedRequest.Provenance["internal_service_job_id"]);
        Assert.Single(renderProvider.Requests);
        Assert.Single(comparisonProvider.Requests);
        Assert.Equal("design-request-internal-001", renderProvider.Requests[0].RequestId);
        Assert.Equal("design-request-internal-001", comparisonProvider.Requests[0].RequestId);
        Assert.Equal("expected-design-render", comparisonProvider.Requests[0].Expected.Label);
        Assert.Single(result.Value.RenderResults);
        Assert.Single(result.Value.ComparisonArtifacts);
        var visualJudgeResult = Assert.Single(result.Value.VisualJudgeResults);
        var qaResult = Assert.Single(result.Value.QaResults);
        Assert.True(visualJudgeResult.IsBlocked);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, visualJudgeResult.Status);
        Assert.Equal(NoConfiguredVisualJudge.Status, visualJudgeResult.Status);
        Assert.False(visualJudgeResult.Provider.ExternalCallExecuted);
        Assert.False(visualJudgeResult.Verdict.Passed);
        Assert.Equal(0.0, visualJudgeResult.Verdict.Score);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, visualJudgeResult.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, visualJudgeResult.EvidenceRefs);
        Assert.Equal(1.0, qaResult.Metrics["visual_judge_blocked"]);
        Assert.Contains(NoConfiguredVisualJudge.Status, qaResult.Warnings);
        Assert.Equal(
            "untrusted_injected_comparison_provider_evidence",
            result.Value.ComparisonArtifacts[0].Metadata["comparison_evidence_trust_classification"]);
        Assert.Equal(bool.FalseString, result.Value.ComparisonArtifacts[0].Metadata["render_evidence_production_trusted"]);
        Assert.Equal(bool.FalseString, result.Value.ComparisonArtifacts[0].Metadata["visual_comparison_proven"]);
        Assert.Contains(DesignConversionInternalService.InjectedRendererMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.InjectedComparisonMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionInternalService.InjectedVisualJudgeMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(DefaultLocalDesignConversionVisualJudge.EvidenceMarker, result.Value.EvidenceRefs);
        var runtimeEvidenceRefs = result.Value.RuntimeFlowResult.EvidenceRefs
            .Concat(result.Value.RuntimeFlowResult.Output.Verdict.EvidenceRefs)
            .Concat(result.Value.EvidenceRefs)
            .ToArray();
        Assert.DoesNotContain(FakeableVisualJudgeDeterministicEvidence, runtimeEvidenceRefs);
        Assert.DoesNotContain(
            runtimeEvidenceRefs,
            evidenceRef => evidenceRef.Contains("local_visual_labeler", StringComparison.Ordinal));
        Assert.Contains(ProviderRequiredVisualJudgeDeterministicEvidence, runtimeEvidenceRefs);
        Assert.Contains(
            $"deterministic-input-evidence:visual_judge_provider_injected={bool.FalseString}",
            runtimeEvidenceRefs);
        Assert.Contains(
            $"deterministic-input-evidence:visual_judge_provider_required={bool.TrueString}",
            runtimeEvidenceRefs);
        Assert.Contains(
            $"deterministic-input-evidence:visual_judge_not_configured={bool.TrueString}",
            runtimeEvidenceRefs);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["visual_judge_provider_injected"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["external_ai_visual_judge_configured"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["local_visual_judge_explicitly_injected"]);
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["visual_judge_not_configured_fallback"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, result.Value.VisualQaBoundaryTrace.VisualJudgeStatus);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, result.Value.VisualQaBoundaryTrace.VisualJudgeProviderBoundary);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(result.Value.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);
        Assert.Contains("VISUAL_JUDGE_PROVIDER_REQUIRED_EXTERNAL_PROVIDER_NOT_CONFIGURED", result.Value.Warnings);
    }

    [Fact]
    public async Task EvaluateAsync_ExplicitDefaultLocalVisualJudgeRunsOnlyWhenSupplied()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var service = new DesignConversionInternalService(
            renderProvider,
            comparisonProvider,
            new DefaultLocalDesignConversionVisualJudge());

        var result = await service.EvaluateAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(renderProvider.Requests);
        Assert.Single(comparisonProvider.Requests);
        var visualJudgeResult = Assert.Single(result.Value!.VisualJudgeResults);
        var qaResult = Assert.Single(result.Value.QaResults);
        Assert.False(visualJudgeResult.IsBlocked);
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.StatusOk, visualJudgeResult.Status);
        Assert.False(visualJudgeResult.Provider.ExternalCallExecuted);
        Assert.False(visualJudgeResult.Verdict.Passed);
        Assert.Equal(0.0, qaResult.Metrics["visual_judge_blocked"]);
        Assert.Contains(DefaultLocalDesignConversionVisualJudge.EvidenceMarker, visualJudgeResult.EvidenceRefs);
        Assert.Contains(DefaultLocalDesignConversionVisualJudge.ExternalProviderNotUsedMarker, visualJudgeResult.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.InjectedVisualJudgeMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, result.Value.EvidenceRefs);
        var runtimeEvidenceRefs = result.Value.RuntimeFlowResult.EvidenceRefs
            .Concat(result.Value.RuntimeFlowResult.Output.Verdict.EvidenceRefs)
            .Concat(result.Value.EvidenceRefs)
            .ToArray();
        Assert.Contains(FakeableVisualJudgeDeterministicEvidence, runtimeEvidenceRefs);
        Assert.Contains(
            $"deterministic-input-evidence:visual_judge_provider_injected={bool.TrueString}",
            runtimeEvidenceRefs);
        Assert.Contains(
            $"deterministic-input-evidence:visual_judge_provider_kind={nameof(DefaultLocalDesignConversionVisualJudge)}",
            runtimeEvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalProviderNotConfiguredMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(NoConfiguredVisualJudge.Status, result.Value.Warnings);
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["visual_judge_provider_injected"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["external_ai_visual_judge_configured"]);
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["local_visual_judge_explicitly_injected"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["local_visual_judge_default_wired"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["visual_judge_not_configured_fallback"]);
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.StatusOk, result.Value.VisualQaBoundaryTrace.VisualJudgeStatus);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, result.Value.VisualQaBoundaryTrace.VisualJudgeProviderBoundary);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, result.Value.VisualQaBoundaryTrace.VisualDecision.ProviderBoundaryStatus);
    }

    [Fact]
    public async Task EvaluateAsync_CallsVisualJudgeWithComparisonArtifactsAndCarriesVerdict()
    {
        var visualJudgeVerdict = new VisualVerdict(
            VerdictId: "fake-visual-judge-verdict",
            Score: 0.67,
            Defects:
            [
                new VisualDefect(
                    DefectId: "visual-judge-defect-spacing",
                    Category: "spacing",
                    Severity: "major",
                    Description: "Rendered button spacing differs from expected design.",
                    X: 20,
                    Y: 30,
                    Width: 90,
                    Height: 24,
                    EvidenceImageIds: ["expected-design-render", "actual-render"])
            ],
            EvidenceRefs: ["visual-judge:fake-verdict"],
            Passed: false);
        var visualJudge = new RecordingVisualJudge(visualJudgeVerdict);
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true)),
            visualJudge);

        var result = await service.EvaluateAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(visualJudge.Requests);
        Assert.Equal("design-job-001:visual-judge-request:1", visualJudge.Requests[0].JudgeRequestId);
        Assert.Equal("design-request-internal-001", visualJudge.Requests[0].RequestId);
        Assert.Equal("design-render-001:expected:1", visualJudge.Requests[0].ExpectedScreenshot.ImageId);
        Assert.Equal("design-job-001:render-request:1:actual", visualJudge.Requests[0].RenderedImage.ImageId);
        Assert.Equal("artifact://diff.png", visualJudge.Requests[0].DiffArtifactUri);
        Assert.Equal("sha256:diff", visualJudge.Requests[0].DiffArtifactSha256);
        Assert.Equal("design-job-001:comparison-request:1:artifact", visualJudge.Requests[0].Metadata["comparison_artifact_id"]);
        Assert.Single(result.Value!.VisualJudgeResults);
        Assert.False(result.Value.VisualJudgeResults[0].IsBlocked);
        Assert.False(result.Value.VisualJudgeResults[0].Provider.ExternalCallExecuted);
        Assert.Equal("fake-visual-judge-verdict", result.Value.VisualJudgeResults[0].Verdict.VerdictId);
        Assert.Single(result.Value.VisualJudgeResults[0].Verdict.Defects);
        Assert.Equal("spacing", result.Value.VisualJudgeResults[0].Verdict.Defects[0].Category);
        Assert.Equal("fake-visual-judge-verdict", result.Value.QaResults[0].Verdict.VerdictId);
        Assert.Equal(0.67, result.Value.QaResults[0].Metrics["visual_judge_score"]);
        Assert.Contains("visual-judge:fake-provider", result.Value.EvidenceRefs);
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["visual_judge_provider_injected"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["external_ai_visual_judge_configured"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["local_visual_judge_explicitly_injected"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["local_visual_judge_default_wired"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["visual_judge_not_configured_fallback"]);
    }

    [Fact]
    public async Task EvaluateAsync_IncludesCanonicalizedExternalComparisonCostInVisualQaBoundary()
    {
        var handler = new SuccessfulExternalVisualJudgeHandler();
        using var httpClient = new HttpClient(handler);
        var externalJudge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-provider.example.test/v1/judge"),
                ProviderName: "openai",
                ModelId: "gpt-visual-boundary",
                ApiKey: "test-api-key",
                ServerOwnedCostPerRequestUsd: 0.0042m),
            httpClient,
            new ConstantVisualJudgeImageContentResolver());
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(externalJudge);
        var service = new DesignConversionInternalService(
            new ExternalCostAccountingRenderProvider(),
            comparisonProvider,
            new RecordingVisualJudge(CreateVerdict("local-visual-low-score", 0.70, passed: false)));
        var baseRequest = CreateRequest();
        var request = baseRequest with
        {
            DesignRenders =
            [
                baseRequest.DesignRenders[0] with
                {
                    Sha256 = $"sha256:{ExternalVisualJudgePngSha256}"
                }
            ],
            Screenshots =
            [
                baseRequest.Screenshots[0] with
                {
                    Sha256 = $"sha256:{ExternalVisualJudgePngSha256}"
                }
            ],
            Budget = baseRequest.Budget with
            {
                MaxImprovementCandidates = 3,
                BudgetUsd = 0.003m
            }
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, handler.CallCount);
        Assert.Single(result.Value!.ComparisonArtifacts);
        var comparisonMetadata = result.Value.ComparisonArtifacts[0].Metadata;
        Assert.Equal(
            DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification,
            comparisonMetadata[DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey]);
        Assert.Equal(bool.TrueString, comparisonMetadata["server_owned_external_visual_ai_receipt_validated"]);
        Assert.Equal(bool.TrueString, comparisonMetadata["network_call_executed"]);
        Assert.Equal(
            bool.TrueString,
            comparisonMetadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(
            bool.TrueString,
            comparisonMetadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(
            bool.TrueString,
            comparisonMetadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
        Assert.Equal(bool.TrueString, comparisonMetadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        Assert.Equal("0.0042", comparisonMetadata["estimated_cost_usd"]);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.DecisionBlocked, result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.Equal(0.0042m, result.Value.VisualQaBoundaryTrace.TierPolicy.EstimatedCostUsd);
        Assert.Equal("0.0042", result.Value.VisualQaBoundaryTrace.TierPolicy.Metadata["estimated_cost_usd"]);
        Assert.Equal("true", result.Value.VisualQaBoundaryTrace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("0", result.Value.VisualQaBoundaryTrace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.CostCapMarker, result.Value.VisualQaBoundaryTrace.EvidenceRefs);
    }

    [Fact]
    public async Task EvaluateAsync_IncludesAllExternalComparisonAndJudgeCostsInVisualQaBoundary()
    {
        var handler = new RecordingMatchingExternalVisualJudgeHandler();
        using var httpClient = new HttpClient(handler);
        var externalJudge = new ExternalHttpDesignConversionVisualJudge(
            new DesignConversionExternalVisualJudgeOptions(
                Endpoint: new Uri("https://visual-provider.example.test/v1/judge"),
                ProviderName: "openai",
                ModelId: "gpt-visual-boundary",
                ApiKey: "test-api-key",
                ServerOwnedCostPerRequestUsd: 0.0125m),
            httpClient,
            new ConstantVisualJudgeImageContentResolver());
        var comparisonProvider = new DesignConversionExternalVisualAiComparisonProvider(externalJudge);
        var service = new DesignConversionInternalService(
            new ExternalCostAccountingRenderProvider(),
            comparisonProvider,
            externalJudge);
        var request = WithExternalVisualJudgePngRefs(CreateTwoViewportRequest(maxRenderQaRuns: 2));

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(4, handler.CallCount);
        Assert.Equal(2, result.Value!.ComparisonArtifacts.Count);
        Assert.Equal(2, result.Value.VisualJudgeResults.Count);
        Assert.All(result.Value.ComparisonArtifacts, AssertChargeableExternalComparisonCost);
        Assert.All(result.Value.VisualJudgeResults, AssertChargeableExternalJudgeCost);
        Assert.Equal(0.05m, result.Value.VisualQaBoundaryTrace.TierPolicy.EstimatedCostUsd);
        Assert.True(result.Value.VisualQaBoundaryTrace.TierPolicy.ExternalCostMeasurementComplete);
        Assert.Equal(0, result.Value.VisualQaBoundaryTrace.TierPolicy.MissingExternalCostMeasurementCount);
        Assert.Equal("0.05", result.Value.VisualQaBoundaryTrace.TierPolicy.Metadata["estimated_cost_usd"]);
        Assert.Equal("true", result.Value.VisualQaBoundaryTrace.TierPolicy.Metadata["external_cost_measurement_complete"]);
        Assert.Equal("0", result.Value.VisualQaBoundaryTrace.TierPolicy.Metadata["missing_external_cost_measurement_count"]);
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            result.Value.VisualQaBoundaryTrace.TierPolicy.Metadata["cost_source"]);
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["visual_judge_provider_injected"]);
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["external_ai_visual_judge_configured"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["local_visual_judge_explicitly_injected"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["local_visual_judge_default_wired"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["visual_judge_not_configured_fallback"]);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.CostMeasurementRequired, result.Value.VisualQaBoundaryTrace.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.ExternalVisualCostMeasurementMissingMarker, result.Value.VisualQaBoundaryTrace.EvidenceRefs);
        Assert.DoesNotContain("EXTERNAL_VISUAL_COST_MEASUREMENT_MISSING", result.Value.Warnings);
    }

    [Fact]
    public async Task EvaluateAsync_BuildsVisualJudgeRequestFromExpectedAndRenderedArtifactsWithoutRawPrivateFields()
    {
        const string privateHtml = "<main data-private=\"luba\">private html</main>";
        const string privateCss = ".private-user{color:red;}";
        const string privateTree = "{\"private\":\"tree-content\"}";
        const string privateBudgetToken = "budget-private-token-456";
        var visualJudge = new RecordingVisualJudge(CreateVerdict("artifact-only-judge", 0.72, passed: false));
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("comparison-artifact-only", 0.72, passed: false)),
            visualJudge);
        var baseRequest = CreateRequest();
        var request = baseRequest with
        {
            Html = privateHtml,
            Css = privateCss,
            Budget = baseRequest.Budget with
            {
                Metadata = new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90",
                    ["private_budget_token"] = privateBudgetToken
                }
            },
            DesignTreePayload = new DesignTreePayload(
                PayloadId: "tree-private-current",
                Format: "figma-json",
                Content: privateTree,
                Sha256: "sha256:tree-private-current",
                Metadata: new Dictionary<string, string>
                {
                    ["private_user_id"] = "user-private-123"
                }),
            OriginalTreePayload = new DesignTreePayload(
                PayloadId: "tree-private-original",
                Format: "figma-json",
                Content: "{\"private\":\"original-tree\"}",
                Sha256: "sha256:tree-private-original",
                Metadata: new Dictionary<string, string>())
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(visualJudge.Requests);
        var judgeRequest = visualJudge.Requests[0];
        Assert.Equal("design-render-001:expected:1", judgeRequest.ExpectedScreenshot.ImageId);
        Assert.Equal("expected-design-render", judgeRequest.ExpectedScreenshot.Label);
        Assert.Equal("artifact://design-render.png", judgeRequest.ExpectedScreenshot.Uri);
        Assert.Equal("sha256:design-render", judgeRequest.ExpectedScreenshot.Sha256);
        Assert.Equal("design-job-001:render-request:1:actual", judgeRequest.RenderedImage.ImageId);
        Assert.Equal("actual-render", judgeRequest.RenderedImage.Label);
        Assert.Equal("artifact://actual-render.png", judgeRequest.RenderedImage.Uri);
        Assert.Equal("sha256:actual-render", judgeRequest.RenderedImage.Sha256);
        Assert.Equal("artifact://diff.png", judgeRequest.DiffArtifactUri);
        Assert.Equal("sha256:diff", judgeRequest.DiffArtifactSha256);
        Assert.Contains("comparison_artifact_id", judgeRequest.Metadata.Keys);
        Assert.Equal("0.9", judgeRequest.Metadata["quality_threshold"]);
        Assert.DoesNotContain("html", judgeRequest.Metadata.Keys);
        Assert.DoesNotContain("css", judgeRequest.Metadata.Keys);
        Assert.DoesNotContain("tree", judgeRequest.Metadata.Keys);
        var metadataText = string.Join(" ", judgeRequest.Metadata.Keys.Concat(judgeRequest.Metadata.Values));
        Assert.DoesNotContain(privateHtml, metadataText, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCss, metadataText, StringComparison.Ordinal);
        Assert.DoesNotContain(privateTree, metadataText, StringComparison.Ordinal);
        Assert.DoesNotContain("user-private-123", metadataText, StringComparison.Ordinal);
        Assert.DoesNotContain(privateBudgetToken, metadataText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_BuildsRuntimeFlowInputAndInvokesRoutingFlow()
    {
        var order = new List<string>();
        var critic = new RecordingCriticUnit(order);
        var improver = new RecordingImproverUnit(order);
        var flow = new DesignConversionRoutingFlow(critic, improver);
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("comparison-fail", 0.42, passed: false)),
            flow);

        var result = await service.EvaluateAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(["critic", "improver"], order);
        Assert.NotNull(critic.Input);
        Assert.Single(critic.Input!.QaResults);
        Assert.Single(critic.Input.ComparisonArtifacts);
        Assert.Equal("comparison-fail", critic.Input.ComparisonArtifacts[0].Verdict.VerdictId);
        Assert.Equal("service_first_no_host", critic.Input.DeterministicEvidenceRefs["internal_service"]);
        Assert.Equal("injected_fakeable", critic.Input.DeterministicEvidenceRefs["render_provider"]);
        Assert.Equal("provider_required_not_configured", critic.Input.DeterministicEvidenceRefs["visual_judge_provider"]);
        Assert.Equal(bool.FalseString, critic.Input.DeterministicEvidenceRefs["visual_judge_provider_injected"]);
        Assert.Equal(bool.FalseString, critic.Input.DeterministicEvidenceRefs["deterministic_logic_claimed_as_learning"]);
        Assert.Equal(["recording-critic", "recording-improver"], result.Value!.RuntimeFlowResult.UnitExecutionOrder);
    }

    [Fact]
    public async Task EvaluateAsync_PropagatesFlowOutputVerdictAndImprovedMarkup()
    {
        var criticVerdict = CreateVerdict("critic-verdict-custom", 0.77, passed: true);
        var flow = new DesignConversionRoutingFlow(
            new RecordingCriticUnit([], criticVerdict),
            new RecordingImproverUnit([], "<main class=\"card improved\">Hello</main>", ".card{display:grid;gap:12px;}"));
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.93, passed: true)),
            flow);

        var result = await service.EvaluateAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("critic-verdict-custom", result.Value!.Verdict.VerdictId);
        Assert.True(result.Value.Verdict.Passed);
        Assert.Equal("<main class=\"card improved\">Hello</main>", result.Value.ImprovedHtml);
        Assert.Equal(".card{display:grid;gap:12px;}", result.Value.ImprovedCss);
        Assert.Equal(result.Value.ImprovedHtml, result.Value.RuntimeFlowResult.Output.Html);
        Assert.Equal(result.Value.ImprovedCss, result.Value.RuntimeFlowResult.Output.Css);
    }

    [Fact]
    public async Task EvaluateAsync_DoesNotHideDeterministicLearningClaim()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.93, passed: true)));

        var result = await service.EvaluateAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False(result.Value!.DeterministicLogicClaimedAsLearning);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["deterministic_logic_claimed_as_learning"]);
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["browser_renderer_provider_available"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["browser_renderer_default_production_wired"]);
        Assert.Equal("not_browser_backed_for_current_injection", result.Value.ScaffoldMarkers["browser_renderer_default_scope"]);
        Assert.Equal(nameof(RecordingRenderProvider), result.Value.ScaffoldMarkers["browser_renderer_injected_provider_kind"]);
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["external_ai_visual_judge_provider_available"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["external_ai_visual_judge_configured"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["local_visual_judge_default_wired"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["local_visual_judge_explicitly_injected"]);
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["visual_judge_not_configured_fallback"]);
        Assert.DoesNotContain("browser_renderer_default_wired", result.Value.ScaffoldMarkers.Keys);
        Assert.DoesNotContain("browser_renderer_implemented", result.Value.ScaffoldMarkers.Keys);
        Assert.DoesNotContain("external_ai_visual_judge_implemented", result.Value.ScaffoldMarkers.Keys);
        Assert.Contains(DesignConversionInternalService.NoLearningClaimMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning, result.Value.Warnings);
    }

    [Fact]
    public async Task EvaluateAsync_BrowserBackedInjectedProviderMarksExplicitLocalProofWithoutProductionDefaultClaim()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var service = new DesignConversionInternalService(
            new BrowserBackedDesignConversionRenderProvider(browserAdapter),
            new RecordingComparisonProvider(CreateVerdict("browser-comparison-pass", 0.94, passed: true)));

        var result = await service.EvaluateAsync(CreateRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(browserAdapter.Requests);
        Assert.Equal(bool.TrueString, result.Value!.ScaffoldMarkers["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, result.Value.ScaffoldMarkers["browser_renderer_default_production_wired"]);
        Assert.Equal("explicit_local_injection_only_not_production_default", result.Value.ScaffoldMarkers["browser_renderer_default_scope"]);
        Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, result.Value.ScaffoldMarkers["browser_renderer_injected_provider_kind"]);
        Assert.DoesNotContain("browser_renderer_default_wired", result.Value.ScaffoldMarkers.Keys);
        Assert.All(result.Value.RenderResults, render =>
        {
            Assert.Equal(bool.TrueString, render.Image.Metadata["browser_or_playwright_used"]);
            Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, render.Image.Metadata["provider_kind"]);
            Assert.Equal(bool.FalseString, render.Image.Metadata["external_ai_used"]);
        });
        Assert.False(result.Value.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);
        Assert.False(result.Value.VisualQaBoundaryTrace.TrainedFeatureClaimed);
    }

    [Fact]
    public async Task EvaluateAsync_FeedsDeterministicQaProvidersAcrossMultipleResolutions()
    {
        var service = new DesignConversionInternalService(
            new DeterministicDesignConversionRenderProvider(),
            new DeterministicDesignConversionComparisonProvider());
        var request = CreateRequest() with
        {
            Budget = new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "internal",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>()),
            RenderResolutions =
            [
                new RenderQaResolution("desktop", 1440, 900, 1.0),
                new RenderQaResolution("mobile", 390, 844, 3.0)
            ],
            Screenshots =
            [
                new DesignRenderArtifact(
                    ArtifactId: "source-shot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://source-shot-desktop.png",
                    Sha256: "sha256:source-shot-desktop",
                    Metadata: new Dictionary<string, string>()),
                new DesignRenderArtifact(
                    ArtifactId: "source-shot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://source-shot-mobile.png",
                    Sha256: "sha256:source-shot-mobile",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders =
            [
                new DesignRenderArtifact(
                    ArtifactId: "design-render-desktop",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://design-render-desktop.png",
                    Sha256: "sha256:design-render-desktop",
                    Metadata: new Dictionary<string, string>()),
                new DesignRenderArtifact(
                    ArtifactId: "design-render-mobile",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://design-render-mobile.png",
                    Sha256: "sha256:design-render-mobile",
                    Metadata: new Dictionary<string, string>())
            ]
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.RenderResults.Count);
        Assert.Equal(2, result.Value.ComparisonArtifacts.Count);
        Assert.Equal(["desktop", "mobile"], result.Value.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.All(result.Value.RenderResults, render =>
        {
            Assert.Equal("deterministic_render_scaffold", render.Image.Metadata["provider_kind"]);
            Assert.Equal(bool.FalseString, render.Image.Metadata["visual_fidelity_claimed"]);
            Assert.Equal("metadata_only_render_evidence", render.Image.Metadata["hidden_determinism_boundary"]);
        });
        Assert.All(result.Value.ComparisonArtifacts, artifact =>
        {
            Assert.Contains(DeterministicDesignConversionComparisonProvider.AiJudgeClaimedMarker, artifact.EvidenceRefs);
            Assert.Contains(DeterministicDesignConversionComparisonProvider.BoundaryMarker, artifact.Verdict.EvidenceRefs);
            Assert.Equal(0.0, artifact.Metrics["ai_visual_judge_claimed"]);
        });
        Assert.Contains(DesignConversionInternalService.InjectedRendererMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.InjectedComparisonMarker, result.Value.EvidenceRefs);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task EvaluateAsync_RequiresPositiveImprovementCandidateBudgetBeforeProviderCalls(
        int maxImprovementCandidates)
    {
        const string privateUserId = "user-private-candidate-budget-123";
        const string privateHtml = "<main data-private=\"candidate-budget\">Secret</main>";
        const string privateCss = ".candidate-budget-private{color:red;}";
        const string privateTree = "{\"private\":\"candidate-budget-tree\"}";
        const string privateOriginalTree = "{\"private\":\"candidate-budget-original-tree\"}";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.92, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var request = CreateRequest() with
        {
            Html = privateHtml,
            Css = privateCss,
            DesignTreePayload = new DesignTreePayload(
                PayloadId: "tree-private-current",
                Format: "figma-json",
                Content: privateTree,
                Sha256: "sha256:tree-private-current",
                Metadata: new Dictionary<string, string>
                {
                    ["private_user_id"] = privateUserId
                }),
            OriginalTreePayload = new DesignTreePayload(
                PayloadId: "tree-private-original",
                Format: "figma-json",
                Content: privateOriginalTree,
                Sha256: "sha256:tree-private-original",
                Metadata: new Dictionary<string, string>()),
            Budget = new DesignConversionBudgetMetadata(
                UserId: privateUserId,
                Tier: "internal",
                MaxImprovementCandidates: maxImprovementCandidates,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>())
        };

        var result = await service.EvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, result.ErrorCode);
        Assert.Equal(
            "DesignConversion improvement candidate budget must be greater than zero.",
            result.ErrorMessage);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.DoesNotContain(privateUserId, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateHtml, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCss, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateTree, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateOriginalTree, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_RejectsNegativeMaxTeacherCallsBeforeProviderCalls()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.92, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var request = CreateRequest();
        request = request with
        {
            Budget = request.Budget with
            {
                MaxTeacherCalls = -1
            }
        };

        var result = await service.EvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsInvalidErrorCode, result.ErrorCode);
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.MaxTeacherCallsInvalidErrorMessage(), result.ErrorMessage);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
    }

    [Fact]
    public async Task EvaluateAsync_RejectsNegativeBudgetUsdForKnownTierZeroEntitlementBeforeProviderCalls()
    {
        const string privateUserId = "user-private-negative-budget-123";
        const string privateHtml = "<main data-private=\"negative-budget\">Secret</main>";
        const string privateCss = ".negative-budget-private{color:red;}";
        const string privateTree = "{\"private\":\"negative-budget-tree\"}";
        const string privateOriginalTree = "{\"private\":\"negative-budget-original-tree\"}";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.92, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var request = CreateRequest() with
        {
            Html = privateHtml,
            Css = privateCss,
            DesignTreePayload = new DesignTreePayload(
                PayloadId: "tree-private-current",
                Format: "figma-json",
                Content: privateTree,
                Sha256: "sha256:tree-private-current",
                Metadata: new Dictionary<string, string>
                {
                    ["private_user_id"] = privateUserId
                }),
            OriginalTreePayload = new DesignTreePayload(
                PayloadId: "tree-private-original",
                Format: "figma-json",
                Content: privateOriginalTree,
                Sha256: "sha256:tree-private-original",
                Metadata: new Dictionary<string, string>()),
            Budget = new DesignConversionBudgetMetadata(
                UserId: privateUserId,
                Tier: "free",
                MaxImprovementCandidates: 0,
                MaxRenderQaRuns: 1,
                BudgetUsd: -0.01m,
                Metadata: new Dictionary<string, string>())
        };

        var result = await service.EvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionInternalService.NegativeBudgetUsdErrorCode, result.ErrorCode);
        Assert.Equal(
            "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
            result.ErrorMessage);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.DoesNotContain(privateUserId, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateHtml, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCss, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateTree, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateOriginalTree, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Html")]
    [InlineData("Css")]
    [InlineData("DesignTreePayload.Content")]
    [InlineData("OriginalTreePayload.Content")]
    public async Task EvaluateAsync_RejectsOversizedRawFieldsBeforeProviderCalls(
        string oversizedField)
    {
        const string privateMarker = "PRIVATE_N23_4_OVERSIZED_PAYLOAD_MARKER";
        const string privateUserId = "user-private-n23-4-size-guard";
        var oversizedPayload = CreateRawPayload(
            DesignConversionInternalService.MaxRawPayloadChars + 1,
            privateMarker);
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.92, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var request = WithRawFieldContent(CreateRequest() with
        {
            Budget = new DesignConversionBudgetMetadata(
                UserId: privateUserId,
                Tier: "internal",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>()),
            Screenshots =
            [
                new DesignRenderArtifact(
                    ArtifactId: "source-shot-private-n23-4",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://private-n23-4-source-shot.png",
                    Sha256: "sha256:private-n23-4-source-shot",
                    Metadata: new Dictionary<string, string>())
            ]
        }, oversizedField, oversizedPayload);

        var result = await service.EvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionInternalService.PayloadTooLargeCode, result.ErrorCode);
        Assert.Contains(oversizedField, result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains(DesignConversionInternalService.MaxRawPayloadChars.ToString(), result.ErrorMessage, StringComparison.Ordinal);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.DoesNotContain(privateMarker, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(oversizedPayload, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(privateUserId, result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact://", result.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain("sha256", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EvaluateAsync_AllowsRawFieldAtExactSizeLimit()
    {
        var atLimitPayload = CreateRawPayload(
            DesignConversionInternalService.MaxRawPayloadChars,
            "N23_4_AT_LIMIT_PAYLOAD_MARKER");
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.92, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var request = WithRawFieldContent(CreateRequest(), "Html", atLimitPayload);

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(renderProvider.Requests);
        Assert.Single(comparisonProvider.Requests);
        Assert.Single(visualJudge.Requests);
        Assert.Equal(atLimitPayload.Length, renderProvider.Requests[0].Html.Length);
    }

    [Fact]
    public async Task EvaluateAsync_RetainsRenderedViewportWhenExpectedReferenceMissingAndContinues()
    {
        const string privateMissingResolutionId = "private-missing-resolution-user-123";
        const string privateHtml = "<main data-private=\"secret-html\">Secret</main>";
        const string privateCss = ".secret-user-css{color:red;}";
        const string privateTree = "{\"private\":\"tree-content\"}";
        var renderProvider = new ViewportFailureRenderProvider(failAtRequestNumber: null);
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-needs-retry", 0.72, passed: false));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-needs-retry", 0.72, passed: false));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var request = CreateTwoViewportRequest(maxRenderQaRuns: 2) with
        {
            Html = privateHtml,
            Css = privateCss,
            DesignTreePayload = new DesignTreePayload(
                PayloadId: "tree-private-current",
                Format: "figma-json",
                Content: privateTree,
                Sha256: "sha256:tree-private-current",
                Metadata: new Dictionary<string, string>
                {
                    ["private_user_id"] = "user-private-123"
                }),
            Budget = new DesignConversionBudgetMetadata(
                UserId: "user-private-123",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.50m,
                Metadata: new Dictionary<string, string>()),
            RenderResolutions =
            [
                new RenderQaResolution(privateMissingResolutionId, 390, 844, 3.0),
                new RenderQaResolution("desktop", 1440, 900, 1.0)
            ],
            Screenshots =
            [
                new DesignRenderArtifact(
                    ArtifactId: "source-shot-mobile-wrong-size-private",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 843,
                    Uri: "artifact://private-source-shot-mobile-wrong-size.png",
                    Sha256: "sha256:private-source-shot-mobile-wrong-size",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders =
            [
                new DesignRenderArtifact(
                    ArtifactId: "design-render-mobile-wrong-size-private",
                    Kind: "design-render",
                    Width: 391,
                    Height: 844,
                    Uri: "artifact://private-design-render-mobile-wrong-size.png",
                    Sha256: "sha256:private-design-render-mobile-wrong-size",
                    Metadata: new Dictionary<string, string>()),
                new DesignRenderArtifact(
                    ArtifactId: "design-render-desktop-private",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://private-design-render-desktop.png",
                    Sha256: "sha256:private-design-render-desktop",
                    Metadata: new Dictionary<string, string>())
            ]
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(
            new[] { privateMissingResolutionId, "desktop" },
            renderProvider.Requests.Select(renderRequest => renderRequest.Resolution.ResolutionId).ToArray());
        Assert.Equal(2, result.Value!.RenderResults.Count);
        Assert.Contains(result.Value.RenderResults, render =>
            render.Image.Resolution.Width == 390 &&
            render.Image.Resolution.Height == 844 &&
            render.Image.Resolution.DeviceScaleFactor == 3.0);
        Assert.Contains(result.Value.RenderResults, render =>
            render.Image.Resolution.Width == 1440 &&
            render.Image.Resolution.Height == 900 &&
            render.Image.Resolution.DeviceScaleFactor == 1.0);

        Assert.Single(comparisonProvider.Requests);
        Assert.Equal("design-job-001:comparison-request:2", comparisonProvider.Requests[0].ComparisonRequestId);
        Assert.Equal("design-render-desktop-private:expected:2", comparisonProvider.Requests[0].Expected.ImageId);
        Assert.Equal("desktop", comparisonProvider.Requests[0].Expected.Resolution.ResolutionId);
        Assert.Equal(1440, comparisonProvider.Requests[0].Expected.Resolution.Width);
        Assert.Equal(900, comparisonProvider.Requests[0].Expected.Resolution.Height);
        Assert.Equal("design-job-001:render-request:2:actual", comparisonProvider.Requests[0].Actual.ImageId);
        Assert.DoesNotContain("wrong-size", comparisonProvider.Requests[0].Expected.ImageId, StringComparison.Ordinal);
        Assert.DoesNotContain(comparisonProvider.Requests, comparisonRequest =>
            comparisonRequest.Actual.Resolution.Width == 390 &&
            comparisonRequest.Actual.Resolution.Height == 844);

        Assert.Single(visualJudge.Requests);
        Assert.Equal("design-job-001:visual-judge-request:2", visualJudge.Requests[0].JudgeRequestId);
        Assert.Equal("design-render-desktop-private:expected:2", visualJudge.Requests[0].ExpectedScreenshot.ImageId);
        Assert.Equal("design-job-001:render-request:2:actual", visualJudge.Requests[0].RenderedImage.ImageId);
        Assert.Single(result.Value.ComparisonArtifacts);
        Assert.Single(result.Value.VisualJudgeResults);
        Assert.Single(result.Value.QaResults);
        Assert.Equal("design-job-001:comparison-request:2:artifact", result.Value.ComparisonArtifacts[0].ArtifactId);
        Assert.Equal("design-job-001:visual-judge-request:2:fake-result", result.Value.VisualJudgeResults[0].JudgeResultId);
        Assert.Equal("design-job-001:qa-result:2", result.Value.QaResults[0].ResultId);
        Assert.DoesNotContain(result.Value.ComparisonArtifacts, artifact =>
            artifact.Actual.Resolution.Width == 390 &&
            artifact.Actual.Resolution.Height == 844);
        Assert.DoesNotContain(result.Value.VisualJudgeResults, judge =>
            judge.JudgeRequestId.EndsWith(":1", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Value.QaResults, qa =>
            qa.Actual.Resolution.Width == 390 &&
            qa.Actual.Resolution.Height == 844);

        var failure = Assert.Single(result.Value.ViewportFailures);
        Assert.Equal("viewport-1", failure.ViewportId);
        Assert.Equal(390, failure.Width);
        Assert.Equal(844, failure.Height);
        Assert.Equal(3.0, failure.DeviceScaleFactor);
        Assert.Equal(1, failure.OneBasedIndex);
        Assert.Equal("expected_reference", failure.Stage);
        Assert.Equal(DesignConversionInternalService.ExpectedRenderResolutionMissingCode, failure.ErrorCode);
        Assert.Contains(DesignConversionInternalService.ViewportOperationFailureMarker, failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:stage=expected_reference", failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:index=1", failure.EvidenceRefs);
        Assert.Contains(
            $"viewport_operation_failure:error_code={DesignConversionInternalService.ExpectedRenderResolutionMissingCode}",
            failure.EvidenceRefs);
        Assert.DoesNotContain(failure.EvidenceRefs, evidenceRef =>
            evidenceRef.Contains(privateMissingResolutionId, StringComparison.Ordinal) ||
            evidenceRef.Contains("artifact", StringComparison.OrdinalIgnoreCase) ||
            evidenceRef.Contains("sha", StringComparison.OrdinalIgnoreCase) ||
            evidenceRef.Contains("diff", StringComparison.OrdinalIgnoreCase));

        Assert.Contains(DesignConversionInternalService.ViewportExpectedReferenceMissingStatus, result.Value.Warnings);
        Assert.Contains(DesignConversionInternalService.ViewportOperationFailureMarker, result.Value.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:stage=expected_reference", result.Value.EvidenceRefs);
        Assert.Contains(
            $"viewport_operation_failure:error_code={DesignConversionInternalService.ExpectedRenderResolutionMissingCode}",
            result.Value.EvidenceRefs);
        Assert.False(result.Value.Verdict.Passed);
        Assert.False(result.Value.RuntimeFlowResult.Output.Accepted);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed,
            result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.False(result.Value.VisualQaBoundaryTrace.TierPolicy.ShouldContinue);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(result.Value.VisualQaBoundaryTrace.VisualDecision.QualityThresholdMet);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);

        var publicFailureText = string.Join(
            " ",
            result.Value.EvidenceRefs
                .Concat(result.Value.Warnings)
                .Concat(result.Value.ViewportFailures.Select(viewportFailure => viewportFailure.Stage))
                .Concat(result.Value.ViewportFailures.Select(viewportFailure => viewportFailure.ErrorCode))
                .Concat(result.Value.ViewportFailures.SelectMany(viewportFailure => viewportFailure.EvidenceRefs))
                .Concat(result.Value.RuntimeFlowResult.Output.Verdict.EvidenceRefs));
        Assert.DoesNotContain(privateMissingResolutionId, publicFailureText, StringComparison.Ordinal);
        Assert.DoesNotContain(privateHtml, publicFailureText, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCss, publicFailureText, StringComparison.Ordinal);
        Assert.DoesNotContain(privateTree, publicFailureText, StringComparison.Ordinal);
        Assert.DoesNotContain("user-private-123", publicFailureText, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact://", publicFailureText, StringComparison.Ordinal);
        Assert.DoesNotContain("sha256:", publicFailureText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private-design-render", publicFailureText, StringComparison.Ordinal);
        Assert.DoesNotContain("private-source-shot", publicFailureText, StringComparison.Ordinal);
        Assert.DoesNotContain("diff.png", publicFailureText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_UsesExactScreenshotWhenNoExactDesignRenderExists()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider);
        var request = CreateRequest() with
        {
            RenderResolutions = [new RenderQaResolution("desktop", 1440, 900, 2.0)],
            DesignRenders =
            [
                new DesignRenderArtifact(
                    ArtifactId: "design-render-wrong-size",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://design-render-wrong-size.png",
                    Sha256: "sha256:design-render-wrong-size",
                    Metadata: new Dictionary<string, string>())
            ],
            Screenshots =
            [
                new DesignRenderArtifact(
                    ArtifactId: "source-shot-exact",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://source-shot-exact.png",
                    Sha256: "sha256:source-shot-exact",
                    Metadata: new Dictionary<string, string>())
            ]
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(renderProvider.Requests);
        Assert.Single(comparisonProvider.Requests);
        var expected = comparisonProvider.Requests[0].Expected;
        Assert.Equal("source-shot-exact:expected:1", expected.ImageId);
        Assert.Equal("artifact://source-shot-exact.png", expected.Uri);
        Assert.Equal("sha256:source-shot-exact", expected.Sha256);
        Assert.Equal("source-shot-exact", expected.Metadata["source_artifact_id"]);
        Assert.Equal("screenshot", expected.Metadata["source_artifact_kind"]);
        Assert.Equal("desktop", expected.Resolution.ResolutionId);
        Assert.Equal(1440, expected.Resolution.Width);
        Assert.Equal(900, expected.Resolution.Height);
        Assert.Equal(2.0, expected.Resolution.DeviceScaleFactor);
    }

    [Fact]
    public async Task EvaluateAsync_UsesExactDesignRenderBeforeExactScreenshot()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider);
        var request = CreateRequest() with
        {
            DesignRenders =
            [
                new DesignRenderArtifact(
                    ArtifactId: "design-render-exact",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://design-render-exact.png",
                    Sha256: "sha256:design-render-exact",
                    Metadata: new Dictionary<string, string>())
            ],
            Screenshots =
            [
                new DesignRenderArtifact(
                    ArtifactId: "source-shot-exact",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://source-shot-exact.png",
                    Sha256: "sha256:source-shot-exact",
                    Metadata: new Dictionary<string, string>())
            ]
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(renderProvider.Requests);
        Assert.Single(comparisonProvider.Requests);
        var expected = comparisonProvider.Requests[0].Expected;
        Assert.Equal("design-render-exact:expected:1", expected.ImageId);
        Assert.Equal("artifact://design-render-exact.png", expected.Uri);
        Assert.Equal("sha256:design-render-exact", expected.Sha256);
        Assert.Equal("design-render-exact", expected.Metadata["source_artifact_id"]);
        Assert.Equal("design-render", expected.Metadata["source_artifact_kind"]);
    }

    [Fact]
    public async Task EvaluateAsync_RejectsEchoMetadataOnlyComparisonAsVisualProof()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new EchoMetadataOnlyComparisonProvider(),
            new DefaultLocalDesignConversionVisualJudge());
        var request = CreateTwoViewportRequest(maxRenderQaRuns: 2);

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.ComparisonArtifacts.Count);
        Assert.All(result.Value.ComparisonArtifacts, artifact =>
        {
            Assert.True(artifact.Verdict.Passed);
            Assert.Equal("metadata_only_comparison", artifact.Metadata["hidden_determinism_boundary"]);
        });
        Assert.All(result.Value.VisualJudgeResults, judge =>
        {
            Assert.False(judge.Verdict.Passed);
            Assert.Equal(0.0, judge.Verdict.Score);
            Assert.Contains(judge.Verdict.Defects, defect =>
                defect.Category == "comparison_visual_comparison_not_proven" &&
                defect.Severity == "major");
        });
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ContinueImprovement,
            result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
            result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(result.Value.VisualQaBoundaryTrace.VisualDecision.QualityThresholdMet);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);
    }

    [Fact]
    public async Task EvaluateAsync_RejectsInjectedComparisonProviderForgingTrustWithSameBoundRefs()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new ForgedTrustSameRefsComparisonProvider(),
            new DefaultLocalDesignConversionVisualJudge());
        var request = CreateTwoViewportRequest(maxRenderQaRuns: 2);

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.ComparisonArtifacts.Count);
        Assert.All(result.Value.ComparisonArtifacts, artifact =>
        {
            Assert.Equal(bool.TrueString, artifact.Metadata["comparison_evidence_server_canonicalized"]);
            Assert.Equal(
                "untrusted_injected_comparison_provider_evidence",
                artifact.Metadata["comparison_evidence_trust_classification"]);
            Assert.Equal(bool.FalseString, artifact.Metadata["render_evidence_production_trusted"]);
            Assert.Equal(bool.FalseString, artifact.Metadata["approval_evidence_allowed"]);
            Assert.Equal(bool.FalseString, artifact.Metadata["visual_comparison_proven"]);
            Assert.Equal(bool.TrueString, artifact.Metadata["comparison_provider_trust_claim_overridden_by_server"]);
            Assert.Equal(bool.TrueString, artifact.Metadata["comparison_image_refs_preserved"]);
        });
        Assert.All(result.Value.VisualJudgeResults, judge =>
        {
            Assert.False(judge.Verdict.Passed);
            Assert.Equal(0.0, judge.Verdict.Score);
            Assert.Contains(judge.Verdict.Defects, defect =>
                defect.Category == "comparison_visual_comparison_not_proven" &&
                defect.Severity == "major");
        });
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionRetry,
            result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(result.Value.VisualQaBoundaryTrace.VisualDecision.QualityThresholdMet);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);
    }

    [Fact]
    public async Task EvaluateAsync_OptInBoundedRenderProviderStartsTwoViewportPreRendersConcurrentlyAndKeepsRequestOrder()
    {
        var renderProvider = new BarrierConcurrentRenderProvider(
            maxConcurrentRenderOperations: 2,
            requiredStartedCountBeforeRelease: 2);
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        using var guard = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var result = await service.EvaluateAsync(CreateTwoViewportRequest(maxRenderQaRuns: 2), guard.Token);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False((object)renderProvider is IDesignConversionBatchRenderProvider);
        Assert.Equal(2, renderProvider.MaxObservedActive);
        Assert.Equal(2, renderProvider.StartOrder.Count);
        Assert.Equal(
            ["desktop", "mobile"],
            renderProvider.StartOrder.Distinct(StringComparer.Ordinal).OrderBy(viewportId => viewportId).ToArray());
        Assert.Equal(2, renderProvider.CompletionOrder.Count);
        Assert.Equal(
            ["desktop", "mobile"],
            renderProvider.CompletionOrder.Distinct(StringComparer.Ordinal).OrderBy(viewportId => viewportId).ToArray());
        Assert.Equal(["desktop", "mobile"], result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "mobile"], comparisonProvider.Requests.Select(comparison => comparison.Actual.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "mobile"], visualJudge.Requests.Select(judge => judge.RenderedImage.Resolution.ResolutionId).ToArray());
        Assert.Equal(["design-job-001:qa-result:1", "design-job-001:qa-result:2"], result.Value.QaResults.Select(qa => qa.ResultId).ToArray());
        Assert.Empty(result.Value.ViewportFailures);
    }

    [Fact]
    public async Task EvaluateAsync_OptInBoundedRenderProviderDoesNotExceedAdvertisedConcurrency()
    {
        var renderProvider = new DelayedConcurrentRenderProvider(
            maxConcurrentRenderOperations: 2,
            delay: TimeSpan.FromMilliseconds(75));
        var service = new DesignConversionInternalService(
            renderProvider,
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true)),
            new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true)));

        var result = await service.EvaluateAsync(CreateThreeViewportRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, renderProvider.StartOrder.Count);
        Assert.Equal(
            ["desktop", "mobile", "tablet"],
            renderProvider.StartOrder.Distinct(StringComparer.Ordinal).OrderBy(viewportId => viewportId).ToArray());
        Assert.Equal(3, renderProvider.CompletionOrder.Count);
        Assert.Equal(
            ["desktop", "mobile", "tablet"],
            renderProvider.CompletionOrder.Distinct(StringComparer.Ordinal).OrderBy(viewportId => viewportId).ToArray());
        Assert.Equal(2, renderProvider.MaxObservedActive);
        Assert.All(renderProvider.ActiveSamples, sample => Assert.InRange(sample, 1, 2));
        Assert.Equal(["desktop", "mobile", "tablet"], result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Empty(result.Value.ViewportFailures);
    }

    [Fact]
    public async Task EvaluateAsync_OptInBoundedRenderProviderLazilyMaterializesOnlyActiveViewportRequests()
    {
        var request = CreateMultiViewportRequest(viewportCount: 8, maxRenderQaRuns: 8);
        var countedResolutions = new IndexerCountingRenderResolutionList(request.RenderResolutions);
        var renderProvider = new ManuallyReleasedConcurrentRenderProvider(
            maxConcurrentRenderOperations: 2,
            onMaxConcurrentRenderOperationsRead: countedResolutions.EnableCounting);
        var service = new DesignConversionInternalService(
            renderProvider,
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true)),
            new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true)));
        var countedRequest = request with { RenderResolutions = countedResolutions };
        using var guard = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var evaluateTask = service.EvaluateAsync(countedRequest, guard.Token);
        try
        {
            await renderProvider.WaitForStartedCountAsync(2, guard.Token);

            Assert.Equal(2, renderProvider.StartOrder.Count);
            Assert.InRange(countedResolutions.IndexReadCount, 0, 2);
        }
        finally
        {
            renderProvider.ReleaseAll();
        }

        var result = await evaluateTask;

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(8, renderProvider.StartOrder.Count);
        Assert.Equal(
            Enumerable.Range(1, 8).Select(index => $"viewport-{index}").ToArray(),
            result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Empty(result.Value.ViewportFailures);
    }

    [Fact]
    public async Task EvaluateAsync_OptInBoundedRenderProviderHardCapsIntMaxAdvertisement()
    {
        var renderProvider = new DelayedConcurrentRenderProvider(
            maxConcurrentRenderOperations: int.MaxValue,
            delay: TimeSpan.FromMilliseconds(75));
        var service = new DesignConversionInternalService(
            renderProvider,
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true)),
            new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true)));

        var result = await service.EvaluateAsync(CreateMultiViewportRequest(viewportCount: 8, maxRenderQaRuns: 8));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(8, renderProvider.StartOrder.Count);
        Assert.Equal(8, renderProvider.CompletionOrder.Count);
        Assert.Equal(4, renderProvider.MaxObservedActive);
        Assert.All(renderProvider.ActiveSamples, sample => Assert.InRange(sample, 1, 4));
        Assert.Equal(
            Enumerable.Range(1, 8).Select(index => $"viewport-{index}").ToArray(),
            result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Empty(result.Value.ViewportFailures);
    }

    [Fact]
    public async Task EvaluateAsync_OptInBoundedRenderProviderReadsAdvertisedConcurrencyExactlyOnce()
    {
        var renderProvider = new SingleReadConcurrentRenderProvider(
            advertisedConcurrency: 2,
            delay: TimeSpan.FromMilliseconds(40));
        var service = new DesignConversionInternalService(
            renderProvider,
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true)),
            new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true)));

        var result = await service.EvaluateAsync(CreateThreeViewportRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, renderProvider.AdvertisementReadCount);
        Assert.Equal(2, renderProvider.MaxObservedActive);
        Assert.Equal(
            ["desktop", "mobile", "tablet"],
            result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Empty(result.Value.ViewportFailures);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task EvaluateAsync_InvalidOptInConcurrentLimitFallsBackToSequentialRendering(int advertisedLimit)
    {
        var renderProvider = new DelayedConcurrentRenderProvider(
            maxConcurrentRenderOperations: advertisedLimit,
            delay: TimeSpan.FromMilliseconds(40));
        var service = new DesignConversionInternalService(
            renderProvider,
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true)),
            new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true)));

        var result = await service.EvaluateAsync(CreateThreeViewportRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, renderProvider.MaxObservedActive);
        Assert.All(renderProvider.ActiveSamples, sample => Assert.Equal(1, sample));
        Assert.Equal(["desktop", "mobile", "tablet"], renderProvider.StartOrder.ToArray());
        Assert.Equal(["desktop", "mobile", "tablet"], renderProvider.CompletionOrder.ToArray());
        Assert.Equal(["desktop", "mobile", "tablet"], result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Empty(result.Value.ViewportFailures);
    }

    [Fact]
    public async Task EvaluateAsync_OptInConcurrentCancellationDrainsStartedWorkers()
    {
        using var cancellation = new CancellationTokenSource();
        using var guard = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellation.Token, guard.Token);
        var renderProvider = new ManuallyReleasedConcurrentRenderProvider(maxConcurrentRenderOperations: 2);
        var service = new DesignConversionInternalService(
            renderProvider,
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true)),
            new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true)));

        var evaluateTask = service.EvaluateAsync(
            CreateMultiViewportRequest(viewportCount: 8, maxRenderQaRuns: 8),
            linked.Token);
        await renderProvider.WaitForStartedCountAsync(2, guard.Token);

        cancellation.Cancel();
        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => evaluateTask);
        }
        finally
        {
            renderProvider.ReleaseAll();
        }

        Assert.Equal(0, renderProvider.ActiveCount);
        Assert.Equal(2, renderProvider.StartOrder.Count);
        Assert.Equal(2, renderProvider.CompletionOrder.Count);
    }

    [Fact]
    public async Task EvaluateAsync_BoundedConcurrentRenderingCompletesAllRendersBeforeComparisonStarts()
    {
        var renderProvider = new DelayedConcurrentRenderProvider(
            maxConcurrentRenderOperations: 2,
            delay: TimeSpan.FromMilliseconds(40));
        var comparisonProvider = new RenderCompletionSnapshotComparisonProvider(
            CreateVerdict("comparison-pass", 0.94, passed: true),
            () => renderProvider.CompletionOrder.Count);
        var service = new DesignConversionInternalService(
            renderProvider,
            comparisonProvider,
            new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true)));

        var result = await service.EvaluateAsync(CreateThreeViewportRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, comparisonProvider.RenderCompletionSnapshots.Count);
        Assert.All(comparisonProvider.RenderCompletionSnapshots, snapshot => Assert.Equal(3, snapshot));
        Assert.Equal(["desktop", "mobile", "tablet"], comparisonProvider.Requests.Select(comparison => comparison.Actual.Resolution.ResolutionId).ToArray());
        Assert.Empty(result.Value!.ViewportFailures);
    }

    [Fact]
    public async Task EvaluateAsync_BatchCapableRenderProviderReceivesOneOrderedBatchAndKeepsEvidenceOrder()
    {
        var renderProvider = new RecordingBatchRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);

        var result = await service.EvaluateAsync(CreateThreeViewportRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(renderProvider.IndividualRequests);
        var batch = Assert.Single(renderProvider.BatchRequests);
        Assert.Equal(["desktop", "mobile", "tablet"], batch.Select(request => request.Resolution.ResolutionId).ToArray());
        Assert.Equal(
            ["design-job-001:render-request:1", "design-job-001:render-request:2", "design-job-001:render-request:3"],
            batch.Select(request => request.RenderRequestId).ToArray());
        Assert.Equal(["desktop", "mobile", "tablet"], result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "mobile", "tablet"], comparisonProvider.Requests.Select(comparison => comparison.Actual.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "mobile", "tablet"], visualJudge.Requests.Select(judge => judge.RenderedImage.Resolution.ResolutionId).ToArray());
        Assert.Equal(
            [
                "render-result:design-job-001:render-request:1:result",
                "render-result:design-job-001:render-request:2:result",
                "render-result:design-job-001:render-request:3:result"
            ],
            result.Value.EvidenceRefs
                .Where(evidenceRef => evidenceRef.StartsWith("render-result:", StringComparison.Ordinal))
                .ToArray());
        Assert.Empty(result.Value.ViewportFailures);
    }

    [Fact]
    public async Task EvaluateAsync_BatchRenderProviderPropagatesCallerCancellationWithoutFallback()
    {
        using var cancellation = new CancellationTokenSource();
        using var guard = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var enteredBatch = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var renderProvider = new RecordingBatchRenderProvider((requests, callerToken) =>
        {
            Assert.Equal(cancellation.Token, callerToken);
            Assert.Equal(3, requests.Count);
            enteredBatch.TrySetResult(true);
            cancellation.Cancel();
            callerToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>([]);
        });
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);

        var evaluate = service.EvaluateAsync(CreateThreeViewportRequest(), cancellation.Token);
        await enteredBatch.Task.WaitAsync(TimeSpan.FromSeconds(5), guard.Token);

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => evaluate);
        Assert.Equal(cancellation.Token, exception.CancellationToken);
        Assert.Empty(renderProvider.IndividualRequests);
        Assert.Single(renderProvider.BatchRequests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
    }

    [Fact]
    public async Task EvaluateAsync_BatchNullMiddleResultPreservesOtherViewportEvidenceAndFailsClosed()
    {
        const string privateMarker = "PRIVATE_BATCH_NULL_ITEM_PAYLOAD";
        var renderProvider = new RecordingBatchRenderProvider(requests =>
        {
            Assert.Equal(3, requests.Count);
            OperationResult<DesignConversionRenderResult>?[] batchResults =
            [
                CreateRenderSuccess(requests[0]),
                null,
                CreateRenderSuccess(requests[2])
            ];
            return Task.FromResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>(batchResults!);
        });
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var request = CreateThreeViewportRequest() with
        {
            Html = $"<main data-secret=\"{privateMarker}\">Secret</main>",
            Css = $".secret{{background:url('{privateMarker}');}}",
            DesignTreePayload = new DesignTreePayload(
                PayloadId: "private-batch-null-current",
                Format: "figma-json",
                Content: $"{{\"private\":\"{privateMarker}\"}}",
                Sha256: "sha256:private-batch-null-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload = new DesignTreePayload(
                PayloadId: "private-batch-null-original",
                Format: "figma-json",
                Content: $"{{\"private\":\"{privateMarker}\"}}",
                Sha256: "sha256:private-batch-null-original",
                Metadata: new Dictionary<string, string>())
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(renderProvider.IndividualRequests);
        Assert.Single(renderProvider.BatchRequests);
        Assert.Equal(["desktop", "tablet"], result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Equal(
            ["design-job-001:render-request:1", "design-job-001:render-request:3"],
            result.Value.RenderResults.Select(render => render.RenderRequestId).ToArray());
        Assert.Equal(["desktop", "tablet"], comparisonProvider.Requests.Select(comparison => comparison.Actual.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "tablet"], visualJudge.Requests.Select(judge => judge.RenderedImage.Resolution.ResolutionId).ToArray());
        Assert.Equal(
            [
                "render-result:design-job-001:render-request:1:result",
                "render-result:design-job-001:render-request:3:result"
            ],
            result.Value.EvidenceRefs
                .Where(evidenceRef => evidenceRef.StartsWith("render-result:", StringComparison.Ordinal))
                .ToArray());

        var failure = Assert.Single(result.Value.ViewportFailures);
        Assert.Equal("mobile", failure.ViewportId);
        Assert.Equal(2, failure.OneBasedIndex);
        Assert.Equal("render", failure.Stage);
        Assert.Equal(DesignConversionInternalService.BatchRenderResultRequiredCode, failure.ErrorCode);
        Assert.Contains("viewport_operation_failure:index=2", failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:viewport=mobile", failure.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.ViewportRenderFailedStatus, result.Value.Warnings);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed,
            result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(result.Value.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);
        Assert.False(result.Value.RuntimeFlowResult.Output.Accepted);
        Assert.False(result.Value.Verdict.Passed);

        var failureDiagnostics = string.Join(
            "\n",
            result.Value.EvidenceRefs
                .Concat(result.Value.Warnings)
                .Concat(result.Value.ViewportFailures.Select(viewportFailure => viewportFailure.ErrorCode))
                .Concat(result.Value.ViewportFailures.SelectMany(viewportFailure => viewportFailure.EvidenceRefs))
                .Concat(result.Value.RuntimeFlowResult.Output.Verdict.EvidenceRefs));
        Assert.DoesNotContain(privateMarker, failureDiagnostics, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_BatchMiddleRenderFailurePreservesOtherViewportEvidenceAndFailsClosed()
    {
        const string privateHtml = "<main data-secret=\"PRIVATE_BATCH_HTML\">Secret</main>";
        const string privateCss = ".secret{background:url('PRIVATE_BATCH_CSS');}";
        const string privateTree = "{\"private\":\"PRIVATE_BATCH_TREE\"}";
        var renderProvider = new RecordingBatchRenderProvider(requests =>
            Task.FromResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>(
                requests.Select((request, index) => index == 1
                    ? OperationResult<DesignConversionRenderResult>.Fail(
                        "raw-private-provider-payload-render PRIVATE_BATCH_HTML PRIVATE_BATCH_CSS PRIVATE_BATCH_TREE",
                        "RENDER_PROVIDER_FAILED")
                    : CreateRenderSuccess(request)).ToArray()));
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var request = CreateThreeViewportRequest() with
        {
            Html = privateHtml,
            Css = privateCss,
            DesignTreePayload = new DesignTreePayload(
                PayloadId: "private-batch-current",
                Format: "figma-json",
                Content: privateTree,
                Sha256: "sha256:private-batch-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload = new DesignTreePayload(
                PayloadId: "private-batch-original",
                Format: "figma-json",
                Content: privateTree,
                Sha256: "sha256:private-batch-original",
                Metadata: new Dictionary<string, string>())
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(renderProvider.IndividualRequests);
        Assert.Single(renderProvider.BatchRequests);
        Assert.Equal(["desktop", "tablet"], result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "tablet"], comparisonProvider.Requests.Select(comparison => comparison.Actual.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "tablet"], visualJudge.Requests.Select(judge => judge.RenderedImage.Resolution.ResolutionId).ToArray());

        var failure = Assert.Single(result.Value.ViewportFailures);
        Assert.Equal("mobile", failure.ViewportId);
        Assert.Equal(2, failure.OneBasedIndex);
        Assert.Equal("render", failure.Stage);
        Assert.Equal("RENDER_PROVIDER_FAILED", failure.ErrorCode);
        Assert.Contains("viewport_operation_failure:index=2", failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:viewport=mobile", failure.EvidenceRefs);
        Assert.Contains("render-result:design-job-001:render-request:1:result", result.Value.EvidenceRefs);
        Assert.Contains("render-result:design-job-001:render-request:3:result", result.Value.EvidenceRefs);
        Assert.DoesNotContain("render-result:design-job-001:render-request:2:result", result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.ViewportRenderFailedStatus, result.Value.Warnings);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed,
            result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(result.Value.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);

        var failureDiagnostics = string.Join(
            "\n",
            result.Value.EvidenceRefs
                .Concat(result.Value.ViewportFailures.Select(viewportFailure => viewportFailure.ErrorCode))
                .Concat(result.Value.ViewportFailures.SelectMany(viewportFailure => viewportFailure.EvidenceRefs))
                .Concat(result.Value.RuntimeFlowResult.Output.Verdict.EvidenceRefs));
        Assert.DoesNotContain("raw-private-provider-payload", failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("PRIVATE_BATCH_HTML", failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("PRIVATE_BATCH_CSS", failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("PRIVATE_BATCH_TREE", failureDiagnostics, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("null", DesignConversionInternalService.BatchRenderResultCountInvalidCode)]
    [InlineData("short", DesignConversionInternalService.BatchRenderResultCountInvalidCode)]
    [InlineData("throws", DesignConversionInternalService.BatchRenderProviderExceptionCode)]
    public async Task EvaluateAsync_MalformedOrThrowingBatchResultFailsClosedWithoutLeakingPrivatePayload(
        string mode,
        string expectedErrorCode)
    {
        const string privateMarker = "PRIVATE_BATCH_PROVIDER_PAYLOAD";
        var renderProvider = new RecordingBatchRenderProvider(requests => mode switch
        {
            "null" => Task.FromResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>(null!),
            "short" => Task.FromResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>(
                requests.Take(2).Select(CreateRenderSuccess).ToArray()),
            "throws" => throw new InvalidOperationException(
                $"raw-private-provider-payload-render {privateMarker}"),
            _ => throw new InvalidOperationException("Unsupported batch test mode.")
        });
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var request = CreateThreeViewportRequest() with
        {
            Html = $"<main data-private=\"{privateMarker}\">Secret</main>",
            Css = $".private{{background:url('{privateMarker}');}}",
            DesignTreePayload = new DesignTreePayload(
                PayloadId: "private-batch-malformed-current",
                Format: "figma-json",
                Content: $"{{\"private\":\"{privateMarker}\"}}",
                Sha256: "sha256:private-batch-malformed-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload = new DesignTreePayload(
                PayloadId: "private-batch-malformed-original",
                Format: "figma-json",
                Content: $"{{\"private\":\"{privateMarker}\"}}",
                Sha256: "sha256:private-batch-malformed-original",
                Metadata: new Dictionary<string, string>())
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(renderProvider.IndividualRequests);
        Assert.Single(renderProvider.BatchRequests);
        Assert.Empty(result.Value!.RenderResults);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Equal(["desktop", "mobile", "tablet"], result.Value.ViewportFailures.Select(failure => failure.ViewportId).ToArray());
        Assert.All(result.Value.ViewportFailures, failure =>
        {
            Assert.Equal("render", failure.Stage);
            Assert.Equal(expectedErrorCode, failure.ErrorCode);
            Assert.Contains(DesignConversionInternalService.ViewportOperationFailureMarker, failure.EvidenceRefs);
        });
        Assert.Contains(DesignConversionInternalService.ViewportRenderFailedStatus, result.Value.Warnings);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);

        var failureDiagnostics = string.Join(
            "\n",
            result.Value.EvidenceRefs
                .Concat(result.Value.ViewportFailures.Select(viewportFailure => viewportFailure.ErrorCode))
                .Concat(result.Value.ViewportFailures.SelectMany(viewportFailure => viewportFailure.EvidenceRefs))
                .Concat(result.Value.RuntimeFlowResult.Output.Verdict.EvidenceRefs));
        Assert.DoesNotContain("raw-private-provider-payload", failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain(privateMarker, failureDiagnostics, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_NonOptInRenderProviderRetainsSequentialViewportRendering()
    {
        var renderProvider = new NonOptInSequentialGuardRenderProvider(delay: TimeSpan.FromMilliseconds(40));
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);

        var result = await service.EvaluateAsync(CreateTwoViewportRequest(maxRenderQaRuns: 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.False((object)renderProvider is IDesignConversionBatchRenderProvider);
        Assert.Equal(1, renderProvider.MaxObservedActive);
        Assert.Equal(["desktop", "mobile"], renderProvider.StartOrder.ToArray());
        Assert.Equal(["desktop", "mobile"], renderProvider.CompletionOrder.ToArray());
        Assert.Equal(["desktop", "mobile"], result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "mobile"], comparisonProvider.Requests.Select(comparison => comparison.Actual.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "mobile"], visualJudge.Requests.Select(judge => judge.RenderedImage.Resolution.ResolutionId).ToArray());
        Assert.Empty(result.Value.ViewportFailures);
    }

    [Fact]
    public async Task EvaluateAsync_OptInConcurrentRenderFailureKeepsFailureAtOriginalIndexAndOutputOrder()
    {
        var renderProvider = new ReorderingFailureConcurrentRenderProvider(maxConcurrentRenderOperations: 2);
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);

        var result = await service.EvaluateAsync(CreateThreeViewportRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, renderProvider.StartOrder.Count);
        Assert.Equal(
            ["desktop", "mobile", "tablet"],
            renderProvider.StartOrder.Distinct(StringComparer.Ordinal).OrderBy(viewportId => viewportId).ToArray());
        Assert.Equal(3, renderProvider.CompletionOrder.Count);
        Assert.Equal(
            ["desktop", "mobile", "tablet"],
            renderProvider.CompletionOrder.Distinct(StringComparer.Ordinal).OrderBy(viewportId => viewportId).ToArray());
        Assert.Equal(2, renderProvider.MaxObservedActive);
        Assert.Equal(["desktop", "tablet"], result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "tablet"], comparisonProvider.Requests.Select(comparison => comparison.Actual.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "tablet"], visualJudge.Requests.Select(judge => judge.RenderedImage.Resolution.ResolutionId).ToArray());
        Assert.Equal(["design-job-001:qa-result:1", "design-job-001:qa-result:3"], result.Value.QaResults.Select(qa => qa.ResultId).ToArray());

        var failure = Assert.Single(result.Value.ViewportFailures);
        Assert.Equal("mobile", failure.ViewportId);
        Assert.Equal(2, failure.OneBasedIndex);
        Assert.Equal("render", failure.Stage);
        Assert.Equal("RENDER_PROVIDER_FAILED", failure.ErrorCode);
        Assert.Contains("viewport_operation_failure:index=2", failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:viewport=mobile", failure.EvidenceRefs);
        Assert.Contains("render-result:design-job-001:render-request:1:result", result.Value.EvidenceRefs);
        Assert.Contains("render-result:design-job-001:render-request:3:result", result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.ViewportRenderFailedStatus, result.Value.Warnings);
    }

    [Theory]
    [InlineData("render", "RENDER_PROVIDER_FAILED", DesignConversionInternalService.ViewportRenderFailedStatus, 2, 2, 2)]
    [InlineData("comparison", "COMPARISON_PROVIDER_FAILED", DesignConversionInternalService.ViewportComparisonFailedStatus, 3, 2, 2)]
    [InlineData("visual_judge", "VISUAL_JUDGE_PROVIDER_FAILED", DesignConversionInternalService.ViewportVisualJudgeFailedStatus, 3, 3, 2)]
    public async Task EvaluateAsync_RecordsViewportFailureAndContinuesToLaterViewports(
        string failureStage,
        string expectedErrorCode,
        string expectedStatus,
        int expectedRenderCount,
        int expectedComparisonCount,
        int expectedQaCount)
    {
        var renderProvider = new ViewportFailureRenderProvider(
            failAtRequestNumber: failureStage == "render" ? 2 : null);
        var comparisonProvider = new ViewportFailureComparisonProvider(
            CreateVerdict("comparison-pass", 0.94, passed: true),
            failAtRequestNumber: failureStage == "comparison" ? 2 : null);
        var visualJudge = new ViewportFailureVisualJudge(
            CreateVerdict("visual-pass", 0.95, passed: true),
            failAtRequestNumber: failureStage == "visual_judge" ? 2 : null);
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);

        var result = await service.EvaluateAsync(CreateThreeViewportRequest());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, renderProvider.Requests.Count);
        Assert.Equal("tablet", renderProvider.Requests[2].Resolution.ResolutionId);
        Assert.Equal(failureStage == "render" ? 2 : 3, comparisonProvider.Requests.Count);
        Assert.Equal(failureStage is "render" or "comparison" ? 2 : 3, visualJudge.Requests.Count);
        Assert.Contains(result.Value!.RenderResults, render => render.Image.Resolution.ResolutionId == "desktop");
        Assert.Contains(result.Value.RenderResults, render => render.Image.Resolution.ResolutionId == "tablet");
        Assert.Equal(expectedRenderCount, result.Value.RenderResults.Count);
        Assert.Equal(expectedComparisonCount, result.Value.ComparisonArtifacts.Count);
        Assert.Equal(2, result.Value.VisualJudgeResults.Count);
        Assert.Equal(expectedQaCount, result.Value.QaResults.Count);

        var failure = Assert.Single(result.Value.ViewportFailures);
        Assert.Equal("mobile", failure.ViewportId);
        Assert.Equal(390, failure.Width);
        Assert.Equal(844, failure.Height);
        Assert.Equal(3.0, failure.DeviceScaleFactor);
        Assert.Equal(2, failure.OneBasedIndex);
        Assert.Equal(failureStage, failure.Stage);
        Assert.Equal(expectedErrorCode, failure.ErrorCode);
        Assert.All(failure.EvidenceRefs, evidenceRef =>
        {
            Assert.DoesNotContain("artifact", evidenceRef, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("sha", evidenceRef, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("diff", evidenceRef, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("raw-private-provider-payload", evidenceRef, StringComparison.Ordinal);
        });

        Assert.DoesNotContain(
            result.Value.VisualJudgeResults,
            judge => string.Equals(judge.Status, expectedStatus, StringComparison.Ordinal));

        Assert.DoesNotContain(
            result.Value.ComparisonArtifacts,
            artifact => artifact.ArtifactId.Contains("viewport-failure", StringComparison.Ordinal));
        Assert.DoesNotContain(
            result.Value.QaResults,
            qa => qa.ResultId.Contains("viewport-failure", StringComparison.Ordinal));
        Assert.Contains(DesignConversionInternalService.ViewportOperationFailureMarker, result.Value.EvidenceRefs);
        Assert.Contains($"viewport_operation_failure:stage={failureStage}", result.Value.EvidenceRefs);
        Assert.Contains($"viewport_operation_failure:error_code={expectedErrorCode}", result.Value.EvidenceRefs);
        Assert.Contains("render-result:design-job-001:render-request:1:result", result.Value.EvidenceRefs);
        Assert.Contains("render-result:design-job-001:render-request:3:result", result.Value.EvidenceRefs);
        Assert.Contains(expectedStatus, result.Value.Warnings);
        Assert.False(result.Value.Verdict.Passed);
        Assert.False(result.Value.RuntimeFlowResult.Output.Accepted);
        Assert.NotEqual(
            DesignConversionVisualQaBoundaryPolicy.DecisionAccepted,
            result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);

        var publicFailureText = string.Join(
            " ",
            result.Value.EvidenceRefs
                .Concat(result.Value.Warnings)
                .Concat(result.Value.ViewportFailures.Select(viewportFailure => viewportFailure.ErrorCode))
                .Concat(result.Value.ViewportFailures.SelectMany(viewportFailure => viewportFailure.EvidenceRefs))
                .Concat(result.Value.VisualJudgeResults.Select(judge => judge.Status))
                .Concat(result.Value.VisualJudgeResults.SelectMany(judge => judge.Verdict.Defects.Select(defect => defect.Description)))
                .Concat(result.Value.RuntimeFlowResult.Output.Verdict.Defects.Select(defect => defect.Description)));
        Assert.DoesNotContain("raw-private-provider-payload", publicFailureText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EvaluateAsync_MiddleRenderFailureAttemptsLaterViewportAndReturnsPartialFailClosedEvidence()
    {
        const string privateHtml = "<main data-secret=\"raw-html-secret\">Secret</main>";
        const string privateCss = ".secret-token{background:url('C:/private/design.png');}";
        const string privateTree = "{\"secret\":\"raw-tree-secret\"}";
        var renderProvider = new ViewportFailureRenderProvider(failAtRequestNumber: 2);
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var baseRequest = CreateThreeViewportRequest();
        var request = baseRequest with
        {
            Html = privateHtml,
            Css = privateCss,
            DesignTreePayload = baseRequest.DesignTreePayload with { Content = privateTree },
            Budget = baseRequest.Budget with
            {
                Metadata = new Dictionary<string, string>
                {
                    ["private_budget_token"] = "budget-secret-token"
                }
            }
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(["desktop", "mobile", "tablet"], renderProvider.Requests.Select(render => render.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "tablet"], result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "tablet"], comparisonProvider.Requests.Select(comparison => comparison.Actual.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "tablet"], visualJudge.Requests.Select(judge => judge.RenderedImage.Resolution.ResolutionId).ToArray());
        Assert.Equal(["desktop", "mobile", "tablet"], result.Value.VisualQaBoundaryTrace.MultiResolutionRender.RequiredViewportIds);
        Assert.Equal(["desktop", "tablet"], result.Value.VisualQaBoundaryTrace.MultiResolutionRender.ObservedViewportIds);
        Assert.Equal(["mobile"], result.Value.VisualQaBoundaryTrace.MultiResolutionRender.MissingViewportIds);
        Assert.False(result.Value.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);

        var failure = Assert.Single(result.Value.ViewportFailures);
        Assert.Equal("mobile", failure.ViewportId);
        Assert.Equal("render", failure.Stage);
        Assert.Equal("RENDER_PROVIDER_FAILED", failure.ErrorCode);
        Assert.Contains(DesignConversionInternalService.ViewportOperationFailureMarker, failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:stage=render", failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:index=2", failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:error_code=RENDER_PROVIDER_FAILED", failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:viewport=mobile", failure.EvidenceRefs);

        Assert.Contains("render-result:design-job-001:render-request:1:result", result.Value.EvidenceRefs);
        Assert.Contains("render-result:design-job-001:render-request:3:result", result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.ViewportRenderFailedStatus, result.Value.Warnings);
        Assert.Contains("VIEWPORT_OPERATION_FAILED", result.Value.Warnings);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed,
            result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.False(result.Value.VisualQaBoundaryTrace.TierPolicy.ShouldContinue);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(result.Value.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);
        Assert.False(result.Value.RuntimeFlowResult.Output.Accepted);
        Assert.False(result.Value.Verdict.Passed);

        var failureDiagnostics = string.Join(
            " ",
            result.Value.EvidenceRefs
                .Concat(result.Value.Warnings)
                .Concat(result.Value.ViewportFailures.Select(viewportFailure => viewportFailure.ErrorCode))
                .Concat(result.Value.ViewportFailures.SelectMany(viewportFailure => viewportFailure.EvidenceRefs))
                .Concat(result.Value.RuntimeFlowResult.Output.Verdict.EvidenceRefs));
        Assert.DoesNotContain("raw-private-provider-payload", failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain(privateHtml, failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCss, failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain(privateTree, failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("budget-secret-token", failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/private", failureDiagnostics, StringComparison.OrdinalIgnoreCase);
        Assert.All(failure.EvidenceRefs, evidenceRef =>
        {
            Assert.DoesNotContain("artifact://", evidenceRef, StringComparison.Ordinal);
            Assert.DoesNotContain("sha256:", evidenceRef, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Theory]
    [InlineData(1, "desktop", 1, "mobile,tablet", "2,3")]
    [InlineData(3, "tablet", 3, "desktop,mobile", "1,2")]
    public async Task EvaluateAsync_BoundaryRenderFailurePreservesOtherViewportEvidenceAndFailsClosed(
        int failAtRequestNumber,
        string failedViewportId,
        int failedOneBasedIndex,
        string expectedSuccessfulViewportIdsCsv,
        string expectedSuccessfulRequestNumbersCsv)
    {
        const string privateHtml = "<main data-secret=\"boundary-raw-html-secret\">Secret</main>";
        const string privateCss = ".boundary-secret-token{background:url('C:/private/boundary-design.png');}";
        const string privateTree = "{\"secret\":\"boundary-raw-tree-secret\"}";
        var renderProvider = new ViewportFailureRenderProvider(failAtRequestNumber);
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var baseRequest = CreateThreeViewportRequest();
        var request = baseRequest with
        {
            Html = privateHtml,
            Css = privateCss,
            DesignTreePayload = baseRequest.DesignTreePayload with { Content = privateTree },
            Budget = baseRequest.Budget with
            {
                Metadata = new Dictionary<string, string>
                {
                    ["private_budget_token"] = "boundary-budget-secret-token"
                }
            }
        };

        var expectedSuccessfulViewportIds = expectedSuccessfulViewportIdsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var expectedSuccessfulRequestNumbers = expectedSuccessfulRequestNumbersCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(["desktop", "mobile", "tablet"], renderProvider.Requests.Select(render => render.Resolution.ResolutionId).ToArray());
        Assert.Equal(expectedSuccessfulViewportIds, result.Value!.RenderResults.Select(render => render.Image.Resolution.ResolutionId).ToArray());
        Assert.Equal(expectedSuccessfulViewportIds, comparisonProvider.Requests.Select(comparison => comparison.Actual.Resolution.ResolutionId).ToArray());
        Assert.Equal(expectedSuccessfulViewportIds, visualJudge.Requests.Select(judge => judge.RenderedImage.Resolution.ResolutionId).ToArray());
        Assert.Equal(
            expectedSuccessfulRequestNumbers.Select(number => $"design-job-001:qa-result:{number}").ToArray(),
            result.Value.QaResults.Select(qa => qa.ResultId).ToArray());
        Assert.Equal(["desktop", "mobile", "tablet"], result.Value.VisualQaBoundaryTrace.MultiResolutionRender.RequiredViewportIds);
        Assert.Equal(expectedSuccessfulViewportIds, result.Value.VisualQaBoundaryTrace.MultiResolutionRender.ObservedViewportIds);
        Assert.Equal([failedViewportId], result.Value.VisualQaBoundaryTrace.MultiResolutionRender.MissingViewportIds);
        Assert.False(result.Value.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);

        var failure = Assert.Single(result.Value.ViewportFailures);
        Assert.Equal(failedViewportId, failure.ViewportId);
        Assert.Equal(failedOneBasedIndex, failure.OneBasedIndex);
        Assert.Equal("render", failure.Stage);
        Assert.Equal("RENDER_PROVIDER_FAILED", failure.ErrorCode);
        Assert.Contains(DesignConversionInternalService.ViewportOperationFailureMarker, failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:stage=render", failure.EvidenceRefs);
        Assert.Contains($"viewport_operation_failure:index={failedOneBasedIndex}", failure.EvidenceRefs);
        Assert.Contains("viewport_operation_failure:error_code=RENDER_PROVIDER_FAILED", failure.EvidenceRefs);
        Assert.Contains($"viewport_operation_failure:viewport={failedViewportId}", failure.EvidenceRefs);

        foreach (var requestNumber in expectedSuccessfulRequestNumbers)
            Assert.Contains($"render-result:design-job-001:render-request:{requestNumber}:result", result.Value.EvidenceRefs);
        Assert.DoesNotContain($"render-result:design-job-001:render-request:{failedOneBasedIndex}:result", result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.ViewportRenderFailedStatus, result.Value.Warnings);
        Assert.Contains("VIEWPORT_OPERATION_FAILED", result.Value.Warnings);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed,
            result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.False(result.Value.VisualQaBoundaryTrace.TierPolicy.ShouldContinue);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionBlocked,
            result.Value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(result.Value.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);
        Assert.False(result.Value.RuntimeFlowResult.Output.Accepted);
        Assert.False(result.Value.Verdict.Passed);

        var failureDiagnostics = string.Join(
            " ",
            result.Value.EvidenceRefs
                .Concat(result.Value.Warnings)
                .Concat(result.Value.ViewportFailures.Select(viewportFailure => viewportFailure.ErrorCode))
                .Concat(result.Value.ViewportFailures.SelectMany(viewportFailure => viewportFailure.EvidenceRefs))
                .Concat(result.Value.RuntimeFlowResult.Output.Verdict.EvidenceRefs));
        Assert.DoesNotContain("raw-private-provider-payload", failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain(privateHtml, failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain(privateCss, failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain(privateTree, failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("boundary-budget-secret-token", failureDiagnostics, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/private", failureDiagnostics, StringComparison.OrdinalIgnoreCase);
        Assert.All(failure.EvidenceRefs, evidenceRef =>
        {
            Assert.DoesNotContain("artifact://", evidenceRef, StringComparison.Ordinal);
            Assert.DoesNotContain("sha256:", evidenceRef, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task EvaluateAsync_PropagatesCallerCancellationDuringViewportOperation()
    {
        using var cancellation = new CancellationTokenSource();
        var service = new DesignConversionInternalService(
            new CallerCancelingRenderProvider(cancellation),
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true)));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.EvaluateAsync(CreateThreeViewportRequest(), cancellation.Token));
    }

    [Fact]
    public async Task EvaluateAsync_MissingExpectedRenderFailsValidationBeforeViewportProvidersRun()
    {
        const string expectedCode = "EXPECTED_RENDER_REQUIRED";
        const string expectedMessage =
            "DesignConversion expected design render or screenshot is required.";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.92, passed: true));
        var service = new DesignConversionInternalService(
            renderProvider,
            comparisonProvider,
            visualJudge);
        var request = CreateTwoViewportRequest(maxRenderQaRuns: 1) with
        {
            Screenshots = [],
            DesignRenders = []
        };

        var result = await service.EvaluateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
    }

    [Fact]
    public async Task EvaluateAsync_TwoRequiredViewportsWithSingleRunBudgetReturnsIncompleteCoverageAndStaysNonReady()
    {
        const string privateMarker = "PRIVATE_RESPONSIVE_RENDER_QA_BUDGET_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.92, passed: true));
        var service = new DesignConversionInternalService(
            renderProvider,
            comparisonProvider,
            visualJudge);
        var request = CreateTwoViewportRequest(maxRenderQaRuns: 1) with
        {
            Html = $"<main data-private=\"{privateMarker}\">Secret</main>",
            Css = $".private-responsive-budget{{background:url('{privateMarker}');}}",
            DesignTreePayload = new DesignTreePayload(
                PayloadId: "tree-private-responsive-budget-current",
                Format: "figma-json",
                Content: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"current\"}}",
                Sha256: "sha256:private-responsive-budget-current",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            OriginalTreePayload = new DesignTreePayload(
                PayloadId: "tree-private-responsive-budget-original",
                Format: "figma-json",
                Content: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\"}}",
                Sha256: "sha256:private-responsive-budget-original",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            Budget = new DesignConversionBudgetMetadata(
                UserId: $"user-{privateMarker}",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 1,
                BudgetUsd: 1.50m,
                Metadata: new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                })
        };

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var value = result.Value!;
        Assert.Equal("desktop", Assert.Single(renderProvider.Requests).Resolution.ResolutionId);
        Assert.Equal("desktop", Assert.Single(comparisonProvider.Requests).Actual.Resolution.ResolutionId);
        Assert.Equal("desktop", Assert.Single(visualJudge.Requests).RenderedImage.Resolution.ResolutionId);
        Assert.Single(value.RenderResults);
        Assert.Single(value.ComparisonArtifacts);
        Assert.Single(value.VisualJudgeResults);
        Assert.Single(value.QaResults);
        Assert.Empty(value.ViewportFailures);

        var coverage = value.VisualQaBoundaryTrace.MultiResolutionRender;
        Assert.True(coverage.IsMultiResolution);
        Assert.Equal(["desktop", "mobile"], coverage.RequiredViewportIds);
        Assert.Equal(["desktop"], coverage.ObservedViewportIds);
        Assert.Equal(["mobile"], coverage.MissingViewportIds);
        Assert.False(coverage.IsComplete);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
            value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.False(value.VisualQaBoundaryTrace.TierPolicy.ShouldContinue);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine,
            value.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.False(value.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate);
        Assert.False(value.VisualQaBoundaryTrace.ProductReady);
        Assert.Contains(
            $"tier_policy:stop_reason={DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete}",
            value.EvidenceRefs);
        Assert.Contains("MULTI_RESOLUTION_RENDER_EVIDENCE_INCOMPLETE", value.Warnings);

        var publicEvidenceText = string.Join(
            " ",
            value.EvidenceRefs
                .Concat(value.Warnings)
                .Concat(coverage.EvidenceRefs)
                .Concat(value.VisualQaBoundaryTrace.TierPolicy.EvidenceRefs)
                .Concat(value.VisualQaBoundaryTrace.TierPolicy.Metadata.SelectMany(pair => new[] { pair.Key, pair.Value }))
                .Concat(value.VisualQaBoundaryTrace.VisualDecision.EvidenceRefs)
                .Concat(value.VisualQaBoundaryTrace.VisualDecision.Metadata.SelectMany(pair => new[] { pair.Key, pair.Value }))
                .Concat(value.RuntimeFlowResult.Output.Verdict.EvidenceRefs));
        Assert.DoesNotContain(privateMarker, publicEvidenceText, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Html, publicEvidenceText, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Css, publicEvidenceText, StringComparison.Ordinal);
        Assert.DoesNotContain(request.DesignTreePayload.Content, publicEvidenceText, StringComparison.Ordinal);
        Assert.DoesNotContain(request.OriginalTreePayload.Content, publicEvidenceText, StringComparison.Ordinal);
        Assert.DoesNotContain(request.Budget.UserId, publicEvidenceText, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact://", publicEvidenceText, StringComparison.Ordinal);
        Assert.DoesNotContain("sha256:", publicEvidenceText, StringComparison.OrdinalIgnoreCase);
    }

    private static readonly byte[] ExternalVisualJudgePngBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=");

    private static readonly string ExternalVisualJudgePngSha256 =
        Convert.ToHexString(SHA256.HashData(ExternalVisualJudgePngBytes)).ToLowerInvariant();

    private static DesignConversionInternalEvaluateRequest CreateRequest()
        => new(
            JobId: "design-job-001",
            RequestId: "design-request-internal-001",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"card\">Hello</main>",
            Css: ".card{display:grid;}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: "tree-current",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: "tree-original",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\",\"version\":\"original\"}",
                Sha256: "sha256:tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots:
            [
                new DesignRenderArtifact(
                    ArtifactId: "source-shot-001",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://source-shot.png",
                    Sha256: "sha256:source-shot",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders:
            [
                new DesignRenderArtifact(
                    ArtifactId: "design-render-001",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://design-render.png",
                    Sha256: "sha256:design-render",
                    Metadata: new Dictionary<string, string>())
            ],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "internal",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>()),
            RenderResolutions: [new RenderQaResolution("desktop", 1440, 900, 1.0)],
            AiTeacherEvidenceRefs: ["teacher:none"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["fixture"] = "internal-service-test"
            },
            Provenance: new Dictionary<string, string>
            {
                ["source"] = "unit-test"
            });

    private static DesignConversionInternalEvaluateRequest WithRawFieldContent(
        DesignConversionInternalEvaluateRequest request,
        string field,
        string content)
        => field switch
        {
            "Html" => request with { Html = content },
            "Css" => request with { Css = content },
            "DesignTreePayload.Content" => request with
            {
                DesignTreePayload = request.DesignTreePayload with { Content = content }
            },
            "OriginalTreePayload.Content" => request with
            {
                OriginalTreePayload = request.OriginalTreePayload with { Content = content }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unknown raw field.")
        };

    private static string CreateRawPayload(int length, string marker)
    {
        if (marker.Length > length)
            throw new ArgumentOutOfRangeException(nameof(length), length, "Length must fit the marker.");

        return marker + new string('x', length - marker.Length);
    }

    private static DesignConversionInternalEvaluateRequest CreateTwoViewportRequest(int maxRenderQaRuns)
        => CreateRequest() with
        {
            Budget = new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: maxRenderQaRuns,
                BudgetUsd: 1.50m,
                Metadata: new Dictionary<string, string>()),
            RenderResolutions =
            [
                new RenderQaResolution("desktop", 1440, 900, 1.0),
                new RenderQaResolution("mobile", 390, 844, 3.0)
            ],
            Screenshots =
            [
                new DesignRenderArtifact(
                    ArtifactId: "source-shot-desktop",
                    Kind: "screenshot",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://source-shot-desktop.png",
                    Sha256: "sha256:source-shot-desktop",
                    Metadata: new Dictionary<string, string>()),
                new DesignRenderArtifact(
                    ArtifactId: "source-shot-mobile",
                    Kind: "screenshot",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://source-shot-mobile.png",
                    Sha256: "sha256:source-shot-mobile",
                    Metadata: new Dictionary<string, string>())
            ],
            DesignRenders =
            [
                new DesignRenderArtifact(
                    ArtifactId: "design-render-desktop",
                    Kind: "design-render",
                    Width: 1440,
                    Height: 900,
                    Uri: "artifact://design-render-desktop.png",
                    Sha256: "sha256:design-render-desktop",
                    Metadata: new Dictionary<string, string>()),
                new DesignRenderArtifact(
                    ArtifactId: "design-render-mobile",
                    Kind: "design-render",
                    Width: 390,
                    Height: 844,
                    Uri: "artifact://design-render-mobile.png",
                    Sha256: "sha256:design-render-mobile",
                    Metadata: new Dictionary<string, string>())
            ]
        };

    private static DesignConversionInternalEvaluateRequest WithExternalVisualJudgePngRefs(
        DesignConversionInternalEvaluateRequest request)
        => request with
        {
            Screenshots = request.Screenshots
                .Select(artifact => artifact with { Sha256 = $"sha256:{ExternalVisualJudgePngSha256}" })
                .ToArray(),
            DesignRenders = request.DesignRenders
                .Select(artifact => artifact with { Sha256 = $"sha256:{ExternalVisualJudgePngSha256}" })
                .ToArray()
        };

    private static void AssertChargeableExternalComparisonCost(DesignConversionQaComparisonArtifact artifact)
    {
        var metadata = artifact.Metadata;
        Assert.Equal(
            DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification,
            metadata[DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey]);
        Assert.Equal(bool.TrueString, metadata[DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey]);
        Assert.Equal(bool.TrueString, metadata["server_owned_external_visual_ai_receipt_validated"]);
        Assert.Equal(bool.TrueString, metadata["server_owned_external_visual_ai_comparison_trust_verified"]);
        Assert.Equal(bool.TrueString, metadata["visual_comparison_proven"]);
        Assert.Equal(bool.TrueString, metadata["network_call_executed"]);
        Assert.Equal(bool.TrueString, metadata["real_external_provider_executed"]);
        Assert.Equal(bool.TrueString, metadata["external_ai_visual_judge_executed"]);
        Assert.Equal("0.0125", metadata["estimated_cost_usd"]);
        Assert.Contains(DesignConversionExternalVisualAiReceiptValidator.ValidatedEvidenceRef, artifact.EvidenceRefs);
        AssertServerOwnedCostMetadata(metadata);
    }

    private static void AssertChargeableExternalJudgeCost(DesignConversionVisualJudgeResult result)
    {
        Assert.Equal(ExternalHttpDesignConversionVisualJudge.StatusOk, result.Status);
        Assert.False(result.IsBlocked);
        Assert.True(result.Provider.ExternalCallExecuted);
        Assert.Equal(0.0125m, result.EstimatedCostUsd);
        Assert.Equal(bool.TrueString, result.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.TrueString, result.Metadata["provider_response_schema_verified"]);
        Assert.Equal(bool.TrueString, result.Provider.Metadata["provider_response_identity_verified"]);
        Assert.Equal(bool.TrueString, result.Provider.Metadata["provider_response_schema_verified"]);
        AssertServerOwnedCostMetadata(result.Metadata);
        AssertServerOwnedCostMetadata(result.Provider.Metadata);
    }

    private static void AssertServerOwnedCostMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        Assert.Equal(
            DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest,
            metadata["cost_source"]);
        Assert.Equal(bool.TrueString, metadata["server_owned_pricing_configured"]);
        Assert.Equal(bool.TrueString, metadata["server_owned_pricing_valid"]);
        Assert.Equal("configured", metadata["server_owned_pricing_status"]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceValue,
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountSourceKey]);
        Assert.Equal("1", metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedAttemptCountKey]);
        Assert.Equal("0.0125", metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostPerAttemptUsdKey]);
        Assert.Equal("0.0125", metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedTotalCostUsdKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingFixedPerAttempt,
            metadata[ExternalHttpDesignConversionVisualJudge.ServerOwnedCostAccountingKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey]);
        Assert.Equal(
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion,
            metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallAttemptedKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.NetworkCallExecutionVerifiedKey]);
        Assert.Equal(bool.TrueString, metadata[ExternalHttpDesignConversionVisualJudge.ProviderResponseReceivedKey]);
    }

    private static DesignConversionInternalEvaluateRequest CreateThreeViewportRequest()
    {
        var viewports = new[]
        {
            new RenderQaResolution("desktop", 1440, 900, 1.0),
            new RenderQaResolution("mobile", 390, 844, 3.0),
            new RenderQaResolution("tablet", 768, 1024, 2.0)
        };

        return CreateRequest() with
        {
            Budget = new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 5,
                MaxRenderQaRuns: 3,
                BudgetUsd: 1.50m,
                Metadata: new Dictionary<string, string>()),
            RenderResolutions = viewports,
            Screenshots = viewports.Select(viewport => new DesignRenderArtifact(
                ArtifactId: $"source-shot-{viewport.ResolutionId}",
                Kind: "screenshot",
                Width: viewport.Width,
                Height: viewport.Height,
                Uri: $"artifact://source-shot-{viewport.ResolutionId}.png",
                Sha256: $"sha256:source-shot-{viewport.ResolutionId}",
                Metadata: new Dictionary<string, string>())).ToArray(),
            DesignRenders = viewports.Select(viewport => new DesignRenderArtifact(
                ArtifactId: $"design-render-{viewport.ResolutionId}",
                Kind: "design-render",
                Width: viewport.Width,
                Height: viewport.Height,
                Uri: $"artifact://design-render-{viewport.ResolutionId}.png",
                Sha256: $"sha256:design-render-{viewport.ResolutionId}",
                Metadata: new Dictionary<string, string>())).ToArray()
        };
    }

    private static DesignConversionInternalEvaluateRequest CreateMultiViewportRequest(
        int viewportCount,
        int maxRenderQaRuns)
    {
        var viewports = Enumerable.Range(1, viewportCount)
            .Select(index => new RenderQaResolution(
                ResolutionId: $"viewport-{index}",
                Width: 320 + index,
                Height: 480 + index,
                DeviceScaleFactor: 1.0))
            .ToArray();

        return CreateRequest() with
        {
            Budget = new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 5,
                MaxRenderQaRuns: maxRenderQaRuns,
                BudgetUsd: 1.50m,
                Metadata: new Dictionary<string, string>()),
            RenderResolutions = viewports,
            Screenshots = viewports.Select(viewport => new DesignRenderArtifact(
                ArtifactId: $"source-shot-{viewport.ResolutionId}",
                Kind: "screenshot",
                Width: viewport.Width,
                Height: viewport.Height,
                Uri: $"artifact://source-shot-{viewport.ResolutionId}.png",
                Sha256: $"sha256:source-shot-{viewport.ResolutionId}",
                Metadata: new Dictionary<string, string>())).ToArray(),
            DesignRenders = viewports.Select(viewport => new DesignRenderArtifact(
                ArtifactId: $"design-render-{viewport.ResolutionId}",
                Kind: "design-render",
                Width: viewport.Width,
                Height: viewport.Height,
                Uri: $"artifact://design-render-{viewport.ResolutionId}.png",
                Sha256: $"sha256:design-render-{viewport.ResolutionId}",
                Metadata: new Dictionary<string, string>())).ToArray()
        };
    }

    private static RenderQaImageRecord CreateImage(string id, string label)
        => new(
            ImageId: id,
            Label: label,
            Resolution: new RenderQaResolution("desktop", 1440, 900, 1.0),
            Uri: $"artifact://{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static VisualVerdict CreateVerdict(string verdictId, double score, bool passed)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects: passed
                ? []
                :
                [
                    new VisualDefect(
                        DefectId: $"{verdictId}:spacing",
                        Category: "spacing",
                        Severity: "major",
                        Description: "Spacing differs from expected render.",
                        X: 16,
                        Y: 24,
                        Width: 80,
                        Height: 20,
                        EvidenceImageIds: ["expected", "actual"])
                ],
            EvidenceRefs: [$"verdict:{verdictId}"],
            Passed: passed);

    private static OperationResult<DesignConversionRenderResult> CreateRenderSuccess(
        DesignConversionRenderRequest request)
    {
        var image = new RenderQaImageRecord(
            ImageId: $"{request.RenderRequestId}:actual",
            Label: "actual-render",
            Resolution: request.Resolution,
            Uri: $"artifact://actual-render-{request.Resolution.ResolutionId}.png",
            Sha256: $"sha256:actual-render-{request.Resolution.ResolutionId}",
            Metadata: request.Metadata);

        return OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
            RenderResultId: $"{request.RenderRequestId}:result",
            RenderRequestId: request.RenderRequestId,
            Image: image,
            ElapsedMs: 11,
            Warnings: ["fake-render-provider"]));
    }

    private static async Task WaitForTaskWithTimeoutAsync(
        Task task,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var timeoutTask = Task.Delay(timeout, timeoutSource.Token);
        var completed = await Task.WhenAny(task, timeoutTask);
        if (!ReferenceEquals(completed, task))
            throw new TimeoutException("Timed out while waiting for the concurrent render test barrier.");

        timeoutSource.Cancel();
        await task;
    }

    private static void RecordMax(ref int target, int candidate)
    {
        while (true)
        {
            var current = Volatile.Read(ref target);
            if (candidate <= current)
                return;

            if (Interlocked.CompareExchange(ref target, candidate, current) == current)
                return;
        }
    }

    private sealed class RecordingBatchRenderProvider :
        IDesignConversionRenderProvider,
        IDesignConversionBatchRenderProvider,
        IDesignConversionBoundedConcurrentRenderProvider
    {
        private readonly Func<IReadOnlyList<DesignConversionRenderRequest>, CancellationToken, Task<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>> _renderBatchAsync;

        public RecordingBatchRenderProvider(
            Func<IReadOnlyList<DesignConversionRenderRequest>, Task<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>>? renderBatchAsync = null,
            int maxConcurrentRenderOperations = 2)
            : this(
                renderBatchAsync is null
                    ? DefaultRenderBatchAsync
                    : (requests, _) => renderBatchAsync(requests),
                maxConcurrentRenderOperations)
        {
        }

        public RecordingBatchRenderProvider(
            Func<IReadOnlyList<DesignConversionRenderRequest>, CancellationToken, Task<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>> renderBatchAsync,
            int maxConcurrentRenderOperations = 2)
        {
            _renderBatchAsync = renderBatchAsync;
            MaxConcurrentRenderOperations = maxConcurrentRenderOperations;
        }

        public int MaxConcurrentRenderOperations { get; }

        public List<IReadOnlyList<DesignConversionRenderRequest>> BatchRequests { get; } = [];

        public List<DesignConversionRenderRequest> IndividualRequests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            IndividualRequests.Add(request);
            throw new InvalidOperationException("Individual RenderAsync must not be called for a batch-capable provider.");
        }

        public async Task<IReadOnlyList<OperationResult<DesignConversionRenderResult>>> RenderBatchAsync(
            IReadOnlyList<DesignConversionRenderRequest> requests,
            CancellationToken cancellationToken = default)
        {
            BatchRequests.Add(requests.ToArray());
            return await _renderBatchAsync(requests, cancellationToken);
        }

        private static Task<IReadOnlyList<OperationResult<DesignConversionRenderResult>>> DefaultRenderBatchAsync(
            IReadOnlyList<DesignConversionRenderRequest> requests,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OperationResult<DesignConversionRenderResult>>>(
                requests.Select(CreateRenderSuccess).ToArray());
    }

    private abstract class ConcurrentRenderProviderBase(int maxConcurrentRenderOperations)
        : IDesignConversionRenderProvider,
            IDesignConversionBoundedConcurrentRenderProvider
    {
        private int _active;
        private int _maxObservedActive;

        public virtual int MaxConcurrentRenderOperations { get; } = maxConcurrentRenderOperations;

        public ConcurrentQueue<string> StartOrder { get; } = new();

        public ConcurrentQueue<string> CompletionOrder { get; } = new();

        public ConcurrentQueue<int> ActiveSamples { get; } = new();

        public int MaxObservedActive => Volatile.Read(ref _maxObservedActive);

        public int ActiveCount => Volatile.Read(ref _active);

        public async Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            StartOrder.Enqueue(request.Resolution.ResolutionId);
            var active = Interlocked.Increment(ref _active);
            ActiveSamples.Enqueue(active);
            RecordMax(ref _maxObservedActive, active);
            try
            {
                return await RenderInsideActiveWindowAsync(request, cancellationToken);
            }
            finally
            {
                CompletionOrder.Enqueue(request.Resolution.ResolutionId);
                Interlocked.Decrement(ref _active);
            }
        }

        protected abstract Task<OperationResult<DesignConversionRenderResult>> RenderInsideActiveWindowAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken);
    }

    private sealed class SingleReadConcurrentRenderProvider(
        int advertisedConcurrency,
        TimeSpan delay)
        : ConcurrentRenderProviderBase(advertisedConcurrency)
    {
        private int _advertisementReadCount;

        public int AdvertisementReadCount => Volatile.Read(ref _advertisementReadCount);

        public override int MaxConcurrentRenderOperations
        {
            get
            {
                var readCount = Interlocked.Increment(ref _advertisementReadCount);
                if (readCount != 1)
                {
                    throw new InvalidOperationException(
                        "Advertised render concurrency must be snapshotted exactly once.");
                }

                return base.MaxConcurrentRenderOperations;
            }
        }

        protected override async Task<OperationResult<DesignConversionRenderResult>> RenderInsideActiveWindowAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            return CreateRenderSuccess(request);
        }
    }

    private sealed class ManuallyReleasedConcurrentRenderProvider(
        int maxConcurrentRenderOperations,
        Action? onMaxConcurrentRenderOperationsRead = null)
        : ConcurrentRenderProviderBase(maxConcurrentRenderOperations)
    {
        private readonly TaskCompletionSource<bool> _release =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly object _startedLock = new();
        private readonly SortedDictionary<int, TaskCompletionSource<bool>> _startedWaiters = [];
        private int _started;

        public override int MaxConcurrentRenderOperations
        {
            get
            {
                onMaxConcurrentRenderOperationsRead?.Invoke();
                return base.MaxConcurrentRenderOperations;
            }
        }

        public async Task WaitForStartedCountAsync(int requiredStartedCount, CancellationToken cancellationToken)
        {
            Task waitTask;
            lock (_startedLock)
            {
                if (_started >= requiredStartedCount)
                    return;

                if (!_startedWaiters.TryGetValue(requiredStartedCount, out var waiter))
                {
                    waiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    _startedWaiters.Add(requiredStartedCount, waiter);
                }

                waitTask = waiter.Task;
            }

            await WaitForTaskWithTimeoutAsync(waitTask, TimeSpan.FromSeconds(3), cancellationToken);
        }

        public void ReleaseAll()
            => _release.TrySetResult(true);

        protected override async Task<OperationResult<DesignConversionRenderResult>> RenderInsideActiveWindowAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken)
        {
            var started = Interlocked.Increment(ref _started);
            lock (_startedLock)
            {
                foreach (var pair in _startedWaiters.Where(pair => pair.Key <= started).ToArray())
                {
                    pair.Value.TrySetResult(true);
                    _startedWaiters.Remove(pair.Key);
                }
            }

            await WaitForTaskWithTimeoutAsync(
                _release.Task,
                TimeSpan.FromSeconds(3),
                cancellationToken);
            return CreateRenderSuccess(request);
        }
    }

    private sealed class BarrierConcurrentRenderProvider(
        int maxConcurrentRenderOperations,
        int requiredStartedCountBeforeRelease)
        : ConcurrentRenderProviderBase(maxConcurrentRenderOperations)
    {
        private readonly TaskCompletionSource<bool> _requiredStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _started;

        protected override async Task<OperationResult<DesignConversionRenderResult>> RenderInsideActiveWindowAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _started) >= requiredStartedCountBeforeRelease)
                _requiredStarted.TrySetResult(true);

            await WaitForTaskWithTimeoutAsync(
                _requiredStarted.Task,
                TimeSpan.FromSeconds(2),
                cancellationToken);
            return CreateRenderSuccess(request);
        }
    }

    private sealed class DelayedConcurrentRenderProvider(
        int maxConcurrentRenderOperations,
        TimeSpan delay)
        : ConcurrentRenderProviderBase(maxConcurrentRenderOperations)
    {
        protected override async Task<OperationResult<DesignConversionRenderResult>> RenderInsideActiveWindowAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(delay, cancellationToken);
            return CreateRenderSuccess(request);
        }
    }

    private sealed class ReorderingFailureConcurrentRenderProvider(int maxConcurrentRenderOperations)
        : ConcurrentRenderProviderBase(maxConcurrentRenderOperations)
    {
        protected override async Task<OperationResult<DesignConversionRenderResult>> RenderInsideActiveWindowAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken)
        {
            var delay = request.Resolution.ResolutionId switch
            {
                "desktop" => TimeSpan.FromMilliseconds(140),
                "mobile" => TimeSpan.FromMilliseconds(10),
                _ => TimeSpan.FromMilliseconds(20)
            };
            await Task.Delay(delay, cancellationToken);

            if (string.Equals(request.Resolution.ResolutionId, "mobile", StringComparison.Ordinal))
            {
                return OperationResult<DesignConversionRenderResult>.Fail(
                    "raw-private-provider-payload-render",
                    "RENDER_PROVIDER_FAILED");
            }

            return CreateRenderSuccess(request);
        }
    }

    private sealed class NonOptInSequentialGuardRenderProvider(TimeSpan delay)
        : IDesignConversionRenderProvider
    {
        private int _active;
        private int _maxObservedActive;

        public ConcurrentQueue<string> StartOrder { get; } = new();

        public ConcurrentQueue<string> CompletionOrder { get; } = new();

        public int MaxObservedActive => Volatile.Read(ref _maxObservedActive);

        public async Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            StartOrder.Enqueue(request.Resolution.ResolutionId);
            var active = Interlocked.Increment(ref _active);
            RecordMax(ref _maxObservedActive, active);
            try
            {
                await Task.Delay(delay, cancellationToken);
                if (active > 1)
                {
                    return OperationResult<DesignConversionRenderResult>.Fail(
                        "Non-opt-in render provider was invoked concurrently.",
                        "NON_OPT_IN_CONCURRENCY_VIOLATION");
                }

                return CreateRenderSuccess(request);
            }
            finally
            {
                CompletionOrder.Enqueue(request.Resolution.ResolutionId);
                Interlocked.Decrement(ref _active);
            }
        }
    }

    private sealed class IndexerCountingRenderResolutionList(IReadOnlyList<RenderQaResolution> inner)
        : IReadOnlyList<RenderQaResolution>
    {
        private int _countingEnabled;
        private int _indexReadCount;

        public int Count => inner.Count;

        public int IndexReadCount => Volatile.Read(ref _indexReadCount);

        public void EnableCounting()
            => Volatile.Write(ref _countingEnabled, 1);

        public RenderQaResolution this[int index]
        {
            get
            {
                if (Volatile.Read(ref _countingEnabled) == 1)
                    Interlocked.Increment(ref _indexReadCount);

                return inner[index];
            }
        }

        public IEnumerator<RenderQaResolution> GetEnumerator()
            => inner.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    private sealed class RenderCompletionSnapshotComparisonProvider(
        VisualVerdict verdict,
        Func<int> readRenderCompletionCount)
        : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public List<int> RenderCompletionSnapshots { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            RenderCompletionSnapshots.Add(readRenderCompletionCount());
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://diff-{request.Actual.Resolution.ResolutionId}.png",
                DiffImageSha256: $"sha256:diff-{request.Actual.Resolution.ResolutionId}",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["comparison:fake-provider"])));
        }
    }

    private sealed class ExternalCostAccountingRenderProvider : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: "artifact://actual-render.png",
                Sha256: $"sha256:{ExternalVisualJudgePngSha256}",
                Metadata: new Dictionary<string, string>
                {
                    ["png_byte_length"] = ExternalVisualJudgePngBytes.Length.ToString()
                });

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 11,
                Warnings: ["external-cost-accounting-render-provider"])));
        }
    }

    private sealed class ConstantVisualJudgeImageContentResolver : IDesignConversionVisualJudgeImageContentResolver
    {
        public Task<OperationResult<DesignConversionVisualJudgeImageContent>> ResolveAsync(
            RenderQaImageRecord image,
            string role,
            long maxBytes,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionVisualJudgeImageContent>.Ok(
                new DesignConversionVisualJudgeImageContent(
                    MediaType: "image/png",
                    Sha256: ExternalVisualJudgePngSha256,
                    Bytes: ExternalVisualJudgePngBytes)));
    }

    private sealed class SuccessfulExternalVisualJudgeHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                      "judge_request_id": "design-job-001:comparison-request:1",
                      "request_id": "design-request-internal-001",
                      "provider": "openai",
                      "model_id": "gpt-visual-boundary",
                      "score": 0.70,
                      "passed": false,
                      "tokens": { "total": 7 },
                      "defects": []
                    }
                    """, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class RecordingMatchingExternalVisualJudgeHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            var body = request.Content is null
                ? "{}"
                : await request.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            var judgeRequestId = root.GetProperty("judge_request_id").GetString() ?? string.Empty;
            var requestId = root.GetProperty("request_id").GetString() ?? string.Empty;
            var provider = root.GetProperty("provider").GetString() ?? "openai";
            var model = root.GetProperty("model").GetString() ?? "gpt-visual-boundary";
            var response = new
            {
                judge_request_id = judgeRequestId,
                request_id = requestId,
                provider,
                model_id = model,
                score = 0.70,
                passed = false,
                evidence_refs = new[] { $"recording-external-visual-judge-call:{CallCount}" },
                tokens = new { total = 7 },
                defects = Array.Empty<object>()
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class RecordingRenderProvider : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: "artifact://actual-render.png",
                Sha256: "sha256:actual-render",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 11,
                Warnings: ["fake-render-provider"])));
        }
    }

    private sealed class ViewportFailureRenderProvider(int? failAtRequestNumber) : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (Requests.Count == failAtRequestNumber)
            {
                return Task.FromResult(OperationResult<DesignConversionRenderResult>.Fail(
                    "raw-private-provider-payload-render",
                    "RENDER_PROVIDER_FAILED"));
            }

            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://actual-render-{request.Resolution.ResolutionId}.png",
                Sha256: $"sha256:actual-render-{request.Resolution.ResolutionId}",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 11,
                Warnings: ["fake-render-provider"])));
        }
    }

    private sealed class CallerCancelingRenderProvider(CancellationTokenSource cancellation)
        : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellation.Cancel();
            throw new OperationCanceledException(cancellation.Token);
        }
    }

    private sealed class RecordingBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        public List<DesignConversionBrowserCaptureRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);

            var render = await new DeterministicLocalDesignConversionPngRenderProvider(Path.Combine(
                    Path.GetTempPath(),
                    "design-conversion-internal-service-browser-fixtures",
                    Guid.NewGuid().ToString("N")))
                .RenderAsync(new DesignConversionRenderRequest(
                    RenderRequestId: $"{request.CaptureRequestId}:local-png-proof",
                    RequestId: request.RequestId,
                    Html: request.DocumentHtml,
                    Css: "body{margin:0;}",
                    Resolution: request.Resolution,
                    Metadata: request.Metadata), cancellationToken);

            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: render.Value!.Image.Uri,
                    ScreenshotSha256: render.Value.Image.Sha256,
                    ElapsedMs: 7,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "pass166r-recording-local-png-browser-adapter",
                        ["browser_process_boundary"] = "recording_adapter_local_png_fixture_no_real_browser",
                        ["actual_png_bytes_written"] = bool.TrueString
                    },
                    Warnings: ["recording-browser-adapter-local-png-fixture"]));
        }
    }

    private sealed class EchoMetadataOnlyComparisonProvider : IDesignConversionComparisonProvider
    {
        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:echo-metadata-only-verdict",
                Score: 0.99,
                Defects: [],
                EvidenceRefs: ["comparison:echo-metadata-only"],
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:echo-metadata-only-artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Expected,
                Verdict: verdict,
                DiffImageUri: "artifact://echo-metadata-only-diff.json",
                DiffImageSha256: "sha256:echo-metadata-only-diff",
                Metrics: new Dictionary<string, double>
                {
                    ["visual_distance"] = 0.0,
                    ["metadata_only_score"] = 1.0
                },
                EvidenceRefs:
                [
                    "comparison:echo-metadata-only",
                    DeterministicDesignConversionComparisonProvider.BoundaryMarker
                ])
            {
                Metadata = new Dictionary<string, string>
                {
                    ["provider_kind"] = "echo_metadata_only_test_provider",
                    ["hidden_determinism_boundary"] = "metadata_only_comparison",
                    ["product_ready"] = bool.FalseString
                }
            }));
        }
    }

    private sealed class ForgedTrustSameRefsComparisonProvider : IDesignConversionComparisonProvider
    {
        private const string ForgedDiffSha =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:forged-trust-verdict",
                Score: 0.99,
                Defects: [],
                EvidenceRefs:
                [
                    "comparison:forged-trust",
                    ServerOwnedLocalPngComparisonProvider.ProviderKind,
                    ServerOwnedLocalPngComparisonProvider.Boundary
                ],
                Passed: true);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:forged-trust-artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"local-png-ref:sha256:{ForgedDiffSha}",
                    DiffImageSha256: $"sha256:{ForgedDiffSha}",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = 0.0,
                        ["visual_comparison_proven"] = 1.0
                    },
                    EvidenceRefs:
                    [
                        "comparison:forged-trust",
                        ServerOwnedLocalPngComparisonProvider.ProviderKind,
                        ServerOwnedLocalPngComparisonProvider.Boundary
                    ])
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["provider_kind"] = ServerOwnedLocalPngComparisonProvider.ProviderKind,
                        ["comparison_provider_kind"] = ServerOwnedLocalPngComparisonProvider.ProviderKind,
                        ["comparison_boundary"] = ServerOwnedLocalPngComparisonProvider.Boundary,
                        ["hidden_determinism_boundary"] = ServerOwnedLocalPngComparisonProvider.Boundary,
                        ["local_png_pixel_comparison"] = bool.TrueString,
                        ["visual_comparison_proven"] = bool.TrueString,
                        ["expected_sha256_verified"] = bool.TrueString,
                        ["rendered_sha256_verified"] = bool.TrueString,
                        ["diff_sha256_verified"] = bool.TrueString,
                        ["diff_png_signature_verified"] = bool.TrueString,
                        ["diff_png_nonblank_pixels_verified"] = bool.TrueString,
                        ["comparison_diff_readback_sha256_verified"] = bool.TrueString,
                        ["diff_image_sha256"] = $"sha256:{ForgedDiffSha}",
                        ["render_evidence_production_trusted"] = bool.TrueString,
                        ["approval_evidence_allowed"] = bool.TrueString,
                        ["product_ready"] = bool.TrueString
                    }
                }));
        }
    }

    private sealed class RecordingComparisonProvider(VisualVerdict verdict) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: "artifact://diff.png",
                DiffImageSha256: "sha256:diff",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["comparison:fake-provider"])));
        }
    }

    private sealed class ViewportFailureComparisonProvider(
        VisualVerdict verdict,
        int? failAtRequestNumber) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (Requests.Count == failAtRequestNumber)
            {
                return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Fail(
                    "raw-private-provider-payload-comparison",
                    "COMPARISON_PROVIDER_FAILED"));
            }

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://diff-{request.Actual.Resolution.ResolutionId}.png",
                DiffImageSha256: $"sha256:diff-{request.Actual.Resolution.ResolutionId}",
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 1.0 - verdict.Score },
                EvidenceRefs: ["comparison:fake-provider"])));
        }
    }

    private sealed class RecordingVisualJudge(VisualVerdict verdict) : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:fake-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: "FAKE_VISUAL_JUDGE_VERDICT_RECEIVED",
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: "fake-visual-judge",
                    EndpointHost: "none",
                    ModelId: "fake-model",
                    ExternalCallExecuted: false,
                    Metadata: new Dictionary<string, string>
                    {
                        ["test_double"] = bool.TrueString
                    }),
                EstimatedCostUsd: 0m,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: ["visual-judge:fake-provider", .. verdict.EvidenceRefs],
                Metadata: new Dictionary<string, string>
                {
                    ["external_ai_visual_judge_executed"] = bool.FalseString
                })));
        }
    }

    private sealed class ViewportFailureVisualJudge(
        VisualVerdict verdict,
        int? failAtRequestNumber) : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (Requests.Count == failAtRequestNumber)
            {
                return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Fail(
                    "raw-private-provider-payload-visual-judge",
                    "VISUAL_JUDGE_PROVIDER_FAILED"));
            }

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:fake-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: "FAKE_VISUAL_JUDGE_VERDICT_RECEIVED",
                IsBlocked: false,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: "fake-visual-judge",
                    EndpointHost: "none",
                    ModelId: "fake-model",
                    ExternalCallExecuted: false,
                    Metadata: new Dictionary<string, string>
                    {
                        ["test_double"] = bool.TrueString
                    }),
                EstimatedCostUsd: 0m,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: ["visual-judge:fake-provider", .. verdict.EvidenceRefs],
                Metadata: new Dictionary<string, string>
                {
                    ["external_ai_visual_judge_executed"] = bool.FalseString
                })));
        }
    }

    private sealed class RecordingCriticUnit(
        List<string> order,
        VisualVerdict? verdict = null) : IDesignConversionCriticUnit
    {
        public DesignConversionCriticUnitInput? Input { get; private set; }

        public Task<OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionCriticUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            order.Add("critic");
            Input = request.Input;
            var outputVerdict = verdict ?? request.Input.ComparisonArtifacts[0].Verdict;
            var output = new DesignConversionCriticUnitOutput(
                OutputId: "recording-critic-output",
                Verdict: outputVerdict,
                QaEvidenceRefs: ["recording-critic-qa-evidence"],
                AiTeacherEvidenceRefs: request.Input.AiTeacherEvidenceRefs,
                DeterministicLogicClaimedAsLearning: false);

            var result = new LogicalUnitResult<DesignConversionCriticUnitOutput>(
                UnitId: "recording-critic",
                UnitRole: "recording",
                DomainTag: DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
                Output: output,
                Confidence: outputVerdict.Score,
                PredicateIds: [],
                DomainTags: [DesignConversionRuntimeScaffoldCriticUnit.DomainTag],
                EvidenceIds: ["recording-critic-evidence"],
                Warnings: []);

            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Ok(result));
        }
    }

    private sealed class RecordingImproverUnit(
        List<string> order,
        string? html = null,
        string? css = null) : IDesignConversionImproverUnit
    {
        public Task<OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionImproverUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            order.Add("improver");
            var output = new DesignConversionImprovementOutput(
                OutputId: "recording-improver-output",
                CandidateId: request.Input.Candidate.CandidateId,
                Html: html ?? request.Input.Candidate.Html,
                Css: css ?? request.Input.Candidate.Css,
                Verdict: request.Input.CriticVerdict,
                QaResults: [],
                EvidenceRefs: request.Input.Candidate.EvidenceRefs,
                Accepted: request.Input.CriticVerdict.Passed);
            var unitOutput = new DesignConversionImproverUnitOutput(
                OutputId: output.OutputId,
                Candidate: request.Input.Candidate,
                Output: output,
                QaEvidenceRefs: request.Input.QaEvidenceRefs,
                AiTeacherEvidenceRefs: request.Input.AiTeacherEvidenceRefs,
                DeterministicLogicClaimedAsLearning: false);

            var result = new LogicalUnitResult<DesignConversionImproverUnitOutput>(
                UnitId: "recording-improver",
                UnitRole: "recording",
                DomainTag: DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
                Output: unitOutput,
                Confidence: request.Input.CriticVerdict.Score,
                PredicateIds: [],
                DomainTags: [DesignConversionRuntimeScaffoldCriticUnit.DomainTag],
                EvidenceIds: ["recording-improver-evidence"],
                Warnings: []);

            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Ok(result));
        }
    }
}
