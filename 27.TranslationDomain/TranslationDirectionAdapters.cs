// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

// This file defines the direction adapter contract and Bootstrap implementation.
// Each of the four direction adapters (RU_EN, EN_RU, HE_EN, EN_HE) implements
// ITranslationDirectionAdapter. The Bootstrap adapters preserve the request
// structure and return a well-formed result so DPO training can proceed
// without requiring pre-trained model weights.
// Governed by: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md Direction 1
//              and decision A-LM-03 (Bootstrap preserves structure, no behaviour change).

using AgentInfrastructure; // OperationResult<T>

// ITranslationDirectionAdapter is the executable cell for one translation direction.
// Implementations may be Bootstrap (template-based) or trained (DPO-weighted).
public interface ITranslationDirectionAdapter
{
    // Direction identifies which of the four supported pairs this adapter handles.
    TranslationDirection Direction { get; }

    // AdapterId is the stable string key used in metrics and model manifests.
    string AdapterId { get; }

    // TranslateAsync applies this adapter's logic to the request and returns a result.
    Task<OperationResult<TranslationRoutingResult>> TranslateAsync(
        TranslationRoutingRequest request,
        CancellationToken cancellationToken = default);
}

// BootstrapTranslationAdapter is the default implementation before DPO (Direct Preference
// Optimization) training produces learned weights. It returns a structured result with
// a placeholder translation so the flow contract is fully exercised and testable.
// Per A-LM-03: the Bootstrap preserves the exact structure that trained adapters will use.
public sealed class BootstrapTranslationAdapter : ITranslationDirectionAdapter
{
    // Direction identifies the specific source→target language pair this adapter covers.
    public TranslationDirection Direction { get; }

    // AdapterId follows the naming pattern "bootstrap-{direction_lowercase}" for discoverability.
    public string AdapterId { get; }

    // Create the Bootstrap adapter for the specified direction.
    public BootstrapTranslationAdapter(TranslationDirection direction)
    {
        // Store the direction so TranslateAsync can embed it in the result.
        Direction = direction;

        // Build the adapter id from the direction name in lower-case (e.g. "bootstrap-ru_en").
        AdapterId = $"bootstrap-{direction.ToString().ToLowerInvariant()}";
    }

    // TranslateAsync returns a Bootstrap result: the input text is preserved with a wrapper
    // indicating this is a Bootstrap pass. The confidence is 0.5 (neutral placeholder)
    // until DPO training produces a real scoring model.
    public Task<OperationResult<TranslationRoutingResult>> TranslateAsync(
        TranslationRoutingRequest request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the result envelope when the token is cancelled.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<TranslationRoutingResult>.Fail("Translation cancelled.", "CANCELLED"));

        // Require a non-null request at the adapter boundary.
        if (request is null)
            return Task.FromResult(OperationResult<TranslationRoutingResult>.Fail(
                "Translation request is required.", "REQUEST_REQUIRED"));

        // Reject empty input text before attempting translation.
        if (string.IsNullOrWhiteSpace(request.InputText))
            return Task.FromResult(OperationResult<TranslationRoutingResult>.Fail(
                "InputText cannot be empty.", "INPUT_TEXT_REQUIRED"));

        // Build the Bootstrap translation placeholder.
        // Format: "[{DIRECTION}:BOOTSTRAP] {original text}"
        // This format is intentionally simple and will be replaced by DPO-trained output.
        var translatedText = $"[{Direction}:BOOTSTRAP] {request.InputText}";

        // Return the structured result with 0.5 confidence (neutral Bootstrap baseline).
        var result = new TranslationRoutingResult(
            Direction: Direction,
            TranslatedText: translatedText,
            Confidence: 0.5,
            AdapterId: AdapterId);

        return Task.FromResult(OperationResult<TranslationRoutingResult>.Ok(result));
    }
}

internal sealed class TrainedTranslationAdapter : ITranslationDirectionAdapter
{
    private readonly SemanticTokenizerUnit _semanticTokenizer;
    private readonly ExecutorUnit _executor;
    private readonly QualityVerifierUnit _verifier;
    private readonly BootstrapTranslationAdapter _fallback;

    public TranslationDirection Direction { get; }

    public string AdapterId => $"trained-{Direction.ToString().ToLowerInvariant()}";

    public TrainedTranslationAdapter(
        TranslationDirection direction,
        SemanticTokenizerUnit semanticTokenizer,
        ExecutorUnit executor,
        QualityVerifierUnit verifier)
    {
        Direction = direction;
        _semanticTokenizer = semanticTokenizer ?? throw new ArgumentNullException(nameof(semanticTokenizer));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
        _fallback = new BootstrapTranslationAdapter(direction);
    }

    public Task<OperationResult<TranslationRoutingResult>> TranslateAsync(
        TranslationRoutingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<TranslationRoutingResult>.Fail("Translation cancelled.", "CANCELLED"));

        var tokens = _semanticTokenizer.Tokenize(request.InputText, request.SourceLanguage);
        var candidate = _executor.Execute(tokens, request.TopicDomain);
        var verification = _verifier.Verify(candidate.Text, tokens);

        var phraseMapCandidate = candidate.AdapterId.EndsWith("-phrase-map", StringComparison.Ordinal);
        if (candidate.Confidence >= 0.5 && (verification.Pass || (phraseMapCandidate && candidate.Confidence >= 0.85)))
        {
            var result = new TranslationRoutingResult(
                Direction,
                candidate.Text,
                Math.Min(0.99, Math.Max(candidate.Confidence, verification.Score)),
                candidate.AdapterId);
            return Task.FromResult(OperationResult<TranslationRoutingResult>.Ok(result));
        }

        return _fallback.TranslateAsync(request, cancellationToken);
    }
}
