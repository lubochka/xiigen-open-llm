// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

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
    [property: JsonPropertyName("validation_errors")] IReadOnlyList<string>? ValidationErrors,
    // VerifiedRuleTags carries the grammar marks a verified training row states about the CORRECT
    // answer of this pair, for example the tense the correct translation really is in. It arrives
    // beside the identifier and the validation fields and is the only source of the rule marks a
    // lesson is stored with. A row written before this field existed simply has no verified
    // annotation: that is an honest absence, not a permission to read the rejected side instead.
    [property: JsonPropertyName("verified_rule_tags")] IReadOnlyList<string>? VerifiedRuleTags = null);

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
            // Рабочий ключ урока собирается КАНОНИЧЕСКИМ в любом режиме подписи: в нём нет и не
            // может быть отрезка происхождения. Прежде режим точной пары приклеивал сюда номер
            // учебной пары, и урок, записанный с номером, читатели не находили никогда — они
            // ищут по разбору текста, а при обычном чтении номеру взяться негде.
            var signature = TranslationSemanticKey.Compose(sequence.Signature);
            // Режим подписи сохраняет свой смысл, но теперь решает только одно: называется ли
            // происхождение этого урока. Номер пары уходит в своё отдельное поле учебной строки,
            // потому что происхождение урока — это не смысл переводимого текста.
            var pairProvenance = signatureMode == TranslationSignatureMode.ExactPair
                ? pair.Id
                : null;
            // Read the grammar marks this pair states about its own correct answer. The verified
            // side is the only source; the "how it is wrong" side is not consulted here, because
            // for a tense pair it states the opposite of the truth.
            var verifiedRuleTags = TranslationVerifiedRuleTags.Canonicalize(pair.VerifiedRuleTags);
            // An annotation that cannot state one real grammar fact stops the whole batch now,
            // before any lesson is written, instead of saving a mark nobody checked.
            if (!verifiedRuleTags.IsSuccess)
                throw new InvalidOperationException(
                    $"Pair '{pair.Id}' has an invalid verified rule annotation: {verifiedRuleTags.ErrorMessage}");
            // Согласованный набор учебных пометок этой строки: проверенное время заменяет догадку
            // сырого разбора, а пометки, осмысленные только при прошедшем времени, при другом
            // времени убираются. Прежде урок хранил догадку сырого разбора рядом с проверенной
            // пометкой, и ключ противоречил сам себе.
            var reconciledRuleTags = TranslationVerifiedRuleTags.ReconcileTrainedTags(
                sequence.RuleTags, verifiedRuleTags.Value!);
            // Набор, который не описывает один ответ, останавливает всю партию здесь же.
            if (!reconciledRuleTags.IsSuccess)
                throw new InvalidOperationException(
                    $"Pair '{pair.Id}' has an inconsistent trained rule annotation: {reconciledRuleTags.ErrorMessage}");
            // Рабочий ключ собирается ЗАНОВО общим вычислителем разбора из образца предложения,
            // лексем и ровно этого набора: тот же вычислитель потом собирает ключи при чтении,
            // поэтому адрес записи и адрес чтения разойтись не могут.
            signature = TranslationSemanticKey.Compose(
                TranslationSemanticTokenizer.ComposeSignatures(
                    sequence.Pattern, sequence.Tokens, reconciledRuleTags.Value!).Signature);
            // Append the verified marks to the signature the correct lesson is stored under.
            signature = AppendRuleTags(signature, BuildVerifiedRuleTags(verifiedRuleTags.Value!));
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
                // Урок разборщика несёт СВОЙ согласованный набор пометок и номер формы записи:
                // по ним обученная единица собирает ключи и пишет слепок состояния.
                new TokenizerOutput(
                    sequence.Pattern,
                    signature,
                    reconciledRuleTags.Value!,
                    TranslationTokenizerAnnotationVersions.ConsistentTagSet),
                new TokenizerOutput(SentencePattern.UNKNOWN, $"WRONG|{signature}"),
                BuildUnitRejectedType("semantic", rejectedType),
                signature,
                sourceId,
                heldOut,
                pairProvenance));

            examples.Add(BuildExample(
                $"{pair.Id}:executor",
                executorUnitId,
                new ExecutorInput(pair.ASide, sourceLanguage, "general"),
                new ExecutorOutput(pair.Chosen, 0.93, wordMappings, phraseMappings),
                new ExecutorOutput(rejectedCandidate, 0.31, null),
                rejectedType,
                signature,
                sourceId,
                heldOut,
                pairProvenance));

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
                heldOut,
                pairProvenance));
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
        bool heldOut,
        // pairProvenance несёт номер учебной пары, из которой получен этот урок. Он приходит
        // отдельным доводом и попадает в отдельное поле строки, а не в рабочий ключ.
        string? pairProvenance = null)
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
            HeldOut: heldOut,
            // Происхождение урока названо отдельным полем: по нему видно, откуда урок, и при
            // этом рабочий ключ поиска от происхождения не зависит.
            PairId: pairProvenance,
            // Форма ключа названа прямо, чтобы загрузчик отличал новую память от старой.
            KeySchemaVersion: TranslationSemanticKey.SchemaVersion);
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

    // FilterByTrainingHistoryUniqueness, manifest overload: the same near-duplicate rule as above,
    // plus one narrow exception for a lesson a person has verified as the opposite of an old lesson.
    // Why the exception is needed: the rule measures shared words only, so a sentence that changes
    // the meaning to its opposite with the single word "не" ("not") still shares most of its words
    // with the positive sentence and is discarded as if it taught nothing. The negative form then
    // never reaches training and the translator keeps refusing that sentence.
    // What changes: a candidate the word-overlap rule rejects is admitted after all when, and only
    // when, the manifest carries a fully valid verified-contrast entry for exactly that candidate,
    // pointing at a pair that really is in the history passed to this call. Everything else is
    // unchanged: the same threshold, the same word splitting, the same decision for every candidate
    // the rule already accepted, and the original order of the candidates.
    // What it never does: it never guesses novelty by itself, never reorders candidates, never
    // admits a pair because a manifest merely exists, and never throws on a bad manifest - a missing
    // or mismatched field simply leaves the original uniqueness decision in force.
    public static IReadOnlyList<TranslationTrainReadyPair> FilterByTrainingHistoryUniqueness(
        IReadOnlyList<TranslationTrainReadyPair> candidates,
        IReadOnlyList<TranslationTrainReadyPair> historyPairs,
        IReadOnlyDictionary<string, VerifiedTranslationContrast>? contrasts,
        double maxOverlapThreshold = 0.70)
    {
        // Empty candidate list passes through unchanged, exactly as in the overload above.
        if (candidates is null || candidates.Count == 0) return [];

        // Keep only history rows that carry a usable source sentence to compare against.
        var usableHistory = (historyPairs ?? Array.Empty<TranslationTrainReadyPair>())
            .Where(pair => pair is not null && !string.IsNullOrWhiteSpace(pair.ASide))
            .ToArray();

        // No history to compare against - all candidates are novel by definition.
        if (usableHistory.Length == 0) return candidates;

        // Build token fingerprints once for the full training history, as the overload above does.
        var historyFingerprints = usableHistory
            .Select(pair => TokenSetForSimilarity(pair.ASide))
            .Where(tokens => tokens.Count > 0)
            .ToArray();

        if (historyFingerprints.Length == 0) return candidates;

        // Index the history by identifier, so a manifest entry can be checked against the exact
        // old lesson it names instead of against any similar-looking row.
        var historyById = new Dictionary<string, TranslationTrainReadyPair>(StringComparer.Ordinal);
        foreach (var pair in usableHistory)
            if (!string.IsNullOrWhiteSpace(pair.Id))
                historyById[pair.Id] = pair;

        // Walk the candidates in their original order and decide each one separately.
        var admitted = new List<TranslationTrainReadyPair>(candidates.Count);
        foreach (var candidate in candidates)
        {
            // A missing candidate or a candidate without a source sentence is dropped, as before.
            if (candidate is null || string.IsNullOrWhiteSpace(candidate.ASide)) continue;

            var candidateTokens = TokenSetForSimilarity(candidate.ASide);

            // A pair with no usable tokens is treated as novel, as before.
            if (candidateTokens.Count == 0)
            {
                admitted.Add(candidate);
                continue;
            }

            // The unchanged near-duplicate rule: too similar to any prior example means rejected.
            var tooSimilarToHistory = historyFingerprints.Any(history =>
                JaccardTokenSimilarity(candidateTokens, history) >= maxOverlapThreshold);

            // Candidates the rule already accepted keep their old outcome without consulting anything.
            if (!tooSimilarToHistory)
            {
                admitted.Add(candidate);
                continue;
            }

            // Only a rejected candidate may be rescued, and only by a valid verified contrast.
            if (HasVerifiedContrastException(candidate, contrasts, historyById))
                admitted.Add(candidate);
        }

        return admitted;
    }

    // HasVerifiedContrastException answers one question for one candidate the word-overlap rule has
    // already rejected: is there a complete, matching, human-verified contrast manifest for exactly
    // this candidate against a pair that is really in the training history passed to this call?
    // Each check is a plain form check on the manual decision; none of them reads meaning.
    private static bool HasVerifiedContrastException(
        TranslationTrainReadyPair candidate,
        IReadOnlyDictionary<string, VerifiedTranslationContrast>? contrasts,
        IReadOnlyDictionary<string, TranslationTrainReadyPair> historyById)
    {
        // No manifest at all means the previous behaviour stands for every candidate.
        if (contrasts is null || contrasts.Count == 0) return false;

        // A candidate without an identifier cannot be addressed by a manifest entry.
        if (string.IsNullOrWhiteSpace(candidate.Id)) return false;

        // The manifest must contain an entry written for this exact candidate.
        if (!contrasts.TryGetValue(candidate.Id, out var contrast) || contrast is null) return false;

        // The entry must name the old lesson it contrasts against.
        if (string.IsNullOrWhiteSpace(contrast.ReferenceId)) return false;

        // That old lesson must really be present in the history handed to this call; a reference to
        // something outside this training history proves nothing about this training run.
        if (!historyById.TryGetValue(contrast.ReferenceId, out var reference)) return false;

        // Finally the entry itself must be complete and must match both pairs byte for byte.
        return contrast.IsValidFor(candidate, reference);
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

    // BuildVerifiedRuleTags turns the canonical bodies of a verified annotation into the rule tags
    // appended to the signature of the correct lesson. The verified side is the only source: a
    // pair without a verified annotation receives no rule tag, and no reading of the "how it is
    // wrong" side can put a mark here any more.
    private static IReadOnlyList<string> BuildVerifiedRuleTags(IReadOnlyList<string> canonicalBodies)
    {
        return canonicalBodies
            .Select(NormalizeRuleTag)
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

    [GeneratedRegex(@"\b[A-Za-z][A-Za-z'\-]*ly\b")]
    private static partial Regex EnglishAdverbRegex();

    [GeneratedRegex(@"\b[A-Za-z][A-Za-z'\-]*\b")]
    private static partial Regex EnglishWordRegex();

    [GeneratedRegex(@"[\p{L}\p{N}_$][\p{L}\p{N}'_\-$]*")]
    private static partial Regex WordRegex();
}
