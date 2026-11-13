// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace TrainableRouter.Training; // Explicit caller-lifetime scientific operations; legacy timeout callers remain bounded.
using AgentInfrastructure; // Typed transport/domain refusals.
using ContinuousLearning; // Safe private artifact references contain no answers.
using ModelServer.Http; // Reuse all local URI, response body and JSON transport boundaries.

public sealed record CodeDocumentationScientificTrainingHttpResponse(string Status, string TrainingRunId, CodeDocumentationPrivateArtifactReference? HeldTrial, string? NativeQualityStatus, CodeDocumentationRepositoryOwnedDpoTrainingHttpResponse? Publication) // HELD has no publication fields or raw closed text.
{ // Accounting is observational, never a permission or score.
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] // Keep publish-only output free of invented new calls.
    public CodeDocumentationScientificEvaluationAccounting? EvaluationAccounting { get; init; } // Explicitly separate logical slots, current dispatches and unknown history.
} // No private response body is exposed.
public sealed class CodeDocumentationScientificTrainingModelServerClient // Separate public methods make caller-lifetime opt-in explicit.
{ // The request's legacy Timeout field is not used by these methods; no duration is invented.
    public const string EndpointPrefix = "/code-documentation/repository-owned-dpo/scientific/"; // Local live API path shared with ModelServer.
    private readonly ICallerLifetimeModelServerJsonTransport _transport; // Inject the purpose-specific transport interface.
    public CodeDocumentationScientificTrainingModelServerClient(ICallerLifetimeModelServerJsonTransport? transport = null) { _transport = transport ?? new HttpClientModelServerJsonTransport(); } // Default owner has HttpClient.Timeout infinite and no new local deadline.
    public Task<OperationResult<CodeDocumentationScientificTrainingHttpResponse>> TrainAndHoldForScienceAsync(CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(request, "train-and-hold", cancellationToken); // One update and the existing C0/T, stopping before canonical publication.
    public Task<OperationResult<CodeDocumentationScientificTrainingHttpResponse>> PublishHeldForScienceAsync(CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(request, "publish-held", cancellationToken); // Server startup supplies pinned review authority; the client sends no verdict or grant.
    public Task<OperationResult<CodeDocumentationScientificTrainingHttpResponse>> ResumeBaselineAndHoldForScienceAsync(CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest request, CancellationToken cancellationToken = default) => ExecuteAsync(request, "resume-baseline-and-hold", cancellationToken); // Explicit same-intent pre-executor continuation; never an automatic retry.
    private async Task<OperationResult<CodeDocumentationScientificTrainingHttpResponse>> ExecuteAsync(CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest request, string operation, CancellationToken token) // Caller cancellation is passed unchanged through this explicit path.
    { // Reuse legacy source/payload/path validation while ignoring its duration as a lifetime authority here.
        var publishOnly = operation == "publish-held"; // Hold and baseline recovery both require a genuine held response.
        var valid = CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient.ValidateRequest(request is null ? null : request with { Timeout = CodeDocumentationRepositoryOwnedDpoTrainingWireContract.DefaultTimeout }); // Keep all receipt/URI/evidence gates, without accepting an implicit time budget.
        if (!valid.IsSuccess || request is null) return valid.MapError<CodeDocumentationScientificTrainingHttpResponse>(); // Unknown/contradictory raw marker refuses before POST.
        if (request.RoutingDisposition != CodeDocumentationRepositoryOwnedDpoTrainingWireContract.RoutingNotPerformed) return Refuse(); // No legacy completed-routing envelope can acquire first-RP01 semantics.
        ModelServerHttpEndpointSafety.TryNormalizeLocalRootBaseUri(request.BaseUri, out var uri); // Existing local root normalization remains authoritative.
        var payload = new CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest(request.DomainSlug, request.DesignPatternSlug, request.DatasetVersion, request.TrainingRunId, request.RoutingRun, request.ProvenanceRefs) { RoutingDisposition = request.RoutingDisposition }; // Wire cannot carry server policy, acceptance or optimizer grants.
        var posted = await _transport.PostJsonForCallerLifetimeAsync<CodeDocumentationRepositoryOwnedDpoTrainingHttpRequest, CodeDocumentationScientificTrainingHttpResponse>(uri!, EndpointPrefix + operation, payload, token).ConfigureAwait(false); // No CancelAfter, HTTP timeout override or arbitrary deadline.
        if (!posted.IsSuccess || posted.Value is null) return posted.MapError<CodeDocumentationScientificTrainingHttpResponse>(); // Cancellation/failure never claims zero optimizer effects.
        if (!posted.Value.IsSuccessStatusCode || posted.Value.Body is not { } body || body.TrainingRunId != request.TrainingRunId || body.Status != (publishOnly ? "PUBLISHED" : "HELD_FOR_SCIENCE") || body.HeldTrial is not { } held || !CodeDocumentationScientificTrainingPolicy.Hex(held.Sha256) || held.ByteLength is <= 0 or > 67_108_864 || !SafeHeldPath(held.RelativePath, request)) return Refuse(); // Require exact operation/run and bounded safe private reference with no fabricated publication.
        if (!publishOnly && (body.Publication is not null || body.NativeQualityStatus is not ("Improved" or "Regressed" or "Indistinguishable" or "IdenticalWeights"))) return Refuse(); // Negative native outcomes are truthfully held, never relabelled published.
        if (operation == "resume-baseline-and-hold" && (body.EvaluationAccounting is not { HistoricalEvaluatorCalls: "UNKNOWN", CompletedLogicalResults: 120 } accounting || accounting.NewDispatchIntents < accounting.NewCallsIssued || accounting.NewCallsIssued < 0 || accounting.ReusedRawAnswers is < 0 or > 60 || accounting.ReusedEvaluatorResults < 0 || accounting.ReusedEvaluatorResults > accounting.ReusedRawAnswers || accounting.NewCallsIssued + accounting.ReusedEvaluatorResults != 120)) return Refuse(); // A recovery response cannot disguise logical completeness as lifetime physical-call evidence.
        if (publishOnly) // Reuse the existing complete ordinary publication wire validator.
        { // Same dataset and exact next checkpoint are mandatory.
            if (body.Publication is null) return Refuse(); // A held reference alone is not publication evidence.
            var text = CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient.ValidateResponseText(body.Publication); // Preserve all legacy human/error text size/control gates before displaying publication evidence.
            if (!text.IsSuccess || body.NativeQualityStatus != "Improved") return Refuse(); // Do not print a rejected/unsafe server body.
            var published = CodeDocumentationRepositoryOwnedDpoTrainingModelServerClient.ValidateSuccessResponse(body.Publication, request, request.RoutingRun.RepositoryOwnedDpoReceipt!); // Preserve receipt/quality/path checks.
            if (!published.IsSuccess) return published.MapError<CodeDocumentationScientificTrainingHttpResponse>(); // Never trust a raw success flag.
        } // No scientific gain is inferred by this client.
        return OperationResult<CodeDocumentationScientificTrainingHttpResponse>.Ok(body); // CLI sees only status, hashes, native quality and bounded public publication evidence.
    } // Raw retained answers stay private.
    private static bool SafeHeldPath(string path, CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest request) => !string.IsNullOrWhiteSpace(path) && path.Length <= CodeDocumentationRepositoryOwnedDpoTrainingWireContract.MaximumEvidenceRefLength && path.StartsWith("training-artifacts/private-evaluation/" + request.DomainSlug + "/" + request.DesignPatternSlug + "/", StringComparison.Ordinal) && path.EndsWith("/" + CodeDocumentationScientificTrainingPolicy.TextHash(request.TrainingRunId) + "/held-trial.json", StringComparison.Ordinal) && !path.Contains('\\') && !path.Contains(':') && path.Split('/').All(p => p.Length > 0 && p is not ("." or "..") && !p.Any(char.IsControl)); // Same safe run digest as the canonical layout owner; no rooted/escaped references can be printed or followed.
    private static OperationResult<CodeDocumentationScientificTrainingHttpResponse> Refuse() => OperationResult<CodeDocumentationScientificTrainingHttpResponse>.Fail("Scientific operation was refused or returned an invalid held/publication contract; reconcile the server journal before retry.", CodeDocumentationScientificTrainingPolicy.Invalid); // No closed text or raw server body in errors.
} // The native quality gate and independent scientific acceptance remain separate server responsibilities.
