// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TrainableRouter.Training; // Keep this lifecycle helper private to the repository training boundary.

using System.Security.Cryptography; // Use System.Security.Cryptography for this fixed resource/process ownership boundary.
using System.Collections; // Enumerate the complete portable process environment for the shared fixture/current-check path.
using System.Runtime.InteropServices; // Retain the native Windows environment snapshot and pin its explicit UTF16 preimage.
using AgentInfrastructure; // Use AgentInfrastructure for this fixed resource/process ownership boundary.
using ContinuousLearning; // Use ContinuousLearning for this fixed resource/process ownership boundary.

/// <summary>Actual application-installed resources. Retaining these resources grants no inference permission.</summary>
internal sealed class CodeDocumentationNativeCodexInstalledResources : IDisposable // Retain installed-image evidence without exposing a launch-permission factory.
{ // Keep the private resource/process boundary inside the repository training owner.
    internal enum Probe { Version, Help, SandboxHelp } // Limit installed metadata probes to version, help and sandbox help.
    private const long InstalledResourceMaximumBytes = 512L * 1024 * 1024; // A bounded installed-image read; this does not change any selection/family cap.
    private readonly FileStream[] files; // Hold all three original read handles until this resource owner is disposed.
    private readonly string[] paths, hashes; // Retain the exact leaf paths and whole-file hashes from acquisition.
    private readonly long[] lengths; // Freeze each acquired file length for current-resource checks.
    private readonly DateTime[] modified; // Freeze each acquired file timestamp independently from its hash.
    private readonly CodeDocumentationOneLessonSession session; // Keep the actual same-Session owner rather than a serialized identity.
    private readonly CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt; // Environment continuity stays subordinate to this original private receipt.
    private readonly RetainedEnvironment environment; // Hold the complete defensive UTF16 environment until owner disposal.
    private bool disposed; // Permanently invalidate this retained resource owner before cleanup.
    internal string ImagePath => paths[0]; // Use only the retained codex image leaf as the executable.
    internal string WorkingDirectory { get; } // Keep the Session-owned working directory supplied by the active receipt.
    internal string ReleaseDirectory { get; } // Keep the single resolved application release for later enumeration checks.

    private CodeDocumentationNativeCodexInstalledResources(string directory, string workingDirectory, // Keep the private resource/process boundary inside the repository training owner.
        FileStream[] files, string[] paths, string[] hashes, CodeDocumentationOneLessonSession session, // Construction cannot import authority from a caller environment or receipt description.
        CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt, RetainedEnvironment environment) // Retain the actual private receipt and complete process preimage.
    { // Keep the private resource/process boundary inside the repository training owner.
        ReleaseDirectory = directory; WorkingDirectory = workingDirectory; // Bind the release and held Session directory into this private owner.
        this.files = files; this.paths = paths; this.hashes = hashes; // Retain the original opened handles and their corresponding paths and hashes.
        this.session = session; this.receipt = receipt; this.environment = environment; // Transfer only actual held objects, never detached environment data as authority.
        lengths = files.Select(f => f.Length).ToArray(); modified = paths.Select(File.GetLastWriteTimeUtc).ToArray(); // Snapshot original lengths and timestamps while all resource handles remain held.
    } // Keep the private resource/process boundary inside the repository training owner.

    internal static OperationResult<CodeDocumentationNativeCodexInstalledResources> OpenUnderHeldSession( // Retain exact application-installed files only under the current held Session.
        CodeDocumentationOneLessonSession session, CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt, CancellationToken token) // Retain exact application-installed files only under the current held Session.
    { // Retain exact application-installed files only under the current held Session.
        if (!receipt.IsActiveFor(session)) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexInstalledResources>(); // Reject a foreign or expired held receipt before installed file acquisition.
        var opened = new List<FileStream>(); // Track every acquired file until successful owner transfer or deterministic cleanup.
        RetainedEnvironment? capturedEnvironment = null; // Cover environment cleanup even if resource construction or current checks throw.
        try // Retain exact application-installed files only under the current held Session.
        { // Retain exact application-installed files only under the current held Session.
            var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenAI", "Codex", "bin"); // Resolve the fixed application-owned Codex bin root from LocalApplicationData.
            if (!Directory.Exists(root)) return Unavailable("The application-installed Codex bin directory is absent: " + root); // Report the exact absent application bin root without a PATH fallback.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(root)) return Unavailable("The application-installed Codex bin path has a reparse point: " + root); // Refuse a redirected application root before enumerating releases.
            // A directory listing is not an active-release certificate. Ambiguity refuses; no newest/PATH fallback.
            var directories = Directory.EnumerateDirectories(root).ToArray(); // Enumerate complete release candidates rather than selecting newest directory metadata.
            if (directories.Length != 1 || Path.GetFileName(directories[0]).Length != 16 || // Require exactly one release whose fixed name has the supported length.
                !Path.GetFileName(directories[0]).All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f')) // Require the application release name to contain only lowercase hexadecimal characters.
                return Unavailable("The installed release cannot be resolved uniquely from the application-owned bin directory."); // Refuse ambiguity rather than guessing the active application image.
            var directory = Path.GetFullPath(directories[0]); // Normalize the single release directory before opening its fixed leaf resources.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory)) return Unavailable("The installed release path has a reparse point."); // Refuse redirection of the selected release directory.
            string[] paths = [Path.Combine(directory, "codex.exe"), Path.Combine(directory, "codex-command-runner.exe"), Path.Combine(directory, "codex-windows-sandbox-setup.exe")]; // Resolve only the installed image, command runner and sandbox setup leaves.
            var hashes = new List<string>(); // Collect complete file hashes in the same fixed leaf order.
            foreach (var path in paths) // Acquire all three exact leaves while preserving earlier handles on later failure.
            { // Retain exact application-installed files only under the current held Session.
                if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)) return Unavailable("Installed resource path has a reparse point: " + path); // Refuse a redirected resource leaf before obtaining its handle.
                // Installed program files have application ACLs, not Session custody ACLs. Never rewrite their ACLs.
                var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan); // Retain a read-only handle whose sharing excludes resource writes and replacement.
                opened.Add(file); // Register the handle immediately so every later failure has a cleanup owner.
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(file.SafeFileHandle, path) || file.Length <= 0 || file.Length > InstalledResourceMaximumBytes) // Check exact opened path and finite nonempty length before hashing.
                    return Unavailable("The opened installed resource does not match its exact path: " + path); // Return a typed refusal for mismatched or invalid acquired files.
                hashes.Add(HashWhole(file, token)); // Hash the complete retained file while observing caller cancellation.
            } // Retain exact application-installed files only under the current held Session.
            token.ThrowIfCancellationRequested(); capturedEnvironment = RetainedEnvironment.CaptureCurrent(); // Capture the entire actual environment only under this existing held receipt.
            var result = TransferRetainedFiles(opened, // Keep the pending handle owner until all current-input checks have succeeded.
                files => new CodeDocumentationNativeCodexInstalledResources(directory, receipt.OwnedDirectory, files, paths, hashes.ToArray(), session, receipt, capturedEnvironment), // The acquisition scope still owns all files/environment if construction throws.
                pending => receipt.IsActiveFor(session) && pending.IsCurrent()); // A throwing filesystem revalidation must dispose the pending owner too.
            if (result is not null) capturedEnvironment = null; // Clear pending ownership only after the last throwing revalidation and accepted transfer.
            return result is null ? CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexInstalledResources>() // Preserve the original false-path refusal without handing out an owner.
                : OperationResult<CodeDocumentationNativeCodexInstalledResources>.Ok(result); // Transfer only the completely revalidated installed-resource owner.
        } // Retain exact application-installed files only under the current held Session.
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException) // Map genuine filesystem access faults to the existing bounded unavailable result.
        { return Unavailable("Opening retained application-installed resources failed: " + ex.GetType().Name + ":" + ex.HResult); } // Report the original filesystem error type and HResult without minting authority.
        finally { try { capturedEnvironment?.Dispose(); } finally { foreach (var file in opened) file.Dispose(); } } // Release pending environment and original handles on every unaccepted exit.
    } // Retain exact application-installed files only under the current held Session.

    internal static T? TransferRetainedFiles<T>(List<FileStream> acquired, Func<FileStream[], T> construct, Func<T, bool> revalidate) where T : class, IDisposable // Shared handle-lifetime logic; isolated test owners cannot issue installed launch authority.
    { // Keep construction, refusal and throwing revalidation under one pending ownership scope.
        T? pending = null; Exception? original = null; // Preserve a construction/revalidation exception while the result remains cleanup-owned.
        try // Preserve the original exception or cancellation from construction and revalidation.
        { // Admit a retained owner only after its complete revalidation succeeds.
            pending = construct(acquired.ToArray()); // The acquisition list still covers a constructor that throws before returning an owner.
            if (!revalidate(pending)) return null; // The finally disposes both pending owner and any originally acquired handles on refusal.
            var accepted = pending; acquired.Clear(); pending = null; // Transfer ownership only after the last potentially throwing check.
            return accepted; // Only this return leaves the retained handles open for the accepted caller.
        } // End the successful-transfer scope without renewing authority.
        catch (Exception error) { original = error; throw; } // Keep the exact original exception and cancellation object across cleanup.
        finally // Every unaccepted exit deterministically releases all handles, including post-acquisition filesystem faults.
        { // Individual handle closure remains attempted even if pending-owner disposal itself fails.
            Exception? cleanup = null; // Retain only one bounded cleanup cause without skipping any individual acquired file.
            try { pending?.Dispose(); } catch (Exception error) { cleanup = error; } // A pending-owner failure cannot prevent original-handle cleanup.
            foreach (var file in acquired) { try { file.Dispose(); } catch (Exception error) { cleanup ??= error; } } // Attempt every acquired handle independently even after an earlier disposal fault.
            acquired.Clear(); // The acquisition list cannot retain closed handles after an unaccepted exit.
            if (cleanup is not null && original is not null) original.Data["NativeResourceCleanup"] = cleanup.GetType().Name + ":" + cleanup.HResult; // Attach a bounded secondary cause to the original error.
            else if (cleanup is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(cleanup).Throw(); // A cleanup-only failure is not silently accepted.
        } // Complete pending-owner cleanup before the original exception returns to its caller.
    } // This reusable lifecycle helper transfers files, never permission or a native binding.

    internal IReadOnlyList<string> ProbeArguments(Probe probe) => probe switch // Expose only the fixed resource probe vector without caller-selected arguments.
    { Probe.Version => ["--version"], Probe.Help => ["--help"], Probe.SandboxHelp => ["sandbox", "--help"], _ => throw new ArgumentOutOfRangeException(nameof(probe)) }; // Expose only the fixed resource probe vector without caller-selected arguments.

    internal bool IsCurrent() // Revalidate the exact retained release and original file handles.
    { // Revalidate the exact retained release and original file handles.
        if (disposed || !receipt.IsActiveFor(session) || !environment.IsCurrent() || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(ReleaseDirectory)) return false; // Require the original active receipt and complete actual environment before file continuity.
        var currentRelease = Directory.EnumerateDirectories(Path.GetDirectoryName(ReleaseDirectory)!).ToArray(); // Re-enumerate the application release root while original file handles remain held.
        if (currentRelease.Length != 1 || !StringComparer.OrdinalIgnoreCase.Equals(Path.GetFullPath(currentRelease[0]), ReleaseDirectory)) return false; // Require the same unique normalized release at every current-input check.
        for (var i = 0; i < files.Length; i++) // Independently compare each original resource handle with its frozen physical metadata.
            if (files[i].Length != lengths[i] || File.GetLastWriteTimeUtc(paths[i]) != modified[i] || // Reject changed length or timestamp independently from handle/path identity.
                !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(files[i].SafeFileHandle, paths[i])) return false; // Reject a retained handle that no longer matches its exact requested leaf.
        // FileShare.Read excludes write/delete replacement while each original resource handle is held.
        return true; // Accept only resource continuity; this result grants no native permission.
    } // Revalidate the exact retained release and original file handles.

    internal bool IsCurrentFor(CodeDocumentationOneLessonSession expected,
        CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt expectedReceipt)
        => ReferenceEquals(session, expected) && ReferenceEquals(receipt, expectedReceipt) && IsCurrent(); // A foreign live resource owner cannot join another Session's inputs.

    internal string Describe(string actualVersion) // Return descriptive installed-resource evidence without granting launch permission.
        => actualVersion + "; image=" + ImagePath + "; image-sha256=" + hashes[0] + // Describe the actual version and exact retained image digest as diagnostic data.
           "; command-runner-sha256=" + hashes[1] + "; sandbox-setup-sha256=" + hashes[2]; // Include both retained helper digests without implying main-loader confinement.
    internal RetainedEnvironment.PinnedBlock PinCurrentEnvironment() // Only this genuine fixed resource owner supplies the production process environment.
    { // Pin a complete copy after actual same-receipt current-input verification.
        if (!IsCurrent()) throw new IOException("Installed launch inputs changed before environment allocation."); // No inherited-environment fallback or renewal after a mismatch.
        return environment.Pin(); // The process owns this finite allocation independently until its create/cleanup scope exits.
    } // This retained environment is resource continuity, never MAIN permission enforcement.
    internal string ResolveCurrentCodexHome() // Resolve only the documented MAIN config root from the actual retained environment.
    {
        if (!IsCurrent()) throw new IOException("Installed inputs changed before MAIN home resolution.");
        return environment.ResolveCodexHome(); // No arbitrary variable lookup or secret-bearing diagnostic is exposed.
    }
    private static string HashWhole(FileStream stream, CancellationToken token) // Hash every bounded installed byte with caller cancellation and complete-length checks.
    { // Hash every bounded installed byte with caller cancellation and complete-length checks.
        stream.Position = 0; using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); var buffer = new byte[65536]; // Read the complete file from its start with one finite SHA256 buffer.
        var length = stream.Length; long observed = 0; int count; // Track original length and all observed bytes to reject incomplete or growing reads.
        while ((count = stream.Read(buffer)) != 0) { token.ThrowIfCancellationRequested(); observed = checked(observed + count); if (observed > length) throw new IOException("Installed resource length changed during hashing."); hash.AppendData(buffer, 0, count); } // Hash every actual chunk, preserving cancellation and rejecting bytes beyond original length.
        if (observed != length || stream.Length != length) throw new IOException("Installed resource was incomplete during hashing."); // Reject short reads or a changed final length before accepting a resource digest.
        stream.Position = 0; return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant(); // Restore the retained file position and return its complete lowercase digest.
    } // Hash every bounded installed byte with caller cancellation and complete-length checks.
    private static OperationResult<CodeDocumentationNativeCodexInstalledResources> Unavailable(string detail) // Return the existing typed installed-permission refusal.
        => OperationResult<CodeDocumentationNativeCodexInstalledResources>.Fail(detail, CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable); // Return the existing typed installed-permission refusal.
    public void Dispose() // End installed-file retention and attempt every owned handle release.
    { // End installed-file retention and attempt every owned handle release.
        if (disposed) return; disposed = true; // End owner validity once, before attempting any retained handle release.
        try { environment.Dispose(); } finally { try { files[0].Dispose(); } finally { try { files[1].Dispose(); } finally { files[2].Dispose(); } } } // End environment retention and attempt image plus both helper releases independently.
    } // End installed-file retention and attempt every owned handle release.

    /// <summary>Complete environment data and allocation lifetime; this helper cannot issue a resource/launch owner.</summary>
    internal sealed class RetainedEnvironment : IDisposable // Share production environment validation/current checks with portable controlled fixtures.
    { // Keep process data private, finite and independent from persisted selection-family bounds.
        internal const int MaximumCodeUnits = 1_048_576; // At most 2MiB of explicit UTF16 including separators; no stored-family or Student cap changes.
        private readonly char[] canonical; // Preserve exact UTF16 code units, including non-BMP pairs, without encoding replacement.
        private bool invalidated, disposed; // An observed mismatch is permanent even if the ambient variable is later restored.
        private RetainedEnvironment(char[] canonical) { this.canonical = canonical; } // Only the complete validating capture paths create data preimages.

        internal static RetainedEnvironment CaptureCurrent() // Read the actual complete process environment; caller dictionaries cannot replace production capture.
        { // Native Windows capture includes hidden drive-current-directory names omitted by generic dictionaries.
            if (OperatingSystem.IsWindows()) return CaptureWindows(); // Preserve every native entry, including '=C:' drive entries.
            var entries = new List<KeyValuePair<string, string>>(); // Portable fixtures use the actual process dictionary, never an imported authority record.
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables()) // Read all entries without silently dropping unfamiliar names.
                entries.Add(new((string)item.Key, (string)item.Value!)); // Immutable strings are copied into the shared validated canonical preimage below.
            return CaptureEntries(entries); // Apply the same finite Windows grammar and canonicalization on every platform.
        } // A complete environment snapshot is data, not a capability factory.

        private static RetainedEnvironment CaptureWindows() // Own exactly the snapshot returned by the Windows environment API.
        { // Scan the finite native block before allocating strings or a managed full preimage.
            var pointer = GetEnvironmentStringsW(); // Windows supplies one complete stable snapshot, including drive-current-directory entries.
            if (pointer == IntPtr.Zero) throw new IOException("Native environment snapshot acquisition failed."); // Never include environment/auth values in diagnostics.
            return CaptureAndReleaseSnapshot(() => // Keep the parsed canonical owner pending until this exact snapshot has been released successfully.
            { // Require complete double-NUL termination within the fixed process allocation bound.
                var length = 0; var previousNull = false; // Count UTF16 code units before any full-block managed copy.
                for (; length < MaximumCodeUnits; length++) // Reject overbound snapshots rather than dropping variables or truncating values.
                { // Observe raw UTF16 code units, preserving surrogate pairs and Windows hidden entries.
                    var isNull = Marshal.ReadInt16(pointer, checked(length * sizeof(char))) == 0; // Read only within the finite scan bound.
                    if (previousNull && isNull) { length++; break; } // Include both final NULs in the complete allocation preimage.
                    previousNull = isNull; // A single separator does not end a nonempty environment block.
                } // End the finite native scan without accepting incomplete termination.
                if (length < 2 || length > MaximumCodeUnits || !previousNull || Marshal.ReadInt16(pointer, checked((length - 1) * sizeof(char))) != 0 || // Require both terminal NULs within the finite native scan.
                    Marshal.ReadInt16(pointer, checked((length - 2) * sizeof(char))) != 0) throw new IOException("Native environment is incomplete or exceeds its finite allocation bound."); // This fixed refusal never prints unknown environment values.
                var block = new char[length]; // Copy only after the complete native terminator and finite allocation length are established.
                try { Marshal.Copy(pointer, block, 0, length); return CaptureBlock(block); } // Production and portable fixtures share the actual drive-aware complete-block parser.
                finally { Array.Clear(block); } // Clear the temporary private complete snapshot even after parser/canonicalizer failure.
            }, () => FreeEnvironmentStringsW(pointer)); // The production release callback is private and fixed; it executes once after every capture outcome.
        } // No private configuration or auth source is inspected to infer an environment.

        internal static RetainedEnvironment CaptureAndReleaseSnapshot(Func<RetainedEnvironment> capture, Func<bool> release) // Share only complete data-owner transfer/release with deterministic fixtures; no installed authority is issued.
        { // A canonical owner cannot leave pending custody while any native release work can still fail.
            RetainedEnvironment? pending = null; System.Runtime.ExceptionServices.ExceptionDispatchInfo? primary = null; // Retain the actual complete owner and exact earlier exception/token through release.
            try // Cover capture, failed release and outward exception arbitration with one pending-owner cleanup scope.
            { // A failed capture still receives its one independent native release attempt.
                try { pending = capture(); if (pending is null) throw new IOException("Native environment capture returned no retained owner."); } // Install pending ownership before evaluating any release callback.
                catch (Exception error) { primary = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error); } // Preserve the original capture/parser/OCE object rather than replacing it with cleanup failure.
                Exception? releaseFailure = null; string? releaseDetail = null; // Retain only bounded actual release information, never exception messages or environment values.
                try // Invoke the one original release exactly once, including capture failure and cancellation paths.
                { // A false return and a thrown release are both explicit failure, never successful owner transfer.
                    if (!release()) { releaseFailure = new IOException("Native environment snapshot release failed."); releaseDetail = "FreeEnvironmentStringsW:false"; } // Preserve the actual fixed false-release fact as bounded secondary evidence.
                } // No retry or second release follows an uncertain release outcome.
                catch (Exception error) { releaseFailure = error; releaseDetail = "FreeEnvironmentStringsW:" + error.GetType().Name + ":" + error.HResult; } // Keep the actual release error object for cleanup-only failure and sanitized type/HResult for an earlier primary.
                if (primary is not null) // The original capture failure always wins over a later actual release cause.
                { // Attach release evidence before the original exception leaves pending-owner cleanup.
                    if (releaseFailure is not null) primary.SourceException.Data["NativeEnvironmentSnapshotCleanup"] = releaseDetail; // Retain the real bounded secondary cause without leaking its message or values.
                    primary.Throw(); // Preserve original dispatch, object and caller token after the release attempt.
                } // A successful capture is still pending until release failure arbitration is finished.
                if (releaseFailure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(releaseFailure).Throw(); // A cleanup-only throw preserves its original object; false-release keeps its fixed IOException.
                var accepted = pending!; pending = null; return accepted; // Transfer only after all potentially throwing native release work has completed successfully.
            } // Unaccepted complete canonical data must be cleared even when release fails after capture succeeded.
            finally { pending?.Dispose(); } // Dispose/clear the actual pending canonical array on every false/throw/unknown unaccepted exit.
        } // This data-only helper cannot construct a genuine resource, permission, Session or launch binding.

        internal static RetainedEnvironment CaptureBlock(ReadOnlySpan<char> block) // Data-only shared parser of the exact complete native UTF16 block.
        { // Reject incomplete terminators and embedded empty entries instead of accepting a prefix and silently dropping its suffix.
            if (block.Length < 2 || block.Length > MaximumCodeUnits || block[^1] != '\0' || block[^2] != '\0') // Bound the complete UTF16 preimage before parsing/canonical allocation.
                throw new IOException("Native environment is incomplete or exceeds its finite allocation bound."); // Report fixed grammar/size information without raw values.
            var entries = new List<KeyValuePair<string, string>>(); var start = 0; // Retain every complete name/value entry and preserve special Windows drive names.
            for (var offset = 0; offset < block.Length - 1; offset++) // Visit the entire supplied block, excluding only its final extra NUL.
            { // Split actual complete entries, never a generic '=' parser that loses '=C:' names.
                if (block[offset] != '\0') continue; // Preserve all UTF16 value characters until their actual separator.
                if (offset == start) // An empty entry is allowed only for the complete two-NUL empty environment.
                { if (block.Length == 2) break; throw new IOException("Native environment contains an incomplete entry inventory."); } // Never accept an early terminator with unexamined suffix bytes.
                var entry = block[start..offset]; var separator = entry[0] == '=' ? entry[1..].IndexOf('=') : entry.IndexOf('='); // Search past the hidden drive name's leading '='.
                if (entry[0] == '=' && separator >= 0) separator++; // Convert the drive-name search offset back to the complete entry offset.
                if (separator <= 0) throw new IOException("Native environment entry has invalid Windows grammar."); // Malformed entries refuse rather than disappear from the inventory.
                entries.Add(new(entry[..separator].ToString(), entry[(separator + 1)..].ToString())); start = offset + 1; // Keep complete names and full values, including empty values or additional '=' characters.
            } // The shared canonicalizer now validates names, duplicates and complete allocation size.
            return CaptureEntries(entries); // Parsed fixture/native data cannot issue a production resource owner.
        } // Complete block parsing is shared evidence, not Windows permission enforcement.

        internal static RetainedEnvironment CaptureEntries(IEnumerable<KeyValuePair<string, string>> entries) // Data-only fixture entry; it cannot construct the private installed resource owner.
        { // Validate the complete finite inventory before allocating the explicit environment block.
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase); var retained = new List<KeyValuePair<string, string>>(); long units = 1; // Count the final extra NUL plus each complete entry/separator.
            foreach (var entry in entries) // Unknown valid variables are retained rather than filtered to an invented allowlist.
            { // Windows names reject NUL/'=' except the exact hidden drive-current-directory grammar.
                var name = entry.Key; var value = entry.Value; // Retain immutable input strings without exposing them in diagnostics.
                var driveName = name is { Length: 3 } && name[0] == '=' && char.IsAsciiLetter(name[1]) && name[2] == ':'; // Preserve required '=C:' names without accepting arbitrary leading '=' names.
                if (string.IsNullOrEmpty(name) || value is null || name.Contains('\0') || value.Contains('\0') || // Embedded NUL cannot represent one complete Windows name/value pair.
                    (name.Contains('=') && !driveName) || !names.Add(name)) throw new IOException("Environment contains an invalid or duplicate Windows entry."); // Case-insensitive duplicates refuse with no name/value leak.
                units = checked(units + name.Length + 1L + value.Length + 1L); // Include '=' and the entry NUL before any full-block allocation.
                if (units > MaximumCodeUnits) throw new IOException("Environment exceeds its finite UTF16 allocation bound."); // Do not allocate/truncate an overbound block.
                retained.Add(new(name, value)); // Preserve every complete admitted entry as defensive immutable data.
            } // Only a fully validated finite inventory can become an explicit block.
            retained.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.Key, right.Key)); // Canonical Windows name order makes enumeration order irrelevant.
            var block = new char[checked((int)Math.Max(2, units))]; var offset = 0; // Empty environment still has the mandatory two NULs.
            foreach (var entry in retained) // Copy exact UTF16 code units; string encoding must not replace a surrogate.
            { // Array initialization supplies complete entry separators and the final extra NUL.
                entry.Key.CopyTo(0, block, offset, entry.Key.Length); offset += entry.Key.Length; block[offset++] = '='; // Copy the entire validated name and one actual delimiter.
                entry.Value.CopyTo(0, block, offset, entry.Value.Length); offset += entry.Value.Length + 1; // Preserve the full value, including an empty value, then its NUL.
            } // No caller array or collection is retained by this environment data owner.
            return new(block); // This private canonical preimage is never serialized or printed by production.
        } // Complete canonical data cannot mint Session, permission or launch authority.

        internal bool IsCurrent() // Compare actual current complete environment against the held defensive preimage.
        { // An observed mismatch invalidates this data owner without releasing or renewing the genuine Session receipt.
            if (disposed || invalidated) return false; // Disposal/restoration cannot revive this original data owner.
            try { using var current = CaptureCurrent(); if (canonical.AsSpan().SequenceEqual(current.canonical)) return true; } // Compare the whole exact canonical UTF16 preimage, not a variable allowlist or digest.
            catch { invalidated = true; throw; } // Unknown current reads invalidate before the original bounded failure leaves the owner.
            invalidated = true; return false; // Retain changed-input refusal even after later ambient restoration.
        } // Current checks grant no instruction/config/MCP enforcement.

        internal string ResolveCodexHome() // Read one documented root from the complete defensive environment, never ambient replacement data.
        {
            if (!IsCurrent()) throw new IOException("Retained environment changed before MAIN home resolution.");
            string? configured = null;
            for (var start = 0; start < canonical.Length - 1;)
            {
                var end = Array.IndexOf(canonical, '\0', start); if (end == start) break;
                ReadOnlySpan<char> entry = canonical.AsSpan(start, end - start);
                const string prefix = "CODEX_HOME=";
                if (entry.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) configured = entry[prefix.Length..].ToString();
                start = end + 1;
            }
            var home = configured ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codex");
            if (string.IsNullOrWhiteSpace(home) || !Path.IsPathFullyQualified(home))
                throw new IOException("The installed MAIN configuration root is not an absolute path.");
            return Path.GetFullPath(home);
        }

        internal PinnedBlock Pin(Action<PinnedBlock>? afterAllocationForTesting = null) // Production and controlled portable fixtures use this exact allocation ownership path.
        { // Do not transfer a pin until all throwing post-allocation fixture work completes.
            if (disposed || invalidated) throw new IOException("Retained environment is no longer current."); // No fallback to inherited environment after owner invalidation.
            var pending = new PinnedBlock(canonical); // Copy and pin the entire already bounded double-NUL UTF16 block.
            try { afterAllocationForTesting?.Invoke(pending); return pending; } // A test hook observes/faults data allocation only; it cannot grant production authority.
            catch { pending.Dispose(); throw; } // Release the actual pin on a fault/cancellation before ownership transfer.
        } // Successful callers must keep this allocation scoped through the actual native create.

        public void Dispose() { if (disposed) return; disposed = true; Array.Clear(canonical); } // End retention before clearing the complete private managed preimage.

        internal sealed class PinnedBlock : IDisposable // One exact pin/copy allocation owner, independently disposable on every process exit.
        { // Keep the explicit environment pointer valid until the original pin is released.
            private readonly char[] block; private GCHandle pin; // Own a defensive full copy and its actual native-address pin.
            internal PinnedBlock(char[] canonical) // Only validated environment data supplies production allocation bytes.
            { // Cover a failure between defensive copying and successful pin acquisition.
                block = canonical.ToArray(); // Never pin a mutable caller collection or the environment owner's canonical array.
                try { pin = GCHandle.Alloc(block, GCHandleType.Pinned); } catch { Array.Clear(block); throw; } // Clear the private copy when pin allocation fails.
            } // A successful constructor retains the actual GCHandle until deterministic disposal.
            internal IntPtr Pointer => pin.IsAllocated ? pin.AddrOfPinnedObject() : throw new ObjectDisposedException(nameof(PinnedBlock)); // A disposed allocation cannot yield an inherited/null fallback pointer.
            internal int CodeUnits => block.Length; // Expose only finite allocation size, never raw environment data or auth diagnostics.
            internal bool IsReleased => !pin.IsAllocated; // Portable fixtures can check actual pin closure without inventing process evidence.
            public void Dispose() { if (!pin.IsAllocated) return; pin.Free(); Array.Clear(block); } // Release the pin before clearing its complete secret-bearing copy; never clear ownership before throwing work.
        } // Allocation cleanup does not change existing process/stream/Job ownership.

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetEnvironmentStringsW(); // Acquire the complete native Unicode environment including hidden Windows drive entries.
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool FreeEnvironmentStringsW(IntPtr environment); // Release the exact acquired native snapshot on every scan/parse exit.
    } // Retained complete environment data remains separate from genuine MAIN enforcement.
} // End installed-file retention and attempt every owned handle release.
