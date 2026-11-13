// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using TrainingLoop;
using TransformerModel.Config;

/// <summary>
/// Creates incremental-training sessions exclusively from server-retained
/// object capabilities. The caller owns the file-access lease; this factory
/// owns the RegistryRoot and AdapterRoot capabilities acquired while binding.
/// </summary>
internal sealed class RetainedCheckpointBackedIncrementalTrainingSessionFactory
    : IIncrementalTrainingSessionFactory, IDisposable
{
    internal const string BindFailedErrorCode =
        "RETAINED_TRAINING_SESSION_FACTORY_BIND_FAILED";
    internal const string DisposedErrorCode =
        "RETAINED_TRAINING_SESSION_FACTORY_DISPOSED";

    private const int MaximumColdStartReasonLength = 512;

    private readonly object _gate = new();
    private readonly ServerOwnedContinuousLearningFileAccess _fileAccess;
    private readonly ServerOwnedContinuousLearningRegistryRootCapability
        _registryRoot;
    private readonly ServerOwnedContinuousLearningAdapterRootCapability
        _adapterRoot;
    private readonly DomainModelRegistry _registry;
    private readonly ModelConfig _modelConfig;
    private readonly TrainingConfig _trainingConfig;
    private readonly Action<ContinuousLearningRetainedArtifactRole>?
        _afterRetainedArtifactOpenForTesting;
    private readonly Func<
        RepositoryOwnedIncrementalTrainingSessionStartRequest,
        OperationResult<RepositoryOwnedModelCheckpointLoadResult>>
        _loadRepositoryCheckpoint;
    private readonly Func<
        string,
        OperationResult<RepositoryOwnedIncrementalTrainingSessionStartRequest>>?
        _resolveRepositoryStartRequest;
    private readonly LoraConfig? _repositoryCheckpointAdapterConfig;
    private bool _disposed;

    private RetainedCheckpointBackedIncrementalTrainingSessionFactory(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        ServerOwnedContinuousLearningRegistryRootCapability registryRoot,
        ServerOwnedContinuousLearningAdapterRootCapability adapterRoot,
        DomainModelRegistry registry,
        ModelConfig modelConfig,
        TrainingConfig trainingConfig,
        Action<ContinuousLearningRetainedArtifactRole>?
            afterRetainedArtifactOpenForTesting,
        Func<
            RepositoryOwnedIncrementalTrainingSessionStartRequest,
            OperationResult<RepositoryOwnedModelCheckpointLoadResult>>?
            loadRepositoryCheckpoint,
        Func<
            string,
            OperationResult<RepositoryOwnedIncrementalTrainingSessionStartRequest>>?
            resolveRepositoryStartRequest,
        LoraConfig? repositoryCheckpointAdapterConfig)
    {
        _fileAccess = fileAccess;
        _registryRoot = registryRoot;
        _adapterRoot = adapterRoot;
        _registry = registry;
        _modelConfig = modelConfig;
        _trainingConfig = trainingConfig;
        _afterRetainedArtifactOpenForTesting =
            afterRetainedArtifactOpenForTesting;
        _loadRepositoryCheckpoint =
            loadRepositoryCheckpoint ?? LoadRepositoryCheckpoint;
        _resolveRepositoryStartRequest = resolveRepositoryStartRequest;
        _repositoryCheckpointAdapterConfig =
            repositoryCheckpointAdapterConfig is null
                ? null
                : LoraApplicator.ValidateAndCanonicalize(
                    repositoryCheckpointAdapterConfig);
    }

    internal static OperationResult<
        RetainedCheckpointBackedIncrementalTrainingSessionFactory> Bind(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        ISessionStorage storage,
        ModelConfig modelConfig,
        TrainingConfig trainingConfig,
        ServerOwnedContinuousLearningRegistryRootCapabilityOptions?
            registryRootOptions = null,
        ServerOwnedContinuousLearningAdapterRootCapabilityOptions?
            adapterRootOptions = null,
        Action<ContinuousLearningRetainedArtifactRole>?
            afterRetainedArtifactOpenForTesting = null,
        Func<
            RepositoryOwnedIncrementalTrainingSessionStartRequest,
            OperationResult<RepositoryOwnedModelCheckpointLoadResult>>?
            loadRepositoryCheckpoint = null,
        Func<
            string,
            OperationResult<RepositoryOwnedIncrementalTrainingSessionStartRequest>>?
            resolveRepositoryStartRequest = null,
        LoraConfig? repositoryCheckpointAdapterConfig = null)
    {
        ArgumentNullException.ThrowIfNull(fileAccess);
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(modelConfig);
        ArgumentNullException.ThrowIfNull(trainingConfig);

        ServerOwnedContinuousLearningRegistryRootCapability? registryRoot = null;
        ServerOwnedContinuousLearningAdapterRootCapability? adapterRoot = null;
        try
        {
            fileAccess.EnsureRetained();
            registryRoot = fileAccess.AcquireRegistryRootCapability(
                registryRootOptions);
            fileAccess.EnsureRetained();
            adapterRoot = fileAccess.AcquireAdapterRootCapability(
                adapterRootOptions);
            fileAccess.EnsureRetained();

            var registry = new DomainModelRegistry(
                storage,
                registryRoot,
                adapterRoot);
            fileAccess.EnsureRetained();

            var factory =
                new RetainedCheckpointBackedIncrementalTrainingSessionFactory(
                    fileAccess,
                    registryRoot,
                    adapterRoot,
                    registry,
                    modelConfig,
                    trainingConfig,
                    afterRetainedArtifactOpenForTesting,
                    loadRepositoryCheckpoint,
                    resolveRepositoryStartRequest,
                    repositoryCheckpointAdapterConfig);
            registryRoot = null;
            adapterRoot = null;
            return OperationResult<
                RetainedCheckpointBackedIncrementalTrainingSessionFactory>.Ok(
                    factory);
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return OperationResult<
                RetainedCheckpointBackedIncrementalTrainingSessionFactory>.Fail(
                    "Retained continuous-learning identity verification failed.",
                    ContinuousLearningPathIdentityException.StableErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            try
            {
                fileAccess.EnsureRetained();
            }
            catch (ContinuousLearningPathIdentityException)
            {
                return OperationResult<
                    RetainedCheckpointBackedIncrementalTrainingSessionFactory>.Fail(
                        "Retained continuous-learning identity verification failed.",
                        ContinuousLearningPathIdentityException.StableErrorCode);
            }

            return OperationResult<
                RetainedCheckpointBackedIncrementalTrainingSessionFactory>.Fail(
                    "The retained incremental-training factory could not be bound.",
                    BindFailedErrorCode);
        }
        finally
        {
            DisposeAcquiredCapabilities(adapterRoot, registryRoot);
        }
    }

    public OperationResult<IncrementalTrainingSession> Create(string domain)
        => Create(domain, CancellationToken.None);

    public OperationResult<IncrementalTrainingSession> Create(
        string domain,
        CancellationToken cancellationToken)
        => _resolveRepositoryStartRequest is null
            ? CreateCore(domain, repositoryStartRequest: null, cancellationToken)
            : CreateResolvedRepositoryStart(domain, cancellationToken);

    public OperationResult<IncrementalTrainingSession> Create(
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest)
        => Create(startRequest, CancellationToken.None);

    public OperationResult<IncrementalTrainingSession> Create(
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest,
        CancellationToken cancellationToken)
    {
        var validation = ValidateRepositoryStartRequest(startRequest);
        if (!validation.IsSuccess)
            return validation.MapError<IncrementalTrainingSession>();

        return CreateCore(
            startRequest.DomainSlug,
            startRequest,
            cancellationToken);
    }

    private OperationResult<IncrementalTrainingSession>
        CreateResolvedRepositoryStart(
            string domain,
            CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_disposed)
                return DisposedFailure();

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                _fileAccess.EnsureRetained();
                var resolved = _resolveRepositoryStartRequest!(domain);
                if (!resolved.IsSuccess)
                {
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        resolved.ErrorMessage,
                        resolved.ErrorCode);
                }

                var request = resolved.Value;
                var validation = ValidateRepositoryStartRequest(request);
                if (!validation.IsSuccess)
                    return validation.MapError<IncrementalTrainingSession>();

                if (!StringComparer.Ordinal.Equals(request!.DomainSlug, domain))
                {
                    // WHY: a configured repository resolver makes the
                    // repository-owned start path authoritative for ordinary
                    // domain creation. A missing or mismatched mapping must
                    // fail closed instead of silently falling back to the
                    // retained-base legacy route, because that would train from
                    // an unconfigured parent and hide composition bugs.
                    return InvalidRepositoryStartRequest()
                        .MapError<IncrementalTrainingSession>();
                }

                return CreateCore(domain, request, cancellationToken);
            }
            catch (ContinuousLearningPathIdentityException)
            {
                return OperationResult<IncrementalTrainingSession>.Fail(
                    "Retained continuous-learning identity verification failed.",
                    ContinuousLearningPathIdentityException.StableErrorCode);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<IncrementalTrainingSession>.Fail(
                    "The incremental training session could not be created.",
                    "TRAINING_SESSION_CREATION_FAILED");
            }
        }
    }

    private OperationResult<IncrementalTrainingSession> CreateCore(
        string domain,
        RepositoryOwnedIncrementalTrainingSessionStartRequest? repositoryStartRequest,
        CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (_disposed)
                return DisposedFailure();

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                _fileAccess.EnsureRetained();
                var adapterResult = _registry.ResolveRetainedAdapter(
                    domain,
                    cancellationToken);
                if (!adapterResult.IsSuccess)
                {
                    _fileAccess.EnsureRetained();
                    cancellationToken.ThrowIfCancellationRequested();
                    return OperationResult<IncrementalTrainingSession>.Fail(
                        adapterResult.ErrorMessage,
                        adapterResult.ErrorCode);
                }

                _fileAccess.EnsureRetained();
                IncrementalTrainingParentAdapterCheckpoint?
                    parentAdapterCheckpoint = null;
                if (adapterResult.Value is { } retainedAdapter)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        using var adapterStream = new MemoryStream(
                            retainedAdapter.Snapshot.CopyBytes(),
                            writable: false);
                        parentAdapterCheckpoint =
                            new IncrementalTrainingParentAdapterCheckpoint(
                                adapterStream,
                                retainedAdapter.Provenance);
                    }
                    catch (IncrementalTrainingCheckpointHashMismatchException)
                    {
                        return OperationResult<IncrementalTrainingSession>.Fail(
                            "The parent adapter bytes do not match registered provenance.",
                            "ADAPTER_PROVENANCE_MISMATCH");
                    }
                    catch (Exception exception)
                        when (SnapshotExceptionGraphClassifier.IsNonFatal(
                            exception))
                    {
                        return OperationResult<IncrementalTrainingSession>.Fail(
                            "Adapter checkpoint is invalid.",
                            "ADAPTER_CHECKPOINT_INVALID");
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    _fileAccess.EnsureRetained();
                }

                RepositoryOwnedModelCheckpointLoadResult?
                    repositoryCheckpoint = null;
                Func<Stream> openBaseCheckpoint;
                IncrementalTrainingCheckpointStartMetadata? checkpointStart = null;
                if (repositoryStartRequest is null)
                {
                    openBaseCheckpoint = () => OpenRetainedArtifact(
                        ContinuousLearningRetainedArtifactRole.BaseCheckpoint,
                        cancellationToken);
                }
                else
                {
                    var loaded = _loadRepositoryCheckpoint(repositoryStartRequest);
                    if (loaded.IsSuccess)
                    {
                        if (loaded.Value is null)
                        {
                            return OperationResult<IncrementalTrainingSession>.Fail(
                                "Repository-owned model checkpoint loader returned no checkpoint result.",
                                RepositoryOwnedModelCheckpointLoader
                                    .InvalidRequestErrorCode);
                        }

                        repositoryCheckpoint = loaded.Value;
                        checkpointStart =
                            IncrementalTrainingCheckpointStartMetadata
                                .FromRepository(
                                    repositoryCheckpoint.Manifest,
                                    repositoryStartRequest);
                        var consumed = false;
                        openBaseCheckpoint = () =>
                        {
                            if (consumed)
                                throw new InvalidOperationException(
                                    "Repository checkpoint stream was already consumed.");

                            consumed = true;
                            return repositoryCheckpoint.Weights;
                        };
                    }
                    else if (RepositoryCheckpointLoadAllowsColdStart(
                                 loaded.ErrorCode,
                                 repositoryStartRequest.ColdStartPolicy))
                    {
                        var reason =
                            repositoryStartRequest.ColdStartPolicy.HumanReason!;
                        // WHY: the first repository-owned model must become
                        // step 000001. If cold-start metadata forgets the
                        // exact typed start request, the publisher cannot
                        // write that first full checkpoint and the next
                        // process will start from zero again.
                        checkpointStart =
                            IncrementalTrainingCheckpointStartMetadata
                                .FromColdStart(
                                    reason,
                                    repositoryStartRequest);
                        openBaseCheckpoint = () => OpenRetainedArtifact(
                            ContinuousLearningRetainedArtifactRole.BaseCheckpoint,
                            cancellationToken);
                    }
                    else
                    {
                        return OperationResult<IncrementalTrainingSession>.Fail(
                            loaded.ErrorMessage,
                            loaded.ErrorCode);
                    }
                }

                OperationResult<IncrementalTrainingSession> session;
                try
                {
                    session = IncrementalTrainingSessionBuilder.Build(
                        _modelConfig,
                        _trainingConfig,
                        openBaseCheckpoint,
                        () => OpenRetainedArtifact(
                            ContinuousLearningRetainedArtifactRole.Tokenizer,
                            cancellationToken),
                        parentAdapterCheckpoint,
                        cancellationToken,
                        checkpointStart,
                        repositoryStartRequest is null
                            ? null
                            : _repositoryCheckpointAdapterConfig);
                }
                finally
                {
                    repositoryCheckpoint?.Dispose();
                }

                _fileAccess.EnsureRetained();
                cancellationToken.ThrowIfCancellationRequested();
                return session;
            }
            catch (ContinuousLearningPathIdentityException)
            {
                return OperationResult<IncrementalTrainingSession>.Fail(
                    "Retained continuous-learning identity verification failed.",
                    ContinuousLearningPathIdentityException.StableErrorCode);
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return OperationResult<IncrementalTrainingSession>.Fail(
                    "The incremental training session could not be created.",
                    "TRAINING_SESSION_CREATION_FAILED");
            }
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            try
            {
                _adapterRoot.Dispose();
            }
            finally
            {
                _registryRoot.Dispose();
            }
        }
    }

    private Stream OpenRetainedArtifact(
        ContinuousLearningRetainedArtifactRole role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _fileAccess.EnsureRetained();
        var stream = _fileAccess.OpenRetainedArtifactForRead(role);
        try
        {
            _afterRetainedArtifactOpenForTesting?.Invoke(role);
            _fileAccess.EnsureRetained();
            cancellationToken.ThrowIfCancellationRequested();
            return stream;
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    private static void DisposeAcquiredCapabilities(
        ServerOwnedContinuousLearningAdapterRootCapability? adapterRoot,
        ServerOwnedContinuousLearningRegistryRootCapability? registryRoot)
    {
        try
        {
            adapterRoot?.Dispose();
        }
        finally
        {
            registryRoot?.Dispose();
        }
    }

    private static OperationResult<IncrementalTrainingSession> DisposedFailure()
        => OperationResult<IncrementalTrainingSession>.Fail(
            "The retained incremental-training factory has been disposed.",
            DisposedErrorCode);

    private static OperationResult<bool> ValidateRepositoryStartRequest(
        RepositoryOwnedIncrementalTrainingSessionStartRequest? request)
    {
        if (request is null
            || request.RuntimeCompatibility is null
            || request.ColdStartPolicy is null)
        {
            return InvalidRepositoryStartRequest();
        }

        if (!request.ColdStartPolicy.IsAuthorized)
        {
            return string.IsNullOrEmpty(request.ColdStartPolicy.HumanReason)
                ? OperationResult<bool>.Ok(true)
                : InvalidRepositoryStartRequest();
        }

        return IsBoundedHumanReason(request.ColdStartPolicy.HumanReason)
            ? OperationResult<bool>.Ok(true)
            : InvalidRepositoryStartRequest();
    }

    private static bool RepositoryCheckpointLoadAllowsColdStart(
        string? errorCode,
        RepositoryOwnedIncrementalTrainingColdStartPolicy coldStartPolicy)
        // WHY: only "no parent exists yet" may intentionally seed from the
        // retained base. Incompatible, corrupt, ambiguous, unavailable, or
        // invalid repository state means the persisted lineage is unsafe, so
        // falling back would silently train from the wrong ancestor.
        => coldStartPolicy.IsAuthorized
           && (StringComparer.Ordinal.Equals(
                   errorCode,
                   RepositoryOwnedModelCheckpointLoader.NoCheckpointTreeErrorCode)
               || StringComparer.Ordinal.Equals(
                   errorCode,
                   RepositoryOwnedModelCheckpointLoader
                       .NoSealedCandidateErrorCode));

    private static OperationResult<RepositoryOwnedModelCheckpointLoadResult>
        LoadRepositoryCheckpoint(
            RepositoryOwnedIncrementalTrainingSessionStartRequest request)
        // WHY: the retained base checkpoint remains the legacy boot artifact,
        // but an explicit repository-owned start request makes the sealed
        // model tree the authority for continuation before any cold-start path
        // can be considered.
        => new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(
            request.RepositoryRoot,
            request.DomainSlug,
            request.DesignPatternSlug,
            request.ModelIdSlug,
            request.ModelVersion,
            request.RuntimeCompatibility);

    private static bool IsBoundedHumanReason(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && value.Length <= MaximumColdStartReasonLength
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && !value.Any(char.IsControl);

    private static OperationResult<bool> InvalidRepositoryStartRequest()
        => OperationResult<bool>.Fail(
            "Repository-owned incremental-training start request is invalid.",
            RepositoryOwnedModelCheckpointLoader.InvalidRequestErrorCode);
}
