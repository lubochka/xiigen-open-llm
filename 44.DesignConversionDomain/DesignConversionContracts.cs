// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

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
    IReadOnlyDictionary<string, string> Metadata);

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
    IReadOnlyDictionary<string, string> Provenance);

public static class DesignConversionThinClientStatus
{
    public const string ContractOnlyNoRuntime = "contract_only_no_runtime";
    public const string Submitted = "submitted";
    public const string Running = "running";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Cancelled = "cancelled";
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

public sealed record DesignConversionThinClientStatusResult(
    DesignConversionThinClientJob Job,
    string ContractStatus,
    string Message,
    IReadOnlyList<DesignConversionThinClientArtifactRef> Artifacts,
    IReadOnlyList<string> Warnings,
    IReadOnlyDictionary<string, string> Provenance);

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
    DesignConversionQuarantineState Quarantine);
