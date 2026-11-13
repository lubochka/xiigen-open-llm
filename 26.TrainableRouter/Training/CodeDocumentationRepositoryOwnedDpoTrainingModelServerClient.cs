// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Globalization;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical; // Classify malformed raw input without throwing across the business boundary.
using ContinuousLearning; // Reuse the existing sealed-pair identity owner.
using ModelServer.Http;

/// <summary>
/// Общий wire contract для CodeDocumentation repository-owned DPO.
/// Он живёт ниже 25.ModelServer, чтобы CLI/runner мог вызвать серверный
/// endpoint без обратной ссылки на исполняемый сервер.
/// </summary>
public static class CodeDocumentationRepositoryOwnedDpoTrainingWireContract
{
    public const string RoutingNotPerformed = "NOT_PERFORMED"; // Explicitly distinguish raw input from a completed routing experiment.
    public const string RoutingDispositionInvalidErrorCode = "CODE_DOCUMENTATION_DPO_ROUTING_DISPOSITION_INVALID"; // Share refusal semantics between both transport ends.

    public static OperationResult<bool> ValidateRoutingDisposition( // Validate only the new opt-in contract; preserve every legacy request.
        CodeDocumentationTrainingRunResult? run, string? disposition, string domainSlug) // Bind the exact pair to its requested domain.
    { // Keep absent legacy observations unchanged.
        if (disposition is null) return OperationResult<bool>.Ok(true); // Null retains the pre-existing routing-run contract.
        if (disposition != RoutingNotPerformed || run is null || // Unknown markers must never acquire raw-input semantics.
            run.TrainingExampleCount != 0 || run.EvaluationExampleCount != 0 || // Raw reopen did not execute a routing curriculum.
            run.RoutedCorrect != 0 || run.SourceCorrect != 0 || run.VersionCorrect != 0 || run.TransitCorrect != 0 || // No routing outcomes were observed.
            run.RoutingAccuracy != 0 || run.SourceGateAccuracy != 0 || run.VersionGateAccuracy != 0 || run.TransitGateAccuracy != 0 || // Zero denotes absent observations only with this explicit marker.
            run.BottomUpOrderAccuracy != 0 || run.CalleeSummaryTransitAccuracy != 0 || run.LocalVsDelegatedAccuracy != 0 || // Refuse invented secondary routing scores too.
            run.CpuMilliseconds != 0 || run.PeakWorkingSetMb != 0 || run.ElapsedMilliseconds != 0 || // Routing resource measurements are absent, including NaN and infinity.
            run.Cases is not { Length: 0 } || run.DpoPairs is not { Length: 0 } || // Raw DPO is not routing-generated DPO or case evidence.
            run.RepositoryOwnedDpoReceipt is not { Source: RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset, PairCount: 1, OrderedPairs.Length: 1 } || // Only a strictly reopened single lesson belongs to this adapter.
            run.RepositoryOwnedDpoPairs is not { Length: 1 } || run.RepositoryOwnedDpoPairs[0] is null) // Require the actual single repository pair, never a receipt-only envelope.
            return InvalidRoutingDisposition(); // Contradictory input is refused before transport or server startup.
        var pair = run.RepositoryOwnedDpoPairs[0]; // Inspect only the supplied training pair, never assessment material.
        if (pair.Domain != domainSlug || pair.Prompt is null || pair.Chosen is null || pair.Rejected is null) // A different domain or malformed content cannot inherit the receipt.
            return InvalidRoutingDisposition(); // Preserve the business-result contract for malformed wire input.
        try // The existing rich-pair identity owner remains authoritative.
        { // Compare the request pair with its exact sealed manifest row.
            return FileDpoPairStore.ComputeIdentity(pair, 5) == run.RepositoryOwnedDpoReceipt.OrderedPairs[0]?.PairId // Bind all rich-pair values rather than only response text.
                ? OperationResult<bool>.Ok(true) : InvalidRoutingDisposition(); // Disk bytes are independently reopened by the existing coordinator.
        } // Finish the deterministic input check without changing trainer behavior.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Malformed pair graphs are invalid requests.
        { return InvalidRoutingDisposition(); } // Do not leak source content through an exception message.
    } // Both client and endpoint call this same opt-in validator.

    private static OperationResult<bool> InvalidRoutingDisposition() // Keep the failure independent of private source contents.
        => OperationResult<bool>.Fail("Raw sealed DPO input requires NOT_PERFORMED routing with no observations and its exact existing single pair.", RoutingDispositionInvalidErrorCode); // State the failed contract without fabricating effects.
    public const string EndpointPath =
        "/internal/continuous-learning/code-documentation/repository-owned-dpo/train-and-publish";
    public const int MaximumProvenanceRefCount = 64;
    public const int MaximumEvidenceRefCount = 128;
    public const int MaximumEvidenceRefLength = 512;
    public const int MaximumHumanTextLength = 2048;
    public const int MaximumErrorTextLength = 2048;
    public const int MaximumCoordinateTextLength = 128;
    public const int MaximumTrainingRunIdLength = 256;
    public const int MaximumRepositoryOwnedDpoReceiptPairCount = 10_000;
    public const string QualityDecisionInvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_QUALITY_DECISION_INVALID";
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromHours(6);
    public static readonly TimeSpan MaximumTimeout =
        HttpClientModelServerJsonTransport.MaximumTimeout;

    public static bool IsValidTimeout(TimeSpan timeout)
        => timeout > TimeSpan.Zero &&
           timeout <= MaximumTimeout &&
           timeout != Timeout.InfiniteTimeSpan;

    public static bool QualityDecisionIsAcceptedImprovement(
        string? status,
        double baselineMeanCorrectFraction,
        double trialMeanCorrectFraction,
        double difference,
        double noiseFloor,
        int expectedEvaluationCallCount,
        int observedEvaluationCallCount)
    {
        var expectedDifference =
            trialMeanCorrectFraction - baselineMeanCorrectFraction;
        return !string.IsNullOrWhiteSpace(status) &&
               status.Length <= MaximumCoordinateTextLength &&
               StringComparer.Ordinal.Equals(status, status.Trim()) &&
               !status.Any(char.IsControl) &&
               double.IsFinite(baselineMeanCorrectFraction) &&
               double.IsFinite(trialMeanCorrectFraction) &&
               double.IsFinite(difference) &&
               double.IsFinite(noiseFloor) &&
               baselineMeanCorrectFraction is >= 0d and <= 1d &&
               trialMeanCorrectFraction is >= 0d and <= 1d &&
               noiseFloor >= 0d &&
               Math.Abs(difference - expectedDifference) <= 1e-9d &&
               expectedDifference > noiseFloor &&
               difference > noiseFloor &&
               expectedEvaluationCallCount > 0 &&
               observedEvaluationCallCount == expectedEvaluationCallCount &&
               StringComparer.Ordinal.Equals(
                   status,
                   ContinuousLearning.MeasurementNoisePolicy.ImprovedStatus);
    }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest(
    string DomainSlug,
    string DesignPatternSlug,
    string DatasetVersion,
    string TrainingRunId,
    CodeDocumentationTrainingRunResult RoutingRun,
    IReadOnlyList<string> ProvenanceRefs)
{ // Preserve the positional constructor and legacy JSON shape.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Old callers omit this new opt-in field entirely.
    public string? RoutingDisposition { get; init; } // NOT_PERFORMED explicitly identifies the raw sealed-dataset composition.
} // Existing routing-run requests need no changes.

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionHttpEvidence(
    string Status,
    double BaselineMeanCorrectFraction,
    double TrialMeanCorrectFraction,
    double Difference,
    double NoiseFloor,
    int ExpectedEvaluationCallCount,
    int ObservedEvaluationCallCount);

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse(
    bool IsSuccess,
    string ErrorCode,
    string ErrorMessage,
    string HumanMessage,
    string? DpoManifestRelativePath,
    string? DpoManifestSha256,
    int? ReopenedDpoPairCount,
    int? ParentCheckpointStep,
    int? PublishedCheckpointStep,
    string? PublishedWeightsRelativePath,
    string? PublishedManifestRelativePath,
    IReadOnlyList<string> EvidenceRefs,
    CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionHttpEvidence?
        QualityDecision = null)
{
    public static CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse
        Success(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence?
                evidence)
    {
        var qualityDecision = evidence?.QualityDecision;
        if (qualityDecision is null ||
            !qualityDecision.Accepted ||
            !CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                .QualityDecisionIsAcceptedImprovement(
                    qualityDecision.Status,
                    qualityDecision.BaselineMeanCorrectFraction,
                    qualityDecision.TrialMeanCorrectFraction,
                    qualityDecision.Difference,
                    qualityDecision.NoiseFloor,
                    qualityDecision.ExpectedEvaluationCallCount,
                    qualityDecision.ObservedEvaluationCallCount))
        {
            return Failure(
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .QualityDecisionInvalidErrorCode,
                "CodeDocumentation repository-owned DPO success response requires an accepted and numerically consistent quality decision.");
        }

        return new CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse(
            true,
            string.Empty,
            string.Empty,
            "Модель документации кода дообучена одной новой сохранённой парой; отдельная проверка подтвердила улучшение, и ModelServer сохранил следующую принятую версию.",
            evidence!.DpoManifestRelativePath,
            evidence.DpoManifestSha256,
            evidence.ReopenedDpoPairCount,
            evidence.ParentCheckpointStep,
            evidence.PublishedCheckpointStep,
            evidence.PublishedWeightsRelativePath,
            evidence.PublishedManifestRelativePath,
            evidence.EvidenceRefs,
            new CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionHttpEvidence(
                qualityDecision.Status,
                qualityDecision.BaselineMeanCorrectFraction,
                qualityDecision.TrialMeanCorrectFraction,
                qualityDecision.Difference,
                qualityDecision.NoiseFloor,
                qualityDecision.ExpectedEvaluationCallCount,
                qualityDecision.ObservedEvaluationCallCount));
    }

    public static CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse
        Failure(string errorCode, string errorMessage)
        => new(
            false,
            string.IsNullOrWhiteSpace(errorCode) ? "ERROR" : errorCode,
            errorMessage,
            "Модель документации кода не получила новую принятую версию: ModelServer закрыл операцию до обучения или после пробной попытки без публикации.",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            [],
            QualityDecision: null);
}

public sealed record CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest(
    Uri BaseUri,
    string DomainSlug,
    string DesignPatternSlug,
    string DatasetVersion,
    string TrainingRunId,
    CodeDocumentationTrainingRunResult RoutingRun,
    IReadOnlyList<string> ProvenanceRefs,
    TimeSpan Timeout)
{ // The local request mirrors the optional wire discriminator.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Preserve serialization compatibility for legacy callers too.
    public string? RoutingDisposition { get; init; } // Raw adapter sets this only after successful strict Reopen.
} // No server-owned model or training authority is exposed here.

/// <summary>
/// Узкий клиент публикации. Он отправляет только DPO receipt/routing evidence и
/// затем проверяет, что сервер вернул repository-relative checkpoint evidence.
/// </summary>
public sealed class CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient
{
    public const string InvalidRequestErrorCode =
        "CODE_DOCUMENTATION_MODEL_SERVER_REQUEST_INVALID";
    public const string ReceiptRequiredErrorCode =
        "CODE_DOCUMENTATION_MODEL_SERVER_DPO_RECEIPT_REQUIRED";
    public const string RejectedErrorCode =
        "CODE_DOCUMENTATION_MODEL_SERVER_REJECTED";
    public const string InvalidResponseErrorCode =
        "CODE_DOCUMENTATION_MODEL_SERVER_RESPONSE_INVALID";
    public const string ManifestMismatchErrorCode =
        "CODE_DOCUMENTATION_MODEL_SERVER_DPO_MANIFEST_MISMATCH";
    public const string UnsafeResponsePathErrorCode =
        "CODE_DOCUMENTATION_MODEL_SERVER_RESPONSE_UNSAFE_PATH";
    public const string UnsafeResponseEvidenceErrorCode =
        "CODE_DOCUMENTATION_MODEL_SERVER_RESPONSE_UNSAFE_EVIDENCE";

    private readonly IModelServerJsonTransport _transport;

    public CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient(
        IModelServerJsonTransport? transport = null)
    {
        _transport = transport ?? new HttpClientModelServerJsonTransport();
    }

    public async Task<OperationResult<
        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>>
        TrainAndPublishAsync(
            CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest
                request,
            CancellationToken cancellationToken = default)
    {
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
            return validation.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>();

        var receipt = request.RoutingRun.RepositoryOwnedDpoReceipt!;
        ModelServerHttpEndpointSafety.TryNormalizeLocalRootBaseUri(
            request.BaseUri,
            out var normalizedBaseUri);
        var payload = new CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest(
            request.DomainSlug,
            request.DesignPatternSlug,
            request.DatasetVersion,
            request.TrainingRunId,
            request.RoutingRun,
            request.ProvenanceRefs)
        { RoutingDisposition = request.RoutingDisposition }; // Carry the explicit raw contract across the actual JSON transport.

        var posted = await _transport
            .PostJsonAsync<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest,
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>(
                normalizedBaseUri!,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .EndpointPath,
                payload,
                request.Timeout,
                cancellationToken)
            .ConfigureAwait(false);
        if (!posted.IsSuccess || posted.Value is null)
            return posted.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>();

        var response = posted.Value.Body;
        if (response is not null)
        {
            var textValidation = ValidateResponseText(response);
            if (!textValidation.IsSuccess)
                return textValidation.MapError<
                    CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>();
        }

        if (!posted.Value.IsSuccessStatusCode)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>.Fail(
                BuildRejectedMessage(posted.Value.StatusCode, response),
                string.IsNullOrWhiteSpace(response?.ErrorCode)
                    ? RejectedErrorCode
                    : response!.ErrorCode);
        }

        if (response is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>.Fail(
                "ModelServer returned success status without the CodeDocumentation DPO publication contract.",
            InvalidResponseErrorCode);
        }

        var responseValidation = ValidateSuccessResponse(
            response,
            request,
            receipt);
        if (!responseValidation.IsSuccess)
            return responseValidation.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>();

        return OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse>.Ok(
            response);
    }

    internal static OperationResult<bool> ValidateRequest(
        CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest? request)
    {
        if (request is null ||
            !ModelServerHttpEndpointSafety.IsSafeLocalRootBaseUri(
                request.BaseUri) ||
            !IsBoundedRequiredText(
                request.DomainSlug,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumCoordinateTextLength) ||
            !IsBoundedRequiredText(
                request.DesignPatternSlug,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumCoordinateTextLength) ||
            !IsBoundedRequiredText(
                request.DatasetVersion,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumCoordinateTextLength) ||
            !IsBoundedRequiredText(
                request.TrainingRunId,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumTrainingRunIdLength) ||
            request.RoutingRun is null ||
            !AreSafeEvidenceRefs(
                request.ProvenanceRefs,
            CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumProvenanceRefCount) ||
            !CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                .IsValidTimeout(request.Timeout))
        {
            return OperationResult<bool>.Fail(
                "CodeDocumentation ModelServer publication needs explicit bounded URI, DPO coordinates, run evidence, provenance refs and timeout.",
                InvalidRequestErrorCode);
        }

        var disposition = CodeDocumentationRepositoryOwnedDpoTrainingWireContract.ValidateRoutingDisposition( // Refuse contradictory raw envelopes before any POST.
            request.RoutingRun, request.RoutingDisposition, request.DomainSlug); // Legacy null markers keep the previous validation below.
        if (!disposition.IsSuccess) return disposition; // Preserve the shared exact refusal code.
        var receipt = request.RoutingRun.RepositoryOwnedDpoReceipt;
        if (receipt is null)
        {
            return OperationResult<bool>.Fail(
                "CodeDocumentation ModelServer publication requires a successfully reopened repository-owned DPO receipt; DPO-only runs do not claim checkpoints.",
                ReceiptRequiredErrorCode);
        }

        if (!IsSafeRepositoryRelativePath(
                receipt.RepositoryRelativeManifestPath) ||
            !IsHexSha256(receipt.ManifestSha256) ||
            !IsHexSha256(receipt.RequestIdentitySha256) ||
            receipt.PairCount <= 0 ||
            receipt.PairCount >
            CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                .MaximumRepositoryOwnedDpoReceiptPairCount ||
            receipt.OrderedPairs is null ||
            receipt.OrderedPairs.Length != receipt.PairCount ||
            receipt.OrderedPairs.Any(static pair =>
                !IsTrimmedVisible(pair.PairId) ||
                !IsSafeRepositoryRelativePath(
                    pair.RepositoryRelativePairFilePath) ||
                !IsHexSha256(pair.PairFileSha256)))
        {
            return OperationResult<bool>.Fail(
                "CodeDocumentation ModelServer request contains an unsafe DPO repository-relative path.",
                InvalidRequestErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    internal static OperationResult<bool> ValidateSuccessResponse(
        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse response,
        CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest request,
        CodeDocumentationRepositoryOwnedDpoDatasetReceipt receipt)
    {
        if (!response.IsSuccess)
        {
            return OperationResult<bool>.Fail(
                string.IsNullOrWhiteSpace(response.ErrorMessage)
                    ? response.HumanMessage
                    : response.ErrorMessage,
                string.IsNullOrWhiteSpace(response.ErrorCode)
                    ? RejectedErrorCode
                    : response.ErrorCode);
        }

        if (!string.IsNullOrEmpty(response.ErrorCode) ||
            !string.IsNullOrEmpty(response.ErrorMessage))
        {
            return OperationResult<bool>.Fail(
                "ModelServer returned a success response that also carried an error code or error message.",
                InvalidResponseErrorCode);
        }

        if (string.IsNullOrWhiteSpace(response.DpoManifestRelativePath) ||
            !IsHexSha256(response.DpoManifestSha256) ||
            response.ReopenedDpoPairCount is null ||
            response.ParentCheckpointStep is null ||
            response.PublishedCheckpointStep is null ||
            string.IsNullOrWhiteSpace(response.PublishedWeightsRelativePath) ||
            string.IsNullOrWhiteSpace(response.PublishedManifestRelativePath) ||
            response.QualityDecision is null ||
            !QualityDecisionIsValid(response.QualityDecision) ||
            !HasBoundedEvidenceRefShape(
                response.EvidenceRefs,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumEvidenceRefCount))
        {
            return OperationResult<bool>.Fail(
                "ModelServer returned an incomplete CodeDocumentation DPO publication response.",
                InvalidResponseErrorCode);
        }

        if (!IsSafeRepositoryRelativePath(response.DpoManifestRelativePath) ||
            !IsSafeRepositoryRelativePath(
                response.PublishedWeightsRelativePath) ||
            !IsSafeRepositoryRelativePath(
                response.PublishedManifestRelativePath))
        {
            return OperationResult<bool>.Fail(
                "ModelServer returned a rooted or escaping path instead of repository-relative checkpoint evidence.",
                UnsafeResponsePathErrorCode);
        }

        if (response.EvidenceRefs.Any(static reference =>
                !IsSafeEvidenceRef(reference)))
        {
            return OperationResult<bool>.Fail(
                "ModelServer returned unsafe evidence refs in the CodeDocumentation DPO publication response.",
                UnsafeResponseEvidenceErrorCode);
        }

        if (!StringComparer.Ordinal.Equals(
                response.DpoManifestRelativePath,
                receipt.RepositoryRelativeManifestPath) ||
            !StringComparer.Ordinal.Equals(
                response.DpoManifestSha256,
                receipt.ManifestSha256) ||
            response.ReopenedDpoPairCount != receipt.PairCount)
        {
            return OperationResult<bool>.Fail(
                "ModelServer DPO publication response does not match the repository-owned DPO receipt sent by the runner.",
                ManifestMismatchErrorCode);
        }

        if (response.ParentCheckpointStep < 1 ||
            response.PublishedCheckpointStep < 1 ||
            response.PublishedCheckpointStep !=
            response.ParentCheckpointStep + 1)
        {
            return OperationResult<bool>.Fail(
                "ModelServer did not publish the next checkpoint step after the reopened parent.",
                InvalidResponseErrorCode);
        }

        var weightsPath = ValidatePublishedArtifactPath(
            response.PublishedWeightsRelativePath!,
            request,
            response.PublishedCheckpointStep.Value,
            "weights.bin");
        var manifestPath = ValidatePublishedArtifactPath(
            response.PublishedManifestRelativePath!,
            request,
            response.PublishedCheckpointStep.Value,
            "manifest.json");
        if (!weightsPath.IsSuccess || !manifestPath.IsSuccess)
        {
            return OperationResult<bool>.Fail(
                "ModelServer returned checkpoint paths outside the exact CodeDocumentation repository-owned model artifact shape.",
                InvalidResponseErrorCode);
        }

        if (!StringComparer.Ordinal.Equals(
                weightsPath.Value,
                manifestPath.Value))
        {
            return OperationResult<bool>.Fail(
                "ModelServer returned weights and manifest paths from different checkpoint directories.",
                InvalidResponseErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool QualityDecisionIsValid(
        CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionHttpEvidence
            qualityDecision)
        => CodeDocumentationRepositoryOwnedDpoTrainingWireContract
            .QualityDecisionIsAcceptedImprovement(
                qualityDecision.Status,
                qualityDecision.BaselineMeanCorrectFraction,
                qualityDecision.TrialMeanCorrectFraction,
                qualityDecision.Difference,
                qualityDecision.NoiseFloor,
                qualityDecision.ExpectedEvaluationCallCount,
                qualityDecision.ObservedEvaluationCallCount);

    internal static OperationResult<bool> ValidateResponseText(
        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse response)
    {
        if (!IsBoundedOptionalText(
                response.ErrorCode,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumErrorTextLength) ||
            !IsBoundedOptionalText(
                response.ErrorMessage,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumErrorTextLength) ||
            !IsBoundedRequiredText(
                response.HumanMessage,
                CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                    .MaximumHumanTextLength))
        {
            return OperationResult<bool>.Fail(
                "ModelServer returned human or error text outside the bounded wire contract.",
                InvalidResponseErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static string BuildRejectedMessage(
        int statusCode,
        CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse? response)
    {
        var details = response is null
            ? "empty or non-contract body"
            : string.IsNullOrWhiteSpace(response.ErrorMessage)
                ? string.IsNullOrWhiteSpace(response.HumanMessage)
                    ? "empty error message"
                    : response.HumanMessage
                : response.ErrorMessage;
        return $"ModelServer rejected CodeDocumentation DPO publication with HTTP {statusCode}: {details}";
    }

    private static bool IsTrimmedVisible(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
           !value.Any(char.IsControl);

    private static bool IsBoundedRequiredText(string? value, int maximumLength)
        => IsTrimmedVisible(value) &&
           value!.Length <= maximumLength;

    private static bool IsBoundedOptionalText(string? value, int maximumLength)
        => value is null ||
           (string.Equals(value, value.Trim(), StringComparison.Ordinal) &&
            value.Length <= maximumLength &&
            !value.Any(char.IsControl));

    private static bool AreSafeEvidenceRefs(
        IReadOnlyList<string>? evidenceRefs,
        int maximumCount)
        => HasBoundedEvidenceRefShape(evidenceRefs, maximumCount) &&
           evidenceRefs!.All(IsSafeEvidenceRef);

    private static bool HasBoundedEvidenceRefShape(
        IReadOnlyList<string>? evidenceRefs,
        int maximumCount)
        => evidenceRefs is not null &&
           evidenceRefs.Count > 0 &&
           evidenceRefs.Count <= maximumCount &&
           evidenceRefs.All(static value =>
               IsTrimmedVisible(value) &&
               value.Length <=
               CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                   .MaximumEvidenceRefLength);

    private static bool IsSafeEvidenceRef(string? value)
    {
        if (!IsTrimmedVisible(value) ||
            value!.Length >
            CodeDocumentationRepositoryOwnedDpoTrainingWireContract
                .MaximumEvidenceRefLength ||
            value.StartsWith("/", StringComparison.Ordinal) ||
            value.StartsWith("\\", StringComparison.Ordinal) ||
            value.Contains('\\', StringComparison.Ordinal) ||
            value.Contains("/mnt/", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("file://", StringComparison.OrdinalIgnoreCase) ||
            ContainsDriveRoot(value))
        {
            return false;
        }

        return true;
    }

    private static OperationResult<string> ValidatePublishedArtifactPath(
        string path,
        CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest request,
        int publishedCheckpointStep,
        string expectedFileName)
    {
        var segments = path.Split('/');
        var expectedStep = publishedCheckpointStep.ToString(
            "D6",
            CultureInfo.InvariantCulture);
        if (publishedCheckpointStep is < 1 or > 999_999 ||
            segments.Length != 9 ||
            !StringComparer.Ordinal.Equals(
                segments[0],
                "training-artifacts") ||
            !StringComparer.Ordinal.Equals(segments[1], "models") ||
            !StringComparer.Ordinal.Equals(segments[2], request.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                segments[3],
                request.DesignPatternSlug) ||
            !IsSafeArtifactPathSegment(segments[4]) ||
            !IsSafeArtifactPathSegment(segments[5]) ||
            !StringComparer.Ordinal.Equals(segments[6], "checkpoints") ||
            !StringComparer.Ordinal.Equals(segments[7], expectedStep) ||
            !StringComparer.Ordinal.Equals(segments[8], expectedFileName))
        {
            return OperationResult<string>.Fail(
                "Published checkpoint path does not match the repository-owned model artifact layout.",
                InvalidResponseErrorCode);
        }

        return OperationResult<string>.Ok(string.Join("/", segments.Take(8)));
    }

    private static bool IsSafeArtifactPathSegment(string segment)
        => IsTrimmedVisible(segment) &&
           segment is not "." and not ".." &&
           !segment.Contains(':', StringComparison.Ordinal) &&
           !segment.Contains('\\', StringComparison.Ordinal) &&
           !segment.Contains('/', StringComparison.Ordinal);

    private static bool IsHexSha256(string? value)
        => value is not null &&
           value.Length == 64 &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');

    private static bool ContainsDriveRoot(string value)
    {
        for (var index = 0; index + 2 < value.Length; index++)
        {
            if (char.IsAsciiLetter(value[index]) &&
                value[index + 1] == ':' &&
                value[index + 2] is '\\' or '/')
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSafeRepositoryRelativePath(string? path)
    {
        if (!IsTrimmedVisible(path) ||
            path!.StartsWith("/", StringComparison.Ordinal) ||
            path.StartsWith("\\", StringComparison.Ordinal) ||
            path.Contains('\\', StringComparison.Ordinal) ||
            path.Contains(':', StringComparison.Ordinal) ||
            path.Contains("//", StringComparison.Ordinal) ||
            Path.IsPathFullyQualified(path))
        {
            return false;
        }

        var segments = path.Split('/');
        return segments.Length > 0 &&
               segments.All(static segment =>
                   segment.Length > 0 &&
                   segment is not "." and not "..");
    }
}
