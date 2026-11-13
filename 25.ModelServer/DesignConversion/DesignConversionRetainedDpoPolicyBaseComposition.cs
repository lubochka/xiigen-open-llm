// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Security.Cryptography;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using TransformerModel.Config;

/// <summary>
/// Opens fresh caller-owned DesignConversion policy checkpoint snapshots from
/// the server-retained base-checkpoint object without exposing its path.
/// </summary>
internal sealed class ContinuousLearningRetainedDpoPolicyCheckpointSource
    : IDesignConversionDpoPolicyCheckpointSource
{
    internal const string BindFailedErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_RETAINED_SOURCE_BIND_FAILED";
    internal const string CancelledErrorCode = "CANCELLED";
    internal const string BindFailedErrorMessage =
        "The retained DesignConversion DPO policy base source could not be bound.";
    internal const string PathIdentityErrorMessage =
        "Retained continuous-learning path identity verification failed.";
    internal const string CancelledErrorMessage =
        "DesignConversion retained DPO policy base binding was cancelled.";

    private readonly ServerOwnedContinuousLearningFileAccess _fileAccess;
    private readonly Action<Stream>?
        _afterOpenBeforeRevalidationForTesting;

    internal ContinuousLearningRetainedDpoPolicyCheckpointSource(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        Action<Stream>? afterOpenBeforeRevalidationForTesting = null)
    {
        ArgumentNullException.ThrowIfNull(fileAccess);
        _fileAccess = fileAccess;
        _afterOpenBeforeRevalidationForTesting =
            afterOpenBeforeRevalidationForTesting;
    }

    public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(Cancelled());

        Stream? openedStream = null;
        try
        {
            _fileAccess.EnsureRetained();
            openedStream = _fileAccess.OpenRetainedArtifactForRead(
                ContinuousLearningRetainedArtifactRole.BaseCheckpoint);
            _afterOpenBeforeRevalidationForTesting?.Invoke(openedStream);
            _fileAccess.EnsureRetained();

            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Cancelled());

            openedStream.Position = 0;
            var successfulTransfer = Task.FromResult(
                OperationResult<Stream>.Ok(openedStream));
            openedStream = null;
            return successfulTransfer;
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return Task.FromResult(cancellationToken.IsCancellationRequested
                ? Cancelled()
                : PathIdentityFailed());
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Cancelled());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Task.FromResult(cancellationToken.IsCancellationRequested
                ? Cancelled()
                : BindFailed());
        }
        finally
        {
            DisposeAfterFailure(openedStream);
        }
    }

    private static void DisposeAfterFailure(Stream? stream)
    {
        if (stream is null)
            return;

        try
        {
            stream.Dispose();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
        }
    }

    private static OperationResult<Stream> Cancelled()
        => OperationResult<Stream>.Fail(
            CancelledErrorMessage,
            CancelledErrorCode);

    private static OperationResult<Stream> PathIdentityFailed()
        => OperationResult<Stream>.Fail(
            PathIdentityErrorMessage,
            ContinuousLearningPathIdentityException.StableErrorCode);

    private static OperationResult<Stream> BindFailed()
        => OperationResult<Stream>.Fail(
            BindFailedErrorMessage,
            BindFailedErrorCode);
}

/// <summary>
/// Opens fresh caller-owned canonical BPE tokenizer snapshots from the same
/// server-retained object set as the DesignConversion policy checkpoint.
/// </summary>
internal sealed class ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource
    : IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource
{
    internal const string BindFailedErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_RETAINED_TOKENIZER_SOURCE_BIND_FAILED";
    internal const string BindFailedErrorMessage =
        "The retained DesignConversion DPO policy base tokenizer source could not be bound.";

    private readonly ServerOwnedContinuousLearningFileAccess _fileAccess;
    private readonly Action<Stream>?
        _afterOpenBeforeRevalidationForTesting;

    internal ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource(
        ServerOwnedContinuousLearningFileAccess fileAccess,
        Action<Stream>? afterOpenBeforeRevalidationForTesting = null)
    {
        ArgumentNullException.ThrowIfNull(fileAccess);
        _fileAccess = fileAccess;
        _afterOpenBeforeRevalidationForTesting =
            afterOpenBeforeRevalidationForTesting;
    }

    public Task<OperationResult<Stream>> OpenCanonicalBpeTokenizerSnapshotAsync(
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(Cancelled());

        Stream? openedStream = null;
        try
        {
            _fileAccess.EnsureRetained();
            openedStream = _fileAccess.OpenRetainedArtifactForRead(
                ContinuousLearningRetainedArtifactRole.Tokenizer);
            _afterOpenBeforeRevalidationForTesting?.Invoke(openedStream);
            _fileAccess.EnsureRetained();

            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Cancelled());

            openedStream.Position = 0;
            var successfulTransfer = Task.FromResult(
                OperationResult<Stream>.Ok(openedStream));
            openedStream = null;
            return successfulTransfer;
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return Task.FromResult(cancellationToken.IsCancellationRequested
                ? Cancelled()
                : PathIdentityFailed());
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Cancelled());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Task.FromResult(cancellationToken.IsCancellationRequested
                ? Cancelled()
                : BindFailed());
        }
        finally
        {
            DisposeAfterFailure(openedStream);
        }
    }

    private static void DisposeAfterFailure(Stream? stream)
    {
        if (stream is null)
            return;

        try
        {
            stream.Dispose();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
        }
    }

    private static OperationResult<Stream> Cancelled()
        => OperationResult<Stream>.Fail(
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .CancelledErrorMessage,
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .CancelledErrorCode);

    private static OperationResult<Stream> PathIdentityFailed()
        => OperationResult<Stream>.Fail(
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .PathIdentityErrorMessage,
            ContinuousLearningPathIdentityException.StableErrorCode);

    private static OperationResult<Stream> BindFailed()
        => OperationResult<Stream>.Fail(
            BindFailedErrorMessage,
            BindFailedErrorCode);
}

/// <summary>
/// Fail-closed provider reference injected into the artifact database before
/// the host starts. It delegates only after hosted retained-object activation
/// binds the exact checkpoint-backed provider and revalidates trust around
/// every later base or continuation request.
/// </summary>
internal sealed class DesignConversionRetainedDpoPolicyBaseProviderGateway
    : IDesignConversionDpoPolicyContinuationProvider
{
    private static readonly Task<bool> StartupWithdrawalAlreadyBegunTask =
        Task.FromResult(true);
    private static readonly AsyncLocal<Func<Task>?>
        InvocationLeaseAcquiredObserver = new();
    internal static Func<Task>? InvocationLeaseAcquiredObserverForTesting
    {
        get => InvocationLeaseAcquiredObserver.Value;
        set => InvocationLeaseAcquiredObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        ReadinessWithdrawnBeforeBindingRemovedObserver = new();
    internal static Action?
        ReadinessWithdrawnBeforeBindingRemovedObserverForTesting
    {
        get => ReadinessWithdrawnBeforeBindingRemovedObserver.Value;
        set => ReadinessWithdrawnBeforeBindingRemovedObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforeStartupReadinessWithdrawalObserver = new();
    internal static Action?
        BeforeStartupReadinessWithdrawalObserverForTesting
    {
        get => BeforeStartupReadinessWithdrawalObserver.Value;
        set => BeforeStartupReadinessWithdrawalObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforePendingRetirementPublicationObserver = new();
    internal static Action?
        BeforePendingRetirementPublicationObserverForTesting
    {
        get => BeforePendingRetirementPublicationObserver.Value;
        set => BeforePendingRetirementPublicationObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        FreshBindingReservedBeforeStartupCaptureObserver = new();
    internal static Action?
        FreshBindingReservedBeforeStartupCaptureObserverForTesting
    {
        get => FreshBindingReservedBeforeStartupCaptureObserver.Value;
        set => FreshBindingReservedBeforeStartupCaptureObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforeInvocationDrainAllocationObserver = new();
    internal static Action?
        BeforeInvocationDrainAllocationObserverForTesting
    {
        get => BeforeInvocationDrainAllocationObserver.Value;
        set => BeforeInvocationDrainAllocationObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforeRetirementWorkAllocationObserver = new();
    internal static Action?
        BeforeRetirementWorkAllocationObserverForTesting
    {
        get => BeforeRetirementWorkAllocationObserver.Value;
        set => BeforeRetirementWorkAllocationObserver.Value = value;
    }
    private static readonly AsyncLocal<Action?>
        BeforeInvocationLeaseAllocationObserver = new();
    internal static Action?
        BeforeInvocationLeaseAllocationObserverForTesting
    {
        get => BeforeInvocationLeaseAllocationObserver.Value;
        set => BeforeInvocationLeaseAllocationObserver.Value = value;
    }
    internal const string ProviderUnavailableErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_RETAINED_PROVIDER_UNAVAILABLE";
    internal const string ProviderInvocationFailedErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_RETAINED_PROVIDER_INVOCATION_FAILED";
    internal const string ProviderUnavailableErrorMessage =
        "The retained DesignConversion DPO policy base provider is unavailable.";
    internal const string ProviderInvocationFailedErrorMessage =
        "The retained DesignConversion DPO policy base provider invocation failed.";

    private readonly object _bindAttemptGate = new();
    private readonly object _bindingGate = new();
    private readonly object _revocationGate = new();
    private BoundProvider? _binding;
    private BoundProvider? _retiringBinding;
    private long _generation;
    private bool _closed;
    private bool _revocationInProgress;
    private Task _retirementTask = Task.CompletedTask;
    private Task<bool> _startupWithdrawalBegunTask =
        StartupWithdrawalAlreadyBegunTask;
    private TaskCompletionSource<object?>? _pendingRetirementCompletion;
    private TaskCompletionSource<bool>?
        _pendingStartupWithdrawalBegunCompletion;
    private RetirementWork? _pendingRetirementWork;
    private RetirementWork? _preparedRetirementWork;

    internal Task AwaitRetirementAsync()
    {
        lock (_bindingGate)
            return _retirementTask;
    }

    internal Task<bool> AwaitStartupWithdrawalBegunAsync()
    {
        lock (_bindingGate)
            return _binding is null || _pendingRetirementWork is null
                ? StartupWithdrawalAlreadyBegunTask
                : _startupWithdrawalBegunTask;
    }

    internal bool IsBound
    {
        get
        {
            lock (_bindingGate)
                return _binding is not null;
        }
    }

    internal OperationResult<ContinuousLearningDpoPolicyBaseBindingToken>
        BindExactProvider(
        DesignConversionCheckpointBackedDpoPolicyBaseProvider provider,
        ContinuousLearningModelServerStartup startup,
        ServerOwnedContinuousLearningFileAccess fileAccess,
        CancellationToken cancellationToken,
        Action? afterValidationBeforeCommitForTesting = null)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(startup);
        ArgumentNullException.ThrowIfNull(fileAccess);
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<ContinuousLearningDpoPolicyBaseBindingToken>();

        lock (_bindAttemptGate)
        {
            return BindExactProviderSerialized(
                provider,
                startup,
                fileAccess,
                cancellationToken,
                afterValidationBeforeCommitForTesting);
        }
    }

    private OperationResult<ContinuousLearningDpoPolicyBaseBindingToken>
        BindExactProviderSerialized(
        DesignConversionCheckpointBackedDpoPolicyBaseProvider provider,
        ContinuousLearningModelServerStartup startup,
        ServerOwnedContinuousLearningFileAccess fileAccess,
        CancellationToken cancellationToken,
        Action? afterValidationBeforeCommitForTesting)
    {
        long capturedGeneration;
        BoundProvider? capturedBinding;
        lock (_bindingGate)
        {
            if (_closed || _revocationInProgress)
                return ProviderUnavailable<
                    ContinuousLearningDpoPolicyBaseBindingToken>();
            capturedBinding = _binding;
            if (capturedBinding is null)
            {
                capturedGeneration = unchecked(_generation + 1);
                _generation = capturedGeneration;
            }
            else
            {
                capturedGeneration = _generation;
            }
        }

        using var cancellationRegistration = cancellationToken.Register(
            static state => ((BindAttemptCancellation)state!).Cancel(),
            new BindAttemptCancellation(
                this,
                capturedGeneration,
                capturedBinding));

        ContinuousLearningDpoPolicyBaseBindingToken bindingToken;
        try
        {
            startup.EnsureDpoPolicyBaseBindingRetained(fileAccess);
            if (capturedBinding is null)
            {
                FreshBindingReservedBeforeStartupCaptureObserver.Value
                    ?.Invoke();
            }
            bindingToken = capturedBinding?.BindingToken
                ?? startup.CaptureFreshGatewayDpoPolicyBaseBindingToken(
                    fileAccess);
            if (capturedBinding is null
                && !bindingToken.TryRegisterTrustLossRevocation(
                    RevokeExactBindingAfterTrustLoss))
            {
                return ProviderUnavailable<
                    ContinuousLearningDpoPolicyBaseBindingToken>();
            }
            afterValidationBeforeCommitForTesting?.Invoke();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<ContinuousLearningDpoPolicyBaseBindingToken>();
        }
        catch (ContinuousLearningPathIdentityException)
        {
            Close();
            return RetainedIdentityFailed<
                ContinuousLearningDpoPolicyBaseBindingToken>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ProviderUnavailable<
                ContinuousLearningDpoPolicyBaseBindingToken>();
        }

        BoundProvider? preparedBinding = null;
        OperationResult<ContinuousLearningDpoPolicyBaseBindingToken>?
            preparedSuccess = null;
        if (capturedBinding is null)
        {
            var committedGeneration = unchecked(capturedGeneration + 1);
            preparedBinding = new BoundProvider(
                provider,
                startup,
                fileAccess,
                bindingToken,
                committedGeneration);
            preparedSuccess = OperationResult<
                ContinuousLearningDpoPolicyBaseBindingToken>.Ok(bindingToken);
        }

        lock (_bindingGate)
        {
            if (_closed || _revocationInProgress)
                return ProviderUnavailable<
                    ContinuousLearningDpoPolicyBaseBindingToken>();
            if (cancellationToken.IsCancellationRequested)
                return Cancelled<
                    ContinuousLearningDpoPolicyBaseBindingToken>();
            if (_generation != capturedGeneration)
                return ProviderUnavailable<
                    ContinuousLearningDpoPolicyBaseBindingToken>();

            if (_binding is null)
            {
                if (preparedBinding is null
                    || preparedSuccess is null
                    || !preparedBinding.Startup.TryRegisterRetirementDependency(
                        () => PrepareInvocationWithdrawalFromStartup(
                            preparedBinding))
                    || !bindingToken.TryMarkGatewayCommitted(this))
                {
                    return ProviderUnavailable<
                        ContinuousLearningDpoPolicyBaseBindingToken>();
                }

                _generation = preparedBinding.Generation;
                _binding = preparedBinding;
                return preparedSuccess;
            }

            if (ReferenceEquals(_binding, capturedBinding)
                && ReferenceEquals(_binding.Provider, provider)
                && ReferenceEquals(_binding.Startup, startup)
                && ReferenceEquals(_binding.FileAccess, fileAccess))
            {
                return OperationResult<
                    ContinuousLearningDpoPolicyBaseBindingToken>.Ok(
                        _binding.BindingToken);
            }
            return ProviderUnavailable<
                ContinuousLearningDpoPolicyBaseBindingToken>();
        }
    }

    internal void Unbind()
        => WithdrawReadinessThenRevokeCurrent(close: false);

    internal void Close()
        => WithdrawReadinessThenRevokeCurrent(close: true);

    internal void RevokeExactBinding(
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
    {
        ArgumentNullException.ThrowIfNull(bindingToken);
        BoundProvider? binding;
        lock (_bindingGate)
            binding = _binding;
        if (binding is not null
            && ReferenceEquals(binding.BindingToken, bindingToken))
        {
            WithdrawReadinessThenRevokeMatching(binding, close: false);
        }
    }

    internal void CloseExactBinding(
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
    {
        ArgumentNullException.ThrowIfNull(bindingToken);
        BoundProvider? binding;
        lock (_bindingGate)
            binding = FindCurrentOrRetiringExactBindingLocked(bindingToken);
        if (binding is not null
            && ReferenceEquals(binding.BindingToken, bindingToken))
        {
            WithdrawReadinessThenRevokeMatching(binding, close: true);
        }
    }

    private void RevokeExactBindingAfterTrustLoss(
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
    {
        BoundProvider? binding;
        lock (_bindingGate)
            binding = FindCurrentOrRetiringExactBindingLocked(bindingToken);
        if (binding is not null
            && ReferenceEquals(binding.BindingToken, bindingToken))
        {
            RevokeMatchingAfterReadinessWithdrawal(binding, close: true);
        }
    }

    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateModelSetAsync(CancellationToken cancellationToken)
        => InvokeBoundProviderAsync(
            static (provider, token) => provider.CreateModelSetAsync(token),
            cancellationToken,
            afterInvocationBeforeCommitForTesting: null);

    public OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>
        DescribePreparation()
    {
        BoundProvider binding;
        IDisposable invocationLease;
        lock (_bindingGate)
        {
            if (_closed || _revocationInProgress || _binding is null)
            {
                return ProviderUnavailable<
                    DesignConversionDpoPolicyBasePreparationDescriptor>();
            }
            binding = _binding;
            if (!binding.TryAcquireInvocation(out invocationLease))
            {
                return ProviderUnavailable<
                    DesignConversionDpoPolicyBasePreparationDescriptor>();
            }
        }

        using (invocationLease)
        try
        {
            if (!binding.Startup.IsDpoPolicyBaseProviderBound(
                    binding.FileAccess,
                    binding.BindingToken))
            {
                return ProviderUnavailable<
                    DesignConversionDpoPolicyBasePreparationDescriptor>();
            }

            var result = binding.Provider.DescribePreparation();

            if (!binding.Startup.IsDpoPolicyBaseProviderBound(
                    binding.FileAccess,
                    binding.BindingToken))
            {
                return ProviderUnavailable<
                    DesignConversionDpoPolicyBasePreparationDescriptor>();
            }
            lock (_bindingGate)
            {
                if (_closed ||
                    _revocationInProgress ||
                    _generation != binding.Generation ||
                    !ReferenceEquals(_binding, binding) ||
                    !binding.Startup.IsDpoPolicyBaseProviderBindingPublished(
                        binding.BindingToken))
                {
                    return ProviderUnavailable<
                        DesignConversionDpoPolicyBasePreparationDescriptor>();
                }
            }

            if (result is not { IsSuccess: true, Value: not null })
            {
                return ProviderUnavailable<
                    DesignConversionDpoPolicyBasePreparationDescriptor>();
            }
            return OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>.Ok(
                new DesignConversionDpoPolicyBasePreparationDescriptor(
                    result.Value.ToIdentity(),
                    result.Value.ToConfig(),
                    binding.ProviderBindingRef,
                    binding.Generation));
        }
        catch (ContinuousLearningPathIdentityException)
        {
            WithdrawReadinessThenRevokeMatching(binding, close: true);
            return RetainedIdentityFailed<
                DesignConversionDpoPolicyBasePreparationDescriptor>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            WithdrawReadinessThenRevokeMatching(binding, close: false);
            return ProviderInvocationFailed<
                DesignConversionDpoPolicyBasePreparationDescriptor>();
        }
    }

    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateModelSetAsync(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            CancellationToken cancellationToken)
        => InvokeBoundProviderAsync(
            (provider, token) => provider.CreateModelSetAsync(
                StripGatewayBinding(preparation),
                token),
            cancellationToken,
            afterInvocationBeforeCommitForTesting: null,
            requiredPreparation: preparation);

    internal Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateModelSetAsyncForTesting(
            Action afterInvocationBeforeCommitForTesting,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(afterInvocationBeforeCommitForTesting);
        return InvokeBoundProviderAsync(
            static (provider, token) => provider.CreateModelSetAsync(token),
            cancellationToken,
            afterInvocationBeforeCommitForTesting);
    }

    internal Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateNullModelSetResultAsyncForTesting(
            CancellationToken cancellationToken)
        => InvokeBoundProviderAsync(
            static (_, _) => Task.FromResult<
                OperationResult<DesignConversionDpoPolicyBaseModelSet>>(null!),
            cancellationToken,
            afterInvocationBeforeCommitForTesting: null);

    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuedModelSetAsync(
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken)
        => InvokeBoundProviderAsync(
            (provider, token) => provider.CreateContinuedModelSetAsync(
                continuationRequest,
                token),
            cancellationToken,
            afterInvocationBeforeCommitForTesting: null);

    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuedModelSetAsync(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation,
            DesignConversionDpoPolicyContinuationRequest continuationRequest,
            CancellationToken cancellationToken)
        => InvokeBoundProviderAsync(
            (provider, token) => provider.CreateContinuedModelSetAsync(
                StripGatewayBinding(preparation),
                continuationRequest,
                token),
            cancellationToken,
            afterInvocationBeforeCommitForTesting: null,
            requiredPreparation: preparation);

    // Сверочный набор для доказательства неизменности продолжения шлюз не
    // строит сам: он передаёт запрос связанному доверенному поставщику тем же
    // способом, каким передаёт продолжение, и сохраняет отказ при отсутствии
    // связывания.
    public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        CreateContinuationVerificationModelSetAsync(
            ModelConfig config,
            DesignConversionDpoPolicyRandomContext randomContext,
            CancellationToken cancellationToken)
        => InvokeBoundProviderAsync(
            (provider, token) =>
                provider.CreateContinuationVerificationModelSetAsync(
                    config,
                    randomContext,
                    token),
            cancellationToken,
            afterInvocationBeforeCommitForTesting: null);

    private async Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
        InvokeBoundProviderAsync(
            Func<
                DesignConversionCheckpointBackedDpoPolicyBaseProvider,
                CancellationToken,
                Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>>
                invoke,
            CancellationToken cancellationToken,
            Action? afterInvocationBeforeCommitForTesting,
            DesignConversionDpoPolicyBasePreparationDescriptor?
                requiredPreparation = null)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionDpoPolicyBaseModelSet>();

        BoundProvider binding;
        IDisposable invocationLease;
        lock (_bindingGate)
        {
            if (_closed || _revocationInProgress || _binding is null)
                return ProviderUnavailable<DesignConversionDpoPolicyBaseModelSet>();
            binding = _binding;
            if (requiredPreparation is not null &&
                !GatewayDescriptorMatchesBinding(requiredPreparation, binding))
            {
                return ProviderUnavailable<DesignConversionDpoPolicyBaseModelSet>();
            }
            if (!binding.TryAcquireInvocation(out invocationLease))
                return ProviderUnavailable<DesignConversionDpoPolicyBaseModelSet>();
        }

        using (invocationLease)
        try
        {
            var invocationObserver = InvocationLeaseAcquiredObserver.Value;
            if (invocationObserver is not null)
                await invocationObserver().ConfigureAwait(false);
            if (!binding.Startup.IsDpoPolicyBaseProviderBound(
                    binding.FileAccess,
                    binding.BindingToken))
            {
                return ProviderUnavailable<
                    DesignConversionDpoPolicyBaseModelSet>();
            }
            if (cancellationToken.IsCancellationRequested)
                return Cancelled<DesignConversionDpoPolicyBaseModelSet>();

            var result = await invoke(binding.Provider, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return Cancelled<DesignConversionDpoPolicyBaseModelSet>();

            if (!binding.Startup.IsDpoPolicyBaseProviderBound(
                    binding.FileAccess,
                    binding.BindingToken))
            {
                return ProviderUnavailable<
                    DesignConversionDpoPolicyBaseModelSet>();
            }
            afterInvocationBeforeCommitForTesting?.Invoke();
            lock (_bindingGate)
            {
                if (_closed
                    || _revocationInProgress
                    || _generation != binding.Generation
                    || !ReferenceEquals(_binding, binding)
                    || !binding.Startup
                        .IsDpoPolicyBaseProviderBindingPublished(
                            binding.BindingToken))
                {
                    return ProviderUnavailable<
                        DesignConversionDpoPolicyBaseModelSet>();
                }
            }
            if (result is not null && !ShouldRevokeAfterResult(result))
                return result;

            var close = result is not null
                        && string.Equals(
                            result.ErrorCode,
                            ContinuousLearningPathIdentityException
                                .StableErrorCode,
                            StringComparison.Ordinal);
            WithdrawReadinessThenRevokeMatching(binding, close);
            if (result is not null)
                return result;
            return ProviderInvocationFailed<
                DesignConversionDpoPolicyBaseModelSet>();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<DesignConversionDpoPolicyBaseModelSet>();
        }
        catch (ContinuousLearningPathIdentityException)
        {
            WithdrawReadinessThenRevokeMatching(binding, close: true);
            return RetainedIdentityFailed<DesignConversionDpoPolicyBaseModelSet>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            WithdrawReadinessThenRevokeMatching(binding, close: false);
            return ProviderInvocationFailed<DesignConversionDpoPolicyBaseModelSet>();
        }
    }

    private static bool GatewayDescriptorMatchesBinding(
        DesignConversionDpoPolicyBasePreparationDescriptor preparation,
        BoundProvider binding)
    {
        if (!string.Equals(
                preparation.ProviderBindingRef,
                binding.ProviderBindingRef,
                StringComparison.Ordinal) ||
            preparation.ProviderBindingGeneration != binding.Generation)
        {
            return false;
        }
        var inner = binding.Provider.DescribePreparation();
        return inner is { IsSuccess: true, Value: not null } &&
               preparation.ToIdentity() == inner.Value.ToIdentity() &&
               DesignConversionDpoPolicyBaseMetadata.ConfigsEqual(
                   preparation.ToConfig(),
                   inner.Value.ToConfig()) &&
               string.Equals(
                   preparation.ConfigSha256,
                   inner.Value.ConfigSha256,
                   StringComparison.Ordinal);
    }

    private static DesignConversionDpoPolicyBasePreparationDescriptor
        StripGatewayBinding(
            DesignConversionDpoPolicyBasePreparationDescriptor preparation)
        => new(preparation.ToIdentity(), preparation.ToConfig());

    private void CancelBindAttempt(
        long capturedGeneration,
        BoundProvider? capturedBinding)
    {
        if (capturedBinding is not null)
            return;

        lock (_bindingGate)
        {
            if (!_closed
                && _binding is null
                && _generation == capturedGeneration)
            {
                _generation = unchecked(_generation + 1);
            }
        }
    }

    private void WithdrawReadinessThenRevokeCurrent(bool close)
    {
        BoundProvider? binding;
        ContinuousLearningDpoPolicyBaseWithdrawal withdrawal;
        lock (_revocationGate)
        {
            lock (_bindingGate)
            {
                binding = _binding;
                if (binding is null)
                {
                    if (close)
                    {
                        _closed = true;
                    }
                    else if (!_closed)
                    {
                        _generation = unchecked(_generation + 1);
                    }
                    return;
                }
                if (_revocationInProgress)
                {
                    if (close)
                        _closed = true;
                    return;
                }
                PreparePendingRetirementLocked(binding, close || _closed);
                _revocationInProgress = true;
            }
        }
        withdrawal = BeginStartupWithdrawalWithRecovery(binding, close);
        WithdrawReadinessThenRevokeCaptured(binding, close, withdrawal);
    }

    private void WithdrawReadinessThenRevokeMatching(
        BoundProvider binding,
        bool close)
    {
        ContinuousLearningDpoPolicyBaseWithdrawal withdrawal;
        lock (_revocationGate)
        {
            lock (_bindingGate)
            {
                if (_generation != binding.Generation ||
                    !ReferenceEquals(_binding, binding))
                {
                    if (close
                        && ReferenceEquals(_retiringBinding, binding)
                        && _preparedRetirementWork is { } retiringWork)
                    {
                        _closed = true;
                        retiringWork.UpgradeToClose();
                    }
                    return;
                }
                if (_revocationInProgress)
                {
                    if (close)
                    {
                        _closed = true;
                        _pendingRetirementWork?.UpgradeToClose();
                    }
                    return;
                }
                PreparePendingRetirementLocked(binding, close || _closed);
                _revocationInProgress = true;
            }
        }
        withdrawal = BeginStartupWithdrawalWithRecovery(binding, close);
        WithdrawReadinessThenRevokeCaptured(binding, close, withdrawal);
    }

    private ContinuousLearningDpoPolicyBaseWithdrawal
        BeginStartupWithdrawalWithRecovery(BoundProvider binding, bool close)
    {
        try
        {
            BeforeStartupReadinessWithdrawalObserver.Value?.Invoke();
            var withdrawal = BeginStartupWithdrawal(binding);
            CompleteStartupWithdrawalBegun(true);
            return withdrawal;
        }
        catch
        {
            RestorePreparedWithdrawalAfterPreTicketFailure(binding, close);
            throw;
        }
    }

    private void CompleteStartupWithdrawalBegun(bool begun)
    {
        TaskCompletionSource<bool>? completion;
        lock (_bindingGate)
        {
            completion = _pendingStartupWithdrawalBegunCompletion;
            _pendingStartupWithdrawalBegunCompletion = null;
        }
        completion?.TrySetResult(begun);
    }

    private void RestorePreparedWithdrawalAfterPreTicketFailure(
        BoundProvider binding,
        bool close)
    {
        TaskCompletionSource<bool>? failedAttempt = null;
        lock (_revocationGate)
        {
            lock (_bindingGate)
            {
                if (_generation != binding.Generation
                    || !ReferenceEquals(_binding, binding)
                    || !_revocationInProgress)
                {
                    return;
                }

                if (close || _closed)
                {
                    _closed = true;
                    _pendingRetirementWork?.UpgradeToClose();
                }
                _revocationInProgress = false;
                failedAttempt = _pendingStartupWithdrawalBegunCompletion;
                if (_pendingRetirementWork is not null)
                {
                    var retryAttempt = new TaskCompletionSource<bool>(
                        TaskCreationOptions.RunContinuationsAsynchronously);
                    _pendingStartupWithdrawalBegunCompletion = retryAttempt;
                    _startupWithdrawalBegunTask = retryAttempt.Task;
                }
                else
                {
                    _pendingStartupWithdrawalBegunCompletion = null;
                    _startupWithdrawalBegunTask =
                        StartupWithdrawalAlreadyBegunTask;
                }
            }
        }
        failedAttempt?.TrySetResult(false);
    }

    private static ContinuousLearningDpoPolicyBaseWithdrawal
        BeginStartupWithdrawal(BoundProvider binding)
    {
        var withdrawal = binding.Startup
            .TryBeginDpoPolicyBaseBindingFailureWithdrawal(
                binding.BindingToken);
        return withdrawal.Outcome
               == ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Rejected
            ? binding.Startup
                .TryBeginDpoPolicyBaseBindingInvariantFailureWithdrawal(
                    binding.BindingToken)
            : withdrawal;
    }

    private void WithdrawReadinessThenRevokeCaptured(
        BoundProvider binding,
        bool close,
        ContinuousLearningDpoPolicyBaseWithdrawal withdrawal)
    {
        RetirementWork? work = null;
        try
        {
            if (withdrawal.Outcome
                != ContinuousLearningDpoPolicyBaseWithdrawalOutcome.Rejected)
            {
                ReadinessWithdrawnBeforeBindingRemovedObserver.Value?.Invoke();
            }
        }
        finally
        {
            lock (_revocationGate)
            {
                lock (_bindingGate)
                {
                    if (_generation == binding.Generation
                        && ReferenceEquals(_binding, binding))
                    {
                        work = CommitPendingRetirementLocked(
                            binding,
                            close || _closed);
                    }
                    else if (ReferenceEquals(_retiringBinding, binding))
                    {
                        work = _preparedRetirementWork;
                        if (work is not null && (close || _closed))
                        {
                            _closed = true;
                            work.UpgradeToClose();
                        }
                    }
                }
            }

            if (withdrawal.Ticket is { } ticket)
            {
                binding.Startup
                    .CompleteDpoPolicyBaseBindingFailureWithdrawal(ticket);
            }
            else
            {
                binding.Startup
                    .CompletePendingDpoPolicyBaseWithdrawalForExactBinding(
                        binding.BindingToken);
            }
            if (work is not null)
                StartRetirementCompletion(work);
        }
    }

    private void RevokeMatchingAfterReadinessWithdrawal(
        BoundProvider binding,
        bool close)
    {
        Task? inProgressRetirement = null;
        RetirementWork? workToStart = null;
        lock (_revocationGate)
        {
            lock (_bindingGate)
            {
                if (_generation != binding.Generation
                    || !ReferenceEquals(_binding, binding))
                {
                    workToStart = ReferenceEquals(_retiringBinding, binding)
                        ? _preparedRetirementWork
                        : null;
                    if (workToStart is null)
                        return;
                    if (close)
                    {
                        _closed = true;
                        workToStart.UpgradeToClose();
                    }
                }
                else if (_revocationInProgress)
                {
                    if (close)
                    {
                        _closed = true;
                        _pendingRetirementWork?.UpgradeToClose();
                    }
                    inProgressRetirement = _retirementTask;
                }
                else
                {
                    PreparePendingRetirementLocked(binding, close);
                    _revocationInProgress = true;
                    workToStart = CommitPendingRetirementLocked(binding, close);
                }
            }
        }
        if (workToStart is not null)
            StartRetirementCompletion(workToStart);
        inProgressRetirement?.GetAwaiter().GetResult();
    }

    private RetirementWork PreparePendingRetirementLocked(
        BoundProvider binding,
        bool close)
    {
        if (_pendingRetirementWork is { } pendingWork)
        {
            if (!ReferenceEquals(pendingWork.Binding, binding))
                throw new InvalidOperationException();
            if (close)
                pendingWork.UpgradeToClose();
            return pendingWork;
        }

        var completion = _pendingRetirementCompletion
            ?? CreatePendingRetirementCompletion();
        var startupWithdrawalBegun =
            _pendingStartupWithdrawalBegunCompletion;
        if (startupWithdrawalBegun is null
            || startupWithdrawalBegun.Task.IsCompleted)
        {
            startupWithdrawalBegun = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingStartupWithdrawalBegunCompletion =
                startupWithdrawalBegun;
            _startupWithdrawalBegunTask = startupWithdrawalBegun.Task;
        }
        BeforeInvocationDrainAllocationObserver.Value?.Invoke();
        var invocationDrain = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        BeforeRetirementWorkAllocationObserver.Value?.Invoke();
        var work = new RetirementWork(
            binding,
            invocationDrain,
            completion,
            close);

        _pendingRetirementCompletion = completion;
        _retirementTask = completion.Task;
        _pendingRetirementWork = work;
        return work;
    }

    private RetirementWork CommitPendingRetirementLocked(
        BoundProvider binding,
        bool close)
    {
        var work = _pendingRetirementWork;
        if (work is null || !ReferenceEquals(work.Binding, binding))
            throw new InvalidOperationException();
        if (close)
            work.UpgradeToClose();

        binding.CommitWithdrawal(work.InvocationDrainCompletion);
        _pendingRetirementWork = null;
        _pendingRetirementCompletion = null;
        _retiringBinding = binding;
        _binding = null;
        _generation = unchecked(_generation + 1);
        _closed |= close;
        _preparedRetirementWork = work;
        return work;
    }

    private static TaskCompletionSource<object?>
        CreatePendingRetirementCompletion()
    {
        BeforePendingRetirementPublicationObserver.Value?.Invoke();
        return new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private void StartRetirementCompletion(RetirementWork work)
    {
        if (Interlocked.Exchange(ref work.Started, 1) == 0)
            _ = CompleteRetirementAsync(work);
    }

    private Task PrepareInvocationWithdrawalFromStartup(
        BoundProvider binding)
    {
        RetirementWork? work = null;
        Task drain = Task.CompletedTask;
        lock (_revocationGate)
        {
            lock (_bindingGate)
            {
                if (_generation == binding.Generation &&
                    ReferenceEquals(_binding, binding))
                {
                    PreparePendingRetirementLocked(binding, close: true);
                    _revocationInProgress = true;
                    work = CommitPendingRetirementLocked(
                        binding,
                        close: true);
                    drain = work.InvocationDrain;
                }
                else if (ReferenceEquals(_retiringBinding, binding) &&
                    _preparedRetirementWork is not null)
                {
                    work = _preparedRetirementWork;
                    drain = work.InvocationDrain;
                }
            }
        }
        if (work is not null)
            StartRetirementCompletion(work);
        return drain;
    }

    private BoundProvider? FindCurrentOrRetiringExactBindingLocked(
        ContinuousLearningDpoPolicyBaseBindingToken bindingToken)
    {
        if (_binding is { } current
            && ReferenceEquals(current.BindingToken, bindingToken))
        {
            return current;
        }
        return _retiringBinding is { } retiring
               && ReferenceEquals(retiring.BindingToken, bindingToken)
            ? retiring
            : null;
    }

    private async Task CompleteRetirementAsync(RetirementWork work)
    {
        Exception? failure = null;
        try
        {
            await Task.Yield();
            await work.Binding.Startup.AwaitRetirementAsync().ConfigureAwait(false);
            await work.InvocationDrain.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            failure = exception;
        }
        finally
        {
            lock (_bindingGate)
            {
                if (ReferenceEquals(_retiringBinding, work.Binding))
                    _retiringBinding = null;
                if (ReferenceEquals(_preparedRetirementWork, work))
                    _preparedRetirementWork = null;
                _revocationInProgress = false;
                if (work.Close)
                    _closed = true;
            }
        }
        if (failure is null)
            work.Completion.TrySetResult(null);
        else
            work.Completion.TrySetException(failure);
    }

    private static bool ShouldRevokeAfterResult(
        OperationResult<DesignConversionDpoPolicyBaseModelSet> result)
        => !result.IsSuccess
           && result.ErrorCode is
               DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .ConfigurationInvalidErrorCode
               or DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .CheckpointSourceFailedErrorCode
               or DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .CheckpointStreamInvalidErrorCode
               or DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .CheckpointHashMismatchErrorCode
               or DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .CheckpointChangedDuringLoadErrorCode
               or DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .CheckpointInvalidErrorCode
               or DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .CanonicalBpeTokenizerSourceFailedErrorCode
               or DesignConversionCheckpointBackedDpoPolicyBaseProvider
                   .CanonicalBpeTokenizerInvalidErrorCode
               or ContinuousLearningPathIdentityException.StableErrorCode;

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .CancelledErrorMessage,
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .CancelledErrorCode);

    private static OperationResult<T> RetainedIdentityFailed<T>()
        => OperationResult<T>.Fail(
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .PathIdentityErrorMessage,
            ContinuousLearningPathIdentityException.StableErrorCode);

    private static OperationResult<T> ProviderUnavailable<T>()
        => OperationResult<T>.Fail(
            ProviderUnavailableErrorMessage,
            ProviderUnavailableErrorCode);

    private static OperationResult<T> ProviderInvocationFailed<T>()
        => OperationResult<T>.Fail(
            ProviderInvocationFailedErrorMessage,
            ProviderInvocationFailedErrorCode);

    private sealed class BoundProvider
    {
        private readonly object _invocationGate = new();
        private int _activeInvocations;
        private bool _withdrawing;
        private TaskCompletionSource<object?>? _invocationDrain;

        internal BoundProvider(
            DesignConversionCheckpointBackedDpoPolicyBaseProvider provider,
            ContinuousLearningModelServerStartup startup,
            ServerOwnedContinuousLearningFileAccess fileAccess,
            ContinuousLearningDpoPolicyBaseBindingToken bindingToken,
            long generation)
        {
            Provider = provider;
            Startup = startup;
            FileAccess = fileAccess;
            BindingToken = bindingToken;
            Generation = generation;
            ProviderBindingRef = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(16));
        }

        internal DesignConversionCheckpointBackedDpoPolicyBaseProvider Provider { get; }
        internal ContinuousLearningModelServerStartup Startup { get; }
        internal ServerOwnedContinuousLearningFileAccess FileAccess { get; }
        internal ContinuousLearningDpoPolicyBaseBindingToken BindingToken { get; }
        internal long Generation { get; }
        internal string ProviderBindingRef { get; }

        internal bool TryAcquireInvocation(out IDisposable lease)
        {
            lock (_invocationGate)
            {
                if (_withdrawing)
                {
                    lease = null!;
                    return false;
                }
                BeforeInvocationLeaseAllocationObserver.Value?.Invoke();
                var candidateLease = new InvocationLease(this);
                _activeInvocations++;
                lease = candidateLease;
                return true;
            }
        }

        internal void CommitWithdrawal(
            TaskCompletionSource<object?> invocationDrain)
        {
            ArgumentNullException.ThrowIfNull(invocationDrain);
            lock (_invocationGate)
            {
                if (_activeInvocations == 0)
                    invocationDrain.TrySetResult(null);
                _invocationDrain = invocationDrain;
                _withdrawing = true;
            }
        }

        private void ReleaseInvocation()
        {
            TaskCompletionSource<object?>? complete = null;
            lock (_invocationGate)
            {
                if (_activeInvocations > 0)
                    _activeInvocations--;
                if (_withdrawing && _activeInvocations == 0)
                    complete = _invocationDrain;
            }
            complete?.TrySetResult(null);
        }

        private sealed class InvocationLease(BoundProvider owner) : IDisposable
        {
            private int _disposed;
            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                    owner.ReleaseInvocation();
            }
        }
    }

    private sealed class RetirementWork(
        BoundProvider binding,
        TaskCompletionSource<object?> invocationDrain,
        TaskCompletionSource<object?> completion,
        bool close)
    {
        private int _close = close ? 1 : 0;
        internal BoundProvider Binding { get; } = binding;
        internal TaskCompletionSource<object?> InvocationDrainCompletion
        {
            get;
        } = invocationDrain;
        internal Task InvocationDrain => InvocationDrainCompletion.Task;
        internal TaskCompletionSource<object?> Completion { get; } = completion;
        internal bool Close => Volatile.Read(ref _close) == 1;
        internal int Started;

        internal void UpgradeToClose()
            => Volatile.Write(ref _close, 1);
    }

    private sealed record BindAttemptCancellation(
        DesignConversionRetainedDpoPolicyBaseProviderGateway Gateway,
        long CapturedGeneration,
        BoundProvider? CapturedBinding)
    {
        internal void Cancel()
            => Gateway.CancelBindAttempt(CapturedGeneration, CapturedBinding);
    }
}

/// <summary>
/// Binds an inactive DesignConversion DPO policy-base provider to one retained
/// checkpoint identity. Binding hashes the checkpoint but does not load a
/// model, allocate training state, or expose a runtime activation path.
/// </summary>
internal static class DesignConversionRetainedDpoPolicyBaseComposition
{
    private const long MaximumCheckpointBytes = 16L * 1024 * 1024 * 1024;
    private const int HashBufferBytes = 128 * 1024;
    private const string PreflightCheckpointSha256 =
        "0000000000000000000000000000000000000000000000000000000000000000";
    internal const string RetainedTokenizerInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_POLICY_BASE_RETAINED_CANONICAL_BPE_TOKENIZER_INVALID";
    internal const string RetainedTokenizerInvalidErrorMessage =
        "The retained DesignConversion DPO canonical BPE tokenizer artifact is invalid.";
    private const string LegacyUnknownTrainingConfigurationFingerprint =
        "0000000000000000000000000000000000000000000000000000000000000000";

    internal static Task<OperationResult<
        DesignConversionCheckpointBackedDpoPolicyBaseProvider>>
        CreateInactiveProviderAsync(
            ServerOwnedContinuousLearningFileAccess? fileAccess,
            ModelConfig? modelConfig,
            string? modelId,
            string? revision,
            CancellationToken cancellationToken = default)
        => CreateInactiveProviderAsync(
            fileAccess,
            modelConfig,
            modelId,
            revision,
            rootInitializationSeed: null,
            LegacyUnknownTrainingConfigurationFingerprint,
            cancellationToken);

    internal static async Task<OperationResult<
        DesignConversionCheckpointBackedDpoPolicyBaseProvider>>
        CreateInactiveProviderAsync(
            ServerOwnedContinuousLearningFileAccess? fileAccess,
            ModelConfig? modelConfig,
            string? modelId,
            string? revision,
            ulong? rootInitializationSeed,
            string trainingConfigurationFingerprint,
            CancellationToken cancellationToken = default, // Preserve caller cancellation at the retained-read and import boundary.
            RepositoryOwnedFullLoraCheckpointDescriptor? initialFullCheckpoint = null) // Carry the same immutable full-LoRA declaration through startup and later policy loading.
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        if (fileAccess is null)
            return BindFailed();

        var source = new ContinuousLearningRetainedDpoPolicyCheckpointSource(
            fileAccess);
        var tokenizerSource =
            new ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource(
                fileAccess);
        var providerOptions = new DesignConversionDpoPolicyBaseProviderOptions(
            rootInitializationSeed,
            trainingConfigurationFingerprint) // Carry the existing exact training configuration fingerprint into retained-provider options.
        { // The retained provider receives the same trusted checkpoint declaration.
            InitialFullCheckpoint = initialFullCheckpoint // Carry the same immutable full-LoRA declaration through startup and later policy loading.
        }; // The retained provider receives the same trusted checkpoint declaration.
        var preflight =
            DesignConversionCheckpointBackedDpoPolicyBaseProvider.CreateWithOptions(
                source,
                modelConfig,
                modelId,
                revision,
                initialFullCheckpoint?.WeightsSha256 ?? PreflightCheckpointSha256, // Use the declared sealed digest when present, otherwise the legacy structural-preflight placeholder; retained bytes are hashed before real provider construction.
                providerOptions);
        if (!preflight.IsSuccess)
            return preflight;

        var tokenizerDescriptor = await CaptureCanonicalBpeTokenizerDescriptorAsync(
            tokenizerSource,
            CopyModelConfig(modelConfig!),
            cancellationToken);
        if (!tokenizerDescriptor.IsSuccess || tokenizerDescriptor.Value is null)
            return tokenizerDescriptor.MapError<
                DesignConversionCheckpointBackedDpoPolicyBaseProvider>();

        OperationResult<Stream> opened;
        try
        {
            opened = await source.OpenCheckpointSnapshotAsync(
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : PathIdentityFailed();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : BindFailed();
        }

        if (!opened.IsSuccess)
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled();

            return opened.MapError<
                DesignConversionCheckpointBackedDpoPolicyBaseProvider>();
        }

        var stream = opened.Value;
        if (stream is null)
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : BindFailed();

        string checkpointSha256;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reportedLength = ValidateCheckpointStream(stream);
            checkpointSha256 = await ComputeCheckpointSha256Async(
                stream,
                reportedLength,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : PathIdentityFailed();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : BindFailed();
        }
        finally
        {
            DisposeOwnedHashStream(stream);
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        try
        {
            fileAccess.EnsureRetained();
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : PathIdentityFailed();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled()
                : BindFailed();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled();

        return DesignConversionCheckpointBackedDpoPolicyBaseProvider.CreateWithOptions(
            source,
            modelConfig,
            modelId,
            revision,
            checkpointSha256,
            providerOptions,
            tokenizerDescriptor.Value,
            tokenizerSource);
    }

    private static async Task<OperationResult<
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>>
        CaptureCanonicalBpeTokenizerDescriptorAsync(
            ContinuousLearningRetainedDpoPolicyCanonicalBpeTokenizerSnapshotSource
                tokenizerSource,
            ModelConfig modelConfig,
            CancellationToken cancellationToken)
    {
        OperationResult<Stream> opened;
        try
        {
            opened = await tokenizerSource.OpenCanonicalBpeTokenizerSnapshotAsync(
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<
                DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled<
                    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>()
                : PathIdentityFailed<
                    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled<
                    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>()
                : BindFailed<
                    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }

        if (!opened.IsSuccess)
        {
            if (cancellationToken.IsCancellationRequested)
                return Cancelled<
                    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();

            return opened.MapError<
                DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }

        var stream = opened.Value;
        if (stream is null)
            return cancellationToken.IsCancellationRequested
                ? Cancelled<
                    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>()
                : TokenizerInvalid();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var descriptor =
                DesignConversionDpoPromotionArtifactContractCodec
                    .BuildCanonicalBpeTokenizerArtifactDescriptor(
                        stream,
                        modelConfig,
                        cancellationToken);
            if (!descriptor.IsSuccess || descriptor.Value is null)
            {
                if (cancellationToken.IsCancellationRequested ||
                    string.Equals(
                        descriptor.ErrorCode,
                        ContinuousLearningRetainedDpoPolicyCheckpointSource
                            .CancelledErrorCode,
                        StringComparison.Ordinal))
                {
                    return Cancelled<
                        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
                }

                return TokenizerInvalid();
            }

            cancellationToken.ThrowIfCancellationRequested();
            return descriptor;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled<
                DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
        catch (ContinuousLearningPathIdentityException)
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled<
                    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>()
                : PathIdentityFailed<
                    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? Cancelled<
                    DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>()
                : TokenizerInvalid();
        }
        finally
        {
            DisposeOwnedHashStream(stream);
        }
    }

    private static long ValidateCheckpointStream(Stream stream)
    {
        if (!stream.CanRead ||
            !stream.CanSeek ||
            stream.Position != 0)
        {
            throw new InvalidDataException();
        }

        var length = stream.Length;
        if (length <= 0 || length > MaximumCheckpointBytes)
            throw new InvalidDataException();
        return length;
    }

    private static async Task<string> ComputeCheckpointSha256Async(
        Stream stream,
        long reportedLength,
        CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[HashBufferBytes];
        long countedBytes = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                break;

            countedBytes = checked(countedBytes + read);
            if (countedBytes > MaximumCheckpointBytes)
                throw new InvalidDataException();
            hash.AppendData(buffer, 0, read);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (countedBytes <= 0 || countedBytes != reportedLength)
            throw new InvalidDataException();
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void DisposeOwnedHashStream(Stream stream)
    {
        try
        {
            stream.Dispose();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
        }
    }

    private static ModelConfig CopyModelConfig(ModelConfig config)
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

    private static OperationResult<
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>
        TokenizerInvalid()
        => OperationResult<
            DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor>.Fail(
                RetainedTokenizerInvalidErrorMessage,
                RetainedTokenizerInvalidErrorCode);

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .CancelledErrorMessage,
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .CancelledErrorCode);

    private static OperationResult<T> PathIdentityFailed<T>()
        => OperationResult<T>.Fail(
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .PathIdentityErrorMessage,
            ContinuousLearningPathIdentityException.StableErrorCode);

    private static OperationResult<T> BindFailed<T>()
        => OperationResult<T>.Fail(
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .BindFailedErrorMessage,
            ContinuousLearningRetainedDpoPolicyCheckpointSource
                .BindFailedErrorCode);

    private static OperationResult<
        DesignConversionCheckpointBackedDpoPolicyBaseProvider> Cancelled()
        => OperationResult<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>.Fail(
                ContinuousLearningRetainedDpoPolicyCheckpointSource
                    .CancelledErrorMessage,
                ContinuousLearningRetainedDpoPolicyCheckpointSource
                    .CancelledErrorCode);

    private static OperationResult<
        DesignConversionCheckpointBackedDpoPolicyBaseProvider>
        PathIdentityFailed()
        => OperationResult<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>.Fail(
                ContinuousLearningRetainedDpoPolicyCheckpointSource
                    .PathIdentityErrorMessage,
                ContinuousLearningPathIdentityException.StableErrorCode);

    private static OperationResult<
        DesignConversionCheckpointBackedDpoPolicyBaseProvider> BindFailed()
        => OperationResult<
            DesignConversionCheckpointBackedDpoPolicyBaseProvider>.Fail(
                ContinuousLearningRetainedDpoPolicyCheckpointSource
                    .BindFailedErrorMessage,
                ContinuousLearningRetainedDpoPolicyCheckpointSource
                    .BindFailedErrorCode);
}
