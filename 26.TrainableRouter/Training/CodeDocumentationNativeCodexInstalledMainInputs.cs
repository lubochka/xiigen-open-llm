// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TrainableRouter.Training;

using AgentInfrastructure;
using ContinuousLearning;

/// <summary>Held disk inputs for the installed MAIN loader. Custody does not grant semantic launch permission.</summary>
internal sealed class CodeDocumentationNativeCodexInstalledMainInputs : IDisposable
{
    private readonly CodeDocumentationOneLessonSession session;
    private readonly CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt;
    private readonly CodeDocumentationNativeCodexInstalledResources resources;
    private readonly RetainedDiskInputs disk;
    private readonly byte[] prompt;
    private readonly string[] arguments;
    private CodeDocumentationNativeCodexInstalledConfigWire.Resolution? installedConfiguration; // Retain all five actual app-server config/requirements/profile/readiness responses privately; none is exec permission.
    private RetainedDiskInputs? resolvedConfigurationFiles; // Hold every actual file-bearing origin/layer, including explicit absence.
    private bool configurationAttempted; // The same retained owner never retries a failed or unknown configuration operation.
    private CodeDocumentationNativeCodexWindowsSecurityEnvironment? resourceProtection;
    private readonly ProtectedProbeOutcome protectedProbe = new(); // Complete normal or original failed outcome remains private under this same owner.
    private bool disposed, invalidated;

    private CodeDocumentationNativeCodexInstalledMainInputs(CodeDocumentationOneLessonSession session,
        CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt,
        CodeDocumentationNativeCodexInstalledResources resources, RetainedDiskInputs disk, byte[] prompt, string[] arguments)
    {
        this.session = session; this.receipt = receipt; this.resources = resources;
        this.disk = disk; this.prompt = prompt; this.arguments = arguments;
    }

    internal static OperationResult<CodeDocumentationNativeCodexInstalledMainInputs> OpenUnderHeldSession(
        CodeDocumentationOneLessonSession session, CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt receipt,
        CodeDocumentationNativeCodexInstalledResources resources, CancellationToken token)
    {
        if (!receipt.IsActiveFor(session) || !resources.IsCurrentFor(session, receipt))
            return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexInstalledMainInputs>();
        RetainedDiskInputs? pending = null; byte[]? prompt = null; Exception? primary = null;
        try
        {
            // These are the documented fixed disk producers, not caller-selected configuration paths.
            // They are a custody increment: external instruction files, managed layers and effective MCP
            // composition still require the installed loader's own resolution before semantic admission.
            var home = resources.ResolveCurrentCodexHome();
            var root = receipt.OwnedDirectory;
            var requestPath = Path.Combine(root, CodeDocumentationNativeCodexSelectionWire.RequestFileName);
            var schemaPath = Path.Combine(root, CodeDocumentationNativeCodexSelectionWire.SchemaFileName);
            var inputs = new List<DiskInput>
            {
                new(requestPath, true, true, CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes),
                new(schemaPath, true, true, CodeDocumentationNativeCodexSelectionWire.FamilyMaximumBytes),
                new(Path.Combine(home, "config.toml"), false, false, RetainedDiskInputs.DiskFileMaximumBytes),
                new(Path.Combine(home, "AGENTS.override.md"), false, false, RetainedDiskInputs.DiskFileMaximumBytes),
                new(Path.Combine(home, "AGENTS.md"), false, false, RetainedDiskInputs.DiskFileMaximumBytes)
            };
            // Preserve every ancestor candidate and explicit absence; no guessed Git-root cutoff or
            // project_doc_max_bytes truncation can conceal a later instruction/configuration source.
            for (var directory = new DirectoryInfo(root); directory is not null; directory = directory.Parent)
            {
                token.ThrowIfCancellationRequested();
                if (inputs.Count > 192) throw new IOException("MAIN input ancestry exceeds its finite custody bound.");
                inputs.Add(new(Path.Combine(directory.FullName, "AGENTS.override.md"), false, false, RetainedDiskInputs.DiskFileMaximumBytes));
                inputs.Add(new(Path.Combine(directory.FullName, "AGENTS.md"), false, false, RetainedDiskInputs.DiskFileMaximumBytes));
                inputs.Add(new(Path.Combine(directory.FullName, ".codex", "config.toml"), false, false, RetainedDiskInputs.DiskFileMaximumBytes));
            }
            pending = RetainedDiskInputs.Open(inputs, token);
            var request = pending.CopyBytes(requestPath);
            var decoded = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(request);
            if (!decoded.IsSuccess || decoded.Value is null) return decoded.MapError<CodeDocumentationNativeCodexInstalledMainInputs>();
            var data = decoded.Value;
            if (data.SessionId != receipt.SessionId || !pending.CopyBytes(schemaPath).AsSpan().SequenceEqual(data.CopyOutputSchemaBytes()) ||
                !data.CopyPacketBytes().AsSpan().SequenceEqual(receipt.Context.CopyCanonicalBytes()) ||
                !data.MatchesApprovalBytes(receipt.CopyApprovalBytes()))
                return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexInstalledMainInputs>();
            prompt = data.RenderCompletePromptBytes(CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes);
            var currentPrompt = CodeDocumentationNativeCodexSelectionWire.Utf8.GetBytes(
                CodeDocumentationNativeCodexSelectionHost.RenderCompletePrompt(receipt.Context, data.Nonce));
            try
            {
                if (!prompt.AsSpan().SequenceEqual(currentPrompt))
                    return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexInstalledMainInputs>();
            }
            finally { Array.Clear(currentPrompt); }
            var stdin = CodeDocumentationNativeCodexSelectionWire.RenderSelectionStdin(request, pending.CopyBytes(schemaPath)); // Derive the exact saved-request digest only after original Session/schema/packet/approval/base checks.
            if (!stdin.IsSuccess || stdin.Value is null) return stdin.MapError<CodeDocumentationNativeCodexInstalledMainInputs>(); // Full derived input overage refuses before semantic Started or process creation.
            Array.Clear(prompt); prompt = stdin.Value; // Replace and clear the old temporary base; this original owner retains only complete immutable derived stdin.
            // Actual 0.162 exec help establishes these controls. This vector is held data only;
            // ignore-user-config excludes only config.toml and cannot disable AGENTS/managed MCP.
            string[] arguments = ["exec", "--ignore-user-config", "--strict-config", "--ephemeral", "--json",
                "--model", "gpt-6-astra", "--config", "model_reasoning_effort=\"ultra\"",
                "--cd", root, "--output-schema", schemaPath, "-"];
            var owner = new CodeDocumentationNativeCodexInstalledMainInputs(session, receipt, resources, pending, prompt, arguments);
            if (!owner.IsCurrentFor(session)) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexInstalledMainInputs>();
            pending = null; prompt = null;
            return OperationResult<CodeDocumentationNativeCodexInstalledMainInputs>.Ok(owner);
        }
        catch (CodeDocumentationNativeCodexSelectionWire.PreparationCapacityException error)
        { primary = error; return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexInstalledMainInputs>(true); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        { primary = error; return OperationResult<CodeDocumentationNativeCodexInstalledMainInputs>.Fail("required-owner=main-Codex-instruction-config-MCP-envelope; MAIN disk input custody failed: " + error.GetType().Name + ":" + error.HResult,
            CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable); }
        catch (Exception error) { primary = error; throw; }
        finally
        {
            try { pending?.Dispose(); }
            catch (Exception cleanup) when (primary is not null)
            { primary.Data["NativeMainInputCleanup"] = cleanup.GetType().Name + ":" + cleanup.HResult; }
            finally { if (prompt is not null) Array.Clear(prompt); }
        }
    }

    internal bool IsCurrentFor(CodeDocumentationOneLessonSession expected)
    {
        if (disposed || invalidated || !ReferenceEquals(session, expected)) return false;
        try
        {
            if (receipt.IsActiveFor(session) && resources.IsCurrentFor(session, receipt) && disk.IsCurrent() && resolvedConfigurationFiles?.IsCurrent() != false && resourceProtection?.IsCurrent() != false) return true; // The actual resolved source files remain subordinate to the original same-Session owner.
        }
        catch { invalidated = true; throw; }
        invalidated = true; return false;
    }

    internal OperationResult<CodeDocumentationNativeCodexLaunchInputOwner> ConsumeBeforeSemanticAttempt(
        CodeDocumentationOneLessonSession expected, CancellationToken token = default) // Preserve the caller token through real configuration consumption and independent native cleanup.
    {
        if (!IsCurrentFor(expected)) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>();
        token.ThrowIfCancellationRequested(); // Cancellation before the fixed configuration operation causes no new process.
        if (configurationAttempted) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(); // Unknown/failed results cannot renew this original data owner.
        configurationAttempted = true; // Charge this single genuine resource operation before entering the existing Job owner.
        var read = CodeDocumentationNativeCodexWindowsProcess.ReadInstalledConfigurationAsync(resources, this, token).GetAwaiter().GetResult(); // Consume the exact installed image under the same retained receipt/environment and MAIN files.
        var pendingResolution = read.Resolution; RetainedDiskInputs? pendingFiles = null; Exception? primary = null; // Hold all returned private data/files until final currentness permits transfer.
        CodeDocumentationNativeCodexInstalledConfigWire.Resolution.ExecutionPrerequisiteData prerequisiteData = default; // Closed descriptive categories are data only, never an installed permission owner.
        try // A matched response alone cannot override failed process/closure or changed local sources.
        { // One process sends initialize/initialized/config/read, configRequirements/read, permissionProfile/list and windowsSandbox/readiness; no teaching material, hook or model request.
            if (pendingResolution is null) return OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Fail( // Preserve the concrete resource failure without printing private stdout/stderr or configuration values.
                "required-owner=main-Codex-instruction-config-MCP-envelope; installed-app-server-config-read-failed; exit=" + read.Process.ExitCode + "; tree-closed=" + read.Process.TreeClosed + "; streams-terminal=" + read.Process.StreamTasksTerminal + "; failure=" + read.Process.Failure,
                CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable); // Failure, nonzero and unknown cleanup never create semantic permission.
            if (!StringComparer.OrdinalIgnoreCase.Equals(pendingResolution.CodexHome, resources.ResolveCurrentCodexHome())) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(); // The actual initialized home must match the same retained environment, without copying auth.
            var paths = pendingResolution.ConfigurationPaths; // Retain all supported file origins/layers; virtual layers remain in the complete private response.
            if (paths.Length != 0) pendingFiles = RetainedDiskInputs.Open(paths.Select(path => new DiskInput(path, false, false, RetainedDiskInputs.DiskFileMaximumBytes)), token, disk.Count, disk.TotalBytes); // Account combined old/new physical custody within the unchanged MAIN bounds.
            if (!IsCurrentForResources(resources) || pendingFiles?.IsCurrent() == false) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(); // Reject old-input drift, appeared/missing sources and foreign owners before transfer.
            token.ThrowIfCancellationRequested(); prerequisiteData = pendingResolution.InterpretExecutionPrerequisites(token); // Consume only retained requirements/readiness after actual process/drain/home/file custody validation.
            token.ThrowIfCancellationRequested(); if (!IsCurrentForResources(resources) || pendingFiles?.IsCurrent() == false) return CodeDocumentationNativeCodexSelectionWire.Refuse<CodeDocumentationNativeCodexLaunchInputOwner>(); // A classification cannot renew currentness, the caller or any admission authority.
            installedConfiguration = pendingResolution; resolvedConfigurationFiles = pendingFiles; pendingResolution = null; pendingFiles = null; // Transfer whole private resolution and actual source handles under this original owner only.
        } // Successful execution consumes installed configuration/prerequisite data only, not exec --ignore-user-config equivalence, policy interpretation or MAIN confinement.
        catch (Exception error) { primary = error; throw; } // Preserve the actual custody/protocol/OCE primary through pending ownership cleanup.
        finally // Release every unaccepted file/data owner and clear raw process output without printing values.
        { // Cleanup failures cannot replace an earlier error or skip remaining private-buffer disposal.
            try { pendingFiles?.Dispose(); } catch (Exception cleanup) when (primary is not null) { primary.Data["NativeResolvedConfigurationCleanup"] = cleanup.GetType().Name + ":" + cleanup.HResult; } // Retain a bounded actual secondary cause on the original error.
            finally { pendingResolution?.Dispose(); Array.Clear(read.Process.StandardOutput); Array.Clear(read.Process.StandardError); } // Retain only the transferred private response and clear redundant raw stream bytes.
        } // No successful or failed resolution mints a launch binding, Started or selected task.
        try
        {
            token.ThrowIfCancellationRequested();
            resourceProtection = CodeDocumentationNativeCodexWindowsSecurityEnvironment.Open(this, resources, token);
            var protectedVersion = CodeDocumentationNativeCodexWindowsProcess.ProbeProtectedVersionAsync(resources, this, resourceProtection, token).GetAwaiter().GetResult();
            protectedProbe.CaptureNormal(protectedVersion);
            if (!IsCurrentFor(expected) || !protectedVersion.TreeClosed || !protectedVersion.StreamTasksTerminal || protectedVersion.Failure is not null ||
                protectedVersion.ExitCode != 0 || protectedVersion.StandardError.Length != 0 || protectedVersion.StandardOutput.Length == 0)
                return OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Fail("required-owner=main-Codex-instruction-config-MCP-envelope; protected-RESOURCE-VERSION-failed; exit=" + protectedVersion.ExitCode +
                    "; tree-closed=" + protectedVersion.TreeClosed + "; streams-terminal=" + protectedVersion.StreamTasksTerminal + "; failure=" + protectedVersion.Failure,
                    CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable);
        }
        catch (CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal refusal)
        { ReleaseProtectionAfterFailure(refusal); return protectedProbe.CaptureRefusalAndMap(refusal); } // Capture the exact exception and all process/cleanup Data before the original PermissionUnavailable mapping.
        catch (Exception error) { ReleaseProtectionAfterFailure(error); protectedProbe.CaptureFailure(error); throw; } // Retain the original error/OCE reference and token, then preserve the original rethrow.
        return OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Fail(
            "required-owner=main-Codex-instruction-config-MCP-envelope; held-main-disk-inputs=" + disk.Count +
            "; held-main-input-bytes=" + disk.TotalBytes + "; held-complete-stdin-bytes=" + prompt.Length +
            "; future-exec-argument-count=" + arguments.Length +
            "; installed-app-server-config=resolved-private; installed-prerequisites=whole-private-data-only; resolved-config-file-count=" + (resolvedConfigurationFiles?.Count ?? 0) +
            "; execution-prerequisite-subset=" + prerequisiteData.Category + "; prerequisite-data-issues=" + prerequisiteData.Issues + "; protected-RESOURCE-VERSION=consumed-private; opaque-policy-config-profile-fields=unresolved" +
            "; remaining=AGENTS-MCP-permission-enforcement-and-protected-MAIN-consumption; app-server-config-is-not-exec-ignore-user-config",
            CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable);
    }

    private void ReleaseProtectionAfterFailure(Exception primary)
    {
        var owned = resourceProtection; resourceProtection = null;
        try { owned?.Dispose(); }
        catch (Exception cleanup) { primary.Data["NativeResourceProtectionCleanup"] = cleanup.GetType().Name + ":" + cleanup.HResult; }
    }

    internal bool IsCurrentForResources(CodeDocumentationNativeCodexInstalledResources expected) // Fixed process consumer requires the same actual resource object and its original Session inputs.
        => ReferenceEquals(resources, expected) && IsCurrentFor(session); // A foreign resource owner or detached response cannot enter the private configuration operation.

    public void Dispose()
    {
        if (disposed) return; disposed = true;
        Exception? first = null; // End owner validity first and attempt every actual private resource cleanup independently.
        foreach (var owned in new IDisposable?[] { resourceProtection, protectedProbe, resolvedConfigurationFiles, installedConfiguration, disk }) // Release environment, detach protected outcome custody, then every original private input.
        { try { owned?.Dispose(); } catch (Exception error) { first ??= error; } } // Retain the original cleanup cause while continuing all remaining ownership releases.
        Array.Clear(prompt); Array.Clear(arguments); // Clear held prompt/vector copies without changing any real configuration or auth files.
        if (first is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(first).Throw(); // A cleanup-only failure remains an actual failure rather than hidden success.
    }

    /// <summary>Private owned outcome data. This cannot construct MainInputs, renew currentness or issue any permission.</summary>
    internal sealed class ProtectedProbeOutcome : IDisposable
    {
        internal enum OutcomeState { Empty, Normal, Refusal, Canceled, Failure, Disposed }
        internal OutcomeState State { get; private set; }
        internal CodeDocumentationNativeCodexWindowsProcess.Observation? Normal { get; private set; }
        internal Exception? OriginalFailure { get; private set; } // Hold the original object, including its entire Data; never copy/project or recreate it.

        internal void CaptureNormal(CodeDocumentationNativeCodexWindowsProcess.Observation observation)
        {
            if (State != OutcomeState.Empty) throw new InvalidOperationException("Protected outcome custody cannot repeat or renew.");
            Normal = observation; State = OutcomeState.Normal;
        }
        internal void CaptureFailure(Exception original)
        {
            if (State is OutcomeState.Disposed or OutcomeState.Refusal or OutcomeState.Canceled or OutcomeState.Failure)
                throw new InvalidOperationException("Protected failed-outcome custody cannot repeat or renew.");
            OriginalFailure = original;
            State = original is CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal ? OutcomeState.Refusal :
                original is OperationCanceledException ? OutcomeState.Canceled : OutcomeState.Failure;
        }
        internal OperationResult<CodeDocumentationNativeCodexLaunchInputOwner> CaptureRefusalAndMap(CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal original)
        {
            CaptureFailure(original);
            return OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Fail("required-owner=main-Codex-instruction-config-MCP-envelope; protected-RESOURCE-unavailable; " + original.Message,
                CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable);
        }
        public void Dispose()
        {
            if (State == OutcomeState.Disposed) return; State = OutcomeState.Disposed;
            var observation = Normal; Normal = null; OriginalFailure = null; // Detach custody without mutating an outward original exception's Data or token.
            if (observation is not null) { Array.Clear(observation.StandardOutput); Array.Clear(observation.StandardError); }
        }
    }

    internal sealed record DiskInput(string Path, bool Required, bool SessionCustody, int MaximumBytes);

    /// <summary>Data-only retained files shared with fixtures. This type cannot issue an installed owner or binding.</summary>
    internal sealed class RetainedDiskInputs : IDisposable
    {
        internal const int DiskFileMaximumBytes = 1024 * 1024;
        private const int TotalMaximumBytes = 8 * DiskFileMaximumBytes;
        private readonly DiskInput[] inputs;
        private readonly FileStream?[] streams;
        private readonly byte[][] bytes;
        private readonly DateTime[] modified;
        private bool disposed, invalidated;
        internal int Count => inputs.Length;
        internal int TotalBytes => bytes.Sum(value => value.Length);

        private RetainedDiskInputs(DiskInput[] inputs)
        { this.inputs = inputs; streams = new FileStream?[inputs.Length]; bytes = new byte[inputs.Length][]; modified = new DateTime[inputs.Length]; }

        internal static RetainedDiskInputs Open(IEnumerable<DiskInput> source, CancellationToken token, int existingCount = 0, int existingBytes = 0, Action? afterFirstAcquisitionForTesting = null) // The optional deny-only fixture can cancel after the actual first handle; production supplies none.
        {
            var inputs = source.Select(input => input with { Path = Path.GetFullPath(input.Path) })
                .DistinctBy(input => input.Path, StringComparer.OrdinalIgnoreCase).ToArray();
            if (existingCount is < 0 or > 195 || existingBytes is < 0 or > TotalMaximumBytes || inputs.Length < 1 || inputs.Length + existingCount > 195 || inputs.Any(input => input.MaximumBytes is < 1 or > DiskFileMaximumBytes)) // Reject aggregate overcapacity without increasing the existing supported custody bounds.
                throw new IOException("MAIN disk inputs exceed their finite custody bounds.");
            var pending = new RetainedDiskInputs(inputs);
            try
            {
                var total = existingBytes; // Include already held physical source bytes before any resolved-source allocation or transfer.
                for (var i = 0; i < inputs.Length; i++)
                {
                    token.ThrowIfCancellationRequested(); var input = inputs[i];
                    if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(input.Path))
                        throw new IOException("MAIN disk input path is redirected.");
                    if (!Exists(input.Path))
                    {
                        if (input.Required) throw new IOException("A required MAIN input is absent.");
                        pending.bytes[i] = []; continue;
                    }
                    var stream = input.SessionCustody
                        ? SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(input.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.SequentialScan)
                        : new FileStream(input.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                    pending.streams[i] = stream;
                    if (i == 0) afterFirstAcquisitionForTesting?.Invoke(); token.ThrowIfCancellationRequested(); // Cancellation after actual handle acquisition must still release that original pending handle.
                    pending.modified[i] = File.GetLastWriteTimeUtc(input.Path);
                    if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, input.Path) ||
                        (input.SessionCustody && !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(input.Path)) ||
                        stream.Length < 0 || stream.Length > input.MaximumBytes)
                        throw new IOException("A MAIN disk input failed physical custody admission.");
                    // Empty optional config/instruction files are complete inputs, unlike required prepared data.
                    byte[] snapshot;
                    if (stream.Length == 0)
                    {
                        stream.Position = 0; if (stream.ReadByte() != -1 || stream.Length != 0) throw new IOException("A MAIN input changed during its empty read.");
                        stream.Position = 0; if (stream.ReadByte() != -1 || stream.Length != 0) throw new IOException("A MAIN input changed during its second empty read.");
                        if (input.Required) throw new IOException("A required MAIN input is empty.");
                        snapshot = [];
                    }
                    else snapshot = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, input.MaximumBytes)
                        ?? throw new IOException("A MAIN disk input is unstable or incomplete.");
                    pending.bytes[i] = snapshot; total = checked(total + snapshot.Length);
                    if (total > TotalMaximumBytes) throw new IOException("MAIN disk inputs exceed their complete aggregate custody bound.");
                }
                if (!pending.IsCurrent()) throw new IOException("MAIN disk inputs changed before custody transfer.");
                return pending;
            }
            catch (Exception primary)
            {
                try { pending.Dispose(); }
                catch (Exception cleanup) { primary.Data["NativeMainDiskInputCleanup"] = cleanup.GetType().Name + ":" + cleanup.HResult; }
                throw;
            }
        }

        internal byte[] CopyBytes(string path)
        {
            if (!IsCurrent()) throw new IOException("MAIN disk inputs are no longer current.");
            var index = Array.FindIndex(inputs, input => StringComparer.OrdinalIgnoreCase.Equals(input.Path, Path.GetFullPath(path)));
            if (index < 0 || streams[index] is null) throw new IOException("The requested MAIN input is not held.");
            return bytes[index].ToArray();
        }

        internal bool IsCurrent()
        {
            if (disposed || invalidated) return false;
            try
            {
                for (var i = 0; i < inputs.Length; i++)
                {
                    var input = inputs[i]; var stream = streams[i];
                    if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(input.Path) ||
                        (stream is null ? Exists(input.Path) : !Exists(input.Path))) { invalidated = true; return false; }
                    if (stream is null) continue;
                    if (stream.SafeFileHandle.IsClosed || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, input.Path) ||
                        (input.SessionCustody && !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(input.Path)) ||
                        stream.Length != bytes[i].Length || File.GetLastWriteTimeUtc(input.Path) != modified[i]) { invalidated = true; return false; }
                    byte[]? current;
                    if (stream.Length == 0)
                    {
                        stream.Position = 0; var first = stream.ReadByte(); stream.Position = 0; var second = stream.ReadByte();
                        current = first == -1 && second == -1 && stream.Length == 0 ? [] : null;
                    }
                    else current = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, input.MaximumBytes);
                    try
                    {
                        if (current is null || !current.AsSpan().SequenceEqual(bytes[i])) { invalidated = true; return false; }
                    }
                    finally { if (current is not null) Array.Clear(current); }
                }
                return true;
            }
            catch { invalidated = true; throw; }
        }

        private static bool Exists(string path)
        {
            try
            {
                var attributes = File.GetAttributes(path);
                if ((attributes & FileAttributes.Directory) != 0) throw new IOException("A MAIN input file path is a directory.");
                return true;
            }
            catch (FileNotFoundException) { return false; }
            catch (DirectoryNotFoundException) { return false; }
        }

        public void Dispose()
        {
            if (disposed) return; disposed = true; Exception? first = null;
            for (var i = 0; i < streams.Length; i++)
            {
                try { streams[i]?.Dispose(); } catch (Exception error) { first ??= error; }
                finally { if (bytes[i] is not null) Array.Clear(bytes[i]); }
            }
            if (first is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(first).Throw();
        }
    }
}
