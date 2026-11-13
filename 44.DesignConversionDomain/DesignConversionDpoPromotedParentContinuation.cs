// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

internal enum DesignConversionDpoPromotedParentIoStage
{
    BeforeDirectoryAcquire,
    HandleBoundIo,
    BeforeTokenizerArtifactOpen,
    BeforeCanonicalBpeTokenizerArtifactOpen,
    BeforeSourceContractOpen,
    BeforePolicyContractOpen,
    BeforeRuntimeCheckpointOpen,
    BeforePolicyCheckpointOpen,
    AfterAllArtifactsOpen,
    BeforeCheckpointTransfer,
    BeforeRetainedCleanup
}

internal static class DesignConversionDpoPromotedParentContinuation
{
    internal const string StateInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_CONTINUATION_PARENT_STATE_INVALID";
    internal const string NotFoundErrorCode =
        "DESIGN_CONVERSION_DPO_CONTINUATION_PARENT_NOT_FOUND";
    internal const string PathInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_CONTINUATION_PARENT_PATH_INVALID";
    internal const string ContractInvalidErrorCode =
        "DESIGN_CONVERSION_DPO_CONTINUATION_PARENT_CONTRACT_INVALID";
    internal const string ArtifactMismatchErrorCode =
        "DESIGN_CONVERSION_DPO_CONTINUATION_PARENT_ARTIFACT_MISMATCH";
    internal const string TrustFailedErrorCode =
        "DESIGN_CONVERSION_DPO_CONTINUATION_PARENT_TRUST_FAILED";
    internal const string SourceAlreadyConsumedErrorCode =
        "DESIGN_CONVERSION_DPO_CONTINUATION_PARENT_SOURCE_ALREADY_CONSUMED";
    internal const string CancelledErrorCode =
        "DESIGN_CONVERSION_DPO_CONTINUATION_PARENT_CANCELLED";

    internal const string ContinuationUsedMetadataKey = "continuation_used";
    internal const string ParentPolicySha256MetadataKey =
        "continuation_parent_policy_sha256";
    internal const string ParentActiveRefSha256MetadataKey =
        "continuation_parent_active_ref_sha256";

    private const string StateInvalidSafeMessage =
        "The DesignConversion DPO continuation parent state is invalid.";
    private const string NotFoundSafeMessage =
        "A required DesignConversion DPO continuation parent artifact was not found.";
    private const string PathInvalidSafeMessage =
        "The DesignConversion DPO continuation parent path binding is invalid.";
    private const string ContractInvalidSafeMessage =
        "The DesignConversion DPO continuation parent contract is invalid.";
    private const string ArtifactMismatchSafeMessage =
        "A DesignConversion DPO continuation parent artifact does not match its trusted binding.";
    private const string TrustFailedSafeMessage =
        "The DesignConversion DPO continuation parent trust boundary failed.";
    private const string SourceAlreadyConsumedSafeMessage =
        "The DesignConversion DPO continuation parent checkpoint source was already consumed.";
    private const string CancelledSafeMessage =
        "DesignConversion DPO continuation parent resolution was cancelled.";

    internal static async Task<OperationResult<DesignConversionDpoPromotedParentContinuationLease>>
        ResolveAsync(
            DesignConversionLocalPromotionActive active,
            string allowedRootDirectory,
            CancellationToken cancellationToken,
            Action<DesignConversionDpoPromotedParentIoStage, string>?
                beforeIoForTesting = null)
    {
        if (cancellationToken.IsCancellationRequested)
            return Cancelled<DesignConversionDpoPromotedParentContinuationLease>();
        if (active is null)
            return StateInvalid<DesignConversionDpoPromotedParentContinuationLease>();
        if (active.Anchors is not { } anchors)
            return StateInvalid<DesignConversionDpoPromotedParentContinuationLease>();

        DesignConversionHandleBoundDirectory? directory = null;
        FileStream? tokenizerStream = null;
        FileStream? canonicalBpeTokenizerStream = null;
        FileStream? sourceContractStream = null;
        FileStream? policyContractStream = null;
        FileStream? runtimeCheckpointStream = null;
        FileStream? policyCheckpointStream = null;
        try
        {
            var resolved = DesignConversionDpoPromotionArtifactContractCodec.ResolvePaths(
                allowedRootDirectory,
                active.PromotionOutputDirectory,
                active.RunId);
            if (!resolved.IsSuccess || resolved.Value is null)
                return PathInvalid<DesignConversionDpoPromotedParentContinuationLease>();

            var paths = resolved.Value;
            if (!PathsExactlyMatch(active, paths))
                return PathInvalid<DesignConversionDpoPromotedParentContinuationLease>();

            cancellationToken.ThrowIfCancellationRequested();
            beforeIoForTesting?.Invoke(
                DesignConversionDpoPromotedParentIoStage.BeforeDirectoryAcquire,
                paths.OutputDirectory);
            directory = DesignConversionHandleBoundDirectory.AcquireExisting(
                paths.OutputDirectory,
                (stage, value) => beforeIoForTesting?.Invoke(
                    DesignConversionDpoPromotedParentIoStage.HandleBoundIo,
                    $"{stage}:{Path.GetFileName(value)}"));

            beforeIoForTesting?.Invoke(
                DesignConversionDpoPromotedParentIoStage.BeforeTokenizerArtifactOpen,
                paths.TokenizerArtifactFileName);
            tokenizerStream = TryOpenRetainedParentArtifact(
                directory,
                paths.TokenizerArtifactFileName);
            var hasCanonicalBpeTokenizer =
                anchors.CanonicalBpeTokenizer is not null;
            if (hasCanonicalBpeTokenizer)
            {
                beforeIoForTesting?.Invoke(
                    DesignConversionDpoPromotedParentIoStage
                        .BeforeCanonicalBpeTokenizerArtifactOpen,
                    paths.CanonicalBpeTokenizerFileName ?? string.Empty);
                canonicalBpeTokenizerStream = TryOpenRetainedParentArtifact(
                    directory,
                    paths.CanonicalBpeTokenizerFileName ?? string.Empty);
            }
            beforeIoForTesting?.Invoke(
                DesignConversionDpoPromotedParentIoStage.BeforeSourceContractOpen,
                paths.SourceEncoderContractFileName);
            sourceContractStream = TryOpenRetainedParentArtifact(
                directory,
                paths.SourceEncoderContractFileName);
            beforeIoForTesting?.Invoke(
                DesignConversionDpoPromotedParentIoStage.BeforePolicyContractOpen,
                paths.PolicyCheckpointContractFileName);
            policyContractStream = TryOpenRetainedParentArtifact(
                directory,
                paths.PolicyCheckpointContractFileName);
            beforeIoForTesting?.Invoke(
                DesignConversionDpoPromotedParentIoStage.BeforeRuntimeCheckpointOpen,
                paths.RuntimeCheckpointFileName);
            runtimeCheckpointStream = TryOpenRetainedParentArtifact(
                directory,
                paths.RuntimeCheckpointFileName);
            beforeIoForTesting?.Invoke(
                DesignConversionDpoPromotedParentIoStage.BeforePolicyCheckpointOpen,
                anchors.PolicyCheckpointFileName);
            policyCheckpointStream = TryOpenRetainedParentArtifact(
                directory,
                anchors.PolicyCheckpointFileName);

            if (tokenizerStream is null ||
                hasCanonicalBpeTokenizer && canonicalBpeTokenizerStream is null ||
                sourceContractStream is null ||
                policyContractStream is null ||
                runtimeCheckpointStream is null ||
                policyCheckpointStream is null)
            {
                var cleanupPolicy = policyCheckpointStream;
                var cleanupRuntime = runtimeCheckpointStream;
                var cleanupPolicyContract = policyContractStream;
                var cleanupSourceContract = sourceContractStream;
                var cleanupCanonicalBpeTokenizer = canonicalBpeTokenizerStream;
                var cleanupTokenizer = tokenizerStream;
                var cleanupDirectory = directory;
                directory = null;
                tokenizerStream = null;
                canonicalBpeTokenizerStream = null;
                sourceContractStream = null;
                policyContractStream = null;
                runtimeCheckpointStream = null;
                policyCheckpointStream = null;
                DisposeResources(
                    beforeIoForTesting,
                    cleanupPolicy,
                    cleanupRuntime,
                    cleanupPolicyContract,
                    cleanupSourceContract,
                    cleanupCanonicalBpeTokenizer,
                    cleanupTokenizer,
                    cleanupDirectory);
                return NotFound<DesignConversionDpoPromotedParentContinuationLease>();
            }

            cancellationToken.ThrowIfCancellationRequested();
            beforeIoForTesting?.Invoke(
                DesignConversionDpoPromotedParentIoStage.AfterAllArtifactsOpen,
                string.Empty);
            var validated = await DesignConversionDpoPromotionArtifactContractCodec
                .ReadAndValidatePromotedParentAsync(
                    directory,
                    paths,
                    anchors,
                    active.ActiveCheckpointSha256,
                    tokenizerStream,
                    canonicalBpeTokenizerStream,
                    sourceContractStream,
                    policyContractStream,
                    runtimeCheckpointStream,
                    policyCheckpointStream,
                    cancellationToken);
            if (!validated.IsSuccess || validated.Value is null)
            {
                var cleanupPolicy = policyCheckpointStream;
                var cleanupRuntime = runtimeCheckpointStream;
                var cleanupPolicyContract = policyContractStream;
                var cleanupSourceContract = sourceContractStream;
                var cleanupCanonicalBpeTokenizer = canonicalBpeTokenizerStream;
                var cleanupTokenizer = tokenizerStream;
                var cleanupDirectory = directory;
                directory = null;
                tokenizerStream = null;
                canonicalBpeTokenizerStream = null;
                sourceContractStream = null;
                policyContractStream = null;
                runtimeCheckpointStream = null;
                policyCheckpointStream = null;
                DisposeResources(
                    beforeIoForTesting,
                    cleanupPolicy,
                    cleanupRuntime,
                    cleanupPolicyContract,
                    cleanupSourceContract,
                    cleanupCanonicalBpeTokenizer,
                    cleanupTokenizer,
                    cleanupDirectory);
                return validated.MapError<
                    DesignConversionDpoPromotedParentContinuationLease>();
            }

            // Одни и те же уже проверенные bytes дают две независимые
            // one-shot custody роли: bridge proof доказывает expected state,
            // provider transfer отдаёт checkpoint built-in loader-у ровно раз.
            var providerCheckpointSource = new OneShotCheckpointSource(
                validated.Value.PolicyCheckpointSnapshotBytes,
                ProviderTransferCheckpointSourceRole,
                beforeIoForTesting);
            var bridgeProofCheckpointSource = new OneShotCheckpointSource(
                validated.Value.PolicyCheckpointSnapshotBytes,
                BridgeProofCheckpointSourceRole,
                beforeIoForTesting);
            var request = new DesignConversionDpoPolicyContinuationRequest(
                providerCheckpointSource,
                validated.Value.SourceEncoderContract,
                validated.Value.PolicyCheckpointContract,
                validated.Value.TokenizerArtifact,
                validated.Value.RuntimeEncodingBinding)
            {
                ParentCanonicalBpeTokenizerSnapshot =
                    validated.Value.ParentCanonicalBpeTokenizerSnapshot,
                CheckpointMutationAdmission =
                    validated.Value.CheckpointMutationAdmission,
                BridgeProofCheckpointSource = bridgeProofCheckpointSource
            };
            var lease = new DesignConversionDpoPromotedParentContinuationLease(
                request,
                providerCheckpointSource,
                bridgeProofCheckpointSource,
                tokenizerStream,
                canonicalBpeTokenizerStream,
                sourceContractStream,
                policyContractStream,
                runtimeCheckpointStream,
                policyCheckpointStream,
                directory,
                active.ActiveLocalModelRef,
                validated.Value.PolicyCheckpointContract.Binding
                    .PolicyCheckpointSha256,
                beforeIoForTesting);

            policyCheckpointStream = null;
            runtimeCheckpointStream = null;
            policyContractStream = null;
            sourceContractStream = null;
            canonicalBpeTokenizerStream = null;
            tokenizerStream = null;
            directory = null;
            return OperationResult<
                DesignConversionDpoPromotedParentContinuationLease>.Ok(lease);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            DisposeResources(
                beforeIoForTesting,
                policyCheckpointStream,
                runtimeCheckpointStream,
                policyContractStream,
                sourceContractStream,
                canonicalBpeTokenizerStream,
                tokenizerStream,
                directory);
            return Cancelled<DesignConversionDpoPromotedParentContinuationLease>();
        }
        catch (DesignConversionHandleBoundNotFoundException)
        {
            DisposeResources(
                beforeIoForTesting,
                policyCheckpointStream,
                runtimeCheckpointStream,
                policyContractStream,
                sourceContractStream,
                canonicalBpeTokenizerStream,
                tokenizerStream,
                directory);
            return NotFound<DesignConversionDpoPromotedParentContinuationLease>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            DisposeResources(
                beforeIoForTesting,
                policyCheckpointStream,
                runtimeCheckpointStream,
                policyContractStream,
                sourceContractStream,
                canonicalBpeTokenizerStream,
                tokenizerStream,
                directory);
            return TrustFailed<DesignConversionDpoPromotedParentContinuationLease>();
        }
        catch (Exception exception)
        {
            var captured = ExceptionDispatchInfo.Capture(exception);
            try
            {
                DisposeResources(
                    beforeIoForTesting,
                    policyCheckpointStream,
                    runtimeCheckpointStream,
                    policyContractStream,
                    sourceContractStream,
                    canonicalBpeTokenizerStream,
                    tokenizerStream,
                    directory);
            }
            catch
            {
                // The original fatal/cancellation graph retains precedence.
            }

            captured.Throw();
            throw;
        }
    }

    private static bool PathsExactlyMatch(
        DesignConversionLocalPromotionActive active,
        DesignConversionDpoPromotionArtifactPaths paths)
        => string.Equals(
               active.PromotionOutputDirectory,
               paths.OutputDirectory,
               StringComparison.Ordinal) &&
           string.Equals(
               active.PromotedCheckpointPath,
               paths.RuntimeCheckpointPath,
               StringComparison.Ordinal) &&
           string.Equals(
               active.ModelCheckpointPath,
               paths.RuntimeCheckpointPath,
               StringComparison.Ordinal) &&
           string.Equals(
               active.ManifestCheckpointPath,
               paths.RuntimeCheckpointPath,
               StringComparison.Ordinal) &&
           string.Equals(
               active.ManifestTokenizerArtifactPath,
               paths.TokenizerArtifactPath,
               StringComparison.Ordinal) &&
           string.Equals(
               active.ManifestSourceEncoderContractPath,
               paths.SourceEncoderContractPath,
               StringComparison.Ordinal) &&
           string.Equals(
               active.ManifestPolicyCheckpointContractPath,
               paths.PolicyCheckpointContractPath,
               StringComparison.Ordinal) &&
           string.Equals(
               Path.GetFileName(active.PromotedCheckpointPath),
               paths.RuntimeCheckpointFileName,
               StringComparison.Ordinal) &&
           string.Equals(
               Path.GetFileName(active.ManifestTokenizerArtifactPath),
               paths.TokenizerArtifactFileName,
               StringComparison.Ordinal) &&
           string.Equals(
               Path.GetFileName(active.ManifestSourceEncoderContractPath),
               paths.SourceEncoderContractFileName,
               StringComparison.Ordinal) &&
           string.Equals(
               Path.GetFileName(active.ManifestPolicyCheckpointContractPath),
               paths.PolicyCheckpointContractFileName,
               StringComparison.Ordinal);

    private static FileStream? TryOpenRetainedParentArtifact(
        DesignConversionHandleBoundDirectory directory,
        string leafName)
    {
        // Windows FileShare.Read даёт mandatory-denial для write/delete/move.
        // Linux этого не обещает: openat(O_NOFOLLOW) даёт retained inode, а
        // валидированные bytes выше переводят deferred checkpoint transfer в
        // path-free snapshot вместо надежды на advisory flock.
        return directory.TryOpenExistingReadOnly(leafName);
    }

    internal static string HashActiveRef(string activeRef)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(activeRef)))
            .ToLowerInvariant();

    internal static OperationResult<T> StateInvalid<T>()
        => OperationResult<T>.Fail(StateInvalidSafeMessage, StateInvalidErrorCode);

    internal static OperationResult<T> NotFound<T>()
        => OperationResult<T>.Fail(NotFoundSafeMessage, NotFoundErrorCode);

    internal static OperationResult<T> PathInvalid<T>()
        => OperationResult<T>.Fail(PathInvalidSafeMessage, PathInvalidErrorCode);

    internal static OperationResult<T> ContractInvalid<T>()
        => OperationResult<T>.Fail(ContractInvalidSafeMessage, ContractInvalidErrorCode);

    internal static OperationResult<T> ArtifactMismatch<T>()
        => OperationResult<T>.Fail(ArtifactMismatchSafeMessage, ArtifactMismatchErrorCode);

    internal static OperationResult<T> TrustFailed<T>()
        => OperationResult<T>.Fail(TrustFailedSafeMessage, TrustFailedErrorCode);

    internal static OperationResult<T> SourceAlreadyConsumed<T>()
        => OperationResult<T>.Fail(
            SourceAlreadyConsumedSafeMessage,
            SourceAlreadyConsumedErrorCode);

    internal static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(CancelledSafeMessage, CancelledErrorCode);

    internal static bool DisposeResources(
        Action<DesignConversionDpoPromotedParentIoStage, string>?
            beforeIoForTesting,
        params IDisposable?[] resources)
    {
        ExceptionDispatchInfo? fatal = null;
        var ordinaryFailure = false;
        try
        {
            beforeIoForTesting?.Invoke(
                DesignConversionDpoPromotedParentIoStage.BeforeRetainedCleanup,
                string.Empty);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            ordinaryFailure = true;
        }
        catch (Exception exception)
        {
            fatal = ExceptionDispatchInfo.Capture(exception);
        }

        foreach (var resource in resources)
        {
            if (resource is null)
                continue;
            try
            {
                resource.Dispose();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                ordinaryFailure = true;
            }
            catch (Exception exception)
            {
                fatal ??= ExceptionDispatchInfo.Capture(exception);
            }
        }

        fatal?.Throw();
        return ordinaryFailure;
    }

    internal const string BridgeProofCheckpointSourceRole = "bridge_proof";
    internal const string ProviderTransferCheckpointSourceRole = "provider_transfer";

    internal sealed class OneShotCheckpointSource
        : IDesignConversionDpoPolicyCheckpointSource, IDisposable
    {
        private byte[]? _snapshotBytes;
        private readonly Action<DesignConversionDpoPromotedParentIoStage, string>?
            _beforeIoForTesting;
        private int _consumed;
        private int _disposed;

        internal OneShotCheckpointSource(
            byte[] snapshotBytes,
            string role,
            Action<DesignConversionDpoPromotedParentIoStage, string>?
                beforeIoForTesting)
        {
            ArgumentNullException.ThrowIfNull(snapshotBytes);
            if (snapshotBytes.Length == 0)
                throw new ArgumentException(
                    "Checkpoint snapshot bytes are required.",
                    nameof(snapshotBytes));
            _snapshotBytes = snapshotBytes.ToArray();
            Role = string.IsNullOrWhiteSpace(role)
                ? ProviderTransferCheckpointSourceRole
                : role;
            _beforeIoForTesting = beforeIoForTesting;
        }

        internal string Role { get; }

        internal int TransferCount => Volatile.Read(ref _consumed);

        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Cancelled<Stream>());

            // Dispose закрывает custody без передачи: после cleanup никто не
            // должен получить bytes и callback не должен создавать видимость IO.
            if (Volatile.Read(ref _disposed) != 0)
                return Task.FromResult(SourceAlreadyConsumed<Stream>());

            _beforeIoForTesting?.Invoke(
                DesignConversionDpoPromotedParentIoStage.BeforeCheckpointTransfer,
                Role);
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Cancelled<Stream>());
            if (Interlocked.CompareExchange(ref _consumed, 1, 0) != 0)
                return Task.FromResult(SourceAlreadyConsumed<Stream>());

            var transferred = Interlocked.Exchange(ref _snapshotBytes, null);
            return Task.FromResult(
                transferred is null
                    ? SourceAlreadyConsumed<Stream>()
                    : OperationResult<Stream>.Ok(
                        new MemoryStream(transferred, writable: false)));
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            _ = Interlocked.Exchange(ref _snapshotBytes, null);
        }
    }
}

internal sealed class DesignConversionDpoPromotedParentContinuationLease
    : IDisposable
{
    private readonly IDisposable _providerCheckpointSource;
    private readonly IDisposable _bridgeProofCheckpointSource;
    private readonly FileStream _tokenizerStream;
    private readonly FileStream? _canonicalBpeTokenizerStream;
    private readonly FileStream _sourceContractStream;
    private readonly FileStream _policyContractStream;
    private readonly FileStream _runtimeCheckpointStream;
    private readonly FileStream _policyCheckpointStream;
    private readonly DesignConversionHandleBoundDirectory _directory;
    private readonly Action<DesignConversionDpoPromotedParentIoStage, string>?
        _beforeIoForTesting;
    private int _disposed;

    internal DesignConversionDpoPromotedParentContinuationLease(
        DesignConversionDpoPolicyContinuationRequest request,
        IDisposable providerCheckpointSource,
        IDisposable bridgeProofCheckpointSource,
        FileStream tokenizerStream,
        FileStream? canonicalBpeTokenizerStream,
        FileStream sourceContractStream,
        FileStream policyContractStream,
        FileStream runtimeCheckpointStream,
        FileStream policyCheckpointStream,
        DesignConversionHandleBoundDirectory directory,
        string activeLocalModelRef,
        string parentPolicyCheckpointSha256,
        Action<DesignConversionDpoPromotedParentIoStage, string>?
            beforeIoForTesting)
    {
        Request = request;
        _providerCheckpointSource = providerCheckpointSource;
        _bridgeProofCheckpointSource = bridgeProofCheckpointSource;
        _tokenizerStream = tokenizerStream;
        _canonicalBpeTokenizerStream = canonicalBpeTokenizerStream;
        _sourceContractStream = sourceContractStream;
        _policyContractStream = policyContractStream;
        _runtimeCheckpointStream = runtimeCheckpointStream;
        _policyCheckpointStream = policyCheckpointStream;
        _directory = directory;
        ActiveLocalModelRef = activeLocalModelRef;
        ParentPolicyCheckpointSha256 = parentPolicyCheckpointSha256;
        _beforeIoForTesting = beforeIoForTesting;
    }

    internal DesignConversionDpoPolicyContinuationRequest Request { get; }

    internal string ActiveLocalModelRef { get; }

    internal string ParentPolicyCheckpointSha256 { get; }

    internal async Task<OperationResult<T>> InvokeAndDisposeAsync<T>(
        Func<DesignConversionDpoPolicyContinuationRequest,
            CancellationToken,
            Task<OperationResult<T>>> operation,
        CancellationToken cancellationToken)
    {
        OperationResult<T>? result = null;
        ExceptionDispatchInfo? operationException = null;
        try
        {
            result = await operation(Request, cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            result = OperationResult<T>.Fail(
                "DesignConversion DPO continuation was cancelled.",
                "CANCELLED");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            result = DesignConversionDpoPromotedParentContinuation
                .TrustFailed<T>();
        }
        catch (Exception exception)
        {
            operationException = ExceptionDispatchInfo.Capture(exception);
        }

        ExceptionDispatchInfo? cleanupException = null;
        var ordinaryCleanupFailure = false;
        try
        {
            ordinaryCleanupFailure = DisposeCore();
        }
        catch (Exception exception)
        {
            cleanupException = ExceptionDispatchInfo.Capture(exception);
        }

        operationException?.Throw();
        cleanupException?.Throw();
        if (result is not null &&
            !result.IsSuccess &&
            string.Equals(result.ErrorCode, "CANCELLED", StringComparison.Ordinal))
        {
            return result;
        }
        return ordinaryCleanupFailure || result is null
            ? DesignConversionDpoPromotedParentContinuation.TrustFailed<T>()
            : result;
    }

    public void Dispose()
        => _ = DisposeCore();

    private bool DisposeCore()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return false;
        return DesignConversionDpoPromotedParentContinuation.DisposeResources(
            _beforeIoForTesting,
            _bridgeProofCheckpointSource,
            _providerCheckpointSource,
            _policyCheckpointStream,
            _runtimeCheckpointStream,
            _policyContractStream,
            _sourceContractStream,
            _canonicalBpeTokenizerStream,
            _tokenizerStream,
            _directory);
    }
}
