// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace N60E1.A;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public sealed record TranslationTrainReadyPair(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("direction")] string Direction,
    [property: JsonPropertyName("pair_family")] string PairFamily,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("a_side")] string ASide,
    [property: JsonPropertyName("chosen")] string Chosen,
    [property: JsonPropertyName("rejected")] string Rejected,
    [property: JsonPropertyName("validation_outcome")] string ValidationOutcome,
    [property: JsonPropertyName("validation_errors")] IReadOnlyList<string>? ValidationErrors);

public enum TranslationSignatureMode
{
    Structural,
    ExactPair
}

public static partial class TranslationStructuredBatchBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static IReadOnlyList<TranslationTrainReadyPair> ParseJsonl(string jsonl)
    {
        if (string.IsNullOrWhiteSpace(jsonl))
            return [];

        var rows = new List<TranslationTrainReadyPair>();
        foreach (var line in jsonl.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries))
        {
            var row = JsonSerializer.Deserialize<TranslationTrainReadyPair>(line, JsonOptions);
            if (row is not null)
                rows.Add(row);
        }

        return rows;
    }

    public static TranslationLearningBatch BuildRuEnGeneralBatch(
        string batchId,
        IReadOnlyList<TranslationTrainReadyPair> pairs,
        bool heldOut = false,
        TranslationSignatureMode signatureMode = TranslationSignatureMode.ExactPair)
    {
        if (pairs.Any(pair => !string.Equals(pair.Direction, "RU_EN", StringComparison.Ordinal)))
            throw new InvalidOperationException("BuildRuEnGeneralBatch accepts only RU_EN pairs.");

        return BuildGeneralBatch(batchId, pairs, heldOut, signatureMode);
    }

    public static TranslationLearningBatch BuildGeneralBatch(
        string batchId,
        IReadOnlyList<TranslationTrainReadyPair> pairs,
        bool heldOut = false,
        TranslationSignatureMode signatureMode = TranslationSignatureMode.ExactPair)
    {
        if (string.IsNullOrWhiteSpace(batchId))
            throw new ArgumentException("Batch id is required.", nameof(batchId));

        if (pairs.Count == 0)
            throw new ArgumentException("At least one pair is required.", nameof(pairs));

        var tokenizer = new TranslationSemanticTokenizer();
        var examples = new List<TranslationLearningExample>(pairs.Count * 3);

        foreach (var pair in pairs)
        {
            if (!Enum.TryParse<TranslationDirection>(pair.Direction, ignoreCase: true, out var direction))
                throw new InvalidOperationException($"Pair '{pair.Id}' has unsupported direction '{pair.Direction}'.");

            if (!string.Equals(pair.ValidationOutcome, "train_ready", StringComparison.Ordinal))
                throw new InvalidOperationException($"Pair '{pair.Id}' is not train-ready.");

            if (pair.ValidationErrors is { Count: > 0 })
                throw new InvalidOperationException($"Pair '{pair.Id}' contains validation errors.");

            var sourceId = string.IsNullOrWhiteSpace(pair.Source) ? pair.Id : pair.Source;
            var sourceLanguage = SourceLanguageFor(direction);
            var executorUnitId = TranslationUnitIds.ExecutorFor(direction);
            var sequence = tokenizer.Tokenize(pair.ASide, sourceLanguage);
            var signature = signatureMode == TranslationSignatureMode.ExactPair
                ? $"{sequence.Signature}|PAIR:{pair.Id}"
                : sequence.Signature;
            signature = AppendRuleTags(signature, BuildRuleTags(pair.Rejected));
            var rejectedType = ExtractRejectedType(pair.Rejected);
            var phraseMappings = BuildPhraseMappings(pair.Rejected);
            var requiredTerm = ExtractRequiredTerm(pair.Chosen, phraseMappings);
            var rejectedCandidate = BuildRejectedCandidate(pair.Chosen, requiredTerm, rejectedType, pair.Rejected);
            var agreementMappings = BuildAgreementMappings(pair.Rejected);
            var wordMappings = BuildWordMappings(direction, sequence, pair.Chosen, requiredTerm, agreementMappings);

            examples.Add(BuildExample(
                $"{pair.Id}:semantic",
                TranslationUnitIds.SemanticTokenizer,
                new TokenizerInput(pair.ASide, sourceLanguage),
                new TokenizerOutput(sequence.Pattern, signature),
                new TokenizerOutput(SentencePattern.UNKNOWN, $"WRONG|{signature}"),
                BuildUnitRejectedType("semantic", rejectedType),
                signature,
                sourceId,
                heldOut));

            examples.Add(BuildExample(
                $"{pair.Id}:executor",
                executorUnitId,
                new ExecutorInput(pair.ASide, sourceLanguage, "general"),
                new ExecutorOutput(pair.Chosen, 0.93, wordMappings, phraseMappings),
                new ExecutorOutput(rejectedCandidate, 0.31, null),
                rejectedType,
                signature,
                sourceId,
                heldOut));

            examples.Add(BuildExample(
                $"{pair.Id}:verifier",
                TranslationUnitIds.QualityVerifier,
                new VerifierInput(pair.ASide, rejectedCandidate),
                new VerifierOutput(
                    0.42,
                    [$"expected '{requiredTerm}' for translated quality marker"],
                    false,
                    pair.Chosen),
                new VerifierOutput(0.91, [], true, null),
                BuildUnitRejectedType("verifier", rejectedType),
                signature,
                sourceId,
                heldOut));
        }

        return new TranslationLearningBatch(
            BatchId: batchId,
            DomainTag: TranslationMachine.DomainTag,
            Examples: examples,
            SourceIds: pairs.Select(pair => string.IsNullOrWhiteSpace(pair.Source) ? pair.Id : pair.Source)
                .Distinct(StringComparer.Ordinal)
                .ToArray());
    }

    private static TranslationLearningExample BuildExample<TInput, TOutput>(
        string id,
        string unitId,
        TInput input,
        TOutput chosen,
        TOutput rejected,
        string rejectedType,
        string signature,
        string sourceId,
        bool heldOut)
    {
        var rejectedJson = JsonSerializer.Serialize(rejected, JsonOptions);
        rejectedJson = rejectedJson.TrimEnd('}') +
            $",\"wrong\":\"[WRONG: {rejectedType} - generated near miss for per-unit translation learning.]\"}}";

        return new TranslationLearningExample(
            ExampleId: id,
            UnitId: unitId,
            FlowId: TranslationRoutingFlow.FlowId,
            InputJson: JsonSerializer.Serialize(input, JsonOptions),
            ChosenOutputJson: JsonSerializer.Serialize(chosen, JsonOptions),
            RejectedOutputJson: rejectedJson,
            RejectedType: rejectedType,
            SemanticSignature: signature,
            SourceId: sourceId,
            HeldOut: heldOut);
    }

    public static string ExtractProductMarker(string chosen)
    {
        var adverb = EnglishAdverbRegex().Match(chosen ?? string.Empty);
        if (adverb.Success)
            return adverb.Value;

        var firstWord = WordRegex().Match(chosen ?? string.Empty);
        return firstWord.Success ? firstWord.Value : "translation";
    }

    // ComputeTokenOverlap returns the Jaccard token-set similarity between two source strings.
    // A value near 1.0 means nearly identical vocabulary; 0.0 means no shared tokens.
    // Used to detect near-duplicate training examples before batch assembly.
    public static double ComputeTokenOverlap(string a, string b)
        => JaccardTokenSimilarity(TokenSetForSimilarity(a), TokenSetForSimilarity(b));

    // FilterByTrainingHistoryUniqueness removes candidate pairs whose a_side token overlap
    // with any already-trained a_side is at or above maxOverlapThreshold (default 0.70).
    // Near-duplicates of training history provide no new learning signal and inflate
    // memorisation accuracy. Removing them forces the model to generalise from novel patterns.
    public static IReadOnlyList<TranslationTrainReadyPair> FilterByTrainingHistoryUniqueness(
        IReadOnlyList<TranslationTrainReadyPair> candidates,
        IReadOnlyList<string> trainedASides,
        double maxOverlapThreshold = 0.70)
    {
        // Empty candidate list passes through unchanged.
        if (candidates is null || candidates.Count == 0) return [];

        // No history to compare against — all candidates are novel by definition.
        if (trainedASides is null || trainedASides.Count == 0) return candidates;

        // Build token fingerprints once for the full training history.
        var historyFingerprints = trainedASides
            .Where(side => !string.IsNullOrWhiteSpace(side))
            .Select(TokenSetForSimilarity)
            .Where(tokens => tokens.Count > 0)
            .ToArray();

        if (historyFingerprints.Length == 0) return candidates;

        // Retain only those candidates whose overlap with every prior training example
        // stays below the threshold.
        return candidates
            .Where(pair =>
            {
                if (pair is null || string.IsNullOrWhiteSpace(pair.ASide)) return false;
                var candidateTokens = TokenSetForSimilarity(pair.ASide);

                // A pair with no usable tokens is treated as novel.
                if (candidateTokens.Count == 0) return true;

                // Reject if any prior example is too similar.
                return !historyFingerprints.Any(history =>
                    JaccardTokenSimilarity(candidateTokens, history) >= maxOverlapThreshold);
            })
            .ToArray();
    }

    // TokenSetForSimilarity builds the normalised token bag for Jaccard comparison:
    // lowercase, letter-or-digit only, split on whitespace.
    private static HashSet<string> TokenSetForSimilarity(string value)
        => (value ?? string.Empty)
            .ToLowerInvariant()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => new string(token.Where(char.IsLetterOrDigit).ToArray()))
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .ToHashSet(StringComparer.Ordinal);

    // JaccardTokenSimilarity computes |intersection| / |union| for two normalised token sets.
    private static double JaccardTokenSimilarity(IReadOnlySet<string> a, IReadOnlySet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        var intersection = a.Count(b.Contains);
        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0.0 : (double)intersection / union;
    }

    private static string ExtractRequiredTerm(
        string chosen,
        IReadOnlyList<ExecutorPhraseMapping>? phraseMappings)
    {
        var phraseMarker = phraseMappings?
            .Select(mapping => WordRegex().Match(mapping.TargetPhrase ?? string.Empty))
            .FirstOrDefault(match => match.Success);

        return phraseMarker?.Value ?? ExtractProductMarker(chosen);
    }

    private static string BuildRejectedCandidate(
        string chosen,
        string requiredTerm,
        string rejectedType,
        string rejectedSide)
    {
        if (string.IsNullOrWhiteSpace(chosen))
            return string.Empty;

        var explicitRejected = ExtractExplicitRejectedCandidate(rejectedSide);
        if (!string.IsNullOrWhiteSpace(explicitRejected))
            return explicitRejected;

        if (!IsOmissionLikeRejectedType(rejectedType))
            return chosen;

        var escaped = Regex.Escape(requiredTerm);
        var candidate = Regex.Replace(chosen, $@"\b{escaped}\b\s*", string.Empty, RegexOptions.IgnoreCase);
        candidate = Regex.Replace(candidate, @"\s{2,}", " ").Trim();
        return string.IsNullOrWhiteSpace(candidate) ? chosen : candidate;
    }

    private static string BuildUnitRejectedType(string unitPrefix, string rejectedType)
    {
        var safeType = string.IsNullOrWhiteSpace(rejectedType)
            ? "translation_quality_miss"
            : rejectedType.Trim();
        return $"{unitPrefix}_{safeType}";
    }

    private static bool IsOmissionLikeRejectedType(string rejectedType)
    {
        var value = rejectedType ?? string.Empty;
        if (value.Contains("agreement", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("case", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("tense", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("aspect", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("gender", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("morpholog", StringComparison.OrdinalIgnoreCase))
            return false;

        return value.Contains("omission", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("missed", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("lost", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("drop", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("marker", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractExplicitRejectedCandidate(string rejectedSide)
    {
        var value = (rejectedSide ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value) || value.StartsWith("[", StringComparison.Ordinal))
            return string.Empty;

        var markerIndex = value.IndexOf("[WRONG:", StringComparison.OrdinalIgnoreCase);
        var candidate = markerIndex >= 0 ? value[..markerIndex] : value;
        return candidate.Trim();
    }

    private static IReadOnlyList<ExecutorWordMapping> BuildWordMappings(
        TranslationDirection direction,
        SemanticTokenSequence sequence,
        string chosen,
        string requiredTerm,
        IReadOnlyList<ExecutorWordMapping>? explicitMappings = null)
    {
        var mappings = new List<ExecutorWordMapping>();
        var seen = new HashSet<(string, SemanticConcept)>();

        if (explicitMappings is not null)
            foreach (var mapping in explicitMappings)
                AddMapping(mapping.SourceWord, mapping.Concept, mapping.TargetWord);

        foreach (var token in sequence.Tokens)
        {
            var target = InferTargetWord(token, chosen);
            if (string.IsNullOrWhiteSpace(target))
                continue;

            AddMapping(token.Word, token.Concept, target);
            if (!string.Equals(token.Word, token.Lemma, StringComparison.OrdinalIgnoreCase))
                AddMapping(token.Lemma, token.Concept, target);
        }

        if (!string.IsNullOrWhiteSpace(requiredTerm))
        {
            var marker = ExtractSourceMarker(sequence);
            if (marker is not null)
            {
                AddMapping(marker.Word, marker.Concept, requiredTerm);
                AddMapping(marker.Lemma, marker.Concept, requiredTerm);
            }
        }

        var firstSourceToken = sequence.Tokens.FirstOrDefault(token =>
            token.Pos is not TranslationPos.PREP and not TranslationPos.CODE_IDENTIFIER);
        var firstTargetWord = ExtractFirstTargetWord(chosen);
        if (firstSourceToken is not null && !string.IsNullOrWhiteSpace(firstTargetWord))
        {
            AddMapping(firstSourceToken.Word, firstSourceToken.Concept, firstTargetWord);
            AddMapping(firstSourceToken.Lemma, firstSourceToken.Concept, firstTargetWord);
        }

        return mappings;

        void AddMapping(string sourceWord, SemanticConcept concept, string targetWord)
        {
            var key = (NormalizeWord(sourceWord), concept);
            if (string.IsNullOrWhiteSpace(key.Item1) ||
                !IsTargetWordCompatible(direction, sourceWord, targetWord) ||
                !seen.Add(key))
                return;

            mappings.Add(new ExecutorWordMapping(sourceWord, concept, targetWord));
        }
    }

    private static string? InferTargetWord(SemanticToken token, string chosen)
        => token.Concept switch
        {
            SemanticConcept.ADD_OP => FindChosenWord(chosen, "Add", "Start", "Launch", "Write", "Log", "Handle"),
            SemanticConcept.VALIDATE_CHECK => FindChosenWord(chosen, "check", "validation"),
            SemanticConcept.NULL_LITERAL => FindChosenWord(chosen, "null"),
            SemanticConcept.METHOD_REF => FindChosenWord(chosen, "method", "function"),
            SemanticConcept.THREAD_REF => FindChosenWord(chosen, "thread"),
            SemanticConcept.FLOW_REF => FindChosenWord(chosen, "flow"),
            SemanticConcept.STREAM_REF => FindChosenWord(chosen, "stream"),
            SemanticConcept.PROCESSING_OF => FindChosenWord(chosen, "processing"),
            SemanticConcept.DATA_ENTITY => FindChosenWord(chosen, "data", "file"),
            SemanticConcept.ERROR_EXCEPTION => FindChosenWord(chosen, "error", "exception"),
            SemanticConcept.CODE_IDENTIFIER => token.Word,
            _ => null
        };

    private static IReadOnlyList<ExecutorPhraseMapping>? BuildPhraseMappings(string rejected)
    {
        var mappings = PhraseMappingRegex()
            .Matches(rejected ?? string.Empty)
            .Select(match => new ExecutorPhraseMapping(
                match.Groups["source"].Value.Trim(),
                match.Groups["target"].Value.Trim()))
            .Where(mapping =>
                !string.IsNullOrWhiteSpace(mapping.SourcePhrase) &&
                !string.IsNullOrWhiteSpace(mapping.TargetPhrase))
            .ToArray();

        return mappings.Length == 0 ? null : mappings;
    }

    private static IReadOnlyList<ExecutorWordMapping> BuildAgreementMappings(string rejected)
    {
        var mappings = AgreementMappingRegex()
            .Matches(rejected ?? string.Empty)
            .Select(match => new ExecutorWordMapping(
                match.Groups["source"].Value.Trim(),
                SemanticConcept.UNKNOWN,
                match.Groups["target"].Value.Trim()))
            .Where(mapping =>
                !string.IsNullOrWhiteSpace(mapping.SourceWord) &&
                !string.IsNullOrWhiteSpace(mapping.TargetWord))
            .ToArray();

        return mappings;
    }

    private static IReadOnlyList<string> BuildRuleTags(string rejected)
    {
        return RuleTagRegex()
            .Matches(rejected ?? string.Empty)
            .Select(match => NormalizeRuleTag(match.Groups["rule"].Value))
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string AppendRuleTags(string signature, IReadOnlyList<string> ruleTags)
    {
        if (ruleTags.Count == 0)
            return signature;

        var existing = signature.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);
        var additions = ruleTags.Where(tag => !existing.Contains(tag)).ToArray();
        return additions.Length == 0
            ? signature
            : $"{signature}|{string.Join("|", additions)}";
    }

    private static string NormalizeRuleTag(string value)
    {
        var normalized = Regex.Replace(value ?? string.Empty, @"[^A-Za-z0-9:_=\-]+", "_")
            .Trim('_')
            .ToUpperInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : $"RULE:{normalized}";
    }

    private static string? FindChosenWord(string chosen, params string[] candidates)
    {
        foreach (var candidate in candidates)
            if (Regex.IsMatch(chosen ?? string.Empty, $@"\b{Regex.Escape(candidate)}\b", RegexOptions.IgnoreCase))
                return candidate;

        return null;
    }

    private static string ExtractFirstTargetWord(string chosen)
    {
        var firstWord = WordRegex().Match(chosen ?? string.Empty);
        return firstWord.Success ? firstWord.Value : string.Empty;
    }

    private static bool IsTargetWordCompatible(TranslationDirection direction, string sourceWord, string targetWord)
    {
        if (string.IsNullOrWhiteSpace(sourceWord) || string.IsNullOrWhiteSpace(targetWord))
            return false;

        return direction switch
        {
            TranslationDirection.EN_RU =>
                ContainsCyrillic(targetWord) &&
                !ContainsLatinLetter(targetWord),
            TranslationDirection.RU_EN =>
                !ContainsCyrillic(targetWord),
            _ => true
        };
    }

    private static bool ContainsCyrillic(string value)
        => (value ?? string.Empty).Any(ch => ch is >= '\u0400' and <= '\u04FF');

    private static bool ContainsLatinLetter(string value)
        => (value ?? string.Empty).Any(ch => ch is >= 'A' and <= 'Z' or >= 'a' and <= 'z');

    private static bool LooksLikeMannerMarker(string word)
    {
        var normalized = NormalizeWord(word);
        return normalized.Length > 2 &&
            (normalized.EndsWith("о", StringComparison.Ordinal) ||
             normalized.EndsWith("но", StringComparison.Ordinal) ||
             normalized.EndsWith("ко", StringComparison.Ordinal) ||
             normalized.EndsWith("ло", StringComparison.Ordinal) ||
             normalized.EndsWith("ённо", StringComparison.Ordinal) ||
             normalized.EndsWith("енно", StringComparison.Ordinal) ||
             normalized.EndsWith("Ð¾", StringComparison.Ordinal) ||
             normalized.EndsWith("Ð½Ð¾", StringComparison.Ordinal) ||
             normalized.EndsWith("ly", StringComparison.OrdinalIgnoreCase));
    }

    private static SemanticToken? ExtractSourceMarker(SemanticTokenSequence sequence)
        => sequence.Tokens.FirstOrDefault(token => token.Pos == TranslationPos.ADV)
           ?? sequence.Tokens.FirstOrDefault(token =>
               token.Pos is not TranslationPos.PREP
                   and not TranslationPos.CODE_IDENTIFIER
                   and not TranslationPos.PRON
                   and not TranslationPos.VERB &&
               token.Concept == SemanticConcept.UNKNOWN &&
               (sequence.RuleTags.Count == 0 || LooksLikeMannerMarker(token.Word) || LooksLikeMannerMarker(token.Lemma)));

    private static string NormalizeWord(string word)
        => Regex.Replace(word ?? string.Empty, @"[^\p{L}\p{N}_$]+", string.Empty).ToLowerInvariant();

    private static string ExtractRejectedType(string rejected)
    {
        var match = WrongTypeRegex().Match(rejected ?? string.Empty);
        return match.Success ? match.Groups["type"].Value.Trim() : "translation_quality_miss";
    }

    private static string SourceLanguageFor(TranslationDirection direction)
        => direction switch
        {
            TranslationDirection.RU_EN => "ru",
            TranslationDirection.EN_RU => "en",
            TranslationDirection.HE_EN => "he",
            TranslationDirection.EN_HE => "en",
            TranslationDirection.ES_EN => "es",
            TranslationDirection.EN_ES => "en",
            _ => "en"
        };

    [GeneratedRegex(@"\[WRONG:\s*(?<type>[a-zA-Z0-9_\-]+)")]
    private static partial Regex WrongTypeRegex();

    [GeneratedRegex(@"\[PHRASE:\s*(?<source>.*?)\s*=>\s*(?<target>.*?)\]", RegexOptions.Singleline)]
    private static partial Regex PhraseMappingRegex();

    [GeneratedRegex(@"\[AGREEMENT:\s*(?<source>.*?)\s*=>\s*(?<target>.*?)\]", RegexOptions.Singleline)]
    private static partial Regex AgreementMappingRegex();

    [GeneratedRegex(@"\[RULE:\s*(?<rule>.*?)\]", RegexOptions.Singleline)]
    private static partial Regex RuleTagRegex();

    [GeneratedRegex(@"\b[A-Za-z][A-Za-z'\-]*ly\b")]
    private static partial Regex EnglishAdverbRegex();

    [GeneratedRegex(@"\b[A-Za-z][A-Za-z'\-]*\b")]
    private static partial Regex EnglishWordRegex();

    [GeneratedRegex(@"[\p{L}\p{N}_$][\p{L}\p{N}'_\-$]*")]
    private static partial Regex WordRegex();
}
