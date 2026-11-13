// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TrainableRouter.Training; // Keep launch ownership inside the repository training boundary.

using System.Text.Json; // Serialize descriptive launch-input bytes without importing authority.
using AgentInfrastructure; // Use the existing typed OperationResult refusal contract.
using ContinuousLearning; // Use the existing secure local-file custody boundary.

/// <summary>Retained launch inputs. Synthetic storage fixtures never establish installed permission enforcement.</summary>
internal sealed class CodeDocumentationNativeCodexLaunchInputOwner : IDisposable // Keep this native lifecycle boundary private and bounded.
{ // Keep this native lifecycle boundary private and bounded.
    internal const string PermissionUnavailable = "CODE_DOCUMENTATION_ONE_LESSON_NATIVE_PERMISSION_ENFORCEMENT_UNAVAILABLE"; // Preserve a concrete next boundary without inventing an enforcement receipt.
    internal const string SyntheticKind = "SyntheticNativeStartedFixture"; // This kind is never an ordinary native permission capability.
    private readonly CodeDocumentationOneLessonSession session; // Retain the actual requesting Session object, never its serialized ID alone.
    private readonly string directory; // A fixture directory is accepted only by the explicit internal fixture entry.
    private readonly FileStream[] files; // Keep all three original handles until independent finals and binding invalidation complete.
    private readonly string[] paths; // Pin exact fixed leaf names, excluding caller argv and provider selection.
    private readonly byte[][] bytes; // Retain complete bounded fixture input preimages for current-input checks.
    private readonly DateTime[] modified; // A same-byte replacement or retimestamp is a changed input too.
    private readonly byte[] canonical; // Freeze a complete descriptive input record; decoding it cannot recreate this owner.
    private bool disposed; // Disposal permanently ends this resource owner.

    private CodeDocumentationNativeCodexLaunchInputOwner(CodeDocumentationOneLessonSession session, string directory, FileStream[] files, string[] paths, byte[][] bytes) // Keep this native lifecycle boundary private and bounded.
    { // Keep this native lifecycle boundary private and bounded.
        this.session = session; this.directory = directory; this.files = files; this.paths = paths; this.bytes = bytes; // Keep exact object and handle references.
        modified = paths.Select(File.GetLastWriteTimeUtc).ToArray(); // Capture timestamps from the held fixed paths.
        canonical = JsonSerializer.SerializeToUtf8Bytes(new { Kind = SyntheticKind, WorkingDirectory = directory, // Keep this native lifecycle boundary private and bounded.
            ImagePath = paths[0], ImageSha256 = CodeDocumentationNativeCodexSelectionWire.Sha256(bytes[0]), ImageLength = bytes[0].Length, // Pin the exact retained fixture image bytes and path.
            ConfigPath = paths[1], ConfigCanonicalBase64 = Convert.ToBase64String(bytes[1]), // Bind the complete fixture config bytes and exact retained path.
            PermissionPath = paths[2], PermissionCanonicalBase64 = Convert.ToBase64String(bytes[2]), // Bind the complete fixture permission input without treating it as enforcement proof.
            ModifiedUtc = modified.Select(t => t.ToString("O", System.Globalization.CultureInfo.InvariantCulture)).ToArray(), // Bind the original held input timestamps.
            Arguments = new[] { "synthetic-no-execution" }, InstructionAndMcpComposition = "synthetic-no-capability" }); // Record fixture data honestly, without enforcement=true or guessed native flags.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal static OperationResult<CodeDocumentationNativeCodexLaunchInputOwner> ResolveInstalledUnderHeldSession( // Refuse ordinary execution until a genuine installed permission owner exists.
        CodeDocumentationOneLessonSession session, CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt,
        CancellationToken token = default) // Resolve genuine installed resources without importing permission authority.
    { // Refuse ordinary execution until a genuine installed permission owner exists.
        if (!receipt.IsActiveFor(session)) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(); // A stale receipt cannot even resolve the fixed installed boundary.
        token.ThrowIfCancellationRequested();
        var opened = CodeDocumentationNativeCodexInstalledResources.OpenUnderHeldSession(session, receipt, token);
        if (!opened.IsSuccess || opened.Value is null) return opened.MapError<CodeDocumentationNativeCodexLaunchInputOwner>();
        using var resources = opened.Value;
        CodeDocumentationNativeCodexWindowsProcess.Observation observation;
        try
        {
            observation = CodeDocumentationNativeCodexWindowsProcess.ProbeInstalledAsync(resources,
                CodeDocumentationNativeCodexInstalledResources.Probe.Version, token).GetAwaiter().GetResult();
        }
        catch (System.ComponentModel.Win32Exception ex)
        { return OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Fail("The installed resource probe failed at " + ex.Message + "; native-error=" + ex.NativeErrorCode, PermissionUnavailable); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
        { return OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Fail("The installed resource probe failed: " + ex.GetType().Name + ":" + ex.HResult + ":" + ex.Message, PermissionUnavailable); }
        if (!receipt.IsActiveFor(session) || !resources.IsCurrent()) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>();
        session.RecordInstalledNativeResourceObservationUnderHeldSession(receipt, observation); // Retain actual command/tree evidence as data; never a launch capability.
        if (!observation.TreeClosed || observation.Failure is not null || observation.ExitCode != 0 || observation.StandardError.Length != 0)
            return OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Fail("The installed version probe was incomplete: " + observation.Failure, PermissionUnavailable);
        var version = CodeDocumentationNativeCodexSelectionWire.Utf8.GetString(observation.StandardOutput).Trim();
        if (!version.StartsWith("codex-cli ", StringComparison.Ordinal) || version.Length > 128 || version.Contains('\n') || version.Contains('\r'))
            return OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Fail("The exact installed image returned an unsupported version response.", PermissionUnavailable);
        // Hold real MAIN disk candidates, complete prepared stdin/schema and the actual installed
        // environment/image together under this same private receipt. Disk custody alone cannot
        // establish effective managed instructions/MCP or protection of the MAIN loader.
        var retained = CodeDocumentationNativeCodexInstalledMainInputs.OpenUnderHeldSession(session, receipt, resources, token);
        if (!retained.IsSuccess || retained.Value is null) return retained.MapError<CodeDocumentationNativeCodexLaunchInputOwner>();
        using var mainInputs = retained.Value;
        token.ThrowIfCancellationRequested();
        return mainInputs.ConsumeBeforeSemanticAttempt(session, token); // Read actual installed configuration under this same held owner before the still-closed semantic binding/Started boundary.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal static OperationResult<CodeDocumentationNativeCodexLaunchInputOwner> OpenSyntheticFixtureUnderHeldSession( // Open only the explicit fixture inputs without granting installed permissions.
        CodeDocumentationOneLessonSession session, CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt, string directory) // Open only the explicit fixture inputs without granting installed permissions.
    { // Open only the explicit fixture inputs without granting installed permissions.
        if (!receipt.IsActiveFor(session)) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(); // Fixture data still requires genuine held material/disclosure owners.
        var handles = new List<FileStream>(); // Release every acquired handle if a later input fails.
        try // Open only the explicit fixture inputs without granting installed permissions.
        { // Open only the explicit fixture inputs without granting installed permissions.
            directory = Path.GetFullPath(directory); // Normalize the fixture's concrete root once before opening fixed leaves.
            if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory)) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(); // A fixture cannot launder a symlink into retained launch inputs.
            string[] paths = [Path.Combine(directory, "synthetic-image.input"), Path.Combine(directory, "synthetic-config.input"), Path.Combine(directory, "synthetic-permission.input")]; // No caller-controlled argv, model, provider or individual leaf path.
            var snapshots = new List<byte[]>(); // Complete bounded preimages remain distinct from permission proof.
            foreach (var path in paths) // Open only the explicit fixture inputs without granting installed permissions.
            { // Open only the explicit fixture inputs without granting installed permissions.
                var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.SequentialScan); handles.Add(stream); // Explicit synthetic fixtures allow controlled input mutation; this is not an installed custody/enforcement claim.
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path)) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(); // Validate custody before length/read allocation.
                if (stream.Length > 1024) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(true); // This is a small synthetic-input bound, not a relaxed installed image cap.
                var input = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, 1024); // Two complete bounded reads reject partial/unstable data.
                if (input is null || input.Length == 0) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(); // Unknown input cannot issue a binding.
                snapshots.Add(input); // Keep complete bytes without silently truncating a fixture.
            } // Finish this bounded owner check without renewing authority or an attempt.
            var owner = new CodeDocumentationNativeCodexLaunchInputOwner(session, directory, handles.ToArray(), paths, snapshots.ToArray()); handles.Clear(); // Transfer custody only after all inputs are admitted.
            return OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Ok(owner); // This owner has SyntheticKind only.
        } // Finish this bounded owner check without renewing authority or an attempt.
        finally { foreach (var stream in handles) stream.Dispose(); } // Failed opening never leaks a retained fixture handle.
    } // Finish this bounded owner check without renewing authority or an attempt.

    internal byte[] CopyCanonicalBytes() => canonical.ToArray(); // A copy is data, never an owner.
    internal bool IsCurrentFor(CodeDocumentationOneLessonSession expected) // Recheck complete actual fixture input bytes and original retained handles.
    { // Recheck complete actual fixture input bytes and original retained handles.
        if (disposed || !ReferenceEquals(session, expected) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(directory)) return false; // Require original owner identity and current path custody.
        for (var i = 0; i < files.Length; i++) // Recheck complete actual fixture input bytes and original retained handles.
        { // Recheck complete actual fixture input bytes and original retained handles.
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(files[i].SafeFileHandle, paths[i]) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(paths[i]) || files[i].Length != bytes[i].Length || File.GetLastWriteTimeUtc(paths[i]) != modified[i]) return false; // Check original retained handle, size, ACL and timestamp independently.
            var current = SecureLocalFileBoundary.ReadBoundedStableBytes(files[i], 1024); // Full current preimages are checked rather than trusting saved digest metadata.
            if (current is null || !current.AsSpan().SequenceEqual(bytes[i])) return false; // Any changed image/config/permission bytes invalidate this owner.
        } // Finish this bounded owner check without renewing authority or an attempt.
        return true; // This proves fixture input continuity only, never Windows enforcement.
    } // Finish this bounded owner check without renewing authority or an attempt.

    public void Dispose() // End launch-input ownership and release every retained resource.
    { // End launch-input ownership and release every retained resource.
        if (disposed) return; disposed = true; // End authority before any handle release.
        try { files[0].Dispose(); } finally { try { files[1].Dispose(); } finally { files[2].Dispose(); } } // Release all handles even when one dispose fails.
    } // Finish this bounded owner check without renewing authority or an attempt.
} // Finish this bounded owner check without renewing authority or an attempt.
