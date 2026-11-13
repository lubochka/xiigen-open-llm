// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DPO;
using LoRA;
using Tokenizer;
using TrainingLoop.Optimizers;
using TransformerModel;

/// <summary>
/// Explicit authorization for the only production path that may construct a
/// fresh AdamW optimizer. A missing authorization is never interpreted as a
/// cold start, because a missing state may also mean an incomplete continuation.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization(
    string? Reason)
{
    public const string EvidenceRef =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_DPO_OPTIMIZER_COLD_START_AUTHORIZED";

    public static CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization
        Authorize(string reason)
        => new(reason);

    public bool IsWellFormed
        => !string.IsNullOrWhiteSpace(Reason)
           && Reason.Length <= 512
           && Reason.IndexOf('\r') < 0
           && Reason.IndexOf('\n') < 0;
}

public sealed record CodeDocumentationServerOwnedDpoTrainingRequest(
    IReadOnlyList<ContinuousLearning.DpoPair> Pairs,
    LlmModel PolicyModel,
    LlmModel ReferenceModel,
    BpeTokenizer Tokenizer,
    LoraConfig LoraConfig,
    RepositoryOwnedModelTrainingAttemptAddress ArtifactAddress,
    IServerOwnedDpoTrainingLeaseBroker LeaseBroker,
    string TrainingRunId,
    int DpoStepCount,
    CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState?
        BaselineAttemptState = null,
    AdamWPortableState? OptimizerStateToRestore = null,
    LoraPairRandomSeedAuthority? AdapterSeedAuthority = null,
    bool SeededTopologyPreparedByRepositoryStart = true,
    LlmModelRandomStateSnapshot? RandomStateToRestore = null,
    DpoRandomStateColdStartAuthorization?
        RandomStateColdStartAuthorization = null,
    CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateColdStartAuthorization?
        OptimizerStateColdStartAuthorization = null,
    DpoContinuationProof? ContinuationProof = null);

public sealed record CodeDocumentationServerOwnedDpoTrainingEvidence(
    int ProcessedPairCount,
    int DpoStepCount,
    string PolicyCheckpointPath,
    string MetricsPath,
    string LegalTeacherPolicyIdentityRef,
    string BaselineAttemptStatePath = "",
    string BaselineAttemptStateSha256 = "",
    long BaselineAttemptStateByteLength = 0,
    string OptimizerStatePath = "",
    string OptimizerStateSha256 = "",
    long OptimizerStateByteLength = 0,
    int OptimizerStateStep = 0,
    int OptimizerStateFormatVersion = 0,
    string OptimizerStateManifestPath = "",
    string OptimizerStateManifestSha256 = "",
    long OptimizerStateManifestByteLength = 0,
    string RandomStatePath = "",
    string RandomStateSha256 = "",
    long RandomStateByteLength = 0,
    int RandomStateStep = 0,
    int RandomStateFormatVersion = 0,
    bool RandomStateRequired = false,
    string RandomStateTopologySha256 = "",
    string RandomStateLoraConfigurationSha256 = "",
    string? RandomStateColdStartReason = null,
    string? OptimizerStateColdStartReason = null);

/// <summary>
/// Обучение документации по парам ответов требует серверного разрешения и
/// проверенного адреса попытки внутри репозитория. Файлы нового урока сохраняются
/// отдельно от принятых версий модели.
/// </summary>
public sealed class CodeDocumentationServerOwnedDpoTrainingExecutor
{
    public const string TrainingAttemptAlreadyExistsErrorCode =
        "CODE_DOCUMENTATION_DPO_TRAINING_ATTEMPT_ALREADY_EXISTS";

    private static readonly AsyncLocal<Func<DpoTrainingExecutionArtifact, bool>?>
        PostPublicationFailureForTesting = new();
    internal static Func<DpoTrainingExecutionArtifact, bool>?
        ForcePostPublicationValidationFailureForTesting
    {
        get => PostPublicationFailureForTesting.Value;
        set => PostPublicationFailureForTesting.Value = value;
    }

    public static Task<OperationResult<
        CodeDocumentationServerOwnedDpoTrainingEvidence>> ExecuteAsync(
            CodeDocumentationServerOwnedDpoTrainingRequest request)
        => new CodeDocumentationServerOwnedDpoTrainingExecutor()
            .ExecuteAsync(request);

    public async Task<OperationResult<
        CodeDocumentationServerOwnedDpoTrainingEvidence>> ExecuteAsync(
            CodeDocumentationServerOwnedDpoTrainingRequest request,
            CancellationToken cancellationToken = default)
        => await ExecuteCoreAsync(request, null, cancellationToken).ConfigureAwait(false);

    /// <summary>Fresh prepared provenance enters the same private claim/trainer sequence.</summary>
    internal Task<OperationResult<CodeDocumentationServerOwnedDpoTrainingEvidence>>
        ExecuteFromPreparedBaselineAsync(CodeDocumentationServerOwnedDpoTrainingRequest request,
            CodeDocumentationPreparedBaselineAttemptInputs inputs,
            CancellationToken cancellationToken = default)
        => inputs is null
            ? Task.FromResult(Invalid("Current prepared-baseline inputs are required.")
                .MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>())
            : ExecuteCoreAsync(request, inputs, cancellationToken);

    private async Task<OperationResult<CodeDocumentationServerOwnedDpoTrainingEvidence>>
        ExecuteCoreAsync(CodeDocumentationServerOwnedDpoTrainingRequest request,
            CodeDocumentationPreparedBaselineAttemptInputs? preparedInputs,
            CancellationToken cancellationToken)
    {
        var preflight = ValidateBeforeCapture(request);
        if (!preflight.IsSuccess || preflight.Value != true)
            return preflight.MapError<
                CodeDocumentationServerOwnedDpoTrainingEvidence>();

        var snapshot = DpoPairBatchSnapshot.Capture(request.Pairs);
        if (!snapshot.IsSuccess || snapshot.Value is null)
            return snapshot.MapError<
                CodeDocumentationServerOwnedDpoTrainingEvidence>();
        if (snapshot.Value.Count == 0)
        {
            return Invalid(
                "At least one CodeDocumentation DPO pair is required.")
                .MapError<
                CodeDocumentationServerOwnedDpoTrainingEvidence>();
        }
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedSnapshot?
            prepared = null;
        string? preparedRequestFingerprint = null;
        if (preparedInputs is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.BaselineAttemptState is not null)
                return Invalid("A supplied baseline state cannot override prepared owner provenance.")
                    .MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
            var absent = RequireAbsentPreparedAttemptRoot(request.ArtifactAddress);
            if (!absent.IsSuccess)
                return absent.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
            var fresh = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .ValidatePreparedAsync(request.ArtifactAddress, preparedInputs, cancellationToken)
                .ConfigureAwait(false);
            if (!fresh.IsSuccess || fresh.Value is null)
                return fresh.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
            prepared = fresh.Value;
            var match = ValidatePreparedRequest(request, preparedInputs, prepared, snapshot.Value);
            if (!match.IsSuccess || match.Value is null)
                return match.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
            preparedRequestFingerprint = match.Value;
            absent = RequireAbsentPreparedAttemptRoot(request.ArtifactAddress);
            if (!absent.IsSuccess)
                return absent.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
        }
        if (request.BaselineAttemptState is not null)
        {
            var baselinePair =
                CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                    .MatchesSingleTrainingPair(
                        request.BaselineAttemptState,
                        snapshot.Value);
            if (!baselinePair.IsSuccess)
            {
                return baselinePair.MapError<
                    CodeDocumentationServerOwnedDpoTrainingEvidence>();
            }
        }

        var content = DpoPairTrainingContentAuthority.Project(snapshot.Value);
        if (!content.IsSuccess || content.Value is null)
            return content.MapError<
                CodeDocumentationServerOwnedDpoTrainingEvidence>();

        var lease = request.LeaseBroker.AcquireCodeDocumentationTrainingLease(
            content.Value,
            request.TrainingRunId);
        if (!lease.IsSuccess || lease.Value is null)
            return lease.MapError<
                CodeDocumentationServerOwnedDpoTrainingEvidence>();

        using var admittedLease = lease.Value;
        var reservationLock = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal // Legal admission still precedes all filesystem reservation effects.
            .AcquirePairReservationLock(request.ArtifactAddress); // Serialize discovery and durable reservation across this model/version's run IDs.
        if (!reservationLock.IsSuccess || reservationLock.Value is null) // Contention or failed custody cannot permit a new mutation.
            return reservationLock.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>(); // Preserve the existing recovery-required error for callers.
        using var heldReservationLock = reservationLock.Value; // Every return, cancellation or exception releases the exclusive filesystem handle.
        if (preparedInputs is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var absent = RequireAbsentPreparedAttemptRoot(request.ArtifactAddress);
            if (!absent.IsSuccess)
                return absent.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
            var fresh = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .ValidatePreparedAsync(request.ArtifactAddress, preparedInputs, cancellationToken)
                .ConfigureAwait(false);
            if (!fresh.IsSuccess || fresh.Value is null)
                return fresh.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
            var match = ValidatePreparedRequest(request, preparedInputs, fresh.Value, request.Pairs);
            if (!match.IsSuccess || match.Value != preparedRequestFingerprint ||
                fresh.Value.InputsSha256 != prepared!.InputsSha256 ||
                fresh.Value.ManifestFileSha256 != prepared.ManifestFileSha256 ||
                !CryptographicOperations.FixedTimeEquals(fresh.Value.Bytes, prepared.Bytes))
                return Invalid("Prepared inputs changed while acquiring the training lease.")
                    .MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
            absent = RequireAbsentPreparedAttemptRoot(request.ArtifactAddress);
            if (!absent.IsSuccess)
                return absent.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
            cancellationToken.ThrowIfCancellationRequested();
        }
        var optimizerStart = ValidateBeforeCapture(request);
        if (!optimizerStart.IsSuccess)
            return optimizerStart.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>();
        var siblingReservation = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal // Repeat the read-only coordinator check inside the actual atomic reservation boundary.
            .RequireNoConflictingSiblingAttempt(request.ArtifactAddress, // No other run can claim while this lock is held.
                snapshot.Value.Select(pair => FileDpoPairStore.ComputeIdentity(pair, 5)).ToArray(), // Use the detached batch's actual content identities.
                prepared?.State.TrainingPairSha256 ?? request.BaselineAttemptState?.TrainingPairSha256, cancellationToken); // Keep the exact sealed lesson hash when its baseline owner supplies it.
        if (!siblingReservation.IsSuccess) // A competing durable or partial reservation prevents this claim and trainer entry.
            return siblingReservation.MapError<CodeDocumentationServerOwnedDpoTrainingEvidence>(); // Never turn unknown sibling state into a fresh attempt.
        var attemptRootReady = ClaimCanonicalAttemptRoot(
            request.ArtifactAddress);
        if (!attemptRootReady.IsSuccess)
        {
            return attemptRootReady.MapError<
                CodeDocumentationServerOwnedDpoTrainingEvidence>();
        }

        var baselineState = preparedInputs is not null
            ? await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .WritePreparedAndReadBackAsync(request.ArtifactAddress, preparedInputs, cancellationToken)
                .ConfigureAwait(false)
            : request.BaselineAttemptState is null
            ? null
            : CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .WriteAndReadBack(
                    request.ArtifactAddress,
                    request.BaselineAttemptState,
                    cancellationToken);
        if (baselineState is not null &&
            (!baselineState.IsSuccess || baselineState.Value is null))
        {
            return baselineState.MapError<
                CodeDocumentationServerOwnedDpoTrainingEvidence>();
        }
        heldReservationLock.Dispose(); // The durable claim/baseline now reserves the pair; release before training while retaining disposal on every earlier exit.

        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback?
            optimizerStateManifest = null;
        var optimizerStateManifestCreated = false;

        void PrepareOptimizerStateManifestBeforePolicyPublication(
            DpoTrainingExecutionArtifact preparedArtifact)
        {
            // DpoTrainer invokes the publication callback once per step. The
            // attempt has one final optimizer-state manifest, so only the final
            // step may create it; intermediate steps still publish their own
            // policy/metrics/optimizer artifacts and are cleaned independently.
            if (baselineState?.Value is null ||
                preparedArtifact.DpoStepCount != request.DpoStepCount)
                return;

            var manifest =
                CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .WriteAndReadBack(
                        request.ArtifactAddress,
                        baselineState.Value.State,
                        preparedArtifact,
                        request.PolicyModel.Config,
                        preparedArtifact.OptimizerSettings,
                        cancellationToken);
            if (!manifest.IsSuccess || manifest.Value is null)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(manifest.ErrorMessage)
                        ? "The CodeDocumentation DPO optimizer-state manifest could not be prepared before policy publication."
                        : manifest.ErrorMessage);
            }

            optimizerStateManifest = manifest.Value;
            optimizerStateManifestCreated = true;
        }

        void RollbackOptimizerStateManifest()
        {
            // The callback is intentionally idempotent: the trainer may invoke
            // it while rolling back its own publication, and the stable executor
            // may invoke it again while validating the returned artifact.
            if (optimizerStateManifestCreated)
            {
                _ = CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestFile
                    .DeleteIfOwned(request.ArtifactAddress);
            }
            optimizerStateManifest = null;
            optimizerStateManifestCreated = false;
        }

        CodeDocumentationServerOwnedDpoTrainingEvidence? validatedEvidence = null;
        OperationResult<bool> ValidatePostPublication(
            DpoTrainingExecutionArtifact publishedArtifact)
        {
            if (ForcePostPublicationValidationFailureForTesting?.Invoke(
                    publishedArtifact) == true)
            {
                return OperationResult<bool>.Fail(
                    "The post-publication validation failure was forced for a deterministic transaction test.",
                    "CODE_DOCUMENTATION_DPO_POST_PUBLICATION_VALIDATION_FAILED");
            }

            var optimizerState = ValidateOptimizerStateArtifact(
                request.ArtifactAddress,
                publishedArtifact,
                cancellationToken);
            if (!optimizerState.IsSuccess || optimizerState.Value is null)
                return optimizerState.MapError<bool>();

            var trialWeights = ValidateTrialWeightsArtifact(
                request.ArtifactAddress,
                publishedArtifact,
                cancellationToken);
            if (!trialWeights.IsSuccess || trialWeights.Value != true)
                return trialWeights;

            var randomState = ValidateRandomStateArtifact(
                request.ArtifactAddress,
                publishedArtifact,
                cancellationToken);
            if (!randomState.IsSuccess ||
                (publishedArtifact.RandomStateRequired && randomState.Value is null))
                return randomState.MapError<bool>();

            if (baselineState?.Value is not null &&
                !optimizerStateManifestCreated)
            {
                return OperationResult<bool>.Fail(
                    "The CodeDocumentation DPO optimizer-state manifest was not prepared before policy publication.",
                    "CODE_DOCUMENTATION_DPO_OPTIMIZER_STATE_MANIFEST_INVALID");
            }

            var evidence = BuildEvidence(
                request.ArtifactAddress,
                publishedArtifact,
                request.DpoStepCount,
                baselineState?.Value,
                optimizerState.Value,
                randomState.Value,
                optimizerStateManifest,
                request.RandomStateColdStartAuthorization?.Reason,
                request.OptimizerStateColdStartAuthorization?.Reason);
            if (!evidence.IsSuccess || evidence.Value is null)
                return evidence.MapError<bool>();

            validatedEvidence = evidence.Value;
            return OperationResult<bool>.Ok(true);
        }

        var artifact = await new StableDpoTrainingExecutor().ExecuteAsync(
                new DpoTrainingExecutionRequest(
                    "code-documentation",
                    snapshot.Value,
                    request.PolicyModel,
                    request.ReferenceModel,
                    request.Tokenizer,
                    request.LoraConfig,
                    request.ArtifactAddress.AttemptRoot,
                    admittedLease,
                    request.DpoStepCount,
                    request.OptimizerStateToRestore,
                    request.AdapterSeedAuthority,
                    request.SeededTopologyPreparedByRepositoryStart,
                    request.RandomStateToRestore,
                    request.RandomStateColdStartAuthorization,
                    BeforePolicyFinalMoveArtifact:
                        PrepareOptimizerStateManifestBeforePolicyPublication,
                    PublicationFailed: RollbackOptimizerStateManifest,
                    PostPublicationValidation: ValidatePostPublication,
                    MaterialVersion: DPO.Models.DpoExactPairMaterial.Version,
                    ContinuationProof: request.ContinuationProof),
                cancellationToken)
            .ConfigureAwait(false);
        if (!artifact.IsSuccess || artifact.Value is null)
            return artifact.MapError<
                CodeDocumentationServerOwnedDpoTrainingEvidence>();

        return validatedEvidence is null
            ? OperationResult<CodeDocumentationServerOwnedDpoTrainingEvidence>.Fail(
                "The CodeDocumentation DPO post-publication validation did not produce evidence.",
                "CODE_DOCUMENTATION_DPO_POST_PUBLICATION_VALIDATION_FAILED")
            : OperationResult<CodeDocumentationServerOwnedDpoTrainingEvidence>.Ok(
                validatedEvidence);
    }

    private static OperationResult<bool> RequireAbsentPreparedAttemptRoot(
        RepositoryOwnedModelTrainingAttemptAddress address)
    {
        if (!SecureLocalFileBoundary.IsSupportedPlatform() ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(address.AttemptRoot))
            return AttemptStorageUnavailable();
        return Directory.Exists(address.AttemptRoot) || File.Exists(address.AttemptRoot)
            ? TrainingAttemptAlreadyExists() : OperationResult<bool>.Ok(true);
    }

    private static OperationResult<string> ValidatePreparedRequest(
        CodeDocumentationServerOwnedDpoTrainingRequest request,
        CodeDocumentationPreparedBaselineAttemptInputs inputs,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.PreparedSnapshot prepared,
        IReadOnlyList<DpoPair> pairs)
    {
        try
        {
            var match = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .MatchesSingleTrainingPair(prepared.State, pairs);
            if (!match.IsSuccess) return match.MapError<string>();
            var expected = prepared.State.BaselineQuality.Calibration.RepresentativeAssessment;
            var hashes = new List<string>();
            foreach (var model in new[] { request.PolicyModel, request.ReferenceModel })
            {
                using var bytes = new MemoryStream();
                model.SaveCheckpointToStream(bytes);
                var hash = Convert.ToHexString(SHA256.HashData(bytes.ToArray())).ToLowerInvariant();
                if (bytes.Length != expected.ModelSerializationBeforeByteLength ||
                    hash != expected.ModelSerializationBeforeSha256 ||
                    TrainingSourceProvenance.Identify(model.Config).Sha256 !=
                        TrainingSourceProvenance.Identify(inputs.ModelConfig).Sha256)
                    return Invalid("Training models do not match the freshly reopened accepted baseline.")
                        .MapError<string>();
                hashes.Add(hash);
            }
            using var tokenizer = new MemoryStream();
            request.Tokenizer.Save(tokenizer);
            using var originalTokenizer = new MemoryStream();
            inputs.QualityGateContext.Tokenizer.Save(originalTokenizer);
            if (!tokenizer.ToArray().AsSpan().SequenceEqual(originalTokenizer.ToArray()))
                return Invalid("The training tokenizer differs from the current original exam.")
                    .MapError<string>();
            return OperationResult<string>.Ok(Convert.ToHexString(SHA256.HashData(
                JsonSerializer.SerializeToUtf8Bytes(new
                {
                    request.LoraConfig, request.PolicyModel.Config, ReferenceConfig = request.ReferenceModel.Config,
                    request.TrainingRunId, request.DpoStepCount, request.ArtifactAddress,
                    request.AdapterSeedAuthority, request.SeededTopologyPreparedByRepositoryStart,
                    OptimizerState = request.OptimizerStateToRestore is null ? null :
                        Convert.ToHexString(SHA256.HashData(request.OptimizerStateToRestore.ToBytes())),
                    request.RandomStateToRestore, request.RandomStateColdStartAuthorization,
                    request.OptimizerStateColdStartAuthorization,
                    Pair = FileDpoPairStore.ComputeIdentity(pairs[0], 5), Hashes = hashes,
                    Tokenizer = Convert.ToHexString(SHA256.HashData(tokenizer.ToArray())),
                }))).ToLowerInvariant());
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid("Prepared training inputs cannot be captured.").MapError<string>();
        }
    }

    private static OperationResult<bool> ValidateStateStartContract(
        CodeDocumentationServerOwnedDpoTrainingRequest request)
    {
        try
        {
            if (request.OptimizerStateToRestore is not null)
            {
                if (request.ContinuationProof is null) DpoExecutionProfile.Mismatch();
                request.ContinuationProof!.RequireMatches(request.OptimizerStateToRestore, request.RandomStateToRestore);
            }
            else if (request.ContinuationProof is not null) DpoExecutionProfile.Mismatch();
        }
        catch (DpoMutationAdmissionException exception) { return OperationResult<bool>.Fail(exception.Message, exception.ErrorCode); }
        if (request.OptimizerStateToRestore is null)
        {
            if (request.RandomStateToRestore is not null)
            {
                return OperationResult<bool>.Fail(
                    "A deterministic RNG continuation cannot be admitted without its paired optimizer state.",
                    "DPO_OPTIMIZER_STATE_REQUIRED_FOR_CONTINUATION");
            }
            return request.OptimizerStateColdStartAuthorization is not null
                   && request.OptimizerStateColdStartAuthorization.IsWellFormed
                ? ValidateRandomStateStartContract(request)
                : OperationResult<bool>.Fail(
                    "A CodeDocumentation DPO run without optimizer state requires an explicit, nonempty cold-start authorization reason.",
                    "CODE_DOCUMENTATION_DPO_OPTIMIZER_STATE_COLD_START_AUTHORIZATION_REQUIRED");
        }

        if (request.OptimizerStateColdStartAuthorization is not null)
        {
            return OperationResult<bool>.Fail(
                "Optimizer-state continuation and cold-start authorization are mutually exclusive.",
                "CODE_DOCUMENTATION_DPO_OPTIMIZER_STATE_COLD_START_AUTHORIZATION_INVALID");
        }

        if (request.OptimizerStateToRestore.FormatVersion !=
                AdamWPortableState.CurrentFormatVersion ||
            request.OptimizerStateToRestore.Step <= 0)
        {
            return OperationResult<bool>.Fail(
                "A non-null CodeDocumentation DPO optimizer state must use the current portable format and contain a positive step.",
                RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode);
        }

        return ValidateRandomStateStartContract(request);
    }

    private static OperationResult<bool> ValidateRandomStateStartContract(
        CodeDocumentationServerOwnedDpoTrainingRequest request)
    {
        var canonical = LoraApplicator.ValidateAndCanonicalize(request.LoraConfig);
        var required = canonical.Dropout > 0f &&
                       canonical.TargetModules.Length > 0;
        if (!required)
        {
            return request.RandomStateToRestore is null &&
                   request.RandomStateColdStartAuthorization is null
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    "Random state was supplied for a topology that does not require it.",
                    "DPO_RANDOM_STATE_UNEXPECTED");
        }

        if (request.AdapterSeedAuthority is null)
        {
            return OperationResult<bool>.Fail(
                "Dropout-enabled repository DPO requires deterministic adapter authority.",
                "DPO_RANDOM_STATE_NONDETERMINISTIC_ADAPTER");
        }
        if (request.RandomStateToRestore is null)
        {
            var cold = request.RandomStateColdStartAuthorization;
            return cold is not null && cold.IsWellFormed &&
                   cold.RootSeed == request.AdapterSeedAuthority.RootInitializationSeed
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    "A deterministic RNG cold-start authorization is required.",
                    "DPO_RANDOM_STATE_COLD_START_AUTHORIZATION_REQUIRED");
        }
        return request.RandomStateColdStartAuthorization is null
            ? OperationResult<bool>.Ok(true)
            : OperationResult<bool>.Fail(
                "RNG continuation and cold-start authorization are mutually exclusive.",
                "DPO_RANDOM_STATE_COLD_START_AUTHORIZATION_INVALID");
    }

    private static OperationResult<bool> ValidateBeforeCapture(
        CodeDocumentationServerOwnedDpoTrainingRequest? request)
    {
        if (request is null)
            return Invalid("CodeDocumentation DPO training request is required.");
        if (request.Pairs is null)
            return Invalid("CodeDocumentation DPO pair input is required.");
        if (request.PolicyModel is null || request.ReferenceModel is null ||
            request.Tokenizer is null || request.LoraConfig is null ||
            request.ArtifactAddress is null || request.LeaseBroker is null)
        {
            return Invalid(
                "CodeDocumentation DPO training dependencies are required.");
        }
        if (ReferenceEquals(request.PolicyModel, request.ReferenceModel))
        {
            return Invalid(
                "CodeDocumentation DPO policy and reference models must be separate instances.");
        }
        if (!RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptRunId(request.TrainingRunId))
        {
            return Invalid(
                "A bounded canonical CodeDocumentation DPO training run id is required.");
        }

        var requestedStepCount = StableDpoTrainingExecutor.ValidateDpoStepCount(
            request.DpoStepCount);
        if (!requestedStepCount.IsSuccess)
            return requestedStepCount.MapError<bool>();

        if (!StringComparer.Ordinal.Equals(
                request.TrainingRunId,
                request.ArtifactAddress.TrainingRunId))
        {
            return Invalid(
                "The CodeDocumentation DPO training run id must match the sealed attempt address.");
        }

        // Попытка — это история запуска, а не принятая числовая версия модели.
        // Повторно строим путь из проверенных свойств модели и имени запуска:
        // переданный объект сам по себе не даёт права писать в произвольный
        // каталог.
        var rebuilt = RepositoryOwnedTrainingArtifactLayout
            .CreateModelTrainingAttemptAddress(
                request.ArtifactAddress.RepositoryRoot,
                request.ArtifactAddress.DomainSlug,
                request.ArtifactAddress.DesignPatternSlug,
                request.ArtifactAddress.ModelIdSlug,
                request.ArtifactAddress.ModelVersion,
                request.TrainingRunId);
        if (!rebuilt.IsSuccess || rebuilt.Value is null ||
            !SameAddress(request.ArtifactAddress, rebuilt.Value))
        {
            // Если пересобранный путь отличается, значит объект-адрес больше
            // не совпадает с проверенной моделью и именем запуска. Такой
            // запрос закрывается до записи файлов.
            return Invalid(
                "The repository-owned training attempt address is not the canonical sealed layout address.");
        }

        try { DpoProjectLoraRecipient.RequirePreparedPolicy(request.PolicyModel, request.LoraConfig); }
        catch (DpoMutationAdmissionException exception)
        { return OperationResult<bool>.Fail(exception.Message, exception.ErrorCode); }
        return ValidateStateStartContract(request);
    }

    private static OperationResult<
        CodeDocumentationServerOwnedDpoTrainingEvidence> BuildEvidence(
            RepositoryOwnedModelTrainingAttemptAddress address,
        DpoTrainingExecutionArtifact artifact,
        int requestedDpoStepCount,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournalReadback?
            baselineState,
        RepositoryOwnedDpoOptimizerStateFileReadback optimizerState,
        LlmModelRandomStateSnapshot? randomState,
        CodeDocumentationRepositoryOwnedDpoTrainingOptimizerStateManifestReadback?
            optimizerStateManifest,
        string? randomStateColdStartReason,
            string? optimizerStateColdStartReason)
    {
        if (artifact.ProcessedPairCount <= 0 ||
            artifact.DpoStepCount != requestedDpoStepCount ||
            !IsExpectedDpoTrainerFinalArtifact(
                artifact.PolicyCheckpointPath,
                address.AttemptRoot,
                DpoTrainer.BuildPolicyCheckpointFileName(
                    requestedDpoStepCount)) ||
            !IsExpectedDpoTrainerFinalArtifact(
                artifact.MetricsPath,
                address.AttemptRoot,
                DpoTrainer.BuildMetricsFileName(requestedDpoStepCount)) ||
            artifact.PostStepOptimizerState is null ||
            artifact.PostStepOptimizerStateIdentity is null ||
            artifact.OptimizerStateStep <= 0 ||
            artifact.OptimizerStateFormatVersion !=
                AdamWPortableState.CurrentFormatVersion ||
            artifact.OptimizerStateByteLength <= 0 ||
            string.IsNullOrWhiteSpace(artifact.OptimizerStateSha256) ||
            artifact.OptimizerStateStep != optimizerState.State.Step ||
            artifact.OptimizerStateFormatVersion !=
                optimizerState.State.FormatVersion ||
            artifact.OptimizerStateByteLength !=
                optimizerState.OptimizerStateByteLength ||
            !StringComparer.Ordinal.Equals(
                artifact.OptimizerStateSha256,
                optimizerState.OptimizerStateSha256) ||
            optimizerState.State.FormatVersion !=
                AdamWPortableState.CurrentFormatVersion ||
            optimizerState.State.Step != artifact.OptimizerStateStep ||
            artifact.TrialWeightsByteLength <= 0 ||
            string.IsNullOrWhiteSpace(artifact.TrialWeightsSha256) ||
            artifact.TrialWeightsPath.Length == 0 ||
            (baselineState is not null && optimizerStateManifest is null) ||
            (optimizerStateManifest is not null &&
             (optimizerStateManifest.Manifest.DpoStepCount !=
                    artifact.DpoStepCount ||
              optimizerStateManifest.Manifest.OptimizerStateStep !=
                    artifact.OptimizerStateStep ||
              !StringComparer.Ordinal.Equals(
                    optimizerStateManifest.Manifest.OptimizerStateSha256,
                    artifact.OptimizerStateSha256) ||
              optimizerStateManifest.Manifest.OptimizerStateByteLength !=
                    artifact.OptimizerStateByteLength ||
              optimizerStateManifest.Manifest.OptimizerStateFormatVersion !=
                    artifact.OptimizerStateFormatVersion ||
              !StringComparer.Ordinal.Equals(
                    optimizerStateManifest.Manifest.TrialWeightsRelativePath,
                    address.AttemptRootRelativePath + "/" +
                        DpoTrainer.BuildPolicyCheckpointFileName(
                            artifact.DpoStepCount)) ||
              !StringComparer.Ordinal.Equals(
                    optimizerStateManifest.Manifest.TrialWeightsSha256,
                    artifact.TrialWeightsSha256) ||
              optimizerStateManifest.Manifest.TrialWeightsByteLength !=
                    artifact.TrialWeightsByteLength))
            || (artifact.RandomStateRequired &&
                (randomState is null ||
                 artifact.RandomStateStep != randomState.RandomStateStep ||
                 artifact.RandomStateFormatVersion != randomState.FormatVersion ||
                 artifact.RandomStateByteLength <= 0 ||
                 string.IsNullOrWhiteSpace(artifact.RandomStatePath) ||
                 string.IsNullOrWhiteSpace(artifact.RandomStateSha256)))
            )
        {
            return OperationResult<
                CodeDocumentationServerOwnedDpoTrainingEvidence>.Fail(
                "The stable DPO executor did not return the expected CodeDocumentation artifacts.",
                "CODE_DOCUMENTATION_DPO_ARTIFACT_INVALID");
        }

        return OperationResult<
            CodeDocumentationServerOwnedDpoTrainingEvidence>.Ok(
            new CodeDocumentationServerOwnedDpoTrainingEvidence(
                artifact.ProcessedPairCount,
                artifact.DpoStepCount,
                artifact.PolicyCheckpointPath,
                artifact.MetricsPath,
                artifact.LegalTeacherPolicyIdentityRef,
                baselineState?.BaselineStatePath ?? string.Empty,
                baselineState?.BaselineStateSha256 ?? string.Empty,
                baselineState?.BaselineStateByteLength ?? 0L,
                artifact.OptimizerStatePath,
                artifact.OptimizerStateSha256,
                artifact.OptimizerStateByteLength,
                artifact.OptimizerStateStep,
                artifact.OptimizerStateFormatVersion,
                optimizerStateManifest?.ManifestPath ?? string.Empty,
                optimizerStateManifest?.ManifestSha256 ?? string.Empty,
                optimizerStateManifest?.ManifestByteLength ?? 0L,
                artifact.RandomStatePath,
                artifact.RandomStateSha256,
                artifact.RandomStateByteLength,
                artifact.RandomStateStep,
                artifact.RandomStateFormatVersion,
                artifact.RandomStateRequired,
                artifact.RandomStateTopologySha256,
                artifact.RandomStateLoraConfigurationSha256,
                randomStateColdStartReason,
                optimizerStateColdStartReason));
    }

    private static OperationResult<RepositoryOwnedDpoOptimizerStateFileReadback>
        ValidateOptimizerStateArtifact(
            RepositoryOwnedModelTrainingAttemptAddress address,
            DpoTrainingExecutionArtifact artifact,
            CancellationToken cancellationToken)
    {
        if (artifact.PostStepOptimizerState is null ||
            artifact.PostStepOptimizerStateIdentity is null ||
            artifact.OptimizerStateStep <= 0 ||
            artifact.OptimizerStateFormatVersion !=
                AdamWPortableState.CurrentFormatVersion ||
            artifact.OptimizerStateByteLength <= 0 ||
            string.IsNullOrWhiteSpace(artifact.OptimizerStateSha256))
        {
            return OperationResult<RepositoryOwnedDpoOptimizerStateFileReadback>.Fail(
                "The CodeDocumentation DPO optimizer-state identity is incomplete.",
                "CODE_DOCUMENTATION_DPO_OPTIMIZER_STATE_INVALID");
        }

        var opened = RepositoryOwnedDpoOptimizerStateFile.OpenExisting(
            address.AttemptRoot,
            artifact.OptimizerStateStep,
            artifact.OptimizerStateSha256,
            cancellationToken);
        if (!opened.IsSuccess || opened.Value is null)
            return opened;

        var expectedPath = Path.GetFullPath(Path.Combine(
            address.AttemptRoot,
            RepositoryOwnedDpoOptimizerStateFile.BuildFileName(
                artifact.OptimizerStateStep)));
        if (!StringComparerForPaths.Equals(
                expectedPath,
                Path.GetFullPath(artifact.OptimizerStatePath)) ||
            opened.Value.OptimizerStateByteLength !=
                artifact.OptimizerStateByteLength ||
            opened.Value.State.Step != artifact.OptimizerStateStep ||
            opened.Value.State.FormatVersion !=
                artifact.OptimizerStateFormatVersion ||
            !System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                artifact.PostStepOptimizerState.ToBytes(),
                opened.Value.State.ToBytes()))
        {
            return OperationResult<RepositoryOwnedDpoOptimizerStateFileReadback>.Fail(
                "The CodeDocumentation DPO optimizer-state file does not match the post-step identity returned by the executor.",
                "CODE_DOCUMENTATION_DPO_OPTIMIZER_STATE_INVALID");
        }

        return opened;
    }

    private static OperationResult<LlmModelRandomStateSnapshot?>
        ValidateRandomStateArtifact(
            RepositoryOwnedModelTrainingAttemptAddress address,
            DpoTrainingExecutionArtifact artifact,
            CancellationToken cancellationToken)
    {
        if (!artifact.RandomStateRequired)
        {
            return artifact.PostStepRandomState is null &&
                   string.IsNullOrEmpty(artifact.RandomStatePath) &&
                   string.IsNullOrEmpty(artifact.RandomStateSha256) &&
                   artifact.RandomStateByteLength == 0 &&
                   artifact.RandomStateStep == 0
                ? OperationResult<LlmModelRandomStateSnapshot?>.Ok(null)
                : OperationResult<LlmModelRandomStateSnapshot?>.Fail(
                    "The DPO artifact carries RNG state for a topology that does not require it.",
                    "DPO_RANDOM_STATE_UNEXPECTED");
        }

        if (artifact.PostStepRandomState is null ||
            artifact.RandomStateStep <= 0 ||
            artifact.RandomStateFormatVersion !=
                (int)RepositoryOwnedDpoRandomStateFile.CurrentFormatVersion ||
            string.IsNullOrWhiteSpace(artifact.RandomStatePath) ||
            string.IsNullOrWhiteSpace(artifact.RandomStateSha256) ||
            artifact.RandomStateByteLength <= 0)
        {
            return OperationResult<LlmModelRandomStateSnapshot?>.Fail(
                "The CodeDocumentation DPO random-state identity is incomplete.",
                "CODE_DOCUMENTATION_DPO_RANDOM_STATE_INVALID");
        }

        var opened = RepositoryOwnedDpoRandomStateFile.OpenAndValidate(
            Path.GetFullPath(artifact.RandomStatePath),
            artifact.RandomStateSha256,
            artifact.RandomStateByteLength,
            artifact.RandomStateStep,
            artifact.RandomStateTopologySha256,
            artifact.RandomStateLoraConfigurationSha256);
        if (!opened.IsSuccess || opened.Value is null)
            return opened.MapError<LlmModelRandomStateSnapshot?>();
        if (!StringComparerForPaths.Equals(
                Path.GetFullPath(artifact.RandomStatePath),
                Path.GetFullPath(Path.Combine(
                    address.AttemptRoot,
                    RepositoryOwnedDpoRandomStateFile.BuildFileName(
                        artifact.RandomStateStep)))) ||
            opened.Value.FormatVersion != artifact.RandomStateFormatVersion ||
            opened.Value.RandomStateStep != artifact.RandomStateStep)
        {
            return OperationResult<LlmModelRandomStateSnapshot?>.Fail(
                "The CodeDocumentation DPO random-state file does not match its sealed artifact identity.",
                "CODE_DOCUMENTATION_DPO_RANDOM_STATE_INVALID");
        }
        return OperationResult<LlmModelRandomStateSnapshot?>.Ok(opened.Value);
    }

    private static OperationResult<bool> ValidateTrialWeightsArtifact(
        RepositoryOwnedModelTrainingAttemptAddress address,
        DpoTrainingExecutionArtifact artifact,
        CancellationToken cancellationToken)
    {
        try
        {
            if (artifact.TrialWeightsByteLength <= 0 ||
                string.IsNullOrWhiteSpace(artifact.TrialWeightsSha256) ||
                !IsExpectedDpoTrainerFinalArtifact(
                    artifact.TrialWeightsPath,
                    address.AttemptRoot,
                    DpoTrainer.BuildPolicyCheckpointFileName(
                        artifact.DpoStepCount)))
            {
                return OperationResult<bool>.Fail(
                    "The CodeDocumentation trial weights identity is incomplete or points outside the attempt.",
                    "CODE_DOCUMENTATION_DPO_TRIAL_WEIGHTS_INVALID");
            }

            var path = TrainingSourceProvenance.RequireCanonicalRegularFilePath(
                artifact.TrialWeightsPath);
            using var stream = File.OpenRead(path);
            if (stream.Length != artifact.TrialWeightsByteLength ||
                stream.Length > 16L * 1024L * 1024L * 1024L)
            {
                return OperationResult<bool>.Fail(
                    "The CodeDocumentation trial weights length differs from the staged bundle identity.",
                    "CODE_DOCUMENTATION_DPO_TRIAL_WEIGHTS_INVALID");
            }

            cancellationToken.ThrowIfCancellationRequested();
            var actualSha256 = Convert.ToHexString(SHA256.HashData(stream))
                .ToLowerInvariant();
            return StringComparer.Ordinal.Equals(
                    actualSha256,
                    artifact.TrialWeightsSha256)
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    "The CodeDocumentation trial weights changed between staging and post-publication validation.",
                    "CODE_DOCUMENTATION_DPO_TRIAL_WEIGHTS_INVALID");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                "The CodeDocumentation trial weights could not be reopened for identity validation.",
                "CODE_DOCUMENTATION_DPO_TRIAL_WEIGHTS_INVALID");
        }
    }

    private static bool IsExpectedDpoTrainerFinalArtifact(
        string path,
        string attemptRoot,
        string expectedFileName)
    {
        try
        {
            var canonicalPath =
                TrainingSourceProvenance.RequireCanonicalRegularFilePath(path);
            var canonicalRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(attemptRoot));
            var expectedPath = Path.GetFullPath(Path.Combine(
                canonicalRoot,
                expectedFileName));

            // Тренер возвращает пути к своим финальным файлам. Здесь повторно
            // проверяем, что это именно ожидаемые файлы внутри каталога
            // попытки, а не соседний или временный файл.
            return StringComparerForPaths.Equals(canonicalPath, expectedPath)
                   && StringComparerForPaths.Equals(
                       Path.GetDirectoryName(canonicalPath),
                       canonicalRoot)
                   && new FileInfo(canonicalPath).Length > 0;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool SameAddress(
        RepositoryOwnedModelTrainingAttemptAddress actual,
        RepositoryOwnedModelTrainingAttemptAddress expected)
        => StringComparerForPaths.Equals(actual.RepositoryRoot, expected.RepositoryRoot)
           && StringComparerForPaths.Equals(actual.ModelArtifactRoot, expected.ModelArtifactRoot)
           && StringComparer.Ordinal.Equals(actual.DomainSlug, expected.DomainSlug)
           && StringComparer.Ordinal.Equals(actual.DesignPatternSlug, expected.DesignPatternSlug)
           && StringComparer.Ordinal.Equals(actual.ModelIdSlug, expected.ModelIdSlug)
           && StringComparer.Ordinal.Equals(actual.ModelVersion, expected.ModelVersion)
           && StringComparer.Ordinal.Equals(actual.TrainingRunId, expected.TrainingRunId)
           && StringComparer.Ordinal.Equals(
               actual.TrainingRunIdSha256,
               expected.TrainingRunIdSha256)
           && StringComparerForPaths.Equals(actual.AttemptRoot, expected.AttemptRoot)
           && StringComparerForPaths.Equals(
               actual.AttemptClaimPath,
               expected.AttemptClaimPath)
           && StringComparer.Ordinal.Equals(
               actual.AttemptRootRelativePath,
               expected.AttemptRootRelativePath)
           && StringComparer.Ordinal.Equals(
               actual.AttemptClaimRelativePath,
               expected.AttemptClaimRelativePath);

    private static OperationResult<bool> ClaimCanonicalAttemptRoot(
        RepositoryOwnedModelTrainingAttemptAddress address)
    {
        try
        {
            if (!RepositoryOwnedTrainingArtifactLayout
                    .IsValidModelTrainingAttemptAddress(address))
            {
                return Invalid(
                    "The repository-owned training attempt address is not the canonical sealed layout address.");
            }

            var attemptRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(address.AttemptRoot));
            if (!StringComparerForPaths.Equals(
                    address.AttemptRoot,
                    attemptRoot))
            {
                return Invalid(
                    "The repository-owned training attempt address is not the canonical sealed layout address.");
            }

            if (!SecureLocalFileBoundary.IsSupportedPlatform()
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    attemptRoot))
            {
                return AttemptStorageUnavailable();
            }

            if (Directory.Exists(attemptRoot))
            {
                if (!SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                        attemptRoot))
                {
                    return AttemptStorageUnavailable();
                }

                return TrainingAttemptAlreadyExists();
            }
            else if (File.Exists(attemptRoot))
            {
                return AttemptStorageUnavailable();
            }
            else
            {
                // На Linux обычное создание каталога зависит от umask и может
                // оставить доступ группе или другим пользователям. Каталог
                // попытки создаём через безопасную файловую границу до запуска
                // тренера; уже существующий небезопасный каталог не исправляем
                // молча, а закрываем отказом.
                SecureLocalFileBoundary.CreateOwnerOnlyDirectory(
                    attemptRoot);
            }

            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    attemptRoot)
                || !Directory.Exists(attemptRoot)
                || !SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                    attemptRoot))
            {
                return AttemptStorageUnavailable();
            }

            var claim = ClaimAttempt(address);
            if (!claim.IsSuccess)
                return claim;

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return AttemptStorageUnavailable();
        }
    }

    private static OperationResult<bool> ClaimAttempt(
        RepositoryOwnedModelTrainingAttemptAddress address)
    {
        try
        {
            using var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                address.AttemptClaimPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                FileOptions.WriteThrough);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    address.AttemptClaimPath)
                || !SecureLocalFileBoundary.HasOwnerOnlyFileMode(
                    address.AttemptClaimPath))
            {
                return AttemptStorageUnavailable();
            }

            var bytes = Encoding.UTF8.GetBytes(
                "training_run_id=" + address.TrainingRunId + Environment.NewLine +
                "training_run_id_sha256=" + address.TrainingRunIdSha256 +
                Environment.NewLine +
                "attempt_root=" + address.AttemptRootRelativePath +
                Environment.NewLine);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(flushToDisk: true);
            return OperationResult<bool>.Ok(true);
        }
        catch (IOException)
        {
            return File.Exists(address.AttemptClaimPath)
                ? TrainingAttemptAlreadyExists()
                : AttemptStorageUnavailable();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return AttemptStorageUnavailable();
        }
    }

    private static OperationResult<bool> Invalid(string message)
        => OperationResult<bool>.Fail(
            message,
            LegalTrainingTeacherAdmissionAuthority
                .TrainingContentInvalidErrorCode);

    private static OperationResult<bool> AttemptStorageUnavailable()
        => OperationResult<bool>.Fail(
            "Repository-owned model training attempt storage is unavailable.",
            RepositoryOwnedModelCheckpointManifestSealer.UnavailableErrorCode);

    private static OperationResult<bool> TrainingAttemptAlreadyExists()
        => OperationResult<bool>.Fail(
            "A CodeDocumentation DPO training attempt already exists for this TrainingRunId; recovery is not implemented, so the executor refuses to mutate weights again.",
            TrainingAttemptAlreadyExistsErrorCode);

    private static StringComparer StringComparerForPaths { get; }
        = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
}
