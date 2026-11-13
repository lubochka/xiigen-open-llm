// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// An opt-in local evaluation-only authority. It asks the separately exposed
/// Qwen/Ollama evaluation capability to compare one exact generated/incumbent
/// pair and issues a single-consume receipt. Production composition registers
/// it only through the explicit local-evaluator opt-in.
/// </summary>
internal sealed class DesignConversionLocalIndependentPreferenceEvaluator
    : IDesignConversionIndependentPreferenceEvidenceIssuer, IDisposable
{
    internal const string RequestSchema =
        "design_conversion_local_independent_preference_request.v1";
    internal const string ResultSchema =
        "design_conversion_local_independent_preference_result.v1";
    internal const string ScorerId =
        "design_conversion_local_qwen_independent_evaluator";
    internal const string ScorerVersion = "1";
    internal const string InvalidRequestErrorCode =
        "DESIGN_CONVERSION_LOCAL_EVALUATOR_REQUEST_INVALID";
    internal const string EvaluationUnavailableErrorCode =
        "DESIGN_CONVERSION_LOCAL_EVALUATOR_UNAVAILABLE";
    internal const string EvaluationProfileInvalidErrorCode =
        "DESIGN_CONVERSION_LOCAL_EVALUATOR_PROFILE_INVALID";
    internal const string EvaluationProvenanceInvalidErrorCode =
        "DESIGN_CONVERSION_LOCAL_EVALUATOR_PROVENANCE_INVALID";
    internal const string EvaluationOutputInvalidErrorCode =
        "DESIGN_CONVERSION_LOCAL_EVALUATOR_OUTPUT_INVALID";
    internal const string DuplicateAttemptErrorCode =
        "DESIGN_CONVERSION_LOCAL_EVALUATOR_ATTEMPT_ALREADY_ISSUED";
    internal const int MaximumPromptUtf8Bytes = 65_536;
    internal const int MaximumOutputUtf8Bytes = 262_144;

    private const string FailureMessage =
        "The local independent preference evaluation did not produce admissible evidence.";
    private static readonly TimeSpan ReceiptLifetime = TimeSpan.FromMinutes(1);
    private readonly object _sync = new();
    private readonly string _expectedModelId;
    private readonly string _expectedManifestDigest;
    private readonly string _expectedModelLayerDigest;
    private readonly string _runtimeInstanceRef;
    private readonly HashSet<string> _reservedOrIssuedAttempts =
        new(StringComparer.Ordinal);
    private readonly Dictionary<string, DesignConversionIndependentPreferenceEvidence>
        _availableEvidence = new(StringComparer.Ordinal);
    private ILegalTeacherEvaluationInferenceClient? _client;

    private DesignConversionLocalIndependentPreferenceEvaluator(
        ILegalTeacherEvaluationInferenceClient client,
        string expectedModelId,
        string expectedManifestDigest,
        string expectedModelLayerDigest,
        string runtimeInstanceRef)
    {
        _client = client;
        _expectedModelId = expectedModelId;
        _expectedManifestDigest = expectedManifestDigest;
        _expectedModelLayerDigest = expectedModelLayerDigest;
        _runtimeInstanceRef = runtimeInstanceRef;
    }

    internal static OperationResult<
        DesignConversionLocalIndependentPreferenceEvaluator> Create(
            ContinuousLearningTp1DpoConfiguration configuration,
            string? runtimeInstanceRef = null)
    {
        if (configuration is null ||
            !IsExpectedModelIdentity(
                configuration.ModelId,
                configuration.ExpectedOllamaManifestDigest,
                configuration.ExpectedOllamaModelLayerDigest) ||
            (!string.IsNullOrWhiteSpace(runtimeInstanceRef) &&
             !IsSha256Ref(runtimeInstanceRef)))
        {
            return Failure<DesignConversionLocalIndependentPreferenceEvaluator>(
                InvalidRequestErrorCode);
        }

        var created = ContinuousLearningTp1EvaluationComposition.CreateClient(
            configuration);
        if (!created.IsSuccess || created.Value is null)
        {
            return OperationResult<
                DesignConversionLocalIndependentPreferenceEvaluator>.Fail(
                    FailureMessage,
                    created.ErrorCode ?? EvaluationUnavailableErrorCode);
        }

        return OperationResult<
            DesignConversionLocalIndependentPreferenceEvaluator>.Ok(
                new DesignConversionLocalIndependentPreferenceEvaluator(
                    created.Value,
                    configuration.ModelId,
                    configuration.ExpectedOllamaManifestDigest,
                    configuration.ExpectedOllamaModelLayerDigest,
                    runtimeInstanceRef ?? NewSha256Identity(
                        "design-conversion-local-evaluator-runtime")));
    }

    internal static DesignConversionLocalIndependentPreferenceEvaluator
        OwnForTesting(
            ILegalTeacherEvaluationInferenceClient client,
            string expectedModelId,
            string expectedManifestDigest,
            string expectedModelLayerDigest,
            string runtimeInstanceRef)
    {
        ArgumentNullException.ThrowIfNull(client);
        if (!IsExpectedModelIdentity(
                expectedModelId,
                expectedManifestDigest,
                expectedModelLayerDigest) ||
            !IsSha256Ref(runtimeInstanceRef))
        {
            throw new ArgumentException(
                "The local evaluator identity is invalid.",
                nameof(expectedModelId));
        }

        return new DesignConversionLocalIndependentPreferenceEvaluator(
            client,
            expectedModelId,
            expectedManifestDigest,
            expectedModelLayerDigest,
            runtimeInstanceRef);
    }

    internal async Task<OperationResult<
        DesignConversionIndependentPreferenceEvidence>> IssueAsync(
            string artifactRootBinding,
            DesignConversionTeacherAuthoredEvaluationPair pair,
            DateTimeOffset observedAtUtc,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var input = ValidateInput(artifactRootBinding, pair, observedAtUtc);
        if (!input.IsSuccess || input.Value is null)
            return input.MapError<DesignConversionIndependentPreferenceEvidence>();

        var attemptKey = input.Value.AttemptKey;
        ILegalTeacherEvaluationInferenceClient? client;
        lock (_sync)
        {
            client = _client;
            if (client is null)
                return Failure(EvaluationUnavailableErrorCode);
            if (!_reservedOrIssuedAttempts.Add(attemptKey))
                return Failure(DuplicateAttemptErrorCode);
        }

        try
        {
            var logicalRequestId = "local-independent-evaluation-request:" +
                Convert.ToHexString(RandomNumberGenerator.GetBytes(16))
                    .ToLowerInvariant();
            var replayNonce = NewSha256Identity(
                "design-conversion-local-evaluator-replay");
            var evaluationModelSha256 = "sha256:" + _expectedModelLayerDigest;
            var provisionalProvenance =
                new DesignConversionIndependentPreferenceScorerProvenance(
                    DesignConversionIndependentPreferenceEvidenceContract
                        .LocalScorerProvenanceKind,
                    ScorerId,
                    ScorerVersion,
                    logicalRequestId,
                    OutputId: string.Empty,
                    _runtimeInstanceRef)
                {
                    EvaluationModelSha256 = evaluationModelSha256,
                    InferenceRequestId = string.Empty
                };
            var scorerRequestSha256 = DesignConversionDpoGenerator
                .BuildIndependentPreferenceScorerRequestSha256(
                    pair,
                    artifactRootBinding,
                    provisionalProvenance,
                    DesignConversionIndependentPreferenceEvidenceContract
                        .EvaluationOnlyProfileId,
                    DesignConversionIndependentPreferenceEvidenceContract
                        .EvaluationOnlyProfileVersion,
                    DesignConversionIndependentPreferenceEvidenceContract
                        .EvaluationOnlyProfileConfigurationSha256,
                    evaluationModelSha256);
            var prompt = BuildPrompt(
                pair,
                artifactRootBinding,
                scorerRequestSha256);
            if (!prompt.IsSuccess || prompt.Value is null)
                return prompt.MapError<DesignConversionIndependentPreferenceEvidence>();

            var permit = new SingleEvaluationCallPermit();
            OperationResult<TeacherEvaluationInferenceResult> evaluated;
            try
            {
                evaluated = await client.EvaluateAsync(
                        prompt.Value,
                        permit,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return Failure(EvaluationUnavailableErrorCode);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!evaluated.IsSuccess || evaluated.Value is null ||
                !permit.CallIssued || permit.ViolationDetected)
            {
                return Failure(
                    IsSafeErrorCode(evaluated.ErrorCode)
                        ? evaluated.ErrorCode!
                        : EvaluationUnavailableErrorCode);
            }

            var result = evaluated.Value;
            if (result.InferenceProfile !=
                    LocalQwenOllamaInferenceProfileAuthority.Evaluation ||
                result.InferenceProfile ==
                    LocalQwenOllamaInferenceProfileAuthority.Authoring)
            {
                return Failure(EvaluationProfileInvalidErrorCode);
            }

            if (!HasExpectedEvaluationProvenance(
                    result.TrainingTeacherProvenance,
                    pair.AuthoringEvidence))
            {
                return Failure(EvaluationProvenanceInvalidErrorCode);
            }

            var rawOutput = result.Output;
            if (string.IsNullOrWhiteSpace(rawOutput))
                return Failure(EvaluationOutputInvalidErrorCode);
            var outputHashLowerHex = Sha256LowerHex(rawOutput);
            if (string.Equals(
                    outputHashLowerHex,
                    pair.AuthoringEvidence.OutputHash,
                    StringComparison.Ordinal))
            {
                return Failure(EvaluationProvenanceInvalidErrorCode);
            }

            var parsed = ParseResult(
                rawOutput,
                scorerRequestSha256,
                input.Value.GeneratedEvaluationIdentity,
                input.Value.IncumbentEvaluationIdentity);
            if (!parsed.IsSuccess || parsed.Value is null)
                return parsed.MapError<DesignConversionIndependentPreferenceEvidence>();

            var scorerOutputSha256 =
                DesignConversionDpoGenerator.Sha256Ref(rawOutput);
            var scorerOutputId = DesignConversionDpoGenerator
                .BuildIndependentPreferenceScorerOutputId(
                    scorerOutputSha256,
                    logicalRequestId,
                    result.TrainingTeacherProvenance.RequestId,
                    _runtimeInstanceRef);
            var provenance = provisionalProvenance with
            {
                OutputId = scorerOutputId,
                InferenceRequestId = result.TrainingTeacherProvenance.RequestId
            };
            var receipt = new DesignConversionIndependentPreferenceEvidence(
                DesignConversionIndependentPreferenceEvidenceContract.Schema,
                pair.AttemptNumber,
                pair.AuthoringEvidence.GeneratedRef,
                pair.AuthoringEvidence.IncumbentRef,
                pair.AuthoringEvidence.GeneratedBinding,
                pair.AuthoringEvidence.IncumbentBinding,
                input.Value.GeneratedEvaluationIdentity,
                input.Value.IncumbentEvaluationIdentity,
                parsed.Value,
                scorerRequestSha256,
                scorerOutputSha256,
                DesignConversionIndependentPreferenceEvidenceContract
                    .EvaluationOnlyProfileId,
                DesignConversionIndependentPreferenceEvidenceContract
                    .EvaluationOnlyProfileVersion,
                DesignConversionIndependentPreferenceEvidenceContract
                    .EvaluationOnlyProfileConfigurationSha256,
                evaluationModelSha256,
                artifactRootBinding,
                provenance,
                observedAtUtc.ToUniversalTime(),
                observedAtUtc.ToUniversalTime().Add(ReceiptLifetime),
                replayNonce,
                ReceiptSha256: string.Empty);
            receipt = receipt with
            {
                ReceiptSha256 = DesignConversionDpoGenerator
                    .BuildIndependentPreferenceReceiptSha256(receipt)
            };
            receipt = receipt with
            {
                IssuerAuthorityCapability = new IssuedAuthorityCapability(
                    receipt.ReceiptSha256,
                    DesignConversionDpoGenerator
                        .BuildIndependentPreferenceScorerOutputCommitmentSha256(
                            receipt))
            };

            lock (_sync)
            {
                if (_client is null)
                    return Failure(EvaluationUnavailableErrorCode);
                _availableEvidence.Add(attemptKey, receipt);
            }

            return OperationResult<
                DesignConversionIndependentPreferenceEvidence>.Ok(receipt);
        }
        finally
        {
            lock (_sync)
            {
                if (!_availableEvidence.ContainsKey(attemptKey))
                    _reservedOrIssuedAttempts.Remove(attemptKey);
            }
        }
    }

    Task<OperationResult<DesignConversionIndependentPreferenceEvidence>>
        IDesignConversionIndependentPreferenceEvidenceIssuer.IssueAsync(
            string artifactRootBinding,
            DesignConversionTeacherAuthoredEvaluationPair pair,
            DateTimeOffset observedAtUtc,
            CancellationToken cancellationToken)
        => IssueAsync(
            artifactRootBinding,
            pair,
            observedAtUtc,
            cancellationToken);

    public bool TryConsumeEvidence(
        string artifactRootBinding,
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DateTimeOffset observedAtUtc,
        out DesignConversionIndependentPreferenceEvidence evidence)
    {
        evidence = null!;
        var input = ValidateInput(artifactRootBinding, pair, observedAtUtc);
        if (!input.IsSuccess || input.Value is null)
            return false;

        lock (_sync)
        {
            if (_client is null ||
                !_availableEvidence.TryGetValue(
                    input.Value.AttemptKey,
                    out var candidate) ||
                candidate.IssuedAtUtc > observedAtUtc.ToUniversalTime() ||
                candidate.ExpiresAtUtc <= observedAtUtc.ToUniversalTime() ||
                observedAtUtc.ToUniversalTime() - candidate.IssuedAtUtc >
                    DesignConversionIndependentPreferenceEvidenceContract
                        .MaximumFreshness ||
                !string.Equals(
                    candidate.ReceiptSha256,
                    DesignConversionDpoGenerator
                        .BuildIndependentPreferenceReceiptSha256(candidate),
                    StringComparison.Ordinal))
            {
                return false;
            }

            _availableEvidence.Remove(input.Value.AttemptKey);
            evidence = candidate;
            return true;
        }
    }

    internal static bool HasIssuedAuthority(
        DesignConversionIndependentPreferenceEvidence evidence)
        => evidence.IssuerAuthorityCapability is IssuedAuthorityCapability capability &&
           capability.Matches(evidence);

    internal static bool TryConsumeIssuedAuthority(
        DesignConversionIndependentPreferenceEvidence evidence)
        => evidence.IssuerAuthorityCapability is IssuedAuthorityCapability capability &&
           capability.TryConsume(evidence);

    public void Dispose()
    {
        ILegalTeacherEvaluationInferenceClient? client;
        lock (_sync)
        {
            client = _client;
            _client = null;
            _availableEvidence.Clear();
            _reservedOrIssuedAttempts.Clear();
        }
        client?.Dispose();
    }

    private OperationResult<ValidatedInput> ValidateInput(
        string artifactRootBinding,
        DesignConversionTeacherAuthoredEvaluationPair pair,
        DateTimeOffset observedAtUtc)
    {
        if (!IsSha256Ref(artifactRootBinding) || pair is null ||
            pair.AttemptNumber <= 1 || pair.AuthoringEvidence is null ||
            pair.GeneratedEvaluation is null || pair.IncumbentEvaluation is null ||
            pair.AuthoringEvidence.AttemptNumber != pair.AttemptNumber ||
            observedAtUtc == default ||
            !string.Equals(
                pair.AuthoringEvidence.InferenceProfileId,
                LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
                StringComparison.Ordinal) ||
            string.Equals(
                pair.AuthoringEvidence.InferenceProfileId,
                LocalQwenOllamaInferenceProfileAuthority.Evaluation.ProfileId,
                StringComparison.Ordinal))
        {
            return Failure<ValidatedInput>(InvalidRequestErrorCode);
        }

        var generatedIdentity = DesignConversionDpoGenerator
            .BuildIndependentPreferenceEvaluationIdentity(
                pair.GeneratedEvaluation);
        var incumbentIdentity = DesignConversionDpoGenerator
            .BuildIndependentPreferenceEvaluationIdentity(
                pair.IncumbentEvaluation);
        if (!IsSha256Ref(generatedIdentity) ||
            !IsSha256Ref(incumbentIdentity) ||
            string.Equals(generatedIdentity, incumbentIdentity,
                StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(pair.AuthoringEvidence.GeneratedRef) ||
            string.IsNullOrWhiteSpace(pair.AuthoringEvidence.IncumbentRef) ||
            string.IsNullOrWhiteSpace(pair.AuthoringEvidence.GeneratedBinding) ||
            string.IsNullOrWhiteSpace(pair.AuthoringEvidence.IncumbentBinding))
        {
            return Failure<ValidatedInput>(InvalidRequestErrorCode);
        }

        var attemptKey = DesignConversionDpoGenerator.Sha256Ref(string.Join(
            "|",
            "design-conversion-local-independent-evaluator-attempt",
            artifactRootBinding,
            pair.AttemptNumber.ToString(CultureInfo.InvariantCulture),
            pair.AuthoringEvidence.GeneratedRef,
            pair.AuthoringEvidence.IncumbentRef,
            pair.AuthoringEvidence.GeneratedBinding,
            pair.AuthoringEvidence.IncumbentBinding,
            generatedIdentity,
            incumbentIdentity));
        return OperationResult<ValidatedInput>.Ok(
            new ValidatedInput(
                attemptKey,
                generatedIdentity,
                incumbentIdentity));
    }

    private OperationResult<string> BuildPrompt(
        DesignConversionTeacherAuthoredEvaluationPair pair,
        string artifactRootBinding,
        string scorerRequestSha256)
    {
        try
        {
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
                   {
                       Indented = false,
                       SkipValidation = false,
                       MaxDepth = 16
                   }))
            {
                writer.WriteStartObject();
                writer.WriteString("schema", RequestSchema);
                writer.WriteString(
                    "role",
                    "evaluation_only_independent_preference_authority");
                writer.WriteString(
                    "scorer_request_sha256",
                    scorerRequestSha256);
                writer.WriteString("artifact_root_binding", artifactRootBinding);
                writer.WriteNumber("attempt_number", pair.AttemptNumber);
                writer.WriteString(
                    "generated_ref",
                    pair.AuthoringEvidence.GeneratedRef);
                writer.WriteString(
                    "incumbent_ref",
                    pair.AuthoringEvidence.IncumbentRef);
                writer.WriteString(
                    "generated_binding",
                    pair.AuthoringEvidence.GeneratedBinding);
                writer.WriteString(
                    "incumbent_binding",
                    pair.AuthoringEvidence.IncumbentBinding);
                writer.WriteString(
                    "generated_evaluation_identity",
                    DesignConversionDpoGenerator
                        .BuildIndependentPreferenceEvaluationIdentity(
                            pair.GeneratedEvaluation));
                writer.WriteString(
                    "incumbent_evaluation_identity",
                    DesignConversionDpoGenerator
                        .BuildIndependentPreferenceEvaluationIdentity(
                            pair.IncumbentEvaluation));
                WriteCandidate(writer, "generated", pair.GeneratedEvaluation);
                WriteCandidate(writer, "incumbent", pair.IncumbentEvaluation);
                // Модели нельзя давать копируемый псевдоответ. Отдельный
                // список меток и человеческая инструкция требуют сравнить
                // качество двух вариантов, а не повторить текст подсказки.
                writer.WriteString(
                    "decision_instruction",
                    "Compare the rendered HTML and CSS quality of both candidates. " +
                    "Set preferred_direction to the label of the better candidate. " +
                    "Never copy this instruction or any placeholder into the answer.");
                writer.WriteStartArray("allowed_preferred_direction_values");
                writer.WriteStringValue(
                    DesignConversionIndependentPreferenceEvidenceContract
                        .GeneratedSide);
                writer.WriteStringValue(
                    DesignConversionIndependentPreferenceEvidenceContract
                        .IncumbentSide);
                writer.WriteEndArray();
                writer.WriteStartObject("required_output");
                writer.WriteString("schema", ResultSchema);
                writer.WriteString(
                    "scorer_request_sha256",
                    scorerRequestSha256);
                writer.WriteString(
                    "generated_evaluation_identity",
                    DesignConversionDpoGenerator
                        .BuildIndependentPreferenceEvaluationIdentity(
                            pair.GeneratedEvaluation));
                writer.WriteString(
                    "incumbent_evaluation_identity",
                    DesignConversionDpoGenerator
                        .BuildIndependentPreferenceEvaluationIdentity(
                            pair.IncumbentEvaluation));
                writer.WriteString(
                    "preferred_direction_property_name",
                    "preferred_direction");
                writer.WriteEndObject();
                writer.WriteBoolean("return_json_only", true);
                writer.WriteBoolean("unknown_fields_forbidden", true);
                writer.WriteBoolean("training_authoring_forbidden", true);
                writer.WriteEndObject();
            }

            if (stream.Length > MaximumPromptUtf8Bytes)
                return Failure<string>(InvalidRequestErrorCode);
            return OperationResult<string>.Ok(
                Encoding.UTF8.GetString(stream.ToArray()));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure<string>(InvalidRequestErrorCode);
        }
    }

    private static void WriteCandidate(
        Utf8JsonWriter writer,
        string propertyName,
        DesignConversionInternalEvaluationResult evaluation)
    {
        var decision = evaluation.VisualQaBoundaryTrace.VisualDecision;
        writer.WriteStartObject(propertyName);
        writer.WriteString("request_id", evaluation.SubmittedRequest.RequestId);
        writer.WriteString("html", evaluation.SubmittedRequest.Html);
        writer.WriteString("css", evaluation.SubmittedRequest.Css);
        writer.WriteNumber("measured_visual_score", decision.QualityScore);
        writer.WriteBoolean(
            "can_persist_training_candidate",
            decision.CanPersistTrainingCandidate);
        writer.WriteBoolean(
            "runtime_output_accepted",
            evaluation.RuntimeFlowResult.Output.Accepted);
        writer.WriteStartArray("evidence_refs");
        foreach (var evidenceRef in evaluation.EvidenceRefs
                     .Concat(evaluation.VisualQaBoundaryTrace.EvidenceRefs)
                     .Where(static value => !string.IsNullOrWhiteSpace(value))
                     .Distinct(StringComparer.Ordinal)
                     .Order(StringComparer.Ordinal))
        {
            writer.WriteStringValue(evidenceRef);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static OperationResult<string> ParseResult(
        string output,
        string expectedRequestSha256,
        string expectedGeneratedIdentity,
        string expectedIncumbentIdentity)
    {
        if (string.IsNullOrWhiteSpace(output) ||
            Encoding.UTF8.GetByteCount(output) > MaximumOutputUtf8Bytes ||
            output.Contains("```", StringComparison.Ordinal))
        {
            return Failure<string>(EvaluationOutputInvalidErrorCode);
        }

        try
        {
            using var document = JsonDocument.Parse(
                output,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 8
                });
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return Failure<string>(EvaluationOutputInvalidErrorCode);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            string? schema = null;
            string? requestSha256 = null;
            string? generatedIdentity = null;
            string? incumbentIdentity = null;
            string? preferredDirection = null;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!seen.Add(property.Name) ||
                    property.Value.ValueKind != JsonValueKind.String)
                {
                    return Failure<string>(EvaluationOutputInvalidErrorCode);
                }

                switch (property.Name)
                {
                    case "schema":
                        schema = property.Value.GetString();
                        break;
                    case "scorer_request_sha256":
                        requestSha256 = property.Value.GetString();
                        break;
                    case "generated_evaluation_identity":
                        generatedIdentity = property.Value.GetString();
                        break;
                    case "incumbent_evaluation_identity":
                        incumbentIdentity = property.Value.GetString();
                        break;
                    case "preferred_direction":
                        preferredDirection = property.Value.GetString();
                        break;
                    default:
                        return Failure<string>(EvaluationOutputInvalidErrorCode);
                }
            }

            if (seen.Count != 5 ||
                !string.Equals(schema, ResultSchema, StringComparison.Ordinal) ||
                !string.Equals(requestSha256, expectedRequestSha256,
                    StringComparison.Ordinal) ||
                !string.Equals(generatedIdentity, expectedGeneratedIdentity,
                    StringComparison.Ordinal) ||
                !string.Equals(incumbentIdentity, expectedIncumbentIdentity,
                    StringComparison.Ordinal) ||
                (!string.Equals(
                     preferredDirection,
                     DesignConversionIndependentPreferenceEvidenceContract
                         .GeneratedSide,
                     StringComparison.Ordinal) &&
                 !string.Equals(
                     preferredDirection,
                     DesignConversionIndependentPreferenceEvidenceContract
                         .IncumbentSide,
                     StringComparison.Ordinal)))
            {
                return Failure<string>(EvaluationOutputInvalidErrorCode);
            }

            return OperationResult<string>.Ok(preferredDirection!);
        }
        catch (JsonException)
        {
            return Failure<string>(EvaluationOutputInvalidErrorCode);
        }
    }

    private bool HasExpectedEvaluationProvenance(
        TrainingTeacherProvenance? provenance,
        DesignConversionDpoTeacherCandidateAuthoringEvidence authoring)
        => provenance is not null &&
           provenance.SourceKind == TrainingTeacherSourceKind.ExternalTeacherOutput &&
           provenance.Family == OpenWeightTeacherFamily.Qwen &&
           provenance.AccessMode == TrainingTeacherAccessMode.LocalHostedOpenWeight &&
           provenance.HostingProvider == TrainingTeacherHostingProvider.Ollama &&
           string.Equals(provenance.ModelId, _expectedModelId,
               StringComparison.Ordinal) &&
           string.Equals(provenance.ModelVersion, _expectedManifestDigest,
               StringComparison.Ordinal) &&
           provenance.ModelLicense == TrainingTeacherModelLicense.Apache20 &&
           !provenance.ProviderTermsChecked &&
           !provenance.ProviderTermsAllowTraining &&
           !provenance.ClosedProprietaryModel &&
           !provenance.TeacherOutputUsedForTraining &&
           provenance.InvocationCount == 1 &&
           string.Equals(
               provenance.ModelArtifactDigestKind,
               LegalTrainingTeacherAdmissionAuthority
                   .OllamaModelLayerSha256DigestKind,
               StringComparison.Ordinal) &&
           string.Equals(
               provenance.ModelArtifactDigest,
               _expectedModelLayerDigest,
               StringComparison.Ordinal) &&
           IsSafeIdentity(provenance.RequestId) &&
           !string.Equals(provenance.RequestId, authoring.Provenance.RequestId,
               StringComparison.Ordinal) &&
           !string.Equals(provenance.RequestId, authoring.GeneratedRequestId,
               StringComparison.Ordinal);

    private static bool IsExpectedModelIdentity(
        string? modelId,
        string? manifestDigest,
        string? modelLayerDigest)
        => ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(modelId) &&
           ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaManifestDigest(
               manifestDigest) &&
           ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelLayerDigest(
               modelLayerDigest);

    private static bool IsSafeIdentity(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= 256 &&
           !value.Any(char.IsControl);

    private static bool IsSafeErrorCode(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= 128 &&
           value.All(static character =>
               character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_');

    private static string NewSha256Identity(string domain)
        => DesignConversionDpoGenerator.Sha256Ref(
            domain + "|" +
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32))
                .ToLowerInvariant());

    private static string Sha256LowerHex(string value)
        => Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant();

    private static bool IsSha256Ref(string? value)
        => value is { Length: 71 } &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.AsSpan(7).ToArray().All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    /// <summary>
    /// Process-local, non-serializable issuer capability. The constructor and
    /// concrete type remain private to the local evaluator, so receipt fields
    /// and public hash helpers are insufficient to mint or reconstruct it.
    /// </summary>
    private sealed class IssuedAuthorityCapability(
        string receiptSha256,
        string scorerOutputCommitmentSha256)
    {
        private int _consumed;

        internal bool Matches(
            DesignConversionIndependentPreferenceEvidence evidence)
            => string.Equals(
                   receiptSha256,
                   evidence.ReceiptSha256,
                   StringComparison.Ordinal) &&
               string.Equals(
                   receiptSha256,
                   DesignConversionDpoGenerator
                       .BuildIndependentPreferenceReceiptSha256(evidence),
                   StringComparison.Ordinal) &&
               string.Equals(
                   scorerOutputCommitmentSha256,
                   DesignConversionDpoGenerator
                       .BuildIndependentPreferenceScorerOutputCommitmentSha256(
                           evidence),
                   StringComparison.Ordinal);

        internal bool TryConsume(
            DesignConversionIndependentPreferenceEvidence evidence)
            => Matches(evidence) &&
               Interlocked.CompareExchange(ref _consumed, 1, 0) == 0;
    }

    private static OperationResult<T> Failure<T>(string errorCode)
        => OperationResult<T>.Fail(FailureMessage, errorCode);

    private static OperationResult<
        DesignConversionIndependentPreferenceEvidence> Failure(string errorCode)
        => Failure<DesignConversionIndependentPreferenceEvidence>(errorCode);

    private sealed record ValidatedInput(
        string AttemptKey,
        string GeneratedEvaluationIdentity,
        string IncumbentEvaluationIdentity);

    private sealed class SingleEvaluationCallPermit : ILegalTeacherCallPermit
    {
        private int _issued;
        private int _violationDetected;

        public bool CallIssued => Volatile.Read(ref _issued) == 1;

        internal bool ViolationDetected =>
            Volatile.Read(ref _violationDetected) == 1;

        public OperationResult<int> TryIssue()
        {
            if (Interlocked.CompareExchange(ref _issued, 1, 0) == 0)
                return OperationResult<int>.Ok(1);

            Volatile.Write(ref _violationDetected, 1);
            return OperationResult<int>.Fail(
                "The local evaluation call permit was already used.",
                LegalTrainingTeacherAdmissionAuthority.CallBudgetExceededErrorCode);
        }
    }
}

/// <summary>
/// Owns one explicitly enabled local evaluator together with its exact
/// artifact-root association. The association is revoked before the transport
/// is disposed, so a stopping runtime cannot issue or consume later evidence.
/// </summary>
internal sealed class DesignConversionLocalIndependentPreferenceEvaluatorRuntime
    : IDisposable
{
    internal const string ArtifactRootAlreadyOwnedErrorCode =
        DesignConversionIndependentPreferenceEvidenceAssociation
            .ArtifactRootAlreadyAssociatedErrorCode;

    private readonly object _sync = new();
    private IDisposable? _associationLease;
    private DesignConversionLocalIndependentPreferenceEvaluator? _evaluator;

    private DesignConversionLocalIndependentPreferenceEvaluatorRuntime(
        DesignConversionLocalIndependentPreferenceEvaluator evaluator,
        IDisposable associationLease,
        string artifactRootBinding)
    {
        _evaluator = evaluator;
        _associationLease = associationLease;
        ArtifactRootBinding = artifactRootBinding;
    }

    internal string ArtifactRootBinding { get; }

    internal bool IsDisposed
    {
        get
        {
            lock (_sync)
                return _evaluator is null;
        }
    }

    internal static OperationResult<
        DesignConversionLocalIndependentPreferenceEvaluatorRuntime> Create(
            IDesignConversionLocalArtifactDb artifactDb,
            ContinuousLearningTp1DpoConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(artifactDb);
        ArgumentNullException.ThrowIfNull(configuration);
        var evaluator = DesignConversionLocalIndependentPreferenceEvaluator
            .Create(configuration);
        if (!evaluator.IsSuccess || evaluator.Value is null)
        {
            return OperationResult<
                DesignConversionLocalIndependentPreferenceEvaluatorRuntime>
                .Fail(
                    evaluator.ErrorMessage ??
                    "The local independent preference evaluator is unavailable.",
                    evaluator.ErrorCode ??
                    DesignConversionLocalIndependentPreferenceEvaluator
                        .EvaluationUnavailableErrorCode);
        }

        return Own(artifactDb, evaluator.Value);
    }

    internal static OperationResult<
        DesignConversionLocalIndependentPreferenceEvaluatorRuntime>
        OwnForTesting(
            IDesignConversionLocalArtifactDb artifactDb,
            DesignConversionLocalIndependentPreferenceEvaluator evaluator)
    {
        ArgumentNullException.ThrowIfNull(artifactDb);
        ArgumentNullException.ThrowIfNull(evaluator);
        return Own(artifactDb, evaluator);
    }

    private static OperationResult<
        DesignConversionLocalIndependentPreferenceEvaluatorRuntime> Own(
            IDesignConversionLocalArtifactDb artifactDb,
            DesignConversionLocalIndependentPreferenceEvaluator evaluator)
    {
        var rootBinding = DesignConversionIndependentPreferenceEvidenceAssociation
            .BuildArtifactRootBinding(artifactDb);
        var associated = DesignConversionIndependentPreferenceEvidenceAssociation
            .AssociateArtifactDb(artifactDb, evaluator);
        if (!associated.IsSuccess || associated.Value is null)
        {
            evaluator.Dispose();
            return OperationResult<
                DesignConversionLocalIndependentPreferenceEvaluatorRuntime>
                .Fail(
                    associated.ErrorMessage ??
                    "The local independent preference evaluator could not own the artifact root.",
                    associated.ErrorCode ?? ArtifactRootAlreadyOwnedErrorCode);
        }

        return OperationResult<
            DesignConversionLocalIndependentPreferenceEvaluatorRuntime>.Ok(
                new DesignConversionLocalIndependentPreferenceEvaluatorRuntime(
                    evaluator,
                    associated.Value,
                    rootBinding));
    }

    public void Dispose()
    {
        IDisposable? associationLease;
        DesignConversionLocalIndependentPreferenceEvaluator? evaluator;
        lock (_sync)
        {
            associationLease = _associationLease;
            evaluator = _evaluator;
            _associationLease = null;
            _evaluator = null;
        }

        associationLease?.Dispose();
        evaluator?.Dispose();
    }
}
