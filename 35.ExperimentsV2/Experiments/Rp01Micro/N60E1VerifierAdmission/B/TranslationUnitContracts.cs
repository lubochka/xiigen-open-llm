// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace N60E1.B;

using System.Text.RegularExpressions;

// This file defines all per-unit contracts for the structured translation training pipeline.
// Governed by: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md Direction 1, decisions A-LM-02, A-LM-04.
// Architecture:
//   TranslationMachine
//     └── TrainStructuredBatchAsync(TranslationLearningBatch)
//           └── routes by UnitId to ITranslationUnitTrainer
//   TranslationRoutingFlow (inference)
//     ├── SemanticTokenizerUnit
//     ├── StructureKnnRetrieverUnit (via TranslationKnnStore)
//     ├── ExecutorUnit<direction>
//     └── QualityVerifierUnit

// SentencePattern names the eight sentence structure patterns from E25 experiment
// (POS bi-gram K-NN classification). Determines word-order rewriting in ExecutorUnit.
public enum SentencePattern
{
    // SVO: subject-verb-object — neutral Russian ("Я добавляю проверку") and default English.
    SVO,
    // SOV: subject-object-verb — Russian topicalization ("Я проверку добавляю").
    SOV,
    // VSO: verb-subject-object — Hebrew action sentences.
    VSO,
    // IMPERATIVE_V: verb+object only, no explicit subject — the dominant form for technical prompts.
    IMPERATIVE_V,
    // IMPERATIVE_VS: verb+subject+object — Hebrew emphatic imperative.
    IMPERATIVE_VS,
    // SVO_INF: starts with infinitive verb, followed by object — Russian goal statements ("запустить поток").
    SVO_INF,
    // UNKNOWN: could not classify with available POS tags.
    UNKNOWN
}

// TranslationPos are the coarse POS (Part-Of-Speech) tags used by the semantic tokenizer.
// Derived from E25/E26 experiments — deliberately coarse to keep the vocabulary small.
public enum TranslationPos
{
    VERB,
    NOUN,
    NOUN_GEN,       // Russian genitive noun (обработки, данных)
    PREP,
    PRON,
    ADJ,
    ADV,
    TECH_TERM,      // language-independent keyword: null, true, false, 404
    CODE_IDENTIFIER // PascalCase or camelCase identifier: ValidateToken, IService
}

// SemanticConcept names the domain concept carried by a token.
// These are the building blocks of the structural signature used for KNN (K-Nearest Neighbour) retrieval.
public enum SemanticConcept
{
    ADD_OP,
    VALIDATE_CHECK,
    NULL_LITERAL,
    METHOD_REF,
    CODE_IDENTIFIER,
    LOG_OP,
    WRITE_OP,
    THREAD_REF,
    FLOW_REF,
    STREAM_REF,
    ERROR_EXCEPTION,
    PROCESSING_OF,
    DATA_ENTITY,
    LAUNCH_OP,
    RETURN_OP,
    HANDLE_OP,
    UNKNOWN
}

// SemanticToken is one tokenized word with its full structural metadata.
public sealed record SemanticToken(
    // Word is the original word from the source text.
    string Word,
    // Lemma is the normalised base form used for concept lookup and E26 disambiguation.
    string Lemma,
    // Pos is the coarse part-of-speech tag.
    TranslationPos Pos,
    // Role is the syntactic role in the sentence: IMPERATIVE, DIRECT_OBJ, CODE_NAME, etc.
    string Role,
    // Concept is the semantic domain concept — UNKNOWN when not recognisable.
    SemanticConcept Concept);

// SemanticTokenSequence is the full structural analysis of one source sentence.
// Signature is used as the KNN lookup key: "IMPERATIVE_V|ADD_OP|VALIDATE_CHECK|NULL_LITERAL|METHOD_REF".
public sealed record SemanticTokenSequence(
    // Pattern is the sentence structure (SVO, IMPERATIVE_V, etc.).
    SentencePattern Pattern,
    // Tokens is the ordered list of semantic tokens.
    IReadOnlyList<SemanticToken> Tokens,
    // PosBigrams is the ordered list of consecutive POS tag pairs — used for E25 KNN matching.
    IReadOnlyList<(TranslationPos A, TranslationPos B)> PosBigrams,
    // Signature is the pipe-delimited structural key: "{Pattern}|{concept1}|{concept2}|...".
    string Signature,
    // PrefixSignature is the pattern plus the first few non-empty concepts.
    string PrefixSignature,
    // StructureSignature is the pattern plus POS roles without specific concepts.
    string StructureSignature,
    // RuleTags are language-structure features used by KNN: tense, case, agreement,
    // exception class, and other grammar signals. They are structural addresses,
    // not translation decisions.
    IReadOnlyList<string> RuleTags);

public static class TranslationUnitIds
{
    public const string SemanticTokenizer = "semantic-tokenizer";
    public const string ExecutorRuEn = "executor-ru_en";
    public const string ExecutorEnRu = "executor-en_ru";
    public const string ExecutorHeEn = "executor-he_en";
    public const string ExecutorEnHe = "executor-en_he";
    public const string QualityVerifier = "quality-verifier";

    public static string ExecutorFor(TranslationDirection direction)
        => $"executor-{direction.ToString().ToLowerInvariant()}";
}

// TranslationLearningExample is the per-unit structured training example.
// It replaces the flat LearningTrainingItem for the translation structured training path.
// Each unit gets its own examples with typed input/output JSON specific to that unit's contract.
public sealed record TranslationLearningExample(
    // ExampleId is the stable identifier for this example row.
    string ExampleId,
    // UnitId is the routing key — matches ITranslationUnitTrainer.UnitId.
    string UnitId,
    // FlowId is the flow this example belongs to.
    string FlowId,
    // InputJson is the unit-specific input object serialised to JSON.
    string InputJson,
    // ChosenOutputJson is the preferred (correct) unit output, serialised to JSON.
    string ChosenOutputJson,
    // RejectedOutputJson is the non-preferred output with a [WRONG: weakness - gap - consequence] annotation.
    string RejectedOutputJson,
    // RejectedType is a short label for the rejection category (e.g. "wrong_pos_compound").
    string RejectedType,
    // SemanticSignature is the structural signature used for KNN indexing.
    string SemanticSignature,
    // SourceId names the corpus source that produced this example.
    string SourceId,
    // HeldOut = true means this row is used only for accuracy measurement, not for training.
    bool HeldOut);

// TranslationLearningBatch carries one batch of structured per-unit examples.
// Passed to TranslationMachine.TrainStructuredBatchAsync.
public sealed record TranslationLearningBatch(
    // BatchId uniquely identifies this batch within the domain training history.
    string BatchId,
    // DomainTag is the string name of the domain ("TranslationDomain").
    string DomainTag,
    // Examples holds all per-unit structured training examples for this batch.
    IReadOnlyList<TranslationLearningExample> Examples,
    // SourceIds lists the corpus sources that contributed examples to this batch.
    IReadOnlyList<string> SourceIds);

// ITranslationUnitTrainer is implemented by each trainable unit in the translation flow.
// Design rule A-LM-02: Train* methods exist only on ILearningMachine and on the unit trainers
// it delegates to — never directly on ILogicalUnit or ILogicalFlow.
public interface ITranslationUnitTrainer
{
    // UnitId is the stable routing key — must match TranslationLearningExample.UnitId.
    string UnitId { get; }

    // TrainOnExample processes one DPO (Direct Preference Optimisation) training example
    // and updates the unit's internal weights or lookup tables.
    void TrainOnExample(TranslationLearningExample example);

    // MeasureAccuracy evaluates the held-out examples and returns accuracy in [0.0, 1.0].
    // Called after all train examples in a batch have been processed.
    double MeasureAccuracy(IReadOnlyList<TranslationLearningExample> heldOut);
}

// TranslationKnnStore is the shared bridge between training and inference for ExecutorUnit.
// Training writes (signature → translation) pairs here; inference reads them.
// Shared state is the only mechanism that connects TrainBatchAsync with GetOrchestratorAsync.
public sealed class TranslationKnnStore
{
    // SourceAwareBroadSignatureSimilarity is the minimum source-token overlap for broad fallback buckets.
    private const double SourceAwareBroadSignatureSimilarity = 0.35;

    // SourceAwarePrefixSimilarity is the lower overlap accepted for more specific prefix buckets.
    private const double SourceAwarePrefixSimilarity = 0.25;

    // SourceTokenRegex extracts language tokens from source text for source-aware KNN guard comparisons.
    private static readonly Regex SourceTokenRegex = new(@"[\p{L}\p{N}_$]+", RegexOptions.Compiled);

    // LowInformationSourceWords are ignored when comparing two source sentences.
    private static readonly HashSet<string> LowInformationSourceWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "to", "of", "in", "on", "at", "by", "for", "with", "from",
        "is", "are", "was", "were", "be", "been", "being", "not", "no", "do", "does", "did",
        "i", "you", "he", "she", "it", "we", "they", "this", "that", "these", "those"
    };

    // _store maps structural signature to the list of (source, target) translation pairs.
    private readonly Dictionary<string, List<(string Source, string Target)>> _store
        = new(StringComparer.Ordinal);

    // AddPair stores one learned translation pair under its structural signature.
    public void AddPair(string signature, string sourceText, string targetText)
    {
        // Ensure the bucket for this signature exists.
        if (!_store.TryGetValue(signature, out var bucket))
        {
            bucket = new List<(string, string)>();
            _store[signature] = bucket;
        }
        // Append the new pair to the bucket — later pairs have higher DPO preference signal.
        bucket.Add((sourceText, targetText));
    }

    // FindNearest returns up to k translation pairs with signatures closest to the query.
    // Exact match is tried first; partial component overlap is the fallback.
    public IReadOnlyList<(string Source, string Target)> FindNearest(string signature, int k = 3)
    {
        // Exact match: return the bucket directly when the signature is known.
        if (_store.TryGetValue(signature, out var exact))
            return exact.Take(k).ToList();

        // Partial match: score each stored signature by component overlap with the query.
        var queryComponents = new HashSet<string>(signature.Split('|'), StringComparer.Ordinal);
        return _store
            .Select(kv => (
                Score: kv.Key.Split('|').Count(c => queryComponents.Contains(c)),
                Pairs: kv.Value))
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(k)
            .SelectMany(x => x.Pairs)
            .Take(k)
            .ToList();
    }

    // FindNearestCascade searches by decreasing structural specificity:
    // full signature, then prefix signature, then exact POS-structure signature.
    // It deliberately avoids loose component overlap in the cascade, because a
    // broad "SVO" overlap can pull an unrelated trained sentence into runtime.
    // It decides how to search, not what translation means; stored targets come only from training data.
    public IReadOnlyList<(string Source, string Target)> FindNearestCascade(
        string fullSignature,
        string prefixSignature,
        string structureSignature,
        int k = 3,
        string? querySourceText = null)
    {
        var fullThreshold = IsLowInformationSignature(fullSignature)
            ? SourceAwareBroadSignatureSimilarity
            : 0.0;
        var full = FindExactWithSourceGuard(fullSignature, k, querySourceText, fullThreshold);
        if (full.Count > 0) return full;

        var prefix = FindExactWithSourceGuard(prefixSignature, k, querySourceText, SourceAwarePrefixSimilarity);
        if (prefix.Count > 0) return prefix;

        var structure = FindExactWithSourceGuard(structureSignature, k, querySourceText, SourceAwareBroadSignatureSimilarity);
        if (structure.Count > 0) return structure;

        return [];
    }

    private IReadOnlyList<(string Source, string Target)> FindExactWithSourceGuard(
        string signature,
        int k,
        string? querySourceText,
        double minimumSourceSimilarity)
    {
        if (!_store.TryGetValue(signature, out var exact))
            return [];

        // Apply candidate-conditioned laws before the cap at every cascade level, including full signatures.
        // An unknown analysis remains eligible for the existing similarity and final-verifier checks.
        var admissible = exact.Where(pair => TranslationMemorySourceLaw.Compare(querySourceText, pair.Source)
            != TranslationMemorySourceLaw.SourceCompatibility.Contradiction);

        if (minimumSourceSimilarity <= 0.0)
            return admissible.Take(k).ToList();

        var queryTokens = SourceTokens(querySourceText);
        if (queryTokens.Count == 0)
            return [];

        return admissible
            .Where(pair => SourceSimilarity(queryTokens, SourceTokens(pair.Source)) >= minimumSourceSimilarity)
            .Take(k)
            .ToList();
    }

    private static bool IsLowInformationSignature(string signature)
    {
        var components = (signature ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries);
        if (components.Length <= 2)
            return true;

        return components
            .Skip(1)
            .All(component => Enum.GetNames<TranslationPos>()
                .Any(pos => string.Equals(pos, component, StringComparison.Ordinal)));
    }

    private static HashSet<string> SourceTokens(string? sourceText)
        => SourceTokenRegex
            .Matches(sourceText ?? string.Empty)
            .Select(match => match.Value.Trim().ToLowerInvariant())
            .Where(token => token.Length >= 2)
            .Where(token => !LowInformationSourceWords.Contains(token))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static double SourceSimilarity(HashSet<string> queryTokens, HashSet<string> candidateTokens)
    {
        if (queryTokens.Count == 0 || candidateTokens.Count == 0)
            return 0.0;

        var overlap = queryTokens.Intersect(candidateTokens, StringComparer.OrdinalIgnoreCase).Count();
        return overlap / (double)queryTokens.Count;
    }

    // Count returns the total number of stored translation pairs across all signatures.
    public int Count => _store.Values.Sum(b => b.Count);

    // ExportCheckpoint builds a TranslationKnnStoreCheckpoint from all current in-memory pairs.
    // Entries are sorted deterministically: signature ascending, then source ascending.
    // The export does not write to disk — the caller is responsible for persistence.
    internal TranslationKnnStoreCheckpoint ExportCheckpoint()
    {
        // Flatten all buckets into a sorted list of (signature, source, target) triples.
        var entries = _store
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .SelectMany(kv => kv.Value
                .Select(pair => new TranslationKnnStoreEntryCheckpoint(kv.Key, pair.Source, pair.Target)))
            .ToList();
        // Wrap in the checkpoint record and return.
        return new TranslationKnnStoreCheckpoint(entries);
    }

    // ImportCheckpoint clears all current pairs and rebuilds the store from checkpoint entries.
    // After import the store is identical to the state it was in when ExportCheckpoint was called.
    internal void ImportCheckpoint(TranslationKnnStoreCheckpoint checkpoint)
    {
        // Remove all previously learned pairs before restoring from checkpoint.
        _store.Clear();
        // Restore each entry by calling the same AddPair method used during training.
        foreach (var entry in checkpoint.Entries)
            AddPair(entry.Signature, entry.Source, entry.Target);
    }
}
