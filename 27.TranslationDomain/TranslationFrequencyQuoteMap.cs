// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

// One row from a frequency/quote map. It is data, not a translation rule.
// SourceWord and TargetPrefix are authored from allowed corpora or first-party material.
public sealed record FrequencyQuoteEntry(
    string SourceWord,
    string SourceLang,
    string GrammaticalRole,
    string SourcePrefix,
    string TargetPrefix,
    string FullSourceSentence,
    string FullTargetSentence,
    string SourceId);

// Builds unit-level DPO examples from frequency/quote rows.
// The builder packages data into trainable examples; it does not decide translations.
public static class FrequencyQuoteMapBatchBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static TranslationLearningBatch BuildBatch(
        string batchId,
        IReadOnlyList<FrequencyQuoteEntry> entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchId);
        ArgumentNullException.ThrowIfNull(entries);

        var tokenizer = new TranslationSemanticTokenizer();
        var examples = new List<TranslationLearningExample>(entries.Count * 3);
        var sourceIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            sourceIds.Add(entry.SourceId);
            var sequence = tokenizer.Tokenize(entry.FullSourceSentence, entry.SourceLang);
            var marker = ExtractTargetMarker(entry.TargetPrefix);
            var rejectedCandidate = BuildRejectedCandidate(entry.FullTargetSentence, marker);

            examples.Add(BuildExample(
                $"{entry.SourceId}:semantic",
                TranslationUnitIds.SemanticTokenizer,
                new TokenizerInput(entry.FullSourceSentence, entry.SourceLang),
                new TokenizerOutput(sequence.Pattern, sequence.Signature),
                AddWrong(
                    JsonSerializer.Serialize(
                        new TokenizerOutput(sequence.Pattern, sequence.StructureSignature),
                        JsonOptions),
                    "frequency_prefix_marker_dropped"),
                "frequency_prefix_marker_dropped",
                sequence.Signature,
                entry.SourceId));

            examples.Add(BuildExample(
                $"{entry.SourceId}:executor",
                TranslationUnitIds.ExecutorRuEn,
                new ExecutorInput(entry.FullSourceSentence, entry.SourceLang, "frequency_quote"),
                new ExecutorOutput(
                    entry.FullTargetSentence,
                    0.93,
                    [new ExecutorWordMapping(entry.SourceWord, SemanticConcept.UNKNOWN, marker)]),
                AddWrong(
                    JsonSerializer.Serialize(
                        new ExecutorOutput(rejectedCandidate, 0.31, null),
                        JsonOptions),
                    "prefix_continuation_lost"),
                "prefix_continuation_lost",
                sequence.Signature,
                entry.SourceId));

            examples.Add(BuildExample(
                $"{entry.SourceId}:verifier",
                TranslationUnitIds.QualityVerifier,
                new VerifierInput(entry.FullSourceSentence, rejectedCandidate),
                new VerifierOutput(
                    0.42,
                    [$"expected '{marker}' for frequency prefix '{entry.SourcePrefix}'"],
                    false,
                    entry.FullTargetSentence),
                AddWrong(
                    JsonSerializer.Serialize(new VerifierOutput(0.91, [], true, null), JsonOptions),
                    "verifier_misses_prefix_marker"),
                "verifier_misses_prefix_marker",
                sequence.Signature,
                entry.SourceId));
        }

        return new TranslationLearningBatch(
            BatchId: batchId,
            DomainTag: TranslationMachine.DomainTag,
            Examples: examples,
            SourceIds: sourceIds.ToArray());
    }

    private static TranslationLearningExample BuildExample<TInput, TOutput>(
        string id,
        string unitId,
        TInput input,
        TOutput chosen,
        string rejectedJson,
        string rejectedType,
        string signature,
        string sourceId)
    {
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
            HeldOut: false);
    }

    private static string AddWrong(string json, string rejectedType)
        => json.TrimEnd('}') + $",\"wrong\":\"[WRONG: {rejectedType} - frequency quote near miss for unit-level learning.]\"}}";

    private static string ExtractTargetMarker(string targetPrefix)
    {
        var parts = Regex.Matches(targetPrefix ?? string.Empty, @"[\p{L}\p{N}_$']+")
            .Select(m => m.Value)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();

        return parts.Length == 0 ? (targetPrefix ?? string.Empty).Trim() : parts[^1];
    }

    private static string BuildRejectedCandidate(string fullTargetSentence, string marker)
    {
        if (string.IsNullOrWhiteSpace(marker))
            return fullTargetSentence;

        var rejected = Regex.Replace(
            fullTargetSentence,
            $@"\b{Regex.Escape(marker)}\b\s*",
            string.Empty,
            RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(100));

        rejected = Regex.Replace(rejected, @"\s{2,}", " ").Trim();
        return string.IsNullOrWhiteSpace(rejected) ? fullTargetSentence : rejected;
    }
}
