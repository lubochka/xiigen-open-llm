// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using Microsoft.Win32.SafeHandles;

/// <summary>
/// Tells a caller whether a DPO dataset came from an already sealed repository
/// manifest or had to be created and sealed during this request.
/// </summary>
public enum RepositoryOwnedDpoDatasetResolutionSource
{
    ExistingSealedRepositoryDataset = 1,
    NewlyPersistedRepositoryDataset = 2,
}

/// <summary>
/// Result of resolving one domain/pattern/version DPO dataset from the
/// canonical repository-owned training-data tree.
/// </summary>
public sealed record RepositoryOwnedDpoDatasetResolution(
    RepositoryOwnedDpoDatasetOpenResult Dataset,
    RepositoryOwnedDpoDatasetResolutionSource Source);

/// <summary>
/// Opens a saved DPO dataset before any expensive teacher/generator path is
/// allowed to run.
/// </summary>
public static class RepositoryOwnedDpoDatasetResolver
{
    public const string MissingErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_MISSING";

    public const string InvalidErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_RESOLUTION_INVALID";

    public const string BusyErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_RESOLUTION_BUSY";

    public const string UnavailableErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_RESOLUTION_UNAVAILABLE";

    public const string CancelledErrorCode =
        "REPOSITORY_OWNED_DPO_DATASET_RESOLUTION_CANCELLED";

    private const string CreationLeaseDirectoryName =
        ".dpo-dataset-creation-leases";
    private const string CreationLeaseFileSuffix = ".lock";
    private const int FileBufferSize = 4096;
    private const long CreationLeaseLockOffset = 0;
    private const long CreationLeaseLockLength = 1;
    private const byte CreationLeaseLockByte = 0;
    private const int LinuxOpenReadOnly = 0;
    private const int LinuxOpenReadWrite = 0x2;
    private const int LinuxOpenCreate = 0x40;
    private const int LinuxOpenExclusive = 0x80;
    private const int LinuxOpenDirectory = 0x10000;
    private const int LinuxOpenNoFollow = 0x20000;
    private const int LinuxOpenCloseOnExec = 0x80000;
    private const int LinuxErrnoNoEntry = 2;
    private const int LinuxErrnoExists = 17;
    private const uint LinuxOwnerDirectoryModeBits = 0x1c0; // 0700
    private const uint LinuxOwnerFileModeBits = 0x180; // 0600
    private static readonly TimeSpan DefaultCreationLeaseTimeout =
        TimeSpan.FromSeconds(10);
    private static readonly UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private static readonly UnixFileMode OwnerFileMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite;
    private static readonly UnixFileMode GroupOrOtherAccessMask =
        UnixFileMode.GroupRead | UnixFileMode.GroupWrite |
        UnixFileMode.GroupExecute | UnixFileMode.OtherRead |
        UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;
    private static readonly UnixFileMode SpecialUnixModeBits =
        UnixFileMode.SetUser | UnixFileMode.SetGroup | UnixFileMode.StickyBit;
    private static readonly object CreationProcessGateRegistrySync = new();
    private static readonly Dictionary<string, CreationProcessGateEntry>
        CreationProcessGateRegistry =
            new(OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);

    public static Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        OpenExistingOrCreateAndSealAsync(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity,
            Func<RepositoryOwnedDpoDatasetAddress, CancellationToken,
                Task<OperationResult<IReadOnlyList<DpoPair>>>>? createPairsAsync,
            CancellationToken cancellationToken = default)
        => OpenExistingOrCreateAndSealAsync(
            address,
            expectedIdentity,
            createPairsAsync,
            cancellationToken,
            DefaultCreationLeaseTimeout);

    public static OperationResult<RepositoryOwnedDpoDatasetResolution>
        OpenExistingOrCreateAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity,
            Func<RepositoryOwnedDpoDatasetAddress,
                OperationResult<IReadOnlyList<DpoPair>>>? createPairs)
        => OpenExistingOrCreateAndSeal(
            address,
            expectedIdentity,
            createPairs,
            CancellationToken.None);

    public static OperationResult<RepositoryOwnedDpoDatasetResolution>
        OpenExistingOrCreateAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity,
            Func<RepositoryOwnedDpoDatasetAddress,
                OperationResult<IReadOnlyList<DpoPair>>>? createPairs,
            CancellationToken cancellationToken)
        => OpenExistingOrCreateAndSeal(
            address,
            expectedIdentity,
            createPairs,
            cancellationToken,
            DefaultCreationLeaseTimeout);

    // WHY: tests need a short bounded wait to prove async timeout and
    // cancellation without making production callers tune the safety window.
    internal static async Task<OperationResult<RepositoryOwnedDpoDatasetResolution>>
        OpenExistingOrCreateAndSealAsync(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity,
            Func<RepositoryOwnedDpoDatasetAddress, CancellationToken,
                Task<OperationResult<IReadOnlyList<DpoPair>>>>? createPairsAsync,
            CancellationToken cancellationToken,
            TimeSpan creationLeaseTimeout)
    {
        if (!IsValidAddress(address)
            || expectedIdentity is null
            || !RepositoryOwnedDpoDatasetManifestSealer
                .IsValidRequestIdentity(expectedIdentity)
            || creationLeaseTimeout <= TimeSpan.Zero
            || creationLeaseTimeout == Timeout.InfiniteTimeSpan)
        {
            return Invalid<RepositoryOwnedDpoDatasetResolution>();
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled<RepositoryOwnedDpoDatasetResolution>();

        var existing = TryOpenExisting(address, expectedIdentity);
        if (existing.IsSuccess && existing.Value is not null)
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Ok(
                new RepositoryOwnedDpoDatasetResolution(
                    existing.Value,
                    RepositoryOwnedDpoDatasetResolutionSource
                        .ExistingSealedRepositoryDataset));
        }

        if (!StringComparer.Ordinal.Equals(
                existing.ErrorCode,
                RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode))
        {
            return existing.MapError<RepositoryOwnedDpoDatasetResolution>();
        }

        // A live same-address creator may have made the seal lock/pairs visible
        // before publishing its manifest. Serialize the reread with that creator;
        // an observed partial history can never authorize another creator.
        var observedPartialDataset = DatasetLooksPartiallyWritten(address);
        if (createPairsAsync is null)
            return observedPartialDataset
                ? existing.MapError<RepositoryOwnedDpoDatasetResolution>()
                : Missing<RepositoryOwnedDpoDatasetResolution>();

        var timeoutBudget = Stopwatch.StartNew();
        var leaseResult = await AcquireCreationLeaseAsync(
                address,
                timeoutBudget,
                creationLeaseTimeout,
                cancellationToken)
            .ConfigureAwait(false);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<RepositoryOwnedDpoDatasetResolution>();

        using var lease = leaseResult.Value;
        if (!lease.StillOwnsRequestedLease()
            || !IsValidAddress(address))
        {
            return Unavailable<RepositoryOwnedDpoDatasetResolution>();
        }

        if (DatasetLooksPartiallyWrittenWithoutManifest(address))
            return observedPartialDataset
                ? existing.MapError<RepositoryOwnedDpoDatasetResolution>()
                : Unavailable<RepositoryOwnedDpoDatasetResolution>();

        var existingUnderLease = TryOpenExisting(address, expectedIdentity);
        if (existingUnderLease.IsSuccess && existingUnderLease.Value is not null)
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Ok(
                new RepositoryOwnedDpoDatasetResolution(
                    existingUnderLease.Value,
                    RepositoryOwnedDpoDatasetResolutionSource
                        .ExistingSealedRepositoryDataset));
        }

        if (!StringComparer.Ordinal.Equals(
                existingUnderLease.ErrorCode,
                RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode))
        {
            return existingUnderLease.MapError<RepositoryOwnedDpoDatasetResolution>();
        }

        if (observedPartialDataset || DatasetLooksPartiallyWritten(address))
            return existingUnderLease.MapError<RepositoryOwnedDpoDatasetResolution>();

        if (createPairsAsync is null)
            return Missing<RepositoryOwnedDpoDatasetResolution>();

        if (cancellationToken.IsCancellationRequested)
            return Cancelled<RepositoryOwnedDpoDatasetResolution>();

        OperationResult<IReadOnlyList<DpoPair>> created;
        try
        {
            // WHY: async callers may wait on a teacher/server route without
            // blocking an ASP.NET request thread. The reservation still wraps
            // the exact address, so only one clean-miss creator can run.
            created = await createPairsAsync(address, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            CleanupUnsealedDatasetRoot(address);
            return Cancelled<RepositoryOwnedDpoDatasetResolution>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            CleanupUnsealedDatasetRoot(address);
            return Invalid<RepositoryOwnedDpoDatasetResolution>();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            CleanupUnsealedDatasetRoot(address);
            return Cancelled<RepositoryOwnedDpoDatasetResolution>();
        }

        if (!created.IsSuccess || created.Value is null)
        {
            CleanupUnsealedDatasetRoot(address);
            return created.MapError<RepositoryOwnedDpoDatasetResolution>();
        }

        var persisted =
            RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
                address,
                expectedIdentity,
                created.Value);
        if (!persisted.IsSuccess)
        {
            CleanupUnsealedDatasetRoot(address);
            return persisted.MapError<RepositoryOwnedDpoDatasetResolution>();
        }

        var reopened =
            RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                address,
                expectedIdentity);
        if (!reopened.IsSuccess || reopened.Value is null)
            return reopened.MapError<RepositoryOwnedDpoDatasetResolution>();

        return OperationResult<RepositoryOwnedDpoDatasetResolution>.Ok(
            new RepositoryOwnedDpoDatasetResolution(
                reopened.Value,
                RepositoryOwnedDpoDatasetResolutionSource
                    .NewlyPersistedRepositoryDataset));
    }

    // WHY: production callers always keep the bounded ten-second wait above.
    // Tests use this internal seam to prove timeout behaviour without making
    // the test suite sleep for ten seconds or widening the public API.
    internal static OperationResult<RepositoryOwnedDpoDatasetResolution>
        OpenExistingOrCreateAndSeal(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity,
            Func<RepositoryOwnedDpoDatasetAddress,
                OperationResult<IReadOnlyList<DpoPair>>>? createPairs,
            CancellationToken cancellationToken,
            TimeSpan creationLeaseTimeout)
    {
        if (!IsValidAddress(address)
            || expectedIdentity is null
            || !RepositoryOwnedDpoDatasetManifestSealer
                .IsValidRequestIdentity(expectedIdentity)
            || creationLeaseTimeout <= TimeSpan.Zero
            || creationLeaseTimeout == Timeout.InfiniteTimeSpan)
            return Invalid<RepositoryOwnedDpoDatasetResolution>();

        if (cancellationToken.IsCancellationRequested)
            return Cancelled<RepositoryOwnedDpoDatasetResolution>();

        var existing = TryOpenExisting(address, expectedIdentity);
        if (existing.IsSuccess && existing.Value is not null)
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Ok(
                new RepositoryOwnedDpoDatasetResolution(
                    existing.Value,
                    RepositoryOwnedDpoDatasetResolutionSource
                        .ExistingSealedRepositoryDataset));
        }

        if (!StringComparer.Ordinal.Equals(
                existing.ErrorCode,
                RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode))
        {
            return existing.MapError<RepositoryOwnedDpoDatasetResolution>();
        }

        // Keep sync callers on the same creation lease/reread contract as async.
        var observedPartialDataset = DatasetLooksPartiallyWritten(address);
        if (createPairs is null)
            return observedPartialDataset
                ? existing.MapError<RepositoryOwnedDpoDatasetResolution>()
                : Missing<RepositoryOwnedDpoDatasetResolution>();

        var timeoutBudget = Stopwatch.StartNew();
        var leaseResult = AcquireCreationLease(
            address,
            timeoutBudget,
            creationLeaseTimeout,
            cancellationToken);
        if (!leaseResult.IsSuccess || leaseResult.Value is null)
            return leaseResult.MapError<RepositoryOwnedDpoDatasetResolution>();

        using var lease = leaseResult.Value;
        if (!lease.StillOwnsRequestedLease()
            || !IsValidAddress(address))
        {
            return Unavailable<RepositoryOwnedDpoDatasetResolution>();
        }

        if (DatasetLooksPartiallyWrittenWithoutManifest(address))
            return observedPartialDataset
                ? existing.MapError<RepositoryOwnedDpoDatasetResolution>()
                : Unavailable<RepositoryOwnedDpoDatasetResolution>();

        var existingUnderLease = TryOpenExisting(address, expectedIdentity);
        if (existingUnderLease.IsSuccess && existingUnderLease.Value is not null)
        {
            return OperationResult<RepositoryOwnedDpoDatasetResolution>.Ok(
                new RepositoryOwnedDpoDatasetResolution(
                    existingUnderLease.Value,
                    RepositoryOwnedDpoDatasetResolutionSource
                        .ExistingSealedRepositoryDataset));
        }

        if (!StringComparer.Ordinal.Equals(
                existingUnderLease.ErrorCode,
                RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode))
        {
            return existingUnderLease.MapError<RepositoryOwnedDpoDatasetResolution>();
        }

        if (observedPartialDataset || DatasetLooksPartiallyWritten(address))
            return existingUnderLease.MapError<RepositoryOwnedDpoDatasetResolution>();

        if (createPairs is null)
            return Missing<RepositoryOwnedDpoDatasetResolution>();

        if (cancellationToken.IsCancellationRequested)
            return Cancelled<RepositoryOwnedDpoDatasetResolution>();

        OperationResult<IReadOnlyList<DpoPair>> created;
        try
        {
            created = createPairs(address);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            CleanupUnsealedDatasetRoot(address);
            return Invalid<RepositoryOwnedDpoDatasetResolution>();
        }

        if (!created.IsSuccess || created.Value is null)
        {
            CleanupUnsealedDatasetRoot(address);
            return created.MapError<RepositoryOwnedDpoDatasetResolution>();
        }

        // WHY: the open-teacher or generator is allowed only after a clean
        // repository miss AND while this exact dataset address is reserved.
        // A second process waits, rereads the sealed manifest, and therefore
        // does not ask the Chinese model to recreate identical DPO rows.
        var persisted =
            RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
                address,
                expectedIdentity,
                created.Value);
        if (!persisted.IsSuccess)
        {
            CleanupUnsealedDatasetRoot(address);
            return persisted.MapError<RepositoryOwnedDpoDatasetResolution>();
        }

        var reopened =
            RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                address,
                expectedIdentity);
        if (!reopened.IsSuccess || reopened.Value is null)
            return reopened.MapError<RepositoryOwnedDpoDatasetResolution>();

        return OperationResult<RepositoryOwnedDpoDatasetResolution>.Ok(
            new RepositoryOwnedDpoDatasetResolution(
                reopened.Value,
                RepositoryOwnedDpoDatasetResolutionSource
                    .NewlyPersistedRepositoryDataset));
    }

    private static OperationResult<RepositoryOwnedDpoDatasetOpenResult>
        TryOpenExisting(
            RepositoryOwnedDpoDatasetAddress address,
            RepositoryOwnedDpoDatasetRequestIdentity expectedIdentity)
    {
        try
        {
            return RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                address,
                expectedIdentity);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<RepositoryOwnedDpoDatasetOpenResult>.Fail(
                "Repository-owned DPO dataset could not be opened.",
                RepositoryOwnedDpoDatasetManifestSealer.UnavailableErrorCode);
        }
    }

    private static bool DatasetLooksPartiallyWritten(
        RepositoryOwnedDpoDatasetAddress address)
    {
        try
        {
            if (File.Exists(address.ManifestPath))
                return true;

            if (!Directory.Exists(address.DatasetRoot))
                return false;

            if (Directory.EnumerateFileSystemEntries(address.DatasetRoot).Any(
                    path => !Path.GetFileName(path)
                        .Equals(
                            RepositoryOwnedTrainingArtifactLayout
                                .PairStoreDirectoryName,
                            StringComparison.Ordinal)))
            {
                return true;
            }

            return Directory.Exists(address.PairsRoot)
                   && Directory.EnumerateFileSystemEntries(
                       address.PairsRoot).Any();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return true;
        }
    }

    private static bool DatasetLooksPartiallyWrittenWithoutManifest(
        RepositoryOwnedDpoDatasetAddress address)
    {
        try
        {
            if (File.Exists(address.ManifestPath))
                return false;

            return DatasetLooksPartiallyWritten(address);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return true;
        }
    }

    private static OperationResult<CreationLease> AcquireCreationLease(
        RepositoryOwnedDpoDatasetAddress address,
        Stopwatch timeoutBudget,
        TimeSpan creationLeaseTimeout,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!SecureLocalFileBoundary.IsSupportedPlatform())
                return Unavailable<CreationLease>();

            var paths = ResolveCreationLeasePaths(address);
            if (!paths.IsSuccess || paths.Value is null)
                return paths.MapError<CreationLease>();

            var directoryReady = EnsureCreationLeaseDirectory(paths.Value);
            if (!directoryReady.IsSuccess)
                return directoryReady.MapError<CreationLease>();

            var processLease = AcquireCreationProcessLease(
                paths.Value.LockPath,
                timeoutBudget,
                creationLeaseTimeout,
                cancellationToken);
            if (!processLease.IsSuccess || processLease.Value is null)
                return processLease.MapError<CreationLease>();

            var processGate = processLease.Value;
            try
            {
                var fileLease = AcquireCreationFileLease(
                    paths.Value,
                    timeoutBudget,
                    creationLeaseTimeout,
                    cancellationToken);
                if (!fileLease.IsSuccess || fileLease.Value is null)
                {
                    processGate.Dispose();
                    return fileLease.MapError<CreationLease>();
                }

                return OperationResult<CreationLease>.Ok(
                    new CreationLease(processGate, fileLease.Value, paths.Value));
            }
            catch
            {
                processGate.Dispose();
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            return Cancelled<CreationLease>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<CreationLease>();
        }
    }

    private static async Task<OperationResult<CreationLease>>
        AcquireCreationLeaseAsync(
            RepositoryOwnedDpoDatasetAddress address,
            Stopwatch timeoutBudget,
            TimeSpan creationLeaseTimeout,
            CancellationToken cancellationToken)
    {
        try
        {
            if (!SecureLocalFileBoundary.IsSupportedPlatform())
                return Unavailable<CreationLease>();

            var paths = ResolveCreationLeasePaths(address);
            if (!paths.IsSuccess || paths.Value is null)
                return paths.MapError<CreationLease>();

            var directoryReady = EnsureCreationLeaseDirectory(paths.Value);
            if (!directoryReady.IsSuccess)
                return directoryReady.MapError<CreationLease>();

            var processLease = await AcquireCreationProcessLeaseAsync(
                    paths.Value.LockPath,
                    timeoutBudget,
                    creationLeaseTimeout,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!processLease.IsSuccess || processLease.Value is null)
                return processLease.MapError<CreationLease>();

            var processGate = processLease.Value;
            try
            {
                var fileLease = await AcquireCreationFileLeaseAsync(
                        paths.Value,
                        timeoutBudget,
                        creationLeaseTimeout,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (!fileLease.IsSuccess || fileLease.Value is null)
                {
                    processGate.Dispose();
                    return fileLease.MapError<CreationLease>();
                }

                return OperationResult<CreationLease>.Ok(
                    new CreationLease(processGate, fileLease.Value, paths.Value));
            }
            catch
            {
                processGate.Dispose();
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            return Cancelled<CreationLease>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<CreationLease>();
        }
    }

    private static OperationResult<CreationProcessLease> AcquireCreationProcessLease(
        string canonicalLockPath,
        Stopwatch timeoutBudget,
        TimeSpan creationLeaseTimeout,
        CancellationToken cancellationToken)
    {
        CreationProcessGateEntry entry;
        lock (CreationProcessGateRegistrySync)
        {
            if (!CreationProcessGateRegistry.TryGetValue(
                    canonicalLockPath,
                    out entry!))
            {
                entry = new CreationProcessGateEntry();
                CreationProcessGateRegistry.Add(canonicalLockPath, entry);
            }

            checked
            {
                entry.ReferenceCount++;
            }
        }

        try
        {
            var remaining = creationLeaseTimeout - timeoutBudget.Elapsed;
            if (remaining <= TimeSpan.Zero)
            {
                ReleaseCreationProcessGateReference(canonicalLockPath, entry);
                return Busy<CreationProcessLease>();
            }

            if (!entry.Semaphore.Wait(remaining, cancellationToken))
            {
                ReleaseCreationProcessGateReference(canonicalLockPath, entry);
                return Busy<CreationProcessLease>();
            }

            return OperationResult<CreationProcessLease>.Ok(
                new CreationProcessLease(canonicalLockPath, entry));
        }
        catch (OperationCanceledException)
        {
            ReleaseCreationProcessGateReference(canonicalLockPath, entry);
            return Cancelled<CreationProcessLease>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            ReleaseCreationProcessGateReference(canonicalLockPath, entry);
            return Unavailable<CreationProcessLease>();
        }
    }

    private static async Task<OperationResult<CreationProcessLease>>
        AcquireCreationProcessLeaseAsync(
            string canonicalLockPath,
            Stopwatch timeoutBudget,
            TimeSpan creationLeaseTimeout,
            CancellationToken cancellationToken)
    {
        CreationProcessGateEntry entry;
        lock (CreationProcessGateRegistrySync)
        {
            if (!CreationProcessGateRegistry.TryGetValue(
                    canonicalLockPath,
                    out entry!))
            {
                entry = new CreationProcessGateEntry();
                CreationProcessGateRegistry.Add(canonicalLockPath, entry);
            }

            checked
            {
                entry.ReferenceCount++;
            }
        }

        try
        {
            var remaining = creationLeaseTimeout - timeoutBudget.Elapsed;
            if (remaining <= TimeSpan.Zero)
            {
                ReleaseCreationProcessGateReference(canonicalLockPath, entry);
                return Busy<CreationProcessLease>();
            }

            var acquired = await entry.Semaphore
                .WaitAsync(remaining, cancellationToken)
                .ConfigureAwait(false);
            if (!acquired)
            {
                ReleaseCreationProcessGateReference(canonicalLockPath, entry);
                return Busy<CreationProcessLease>();
            }

            return OperationResult<CreationProcessLease>.Ok(
                new CreationProcessLease(canonicalLockPath, entry));
        }
        catch (OperationCanceledException)
        {
            ReleaseCreationProcessGateReference(canonicalLockPath, entry);
            return Cancelled<CreationProcessLease>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            ReleaseCreationProcessGateReference(canonicalLockPath, entry);
            return Unavailable<CreationProcessLease>();
        }
    }

    private static OperationResult<CreationFileLease> AcquireCreationFileLease(
        CreationLeasePaths paths,
        Stopwatch timeoutBudget,
        TimeSpan creationLeaseTimeout,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (creationLeaseTimeout - timeoutBudget.Elapsed <= TimeSpan.Zero)
                return Busy<CreationFileLease>();

            FileStream? stream = null;
            CreationFileLease? lease = null;
            try
            {
                var streamResult = OpenPreparedCreationLeaseLockFile(paths);
                if (!streamResult.IsSuccess || streamResult.Value is null)
                    return streamResult.MapError<CreationFileLease>();
                stream = streamResult.Value;

                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    stream.Lock(
                        CreationLeaseLockOffset,
                        CreationLeaseLockLength);
                }
                else
                {
                    stream.Dispose();
                    return Unavailable<CreationFileLease>();
                }

                lease = new CreationFileLease(stream);
                if (!lease.StillOwns(paths.LockPath)
                    || CreationLeasePathIsUnsafe(paths)
                    || !CreationLeaseLockHandleIsSafe(
                        stream.SafeFileHandle,
                        paths.LockPath)
                    || !CreationLeaseLockContentIsExpected(stream))
                {
                    lease.Dispose();
                    return Unavailable<CreationFileLease>();
                }

                return OperationResult<CreationFileLease>.Ok(lease);
            }
            catch (IOException exception)
                when (FileDpoPairStore.IsLockContention(
                    exception,
                    OperatingSystem.IsWindows()))
            {
                lease?.Dispose();
                stream?.Dispose();
                var remaining = creationLeaseTimeout - timeoutBudget.Elapsed;
                if (remaining <= TimeSpan.Zero)
                    return Busy<CreationFileLease>();

                var delay = remaining < TimeSpan.FromMilliseconds(25)
                    ? remaining
                    : TimeSpan.FromMilliseconds(25);
                if (cancellationToken.WaitHandle.WaitOne(delay))
                    return Cancelled<CreationFileLease>();
            }
            catch (OperationCanceledException)
            {
                lease?.Dispose();
                stream?.Dispose();
                return Cancelled<CreationFileLease>();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                lease?.Dispose();
                stream?.Dispose();
                return Unavailable<CreationFileLease>();
            }
        }
    }

    private static async Task<OperationResult<CreationFileLease>>
        AcquireCreationFileLeaseAsync(
            CreationLeasePaths paths,
            Stopwatch timeoutBudget,
            TimeSpan creationLeaseTimeout,
            CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (creationLeaseTimeout - timeoutBudget.Elapsed <= TimeSpan.Zero)
                return Busy<CreationFileLease>();

            FileStream? stream = null;
            CreationFileLease? lease = null;
            try
            {
                var streamResult = OpenPreparedCreationLeaseLockFile(paths);
                if (!streamResult.IsSuccess || streamResult.Value is null)
                    return streamResult.MapError<CreationFileLease>();
                stream = streamResult.Value;

                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    stream.Lock(
                        CreationLeaseLockOffset,
                        CreationLeaseLockLength);
                }
                else
                {
                    stream.Dispose();
                    return Unavailable<CreationFileLease>();
                }

                lease = new CreationFileLease(stream);
                if (!lease.StillOwns(paths.LockPath)
                    || CreationLeasePathIsUnsafe(paths)
                    || !CreationLeaseLockHandleIsSafe(
                        stream.SafeFileHandle,
                        paths.LockPath)
                    || !CreationLeaseLockContentIsExpected(stream))
                {
                    lease.Dispose();
                    return Unavailable<CreationFileLease>();
                }

                return OperationResult<CreationFileLease>.Ok(lease);
            }
            catch (IOException exception)
                when (FileDpoPairStore.IsLockContention(
                    exception,
                    OperatingSystem.IsWindows()))
            {
                lease?.Dispose();
                stream?.Dispose();
                var remaining = creationLeaseTimeout - timeoutBudget.Elapsed;
                if (remaining <= TimeSpan.Zero)
                    return Busy<CreationFileLease>();

                var delay = remaining < TimeSpan.FromMilliseconds(25)
                    ? remaining
                    : TimeSpan.FromMilliseconds(25);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                lease?.Dispose();
                stream?.Dispose();
                return Cancelled<CreationFileLease>();
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                lease?.Dispose();
                stream?.Dispose();
                return Unavailable<CreationFileLease>();
            }
        }
    }

    private static OperationResult<CreationLeasePaths> ResolveCreationLeasePaths(
        RepositoryOwnedDpoDatasetAddress address)
    {
        try
        {
            if (!IsValidAddress(address))
                return Invalid<CreationLeasePaths>();

            var leaseRoot = Path.GetFullPath(Path.Combine(
                address.RepositoryRoot,
                "training-artifacts"));
            var creationLeaseRoot = Path.GetFullPath(Path.Combine(
                leaseRoot,
                CreationLeaseDirectoryName));
            var domainLeaseDirectory = Path.GetFullPath(Path.Combine(
                creationLeaseRoot,
                address.DomainSlug));
            var lockDirectory = Path.GetFullPath(Path.Combine(
                domainLeaseDirectory,
                address.DesignPatternSlug));
            var lockPath = Path.GetFullPath(Path.Combine(
                lockDirectory,
                address.DatasetVersion + CreationLeaseFileSuffix));

            if (!IsStrictDescendant(leaseRoot, address.RepositoryRoot)
                || !IsStrictDescendant(creationLeaseRoot, leaseRoot)
                || !IsStrictDescendant(domainLeaseDirectory, creationLeaseRoot)
                || !IsStrictDescendant(lockDirectory, domainLeaseDirectory)
                || !IsStrictDescendant(lockPath, lockDirectory)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    leaseRoot)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    creationLeaseRoot)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    domainLeaseDirectory)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    lockDirectory)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    lockPath)
                || ExpectedDirectorySegmentIsFile(
                    address.RepositoryRoot,
                    lockDirectory))
            {
                return Unavailable<CreationLeasePaths>();
            }

            return OperationResult<CreationLeasePaths>.Ok(
                new CreationLeasePaths(
                    leaseRoot,
                    creationLeaseRoot,
                    domainLeaseDirectory,
                    lockDirectory,
                    lockPath));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<CreationLeasePaths>();
        }
    }

    private static OperationResult<bool> EnsureCreationLeaseDirectory(
        CreationLeasePaths paths)
    {
        try
        {
            if (OperatingSystem.IsLinux())
            {
                if (CreationLeasePathIsUnsafe(paths)
                    || File.Exists(paths.TrainingArtifactsRoot))
                {
                    return Unavailable<bool>();
                }

                if (!Directory.Exists(paths.TrainingArtifactsRoot))
                    Directory.CreateDirectory(paths.TrainingArtifactsRoot);

                return EnsureLinuxOwnerOnlyCreationLeaseDirectoryChain(paths)
                    ? OperationResult<bool>.Ok(true)
                    : Unavailable<bool>();
            }

            if (CreationLeasePathIsUnsafe(paths)
                || File.Exists(paths.TrainingArtifactsRoot)
                || File.Exists(paths.LeaseRoot)
                || File.Exists(paths.DomainLeaseDirectory)
                || File.Exists(paths.LockDirectory))
            {
                return Unavailable<bool>();
            }

            if (!Directory.Exists(paths.TrainingArtifactsRoot))
                Directory.CreateDirectory(paths.TrainingArtifactsRoot);

            foreach (var leaseDirectory in CreationLeaseDirectories(paths))
            {
                if (!EnsureOwnerOnlyCreationLeaseDirectory(paths, leaseDirectory))
                    return Unavailable<bool>();
            }

            if (CreationLeasePathIsUnsafe(paths)
                || !SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                    paths.LeaseRoot)
                || !SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                    paths.DomainLeaseDirectory)
                || !SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                    paths.LockDirectory))
            {
                return Unavailable<bool>();
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<bool>();
        }
    }

    private static bool CreationLeasePathIsUnsafe(CreationLeasePaths paths)
        => SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
               paths.TrainingArtifactsRoot)
           || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
               paths.LeaseRoot)
           || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
               paths.DomainLeaseDirectory)
           || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
               paths.LockDirectory)
           || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
               paths.LockPath);

    private static IReadOnlyList<string> CreationLeaseDirectories(
        CreationLeasePaths paths)
        => [paths.LeaseRoot, paths.DomainLeaseDirectory, paths.LockDirectory];

    private static bool EnsureOwnerOnlyCreationLeaseDirectory(
        CreationLeasePaths paths,
        string leaseDirectory)
    {
        // WHY: .NET's recursive POSIX create applies the requested mode only to
        // the leaf. Creating each lease level separately prevents a clean clone
        // from getting world-readable intermediate reservation directories.
        if (CreationLeasePathIsUnsafe(paths) || File.Exists(leaseDirectory))
            return false;

        if (!Directory.Exists(leaseDirectory))
        {
            if (OperatingSystem.IsWindows())
                Directory.CreateDirectory(leaseDirectory);
            else
                Directory.CreateDirectory(leaseDirectory, OwnerDirectoryMode);
        }

        // WHY: pre-existing broad-mode or reparse lease directories stay
        // fail-closed. The resolver must not chmod unknown prior content or
        // silently turn an attacker-observable reservation path into authority.
        return !CreationLeasePathIsUnsafe(paths)
               && Directory.Exists(leaseDirectory)
               && SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                   leaseDirectory);
    }

    [SupportedOSPlatform("linux")]
    private static bool EnsureLinuxOwnerOnlyCreationLeaseDirectoryChain(
        CreationLeasePaths paths)
    {
        if (!OperatingSystem.IsLinux() || CreationLeasePathIsUnsafe(paths))
            return false;

        SafeFileHandle? current = null;
        try
        {
            current = OpenLinuxDirectoryNoFollow(
                paths.TrainingArtifactsRoot,
                out _);
            if (current is null
                || !LinuxDirectoryHandleMatchesPath(
                    current,
                    paths.TrainingArtifactsRoot))
            {
                return false;
            }

            foreach (var (segment, fullPath) in new[]
                     {
                         (CreationLeaseDirectoryName, paths.LeaseRoot),
                         (Path.GetFileName(paths.DomainLeaseDirectory),
                             paths.DomainLeaseDirectory),
                         (Path.GetFileName(paths.LockDirectory),
                             paths.LockDirectory),
                     })
            {
                if (string.IsNullOrWhiteSpace(segment)
                    || segment.IndexOfAny(
                        [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar])
                    >= 0)
                {
                    return false;
                }

                var child = OpenOrCreateLinuxLeaseDirectoryAt(
                    current,
                    segment,
                    fullPath);
                if (child is null
                    || !EnsureLinuxDirectoryHandleOwnerOnly(child, fullPath))
                {
                    child?.Dispose();
                    return false;
                }

                current.Dispose();
                current = child;
            }

            return !CreationLeasePathIsUnsafe(paths)
                   && SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                       paths.LeaseRoot)
                   && SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                       paths.DomainLeaseDirectory)
                   && SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(
                       paths.LockDirectory);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
        finally
        {
            current?.Dispose();
        }
    }

    [SupportedOSPlatform("linux")]
    private static SafeFileHandle? OpenOrCreateLinuxLeaseDirectoryAt(
        SafeFileHandle parent,
        string segment,
        string fullPath)
    {
        var child = OpenLinuxDirectoryAtNoFollow(parent, segment, out var errno);
        if (child is not null)
            return LinuxDirectoryHandleMatchesPath(child, fullPath)
                ? child
                : DisposeAndNull(child);

        if (errno != LinuxErrnoNoEntry)
            return null;

        if (LinuxMkdirAt(parent, segment, LinuxOwnerDirectoryModeBits) != 0)
        {
            errno = Marshal.GetLastPInvokeError();
            if (errno != LinuxErrnoExists)
                return null;
        }

        child = OpenLinuxDirectoryAtNoFollow(parent, segment, out _);
        if (child is null)
            return null;
        if (LinuxDirectoryHandleMatchesPath(child, fullPath))
            return child;
        child.Dispose();
        return null;
    }

    [SupportedOSPlatform("linux")]
    private static bool EnsureLinuxDirectoryHandleOwnerOnly(
        SafeFileHandle handle,
        string requestedPath)
    {
        if (!LinuxDirectoryHandleMatchesPath(handle, requestedPath))
            return false;

        var mode = File.GetUnixFileMode(handle);
        if (!UnixModeCanBeNarrowedToOwner(mode, OwnerDirectoryMode))
            return false;

        if (!UnixModeIsOwnerOnly(mode, OwnerDirectoryMode))
        {
            // Важное место безопасности: Git clone даёт 0755, но chmod здесь
            // идёт только через уже открытый no-follow дескриптор точного
            // lease-каталога. Мы не меняем весь `training-artifacts` и не
            // делаем «проверил путь — chmodнул путь».
            File.SetUnixFileMode(handle, OwnerDirectoryMode);
        }

        return LinuxDirectoryHandleMatchesPath(handle, requestedPath)
               && UnixModeIsOwnerOnly(
                   File.GetUnixFileMode(handle),
                   OwnerDirectoryMode);
    }

    private static OperationResult<FileStream> OpenPreparedCreationLeaseLockFile(
        CreationLeasePaths paths)
    {
        if (OperatingSystem.IsLinux())
            return OpenPreparedLinuxCreationLeaseLockFile(paths);

        if (CreationLeasePathIsUnsafe(paths))
            return Unavailable<FileStream>();

        FileStream stream;
        bool newlyCreated;
        try
        {
            // A zero-byte file can come from a broken tracked lock after clone or
            // manual damage. Creation identity must come from the filesystem's
            // create-new result, not from the file length, otherwise the resolver
            // would silently rewrite corruption and call the DPO creator.
            stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                paths.LockPath,
                FileMode.CreateNew,
                FileAccess.ReadWrite,
                FileShare.None,
                FileOptions.WriteThrough,
                FileBufferSize);
            newlyCreated = true;
        }
        catch (IOException) when (File.Exists(paths.LockPath))
        {
            stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                paths.LockPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None,
                FileOptions.WriteThrough,
                FileBufferSize);
            newlyCreated = false;
        }

        try
        {
            if (!CreationLeaseLockHandleIsSafe(
                    stream.SafeFileHandle,
                    paths.LockPath)
                || !EnsureCreationLeaseLockContentForOpenedStream(
                    stream,
                    newlyCreated)
                || !CreationLeaseLockContentIsExpected(stream))
            {
                stream.Dispose();
                return Unavailable<FileStream>();
            }

            return OperationResult<FileStream>.Ok(stream);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    [SupportedOSPlatform("linux")]
    private static OperationResult<FileStream>
        OpenPreparedLinuxCreationLeaseLockFile(CreationLeasePaths paths)
    {
        try
        {
            if (CreationLeasePathIsUnsafe(paths))
                return Unavailable<FileStream>();

            using var directoryHandle = OpenLinuxDirectoryNoFollow(
                paths.LockDirectory,
                out _);
            if (directoryHandle is null
                || !LinuxDirectoryHandleMatchesPath(
                    directoryHandle,
                    paths.LockDirectory))
            {
                return Unavailable<FileStream>();
            }

            var lockFileName = Path.GetFileName(paths.LockPath);
            if (string.IsNullOrWhiteSpace(lockFileName)
                || lockFileName.IndexOfAny(
                    [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar])
                >= 0)
            {
                return Unavailable<FileStream>();
            }

            var fileHandle = OpenLinuxFileAtNoFollow(
                directoryHandle,
                lockFileName,
                createNew: false,
                out var errno);
            var newlyCreated = false;
            if (fileHandle is null && errno == LinuxErrnoNoEntry)
            {
                fileHandle = OpenLinuxFileAtNoFollow(
                    directoryHandle,
                    lockFileName,
                    createNew: true,
                    out errno);
                newlyCreated = fileHandle is not null;
                if (fileHandle is null && errno == LinuxErrnoExists)
                {
                    fileHandle = OpenLinuxFileAtNoFollow(
                        directoryHandle,
                        lockFileName,
                        createNew: false,
                        out errno);
                    newlyCreated = false;
                }
            }

            if (fileHandle is null)
                return Unavailable<FileStream>();

            var stream = new FileStream(
                fileHandle,
                FileAccess.ReadWrite,
                FileBufferSize,
                isAsync: false);
            try
            {
                if (!CreationLeaseLockHandleIsSafe(
                        stream.SafeFileHandle,
                        paths.LockPath)
                    || !EnsureCreationLeaseLockContentForOpenedStream(
                        stream,
                        newlyCreated)
                    || !EnsureLinuxLockHandleOwnerOnly(
                        stream.SafeFileHandle,
                        paths.LockPath)
                    || !CreationLeaseLockContentIsExpected(stream))
                {
                    stream.Dispose();
                    return Unavailable<FileStream>();
                }

                return OperationResult<FileStream>.Ok(stream);
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<FileStream>();
        }
    }

    [SupportedOSPlatform("linux")]
    private static bool EnsureLinuxLockHandleOwnerOnly(
        SafeFileHandle handle,
        string requestedPath)
    {
        if (!CreationLeaseLockHandleIsSafe(handle, requestedPath))
            return false;

        var mode = File.GetUnixFileMode(handle);
        if (!UnixModeCanBeNarrowedToOwner(mode, OwnerFileMode))
            return false;

        if (!UnixModeIsOwnerOnly(mode, OwnerFileMode))
        {
            // Git хранит постоянный однобайтовый lock как 0644. Сужаем его
            // до 0600 только после no-follow open и проверки, что это именно
            // ожидаемый обычный файл с ожидаемым содержимым.
            File.SetUnixFileMode(handle, OwnerFileMode);
        }

        return CreationLeaseLockHandleIsSafe(handle, requestedPath)
               && UnixModeIsOwnerOnly(File.GetUnixFileMode(handle), OwnerFileMode);
    }

    private static bool EnsureCreationLeaseLockContentForOpenedStream(
        FileStream stream,
        bool newlyCreated)
    {
        if (newlyCreated)
        {
            if (stream.Length != 0)
                return false;
            stream.SetLength(CreationLeaseLockLength);
            stream.Position = 0;
            stream.WriteByte(CreationLeaseLockByte);
            stream.Flush(flushToDisk: true);
            return CreationLeaseLockContentIsExpected(stream);
        }

        return CreationLeaseLockContentIsExpected(stream);
    }

    private static bool CreationLeaseLockContentIsExpected(FileStream stream)
    {
        try
        {
            var position = stream.CanSeek ? stream.Position : 0;
            try
            {
                if (stream.Length != CreationLeaseLockLength)
                    return false;
                stream.Position = 0;
                return stream.ReadByte() == CreationLeaseLockByte
                       && stream.ReadByte() < 0
                       && stream.Length == CreationLeaseLockLength;
            }
            finally
            {
                if (stream.CanSeek)
                    stream.Position = Math.Min(position, stream.Length);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool CreationLeaseLockHandleIsSafe(
        SafeFileHandle handle,
        string requestedPath)
    {
        try
        {
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    handle,
                    requestedPath))
            {
                return false;
            }

            var attributes = File.GetAttributes(handle);
            return (attributes & FileAttributes.Directory) == 0
                   && (attributes & FileAttributes.ReparsePoint) == 0;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool LinuxDirectoryHandleMatchesPath(
        SafeFileHandle handle,
        string requestedPath)
    {
        try
        {
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    handle,
                    requestedPath))
            {
                return false;
            }

            var attributes = File.GetAttributes(handle);
            return (attributes & FileAttributes.Directory) != 0
                   && (attributes & FileAttributes.ReparsePoint) == 0;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    private static bool UnixModeCanBeNarrowedToOwner(
        UnixFileMode mode,
        UnixFileMode ownerMode)
        => (mode & ownerMode) == ownerMode
           && (mode & SpecialUnixModeBits) == 0;

    private static bool UnixModeIsOwnerOnly(
        UnixFileMode mode,
        UnixFileMode ownerMode)
        => UnixModeCanBeNarrowedToOwner(mode, ownerMode)
           && (mode & GroupOrOtherAccessMask) == 0;

    [SupportedOSPlatform("linux")]
    private static SafeFileHandle? OpenLinuxDirectoryNoFollow(
        string path,
        out int errno)
    {
        var descriptor = LinuxOpen(
            path,
            LinuxOpenReadOnly | LinuxOpenDirectory |
            LinuxOpenNoFollow | LinuxOpenCloseOnExec);
        return WrapLinuxDescriptor(descriptor, out errno);
    }

    [SupportedOSPlatform("linux")]
    private static SafeFileHandle? OpenLinuxDirectoryAtNoFollow(
        SafeFileHandle parent,
        string segment,
        out int errno)
    {
        var descriptor = LinuxOpenAt(
            parent,
            segment,
            LinuxOpenReadOnly | LinuxOpenDirectory |
            LinuxOpenNoFollow | LinuxOpenCloseOnExec,
            0);
        return WrapLinuxDescriptor(descriptor, out errno);
    }

    [SupportedOSPlatform("linux")]
    private static SafeFileHandle? OpenLinuxFileAtNoFollow(
        SafeFileHandle parent,
        string fileName,
        bool createNew,
        out int errno)
    {
        var flags = LinuxOpenReadWrite | LinuxOpenNoFollow |
                    LinuxOpenCloseOnExec;
        if (createNew)
            flags |= LinuxOpenCreate | LinuxOpenExclusive;

        var descriptor = LinuxOpenAt(
            parent,
            fileName,
            flags,
            LinuxOwnerFileModeBits);
        return WrapLinuxDescriptor(descriptor, out errno);
    }

    private static SafeFileHandle? WrapLinuxDescriptor(
        int descriptor,
        out int errno)
    {
        if (descriptor < 0)
        {
            errno = Marshal.GetLastPInvokeError();
            return null;
        }

        errno = 0;
        var handle = new SafeFileHandle(new IntPtr(descriptor), ownsHandle: true);
        if (!handle.IsInvalid)
            return handle;

        handle.Dispose();
        return null;
    }

    private static SafeFileHandle? DisposeAndNull(SafeFileHandle handle)
    {
        handle.Dispose();
        return null;
    }

    private static void CleanupUnsealedDatasetRoot(
        RepositoryOwnedDpoDatasetAddress address)
    {
        try
        {
            if (RepositoryOwnedDpoDatasetManifestSealer
                .OpenSealedForManifestIntegrity(address)
                .IsSuccess)
            {
                return;
            }

            if (!Directory.Exists(address.DatasetRoot)
                || !IsValidAddress(address)
                || !IsStrictDescendant(address.DatasetRoot, address.DpoArtifactRoot)
                || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(
                    address.DatasetRoot))
            {
                return;
            }

            // WHY: failed teacher generation must not leave a half-dataset
            // that makes the next run believe reusable DPO exists. Only the
            // exact clean-miss dataset root is removed, never the DPO root.
            Directory.Delete(address.DatasetRoot, recursive: true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // The resolver returns the original failure. The next resolver call
            // treats any remaining partial dataset as fail-closed.
        }
    }

    private static void ReleaseCreationProcessGateReference(
        string canonicalLockPath,
        CreationProcessGateEntry entry)
    {
        SemaphoreSlim? semaphoreToDispose = null;
        lock (CreationProcessGateRegistrySync)
        {
            entry.ReferenceCount--;
            if (entry.ReferenceCount != 0)
                return;
            if (!CreationProcessGateRegistry.TryGetValue(
                    canonicalLockPath,
                    out var registered)
                || !ReferenceEquals(registered, entry))
            {
                throw new InvalidOperationException(
                    "DPO dataset creation gate registry identity was lost.");
            }

            CreationProcessGateRegistry.Remove(canonicalLockPath);
            semaphoreToDispose = entry.Semaphore;
        }

        semaphoreToDispose.Dispose();
    }

    private static void ReleaseOwnedCreationProcessGate(
        string canonicalLockPath,
        CreationProcessGateEntry entry)
    {
        try
        {
            entry.Semaphore.Release();
        }
        finally
        {
            ReleaseCreationProcessGateReference(canonicalLockPath, entry);
        }
    }

    private static bool ExpectedDirectorySegmentIsFile(
        string repositoryRoot,
        string expectedDirectory)
    {
        var canonicalRepositoryRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(repositoryRoot));
        var canonicalExpectedDirectory = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(expectedDirectory));
        if (!IsStrictDescendant(canonicalExpectedDirectory, canonicalRepositoryRoot))
            return true;

        var relative = Path.GetRelativePath(
            canonicalRepositoryRoot,
            canonicalExpectedDirectory);
        var current = canonicalRepositoryRoot;
        foreach (var segment in relative.Split(
                     new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                     StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            if (File.Exists(current))
                return true;
        }

        return false;
    }

    private static bool IsStrictDescendant(string candidate, string root)
    {
        var normalizedCandidate = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(candidate));
        var normalizedRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(root));
        if (PathsEqual(normalizedCandidate, normalizedRoot))
            return false;
        var prefix = normalizedRoot + Path.DirectorySeparatorChar;
        return normalizedCandidate.StartsWith(
            prefix,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
    }

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static bool IsValidAddress(
        RepositoryOwnedDpoDatasetAddress? address)
        => address is not null
           && RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
               address);

    private static OperationResult<T> Missing<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset is missing.",
            MissingErrorCode);

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset resolution is invalid.",
            InvalidErrorCode);

    private static OperationResult<T> Busy<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset creation reservation is busy.",
            BusyErrorCode);

    private static OperationResult<T> Unavailable<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset creation reservation is unavailable.",
            UnavailableErrorCode);

    private static OperationResult<T> Cancelled<T>()
        => OperationResult<T>.Fail(
            "Repository-owned DPO dataset creation reservation was cancelled.",
            CancelledErrorCode);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int LinuxOpen(string path, int flags);

    [DllImport("libc", EntryPoint = "openat", SetLastError = true)]
    private static extern int LinuxOpenAt(
        SafeFileHandle directory,
        string path,
        int flags,
        uint mode);

    [DllImport("libc", EntryPoint = "mkdirat", SetLastError = true)]
    private static extern int LinuxMkdirAt(
        SafeFileHandle directory,
        string path,
        uint mode);

    private sealed record CreationLeasePaths(
        string TrainingArtifactsRoot,
        string LeaseRoot,
        string DomainLeaseDirectory,
        string LockDirectory,
        string LockPath);

    private sealed class CreationProcessGateEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int ReferenceCount { get; set; }
    }

    private sealed class CreationProcessLease(
        string canonicalLockPath,
        CreationProcessGateEntry entry) : IDisposable
    {
        private CreationProcessGateEntry? _entry = entry;

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref _entry, null);
            if (owned is not null)
                ReleaseOwnedCreationProcessGate(canonicalLockPath, owned);
        }
    }

    private sealed class CreationFileLease(FileStream stream) : IDisposable
    {
        private FileStream? _stream = stream;

        public bool StillOwns(string requestedPath)
        {
            var owned = Volatile.Read(ref _stream);
            return owned is not null
                && owned.Length == CreationLeaseLockLength
                && CreationLeaseLockHandleIsSafe(
                    owned.SafeFileHandle,
                    requestedPath)
                && CreationLeaseLockContentIsExpected(owned);
        }

        public void Dispose()
        {
            var owned = Interlocked.Exchange(ref _stream, null);
            if (owned is null)
                return;

            try
            {
                if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
                {
                    owned.Unlock(
                        CreationLeaseLockOffset,
                        CreationLeaseLockLength);
                }
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                // Closing the handle below releases the lock even when a
                // platform edge case rejects explicit unlock.
            }
            finally
            {
                owned.Dispose();
                // WHY: the one-byte file deliberately remains after unlock.
                // Deleting it can let a Linux waiter keep the old inode while
                // a newcomer locks a replacement inode, splitting ownership.
            }
        }
    }

    private sealed class CreationLease(
        CreationProcessLease processLease,
        CreationFileLease fileLease,
        CreationLeasePaths paths) : IDisposable
    {
        private CreationProcessLease? _processLease = processLease;
        private CreationFileLease? _fileLease = fileLease;

        public bool StillOwnsRequestedLease()
            => Volatile.Read(ref _fileLease) is { } lease
               && lease.StillOwns(paths.LockPath)
               && !CreationLeasePathIsUnsafe(paths);

        public void Dispose()
        {
            var file = Interlocked.Exchange(ref _fileLease, null);
            file?.Dispose();
            var process = Interlocked.Exchange(ref _processLease, null);
            process?.Dispose();
        }
    }
}
