// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using System.Runtime.ExceptionServices;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

/// <summary>
/// Transfers ownership of one fresh checkpoint snapshot stream to the caller
/// for every successful call. The stream must be readable, seekable, positioned
/// at zero, and must not be shared with another consumer.
/// </summary>
public interface IDesignConversionDpoPolicyCheckpointSource
{
    Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
        CancellationToken cancellationToken);
}

/// <summary>
/// Builds physically independent DesignConversion DPO policy bases from one
/// exact, caller-owned checkpoint snapshot without exposing a filesystem path.
/// </summary>
public sealed class DesignConversionCheckpointBackedDpoPolicyBaseProvider
    : IDesignConversionDpoPolicyContinuationProvider
{
    public const string ConfigurationInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_CONFIGURATION_INVALID";
    public const string CheckpointSourceFailedErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_CHECKPOINT_SOURCE_FAILED";
    public const string CheckpointStreamInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_CHECKPOINT_STREAM_INVALID";
    public const string CheckpointHashMismatchErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_CHECKPOINT_HASH_MISMATCH";
    public const string CheckpointChangedDuringLoadErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_CHECKPOINT_CHANGED_DURING_LOAD";
    public const string CheckpointInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_CHECKPOINT_INVALID";
    public const string ContinuationContractInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_CONTINUATION_CONTRACT_INVALID";
    public const string ContinuationCheckpointMismatchErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_CONTINUATION_CHECKPOINT_MISMATCH";
    public const string ContinuationLoraInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_CONTINUATION_LORA_INVALID";
    public const string ContinuationRandomContextMismatchErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_CONTINUATION_RANDOM_CONTEXT_MISMATCH";
    public const string CanonicalBpeTokenizerSourceFailedErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_CANONICAL_BPE_TOKENIZER_SOURCE_FAILED";
    public const string CanonicalBpeTokenizerInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_CANONICAL_BPE_TOKENIZER_INVALID";

    private const long MaximumCheckpointBytes = 16L * 1024 * 1024 * 1024;
    private const int HashBufferBytes = 128 * 1024;
    private const string CancelledErrorCode = "CANCELLED";
    private const string ConfigurationInvalidErrorMessage =
        "DesignConversion DPO checkpoint-backed policy base configuration is invalid.";
    private const string CheckpointSourceFailedErrorMessage =
        "The DesignConversion DPO policy base checkpoint source failed.";
    private const string CheckpointStreamInvalidErrorMessage =
        "The DesignConversion DPO policy base checkpoint stream is invalid.";
    private const string CheckpointHashMismatchErrorMessage =
        "The DesignConversion DPO policy base checkpoint hash does not match.";
    private const string CheckpointChangedDuringLoadErrorMessage =
        "The DesignConversion DPO policy base checkpoint changed during loading.";
    private const string CheckpointInvalidErrorMessage =
        "The DesignConversion DPO policy base checkpoint is invalid.";
    private const string CancelledErrorMessage =
        "DesignConversion DPO checkpoint-backed policy base creation was cancelled.";
    private const string ContinuationContractInvalidErrorMessage =
        "The DesignConversion DPO continuation contract is invalid.";
    private const string ContinuationCheckpointMismatchErrorMessage =
        "The DesignConversion DPO continuation checkpoint does not match its contract.";
    private const string ContinuationLoraInvalidErrorMessage =
        "The DesignConversion DPO continuation LoRA topology is invalid.";
    private const string ContinuationRandomContextMismatchErrorMessage =
        "The DesignConversion DPO continuation random context does not match its provider authority.";
    private const string CanonicalBpeTokenizerSourceFailedErrorMessage =
        "The DesignConversion DPO canonical BPE tokenizer snapshot source failed.";
    private const string CanonicalBpeTokenizerInvalidErrorMessage =
        "The DesignConversion DPO canonical BPE tokenizer snapshot is invalid.";

    private readonly IDesignConversionDpoPolicyCheckpointSource _checkpointSource;
    private readonly ModelConfig _ownedConfig;
    private readonly DesignConversionDpoPolicyBaseIdentity _identity;
    private readonly DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor?
        _expectedCanonicalBpeTokenizerArtifactDescriptor;
    private readonly IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource?
        _canonicalBpeTokenizerSnapshotSource;
    private readonly DesignConversionDpoPolicyBaseProviderOptions? _providerOptions;
    private readonly DesignConversionDpoPolicyRandomContext? _currentRandomContext;

    private DesignConversionCheckpointBackedDpoPolicyBaseProvider(
        IDesignConversionDpoPolicyCheckpointSource checkpointSource,
        ModelConfig ownedConfig,
        DesignConversionDpoPolicyBaseIdentity identity,
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor?
            expectedCanonicalBpeTokenizerArtifactDescriptor,
        IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource?
            canonicalBpeTokenizerSnapshotSource,
        DesignConversionDpoPolicyBaseProviderOptions? providerOptions,
        DesignConversionDpoPolicyRandomContext? currentRandomContext)
    {
        _checkpointSource = checkpointSource;
        _ownedConfig = ownedConfig;
        _identity = identity;
        _expectedCanonicalBpeTokenizerArtifactDescriptor =
            expectedCanonicalBpeTokenizerArtifactDescriptor;
        _canonicalBpeTokenizerSnapshotSource = canonicalBpeTokenizerSnapshotSource;
        _providerOptions = providerOptions;
        _currentRandomContext = currentRandomContext;
    }

    internal DesignConversionDpoPolicyBaseProviderOptions? ProviderOptions =>
        _providerOptions;

    internal DesignConversionDpoPolicyRandomContext? CurrentRandomContext =>
        _currentRandomContext;

    public static OperationResult<DesignConversionCheckpointBackedDpoPolicyBaseProvider> Create(
        IDesignConversionDpoPolicyCheckpointSource? checkpointSource,
        ModelConfig? modelConfig,
        string? baseModelId,
        string? baseRevision,
        string? expectedCheckpointSha256,
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor?
            expectedCanonicalBpeTokenizerArtifactDescriptor = null,
        IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource?
            canonicalBpeTokenizerSnapshotSource = null)
    {
        var identity = new DesignConversionDpoPolicyBaseIdentity(
            ProviderKind: "checkpoint_backed",
            ConfigurationKind: "explicit_model_config_v1",
            BaseModelId: baseModelId ?? string.Empty,
            BaseRevision: baseRevision ?? string.Empty,
            BaseCheckpointSha256: expectedCheckpointSha256 ?? string.Empty);
        return CreateTrusted(
            checkpointSource,
            modelConfig,
            identity,
            expectedCanonicalBpeTokenizerArtifactDescriptor,
            canonicalBpeTokenizerSnapshotSource);
    }

    public static OperationResult<DesignConversionCheckpointBackedDpoPolicyBaseProvider>
        CreateWithOptions(
            IDesignConversionDpoPolicyCheckpointSource? checkpointSource,
            ModelConfig? modelConfig,
            string? baseModelId,
            string? baseRevision,
            string? expectedCheckpointSha256,
            DesignConversionDpoPolicyBaseProviderOptions? options,
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor?
                expectedCanonicalBpeTokenizerArtifactDescriptor = null,
            IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource?
                canonicalBpeTokenizerSnapshotSource = null)
    {
        var identity = new DesignConversionDpoPolicyBaseIdentity(
            ProviderKind: "checkpoint_backed",
            ConfigurationKind: "explicit_model_config_v1",
            BaseModelId: baseModelId ?? string.Empty,
            BaseRevision: baseRevision ?? string.Empty,
            BaseCheckpointSha256: expectedCheckpointSha256 ?? string.Empty);
        return CreateTrustedCore(
            checkpointSource,
            modelConfig,
            identity,
            options,
            optionsRequired: true,
            expectedCanonicalBpeTokenizerArtifactDescriptor,
            canonicalBpeTokenizerSnapshotSource);
    }

    internal static OperationResult<DesignConversionCheckpointBackedDpoPolicyBaseProvider>
        CreateTrusted(
            IDesignConversionDpoPolicyCheckpointSource? checkpointSource,
            ModelConfig? modelConfig,
            DesignConversionDpoPolicyBaseIdentity? identity,
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor?
                expectedCanonicalBpeTokenizerArtifactDescriptor = null,
            IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource?
                canonicalBpeTokenizerSnapshotSource = null)
    {
        return CreateTrustedCore(
            checkpointSource,
            modelConfig,
            identity,
            options: null,
            optionsRequired: false,
            expectedCanonicalBpeTokenizerArtifactDescriptor,
            canonicalBpeTokenizerSnapshotSource);
    }

    internal static OperationResult<DesignConversionCheckpointBackedDpoPolicyBaseProvider>
        CreateTrustedWithOptions(
            IDesignConversionDpoPolicyCheckpointSource? checkpointSource,
            ModelConfig? modelConfig,
            DesignConversionDpoPolicyBaseIdentity? identity,
            DesignConversionDpoPolicyBaseProviderOptions? options,
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor?
                expectedCanonicalBpeTokenizerArtifactDescriptor = null,
            IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource?
                canonicalBpeTokenizerSnapshotSource = null)
    {
        return CreateTrustedCore(
            checkpointSource,
            modelConfig,
            identity,
            options,
            optionsRequired: true,
            expectedCanonicalBpeTokenizerArtifactDescriptor,
            canonicalBpeTokenizerSnapshotSource);
    }

    private static OperationResult<DesignConversionCheckpointBackedDpoPolicyBaseProvider>
        CreateTrustedCore(
            IDesignConversionDpoPolicyCheckpointSource? checkpointSource,
            ModelConfig? modelConfig,
            DesignConversionDpoPolicyBaseIdentity? identity,
            DesignConversionDpoPolicyBaseProviderOptions? options,
            bool optionsRequired,
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor?
                expectedCanonicalBpeTokenizerArtifactDescriptor,
            IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource?
                canonicalBpeTokenizerSnapshotSource)
    {
        if (optionsRequired &&
            (options is null ||
             !DesignConversionDpoPolicyRandomContextContract.IsCanonicalFingerprint(
                 options.TrainingConfigurationFingerprint)))
        {
            return ConfigurationInvalid();
        }

        var ownedOptions = options is null ? null : options with { };
        var currentRandomContext = ownedOptions?.RootInitializationSeed is ulong rootSeed
            ? DesignConversionDpoPolicyRandomContextContract.CreateCurrent(
                rootSeed,
                ownedOptions.TrainingConfigurationFingerprint)
            : null;
        var ownedConfig = modelConfig is null ? null : CopyConfig(modelConfig);
        if (ownedOptions?.InitialFullCheckpoint is { } declared // Carry the same immutable full-LoRA declaration through startup and later policy loading.
            && (ownedConfig is null || !declared.MatchesModelConfig(ownedConfig).IsSuccess // Treat a typed shape refusal as invalid provider configuration.
                || declared.WeightsSha256 != identity?.BaseCheckpointSha256 // Bind the exact sealed weight digest instead of inferring identity from shape.
                || declared.RootInitializationSeed != ownedOptions.RootInitializationSeed)) // Reject provider options whose root initialization seed differs from the descriptor's seed.
            return ConfigurationInvalid(); // Fail closed when the preceding trusted metadata operation was refused.
        var ownedCanonicalBpeTokenizerArtifactDescriptor =
            expectedCanonicalBpeTokenizerArtifactDescriptor is null
                ? null
                : DesignConversionDpoPolicyBaseMetadata
                    .CopyCanonicalBpeTokenizerArtifactDescriptor(
                        expectedCanonicalBpeTokenizerArtifactDescriptor);
        var hasCanonicalBpeTokenizerDescriptor =
            ownedCanonicalBpeTokenizerArtifactDescriptor is not null;
        var hasCanonicalBpeTokenizerSource =
            canonicalBpeTokenizerSnapshotSource is not null;
        if (checkpointSource is null ||
            !DesignConversionDpoPolicyBaseMetadata.IsValidConfig(ownedConfig) ||
            !DesignConversionDpoPolicyBaseMetadata.IsValidIdentity(identity) ||
            hasCanonicalBpeTokenizerDescriptor != hasCanonicalBpeTokenizerSource ||
            hasCanonicalBpeTokenizerDescriptor &&
            !IsExpectedCanonicalBpeTokenizerDescriptorStructurallySafe(
                ownedCanonicalBpeTokenizerArtifactDescriptor!))
        {
            return ConfigurationInvalid();
        }

        return OperationResult<DesignConversionCheckpointBackedDpoPolicyBaseProvider>.Ok(
            new DesignConversionCheckpointBackedDpoPolicyBaseProvider(
                checkpointSource,
                ownedConfig!,
                identity!,
                ownedCanonicalBpeTokenizerArtifactDescriptor,
                canonicalBpeTokenizerSnapshotSource,
                ownedOptions,
                currentRandomContext));
    }

    public async Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>> CreateModelSetAsync(
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        var tokenizerReferenceResult =
            await CreateConfiguredCanonicalBpeTokenizerSnapshotReferenceAsync(
                cancellationToken);
        if (!tokenizerReferenceResult.IsSuccess)
            return CanonicalBpeTokenizerFailed(tokenizerReferenceResult);
        var tokenizerReference = tokenizerReferenceResult.Value;

        OperationResult<Stream>? sourceResult;
        try
        {
            sourceResult = await _checkpointSource.OpenCheckpointSnapshotAsync(
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            return CheckpointSourceFailed();
        }

        if (sourceResult is null || !sourceResult.IsSuccess)
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            return CheckpointSourceFailed();
        }

        var stream = sourceResult.Value;
        if (stream is null)
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            return CheckpointStreamInvalid();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!IsAdmissibleSnapshotStream(stream))
                return CheckpointStreamInvalid();

            var initialHash = await ComputeSha256Async(stream, cancellationToken);
            if (!string.Equals(
                    initialHash,
                    _identity.BaseCheckpointSha256,
                    StringComparison.Ordinal))
            {
                return CheckpointHashMismatch();
            }

            Rewind(stream);
            if (_providerOptions?.InitialFullCheckpoint is { } declared) // Reuse the same server-owned full-LoRA declaration for actual model loading.
            { // Typed artifact failure cannot be ignored before three independent imports.
                var validation = declared.ValidateArtifact(stream, _ownedConfig, MaximumCheckpointBytes, cancellationToken); // Preserve the retained-byte and exact schema refusal code.
                if (!validation.IsSuccess) return validation.MapError<DesignConversionDpoPolicyBaseModelSet>(); // Return no models and no fallback after a typed refusal.
            } // The declaration changes format admission only, not legal or continuation authority.
            else // Without a declaration, retain the strict base-only validator that refuses adapter-bearing artifacts.
                LlmModel.ValidateFullCheckpointArtifact( // Reuse exact key, shape, payload, fingerprint and EOF checks.
                    stream, _ownedConfig, MaximumCheckpointBytes, cancellationToken); // Preserve caller cancellation at the retained-read and import boundary.

            var policyResult = LoadIndependentModel(stream, cancellationToken); // Create and import the policy only after actual artifact admission.
            if (!policyResult.IsSuccess) return policyResult.MapError<DesignConversionDpoPolicyBaseModelSet>(); // Preserve typed destination refusal without publishing a partial model set.
            if (policyResult.Value is not { } policy) return CheckpointInvalid(); // A missing destination can never be treated as a successful import.
            var referenceResult = LoadIndependentModel(stream, cancellationToken, referenceModel: true); // Preserve the admitted separate reference stream and storage.
            if (!referenceResult.IsSuccess) return referenceResult.MapError<DesignConversionDpoPolicyBaseModelSet>(); // A refused reference prevents every model from escaping.
            if (referenceResult.Value is not { } reference) return CheckpointInvalid(); // Require an actual independently allocated reference destination.
            var freshResult = LoadIndependentModel(stream, cancellationToken); // Import the same saved tensors into a distinct fresh-load destination.
            if (!freshResult.IsSuccess) return freshResult.MapError<DesignConversionDpoPolicyBaseModelSet>(); // Do not substitute another model after fresh-load refusal.
            if (freshResult.Value is not { } freshLoadTarget) return CheckpointInvalid(); // Require the actual third destination before publishing the model set.

            Rewind(stream);
            var finalHash = await ComputeSha256Async(stream, cancellationToken);
            if (!string.Equals(finalHash, initialHash, StringComparison.Ordinal) ||
                !string.Equals(
                    finalHash,
                    _identity.BaseCheckpointSha256,
                    StringComparison.Ordinal))
            {
                return CheckpointChangedDuringLoad();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                new DesignConversionDpoPolicyBaseModelSet(
                    _identity,
                    policy,
                    reference,
                    freshLoadTarget)
                {
                    CanonicalBpeTokenizerSnapshot = tokenizerReference,
                    RandomContext = _currentRandomContext
                });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CheckpointInvalid();
        }
        finally
        {
            DisposeOwnedStream(stream);
        }
    }

    public OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>
        DescribePreparation()
        => OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>.Ok(
            new DesignConversionDpoPolicyBasePreparationDescriptor(
                _identity,
                _ownedConfig));

    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateModelSetAsync(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            CancellationToken cancellationToken)
        => PreparationMatchesOwnedState(preparation)
            ? CreateModelSetAsync(cancellationToken)
            : Task.FromResult(OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                "The DesignConversion DPO policy base preparation descriptor is invalid.",
                IDesignConversionDpoPolicyBaseProvider.PreparationDescriptorUnavailableErrorCode));

    public async Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuedModelSetAsync(
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (continuationRequest is null ||
            continuationRequest.CheckpointSource is null ||
            continuationRequest.SourceEncoderContract is null ||
            continuationRequest.PolicyCheckpointContract is null ||
            continuationRequest.TokenizerArtifact is null ||
            continuationRequest.RuntimeEncodingBinding is null)
        {
            return ContinuationContractInvalid();
        }

        var policyContract = continuationRequest.PolicyCheckpointContract;
        var binding = policyContract.Binding;
        var trainingContext = policyContract.TrainingContext;
        if (string.Equals(
                policyContract.Schema,
                DesignConversionDpoPromotionArtifactContractCodec
                    .LegacyCommittedPolicyCheckpointContractSchema,
                StringComparison.Ordinal) ||
            string.Equals(
                policyContract.Schema,
                DesignConversionDpoPromotionArtifactContractCodec
                    .LegacyPolicyCheckpointContractSchema,
                StringComparison.Ordinal))
        {
            return CheckpointMutationAdmissionFailed(
                DesignConversionDpoCheckpointMutationAdmission.Legacy<bool>());
        }

        if (_providerOptions?.RootInitializationSeed is not ulong rootSeed ||
            !DesignConversionDpoPolicyRandomContextContract.MatchesCurrentAuthority(
                _currentRandomContext,
                rootSeed,
                _providerOptions.TrainingConfigurationFingerprint) ||
            trainingContext?.RandomContext is null ||
            !DesignConversionDpoPolicyRandomContextContract.MatchesCurrentAuthority(
                trainingContext.RandomContext,
                rootSeed,
                _providerOptions.TrainingConfigurationFingerprint) ||
            !Equals(trainingContext.RandomContext, _currentRandomContext))
        {
            return ContinuationRandomContextMismatch();
        }
        if (!DesignConversionDpoPolicyBaseMetadata.IsCanonicalLoraContext(
                trainingContext?.Lora,
                _ownedConfig))
        {
            return ContinuationLoraInvalid();
        }

        if (!DesignConversionDpoPromotionArtifactContractCodec.ValidateContractPair(
                continuationRequest.SourceEncoderContract,
                policyContract) ||
            binding is null ||
            trainingContext is null ||
            !ContinuationIdentityMatches(trainingContext) ||
            !DesignConversionDpoPolicyBaseMetadata.TrainingContextMatchesConfig(
                trainingContext,
                _ownedConfig) ||
            !string.Equals(
                binding.PolicyCheckpointFileName,
                DPO.DpoTrainer.BuildPolicyCheckpointFileName(policyContract.DpoStepCount),
                StringComparison.Ordinal) ||
            !DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(
                binding.PolicyCheckpointSha256) ||
            binding.PolicyCheckpointByteLength <= 0 ||
            binding.PolicyCheckpointByteLength > MaximumCheckpointBytes)
        {
            if (continuationRequest.CheckpointMutationAdmission is not null)
            {
                return CheckpointMutationAdmissionFailed(
                    DesignConversionDpoCheckpointMutationAdmission
                        .Mismatch<bool>());
            }

            return ContinuationContractInvalid();
        }

        // Admission проверяется после прежних current-contract gates, но до
        // открытия tokenizer/checkpoint source: provider без live attestation
        // не получает даже первую возможность загрузить mutable веса.
        var mutationAdmissionValidation =
            DesignConversionDpoCheckpointMutationAdmission
                .ValidateCurrentForContinuation(
                    continuationRequest.CheckpointMutationAdmission,
                    continuationRequest.SourceEncoderContract,
                    policyContract);
        if (!mutationAdmissionValidation.IsSuccess)
            return CheckpointMutationAdmissionFailed(mutationAdmissionValidation);

        var runtimeBinding = DesignConversionDpoPromotionArtifactContractCodec
            .BuildRuntimeEncodingBinding(
                continuationRequest.SourceEncoderContract,
                continuationRequest.TokenizerArtifact);
        if (!runtimeBinding.IsSuccess ||
            runtimeBinding.Value is null ||
            runtimeBinding.Value != continuationRequest.RuntimeEncodingBinding)
        {
            return ContinuationContractInvalid();
        }

        var tokenizerReferenceResult =
            await ResolveContinuationCanonicalBpeTokenizerSnapshotReferenceAsync(
                continuationRequest,
                binding,
                cancellationToken);
        if (!tokenizerReferenceResult.IsSuccess)
            return CanonicalBpeTokenizerFailed(tokenizerReferenceResult);
        var tokenizerReference = tokenizerReferenceResult.Value;

        OperationResult<Stream>? sourceResult;
        try
        {
            sourceResult = await continuationRequest.CheckpointSource
                .OpenCheckpointSnapshotAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();
            return ContinuationCheckpointMismatch();
        }

        if (sourceResult is null || !sourceResult.IsSuccess || sourceResult.Value is null)
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();
            return ContinuationCheckpointMismatch();
        }

        var stream = sourceResult.Value;
        OperationResult<DesignConversionDpoPolicyBaseModelSet>? operationResult = null;
        ExceptionDispatchInfo? processingException = null;
        try
        {
            operationResult = await ProcessContinuedCheckpointStreamAsync(
                stream,
                binding,
                runtimeBinding.Value,
                tokenizerReference,
                _currentRandomContext!,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            operationResult = Cancelled();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            operationResult = ContinuationCheckpointMismatch();
        }
        catch (Exception exception)
        {
            processingException = ExceptionDispatchInfo.Capture(exception);
        }

        try
        {
            stream.Dispose();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // The owned stream was finalized exactly once. An ordinary disposal
            // failure cannot replace the already-normalized operation result.
        }

        processingException?.Throw();
        return operationResult ?? throw new InvalidOperationException(
            "DesignConversion DPO continuation processing produced no outcome.");
    }

    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuedModelSetAsync(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken)
        => PreparationMatchesOwnedState(preparation)
            ? CreateContinuedModelSetAsync(continuationRequest, cancellationToken)
            : Task.FromResult(OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                "The DesignConversion DPO policy base preparation descriptor is invalid.",
                IDesignConversionDpoPolicyBaseProvider.PreparationDescriptorUnavailableErrorCode));

    private bool PreparationMatchesOwnedState(
        DesignConversionDpoPolicyBasePreparationDescriptor? preparation)
        => preparation is not null &&
           string.IsNullOrEmpty(preparation.ProviderBindingRef) &&
           preparation.ProviderBindingGeneration == 0 &&
           preparation.ToIdentity() == _identity &&
           DesignConversionDpoPolicyBaseMetadata.ConfigsEqual(
               preparation.ToConfig(),
               _ownedConfig) &&
           string.Equals(
               preparation.ConfigSha256,
               DesignConversionDpoPolicyBaseMetadata.ComputeConfigSha256(_ownedConfig),
               StringComparison.Ordinal);

    private async Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        ProcessContinuedCheckpointStreamAsync(
            Stream stream,
            DesignConversionDpoPromotionArtifactBinding binding,
            DesignConversionDpoRuntimeEncodingBinding runtimeEncodingBinding,
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference? tokenizerReference,
            DesignConversionDpoPolicyRandomContext randomContext,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsAdmissibleContinuationStream(
                stream,
                binding.PolicyCheckpointByteLength))
        {
            return ContinuationCheckpointMismatch();
        }

        var initialHash = await ComputeSha256Async(
            stream,
            binding.PolicyCheckpointByteLength,
            cancellationToken);
        if (!string.Equals(
                initialHash,
                binding.PolicyCheckpointSha256,
                StringComparison.Ordinal))
        {
            return ContinuationCheckpointMismatch();
        }

        LlmModel policy;
        LlmModel reference;
        LlmModel freshLoadTarget;
        try
        {
            policy = CreateIndependentContinuationModel(
                randomContext,
                DesignConversionDpoPolicyModelRole.Policy,
                cancellationToken);
            reference = CreateIndependentContinuationModel(
                randomContext,
                DesignConversionDpoPolicyModelRole.Reference,
                cancellationToken);
            freshLoadTarget = CreateIndependentContinuationModel(
                randomContext,
                DesignConversionDpoPolicyModelRole.FreshLoad,
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception)
            when ((exception is InvalidOperationException
                   or ArgumentException
                   or OverflowException) &&
                  SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ContinuationLoraInvalid();
        }

        try
        {
            LoadContinuedCheckpoint(policy, stream, cancellationToken);
            LoadContinuedCheckpoint(reference, stream, cancellationToken);
            LoadContinuedCheckpoint(freshLoadTarget, stream, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ContinuationCheckpointMismatch();
        }

        // Checkpoint integrity is authoritative before any model comparison:
        // a between-load mutation is a checkpoint mismatch, not a topology error.
        if (stream.Length != binding.PolicyCheckpointByteLength)
            return ContinuationCheckpointMismatch();
        var finalHash = await ComputeSha256Async(
            stream,
            binding.PolicyCheckpointByteLength,
            cancellationToken);
        if (!string.Equals(finalHash, initialHash, StringComparison.Ordinal) ||
            !string.Equals(
                finalHash,
                binding.PolicyCheckpointSha256,
                StringComparison.Ordinal) ||
            stream.Length != binding.PolicyCheckpointByteLength)
        {
            return ContinuationCheckpointMismatch();
        }

        if (!IndependentContinuationModelsAreIdentical(
                policy,
                reference,
                freshLoadTarget))
        {
            return ContinuationLoraInvalid();
        }

        cancellationToken.ThrowIfCancellationRequested();
        var continuationIdentity = _identity with
        {
            BaseCheckpointSha256 = binding.PolicyCheckpointSha256
        };
        return OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
            new DesignConversionDpoPolicyBaseModelSet(
                continuationIdentity,
                policy,
                reference,
                freshLoadTarget,
                runtimeEncodingBinding)
            {
                CanonicalBpeTokenizerSnapshot = tokenizerReference,
                RandomContext = randomContext
            });
    }

    private async Task<OperationResult<DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>>
        ResolveContinuationCanonicalBpeTokenizerSnapshotReferenceAsync(
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            DesignConversionDpoPromotionArtifactBinding binding,
            CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return CancelledReference();

        var parentReference = continuationRequest.ParentCanonicalBpeTokenizerSnapshot;
        if (binding.CanonicalBpeTokenizer is { } parentBinding)
        {
            if (CanonicalBpeTokenizerReferenceMatchesBinding(
                    parentReference,
                    parentBinding))
            {
                return OperationResult<
                    DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>.Ok(
                    parentReference);
            }

            if (parentReference is not null)
                return ContinuationContractInvalidReference();

            var configuredReference =
                await CreateConfiguredCanonicalBpeTokenizerSnapshotReferenceAsync(
                    cancellationToken);
            if (!configuredReference.IsSuccess)
                return configuredReference;
            return CanonicalBpeTokenizerReferenceMatchesBinding(
                    configuredReference.Value,
                    parentBinding)
                ? OperationResult<
                    DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>.Ok(
                    configuredReference.Value)
                : ContinuationContractInvalidReference();
        }

        if (parentReference is not null)
            return ContinuationContractInvalidReference();

        return await CreateConfiguredCanonicalBpeTokenizerSnapshotReferenceAsync(
            cancellationToken);
    }

    private async Task<OperationResult<DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>>
        CreateConfiguredCanonicalBpeTokenizerSnapshotReferenceAsync(
            CancellationToken cancellationToken)
    {
        if (_expectedCanonicalBpeTokenizerArtifactDescriptor is null &&
            _canonicalBpeTokenizerSnapshotSource is null)
        {
            return OperationResult<
                DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>.Ok(null);
        }

        if (_expectedCanonicalBpeTokenizerArtifactDescriptor is null ||
            _canonicalBpeTokenizerSnapshotSource is null)
        {
            return CanonicalBpeTokenizerInvalidReference();
        }

        if (cancellationToken.IsCancellationRequested)
            return CancelledReference();

        OperationResult<Stream>? sourceResult;
        try
        {
            sourceResult = await _canonicalBpeTokenizerSnapshotSource
                .OpenCanonicalBpeTokenizerSnapshotAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CancelledReference();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            if (cancellationToken.IsCancellationRequested)
                return CancelledReference();
            return CanonicalBpeTokenizerSourceFailedReference();
        }

        if (sourceResult is null || !sourceResult.IsSuccess)
        {
            if (cancellationToken.IsCancellationRequested)
                return CancelledReference();
            return CanonicalBpeTokenizerSourceFailedReference();
        }

        var stream = sourceResult.Value;
        if (stream is null)
        {
            if (cancellationToken.IsCancellationRequested)
                return CancelledReference();
            return CanonicalBpeTokenizerInvalidReference();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var revalidated = DesignConversionDpoPromotionArtifactContractCodec
                .RevalidateCanonicalBpeTokenizerArtifactDescriptor(
                    stream,
                    _ownedConfig,
                    _expectedCanonicalBpeTokenizerArtifactDescriptor,
                    cancellationToken);
            if (!revalidated.IsSuccess || revalidated.Value is null)
            {
                if (cancellationToken.IsCancellationRequested ||
                    string.Equals(
                        revalidated.ErrorCode,
                        CancelledErrorCode,
                        StringComparison.Ordinal))
                {
                    return CancelledReference();
                }

                return CanonicalBpeTokenizerInvalidReference();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return OperationResult<
                DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>.Ok(
                    new DesignConversionDpoCanonicalBpeTokenizerSnapshotReference(
                        revalidated.Value,
                        _ownedConfig,
                        _canonicalBpeTokenizerSnapshotSource));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CancelledReference();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return CanonicalBpeTokenizerInvalidReference();
        }
        finally
        {
            DisposeOwnedStream(stream);
        }
    }

    private bool ContinuationIdentityMatches(
        DesignConversionDpoPolicyCheckpointTrainingContext context)
        => string.Equals(context.ProviderKind, _identity.ProviderKind, StringComparison.Ordinal) &&
           string.Equals(context.ConfigKind, _identity.ConfigurationKind, StringComparison.Ordinal) &&
           string.Equals(context.BaseModelId, _identity.BaseModelId, StringComparison.Ordinal) &&
           string.Equals(context.BaseRevision, _identity.BaseRevision, StringComparison.Ordinal);

    private LlmModel CreateIndependentContinuationModel(
        DesignConversionDpoPolicyRandomContext randomContext,
        DesignConversionDpoPolicyModelRole role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var model = new LlmModel(
            DesignConversionDpoPolicyBaseMetadata.CopyConfig(_ownedConfig));
        DesignConversionDpoPolicyBaseMetadata.AttachSeededCanonicalLoraTopology(
            model,
            randomContext,
            role);
        cancellationToken.ThrowIfCancellationRequested();
        return model;
    }

    // Сверочный набор моделей для доказательства неизменности продолжения.
    // Мост больше не строит модели сам: он просит их у внедрённого поставщика,
    // а отпечаток считает у себя (правило 2 проекта — сначала фабрика).
    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuationVerificationModelSetAsync(
            ModelConfig config,
            DesignConversionDpoPolicyRandomContext randomContext,
            CancellationToken cancellationToken)
    {
        // Отмену уважаем до любой работы над моделями.
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    CancelledErrorMessage,
                    CancelledErrorCode));
        }

        try
        {
            // Три независимые модели строятся из настройки, которую передал
            // мост, и посевом канонной топологии по своей роли — ровно так,
            // как это делал закрытый метод моста до переноса.
            var policy = CreateVerificationModel(
                config,
                randomContext,
                DesignConversionDpoPolicyModelRole.Policy,
                cancellationToken);
            var reference = CreateVerificationModel(
                config,
                randomContext,
                DesignConversionDpoPolicyModelRole.Reference,
                cancellationToken);
            var freshLoadTarget = CreateVerificationModel(
                config,
                randomContext,
                DesignConversionDpoPolicyModelRole.FreshLoad,
                cancellationToken);
            // Личность и случайный контекст переносим в набор без изменений,
            // чтобы мост сверял то же, что сверял раньше.
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Ok(
                    new DesignConversionDpoPolicyBaseModelSet(
                        _identity,
                        policy,
                        reference,
                        freshLoadTarget)
                    {
                        RandomContext = randomContext
                    }));
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Отмена во время построения — это отмена, а не поломка договора.
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    CancelledErrorMessage,
                    CancelledErrorCode));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Нефатальная ошибка построения закрывается кодом про негодную
            // настройку: сверочный набор без этой настройки не существует.
            return Task.FromResult(
                OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
                    "The DesignConversion DPO continuation verification model set could not be built.",
                    ConfigurationInvalidErrorCode));
        }
    }

    // Одна сверочная модель: копия настройки плюс посев канонной топологии по
    // роли. Отличие от обычного пути продолжения только в источнике настройки
    // — здесь её задаёт мост, а не собственная настройка поставщика.
    private static LlmModel CreateVerificationModel(
        ModelConfig config,
        DesignConversionDpoPolicyRandomContext randomContext,
        DesignConversionDpoPolicyModelRole role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var model = new LlmModel(
            DesignConversionDpoPolicyBaseMetadata.CopyConfig(config));
        DesignConversionDpoPolicyBaseMetadata.AttachSeededCanonicalLoraTopology(
            model,
            randomContext,
            role);
        cancellationToken.ThrowIfCancellationRequested();
        return model;
    }

    private static void LoadContinuedCheckpoint(
        LlmModel model,
        Stream stream,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Rewind(stream);
        model.LoadCheckpoint(stream);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private static bool IsAdmissibleContinuationStream(
        Stream stream,
        long expectedByteLength)
    {
        try
        {
            return stream.CanRead &&
                   stream.CanSeek &&
                   stream.Position == 0 &&
                   expectedByteLength > 0 &&
                   expectedByteLength <= MaximumCheckpointBytes &&
                   stream.Length == expectedByteLength;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool IndependentContinuationModelsAreIdentical(
        LlmModel policy,
        LlmModel reference,
        LlmModel freshLoadTarget)
    {
        if (ReferenceEquals(policy, reference) ||
            ReferenceEquals(policy, freshLoadTarget) ||
            ReferenceEquals(reference, freshLoadTarget) ||
            ReferenceEquals(policy.Config, reference.Config) ||
            ReferenceEquals(policy.Config, freshLoadTarget.Config) ||
            ReferenceEquals(reference.Config, freshLoadTarget.Config) ||
            !DesignConversionDpoPolicyBaseMetadata.ConfigsEqual(
                policy.Config,
                reference.Config) ||
            !DesignConversionDpoPolicyBaseMetadata.ConfigsEqual(
                policy.Config,
                freshLoadTarget.Config))
        {
            return false;
        }

        var sets = new[]
        {
            policy.GetCheckpointParameterEntries(),
            reference.GetCheckpointParameterEntries(),
            freshLoadTarget.GetCheckpointParameterEntries()
        };
        if (sets[0].Count == 0 ||
            sets[0].Count != sets[1].Count ||
            sets[0].Count != sets[2].Count)
        {
            return false;
        }

        var parameters = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var tensors = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var storages = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var arrays = new HashSet<object>(ReferenceEqualityComparer.Instance);
        for (var setIndex = 0; setIndex < sets.Length; setIndex++)
        {
            for (var entryIndex = 0; entryIndex < sets[setIndex].Count; entryIndex++)
            {
                var actual = sets[setIndex][entryIndex];
                var expected = sets[0][entryIndex];
                if (!string.Equals(actual.Key, expected.Key, StringComparison.Ordinal) ||
                    !actual.Parameter.Value.Shape.Dimensions.SequenceEqual(
                        expected.Parameter.Value.Shape.Dimensions) ||
                    actual.Parameter.Value.Data.Length != expected.Parameter.Value.Data.Length ||
                    !parameters.Add(actual.Parameter) ||
                    !tensors.Add(actual.Parameter.Value) ||
                    !storages.Add(actual.Parameter.Value.Storage) ||
                    !arrays.Add(actual.Parameter.Value.Data))
                {
                    return false;
                }

                for (var valueIndex = 0;
                     valueIndex < actual.Parameter.Value.Data.Length;
                     valueIndex++)
                {
                    if (BitConverter.SingleToInt32Bits(
                            actual.Parameter.Value.Data[valueIndex]) !=
                        BitConverter.SingleToInt32Bits(
                            expected.Parameter.Value.Data[valueIndex]))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static async Task<string> ComputeSha256Async(
        Stream stream,
        long expectedByteLength,
        CancellationToken cancellationToken)
    {
        Rewind(stream);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[HashBufferBytes];
        long totalBytes = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await stream.ReadAsync(buffer.AsMemory(), cancellationToken);
            if (read == 0)
                break;

            totalBytes = checked(totalBytes + read);
            if (totalBytes > expectedByteLength || totalBytes > MaximumCheckpointBytes)
            {
                throw new InvalidDataException(
                    "Continuation checkpoint exceeded its admitted bound.");
            }
            hash.AppendData(buffer, 0, read);
        }

        if (totalBytes != expectedByteLength)
            throw new InvalidDataException("Continuation checkpoint length changed.");
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private OperationResult<LlmModel> LoadIndependentModel( // Propagate the new typed destination contract through the actual importer.
        Stream stream, // Each destination reads the same retained snapshot rather than a replacement artifact.
        CancellationToken cancellationToken, // Preserve caller cancellation at the retained-read and import boundary.
        bool referenceModel = false) // Select the reference adapter seed stream when true; policy and fresh-load destinations use the policy stream with separate model storage.
    {
        cancellationToken.ThrowIfCancellationRequested();
        Rewind(stream);
        var modelResult = _providerOptions?.InitialFullCheckpoint is { } declared // Use declared seeded slots only for the trusted full-LoRA path.
            ? declared.CreateModelForLoad(CopyConfig(_ownedConfig), referenceModel) // Create a distinct destination whose saved tensors will be imported in full.
            : OperationResult<LlmModel>.Ok(new LlmModel(CopyConfig(_ownedConfig))); // Keep legacy base-only construction and import unchanged.
        if (!modelResult.IsSuccess) return modelResult; // Preserve typed initialization failure without importing into partial state.
        if (modelResult.Value is not { } model) // Canonical success still requires a real destination object.
            return OperationResult<LlmModel>.Fail(CheckpointInvalidErrorMessage, CheckpointInvalidErrorCode); // Fail closed rather than dereferencing or publishing a missing model.
        model.LoadCheckpoint(stream); // Existing full import overwrites every saved base and adapter tensor after slot creation.
        return OperationResult<LlmModel>.Ok(model); // Return only the independently imported destination to the guarded model-set owner.
    }

    private static bool IsAdmissibleSnapshotStream(Stream stream)
    {
        try
        {
            return stream.CanRead &&
                   stream.CanSeek &&
                   stream.Position == 0 &&
                   stream.Length > 0 &&
                   stream.Length <= MaximumCheckpointBytes;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static async Task<string> ComputeSha256Async(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[HashBufferBytes];
        long totalBytes = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await stream.ReadAsync(buffer.AsMemory(), cancellationToken);
            if (read == 0)
                break;

            totalBytes = checked(totalBytes + read);
            if (totalBytes > MaximumCheckpointBytes)
                throw new InvalidDataException("Checkpoint stream exceeded its admitted bound.");
            hash.AppendData(buffer, 0, read);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void Rewind(Stream stream)
        => stream.Position = 0;

    private static void DisposeOwnedStream(Stream stream)
    {
        try
        {
            stream.Dispose();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // The provider owns the stream, but a nonfatal disposal failure
            // cannot replace the stable boundary result or leak source details.
        }
    }

    private static ModelConfig CopyConfig(ModelConfig config)
        => new()
        {
            VocabSize = config.VocabSize,
            ContextLen = config.ContextLen,
            HiddenDim = config.HiddenDim,
            NumLayers = config.NumLayers,
            NumHeads = config.NumHeads,
            NumKvHeads = config.NumKvHeads,
            FfnHiddenDim = config.FfnHiddenDim,
            RopeTheta = config.RopeTheta,
            RmsNormEps = config.RmsNormEps
        };

    private static bool IsExpectedCanonicalBpeTokenizerDescriptorStructurallySafe(
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor descriptor)
        => string.Equals(
               descriptor.Schema,
               DesignConversionDpoPromotionArtifactContractCodec
                   .CanonicalBpeTokenizerArtifactDescriptorSchema,
               StringComparison.Ordinal) &&
           string.Equals(
               descriptor.ArtifactKind,
               DesignConversionDpoPromotionArtifactContractCodec
                   .CanonicalBpeTokenizerArtifactKind,
               StringComparison.Ordinal) &&
           DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(descriptor.Sha256) &&
           descriptor.ByteLength > 0 &&
           descriptor.ByteLength <= BpeTokenizer.MaximumCanonicalArtifactBytes &&
           descriptor.VocabularySize > 0 &&
           DesignConversionDpoPolicyBaseMetadata.IsCanonicalSha256(
               descriptor.CanonicalStateIdentitySha256) &&
           string.Equals(
               descriptor.Sha256,
               descriptor.CanonicalStateIdentitySha256,
               StringComparison.Ordinal) &&
           !descriptor.RawPayloadPersisted &&
           !descriptor.AbsolutePathPersisted;

    private static bool CanonicalBpeTokenizerReferenceMatchesBinding(
        DesignConversionDpoCanonicalBpeTokenizerSnapshotReference? reference,
        DesignConversionDpoCanonicalBpeTokenizerArtifactBinding binding)
    {
        if (reference is null ||
            binding.Descriptor is null ||
            !IsExpectedCanonicalBpeTokenizerDescriptorStructurallySafe(
                binding.Descriptor))
        {
            return false;
        }

        var descriptor = reference.Descriptor;
        return IsExpectedCanonicalBpeTokenizerDescriptorStructurallySafe(descriptor) &&
               string.Equals(binding.Sha256, descriptor.Sha256, StringComparison.Ordinal) &&
               binding.ByteLength == descriptor.ByteLength &&
               string.Equals(binding.Descriptor.Schema, descriptor.Schema, StringComparison.Ordinal) &&
               string.Equals(
                   binding.Descriptor.ArtifactKind,
                   descriptor.ArtifactKind,
                   StringComparison.Ordinal) &&
               string.Equals(binding.Descriptor.Sha256, descriptor.Sha256, StringComparison.Ordinal) &&
               binding.Descriptor.ByteLength == descriptor.ByteLength &&
               binding.Descriptor.VocabularySize == descriptor.VocabularySize &&
               string.Equals(
                   binding.Descriptor.CanonicalStateIdentitySha256,
                   descriptor.CanonicalStateIdentitySha256,
                   StringComparison.Ordinal) &&
               binding.Descriptor.RawPayloadPersisted == descriptor.RawPayloadPersisted &&
               binding.Descriptor.AbsolutePathPersisted == descriptor.AbsolutePathPersisted;
    }

    private static OperationResult<DesignConversionCheckpointBackedDpoPolicyBaseProvider>
        ConfigurationInvalid()
        => OperationResult<DesignConversionCheckpointBackedDpoPolicyBaseProvider>.Fail(
            ConfigurationInvalidErrorMessage,
            ConfigurationInvalidErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet> Cancelled()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            CancelledErrorMessage,
            CancelledErrorCode);

    private static OperationResult<DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>
        CancelledReference()
        => OperationResult<
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>.Fail(
            CancelledErrorMessage,
            CancelledErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        CheckpointSourceFailed()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            CheckpointSourceFailedErrorMessage,
            CheckpointSourceFailedErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        CheckpointStreamInvalid()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            CheckpointStreamInvalidErrorMessage,
            CheckpointStreamInvalidErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        CheckpointHashMismatch()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            CheckpointHashMismatchErrorMessage,
            CheckpointHashMismatchErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        CheckpointChangedDuringLoad()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            CheckpointChangedDuringLoadErrorMessage,
            CheckpointChangedDuringLoadErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        CheckpointInvalid()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            CheckpointInvalidErrorMessage,
            CheckpointInvalidErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        ContinuationContractInvalid()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            ContinuationContractInvalidErrorMessage,
            ContinuationContractInvalidErrorCode);

    private static OperationResult<DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>
        ContinuationContractInvalidReference()
        => OperationResult<
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>.Fail(
            ContinuationContractInvalidErrorMessage,
            ContinuationContractInvalidErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        ContinuationCheckpointMismatch()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            ContinuationCheckpointMismatchErrorMessage,
            ContinuationCheckpointMismatchErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        ContinuationLoraInvalid()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            ContinuationLoraInvalidErrorMessage,
            ContinuationLoraInvalidErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        ContinuationRandomContextMismatch()
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            ContinuationRandomContextMismatchErrorMessage,
            ContinuationRandomContextMismatchErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        CheckpointMutationAdmissionFailed(OperationResult<bool> validation)
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            validation.ErrorMessage ??
            DesignConversionDpoCheckpointMutationAdmission
                .SafeAttestationRequiredMessage,
            validation.ErrorCode ??
            DesignConversionDpoCheckpointMutationAdmission
                .AttestationRequiredErrorCode);

    private static OperationResult<DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>
        CanonicalBpeTokenizerSourceFailedReference()
        => OperationResult<
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>.Fail(
            CanonicalBpeTokenizerSourceFailedErrorMessage,
            CanonicalBpeTokenizerSourceFailedErrorCode);

    private static OperationResult<DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>
        CanonicalBpeTokenizerInvalidReference()
        => OperationResult<
            DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>.Fail(
            CanonicalBpeTokenizerInvalidErrorMessage,
            CanonicalBpeTokenizerInvalidErrorCode);

    private static OperationResult<DesignConversionDpoPolicyBaseModelSet>
        CanonicalBpeTokenizerFailed(
            OperationResult<DesignConversionDpoCanonicalBpeTokenizerSnapshotReference?>
                tokenizerReferenceResult)
        => OperationResult<DesignConversionDpoPolicyBaseModelSet>.Fail(
            tokenizerReferenceResult.ErrorMessage ?? CanonicalBpeTokenizerInvalidErrorMessage,
            tokenizerReferenceResult.ErrorCode ?? CanonicalBpeTokenizerInvalidErrorCode);
}
