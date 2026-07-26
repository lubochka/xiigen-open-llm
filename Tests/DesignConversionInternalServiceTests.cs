// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Collections.Concurrent;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionInternalServiceTests
{
    [Fact]
    public async Task EvaluateAsync_MapsInternalRequestToSubmittedRequestAndProviderEvidence()
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
        Assert.Single(result.Value.VisualJudgeResults);
        Assert.Single(result.Value.QaResults);
        Assert.False(result.Value.VisualJudgeResults[0].IsBlocked);
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.StatusOk, result.Value.VisualJudgeResults[0].Status);
        Assert.False(result.Value.VisualJudgeResults[0].Provider.ExternalCallExecuted);
        Assert.False(result.Value.VisualJudgeResults[0].Verdict.Passed);
        Assert.Equal(0.0, result.Value.VisualJudgeResults[0].Verdict.Score);
        Assert.Equal(
            "untrusted_injected_comparison_provider_evidence",
            result.Value.ComparisonArtifacts[0].Metadata["comparison_evidence_trust_classification"]);
        Assert.Equal(bool.FalseString, result.Value.ComparisonArtifacts[0].Metadata["render_evidence_production_trusted"]);
        Assert.Equal(bool.FalseString, result.Value.ComparisonArtifacts[0].Metadata["visual_comparison_proven"]);
        Assert.Contains(DesignConversionInternalService.InjectedRendererMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.InjectedComparisonMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionInternalService.InjectedVisualJudgeMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(NoConfiguredVisualJudge.Status, result.Value.Warnings);
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
        Assert.Equal(bool.TrueString, result.Value.ScaffoldMarkers["local_visual_judge_default_wired"]);
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
            new EchoMetadataOnlyComparisonProvider());
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
            new ForgedTrustSameRefsComparisonProvider());
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
    public async Task EvaluateAsync_NonOptInRenderProviderRetainsSequentialViewportRendering()
    {
        var renderProvider = new NonOptInSequentialGuardRenderProvider(delay: TimeSpan.FromMilliseconds(40));
        var comparisonProvider = new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.94, passed: true));
        var visualJudge = new RecordingVisualJudge(CreateVerdict("visual-pass", 0.95, passed: true));
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);

        var result = await service.EvaluateAsync(CreateTwoViewportRequest(maxRenderQaRuns: 2));

        Assert.True(result.IsSuccess, result.ErrorMessage);
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
    public async Task EvaluateAsync_RecordsIncompleteMultiResolutionAndLocalVisualJudgeTrace()
    {
        var service = new DesignConversionInternalService(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(CreateVerdict("comparison-pass", 0.91, passed: true)));
        var request = CreateTwoViewportRequest(maxRenderQaRuns: 1);

        var result = await service.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(result.Value!.RenderResults);
        Assert.False(result.Value.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete);
        Assert.True(result.Value.VisualQaBoundaryTrace.MultiResolutionRender.IsMultiResolution);
        Assert.Equal(["mobile"], result.Value.VisualQaBoundaryTrace.MultiResolutionRender.MissingViewportIds);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete,
            result.Value.VisualQaBoundaryTrace.TierPolicy.StopReason);
        Assert.False(result.Value.VisualQaBoundaryTrace.TierPolicy.ShouldContinue);
        Assert.Equal(DefaultLocalDesignConversionVisualJudge.StatusOk, result.Value.VisualQaBoundaryTrace.VisualJudgeStatus);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.MultiResolutionRenderMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker, result.Value.EvidenceRefs);
        Assert.DoesNotContain(DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderRequiredMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.TierPolicyMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ImprovementLoopMarker, result.Value.EvidenceRefs);
        Assert.Contains(DesignConversionVisualQaBoundaryPolicy.ProductNotReadyMarker, result.Value.EvidenceRefs);
        Assert.False(result.Value.VisualQaBoundaryTrace.ProductReady);
        Assert.False(result.Value.VisualQaBoundaryTrace.TrainedFeatureClaimed);
    }

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
