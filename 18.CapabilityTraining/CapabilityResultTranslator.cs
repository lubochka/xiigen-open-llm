// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

// This translator converts a learned capability routing result into developer-readable text.
public static class CapabilityResultTranslator
{
    private const int MaximumSignedIdentifierLength = 16_384;
    private const int MaximumSignedPredicateCount = 256;
    private const int MaximumSignedPromptLength = 1_048_576;

    public static string Translate(string knnOutput, LogicalSubPhase subPhase, string originalPrompt)
    {
        var authoritativeLines = ComparisonLines(knnOutput).ToArray();
        var promptLine = string.IsNullOrWhiteSpace(originalPrompt) ||
                         ContainsRecognizedPromptMetadata(authoritativeLines) ||
                         ContainsEquivalentRawPromptSequence(knnOutput, originalPrompt)
            ? string.Empty
            : $"For your request: \"{originalPrompt.Trim()}\"";
        var subPhaseLine = $"Sub-phase {subPhase.SubPhaseId} ({subPhase.Name})";
        var scopeLine = string.IsNullOrWhiteSpace(subPhase.Purpose)
            ? string.Empty
            : $"Scope: {subPhase.Purpose}";
        var focusLine = subPhase.PredicateIds.Length == 0
            ? string.Empty
            : $"Focus areas: {string.Join(", ", subPhase.PredicateIds)}";

        return AppendAnnotations(
            knnOutput,
            authoritativeLines,
            [promptLine, subPhaseLine, scopeLine, focusLine]);
    }

    public static string TranslateFromKnnOutput(string knnOutput, string originalPrompt)
    {
        if (string.IsNullOrWhiteSpace(knnOutput))
            return string.IsNullOrWhiteSpace(originalPrompt)
                ? "Capability identified."
                : $"Capability identified for: \"{originalPrompt.Trim()}\"";

        if (string.IsNullOrWhiteSpace(originalPrompt))
            return knnOutput;

        var authoritativeLines = ComparisonLines(knnOutput).ToArray();
        var promptLine = ContainsRecognizedPromptMetadata(authoritativeLines) ||
                         ContainsEquivalentRawPromptSequence(knnOutput, originalPrompt)
            ? string.Empty
            : $"For your request: \"{originalPrompt.Trim()}\"";

        return AppendAnnotations(knnOutput, authoritativeLines, [promptLine]);
    }

    public static string TranslateSignedGeneratedCapability(
        string knnOutput,
        string subPhaseId,
        string taskType,
        IReadOnlyList<string>? predicateIds,
        string originalPrompt)
    {
        var signedSubPhaseId = SnapshotSignedIdentifier(subPhaseId);
        var signedTaskType = SnapshotSignedIdentifier(taskType);
        var signedPredicateIds = SnapshotSignedPredicateIds(predicateIds);
        var signedOriginalPrompt = SnapshotSignedPrompt(originalPrompt);
        var authoritativeLines = ComparisonLines(knnOutput).ToArray();
        var promptLine = string.IsNullOrWhiteSpace(signedOriginalPrompt) ||
                         ContainsRecognizedPromptMetadata(authoritativeLines) ||
                         ContainsEquivalentRawPromptSequence(
                             knnOutput,
                             signedOriginalPrompt)
            ? string.Empty
            : $"For your request: \"{signedOriginalPrompt.Trim()}\"";
        var subPhaseLine = string.IsNullOrEmpty(signedSubPhaseId)
            ? string.Empty
            : string.IsNullOrEmpty(signedTaskType)
                ? $"Sub-phase {signedSubPhaseId}"
                : $"Sub-phase {signedSubPhaseId} ({signedTaskType})";
        var focusLine = signedPredicateIds.Length == 0
            ? string.Empty
            : $"Focus areas: {string.Join(", ", signedPredicateIds)}";

        return AppendAnnotations(
            knnOutput,
            authoritativeLines,
            [promptLine, subPhaseLine, focusLine]);
    }

    internal static string[] SnapshotSignedPredicateIds(
        IReadOnlyList<string>? predicateIds)
    {
        if (predicateIds is null ||
            predicateIds.Count is <= 0 or > MaximumSignedPredicateCount)
        {
            return [];
        }

        var snapshot = predicateIds.ToArray();
        return snapshot.All(static value =>
                !string.IsNullOrWhiteSpace(value) &&
                value.Length <= MaximumSignedIdentifierLength)
            ? snapshot
            : [];
    }

    private static string SnapshotSignedIdentifier(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaximumSignedIdentifierLength
            ? value
            : string.Empty;

    private static string SnapshotSignedPrompt(string? value)
        => value is not null && value.Length <= MaximumSignedPromptLength
            ? value
            : string.Empty;

    private static string AppendAnnotations(
        string authoritativeOutput,
        IReadOnlyList<string> authoritativeLines,
        IEnumerable<string> candidates)
    {
        var annotations = candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Where(candidate => !ContainsEquivalentLine(authoritativeLines, candidate))
            .ToArray();
        if (annotations.Length == 0)
            return authoritativeOutput;

        var annotationBlock = string.Join(Environment.NewLine, annotations);
        if (string.IsNullOrWhiteSpace(authoritativeOutput))
            return annotationBlock;

        var separator = authoritativeOutput.EndsWith('\r') || authoritativeOutput.EndsWith('\n')
            ? string.Empty
            : Environment.NewLine;
        return authoritativeOutput + separator + annotationBlock;
    }

    private static IEnumerable<string> ComparisonLines(string text)
    {
        if (text is null)
            yield break;

        using var reader = new StringReader(text);
        while (reader.ReadLine() is { } line)
            yield return line;
    }

    private static bool ContainsRecognizedPromptMetadata(IEnumerable<string> lines)
    {
        return lines.Any(line => TryExtractPromptMetadataValue(line, out _));
    }

    private static bool ContainsEquivalentRawPromptSequence(
        string authoritativeOutput,
        string originalPrompt)
    {
        var promptTokens = ComparisonTokens(originalPrompt);
        if (promptTokens.Length == 0)
            return false;

        var outputTokens = ComparisonTokens(authoritativeOutput);
        if (outputTokens.Length < promptTokens.Length)
            return false;

        for (var start = 0; start <= outputTokens.Length - promptTokens.Length; start++)
        {
            var matches = true;
            for (var offset = 0; offset < promptTokens.Length; offset++)
            {
                if (string.Equals(
                        outputTokens[start + offset],
                        promptTokens[offset],
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                matches = false;
                break;
            }

            if (matches)
                return true;
        }

        return false;
    }

    private static string[] ComparisonTokens(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return NormalizePromptValue(value)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool ContainsEquivalentLine(
        IEnumerable<string> authoritativeLines,
        string annotation)
    {
        var normalizedAnnotation = NormalizePromptValue(annotation);
        return authoritativeLines.Any(line =>
            string.Equals(
                NormalizePromptValue(line),
                normalizedAnnotation,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool TryExtractPromptMetadataValue(string line, out string value)
    {
        string[] supportedPrefixes = ["For your request:", "Prompt:", "Request:"];
        var candidate = line.TrimStart();
        foreach (var prefix in supportedPrefixes)
        {
            if (!candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            value = candidate[prefix.Length..];
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static string NormalizePromptValue(string value)
    {
        var normalized = value.Trim();
        if (normalized.Length >= 2 &&
            ((normalized[0] == '"' && normalized[^1] == '"') ||
             (normalized[0] == '\'' && normalized[^1] == '\'')))
        {
            normalized = normalized[1..^1].Trim();
        }

        return string.Join(
            " ",
            normalized.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
