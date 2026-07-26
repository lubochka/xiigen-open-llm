// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using AgentInfrastructure;

public sealed record DesignConversionLocalServiceCompositionDescriptor(
    string CompositionKind,
    IReadOnlyDictionary<string, string> BoundaryMetadata,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings);

public sealed record DesignConversionLocalServiceEvaluationResult(
    DesignConversionInternalEvaluationResult Evaluation,
    DesignConversionLocalServiceCompositionDescriptor Descriptor,
    IReadOnlyDictionary<string, string> BoundaryMetadata,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Warnings);

public sealed class DesignConversionLocalServiceComposition
{
    public const string BrowserBackedLocalCompositionKind = "browser_backed_local_service_composition";
    public const string BrowserBackedLocalDefaultCompositionKind = "browser_backed_local_default_service_composition";
    public const string DeterministicLocalCompositionKind = "deterministic_local_service_composition";
    public const string DeterministicLocalPngCompositionKind = "deterministic_local_png_artifact_service_composition";
    public const string ExplicitLocalCompositionMarker = "design_conversion_explicit_local_service_composition";
    public const string BrowserBackedCompositionMarker = "browser_backed_render_provider_explicitly_wired";
    public const string BrowserBackedLocalDefaultCompositionMarker = "browser_backed_render_provider_local_default_wired";
    public const string DeterministicCompositionMarker = "deterministic_render_provider_explicitly_wired";
    public const string DeterministicLocalPngCompositionMarker = "deterministic_local_png_render_provider_explicitly_wired";
    public const string BrowserBackedLocalDefaultScope = "local_default_browser_runtime_not_production";
    public const string LocalVisualJudgeMarker = "local_visual_judge_default_or_injected_non_network";
    public const string ProductNotReadyMarker = "local_composition_product_ready_false";

    public DesignConversionInternalService Service { get; }
    public IDesignConversionRenderProvider RenderProvider { get; }
    public IDesignConversionComparisonProvider ComparisonProvider { get; }
    public IDesignConversionVisualJudge VisualJudge { get; }
    public DesignConversionLocalServiceCompositionDescriptor Descriptor { get; }

    private delegate DesignConversionInternalService RequestScopedServiceFactory(
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge);

    private readonly RequestScopedServiceFactory _requestScopedServiceFactory;

    private DesignConversionLocalServiceComposition(
        DesignConversionInternalService service,
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge,
        RequestScopedServiceFactory requestScopedServiceFactory,
        DesignConversionLocalServiceCompositionDescriptor descriptor)
    {
        Service = service ?? throw new ArgumentNullException(nameof(service));
        RenderProvider = renderProvider ?? throw new ArgumentNullException(nameof(renderProvider));
        ComparisonProvider = comparisonProvider ?? throw new ArgumentNullException(nameof(comparisonProvider));
        VisualJudge = visualJudge ?? throw new ArgumentNullException(nameof(visualJudge));
        _requestScopedServiceFactory = requestScopedServiceFactory ?? throw new ArgumentNullException(nameof(requestScopedServiceFactory));
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
    }

    public DesignConversionInternalService CreateRequestScopedService(
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge)
    {
        if (comparisonProvider is null)
            throw new ArgumentNullException(nameof(comparisonProvider));
        if (visualJudge is null)
            throw new ArgumentNullException(nameof(visualJudge));

        return _requestScopedServiceFactory(comparisonProvider, visualJudge);
    }

    public static DesignConversionLocalServiceComposition CreateBrowserBackedLocal(
        IDesignConversionBrowserProcessAdapter browserAdapter,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge? visualJudge = null,
        DesignConversionRoutingFlow? routingFlow = null)
    {
        if (browserAdapter is null)
            throw new ArgumentNullException(nameof(browserAdapter));
        if (comparisonProvider is null)
            throw new ArgumentNullException(nameof(comparisonProvider));

        var localVisualJudge = visualJudge ?? new DefaultLocalDesignConversionVisualJudge();
        var renderProvider = new BrowserBackedDesignConversionRenderProvider(browserAdapter);
        var resolvedRoutingFlow = routingFlow ?? new DesignConversionRoutingFlow();
        var service = new DesignConversionInternalService(
            renderProvider,
            comparisonProvider,
            localVisualJudge,
            resolvedRoutingFlow);

        return new DesignConversionLocalServiceComposition(
            service,
            renderProvider,
            comparisonProvider,
            localVisualJudge,
            (requestComparisonProvider, requestVisualJudge) => new DesignConversionInternalService(
                renderProvider,
                requestComparisonProvider,
                requestVisualJudge,
                resolvedRoutingFlow),
            BuildDescriptor(
                BrowserBackedLocalCompositionKind,
                browserRendererDefaultWired: true,
                browserRendererExplicitLocalInjectionProven: true,
                browserRendererDefaultScope: "explicit_local_composition_only",
                renderProviderKind: BrowserBackedDesignConversionRenderProvider.ProviderKind,
                localVisualJudgeDefaultWired: visualJudge is null,
                evidenceRefs:
                [
                    ExplicitLocalCompositionMarker,
                    BrowserBackedCompositionMarker,
                    BrowserBackedDesignConversionRenderProvider.BrowserUsedMarker,
                    BrowserBackedDesignConversionRenderProvider.ExternalAiUsedMarker,
                    LocalVisualJudgeMarker,
                    ProductNotReadyMarker
                ]));
    }

    public static DesignConversionLocalServiceComposition CreateBrowserBackedLocalDefault(
        IDesignConversionBrowserProcessAdapter browserAdapter,
        IDesignConversionComparisonProvider? comparisonProvider = null,
        IDesignConversionVisualJudge? visualJudge = null,
        DesignConversionRoutingFlow? routingFlow = null)
    {
        if (browserAdapter is null)
            throw new ArgumentNullException(nameof(browserAdapter));

        var localVisualJudge = visualJudge ?? new DefaultLocalDesignConversionVisualJudge();
        var renderProvider = new BrowserBackedDesignConversionRenderProvider(browserAdapter);
        var resolvedComparisonProvider = comparisonProvider ?? new DeterministicDesignConversionComparisonProvider();
        var resolvedRoutingFlow = routingFlow ?? new DesignConversionRoutingFlow();
        var service = new DesignConversionInternalService(
            renderProvider,
            resolvedComparisonProvider,
            localVisualJudge,
            resolvedRoutingFlow);

        return new DesignConversionLocalServiceComposition(
            service,
            renderProvider,
            resolvedComparisonProvider,
            localVisualJudge,
            (requestComparisonProvider, requestVisualJudge) => new DesignConversionInternalService(
                renderProvider,
                requestComparisonProvider,
                requestVisualJudge,
                resolvedRoutingFlow),
            BuildDescriptor(
                BrowserBackedLocalDefaultCompositionKind,
                browserRendererDefaultWired: true,
                browserRendererExplicitLocalInjectionProven: true,
                browserRendererDefaultScope: BrowserBackedLocalDefaultScope,
                renderProviderKind: BrowserBackedDesignConversionRenderProvider.ProviderKind,
                localVisualJudgeDefaultWired: visualJudge is null,
                evidenceRefs:
                [
                    ExplicitLocalCompositionMarker,
                    BrowserBackedCompositionMarker,
                    BrowserBackedLocalDefaultCompositionMarker,
                    BrowserBackedDesignConversionRenderProvider.BrowserUsedMarker,
                    BrowserBackedDesignConversionRenderProvider.ExternalAiUsedMarker,
                    LocalVisualJudgeMarker,
                    ProductNotReadyMarker
                ]));
    }

    public static DesignConversionLocalServiceComposition CreateBrowserBackedMachineLocalDefault(
        IDesignConversionBrowserProcessAdapter browserAdapter,
        DesignConversionMachine machine,
        IDesignConversionComparisonProvider? comparisonProvider = null,
        IDesignConversionVisualJudge? visualJudge = null)
    {
        if (browserAdapter is null)
            throw new ArgumentNullException(nameof(browserAdapter));
        if (machine is null)
            throw new ArgumentNullException(nameof(machine));

        var localVisualJudge = visualJudge ?? new DefaultLocalDesignConversionVisualJudge();
        var renderProvider = new BrowserBackedDesignConversionRenderProvider(browserAdapter);
        var resolvedComparisonProvider = comparisonProvider ?? new DeterministicDesignConversionComparisonProvider();
        var service = new DesignConversionInternalService(
            renderProvider,
            resolvedComparisonProvider,
            localVisualJudge,
            machine);

        return new DesignConversionLocalServiceComposition(
            service,
            renderProvider,
            resolvedComparisonProvider,
            localVisualJudge,
            (requestComparisonProvider, requestVisualJudge) => new DesignConversionInternalService(
                renderProvider,
                requestComparisonProvider,
                requestVisualJudge,
                machine),
            BuildDescriptor(
                BrowserBackedLocalDefaultCompositionKind,
                browserRendererDefaultWired: true,
                browserRendererExplicitLocalInjectionProven: true,
                browserRendererDefaultScope: BrowserBackedLocalDefaultScope,
                renderProviderKind: BrowserBackedDesignConversionRenderProvider.ProviderKind,
                localVisualJudgeDefaultWired: visualJudge is null,
                evidenceRefs:
                [
                    ExplicitLocalCompositionMarker,
                    BrowserBackedCompositionMarker,
                    BrowserBackedLocalDefaultCompositionMarker,
                    BrowserBackedDesignConversionRenderProvider.BrowserUsedMarker,
                    BrowserBackedDesignConversionRenderProvider.ExternalAiUsedMarker,
                    LocalVisualJudgeMarker,
                    ProductNotReadyMarker
                ]));
    }

    public static DesignConversionLocalServiceComposition CreateDeterministicLocal(
        IDesignConversionComparisonProvider? comparisonProvider = null,
        IDesignConversionVisualJudge? visualJudge = null,
        DesignConversionRoutingFlow? routingFlow = null)
    {
        var localVisualJudge = visualJudge ?? new DefaultLocalDesignConversionVisualJudge();
        var renderProvider = new DeterministicDesignConversionRenderProvider();
        var resolvedComparisonProvider = comparisonProvider ?? new DeterministicDesignConversionComparisonProvider();
        var resolvedRoutingFlow = routingFlow ?? new DesignConversionRoutingFlow();
        var service = new DesignConversionInternalService(
            renderProvider,
            resolvedComparisonProvider,
            localVisualJudge,
            resolvedRoutingFlow);

        return new DesignConversionLocalServiceComposition(
            service,
            renderProvider,
            resolvedComparisonProvider,
            localVisualJudge,
            (requestComparisonProvider, requestVisualJudge) => new DesignConversionInternalService(
                renderProvider,
                requestComparisonProvider,
                requestVisualJudge,
                resolvedRoutingFlow),
            BuildDescriptor(
                DeterministicLocalCompositionKind,
                browserRendererDefaultWired: false,
                browserRendererExplicitLocalInjectionProven: false,
                browserRendererDefaultScope: "not_wired_for_deterministic_local_composition",
                renderProviderKind: DeterministicDesignConversionRenderProvider.ProviderKind,
                localVisualJudgeDefaultWired: visualJudge is null,
                evidenceRefs:
                [
                    ExplicitLocalCompositionMarker,
                    DeterministicCompositionMarker,
                    DeterministicDesignConversionRenderProvider.BrowserUsedMarker,
                    DeterministicDesignConversionRenderProvider.ExternalAiUsedMarker,
                    LocalVisualJudgeMarker,
                    ProductNotReadyMarker
                ]));
    }

    public static DesignConversionLocalServiceComposition CreateDeterministicLocalPng(
        string artifactRoot,
        IDesignConversionComparisonProvider? comparisonProvider = null,
        IDesignConversionVisualJudge? visualJudge = null,
        DesignConversionRoutingFlow? routingFlow = null)
    {
        var localVisualJudge = visualJudge ?? new DefaultLocalDesignConversionVisualJudge();
        var renderProvider = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot);
        var resolvedComparisonProvider = comparisonProvider ?? new DeterministicDesignConversionComparisonProvider();
        var resolvedRoutingFlow = routingFlow ?? new DesignConversionRoutingFlow();
        var service = new DesignConversionInternalService(
            renderProvider,
            resolvedComparisonProvider,
            localVisualJudge,
            resolvedRoutingFlow);

        return new DesignConversionLocalServiceComposition(
            service,
            renderProvider,
            resolvedComparisonProvider,
            localVisualJudge,
            (requestComparisonProvider, requestVisualJudge) => new DesignConversionInternalService(
                renderProvider,
                requestComparisonProvider,
                requestVisualJudge,
                resolvedRoutingFlow),
            BuildDescriptor(
                DeterministicLocalPngCompositionKind,
                browserRendererDefaultWired: false,
                browserRendererExplicitLocalInjectionProven: false,
                browserRendererDefaultScope: "not_wired_for_deterministic_local_png_artifact_composition",
                renderProviderKind: DeterministicLocalDesignConversionPngRenderProvider.ProviderKind,
                localVisualJudgeDefaultWired: visualJudge is null,
                evidenceRefs:
                [
                    ExplicitLocalCompositionMarker,
                    DeterministicLocalPngCompositionMarker,
                    DeterministicLocalDesignConversionPngRenderProvider.BrowserUsedMarker,
                    DeterministicLocalDesignConversionPngRenderProvider.ExternalAiUsedMarker,
                    DeterministicLocalDesignConversionPngRenderProvider.BytesWrittenMarker,
                    DeterministicLocalDesignConversionPngRenderProvider.BoundaryMarker,
                    LocalVisualJudgeMarker,
                    ProductNotReadyMarker
                ]));
    }

    public async Task<OperationResult<DesignConversionLocalServiceEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        CancellationToken cancellationToken = default)
    {
        var evaluation = await Service.EvaluateAsync(request, cancellationToken);
        if (!evaluation.IsSuccess)
            return evaluation.MapError<DesignConversionLocalServiceEvaluationResult>();

        var value = evaluation.Value!;
        var boundaryMetadata = BuildEvaluationBoundaryMetadata(Descriptor, value);
        var evidenceRefs = BuildEvaluationEvidenceRefs(Descriptor, value, boundaryMetadata);
        var warnings = Descriptor.Warnings
            .Concat(value.Warnings)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(warning => warning, StringComparer.Ordinal)
            .ToArray();

        return OperationResult<DesignConversionLocalServiceEvaluationResult>.Ok(
            new DesignConversionLocalServiceEvaluationResult(
                Evaluation: value,
                Descriptor: Descriptor,
                BoundaryMetadata: boundaryMetadata,
                EvidenceRefs: evidenceRefs,
                Warnings: warnings));
    }

    private static DesignConversionLocalServiceCompositionDescriptor BuildDescriptor(
        string compositionKind,
        bool browserRendererDefaultWired,
        bool browserRendererExplicitLocalInjectionProven,
        string browserRendererDefaultScope,
        string renderProviderKind,
        bool localVisualJudgeDefaultWired,
        IReadOnlyList<string> evidenceRefs)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["composition_kind"] = compositionKind,
            ["explicit_local_service_composition"] = bool.TrueString,
            ["browser_renderer_default_wired"] = browserRendererDefaultWired.ToString(),
            ["browser_renderer_explicit_local_injection_proven"] = browserRendererExplicitLocalInjectionProven.ToString(),
            ["browser_renderer_default_production_wired"] = bool.FalseString,
            ["browser_renderer_default_scope"] = browserRendererDefaultScope,
            ["render_provider_kind"] = renderProviderKind,
            ["comparison_provider_injected"] = bool.TrueString,
            ["local_visual_judge_default_wired"] = localVisualJudgeDefaultWired.ToString(),
            ["visual_judge_boundary"] = LocalVisualJudgeMarker,
            ["external_ai_visual_judge_executed"] = bool.FalseString,
            ["real_external_provider_executed"] = bool.FalseString,
            ["network_call_executed"] = bool.FalseString,
            ["public_production_default_ready"] = bool.FalseString,
            ["public_http_endpoint_verified"] = bool.FalseString,
            ["socket_opened"] = bool.FalseString,
            ["cross_repo_caller_verified"] = bool.FalseString,
            ["product_ready"] = bool.FalseString,
            ["trained_feature_claimed"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };

        return new DesignConversionLocalServiceCompositionDescriptor(
            CompositionKind: compositionKind,
            BoundaryMetadata: metadata,
            EvidenceRefs: evidenceRefs
                .Append(ProductNotReadyMarker)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            Warnings:
            [
                "LOCAL_SERVICE_COMPOSITION_ONLY_NOT_PUBLIC_OR_PRODUCTION_DEFAULT",
                "EXTERNAL_AI_VISUAL_JUDGE_NOT_EXECUTED_BY_COMPOSITION_DEFAULT"
            ]);
    }

    private static IReadOnlyDictionary<string, string> BuildEvaluationBoundaryMetadata(
        DesignConversionLocalServiceCompositionDescriptor descriptor,
        DesignConversionInternalEvaluationResult evaluation)
    {
        var browserRenderExecuted = evaluation.RenderResults.Any(result =>
            IsTruthy(result.Image.Metadata, "browser_or_playwright_used"));
        var localVisualJudgeObserved = evaluation.VisualJudgeResults.Any(result =>
            IsTruthy(result.Provider.Metadata, DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker));
        var externalAiExecuted = evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted;
        var viewportCoverage = evaluation.VisualQaBoundaryTrace.MultiResolutionRender;
        var metadata = new Dictionary<string, string>(descriptor.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["evaluation_boundary_metadata"] = bool.TrueString,
            ["browser_render_executed"] = browserRenderExecuted.ToString(),
            ["browser_render_not_executed"] = (!browserRenderExecuted).ToString(),
            ["local_visual_judge_observed"] = localVisualJudgeObserved.ToString(),
            ["external_ai_visual_judge_executed"] = externalAiExecuted.ToString(),
            ["real_external_provider_executed"] = externalAiExecuted.ToString(),
            ["network_call_executed"] = externalAiExecuted.ToString(),
            ["render_result_count"] = evaluation.RenderResults.Count.ToString(CultureInfo.InvariantCulture),
            ["comparison_artifact_count"] = evaluation.ComparisonArtifacts.Count.ToString(CultureInfo.InvariantCulture),
            ["visual_judge_result_count"] = evaluation.VisualJudgeResults.Count.ToString(CultureInfo.InvariantCulture),
            ["qa_result_count"] = evaluation.QaResults.Count.ToString(CultureInfo.InvariantCulture),
            ["multi_viewport_required_count"] = viewportCoverage.RequiredViewportIds.Count.ToString(CultureInfo.InvariantCulture),
            ["multi_viewport_observed_count"] = viewportCoverage.ObservedViewportIds.Count.ToString(CultureInfo.InvariantCulture),
            ["multi_viewport_missing_count"] = viewportCoverage.MissingViewportIds.Count.ToString(CultureInfo.InvariantCulture),
            ["multi_viewport_complete"] = viewportCoverage.IsComplete.ToString(),
            ["multi_viewport_required_ids"] = JoinViewportIds(viewportCoverage.RequiredViewportIds),
            ["multi_viewport_observed_ids"] = JoinViewportIds(viewportCoverage.ObservedViewportIds),
            ["multi_viewport_missing_ids"] = JoinViewportIds(viewportCoverage.MissingViewportIds),
            ["visual_judge_status"] = evaluation.VisualQaBoundaryTrace.VisualJudgeStatus,
            ["visual_judge_provider_boundary"] = evaluation.VisualQaBoundaryTrace.VisualJudgeProviderBoundary,
            ["tier_policy_stop_reason"] = evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason,
            ["best_visual_score"] = evaluation.VisualQaBoundaryTrace.TierPolicy.BestScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["required_visual_score"] = evaluation.VisualQaBoundaryTrace.TierPolicy.RequiredScore.ToString("0.####", CultureInfo.InvariantCulture),
            ["product_ready"] = evaluation.VisualQaBoundaryTrace.ProductReady.ToString(),
            ["trained_feature_claimed"] = evaluation.VisualQaBoundaryTrace.TrainedFeatureClaimed.ToString(),
            ["service_scaffold_browser_renderer_explicit_local_injection_proven"] = TryRead(
                evaluation.ScaffoldMarkers,
                "browser_renderer_explicit_local_injection_proven"),
            ["service_scaffold_browser_renderer_default_production_wired"] = TryRead(
                evaluation.ScaffoldMarkers,
                "browser_renderer_default_production_wired"),
            ["service_scaffold_browser_renderer_default_scope"] = TryRead(
                evaluation.ScaffoldMarkers,
                "browser_renderer_default_scope"),
            ["service_scaffold_browser_renderer_provider_kind"] = TryRead(
                evaluation.ScaffoldMarkers,
                "browser_renderer_injected_provider_kind"),
            ["refs_or_hashes_only"] = bool.TrueString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["provider_payload_included"] = bool.FalseString
        };

        return metadata;
    }

    private static string JoinViewportIds(IReadOnlyList<string> viewportIds)
        => string.Join(",", viewportIds.Select(SafeViewportId));

    private static string SafeViewportId(string viewportId)
    {
        if (string.IsNullOrWhiteSpace(viewportId))
            return "unknown";

        var safe = new string(viewportId
            .Trim()
            .Take(64)
            .Select(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or '@'
                ? character
                : '_')
            .ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "unknown" : safe;
    }

    private static IReadOnlyList<string> BuildEvaluationEvidenceRefs(
        DesignConversionLocalServiceCompositionDescriptor descriptor,
        DesignConversionInternalEvaluationResult evaluation,
        IReadOnlyDictionary<string, string> boundaryMetadata)
    {
        var hashedEvaluationRefs = evaluation.EvidenceRefs
            .Concat(evaluation.VisualQaBoundaryTrace.EvidenceRefs)
            .Select(reference => DesignConversionDpoLeakageGuard.HashRef("dcm-local-service-evidence", reference));

        return descriptor.EvidenceRefs
            .Concat(hashedEvaluationRefs)
            .Append($"composition_kind:{descriptor.CompositionKind}")
            .Append($"browser_renderer_default_wired:{boundaryMetadata["browser_renderer_default_wired"]}")
            .Append($"browser_renderer_explicit_local_injection_proven:{boundaryMetadata["browser_renderer_explicit_local_injection_proven"]}")
            .Append($"browser_renderer_default_production_wired:{boundaryMetadata["browser_renderer_default_production_wired"]}")
            .Append($"browser_renderer_default_scope:{boundaryMetadata["browser_renderer_default_scope"]}")
            .Append($"service_scaffold_browser_renderer_explicit_local_injection_proven:{boundaryMetadata["service_scaffold_browser_renderer_explicit_local_injection_proven"]}")
            .Append($"service_scaffold_browser_renderer_default_production_wired:{boundaryMetadata["service_scaffold_browser_renderer_default_production_wired"]}")
            .Append($"browser_render_executed:{boundaryMetadata["browser_render_executed"]}")
            .Append($"external_ai_visual_judge_executed:{boundaryMetadata["external_ai_visual_judge_executed"]}")
            .Append($"local_visual_judge_observed:{boundaryMetadata["local_visual_judge_observed"]}")
            .Append("raw_html_included:false")
            .Append("raw_css_included:false")
            .Append("raw_design_tree_included:false")
            .Append("provider_payload_included:false")
            .Append(ProductNotReadyMarker)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reference => reference, StringComparer.Ordinal)
            .ToArray();
    }

    private static string TryRead(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) ? value : string.Empty;

    private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));
}
