// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TrainableRouter.Training; // Keep installed protocol data inside the existing training boundary.

using System.Runtime.ExceptionServices; // Preserve the first actual protocol, current-input or cancellation error.
using System.Text; // Validate complete UTF8 before accepting a finite protocol frame.
using System.Text.Json; // Consume the installed app-server schema without importing permission authority.

/// <summary>One fixed configuration/prerequisite dialogue. Parsed output is private data, never native permission.</summary>
internal sealed class CodeDocumentationNativeCodexInstalledConfigWire : IDisposable // Share real process dialogue and literal portable transcript tests.
{ // Bound this operation to five fixed responses and the required initialized notification, without semantic requests.
    private const int MaximumBytes = 65536; // Preserve the existing combined diagnostic process output ceiling.
    private static readonly UTF8Encoding Utf8 = new(false, true); // Reject invalid UTF8 rather than replacing private response bytes.
    private readonly TaskCompletionSource<bool> initialized = new(TaskCreationOptions.RunContinuationsAsynchronously); // The matched initialize response gates the next outgoing request.
    private readonly TaskCompletionSource<bool> resolved = new(TaskCreationOptions.RunContinuationsAsynchronously); // The matched config response gates requirements/read.
    private readonly TaskCompletionSource<bool> requirements = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> profiles = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> readiness = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> requirementsRequested = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> profilesRequested = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> readinessRequested = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> requested = new(TaskCreationOptions.RunContinuationsAsynchronously); // Expose only dialogue progress to deterministic stream fixtures.
    private readonly List<byte> pending = new(MaximumBytes); // Keep one finite backing allocation so discarded private prefixes cannot survive a growth copy.
    private readonly string directory; // The real caller supplies only its already held Session working directory.
    private byte[]? initializeBytes, configurationBytes, requirementsBytes, profilesBytes, readinessBytes; // Retain all five whole private responses, including unknown data.
    private ExceptionDispatchInfo? primary; // The original failure wins over later stream cleanup faults.
    private int received, sent, phase, initializeRequestIssued, configurationRequestIssued, requirementsRequestIssued, profilesRequestIssued, readinessRequestIssued; // Each matched response requires its own atomic actual-write witness.
    private bool writing, writerCompleted, complete, disposed; // Success also requires every request write/flush and currentness check to finish.
    internal Task ConfigurationRequestSent => requested.Task; // This data-only witness cannot mint a launch binding.
    internal Task RequirementsRequestSent => requirementsRequested.Task;
    internal Task PermissionProfilesRequestSent => profilesRequested.Task;
    internal Task SandboxReadinessRequestSent => readinessRequested.Task; // Progress signals are data-only and follow full actual writes/flushes.
    internal ExceptionDispatchInfo? Primary => Volatile.Read(ref primary); // Read the single winning writer/stdout error before process cleanup arbitration.

    internal CodeDocumentationNativeCodexInstalledConfigWire(string directory) // This constructor creates protocol data only, not an installed owner.
    { this.directory = Absolute(directory); } // Require the complete normalized held working directory before rendering requests.

    internal async Task WriteAsync(Stream input, Func<bool> current, CancellationToken callerToken, CancellationToken cleanupToken = default) // Preserve the actual caller independently from owned stream cleanup.
    { // Wait for actual response evidence rather than blindly pipelining initialization.
        if (writing || disposed) throw new IOException("The installed configuration dialogue cannot be reused."); // Refuse a second writer before another outgoing request.
        writing = true; // Consume the dialogue once even when the first write fails.
        using var operation = CancellationTokenSource.CreateLinkedTokenSource(callerToken, cleanupToken); // I/O wakes for either cause; this private token is never reported as caller identity.
        var ioToken = operation.Token; // Use linked cancellation only for finite asynchronous transport/waits.
        try // Preserve an actual input, currentness or cancellation error across independent cleanup.
        { // Fixed methods and ids exclude thread, turn, model and tool operations.
            CheckCurrent(current, callerToken); // Revalidate the same held resources and exact caller before the first protocol write.
            await WriteFrame(input, "{\"id\":1,\"method\":\"initialize\",\"params\":{\"clientInfo\":{\"name\":\"xiigen-config-resolution\",\"version\":\"1\"}}}\n", current, callerToken, ioToken, requestId: 1).ConfigureAwait(false); // Negotiate the installed server without experimental tools or model work.
            await initialized.Task.WaitAsync(ioToken).ConfigureAwait(false); // Either cancellation wakes the actual response wait, with caller identity restored below.
            CheckCurrent(current, callerToken); // Input drift or original caller cancellation cannot authorize the next write.
            await WriteFrame(input, "{\"method\":\"initialized\"}\n", current, callerToken, ioToken).ConfigureAwait(false); // Deliver the required notification once under both lifetimes.
            CheckCurrent(current, callerToken); // Caller cancellation during notification flush refuses config/read before its request begins.
            await WriteFrame(input, JsonSerializer.Serialize(new { id = 2, method = "config/read", @params = new { cwd = directory, includeLayers = true } }) + "\n", current, callerToken, ioToken, requestId: 2).ConfigureAwait(false); // Resolve all current layers from this held directory only.
            requested.TrySetResult(true); // Let physical stream fixtures observe the actual config request.
            await resolved.Task.WaitAsync(ioToken).ConfigureAwait(false);
            CheckCurrent(current, callerToken);
            await WriteFrame(input, "{\"id\":3,\"method\":\"configRequirements/read\",\"params\":null}\n", current, callerToken, ioToken, requestId: 3).ConfigureAwait(false);
            requirementsRequested.TrySetResult(true);
            await requirements.Task.WaitAsync(ioToken).ConfigureAwait(false);
            CheckCurrent(current, callerToken);
            await WriteFrame(input, JsonSerializer.Serialize(new { id = 4, method = "permissionProfile/list", @params = new { cwd = directory } }) + "\n", current, callerToken, ioToken, requestId: 4).ConfigureAwait(false); // Default full result set: no cursor, limit or selected profile.
            profilesRequested.TrySetResult(true);
            await profiles.Task.WaitAsync(ioToken).ConfigureAwait(false);
            CheckCurrent(current, callerToken);
            await WriteFrame(input, "{\"id\":5,\"method\":\"windowsSandbox/readiness\",\"params\":null}\n", current, callerToken, ioToken, requestId: 5).ConfigureAwait(false);
            readinessRequested.TrySetResult(true);
            await readiness.Task.WaitAsync(ioToken).ConfigureAwait(false); // Closing stdin follows all five matched responses and complete writes/flushes.
            CheckCurrent(current, callerToken); // A response cannot outlive the actual caller or retained requesting inputs.
            writerCompleted = true; // Parsing a fast response during FlushAsync cannot transfer successful data before the full writer finishes.
        } // The process owner still closes and verifies all streams and descendants independently.
        catch (OperationCanceledException error) when (callerToken.IsCancellationRequested && error.CancellationToken != callerToken) // A linked I/O token must never masquerade as the human caller's token.
        { try { callerToken.ThrowIfCancellationRequested(); } catch (OperationCanceledException callerError) { Fail(callerError); throw; } throw; } // Capture and rethrow an exact original-caller OCE rather than private linked-token cancellation.
        catch (OperationCanceledException) when (cleanupToken.IsCancellationRequested && !callerToken.IsCancellationRequested) { throw; } // Cleanup-only cancellation is joined as secondary and cannot mint protocol primary.
        catch (Exception error) { Fail(error); throw; } // Preserve actual I/O/protocol/current-input or caller-token errors.
    } // Return only after the fixed dialogue completed or its original error was captured.

    private async Task WriteFrame(Stream input, string frame, Func<bool> current, CancellationToken callerToken, CancellationToken ioToken, int requestId = 0) // Charge and guard one fixed complete frame.
    { // The existing prepared-input ceiling also bounds each rendered protocol write.
        var bytes = Utf8.GetBytes(frame); // Strict encoding rejects a malformed path instead of silently replacing it.
        try // Do not issue either response witness before rendering, bounds and actual caller/currentness admission.
        { // These are the only six fixed frames in this operation.
            sent = checked(sent + bytes.Length); if (sent > CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes) throw new IOException("Installed configuration requests exceed the finite input bound."); // Charge the whole frame before retaining any issuance witness.
            CheckCurrent(current, callerToken); ioToken.ThrowIfCancellationRequested(); // Guard immediately before actual first write, even when current() was blocked.
            if (requestId == 1) Volatile.Write(ref initializeRequestIssued, 1); else if (requestId == 2) Volatile.Write(ref configurationRequestIssued, 1); // A legitimate fast response during the actual Write/Flush is correlated, not unsolicited.
            else if (requestId == 3) Volatile.Write(ref requirementsRequestIssued, 1);
            else if (requestId == 4) Volatile.Write(ref profilesRequestIssued, 1);
            else if (requestId == 5) Volatile.Write(ref readinessRequestIssued, 1);
            await input.WriteAsync(bytes, ioToken).ConfigureAwait(false); // Linked transport cancellation is normalized to caller identity only in its owning catch.
            callerToken.ThrowIfCancellationRequested(); Primary?.Throw(); // No flush or later frame follows a known earlier failure/caller cancellation.
            await input.FlushAsync(ioToken).ConfigureAwait(false); callerToken.ThrowIfCancellationRequested(); // Check the actual caller again after a possibly suspending flush.
        } // Success still requires complete write/flush, not merely an issued witness.
        finally { Array.Clear(bytes); } // Clear the temporary complete request buffer on every exit.
    } // No user-supplied method, id or additional protocol call reaches this writer.

    internal void Accept(ReadOnlySpan<byte> bytes) // The real stdout drain and literal fixtures share identical full-byte parsing.
    { // Reject any extra response/notification rather than treating protocol output as a capability.
        try // Capture the first parsing failure for the writer and native cleanup arbitration.
        { // Enforce allocation bounds before retaining another private byte.
            if (disposed || complete || primary is not null) throw new IOException("Installed configuration output is no longer consumable."); // EOF, disposal and failure never renew the dialogue.
            received = checked(received + bytes.Length); if (received > MaximumBytes) throw new IOException("Installed configuration output exceeds the finite bound."); // Charge every observed stdout byte, including separators and malformed suffixes.
            foreach (var value in bytes) // A multibyte UTF8 sequence can cross reads but cannot cross its actual complete line.
            { // Retain a whole frame until the actual LF before validating UTF8 and JSON.
                if (value != 10) { pending.Add(value); continue; } // No prefix is parsed before its line terminator.
                var frame = pending.ToArray(); for (var i = 0; i < pending.Count; i++) pending[i] = 0; pending.Clear(); // Clear actual used backing slots before discarding the complete private prefix.
                try { ParseFrame(frame); } finally { Array.Clear(frame); } // Parsed private bytes are retained only by defensive response copies.
            } // Any later bytes remain charged and are checked by FinishOutput at real EOF.
        } // Invalid output faults the actual stdout reader and unblocks any waiting stdin writer.
        catch (Exception error) { Fail(error); throw; } // Keep the same original failure object through both task paths.
    } // Output remains private protocol data rather than permission or model-selection evidence.

    private void ParseFrame(byte[] frame) // Validate the actual installed response envelope and all source identities.
    { // The installed JSONRPCResponse schema uses id/result, with no method-bearing notifications here.
        if (!writing || frame.Length == 0 || phase >= 5) throw new IOException("Installed configuration output contains an unsolicited, extra or empty frame."); // A real actual-write witness and exactly five correlated responses are required.
        _ = Utf8.GetString(frame); using var json = JsonDocument.Parse(frame); // Validate all UTF8 and the complete JSON representation without printing values.
        var root = json.RootElement; RejectDuplicates(root); // Duplicate keys anywhere cannot silently override private config or source identities.
        RequireObject(root, "id", "result"); // Error, method, unrelated or extra envelope fields fail before accepting a result.
        if (!root.GetProperty("id").TryGetInt32(out var id) || id != phase + 1) throw new IOException("Installed configuration response id is unexpected."); // String ids, duplicates and reordered results refuse.
        var result = root.GetProperty("result"); // The result stays whole even when only its required shape is interpreted.
        if (phase == 0) // The initialize response is required before initialized/config/read.
        { // Use the installed InitializeResponse's four mandatory fields without fabricating server telemetry.
            if (Volatile.Read(ref initializeRequestIssued) != 1) throw new IOException("Installed initialization response preceded its actual request."); // writing=true or a blocked currentness callback cannot admit unsolicited id1.
            RequireObject(result, "codexHome", "platformFamily", "platformOs", "userAgent"); // Missing/null/extra initialization fields reject the configuration operation.
            _ = Absolute(String(result, "codexHome")); _ = String(result, "platformFamily"); _ = String(result, "platformOs"); _ = String(result, "userAgent"); // Retain and validate actual server identity data without granting authority.
            initializeBytes = frame.ToArray(); phase = 1; initialized.TrySetResult(true); // Only the complete matched id 1 advances the outgoing dialogue.
        } // No model or tool request is issued by parsing initialization.
        else if (phase == 1) // Retain complete config/origins/layers, including unknown keys in config values.
        { // includeLayers=true requires the actual complete returned layer inventory.
            if (Volatile.Read(ref configurationRequestIssued) != 1) throw new IOException("Installed configuration response preceded its request."); // Unsolicited id 2 cannot become a resolved input owner.
            RequireObject(result, "config", "origins", "layers"); // An omitted/null layer inventory cannot masquerade as complete resolution.
            if (result.GetProperty("config").ValueKind != JsonValueKind.Object || result.GetProperty("origins").ValueKind != JsonValueKind.Object || result.GetProperty("layers").ValueKind != JsonValueKind.Array) throw new IOException("Installed configuration resolution is incomplete."); // Require whole typed owners rather than a summary or null placeholder.
            _ = ReadSources(result); // Validate every origin and layer source before retaining the private complete response.
            configurationBytes = frame.ToArray(); phase = 2; resolved.TrySetResult(true);
        } // Configuration metadata never imports native launch or selected-task authority.
        else if (phase == 2)
        {
            if (Volatile.Read(ref requirementsRequestIssued) != 1) throw new IOException("Installed requirements response preceded its actual request.");
            if (result.ValueKind != JsonValueKind.Object || (result.TryGetProperty("requirements", out var policy) && policy.ValueKind is not (JsonValueKind.Null or JsonValueKind.Object))) throw new IOException("Installed requirements result has invalid shape."); // Absence, null and object remain distinct; the entire nested policy is opaque, not semantically validated.
            requirementsBytes = frame.ToArray(); phase = 3; requirements.TrySetResult(true);
        }
        else if (phase == 3)
        {
            if (Volatile.Read(ref profilesRequestIssued) != 1) throw new IOException("Installed permission-profile response preceded its actual request.");
            if (result.ValueKind != JsonValueKind.Object || !result.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) throw new IOException("Installed permission-profile inventory has invalid shape.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in data.EnumerateArray())
            {
                if (!ids.Add(String(row, "id")) || !row.TryGetProperty("allowed", out var allowed) || allowed.ValueKind is not (JsonValueKind.True or JsonValueKind.False) || (row.TryGetProperty("description", out var description) && description.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))) throw new IOException("Installed permission-profile row is incomplete or repeated."); // Open rows retain unknown fields; id/allowed are data, never an enforcement grant.
            }
            if (result.TryGetProperty("nextCursor", out var cursor) && cursor.ValueKind != JsonValueKind.Null) throw new IOException("Installed permission-profile inventory is partial or has invalid cursor shape."); // Refuse nonnull cursors instead of silently accepting a page or adding a retry loop.
            profilesBytes = frame.ToArray(); phase = 4; profiles.TrySetResult(true);
        }
        else
        {
            if (Volatile.Read(ref readinessRequestIssued) != 1) throw new IOException("Installed sandbox readiness response preceded its actual request.");
            if (result.ValueKind != JsonValueKind.Object || String(result, "status") is not ("ready" or "notConfigured" or "updateRequired")) throw new IOException("Installed sandbox readiness result has invalid shape.");
            readinessBytes = frame.ToArray(); phase = 5; readiness.TrySetResult(true); // All three statuses are retained data; no setup, update, model or admin operation follows.
        }
    } // Every complete response remains private until disposal or explicit data-only transfer.

    internal void FinishOutput() // Call only after the stdout stream actually reaches EOF.
    { // EOF cannot repair a missing response or partial final line.
        try { if (disposed || complete || pending.Count != 0 || phase != 5 || primary is not null) throw new IOException("Installed configuration output is partial or incomplete."); complete = true; } // Whole framing and all five exact responses are mandatory.
        catch (Exception error) { Fail(error); throw; } // Wake a waiting writer with the original incomplete-output failure.
    } // A successful parser still requires the process owner's exit/tree/drain checks.

    internal Resolution TakeResolution() // Transfer complete private data only after real EOF and the matched dialogue.
    { // The process caller separately requires successful exit and whole owned cleanup.
        if (!complete || !writerCompleted || disposed || primary is not null || initializeBytes is null || configurationBytes is null || requirementsBytes is null || profilesBytes is null || readinessBytes is null) throw new IOException("Installed configuration resolution is unavailable."); // Parsed prefix, pending flush or failed cleanup cannot request a successful result.
        var value = new Resolution(initializeBytes, configurationBytes, requirementsBytes, profilesBytes, readinessBytes); initializeBytes = null; configurationBytes = null; requirementsBytes = null; profilesBytes = null; readinessBytes = null; return value; // Transfer all five private preimages without exposing a permission constructor.
    } // Retained sources are data until actual filesystem custody is acquired by the existing MAIN owner.

    internal sealed class Resolution : IDisposable // A data-only result can never issue an installed owner or native binding.
    { // Keep unknown configuration fields in the complete private source response.
        private readonly byte[] initialize, configuration, requirements, profiles, readiness; private bool disposed; // End data retention once before clearing all five private preimages.
        internal Resolution(byte[] initialize, byte[] configuration, byte[] requirements, byte[] profiles, byte[] readiness) { this.initialize = initialize; this.configuration = configuration; this.requirements = requirements; this.profiles = profiles; this.readiness = readiness; } // Only the completed dialogue transfers its private arrays.
        internal byte[] CopyInitializeResponse() => !disposed ? initialize.ToArray() : throw new IOException("Installed configuration resolution is disposed.");
        internal byte[] CopyConfigurationResponse() => !disposed ? configuration.ToArray() : throw new IOException("Installed configuration resolution is disposed."); // Tests can inspect a defensive data copy, never a launch capability.
        internal byte[] CopyRequirementsResponse() => !disposed ? requirements.ToArray() : throw new IOException("Installed configuration resolution is disposed.");
        internal byte[] CopyPermissionProfilesResponse() => !disposed ? profiles.ToArray() : throw new IOException("Installed configuration resolution is disposed.");
        internal byte[] CopySandboxReadinessResponse() => !disposed ? readiness.ToArray() : throw new IOException("Installed configuration resolution is disposed.");
        internal string CodexHome { get { if (disposed) throw new IOException("Installed configuration resolution is disposed."); using var json = JsonDocument.Parse(initialize); return Absolute(String(json.RootElement.GetProperty("result"), "codexHome")); } } // Bind the actual initialize home to the retained resource environment.
        internal string[] ConfigurationPaths { get { if (disposed) throw new IOException("Installed configuration resolution is disposed."); using var json = JsonDocument.Parse(configuration); return ReadSources(json.RootElement.GetProperty("result")); } } // Return every supported physical configuration source while retaining all virtual layer data.
        internal enum FieldPresence { Missing, Null, Object, Empty, Values, NonEmpty, Invalid } // Retain absent/null/empty distinctions; none is execution authority.
        [Flags] internal enum DeclaredSandboxModes { None = 0, ReadOnly = 1, WorkspaceWrite = 2, DangerFullAccess = 4 } // Declared requirement labels only, never applied sandbox state.
        [Flags] internal enum DeclaredWindowsImplementations { None = 0, Elevated = 1, Unelevated = 2, Mxc = 4 } // No active implementation is inferred from this inventory.
        internal enum ReadinessData { Invalid, Ready, NotConfigured, UpdateRequired } // Ready describes the installed reply, not mutation-free execution.
        [Flags] internal enum PrerequisiteIssues { None = 0, MissingRequirements = 1, InvalidDeclaredData = 2, ReadOnlyExcluded = 4, WindowsImplementationsExcluded = 8, ManagedInstructionsNeedComposition = 16, SandboxNotConfigured = 32, SandboxUpdateRequired = 64 } // Closed categories contain no private content.
        internal readonly record struct ExecutionPrerequisiteData(FieldPresence Requirements, FieldPresence SandboxModesPresence,
            DeclaredSandboxModes SandboxModes, FieldPresence WindowsImplementationsPresence, DeclaredWindowsImplementations WindowsImplementations,
            FieldPresence DeveloperInstructions, ReadinessData Readiness, PrerequisiteIssues Issues) // Immutable descriptive subset cannot reconstruct Session/receipt/resources or grant a binding.
        {
            internal string Category => Issues == PrerequisiteIssues.None ? "declared-subset-mechanism-required" : "declared-subset-blocked-or-unresolved"; // Unknown other/config/profile fields stay opaque and enforcement remains unresolved even for the compatible subset.
        }
        internal ExecutionPrerequisiteData InterpretExecutionPrerequisites(CancellationToken token = default) // Consume retained complete private data without I/O, RPC, copied secrets or a permission constructor.
        {
            token.ThrowIfCancellationRequested(); if (disposed) throw new IOException("Installed configuration resolution is disposed."); // Preserve the original caller and ended-owner boundary.
            using var policyJson = JsonDocument.Parse(requirements); using var readinessJson = JsonDocument.Parse(readiness); // Documents are temporary/disposed; original private arrays remain retained unchanged.
            RejectDuplicates(policyJson.RootElement); RejectDuplicates(readinessJson.RootElement); // Reuse the existing bounded duplicate guard; unknown private fields cannot override selected data.
            var result = policyJson.RootElement.GetProperty("result"); var sandbox = DeclaredSandboxModes.None; var windows = DeclaredWindowsImplementations.None;
            var requirementsPresence = FieldPresence.Invalid; var sandboxPresence = FieldPresence.Missing; var windowsPresence = FieldPresence.Missing; var instructions = FieldPresence.Missing; var issues = PrerequisiteIssues.None;
            if (result.ValueKind != JsonValueKind.Object) issues |= PrerequisiteIssues.InvalidDeclaredData; // Normally rejected by the unchanged five-response parser, still no implicit compatible classification.
            else if (!result.TryGetProperty("requirements", out var policy)) { requirementsPresence = FieldPresence.Missing; issues |= PrerequisiteIssues.MissingRequirements; }
            else if (policy.ValueKind == JsonValueKind.Null) requirementsPresence = FieldPresence.Null; // No published requirements data is not a permission statement.
            else if (policy.ValueKind != JsonValueKind.Object) issues |= PrerequisiteIssues.InvalidDeclaredData;
            else
            {
                requirementsPresence = FieldPresence.Object;
                sandboxPresence = ReadDeclaredArray(policy, "allowedSandboxModes", false, out var sandboxBits); sandbox = (DeclaredSandboxModes)sandboxBits;
                windowsPresence = ReadDeclaredArray(policy, "allowedWindowsSandboxImplementations", true, out var windowsBits); windows = (DeclaredWindowsImplementations)windowsBits;
                if (sandboxPresence == FieldPresence.Invalid || windowsPresence == FieldPresence.Invalid) issues |= PrerequisiteIssues.InvalidDeclaredData;
                if ((sandboxPresence is FieldPresence.Empty or FieldPresence.Values) && (sandbox & DeclaredSandboxModes.ReadOnly) == 0) issues |= PrerequisiteIssues.ReadOnlyExcluded;
                if (windowsPresence == FieldPresence.Empty) issues |= PrerequisiteIssues.WindowsImplementationsExcluded; // An empty declared inventory excludes all; nonempty still does not identify an active runtime mode.
                if (policy.TryGetProperty("additionalDeveloperInstructions", out var developer))
                {
                    instructions = developer.ValueKind == JsonValueKind.Null ? FieldPresence.Null : developer.ValueKind != JsonValueKind.String ? FieldPresence.Invalid : developer.ValueEquals("") ? FieldPresence.Empty : FieldPresence.NonEmpty; // Never materialize/log the private instruction string.
                    if (instructions == FieldPresence.Invalid) issues |= PrerequisiteIssues.InvalidDeclaredData;
                    if (instructions == FieldPresence.NonEmpty) issues |= PrerequisiteIssues.ManagedInstructionsNeedComposition;
                }
            }
            var readyResult = readinessJson.RootElement.GetProperty("result"); var ready = ReadinessData.Invalid;
            if (readyResult.ValueKind == JsonValueKind.Object && readyResult.TryGetProperty("status", out var status) && status.ValueKind == JsonValueKind.String)
                ready = status.ValueEquals("ready") ? ReadinessData.Ready : status.ValueEquals("notConfigured") ? ReadinessData.NotConfigured : status.ValueEquals("updateRequired") ? ReadinessData.UpdateRequired : ReadinessData.Invalid;
            if (ready == ReadinessData.Invalid) issues |= PrerequisiteIssues.InvalidDeclaredData;
            if (ready == ReadinessData.NotConfigured) issues |= PrerequisiteIssues.SandboxNotConfigured;
            if (ready == ReadinessData.UpdateRequired) issues |= PrerequisiteIssues.SandboxUpdateRequired;
            token.ThrowIfCancellationRequested(); if (disposed) throw new IOException("Installed configuration resolution is disposed."); // Classification cannot outlive this retained data owner or caller.
            return new(requirementsPresence, sandboxPresence, sandbox, windowsPresence, windows, instructions, ready, issues);
        }
        private static FieldPresence ReadDeclaredArray(JsonElement policy, string name, bool windows, out int bits) // Interpret exactly the two declared nullable enum inventories, not a recursive policy/schema engine.
        {
            bits = 0; if (!policy.TryGetProperty(name, out var field)) return FieldPresence.Missing;
            if (field.ValueKind == JsonValueKind.Null) return FieldPresence.Null;
            if (field.ValueKind != JsonValueKind.Array) return FieldPresence.Invalid;
            foreach (var item in field.EnumerateArray())
            {
                var bit = item.ValueKind != JsonValueKind.String ? 0 : windows
                    ? item.ValueEquals("elevated") ? 1 : item.ValueEquals("unelevated") ? 2 : item.ValueEquals("mxc") ? 4 : 0
                    : item.ValueEquals("read-only") ? 1 : item.ValueEquals("workspace-write") ? 2 : item.ValueEquals("danger-full-access") ? 4 : 0;
                if (bit == 0 || (bits & bit) != 0) { bits = 0; return FieldPresence.Invalid; } // Unknown/duplicate/wrongly typed entries cannot silently narrow or allow an inventory.
                bits |= bit;
            }
            return bits == 0 ? FieldPresence.Empty : FieldPresence.Values;
        }
        public void Dispose() { if (disposed) return; disposed = true; Array.Clear(initialize); Array.Clear(configuration); Array.Clear(requirements); Array.Clear(profiles); Array.Clear(readiness); } // No private policy/profile/credential values are emitted or persisted by this result.
    } // Parsing and custody remain separate from permission/enforcement admission.

    private static string[] ReadSources(JsonElement result) // Interpret the complete installed ConfigLayerSource union, not a guessed disk-only allowlist.
    { // Validate every origin and every layer independently, preserving virtual identities in the whole response.
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Repeated references share physical custody without dropping their response identities.
        foreach (var origin in result.GetProperty("origins").EnumerateObject()) { RequireObject(origin.Value, "name", "version"); _ = String(origin.Value, "version"); Source(origin.Value.GetProperty("name"), paths); } // Origins carry exact resolved source metadata.
        foreach (var layer in result.GetProperty("layers").EnumerateArray()) // Whole layers include their private config values, including unknown fields.
        { // Only the optional disabledReason is additional to the three required installed fields.
            RequireObject(layer, ["config", "name", "version"], ["disabledReason"]); _ = String(layer, "version"); // Require complete layer identity while retaining its full unmodified config value.
            if (layer.GetProperty("config").ValueKind != JsonValueKind.Object) throw new IOException("Installed configuration layer is incomplete."); // A null/scalar layer cannot stand in for its complete private configuration.
            if (layer.TryGetProperty("disabledReason", out var reason) && reason.ValueKind is not (JsonValueKind.Null or JsonValueKind.String)) throw new IOException("Installed configuration layer reason has invalid shape."); // A typed optional reason cannot conceal another source inventory.
            Source(layer.GetProperty("name"), paths); // A source present only in layers still receives actual custody.
        } // Source counts stay within the existing finite MAIN file inventory.
        if (paths.Count > 195) throw new IOException("Installed configuration sources exceed the MAIN custody bound."); return paths.ToArray(); // Refuse overcapacity instead of truncating source paths.
    } // Complete source metadata stays inside the retained response rather than being a permission receipt.

    private static void Source(JsonElement source, HashSet<string> paths) // Resolve exact file-bearing source variants from the installed schema.
    { // Unknown source kinds refuse rather than being silently ignored.
        var type = String(source, "type"); // Source fields are metadata, never caller-defined read roots.
        switch (type) // The actual SOURCE67 union supplies these supported source discriminants.
        { // Virtual sources remain fully retained without inventing local file paths.
            case "packagedDefaults": case "system": case "legacyManagedConfigTomlFromFile": RequireObject(source, "type", "file"); paths.Add(ConfigurationPath(String(source, "file"))); break; // Hold the exact package/system/legacy file supplied by the installed loader.
            case "user": RequireObject(source, ["type", "file"], ["profile"]); if (source.TryGetProperty("profile", out var profile) && profile.ValueKind is not (JsonValueKind.Null or JsonValueKind.String)) throw new IOException("Installed configuration profile is invalid."); paths.Add(ConfigurationPath(String(source, "file"))); break; // Retain the full user/profile layer and its actual configuration file.
            case "project": RequireObject(source, "type", "dotCodexFolder"); paths.Add(ConfigurationPath(Path.Combine(Absolute(String(source, "dotCodexFolder")), "config.toml"))); break; // The installed project source names the folder whose config.toml is consumed.
            case "mdm": RequireObject(source, "type", "domain", "key"); _ = String(source, "domain"); _ = String(source, "key"); break; // MDM supplies a virtual identity, not a guessed local path.
            case "enterpriseManaged": RequireObject(source, "type", "id", "name"); _ = String(source, "id"); _ = String(source, "name"); break; // Preserve cloud-managed identity without creating an external lookup or network request.
            case "sessionFlags": case "legacyManagedConfigTomlFromMdm": RequireObject(source, "type"); break; // Preserve the complete virtual layer as data only.
            default: throw new IOException("Installed configuration source kind is unsupported."); // A newer/unrecognized union cannot grant implicit access or complete-resolution claims.
        } // File sources are subsequently reopened under the existing physical custody checks.
    } // No source metadata is treated as a launch permission or confinement certificate.

    private static string ConfigurationPath(string value) // Exclude the documented auth file from configuration-source custody.
    { var path = Absolute(value); if (string.Equals(Path.GetFileName(path), "auth.json", StringComparison.OrdinalIgnoreCase)) throw new IOException("Authentication files are not configuration custody sources."); return path; } // Never snapshot or copy auth bytes even if a malformed source claims them as configuration.
    private static string Absolute(string value) // Normalize only a finite fully qualified native path.
    { if (string.IsNullOrWhiteSpace(value) || value.Length > 32767 || !Path.IsPathFullyQualified(value)) throw new IOException("Installed configuration source path is invalid."); return Path.GetFullPath(value); } // No relative path or implicit current-directory replacement is accepted.
    private static string String(JsonElement value, string name) // Read a required nonempty installed string without including private content in errors.
    { if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(name, out var field) || field.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(field.GetString())) throw new IOException("Installed configuration string field is incomplete."); return field.GetString()!; } // Missing/null/empty identity fields fail closed.
    private static void RequireObject(JsonElement value, params string[] required) => RequireObject(value, required, []); // Closed protocol identities exclude unsolicited fields and messages.
    private static void RequireObject(JsonElement value, string[] required, string[] optional) // Validate mandatory fields and reject unrelated protocol/schema envelope data.
    { if (value.ValueKind != JsonValueKind.Object || required.Any(name => !value.TryGetProperty(name, out _)) || value.EnumerateObject().Any(field => !required.Contains(field.Name) && !optional.Contains(field.Name))) throw new IOException("Installed configuration object has invalid fields."); } // Unknown configuration keys remain allowed inside whole config values, not authority-bearing envelopes.
    private static void RejectDuplicates(JsonElement value) // A repeated key cannot replace a source or a private configuration value silently.
    { if (value.ValueKind == JsonValueKind.Object) { var names = new HashSet<string>(StringComparer.Ordinal); foreach (var field in value.EnumerateObject()) { if (!names.Add(field.Name)) throw new IOException("Installed configuration object contains duplicate fields."); RejectDuplicates(field.Value); } } else if (value.ValueKind == JsonValueKind.Array) foreach (var item in value.EnumerateArray()) RejectDuplicates(item); } // Traverse the complete bounded response, including unknown config subtrees.
    private void CheckCurrent(Func<bool> current, CancellationToken token) // Guard every fixed write with the actual caller and an immutable earlier protocol cause.
    { Primary?.Throw(); token.ThrowIfCancellationRequested(); if (!current()) throw new IOException("Installed configuration inputs changed before protocol consumption."); Primary?.Throw(); token.ThrowIfCancellationRequested(); } // A blocked current() cannot allow an intervening unsolicited response to resume writes.
    internal void ObserveSourceFailure(Exception error) => Fail(error); // Actual stdout/stderr read or budget failures are captured before owned cleanup cancellation.
    private void Fail(Exception error) // Propagate one original error to both independent protocol waiters.
    { var captured = ExceptionDispatchInfo.Capture(error); var winner = Interlocked.CompareExchange(ref primary, captured, null) ?? captured; initialized.TrySetException(winner.SourceException); resolved.TrySetException(winner.SourceException); requirements.TrySetException(winner.SourceException); profiles.TrySetException(winner.SourceException); readiness.TrySetException(winner.SourceException); requested.TrySetException(winner.SourceException); requirementsRequested.TrySetException(winner.SourceException); profilesRequested.TrySetException(winner.SourceException); readinessRequested.TrySetException(winner.SourceException); } // Writer and stdout race to one immutable original primary; later cleanup cannot replace it.
    public void Dispose() // End protocol-data retention without touching configuration, auth or policy files.
    { if (disposed) return; disposed = true; if (initializeBytes is not null) Array.Clear(initializeBytes); if (configurationBytes is not null) Array.Clear(configurationBytes); if (requirementsBytes is not null) Array.Clear(requirementsBytes); if (profilesBytes is not null) Array.Clear(profilesBytes); if (readinessBytes is not null) Array.Clear(readinessBytes); for (var i = 0; i < pending.Count; i++) pending[i] = 0; pending.Clear(); } // Clear complete and partial private buffers on every untransferred exit.
} // This helper owns a fixed data dialogue and has no permission or selected-task factory.
