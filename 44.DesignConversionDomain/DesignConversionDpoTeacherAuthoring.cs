// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

internal sealed record DesignConversionDpoTeacherCandidateAuthoringEvidence(
    string Schema,
    string IncumbentRef,
    string GeneratedRef,
    string IncumbentBinding,
    string GeneratedBinding,
    string GeneratedCandidateId,
    string GeneratedRequestId,
    string OutputHash,
    TrainingTeacherProvenance Provenance,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata,
    int AttemptNumber = 2)
{
    internal string InferenceProfileId { get; init; } = string.Empty;

    internal string InferenceProfileConfigurationSha256 { get; init; } =
        string.Empty;
}

internal sealed record DesignConversionTeacherAuthoredEvaluationPair(
    int AttemptNumber,
    DesignConversionInternalEvaluationResult GeneratedEvaluation,
    DesignConversionInternalEvaluationResult IncumbentEvaluation,
    DesignConversionDpoTeacherCandidateAuthoringEvidence AuthoringEvidence,
    string GeneratedCandidateId = "");

internal static class DesignConversionIndependentPreferenceEvidenceContract
{
    internal const string Schema =
        "design_conversion_independent_preference_evidence.v1";
    internal const string EvaluationOnlyProfileId =
        LocalQwenOllamaInferenceProfileAuthority.EvaluationProfileId;
    internal const string EvaluationOnlyProfileVersion = "1";
    internal static readonly string EvaluationOnlyProfileConfigurationSha256 =
        LocalQwenOllamaInferenceProfileAuthority.Evaluation.ConfigurationSha256;
    internal static readonly string EvaluationOnlyModelSha256 =
        BuildContractSha256(
            "design_conversion_independent_local_evaluation_model_fixture|v1");
    internal const string LocalScorerProvenanceKind =
        "independent_local_evaluation_scorer";
    internal const string GeneratedSide = "generated";
    internal const string IncumbentSide = "incumbent";
    internal static readonly TimeSpan MaximumFreshness = TimeSpan.FromMinutes(5);

    private static string BuildContractSha256(string value)
        => $"sha256:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant()}";
}

internal sealed record DesignConversionIndependentPreferenceScorerProvenance(
    string Kind,
    string ScorerId,
    string ScorerVersion,
    string RequestId,
    string OutputId,
    string RuntimeInstanceRef)
{
    internal string EvaluationProfileId { get; init; } =
        DesignConversionIndependentPreferenceEvidenceContract
            .EvaluationOnlyProfileId;

    internal string EvaluationProfileVersion { get; init; } =
        DesignConversionIndependentPreferenceEvidenceContract
            .EvaluationOnlyProfileVersion;

    internal string EvaluationProfileConfigurationSha256 { get; init; } =
        DesignConversionIndependentPreferenceEvidenceContract
            .EvaluationOnlyProfileConfigurationSha256;

    internal string EvaluationModelSha256 { get; init; } =
        DesignConversionIndependentPreferenceEvidenceContract
            .EvaluationOnlyModelSha256;

    internal string InferenceRequestId { get; init; } = RequestId;
}

/// <summary>
/// Evidence input accepted from a separately wired local evaluation scorer.
/// The local issuer is opt-in and is not registered by default production
/// composition. Its authority capability is deliberately process-local and
/// non-serializable in this slice.
/// </summary>
internal sealed record DesignConversionIndependentPreferenceEvidence(
    string Schema,
    int AttemptNumber,
    string GeneratedRef,
    string IncumbentRef,
    string GeneratedBinding,
    string IncumbentBinding,
    string GeneratedEvaluationIdentity,
    string IncumbentEvaluationIdentity,
    string PreferredDirection,
    string ScorerRequestSha256,
    string ScorerOutputSha256,
    string EvaluationProfileId,
    string EvaluationProfileVersion,
    string EvaluationProfileConfigurationSha256,
    string EvaluationModelSha256,
    string ArtifactRootBinding,
    DesignConversionIndependentPreferenceScorerProvenance ScorerProvenance,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    string ReplayNonce,
    string ReceiptSha256)
{
    [System.Text.Json.Serialization.JsonIgnore]
    internal object? IssuerAuthorityCapability { get; init; }
}

internal interface IDesignConversionIndependentPreferenceEvidenceSource
{
    bool TryConsumeEvidence(
        string artifactRootBinding,
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DateTimeOffset observedAtUtc,
        out DesignConversionIndependentPreferenceEvidence evidence);
}

internal interface IDesignConversionIndependentPreferenceEvidenceIssuer
    : IDesignConversionIndependentPreferenceEvidenceSource
{
    Task<OperationResult<DesignConversionIndependentPreferenceEvidence>>
        IssueAsync(
            string artifactRootBinding,
            DesignConversionTeacherAuthoredEvaluationPair pair,
            DateTimeOffset observedAtUtc,
            CancellationToken cancellationToken = default);
}

internal static class DesignConversionIndependentPreferenceEvidenceAssociation
{
    internal const string ArtifactRootAlreadyAssociatedErrorCode =
        "DESIGN_CONVERSION_LOCAL_EVALUATOR_ARTIFACT_ROOT_ALREADY_ASSOCIATED";
    private const string AssociationFailedSafeMessage =
        "The local independent preference evaluator could not own the artifact root.";
    private static readonly object Sync = new();
    private static readonly ConditionalWeakTable<
        IDesignConversionLocalArtifactDb,
        Holder> Sources = new();
    private static readonly Dictionary<
        string,
        WeakReference<IDesignConversionLocalArtifactDb>>
        ActiveArtifactRootBindings =
        new(StringComparer.Ordinal);

    internal static OperationResult<IDisposable> AssociateArtifactDb(
        IDesignConversionLocalArtifactDb artifactDb,
        IDesignConversionIndependentPreferenceEvidenceIssuer source)
    {
        ArgumentNullException.ThrowIfNull(artifactDb);
        ArgumentNullException.ThrowIfNull(source);
        var artifactRootBinding = BuildArtifactRootBinding(artifactDb);
        lock (Sync)
        {
            if (Sources.TryGetValue(artifactDb, out _) ||
                ActiveArtifactRootBindings.TryGetValue(
                    artifactRootBinding,
                    out var existingRoot) &&
                existingRoot.TryGetTarget(out _))
            {
                return OperationResult<IDisposable>.Fail(
                    AssociationFailedSafeMessage,
                    ArtifactRootAlreadyAssociatedErrorCode);
            }

            ActiveArtifactRootBindings[artifactRootBinding] =
                new WeakReference<IDesignConversionLocalArtifactDb>(artifactDb);

            Sources.Add(
                artifactDb,
                new Holder(source, artifactRootBinding));
        }

        return OperationResult<IDisposable>.Ok(
            new AssociationLease(
                artifactDb,
                source,
                artifactRootBinding,
                ownsArtifactRootBinding: true));
    }

    internal static IDisposable AssociateArtifactDbForTesting(
        IDesignConversionLocalArtifactDb artifactDb,
        IDesignConversionIndependentPreferenceEvidenceSource source)
    {
        ArgumentNullException.ThrowIfNull(artifactDb);
        ArgumentNullException.ThrowIfNull(source);
        lock (Sync)
        {
            Sources.Remove(artifactDb);
            Sources.Add(
                artifactDb,
                new Holder(source, BuildArtifactRootBinding(artifactDb)));
        }

        return new AssociationLease(
            artifactDb,
            source,
            artifactRootBinding: string.Empty,
            ownsArtifactRootBinding: false);
    }

    internal static EvidenceSourceRegistration? Resolve(
        IDesignConversionLocalArtifactDb artifactDb)
    {
        lock (Sync)
            return Sources.TryGetValue(artifactDb, out var holder)
                ? holder.Registration
                : null;
    }

    internal static string BuildArtifactRootBinding(
        IDesignConversionLocalArtifactDb artifactDb)
    {
        ArgumentNullException.ThrowIfNull(artifactDb);
        var rootIdentity = artifactDb is
            IDesignConversionLocalPromotionActivationAuthority rooted
                ? Path.GetFullPath(rooted.AllowedArtifactRootDirectory)
                : string.Concat(
                    artifactDb.GetType().AssemblyQualifiedName,
                    "|instance=",
                    RuntimeHelpers.GetHashCode(artifactDb)
                        .ToString(CultureInfo.InvariantCulture));
        if (OperatingSystem.IsWindows() &&
            artifactDb is IDesignConversionLocalPromotionActivationAuthority)
        {
            rootIdentity = rootIdentity.ToUpperInvariant();
        }
        return DesignConversionDpoGenerator.Sha256Ref(
            $"design-conversion-local-artifact-root|{rootIdentity}");
    }

    internal sealed class EvidenceSourceRegistration(
        IDesignConversionIndependentPreferenceEvidenceSource source,
        string artifactRootBinding)
    {
        private readonly object _consumeSync = new();
        private readonly HashSet<string> _consumedReceipts =
            new(StringComparer.Ordinal);
        private readonly HashSet<string> _consumedAttempts =
            new(StringComparer.Ordinal);

        internal IDesignConversionIndependentPreferenceEvidenceSource Source { get; } =
            source;

        internal string ArtifactRootBinding { get; } = artifactRootBinding;

        internal bool CanIssueEvidence =>
            Source is IDesignConversionIndependentPreferenceEvidenceIssuer;

        internal Task<OperationResult<
            DesignConversionIndependentPreferenceEvidence>> IssueEvidenceAsync(
                DesignConversionTeacherAuthoredEvaluationPair pair,
                DateTimeOffset observedAtUtc,
                CancellationToken cancellationToken)
        {
            if (Source is not
                IDesignConversionIndependentPreferenceEvidenceIssuer issuer)
            {
                return Task.FromResult(
                    OperationResult<
                        DesignConversionIndependentPreferenceEvidence>.Fail(
                        "The local independent preference evaluator is not configured.",
                        "DESIGN_CONVERSION_LOCAL_EVALUATOR_NOT_CONFIGURED"));
            }

            return issuer.IssueAsync(
                ArtifactRootBinding,
                pair,
                observedAtUtc,
                cancellationToken);
        }

        internal bool TryConsumeEvidence(
            DesignConversionTeacherAuthoredEvaluationPair pair,
            DateTimeOffset observedAtUtc,
            out DesignConversionIndependentPreferenceEvidence evidence)
        {
            evidence = null!;
            if (!Source.TryConsumeEvidence(
                    ArtifactRootBinding,
                    pair,
                    observedAtUtc,
                    out var candidate) ||
                candidate is null)
            {
                return false;
            }

            var attemptKey = string.Join("|",
            [
                ArtifactRootBinding,
                pair.AttemptNumber.ToString(CultureInfo.InvariantCulture),
                pair.AuthoringEvidence.GeneratedRef,
                pair.AuthoringEvidence.IncumbentRef
            ]);
            lock (_consumeSync)
            {
                if (_consumedReceipts.Contains(candidate.ReceiptSha256) ||
                    _consumedAttempts.Contains(attemptKey))
                {
                    return false;
                }

                _consumedReceipts.Add(candidate.ReceiptSha256);
                _consumedAttempts.Add(attemptKey);
            }

            evidence = candidate;
            return true;
        }
    }

    private sealed record Holder(
        EvidenceSourceRegistration Registration)
    {
        internal Holder(
            IDesignConversionIndependentPreferenceEvidenceSource source,
            string artifactRootBinding)
            : this(new EvidenceSourceRegistration(source, artifactRootBinding))
        {
        }

        internal IDesignConversionIndependentPreferenceEvidenceSource Source =>
            Registration.Source;
    }

    private sealed class AssociationLease(
        IDesignConversionLocalArtifactDb artifactDb,
        IDesignConversionIndependentPreferenceEvidenceSource source,
        string artifactRootBinding,
        bool ownsArtifactRootBinding)
        : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            lock (Sync)
            {
                if (Sources.TryGetValue(artifactDb, out var holder) &&
                    ReferenceEquals(holder.Source, source))
                {
                    Sources.Remove(artifactDb);
                }
                if (ownsArtifactRootBinding &&
                    ActiveArtifactRootBindings.TryGetValue(
                        artifactRootBinding,
                        out var activeRoot) &&
                    activeRoot.TryGetTarget(out var activeArtifactDb) &&
                    ReferenceEquals(activeArtifactDb, artifactDb))
                {
                    ActiveArtifactRootBindings.Remove(artifactRootBinding);
                }
            }
        }
    }
}

internal sealed record DesignConversionTeacherAuthoredCandidateResult(
    DesignConversionImprovementCandidate Candidate,
    DesignConversionDpoTeacherCandidateAuthoringEvidence? AuthoringEvidence,
    string IncumbentBinding);

/// <summary>
/// Internal capability implemented only by a candidate strategy that can use a
/// server-owned legal teacher call to author one candidate. Candidate authoring
/// never supplies preference directionality.
/// The public candidate-strategy contract remains unchanged.
/// </summary>
internal interface IDesignConversionTeacherAuthoringCandidateStrategy
{
    Task<OperationResult<DesignConversionTeacherAuthoredCandidateResult>>
        AuthorCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            DesignConversionDpoTeacherCallReservation reservation,
            CancellationToken cancellationToken = default);
}

/// <summary>
/// Adds legal teacher authoring to an existing strategy without owning either
/// the artifact database or the activation-owned teacher runtime. If no active
/// runtime is associated with the exact database instance, the wrapped strategy
/// is used unchanged and no preference authority is created.
/// </summary>
internal sealed class DesignConversionDpoTeacherAuthoringCandidateStrategy(
    IDesignConversionImprovementCandidateStrategy inner,
    IDesignConversionLocalArtifactDb artifactDb)
    : IDesignConversionImprovementCandidateStrategy,
      IDesignConversionTeacherAuthoringCandidateStrategy
{
    internal const string StrategyMarker =
        "design_conversion_runtime_teacher_authoring_candidate_strategy";
    internal const string CandidateAuthoringMarker =
        "runtime_teacher_candidate_authoring_refs_only";
    internal const string FallbackRequiredErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_AUTHORING_FALLBACK_REQUIRED";

    private readonly IDesignConversionImprovementCandidateStrategy _inner =
        inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly IDesignConversionLocalArtifactDb _artifactDb =
        artifactDb ?? throw new ArgumentNullException(nameof(artifactDb));

    public Task<OperationResult<DesignConversionImprovementCandidate>>
        CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        => _inner.CreateNextCandidateAsync(request, cancellationToken);

    public async Task<OperationResult<DesignConversionTeacherAuthoredCandidateResult>>
        AuthorCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            DesignConversionDpoTeacherCallReservation reservation,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(reservation);
        cancellationToken.ThrowIfCancellationRequested();

        var runtime = DesignConversionDpoTeacherAttestationAssociation.Resolve(_artifactDb);
        if (runtime is null ||
            (reservation.ConfiguredMaximumCalls.HasValue &&
             reservation.InitialObservedCallCount >= reservation.ConfiguredMaximumCalls.Value))
        {
            return OperationResult<DesignConversionTeacherAuthoredCandidateResult>.Fail(
                "Runtime teacher authoring is unavailable; deterministic candidate fallback is required.",
                FallbackRequiredErrorCode);
        }

        return await runtime
            .AuthorCandidateAsync(request, reservation, cancellationToken)
            .ConfigureAwait(false);
    }
}

internal sealed partial class DesignConversionDpoTeacherAttestationRuntime
{
    internal const string AuthoringSchema =
        "design_conversion_dpo_teacher_authoring.v1";
    internal const int MaximumAuthoringPromptUtf8Bytes = 65_536;
    internal const int MaximumAuthoringOutputUtf8Bytes = 262_144;
    internal const string AuthoringOutputInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_AUTHORING_OUTPUT_INVALID";
    internal const string AuthoringCandidateNoOpErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_AUTHORING_CANDIDATE_NO_OP";
    private const string AuthoringOutputHashDomain =
        "xiigen:design-conversion-dpo-teacher-authoring-output:v1\0";
    private const string IncumbentBindingDomain =
        "xiigen:design-conversion-dpo-teacher-incumbent-binding:v1\0";
    private const string GeneratedBindingDomain =
        "xiigen:design-conversion-dpo-teacher-generated-binding:v1\0";
    private const string GeneratedCandidateIdentityDomain =
        "xiigen:design-conversion-dpo-teacher-generated-candidate:v1\0";

    internal async Task<OperationResult<DesignConversionTeacherAuthoredCandidateResult>>
        AuthorCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            DesignConversionDpoTeacherCallReservation reservation,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request is null || reservation is null ||
            request.CompletedAttemptNumber <= 0 ||
            request.CurrentRequest is null ||
            request.CurrentEvaluation is null)
        {
            return AuthoringFailure(InvalidPairErrorCode);
        }

        var repositoryOwnedDpoContextValidation =
            request.RepositoryOwnedDpoTransactionContext?.Validate();
        if (repositoryOwnedDpoContextValidation is { IsSuccess: false })
        {
            // WHY: enabled repository-owned DPO reuse is an authority boundary,
            // not an advisory hint; invalid transaction identity must stop here
            // before availability checks, prompt construction, or teacher calls.
            return OperationResult<DesignConversionTeacherAuthoredCandidateResult>
                .Fail(
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .InvalidSafeMessage,
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .InvalidErrorCode);
        }

        bool available;
        try
        {
            available = _isAvailable();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return AuthoringFailure(RuntimeUnavailableErrorCode);
        }

        if (Volatile.Read(ref _disposed) != 0 || !available)
            return AuthoringFailure(RuntimeUnavailableErrorCode);

        var incumbentBinding = BuildIncumbentBinding(request.CurrentRequest);
        var incumbentRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-dpo-teacher-incumbent",
            incumbentBinding);
        var prompt = BuildAuthoringPrompt(request, incumbentRef);
        if (!prompt.IsSuccess || prompt.Value is null)
            return AuthoringFailure(prompt.ErrorCode ?? AuthoringOutputInvalidErrorCode);

        var profileAdmissionError = GetDeclaredAuthoringProfileAdmissionError(); // Use the same exact legacy passport admission as attestation before inference or permit spend.
        if (profileAdmissionError is not null) return AuthoringFailure(profileAdmissionError); // A startup-selected CPU client cannot silently acquire DesignConversion authoring compatibility.

        OperationResult<TrainingTeacherInferenceResult> inference;
        try
        {
            inference = await _client
                .InferAsync(prompt.Value, reservation, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return AuthoringFailure(RuntimeUnavailableErrorCode);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (inference is null || !inference.IsSuccess || inference.Value is null)
            return AuthoringFailure(SanitizeErrorCode(inference?.ErrorCode));
        if (!reservation.CallIssued)
        {
            return AuthoringFailure(
                DesignConversionDpoTeacherCallReservation.InvalidStateErrorCode);
        }
        var inferenceProfile = inference.Value.InferenceProfile;
        if (!LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(
                inferenceProfile))
        {
            return AuthoringFailure(InferenceProfileInvalidErrorCode);
        }
        if (!IsValidProvenance(inference.Value.TrainingTeacherProvenance))
            return AuthoringFailure(ProvenanceInvalidErrorCode);

        return ParseAuthoringOutput(
            inference.Value.Output,
            request,
            incumbentRef,
            incumbentBinding,
            inference.Value.TrainingTeacherProvenance,
            inferenceProfile!);
    }

    internal static string BuildIncumbentBinding(
        DesignConversionInternalEvaluateRequest request)
        => request is null
            ? string.Empty
            : HashLengthPrefixed(
                IncumbentBindingDomain,
                request.RequestId,
                request.Html,
                request.Css);

    internal static string BuildIncumbentBinding(
        SubmittedDesignConversionRequest request)
        => request is null
            ? string.Empty
            : HashLengthPrefixed(
                IncumbentBindingDomain,
                request.RequestId,
                request.Html,
                request.Css);

    internal static string BuildGeneratedBinding(string html, string css)
        => HashLengthPrefixed(GeneratedBindingDomain, html, css);

    internal static string BuildGeneratedRefFromCandidateId(string candidateId)
    {
        const string prefix = "teacher-authored:";
        return !string.IsNullOrWhiteSpace(candidateId) &&
               candidateId.StartsWith(prefix, StringComparison.Ordinal)
            ? DesignConversionDpoLeakageGuard.HashRef(
                "dcm-dpo-teacher-generated",
                candidateId[prefix.Length..])
            : string.Empty;
    }

    internal static string BuildGeneratedRequestIdFromCandidateId(string candidateId)
    {
        const string prefix = "teacher-authored:";
        return !string.IsNullOrWhiteSpace(candidateId) &&
               candidateId.StartsWith(prefix, StringComparison.Ordinal)
            ? "teacher-authored-request:" + candidateId[prefix.Length..]
            : string.Empty;
    }

    internal static bool HasCanonicalAuthoredProvenance(
        TrainingTeacherProvenance? provenance)
        => provenance is not null &&
           provenance.SourceKind == TrainingTeacherSourceKind.ExternalTeacherOutput &&
           provenance.Family == OpenWeightTeacherFamily.Qwen &&
           provenance.AccessMode == TrainingTeacherAccessMode.LocalHostedOpenWeight &&
           provenance.HostingProvider == TrainingTeacherHostingProvider.Ollama &&
           provenance.ModelLicense == TrainingTeacherModelLicense.Apache20 &&
           !provenance.ProviderTermsChecked &&
           !provenance.ProviderTermsAllowTraining &&
           !provenance.ClosedProprietaryModel &&
           provenance.TeacherOutputUsedForTraining &&
           provenance.InvocationCount == 1 &&
           ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(
               provenance.ModelId) &&
           ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaManifestDigest(
               provenance.ModelVersion) &&
           LegalTrainingTeacherAdmissionAuthority
               .IsCanonicalLocalOllamaArtifactIdentity(
                   provenance.ModelVersion,
                   provenance.ModelArtifactDigestKind,
                   provenance.ModelArtifactDigest) &&
           !string.IsNullOrWhiteSpace(provenance.RequestId) &&
           provenance.RequestId.Length <= MaximumProvenanceIdentityLength &&
           !provenance.RequestId.Any(char.IsControl);

    internal static OperationResult<DesignConversionTeacherAuthoredCandidateResult>
        ParseAuthoringOutputForTesting(
            string output,
            DesignConversionCandidateUpdateRequest request,
            TrainingTeacherProvenance provenance)
    {
        var incumbentBinding = BuildIncumbentBinding(request.CurrentRequest);
        var incumbentRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-dpo-teacher-incumbent",
            incumbentBinding);
        return ParseAuthoringOutput(
            output,
            request,
            incumbentRef,
            incumbentBinding,
            provenance,
            LocalQwenOllamaInferenceProfileAuthority.Authoring);
    }

    private static OperationResult<string> BuildAuthoringPrompt(
        DesignConversionCandidateUpdateRequest request,
        string incumbentRef)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
               {
                   Indented = false,
                   MaxDepth = MaximumJsonDepth,
                   SkipValidation = false
               }))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", AuthoringSchema);
            writer.WriteString("task", "author_one_design_candidate");
            writer.WriteNumber(
                "completed_attempt_number",
                request.CompletedAttemptNumber);
            writer.WriteNumber(
                "candidate_attempt_number",
                request.CompletedAttemptNumber + 1);
            writer.WriteString("incumbent_ref", incumbentRef);
            writer.WriteString("incumbent_html", request.CurrentRequest.Html);
            writer.WriteString("incumbent_css", request.CurrentRequest.Css);
            writer.WriteString(
                "measured_visual_score",
                request.CurrentEvaluation.VisualQaBoundaryTrace.VisualDecision.QualityScore
                    .ToString("R", CultureInfo.InvariantCulture));
            writer.WriteStartObject("required_output");
            writer.WriteString("schema", AuthoringSchema);
            writer.WriteString("incumbent_ref", incumbentRef);
            writer.WriteString("candidate_html", "complete candidate HTML");
            writer.WriteString("candidate_css", "complete candidate CSS");
            writer.WriteEndObject();
            writer.WriteBoolean("return_json_only", true);
            writer.WriteBoolean("unknown_fields_forbidden", true);
            writer.WriteEndObject();
        }

        if (stream.Length > MaximumAuthoringPromptUtf8Bytes)
        {
            return OperationResult<string>.Fail(
                FailureSafeMessage,
                AuthoringOutputInvalidErrorCode);
        }

        return OperationResult<string>.Ok(
            Encoding.UTF8.GetString(stream.ToArray()));
    }

    private static OperationResult<DesignConversionTeacherAuthoredCandidateResult>
        ParseAuthoringOutput(
            string output,
            DesignConversionCandidateUpdateRequest request,
            string expectedIncumbentRef,
            string incumbentBinding,
            TrainingTeacherProvenance provenance,
            TrainingTeacherInferenceProfileEvidence inferenceProfile)
    {
        if (string.IsNullOrWhiteSpace(output) ||
            Encoding.UTF8.GetByteCount(output) > MaximumAuthoringOutputUtf8Bytes ||
            output.Contains("```", StringComparison.Ordinal))
        {
            return AuthoringFailure(AuthoringOutputInvalidErrorCode);
        }

        string? schema = null;
        string? incumbentRef = null;
        string? candidateHtml = null;
        string? candidateCss = null;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        try
        {
            var bytes = Encoding.UTF8.GetBytes(output);
            var reader = new Utf8JsonReader(bytes, new JsonReaderOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = MaximumJsonDepth
            });
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                return AuthoringFailure(AuthoringOutputInvalidErrorCode);

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    return AuthoringFailure(AuthoringOutputInvalidErrorCode);
                var property = reader.GetString();
                if (property is null || !seen.Add(property) ||
                    !reader.Read() || reader.TokenType != JsonTokenType.String)
                {
                    return AuthoringFailure(AuthoringOutputInvalidErrorCode);
                }

                var value = reader.GetString() ?? string.Empty;
                switch (property)
                {
                    case "schema": schema = value; break;
                    case "incumbent_ref": incumbentRef = value; break;
                    case "candidate_html": candidateHtml = value; break;
                    case "candidate_css": candidateCss = value; break;
                    default: return AuthoringFailure(AuthoringOutputInvalidErrorCode);
                }
            }

            if (reader.TokenType != JsonTokenType.EndObject || reader.Read())
                return AuthoringFailure(AuthoringOutputInvalidErrorCode);
        }
        catch (JsonException)
        {
            return AuthoringFailure(AuthoringOutputInvalidErrorCode);
        }

        if (seen.Count != 4 ||
            !string.Equals(schema, AuthoringSchema, StringComparison.Ordinal) ||
            !string.Equals(incumbentRef, expectedIncumbentRef, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(candidateHtml) ||
            string.IsNullOrWhiteSpace(candidateCss) ||
            HasUnsafeControls(candidateHtml) ||
            HasUnsafeControls(candidateCss))
        {
            return AuthoringFailure(AuthoringOutputInvalidErrorCode);
        }

        if (string.Equals(candidateHtml, request.CurrentRequest.Html, StringComparison.Ordinal) &&
            string.Equals(candidateCss, request.CurrentRequest.Css, StringComparison.Ordinal))
        {
            return AuthoringFailure(AuthoringCandidateNoOpErrorCode);
        }

        var generatedBinding = BuildGeneratedBinding(candidateHtml, candidateCss);
        var serverCandidateIdentity = HashLengthPrefixed(
            GeneratedCandidateIdentityDomain,
            request.LoopId,
            request.CompletedAttemptNumber.ToString(CultureInfo.InvariantCulture),
            incumbentBinding,
            generatedBinding);
        var generatedRef = DesignConversionDpoLeakageGuard.HashRef(
            "dcm-dpo-teacher-generated",
            serverCandidateIdentity);
        var outputHash = HashLengthPrefixed(AuthoringOutputHashDomain, output);
        var nextAttempt = request.CompletedAttemptNumber + 1;
        var generatedCandidateId = $"teacher-authored:{serverCandidateIdentity}";
        var generatedRequestId = $"teacher-authored-request:{serverCandidateIdentity}";
        var candidate = new DesignConversionImprovementCandidate(
            CandidateId: generatedCandidateId,
            RequestId: generatedRequestId,
            Html: candidateHtml,
            Css: candidateCss,
            ChangeReasons:
            [
                "A legally admitted training teacher authored one complete candidate; preference directionality is not part of authoring."
            ],
            EvidenceRefs:
            [
                DesignConversionDpoTeacherAuthoringCandidateStrategy.StrategyMarker,
                DesignConversionDpoTeacherAuthoringCandidateStrategy.CandidateAuthoringMarker,
                $"teacher-authoring:attempt={nextAttempt.ToString(CultureInfo.InvariantCulture)}",
                $"teacher-authoring:incumbent_ref={expectedIncumbentRef}",
                $"teacher-authoring:generated_ref={generatedRef}",
                $"teacher-authoring:output_hash={outputHash}",
                .. DesignConversionDpoTeacherAttestationRuntime
                    .BuildInferenceProfileEvidenceRefs(inferenceProfile)
            ],
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["candidate_strategy"] =
                    DesignConversionDpoTeacherAuthoringCandidateStrategy.StrategyMarker,
                ["runtime_teacher_authored_candidate"] = bool.TrueString,
                ["candidate_authoring_only"] = bool.TrueString,
                ["teacher_authoring_schema"] = AuthoringSchema,
                ["teacher_authoring_output_hash"] = outputHash,
                ["teacher_authoring_incumbent_ref"] = expectedIncumbentRef,
                ["teacher_authoring_generated_ref"] = generatedRef,
                ["teacher_inference_profile_id"] = inferenceProfile.ProfileId,
                ["teacher_inference_profile_configuration_sha256"] =
                    inferenceProfile.ConfigurationSha256,
                ["raw_teacher_prompt_persisted"] = bool.FalseString,
                ["raw_teacher_output_persisted"] = bool.FalseString,
                ["raw_teacher_rationale_persisted"] = bool.FalseString,
                ["deterministic_measurement_used_as_admission_veto"] = bool.TrueString,
                [DesignConversionModelAuthorityBoundary.TrainedModelAuthorityKey] = bool.FalseString,
                ["external_provider_called"] = bool.TrueString,
                ["network_call_executed"] = bool.TrueString,
                ["product_ready"] = bool.FalseString
            });
        var authoringEvidence = new DesignConversionDpoTeacherCandidateAuthoringEvidence(
            AuthoringSchema,
            expectedIncumbentRef,
            generatedRef,
            incumbentBinding,
            generatedBinding,
            generatedCandidateId,
            generatedRequestId,
            outputHash,
            provenance,
            candidate.EvidenceRefs,
            candidate.Metadata,
            nextAttempt)
        {
            InferenceProfileId = inferenceProfile.ProfileId,
            InferenceProfileConfigurationSha256 =
                inferenceProfile.ConfigurationSha256
        };
        return OperationResult<DesignConversionTeacherAuthoredCandidateResult>.Ok(
            new DesignConversionTeacherAuthoredCandidateResult(
                candidate,
                authoringEvidence,
                incumbentBinding));
    }

    private static bool HasUnsafeControls(string value)
        => value.Any(character =>
            (character < ' ' && character is not '\r' and not '\n' and not '\t') ||
            character == '\u007f' ||
            char.GetUnicodeCategory(character) ==
            System.Globalization.UnicodeCategory.Control &&
            character is not '\r' and not '\n' and not '\t');

    private static string HashLengthPrefixed(string domain, params string?[] values)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendLengthPrefixed(hash, Encoding.UTF8.GetBytes(domain));
        foreach (var value in values)
            AppendLengthPrefixed(hash, Encoding.UTF8.GetBytes(value ?? string.Empty));
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendLengthPrefixed(IncrementalHash hash, byte[] value)
    {
        Span<byte> length = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(length, value.LongLength);
        hash.AppendData(length);
        hash.AppendData(value);
    }

    private static OperationResult<DesignConversionTeacherAuthoredCandidateResult>
        AuthoringFailure(string errorCode)
        => OperationResult<DesignConversionTeacherAuthoredCandidateResult>.Fail(
            FailureSafeMessage,
            errorCode);
}
