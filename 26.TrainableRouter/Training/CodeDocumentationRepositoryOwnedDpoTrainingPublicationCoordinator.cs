// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

// The test assembly exercises these same recovery validators, not a copied
// test-only implementation. This does not expose a new public recovery API.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]

namespace TrainableRouter.Training;

using System.Security.Cryptography;
using System.Text.Json; // Serialize captured prepared metadata with the existing JSON representation.
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using LoRA;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using TrainingLoop.Optimizers;

/// <summary>
/// One bounded CodeDocumentation lifecycle request: take the DPO dataset already
/// sealed in the repository, start the model from the repository-owned checkpoint
/// tree, run DPO once, and publish the resulting full model back to the same tree.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest(
    CodeDocumentationTrainingRunResult RoutingRun,
    RepositoryOwnedDpoDatasetAddress DpoDatasetAddress,
    CodeDocumentationRepositoryOwnedModelCheckpointStartRequest StartRequest,
    BpeTokenizer Tokenizer,
    LoraConfig LoraConfig,
    IServerOwnedDpoTrainingLeaseBroker LeaseBroker,
    string TrainingRunId,
    int DpoStepCount,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<string> ProvenanceRefs,
    CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext?
        QualityGate = null,
    AdamWPortableState? OptimizerStateToRestore = null,
    RepositoryOwnedModelTrainingAttemptAddress? OptimizerStateSourceAttempt = null,
    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization?
        OptimizerStateColdStartAuthorization = null,
    LoraPairRandomSeedAuthority? AdapterSeedAuthority = null,
    LlmModelRandomStateSnapshot? RandomStateToRestore = null,
    DpoRandomStateColdStartAuthorization?
        RandomStateColdStartAuthorization = null)
{
    internal DpoContinuationProof? ContinuationProof { get; init; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] // Retain legacy constructor/JSON omission while carrying truthful raw source semantics to the core.
    public string? RoutingDisposition { get; init; } // Raw input never pretends to be a completed routing experiment.
    internal CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ScientificInvocation? Scientific { get; init; } // Only the explicit server-owned scientific entry can attach publication authority.
}

/// <summary>
/// Human-readable evidence that the lifecycle used repository DPO rows and
/// repository model checkpoints instead of a cache, quarantine folder, or fresh
/// model restart.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence(
    string DpoManifestRelativePath,
    string DpoManifestSha256,
    int ReopenedDpoPairCount,
    int? ParentCheckpointStep,
    int PublishedCheckpointStep,
    string PublishedWeightsRelativePath,
    string PublishedManifestRelativePath,
    CodeDocumentationServerOwnedDpoTrainingEvidence Training,
    CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence Publication,
    IReadOnlyList<string> EvidenceRefs,
    CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence?
        QualityDecision = null,
    string? OptimizerStateColdStartReason = null);

/// <summary>
/// Coordinates the first real CodeDocumentation path that both reuses a sealed
/// DPO dataset and publishes trained model weights in the repository.
/// </summary>
public static partial class CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator
{
    private sealed record OptimizerStateSourceResolution(
        AdamWPortableState? OptimizerState,
        LlmModelRandomStateSnapshot? RandomState,
        DpoContinuationProof? Proof = null);

    public const string InvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_PUBLICATION_INVALID";
    public const string DpoDatasetMismatchErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_PUBLICATION_DPO_MISMATCH";
    public const string StartFailedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_PUBLICATION_START_FAILED";
    public const string TrainingFailedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_PUBLICATION_TRAINING_FAILED";
    public const string SingleNewPairRequiredErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_SINGLE_NEW_PAIR_REQUIRED";
    public const string PairAlreadyAppliedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_PAIR_ALREADY_APPLIED";
    public const string AppliedPairIdentityConflictErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_APPLIED_PAIR_IDENTITY_CONFLICT";
    public const string ParentCheckpointRequiredErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_PARENT_REQUIRED";
    public const string TrialCheckpointMismatchErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_TRIAL_CHECKPOINT_MISMATCH";
    public const string TrainingAttemptRecoveryRequiredErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_ATTEMPT_RECOVERY_REQUIRED";
    public const string PublishFailedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_PUBLICATION_PUBLISH_FAILED";
    public const string OptimizerStateRestoreRequiredErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_OPTIMIZER_STATE_RESTORE_REQUIRED";
    public const string OptimizerStateColdStartAuthorizationRequiredErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_OPTIMIZER_STATE_COLD_START_AUTHORIZATION_REQUIRED";
    public const string OptimizerStateColdStartAuthorizationInvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_OPTIMIZER_STATE_COLD_START_AUTHORIZATION_INVALID";
    public const string RandomStateRestoreRequiredErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_RANDOM_STATE_RESTORE_REQUIRED";
    public const string RandomStateSourceMismatchErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_RANDOM_STATE_SOURCE_MISMATCH";
    public const string AdapterSeedAuthorityMismatchErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_TRAINING_ADAPTER_SEED_AUTHORITY_MISMATCH";

    /// <summary>The issuer projects its own result, then a distinct receiving owner authenticates it.</summary>
    internal static async Task<OperationResult<OrdinaryBaseline>> PrepareOrdinaryBaselineAsync( // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest start, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        CancellationToken cancellationToken, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        CodeDocumentationPreparedBaselineExternalReference? expectedOriginal = null) // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
    { // Begin use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        var issuing = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        var original = await (expectedOriginal is null ? issuing.ProduceOrOpenOrdinaryAsync(catalog, selector, start.RepositoryRoot, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, start.ModelVersion, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            start.ModelConfig, start.RuntimeCompatibility, context, start.LoraConfig, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            start.AdapterSeedAuthority, cancellationToken) : issuing.OpenExistingOrdinaryAsync(catalog,selector,start.RepositoryRoot, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            start.DomainSlug,start.DesignPatternSlug,start.ModelIdSlug,start.ModelVersion,start.ModelConfig,start.RuntimeCompatibility, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            context,start.LoraConfig,start.AdapterSeedAuthority,cancellationToken)).ConfigureAwait(false); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        if (!original.IsSuccess || original.Value is null) return original.MapError<OrdinaryBaseline>(); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        var reference = issuing.GetExternalReference(original.Value.Readback); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        if (!reference.IsSuccess || reference.Value is null) return reference.MapError<OrdinaryBaseline>(); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        if(expectedOriginal is not null && (reference.Value.ReceiptId!=expectedOriginal.ReceiptId || // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            reference.Value.ReceiptSha256!=expectedOriginal.ReceiptSha256)) // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            return OperationResult<OrdinaryBaseline>.Fail("Saved selection ORIGINAL differs from the existing owner.", // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
                CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        var receiving = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        var opened = await receiving.OpenExistingByExternalReferenceAsync(reference.Value, catalog, selector, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            start.RepositoryRoot, start.DomainSlug, start.DesignPatternSlug, start.ModelIdSlug, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            start.ModelVersion, start.ModelConfig, start.RuntimeCompatibility, original.Value.Context, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            start.LoraConfig, start.AdapterSeedAuthority, cancellationToken).ConfigureAwait(false); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        if (!opened.IsSuccess || opened.Value is null) return opened.MapError<OrdinaryBaseline>(); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
        return OperationResult<OrdinaryBaseline>.Ok(OrdinaryBaseline.Mint(receiving, opened.Value, // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
            reference.Value, original.Value.Context)); // Use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.
    } // Finish use the saved ORIGINAL expectation to select existing-only capture and then authenticate it with a distinct receiving owner.

    internal sealed class OrdinaryBaseline
    {
        private OrdinaryBaseline(CodeDocumentationPreparedBaselineReadback readback,
            CodeDocumentationPreparedBaselineExternalReference reference,
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context)
        { Readback = readback; ExternalReference = reference; Context = context; }
        internal CodeDocumentationPreparedBaselineReadback Readback { get; }
        internal CodeDocumentationPreparedBaselineExternalReference ExternalReference { get; }
        internal CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext Context { get; }
        internal static OrdinaryBaseline Mint(CodeDocumentationRepositoryOwnedPreparedBaselineJournal owner,
            CodeDocumentationPreparedBaselineReadback readback,
            CodeDocumentationPreparedBaselineExternalReference reference,
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context)
        {
            var projected = owner.GetExternalReference(readback);
            if (!projected.IsSuccess || projected.Value is null ||
                projected.Value.ReceiptId != reference.ReceiptId || projected.Value.ReceiptSha256 != reference.ReceiptSha256)
                throw new InvalidOperationException("Receiving original authentication failed.");
            return new(readback, reference, context);
        }
    }

    /// <summary>
    /// Reads the original committed pre-authoring measurement. This engineering
    /// prerequisite never enters the training/publication lifecycle below.
    /// </summary>
    internal static async Task<OperationResult<CodeDocumentationPreparedBaselineReadback>>
        ReadPreparedBaselinePrerequisiteAsync(
            CodeDocumentationOneLessonGrantCatalog catalog,
            CodeDocumentationOneLessonSelectorV1 selector,
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest acceptedParentRequest,
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext frozenQualityContext,
            CancellationToken cancellationToken = default)
    {
        if (catalog is null || selector is null || acceptedParentRequest is null ||
            acceptedParentRequest.ModelConfig is null || acceptedParentRequest.RuntimeCompatibility is null ||
            frozenQualityContext is null)
            return OperationResult<CodeDocumentationPreparedBaselineReadback>.Fail(
                "The original prepared-baseline inputs are required.", InvalidErrorCode);

        var owner = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        var opened = await owner.OpenExistingAsync(catalog, selector,
            acceptedParentRequest.RepositoryRoot, acceptedParentRequest.DomainSlug,
            acceptedParentRequest.DesignPatternSlug, acceptedParentRequest.ModelIdSlug,
            acceptedParentRequest.ModelVersion, acceptedParentRequest.ModelConfig,
            acceptedParentRequest.RuntimeCompatibility, frozenQualityContext,
            acceptedParentRequest.LoraConfig, acceptedParentRequest.AdapterSeedAuthority,
            cancellationToken).ConfigureAwait(false);
        if (!opened.IsSuccess || opened.Value is null) return opened;

        var readback = opened.Value;
        if (!owner.IsIssuedReadback(readback))
            return OperationResult<CodeDocumentationPreparedBaselineReadback>.Fail(
                "The readback was not issued by this original owner.", InvalidErrorCode);
        var classification = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
            .BaselineRequiresNewTrainingPair(readback.Baseline);
        var needsLesson = classification.IsSuccess && classification.Value;
        var alreadySufficient = !classification.IsSuccess && classification.ErrorCode ==
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BaselineAlreadySufficientErrorCode;
        if ((!needsLesson && !alreadySufficient) ||
            !StringComparer.Ordinal.Equals(readback.Classification,
                needsLesson ? "NEEDS_LESSON" : "NO_TRAINING_NEEDED"))
            return OperationResult<CodeDocumentationPreparedBaselineReadback>.Fail(
                "The original owner readback and quality policy disagree.", InvalidErrorCode);
        return opened;
    }

    public static Task<OperationResult< // Keep the existing public training and publication result contract.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>>
        TrainAndPublishAsync(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request,
            CancellationToken cancellationToken = default)
        => TrainAndPublishCoreAsync(request, null, null, cancellationToken); // Enter the ordinary path without a prepared original or invocation capture.

    // The prepared original is freshly read by the existing journal. It never
    // authorizes a second calibration or a coordinator-owned attempt claim.
    internal static Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>> // Expose the bounded prepared entry only to the existing internal consumers.
        ExecuteFromPreparedBaselineAsync( // Run one lesson from the already measured prepared original.
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Accept the current server-owned publication request.
            CodeDocumentationPreparedBaselineAttemptInputs inputs, // Accept the existing owner-bound prepared original inputs.
            CancellationToken cancellationToken = default) // Preserve caller cancellation for the prepared route.
        => ExecuteCapturedPreparedAsync(request, inputs, cancellationToken); // Capture the invocation before entering the shared publication path.

    /// <summary>Returns a completed measurement; success here is not a publication permit.</summary>
    internal static Task<OperationResult<CompletedPreparedTrialOutcome>> ExecutePreparedLessonWithOutcomeAsync( // Enter the existing lifecycle with an internal measurement projection.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Require the same current publication request.
        CodeDocumentationPreparedBaselineAttemptInputs inputs, // Require the existing prepared ORIGINAL bindings.
        CancellationToken cancellationToken = default) // Preserve cancellation as refusal.
        => CompletedPreparedTrialOutcome.ExecuteAsync(request, inputs, cancellationToken); // Only this lifecycle can mint the result.

    internal sealed class CompletedPreparedTrialOutcome // Retain complete measurement evidence without exposing a publication authority.
    { // Protect the complete owner-produced graph against caller mutation.
        private readonly byte[] _detailedBytes; // Keep the serialized snapshot private and never return its array.

        private CompletedPreparedTrialOutcome( // Construction is inaccessible to callers and arbitrary DTO factories.
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback readback, // Consume only the private lifecycle's current owner readback.
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Bind the same captured and validated request.
            CodeDocumentationPreparedBaselineAttemptInputs inputs, int? publishedCheckpointStep) // Bind the same ORIGINAL and any actual successful publication.
        { // Detach every mutable graph member while keeping primitive provenance immutable.
            _detailedBytes = ValueBytes(readback.Detailed); // Snapshot both complete measurement series with the existing JSON representation.
            Disposition = readback.Detailed.Disposition; // Retain the quality owner's actual disposition.
            TrainingRunId = request.TrainingRunId; // Retain the current training attempt identity.
            TrainingPairId = readback.Detailed.Evidence.TrainingPairId; // Retain the sole owner-bound training pair.
            OriginalSha256 = inputs.Reference.Sha256; // Bind the durable original measurement.
            StartedSha256 = readback.StartedSha256; // Bind the actual dispatch record.
            CompletedSha256 = readback.CompletedSha256; // Bind the exact freshly admitted completed record.
            DpoManifestRelativePath = request.DpoDatasetAddress.ManifestRelativePath; // Retain the current lesson manifest address.
            DpoManifestSha256 = request.RoutingRun.RepositoryOwnedDpoReceipt!.ManifestSha256; // Retain the verified current dataset manifest identity.
            AttemptRootRelativePath = CreateTrainingAttemptAddress(request.StartRequest, request.TrainingRunId).Value!.AttemptRootRelativePath; // Retain the owner-resolved attempt address.
            PublishedCheckpointStep = publishedCheckpointStep; // Report publication only after the existing positive path succeeds.
        } // Finish immutable snapshot construction.

        internal CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition Disposition { get; } // Expose the immutable completed comparison.
        internal string TrainingRunId { get; } // Expose the immutable attempt identity.
        internal string TrainingPairId { get; } // Expose the immutable sole-pair identity.
        internal string OriginalSha256 { get; } // Expose the immutable ORIGINAL content hash.
        internal string StartedSha256 { get; } // Expose the immutable dispatch record hash.
        internal string CompletedSha256 { get; } // Expose the immutable complete record hash.
        internal string DpoManifestRelativePath { get; } // Expose the immutable dataset manifest address.
        internal string DpoManifestSha256 { get; } // Expose the immutable dataset manifest hash.
        internal string AttemptRootRelativePath { get; } // Expose the immutable attempt address.
        internal int? PublishedCheckpointStep { get; } // Report the observed successful publication step when present.
        internal CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult Detailed // Return a detached full graph on every read.
            => JsonSerializer.Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingDetailedQualityResult>(_detailedBytes) // Reconstruct collections without exposing the private snapshot.
                ?? throw new InvalidOperationException("The completed measurement snapshot is required."); // A missing private snapshot is an invariant defect, not measured neutral quality.

        // Construction remains inside this lifecycle entry. No DTO factory or
        // error-code conversion can mint a completed outcome for another caller.
        internal static async Task<OperationResult<CompletedPreparedTrialOutcome>> ExecuteAsync( // Run the single lifecycle instead of converting a legacy error into evidence.
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Use the original current request for capture.
            CodeDocumentationPreparedBaselineAttemptInputs inputs, CancellationToken token) // Use the current ORIGINAL binding and caller cancellation.
        { // Keep completion authority private and invocation-local.
            CompletedPreparedTrialOutcome? outcome = null; // Remain uncompleted until the core validates a real owner readback.
            var completion = new PreparedCompletionCapture((readback, step) => // Let only the private core deliver completed evidence.
                outcome = new CompletedPreparedTrialOutcome(readback, request, inputs, step)); // Mint the detached immutable envelope inside its inaccessible constructor.
            var legacy = await ExecuteCapturedPreparedAsync(request, inputs, token, completion).ConfigureAwait(false); // Reuse the exact existing lifecycle.
            token.ThrowIfCancellationRequested(); // Do not issue successful measured completion to a cancelled caller.
            if (outcome is null) // An error or missing owner capture cannot manufacture a measurement.
                return legacy.IsSuccess // Refuse even an unexpected successful core with no completed projection.
                    ? AttemptRecoveryRequired<CompletedPreparedTrialOutcome>("The prepared lifecycle produced no completed outcome.") // Keep incomplete state a refusal.
                    : legacy.MapError<CompletedPreparedTrialOutcome>(); // Preserve the current actual failure.
            if (outcome.PublishedCheckpointStep is not null && !legacy.IsSuccess) // Improved requires the successful original publication result.
                return legacy.MapError<CompletedPreparedTrialOutcome>(); // Never promote a partial positive projection.
            return OperationResult<CompletedPreparedTrialOutcome>.Ok(outcome); // Only the private owner capture establishes completion.
        } // Finish the measurement projection of the same core result.
    } // Finish the completed evidence envelope.

    private sealed class PreparedCompletionCapture( // Keep the projection callback out of caller-visible request/input DTOs.
        Action<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback, int?> capture) // Accept evidence only from the existing private lifecycle.
    { // No public or internal DTO factory can obtain this private capture.
        internal void Capture(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback readback, // Deliver the real freshly admitted completed owner.
            int? publishedStep) => capture(readback, publishedStep); // Carry the observed publication step without permitting publication.
    } // Finish the private invocation projection.

    private static async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>> // Share the current training and publication lifecycle between both entries.
        TrainAndPublishCoreAsync( // Execute the ordinary or prepared request through the same owner.
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Retain the complete current publication request.
            CodeDocumentationPreparedBaselineAttemptInputs? preparedInputs, // Distinguish reuse of a prepared original from ordinary baseline calibration.
            PreparedInvocationCapture? capture, // Carry the immutable invocation snapshot and its source guard.
            CancellationToken cancellationToken,
            PreparedCompletionCapture? completion = null) // The private projection never enters caller inputs.
    {
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
        {
            return validation.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var scientificAdmission = AdmitScientificRoute(request, cancellationToken); // A durable intent blocks every ordinary/direct resume, including incomplete holds.
        if (!scientificAdmission.IsSuccess) return scientificAdmission.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Refuse before START, calibration or optimizer effects.

        if (request.AdapterSeedAuthority is { } publicationAuthority &&
            request.StartRequest.AdapterSeedAuthority is { } startAuthority &&
            (publicationAuthority.RootInitializationSeed !=
                startAuthority.RootInitializationSeed ||
             !StringComparer.Ordinal.Equals(
                 publicationAuthority.PolicyDropoutStreamDomain,
                 startAuthority.PolicyDropoutStreamDomain) ||
             !StringComparer.Ordinal.Equals(
                 publicationAuthority.ReferenceDropoutStreamDomain,
                 startAuthority.ReferenceDropoutStreamDomain)))
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>.Fail(
                "The training and START requests declare different adapter seed authorities.",
                AdapterSeedAuthorityMismatchErrorCode);
        }

        var adapterSeedAuthority = request.AdapterSeedAuthority ??
            request.StartRequest.AdapterSeedAuthority;
        request = request with
        {
            AdapterSeedAuthority = adapterSeedAuthority,
            StartRequest = request.StartRequest with
            {
                AdapterSeedAuthority = adapterSeedAuthority
            }
        };

        // Resolve the RNG together with the canonical optimizer source before
        // START opens a checkpoint. This keeps a continuation from constructing
        // a seeded topology first and discovering later that its random cursor
        // was unavailable or came from a different accepted-parent attempt.
        var sourceState = ResolveOptimizerAndRandomStateBeforePrepareStart(
            request,
            cancellationToken);
        if (!sourceState.IsSuccess)
        {
            return sourceState.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        request = request with
        {
            OptimizerStateToRestore = sourceState.Value?.OptimizerState ??
                request.OptimizerStateToRestore,
            RandomStateToRestore = sourceState.Value?.RandomState ??
                request.RandomStateToRestore,
            ContinuationProof = sourceState.Value?.Proof
        };

        var reopenedDpo = ReopenAndValidateRepositoryOwnedDpo(request);
        if (!reopenedDpo.IsSuccess || reopenedDpo.Value is null)
        {
            return reopenedDpo.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        if (request.Scientific is null && reopenedDpo.Value.Manifest.RequestIdentity.Schema == RepositoryOwnedDpoDatasetManifestSealer.RawRequestIdentitySchema) // Actual raw identity cannot bypass science by omitting a marker on a direct coordinator call.
            return OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>.Fail("Raw first-RP01 training requires the explicit scientific hold/publish route.", ScientificHoldRequired); // Refuse before START, C0 or optimizer work.

        var singlePair = SelectSingleTrainingPair(
            reopenedDpo.Value,
            request.RoutingRun.RepositoryOwnedDpoReceipt!);
        if (!singlePair.IsSuccess || singlePair.Value is null)
            return singlePair.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();

        if (request.QualityGate is null)
        {
            return Invalid<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var qualityPlan =
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .PlanQualityGate(
                    request.QualityGate!.Corpus,
                    request.QualityGate.GenerationSettings,
                    request.QualityGate.EvaluatorConfiguration,
                    request.QualityGate.NoisePolicyConfig);
        if (!qualityPlan.IsSuccess)
        {
            return qualityPlan.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        // Exact current-plan capacity is proved before optimizer/model mutation.
        // An ORIGINAL admission alone does not cover the longer Trial run IDs.
        if (preparedInputs is not null && (CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .CompleteTrialUpperBoundBytes(request.QualityGate!) is null ||
                BuildQualityAssessmentRunId(request.TrainingRunId, "trial").Length > 256 - "-repeat-000".Length))
            return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();

        var start =
            CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .PrepareStart(request.StartRequest, cancellationToken);
        if (!start.IsSuccess || start.Value is null)
        {
            if (preparedInputs is not null)
            {
                var knownAttempt = CreateTrainingAttemptAddress(request.StartRequest, request.TrainingRunId);
                if (knownAttempt.IsSuccess && knownAttempt.Value is not null &&
                    TrainingAttemptAlreadyExistsForResume(knownAttempt.Value).Value)
                    return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(start.ErrorMessage);
            }
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>
                .Fail(start.ErrorMessage, StartFailedErrorCode);
        }
        if (start.Value.ParentManifest is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>
                .Fail(
                    "CodeDocumentation repository-owned DPO quality-gated training requires a previously accepted parent checkpoint; cold start cannot be accepted as an improvement baseline.",
                    ParentCheckpointRequiredErrorCode);
        }

        var alreadyApplied = PairAlreadyAppliedInVerifiedLineage(
            request.StartRequest,
            start.Value.ParentManifest,
            singlePair.Value.ManifestPair);
        if (!alreadyApplied.IsSuccess)
        {
            return alreadyApplied.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }
        if (alreadyApplied.Value)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>
                .Fail(
                    "The sealed CodeDocumentation DPO pair was already applied in the verified accepted model lineage; the route refuses to train it again.",
                    PairAlreadyAppliedErrorCode);
        }

        var trainingAttemptAddress = CreateTrainingAttemptAddress(
            request.StartRequest,
            request.TrainingRunId);
        if (!trainingAttemptAddress.IsSuccess ||
            trainingAttemptAddress.Value is null)
        {
            return trainingAttemptAddress.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var invocationLease = request.Scientific?.Store.AcquireScientificInvocationLease(); // Source/legal/parent and exact pair admission above precede this filesystem effect.
        if (invocationLease is not null && (!invocationLease.IsSuccess || invocationLease.Value is null)) return invocationLease.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Fail concurrent same-run fresh/recovery/publish requests closed.
        using var scientificInvocationLease = invocationLease?.Value; // Retain the distinct per-run lease across every await, return and cancellation below.
        var existingAttempt = TrainingAttemptAlreadyExistsForResume(
            trainingAttemptAddress.Value);
        if (!existingAttempt.IsSuccess)
        {
            return existingAttempt.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }
        if (preparedInputs is not null && (request.OptimizerStateSourceAttempt is not null || // Require independent source validation only for a privately prepared warm continuation.
            start.Value.ParentManifest.ParentCheckpoint is not null)) // A trained accepted parent also requires canonical source S even when the caller omits its source attempt.
        { // Capture warm source identity after the AlreadyApplied refusal and before any recovery or new update.
            // A warm prepared recovery must independently prove its pre-update
            // source S; the saved attempt below is the distinct post-update T.
            var sourceRequest = request with // Retain source S separately from the later saved post-update Trial T.
            { // Detach mutable continuation state while preserving every request value and owner address.
                OptimizerStateToRestore = request.OptimizerStateToRestore is null ? null : // Keep absent caller optimizer state absent; copy present source state by its portable bytes.
                    AdamWPortableState.FromBytes(request.OptimizerStateToRestore.ToBytes()), // Reconstruct a separate AdamW source snapshot so later caller mutation cannot change validation.
                RandomStateToRestore = request.RandomStateToRestore is null ? null : DetachValue(request.RandomStateToRestore) // Detach the full source RNG graph while retaining null continuation semantics.
            }; // Finish the detached source request used by all subsequent readonly owner checks.
            var parentBytes = ValueBytes(start.Value.ParentManifest); // Pin the complete accepted-parent manifest rather than only its checkpoint hash.
            var dpoBytes = ValueBytes(reopenedDpo.Value.Manifest); // Pin every current lesson dataset manifest value for later owner readback comparison.
            var pairBytes = ValueBytes(singlePair.Value.ManifestPair); // Pin the complete selected lesson pair so the callback cannot admit a different lesson.
            OperationResult<bool> CurrentSource() => ValidatePreparedWarmSourceContinuation( // Define an invocation-local readonly source check without invoking capture guards recursively.
                sourceRequest, parentBytes, dpoBytes, pairBytes, cancellationToken); // Bind the detached source, complete metadata snapshots and current cancellation to that check.
            var currentSource = CurrentSource(); // Prove source S through its existing owners before binding or reopening Trial T.
            if (!currentSource.IsSuccess) // Refuse continuation when source owner readback or the captured parent and lesson no longer match.
                return existingAttempt.Value // Choose refusal semantics according to whether this training attempt already exists.
                    ? AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(currentSource.ErrorMessage) // Preserve RecoveryRequired when durable attempt effects already exist and source proof fails.
                    : currentSource.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Propagate the actual source refusal for a fresh attempt before any training effect.
            capture!.BindPreparedWarmSourceContinuation(CurrentSource); // Bind the already validated source check once to the private prepared invocation capture.
        } // Finish warm-only admission before the existing attempt recovery or fresh-run branches.
        if (request.Scientific is { ResumeBaseline: true } && existingAttempt.Value) return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>("Partial-baseline recovery refuses an existing canonical attempt; reconcile its saved outcome."); // Never fall through from this explicit pre-executor route to a second update.
        if (existingAttempt.Value)
        {
            return await ResumeQualityDecisionFromExistingAttemptAsync(
                    request,
                    reopenedDpo.Value,
                    singlePair.Value,
                    start.Value,
                    trainingAttemptAddress.Value,
                    cancellationToken, preparedInputs, capture, completion) // Preserve the private outcome projection during recovery.
                .ConfigureAwait(false);
        }

        var siblingReservation = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal // Preserve published refusal and same-run recovery before admitting a fresh baseline.
            .RequireNoConflictingSiblingAttempt(trainingAttemptAddress.Value, // Discover only sibling attempts in this canonical model/version.
                [singlePair.Value.ManifestPair.PairId], singlePair.Value.ManifestPair.PairFileSha256, cancellationToken); // Bind the current sealed pair, not the new run ID alone.
        if (!siblingReservation.IsSuccess) // Unknown or already reserved pair outcomes must be reconciled before new baseline/effect work.
            return siblingReservation.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Expose the existing recovery-required refusal to the ordinary consumer.

        var attemptPreflight = TrainingAttemptIsFreshForNewQualityRun(
            trainingAttemptAddress.Value);
        if (!attemptPreflight.IsSuccess)
        {
            return attemptPreflight.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var acceptedPassport = CreateAcceptedPassport(
            start.Value.ParentManifest,
            request.StartRequest.MaximumCheckpointBytes);
        if (request.Scientific is { } scientific) // Persist preregistered hold intent before the first C0 generation or optimizer effect.
        {
            var intent = BeginScientificHold(request, start.Value.ParentManifest!, reopenedDpo.Value.Manifest, singlePair.Value.ManifestPair, cancellationToken); // Bind actual admitted parent, lesson and all evaluation conditions.
            if (!intent.IsSuccess) return intent.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // A failed or changed intent cannot start evaluation.
            request = request with { QualityGate = request.QualityGate! with { PrivateAnswerCapture = (ICodeDocumentationPrivateAnswerCapture?)scientific.Recovery ?? scientific.Store } }; // Retain every exact baseline/trial answer in the configured private owner.
            if (scientific.Recovery is { } recovery) // Fully prevalidate all saved evaluator evidence before the first new generation or external dispatch.
            { // The same assessment owner validates both cached and current checker results.
                var retained = await CodeDocumentationSavedModelAssessmentService.ValidateRetainedBaselineAsync(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BuildAssessmentRequest(request.QualityGate!, start.Value.Model, acceptedPassport, BuildQualityAssessmentRunId(request.TrainingRunId, "baseline")), recovery, cancellationToken).ConfigureAwait(false); // This branch has no permit issuance or result write.
                if (!retained.IsSuccess) return retained.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // A malformed later slot cannot be bypassed by earlier missing slots.
            } // Calibration/executor begin only after the complete original inventory is admitted.
        }
        var qualityGate =
            new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService();
        OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality> baselineQuality; // Hold the original measured quality for either existing entry path.
        if (preparedInputs is not null) // Reuse the original only when the existing prepared inputs were supplied.
        { // Admit the prepared original before constructing any training request.
            if (capture?.IsCurrentForPreparedContinuation() != true) return ChangedPreparedInputs(); // Reject changed prepared inputs before reading the attempt binding.
            var prepared = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal // Read prepared attempt evidence through the existing baseline journal owner.
                .ValidatePreparedAsync(trainingAttemptAddress.Value, preparedInputs, cancellationToken) // Validate the same attempt address and prepared input binding under cancellation.
                .ConfigureAwait(false); // Await the original readback without a synchronization-context dependency.
            if (capture.IsCurrentForPreparedContinuation() != true) return ChangedPreparedInputs(); // Recheck mutable sources immediately after the journal await.
            if (!prepared.IsSuccess || prepared.Value is null) // Require an actual successful prepared attempt readback.
                return prepared.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Preserve the prepared journal failure without entering training.
            var lessonMatches = BaselineAttemptStateMatchesCurrentLesson(prepared.Value.State, // Bind the retained original to the current single lesson.
                start.Value.ParentManifest, request.DpoDatasetAddress.ManifestRelativePath, // Check the accepted parent and current dataset manifest address.
                reopenedDpo.Value.Manifest, singlePair.Value.ManifestPair); // Check the reopened dataset and actual single manifest pair.
            if (!lessonMatches.IsSuccess) // Refuse an original that belongs to a different lesson.
                return lessonMatches.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Return the existing lesson-binding error to the publication caller.
            baselineQuality = OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality> // Keep the normal baseline-quality result container for the reused original.
                .Ok(prepared.Value.State.BaselineQuality); // Reuse the journal's complete measured baseline without new checker calls.
        } // Finish admission of the existing prepared original.
        else // Retain ordinary calibration when no prepared original was supplied.
        { // Run the existing ordinary baseline path in its original scope.
            baselineQuality = await qualityGate // Measure the accepted model only for the ordinary entry.
            .CalibrateBaselineAsync(
                request.QualityGate,
                start.Value.Model,
                acceptedPassport,
                BuildQualityAssessmentRunId(
                    request.TrainingRunId,
                    "baseline"),
                cancellationToken)
            .ConfigureAwait(false);
        } // Finish the ordinary baseline calibration branch.
        if (!baselineQuality.IsSuccess || baselineQuality.Value is null)
        {
            return baselineQuality.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        if (request.Scientific is { } completedBaseline) request = request with { QualityGate = request.QualityGate! with { PrivateAnswerCapture = completedBaseline.Store } }; // C0 reuse ends here; Trial always uses ordinary generation with create-only evidence retention.

        var baselineNeedsPair =
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
                .BaselineRequiresNewTrainingPair(baselineQuality.Value);
        if (!baselineNeedsPair.IsSuccess)
        {
            return baselineNeedsPair.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var reference = CloneModelBeforeMutation(
            start.Value.Model,
            request.LoraConfig,
            request.AdapterSeedAuthority);
        if (!reference.IsSuccess || reference.Value is null)
        {
            return reference.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var optimizerStateToRestore = ResolveFreshRunOptimizerState(
            request,
            start.Value,
            reopenedDpo.Value,
            singlePair.Value,
            cancellationToken);
        if (!optimizerStateToRestore.IsSuccess)
        {
            return optimizerStateToRestore.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var baselineAttemptState = preparedInputs is not null ? null : CreateBaselineAttemptState( // Leave prepared attempt-state creation to its journal owner.
            trainingAttemptAddress.Value,
            acceptedPassport,
            request.DpoDatasetAddress.ManifestRelativePath,
            reopenedDpo.Value.Manifest,
            singlePair.Value.ManifestPair,
            baselineQuality.Value);
        var trainingRequest = new CodeDocumentationServerOwnedDpoTrainingRequest( // Construct the existing single-step server-owned training request.
                        [singlePair.Value.Pair],
                        start.Value.Model,
                        reference.Value,
                        request.Tokenizer,
                        request.LoraConfig,
                        trainingAttemptAddress.Value,
                        request.LeaseBroker,
                        request.TrainingRunId,
                        request.DpoStepCount,
                        baselineAttemptState,
                        OptimizerStateToRestore: optimizerStateToRestore.Value,
                        AdapterSeedAuthority: request.AdapterSeedAuthority,
                         SeededTopologyPreparedByRepositoryStart:
                             start.Value.SeededTopologyPreparedByRepositoryStart,
                        RandomStateToRestore: request.RandomStateToRestore,
                        RandomStateColdStartAuthorization:
                            request.RandomStateColdStartAuthorization,
                        OptimizerStateColdStartAuthorization:
                            request.OptimizerStateColdStartAuthorization,
                        ContinuationProof: request.ContinuationProof); // Preserve canonical source proof through the actual executor.
        if (capture is not null && !capture.IsCurrentForPreparedContinuation()) return ChangedPreparedInputs(); // Reject source changes immediately before executor entry.
        var executor = new CodeDocumentationServerOwnedDpoTrainingExecutor(); // Use the existing server-owned training executor for both routes.
        var training = preparedInputs is null // Select the executor entry from the presence of prepared original inputs.
            ? await executor.ExecuteAsync(trainingRequest, cancellationToken).ConfigureAwait(false) // Keep the ordinary executor lifecycle when no prepared original exists.
            : await executor.ExecuteFromPreparedBaselineAsync(trainingRequest, preparedInputs, // Route the prepared request through the executor's prepared-original entry.
                cancellationToken).ConfigureAwait(false); // Await the prepared execution with the existing cancellation token.
        // A callback can have changed caller state after a real update. Keep the
        // committed continuation; this refusal makes no zero-effect assertion.
        if (capture is not null && !capture.IsCurrentForPreparedContinuation()) // Detect invocation changes after the executor may have applied weights.
            return ChangedPreparedInputs(afterExecutor: true); // Require recovery instead of asserting zero effect after executor entry.
        if (!training.IsSuccess || training.Value is null)
        {
            if (training.ErrorCode == TrainingAttemptRecoveryRequiredErrorCode) // A sibling reservation can appear after the coordinator's read-only precheck.
                return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(training.ErrorMessage); // Preserve the executor's recovery refusal instead of hiding it as generic training failure.
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>
                .Fail(training.ErrorMessage, TrainingFailedErrorCode);
        }

        var trialCheckpoint = ReopenTrialCheckpointFromAttempt(
            request.StartRequest,
            trainingAttemptAddress.Value,
            training.Value.DpoStepCount,
            cancellationToken);
        if (!trialCheckpoint.IsSuccess || trialCheckpoint.Value is null)
        {
            return trialCheckpoint.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var trialPassport = CreateTrialPassport(
            request.StartRequest,
            start.Value.ParentManifest,
            trialCheckpoint.Value);
        if (!trialPassport.IsSuccess || trialPassport.Value is null)
        {
            return trialPassport.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        string? completedTrialSha256 = null;
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback? preparedTrial = null;
        OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence> qualityDecision;
        if (request.Scientific is not null) // The explicit route retains complete positive or negative measurements and stops before canonical publication.
            return await CompleteScientificHoldAsync(request, start.Value.ParentManifest!, reopenedDpo.Value.Manifest, singlePair.Value.ManifestPair, baselineQuality.Value, trialCheckpoint.Value, trialPassport.Value, training.Value, cancellationToken).ConfigureAwait(false); // Reuse the sole existing trial evaluation, never an extra baseline stage.
        if (preparedInputs is not null)
        {
            var durable = await EvaluatePreparedTrialDurablyAsync(request, preparedInputs, capture!, trainingAttemptAddress.Value,
                start.Value.ParentManifest, reopenedDpo.Value.Manifest, singlePair.Value.ManifestPair,
                baselineQuality.Value, trialCheckpoint.Value, trialPassport.Value, cancellationToken).ConfigureAwait(false);
            if (!durable.IsSuccess || durable.Value is null) return durable.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
            completedTrialSha256 = durable.Value.CompletedSha256;
            preparedTrial = durable.Value;
            qualityDecision = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.MapCompletedTrial(durable.Value.Detailed);
        }
        else qualityDecision = await qualityGate.EvaluateTrialAsync(
                request.QualityGate,
                baselineQuality.Value,
                trialCheckpoint.Value.Model,
                trialPassport.Value,
                BuildQualityAssessmentRunId(
                    request.TrainingRunId,
                    "trial"),
                singlePair.Value.ManifestPair.PairId,
                cancellationToken)
            .ConfigureAwait(false);
        if (capture is not null && !capture.IsCurrentForPreparedContinuation()) // Recheck prepared inputs after trial-checkpoint readback.
            return ChangedPreparedInputs(afterExecutor: true); // Retain the applied attempt when post-executor inputs changed.
        if (!qualityDecision.IsSuccess || qualityDecision.Value is null)
        {
            if (completion is not null && preparedTrial is not null && IsCompletedNegative(preparedTrial.Detailed.Disposition))
            {
                var fresh = await ReopenCompletedNegativeAsync(request, preparedInputs!, capture!, trainingAttemptAddress.Value,
                    start.Value.ParentManifest, reopenedDpo.Value.Manifest, singlePair.Value.ManifestPair,
                    baselineQuality.Value, trialCheckpoint.Value, trialPassport.Value, preparedTrial, cancellationToken).ConfigureAwait(false);
                if (!fresh.IsSuccess || fresh.Value is null) return fresh.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
                completion.Capture(fresh.Value, null);
            }
            return qualityDecision.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var publicationStart = CreatePublicationStartFromReloadedTrial(
            start.Value,
            trialCheckpoint.Value);
        var dpoIdentity = new RepositoryOwnedDpoDatasetManifestIdentity(
            request.DpoDatasetAddress.ManifestRelativePath,
            reopenedDpo.Value.Manifest.ManifestSha256);
        if (capture is not null && !capture.IsCurrentForPreparedContinuation()) // Recheck source identity after the quality-assessment await.
            return ChangedPreparedInputs(afterExecutor: true); // Require recovery for changes during quality assessment.
        if (preparedInputs is not null) // Add prepared-original freshness validation before publication.
        { // Read current durable evidence for the prepared publication path.
            var freshPublicationInputs = await ValidatePreparedBeforePublicationAsync(request, preparedInputs, // Validate complete prepared inputs through the existing publication guard.
                capture!, trainingAttemptAddress.Value, start.Value.ParentManifest, // Bind capture, attempt and accepted parent to the same current operation.
                reopenedDpo.Value.Manifest, singlePair.Value.ManifestPair, baselineQuality.Value!, // Bind the reopened lesson pair and complete original quality graph.
                trialCheckpoint.Value, cancellationToken, expectedCompletedSha256: completedTrialSha256).ConfigureAwait(false); // Require the exact receipt consumed above.
            if (!freshPublicationInputs.IsSuccess) // Require successful durable freshness validation before publishing.
                return freshPublicationInputs.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Return the validation failure while retaining the training attempt.
        } // Finish the prepared prepublication guard.
        var currentScienceGuard = AdmitScientificRoute(request, cancellationToken); // Recheck durable HOLD after measuring awaits, including a concurrently created intent.
        if (!currentScienceGuard.IsSuccess) return currentScienceGuard.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // An ordinary route never gains publication after a science reservation appears.
        var publication =
            CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                .Publish(
                    request.StartRequest,
                    publicationStart,
                    dpoIdentity,
                    request.CreatedAtUtc,
                    request.ProvenanceRefs,
                    cancellationToken);
        if (!publication.IsSuccess || publication.Value is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>
                .Fail(publication.ErrorMessage, PublishFailedErrorCode);
        }

        if (completion is not null && preparedTrial is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!capture!.IsCurrent()) return ChangedPreparedInputs(afterExecutor: true);
            completion.Capture(preparedTrial, publication.Value.Step);
        }
        return OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>.Ok(
            new CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence(
                request.DpoDatasetAddress.ManifestRelativePath,
                reopenedDpo.Value.Manifest.ManifestSha256,
                1,
                start.Value.ParentManifest?.Step,
                publication.Value.Step,
                publication.Value.RepositoryRelativeWeightsPath,
                publication.Value.RepositoryRelativeManifestPath,
                training.Value,
                publication.Value,
                BuildEvidenceRefs(
                    publicationStart,
                    publication.Value,
                    qualityDecision.Value),
                qualityDecision.Value,
                request.OptimizerStateColdStartAuthorization?.Reason));
    }

    private static async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>> // Adapt a captured prepared invocation to the shared producer lifecycle.
        ExecuteCapturedPreparedAsync(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Accept the complete publication request before making the snapshot.
            CodeDocumentationPreparedBaselineAttemptInputs inputs, CancellationToken cancellationToken,
            PreparedCompletionCapture? completion = null) // Keep outcome minting private to the same captured invocation.
    { // Validate and detach the existing prepared invocation metadata.
        var valid = ValidateRequest(request); // Run the existing publication-request admission first.
        if (!valid.IsSuccess) // Stop capture when the current publication request is invalid.
            return valid.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Return the existing request validation error unchanged.
        cancellationToken.ThrowIfCancellationRequested(); // Preserve cancellation before inspecting or copying prepared inputs.
        if (inputs is null || request.DpoStepCount != 1 || request.QualityGate is null) // Require prepared inputs, exactly one step and the server quality context.
            return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Reject requests outside the admitted one-lesson prepared entry.
        try // Contain nonfatal metadata capture errors within ordinary operation failures.
        { // Create and validate the prepared invocation snapshot.
            var capture = new PreparedInvocationCapture(request, inputs); // Detach current values and retain a guard on their source objects.
            if (!capture.IsCurrent() || !PreparedInputsMatchRequest(capture.Request, capture.Inputs)) // Require the snapshot to remain current and match the prepared request.
                return ChangedPreparedInputs(); // Reject mismatched or changed sources before producer entry.
            return await TrainAndPublishCoreAsync(capture.Request, capture.Inputs, capture, // Run the shared lifecycle with the detached request and prepared original.
                cancellationToken, completion).ConfigureAwait(false); // Await the same producer and its private projection.
        } // Finish the guarded snapshot and producer-entry region.
        catch (OperationCanceledException) { throw; } // Keep cancellation observable to the prepared caller.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Convert only nonfatal capture exceptions into the existing invalid result.
        { // Refuse malformed capture data without claiming training effects.
            return Invalid<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Return the current invalid-publication error on capture failure.
        } // Finish the nonfatal capture failure branch.
    } // Finish captured prepared invocation execution.

    private static OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence> // Return the existing publication evidence failure shape for source changes.
        ChangedPreparedInputs(bool afterExecutor = false) // Distinguish changes before training from changes after executor entry.
        => OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>.Fail( // Build the existing failure result without granting publication authority.
            afterExecutor // Choose the message from the actual executor-entry boundary.
                ? "Prepared invocation inputs changed after executor entry or during recovery; retain the attempt and require recovery. No zero-effect claim is made." // Require recovery when the applied attempt may already have effects.
                : "The current publication request and prepared original inputs do not match or changed before training.", // Explain mismatched inputs before any training execution.
            afterExecutor ? TrainingAttemptRecoveryRequiredErrorCode : InvalidErrorCode); // Use the existing recovery or invalid error code for the corresponding boundary.

    private static bool PreparedInputsMatchRequest( // Compare prepared original admission with the current publication request.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Read the current server-owned request values for binding.
        CodeDocumentationPreparedBaselineAttemptInputs inputs) // Read the prepared original's existing owner-bound inputs.
        => inputs.Reference.Classification == "NEEDS_LESSON" && // Allow this entry only for an original that needs one lesson.
           inputs.Reference.RepositoryRoot == request.StartRequest.RepositoryRoot && // Require the prepared original and request to share the repository root.
           ValueBytes(request.StartRequest.ModelConfig).AsSpan().SequenceEqual(ValueBytes(inputs.ModelConfig)) && // Bind the full current model configuration by its serialized value.
           ValueBytes(request.StartRequest.RuntimeCompatibility).AsSpan().SequenceEqual(ValueBytes(inputs.RuntimeCompatibility)) && // Bind the complete runtime compatibility contract by serialized value.
           ValueBytes(request.DpoDatasetAddress).AsSpan().SequenceEqual(ValueBytes(inputs.LessonDatasetAddress)) && // Require the current lesson dataset address to match prepared admission.
           inputs.LessonExpectedRequestIdentity.RequestIdentitySha256 == // Use the prepared lesson's expected request identity.
               request.RoutingRun.RepositoryOwnedDpoReceipt!.RequestIdentitySha256 && // Bind that identity to the actual routing receipt.
           QualityInputsSha256(request.QualityGate!) == QualityInputsSha256(inputs.QualityGateContext) && // Require the complete server exam inputs to match the original admission.
           TokenizerSha256(request.Tokenizer) == TokenizerSha256(inputs.QualityGateContext.Tokenizer); // Require byte-identical tokenizer state for the current lesson.

    private static byte[] ValueBytes<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value); // Serialize complete metadata values for equality and detached capture.

    private static OperationResult<bool> ValidatePreparedWarmSourceContinuation( // Revalidate canonical warm source S through existing readonly owners at continuation boundaries.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Use the detached source request so mutable caller continuation DTOs cannot become new authority.
        byte[] parentBytes, byte[] dpoBytes, byte[] pairBytes, CancellationToken token) // Compare complete captured parent and lesson bytes under the invocation cancellation token.
    { // Read current owner state without acquiring a lease, evaluating or training.
        // Only existing readonly owners participate. In particular, this helper
        // never calls capture freshness methods, lease acquisition or training.
        token.ThrowIfCancellationRequested(); // Keep caller cancellation observable before any source owner readback.
        var currentStart = CodeDocumentationRepositoryOwnedModelCheckpointStartService.PrepareStart(request.StartRequest, token); // Reopen the accepted parent through its existing checkpoint-start owner.
        if (!currentStart.IsSuccess || currentStart.Value?.ParentManifest is null || // Require a successful current start and a real accepted-parent manifest.
            !ValueBytes(currentStart.Value.ParentManifest).AsSpan().SequenceEqual(parentBytes)) // Compare every accepted-parent manifest byte with the admission snapshot.
            return OptimizerStateRequired<bool>("The accepted parent changed during prepared source continuation validation."); // Refuse a changed or unreadable accepted parent instead of substituting a cold source.
        var currentDpo = ReopenAndValidateRepositoryOwnedDpo(request); // Reopen and validate the current lesson through the repository-owned DPO owner.
        if (!currentDpo.IsSuccess || currentDpo.Value is null) // Require the lesson owner to return a complete successful dataset readback.
            return currentDpo.MapError<bool>(); // Preserve the actual dataset-owner refusal rather than inventing a replacement lesson.
        var currentPair = SelectSingleTrainingPair(currentDpo.Value, request.RoutingRun.RepositoryOwnedDpoReceipt!); // Select the same single lesson pair using the request repository-owned receipt.
        if (!currentPair.IsSuccess || currentPair.Value is null) // Require the selected pair owner to return its complete successful readback.
            return currentPair.MapError<bool>(); // Preserve the actual pair-selection refusal before optimizer or RNG admission.
        if (!ValueBytes(currentDpo.Value.Manifest).AsSpan().SequenceEqual(dpoBytes) || // Require the complete current dataset manifest to equal the captured lesson manifest.
            !ValueBytes(currentPair.Value.ManifestPair).AsSpan().SequenceEqual(pairBytes)) // Also require the complete selected pair metadata to equal the captured pair.
            return OptimizerStateRequired<bool>("The current lesson changed during prepared source continuation validation."); // Refuse any changed lesson identity before accepting its source continuation.
        var paired = ResolveOptimizerAndRandomStateBeforePrepareStart(request, token); // Revalidate paired optimizer and RNG source authority through the existing continuation resolver.
        if (!paired.IsSuccess) return paired.MapError<bool>(); // Propagate paired-source refusal without replacing missing, mixed or foreign source state.
        request = request with { ContinuationProof = paired.Value?.Proof };
        // This includes the full source baseline/manifest/runtime contract and
        // actual source policy == current accepted Parent, including S bytes.
        var optimizer = ResolveFreshRunOptimizerState(request, currentStart.Value, currentDpo.Value, currentPair.Value, token); // Recheck full source baseline, runtime, optimizer and policy equality with the accepted parent.
        return optimizer.IsSuccess ? OperationResult<bool>.Ok(true) : optimizer.MapError<bool>(); // Admit unchanged source S only when the existing fresh-run optimizer owner validates it.
    } // Finish readonly source validation without changing accepted state or durable artifacts.
    private static T DetachValue<T>(T value) // Detach mutable serialized metadata while preserving its values.
        => JsonSerializer.Deserialize<T>(ValueBytes(value)) ?? throw new InvalidOperationException("A captured value is required."); // Require a real reconstructed captured value rather than a missing snapshot.

    private static string TokenizerSha256(BpeTokenizer tokenizer) // Fingerprint actual tokenizer serialization for invocation identity.
    { // Read tokenizer state without mutating its source.
        using var bytes = new MemoryStream(); // Hold tokenizer serialization in a bounded invocation-local memory stream.
        tokenizer.Save(bytes); // Use the tokenizer's existing serialization format for the fingerprint.
        return Convert.ToHexString(SHA256.HashData(bytes.ToArray())).ToLowerInvariant(); // Return the actual serialized tokenizer SHA in canonical lowercase.
    } // Finish the tokenizer fingerprint calculation.

    private static BpeTokenizer DetachTokenizer(BpeTokenizer tokenizer) // Detach the tokenizer from the caller's mutable instance.
    { // Reconstruct the tokenizer through its normal serialization owner.
        using var bytes = new MemoryStream(); // Hold the tokenizer's normal serialized bytes in memory.
        tokenizer.Save(bytes); // Save the source tokenizer's actual current state.
        bytes.Position = 0; // Rewind the serialized bytes for normal tokenizer loading.
        return BpeTokenizer.Load(bytes); // Load a separate tokenizer instance with equivalent serialized state.
    } // Finish the detached tokenizer readback.

    private static string QualityInputsSha256(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context) // Fingerprint the complete server-owned exam metadata for capture validation.
        => Convert.ToHexString(SHA256.HashData(ValueBytes(new // Hash the full serialized exam identity and tokenizer state.
        { // Select every current exam field used by prepared admission.
            context.Corpus, context.CorpusManifest, context.EvaluatorConfiguration, // Bind the full corpus, sealed manifest and evaluator configuration.
            context.GenerationSettings, context.NoisePolicyConfig, context.MaximumDuration, // Bind generation, uncertainty policy and the exam deadline.
            context.ObservedAtUtc, context.EvidenceRefs, Tokenizer = TokenizerSha256(context.Tokenizer) // Bind observation time, evidence references and actual tokenizer state.
        }))).ToLowerInvariant(); // Return the complete exam-input SHA in canonical lowercase.

    private static CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext DetachQualityContext( // Detach only the mutable exam members before producer execution.
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context) // Retain the existing server exam context and its ownership.
        // Corpus/manifest/configuration are immutable factory-owned values. The
        // actual evaluation capability stays live; its answers are still checked
        // by the quality owner. Mutable tokenizer/evidence storage is detached.
        => context with { Tokenizer = DetachTokenizer(context.Tokenizer), EvidenceRefs = context.EvidenceRefs.ToArray() }; // Copy tokenizer state and evidence arrays while preserving every other exam value.

    private static string PreparedInvocationSha256(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Fingerprint the complete prepared invocation and current grant resolution.
        CodeDocumentationPreparedBaselineAttemptInputs inputs) // Include the prepared original's owner-bound inputs in that fingerprint.
    { // Resolve all current values before capture or freshness comparison.
        var reference = inputs.Reference; // Read the current durable prepared-original reference.
        var grant = inputs.Catalog.Resolve(inputs.Selector); // Resolve the current lesson selector through its existing catalog.
        if (!grant.IsSuccess || grant.Value is null) throw new InvalidOperationException("The current grant is unavailable."); // Require a present admitted grant before fingerprinting invocation authority.
        return Convert.ToHexString(SHA256.HashData(ValueBytes(new // Hash the complete serialized prepared invocation snapshot.
        { // Include every mutable metadata source guarded by this capture.
            request.RoutingRun, request.DpoDatasetAddress, request.StartRequest, request.LoraConfig, // Bind routing, dataset address, accepted start and training LoRA configuration.
            request.TrainingRunId, request.DpoStepCount, request.CreatedAtUtc, request.ProvenanceRefs, // Bind run identity, one-step request, timestamp and provenance references.
            Quality = QualityInputsSha256(request.QualityGate!), Tokenizer = TokenizerSha256(request.Tokenizer), // Bind full exam inputs and actual tokenizer state.
            Optimizer = request.OptimizerStateToRestore?.ToBytes(), request.OptimizerStateSourceAttempt, // Bind optimizer bytes and their original attempt source.
            request.OptimizerStateColdStartAuthorization, request.AdapterSeedAuthority, // Bind explicit optimizer cold-start and adapter-seed authorities.
            request.RandomStateToRestore, request.RandomStateColdStartAuthorization, // Bind random-state continuation and its explicit cold-start authority.
            Reference = new { reference.FormatVersion, reference.RepositoryRoot, reference.ObjectiveSha256, // Bind the prepared reference format, repository and original objective.
                reference.BaselineRoot, reference.BaselineRootRelativePath, reference.BaselinePath, // Bind the exact prepared baseline root and path.
                reference.ClaimPath, reference.LockPath, reference.RequestSha256, reference.Sha256, // Bind claim and lock addresses plus request and original content SHAs.
                reference.ByteLength, reference.Classification }, // Bind original byte length and needs-lesson classification.
            Grant = grant.Value, inputs.Catalog.MaxTeacherCalls, inputs.Selector, // Bind the resolved full grant, catalog ceiling and selected lesson.
            inputs.ModelConfig, inputs.RuntimeCompatibility, // Bind complete prepared model and runtime compatibility values.
            PreparedQuality = QualityInputsSha256(inputs.QualityGateContext), // Bind the original prepared exam context through its complete fingerprint.
            inputs.OriginalLoraConfig, inputs.OriginalAdapterSeeds, // Keep nullable original LoRA and adapter seeds distinct from training inputs.
            inputs.LessonDatasetAddress, inputs.LessonExpectedRequestIdentity // Bind the current complete lesson address and expected request identity.
        }))).ToLowerInvariant(); // Return the full invocation fingerprint in canonical lowercase.
    } // Finish prepared invocation fingerprinting.

    // Invocation-local observation guard, not a permission or a workflow owner.
    // It compares caller metadata around awaits and the real broker boundary.
    private sealed class PreparedInvocationCapture // Own a detached invocation and a guard on its original source objects.
    { // Keep capture state local to one prepared producer invocation.
        private readonly CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest _source; // Retain the source request for post-await mutation checks.
        private readonly CodeDocumentationPreparedBaselineAttemptInputs _sourceInputs; // Retain original prepared inputs for post-await mutation checks.
        private readonly string _sourceSha256; // Keep the source invocation SHA as the immutable comparison anchor.
        private Func<OperationResult<bool>>? _preparedWarmSourceContinuation; // Retain the private readonly warm source validator for this invocation after independent admission.
        internal bool HasPreparedWarmSourceContinuation => _preparedWarmSourceContinuation is not null; // Expose only whether warm source proof is bound for the narrowly admitted saved-Trial RNG readback.
        internal CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest Request { get; } // Expose the detached publication request to the existing shared owner.
        internal CodeDocumentationPreparedBaselineAttemptInputs Inputs { get; } // Expose detached prepared inputs without replacing their owner.
        internal string HistoricalInvocationSha256 => _sourceSha256; // Audit-only outside this invocation.

        internal PreparedInvocationCapture(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Capture the current request before executor or lease acquisition.
            CodeDocumentationPreparedBaselineAttemptInputs inputs) // Capture the existing owner-bound prepared original inputs.
        { // Snapshot values and prove the detached snapshot is equivalent.
            _source = request; // Remember the current request object for later freshness checks.
            _sourceInputs = inputs; // Remember current prepared input objects for later freshness checks.
            _sourceSha256 = PreparedInvocationSha256(request, inputs); // Fingerprint all source invocation values before detaching them.
            var grant = inputs.Catalog.Resolve(inputs.Selector).Value!; // Reuse the actual currently resolved prepared lesson grant.
            Inputs = inputs with // Create detached prepared inputs with the same admitted values.
            { // Copy only mutable members of the prepared input snapshot.
                Catalog = CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([grant], inputs.Catalog.MaxTeacherCalls), // Detach the admitted grant while preserving the catalog teacher-call ceiling.
                ModelConfig = inputs.ModelConfig with { }, // Copy the original complete model configuration value.
                OriginalLoraConfig = inputs.OriginalLoraConfig is null ? null : inputs.OriginalLoraConfig with // Preserve nullable original LoRA rather than inventing training LoRA authority.
                    { TargetModules = inputs.OriginalLoraConfig.TargetModules.ToArray() }, // Detach original LoRA target modules when original LoRA exists.
                QualityGateContext = DetachQualityContext(inputs.QualityGateContext), // Detach mutable tokenizer and evidence references in the original exam context.
                LessonExpectedRequestIdentity = DetachValue(inputs.LessonExpectedRequestIdentity) // Detach the current complete lesson request identity.
            }; // Finish the detached prepared-input value snapshot.
            Request = request with // Create the detached publication request for producer execution.
            { // Copy mutable request members without changing their values.
                RoutingRun = DetachValue(request.RoutingRun), // Detach the complete routing graph and its repository receipt.
                StartRequest = request.StartRequest with // Copy accepted-start metadata into the detached request.
                { // Detach mutable values of the accepted-start request.
                    ModelConfig = request.StartRequest.ModelConfig with { }, // Copy the complete accepted model configuration value.
                    LoraConfig = request.StartRequest.LoraConfig! with // Copy the required training LoRA configuration separately from original LoRA.
                        { TargetModules = request.StartRequest.LoraConfig!.TargetModules.ToArray() } // Detach training target modules from the caller's mutable array.
                }, // Finish the detached accepted-start request.
                Tokenizer = DetachTokenizer(request.Tokenizer), // Load a separate tokenizer instance with identical current serialization.
                LoraConfig = request.LoraConfig with { TargetModules = request.LoraConfig.TargetModules.ToArray() }, // Detach the active training LoRA target-module array.
                QualityGate = DetachQualityContext(request.QualityGate!), // Detach mutable exam members for the trial assessment.
                ProvenanceRefs = request.ProvenanceRefs.ToArray(), // Copy provenance references so source edits cannot alter execution metadata.
                OptimizerStateToRestore = request.OptimizerStateToRestore is null ? null : // Preserve absent optimizer continuation or detach its actual portable bytes.
                    AdamWPortableState.FromBytes(request.OptimizerStateToRestore.ToBytes()) ?? // Reconstruct optimizer state through its existing byte owner.
                        throw new InvalidOperationException("The captured optimizer bytes are invalid."), // Reject invalid captured optimizer bytes before executor entry.
                RandomStateToRestore = request.RandomStateToRestore is null ? null : DetachValue(request.RandomStateToRestore), // Detach present random-state continuation while preserving null cold-start semantics.
                LeaseBroker = new CapturedPreparedLeaseBroker(request.LeaseBroker, this) // Guard the existing training lease owner with this invocation capture.
            }; // Finish the detached publication request value snapshot.
            if (PreparedInvocationSha256(Request, Inputs) != _sourceSha256) // Require source and detached snapshots to have identical complete fingerprints.
                throw new InvalidOperationException("Capturing invocation metadata changed its values."); // Reject a capture operation that changed invocation values.
        } // Finish validated prepared invocation capture.

        internal bool IsCurrent() // Check whether original source values still match their capture.
        { // Recompute complete source identity safely after awaits and lease calls.
            try { return PreparedInvocationSha256(_source, _sourceInputs) == _sourceSha256; } // Compare current source invocation values with their original SHA.
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return false; } // Treat nonfatal fingerprinting failure as changed or unreadable source metadata.
        } // Finish the source freshness check.

        internal void BindPreparedWarmSourceContinuation(Func<OperationResult<bool>> currentSource) // Bind the canonical warm source validator to this prepared invocation exactly once.
        { // Protect the binding as an internal programming invariant rather than a business decision.
            if (_preparedWarmSourceContinuation is not null) // Detect an erroneous second binding that would replace the invocation source authority.
                throw new InvalidOperationException("Prepared warm source validation is already bound."); // Throw for the impossible duplicate-bind programming invariant, not for an ordinary owner refusal.
            _preparedWarmSourceContinuation = currentSource; // Retain the validated readonly callback without executing or changing its source owners.
        } // Finish the one-time private binding of warm continuation proof.

        internal bool IsCurrentForPreparedContinuation() // Combine original invocation metadata freshness with current canonical warm source readback.
        { // Use both guards before prepared effects, outcome readback and publication.
            if (!IsCurrent()) return false; // Refuse changed caller metadata before invoking the independent source-owner check.
            try { return _preparedWarmSourceContinuation?.Invoke().IsSuccess ?? true; } // Require bound warm owner proof; preserve the existing metadata-only path for other prepared invocations.
            catch (OperationCanceledException) { throw; } // Rethrow cancellation so a canceled invocation cannot become an ordinary stale-source result.
            catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) { return false; } // Conservatively refuse nonfatal unreadable source state without masking fatal exceptions.
        } // Finish the combined continuation guard without taking a lease or creating new effects.
    } // Finish the invocation snapshot and its source guard.

    // Preserve the real server-issued lease. If its callback changes invocation
    // metadata, dispose it before the existing executor can claim a fresh root.
    private sealed class CapturedPreparedLeaseBroker(IServerOwnedDpoTrainingLeaseBroker owner, // Wrap the existing lease owner without inventing another admission authority.
        PreparedInvocationCapture capture) : IServerOwnedDpoTrainingLeaseBroker // Bind lease acquisition to the current prepared invocation capture.
    { // Preserve existing lease ownership while checking invocation freshness.
        public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease> AcquireTranslationTrainingLease( // Retain the existing translation-training lease interface.
            DpoTrainingContentBatch content, string trainingRunId) // Forward translation content and run identity to their existing owner.
            => owner.AcquireTranslationTrainingLease(content, trainingRunId); // Preserve the translation lease behavior unchanged.

        public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease> AcquireCodeDocumentationTrainingLease( // Guard the existing CodeDocumentation training lease boundary.
            DpoTrainingContentBatch content, string trainingRunId) // Pass complete current content and run identity to the admission owner.
        { // Check capture freshness around the actual lease acquisition call.
            if (!capture.IsCurrentForPreparedContinuation()) return ChangedLease(); // Refuse changed metadata before asking the owner for a training lease.
            var acquired = owner.AcquireCodeDocumentationTrainingLease(content, trainingRunId); // Acquire the real lease from the existing training admission owner.
            if (capture.IsCurrentForPreparedContinuation()) return acquired; // Return the owner's actual result only while captured metadata remains current.
            acquired.Value?.Dispose(); // Dispose any acquired lease when source metadata changed during acquisition.
            return ChangedLease(); // Report the source change without transferring the invalidated lease.
        } // Finish the guarded CodeDocumentation lease acquisition.

        private static OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease> ChangedLease() // Return the existing lease result shape for invocation changes.
            => OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail( // Build an existing invalid failure without a usable training lease.
                "Prepared invocation metadata changed while acquiring the training lease.", InvalidErrorCode); // Explain that prepared metadata changed during lease acquisition.
    } // Finish the capture-aware wrapper around the existing lease owner.

    private static async Task<OperationResult<bool>> ValidatePreparedBeforePublicationAsync( // Revalidate complete durable prepared evidence immediately before publication.
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, // Use the current detached publication request for durable readback.
        CodeDocumentationPreparedBaselineAttemptInputs inputs, PreparedInvocationCapture capture, // Bind the prepared original inputs and their source guard.
        RepositoryOwnedModelTrainingAttemptAddress attempt, RepositoryOwnedModelCheckpointManifest? parent, // Bind the current applied attempt and accepted parent manifest.
        RepositoryOwnedDpoDatasetManifest dpo, RepositoryOwnedDpoDatasetManifestPair pair, // Bind the reopened full lesson dataset and single pair.
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality original, // Bind the original complete measured quality graph.
        CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad trial, CancellationToken cancellationToken,
        bool requireCompletedTrial = true, string? expectedCompletedSha256 = null) // Dispatch uses the same full continuation guard before the record exists.
    { // Read every durable continuation component before granting publication.
        if (!capture.IsCurrentForPreparedContinuation()) return AttemptRecoveryRequired<bool>("Prepared metadata changed before publication."); // Require recovery if invocation metadata changed before publication.
        var baseline = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal // Read the original through the current prepared baseline journal owner.
            .OpenPreparedExistingAsync(attempt, inputs, cancellationToken).ConfigureAwait(false); // Open the existing prepared binding at the applied attempt address.
        if (!capture.IsCurrentForPreparedContinuation() || !baseline.IsSuccess || baseline.Value is null) // Require successful current original readback after the await.
            return AttemptRecoveryRequired<bool>(baseline.ErrorMessage); // Preserve recovery semantics when the original cannot be freshly admitted.
        var current = ReopenAndValidateRepositoryOwnedDpo(request); // Reopen and validate the actual repository-owned lesson dataset.
        if (!current.IsSuccess || current.Value is null || // Require a successful complete current lesson readback.
            !ValueBytes(current.Value.Manifest).AsSpan().SequenceEqual(ValueBytes(dpo)) || // Compare every current dataset manifest byte with the captured lesson.
            !ValueBytes(baseline.Value.State.BaselineQuality).AsSpan().SequenceEqual(ValueBytes(original)) || // Compare every retained original quality-graph byte with the fresh journal original.
            !BaselineAttemptStateMatchesCurrentLesson(baseline.Value.State, parent, // Recheck accepted-parent and current-lesson binding through the existing helper.
                request.DpoDatasetAddress.ManifestRelativePath, dpo, pair).IsSuccess) // Include the actual manifest address and pair in that binding.
            return AttemptRecoveryRequired<bool>("The original, accepted parent or complete current lesson changed before publication."); // Require recovery when original, accepted parent or complete lesson changed.
        var marker = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile.OpenExisting(attempt, cancellationToken); // Open the existing optimizer continuation marker from the applied attempt.
        if (!marker.IsSuccess || marker.Value is null || // Require a successful current continuation-marker readback.
            !CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile // Use the existing optimizer manifest owner for baseline binding.
                .MatchesBaseline(marker.Value.Manifest, attempt, baseline.Value.State).IsSuccess) // Bind the actual marker to this attempt and freshly read original state.
            return AttemptRecoveryRequired<bool>("The continuation marker changed before publication."); // Require recovery when the continuation marker changed.
        var optimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(attempt.AttemptRoot, // Open the actual saved optimizer state from this attempt.
            marker.Value.Manifest.OptimizerStateStep, marker.Value.Manifest.OptimizerStateSha256, cancellationToken); // Verify its recorded step and SHA using the current optimizer owner.
        var reopenedTrial = ReopenTrialCheckpointFromAttempt(request.StartRequest, attempt, request.DpoStepCount, cancellationToken); // Reopen actual trial weights through the existing checkpoint owner.
        if (!optimizer.IsSuccess || optimizer.Value is null || // Require the complete optimizer readback to succeed.
            optimizer.Value.OptimizerStateByteLength != marker.Value.Manifest.OptimizerStateByteLength || // Require actual optimizer byte length to match the continuation marker.
            optimizer.Value.State.Step != marker.Value.Manifest.OptimizerStateStep || // Require actual optimizer step to match the continuation marker.
            optimizer.Value.State.FormatVersion != marker.Value.Manifest.OptimizerStateFormatVersion || // Require actual optimizer format to match the continuation marker.
            !reopenedTrial.IsSuccess || reopenedTrial.Value is null || // Require a successful actual trial-checkpoint readback.
            reopenedTrial.Value.WeightsSha256 != trial.WeightsSha256 || // Require reopened trial SHA to match the assessed checkpoint.
            reopenedTrial.Value.WeightsByteLength != trial.WeightsByteLength || // Require reopened trial byte length to match the assessed checkpoint.
            !ValidateTrialCheckpointAgainstManifest(marker.Value.Manifest, attempt, reopenedTrial.Value).IsSuccess || // Bind the reopened trial checkpoint to its current durable marker.
            !ValidateOptimizerStateRuntimeIdentity(marker.Value.Manifest, attempt, reopenedTrial.Value.Model, // Validate the complete optimizer parameter identity against the real trial model.
                request.StartRequest.ModelConfig, optimizer.Value.State).IsSuccess || // Use current model configuration and actual optimizer state for runtime binding.
            !OpenAndValidateExistingAttemptRandomState(request.StartRequest.ModelConfig, request.LoraConfig, // Read and validate actual continuation RNG through the existing owner.
                request.AdapterSeedAuthority, null, attempt, marker.Value.Manifest, // Bind current adapter seed authority and the same attempt marker.
                cancellationToken).IsSuccess || !capture.IsCurrentForPreparedContinuation()) // Require RNG validation and unchanged invocation metadata after all readbacks.
            return AttemptRecoveryRequired<bool>("The complete trial, optimizer or RNG changed before publication."); // Require recovery when complete trial, optimizer or RNG evidence changed.
        if (requireCompletedTrial)
        {
            var passport = CreateTrialPassport(request.StartRequest, parent, reopenedTrial.Value);
            if (!passport.IsSuccess || passport.Value is null) return AttemptRecoveryRequired<bool>(passport.ErrorMessage);
            var exam = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BuildAssessmentRequest(
                request.QualityGate!, reopenedTrial.Value.Model, passport.Value, BuildQualityAssessmentRunId(request.TrainingRunId, "trial"));
            var complete = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.OpenCompletedTrialAsync(
                attempt, inputs, exam, pair.PairId, cancellationToken).ConfigureAwait(false);
            if (!complete.IsSuccess || complete.Value?.Detailed.Disposition != CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Improved ||
                expectedCompletedSha256 is null || complete.Value.CompletedSha256 != expectedCompletedSha256 ||
                !capture.IsCurrentForPreparedContinuation()) return AttemptRecoveryRequired<bool>("The durable completed Trial changed before publication."); // Require both captured metadata and canonical source S to remain current after completed-Trial readback.
        }
        return OperationResult<bool>.Ok(true); // Admit publication only after the full current durable evidence readback.
    } // Finish complete prepared prepublication validation.

    private static bool IsCompletedNegative(CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition disposition)
        => disposition is CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Regressed
            or CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.Indistinguishable
            or CodeDocumentationRepositoryOwnedDpoTrainingQualityDisposition.IdenticalWeights;

    private static async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>>
        ReopenCompletedNegativeAsync(CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request,
            CodeDocumentationPreparedBaselineAttemptInputs inputs, PreparedInvocationCapture capture,
            RepositoryOwnedModelTrainingAttemptAddress attempt, RepositoryOwnedModelCheckpointManifest? parent,
            RepositoryOwnedDpoDatasetManifest dpo, RepositoryOwnedDpoDatasetManifestPair pair,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality original,
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad trial,
            CodeDocumentationSavedModelAssessmentModelPassport passport,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback expected,
            CancellationToken token)
    {
        var before = await ValidatePreparedBeforePublicationAsync(request, inputs, capture, attempt, parent, dpo, pair,
            original, trial, token, requireCompletedTrial: false).ConfigureAwait(false);
        if (!before.IsSuccess) return before.MapError<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>();
        var exam = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BuildAssessmentRequest(
            request.QualityGate!, trial.Model, passport, BuildQualityAssessmentRunId(request.TrainingRunId, "trial"));
        var opened = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.OpenCompletedTrialAsync(
            attempt, inputs, exam, pair.PairId, token).ConfigureAwait(false);
        if (!opened.IsSuccess || opened.Value is null) // Preserve actual owner refusal before the final current readbacks.
            return opened;
        var after = await ValidatePreparedBeforePublicationAsync(request, inputs, capture, attempt, parent, dpo, pair,
            original, trial, token, requireCompletedTrial: false).ConfigureAwait(false);
        if (!after.IsSuccess) // Do not let a valid completed record mask a changed continuation bundle.
            return after.MapError<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>();
        var current = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.OpenCompletedTrialAsync( // Read exact complete bytes again after the continuation await.
            attempt, inputs, exam, pair.PairId, token).ConfigureAwait(false); // Require the current complete owner at the final return boundary.
        if (!current.IsSuccess || current.Value is null || !after.IsSuccess || !capture.IsCurrentForPreparedContinuation() || // Require successful final negative owner readbacks and combined source freshness before returning the outcome.
            current.Value.CompletedSha256 != expected.CompletedSha256 || current.Value.StartedSha256 != expected.StartedSha256 ||
            current.Value.Detailed.Disposition != expected.Detailed.Disposition || !IsCompletedNegative(current.Value.Detailed.Disposition))
            return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>(
                "The durable completed negative Trial changed before outcome readback.");
        token.ThrowIfCancellationRequested();
        return current;
    }

    private static async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>> EvaluatePreparedTrialDurablyAsync(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request, CodeDocumentationPreparedBaselineAttemptInputs inputs,
        PreparedInvocationCapture capture, RepositoryOwnedModelTrainingAttemptAddress attempt,
        RepositoryOwnedModelCheckpointManifest parent, RepositoryOwnedDpoDatasetManifest dpo, RepositoryOwnedDpoDatasetManifestPair pair,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality original, CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad trial,
        CodeDocumentationSavedModelAssessmentModelPassport passport, CancellationToken token)
    {
        async Task<bool> Current() => capture.IsCurrentForPreparedContinuation() && (await ValidatePreparedBeforePublicationAsync(request, inputs, // Combine source S freshness with full durable continuation validation around prepared Trial dispatch.
            capture, attempt, parent, dpo, pair, original, trial, token, requireCompletedTrial: false).ConfigureAwait(false)).IsSuccess;
        if (!await Current().ConfigureAwait(false)) return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>("Prepared Trial inputs are not current before dispatch.");
        var exam = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BuildAssessmentRequest(
            request.QualityGate!, trial.Model, passport, BuildQualityAssessmentRunId(request.TrainingRunId, "trial"));
        var winner = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialDispatch.ClaimAsync(
            attempt, inputs, exam, capture.HistoricalInvocationSha256, token).ConfigureAwait(false);
        if (!winner.IsSuccess || winner.Value is null || !await Current().ConfigureAwait(false))
            return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>("Trial dispatch is already claimed, incomplete or changed.");
        var dispatch = await winner.Value.ConsumeDispatchAsync(attempt, inputs, exam, token).ConfigureAwait(false);
        if (!dispatch.IsSuccess || !capture.IsCurrentForPreparedContinuation()) // Refuse failed dispatch or changed metadata and source before invoking the Trial evaluator.
            return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>("Trial dispatch identity changed before evaluation.");
        var measured = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialDetailedAsync(
            request.QualityGate, original, trial.Model, passport, exam.AssessmentRunId, pair.PairId, token).ConfigureAwait(false);
        if (!capture.IsCurrentForPreparedContinuation()) return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>("Prepared Trial inputs changed after evaluation."); // Require recovery when the combined source guard changes across the evaluation await.
        if (!measured.IsSuccess || measured.Value is null) return measured.MapError<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>();
        if (!await Current().ConfigureAwait(false)) return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>("Prepared Trial inputs changed after evaluation.");
        var completed = await winner.Value.CompleteAsync(attempt, inputs, exam, measured.Value.Trial, pair.PairId, token).ConfigureAwait(false);
        if (!completed.IsSuccess || completed.Value is null || !await Current().ConfigureAwait(false))
            return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback>("Completed Trial was not durably re-admitted.");
        return completed;
    }

    private static OperationResult<bool> ValidateRequest(
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest? request)
    {
        if (request is null ||
            request.RoutingRun is null ||
            request.DpoDatasetAddress is null ||
            request.StartRequest is null ||
            request.Tokenizer is null ||
            request.LoraConfig is null ||
            request.StartRequest.LoraConfig is null ||
            request.LeaseBroker is null ||
            request.CreatedAtUtc.Offset != TimeSpan.Zero ||
            request.ProvenanceRefs is null ||
            request.ProvenanceRefs.Count == 0 ||
            string.IsNullOrWhiteSpace(request.TrainingRunId) ||
            request.RoutingRun.RepositoryOwnedDpoReceipt is null ||
            request.RoutingRun.RepositoryOwnedDpoPairs is null ||
            request.RoutingRun.RepositoryOwnedDpoPairs.Length == 0)
        {
            return Invalid<bool>();
        }

        var addressValid = RepositoryOwnedTrainingArtifactLayout
            .IsValidDpoDatasetAddress(request.DpoDatasetAddress);
        if (!addressValid ||
            !StringComparer.Ordinal.Equals(
                request.DpoDatasetAddress.RepositoryRoot,
                request.StartRequest.RepositoryRoot) ||
            !StringComparer.Ordinal.Equals(
                request.DpoDatasetAddress.DomainSlug,
                request.StartRequest.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                request.DpoDatasetAddress.DesignPatternSlug,
                request.StartRequest.DesignPatternSlug))
        {
            return Invalid<bool>();
        }

        if (!LoraConfigsMatch(request.LoraConfig, request.StartRequest.LoraConfig))
        {
            return Invalid<bool>();
        }

        // WHY: the caller must point at the same repository-owned DPO manifest
        // the routing run reported. A detached receipt cannot choose one dataset
        // while training silently consumes another.
        var receipt = request.RoutingRun.RepositoryOwnedDpoReceipt;
        if (string.IsNullOrWhiteSpace(receipt.ManifestSha256) ||
            string.IsNullOrWhiteSpace(receipt.RepositoryRelativeManifestPath) ||
            string.IsNullOrWhiteSpace(receipt.RequestIdentitySha256) ||
            receipt.PairCount <= 0 ||
            receipt.OrderedPairs is null ||
            receipt.OrderedPairs.Length == 0)
        {
            return Invalid<bool>();
        }

        return StringComparer.Ordinal.Equals(
                receipt.RepositoryRelativeManifestPath,
                request.DpoDatasetAddress.ManifestRelativePath)
            ? OperationResult<bool>.Ok(true)
            : Mismatch<bool>();
    }

    private static OperationResult<RepositoryOwnedDpoDatasetOpenResult>
        ReopenAndValidateRepositoryOwnedDpo(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request)
    {
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestIdentityOnly(request.DpoDatasetAddress);
        if (!identity.IsSuccess || identity.Value is null)
            return identity.MapError<RepositoryOwnedDpoDatasetOpenResult>();

        var opened = RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
            request.DpoDatasetAddress,
            identity.Value.RequestIdentity);
        if (!opened.IsSuccess || opened.Value is null)
            return opened;

        var receipt = request.RoutingRun.RepositoryOwnedDpoReceipt!;
        if (!ReceiptMatchesOpenedDataset(receipt, opened.Value) ||
            !SequenceEqual(
                request.RoutingRun.RepositoryOwnedDpoPairs,
                opened.Value.Pairs))
        {
            return Mismatch<RepositoryOwnedDpoDatasetOpenResult>();
        }

        return opened;
    }

    private static async Task<OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>>
        ResumeQualityDecisionFromExistingAttemptAsync(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request,
            RepositoryOwnedDpoDatasetOpenResult reopenedDpo,
            SingleRepositoryOwnedDpoTrainingPair singlePair,
            CodeDocumentationRepositoryOwnedModelCheckpointStart start,
            RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
            CancellationToken cancellationToken, // Keep cancellation on the existing attempt recovery route.
            CodeDocumentationPreparedBaselineAttemptInputs? preparedInputs = null, // Carry prepared original inputs when this is a prepared recovery.
            PreparedInvocationCapture? capture = null,
            PreparedCompletionCapture? completion = null) // Carry the private projection without changing recovery admission.
    {
        if (capture is not null && !capture.IsCurrentForPreparedContinuation()) // Check captured source freshness before reopening the applied attempt.
            return ChangedPreparedInputs(afterExecutor: true); // Require recovery for changed sources without claiming zero effect.
        Func<OperationResult<bool>>? currentOrdinaryWarmSource = null; // Keep ordinary source admission local to this recovery invocation.
        if (preparedInputs is null && (request.OptimizerStateSourceAttempt is not null || // An ordinary canonical source requires independent validation before saved Trial T.
            start.ParentManifest?.ParentCheckpoint is not null)) // A trained accepted parent also requires source S when the caller omitted it.
        { // Detach and prove the pre-update source without changing the post-update trial admission.
            var sourceRequest = request with // Preserve source S for every later revalidation rather than substituting saved Trial T.
            {
                OptimizerStateToRestore = request.OptimizerStateToRestore is null ? null : // Preserve absent caller state and detach present optimizer state by portable bytes.
                    AdamWPortableState.FromBytes(request.OptimizerStateToRestore.ToBytes()), // Prevent caller mutation from changing the captured optimizer source.
                RandomStateToRestore = request.RandomStateToRestore is null ? null : DetachValue(request.RandomStateToRestore) // Detach the complete caller source RNG graph.
            };
            var parentBytes = ValueBytes(start.ParentManifest); // Pin the complete accepted parent for the existing source validator.
            var dpoBytes = ValueBytes(reopenedDpo.Manifest); // Pin the complete current lesson manifest.
            var pairBytes = ValueBytes(singlePair.ManifestPair); // Pin the selected single lesson pair.
            OperationResult<bool> CurrentSource() => ValidatePreparedWarmSourceContinuation( // Reuse full parent, lesson, source-policy and closed-proof admission.
                sourceRequest, parentBytes, dpoBytes, pairBytes, cancellationToken);
            var currentSource = CurrentSource(); // Require successful source S admission before reading any saved Trial T state.
            if (!currentSource.IsSuccess || currentSource.Value != true) // Never treat absent or failed source proof as a cold recovery.
                return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(currentSource.ErrorMessage);
            currentOrdinaryWarmSource = CurrentSource; // Only successful independent admission permits distinct source and trial RNG steps.
        }
        var baseline = preparedInputs is null // Choose the existing baseline readback from the original entry path.
            ? CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal // Use the ordinary baseline journal entry for ordinary recovery.
                .OpenExisting(attemptAddress, cancellationToken) // Open the existing ordinary attempt original under cancellation.
            : await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal // Use the prepared journal readback for prepared recovery.
                .OpenPreparedExistingAsync(attemptAddress, preparedInputs, cancellationToken) // Reopen the current prepared original binding at the same attempt.
                .ConfigureAwait(false); // Await prepared readback before using its retained complete baseline.
        if (capture is not null && !capture.IsCurrentForPreparedContinuation()) // Recheck captured source metadata immediately after recovery readback.
            return ChangedPreparedInputs(afterExecutor: true); // Retain the attempt and require recovery on changed inputs.
        if (!baseline.IsSuccess || baseline.Value is null)
        {
            return AttemptRecoveryRequired<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(
                baseline.ErrorMessage);
        }

        var baselinePair =
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .MatchesSingleTrainingPair(
                    baseline.Value.State,
                    [singlePair.Pair]);
        if (!baselinePair.IsSuccess)
        {
            return AttemptRecoveryRequired<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(
                baselinePair.ErrorMessage);
        }

        var lessonMatches = BaselineAttemptStateMatchesCurrentLesson(
            baseline.Value.State,
            start.ParentManifest,
            request.DpoDatasetAddress.ManifestRelativePath,
            reopenedDpo.Manifest,
            singlePair.ManifestPair);
        if (!lessonMatches.IsSuccess)
        {
            return lessonMatches.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var optimizerStateManifest =
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                .OpenExisting(attemptAddress, cancellationToken);
        if (!optimizerStateManifest.IsSuccess ||
            optimizerStateManifest.Value is null)
        {
            return AttemptRecoveryRequired<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(
                optimizerStateManifest.ErrorMessage);
        }

        var manifestMatchesBaseline =
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                .MatchesBaseline(
                    optimizerStateManifest.Value.Manifest,
                    attemptAddress,
                    baseline.Value.State);
        if (!manifestMatchesBaseline.IsSuccess ||
            optimizerStateManifest.Value.Manifest.DpoStepCount !=
                request.DpoStepCount)
        {
            return AttemptRecoveryRequired<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(
                "The saved CodeDocumentation DPO optimizer-state commit marker does not match the current run, parent, pair, or requested DPO step.");
        }

        var optimizerState = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
            attemptAddress.AttemptRoot,
            optimizerStateManifest.Value.Manifest.OptimizerStateStep,
            optimizerStateManifest.Value.Manifest.OptimizerStateSha256,
            cancellationToken);
        if (!optimizerState.IsSuccess || optimizerState.Value is null ||
            optimizerState.Value.OptimizerStateByteLength !=
                optimizerStateManifest.Value.Manifest.OptimizerStateByteLength ||
            optimizerState.Value.State.Step !=
                optimizerStateManifest.Value.Manifest.OptimizerStateStep ||
            optimizerState.Value.State.FormatVersion !=
                optimizerStateManifest.Value.Manifest.OptimizerStateFormatVersion)
        {
            return AttemptRecoveryRequired<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(
                "The saved CodeDocumentation DPO optimizer state is missing, corrupt, or has the wrong hash, length, step, or format.");
        }

        // Same-run recovery is evaluation-only, but it still requires the
        // complete continuation bundle. Validate the repository-owned RNG bytes
        // before reopening the trial or allowing evaluation/publication to run.
        var randomState = OpenAndValidateExistingAttemptRandomState(
            request.StartRequest.ModelConfig,
            request.LoraConfig,
            request.AdapterSeedAuthority,
            capture?.HasPreparedWarmSourceContinuation == true || currentOrdinaryWarmSource is not null ? null : request.RandomStateToRestore, // Omit optional S/T equality only after independent warm source admission; retain root caller equality.
            attemptAddress,
            optimizerStateManifest.Value.Manifest,
            cancellationToken);
        if (!randomState.IsSuccess)
        {
            return AttemptRecoveryRequired<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(
                randomState.ErrorMessage);
        }

        var trialCheckpoint = ReopenTrialCheckpointFromAttempt(
            request.StartRequest,
            attemptAddress,
            request.DpoStepCount,
            cancellationToken);
        if (!trialCheckpoint.IsSuccess || trialCheckpoint.Value is null)
        {
            if (preparedInputs is not null) // Keep prepared recovery strict when the actual trial checkpoint cannot be reopened.
                return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>( // Return the existing recovery-required publication failure.
                    trialCheckpoint.ErrorMessage); // Preserve the actual checkpoint readback failure explanation.
            return trialCheckpoint.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var trialIdentity = ValidateTrialCheckpointAgainstManifest(
            optimizerStateManifest.Value.Manifest,
            attemptAddress,
            trialCheckpoint.Value);
        if (!trialIdentity.IsSuccess)
        {
            return AttemptRecoveryRequired<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(
                trialIdentity.ErrorMessage);
        }

        // The accepted START model is the adapter-free parent baseline. The
        // persisted optimizer state belongs to the already-applied trial
        // checkpoint, which must be reopened with its LoRA topology before
        // runtime parameter-shape validation can be meaningful.
        var recoveredRuntimeIdentity = ValidateOptimizerStateRuntimeIdentity(
            optimizerStateManifest.Value.Manifest,
            attemptAddress,
            trialCheckpoint.Value.Model,
            request.StartRequest.ModelConfig,
            optimizerState.Value.State);
        if (!recoveredRuntimeIdentity.IsSuccess)
        {
            return AttemptRecoveryRequired<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(
                recoveredRuntimeIdentity.ErrorMessage);
        }

        var trialPassport = CreateTrialPassport(
            request.StartRequest,
            start.ParentManifest,
            trialCheckpoint.Value);
        if (!trialPassport.IsSuccess || trialPassport.Value is null)
        {
            return trialPassport.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var trainingEvidence = BuildRecoveredTrainingEvidence(
            attemptAddress,
            request.DpoStepCount,
            baseline.Value,
            trialCheckpoint.Value,
            optimizerState.Value,
            optimizerStateManifest.Value,
            randomState.Value);
        if (!trainingEvidence.IsSuccess || trainingEvidence.Value is null)
        {
            return trainingEvidence.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        if (request.Scientific is not null) // Recovery has now passed the existing baseline/optimizer/RNG/trial/runtime validators.
            return PublishScientificHeld(request, start, reopenedDpo.Manifest, singlePair.ManifestPair, baseline.Value.State.BaselineQuality, trialCheckpoint.Value, trialPassport.Value, trainingEvidence.Value, cancellationToken); // Publish-only admission precedes the ordinary re-evaluation branch and performs no assessment.

        string? completedTrialSha256 = null;
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedTrialReadback? preparedTrial = null;
        OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence> qualityDecision;
        if (preparedInputs is not null)
        {
            var current = await ValidatePreparedBeforePublicationAsync(request, preparedInputs, capture!, attemptAddress,
                start.ParentManifest, reopenedDpo.Manifest, singlePair.ManifestPair, baseline.Value.State.BaselineQuality,
                trialCheckpoint.Value, cancellationToken, requireCompletedTrial: false).ConfigureAwait(false);
            if (!current.IsSuccess) return current.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
            var exam = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BuildAssessmentRequest(
                request.QualityGate!, trialCheckpoint.Value.Model, trialPassport.Value, BuildQualityAssessmentRunId(request.TrainingRunId, "trial"));
            var completed = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.OpenCompletedTrialAsync(
                attemptAddress, preparedInputs, exam, singlePair.ManifestPair.PairId, cancellationToken).ConfigureAwait(false);
            current = await ValidatePreparedBeforePublicationAsync(request, preparedInputs, capture!, attemptAddress,
                start.ParentManifest, reopenedDpo.Manifest, singlePair.ManifestPair, baseline.Value.State.BaselineQuality,
                trialCheckpoint.Value, cancellationToken, requireCompletedTrial: false).ConfigureAwait(false);
            if (!completed.IsSuccess || completed.Value is null || !current.IsSuccess || !capture!.IsCurrentForPreparedContinuation()) // Require complete durable Trial and current canonical source proof after both recovery readback awaits.
                return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>("The applied attempt has no complete matching durable Trial.");
            completedTrialSha256 = completed.Value.CompletedSha256;
            preparedTrial = completed.Value;
            qualityDecision = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.MapCompletedTrial(completed.Value.Detailed);
        }
        else qualityDecision = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService().EvaluateTrialAsync(
                    request.QualityGate,
                    baseline.Value.State.BaselineQuality,
                    trialCheckpoint.Value.Model,
                    trialPassport.Value,
                    BuildQualityAssessmentRunId(
                        request.TrainingRunId,
                        "trial"),
                    singlePair.ManifestPair.PairId,
                    cancellationToken)
                .ConfigureAwait(false);
        if (currentOrdinaryWarmSource is not null) // Reopen source S after the ordinary quality evaluation await.
        {
            var currentSource = currentOrdinaryWarmSource(); // Preserve the original S request and validate its current complete owner state.
            if (!currentSource.IsSuccess || currentSource.Value != true) // A changed or missing source leaves the applied trial recovery-required.
                return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(currentSource.ErrorMessage);
        }
        if (capture is not null && !capture.IsCurrentForPreparedContinuation()) // Check source freshness after resumed trial-checkpoint admission.
            return ChangedPreparedInputs(afterExecutor: true); // Require recovery if inputs changed during resumed trial readback.
        if (!qualityDecision.IsSuccess || qualityDecision.Value is null)
        {
            if (completion is not null && preparedTrial is not null && IsCompletedNegative(preparedTrial.Detailed.Disposition))
            {
                var fresh = await ReopenCompletedNegativeAsync(request, preparedInputs!, capture!, attemptAddress,
                    start.ParentManifest, reopenedDpo.Manifest, singlePair.ManifestPair, baseline.Value.State.BaselineQuality,
                    trialCheckpoint.Value, trialPassport.Value, preparedTrial, cancellationToken).ConfigureAwait(false);
                if (!fresh.IsSuccess || fresh.Value is null) return fresh.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
                completion.Capture(fresh.Value, null);
            }
            return qualityDecision.MapError<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>();
        }

        var publicationStart = CreatePublicationStartFromReloadedTrial(
            start,
            trialCheckpoint.Value);
        var dpoIdentity = new RepositoryOwnedDpoDatasetManifestIdentity(
            request.DpoDatasetAddress.ManifestRelativePath,
            reopenedDpo.Manifest.ManifestSha256);
        if (capture is not null && !capture.IsCurrentForPreparedContinuation()) // Check source freshness after resumed quality assessment.
            return ChangedPreparedInputs(afterExecutor: true); // Require recovery if inputs changed during the resumed assessment.
        if (preparedInputs is not null) // Apply full current durable validation to prepared recovery before publication.
        { // Read the complete retained original and continuation evidence again.
            var freshPublicationInputs = await ValidatePreparedBeforePublicationAsync(request, preparedInputs, // Invoke the same prepared prepublication guard for the recovered attempt.
                capture!, attemptAddress, start.ParentManifest, reopenedDpo.Manifest, // Bind capture, recovered attempt, accepted parent and current dataset.
                singlePair.ManifestPair, baseline.Value.State.BaselineQuality, // Bind the actual current pair and complete journal baseline graph.
                trialCheckpoint.Value, cancellationToken, expectedCompletedSha256: completedTrialSha256).ConfigureAwait(false); // Bind the exact consumed completed record.
            if (!freshPublicationInputs.IsSuccess) // Require successful complete freshness validation before resumed publication.
                return freshPublicationInputs.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Preserve the current validation failure without publishing changed evidence.
        } // Finish the prepared recovery prepublication guard.
        if (currentOrdinaryWarmSource is not null) // Revalidate ordinary source S immediately before resumed publication.
        {
            var currentSource = currentOrdinaryWarmSource(); // Require the same accepted parent, lesson and canonical source proof at publication.
            if (!currentSource.IsSuccess || currentSource.Value != true) // Do not publish a retained Trial T after source authority changes.
                return AttemptRecoveryRequired<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(currentSource.ErrorMessage);
        }
        var currentScienceGuard = AdmitScientificRoute(request, cancellationToken); // Recheck HOLD immediately before ordinary resumed publication, after all awaits.
        if (!currentScienceGuard.IsSuccess) return currentScienceGuard.MapError<CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>(); // Complete or partial scientific state cannot bypass through recovery.
        var publication =
            CodeDocumentationRepositoryOwnedModelCheckpointPublicationService
                .Publish(
                    request.StartRequest,
                    publicationStart,
                    dpoIdentity,
                    request.CreatedAtUtc,
                    request.ProvenanceRefs,
                    cancellationToken);
        if (!publication.IsSuccess || publication.Value is null)
        {
            return OperationResult<
                CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>
                .Fail(publication.ErrorMessage, PublishFailedErrorCode);
        }

        if (completion is not null && preparedTrial is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!capture!.IsCurrent()) return ChangedPreparedInputs(afterExecutor: true);
            completion.Capture(preparedTrial, publication.Value.Step);
        }
        return OperationResult<
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence>.Ok(
            new CodeDocumentationRepositoryOwnedDpoTrainingPublicationEvidence(
                request.DpoDatasetAddress.ManifestRelativePath,
                reopenedDpo.Manifest.ManifestSha256,
                1,
                start.ParentManifest?.Step,
                publication.Value.Step,
                publication.Value.RepositoryRelativeWeightsPath,
                publication.Value.RepositoryRelativeManifestPath,
                trainingEvidence.Value,
                publication.Value,
                BuildEvidenceRefs(
                    publicationStart,
                    publication.Value,
                    qualityDecision.Value),
                qualityDecision.Value));
    }

    private static OperationResult<SingleRepositoryOwnedDpoTrainingPair>
        SelectSingleTrainingPair(
            RepositoryOwnedDpoDatasetOpenResult opened,
            CodeDocumentationRepositoryOwnedDpoDatasetReceipt receipt)
    {
        if (opened.Manifest.PairCount != 1 ||
            opened.Manifest.OrderedPairs.Count != 1 ||
            opened.Pairs.Count != 1 ||
            receipt.PairCount != 1 ||
            receipt.OrderedPairs is null ||
            receipt.OrderedPairs.Length != 1 ||
            receipt.OrderedPairs[0] != opened.Manifest.OrderedPairs[0])
        {
            return OperationResult<SingleRepositoryOwnedDpoTrainingPair>.Fail(
                "CodeDocumentation repository-owned DPO quality gate requires exactly one sealed, explicitly identified new pair; ambiguous batches are rejected before model mutation.",
                SingleNewPairRequiredErrorCode);
        }

        return OperationResult<SingleRepositoryOwnedDpoTrainingPair>.Ok(
            new SingleRepositoryOwnedDpoTrainingPair(
                opened.Manifest.OrderedPairs[0],
                opened.Pairs[0]));
    }

    private static OperationResult<bool> PairAlreadyAppliedInVerifiedLineage(
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
        RepositoryOwnedModelCheckpointManifest parent,
        RepositoryOwnedDpoDatasetManifestPair candidatePair)
    {
        if (request is null ||
            parent is null ||
            candidatePair is null)
        {
            return Invalid<bool>();
        }

        var current = parent;
        var visitedSteps = new HashSet<int>();
        while (true)
        {
            if (!visitedSteps.Add(current.Step))
                return Invalid<bool>();

            var openedDpo = OpenCheckpointDpoManifest(
                request,
                current.DpoDatasetManifest);
            if (!openedDpo.IsSuccess || openedDpo.Value is null)
                return openedDpo.MapError<bool>();

            var applied = current.DpoApplication == RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate // Только роль из заново проверенной sealed истории означает provenance без optimizer update.
                ? OperationResult<bool>.Ok(false) // Исходный owner baseline не расходует выбранную первую учебную пару.
                : PairAlreadyAppliedByIdentity( // Любой обычный или legacy корень сохраняет прежний запрет повтора и changed-byte conflict.
                    openedDpo.Value.Manifest.OrderedPairs, candidatePair); // Сравниваем exact PairId и SHA уже применённых строк.
            if (!applied.IsSuccess)
                return applied;
            if (applied.Value)
            {
                return OperationResult<bool>.Ok(true);
            }

            if (current.ParentCheckpoint is null)
                return OperationResult<bool>.Ok(false);

            var parentAddress =
                RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                    request.RepositoryRoot,
                    request.DomainSlug,
                    request.DesignPatternSlug,
                    request.ModelIdSlug,
                    request.ModelVersion,
                    current.ParentCheckpoint.Step);
            if (!parentAddress.IsSuccess || parentAddress.Value is null)
                return parentAddress.MapError<bool>();

            var reopened =
                RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(
                    parentAddress.Value);
            if (!reopened.IsSuccess || reopened.Value is null ||
                !ParentIdentityMatchesOpened(
                    current.ParentCheckpoint,
                    reopened.Value.Manifest,
                    reopened.Value))
            {
                return Invalid<bool>();
            }

            current = reopened.Value.Manifest;
        }
    }

    private static OperationResult<RepositoryOwnedDpoDatasetOpenResult>
        OpenCheckpointDpoManifest(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            RepositoryOwnedDpoDatasetManifestIdentity identity)
    {
        var address = ResolveDpoDatasetAddressFromManifestIdentity(
            request,
            identity);
        if (!address.IsSuccess || address.Value is null)
            return address.MapError<RepositoryOwnedDpoDatasetOpenResult>();

        var opened = RepositoryOwnedDpoDatasetManifestSealer
            .OpenManifestIdentityOnly(address.Value);
        if (!opened.IsSuccess || opened.Value is null ||
            !StringComparer.Ordinal.Equals(
                opened.Value.Manifest.ManifestSha256,
                identity.ManifestSha256) ||
            !StringComparer.Ordinal.Equals(
                address.Value.ManifestRelativePath,
                identity.RepositoryRelativeManifestPath))
        {
            return Invalid<RepositoryOwnedDpoDatasetOpenResult>();
        }

        return OperationResult<RepositoryOwnedDpoDatasetOpenResult>.Ok(
            new RepositoryOwnedDpoDatasetOpenResult(
                opened.Value.Manifest,
                []));
    }

    private static OperationResult<RepositoryOwnedDpoDatasetAddress>
        ResolveDpoDatasetAddressFromManifestIdentity(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            RepositoryOwnedDpoDatasetManifestIdentity identity)
    {
        if (identity is null ||
            string.IsNullOrWhiteSpace(identity.RepositoryRelativeManifestPath))
        {
            return Invalid<RepositoryOwnedDpoDatasetAddress>();
        }

        var prefix =
            "training-data/dpo/" +
            request.DomainSlug +
            "/" +
            request.DesignPatternSlug +
            "/";
        const string suffix = "/manifest.json";
        var value = identity.RepositoryRelativeManifestPath;
        if (!value.StartsWith(prefix, StringComparison.Ordinal) ||
            !value.EndsWith(suffix, StringComparison.Ordinal))
        {
            return Invalid<RepositoryOwnedDpoDatasetAddress>();
        }

        var datasetVersion = value[
            prefix.Length..^suffix.Length];
        return RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            request.RepositoryRoot,
            request.DomainSlug,
            request.DesignPatternSlug,
            datasetVersion);
    }

    private static OperationResult<bool> PairAlreadyAppliedByIdentity(
        IReadOnlyList<RepositoryOwnedDpoDatasetManifestPair> lineagePairs,
        RepositoryOwnedDpoDatasetManifestPair candidatePair)
    {
        foreach (var lineagePair in lineagePairs)
        {
            if (!StringComparer.Ordinal.Equals(
                    lineagePair.PairId,
                    candidatePair.PairId))
            {
                continue;
            }

            if (StringComparer.Ordinal.Equals(
                    lineagePair.PairFileSha256,
                    candidatePair.PairFileSha256))
            {
                return OperationResult<bool>.Ok(true);
            }

            return OperationResult<bool>.Fail(
                "The accepted CodeDocumentation lineage already contains this DPO PairId with different sealed bytes; the route refuses to guess which lesson is authoritative.",
                AppliedPairIdentityConflictErrorCode);
        }

        return OperationResult<bool>.Ok(false);
    }

    private static bool ParentIdentityMatchesOpened(
        RepositoryOwnedModelCheckpointParentIdentity expected,
        RepositoryOwnedModelCheckpointManifest manifest,
        RepositoryOwnedModelCheckpointOpenResult opened)
        => manifest.Step == expected.Step &&
           StringComparer.Ordinal.Equals(
               manifest.RepositoryRelativeManifestPath,
               expected.RepositoryRelativeManifestPath) &&
           StringComparer.Ordinal.Equals(
               manifest.WeightsSha256,
               expected.CheckpointSha256) &&
           manifest.WeightsByteLength == expected.CheckpointByteLength &&
           StringComparer.Ordinal.Equals(
               opened.WeightsSha256,
               expected.CheckpointSha256) &&
           opened.WeightsByteLength == expected.CheckpointByteLength;

    private static CodeDocumentationSavedModelAssessmentModelPassport
        CreateAcceptedPassport(
            RepositoryOwnedModelCheckpointManifest parent,
            long maximumModelSerializationBytes)
        => new(
            parent.Domain,
            parent.DesignPattern,
            parent.ModelId,
            parent.Version,
            parent.Step,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256,
            parent.RepositoryRelativeWeightsPath,
            parent.WeightsSha256,
            parent.WeightsByteLength,
            maximumModelSerializationBytes,
            CodeDocumentationSavedModelAssessmentModelPassport
                .AcceptedRepositoryCheckpointSourceKind,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256);

    private static CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState
        CreateBaselineAttemptState(
            RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
            CodeDocumentationSavedModelAssessmentModelPassport acceptedPassport,
            string dpoManifestRelativePath,
            RepositoryOwnedDpoDatasetManifest dpoManifest,
            RepositoryOwnedDpoDatasetManifestPair trainingPair,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality
                baselineQuality)
        => new(
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .CurrentFormatVersion,
            attemptAddress.TrainingRunId,
            attemptAddress.TrainingRunIdSha256,
            attemptAddress.AttemptRootRelativePath,
            attemptAddress.AttemptClaimRelativePath,
            attemptAddress.DomainSlug,
            attemptAddress.DesignPatternSlug,
            attemptAddress.ModelIdSlug,
            attemptAddress.ModelVersion,
            acceptedPassport,
            dpoManifestRelativePath,
            dpoManifest.ManifestSha256,
            trainingPair.PairId,
            trainingPair.RepositoryRelativePairFilePath,
            trainingPair.PairFileSha256,
            baselineQuality);

    private static OperationResult<OptimizerStateSourceResolution>
        ResolveOptimizerAndRandomStateBeforePrepareStart(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.OptimizerStateSourceAttempt is null)
        {
            if (request.OptimizerStateToRestore is not null || request.RandomStateToRestore is not null)
                return ProfileMismatch<OptimizerStateSourceResolution>("Raw continuation state requires a canonical exact-profile source attempt.");
            return OperationResult<OptimizerStateSourceResolution>.Ok(
                new(null, null));
        }

        var sourceAttempt = request.OptimizerStateSourceAttempt;
        if (!RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptAddress(sourceAttempt) ||
            !StringComparerForPaths.Equals(
                sourceAttempt.RepositoryRoot,
                request.StartRequest.RepositoryRoot) ||
            !StringComparer.Ordinal.Equals(
                sourceAttempt.DomainSlug,
                request.StartRequest.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                sourceAttempt.DesignPatternSlug,
                request.StartRequest.DesignPatternSlug) ||
            !StringComparer.Ordinal.Equals(
                sourceAttempt.ModelIdSlug,
                request.StartRequest.ModelIdSlug) ||
            !StringComparer.Ordinal.Equals(
                sourceAttempt.ModelVersion,
                request.StartRequest.ModelVersion) ||
            StringComparer.Ordinal.Equals(
                sourceAttempt.TrainingRunId,
                request.TrainingRunId))
        {
            return OperationResult<OptimizerStateSourceResolution>.Fail(
                "The random-state source attempt is not a distinct canonical attempt in the requested repository model lineage.",
                RandomStateRestoreRequiredErrorCode);
        }

        var sourceManifest =
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                .OpenExisting(sourceAttempt, cancellationToken);
        if (!sourceManifest.IsSuccess || sourceManifest.Value is null)
        {
            return OperationResult<OptimizerStateSourceResolution>.Fail(
                sourceManifest.ErrorMessage,
                sourceManifest.ErrorCode == DpoExecutionProfile.MismatchErrorCode
                    ? DpoExecutionProfile.MismatchErrorCode : RandomStateRestoreRequiredErrorCode);
        }

        var runtimeRequiresRandomState =
            request.AdapterSeedAuthority is not null &&
            request.LoraConfig.Dropout > 0f &&
            request.LoraConfig.TargetModules.Length > 0;
        var manifest = sourceManifest.Value.Manifest;
        var sourceOptimizer = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
            sourceAttempt.AttemptRoot,
            manifest.OptimizerStateStep,
            manifest.OptimizerStateSha256,
            cancellationToken);
        if (!sourceOptimizer.IsSuccess || sourceOptimizer.Value is null ||
            sourceOptimizer.Value.State.Step != manifest.OptimizerStateStep ||
            sourceOptimizer.Value.State.FormatVersion !=
                manifest.OptimizerStateFormatVersion)
        {
            return OperationResult<OptimizerStateSourceResolution>.Fail(
                sourceOptimizer.ErrorMessage,
                RandomStateRestoreRequiredErrorCode);
        }

        if (request.OptimizerStateToRestore is not null &&
            !CryptographicOperations.FixedTimeEquals(
                request.OptimizerStateToRestore.ToBytes(),
                sourceOptimizer.Value.State.ToBytes()))
        {
            return OperationResult<OptimizerStateSourceResolution>.Fail(
                "The caller-supplied optimizer state does not match the canonical optimizer and RNG source attempt.",
                RandomStateSourceMismatchErrorCode);
        }

        if (!manifest.RandomStateRequired)
        {
            if (runtimeRequiresRandomState)
            {
                return OperationResult<OptimizerStateSourceResolution>.Fail(
                    "The accepted-parent optimizer source attempt has no repository-owned RNG state, but the requested seeded dropout topology requires continuation RNG.",
                    RandomStateRestoreRequiredErrorCode);
            }

            if (request.RandomStateToRestore is not null)
                return ProfileMismatch<OptimizerStateSourceResolution>("The canonical source has no RNG state.");
            return BindContinuationSource(sourceAttempt, sourceOptimizer.Value.State, null, cancellationToken);
        }

        if (!runtimeRequiresRandomState ||
            manifest.RandomStateStep <= 0 ||
            manifest.RandomStateFormatVersion !=
                LlmModelRandomStateSnapshot.CurrentFormatVersion ||
            string.IsNullOrWhiteSpace(manifest.RandomStateRelativePath) ||
            !StringComparer.Ordinal.Equals(
                manifest.RandomStateRelativePath,
                manifest.AttemptRootRelativePath + "/" +
                    RepositoryOwnedDpoRandomStateFile.BuildFileName(
                        manifest.RandomStateStep)))
        {
            return OperationResult<OptimizerStateSourceResolution>.Fail(
                "The optimizer source attempt contains RNG metadata that does not match the requested seeded topology.",
                RandomStateSourceMismatchErrorCode);
        }

        var canonicalLoraConfig = LoraApplicator.ValidateAndCanonicalize(
            request.LoraConfig);
        var expectedTopology = LoraRandomStateIdentity.ComputeTopologySha256(
            request.StartRequest.ModelConfig,
            canonicalLoraConfig,
            request.AdapterSeedAuthority!);
        var expectedConfiguration =
            LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
                canonicalLoraConfig,
                request.AdapterSeedAuthority!);
        if (!StringComparer.Ordinal.Equals(
                manifest.RandomStateTopologySha256,
                expectedTopology) ||
            !StringComparer.Ordinal.Equals(
                manifest.LoraConfigurationSha256,
                expectedConfiguration))
        {
            return OperationResult<OptimizerStateSourceResolution>.Fail(
                "The optimizer source attempt RNG identity does not match the current model, LoRA configuration, and seed authority.",
                RandomStateSourceMismatchErrorCode);
        }

        var randomStatePath = Path.Combine(
            sourceAttempt.AttemptRoot,
            RepositoryOwnedDpoRandomStateFile.BuildFileName(
                manifest.RandomStateStep));
        var sourceRandomState = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
            randomStatePath,
            manifest.RandomStateSha256,
            manifest.RandomStateByteLength,
            manifest.RandomStateStep,
            manifest.RandomStateTopologySha256,
            manifest.LoraConfigurationSha256);
        if (!sourceRandomState.IsSuccess || sourceRandomState.Value is null)
        {
            return OperationResult<OptimizerStateSourceResolution>.Fail(
                sourceRandomState.ErrorMessage,
                RandomStateRestoreRequiredErrorCode);
        }

        if (request.RandomStateToRestore is not null &&
            !RandomStateSnapshotsEqual(
                request.RandomStateToRestore,
                sourceRandomState.Value))
        {
            return OperationResult<OptimizerStateSourceResolution>.Fail(
                "The caller-supplied RNG state does not match the sealed canonical optimizer source attempt.",
                RandomStateSourceMismatchErrorCode);
        }

        return BindContinuationSource(sourceAttempt, sourceOptimizer.Value.State, sourceRandomState.Value, cancellationToken);
    }

    private static OperationResult<OptimizerStateSourceResolution> BindContinuationSource(
        RepositoryOwnedModelTrainingAttemptAddress address, AdamWPortableState optimizer,
        LlmModelRandomStateSnapshot? random, CancellationToken cancellationToken)
    {
        var proof = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
            .IssueContinuationProof(address, optimizer, random, cancellationToken);
        return proof.IsSuccess && proof.Value is not null
            ? OperationResult<OptimizerStateSourceResolution>.Ok(new(optimizer, random, proof.Value))
            : proof.MapError<OptimizerStateSourceResolution>();
    }

    private static OperationResult<T> ProfileMismatch<T>(string message)
        => OperationResult<T>.Fail(message, DpoExecutionProfile.MismatchErrorCode);

    private static bool RandomStateSnapshotsEqual(
        LlmModelRandomStateSnapshot left,
        LlmModelRandomStateSnapshot right)
    {
        if (left.FormatVersion != right.FormatVersion ||
            !StringComparer.Ordinal.Equals(left.Algorithm, right.Algorithm) ||
            !StringComparer.Ordinal.Equals(left.TopologySha256, right.TopologySha256) ||
            !StringComparer.Ordinal.Equals(
                left.LoraConfigurationSha256,
                right.LoraConfigurationSha256) ||
            left.RandomStateStep != right.RandomStateStep ||
            left.Entries.Count != right.Entries.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Entries.Count; index++)
        {
            var leftEntry = left.Entries[index];
            var rightEntry = right.Entries[index];
            if (!StringComparer.Ordinal.Equals(
                    leftEntry.AdapterKey,
                    rightEntry.AdapterKey) ||
                !StringComparer.Ordinal.Equals(
                    leftEntry.StreamDomain,
                    rightEntry.StreamDomain) ||
                leftEntry.InitializationSeed !=
                    rightEntry.InitializationSeed ||
                leftEntry.DropoutSeed != rightEntry.DropoutSeed ||
                leftEntry.ConsumedCursor != rightEntry.ConsumedCursor)
            {
                return false;
            }
        }

        return true;
    }

    internal static OperationResult<LlmModelRandomStateSnapshot?>
        OpenAndValidateExistingAttemptRandomState(
            ModelConfig modelConfig,
            LoraConfig loraConfig,
            LoraPairRandomSeedAuthority? adapterSeedAuthority,
            LlmModelRandomStateSnapshot? callerRandomState,
            RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest manifest,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var canonical = LoraApplicator.ValidateAndCanonicalize(loraConfig);
        var required = canonical.Dropout > 0f &&
                       canonical.TargetModules.Length > 0;
        if (!required)
        {
            return manifest.RandomStateRequired
                ? OperationResult<LlmModelRandomStateSnapshot?>.Fail(
                    "The saved attempt requires RNG continuation, but the current recovery topology does not provide the matching seeded dropout configuration.",
                    RandomStateSourceMismatchErrorCode)
                : OperationResult<LlmModelRandomStateSnapshot?>.Ok(null);
        }

        if (adapterSeedAuthority is null ||
            !manifest.RandomStateRequired ||
            manifest.RandomStateStep <= 0 ||
            manifest.RandomStateFormatVersion !=
                LlmModelRandomStateSnapshot.CurrentFormatVersion ||
            string.IsNullOrWhiteSpace(manifest.RandomStateRelativePath) ||
            !StringComparer.Ordinal.Equals(
                manifest.RandomStateRelativePath,
                manifest.AttemptRootRelativePath + "/" +
                    RepositoryOwnedDpoRandomStateFile.BuildFileName(
                        manifest.RandomStateStep)))
        {
            return OperationResult<LlmModelRandomStateSnapshot?>.Fail(
                "The same-run recovery attempt is missing a valid required RNG identity.",
                RandomStateRestoreRequiredErrorCode);
        }

        var expectedTopology = LoraRandomStateIdentity.ComputeTopologySha256(
            modelConfig, canonical, adapterSeedAuthority);
        var expectedConfiguration = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(
            canonical, adapterSeedAuthority);
        if (!StringComparer.Ordinal.Equals(manifest.RandomStateTopologySha256, expectedTopology) ||
            !StringComparer.Ordinal.Equals(manifest.LoraConfigurationSha256, expectedConfiguration))
        {
            return OperationResult<LlmModelRandomStateSnapshot?>.Fail(
                "The saved RNG identity does not match the current topology, LoRA configuration and seed authority.",
                RandomStateSourceMismatchErrorCode);
        }

        var randomStatePath = Path.GetFullPath(Path.Combine(
            attemptAddress.AttemptRoot,
            RepositoryOwnedDpoRandomStateFile.BuildFileName(
                manifest.RandomStateStep)));
        var opened = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
            randomStatePath,
            manifest.RandomStateSha256,
            manifest.RandomStateByteLength,
            manifest.RandomStateStep,
            manifest.RandomStateTopologySha256,
            manifest.LoraConfigurationSha256);
        cancellationToken.ThrowIfCancellationRequested();
        if (!opened.IsSuccess || opened.Value is null)
        {
            return OperationResult<LlmModelRandomStateSnapshot?>.Fail(
                string.IsNullOrWhiteSpace(opened.ErrorMessage)
                    ? "The required same-run recovery RNG file is missing or corrupt."
                    : opened.ErrorMessage,
                RandomStateRestoreRequiredErrorCode);
        }

        if (callerRandomState is not null &&
            !RandomStateSnapshotsEqual(
                callerRandomState,
                opened.Value))
        {
            return OperationResult<LlmModelRandomStateSnapshot?>.Fail(
                "The caller-supplied RNG state does not match the same-run recovery bundle.",
                RandomStateSourceMismatchErrorCode);
        }

        return OperationResult<LlmModelRandomStateSnapshot?>.Ok(opened.Value);
    }

    internal static OperationResult<bool> ValidateTrialCheckpointAgainstManifest(
        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest manifest,
        RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
        CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad trial)
    {
        var expectedRelativePath = manifest.AttemptRootRelativePath + "/" +
            DpoTrainer.BuildPolicyCheckpointFileName(manifest.DpoStepCount);
        var expectedAbsolutePath = Path.GetFullPath(Path.Combine(
            attemptAddress.AttemptRoot,
            DpoTrainer.BuildPolicyCheckpointFileName(manifest.DpoStepCount)));
        var matches = StringComparer.Ordinal.Equals(
                          manifest.TrialWeightsRelativePath,
                          expectedRelativePath) &&
                      StringComparer.Ordinal.Equals(
                          trial.RepositoryRelativeWeightsPath,
                          manifest.TrialWeightsRelativePath) &&
                      StringComparer.Ordinal.Equals(
                          trial.WeightsSha256,
                          manifest.TrialWeightsSha256) &&
                      trial.WeightsByteLength == manifest.TrialWeightsByteLength &&
                      StringComparerForPaths.Equals(
                          Path.GetFullPath(trial.WeightsPath),
                          expectedAbsolutePath);
        return matches
            ? OperationResult<bool>.Ok(true)
            : OperationResult<bool>.Fail(
                "The saved trial checkpoint does not match the immutable weights identity sealed with the optimizer/RNG bundle.",
                TrialCheckpointMismatchErrorCode);
    }

    private static OperationResult<AdamWPortableState?>
        ResolveFreshRunOptimizerState(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationRequest request,
            CodeDocumentationRepositoryOwnedModelCheckpointStart start,
            RepositoryOwnedDpoDatasetOpenResult reopenedDpo,
            SingleRepositoryOwnedDpoTrainingPair singlePair,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(reopenedDpo);
        ArgumentNullException.ThrowIfNull(singlePair);

        // A root accepted checkpoint is the only implicit optimizer cold-start
        // boundary. Once the accepted parent itself has a parent, a new DPO run
        // is a continuation and may not silently construct a fresh AdamW.
        var continuationRequired =
            start.ParentManifest?.ParentCheckpoint is not null;
        if (request.OptimizerStateSourceAttempt is null)
        {
            if (continuationRequired)
            {
                return OptimizerStateRequired<AdamWPortableState?>(
                    "The accepted CodeDocumentation parent is a continuation checkpoint, but no canonical optimizer-state source attempt was supplied.");
            }

            if (request.OptimizerStateToRestore is null)
            {
                // A root accepted checkpoint is not, by itself, authorization
                // to construct a fresh AdamW. The caller must name the explicit
                // initial-training path and leave an auditable reason.
                if (start.ParentManifest is null ||
                    request.OptimizerStateColdStartAuthorization is null ||
                    !request.OptimizerStateColdStartAuthorization.IsWellFormed)
                {
                    return OptimizerStateColdStartAuthorizationRequired<
                        AdamWPortableState?>();
                }

                return OperationResult<AdamWPortableState?>.Ok(null);
            }

            if (request.OptimizerStateColdStartAuthorization is not null)
            {
                return OptimizerStateColdStartAuthorizationInvalid<
                    AdamWPortableState?>();
            }

            return ProfileMismatch<AdamWPortableState?>("Raw optimizer state cannot replace canonical source proof.");
        }

        var sourceAttempt = request.OptimizerStateSourceAttempt;
        if (!RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptAddress(sourceAttempt) ||
            !StringComparerForPaths.Equals(
                sourceAttempt.RepositoryRoot,
                request.StartRequest.RepositoryRoot) ||
            !StringComparer.Ordinal.Equals(
                sourceAttempt.DomainSlug,
                request.StartRequest.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                sourceAttempt.DesignPatternSlug,
                request.StartRequest.DesignPatternSlug) ||
            !StringComparer.Ordinal.Equals(
                sourceAttempt.ModelIdSlug,
                request.StartRequest.ModelIdSlug) ||
            !StringComparer.Ordinal.Equals(
                sourceAttempt.ModelVersion,
                request.StartRequest.ModelVersion) ||
            StringComparer.Ordinal.Equals(
                sourceAttempt.TrainingRunId,
                request.TrainingRunId))
        {
            return OptimizerStateRequired<AdamWPortableState?>(
                "The optimizer-state source attempt is not a distinct canonical attempt in the requested repository model lineage.");
        }

        if (request.OptimizerStateColdStartAuthorization is not null)
        {
            return OptimizerStateColdStartAuthorizationInvalid<
                AdamWPortableState?>();
        }

        var sourceBaseline =
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(sourceAttempt, cancellationToken);
        if (!sourceBaseline.IsSuccess || sourceBaseline.Value is null)
        {
            return OptimizerStateRequired<AdamWPortableState?>(
                sourceBaseline.ErrorMessage);
        }

        var sourceManifest =
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                .OpenExisting(sourceAttempt, cancellationToken);
        if (!sourceManifest.IsSuccess || sourceManifest.Value is null)
        {
            return OptimizerStateRequired<AdamWPortableState?>(
                sourceManifest.ErrorMessage);
        }

        var sourceManifestMatchesBaseline =
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                .MatchesBaseline(
                    sourceManifest.Value.Manifest,
                    sourceAttempt,
                    sourceBaseline.Value.State);
        if (!sourceManifestMatchesBaseline.IsSuccess ||
            sourceManifestMatchesBaseline.Value != true ||
            sourceManifest.Value.Manifest.DpoStepCount <= 0 ||
            sourceManifest.Value.Manifest.OptimizerStateStep <= 0 ||
            sourceManifest.Value.Manifest.OptimizerStateStep <
                sourceManifest.Value.Manifest.DpoStepCount)
        {
            return OptimizerStateRequired<AdamWPortableState?>(
                "The optimizer-state source manifest does not match its sealed run, parent, pair, or step identity.");
        }

        var sourceState = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
            sourceAttempt.AttemptRoot,
            sourceManifest.Value.Manifest.OptimizerStateStep,
            sourceManifest.Value.Manifest.OptimizerStateSha256,
            cancellationToken);
        if (!sourceState.IsSuccess || sourceState.Value is null ||
            sourceState.Value.OptimizerStateByteLength !=
                sourceManifest.Value.Manifest.OptimizerStateByteLength ||
            sourceState.Value.State.Step !=
                sourceManifest.Value.Manifest.OptimizerStateStep ||
            sourceState.Value.State.FormatVersion !=
                sourceManifest.Value.Manifest.OptimizerStateFormatVersion ||
            !StringComparerForPaths.Equals(
                sourceState.Value.OptimizerStatePath,
                Path.GetFullPath(Path.Combine(
                    sourceAttempt.AttemptRoot,
                    RepositoryOwnedDpoOptimizerStateFile.BuildFileName(
                        sourceManifest.Value.Manifest.OptimizerStateStep)))) ||
            !StringComparer.Ordinal.Equals(
                sourceManifest.Value.Manifest.DpoManifestRelativePath,
                sourceBaseline.Value.State.DpoManifestRelativePath) ||
            !StringComparer.Ordinal.Equals(
                sourceManifest.Value.Manifest.DpoManifestSha256,
                sourceBaseline.Value.State.DpoManifestSha256) ||
            !StringComparer.Ordinal.Equals(
                sourceManifest.Value.Manifest.TrainingPairId,
                sourceBaseline.Value.State.TrainingPairId) ||
            !StringComparer.Ordinal.Equals(
                sourceManifest.Value.Manifest.TrainingPairRelativePath,
                sourceBaseline.Value.State.TrainingPairRelativePath) ||
            !StringComparer.Ordinal.Equals(
                sourceManifest.Value.Manifest.TrainingPairSha256,
                sourceBaseline.Value.State.TrainingPairSha256))
        {
            return OptimizerStateRequired<AdamWPortableState?>(
                "The optimizer-state source bytes are missing, corrupt, or disagree with their sealed manifest identity.");
        }

        var sourceRuntimeIdentity = ValidateOptimizerStateRuntimeIdentity(
            sourceManifest.Value.Manifest,
            sourceAttempt,
            start.Model,
            request.StartRequest.ModelConfig,
            sourceState.Value.State);
        if (!sourceRuntimeIdentity.IsSuccess)
        {
            return OptimizerStateRequired<AdamWPortableState?>(
                sourceRuntimeIdentity.ErrorMessage);
        }

        // The source attempt's durable policy must be the exact accepted parent
        // that the fresh trainer is about to continue. This binds optimizer
        // memory to model weights rather than merely trusting a matching hash.
        var sourcePolicy = HashTrialCheckpointFile(
            request.StartRequest,
            sourceAttempt,
            sourceManifest.Value.Manifest.DpoStepCount,
            Path.Combine(
                sourceAttempt.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(
                    sourceManifest.Value.Manifest.DpoStepCount)),
            cancellationToken);
        if (!sourcePolicy.IsSuccess || sourcePolicy.Value is null ||
            start.ParentManifest is null ||
            !StringComparer.Ordinal.Equals(
                sourcePolicy.Value.RepositoryRelativePath,
                sourceManifest.Value.Manifest.TrialWeightsRelativePath) ||
            !StringComparer.Ordinal.Equals(
                sourcePolicy.Value.Sha256,
                sourceManifest.Value.Manifest.TrialWeightsSha256) ||
            sourcePolicy.Value.ByteLength !=
                sourceManifest.Value.Manifest.TrialWeightsByteLength ||
            !StringComparer.Ordinal.Equals(
                sourcePolicy.Value.Sha256,
                start.ParentManifest.WeightsSha256) ||
            sourcePolicy.Value.ByteLength != start.ParentManifest.WeightsByteLength)
        {
            return OptimizerStateRequired<AdamWPortableState?>(
                "The optimizer-state source policy checkpoint is not the accepted parent for the requested continuation.");
        }

        var validatedState = ValidatePortableOptimizerState(
            sourceState.Value.State,
            sourceState.Value.State.Step);
        if (!validatedState.IsSuccess || validatedState.Value is null)
            return validatedState;

        try
        {
            if (request.ContinuationProof is null)
                return ProfileMismatch<AdamWPortableState?>("Canonical continuation proof is required.");
            request.ContinuationProof.RequireMatches(validatedState.Value, request.RandomStateToRestore);
        }
        catch (DpoMutationAdmissionException exception)
        {
            return OperationResult<AdamWPortableState?>.Fail(exception.Message, exception.ErrorCode);
        }

        if (request.OptimizerStateToRestore is not null &&
            !CryptographicOperations.FixedTimeEquals(
                request.OptimizerStateToRestore.ToBytes(),
                validatedState.Value.ToBytes()))
        {
            return OptimizerStateRequired<AdamWPortableState?>(
                "The caller-supplied optimizer state does not match the sealed canonical source bytes.");
        }

        // The current lesson is independently checked above by the reopened DPO
        // manifest and pair admission. Keep the arguments in this seam explicit
        // so a future continuation cannot accidentally bypass those checks while
        // changing only the optimizer source lookup.
        _ = reopenedDpo.Manifest.ManifestSha256;
        _ = singlePair.ManifestPair.PairId;
        return validatedState;
    }

    private static OperationResult<AdamWPortableState?>
        ValidatePortableOptimizerState(
            AdamWPortableState state,
            int expectedStep)
    {
        if (state is null ||
            state.FormatVersion != AdamWPortableState.CurrentFormatVersion ||
            expectedStep <= 0 ||
            state.Step != expectedStep)
        {
            return OptimizerStateRequired<AdamWPortableState?>(
                "The supplied optimizer state is empty, incompatible, or has the wrong format or step.");
        }

        return OperationResult<AdamWPortableState?>.Ok(state);
    }

    private static OperationResult<bool> ValidateOptimizerStateRuntimeIdentity(
        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifest manifest,
        RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
        LlmModel model,
        ModelConfig modelConfig,
        AdamWPortableState state)
    {
        try
        {
            var bindings = model.GetCheckpointParameterEntries()
                .Select(static entry =>
                    new AdamWParameterBinding(entry.Key, entry.Parameter))
                .ToArray();
            var matches =
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .MatchesRuntime(
                        manifest,
                        attemptAddress,
                        modelConfig,
                        bindings,
                        DpoTrainer.ProductionOptimizerSettings,
                        state);
            return matches.IsSuccess && matches.Value == true
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    "The persisted CodeDocumentation DPO optimizer state is incompatible with the current model shape, parameter map, or AdamW settings.",
                    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                        .InvalidErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "The persisted CodeDocumentation DPO optimizer state is incompatible with the current model shape, parameter map, or AdamW settings.",
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .InvalidErrorCode);
        }
    }

    private static OperationResult<CodeDocumentationSavedModelAssessmentModelPassport>
        CreateTrialPassport(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            RepositoryOwnedModelCheckpointManifest? parent,
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad
                trialCheckpoint)
    {
        if (request is null ||
            parent is null ||
            trialCheckpoint is null ||
            trialCheckpoint.WeightsByteLength <= 0 ||
            trialCheckpoint.WeightsByteLength >
            request.MaximumCheckpointBytes ||
            !IsSafeRepositoryRelativePath(
                trialCheckpoint.RepositoryRelativeWeightsPath))
        {
            return OperationResult<CodeDocumentationSavedModelAssessmentModelPassport>
                .Fail(
                    "The reloaded CodeDocumentation trial checkpoint identity is not valid for quality evaluation.",
                    TrialCheckpointMismatchErrorCode);
        }

        return OperationResult<CodeDocumentationSavedModelAssessmentModelPassport>
            .Ok(new CodeDocumentationSavedModelAssessmentModelPassport(
                parent.Domain,
                parent.DesignPattern,
                parent.ModelId,
                parent.Version,
                parent.Step + 1,
                string.Empty,
                string.Empty,
                trialCheckpoint.RepositoryRelativeWeightsPath,
                trialCheckpoint.WeightsSha256,
                trialCheckpoint.WeightsByteLength,
                request.MaximumCheckpointBytes,
                CodeDocumentationSavedModelAssessmentModelPassport
                    .TrialTrainingAttemptCheckpointSourceKind,
                trialCheckpoint.RepositoryRelativeWeightsPath,
                trialCheckpoint.WeightsSha256));
    }

    private static OperationResult<CheckpointByteIdentity>
        HashModelSerialization(
            LlmModel model,
            long maximumBytes,
            CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream();
            model.SaveCheckpointToStream(stream);
            if (stream.Length <= 0 || stream.Length > maximumBytes)
                return Invalid<CheckpointByteIdentity>();
            cancellationToken.ThrowIfCancellationRequested();
            return OperationResult<CheckpointByteIdentity>.Ok(
                new CheckpointByteIdentity(
                    Convert
                        .ToHexString(SHA256.HashData(stream.ToArray()))
                        .ToLowerInvariant(),
                    stream.Length,
                    RepositoryRelativePath: string.Empty));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<CheckpointByteIdentity>();
        }
    }

    private static OperationResult<CheckpointByteIdentity>
        HashTrialCheckpointFile(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
            int dpoStepCount,
            string checkpointPath,
            CancellationToken cancellationToken)
    {
        try
        {
            var canonicalPath =
                Path.GetFullPath(checkpointPath);
            var canonicalAttemptRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(attemptAddress.AttemptRoot));
            var expectedFinalPath = Path.GetFullPath(Path.Combine(
                canonicalAttemptRoot,
                DPO.DpoTrainer.BuildPolicyCheckpointFileName(dpoStepCount)));
            if (!StringComparerForPaths.Equals(
                    canonicalPath,
                    expectedFinalPath))
            {
                return Invalid<CheckpointByteIdentity>();
            }

            var canonicalRepositoryRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(request.RepositoryRoot));
            if (!canonicalPath.StartsWith(
                    canonicalAttemptRoot + Path.DirectorySeparatorChar,
                    PathComparison) ||
                !canonicalPath.StartsWith(
                    canonicalRepositoryRoot + Path.DirectorySeparatorChar,
                    PathComparison) ||
                !File.Exists(canonicalPath) ||
                Directory.Exists(canonicalPath) ||
                AnyExistingPathSegmentIsReparsePoint(canonicalPath))
            {
                return Invalid<CheckpointByteIdentity>();
            }

            using var stream = File.OpenRead(canonicalPath);
            if (stream.Length <= 0 ||
                stream.Length > request.MaximumCheckpointBytes)
            {
                return Invalid<CheckpointByteIdentity>();
            }

            using var hash = IncrementalHash.CreateHash(
                HashAlgorithmName.SHA256);
            var buffer = new byte[81920];
            long total = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read == 0)
                    break;
                total += read;
                if (total > request.MaximumCheckpointBytes)
                    return Invalid<CheckpointByteIdentity>();

                hash.AppendData(buffer.AsSpan(0, read));
            }

            var repositoryRelative = Path.GetRelativePath(
                    canonicalRepositoryRoot,
                    canonicalPath)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            if (!IsSafeRepositoryRelativePath(repositoryRelative))
                return Invalid<CheckpointByteIdentity>();

            return OperationResult<CheckpointByteIdentity>.Ok(
                new CheckpointByteIdentity(
                    Convert
                        .ToHexString(hash.GetHashAndReset())
                        .ToLowerInvariant(),
                    total,
                    repositoryRelative));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<CheckpointByteIdentity>();
        }
    }

    private static string BuildQualityAssessmentRunId(
        string trainingRunId,
        string suffix)
    {
        var appended = trainingRunId + ":quality-" + suffix;
        if (appended.Length <= 256)
            return appended;

        var hash = Convert.ToHexString(
                SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(trainingRunId)))
            .ToLowerInvariant()[..16];
        return "code-doc-quality-" + suffix + "-sha256-" + hash;
    }

    private static OperationResult<RepositoryOwnedModelTrainingAttemptAddress>
        CreateTrainingAttemptAddress(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            string trainingRunId)
    {
        try
        {
            // Файлы нового урока сначала сохраняются в истории попытки. Если
            // подготовка или обучение оборвутся до начала публикации, этот
            // черновой результат не должен занимать папку принятой версии
            // модели.
            return RepositoryOwnedTrainingArtifactLayout
                .CreateModelTrainingAttemptAddress(
                    request.RepositoryRoot,
                    request.DomainSlug,
                    request.DesignPatternSlug,
                    request.ModelIdSlug,
                    request.ModelVersion,
                    trainingRunId);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<RepositoryOwnedModelTrainingAttemptAddress>();
        }
    }

    private static OperationResult<bool> TrainingAttemptAlreadyExistsForResume(
        RepositoryOwnedModelTrainingAttemptAddress address)
    {
        try
        {
            if (!RepositoryOwnedTrainingArtifactLayout
                    .IsValidModelTrainingAttemptAddress(address))
            {
                return Invalid<bool>();
            }

            var attemptRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(address.AttemptRoot));
            if (!StringComparerForPaths.Equals(address.AttemptRoot, attemptRoot) ||
                AnyExistingPathSegmentIsReparsePoint(attemptRoot))
            {
                return Invalid<bool>();
            }

            return OperationResult<bool>.Ok(
                Directory.Exists(attemptRoot) ||
                File.Exists(attemptRoot) ||
                File.Exists(address.AttemptClaimPath));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<bool>();
        }
    }

    private static OperationResult<bool> BaselineAttemptStateMatchesCurrentLesson(
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState state,
        RepositoryOwnedModelCheckpointManifest? parent,
        string dpoManifestRelativePath,
        RepositoryOwnedDpoDatasetManifest dpoManifest,
        RepositoryOwnedDpoDatasetManifestPair trainingPair)
    {
        if (state is null ||
            parent is null ||
            string.IsNullOrWhiteSpace(dpoManifestRelativePath) ||
            dpoManifest is null ||
            trainingPair is null)
        {
            return AttemptRecoveryRequired<bool>();
        }

        var passport = state.AcceptedModelPassport;
        var matches = passport is not null &&
                      StringComparer.Ordinal.Equals(
                          passport.ModelSourceKind,
                          CodeDocumentationSavedModelAssessmentModelPassport
                              .AcceptedRepositoryCheckpointSourceKind) &&
                      StringComparer.Ordinal.Equals(
                          passport.DomainSlug,
                          parent.Domain) &&
                      StringComparer.Ordinal.Equals(
                          passport.DesignPatternSlug,
                          parent.DesignPattern) &&
                      StringComparer.Ordinal.Equals(
                          passport.ModelIdSlug,
                          parent.ModelId) &&
                      StringComparer.Ordinal.Equals(
                          passport.ModelVersion,
                          parent.Version) &&
                      passport.CheckpointStep == parent.Step &&
                      StringComparer.Ordinal.Equals(
                          passport.RepositoryRelativeManifestPath,
                          parent.RepositoryRelativeManifestPath) &&
                      StringComparer.Ordinal.Equals(
                          passport.ManifestSha256,
                          parent.ManifestSha256) &&
                      StringComparer.Ordinal.Equals(
                          passport.RepositoryRelativeWeightsPath,
                          parent.RepositoryRelativeWeightsPath) &&
                      StringComparer.Ordinal.Equals(
                          passport.WeightsSha256,
                          parent.WeightsSha256) &&
                      passport.WeightsByteLength ==
                      parent.WeightsByteLength &&
                      StringComparer.Ordinal.Equals(
                          state.DpoManifestRelativePath,
                          dpoManifestRelativePath) &&
                      StringComparer.Ordinal.Equals(
                          state.DpoManifestSha256,
                          dpoManifest.ManifestSha256) &&
                      StringComparer.Ordinal.Equals(
                          state.TrainingPairId,
                          trainingPair.PairId) &&
                      StringComparer.Ordinal.Equals(
                          state.TrainingPairRelativePath,
                          trainingPair.RepositoryRelativePairFilePath) &&
                      StringComparer.Ordinal.Equals(
                          state.TrainingPairSha256,
                          trainingPair.PairFileSha256);

        return matches
            ? OperationResult<bool>.Ok(true)
            : AttemptRecoveryRequired<bool>(
                "The saved CodeDocumentation DPO baseline attempt does not match the accepted parent checkpoint, reopened DPO manifest, or single training pair requested for resume.");
    }

    private static OperationResult<
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>
        ReopenTrialCheckpointFromAttempt(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
            int dpoStepCount,
            CancellationToken cancellationToken)
    {
        var loaded =
            CodeDocumentationRepositoryOwnedModelCheckpointStartService
                .LoadTrainingAttemptCheckpoint(
                    request,
                    attemptAddress,
                    dpoStepCount,
                    cancellationToken);
        if (loaded.IsSuccess && loaded.Value is not null)
            return loaded;

        return OperationResult<
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>.Fail(
            string.IsNullOrWhiteSpace(loaded.ErrorMessage)
                ? "The CodeDocumentation trial checkpoint could not be reopened from the training attempt before quality evaluation or publication."
                : loaded.ErrorMessage,
            TrialCheckpointMismatchErrorCode);
    }

    private static OperationResult<CodeDocumentationServerOwnedDpoTrainingEvidence>
        BuildRecoveredTrainingEvidence(
            RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
            int dpoStepCount,
            CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback
                baseline,
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad
                trialCheckpoint,
            RepositoryOwnedDpoOptimizerStateFileReadback optimizerState,
            CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback
                optimizerStateManifest,
            LlmModelRandomStateSnapshot? randomState)
    {
        var metricsPath = ResolveTrainingAttemptMetricsPath(
            attemptAddress,
            dpoStepCount);
        if (!metricsPath.IsSuccess || metricsPath.Value is null)
        {
            return metricsPath.MapError<
                CodeDocumentationServerOwnedDpoTrainingEvidence>();
        }

        var manifest = optimizerStateManifest.Manifest;
        var randomStatePath = randomState is null
            ? string.Empty
            : Path.GetFullPath(Path.Combine(
                attemptAddress.AttemptRoot,
                RepositoryOwnedDpoRandomStateFile.BuildFileName(
                    manifest.RandomStateStep)));

        return OperationResult<CodeDocumentationServerOwnedDpoTrainingEvidence>.Ok(
            new CodeDocumentationServerOwnedDpoTrainingEvidence(
                ProcessedPairCount: 1,
                dpoStepCount,
                trialCheckpoint.WeightsPath,
                metricsPath.Value,
                LegalTeacherPolicyIdentityRef:
                "recovered-from-existing-code-documentation-training-attempt",
                baseline.BaselineStatePath,
                baseline.BaselineStateSha256,
                baseline.BaselineStateByteLength,
                optimizerState.OptimizerStatePath,
                optimizerState.OptimizerStateSha256,
                optimizerState.OptimizerStateByteLength,
                optimizerState.State.Step,
                optimizerState.State.FormatVersion,
                optimizerStateManifest.ManifestPath,
                optimizerStateManifest.ManifestSha256,
                optimizerStateManifest.ManifestByteLength,
                randomStatePath,
                randomState is null ? string.Empty : manifest.RandomStateSha256,
                randomState is null ? 0 : manifest.RandomStateByteLength,
                randomState is null ? 0 : manifest.RandomStateStep,
                randomState is null ? 0 : manifest.RandomStateFormatVersion,
                randomState is not null,
                randomState is null ? string.Empty : manifest.RandomStateTopologySha256,
                randomState is null ? string.Empty : manifest.LoraConfigurationSha256));
    }

    private static OperationResult<string> ResolveTrainingAttemptMetricsPath(
        RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
        int dpoStepCount)
    {
        try
        {
            if (dpoStepCount <= 0 ||
                !RepositoryOwnedTrainingArtifactLayout
                    .IsValidModelTrainingAttemptAddress(attemptAddress))
            {
                return AttemptRecoveryRequired<string>();
            }

            var attemptRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(attemptAddress.AttemptRoot));
            var metricsPath = Path.GetFullPath(Path.Combine(
                attemptRoot,
                DPO.DpoTrainer.BuildMetricsFileName(dpoStepCount)));
            if (!StringComparerForPaths.Equals(
                    attemptRoot,
                    attemptAddress.AttemptRoot) ||
                AnyExistingPathSegmentIsReparsePoint(metricsPath) ||
                !metricsPath.StartsWith(
                    attemptRoot + Path.DirectorySeparatorChar,
                    PathComparison) ||
                !File.Exists(metricsPath) ||
                Directory.Exists(metricsPath) ||
                new FileInfo(metricsPath).Length <= 0)
            {
                return AttemptRecoveryRequired<string>(
                    "The CodeDocumentation training attempt metrics file is missing, unsafe, or empty, so the existing attempt cannot be resumed as an already-applied lesson.");
            }

            return OperationResult<string>.Ok(metricsPath);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return AttemptRecoveryRequired<string>();
        }
    }

    private static CodeDocumentationRepositoryOwnedModelCheckpointStart
        CreatePublicationStartFromReloadedTrial(
            CodeDocumentationRepositoryOwnedModelCheckpointStart acceptedStart,
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad
                trialCheckpoint)
        => acceptedStart with
        {
            Model = trialCheckpoint.Model,
            EvidenceRefs = acceptedStart.EvidenceRefs
                .Concat(trialCheckpoint.EvidenceRefs)
                .ToArray(),
            Diagnostics = acceptedStart.Diagnostics
                .Concat([
                    "CodeDocumentation trial checkpoint was reopened from the repository-owned training attempt and hash-verified before quality evaluation/publication."
                ])
                .ToArray()
        };

    private static OperationResult<bool> TrainingAttemptIsFreshForNewQualityRun(
        RepositoryOwnedModelTrainingAttemptAddress address)
    {
        try
        {
            if (!RepositoryOwnedTrainingArtifactLayout
                    .IsValidModelTrainingAttemptAddress(address))
            {
                return Invalid<bool>();
            }

            var attemptRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(address.AttemptRoot));
            if (!StringComparerForPaths.Equals(address.AttemptRoot, attemptRoot) ||
                AnyExistingPathSegmentIsReparsePoint(attemptRoot))
            {
                return Invalid<bool>();
            }

            if (Directory.Exists(attemptRoot) ||
                File.Exists(attemptRoot) ||
                File.Exists(address.AttemptClaimPath))
            {
                return OperationResult<bool>.Fail(
                    "A CodeDocumentation DPO training attempt already exists for this TrainingRunId; recovery of the started lesson must run before another quality-gated attempt can evaluate or train.",
                    TrainingAttemptRecoveryRequiredErrorCode);
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<bool>();
        }
    }

    private static OperationResult<LlmModel> CloneModelBeforeMutation(
        LlmModel model,
        LoraConfig? loraConfig,
        LoraPairRandomSeedAuthority? adapterSeedAuthority)
    {
        try
        {
            using var checkpoint = new MemoryStream();
            model.SaveCheckpointToStream(checkpoint);
            checkpoint.Position = 0;

            var clone = TryLoadReferenceClone(
                model.Config,
                checkpoint,
                applyLoraTopology: false,
                loraConfig,
                adapterSeedAuthority);
            if (clone.IsSuccess)
                return clone;

            if (loraConfig is not null)
            {
                checkpoint.Position = 0;
                clone = TryLoadReferenceClone(
                    model.Config,
                    checkpoint,
                    applyLoraTopology: true,
                    loraConfig,
                    adapterSeedAuthority);
                if (clone.IsSuccess)
                    return clone;
            }

            return Invalid<LlmModel>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<LlmModel>();
        }
    }

    private static OperationResult<LlmModel> TryLoadReferenceClone(
        ModelConfig modelConfig,
        Stream checkpoint,
        bool applyLoraTopology,
        LoraConfig? loraConfig,
        LoraPairRandomSeedAuthority? adapterSeedAuthority)
    {
        try
        {
            // Неизменяемая копия для DPO-сравнения должна иметь ту же форму,
            // что и обучаемая модель. Первый сохранённый вариант может
            // содержать только основные веса, а следующий после LoRA-обучения
            // уже хранит дополнительные матрицы адаптера. Поэтому сначала
            // пробуем открыть сохранённое состояние как обычную модель, а затем
            // — с заранее восстановленными LoRA-слотами, как это делает сервис
            // загрузки.
            var clone = new LlmModel(modelConfig);
            if (applyLoraTopology)
            {
                if (loraConfig is null)
                    return Invalid<LlmModel>();

                var canonical = LoraApplicator.ValidateAndCanonicalize(loraConfig);
                if (adapterSeedAuthority is not null)
                {
                    LoraApplicator.ApplySeeded(
                        clone,
                        canonical,
                        adapterSeedAuthority.RootInitializationSeed,
                        adapterSeedAuthority.ReferenceDropoutStreamDomain);
                }
                else
                {
                    if (canonical.Dropout > 0f && canonical.TargetModules.Length > 0)
                        return Invalid<LlmModel>();
                    LoraApplicator.Apply(clone, canonical);
                }
            }

            clone.LoadCheckpoint(checkpoint);
            return OperationResult<LlmModel>.Ok(clone);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<LlmModel>();
        }
    }

    private static bool ReceiptMatchesOpenedDataset(
        CodeDocumentationRepositoryOwnedDpoDatasetReceipt receipt,
        RepositoryOwnedDpoDatasetOpenResult opened)
    {
        // WHY: receipts can cross process boundaries. A malformed receipt should
        // become a controlled mismatch/invalid result before model START or
        // checkpoint publication, never a NullReferenceException halfway through.
        if (receipt.OrderedPairs is null)
            return false;

        if (!StringComparer.Ordinal.Equals(
                receipt.ManifestSha256,
                opened.Manifest.ManifestSha256) ||
            receipt.PairCount != opened.Manifest.PairCount ||
            receipt.OrderedPairs.Length != opened.Manifest.OrderedPairs.Count ||
            !StringComparer.Ordinal.Equals(
                receipt.RequestIdentitySha256,
                opened.Manifest.RequestIdentity.RequestIdentitySha256))
        {
            return false;
        }

        for (var index = 0; index < receipt.OrderedPairs.Length; index++)
        {
            if (receipt.OrderedPairs[index] != opened.Manifest.OrderedPairs[index])
                return false;
        }

        return true;
    }

    private static bool SequenceEqual(
        IReadOnlyList<DpoPair> left,
        IReadOnlyList<DpoPair> right)
    {
        if (left.Count != right.Count)
            return false;

        for (var index = 0; index < left.Count; index++)
        {
            if (left[index] != right[index])
                return false;
        }

        return true;
    }

    private static bool LoraConfigsMatch(LoraConfig actual, LoraConfig expected)
    {
        try
        {
            var actualCanonical = LoraApplicator.ValidateAndCanonicalize(actual);
            var expectedCanonical = LoraApplicator.ValidateAndCanonicalize(expected);
            return actualCanonical.Rank == expectedCanonical.Rank &&
                   actualCanonical.Alpha.Equals(expectedCanonical.Alpha) &&
                   actualCanonical.Dropout.Equals(expectedCanonical.Dropout) &&
                   actualCanonical.TargetModules.SequenceEqual(
                       expectedCanonical.TargetModules,
                       StringComparer.Ordinal);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static IReadOnlyList<string> BuildEvidenceRefs(
        CodeDocumentationRepositoryOwnedModelCheckpointStart start,
        CodeDocumentationRepositoryOwnedModelCheckpointPublicationEvidence
            publication,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityDecisionEvidence
            qualityDecision)
    {
        var refs = new List<string>(
            start.EvidenceRefs.Count +
            qualityDecision.EvidenceRefs.Count +
            8)
        {
            "code-documentation-dpo-dataset-reopened-from-repository",
            "code-documentation-policy-started-from-repository-model-tree",
            start.ParentManifest is null
                ? "code-documentation-model-lineage-cold-start-authorized"
                : "code-documentation-model-lineage-parent-step:" +
                  start.ParentManifest.Step.ToString(
                      System.Globalization.CultureInfo.InvariantCulture),
            "code-documentation-dpo-training-executed-before-publication",
            "code-documentation-quality-gate:accepted-improvement",
            "repository_model_manifest:" +
            publication.RepositoryRelativeManifestPath,
            "repository_model_weights:" +
            publication.RepositoryRelativeWeightsPath
        };
        refs.AddRange(start.EvidenceRefs);
        refs.AddRange(qualityDecision.EvidenceRefs);
        return Array.AsReadOnly(refs.ToArray());
    }

    private static bool IsSafeRepositoryRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            !StringComparer.Ordinal.Equals(path, path.Trim()) ||
            path.Any(char.IsControl) ||
            path.StartsWith("/", StringComparison.Ordinal) ||
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

    private static bool AnyExistingPathSegmentIsReparsePoint(string path)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            var root = Path.GetPathRoot(fullPath);
            if (string.IsNullOrEmpty(root))
                return true;

            var relative = Path.GetRelativePath(root, fullPath);
            var current = root;
            foreach (var segment in relative.Split(
                         [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                         StringSplitOptions.RemoveEmptyEntries))
            {
                current = string.IsNullOrEmpty(current)
                    ? segment
                    : Path.Combine(current, segment);
                if (!Directory.Exists(current) && !File.Exists(current))
                    continue;

                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    return true;
            }

            return false;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return true;
        }
    }

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "CodeDocumentation repository-owned DPO training publication needs one sealed DPO dataset, one valid START request, one server-owned training lease broker, UTC publication time, and nonempty provenance.",
            InvalidErrorCode);

    private static OperationResult<T> Mismatch<T>()
        => OperationResult<T>.Fail(
            "CodeDocumentation repository-owned DPO receipt, reopened manifest, and in-memory runner pairs do not describe the same sealed dataset.",
            DpoDatasetMismatchErrorCode);

    private static OperationResult<T> AttemptRecoveryRequired<T>(
        string? message = null)
        => OperationResult<T>.Fail(
            string.IsNullOrWhiteSpace(message)
                ? "CodeDocumentation found an existing DPO lesson attempt, but the saved baseline, pair identity, checkpoint, or metrics do not line up safely enough to resume quality evaluation."
                : message,
            TrainingAttemptRecoveryRequiredErrorCode);

    private static OperationResult<T> OptimizerStateRequired<T>(
        string? message = null)
        => OperationResult<T>.Fail(
            string.IsNullOrWhiteSpace(message)
                ? "CodeDocumentation continuation requires a complete, compatible optimizer-state bundle from a canonical attempt."
                : message,
            OptimizerStateRestoreRequiredErrorCode);

    private static OperationResult<T>
        OptimizerStateColdStartAuthorizationRequired<T>()
        => OperationResult<T>.Fail(
            "CodeDocumentation root training may create a fresh optimizer only through an explicit initial-training cold-start authorization with a nonempty reason.",
            OptimizerStateColdStartAuthorizationRequiredErrorCode);

    private static OperationResult<T>
        OptimizerStateColdStartAuthorizationInvalid<T>()
        => OperationResult<T>.Fail(
            "CodeDocumentation optimizer continuation cannot also carry a cold-start authorization.",
            OptimizerStateColdStartAuthorizationInvalidErrorCode);

    private static StringComparison PathComparison
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static StringComparer StringComparerForPaths { get; }
        = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private sealed record SingleRepositoryOwnedDpoTrainingPair(
        RepositoryOwnedDpoDatasetManifestPair ManifestPair,
        DpoPair Pair);

    private sealed record CheckpointByteIdentity(
        string Sha256,
        long ByteLength,
        string RepositoryRelativePath);
}
