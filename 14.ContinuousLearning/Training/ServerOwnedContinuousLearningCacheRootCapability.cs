// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

internal enum ServerOwnedContinuousLearningCacheLeaf
{
    OwnerLock,
    CanonicalCache,
    WriteTemp,
    PublicationBackup,
    AuditLog,
}

internal enum ServerOwnedContinuousLearningCacheIoStage
{
    BeforeFileFlush,
    AfterFileFlush,
    BeforeHandleRename,
    AfterHandleRename,
    BeforeExactDelete,
    AfterExactDelete,
    AfterBoundedExactReadPostValidationBeforeComparison,
    BeforeRootDirectoryFlush,
    AfterRootDirectoryFlush,
}

internal enum ServerOwnedContinuousLearningCacheNativeCall
{
    FileFlush,
    RootDirectoryFlush,
}

internal sealed record ServerOwnedContinuousLearningCacheRootCapabilityOptions
{
    internal Action<ServerOwnedContinuousLearningCacheIoStage>?
        ObserveIoStageForTesting
    {
        get;
        init;
    }

    internal Func<ServerOwnedContinuousLearningCacheNativeCall, int?>?
        NativeErrorForTesting
    {
        get;
        init;
    }

    internal Func<
        ServerOwnedContinuousLearningCacheExactReadPostValidation,
        ServerOwnedContinuousLearningCacheExactReadPostValidation>?
        OverrideExactReadPostValidationForTesting
    {
        get;
        init;
    }
}

internal readonly record struct ServerOwnedContinuousLearningCacheFileIdentity(
    ulong VolumeSerialNumber,
    ulong FileIdLow,
    ulong FileIdHigh);

internal readonly record struct ServerOwnedContinuousLearningCacheInventoryEntry(
    ServerOwnedContinuousLearningCacheLeaf Leaf,
    ServerOwnedContinuousLearningCacheFileIdentity Identity,
    long Length);

internal readonly record struct ServerOwnedContinuousLearningCacheExactReadPostValidation(
    ServerOwnedContinuousLearningCacheFileIdentity Identity,
    long Length);

/// <summary>
/// Revocable, string-free authority over the compiler cache's five fixed leaves.
/// It never exposes a path or native handle to consumers. It privately retains
/// only the owner-lock handle for lifetime exclusivity and closes that handle
/// synchronously on disposal or revocation. Every operation is serialized by
/// the owning retained-object gate and bracketed by CacheRoot identity validation.
/// The surrounding file-access owner can outlive this child lock, so a failed
/// cache open releases the compiler-cache lock without pretending that the
/// broader mutation-owner lease has ended.
/// </summary>
internal sealed class ServerOwnedContinuousLearningCacheRootCapability : IDisposable
{
    private readonly ServerOwnedContinuousLearningFileAccess _owner;
    private readonly SafeFileHandle _ownerLockHandle;
    private readonly ServerOwnedContinuousLearningCacheRootCapabilityOptions
        _options;
    private bool _disposed;
    private bool _revoked;

    internal ServerOwnedContinuousLearningCacheRootCapability(
        ServerOwnedContinuousLearningFileAccess owner,
        SafeFileHandle ownerLockHandle,
        ServerOwnedContinuousLearningCacheRootCapabilityOptions options)
    {
        _owner = owner;
        _ownerLockHandle = ownerLockHandle;
        _options = options;
    }

    internal IReadOnlyList<ServerOwnedContinuousLearningCacheInventoryEntry>
        Inventory(int maximumEntryCount)
    {
        ValidateInventoryBound(maximumEntryCount);
        return _owner.InventoryCompilerCacheLeaves(this, maximumEntryCount);
    }

    internal ServerOwnedContinuousLearningCacheFileIdentity OpenExistingRegularLeaf(
        ServerOwnedContinuousLearningCacheLeaf leaf,
        long maximumBytes)
    {
        ValidateReadableLeafAndBound(leaf, maximumBytes);
        return _owner.OpenExistingCompilerCacheLeaf(this, leaf, maximumBytes);
    }

    internal ServerOwnedContinuousLearningCacheFileIdentity CreateWriteTemp(
        ReadOnlyMemory<byte> bytes,
        int maximumBytes)
    {
        ValidateBound(maximumBytes);
        if (bytes.Length > maximumBytes)
            throw new ArgumentOutOfRangeException(nameof(bytes));
        return _owner.CreateCompilerCacheWriteTemp(this, bytes, maximumBytes);
    }

    internal byte[] ReadBounded(
        ServerOwnedContinuousLearningCacheLeaf leaf,
        int maximumBytes)
    {
        ValidateReadableLeafAndBound(leaf, maximumBytes);
        return _owner.ReadCompilerCacheLeaf(this, leaf, maximumBytes);
    }

    internal byte[] ReadBoundedExact(
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        int maximumBytes)
    {
        ValidateReadableLeafAndBound(leaf, maximumBytes);
        ValidateIdentity(expectedIdentity);
        return _owner.ReadExactCompilerCacheLeaf(
            this,
            leaf,
            expectedIdentity,
            maximumBytes);
    }

    internal ServerOwnedContinuousLearningCacheFileIdentity QueryExactIdentity(
        ServerOwnedContinuousLearningCacheLeaf leaf,
        long maximumBytes)
    {
        ValidateReadableLeafAndBound(leaf, maximumBytes);
        return _owner.QueryCompilerCacheLeafIdentity(this, leaf, maximumBytes);
    }

    internal void ValidateExactIdentity(
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes)
    {
        ValidateReadableLeafAndBound(leaf, maximumBytes);
        ValidateIdentity(expectedIdentity);
        _owner.ValidateCompilerCacheLeafIdentity(
            this,
            leaf,
            expectedIdentity,
            maximumBytes);
    }

    internal void RenameNoReplace(
        ServerOwnedContinuousLearningCacheLeaf source,
        ServerOwnedContinuousLearningCacheFileIdentity expectedSourceIdentity,
        ServerOwnedContinuousLearningCacheLeaf destination,
        long maximumBytes)
    {
        ValidateReadableLeafAndBound(source, maximumBytes);
        ValidateRenameEdge(source, destination);
        ValidateIdentity(expectedSourceIdentity);
        _owner.RenameCompilerCacheLeafNoReplace(
            this,
            source,
            expectedSourceIdentity,
            destination,
            maximumBytes);
    }

    internal void DeleteExactOpenedIdentity(
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes)
    {
        ValidateRecoveryLeafAndBound(leaf, maximumBytes);
        ValidateIdentity(expectedIdentity);
        _owner.DeleteCompilerCacheLeafExact(
            this,
            leaf,
            expectedIdentity,
            maximumBytes);
    }

    internal void FlushFile(
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes)
    {
        ValidateReadableLeafAndBound(leaf, maximumBytes);
        ValidateIdentity(expectedIdentity);
        _owner.FlushCompilerCacheLeaf(
            this,
            leaf,
            expectedIdentity,
            maximumBytes);
    }

    internal void FlushRootDirectory()
        => _owner.FlushCompilerCacheRoot(this);

    // Disposal routes through the parent owner so the parent can keep a single
    // authoritative view of whether its one-shot cache child is gone.
    public void Dispose()
        => _owner.DisposeCompilerCacheRootCapability(this);

    internal void ThrowIfUnavailableLocked()
    {
        if (_disposed || _revoked)
            throw new ContinuousLearningPathIdentityException();
    }

    internal ServerOwnedContinuousLearningCacheInventoryEntry
        DescribeOwnerLockLocked()
        => ServerOwnedContinuousLearningCacheRootNative.DescribeOwnerLock(
            _ownerLockHandle);

    internal void ObserveIoStageLocked(
        ServerOwnedContinuousLearningCacheIoStage stage)
        => _options.ObserveIoStageForTesting?.Invoke(stage);

    internal int? GetNativeErrorForTestingLocked(
        ServerOwnedContinuousLearningCacheNativeCall call)
        => _options.NativeErrorForTesting?.Invoke(call);

    internal ServerOwnedContinuousLearningCacheExactReadPostValidation
        OverrideExactReadPostValidationForTestingLocked(
            ServerOwnedContinuousLearningCacheExactReadPostValidation actual)
        => _options.OverrideExactReadPostValidationForTesting?.Invoke(actual) ?? actual;

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

    private static void ValidateInventoryBound(int maximumEntryCount)
    {
        if (maximumEntryCount is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(maximumEntryCount));
    }

    private static void ValidateReadableLeafAndBound(
        ServerOwnedContinuousLearningCacheLeaf leaf,
        long maximumBytes)
    {
        ValidateReadableLeaf(leaf);
        ValidateBound(maximumBytes);
    }

    private static void ValidateRecoveryLeafAndBound(
        ServerOwnedContinuousLearningCacheLeaf leaf,
        long maximumBytes)
    {
        if (leaf is not ServerOwnedContinuousLearningCacheLeaf.WriteTemp and
            not ServerOwnedContinuousLearningCacheLeaf.PublicationBackup)
        {
            throw new ArgumentOutOfRangeException(nameof(leaf));
        }
        ValidateBound(maximumBytes);
    }

    private static void ValidateReadableLeaf(
        ServerOwnedContinuousLearningCacheLeaf leaf)
    {
        if (leaf is not ServerOwnedContinuousLearningCacheLeaf.CanonicalCache and
            not ServerOwnedContinuousLearningCacheLeaf.WriteTemp and
            not ServerOwnedContinuousLearningCacheLeaf.PublicationBackup)
        {
            throw new ArgumentOutOfRangeException(nameof(leaf));
        }
    }

    private static void ValidateRenameEdge(
        ServerOwnedContinuousLearningCacheLeaf source,
        ServerOwnedContinuousLearningCacheLeaf destination)
    {
        var valid = source == ServerOwnedContinuousLearningCacheLeaf.CanonicalCache &&
                    destination == ServerOwnedContinuousLearningCacheLeaf.PublicationBackup ||
                    source == ServerOwnedContinuousLearningCacheLeaf.WriteTemp &&
                    destination == ServerOwnedContinuousLearningCacheLeaf.CanonicalCache ||
                    source == ServerOwnedContinuousLearningCacheLeaf.PublicationBackup &&
                    destination == ServerOwnedContinuousLearningCacheLeaf.CanonicalCache;
        if (!valid)
            throw new ArgumentException("The fixed cache rename edge is not permitted.");
    }

    private static void ValidateBound(long maximumBytes)
    {
        if (maximumBytes is <= 0 or > ServerOwnedContinuousLearningCacheRootNative
                .MaximumOperationBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumBytes));
        }
    }

    private static void ValidateIdentity(
        ServerOwnedContinuousLearningCacheFileIdentity identity)
    {
        if (identity.VolumeSerialNumber == 0 ||
            identity.FileIdLow == 0 && identity.FileIdHigh == 0)
        {
            throw new ArgumentException("A nonzero opened-file identity is required.");
        }
    }
}

internal sealed partial class ServerOwnedContinuousLearningFileAccess
{
    private bool _compilerCacheRootCapabilityAcquisitionConsumed;
    private ServerOwnedContinuousLearningCacheRootCapability?
        _compilerCacheRootCapability;

    internal ServerOwnedContinuousLearningCacheRootCapability
        AcquireCompilerCacheRootCapability(
            ServerOwnedContinuousLearningCacheRootCapabilityOptions? options = null)
    {
        Action? callback = null;
        ExceptionDispatchInfo? acquisitionFailure = null;
        ServerOwnedContinuousLearningCacheRootCapability? capability = null;
        var trustFailed = false;
        options ??= new ServerOwnedContinuousLearningCacheRootCapabilityOptions();
        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            if (_compilerCacheRootCapabilityAcquisitionConsumed)
            {
                throw new InvalidOperationException(
                    "The compiler CacheRoot capability is one-shot.");
            }

            var root = FindRetainedRoot(ContinuousLearningRetainedRootRole.Cache);
            if (!TryValidateRetainedEntry(root))
            {
                _compilerCacheRootCapabilityAcquisitionConsumed = true;
                callback = LoseRetainedObjectTrustLocked();
                trustFailed = true;
            }
            else
            {
                SafeFileHandle? ownerLockHandle = null;
                try
                {
                    ownerLockHandle = ServerOwnedContinuousLearningCacheRootNative
                        .AcquireOwnerLock(root.Handle);
                }
                catch (Exception exception)
                    when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                        .IsNonFatal(exception))
                {
                    acquisitionFailure = ExceptionDispatchInfo.Capture(exception);
                }

                if (!TryValidateRetainedEntry(root))
                {
                    ownerLockHandle?.Dispose();
                    _compilerCacheRootCapabilityAcquisitionConsumed = true;
                    callback = LoseRetainedObjectTrustLocked();
                    trustFailed = true;
                }
                else if (acquisitionFailure is null)
                {
                    // The child owns only this cache-root lock handle. Keeping
                    // the parent alive is what continues to protect the rest of
                    // the admitted mutation domain.
                    _compilerCacheRootCapabilityAcquisitionConsumed = true;
                    capability = new ServerOwnedContinuousLearningCacheRootCapability(
                        this,
                        ownerLockHandle!,
                        options);
                    _compilerCacheRootCapability = capability;
                }
            }
        }

        InvokeRetainedObjectTrustLost(callback);
        if (trustFailed)
            throw new ContinuousLearningPathIdentityException();
        acquisitionFailure?.Throw();
        return capability!;
    }

    internal void DisposeCompilerCacheRootCapability(
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_compilerCacheRootCapability, capability))
                capability.DisposeLocked();
        }
    }

    internal IReadOnlyList<ServerOwnedContinuousLearningCacheInventoryEntry>
        InventoryCompilerCacheLeaves(
            ServerOwnedContinuousLearningCacheRootCapability capability,
            int maximumEntryCount)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root => ServerOwnedContinuousLearningCacheRootNative.Inventory(
                root,
                maximumEntryCount,
                capability.DescribeOwnerLockLocked()));

    internal ServerOwnedContinuousLearningCacheFileIdentity
        OpenExistingCompilerCacheLeaf(
            ServerOwnedContinuousLearningCacheRootCapability capability,
            ServerOwnedContinuousLearningCacheLeaf leaf,
            long maximumBytes)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root => ServerOwnedContinuousLearningCacheRootNative.OpenExisting(
                root,
                leaf,
                maximumBytes));

    internal ServerOwnedContinuousLearningCacheFileIdentity
        CreateCompilerCacheWriteTemp(
            ServerOwnedContinuousLearningCacheRootCapability capability,
            ReadOnlyMemory<byte> bytes,
            int maximumBytes)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root => ServerOwnedContinuousLearningCacheRootNative.CreateWriteTemp(
                root,
                bytes,
                maximumBytes,
                capability));

    internal byte[] ReadCompilerCacheLeaf(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        int maximumBytes)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root => ServerOwnedContinuousLearningCacheRootNative.ReadBounded(
                root,
                leaf,
                maximumBytes));

    internal byte[] ReadExactCompilerCacheLeaf(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        int maximumBytes)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root => ServerOwnedContinuousLearningCacheRootNative.ReadBoundedExact(
                root,
                leaf,
                expectedIdentity,
                maximumBytes,
                capability));

    internal ServerOwnedContinuousLearningCacheFileIdentity
        QueryCompilerCacheLeafIdentity(
            ServerOwnedContinuousLearningCacheRootCapability capability,
            ServerOwnedContinuousLearningCacheLeaf leaf,
            long maximumBytes)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root => ServerOwnedContinuousLearningCacheRootNative.OpenExisting(
                root,
                leaf,
                maximumBytes));

    internal void ValidateCompilerCacheLeafIdentity(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningCacheRootNative.ValidateIdentity(
                    root,
                    leaf,
                    expectedIdentity,
                    maximumBytes);
                return true;
            });

    internal void RenameCompilerCacheLeafNoReplace(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        ServerOwnedContinuousLearningCacheLeaf source,
        ServerOwnedContinuousLearningCacheFileIdentity expectedSourceIdentity,
        ServerOwnedContinuousLearningCacheLeaf destination,
        long maximumBytes)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningCacheRootNative.RenameNoReplace(
                    root,
                    source,
                    expectedSourceIdentity,
                    destination,
                    maximumBytes,
                    capability);
                return true;
            });

    internal void DeleteCompilerCacheLeafExact(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningCacheRootNative.DeleteExact(
                    root,
                    leaf,
                    expectedIdentity,
                    maximumBytes,
                    capability);
                return true;
            });

    internal void FlushCompilerCacheLeaf(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningCacheRootNative.FlushFile(
                    root,
                    leaf,
                    expectedIdentity,
                    maximumBytes,
                    capability);
                return true;
            });

    internal void FlushCompilerCacheRoot(
        ServerOwnedContinuousLearningCacheRootCapability capability)
        => ExecuteCompilerCacheRootOperation(
            capability,
            root =>
            {
                ServerOwnedContinuousLearningCacheRootNative.FlushRoot(
                    root,
                    capability);
                return true;
            });

    private T ExecuteCompilerCacheRootOperation<T>(
        ServerOwnedContinuousLearningCacheRootCapability capability,
        Func<SafeFileHandle, T> operation)
    {
        Action? callback = null;
        ExceptionDispatchInfo? operationFailure = null;
        var trustFailed = false;
        var result = default(T)!;
        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            if (!ReferenceEquals(_compilerCacheRootCapability, capability))
                throw new ContinuousLearningPathIdentityException();
            capability.ThrowIfUnavailableLocked();

            var root = FindRetainedRoot(ContinuousLearningRetainedRootRole.Cache);
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

internal static class ServerOwnedContinuousLearningCacheRootNative
{
    internal const long MaximumOperationBytes = 64L * 1024L * 1024L;

    private const uint GenericRead = 0x80000000;
    private const uint GenericWrite = 0x40000000;
    private const uint DeleteAccess = 0x00010000;
    private const uint Synchronize = 0x00100000;
    private const uint FileReadAttributes = 0x00000080;
    private const uint FileAttributeNormal = 0x00000080;
    private const uint FileOpen = 1;
    private const uint FileCreate = 2;
    private const uint FileOpenIf = 3;
    private const uint FileSynchronousIoNonAlert = 0x00000020;
    private const uint FileNonDirectoryFile = 0x00000040;
    private const uint FileOpenReparsePoint = 0x00200000;
    private const uint FileWriteThrough = 0x00000002;
    private const uint ObjectCaseInsensitive = 0x00000040;
    private const uint ObjectDontReparse = 0x00001000;
    private const uint FileAttributeDirectory = 0x00000010;
    private const uint FileAttributeReparsePoint = 0x00000400;
    private const int FileRenameInformation = 10;
    private const int StatusNoSuchFile = unchecked((int)0xC000000F);
    private const int StatusObjectNameNotFound = unchecked((int)0xC0000034);
    private const int StatusObjectPathNotFound = unchecked((int)0xC000003A);
    private const int StatusObjectNameCollision = unchecked((int)0xC0000035);
    private const int PosixOpenReadOnly = 0;
    private const int PosixOpenReadWrite = 2;
    private const int PosixOpenCreate = 0x40;
    private const int PosixOpenExclusive = 0x80;
    private const int PosixOpenNoFollow = 0x20000;
    private const int PosixOpenCloseOnExec = 0x80000;
    private const uint PosixOwnerReadWriteMode = 0x180;
    private const uint PosixFileTypeMask = 0xF000;
    private const uint PosixRegularFileType = 0x8000;
    private const uint PosixPermissionBitsMask = 0x1FF;
    private const uint PosixGroupOrOtherWriteMask = 0x12;
    private const int PosixEntryNotFound = 2;
    private const int PosixNameExists = 17;
    private const int PosixTooManySymbolicLinks = 40;
    private const int PosixWouldBlock = 11;
    private const int PosixFlockExclusive = 0x02;
    private const int PosixFlockNonBlocking = 0x04;
    private const int PosixNameMaxUtf8Bytes = 255;

    internal static IReadOnlyList<ServerOwnedContinuousLearningCacheInventoryEntry>
        Inventory(
            SafeFileHandle root,
            int maximumEntryCount,
            ServerOwnedContinuousLearningCacheInventoryEntry ownerLock)
    {
        var entries = new List<ServerOwnedContinuousLearningCacheInventoryEntry>(5)
        {
            ownerLock,
        };
        if (entries.Count > maximumEntryCount)
        {
            throw new InvalidDataException(
                "The fixed-leaf cache inventory exceeded its bound.");
        }
        foreach (var leaf in InventoryLeaves)
        {
            using var handle = OpenDataRelative(
                root,
                leaf,
                FileOpen,
                mutationAccess: false,
                out var status);
            if (IsTypedAbsentNamedLeaf(status))
                continue;
            ThrowIfOpenFailed(handle, status, allowMissing: false);
            var information = ValidateRegularLeaf(handle, MaximumOperationBytes);
            entries.Add(new ServerOwnedContinuousLearningCacheInventoryEntry(
                leaf,
                information.Identity,
                information.Length));
            if (entries.Count > maximumEntryCount)
            {
                throw new InvalidDataException(
                    "The fixed-leaf cache inventory exceeded its bound.");
            }
        }
        return entries.ToArray();
    }

    internal static SafeFileHandle AcquireOwnerLock(SafeFileHandle root)
    {
        var handle = OpenRelative(
            root,
            ServerOwnedContinuousLearningCacheLeaf.OwnerLock,
            FileOpenIf,
            GenericRead | GenericWrite | FileReadAttributes | Synchronize,
            FileShare.None,
            out var status);
        try
        {
            ThrowIfOpenFailed(handle, status, allowMissing: false);
            _ = ValidateRegularLeaf(handle, maximumBytes: 4 * 1024);
            AcquirePosixOwnerLockIfNeeded(handle);
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    internal static ServerOwnedContinuousLearningCacheInventoryEntry
        DescribeOwnerLock(SafeFileHandle ownerLockHandle)
    {
        var information = ValidateRegularLeaf(ownerLockHandle, maximumBytes: 4 * 1024);
        return new ServerOwnedContinuousLearningCacheInventoryEntry(
            ServerOwnedContinuousLearningCacheLeaf.OwnerLock,
            information.Identity,
            information.Length);
    }

    internal static ServerOwnedContinuousLearningCacheFileIdentity OpenExisting(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        long maximumBytes)
    {
        using var handle = OpenDataRelative(
            root,
            leaf,
            FileOpen,
            mutationAccess: false,
            out var status);
        ThrowIfOpenFailed(handle, status, allowMissing: false);
        return ValidateRegularLeaf(handle, maximumBytes).Identity;
    }

    internal static ServerOwnedContinuousLearningCacheFileIdentity CreateWriteTemp(
        SafeFileHandle root,
        ReadOnlyMemory<byte> bytes,
        int maximumBytes,
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        using var handle = OpenDataRelative(
            root,
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
            FileCreate,
            mutationAccess: true,
            out var status);
        ThrowIfOpenFailed(handle, status, allowMissing: false);
        _ = ValidateRegularLeaf(handle, maximumBytes);
        if (!bytes.IsEmpty)
            RandomAccess.Write(handle, bytes.Span, 0);
        SetEndOfFile(handle, bytes.Length);
        FlushFileHandle(handle, capability);
        return ValidateRegularLeaf(handle, maximumBytes).Identity;
    }

    internal static byte[] ReadBounded(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        int maximumBytes)
    {
        using var handle = OpenDataRelative(
            root,
            leaf,
            FileOpen,
            mutationAccess: false,
            out var status);
        ThrowIfOpenFailed(handle, status, allowMissing: false);
        var information = ValidateRegularLeaf(handle, maximumBytes);
        var bytes = new byte[checked((int)information.Length)];
        var offset = 0;
        while (offset < bytes.Length)
        {
            var read = RandomAccess.Read(handle, bytes.AsSpan(offset), offset);
            if (read == 0)
                throw new EndOfStreamException("A fixed cache leaf changed during bounded read.");
            offset = checked(offset + read);
        }
        _ = ValidateRegularLeaf(handle, maximumBytes);
        return bytes;
    }

    internal static byte[] ReadBoundedExact(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        int maximumBytes,
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        using var handle = OpenDataRelative(
            root,
            leaf,
            FileOpen,
            mutationAccess: false,
            out var status);
        ThrowIfOpenFailed(handle, status, allowMissing: false);
        var before = ValidateRegularLeaf(handle, maximumBytes);
        if (before.Identity != expectedIdentity)
            throw new InvalidDataException("The fixed cache leaf identity changed.");

        var bytes = new byte[checked((int)before.Length)];
        var offset = 0;
        while (offset < bytes.Length)
        {
            var read = RandomAccess.Read(handle, bytes.AsSpan(offset), offset);
            if (read == 0)
            {
                throw new EndOfStreamException(
                    "A fixed cache leaf changed during bounded exact read.");
            }
            offset = checked(offset + read);
        }

        var after = ValidateRegularLeaf(handle, maximumBytes);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningCacheIoStage
                .AfterBoundedExactReadPostValidationBeforeComparison);
        var observed = capability.OverrideExactReadPostValidationForTestingLocked(
            new ServerOwnedContinuousLearningCacheExactReadPostValidation(
                after.Identity,
                after.Length));
        if (observed.Identity != before.Identity || observed.Length != before.Length)
        {
            throw new InvalidDataException(
                "The fixed cache leaf changed during bounded exact read.");
        }
        return bytes;
    }

    internal static void ValidateIdentity(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes)
    {
        var actual = OpenExisting(root, leaf, maximumBytes);
        if (actual != expectedIdentity)
            throw new InvalidDataException("The fixed cache leaf identity changed.");
    }

    internal static void RenameNoReplace(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheLeaf source,
        ServerOwnedContinuousLearningCacheFileIdentity expectedSourceIdentity,
        ServerOwnedContinuousLearningCacheLeaf destination,
        long maximumBytes,
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        using var handle = OpenDataRelative(
            root,
            source,
            FileOpen,
            mutationAccess: true,
            out var status);
        ThrowIfOpenFailed(handle, status, allowMissing: false);
        var before = ValidateRegularLeaf(handle, maximumBytes);
        if (before.Identity != expectedSourceIdentity)
            throw new InvalidDataException("The rename source identity changed.");
        RenameHandleNoReplace(
            root,
            handle,
            LeafName(source),
            LeafName(destination),
            capability);
        var after = ValidateRegularLeaf(handle, maximumBytes);
        if (after.Identity != expectedSourceIdentity)
            throw new InvalidDataException("The renamed cache leaf identity changed.");
    }

    internal static void DeleteExact(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes,
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        using var handle = OpenDataRelative(
            root,
            leaf,
            FileOpen,
            mutationAccess: true,
            out var status);
        ThrowIfOpenFailed(handle, status, allowMissing: false);
        var actual = ValidateRegularLeaf(handle, maximumBytes).Identity;
        if (actual != expectedIdentity)
            throw new InvalidDataException("The delete target identity changed.");
        MarkDeleteOnClose(root, LeafName(leaf), handle, capability);
    }

    internal static void FlushFile(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        ServerOwnedContinuousLearningCacheFileIdentity expectedIdentity,
        long maximumBytes,
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        using var handle = OpenDataRelative(
            root,
            leaf,
            FileOpen,
            mutationAccess: true,
            out var status);
        ThrowIfOpenFailed(handle, status, allowMissing: false);
        var actual = ValidateRegularLeaf(handle, maximumBytes).Identity;
        if (actual != expectedIdentity)
            throw new InvalidDataException("The flush target identity changed.");
        FlushFileHandle(handle, capability);
    }

    internal static void FlushRoot(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningCacheIoStage.BeforeRootDirectoryFlush);
        if (capability.GetNativeErrorForTestingLocked(
                ServerOwnedContinuousLearningCacheNativeCall.RootDirectoryFlush)
            is not null)
        {
            throw new IOException("The retained CacheRoot directory flush failed.");
        }
        FlushNamedRootDirectory(root);
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningCacheIoStage.AfterRootDirectoryFlush);
    }

    // Adapter publication uses these named-leaf primitives through its own
    // capability. They deliberately keep the same retained-root, no-follow,
    // identity, no-replace and flush semantics as the fixed cache leaves while
    // never widening the cache capability's public authority.
    internal static SafeFileHandle AcquireNamedOwnerLock(
        SafeFileHandle root,
        string canonicalLeafName)
    {
        var handle = OpenNamedRelative(
            root,
            canonicalLeafName,
            FileOpenIf,
            GenericRead | GenericWrite | FileReadAttributes | Synchronize,
            FileShare.None,
            out var status);
        try
        {
            ThrowIfOpenFailed(handle, status, allowMissing: false);
            _ = ValidateRegularLeaf(handle, maximumBytes: 4 * 1024);
            AcquirePosixOwnerLockIfNeeded(handle);
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    internal static SafeFileHandle CreateNamedWriteLeaf(
        SafeFileHandle root,
        string canonicalLeafName,
        long maximumBytes)
    {
        var handle = OpenNamedRelative(
            root,
            canonicalLeafName,
            FileCreate,
            GenericRead | GenericWrite | DeleteAccess | FileReadAttributes | Synchronize,
            FileShare.Read,
            out var status);
        try
        {
            ThrowIfOpenFailed(handle, status, allowMissing: false);
            _ = ValidateRegularLeaf(handle, maximumBytes);
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    internal static SafeFileHandle OpenNamedRegularLeaf(
        SafeFileHandle root,
        string canonicalLeafName,
        bool mutationAccess,
        long maximumBytes)
    {
        var handle = OpenNamedRelative(
            root,
            canonicalLeafName,
            FileOpen,
            GenericRead |
            FileReadAttributes |
            Synchronize |
            (mutationAccess ? GenericWrite | DeleteAccess : 0),
            FileShare.Read,
            out var status);
        try
        {
            ThrowIfOpenFailed(handle, status, allowMissing: false);
            _ = ValidateRegularLeaf(handle, maximumBytes);
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Opens one fixed protocol leaf while preserving a normal, typed absence
    /// outcome for the RegistryRoot canonical leaf. Callers supply only their
    /// own private canonical constants; this helper is not capability surface.
    /// </summary>
    internal static SafeFileHandle? TryOpenNamedRegularLeaf(
        SafeFileHandle root,
        string canonicalLeafName,
        bool mutationAccess,
        long maximumBytes)
    {
        var handle = OpenNamedRelative(
            root,
            canonicalLeafName,
            FileOpen,
            GenericRead |
            FileReadAttributes |
            Synchronize |
            (mutationAccess ? GenericWrite | DeleteAccess : 0),
            FileShare.Read,
            out var status);
        if (IsTypedAbsentNamedLeaf(status))
        {
            handle.Dispose();
            return null;
        }

        try
        {
            ThrowIfOpenFailed(handle, status, allowMissing: false);
            _ = ValidateRegularLeaf(handle, maximumBytes);
            return handle;
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    internal static ServerOwnedContinuousLearningCacheFileIdentity
        DescribeNamedRegularLeaf(
            SafeFileHandle handle,
            long maximumBytes,
            out long length)
    {
        var information = ValidateRegularLeaf(handle, maximumBytes);
        length = information.Length;
        return information.Identity;
    }

    internal static void FlushNamedRegularLeaf(SafeFileHandle handle)
    {
        if (OperatingSystem.IsLinux())
        {
            if (FsyncPosix(ToPosixFileDescriptor(handle)) != 0)
            {
                throw new IOException(
                    "The handle-relative POSIX named leaf flush failed.");
            }
            return;
        }

        if (!FlushFileBuffersWindows(handle))
            throw new IOException("The handle-relative named leaf flush failed.");
    }

    internal static void RenameNamedLeafNoReplace(
        SafeFileHandle root,
        SafeFileHandle source,
        string sourceName,
        string destinationName)
    {
        if (OperatingSystem.IsLinux())
        {
            if (!IsSafeSingleLeafName(destinationName))
                throw new IOException("The POSIX fixed leaf namespace mutation name is unsafe.");
            ThrowPosixExactNamespaceMutationUnsupported(source, sourceName);
            return;
        }

        var rootAddRef = false;
        var nameBytes = checked(destinationName.Length * sizeof(char));
        var rootOffset = IntPtr.Size == 8 ? 8 : 4;
        var lengthOffset = rootOffset + IntPtr.Size;
        var nameOffset = lengthOffset + sizeof(uint);
        var buffer = Marshal.AllocHGlobal(checked(nameOffset + nameBytes));
        try
        {
            for (var index = 0; index < nameOffset + nameBytes; index++)
                Marshal.WriteByte(buffer, index, 0);
            root.DangerousAddRef(ref rootAddRef);
            Marshal.WriteIntPtr(buffer, rootOffset, root.DangerousGetHandle());
            Marshal.WriteInt32(buffer, lengthOffset, nameBytes);
            Marshal.Copy(
                destinationName.ToCharArray(),
                0,
                IntPtr.Add(buffer, nameOffset),
                destinationName.Length);
            var status = NtSetInformationFileWindows(
                source,
                out _,
                buffer,
                checked((uint)(nameOffset + nameBytes)),
                FileRenameInformation);
            if (status == StatusObjectNameCollision)
                throw new IOException("The named-leaf destination is occupied.");
            if (status < 0)
                throw new IOException("The handle-relative named-leaf rename failed.");
        }
        finally
        {
            if (rootAddRef)
                root.DangerousRelease();
            Marshal.FreeHGlobal(buffer);
        }
    }

    internal static void MarkNamedRegularLeafDeleteOnClose(SafeFileHandle handle)
    {
        if (OperatingSystem.IsLinux())
        {
            // Старый overload без имени не получает лазейку path-cleanup:
            // открытый fd проверяется, после чего POSIX exact delete всё равно
            // fail closed как неподдержанная namespace mutation.
            _ = ValidateRegularLeafPosix(handle, MaximumOperationBytes);
            throw new ContinuousLearningPosixExactMutationUnsupportedException();
        }

        var buffer = Marshal.AllocHGlobal(sizeof(int));
        try
        {
            Marshal.WriteInt32(buffer, 1);
            if (!SetFileInformationByHandleWindows(
                    handle,
                    FileInfoByHandleClass.FileDispositionInfo,
                    buffer,
                    sizeof(int)))
            {
                throw new IOException("The exact-identity named-leaf delete failed.");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    internal static void MarkNamedRegularLeafDeleteOnClose(
        SafeFileHandle root,
        string canonicalLeafName,
        SafeFileHandle handle)
    {
        if (OperatingSystem.IsLinux())
        {
            ThrowPosixExactNamespaceMutationUnsupported(handle, canonicalLeafName);
            return;
        }

        MarkNamedRegularLeafDeleteOnClose(handle);
    }

    internal static void FlushNamedRootDirectory(SafeFileHandle root)
    {
        if (OperatingSystem.IsLinux())
        {
            if (FsyncPosix(ToPosixFileDescriptor(root)) != 0)
            {
                throw new IOException(
                    "The retained POSIX named root directory flush failed.");
            }
            return;
        }

        if (NtFlushBuffersFileWindows(root, out _) < 0)
            throw new IOException("The retained named root directory flush failed.");
    }

    private static readonly ServerOwnedContinuousLearningCacheLeaf[] InventoryLeaves =
    [
        ServerOwnedContinuousLearningCacheLeaf.CanonicalCache,
        ServerOwnedContinuousLearningCacheLeaf.WriteTemp,
        ServerOwnedContinuousLearningCacheLeaf.PublicationBackup,
        ServerOwnedContinuousLearningCacheLeaf.AuditLog,
    ];

    private static string LeafName(ServerOwnedContinuousLearningCacheLeaf leaf)
        => leaf switch
        {
            ServerOwnedContinuousLearningCacheLeaf.OwnerLock =>
                ".semantic-compiler.owner.lock",
            ServerOwnedContinuousLearningCacheLeaf.CanonicalCache =>
                "semantic-compiler-cache-v1.bin",
            ServerOwnedContinuousLearningCacheLeaf.WriteTemp =>
                ".semantic-compiler-cache-v1.write.tmp",
            ServerOwnedContinuousLearningCacheLeaf.PublicationBackup =>
                ".semantic-compiler-cache-v1.publication-backup",
            ServerOwnedContinuousLearningCacheLeaf.AuditLog =>
                "semantic-compiler-audit-v1.log",
            _ => throw new ArgumentOutOfRangeException(nameof(leaf)),
        };

    private static SafeFileHandle OpenDataRelative(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        uint disposition,
        bool mutationAccess,
        out int status)
        => OpenRelative(
            root,
            leaf,
            disposition,
            GenericRead |
            FileReadAttributes |
            Synchronize |
            (mutationAccess ? GenericWrite | DeleteAccess : 0),
            FileShare.Read,
            out status);

    private static SafeFileHandle OpenRelative(
        SafeFileHandle root,
        ServerOwnedContinuousLearningCacheLeaf leaf,
        uint disposition,
        uint desiredAccess,
        FileShare share,
        out int status)
    {
        return OpenNamedRelative(
            root,
            LeafName(leaf),
            disposition,
            desiredAccess,
            share,
            out status);
    }

    private static SafeFileHandle OpenNamedRelative(
        SafeFileHandle root,
        string name,
        uint disposition,
        uint desiredAccess,
        FileShare share,
        out int status)
    {
        if (OperatingSystem.IsLinux())
        {
            return OpenNamedRelativePosix(
                root,
                name,
                disposition,
                desiredAccess,
                out status);
        }

        if (!IsSafeSingleLeafName(name))
            throw new ArgumentException("A canonical single leaf name is required.", nameof(name));
        var nameBuffer = Marshal.StringToHGlobalUni(name);
        var unicodeStringPointer = Marshal.AllocHGlobal(
            Marshal.SizeOf<UnicodeString>());
        var rootAddRef = false;
        try
        {
            var nameLength = checked((ushort)(name.Length * sizeof(char)));
            Marshal.StructureToPtr(
                new UnicodeString
                {
                    Length = nameLength,
                    MaximumLength = checked((ushort)(nameLength + sizeof(char))),
                    Buffer = nameBuffer,
                },
                unicodeStringPointer,
                fDeleteOld: false);
            root.DangerousAddRef(ref rootAddRef);
            var attributes = new ObjectAttributes
            {
                Length = Marshal.SizeOf<ObjectAttributes>(),
                RootDirectory = root.DangerousGetHandle(),
                ObjectName = unicodeStringPointer,
                Attributes = ObjectCaseInsensitive | ObjectDontReparse,
                SecurityDescriptor = IntPtr.Zero,
                SecurityQualityOfService = IntPtr.Zero,
            };
            status = NtCreateFileWindows(
                out var handle,
                desiredAccess,
                ref attributes,
                out _,
                IntPtr.Zero,
                FileAttributeNormal,
                (uint)share,
                disposition,
                FileSynchronousIoNonAlert |
                FileNonDirectoryFile |
                FileOpenReparsePoint |
                FileWriteThrough,
                IntPtr.Zero,
                0);
            return handle;
        }
        finally
        {
            if (rootAddRef)
                root.DangerousRelease();
            Marshal.FreeHGlobal(unicodeStringPointer);
            Marshal.FreeHGlobal(nameBuffer);
        }
    }

    private static bool IsSafeSingleLeafName(string? name)
        => !string.IsNullOrWhiteSpace(name) &&
           name.Length <= 255 &&
           name is not "." and not ".." &&
           !Path.IsPathRooted(name) &&
           name[0] is not ' ' and not '\\' and not '/' &&
           name[^1] is not ' ' and not '.' &&
           !name.Any(character =>
               character == '\0' ||
               char.IsControl(character) ||
               character is '\\' or '/' or ':' or '*' or '?' or '<' or '>' or '"' or '|');

    private static void ThrowIfOpenFailed(
        SafeFileHandle handle,
        int status,
        bool allowMissing)
    {
        if (status >= 0 && !handle.IsInvalid)
            return;
        if (IsMissing(status))
        {
            if (allowMissing)
                return;
            throw new FileNotFoundException("The fixed cache leaf does not exist.");
        }
        if (status == StatusObjectNameCollision)
            throw new IOException("The fixed cache leaf already exists.");
        if (status == -PosixNameExists)
            throw new IOException("The fixed POSIX cache leaf already exists.");
        throw new IOException("The handle-relative fixed cache leaf open failed.");
    }

    private static bool IsMissing(int status)
        => status is StatusNoSuchFile or
            StatusObjectNameNotFound or
            StatusObjectPathNotFound ||
            status == -PosixEntryNotFound;

    private static bool IsTypedAbsentNamedLeaf(int status)
    {
        // Optional inventory/read может считать absence только реальное
        // отсутствие имени. POSIX ELOOP от O_NOFOLLOW доказывает symlink-атаку
        // внутри retained root boundary и обязан fail closed, а не превращаться
        // в "пустой namespace".
        return status is StatusNoSuchFile or
            StatusObjectNameNotFound or
            StatusObjectPathNotFound ||
            status == -PosixEntryNotFound;
    }

    private static SafeFileHandle OpenNamedRelativePosix(
        SafeFileHandle root,
        string name,
        uint disposition,
        uint desiredAccess,
        out int status)
    {
        EnsurePosixLinuxX64();
        // POSIX-ветка сохраняет ту же границу владения: имя листа безопасно само по себе,
        // но доверие рождается только из retained root fd + openat(..., O_NOFOLLOW).
        if (!IsSafeSingleLeafName(name) ||
            System.Text.Encoding.UTF8.GetByteCount(name) > PosixNameMaxUtf8Bytes)
        {
            throw new ArgumentException(
                "A canonical POSIX single leaf name is required.",
                nameof(name));
        }

        var flags = (desiredAccess & (GenericWrite | DeleteAccess)) != 0
            ? PosixOpenReadWrite
            : PosixOpenReadOnly;
        flags |= PosixOpenNoFollow | PosixOpenCloseOnExec;
        if (disposition == FileCreate)
            flags |= PosixOpenCreate | PosixOpenExclusive;
        else if (disposition == FileOpenIf)
            flags |= PosixOpenCreate;
        else if (disposition != FileOpen)
            throw new IOException("The POSIX fixed leaf disposition is unsupported.");

        var descriptor = OpenAtPosix(
            ToPosixFileDescriptor(root),
            name,
            flags,
            PosixOwnerReadWriteMode);
        status = descriptor < 0 ? -Marshal.GetLastPInvokeError() : 0;
        return new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
    }

    private static LeafInformation ValidateRegularLeafPosix(
        SafeFileHandle handle,
        long maximumBytes)
    {
        EnsurePosixLinuxX64();
        // Инвариант доверия Linux: удержанный fd обязан указывать на один обычный файл
        // текущего uid; hardlink/symlink/world-writable подмена закрывается исключением.
        if (handle.IsInvalid || handle.IsClosed ||
            FStatPosix(ToPosixFileDescriptor(handle), out var stat) != 0 ||
            (stat.Mode & PosixFileTypeMask) != PosixRegularFileType ||
            stat.Size < 0 ||
            stat.Size > maximumBytes ||
            stat.LinkCount != 1 ||
            stat.Device == 0 ||
            stat.Inode == 0)
        {
            throw new InvalidDataException(
                "The fixed POSIX cache leaf is not a trusted regular file.");
        }

        var permissionBits = stat.Mode & PosixPermissionBitsMask;
        if (stat.Uid != GetEffectiveUserIdPosix() ||
            (permissionBits & PosixGroupOrOtherWriteMask) != 0)
        {
            throw new InvalidDataException(
                "The fixed POSIX cache leaf is outside the owner-write trust boundary.");
        }

        return new LeafInformation(
            new ServerOwnedContinuousLearningCacheFileIdentity(
                stat.Device,
                stat.Inode,
                0UL),
            stat.Size);
    }

    private static LeafInformation ValidateRegularLeaf(
        SafeFileHandle handle,
        long maximumBytes)
    {
        if (OperatingSystem.IsLinux())
            return ValidateRegularLeafPosix(handle, maximumBytes);

        if (handle.IsInvalid || handle.IsClosed ||
            !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileAttributeTagInfo,
                out FileAttributeTagInformation attributes,
                (uint)Marshal.SizeOf<FileAttributeTagInformation>()) ||
            (attributes.FileAttributes &
             (FileAttributeDirectory | FileAttributeReparsePoint)) != 0 ||
            !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileStandardInfo,
                out FileStandardInformation standard,
                (uint)Marshal.SizeOf<FileStandardInformation>()) ||
            standard.EndOfFile < 0 ||
            standard.EndOfFile > maximumBytes ||
            standard.DeletePending != 0 ||
            standard.Directory != 0 ||
            standard.NumberOfLinks != 1 ||
            !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileIdInfo,
                out FileIdInformation identity,
                (uint)Marshal.SizeOf<FileIdInformation>()) ||
            identity.VolumeSerialNumber == 0 ||
            identity.FileId.Low == 0 && identity.FileId.High == 0)
        {
            throw new InvalidDataException("The fixed cache leaf is not a trusted regular file.");
        }

        return new LeafInformation(
            new ServerOwnedContinuousLearningCacheFileIdentity(
                identity.VolumeSerialNumber,
                identity.FileId.Low,
                identity.FileId.High),
            standard.EndOfFile);
    }

    private static void SetEndOfFile(SafeFileHandle handle, long length)
    {
        if (OperatingSystem.IsLinux())
        {
            if (FTruncatePosix(ToPosixFileDescriptor(handle), length) != 0)
                throw new IOException("The fixed POSIX cache leaf truncate failed.");
            return;
        }

        var information = new FileEndOfFileInformation { EndOfFile = length };
        if (!SetFileInformationByHandleWindows(
                handle,
                FileInfoByHandleClass.FileEndOfFileInfo,
                ref information,
                (uint)Marshal.SizeOf<FileEndOfFileInformation>()))
        {
            throw new IOException("The fixed cache leaf truncate failed.");
        }
    }

    private static void FlushFileHandle(
        SafeFileHandle handle,
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningCacheIoStage.BeforeFileFlush);
        if (capability.GetNativeErrorForTestingLocked(
                ServerOwnedContinuousLearningCacheNativeCall.FileFlush)
            is not null)
        {
            throw new IOException("The fixed cache leaf flush failed.");
        }
        if (OperatingSystem.IsLinux())
        {
            if (FsyncPosix(ToPosixFileDescriptor(handle)) != 0)
                throw new IOException("The fixed POSIX cache leaf flush failed.");
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningCacheIoStage.AfterFileFlush);
            return;
        }

        if (!FlushFileBuffersWindows(handle))
            throw new IOException("The fixed cache leaf flush failed.");
        capability.ObserveIoStageLocked(
            ServerOwnedContinuousLearningCacheIoStage.AfterFileFlush);
    }

    private static void RenameHandleNoReplace(
        SafeFileHandle root,
        SafeFileHandle source,
        string sourceName,
        string destinationName,
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        if (OperatingSystem.IsLinux())
        {
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningCacheIoStage.BeforeHandleRename);
            RenameNamedLeafNoReplace(root, source, sourceName, destinationName);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningCacheIoStage.AfterHandleRename);
            return;
        }

        var rootAddRef = false;
        var nameBytes = checked(destinationName.Length * sizeof(char));
        var rootOffset = IntPtr.Size == 8 ? 8 : 4;
        var lengthOffset = rootOffset + IntPtr.Size;
        var nameOffset = lengthOffset + sizeof(uint);
        var buffer = Marshal.AllocHGlobal(checked(nameOffset + nameBytes));
        try
        {
            for (var index = 0; index < nameOffset + nameBytes; index++)
                Marshal.WriteByte(buffer, index, 0);
            root.DangerousAddRef(ref rootAddRef);
            Marshal.WriteIntPtr(buffer, rootOffset, root.DangerousGetHandle());
            Marshal.WriteInt32(buffer, lengthOffset, nameBytes);
            Marshal.Copy(
                destinationName.ToCharArray(),
                0,
                IntPtr.Add(buffer, nameOffset),
                destinationName.Length);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningCacheIoStage.BeforeHandleRename);
            var status = NtSetInformationFileWindows(
                source,
                out _,
                buffer,
                checked((uint)(nameOffset + nameBytes)),
                FileRenameInformation);
            if (status == StatusObjectNameCollision)
                throw new IOException("The fixed cache rename destination is occupied.");
            if (status < 0)
                throw new IOException("The handle-based fixed cache leaf rename failed.");
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningCacheIoStage.AfterHandleRename);
        }
        finally
        {
            if (rootAddRef)
                root.DangerousRelease();
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static void MarkDeleteOnClose(
        SafeFileHandle root,
        string leafName,
        SafeFileHandle handle,
        ServerOwnedContinuousLearningCacheRootCapability capability)
    {
        if (OperatingSystem.IsLinux())
        {
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningCacheIoStage.BeforeExactDelete);
            MarkNamedRegularLeafDeleteOnClose(root, leafName, handle);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningCacheIoStage.AfterExactDelete);
            return;
        }

        var buffer = Marshal.AllocHGlobal(sizeof(int));
        try
        {
            Marshal.WriteInt32(buffer, 1);
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningCacheIoStage.BeforeExactDelete);
            if (!SetFileInformationByHandleWindows(
                    handle,
                    FileInfoByHandleClass.FileDispositionInfo,
                    buffer,
                    sizeof(int)))
            {
                throw new IOException("The exact-identity fixed cache leaf delete failed.");
            }
            capability.ObserveIoStageLocked(
                ServerOwnedContinuousLearningCacheIoStage.AfterExactDelete);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static void ThrowPosixExactNamespaceMutationUnsupported(
        SafeFileHandle handle,
        string canonicalLeafName)
    {
        EnsurePosixLinuxX64();
        // Linux не даёт безопасного аналога Windows handle-bound delete/rename:
        // unlinkat/renameat2 выбирают объект по имени в момент syscall, уже после
        // fd-validation. Поэтому сначала заново проверяем открытый fd и затем
        // fail closed до любой namespace mutation.
        if (!IsSafeSingleLeafName(canonicalLeafName))
            throw new IOException("The POSIX fixed leaf namespace mutation name is unsafe.");

        _ = ValidateRegularLeafPosix(handle, MaximumOperationBytes);
        throw new ContinuousLearningPosixExactMutationUnsupportedException();
    }

    private static void AcquirePosixOwnerLockIfNeeded(SafeFileHandle handle)
    {
        if (!OperatingSystem.IsLinux())
            return;

        // flock удерживается на открытом owner-lock fd: Linux не получает allow-all,
        // а конкурирующий владелец получает детерминированный fail-closed отказ.
        if (FlockPosix(
                ToPosixFileDescriptor(handle),
                PosixFlockExclusive | PosixFlockNonBlocking) == 0)
        {
            return;
        }

        var error = Marshal.GetLastPInvokeError();
        throw new IOException(
            error == PosixWouldBlock
                ? "The POSIX fixed owner lock is already held."
                : "The POSIX fixed owner lock acquisition failed.");
    }

    private static int ToPosixFileDescriptor(SafeFileHandle handle)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw new IOException("The POSIX fixed leaf handle is unavailable.");
        return checked((int)handle.DangerousGetHandle().ToInt64());
    }

    private static void EnsurePosixLinuxX64()
    {
        if (!OperatingSystem.IsLinux() ||
            RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            throw new IOException(
                "The POSIX fixed leaf backend is supported only on Linux x64.");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PosixStat
    {
        public ulong Device;
        public ulong Inode;
        public ulong LinkCount;
        public uint Mode;
        public uint Uid;
        public uint Gid;
        public int Padding0;
        public ulong DeviceId;
        public long Size;
        public long BlockSize;
        public long Blocks;
        public long AccessTimeSeconds;
        public long AccessTimeNanoseconds;
        public long ModificationTimeSeconds;
        public long ModificationTimeNanoseconds;
        public long ChangeTimeSeconds;
        public long ChangeTimeNanoseconds;
        public long Unused0;
        public long Unused1;
        public long Unused2;
    }

    private readonly record struct LeafInformation(
        ServerOwnedContinuousLearningCacheFileIdentity Identity,
        long Length);

    private enum FileInfoByHandleClass
    {
        FileStandardInfo = 1,
        FileRenameInfo = 3,
        FileDispositionInfo = 4,
        FileEndOfFileInfo = 6,
        FileAttributeTagInfo = 9,
        FileIdInfo = 18,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileStandardInformation
    {
        public long AllocationSize;
        public long EndOfFile;
        public uint NumberOfLinks;
        public byte DeletePending;
        public byte Directory;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileAttributeTagInformation
    {
        public uint FileAttributes;
        public uint ReparseTag;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileId128
    {
        public ulong Low;
        public ulong High;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileIdInformation
    {
        public ulong VolumeSerialNumber;
        public FileId128 FileId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileEndOfFileInformation
    {
        public long EndOfFile;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UnicodeString
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ObjectAttributes
    {
        public int Length;
        public IntPtr RootDirectory;
        public IntPtr ObjectName;
        public uint Attributes;
        public IntPtr SecurityDescriptor;
        public IntPtr SecurityQualityOfService;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IoStatusBlock
    {
        public IntPtr Status;
        public IntPtr Information;
    }

    [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
    private static extern int OpenAtPosix(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int FStatPosix(
        int fileDescriptor,
        out PosixStat stat);

    [DllImport("libc", EntryPoint = "ftruncate", SetLastError = true)]
    private static extern int FTruncatePosix(
        int fileDescriptor,
        long length);

    [DllImport("libc", EntryPoint = "fsync", SetLastError = true)]
    private static extern int FsyncPosix(int fileDescriptor);

    [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
    private static extern int FlockPosix(
        int fileDescriptor,
        int operation);

    [DllImport("libc", EntryPoint = "geteuid")]
    private static extern uint GetEffectiveUserIdPosix();

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileAttributeTagInformation fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileStandardInformation fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileIdInformation fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "SetFileInformationByHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandleWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        ref FileEndOfFileInformation fileInformation,
        uint bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "SetFileInformationByHandle", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandleWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        IntPtr fileInformation,
        int bufferSize);

    [DllImport("kernel32.dll", EntryPoint = "FlushFileBuffers", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FlushFileBuffersWindows(SafeFileHandle file);

    [DllImport("ntdll.dll", EntryPoint = "NtCreateFile")]
    private static extern int NtCreateFileWindows(
        out SafeFileHandle fileHandle,
        uint desiredAccess,
        ref ObjectAttributes objectAttributes,
        out IoStatusBlock ioStatusBlock,
        IntPtr allocationSize,
        uint fileAttributes,
        uint shareAccess,
        uint createDisposition,
        uint createOptions,
        IntPtr extendedAttributes,
        uint extendedAttributesLength);

    [DllImport("ntdll.dll", EntryPoint = "NtSetInformationFile")]
    private static extern int NtSetInformationFileWindows(
        SafeFileHandle fileHandle,
        out IoStatusBlock ioStatusBlock,
        IntPtr fileInformation,
        uint length,
        int fileInformationClass);

    [DllImport("ntdll.dll", EntryPoint = "NtFlushBuffersFile")]
    private static extern int NtFlushBuffersFileWindows(
        SafeFileHandle fileHandle,
        out IoStatusBlock ioStatusBlock);
}
