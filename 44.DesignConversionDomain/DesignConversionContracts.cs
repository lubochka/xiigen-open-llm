// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using ContinuousLearning;
using DPO.Models;

public enum DesignConversionSourceKind
{
    Figma,
    Canva,
    Other
}

public sealed record DesignConversionBudgetMetadata(
    string UserId,
    string Tier,
    int MaxImprovementCandidates,
    int MaxRenderQaRuns,
    decimal BudgetUsd,
    IReadOnlyDictionary<string, string> Metadata,
    double? RenderQaPassThreshold = null,
    int? MaxImprovementIterations = null,
    int? MaxTeacherCalls = null);

public sealed record DesignTreePayload(
    string PayloadId,
    string Format,
    string Content,
    string Sha256,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignRenderArtifact(
    string ArtifactId,
    string Kind,
    int Width,
    int Height,
    string Uri,
    string Sha256,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record SubmittedDesignConversionRequest(
    string RequestId,
    DesignConversionSourceKind SourceKind,
    string Html,
    string Css,
    DesignTreePayload DesignTreePayload,
    DesignTreePayload OriginalTreePayload,
    IReadOnlyList<DesignRenderArtifact> Screenshots,
    IReadOnlyList<DesignRenderArtifact> DesignRenders,
    DesignConversionBudgetMetadata Budget,
    IReadOnlyDictionary<string, string> Provenance)
{
    /// <summary>
    /// Carries the repository-owned DPO transaction authority beside the submitted
    /// request so the thin-client runtime can thread it into the live teacher path
    /// without mutating the legacy internal evaluation DTO.
    /// </summary>
    public DesignConversionRepositoryOwnedDpoTransactionContext?
        RepositoryOwnedDpoTransactionContext
    {
        get;
        init;
    }
}

public static class DesignConversionThinClientStatus
{
    public const string ContractOnlyNoRuntime = "contract_only_no_runtime";
    public const string Submitted = "submitted";
    public const string Running = "running";
    public const string Completed = "completed";
    public const string Quarantined = "quarantined";
    public const string NotReady = "not_ready";
    public const string Failed = "failed";
    public const string Cancelled = "cancelled";

    public static string FromVisualQaDecision(string? visualDecision)
        => TryFromKnownVisualQaDecision(visualDecision, out var status)
            ? status
            : NotReady;

    public static bool TryFromKnownVisualQaDecision(string? visualDecision, out string status)
    {
        status = NotReady;
        if (string.IsNullOrWhiteSpace(visualDecision))
            return false;

        switch (visualDecision.Trim())
        {
            case DesignConversionVisualQaBoundaryPolicy.DecisionAccepted:
                status = Completed;
                return true;
            case DesignConversionVisualQaBoundaryPolicy.DecisionQuarantine:
                status = Quarantined;
                return true;
            case DesignConversionVisualQaBoundaryPolicy.DecisionRetry:
            case DesignConversionVisualQaBoundaryPolicy.DecisionBlocked:
                status = NotReady;
                return true;
            default:
                return false;
        }
    }
}

public sealed record DesignConversionThinClientJob(
    string JobId,
    string RequestId,
    string Status,
    string StatusUri,
    DateTimeOffset SubmittedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionThinClientArtifactRef(
    string ArtifactId,
    string Kind,
    string Uri,
    string Sha256,
    string MediaType,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionThinClientStatusQualityMetrics(
    bool Available,
    double? Score,
    double? RequiredThreshold,
    bool? Passed,
    bool? ThresholdMet,
    int DefectCount,
    string Source,
    string? ThresholdSource,
    string? UnavailableReason)
{
    public static DesignConversionThinClientStatusQualityMetrics Unavailable(
        string source,
        string unavailableReason)
        => new(
            Available: false,
            Score: null,
            RequiredThreshold: null,
            Passed: null,
            ThresholdMet: null,
            DefectCount: 0,
            Source: source,
            ThresholdSource: null,
            UnavailableReason: unavailableReason);
}

public sealed record DesignConversionThinClientStatusDefect(
    string DefectRef,
    string Category,
    string Severity,
    string Message,
    int Ordinal,
    IReadOnlyList<string> EvidenceRefs);

public sealed record DesignConversionThinClientStatusResult(
    DesignConversionThinClientJob Job,
    string ContractStatus,
    string Message,
    IReadOnlyList<DesignConversionThinClientArtifactRef> Artifacts,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> Provenance)
{
    public DesignConversionThinClientStatusQualityMetrics QualityMetrics { get; init; } =
        DesignConversionThinClientStatusQualityMetrics.Unavailable(
            source: "status_not_completed",
            unavailableReason: "quality_metrics_not_available_for_status");

    public IReadOnlyList<DesignConversionThinClientStatusDefect> Defects { get; init; } = [];
}

public sealed record RenderQaResolution(
    string ResolutionId,
    int Width,
    int Height,
    double DeviceScaleFactor);

public sealed record RenderQaImageRecord(
    string ImageId,
    string Label,
    RenderQaResolution Resolution,
    string Uri,
    string Sha256,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record VisualDefect(
    string DefectId,
    string Category,
    string Severity,
    string Description,
    double X,
    double Y,
    double Width,
    double Height,
    IReadOnlyList<string> EvidenceImageIds);

public sealed record VisualVerdict(
    string VerdictId,
    double Score,
    IReadOnlyList<VisualDefect> Defects,
    IReadOnlyList<string> EvidenceRefs,
    bool Passed);

public sealed record RenderQaResultRecord(
    string ResultId,
    string RequestId,
    RenderQaImageRecord Expected,
    RenderQaImageRecord Actual,
    VisualVerdict Verdict,
    IReadOnlyDictionary<string, double> Metrics,
    IReadOnlyList<string> Warnings);

public sealed record DesignConversionImprovementCandidate(
    string CandidateId,
    string RequestId,
    string Html,
    string Css,
    IReadOnlyList<string> ChangeReasons,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionImprovementOutput(
    string OutputId,
    string CandidateId,
    string Html,
    string Css,
    VisualVerdict Verdict,
    IReadOnlyList<RenderQaResultRecord> QaResults,
    IReadOnlyList<string> EvidenceRefs,
    bool Accepted);

public sealed record DesignConversionDpoProvenance(
    string SourceRequestId,
    string SourceKind,
    string SourceManifestRef,
    string ReviewerRef,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionLicenseScope(
    string LicenseId,
    string RightsScope,
    bool AllowsTraining,
    bool ContainsUserPrivateData,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionQuarantineState(
    bool IsQuarantined,
    string Reason,
    IReadOnlyList<string> EvidenceRefs);

/// <summary>
/// Process-private teacher payload.  This type is deliberately not a public
/// contract: durable access is granted only by the custody store after the
/// matching authority record has committed.
/// </summary>
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
internal sealed class DesignConversionDpoPayloadCustodySecret
{
    private readonly string _prompt;
    private readonly string _chosenResponse;
    private readonly string _rejectedResponse;

    internal DesignConversionDpoPayloadCustodySecret(
        string version,
        string prompt,
        string chosenResponse,
        string rejectedResponse,
        string promptSha256,
        string chosenResponseSha256,
        string rejectedResponseSha256,
        string governanceSha256,
        string trainingRecordBindingSha256)
    {
        Version = version;
        _prompt = prompt;
        _chosenResponse = chosenResponse;
        _rejectedResponse = rejectedResponse;
        PromptSha256 = promptSha256;
        ChosenResponseSha256 = chosenResponseSha256;
        RejectedResponseSha256 = rejectedResponseSha256;
        GovernanceSha256 = governanceSha256;
        TrainingRecordBindingSha256 = trainingRecordBindingSha256;
    }

    internal string Version { get; }
    [System.Text.Json.Serialization.JsonIgnore]
    internal string Prompt => _prompt;
    [System.Text.Json.Serialization.JsonIgnore]
    internal string ChosenResponse => _chosenResponse;
    [System.Text.Json.Serialization.JsonIgnore]
    internal string RejectedResponse => _rejectedResponse;
    internal string PromptSha256 { get; }
    internal string ChosenResponseSha256 { get; }
    internal string RejectedResponseSha256 { get; }
    internal string GovernanceSha256 { get; }
    internal string TrainingRecordBindingSha256 { get; }

    internal DesignConversionDpoPayloadCustodySecret Rebind(
        string governanceSha256,
        string trainingRecordBindingSha256)
        => new(
            Version,
            _prompt,
            _chosenResponse,
            _rejectedResponse,
            PromptSha256,
            ChosenResponseSha256,
            RejectedResponseSha256,
            governanceSha256,
            trainingRecordBindingSha256);

    private string DebuggerDisplay =>
        $"PayloadCustodySecret(binding={TrainingRecordBindingSha256}, raw=[REDACTED])";

    public override string ToString() => DebuggerDisplay;
}

public sealed record DesignConversionDpoTrainingRecordSummary(
    string RecordId,
    string UnitId,
    string Status,
    string PayloadCustodyStatus,
    string PromptSha256,
    string ChosenResponseSha256,
    string RejectedResponseSha256,
    string TrainingRecordBindingSha256);

public sealed record DesignConversionLearningTrainingItemSummary(
    string ItemId,
    string PromptSha256,
    string ChosenResponseSha256,
    string RejectedResponseSha256);

public sealed record DesignConversionDpoTrainingRecord(
    string RecordId,
    string UnitId,
    string InternalUnitStatePayload,
    IReadOnlyList<double> InternalVectorPayload,
    string SourceStructurePayload,
    DesignConversionImprovementOutput Chosen,
    DesignConversionImprovementOutput Rejected,
    string? TieBreakerPayload,
    double UsefulnessScore,
    string NegativeQualityReason,
    string NegativeReasonTaxonomy,
    DesignConversionDpoProvenance Provenance,
    DesignConversionLicenseScope License,
    DesignConversionQuarantineState Quarantine,
    TrainingTeacherProvenance? TeacherProvenance = null)
{
    // Авторство сторон хранится init-полями: старые positional-конструкторы
    // остаются совместимыми, а training bridge получает паспорт.
    public DpoPairSideAuthorKind? ChosenAuthorKind { get; init; }

    // Сторона rejected проверяется отдельно: направление пары нельзя вывести
    // из score без потери юридического смысла источника.
    public DpoPairSideAuthorKind? RejectedAuthorKind { get; init; }

    [System.Text.Json.Serialization.JsonIgnore]
    internal DesignConversionDpoPayloadCustodySecret? PayloadCustodySecret { get; init; }
}
