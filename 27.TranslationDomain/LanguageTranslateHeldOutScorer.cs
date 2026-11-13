// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text;
using AgentInfrastructure;
using ContinuousLearning;

public sealed class LanguageTranslateHeldOutScorer : IHeldOutScorer
{
    public const string DomainTag = "language-translate";
    public const string NormalizationProfile =
        "unicode_nfc_trim_lf_ascii_whitespace_v1";
    public const string MetricProfile =
        "exact_match+char_accuracy_equal_weight_v1";
    public const string SelfAuthoredOrigin = "synthetic_self_authored";
    public const string HeldOutSplit = "held-out";

    private const int MaximumRows = 4_096;
    private const int MaximumRunesPerValue = 8_192;
    private const int MaximumUtf16CodeUnitsPerValue = 16_384;
    private const long MaximumEditCellsPerComparison = 8_000_000;
    private const long MaximumAggregateEditCells = 32_000_000;

    public LanguageTranslateHeldOutScorer(string scorerId, string scorerVersion)
    {
        if (!IsSafeIdentity(scorerId) || !IsSafeIdentity(scorerVersion))
            throw new ArgumentException("Explicit safe scorer id and version are required.");
        Identity = new HeldOutScorerIdentity(
            DomainTag,
            scorerId,
            scorerVersion,
            NormalizationProfile,
            MetricProfile);
    }

    public HeldOutScorerIdentity Identity { get; }

    public OperationResult<ProductAccuracyScore> Score(HeldOutScoringRequest request)
        => Score(request, CancellationToken.None);

    public OperationResult<ProductAccuracyScore> Score(
        HeldOutScoringRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null
            || !StringComparer.Ordinal.Equals(request.DomainTag, DomainTag)
            || !IsSafeIdentity(request.EvaluationSetId)
            || request.ExpectedLabelSource is null
            || !StringComparer.Ordinal.Equals(
                request.ExpectedLabelSource.Origin,
                SelfAuthoredOrigin)
            || request.ExpectedLabelSource.FromNodeUnderTest
            || !StringComparer.Ordinal.Equals(
                request.ExpectedLabelSource.SplitId,
                HeldOutSplit)
            || !IsCanonicalEvidenceRef(request.ExpectedLabelSource.AuthorityRef)
            || request.Rows is null
            || request.Rows.Count is 0 or > MaximumRows)
        {
            return Failure<ProductAccuracyScore>("HELD_OUT_SCORING_REQUEST_INVALID");
        }

        var seenCases = new HashSet<string>(StringComparer.Ordinal);
        long aggregateEditCells = 0;
        foreach (var row in request.Rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (row is null
                || !IsSafeIdentity(row.CaseId)
                || !seenCases.Add(row.CaseId)
                || row.ExpectedLabel is null
                || row.PredictedOutput is null)
            {
                return Failure<ProductAccuracyScore>("HELD_OUT_SCORING_ROW_INVALID");
            }
            var measured = MeasureEditCells(
                row.ExpectedLabel,
                row.PredictedOutput,
                cancellationToken);
            if (!measured.IsSuccess)
                return measured.MapError<ProductAccuracyScore>();
            if (measured.Value > MaximumAggregateEditCells - aggregateEditCells)
                return Failure<ProductAccuracyScore>(
                    "HELD_OUT_SCORING_WORK_LIMIT_EXCEEDED");
            aggregateEditCells += measured.Value;
        }

        var total = 0D;
        foreach (var row in request.Rows.OrderBy(
                     static row => row.CaseId,
                     StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var scored = ScoreOne(
                row.ExpectedLabel,
                row.PredictedOutput,
                cancellationToken);
            if (!scored.IsSuccess || scored.Value is null)
                return scored.MapError<ProductAccuracyScore>();
            total += scored.Value.CandidateScore;
        }

        return OperationResult<ProductAccuracyScore>.Ok(
            new ProductAccuracyScore(
                RuntimeProductAccuracy: total / request.Rows.Count,
                PerDomainMetricUsed: MetricProfile,
                ScoredRows: request.Rows.Count,
                ExpectedLabelOrigin: SelfAuthoredOrigin));
    }

    public static OperationResult<LanguageTranslateCandidateScore> ScoreOne(
        string expectedLabel,
        string predictedOutput)
        => ScoreOne(expectedLabel, predictedOutput, CancellationToken.None);

    public static OperationResult<LanguageTranslateCandidateScore> ScoreOne(
        string expectedLabel,
        string predictedOutput,
        CancellationToken cancellationToken)
    {
        var prepared = PrepareComparison(
            expectedLabel,
            predictedOutput,
            cancellationToken);
        if (!prepared.IsSuccess || prepared.Value is null)
            return prepared.MapError<LanguageTranslateCandidateScore>();
        var comparison = prepared.Value;

        var exact = StringComparer.Ordinal.Equals(
            comparison.Expected,
            comparison.Predicted);
        var accuracy = CharacterAccuracy(
            comparison.ExpectedRunes,
            comparison.PredictedRunes,
            cancellationToken);
        return OperationResult<LanguageTranslateCandidateScore>.Ok(
            new LanguageTranslateCandidateScore(
                ExactMatch: exact,
                CharacterAccuracy: accuracy,
                CandidateScore: 0.5D * (exact ? 1D : 0D) + 0.5D * accuracy));
    }

    public static OperationResult<long> EstimateWorkCells(
        string expectedLabel,
        string predictedOutput,
        CancellationToken cancellationToken)
    {
        var prepared = PrepareComparison(
            expectedLabel,
            predictedOutput,
            cancellationToken);
        if (!prepared.IsSuccess || prepared.Value is null)
            return prepared.MapError<long>();
        return OperationResult<long>.Ok(prepared.Value.EditCells);
    }

    private static OperationResult<PreparedComparison> PrepareComparison(
        string expectedLabel,
        string predictedOutput,
        CancellationToken cancellationToken)
    {
        if (expectedLabel is null || predictedOutput is null)
            return Failure<PreparedComparison>("HELD_OUT_SCORING_VALUE_REQUIRED");
        cancellationToken.ThrowIfCancellationRequested();
        if (expectedLabel.Length > MaximumUtf16CodeUnitsPerValue
            || predictedOutput.Length > MaximumUtf16CodeUnitsPerValue)
        {
            return Failure<PreparedComparison>(
                "HELD_OUT_SCORING_VALUE_LIMIT_EXCEEDED");
        }

        string expected;
        string predicted;
        try
        {
            expected = NormalizeBounded(expectedLabel);
            cancellationToken.ThrowIfCancellationRequested();
            predicted = NormalizeBounded(predictedOutput);
        }
        catch (ArgumentException)
        {
            return Failure<PreparedComparison>("HELD_OUT_SCORING_VALUE_INVALID");
        }
        cancellationToken.ThrowIfCancellationRequested();
        var expectedRunes = expected.EnumerateRunes().ToArray();
        var predictedRunes = predicted.EnumerateRunes().ToArray();
        if (expectedRunes.Length > MaximumRunesPerValue
            || predictedRunes.Length > MaximumRunesPerValue)
        {
            return Failure<PreparedComparison>(
                "HELD_OUT_SCORING_VALUE_LIMIT_EXCEEDED");
        }

        var editCells = (long)expectedRunes.Length * predictedRunes.Length;
        if (editCells > MaximumEditCellsPerComparison)
        {
            return Failure<PreparedComparison>(
                "HELD_OUT_SCORING_WORK_LIMIT_EXCEEDED");
        }

        return OperationResult<PreparedComparison>.Ok(new PreparedComparison(
            expected,
            predicted,
            expectedRunes,
            predictedRunes,
            editCells));
    }

    private static string NormalizeBounded(string value)
    {
        var normalized = value
            .Normalize(NormalizationForm.FormC)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        var runes = normalized.EnumerateRunes().ToArray();
        var first = 0;
        while (first < runes.Length && Rune.IsWhiteSpace(runes[first]))
            first++;
        var last = runes.Length - 1;
        while (last >= first && Rune.IsWhiteSpace(runes[last]))
            last--;

        var builder = new StringBuilder(normalized.Length);
        var inAsciiWhitespace = false;
        for (var index = first; index <= last; index++)
        {
            var rune = runes[index];
            if (IsAsciiWhitespace(rune))
            {
                if (!inAsciiWhitespace)
                    builder.Append(' ');
                inAsciiWhitespace = true;
                continue;
            }
            inAsciiWhitespace = false;
            builder.Append(rune.ToString());
        }
        return builder.ToString();
    }

    private static double CharacterAccuracy(
        Rune[] expected,
        Rune[] predicted,
        CancellationToken cancellationToken)
    {
        if (expected.Length == 0 && predicted.Length == 0)
            return 1D;
        if (expected.Length == 0 || predicted.Length == 0)
            return 0D;

        var shorter = expected;
        var longer = predicted;
        if (shorter.Length > longer.Length)
            (shorter, longer) = (longer, shorter);
        var previous = new int[shorter.Length + 1];
        var current = new int[shorter.Length + 1];
        for (var index = 0; index <= shorter.Length; index++)
            previous[index] = index;
        for (var row = 1; row <= longer.Length; row++)
        {
            if ((row & 63) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            current[0] = row;
            for (var column = 1; column <= shorter.Length; column++)
            {
                var replacement = previous[column - 1]
                    + (longer[row - 1] == shorter[column - 1] ? 0 : 1);
                current[column] = Math.Min(
                    Math.Min(previous[column] + 1, current[column - 1] + 1),
                    replacement);
            }
            (previous, current) = (current, previous);
        }
        var distance = previous[shorter.Length];
        return Math.Clamp(
            1D - (double)distance / Math.Max(expected.Length, predicted.Length),
            0D,
            1D);
    }

    private static OperationResult<long> MeasureEditCells(
        string expected,
        string predicted,
        CancellationToken cancellationToken)
    {
        return EstimateWorkCells(expected, predicted, cancellationToken);
    }

    private static bool IsAsciiWhitespace(Rune rune)
        => rune.Value is >= 0x09 and <= 0x0D or 0x20;

    private static bool IsSafeIdentity(string value)
        => !string.IsNullOrEmpty(value)
            && value.Length <= 128
            && char.IsAsciiLetterOrDigit(value[0])
            && value.All(static character =>
                char.IsAsciiLetterOrDigit(character)
                || character is '_' or '-' or '.' or ':');

    private static bool IsCanonicalEvidenceRef(string value)
        => value is not null
            && value.StartsWith("sha256:", StringComparison.Ordinal)
            && value.Length == 71
            && value.AsSpan(7).IndexOfAnyExcept("0123456789abcdef") < 0;

    private static OperationResult<T> Failure<T>(string code)
        => OperationResult<T>.Fail("Held-out language scoring input was rejected.", code);

    private static OperationResult<LanguageTranslateCandidateScore> Failure(string code)
        => Failure<LanguageTranslateCandidateScore>(code);

    private sealed record PreparedComparison(
        string Expected,
        string Predicted,
        Rune[] ExpectedRunes,
        Rune[] PredictedRunes,
        long EditCells);
}

public sealed record LanguageTranslateCandidateScore(
    bool ExactMatch,
    double CharacterAccuracy,
    double CandidateScore);
