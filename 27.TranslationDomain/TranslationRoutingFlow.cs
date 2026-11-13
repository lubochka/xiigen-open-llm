// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

// This flow routes a TranslationRoutingRequest to the correct direction adapter
// (RU_EN, EN_RU, HE_EN, or EN_HE) and returns a translation result.
// Bootstrap behaviour: selects the adapter and returns a structured result;
// trained behaviour (after DPO batches) will produce real translations.
// Governed by: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md Direction 1, decision A-LM-04.
// Flow id: translation-routing-flow

using System.Globalization;
using AgentInfrastructure;         // OperationResult<T>
using AgentInfrastructure.Logical; // ILogicalFlow, LogicalFlowRequest, LogicalFlowResult, etc.

// TranslationRoutingFlow implements ILogicalFlow<TranslationRoutingRequest>.
// It is the orchestration entry point for all translation requests.
// It selects the correct direction adapter based on SourceLanguage + TargetLanguage,
// then delegates to the Bootstrap translation adapter for that direction.
public sealed class TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>
{
    // FlowId is the stable identifier used to register this flow in the orchestrator registry.
    public const string FlowId = "translation-routing-flow";

    // ScenarioId is the routing key used by AdaptiveLogicalOrchestrator.
    public const string ScenarioId = "translation-routing-flow";

    // _adapters maps each TranslationDirection to its Bootstrap direction adapter.
    private readonly IReadOnlyDictionary<TranslationDirection, ITranslationDirectionAdapter> _adapters;

    // When called without arguments the flow creates all four Bootstrap adapters.
    // Custom adapters can be supplied for testing or trained-unit replacement.
    public TranslationRoutingFlow(
        IReadOnlyDictionary<TranslationDirection, ITranslationDirectionAdapter>? adapters = null)
    {
        // Store the adapters; fall back to all four Bootstrap adapters when none are supplied.
        _adapters = adapters ?? BuildBootstrapAdapters();
    }

    // ExecuteAsync routes the request to the correct direction adapter and returns a flow result.
    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<TranslationRoutingRequest> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation immediately when the token was already cancelled.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Translation routing flow cancelled.", "CANCELLED");

        // Require a non-null request at the flow boundary.
        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Logical flow request is required.", "REQUEST_REQUIRED");

        // Reject requests for a different flow id to prevent misrouting.
        if (!string.Equals(request.FlowId, FlowId, StringComparison.Ordinal))
            return OperationResult<LogicalFlowResult>.Fail(
                $"Unsupported flow id '{request.FlowId}'. Expected '{FlowId}'.", "UNSUPPORTED_FLOW_ID");

        // Reject null input — the routing request is always required.
        if (request.Input is null)
            return OperationResult<LogicalFlowResult>.Fail("TranslationRoutingRequest input is required.", "INPUT_REQUIRED");

        // Resolve which direction adapter should handle this source→target pair.
        var directionResult = ResolveDirection(request.Input.SourceLanguage, request.Input.TargetLanguage);
        if (!directionResult.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(directionResult.ErrorMessage, directionResult.ErrorCode);

        var direction = directionResult.Value!;

        // Look up the adapter for the resolved direction.
        if (!_adapters.TryGetValue(direction, out var adapter))
            return OperationResult<LogicalFlowResult>.Fail(
                $"No adapter registered for direction '{direction}'.", "ADAPTER_NOT_FOUND");

        // Delegate the actual translation to the direction adapter.
        var adapterResult = await adapter.TranslateAsync(request.Input, cancellationToken);
        if (!adapterResult.IsSuccess)
            return adapterResult.MapError<LogicalFlowResult>();

        var translationResult = adapterResult.Value!;

        // Build a minimal LogicalFlowSummary from the translation outcome.
        // A Bootstrap wrapper preserves the source but supplies no translated answer, even at neutral confidence.
        var passed = translationResult.Confidence > 0.0 &&
            !translationResult.AdapterId.StartsWith("bootstrap-", StringComparison.Ordinal);
        var summary = new LogicalFlowSummary(
            SummaryId: $"{FlowId}-{request.RunId}",
            Tier: 1,
            BaselinePrimary: 0,
            EnhancedPrimary: translationResult.Confidence,
            PrimaryMetricName: "translation_confidence",
            LowerIsBetter: false,
            Passed: passed,
            ShouldScaleUp: false,
            NextAction: passed ? "return_translation" : "request_more_context",
            ScalingSignal: 0,
            ScalingSignalNote: "",
            Notes: $"direction={direction}; adapter_id={translationResult.AdapterId}; topic={request.Input.TopicDomain}",
            BaselineTimeMs: 0,
            EnhancedTimeMs: 0);

        // Build the metrics dictionary for traceability and monitoring.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["direction"]            = direction.ToString(),
            ["adapter_id"]           = translationResult.AdapterId,
            ["source_language"]      = request.Input.SourceLanguage,
            ["target_language"]      = request.Input.TargetLanguage,
            ["topic_domain"]         = request.Input.TopicDomain,
            ["confidence"]           = translationResult.Confidence.ToString("F4", CultureInfo.InvariantCulture),
            ["input_length"]         = request.Input.InputText.Length.ToString(CultureInfo.InvariantCulture),
            ["translated_length"]    = translationResult.TranslatedText.Length.ToString(CultureInfo.InvariantCulture)
        };

        // Build and return the LogicalFlowResult carrying the translated text.
        var flowResult = new LogicalFlowResult(
            FlowId: FlowId,
            ScenarioId: ScenarioId,
            OverallVerdict: passed ? "TRANSLATED" : "TRANSLATION_LOW_CONFIDENCE",
            FinalHumanResult: translationResult.TranslatedText,
            Summary: summary,
            Metrics: metrics,
            EvidenceIds: [$"translation-adapter:{translationResult.AdapterId}"],
            Warnings: passed ? [] : ["Low confidence — semantic translation quality has not been established."]);

        return OperationResult<LogicalFlowResult>.Ok(flowResult);
    }

    // ResolveDirection maps a (sourceLanguage, targetLanguage) pair to a TranslationDirection enum value.
    // Supports case-insensitive ISO-639-1 codes (e.g. "ru", "RU", "Ru" are all valid for Russian).
    private static OperationResult<TranslationDirection> ResolveDirection(string sourceLang, string targetLang)
    {
        // Normalize to lower-case for consistent comparison regardless of caller casing.
        var src = sourceLang?.Trim().ToLowerInvariant() ?? "";
        var tgt = targetLang?.Trim().ToLowerInvariant() ?? "";

        // Reject empty or missing language codes before attempting direction lookup.
        if (string.IsNullOrWhiteSpace(src))
            return OperationResult<TranslationDirection>.Fail("SourceLanguage is required.", "SOURCE_LANGUAGE_REQUIRED");
        if (string.IsNullOrWhiteSpace(tgt))
            return OperationResult<TranslationDirection>.Fail("TargetLanguage is required.", "TARGET_LANGUAGE_REQUIRED");

        // Map the four supported direction pairs to their enum values.
        return (src, tgt) switch
        {
            ("ru", "en") => OperationResult<TranslationDirection>.Ok(TranslationDirection.RU_EN),
            ("en", "ru") => OperationResult<TranslationDirection>.Ok(TranslationDirection.EN_RU),
            ("he", "en") => OperationResult<TranslationDirection>.Ok(TranslationDirection.HE_EN),
            ("en", "he") => OperationResult<TranslationDirection>.Ok(TranslationDirection.EN_HE),
            ("es", "en") => OperationResult<TranslationDirection>.Ok(TranslationDirection.ES_EN),
            ("en", "es") => OperationResult<TranslationDirection>.Ok(TranslationDirection.EN_ES),
            _            => OperationResult<TranslationDirection>.Fail(
                $"Unsupported translation direction: '{src}' → '{tgt}'. Supported: ru↔en, he↔en, es↔en.",
                "UNSUPPORTED_DIRECTION")
        };
    }

    // BuildBootstrapAdapters returns all four direction adapters using the Bootstrap implementations.
    private static IReadOnlyDictionary<TranslationDirection, ITranslationDirectionAdapter> BuildBootstrapAdapters()
    {
        // Return one Bootstrap adapter per direction.
        return new Dictionary<TranslationDirection, ITranslationDirectionAdapter>
        {
            [TranslationDirection.RU_EN] = new BootstrapTranslationAdapter(TranslationDirection.RU_EN),
            [TranslationDirection.EN_RU] = new BootstrapTranslationAdapter(TranslationDirection.EN_RU),
            [TranslationDirection.HE_EN] = new BootstrapTranslationAdapter(TranslationDirection.HE_EN),
            [TranslationDirection.EN_HE] = new BootstrapTranslationAdapter(TranslationDirection.EN_HE),
            [TranslationDirection.ES_EN] = new BootstrapTranslationAdapter(TranslationDirection.ES_EN),
            [TranslationDirection.EN_ES] = new BootstrapTranslationAdapter(TranslationDirection.EN_ES)
        };
    }
}
