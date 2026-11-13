// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;
using Domain = SemanticCompiler29.Domain;

// This record carries every execution flag that a logical unit, flow, or orchestrator can see.
public sealed record LogicalExecutionFlags
{
    // This list names behaviors that improve routing and learning quality.
    public static IReadOnlyList<string> OptimizationFlagNames { get; } = Array.AsReadOnly([
        nameof(UseCompiledPredicateRepresentation),
        nameof(UseDomainScopedRetrieval),
        nameof(UseLowConfidenceLessonSignal),
        nameof(UseLearnedUnitWhenAvailable),
        nameof(UseTraceEvidence),
        nameof(UseContextCarryForward),
        nameof(UseIdleLearningScheduling),
        nameof(UseE35DependencyDetection),
        nameof(UseSkillRetriever),
        nameof(UseReplayFusionRanking)
    ]);

    // This list names behaviors that protect correctness and user authority.
    public static IReadOnlyList<string> SafetyFlagNames { get; } = Array.AsReadOnly([
        nameof(UseArbiterAuthority),
        nameof(UseKnowledgeEdgeDetection),
        nameof(UseInternalFirstExecution),
        nameof(RequireExternalAiApproval),
        nameof(RejectArbiterContradictions)
    ]);

    // This list names flags that describe how an experiment or proof run is being executed.
    public static IReadOnlyList<string> ExperimentControlFlagNames { get; } = Array.AsReadOnly([
        nameof(UseDeterministicFixtureBaseline),
        nameof(WriteEvidenceArtifacts),
        nameof(CompareAgainstOracle),
        nameof(FailOnMetricDrift)
    ]);

    public bool UseCompiledPredicateRepresentation { get; init; } = true;
    public bool UseDomainScopedRetrieval { get; init; } = true;
    // FIX-LESSON-SIGNAL-01: признак включает ЗАКАЗ УРОКА при низкой уверенности,
    // а не отказ в ответе. Прежнее имя `UseConfidenceFloor` читалось как жёсткий
    // минимум, которого обязан достичь ответ, — и обманывало: ниже порога
    // обучаемая память соседей всё равно возвращает ответ ближайшего соседа и
    // лишь добавляет предупреждение «край знания». Это предупреждение — вход
    // учебного цикла: оркестратор заказывает по нему учебный пример и готовит
    // учебные ветви. Отказом его делать НЕЛЬЗЯ, и тому два измеренных
    // основания: в единственных четырёх наблюдённых случаях ниже порога ответ
    // был верным во всех четырёх, то есть отказ отверг бы четыре верных ответа
    // из четырёх; и отказ лишил бы учебный цикл его входа ровно там, где
    // обучение нужнее. Подробности и числа — в документе проекта обучаемой
    // инфраструктуры и в реестре оставшейся работы.
    public bool UseLowConfidenceLessonSignal { get; init; } = true;
    public bool UseLearnedUnitWhenAvailable { get; init; } = true;
    public bool UseTraceEvidence { get; init; } = true;
    public bool UseContextCarryForward { get; init; } = true;
    public bool UseIdleLearningScheduling { get; init; } = true;
    public bool UseE35DependencyDetection { get; init; }
    public bool UseSkillRetriever { get; init; }
    public bool UseReplayFusionRanking { get; init; }
    public bool UseArbiterAuthority { get; init; } = true;
    public bool UseKnowledgeEdgeDetection { get; init; } = true;
    public bool UseInternalFirstExecution { get; init; } = true;
    public bool RequireExternalAiApproval { get; init; } = true;
    public bool RejectArbiterContradictions { get; init; } = true;
    public bool UseDeterministicFixtureBaseline { get; init; }
    public bool WriteEvidenceArtifacts { get; init; } = true;
    public bool CompareAgainstOracle { get; init; } = true;
    public bool FailOnMetricDrift { get; init; } = true;

    // FIX-LESSON-SIGNAL-01: устаревший псевдоним прежнего имени. Своего
    // хранилища у него НЕТ — он читает настоящий признак и при создании
    // пересылает в него, поэтому разойтись с ним не может по устройству, и в
    // равенстве записи не участвует.
    //
    // Зачем псевдоним вообще оставлен, а не выброшен. Двадцать замороженных
    // хозяев опытов берут этот договор ссылкой на проект и называют прежнее
    // имя; среди них `P9501`, `P9601` и `P9701` — те самые опыты, которые
    // нашли и подтвердили этот дефект. Простое переименование оставило бы их
    // несобираемыми, то есть лишило бы проект возможности пересобрать и
    // перепроверить собственные доказательства. Псевдоним сохраняет их
    // сборку, выдавая предупреждение об устаревании вместо ошибки.
    //
    // В открытый перечень имён признаков псевдоним намеренно НЕ внесён: он не
    // отдельное поведение, и счёт имён там остаётся прежним.
    [Obsolete(
        "Имя обманывало: это не минимум уверенности, которого обязан достичь " +
        "ответ, а заказ урока при низкой уверенности. Ниже порога ответ " +
        "возвращается с предупреждением, и это предупреждение — вход учебного " +
        "цикла. Используйте UseLowConfidenceLessonSignal.")]
    public bool UseConfidenceFloor
    {
        get => UseLowConfidenceLessonSignal;
        init => UseLowConfidenceLessonSignal = value;
    }
}

// This record is the input envelope every logical unit receives.
public sealed record LogicalUnitRequest<TInput>(
    string RunId,
    string UnitId,
    string UnitRole,
    string DomainTag,
    TInput Input,
    LogicalExecutionFlags Flags,
    IReadOnlyDictionary<string, object> Metadata);

// This record is the output envelope every logical unit returns.
public sealed record LogicalUnitResult<TOutput>(
    string UnitId,
    string UnitRole,
    string DomainTag,
    TOutput Output,
    double Confidence,
    string[] PredicateIds,
    string[] DomainTags,
    string[] EvidenceIds,
    string[] Warnings)
{
    // TransitionSignal is optional. Null means this unit requests no follow-up transition.
    // Keep it off the positional constructor so existing unit result call sites stay stable.
    public PhaseTransitionSignal? TransitionSignal { get; init; }
}

// This record carries an advisory request from a flow node to re-enter orchestration after the current branch completes.
public sealed record PhaseTransitionSignal(
    Domain SuggestedDomain,
    string TransitionPrompt,
    string SourceNodeId,
    string SourceSubPhaseId,
    string[] EvidenceIds);

// This interface is the smallest executable contract for a dynamic-flow cell.
public interface ILogicalUnit<TInput, TOutput>
{
    Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteAsync(
        LogicalUnitRequest<TInput> request,
        CancellationToken cancellationToken = default);
}

// This record carries a request to execute one logical flow.
public sealed record LogicalFlowRequest<TInput>(
    string RunId,
    string FlowId,
    string ScenarioId,
    TInput Input,
    LogicalExecutionFlags Flags,
    IReadOnlyDictionary<string, object> Metadata);

// This record carries a runtime-neutral summary of a flow or experiment run.
public sealed record LogicalFlowSummary(
    string SummaryId,
    int Tier,
    double BaselinePrimary,
    double EnhancedPrimary,
    string PrimaryMetricName,
    bool LowerIsBetter,
    bool Passed,
    bool ShouldScaleUp,
    string NextAction,
    double ScalingSignal,
    string ScalingSignalNote,
    string Notes,
    double BaselineTimeMs,
    double EnhancedTimeMs);

// This enum classifies whether content offered to capability training has a
// typed source attestation. The zero value is deliberately inadmissible.
public enum LogicalTrainingContentSourceKind
{
    Unattested,
    DeterministicSyntheticNoExternalTeacher
}

public sealed record LogicalTrainingContentDeterministicInputs(
    string OriginalUserRequest,
    string DomainAdapterTag,
    string PhaseId,
    string SubPhaseId,
    string SubPhaseName,
    string SubPhasePurpose,
    IReadOnlyList<string> PredicateIds,
    IReadOnlyList<string> DomainIds);

// This opaque neutral attestation binds a trusted source classification to the
// exact deep flow snapshot and, for training-scoped issuances, every
// deterministic input used to derive DPO content. Only AgentInfrastructure can
// issue it; public callers can revalidate but cannot construct an attestation.
public sealed class LogicalTrainingContentProvenance
{
    private const int MaximumStringLength = 16_384;
    private const int MaximumMetricCount = 256;
    private const int MaximumEvidenceCount = 256;
    private const int MaximumWarningCount = 256;
    private const int MaximumTrainingInputCount = 256;

    private readonly ReadOnlyCollection<string>
        _authorizedReplaySourceBindingSha256s;

    private LogicalTrainingContentProvenance(
        LogicalTrainingContentSourceKind sourceKind,
        string bindingSha256,
        bool bindsTrainingInputs,
        string? currentDpoPreferenceSha256,
        string? replaySealGroupSha256,
        IReadOnlyList<string>? authorizedReplaySourceBindingSha256s,
        string? authorizedReplayPayloadSha256,
        string? authorizedReplayDpoPreferenceSha256)
    {
        SourceKind = sourceKind;
        BindingSha256 = bindingSha256;
        BindsTrainingInputs = bindsTrainingInputs;
        CurrentDpoPreferenceSha256 = currentDpoPreferenceSha256;
        ReplaySealGroupSha256 = replaySealGroupSha256;
        _authorizedReplaySourceBindingSha256s = Array.AsReadOnly(
            authorizedReplaySourceBindingSha256s?.ToArray() ?? []);
        AuthorizedReplayPayloadSha256 = authorizedReplayPayloadSha256;
        AuthorizedReplayDpoPreferenceSha256 =
            authorizedReplayDpoPreferenceSha256;
    }

    public LogicalTrainingContentSourceKind SourceKind { get; }

    public string BindingSha256 { get; }

    public bool BindsTrainingInputs { get; }

    /// <summary>
    /// Exact issuer-derived digest of the current Prompt/Chosen/Rejected/vector
    /// projection authorized for this source. It is absent on source-only
    /// attestations.
    /// </summary>
    public string? CurrentDpoPreferenceSha256 { get; }

    /// <summary>
    /// Opaque issuer-derived identity for the exact request/domain/phase/
    /// sub-phase group whose distinct current-source bindings may seed one
    /// replay descriptor. CapabilityTraining can copy but cannot issue or
    /// broaden this identity.
    /// </summary>
    public string? ReplaySealGroupSha256 { get; }

    /// <summary>
    /// Exact strictly canonical source composite authorized for the replay
    /// projection of this group.
    /// </summary>
    public IReadOnlyList<string> AuthorizedReplaySourceBindingSha256s =>
        _authorizedReplaySourceBindingSha256s;

    public string? AuthorizedReplayPayloadSha256 { get; }

    public string? AuthorizedReplayDpoPreferenceSha256 { get; }

    public bool IsCanonical =>
        SourceKind == LogicalTrainingContentSourceKind
            .DeterministicSyntheticNoExternalTeacher &&
        IsCanonicalSha256(BindingSha256) &&
        (BindsTrainingInputs
            ? IsCanonicalSha256(CurrentDpoPreferenceSha256) &&
              IsCanonicalSha256(ReplaySealGroupSha256) &&
              IsStrictCanonicalSha256Composite(
                  _authorizedReplaySourceBindingSha256s) &&
              _authorizedReplaySourceBindingSha256s.Contains(
                  BindingSha256,
                  StringComparer.Ordinal) &&
              IsCanonicalSha256(AuthorizedReplayPayloadSha256) &&
              IsCanonicalSha256(AuthorizedReplayDpoPreferenceSha256)
            : CurrentDpoPreferenceSha256 is null &&
              ReplaySealGroupSha256 is null &&
              _authorizedReplaySourceBindingSha256s.Count == 0 &&
              AuthorizedReplayPayloadSha256 is null &&
              AuthorizedReplayDpoPreferenceSha256 is null);

    public bool Attests(LogicalFlowResult? source)
    {
        if (source is null || !IsCanonical || BindsTrainingInputs ||
            !TryComputeBindingSha256(source, null, out var bindingSha256))
        {
            return false;
        }

        return StringComparer.Ordinal.Equals(
            BindingSha256,
            bindingSha256);
    }

    public bool Attests(
        LogicalFlowResult? source,
        LogicalTrainingContentDeterministicInputs? inputs)
    {
        if (source is null || inputs is null || !IsCanonical ||
            !BindsTrainingInputs ||
            !TryComputeBindingSha256(source, inputs, out var bindingSha256) ||
            !TryComputeReplaySealGroupSha256(
                inputs,
                out var replaySealGroupSha256))
        {
            return false;
        }
        return StringComparer.Ordinal.Equals(BindingSha256, bindingSha256) &&
               StringComparer.Ordinal.Equals(
                    ReplaySealGroupSha256,
                    replaySealGroupSha256);
    }

    public bool AuthorizesCurrentPreference(string? dpoPreferenceSha256)
        => IsCanonical && BindsTrainingInputs &&
           IsCanonicalSha256(dpoPreferenceSha256) &&
           StringComparer.Ordinal.Equals(
               CurrentDpoPreferenceSha256,
               dpoPreferenceSha256);

    public bool AuthorizesReplay(
        string? replaySealGroupSha256,
        IReadOnlyList<string>? sourceBindingSha256s,
        string? replayPayloadSha256,
        string? dpoPreferenceSha256)
    {
        if (!IsCanonical || !BindsTrainingInputs ||
            !IsCanonicalSha256(replaySealGroupSha256) ||
            sourceBindingSha256s is null ||
            !IsStrictCanonicalSha256Composite(sourceBindingSha256s) ||
            !IsCanonicalSha256(replayPayloadSha256) ||
            !IsCanonicalSha256(dpoPreferenceSha256))
        {
            return false;
        }

        return StringComparer.Ordinal.Equals(
                   ReplaySealGroupSha256,
                   replaySealGroupSha256) &&
               _authorizedReplaySourceBindingSha256s.SequenceEqual(
                   sourceBindingSha256s,
                   StringComparer.Ordinal) &&
               StringComparer.Ordinal.Equals(
                   AuthorizedReplayPayloadSha256,
                   replayPayloadSha256) &&
               StringComparer.Ordinal.Equals(
                   AuthorizedReplayDpoPreferenceSha256,
                   dpoPreferenceSha256);
    }

    public LogicalTrainingContentProvenance Snapshot()
        => new(
            SourceKind,
            BindingSha256,
            BindsTrainingInputs,
            CurrentDpoPreferenceSha256,
            ReplaySealGroupSha256,
            _authorizedReplaySourceBindingSha256s,
            AuthorizedReplayPayloadSha256,
            AuthorizedReplayDpoPreferenceSha256);

    internal static LogicalTrainingContentProvenance
        IssueDeterministicSynthetic(LogicalFlowResult source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!TryComputeBindingSha256(source, null, out var bindingSha256))
        {
            throw new InvalidOperationException(
                "Logical training content provenance source is invalid.");
        }
        return new LogicalTrainingContentProvenance(
            LogicalTrainingContentSourceKind
                .DeterministicSyntheticNoExternalTeacher,
            bindingSha256,
            bindsTrainingInputs: false,
            currentDpoPreferenceSha256: null,
            replaySealGroupSha256: null,
            authorizedReplaySourceBindingSha256s: null,
            authorizedReplayPayloadSha256: null,
            authorizedReplayDpoPreferenceSha256: null);
    }

    internal static LogicalTrainingContentProvenance
        IssueDeterministicSynthetic(
            LogicalFlowResult source,
            LogicalTrainingContentDeterministicInputs inputs,
            string currentDpoPreferenceSha256,
            IReadOnlyList<string> authorizedReplaySourceBindingSha256s,
            string authorizedReplayPayloadSha256,
            string authorizedReplayDpoPreferenceSha256)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(authorizedReplaySourceBindingSha256s);
        if (!TryComputeBindingSha256(source, inputs, out var bindingSha256) ||
            !TryComputeReplaySealGroupSha256(
                inputs,
                out var replaySealGroupSha256) ||
            !IsCanonicalSha256(currentDpoPreferenceSha256) ||
            !IsStrictCanonicalSha256Composite(
                authorizedReplaySourceBindingSha256s) ||
            !authorizedReplaySourceBindingSha256s.Contains(
                bindingSha256,
                StringComparer.Ordinal) ||
            !IsCanonicalSha256(authorizedReplayPayloadSha256) ||
            !IsCanonicalSha256(authorizedReplayDpoPreferenceSha256))
        {
            throw new InvalidOperationException(
                "Logical training content provenance source is invalid.");
        }
        return new LogicalTrainingContentProvenance(
            LogicalTrainingContentSourceKind
            .DeterministicSyntheticNoExternalTeacher,
            bindingSha256,
            bindsTrainingInputs: true,
            currentDpoPreferenceSha256,
            replaySealGroupSha256,
            authorizedReplaySourceBindingSha256s,
            authorizedReplayPayloadSha256,
            authorizedReplayDpoPreferenceSha256);
    }

    internal static bool TryComputeDeterministicTrainingIdentities(
        LogicalFlowResult source,
        LogicalTrainingContentDeterministicInputs inputs,
        out string bindingSha256,
        out string replaySealGroupSha256)
    {
        bindingSha256 = string.Empty;
        replaySealGroupSha256 = string.Empty;
        return TryComputeBindingSha256(
                   source,
                   inputs,
                   out bindingSha256) &&
               TryComputeReplaySealGroupSha256(
                   inputs,
                   out replaySealGroupSha256);
    }

    private static bool TryComputeReplaySealGroupSha256(
        LogicalTrainingContentDeterministicInputs inputs,
        out string replaySealGroupSha256)
    {
        replaySealGroupSha256 = string.Empty;
        try
        {
            if (!IsBoundedText(inputs.OriginalUserRequest) ||
                !IsBoundedText(inputs.DomainAdapterTag) ||
                !IsBoundedText(inputs.PhaseId) ||
                !IsBoundedText(inputs.SubPhaseId))
            {
                return false;
            }
            var canonical = new
            {
                Schema = "logical-capability-replay-seal-group-v1",
                inputs.OriginalUserRequest,
                inputs.DomainAdapterTag,
                inputs.PhaseId,
                inputs.SubPhaseId
            };
            replaySealGroupSha256 = ComputeSha256(
                JsonSerializer.SerializeToUtf8Bytes(canonical));
            return true;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            replaySealGroupSha256 = string.Empty;
            return false;
        }
    }

    private static bool TryComputeBindingSha256(
        LogicalFlowResult source,
        LogicalTrainingContentDeterministicInputs? inputs,
        out string bindingSha256)
    {
        bindingSha256 = string.Empty;
        try
        {
            string[]? predicateIds = null;
            string[]? domainIds = null;
            if (inputs is not null &&
                (inputs.PredicateIds is null || inputs.DomainIds is null ||
                 !TrySnapshotStrings(
                     inputs.PredicateIds,
                     MaximumTrainingInputCount,
                     out predicateIds) ||
                 !TrySnapshotStrings(
                     inputs.DomainIds,
                     MaximumTrainingInputCount,
                     out domainIds)))
            {
                return false;
            }

            if (source.Summary is null || source.Metrics is null ||
                source.EvidenceIds is null || source.Warnings is null ||
                source.TransitionSignals is null ||
                source.EdgeTargetHandoff is not null ||
                source.TransitionSignals.Length != 0 ||
                !IsBoundedText(source.FlowId) ||
                !IsBoundedText(source.ScenarioId) ||
                !IsBoundedText(source.OverallVerdict) ||
                !IsBoundedText(source.FinalHumanResult) ||
                !IsBoundedText(source.Summary.SummaryId) ||
                !IsBoundedText(source.Summary.PrimaryMetricName) ||
                !IsBoundedText(source.Summary.NextAction) ||
                !IsBoundedText(source.Summary.ScalingSignalNote) ||
                !IsBoundedText(source.Summary.Notes) ||
                !TrySnapshotMetrics(source.Metrics, out var metrics) ||
                !TrySnapshotStrings(
                    source.EvidenceIds,
                    MaximumEvidenceCount,
                    out var evidenceIds) ||
                !TrySnapshotStrings(
                    source.Warnings,
                    MaximumWarningCount,
                    out var warnings) ||
                (inputs is not null &&
                 (!IsBoundedText(inputs.OriginalUserRequest) ||
                  !IsBoundedText(inputs.DomainAdapterTag) ||
                  !IsBoundedText(inputs.PhaseId) ||
                  !IsBoundedText(inputs.SubPhaseId) ||
                  !IsBoundedText(inputs.SubPhaseName) ||
                  !IsBoundedText(inputs.SubPhasePurpose))))
            {
                return false;
            }
            var canonical = new
            {
                source.FlowId,
                source.ScenarioId,
                source.OverallVerdict,
                source.FinalHumanResult,
                Summary = source.Summary,
                Metrics = metrics,
                EvidenceIds = evidenceIds,
                Warnings = warnings,
                TransitionSignals = Array.Empty<object>(),
                EdgeTargetHandoff = (object?)null,
                TrainingInputs = inputs is null ? null : new
                {
                    inputs.OriginalUserRequest,
                    inputs.DomainAdapterTag,
                    inputs.PhaseId,
                    inputs.SubPhaseId,
                    inputs.SubPhaseName,
                    inputs.SubPhasePurpose,
                    PredicateIds = predicateIds,
                    DomainIds = domainIds
                }
            };
            bindingSha256 = ComputeSha256(
                JsonSerializer.SerializeToUtf8Bytes(canonical));
            return true;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            bindingSha256 = string.Empty;
            return false;
        }
    }

    private static bool TrySnapshotMetrics(
        IReadOnlyDictionary<string, string> source,
        out KeyValuePair<string, string>[] snapshot)
    {
        snapshot = [];
        var values = new List<KeyValuePair<string, string>>(
            Math.Min(source.Count, MaximumMetricCount));
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (values.Count == MaximumMetricCount ||
                !IsBoundedText(enumerator.Current.Key) ||
                !IsBoundedText(enumerator.Current.Value))
            {
                return false;
            }
            values.Add(enumerator.Current);
        }
        snapshot = values
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .ToArray();
        return snapshot
            .Select(static pair => pair.Key)
            .Distinct(StringComparer.Ordinal)
            .Count() == snapshot.Length;
    }

    private static bool TrySnapshotStrings(
        IEnumerable<string> source,
        int maximumCount,
        out string[] snapshot)
    {
        snapshot = [];
        var values = new List<string>(Math.Min(maximumCount, 64));
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (values.Count == maximumCount ||
                !IsBoundedText(enumerator.Current))
            {
                return false;
            }
            values.Add(enumerator.Current);
        }
        snapshot = values.ToArray();
        return true;
    }

    private static bool IsBoundedText(string? value)
        => value is not null && value.Length <= MaximumStringLength;

    private static string ComputeSha256(byte[] content)
        => Convert.ToHexString(
                SHA256.HashData(content))
            .ToLowerInvariant();

    private static bool IsCanonicalSha256(string? value)
        => value is not null &&
           value.Length == 64 &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsStrictCanonicalSha256Composite(
        IReadOnlyList<string> values)
    {
        if (values.Count is 0 or > MaximumTrainingInputCount)
            return false;
        string? previous = null;
        for (var index = 0; index < values.Count; index++)
        {
            var current = values[index];
            if (!IsCanonicalSha256(current) ||
                previous is not null &&
                StringComparer.Ordinal.Compare(previous, current) >= 0)
            {
                return false;
            }
            previous = current;
        }
        return true;
    }
}

// This record carries the normalized result of a logical flow execution.
public sealed record LogicalFlowResult(
    string FlowId,
    string ScenarioId,
    string OverallVerdict,
    string FinalHumanResult,
    LogicalFlowSummary Summary,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds,
    string[] Warnings)
{
    // TransitionSignals carries advisory post-branch transition requests emitted by flow nodes.
    public PhaseTransitionSignal[] TransitionSignals { get; init; } = [];

    // EdgeTargetHandoff is inference-only. Null means this flow produced no learned edge target handoff.
    // It stays off the positional constructor so all existing flow-result call sites remain source compatible.
    public DynamicGraphEdgeTargetHandoff? EdgeTargetHandoff { get; init; }

    // Null or Unattested content is never eligible for capability training. A
    // trusted training-scoped issuance binds the complete deep snapshot and all
    // deterministic topology/request inputs used to derive preference bytes.
    public LogicalTrainingContentProvenance? TrainingContentProvenance { get; init; }
}

// This record carries one completed branch as compact predicate-level context for downstream branches.
public sealed record LogicalContextFragment(
    string FlowId,
    string ScenarioId,
    string OverallVerdict,
    string FinalHumanResult,
    string? PhaseId,
    string? SubPhaseId,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds,
    string[] Warnings);

// This record carries every upstream branch fragment that a downstream branch may inherit.
public sealed record LogicalContextPacket(
    string RunId,
    string[] SourceFlowIds,
    string[] SourceScenarioIds,
    LogicalContextFragment[] Fragments,
    DateTimeOffset CreatedAtUtc);

// This interface is the executable contract for a topology of logical units.
public interface ILogicalFlow<TInput>
{
    Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<TInput> request,
        CancellationToken cancellationToken = default);
}

// This record carries a user-level request into the logical orchestrator.
public sealed record LogicalOrchestratorRequest(
    string RunId,
    string ScenarioId,
    string UserRequest,
    LogicalExecutionFlags? Flags,
    IReadOnlyDictionary<string, object> Metadata);

// This record carries the assistant-level result returned by the logical orchestrator.
public sealed record LogicalOrchestratorResult(
    string ScenarioId,
    string SelectedFlowId,
    string OverallVerdict,
    string FinalHumanResult,
    LogicalExecutionFlags AppliedFlags,
    LogicalFlowResult FlowResult)
{
    public LogicalFlowResult[] BranchResults { get; init; } = [];
    public string[] Warnings { get; init; } = [];
    public LogicalContextPacket? OutputContextPacket { get; init; }
}

// This record binds one route or scenario to one executable logical flow.
public sealed record LogicalFlowBinding(string FlowId, ILogicalFlow<string> Flow)
{
    public string ScenarioId { get; init; } = "";
}

// This interface is the assistant-level coordinator over logical flows.
public interface ILogicalOrchestrator
{
    Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default);
}

// Trust level classification used by the E40V2 trust scorer.
public enum TrustLevel { Low, Medium, High }

// Input features for E40V2 trust scoring.
public sealed record SourceFeatures(
    bool IsInternal,
    bool HasCIPassing,
    bool HasTests,
    int UpdateDaysAgo,
    int PopularityScore,
    bool IsOfficialDocs,
    bool DomainMatch);

// Output of E40V2 trust scoring.
public sealed record TrustScorerOutput(double Score, TrustLevel Level);

// Shared trust scoring contract consumed by project 21 without reflecting into project 16.
public interface ITrustScorer
{
    OperationResult<TrustScorerOutput> Score(SourceFeatures features);
}
