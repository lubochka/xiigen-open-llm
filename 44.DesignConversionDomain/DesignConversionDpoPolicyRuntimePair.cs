// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

internal static class DesignConversionDpoPolicyRuntimePairContract
{
    internal const string Schema = "design_conversion_dpo_policy_runtime_pair.v1";
    internal const string GeneratedSide = "generated";
    internal const string IncumbentSide = "incumbent";
    internal const string RootOwnershipBindingProvenanceKey =
        DesignConversionRuntimeRejectedEvaluationHandoff
            .RootOwnershipBindingProvenanceKey;
    internal const string SubmittedRequestJobIdProvenanceKey =
        "internal_service_job_id";
    internal const string PairBindingInvalidReason =
        "dpo_policy_runtime_pair_binding_invalid";
    internal const string PairContentInvalidReason =
        "dpo_policy_runtime_pair_content_invalid";
    internal const string PairRootInvalidReason =
        "dpo_policy_runtime_pair_root_invalid";
    internal const string PairReceiptInvalidReason =
        "dpo_policy_runtime_pair_receipt_invalid";
}

internal sealed record DesignConversionDpoPolicyRuntimeSide(
    string Side,
    string Html,
    string Css,
    string ContentSha256,
    string CustodyRef,
    string CustodyBinding,
    string EvaluationIdentity,
    string EvaluationJobId,
    string EvaluationRequestId,
    string OutputId,
    string CandidateId,
    string VerdictId);

/// <summary>
/// Process-local, coordinator-owned carrier for one exact, independently
/// evaluated generated/incumbent pair.  It deliberately has no caller supplied
/// chosen/preferred label: the policy scorer always computes generated minus
/// incumbent from the two exact sides.
/// </summary>
internal sealed record DesignConversionDpoPolicyRuntimePair(
    string Schema,
    int AttemptNumber,
    string RootJobId,
    string RootRequestId,
    string RootOwnershipBinding,
    string GeneratedCandidateId,
    DesignConversionDpoPolicyRuntimeSide Generated,
    DesignConversionDpoPolicyRuntimeSide Incumbent,
    string ArtifactRootBinding,
    DesignConversionIndependentPreferenceEvidence IndependentEvaluation,
    DateTimeOffset ObservedAtUtc,
    string PairBindingSha256,
    DesignConversionTeacherAuthoredEvaluationPair SourcePair)
{
    [System.Text.Json.Serialization.JsonIgnore]
    internal object? RuntimeAuthorityCapability { get; init; }

    internal string BuildPolicyPrompt()
        => string.Join(
            " ",
            "design-conversion-runtime-pair",
            $"root={RootOwnershipBinding}",
            $"attempt={AttemptNumber.ToString(CultureInfo.InvariantCulture)}",
            $"generated={Generated.CustodyRef}",
            $"incumbent={Incumbent.CustodyRef}");
}

internal interface IDesignConversionDpoPolicyPairRuntimeEvaluator
{
    Task<OperationResult<DesignConversionInternalEvaluationResult>>
        ApplyDpoPolicyRuntimePairAsync(
            DesignConversionInternalEvaluationResult evaluation,
            DesignConversionDpoPolicyRuntimePair pair,
            CancellationToken cancellationToken = default);
}

internal static class DesignConversionDpoPolicyRuntimePairFactory
{
    internal static OperationResult<DesignConversionDpoPolicyRuntimePair> Create(
        DesignConversionInternalEvaluateRequest rootRequest,
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DesignConversionIndependentPreferenceEvidence independentEvaluation,
        string artifactRootBinding,
        DateTimeOffset observedAtUtc)
    {
        if (rootRequest is null || pair is null || independentEvaluation is null)
            return Invalid(DesignConversionDpoPolicyRuntimePairContract.PairBindingInvalidReason);

        if (!DesignConversionDpoGenerator
                .TryValidateIndependentPreferenceEvidenceForRuntime(
                    pair,
                    independentEvaluation,
                    artifactRootBinding,
                    observedAtUtc,
                    out _))
        {
            return Invalid(DesignConversionDpoPolicyRuntimePairContract.PairReceiptInvalidReason);
        }

        var rootBinding = BuildRootOwnershipBinding(
            rootRequest.JobId,
            rootRequest.RequestId);
        if (!HasRootOwnership(rootRequest.Provenance, rootBinding) ||
            !HasRootOwnership(pair.GeneratedEvaluation, rootBinding) ||
            !HasRootOwnership(pair.IncumbentEvaluation, rootBinding))
        {
            return Invalid(DesignConversionDpoPolicyRuntimePairContract.PairRootInvalidReason);
        }

        var authoring = pair.AuthoringEvidence;
        var generatedRequest = pair.GeneratedEvaluation.SubmittedRequest;
        var incumbentRequest = pair.IncumbentEvaluation.SubmittedRequest;
        if (pair.AttemptNumber < 2 ||
            pair.AttemptNumber != authoring.AttemptNumber ||
            string.IsNullOrWhiteSpace(pair.GeneratedCandidateId) ||
            !string.Equals(
                pair.GeneratedCandidateId,
                authoring.GeneratedCandidateId,
                StringComparison.Ordinal) ||
            !string.Equals(
                generatedRequest.RequestId,
                authoring.GeneratedRequestId,
                StringComparison.Ordinal) ||
            !string.Equals(
                authoring.GeneratedBinding,
                DesignConversionDpoTeacherAttestationRuntime
                    .BuildGeneratedBinding(
                        generatedRequest.Html,
                        generatedRequest.Css),
                StringComparison.Ordinal) ||
            !string.Equals(
                authoring.IncumbentBinding,
                DesignConversionDpoTeacherAttestationRuntime
                    .BuildIncumbentBinding(incumbentRequest),
                StringComparison.Ordinal))
        {
            return Invalid(DesignConversionDpoPolicyRuntimePairContract.PairBindingInvalidReason);
        }

        var generated = BuildSide(
            DesignConversionDpoPolicyRuntimePairContract.GeneratedSide,
            pair.GeneratedEvaluation,
            authoring.GeneratedRef,
            authoring.GeneratedBinding);
        var incumbent = BuildSide(
            DesignConversionDpoPolicyRuntimePairContract.IncumbentSide,
            pair.IncumbentEvaluation,
            authoring.IncumbentRef,
            authoring.IncumbentBinding);
        if (!SidesAreDistinct(generated, incumbent))
            return Invalid(DesignConversionDpoPolicyRuntimePairContract.PairContentInvalidReason);

        var candidate = new DesignConversionDpoPolicyRuntimePair(
            DesignConversionDpoPolicyRuntimePairContract.Schema,
            pair.AttemptNumber,
            rootRequest.JobId,
            rootRequest.RequestId,
            rootBinding,
            pair.GeneratedCandidateId,
            generated,
            incumbent,
            artifactRootBinding,
            independentEvaluation,
            observedAtUtc.ToUniversalTime(),
            PairBindingSha256: string.Empty,
            pair);
        var bound = candidate with
        {
            PairBindingSha256 = BuildPairBindingSha256(candidate)
        };
        return OperationResult<DesignConversionDpoPolicyRuntimePair>.Ok(
            bound with
            {
                RuntimeAuthorityCapability = new RuntimePairAuthorityCapability(
                    bound.PairBindingSha256,
                    bound.IndependentEvaluation.ReceiptSha256)
            });
    }

    internal static bool TryValidateForRuntime(
        DesignConversionDpoPolicyRuntimePair? pair,
        SubmittedDesignConversionRequest? currentRequest,
        DateTimeOffset observedAtUtc,
        out string reason)
    {
        reason = DesignConversionDpoPolicyRuntimePairContract.PairBindingInvalidReason;
        if (pair is null || currentRequest is null ||
            !string.Equals(
                pair.Schema,
                DesignConversionDpoPolicyRuntimePairContract.Schema,
                StringComparison.Ordinal) ||
            pair.SourcePair is null || pair.IndependentEvaluation is null ||
            pair.RuntimeAuthorityCapability is not
                RuntimePairAuthorityCapability capability ||
            !capability.Matches(pair) ||
            pair.AttemptNumber != pair.SourcePair.AttemptNumber ||
            !string.Equals(
                pair.GeneratedCandidateId,
                pair.SourcePair.GeneratedCandidateId,
                StringComparison.Ordinal) ||
            !string.Equals(
                pair.PairBindingSha256,
                BuildPairBindingSha256(pair),
                StringComparison.Ordinal))
        {
            return false;
        }

        if (!DesignConversionDpoGenerator
                .TryValidateIndependentPreferenceEvidenceForRuntime(
                    pair.SourcePair,
                    pair.IndependentEvaluation,
                    pair.ArtifactRootBinding,
                    observedAtUtc,
                    out _))
        {
            reason = DesignConversionDpoPolicyRuntimePairContract.PairReceiptInvalidReason;
            return false;
        }

        var expectedGenerated = BuildSide(
            DesignConversionDpoPolicyRuntimePairContract.GeneratedSide,
            pair.SourcePair.GeneratedEvaluation,
            pair.SourcePair.AuthoringEvidence.GeneratedRef,
            pair.SourcePair.AuthoringEvidence.GeneratedBinding);
        var expectedIncumbent = BuildSide(
            DesignConversionDpoPolicyRuntimePairContract.IncumbentSide,
            pair.SourcePair.IncumbentEvaluation,
            pair.SourcePair.AuthoringEvidence.IncumbentRef,
            pair.SourcePair.AuthoringEvidence.IncumbentBinding);
        if (pair.Generated != expectedGenerated ||
            pair.Incumbent != expectedIncumbent ||
            !SidesAreDistinct(pair.Generated, pair.Incumbent) ||
            !string.Equals(currentRequest.RequestId,
                pair.Generated.EvaluationRequestId, StringComparison.Ordinal) ||
            !string.Equals(currentRequest.Html,
                pair.Generated.Html, StringComparison.Ordinal) ||
            !string.Equals(currentRequest.Css,
                pair.Generated.Css, StringComparison.Ordinal))
        {
            reason = DesignConversionDpoPolicyRuntimePairContract.PairContentInvalidReason;
            return false;
        }

        var expectedRootBinding = BuildRootOwnershipBinding(
            pair.RootJobId,
            pair.RootRequestId);
        if (!string.Equals(pair.RootOwnershipBinding,
                expectedRootBinding, StringComparison.Ordinal) ||
            !HasRootOwnership(currentRequest.Provenance, expectedRootBinding) ||
            !HasRootOwnership(pair.SourcePair.GeneratedEvaluation,
                expectedRootBinding) ||
            !HasRootOwnership(pair.SourcePair.IncumbentEvaluation,
                expectedRootBinding))
        {
            reason = DesignConversionDpoPolicyRuntimePairContract.PairRootInvalidReason;
            return false;
        }

        reason = string.Empty;
        return true;
    }

    internal static bool TryConsumeRuntimeAuthority(
        DesignConversionDpoPolicyRuntimePair pair)
        => pair?.RuntimeAuthorityCapability is
               RuntimePairAuthorityCapability capability &&
           capability.TryConsume(pair);

    internal static string BuildPairBindingSha256(
        DesignConversionDpoPolicyRuntimePair pair)
    {
        var snapshot = new
        {
            schema = pair.Schema ?? string.Empty,
            attempt_number = pair.AttemptNumber,
            root_job_id = pair.RootJobId ?? string.Empty,
            root_request_id = pair.RootRequestId ?? string.Empty,
            root_ownership_binding = pair.RootOwnershipBinding ?? string.Empty,
            generated_candidate_id = pair.GeneratedCandidateId ?? string.Empty,
            generated = PairSideSnapshot(pair.Generated),
            incumbent = PairSideSnapshot(pair.Incumbent),
            artifact_root_binding = pair.ArtifactRootBinding ?? string.Empty,
            independent_schema =
                pair.IndependentEvaluation?.Schema ?? string.Empty,
            independent_attempt_number =
                pair.IndependentEvaluation?.AttemptNumber ?? 0,
            independent_generated_ref =
                pair.IndependentEvaluation?.GeneratedRef ?? string.Empty,
            independent_incumbent_ref =
                pair.IndependentEvaluation?.IncumbentRef ?? string.Empty,
            independent_generated_binding =
                pair.IndependentEvaluation?.GeneratedBinding ?? string.Empty,
            independent_incumbent_binding =
                pair.IndependentEvaluation?.IncumbentBinding ?? string.Empty,
            independent_generated_evaluation_identity =
                pair.IndependentEvaluation?.GeneratedEvaluationIdentity ??
                string.Empty,
            independent_incumbent_evaluation_identity =
                pair.IndependentEvaluation?.IncumbentEvaluationIdentity ??
                string.Empty,
            independent_preferred_direction =
                pair.IndependentEvaluation?.PreferredDirection ?? string.Empty,
            independent_receipt_sha256 =
                pair.IndependentEvaluation?.ReceiptSha256 ?? string.Empty,
            scorer_request_sha256 =
                pair.IndependentEvaluation?.ScorerRequestSha256 ?? string.Empty,
            scorer_output_sha256 =
                pair.IndependentEvaluation?.ScorerOutputSha256 ?? string.Empty,
            evaluation_profile_id =
                pair.IndependentEvaluation?.EvaluationProfileId ?? string.Empty,
            evaluation_profile_version =
                pair.IndependentEvaluation?.EvaluationProfileVersion ??
                string.Empty,
            evaluation_profile_configuration_sha256 =
                pair.IndependentEvaluation?
                    .EvaluationProfileConfigurationSha256 ?? string.Empty,
            evaluation_model_sha256 =
                pair.IndependentEvaluation?.EvaluationModelSha256 ?? string.Empty,
            independent_artifact_root_binding =
                pair.IndependentEvaluation?.ArtifactRootBinding ?? string.Empty,
            scorer_provenance = IndependentScorerProvenanceSnapshot(
                pair.IndependentEvaluation?.ScorerProvenance),
            independent_issued_at_utc = pair.IndependentEvaluation?.IssuedAtUtc
                .ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ??
                string.Empty,
            independent_expires_at_utc = pair.IndependentEvaluation?.ExpiresAtUtc
                .ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ??
                string.Empty,
            independent_replay_nonce =
                pair.IndependentEvaluation?.ReplayNonce ?? string.Empty,
            source_pair_attempt_number = pair.SourcePair?.AttemptNumber ?? 0,
            source_pair_generated_candidate_id =
                pair.SourcePair?.GeneratedCandidateId ?? string.Empty,
            source_pair_generated_evaluation_identity =
                BuildSourceEvaluationIdentity(
                    pair.SourcePair?.GeneratedEvaluation),
            source_pair_incumbent_evaluation_identity =
                BuildSourceEvaluationIdentity(
                    pair.SourcePair?.IncumbentEvaluation),
            observed_at_utc = pair.ObservedAtUtc.ToUniversalTime()
                .ToString("O", CultureInfo.InvariantCulture)
        };
        return DesignConversionDpoGenerator.Sha256Ref(
            JsonSerializer.Serialize(snapshot));
    }

    private static object IndependentScorerProvenanceSnapshot(
        DesignConversionIndependentPreferenceScorerProvenance? provenance)
        => provenance is null
            ? new { missing = true }
            : new
            {
                kind = provenance.Kind ?? string.Empty,
                scorer_id = provenance.ScorerId ?? string.Empty,
                scorer_version = provenance.ScorerVersion ?? string.Empty,
                request_id = provenance.RequestId ?? string.Empty,
                output_id = provenance.OutputId ?? string.Empty,
                runtime_instance_ref =
                    provenance.RuntimeInstanceRef ?? string.Empty,
                evaluation_profile_id =
                    provenance.EvaluationProfileId ?? string.Empty,
                evaluation_profile_version =
                    provenance.EvaluationProfileVersion ?? string.Empty,
                evaluation_profile_configuration_sha256 =
                    provenance.EvaluationProfileConfigurationSha256 ??
                    string.Empty,
                evaluation_model_sha256 =
                    provenance.EvaluationModelSha256 ?? string.Empty,
                inference_request_id =
                    provenance.InferenceRequestId ?? string.Empty
            };

    private static string BuildSourceEvaluationIdentity(
        DesignConversionInternalEvaluationResult? evaluation)
        => evaluation is null
            ? string.Empty
            : DesignConversionDpoGenerator
                .BuildIndependentPreferenceEvaluationIdentity(evaluation);

    private static object PairSideSnapshot(
        DesignConversionDpoPolicyRuntimeSide? side)
        => side is null
            ? new { missing = true }
            : new
            {
                side = side.Side ?? string.Empty,
                html_sha256 = DesignConversionDpoGenerator.Sha256Ref(
                    side.Html ?? string.Empty),
                css_sha256 = DesignConversionDpoGenerator.Sha256Ref(
                    side.Css ?? string.Empty),
                content_sha256 = side.ContentSha256 ?? string.Empty,
                custody_ref = side.CustodyRef ?? string.Empty,
                custody_binding = side.CustodyBinding ?? string.Empty,
                evaluation_identity = side.EvaluationIdentity ?? string.Empty,
                evaluation_job_id = side.EvaluationJobId ?? string.Empty,
                evaluation_request_id = side.EvaluationRequestId ?? string.Empty,
                output_id = side.OutputId ?? string.Empty,
                candidate_id = side.CandidateId ?? string.Empty,
                verdict_id = side.VerdictId ?? string.Empty
            };

    private static DesignConversionDpoPolicyRuntimeSide BuildSide(
        string side,
        DesignConversionInternalEvaluationResult evaluation,
        string custodyRef,
        string custodyBinding)
    {
        var request = evaluation.SubmittedRequest;
        var output = evaluation.RuntimeFlowResult.Output;
        var serialized = DesignConversionDpoPolicyCheckpointBridge
            .SerializePolicySideText(request.Html, request.Css);
        return new DesignConversionDpoPolicyRuntimeSide(
            side,
            request.Html,
            request.Css,
            DesignConversionDpoGenerator.Sha256Ref(serialized),
            custodyRef,
            custodyBinding,
            DesignConversionDpoGenerator
                .BuildIndependentPreferenceEvaluationIdentity(evaluation),
            evaluation.JobId,
            request.RequestId,
            output.OutputId,
            output.CandidateId,
            output.Verdict.VerdictId);
    }

    private static bool SidesAreDistinct(
        DesignConversionDpoPolicyRuntimeSide? generated,
        DesignConversionDpoPolicyRuntimeSide? incumbent)
        => generated is not null && incumbent is not null &&
           string.Equals(generated.Side,
               DesignConversionDpoPolicyRuntimePairContract.GeneratedSide,
               StringComparison.Ordinal) &&
           string.Equals(incumbent.Side,
               DesignConversionDpoPolicyRuntimePairContract.IncumbentSide,
               StringComparison.Ordinal) &&
           !string.Equals(generated.ContentSha256,
               incumbent.ContentSha256, StringComparison.Ordinal) &&
           !string.Equals(generated.CustodyRef,
               incumbent.CustodyRef, StringComparison.Ordinal) &&
           !string.Equals(generated.CustodyBinding,
               incumbent.CustodyBinding, StringComparison.Ordinal) &&
           !string.Equals(generated.EvaluationIdentity,
               incumbent.EvaluationIdentity, StringComparison.Ordinal) &&
            !string.Equals(generated.EvaluationJobId,
                incumbent.EvaluationJobId, StringComparison.Ordinal) &&
            !string.Equals(generated.EvaluationRequestId,
                incumbent.EvaluationRequestId, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(generated.OutputId) &&
            !string.IsNullOrWhiteSpace(incumbent.OutputId) &&
            !string.Equals(generated.OutputId,
                incumbent.OutputId, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(generated.CandidateId) &&
            !string.IsNullOrWhiteSpace(incumbent.CandidateId) &&
            !string.Equals(generated.CandidateId,
                incumbent.CandidateId, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(generated.VerdictId) &&
            !string.IsNullOrWhiteSpace(incumbent.VerdictId) &&
            !string.Equals(generated.VerdictId,
                incumbent.VerdictId, StringComparison.Ordinal);

    private static bool HasRootOwnership(
        DesignConversionInternalEvaluationResult evaluation,
        string expectedRootBinding)
        => evaluation?.SubmittedRequest is { } request &&
           HasRootOwnership(request.Provenance, expectedRootBinding) &&
           request.Provenance.TryGetValue(
               DesignConversionDpoPolicyRuntimePairContract
                   .SubmittedRequestJobIdProvenanceKey,
               out var evaluationJobId) &&
           string.Equals(
               evaluationJobId,
               evaluation.JobId,
               StringComparison.Ordinal);

    private static bool HasRootOwnership(
        IReadOnlyDictionary<string, string>? provenance,
        string expectedRootBinding)
        => provenance is not null &&
           provenance.TryGetValue(
               DesignConversionDpoPolicyRuntimePairContract
                   .RootOwnershipBindingProvenanceKey,
               out var rootBinding) &&
           string.Equals(rootBinding, expectedRootBinding,
               StringComparison.Ordinal);

    private static string BuildRootOwnershipBinding(
        string rootJobId,
        string rootRequestId)
        => DesignConversionDpoLeakageGuard.HashRef(
            "dcm-runtime-rejected-root-ownership",
            string.Concat(rootJobId, "|", rootRequestId));

    private static OperationResult<DesignConversionDpoPolicyRuntimePair> Invalid(
        string reason)
        => OperationResult<DesignConversionDpoPolicyRuntimePair>.Fail(
            "The coordinator-owned DPO runtime pair is unavailable.",
            reason);

    private sealed class RuntimePairAuthorityCapability(
        string pairBindingSha256,
        string receiptSha256)
    {
        private int _consumed;

        internal bool Matches(DesignConversionDpoPolicyRuntimePair pair)
            => pair is not null &&
               string.Equals(pairBindingSha256,
                   pair.PairBindingSha256, StringComparison.Ordinal) &&
               string.Equals(receiptSha256,
                   pair.IndependentEvaluation?.ReceiptSha256,
                   StringComparison.Ordinal);

        internal bool TryConsume(DesignConversionDpoPolicyRuntimePair pair)
            => Matches(pair) &&
               Interlocked.CompareExchange(ref _consumed, 1, 0) == 0;
    }
}
