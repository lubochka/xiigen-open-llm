// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;
using TransformerModel;

/// <summary>
/// A bounded, canonical publication request.  Domain and publication values are
/// identifiers, not paths, and are validated before an AdapterRoot capability
/// can turn them into a deterministic final leaf name.
/// </summary>
internal sealed record ServerOwnedContinuousLearningAdapterPublicationRequest
{
    private const int MaximumIdentifierLength = 48;

    private ServerOwnedContinuousLearningAdapterPublicationRequest(
        string domain,
        string publicationId)
    {
        Domain = domain;
        PublicationId = publicationId;
    }

    internal string Domain { get; }

    internal string PublicationId { get; }

    internal static ServerOwnedContinuousLearningAdapterPublicationRequest Create(
        string domain,
        string publicationId)
    {
        if (!IsCanonicalIdentifier(domain))
            throw new ArgumentException("A bounded canonical domain identifier is required.", nameof(domain));
        if (!IsCanonicalIdentifier(publicationId))
        {
            throw new ArgumentException(
                "A bounded canonical publication identifier is required.",
                nameof(publicationId));
        }

        return new ServerOwnedContinuousLearningAdapterPublicationRequest(
            domain,
            publicationId);
    }

    internal string GetCanonicalFinalLeafName()
    {
        // Length-prefix both components: separators alone are not injective
        // because valid canonical identifiers themselves may contain '--'.
        var leafName = $"adapter-v2--d{Domain.Length}-{Domain}--p{PublicationId.Length}-{PublicationId}.lora";
        if (leafName.Length > 255)
            throw new InvalidOperationException("The canonical adapter leaf exceeds the Windows leaf limit.");
        return leafName;
    }

    /// <summary>
    /// Validates a receipt leaf without reconstructing it from caller-provided
    /// path components. Reconciliation receives opaque publication evidence,
    /// so this is deliberately as strict as <see cref="GetCanonicalFinalLeafName"/>.
    /// </summary>
    internal static bool IsCanonicalFinalLeafName(string? leafName)
    {
        const string prefix = "adapter-v2--d";
        const string publicationSeparator = "--p";
        const string suffix = ".lora";

        if (string.IsNullOrEmpty(leafName) || leafName.Length > 255 ||
            !leafName.StartsWith(prefix, StringComparison.Ordinal) ||
            !leafName.EndsWith(suffix, StringComparison.Ordinal))
        {
            return false;
        }

        var cursor = prefix.Length;
        if (!TryReadLengthPrefixedIdentifier(
                leafName,
                ref cursor,
                '-',
                out var domain) ||
            !leafName.AsSpan(cursor).StartsWith(publicationSeparator, StringComparison.Ordinal))
        {
            return false;
        }

        cursor += publicationSeparator.Length;
        if (!TryReadLengthPrefixedIdentifier(
                leafName,
                ref cursor,
                '-',
                out var publicationId))
        {
            return false;
        }

        return cursor == leafName.Length - suffix.Length &&
               IsCanonicalIdentifier(domain) &&
               IsCanonicalIdentifier(publicationId);
    }

    private static bool TryReadLengthPrefixedIdentifier(
        string value,
        ref int cursor,
        char separator,
        out string identifier)
    {
        identifier = string.Empty;
        var lengthStart = cursor;
        while (cursor < value.Length && char.IsAsciiDigit(value[cursor]))
            cursor++;
        if (cursor == lengthStart || cursor >= value.Length || value[cursor] != separator ||
            (cursor - lengthStart > 1 && value[lengthStart] == '0') ||
            !int.TryParse(value.AsSpan(lengthStart, cursor - lengthStart), out var length))
        {
            return false;
        }

        cursor++;
        if (length <= 0 || length > MaximumIdentifierLength ||
            cursor > value.Length - length)
        {
            return false;
        }

        identifier = value.Substring(cursor, length);
        cursor += length;
        return true;
    }

    private static bool IsCanonicalIdentifier(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= MaximumIdentifierLength &&
           value[0] is >= 'a' and <= 'z' or >= '0' and <= '9' &&
           value[^1] is >= 'a' and <= 'z' or >= '0' and <= '9' &&
           value.All(character =>
               character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-');
}

internal readonly record struct ServerOwnedContinuousLearningAdapterRootIdentity(
    ulong VolumeSerialNumber,
    ulong FileIdLow,
    ulong FileIdHigh);

/// <summary>
/// Identity of the final regular-file object observed while its no-write-share
/// handle was retained through validation and root-directory flush.
/// </summary>
internal readonly record struct ServerOwnedContinuousLearningAdapterArtifactIdentity(
    ulong VolumeSerialNumber,
    ulong FileIdLow,
    ulong FileIdHigh);

/// <summary>
/// Opaque publication evidence.  It intentionally contains a canonical leaf
/// name and identities only: no configured path and no native handle escape.
/// </summary>
internal readonly record struct ServerOwnedContinuousLearningAdapterPublication(
    string CanonicalLeafName,
    ServerOwnedContinuousLearningAdapterRootIdentity RetainedRootIdentity,
    ServerOwnedContinuousLearningAdapterArtifactIdentity FinalArtifactIdentity,
    long Length,
    string Sha256Hex);

/// <summary>
/// Path-free immutable evidence for one exact bounded adapter read. The
/// retained bytes are private and callers can obtain only defensive copies.
/// </summary>
internal sealed class ServerOwnedContinuousLearningAdapterSnapshot
{
    private readonly byte[] _bytes;

    internal ServerOwnedContinuousLearningAdapterSnapshot(
        ServerOwnedContinuousLearningAdapterPublication publication,
        byte[] bytes)
    {
        Publication = publication;
        _bytes = bytes.ToArray();
    }

    internal ServerOwnedContinuousLearningAdapterPublication Publication { get; }

    internal string CanonicalLeafName => Publication.CanonicalLeafName;

    internal ServerOwnedContinuousLearningAdapterRootIdentity RetainedRootIdentity
        => Publication.RetainedRootIdentity;

    internal ServerOwnedContinuousLearningAdapterArtifactIdentity FinalArtifactIdentity
        => Publication.FinalArtifactIdentity;

    internal long Length => Publication.Length;

    internal string Sha256Hex => Publication.Sha256Hex;

    internal byte[] CopyBytes() => _bytes.ToArray();
}

internal enum ServerOwnedContinuousLearningAdapterIoStage
{
    AfterTemporaryLeafCreated,
    AfterSerializationFlush,
    BeforeReadbackValidation,
    BeforeCommit,
    AfterCommit,
    BeforeRootDirectoryFlush,
    AfterRootDirectoryFlush,
    BeforeReconciliationRootDirectoryFlush,
    AfterReconciliationRootDirectoryFlush,
    BeforeExactReadOpen,
    AfterExactReadOpen,
    DuringExactRead,
    AfterExactReadValidation,
}

internal readonly record struct ServerOwnedContinuousLearningAdapterIoObservation(
    ServerOwnedContinuousLearningAdapterIoStage Stage,
    string? CanonicalTemporaryLeafName);

internal sealed record ServerOwnedContinuousLearningAdapterRootCapabilityOptions
{
    internal long MaximumArtifactBytes { get; init; } =
        ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes;

    // This exposes only the canonical temporary leaf, never an absolute path;
    // production code does not supply it.  It exists to adversarially test
    // replacement of a temporary directory entry between write and commit.
    internal Action<ServerOwnedContinuousLearningAdapterIoObservation>?
        ObserveIoStageForTesting
    {
        get;
        init;
    }

    // A narrow test seam for proving cleanup on serializer failure.  The
    // production path always uses LlmModel.SaveAdapterCheckpoint(Stream).
    internal Action<LlmModel, Stream>? SerializeAdapterForTesting
    {
        get;
        init;
    }

    // Test-only wrapper for proving propagation of cancellation into an
    // in-flight asynchronous inner stream operation. It receives a
    // caller-owned FileStream and must not dispose it.
    internal Func<Stream, Stream>? WrapSerializerStreamForTesting
    {
        get;
        init;
    }

    // Test-only disposal witness. It proves the bounded writer releases its
    // IncrementalHash on success and exceptional serializer paths without
    // exposing that state from the production capability.
    internal Action? BoundedWriterDisposedForTesting { get; init; }

    // Narrow failure seam used only to prove that a rename is reported as a
    // committed-but-unverified artifact if durable directory flush cannot be
    // established. Production always invokes the native flush.
    internal bool FailRootDirectoryFlushForTesting { get; init; }
}

/// <summary>
/// Revocable one-shot authority over AdapterRoot publication.  It never opens
/// a configured path after acquisition; every later file operation is relative
/// to the retained root handle and rejects reparse points, identity changes,
/// occupied final names and unchecked cleanup targets.
/// </summary>
internal sealed class ServerOwnedContinuousLearningAdapterRootCapability : IDisposable
{
    private readonly ServerOwnedContinuousLearningFileAccess _owner;
    private readonly SafeFileHandle _ownerLockHandle;
    private readonly ServerOwnedContinuousLearningAdapterRootCapabilityOptions
        _options;
    private bool _disposed;
    private bool _revoked;
    private bool _rootDirectoryFlushFailureInjected;

    internal ServerOwnedContinuousLearningAdapterRootCapability(
        ServerOwnedContinuousLearningFileAccess owner,
        SafeFileHandle ownerLockHandle,
        ServerOwnedContinuousLearningAdapterRootCapabilityOptions options)
    {
        _owner = owner;
        _ownerLockHandle = ownerLockHandle;
        _options = options;
    }

    internal ServerOwnedContinuousLearningAdapterPublication Publish(
        LlmModel model,
        ServerOwnedContinuousLearningAdapterPublicationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        if (_options.MaximumArtifactBytes is <= 0 or >
            ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes)
        {
            throw new InvalidOperationException("The AdapterRoot publication bound is invalid.");
        }

        return _owner.PublishAdapterCheckpoint(
            this,
            model,
            request,
            cancellationToken);
    }

    /// <summary>
    /// Re-establishes the identity, contents, model-specific adapter schema,
    /// and directory durability of an artifact whose no-replace rename had
    /// already committed when publication reported an indeterminate outcome.
    /// It accepts only opaque receipt data and never exposes a path or handle.
    /// Cancellation is honoured before opening the final leaf and before the
    /// reconciliation directory flush; once that flush starts, verification
    /// completes without caller cancellation ambiguity.
    /// </summary>
    internal ServerOwnedContinuousLearningAdapterPublication ReconcileCommitted(
        LlmModel model,
        ServerOwnedContinuousLearningAdapterPublication receipt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        cancellationToken.ThrowIfCancellationRequested();
        if (_options.MaximumArtifactBytes is <= 0 or >
            ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes)
        {
            throw new InvalidOperationException("The AdapterRoot publication bound is invalid.");
        }

        return _owner.ReconcileCommittedAdapterCheckpoint(
            this,
            model,
            receipt,
            cancellationToken);
    }

    /// <summary>
    /// Reopens an already-published adapter from opaque publication evidence.
    /// The caller supplies no path or model authority. The returned snapshot
    /// contains bounded defensive bytes and immutable identity/hash evidence.
    /// </summary>
    internal ServerOwnedContinuousLearningAdapterSnapshot ReadExact(
        ServerOwnedContinuousLearningAdapterPublication receipt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_options.MaximumArtifactBytes is <= 0 or >
            ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes)
        {
            throw new InvalidOperationException("The AdapterRoot exact-read bound is invalid.");
        }

        return _owner.ReadExactAdapterCheckpoint(
            this,
            receipt,
            cancellationToken);
    }

    /// <summary>
    /// Validates and converts stable path-free artifact evidence before
    /// delegating to the receipt-based exact-read trust boundary.
    /// </summary>
    internal ServerOwnedContinuousLearningAdapterSnapshot ReadExact(
        AdapterArtifactReference reference,
        CancellationToken cancellationToken = default)
        => ReadExact(
            AdapterArtifactReference.ToPublication(reference),
            cancellationToken);

    public void Dispose()
        => _owner.DisposeAdapterRootCapability(this);

    internal void ThrowIfUnavailableLocked()
    {
        if (_disposed || _revoked)
            throw new ContinuousLearningPathIdentityException();
    }

    internal long GetMaximumArtifactBytesLocked()
        => _options.MaximumArtifactBytes;

    internal void ObserveIoStageLocked(
        ServerOwnedContinuousLearningAdapterIoObservation observation)
        => _options.ObserveIoStageForTesting?.Invoke(observation);

    internal void SerializeAdapterLocked(LlmModel model, Stream stream)
    {
        if (_options.SerializeAdapterForTesting is { } serializer)
        {
            serializer(model, stream);
            return;
        }

        model.SaveAdapterCheckpoint(stream);
    }

    internal Stream WrapSerializerStreamLocked(Stream stream)
        => _options.WrapSerializerStreamForTesting?.Invoke(stream) ?? stream;

    internal Action? GetBoundedWriterDisposedCallbackLocked()
        => _options.BoundedWriterDisposedForTesting;

    internal void ThrowIfRootDirectoryFlushFailureForTesting()
    {
        if (_options.FailRootDirectoryFlushForTesting && !_rootDirectoryFlushFailureInjected)
        {
            _rootDirectoryFlushFailureInjected = true;
            throw new IOException("TEST_ADAPTER_ROOT_DIRECTORY_FLUSH_FAILURE");
        }
    }

    internal void DisposeLocked()
    {
        _disposed = true;
        _revoked = true;
        _ownerLockHandle.Dispose();
    }

    internal void RevokeLocked()
    {
        _revoked = true;
        _ownerLockHandle.Dispose();
    }
}

internal sealed partial class ServerOwnedContinuousLearningFileAccess
{
    private bool _adapterRootCapabilityAcquisitionConsumed;
    private ServerOwnedContinuousLearningAdapterRootCapability?
        _adapterRootCapability;

    internal ServerOwnedContinuousLearningAdapterRootCapability
        AcquireAdapterRootCapability(
            ServerOwnedContinuousLearningAdapterRootCapabilityOptions? options = null)
    {
        Action? callback = null;
        ExceptionDispatchInfo? acquisitionFailure = null;
        ServerOwnedContinuousLearningAdapterRootCapability? capability = null;
        var trustFailed = false;
        options ??= new ServerOwnedContinuousLearningAdapterRootCapabilityOptions();
        if (options.MaximumArtifactBytes is <= 0 or >
            ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(options));
        }

        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            if (_adapterRootCapabilityAcquisitionConsumed)
                throw new InvalidOperationException("The AdapterRoot capability is one-shot.");

            var root = FindRetainedRoot(ContinuousLearningRetainedRootRole.Adapter);
            if (!TryValidateRetainedEntry(root))
            {
                _adapterRootCapabilityAcquisitionConsumed = true;
                callback = LoseRetainedObjectTrustLocked();
                trustFailed = true;
            }
            else
            {
                SafeFileHandle? ownerLockHandle = null;
                try
                {
                    ownerLockHandle = ServerOwnedContinuousLearningCacheRootNative
                        .AcquireNamedOwnerLock(
                            root.Handle,
                            ".continuous-learning.adapter-owner.lock");
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    acquisitionFailure = ExceptionDispatchInfo.Capture(exception);
                }

                if (acquisitionFailure is not null)
                {
                    ownerLockHandle?.Dispose();
                }

                if (!TryValidateRetainedEntry(root))
                {
                    ownerLockHandle?.Dispose();
                    _adapterRootCapabilityAcquisitionConsumed = true;
                    callback = LoseRetainedObjectTrustLocked();
                    trustFailed = true;
                }
                else if (acquisitionFailure is null)
                {
                    _adapterRootCapabilityAcquisitionConsumed = true;
                    capability = new ServerOwnedContinuousLearningAdapterRootCapability(
                        this,
                        ownerLockHandle!,
                        options);
                    _adapterRootCapability = capability;
                }
            }
        }

        InvokeRetainedObjectTrustLost(callback);
        if (trustFailed)
            throw new ContinuousLearningPathIdentityException();
        acquisitionFailure?.Throw();
        return capability!;
    }

    internal void DisposeAdapterRootCapability(
        ServerOwnedContinuousLearningAdapterRootCapability capability)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_adapterRootCapability, capability))
                capability.DisposeLocked();
        }
    }

    internal ServerOwnedContinuousLearningAdapterPublication PublishAdapterCheckpoint(
        ServerOwnedContinuousLearningAdapterRootCapability capability,
        LlmModel model,
        ServerOwnedContinuousLearningAdapterPublicationRequest request,
        CancellationToken cancellationToken)
    {
        Action? callback = null;
        ExceptionDispatchInfo? operationFailure = null;
        ServerOwnedContinuousLearningAdapterPublication result = default;
        var trustFailed = false;

        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            if (!ReferenceEquals(_adapterRootCapability, capability))
                throw new ContinuousLearningPathIdentityException();
            capability.ThrowIfUnavailableLocked();

            var root = FindRetainedRoot(ContinuousLearningRetainedRootRole.Adapter);
            if (!TryValidateRetainedEntry(root))
            {
                callback = LoseRetainedObjectTrustLocked();
                trustFailed = true;
            }
            else
            {
                try
                {
                    result = ServerOwnedContinuousLearningAdapterRootNative.Publish(
                        root.Handle,
                        new ServerOwnedContinuousLearningAdapterRootIdentity(
                            root.Identity.VolumeSerialNumber,
                            root.Identity.FileIdLow,
                            root.Identity.FileIdHigh),
                        model,
                        request,
                        capability,
                        () => TryValidateRetainedEntry(root),
                        cancellationToken);
                }
                catch (AdapterPublicationCommitIndeterminateException exception)
                {
                    // Rename already happened. Preserve its immutable evidence
                    // even if the retained-root lease is revoked below.
                    operationFailure = ExceptionDispatchInfo.Capture(exception);
                }
                catch (AdapterPublicationTrustException exception)
                {
                    operationFailure = ExceptionDispatchInfo.Capture(exception);
                    callback = LoseRetainedObjectTrustLocked();
                    trustFailed = true;
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    operationFailure = ExceptionDispatchInfo.Capture(exception);
                }

                if (!TryValidateRetainedEntry(root))
                {
                    callback = LoseRetainedObjectTrustLocked();
                    trustFailed = true;
                }
            }
        }

        InvokeRetainedObjectTrustLost(callback);
        if (trustFailed &&
            operationFailure?.SourceException is AdapterPublicationCommitIndeterminateException)
        {
            operationFailure.Throw();
        }
        if (trustFailed)
            throw new ContinuousLearningPathIdentityException();
        operationFailure?.Throw();
        return result;
    }

    internal ServerOwnedContinuousLearningAdapterPublication ReconcileCommittedAdapterCheckpoint(
        ServerOwnedContinuousLearningAdapterRootCapability capability,
        LlmModel model,
        ServerOwnedContinuousLearningAdapterPublication receipt,
        CancellationToken cancellationToken)
    {
        ValidateReconciliationReceipt(receipt);

        Action? callback = null;
        ExceptionDispatchInfo? operationFailure = null;
        ServerOwnedContinuousLearningAdapterPublication result = default;
        var trustFailed = false;

        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            if (!ReferenceEquals(_adapterRootCapability, capability))
                throw new ContinuousLearningPathIdentityException();
            capability.ThrowIfUnavailableLocked();

            var root = FindRetainedRoot(ContinuousLearningRetainedRootRole.Adapter);
            var currentRootIdentity = new ServerOwnedContinuousLearningAdapterRootIdentity(
                root.Identity.VolumeSerialNumber,
                root.Identity.FileIdLow,
                root.Identity.FileIdHigh);
            if (receipt.RetainedRootIdentity != currentRootIdentity)
            {
                throw new ArgumentException(
                    "The committed receipt belongs to a different AdapterRoot.",
                    nameof(receipt));
            }

            if (!TryValidateRetainedEntry(root))
            {
                callback = LoseRetainedObjectTrustLocked();
                trustFailed = true;
            }
            else
            {
                try
                {
                    result = ServerOwnedContinuousLearningAdapterRootNative.ReconcileCommitted(
                        root.Handle,
                        currentRootIdentity,
                        model,
                        receipt,
                        capability,
                        () => TryValidateRetainedEntry(root),
                        cancellationToken);
                }
                catch (AdapterPublicationTrustException exception)
                {
                    operationFailure = ExceptionDispatchInfo.Capture(exception);
                    callback = LoseRetainedObjectTrustLocked();
                    trustFailed = true;
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    operationFailure = ExceptionDispatchInfo.Capture(exception);
                }

                if (!TryValidateRetainedEntry(root))
                {
                    callback = LoseRetainedObjectTrustLocked();
                    trustFailed = true;
                }
            }
        }

        InvokeRetainedObjectTrustLost(callback);
        if (trustFailed)
            throw new ContinuousLearningPathIdentityException();
        operationFailure?.Throw();
        return result;
    }

    internal ServerOwnedContinuousLearningAdapterSnapshot ReadExactAdapterCheckpoint(
        ServerOwnedContinuousLearningAdapterRootCapability capability,
        ServerOwnedContinuousLearningAdapterPublication receipt,
        CancellationToken cancellationToken)
    {
        ValidatePublicationReceipt(receipt);
        if (receipt.Length > capability.GetMaximumArtifactBytesLocked())
        {
            throw new ArgumentException(
                "The adapter receipt exceeds this capability's exact-read bound.",
                nameof(receipt));
        }

        Action? callback = null;
        ExceptionDispatchInfo? operationFailure = null;
        ServerOwnedContinuousLearningAdapterSnapshot? result = null;
        var trustFailed = false;

        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            if (!ReferenceEquals(_adapterRootCapability, capability))
                throw new ContinuousLearningPathIdentityException();
            capability.ThrowIfUnavailableLocked();

            var root = FindRetainedRoot(ContinuousLearningRetainedRootRole.Adapter);
            var currentRootIdentity = new ServerOwnedContinuousLearningAdapterRootIdentity(
                root.Identity.VolumeSerialNumber,
                root.Identity.FileIdLow,
                root.Identity.FileIdHigh);
            if (receipt.RetainedRootIdentity != currentRootIdentity)
            {
                throw new ArgumentException(
                    "The adapter receipt belongs to a different AdapterRoot.",
                    nameof(receipt));
            }

            if (!TryValidateRetainedEntry(root))
            {
                callback = LoseRetainedObjectTrustLocked();
                trustFailed = true;
            }
            else
            {
                try
                {
                    result = ServerOwnedContinuousLearningAdapterRootNative.ReadExact(
                        root.Handle,
                        receipt,
                        capability,
                        () => TryValidateRetainedEntry(root),
                        cancellationToken);
                }
                catch (AdapterPublicationTrustException exception)
                {
                    operationFailure = ExceptionDispatchInfo.Capture(exception);
                    callback = LoseRetainedObjectTrustLocked();
                    trustFailed = true;
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    operationFailure = ExceptionDispatchInfo.Capture(exception);
                }

                if (!TryValidateRetainedEntry(root))
                {
                    callback = LoseRetainedObjectTrustLocked();
                    trustFailed = true;
                }
            }
        }

        InvokeRetainedObjectTrustLost(callback);
        if (trustFailed)
            throw new ContinuousLearningPathIdentityException();
        operationFailure?.Throw();
        return result!;
    }

    private static void ValidateReconciliationReceipt(
        ServerOwnedContinuousLearningAdapterPublication receipt)
        => ValidatePublicationReceipt(receipt);

    private static void ValidatePublicationReceipt(
        ServerOwnedContinuousLearningAdapterPublication receipt)
    {
        if (!ServerOwnedContinuousLearningAdapterPublicationRequest
                .IsCanonicalFinalLeafName(receipt.CanonicalLeafName) ||
            receipt.Length <= 0 ||
            receipt.Length > ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes ||
            !IsValidIdentity(receipt.RetainedRootIdentity.VolumeSerialNumber,
                receipt.RetainedRootIdentity.FileIdLow,
                receipt.RetainedRootIdentity.FileIdHigh) ||
            !IsValidIdentity(receipt.FinalArtifactIdentity.VolumeSerialNumber,
                receipt.FinalArtifactIdentity.FileIdLow,
                receipt.FinalArtifactIdentity.FileIdHigh) ||
            !IsCanonicalSha256(receipt.Sha256Hex))
        {
            throw new ArgumentException("The committed receipt is malformed.", nameof(receipt));
        }
    }

    private static bool IsValidIdentity(ulong volumeSerialNumber, ulong fileIdLow, ulong fileIdHigh)
        => volumeSerialNumber != 0 && (fileIdLow != 0 || fileIdHigh != 0);

    private static bool IsCanonicalSha256(string? value)
        => value is { Length: 64 } &&
           value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');
}

internal sealed class AdapterPublicationTrustException : IOException
{
    internal AdapterPublicationTrustException(string message)
        : base(message)
    {
    }

    internal AdapterPublicationTrustException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// The no-replace rename has completed, but a subsequent durability or final
/// verification operation failed. This is deliberately not a success result:
/// callers receive the immutable last verified artifact descriptor and must
/// reconcile that named artifact rather than blindly retrying the same ID.
/// </summary>
internal sealed class AdapterPublicationCommitIndeterminateException : IOException
{
    internal AdapterPublicationCommitIndeterminateException(
        ServerOwnedContinuousLearningAdapterPublication committedArtifact,
        Exception innerException)
        : base("ADAPTER_PUBLICATION_COMMITTED_BUT_NOT_DURABLY_VERIFIED", innerException)
    {
        CommittedArtifact = committedArtifact;
    }

    internal ServerOwnedContinuousLearningAdapterPublication CommittedArtifact { get; }
}

internal static class ServerOwnedContinuousLearningAdapterRootNative
{
    internal static ServerOwnedContinuousLearningAdapterPublication Publish(
        SafeFileHandle root,
        ServerOwnedContinuousLearningAdapterRootIdentity rootIdentity,
        LlmModel model,
        ServerOwnedContinuousLearningAdapterPublicationRequest request,
        ServerOwnedContinuousLearningAdapterRootCapability capability,
        Func<bool> rootStillTrusted,
        CancellationToken cancellationToken)
    {
        if (OperatingSystem.IsLinux())
        {
            return PublishCreateOnlyPosix(
                root,
                rootIdentity,
                model,
                request,
                capability,
                rootStillTrusted,
                cancellationToken);
        }

        var maximumBytes = capability.GetMaximumArtifactBytesLocked();
        var finalLeafName = request.GetCanonicalFinalLeafName();
        var temporaryLeafName = $".{finalLeafName}.{Guid.NewGuid():N}.tmp";
        ServerOwnedContinuousLearningCacheFileIdentity temporaryIdentity = default;
        AdapterWriteReceipt writeReceipt = default;
        ServerOwnedContinuousLearningAdapterPublication committedArtifact = default;
        var commitEstablished = false;
        var published = false;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var writeHandle = ServerOwnedContinuousLearningCacheRootNative
                       .CreateNamedWriteLeaf(root, temporaryLeafName, maximumBytes))
            {
                temporaryIdentity = ServerOwnedContinuousLearningCacheRootNative
                    .DescribeNamedRegularLeaf(writeHandle, maximumBytes, out _);
                capability.ObserveIoStageLocked(new(
                    ServerOwnedContinuousLearningAdapterIoStage.AfterTemporaryLeafCreated,
                    temporaryLeafName));
                using (var stream = CreateNonOwningWriteStream(writeHandle))
                {
                    var serializerStream = capability.WrapSerializerStreamLocked(stream);
                    using var boundedWriter = new BoundedWriteStream(
                        serializerStream,
                        maximumBytes,
                        cancellationToken,
                        capability.GetBoundedWriterDisposedCallbackLocked());
                    capability.SerializeAdapterLocked(model, boundedWriter);
                    cancellationToken.ThrowIfCancellationRequested();
                    stream.Flush(flushToDisk: true);
                    cancellationToken.ThrowIfCancellationRequested();
                    writeReceipt = boundedWriter.Complete();
                }
            }

            capability.ObserveIoStageLocked(new(
                ServerOwnedContinuousLearningAdapterIoStage.AfterSerializationFlush,
                temporaryLeafName));
            cancellationToken.ThrowIfCancellationRequested();
            if (!rootStillTrusted())
                throw new AdapterPublicationTrustException("AdapterRoot identity changed before readback.");

            capability.ObserveIoStageLocked(new(
                ServerOwnedContinuousLearningAdapterIoStage.BeforeReadbackValidation,
                temporaryLeafName));
            using (var source = ServerOwnedContinuousLearningCacheRootNative
                       .OpenNamedRegularLeaf(root, temporaryLeafName, mutationAccess: true, maximumBytes))
            {
                var observedIdentity = ServerOwnedContinuousLearningCacheRootNative
                    .DescribeNamedRegularLeaf(source, maximumBytes, out var observedLength);
                if (observedIdentity != temporaryIdentity || observedLength != writeReceipt.Length)
                {
                    throw new AdapterPublicationTrustException(
                        "Adapter temporary leaf identity changed before commit.");
                }

                AdapterReadback readback;
                using (var sourceStream = CreateNonOwningReadStream(source))
                {
                    readback = ReadbackAndHash(
                        sourceStream,
                        source,
                        temporaryIdentity,
                        maximumBytes,
                        model,
                        cancellationToken);
                }
                if (!readback.Matches(writeReceipt))
                {
                    throw new AdapterPublicationTrustException(
                        "Adapter bytes changed after serialization and before commit.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (!rootStillTrusted())
                    throw new AdapterPublicationTrustException("AdapterRoot identity changed before commit.");

                capability.ObserveIoStageLocked(new(
                    ServerOwnedContinuousLearningAdapterIoStage.BeforeCommit,
                    temporaryLeafName));
                cancellationToken.ThrowIfCancellationRequested();
                if (!rootStillTrusted())
                    throw new AdapterPublicationTrustException("AdapterRoot identity changed at commit.");

                // This is the irreversible commit point. From the successful
                // no-replace rename onward caller cancellation is intentionally
                // ignored: the method must finish verification/flush and either
                // return a durable result or report the committed artifact.
                ServerOwnedContinuousLearningCacheRootNative.RenameNamedLeafNoReplace(
                    root,
                    source,
                    temporaryLeafName,
                    finalLeafName);
                commitEstablished = true;
                published = true;
                committedArtifact = CreatePublication(
                    finalLeafName,
                    rootIdentity,
                    observedIdentity,
                    readback);

                var renamedIdentity = ServerOwnedContinuousLearningCacheRootNative
                    .DescribeNamedRegularLeaf(source, maximumBytes, out var renamedLength);
                if (renamedIdentity != temporaryIdentity || renamedLength != readback.Length)
                {
                    throw new AdapterPublicationTrustException(
                        "Adapter final leaf identity changed during commit.");
                }

                capability.ObserveIoStageLocked(new(
                    ServerOwnedContinuousLearningAdapterIoStage.AfterCommit,
                    CanonicalTemporaryLeafName: null));
                if (!rootStillTrusted())
                    throw new AdapterPublicationTrustException("AdapterRoot identity changed after commit.");

                // Keep the renamed source handle open with FileShare.Read over
                // final validation, directory flush and final recheck. It is a
                // handle to the final object, not another path lookup.
                AdapterReadback finalReadback;
                using (var finalStream = CreateNonOwningReadStream(source))
                {
                    finalReadback = ReadbackAndHash(
                        finalStream,
                        source,
                        temporaryIdentity,
                        maximumBytes,
                        model,
                        CancellationToken.None);
                }
                if (!finalReadback.Matches(readback))
                {
                    throw new AdapterPublicationTrustException(
                        "Adapter final leaf changed after commit.");
                }

                capability.ObserveIoStageLocked(new(
                    ServerOwnedContinuousLearningAdapterIoStage.BeforeRootDirectoryFlush,
                    CanonicalTemporaryLeafName: null));
                capability.ThrowIfRootDirectoryFlushFailureForTesting();
                ServerOwnedContinuousLearningCacheRootNative.FlushNamedRootDirectory(root);
                capability.ObserveIoStageLocked(new(
                    ServerOwnedContinuousLearningAdapterIoStage.AfterRootDirectoryFlush,
                    CanonicalTemporaryLeafName: null));
                if (!rootStillTrusted())
                    throw new AdapterPublicationTrustException("AdapterRoot identity changed after commit flush.");

                AdapterReadback finalRecheck;
                using (var finalStream = CreateNonOwningReadStream(source))
                {
                    finalRecheck = ReadbackAndHash(
                        finalStream,
                        source,
                        temporaryIdentity,
                        maximumBytes,
                        model,
                        CancellationToken.None);
                }
                if (!finalRecheck.Matches(finalReadback))
                {
                    throw new AdapterPublicationTrustException(
                        "Adapter final leaf changed after durable root flush.");
                }

                return CreatePublication(
                    finalLeafName,
                    rootIdentity,
                    renamedIdentity,
                    finalRecheck);
            }
        }
        catch (AdapterPublicationCommitIndeterminateException)
        {
            throw;
        }
        catch (Exception exception)
            when (commitEstablished &&
                  AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                      .IsNonFatal(exception))
        {
            throw new AdapterPublicationCommitIndeterminateException(
                committedArtifact,
                exception);
        }
        finally
        {
            if (!published && IsValidIdentity(temporaryIdentity))
                TryDeleteOnlyOwnTemporaryLeaf(
                    root,
                    temporaryLeafName,
                    temporaryIdentity,
                    maximumBytes,
                    rootStillTrusted);
        }
    }

    private static ServerOwnedContinuousLearningAdapterPublication
        PublishCreateOnlyPosix(
            SafeFileHandle root,
            ServerOwnedContinuousLearningAdapterRootIdentity rootIdentity,
            LlmModel model,
            ServerOwnedContinuousLearningAdapterPublicationRequest request,
            ServerOwnedContinuousLearningAdapterRootCapability capability,
            Func<bool> rootStillTrusted,
            CancellationToken cancellationToken)
    {
        var maximumBytes = capability.GetMaximumArtifactBytesLocked();
        var finalLeafName = request.GetCanonicalFinalLeafName();
        var commitEvidenceReady = false;
        ServerOwnedContinuousLearningAdapterPublication committedArtifact = default;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!rootStillTrusted())
            {
                throw new AdapterPublicationTrustException(
                    "AdapterRoot identity changed before POSIX create-only commit.");
            }

            using var final = ServerOwnedContinuousLearningCacheRootNative
                .CreateNamedWriteLeaf(root, finalLeafName, maximumBytes);
            var finalIdentity = ServerOwnedContinuousLearningCacheRootNative
                .DescribeNamedRegularLeaf(final, maximumBytes, out var initialLength);
            if (initialLength != 0)
            {
                throw new AdapterPublicationTrustException(
                    "Adapter POSIX create-only final leaf was not born empty.");
            }

            AdapterWriteReceipt writeReceipt;
            using (var stream = CreateNonOwningWriteStream(final))
            {
                // POSIX create-only publication показывает final-имя до полного
                // успеха, но доверенный receipt появляется только после
                // serializer, hash, length и root checks ниже.
                var serializerStream = capability.WrapSerializerStreamLocked(stream);
                using var boundedWriter = new BoundedWriteStream(
                    serializerStream,
                    maximumBytes,
                    cancellationToken,
                    capability.GetBoundedWriterDisposedCallbackLocked());
                capability.SerializeAdapterLocked(model, boundedWriter);
                cancellationToken.ThrowIfCancellationRequested();
                stream.Flush(flushToDisk: true);
                writeReceipt = boundedWriter.Complete();
            }

            ServerOwnedContinuousLearningCacheRootNative.FlushNamedRegularLeaf(final);
            capability.ObserveIoStageLocked(new(
                ServerOwnedContinuousLearningAdapterIoStage.AfterSerializationFlush,
                CanonicalTemporaryLeafName: null));
            if (!rootStillTrusted())
            {
                throw new AdapterPublicationTrustException(
                    "AdapterRoot identity changed after POSIX serialization.");
            }

            capability.ObserveIoStageLocked(new(
                ServerOwnedContinuousLearningAdapterIoStage.BeforeReadbackValidation,
                CanonicalTemporaryLeafName: null));
            var observedIdentity = ServerOwnedContinuousLearningCacheRootNative
                .DescribeNamedRegularLeaf(final, maximumBytes, out var observedLength);
            if (observedIdentity != finalIdentity ||
                observedLength != writeReceipt.Length)
            {
                throw new AdapterPublicationTrustException(
                    "Adapter POSIX final leaf changed before readback.");
            }

            AdapterReadback readback;
            using (var finalStream = CreateNonOwningReadStream(final))
            {
                readback = ReadbackAndHash(
                    finalStream,
                    final,
                    finalIdentity,
                    maximumBytes,
                    model,
                    cancellationToken);
            }
            if (!readback.Matches(writeReceipt))
            {
                throw new AdapterPublicationTrustException(
                    "Adapter POSIX final bytes changed after serialization.");
            }

            committedArtifact = CreatePublication(
                finalLeafName,
                rootIdentity,
                finalIdentity,
                readback);
            commitEvidenceReady = true;

            capability.ObserveIoStageLocked(new(
                ServerOwnedContinuousLearningAdapterIoStage.AfterCommit,
                CanonicalTemporaryLeafName: null));
            capability.ObserveIoStageLocked(new(
                ServerOwnedContinuousLearningAdapterIoStage.BeforeRootDirectoryFlush,
                CanonicalTemporaryLeafName: null));
            capability.ThrowIfRootDirectoryFlushFailureForTesting();
            ServerOwnedContinuousLearningCacheRootNative.FlushNamedRootDirectory(root);
            capability.ObserveIoStageLocked(new(
                ServerOwnedContinuousLearningAdapterIoStage.AfterRootDirectoryFlush,
                CanonicalTemporaryLeafName: null));
            if (!rootStillTrusted())
            {
                throw new AdapterPublicationTrustException(
                    "AdapterRoot identity changed after POSIX root flush.");
            }

            AdapterReadback finalRecheck;
            using (var finalStream = CreateNonOwningReadStream(final))
            {
                finalRecheck = ReadbackAndHash(
                    finalStream,
                    final,
                    finalIdentity,
                    maximumBytes,
                    model,
                    CancellationToken.None);
            }
            if (!finalRecheck.Matches(readback))
            {
                throw new AdapterPublicationTrustException(
                    "Adapter POSIX final leaf changed after durable root flush.");
            }

            return CreatePublication(
                finalLeafName,
                rootIdentity,
                finalIdentity,
                finalRecheck);
        }
        catch (AdapterPublicationCommitIndeterminateException)
        {
            throw;
        }
        catch (Exception exception)
            when (commitEvidenceReady &&
                  AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                      .IsNonFatal(exception))
        {
            throw new AdapterPublicationCommitIndeterminateException(
                committedArtifact,
                exception);
        }
    }

    internal static ServerOwnedContinuousLearningAdapterPublication ReconcileCommitted(
        SafeFileHandle root,
        ServerOwnedContinuousLearningAdapterRootIdentity rootIdentity,
        LlmModel model,
        ServerOwnedContinuousLearningAdapterPublication receipt,
        ServerOwnedContinuousLearningAdapterRootCapability capability,
        Func<bool> rootStillTrusted,
        CancellationToken cancellationToken)
    {
        // Reconciliation has one intentional side effect: retrying the root
        // directory flush that failed after an already-successful rename.  A
        // caller may cancel safely before that point. Once the flush begins,
        // finish validation so cancellation cannot obscure the final state.
        cancellationToken.ThrowIfCancellationRequested();
        if (!rootStillTrusted())
            throw new AdapterPublicationTrustException("AdapterRoot identity changed before reconciliation.");

        SafeFileHandle? final = null;
        ServerOwnedContinuousLearningCacheFileIdentity initialIdentity;
        long initialLength;
        try
        {
            final = ServerOwnedContinuousLearningCacheRootNative
                .OpenNamedRegularLeaf(
                    root,
                    receipt.CanonicalLeafName,
                    mutationAccess: false,
                    capability.GetMaximumArtifactBytesLocked());
            initialIdentity = ServerOwnedContinuousLearningCacheRootNative
                .DescribeNamedRegularLeaf(
                    final,
                    capability.GetMaximumArtifactBytesLocked(),
                    out initialLength);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException &&
                  AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                      .IsNonFatal(exception))
        {
            final?.Dispose();
            throw new AdapterPublicationTrustException(
                "Adapter final leaf cannot be trusted for reconciliation.",
                exception);
        }

        using (final)
        {
        if (ToArtifactIdentity(initialIdentity) != receipt.FinalArtifactIdentity ||
            initialLength != receipt.Length)
        {
            throw new AdapterPublicationTrustException(
                "Adapter final leaf does not match the committed receipt.");
        }

        if (!rootStillTrusted())
            throw new AdapterPublicationTrustException("AdapterRoot identity changed before reconciliation readback.");

        AdapterReadback initialReadback;
        try
        {
            using var finalStream = CreateNonOwningReadStream(final);
            initialReadback = ReadbackAndHash(
                finalStream,
                final,
                initialIdentity,
                capability.GetMaximumArtifactBytesLocked(),
                model,
                cancellationToken);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException &&
                  AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                      .IsNonFatal(exception))
        {
            throw new AdapterPublicationTrustException(
                "Adapter final content cannot be trusted for reconciliation.",
                exception);
        }
        if (!initialReadback.Matches(receipt))
        {
            throw new AdapterPublicationTrustException(
                "Adapter final content does not match the committed receipt.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!rootStillTrusted())
            throw new AdapterPublicationTrustException("AdapterRoot identity changed before reconciliation flush.");

        // The retained handle still denies write sharing at this point. Do not
        // perform another path-based open while reconciliation is in progress.
        capability.ObserveIoStageLocked(new(
            ServerOwnedContinuousLearningAdapterIoStage.BeforeReconciliationRootDirectoryFlush,
            CanonicalTemporaryLeafName: null));
        capability.ThrowIfRootDirectoryFlushFailureForTesting();
        ServerOwnedContinuousLearningCacheRootNative.FlushNamedRootDirectory(root);
        capability.ObserveIoStageLocked(new(
            ServerOwnedContinuousLearningAdapterIoStage.AfterReconciliationRootDirectoryFlush,
            CanonicalTemporaryLeafName: null));
        if (!rootStillTrusted())
            throw new AdapterPublicationTrustException("AdapterRoot identity changed after reconciliation flush.");

        AdapterReadback finalReadback;
        try
        {
            using var finalStream = CreateNonOwningReadStream(final);
            finalReadback = ReadbackAndHash(
                finalStream,
                final,
                initialIdentity,
                capability.GetMaximumArtifactBytesLocked(),
                model,
                CancellationToken.None);
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            throw new AdapterPublicationTrustException(
                "Adapter final content changed during reconciliation verification.",
                exception);
        }
        if (!finalReadback.Matches(initialReadback) || !finalReadback.Matches(receipt))
        {
            throw new AdapterPublicationTrustException(
                "Adapter final leaf changed during reconciliation.");
        }

        if (!rootStillTrusted())
            throw new AdapterPublicationTrustException("AdapterRoot identity changed after reconciliation.");

        return CreatePublication(
            receipt.CanonicalLeafName,
            rootIdentity,
            initialIdentity,
            finalReadback);
        }
    }

    internal static ServerOwnedContinuousLearningAdapterSnapshot ReadExact(
        SafeFileHandle root,
        ServerOwnedContinuousLearningAdapterPublication receipt,
        ServerOwnedContinuousLearningAdapterRootCapability capability,
        Func<bool> rootStillTrusted,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!rootStillTrusted())
            throw new AdapterPublicationTrustException("AdapterRoot identity changed before exact read.");

        capability.ObserveIoStageLocked(new(
            ServerOwnedContinuousLearningAdapterIoStage.BeforeExactReadOpen,
            CanonicalTemporaryLeafName: null));
        cancellationToken.ThrowIfCancellationRequested();
        if (!rootStillTrusted())
            throw new AdapterPublicationTrustException("AdapterRoot identity changed before exact-read open.");

        SafeFileHandle? final = null;
        try
        {
            final = ServerOwnedContinuousLearningCacheRootNative.OpenNamedRegularLeaf(
                root,
                receipt.CanonicalLeafName,
                mutationAccess: false,
                capability.GetMaximumArtifactBytesLocked());
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException &&
                  AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                      .IsNonFatal(exception))
        {
            final?.Dispose();
            throw new AdapterPublicationTrustException(
                "The published adapter leaf cannot be trusted for exact read.",
                exception);
        }

        using (final)
        {
            try
            {
                var initialIdentity = ServerOwnedContinuousLearningCacheRootNative
                    .DescribeNamedRegularLeaf(
                        final,
                        capability.GetMaximumArtifactBytesLocked(),
                        out var initialLength);
                if (ToArtifactIdentity(initialIdentity) != receipt.FinalArtifactIdentity ||
                    initialLength != receipt.Length)
                {
                    throw new AdapterPublicationTrustException(
                        "The published adapter leaf does not match its exact-read receipt.");
                }

                capability.ObserveIoStageLocked(new(
                    ServerOwnedContinuousLearningAdapterIoStage.AfterExactReadOpen,
                    CanonicalTemporaryLeafName: null));
                cancellationToken.ThrowIfCancellationRequested();
                if (!rootStillTrusted())
                {
                    throw new AdapterPublicationTrustException(
                        "AdapterRoot identity changed after exact-read open.");
                }

                using var stream = CreateNonOwningReadStream(final);
                using (var cancellableValidationStream =
                       new TrustAndCancellationAwareReadStream(
                           stream,
                           rootStillTrusted,
                           cancellationToken))
                {
                    LlmModel.ValidateAdapterCheckpointArtifact(cancellableValidationStream);
                    if (cancellableValidationStream.Position != cancellableValidationStream.Length)
                    {
                        throw new InvalidDataException(
                            "Adapter checkpoint validation did not consume exact EOF.");
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                if (!rootStillTrusted())
                {
                    throw new AdapterPublicationTrustException(
                        "AdapterRoot identity changed after exact-read validation.");
                }

                stream.Position = 0;
                var bytes = new byte[checked((int)receipt.Length)];
                using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                var offset = 0;
                while (offset < bytes.Length)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!rootStillTrusted())
                    {
                        throw new AdapterPublicationTrustException(
                            "AdapterRoot identity changed during exact read.");
                    }

                    var read = stream.Read(
                        bytes,
                        offset,
                        Math.Min(64 * 1024, bytes.Length - offset));
                    if (read == 0)
                        throw new EndOfStreamException("The published adapter was truncated during exact read.");
                    hash.AppendData(bytes, offset, read);
                    offset = checked(offset + read);
                    capability.ObserveIoStageLocked(new(
                        ServerOwnedContinuousLearningAdapterIoStage.DuringExactRead,
                        CanonicalTemporaryLeafName: null));
                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (stream.ReadByte() != -1)
                    throw new InvalidDataException("The published adapter grew during exact read.");

                var observedHash = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
                var finalIdentity = ServerOwnedContinuousLearningCacheRootNative
                    .DescribeNamedRegularLeaf(
                        final,
                        capability.GetMaximumArtifactBytesLocked(),
                        out var finalLength);
                capability.ObserveIoStageLocked(new(
                    ServerOwnedContinuousLearningAdapterIoStage.AfterExactReadValidation,
                    CanonicalTemporaryLeafName: null));
                cancellationToken.ThrowIfCancellationRequested();
                if (!rootStillTrusted())
                {
                    throw new AdapterPublicationTrustException(
                        "AdapterRoot identity changed after exact read.");
                }

                if (finalIdentity != initialIdentity ||
                    finalLength != initialLength ||
                    !string.Equals(observedHash, receipt.Sha256Hex, StringComparison.Ordinal))
                {
                    throw new AdapterPublicationTrustException(
                        "The published adapter changed or failed exact hash verification.");
                }

                var snapshot = new ServerOwnedContinuousLearningAdapterSnapshot(receipt, bytes);
                cancellationToken.ThrowIfCancellationRequested();
                if (!rootStillTrusted())
                {
                    throw new AdapterPublicationTrustException(
                        "AdapterRoot identity changed while finalizing exact-read evidence.");
                }
                return snapshot;
            }
            catch (AdapterPublicationTrustException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
                when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                    .IsNonFatal(exception))
            {
                throw new AdapterPublicationTrustException(
                    "The published adapter content cannot be trusted for exact read.",
                    exception);
            }
        }
    }

    private static AdapterReadback ReadbackAndHash(
        FileStream stream,
        SafeFileHandle handle,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes,
        LlmModel model,
        CancellationToken cancellationToken)
    {
        var observedIdentity = ServerOwnedContinuousLearningCacheRootNative
            .DescribeNamedRegularLeaf(handle, maximumBytes, out var expectedLength);
        if (observedIdentity != expectedIdentity || expectedLength <= 0)
        {
            throw new AdapterPublicationTrustException(
                "Adapter temporary leaf identity changed before readback.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        stream.Position = 0;
        model.ValidateCurrentAdapterCheckpointArtifact(stream);
        if (stream.Position != stream.Length)
        {
            throw new InvalidDataException(
                "Adapter checkpoint validation did not consume exact EOF.");
        }

        stream.Position = 0;
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[64 * 1024];
        long total = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = stream.Read(buffer, 0, buffer.Length);
            if (read == 0)
                break;
            total = checked(total + read);
            if (total > maximumBytes)
                throw new InvalidDataException("Adapter checkpoint exceeded its publication bound.");
            hash.AppendData(buffer, 0, read);
        }

        if (total != expectedLength)
            throw new InvalidDataException("Adapter checkpoint changed during readback.");
        var postReadIdentity = ServerOwnedContinuousLearningCacheRootNative
            .DescribeNamedRegularLeaf(handle, maximumBytes, out var postReadLength);
        if (postReadIdentity != expectedIdentity || postReadLength != expectedLength)
        {
            throw new AdapterPublicationTrustException(
                "Adapter checkpoint identity changed during readback.");
        }
        return new AdapterReadback(
            ToArtifactIdentity(postReadIdentity),
            total,
            Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
    }

    private static FileStream CreateNonOwningReadStream(SafeFileHandle handle)
    {
        var borrowedHandle = new SafeFileHandle(handle.DangerousGetHandle(), ownsHandle: false);
        return new FileStream(
            borrowedHandle,
            FileAccess.Read,
            bufferSize: 64 * 1024,
            isAsync: false);
    }

    private static FileStream CreateNonOwningWriteStream(SafeFileHandle handle)
    {
        // Readback и directory flush ещё нуждаются в retained leaf fd после
        // serializer stream; borrowed wrapper не даёт dispose случайно закрыть
        // этот доказательный handle.
        var borrowedHandle = new SafeFileHandle(handle.DangerousGetHandle(), ownsHandle: false);
        return new FileStream(
            borrowedHandle,
            FileAccess.Write,
            bufferSize: 64 * 1024,
            isAsync: false);
    }

    private static ServerOwnedContinuousLearningAdapterPublication CreatePublication(
        string finalLeafName,
        ServerOwnedContinuousLearningAdapterRootIdentity rootIdentity,
        ServerOwnedContinuousLearningCacheFileIdentity artifactIdentity,
        AdapterReadback content)
    {
        var immutableIdentity = ToArtifactIdentity(artifactIdentity);
        if (content.FinalArtifactIdentity != immutableIdentity)
        {
            throw new AdapterPublicationTrustException(
                "Adapter content was not read from the expected final artifact.");
        }

        return new ServerOwnedContinuousLearningAdapterPublication(
            finalLeafName,
            rootIdentity,
            immutableIdentity,
            content.Length,
            content.Sha256Hex);
    }

    private static ServerOwnedContinuousLearningAdapterArtifactIdentity ToArtifactIdentity(
        ServerOwnedContinuousLearningCacheFileIdentity identity)
        => new(identity.VolumeSerialNumber, identity.FileIdLow, identity.FileIdHigh);

    private static void TryDeleteOnlyOwnTemporaryLeaf(
        SafeFileHandle root,
        string temporaryLeafName,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes,
        Func<bool> rootStillTrusted)
    {
        try
        {
            if (!rootStillTrusted())
                return;
            using var handle = ServerOwnedContinuousLearningCacheRootNative
                .OpenNamedRegularLeaf(
                    root,
                    temporaryLeafName,
                    mutationAccess: true,
                    ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes);
            var actual = ServerOwnedContinuousLearningCacheRootNative
                .DescribeNamedRegularLeaf(
                    handle,
                    ServerOwnedContinuousLearningCacheRootNative.MaximumOperationBytes,
                    out _);
            if (actual != expectedIdentity)
                return;
            ServerOwnedContinuousLearningCacheRootNative
                .MarkNamedRegularLeafDeleteOnClose(
                    root,
                    temporaryLeafName,
                    handle);
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            // A cleanup failure must never cause deletion by a path fallback.
        }
    }

    private static bool IsValidIdentity(
        ServerOwnedContinuousLearningCacheFileIdentity identity)
        => identity.VolumeSerialNumber != 0 &&
           (identity.FileIdLow != 0 || identity.FileIdHigh != 0);

    private readonly record struct AdapterWriteReceipt(long Length, string Sha256Hex);

    private readonly record struct AdapterReadback(
        ServerOwnedContinuousLearningAdapterArtifactIdentity FinalArtifactIdentity,
        long Length,
        string Sha256Hex)
    {
        internal bool Matches(AdapterWriteReceipt receipt)
            => Length == receipt.Length &&
               string.Equals(Sha256Hex, receipt.Sha256Hex, StringComparison.Ordinal);

        internal bool Matches(AdapterReadback other)
            => FinalArtifactIdentity == other.FinalArtifactIdentity &&
               Length == other.Length &&
               string.Equals(Sha256Hex, other.Sha256Hex, StringComparison.Ordinal);

        internal bool Matches(ServerOwnedContinuousLearningAdapterPublication receipt)
            => FinalArtifactIdentity == receipt.FinalArtifactIdentity &&
               Length == receipt.Length &&
               string.Equals(Sha256Hex, receipt.Sha256Hex, StringComparison.Ordinal);
    }

    /// <summary>
    /// Preserves the current v2 serializer's seekable-stream contract while
    /// refusing every write or position expansion beyond the caller-approved
    /// publication bound. It deliberately does not dispose its caller-owned
    /// inner stream.
    /// </summary>
    private sealed class BoundedWriteStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _maximumLength;
        private readonly CancellationToken _publicationCancellationToken;
        private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        private readonly Action? _disposedCallback;
        private long _hashedLength;
        private bool _disposed;

        internal BoundedWriteStream(
            Stream inner,
            long maximumLength,
            CancellationToken publicationCancellationToken,
            Action? disposedCallback)
        {
            _inner = inner;
            _maximumLength = maximumLength;
            _publicationCancellationToken = publicationCancellationToken;
            _disposedCallback = disposedCallback;
        }

        public override bool CanRead => false;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => _inner.CanWrite;

        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set
            {
                ThrowIfPublicationCancelled();
                EnsureSequentialPosition(value);
                EnsureWithinBound(value);
                _inner.Position = value;
            }
        }

        public override void Flush()
        {
            ThrowIfPublicationCancelled();
            _inner.Flush();
            ThrowIfPublicationCancelled();
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            ThrowIfPublicationCancelled();
            cancellationToken.ThrowIfCancellationRequested();
            using var linkedCancellation = CreateLinkedCancellation(
                cancellationToken,
                out var effectiveCancellationToken);
            await _inner.FlushAsync(effectiveCancellationToken).ConfigureAwait(false);
            ThrowIfPublicationCancelled();
            cancellationToken.ThrowIfCancellationRequested();
        }

        public override int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfPublicationCancelled();
            var target = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => checked(_inner.Position + offset),
                SeekOrigin.End => checked(_inner.Length + offset),
                _ => throw new ArgumentOutOfRangeException(nameof(origin)),
            };
            EnsureSequentialPosition(target);
            EnsureWithinBound(target);
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            ThrowIfPublicationCancelled();
            EnsureSequentialPosition(value);
            EnsureWithinBound(value);
            _inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ThrowIfPublicationCancelled();
            EnsureSequentialPosition(_inner.Position);
            EnsureWriteWithinBound(count);
            _inner.Write(buffer, offset, count);
            _hash.AppendData(buffer, offset, count);
            _hashedLength = checked(_hashedLength + count);
            ThrowIfPublicationCancelled();
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            ThrowIfPublicationCancelled();
            EnsureSequentialPosition(_inner.Position);
            EnsureWriteWithinBound(buffer.Length);
            _inner.Write(buffer);
            _hash.AppendData(buffer);
            _hashedLength = checked(_hashedLength + buffer.Length);
            ThrowIfPublicationCancelled();
        }

        public override void WriteByte(byte value)
        {
            ThrowIfPublicationCancelled();
            EnsureSequentialPosition(_inner.Position);
            EnsureWriteWithinBound(1);
            _inner.WriteByte(value);
            _hash.AppendData([value]);
            _hashedLength = checked(_hashedLength + 1);
            ThrowIfPublicationCancelled();
        }

        public override async Task WriteAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ThrowIfPublicationCancelled();
            cancellationToken.ThrowIfCancellationRequested();
            EnsureSequentialPosition(_inner.Position);
            EnsureWriteWithinBound(count);
            using var linkedCancellation = CreateLinkedCancellation(
                cancellationToken,
                out var effectiveCancellationToken);
            await _inner.WriteAsync(buffer, offset, count, effectiveCancellationToken).ConfigureAwait(false);
            _hash.AppendData(buffer, offset, count);
            _hashedLength = checked(_hashedLength + count);
            ThrowIfPublicationCancelled();
            cancellationToken.ThrowIfCancellationRequested();
        }

        public override async ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            ThrowIfPublicationCancelled();
            cancellationToken.ThrowIfCancellationRequested();
            EnsureSequentialPosition(_inner.Position);
            EnsureWriteWithinBound(buffer.Length);
            using var linkedCancellation = CreateLinkedCancellation(
                cancellationToken,
                out var effectiveCancellationToken);
            await _inner.WriteAsync(buffer, effectiveCancellationToken).ConfigureAwait(false);
            _hash.AppendData(buffer.Span);
            _hashedLength = checked(_hashedLength + buffer.Length);
            ThrowIfPublicationCancelled();
            cancellationToken.ThrowIfCancellationRequested();
        }

        internal AdapterWriteReceipt Complete()
        {
            ThrowIfPublicationCancelled();
            EnsureSequentialPosition(_inner.Position);
            if (_inner.Length != _hashedLength)
            {
                throw new InvalidDataException(
                    "Adapter serializer produced a non-sequential or truncated artifact.");
            }

            return new AdapterWriteReceipt(
                _hashedLength,
                Convert.ToHexString(_hash.GetHashAndReset()).ToLowerInvariant());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _hash.Dispose();
                _disposedCallback?.Invoke();
            }

            // The publication owns and closes the caller-owned inner stream
            // separately; disposing the wrapper must never close that stream.
            base.Dispose(disposing);
        }

        private CancellationTokenSource? CreateLinkedCancellation(
            CancellationToken serializerCancellationToken,
            out CancellationToken effectiveCancellationToken)
        {
            if (!_publicationCancellationToken.CanBeCanceled)
            {
                effectiveCancellationToken = serializerCancellationToken;
                return null;
            }

            if (!serializerCancellationToken.CanBeCanceled ||
                serializerCancellationToken == _publicationCancellationToken)
            {
                effectiveCancellationToken = _publicationCancellationToken;
                return null;
            }

            var linked = CancellationTokenSource.CreateLinkedTokenSource(
                _publicationCancellationToken,
                serializerCancellationToken);
            effectiveCancellationToken = linked.Token;
            return linked;
        }

        private void EnsureWriteWithinBound(int count)
            => EnsureWithinBound(checked(_inner.Position + count));

        private void EnsureSequentialPosition(long value)
        {
            if (value != _hashedLength)
            {
                throw new InvalidDataException(
                    "Adapter serializer attempted a non-sequential write or seek.");
            }
        }

        private void ThrowIfPublicationCancelled()
            => _publicationCancellationToken.ThrowIfCancellationRequested();

        private void EnsureWithinBound(long value)
        {
            if (value < 0 || value > _maximumLength)
            {
                throw new InvalidDataException(
                    "Adapter checkpoint exceeded its publication bound while writing.");
            }
        }
    }

    /// <summary>
    /// Adds cooperative cancellation and retained-root trust checks to the
    /// generic validator without taking ownership of its caller-owned stream.
    /// </summary>
    private sealed class TrustAndCancellationAwareReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly Func<bool> _rootStillTrusted;
        private readonly CancellationToken _cancellationToken;

        internal TrustAndCancellationAwareReadStream(
            Stream inner,
            Func<bool> rootStillTrusted,
            CancellationToken cancellationToken)
        {
            _inner = inner;
            _rootStillTrusted = rootStillTrusted;
            _cancellationToken = cancellationToken;
        }

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => false;

        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set
            {
                ThrowIfUnavailable();
                _inner.Position = value;
            }
        }

        public override void Flush()
            => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfUnavailable();
            var read = _inner.Read(buffer, offset, count);
            ThrowIfUnavailable();
            return read;
        }

        public override int Read(Span<byte> buffer)
        {
            ThrowIfUnavailable();
            var read = _inner.Read(buffer);
            ThrowIfUnavailable();
            return read;
        }

        public override int ReadByte()
        {
            ThrowIfUnavailable();
            var value = _inner.ReadByte();
            ThrowIfUnavailable();
            return value;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfUnavailable();
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
            => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            // The exact-read operation owns the inner stream separately.
            base.Dispose(disposing);
        }

        private void ThrowIfUnavailable()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            if (!_rootStillTrusted())
            {
                throw new AdapterPublicationTrustException(
                    "AdapterRoot identity changed during checkpoint validation.");
            }
        }
    }
}
