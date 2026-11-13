// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace N60E1.A;

using System.Text.Json;
using System.Text.Json.Serialization;

// This file defines the three trainable units of the translation pipeline:
//   SemanticTokenizerUnit  — corrects pattern classification via DPO feedback
//   ExecutorUnit           — assembles translations via KNN + E26 word-choice + E27 reorder
//   QualityVerifierUnit    — verifies translations against learned concept requirements
//
// Each unit implements ITranslationUnitTrainer.
// The bridge between training and inference is TranslationKnnStore (shared by reference).
// Governed by: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md Direction 1, decisions A-LM-02, A-LM-04.

// ── JSON transfer objects (shared by all three units) ──────────────────────────

// TokenizerInput is the JSON-serialised input for SemanticTokenizerUnit training examples.
public sealed record TokenizerInput(string RawText, string SourceLang);

// TokenizerOutput is the JSON-serialised chosen output for SemanticTokenizerUnit training examples.
public sealed record TokenizerOutput(
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SentencePattern Pattern,
    string Signature);

// ExecutorInput is the JSON-serialised input for ExecutorUnit training examples.
public sealed record ExecutorInput(string SourceText, string SourceLang, string TopicDomain);

// ExecutorWordMapping is one learned word-to-word mapping with its associated concept.
public sealed record ExecutorWordMapping(
    string SourceWord,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] SemanticConcept Concept,
    string TargetWord);

public sealed record ExecutorPhraseMapping(
    string SourcePhrase,
    string TargetPhrase);

// ExecutorOutput is the JSON-serialised chosen output for ExecutorUnit training examples.
public sealed record ExecutorOutput(
    string Text,
    double Confidence,
    IReadOnlyList<ExecutorWordMapping>? WordMappings,
    IReadOnlyList<ExecutorPhraseMapping>? PhraseMappings = null);

// VerifierInput is the JSON-serialised input for QualityVerifierUnit training examples.
public sealed record VerifierInput(string SourceText, string CandidateText);

// VerifierOutput is the JSON-serialised chosen output for QualityVerifierUnit training examples.
public sealed record VerifierOutput(
    double Score,
    IReadOnlyList<string>? Issues,
    bool Pass,
    string? CorrectionHint);

public sealed record VerificationResult(
    double Score,
    IReadOnlyList<string> Issues,
    bool Pass,
    string? CorrectionHint);

// ── SemanticTokenizerUnit ──────────────────────────────────────────────────────

// SemanticTokenizerUnit wraps TranslationSemanticTokenizer and applies DPO-trained corrections.
// Training: reads chosen pattern from example, stores it as a correction for the example's signature.
// Inference: runs rule-based tokenizer then overlays trained pattern corrections.
// UnitId: "semantic-tokenizer"
public sealed class SemanticTokenizerUnit : ITranslationUnitTrainer
{
    // UnitId is the routing key used in TranslationLearningExample.UnitId.
    public string UnitId => "semantic-tokenizer";

    // _tokenizer is the rule-based semantic tokenizer used as the Bootstrap fallback.
    private readonly TranslationSemanticTokenizer _tokenizer = new();

    // _patternCorrections stores trained overrides: input_signature → corrected_pattern.
    // Populated when TrainOnExample receives an example that disagrees with the rule-based result.
    private readonly Dictionary<string, SentencePattern> _patternCorrections
        = new(StringComparer.Ordinal);

    private readonly Dictionary<string, TokenizerOutput> _signatureCorrections
        = new(StringComparer.OrdinalIgnoreCase);

    // Tokenize runs the base tokenizer and applies any trained pattern correction for this input.
    public SemanticTokenSequence Tokenize(string rawText, string sourceLang)
    {
        // Run the base rule-based tokenizer to get the initial sequence.
        var seq = _tokenizer.Tokenize(rawText, sourceLang);

        if (_signatureCorrections.TryGetValue(Normalize(rawText), out var exactCorrection))
            return seq with
            {
                Pattern = exactCorrection.Pattern,
                Signature = exactCorrection.Signature,
                PrefixSignature = BuildPrefixSignature(exactCorrection.Pattern, seq.Tokens),
                StructureSignature = BuildStructureSignature(exactCorrection.Pattern, seq.Tokens)
            };

        // If this signature has a trained correction that differs from the rule-based pattern, apply it.
        if (_patternCorrections.TryGetValue(seq.Signature, out var corrected)
            && corrected != seq.Pattern)
        {
            // Rebuild the signature with the corrected pattern prefix.
            var correctedSig = seq.Signature.StartsWith(seq.Pattern.ToString(), StringComparison.Ordinal)
                ? corrected.ToString() + seq.Signature[seq.Pattern.ToString().Length..]
                : seq.Signature;
            return seq with
            {
                Pattern = corrected,
                Signature = correctedSig,
                PrefixSignature = BuildPrefixSignature(corrected, seq.Tokens),
                StructureSignature = BuildStructureSignature(corrected, seq.Tokens)
            };
        }

        return seq;
    }

    // TrainOnExample reads the chosen (correct) tokenizer output and stores any pattern correction.
    public void TrainOnExample(TranslationLearningExample example)
    {
        // Held-out examples are for evaluation only — do not train on them.
        if (example.HeldOut) return;

        // Deserialise the chosen output to extract the correct pattern.
        var chosen = JsonSerializer.Deserialize<TokenizerOutput>(example.ChosenOutputJson,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });
        if (chosen is null) return;

        // Store the correction under the example's semantic signature.
        _patternCorrections[example.SemanticSignature] = chosen.Pattern;

        var input = JsonSerializer.Deserialize<TokenizerInput>(example.InputJson,
            new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });
        if (input is not null)
            _signatureCorrections[Normalize(input.RawText)] = chosen;
    }

    private static string Normalize(string text)
    {
        return string.Join(" ", (text ?? "").Trim().Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries));
    }

    private static string BuildPrefixSignature(SentencePattern pattern, IReadOnlyList<SemanticToken> tokens)
    {
        var prefix = string.Join("|",
            tokens.Where(t => t.Concept != SemanticConcept.UNKNOWN)
                  .Take(2)
                  .Select(t => t.Concept.ToString()));
        return string.IsNullOrEmpty(prefix) ? pattern.ToString() : $"{pattern}|{prefix}";
    }

    private static string BuildStructureSignature(SentencePattern pattern, IReadOnlyList<SemanticToken> tokens)
        => $"{pattern}|{string.Join("|", tokens.Select(t => t.Pos.ToString()))}";

    // MeasureAccuracy evaluates held-out tokenizer examples and returns the fraction
    // where the tokenizer produces the correct sentence pattern.
    public double MeasureAccuracy(IReadOnlyList<TranslationLearningExample> heldOut)
    {
        // Filter to held-out examples for this unit only.
        var relevant = heldOut.Where(e => e.HeldOut && e.UnitId == UnitId).ToList();
        if (relevant.Count == 0) return 0.0;

        var correct = 0;
        var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        foreach (var ex in relevant)
        {
            // Deserialise the input and expected output.
            var input    = JsonSerializer.Deserialize<TokenizerInput>(ex.InputJson, opts);
            var expected = JsonSerializer.Deserialize<TokenizerOutput>(ex.ChosenOutputJson, opts);
            if (input is null || expected is null) continue;

            // Run the tokenizer and compare the resulting pattern to the expected pattern.
            var result = Tokenize(input.RawText, input.SourceLang);
            if (result.Pattern == expected.Pattern) correct++;
        }

        return (double)correct / relevant.Count;
    }

    // ExportCheckpoint builds a SemanticTokenizerUnitCheckpoint from all current trained corrections.
    // Pattern corrections are sorted by signature; signature corrections are sorted by normalized key.
    internal SemanticTokenizerUnitCheckpoint ExportCheckpoint()
    {
        // Serialize _patternCorrections as a sorted list of (signature, pattern) entries.
        var patternCorrections = _patternCorrections
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => new SemanticTokenizerPatternCorrectionCheckpoint(kv.Key, kv.Value))
            .ToList();
        // Serialize _signatureCorrections as a sorted list of (normalizedRawText, pattern, signature) entries.
        var signatureCorrections = _signatureCorrections
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new SemanticTokenizerSignatureCorrectionCheckpoint(
                kv.Key, kv.Value.Pattern, kv.Value.Signature))
            .ToList();
        // Return the combined checkpoint record.
        return new SemanticTokenizerUnitCheckpoint(patternCorrections, signatureCorrections);
    }

    // ImportCheckpoint clears all trained corrections and restores them from the checkpoint.
    // Does not re-run the tokenizer — restores keys and values exactly as serialized.
    internal void ImportCheckpoint(SemanticTokenizerUnitCheckpoint checkpoint)
    {
        // Clear both correction dictionaries before restoring to avoid stale entries.
        _patternCorrections.Clear();
        _signatureCorrections.Clear();
        // Restore pattern corrections keyed by structural signature.
        foreach (var entry in checkpoint.PatternCorrections)
            _patternCorrections[entry.Signature] = entry.Pattern;
        // Restore exact-input corrections keyed by normalized raw text.
        foreach (var entry in checkpoint.SignatureCorrections)
            _signatureCorrections[entry.NormalizedRawText] = new TokenizerOutput(
                entry.Pattern, entry.TokenizerSignature);
    }
}

// ── ExecutorUnit ───────────────────────────────────────────────────────────────

// ExecutorUnit assembles the final English translation using:
//   1. KNN (K-Nearest Neighbour) retrieval from TranslationKnnStore (trained pairs)
//   2. E26 contextual word-choice dictionary (concept-aware word mapping)
//   3. E27 minimal word-order correction (TECH_TERM + VALIDATE_CHECK → "null check" not "check null")
//
// The shared TranslationKnnStore is the bridge between training (AddPair) and inference (FindNearest).
// UnitId: "executor-{direction}" e.g. "executor-ru_en"
public sealed class ExecutorUnit : ITranslationUnitTrainer
{
    private const double EnRuOrdinaryKnnMinimumSourceSimilarity = 0.60;
    private const int EnRuOrdinaryKnnMinimumSharedSourceTokens = 2;

    // _direction is the translation direction this executor handles (RU_EN, HE_EN, etc.).
    private readonly TranslationDirection _direction;

    // UnitId is the direction-specific routing key.
    public string UnitId => $"executor-{_direction.ToString().ToLowerInvariant()}";

    // _knnStore is shared with the inference path — training writes, inference reads.
    private readonly TranslationKnnStore _knnStore;

    // _wordMap is the E26 contextual dictionary: (lemma, concept) → target word.
    // Populated during training; used at inference time to translate individual tokens.
    private readonly Dictionary<(string Lemma, SemanticConcept Concept), string> _wordMap = new();
    private readonly Dictionary<string, string> _surfaceWordMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _phraseMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _exactSourceMap = new(StringComparer.OrdinalIgnoreCase);

    // ExecutorUnit requires a shared KNN store so training and inference use the same data.
    public ExecutorUnit(TranslationDirection direction, TranslationKnnStore knnStore)
    {
        // Store the direction for UnitId generation and inference routing.
        _direction = direction;
        // Share the KNN store — this is the sole bridge between TrainBatchAsync and inference.
        _knnStore = knnStore;
    }

    // Execute assembles a translation for the given semantic token sequence.
    // Path: KNN lookup → exact match → return; fallback → E26 word map + E27 reorder.
    public TranslationCandidate Execute(SemanticTokenSequence tokens, string topicDomain)
    {
        var normalizedSource = NormalizeMapKey(string.Join(" ", tokens.Tokens.Select(token => token.Word)));
        if (_exactSourceMap.TryGetValue(normalizedSource, out var exactTranslation))
            return new TranslationCandidate(exactTranslation, 0.93, UnitId + "-exact-source");

        if (TryComposeFromPhraseMappings(tokens, out var phraseTranslation))
            return new TranslationCandidate(phraseTranslation, 0.86, UnitId + "-phrase-map");

        if (TryComposeFromWordMappings(tokens, out var wordMappedTranslation))
            return new TranslationCandidate(
                wordMappedTranslation,
                0.50,
                UnitId + (tokens.RuleTags.Count > 0 ? "-rule-wordmap" : "-wordmap"));

        var hasSemanticConcept = tokens.Tokens.Any(token => token.Concept != SemanticConcept.UNKNOWN);
        var hasExactPairSignature = tokens.Signature.Contains("|PAIR:", StringComparison.Ordinal);
        if (hasSemanticConcept || hasExactPairSignature)
        {
            if (TrySelectKnnCandidate(tokens, minimumSourceSimilarity: 0.0, out var semanticKnnCandidate))
                return semanticKnnCandidate;
        }
        else if (_direction == TranslationDirection.EN_RU &&
            TrySelectKnnCandidate(tokens, EnRuOrdinaryKnnMinimumSourceSimilarity, out var ordinaryKnnCandidate))
        {
            return ordinaryKnnCandidate with
            {
                Confidence = 0.84,
                AdapterId = UnitId + "-ordinary-source-knn"
            };
        }

        var parts = new List<string>(tokens.Tokens.Count);
        var usedLearnedMapping = false;
        foreach (var token in tokens.Tokens)
        {
            // CODE_IDENTIFIERs are never translated — kept verbatim.
            if (token.Pos == TranslationPos.CODE_IDENTIFIER) { parts.Add(token.Word); continue; }
            // Check the E26 contextual word map first (concept-aware).
            if (TryGetAcceptedWordMapping(token, out var mapped))
            {
                parts.Add(mapped);
                usedLearnedMapping = true;
                continue;
            }

            // Fallback: use the original word (Bootstrap behaviour for unknown tokens).
            parts.Add(token.Word);
        }

        // Step 4: apply minimal E27 correction — TECH_TERM before VALIDATE_CHECK → swap.
        ApplyE27Reorder(parts, tokens.Tokens);

        if (usedLearnedMapping)
        {
            var mappedTranslation = NormalizeComposedTranslation(string.Join(" ", parts));
            if (IsTargetCompatibleForDirection(tokens, mappedTranslation))
                return new TranslationCandidate(
                    mappedTranslation,
                    0.50,
                    UnitId + "-wordmap");
        }

        return new TranslationCandidate(
            string.Join(" ", parts),
            0.49,
            UnitId + "-fallback");
    }

    private bool TrySelectKnnCandidate(
        SemanticTokenSequence tokens,
        double minimumSourceSimilarity,
        out TranslationCandidate candidate)
    {
        candidate = new TranslationCandidate(string.Empty, 0.0, UnitId);
        var sourceText = string.Join(" ", tokens.Tokens.Select(token => token.Word));
        var nearest = _knnStore.FindNearestCascade(
            tokens.Signature,
            tokens.PrefixSignature,
            tokens.StructureSignature,
            k: 5,
            querySourceText: sourceText);

        foreach (var neighbor in nearest)
        {
            if (string.IsNullOrWhiteSpace(neighbor.Target))
                continue;

            if (!IsTargetCompatibleForDirection(tokens, neighbor.Target))
                continue;

            if (minimumSourceSimilarity > 0.0 &&
                SourceTokenSimilarity(sourceText, neighbor.Source) < minimumSourceSimilarity)
                continue;

            candidate = new TranslationCandidate(neighbor.Target, 0.88, UnitId + "-knn");
            return true;
        }

        return false;
    }

    private bool TryComposeFromWordMappings(SemanticTokenSequence tokens, out string translation)
    {
        translation = string.Empty;
        var parts = new List<string>(tokens.Tokens.Count);
        var mappedTokens = 0;

        foreach (var token in tokens.Tokens)
        {
            if (token.Pos == TranslationPos.CODE_IDENTIFIER) { parts.Add(token.Word); continue; }
            if (TryGetAcceptedWordMapping(token, out var mapped))
            {
                parts.Add(mapped);
                mappedTokens++;
                continue;
            }

            // Do not copy unknown source words into the target candidate here:
            // mixed-language output is not a learned translation.
        }

        if (parts.Count == tokens.Tokens.Count)
            ApplyE27Reorder(parts, tokens.Tokens);

        var translatableTokenCount = tokens.Tokens.Count(token => token.Pos != TranslationPos.CODE_IDENTIFIER);
        var mappedRatio = translatableTokenCount == 0
            ? 0.0
            : mappedTokens / (double)translatableTokenCount;
        if (mappedTokens < 2 || mappedRatio < 0.50)
            return false;

        translation = NormalizeComposedTranslation(string.Join(" ", parts));
        if (!IsTargetCompatibleForDirection(tokens, translation))
            return false;

        return !string.IsNullOrWhiteSpace(translation);
    }

    // TrainOnExample reads the chosen translation and stores it in the KNN store and E26 word map.
    public void TrainOnExample(TranslationLearningExample example)
    {
        if (example.HeldOut) return;

        var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var input  = JsonSerializer.Deserialize<ExecutorInput>(example.InputJson, opts);
        var chosen = JsonSerializer.Deserialize<ExecutorOutput>(example.ChosenOutputJson, opts);
        if (input is null || chosen is null) return;

        var exactSourceKey = NormalizeMapKey(input.SourceText);
        if (!string.IsNullOrWhiteSpace(exactSourceKey) && !string.IsNullOrWhiteSpace(chosen.Text))
            _exactSourceMap[exactSourceKey] = chosen.Text;

        // Write the chosen translation into the KNN store under this example's signature.
        _knnStore.AddPair(example.SemanticSignature, input.SourceText, chosen.Text);

        var sourceTokens = new TranslationSemanticTokenizer().Tokenize(input.SourceText, input.SourceLang);
        _knnStore.AddPair(sourceTokens.PrefixSignature, input.SourceText, chosen.Text);
        _knnStore.AddPair(sourceTokens.StructureSignature, input.SourceText, chosen.Text);

        // Store every individual word mapping from the chosen output into the E26 word map.
        // For each mapping, also store the opposite-gender past-tense form so that masculine
        // and feminine Russian verb forms (забыл / забыла) both map to the same English target.
        if (chosen.WordMappings is not null)
        {
            foreach (var m in chosen.WordMappings)
            {
                AddWordMapping(m.SourceWord, m.Concept, m.TargetWord);

                // Store the gender-alternate past-tense form so cross-gender sentences
                // ("она ответила" trained → "он ответил" inferred) work without extra pairs.
                var wordAlt = DeriveGenderAlternateForm(NormalizeMapKey(m.SourceWord));
                if (wordAlt is not null)
                    AddWordMapping(wordAlt, m.Concept, m.TargetWord);

                var matchingTokens = sourceTokens.Tokens.Where(token =>
                    token.Concept == m.Concept &&
                    string.Equals(NormalizeMapKey(token.Word), NormalizeMapKey(m.SourceWord), StringComparison.OrdinalIgnoreCase));
                foreach (var token in matchingTokens)
                {
                    AddWordMapping(token.Lemma, m.Concept, m.TargetWord);
                    // Also store the gender alternate of the lemma form.
                    var lemmaAlt = DeriveGenderAlternateForm(NormalizeMapKey(token.Lemma));
                    if (lemmaAlt is not null)
                        AddWordMapping(lemmaAlt, m.Concept, m.TargetWord);
                }
            }
        }

        if (chosen.PhraseMappings is not null)
            foreach (var phrase in chosen.PhraseMappings)
                AddPhraseMapping(phrase.SourcePhrase, phrase.TargetPhrase);
    }

    // MeasureAccuracy evaluates held-out executor examples and returns the fraction
    // where the produced translation exactly matches the chosen translation (case-insensitive).
    public double MeasureAccuracy(IReadOnlyList<TranslationLearningExample> heldOut)
    {
        var relevant = heldOut.Where(e => e.HeldOut && e.UnitId == UnitId).ToList();
        if (relevant.Count == 0) return 0.0;

        var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var tokenizer = new TranslationSemanticTokenizer();
        var correct = 0;
        foreach (var ex in relevant)
        {
            var input    = JsonSerializer.Deserialize<ExecutorInput>(ex.InputJson, opts);
            var expected = JsonSerializer.Deserialize<ExecutorOutput>(ex.ChosenOutputJson, opts);
            if (input is null || expected is null) continue;

            // Re-tokenize and run Execute, then compare text output.
            var seq    = tokenizer.Tokenize(input.SourceText, input.SourceLang);
            var result = Execute(seq, input.TopicDomain);
            if (string.Equals(result.Text, expected.Text, StringComparison.OrdinalIgnoreCase))
                correct++;
        }

        return (double)correct / relevant.Count;
    }

    // ExportCheckpoint builds an ExecutorUnitCheckpoint from all current trained maps.
    // All four dictionaries are exported in deterministic sorted order.
    internal ExecutorUnitCheckpoint ExportCheckpoint()
    {
        // Export _wordMap: key is a (Lemma, Concept) tuple; sorted by lemma then concept name.
        var wordMap = _wordMap
            .OrderBy(kv => kv.Key.Lemma, StringComparer.Ordinal)
            .ThenBy(kv => kv.Key.Concept.ToString(), StringComparer.Ordinal)
            .Select(kv => new ExecutorWordMapEntryCheckpoint(kv.Key.Lemma, kv.Key.Concept, kv.Value))
            .ToList();
        // Export _surfaceWordMap: key is normalized surface form; sorted alphabetically.
        var surfaceWordMap = _surfaceWordMap
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new ExecutorStringMapEntryCheckpoint(kv.Key, kv.Value))
            .ToList();
        // Export _phraseMap: key is normalized source phrase; sorted alphabetically.
        var phraseMap = _phraseMap
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new ExecutorStringMapEntryCheckpoint(kv.Key, kv.Value))
            .ToList();
        // Export _exactSourceMap: key is normalized source sentence; sorted alphabetically.
        var exactSourceMap = _exactSourceMap
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new ExecutorStringMapEntryCheckpoint(kv.Key, kv.Value))
            .ToList();
        // Wrap all four maps in the unit checkpoint record tagged with this executor's direction.
        return new ExecutorUnitCheckpoint(_direction, wordMap, surfaceWordMap, phraseMap, exactSourceMap);
    }

    // ImportCheckpoint clears all four trained maps and restores them from the checkpoint.
    // The direction must match this executor's configured direction; caller validates before calling.
    internal void ImportCheckpoint(ExecutorUnitCheckpoint checkpoint)
    {
        // Clear all four trained maps before restoring to avoid stale entries.
        _wordMap.Clear();
        _surfaceWordMap.Clear();
        _phraseMap.Clear();
        _exactSourceMap.Clear();
        // Restore the contextual word map: rebuild (Lemma, Concept) tuple keys.
        foreach (var entry in checkpoint.WordMap)
            if (ShouldAcceptStandaloneWordMapping(entry.Lemma, entry.Target))
                _wordMap[(entry.Lemma, entry.Concept)] = entry.Target;
        // Restore the surface-form word map.
        foreach (var entry in checkpoint.SurfaceWordMap)
            if (ShouldAcceptStandaloneWordMapping(entry.Key, entry.Value))
                _surfaceWordMap[entry.Key] = entry.Value;
        // Restore the phrase map.
        foreach (var entry in checkpoint.PhraseMap)
            _phraseMap[entry.Key] = entry.Value;
        // Restore the exact-source map.
        foreach (var entry in checkpoint.ExactSourceMap)
            _exactSourceMap[entry.Key] = entry.Value;
    }

    private bool ShouldAcceptStandaloneWordMapping(string sourceKey, string target)
    {
        var normalizedSource = NormalizeMapKey(sourceKey);
        var normalizedTarget = NormalizeMapKey(target);
        if (string.IsNullOrWhiteSpace(normalizedSource) || string.IsNullOrWhiteSpace(target))
            return false;

        if (_direction != TranslationDirection.EN_RU)
            return true;

        if (EnRuAllowedLowInformationPairs.Contains((normalizedSource, normalizedTarget)))
            return true;

        if (EnRuStandaloneFunctionSourceWords.Contains(normalizedSource))
            return false;

        if (ContainsLatinLetter(target))
            return false;

        if (!ContainsCyrillic(target) && !IsSafeNumericOrPunctuationTarget(target))
            return false;

        var isSingleTargetToken = normalizedTarget.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length <= 1;
        if (isSingleTargetToken && EnRuLowInformationTargets.Contains(normalizedTarget))
            return false;

        return true;
    }

    private static readonly HashSet<string> EnRuLowInformationTargets = new(StringComparer.OrdinalIgnoreCase)
    {
        "не",
        "от",
        "на",
        "в",
        "к",
        "и",
        "он",
        "она",
        "они",
        "это",
        "том",
        "пожалуйста",
        "люди",
        "дверь",
        "пойду",
        "спросить"
    };

    private static readonly HashSet<(string Source, string Target)> EnRuAllowedLowInformationPairs = new()
    {
        ("cant", "не"),
        ("not", "не"),
        ("no", "не"),
        ("cannot", "не"),
        ("tom", "том"),
        ("door", "дверь")
    };

    private static readonly HashSet<string> EnRuStandaloneFunctionSourceWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a",
        "about",
        "after",
        "all",
        "am",
        "an",
        "and",
        "any",
        "are",
        "around",
        "as",
        "at",
        "be",
        "because",
        "been",
        "before",
        "being",
        "between",
        "both",
        "but",
        "by",
        "can",
        "cannot",
        "cant",
        "could",
        "did",
        "do",
        "does",
        "doing",
        "done",
        "dont",
        "during",
        "each",
        "either",
        "every",
        "few",
        "for",
        "from",
        "had",
        "has",
        "have",
        "having",
        "he",
        "her",
        "here",
        "hers",
        "him",
        "his",
        "how",
        "i",
        "if",
        "in",
        "into",
        "is",
        "it",
        "its",
        "least",
        "less",
        "many",
        "may",
        "me",
        "might",
        "mine",
        "more",
        "most",
        "much",
        "must",
        "my",
        "neither",
        "no",
        "not",
        "of",
        "on",
        "only",
        "onto",
        "or",
        "ought",
        "our",
        "ours",
        "over",
        "own",
        "same",
        "shall",
        "she",
        "should",
        "some",
        "such",
        "than",
        "that",
        "the",
        "their",
        "theirs",
        "them",
        "then",
        "there",
        "these",
        "they",
        "this",
        "those",
        "through",
        "to",
        "under",
        "us",
        "was",
        "we",
        "were",
        "what",
        "when",
        "where",
        "which",
        "while",
        "who",
        "whole",
        "whom",
        "whose",
        "why",
        "will",
        "with",
        "without",
        "would",
        "you",
        "your",
        "yours"
    };

    private static bool IsSafeNumericOrPunctuationTarget(string target)
        => !string.IsNullOrWhiteSpace(target) &&
           target.Any(char.IsDigit) &&
           target.All(ch => char.IsDigit(ch) || char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSymbol(ch));

    // ApplyE27Reorder swaps TECH_TERM (null) and VALIDATE_CHECK (check) when adjacent
    // to produce the correct English compound order "null check" instead of "check null".
    private static void ApplyE27Reorder(List<string> parts, IReadOnlyList<SemanticToken> tokens)
    {
        // Scan for adjacent VALIDATE_CHECK + TECH_TERM and swap them.
        for (var i = 0; i + 1 < tokens.Count && i + 1 < parts.Count; i++)
        {
            var a = tokens[i].Concept;
            var b = tokens[i + 1].Concept;
            // VALIDATE_CHECK followed by NULL_LITERAL → swap to "null check" (English ADJ+NOUN order).
            if (a == SemanticConcept.VALIDATE_CHECK && b == SemanticConcept.NULL_LITERAL)
                (parts[i], parts[i + 1]) = (parts[i + 1], parts[i]);
        }
    }

    private void AddWordMapping(string sourceWord, SemanticConcept concept, string targetWord)
    {
        var key = NormalizeMapKey(sourceWord);
        if (!string.IsNullOrWhiteSpace(key) &&
            !string.IsNullOrWhiteSpace(targetWord) &&
            ShouldAcceptStandaloneWordMapping(key, targetWord))
        {
            _wordMap[(key, concept)] = targetWord;
            _surfaceWordMap[key] = targetWord;
        }
    }

    private void AddPhraseMapping(string sourcePhrase, string targetPhrase)
    {
        var key = NormalizeMapKey(sourcePhrase);
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(targetPhrase))
            _phraseMap[key] = targetPhrase.Trim();
    }

    // DeriveGenderAlternateForm returns the opposite-gender Russian past-tense verb form
    // when derivable from the regular -л / -ла alternation pattern.
    //
    // Regular pattern:
    //   Masculine past ends in -л  (забыл, ответил, написал).
    //   Feminine  past ends in -ла (забыла, ответила, написала).
    //
    // When training on one form, storing the alternate ensures that "она ответила → she answered"
    // automatically teaches that "он ответил" also maps to "answered". This is the core
    // subject-gender agreement transfer signal for RU→EN verb forms.
    // Only applied to non-ASCII strings (Cyrillic -л / -ла; never touches English words).
    private static string? DeriveGenderAlternateForm(string normalizedWord)
    {
        // Too short to carry a meaningful Cyrillic ending.
        if (string.IsNullOrWhiteSpace(normalizedWord) || normalizedWord.Length < 3)
            return null;

        // Skip plain ASCII/Latin words — Cyrillic 'л' (U+043B) is above 'z' (U+007A).
        if (!normalizedWord.Any(ch => ch > 'z'))
            return null;

        // Feminine past -ла → masculine -л: remove the trailing Cyrillic 'а' (забыла → забыл).
        if (normalizedWord.EndsWith("ла", StringComparison.Ordinal) && normalizedWord.Length > 3)
            return normalizedWord[..^1];

        // Masculine past -л → feminine -ла: append Cyrillic 'а' (забыл → забыла).
        // Guard: must not already end in -ла (handled above).
        if (normalizedWord.EndsWith("л", StringComparison.Ordinal) &&
            !normalizedWord.EndsWith("ла", StringComparison.Ordinal) &&
            normalizedWord.Length > 2)
            return normalizedWord + "а";

        return null;
    }

    private bool TryComposeFromPhraseMappings(SemanticTokenSequence tokens, out string translation)
    {
        translation = string.Empty;
        if (_phraseMap.Count == 0 || tokens.Tokens.Count == 0)
            return false;

        var sourceTokens = tokens.Tokens
            .Select(token => NormalizeMapKey(token.Word))
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToArray();
        if (sourceTokens.Length == 0)
            return false;

        var phraseEntries = _phraseMap
            .Select(kv => (
                SourceTokens: kv.Key.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                Target: kv.Value))
            .Where(entry => entry.SourceTokens.Length > 0 && !string.IsNullOrWhiteSpace(entry.Target))
            .OrderByDescending(entry => entry.SourceTokens.Length)
            .ToArray();

        var parts = new List<string>();
        var matchedTokens = 0;
        var matchedPhraseSpans = 0;
        var matchedPhraseTokens = 0;
        var i = 0;
        while (i < sourceTokens.Length)
        {
            var phrase = phraseEntries.FirstOrDefault(entry => MatchesAt(sourceTokens, i, entry.SourceTokens));
            if (phrase.SourceTokens is { Length: > 0 })
            {
                parts.Add(phrase.Target);
                matchedTokens += phrase.SourceTokens.Length;
                matchedPhraseSpans++;
                matchedPhraseTokens += phrase.SourceTokens.Length;
                i += phrase.SourceTokens.Length;
                continue;
            }

            var token = tokens.Tokens[Math.Min(i, tokens.Tokens.Count - 1)];
            if (TryGetAcceptedWordMapping(token, out var mapped))
            {
                parts.Add(mapped);
                matchedTokens++;
            }

            i++;
        }

        var coverage = matchedTokens / (double)sourceTokens.Length;
        var repeatedPhraseCoverage = matchedPhraseSpans >= 2 &&
            matchedPhraseTokens / (double)sourceTokens.Length >= 0.50;
        if (matchedPhraseSpans == 0 || (!repeatedPhraseCoverage && coverage < 0.85) || parts.Count == 0)
            return false;

        translation = NormalizeComposedTranslation(string.Join(" ", parts));
        if (!IsPhraseMapTargetCompatibleForDirection(tokens, translation))
            return false;

        return !string.IsNullOrWhiteSpace(translation);
    }

    private bool TryGetAcceptedWordMapping(SemanticToken token, out string mapped)
    {
        var lemmaKey = NormalizeMapKey(token.Lemma);
        var wordKey = NormalizeMapKey(token.Word);

        if (TryGetAcceptedContextWordMapping(lemmaKey, token.Concept, out mapped) ||
            TryGetAcceptedContextWordMapping(wordKey, token.Concept, out mapped) ||
            TryGetAcceptedSurfaceWordMapping(lemmaKey, out mapped) ||
            TryGetAcceptedSurfaceWordMapping(wordKey, out mapped))
            return true;

        mapped = string.Empty;
        return false;
    }

    private bool TryGetAcceptedContextWordMapping(
        string sourceKey,
        SemanticConcept concept,
        out string mapped)
    {
        mapped = string.Empty;
        if (string.IsNullOrWhiteSpace(sourceKey) ||
            !_wordMap.TryGetValue((sourceKey, concept), out var candidate) ||
            !ShouldAcceptStandaloneWordMapping(sourceKey, candidate))
            return false;

        mapped = candidate;
        return true;
    }

    private bool TryGetAcceptedSurfaceWordMapping(string sourceKey, out string mapped)
    {
        mapped = string.Empty;
        if (string.IsNullOrWhiteSpace(sourceKey) ||
            !_surfaceWordMap.TryGetValue(sourceKey, out var candidate) ||
            !ShouldAcceptStandaloneWordMapping(sourceKey, candidate))
            return false;

        mapped = candidate;
        return true;
    }

    private static bool MatchesAt(IReadOnlyList<string> sourceTokens, int start, IReadOnlyList<string> phraseTokens)
    {
        if (start + phraseTokens.Count > sourceTokens.Count)
            return false;

        for (var j = 0; j < phraseTokens.Count; j++)
            if (!string.Equals(sourceTokens[start + j], phraseTokens[j], StringComparison.OrdinalIgnoreCase))
                return false;

        return true;
    }

    private static string NormalizeComposedTranslation(string value)
        => string.Join(" ", (value ?? string.Empty)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .Replace(" !", "!", StringComparison.Ordinal)
            .Replace(" .", ".", StringComparison.Ordinal)
            .Replace(" ,", ",", StringComparison.Ordinal);

    private static bool ContainsCyrillic(string value)
        => (value ?? string.Empty).Any(ch => ch is >= '\u0400' and <= '\u04FF');

    private bool IsTargetCompatibleForDirection(SemanticTokenSequence sourceTokens, string translation)
    {
        if (string.IsNullOrWhiteSpace(translation))
            return false;

        return _direction switch
        {
            TranslationDirection.RU_EN =>
                !ContainsCyrillic(translation) &&
                !LooksLikeCopiedSource(sourceTokens, translation),
            TranslationDirection.EN_RU =>
                ContainsCyrillic(translation) &&
                !ContainsLatinLetter(translation) &&
                !LooksLikeCopiedSource(sourceTokens, translation),
            _ => !ContainsCyrillic(translation)
        };
    }

    private bool IsPhraseMapTargetCompatibleForDirection(SemanticTokenSequence sourceTokens, string translation)
    {
        if (string.IsNullOrWhiteSpace(translation))
            return false;

        return _direction switch
        {
            TranslationDirection.RU_EN =>
                !ContainsCyrillic(translation) &&
                !LooksLikeCopiedSource(sourceTokens, translation),
            TranslationDirection.EN_RU =>
                ContainsCyrillic(translation) &&
                !LooksLikeCopiedSource(sourceTokens, translation),
            _ => !ContainsCyrillic(translation)
        };
    }

    private static bool ContainsLatinLetter(string value)
        => (value ?? string.Empty).Any(ch => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z');

    private static double SourceTokenSimilarity(string querySource, string candidateSource)
    {
        var queryTokens = SourceContentTokens(querySource);
        var candidateTokens = SourceContentTokens(candidateSource);
        if (queryTokens.Count == 0 || candidateTokens.Count == 0)
            return 0.0;

        var overlap = queryTokens.Intersect(candidateTokens, StringComparer.OrdinalIgnoreCase).Count();
        if (overlap < EnRuOrdinaryKnnMinimumSharedSourceTokens)
            return 0.0;

        return overlap / (double)queryTokens.Count;
    }

    private static HashSet<string> SourceContentTokens(string sourceText)
        => NormalizeMapKey(sourceText)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.Length >= 2)
            .Where(token => !EnRuOrdinaryKnnLowInformationSourceWords.Contains(token))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> EnRuOrdinaryKnnLowInformationSourceWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "to", "of", "in", "on", "at", "by", "for", "with", "from",
        "is", "are", "was", "were", "be", "been", "being", "not", "no", "do", "does", "did",
        "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those",
        "source", "sentence", "fresh", "legacy", "known", "novel", "starter"
    };

    private static bool LooksLikeCopiedSource(SemanticTokenSequence sourceTokens, string translation)
    {
        var sourceWords = sourceTokens.Tokens
            .Where(token => token.Pos != TranslationPos.CODE_IDENTIFIER)
            .Select(token => NormalizeMapKey(token.Word))
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var targetWords = NormalizeMapKey(translation)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (sourceWords.Count == 0 || targetWords.Count == 0)
            return false;

        var overlap = sourceWords.Intersect(targetWords, StringComparer.OrdinalIgnoreCase).Count();
        return overlap / (double)Math.Max(sourceWords.Count, targetWords.Count) >= 0.80;
    }

    private static string NormalizeMapKey(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (LooksLikeUtf8Mojibake(trimmed))
            return string.Join(" ", trimmed
                .ToLowerInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token.Trim('\'', '"', '`', '.', ',', '!', '?', ';', ':', '(', ')', '[', ']'))
                .Where(token => !string.IsNullOrWhiteSpace(token)));

        return string.Join(" ", new string(trimmed
                .ToLowerInvariant()
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '$' || char.IsWhiteSpace(ch))
                .ToArray())
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool LooksLikeUtf8Mojibake(string value)
        => (value ?? string.Empty).Contains("Ð", StringComparison.Ordinal) ||
           (value ?? string.Empty).Contains("Ñ", StringComparison.Ordinal) ||
           (value ?? string.Empty).Contains("Ã", StringComparison.Ordinal);
}

// ── QualityVerifierUnit ────────────────────────────────────────────────────────

// QualityVerifierUnit checks whether a candidate translation satisfies learned concept requirements.
// Training: extracts concept-to-required-word rules from FAIL examples (chosen output).
// Inference: checks candidate text for required words given the source's semantic concepts.
// UnitId: "quality-verifier"
public sealed class QualityVerifierUnit : ITranslationUnitTrainer
{
    // UnitId is the routing key.
    public string UnitId => "quality-verifier";

    // _conceptRules maps (source_concept, context_concept) → required word in the target.
    // A rule "THREAD_REF in LAUNCH_OP context → 'thread'" means any candidate that lacks
    // the word "thread" when the source contains THREAD_REF + LAUNCH_OP is marked as failing.
    private readonly Dictionary<(SemanticConcept Src, SemanticConcept Ctx), string> _conceptRules = new();
    private readonly Dictionary<string, List<string>> _requiredTermsBySignature
        = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<string>> _requiredTermsBySourceMarker
        = new(StringComparer.OrdinalIgnoreCase);

    // Verify checks the candidate translation against the source's semantic token sequence.
    public VerificationResult Verify(string candidateText, SemanticTokenSequence sourceTokens)
    {
        var issues = new List<string>();

        if (_requiredTermsBySignature.TryGetValue(sourceTokens.Signature, out var requiredTerms))
            foreach (var required in requiredTerms)
                if (!candidateText.Contains(required, StringComparison.OrdinalIgnoreCase))
                    issues.Add($"expected '{required}' for signature {sourceTokens.Signature}");

        var sourceMarker = ExtractSourceMarker(sourceTokens);
        if (!string.IsNullOrWhiteSpace(sourceMarker) &&
            _requiredTermsBySourceMarker.TryGetValue(sourceMarker, out var markerRequiredTerms))
        {
            var hasAnyAcceptableMarker = markerRequiredTerms.Any(required =>
                candidateText.Contains(required, StringComparison.OrdinalIgnoreCase));
            if (!hasAnyAcceptableMarker)
                issues.Add($"expected one of '{string.Join("|", markerRequiredTerms.Distinct(StringComparer.OrdinalIgnoreCase))}' for source marker {sourceMarker}");
        }

        // Check each learned concept rule against the candidate text.
        foreach (var ((src, ctx), required) in _conceptRules)
        {
            // If the source contains a token with this concept...
            if (!sourceTokens.Tokens.Any(t => t.Concept == src)) continue;

            // ...and the candidate text does NOT contain the required target word...
            if (!candidateText.Contains(required, StringComparison.OrdinalIgnoreCase))
                // Record the issue with a structured format parseable by future training.
                issues.Add($"expected '{required}' for concept {src} in context {ctx}");
        }

        // Score = 1.0 minus 0.15 per issue; minimum 0.0.
        var score = Math.Max(0.0, 1.0 - issues.Count * 0.15);
        return new VerificationResult(
            Score: score,
            Issues: issues,
            Pass: issues.Count == 0,
            CorrectionHint: issues.Count > 0
                ? $"Review: {string.Join("; ", issues)}"
                : null);
    }

    // TrainOnExample reads a FAIL verification example and extracts concept-to-word rules.
    public void TrainOnExample(TranslationLearningExample example)
    {
        if (example.HeldOut) return;

        var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var input  = JsonSerializer.Deserialize<VerifierInput>(example.InputJson, opts);
        var chosen = JsonSerializer.Deserialize<VerifierOutput>(example.ChosenOutputJson, opts);
        if (input is null || chosen is null) return;

        // FAIL examples carry explicit issues that name the concept and required word.
        if (!chosen.Pass && chosen.Issues is not null)
            foreach (var issue in chosen.Issues)
                if (TryParseIssue(issue, out var src, out var ctx, out var req))
                {
                    if (!_requiredTermsBySignature.TryGetValue(example.SemanticSignature, out var requiredTerms))
                    {
                        requiredTerms = [];
                        _requiredTermsBySignature[example.SemanticSignature] = requiredTerms;
                    }

                    requiredTerms.Add(req);

                    if (ctx != SemanticConcept.UNKNOWN && src != SemanticConcept.CODE_IDENTIFIER)
                        _conceptRules[(src, ctx)] = req;
                }
                else if (TryParseRequiredTerm(issue, out req))
                {
                    var qualityMarkerIssue = issue.Contains("translated quality marker", StringComparison.OrdinalIgnoreCase);
                    if (!qualityMarkerIssue && SignatureHasSpecificConcept(example.SemanticSignature))
                    {
                        if (!_requiredTermsBySignature.TryGetValue(example.SemanticSignature, out var requiredTerms))
                        {
                            requiredTerms = [];
                            _requiredTermsBySignature[example.SemanticSignature] = requiredTerms;
                        }

                        requiredTerms.Add(req);
                    }

                    var marker = ExtractSourceMarker(input.SourceText);
                    if (!string.IsNullOrWhiteSpace(marker))
                    {
                        if (!_requiredTermsBySourceMarker.TryGetValue(marker, out var markerRequiredTerms))
                        {
                            markerRequiredTerms = [];
                            _requiredTermsBySourceMarker[marker] = markerRequiredTerms;
                        }

                        markerRequiredTerms.Add(req);
                    }
                }
    }

    // MeasureAccuracy evaluates held-out verifier examples and returns the fraction
    // where the produced Pass/Fail verdict matches the expected verdict.
    public double MeasureAccuracy(IReadOnlyList<TranslationLearningExample> heldOut)
    {
        var relevant = heldOut.Where(e => e.HeldOut && e.UnitId == UnitId).ToList();
        if (relevant.Count == 0) return 0.0;

        var opts = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var tokenizer = new TranslationSemanticTokenizer();
        var correct = 0;
        foreach (var ex in relevant)
        {
            var input    = JsonSerializer.Deserialize<VerifierInput>(ex.InputJson, opts);
            var expected = JsonSerializer.Deserialize<VerifierOutput>(ex.ChosenOutputJson, opts);
            if (input is null || expected is null) continue;

            // Re-tokenize the source and run Verify against the candidate.
            var seq    = tokenizer.Tokenize(input.SourceText, "ru");
            var result = Verify(input.CandidateText, seq);
            if (result.Pass == expected.Pass) correct++;
        }

        return (double)correct / relevant.Count;
    }

    // ExportCheckpoint builds a QualityVerifierUnitCheckpoint from all current trained rules.
    // All three dictionaries are exported in deterministic sorted order.
    internal QualityVerifierUnitCheckpoint ExportCheckpoint()
    {
        // Export _conceptRules: key is a (Src, Ctx) concept tuple; sorted by concept name strings.
        var conceptRules = _conceptRules
            .OrderBy(kv => kv.Key.Src.ToString(), StringComparer.Ordinal)
            .ThenBy(kv => kv.Key.Ctx.ToString(), StringComparer.Ordinal)
            .Select(kv => new QualityVerifierConceptRuleCheckpoint(kv.Key.Src, kv.Key.Ctx, kv.Value))
            .ToList();
        // Export _requiredTermsBySignature: one entry per structural signature with its term list.
        var requiredBySignature = _requiredTermsBySignature
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => new QualityVerifierStringListCheckpoint(kv.Key, kv.Value.ToList()))
            .ToList();
        // Export _requiredTermsBySourceMarker: one entry per source marker with its term list.
        var requiredByMarker = _requiredTermsBySourceMarker
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new QualityVerifierStringListCheckpoint(kv.Key, kv.Value.ToList()))
            .ToList();
        // Wrap all three rule sets in the unit checkpoint record.
        return new QualityVerifierUnitCheckpoint(conceptRules, requiredBySignature, requiredByMarker);
    }

    // ImportCheckpoint clears all three rule dictionaries and restores them from the checkpoint.
    // Lists are restored exactly; no deduplication is applied because training pairs are authoritative.
    internal void ImportCheckpoint(QualityVerifierUnitCheckpoint checkpoint)
    {
        // Clear all three rule dictionaries before restoring to avoid stale rules.
        _conceptRules.Clear();
        _requiredTermsBySignature.Clear();
        _requiredTermsBySourceMarker.Clear();
        // Restore concept-pair rules by rebuilding (Src, Ctx) tuple keys.
        foreach (var rule in checkpoint.ConceptRules)
            _conceptRules[(rule.SourceConcept, rule.ContextConcept)] = rule.Required;
        // Restore per-signature required-term lists.
        foreach (var entry in checkpoint.RequiredTermsBySignature)
            _requiredTermsBySignature[entry.Key] = entry.Values.ToList();
        // Restore per-source-marker required-term lists.
        foreach (var entry in checkpoint.RequiredTermsBySourceMarker)
            _requiredTermsBySourceMarker[entry.Key] = entry.Values.ToList();
    }

    // TryParseIssue parses "expected '{word}' for concept {src} in context {ctx}".
    private static bool TryParseIssue(
        string issue,
        out SemanticConcept src,
        out SemanticConcept ctx,
        out string required)
    {
        src = SemanticConcept.UNKNOWN;
        ctx = SemanticConcept.UNKNOWN;
        required = string.Empty;

        // Split on single-quote to extract the required word.
        var parts = issue.Split('\'');
        if (parts.Length < 3) return false;
        required = parts[1];

        // Scan for concept names in the remainder of the issue string.
        var rest = parts[2];
        foreach (var name in Enum.GetNames<SemanticConcept>())
        {
            if (!rest.Contains(name, StringComparison.OrdinalIgnoreCase)) continue;
            if (src == SemanticConcept.UNKNOWN)
                src = Enum.Parse<SemanticConcept>(name);
            else
            {
                ctx = Enum.Parse<SemanticConcept>(name);
                break;
            }
        }

        return src != SemanticConcept.UNKNOWN && !string.IsNullOrEmpty(required);
    }

    private static bool TryParseRequiredTerm(string issue, out string required)
    {
        required = string.Empty;
        var parts = issue.Split('\'');
        if (parts.Length < 3) return false;

        required = parts[1];
        return !string.IsNullOrWhiteSpace(required);
    }

    private static bool SignatureHasSpecificConcept(string signature)
        => (signature ?? string.Empty).Split('|')
            .Skip(1)
            .Any(component => Enum.GetNames<SemanticConcept>()
                .Any(name => string.Equals(component, name, StringComparison.Ordinal)));

    private static string ExtractSourceMarker(string sourceText)
        => ExtractSourceMarker(new TranslationSemanticTokenizer().Tokenize(sourceText, "ru"));

    private static string ExtractSourceMarker(SemanticTokenSequence sourceTokens)
    {
        var token = sourceTokens.Tokens.FirstOrDefault(t => t.Pos == TranslationPos.ADV)
            ?? sourceTokens.Tokens.FirstOrDefault(t =>
                t.Pos is not TranslationPos.PREP
                    and not TranslationPos.PRON
                    and not TranslationPos.CODE_IDENTIFIER
                    and not TranslationPos.VERB &&
                t.Concept == SemanticConcept.UNKNOWN &&
                (sourceTokens.RuleTags.Count == 0 || LooksLikeMannerMarker(t.Word) || LooksLikeMannerMarker(t.Lemma)));

        return NormalizeMarker(token?.Lemma ?? token?.Word ?? string.Empty);
    }

    private static string NormalizeMarker(string value)
        => string.Concat((value ?? string.Empty)
            .Trim()
            .Trim('\'', '"', '`', '.', ',', '!', '?', ';', ':', '«', '»', '“', '”', '(', ')', '[', ']')
            .ToLowerInvariant()
            .Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '$'));

    private static bool LooksLikeMannerMarker(string value)
    {
        var marker = NormalizeMarker(value);
        return marker.Length > 2 &&
            (marker.EndsWith("Ð¾", StringComparison.Ordinal) ||
             marker.EndsWith("Ð½Ð¾", StringComparison.Ordinal) ||
             marker.EndsWith("ÐºÐ¾", StringComparison.Ordinal) ||
             marker.EndsWith("Ð»Ð¾", StringComparison.Ordinal) ||
             marker.EndsWith("ÃÂ¾", StringComparison.Ordinal) ||
             marker.EndsWith("ÃÂ½ÃÂ¾", StringComparison.Ordinal) ||
             marker.EndsWith("ly", StringComparison.OrdinalIgnoreCase));
    }
}

// TranslationCandidate is the output of ExecutorUnit: a translated text with confidence.
public sealed record TranslationCandidate(
    // Text is the assembled translation.
    string Text,
    // Confidence is the executor's estimated quality score [0.0–1.0].
    double Confidence,
    // AdapterId identifies which path produced this candidate.
    string AdapterId);
