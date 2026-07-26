// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

public interface IDesignConversionThinClientJobEvaluator
{
    Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class DesignConversionInternalServiceThinClientJobEvaluator(
    DesignConversionInternalService service) : IDesignConversionThinClientJobEvaluator
{
    private readonly DesignConversionInternalService _service =
        service ?? throw new ArgumentNullException(nameof(service));

    public Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        CancellationToken cancellationToken = default)
        => _service.EvaluateAsync(request, cancellationToken);
}

public sealed class DesignConversionImprovementLoopThinClientJobEvaluator : IDesignConversionThinClientJobEvaluator
{
    public const string SubmitPathLoopExecutedMarker = "DESIGN_CONVERSION_SUBMIT_PATH_IMPROVEMENT_LOOP_EXECUTED";
    public const string SubmitPathLoopFinalEvaluationMarker = "DESIGN_CONVERSION_SUBMIT_PATH_IMPROVEMENT_LOOP_FINAL_EVALUATION_RETURNED";
    public const string SubmitPathLoopMaxAttemptsMarker = "DESIGN_CONVERSION_SUBMIT_PATH_IMPROVEMENT_LOOP_MAX_ATTEMPTS_FROM_BUDGET";
    public const string MaxAttemptsExceededErrorCode = "IMPROVEMENT_LOOP_MAX_ATTEMPTS_EXCEEDED";

    private readonly IDesignConversionThinClientJobEvaluator _inner;
    private readonly IDesignConversionImprovementCandidateStrategy _candidateStrategy;

    public DesignConversionImprovementLoopThinClientJobEvaluator(
        IDesignConversionThinClientJobEvaluator inner,
        IDesignConversionImprovementCandidateStrategy? candidateStrategy = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _candidateStrategy = candidateStrategy ?? new DesignConversionRuntimeImprovementCandidateStrategy();
    }

    public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
        DesignConversionInternalEvaluateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion internal evaluation request is required.",
                "REQUEST_REQUIRED");
        }

        if (request.Budget is not null &&
            (request.Budget.MaxImprovementCandidates < 0 ||
             (request.Budget.MaxImprovementCandidates == 0 &&
              !DesignConversionVisualQaBoundaryPolicy.IsServerOwnedEntitlementTier(request.Budget.Tier))))
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion improvement candidate budget must be greater than zero.",
                DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);
        }

        if (request.Budget is not null &&
            request.Budget.MaxImprovementCandidates > DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts)
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts}.",
                MaxAttemptsExceededErrorCode);
        }

        var maxAttempts = request.Budget is null
            ? 1
            : DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierMaxImprovementCandidates(
                request.Budget.MaxImprovementCandidates,
                request.Budget.Tier);
        if (maxAttempts <= 1)
            return await _inner.EvaluateAsync(request, cancellationToken);

        var coordinator = new DesignConversionImprovementLoopCoordinator(
            _inner,
            _candidateStrategy);
        var loop = await coordinator.RunAsync(
            new DesignConversionImprovementLoopRequest(
                LoopId: $"{request.JobId}:submit-path-loop",
                InitialEvaluationRequest: request,
                MaxAttempts: maxAttempts,
                Metadata: new Dictionary<string, string>
                {
                    ["submit_path_improvement_loop"] = bool.TrueString,
                    ["max_attempts_source"] = "DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierMaxImprovementCandidates",
                    ["product_ready"] = bool.FalseString
                }),
            cancellationToken);

        if (!loop.IsSuccess)
            return loop.MapError<DesignConversionInternalEvaluationResult>();

        var result = loop.Value!;
        if (result.FinalEvaluation is null)
        {
            return OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                "DesignConversion improvement loop finished without a final evaluation.",
                "IMPROVEMENT_LOOP_FINAL_EVALUATION_MISSING");
        }

        return OperationResult<DesignConversionInternalEvaluationResult>.Ok(
            AddLoopBoundaryEvidence(result.FinalEvaluation, result, request, maxAttempts));
    }

    private static DesignConversionInternalEvaluationResult AddLoopBoundaryEvidence(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionImprovementLoopResult loop,
        DesignConversionInternalEvaluateRequest rootRequest,
        int maxAttempts)
    {
        var finalEvaluationJobId = evaluation.JobId;
        var finalEvaluationRequestId = evaluation.SubmittedRequest.RequestId;
        var finalEvaluationJobRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-submit-path-final-job",
            finalEvaluationJobId);
        var finalEvaluationRequestRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-submit-path-final-request",
            finalEvaluationRequestId);
        var replacements = FinalLoopIdentityReplacements.Create(
            rootRequest.JobId,
            rootRequest.RequestId,
            finalEvaluationJobId,
            finalEvaluationRequestId,
            evaluation);
        var sanitizedEvaluation = RewriteFinalLoopIdentityReferences(evaluation, replacements);
        var sanitizedInheritedEvidenceRefs = sanitizedEvaluation.EvidenceRefs
            .Concat(loop.EvidenceRefs)
            .Concat(loop.Attempts.SelectMany(attempt => attempt.EvidenceRefs))
            .Select(replacements.Rewrite);
        var loopVisualCostAccounting = BuildLoopVisualCostAccounting(loop.Attempts);

        return sanitizedEvaluation with
        {
            JobId = rootRequest.JobId,
            SubmittedRequest = sanitizedEvaluation.SubmittedRequest with { RequestId = rootRequest.RequestId },
            VisualCostAccounting = loopVisualCostAccounting,
            EvidenceRefs = sanitizedInheritedEvidenceRefs
                .Append(SubmitPathLoopExecutedMarker)
                .Append(SubmitPathLoopFinalEvaluationMarker)
                .Append(SubmitPathLoopMaxAttemptsMarker)
                .Append($"submit_path_improvement_loop_final_evaluation_job_ref={finalEvaluationJobRef}")
                .Append($"submit_path_improvement_loop_final_evaluation_request_ref={finalEvaluationRequestRef}")
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray(),
            Warnings = evaluation.Warnings
                .Concat(loop.Warnings)
                .Concat([
                    "submit_path_improvement_loop=true",
                    $"submit_path_improvement_loop_attempt_count={loop.AttemptCount}",
                    $"submit_path_improvement_loop_stop_reason={loop.StopReason}",
                    $"submit_path_improvement_loop_quality_reached={loop.QualityReached}",
                    $"submit_path_improvement_loop_completed={loop.Completed}",
                    $"submit_path_improvement_loop_max_attempts={maxAttempts}",
                    "product_ready=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .OrderBy(warning => warning, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static DesignConversionVisualCostAccounting BuildLoopVisualCostAccounting(
        IReadOnlyList<DesignConversionImprovementLoopAttemptTelemetry> attempts)
    {
        var sources = attempts
            .Select(attempt => attempt.VisualCostSource)
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Select(source => source.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var costSource = sources.Length switch
        {
            0 => DesignConversionVisualCostAccounting.UnknownCostSource,
            1 => sources[0],
            _ => DesignConversionVisualCostAccounting.MixedCostSource
        };

        return new DesignConversionVisualCostAccounting(
            TotalCostUsd: attempts.Select(attempt => attempt.EstimatedCostUsd).Sum(),
            MeasurementComplete: attempts.Count > 0 &&
                attempts.All(attempt => attempt.VisualCostMeasurementComplete),
            MissingMeasurementCount: attempts.Select(attempt => attempt.VisualCostMissingMeasurementCount).Sum(),
            EvaluationAttemptCount: attempts.Count,
            CostSource: costSource);
    }

    private static DesignConversionInternalEvaluationResult RewriteFinalLoopIdentityReferences(
        DesignConversionInternalEvaluationResult evaluation,
        FinalLoopIdentityReplacements r)
        => evaluation with
        {
            SubmittedRequest = Rewrite(evaluation.SubmittedRequest, r),
            RenderResults = evaluation.RenderResults.Select(value => Rewrite(value, r)).ToArray(),
            ComparisonArtifacts = evaluation.ComparisonArtifacts.Select(value => Rewrite(value, r)).ToArray(),
            VisualJudgeResults = evaluation.VisualJudgeResults.Select(value => Rewrite(value, r)).ToArray(),
            QaResults = evaluation.QaResults.Select(value => Rewrite(value, r)).ToArray(),
            RuntimeFlowResult = Rewrite(evaluation.RuntimeFlowResult, r),
            Verdict = Rewrite(evaluation.Verdict, r),
            EvidenceRefs = Rewrite(evaluation.EvidenceRefs, r),
            ScaffoldMarkers = RewriteMetadataValues(evaluation.ScaffoldMarkers, r),
            VisualQaBoundaryTrace = Rewrite(evaluation.VisualQaBoundaryTrace, r),
            ViewportFailures = evaluation.ViewportFailures.Select(failure => failure with
            {
                ViewportId = r.Rewrite(failure.ViewportId),
                EvidenceRefs = Rewrite(failure.EvidenceRefs, r)
            }).ToArray()
        };

    private static DesignConversionRuntimeFlowResult Rewrite(DesignConversionRuntimeFlowResult result, FinalLoopIdentityReplacements r)
        => result with { Request = Rewrite(result.Request, r), CriticOutput = result.CriticOutput with { OutputId = r.Rewrite(result.CriticOutput.OutputId), Verdict = Rewrite(result.CriticOutput.Verdict, r), QaEvidenceRefs = Rewrite(result.CriticOutput.QaEvidenceRefs, r), AiTeacherEvidenceRefs = Rewrite(result.CriticOutput.AiTeacherEvidenceRefs, r) }, ImproverOutput = result.ImproverOutput with { OutputId = r.Rewrite(result.ImproverOutput.OutputId), Candidate = Rewrite(result.ImproverOutput.Candidate, r), Output = Rewrite(result.ImproverOutput.Output, r), QaEvidenceRefs = Rewrite(result.ImproverOutput.QaEvidenceRefs, r), AiTeacherEvidenceRefs = Rewrite(result.ImproverOutput.AiTeacherEvidenceRefs, r) }, Candidate = Rewrite(result.Candidate, r), Output = Rewrite(result.Output, r), EvidenceRefs = Rewrite(result.EvidenceRefs, r) };

    private static SubmittedDesignConversionRequest Rewrite(SubmittedDesignConversionRequest request, FinalLoopIdentityReplacements r)
        => request with { DesignTreePayload = Rewrite(request.DesignTreePayload, r), OriginalTreePayload = Rewrite(request.OriginalTreePayload, r), Screenshots = request.Screenshots.Select(artifact => Rewrite(artifact, r)).ToArray(), DesignRenders = request.DesignRenders.Select(artifact => Rewrite(artifact, r)).ToArray(), Provenance = RewriteMetadataValues(request.Provenance, r) };

    private static DesignTreePayload Rewrite(DesignTreePayload payload, FinalLoopIdentityReplacements r)
        => payload with { PayloadId = r.Rewrite(payload.PayloadId), Sha256 = r.Rewrite(payload.Sha256), Metadata = RewriteMetadataValues(payload.Metadata, r) };

    private static DesignRenderArtifact Rewrite(DesignRenderArtifact artifact, FinalLoopIdentityReplacements r)
        => artifact with { ArtifactId = r.Rewrite(artifact.ArtifactId), Uri = r.Rewrite(artifact.Uri), Sha256 = r.Rewrite(artifact.Sha256), Metadata = RewriteMetadataValues(artifact.Metadata, r) };

    private static DesignConversionRenderResult Rewrite(DesignConversionRenderResult result, FinalLoopIdentityReplacements r)
        => result with { RenderResultId = r.Rewrite(result.RenderResultId), RenderRequestId = r.Rewrite(result.RenderRequestId), Image = Rewrite(result.Image, r), BrowserRenderProofReceipt = result.BrowserRenderProofReceipt is null ? null : result.BrowserRenderProofReceipt with { RenderResultId = r.Rewrite(result.BrowserRenderProofReceipt.RenderResultId), RenderRequestId = r.Rewrite(result.BrowserRenderProofReceipt.RenderRequestId), RequestId = r.Rewrite(result.BrowserRenderProofReceipt.RequestId), ImageId = r.Rewrite(result.BrowserRenderProofReceipt.ImageId), ScreenshotUri = r.Rewrite(result.BrowserRenderProofReceipt.ScreenshotUri), NormalizedSha256 = r.Rewrite(result.BrowserRenderProofReceipt.NormalizedSha256), ResolutionId = r.Rewrite(result.BrowserRenderProofReceipt.ResolutionId) } };

    private static DesignConversionQaComparisonArtifact Rewrite(DesignConversionQaComparisonArtifact artifact, FinalLoopIdentityReplacements r)
        => artifact with { ArtifactId = r.Rewrite(artifact.ArtifactId), ComparisonRequestId = r.Rewrite(artifact.ComparisonRequestId), Expected = Rewrite(artifact.Expected, r), Actual = Rewrite(artifact.Actual, r), Verdict = Rewrite(artifact.Verdict, r), DiffImageUri = r.Rewrite(artifact.DiffImageUri), DiffImageSha256 = r.Rewrite(artifact.DiffImageSha256), Metrics = r.RewriteExternalTrustMetrics(artifact.Metrics), EvidenceRefs = Rewrite(artifact.EvidenceRefs, r), Metadata = RewriteMetadataValues(artifact.Metadata, r) };

    private static DesignConversionVisualJudgeResult Rewrite(DesignConversionVisualJudgeResult result, FinalLoopIdentityReplacements r)
        => result with { JudgeResultId = r.Rewrite(result.JudgeResultId), JudgeRequestId = r.Rewrite(result.JudgeRequestId), Verdict = Rewrite(result.Verdict, r), Provider = result.Provider with { EndpointHost = r.Rewrite(result.Provider.EndpointHost), ModelId = r.Rewrite(result.Provider.ModelId), Metadata = RewriteMetadataValues(result.Provider.Metadata, r) }, EvidenceRefs = Rewrite(result.EvidenceRefs, r), Metadata = RewriteMetadataValues(result.Metadata, r) };

    private static DesignConversionImprovementCandidate Rewrite(DesignConversionImprovementCandidate candidate, FinalLoopIdentityReplacements r)
        => candidate with { CandidateId = r.Rewrite(candidate.CandidateId), RequestId = r.Rewrite(candidate.RequestId), EvidenceRefs = Rewrite(candidate.EvidenceRefs, r), Metadata = RewriteMetadataValues(candidate.Metadata, r) };

    private static DesignConversionImprovementOutput Rewrite(DesignConversionImprovementOutput output, FinalLoopIdentityReplacements r)
        => output with { OutputId = r.Rewrite(output.OutputId), CandidateId = r.Rewrite(output.CandidateId), Verdict = Rewrite(output.Verdict, r), QaResults = output.QaResults.Select(result => Rewrite(result, r)).ToArray(), EvidenceRefs = Rewrite(output.EvidenceRefs, r) };

    private static RenderQaResultRecord Rewrite(RenderQaResultRecord result, FinalLoopIdentityReplacements r)
        => result with { ResultId = r.Rewrite(result.ResultId), RequestId = r.Rewrite(result.RequestId), Expected = Rewrite(result.Expected, r), Actual = Rewrite(result.Actual, r), Verdict = Rewrite(result.Verdict, r) };

    private static RenderQaImageRecord Rewrite(RenderQaImageRecord image, FinalLoopIdentityReplacements r)
        => image with { ImageId = r.Rewrite(image.ImageId), Resolution = image.Resolution with { ResolutionId = r.Rewrite(image.Resolution.ResolutionId) }, Uri = r.Rewrite(image.Uri), Sha256 = r.Rewrite(image.Sha256), Metadata = RewriteMetadataValues(image.Metadata, r) };

    private static VisualVerdict Rewrite(VisualVerdict verdict, FinalLoopIdentityReplacements r)
        => verdict with { VerdictId = r.Rewrite(verdict.VerdictId), Defects = verdict.Defects.Select(defect => defect with { DefectId = r.Rewrite(defect.DefectId), EvidenceImageIds = Rewrite(defect.EvidenceImageIds, r) }).ToArray(), EvidenceRefs = Rewrite(verdict.EvidenceRefs, r) };

    private static DesignConversionVisualQaBoundaryTrace Rewrite(DesignConversionVisualQaBoundaryTrace trace, FinalLoopIdentityReplacements r)
        => trace with { MultiResolutionRender = trace.MultiResolutionRender with { RequiredViewportIds = Rewrite(trace.MultiResolutionRender.RequiredViewportIds, r), ObservedViewportIds = Rewrite(trace.MultiResolutionRender.ObservedViewportIds, r), MissingViewportIds = Rewrite(trace.MultiResolutionRender.MissingViewportIds, r), EvidenceRefs = Rewrite(trace.MultiResolutionRender.EvidenceRefs, r) }, TierPolicy = trace.TierPolicy with { EvidenceRefs = Rewrite(trace.TierPolicy.EvidenceRefs, r), Metadata = RewriteMetadataValues(trace.TierPolicy.Metadata, r) }, VisualDecision = trace.VisualDecision with { CorrectionFindings = trace.VisualDecision.CorrectionFindings.Select(finding => finding with { FindingId = r.Rewrite(finding.FindingId), EvidenceImageIds = Rewrite(finding.EvidenceImageIds, r) }).ToArray(), EvidenceRefs = Rewrite(trace.VisualDecision.EvidenceRefs, r), Metadata = RewriteMetadataValues(trace.VisualDecision.Metadata, r) }, EvidenceRefs = Rewrite(trace.EvidenceRefs, r) };

    private static IReadOnlyList<string> Rewrite(IEnumerable<string> values, FinalLoopIdentityReplacements r)
        => values.Select(r.RewriteEvidenceReference).ToArray();

    private static IReadOnlyDictionary<string, string> RewriteMetadataValues(IReadOnlyDictionary<string, string> metadata, FinalLoopIdentityReplacements r)
    {
        var rewritten = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata)
            rewritten[pair.Key] = r.RewriteMetadataValue(pair.Key, pair.Value);

        return rewritten;
    }

    private sealed class FinalLoopIdentityReplacements
    {
        private static readonly HashSet<string> UntrustedExternalBooleanMetadataKeys = new(StringComparer.Ordinal)
        {
            "network_call_executed",
            "external_network_call_executed",
            "external_provider_called",
            "real_external_provider_executed",
            "external_visual_ai_executed",
            "external_ai_visual_judge_executed",
            "visual_ai_executed",
            "visual_ai_provider_contract_executed",
            "provider_response_identity_verified",
            "provider_response_schema_verified",
            "provider_response_verified",
            "visual_fidelity_verified",
            "visual_fidelity_provider_receipt_verified",
            "visual_fidelity_response_identity_verified",
            "visual_fidelity_response_schema_verified",
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey,
            ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey
        };

        private static readonly HashSet<string> UntrustedExternalMetricKeys = new(StringComparer.Ordinal)
        {
            "network_call_executed",
            "real_external_provider_executed",
            "external_visual_ai_executed",
            "external_ai_visual_judge_executed"
        };

        private readonly (string Raw, string Replacement)[] _items;

        private FinalLoopIdentityReplacements(
            IEnumerable<(string Raw, string Replacement)> items,
            bool downgradeExternalReceipts)
        {
            DowngradeExternalReceipts = downgradeExternalReceipts;
            _items = items
                .OrderByDescending(item => item.Raw.Length)
                .ThenBy(item => item.Raw, StringComparer.Ordinal)
                .ToArray();
        }

        private bool DowngradeExternalReceipts { get; }

        public static FinalLoopIdentityReplacements Create(
            string rootJobId,
            string rootRequestId,
            string finalJobId,
            string finalRequestId,
            DesignConversionInternalEvaluationResult evaluation)
        {
            var externalArtifacts = evaluation.ComparisonArtifacts
                .Where(IsExternalComparisonReceipt)
                .ToArray();
            var externalJudgeResults = evaluation.VisualJudgeResults
                .Where(IsExternalJudgeReceipt)
                .ToArray();
            var hasExternalReceipts = externalArtifacts.Length > 0 || externalJudgeResults.Length > 0;
            var externalReceiptsValid = HasValidOriginalExternalReceipts(
                externalArtifacts,
                externalJudgeResults,
                finalJobId,
                finalRequestId);
            var downgradeExternalReceipts = hasExternalReceipts && !externalReceiptsValid;
            var items = new List<(string Raw, string Replacement)>();
            Add(
                items,
                finalJobId,
                downgradeExternalReceipts
                    ? DesignConversionDpoLeakageGuard.HashRef("dcm-untrusted-final-loop-job", finalJobId)
                    : rootJobId);
            Add(
                items,
                finalRequestId,
                downgradeExternalReceipts
                    ? DesignConversionDpoLeakageGuard.HashRef("dcm-untrusted-final-loop-request", finalRequestId)
                    : rootRequestId);

            if (hasExternalReceipts && externalReceiptsValid)
            {
                var rootReplacements = new FinalLoopIdentityReplacements(
                    items,
                    downgradeExternalReceipts: false);
                AddCanonicalComparisonResultReplacements(items, rootReplacements, externalArtifacts);
                AddCanonicalVisualJudgeResultReplacements(items, rootReplacements, externalJudgeResults);
            }

            return new FinalLoopIdentityReplacements(items, downgradeExternalReceipts);
        }

        private static void AddCanonicalComparisonResultReplacements(
            List<(string Raw, string Replacement)> items,
            FinalLoopIdentityReplacements rootReplacements,
            IEnumerable<DesignConversionQaComparisonArtifact> artifacts)
        {
            foreach (var artifact in artifacts)
            {
                var providerRequestId = ReadMetadata(
                    artifact.Metadata,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey);
                var comparisonRequestId = artifact.ComparisonRequestId;
                var comparisonResultId = ReadMetadata(
                    artifact.Metadata,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey);
                var comparisonArtifactId = ReadMetadata(
                    artifact.Metadata,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey);
                var normalizedProviderRequestId = rootReplacements.Rewrite(providerRequestId);
                var normalizedComparisonRequestId = rootReplacements.Rewrite(comparisonRequestId);
                var normalizedComparisonResultId =
                    ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
                        normalizedProviderRequestId,
                        normalizedComparisonRequestId);
                var normalizedComparisonArtifactId =
                    ExternalHttpDesignConversionVisualJudge.BuildCanonicalComparisonArtifactId(
                        normalizedProviderRequestId,
                        normalizedComparisonRequestId);

                Add(items, comparisonResultId, normalizedComparisonResultId);
                Add(items, comparisonArtifactId, normalizedComparisonArtifactId);

                var originalDefaultDiffUri = BuildDefaultComparisonDiffUri(comparisonResultId);
                if (string.Equals(artifact.DiffImageUri, originalDefaultDiffUri, StringComparison.Ordinal))
                {
                    Add(
                        items,
                        originalDefaultDiffUri,
                        BuildDefaultComparisonDiffUri(normalizedComparisonResultId));
                }
            }
        }

        private static void AddCanonicalVisualJudgeResultReplacements(
            List<(string Raw, string Replacement)> items,
            FinalLoopIdentityReplacements rootReplacements,
            IEnumerable<DesignConversionVisualJudgeResult> results)
        {
            foreach (var result in results)
            {
                var resultComparisonRequestId = ReadMetadata(
                    result.Metadata,
                    ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey);
                var normalizedJudgeRequestId = rootReplacements.Rewrite(result.JudgeRequestId);
                var normalizedComparisonRequestId = rootReplacements.Rewrite(resultComparisonRequestId);
                var normalizedJudgeResultId =
                    ExternalHttpDesignConversionVisualJudge.BuildCanonicalVisualResultId(
                        normalizedJudgeRequestId,
                        normalizedComparisonRequestId);
                Add(items, result.JudgeResultId, normalizedJudgeResultId);
            }
        }

        private static bool HasValidOriginalExternalReceipts(
            IReadOnlyList<DesignConversionQaComparisonArtifact> artifacts,
            IReadOnlyList<DesignConversionVisualJudgeResult> judgeResults,
            string finalJobId,
            string finalRequestId)
            => DesignConversionExternalVisualAiReceiptValidator.HasValidOriginalExternalReceipts(
                artifacts,
                judgeResults,
                finalJobId,
                finalRequestId);

        private static bool IsExternalComparisonReceipt(DesignConversionQaComparisonArtifact artifact)
            => IsTruthy(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey) ||
               IsTruthy(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey) ||
               IsTruthy(artifact.Metadata, "network_call_executed") ||
               IsTruthy(artifact.Metadata, "external_visual_ai_executed");

        private static bool IsExternalJudgeReceipt(DesignConversionVisualJudgeResult result)
            => result.Provider.ExternalCallExecuted ||
               IsTruthy(result.Provider.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey) ||
               IsTruthy(result.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey);

        private static bool IsTruthy(IReadOnlyDictionary<string, string> metadata, string key)
            => metadata.TryGetValue(key, out var value) &&
               (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "1", StringComparison.Ordinal) ||
                string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

        private static bool IsSafeIdentity(string? value)
            => !string.IsNullOrWhiteSpace(value) &&
               !string.Equals(value, "none", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(value, "missing", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(value, "unconfigured", StringComparison.OrdinalIgnoreCase);

        public string RewriteMetadataValue(string key, string value)
        {
            var rewritten = Rewrite(value);
            return DowngradeExternalReceipts && UntrustedExternalBooleanMetadataKeys.Contains(key)
                ? bool.FalseString
                : rewritten;
        }

        public string RewriteEvidenceReference(string value)
        {
            var rewritten = Rewrite(value);
            if (!DowngradeExternalReceipts)
                return rewritten;

            if (string.Equals(rewritten, "network_call_executed:true", StringComparison.Ordinal))
                return "network_call_executed:false";
            if (string.Equals(rewritten, "real_external_provider_executed:true", StringComparison.Ordinal))
                return "real_external_provider_executed:false";
            if (string.Equals(rewritten, "external_visual_ai_executed:true", StringComparison.Ordinal))
                return "external_visual_ai_executed:false";
            if (string.Equals(rewritten, "external_ai_visual_judge_executed:true", StringComparison.Ordinal) ||
                string.Equals(rewritten, "visual_ai_executed_true", StringComparison.Ordinal))
            {
                return DesignConversionVisualQaBoundaryPolicy.VisualAiExecutedFalseMarker;
            }

            return string.Equals(
                rewritten,
                $"{ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey}:true",
                StringComparison.Ordinal)
                ? $"{ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey}:false"
                : rewritten;
        }

        public IReadOnlyDictionary<string, double> RewriteExternalTrustMetrics(
            IReadOnlyDictionary<string, double> metrics)
        {
            if (!DowngradeExternalReceipts)
                return metrics;

            var rewritten = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var pair in metrics)
            {
                rewritten[pair.Key] = UntrustedExternalMetricKeys.Contains(pair.Key)
                    ? 0.0
                    : pair.Value;
            }

            return rewritten;
        }

        private static string ReadMetadata(
            IReadOnlyDictionary<string, string> metadata,
            string key)
            => metadata.TryGetValue(key, out var value) ? value : string.Empty;

        private static string BuildDefaultComparisonDiffUri(string comparisonResultId)
            => $"artifact://design-conversion/visual-judge/{Uri.EscapeDataString(comparisonResultId)}";

        public string Rewrite(string value)
        {
            var input = value ?? string.Empty;
            if (input.Length == 0 || _items.Length == 0)
                return input;

            var rewritten = new StringBuilder(input.Length);
            for (var index = 0; index < input.Length;)
            {
                var matched = false;
                foreach (var item in _items)
                {
                    if (!input.AsSpan(index).StartsWith(item.Raw, StringComparison.Ordinal))
                        continue;

                    rewritten.Append(item.Replacement);
                    index += item.Raw.Length;
                    matched = true;
                    break;
                }

                if (!matched)
                    rewritten.Append(input[index++]);
            }

            return rewritten.ToString();
        }

        private static void Add(List<(string Raw, string Replacement)> items, string raw, string root)
        {
            if (string.IsNullOrWhiteSpace(raw) || string.Equals(raw, root, StringComparison.Ordinal))
                return;

            if (items.Any(item => string.Equals(item.Raw, raw, StringComparison.Ordinal)))
                return;

            items.Add((raw, root));
        }
    }
}

public sealed class DesignConversionThinClientJobRuntime
{
    public const string RuntimeBoundary = "in_process_thin_client_job_runtime_only";
    public const string StorageBoundary = "in_memory_only";
    public const string DeterministicExecutionBoundary = "deterministic_internal_service_path_only";
    public const string Flow47VerificationAuthority = "host_boundary";
    public const string Flow47VerificationState = "not_evaluated_in_thin_runtime";
    public const string VisualCostRecordsReadFailedErrorCode = "DESIGN_CONVERSION_THIN_CLIENT_VISUAL_COST_RECORDS_READ_FAILED";
    public const string VisualCostRecordsReadFailedSafeMessage =
        "DesignConversion visual-cost records could not be read for the requested job.";
    public const string VisualCostRecordsCancelledSafeMessage =
        "DesignConversion visual-cost record readback cancelled.";
    public const string BrowserRuntimeBlockerSafeMessage =
        "DesignConversion browser runtime blocker at thin-client boundary.";
    public const string BrowserScreenshotFailureSafeMessage =
        "DesignConversion browser screenshot failed at thin-client boundary.";
    public const string BrowserRuntimeBlockerProvenanceKey = "browser_runtime_blocker";
    public const string BrowserRuntimeBlockerErrorCodeProvenanceKey = "browser_runtime_blocker_error_code";
    public const string TrustedVisualCostBudgetStoreRequiredErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_STORE_REQUIRED";
    public const string TrustedVisualCostBudgetContractInvalidErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_CONTRACT_INVALID";
    public const string TrustedVisualCostBudgetReservationDeniedErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_RESERVATION_DENIED";
    public const string TrustedVisualCostBudgetReservationFailedErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_RESERVATION_FAILED";
    public const string TrustedVisualCostBudgetSettlementFailedErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_BUDGET_SETTLEMENT_FAILED";
    public const string TrustedVisualCostAccountingIncompleteErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_ACCOUNTING_INCOMPLETE";
    public const string TrustedVisualCostActualExceedsReservationErrorCode =
        "DESIGN_CONVERSION_TRUSTED_VISUAL_COST_ACTUAL_EXCEEDS_RESERVATION";

    private const string TrustedEntitlementBoundKey = "trusted_visual_cost_entitlement_bound";
    private const string TrustedEntitlementAuthorityKey = "trusted_visual_cost_entitlement_authority";
    private const string TrustedUserRefKey = "trusted_visual_cost_user_ref";
    private const string TrustedTenantIdRefKey = "trusted_visual_cost_tenant_id_ref";
    private const string TrustedUserIdentityAuthorityKey = "trusted_visual_cost_user_identity_authority";
    private const string ServerAssignedDesignTierKey = "server_assigned_design_tier";
    private const string ServerAssignedMonthlyLimitUsdKey = "server_assigned_monthly_visual_cost_limit_usd";
    private const string DesignTierAuthorityKey = "design_tier_authority";
    private const string MonthlyVisualCostLimitAuthorityKey = "monthly_visual_cost_limit_authority";
    private const string UserIdentityAuthorityKey = "user_identity_authority";
    private const string TrustedPeriodKindKey = "trusted_visual_cost_period_kind";
    private const string TrustedReservationStoreWiredKey = "trusted_visual_cost_reservation_store_wired";
    private const string PlatformTenantConfigurationAuthority = "platform_tenant_configuration";
    private const string VerifiedInboundPrincipalAuthority = "verified_inbound_principal";

    private static readonly IReadOnlyList<string> BoundaryWarnings =
    [
        "broad_unfiltered_tests_project_verified=false",
        "public_api_verified=false",
        "flow47_verification_authority=" + Flow47VerificationAuthority,
        "flow47_verification_state=" + Flow47VerificationState,
        "platform_integration_verified=false",
        "persistence_verified=false",
        "product_ready_verified=false",
        "runtime_ready_verified=false",
        "trained_feature_verified=false",
        "model_training_verified=false",
        "browser_e2e_verified=false",
        "visual_fidelity_verified=false",
        "deployment_verified=false",
        "host_readiness_verified=false"
    ];

    private readonly object _gate = new();
    private readonly IDesignConversionThinClientJobEvaluator _evaluator;
    private readonly IDesignConversionLocalArtifactDb? _artifactDb;
    private readonly IDesignConversionVisualCostLedger _visualCostLedger;
    private readonly IDesignConversionVisualCostBudgetStore? _visualCostBudgetStore;
    private readonly Func<DateTimeOffset> _utcNow;
    private readonly Dictionary<string, SubmittedDesignConversionRequest> _requests = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DesignConversionThinClientJob> _jobs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DesignConversionThinClientStatusResult> _statuses = new(StringComparer.Ordinal);

    public DesignConversionThinClientJobRuntime(
        DesignConversionInternalService service,
        Func<DateTimeOffset>? utcNow = null,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null)
        : this(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            utcNow,
            artifactDb,
            visualCostLedger,
            visualCostBudgetStore)
    {
    }

    public DesignConversionThinClientJobRuntime(
        IDesignConversionThinClientJobEvaluator evaluator,
        Func<DateTimeOffset>? utcNow = null,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        IDesignConversionVisualCostLedger? visualCostLedger = null,
        IDesignConversionVisualCostBudgetStore? visualCostBudgetStore = null)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
        _artifactDb = artifactDb;
        _visualCostLedger = visualCostLedger ?? JsonlDesignConversionVisualCostLedger.CreateDefault();
        _visualCostBudgetStore = visualCostBudgetStore;
    }

    public IDesignConversionLocalArtifactDb? LocalArtifactDb => _artifactDb;

    public IDesignConversionVisualCostLedger VisualCostLedger => _visualCostLedger;

    public IDesignConversionVisualCostBudgetStore? VisualCostBudgetStore => _visualCostBudgetStore;

    public async Task<OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>> GetVisualCostRecordsAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>.Fail(
                "DesignConversion thin-client job id is required.",
                "JOB_ID_REQUIRED");

        if (cancellationToken.IsCancellationRequested)
            return VisualCostRecordsCancelled<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

        var evaluationJobRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef(jobId);
        OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>> records;
        try
        {
            records = await _visualCostLedger.ListRecordsAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return VisualCostRecordsCancelled<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }
        catch (Exception)
        {
            return VisualCostRecordsReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }

        if (!records.IsSuccess)
        {
            if (string.Equals(records.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                return VisualCostRecordsCancelled<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();

            return VisualCostRecordsReadFailed<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>();
        }

        // Unknown job ids intentionally return an empty set, avoiding a job-existence oracle.
        var matches = (records.Value ?? [])
            .Where(record => string.Equals(record.EvaluationJobRef, evaluationJobRef, StringComparison.Ordinal))
            .OrderBy(record => record.RecordedAtUtc)
            .ThenBy(record => record.RecordId, StringComparer.Ordinal)
            .ToArray();

        return OperationResult<IReadOnlyList<DesignConversionVisualCostLedgerRecord>>.Ok(matches);
    }

    public OperationResult<DesignConversionThinClientJob> Submit(SubmittedDesignConversionRequest request)
    {
        var validation = ValidateSubmission(request);
        if (!validation.IsSuccess)
            return validation.MapError<DesignConversionThinClientJob>();

        var now = _utcNow();
        var jobId = BuildJobId(request.RequestId);
        var job = new DesignConversionThinClientJob(
            JobId: jobId,
            RequestId: request.RequestId,
            Status: DesignConversionThinClientStatus.Submitted,
            StatusUri: BuildStatusUri(jobId),
            SubmittedAtUtc: now,
            UpdatedAtUtc: null,
            Metadata: BuildJobMetadata(request, status: DesignConversionThinClientStatus.Submitted, errorCode: null));
        var status = BuildStatusResult(
            job,
            DesignConversionThinClientStatus.Submitted,
            "In-process thin-client job submitted; explicit RunAsync is required before internal execution.",
            artifacts: [],
            extraWarnings: [],
            request.Provenance);

        lock (_gate)
        {
            if (_jobs.ContainsKey(jobId))
                return OperationResult<DesignConversionThinClientJob>.Fail(
                    $"DesignConversion thin-client job already exists for request '{request.RequestId}'.",
                    "JOB_ALREADY_EXISTS");

            _requests[jobId] = request;
            _jobs[jobId] = job;
            _statuses[jobId] = status;
        }

        return OperationResult<DesignConversionThinClientJob>.Ok(job);
    }

    public OperationResult<DesignConversionThinClientStatusResult> GetStatus(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                "DesignConversion thin-client job id is required.",
                "JOB_ID_REQUIRED");

        lock (_gate)
        {
            if (_statuses.TryGetValue(jobId, out var status))
                return OperationResult<DesignConversionThinClientStatusResult>.Ok(status);
        }

        return OperationResult<DesignConversionThinClientStatusResult>.Fail(
            $"DesignConversion thin-client job '{jobId}' was not found.",
            "JOB_NOT_FOUND");
    }

    public async Task<OperationResult<DesignConversionThinClientStatusResult>> RunAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                "DesignConversion thin-client job id is required.",
                "JOB_ID_REQUIRED");

        SubmittedDesignConversionRequest request;
        DesignConversionThinClientJob runningJob;
        lock (_gate)
        {
            if (!_jobs.TryGetValue(jobId, out var currentJob) || !_requests.TryGetValue(jobId, out var storedRequest))
            {
                return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                    $"DesignConversion thin-client job '{jobId}' was not found.",
                    "JOB_NOT_FOUND");
            }

            request = storedRequest;

            if (string.Equals(currentJob.Status, DesignConversionThinClientStatus.Running, StringComparison.Ordinal))
            {
                return OperationResult<DesignConversionThinClientStatusResult>.Fail(
                    $"DesignConversion thin-client job '{jobId}' is already running.",
                    "JOB_ALREADY_RUNNING");
            }

            if (string.Equals(currentJob.Status, DesignConversionThinClientStatus.Completed, StringComparison.Ordinal) ||
                string.Equals(currentJob.Status, DesignConversionThinClientStatus.Failed, StringComparison.Ordinal) ||
                string.Equals(currentJob.Status, DesignConversionThinClientStatus.Cancelled, StringComparison.Ordinal))
            {
                return OperationResult<DesignConversionThinClientStatusResult>.Ok(_statuses[jobId]);
            }

            runningJob = UpdateJob(currentJob, DesignConversionThinClientStatus.Running, errorCode: null);
            _jobs[jobId] = runningJob;
            _statuses[jobId] = BuildStatusResult(
                runningJob,
                DesignConversionThinClientStatus.Running,
                "In-process thin-client job is running through the injected internal evaluator.",
                artifacts: [],
                extraWarnings: [],
                request.Provenance);
        }

        var internalRequest = BuildInternalEvaluateRequest(request, runningJob);
        OperationResult<DesignConversionInternalEvaluationResult> evaluation;
        VisualCostBudgetReservationContext? visualCostReservation = null;
        OperationResult<VisualCostBudgetSettlementContext>? visualCostSettlement = null;
        var evaluatorInvoked = false;
        if (!internalRequest.IsSuccess)
        {
            evaluation = internalRequest.MapError<DesignConversionInternalEvaluationResult>();
        }
        else
        {
            var reservation = await TryReserveVisualCostBudgetAsync(request, runningJob, cancellationToken);
            if (!reservation.IsSuccess)
            {
                evaluation = reservation.MapError<DesignConversionInternalEvaluationResult>();
            }
            else
            {
                visualCostReservation = reservation.Value;
                try
                {
                    evaluatorInvoked = true;
                    evaluation = await _evaluator.EvaluateAsync(internalRequest.Value!, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    evaluation = OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                        "DesignConversion thin-client job run cancelled.",
                        "CANCELLED");
                }
                catch (Exception ex)
                {
                    evaluation = OperationResult<DesignConversionInternalEvaluationResult>.Fail(
                        ex.Message,
                        "THIN_CLIENT_JOB_RUNTIME_EXCEPTION");
                }
            }
        }

        if (visualCostReservation is not null && evaluatorInvoked)
            visualCostSettlement = await SettleVisualCostBudgetAsync(visualCostReservation, evaluation);

        DesignConversionThinClientStatusResult terminalStatus;
        if (evaluation.IsSuccess)
        {
            if (visualCostSettlement is not null && !visualCostSettlement.IsSuccess)
            {
                terminalStatus = BuildFailedStatus(
                    runningJob,
                    request,
                    visualCostSettlement.ErrorMessage,
                    visualCostSettlement.ErrorCode);
            }
            else if (visualCostSettlement?.Value?.FailJob == true)
            {
                terminalStatus = BuildFailedStatus(
                    runningJob,
                    request,
                    visualCostSettlement.Value.FailureMessage,
                    visualCostSettlement.Value.FailureCode);
            }
            else if (TryGetKnownBrowserFailureFromEvaluation(evaluation.Value!, out var browserFailureCode))
            {
                terminalStatus = BuildFailedStatus(
                    runningJob,
                    request,
                    string.Empty,
                    browserFailureCode);
            }
            else
            {
                var completedStatus = BuildCompletedStatus(runningJob, request, evaluation.Value!);
                terminalStatus = await PersistCompletedEvaluationAsync(
                    runningJob,
                    request,
                    completedStatus,
                    evaluation.Value!,
                    cancellationToken);
            }
        }
        else
        {
            terminalStatus = visualCostSettlement is not null && !visualCostSettlement.IsSuccess
                ? BuildFailedStatus(runningJob, request, visualCostSettlement.ErrorMessage, visualCostSettlement.ErrorCode)
                : BuildFailedStatus(runningJob, request, evaluation.ErrorMessage, evaluation.ErrorCode);
        }

        terminalStatus = MarkVisualCostBudgetReservation(terminalStatus, visualCostReservation, visualCostSettlement?.Value);

        lock (_gate)
        {
            _jobs[jobId] = terminalStatus.Job;
            _statuses[jobId] = terminalStatus;
        }

        return OperationResult<DesignConversionThinClientStatusResult>.Ok(terminalStatus);
    }

    private async Task<DesignConversionThinClientStatusResult> PersistCompletedEvaluationAsync(
        DesignConversionThinClientJob runningJob,
        SubmittedDesignConversionRequest request,
        DesignConversionThinClientStatusResult completedStatus,
        DesignConversionInternalEvaluationResult evaluation,
        CancellationToken cancellationToken)
    {
        var createdAtUtc = _utcNow();
        var visualCostRecord = BuildVisualCostLedgerRecord(evaluation, request.Budget, createdAtUtc);
        OperationResult<DesignConversionVisualCostLedgerRecord> visualCostAppend;
        try
        {
            visualCostAppend = await _visualCostLedger.AppendRecordAsync(visualCostRecord, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            visualCostAppend = OperationResult<DesignConversionVisualCostLedgerRecord>.Fail(
                ex.Message,
                "VISUAL_COST_LEDGER_PERSISTENCE_EXCEPTION");
        }

        if (!visualCostAppend.IsSuccess)
        {
            if (string.Equals(visualCostAppend.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                throw new OperationCanceledException(cancellationToken);

            return MarkVisualCostLedgerPersistenceFailed(
                BuildFailedStatus(
                    runningJob,
                    request,
                    "DesignConversion thin-client job failed after evaluation because visual-cost accounting could not be durably recorded.",
                    SafePersistenceErrorCode(visualCostAppend.ErrorCode)));
        }

        completedStatus = MarkVisualCostLedgerPersistence(completedStatus, visualCostAppend.Value!);

        if (_artifactDb is null)
            return completedStatus;

        var dpoBuild = new DesignConversionDpoGenerator()
            .BuildRuntimeCreatedTrainingCandidates(evaluation, createdAtUtc);
        var dpoCandidates = dpoBuild.IsSuccess
            ? dpoBuild.Value!.AcceptedRecords
            : [];
        var record = DesignConversionLocalArtifactDbRecord.FromEvaluation(
            evaluation,
            dpoCandidates: dpoCandidates,
            createdAtUtc: createdAtUtc,
            metadata: BuildLocalArtifactDbMetadata(
                evaluation,
                dpoCandidates.Count,
                dpoBuild.IsSuccess ? dpoBuild.Value!.Warnings : [dpoBuild.ErrorCode ?? "runtime_created_dpo_candidate_build_failed"]));
        OperationResult<DesignConversionLocalArtifactDbRecord> append;
        try
        {
            append = await _artifactDb.AppendRecordAsync(record, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            append = OperationResult<DesignConversionLocalArtifactDbRecord>.Fail(
                ex.Message,
                "LOCAL_ARTIFACT_DB_PERSISTENCE_EXCEPTION");
        }

        if (!append.IsSuccess)
        {
            if (string.Equals(append.ErrorCode, "CANCELLED", StringComparison.Ordinal))
                throw new OperationCanceledException(cancellationToken);

            var errorCode = string.IsNullOrWhiteSpace(append.ErrorCode)
                ? "LOCAL_ARTIFACT_DB_PERSISTENCE_FAILED"
                : append.ErrorCode;
            return MarkLocalArtifactDbPersistenceFailed(completedStatus, errorCode);
        }

        return MarkLocalArtifactDbPersistence(completedStatus, append.Value!);
    }

    private static DesignConversionVisualCostLedgerRecord BuildVisualCostLedgerRecord(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionBudgetMetadata budget,
        DateTimeOffset createdAtUtc)
    {
        var recordedAtUtc = createdAtUtc.ToUniversalTime();
        var accounting = evaluation.VisualCostAccounting ?? DesignConversionVisualCostAccounting.Unavailable;
        var userRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef(budget.UserId);
        if (!userRef.IsSuccess)
            throw new InvalidOperationException("DesignConversion visual-cost ledger user ref source is required.");
        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(recordedAtUtc);
        var evaluationJobRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalEvaluationJobRef(
            evaluation.JobId);
        var submittedRequestRef = JsonlDesignConversionVisualCostLedger.BuildCanonicalSubmittedRequestRef(
            evaluation.SubmittedRequest.RequestId);
        var recordId = JsonlDesignConversionVisualCostLedger.BuildRecordId(
            evaluationJobRef,
            submittedRequestRef,
            accounting);
        return new DesignConversionVisualCostLedgerRecord(
            RecordId: recordId,
            LedgerKind: JsonlDesignConversionVisualCostLedger.LedgerKind,
            SchemaVersion: JsonlDesignConversionVisualCostLedger.SchemaVersion,
            EvaluationJobRef: evaluationJobRef,
            SubmittedRequestRef: submittedRequestRef,
            TotalCostUsd: accounting.TotalCostUsd,
            MeasurementComplete: accounting.MeasurementComplete,
            MissingMeasurementCount: accounting.MissingMeasurementCount,
            EvaluationAttemptCount: accounting.EvaluationAttemptCount,
            CostSource: string.IsNullOrWhiteSpace(accounting.CostSource)
                ? DesignConversionVisualCostAccounting.UnknownCostSource
                : accounting.CostSource,
            RecordedAtUtc: recordedAtUtc,
            EvidenceRefs: [],
            Metadata: new Dictionary<string, string>()) with
            {
                UserRef = userRef.Value!,
                SubmittedTier = JsonlDesignConversionVisualCostLedger.NormalizeSubmittedTier(budget.Tier),
                PeriodKind = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
                PeriodStartUtc = period.StartUtc,
                PeriodEndExclusiveUtc = period.EndExclusiveUtc
            };
    }

    private async Task<OperationResult<VisualCostBudgetReservationContext?>> TryReserveVisualCostBudgetAsync(
        SubmittedDesignConversionRequest request,
        DesignConversionThinClientJob job,
        CancellationToken cancellationToken)
    {
        if (!IsTruthy(request.Provenance, TrustedEntitlementBoundKey))
            return OperationResult<VisualCostBudgetReservationContext?>.Ok(null);

        if (_visualCostBudgetStore is null)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget store is required before evaluation.",
                TrustedVisualCostBudgetStoreRequiredErrorCode);
        }

        var budgetRequest = BuildVisualCostBudgetReservationRequest(request, job, _utcNow());
        if (!budgetRequest.IsSuccess)
            return budgetRequest.MapError<VisualCostBudgetReservationContext?>();

        OperationResult<DesignConversionVisualCostReservationResult> reservation;
        try
        {
            reservation = await _visualCostBudgetStore.TryReserveAsync(budgetRequest.Value!.Request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget reservation cancelled.",
                "CANCELLED");
        }
        catch (Exception)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget reservation failed closed.",
                TrustedVisualCostBudgetReservationFailedErrorCode);
        }

        if (!reservation.IsSuccess)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget reservation failed closed.",
                SafeVisualCostBudgetErrorCode(reservation.ErrorCode, TrustedVisualCostBudgetReservationFailedErrorCode));
        }

        if (!reservation.Value!.Reserved)
        {
            return OperationResult<VisualCostBudgetReservationContext?>.Fail(
                "DesignConversion trusted visual-cost budget reservation was denied.",
                string.IsNullOrWhiteSpace(reservation.Value.DenialReasonCode) ||
                string.Equals(reservation.Value.DenialReasonCode, JsonlDesignConversionVisualCostBudgetStore.NoDenialReason, StringComparison.Ordinal)
                    ? TrustedVisualCostBudgetReservationDeniedErrorCode
                    : reservation.Value.DenialReasonCode);
        }

        return OperationResult<VisualCostBudgetReservationContext?>.Ok(
            budgetRequest.Value with
            {
                Reservation = reservation.Value
            });
    }

    private static OperationResult<VisualCostBudgetReservationContext> BuildVisualCostBudgetReservationRequest(
        SubmittedDesignConversionRequest request,
        DesignConversionThinClientJob job,
        DateTimeOffset requestedAtUtc)
    {
        var metadata = request.Budget.Metadata ?? new Dictionary<string, string>();
        var provenance = request.Provenance ?? new Dictionary<string, string>();

        if (!IsTruthy(provenance, TrustedEntitlementBoundKey) ||
            !IsTruthy(provenance, TrustedReservationStoreWiredKey) ||
            !IsTruthy(metadata, TrustedEntitlementBoundKey) ||
            !IsTruthy(metadata, TrustedReservationStoreWiredKey))
        {
            return InvalidTrustedVisualCostContract();
        }

        if (!HasExact(metadata, TrustedEntitlementAuthorityKey, PlatformTenantConfigurationAuthority) ||
            !HasExact(metadata, DesignTierAuthorityKey, PlatformTenantConfigurationAuthority) ||
            !HasExact(metadata, MonthlyVisualCostLimitAuthorityKey, PlatformTenantConfigurationAuthority) ||
            !HasExact(metadata, TrustedUserIdentityAuthorityKey, VerifiedInboundPrincipalAuthority) ||
            !HasExact(metadata, UserIdentityAuthorityKey, VerifiedInboundPrincipalAuthority) ||
            !HasExact(metadata, TrustedPeriodKindKey, JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind))
        {
            return InvalidTrustedVisualCostContract();
        }

        if (!TryReadRequired(metadata, TrustedUserRefKey, out var trustedUserRef) ||
            !TryReadRequired(metadata, TrustedTenantIdRefKey, out var trustedTenantIdRef) ||
            !TryReadRequired(metadata, ServerAssignedDesignTierKey, out var submittedTier) ||
            !TryReadRequired(metadata, ServerAssignedMonthlyLimitUsdKey, out var monthlyLimitText) ||
            !TryParseCanonicalUsd(monthlyLimitText, out var monthlyLimitUsd))
        {
            return InvalidTrustedVisualCostContract();
        }

        if (!string.Equals(request.Budget.Tier, submittedTier, StringComparison.Ordinal) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_user_ref", trustedUserRef) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_tenant_id_ref", trustedTenantIdRef) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_design_tier", submittedTier) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_monthly_limit_usd", monthlyLimitText) ||
            !HasExact(provenance, "trusted_visual_cost_entitlement_period_kind", JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind))
        {
            return InvalidTrustedVisualCostContract();
        }

        var requestedReserveUsd = DesignConversionVisualQaBoundaryPolicy.ResolveEffectiveTierBudgetUsd(
            request.Budget.BudgetUsd,
            submittedTier);
        if (requestedReserveUsd <= 0m)
            return InvalidTrustedVisualCostContract();

        var compositeIdentity = string.Join("|", "tenant", trustedTenantIdRef, "user", trustedUserRef);
        var storeUserRef = JsonlDesignConversionVisualCostLedger.BuildPseudonymousUserRef(compositeIdentity);
        if (!storeUserRef.IsSuccess)
            return InvalidTrustedVisualCostContract();

        var period = JsonlDesignConversionVisualCostLedger.BuildUtcCalendarMonthPeriod(requestedAtUtc.ToUniversalTime());
        var reservationRef = JsonlDesignConversionVisualCostBudgetStore.BuildCanonicalReservationRef(string.Join(
            "|",
            job.JobId,
            request.RequestId,
            storeUserRef.Value!,
            trustedTenantIdRef,
            trustedUserRef,
            submittedTier,
            period.StartUtc.ToString("O", CultureInfo.InvariantCulture),
            requestedReserveUsd.ToString("0.####", CultureInfo.InvariantCulture),
            monthlyLimitText));
        var reservationRequest = new DesignConversionVisualCostReservationRequest(
            ReservationRef: reservationRef,
            UserRef: storeUserRef.Value!,
            SubmittedTier: submittedTier,
            PeriodStartUtc: period.StartUtc,
            PeriodEndExclusiveUtc: period.EndExclusiveUtc,
            MonthlyLimitUsd: monthlyLimitUsd,
            RequestedReserveUsd: requestedReserveUsd,
            RequestedAtUtc: requestedAtUtc.ToUniversalTime());

        return OperationResult<VisualCostBudgetReservationContext>.Ok(new VisualCostBudgetReservationContext(
            Request: reservationRequest,
            Reservation: null));
    }

    private async Task<OperationResult<VisualCostBudgetSettlementContext>> SettleVisualCostBudgetAsync(
        VisualCostBudgetReservationContext reservation,
        OperationResult<DesignConversionInternalEvaluationResult> evaluation)
    {
        if (_visualCostBudgetStore is null)
        {
            return OperationResult<VisualCostBudgetSettlementContext>.Fail(
                "DesignConversion trusted visual-cost budget store is required for settlement.",
                TrustedVisualCostBudgetSettlementFailedErrorCode);
        }

        var accounting = evaluation.IsSuccess
            ? evaluation.Value!.VisualCostAccounting ?? DesignConversionVisualCostAccounting.Unavailable
            : DesignConversionVisualCostAccounting.Unavailable;
        var actualCostKnown =
            evaluation.IsSuccess &&
            accounting.MeasurementComplete &&
            accounting.MissingMeasurementCount == 0 &&
            accounting.EvaluationAttemptCount > 0 &&
            accounting.TotalCostUsd >= 0m &&
            accounting.TotalCostUsd <= reservation.Request.RequestedReserveUsd;
        var chargedCostUsd = actualCostKnown
            ? accounting.TotalCostUsd
            : reservation.Request.RequestedReserveUsd;
        var failJob = evaluation.IsSuccess && !actualCostKnown;
        var failureCode = accounting.TotalCostUsd > reservation.Request.RequestedReserveUsd
            ? TrustedVisualCostActualExceedsReservationErrorCode
            : TrustedVisualCostAccountingIncompleteErrorCode;
        var failureMessage = accounting.TotalCostUsd > reservation.Request.RequestedReserveUsd
            ? "DesignConversion trusted visual-cost accounting exceeded the reserved amount."
            : "DesignConversion trusted visual-cost accounting was incomplete and failed closed.";

        OperationResult<DesignConversionVisualCostSettlementResult> settlement;
        try
        {
            settlement = await _visualCostBudgetStore.SettleAsync(
                new DesignConversionVisualCostSettlementRequest(
                    ReservationRef: reservation.Request.ReservationRef,
                    UserRef: reservation.Request.UserRef,
                    PeriodStartUtc: reservation.Request.PeriodStartUtc,
                    PeriodEndExclusiveUtc: reservation.Request.PeriodEndExclusiveUtc,
                    ActualCostUsd: chargedCostUsd,
                    SettledAtUtc: _utcNow().ToUniversalTime()),
                CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            return OperationResult<VisualCostBudgetSettlementContext>.Fail(
                "DesignConversion trusted visual-cost budget settlement cancelled.",
                "CANCELLED");
        }
        catch (Exception)
        {
            return OperationResult<VisualCostBudgetSettlementContext>.Fail(
                "DesignConversion trusted visual-cost budget settlement failed closed.",
                TrustedVisualCostBudgetSettlementFailedErrorCode);
        }

        if (!settlement.IsSuccess)
        {
            return OperationResult<VisualCostBudgetSettlementContext>.Fail(
                "DesignConversion trusted visual-cost budget settlement failed closed.",
                SafeVisualCostBudgetErrorCode(settlement.ErrorCode, TrustedVisualCostBudgetSettlementFailedErrorCode));
        }

        return OperationResult<VisualCostBudgetSettlementContext>.Ok(new VisualCostBudgetSettlementContext(
            Settlement: settlement.Value!,
            ChargedCostUsd: chargedCostUsd,
            ActualCostKnown: actualCostKnown,
            FailJob: failJob,
            FailureCode: failureCode,
            FailureMessage: failureMessage));
    }

    private static DesignConversionThinClientStatusResult MarkVisualCostBudgetReservation(
        DesignConversionThinClientStatusResult status,
        VisualCostBudgetReservationContext? reservation,
        VisualCostBudgetSettlementContext? settlement)
    {
        if (reservation is null)
            return status;

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [TrustedReservationStoreWiredKey] = bool.TrueString,
            ["trusted_visual_cost_budget_reserved"] = bool.TrueString,
            ["trusted_visual_cost_budget_reservation_ref"] = reservation.Request.ReservationRef,
            ["trusted_visual_cost_budget_user_ref"] = reservation.Request.UserRef,
            ["trusted_visual_cost_budget_submitted_tier"] = reservation.Request.SubmittedTier,
            ["trusted_visual_cost_budget_period_kind"] = JsonlDesignConversionVisualCostLedger.UtcCalendarMonthPeriodKind,
            ["trusted_visual_cost_budget_period_start_utc"] =
                reservation.Request.PeriodStartUtc.ToString("O", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_period_end_exclusive_utc"] =
                reservation.Request.PeriodEndExclusiveUtc.ToString("O", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_monthly_limit_usd"] =
                reservation.Request.MonthlyLimitUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_requested_reserve_usd"] =
                reservation.Request.RequestedReserveUsd.ToString("0.####", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_reservation_status"] =
                reservation.Reservation?.Status ?? JsonlDesignConversionVisualCostBudgetStore.StatusReserved,
            ["trusted_visual_cost_budget_reservation_replayed"] =
                (reservation.Reservation?.Replayed ?? false).ToString(CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_settled"] = (settlement is not null).ToString(CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_settlement_status"] = settlement?.Settlement.Status ?? "not-settled",
            ["trusted_visual_cost_budget_settlement_replayed"] =
                (settlement?.Settlement.Replayed ?? false).ToString(CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_charged_cost_usd"] =
                (settlement?.ChargedCostUsd ?? 0m).ToString("0.####", CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_actual_cost_known"] =
                (settlement?.ActualCostKnown ?? false).ToString(CultureInfo.InvariantCulture),
            ["trusted_visual_cost_budget_release_used"] = bool.FalseString,
            ["trusted_visual_cost_budget_enforcement"] = "reserve_before_evaluator_settle_after_invocation"
        };

        return status with
        {
            Job = status.Job with
            {
                Metadata = MergeDictionaries(status.Job.Metadata, metadata)
            },
            Warnings = status.Warnings
                .Concat([
                    "trusted_visual_cost_reservation_store_wired=true",
                    "trusted_visual_cost_budget_reserved=true",
                    settlement is null
                        ? "trusted_visual_cost_budget_settled=false"
                        : "trusted_visual_cost_budget_settled=true",
                    settlement?.ActualCostKnown == true
                        ? "trusted_visual_cost_budget_actual_cost_known=true"
                        : "trusted_visual_cost_budget_actual_cost_known=false",
                    "trusted_visual_cost_budget_release_used=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(status.Provenance, metadata)
        };
    }

    private static OperationResult<VisualCostBudgetReservationContext> InvalidTrustedVisualCostContract()
        => OperationResult<VisualCostBudgetReservationContext>.Fail(
            "DesignConversion trusted visual-cost budget contract is invalid.",
            TrustedVisualCostBudgetContractInvalidErrorCode);

    private static bool TryReadRequired(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        out string value)
    {
        value = string.Empty;
        if (!metadata.TryGetValue(key, out var raw) ||
            string.IsNullOrWhiteSpace(raw) ||
            !string.Equals(raw, raw.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        value = raw;
        return true;
    }

    private static bool HasExact(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string expected)
        => metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);

    private static bool IsTruthy(IReadOnlyDictionary<string, string>? metadata, string key)
        => metadata is not null &&
           metadata.TryGetValue(key, out var value) &&
           bool.TryParse(value, out var parsed) &&
           parsed;

    private static bool TryParseCanonicalUsd(string value, out decimal amount)
    {
        amount = 0m;
        if (string.IsNullOrWhiteSpace(value) ||
            !string.Equals(value, value.Trim(), StringComparison.Ordinal) ||
            value.Contains("e", StringComparison.OrdinalIgnoreCase) ||
            value.Contains('+') ||
            value.Contains('-'))
        {
            return false;
        }

        if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount))
            return false;

        return amount > 0m &&
               string.Equals(amount.ToString("0.####", CultureInfo.InvariantCulture), value, StringComparison.Ordinal);
    }

    private static string SafeVisualCostBudgetErrorCode(string? errorCode, string fallback)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return fallback;

        var safe = new string(errorCode
            .Where(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-')
            .ToArray());
        return string.IsNullOrWhiteSpace(safe) ? fallback : safe;
    }

    private static OperationResult<bool> ValidateSubmission(SubmittedDesignConversionRequest request)
    {
        if (request is null)
            return OperationResult<bool>.Fail("DesignConversion thin-client submission is required.", "REQUEST_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return OperationResult<bool>.Fail("DesignConversion request id is required.", "REQUEST_ID_REQUIRED");
        if (request.DesignTreePayload is null || request.OriginalTreePayload is null)
            return OperationResult<bool>.Fail("DesignConversion tree payloads are required.", "TREE_PAYLOADS_REQUIRED");
        if (request.Budget is null)
            return OperationResult<bool>.Fail("DesignConversion budget metadata is required.", "BUDGET_REQUIRED");
        if (string.IsNullOrWhiteSpace(request.Budget.UserId))
            return OperationResult<bool>.Fail("DesignConversion budget user id is required.", "USER_ID_REQUIRED");
        if (request.Budget.MaxImprovementCandidates < 0 ||
            (request.Budget.MaxImprovementCandidates == 0 &&
             !DesignConversionVisualQaBoundaryPolicy.IsServerOwnedEntitlementTier(request.Budget.Tier)))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion improvement candidate budget must be greater than zero.",
                DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);
        }
        if (request.Budget.MaxImprovementCandidates > DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts)
        {
            return OperationResult<bool>.Fail(
                $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts}.",
                DesignConversionImprovementLoopThinClientJobEvaluator.MaxAttemptsExceededErrorCode);
        }
        if (request.Budget.MaxRenderQaRuns <= 0)
            return OperationResult<bool>.Fail("DesignConversion render QA budget must be greater than zero.", "RENDER_QA_BUDGET_REQUIRED");
        if ((request.DesignRenders is null || request.DesignRenders.Count == 0) &&
            (request.Screenshots is null || request.Screenshots.Count == 0))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion expected design render or screenshot is required.",
                "EXPECTED_RENDER_REQUIRED");
        }

        var renderResolutions = BuildRenderResolutions(request);
        if (!renderResolutions.IsSuccess)
            return renderResolutions.MapError<bool>();

        if (renderResolutions.Value!.Count == 0)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion render artifacts must include positive width and height.",
                "RENDER_RESOLUTION_REQUIRED");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<DesignConversionInternalEvaluateRequest> BuildInternalEvaluateRequest(
        SubmittedDesignConversionRequest request,
        DesignConversionThinClientJob job)
    {
        var renderResolutions = BuildRenderResolutions(request);
        if (!renderResolutions.IsSuccess)
            return renderResolutions.MapError<DesignConversionInternalEvaluateRequest>();

        if (renderResolutions.Value!.Count == 0)
        {
            return OperationResult<DesignConversionInternalEvaluateRequest>.Fail(
                "DesignConversion render artifacts must include positive width and height.",
                "RENDER_RESOLUTION_REQUIRED");
        }

        return OperationResult<DesignConversionInternalEvaluateRequest>.Ok(new DesignConversionInternalEvaluateRequest(
            JobId: job.JobId,
            RequestId: request.RequestId,
            SourceKind: request.SourceKind,
            Html: request.Html,
            Css: request.Css,
            DesignTreePayload: request.DesignTreePayload,
            OriginalTreePayload: request.OriginalTreePayload,
            Screenshots: request.Screenshots?.ToArray() ?? [],
            DesignRenders: request.DesignRenders?.ToArray() ?? [],
            Budget: request.Budget,
            RenderResolutions: renderResolutions.Value,
            AiTeacherEvidenceRefs: ["thin-client-runtime:no-ai-teacher-executed"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["thin_client_job_runtime"] = RuntimeBoundary,
                ["status_store"] = StorageBoundary,
                ["execution_path"] = DeterministicExecutionBoundary,
                ["public_api_verified"] = "false",
                ["persistence_verified"] = "false",
                ["product_ready_verified"] = "false",
                ["runtime_ready_verified"] = "false",
                ["trained_feature_verified"] = "false"
            },
            Provenance: MergeDictionaries(
                request.Provenance,
                new Dictionary<string, string>
                {
                    ["thin_client_job_id"] = job.JobId,
                    ["thin_client_status_uri"] = job.StatusUri,
                    ["submission_mapper"] = nameof(DesignConversionThinClientJobRuntime),
                    ["runtime_boundary"] = RuntimeBoundary,
                    ["storage_boundary"] = StorageBoundary
                })));
    }

    private static OperationResult<IReadOnlyList<RenderQaResolution>> BuildRenderResolutions(
        SubmittedDesignConversionRequest request)
        => DesignConversionRenderResolutionFactory.CreateModelServerMinimumResponsiveMatrix(
            (request.DesignRenders ?? []).Concat(request.Screenshots ?? []));

    private DesignConversionThinClientStatusResult BuildCompletedStatus(
        DesignConversionThinClientJob runningJob,
        SubmittedDesignConversionRequest request,
        DesignConversionInternalEvaluationResult evaluation)
    {
        var completedJob = UpdateJob(runningJob, DesignConversionThinClientStatus.Completed, errorCode: null);
        var artifacts = new[]
        {
            BuildArtifact(completedJob, "improved-html", "text/html", evaluation.ImprovedHtml),
            BuildArtifact(completedJob, "improved-css", "text/css", evaluation.ImprovedCss),
            BuildArtifact(completedJob, "runtime-flow-output", "application/json", evaluation.RuntimeFlowResult.Output.OutputId)
        };
        var warnings = evaluation.Warnings
            .Concat(evaluation.DeterministicLogicClaimedAsLearning
                ? ["deterministic_logic_claimed_as_learning=true"]
                : ["deterministic_logic_claimed_as_learning=false"])
            .ToArray();

        return BuildStatusResult(
            completedJob,
            DesignConversionThinClientStatus.Completed,
            "In-process thin-client job completed through the injected internal evaluator.",
            artifacts,
            warnings,
            MergeDictionaries(
                request.Provenance,
                new Dictionary<string, string>
                {
                    ["internal_evaluation_job_id"] = evaluation.JobId,
                    ["runtime_flow_unit_order"] = string.Join(">", evaluation.RuntimeFlowResult.UnitExecutionOrder),
                    ["deterministic_logic_claimed_as_learning"] = evaluation.DeterministicLogicClaimedAsLearning ? "true" : "false"
                }));
    }

    private DesignConversionThinClientStatusResult BuildFailedStatus(
        DesignConversionThinClientJob runningJob,
        SubmittedDesignConversionRequest request,
        string message,
        string errorCode)
    {
        var status = string.Equals(errorCode, "CANCELLED", StringComparison.Ordinal)
            ? DesignConversionThinClientStatus.Cancelled
            : DesignConversionThinClientStatus.Failed;
        var failedJob = UpdateJob(runningJob, status, errorCode);
        var isBrowserRuntimeBlocker = IsBrowserRuntimeBlockerErrorCode(errorCode);
        var safeMessage = ResolveFailureSafeMessage(message, errorCode, isBrowserRuntimeBlocker);
        var extraWarnings = isBrowserRuntimeBlocker
            ? new[]
            {
                $"internal_evaluator_error_code={errorCode}",
                $"{BrowserRuntimeBlockerProvenanceKey}=true",
                $"{BrowserRuntimeBlockerErrorCodeProvenanceKey}={errorCode}"
            }
            : [$"internal_evaluator_error_code={errorCode}"];
        var failureProvenance = new Dictionary<string, string>
        {
            ["internal_evaluator_error_code"] = errorCode,
            ["runtime_boundary"] = RuntimeBoundary
        };
        if (isBrowserRuntimeBlocker)
        {
            failureProvenance[BrowserRuntimeBlockerProvenanceKey] = bool.TrueString;
            failureProvenance[BrowserRuntimeBlockerErrorCodeProvenanceKey] = errorCode;
        }

        return BuildStatusResult(
            failedJob,
            status,
            safeMessage,
            artifacts: [],
            extraWarnings,
            MergeDictionaries(request.Provenance, failureProvenance));
    }

    private static string BuildBrowserRuntimeBlockerSafeMessage(string errorCode)
        => $"{BrowserRuntimeBlockerSafeMessage} error_code={errorCode}.";

    private static string BuildBrowserScreenshotFailureSafeMessage(string errorCode)
        => $"{BrowserScreenshotFailureSafeMessage} error_code={errorCode}.";

    private static string ResolveFailureSafeMessage(
        string message,
        string errorCode,
        bool isBrowserRuntimeBlocker)
    {
        if (isBrowserRuntimeBlocker)
            return BuildBrowserRuntimeBlockerSafeMessage(errorCode);

        if (string.Equals(errorCode, LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode, StringComparison.Ordinal))
            return BuildBrowserScreenshotFailureSafeMessage(errorCode);

        return string.IsNullOrWhiteSpace(message)
            ? "DesignConversion thin-client job failed."
            : message;
    }

    private static bool IsBrowserRuntimeBlockerErrorCode(string errorCode)
        => string.Equals(errorCode, LocalChromiumBrowserProcessAdapter.RuntimeUnavailableCode, StringComparison.Ordinal) ||
           string.Equals(errorCode, LocalChromiumBrowserProcessAdapter.RuntimeCrashedBeforeScreenshotCode, StringComparison.Ordinal) ||
           string.Equals(errorCode, "BROWSER_SCREENSHOT_TIMEOUT", StringComparison.Ordinal);

    private static bool IsKnownBrowserFailureErrorCode(string errorCode)
        => IsBrowserRuntimeBlockerErrorCode(errorCode) ||
           string.Equals(errorCode, LocalChromiumBrowserProcessAdapter.ScreenshotFailedCode, StringComparison.Ordinal);

    private static bool TryGetKnownBrowserFailureFromEvaluation(
        DesignConversionInternalEvaluationResult evaluation,
        out string errorCode)
    {
        foreach (var failure in evaluation.ViewportFailures)
        {
            if (IsKnownBrowserFailureErrorCode(failure.ErrorCode))
            {
                errorCode = failure.ErrorCode;
                return true;
            }
        }

        errorCode = string.Empty;
        return false;
    }

    private static DesignConversionThinClientStatusResult MarkLocalArtifactDbPersistence(
        DesignConversionThinClientStatusResult completedStatus,
        DesignConversionLocalArtifactDbRecord record)
    {
        var metadata = BuildLocalArtifactDbStatusMetadata(record);
        return completedStatus with
        {
            Job = completedStatus.Job with
            {
                Metadata = MergeDictionaries(completedStatus.Job.Metadata, metadata)
            },
            Warnings = completedStatus.Warnings
                .Concat([
                    "local_artifact_db_persistence_verified=true",
                    $"runtime_created_dpo_candidate_count={record.DpoCandidates.Count}",
                    record.DpoCandidates.Count > 0
                        ? "runtime_created_dpo_candidate=true"
                        : "runtime_created_dpo_candidate=false",
                    record.DpoCandidates.Count > 0
                        ? "runtime_created_dpo_candidate_review_pending=true"
                        : "runtime_created_dpo_candidate_review_pending=false",
                    record.DpoCandidates.Count > 0
                        ? "review_required=true"
                        : "review_required=false",
                    record.DpoCandidates.Count > 0
                        ? "approval_status=pending"
                        : "approval_status=not-required",
                    "approved_for_training=false",
                    "training_candidate_only=true",
                    "production_dpo=false",
                    "product_not_ready=true",
                    "model_readiness_claimed=false",
                    "high_quality_negative_pair_not_proven=true",
                    "held_out_not_proven=true",
                    "source_disjoint_not_proven=true",
                    "production_db_ready=false",
                    "trained_model_ready=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(completedStatus.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkLocalArtifactDbPersistenceFailed(
        DesignConversionThinClientStatusResult completedStatus,
        string errorCode)
    {
        var safeErrorCode = SafePersistenceErrorCode(errorCode);
        var metadata = new Dictionary<string, string>
        {
            ["local_artifact_db_persistence_verified"] = bool.FalseString,
            ["local_artifact_db_persistence_failed"] = bool.TrueString,
            ["local_artifact_db_persistence_status"] = "non_durable",
            ["local_artifact_db_persistence_error_code"] = safeErrorCode,
            ["local_artifact_db_autowrite"] = bool.FalseString,
            ["n8_12_non_durable_result_preserved"] = bool.TrueString,
            ["storage_training_artifact_db"] = bool.FalseString,
            ["local_artifact_db"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString
        };

        return completedStatus with
        {
            Job = completedStatus.Job with
            {
                Metadata = MergeDictionaries(completedStatus.Job.Metadata, metadata)
            },
            Warnings = completedStatus.Warnings
                .Concat([
                    "local_artifact_db_persistence_verified=false",
                    "local_artifact_db_persistence_failed=true",
                    "local_artifact_db_persistence_status=non_durable",
                    $"local_artifact_db_persistence_error_code={safeErrorCode}",
                    "local_artifact_db_autowrite=false",
                    "n8_12_non_durable_result_preserved=true",
                    "production_db_ready=false",
                    "product_not_ready=true"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(completedStatus.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkVisualCostLedgerPersistence(
        DesignConversionThinClientStatusResult completedStatus,
        DesignConversionVisualCostLedgerRecord record)
    {
        var metadata = BuildVisualCostLedgerStatusMetadata(record);
        return completedStatus with
        {
            Job = completedStatus.Job with
            {
                Metadata = MergeDictionaries(completedStatus.Job.Metadata, metadata)
            },
            Warnings = completedStatus.Warnings
                .Concat([
                    "visual_cost_ledger_persistence_verified=true",
                    "visual_cost_ledger_recorded=true",
                    "visual_cost_ledger_training_artifact_db=false",
                    "visual_cost_ledger_quota_enforcement=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(completedStatus.Provenance, metadata)
        };
    }

    private static DesignConversionThinClientStatusResult MarkVisualCostLedgerPersistenceFailed(
        DesignConversionThinClientStatusResult failedStatus)
    {
        var metadata = new Dictionary<string, string>
        {
            ["visual_cost_ledger_persistence_verified"] = bool.FalseString,
            ["visual_cost_ledger_persistence_failed"] = bool.TrueString,
            ["visual_cost_ledger_persistence_status"] = "failed_closed",
            ["visual_cost_ledger_training_artifact_db"] = bool.FalseString,
            ["visual_cost_ledger_quota_enforcement"] = bool.FalseString,
            ["completed_without_visual_cost_accounting"] = bool.FalseString
        };

        return failedStatus with
        {
            Job = failedStatus.Job with
            {
                Metadata = MergeDictionaries(failedStatus.Job.Metadata, metadata)
            },
            Warnings = failedStatus.Warnings
                .Concat([
                    "visual_cost_ledger_persistence_verified=false",
                    "visual_cost_ledger_persistence_failed=true",
                    "visual_cost_ledger_persistence_status=failed_closed",
                    "completed_without_visual_cost_accounting=false"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance = MergeDictionaries(failedStatus.Provenance, metadata)
        };
    }

    private static IReadOnlyDictionary<string, string> BuildLocalArtifactDbMetadata(
        DesignConversionInternalEvaluationResult evaluation,
        int runtimeCreatedDpoCandidateCount,
        IReadOnlyList<string> dpoCandidateWarnings)
        => new Dictionary<string, string>
        {
            ["artifact_db_record_source"] = nameof(DesignConversionThinClientJobRuntime),
            ["artifact_db_single_writer_owner"] = nameof(DesignConversionThinClientJobRuntime),
            ["artifact_db_record_job_binding"] = "root_submit_job_final_evaluation_result",
            ["final_candidate_from_loop"] = IsSubmitPathLoopEvaluation(evaluation).ToString(),
            ["final_visual_score"] = evaluation.VisualQaBoundaryTrace.TierPolicy.AcceptanceScore
                .ToString("0.####", CultureInfo.InvariantCulture),
            ["attempt_count"] = ExtractWarningValue(
                evaluation.Warnings,
                "submit_path_improvement_loop_attempt_count=",
                "1"),
            ["final_loop_stop_reason"] = ExtractWarningValue(
                evaluation.Warnings,
                "submit_path_improvement_loop_stop_reason=",
                "not-run"),
            ["runtime_boundary"] = RuntimeBoundary,
            ["local_artifact_db_persistence_verified"] = bool.TrueString,
            ["storage_training_artifact_db"] = bool.TrueString,
            ["local_artifact_db"] = bool.TrueString,
            ["conversion_result"] = "refs_or_hashes_only",
            ["render_artifact_refs"] = "refs_or_hashes_only",
            ["visual_judge_status"] = evaluation.VisualQaBoundaryTrace.VisualJudgeStatus,
            ["visual_judge_provider_boundary"] = evaluation.VisualQaBoundaryTrace.VisualJudgeProviderBoundary,
            ["external_ai_visual_judge_executed"] = evaluation.VisualQaBoundaryTrace.ExternalAiVisualJudgeExecuted.ToString(),
            ["tier_policy_stop_reason"] = evaluation.VisualQaBoundaryTrace.TierPolicy.StopReason,
            ["training_candidate_only_boundary"] = runtimeCreatedDpoCandidateCount > 0
                ? "runtime_created_dpo_candidate_training_candidate_only_review_pending"
                : "runtime_created_dpo_candidate_not_created",
            ["runtime_created_dpo_candidate"] = (runtimeCreatedDpoCandidateCount > 0).ToString(),
            ["runtime_created_dpo_candidate_count"] = runtimeCreatedDpoCandidateCount.ToString(),
            ["runtime_created_dpo_candidate_warnings"] = string.Join("|", dpoCandidateWarnings ?? []),
            ["runtime_created_dpo_candidate_review_status"] = runtimeCreatedDpoCandidateCount > 0
                ? "runtime-candidate-review-pending"
                : "not-created",
            ["review_required"] = (runtimeCreatedDpoCandidateCount > 0).ToString(),
            ["approval_required"] = (runtimeCreatedDpoCandidateCount > 0).ToString(),
            ["approval_status"] = runtimeCreatedDpoCandidateCount > 0 ? "pending" : "not-required",
            ["approved_for_training"] = bool.FalseString,
            ["production_dpo"] = bool.FalseString,
            ["high_quality_negative_pair_not_proven"] = bool.TrueString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["public_api_verified"] = bool.FalseString,
            ["production_db_ready"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["trained_feature_verified"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_tree_payload_included"] = bool.FalseString,
            ["prompt_payload_included"] = bool.FalseString,
            ["external_provider_payload_included"] = bool.FalseString
        };

    private static bool IsSubmitPathLoopEvaluation(DesignConversionInternalEvaluationResult evaluation)
        => evaluation.EvidenceRefs.Contains(
            DesignConversionImprovementLoopThinClientJobEvaluator.SubmitPathLoopExecutedMarker);

    private static string ExtractWarningValue(
        IReadOnlyList<string> warnings,
        string prefix,
        string fallback)
    {
        var value = warnings.FirstOrDefault(warning => warning.StartsWith(prefix, StringComparison.Ordinal));
        return string.IsNullOrWhiteSpace(value) ? fallback : value[prefix.Length..];
    }

    private static string SafePersistenceErrorCode(string? errorCode)
    {
        var value = string.IsNullOrWhiteSpace(errorCode)
            ? "LOCAL_ARTIFACT_DB_PERSISTENCE_FAILED"
            : errorCode;
        var safe = new string(value
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-' ? char.ToLowerInvariant(ch) : '_')
            .ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "local_artifact_db_persistence_failed" : safe;
    }

    private static OperationResult<T> VisualCostRecordsReadFailed<T>()
        => OperationResult<T>.Fail(
            VisualCostRecordsReadFailedSafeMessage,
            VisualCostRecordsReadFailedErrorCode);

    private static OperationResult<T> VisualCostRecordsCancelled<T>()
        => OperationResult<T>.Fail(VisualCostRecordsCancelledSafeMessage, "CANCELLED");

    private static IReadOnlyDictionary<string, string> BuildLocalArtifactDbStatusMetadata(
        DesignConversionLocalArtifactDbRecord record)
        => new Dictionary<string, string>
        {
            ["local_artifact_db_persistence_verified"] = "true",
            ["local_artifact_db_record_id"] = record.RecordId,
            ["storage_training_artifact_db"] = "true",
            ["local_artifact_db"] = "true",
            ["conversion_result"] = "refs_or_hashes_only",
            ["render_artifact_refs"] = "refs_or_hashes_only",
            ["visual_judge_status"] = record.VisualJudgeStatus.Status,
            ["runtime_created_dpo_candidate"] = (record.DpoCandidates.Count > 0).ToString(),
            ["runtime_created_dpo_candidate_count"] = record.DpoCandidates.Count.ToString(),
            ["runtime_created_dpo_candidate_review_status"] = record.DpoCandidates.Count > 0
                ? "runtime-candidate-review-pending"
                : "not-created",
            ["review_required"] = (record.DpoCandidates.Count > 0).ToString(),
            ["approval_required"] = (record.DpoCandidates.Count > 0).ToString(),
            ["approval_status"] = record.DpoCandidates.Count > 0 ? "pending" : "not-required",
            ["approved_for_training"] = "false",
            ["training_candidate_only"] = record.DpoCandidates.Any(candidate => candidate.TrainingCandidateOnly).ToString(),
            ["production_dpo"] = "false",
            ["high_quality_negative_pair_not_proven"] = "true",
            ["held_out_not_proven"] = "true",
            ["source_disjoint_not_proven"] = "true",
            ["product_not_ready"] = "true",
            ["model_readiness_claimed"] = "false",
            ["public_api_verified"] = "false",
            ["production_db_ready"] = "false",
            ["trained_model_ready"] = "false"
        };

    private static IReadOnlyDictionary<string, string> BuildVisualCostLedgerStatusMetadata(
        DesignConversionVisualCostLedgerRecord record)
        => new Dictionary<string, string>
        {
            ["visual_cost_ledger_persistence_verified"] = bool.TrueString,
            ["visual_cost_ledger_record_id"] = record.RecordId,
            ["visual_cost_ledger_kind"] = record.LedgerKind,
            ["visual_cost_ledger_schema_version"] = record.SchemaVersion,
            ["visual_cost_ledger_total_cost_usd"] = record.TotalCostUsd.ToString("G29", CultureInfo.InvariantCulture),
            ["visual_cost_ledger_measurement_complete"] = record.MeasurementComplete.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_ledger_missing_measurement_count"] = record.MissingMeasurementCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_ledger_evaluation_attempt_count"] = record.EvaluationAttemptCount.ToString(CultureInfo.InvariantCulture),
            ["visual_cost_ledger_cost_source"] = record.CostSource,
            ["visual_cost_ledger_user_ref"] = record.UserRef,
            ["visual_cost_ledger_user_ref_semantics"] = "pseudonymous_technical_scoped_sha256_not_anonymous_not_secret",
            ["visual_cost_ledger_submitted_tier"] = record.SubmittedTier,
            ["visual_cost_ledger_tier_authority"] = "submitted_context_only",
            ["visual_cost_ledger_period_kind"] = record.PeriodKind,
            ["visual_cost_ledger_period_start_utc"] = record.PeriodStartUtc.ToString("O", CultureInfo.InvariantCulture),
            ["visual_cost_ledger_period_end_exclusive_utc"] = record.PeriodEndExclusiveUtc.ToString("O", CultureInfo.InvariantCulture),
            ["visual_cost_ledger_training_artifact_db"] = bool.FalseString,
            ["visual_cost_ledger_quota_enforcement"] = bool.FalseString,
            ["visual_cost_ledger_monthly_cap_enforcement"] = bool.FalseString,
            ["visual_cost_ledger_scope"] = "record_and_aggregate_only"
        };

    private static DesignConversionThinClientStatusResult BuildStatusResult(
        DesignConversionThinClientJob job,
        string contractStatus,
        string message,
        IReadOnlyList<DesignConversionThinClientArtifactRef> artifacts,
        IReadOnlyList<string> extraWarnings,
        IReadOnlyDictionary<string, string>? provenance)
        => new(
            Job: job,
            ContractStatus: contractStatus,
            Message: message,
            Artifacts: artifacts,
            Warnings: BoundaryWarnings
                .Concat(extraWarnings)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            Provenance: MergeDictionaries(
                provenance,
                new Dictionary<string, string>
                {
                    ["submitted_request_contract"] = nameof(SubmittedDesignConversionRequest),
                    ["job_contract"] = nameof(DesignConversionThinClientJob),
                    ["status_contract"] = nameof(DesignConversionThinClientStatusResult),
                    ["runtime_adapter"] = nameof(DesignConversionThinClientJobRuntime),
                    ["runtime_boundary"] = RuntimeBoundary,
                    ["storage_boundary"] = StorageBoundary,
                    ["public_api_verified"] = "false",
                    ["flow47_verification_authority"] = Flow47VerificationAuthority,
                    ["flow47_verification_state"] = Flow47VerificationState,
                    ["persistence_verified"] = "false",
                    ["product_ready_verified"] = "false",
                    ["runtime_ready_verified"] = "false",
                    ["host_readiness_verified"] = "false"
                }));

    private DesignConversionThinClientJob UpdateJob(
        DesignConversionThinClientJob job,
        string status,
        string? errorCode)
        => job with
        {
            Status = status,
            UpdatedAtUtc = _utcNow(),
            Metadata = BuildJobMetadata(
                _requests.TryGetValue(job.JobId, out var request) ? request : null,
                status,
                errorCode)
        };

    private static IReadOnlyDictionary<string, string> BuildJobMetadata(
        SubmittedDesignConversionRequest? request,
        string status,
        string? errorCode)
    {
        var metadata = new Dictionary<string, string>
        {
            ["status"] = status,
            ["runtime_boundary"] = RuntimeBoundary,
            ["storage_boundary"] = StorageBoundary,
            ["public_api_verified"] = "false",
            ["flow47_verification_authority"] = Flow47VerificationAuthority,
            ["flow47_verification_state"] = Flow47VerificationState,
            ["platform_integration_verified"] = "false",
            ["persistence_verified"] = "false",
            ["product_ready_verified"] = "false",
            ["runtime_ready_verified"] = "false",
            ["trained_feature_verified"] = "false"
        };

        if (request is not null)
        {
            metadata["source_kind"] = request.SourceKind.ToString();
            metadata["budget_tier"] = request.Budget.Tier;
        }

        if (!string.IsNullOrWhiteSpace(errorCode))
            metadata["error_code"] = errorCode;

        return metadata;
    }

    private static DesignConversionThinClientArtifactRef BuildArtifact(
        DesignConversionThinClientJob job,
        string kind,
        string mediaType,
        string content)
    {
        var sha = Sha256Text(content ?? string.Empty);
        return new DesignConversionThinClientArtifactRef(
            ArtifactId: $"{job.JobId}:{kind}",
            Kind: kind,
            Uri: $"memory://design-conversion/jobs/{Uri.EscapeDataString(job.JobId)}/{kind}",
            Sha256: $"sha256:{sha}",
            MediaType: mediaType,
            Metadata: new Dictionary<string, string>
            {
                ["request_id"] = job.RequestId,
                ["job_id"] = job.JobId,
                ["storage_boundary"] = StorageBoundary
            });
    }

    private static string BuildJobId(string requestId)
    {
        var sanitized = new string(requestId
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-')
            .ToArray());
        return $"thin-job-{sanitized}";
    }

    private static string BuildStatusUri(string jobId)
        => $"memory://design-conversion/jobs/{Uri.EscapeDataString(jobId)}/status";

    private static IReadOnlyDictionary<string, string> MergeDictionaries(
        IReadOnlyDictionary<string, string>? first,
        IReadOnlyDictionary<string, string> second)
        => (first ?? new Dictionary<string, string>())
            .Concat(second)
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Last().Value, StringComparer.Ordinal);

    private static string Sha256Text(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed record VisualCostBudgetReservationContext(
        DesignConversionVisualCostReservationRequest Request,
        DesignConversionVisualCostReservationResult? Reservation);

    private sealed record VisualCostBudgetSettlementContext(
        DesignConversionVisualCostSettlementResult Settlement,
        decimal ChargedCostUsd,
        bool ActualCostKnown,
        bool FailJob,
        string FailureCode,
        string FailureMessage);
}
