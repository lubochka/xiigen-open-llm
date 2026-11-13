// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Security.Cryptography;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using LoRA;
using TransformerModel;
using TransformerModel.Config;

/// <summary>
/// Canonical repository-owned startup request for the CodeDocumentation common
/// Transformer model. The request names an exact model tree under
/// training-artifacts/models and the exact runtime fingerprints the sealed
/// parent checkpoint must match before any model instance is returned.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedModelCheckpointStartRequest(
    string RepositoryRoot,
    string DomainSlug,
    string DesignPatternSlug,
    string ModelIdSlug,
    string ModelVersion,
    ModelConfig ModelConfig,
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    RepositoryOwnedIncrementalTrainingColdStartPolicy ColdStartPolicy,
    long MaximumCheckpointBytes = 16L * 1024L * 1024L * 1024L,
    LoraConfig? LoraConfig = null,
    LoraPairRandomSeedAuthority? AdapterSeedAuthority = null, // Сохраняем прежнюю отдельную authority адаптеров.
    ulong? BaseInitializationSeed = null); // Только явный opt-in задаёт воспроизводимые базовые веса нового cold start.

/// <summary>
/// Result of a CodeDocumentation model START boundary. A parent manifest means
/// the returned model was loaded from sealed repository bytes; a null parent
/// means a human-authorized clean cold start created a fresh model.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedModelCheckpointStart(
    LlmModel Model,
    RepositoryOwnedModelCheckpointManifest? ParentManifest,
    string? ColdStartReason,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Diagnostics,
    bool SeededTopologyPreparedByRepositoryStart = true);

/// <summary>
/// A training-attempt policy checkpoint reopened from disk into a fresh model
/// instance and checked by the checkpoint file hash.
/// </summary>
public sealed record CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad(
    LlmModel Model,
    string WeightsPath,
    string RepositoryRelativeWeightsPath,
    string WeightsSha256,
    long WeightsByteLength,
    IReadOnlyList<string> EvidenceRefs);

/// <summary>
/// Starts the CodeDocumentation common LlmModel from repository-owned model
/// checkpoints only. This service deliberately does not train, publish,
/// mutate command-line wiring, or create repository artifacts; it only chooses
/// the safe START state for the next bounded training slice.
/// </summary>
public static class CodeDocumentationRepositoryOwnedModelCheckpointStartService
{
    // These evidence constants are returned only after the named safety boundary
    // has actually run, so downstream reports can distinguish repository load,
    // full checkpoint validation, and the final LlmModel.LoadCheckpoint import.
    public const string LoadedFromRepositoryEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_LOADED";
    public const string FullCheckpointValidatedEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_FULL_LLM_CHECKPOINT_VALIDATED";
    public const string LlmModelLoadCheckpointCalledEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_LLM_MODEL_LOAD_CHECKPOINT_CALLED";
    public const string ColdStartAuthorizedEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_COLD_START_AUTHORIZED";
    public const string TrainingAttemptCheckpointReopenedEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_ATTEMPT_CHECKPOINT_REOPENED";
    public const string TrainingAttemptCheckpointHashVerifiedEvidence =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_ATTEMPT_CHECKPOINT_HASH_VERIFIED";

    // These error codes preserve the fail-closed lifecycle contract: invalid
    // identity, malformed payload, unauthorized empty lineage, and cancellation
    // stay separate instead of being collapsed into a silent fresh model.
    public const string InvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_START_INVALID";
    public const string CheckpointPayloadInvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PAYLOAD_INVALID";
    public const string TrainingAttemptCheckpointInvalidErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_TRAINING_ATTEMPT_CHECKPOINT_INVALID";
    public const string ColdStartNotAuthorizedErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_COLD_START_NOT_AUTHORIZED";
    public const string CancelledErrorCode =
        "CODE_DOCUMENTATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_START_CANCELLED";

    public static OperationResult<CodeDocumentationRepositoryOwnedModelCheckpointStart>
        PrepareStart(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            CancellationToken cancellationToken = default,
            bool initialOnly = false) // Opt-in initial control refuses checkpoint candidates before payload reads; legacy callers keep their default.
    {
        // WHY: validate the full repository-owned target before touching disk
        // so a caller cannot redirect START to another domain, another model
        // configuration, or a noncanonical repository path.
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
            return validation.MapError<CodeDocumentationRepositoryOwnedModelCheckpointStart>();

        // Cancellation before discovery is reported through this service's
        // stable result envelope, matching the later validation/load boundary.
        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<CodeDocumentationRepositoryOwnedModelCheckpointStart>.Fail(
                "CodeDocumentation repository-owned LlmModel START was cancelled.",
                CancelledErrorCode);
        }

        // RepositoryOwnedModelCheckpointLoader owns the sealed-tree mechanics:
        // latest numeric step selection, manifest/hash verification, and
        // compatibility matching are delegated to the existing shared loader.
        var loaded = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                request.RepositoryRoot,
                request.DomainSlug,
                request.DesignPatternSlug,
                request.ModelIdSlug,
                request.ModelVersion,
                request.RuntimeCompatibility,
                initialOnly); // The loader owns the discovery-only refusal before custody or accepted-state reads.
        if (loaded.IsSuccess && loaded.Value is not null)
        {
            // The load result owns a fresh weight stream; disposing it after the
            // model import prevents a later caller from reusing consumed bytes.
            using var loadedCheckpoint = loaded.Value;

            // Full checkpoint validation happens before model construction so
            // hostile or mismatched bytes cannot allocate and partially mutate
            // a Transformer model instance.
            var freshModel = LoadIntoFreshModel(
                loadedCheckpoint.Weights,
                request.ModelConfig,
                request.MaximumCheckpointBytes,
                request.LoraConfig,
                request.AdapterSeedAuthority,
                cancellationToken);
            if (!freshModel.IsSuccess || freshModel.Value is null)
                return freshModel.MapError<CodeDocumentationRepositoryOwnedModelCheckpointStart>();

            // Success returns both the restored model and the parent manifest,
            // because the next training slice must continue lineage from these
            // exact repository-owned bytes rather than from a hidden baseline.
            return OperationResult<CodeDocumentationRepositoryOwnedModelCheckpointStart>.Ok(
                new CodeDocumentationRepositoryOwnedModelCheckpointStart(
                    freshModel.Value,
                    loadedCheckpoint.Manifest,
                    ColdStartReason: null,
                    [
                        LoadedFromRepositoryEvidence,
                        FullCheckpointValidatedEvidence,
                        LlmModelLoadCheckpointCalledEvidence,
                        "repository_manifest:" +
                        loadedCheckpoint.Manifest.RepositoryRelativeManifestPath,
                        "repository_weights:" +
                        loadedCheckpoint.Manifest.RepositoryRelativeWeightsPath
                    ],
                    loadedCheckpoint.Diagnostics
                        .Append(
                            "CodeDocumentation START validated the full checkpoint stream against ModelConfig before constructing a fresh LlmModel and calling LoadCheckpoint.")
                        .ToArray()));
        }

        // Clean absence is the only cold-start route: no checkpoint tree or no
        // sealed candidate means this exact lineage has not begun yet. The
        // human reason makes that first empty model auditable.
        if (IsCleanMissingRepositoryOwnedModelCheckpoint(loaded) &&
            request.ColdStartPolicy.IsAuthorized)
        {
            // The cold-start model is fresh but still repository-owned: the
            // caller must publish any later trained checkpoint back to the
            // canonical model tree in a separate admitted slice.
            var coldModel = CreateColdStartModel(
                request.ModelConfig,
                request.LoraConfig,
                request.AdapterSeedAuthority, // Authority LoRA не меняет legacy инициализацию базы.
                request.BaseInitializationSeed); // Отдельная optional authority действует только при clean missing.
            if (!coldModel.IsSuccess || coldModel.Value is null)
                return coldModel.MapError<CodeDocumentationRepositoryOwnedModelCheckpointStart>();

            // ParentManifest remains null intentionally; the explicit reason is
            // the lineage root and prevents a quiet fallback from being mistaken
            // for continuation from a saved model.
            return OperationResult<CodeDocumentationRepositoryOwnedModelCheckpointStart>.Ok(
                new CodeDocumentationRepositoryOwnedModelCheckpointStart(
                    coldModel.Value,
                    ParentManifest: null,
                    request.ColdStartPolicy.HumanReason,
                    [ColdStartAuthorizedEvidence],
                    [
                        string.IsNullOrWhiteSpace(loaded.ErrorMessage)
                            ? "No sealed CodeDocumentation LlmModel checkpoint exists for this exact repository-owned identity."
                            : loaded.ErrorMessage
                    ]));
        }

        // Missing checkpoint without a human reason fails closed, because an
        // accidental empty model would erase the requirement to continue from
        // repository-owned weights when they are expected.
        if (IsCleanMissingRepositoryOwnedModelCheckpoint(loaded))
        {
            return OperationResult<CodeDocumentationRepositoryOwnedModelCheckpointStart>.Fail(
                "CodeDocumentation repository-owned LlmModel cold start is denied: no sealed parent exists for this exact model identity and no explicit nonempty human reason was supplied.",
                ColdStartNotAuthorizedErrorCode);
        }

        // Any non-clean loader failure is authoritative: corrupt, incompatible,
        // ambiguous, invalid, or unavailable checkpoint state must not fall back
        // to an empty model, even when a cold-start policy was supplied.
        return loaded.MapError<CodeDocumentationRepositoryOwnedModelCheckpointStart>();
    }

    /// <summary>
    /// Reopens the policy checkpoint written by one repository-owned training
    /// attempt. The result is a fresh model instance loaded from that exact
    /// disk file, not the live model object that performed training.
    /// </summary>
    public static OperationResult<
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>
        LoadTrainingAttemptCheckpoint(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest? request,
            RepositoryOwnedModelTrainingAttemptAddress? attemptAddress,
            int dpoStepCount,
            CancellationToken cancellationToken = default)
    {
        var requestValidation = ValidateRequest(request);
        if (!requestValidation.IsSuccess)
        {
            return requestValidation.MapError<
                CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>();
        }

        if (attemptAddress is null ||
            dpoStepCount <= 0 ||
            !RepositoryOwnedTrainingArtifactLayout
                .IsValidModelTrainingAttemptAddress(attemptAddress) ||
            !StringComparerForPaths.Equals(
                request!.RepositoryRoot,
                attemptAddress.RepositoryRoot) ||
            !StringComparer.Ordinal.Equals(
                request.DomainSlug,
                attemptAddress.DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                request.DesignPatternSlug,
                attemptAddress.DesignPatternSlug) ||
            !StringComparer.Ordinal.Equals(
                request.ModelIdSlug,
                attemptAddress.ModelIdSlug) ||
            !StringComparer.Ordinal.Equals(
                request.ModelVersion,
                attemptAddress.ModelVersion))
        {
            return TrainingAttemptCheckpointInvalid();
        }

        var checkpointPath = ResolveTrainingAttemptPolicyCheckpointPath(
            request,
            attemptAddress,
            dpoStepCount);
        if (!checkpointPath.IsSuccess || checkpointPath.Value is null)
        {
            return checkpointPath.MapError<
                CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>();
        }

        var fileIdentity = HashTrainingAttemptCheckpointFile(
            request,
            attemptAddress,
            checkpointPath.Value,
            cancellationToken);
        if (!fileIdentity.IsSuccess || fileIdentity.Value is null)
        {
            return fileIdentity.MapError<
                CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>();
        }

        OperationResult<LlmModel> loaded;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var stream = File.OpenRead(checkpointPath.Value);
            if (stream.Length != fileIdentity.Value.ByteLength)
                return TrainingAttemptCheckpointInvalid();

            loaded = LoadIntoFreshModel(
                stream,
                request.ModelConfig,
                request.MaximumCheckpointBytes,
                request.LoraConfig,
                request.AdapterSeedAuthority,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return TrainingAttemptCheckpointInvalid();
        }

        if (!loaded.IsSuccess || loaded.Value is null)
        {
            return loaded.MapError<
                CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>();
        }

        var modelIdentity = HashModelSerialization(
            loaded.Value,
            request.MaximumCheckpointBytes,
            cancellationToken);
        if (!modelIdentity.IsSuccess || modelIdentity.Value is null)
        {
            return modelIdentity.MapError<
                CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>();
        }

        if (modelIdentity.Value.ByteLength != fileIdentity.Value.ByteLength ||
            !StringComparer.Ordinal.Equals(
                modelIdentity.Value.Sha256,
                fileIdentity.Value.Sha256))
        {
            return TrainingAttemptCheckpointInvalid();
        }

        return OperationResult<
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>.Ok(
            new CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad(
                loaded.Value,
                checkpointPath.Value,
                fileIdentity.Value.RepositoryRelativePath,
                fileIdentity.Value.Sha256,
                fileIdentity.Value.ByteLength,
                [
                    TrainingAttemptCheckpointReopenedEvidence,
                    TrainingAttemptCheckpointHashVerifiedEvidence,
                    "training_attempt_weights:" +
                    fileIdentity.Value.RepositoryRelativePath
                ]));
    }

    public static OperationResult<bool> ValidateRequest(
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest? request)
    {
        // The START request is all-or-nothing. Missing model configuration,
        // runtime fingerprints, cold-start policy, or byte limit means the
        // service cannot prove what model tree or payload it is allowed to use.
        if (request is null ||
            request.ModelConfig is null ||
            request.RuntimeCompatibility is null ||
            request.ColdStartPolicy is null ||
            request.MaximumCheckpointBytes <= 0)
        {
            return Invalid();
        }

        // CodeDocumentation may own several documentation domains, but every
        // accepted domain must come from the immutable allowlist maintained by
        // the Direct Preference Optimization dataset authoring boundary.
        var domainProfile = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring
            .ResolveSupportedDomainProfile(request.DomainSlug);
        if (!domainProfile.IsSuccess || domainProfile.Value is null)
            return Invalid();

        // The model checkpoint path is tied to the domain profile's design
        // pattern; accepting a mismatched pattern would let one domain borrow
        // another domain's saved weights.
        if (!StringComparer.Ordinal.Equals(
                domainProfile.Value.DesignPatternSlug,
                request.DesignPatternSlug))
        {
            return Invalid();
        }

        // These pure validation checks keep model shape, runtime compatibility,
        // ModelConfig Secure Hash Algorithm 256-bit (SHA-256), and cold-start
        // authorization consistent before the loader opens checkpoint bytes.
        if (!ModelConfigIsWellFormed(request.ModelConfig) ||
            !RuntimeCompatibilityIsWellFormed(request.RuntimeCompatibility) ||
            !RuntimeCompatibilityMatchesModelConfig(
                request.RuntimeCompatibility,
                request.ModelConfig) ||
            !ColdStartPolicyIsWellFormed(request.ColdStartPolicy) ||
            !LoraConfigIsWellFormed(request.LoraConfig))
        {
            return Invalid();
        }

        // WHY: layout validation proves this is the canonical repository-owned
        // model address: fully-qualified repo root, sentinels, safe slugs,
        // descendant paths, and no existing reparse traversal.
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                request.RepositoryRoot,
                request.DomainSlug,
                request.DesignPatternSlug,
                request.ModelIdSlug,
                request.ModelVersion,
                step: 1);

        // A valid step-one address proves the whole model identity is expressible
        // in the repository-owned layout; discovery can then look for any latest
        // compatible step under the same validated identity.
        return address.IsSuccess
            ? OperationResult<bool>.Ok(true)
            : Invalid();
    }

    private static OperationResult<LlmModel> LoadIntoFreshModel(
        Stream weights,
        ModelConfig modelConfig,
        long maximumCheckpointBytes,
        LoraConfig? loraConfig,
        LoraPairRandomSeedAuthority? adapterSeedAuthority,
        CancellationToken cancellationToken)
    {
        try
        {
            // The same cancellation token protects both streaming validation and
            // the subsequent import boundary, so START cannot finish after the
            // caller has explicitly abandoned it.
            cancellationToken.ThrowIfCancellationRequested();

            var model = TryLoadAdapterFreeCheckpoint(
                weights,
                modelConfig,
                maximumCheckpointBytes,
                cancellationToken);
            if (!model.IsSuccess && loraConfig is not null)
            {
                model = TryLoadLoraCheckpoint(
                    weights,
                    modelConfig,
                    maximumCheckpointBytes,
                    loraConfig,
                    adapterSeedAuthority,
                    cancellationToken);
            }
            if (!model.IsSuccess || model.Value is null)
                return model;

            // Returning the model here proves both validation and import reached
            // the same exact stream without falling back to default weights.
            return model;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cancellation is expected control flow, not corrupt checkpoint
            // evidence; keep it separate for callers and tests.
            return OperationResult<LlmModel>.Fail(
                "CodeDocumentation repository-owned LlmModel START was cancelled.",
                CancelledErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Malformed, wrong-config, unsupported, or unreadable payloads all
            // fail closed as checkpoint payload errors. The returned model is
            // absent, so no caller can accidentally train from a bad baseline.
            return OperationResult<LlmModel>.Fail(
                "Repository-owned CodeDocumentation checkpoint payload is not a valid full LlmModel checkpoint for the requested ModelConfig.",
                CheckpointPayloadInvalidErrorCode);
        }
    }

    private static OperationResult<LlmModel> TryLoadAdapterFreeCheckpoint(
        Stream weights,
        ModelConfig modelConfig,
        long maximumCheckpointBytes,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validation expects to read from the beginning of the loader-owned
            // stream; seekable checked streams are reset without changing bytes.
            if (weights.CanSeek)
                weights.Position = 0;

            // Adapter-free checkpoints can still use the allocation-safe static
            // validator before constructing the destination model.
            LlmModel.ValidateFullCheckpointArtifact(
                weights,
                modelConfig,
                maximumCheckpointBytes,
                cancellationToken);

            weights.Position = 0;
            var model = new LlmModel(modelConfig);
            model.LoadCheckpoint(weights);
            return OperationResult<LlmModel>.Ok(model);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<LlmModel>.Fail(
                "Repository-owned CodeDocumentation checkpoint payload is not an adapter-free full LlmModel checkpoint.",
                CheckpointPayloadInvalidErrorCode);
        }
    }

    private static OperationResult<LlmModel> TryLoadLoraCheckpoint(
        Stream weights,
        ModelConfig modelConfig,
        long maximumCheckpointBytes,
        LoraConfig loraConfig,
        LoraPairRandomSeedAuthority? adapterSeedAuthority,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (weights.Length <= 0 || weights.Length > maximumCheckpointBytes)
            {
                return OperationResult<LlmModel>.Fail(
                    "Repository-owned CodeDocumentation LoRA checkpoint exceeds the admitted artifact limit.",
                    CheckpointPayloadInvalidErrorCode);
            }

            if (weights.CanSeek)
                weights.Position = 0;

            // WHY: DPO publishes a full checkpoint that includes LoRA adapter
            // tensors. A fresh model must expose the same adapter slots before
            // LoadCheckpoint can validate and import those repository bytes.
            var model = new LlmModel(modelConfig);
            var canonical = LoraApplicator.ValidateAndCanonicalize(loraConfig);
            if (adapterSeedAuthority is not null)
            {
                LoraApplicator.ApplySeeded(
                    model,
                    canonical,
                    adapterSeedAuthority.RootInitializationSeed,
                    adapterSeedAuthority.PolicyDropoutStreamDomain);
            }
            else
            {
                if (canonical.Dropout > 0f && canonical.TargetModules.Length > 0)
                {
                    return OperationResult<LlmModel>.Fail(
                        "Dropout-enabled repository START requires deterministic adapter authority.",
                        CheckpointPayloadInvalidErrorCode);
                }
                LoraApplicator.Apply(model, canonical);
            }
            model.LoadCheckpoint(weights);
            return OperationResult<LlmModel>.Ok(model);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<LlmModel>.Fail(
                "Repository-owned CodeDocumentation checkpoint payload is not a valid full LlmModel checkpoint with the requested LoRA slots.",
                CheckpointPayloadInvalidErrorCode);
        }
    }

    private static OperationResult<string> ResolveTrainingAttemptPolicyCheckpointPath(
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
        RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
        int dpoStepCount)
    {
        try
        {
            var attemptRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(attemptAddress.AttemptRoot));
            var checkpointPath = Path.GetFullPath(Path.Combine(
                attemptRoot,
                DPO.DpoTrainer.BuildPolicyCheckpointFileName(dpoStepCount)));
            var repositoryRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(request.RepositoryRoot));
            if (!StringComparerForPaths.Equals(
                    attemptRoot,
                    attemptAddress.AttemptRoot) ||
                AnyExistingPathSegmentIsReparsePoint(checkpointPath) ||
                !checkpointPath.StartsWith(
                    attemptRoot + Path.DirectorySeparatorChar,
                    PathComparison) ||
                !checkpointPath.StartsWith(
                    repositoryRoot + Path.DirectorySeparatorChar,
                    PathComparison))
            {
                return TrainingAttemptCheckpointInvalid<string>();
            }

            return OperationResult<string>.Ok(checkpointPath);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return TrainingAttemptCheckpointInvalid<string>();
        }
    }

    private static OperationResult<CheckpointByteIdentity>
        HashTrainingAttemptCheckpointFile(
            CodeDocumentationRepositoryOwnedModelCheckpointStartRequest request,
            RepositoryOwnedModelTrainingAttemptAddress attemptAddress,
            string checkpointPath,
            CancellationToken cancellationToken)
    {
        try
        {
            var canonicalPath = Path.GetFullPath(checkpointPath);
            var attemptRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(attemptAddress.AttemptRoot));
            var repositoryRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(request.RepositoryRoot));
            if (!canonicalPath.StartsWith(
                    attemptRoot + Path.DirectorySeparatorChar,
                    PathComparison) ||
                !canonicalPath.StartsWith(
                    repositoryRoot + Path.DirectorySeparatorChar,
                    PathComparison) ||
                AnyExistingPathSegmentIsReparsePoint(canonicalPath) ||
                !File.Exists(canonicalPath) ||
                Directory.Exists(canonicalPath))
            {
                return TrainingAttemptCheckpointInvalid<CheckpointByteIdentity>();
            }

            using var stream = File.OpenRead(canonicalPath);
            if (stream.Length <= 0 ||
                stream.Length > request.MaximumCheckpointBytes)
            {
                return TrainingAttemptCheckpointInvalid<CheckpointByteIdentity>();
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
                {
                    return TrainingAttemptCheckpointInvalid<
                        CheckpointByteIdentity>();
                }

                hash.AppendData(buffer.AsSpan(0, read));
            }

            var repositoryRelativePath = Path.GetRelativePath(
                    repositoryRoot,
                    canonicalPath)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            if (!IsSafeRepositoryRelativePath(repositoryRelativePath))
            {
                return TrainingAttemptCheckpointInvalid<
                    CheckpointByteIdentity>();
            }

            return OperationResult<CheckpointByteIdentity>.Ok(
                new CheckpointByteIdentity(
                    Convert
                        .ToHexString(hash.GetHashAndReset())
                        .ToLowerInvariant(),
                    total,
                    repositoryRelativePath));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return TrainingAttemptCheckpointInvalid<CheckpointByteIdentity>();
        }
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
            {
                return TrainingAttemptCheckpointInvalid<CheckpointByteIdentity>();
            }

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
            return TrainingAttemptCheckpointInvalid<CheckpointByteIdentity>();
        }
    }

    private static OperationResult<LlmModel> CreateColdStartModel(
        ModelConfig modelConfig,
        LoraConfig? loraConfig,
        LoraPairRandomSeedAuthority? adapterSeedAuthority, // Сохраняем независимую authority для LoRA.
        ulong? baseInitializationSeed) // Отсутствие opt-in оставляет прежний global RNG путь.
    {
        try
        {
            // Cold start still validates constructibility of the exact requested
            // configuration; an invalid ModelConfig cannot become a hidden
            // runtime model through the explicit-human-reason path.
            using var baseScope = baseInitializationSeed is { } seed // Scope живёт только внутри owner construction.
                ? TensorCore.Init.Initializers.UseDeterministicSeed(seed) : null; // Null не изменяет legacy random context.
            var model = new LlmModel(modelConfig); // Owner создаёт фактический production LlmModel с объявленной формой.
            if (loraConfig is not null)
            {
                var canonical = LoraApplicator.ValidateAndCanonicalize(loraConfig);
                if (canonical.Dropout > 0f &&
                    canonical.TargetModules.Length > 0 &&
                    adapterSeedAuthority is null)
                {
                    return OperationResult<LlmModel>.Fail(
                        "Dropout-enabled repository cold start requires deterministic adapter authority.",
                        ColdStartNotAuthorizedErrorCode);
                }
                if (adapterSeedAuthority is not null)
                {
                    LoraApplicator.ApplySeeded(
                        model,
                        canonical,
                        adapterSeedAuthority.RootInitializationSeed,
                        adapterSeedAuthority.PolicyDropoutStreamDomain);
                }
                else
                {
                    LoraApplicator.Apply(model, canonical);
                }
            }
            return OperationResult<LlmModel>.Ok(model);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Construction failure is an invalid START request, not a repository
            // miss. Callers must fix the model shape instead of publishing an
            // unusable checkpoint lineage.
            return OperationResult<LlmModel>.Fail(
                "CodeDocumentation cold-start ModelConfig cannot construct an LlmModel.",
                InvalidErrorCode);
        }
    }

    // The Transformer configuration must be internally constructible before it
    // is used either to validate checkpoint shape or to create a cold-start
    // model; head divisibility catches common tensor-shape mismatches early.
    private static bool ModelConfigIsWellFormed(ModelConfig config)
        => config.VocabSize > 0
           && config.ContextLen > 0
           && config.HiddenDim > 0
           && config.NumLayers > 0
           && config.NumHeads > 0
           && config.NumKvHeads > 0
           && config.FfnHiddenDim > 0
           && float.IsFinite(config.RopeTheta)
           && config.RopeTheta > 0f
           && float.IsFinite(config.RmsNormEps)
           && config.RmsNormEps > 0f
           && config.HiddenDim % config.NumHeads == 0
           && config.NumHeads % config.NumKvHeads == 0;

    // RuntimeCompatibility.ConfigSha256 is the bridge between the manifest and
    // the actual ModelConfig object; matching it prevents a caller from pairing
    // correct-looking topology labels with a different tensor shape.
    private static bool RuntimeCompatibilityMatchesModelConfig(
        RepositoryOwnedModelRuntimeCompatibility compatibility,
        ModelConfig config)
        => StringComparer.Ordinal.Equals(
            compatibility.ConfigSha256,
            TrainingSourceProvenance.Identify(config).Sha256);

    // Every compatibility field must be canonical before loader comparison so
    // uppercase hashes, whitespace, or unsafe keys cannot create ambiguous
    // runtime identities.
    private static bool RuntimeCompatibilityIsWellFormed(
        RepositoryOwnedModelRuntimeCompatibility compatibility)
        => IsCanonicalSha256(compatibility.ConfigSha256)
           && IsCanonicalSha256(compatibility.TopologySha256)
           && IsCanonicalSha256(compatibility.TokenizerSha256)
           && IsCanonicalSha256(compatibility.TrainerSha256)
           && IsCanonicalSha256(compatibility.SchemaSha256)
           && IsSafeCompatibilityKey(compatibility.CompatibilityKey);

    // A denied cold-start policy must carry no reason; an authorized policy must
    // carry a bounded human sentence. That separation catches both accidental
    // empty starts and accidental stale reasons on denied requests.
    private static bool ColdStartPolicyIsWellFormed(
        RepositoryOwnedIncrementalTrainingColdStartPolicy policy)
        => policy.IsAuthorized
            ? IsBoundedHumanReason(policy.HumanReason)
            : string.IsNullOrEmpty(policy.HumanReason);

    // A null LoRA config means "only adapter-free checkpoints are expected".
    // A non-null config is the DPO continuation path: the same adapter slots
    // must be recreated before a full checkpoint with `lora.*` tensors can load.
    private static bool LoraConfigIsWellFormed(LoraConfig? config)
    {
        if (config is null)
            return true;

        try
        {
            return LoraApplicator.ValidateAndCanonicalize(config)
                .TargetModules.Length > 0;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    // Only absence of the tree or absence of sealed candidates is "clean
    // missing." Other loader errors mean unsafe persisted state and therefore
    // must not route into cold start.
    private static bool IsCleanMissingRepositoryOwnedModelCheckpoint<T>(
        OperationResult<T> result)
        => string.Equals(
               result.ErrorCode,
               RepositoryOwnedModelCheckpointLoader.NoCheckpointTreeErrorCode,
               StringComparison.Ordinal) ||
           string.Equals(
               result.ErrorCode,
               RepositoryOwnedModelCheckpointLoader.NoSealedCandidateErrorCode,
               StringComparison.Ordinal);

    // Repository manifests use lower-case hexadecimal SHA-256 strings; any
    // other spelling is rejected before it can reach path or payload selection.
    private static bool IsCanonicalSha256(string? value)
        => value is { Length: 64 } &&
           value.All(static character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    // The compatibility key is human-readable but still path-adjacent identity
    // data, so it allows only stable slug characters and exact trim equality.
    private static bool IsSafeCompatibilityKey(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           value.Length <= 256 &&
           value.All(static character =>
               character is >= 'a' and <= 'z'
               || character is >= '0' and <= '9'
               || character is '-' or '_' or '.');

    // Human cold-start reasons are deliberately bounded prose. Control
    // characters or whitespace-only values would make audit logs ambiguous.
    private static bool IsBoundedHumanReason(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           value.Length <= 512 &&
           !value.Any(char.IsControl);

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

    // All validation failures return one stable CodeDocumentation START code so
    // callers cannot infer partial path or payload details from invalid input.
    private static OperationResult<bool> Invalid()
        => OperationResult<bool>.Fail(
            "CodeDocumentation repository-owned LlmModel checkpoint START request is invalid.",
            InvalidErrorCode);

    private static OperationResult<
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>
        TrainingAttemptCheckpointInvalid()
        => TrainingAttemptCheckpointInvalid<
            CodeDocumentationRepositoryOwnedTrainingAttemptCheckpointLoad>();

    private static OperationResult<T> TrainingAttemptCheckpointInvalid<T>()
        => OperationResult<T>.Fail(
            "CodeDocumentation training attempt checkpoint could not be reopened from disk as the expected full model checkpoint or did not match the hash of the reserialized fresh model.",
            TrainingAttemptCheckpointInvalidErrorCode);

    private static StringComparison PathComparison
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static StringComparer StringComparerForPaths { get; }
        = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private sealed record CheckpointByteIdentity(
        string Sha256,
        long ByteLength,
        string RepositoryRelativePath);
}
