// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Buffers.Binary;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;

internal enum ServerOwnedContinuousLearningRegistryLeaf
{
    OwnerLock,
    CanonicalRegistry,
    WriteTemp,
    PublicationBackup,
    RollbackEvidence,
    AcceptedStaging,
    AcceptedEvidence,
}

internal enum ServerOwnedContinuousLearningRegistryCanonicalState
{
    Absent,
    Present,
}

internal enum ServerOwnedContinuousLearningRegistryLeafState
{
    Absent,
    Present,
}

internal enum ServerOwnedContinuousLearningRegistryReconcileDisposition
{
    AlreadyRestoredAbsence,
    RestoredAbsence,
    AlreadyRestoredPrior,
    RestoredPrior,
}

internal enum ServerOwnedContinuousLearningRegistryIoStage
{
    AfterWriteTempOpenBeforeGrowth,
    AfterCanonicalCreateOnlyOpenBeforeGrowth,
    BeforeFileFlush,
    AfterFileFlush,
    AfterReadOpenBeforeContentRead,
    BeforeNoReplaceRename,
    AfterNoReplaceRename,
    BeforeExactDelete,
    AfterExactDelete,
    AfterRejectedArtifactDelete,
    AfterPriorArtifactRestore,
    AfterRollbackEvidenceDurable,
    AfterRollbackWorkingCopyDurable,
    AfterRollbackCanonicalDurable,
    AfterAcceptedStagingDurable,
    AfterAcceptedEvidenceRename,
    AfterAcceptedCommitDurable,
    AfterAcceptedBackupDelete,
    AfterAcceptedRollbackDelete,
    AfterAcceptedEvidenceDelete,
    BeforeRootDirectoryFlush,
    AfterRootDirectoryFlush,
    AfterOperationBeforeRootRevalidation,
}

internal enum ServerOwnedContinuousLearningRegistryNativeCall
{
    FileFlush,
    NoReplaceRename,
    RootDirectoryFlush,
}

internal sealed record ServerOwnedContinuousLearningRegistryRootCapabilityOptions
{
    internal Action<ServerOwnedContinuousLearningRegistryIoStage>?
        ObserveIoStageForTesting
    {
        get;
        init;
    }

    internal Func<ServerOwnedContinuousLearningRegistryNativeCall, bool>?
        FailNativeCallForTesting
    {
        get;
        init;
    }
}

internal readonly record struct ServerOwnedContinuousLearningRegistryFileIdentity(
    ulong VolumeSerialNumber,
    ulong FileIdLow,
    ulong FileIdHigh);

/// <summary>
/// Immutable SHA-256 evidence without exposing a mutable byte array or a
/// caller-controlled string through the capability boundary.
/// </summary>
internal readonly record struct ServerOwnedContinuousLearningRegistryContentHash(
    ulong Word0,
    ulong Word1,
    ulong Word2,
    ulong Word3)
{
    internal static ServerOwnedContinuousLearningRegistryContentHash FromBytes(
        ReadOnlySpan<byte> bytes)
    {
        Span<byte> digest = stackalloc byte[32];
        SHA256.HashData(bytes, digest);
        return new(
            BinaryPrimitives.ReadUInt64BigEndian(digest[0..8]),
            BinaryPrimitives.ReadUInt64BigEndian(digest[8..16]),
            BinaryPrimitives.ReadUInt64BigEndian(digest[16..24]),
            BinaryPrimitives.ReadUInt64BigEndian(digest[24..32]));
    }
}

internal readonly record struct ServerOwnedContinuousLearningRegistryFileDescriptor(
    ServerOwnedContinuousLearningRegistryFileIdentity Identity,
    long Length);

/// <summary>
/// Path-free immutable evidence for one exact bounded read. Bytes are retained
/// privately and can only be obtained as a defensive copy.
/// </summary>
internal sealed class ServerOwnedContinuousLearningRegistrySnapshot
{
    private readonly byte[] _bytes;

    internal ServerOwnedContinuousLearningRegistrySnapshot(
        ServerOwnedContinuousLearningRegistryFileIdentity identity,
        byte[] bytes)
    {
        Identity = identity;
        _bytes = bytes.ToArray();
        Length = _bytes.LongLength;
        Sha256 = ServerOwnedContinuousLearningRegistryContentHash.FromBytes(_bytes);
    }

    internal ServerOwnedContinuousLearningRegistryFileIdentity Identity { get; }

    internal long Length { get; }

    internal ServerOwnedContinuousLearningRegistryContentHash Sha256 { get; }

    internal byte[] CopyBytes() => _bytes.ToArray();

    internal bool Matches(ServerOwnedContinuousLearningRegistrySnapshot other)
        => Identity == other.Identity &&
           Length == other.Length &&
           Sha256 == other.Sha256 &&
           _bytes.AsSpan().SequenceEqual(other._bytes);

    internal bool ContentMatches(ServerOwnedContinuousLearningRegistrySnapshot other)
        => Length == other.Length &&
           Sha256 == other.Sha256 &&
           _bytes.AsSpan().SequenceEqual(other._bytes);
}

/// <summary>
/// Typed, path-free state for one member of the fixed registry namespace.
/// Only an actually missing leaf is represented as <see cref="Absent"/>.
/// </summary>
internal sealed class ServerOwnedContinuousLearningRegistryLeafObservation
{
    private ServerOwnedContinuousLearningRegistryLeafObservation(
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryLeafState state,
        ServerOwnedContinuousLearningRegistrySnapshot? snapshot)
    {
        Leaf = leaf;
        State = state;
        Snapshot = snapshot;
    }

    internal ServerOwnedContinuousLearningRegistryLeaf Leaf { get; }

    internal ServerOwnedContinuousLearningRegistryLeafState State { get; }

    internal ServerOwnedContinuousLearningRegistrySnapshot? Snapshot { get; }

    internal static ServerOwnedContinuousLearningRegistryLeafObservation Absent(
        ServerOwnedContinuousLearningRegistryLeaf leaf)
        => new(leaf, ServerOwnedContinuousLearningRegistryLeafState.Absent, null);

    internal static ServerOwnedContinuousLearningRegistryLeafObservation Present(
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistrySnapshot snapshot)
        => new(leaf, ServerOwnedContinuousLearningRegistryLeafState.Present, snapshot);

    internal bool Matches(ServerOwnedContinuousLearningRegistrySnapshot expected)
        => State == ServerOwnedContinuousLearningRegistryLeafState.Present &&
           Snapshot is not null &&
           Snapshot.Matches(expected);
}

/// <summary>
/// Complete immutable evidence for all six registry data leaves: canonical,
/// write temporary, publication backup, rollback evidence, accepted staging,
/// and accepted evidence. The owner-lock leaf is authority, not registry data,
/// and is deliberately excluded.
/// Content/schema meaning remains DomainModelRegistry's responsibility.
/// </summary>
internal sealed class ServerOwnedContinuousLearningRegistryNamespaceInventory
{
    internal const int FixedDataLeafCount = 6;

    internal ServerOwnedContinuousLearningRegistryNamespaceInventory(
        ServerOwnedContinuousLearningRegistryLeafObservation canonicalRegistry,
        ServerOwnedContinuousLearningRegistryLeafObservation writeTemp,
        ServerOwnedContinuousLearningRegistryLeafObservation publicationBackup,
        ServerOwnedContinuousLearningRegistryLeafObservation rollbackEvidence,
        ServerOwnedContinuousLearningRegistryLeafObservation acceptedStaging,
        ServerOwnedContinuousLearningRegistryLeafObservation acceptedEvidence)
    {
        CanonicalRegistry = canonicalRegistry;
        WriteTemp = writeTemp;
        PublicationBackup = publicationBackup;
        RollbackEvidence = rollbackEvidence;
        AcceptedStaging = acceptedStaging;
        AcceptedEvidence = acceptedEvidence;
    }

    internal ServerOwnedContinuousLearningRegistryLeafObservation CanonicalRegistry { get; }

    internal ServerOwnedContinuousLearningRegistryLeafObservation WriteTemp { get; }

    internal ServerOwnedContinuousLearningRegistryLeafObservation PublicationBackup { get; }

    internal ServerOwnedContinuousLearningRegistryLeafObservation RollbackEvidence { get; }

    internal ServerOwnedContinuousLearningRegistryLeafObservation AcceptedStaging { get; }

    internal ServerOwnedContinuousLearningRegistryLeafObservation AcceptedEvidence { get; }
}

/// <summary>
/// Canonical, capability-owned transaction evidence for an interrupted
/// rollback. Registry JSON remains opaque here; the registry layer validates
/// its meaning before authorizing recovery.
/// </summary>
internal sealed class ServerOwnedContinuousLearningRegistryRollbackEvidence
{
    internal const int EncodedHeaderLength = 144;
    internal const long MaximumEncodedBytes =
        DomainModelRegistry.MaximumRegistryBytes + EncodedHeaderLength;

    private readonly byte[] _priorBytes;

    internal ServerOwnedContinuousLearningRegistryRollbackEvidence(
        ServerOwnedContinuousLearningRegistrySnapshot markerArtifact,
        ServerOwnedContinuousLearningRegistryCanonicalState priorState,
        ServerOwnedContinuousLearningRegistryFileIdentity priorIdentity,
        ServerOwnedContinuousLearningRegistryContentHash priorSha256,
        byte[] priorBytes,
        bool hasRejectedArtifact,
        ServerOwnedContinuousLearningRegistryFileIdentity rejectedIdentity,
        long rejectedLength,
        ServerOwnedContinuousLearningRegistryContentHash rejectedSha256)
    {
        MarkerArtifact = markerArtifact;
        PriorState = priorState;
        PriorIdentity = priorIdentity;
        PriorSha256 = priorSha256;
        _priorBytes = priorBytes.ToArray();
        HasRejectedArtifact = hasRejectedArtifact;
        RejectedIdentity = rejectedIdentity;
        RejectedLength = rejectedLength;
        RejectedSha256 = rejectedSha256;
    }

    internal ServerOwnedContinuousLearningRegistrySnapshot MarkerArtifact { get; }

    internal ServerOwnedContinuousLearningRegistryCanonicalState PriorState { get; }

    internal ServerOwnedContinuousLearningRegistryFileIdentity PriorIdentity { get; }

    internal long PriorLength => _priorBytes.LongLength;

    internal ServerOwnedContinuousLearningRegistryContentHash PriorSha256 { get; }

    internal bool HasRejectedArtifact { get; }

    internal ServerOwnedContinuousLearningRegistryFileIdentity RejectedIdentity { get; }

    internal long RejectedLength { get; }

    internal ServerOwnedContinuousLearningRegistryContentHash RejectedSha256 { get; }

    internal byte[] CopyPriorBytes() => _priorBytes.ToArray();

    internal bool PriorArtifactMatches(
        ServerOwnedContinuousLearningRegistrySnapshot artifact)
        => PriorState == ServerOwnedContinuousLearningRegistryCanonicalState.Present &&
           artifact.Identity == PriorIdentity &&
           PriorContentMatches(artifact);

    internal bool PriorContentMatches(
        ServerOwnedContinuousLearningRegistrySnapshot artifact)
        => PriorState == ServerOwnedContinuousLearningRegistryCanonicalState.Present &&
           artifact.Length == PriorLength &&
           artifact.Sha256 == PriorSha256 &&
           artifact.CopyBytes().AsSpan().SequenceEqual(_priorBytes);

    internal bool RejectedArtifactMatches(
        ServerOwnedContinuousLearningRegistrySnapshot artifact)
        => HasRejectedArtifact &&
           artifact.Identity == RejectedIdentity &&
           artifact.Length == RejectedLength &&
           artifact.Sha256 == RejectedSha256;
}

/// <summary>
/// Typed commit evidence. The complete rollback record is embedded so an
/// accepted transaction remains independently verifiable after rollback
/// provenance has been removed during idempotent cleanup.
/// </summary>
internal sealed class ServerOwnedContinuousLearningRegistryAcceptedEvidence
{
    internal const int EncodedHeaderLength = 80;
    internal const long MaximumEncodedBytes =
        ServerOwnedContinuousLearningRegistryRollbackEvidence.MaximumEncodedBytes +
        EncodedHeaderLength;

    internal ServerOwnedContinuousLearningRegistryAcceptedEvidence(
        ServerOwnedContinuousLearningRegistrySnapshot markerArtifact,
        ServerOwnedContinuousLearningRegistryRollbackEvidence rollbackEvidence)
    {
        MarkerArtifact = markerArtifact;
        RollbackEvidence = rollbackEvidence;
    }

    internal ServerOwnedContinuousLearningRegistrySnapshot MarkerArtifact { get; }

    internal ServerOwnedContinuousLearningRegistryRollbackEvidence RollbackEvidence { get; }
}

internal sealed class ServerOwnedContinuousLearningRegistryPriorStateEvidence
{
    private ServerOwnedContinuousLearningRegistryPriorStateEvidence(
        ServerOwnedContinuousLearningRegistryCanonicalState state,
        ServerOwnedContinuousLearningRegistrySnapshot? snapshot)
    {
        State = state;
        Snapshot = snapshot;
    }

    internal ServerOwnedContinuousLearningRegistryCanonicalState State { get; }

    internal ServerOwnedContinuousLearningRegistrySnapshot? Snapshot { get; }

    internal static ServerOwnedContinuousLearningRegistryPriorStateEvidence Absent()
        => new(ServerOwnedContinuousLearningRegistryCanonicalState.Absent, null);

    internal static ServerOwnedContinuousLearningRegistryPriorStateEvidence Present(
        ServerOwnedContinuousLearningRegistrySnapshot snapshot)
        => new(
            ServerOwnedContinuousLearningRegistryCanonicalState.Present,
            snapshot ?? throw new ArgumentNullException(nameof(snapshot)));
}

internal sealed record ServerOwnedContinuousLearningRegistryReconcileResult(
    ServerOwnedContinuousLearningRegistryReconcileDisposition Disposition,
    ServerOwnedContinuousLearningRegistryNamespaceInventory FinalInventory);

/// <summary>
/// Canonical absence is a normal typed state. Any malformed, non-regular, or
/// identity-lost leaf throws instead and therefore cannot be mistaken for
/// absence.
/// </summary>
internal sealed class ServerOwnedContinuousLearningRegistryCanonicalRead
{
    private ServerOwnedContinuousLearningRegistryCanonicalRead(
        ServerOwnedContinuousLearningRegistryCanonicalState state,
        ServerOwnedContinuousLearningRegistrySnapshot? snapshot)
    {
        State = state;
        Snapshot = snapshot;
    }

    internal ServerOwnedContinuousLearningRegistryCanonicalState State { get; }

    internal ServerOwnedContinuousLearningRegistrySnapshot? Snapshot { get; }

    internal static ServerOwnedContinuousLearningRegistryCanonicalRead Absent()
        => new(ServerOwnedContinuousLearningRegistryCanonicalState.Absent, null);

    internal static ServerOwnedContinuousLearningRegistryCanonicalRead Present(
        ServerOwnedContinuousLearningRegistrySnapshot snapshot)
        => new(ServerOwnedContinuousLearningRegistryCanonicalState.Present, snapshot);
}

/// <summary>
/// Revocable, one-shot, path-free authority over RegistryRoot's fixed
/// publication protocol. It does not know or validate registry JSON semantics;
/// it only provides bounded immutable byte and crash-recovery primitives.
/// </summary>
internal sealed class ServerOwnedContinuousLearningRegistryRootCapability : IDisposable
{
    private readonly ServerOwnedContinuousLearningFileAccess _owner;
    private readonly SafeFileHandle _ownerLockHandle;
    private readonly ServerOwnedContinuousLearningRegistryRootCapabilityOptions _options;
    private bool _disposed;
    private bool _revoked;

    internal ServerOwnedContinuousLearningRegistryRootCapability(
        ServerOwnedContinuousLearningFileAccess owner,
        SafeFileHandle ownerLockHandle,
        ServerOwnedContinuousLearningRegistryRootCapabilityOptions options)
    {
        _owner = owner;
        _ownerLockHandle = ownerLockHandle;
        _options = options;
    }

    internal ServerOwnedContinuousLearningRegistryCanonicalRead ReadCanonical()
        => _owner.ReadCanonicalRegistry(this);

    internal ServerOwnedContinuousLearningRegistryNamespaceInventory Inventory()
        => _owner.InventoryRegistryNamespace(this);

    internal ServerOwnedContinuousLearningRegistryFileDescriptor CreateWriteTemp(
        ReadOnlyMemory<byte> bytes)
    {
        if (bytes.IsEmpty || bytes.Length > DomainModelRegistry.MaximumRegistryBytes)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        return _owner.CreateRegistryWriteTemp(this, bytes.ToArray());
    }

    internal ServerOwnedContinuousLearningRegistrySnapshot CreateCanonicalIfAbsent(
        ReadOnlyMemory<byte> bytes)
    {
        if (bytes.IsEmpty || bytes.Length > DomainModelRegistry.MaximumRegistryBytes)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        return _owner.CreateCanonicalRegistryIfAbsent(this, bytes.ToArray());
    }

    internal ServerOwnedContinuousLearningRegistrySnapshot ReadExact(
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity)
    {
        ValidateReadableLeaf(leaf);
        ValidateIdentity(expectedIdentity);
        return _owner.ReadExactRegistryLeaf(this, leaf, expectedIdentity);
    }

    internal ServerOwnedContinuousLearningRegistryFileDescriptor QueryExactIdentity(
        ServerOwnedContinuousLearningRegistryLeaf leaf)
    {
        ValidateReadableLeaf(leaf);
        return _owner.QueryRegistryLeafIdentity(this, leaf);
    }

    internal void ValidateExactIdentity(
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity)
    {
        ValidateReadableLeaf(leaf);
        ValidateIdentity(expectedIdentity);
        _owner.ValidateRegistryLeafIdentity(this, leaf, expectedIdentity);
    }

    internal void RenameNoReplace(
        ServerOwnedContinuousLearningRegistryLeaf source,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedSourceIdentity,
        ServerOwnedContinuousLearningRegistryLeaf destination)
    {
        ValidateRenameEdge(source, destination);
        ValidateIdentity(expectedSourceIdentity);
        _owner.RenameRegistryLeafNoReplace(
            this,
            source,
            expectedSourceIdentity,
            destination);
    }

    internal void DeleteExactOpenedIdentity(
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity)
    {
        ValidateRecoveryLeaf(leaf);
        ValidateIdentity(expectedIdentity);
        _owner.DeleteRegistryLeafExact(this, leaf, expectedIdentity);
    }

    internal void FlushFile(
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity)
    {
        ValidateReadableLeaf(leaf);
        ValidateIdentity(expectedIdentity);
        _owner.FlushRegistryLeaf(this, leaf, expectedIdentity);
    }

    internal void FlushRootDirectory()
        => _owner.FlushRegistryRoot(this);

    internal ServerOwnedContinuousLearningRegistryNamespaceInventory RestoreBackupOnly(
        ServerOwnedContinuousLearningRegistrySnapshot priorArtifact)
    {
        ArgumentNullException.ThrowIfNull(priorArtifact);
        ValidateSnapshot(priorArtifact);
        return _owner.RestoreRegistryBackupOnly(this, priorArtifact);
    }

    internal ServerOwnedContinuousLearningRegistryRollbackEvidence DecodeRollbackEvidence(
        ServerOwnedContinuousLearningRegistrySnapshot markerArtifact)
    {
        ArgumentNullException.ThrowIfNull(markerArtifact);
        ValidateRollbackMarker(markerArtifact);
        return ServerOwnedContinuousLearningRegistryRootNative.DecodeRollbackEvidence(
            markerArtifact);
    }

    internal ServerOwnedContinuousLearningRegistryAcceptedEvidence DecodeAcceptedEvidence(
        ServerOwnedContinuousLearningRegistrySnapshot markerArtifact)
    {
        ArgumentNullException.ThrowIfNull(markerArtifact);
        ValidateAcceptedMarker(markerArtifact);
        return ServerOwnedContinuousLearningRegistryRootNative.DecodeAcceptedEvidence(
            markerArtifact);
    }

    internal ServerOwnedContinuousLearningRegistryRollbackEvidence PreparePublication(
        ServerOwnedContinuousLearningRegistrySnapshot candidateArtifact,
        ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState)
    {
        ArgumentNullException.ThrowIfNull(candidateArtifact);
        ArgumentNullException.ThrowIfNull(priorState);
        ValidatePublicationEvidence(candidateArtifact, priorState);
        return _owner.PrepareRegistryPublication(this, candidateArtifact, priorState);
    }

    internal ServerOwnedContinuousLearningRegistryAcceptedEvidence CommitPublication(
        ServerOwnedContinuousLearningRegistrySnapshot candidateArtifact,
        ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState,
        ServerOwnedContinuousLearningRegistryRollbackEvidence rollbackEvidence)
    {
        ArgumentNullException.ThrowIfNull(candidateArtifact);
        ArgumentNullException.ThrowIfNull(priorState);
        ArgumentNullException.ThrowIfNull(rollbackEvidence);
        ValidatePublicationEvidence(candidateArtifact, priorState);
        ValidateRollbackMarker(rollbackEvidence.MarkerArtifact);
        return _owner.CommitRegistryPublication(
            this,
            candidateArtifact,
            priorState,
            rollbackEvidence);
    }

    internal ServerOwnedContinuousLearningRegistryNamespaceInventory ResumeAcceptedCleanup(
        ServerOwnedContinuousLearningRegistryAcceptedEvidence acceptedEvidence)
    {
        ArgumentNullException.ThrowIfNull(acceptedEvidence);
        ValidateAcceptedMarker(acceptedEvidence.MarkerArtifact);
        return _owner.ResumeAcceptedRegistryCleanup(this, acceptedEvidence);
    }

    internal ServerOwnedContinuousLearningRegistryNamespaceInventory
        ResumeRollbackReconstruction(
            ServerOwnedContinuousLearningRegistryRollbackEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ValidateRollbackMarker(evidence.MarkerArtifact);
        return _owner.ResumeRegistryRollbackReconstruction(this, evidence);
    }

    /// <summary>
    /// Reconciles only namespace states that can arise while publishing one
    /// exact new artifact. It never interprets registry JSON and it never
    /// accepts path or leaf-name input from the caller.
    /// </summary>
    internal ServerOwnedContinuousLearningRegistryReconcileResult
        ReconcileRejectedPublication(
            ServerOwnedContinuousLearningRegistrySnapshot rejectedArtifact,
            ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState)
    {
        ArgumentNullException.ThrowIfNull(rejectedArtifact);
        ArgumentNullException.ThrowIfNull(priorState);
        ValidatePublicationEvidence(rejectedArtifact, priorState);

        return _owner.ReconcileRejectedRegistryPublication(
            this,
            rejectedArtifact,
            priorState);
    }

    public void Dispose()
        => _owner.DisposeRegistryRootCapability(this);

    internal void ThrowIfUnavailableLocked()
    {
        if (_disposed || _revoked)
            throw new ContinuousLearningPathIdentityException();
    }

    internal void ObserveIoStageLocked(
        ServerOwnedContinuousLearningRegistryIoStage stage)
        => _options.ObserveIoStageForTesting?.Invoke(stage);

    internal void ThrowIfNativeCallFailsForTestingLocked(
        ServerOwnedContinuousLearningRegistryNativeCall call)
    {
        if (_options.FailNativeCallForTesting?.Invoke(call) == true)
            throw new IOException("A retained RegistryRoot native operation failed.");
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

    private static void ValidateReadableLeaf(
        ServerOwnedContinuousLearningRegistryLeaf leaf)
    {
        if (leaf is not ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry and
            not ServerOwnedContinuousLearningRegistryLeaf.WriteTemp and
            not ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup and
            not ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence and
            not ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging and
            not ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence)
        {
            throw new ArgumentOutOfRangeException(nameof(leaf));
        }
    }

    private static void ValidateRecoveryLeaf(
        ServerOwnedContinuousLearningRegistryLeaf leaf)
    {
        if (leaf is not ServerOwnedContinuousLearningRegistryLeaf.WriteTemp and
            not ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup and
            not ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence and
            not ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging and
            not ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence)
        {
            throw new ArgumentOutOfRangeException(nameof(leaf));
        }
    }

    private static void ValidateRenameEdge(
        ServerOwnedContinuousLearningRegistryLeaf source,
        ServerOwnedContinuousLearningRegistryLeaf destination)
    {
        var valid = source == ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry &&
                    destination == ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup ||
                    source == ServerOwnedContinuousLearningRegistryLeaf.WriteTemp &&
                    destination == ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry ||
                    source == ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup &&
                    destination == ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry ||
                    source == ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging &&
                    destination == ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence;
        if (!valid)
            throw new ArgumentException("The fixed registry rename edge is not permitted.");
    }

    private static void ValidateIdentity(
        ServerOwnedContinuousLearningRegistryFileIdentity identity)
    {
        if (identity.VolumeSerialNumber == 0 ||
            identity.FileIdLow == 0 && identity.FileIdHigh == 0)
        {
            throw new ArgumentException("A nonzero opened-file identity is required.");
        }
    }

    private static void ValidateSnapshot(
        ServerOwnedContinuousLearningRegistrySnapshot snapshot)
    {
        ValidateIdentity(snapshot.Identity);
        if (snapshot.Length <= 0 || snapshot.Length > DomainModelRegistry.MaximumRegistryBytes)
            throw new ArgumentException("Registry evidence must be non-empty and bounded.");
        if (snapshot.CopyBytes().LongLength != snapshot.Length)
            throw new ArgumentException("Registry evidence length is inconsistent.");
    }

    private static void ValidateRollbackMarker(
        ServerOwnedContinuousLearningRegistrySnapshot snapshot)
    {
        ValidateIdentity(snapshot.Identity);
        if (snapshot.Length <
                ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength ||
            snapshot.Length >
                ServerOwnedContinuousLearningRegistryRollbackEvidence.MaximumEncodedBytes ||
            snapshot.CopyBytes().LongLength != snapshot.Length)
        {
            throw new ArgumentException("Rollback evidence is malformed or out of bounds.");
        }
    }

    private static void ValidateAcceptedMarker(
        ServerOwnedContinuousLearningRegistrySnapshot snapshot)
    {
        ValidateIdentity(snapshot.Identity);
        if (snapshot.Length <
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.EncodedHeaderLength +
                ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength ||
            snapshot.Length >
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.MaximumEncodedBytes ||
            snapshot.CopyBytes().LongLength != snapshot.Length)
        {
            throw new ArgumentException("Accepted evidence is malformed or out of bounds.");
        }
    }

    private static void ValidatePublicationEvidence(
        ServerOwnedContinuousLearningRegistrySnapshot artifact,
        ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState)
    {
        ValidateSnapshot(artifact);
        if (priorState.State == ServerOwnedContinuousLearningRegistryCanonicalState.Present)
        {
            if (priorState.Snapshot is null)
                throw new ArgumentException("Present prior evidence requires a snapshot.", nameof(priorState));
            ValidateSnapshot(priorState.Snapshot);
            if (priorState.Snapshot.Identity == artifact.Identity)
                throw new ArgumentException("Prior and publication identities must differ.", nameof(priorState));
        }
        else if (priorState.Snapshot is not null)
        {
            throw new ArgumentException("Absent prior evidence cannot contain a snapshot.", nameof(priorState));
        }
    }
}

internal sealed partial class ServerOwnedContinuousLearningFileAccess
{
    private bool _registryRootCapabilityAcquisitionConsumed;
    private ServerOwnedContinuousLearningRegistryRootCapability? _registryRootCapability;

    internal ServerOwnedContinuousLearningRegistryRootCapability
        AcquireRegistryRootCapability(
            ServerOwnedContinuousLearningRegistryRootCapabilityOptions? options = null)
    {
        Action? callback = null;
        ExceptionDispatchInfo? acquisitionFailure = null;
        ServerOwnedContinuousLearningRegistryRootCapability? capability = null;
        var trustFailed = false;
        options ??= new ServerOwnedContinuousLearningRegistryRootCapabilityOptions();

        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            if (_registryRootCapabilityAcquisitionConsumed)
                throw new InvalidOperationException("The RegistryRoot capability is one-shot.");

            var root = FindRetainedRoot(ContinuousLearningRetainedRootRole.Registry);
            if (!TryValidateRetainedEntry(root))
            {
                _registryRootCapabilityAcquisitionConsumed = true;
                callback = LoseRetainedObjectTrustLocked();
                trustFailed = true;
            }
            else
            {
                SafeFileHandle? ownerLockHandle = null;
                try
                {
                    ownerLockHandle = ServerOwnedContinuousLearningRegistryRootNative
                        .AcquireOwnerLock(root.Handle);
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    acquisitionFailure = ExceptionDispatchInfo.Capture(exception);
                }

                if (acquisitionFailure is not null)
                    ownerLockHandle?.Dispose();

                if (!TryValidateRetainedEntry(root))
                {
                    ownerLockHandle?.Dispose();
                    _registryRootCapabilityAcquisitionConsumed = true;
                    callback = LoseRetainedObjectTrustLocked();
                    trustFailed = true;
                }
                else if (acquisitionFailure is null)
                {
                    _registryRootCapabilityAcquisitionConsumed = true;
                    capability = new ServerOwnedContinuousLearningRegistryRootCapability(
                        this,
                        ownerLockHandle!,
                        options);
                    _registryRootCapability = capability;
                }
            }
        }

        InvokeRetainedObjectTrustLost(callback);
        if (trustFailed)
            throw new ContinuousLearningPathIdentityException();
        acquisitionFailure?.Throw();
        return capability!;
    }

    internal void DisposeRegistryRootCapability(
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_registryRootCapability, capability))
                capability.DisposeLocked();
        }
    }

    internal ServerOwnedContinuousLearningRegistryCanonicalRead ReadCanonicalRegistry(
        ServerOwnedContinuousLearningRegistryRootCapability capability)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.ReadCanonical(
                root,
                capability));

    internal ServerOwnedContinuousLearningRegistryNamespaceInventory
        InventoryRegistryNamespace(
            ServerOwnedContinuousLearningRegistryRootCapability capability)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.Inventory(
                root,
                capability));

    internal ServerOwnedContinuousLearningRegistryFileDescriptor CreateRegistryWriteTemp(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ReadOnlyMemory<byte> bytes)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.CreateWriteTemp(
                root,
                bytes,
                capability));

    internal ServerOwnedContinuousLearningRegistrySnapshot CreateCanonicalRegistryIfAbsent(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ReadOnlyMemory<byte> bytes)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.CreateCanonicalIfAbsent(
                root,
                bytes,
                capability));

    internal ServerOwnedContinuousLearningRegistrySnapshot ReadExactRegistryLeaf(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.ReadExact(
                root,
                leaf,
                expectedIdentity,
                capability));

    internal ServerOwnedContinuousLearningRegistryFileDescriptor QueryRegistryLeafIdentity(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ServerOwnedContinuousLearningRegistryLeaf leaf)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.QueryIdentity(root, leaf));

    internal void ValidateRegistryLeafIdentity(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity)
        => ExecuteRegistryRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningRegistryRootNative.ValidateIdentity(
                    root,
                    leaf,
                    expectedIdentity);
                return true;
            });

    internal void RenameRegistryLeafNoReplace(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ServerOwnedContinuousLearningRegistryLeaf source,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedSourceIdentity,
        ServerOwnedContinuousLearningRegistryLeaf destination)
        => ExecuteRegistryRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningRegistryRootNative.RenameNoReplace(
                    root,
                    source,
                    expectedSourceIdentity,
                    destination,
                    capability);
                return true;
            });

    internal void DeleteRegistryLeafExact(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity)
        => ExecuteRegistryRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningRegistryRootNative.DeleteExact(
                    root,
                    leaf,
                    expectedIdentity,
                    capability);
                return true;
            });

    internal void FlushRegistryLeaf(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity)
        => ExecuteRegistryRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningRegistryRootNative.FlushFile(
                    root,
                    leaf,
                    expectedIdentity,
                    capability);
                return true;
            });

    internal void FlushRegistryRoot(
        ServerOwnedContinuousLearningRegistryRootCapability capability)
        => ExecuteRegistryRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningRegistryRootNative.FlushRoot(
                    root,
                    capability);
                return true;
            });

    internal ServerOwnedContinuousLearningRegistryRollbackEvidence PrepareRegistryPublication(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ServerOwnedContinuousLearningRegistrySnapshot candidateArtifact,
        ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.PreparePublication(
                root,
                candidateArtifact,
                priorState,
                capability));

    internal ServerOwnedContinuousLearningRegistryAcceptedEvidence CommitRegistryPublication(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        ServerOwnedContinuousLearningRegistrySnapshot candidateArtifact,
        ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState,
        ServerOwnedContinuousLearningRegistryRollbackEvidence rollbackEvidence)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.CommitPublication(
                root,
                candidateArtifact,
                priorState,
                rollbackEvidence,
                capability));

    internal ServerOwnedContinuousLearningRegistryNamespaceInventory
        ResumeAcceptedRegistryCleanup(
            ServerOwnedContinuousLearningRegistryRootCapability capability,
            ServerOwnedContinuousLearningRegistryAcceptedEvidence acceptedEvidence)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.ResumeAcceptedCleanup(
                root,
                acceptedEvidence,
                capability));

    internal ServerOwnedContinuousLearningRegistryNamespaceInventory
        RestoreRegistryBackupOnly(
            ServerOwnedContinuousLearningRegistryRootCapability capability,
            ServerOwnedContinuousLearningRegistrySnapshot priorArtifact)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative.RestoreBackupOnly(
                root,
                priorArtifact,
                capability));

    internal ServerOwnedContinuousLearningRegistryNamespaceInventory
        ResumeRegistryRollbackReconstruction(
            ServerOwnedContinuousLearningRegistryRootCapability capability,
            ServerOwnedContinuousLearningRegistryRollbackEvidence evidence)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative
                .ResumeRollbackReconstruction(root, evidence, capability));

    internal ServerOwnedContinuousLearningRegistryReconcileResult
        ReconcileRejectedRegistryPublication(
            ServerOwnedContinuousLearningRegistryRootCapability capability,
            ServerOwnedContinuousLearningRegistrySnapshot rejectedArtifact,
            ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState)
        => ExecuteRegistryRootOperation(
            capability,
            root => ServerOwnedContinuousLearningRegistryRootNative
                .ReconcileRejectedPublication(
                    root,
                    rejectedArtifact,
                    priorState,
                    capability));

    private T ExecuteRegistryRootOperation<T>(
        ServerOwnedContinuousLearningRegistryRootCapability capability,
        Func<SafeFileHandle, T> operation)
    {
        Action? callback = null;
        ExceptionDispatchInfo? operationFailure = null;
        T result = default!;
        var trustFailed = false;

        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            if (!ReferenceEquals(_registryRootCapability, capability))
                throw new ContinuousLearningPathIdentityException();
            capability.ThrowIfUnavailableLocked();

            var root = FindRetainedRoot(ContinuousLearningRetainedRootRole.Registry);
            if (!TryValidateRetainedEntry(root))
            {
                callback = LoseRetainedObjectTrustLocked();
                trustFailed = true;
            }
            else
            {
                try
                {
                    result = operation(root.Handle);
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    operationFailure = ExceptionDispatchInfo.Capture(exception);
                }

                try
                {
                    capability.ObserveIoStageLocked(
                        ServerOwnedContinuousLearningRegistryIoStage
                            .AfterOperationBeforeRootRevalidation);
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    operationFailure ??= ExceptionDispatchInfo.Capture(exception);
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
}

internal static class ServerOwnedContinuousLearningRegistryRootNative
{
    private const string OwnerLockLeafName = ".continuous-learning.registry-owner.lock";
    private const string CanonicalRegistryLeafName = "registry.json";
    private const string WriteTempLeafName = ".registry.json.write.tmp";
    private const string PublicationBackupLeafName = ".registry.json.publication.bak";
    private const string RollbackEvidenceLeafName = ".registry.json.rollback.evidence";
    private const string AcceptedStagingLeafName = ".registry.json.accepted.staging";
    private const string AcceptedEvidenceLeafName = ".registry.json.accepted.evidence";
    private const ulong RollbackEvidenceMagic = 0x58494752424B3031UL;
    private const uint RollbackEvidenceVersion = 1;
    private const uint RollbackPriorPresentFlag = 1;
    private const uint RollbackRejectedPresentFlag = 2;
    private const ulong AcceptedEvidenceMagic = 0x5849474143433031UL;
    private const uint AcceptedEvidenceVersion = 1;

    private sealed class HeldRegistryLeaf : IDisposable
    {
        internal HeldRegistryLeaf(
            SafeFileHandle root,
            ServerOwnedContinuousLearningRegistryLeaf leaf,
            SafeFileHandle handle,
            ServerOwnedContinuousLearningRegistrySnapshot snapshot)
        {
            Root = root;
            Leaf = leaf;
            Handle = handle;
            Snapshot = snapshot;
        }

        internal SafeFileHandle Root { get; }

        internal ServerOwnedContinuousLearningRegistryLeaf Leaf { get; }

        internal SafeFileHandle Handle { get; }

        internal ServerOwnedContinuousLearningRegistrySnapshot Snapshot { get; }

        public void Dispose() => Handle.Dispose();
    }

    internal static SafeFileHandle AcquireOwnerLock(SafeFileHandle root)
        => ServerOwnedContinuousLearningCacheRootNative.AcquireNamedOwnerLock(
            root,
            OwnerLockLeafName);

    internal static ServerOwnedContinuousLearningRegistryNamespaceInventory Inventory(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        HeldRegistryLeaf? canonical = null;
        HeldRegistryLeaf? writeTemp = null;
        HeldRegistryLeaf? publicationBackup = null;
        HeldRegistryLeaf? rollbackEvidence = null;
        HeldRegistryLeaf? acceptedStaging = null;
        HeldRegistryLeaf? acceptedEvidence = null;
        try
        {
            canonical = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                mutationAccess: false,
                capability);
            writeTemp = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                mutationAccess: false,
                capability);
            publicationBackup = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup,
                mutationAccess: false,
                capability);
            rollbackEvidence = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence,
                mutationAccess: false,
                capability);
            acceptedStaging = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging,
                mutationAccess: false,
                capability);
            acceptedEvidence = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
                mutationAccess: false,
                capability);

            // Present leaves stay opened without write/delete sharing until all
            // evidence has been captured. Exact-missing leaves are reprobed so
            // a cooperative publication cannot be silently misclassified.
            RevalidateAbsent(root, canonical, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            RevalidateAbsent(root, writeTemp, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp);
            RevalidateAbsent(root, publicationBackup, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            RevalidateAbsent(root, rollbackEvidence, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence);
            RevalidateAbsent(root, acceptedStaging, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging);
            RevalidateAbsent(root, acceptedEvidence, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence);
            return CreateInventory(
                canonical,
                writeTemp,
                publicationBackup,
                rollbackEvidence,
                acceptedStaging,
                acceptedEvidence);
        }
        finally
        {
            acceptedEvidence?.Dispose();
            acceptedStaging?.Dispose();
            rollbackEvidence?.Dispose();
            publicationBackup?.Dispose();
            writeTemp?.Dispose();
            canonical?.Dispose();
        }
    }

    internal static ServerOwnedContinuousLearningRegistryRollbackEvidence PreparePublication(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistrySnapshot candidateArtifact,
        ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        HeldRegistryLeaf? canonical = null;
        HeldRegistryLeaf? writeTemp = null;
        HeldRegistryLeaf? publicationBackup = null;
        HeldRegistryLeaf? rollbackEvidence = null;
        HeldRegistryLeaf? acceptedStaging = null;
        HeldRegistryLeaf? acceptedEvidence = null;
        try
        {
            canonical = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry, true, capability);
            writeTemp = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp, true, capability);
            publicationBackup = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup, true, capability);
            rollbackEvidence = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence, true, capability);
            acceptedStaging = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging, true, capability);
            acceptedEvidence = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence, true, capability);
            RevalidateAbsent(root, canonical, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            RevalidateAbsent(root, writeTemp, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp);
            RevalidateAbsent(root, publicationBackup, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            RevalidateAbsent(root, rollbackEvidence, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence);
            RevalidateAbsent(root, acceptedStaging, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging);
            RevalidateAbsent(root, acceptedEvidence, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence);

            if (writeTemp is null || !writeTemp.Snapshot.Matches(candidateArtifact) ||
                publicationBackup is not null || rollbackEvidence is not null ||
                acceptedStaging is not null || acceptedEvidence is not null)
            {
                throw new InvalidDataException("The publication journal precondition changed.");
            }
            if (priorState.State == ServerOwnedContinuousLearningRegistryCanonicalState.Absent)
            {
                if (canonical is not null)
                    throw new InvalidDataException("An absent publication prior became present.");
            }
            else if (priorState.Snapshot is null || canonical is null ||
                     !canonical.Snapshot.Matches(priorState.Snapshot))
            {
                throw new InvalidDataException("The exact publication prior changed.");
            }

            var descriptor = CreateRollbackEvidence(
                root,
                priorState,
                candidateArtifact,
                capability);
            var marker = ReadExact(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence,
                descriptor.Identity,
                capability);
            var evidence = DecodeRollbackEvidence(marker);
            if (!evidence.RejectedArtifactMatches(candidateArtifact) ||
                evidence.PriorState != priorState.State ||
                (priorState.Snapshot is not null &&
                 !evidence.PriorArtifactMatches(priorState.Snapshot)))
            {
                throw new InvalidDataException("The durable publication journal changed.");
            }
            FlushRoot(root, capability);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.AfterRollbackEvidenceDurable);
            return evidence;
        }
        finally
        {
            acceptedEvidence?.Dispose();
            acceptedStaging?.Dispose();
            rollbackEvidence?.Dispose();
            publicationBackup?.Dispose();
            writeTemp?.Dispose();
            canonical?.Dispose();
        }
    }

    internal static ServerOwnedContinuousLearningRegistryAcceptedEvidence CommitPublication(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistrySnapshot candidateArtifact,
        ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState,
        ServerOwnedContinuousLearningRegistryRollbackEvidence expectedRollback,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        HeldRegistryLeaf? canonical = null;
        HeldRegistryLeaf? writeTemp = null;
        HeldRegistryLeaf? publicationBackup = null;
        HeldRegistryLeaf? rollbackEvidence = null;
        HeldRegistryLeaf? acceptedStaging = null;
        HeldRegistryLeaf? acceptedEvidence = null;
        try
        {
            canonical = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry, true, capability);
            writeTemp = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp, true, capability);
            publicationBackup = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup, true, capability);
            rollbackEvidence = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence, true, capability);
            acceptedStaging = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging, true, capability);
            acceptedEvidence = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence, true, capability);
            RevalidateAbsent(root, canonical, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            RevalidateAbsent(root, writeTemp, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp);
            RevalidateAbsent(root, publicationBackup, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            RevalidateAbsent(root, rollbackEvidence, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence);
            RevalidateAbsent(root, acceptedStaging, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging);
            RevalidateAbsent(root, acceptedEvidence, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence);

            if (canonical is null || !canonical.Snapshot.Matches(candidateArtifact) ||
                writeTemp is not null || rollbackEvidence is null ||
                !rollbackEvidence.Snapshot.Matches(expectedRollback.MarkerArtifact) ||
                acceptedStaging is not null || acceptedEvidence is not null)
            {
                throw new InvalidDataException("The pre-commit registry namespace changed.");
            }
            if (priorState.State == ServerOwnedContinuousLearningRegistryCanonicalState.Absent)
            {
                if (publicationBackup is not null)
                    throw new InvalidDataException("An absent prior cannot have a backup.");
            }
            else if (priorState.Snapshot is null || publicationBackup is null ||
                     !publicationBackup.Snapshot.Matches(priorState.Snapshot))
            {
                throw new InvalidDataException("The exact pre-commit backup changed.");
            }

            var descriptor = CreateAcceptedStaging(root, expectedRollback, capability);
            acceptedStaging = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging,
                mutationAccess: true,
                capability);
            if (acceptedStaging is null ||
                acceptedStaging.Snapshot.Identity != descriptor.Identity)
            {
                throw new InvalidDataException("Accepted staging evidence changed.");
            }
            var staged = DecodeAcceptedEvidence(acceptedStaging.Snapshot);
            if (!staged.RollbackEvidence.MarkerArtifact.Matches(
                    expectedRollback.MarkerArtifact))
            {
                throw new InvalidDataException("Accepted staging transaction changed.");
            }
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.AfterAcceptedStagingDurable);

            var commitLeaf = acceptedStaging;
            acceptedStaging = null;
            try
            {
                RenameHeldNoReplace(
                    root,
                    commitLeaf,
                    ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
                    capability);
                capability.ObserveIoStageLocked(
                    ServerOwnedContinuousLearningRegistryIoStage.AfterAcceptedEvidenceRename);
                FlushRoot(root, capability);
                capability.ObserveIoStageLocked(
                    ServerOwnedContinuousLearningRegistryIoStage.AfterAcceptedCommitDurable);
                return DecodeAcceptedEvidence(commitLeaf.Snapshot);
            }
            finally
            {
                commitLeaf.Dispose();
            }
        }
        finally
        {
            acceptedEvidence?.Dispose();
            acceptedStaging?.Dispose();
            rollbackEvidence?.Dispose();
            publicationBackup?.Dispose();
            writeTemp?.Dispose();
            canonical?.Dispose();
        }
    }

    internal static ServerOwnedContinuousLearningRegistryNamespaceInventory ResumeAcceptedCleanup(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryAcceptedEvidence expectedAccepted,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        HeldRegistryLeaf? canonical = null;
        HeldRegistryLeaf? writeTemp = null;
        HeldRegistryLeaf? publicationBackup = null;
        HeldRegistryLeaf? rollbackEvidence = null;
        HeldRegistryLeaf? acceptedStaging = null;
        HeldRegistryLeaf? acceptedEvidence = null;
        try
        {
            canonical = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry, true, capability);
            writeTemp = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp, true, capability);
            publicationBackup = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup, true, capability);
            rollbackEvidence = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence, true, capability);
            acceptedStaging = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging, true, capability);
            acceptedEvidence = OpenHeldOptional(root, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence, true, capability);
            RevalidateAbsent(root, canonical, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            RevalidateAbsent(root, writeTemp, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp);
            RevalidateAbsent(root, publicationBackup, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            RevalidateAbsent(root, rollbackEvidence, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence);
            RevalidateAbsent(root, acceptedStaging, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging);
            RevalidateAbsent(root, acceptedEvidence, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence);

            if (acceptedEvidence is null ||
                !acceptedEvidence.Snapshot.Matches(expectedAccepted.MarkerArtifact))
            {
                throw new InvalidDataException("The exact accepted evidence is unavailable.");
            }
            var exactAccepted = DecodeAcceptedEvidence(acceptedEvidence.Snapshot);
            if (!exactAccepted.RollbackEvidence.MarkerArtifact.Matches(
                    expectedAccepted.RollbackEvidence.MarkerArtifact))
            {
                throw new InvalidDataException("Accepted transaction evidence changed.");
            }
            var transaction = exactAccepted.RollbackEvidence;
            if (canonical is null || !transaction.RejectedArtifactMatches(canonical.Snapshot) ||
                writeTemp is not null || acceptedStaging is not null)
            {
                throw new InvalidDataException("The accepted registry namespace changed.");
            }
            if (rollbackEvidence is not null &&
                !rollbackEvidence.Snapshot.Matches(transaction.MarkerArtifact))
            {
                throw new InvalidDataException("Accepted rollback provenance is foreign.");
            }
            if (publicationBackup is not null &&
                !transaction.PriorArtifactMatches(publicationBackup.Snapshot))
            {
                throw new InvalidDataException("Accepted publication backup is foreign.");
            }
            if (transaction.PriorState ==
                    ServerOwnedContinuousLearningRegistryCanonicalState.Absent &&
                publicationBackup is not null)
            {
                throw new InvalidDataException("Accepted first publication has a backup.");
            }

            FlushHeldExact(canonical, canonical.Snapshot, capability);
            FlushRoot(root, capability);
            if (publicationBackup is not null)
            {
                var backup = publicationBackup;
                publicationBackup = null;
                DeleteHeldExact(backup, backup.Snapshot, capability);
                capability.ObserveIoStageLocked(
                    ServerOwnedContinuousLearningRegistryIoStage.AfterAcceptedBackupDelete);
                FlushRoot(root, capability);
            }
            if (rollbackEvidence is not null)
            {
                var rollback = rollbackEvidence;
                rollbackEvidence = null;
                DeleteHeldExact(rollback, rollback.Snapshot, capability);
                capability.ObserveIoStageLocked(
                    ServerOwnedContinuousLearningRegistryIoStage.AfterAcceptedRollbackDelete);
                FlushRoot(root, capability);
            }
            var accepted = acceptedEvidence;
            acceptedEvidence = null;
            DeleteHeldExact(accepted, accepted.Snapshot, capability);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.AfterAcceptedEvidenceDelete);
            FlushRoot(root, capability);

            var exactCandidate = canonical.Snapshot;
            canonical.Dispose();
            canonical = null;
            var finalInventory = Inventory(root, capability);
            EnsureOnlyCanonicalMatches(finalInventory, exactCandidate);
            return finalInventory;
        }
        finally
        {
            acceptedEvidence?.Dispose();
            acceptedStaging?.Dispose();
            rollbackEvidence?.Dispose();
            publicationBackup?.Dispose();
            writeTemp?.Dispose();
            canonical?.Dispose();
        }
    }

    internal static ServerOwnedContinuousLearningRegistryNamespaceInventory
        RestoreBackupOnly(
            SafeFileHandle root,
            ServerOwnedContinuousLearningRegistrySnapshot priorArtifact,
            ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        HeldRegistryLeaf? canonical = null;
        HeldRegistryLeaf? writeTemp = null;
        HeldRegistryLeaf? publicationBackup = null;
        HeldRegistryLeaf? rollbackEvidence = null;
        HeldRegistryLeaf? acceptedStaging = null;
        HeldRegistryLeaf? acceptedEvidence = null;
        try
        {
            canonical = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                mutationAccess: true,
                capability);
            writeTemp = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                mutationAccess: true,
                capability);
            publicationBackup = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup,
                mutationAccess: true,
                capability);
            rollbackEvidence = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence,
                mutationAccess: true,
                capability);
            acceptedStaging = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging,
                mutationAccess: true,
                capability);
            acceptedEvidence = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
                mutationAccess: true,
                capability);
            RevalidateAbsent(root, canonical, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            RevalidateAbsent(root, writeTemp, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp);
            RevalidateAbsent(root, publicationBackup, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            RevalidateAbsent(root, rollbackEvidence, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence);
            RevalidateAbsent(root, acceptedStaging, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging);
            RevalidateAbsent(root, acceptedEvidence, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence);
            if (canonical is not null || writeTemp is not null ||
                publicationBackup is null ||
                !publicationBackup.Snapshot.Matches(priorArtifact) ||
                rollbackEvidence is not null || acceptedStaging is not null ||
                acceptedEvidence is not null)
            {
                throw new InvalidDataException("The backup-only registry namespace is ambiguous.");
            }

            return BeginRollbackReconstruction(
                root,
                ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(
                    priorArtifact),
                rejectedArtifact: null,
                ref canonical,
                ref writeTemp,
                ref publicationBackup,
                capability).FinalInventory;
        }
        finally
        {
            acceptedEvidence?.Dispose();
            acceptedStaging?.Dispose();
            rollbackEvidence?.Dispose();
            publicationBackup?.Dispose();
            writeTemp?.Dispose();
            canonical?.Dispose();
        }
    }

    internal static ServerOwnedContinuousLearningRegistryReconcileResult
        ReconcileRejectedPublication(
            SafeFileHandle root,
            ServerOwnedContinuousLearningRegistrySnapshot rejectedArtifact,
            ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState,
            ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        HeldRegistryLeaf? canonical = null;
        HeldRegistryLeaf? writeTemp = null;
        HeldRegistryLeaf? publicationBackup = null;
        HeldRegistryLeaf? rollbackEvidence = null;
        HeldRegistryLeaf? acceptedStaging = null;
        HeldRegistryLeaf? acceptedEvidence = null;
        try
        {
            // This is both the required initial full inventory and the retained
            // authority for the mutation. All present leaves remain opened
            // without write/delete sharing until their exact disposition is
            // decided; no later path lookup substitutes a different object.
            canonical = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                mutationAccess: true,
                capability);
            writeTemp = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                mutationAccess: true,
                capability);
            publicationBackup = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup,
                mutationAccess: true,
                capability);
            rollbackEvidence = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence,
                mutationAccess: true,
                capability);
            acceptedStaging = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging,
                mutationAccess: true,
                capability);
            acceptedEvidence = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
                mutationAccess: true,
                capability);

            RevalidateAbsent(root, canonical, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            RevalidateAbsent(root, writeTemp, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp);
            RevalidateAbsent(root, publicationBackup, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            RevalidateAbsent(root, rollbackEvidence, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence);
            RevalidateAbsent(root, acceptedStaging, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging);
            RevalidateAbsent(root, acceptedEvidence, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence);
            _ = CreateInventory(
                canonical,
                writeTemp,
                publicationBackup,
                rollbackEvidence,
                acceptedStaging,
                acceptedEvidence);

            if (acceptedEvidence is not null)
            {
                throw new InvalidDataException(
                    "A committed registry publication cannot be rolled back.");
            }

            if (rollbackEvidence is not null)
            {
                var evidence = DecodeRollbackEvidence(rollbackEvidence.Snapshot);
                var priorMatches = priorState.State == evidence.PriorState &&
                                   (priorState.State ==
                                        ServerOwnedContinuousLearningRegistryCanonicalState.Absent ||
                                    priorState.Snapshot is not null &&
                                    evidence.PriorArtifactMatches(priorState.Snapshot));
                if (!priorMatches || !evidence.RejectedArtifactMatches(rejectedArtifact))
                {
                    throw new InvalidDataException(
                        "Rollback evidence does not match the requested publication.");
                }

                if (acceptedStaging is not null)
                {
                    var stagedEvidence = DecodeAcceptedEvidence(
                        acceptedStaging.Snapshot);
                    if (!stagedEvidence.RollbackEvidence.MarkerArtifact.Matches(
                            evidence.MarkerArtifact))
                    {
                        throw new InvalidDataException(
                            "Accepted staging belongs to a foreign rollback transaction.");
                    }
                    var staging = acceptedStaging;
                    acceptedStaging = null;
                    DeleteHeldExact(staging, staging.Snapshot, capability);
                    FlushRoot(root, capability);
                }

                var alreadyRestored = evidence.PriorState ==
                        ServerOwnedContinuousLearningRegistryCanonicalState.Absent
                    ? canonical is null && writeTemp is null && publicationBackup is null
                    : canonical is not null &&
                      evidence.PriorContentMatches(canonical.Snapshot) &&
                      writeTemp is null;

                rollbackEvidence.Dispose();
                rollbackEvidence = null;
                publicationBackup?.Dispose();
                publicationBackup = null;
                writeTemp?.Dispose();
                writeTemp = null;
                canonical?.Dispose();
                canonical = null;
                var final = ResumeRollbackReconstruction(root, evidence, capability);
                return new(
                    priorState.State ==
                        ServerOwnedContinuousLearningRegistryCanonicalState.Absent
                        ? alreadyRestored
                            ? ServerOwnedContinuousLearningRegistryReconcileDisposition
                                .AlreadyRestoredAbsence
                            : ServerOwnedContinuousLearningRegistryReconcileDisposition
                                .RestoredAbsence
                        : alreadyRestored
                            ? ServerOwnedContinuousLearningRegistryReconcileDisposition
                                .AlreadyRestoredPrior
                            : ServerOwnedContinuousLearningRegistryReconcileDisposition
                                .RestoredPrior,
                    final);
            }

            return priorState.State == ServerOwnedContinuousLearningRegistryCanonicalState.Absent
                ? ReconcileToAbsence(
                    root,
                    rejectedArtifact,
                    ref canonical,
                    ref writeTemp,
                    publicationBackup,
                    capability)
                : ReconcileToPrior(
                    root,
                    rejectedArtifact,
                    priorState.Snapshot!,
                    ref canonical,
                    ref writeTemp,
                    ref publicationBackup,
                    capability);
        }
        finally
        {
            acceptedEvidence?.Dispose();
            acceptedStaging?.Dispose();
            rollbackEvidence?.Dispose();
            publicationBackup?.Dispose();
            writeTemp?.Dispose();
            canonical?.Dispose();
        }
    }

    private static ServerOwnedContinuousLearningRegistryReconcileResult
        ReconcileToAbsence(
            SafeFileHandle root,
            ServerOwnedContinuousLearningRegistrySnapshot rejectedArtifact,
            ref HeldRegistryLeaf? canonical,
            ref HeldRegistryLeaf? writeTemp,
            HeldRegistryLeaf? publicationBackup,
            ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        if (publicationBackup is not null)
            throw new InvalidDataException("An absent prior state cannot have a publication backup.");

        if (canonical is null && writeTemp is null)
        {
            var beforeDurabilityRetry = Inventory(root, capability);
            EnsureAllDataLeavesAbsent(beforeDurabilityRetry);
            FlushRoot(root, capability);
            var alreadyAbsent = Inventory(root, capability);
            EnsureAllDataLeavesAbsent(alreadyAbsent);
            return new(
                ServerOwnedContinuousLearningRegistryReconcileDisposition
                    .AlreadyRestoredAbsence,
                alreadyAbsent);
        }

        if ((canonical is not null &&
             (writeTemp is not null || !canonical.Snapshot.Matches(rejectedArtifact))) ||
            (writeTemp is not null &&
             (canonical is not null || !writeTemp.Snapshot.Matches(rejectedArtifact))))
        {
            throw new InvalidDataException("The registry namespace is ambiguous for absence rollback.");
        }

        return BeginRollbackReconstruction(
            root,
            ServerOwnedContinuousLearningRegistryPriorStateEvidence.Absent(),
            rejectedArtifact,
            ref canonical,
            ref writeTemp,
            ref publicationBackup,
            capability);
    }

    private static ServerOwnedContinuousLearningRegistryReconcileResult
        ReconcileToPrior(
            SafeFileHandle root,
            ServerOwnedContinuousLearningRegistrySnapshot rejectedArtifact,
            ServerOwnedContinuousLearningRegistrySnapshot priorArtifact,
            ref HeldRegistryLeaf? canonical,
            ref HeldRegistryLeaf? writeTemp,
            ref HeldRegistryLeaf? publicationBackup,
            ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var canonicalHasPriorContent =
            canonical?.Snapshot.ContentMatches(priorArtifact) == true;
        var canonicalIsRejected = canonical?.Snapshot.Matches(rejectedArtifact) == true;
        var tempIsRejected = writeTemp?.Snapshot.Matches(rejectedArtifact) == true;
        var tempHasPriorContent =
            writeTemp?.Snapshot.ContentMatches(priorArtifact) == true;
        var backupIsPrior = publicationBackup?.Snapshot.Matches(priorArtifact) == true;

        if ((canonical is not null && !canonicalHasPriorContent && !canonicalIsRejected) ||
            (writeTemp is not null && !tempIsRejected && !tempHasPriorContent) ||
            (publicationBackup is not null && !backupIsPrior))
        {
            throw new InvalidDataException("The registry namespace contains unexpected publication evidence.");
        }

        if (canonicalHasPriorContent && publicationBackup is null)
        {
            if (writeTemp is not null)
            {
                if (!tempIsRejected)
                    throw new InvalidDataException("The restored registry namespace is ambiguous.");
                var rejected = writeTemp;
                writeTemp = null;
                DeleteHeldExact(rejected, rejectedArtifact, capability);
                capability.ObserveIoStageLocked(
                    ServerOwnedContinuousLearningRegistryIoStage.AfterRejectedArtifactDelete);
                FlushRoot(root, capability);
            }

            // A restart may observe the exact prior object after its restore
            // rename but before either durability barrier completed. Retry is
            // successful only after the same retained object and its parent
            // namespace have both been flushed again.
            FlushHeldExact(canonical!, canonical!.Snapshot, capability);
            FlushRoot(root, capability);
            canonical!.Dispose();
            canonical = null;
            var finalInventory = Inventory(root, capability);
            EnsureOnlyCanonicalContentMatches(finalInventory, priorArtifact);
            return new(
                ServerOwnedContinuousLearningRegistryReconcileDisposition
                    .AlreadyRestoredPrior,
                finalInventory);
        }

        if ((publicationBackup is not null && !backupIsPrior) || tempHasPriorContent)
            throw new InvalidDataException("The exact prior publication backup is unavailable.");

        var candidateCount = (canonicalIsRejected ? 1 : 0) + (tempIsRejected ? 1 : 0);
        if (candidateCount > 1 || (candidateCount == 0 && publicationBackup is null))
            throw new InvalidDataException("No exact rejected publication evidence remains.");

        return BeginRollbackReconstruction(
            root,
            ServerOwnedContinuousLearningRegistryPriorStateEvidence.Present(priorArtifact),
            rejectedArtifact,
            ref canonical,
            ref writeTemp,
            ref publicationBackup,
            capability);
    }

    private static ServerOwnedContinuousLearningRegistryReconcileResult
        BeginRollbackReconstruction(
            SafeFileHandle root,
            ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState,
            ServerOwnedContinuousLearningRegistrySnapshot? rejectedArtifact,
            ref HeldRegistryLeaf? canonical,
            ref HeldRegistryLeaf? writeTemp,
            ref HeldRegistryLeaf? publicationBackup,
            ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var descriptor = CreateRollbackEvidence(
            root,
            priorState,
            rejectedArtifact,
            capability);
        var marker = ReadExact(
            root,
            ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence,
            descriptor.Identity,
            capability);
        var evidence = DecodeRollbackEvidence(marker);
        FlushRoot(root, capability);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.AfterRollbackEvidenceDurable);

        publicationBackup?.Dispose();
        publicationBackup = null;
        writeTemp?.Dispose();
        writeTemp = null;
        canonical?.Dispose();
        canonical = null;

        var final = ResumeRollbackReconstruction(root, evidence, capability);
        return new(
            priorState.State == ServerOwnedContinuousLearningRegistryCanonicalState.Absent
                ? ServerOwnedContinuousLearningRegistryReconcileDisposition.RestoredAbsence
                : ServerOwnedContinuousLearningRegistryReconcileDisposition.RestoredPrior,
            final);
    }

    internal static ServerOwnedContinuousLearningRegistryNamespaceInventory
        ResumeRollbackReconstruction(
            SafeFileHandle root,
            ServerOwnedContinuousLearningRegistryRollbackEvidence expectedEvidence,
            ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        HeldRegistryLeaf? canonical = null;
        HeldRegistryLeaf? writeTemp = null;
        HeldRegistryLeaf? publicationBackup = null;
        HeldRegistryLeaf? rollbackEvidence = null;
        HeldRegistryLeaf? acceptedStaging = null;
        HeldRegistryLeaf? acceptedEvidence = null;
        try
        {
            canonical = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                mutationAccess: true,
                capability);
            writeTemp = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                mutationAccess: true,
                capability);
            publicationBackup = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup,
                mutationAccess: true,
                capability);
            rollbackEvidence = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence,
                mutationAccess: true,
                capability);
            acceptedStaging = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging,
                mutationAccess: true,
                capability);
            acceptedEvidence = OpenHeldOptional(
                root,
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence,
                mutationAccess: true,
                capability);
            RevalidateAbsent(root, canonical, ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry);
            RevalidateAbsent(root, writeTemp, ServerOwnedContinuousLearningRegistryLeaf.WriteTemp);
            RevalidateAbsent(root, publicationBackup, ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup);
            RevalidateAbsent(root, rollbackEvidence, ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence);
            RevalidateAbsent(root, acceptedStaging, ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging);
            RevalidateAbsent(root, acceptedEvidence, ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence);

            if (rollbackEvidence is null ||
                !rollbackEvidence.Snapshot.Matches(expectedEvidence.MarkerArtifact))
            {
                throw new InvalidDataException("The exact rollback evidence is unavailable.");
            }
            var evidence = DecodeRollbackEvidence(rollbackEvidence.Snapshot);
            if (acceptedEvidence is not null)
            {
                throw new InvalidDataException(
                    "Committed evidence cannot enter rollback reconstruction.");
            }
            if (acceptedStaging is not null)
            {
                var stagedEvidence = DecodeAcceptedEvidence(acceptedStaging.Snapshot);
                if (!stagedEvidence.RollbackEvidence.MarkerArtifact.Matches(
                        evidence.MarkerArtifact))
                {
                    throw new InvalidDataException(
                        "Accepted staging belongs to a foreign rollback transaction.");
                }
                var staging = acceptedStaging;
                acceptedStaging = null;
                DeleteHeldExact(staging, staging.Snapshot, capability);
                FlushRoot(root, capability);
            }

            var canonicalIsPrior = canonical is not null &&
                                   evidence.PriorContentMatches(canonical.Snapshot);
            var canonicalIsRejected = canonical is not null &&
                                      evidence.RejectedArtifactMatches(canonical.Snapshot);
            var tempIsPrior = writeTemp is not null &&
                              evidence.PriorContentMatches(writeTemp.Snapshot);
            var tempIsRejected = writeTemp is not null &&
                                 evidence.RejectedArtifactMatches(writeTemp.Snapshot);
            var backupIsPrior = publicationBackup is not null &&
                                evidence.PriorArtifactMatches(publicationBackup.Snapshot);

            if ((canonical is not null && !canonicalIsPrior && !canonicalIsRejected) ||
                (writeTemp is not null && !tempIsPrior && !tempIsRejected) ||
                (publicationBackup is not null && !backupIsPrior) ||
                (canonicalIsPrior && writeTemp is not null && !tempIsRejected) ||
                (canonicalIsRejected && tempIsRejected))
            {
                throw new InvalidDataException(
                    "Rollback cross-leaf evidence is malformed or ambiguous.");
            }
            if (evidence.PriorState ==
                ServerOwnedContinuousLearningRegistryCanonicalState.Absent)
            {
                if (publicationBackup is not null || canonicalIsPrior || tempIsPrior ||
                    !evidence.HasRejectedArtifact)
                {
                    throw new InvalidDataException(
                        "Absence rollback contains unexpected prior evidence.");
                }

                if (canonicalIsRejected)
                {
                    var rejected = canonical!;
                    canonical = null;
                    DeleteHeldExact(rejected, rejected.Snapshot, capability);
                    capability.ObserveIoStageLocked(
                        ServerOwnedContinuousLearningRegistryIoStage
                            .AfterRejectedArtifactDelete);
                    FlushRoot(root, capability);
                }
                else if (tempIsRejected)
                {
                    var rejected = writeTemp!;
                    writeTemp = null;
                    DeleteHeldExact(rejected, rejected.Snapshot, capability);
                    capability.ObserveIoStageLocked(
                        ServerOwnedContinuousLearningRegistryIoStage
                            .AfterRejectedArtifactDelete);
                    FlushRoot(root, capability);
                }
                else if (canonical is not null || writeTemp is not null)
                {
                    throw new InvalidDataException("Absence rollback evidence is ambiguous.");
                }
                else
                {
                    // The rejected leaf was already deleted. Retry its parent
                    // namespace durability before removing the durable marker.
                    FlushRoot(root, capability);
                }

                var marker = rollbackEvidence;
                rollbackEvidence = null;
                DeleteHeldExact(marker, marker.Snapshot, capability);
                FlushRoot(root, capability);
                var absent = Inventory(root, capability);
                EnsureAllDataLeavesAbsent(absent);
                return absent;
            }

            if (evidence.PriorState !=
                    ServerOwnedContinuousLearningRegistryCanonicalState.Present ||
                evidence.PriorLength <= 0)
            {
                throw new InvalidDataException("Rollback prior evidence is unavailable.");
            }
            if ((!evidence.HasRejectedArtifact &&
                 (canonicalIsRejected || tempIsRejected)) ||
                (canonical is null && writeTemp is null && publicationBackup is null))
            {
                throw new InvalidDataException(
                    "Prior rollback has no reachable reconstruction source.");
            }

            if (canonicalIsPrior)
            {
                if (tempIsRejected)
                {
                    var rejected = writeTemp!;
                    writeTemp = null;
                    DeleteHeldExact(rejected, rejected.Snapshot, capability);
                    capability.ObserveIoStageLocked(
                        ServerOwnedContinuousLearningRegistryIoStage
                            .AfterRejectedArtifactDelete);
                    FlushRoot(root, capability);
                }
                FlushHeldExact(canonical!, canonical!.Snapshot, capability);
                FlushRoot(root, capability);
                capability.ObserveIoStageLocked(
                    ServerOwnedContinuousLearningRegistryIoStage
                        .AfterRollbackCanonicalDurable);
            }
            else
            {
                if (tempIsRejected)
                {
                    var rejected = writeTemp!;
                    writeTemp = null;
                    DeleteHeldExact(rejected, rejected.Snapshot, capability);
                    capability.ObserveIoStageLocked(
                        ServerOwnedContinuousLearningRegistryIoStage
                            .AfterRejectedArtifactDelete);
                    FlushRoot(root, capability);
                    tempIsPrior = false;
                }

                if (!tempIsPrior)
                {
                    if (writeTemp is not null)
                        throw new InvalidDataException("Rollback working copy is ambiguous.");
                    var descriptor = CreateWriteTemp(
                        root,
                        evidence.CopyPriorBytes(),
                        capability);
                    writeTemp = OpenHeldOptional(
                        root,
                        ServerOwnedContinuousLearningRegistryLeaf.WriteTemp,
                        mutationAccess: true,
                        capability);
                    if (writeTemp is null ||
                        writeTemp.Snapshot.Identity != descriptor.Identity ||
                        !evidence.PriorContentMatches(writeTemp.Snapshot))
                    {
                        throw new InvalidDataException("Rollback working copy changed.");
                    }
                    FlushRoot(root, capability);
                    capability.ObserveIoStageLocked(
                        ServerOwnedContinuousLearningRegistryIoStage
                            .AfterRollbackWorkingCopyDurable);
                }

                if (canonicalIsRejected)
                {
                    var rejected = canonical!;
                    canonical = null;
                    DeleteHeldExact(rejected, rejected.Snapshot, capability);
                    capability.ObserveIoStageLocked(
                        ServerOwnedContinuousLearningRegistryIoStage
                            .AfterRejectedArtifactDelete);
                    FlushRoot(root, capability);
                }
                else if (canonical is not null)
                {
                    throw new InvalidDataException("Rollback canonical state is ambiguous.");
                }

                var priorWorkingCopy = writeTemp!;
                writeTemp = null;
                try
                {
                    RenameHeldNoReplace(
                        root,
                        priorWorkingCopy,
                        ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry,
                        capability);
                    capability.ObserveIoStageLocked(
                        ServerOwnedContinuousLearningRegistryIoStage.AfterPriorArtifactRestore);
                    FlushHeldExact(priorWorkingCopy, priorWorkingCopy.Snapshot, capability);
                    FlushRoot(root, capability);
                    capability.ObserveIoStageLocked(
                        ServerOwnedContinuousLearningRegistryIoStage
                            .AfterRollbackCanonicalDurable);
                }
                finally
                {
                    priorWorkingCopy.Dispose();
                }
            }

            if (publicationBackup is not null)
            {
                var backup = publicationBackup;
                publicationBackup = null;
                DeleteHeldExact(backup, backup.Snapshot, capability);
                FlushRoot(root, capability);
            }

            var exactMarker = rollbackEvidence;
            rollbackEvidence = null;
            DeleteHeldExact(exactMarker, exactMarker.Snapshot, capability);
            FlushRoot(root, capability);
            canonical?.Dispose();
            canonical = null;
            var finalInventory = Inventory(root, capability);
            EnsureOnlyCanonicalContentMatches(finalInventory, evidence);
            return finalInventory;
        }
        finally
        {
            acceptedEvidence?.Dispose();
            acceptedStaging?.Dispose();
            rollbackEvidence?.Dispose();
            publicationBackup?.Dispose();
            writeTemp?.Dispose();
            canonical?.Dispose();
        }
    }

    private static void DeleteHeldExact(
        HeldRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistrySnapshot expected,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var maximumLength = MaximumLeafBytes(leaf.Leaf);
        if (!leaf.Snapshot.Matches(expected) ||
            !ReadHeldExact(leaf.Handle, expected.Identity, maximumLength).Matches(expected))
        {
            throw new InvalidDataException("The rejected registry artifact changed.");
        }

        try
        {
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.BeforeExactDelete);
            ServerOwnedContinuousLearningCacheRootNative.MarkNamedRegularLeafDeleteOnClose(
                leaf.Root,
                LeafName(leaf.Leaf),
                leaf.Handle);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.AfterExactDelete);
        }
        finally
        {
            leaf.Dispose();
        }
    }

    private static void RenameHeldNoReplace(
        SafeFileHandle root,
        HeldRegistryLeaf source,
        ServerOwnedContinuousLearningRegistryLeaf destination,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var maximumLength = MaximumLeafBytes(source.Leaf);
        var before = Describe(source.Handle, maximumLength);
        if (before.Identity != source.Snapshot.Identity ||
            before.Length != source.Snapshot.Length)
        {
            throw new InvalidDataException("The registry restore source changed.");
        }

        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.BeforeNoReplaceRename);
        capability.ThrowIfNativeCallFailsForTestingLocked(
            ServerOwnedContinuousLearningRegistryNativeCall.NoReplaceRename);
        ServerOwnedContinuousLearningCacheRootNative.RenameNamedLeafNoReplace(
            root,
            source.Handle,
            LeafName(source.Leaf),
            LeafName(destination));
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.AfterNoReplaceRename);
        if (Describe(source.Handle, maximumLength) != before)
            throw new InvalidDataException("The restored registry identity changed during rename.");
    }

    private static void FlushHeldExact(
        HeldRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistrySnapshot expected,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var maximumLength = MaximumLeafBytes(leaf.Leaf);
        if (!ReadHeldExact(leaf.Handle, expected.Identity, maximumLength).Matches(expected))
            throw new InvalidDataException("The registry flush target changed.");
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.BeforeFileFlush);
        capability.ThrowIfNativeCallFailsForTestingLocked(
            ServerOwnedContinuousLearningRegistryNativeCall.FileFlush);
        ServerOwnedContinuousLearningCacheRootNative.FlushNamedRegularLeaf(leaf.Handle);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.AfterFileFlush);
        if (!ReadHeldExact(leaf.Handle, expected.Identity, maximumLength).Matches(expected))
            throw new InvalidDataException("The registry flush target changed.");
    }

    private static HeldRegistryLeaf? OpenHeldOptional(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        bool mutationAccess,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var maximumLength = MaximumLeafBytes(leaf);
        var handle = ServerOwnedContinuousLearningCacheRootNative.TryOpenNamedRegularLeaf(
             root,
             LeafName(leaf),
             mutationAccess,
             maximumLength);
        if (handle is null)
            return null;

        try
        {
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.AfterReadOpenBeforeContentRead);
            var descriptor = Describe(handle, maximumLength);
            if (descriptor.Length == 0)
                throw new InvalidDataException("A fixed registry data leaf is empty.");
            return new HeldRegistryLeaf(
                root,
                leaf,
                handle,
                ReadHeldExact(handle, descriptor.Identity, maximumLength));
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    private static void RevalidateAbsent(
        SafeFileHandle root,
        HeldRegistryLeaf? held,
        ServerOwnedContinuousLearningRegistryLeaf leaf)
    {
        if (held is not null)
            return;
        using var probe = ServerOwnedContinuousLearningCacheRootNative
            .TryOpenNamedRegularLeaf(
                root,
                LeafName(leaf),
                mutationAccess: false,
                MaximumLeafBytes(leaf));
        if (probe is not null)
            throw new InvalidDataException("The fixed registry namespace changed during inventory.");
    }

    private static ServerOwnedContinuousLearningRegistryNamespaceInventory CreateInventory(
        HeldRegistryLeaf? canonical,
        HeldRegistryLeaf? writeTemp,
        HeldRegistryLeaf? publicationBackup,
        HeldRegistryLeaf? rollbackEvidence,
        HeldRegistryLeaf? acceptedStaging,
        HeldRegistryLeaf? acceptedEvidence)
        => new(
            Observe(ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry, canonical),
            Observe(ServerOwnedContinuousLearningRegistryLeaf.WriteTemp, writeTemp),
            Observe(ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup, publicationBackup),
            Observe(ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence, rollbackEvidence),
            Observe(ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging, acceptedStaging),
            Observe(ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence, acceptedEvidence));

    private static ServerOwnedContinuousLearningRegistryLeafObservation Observe(
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        HeldRegistryLeaf? held)
        => held is null
            ? ServerOwnedContinuousLearningRegistryLeafObservation.Absent(leaf)
            : ServerOwnedContinuousLearningRegistryLeafObservation.Present(leaf, held.Snapshot);

    private static void EnsureAllDataLeavesAbsent(
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory)
    {
        if (inventory.CanonicalRegistry.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.WriteTemp.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.PublicationBackup.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.RollbackEvidence.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedStaging.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedEvidence.State != ServerOwnedContinuousLearningRegistryLeafState.Absent)
        {
            throw new InvalidDataException("Registry absence was not restored exactly.");
        }
    }

    private static void EnsureOnlyCanonicalMatches(
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory,
        ServerOwnedContinuousLearningRegistrySnapshot expected)
    {
        if (!inventory.CanonicalRegistry.Matches(expected) ||
            inventory.WriteTemp.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.PublicationBackup.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.RollbackEvidence.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedStaging.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedEvidence.State != ServerOwnedContinuousLearningRegistryLeafState.Absent)
        {
            throw new InvalidDataException("The prior registry state was not restored exactly.");
        }
    }

    private static void EnsureOnlyCanonicalContentMatches(
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory,
        ServerOwnedContinuousLearningRegistrySnapshot expected)
    {
        if (inventory.CanonicalRegistry.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Present ||
            inventory.CanonicalRegistry.Snapshot is null ||
            !inventory.CanonicalRegistry.Snapshot.ContentMatches(expected) ||
            inventory.WriteTemp.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.PublicationBackup.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.RollbackEvidence.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedStaging.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedEvidence.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent)
        {
            throw new InvalidDataException("The prior registry bytes were not restored exactly.");
        }
    }

    private static void EnsureOnlyCanonicalContentMatches(
        ServerOwnedContinuousLearningRegistryNamespaceInventory inventory,
        ServerOwnedContinuousLearningRegistryRollbackEvidence expected)
    {
        if (inventory.CanonicalRegistry.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Present ||
            inventory.CanonicalRegistry.Snapshot is null ||
            !expected.PriorContentMatches(inventory.CanonicalRegistry.Snapshot) ||
            inventory.WriteTemp.State != ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.PublicationBackup.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.RollbackEvidence.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedStaging.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent ||
            inventory.AcceptedEvidence.State !=
                ServerOwnedContinuousLearningRegistryLeafState.Absent)
        {
            throw new InvalidDataException("The rollback prior bytes were not restored exactly.");
        }
    }

    internal static ServerOwnedContinuousLearningRegistryCanonicalRead ReadCanonical(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var handle = ServerOwnedContinuousLearningCacheRootNative
            .TryOpenNamedRegularLeaf(
                root,
                CanonicalRegistryLeafName,
                mutationAccess: false,
                DomainModelRegistry.MaximumRegistryBytes);
        if (handle is null)
            return ServerOwnedContinuousLearningRegistryCanonicalRead.Absent();

        using (handle)
        {
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage
                    .AfterReadOpenBeforeContentRead);
            var descriptor = Describe(
                handle,
                DomainModelRegistry.MaximumRegistryBytes);
            if (descriptor.Length == 0)
                throw new InvalidDataException("The canonical registry leaf is empty.");
            return ServerOwnedContinuousLearningRegistryCanonicalRead.Present(
                ReadHeldExact(
                    handle,
                    descriptor.Identity,
                    DomainModelRegistry.MaximumRegistryBytes));
        }
    }

    private static ServerOwnedContinuousLearningRegistryFileDescriptor
        CreateRollbackEvidence(
            SafeFileHandle root,
            ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState,
            ServerOwnedContinuousLearningRegistrySnapshot? rejectedArtifact,
            ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var bytes = EncodeRollbackEvidence(priorState, rejectedArtifact);
        using var handle = ServerOwnedContinuousLearningCacheRootNative
            .CreateNamedWriteLeaf(
                root,
                RollbackEvidenceLeafName,
                ServerOwnedContinuousLearningRegistryRollbackEvidence.MaximumEncodedBytes);
        var ownedIdentity = Describe(
            handle,
            ServerOwnedContinuousLearningRegistryRollbackEvidence.MaximumEncodedBytes).Identity;
        var complete = false;
        try
        {
            RandomAccess.Write(handle, bytes, 0);
            RandomAccess.SetLength(handle, bytes.Length);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.BeforeFileFlush);
            capability.ThrowIfNativeCallFailsForTestingLocked(
                ServerOwnedContinuousLearningRegistryNativeCall.FileFlush);
            ServerOwnedContinuousLearningCacheRootNative.FlushNamedRegularLeaf(handle);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.AfterFileFlush);
            var descriptor = Describe(
                handle,
                ServerOwnedContinuousLearningRegistryRollbackEvidence.MaximumEncodedBytes);
            if (descriptor.Identity != ownedIdentity || descriptor.Length != bytes.Length)
                throw new InvalidDataException("Rollback evidence changed during creation.");
            complete = true;
            return descriptor;
        }
        finally
        {
            if (!complete)
            {
                try
                {
                    if (Describe(
                            handle,
                            ServerOwnedContinuousLearningRegistryRollbackEvidence
                                .MaximumEncodedBytes).Identity == ownedIdentity)
                    {
                        ServerOwnedContinuousLearningCacheRootNative
                            .MarkNamedRegularLeafDeleteOnClose(
                                root,
                                RollbackEvidenceLeafName,
                                handle);
                    }
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    // Never delete a replacement object by name.
                }
            }
        }
    }

    private static ServerOwnedContinuousLearningRegistryFileDescriptor
        CreateAcceptedStaging(
            SafeFileHandle root,
            ServerOwnedContinuousLearningRegistryRollbackEvidence rollbackEvidence,
            ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var bytes = EncodeAcceptedEvidence(rollbackEvidence);
        using var handle = ServerOwnedContinuousLearningCacheRootNative
            .CreateNamedWriteLeaf(
                root,
                AcceptedStagingLeafName,
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.MaximumEncodedBytes);
        var ownedIdentity = Describe(
            handle,
            ServerOwnedContinuousLearningRegistryAcceptedEvidence.MaximumEncodedBytes).Identity;
        var complete = false;
        try
        {
            RandomAccess.Write(handle, bytes, 0);
            RandomAccess.SetLength(handle, bytes.Length);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.BeforeFileFlush);
            capability.ThrowIfNativeCallFailsForTestingLocked(
                ServerOwnedContinuousLearningRegistryNativeCall.FileFlush);
            ServerOwnedContinuousLearningCacheRootNative.FlushNamedRegularLeaf(handle);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.AfterFileFlush);
            var descriptor = Describe(
                handle,
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.MaximumEncodedBytes);
            if (descriptor.Identity != ownedIdentity || descriptor.Length != bytes.Length)
                throw new InvalidDataException("Accepted staging changed during creation.");
            complete = true;
            return descriptor;
        }
        finally
        {
            if (!complete)
            {
                try
                {
                    if (Describe(
                            handle,
                            ServerOwnedContinuousLearningRegistryAcceptedEvidence
                                .MaximumEncodedBytes).Identity == ownedIdentity)
                    {
                        ServerOwnedContinuousLearningCacheRootNative
                            .MarkNamedRegularLeafDeleteOnClose(
                                root,
                                AcceptedStagingLeafName,
                                handle);
                    }
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    // Never delete a replacement object by name.
                }
            }
        }
    }

    internal static ServerOwnedContinuousLearningRegistryAcceptedEvidence
        DecodeAcceptedEvidence(
            ServerOwnedContinuousLearningRegistrySnapshot markerArtifact)
    {
        var bytes = markerArtifact.CopyBytes();
        if (bytes.Length <
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.EncodedHeaderLength +
                ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength ||
            BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(0, 8)) !=
                AcceptedEvidenceMagic ||
            BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(8, 4)) !=
                AcceptedEvidenceVersion ||
            BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(12, 4)) != 0)
        {
            throw new InvalidDataException("Accepted evidence framing is invalid.");
        }

        var rollbackLength = BinaryPrimitives.ReadInt64BigEndian(bytes.AsSpan(16, 8));
        var rollbackHash = ReadContentHash(bytes.AsSpan(24, 32));
        var rollbackIdentity = ReadIdentity(bytes.AsSpan(56, 24));
        var expectedLength = checked(
            ServerOwnedContinuousLearningRegistryAcceptedEvidence.EncodedHeaderLength +
            rollbackLength);
        if (rollbackLength <
                ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength ||
            rollbackLength >
                ServerOwnedContinuousLearningRegistryRollbackEvidence.MaximumEncodedBytes ||
            expectedLength != bytes.LongLength ||
            !IsNonzeroIdentity(rollbackIdentity))
        {
            throw new InvalidDataException("Accepted evidence bounds are invalid.");
        }

        var rollbackBytes =
            bytes[ServerOwnedContinuousLearningRegistryAcceptedEvidence.EncodedHeaderLength..];
        if (ServerOwnedContinuousLearningRegistryContentHash.FromBytes(rollbackBytes) !=
            rollbackHash)
        {
            throw new InvalidDataException("Accepted rollback evidence failed integrity validation.");
        }
        var rollbackArtifact = new ServerOwnedContinuousLearningRegistrySnapshot(
            rollbackIdentity,
            rollbackBytes);
        return new(
            markerArtifact,
            DecodeRollbackEvidence(rollbackArtifact));
    }

    private static byte[] EncodeAcceptedEvidence(
        ServerOwnedContinuousLearningRegistryRollbackEvidence rollbackEvidence)
    {
        var rollback = rollbackEvidence.MarkerArtifact;
        var rollbackBytes = rollback.CopyBytes();
        var encoded = new byte[checked(
            ServerOwnedContinuousLearningRegistryAcceptedEvidence.EncodedHeaderLength +
            rollbackBytes.Length)];
        BinaryPrimitives.WriteUInt64BigEndian(encoded.AsSpan(0, 8), AcceptedEvidenceMagic);
        BinaryPrimitives.WriteUInt32BigEndian(encoded.AsSpan(8, 4), AcceptedEvidenceVersion);
        BinaryPrimitives.WriteInt64BigEndian(encoded.AsSpan(16, 8), rollback.Length);
        WriteContentHash(encoded.AsSpan(24, 32), rollback.Sha256);
        WriteIdentity(encoded.AsSpan(56, 24), rollback.Identity);
        rollbackBytes.CopyTo(
            encoded.AsSpan(
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.EncodedHeaderLength));
        return encoded;
    }

    internal static ServerOwnedContinuousLearningRegistryRollbackEvidence
        DecodeRollbackEvidence(
            ServerOwnedContinuousLearningRegistrySnapshot markerArtifact)
    {
        var bytes = markerArtifact.CopyBytes();
        if (bytes.Length <
                ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength ||
            BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(0, 8)) !=
                RollbackEvidenceMagic ||
            BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(8, 4)) !=
                RollbackEvidenceVersion)
        {
            throw new InvalidDataException("Rollback evidence framing is invalid.");
        }

        var flags = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(12, 4));
        if ((flags & ~(RollbackPriorPresentFlag | RollbackRejectedPresentFlag)) != 0)
            throw new InvalidDataException("Rollback evidence flags are invalid.");
        var priorPresent = (flags & RollbackPriorPresentFlag) != 0;
        var rejectedPresent = (flags & RollbackRejectedPresentFlag) != 0;
        if (!priorPresent && !rejectedPresent)
            throw new InvalidDataException("Rollback evidence has no transaction state.");

        var priorLength = BinaryPrimitives.ReadInt64BigEndian(bytes.AsSpan(16, 8));
        var rejectedLength = BinaryPrimitives.ReadInt64BigEndian(bytes.AsSpan(24, 8));
        var priorHash = ReadContentHash(bytes.AsSpan(32, 32));
        var rejectedHash = ReadContentHash(bytes.AsSpan(64, 32));
        var priorIdentity = ReadIdentity(bytes.AsSpan(96, 24));
        var rejectedIdentity = ReadIdentity(bytes.AsSpan(120, 24));
        var expectedLength = checked(
            ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength +
            priorLength);
        if (priorLength < 0 || priorLength > DomainModelRegistry.MaximumRegistryBytes ||
            rejectedLength < 0 || rejectedLength > DomainModelRegistry.MaximumRegistryBytes ||
            expectedLength != bytes.LongLength ||
            priorPresent != (priorLength > 0) ||
            rejectedPresent != (rejectedLength > 0) ||
            (priorPresent
                ? !IsNonzeroIdentity(priorIdentity)
                : priorIdentity != default) ||
            (rejectedPresent
                ? !IsNonzeroIdentity(rejectedIdentity)
                : rejectedIdentity != default))
        {
            throw new InvalidDataException("Rollback evidence bounds are invalid.");
        }
        if (!priorPresent && priorHash != default ||
            !rejectedPresent && rejectedHash != default)
        {
            throw new InvalidDataException("Rollback absence evidence is non-canonical.");
        }

        var priorBytes = priorPresent
            ? bytes[ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength..]
            : [];
        if (priorPresent &&
            ServerOwnedContinuousLearningRegistryContentHash.FromBytes(priorBytes) != priorHash)
        {
            throw new InvalidDataException("Rollback prior bytes failed integrity validation.");
        }

        return new ServerOwnedContinuousLearningRegistryRollbackEvidence(
            markerArtifact,
            priorPresent
                ? ServerOwnedContinuousLearningRegistryCanonicalState.Present
                : ServerOwnedContinuousLearningRegistryCanonicalState.Absent,
            priorIdentity,
            priorHash,
            priorBytes,
            rejectedPresent,
            rejectedIdentity,
            rejectedLength,
            rejectedHash);
    }

    private static byte[] EncodeRollbackEvidence(
        ServerOwnedContinuousLearningRegistryPriorStateEvidence priorState,
        ServerOwnedContinuousLearningRegistrySnapshot? rejectedArtifact)
    {
        var prior = priorState.Snapshot;
        var priorPresent = priorState.State ==
                           ServerOwnedContinuousLearningRegistryCanonicalState.Present;
        if (priorPresent != (prior is not null) || !priorPresent && rejectedArtifact is null)
            throw new InvalidDataException("Rollback evidence inputs are inconsistent.");
        var priorBytes = prior?.CopyBytes() ?? [];
        var encoded = new byte[checked(
            ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength +
            priorBytes.Length)];
        BinaryPrimitives.WriteUInt64BigEndian(encoded.AsSpan(0, 8), RollbackEvidenceMagic);
        BinaryPrimitives.WriteUInt32BigEndian(encoded.AsSpan(8, 4), RollbackEvidenceVersion);
        var flags = (priorPresent ? RollbackPriorPresentFlag : 0) |
                    (rejectedArtifact is not null ? RollbackRejectedPresentFlag : 0);
        BinaryPrimitives.WriteUInt32BigEndian(encoded.AsSpan(12, 4), flags);
        BinaryPrimitives.WriteInt64BigEndian(encoded.AsSpan(16, 8), prior?.Length ?? 0);
        BinaryPrimitives.WriteInt64BigEndian(
            encoded.AsSpan(24, 8),
            rejectedArtifact?.Length ?? 0);
        WriteContentHash(encoded.AsSpan(32, 32), prior?.Sha256 ?? default);
        WriteContentHash(encoded.AsSpan(64, 32), rejectedArtifact?.Sha256 ?? default);
        WriteIdentity(encoded.AsSpan(96, 24), prior?.Identity ?? default);
        WriteIdentity(encoded.AsSpan(120, 24), rejectedArtifact?.Identity ?? default);
        priorBytes.CopyTo(
            encoded.AsSpan(
                ServerOwnedContinuousLearningRegistryRollbackEvidence.EncodedHeaderLength));
        return encoded;
    }

    private static void WriteContentHash(
        Span<byte> destination,
        ServerOwnedContinuousLearningRegistryContentHash hash)
    {
        BinaryPrimitives.WriteUInt64BigEndian(destination[0..8], hash.Word0);
        BinaryPrimitives.WriteUInt64BigEndian(destination[8..16], hash.Word1);
        BinaryPrimitives.WriteUInt64BigEndian(destination[16..24], hash.Word2);
        BinaryPrimitives.WriteUInt64BigEndian(destination[24..32], hash.Word3);
    }

    private static ServerOwnedContinuousLearningRegistryContentHash ReadContentHash(
        ReadOnlySpan<byte> source)
        => new(
            BinaryPrimitives.ReadUInt64BigEndian(source[0..8]),
            BinaryPrimitives.ReadUInt64BigEndian(source[8..16]),
            BinaryPrimitives.ReadUInt64BigEndian(source[16..24]),
            BinaryPrimitives.ReadUInt64BigEndian(source[24..32]));

    private static void WriteIdentity(
        Span<byte> destination,
        ServerOwnedContinuousLearningRegistryFileIdentity identity)
    {
        BinaryPrimitives.WriteUInt64BigEndian(destination[0..8], identity.VolumeSerialNumber);
        BinaryPrimitives.WriteUInt64BigEndian(destination[8..16], identity.FileIdLow);
        BinaryPrimitives.WriteUInt64BigEndian(destination[16..24], identity.FileIdHigh);
    }

    private static ServerOwnedContinuousLearningRegistryFileIdentity ReadIdentity(
        ReadOnlySpan<byte> source)
        => new(
            BinaryPrimitives.ReadUInt64BigEndian(source[0..8]),
            BinaryPrimitives.ReadUInt64BigEndian(source[8..16]),
            BinaryPrimitives.ReadUInt64BigEndian(source[16..24]));

    private static bool IsNonzeroIdentity(
        ServerOwnedContinuousLearningRegistryFileIdentity identity)
        => identity.VolumeSerialNumber != 0 &&
           (identity.FileIdLow != 0 || identity.FileIdHigh != 0);

    internal static ServerOwnedContinuousLearningRegistryFileDescriptor CreateWriteTemp(
        SafeFileHandle root,
        ReadOnlyMemory<byte> bytes,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        if (bytes.IsEmpty || bytes.Length > DomainModelRegistry.MaximumRegistryBytes)
            throw new InvalidDataException("The registry write exceeds its fixed bound.");

        using var handle = ServerOwnedContinuousLearningCacheRootNative
            .CreateNamedWriteLeaf(
                root,
                WriteTempLeafName,
                DomainModelRegistry.MaximumRegistryBytes);
        var ownedIdentity = Describe(
            handle,
            DomainModelRegistry.MaximumRegistryBytes).Identity;
        var complete = false;
        try
        {
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage
                    .AfterWriteTempOpenBeforeGrowth);
            if (!bytes.IsEmpty)
                RandomAccess.Write(handle, bytes.Span, 0);
            RandomAccess.SetLength(handle, bytes.Length);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.BeforeFileFlush);
            capability.ThrowIfNativeCallFailsForTestingLocked(
                ServerOwnedContinuousLearningRegistryNativeCall.FileFlush);
            ServerOwnedContinuousLearningCacheRootNative.FlushNamedRegularLeaf(handle);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningRegistryIoStage.AfterFileFlush);
            var descriptor = Describe(
                handle,
                DomainModelRegistry.MaximumRegistryBytes);
            if (descriptor.Identity != ownedIdentity || descriptor.Length != bytes.Length)
                throw new InvalidDataException("The registry write temporary leaf changed.");
            complete = true;
            return descriptor;
        }
        finally
        {
            if (!complete)
            {
                try
                {
                    if (Describe(
                            handle,
                            DomainModelRegistry.MaximumRegistryBytes).Identity == ownedIdentity)
                    {
                        ServerOwnedContinuousLearningCacheRootNative
                            .MarkNamedRegularLeafDeleteOnClose(
                                root,
                                WriteTempLeafName,
                                handle);
                    }
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    // The exact opened object was not proven; never fall back
                    // to deleting by path.
                }
            }
        }
    }

    internal static ServerOwnedContinuousLearningRegistrySnapshot CreateCanonicalIfAbsent(
        SafeFileHandle root,
        ReadOnlyMemory<byte> bytes,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        if (bytes.IsEmpty || bytes.Length > DomainModelRegistry.MaximumRegistryBytes)
            throw new InvalidDataException("The canonical registry write exceeds its fixed bound.");

        using var handle = ServerOwnedContinuousLearningCacheRootNative
            .CreateNamedWriteLeaf(
                root,
                CanonicalRegistryLeafName,
                DomainModelRegistry.MaximumRegistryBytes);
        var ownedIdentity = Describe(
            handle,
            DomainModelRegistry.MaximumRegistryBytes).Identity;

        // Это единственная portable POSIX registry publication: prior должен
        // быть absent, final leaf создаётся сразу no-replace/no-follow open, и
        // доверие рождается только после полного byte/readback/root-fsync цикла.
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage
                .AfterCanonicalCreateOnlyOpenBeforeGrowth);
        if (!bytes.IsEmpty)
            RandomAccess.Write(handle, bytes.Span, 0);
        RandomAccess.SetLength(handle, bytes.Length);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.BeforeFileFlush);
        capability.ThrowIfNativeCallFailsForTestingLocked(
            ServerOwnedContinuousLearningRegistryNativeCall.FileFlush);
        ServerOwnedContinuousLearningCacheRootNative.FlushNamedRegularLeaf(handle);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.AfterFileFlush);
        var descriptor = Describe(
            handle,
            DomainModelRegistry.MaximumRegistryBytes);
        if (descriptor.Identity != ownedIdentity || descriptor.Length != bytes.Length)
            throw new InvalidDataException("The canonical registry create-only leaf changed.");

        var snapshot = ReadHeldExact(
            handle,
            descriptor.Identity,
            DomainModelRegistry.MaximumRegistryBytes);
        if (!snapshot.CopyBytes().AsSpan().SequenceEqual(bytes.Span))
            throw new InvalidDataException("The canonical registry bytes changed after write.");

        FlushRoot(root, capability);
        var durable = ReadHeldExact(
            handle,
            descriptor.Identity,
            DomainModelRegistry.MaximumRegistryBytes);
        if (!durable.Matches(snapshot))
            throw new InvalidDataException("The canonical registry bytes changed after root flush.");
        return durable;
    }

    internal static ServerOwnedContinuousLearningRegistrySnapshot ReadExact(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var maximumLength = MaximumLeafBytes(leaf);
        using var handle = ServerOwnedContinuousLearningCacheRootNative
            .OpenNamedRegularLeaf(
                root,
                LeafName(leaf),
                mutationAccess: false,
                maximumLength);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.AfterReadOpenBeforeContentRead);
        return ReadHeldExact(handle, expectedIdentity, maximumLength);
    }

    internal static ServerOwnedContinuousLearningRegistryFileDescriptor QueryIdentity(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryLeaf leaf)
    {
        var maximumLength = MaximumLeafBytes(leaf);
        using var handle = ServerOwnedContinuousLearningCacheRootNative
            .OpenNamedRegularLeaf(
                root,
                LeafName(leaf),
                mutationAccess: false,
                maximumLength);
        return Describe(handle, maximumLength);
    }

    internal static void ValidateIdentity(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity)
    {
        if (QueryIdentity(root, leaf).Identity != expectedIdentity)
            throw new InvalidDataException("The fixed registry leaf identity changed.");
    }

    internal static void RenameNoReplace(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryLeaf source,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedSourceIdentity,
        ServerOwnedContinuousLearningRegistryLeaf destination,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var maximumLength = MaximumLeafBytes(source);
        using var handle = ServerOwnedContinuousLearningCacheRootNative
            .OpenNamedRegularLeaf(
                root,
                LeafName(source),
                mutationAccess: true,
                maximumLength);
        var before = Describe(handle, maximumLength);
        if (before.Identity != expectedSourceIdentity)
            throw new InvalidDataException("The registry rename source identity changed.");
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.BeforeNoReplaceRename);
        capability.ThrowIfNativeCallFailsForTestingLocked(
            ServerOwnedContinuousLearningRegistryNativeCall.NoReplaceRename);
        ServerOwnedContinuousLearningCacheRootNative.RenameNamedLeafNoReplace(
            root,
            handle,
            LeafName(source),
            LeafName(destination));
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.AfterNoReplaceRename);
        var after = Describe(handle, maximumLength);
        if (after.Identity != before.Identity || after.Length != before.Length)
            throw new InvalidDataException("The renamed registry leaf identity changed.");
    }

    internal static void DeleteExact(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var maximumLength = MaximumLeafBytes(leaf);
        using var handle = ServerOwnedContinuousLearningCacheRootNative
            .OpenNamedRegularLeaf(
                root,
                LeafName(leaf),
                mutationAccess: true,
                maximumLength);
        if (Describe(handle, maximumLength).Identity != expectedIdentity)
            throw new InvalidDataException("The registry cleanup target identity changed.");
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.BeforeExactDelete);
        ServerOwnedContinuousLearningCacheRootNative.MarkNamedRegularLeafDeleteOnClose(
            root,
            LeafName(leaf),
            handle);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.AfterExactDelete);
    }

    internal static void FlushFile(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryLeaf leaf,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        var maximumLength = MaximumLeafBytes(leaf);
        using var handle = ServerOwnedContinuousLearningCacheRootNative
            .OpenNamedRegularLeaf(
                root,
                LeafName(leaf),
                mutationAccess: true,
                maximumLength);
        var before = Describe(handle, maximumLength);
        if (before.Identity != expectedIdentity)
            throw new InvalidDataException("The registry flush target identity changed.");
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.BeforeFileFlush);
        capability.ThrowIfNativeCallFailsForTestingLocked(
            ServerOwnedContinuousLearningRegistryNativeCall.FileFlush);
        ServerOwnedContinuousLearningCacheRootNative.FlushNamedRegularLeaf(handle);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.AfterFileFlush);
        if (Describe(handle, maximumLength) != before)
            throw new InvalidDataException("The registry leaf changed during flush.");
    }

    internal static void FlushRoot(
        SafeFileHandle root,
        ServerOwnedContinuousLearningRegistryRootCapability capability)
    {
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.BeforeRootDirectoryFlush);
        capability.ThrowIfNativeCallFailsForTestingLocked(
            ServerOwnedContinuousLearningRegistryNativeCall.RootDirectoryFlush);
        ServerOwnedContinuousLearningCacheRootNative.FlushNamedRootDirectory(root);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningRegistryIoStage.AfterRootDirectoryFlush);
    }

    private static ServerOwnedContinuousLearningRegistrySnapshot ReadHeldExact(
        SafeFileHandle handle,
        ServerOwnedContinuousLearningRegistryFileIdentity expectedIdentity,
        long maximumLength)
    {
        var before = Describe(handle, maximumLength);
        if (before.Identity != expectedIdentity)
            throw new InvalidDataException("The fixed registry leaf identity changed.");
        var bytes = new byte[checked((int)before.Length)];
        var offset = 0;
        while (offset < bytes.Length)
        {
            var read = RandomAccess.Read(handle, bytes.AsSpan(offset), offset);
            if (read == 0)
                throw new EndOfStreamException("The fixed registry leaf changed during read.");
            offset = checked(offset + read);
        }
        var after = Describe(handle, maximumLength);
        if (after != before)
            throw new InvalidDataException("The fixed registry leaf changed during read.");
        return new ServerOwnedContinuousLearningRegistrySnapshot(after.Identity, bytes);
    }

    private static ServerOwnedContinuousLearningRegistryFileDescriptor Describe(
        SafeFileHandle handle,
        long maximumLength)
    {
        var identity = ServerOwnedContinuousLearningCacheRootNative
            .DescribeNamedRegularLeaf(
                handle,
                maximumLength,
                out var length);
        return new(
            new ServerOwnedContinuousLearningRegistryFileIdentity(
                identity.VolumeSerialNumber,
                identity.FileIdLow,
                identity.FileIdHigh),
            length);
    }

    private static string LeafName(ServerOwnedContinuousLearningRegistryLeaf leaf)
        => leaf switch
        {
            ServerOwnedContinuousLearningRegistryLeaf.OwnerLock =>
                OwnerLockLeafName,
            ServerOwnedContinuousLearningRegistryLeaf.CanonicalRegistry =>
                CanonicalRegistryLeafName,
            ServerOwnedContinuousLearningRegistryLeaf.WriteTemp =>
                WriteTempLeafName,
            ServerOwnedContinuousLearningRegistryLeaf.PublicationBackup =>
                PublicationBackupLeafName,
            ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence =>
                RollbackEvidenceLeafName,
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging =>
                AcceptedStagingLeafName,
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence =>
                AcceptedEvidenceLeafName,
            _ => throw new ArgumentOutOfRangeException(nameof(leaf)),
        };

    private static long MaximumLeafBytes(ServerOwnedContinuousLearningRegistryLeaf leaf)
        => leaf switch
        {
            ServerOwnedContinuousLearningRegistryLeaf.RollbackEvidence =>
                ServerOwnedContinuousLearningRegistryRollbackEvidence.MaximumEncodedBytes,
            ServerOwnedContinuousLearningRegistryLeaf.AcceptedStaging or
                ServerOwnedContinuousLearningRegistryLeaf.AcceptedEvidence =>
                ServerOwnedContinuousLearningRegistryAcceptedEvidence.MaximumEncodedBytes,
            _ => DomainModelRegistry.MaximumRegistryBytes,
        };
}
