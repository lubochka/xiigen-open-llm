// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TrainableRouter.Training; // Keep this lifecycle helper private to the repository training boundary.

using System.ComponentModel; // Preserve exact Windows error codes in Win32Exception.
using System.Diagnostics; // Measure finite process and cleanup phase elapsed time.
using System.Runtime.ExceptionServices; // Rethrow the original primary exception after owned cleanup.
using System.Runtime.InteropServices; // Bind exact native Windows Job, pipe and process structures.
using System.Text; // Quote exact literal Windows command arguments.
using Microsoft.Win32.SafeHandles; // Retain native resources with explicit SafeHandle ownership and deterministic closure.
using ContinuousLearning; // Verify exact public-fixture file handles through the existing physical boundary.

/// <summary>One finite Windows process tree. Its output is data, never selection authority.</summary>
internal sealed class CodeDocumentationNativeCodexWindowsProcess // Own one fixed finite Windows process tree; output remains diagnostic data.
{ // Keep the private resource/process boundary inside the repository training owner.
    internal sealed record Observation(uint RootPid, long RootBirthFileTime, uint ExitCode, // Report root identity and separate byte streams without issuing selection authority.
        byte[] StandardOutput, byte[] StandardError, uint TotalProcesses, bool TreeClosed, // Retain exact separate stdout/stderr bytes and independent tree accounting.
        bool DescendantsAfterRoot, string? Failure, bool StreamTasksTerminal = false, string? StreamCleanupFailure = null, // Tree closure and managed-task closure are separate observations.
        bool RootExitObserved = false, bool OutputOverflow = false, string? PriorFailure = null); // DescendantsAfterRoot means unfinished natural whole-Job observation within the original deadline; forced cleanup never clears that refusal.

    private const uint KillOnClose = 0x2000, CreateSuspended = 4, ExtendedStartup = 0x80000, // Keep the fixed native flags and finite resource limits without changing semantic caps.
        UnicodeEnvironment = 0x400, NoWindow = 0x08000000; // Keep the fixed native flags and finite resource limits without changing semantic caps.
    private readonly SafeFileHandle job; // Keep the original Job handle as the lifetime boundary for every descendant.
    private SafeFileHandle? process, thread; // Retain root and suspended-thread handles until explicit cleanup.
    private bool used; // Prevent a second process effect from the same tree owner.
    private bool jobAssigned; // A returned suspended root still needs exact cleanup if a protected OS create did not honor JOB_LIST.

    private CodeDocumentationNativeCodexWindowsProcess() // Create the fixed Windows Job boundary before any child process exists.
    { // Create the fixed Windows Job boundary before any child process exists.
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Native Codex process ownership requires Windows."); // Reject unsupported platforms before native allocation or process effects.
        job = new SafeFileHandle(CheckHandle(CreateJobObjectW(IntPtr.Zero, null), "CreateJobObjectW"), true); // Acquire the Job handle before any suspended process can be created.
        try // Create the fixed Windows Job boundary before any child process exists.
        { // Create the fixed Windows Job boundary before any child process exists.
            var limits = new ExtendedLimits { Basic = new BasicLimits { Flags = KillOnClose } }; // Request kill-on-close Job ownership without breakaway permission.
            Check(SetInformationJobObject(job, 9, ref limits, (uint)Marshal.SizeOf<ExtendedLimits>()), "SetInformationJobObject"); // Apply the fixed Job lifetime limits and preserve any exact Windows error.
            Check(QueryLimits(job, 9, out var observed, (uint)Marshal.SizeOf<ExtendedLimits>(), out var length), "QueryInformationJobObject limits"); // Read back the actual Job limits before allowing process creation.
            if (length != Marshal.SizeOf<ExtendedLimits>() || observed.Basic.Flags != KillOnClose) // Reject a Job whose observed limits differ from the fixed owner contract.
                throw new IOException("Unexpected Job limits; breakaway is not admitted."); // Refuse unverified Job limits instead of admitting descendant escape.
        } // Create the fixed Windows Job boundary before any child process exists.
        catch { job.Dispose(); throw; } // Dispose the acquired Job when initialization fails before process creation.
    } // Create the fixed Windows Job boundary before any child process exists.

    // A retained fixed installed-resource owner chooses the vector. No HTTP/DI argument vector enters here.
    internal static async Task<Observation> ProbeInstalledAsync(CodeDocumentationNativeCodexInstalledResources resources, // Probe only the retained installed image and fixed resource vector.
        CodeDocumentationNativeCodexInstalledResources.Probe probe, CancellationToken token) // Probe only the retained installed image and fixed resource vector.
    { // Probe only the retained installed image and fixed resource vector.
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess()); // Keep this fixed probe/canary under the production Job/process handle lifetime.
        return await lifetime.Owner.RunAsync(resources.ImagePath, resources.ProbeArguments(probe), // Run exactly one retained resource or fixed harmless canary under the same owner.
            resources.WorkingDirectory, Array.Empty<byte>(), 65536, TimeSpan.FromSeconds(20), // Use the held Session directory, empty stdin and existing finite diagnostic bounds.
            () => resources.IsCurrent(), resources.PinCurrentEnvironment, token).ConfigureAwait(false); // Only the genuine retained owner supplies the complete explicit environment and current checks.
    } // Probe only the retained installed image and fixed resource vector.

    internal static async Task<Observation> ProbeProtectedVersionAsync(CodeDocumentationNativeCodexInstalledResources resources,
        CodeDocumentationNativeCodexInstalledMainInputs inputs, CodeDocumentationNativeCodexWindowsSecurityEnvironment protection, CancellationToken token)
    {
        if (!protection.IsCurrentFor(inputs, resources) || !inputs.IsCurrentForResources(resources))
            throw new IOException("Protected VERSION requires its original retained inputs and security environment.");
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess());
        return await lifetime.Owner.RunAsync(resources.ImagePath, resources.ProbeArguments(CodeDocumentationNativeCodexInstalledResources.Probe.Version),
            resources.WorkingDirectory, [], 65536, TimeSpan.FromSeconds(20),
            () => protection.IsCurrentFor(inputs, resources) && inputs.IsCurrentForResources(resources), resources.PinCurrentEnvironment,
            token, protection: protection).ConfigureAwait(false);
    }

    internal sealed record ConfigurationRead(Observation Process, CodeDocumentationNativeCodexInstalledConfigWire.Resolution? Resolution); // A successful process response remains private data, never semantic authority.
    internal static async Task<ConfigurationRead> ReadInstalledConfigurationAsync(CodeDocumentationNativeCodexInstalledResources resources, // Use the exact installed image already held by the requesting Session.
        CodeDocumentationNativeCodexInstalledMainInputs inputs, CancellationToken token) // Caller/HTTP cannot supply an executable, method, argument vector or environment.
    { // Reuse the existing single-use suspended Job, exact environment and independent cleanup owner.
        using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(resources.WorkingDirectory); // Render only the fixed configuration dialogue under the held working root.
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess()); // Acquire this finite Job before the actual configuration process exists.
        var observation = await lifetime.Owner.RunAsync(resources.ImagePath, ["app-server", "--stdio", "--strict-config"], // SOURCE67 installed help supports this fixed local stdio interface.
            resources.WorkingDirectory, [], 65536, TimeSpan.FromSeconds(20), // Preserve existing diagnostic input/output/time ceilings; never send the teaching prompt.
            () => inputs.IsCurrentForResources(resources), resources.PinCurrentEnvironment, token, configuration: wire).ConfigureAwait(false); // Check the same Session receipt/resources/disk before create, resume and every dialogue write.
        if (!observation.TreeClosed || !observation.StreamTasksTerminal || observation.Failure is not null || observation.ExitCode != 0 || observation.StandardError.Length != 0) // A matched JSON response cannot override nonzero, stderr, partial or unknown closure.
            return new(observation, null); // Preserve the actual failed resource observation without issuing configuration or permission authority.
        return new(observation, wire.TakeResolution()); // Only complete EOF plus exact process/drain closure transfers the full private response data.
    } // No thread, turn, model, tool or native-selection request is sent by this operation.

    // Explicit harmless test entry. This cannot construct a launch binding or return a Native result.
    internal static async Task<Observation> RunHarmlessCanaryForTestingAsync(string workingDirectory, string command, // Own this fixed harmless lifecycle canary without any native selection authority.
        int outputLimit, TimeSpan deadline, CancellationToken token) // Own this fixed harmless lifecycle canary without any native selection authority.
    { // Own this fixed harmless lifecycle canary without any native selection authority.
        if (command != "echo SOURCE56" && command != "echo SOURCE56-OUT & echo SOURCE56-ERR 1>&2" && // Allow only the fixed harmless output/overflow/wait/descendant canary vectors.
            command != "for /L %i in (1,1,2000) do @echo SOURCE56" && command != "ping -n 30 127.0.0.1 >nul" && // Continue the fixed canary allowlist without admitting caller-selected execution.
            command != "start /b ping -n 30 127.0.0.1 >nul") // Continue the fixed canary allowlist without admitting caller-selected execution.
            throw new ArgumentException("Only fixed harmless process-owner test commands are admitted.", nameof(command)); // Reject commands outside the harmless lifecycle fixture allowlist.
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess()); // Keep this fixed probe/canary under the production Job/process handle lifetime.
        using var environment = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent(); // This fixed harmless fixture captures actual data without issuing installed launch authority.
        return await lifetime.Owner.RunAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cmd.exe"), // Run exactly one retained resource or fixed harmless canary under the same owner.
            ["/d", "/s", "/c", command], workingDirectory, Array.Empty<byte>(), outputLimit, deadline, () => true, () => environment.Pin(), token).ConfigureAwait(false); // Keep the fixture's explicit complete environment under the same allocation lifetime.
    } // Own this fixed harmless lifecycle canary without any native selection authority.

    internal sealed record ProtectionFixtureObservation(Observation Process, bool ParentReadAndWriteEstablished, bool OriginalsCurrent, bool RootWriteAbsent, bool ChildWriteAbsent);
    internal static async Task<ProtectionFixtureObservation> RunResourceProtectionForTestingAsync(CancellationToken token)
    {
        using var fixture = PublicProtectionFixture.Open();
        using var protection = CodeDocumentationNativeCodexWindowsSecurityEnvironment.OpenPublicFixture(fixture, token);
        using var environment = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent();
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess());
        var observation = await lifetime.Owner.RunAsync(Path.Combine(Environment.SystemDirectory, "cmd.exe"), ["/d", "/s", "/c", "call native-protection-root.cmd"],
            fixture.Session, [], 65536, TimeSpan.FromSeconds(20), () => fixture.IsCurrent() && protection.IsCurrent(), () => environment.Pin(), token, protection: protection).ConfigureAwait(false);
        return new(observation, fixture.ParentBaseline, fixture.IsCurrent(), !File.Exists(Path.Combine(fixture.Session, "new-root.fixture")), !File.Exists(Path.Combine(fixture.Session, "new-child.fixture")));
    }

    // Finite temporary public-file custody; never an installed owner. It accepts no caller paths or shell commands.
    internal sealed class PublicProtectionFixture : IDisposable
    {
        private readonly string root;
        private readonly Dictionary<string, byte[]> originals = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<FileStream> held = [];
        internal string Session { get; }
        internal string Release { get; }
        internal bool ParentBaseline { get; private set; }
        private PublicProtectionFixture(string root) { this.root = root; Session = Path.Combine(root, "session"); Release = Path.Combine(root, "release"); }
        internal static PublicProtectionFixture Open()
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("The public PSEC fixture requires Windows.");
            var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "xiigen-s04-psec-" + Guid.NewGuid().ToString("N")));
            var pending = new PublicProtectionFixture(root);
            try
            {
                Directory.CreateDirectory(pending.Session); Directory.CreateDirectory(pending.Release);
                pending.Write(Path.Combine(pending.Session, "public-read.fixture"), "S04-ALLOWED-PUBLIC\r\n");
                pending.Write(Path.Combine(pending.Session, CodeDocumentationNativeCodexSelectionWire.RequestFileName), "S04-EXPLICIT-PRIVATE-PUBLIC-LITERAL\r\n");
                pending.Write(Path.Combine(root, "public-outside.fixture"), "S04-DEFAULT-PRIVATE-PUBLIC-LITERAL\r\n");
                static string Attempts(string who, string leaf) => "@echo off\r\necho " + who + "-ALLOWED\r\ntype public-read.fixture\r\necho " + who + "-EXPLICIT\r\ntype native-codex-selection.request\r\necho " + who + "-EXPLICIT-EXIT=%errorlevel%\r\necho " + who + "-DEFAULT\r\ntype ..\\public-outside.fixture\r\necho " + who + "-DEFAULT-EXIT=%errorlevel%\r\necho " + who + "-WRITE\r\n>" + leaf + " echo S04-FORBIDDEN-WRITE\r\necho " + who + "-WRITE-EXIT=%errorlevel%\r\n";
                pending.Write(Path.Combine(pending.Session, "native-protection-child.cmd"), Attempts("CHILD", "new-child.fixture") + "exit /b 0\r\n");
                pending.Write(Path.Combine(pending.Session, "native-protection-root.cmd"), Attempts("ROOT", "new-root.fixture") + "\"" + Path.Combine(Environment.SystemDirectory, "cmd.exe") + "\" /d /s /c \"call native-protection-child.cmd\"\r\nexit /b 0\r\n");
                var baseline = Path.Combine(pending.Session, "parent-baseline.fixture");
                File.WriteAllText(baseline, "S04-PARENT-WRITABLE", Encoding.ASCII);
                pending.ParentBaseline = File.ReadAllText(baseline, Encoding.ASCII) == "S04-PARENT-WRITABLE"; File.Delete(baseline);
                foreach (var pair in pending.originals)
                {
                    // Share.ReadWrite makes an ordinary readable/writable baseline explicit; denial cannot be attributed to our share lock.
                    var file = new FileStream(pair.Key, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); pending.held.Add(file);
                    if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(file.SafeFileHandle, pair.Key) || !File.ReadAllBytes(pair.Key).AsSpan().SequenceEqual(pair.Value))
                        throw new IOException("Public protection fixture custody failed.");
                }
                return pending;
            }
            catch (Exception error) { try { pending.Dispose(); } catch (Exception cleanup) { error.Data["PsecFixtureCleanup"] = cleanup.GetType().Name + ":" + cleanup.HResult; } throw; }
        }
        private void Write(string path, string text)
        { var bytes = Encoding.ASCII.GetBytes(text); using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read); file.Write(bytes); file.Flush(true); originals.Add(path, bytes); }
        internal bool IsCurrent() => originals.All(pair => File.ReadAllBytes(pair.Key).AsSpan().SequenceEqual(pair.Value));
        public void Dispose()
        {
            foreach (var file in held) file.Dispose();
            // Exact owned paths only, checked against the resolved owner boundary; no glob or recursive repository deletion.
            var boundary = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
            foreach (var path in originals.Keys.Concat(new[] { Path.Combine(Session, "parent-baseline.fixture"), Path.Combine(Session, "new-root.fixture"), Path.Combine(Session, "new-child.fixture") }))
            { if (!Path.GetFullPath(path).StartsWith(boundary, StringComparison.OrdinalIgnoreCase)) throw new IOException("Fixture cleanup boundary mismatch."); File.Delete(path); }
            foreach (var directory in new[] { Session, Release })
            { if (!Path.GetFullPath(directory).StartsWith(boundary, StringComparison.OrdinalIgnoreCase)) throw new IOException("Fixture cleanup directory boundary mismatch."); if (Directory.Exists(directory)) Directory.Delete(directory); }
            if (Directory.Exists(root)) Directory.Delete(root);
        }
    }

    internal static async Task<Observation> RunCancellationCanaryForTestingAsync(string workingDirectory, CancellationTokenSource cancellation) // Use actual stdout delivery to request caller cancellation under the same owned Job.
    { // Use actual stdout delivery to request caller cancellation under the same owned Job.
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess()); // Keep this fixed probe/canary under the production Job/process handle lifetime.
        using var environment = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent(); // Capture complete environment data for this fixed cancellation fixture only.
        // Cancellation follows an actual owned stdout read after resume; it cannot race cold JIT before creation.
        return await lifetime.Owner.RunAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cmd.exe"), // Run exactly one retained resource or fixed harmless canary under the same owner.
            ["/d", "/s", "/c", "echo SOURCE56-CANCEL-READY & ping -n 30 127.0.0.1 >nul"], workingDirectory, // Use actual stdout delivery to request caller cancellation under the same owned Job.
            Array.Empty<byte>(), 1024, TimeSpan.FromSeconds(5), () => true, () => environment.Pin(), cancellation.Token, cancellation).ConfigureAwait(false); // Use actual stdout delivery to request caller cancellation under the same owned Job.
    } // Use actual stdout delivery to request caller cancellation under the same owned Job.

    internal static async Task<Observation> RunLateCancellationCanaryForTestingAsync(string workingDirectory, CancellationTokenSource cancellation) // A fixed harmless process proves cancellation delivered strictly after confirmed root exit.
    { // This test-only entry returns process data and cannot construct a native launch binding.
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess()); // Keep this fixed probe/canary under the production Job/process handle lifetime.
        using var environment = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent(); // Keep the harmless late-output fixture on the same explicit allocation path.
        var rootExited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); // Hold actual stdout delivery until the owner confirms the root has exited.
        return await lifetime.Owner.RunAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cmd.exe"), // Run exactly one retained resource or fixed harmless canary under the same owner.
            ["/d", "/s", "/c", "echo SOURCE58-LATE-CANCEL"], workingDirectory, Array.Empty<byte>(), 1024, TimeSpan.FromSeconds(5), // One fixed root command needs no caller-selected child; cumulative Job accounting is not a one-process identity claim.
            () => true, () => environment.Pin(), cancellation.Token, cancellation, rootExited).ConfigureAwait(false); // Reuse production drain/cancellation/cleanup behavior with a causal delivery gate only.
    } // The delayed stdout witness cannot become selection authority.

    internal static async Task<Observation> RunWaitFailureCanaryForTestingAsync(string workingDirectory, bool failCleanup, CancellationToken token) // Exercise a real WAIT_FAILED through the unchanged owned catch/finally route with deny-only faults.
    { // This fixed harmless fixture retains the actual root/Job while querying a deliberately invalid separate handle.
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess()); // Keep the actual process and every descendant under the same original native owner.
        using var environment = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent(); // Use the existing actual environment allocation path without issuing installed permission.
        return await lifetime.Owner.RunAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cmd.exe"), // The fixture cannot choose an alternate image or pedagogical command.
            ["/d", "/s", "/c", "ping -n 30 127.0.0.1 >nul"], workingDirectory, [], 1024, TimeSpan.FromSeconds(5), // The same finite harmless wait canary produces no model or native-selection output.
            () => true, () => environment.Pin(), token, invalidWaitForTesting: true, failWaitCleanupForTesting: failCleanup).ConfigureAwait(false); // Both injected faults only deny completion and cannot mint a successful native result.
    } // Actual Job/drain cleanup remains mandatory even after the deliberately failed wait.

    internal static async Task<Observation> RunConfigurationCurrentFailureCanaryForTestingAsync(string workingDirectory, IOException currentFailure, bool failCleanup, CancellationToken token) // A fixed deny-only fixture checks an actual writer failure before later owned cleanup.
    { // No installed permission or native-selection binding can be constructed by this harmless fixture.
        using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(workingDirectory); // Use the actual fixed writer with no fabricated configuration result.
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess()); // Preserve the real atomic Job/root/drain ownership path.
        using var environment = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent(); // Keep the existing finite environment allocation and independent disposal.
        return await lifetime.Owner.RunAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cmd.exe"), // This fixture cannot choose another image or a pedagogical invocation.
            ["/d", "/s", "/c", "ping -n 30 127.0.0.1 >nul"], workingDirectory, [], 1024, TimeSpan.FromSeconds(5), // Reuse the existing harmless wait vector and finite bounds.
            () => true, () => environment.Pin(), token, configuration: wire, failWaitCleanupForTesting: failCleanup, configurationCurrentFailureForTesting: currentFailure).ConfigureAwait(false); // The injected current-input exception denies every protocol write and only adds a later cleanup fault.
    } // Actual tree and managed-stream closure remain mandatory independently of the original failure.

    internal sealed record NaturalChildObservation(Observation Process, uint ActiveAtRootExit, bool ChildReadyBeforeRelease, bool ReleasedAfterRootExit); // Physical data only, never launch permission.
    internal static async Task<NaturalChildObservation> RunNaturalChildCanaryForTestingAsync(string workingDirectory, CancellationToken token, CancellationTokenSource? cancelAfterRoot = null, IOException? failAfterRoot = null, bool failCleanup = false)
    {
        if (cancelAfterRoot is not null && cancelAfterRoot.Token != token) throw new ArgumentException("The fixed cancellation fixture requires its original caller token.");
        // Fixed relative leaf names prevent caller path text from entering cmd syntax. CreateNew preserves existing fixture evidence.
        static void WriteFixture(string directory, string leaf, string text)
        { using var file = new FileStream(Path.Combine(directory, leaf), FileMode.CreateNew, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough); file.Write(Encoding.ASCII.GetBytes(text)); file.Flush(true); }
        var systemCmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cmd.exe");
        WriteFixture(workingDirectory, "native-natural-child.cmd", "@echo off\r\n>native-child-prepared.fixture echo CHILD-READY\r\nmove /y native-child-prepared.fixture native-child-ready.fixture >nul\r\n:wait\r\nif not exist native-child-release.fixture goto wait\r\necho SOURCE-NATURAL-CHILD\r\nexit /b 0\r\n"); // Publish the complete ready marker by same-directory rename, not create-before-write existence.
        WriteFixture(workingDirectory, "native-natural-root.cmd", "@echo off\r\nstart \"\" /b \"" + systemCmd + "\" /d /s /c \"call native-natural-child.cmd\"\r\n:wait\r\nif not exist native-child-ready.fixture goto wait\r\necho SOURCE-NATURAL-ROOT\r\necho SOURCE-NATURAL-ERR 1>&2\r\nexit /b 0\r\n");
        using var lifetime = new ProcessLifetime(new CodeDocumentationNativeCodexWindowsProcess());
        using var environment = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent();
        uint activeAtRoot = 0; var ready = false; var released = false;
        var observation = await lifetime.Owner.RunAsync(systemCmd,
            ["/d", "/s", "/c", "call native-natural-root.cmd"], workingDirectory, [], 1024, TimeSpan.FromSeconds(5), () => true, () => environment.Pin(), token,
            failWaitCleanupForTesting: failCleanup, rootAccountingForTesting: active =>
            {
                activeAtRoot = active; ready = File.ReadAllText(Path.Combine(workingDirectory, "native-child-ready.fixture")) == "CHILD-READY\r\n";
                if (active == 0 || !ready) throw new IOException("The fixed child was not alive in the retained Job at exact root exit.");
                if (failAfterRoot is not null) throw failAfterRoot; // This fixed fault only denies observation; actual Job/drain cleanup remains mandatory.
                if (cancelAfterRoot is not null) { cancelAfterRoot.Cancel(); return; }
                WriteFixture(workingDirectory, "native-child-release.fixture", "RELEASE-AFTER-ROOT\r\n"); released = true; // Release occurs only after checked root exit AND positive actual owned accounting.
            }).ConfigureAwait(false);
        return new(observation, activeAtRoot, ready, released);
    }

    private async Task<Observation> RunAsync(string image, IReadOnlyList<string> arguments, string workingDirectory, // Preserve one finite process effect with exact root birth, tree and stream cleanup.
        byte[] input, int outputLimit, TimeSpan deadline, Func<bool> current, // Preserve one finite process effect with exact root birth, tree and stream cleanup.
        Func<CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.PinnedBlock> allocateEnvironment, CancellationToken token, // Production supplies only the fixed retained resource owner method, never a caller environment dictionary.
        CancellationTokenSource? cancellationAfterObservedOutputForTesting = null, TaskCompletionSource<bool>? rootExitForTesting = null, CodeDocumentationNativeCodexInstalledConfigWire? configuration = null, bool invalidWaitForTesting = false, bool failWaitCleanupForTesting = false, IOException? configurationCurrentFailureForTesting = null, Action<uint>? rootAccountingForTesting = null,
        CodeDocumentationNativeCodexWindowsSecurityEnvironment? protection = null) // Only a retained internal RESOURCE owner can attach protection; it cannot issue semantic authority.
    { // Keep the admitted single-effect process boundary inside its owned cleanup scope.
        if (used) throw new InvalidOperationException("A process tree owner cannot be reused."); // Reject owner reuse before any second native effect.
        used = true; // Consume this owner once even when validation or creation subsequently fails.
        if (outputLimit is < 1 or > 65536 || deadline <= TimeSpan.Zero || deadline > TimeSpan.FromSeconds(20) || // Validate existing diagnostic output, duration and input bounds before allocation.
            input.Length > CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes) // Keep the existing request input cap rather than increasing selection limits.
            throw new ArgumentOutOfRangeException(nameof(outputLimit)); // Refuse invalid finite process bounds before acquiring pipes.
        token.ThrowIfCancellationRequested(); // Preserve exact caller cancellation before allocation or while the owned root is running.
        using var stdout = Pipe.Create(false); using var stderr = Pipe.Create(false); using var stdin = Pipe.Create(true); // Create separate parent/child endpoints for all three standard streams.
        using var drainCancellation = new CancellationTokenSource(); // Own stream cancellation independently from the caller token and native Job.
        SafeFileHandle? outputRead = null, errorRead = null, inputWrite = null; // Track every detached parent pipe endpoint for unconditional closure.
        Attributes? attributes = null; // Keep the stable protection HANDLE slot through whole Job and every stream closure.
        Task<byte[]>? outDrain = null, errDrain = null; Task? inputDrain = null; // Track each started reader/writer separately, including partial-start failures.
        ExceptionDispatchInfo? primary = null; uint pid = 0, exit = 0, total = 0; long birth = 0; // Retain the first actual exception and exact native root identity for outward reporting.
        bool closed = false, descendants = false, rootExited = false; string? failure = null; OwnedStreamCleanup? streams = null; // Record exact root, Job and managed-task completion independently.
        byte[] outBytes = [], errBytes = []; var clock = Stopwatch.StartNew(); var budget = new DrainBudget(outputLimit); // Initialize separate output preimages, finite elapsed time and shared byte accounting.
        try // Preserve one finite process effect with exact root birth, tree and stream cleanup.
        { // Keep native creation and root observation inside the original-primary capture scope.
            stdout.MakeReadPrivate(); stderr.MakeReadPrivate(); stdin.MakeWritePrivate(); // Exclude parent-only endpoints from the inherited handle list.
            attributes = new Attributes(job, [stdin.Read, stdout.Write, stderr.Write], protection); // Bind protection together with the original atomic Job and fixed handles.
            var startup = new StartupEx { Startup = new Startup { Size = (uint)Marshal.SizeOf<StartupEx>(), // Bind the exact three standard handles in the extended native startup structure.
                Flags = 0x100, Input = stdin.Read.DangerousGetHandle(), Output = stdout.Write.DangerousGetHandle(), // Enable only the fixed child standard streams in the native startup record.
                Error = stderr.Write.DangerousGetHandle() }, Attributes = attributes.Pointer }; // Complete stderr and the retained Job/handle attribute pointer before creation.
            var command = new StringBuilder(Quote(image) + string.Concat(arguments.Select(a => " " + Quote(a)))); // Quote image and each argument without shell-derived execution changes.
            using var environment = allocateEnvironment(); // Release the complete explicit UTF16 pin on every validation, cancellation, native failure and successful exit.
            if (!current()) throw new IOException("Installed launch inputs changed before first process creation."); // Refuse changed installed inputs immediately before the first OS create.
            token.ThrowIfCancellationRequested(); // Preserve exact caller cancellation before allocation or while the owned root is running.
            // JOB_LIST assigns the Job atomically in CreateProcess. The first thread is still suspended.
            var created = CreateProcessW(image, command, IntPtr.Zero, IntPtr.Zero, true, // Create the first process suspended with atomic Job and inherited-handle attributes.
                CreateSuspended | ExtendedStartup | UnicodeEnvironment | NoWindow, environment.Pointer, // Pass the retained complete double-NUL UTF16 block; inherited mutable environment is never a fallback.
                workingDirectory, ref startup, out var information); // Use the held directory and receive exact created process/thread identity.
            var creationError = Marshal.GetLastWin32Error(); // Capture the exact native creation error before any subsequent Windows call.
            // Retain out-parameter handles even if creation reports failure.
            if (information.Process != IntPtr.Zero) process = new SafeFileHandle(information.Process, true); // Retain any returned process handle even when creation reports failure.
            if (information.Thread != IntPtr.Zero) thread = new SafeFileHandle(information.Thread, true); // Retain any returned suspended-thread handle for the same failed/successful creation.
            pid = information.ProcessId; // Record the actual native root PID rather than a guessed descendant or command shell.
            if (!created) throw new Win32Exception(creationError, "CreateProcessW with suspended JOB_LIST failed."); // Propagate the original suspended Job-list creation error with retained handles.
            Check(GetProcessTimes(process!, out birth, out _, out _, out _), "GetProcessTimes root birth"); // Read the actual root birth time for exact process identity.
            Check(IsProcessInJob(process!, job, out var assigned), "IsProcessInJob"); // Verify the created suspended root belongs to this retained Job.
            jobAssigned = assigned;
            if (!assigned)
            {
                if (protection is not null) throw new CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal("CreateProcessW SECURITY_ENVIRONMENT+JOB_LIST root membership unavailable"); // Actual membership=false is a predicate refusal, not an invented native HRESULT.
                throw new IOException("The suspended process was not assigned to its retained Job.");
            }
            if (!current()) throw new IOException("The assigned suspended process lost its retained launch inputs."); // Split the actual membership and input predicates before any resume.
            stdout.Write.Dispose(); stderr.Write.Dispose(); stdin.Read.Dispose(); // Release all parent copies of child pipe ends so real EOF can reach the readers.
            outputRead = stdout.DetachRead(); errorRead = stderr.DetachRead(); inputWrite = stdin.DetachWrite(); // Transfer parent pipe endpoints into explicit managed stream ownership.
            outDrain = DrainAsync(outputRead, budget, drainCancellation.Token, cancellationAfterObservedOutputForTesting, rootExitForTesting?.Task, configuration); // The configuration-only mode consumes whole correlated frames from this same actual stdout drain.
            errDrain = DrainAsync(errorRead, budget, drainCancellation.Token, configuration: configuration, consumeConfiguration: false); // Private stderr shares first-cause/clearing ownership without parsing it as JSON protocol output.
            inputDrain = configuration is null ? WriteAsync(inputWrite, input, drainCancellation.Token) : WriteConfigurationAsync(inputWrite, configuration, configurationCurrentFailureForTesting is null ? current : () => throw configurationCurrentFailureForTesting, token, drainCancellation.Token); // The original caller and independent cleanup tokens stay distinct through the actual production writer.
            if (!current()) throw new IOException("Installed inputs changed immediately before resume."); // Revalidate retained inputs again immediately before resuming the root.
            token.ThrowIfCancellationRequested(); // Preserve exact caller cancellation before allocation or while the owned root is running.
            var previousSuspendCount = ResumeThread(thread!); // Invoke once for this verified retained root; a zero count means execution was already possible.
            var resumeError = previousSuspendCount == uint.MaxValue ? Marshal.GetLastWin32Error() : 0; // Capture an actual API failure immediately before classifier or cleanup can alter last error.
            CheckResumeOutcome(previousSuspendCount, resumeError); // Only count1 can continue; every other result uses the unchanged original-primary cleanup path.
            thread.Dispose(); thread = null; // Release the now-unused suspended-thread handle after successful resume.
            while (CheckedRootWait(invalidWaitForTesting) == 258) // Immediately capture WAIT_FAILED at the actual wait before another Windows call can overwrite its error.
            { // Monitor this one resumed root while preserving cancellation for mandatory cleanup.
                token.ThrowIfCancellationRequested(); // Preserve exact caller cancellation before allocation or while the owned root is running.
                if (budget.Overflow) { failure = "output-overflow"; break; } // Stop successful observation when combined stdout/stderr exceed the fixed byte budget.
                if (outDrain.IsFaulted || errDrain.IsFaulted || inputDrain.IsFaulted) { failure = "stream-failure"; break; } // Keep any started stream fault explicit while the native tree remains owned.
                if (clock.Elapsed >= deadline) { failure = "deadline"; break; } // Stop this owned effect at its original finite deadline.
                await Task.Delay(10, CancellationToken.None).ConfigureAwait(false); // Yield finitely without allowing caller cancellation to skip mandatory cleanup.
            } // Finish this bounded process operation before its next mandatory ownership check.
            if (CheckedRootWait(invalidWaitForTesting) == 0) // Use the same checked wait for completion; failure is the original Win32 primary rather than root-incomplete.
            { // Capture confirmed root completion before releasing the late-output fixture gate.
                rootExited = true; // The late-output fixture gate remains pending until this same Job naturally reports active0.
                Check(GetExitCodeProcess(process!, out exit), "GetExitCodeProcess"); // Read the actual root exit code only after its native completion signal.
                var accounting = Accounting(); total = accounting.Total; // Read total/active Job process counts after confirmed root exit.
                descendants = accounting.Active != 0;
                rootAccountingForTesting?.Invoke(accounting.Active); // Fixed fixture observes child-alive BEFORE release, after the actual root handle/exit-code witness.
                while (true) // Observe natural closure of THIS Job using the original Stopwatch, never a new grace period.
                {
                    token.ThrowIfCancellationRequested();
                    if (budget.Overflow) { failure ??= "output-overflow"; break; }
                    if (outDrain.IsFaulted || errDrain.IsFaulted || inputDrain.IsFaulted) { failure ??= "stream-failure"; break; }
                    if (clock.Elapsed >= deadline) { descendants = true; failure ??= "descendants-after-root"; break; }
                    accounting = Accounting(); total = accounting.Total; // A failed actual native query throws immediately; no fabricated zero count.
                    if (clock.Elapsed >= deadline) { descendants = true; failure ??= "descendants-after-root"; break; }
                    descendants = accounting.Active != 0;
                    if (!descendants) break; // Only naturally observed active0 before the original deadline permits further success checks.
                    await Task.Delay(10, CancellationToken.None).ConfigureAwait(false);
                }
                if (!descendants) rootExitForTesting?.TrySetResult(true); // Deny-only fixture ordering removes root-exit/Job-accounting cancellation races; production has no gate.
                if (exit != 0) failure ??= "nonzero-exit"; // Keep a nonzero native exit explicit without clearing earlier failures.
            } // Complete root-exit accounting without treating it as descendant or stream closure.
            else failure ??= "root-incomplete"; // Retain incomplete root observation rather than manufacturing a successful exit.
        } // Converge creation, resume and wait failures into the original-primary cleanup path.
        catch (Exception ex) { primary = configuration?.Primary ?? ExceptionDispatchInfo.Capture(ex); } // Preserve a protocol error already captured before a subsequent loop/native/cancellation error.
        finally // Execute whole-tree and independent stream cleanup on every process exit.
        { // Always attempt whole-tree termination before independent managed stream cleanup.
            primary ??= configuration?.Primary; // Capture an already observed writer/stdout fault before Job cleanup can produce its own later error.
            try { closed = Quiesce(out total); if (!closed) failure ??= "tree-not-quiescent"; if (failWaitCleanupForTesting) throw new IOException("Controlled wait-cleanup refusal."); } // The deny-only fixture faults after actual Job quiescence so all remaining real stream cleanup still executes.
            catch (Exception cleanup) // Catch actual Job termination/accounting errors while preserving any existing primary.
            { // Retain Job cleanup failure without replacing an earlier process or cancellation error.
                if (primary is null) primary = ExceptionDispatchInfo.Capture(cleanup); // Retain a cleanup-only exception only when no earlier primary error exists.
                else primary.SourceException.Data["NativeProcessCleanup"] = cleanup.GetType().Name + ":" + cleanup.HResult; // Attach a bounded actual cleanup cause to the preserved primary exception.
            } // Finish this bounded process operation before its next mandatory ownership check.
            // Release all parent copies of child ends, including failures before create/resume.
            stdout.Write.Dispose(); stderr.Write.Dispose(); stdin.Read.Dispose(); // Release all parent copies of child pipe ends so real EOF can reach the readers.
            rootExitForTesting?.TrySetResult(false); // A failed/incomplete root cannot leave a fixture-delayed drain waiting forever.
            streams = await CloseOwnedStreamsAsync(outDrain, errDrain, inputDrain, drainCancellation.Cancel, // Each started task gets its own bounded join even if another stream faults.
                () => outputRead?.Dispose(), () => errorRead?.Dispose(), () => inputWrite?.Dispose(), // Dispose every parent stream before the final independent terminal verification.
                TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).ConfigureAwait(false); // Preserve existing grace and terminal-cleanup bounds.
            outBytes = streams.StandardOutput; errBytes = streams.StandardError; // Only independently completed successful readers contribute response bytes.
            thread?.Dispose(); thread = null; // The retained suspended-thread handle is released after all independent stream attempts.
            attributes?.Dispose(); // Stable SECURITY_ENVIRONMENT storage is released after the Job/drains, never before suspended resume or cleanup.
        } // Finish all mandatory tree and stream cleanup before observing late caller cancellation.
        var priorFailure = failure; failure = ResolveFinalFailure(priorFailure, budget, closed, streams); // Arbitrate only after actual independent drains finish; a late observed overflow cannot remain merely a root nonzero exit.
        primary ??= configuration?.Primary; // Preserve the original protocol/current-input error after independent cleanup rather than replacing it with a generic stream failure.
        primary = ResolveFinalPrimary(primary, token, streams); // Choose the final outward error before attaching actual bounded stream cleanup causes.
        if (primary is not null) // Return the earliest actual error with exact process and managed-task closure evidence.
        { // Attach exact ownership evidence before rethrowing the retained original primary.
            primary.SourceException.Data["NativeProcessRootPid"] = pid; // Attach the exact observed root PID to the original outward exception.
            primary.SourceException.Data["NativeProcessRootBirthFileTime"] = birth; // Attach the exact native root birth identity to the preserved exception.
            primary.SourceException.Data["NativeProcessTreeClosed"] = closed; // Report actual Job quiescence independently from managed stream closure.
            primary.SourceException.Data["NativeProcessJobAssigned"] = jobAssigned; // Preserve the actual atomic assignment predicate, including refusal before resume.
            primary.SourceException.Data["NativeProcessTotalProcesses"] = total; // Retain the raw cumulative lifetime Job count, including already terminated associated processes.
            primary.SourceException.Data["NativeProcessRootExitObserved"] = rootExited; // An exact root handle signal is distinct from Job lifetime process accounting.
            primary.SourceException.Data["NativeProcessExitCode"] = exit; // The root exit code is meaningful only with its separate confirmed-completion witness.
            primary.SourceException.Data["NativeProcessDescendantsAfterRoot"] = descendants; // Preserve actual still-active Job membership observed after the root exited.
            primary.SourceException.Data["NativeProcessOutputOverflow"] = budget.Overflow; // Known bounded-output failure remains visible even under stronger incomplete-closure classification.
            primary.SourceException.Data["NativeProcessFailure"] = failure; primary.SourceException.Data["NativeProcessPriorFailure"] = priorFailure; // Retain final classification and earlier observed reason without replacing the original exception or token.
            primary.SourceException.Data["NativeProcessCancellationOutputWitness"] = budget.CancellationOutputWitness; // Report whether an actual owned stdout read requested caller cancellation.
            primary.SourceException.Data["NativeProcessCancellationAfterRootExitWitness"] = budget.CancellationAfterRootExitWitness; // Distinguish confirmed late delivery from ordinary output cancellation.
            primary.SourceException.Data["NativeProcessStreamTasksTerminal"] = streams?.AllTerminal == true; // Job closure alone never claims all managed tasks stopped.
            RethrowAfterPrivateOutputCleanup(primary, configuration, outBytes, errBytes); // Clear otherwise-unreturned private observation copies before preserving this original throw.
        } // Finish the original-primary branch before returning only an explicit observation.
        return new(pid, birth, exit, outBytes, errBytes, total, closed, descendants, failure, streams?.AllTerminal == true, streams?.Failure, rootExited, budget.Overflow, priorFailure); // Return exact root/tree/stream observations and every independent overflow/prior cause as data.
    } // Finish this bounded process operation before its next mandatory ownership check.

    internal static void RethrowAfterPrivateOutputCleanup(ExceptionDispatchInfo primary, CodeDocumentationNativeCodexInstalledConfigWire? configuration, byte[] stdout, byte[] stderr) // The actual primary branch and literal fixtures share one private-copy cleanup point.
    { if (configuration is not null) { Array.Clear(stdout); Array.Clear(stderr); } primary.Throw(); } // Keep legacy diagnostic copies intact; clear only unreturned configuration copies and retain exact original exception/token.

    private uint CheckedRootWait(bool invalidForTesting) // The two real root-wait sites share one immediate last-error capture contract.
    { // A separate invalid fixture handle never disposes or substitutes the actual retained root used for cleanup.
        using var invalid = invalidForTesting ? new SafeFileHandle(IntPtr.Zero, false) : null; // Zero is an invalid native wait handle; no pseudo-handle or actual retained root is disposed by this deny-only fixture.
        var result = WaitForSingleObject(invalid ?? process!, 0); // Query exactly one selected handle and retain the raw native result.
        var nativeError = result == uint.MaxValue ? Marshal.GetLastWin32Error() : 0; // Capture immediately before any cleanup/native call can overwrite WAIT_FAILED's last error.
        return CheckWaitOutcome(result, nativeError); // A failed wait throws through the existing primary/finally/ExceptionDispatchInfo path.
    } // Timeout and signaled results keep their original root-observation meanings.
    internal static uint CheckWaitOutcome(uint result, int nativeError) // Pure literal-result fixtures validate the same production result/error arbitration.
    { // Only the supported zero-timeout wait outcomes can enter the existing loop/completion branches.
        if (result == uint.MaxValue) throw new Win32Exception(nativeError, "WaitForSingleObject root failed."); // Preserve the immediately captured native code as the original primary.
        if (result is not (0 or 258)) throw new IOException("Unexpected root wait result."); // An unknown/abandoned result cannot be treated as successful completion or timeout.
        return result; // Return the actual signaled/timeout value without manufacturing a new observation.
    } // This checked result helper grants no process, configuration or semantic authority.

    internal static uint CheckResumeOutcome(uint previousSuspendCount, int nativeError) // Pure classification grants no owner/admission and performs no native operation.
    {
        if (previousSuspendCount == uint.MaxValue) throw new Win32Exception(nativeError, "ResumeThread failed."); // Even native error0 cannot turn the API failure sentinel into success.
        if (previousSuspendCount != 1) throw new IOException("ResumeThread previous suspend count=" + previousSuspendCount.ToString(System.Globalization.CultureInfo.InvariantCulture) + "; expected=1; cause-source=ResumeThread-return."); // Preserve the exact unsigned observation rather than a fabricated Win32 error.
        return previousSuspendCount; // Count1 alone means the once-suspended initial thread was restarted.
    }

    internal static string? ResolveFinalFailure(string? priorFailure, DrainBudget budget, bool treeClosed, OwnedStreamCleanup? streams) // Production and bounded I/O fixtures share final observation arbitration without granting launch authority.
    { // Actual closure unknowns outrank known output failure; all earlier reasons remain separate observation data.
        if (!treeClosed) return "tree-not-quiescent"; // Never resolve a known overflow or root exit into successful whole-Job closure.
        if (streams is { AllTerminal: false }) return "stream-tasks-nonterminal"; // A live independent stream stays unknown even when another reader already proved overflow.
        if (priorFailure is not (null or "nonzero-exit" or "stream-failure" or "output-overflow")) return priorFailure; // Preserve deadline, descendants, root-incomplete and any unrecognized earlier refusal.
        if (budget.Overflow) return "output-overflow"; // Actual shared-budget overflow takes precedence over a nonzero root exit or terminal pipe fault discovered first.
        if (streams?.Failure is not null) return "stream-failure"; // Real terminal stream/cleanup failure cannot become a successful observation or merely nonzero-exit.
        return priorFailure; // With no later bounded-output/cleanup cause, retain the actual earlier root result.
    } // Error-object and cancellation arbitration remain in the unchanged independent primary owner below.

    internal static ExceptionDispatchInfo? ResolveFinalPrimary(ExceptionDispatchInfo? primary, CancellationToken token, OwnedStreamCleanup? streams) // Share only final error arbitration with isolated I/O fixtures; this grants no launch authority.
    { // Stream cleanup may request late caller cancellation after the process wait succeeded.
        if (primary is null && token.IsCancellationRequested) // Preserve any earlier process or Job error ahead of cleanup-delivered cancellation.
        { // Obtain the outward cancellation from the exact caller token rather than constructing a replacement primary.
            try { token.ThrowIfCancellationRequested(); } catch (OperationCanceledException cancellation) { primary = ExceptionDispatchInfo.Capture(cancellation); } // Capture the exact token-bearing OCE for later rethrow.
        } // The final primary is now known, including a late OCE created after stream cleanup.
        if (primary is not null && streams?.Failure is { } streamFailure) primary.SourceException.Data["NativeProcessStreams"] = streamFailure; // Retain the actual bounded stream cause on whichever original exception will leave this owner.
        return primary; // Return the original dispatch/object unchanged when an earlier error already exists.
    } // Pure error selection does not alter independent tree, stream or handle cleanup.

    internal sealed record OwnedStreamCleanup(byte[] StandardOutput, byte[] StandardError, bool OutputTerminal, bool ErrorTerminal, bool InputTerminal, string? Failure) // Data-only cleanup observation shared with deterministic resource fixtures.
    { internal bool AllTerminal => OutputTerminal && ErrorTerminal && InputTerminal; } // Every started task must independently be terminal before closure can be claimed.

    internal static Task<OwnedStreamCleanup> CloseOwnedStreamsForTestingAsync(Task<byte[]> output, Task<byte[]> error, Task input, Action cancel, Action closeOutput, Action closeError, Action closeInput) // Isolated task/stream fixtures exercise the production cleanup without any installed authority.
        => CloseOwnedStreamsAsync(output, error, input, cancel, closeOutput, closeError, closeInput, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(1)); // Immediate initial deadlines expose pending fixtures; the ordinary terminal bound lets actual disposal continuations finish.

    private static async Task<OwnedStreamCleanup> CloseOwnedStreamsAsync(Task<byte[]>? output, Task<byte[]>? error, Task? input, Action cancel, Action closeOutput, Action closeError, Action closeInput, TimeSpan outputGrace, TimeSpan inputGrace, TimeSpan terminalGrace) // Close readers and writer independently with finite bounds.
    { // Stream faults are bounded secondary evidence and cannot skip another stream's cleanup.
        var failures = new List<string>(); // Retain at most the fixed three task and four cleanup-operation causes.
        async Task Observe(Task? task, string name, TimeSpan grace) // Attempt one started task without propagating its failure to another join.
        { // A completed fault must be observed while a pending task receives an explicit finite deadline.
            if (task is null) return; // Failures before a task starts leave no task to join.
            if (!task.IsCompleted && await Task.WhenAny(task, Task.Delay(grace, CancellationToken.None)).ConfigureAwait(false) != task) { failures.Add(name + ":deadline"); return; } // A pending task is retained as incomplete evidence.
            try { await task.ConfigureAwait(false); } catch (Exception streamError) { failures.Add(name + ":" + streamError.GetType().Name + ":" + streamError.HResult); } // Observe the actual task error without replacing a caller primary.
        } // Complete this independent join regardless of another stream's state.
        var outputPhase = Stopwatch.StartNew(); // Both reader attempts share the original output-grace budget instead of receiving five seconds each.
        await Observe(output, "stdout", outputGrace).ConfigureAwait(false); // Observe stdout independently within the common finite output phase.
        await Observe(error, "stderr", Remaining(outputGrace, outputPhase)).ConfigureAwait(false); // Attempt stderr even after stdout fault/timeout, using only the common phase's remaining grace.
        await Observe(input, "stdin", inputGrace).ConfigureAwait(false); // Empty current production input does not weaken the general writer contract.
        foreach (var (name, close) in new (string, Action)[] { ("cancel", cancel), ("stdout-close", closeOutput), ("stderr-close", closeError), ("stdin-close", closeInput) }) // Attempt cancellation and every handle disposal independently.
        { try { close(); } catch (Exception streamError) { failures.Add(name + ":" + streamError.GetType().Name + ":" + streamError.HResult); } } // A cleanup callback failure never prevents the remaining fixed cleanup operations.
        var terminalPhase = Stopwatch.StartNew(); // Every final task check shares one finite post-disposal phase.
        await ObserveTerminal(output, "stdout").ConfigureAwait(false); // Verify actual stdout terminal state after cancellation and disposal.
        await ObserveTerminal(error, "stderr").ConfigureAwait(false); // Verify stderr separately even after a stdout fault or timeout.
        await ObserveTerminal(input, "stdin").ConfigureAwait(false); // Verify the writer separately even when output could not close.
        async Task ObserveTerminal(Task? task, string name) // Final bounded verification is per task, not a WhenAll/root-exit inference.
        { // Do not create success from a timeout whose underlying task remains live.
            if (task is null) return; // An unstarted slot is vacuously closed.
            if (!task.IsCompleted && await Task.WhenAny(task, Task.Delay(Remaining(terminalGrace, terminalPhase), CancellationToken.None)).ConfigureAwait(false) != task) { failures.Add(name + ":nonterminal"); return; } // Each surviving task remains explicit unknown within the common final phase deadline.
            try { await task.ConfigureAwait(false); } catch (Exception streamError) { var detail = name + ":" + streamError.GetType().Name + ":" + streamError.HResult; if (!failures.Contains(detail)) failures.Add(detail); } // Observe final faults once while retaining the initial deadline.
        } // A terminal fault is closed ownership, but never successful output.
        static TimeSpan Remaining(TimeSpan bound, Stopwatch elapsed) { var left = bound - elapsed.Elapsed; return left > TimeSpan.Zero ? left : TimeSpan.Zero; } // Sample elapsed once so an expiring phase cannot pass a negative Task.Delay duration.
        return new(output?.Status == TaskStatus.RanToCompletion ? output.Result : [], error?.Status == TaskStatus.RanToCompletion ? error.Result : [], // Return bytes only from completed successful reads.
            output?.IsCompleted != false, error?.IsCompleted != false, input?.IsCompleted != false, failures.Count == 0 ? null : string.Join(";", failures)); // The fixed inventory bounds failure text without losing individual task states.
    } // Complete all independent cleanup attempts before returning closure data.

    private AccountingInfo Accounting() // Read exact Job accounting instead of inferring descendant closure from root exit.
    { // Read exact Job accounting instead of inferring descendant closure from root exit.
        Check(QueryAccounting(job, 1, out var value, (uint)Marshal.SizeOf<AccountingInfo>(), out var length), "QueryInformationJobObject accounting"); // Read actual total/active processes from the retained Job.
        if (length != Marshal.SizeOf<AccountingInfo>() || value.Active > value.Total) throw new IOException("Invalid Job accounting."); // Refuse inconsistent accounting before using it as a tree-closure certificate.
        return value; // Return the verified Job accounting preimage.
    } // Read exact Job accounting instead of inferring descendant closure from root exit.
    private bool Quiesce(out uint total) // Terminate and finitely verify this owner's entire native Job tree.
    { // Terminate and finitely verify this owner's entire native Job tree.
        var clock = Stopwatch.StartNew(); // The same original five-second cleanup bound covers both Job and any returned unassigned suspended root.
        var separateRoot = process is not null && !jobAssigned;
        if (separateRoot && CheckedRootWait(false) == 258) Check(TerminateProcess(process!, 1), "TerminateProcess unassigned owned suspended root"); // Refusal-only cleanup by the exact retained handle; never resume/adopt/fallback.
        var value = Accounting(); total = value.Total; // Capture the current owned process count before termination.
        if (value.Active != 0) Check(TerminateJobObject(job, 1), "TerminateJobObject"); // Terminate only this owner's Job when any owned process remains active.
        do { value = Accounting(); total = value.Total; if (value.Active == 0 && (!separateRoot || CheckedRootWait(false) == 0)) return true; Thread.Sleep(10); } // Job0 alone cannot certify an unassigned returned root; require its real wait signal too.
        while (clock.Elapsed < TimeSpan.FromSeconds(5)); // Limit descendant termination confirmation to the existing five-second bound.
        return false; // Return explicit unknown tree closure if the finite quiescence check expires.
    } // Terminate and finitely verify this owner's entire native Job tree.
    private static async Task<byte[]> DrainAsync(SafeFileHandle handle, DrainBudget budget, CancellationToken token, // Collect actual bounded output with cancellable asynchronous pipe reads.
        CancellationTokenSource? cancellationAfterObservedOutputForTesting = null, Task<bool>? deliveryAfterRootExitForTesting = null, CodeDocumentationNativeCodexInstalledConfigWire? configuration = null, bool consumeConfiguration = true) // Both private streams capture causes; only stdout consumes the fixed protocol.
    { // Collect actual bounded output with cancellable asynchronous pipe reads.
        using var stream = new FileStream(handle, FileAccess.Read, 1024, true); // Keep the original detached overlapped endpoint under its unconditional independent stream disposal.
        return await DrainStreamAsync(stream, budget, token, cancellationAfterObservedOutputForTesting, deliveryAfterRootExitForTesting, configuration, consumeConfiguration).ConfigureAwait(false); // Actual native pipes and bounded fixtures use the identical read/reserve/delivery loop.
    } // Native endpoint ownership ends only after its real shared drain has reached a terminal state.
    internal static Task<byte[]> DrainStreamForTestingAsync(Stream stream, DrainBudget budget, Task<bool>? deliveryAfterRootExit = null) // A caller-owned physical I/O fixture cannot create an installed process or launch binding.
        => DrainStreamAsync(stream, budget, CancellationToken.None, null, deliveryAfterRootExit); // Share actual byte reads and budget reservations rather than constructing a fake overflow result.
    internal static Task<byte[]> DrainConfigurationStreamForTestingAsync(Stream stream, DrainBudget budget, CodeDocumentationNativeCodexInstalledConfigWire wire, CancellationToken cleanupToken, Action<byte[], byte[]> buffers, bool stdout = true) // Public-literal fixtures observe real shared buffer lifetimes without installed authority.
        => DrainStreamAsync(stream, budget, cleanupToken, null, null, wire, stdout, buffers); // No separate parser, fabricated budget result or native launcher is introduced.
    private static async Task<byte[]> DrainStreamAsync(Stream stream, DrainBudget budget, CancellationToken token, CancellationTokenSource? cancellationAfterObservedOutputForTesting, Task<bool>? deliveryAfterRootExitForTesting, CodeDocumentationNativeCodexInstalledConfigWire? configuration = null, bool consumeConfiguration = true, Action<byte[], byte[]>? buffersForTesting = null) // Preserve the same shared finite drain budget and optional private-data mode.
    { // The native wrapper owns its FileStream; bounded fixtures own and independently dispose their supplied stream.
        using var bytes = configuration is null ? new MemoryStream() : new MemoryStream(budget.Maximum); // One finite configuration backing allocation avoids discarded private growth copies.
        var chunk = new byte[1024]; int count; // Use the fixed read buffer without allocating from untrusted output length.
        try // Capture genuine source causes before owned cancellation; clear both actual temporary allocations on every private-mode exit.
        { // Legacy diagnostic observations keep their existing byte-return semantics.
        buffersForTesting?.Invoke(chunk, bytes.GetBuffer()); // Test-only literal references witness the actual buffers used below, not replacement zero arrays.
        while ((count = await stream.ReadAsync(chunk, token).ConfigureAwait(false)) != 0) // Read actual pipe bytes asynchronously until EOF under owned stream cancellation.
        { // Collect actual bounded output with cancellable asynchronous pipe reads.
            var afterRootExit = deliveryAfterRootExitForTesting is not null && await deliveryAfterRootExitForTesting.WaitAsync(token).ConfigureAwait(false); // Hold the actual read chunk until the deterministic root-exit gate opens.
            if (!budget.Reserve(count)) throw new IOException("Finite native stream budget exceeded."); // Reject combined-stream overflow before accepting the observed chunk.
            if (consumeConfiguration) configuration?.Accept(chunk.AsSpan(0, count)); // Stderr never supplies JSON responses, while both streams share original-error custody.
            bytes.Write(chunk, 0, count); // Append only the actual chunk admitted by the finite shared budget.
            if (cancellationAfterObservedOutputForTesting is not null) // Request fixture cancellation only after a genuine owned output read.
            { // Collect actual bounded output with cancellable asynchronous pipe reads.
                budget.CancellationOutputWitness = true; budget.CancellationAfterRootExitWitness = afterRootExit; cancellationAfterObservedOutputForTesting.Cancel(); // Cancel the exact caller only after the controlled real-output witness.
                cancellationAfterObservedOutputForTesting = null; // Ensure this stdout witness requests caller cancellation only once.
            } // Collect actual bounded output with cancellable asynchronous pipe reads.
        } // Collect actual bounded output with cancellable asynchronous pipe reads.
        if (consumeConfiguration) configuration?.FinishOutput(); // Only actual stdout EOF validates the whole fixed dialogue.
        return bytes.ToArray(); // Return the complete admitted stream bytes after real EOF.
        } // A successful returned copy survives until transfer or the owner's original-primary throw cleanup.
        catch (Exception error) // Observe the real read/reservation/parser error before mandatory cancellation changes another task's result.
        { if (!(error is OperationCanceledException && token.IsCancellationRequested)) configuration?.ObserveSourceFailure(error); throw; } // Owned cleanup-only cancellation remains secondary; a real earlier I/O/protocol cause cannot become a private-token OCE.
        finally { if (configuration is not null) { Array.Clear(chunk); Array.Clear(bytes.GetBuffer()); } } // Clear the actual owned chunk and full collector capacity without touching returned/retained successful copies.
    } // Collect actual bounded output with cancellable asynchronous pipe reads.
    private static async Task WriteAsync(SafeFileHandle handle, byte[] input, CancellationToken token) // Write only this owner's finite input through its cancellable pipe.
    { using var stream = new FileStream(handle, FileAccess.Write, 1024, true); await stream.WriteAsync(input, token).ConfigureAwait(false); } // Write only this owner's finite input through its cancellable pipe.
    private static async Task WriteConfigurationAsync(SafeFileHandle handle, CodeDocumentationNativeCodexInstalledConfigWire wire, Func<bool> current, CancellationToken callerToken, CancellationToken cleanupToken) // Own the same stdin endpoint under original caller and independent cleanup lifetimes.
    { using var stream = new FileStream(handle, FileAccess.Write, 1024, true); await WriteConfigurationAsync(stream, wire, current, callerToken, cleanupToken).ConfigureAwait(false); } // The production handle wrapper calls the same data-only writer exercised by causal stream fixtures.
    internal static Task WriteConfigurationAsync(Stream stream, CodeDocumentationNativeCodexInstalledConfigWire wire, Func<bool> current, CancellationToken callerToken, CancellationToken cleanupToken) // Share fixed protocol writing, never installed process/permission construction.
        => wire.WriteAsync(stream, current, callerToken, cleanupToken); // No private cleanup token is substituted for the actual caller token.
    internal static string Quote(string value) // Encode one literal Windows argument without changing its content.
    { // Encode one literal Windows argument without changing its content.
        if (value.IndexOf('\0') >= 0) throw new ArgumentException("NUL is not a Windows process argument."); // Reject NUL because it cannot represent one literal Windows argument.
        if (value.Length != 0 && !value.Any(c => char.IsWhiteSpace(c) || c == '"')) return value; // Keep a literal argument unchanged only when Windows quoting is unnecessary.
        var text = new StringBuilder("\""); int slash = 0; // Begin quoted-argument construction and count pending backslashes.
        foreach (var c in value) // Preserve every argument character while applying Windows backslash/quote rules.
        { if (c == '\\') { slash++; continue; } text.Append('\\', c == '"' ? slash * 2 + 1 : slash); slash = 0; text.Append(c); } // Escape only backslashes that precede a quote, preserving the literal argument content.
        return text.Append('\\', slash * 2).Append('"').ToString(); // Double trailing backslashes before the closing Windows argument quote.
    } // Encode one literal Windows argument without changing its content.
    internal sealed class DrainBudget(int maximum) // Share finite diagnostic byte accounting with data-only real-I/O fixtures; this object issues no process authority.
    { // Share the finite diagnostic byte budget between stdout and stderr.
        private int used; // Count actual stdout and stderr bytes with shared atomic accounting.
        internal int Maximum => maximum is > 0 and <= 65536 ? maximum : throw new IOException("Configuration drain bound is invalid."); // Configuration buffers use only the unchanged finite production diagnostic ceiling.
        internal bool CancellationOutputWitness; // Retain the fact that a real stdout read requested caller cancellation.
        internal bool CancellationAfterRootExitWitness; // Record the causal late-delivery control independently from token state.
        internal bool Overflow => Volatile.Read(ref used) > maximum; // Read shared overflow state safely across the two async readers.
        internal bool Reserve(int count) => Interlocked.Add(ref used, count) <= maximum; // Atomically charge each actual read against the unchanged finite byte maximum.
    } // Share the finite diagnostic byte budget between stdout and stderr.
    private sealed class ProcessLifetime(CodeDocumentationNativeCodexWindowsProcess owner) : IDisposable // Release the retained thread, root process and Job handles on every exit.
    { internal CodeDocumentationNativeCodexWindowsProcess Owner => owner; public void Dispose() { try { owner.thread?.Dispose(); } finally { try { owner.process?.Dispose(); } finally { owner.job.Dispose(); } } } } // Release the retained thread, root process and Job handles on every exit.
    private sealed class Pipe : IDisposable // Keep parent overlapped pipe endpoints separate from inherited child endpoints.
    { // Keep parent overlapped pipe endpoints separate from inherited child endpoints.
        internal SafeFileHandle Read, Write; // Keep the exact parent and child endpoints until detached or disposed.
        private Pipe(SafeFileHandle read, SafeFileHandle write) { Read = read; Write = write; } // Retain the matching connected pipe endpoints in one cleanup owner.
        internal static Pipe Create(bool input) // Keep parent overlapped pipe endpoints separate from inherited child endpoints.
        { // Keep parent overlapped pipe endpoints separate from inherited child endpoints.
            var security = new Security { Length = (uint)Marshal.SizeOf<Security>(), Inherit = true }; // Allow inheritance only for the fixed child pipe endpoints.
            var name = "\\\\.\\pipe\\xiigen-native-" + Guid.NewGuid().ToString("N"); // Create a unique local first-instance pipe name for this single owner.
            // Parent endpoints are overlapped: cancellation never depends on interrupting a synchronous managed thread.
            var parent = new SafeFileHandle(CheckHandle(CreateNamedPipeW(name, (input ? 2u : 1u) | 0x40000000 | 0x80000, // Create an overlapped parent pipe with the fixed input/output direction.
                8, 1, 4096, 4096, 0, IntPtr.Zero), "CreateNamedPipeW"), true); // Use byte-stream pipes and fixed buffers without remote pipe clients.
            SafeFileHandle? child = null; // Track any child endpoint acquired before a later pipe setup failure.
            try // Keep parent overlapped pipe endpoints separate from inherited child endpoints.
            { // Keep parent overlapped pipe endpoints separate from inherited child endpoints.
                child = new SafeFileHandle(CheckHandle(CreateFileW(name, input ? 0x80000000u : 0x40000000u, // Open the matching local child endpoint with only its required stream access.
                    0, ref security, 3, 0, IntPtr.Zero), "CreateFileW pipe child"), true); // Keep the child endpoint inheritable and open only this exact existing pipe.
                // Opening the client endpoint connected the local first-instance pipe. A null-overlapped
                // connection attempt is deliberately avoided on the asynchronous server handle.
                return input ? new(child, parent) : new(parent, child); // Orient input/output endpoints so only the child side is inherited.
            } // Keep parent overlapped pipe endpoints separate from inherited child endpoints.
            catch { child?.Dispose(); parent.Dispose(); throw; } // Close both endpoints when connected-pipe construction fails.
        } // Keep parent overlapped pipe endpoints separate from inherited child endpoints.
        internal void MakeReadPrivate() => Check(SetHandleInformation(Read, 1, 0), "SetHandleInformation read"); // Remove inheritance from the parent stdout/stderr read endpoint.
        internal void MakeWritePrivate() => Check(SetHandleInformation(Write, 1, 0), "SetHandleInformation write"); // Remove inheritance from the parent stdin write endpoint.
        internal SafeFileHandle DetachRead() { var result = Read; Read = new SafeFileHandle(IntPtr.Zero, true); return result; } // Move the parent read handle into explicit async-task ownership.
        internal SafeFileHandle DetachWrite() { var result = Write; Write = new SafeFileHandle(IntPtr.Zero, true); return result; } // Move the parent write handle into explicit async-task ownership.
        public void Dispose() { try { Read.Dispose(); } finally { Write.Dispose(); } } // Release both original pipe endpoints independently.
    } // Keep parent overlapped pipe endpoints separate from inherited child endpoints.
    private sealed class Attributes : IDisposable // Bind only this Job and the three fixed standard handles into process creation.
    { // Bind only this Job and the three fixed standard handles into process creation.
        internal IntPtr Pointer; // Keep the retained native process attribute allocation until creation ends.
        private IntPtr jobs, handles, securityEnvironment; // Stable HANDLE storage survives suspended create/resume and the whole RunAsync scope.
        private bool initialized; // Delete the native attribute list only after successful initialization.
        internal Attributes(SafeFileHandle job, SafeFileHandle[] inherited, CodeDocumentationNativeCodexWindowsSecurityEnvironment? protection)
        { // Bind only this Job and the three fixed standard handles into process creation.
            try // Bind only this Job and the three fixed standard handles into process creation.
            { // Bind only this Job and the three fixed standard handles into process creation.
                var count = protection is null ? 2 : 3;
                nuint size = 0; var sized = InitializeProcThreadAttributeList(IntPtr.Zero, count, 0, ref size); // Ask Windows for the exact bounded process-attribute allocation size.
                if (sized || Marshal.GetLastWin32Error() != 122 || size is 0 or > 1048576) throw new IOException("Invalid process attribute sizing."); // Refuse unexpected attribute sizing errors or unbounded native allocation.
                Pointer = Marshal.AllocHGlobal(checked((int)size)); // Allocate only the size verified from the native attribute API.
                Check(InitializeProcThreadAttributeList(Pointer, count, 0, ref size), "InitializeProcThreadAttributeList"); initialized = true;
                jobs = Marshal.AllocHGlobal(IntPtr.Size); Marshal.WriteIntPtr(jobs, job.DangerousGetHandle()); // Store this retained Job handle in the atomic Job-list attribute.
                handles = Marshal.AllocHGlobal(IntPtr.Size * inherited.Length); // Allocate exactly the three allowed inherited standard-handle slots.
                for (var i = 0; i < inherited.Length; i++) Marshal.WriteIntPtr(handles, i * IntPtr.Size, inherited[i].DangerousGetHandle()); // Copy only the fixed child standard endpoints into the inherited-handle array.
                Check(UpdateProcThreadAttribute(Pointer, 0, 0x2000D, jobs, (nuint)IntPtr.Size, IntPtr.Zero, IntPtr.Zero), "UpdateProcThreadAttribute JOB_LIST"); // Bind this Job atomically into suspended process creation.
                Check(UpdateProcThreadAttribute(Pointer, 0, 0x20002, handles, (nuint)(IntPtr.Size * inherited.Length), IntPtr.Zero, IntPtr.Zero), "UpdateProcThreadAttribute HANDLE_LIST"); // Restrict process inheritance to the three fixed child standard endpoints.
                if (protection is not null)
                {
                    securityEnvironment = Marshal.AllocHGlobal(IntPtr.Size); Marshal.WriteIntPtr(securityEnvironment, protection.Handle);
                    if (!UpdateProcThreadAttribute(Pointer, 0, 0x20023, securityEnvironment, (nuint)IntPtr.Size, IntPtr.Zero, IntPtr.Zero))
                        throw new CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal("UpdateProcThreadAttribute SECURITY_ENVIRONMENT", Marshal.GetLastWin32Error());
                } // An admission failure throws before CreateProcess; there is no unprotected retry.
            } // Bind only this Job and the three fixed standard handles into process creation.
            catch { Dispose(); throw; } // Release partial attribute allocations before preserving the original native failure.
        } // Bind only this Job and the three fixed standard handles into process creation.
        public void Dispose() // Release the retained native attribute list and allocation buffers once creation is complete.
        { if (initialized) { initialized = false; DeleteProcThreadAttributeList(Pointer); } Marshal.FreeHGlobal(Pointer); Pointer = IntPtr.Zero; Marshal.FreeHGlobal(jobs); jobs = IntPtr.Zero; Marshal.FreeHGlobal(handles); handles = IntPtr.Zero; Marshal.FreeHGlobal(securityEnvironment); securityEnvironment = IntPtr.Zero; }
    } // Bind only this Job and the three fixed standard handles into process creation.
    private static void Check(bool result, string operation) { if (!result) throw new Win32Exception(Marshal.GetLastWin32Error(), operation + " failed."); } // Preserve the exact native operation error for failed Windows calls.
    private static IntPtr CheckHandle(IntPtr handle, string operation) { if (handle == IntPtr.Zero || handle == new IntPtr(-1)) throw new Win32Exception(Marshal.GetLastWin32Error(), operation + " failed."); return handle; } // Reject absent or invalid native handles before wrapping ownership.
    [StructLayout(LayoutKind.Sequential)] private struct Security { internal uint Length; internal IntPtr Descriptor; [MarshalAs(UnmanagedType.Bool)] internal bool Inherit; } // Represent pipe endpoint inheritance without rewriting application or Session ACLs.
    [StructLayout(LayoutKind.Sequential)] private struct BasicLimits { internal long ProcessTime, JobTime; internal uint Flags; internal nuint Minimum, Maximum; internal uint ActiveLimit; internal nuint Affinity; internal uint Priority, Scheduling; } // Represent the fixed kill-on-close Job flags without admitting process breakaway.
    [StructLayout(LayoutKind.Sequential)] private struct IoCounters { internal ulong ReadOperations, WriteOperations, OtherOperations, ReadBytes, WriteBytes, OtherBytes; } // Preserve the full native Job limit structure layout without inventing I/O budget controls.
    [StructLayout(LayoutKind.Sequential)] private struct ExtendedLimits { internal BasicLimits Basic; internal IoCounters Io; internal nuint ProcessMemory, JobMemory, PeakProcessMemory, PeakJobMemory; } // Represent the Windows Job limit envelope whose flags are read back before creation.
    [StructLayout(LayoutKind.Sequential)] private struct AccountingInfo { internal long UserTime, KernelTime, PeriodUserTime, PeriodKernelTime; internal uint PageFaults, Total, Active, Terminated; } // Total is cumulative lifetime Job membership; only Active==0 certifies current whole-tree quiescence.
    [StructLayout(LayoutKind.Sequential)] private struct Startup { internal uint Size; internal IntPtr Reserved, Desktop, Title; internal uint X, Y, XSize, YSize, XChars, YChars, Fill, Flags; internal ushort Show, ReservedSize; internal IntPtr ReservedBytes, Input, Output, Error; } // Represent the child's three fixed standard handles in native process startup.
    [StructLayout(LayoutKind.Sequential)] private struct StartupEx { internal Startup Startup; internal IntPtr Attributes; } // Carry the fixed Job and handle attribute list alongside native startup.
    [StructLayout(LayoutKind.Sequential)] private struct Information { internal IntPtr Process, Thread; internal uint ProcessId, ThreadId; } // Receive the exact created root/thread handles and PID from Windows.
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateJobObjectW(IntPtr security, string? name); // Acquire the private Job that owns this process tree before any OS create.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetInformationJobObject(SafeFileHandle job, int kind, ref ExtendedLimits limits, uint size); // Apply kill-on-close limits to the retained Job without breakaway.
    [DllImport("kernel32.dll", EntryPoint = "QueryInformationJobObject", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool QueryLimits(SafeFileHandle job, int kind, out ExtendedLimits limits, uint size, out uint length); // Read back exact Job limits before accepting its native ownership boundary.
    [DllImport("kernel32.dll", EntryPoint = "QueryInformationJobObject", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool QueryAccounting(SafeFileHandle job, int kind, out AccountingInfo value, uint size, out uint length); // Read actual total and active owned processes for quiescence verification.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool TerminateJobObject(SafeFileHandle job, uint exit); // Terminate only the retained Job and all its owned descendants.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool TerminateProcess(SafeFileHandle process, uint exit); // Close only an owned suspended root whose atomic Job admission was refused.
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateNamedPipeW(string name, uint openMode, uint pipeMode, uint instances, uint outputSize, uint inputSize, uint timeout, IntPtr security); // Create the fixed-direction overlapped parent endpoint for cancellable I/O.
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateFileW(string name, uint access, uint share, ref Security security, uint creation, uint flags, IntPtr template); // Open only the matching local child endpoint of this owner's named pipe.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetHandleInformation(SafeFileHandle handle, uint mask, uint flags); // Remove inheritance from parent-only pipe endpoints.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool InitializeProcThreadAttributeList(IntPtr list, int count, uint flags, ref nuint size); // Size or initialize the two native creation attributes for Job and fixed handles.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UpdateProcThreadAttribute(IntPtr list, uint flags, nuint attribute, IntPtr value, nuint size, IntPtr previous, IntPtr returned); // Bind the retained Job or fixed inherited handle list into suspended creation.
    [DllImport("kernel32.dll")] private static extern void DeleteProcThreadAttributeList(IntPtr list); // Destroy only the initialized native creation attribute list before freeing its buffers.
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CreateProcessW(string image, StringBuilder command, IntPtr processSecurity, IntPtr threadSecurity, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint flags, IntPtr environment, string directory, ref StartupEx startup, out Information information); // Create one root suspended with atomic Job assignment and only fixed inherited standard handles.
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint ResumeThread(SafeFileHandle thread); // Resume the verified suspended root only after current-input and token checks.
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(SafeFileHandle process, uint milliseconds); // Read the exact root handle completion state without conflating it with descendant closure.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetExitCodeProcess(SafeFileHandle process, out uint exit); // Read the completed owned root's actual native exit code.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetProcessTimes(SafeFileHandle process, out long birth, out long exit, out long kernel, out long user); // Read the native root birth time used with its PID as exact process identity.
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool IsProcessInJob(SafeFileHandle process, SafeFileHandle job, [MarshalAs(UnmanagedType.Bool)] out bool assigned); // Confirm the suspended created root actually belongs to the retained Job.
} // End the private finite Windows process owner without exposing selection authority.
