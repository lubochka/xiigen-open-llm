// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

internal enum ServerOwnedContinuousLearningFileAccessPlatform
{
    Windows,
    Posix,
    Unsupported,
}

internal enum ContinuousLearningRetainedObjectRole
{
    VolumeOrShareAnchor,
    LexicalAncestor,
    BaseCheckpoint,
    Tokenizer,
    AdapterRoot,
    CacheRoot,
    RegistryRoot,
    DpoPairRoot,
    SealedRepositoryDpoSourceRoot, // Retain a sealed repository input without granting mutable pair-store ownership.
}

internal enum ContinuousLearningRetainedObjectKind
{
    RegularFile,
    Directory,
}

internal enum ContinuousLearningRetainedRootRole
{
    Adapter,
    Cache,
    Registry,
    DpoPairs,
    SealedDpoSource, // Resolve only the read namespace admitted for an immutable repository dataset.
}

internal enum ContinuousLearningDpoPairRootAccess // Keep the legacy store distinct from a repository-owned sealed input.
{ // A source role never aliases the mutation role used by the legacy DPO pair store.
    MutablePairStore, // Preserve existing callers' mutation-ownership admission by default.
    SealedRepositorySource, // Admit only reading and traversal of an already sealed repository dataset.
} // The retained owner rejects unknown role values before opening any path.

/// <summary>
/// The two immutable model artifacts which may be read through a duplicated
/// retained handle during inactive startup materialization.
/// </summary>
internal enum ContinuousLearningRetainedArtifactRole
{
    BaseCheckpoint,
    Tokenizer,
}

internal sealed record ServerOwnedContinuousLearningFileAccessOptions
{
    internal ServerOwnedContinuousLearningFileAccessPlatform? PlatformForTesting
    {
        get;
        init;
    }

    internal Action<string, string>? AfterParentRetainedBeforeChildOpenForTesting
    {
        get;
        init;
    }
}

internal sealed class ContinuousLearningPathIdentityException : IOException
{
    internal const string StableErrorCode =
        "CONTINUOUS_LEARNING_PATH_IDENTITY_VERIFICATION_FAILED";

    internal ContinuousLearningPathIdentityException()
        : base(StableErrorCode)
    {
    }
}

internal sealed class ContinuousLearningPosixExactMutationUnsupportedException
    : IOException
{
    internal const string StableErrorCode =
        "CONTINUOUS_LEARNING_POSIX_EXACT_NAMESPACE_MUTATION_UNSUPPORTED";

    internal ContinuousLearningPosixExactMutationUnsupportedException()
        : base(StableErrorCode)
    {
    }
}

internal sealed class ContinuousLearningMutationAdmissionDomainAlreadyOwnedException
    : IOException
{
    internal const string StableErrorCode =
        "CONTINUOUS_LEARNING_MUTATION_ADMISSION_DOMAIN_ALREADY_OWNED";

    internal ContinuousLearningMutationAdmissionDomainAlreadyOwnedException()
        : base(StableErrorCode)
    {
    }
}

/// <summary>
/// R1.2 host-owned retained-object lease for the six admitted paths. Every path
/// is opened relative to a retained parent with no-follow semantics and without
/// delete sharing. Separate one-shot capabilities may perform fixed,
/// handle-relative CacheRoot, AdapterRoot, or RegistryRoot I/O; this lease
/// defines no cache/registry schema, pipeline, model, or training behavior.
/// A live lease intentionally keeps the mutation domain exclusive until Dispose;
/// child capabilities may end earlier, but they do not authorize a second parent
/// owner while this broader lease is still alive.
/// </summary>
internal sealed partial class ServerOwnedContinuousLearningFileAccess : IDisposable
{
    private const uint GenericWrite = 0x40000000;
    private const uint DeleteAccess = 0x00010000;
    private const uint Synchronize = 0x00100000;
    private const uint FileReadData = 0x00000001;
    private const uint FileListDirectory = 0x00000001;
    private const uint FileAddFile = 0x00000002;
    private const uint FileTraverse = 0x00000020;
    private const uint FileDeleteChild = 0x00000040;
    private const uint FileReadAttributes = 0x00000080;
    private const uint FileAttributeNormal = 0x00000080;
    private const uint FileAttributeDirectory = 0x00000010;
    private const uint FileAttributeReparsePoint = 0x00000400;
    private const uint FileFlagBackupSemantics = 0x02000000;
    private const uint FileFlagOpenReparsePoint = 0x00200000;
    private const uint OpenExisting = 3;
    private const uint FileOpen = 1;
    private const uint FileDirectoryFile = 0x00000001;
    private const uint FileSynchronousIoNonAlert = 0x00000020;
    private const uint FileNonDirectoryFile = 0x00000040;
    private const uint FileOpenReparsePoint = 0x00200000;
    private const uint ObjectCaseInsensitive = 0x00000040;
    private const uint ObjectDontReparse = 0x00001000;
    private const int StatusSharingViolation = unchecked((int)0xC0000043);

    private readonly object _gate = new();
    private readonly RetainedObjectEntry[] _entries;
    private Action? _onRetainedObjectTrustLost;
    private bool _disposed;
    private bool _retainedObjectTrustLost;

    private ServerOwnedContinuousLearningFileAccess(
        RetainedObjectEntry[] entries,
        Action onRetainedObjectTrustLost)
    {
        _entries = entries;
        _onRetainedObjectTrustLost = onRetainedObjectTrustLost;
    }

    internal static ServerOwnedContinuousLearningFileAccess Acquire(
        string baseCheckpoint,
        string tokenizer,
        string adapterRoot,
        string cacheRoot,
        string registryRoot,
        string dpoPairRoot,
        ServerOwnedContinuousLearningFileAccessOptions? options,
        Action onRetainedObjectTrustLost, // Keep the existing lifetime callback for either admitted DPO root role.
        ContinuousLearningDpoPairRootAccess dpoPairRootAccess = ContinuousLearningDpoPairRootAccess.MutablePairStore) // Existing callers retain the mutable pair-store contract.
    {
        ArgumentNullException.ThrowIfNull(baseCheckpoint);
        ArgumentNullException.ThrowIfNull(tokenizer);
        ArgumentNullException.ThrowIfNull(adapterRoot);
        ArgumentNullException.ThrowIfNull(cacheRoot);
        ArgumentNullException.ThrowIfNull(registryRoot);
        ArgumentNullException.ThrowIfNull(dpoPairRoot);
        ArgumentNullException.ThrowIfNull(onRetainedObjectTrustLost);
        var dpoObjectRole = dpoPairRootAccess switch // Bind one explicit storage role before either platform opens a handle.
        { // Unknown roles cannot silently fall back to a weaker admission mode.
            ContinuousLearningDpoPairRootAccess.MutablePairStore => ContinuousLearningRetainedObjectRole.DpoPairRoot, // Keep the legacy mutation lease.
            ContinuousLearningDpoPairRootAccess.SealedRepositorySource => ContinuousLearningRetainedObjectRole.SealedRepositoryDpoSourceRoot, // Sealed input has a separate read namespace.
            _ => throw new ContinuousLearningPathIdentityException(), // Fail closed for invalid internal role values.
        }; // The role remains fixed for the lifetime of this retained owner.
        options ??= new ServerOwnedContinuousLearningFileAccessOptions();
        var platform = options.PlatformForTesting ?? DetectPlatform();

        try
        {
            return platform switch
            {
                ServerOwnedContinuousLearningFileAccessPlatform.Windows =>
                    AcquireWindows(
                        baseCheckpoint,
                        tokenizer,
                        adapterRoot,
                        cacheRoot,
                        registryRoot,
                        dpoPairRoot,
                        dpoObjectRole, // Carry the admitted role to the Windows terminal open.
                        options,
                        onRetainedObjectTrustLost),
                ServerOwnedContinuousLearningFileAccessPlatform.Posix =>
                    AcquirePosix(
                        baseCheckpoint,
                        tokenizer,
                        adapterRoot,
                        cacheRoot,
                        registryRoot,
                        dpoPairRoot,
                        dpoObjectRole, // Carry the same role to POSIX without changing legacy mutation leases.
                        options,
                        onRetainedObjectTrustLost),
                _ => throw new ContinuousLearningPathIdentityException(),
            };
        }
        catch (ContinuousLearningMutationAdmissionDomainAlreadyOwnedException)
        {
            throw;
        }
        catch (ContinuousLearningPathIdentityException)
        {
            throw;
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            throw new ContinuousLearningPathIdentityException();
        }
    }

    public void Dispose()
    {
        Action? callback;
        lock (_gate)
        {
            if (_disposed)
                return;
            _disposed = true;
            callback = LoseRetainedObjectTrustLocked();
            for (var index = _entries.Length - 1; index >= 0; index--)
                _entries[index].Handle.Dispose();
        }
        InvokeRetainedObjectTrustLost(callback);
    }

    internal void EnsureRetained()
    {
        Action? callback = null;
        var failed = false;
        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            var valid = _entries.Length != 0;
            foreach (var entry in _entries)
                valid &= TryValidateRetainedEntry(entry);
            if (!valid)
            {
                callback = LoseRetainedObjectTrustLocked();
                failed = true;
            }
        }
        InvokeRetainedObjectTrustLost(callback);
        if (failed)
            throw new ContinuousLearningPathIdentityException();
    }

    /// <summary>
    /// Opens a caller-owned stream for one of the two immutable artifacts by
    /// reopening its already-retained Windows file object. ReOpenFile creates
    /// an independent file object (and therefore an independent file pointer),
    /// unlike DuplicateHandle; no configured path is ever consulted.
    /// </summary>
    internal FileStream OpenRetainedArtifactForRead(
        ContinuousLearningRetainedArtifactRole artifactRole)
    {
        Action? callback = null;
        SafeFileHandle? reopened = null;
        try
        {
            lock (_gate)
            {
                ThrowIfUnavailableLocked();
                var entry = FindRetainedArtifact(artifactRole);
                if (!TryValidateRetainedEntry(entry))
                {
                    callback = LoseRetainedObjectTrustLocked();
                    throw new ContinuousLearningPathIdentityException();
                }

                reopened = entry.Identity.Platform switch
                {
                    ServerOwnedContinuousLearningFileAccessPlatform.Windows =>
                        ReOpenFileWindows(
                            entry.Handle,
                            FileReadData | FileReadAttributes | Synchronize,
                            (uint)FileShare.Read,
                            FileFlagOpenReparsePoint),
                    ServerOwnedContinuousLearningFileAccessPlatform.Posix =>
                        OpenRetainedArtifactForReadPosix(entry),
                    _ => throw new ContinuousLearningPathIdentityException(),
                };
                if (reopened.IsInvalid)
                {
                    reopened.Dispose();
                    reopened = null;
                    callback = LoseRetainedObjectTrustLocked();
                    throw new ContinuousLearningPathIdentityException();
                }
            }

            return new FileStream(
                reopened!,
                FileAccess.Read,
                bufferSize: 64 * 1024,
                isAsync: false);
        }
        catch (ContinuousLearningPathIdentityException)
        {
            reopened?.Dispose();
            throw;
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            reopened?.Dispose();
            lock (_gate)
                callback ??= LoseRetainedObjectTrustLocked();
            throw new ContinuousLearningPathIdentityException();
        }
        finally
        {
            InvokeRetainedObjectTrustLost(callback);
        }
    }

    internal bool ValidateExistingRegularLeaf(
        ContinuousLearningRetainedRootRole rootRole,
        string leafName,
        long maximumBytes,
        Action? afterLeafOpenBeforeRootRevalidationForTesting = null)
    {
        if (!IsValidRootRole(rootRole) ||
            maximumBytes <= 0 ||
            !IsValidSingleLeafName(leafName))
        {
            return false;
        }

        Action? callback = null;
        var trustFailed = false;
        var leafAccepted = false;
        lock (_gate)
        {
            ThrowIfUnavailableLocked();
            var root = FindRetainedRoot(rootRole);
            if (!TryValidateRetainedEntry(root))
            {
                callback = LoseRetainedObjectTrustLocked();
                trustFailed = true;
            }
            else
            {
                if (root.Identity.Platform ==
                    ServerOwnedContinuousLearningFileAccessPlatform.Posix)
                {
                    using var leafHandle = TryOpenExistingRegularLeafPosix(
                        root.Handle,
                        leafName);
                    if (leafHandle is not null)
                    {
                        leafAccepted = TryValidateRegularLeafPosix(
                            leafHandle,
                            maximumBytes);
                        afterLeafOpenBeforeRootRevalidationForTesting?.Invoke();
                    }
                    if (!TryValidateRetainedEntry(root))
                    {
                        callback = LoseRetainedObjectTrustLocked();
                        trustFailed = true;
                        leafAccepted = false;
                    }
                }
                else
                {
                    var leafHandle = NtOpenExistingRegularLeafRelative(
                        root.Handle,
                        leafName,
                        out var status);
                    try
                    {
                        if (status >= 0 && !leafHandle.IsInvalid)
                        {
                            leafAccepted = TryValidateRegularLeaf(
                                leafHandle,
                                maximumBytes);
                            afterLeafOpenBeforeRootRevalidationForTesting
                                ?.Invoke();
                        }

                        if (!TryValidateRetainedEntry(root))
                        {
                            callback = LoseRetainedObjectTrustLocked();
                            trustFailed = true;
                            leafAccepted = false;
                        }
                    }
                    finally
                    {
                        leafHandle.Dispose();
                    }
                }
            }
        }

        InvokeRetainedObjectTrustLost(callback);
        if (trustFailed)
            throw new ContinuousLearningPathIdentityException();
        return leafAccepted;
    }

    private void ThrowIfUnavailableLocked()
    {
        if (_disposed || _retainedObjectTrustLost)
            throw new ContinuousLearningPathIdentityException();
    }

    private Action? LoseRetainedObjectTrustLocked()
    {
        if (_retainedObjectTrustLost)
            return null;
        _retainedObjectTrustLost = true;
        _compilerCacheRootCapability?.RevokeLocked();
        _adapterRootCapability?.RevokeLocked();
        _registryRootCapability?.RevokeLocked();
        var callback = _onRetainedObjectTrustLost;
        _onRetainedObjectTrustLost = null;
        return callback;
    }

    private static void InvokeRetainedObjectTrustLost(Action? callback)
    {
        if (callback is null)
            return;
        try
        {
            callback();
        }
        catch (Exception exception)
            when (AgentInfrastructure.Logical.SnapshotExceptionGraphClassifier
                .IsNonFatal(exception))
        {
            // Trust is already sticky-lost; callbacks cannot restore it.
        }
    }

    private RetainedObjectEntry FindRetainedRoot(
        ContinuousLearningRetainedRootRole rootRole)
    {
        var objectRole = rootRole switch
        {
            ContinuousLearningRetainedRootRole.Adapter =>
                ContinuousLearningRetainedObjectRole.AdapterRoot,
            ContinuousLearningRetainedRootRole.Cache =>
                ContinuousLearningRetainedObjectRole.CacheRoot,
            ContinuousLearningRetainedRootRole.Registry =>
                ContinuousLearningRetainedObjectRole.RegistryRoot,
            ContinuousLearningRetainedRootRole.DpoPairs =>
                ContinuousLearningRetainedObjectRole.DpoPairRoot,
            ContinuousLearningRetainedRootRole.SealedDpoSource => ContinuousLearningRetainedObjectRole.SealedRepositoryDpoSourceRoot, // A sealed reader never resolves the mutable DPO store role.
            _ => throw new ContinuousLearningPathIdentityException(),
        };
        var entry = _entries.SingleOrDefault(candidate => candidate.Role == objectRole);
        return entry ?? throw new ContinuousLearningPathIdentityException();
    }

    private RetainedObjectEntry FindRetainedArtifact(
        ContinuousLearningRetainedArtifactRole artifactRole)
    {
        var objectRole = artifactRole switch
        {
            ContinuousLearningRetainedArtifactRole.BaseCheckpoint =>
                ContinuousLearningRetainedObjectRole.BaseCheckpoint,
            ContinuousLearningRetainedArtifactRole.Tokenizer =>
                ContinuousLearningRetainedObjectRole.Tokenizer,
            _ => throw new ContinuousLearningPathIdentityException(),
        };
        var entry = _entries.SingleOrDefault(candidate => candidate.Role == objectRole);
        return entry is { Kind: ContinuousLearningRetainedObjectKind.RegularFile }
            ? entry
            : throw new ContinuousLearningPathIdentityException();
    }

    private static bool IsValidRootRole(ContinuousLearningRetainedRootRole role)
        => role is ContinuousLearningRetainedRootRole.Adapter
            or ContinuousLearningRetainedRootRole.Cache
            or ContinuousLearningRetainedRootRole.Registry
            or ContinuousLearningRetainedRootRole.DpoPairs // Keep legacy pair-store leaf validation available.
            or ContinuousLearningRetainedRootRole.SealedDpoSource; // Allow the existing no-follow read validator to inspect sealed input leaves.

    private static bool IsValidSingleLeafName(string? leafName)
    {
        if (string.IsNullOrWhiteSpace(leafName) ||
            leafName.Length > 255 ||
            leafName is "." or ".." ||
            leafName[0] is ' ' or '\\' or '/' ||
            leafName[^1] is ' ' or '.' ||
            leafName.Any(character =>
                character == '\0' ||
                char.IsControl(character) ||
                character is '\\' or '/' or ':' or '*' or '?' or '<' or '>' or '"' or '|') ||
            Path.IsPathRooted(leafName) ||
            leafName.StartsWith(@"\\?\", StringComparison.Ordinal) ||
            leafName.StartsWith(@"\\.\", StringComparison.Ordinal) ||
            leafName.StartsWith(@"\??\", StringComparison.Ordinal))
        {
            return false;
        }

        var basename = leafName.Split('.', 2)[0];
        if (basename.Equals("CON", StringComparison.OrdinalIgnoreCase) ||
            basename.Equals("PRN", StringComparison.OrdinalIgnoreCase) ||
            basename.Equals("AUX", StringComparison.OrdinalIgnoreCase) ||
            basename.Equals("NUL", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return !(basename.Length == 4 &&
                 (basename.StartsWith("COM", StringComparison.OrdinalIgnoreCase) ||
                  basename.StartsWith("LPT", StringComparison.OrdinalIgnoreCase)) &&
                 basename[3] is >= '1' and <= '9');
    }

    private static ServerOwnedContinuousLearningFileAccess AcquireWindows(
        string baseCheckpoint,
        string tokenizer,
        string adapterRoot,
        string cacheRoot,
        string registryRoot,
        string dpoPairRoot,
        ContinuousLearningRetainedObjectRole dpoObjectRole, // Select read-only sealed input or the unchanged mutable pair-store terminal.
        ServerOwnedContinuousLearningFileAccessOptions options,
        Action onRetainedObjectTrustLost)
    {
        var entries = new List<RetainedObjectEntry>();
        var retained = new Dictionary<string, AcquiredPath>(
            StringComparer.OrdinalIgnoreCase);
        var terminalIdentities = new List<RetainedFileIdentity>(6);
        try
        {
            terminalIdentities.Add(AcquireTerminal(
                baseCheckpoint,
                ContinuousLearningRetainedObjectRole.BaseCheckpoint,
                requireDirectory: false,
                maximumFileBytes: ContinuousLearningModelServerConfigurationLoader
                    .MaximumBaseCheckpointBytes,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminal(
                tokenizer,
                ContinuousLearningRetainedObjectRole.Tokenizer,
                requireDirectory: false,
                maximumFileBytes: ContinuousLearningModelServerConfigurationLoader
                    .MaximumTokenizerBytes,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminal(
                adapterRoot,
                ContinuousLearningRetainedObjectRole.AdapterRoot,
                requireDirectory: true,
                maximumFileBytes: null,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminal(
                cacheRoot,
                ContinuousLearningRetainedObjectRole.CacheRoot,
                requireDirectory: true,
                maximumFileBytes: null,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminal(
                registryRoot,
                ContinuousLearningRetainedObjectRole.RegistryRoot,
                requireDirectory: true,
                maximumFileBytes: null,
                entries,
                retained,
                options));
            terminalIdentities.Add(AcquireTerminal(
                dpoPairRoot,
                dpoObjectRole, // The distinct sealed source role requests no mutation ownership or write access.
                requireDirectory: true,
                maximumFileBytes: null,
                entries,
                retained,
                options));

            if (terminalIdentities.Distinct().Count() != terminalIdentities.Count)
                throw new ContinuousLearningPathIdentityException();

            return new ServerOwnedContinuousLearningFileAccess(
                entries.ToArray(),
                onRetainedObjectTrustLost);
        }
        catch
        {
            for (var index = entries.Count - 1; index >= 0; index--)
                entries[index].Handle.Dispose();
            throw;
        }
    }

    private static RetainedFileIdentity AcquireTerminal(
        string terminalPath,
        ContinuousLearningRetainedObjectRole terminalRole,
        bool requireDirectory,
        long? maximumFileBytes,
        List<RetainedObjectEntry> entries,
        Dictionary<string, AcquiredPath> retained,
        ServerOwnedContinuousLearningFileAccessOptions options)
    {
        var normalizedTerminal = NormalizeExpectedPath(terminalPath);
        var pathRoot = Path.GetPathRoot(normalizedTerminal)
            ?? throw new ContinuousLearningPathIdentityException();
        var normalizedRoot = NormalizeExpectedPath(pathRoot);
        var segments = normalizedTerminal[pathRoot.Length..]
            .Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            // A volume/share anchor can also be a configured terminal. Reusing
            // an ancestor-shared anchor would weaken the terminal lease, so
            // this rare shape fails closed instead.
            throw new ContinuousLearningPathIdentityException();
        }

        if (!retained.TryGetValue(normalizedRoot, out var parent))
        {
            var rootHandle = OpenWindowsRoot(pathRoot);
            try
            {
                var rootIdentity = ValidateWindowsHandle(
                    rootHandle,
                    normalizedRoot,
                    requireDirectory: true);
                var rootEntry = new RetainedObjectEntry(
                    ContinuousLearningRetainedObjectRole.VolumeOrShareAnchor,
                    rootHandle,
                    rootIdentity,
                    ContinuousLearningRetainedObjectKind.Directory,
                    MinimumBytes: 0,
                    MaximumBytes: 0,
                    RequiredLinkCount: 0,
                    ParentHandleForIndependentOpen: null,
                    LeafNameUtf8ForIndependentOpen: null);
                parent = new AcquiredPath(
                    normalizedRoot,
                    rootEntry);
                retained.Add(normalizedRoot, parent);
                entries.Add(rootEntry);
            }
            catch
            {
                rootHandle.Dispose();
                throw;
            }
        }

        for (var index = 0; index < segments.Length; index++)
        {
            var segment = segments[index];
            EnsureSingleName(segment);
            var isTerminal = index == segments.Length - 1;
            var childRequiresDirectory = !isTerminal || requireDirectory;
            var childPath = NormalizeExpectedPath(
                Path.Combine(parent.ExpectedPath, segment));
            if (retained.TryGetValue(childPath, out var existing))
            {
                if ((existing.Entry.Kind == ContinuousLearningRetainedObjectKind.Directory)
                    != childRequiresDirectory)
                    throw new ContinuousLearningPathIdentityException();
                parent = existing;
                continue;
            }

            _ = ValidateWindowsHandle(
                parent.Entry.Handle,
                parent.ExpectedPath,
                requireDirectory: true);
            options.AfterParentRetainedBeforeChildOpenForTesting?.Invoke(
                parent.ExpectedPath,
                childPath);
            _ = ValidateWindowsHandle(
                parent.Entry.Handle,
                parent.ExpectedPath,
                requireDirectory: true);

            var childHandle = NtOpenWindowsRelative(
                parent.Entry.Handle,
                segment,
                childRequiresDirectory,
                isTerminal,
                isTerminal && IsMutableMutationRoot(terminalRole),
                isTerminal &&
                (terminalRole == ContinuousLearningRetainedObjectRole.CacheRoot ||
                 terminalRole == ContinuousLearningRetainedObjectRole.AdapterRoot ||
                 terminalRole == ContinuousLearningRetainedObjectRole.RegistryRoot),
                out var status);
            if (status < 0 || childHandle.IsInvalid)
            {
                childHandle.Dispose();
                if (status == StatusSharingViolation &&
                    isTerminal &&
                    IsMutableMutationRoot(terminalRole))
                {
                    throw new
                        ContinuousLearningMutationAdmissionDomainAlreadyOwnedException();
                }
                throw new ContinuousLearningPathIdentityException();
            }

            try
            {
                var childIdentity = ValidateWindowsHandle(
                    childHandle,
                    childPath,
                    childRequiresDirectory);
                var childEntry = new RetainedObjectEntry(
                    isTerminal
                        ? terminalRole
                        : ContinuousLearningRetainedObjectRole.LexicalAncestor,
                    childHandle,
                    childIdentity,
                    childRequiresDirectory
                        ? ContinuousLearningRetainedObjectKind.Directory
                        : ContinuousLearningRetainedObjectKind.RegularFile,
                    MinimumBytes: childRequiresDirectory ? 0 : 1,
                    MaximumBytes: childRequiresDirectory
                        ? 0
                        : maximumFileBytes!.Value,
                    RequiredLinkCount: childRequiresDirectory ? 0U : 1U,
                    ParentHandleForIndependentOpen: null,
                    LeafNameUtf8ForIndependentOpen: null);
                var child = new AcquiredPath(
                    childPath,
                    childEntry);
                retained.Add(childPath, child);
                entries.Add(childEntry);
                parent = child;
            }
            catch
            {
                childHandle.Dispose();
                throw;
            }
        }

        if ((parent.Entry.Kind == ContinuousLearningRetainedObjectKind.Directory)
            != requireDirectory)
            throw new ContinuousLearningPathIdentityException();
        if (!requireDirectory)
            ValidateArtifactStandardInformation(
                parent.Entry.Handle,
                maximumFileBytes!.Value);
        return parent.Entry.Identity;
    }

    private static SafeFileHandle OpenWindowsRoot(string pathRoot)
    {
        var handle = CreateFileWindows(
            pathRoot,
            FileReadAttributes | FileTraverse | Synchronize,
            (uint)(FileShare.Read | FileShare.Write),
            IntPtr.Zero,
            OpenExisting,
            FileFlagBackupSemantics | FileFlagOpenReparsePoint,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            handle.Dispose();
            throw new ContinuousLearningPathIdentityException();
        }
        return handle;
    }

    private static SafeFileHandle NtOpenWindowsRelative(
        SafeFileHandle parentHandle,
        string singleName,
        bool requireDirectory,
        bool isTerminal,
        bool requireMutationOwnership,
        bool requireCapabilityMutationAccess,
        out int status)
    {
        var nameBuffer = Marshal.StringToHGlobalUni(singleName);
        var unicodeStringPointer = Marshal.AllocHGlobal(
            Marshal.SizeOf<UnicodeString>());
        var parentAddRef = false;
        try
        {
            var nameLength = checked((ushort)(singleName.Length * sizeof(char)));
            var unicodeString = new UnicodeString
            {
                Length = nameLength,
                MaximumLength = checked((ushort)(nameLength + sizeof(char))),
                Buffer = nameBuffer,
            };
            Marshal.StructureToPtr(
                unicodeString,
                unicodeStringPointer,
                fDeleteOld: false);

            parentHandle.DangerousAddRef(ref parentAddRef);
            var objectAttributes = new ObjectAttributes
            {
                Length = Marshal.SizeOf<ObjectAttributes>(),
                RootDirectory = parentHandle.DangerousGetHandle(),
                ObjectName = unicodeStringPointer,
                Attributes = ObjectCaseInsensitive | ObjectDontReparse,
                SecurityDescriptor = IntPtr.Zero,
                SecurityQualityOfService = IntPtr.Zero,
            };
            var desiredAccess = requireDirectory
                ? FileReadAttributes |
                  FileTraverse |
                  Synchronize |
                  (requireCapabilityMutationAccess || requireMutationOwnership
                      ? GenericWrite |
                        FileListDirectory |
                        FileAddFile |
                        FileDeleteChild |
                        (requireMutationOwnership ? DeleteAccess : 0)
                      : 0)
                : isTerminal
                    ? FileReadData | FileReadAttributes | Synchronize
                    : throw new ContinuousLearningPathIdentityException();
            var shareAccess = requireMutationOwnership
                ? requireCapabilityMutationAccess
                    ? (uint)(FileShare.Read | FileShare.Write)
                    : (uint)FileShare.Read
                : requireCapabilityMutationAccess
                    ? (uint)(FileShare.Read | FileShare.Write)
                : isTerminal
                    ? (uint)FileShare.Read
                : (uint)(FileShare.Read | FileShare.Write);
            status = NtCreateFileWindows(
                out var handle,
                desiredAccess,
                ref objectAttributes,
                out _,
                IntPtr.Zero,
                requireDirectory ? FileAttributeDirectory : FileAttributeNormal,
                shareAccess,
                FileOpen,
                FileSynchronousIoNonAlert |
                FileOpenReparsePoint |
                (requireDirectory ? FileDirectoryFile : FileNonDirectoryFile),
                IntPtr.Zero,
                0);
            return handle;
        }
        finally
        {
            if (parentAddRef)
                parentHandle.DangerousRelease();
            Marshal.FreeHGlobal(unicodeStringPointer);
            Marshal.FreeHGlobal(nameBuffer);
        }
    }

    private static bool IsMutableMutationRoot(
        ContinuousLearningRetainedObjectRole role)
        => role is ContinuousLearningRetainedObjectRole.AdapterRoot
            or ContinuousLearningRetainedObjectRole.CacheRoot
            or ContinuousLearningRetainedObjectRole.RegistryRoot
            or ContinuousLearningRetainedObjectRole.DpoPairRoot;

    private static SafeFileHandle NtOpenExistingRegularLeafRelative(
        SafeFileHandle parentHandle,
        string singleName,
        out int status)
    {
        var nameBuffer = Marshal.StringToHGlobalUni(singleName);
        var unicodeStringPointer = Marshal.AllocHGlobal(
            Marshal.SizeOf<UnicodeString>());
        var parentAddRef = false;
        try
        {
            var nameLength = checked((ushort)(singleName.Length * sizeof(char)));
            Marshal.StructureToPtr(
                new UnicodeString
                {
                    Length = nameLength,
                    MaximumLength = checked((ushort)(nameLength + sizeof(char))),
                    Buffer = nameBuffer,
                },
                unicodeStringPointer,
                fDeleteOld: false);
            parentHandle.DangerousAddRef(ref parentAddRef);
            var objectAttributes = new ObjectAttributes
            {
                Length = Marshal.SizeOf<ObjectAttributes>(),
                RootDirectory = parentHandle.DangerousGetHandle(),
                ObjectName = unicodeStringPointer,
                Attributes = ObjectCaseInsensitive | ObjectDontReparse,
                SecurityDescriptor = IntPtr.Zero,
                SecurityQualityOfService = IntPtr.Zero,
            };
            status = NtCreateFileWindows(
                out var handle,
                FileReadAttributes | Synchronize,
                ref objectAttributes,
                out _,
                IntPtr.Zero,
                FileAttributeNormal,
                (uint)(FileShare.Read | FileShare.Write | FileShare.Delete),
                FileOpen,
                FileSynchronousIoNonAlert |
                FileOpenReparsePoint |
                FileNonDirectoryFile,
                IntPtr.Zero,
                0);
            return handle;
        }
        finally
        {
            if (parentAddRef)
                parentHandle.DangerousRelease();
            Marshal.FreeHGlobal(unicodeStringPointer);
            Marshal.FreeHGlobal(nameBuffer);
        }
    }

    private static bool TryValidateRetainedEntry(RetainedObjectEntry entry)
    {
        if (entry.Identity.Platform ==
            ServerOwnedContinuousLearningFileAccessPlatform.Posix)
        {
            // Linux не имеет Windows final-path oracle, поэтому проверка
            // делегируется POSIX backend: он заново открывает имя через
            // удержанный parent dirfd и сверяет device+inode.
            return TryValidateRetainedEntryPosix(entry);
        }

        if (entry.Handle.IsInvalid || entry.Handle.IsClosed ||
            !GetFileInformationByHandleExWindows(
                entry.Handle,
                FileInfoByHandleClass.FileAttributeTagInfo,
                out FileAttributeTagInformation attributeInfo,
                (uint)Marshal.SizeOf<FileAttributeTagInformation>()) ||
            (attributeInfo.FileAttributes & FileAttributeReparsePoint) != 0)
        {
            return false;
        }

        var isDirectory =
            (attributeInfo.FileAttributes & FileAttributeDirectory) != 0;
        if (isDirectory !=
            (entry.Kind == ContinuousLearningRetainedObjectKind.Directory) ||
            !TryReadWindowsIdentity(entry.Handle, out var identity) ||
            identity != entry.Identity)
        {
            return false;
        }

        if (entry.Kind == ContinuousLearningRetainedObjectKind.RegularFile)
        {
            return TryValidateStandardInformation(
                entry.Handle,
                entry.MinimumBytes,
                entry.MaximumBytes,
                entry.RequiredLinkCount);
        }
        return TryValidateDirectoryStandardInformation(entry.Handle) &&
               entry.MinimumBytes == 0 &&
               entry.MaximumBytes == 0 &&
               entry.RequiredLinkCount == 0;
    }

    private static bool TryValidateRegularLeaf(
        SafeFileHandle handle,
        long maximumBytes)
    {
        if (handle.IsInvalid || handle.IsClosed ||
            !GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileAttributeTagInfo,
                out FileAttributeTagInformation attributeInfo,
                (uint)Marshal.SizeOf<FileAttributeTagInformation>()) ||
            (attributeInfo.FileAttributes &
             (FileAttributeReparsePoint | FileAttributeDirectory)) != 0 ||
            !TryReadWindowsIdentity(handle, out _))
        {
            return false;
        }
        return TryValidateStandardInformation(
            handle,
            minimumBytes: 1,
            maximumBytes,
            requiredLinkCount: 1);
    }

    private static bool TryReadWindowsIdentity(
        SafeFileHandle handle,
        out RetainedFileIdentity identity)
    {
        identity = default;
        if (!GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileIdInfo,
                out FileIdInformation identityInfo,
                (uint)Marshal.SizeOf<FileIdInformation>()) ||
            identityInfo.VolumeSerialNumber == 0 ||
            identityInfo.FileId.Low == 0 && identityInfo.FileId.High == 0)
        {
            return false;
        }
        identity = new RetainedFileIdentity(
            ServerOwnedContinuousLearningFileAccessPlatform.Windows,
            identityInfo.VolumeSerialNumber,
            identityInfo.FileId.Low,
            identityInfo.FileId.High);
        return true;
    }

    private static bool TryValidateStandardInformation(
        SafeFileHandle handle,
        long minimumBytes,
        long maximumBytes,
        uint requiredLinkCount)
        => GetFileInformationByHandleExWindows(
               handle,
               FileInfoByHandleClass.FileStandardInfo,
               out FileStandardInformation standardInfo,
               (uint)Marshal.SizeOf<FileStandardInformation>()) &&
           standardInfo.EndOfFile >= minimumBytes &&
           standardInfo.EndOfFile <= maximumBytes &&
           standardInfo.DeletePending == 0 &&
           standardInfo.Directory == 0 &&
           standardInfo.NumberOfLinks == requiredLinkCount;

    private static RetainedFileIdentity ValidateWindowsHandle(
        SafeFileHandle handle,
        string expectedPath,
        bool requireDirectory)
    {
        if (handle.IsInvalid || handle.IsClosed)
            throw new ContinuousLearningPathIdentityException();
        if (!GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileAttributeTagInfo,
                out FileAttributeTagInformation attributeInfo,
                (uint)Marshal.SizeOf<FileAttributeTagInformation>()))
        {
            throw new ContinuousLearningPathIdentityException();
        }

        if ((attributeInfo.FileAttributes & FileAttributeReparsePoint) != 0)
            throw new ContinuousLearningPathIdentityException();
        var isDirectory =
            (attributeInfo.FileAttributes & FileAttributeDirectory) != 0;
        if (isDirectory != requireDirectory)
            throw new ContinuousLearningPathIdentityException();
        if (requireDirectory && !TryValidateDirectoryStandardInformation(handle))
            throw new ContinuousLearningPathIdentityException();

        var finalPath = GetWindowsFinalPath(handle);
        if (!string.Equals(
                NormalizeExpectedPath(finalPath),
                NormalizeExpectedPath(expectedPath),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ContinuousLearningPathIdentityException();
        }

        if (!GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileIdInfo,
                out FileIdInformation identityInfo,
                (uint)Marshal.SizeOf<FileIdInformation>()))
        {
            throw new ContinuousLearningPathIdentityException();
        }
        if (identityInfo.VolumeSerialNumber == 0 ||
            identityInfo.FileId.Low == 0 && identityInfo.FileId.High == 0)
        {
            throw new ContinuousLearningPathIdentityException();
        }

        return new RetainedFileIdentity(
            ServerOwnedContinuousLearningFileAccessPlatform.Windows,
            identityInfo.VolumeSerialNumber,
            identityInfo.FileId.Low,
            identityInfo.FileId.High);
    }

    private static void ValidateArtifactStandardInformation(
        SafeFileHandle handle,
        long maximumBytes)
    {
        if (!GetFileInformationByHandleExWindows(
                handle,
                FileInfoByHandleClass.FileStandardInfo,
                out FileStandardInformation standardInfo,
                (uint)Marshal.SizeOf<FileStandardInformation>()))
        {
            throw new ContinuousLearningPathIdentityException();
        }
        if (standardInfo.EndOfFile <= 0 ||
            standardInfo.EndOfFile > maximumBytes ||
            standardInfo.DeletePending != 0 ||
            standardInfo.Directory != 0 ||
            standardInfo.NumberOfLinks != 1)
        {
            throw new ContinuousLearningPathIdentityException();
        }
    }

    private static bool TryValidateDirectoryStandardInformation(
        SafeFileHandle handle)
        => GetFileInformationByHandleExWindows(
               handle,
               FileInfoByHandleClass.FileStandardInfo,
               out FileStandardInformation standardInfo,
               (uint)Marshal.SizeOf<FileStandardInformation>()) &&
           standardInfo.DeletePending == 0 &&
           standardInfo.Directory != 0;

    private static string GetWindowsFinalPath(SafeFileHandle handle)
    {
        var capacity = 512;
        while (capacity <= 32 * 1024)
        {
            var buffer = new StringBuilder(capacity);
            var length = GetFinalPathNameByHandleWindows(
                handle,
                buffer,
                (uint)capacity,
                0);
            if (length == 0)
                throw new ContinuousLearningPathIdentityException();
            if (length < capacity)
                return buffer.ToString();
            capacity = checked((int)length + 1);
        }
        throw new ContinuousLearningPathIdentityException();
    }

    private static string NormalizeExpectedPath(string path)
    {
        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            path = @"\\" + path[8..];
        else if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
            path = path[4..];

        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath);
        return string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase)
            ? fullPath
            : Path.TrimEndingDirectorySeparator(fullPath);
    }

    private static void EnsureSingleName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value is "." or ".." ||
            !string.Equals(Path.GetFileName(value), value, StringComparison.Ordinal))
        {
            throw new ContinuousLearningPathIdentityException();
        }
    }

    private static ServerOwnedContinuousLearningFileAccessPlatform DetectPlatform()
    {
        if (OperatingSystem.IsWindows())
            return ServerOwnedContinuousLearningFileAccessPlatform.Windows;
        if (OperatingSystem.IsLinux())
            return ServerOwnedContinuousLearningFileAccessPlatform.Posix;
        return ServerOwnedContinuousLearningFileAccessPlatform.Unsupported;
    }

    private sealed record AcquiredPath(
        string ExpectedPath,
        RetainedObjectEntry Entry);

    private sealed record RetainedObjectEntry(
        ContinuousLearningRetainedObjectRole Role,
        SafeFileHandle Handle,
        RetainedFileIdentity Identity,
        ContinuousLearningRetainedObjectKind Kind,
        long MinimumBytes,
        long MaximumBytes,
        uint RequiredLinkCount,
        SafeFileHandle? ParentHandleForIndependentOpen,
        byte[]? LeafNameUtf8ForIndependentOpen);

    private readonly record struct RetainedFileIdentity(
        ServerOwnedContinuousLearningFileAccessPlatform Platform,
        ulong VolumeSerialNumber,
        ulong FileIdLow,
        ulong FileIdHigh);

    private enum FileInfoByHandleClass
    {
        FileStandardInfo = 1,
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

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindows(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", EntryPoint = "ReOpenFile", SetLastError = true)]
    private static extern SafeFileHandle ReOpenFileWindows(
        SafeFileHandle originalFile,
        uint desiredAccess,
        uint shareMode,
        uint flagsAndAttributes);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileAttributeTagInformation fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileStandardInformation fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFileInformationByHandleEx",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleExWindows(
        SafeFileHandle file,
        FileInfoByHandleClass fileInformationClass,
        out FileIdInformation fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetFinalPathNameByHandleW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern uint GetFinalPathNameByHandleWindows(
        SafeFileHandle file,
        StringBuilder filePath,
        uint filePathLength,
        uint flags);

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
}
