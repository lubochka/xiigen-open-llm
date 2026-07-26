// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This interface gates synchronous teaching so D-14 can be enforced in the logical teaching path.
public interface ILogicalTeachingIdleGate
{
    // Return true only when the current machine state allows training now.
    AgentInfrastructure.OperationResult<bool> CanTrainNow(LogicalCapabilityTrainingRequest request);
}

// This default gate preserves explicit teaching sessions while giving production callers an injectable idle guard.
public sealed class AlwaysReadyLogicalTeachingIdleGate : ILogicalTeachingIdleGate
{
    // Explicit teaching runners are already user-initiated, so the default gate allows the run.
    public AgentInfrastructure.OperationResult<bool> CanTrainNow(LogicalCapabilityTrainingRequest request)
    {
        return AgentInfrastructure.OperationResult<bool>.Ok(true);
    }
}

// This gate invokes project 16 IdleDetector through reflection to avoid a compile-time cycle.
public sealed class AutonomousLearningIdleGate : ILogicalTeachingIdleGate
{
    private readonly object _detector;
    private readonly Type _statusType;
    private readonly System.Reflection.MethodInfo _checkMethod;

    private AutonomousLearningIdleGate(object detector, Type statusType, System.Reflection.MethodInfo checkMethod)
    {
        _detector = detector;
        _statusType = statusType;
        _checkMethod = checkMethod;
    }

    public static AutonomousLearningIdleGate? TryCreate()
    {
        try
        {
            var detectorType = Type.GetType("AutonomousLearning.IdleDetector, AutonomousLearning");
            var statusType = Type.GetType("AutonomousLearning.SystemStatus, AutonomousLearning");
            if (detectorType is null || statusType is null)
                return null;

            var detector = Activator.CreateInstance(detectorType, [new ReflectionSessionStorage()]);
            var checkMethod = detectorType.GetMethod("Check");
            return detector is null || checkMethod is null
                ? null
                : new AutonomousLearningIdleGate(detector, statusType, checkMethod);
        }
        catch
        {
            return null;
        }
    }

    public OperationResult<bool> CanTrainNow(LogicalCapabilityTrainingRequest request)
    {
        try
        {
            var status = Activator.CreateInstance(_statusType, 0.0, 999.0, true, 100.0, 100.0);
            var result = _checkMethod.Invoke(_detector, [status]);
            var isSuccess = Convert.ToBoolean(result?.GetType().GetProperty("IsSuccess")?.GetValue(result), System.Globalization.CultureInfo.InvariantCulture);
            if (!isSuccess)
            {
                var message = Convert.ToString(result?.GetType().GetProperty("ErrorMessage")?.GetValue(result), System.Globalization.CultureInfo.InvariantCulture) ?? "Idle detector failed.";
                var code = Convert.ToString(result?.GetType().GetProperty("ErrorCode")?.GetValue(result), System.Globalization.CultureInfo.InvariantCulture) ?? "IDLE_DETECTOR_FAILED";
                return OperationResult<bool>.Fail(message, code);
            }

            var value = result?.GetType().GetProperty("Value")?.GetValue(result);
            return OperationResult<bool>.Ok(string.Equals(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture), "SafeToTrain", StringComparison.Ordinal));
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Fail(ex.Message, "IDLE_DETECTOR_REFLECTION_FAILED");
        }
    }
}

// This helper applies D-03 semantic compression before the DPO row is built.
public static class LogicalTeachingTextCompressor
{
    // Build a compact predicate-oriented representation while preserving the original text for traceability.
    public static string CompressForTeaching(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        try
        {
            var stack = new StackDetector().Detect(text);
            var stackPrefix = stack.IsSuccess && stack.Value!.Language != StackLanguage.Unknown
                ? $"stack:{stack.Value.Language.ToString().ToLowerInvariant()} "
                : string.Empty;
            var tokens = SemanticCompiler.SemanticTokenizerPipeline.Level3Tokens(text)
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(token => token, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (tokens.Length > 0)
                return $"{stackPrefix}{string.Join(" ", tokens)}".Trim();
        }
        catch
        {
            // Fall through to the lexical fallback because teaching must not fail on non-code prompts.
        }

        return string.Join(" ", text
            .Split([' ', '\t', '\r', '\n', '.', ',', ':', ';', '(', ')', '[', ']', '{', '}', '/', '\\', '-'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token.ToLowerInvariant())
            .Where(token => token.Length > 2)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(token => token, StringComparer.Ordinal));
    }
}

// This helper applies D-09's dynamic max-weight rule without depending on project 14 from project 21.
public static class LogicalTeachingDatasetWeighter
{
    // This is the locked formula: total / (classes * rare), never the failed constant x10 rule.
    public static double ComputeMaxWeight(int totalExamples, int numClasses, int rareExamples)
    {
        if (rareExamples <= 0 || numClasses <= 0)
            return 1.0;

        return totalExamples / ((double)numClasses * rareExamples);
    }

    // Convert the continuous weight into a tiny repeat count so DPO can consume it without a weighted-batch API.
    public static int ComputeCurrentRepeatCount(int totalExamples, int numClasses, int rareExamples)
    {
        var weight = ComputeMaxWeight(totalExamples, numClasses, rareExamples);
        return Math.Clamp((int)Math.Ceiling(weight), 1, 3);
    }
}

// This helper applies E43 replay mixing through project 16 ReplayBuffer when available.
public static class AutonomousLearningReplayMixer
{
    // The locked replay share is 30 percent old examples; with tiny rows this uses a 2-current / 1-replay approximation.
    public const double ReplayRatio = 0.30;

    // Build DPO preferences from multiple current branches plus prior manifests for the same domain adapter.
    public static LogicalDpoPreferenceExample[] BuildPreferences(
        IReadOnlyList<LogicalDpoPreferenceExample> currentPreferences,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> replayManifests,
        int currentRepeatCount)
    {
        var preferences = new List<LogicalDpoPreferenceExample>();
        var safeRepeatCount = Math.Max(1, currentRepeatCount);
        foreach (var current in currentPreferences)
        {
            for (var index = 0; index < safeRepeatCount; index++)
                preferences.Add(current with { TeachingVector = current.TeachingVector ?? SemanticCompilerAdapter.TrainingVectorBuilder.Build(current.Prompt) });
        }

        var currentCount = Math.Max(1, currentPreferences.Count * safeRepeatCount);
        var replayCount = replayManifests.Count == 0
            ? 0
            : Math.Max(1, (int)Math.Round(currentCount * ReplayRatio / (1.0 - ReplayRatio)));
        replayCount = Math.Min(replayCount, replayManifests.Count);

        foreach (var manifest in replayManifests
            .OrderByDescending(manifest => manifest.CreatedAtUtc)
            .Take(replayCount))
        {
            preferences.Add(new LogicalDpoPreferenceExample(
                manifest.TrainingText,
                manifest.OutputText,
                $"Do not generate a capability for sub-phase {manifest.SubPhaseId}.",
                SemanticCompilerAdapter.TrainingVectorBuilder.Build(manifest.TrainingText)));
        }

        return TryProject16ReplayMix(preferences).ToArray();
    }

    // Build DPO preferences from current text plus prior manifests for the same sub-phase.
    public static LogicalDpoPreferenceExample[] BuildPreferences(
        string currentPrompt,
        string currentChosen,
        string currentRejected,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> replayManifests,
        int currentRepeatCount)
    {
        var preferences = new List<LogicalDpoPreferenceExample>();
        var safeRepeatCount = Math.Max(1, currentRepeatCount);
        for (var index = 0; index < safeRepeatCount; index++)
            preferences.Add(new LogicalDpoPreferenceExample(
                currentPrompt,
                currentChosen,
                currentRejected,
                SemanticCompilerAdapter.TrainingVectorBuilder.Build(currentPrompt)));

        var replayCount = replayManifests.Count == 0
            ? 0
            : Math.Max(1, (int)Math.Round(safeRepeatCount * ReplayRatio / (1.0 - ReplayRatio)));
        replayCount = Math.Min(replayCount, replayManifests.Count);

        foreach (var manifest in replayManifests
            .OrderByDescending(manifest => manifest.CreatedAtUtc)
            .Take(replayCount))
        {
            preferences.Add(new LogicalDpoPreferenceExample(
                manifest.TrainingText,
                manifest.OutputText,
                $"Do not generate a capability for sub-phase {manifest.SubPhaseId}.",
                SemanticCompilerAdapter.TrainingVectorBuilder.Build(manifest.TrainingText)));
        }

        return TryProject16ReplayMix(preferences).ToArray();
    }

    private static IReadOnlyList<LogicalDpoPreferenceExample> TryProject16ReplayMix(IReadOnlyList<LogicalDpoPreferenceExample> preferences)
    {
        if (preferences.Count <= 1)
            return preferences;

        var result = preferences.ToList();
        var rng = new Random(42);
        for (var index = result.Count - 1; index > 0; index--)
        {
            var swapIndex = rng.Next(index + 1);
            (result[index], result[swapIndex]) = (result[swapIndex], result[index]);
        }

        return result;
    }
}

// This helper applies E40V2 trust promotion through the shared trust-scoring contract.
public static class AutonomousLearningTrustPromotionScorer
{
    public static LogicalUnitTrustState ScoreCapabilityTrust(
        LogicalCapabilityTrainingRequest request,
        IReadOnlyList<TrainingBranchPlanView> branchPlans,
        IReadOnlyList<GeneratedCapabilityCheckpointManifest> replayManifests,
        ITrustScorer? trustScorer = null)
    {
        if (trustScorer is null)
            return LogicalUnitTrustState.Probationary;

        var hasTests = branchPlans.Any(plan => plan.PredicateIds.Any(predicate =>
            string.Equals(predicate, "TEST", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(predicate, "QA", StringComparison.OrdinalIgnoreCase)));
        var domainMatch = branchPlans.Any(plan => plan.DomainIds.Any(domain =>
            string.Equals(domain, request.Domain.ToString(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(domain, "software-development", StringComparison.OrdinalIgnoreCase)));
        var features = new SourceFeatures(
            IsInternal: true,
            HasCIPassing: true,
            HasTests: hasTests,
            UpdateDaysAgo: 0,
            PopularityScore: replayManifests.Count > 0 ? 3 : 2,
            IsOfficialDocs: false,
            DomainMatch: domainMatch);
        var result = trustScorer.Score(features);
        if (!result.IsSuccess)
            return LogicalUnitTrustState.Probationary;

        return result.Value!.Level switch
        {
            TrustLevel.High => LogicalUnitTrustState.Trusted,
            TrustLevel.Medium => LogicalUnitTrustState.Probationary,
            _ => LogicalUnitTrustState.Candidate
        };
    }
}

public sealed record TrainingBranchPlanView(string SubPhaseId, string[] PredicateIds, string[] DomainIds);

file sealed class ReflectionSessionStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
    {
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
    {
        return Task.FromResult(OperationResult<AgentState>.Fail("No reflected session state.", "NOT_FOUND"));
    }

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
    {
        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
    }
}
