// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace ContinuousLearning; // Keep saved-root admission in the existing learning owner.

using System.Security.Cryptography; // Stream exact artifact digests without allocating model tensors.
using AgentInfrastructure; // Return the canonical result type to every new public caller.
using AgentInfrastructure.Logical; // Reuse the shared nonfatal exception boundary.
using LoRA; // Reuse canonical topology and deterministic adapter initialization.
using TransformerModel; // Reuse the existing full-checkpoint codec and model importer.
using TransformerModel.Config; // Bind the complete admitted transformer shape.

/// <summary>
/// Server-owned, no-model declaration of one verified initial full-LoRA parent.
/// It is not a legal-training, optimizer cold-start, or continuation grant.
/// </summary>
public sealed class RepositoryOwnedFullLoraCheckpointDescriptor // The declaration carries saved bytes and topology, never execution permission.
{ // This owner keeps immutable metadata separate from retained file capabilities.
    private readonly ModelConfig _modelConfig; // Retain the exact admitted base shape.
    private readonly LoraConfig _loraConfig; // Retain canonical rank, alpha, targets and zero dropout.
    private readonly LoraPairRandomSeedAuthority _seedAuthority; // Retain the existing separate policy/reference seed streams.
    private readonly FullCheckpointLoraTopology _topology; // Describe only the exact serialized adapter slots.
    private const string IdentityMismatch = "REPOSITORY_OWNED_FULL_LORA_CHECKPOINT_IDENTITY_MISMATCH"; // Keep size/hash refusals typed without exposing private paths.

    internal RepositoryOwnedFullLoraCheckpointDescriptor( // Only the trusted existing server owner can mint this saved-root declaration.
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest, // Carry the denied-cold-start repository lane already admitted by generation.
        ModelConfig modelConfig, // Capture the same model shape that startup must independently match.
        LoraConfig loraConfig, // Use the explicit adapter profile rather than changing generic trainer defaults.
        LoraPairRandomSeedAuthority seedAuthority, // Preserve the declared initialization and role streams.
        RepositoryOwnedModelCheckpointLoadResult verifiedParent) // Accept metadata from the existing strict sealed-parent loader.
    { // This no-model constructor checks provenance metadata before retaining it.
        ArgumentNullException.ThrowIfNull(verifiedParent); // A declaration cannot exist without a loader-owned verified parent.
        var canonical = LoraApplicator.ValidateAndCanonicalize(loraConfig); // Reuse the canonical adapter allowlist and numeric checks.
        if (canonical.Dropout != 0f || canonical.TargetModules.Length == 0 // This bounded root profile requires real slots with zero dropout.
            || startRequest.ColdStartPolicy.IsAuthorized // A saved-root declaration cannot authorize fresh model creation.
            || verifiedParent.Manifest.Step != 1 // Only the existing first checkpoint may enter this initial path.
            || verifiedParent.Manifest.ParentCheckpoint is not null // A descendant cannot acquire initial-root semantics.
            || verifiedParent.Manifest.DpoApplication != RepositoryOwnedModelCheckpointDpoApplication.InitialBaselineNoUpdate // Preserve the explicit untrained-root provenance.
            || LoraRandomStateIdentity.ComputeTopologySha256(modelConfig, canonical, seedAuthority) // Recompute topology using the existing canonical identity owner.
                != startRequest.RuntimeCompatibility.TopologySha256) // Require exact rank, alpha, targets, dropout and seed agreement.
            throw new ArgumentException("Initial full-LoRA checkpoint declaration is invalid."); // Reject misuse inside the trusted constructor without minting a declaration.

        StartRequest = startRequest with { }; // Own the immutable repository identity snapshot.
        _modelConfig = modelConfig with { }; // Prevent caller replacement from changing the admitted shape.
        _loraConfig = canonical with { TargetModules = canonical.TargetModules.ToArray() }; // Own the target array rather than sharing mutable configuration storage.
        _seedAuthority = seedAuthority with { }; // Keep both role streams attached to this exact declaration.
        _topology = new FullCheckpointLoraTopology(canonical.Rank, canonical.TargetModules); // Derive the codec schema from admitted canonical adapter metadata.
        ManifestSha256 = verifiedParent.Manifest.ManifestSha256; // Bind the exact canonical root manifest selected by the server owner.
        WeightsSha256 = verifiedParent.WeightsSha256; // Bind the strict loader's verified weight bytes.
        WeightsByteLength = verifiedParent.WeightsByteLength; // Keep retained reads at the sealed artifact length.
    } // The verified stream remains owned and disposed by its existing loader caller.

    internal RepositoryOwnedIncrementalTrainingSessionStartRequest StartRequest { get; } // Startup must compare this exact lane against its own deferred request.
    internal LoraConfig LoraConfig => _loraConfig with { TargetModules = _loraConfig.TargetModules.ToArray() }; // Consumers receive a copy of the same zero-dropout profile.
    public string ManifestSha256 { get; } // Expose saved-parent identity without exposing a filesystem path.
    public string WeightsSha256 { get; } // Let retained consumers require the same sealed digest.
    public long WeightsByteLength { get; } // Let retained consumers require the same sealed size.
    public ulong RootInitializationSeed => _seedAuthority.RootInitializationSeed; // Preserve the existing root initialization authority.
    public OperationResult<bool> MatchesModelConfig(ModelConfig config) // Return a contextual refusal rather than a successful false shape comparison.
        => config == _modelConfig // Compare all fields of the immutable model record.
            ? OperationResult<bool>.Ok(true) // Success means this is the declared shape, never merely a completed comparison.
            : OperationResult<bool>.Fail("Declared full-LoRA model configuration mismatch.", "LLM_CHECKPOINT_CONFIG_MISMATCH"); // A different shape cannot reach slot creation or artifact admission.

    /// <summary>Checks retained bytes against the sealed parent's exact hash and schema; caller cancellation still propagates.</summary>
    public OperationResult<bool> ValidateArtifact(Stream stream, ModelConfig config, long maximumBytes, // Return typed byte/schema failures without constructing a model.
        CancellationToken cancellationToken = default) // Preserve the caller's cancellation authority across the streaming checks.
    { // Neither a valid artifact nor this result grants training or optimizer permission.
        var match = MatchesModelConfig(config); // Refuse a different base shape before reading its claimed adapter artifact.
        if (!match.IsSuccess) return match; // Preserve the exact typed configuration refusal.
        try // Keep malformed streams and read failures inside the public result boundary.
        { // The same retained stream supplies both the digest and the full keyed schema.
            cancellationToken.ThrowIfCancellationRequested(); // Do not start retained I/O after explicit caller cancellation.
            if (stream is null || !stream.CanRead || !stream.CanSeek // A retained snapshot must support bounded replay for validation and import.
                || stream.Length != WeightsByteLength || stream.Length > maximumBytes) // Shape metadata cannot substitute for exact sealed size.
                return OperationResult<bool>.Fail("Declared full-LoRA checkpoint identity mismatch.", IdentityMismatch); // Refuse a different artifact before hashing or model allocation.
            stream.Position = 0; // Hash the complete retained artifact, not a caller-selected suffix.
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); // Reuse SHA256 for the sealed byte identity.
            var buffer = new byte[128 * 1024]; // Bound validation memory independently of the model size.
            int read; // Track each bounded chunk without allocating tensor storage.
            while ((read = stream.Read(buffer)) != 0) // Consume the actual retained bytes through EOF.
            { // Each chunk contributes only to the immutable-parent comparison.
                cancellationToken.ThrowIfCancellationRequested(); // Cancellation remains an exception rather than a malformed-artifact result.
                hash.AppendData(buffer, 0, read); // Include every read byte in the sealed digest.
            } // No model or training state is created by this streaming pass.
            if (Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant() != WeightsSha256) // Require the strict loader's exact weight digest.
                return OperationResult<bool>.Fail("Declared full-LoRA checkpoint identity mismatch.", IdentityMismatch); // Do not accept a same-shaped replacement checkpoint.
            stream.Position = 0; // Reuse the same bytes for the existing exact keyed codec.
            return LlmModel.ValidateFullCheckpointArtifact(stream, config, _topology, maximumBytes, cancellationToken); // Preserve exact codec count/key/shape/payload error codes.
        } // A successful result means exact byte and schema admission only.
        catch (OperationCanceledException) // Keep the existing explicit cancellation control flow visible to callers.
        { // Cancellation cannot be downgraded to a business refusal.
            throw; // Preserve the original caller cancellation and token.
        } // No artifact-success result is produced after cancellation.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Reuse the shared boundary without masking fatal process failures.
        { // Stream and I/O business failures must leave the public API as a typed refusal.
            return OperationResult<bool>.Fail("Declared full-LoRA checkpoint could not be validated.", "REPOSITORY_OWNED_FULL_LORA_CHECKPOINT_INVALID"); // Exclude private paths and raw exceptions from the result.
        } // Failure never authorizes a fallback artifact or fresh model.
    } // The caller still owns the retained stream and its capability lifetime.

    /// <summary>Creates seeded slots before existing full import; it does not load weights or grant execution rights.</summary>
    public OperationResult<LlmModel> CreateModelForLoad(ModelConfig config, bool referenceModel = false) // Return a typed destination-creation result to every retained consumer.
    { // Each successful call must create an independent destination for all saved tensors.
        var match = MatchesModelConfig(config); // A destination must expose exactly the declared base shape.
        if (!match.IsSuccess) return match.MapError<LlmModel>(); // Preserve contextual shape refusal without allocating a model.
        try // Convert model/topology initialization failures to the canonical public result.
        { // Slot creation precedes full import, so saved adapters are never reset after loading.
            var model = new LlmModel(config with { }); // Own fresh base storage for this one policy/reference/fresh-load copy.
            LoraApplicator.ApplySeeded(model, LoraConfig, RootInitializationSeed, // Reuse canonical seeded slot creation and its base-freeze behavior.
                referenceModel ? _seedAuthority.ReferenceDropoutStreamDomain : _seedAuthority.PolicyDropoutStreamDomain); // Preserve the declared independent role streams.
            return OperationResult<LlmModel>.Ok(model); // The existing importer must still overwrite every base and adapter tensor.
        } // No checkpoint or optimizer bytes are created here.
        catch (OperationCanceledException) // Preserve existing cancellation behavior if initialization reports cancellation.
        { // A cancelled destination cannot be represented as a business success.
            throw; // Keep cancellation separate from malformed topology.
        } // No model is returned from the cancelled branch.
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception)) // Reuse the shared nonfatal classification for initialization failures.
        { // A failed destination cannot escape as a partially usable model.
            return OperationResult<LlmModel>.Fail("Declared full-LoRA destination could not be initialized.", "REPOSITORY_OWNED_FULL_LORA_MODEL_INITIALIZATION_FAILED"); // Return no model and no fallback authority.
        } // Existing callers must check this result before importing saved weights.
    } // Retained capability and later legal/optimizer checks remain at their existing owners.
} // The descriptor remains metadata, not a training or continuation capability.
