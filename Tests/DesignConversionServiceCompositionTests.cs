// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionServiceCompositionTests
{
    [Fact]
    public async Task CreateBrowserBackedLocal_EvaluatesThroughBrowserProviderAndDefaultLocalVisualJudge()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96, passed: true);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            browserAdapter,
            comparisonProvider);

        var result = await composition.EvaluateAsync(CreateRequest("pass132-browser-composition"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(bool.TrueString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal(bool.TrueString, composition.Descriptor.BoundaryMetadata["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal("explicit_local_composition_only", composition.Descriptor.BoundaryMetadata["browser_renderer_default_scope"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["public_production_default_ready"]);
        Assert.Equal(bool.TrueString, result.Value!.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_default_production_wired"]);
        Assert.Equal("explicit_local_injection_only_not_production_default", result.Value.BoundaryMetadata["service_scaffold_browser_renderer_default_scope"]);
        Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_provider_kind"]);
        Assert.DoesNotContain("service_scaffold_browser_renderer_default_wired", result.Value.BoundaryMetadata.Keys);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["local_visual_judge_default_wired"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["local_visual_judge_observed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["render_result_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["comparison_artifact_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["visual_judge_result_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_required_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["multi_viewport_observed_count"]);
        Assert.Equal("0", result.Value.BoundaryMetadata["multi_viewport_missing_count"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal("mobile,desktop", result.Value.BoundaryMetadata["multi_viewport_required_ids"]);
        Assert.Equal("mobile,desktop", result.Value.BoundaryMetadata["multi_viewport_observed_ids"]);
        Assert.Equal(string.Empty, result.Value.BoundaryMetadata["multi_viewport_missing_ids"]);
        Assert.Equal(2, browserAdapter.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.All(result.Value.Evaluation.RenderResults, render =>
        {
            Assert.Equal(bool.TrueString, render.Image.Metadata["browser_or_playwright_used"]);
            Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, render.Image.Metadata["provider_kind"]);
        });
        Assert.False(result.Value.Evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted);
        Assert.Equal(
            DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
            result.Value.Evaluation.VisualQaBoundaryTrace.VisualJudgeProviderBoundary);
        Assert.Contains(
            "browser_renderer_default_wired:True",
            result.Value.EvidenceRefs);
        Assert.Contains(
            "browser_renderer_explicit_local_injection_proven:True",
            result.Value.EvidenceRefs);
        Assert.Contains(
            "browser_renderer_default_production_wired:False",
            result.Value.EvidenceRefs);
        Assert.Contains(
            "service_scaffold_browser_renderer_explicit_local_injection_proven:True",
            result.Value.EvidenceRefs);
        Assert.Contains(
            "external_ai_visual_judge_executed:False",
            result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task CreateBrowserBackedLocalDefault_EvaluatesThroughBrowserProviderAsLocalDefaultWithoutProductionClaim()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.97, passed: true);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalDefault(
            browserAdapter,
            comparisonProvider);

        var result = await composition.EvaluateAsync(CreateRequest("pass132-browser-default-composition"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionKind,
            composition.Descriptor.CompositionKind);
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionKind,
            result.Value!.Descriptor.CompositionKind);
        Assert.Contains(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionMarker,
            composition.Descriptor.EvidenceRefs);
        Assert.Contains(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionMarker,
            result.Value!.EvidenceRefs);
        Assert.Equal(bool.TrueString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal(bool.TrueString, composition.Descriptor.BoundaryMetadata["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultScope,
            composition.Descriptor.BoundaryMetadata["browser_renderer_default_scope"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["public_production_default_ready"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["product_ready"]);

        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultScope,
            result.Value.BoundaryMetadata["browser_renderer_default_scope"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_default_production_wired"]);
        Assert.Equal("explicit_local_injection_only_not_production_default", result.Value.BoundaryMetadata["service_scaffold_browser_renderer_default_scope"]);
        Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_provider_kind"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["render_result_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["comparison_artifact_count"]);
        Assert.Equal("2", result.Value.BoundaryMetadata["visual_judge_result_count"]);

        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["public_production_default_ready"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["socket_opened"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["network_call_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["real_external_provider_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["provider_payload_included"]);

        Assert.Equal(2, browserAdapter.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Collection(
            browserAdapter.Requests,
            request => Assert.Equal("mobile", request.Resolution.ResolutionId),
            request => Assert.Equal("desktop", request.Resolution.ResolutionId));
        Assert.Collection(
            result.Value.Evaluation.RenderResults,
            render =>
            {
                Assert.Equal("mobile", render.Image.Resolution.ResolutionId);
                Assert.Equal(bool.TrueString, render.Image.Metadata["browser_or_playwright_used"]);
                Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, render.Image.Metadata["provider_kind"]);
            },
            render =>
            {
                Assert.Equal("desktop", render.Image.Resolution.ResolutionId);
                Assert.Equal(bool.TrueString, render.Image.Metadata["browser_or_playwright_used"]);
                Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, render.Image.Metadata["provider_kind"]);
            });
        Assert.False(result.Value.Evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted);
        Assert.False(result.Value.Evaluation.VisualQaBoundaryTrace.ProductReady);
        Assert.Contains(
            $"browser_renderer_default_scope:{DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultScope}",
            result.Value.EvidenceRefs);
        Assert.Contains(
            "browser_renderer_default_production_wired:False",
            result.Value.EvidenceRefs);
    }

    [Fact]
    public async Task CreateBrowserBackedLocalDefault_PassesSubmittedHtmlCssAndViewportDsfToBrowserAdapter()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.98, passed: true);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocalDefault(
            browserAdapter,
            comparisonProvider);
        var request = CreateRequest("n6-browser-input-flow") with
        {
            Html = "<html><body><main class=\"n6-card\">N6 submitted html</main></body></html>",
            Css = ".n6-card{display:grid;grid-template-columns:1fr 2fr;color:#0f172a;}",
            Screenshots =
            [
                Expected("n6-browser-desktop-shot", 1024, 768),
                Expected("n6-browser-retina-shot", 320, 240)
            ],
            DesignRenders =
            [
                Expected("n6-browser-desktop-render", 1024, 768),
                Expected("n6-browser-retina-render", 320, 240)
            ],
            RenderResolutions =
            [
                new RenderQaResolution("n6-desktop", 1024, 768, 1.0),
                new RenderQaResolution("n6-retina", 320, 240, 2.0)
            ],
            Budget = CreateRequest("n6-browser-input-flow-budget").Budget with
            {
                MaxRenderQaRuns = 2
            }
        };

        var result = await composition.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, browserAdapter.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Collection(
            browserAdapter.Requests,
            first =>
            {
                Assert.Contains("N6 submitted html", first.DocumentHtml, StringComparison.Ordinal);
                Assert.Contains(".n6-card{display:grid;grid-template-columns:1fr 2fr;color:#0f172a;}", first.DocumentHtml, StringComparison.Ordinal);
                Assert.Contains(LocalChromiumBrowserProcessAdapter.StrictOfflineDocumentCspContent, first.DocumentHtml, StringComparison.Ordinal);
                Assert.Equal("n6-desktop", first.Resolution.ResolutionId);
                Assert.Equal(1024, first.Resolution.Width);
                Assert.Equal(768, first.Resolution.Height);
                Assert.Equal(1.0, first.Resolution.DeviceScaleFactor);
            },
            second =>
            {
                Assert.Contains("N6 submitted html", second.DocumentHtml, StringComparison.Ordinal);
                Assert.Contains(".n6-card{display:grid;grid-template-columns:1fr 2fr;color:#0f172a;}", second.DocumentHtml, StringComparison.Ordinal);
                Assert.Contains(LocalChromiumBrowserProcessAdapter.StrictOfflineDocumentCspContent, second.DocumentHtml, StringComparison.Ordinal);
                Assert.Equal("n6-retina", second.Resolution.ResolutionId);
                Assert.Equal(320, second.Resolution.Width);
                Assert.Equal(240, second.Resolution.Height);
                Assert.Equal(2.0, second.Resolution.DeviceScaleFactor);
            });
        Assert.Collection(
            result.Value!.Evaluation.RenderResults,
            render =>
            {
                Assert.Equal("n6-desktop", render.Image.Resolution.ResolutionId);
                Assert.Equal("1024", render.Image.Metadata["browser_screenshot_png_width"]);
                Assert.Equal("768", render.Image.Metadata["browser_screenshot_png_height"]);
            },
            render =>
            {
                Assert.Equal("n6-retina", render.Image.Resolution.ResolutionId);
                Assert.Equal("640", render.Image.Metadata["browser_screenshot_png_width"]);
                Assert.Equal("480", render.Image.Metadata["browser_screenshot_png_height"]);
            });
    }

    [Fact]
    public async Task CreateBrowserBackedMachineLocalDefault_EvaluatesThroughBrowserDefaultAndMachineBackedRuntimeFlow()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var comparisonProvider = new RecordingComparisonProvider(score: 0.62, passed: false);
        var machine = await TrainMinimalMachineForLocalCheckpointContributionAsync();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedMachineLocalDefault(
            browserAdapter,
            machine,
            comparisonProvider);

        var result = await composition.EvaluateAsync(CreateRequest("n15-route4a-browser-machine-default-composition"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionKind,
            composition.Descriptor.CompositionKind);
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionKind,
            result.Value!.Descriptor.CompositionKind);
        Assert.Contains(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultCompositionMarker,
            composition.Descriptor.EvidenceRefs);
        Assert.Equal(bool.TrueString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal(bool.TrueString, composition.Descriptor.BoundaryMetadata["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal(
            DesignConversionLocalServiceComposition.BrowserBackedLocalDefaultScope,
            composition.Descriptor.BoundaryMetadata["browser_renderer_default_scope"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["product_ready"]);
        Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_provider_kind"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["public_production_default_ready"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal(2, browserAdapter.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);

        var candidate = result.Value.Evaluation.RuntimeFlowResult.Candidate;
        Assert.Equal(
            "critic_local_checkpoint_feature_adjusted_score",
            candidate.Metadata["logical_flow_primary_metric_name"]);
        Assert.True(
            int.Parse(candidate.Metadata["LocalCheckpointFeatureContributionCount"], System.Globalization.CultureInfo.InvariantCulture) > 0);
        Assert.Equal(bool.TrueString, candidate.Metadata["local_checkpoint_weights_configured"]);
        Assert.Equal(
            1,
            candidate.EvidenceRefs.Count(reference => string.Equals(
                "logical-flow:design-conversion-routing-flow",
                reference,
                StringComparison.Ordinal)));
        Assert.Equal(
            1,
            candidate.EvidenceRefs.Count(reference => string.Equals(
                "logical-orchestrator-selected-flow:design-conversion-routing-flow",
                reference,
                StringComparison.Ordinal)));
        Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, candidate.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, candidate.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, result.Value.Evaluation.Warnings);
    }

    [Fact]
    public async Task CreateRequestScopedService_RoutingFlowCompositionUsesCapturedRoutingFlowAfterProviderReplacement()
    {
        var originalComparisonProvider = new RecordingComparisonProvider(score: 0.10, passed: false);
        var originalVisualJudge = new RecordingVisualJudge(score: 0.10, passed: false, evidenceMarker: "b1-original-routing-judge");
        var markerCritic = new MarkerCriticUnit("b1-captured-routing-flow-critic");
        var markerImprover = new MarkerImproverUnit("b1-captured-routing-flow-improver");
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal(
            originalComparisonProvider,
            originalVisualJudge,
            new DesignConversionRoutingFlow(markerCritic, markerImprover));
        var requestComparisonProvider = new RecordingComparisonProvider(score: 0.99, passed: true);
        var requestVisualJudge = new RecordingVisualJudge(score: 0.99, passed: true, evidenceMarker: "b1-request-routing-judge");
        var requestScopedService = composition.CreateRequestScopedService(
            requestComparisonProvider,
            requestVisualJudge);

        var result = await requestScopedService.EvaluateAsync(CreateRequest("b1-request-scoped-routing-flow"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(originalComparisonProvider.Requests);
        Assert.Empty(originalVisualJudge.Requests);
        Assert.Equal(2, requestComparisonProvider.Requests.Count);
        Assert.Equal(2, requestVisualJudge.Requests.Count);
        Assert.Equal(1, markerCritic.ExecuteCount);
        Assert.Equal(1, markerImprover.ExecuteCount);
        Assert.Contains("b1-captured-routing-flow-critic", result.Value!.RuntimeFlowResult.EvidenceRefs);
        Assert.Contains("b1-captured-routing-flow-improver", result.Value.RuntimeFlowResult.EvidenceRefs);
        Assert.Contains(
            "b1-request-routing-judge",
            result.Value.VisualJudgeResults.SelectMany(judge => judge.EvidenceRefs));
        Assert.DoesNotContain(
            "b1-original-routing-judge",
            result.Value.VisualJudgeResults.SelectMany(judge => judge.EvidenceRefs));
    }

    [Fact]
    public async Task CreateRequestScopedService_MachineBackedCompositionRetainsMachineOrchestratorAfterProviderReplacement()
    {
        var browserAdapter = new RecordingBrowserAdapter();
        var originalComparisonProvider = new RecordingComparisonProvider(score: 0.10, passed: false);
        var originalVisualJudge = new RecordingVisualJudge(score: 0.10, passed: false, evidenceMarker: "b1-original-machine-judge");
        var machine = await TrainMinimalMachineForLocalCheckpointContributionAsync();
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedMachineLocalDefault(
            browserAdapter,
            machine,
            originalComparisonProvider,
            originalVisualJudge);
        var requestComparisonProvider = new RecordingComparisonProvider(score: 0.62, passed: false);
        var requestVisualJudge = new RecordingVisualJudge(score: 0.62, passed: false, evidenceMarker: "b1-request-machine-judge");
        var requestScopedService = composition.CreateRequestScopedService(
            requestComparisonProvider,
            requestVisualJudge);

        var result = await requestScopedService.EvaluateAsync(CreateRequest("b1-request-scoped-machine"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(originalComparisonProvider.Requests);
        Assert.Empty(originalVisualJudge.Requests);
        Assert.Equal(2, browserAdapter.Requests.Count);
        Assert.Equal(2, requestComparisonProvider.Requests.Count);
        Assert.Equal(2, requestVisualJudge.Requests.Count);

        var candidate = result.Value!.RuntimeFlowResult.Candidate;
        Assert.Equal(
            "critic_local_checkpoint_feature_adjusted_score",
            candidate.Metadata["logical_flow_primary_metric_name"]);
        Assert.True(
            int.Parse(
                candidate.Metadata["LocalCheckpointFeatureContributionCount"],
                System.Globalization.CultureInfo.InvariantCulture) > 0);
        Assert.Equal(bool.TrueString, candidate.Metadata["local_checkpoint_weights_configured"]);
        Assert.Contains(
            "logical-orchestrator-selected-flow:design-conversion-routing-flow",
            candidate.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, candidate.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.LocalCheckpointFeatureAdjustmentEvidence, candidate.EvidenceRefs);
        Assert.Contains(DesignConversionMachine.RuntimeScaffoldNotTrainedModelWarning, result.Value.Warnings);
        Assert.Contains(
            "b1-request-machine-judge",
            result.Value.VisualJudgeResults.SelectMany(judge => judge.EvidenceRefs));
        Assert.DoesNotContain(
            "b1-original-machine-judge",
            result.Value.VisualJudgeResults.SelectMany(judge => judge.EvidenceRefs));
    }

    [Fact]
    public void CreateRequestScopedService_RequiresComparisonProvider()
    {
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal(
            new RecordingComparisonProvider(score: 0.93, passed: true));
        var visualJudge = new RecordingVisualJudge(score: 0.93, passed: true, evidenceMarker: "b1-null-validation-judge");

        var exception = Assert.Throws<ArgumentNullException>(() =>
            composition.CreateRequestScopedService(null!, visualJudge));

        Assert.Equal("comparisonProvider", exception.ParamName);
    }

    [Fact]
    public void CreateRequestScopedService_RequiresVisualJudge()
    {
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal(
            new RecordingComparisonProvider(score: 0.93, passed: true));
        var comparisonProvider = new RecordingComparisonProvider(score: 0.93, passed: true);

        var exception = Assert.Throws<ArgumentNullException>(() =>
            composition.CreateRequestScopedService(comparisonProvider, null!));

        Assert.Equal("visualJudge", exception.ParamName);
    }

    [Fact]
    public async Task CreateDeterministicLocal_PreservesNonBrowserDefaultBoundary()
    {
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal(
            new RecordingComparisonProvider(score: 0.93, passed: true));

        var result = await composition.EvaluateAsync(CreateRequest("pass132-deterministic-composition"));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal(bool.FalseString, result.Value!.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.TrueString, result.Value.BoundaryMetadata["browser_render_not_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_default_production_wired"]);
        Assert.Equal("not_browser_backed_for_current_injection", result.Value.BoundaryMetadata["service_scaffold_browser_renderer_default_scope"]);
        Assert.Equal(DeterministicDesignConversionRenderProvider.ProviderKind, result.Value.BoundaryMetadata["service_scaffold_browser_renderer_provider_kind"]);
        Assert.DoesNotContain("service_scaffold_browser_renderer_default_wired", result.Value.BoundaryMetadata.Keys);
        Assert.All(result.Value.Evaluation.RenderResults, render =>
        {
            Assert.Equal(bool.FalseString, render.Image.Metadata["browser_or_playwright_used"]);
            Assert.Equal(DeterministicDesignConversionRenderProvider.ProviderKind, render.Image.Metadata["provider_kind"]);
        });
    }

    [Fact]
    public async Task EvaluateAsync_PublicCompositionMetadataDoesNotExposeRawMarkupTreeOrPrivateMarkers()
    {
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            new RecordingBrowserAdapter(),
            new RecordingComparisonProvider(score: 0.96, passed: true));
        var request = CreateRequest("pass132-private-boundary") with
        {
            Html = "<main data-private=\"PRIVATE_PASS132_SECRET\"><h1>Private</h1></main>",
            Css = ".private{background:url('PRIVATE_PASS132_SECRET');}",
            DesignTreePayload = new DesignTreePayload(
                PayloadId: "tree-private",
                Format: "figma-json",
                Content: "{\"private\":\"PRIVATE_PASS132_SECRET\"}",
                Sha256: "sha256:tree-private",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = "PRIVATE_PASS132_SECRET"
                }),
            OriginalTreePayload = new DesignTreePayload(
                PayloadId: "tree-original-private",
                Format: "figma-json",
                Content: "{\"original\":\"PRIVATE_PASS132_SECRET\"}",
                Sha256: "sha256:tree-original-private",
                Metadata: new Dictionary<string, string>())
        };

        var result = await composition.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var publicText = JsonSerializer.Serialize(new
        {
            composition.Descriptor,
            result.Value!.BoundaryMetadata,
            result.Value.EvidenceRefs,
            result.Value.Warnings
        });
        Assert.DoesNotContain(request.Html, publicText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.Css, publicText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(request.DesignTreePayload.Content, publicText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIVATE_PASS132_SECRET", publicText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, result.Value.BoundaryMetadata["provider_payload_included"]);
    }

    [Fact]
    public async Task CreateBrowserBackedLocal_ReportsBoundedBrowserAdapterFailureWithoutReadyClaim()
    {
        var comparisonProvider = new RecordingComparisonProvider(score: 0.96, passed: true);
        var composition = DesignConversionLocalServiceComposition.CreateBrowserBackedLocal(
            new FailingBrowserAdapter(),
            comparisonProvider);
        var request = CreateRequest("pass132-browser-unavailable");
        request = request with
        {
            Budget = request.Budget with { MaxRenderQaRuns = 1 },
            RenderResolutions = [request.RenderResolutions[0]]
        };

        var result = await composition.EvaluateAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var localEvaluation = Assert.IsType<DesignConversionLocalServiceEvaluationResult>(result.Value);
        var evaluation = localEvaluation.Evaluation;
        var failure = Assert.Single(evaluation.ViewportFailures);
        Assert.Equal("render", failure.Stage);
        Assert.Equal("BROWSER_RUNTIME_UNAVAILABLE", failure.ErrorCode);
        Assert.Empty(evaluation.RenderResults);
        Assert.Empty(evaluation.ComparisonArtifacts);
        Assert.Empty(comparisonProvider.Requests);
        Assert.False(evaluation.Verdict.Passed);
        Assert.False(evaluation.RuntimeFlowResult.Output.Accepted);
        Assert.NotEqual(
            DesignConversionVisualQaBoundaryPolicy.DecisionAccepted,
            evaluation.VisualQaBoundaryTrace.VisualDecision.Decision);
        Assert.Contains(DesignConversionInternalService.ViewportRenderFailedStatus, evaluation.Warnings);
        Assert.Equal(bool.TrueString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_wired"]);
        Assert.Equal(bool.TrueString, composition.Descriptor.BoundaryMetadata["browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["browser_renderer_default_production_wired"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["public_production_default_ready"]);
        Assert.Equal(bool.FalseString, composition.Descriptor.BoundaryMetadata["external_ai_visual_judge_executed"]);
    }

    private static DesignConversionInternalEvaluateRequest CreateRequest(string id)
        => new(
            JobId: $"{id}-job",
            RequestId: $"{id}-request",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"screen\"><h1>Dashboard</h1></main>",
            Css: ".screen{display:grid;gap:16px;color:#123456;}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: $"{id}-tree",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: $"sha256:{id}-tree",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: $"{id}-original-tree",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\",\"version\":\"original\"}",
                Sha256: $"sha256:{id}-original-tree",
                Metadata: new Dictionary<string, string>()),
            Screenshots:
            [
                Expected($"{id}-mobile-shot", 390, 844),
                Expected($"{id}-desktop-shot", 1440, 900)
            ],
            DesignRenders:
            [
                Expected($"{id}-mobile-render", 390, 844),
                Expected($"{id}-desktop-render", 1440, 900)
            ],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "internal",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.50m,
                Metadata: new Dictionary<string, string>()),
            RenderResolutions:
            [
                new RenderQaResolution("mobile", 390, 844, 1.0),
                new RenderQaResolution("desktop", 1440, 900, 1.0)
            ],
            AiTeacherEvidenceRefs: ["teacher:none"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["fixture"] = "pass132-local-service-composition"
            },
            Provenance: new Dictionary<string, string>
            {
                ["source"] = "pass132-test"
            });

    private static DesignRenderArtifact Expected(string id, int width, int height)
        => new(
            ArtifactId: id,
            Kind: "design-render",
            Width: width,
            Height: height,
            Uri: $"artifact://pass132/{id}.png",
            Sha256: $"sha256:{Sha256Text(id)}",
            Metadata: new Dictionary<string, string>());

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static async Task<DesignConversionMachine> TrainMinimalMachineForLocalCheckpointContributionAsync()
    {
        var machine = new DesignConversionMachine();
        var training = await machine.TrainBatchAsync(new LearningBatch<DesignConversionDomain.DesignConversionDomain>(
            BatchId: "n15-route4a-machine-local-default-batch",
            DomainTag: DesignConversionMachine.DomainTag,
            TrainingItems:
            [
                new LearningTrainingItem(
                    ItemId: "n15-route4a-machine-local-default-critic",
                    Prompt: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["unit_id"] = DesignConversionRuntimeScaffoldCriticUnit.UnitId,
                        ["source_kind"] = "Figma",
                        ["source_manifest_hash"] = "n15-route4a-source-manifest",
                        ["source_identity_hash"] = "n15-route4a-source-identity",
                        ["source_structure_hash"] = "n15-route4a-source-structure",
                        ["pair_family"] = "visual"
                    }),
                    ChosenResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["visual_score"] = 0.97,
                        ["accepted"] = true
                    }),
                    RejectedResponse: JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        ["visual_score"] = 0.11,
                        ["accepted"] = false
                    }))
            ],
            SourceIds: ["n15-route4a-source-disjoint-local"]));

        Assert.True(training.IsSuccess, training.ErrorMessage);
        Assert.True(training.Value!.RowsTrained > 0);
        return machine;
    }

    private sealed class RecordingBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        public List<DesignConversionBrowserCaptureRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var physicalWidth = (int)Math.Round(
                request.Resolution.Width * request.Resolution.DeviceScaleFactor,
                MidpointRounding.AwayFromZero);
            var physicalHeight = (int)Math.Round(
                request.Resolution.Height * request.Resolution.DeviceScaleFactor,
                MidpointRounding.AwayFromZero);
            var render = await new DeterministicLocalDesignConversionPngRenderProvider(Path.Combine(
                    Path.GetTempPath(),
                    "llm_mvp_core",
                    "design-conversion-service-composition-browser-adapter",
                    Guid.NewGuid().ToString("N")))
                .RenderAsync(
                    new DesignConversionRenderRequest(
                        RenderRequestId: $"{request.CaptureRequestId}:service-composition-browser-png",
                        RequestId: request.RequestId,
                        Html: request.DocumentHtml,
                        Css: "body{margin:0}.service-composition-browser-proof{color:#123456;background:#f8fafc}",
                        Resolution: new RenderQaResolution(
                            $"{request.Resolution.ResolutionId}-physical",
                            physicalWidth,
                            physicalHeight,
                            1.0),
                        Metadata: request.Metadata),
                    cancellationToken);

            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            var sha = render.Value!.Image.Sha256["sha256:".Length..];

            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(
                new DesignConversionBrowserCaptureResult(
                    ScreenshotUri: render.Value.Image.Uri,
                    ScreenshotSha256: sha,
                    ElapsedMs: 8,
                    Metadata: new Dictionary<string, string>
                    {
                        ["browser_adapter_kind"] = "pass132-fake-browser-adapter",
                        ["browser_process_boundary"] = "fake_adapter_inspectable_local_png_no_real_browser"
                    },
                    Warnings: []));
        }
    }

    private sealed class FailingBrowserAdapter : IDesignConversionBrowserProcessAdapter
    {
        public Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<DesignConversionBrowserCaptureResult>.Fail(
                "No Chromium-family browser executable is available for pass132 fake failure.",
                "BROWSER_RUNTIME_UNAVAILABLE"));
    }

    private sealed class MarkerCriticUnit(string evidenceMarker) : IDesignConversionCriticUnit
    {
        public int ExecuteCount { get; private set; }

        public Task<OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionCriticUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Fail(
                    "Marker critic cancelled.",
                    "CANCELLED"));

            if (request is null || request.Input is null)
                return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Fail(
                    "Marker critic input is required.",
                    "CRITIC_INPUT_REQUIRED"));

            ExecuteCount++;
            var verdicts = request.Input.QaResults
                .Select(result => result.Verdict)
                .Concat(request.Input.ComparisonArtifacts.Select(artifact => artifact.Verdict))
                .ToArray();
            var score = verdicts.Length == 0 ? 0.0 : verdicts.Min(verdict => verdict.Score);
            var passed = verdicts.Length > 0 && verdicts.All(verdict => verdict.Passed);
            var evidenceRefs = request.Input.QaResults
                .Select(result => $"marker-qa:{result.ResultId}")
                .Concat(request.Input.ComparisonArtifacts.Select(artifact => $"marker-comparison:{artifact.ArtifactId}"))
                .Append(evidenceMarker)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var output = new DesignConversionCriticUnitOutput(
                OutputId: $"{evidenceMarker}:critic-output",
                Verdict: new VisualVerdict(
                    VerdictId: $"{evidenceMarker}:critic-verdict",
                    Score: score,
                    Defects: verdicts.SelectMany(verdict => verdict.Defects).ToArray(),
                    EvidenceRefs: evidenceRefs,
                    Passed: passed),
                QaEvidenceRefs: evidenceRefs,
                AiTeacherEvidenceRefs: request.Input.AiTeacherEvidenceRefs.ToArray(),
                DeterministicLogicClaimedAsLearning: false);
            var result = new LogicalUnitResult<DesignConversionCriticUnitOutput>(
                UnitId: $"{evidenceMarker}:unit",
                UnitRole: "b1_marker_critic",
                DomainTag: DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
                Output: output,
                Confidence: score,
                PredicateIds: ["b1_marker_critic"],
                DomainTags: [DesignConversionRuntimeScaffoldCriticUnit.DomainTag],
                EvidenceIds: evidenceRefs,
                Warnings: []);

            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Ok(result));
        }
    }

    private sealed class MarkerImproverUnit(string evidenceMarker) : IDesignConversionImproverUnit
    {
        public int ExecuteCount { get; private set; }

        public Task<OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionImproverUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Fail(
                    "Marker improver cancelled.",
                    "CANCELLED"));

            if (request is null || request.Input is null)
                return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Fail(
                    "Marker improver input is required.",
                    "IMPROVER_INPUT_REQUIRED"));

            ExecuteCount++;
            var candidate = request.Input.Candidate with
            {
                EvidenceRefs = request.Input.Candidate.EvidenceRefs
                    .Append(evidenceMarker)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                Metadata = request.Input.Candidate.Metadata
                    .Concat(new Dictionary<string, string>
                    {
                        ["b1_captured_routing_flow"] = evidenceMarker
                    })
                    .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal)
            };
            var output = new DesignConversionImprovementOutput(
                OutputId: $"{evidenceMarker}:improver-output",
                CandidateId: candidate.CandidateId,
                Html: candidate.Html,
                Css: candidate.Css,
                Verdict: request.Input.CriticVerdict,
                QaResults: [],
                EvidenceRefs: candidate.EvidenceRefs,
                Accepted: request.Input.CriticVerdict.Passed);
            var unitOutput = new DesignConversionImproverUnitOutput(
                OutputId: output.OutputId,
                Candidate: candidate,
                Output: output,
                QaEvidenceRefs: request.Input.QaEvidenceRefs.ToArray(),
                AiTeacherEvidenceRefs: request.Input.AiTeacherEvidenceRefs.ToArray(),
                DeterministicLogicClaimedAsLearning: false);
            var result = new LogicalUnitResult<DesignConversionImproverUnitOutput>(
                UnitId: $"{evidenceMarker}:unit",
                UnitRole: "b1_marker_improver",
                DomainTag: DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
                Output: unitOutput,
                Confidence: request.Input.CriticVerdict.Score,
                PredicateIds: ["b1_marker_improver"],
                DomainTags: [DesignConversionRuntimeScaffoldCriticUnit.DomainTag],
                EvidenceIds: [evidenceMarker],
                Warnings: []);

            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Ok(result));
        }
    }

    private sealed class RecordingComparisonProvider(double score, bool passed) : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var diffSha = Sha256Text(string.Join(
                "|",
                request.ComparisonRequestId,
                request.Expected.Sha256,
                request.Actual.Sha256,
                score,
                passed));
            IReadOnlyList<VisualDefect> defects = passed
                ? Array.Empty<VisualDefect>()
                :
                [
                    new VisualDefect(
                        DefectId: $"{request.ComparisonRequestId}:spacing",
                        Category: "spacing",
                        Severity: "minor",
                        Description: "Pass132 comparison test defect.",
                        X: 10,
                        Y: 12,
                        Width: 24,
                        Height: 16,
                        EvidenceImageIds: [request.Expected.ImageId, request.Actual.ImageId])
                ];
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:pass132-verdict",
                Score: score,
                Defects: defects,
                EvidenceRefs: [$"pass132-comparison:{request.ComparisonRequestId}"],
                Passed: passed);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(
                new DesignConversionQaComparisonArtifact(
                    ArtifactId: $"{request.ComparisonRequestId}:pass132-artifact",
                    ComparisonRequestId: request.ComparisonRequestId,
                    Expected: request.Expected,
                    Actual: request.Actual,
                    Verdict: verdict,
                    DiffImageUri: $"artifact://pass132/diff/{diffSha}.json",
                    DiffImageSha256: $"sha256:{diffSha}",
                    Metrics: new Dictionary<string, double>
                    {
                        ["visual_distance"] = 1.0 - score
                    },
                    EvidenceRefs: ["pass132-comparison-provider"])
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["external_ai_visual_judge_executed"] = bool.FalseString,
                        ["visual_ai_executed"] = bool.FalseString,
                        ["raw_html_included"] = bool.FalseString,
                        ["raw_css_included"] = bool.FalseString,
                        ["raw_design_tree_included"] = bool.FalseString
                    }
                }));
        }
    }

    private sealed class RecordingVisualJudge(double score, bool passed, string evidenceMarker) : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Fail(
                    "Recording visual judge cancelled.",
                    "CANCELLED"));

            Requests.Add(request);
            IReadOnlyList<VisualDefect> defects = passed
                ? []
                :
                [
                    new VisualDefect(
                        DefectId: $"{request.JudgeRequestId}:recording-judge-defect",
                        Category: "spacing",
                        Severity: "minor",
                        Description: "B1 recording visual judge defect.",
                        X: 4,
                        Y: 6,
                        Width: 8,
                        Height: 10,
                        EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
                ];
            var evidenceRefs = new[]
                {
                    evidenceMarker,
                    DefaultLocalDesignConversionVisualJudge.EvidenceMarker,
                    DefaultLocalDesignConversionVisualJudge.ExternalProviderNotUsedMarker,
                    NoConfiguredVisualJudge.EvidenceMarker,
                    DesignConversionVisualQaBoundaryPolicy.VisualJudgeProviderBoundaryMarker,
                    DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
                    DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker
                }
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:recording-judge-verdict",
                Score: score,
                Defects: defects,
                EvidenceRefs: evidenceRefs,
                Passed: passed);

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(
                new DesignConversionVisualJudgeResult(
                    JudgeResultId: $"{request.JudgeRequestId}:recording-judge-result",
                    JudgeRequestId: request.JudgeRequestId,
                    Status: DefaultLocalDesignConversionVisualJudge.StatusOk,
                    IsBlocked: false,
                    Verdict: verdict,
                    Provider: new DesignConversionVisualJudgeProviderMetadata(
                        ProviderName: "b1-recording-local-visual-judge",
                        EndpointHost: "local",
                        ModelId: "b1-recording-v1",
                        ExternalCallExecuted: false,
                        Metadata: new Dictionary<string, string>
                        {
                            ["configured"] = bool.TrueString,
                            ["provider_boundary"] = DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
                            [DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker] = bool.TrueString,
                            ["external_provider_call"] = bool.FalseString,
                            ["external_ai_visual_judge_executed"] = bool.FalseString,
                            ["visual_ai_executed"] = bool.FalseString
                        }),
                    EstimatedCostUsd: 0m,
                    TokenMetadata: new Dictionary<string, double>(),
                    EvidenceRefs: evidenceRefs,
                    Metadata: new Dictionary<string, string>
                    {
                        ["status"] = DefaultLocalDesignConversionVisualJudge.StatusOk,
                        ["provider_boundary"] = DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker,
                        [DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker] = bool.TrueString,
                        ["external_ai_visual_judge_executed"] = bool.FalseString,
                        ["visual_ai_executed"] = bool.FalseString,
                        ["score"] = score.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                    })));
        }
    }
}
