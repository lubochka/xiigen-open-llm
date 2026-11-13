// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

internal sealed record DesignConversionDpoTeacherAttestation(
    string Schema,
    string ChosenRef,
    string RejectedRef,
    string OutputHash,
    TrainingTeacherProvenance Provenance,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> Metadata)
{
    internal string InferenceProfileId { get; init; } = string.Empty;

    internal string InferenceProfileConfigurationSha256 { get; init; } =
        string.Empty;
}

internal sealed class DesignConversionDpoTeacherCallReservation
    : ITrainingTeacherCallPermit
{
    internal const string InvalidStateErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_CALL_RESERVATION_INVALID";
    internal const string ExhaustedErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_CALL_RESERVATION_EXHAUSTED";
    private const string FailureSafeMessage =
        "DesignConversion DPO teacher call reservation was invalid or exhausted.";

    private readonly int? _configuredMaximumCalls;
    private readonly int _observedCallCount;
    private int _callIssued;

    private DesignConversionDpoTeacherCallReservation(
        int? configuredMaximumCalls,
        int observedCallCount)
    {
        _configuredMaximumCalls = configuredMaximumCalls;
        _observedCallCount = observedCallCount;
    }

    internal int? ConfiguredMaximumCalls => _configuredMaximumCalls;

    internal int InitialObservedCallCount => _observedCallCount;

    internal bool CallIssued => Volatile.Read(ref _callIssued) != 0;

    internal int ActualObservedCallCount =>
        _observedCallCount + (CallIssued ? 1 : 0);

    internal static OperationResult<DesignConversionDpoTeacherCallReservation>
        Create(int? configuredMaximumCalls, int observedCallCount)
    {
        if (configuredMaximumCalls is < 0 ||
            observedCallCount < 0 ||
            observedCallCount == int.MaxValue ||
            (configuredMaximumCalls.HasValue &&
             observedCallCount > configuredMaximumCalls.Value))
        {
            return OperationResult<DesignConversionDpoTeacherCallReservation>.Fail(
                FailureSafeMessage,
                InvalidStateErrorCode);
        }

        return OperationResult<DesignConversionDpoTeacherCallReservation>.Ok(
            new DesignConversionDpoTeacherCallReservation(
                configuredMaximumCalls,
                observedCallCount));
    }

    internal OperationResult<int> TryIssue()
    {
        if (_configuredMaximumCalls.HasValue &&
            _observedCallCount >= _configuredMaximumCalls.Value)
        {
            return OperationResult<int>.Fail(
                FailureSafeMessage,
                ExhaustedErrorCode);
        }

        if (Interlocked.CompareExchange(ref _callIssued, 1, 0) != 0)
        {
            return OperationResult<int>.Fail(
                FailureSafeMessage,
                InvalidStateErrorCode);
        }

        return OperationResult<int>.Ok(_observedCallCount + 1);
    }

    bool ITrainingTeacherCallPermit.CallIssued => CallIssued;

    OperationResult<int> ITrainingTeacherCallPermit.TryIssue() => TryIssue();
}

internal sealed record DesignConversionTeacherCallUsageReceipt
{
    private DesignConversionTeacherCallUsageReceipt(
        string rootJobId,
        string rootRequestId,
        int? configuredMaximumCalls,
        int observedCallCount,
        bool isValid)
    {
        RootJobId = rootJobId;
        RootRequestId = rootRequestId;
        ConfiguredMaximumCalls = configuredMaximumCalls;
        ObservedCallCount = observedCallCount;
        IsValid = isValid;
    }

    internal string RootJobId { get; }
    internal string RootRequestId { get; }
    internal int? ConfiguredMaximumCalls { get; }
    internal int ObservedCallCount { get; }
    internal bool IsValid { get; }

    internal static DesignConversionTeacherCallUsageReceipt Create(
        string rootJobId,
        string rootRequestId,
        int? configuredMaximumCalls,
        int observedCallCount,
        bool isValid)
        => new(
            rootJobId,
            rootRequestId,
            configuredMaximumCalls,
            observedCallCount,
            isValid &&
            !string.IsNullOrWhiteSpace(rootJobId) &&
            !string.IsNullOrWhiteSpace(rootRequestId) &&
            configuredMaximumCalls is not < 0 &&
            observedCallCount >= 0 &&
            (!configuredMaximumCalls.HasValue ||
             observedCallCount <= configuredMaximumCalls.Value));
}

internal static class DesignConversionTeacherCallUsageReceiptAssociation
{
    private static readonly ConditionalWeakTable<
        DesignConversionImprovementLoopResult,
        DesignConversionTeacherCallUsageReceipt> LoopReceipts = new();
    private static readonly object LoopReceiptGate = new();
    private static readonly ConditionalWeakTable<
        DesignConversionInternalEvaluationResult,
        DesignConversionTeacherCallUsageReceipt> EvaluationReceipts = new();

    internal static void Associate(
        DesignConversionImprovementLoopResult result,
        DesignConversionTeacherCallUsageReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(receipt);
        lock (LoopReceiptGate)
            LoopReceipts.Add(result, receipt);
    }

    internal static void Associate(
        DesignConversionInternalEvaluationResult evaluation,
        DesignConversionTeacherCallUsageReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(evaluation);
        ArgumentNullException.ThrowIfNull(receipt);
        EvaluationReceipts.Add(evaluation, receipt);
    }

    internal static bool TryResolve(
        DesignConversionImprovementLoopResult result,
        out DesignConversionTeacherCallUsageReceipt? receipt)
    {
        lock (LoopReceiptGate)
            return LoopReceipts.TryGetValue(result, out receipt);
    }

    internal static DesignConversionImprovementLoopResult CarryForward(
        DesignConversionImprovementLoopResult source,
        DesignConversionImprovementLoopResult target)
    {
        Propagate(source, target);
        return target;
    }

    internal static bool Propagate(
        DesignConversionImprovementLoopResult source,
        DesignConversionImprovementLoopResult target)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        if (ReferenceEquals(source, target))
            return TryResolve(source, out _);

        lock (LoopReceiptGate)
        {
            if (!LoopReceipts.TryGetValue(source, out var receipt) ||
                receipt is null)
            {
                return false;
            }

            if (LoopReceipts.TryGetValue(target, out var existing))
                return ReferenceEquals(existing, receipt);

            LoopReceipts.Add(target, receipt);
            return true;
        }
    }

    internal static bool TryResolve(
        DesignConversionInternalEvaluationResult evaluation,
        out DesignConversionTeacherCallUsageReceipt? receipt)
        => EvaluationReceipts.TryGetValue(evaluation, out receipt);
}

internal sealed class DesignConversionTeacherCallUsageAccumulator
{
    private readonly int _initialObservedCallCount;
    private int _issuedCallCount;

    internal DesignConversionTeacherCallUsageAccumulator(
        int? configuredMaximumCalls,
        int initialObservedCallCount,
        bool initialStateValid)
    {
        ConfiguredMaximumCalls = configuredMaximumCalls;
        InitialStateValid = initialStateValid;
        _initialObservedCallCount = Math.Max(0, initialObservedCallCount);
    }

    internal int? ConfiguredMaximumCalls { get; }
    internal bool InitialStateValid { get; }
    internal int InitialObservedCallCount => _initialObservedCallCount;
    internal int ActualObservedCallCount =>
        _initialObservedCallCount + Volatile.Read(ref _issuedCallCount);

    internal void ObserveIssuedCall()
        => Interlocked.CompareExchange(ref _issuedCallCount, 1, 0);
}

internal sealed partial class DesignConversionDpoTeacherAttestationRuntime : IDisposable
{
    internal const string Schema = "design_conversion_dpo_teacher_attestation.v1";
    internal const string FailureSafeMessage =
        "DesignConversion DPO teacher attestation was unavailable or rejected.";
    internal const string RuntimeUnavailableErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_RUNTIME_UNAVAILABLE";
    internal const string InvalidPairErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_PAIR_INVALID";
    internal const string OutputInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_OUTPUT_INVALID";
    internal const string PreferenceRejectedErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_PREFERENCE_REJECTED";
    internal const string ProvenanceInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_PROVENANCE_INVALID";
    internal const string InferenceProfileInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_TEACHER_INFERENCE_PROFILE_INVALID";
    internal const int MaximumOutputUtf8Bytes = 8 * 1024;
    internal const int MaximumPromptUtf8Bytes = 12 * 1024;
    internal const int MaximumJsonDepth = 8;
    internal const int MaximumProvenanceIdentityLength = 256;
    private const int MaximumEvidenceRefs = 32;
    private const int MaximumDefectCategories = 16;
    private const string OutputHashDomain =
        "xiigen:design-conversion-dpo-teacher-attestation-output:v1\0";

    private readonly ITrainingTeacherInferenceClient _client;
    private readonly string _expectedModelId;
    private readonly string _expectedOllamaManifestDigest;
    private readonly string _expectedOllamaModelLayerDigest;
    private readonly Func<bool> _isAvailable;
    private int _disposed;

    internal DesignConversionDpoTeacherAttestationRuntime(
        ITrainingTeacherInferenceClient client,
        string expectedModelId,
        string expectedOllamaManifestDigest,
        string expectedOllamaModelLayerDigest,
        Func<bool>? isAvailable = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        if (!ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(
                expectedModelId))
        {
            throw new ArgumentException(
                "A canonical expected local teacher model identity is required.",
                nameof(expectedModelId));
        }
        if (!ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaManifestDigest(
                    expectedOllamaManifestDigest))
        {
            throw new ArgumentException(
                "A canonical expected local teacher manifest digest is required.",
                nameof(expectedOllamaManifestDigest));
        }
        if (!ContinuousLearningTp1DpoConfiguration
                .IsCanonicalOllamaModelLayerDigest(
                    expectedOllamaModelLayerDigest))
        {
            throw new ArgumentException(
                "A canonical expected local teacher model-layer digest is required.",
                nameof(expectedOllamaModelLayerDigest));
        }
        _expectedModelId = expectedModelId;
        _expectedOllamaManifestDigest = expectedOllamaManifestDigest;
        _expectedOllamaModelLayerDigest = expectedOllamaModelLayerDigest;
        _isAvailable = isAvailable ?? (() => true);
    }

    internal async Task<OperationResult<DesignConversionDpoTeacherAttestation>>
        AttestAsync(
            DesignConversionInternalEvaluationResult chosenEvaluation,
            DesignConversionInternalEvaluationResult rejectedEvaluation,
            DesignConversionDpoTeacherCallReservation reservation,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (reservation is null)
            return Failure(DesignConversionDpoTeacherCallReservation.InvalidStateErrorCode);

        bool available;
        try
        {
            available = _isAvailable();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure(RuntimeUnavailableErrorCode);
        }
        if (Volatile.Read(ref _disposed) != 0 || !available)
            return Failure(RuntimeUnavailableErrorCode);

        var request = BuildRequest(chosenEvaluation, rejectedEvaluation);
        if (!request.IsSuccess || request.Value is null)
            return Failure(request.ErrorCode ?? InvalidPairErrorCode);

        var profileAdmissionError = GetDeclaredAuthoringProfileAdmissionError(); // Admit the declared passport before inference can consume this reservation.
        if (profileAdmissionError is not null) return Failure(profileAdmissionError); // DesignConversion has no CPU-profile authoring compatibility grant.

        OperationResult<TrainingTeacherInferenceResult> inference;
        try
        {
            inference = await _client
                .InferAsync(request.Value.Prompt, reservation, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure(RuntimeUnavailableErrorCode);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (inference is null || !inference.IsSuccess || inference.Value is null)
            return Failure(SanitizeErrorCode(inference?.ErrorCode));
        if (!reservation.CallIssued)
        {
            return Failure(
                DesignConversionDpoTeacherCallReservation.InvalidStateErrorCode);
        }
        if (!LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(
                inference.Value.InferenceProfile))
        {
            return Failure(InferenceProfileInvalidErrorCode);
        }
        if (!IsValidProvenance(inference.Value.TrainingTeacherProvenance))
            return Failure(ProvenanceInvalidErrorCode);

        var parsed = ParseOutput(inference.Value.Output, request.Value);
        if (!parsed.IsSuccess || parsed.Value is null)
            return parsed;

        cancellationToken.ThrowIfCancellationRequested();
        return OperationResult<DesignConversionDpoTeacherAttestation>.Ok(
            parsed.Value with
            {
                Provenance = inference.Value.TrainingTeacherProvenance,
                InferenceProfileId = inference.Value.InferenceProfile!.ProfileId,
                InferenceProfileConfigurationSha256 =
                    inference.Value.InferenceProfile.ConfigurationSha256,
                EvidenceRefs = parsed.Value.EvidenceRefs
                    .Concat(BuildInferenceProfileEvidenceRefs(
                        inference.Value.InferenceProfile))
                    .ToArray(),
                Metadata = WithInferenceProfileMetadata(
                    parsed.Value.Metadata,
                    inference.Value.InferenceProfile)
            });
    }

    private string? GetDeclaredAuthoringProfileAdmissionError() // Shared pre-effect admission for both legacy-only DesignConversion authoring entrances.
    { // A null passport preserves existing legacy adapters; response evidence remains independently checked after inference.
        try // Read untrusted adapter metadata under the runtime's existing nonfatal failure policy.
        { // The process-owned local client's immutable passport is read once per attempted call.
            var declaredProfile = _client.AuthoringProfile; // Avoid a changing getter producing inconsistent admission decisions.
            return declaredProfile is null || LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(declaredProfile) ? null : InferenceProfileInvalidErrorCode; // Exact legacy compatibility only: CPU, evaluation, unknown and forged passports refuse before any permit spend.
        } // Successful admission leaves the existing request, response and provenance checks unchanged.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Preserve fatal exception graphs while sanitizing ordinary adapter failures.
        { // A failing passport getter is unavailable and cannot reach inference.
            return RuntimeUnavailableErrorCode; // Use the existing runtime-safe failure rather than exposing adapter details.
        } // Both callers retain their established result types and error sanitization.
    } // This guard grants no new profile, source rights, training authority or evaluation compatibility.

    internal static OperationResult<string> BuildCanonicalPromptForTesting(
        DesignConversionInternalEvaluationResult chosenEvaluation,
        DesignConversionInternalEvaluationResult rejectedEvaluation)
    {
        var request = BuildRequest(chosenEvaluation, rejectedEvaluation);
        return request.IsSuccess && request.Value is not null
            ? OperationResult<string>.Ok(request.Value.Prompt)
            : OperationResult<string>.Fail(
                FailureSafeMessage,
                request.ErrorCode ?? InvalidPairErrorCode);
    }

    internal static string BuildOutputRef(DesignConversionImprovementOutput output)
        => DesignConversionDpoLeakageGuard.HashRef(
            "dcm-dpo-teacher-output",
            string.Join(
                "|",
                output?.OutputId,
                output?.CandidateId,
                output?.Verdict?.VerdictId,
                output?.Verdict?.Score.ToString(
                    "R",
                    CultureInfo.InvariantCulture)));

    internal static OperationResult<DesignConversionDpoTeacherAttestation>
        ParseOutputForTesting(
            string output,
            DesignConversionInternalEvaluationResult chosenEvaluation,
            DesignConversionInternalEvaluationResult rejectedEvaluation)
    {
        var request = BuildRequest(chosenEvaluation, rejectedEvaluation);
        return request.IsSuccess && request.Value is not null
            ? ParseOutput(output, request.Value)
            : Failure(request.ErrorCode ?? InvalidPairErrorCode);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            _client.Dispose();
    }

    private static OperationResult<AttestationRequest> BuildRequest(
        DesignConversionInternalEvaluationResult chosenEvaluation,
        DesignConversionInternalEvaluationResult rejectedEvaluation)
    {
        if (chosenEvaluation is null || rejectedEvaluation is null)
        {
            return OperationResult<AttestationRequest>.Fail(
                FailureSafeMessage,
                InvalidPairErrorCode);
        }

        var chosen = chosenEvaluation.RuntimeFlowResult?.Output;
        var rejected = rejectedEvaluation.RuntimeFlowResult?.Output;
        if (chosen is null || rejected is null ||
            string.IsNullOrWhiteSpace(chosen.OutputId) ||
            string.IsNullOrWhiteSpace(rejected.OutputId) ||
            string.Equals(chosen.OutputId, rejected.OutputId, StringComparison.Ordinal) ||
            !double.IsFinite(chosen.Verdict.Score) ||
            !double.IsFinite(rejected.Verdict.Score) ||
            chosen.Verdict.Score is < 0 or > 1 ||
            rejected.Verdict.Score is < 0 or > 1 ||
            chosen.Verdict.Score <= rejected.Verdict.Score)
        {
            return OperationResult<AttestationRequest>.Fail(
                FailureSafeMessage,
                InvalidPairErrorCode);
        }

        var chosenRef = BuildOutputRef(chosen);
        var rejectedRef = BuildOutputRef(rejected);
        var defectCategories = rejected.Verdict.Defects
            .Concat(rejectedEvaluation.ComparisonArtifacts.SelectMany(value => value.Verdict.Defects))
            .Concat(rejectedEvaluation.VisualJudgeResults.SelectMany(value => value.Verdict.Defects))
            .Select(value => value.Category)
            .Where(DesignConversionNegativeReasonTaxonomy.IsAllowed)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Take(MaximumDefectCategories)
            .ToArray();
        var evidenceRefs = rejected.EvidenceRefs
            .Concat(rejected.Verdict.EvidenceRefs)
            .Concat(rejectedEvaluation.EvidenceRefs)
            .Concat(rejectedEvaluation.ComparisonArtifacts.Select(value => value.ArtifactId))
            .Concat(rejectedEvaluation.VisualJudgeResults.Select(value => value.JudgeResultId))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => DesignConversionDpoLeakageGuard.HashRef(
                "dcm-dpo-teacher-evidence",
                value))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Take(MaximumEvidenceRefs)
            .ToArray();
        if (defectCategories.Length == 0 || evidenceRefs.Length == 0)
        {
            return OperationResult<AttestationRequest>.Fail(
                FailureSafeMessage,
                InvalidPairErrorCode);
        }

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
               {
                   Indented = false,
                   MaxDepth = MaximumJsonDepth,
                   SkipValidation = false
               }))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", Schema);
            writer.WriteString("task", "attest_measured_preference_only");
            writer.WriteStartObject("chosen");
            writer.WriteString("ref", chosenRef);
            writer.WriteString(
                "measured_visual_score",
                chosen.Verdict.Score.ToString("R", CultureInfo.InvariantCulture));
            writer.WriteEndObject();
            writer.WriteStartObject("rejected");
            writer.WriteString("ref", rejectedRef);
            writer.WriteString(
                "measured_visual_score",
                rejected.Verdict.Score.ToString("R", CultureInfo.InvariantCulture));
            writer.WriteStartArray("allowed_defects");
            foreach (var category in defectCategories)
                writer.WriteStringValue(category);
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteStartArray("evidence_refs");
            foreach (var evidenceRef in evidenceRefs)
                writer.WriteStringValue(evidenceRef);
            writer.WriteEndArray();
            writer.WriteStartObject("required_output");
            writer.WriteString("schema", Schema);
            writer.WriteString("chosen_ref", chosenRef);
            writer.WriteString("rejected_ref", rejectedRef);
            writer.WriteString("preferred_ref", chosenRef);
            writer.WriteBoolean("supports_preference", true);
            writer.WriteString("evidence_refs", "subset_of_input_evidence_refs");
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        if (stream.Length > MaximumPromptUtf8Bytes)
        {
            return OperationResult<AttestationRequest>.Fail(
                FailureSafeMessage,
                InvalidPairErrorCode);
        }

        return OperationResult<AttestationRequest>.Ok(new AttestationRequest(
            Encoding.UTF8.GetString(stream.ToArray()),
            chosenRef,
            rejectedRef,
            evidenceRefs));
    }

    private static OperationResult<DesignConversionDpoTeacherAttestation>
        ParseOutput(string output, AttestationRequest request)
    {
        if (string.IsNullOrWhiteSpace(output))
            return Failure(OutputInvalidErrorCode);

        byte[] utf8;
        try
        {
            utf8 = new UTF8Encoding(false, true).GetBytes(output);
        }
        catch (EncoderFallbackException)
        {
            return Failure(OutputInvalidErrorCode);
        }
        if (utf8.Length > MaximumOutputUtf8Bytes)
            return Failure(OutputInvalidErrorCode);

        try
        {
            using var document = JsonDocument.Parse(
                utf8,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumJsonDepth
                });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return Failure(OutputInvalidErrorCode);

            var allowed = new HashSet<string>(StringComparer.Ordinal)
            {
                "schema",
                "chosen_ref",
                "rejected_ref",
                "preferred_ref",
                "supports_preference",
                "evidence_refs"
            };
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in root.EnumerateObject())
            {
                if (!allowed.Contains(property.Name) || !seen.Add(property.Name))
                    return Failure(OutputInvalidErrorCode);
            }
            if (seen.Count != allowed.Count ||
                !TryReadExactString(root, "schema", Schema) ||
                !TryReadExactString(root, "chosen_ref", request.ChosenRef) ||
                !TryReadExactString(root, "rejected_ref", request.RejectedRef) ||
                !TryReadExactString(root, "preferred_ref", request.ChosenRef) ||
                !root.TryGetProperty("supports_preference", out var supports) ||
                supports.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                return Failure(OutputInvalidErrorCode);
            }
            if (!supports.GetBoolean())
                return Failure(PreferenceRejectedErrorCode);
            if (!root.TryGetProperty("evidence_refs", out var evidence) ||
                evidence.ValueKind != JsonValueKind.Array)
            {
                return Failure(OutputInvalidErrorCode);
            }

            var acceptedEvidence = new List<string>();
            var acceptedSet = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in evidence.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String ||
                    item.GetString() is not { } value ||
                    !request.EvidenceRefs.Contains(value, StringComparer.Ordinal) ||
                    !acceptedSet.Add(value) ||
                    acceptedEvidence.Count >= MaximumEvidenceRefs)
                {
                    return Failure(OutputInvalidErrorCode);
                }
                acceptedEvidence.Add(value);
            }
            if (acceptedEvidence.Count == 0)
                return Failure(OutputInvalidErrorCode);
            if (!acceptedEvidence.SequenceEqual(
                    acceptedEvidence.Order(StringComparer.Ordinal),
                    StringComparer.Ordinal))
            {
                return Failure(OutputInvalidErrorCode);
            }

            var canonicalOutput = BuildCanonicalOutput(
                request,
                acceptedEvidence);
            if (!string.Equals(output, canonicalOutput, StringComparison.Ordinal))
                return Failure(OutputInvalidErrorCode);

            var hashBytes = SHA256.HashData(
                Encoding.UTF8.GetBytes(OutputHashDomain + canonicalOutput));
            var outputHash = "sha256:" + Convert.ToHexString(hashBytes).ToLowerInvariant();
            return OperationResult<DesignConversionDpoTeacherAttestation>.Ok(new(
                Schema,
                request.ChosenRef,
                request.RejectedRef,
                outputHash,
                Provenance: null!,
                EvidenceRefs: acceptedEvidence,
                Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["teacher_attestation_schema"] = Schema,
                    ["teacher_attestation_output_hash"] = outputHash,
                    ["teacher_attestation_supports_preference"] = bool.TrueString,
                    ["teacher_attestation_raw_output_persisted"] = bool.FalseString,
                    ["teacher_attestation_teacher_selected_preference"] = bool.FalseString
                }));
        }
        catch (JsonException)
        {
            return Failure(OutputInvalidErrorCode);
        }
    }

    private static string BuildCanonicalOutput(
        AttestationRequest request,
        IReadOnlyList<string> evidenceRefs)
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
            writer.WriteString("schema", Schema);
            writer.WriteString("chosen_ref", request.ChosenRef);
            writer.WriteString("rejected_ref", request.RejectedRef);
            writer.WriteString("preferred_ref", request.ChosenRef);
            writer.WriteBoolean("supports_preference", true);
            writer.WriteStartArray("evidence_refs");
            foreach (var evidenceRef in evidenceRefs)
                writer.WriteStringValue(evidenceRef);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    internal static IReadOnlyList<string> BuildInferenceProfileEvidenceRefs(
        TrainingTeacherInferenceProfileEvidence inferenceProfile)
        =>
        [
            $"teacher-inference-profile:id={inferenceProfile.ProfileId}",
            $"teacher-inference-profile:configuration_sha256={inferenceProfile.ConfigurationSha256}"
        ];

    internal static IReadOnlyDictionary<string, string> WithInferenceProfileMetadata(
        IReadOnlyDictionary<string, string> metadata,
        TrainingTeacherInferenceProfileEvidence inferenceProfile)
    {
        var result = new Dictionary<string, string>(metadata, StringComparer.Ordinal)
        {
            ["teacher_inference_profile_id"] = inferenceProfile.ProfileId,
            ["teacher_inference_profile_configuration_sha256"] =
                inferenceProfile.ConfigurationSha256
        };
        return result;
    }

    private static bool TryReadExactString(
        JsonElement root,
        string propertyName,
        string expected)
        => root.TryGetProperty(propertyName, out var property) &&
           property.ValueKind == JsonValueKind.String &&
           string.Equals(property.GetString(), expected, StringComparison.Ordinal);

    private bool IsValidProvenance(TrainingTeacherProvenance provenance)
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
           StringComparer.Ordinal.Equals(
               provenance.ModelId,
               _expectedModelId) &&
           ContinuousLearningTp1DpoConfiguration
               .IsCanonicalOllamaManifestDigest(provenance.ModelVersion) &&
           StringComparer.Ordinal.Equals(
               provenance.ModelVersion,
               _expectedOllamaManifestDigest) &&
           LegalTrainingTeacherAdmissionAuthority
               .IsCanonicalLocalOllamaArtifactIdentity(
                    provenance.ModelVersion,
                    provenance.ModelArtifactDigestKind,
                    provenance.ModelArtifactDigest) &&
           StringComparer.Ordinal.Equals(
               provenance.ModelArtifactDigest,
               _expectedOllamaModelLayerDigest) &&
           IsSafeProvenanceIdentity(provenance.RequestId);

    private static bool IsSafeProvenanceIdentity(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaximumProvenanceIdentityLength &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !value.Any(char.IsControl);

    private static string SanitizeErrorCode(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode) ||
            errorCode.Length > 128 ||
            errorCode.Any(value => value is not (>= 'A' and <= 'Z')
                and not (>= '0' and <= '9') and not '_'))
        {
            return RuntimeUnavailableErrorCode;
        }
        return errorCode;
    }

    private static OperationResult<DesignConversionDpoTeacherAttestation>
        Failure(string errorCode)
        => OperationResult<DesignConversionDpoTeacherAttestation>.Fail(
            FailureSafeMessage,
            errorCode);

    private sealed record AttestationRequest(
        string Prompt,
        string ChosenRef,
        string RejectedRef,
        IReadOnlyList<string> EvidenceRefs);
}

internal static class DesignConversionDpoTeacherAttestationAssociation
{
    private static readonly ConditionalWeakTable<
        IDesignConversionDpoPolicyBaseProvider,
        Registration> Registrations = new();
    private static readonly ConditionalWeakTable<
        IDesignConversionLocalArtifactDb,
        Registration> ArtifactDbRegistrations = new();
    private static readonly object Gate = new();

    internal static IDisposable Associate(
        IDesignConversionDpoPolicyBaseProvider provider,
        DesignConversionDpoTeacherAttestationRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(runtime);
        var registration = new Registration(runtime);
        lock (Gate)
            Registrations.Add(provider, registration);
        return new AssociationLease(provider, registration);
    }

    internal static TrainingCapabilityLease Associate(
        IDesignConversionDpoPolicyBaseProvider provider,
        DesignConversionDpoTeacherAttestationRuntime runtime,
        ContinuousLearningDesignConversionTeacherAssociationCapability.Issuer
            issuer)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentNullException.ThrowIfNull(issuer);
        var registration = new Registration(runtime);
        lock (Gate)
            Registrations.Add(provider, registration);
        ContinuousLearningDesignConversionTeacherAssociationCapability?
            capability = null;
        try
        {
            capability = new RegisteredDesignConversionTeacherAssociationCapability(
                issuer,
                provider,
                runtime,
                registration);
            return new TrainingCapabilityLease(
                provider,
                registration,
                capability);
        }
        catch
        {
            capability?.Revoke();
            Remove(provider, registration);
            throw;
        }
    }

    internal static DesignConversionDpoTeacherAttestationRuntime? Resolve(
        IDesignConversionLocalArtifactDb? artifactDb)
    {
        if (artifactDb is null)
            return null;
        lock (Gate)
        {
            if (ArtifactDbRegistrations.TryGetValue(
                    artifactDb,
                    out var artifactDbRegistration))
            {
                return artifactDbRegistration.Runtime;
            }
        }

        var provider = (artifactDb as JsonlDesignConversionLocalArtifactDb)
            ?.DpoPolicyBaseProvider;
        return ResolveExactProvider(provider);
    }

    internal static DesignConversionDpoTeacherAttestationRuntime?
        ResolveExactProvider(
            IDesignConversionDpoPolicyBaseProvider? provider)
    {
        if (provider is null)
            return null;
        lock (Gate)
        {
            return Registrations.TryGetValue(provider, out var registration)
                ? registration.Runtime
                : null;
        }
    }

    internal static IDisposable AssociateArtifactDbForTesting(
        IDesignConversionLocalArtifactDb artifactDb,
        DesignConversionDpoTeacherAttestationRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(artifactDb);
        ArgumentNullException.ThrowIfNull(runtime);
        var registration = new Registration(runtime);
        lock (Gate)
            ArtifactDbRegistrations.Add(artifactDb, registration);
        return new ArtifactDbAssociationLease(artifactDb, registration);
    }

    private static void Remove(
        IDesignConversionDpoPolicyBaseProvider provider,
        Registration registration)
    {
        lock (Gate)
        {
            if (Registrations.TryGetValue(provider, out var current) &&
                ReferenceEquals(current, registration))
            {
                Registrations.Remove(provider);
            }
        }
    }

    private static void RemoveArtifactDb(
        IDesignConversionLocalArtifactDb artifactDb,
        Registration registration)
    {
        lock (Gate)
        {
            if (ArtifactDbRegistrations.TryGetValue(
                    artifactDb,
                    out var current) &&
                ReferenceEquals(current, registration))
            {
                ArtifactDbRegistrations.Remove(artifactDb);
            }
        }
    }

    internal sealed record Registration(
        DesignConversionDpoTeacherAttestationRuntime Runtime);

    private sealed class
        RegisteredDesignConversionTeacherAssociationCapability
        : ContinuousLearningDesignConversionTeacherAssociationCapability
    {
        internal RegisteredDesignConversionTeacherAssociationCapability(
            ContinuousLearningDesignConversionTeacherAssociationCapability
                .Issuer issuer,
            IDesignConversionDpoPolicyBaseProvider provider,
            DesignConversionDpoTeacherAttestationRuntime runtime,
            Registration registration)
            : base(issuer, provider, runtime, registration)
        {
        }
    }

    private sealed class AssociationLease : IDisposable
    {
        private State? _state;

        internal AssociationLease(
            IDesignConversionDpoPolicyBaseProvider provider,
            Registration registration)
            => _state = new State(provider, registration);

        public void Dispose()
        {
            var state = Interlocked.Exchange(ref _state, null);
            if (state is not null)
                Remove(state.Provider, state.Registration);
        }

        private sealed record State(
            IDesignConversionDpoPolicyBaseProvider Provider,
            Registration Registration);
    }

    internal sealed class TrainingCapabilityLease : IDisposable
    {
        private State? _state;

        internal TrainingCapabilityLease(
            IDesignConversionDpoPolicyBaseProvider provider,
            Registration registration,
            ContinuousLearningDesignConversionTeacherAssociationCapability
                capability)
        {
            _state = new State(provider, registration, capability);
            Capability = capability;
        }

        internal ContinuousLearningDesignConversionTeacherAssociationCapability
            Capability { get; }

        public void Dispose()
        {
            var state = Volatile.Read(ref _state);
            if (state is null)
                return;

            lock (state)
            {
                if (!ReferenceEquals(Volatile.Read(ref _state), state))
                    return;
                state.Capability.Revoke();
                Remove(state.Provider, state.Registration);
                Interlocked.CompareExchange(ref _state, null, state);
            }
        }

        private sealed record State(
            IDesignConversionDpoPolicyBaseProvider Provider,
            Registration Registration,
            ContinuousLearningDesignConversionTeacherAssociationCapability
                Capability);
    }

    private sealed class ArtifactDbAssociationLease : IDisposable
    {
        private State? _state;

        internal ArtifactDbAssociationLease(
            IDesignConversionLocalArtifactDb artifactDb,
            Registration registration)
            => _state = new State(artifactDb, registration);

        public void Dispose()
        {
            var state = Interlocked.Exchange(ref _state, null);
            if (state is not null)
                RemoveArtifactDb(state.ArtifactDb, state.Registration);
        }

        private sealed record State(
            IDesignConversionLocalArtifactDb ArtifactDb,
            Registration Registration);
    }
}
