// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Encodings.Web;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;
using TrainableRouter.Training;
using NativeConfigResolution = TrainableRouter.Training.CodeDocumentationNativeCodexInstalledConfigWire.Resolution; // Data-only retained resolution; this alias constructs no installed authority.
using TrainingLoop.Optimizers;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace LlmFromScratch.Tests;

[Collection(ModelServerContinuousLearningEnvironmentCollection.Name)]
public sealed class CodeDocumentationPreparedBaselineTests(ITestOutputHelper output)
{
    [Fact]
    public void PsecWire()
    {
        // Independent literal decoding follows the pinned Microsoft schema, not the producer's offsets/constants.
        var wire = CodeDocumentationNativeCodexWindowsSecurityEnvironment.EncodeResourcePolicy(@"C:\s04-public-fixture\session", @"C:\s04-public-fixture\release", @"C:\Windows\System32");
        ushort U16(int p) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(wire.AsSpan(p, 2));
        uint U32(int p) => System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(wire.AsSpan(p, 4));
        int Field(int table, int slot) { var v = table - checked((int)U32(table)); return U16(v + 4 + slot * 2) is var n && n != 0 ? table + n : 0; }
        int Target(int p) => p + checked((int)U32(p));
        string[] Strings(int p)
        {
            var v = Target(p); var values = new string[checked((int)U32(v))];
            for (var i = 0; i < values.Length; i++) { var text = Target(v + 4 + i * 4); var n = checked((int)U32(text)); Assert.Equal(0, wire[text + 4 + n]); values[i] = new UTF8Encoding(false, true).GetString(wire, text + 4, n); }
            return values;
        }
        Assert.Equal("PSEC", Encoding.ASCII.GetString(wire, 4, 4)); var root = checked((int)U32(0));
        Assert.Equal((ushort)1, U16(Field(root, 0))); Assert.Equal((ushort)0, U16(Field(root, 0) + 2));
        foreach (var slot in new[] { 1, 2, 3, 4 }) Assert.Equal(0, Field(root, slot));
        Assert.Equal(new[] { @"C:\s04-public-fixture\session", @"C:\s04-public-fixture\release", @"C:\Windows\System32" }, Strings(Field(root, 5)));
        Assert.Equal(new[] { @"C:\s04-public-fixture\session\native-codex-selection.request" }, Strings(Field(root, 6)));
        var network = Target(Field(root, 7)); Assert.Equal(0, Field(network, 0)); Assert.Equal(0, Field(network, 2));
        var egress = Target(Field(network, 1)); Assert.NotEqual(0, Field(egress, 0)); Assert.Equal(0, wire[Field(egress, 0)]);
        Assert.Equal(0, Field(egress, 1)); Assert.Equal(0, Field(egress, 2)); Assert.InRange(wire.Length, 8, 65536);
        // Full non-BMP UTF8 survives; expected bytes are independently specified.
        var unicode = CodeDocumentationNativeCodexWindowsSecurityEnvironment.EncodeResourcePolicy(@"C:\s04\😀", @"C:\release", @"C:\Windows\System32");
        Assert.True(unicode.AsSpan().IndexOf(new byte[] { 0xf0, 0x9f, 0x98, 0x80 }) >= 0);
    }

    [Fact]
    public void PsecBounds()
    {
        foreach (var root in new[] { "relative", @"C:\session\..\other", "C:\\session\0suffix", "C:\\session\\" + '\ud800', @"C:\" + new string('x', 65536) })
            Assert.ThrowsAny<IOException>(() => CodeDocumentationNativeCodexWindowsSecurityEnvironment.EncodeResourcePolicy(root, @"C:\release", @"C:\Windows\System32")); // PathTooLongException is an actual before-API IOException refusal, not a different permission outcome.
        Assert.Throws<IOException>(() => CodeDocumentationNativeCodexWindowsSecurityEnvironment.EncodeResourcePolicy(@"C:\session", @"c:\SESSION", @"C:\Windows\System32"));
        Assert.Equal(1u, CodeDocumentationNativeCodexWindowsProcess.CheckResumeOutcome(1, 12345)); // A stale supplied native code has no meaning for a successful count1 return.
        foreach (var count in new[] { 0u, 2u, uint.MaxValue - 1 })
        {
            IOException Denied(Func<uint, int, uint> classify) => Assert.Throws<IOException>(() => classify(count, 12345)); // An all-counts-normal implementation cannot satisfy this exact deny assertion.
            var guard = Denied(CodeDocumentationNativeCodexWindowsProcess.CheckResumeOutcome);
            Assert.Equal("ResumeThread previous suspend count=" + count.ToString(System.Globalization.CultureInfo.InvariantCulture) + "; expected=1; cause-source=ResumeThread-return.", guard.Message);
            Assert.NotNull(Record.Exception(() => Denied((_, _) => 1))); // Literal permissive counterfactual fails the same denied-count assertion; no physical suspend operation is reproduced.
            using var canceled = new CancellationTokenSource(); canceled.Cancel();
            var captured = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(guard);
            var secondary = new CodeDocumentationNativeCodexWindowsProcess.OwnedStreamCleanup([], [], true, true, true, "literal-unrelated-cleanup");
            var resolved = CodeDocumentationNativeCodexWindowsProcess.ResolveFinalPrimary(captured, canceled.Token, secondary);
            Assert.Same(captured, resolved); Assert.Same(guard, resolved!.SourceException); Assert.Equal("literal-unrelated-cleanup", guard.Data["NativeProcessStreams"]);
            Assert.Same(guard, Record.Exception(() => CodeDocumentationNativeCodexWindowsProcess.RethrowAfterPrivateOutputCleanup(resolved, null, [], []))); // The existing production arbitration/rethrow preserves the guard ahead of literal cleanup and late cancellation.
        }
        foreach (var nativeError in new[] { 0, 12345 })
        {
            var failure = Assert.Throws<System.ComponentModel.Win32Exception>(() => CodeDocumentationNativeCodexWindowsProcess.CheckResumeOutcome(uint.MaxValue, nativeError));
            Assert.Equal(nativeError, failure.NativeErrorCode); Assert.Equal("ResumeThread failed.", failure.Message); // Supplied literal codes test data arbitration, never an actual GetLastError observation.
        }
        output.WriteLine("RESUME-COUNT data-only: count1 accepted; 0/2/4294967294 refused; MAX literal native codes0/12345 preserved; same production primary/rethrow retained ahead of literal cleanup/cancellation; no physical count or cleanup fault reproduced.");
    }

    [Fact]
    public void PsecLifetime()
    {
        using var canceled = new CancellationTokenSource(); canceled.Cancel(); var original = new OperationCanceledException(canceled.Token);
        foreach (var mode in new[] { "query-error", "missing-bit", "create-error", "null-handle", "cancel", "success" })
        {
            var events = new List<string>(); int createCount = 0;
            using var api = new CodeDocumentationNativeCodexWindowsSecurityEnvironment.NativeLifetime(
                (out ulong flags) => { events.Add("query"); flags = mode == "missing-bit" ? 2UL : 1UL; return mode == "query-error" ? unchecked((int)0x80070005) : 0; },
                (byte[] bytes, uint size, uint flags, out IntPtr handle) => { events.Add("create"); createCount++; Assert.Equal(0u, flags); Assert.Equal(8u, size); handle = mode == "null-handle" ? IntPtr.Zero : new IntPtr(42); if (mode == "cancel") throw original; return mode == "create-error" ? unchecked((int)0x80004005) : 0; },
                handle => { Assert.Equal(new IntPtr(42), handle); events.Add("close"); }, () => events.Add("unload"));
            try
            {
                if (mode is "query-error" or "missing-bit")
                { var refusal = Assert.Throws<CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal>(api.RequireDenySupport); Assert.Equal((int?)(mode == "query-error" ? unchecked((int)0x80070005) : 0), refusal.NativeResult); }
                else
                {
                    api.RequireDenySupport();
                    if (mode == "cancel") { var error = Assert.Throws<OperationCanceledException>(() => api.Create(new byte[8])); Assert.Same(original, error); Assert.Equal(canceled.Token, error.CancellationToken); }
                    else if (mode is "create-error" or "null-handle") Assert.Throws<CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal>(() => api.Create(new byte[8]));
                    else { api.Create(new byte[8]); Assert.Equal(new IntPtr(42), api.Handle); }
                    Assert.Throws<InvalidOperationException>(() => api.Create(new byte[8])); Assert.Equal(1, createCount);
                }
            }
            finally { api.Dispose(); api.Dispose(); }
            Assert.Equal("unload", events[^1]); Assert.Single(events.Where(e => e == "unload"));
            if (mode is "create-error" or "success" or "cancel") { Assert.Equal("close", events[^2]); Assert.Single(events.Where(e => e == "close")); }
            else Assert.DoesNotContain("close", events);
        }
        var originalClose = new IOException("literal close failure"); var cleanupOrder = new List<string>();
        var failedCleanup = new CodeDocumentationNativeCodexWindowsSecurityEnvironment.NativeLifetime(
            (out ulong flags) => { flags = 1; return 0; }, (byte[] wire, uint size, uint flags, out IntPtr handle) => { handle = new IntPtr(43); return 0; },
            handle => { cleanupOrder.Add("close"); throw originalClose; }, () => { cleanupOrder.Add("unload"); throw new IOException("literal later unload failure"); });
        failedCleanup.Create(new byte[8]); Assert.Same(originalClose, Assert.Throws<IOException>(failedCleanup.Dispose));
        Assert.Equal(new[] { "close", "unload" }, cleanupOrder); Assert.True(originalClose.Data.Contains("SecurityEnvironmentModuleCleanup")); failedCleanup.Dispose();
        foreach (var primary in new Exception[] { new IOException("literal acquisition primary"), original })
        {
            var released = new List<string>();
            CodeDocumentationNativeCodexWindowsSecurityEnvironment.CloseOwners(primary,
                new PsecCleanupProbe(() => { released.Add("environment"); throw new IOException("literal environment cleanup"); }),
                new PsecCleanupProbe(() => { released.Add("image"); throw new IOException("literal image cleanup"); }));
            Assert.Equal(new[] { "environment", "image" }, released); Assert.True(primary.Data.Contains("SecurityEnvironmentOwnerCleanup0")); Assert.True(primary.Data.Contains("SecurityEnvironmentOwnerCleanup1"));
            if (primary is OperationCanceledException oce) { Assert.Same(original, oce); Assert.Equal(canceled.Token, oce.CancellationToken); }
        }
        var firstRelease = new IOException("literal first environment release"); var imageReleased = false;
        Assert.Same(firstRelease, Assert.Throws<IOException>(() => CodeDocumentationNativeCodexWindowsSecurityEnvironment.CloseOwners(null,
            new PsecCleanupProbe(() => throw firstRelease), new PsecCleanupProbe(() => { imageReleased = true; throw new IOException("literal later image release"); }))));
        Assert.True(imageReleased); Assert.True(firstRelease.Data.Contains("SecurityEnvironmentOwnerCleanup1"));

        // F01: causal custody/mapping uses the exact production helper, never a fabricated MainInputs/permission owner.
        var failedProbe = new CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal("literal protected membership refusal", unchecked((int)0x80070005));
        var opaqueSecondary = new object();
        var expectedData = new Dictionary<string, object>
        {
            ["NativeProcessRootPid"] = 321u, ["NativeProcessRootBirthFileTime"] = 123456789L,
            ["NativeProcessJobAssigned"] = false, ["NativeProcessTreeClosed"] = true,
            ["NativeProcessRootExitObserved"] = false, ["NativeProcessExitCode"] = 0u,
            ["NativeProcessStreamTasksTerminal"] = true, ["NativeProcessTotalProcesses"] = 0u,
            ["NativeProcessFailure"] = "literal membership refusal", ["NativeProcessPriorFailure"] = "literal earlier observation",
            ["NativeProcessStreams"] = "literal independent drain cleanup", ["NativeProcessCleanup"] = opaqueSecondary,
            ["NativeResourceProtectionCleanup"] = "literal post-process protection cleanup"
        };
        foreach (var pair in expectedData) failedProbe.Data[pair.Key] = pair.Value;
        static void VerifyCapturedFailure(CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome data, Exception expected)
        { Assert.Same(expected, data.OriginalFailure); Assert.NotNull(data.OriginalFailure); Assert.Same(expected.Data, data.OriginalFailure.Data); }
        using (var messageOnly = new CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome())
        {
            // Independent negative control reproduces old mapping's information loss, not an old genuine Session execution.
            var oldMapping = OperationResult<CodeDocumentationNativeCodexLaunchInputOwner>.Fail("required-owner=main-Codex-instruction-config-MCP-envelope; protected-RESOURCE-unavailable; " + failedProbe.Message, CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable);
            Assert.False(oldMapping.IsSuccess); Assert.Equal(CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable, oldMapping.ErrorCode);
            Assert.NotNull(Record.Exception(() => VerifyCapturedFailure(messageOnly, failedProbe)));
            output.WriteLine("F01 RED control: message-only mapping fails original-exception custody oracle; data-only control, not old genuine Session runtime.");
        }
        var refusalCustody = new CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome();
        var mapped = refusalCustody.CaptureRefusalAndMap(failedProbe);
        Assert.False(mapped.IsSuccess); Assert.Equal(CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable, mapped.ErrorCode);
        Assert.Equal(CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome.OutcomeState.Refusal, refusalCustody.State);
        Assert.Null(refusalCustody.Normal); VerifyCapturedFailure(refusalCustody, failedProbe);
        Assert.Equal((int?)unchecked((int)0x80070005), Assert.IsType<CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal>(refusalCustody.OriginalFailure).NativeResult);
        Assert.Equal(expectedData.Count, refusalCustody.OriginalFailure!.Data.Count);
        foreach (var pair in expectedData) Assert.Same(pair.Value, refusalCustody.OriginalFailure.Data[pair.Key]);
        Assert.Throws<InvalidOperationException>(() => refusalCustody.CaptureFailure(failedProbe));
        refusalCustody.Dispose(); refusalCustody.Dispose();
        Assert.Equal(CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome.OutcomeState.Disposed, refusalCustody.State);
        Assert.Null(refusalCustody.OriginalFailure); Assert.Null(refusalCustody.Normal);
        Assert.Equal(expectedData.Count, failedProbe.Data.Count); foreach (var pair in expectedData) Assert.Same(pair.Value, failedProbe.Data[pair.Key]);
        Assert.Throws<InvalidOperationException>(() => refusalCustody.CaptureRefusalAndMap(failedProbe));

        foreach (var failed in new Exception[] { original, new IOException("literal protected generic primary") })
        {
            using var failureCustody = new CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome();
            var exactSecondary = new object(); failed.Data["literal-final-cleanup"] = exactSecondary;
            void ProductionStyleRethrow() { try { throw failed; } catch (Exception error) { failureCustody.CaptureFailure(error); throw; } }
            Assert.Same(failed, Record.Exception(ProductionStyleRethrow)); VerifyCapturedFailure(failureCustody, failed);
            Assert.Null(failureCustody.Normal); Assert.Same(exactSecondary, failureCustody.OriginalFailure!.Data["literal-final-cleanup"]);
            Assert.Equal(failed is OperationCanceledException ? CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome.OutcomeState.Canceled : CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome.OutcomeState.Failure, failureCustody.State);
            if (failed is OperationCanceledException oce) { Assert.Same(original, oce); Assert.Equal(canceled.Token, oce.CancellationToken); }
            failureCustody.Dispose(); Assert.Null(failureCustody.OriginalFailure); Assert.Same(exactSecondary, failed.Data["literal-final-cleanup"]);
        }
        var normalBytes = Encoding.ASCII.GetBytes("literal-version-output"); var normalErrors = Encoding.ASCII.GetBytes("literal-terminal-error");
        var normalProbe = new CodeDocumentationNativeCodexWindowsProcess.Observation(12, 34, 1, normalBytes, normalErrors, 1, true, false, "literal normal failure observation", true);
        var normalCustody = new CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome(); normalCustody.CaptureNormal(normalProbe);
        Assert.Same(normalProbe, normalCustody.Normal); Assert.Null(normalCustody.OriginalFailure);
        Assert.Equal(CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome.OutcomeState.Normal, normalCustody.State);
        Assert.Throws<InvalidOperationException>(() => normalCustody.CaptureNormal(normalProbe));
        normalCustody.CaptureFailure(original); // A later original currentness/cancellation failure must not discard already received normal bytes.
        Assert.Same(normalProbe, normalCustody.Normal); VerifyCapturedFailure(normalCustody, original);
        normalCustody.Dispose(); Assert.All(normalBytes, value => Assert.Equal((byte)0, value)); Assert.All(normalErrors, value => Assert.Equal((byte)0, value));
        Assert.Null(normalCustody.Normal); Assert.Null(normalCustody.OriginalFailure); Assert.Equal(canceled.Token, original.CancellationToken);
        output.WriteLine("F01 GREEN: production capture/mapping preserves whole original Refusal/Data, generic primary/OCE identity/token, normal/failure distinction and disposal detachment.");

        // Actual readonly FileStream failures exercise the same production acquisition/classification seam.
        var moduleFixtureRoot = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "xiigen-s04-module-absence-" + Guid.NewGuid().ToString("N")));
        if (Directory.Exists(moduleFixtureRoot)) throw new IOException("The unique public module fixture root already exists.");
        Directory.CreateDirectory(moduleFixtureRoot);
        var publicImage = Path.Combine(moduleFixtureRoot, "public-image.fixture");
        try
        {
            var paths = new[] { Path.Combine(moduleFixtureRoot, "absent-module.dll"), Path.Combine(moduleFixtureRoot, "absent-parent", "processmodel.dll") };
            for (var index = 0; index < paths.Length; index++)
            {
                var laterStageEntered = false;
                var refused = Assert.Throws<CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal>(() =>
                { using var image = CodeDocumentationNativeCodexWindowsSecurityEnvironment.OpenModuleImageForRetention(paths[index]); laterStageEntered = true; });
                Assert.False(laterStageEntered); // No export/query/create/environment step follows the actual absent-file acquisition.
                var cause = refused.InnerException;
                if (index == 0) Assert.IsType<FileNotFoundException>(cause); else Assert.IsType<DirectoryNotFoundException>(cause);
                Assert.NotNull(cause); Assert.Equal(cause.HResult, refused.HResult); Assert.Equal((int?)cause.HResult, refused.NativeResult);
                Assert.Contains("cause-source=FileStream-acquisition", refused.Message); Assert.Contains("original-hresult=0x" + unchecked((uint)cause.HResult).ToString("X8"), refused.Message);
                Assert.DoesNotContain("QueryProcessSecurityEnvironmentSupport", refused.Message); Assert.DoesNotContain("CreateProcessSecurityEnvironment", refused.Message);
                var causeDataSentinel = new object(); cause.Data["literal-cause-evidence"] = causeDataSentinel;
                var secondClassification = Assert.IsType<CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal>(CodeDocumentationNativeCodexWindowsSecurityEnvironment.ClassifyModuleImageAcquisition(cause));
                Assert.Same(cause, secondClassification.InnerException);
                using var custody = new CodeDocumentationNativeCodexInstalledMainInputs.ProtectedProbeOutcome(); var absenceMapping = custody.CaptureRefusalAndMap(refused);
                Assert.False(absenceMapping.IsSuccess); Assert.Equal(CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable, absenceMapping.ErrorCode);
                Assert.Same(refused, custody.OriginalFailure); Assert.Same(cause, custody.OriginalFailure!.InnerException); Assert.Same(causeDataSentinel, custody.OriginalFailure.InnerException!.Data["literal-cause-evidence"]);
                custody.Dispose(); Assert.Null(custody.OriginalFailure); Assert.Same(causeDataSentinel, refused.InnerException!.Data["literal-cause-evidence"]);
                output.WriteLine("MODULE-ABSENCE actual {0}; HResult=0x{1}; original cause retained; later-stage-entered={2}; data-only fixture", cause.GetType().Name, unchecked((uint)cause.HResult).ToString("X8"), laterStageEntered);
            }
            // A present public literal is readonly file data, never a module/permission availability certificate.
            using (var file = new FileStream(publicImage, FileMode.CreateNew, FileAccess.Write, FileShare.Read)) { file.Write("PUBLIC-PSEC-IMAGE"u8); file.Flush(true); }
            using (var retained = CodeDocumentationNativeCodexWindowsSecurityEnvironment.OpenModuleImageForRetention(publicImage))
            { Assert.True(retained.CanRead); Assert.False(retained.CanWrite); Assert.Equal(17, retained.Length); }
            var unexpectedAcquisition = Record.Exception(() => { using var image = CodeDocumentationNativeCodexWindowsSecurityEnvironment.OpenModuleImageForRetention(moduleFixtureRoot); });
            Assert.NotNull(unexpectedAcquisition); Assert.IsNotType<CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal>(unexpectedAcquisition);
            Assert.Same(unexpectedAcquisition, CodeDocumentationNativeCodexWindowsSecurityEnvironment.ClassifyModuleImageAcquisition(unexpectedAcquisition));
            foreach (var unexpected in new Exception[] { new IOException("literal unrelated IO fault"), new UnauthorizedAccessException("literal access fault"), original })
            { Assert.Same(unexpected, CodeDocumentationNativeCodexWindowsSecurityEnvironment.ClassifyModuleImageAcquisition(unexpected)); if (unexpected is OperationCanceledException oce) Assert.Equal(canceled.Token, oce.CancellationToken); }
            output.WriteLine("MODULE-ABSENCE negative control: actual {0} kept as original unexpected acquisition fault; no native/environment/model calls", unexpectedAcquisition.GetType().Name);
        }
        finally
        {
            var boundary = moduleFixtureRoot + Path.DirectorySeparatorChar;
            if (!Path.GetFullPath(publicImage).StartsWith(boundary, StringComparison.OrdinalIgnoreCase)) throw new IOException("Public module fixture cleanup boundary mismatch.");
            File.Delete(publicImage); Directory.Delete(moduleFixtureRoot); // Exact owned leaf/root only; no glob, recursive delete or System32 mutation.
        }
    }

    private sealed class PsecCleanupProbe(Action release) : IDisposable { public void Dispose() => release(); }

    [Fact]
    public async Task PsecPhysical()
    {
        if (!OperatingSystem.IsWindows()) { await Assert.ThrowsAsync<PlatformNotSupportedException>(() => CodeDocumentationNativeCodexWindowsProcess.RunResourceProtectionForTestingAsync(CancellationToken.None)); return; }
        CodeDocumentationNativeCodexWindowsProcess.ProtectionFixtureObservation data;
        try { data = await CodeDocumentationNativeCodexWindowsProcess.RunResourceProtectionForTestingAsync(CancellationToken.None); }
        catch (CodeDocumentationNativeCodexWindowsSecurityEnvironment.Refusal refusal)
        {
            output.WriteLine("PSEC unsupported/refused: {0}; positive protection and Session join NOT_ESTABLISHED", refusal.Message);
            foreach (var key in new[] { "NativeProcessRootPid", "NativeProcessRootBirthFileTime", "NativeProcessJobAssigned", "NativeProcessTreeClosed", "NativeProcessStreamTasksTerminal", "NativeProcessTotalProcesses", "NativeProcessExitCode" })
                if (refusal.Data.Contains(key)) output.WriteLine("{0}={1}", key, refusal.Data[key]);
            if (refusal.Data.Contains("NativeProcessJobAssigned")) { Assert.Equal(false, refusal.Data["NativeProcessJobAssigned"]); Assert.Equal(true, refusal.Data["NativeProcessTreeClosed"]); Assert.Equal(true, refusal.Data["NativeProcessStreamTasksTerminal"]); }
            Assert.NotEmpty(refusal.Operation); return;
        }
        var process = data.Process; output.WriteLine("PSEC physical stdout={0}; stderr={1}; PID={2}; birth={3}; total={4}", Encoding.UTF8.GetString(process.StandardOutput), Encoding.UTF8.GetString(process.StandardError), process.RootPid, process.RootBirthFileTime, process.TotalProcesses);
        Assert.True(data.ParentReadAndWriteEstablished); Assert.True(data.OriginalsCurrent); Assert.True(data.RootWriteAbsent); Assert.True(data.ChildWriteAbsent);
        Assert.True(process.TreeClosed); Assert.True(process.StreamTasksTerminal); Assert.True(process.RootExitObserved); Assert.False(process.DescendantsAfterRoot); Assert.Null(process.Failure); Assert.Equal(0u, process.ExitCode); Assert.True(process.TotalProcesses >= 2);
        var stdout = Encoding.ASCII.GetString(process.StandardOutput);
        Assert.Equal(2, stdout.Split("S04-ALLOWED-PUBLIC", StringSplitOptions.None).Length - 1);
        Assert.DoesNotContain("S04-EXPLICIT-PRIVATE-PUBLIC-LITERAL", stdout); Assert.DoesNotContain("S04-DEFAULT-PRIVATE-PUBLIC-LITERAL", stdout);
        foreach (var who in new[] { "ROOT", "CHILD" }) foreach (var attempt in new[] { "EXPLICIT", "DEFAULT", "WRITE" })
        { var prefix = who + "-" + attempt + "-EXIT="; var line = Assert.Single(stdout.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(line => line.StartsWith(prefix, StringComparison.Ordinal))); Assert.True(int.TryParse(line[prefix.Length..], out var exit)); Assert.NotEqual(0, exit); }
        Assert.NotEmpty(process.StandardError); // Record observed shell errors; do not invent Win32 ERROR_ACCESS_DENIED from missing bytes.
    }

    private sealed class ProtectedDisclosureHarness : IDisposable
    {
        internal OrdinaryDiagnosticHarness H { get; }
        internal CodeDocumentationOneLessonStartupSelectionTests.TeachingDisclosureProcessEnvironment Environment { get; private set; } = null!;
        internal ContinuousLearningModelServerStartup Startup => H.Startup;
        private readonly bool controlledNativeParent;
        private bool cleanNativePrerequisiteVerified;
        internal ProtectedDisclosureHarness(bool callback = false, Func<string,string>? alter = null, bool nativePreparation = false, string? nativeCapacityTask = null, bool nativeSelectedStudent = false, bool oversizedNativePreparation = false, bool rawUtf8References = false)
        {
            try
            {
                controlledNativeParent = nativeSelectedStudent || nativePreparation || nativeCapacityTask is not null; // Only the intended native positive families opt into the genuine configured parent.
                H = new((_,_) => (false,"missing_essential_behavior"), f => rawUtf8References ? FullMaterialFixtureCurriculum(SelectionCategories(f.SealGenuineTeaching("student-task-branches-raw",2))) : nativeSelectedStudent ? FullMaterialFixtureCurriculum(SelectionCategories(f.SealGenuineTeaching("student-task-branches",2))) : nativeCapacityTask is not null ? NativeCapacityCurriculum(f, nativeCapacityTask) : nativePreparation ? oversizedNativePreparation ? OversizedNativePreparationCurriculum(f) : NativePreparationCurriculum(f) : FullMaterialFixtureCurriculum(SelectionCategories(f.SealGenuineTeaching("material-unicode",4))),
                    configureGenuineParent: controlledNativeParent ? CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.ConfigureSelectedTaskParent : null,
                    syntheticSelectedStudentMode: nativeSelectedStudent,
                    startupFactory: f =>
                    {
                        var json = CodeDocumentationOneLessonStartupSelectionTests.TeachingDisclosureFixtureLiteral(f, rawUtf8References);
                        Environment = new(alter?.Invoke(json) ?? json, f);
                        if (callback) return ContinuousLearningModelServerStartup.Create(System.Environment.GetEnvironmentVariable);
                        using var services = ModelServer.Program.CreateLocalLoopbackBuilder().Services.BuildServiceProvider();
                        return services.GetRequiredService<ContinuousLearningModelServerStartup>();
                    });
            }
            catch { Environment?.Dispose(); throw; }
        }
        internal Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.TeachingDisclosureInspection>> Inspect(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b, CancellationToken token = default)
            => H.Open.Session.InspectStartupApprovedTeachingSelectionMaterialsAsync(b, Startup, H.Fixture.OrdinarySelector, token);
        internal Task<OperationResult<CodeDocumentationNativeCodexSelectionHost.NativeSelectionPreparationInspection>> PrepareNative(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b, CancellationToken token = default)
        {
            if (controlledNativeParent && !cleanNativePrerequisiteVerified)
            {
                var approval = Startup.ResolveProtectedTeachingDisclosureApproval(H.Grant);
                CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.AssertProtectedCleanOriginal(H.Fixture, b.Readback.Baseline, [H.Client],
                    approval.IsSuccess ? approval.Value!.ExposedMetadata() : null);
                cleanNativePrerequisiteVerified = true; // Later deliberate mutations exercise production refusals without this positive-control assertion replacing them.
            }
            return H.Open.Session.CaptureStartupApprovedNativeSelectionAsync(b, Startup, H.Fixture.OrdinarySelector, token);
        }
        public void Dispose() { try { H.Dispose(); } finally { Environment.Dispose(); } }
    }

    internal static CodeDocumentationOneLessonTeachingCurriculumV2 NativePreparationCurriculum(
        CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture fixture)
        => fixture.SealGenuineNativePreparation();

    private static CodeDocumentationOneLessonTeachingCurriculumV2 OversizedNativePreparationCurriculum(
        CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture fixture)
    {
        // Declare three eligible Unicode cells and the full unused second SOURCE before any grant freezes them.
        var complete = SelectionCategories(fixture.SealGenuineTeaching("material-unicode", 4));
        var descriptor = complete.Sources[0];
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(fixture.RepositoryRoot, descriptor.DomainSlug, descriptor.SubjectSlug, descriptor.CorpusVersion).Value!;
        var entries = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address).Value!.Corpus.Entries;
        var first = complete.Items.Where(i => i.SourceId == descriptor.SourceId).ToArray();
        var extra = entries.First(e => first.All(i => i.ReferenceEntryId != e.EntryId));
        var third = new CodeDocumentationOneLessonTeachingItemV2("item-teach-7-third", descriptor.SourceId, extra.EntryId, extra.SourceEntityFingerprint);
        var eligible = first.Append(third).Select(i => i.ItemId).ToArray();
        // The unused SOURCE still needs its declared items. Removing those links while retaining its descriptor
        // makes the genuine startup graph invalid before any protected/native owner can be reached.
        var items = complete.Items.Append(third).ToArray();
        return complete with { Items = items, CategoryBindings = [
            new("missing_essential_behavior", eligible),
            new("invented_behavior", complete.CategoryBindings[1].OrderedItemIds.Append(third.ItemId).ToArray())] };
    }

    private static (byte[] Request, byte[] Schema) NativePreparationBytes(CodeDocumentationOneLessonSession session)
        => (((byte[])FreshSessionField(session, "_nativePreparedRequestBytes")!).ToArray(), ((byte[])FreshSessionField(session, "_nativePreparedSchemaBytes")!).ToArray());

    private static void AssertNativePreparationReleased(CodeDocumentationOneLessonSession session)
    {
        AssertDisclosureReleased(session); Assert.Null(FreshSessionField(session, "_nativePreparationReceipt"));
        Assert.Null(FreshSessionField(session, "_nativePreparationNonce")); Assert.Equal(false, FreshSessionField(session, "_nativePreparationIssuing"));
        Assert.Null(FreshSessionField(session, "_nativeLaunchBinding")); Assert.Null(FreshSessionField(session, "_nativeLaunchInputs")); Assert.Null(FreshSessionField(session, "_nativeLaunchHost")); // New launch ownership expires before CAS release too.
        Assert.Null(FreshSessionField(session, "_syntheticNativeLaunchDirectory")); Assert.Equal(false, FreshSessionField(session, "_nativeLaunchIssuing")); // Fixture scope and issuer cannot survive the operation.
    } // Finish this bounded owner check without renewing authority or an attempt.

    private static string CreateNativeStartedFixtureInputs(ProtectedDisclosureHarness p) // Create clearly labelled durable fixture input files.
    { // Create clearly labelled durable fixture input files.
        var directory = Path.Combine(p.H.Fixture.Root, "synthetic-native-started-inputs"); Directory.CreateDirectory(directory); // The existing unique owned lane isolates each harness/fault and includes these inputs in exact fixture cleanup.
        foreach (var (name, value) in new[] { ("synthetic-image.input", "fixture-image-v1"), ("synthetic-config.input", "fixture-config-v1"), ("synthetic-permission.input", "fixture-no-native-permission") }) // Create clearly labelled durable fixture input files.
        { using var file = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write, FileShare.Read, FileOptions.WriteThrough); file.Write(Encoding.UTF8.GetBytes(value)); file.Flush(true); } // Use actual owner-only files and complete durable fixture inputs.
        return directory; // A directory string alone still cannot mint a launch binding.
    } // Finish this bounded owner check without renewing authority or an attempt.

    private static byte[] ReadNativeStartedFixtureEvidence(string path)
    {
        // Observe actual physical evidence independently while the Session retains its RW custody writer.
        path = Path.GetFullPath(path);
        Assert.False(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path));
        using var file = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.Open,
            FileAccess.Read, FileShare.ReadWrite, FileOptions.SequentialScan);
        Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(file.SafeFileHandle, path));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path));
        var timestamp = File.GetLastWriteTimeUtc(path);
        Assert.InRange(file.Length, 0L, (long)CodeDocumentationNativeCodexSelectionWire.StartedMaximumBytes);
        byte[] bytes;
        if (file.Length == 0)
        {
            // Empty interrupted evidence is observable here, but remains invalid to every product decoder/reader.
            file.Position = 0; Assert.Equal(-1, file.ReadByte()); Assert.Equal(0L, file.Length);
            file.Position = 0; Assert.Equal(-1, file.ReadByte()); Assert.Equal(0L, file.Length);
            bytes = [];
        }
        else
        {
            var stable = SecureLocalFileBoundary.ReadBoundedStableBytes(file, CodeDocumentationNativeCodexSelectionWire.StartedMaximumBytes);
            Assert.NotNull(stable); bytes = stable!; // Two complete bounded physical reads must agree, including EOF and length.
        }
        Assert.Equal(bytes.LongLength, file.Length);
        Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(file.SafeFileHandle, path));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path));
        Assert.False(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path));
        Assert.Equal(timestamp, File.GetLastWriteTimeUtc(path));
        return bytes;
    }

    [Fact] // Discover this bounded regression through the existing xUnit project.
    public async Task NativeStarted_UnknownInstalledPermissionPreservesPreparedUnsent() // Verify the actual NativeStarted_UnknownInstalledPermissionPreservesPreparedUnsent boundary.
    { // Verify the actual NativeStarted_UnknownInstalledPermissionPreservesPreparedUnsent boundary.
        using var p = new ProtectedDisclosureHarness(nativePreparation: true); var h = p.H; var baseline = (await h.Consume()).Value!; Assert.NotNull(baseline); // Reach the genuine current protected preparation path.
        var first = await p.PrepareNative(baseline); Assert.True(first.IsSuccess, first.ErrorCode); Assert.Equal(CodeDocumentationNativeCodexLaunchInputOwner.PermissionUnavailable, first.Value!.LaunchBoundaryErrorCode); // Unknown installed enforcement refuses only the next boundary.
        Assert.NotNull(first.Value.LaunchBoundaryDetail);
        if (h.Open.Session.LastInstalledNativeResourceObservationForTesting is { } resource)
        {
            Assert.True(resource.TreeClosed); Assert.Null(resource.Failure); Assert.True(resource.RootPid > 0); Assert.True(resource.RootBirthFileTime > 0);
            Assert.Equal(0u, resource.ExitCode); Assert.Empty(resource.StandardError);
            Assert.StartsWith("codex-cli ", Encoding.UTF8.GetString(resource.StandardOutput));
            Assert.Contains("required-owner=main-Codex-instruction-config-MCP-envelope", first.Value.LaunchBoundaryDetail);
        }
        var saved = NativePreparationBytes(h.Open.Session); var path = Path.Combine(Path.GetDirectoryName(h.Fixture.OrdinaryHistory)!, CodeDocumentationNativeCodexSelectionWire.StartedFileName); // Inspect actual live storage, not inspection metadata alone.
        Assert.False(File.Exists(path)); Assert.Equal((0, 0, false), h.Open.Session.NativeStartedAccounting); AssertNativePreparationReleased(h.Open.Session); // No Started/process reservation was made.
        var again = await p.PrepareNative(baseline); Assert.True(again.IsSuccess, again.ErrorCode); Assert.True(again.Value!.Replayed); Assert.Equal(saved.Request, NativePreparationBytes(h.Open.Session).Request); Assert.Equal(saved.Schema, NativePreparationBytes(h.Open.Session).Schema); // Same-byte preparation replay retains the original nonce.
        Assert.False(File.Exists(path)); AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); // No ordinary native effect, model, Teacher or optimizer is fabricated.
    } // Finish this bounded owner check without renewing authority or an attempt.

    [Fact]
    public void NativeMainInputs_CompletePrivateDiskBytesAndAbsentCandidatesRemainHeld()
    {
        using var fixture = new JournalStorageFixture();
        var request = Path.Combine(fixture.ModelVersionRoot, "main-request.fixture");
        var empty = Path.Combine(fixture.ModelVersionRoot, "main-empty-config.fixture");
        var absent = Path.Combine(fixture.ModelVersionRoot, "main-absent-instruction.fixture");
        // Opaque private config bytes must survive custody unchanged, including Unicode and non-UTF8.
        byte[] complete = [0xff, 0, .. Encoding.UTF8.GetBytes("fixture-private-value-עברית-😀\nfull-suffix")];
        WriteMainInputFixture(request, complete); WriteMainInputFixture(empty, []);
        using var held = CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open([
            new(request, true, true, 1024), new(empty, false, true, 1024), new(absent, false, true, 1024)], CancellationToken.None);
        Assert.True(held.IsCurrent()); Assert.Equal(3, held.Count); Assert.Equal(complete.Length, held.TotalBytes);
        var copy = held.CopyBytes(request); Assert.Equal(complete, copy); copy[0] = 1;
        Assert.Equal(complete, held.CopyBytes(request)); Assert.Empty(held.CopyBytes(empty));
        Assert.Throws<IOException>(() => held.CopyBytes(absent)); // Explicit absence is retained, never replaced by an empty/opened input.
        Assert.True(held.IsCurrent()); Assert.False(File.Exists(absent));
        Assert.Equal(complete, File.ReadAllBytes(request)); // No custody operation rewrites a private source or emits its values.
    }

    [Fact]
    public void NativeMainInputs_ChangedAppearedForeignAndDisposedInputsNeverRenew()
    {
        foreach (var fault in new[] { "bytes", "timestamp", "appeared", "disposed" })
        {
            using var fixture = new JournalStorageFixture();
            var original = Path.Combine(fixture.ModelVersionRoot, "main-original.fixture");
            var absent = Path.Combine(fixture.ModelVersionRoot, "main-absent.fixture");
            byte[] complete = Encoding.UTF8.GetBytes("same-size-complete-input"); WriteMainInputFixture(original, complete);
            using var held = CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open([
                new(original, true, true, 1024), new(absent, false, true, 1024)], CancellationToken.None);
            Assert.True(held.IsCurrent()); var modified = File.GetLastWriteTimeUtc(original);
            Assert.Throws<IOException>(() => held.CopyBytes(Path.Combine(fixture.ModelVersionRoot, "foreign.fixture")));
            Assert.True(held.IsCurrent()); // A foreign copy request grants no bytes and does not renew/change the actual owner.
            if (fault == "bytes")
            {
                var changed = complete.ToArray(); changed[^1] ^= 1;
                using (var writer = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(original, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.WriteThrough))
                { writer.Write(changed); writer.Flush(true); }
                File.SetLastWriteTimeUtc(original, modified); // Same length/timestamp still cannot hide changed whole bytes.
            }
            else if (fault == "timestamp") File.SetLastWriteTimeUtc(original, modified.AddSeconds(3));
            else if (fault == "appeared") WriteMainInputFixture(absent, Encoding.UTF8.GetBytes("late-instruction"));
            else held.Dispose();
            Assert.False(held.IsCurrent()); Assert.Throws<IOException>(() => held.CopyBytes(original));
            if (fault == "bytes")
            {
                using var writer = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(original, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.WriteThrough);
                writer.Write(complete); writer.Flush(true); File.SetLastWriteTimeUtc(original, modified);
            }
            else if (fault == "timestamp") File.SetLastWriteTimeUtc(original, modified);
            else if (fault == "appeared") File.Delete(absent); // Restore the exact fixture absence; it cannot renew the observed-invalid owner.
            Assert.False(held.IsCurrent()); // Restoration/disposal never revives the original observed-invalid owner.
        }
    }

    [Fact]
    public void NativeMainInputs_FailedAcquisitionClosesEarlierFilesAndPreservesCancellation()
    {
        using var fixture = new JournalStorageFixture();
        var first = Path.Combine(fixture.ModelVersionRoot, "main-first.fixture");
        var missing = Path.Combine(fixture.ModelVersionRoot, "main-required-missing.fixture");
        byte[] complete = Encoding.UTF8.GetBytes("complete-retained-first-file"); WriteMainInputFixture(first, complete);
        Assert.Throws<IOException>(() => CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open([
            new(first, true, true, 1024), new(missing, true, true, 1024)], CancellationToken.None));
        using (var exclusive = new FileStream(first, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        { Assert.Equal(complete.Length, exclusive.Length); }
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var thrown = Assert.Throws<OperationCanceledException>(() => CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open([
            new(first, true, true, 1024)], cancellation.Token));
        Assert.Equal(cancellation.Token, thrown.CancellationToken); Assert.Equal(complete, File.ReadAllBytes(first));
    }

    [Fact]
    public async Task NativeConfig_CompleteCorrelatedDialogueRetainsWholePrivateResolution()
    {
        using var fixture = new JournalStorageFixture(); var root = fixture.ModelVersionRoot;
        using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new MemoryStream();
        var writer = wire.WriteAsync(input, () => true, CancellationToken.None);
        Assert.DoesNotContain("config/read", Encoding.UTF8.GetString(input.ToArray()));
        var initialize = LiteralNativeConfigInitialize(root); wire.Accept(initialize.AsSpan(0, 7)); wire.Accept(initialize.AsSpan(7));
        await wire.ConfigurationRequestSent.WaitAsync(TimeSpan.FromSeconds(2));
        var actual = Encoding.UTF8.GetString(input.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, actual.Length);
        using (var first = JsonDocument.Parse(actual[0])) { Assert.Equal(1, first.RootElement.GetProperty("id").GetInt32()); Assert.Equal("initialize", first.RootElement.GetProperty("method").GetString()); }
        using (var notification = JsonDocument.Parse(actual[1])) { Assert.Equal("initialized", notification.RootElement.GetProperty("method").GetString()); Assert.Single(notification.RootElement.EnumerateObject()); }
        using (var request = JsonDocument.Parse(actual[2])) { Assert.Equal(2, request.RootElement.GetProperty("id").GetInt32()); Assert.Equal("config/read", request.RootElement.GetProperty("method").GetString()); Assert.Equal(root, request.RootElement.GetProperty("params").GetProperty("cwd").GetString()); Assert.True(request.RootElement.GetProperty("params").GetProperty("includeLayers").GetBoolean()); }
        var response = LiteralNativeConfigResponse(root); var unicode = response.AsSpan().IndexOf(new byte[] { 0xf0, 0x9f, 0x98, 0x80 });
        Assert.True(unicode > 0); wire.Accept(response.AsSpan(0, unicode + 1)); wire.Accept(response.AsSpan(unicode + 1));
        await CompleteLiteralNativePrerequisites(wire);
        await writer.WaitAsync(TimeSpan.FromSeconds(2)); wire.FinishOutput(); using var resolution = wire.TakeResolution();
        var copy = resolution.CopyConfigurationResponse(); Assert.Equal(response[..^1], copy);
        using (var json = JsonDocument.Parse(copy)) { var result = json.RootElement.GetProperty("result"); Assert.Equal("whole-עברית-😀-suffix", result.GetProperty("config").GetProperty("unknown_private").GetProperty("value").GetString()); Assert.Equal(8, result.GetProperty("layers").GetArrayLength()); }
        Assert.Equal(root, resolution.CodexHome);
        Assert.Equal(new[] { "packaged.toml", "system.toml", "legacy.toml", "user.toml", "project/config.toml" }.Select(name => Path.GetFullPath(Path.Combine(root, name))).Order(), resolution.ConfigurationPaths.Order());
        copy[0] = 0; Assert.Equal(response[..^1], resolution.CopyConfigurationResponse());
        Assert.Equal(new[] { "initialize", "initialized", "config/read" }, actual.Select(line => JsonNode.Parse(line)!["method"]!.GetValue<string>())); // The actual writer sends no thread, turn, model or tool operation.
    }

    [Fact]
    public async Task NativeConfig_ProtocolFaultsRejectCompletePartialAndUnrelatedOutput()
    {
        using var fixture = new JournalStorageFixture(); var root = fixture.ModelVersionRoot;
        var valid = Encoding.UTF8.GetString(LiteralNativeConfigResponse(root));
        foreach (var fault in new[] { "wrong-id", "duplicate-id", "duplicate-config", "error", "extra-method", "partial", "missing-layer", "null-layer-config", "invalid-utf8", "overbound", "extra-frame" })
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new MemoryStream();
            var writer = wire.WriteAsync(input, () => true, CancellationToken.None); wire.Accept(LiteralNativeConfigInitialize(root));
            await wire.ConfigurationRequestSent.WaitAsync(TimeSpan.FromSeconds(2));
            var changed = fault switch {
                "wrong-id" => valid.Replace("\"id\":2", "\"id\":3", StringComparison.Ordinal),
                "duplicate-id" => valid.Replace("\"id\":2", "\"id\":2,\"id\":2", StringComparison.Ordinal),
                "duplicate-config" => valid.Replace("\"config\":{\"unknown_private\"", "\"config\":{\"repeat\":1,\"repeat\":2,\"unknown_private\"", StringComparison.Ordinal),
                "error" => "{\"id\":2,\"error\":{\"code\":-1,\"message\":\"private-not-logged\"}}\n",
                "extra-method" => valid.Replace("\"id\":2", "\"method\":\"turn/start\",\"id\":2", StringComparison.Ordinal),
                "partial" => valid[..^1], "missing-layer" => "{\"id\":2,\"result\":{\"config\":{},\"origins\":{}}}\n",
                "null-layer-config" => valid.Replace("\"config\":{\"unknown_layer\":\"whole-private-layer-suffix\"}", "\"config\":null", StringComparison.Ordinal),
                "extra-frame" => valid + "{\"method\":\"thread/started\",\"params\":{}}\n", _ => valid };
            var bytes = fault == "invalid-utf8" ? new byte[] { 0xff, 10 } : fault == "overbound" ? new byte[65537] : Encoding.UTF8.GetBytes(changed);
            var primary = Record.Exception(() => { wire.Accept(bytes); if (fault == "partial") wire.FinishOutput(); }); Assert.NotNull(primary);
            var writerError = await Record.ExceptionAsync(() => writer.WaitAsync(TimeSpan.FromSeconds(2)));
            if (writerError is not null) Assert.Same(primary, writerError); // A late extra frame may follow writer completion, but its parser failure still forbids resolution.
            Assert.Same(primary, wire.Primary!.SourceException); Assert.Throws<IOException>(() => wire.TakeResolution());
            Assert.DoesNotContain("thread/", Encoding.UTF8.GetString(input.ToArray())); Assert.DoesNotContain("turn/", Encoding.UTF8.GetString(input.ToArray()));
        }
        // Unsolicited initialize/config bytes cannot complete a real dialogue that has not sent config/read.
        using var unrequested = new CodeDocumentationNativeCodexInstalledConfigWire(root);
        Assert.Throws<IOException>(() => unrequested.Accept(LiteralNativeConfigInitialize(root))); Assert.Throws<IOException>(() => unrequested.TakeResolution());
    }

    [Fact]
    public async Task NativeConfig_ResponseDuringConfigurationFlushWaitsForCompleteWriter()
    {
        using var fixture = new JournalStorageFixture(); var root = fixture.ModelVersionRoot;
        using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root);
        using var input = new ConfigResponseBeforeFlushStream(() => wire.Accept(LiteralNativeConfigResponse(root)), () => wire.Accept(LiteralNativeSandboxReadiness()));
        var writer = wire.WriteAsync(input, () => true, CancellationToken.None); wire.Accept(LiteralNativeConfigInitialize(root));
        await input.ConfigurationFlushStarted.Task.WaitAsync(TimeSpan.FromSeconds(2)); // The literal server reply is already parsed while the actual config request flush remains pending.
        Assert.False(writer.IsCompleted); Assert.False(wire.ConfigurationRequestSent.IsCompleted);
        Assert.Throws<IOException>(() => wire.TakeResolution()); // Response/EOF cannot manufacture completion before the whole actual write/flush.
        input.ReleaseConfigurationFlush.TrySetResult(true);
        await wire.RequirementsRequestSent.WaitAsync(TimeSpan.FromSeconds(2)); wire.Accept(LiteralNativeRequirements());
        await wire.PermissionProfilesRequestSent.WaitAsync(TimeSpan.FromSeconds(2)); wire.Accept(LiteralNativePermissionProfiles());
        await input.ReadinessFlushStarted.Task.WaitAsync(TimeSpan.FromSeconds(2)); wire.FinishOutput();
        Assert.False(writer.IsCompleted); Assert.Throws<IOException>(() => wire.TakeResolution()); // All five responses/EOF still cannot skip the pending final flush.
        input.ReleaseReadinessFlush.TrySetResult(true); await writer.WaitAsync(TimeSpan.FromSeconds(2));
        using var resolution = wire.TakeResolution(); Assert.Equal(LiteralNativeConfigResponse(root)[..^1], resolution.CopyConfigurationResponse());
        var methods = Encoding.UTF8.GetString(input.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => JsonNode.Parse(line)!["method"]!.GetValue<string>());
        Assert.Equal(new[] { "initialize", "initialized", "config/read", "configRequirements/read", "permissionProfile/list", "windowsSandbox/readiness" }, methods); Assert.Null(wire.Primary);
    }

    private sealed class ConfigResponseBeforeFlushStream(Action response, Action readinessResponse) : MemoryStream
    {
        internal TaskCompletionSource<bool> ConfigurationFlushStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource<bool> ReleaseConfigurationFlush { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource<bool> ReadinessFlushStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource<bool> ReleaseReadinessFlush { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool configuration, readiness;
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await base.WriteAsync(buffer, cancellationToken);
            if (Encoding.UTF8.GetString(buffer.Span).Contains("\"method\":\"config/read\"", StringComparison.Ordinal)) { configuration = true; response(); }
            if (Encoding.UTF8.GetString(buffer.Span).Contains("\"method\":\"windowsSandbox/readiness\"", StringComparison.Ordinal)) { readiness = true; readinessResponse(); }
        }
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            if (configuration) { ConfigurationFlushStarted.TrySetResult(true); await ReleaseConfigurationFlush.Task.WaitAsync(cancellationToken); }
            if (readiness) { ReadinessFlushStarted.TrySetResult(true); await ReleaseReadinessFlush.Task.WaitAsync(cancellationToken); }
            await base.FlushAsync(cancellationToken);
        }
    }

    [Fact]
    public async Task NativeConfig_CurrentInputDriftAndCancellationSendNoLaterRequests()
    {
        using var fixture = new JournalStorageFixture(); var root = fixture.ModelVersionRoot;
        using (var canceled = new CancellationTokenSource())
        using (var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root))
        using (var input = new MemoryStream())
        {
            canceled.Cancel(); var error = await Assert.ThrowsAsync<OperationCanceledException>(() => wire.WriteAsync(input, () => true, canceled.Token));
            Assert.Equal(canceled.Token, error.CancellationToken); Assert.Same(error, wire.Primary!.SourceException); Assert.Empty(input.ToArray());
        }
        using (var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root))
        using (var input = new MemoryStream())
        {
            var current = true; var writer = wire.WriteAsync(input, () => current, CancellationToken.None);
            Assert.Contains("initialize", Encoding.UTF8.GetString(input.ToArray())); current = false; wire.Accept(LiteralNativeConfigInitialize(root));
            var error = await Assert.ThrowsAsync<IOException>(() => writer.WaitAsync(TimeSpan.FromSeconds(2)));
            Assert.Same(error, wire.Primary!.SourceException); Assert.DoesNotContain("initialized", Encoding.UTF8.GetString(input.ToArray())); Assert.DoesNotContain("config/read", Encoding.UTF8.GetString(input.ToArray()));
            current = true; Assert.Throws<IOException>(() => wire.TakeResolution()); // Restoring ambient input cannot revive the already failed original dialogue.
        }
        using (var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root))
        {
            var current = true;
            using var input = new DriftDuringInitializedFlushStream(() => current = false);
            var writer = wire.WriteAsync(input, () => current, CancellationToken.None); wire.Accept(LiteralNativeConfigInitialize(root));
            var error = await Assert.ThrowsAsync<IOException>(() => writer.WaitAsync(TimeSpan.FromSeconds(2)));
            Assert.True(input.DriftObserved); Assert.Same(error, wire.Primary!.SourceException);
            var methods = Encoding.UTF8.GetString(input.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => JsonNode.Parse(line)!["method"]!.GetValue<string>());
            Assert.Equal(new[] { "initialize", "initialized" }, methods); Assert.DoesNotContain("config/read", Encoding.UTF8.GetString(input.ToArray()));
            current = true; Assert.Throws<IOException>(() => wire.TakeResolution()); // A notification-flush drift cannot be repaired by restoring the fixture flag.
        }
    }

    private sealed class DriftDuringInitializedFlushStream(Action drift) : MemoryStream
    {
        private bool initialized;
        internal bool DriftObserved { get; private set; }
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        { await base.WriteAsync(buffer, cancellationToken); initialized = Encoding.UTF8.GetString(buffer.Span) == "{\"method\":\"initialized\"}\n"; }
        public override async Task FlushAsync(CancellationToken cancellationToken)
        { if (initialized) { drift(); DriftObserved = true; } await base.FlushAsync(cancellationToken); }
    }

    [Fact]
    public async Task NativeConfig_ResolvedFileCustodyPreservesAbsentAndChangedSources()
    {
        using var fixture = new JournalStorageFixture(); var root = fixture.ModelVersionRoot;
        var present = Path.Combine(root, "packaged.toml"); var absent = Path.Combine(root, "system.toml");
        byte[] whole = Encoding.UTF8.GetBytes("private-opaque-complete-😀-suffix"); WriteMainInputFixture(present, whole);
        using var resolution = await LiteralNativeConfigResolution(root);
        using var held = CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open(resolution.ConfigurationPaths.Select(path =>
            new CodeDocumentationNativeCodexInstalledMainInputs.DiskInput(path, false, true, 1024)), CancellationToken.None);
        Assert.True(held.IsCurrent()); Assert.Equal(5, held.Count); Assert.Equal(whole, held.CopyBytes(present)); Assert.Throws<IOException>(() => held.CopyBytes(absent));
        var timestamp = File.GetLastWriteTimeUtc(present); var changed = whole.ToArray(); changed[^1] ^= 1;
        using (var writer = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(present, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.WriteThrough)) { writer.Write(changed); writer.Flush(true); }
        File.SetLastWriteTimeUtc(present, timestamp); Assert.False(held.IsCurrent());
        using (var writer = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(present, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, FileOptions.WriteThrough)) { writer.Write(whole); writer.Flush(true); }
        File.SetLastWriteTimeUtc(present, timestamp); Assert.False(held.IsCurrent()); Assert.Throws<IOException>(() => held.CopyBytes(present));
        using var fresh = CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open(resolution.ConfigurationPaths.Select(path =>
            new CodeDocumentationNativeCodexInstalledMainInputs.DiskInput(path, false, true, 1024)), CancellationToken.None);
        Assert.True(fresh.IsCurrent()); WriteMainInputFixture(absent, Encoding.UTF8.GetBytes("appeared-after-resolution")); Assert.False(fresh.IsCurrent());
        Assert.Equal(whole, File.ReadAllBytes(present)); // Resolution/custody never writes configuration sources or restores their contents itself.
    }

    [Fact]
    public async Task NativeConfig_SourceIdentitiesRejectUnsafeAndUnknownPaths()
    {
        using var fixture = new JournalStorageFixture(); var root = fixture.ModelVersionRoot; var original = Encoding.UTF8.GetString(LiteralNativeConfigResponse(root));
        foreach (var changed in new[] {
            original.Replace("\"type\":\"packagedDefaults\"", "\"type\":\"unrecognizedSource\"", StringComparison.Ordinal),
            original.Replace(JsonSerializer.Serialize(Path.Combine(root, "packaged.toml")), "\"relative.toml\"", StringComparison.Ordinal),
            original.Replace(JsonSerializer.Serialize(Path.Combine(root, "packaged.toml")), JsonSerializer.Serialize(Path.Combine(root, "auth.json")), StringComparison.Ordinal),
            original.Replace("\"version\":\"v1\"", "\"version\":null", StringComparison.Ordinal) })
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new MemoryStream();
            var writer = wire.WriteAsync(input, () => true, CancellationToken.None); wire.Accept(LiteralNativeConfigInitialize(root)); await wire.ConfigurationRequestSent.WaitAsync(TimeSpan.FromSeconds(2));
            var error = Assert.Throws<IOException>(() => wire.Accept(Encoding.UTF8.GetBytes(changed)));
            Assert.Same(error, await Assert.ThrowsAsync<IOException>(() => writer.WaitAsync(TimeSpan.FromSeconds(2)))); Assert.Throws<IOException>(() => wire.TakeResolution());
        }
        Assert.False(File.Exists(Path.Combine(root, "auth.json"))); // No parser/source rejection opens, copies or fabricates authentication bytes.
    }

    [Fact]
    public void NativeConfig_AggregateCustodyBoundClosesPendingSources()
    {
        using var fixture = new JournalStorageFixture(); var path = Path.Combine(fixture.ModelVersionRoot, "resolved-bound.toml"); WriteMainInputFixture(path, [1, 2, 3]);
        CodeDocumentationNativeCodexInstalledMainInputs.DiskInput[] inputs = [new(path, false, true, 1024)];
        using (var valid = CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open(inputs, CancellationToken.None, 194, 8388605)) { Assert.True(valid.IsCurrent()); Assert.Equal(3, valid.TotalBytes); }
        Assert.Throws<IOException>(() => CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open(inputs, CancellationToken.None, 195, 0));
        Assert.Throws<IOException>(() => CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open(inputs, CancellationToken.None, 194, 8388606));
        using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None); Assert.Equal(3, exclusive.Length); // Failed combined-byte admission closes the actual already opened source handle.
    }

    [Fact]
    public void NativeMainInputs_CancellationAfterFirstAcquisitionClosesActualHandle()
    {
        using var fixture = new JournalStorageFixture(); using var cancellation = new CancellationTokenSource();
        var first = Path.Combine(fixture.ModelVersionRoot, "resolved-cancel-first.toml"); var second = Path.Combine(fixture.ModelVersionRoot, "resolved-cancel-second.toml");
        WriteMainInputFixture(first, [1, 2, 3]); WriteMainInputFixture(second, [4, 5, 6]); var reached = 0;
        var error = Assert.Throws<OperationCanceledException>(() => CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open([
            new(first, true, true, 1024), new(second, true, true, 1024)], cancellation.Token, afterFirstAcquisitionForTesting: () =>
            {
                reached++; Assert.Throws<IOException>(() => { using var denied = new FileStream(first, FileMode.Open, FileAccess.ReadWrite, FileShare.None); });
                cancellation.Cancel();
            }));
        Assert.Equal(1, reached); Assert.Equal(cancellation.Token, error.CancellationToken);
        using var releasedFirst = new FileStream(first, FileMode.Open, FileAccess.ReadWrite, FileShare.None); using var releasedSecond = new FileStream(second, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.Equal(3, releasedFirst.Length); Assert.Equal(3, releasedSecond.Length); // The controlled cancellation occurred after real first custody, and neither pending handle survives.
    }

    [Fact]
    public void NativeProcess_CheckedWaitPreservesSignaledTimeoutAndImmediateNativeError()
    {
        Assert.Equal(0u, CodeDocumentationNativeCodexWindowsProcess.CheckWaitOutcome(0, 123)); // A signaled root is independent of stale last-error data.
        Assert.Equal(258u, CodeDocumentationNativeCodexWindowsProcess.CheckWaitOutcome(258, 123)); // WAIT_TIMEOUT continues polling rather than fabricating completion.
        var failed = Assert.Throws<System.ComponentModel.Win32Exception>(() => CodeDocumentationNativeCodexWindowsProcess.CheckWaitOutcome(0xffffffff, 6));
        Assert.Equal(6, failed.NativeErrorCode); Assert.Throws<IOException>(() => CodeDocumentationNativeCodexWindowsProcess.CheckWaitOutcome(128, 0));
    }

    [Fact]
    public async Task NativeProcess_RealFailedWaitPreservesPrimaryThroughCleanupFailure()
    {
        if (!OperatingSystem.IsWindows())
        { await Assert.ThrowsAsync<PlatformNotSupportedException>(() => CodeDocumentationNativeCodexWindowsProcess.RunWaitFailureCanaryForTestingAsync(Path.GetTempPath(), true, CancellationToken.None)); return; }
        foreach (var failCleanup in new[] { false, true })
        {
            var error = await Assert.ThrowsAsync<System.ComponentModel.Win32Exception>(() => CodeDocumentationNativeCodexWindowsProcess.RunWaitFailureCanaryForTestingAsync(Path.GetTempPath(), failCleanup, CancellationToken.None));
            Assert.Equal(6, error.NativeErrorCode); Assert.Equal(true, error.Data["NativeProcessTreeClosed"]); Assert.Equal(true, error.Data["NativeProcessStreamTasksTerminal"]);
            Assert.True((uint)error.Data["NativeProcessRootPid"]! > 0); Assert.True((long)error.Data["NativeProcessRootBirthFileTime"]! > 0);
            Assert.Equal(failCleanup, error.Data.Contains("NativeProcessCleanup")); // An actual later cleanup denial attaches evidence but cannot replace the original WAIT_FAILED Win32 error.
        }
    }

    [Fact]
    public async Task NativeProcess_ConfigCurrentPrimarySurvivesLaterOwnedCleanupFault()
    {
        var primary = new IOException("literal current-input refusal before protocol write");
        if (!OperatingSystem.IsWindows()) { await Assert.ThrowsAsync<PlatformNotSupportedException>(() => CodeDocumentationNativeCodexWindowsProcess.RunConfigurationCurrentFailureCanaryForTestingAsync(Path.GetTempPath(), primary, true, CancellationToken.None)); return; }
        foreach (var failCleanup in new[] { false, true })
        {
            var original = new IOException("literal current-input refusal before protocol write");
            var error = await Assert.ThrowsAsync<IOException>(() => CodeDocumentationNativeCodexWindowsProcess.RunConfigurationCurrentFailureCanaryForTestingAsync(Path.GetTempPath(), original, failCleanup, CancellationToken.None));
            Assert.Same(original, error); Assert.Equal(failCleanup, error.Data.Contains("NativeProcessCleanup"));
            Assert.Equal(true, error.Data["NativeProcessTreeClosed"]); Assert.Equal(true, error.Data["NativeProcessStreamTasksTerminal"]);
            Assert.True((uint)error.Data["NativeProcessRootPid"]! > 0); Assert.True((long)error.Data["NativeProcessRootBirthFileTime"]! > 0);
        }
    }

    [Fact]
    public async Task NativeConfig_ConcurrentWriterAndParserFaultRetainOneOriginalPrimary()
    {
        using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(Path.GetTempPath());
        var writeFailure = new IOException("literal independently injected stdin failure");
        using var input = new ConcurrentConfigurationFailureStream(writeFailure);
        var writer = wire.WriteAsync(input, () => true, CancellationToken.None);
        await input.Entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var parser = Task.Run(() => Record.Exception(() => wire.Accept(new byte[] { 0xff, 10 })));
        input.Release.TrySetResult(true);
        var writeError = await Record.ExceptionAsync(() => writer.WaitAsync(TimeSpan.FromSeconds(2)));
        var parseError = await parser.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Same(writeFailure, writeError); Assert.NotNull(parseError);
        var winner = wire.Primary!.SourceException; Assert.True(ReferenceEquals(winner, writeFailure) || ReferenceEquals(winner, parseError));
        Assert.Throws<IOException>(() => wire.FinishOutput()); Assert.Same(winner, wire.Primary.SourceException); Assert.Throws<IOException>(() => wire.TakeResolution());
        Assert.Empty(input.ToArray()); // Competing failures do not permit a later request or any semantic method.
    }

    private sealed class ConcurrentConfigurationFailureStream(IOException failure) : MemoryStream
    {
        internal TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource<bool> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        { Entered.TrySetResult(true); await Release.Task.WaitAsync(cancellationToken); throw failure; }
    }

    [Fact]
    public async Task NativeConfig_ProductionWriterPreservesCallerCancellationDuringNotificationFlush()
    {
        using var caller = new CancellationTokenSource(); using var cleanup = new CancellationTokenSource();
        using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(Path.GetTempPath());
        using var input = new DriftDuringInitializedFlushStream(caller.Cancel);
        var writer = CodeDocumentationNativeCodexWindowsProcess.WriteConfigurationAsync(input, wire, () => true, caller.Token, cleanup.Token);
        wire.Accept(LiteralNativeConfigInitialize(Path.GetTempPath()));
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writer.WaitAsync(TimeSpan.FromSeconds(2)));
        Assert.Equal(caller.Token, error.CancellationToken); Assert.Same(error, wire.Primary!.SourceException); Assert.False(cleanup.IsCancellationRequested);
        Assert.Equal(new[] { "initialize", "initialized" }, Encoding.UTF8.GetString(input.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => JsonNode.Parse(line)!["method"]!.GetValue<string>()));
        var outputClosed = false; var errorClosed = false;
        var closure = await CodeDocumentationNativeCodexWindowsProcess.CloseOwnedStreamsForTestingAsync(Task.FromResult(Array.Empty<byte>()), Task.FromResult(Array.Empty<byte>()), writer, cleanup.Cancel, () => outputClosed = true, () => errorClosed = true, input.Dispose);
        Assert.True(closure.AllTerminal); Assert.True(outputClosed && errorClosed); Assert.False(input.CanWrite); Assert.True(cleanup.IsCancellationRequested);
        Assert.Same(error, CodeDocumentationNativeCodexWindowsProcess.ResolveFinalPrimary(wire.Primary, caller.Token, closure)!.SourceException);
        Assert.Throws<IOException>(() => wire.TakeResolution()); // Caller cancellation and real managed-stream closure issue no semantic permission.
    }

    [Fact]
    public async Task NativeConfig_InitializeIssuanceRejectsBlockedCurrentAndAcceptsFastDuplexReply()
    {
        using (var wire = new CodeDocumentationNativeCodexInstalledConfigWire(Path.GetTempPath()))
        using (var input = new MemoryStream())
        using (var entered = new ManualResetEventSlim())
        using (var release = new ManualResetEventSlim())
        {
            var writer = Task.Run(() => CodeDocumentationNativeCodexWindowsProcess.WriteConfigurationAsync(input, wire, () => { entered.Set(); if (!release.Wait(TimeSpan.FromSeconds(2))) throw new IOException("bounded fixture currentness wait expired"); return true; }, CancellationToken.None, CancellationToken.None));
            try
            {
                Assert.True(entered.Wait(TimeSpan.FromSeconds(2)));
                var primary = Assert.Throws<IOException>(() => wire.Accept(LiteralNativeConfigInitialize(Path.GetTempPath())));
                release.Set(); var error = await Assert.ThrowsAsync<IOException>(() => writer.WaitAsync(TimeSpan.FromSeconds(2)));
                Assert.Same(primary, error); Assert.Empty(input.ToArray()); Assert.Throws<IOException>(() => wire.TakeResolution());
            }
            finally { release.Set(); await Record.ExceptionAsync(() => writer.WaitAsync(TimeSpan.FromSeconds(2))); }
        }
        foreach (var replyDuringFlush in new[] { false, true })
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(Path.GetTempPath());
            using var input = new FastConfigurationReplyStream(wire, replyDuringFlush);
            await CodeDocumentationNativeCodexWindowsProcess.WriteConfigurationAsync(input, wire, () => true, CancellationToken.None, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2));
            wire.FinishOutput(); using var resolution = wire.TakeResolution(); Assert.Equal(LiteralNativeConfigResponse(Path.GetTempPath())[..^1], resolution.CopyConfigurationResponse());
            Assert.Equal(6, Encoding.UTF8.GetString(input.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries).Length); Assert.Null(wire.Primary);
        }
    }

    private sealed class FastConfigurationReplyStream(CodeDocumentationNativeCodexInstalledConfigWire wire, bool duringFlush) : MemoryStream
    {
        private byte[]? response;
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await base.WriteAsync(buffer, cancellationToken); using var json = JsonDocument.Parse(buffer[..^1]); var method = json.RootElement.GetProperty("method").GetString();
            response = method switch { "initialize" => LiteralNativeConfigInitialize(Path.GetTempPath()), "config/read" => LiteralNativeConfigResponse(Path.GetTempPath()), "configRequirements/read" => LiteralNativeRequirements(), "permissionProfile/list" => LiteralNativePermissionProfiles(), "windowsSandbox/readiness" => LiteralNativeSandboxReadiness(), _ => null };
            if (!duringFlush && response is { } frame) { wire.Accept(frame); response = null; }
        }
        public override async Task FlushAsync(CancellationToken cancellationToken)
        { if (response is { } frame) { wire.Accept(frame); response = null; } await base.FlushAsync(cancellationToken); }
    }

    [Fact]
    public async Task NativeConfig_DrainFailureSurvivesCleanupCancellationAndDenial()
    {
        foreach (var overflow in new[] { false, true })
        {
            using var cleanup = new CancellationTokenSource(); using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(Path.GetTempPath());
            using var input = new PendingInitializationFlushStream(); var writer = CodeDocumentationNativeCodexWindowsProcess.WriteConfigurationAsync(input, wire, () => true, CancellationToken.None, cleanup.Token);
            await input.Entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
            var literalFailure = new IOException("literal independent source read failure"); using var source = new SourceReadFailureStream(literalFailure);
            var budget = new CodeDocumentationNativeCodexWindowsProcess.DrainBudget(overflow ? 3 : 65536); byte[]? chunk = null, collector = null;
            var drain = CodeDocumentationNativeCodexWindowsProcess.DrainConfigurationStreamForTestingAsync(source, budget, wire, cleanup.Token, (a, b) => { chunk = a; collector = b; });
            try
            {
                var sourceError = await Assert.ThrowsAsync<IOException>(() => drain.WaitAsync(TimeSpan.FromSeconds(2)));
                if (!overflow) Assert.Same(literalFailure, sourceError); Assert.Equal(overflow, budget.Overflow); Assert.Same(sourceError, wire.Primary!.SourceException);
                Assert.All(chunk!, value => Assert.Equal((byte)0, value)); Assert.All(collector!, value => Assert.Equal((byte)0, value));
                var inputClosed = false;
                var closure = await CodeDocumentationNativeCodexWindowsProcess.CloseOwnedStreamsForTestingAsync(drain, Task.FromResult(Array.Empty<byte>()), writer, cleanup.Cancel, () => { source.Dispose(); throw new IOException("literal later stdout-close denial"); }, () => { }, () => { input.Dispose(); inputClosed = true; });
                Assert.True(closure.AllTerminal); Assert.True(inputClosed); Assert.False(source.CanRead); Assert.Contains("stdout-close:IOException", closure.Failure);
                var writeError = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writer); Assert.NotEqual(CancellationToken.None, writeError.CancellationToken);
                var primary = CodeDocumentationNativeCodexWindowsProcess.ResolveFinalPrimary(wire.Primary, CancellationToken.None, closure);
                Assert.Same(sourceError, primary!.SourceException); Assert.Contains("stdout-close:IOException", Assert.IsType<string>(sourceError.Data["NativeProcessStreams"]));
                Assert.DoesNotContain("config/read", Encoding.UTF8.GetString(input.ToArray())); Assert.Throws<IOException>(() => wire.TakeResolution());
            }
            finally { cleanup.Cancel(); await Record.ExceptionAsync(() => writer.WaitAsync(TimeSpan.FromSeconds(2))); }
        }
    }

    private sealed class PendingInitializationFlushStream : MemoryStream
    {
        internal TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async Task FlushAsync(CancellationToken cancellationToken) { Entered.TrySetResult(true); await release.Task.WaitAsync(cancellationToken); }
    }
    private sealed class SourceReadFailureStream(IOException failure) : MemoryStream
    {
        private bool read;
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        { if (read) throw failure; read = true; var prefix = Encoding.UTF8.GetBytes("{\"id\":1"); prefix.AsMemory().CopyTo(buffer); return ValueTask.FromResult(prefix.Length); }
    }

    [Fact]
    public async Task NativeConfig_PrivateDrainBuffersAndPrimaryCopiesClearWithoutLosingResolution()
    {
        using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(Path.GetTempPath()); using var input = new MemoryStream();
        var writer = CodeDocumentationNativeCodexWindowsProcess.WriteConfigurationAsync(input, wire, () => true, CancellationToken.None, CancellationToken.None);
        using var source = new ConfigurationTranscriptStream(wire); byte[]? chunk = null, collector = null;
        var drain = CodeDocumentationNativeCodexWindowsProcess.DrainConfigurationStreamForTestingAsync(source, new(65536), wire, CancellationToken.None, (a, b) => { chunk = a; collector = b; });
        var raw = await drain.WaitAsync(TimeSpan.FromSeconds(2)); await writer.WaitAsync(TimeSpan.FromSeconds(2)); using var resolution = wire.TakeResolution();
        Assert.Equal(LiteralNativeConfigInitialize(Path.GetTempPath()).Concat(LiteralNativeConfigResponse(Path.GetTempPath())).Concat(LiteralNativeRequirements()).Concat(LiteralNativePermissionProfiles()).Concat(LiteralNativeSandboxReadiness()), raw);
        Assert.All(chunk!, value => Assert.Equal((byte)0, value)); Assert.All(collector!, value => Assert.Equal((byte)0, value));
        byte[]? errorChunk = null, errorCollector = null; byte[] publicError = Encoding.UTF8.GetBytes("literal private-mode stderr suffix"); using var errorSource = new MemoryStream(publicError);
        var stderr = await CodeDocumentationNativeCodexWindowsProcess.DrainConfigurationStreamForTestingAsync(errorSource, new(65536), wire, CancellationToken.None, (a, b) => { errorChunk = a; errorCollector = b; }, stdout: false);
        Assert.Equal(publicError, stderr); Assert.All(errorChunk!, value => Assert.Equal((byte)0, value)); Assert.All(errorCollector!, value => Assert.Equal((byte)0, value));
        var original = new IOException("literal original owner refusal"); var captured = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(original);
        Assert.Same(original, Assert.Throws<IOException>(() => CodeDocumentationNativeCodexWindowsProcess.RethrowAfterPrivateOutputCleanup(captured, wire, raw, stderr)));
        Assert.All(raw, value => Assert.Equal((byte)0, value)); Assert.All(stderr, value => Assert.Equal((byte)0, value));
        Assert.Equal(LiteralNativeConfigResponse(Path.GetTempPath())[..^1], resolution.CopyConfigurationResponse()); // Actual returned private resolution survives clearing separate redundant observations.
        byte[] diagnostic = Encoding.UTF8.GetBytes("literal legacy diagnostic"); var saved = diagnostic.ToArray();
        Assert.Same(original, Assert.Throws<IOException>(() => CodeDocumentationNativeCodexWindowsProcess.RethrowAfterPrivateOutputCleanup(captured, null, diagnostic, []))); Assert.Equal(saved, diagnostic);
    }

    private sealed class ConfigurationTranscriptStream(CodeDocumentationNativeCodexInstalledConfigWire wire) : MemoryStream
    {
        private readonly byte[][] frames = [LiteralNativeConfigInitialize(Path.GetTempPath()), LiteralNativeConfigResponse(Path.GetTempPath()), LiteralNativeRequirements(), LiteralNativePermissionProfiles(), LiteralNativeSandboxReadiness()]; private int frame, offset;
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (frame == frames.Length) return 0;
            if (frame > 0) await (frame switch { 1 => wire.ConfigurationRequestSent, 2 => wire.RequirementsRequestSent, 3 => wire.PermissionProfilesRequestSent, _ => wire.SandboxReadinessRequestSent }).WaitAsync(cancellationToken);
            var count = Math.Min(buffer.Length, frames[frame].Length - offset); frames[frame].AsMemory(offset, count).CopyTo(buffer); offset += count;
            if (offset == frames[frame].Length) { frame++; offset = 0; } return count;
        }
    }

    private static byte[] LiteralNativeConfigInitialize(string root) => Encoding.UTF8.GetBytes("{\"id\":1,\"result\":{\"codexHome\":" + JsonSerializer.Serialize(root) + ",\"platformFamily\":\"windows\",\"platformOs\":\"windows\",\"userAgent\":\"literal-installed-protocol-fixture\"}}\n");
    private static byte[] LiteralNativeConfigResponse(string root)
    {
        var sources = new[] { new JsonObject { ["type"] = "packagedDefaults", ["file"] = Path.Combine(root, "packaged.toml") },
            new JsonObject { ["type"] = "system", ["file"] = Path.Combine(root, "system.toml") }, new JsonObject { ["type"] = "legacyManagedConfigTomlFromFile", ["file"] = Path.Combine(root, "legacy.toml") },
            new JsonObject { ["type"] = "user", ["file"] = Path.Combine(root, "user.toml"), ["profile"] = "fixture-profile" }, new JsonObject { ["type"] = "project", ["dotCodexFolder"] = Path.Combine(root, "project") },
            new JsonObject { ["type"] = "mdm", ["domain"] = "fixture", ["key"] = "complete" }, new JsonObject { ["type"] = "enterpriseManaged", ["id"] = "fixture-cloud", ["name"] = "complete-virtual" }, new JsonObject { ["type"] = "sessionFlags" } };
        var layers = new JsonArray(); foreach (var source in sources) layers.Add(new JsonObject { ["name"] = source.DeepClone(), ["version"] = "v1", ["config"] = new JsonObject { ["unknown_layer"] = "whole-private-layer-suffix" } });
        var origins = new JsonObject { ["unknown_private"] = new JsonObject { ["name"] = sources[0].DeepClone(), ["version"] = "v1" } };
        const string valuePlaceholder = "literal-four-byte-utf8-config-value";
        var result = new JsonObject { ["config"] = new JsonObject { ["unknown_private"] = new JsonObject { ["value"] = valuePlaceholder, ["nested"] = new JsonArray(1, false, null) } }, ["origins"] = origins, ["layers"] = layers };
        // JSON serializers may escape supplementary scalars even with relaxed escaping.
        // This public fixture deliberately contains the physical four-byte scalar that the reader splits.
        var response = new JsonObject { ["id"] = 2, ["result"] = result }.ToJsonString(new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        return Encoding.UTF8.GetBytes(response.Replace("\"" + valuePlaceholder + "\"", "\"whole-עברית-😀-suffix\"", StringComparison.Ordinal) + "\n");
    }
    private static async Task<CodeDocumentationNativeCodexInstalledConfigWire.Resolution> LiteralNativeConfigResolution(string root)
    {
        using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new MemoryStream(); var writer = wire.WriteAsync(input, () => true, CancellationToken.None);
        wire.Accept(LiteralNativeConfigInitialize(root)); await wire.ConfigurationRequestSent.WaitAsync(TimeSpan.FromSeconds(2)); wire.Accept(LiteralNativeConfigResponse(root)); await CompleteLiteralNativePrerequisites(wire); await writer.WaitAsync(TimeSpan.FromSeconds(2)); wire.FinishOutput(); return wire.TakeResolution();
    }

    private static byte[] LiteralNativeRequirements() => Encoding.UTF8.GetBytes("{\"id\":3,\"result\":{\"requirements\":{\"allowedPermissionProfiles\":\"opaque-not-semantically-validated\",\"unknown_policy\":{\"suffix\":\"whole-private-policy-tail\",\"nested\":[1,false,null]}},\"unknown_top\":{\"complete\":true}}}\n");
    private static byte[] LiteralNativePermissionProfiles() => Encoding.UTF8.GetBytes("{\"id\":4,\"result\":{\"data\":[{\"id\":\"fixture-profile-A\",\"allowed\":true,\"description\":null,\"unknown_row\":{\"suffix\":\"whole-private-profile-tail\"}},{\"id\":\"fixture-profile-B\",\"allowed\":false}],\"nextCursor\":null,\"unknown_top\":[1,false,null]}}\n");
    private static byte[] LiteralNativeSandboxReadiness() => Encoding.UTF8.GetBytes("{\"id\":5,\"result\":{\"status\":\"ready\",\"unknown_readiness\":{\"suffix\":\"whole-private-readiness-tail\"}}}\n");
    private static async Task CompleteLiteralNativePrerequisites(CodeDocumentationNativeCodexInstalledConfigWire wire)
    {
        await wire.RequirementsRequestSent.WaitAsync(TimeSpan.FromSeconds(2)); wire.Accept(LiteralNativeRequirements());
        await wire.PermissionProfilesRequestSent.WaitAsync(TimeSpan.FromSeconds(2)); wire.Accept(LiteralNativePermissionProfiles());
        await wire.SandboxReadinessRequestSent.WaitAsync(TimeSpan.FromSeconds(2)); wire.Accept(LiteralNativeSandboxReadiness());
    }

    [Fact]
    public async Task NativeConfig_InstalledPrerequisitesRetainWholePrivatePolicyData()
    {
        var root = Path.GetTempPath();
        foreach (var state in new[] { "absent", "null", "object" })
        {
            var policy = state == "absent" ? Encoding.UTF8.GetBytes("{\"id\":3,\"result\":{\"unknown_top\":\"complete-absent-policy\"}}\n") : state == "null" ? Encoding.UTF8.GetBytes("{\"id\":3,\"result\":{\"requirements\":null,\"unknown_top\":\"complete-null-policy\"}}\n") : LiteralNativeRequirements();
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root);
            using var input = new PrerequisiteDuplexStream(wire, root, requirementsReply: policy);
            await CodeDocumentationNativeCodexWindowsProcess.WriteConfigurationAsync(input, wire, () => true, CancellationToken.None, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2));
            wire.FinishOutput(); using var resolution = wire.TakeResolution();
            var interpreted = resolution.InterpretExecutionPrerequisites();
            Assert.Equal(state == "absent" ? NativeConfigResolution.FieldPresence.Missing : state == "null" ? NativeConfigResolution.FieldPresence.Null : NativeConfigResolution.FieldPresence.Object, interpreted.Requirements);
            Assert.Equal(state == "absent" ? NativeConfigResolution.PrerequisiteIssues.MissingRequirements : NativeConfigResolution.PrerequisiteIssues.None, interpreted.Issues);
            Assert.Equal(state == "absent" ? "declared-subset-blocked-or-unresolved" : "declared-subset-mechanism-required", interpreted.Category); // Even null+ready is only descriptive subset data, with every other opaque policy/config/profile still unresolved.
            Func<byte[]>[] copies = [resolution.CopyInitializeResponse, resolution.CopyConfigurationResponse, resolution.CopyRequirementsResponse, resolution.CopyPermissionProfilesResponse, resolution.CopySandboxReadinessResponse];
            byte[][] expected = [LiteralNativeConfigInitialize(root)[..^1], LiteralNativeConfigResponse(root)[..^1], policy[..^1], LiteralNativePermissionProfiles()[..^1], LiteralNativeSandboxReadiness()[..^1]];
            for (var i = 0; i < copies.Length; i++) { var copy = copies[i](); Assert.Equal(expected[i], copy); copy[0] = 0; Assert.Equal(expected[i], copies[i]()); }
            using (var json = JsonDocument.Parse(resolution.CopyRequirementsResponse()))
            {
                var result = json.RootElement.GetProperty("result"); Assert.Equal(state != "absent", result.TryGetProperty("requirements", out var value));
                if (state == "null") Assert.Equal(JsonValueKind.Null, value.ValueKind);
                if (state == "object") { Assert.Equal("opaque-not-semantically-validated", value.GetProperty("allowedPermissionProfiles").GetString()); Assert.Equal("whole-private-policy-tail", value.GetProperty("unknown_policy").GetProperty("suffix").GetString()); }
            }
            var requests = Encoding.UTF8.GetString(input.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => JsonNode.Parse(line)!).ToArray();
            Assert.Equal(new[] { "initialize", "initialized", "config/read", "configRequirements/read", "permissionProfile/list", "windowsSandbox/readiness" }, requests.Select(item => item["method"]!.GetValue<string>()));
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, requests.Where(item => item["id"] is not null).Select(item => item["id"]!.GetValue<int>()));
            using (var request = JsonDocument.Parse(requests[3].ToJsonString())) Assert.Equal(JsonValueKind.Null, request.RootElement.GetProperty("params").ValueKind);
            using (var request = JsonDocument.Parse(requests[5].ToJsonString())) Assert.Equal(JsonValueKind.Null, request.RootElement.GetProperty("params").ValueKind);
            Assert.Single(requests[4]["params"]!.AsObject()); Assert.Equal(Path.GetFullPath(root), requests[4]["params"]!["cwd"]!.GetValue<string>());
            var retained = PrivateProtocolArrays(resolution); Assert.Equal(5, retained.Length); Assert.All(retained, bytes => Assert.NotEmpty(bytes));
            resolution.Dispose(); Assert.All(retained, bytes => Assert.All(bytes, value => Assert.Equal((byte)0, value))); foreach (var copy in copies) Assert.Throws<IOException>(() => copy());
            Assert.Throws<IOException>(() => resolution.InterpretExecutionPrerequisites()); // The new consumer cannot revive disposed retained bytes.
            Assert.All(input.RequestBuffers, bytes => Assert.All(bytes, value => Assert.Equal((byte)0, value))); // Actual temporary writer arrays clear, while transferred private data survives until its own disposal.
        }
        foreach (var status in new[] { "ready", "notConfigured", "updateRequired" })
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root);
            using var input = new PrerequisiteDuplexStream(wire, root, readinessReply: Encoding.UTF8.GetBytes("{\"id\":5,\"result\":{\"status\":\"" + status + "\",\"unknown\":[false,null]}}\n"));
            await wire.WriteAsync(input, () => true, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2)); wire.FinishOutput(); using var data = wire.TakeResolution();
            using var json = JsonDocument.Parse(data.CopySandboxReadinessResponse()); Assert.Equal(status, json.RootElement.GetProperty("result").GetProperty("status").GetString()); Assert.Equal(6, input.Methods.Count); // Even ready produces only these data requests, never setup, thread, turn or model authority.
            var interpreted = data.InterpretExecutionPrerequisites();
            Assert.Equal(status == "ready" ? NativeConfigResolution.ReadinessData.Ready : status == "notConfigured" ? NativeConfigResolution.ReadinessData.NotConfigured : NativeConfigResolution.ReadinessData.UpdateRequired, interpreted.Readiness);
            Assert.Equal(status == "ready" ? NativeConfigResolution.PrerequisiteIssues.None : status == "notConfigured" ? NativeConfigResolution.PrerequisiteIssues.SandboxNotConfigured : NativeConfigResolution.PrerequisiteIssues.SandboxUpdateRequired, interpreted.Issues);
        }
        // These literal full dialogues execute the actual retained data interpreter, not a copied policy checker or installed permission fixture.
        async Task<NativeConfigResolution> ResolveLiteralSubset(string policy, string status = "ready")
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root);
            using var input = new PrerequisiteDuplexStream(wire, root, requirementsReply: Encoding.UTF8.GetBytes("{\"id\":3,\"result\":{\"requirements\":" + policy + ",\"unknown_top\":{\"full\":\"private-tail\"}}}\n"), readinessReply: Encoding.UTF8.GetBytes("{\"id\":5,\"result\":{\"status\":\"" + status + "\",\"unknown\":[false,null]}}\n"));
            await wire.WriteAsync(input, () => true, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2)); wire.FinishOutput(); Assert.Equal(6, input.Methods.Count); return wire.TakeResolution();
        }
        (string Json, NativeConfigResolution.FieldPresence Presence, NativeConfigResolution.DeclaredSandboxModes Modes, NativeConfigResolution.PrerequisiteIssues Issues)[] sandboxCases = [
            ("{}",NativeConfigResolution.FieldPresence.Missing,0,0),("{\"allowedSandboxModes\":null}",NativeConfigResolution.FieldPresence.Null,0,0),
            ("{\"allowedSandboxModes\":[]}",NativeConfigResolution.FieldPresence.Empty,0,NativeConfigResolution.PrerequisiteIssues.ReadOnlyExcluded),
            ("{\"allowedSandboxModes\":[\"read-only\"]}",NativeConfigResolution.FieldPresence.Values,NativeConfigResolution.DeclaredSandboxModes.ReadOnly,0),
            ("{\"allowedSandboxModes\":[\"workspace-write\",\"danger-full-access\"]}",NativeConfigResolution.FieldPresence.Values,NativeConfigResolution.DeclaredSandboxModes.WorkspaceWrite|NativeConfigResolution.DeclaredSandboxModes.DangerFullAccess,NativeConfigResolution.PrerequisiteIssues.ReadOnlyExcluded),
            ("{\"allowedSandboxModes\":[\"read-only\",\"workspace-write\",\"danger-full-access\"]}",NativeConfigResolution.FieldPresence.Values,NativeConfigResolution.DeclaredSandboxModes.ReadOnly|NativeConfigResolution.DeclaredSandboxModes.WorkspaceWrite|NativeConfigResolution.DeclaredSandboxModes.DangerFullAccess,0)
        ];
        foreach (var item in sandboxCases)
        {
            using var data = await ResolveLiteralSubset(item.Json); var bytes = data.CopyRequirementsResponse(); var interpreted = data.InterpretExecutionPrerequisites();
            Assert.Equal(item.Presence, interpreted.SandboxModesPresence); Assert.Equal(item.Modes, interpreted.SandboxModes); Assert.Equal(item.Issues, interpreted.Issues); Assert.Equal(bytes, data.CopyRequirementsResponse());
        }
        foreach (var field in new[] { "allowedSandboxModes", "allowedWindowsSandboxImplementations" })
        foreach (var literal in new[] { "false", "1", "\"read-only\"", "{}", "[null]", "[true]", "[1]", "[\"unknown\"]", field == "allowedSandboxModes" ? "[\"read-only\",\"read-only\"]" : "[\"mxc\",\"mxc\"]" })
        {
            using var data = await ResolveLiteralSubset("{\"" + field + "\":" + literal + "}"); var interpreted = data.InterpretExecutionPrerequisites();
            Assert.Equal(NativeConfigResolution.PrerequisiteIssues.InvalidDeclaredData, interpreted.Issues); Assert.Equal("declared-subset-blocked-or-unresolved", interpreted.Category);
            Assert.Equal(NativeConfigResolution.FieldPresence.Invalid, field == "allowedSandboxModes" ? interpreted.SandboxModesPresence : interpreted.WindowsImplementationsPresence);
        }
        (string Json, NativeConfigResolution.FieldPresence Presence, NativeConfigResolution.DeclaredWindowsImplementations Implementations, NativeConfigResolution.PrerequisiteIssues Issues)[] windowsCases = [
            ("{}",NativeConfigResolution.FieldPresence.Missing,0,0),("{\"allowedWindowsSandboxImplementations\":null}",NativeConfigResolution.FieldPresence.Null,0,0),
            ("{\"allowedWindowsSandboxImplementations\":[]}",NativeConfigResolution.FieldPresence.Empty,0,NativeConfigResolution.PrerequisiteIssues.WindowsImplementationsExcluded),
            ("{\"allowedWindowsSandboxImplementations\":[\"elevated\"]}",NativeConfigResolution.FieldPresence.Values,NativeConfigResolution.DeclaredWindowsImplementations.Elevated,0),
            ("{\"allowedWindowsSandboxImplementations\":[\"unelevated\"]}",NativeConfigResolution.FieldPresence.Values,NativeConfigResolution.DeclaredWindowsImplementations.Unelevated,0),
            ("{\"allowedWindowsSandboxImplementations\":[\"mxc\"]}",NativeConfigResolution.FieldPresence.Values,NativeConfigResolution.DeclaredWindowsImplementations.Mxc,0),
            ("{\"allowedWindowsSandboxImplementations\":[\"mxc\",\"elevated\",\"unelevated\"]}",NativeConfigResolution.FieldPresence.Values,NativeConfigResolution.DeclaredWindowsImplementations.Mxc|NativeConfigResolution.DeclaredWindowsImplementations.Elevated|NativeConfigResolution.DeclaredWindowsImplementations.Unelevated,0)
        ];
        foreach (var item in windowsCases)
        {
            using var data = await ResolveLiteralSubset(item.Json); var interpreted = data.InterpretExecutionPrerequisites();
            Assert.Equal(item.Presence, interpreted.WindowsImplementationsPresence); Assert.Equal(item.Implementations, interpreted.WindowsImplementations); Assert.Equal(item.Issues, interpreted.Issues); // Declared values never assert which mode is active.
        }
        (string Json, NativeConfigResolution.FieldPresence Presence, NativeConfigResolution.PrerequisiteIssues Issues)[] instructionCases = [
            ("{}",NativeConfigResolution.FieldPresence.Missing,0),("{\"additionalDeveloperInstructions\":null}",NativeConfigResolution.FieldPresence.Null,0),
            ("{\"additionalDeveloperInstructions\":\"\"}",NativeConfigResolution.FieldPresence.Empty,0),("{\"additionalDeveloperInstructions\":\"literal-private-instruction-tail\"}",NativeConfigResolution.FieldPresence.NonEmpty,NativeConfigResolution.PrerequisiteIssues.ManagedInstructionsNeedComposition),
            ("{\"additionalDeveloperInstructions\":\" \"}",NativeConfigResolution.FieldPresence.NonEmpty,NativeConfigResolution.PrerequisiteIssues.ManagedInstructionsNeedComposition),
            ("{\"additionalDeveloperInstructions\":false}",NativeConfigResolution.FieldPresence.Invalid,NativeConfigResolution.PrerequisiteIssues.InvalidDeclaredData),
            ("{\"additionalDeveloperInstructions\":[]}",NativeConfigResolution.FieldPresence.Invalid,NativeConfigResolution.PrerequisiteIssues.InvalidDeclaredData),
            ("{\"additionalDeveloperInstructions\":{}}",NativeConfigResolution.FieldPresence.Invalid,NativeConfigResolution.PrerequisiteIssues.InvalidDeclaredData)
        ];
        foreach (var item in instructionCases)
        {
            using var data = await ResolveLiteralSubset(item.Json); var original = data.CopyRequirementsResponse(); var interpreted = data.InterpretExecutionPrerequisites();
            Assert.Equal(item.Presence, interpreted.DeveloperInstructions); Assert.Equal(item.Issues, interpreted.Issues); Assert.DoesNotContain("literal-private-instruction-tail", interpreted.ToString()); Assert.Equal(original, data.CopyRequirementsResponse());
            using var caller = new CancellationTokenSource(); caller.Cancel(); var error = Assert.ThrowsAny<OperationCanceledException>(() => data.InterpretExecutionPrerequisites(caller.Token)); Assert.Equal(caller.Token, error.CancellationToken); Assert.Equal(original, data.CopyRequirementsResponse());
        }
        using (var data = await ResolveLiteralSubset("{\"allowedSandboxModes\":[],\"allowedWindowsSandboxImplementations\":[],\"additionalDeveloperInstructions\":\"private-unlogged\",\"unknown\":{\"full\":[1,false,null]}}", "notConfigured"))
        {
            var interpreted = data.InterpretExecutionPrerequisites(); Assert.Equal(NativeConfigResolution.PrerequisiteIssues.ReadOnlyExcluded|NativeConfigResolution.PrerequisiteIssues.WindowsImplementationsExcluded|NativeConfigResolution.PrerequisiteIssues.ManagedInstructionsNeedComposition|NativeConfigResolution.PrerequisiteIssues.SandboxNotConfigured, interpreted.Issues); // Simultaneous blockers remain distinct rather than one priority hiding the others.
        }
    }

    [Fact]
    public async Task NativeConfig_PrerequisiteRepliesRequireActualSerialIssuance()
    {
        var root = Path.GetTempPath();
        foreach (var duringFlush in new[] { false, true })
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new PrerequisiteDuplexStream(wire, root, duringFlush: duringFlush);
            await wire.WriteAsync(input, () => true, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2)); wire.FinishOutput(); using var data = wire.TakeResolution();
            Assert.Equal(new[] { 1, 0, 2, 3, 4, 5 }, input.RequestIds); Assert.Null(wire.Primary); Assert.Equal(LiteralNativePermissionProfiles()[..^1], data.CopyPermissionProfilesResponse()); // Genuine fast replies arrive during actual writes/flushes, not only after a scripted sent marker.
        }
        foreach (var target in new[] { 3, 4, 5 })
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new PrerequisiteDuplexStream(wire, root);
            using var entered = new ManualResetEventSlim(); using var release = new ManualResetEventSlim();
            var writer = Task.Run(() => wire.WriteAsync(input, () => { if (input.LastId == target - 1) { entered.Set(); if (!release.Wait(TimeSpan.FromSeconds(2))) throw new IOException("bounded later currentness fixture expired"); } return true; }, CancellationToken.None));
            try
            {
                Assert.True(entered.Wait(TimeSpan.FromSeconds(2))); var original = Assert.Throws<IOException>(() => wire.Accept(LiteralPrerequisiteReply(target)));
                release.Set(); Assert.Same(original, await Assert.ThrowsAsync<IOException>(() => writer.WaitAsync(TimeSpan.FromSeconds(2)))); Assert.DoesNotContain(target, input.RequestIds); Assert.Same(original, wire.Primary!.SourceException);
                Assert.Throws<IOException>(() => wire.TakeResolution());
            }
            finally { release.Set(); await Record.ExceptionAsync(() => writer.WaitAsync(TimeSpan.FromSeconds(2))); }
        }
        foreach (var target in new[] { 3, 4, 5 })
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new PrerequisiteDuplexStream(wire, root, heldFlush: target);
            var writer = wire.WriteAsync(input, () => true, CancellationToken.None);
            try
            {
                await input.FlushEntered.Task.WaitAsync(TimeSpan.FromSeconds(2)); Assert.False(writer.IsCompleted); Assert.False(PrerequisiteRequestSent(wire, target).IsCompleted);
                Assert.DoesNotContain(target + 1, input.RequestIds); Assert.Throws<IOException>(() => wire.TakeResolution());
                if (target == 5) { wire.FinishOutput(); Assert.Throws<IOException>(() => wire.TakeResolution()); }
                input.ReleaseFlush.TrySetResult(true); await writer.WaitAsync(TimeSpan.FromSeconds(2)); if (target != 5) wire.FinishOutput(); using var data = wire.TakeResolution(); Assert.Equal(6, input.Methods.Count);
            }
            finally { input.ReleaseFlush.TrySetResult(true); await Record.ExceptionAsync(() => writer.WaitAsync(TimeSpan.FromSeconds(2))); }
        }
    }

    [Fact]
    public async Task NativeConfig_PrerequisiteSchemaAndPartialProfileInventoryRefuse()
    {
        var root = Path.GetTempPath();
        (int Id, string Json, bool Eof)[] faults = [
            (3,"{\"id\":3,\"result\":null}\n",false),(3,"{\"id\":3,\"result\":[]}\n",false),(3,"{\"id\":3,\"result\":{\"requirements\":false}}\n",false),(3,"{\"id\":3,\"result\":{\"requirements\":[]}}\n",false),(3,"{\"id\":3,\"result\":{\"requirements\":\"policy\"}}\n",false),
            (3,"{\"id\":3,\"result\":{\"requirements\":{\"unknown\":1,\"unknown\":2}}}\n",false),(3,"{\"id\":3,\"result\":{\"requirements\":null}}",true),(3,"{\"id\":4,\"result\":{}}\n",false),(3,"{\"id\":3,\"error\":{\"code\":-1}}\n",false),
            (3,"{\"id\":3,\"result\":{\"requirements\":{\"allowedSandboxModes\":null,\"allowedSandboxModes\":[\"read-only\"]}}}\n",false),
            (3,"{\"id\":3,\"result\":{\"requirements\":{\"additionalDeveloperInstructions\":null,\"additionalDeveloperInstructions\":\"duplicate\"}}}\n",false),
            (4,"{\"id\":4,\"result\":{}}\n",false),(4,"{\"id\":4,\"result\":{\"data\":null}}\n",false),(4,"{\"id\":4,\"result\":{\"data\":[null]}}\n",false),(4,"{\"id\":4,\"result\":{\"data\":[{\"id\":\"\",\"allowed\":true}]}}\n",false),
            (4,"{\"id\":4,\"result\":{\"data\":[{\"id\":\"a\"}]}}\n",false),(4,"{\"id\":4,\"result\":{\"data\":[{\"id\":\"a\",\"allowed\":\"true\"}]}}\n",false),(4,"{\"id\":4,\"result\":{\"data\":[{\"id\":\"a\",\"allowed\":true,\"description\":1}]}}\n",false),
            (4,"{\"id\":4,\"result\":{\"data\":[{\"id\":\"a\",\"allowed\":true},{\"id\":\"a\",\"allowed\":false}]}}\n",false),(4,"{\"id\":4,\"result\":{\"data\":[],\"nextCursor\":\"more\"}}\n",false),(4,"{\"id\":4,\"result\":{\"data\":[],\"nextCursor\":\"\"}}\n",false),(4,"{\"id\":4,\"result\":{\"data\":[],\"nextCursor\":0}}\n",false),
            (4,"{\"id\":4,\"result\":{\"data\":[],\"unknown\":{\"duplicate\":1,\"duplicate\":2}}}\n",false),(4,"{\"id\":5,\"result\":{\"status\":\"ready\"}}\n",false),(4,"",true),
            (5,"{\"id\":5,\"result\":{}}\n",false),(5,"{\"id\":5,\"result\":{\"status\":null}}\n",false),(5,"{\"id\":5,\"result\":{\"status\":true}}\n",false),(5,"{\"id\":5,\"result\":{\"status\":\"READY\"}}\n",false),(5,"{\"id\":5,\"result\":{\"status\":\"unknown\"}}\n",false),(5,"{\"id\":5,\"result\":{\"status\":\"ready\"},\"method\":\"thread/start\"}\n",false),(5,"{\"id\":5,\"result\":{\"status\":\"ready\"}}",true)
        ];
        foreach (var fault in faults)
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new MemoryStream(); var writer = wire.WriteAsync(input, () => true, CancellationToken.None);
            await PrimePrerequisiteRequest(wire, root, fault.Id);
            var original = Record.Exception(() => { wire.Accept(Encoding.UTF8.GetBytes(fault.Json)); if (fault.Eof) wire.FinishOutput(); }); Assert.NotNull(original);
            Assert.Same(original, await Record.ExceptionAsync(() => writer.WaitAsync(TimeSpan.FromSeconds(2)))); Assert.Same(original, wire.Primary!.SourceException); Assert.Throws<IOException>(() => wire.TakeResolution());
            var methods = Encoding.UTF8.GetString(input.ToArray()); Assert.DoesNotContain("thread/", methods); Assert.DoesNotContain("turn/", methods);
        }
        foreach (var bytes in new[] { new byte[] { 0xff, 10 }, new byte[65537], Encoding.UTF8.GetBytes("{\"id\":3,\"id\":3,\"result\":{}}\n"), Encoding.UTF8.GetBytes("{not-json}\n") })
        {
            using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new MemoryStream(); var writer = wire.WriteAsync(input, () => true, CancellationToken.None); await PrimePrerequisiteRequest(wire, root, 3);
            var original = Record.Exception(() => wire.Accept(bytes)); Assert.NotNull(original); Assert.Same(original, await Record.ExceptionAsync(() => writer.WaitAsync(TimeSpan.FromSeconds(2)))); Assert.Throws<IOException>(() => wire.TakeResolution());
        }
        using (var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root))
        using (var input = new PrerequisiteDuplexStream(wire, root, profilesReply: Encoding.UTF8.GetBytes("{\"id\":4,\"result\":{\"data\":[],\"unknown\":\"complete-empty-inventory\"}}\n")))
        {
            await wire.WriteAsync(input, () => true, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2)); wire.FinishOutput();
            var original = Assert.Throws<IOException>(() => wire.Accept(Encoding.UTF8.GetBytes("{\"id\":5,\"result\":{\"status\":\"ready\"}}\n"))); Assert.Same(original, wire.Primary!.SourceException); Assert.Throws<IOException>(() => wire.TakeResolution()); // Empty inventory was complete; duplicate/extra final response is independently rejected.
        }
    }

    [Fact]
    public async Task NativeConfig_PrerequisiteCancellationAndCleanupPreserveOriginalCause()
    {
        var root = Path.GetTempPath();
        foreach (var target in new[] { 3, 4, 5 })
        {
            using var caller = new CancellationTokenSource(); using var cleanup = new CancellationTokenSource(); using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root);
            using var input = new PrerequisiteDuplexStream(wire, root, afterReply: id => { if (id == target) caller.Cancel(); });
            var writer = CodeDocumentationNativeCodexWindowsProcess.WriteConfigurationAsync(input, wire, () => true, caller.Token, cleanup.Token);
            var original = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writer.WaitAsync(TimeSpan.FromSeconds(2)));
            Assert.Equal(caller.Token, original.CancellationToken); Assert.Same(original, wire.Primary!.SourceException); Assert.DoesNotContain(target + 1, input.RequestIds);
            var closed = false; var closure = await CodeDocumentationNativeCodexWindowsProcess.CloseOwnedStreamsForTestingAsync(Task.FromResult(Array.Empty<byte>()), Task.FromResult(Array.Empty<byte>()), writer, cleanup.Cancel, () => { closed = true; throw new IOException("literal later prerequisite cleanup fault"); }, () => { }, input.Dispose);
            Assert.True(closure.AllTerminal); Assert.True(closed); Assert.Same(original, CodeDocumentationNativeCodexWindowsProcess.ResolveFinalPrimary(wire.Primary, caller.Token, closure)!.SourceException); Assert.False(input.CanWrite);
            Assert.All(input.RequestBuffers, bytes => Assert.All(bytes, value => Assert.Equal((byte)0, value))); var retained = PrivateProtocolArrays(wire); wire.Dispose(); Assert.All(retained, bytes => Assert.All(bytes, value => Assert.Equal((byte)0, value))); Assert.Throws<IOException>(() => wire.TakeResolution());
        }
        foreach (var target in new[] { 3, 4, 5 })
        {
            var current = true; using var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root); using var input = new PrerequisiteDuplexStream(wire, root, afterReply: id => { if (id == target) current = false; });
            var original = await Assert.ThrowsAsync<IOException>(() => wire.WriteAsync(input, () => current, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2)));
            Assert.Same(original, wire.Primary!.SourceException); Assert.DoesNotContain(target + 1, input.RequestIds); current = true; Assert.Throws<IOException>(() => wire.TakeResolution());
        }
        using (var cleanup = new CancellationTokenSource())
        using (var wire = new CodeDocumentationNativeCodexInstalledConfigWire(root))
        using (var input = new MemoryStream())
        {
            var writer = CodeDocumentationNativeCodexWindowsProcess.WriteConfigurationAsync(input, wire, () => true, CancellationToken.None, cleanup.Token); await PrimePrerequisiteRequest(wire, root, 3);
            var original = new IOException("literal later-phase source read failure"); using var source = new SourceReadFailureStream(original); byte[]? chunk = null, collector = null;
            var drain = CodeDocumentationNativeCodexWindowsProcess.DrainConfigurationStreamForTestingAsync(source, new(65536), wire, cleanup.Token, (a, b) => { chunk = a; collector = b; });
            Assert.Same(original, await Assert.ThrowsAsync<IOException>(() => drain.WaitAsync(TimeSpan.FromSeconds(2)))); Assert.All(chunk!, value => Assert.Equal((byte)0, value)); Assert.All(collector!, value => Assert.Equal((byte)0, value));
            var closure = await CodeDocumentationNativeCodexWindowsProcess.CloseOwnedStreamsForTestingAsync(drain, Task.FromResult(Array.Empty<byte>()), writer, cleanup.Cancel, source.Dispose, () => { throw new IOException("literal later close fault"); }, input.Dispose);
            Assert.True(closure.AllTerminal); Assert.Same(original, CodeDocumentationNativeCodexWindowsProcess.ResolveFinalPrimary(wire.Primary, CancellationToken.None, closure)!.SourceException);
            var pending = (List<byte>)typeof(CodeDocumentationNativeCodexInstalledConfigWire).GetField("pending", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(wire)!;
            var backing = (byte[])typeof(List<byte>).GetField("_items", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(pending)!; Assert.Contains(backing, value => value != 0);
            wire.Dispose(); Assert.All(backing, value => Assert.Equal((byte)0, value)); Assert.Throws<IOException>(() => wire.TakeResolution());
        }
        using var fixture = new JournalStorageFixture(); var path = Path.Combine(fixture.ModelVersionRoot, "prerequisite-acquisition.fixture"); WriteMainInputFixture(path, [1, 2, 3]); using var cancellation = new CancellationTokenSource();
        var acquired = false; var error = Assert.Throws<OperationCanceledException>(() => CodeDocumentationNativeCodexInstalledMainInputs.RetainedDiskInputs.Open([new(path, true, true, 1024)], cancellation.Token, afterFirstAcquisitionForTesting: () => { acquired = true; cancellation.Cancel(); }));
        Assert.True(acquired); Assert.Equal(cancellation.Token, error.CancellationToken); using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None); Assert.Equal(3, exclusive.Length); // Actual pending acquisition closes; no Session/permission owner is fabricated.
    }

    private static byte[][] PrivateProtocolArrays(object value) => value.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Where(field => field.FieldType == typeof(byte[])).Select(field => (byte[]?)field.GetValue(value)).Where(bytes => bytes is not null).Select(bytes => bytes!).ToArray();
    private static Task PrerequisiteRequestSent(CodeDocumentationNativeCodexInstalledConfigWire wire, int id) => id switch { 3 => wire.RequirementsRequestSent, 4 => wire.PermissionProfilesRequestSent, 5 => wire.SandboxReadinessRequestSent, _ => throw new ArgumentOutOfRangeException(nameof(id)) };
    private static byte[] LiteralPrerequisiteReply(int id) => id switch { 3 => LiteralNativeRequirements(), 4 => LiteralNativePermissionProfiles(), 5 => LiteralNativeSandboxReadiness(), _ => throw new ArgumentOutOfRangeException(nameof(id)) };
    private static async Task PrimePrerequisiteRequest(CodeDocumentationNativeCodexInstalledConfigWire wire, string root, int id)
    {
        wire.Accept(LiteralNativeConfigInitialize(root)); await wire.ConfigurationRequestSent.WaitAsync(TimeSpan.FromSeconds(2)); wire.Accept(LiteralNativeConfigResponse(root));
        for (var phase = 3; phase <= id; phase++) { await PrerequisiteRequestSent(wire, phase).WaitAsync(TimeSpan.FromSeconds(2)); if (phase < id) wire.Accept(LiteralPrerequisiteReply(phase)); }
    }
    private sealed class PrerequisiteDuplexStream(CodeDocumentationNativeCodexInstalledConfigWire wire, string root, bool duringFlush = false, int heldFlush = 0, Action<int>? afterReply = null, byte[]? requirementsReply = null, byte[]? profilesReply = null, byte[]? readinessReply = null) : MemoryStream
    {
        private int lastId; private byte[]? reply;
        internal int LastId => Volatile.Read(ref lastId);
        internal List<string> Methods { get; } = [];
        internal List<int> RequestIds { get; } = [];
        internal List<byte[]> RequestBuffers { get; } = [];
        internal TaskCompletionSource<bool> FlushEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource<bool> ReleaseFlush { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (System.Runtime.InteropServices.MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> segment) && segment.Array is { } owned) RequestBuffers.Add(owned);
            await base.WriteAsync(buffer, cancellationToken); using var json = JsonDocument.Parse(buffer[..^1]); var method = json.RootElement.GetProperty("method").GetString()!; var id = json.RootElement.TryGetProperty("id", out var number) ? number.GetInt32() : 0;
            Methods.Add(method); RequestIds.Add(id); if (id != 0) Volatile.Write(ref lastId, id);
            reply = method switch { "initialize" => LiteralNativeConfigInitialize(root), "config/read" => LiteralNativeConfigResponse(root), "configRequirements/read" => requirementsReply ?? LiteralNativeRequirements(), "permissionProfile/list" => profilesReply ?? LiteralNativePermissionProfiles(), "windowsSandbox/readiness" => readinessReply ?? LiteralNativeSandboxReadiness(), _ => null };
            if (!duringFlush && reply is { } response) { reply = null; wire.Accept(response); afterReply?.Invoke(id); }
        }
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            if (reply is { } response) { reply = null; wire.Accept(response); afterReply?.Invoke(LastId); }
            if (heldFlush != 0 && LastId == heldFlush) { FlushEntered.TrySetResult(true); await ReleaseFlush.Task.WaitAsync(cancellationToken); }
            await base.FlushAsync(cancellationToken);
        }
    }

    private static void WriteMainInputFixture(string path, byte[] bytes)
    {
        using var file = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite, FileOptions.WriteThrough);
        file.Write(bytes); file.Flush(true);
    }

    [Fact]
    public async Task NativeProcess_ExactStreamsRootBirthAndTreeClosure()
    {
        if (!OperatingSystem.IsWindows()) { await AssertNativeProcessPlatformRefusal(); return; }
        var result = await CodeDocumentationNativeCodexWindowsProcess.RunHarmlessCanaryForTestingAsync(
            Path.GetTempPath(), "echo SOURCE56-OUT & echo SOURCE56-ERR 1>&2", 1024, TimeSpan.FromSeconds(5), CancellationToken.None);
        output.WriteLine(JsonSerializer.Serialize(result));
        Assert.Null(result.Failure); Assert.Equal(0u, result.ExitCode); Assert.True(result.TreeClosed);
        Assert.False(result.DescendantsAfterRoot); Assert.NotEqual(0u, result.RootPid); Assert.True(result.RootBirthFileTime > 0);
        // Windows TotalProcesses counts every process associated during this Job's lifetime, including terminated members; it does not identify the root.
        Assert.True(result.TotalProcesses >= 1); Assert.True(result.RootExitObserved); Assert.True(result.StreamTasksTerminal);
        Assert.False(result.OutputOverflow); Assert.Null(result.StreamCleanupFailure); // Exact root PID/birth, zero exit, whole-Job quiescence, no active descendants and complete stream bytes carry the owner guarantee.
        Assert.Equal("SOURCE56-OUT \r\n", Encoding.UTF8.GetString(result.StandardOutput));
        Assert.Equal("SOURCE56-ERR \r\n", Encoding.UTF8.GetString(result.StandardError));
    }

    [Fact]
    public async Task NativeProcess_OverflowDeadlineAndDescendantExitStayUnknown()
    {
        if (!OperatingSystem.IsWindows()) { await AssertNativeProcessPlatformRefusal(); return; }
        var overflow = await CodeDocumentationNativeCodexWindowsProcess.RunHarmlessCanaryForTestingAsync(
            Path.GetTempPath(), "for /L %i in (1,1,2000) do @echo SOURCE56", 128, TimeSpan.FromSeconds(5), CancellationToken.None);
        output.WriteLine(JsonSerializer.Serialize(overflow));
        Assert.Equal("output-overflow", overflow.Failure); Assert.True(overflow.OutputOverflow); Assert.True(overflow.TreeClosed); Assert.True(overflow.StreamTasksTerminal);
        Assert.InRange(overflow.StandardOutput.Length + overflow.StandardError.Length, 0, 128);
        var deadline = await CodeDocumentationNativeCodexWindowsProcess.RunHarmlessCanaryForTestingAsync(
            Path.GetTempPath(), "ping -n 30 127.0.0.1 >nul", 1024, TimeSpan.FromMilliseconds(150), CancellationToken.None);
        output.WriteLine(JsonSerializer.Serialize(deadline));
        Assert.Equal("deadline", deadline.Failure); Assert.True(deadline.TreeClosed); Assert.True(deadline.StreamTasksTerminal); Assert.True(deadline.TotalProcesses >= 1);
        Assert.True(deadline.RootPid > 0); Assert.True(deadline.RootBirthFileTime > 0);
        var child = await CodeDocumentationNativeCodexWindowsProcess.RunHarmlessCanaryForTestingAsync(
            Path.GetTempPath(), "start /b ping -n 30 127.0.0.1 >nul", 1024, TimeSpan.FromSeconds(5), CancellationToken.None);
        output.WriteLine(JsonSerializer.Serialize(child));
        Assert.Equal("descendants-after-root", child.Failure); Assert.True(child.DescendantsAfterRoot);
        Assert.True(child.TreeClosed); Assert.True(child.RootExitObserved); Assert.True(child.StreamTasksTerminal); Assert.Equal(0u, child.ExitCode); Assert.True(child.TotalProcesses >= 2);
    }

    [Fact]
    public async Task NativeProcess_NaturalOwnedChildClosesWithinOriginalDeadline()
    {
        if (!OperatingSystem.IsWindows()) { await AssertNativeProcessPlatformRefusal(); return; }
        using (var fixture = new JournalStorageFixture())
        {
            var data = await CodeDocumentationNativeCodexWindowsProcess.RunNaturalChildCanaryForTestingAsync(fixture.ModelVersionRoot, CancellationToken.None);
            Assert.True(data.ActiveAtRootExit > 0); Assert.True(data.ChildReadyBeforeRelease); Assert.True(data.ReleasedAfterRootExit);
            var actual = data.Process; Assert.True(actual.RootExitObserved); Assert.NotEqual(0u, actual.RootPid); Assert.True(actual.RootBirthFileTime > 0);
            Assert.Equal(0u, actual.ExitCode); Assert.True(actual.TotalProcesses >= 2); Assert.True(actual.TreeClosed); Assert.False(actual.DescendantsAfterRoot); Assert.Null(actual.Failure);
            Assert.True(actual.StreamTasksTerminal); Assert.Null(actual.StreamCleanupFailure); Assert.False(actual.OutputOverflow);
            Assert.Equal("SOURCE-NATURAL-ROOT\r\nSOURCE-NATURAL-CHILD\r\n", Encoding.UTF8.GetString(actual.StandardOutput)); Assert.Equal("SOURCE-NATURAL-ERR \r\n", Encoding.UTF8.GetString(actual.StandardError)); // cmd retains the literal space before its stderr redirection, as in the existing exact-stream canary.
            Assert.Equal("RELEASE-AFTER-ROOT\r\n", File.ReadAllText(Path.Combine(fixture.ModelVersionRoot, "native-child-release.fixture"))); // Neither sleep nor total count substitutes for the causal alive-before-release witness.
        }
        using (var fixture = new JournalStorageFixture())
        using (var caller = new CancellationTokenSource())
        {
            var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CodeDocumentationNativeCodexWindowsProcess.RunNaturalChildCanaryForTestingAsync(fixture.ModelVersionRoot, caller.Token, cancelAfterRoot: caller));
            Assert.Equal(caller.Token, error.CancellationToken); Assert.Equal(true, error.Data["NativeProcessRootExitObserved"]); Assert.Equal(true, error.Data["NativeProcessDescendantsAfterRoot"]);
            Assert.Equal(true, error.Data["NativeProcessTreeClosed"]); Assert.Equal(true, error.Data["NativeProcessStreamTasksTerminal"]); Assert.False(File.Exists(Path.Combine(fixture.ModelVersionRoot, "native-child-release.fixture")));
        }
        using (var fixture = new JournalStorageFixture())
        {
            var original = new IOException("literal original post-root observation refusal");
            var error = await Assert.ThrowsAsync<IOException>(() => CodeDocumentationNativeCodexWindowsProcess.RunNaturalChildCanaryForTestingAsync(fixture.ModelVersionRoot, CancellationToken.None, failAfterRoot: original, failCleanup: true));
            Assert.Same(original, error); Assert.Equal(true, error.Data["NativeProcessRootExitObserved"]); Assert.Equal(true, error.Data["NativeProcessTreeClosed"]); Assert.Equal(true, error.Data["NativeProcessStreamTasksTerminal"]);
            Assert.Contains("IOException", Assert.IsType<string>(error.Data["NativeProcessCleanup"])); Assert.False(File.Exists(Path.Combine(fixture.ModelVersionRoot, "native-child-release.fixture"))); // Later forced closure is cleanup only; it never turns the original fault into green.
        }
    }

    [Fact]
    public async Task NativeProcess_CancellationPreservesTokenAndClosesOwnedTree()
    {
        if (!OperatingSystem.IsWindows()) { await AssertNativeProcessPlatformRefusal(); return; }
        using var cancellation = new CancellationTokenSource();
        var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CodeDocumentationNativeCodexWindowsProcess.RunCancellationCanaryForTestingAsync(Path.GetTempPath(), cancellation));
        Assert.Equal(cancellation.Token, thrown.CancellationToken);
        Assert.Equal(true, thrown.Data["NativeProcessTreeClosed"]);
        Assert.NotEqual(0u, Assert.IsType<uint>(thrown.Data["NativeProcessRootPid"]));
        Assert.True(Assert.IsType<long>(thrown.Data["NativeProcessRootBirthFileTime"]) > 0);
        Assert.True(Assert.IsType<uint>(thrown.Data["NativeProcessTotalProcesses"]) >= 1);
        Assert.Equal(true, thrown.Data["NativeProcessCancellationOutputWitness"]);
    }

    [Fact] // Exercise the production transfer boundary with isolated physical files, never installed launch inputs.
    public void NativeResources_PostAcquisitionFaultClosesEveryHandleAndPreservesBytes()
    {
        var fixture = new JournalStorageFixture();
        Exception? fixturePrimary = null;
        try
        {
        var paths = Enumerable.Range(0, 3).Select(i => Path.Combine(fixture.ModelVersionRoot, "retained-resource-" + i + ".fixture")).ToArray();
        byte[][] expected = [Encoding.UTF8.GetBytes("fixture-image-whole"), Encoding.UTF8.GetBytes("fixture-helper-whole"), Encoding.UTF8.GetBytes("fixture-setup-whole")];
        for (var i = 0; i < paths.Length; i++) File.WriteAllBytes(paths[i], expected[i]);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var fault = new IOException("controlled post-acquisition revalidation fault");
        var originalCancellation = new OperationCanceledException(cancellation.Token);
        foreach (var mode in new[] { "false", "fault", "cancellation", "accepted" })
        {
            var acquired = new List<FileStream>();
            Exception? acquisitionPrimary = null;
            try
            {
            foreach (var path in paths) acquired.Add(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            var handles = acquired.Select(f => f.SafeFileHandle).ToArray(); var reached = false;
            RetainedFixtureFiles? Transfer() => CodeDocumentationNativeCodexInstalledResources.TransferRetainedFiles(acquired,
                files => new RetainedFixtureFiles(files), owner =>
                {
                    reached = true; Assert.Equal(3, owner.Files.Length); Assert.All(handles, h => Assert.False(h.IsClosed));
                    if (OperatingSystem.IsWindows()) foreach (var path in paths) Assert.Throws<IOException>(() => { using var denied = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.None); });
                    if (mode == "fault") throw fault;
                    if (mode == "cancellation") throw originalCancellation;
                    return mode == "accepted";
                });
            if (mode == "fault") Assert.Same(fault, Assert.Throws<IOException>(() => Transfer()));
            else if (mode == "cancellation") Assert.Same(originalCancellation, Assert.Throws<OperationCanceledException>(() => Transfer()));
            else if (mode == "accepted") { using var accepted = Transfer(); Assert.NotNull(accepted); Assert.All(handles, h => Assert.False(h.IsClosed)); }
            else Assert.Null(Transfer());
            Assert.True(reached); Assert.Empty(acquired); Assert.All(handles, h => Assert.True(h.IsClosed));
            for (var i = 0; i < paths.Length; i++)
            {
                using var exclusive = new FileStream(paths[i], FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                using var bytes = new MemoryStream(); exclusive.CopyTo(bytes); Assert.Equal(expected[i], bytes.ToArray());
            }
            }
            catch (Exception error) { acquisitionPrimary = error; throw; }
            finally
            {
                // These are fixture-owned leftovers only; the assertions above still require the production transfer to close its own handles.
                Exception? cleanupFailure = null;
                foreach (var file in acquired) try { file.Dispose(); } catch (Exception error) { cleanupFailure ??= error; }
                acquired.Clear();
                if (cleanupFailure is not null)
                {
                    if (acquisitionPrimary is null) throw cleanupFailure;
                    acquisitionPrimary.Data["NativeResourceFixtureHandleCleanupFailure"] = cleanupFailure;
                }
            }
        }
        }
        catch (Exception error) { fixturePrimary = error; throw; }
        finally
        {
            // Directory teardown must not replace a load/assertion/transfer failure with an in-use file error.
            try { fixture.Dispose(); }
            catch (Exception cleanupFailure) when (fixturePrimary is not null) { fixturePrimary.Data["NativeResourceFixtureDirectoryCleanupFailure"] = cleanupFailure; }
        }
    }

    [Fact] // Exercise the exact production environment canonicalizer and pin with independently specified public fixture bytes.
    public void NativeEnvironment_CompleteUnicodeDriveEntriesAndEmptyBlockRemainExact()
    {
        KeyValuePair<string, string>[] entries = [new("UNLISTED", "whole-public-suffix"), new("UNICODE", "שלום😀漢字=tail"), new("EMPTY", ""), new("=C:", "C:\\目录")];
        var expected = "=C:=C:\\目录\0EMPTY=\0UNICODE=שלום😀漢字=tail\0UNLISTED=whole-public-suffix\0\0".ToCharArray();
        using var retained = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureEntries(entries);
        entries[0] = new("UNLISTED", "changed-after-capture"); // Mutating the original collection cannot mutate the complete held preimage.
        using (var first = retained.Pin())
        {
            Assert.NotEqual(IntPtr.Zero, first.Pointer); Assert.Equal(expected.Length, first.CodeUnits);
            var actual = new char[first.CodeUnits]; System.Runtime.InteropServices.Marshal.Copy(first.Pointer, actual, 0, actual.Length);
            Assert.Equal(expected, actual); Assert.Equal('\0', actual[^1]); Assert.Equal('\0', actual[^2]);
            System.Runtime.InteropServices.Marshal.WriteInt16(first.Pointer, 0, (short)'!'); // The actual native-address copy must also be defensive against modification.
        }
        using (var second = retained.Pin())
        {
            var actual = new char[second.CodeUnits]; System.Runtime.InteropServices.Marshal.Copy(second.Pointer, actual, 0, actual.Length);
            Assert.Equal(expected, actual); // A fresh pin retains the full original Unicode/unknown-variable preimage.
        }
        using (var nativeFixture = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureBlock(expected))
        using (var block = nativeFixture.Pin())
        {
            var actual = new char[block.CodeUnits]; System.Runtime.InteropServices.Marshal.Copy(block.Pointer, actual, 0, actual.Length);
            Assert.Equal(expected, actual); // The very parser used after GetEnvironmentStringsW must retain '=C:' and all complete Unicode values too.
        }
        using var empty = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureEntries([]);
        using var emptyBlock = empty.Pin(); var emptyChars = new char[emptyBlock.CodeUnits];
        System.Runtime.InteropServices.Marshal.Copy(emptyBlock.Pointer, emptyChars, 0, emptyChars.Length);
        Assert.Equal(new char[] { '\0', '\0' }, emptyChars); // An empty explicit environment still has two terminal UTF16 NULs.
    }

    [Fact] // Reject malformed complete inventories rather than silently dropping or overwriting environment entries.
    public void NativeEnvironment_DuplicateCaseAndInvalidWindowsEntriesRefuseWithoutValues()
    {
        KeyValuePair<string, string>[][] invalid = [
            [new("PUBLIC", "fixture-secret-marker"), new("public", "other")],
            [new("", "fixture-secret-marker")], [new("bad=name", "fixture-secret-marker")],
            [new("=C", "fixture-secret-marker")], [new("=1:", "fixture-secret-marker")], [new("=C:=", "fixture-secret-marker")],
            [new("PUBLIC\0TAIL", "fixture-secret-marker")], [new("PUBLIC", "fixture-secret-marker\0tail")],
            [new(null!, "fixture-secret-marker")], [new("PUBLIC", null!)]
        ];
        foreach (var entries in invalid)
        {
            var refused = Assert.Throws<IOException>(() => CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureEntries(entries));
            Assert.DoesNotContain("fixture-secret-marker", refused.Message); Assert.DoesNotContain("bad=name", refused.Message);
        }
        foreach (var block in new[] { "PUBLIC=fixture-secret-marker\0", "PUBLIC=fixture-secret-marker\0\0TAIL=whole\0\0", "PUBLIC=x\0public=y\0\0", "=C=fixture-secret-marker\0\0", "MISSING_DELIMITER\0\0" })
        {
            var refused = Assert.Throws<IOException>(() => CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureBlock(block.AsSpan()));
            Assert.DoesNotContain("fixture-secret-marker", refused.Message); // Incomplete inventory and native-block duplicates use the same nonsecret refusal path.
        }
    }

    [Fact] // Observe a real controlled ambient mutation through the same current-read code used before production creation.
    public void NativeEnvironment_ActualAmbientMutationPermanentlyInvalidatesHeldPreimage()
    {
        var name = "XIIGEN_SOURCE61_PUBLIC_ENV_" + Guid.NewGuid().ToString("N");
        var original = Environment.GetEnvironmentVariable(name); Assert.Null(original); // The controlled unique fixture never reads or reports another variable's value.
        Exception? primary = null;
        try
        {
            Environment.SetEnvironmentVariable(name, "SOURCE61-public-א😀");
            using var retained = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent();
            Assert.True(retained.IsCurrent());
            Environment.SetEnvironmentVariable(name, "SOURCE61-public-changed");
            Assert.False(retained.IsCurrent());
            Environment.SetEnvironmentVariable(name, "SOURCE61-public-א😀");
            Assert.False(retained.IsCurrent()); // Restoring bytes cannot renew the original retained data owner.
            Assert.Throws<IOException>(() => retained.Pin());
            using var restored = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent();
            Assert.True(restored.IsCurrent()); // Actual fresh capture confirms restoration independently from the invalidated owner.
            restored.Dispose(); Assert.False(restored.IsCurrent()); Assert.Throws<IOException>(() => restored.Pin());
        }
        catch (Exception error) { primary = error; throw; }
        finally
        {
            try { Environment.SetEnvironmentVariable(name, original); }
            catch (Exception cleanup) when (primary is not null) { primary.Data["PublicEnvironmentFixtureRestore"] = cleanup.GetType().Name + ":" + cleanup.HResult; }
        }
        Assert.Equal(original, Environment.GetEnvironmentVariable(name)); // The ambient fixture leaves no persistent process input.
    }

    [Fact] // The actual finite UTF16 guard precedes the shared pin allocation, with a complete within-bound control.
    public void NativeEnvironment_OverboundRefusesBeforePinWithoutTruncation()
    {
        var maximum = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.MaximumCodeUnits;
        using (var admitted = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureEntries([new("X", new string('界', maximum - 4))]))
        using (var block = admitted.Pin())
        {
            Assert.Equal(maximum, block.CodeUnits); Assert.Equal((short)'界', System.Runtime.InteropServices.Marshal.ReadInt16(block.Pointer, (maximum - 3) * sizeof(char)));
            Assert.Equal((short)0, System.Runtime.InteropServices.Marshal.ReadInt16(block.Pointer, (maximum - 2) * sizeof(char)));
            Assert.Equal((short)0, System.Runtime.InteropServices.Marshal.ReadInt16(block.Pointer, (maximum - 1) * sizeof(char)));
        }
        var allocations = 0;
        var refused = Assert.Throws<IOException>(() =>
        {
            using var oversized = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureEntries([new("X", new string('界', maximum - 3))]);
            using var unreachable = oversized.Pin(_ => allocations++); // This is the real production allocation helper, never an OS create or fabricated root.
        });
        Assert.Equal(0, allocations); Assert.Contains("finite UTF16 allocation bound", refused.Message);
    }

    [Fact] // Preserve an exact post-allocation fault/cancellation and release the actual pin before it leaves the shared helper.
    public void NativeEnvironment_PostAllocationFaultAndCancellationReleaseActualPin()
    {
        using var retained = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureEntries([new("PUBLIC", "fixture-😀")]);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Exception[] faults = [new IOException("controlled environment pin fault"), new OperationCanceledException(cancellation.Token)];
        foreach (var original in faults)
        {
            CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.PinnedBlock? observed = null;
            var thrown = Assert.ThrowsAny<Exception>(() => retained.Pin(block =>
            {
                observed = block; Assert.False(block.IsReleased); Assert.NotEqual(IntPtr.Zero, block.Pointer); throw original;
            }));
            Assert.Same(original, thrown); Assert.NotNull(observed); Assert.True(observed!.IsReleased);
            Assert.Throws<ObjectDisposedException>(() => observed.Pointer);
            if (thrown is OperationCanceledException canceled) Assert.Equal(cancellation.Token, canceled.CancellationToken);
        }
        using var accepted = retained.Pin(); Assert.False(accepted.IsReleased); accepted.Dispose(); Assert.True(accepted.IsReleased);
        accepted.Dispose(); Assert.True(accepted.IsReleased); // Cleanup is idempotent and cannot expose a null/inherited fallback pointer.
    }

    [Fact] // Exercise the production transfer/release boundary with real public canonical arrays, including failure after capture succeeds.
    public void NativeEnvironment_SnapshotReleaseFailureClearsPendingCanonical()
    {
        var expected = "=C:=C:\\公开\0PUBLIC=שלום😀=complete\0\0".ToCharArray();
        foreach (var mode in new[] { "success", "false", "throw" })
        {
            CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment? captured = null, accepted = null;
            char[]? canonical = null; var captures = 0; var releases = 0;
            var releaseFault = new IOException("controlled public snapshot release fault");
            try
            {
                CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment Transfer() =>
                    CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureAndReleaseSnapshot(() =>
                    {
                        captures++; captured = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureBlock(expected);
                        canonical = Assert.IsType<char[]>(typeof(CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment)
                            .GetField("canonical", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(captured)); // Inspect only this controlled public fixture's actual retained array.
                        return captured;
                    }, () =>
                    {
                        releases++; Assert.Equal(1, captures); Assert.Equal(expected, canonical); Assert.NotNull(captured);
                        using var beforeRelease = captured!.Pin(); Assert.False(beforeRelease.IsReleased); // Release starts while the complete canonical owner is still pending and valid.
                        if (mode == "throw") throw releaseFault;
                        return mode == "success";
                    });
                if (mode == "success")
                {
                    accepted = Transfer(); Assert.Same(captured, accepted); Assert.Equal(expected, canonical);
                    using var block = accepted.Pin(); var actual = new char[block.CodeUnits];
                    System.Runtime.InteropServices.Marshal.Copy(block.Pointer, actual, 0, actual.Length);
                    Assert.Equal(expected, actual); // Transfer preserves the whole Unicode/hidden-drive preimage only after successful release.
                }
                else
                {
                    var thrown = Assert.ThrowsAny<Exception>(() => accepted = Transfer()); Assert.Null(accepted);
                    if (mode == "throw") Assert.Same(releaseFault, thrown);
                    else Assert.Equal("Native environment snapshot release failed.", Assert.IsType<IOException>(thrown).Message);
                    Assert.NotNull(canonical); Assert.All(canonical!, unit => Assert.Equal('\0', unit)); // The very array allocated by CaptureBlock must have been cleared, not just an unrelated temporary copy.
                    Assert.NotNull(captured); Assert.False(captured!.IsCurrent()); Assert.Throws<IOException>(() => captured.Pin());
                }
                Assert.Equal(1, captures); Assert.Equal(1, releases); // Neither failed release outcome renews or retries the native snapshot release.
            }
            finally { accepted?.Dispose(); captured?.Dispose(); } // No successful fixture owner survives an assertion fault or an unexpected transfer result.
        }
        var currentReleases = 0; CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment? actualCurrent = null;
        using var current = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureAndReleaseSnapshot(
            () => actualCurrent = CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureCurrent(),
            () => { currentReleases++; Assert.NotNull(actualCurrent); Assert.True(actualCurrent!.IsCurrent()); return true; });
        Assert.Same(actualCurrent, current); Assert.Equal(1, currentReleases); Assert.True(current.IsCurrent()); // Actual current-read is a success control; no ambient values are inspected or printed.
        using var currentPin = current.Pin(); Assert.False(currentPin.IsReleased); Assert.NotEqual(IntPtr.Zero, currentPin.Pointer);
    }

    [Fact] // Earlier capture/parser/cancellation must keep its exact primary object while the one actual release outcome remains bounded secondary evidence.
    public void NativeEnvironment_SnapshotReleasePreservesEarlierPrimaryAndToken()
    {
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        foreach (var canceled in new[] { false, true })
        foreach (var releaseMode in new[] { "success", "false", "throw" })
        {
            Exception original = canceled ? new OperationCanceledException(cancellation.Token) : new IOException("controlled public capture fault");
            var releaseFault = new IOException("public-release-sentinel-not-secondary-text"); var captures = 0; var releases = 0;
            var thrown = Assert.ThrowsAny<Exception>(() => CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureAndReleaseSnapshot(
                () => { captures++; throw original; }, () => { releases++; if (releaseMode == "throw") throw releaseFault; return releaseMode == "success"; }));
            Assert.Same(original, thrown); Assert.Equal(1, captures); Assert.Equal(1, releases);
            if (canceled) Assert.Equal(cancellation.Token, Assert.IsType<OperationCanceledException>(thrown).CancellationToken);
            if (releaseMode == "success") Assert.False(thrown.Data.Contains("NativeEnvironmentSnapshotCleanup"));
            else
            {
                var detail = Assert.IsType<string>(thrown.Data["NativeEnvironmentSnapshotCleanup"]);
                Assert.Equal(releaseMode == "false" ? "FreeEnvironmentStringsW:false" : "FreeEnvironmentStringsW:IOException:" + releaseFault.HResult, detail);
                Assert.DoesNotContain("public-release-sentinel", detail); // No release exception message or environment value leaks into retained secondary text.
            }
        }
        Exception? actualParserFault = null; var parserReleases = 0;
        var parserThrown = Assert.Throws<IOException>(() => CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureAndReleaseSnapshot(() =>
        {
            try { return CodeDocumentationNativeCodexInstalledResources.RetainedEnvironment.CaptureBlock("PUBLIC=x\0public=y\0\0".AsSpan()); }
            catch (Exception error) { actualParserFault = error; throw; } // Capture a genuine shared-parser duplicate-name refusal, not a fabricated parser result.
        }, () => { parserReleases++; return false; }));
        Assert.Same(actualParserFault, parserThrown); Assert.Equal(1, parserReleases);
        Assert.Equal("FreeEnvironmentStringsW:false", Assert.IsType<string>(parserThrown.Data["NativeEnvironmentSnapshotCleanup"]));
    }

    private sealed class RetainedFixtureFiles(FileStream[] files) : IDisposable // Data-only test owner cannot construct the private installed-resource type.
    { internal FileStream[] Files => files; public void Dispose() { foreach (var file in files) file.Dispose(); } }

    [Fact] // Confirm caller cancellation from real stdout delivery after the owned native root already exited.
    public async Task NativeProcess_LateOutputCancellationPreservesTokenAfterRootExit()
    {
        if (!OperatingSystem.IsWindows()) { await AssertNativeProcessPlatformRefusal(); return; }
        using var cancellation = new CancellationTokenSource();
        var thrown = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CodeDocumentationNativeCodexWindowsProcess.RunLateCancellationCanaryForTestingAsync(Path.GetTempPath(), cancellation));
        Assert.Equal(cancellation.Token, thrown.CancellationToken);
        Assert.Equal(true, thrown.Data["NativeProcessTreeClosed"]); Assert.Equal(true, thrown.Data["NativeProcessStreamTasksTerminal"]);
        Assert.Equal(true, thrown.Data["NativeProcessCancellationOutputWitness"]); Assert.Equal(true, thrown.Data["NativeProcessCancellationAfterRootExitWitness"]);
        Assert.NotEqual(0u, Assert.IsType<uint>(thrown.Data["NativeProcessRootPid"]));
        Assert.True(Assert.IsType<long>(thrown.Data["NativeProcessRootBirthFileTime"]) > 0);
        // The raw lifetime Job count may include terminated members; root identity remains the exact CreateProcessW PID/GetProcessTimes birth checked above.
        Assert.True(Assert.IsType<uint>(thrown.Data["NativeProcessTotalProcesses"]) >= 1);
        Assert.Equal(true, thrown.Data["NativeProcessRootExitObserved"]); Assert.Equal(0u, Assert.IsType<uint>(thrown.Data["NativeProcessExitCode"]));
        Assert.Equal(false, thrown.Data["NativeProcessDescendantsAfterRoot"]); Assert.Equal(false, thrown.Data["NativeProcessOutputOverflow"]);
    }

    [Fact] // A real faulted stream must not skip independent pending reader/writer cleanup.
    public async Task NativeProcess_IndependentStreamCleanupClosesWriterAfterOutputFault()
    {
        var original = new IOException("controlled stdout fault");
        using var stdout = new ControlledOwnedStream(original); using var stderr = new ControlledOwnedStream(); using var stdin = new ControlledOwnedStream();
        async Task<byte[]> Read(ControlledOwnedStream stream) { await stream.ReadAsync(new Memory<byte>(new byte[1])); return []; }
        var outputTask = Read(stdout); var errorTask = Read(stderr); var inputTask = stdin.WriteAsync(new ReadOnlyMemory<byte>(new byte[] { 1 })).AsTask();
        Assert.True(outputTask.IsFaulted); Assert.False(errorTask.IsCompleted); Assert.False(inputTask.IsCompleted);
        Assert.True(stdout.ReadStarted); Assert.True(stderr.ReadStarted); Assert.True(stdin.WriteStarted);
        var cancellations = 0;
        var closed = await CodeDocumentationNativeCodexWindowsProcess.CloseOwnedStreamsForTestingAsync(outputTask, errorTask, inputTask,
            () => cancellations++, stdout.Dispose, stderr.Dispose, stdin.Dispose);
        Assert.Equal(1, cancellations); Assert.Equal(1, stdout.DisposeCount); Assert.Equal(1, stderr.DisposeCount); Assert.Equal(1, stdin.DisposeCount);
        Assert.True(outputTask.IsCompleted); Assert.True(errorTask.IsCompleted); Assert.True(inputTask.IsCompleted); Assert.True(closed.AllTerminal);
        Assert.Contains("stdout:IOException:", closed.Failure); Assert.Contains("stderr:deadline", closed.Failure); Assert.Contains("stdin:deadline", closed.Failure);
        Assert.Same(original, outputTask.Exception!.InnerException); Assert.Empty(closed.StandardOutput); Assert.Empty(closed.StandardError);
        // A deliberately unresponsive fixture task remains explicit unknown; release it after observing the falsifier so no task survives the test.
        var nonterminal = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            var unknown = await CodeDocumentationNativeCodexWindowsProcess.CloseOwnedStreamsForTestingAsync(nonterminal.Task, Task.FromResult(Array.Empty<byte>()), Task.CompletedTask,
                () => { }, () => { }, () => { }, () => { });
            Assert.False(unknown.AllTerminal); Assert.False(unknown.OutputTerminal); Assert.Contains("stdout:nonterminal", unknown.Failure);
        }
        finally { nonterminal.TrySetResult([]); await nonterminal.Task; }
    }

    [Fact] // Combine actual bounded I/O cleanup faults and late caller cancellation at the production final-primary decision.
    public async Task NativeProcess_FinalPrimaryRetainsStreamCauseAfterLateCancellation()
    {
        foreach (var leaveReaderOpen in new[] { false, true })
        {
            using var cancellation = new CancellationTokenSource();
            var streamFault = new IOException("controlled combined stdout fault");
            using var stdout = new ControlledOwnedStream(streamFault); using var stderr = new ControlledOwnedStream(); using var stdin = new ControlledOwnedStream();
            async Task<byte[]> Read(ControlledOwnedStream stream) { await stream.ReadAsync(new Memory<byte>(new byte[1])); return []; }
            var outputTask = Read(stdout); var errorTask = Read(stderr); var inputTask = stdin.WriteAsync(new ReadOnlyMemory<byte>(new byte[] { 1 })).AsTask();
            try
            {
                Assert.False(cancellation.IsCancellationRequested); Assert.True(outputTask.IsFaulted); Assert.False(errorTask.IsCompleted); Assert.False(inputTask.IsCompleted);
                var cancellations = 0;
                var closed = await CodeDocumentationNativeCodexWindowsProcess.CloseOwnedStreamsForTestingAsync(outputTask, errorTask, inputTask,
                    () => { cancellations++; cancellation.Cancel(); }, stdout.Dispose, () => { if (!leaveReaderOpen) stderr.Dispose(); }, stdin.Dispose);
                Assert.Equal(1, cancellations); Assert.True(cancellation.IsCancellationRequested);
                Assert.True(stdout.ReadStarted); Assert.True(stderr.ReadStarted); Assert.True(stdin.WriteStarted);
                Assert.Equal(1, stdout.DisposeCount); Assert.Equal(leaveReaderOpen ? 0 : 1, stderr.DisposeCount); Assert.Equal(1, stdin.DisposeCount);
                Assert.Same(streamFault, outputTask.Exception!.InnerException);
                Assert.True(outputTask.IsCompleted); Assert.True(inputTask.IsCompleted); Assert.Equal(!leaveReaderOpen, errorTask.IsCompleted);
                Assert.True(closed.OutputTerminal); Assert.True(closed.InputTerminal); Assert.Equal(!leaveReaderOpen, closed.ErrorTerminal); Assert.Equal(!leaveReaderOpen, closed.AllTerminal);
                Assert.Empty(closed.StandardOutput); Assert.Empty(closed.StandardError);
                Assert.Contains("stdout:IOException:", closed.Failure); Assert.Contains("stderr:deadline", closed.Failure); Assert.Contains("stdin:deadline", closed.Failure);
                if (leaveReaderOpen) Assert.Contains("stderr:nonterminal", closed.Failure);
                // This is the same production decision used after native cleanup, with its real I/O result; no process/root-exit witness is fabricated.
                var final = CodeDocumentationNativeCodexWindowsProcess.ResolveFinalPrimary(null, cancellation.Token, closed);
                Assert.NotNull(final); var late = Assert.Throws<OperationCanceledException>(() => final!.Throw());
                Assert.Same(final!.SourceException, late); Assert.Equal(cancellation.Token, late.CancellationToken);
                Assert.Equal(closed.Failure, Assert.IsType<string>(late.Data["NativeProcessStreams"]));
                Assert.False(late.Data.Contains("NativeProcessRootPid")); // The portable arbitration fixture makes no native-root execution claim.
                var earlier = new InvalidOperationException("controlled earlier process error");
                var captured = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(earlier);
                var retained = CodeDocumentationNativeCodexWindowsProcess.ResolveFinalPrimary(captured, cancellation.Token, closed);
                Assert.Same(captured, retained); Assert.Same(earlier, Assert.Throws<InvalidOperationException>(() => retained!.Throw()));
                Assert.Equal(closed.Failure, Assert.IsType<string>(earlier.Data["NativeProcessStreams"]));
                Assert.Null(CodeDocumentationNativeCodexWindowsProcess.ResolveFinalPrimary(null, CancellationToken.None, closed)); // Without an earlier error or cancellation, cleanup remains explicit observation data.
            }
            finally
            {
                stdout.Dispose(); stderr.Dispose(); stdin.Dispose(); // Release the deliberate nonterminal reader even if an assertion fails.
                try { await Task.WhenAll(outputTask, errorTask, inputTask); } catch { } // Observe all controlled terminal faults after every resource has been released.
            }
            Assert.True(outputTask.IsCompleted); Assert.True(errorTask.IsCompleted); Assert.True(inputTask.IsCompleted);
        }
    }

    [Fact] // Actual shared drainage proves overflow delivered after an earlier root-result observation; this portable fixture claims no native PID or process execution.
    public async Task NativeProcess_PostRootDrainOverflowOverridesNonzeroAndPreservesUnknownPrimary()
    {
        var actualBytes = Encoding.UTF8.GetBytes("SOURCE62-" + new string('X', 128));
        foreach (var leaveErrorOpen in new[] { false, true })
        {
            using var stdout = new MemoryStream(actualBytes); using var stderr = new ControlledOwnedStream();
            using var cancellation = new CancellationTokenSource();
            var budget = new CodeDocumentationNativeCodexWindowsProcess.DrainBudget(128);
            var deliveryAfterRoot = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var outputTask = CodeDocumentationNativeCodexWindowsProcess.DrainStreamForTestingAsync(stdout, budget, deliveryAfterRoot.Task);
            var errorTask = CodeDocumentationNativeCodexWindowsProcess.DrainStreamForTestingAsync(stderr, budget);
            try
            {
                Assert.Equal((long)actualBytes.Length, stdout.Position); // A real read occurred, but the production delivery gate has not yet charged its bytes.
                Assert.False(outputTask.IsCompleted); Assert.False(budget.Overflow); Assert.True(stderr.ReadStarted); Assert.False(errorTask.IsCompleted);
                const string priorRootFailure = "nonzero-exit"; // Only earlier-result data is supplied; no native root/Job identity is fabricated by this controlled fixture.
                Assert.Equal(priorRootFailure, CodeDocumentationNativeCodexWindowsProcess.ResolveFinalFailure(priorRootFailure, budget, true, null));
                deliveryAfterRoot.SetResult(true); // Match the production ordering: a confirmed root observation releases actual read delivery afterward.
                var overflow = await Assert.ThrowsAsync<IOException>(() => outputTask); // The actual shared DrainBudget.Reserve path creates this exception from the real oversized chunk.
                Assert.True(budget.Overflow); Assert.Same(overflow, outputTask.Exception!.InnerException);
                var cleanup = await CodeDocumentationNativeCodexWindowsProcess.CloseOwnedStreamsForTestingAsync(outputTask, errorTask, Task.CompletedTask,
                    cancellation.Cancel, stdout.Dispose, () => { if (!leaveErrorOpen) stderr.Dispose(); }, () => { });
                Assert.Equal(!leaveErrorOpen, cleanup.AllTerminal); Assert.True(cleanup.OutputTerminal); Assert.True(cleanup.InputTerminal);
                Assert.Equal(!leaveErrorOpen, cleanup.ErrorTerminal); Assert.Contains("stdout:IOException:", cleanup.Failure); Assert.Contains("stderr:deadline", cleanup.Failure);
                Assert.Empty(cleanup.StandardOutput); Assert.Empty(cleanup.StandardError);
                var finalFailure = CodeDocumentationNativeCodexWindowsProcess.ResolveFinalFailure(priorRootFailure, budget, true, cleanup);
                Assert.Equal(leaveErrorOpen ? "stream-tasks-nonterminal" : "output-overflow", finalFailure); // Unknown stream closure stays visible; terminal output overflow cannot remain merely nonzero-exit.
                if (leaveErrorOpen) Assert.Contains("stderr:nonterminal", cleanup.Failure);
                var originalCancellation = new OperationCanceledException(cancellation.Token);
                var originalDispatch = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(originalCancellation);
                var finalPrimary = CodeDocumentationNativeCodexWindowsProcess.ResolveFinalPrimary(originalDispatch, cancellation.Token, cleanup);
                Assert.Same(originalDispatch, finalPrimary); Assert.Same(originalCancellation, Assert.Throws<OperationCanceledException>(() => finalPrimary!.Throw()));
                Assert.Equal(cancellation.Token, originalCancellation.CancellationToken); Assert.Equal(cleanup.Failure, originalCancellation.Data["NativeProcessStreams"]);
                Assert.False(originalCancellation.Data.Contains("NativeProcessRootPid")); // This fixture tests actual I/O and final arbitration, not a fabricated Windows inventory.
            }
            finally
            {
                deliveryAfterRoot.TrySetResult(false); stdout.Dispose(); stderr.Dispose(); // Release every real fixture stream and any gated delivery even if an assertion fails.
                try { await Task.WhenAll(outputTask, errorTask); } catch { } // Observe all actual terminal fixture errors after unconditional disposal.
            }
            Assert.True(outputTask.IsCompleted); Assert.True(errorTask.IsCompleted);
        }
        using var control = new MemoryStream(Encoding.UTF8.GetBytes("SOURCE62-CONTROL"));
        var withinBudget = new CodeDocumentationNativeCodexWindowsProcess.DrainBudget(128);
        var controlBytes = await CodeDocumentationNativeCodexWindowsProcess.DrainStreamForTestingAsync(control, withinBudget);
        Assert.Equal("SOURCE62-CONTROL", Encoding.UTF8.GetString(controlBytes)); Assert.False(withinBudget.Overflow);
        var controlCleanup = await CodeDocumentationNativeCodexWindowsProcess.CloseOwnedStreamsForTestingAsync(Task.FromResult(controlBytes), Task.FromResult(Array.Empty<byte>()), Task.CompletedTask,
            () => { }, control.Dispose, () => { }, () => { });
        Assert.True(controlCleanup.AllTerminal); Assert.Null(controlCleanup.Failure);
        Assert.Equal("nonzero-exit", CodeDocumentationNativeCodexWindowsProcess.ResolveFinalFailure("nonzero-exit", withinBudget, true, controlCleanup)); // A genuine within-budget read does not relabel an earlier nonzero result as overflow.
    }

    private sealed class ControlledOwnedStream(Exception? readFault = null) : Stream // Isolated deterministic I/O fixture; it never launches a process or imports authority.
    {
        private readonly TaskCompletionSource<int> read = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource write = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool ReadStarted, WriteStarted; internal int DisposeCount;
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken token = default)
        { ReadStarted = true; return new(readFault is null ? read.Task : Task.FromException<int>(readFault)); }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken token = default)
        { WriteStarted = true; return new(write.Task); }
        protected override void Dispose(bool disposing)
        { if (disposing && DisposeCount++ == 0) { if (ReadStarted && readFault is null) read.TrySetException(new ObjectDisposedException("controlled-reader")); if (WriteStarted) write.TrySetException(new ObjectDisposedException("controlled-writer")); } base.Dispose(disposing); }
        public override bool CanRead => true; public override bool CanWrite => true; public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException(); public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { } public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException(); public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    private static async Task AssertNativeProcessPlatformRefusal()
    {
        var refusal = await Assert.ThrowsAsync<PlatformNotSupportedException>(() =>
            CodeDocumentationNativeCodexWindowsProcess.RunHarmlessCanaryForTestingAsync(Path.GetTempPath(),
                "echo SOURCE56", 1024, TimeSpan.FromSeconds(5), CancellationToken.None));
        Assert.Equal("Native Codex process ownership requires Windows.", refusal.Message);
    }

    [Fact] // Discover this bounded regression through the existing xUnit project.
    public async Task NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam() // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
    { // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
        using var p = new ProtectedDisclosureHarness(nativePreparation: true); var h = p.H; var baseline = (await h.Consume()).Value!; Assert.NotNull(baseline); // Full current receiving/raw/disclosure owners remain mandatory.
        Assert.True((await p.PrepareNative(baseline)).IsSuccess); var saved = NativePreparationBytes(h.Open.Session); var directory = CreateNativeStartedFixtureInputs(p); // Establish a valid PreparedUnsent control before the explicit fixture entry.
        var owner = h.Open.Session; var points = new List<string>(); CodeDocumentationOneLessonSession.NativeSelectionLaunchBindingV1? binding = null; CodeDocumentationNativeCodexSelectionHost? host = null; // Observe real private lifetime without supplying result authority.
        var startedPath = Path.Combine(Path.GetDirectoryName(h.Fixture.OrdinaryHistory)!, CodeDocumentationNativeCodexSelectionWire.StartedFileName); // Use the actual fixed native leaf.
        try // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
        { // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
            owner.SelectionMaterialPointForTesting = point => // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
            { // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
                points.Add(point); // Preserve observed order, including independent post-consumer finals.
                if (point == "native-launch-binding-issued") // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
                { // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
                    binding = (CodeDocumentationOneLessonSession.NativeSelectionLaunchBindingV1)FreshSessionField(owner, "_nativeLaunchBinding")!; host = (CodeDocumentationNativeCodexSelectionHost)FreshSessionField(owner, "_nativeLaunchHost")!; Assert.True(binding.IsActiveFor(owner, host)); // Only this genuine operation holds the binding.
                    Assert.False(CodeDocumentationOneLessonSession.NativeSelectionLaunchBindingV1.Issue(owner).IsSuccess); // A caller observing the binding cannot reuse the short private issuing scope.
                    var preparation = (CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt)FreshSessionField(owner, "_nativePreparationReceipt")!; var foreignConsumer = CodeDocumentationNativeCodexSelectionHost.FromHeldSession(owner, preparation).Value!; Assert.NotNull(foreignConsumer); Assert.False(binding.IsActiveFor(owner, foreignConsumer)); // Same metadata/receipt does not authorize another Host instance.
                    Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, p.PrepareNative(baseline).GetAwaiter().GetResult().ErrorCode); // Reentry refuses while the original CAS remains held.
                } // Finish this bounded owner check without renewing authority or an attempt.
                if (point == "native-first-process-create-seam-synthetic") // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
                { // Verify the actual NativeStarted_PrivateBindingDurableReadbackPrecedesSyntheticCreateSeam boundary.
                    Assert.Contains("native-started-flushed", points); Assert.Contains("native-started-readback-staged", points); Assert.True(points.IndexOf("native-started-flushed") < points.IndexOf("native-started-readback-staged")); // The actual full durable write/readback precedes the injectable creation seam.
                    var bytes = ((byte[])FreshSessionField(owner, "_nativeStartedBytes")!).ToArray(); using var file = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(startedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.None); Assert.Equal(bytes, SecureLocalFileBoundary.ReadBoundedStableBytes(file, 8192)); // Independently inspect the complete physical Started at the seam.
                    var data = CodeDocumentationNativeCodexSelectionWire.DecodeStarted(bytes); Assert.True(data.IsSuccess); Assert.Equal(CodeDocumentationNativeCodexLaunchInputOwner.SyntheticKind, data.Value!.LaunchKind); Assert.Equal(CodeDocumentationNativeCodexSelectionWire.Sha256(saved.Request), data.Value.PreparedRequestSha256); Assert.Equal(CodeDocumentationNativeCodexSelectionWire.DecodePrepared(saved.Request).Value!.Nonce, data.Value.Nonce); // Fixture storage is labelled and bound to the exact existing request/nonce.
                } // Finish this bounded owner check without renewing authority or an attempt.
            }; // Finish this bounded owner check without renewing authority or an attempt.
            var attempt = await owner.CaptureSyntheticNativeStartedFixtureAsync(baseline, p.Startup, h.Fixture.OrdinarySelector, directory); Assert.False(attempt.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, attempt.ErrorCode); // The fixture exercises the fixed storage consumer and conservatively ends unknown, without OS creation.
        } // Finish this bounded owner check without renewing authority or an attempt.
        finally { owner.SelectionMaterialPointForTesting = null; } // Restore observation even on a failed assertion.
        Assert.Equal(1, points.Count(point => point == "native-first-process-create-seam-synthetic")); // Require the positive storage witness to reach its observation seam exactly once; no OS process is created.
        Assert.NotNull(binding); Assert.NotNull(host); Assert.False(binding!.IsActiveFor(owner, host!)); AssertNativePreparationReleased(owner); Assert.Equal((1, 0, true), owner.NativeStartedAccounting); // Expired binding and attempt accounting survive no-relaunch cleanup.
        var retained = ReadNativeStartedFixtureEvidence(startedPath); var timestamp = File.GetLastWriteTimeUtc(startedPath); var nonce = CodeDocumentationNativeCodexSelectionWire.DecodeStarted(retained).Value!.Nonce; // Snapshot actual saved unknown evidence through an independent compatible reader.
        var repeat = await p.PrepareNative(baseline); Assert.False(repeat.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, repeat.ErrorCode); Assert.Equal(retained, ReadNativeStartedFixtureEvidence(startedPath)); Assert.Equal(timestamp, File.GetLastWriteTimeUtc(startedPath)); Assert.Equal(nonce, CodeDocumentationNativeCodexSelectionWire.DecodePrepared(NativePreparationBytes(owner).Request).Value!.Nonce); // Same-owner retry neither renews nonce nor rewrites/timestamps evidence.
        h.Open.Dispose(); var reopened = h.Reopen(); Assert.False(reopened.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, reopened.ErrorCode); Assert.Null(reopened.Value); Assert.Equal(retained, ReadNativeStartedFixtureEvidence(startedPath)); Assert.Equal(timestamp, File.GetLastWriteTimeUtc(startedPath)); // Fresh opener cannot relaunch a Started-only/unknown attempt.
        AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); // Synthetic storage proves no real model/Teacher/optimizer execution.
    } // Finish this bounded owner check without renewing authority or an attempt.

    [Fact] // Discover this bounded regression through the existing xUnit project.
    public async Task NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence() // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
    { // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
        foreach (var fault in new[] { "partial", "image", "config", "permission", "cancel" }) // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
        { // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
            using var p = new ProtectedDisclosureHarness(nativePreparation: true); var h = p.H; var baseline = (await h.Consume()).Value!; Assert.NotNull(baseline); Assert.True((await p.PrepareNative(baseline)).IsSuccess); // Every fault starts from actual successful full protected preparation.
            var saved = NativePreparationBytes(h.Open.Session); var directory = CreateNativeStartedFixtureInputs(p); var owner = h.Open.Session; var points = new List<string>(); using var cancellation = new CancellationTokenSource(); cancellation.Cancel(); var primary = new OperationCanceledException("synthetic-native-started-cancel", null, cancellation.Token); // Retain the exact intended primary object/token.
            var path = Path.Combine(Path.GetDirectoryName(h.Fixture.OrdinaryHistory)!, CodeDocumentationNativeCodexSelectionWire.StartedFileName); var createSeams = 0; var staged = false; // Count the actual fixed creation seam, not an OS process.
            try // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
            { // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
                owner.SelectionMaterialPointForTesting = point => // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
                { // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
                    points.Add(point); if (point == "native-first-process-create-seam-synthetic") createSeams++; // Independently count whether changed/partial data reached the creation seam.
                    if (fault == "partial" && point == "native-started-file-created") throw primary; // An interrupted empty append remains physically retained and charged.
                    if (point == "native-started-readback-staged") // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
                    { // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
                        staged = true; if (fault == "cancel") throw primary; // Throw the actual exact OCE after successful full storage readback.
                        if (fault is "image" or "config" or "permission") // Verify the actual NativeStarted_PartialChangedInputsAndCancellationKeepUnknownEvidence boundary.
                        { var leaf = Path.Combine(directory, "synthetic-" + fault + ".input"); using var changed = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(leaf, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.WriteThrough); changed.Position = 0; changed.WriteByte((byte)'X'); changed.Flush(true); } // Actual changed bytes under the deliberately mutable synthetic input fixture invalidate the capability before the creation seam.
                    } // Finish this bounded owner check without renewing authority or an attempt.
                }; // Finish this bounded owner check without renewing authority or an attempt.
                if (fault is "partial" or "cancel") { var thrown = await Assert.ThrowsAsync<OperationCanceledException>(() => owner.CaptureSyntheticNativeStartedFixtureAsync(baseline, p.Startup, h.Fixture.OrdinarySelector, directory)); Assert.Same(primary, thrown); Assert.Equal(cancellation.Token, thrown.CancellationToken); } // No secondary final error may replace the primary OCE/token.
                else { var result = await owner.CaptureSyntheticNativeStartedFixtureAsync(baseline, p.Startup, h.Fixture.OrdinarySelector, directory); Assert.False(result.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, result.ErrorCode); } // Changed inputs retain exact recovery refusal.
            } // Finish this bounded owner check without renewing authority or an attempt.
            finally { owner.SelectionMaterialPointForTesting = null; } // Always restore the fixed observer.
            Assert.Equal(0, createSeams); Assert.Equal(fault != "partial", staged); Assert.True(File.Exists(path)); Assert.Equal((1, 0, true), owner.NativeStartedAccounting); AssertNativePreparationReleased(owner); // Neither partial nor changed/cancelled storage renews its attempt.
            foreach (var point in DisclosureFinalPoints.Append("native-prepared-family-final-none").Append("native-started-family-final-none").Append("native-launch-input-final-none")) Assert.True(points.Count(p => p == point) >= 2, fault + ": " + point); // Both prechecks and post-primary independent None attempts complete.
            var retained = ReadNativeStartedFixtureEvidence(path); var time = File.GetLastWriteTimeUtc(path);
            if (fault == "partial") Assert.Empty(retained); // The injected prewrite OCE preserves the real empty file, never a synthesized snapshot.
            else
            {
                var started = CodeDocumentationNativeCodexSelectionWire.DecodeStarted(retained); Assert.True(started.IsSuccess, started.ErrorCode);
                Assert.Equal(CodeDocumentationNativeCodexLaunchInputOwner.SyntheticKind, started.Value!.LaunchKind);
                Assert.Equal(CodeDocumentationNativeCodexSelectionWire.Sha256(saved.Request), started.Value.PreparedRequestSha256);
                Assert.Equal(CodeDocumentationNativeCodexSelectionWire.DecodePrepared(saved.Request).Value!.Nonce, started.Value.Nonce);
            }
            Assert.Equal(saved.Request, NativePreparationBytes(owner).Request); Assert.Equal(saved.Schema, NativePreparationBytes(owner).Schema); h.Open.Dispose(); var reopen = h.Reopen(); Assert.False(reopen.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, reopen.ErrorCode); Assert.Equal(retained, ReadNativeStartedFixtureEvidence(path)); Assert.Equal(time, File.GetLastWriteTimeUtc(path)); // Reopen preserves actual partial/full evidence without launch/reset/new nonce.
            AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); // None of these fixtures calls an OS process or pedagogical model.
        } // Finish this bounded owner check without renewing authority or an attempt.
    } // Finish this bounded owner check without renewing authority or an attempt.

    [Fact] // Discover this bounded regression through the existing xUnit project.
    public void WireNativeStarted_ClosedPhysicalDataCannotIssueLaunchBinding() // Verify the actual WireNativeStarted_ClosedPhysicalDataCannotIssueLaunchBinding boundary.
    { // Verify the actual WireNativeStarted_ClosedPhysicalDataCannotIssueLaunchBinding boundary.
        var launch = Encoding.UTF8.GetBytes("{\"fixture\":\"data-only\"}"); var value = new CodeDocumentationNativeCodexSelectionWire.NativeStartedV1(new string('a', 64), new string('b', 32), new string('c', 64), 100, // Verify the actual WireNativeStarted_ClosedPhysicalDataCannotIssueLaunchBinding boundary.
            new string('d', 64), 666, new string('e', 64), new string('f', 64), new string('1', 64), CodeDocumentationNativeCodexLaunchInputOwner.SyntheticKind, Convert.ToBase64String(launch), Convert.ToHexString(SHA256.HashData(launch)).ToLowerInvariant()); // Independent fixed literal data has no retained Session/resource owners.
        var expected = Encoding.UTF8.GetBytes("{\"SchemaVersion\":1,\"Domain\":\"code-documentation-native-selection-started-v1\",\"SessionId\":\"" + new string('a', 64) + "\",\"Nonce\":\"" + new string('b', 32) + "\",\"PreparedRequestSha256\":\"" + new string('c', 64) + "\",\"PreparedRequestLength\":100,\"OutputSchemaSha256\":\"" + new string('d', 64) + "\",\"OutputSchemaLength\":666,\"BindingSha256\":\"" + new string('e', 64) + "\",\"InputSha256\":\"" + new string('f', 64) + "\",\"ApprovalEvidenceSha256\":\"" + new string('1', 64) + "\",\"LaunchKind\":\"SyntheticNativeStartedFixture\",\"LaunchInputsCanonicalBase64\":\"e1wiZml4dHVyZVwiOlwiZGF0YS1vbmx5XCJ9\",\"LaunchInputsSha256\":\"placeholder\"}\n"); // Replace only independently computed literal launch fields below.
        var text = Encoding.UTF8.GetString(expected).Replace("e1wiZml4dHVyZVwiOlwiZGF0YS1vbmx5XCJ9", Convert.ToBase64String(launch), StringComparison.Ordinal).Replace("placeholder", value.LaunchInputsSha256, StringComparison.Ordinal); expected = Encoding.UTF8.GetBytes(text); // Literal field order/schema is independent of the product writer.
        Assert.Equal(expected, CodeDocumentationNativeCodexSelectionWire.WriteStarted(value)); Assert.True(CodeDocumentationNativeCodexSelectionWire.DecodeStarted(expected).IsSuccess); // Canonical data decoding succeeds as data only.
        foreach (var invalid in new[] { Encoding.UTF8.GetBytes(text.Replace("\"SchemaVersion\":1", "\"SchemaVersion\":1,\"SchemaVersion\":1", StringComparison.Ordinal)), Encoding.UTF8.GetBytes(text.Replace("\"SchemaVersion\":1", "\"SchemaVersion\":1.0", StringComparison.Ordinal)), Encoding.UTF8.GetBytes(text.Replace("\"Nonce\":", "\"nonce\":", StringComparison.Ordinal)), Encoding.UTF8.GetBytes(text.Replace("SyntheticNativeStartedFixture", "NativeInstalledEnforcement", StringComparison.Ordinal)), Encoding.UTF8.GetBytes(text.Replace("\"OutputSchemaLength\":666", "\"OutputSchemaLength\":null", StringComparison.Ordinal)), expected[..^1], expected.Append((byte)' ').ToArray(), new byte[] { 0xff, 10 } }) // Verify the actual WireNativeStarted_ClosedPhysicalDataCannotIssueLaunchBinding boundary.
        { var result = CodeDocumentationNativeCodexSelectionWire.DecodeStarted(invalid); Assert.False(result.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, result.ErrorCode); } // Reject duplicate/type/case/kind/partial/UTF8 aliases before any Session capability exists.
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode, CodeDocumentationNativeCodexSelectionWire.DecodeStarted(new byte[8193]).ErrorCode); // Physical preparse overage is independent of syntax.
        Assert.All(typeof(CodeDocumentationOneLessonSession.NativeSelectionLaunchBindingV1).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), c => Assert.True(c.IsPrivate)); // There is no public/internal data import constructor.
        Assert.Equal(8192, CodeDocumentationNativeCodexSelectionWire.StartedMaximumBytes); Assert.Equal(57364, CodeDocumentationNativeCodexSelectionWire.FamilyMaximumBytes); Assert.Equal(229376, CodeDocumentationNativeCodexSelectionWire.SessionMaximumBytes); // Preserve all existing storage ceilings.
    }

    [Fact]
    public async Task NativePrepared_FullProtectedPacketAndSchemaReachFixedOwner()
    {
        using var p = new ProtectedDisclosureHarness(nativePreparation: true); var h = p.H; var b = (await h.Consume()).Value!; Assert.NotNull(b);
        var old = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext? actual = null;
        CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt? receipt = null;
        var finals = new List<string>(); var calls = h.Client.Calls;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = c =>
            {
                actual = c;
                var approval = h.Startup.ResolveProtectedTeachingDisclosureApproval(h.Grant).Value!;
                var anticipated = IndependentlyExpectedNativeRequest(p, c, new string('1', 32));
                Assert.InRange(c.CopyCanonicalBytes().Length, 1, 16384); Assert.InRange(anticipated.Length, 1, 25731);
                output.WriteLine("NATIVE_PREPARED_COMPLETE_LENGTHS input={0}; packet={1}; request={2}; schema={3}; family={4}",
                    c.MetadataInput.CopyBytes().Length, c.CopyCanonicalBytes().Length, anticipated.Length,
                    CodeDocumentationNativeCodexSelectionWire.CopySchemaBytes().Length, anticipated.Length + CodeDocumentationNativeCodexSelectionWire.CopySchemaBytes().Length);
                var anticipatedData = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(anticipated);
                output.WriteLine("NATIVE_PREPARED_ANTICIPATED_DECODE success={0}; code={1}; beforeSessionStore=true", anticipatedData.IsSuccess, anticipatedData.ErrorCode);
                Assert.True(anticipatedData.IsSuccess, "Independent current-owner V2 decode before Session Store: " + anticipatedData.ErrorCode);
                Assert.Equal(c.CopyBindingBytes(), anticipatedData.Value!.CopyBindingBytes());
                Assert.Equal(c.MetadataInput.CopyBytes(), anticipatedData.Value.CopyInputBytes());
                Assert.Equal(c.CopyCanonicalBytes(), anticipatedData.Value.CopyPacketBytes());
                Assert.True(anticipatedData.Value.MatchesApprovalBytes(approval.CopyBytes()));
                Assert.True(anticipatedData.Value.MatchesPromptBytes(IndependentlyExpectedNativePrompt(c, new string('1', 32))));
                var projectionStage = "ProjectV2";
                try
                {
                    var projected = CodeDocumentationNativeCodexSelectionWire.ProjectV2(h.Open.Session.Snapshot.SessionId,
                        c.CopyBindingBytes(), c.MetadataInput.CopyBytes(), approval.CopyBytes(), approval.EvidenceSha256, c, new string('1', 32));
                    projectionStage = "WriteV2";
                    var projectedBytes = CodeDocumentationNativeCodexSelectionWire.WriteV2(projected);
                    output.WriteLine("NATIVE_PREPARED_CURRENT_OWNER_PROJECT requestBytes={0}; matchesIndependent={1}; beforeSessionStore=true",
                        projectedBytes.Length, projectedBytes.AsSpan().SequenceEqual(anticipated));
                    Assert.Equal(anticipated, projectedBytes);
                }
                catch (Exception exception)
                {
                    // Fixed stage/type and known owner-comparison labels only; no raw material or arbitrary exception message.
                    var comparison = exception.Message switch {
                        "Complete owner preimages differ." => "complete-owner-preimages",
                        "Complete owner prompt differs." => "complete-owner-prompt",
                        _ => "other-data-refusal" };
                    output.WriteLine("NATIVE_PREPARED_CURRENT_OWNER_PROJECT_FAILURE stage={0}; type={1}; comparison={2}; beforeSessionStore=true",
                        projectionStage, exception.GetType().Name, comparison);
                    foreach (var frame in new System.Diagnostics.StackTrace(exception, true).GetFrames().Take(4))
                        output.WriteLine("NATIVE_PREPARED_PROJECT_FRAME file={0}; line={1}; method={2}",
                            Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber(), frame.GetMethod()?.Name);
                    throw;
                }
            };
            h.Open.Session.SelectionMaterialPointForTesting = point =>
            {
                if (DisclosureFinalPoints.Contains(point) || point == "native-prepared-family-final-none") finals.Add(point);
                if (point == "native-preparation-readback-staged")
                {
                    receipt = (CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt)FreshSessionField(h.Open.Session, "_nativePreparationReceipt")!;
                    Assert.True(receipt.IsActiveFor(h.Open.Session));
                    Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, p.PrepareNative(b).GetAwaiter().GetResult().ErrorCode);
                }
            };
            var result = await p.PrepareNative(b); Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage); Assert.True(result.Value!.Prepared); Assert.False(result.Value.Replayed); Assert.Equal(3, result.Value.CandidateCount);
            var saved = NativePreparationBytes(h.Open.Session); using var data = JsonDocument.Parse(saved.Request); var request = data.RootElement;
            Assert.NotNull(actual); Assert.Equal(3, actual!.Candidates.Count);
            var decoded = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(saved.Request); Assert.True(decoded.IsSuccess, decoded.ErrorCode); Assert.Equal(2, decoded.Value!.SchemaVersion);
            Assert.Equal(IndependentlyExpectedNativeRequest(p, actual, decoded.Value.Nonce), saved.Request);
            Assert.Equal(actual.CopyCanonicalBytes(), decoded.Value.CopyPacketBytes()); Assert.Equal(actual.CopyBindingBytes(), decoded.Value.CopyBindingBytes()); Assert.Equal(actual.MetadataInput.CopyBytes(), decoded.Value.CopyInputBytes());
            var expectedCandidates = NativeCandidatesOracle(actual);
            Assert.Equal(actual.CopyCanonicalBytes(), NativePacketOracle(JsonNode.Parse(actual.CopyBindingBytes())!, JsonNode.Parse(actual.MetadataInput.CopyBytes())!, expectedCandidates));
            var expectedPrompt = IndependentlyExpectedNativePrompt(actual, decoded.Value.Nonce);
            Assert.True(decoded.Value.MatchesPromptBytes(expectedPrompt)); Assert.Equal(expectedPrompt, Encoding.UTF8.GetBytes(CodeDocumentationNativeCodexSelectionHost.RenderCompletePrompt(actual, decoded.Value.Nonce)));
            using var graph = JsonDocument.Parse(decoded.Value.CopyInputBytes());
            Assert.Equal(2, graph.RootElement.GetProperty("TeachingGraph").GetProperty("Sources").GetArrayLength());
            Assert.Equal(4, graph.RootElement.GetProperty("TeachingGraph").GetProperty("Items").GetArrayLength());
            Assert.Equal(3, graph.RootElement.GetProperty("TeachingGraph").GetProperty("Sources")[0].GetProperty("Entries").GetArrayLength());
            Assert.Equal(1, graph.RootElement.GetProperty("TeachingGraph").GetProperty("Sources")[1].GetProperty("Entries").GetArrayLength());
            foreach (var candidate in actual.Candidates)
            {
                var descriptor = h.Grant.SourceSelection!.TeachingCurriculum!.Sources.Single(s => s.SourceId == candidate.SourceId);
                var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, descriptor.DomainSlug, descriptor.SubjectSlug, descriptor.CorpusVersion).Value!;
                var entry = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address).Value!.Corpus.Entries.Single(e => e.EntryId == candidate.ReferenceEntryId);
                Assert.Equal(entry.OwnerInput, candidate.OwnerInput); Assert.Equal(entry.PaidModelReferenceAnswer, candidate.PaidModelReferenceAnswer);
                Assert.Equal(entry.PaidReferenceProvenance.CanonicalRepresentation, candidate.PaidReferenceProvenance.CanonicalRepresentation);
                Assert.EndsWith("\tEND", candidate.OwnerInput); Assert.EndsWith("\tEND", candidate.PaidModelReferenceAnswer);
            }
            Assert.True(decoded.Value.MatchesApprovalBytes(h.Startup.ResolveProtectedTeachingDisclosureApproval(h.Grant).Value!.CopyBytes()));
            Assert.Equal("gpt-6-astra", request.GetProperty("RequestedModel").GetString()); Assert.Equal("ultra", request.GetProperty("RequestedReasoningEffort").GetString());
            Assert.Equal("ba967b55007606dcfed2dca4e8c5f856c2134170a020706ca8ff9b6d6f35062a", request.GetProperty("PolicyVersion").GetString());
            Assert.Contains("do not silently truncate the catalog", request.GetProperty("PolicyContentUtf8").GetString());
            Assert.Contains("For otherwise equivalent eligible choices", request.GetProperty("PolicyContentUtf8").GetString());
            Assert.Equal(saved.Schema, Convert.FromBase64String(request.GetProperty("OutputSchemaCanonicalBase64").GetString()!));
            using var schema = JsonDocument.Parse(saved.Schema); Assert.False(schema.RootElement.GetProperty("additionalProperties").GetBoolean());
            Assert.Equal(new[] { "Nonce", "PreparedRequestSha256", "ItemId", "SourceId", "ReferenceEntryId", "SourceEntityFingerprint", "MaterialAdmissionEvidenceSha256", "Reason" }, schema.RootElement.GetProperty("required").EnumerateArray().Select(x => x.GetString()));
            foreach (var point in DisclosureFinalPoints.Append("native-prepared-family-final-none")) Assert.Equal(2, finals.Count(x => x == point));
            Assert.NotNull(receipt); Assert.False(receipt!.IsActiveFor(h.Open.Session)); AssertNativePreparationReleased(h.Open.Session);
            Assert.Equal(calls, h.Client.Calls); AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); p.Environment.AssertStillConfigured();
        }
        finally { h.Open.Session.SelectionMaterialPointForTesting = null; CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = old; }
    }

    [Fact]
    public async Task NativePrepared_SameNonceReopenReconstructsFreshOwners()
    {
        using var p = new ProtectedDisclosureHarness(nativePreparation: true); var h = p.H; var b = (await h.Consume()).Value!;
        var originalCalls = h.Client.Calls; Assert.Equal(10, originalCalls); // The original harness owns both saved-baseline evaluation series.
        var first = await p.PrepareNative(b); Assert.True(first.IsSuccess, first.ErrorCode); var bytes = NativePreparationBytes(h.Open.Session); Assert.Equal(originalCalls, h.Client.Calls); // Preparation adds no Generate call to that exact original client.
        var old = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting; var captures = 0; var raw = 0;
        var oldRead = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++;
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = _ => raw++;
            var second = await p.PrepareNative(b); Assert.True(second.IsSuccess, second.ErrorCode); Assert.True(second.Value!.Replayed);
            var repeated = NativePreparationBytes(h.Open.Session); Assert.Equal(bytes.Request, repeated.Request); Assert.Equal(bytes.Schema, repeated.Schema);
            Assert.Equal(originalCalls, h.Client.Calls); AssertFullMaterialNoEffects(h); // Replaying in the original owner preserves its strict ten-call baseline and zero effects.
            h.Open.Session.Dispose(); Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); // Release custody before reconstructing independently owned startup and evaluator objects.
            using var services = ModelServer.Program.CreateLocalLoopbackBuilder().Services.BuildServiceProvider();
            var freshStartup = services.GetRequiredService<ContinuousLearningModelServerStartup>(); Assert.NotSame(p.Startup, freshStartup);
            using var fresh = new OrdinaryDiagnosticHarness((_, _) => (false, "missing_essential_behavior"),
                existingFixture: h.Fixture, startupFactory: _ => freshStartup);
            var freshCallsBeforeConsume = fresh.Client.Calls; Assert.Equal(0, freshCallsBeforeConsume); // A new client has not generated the baseline saved by the original harness.
            b = (await fresh.Consume()).Value!; Assert.NotNull(b); Assert.Equal(freshCallsBeforeConsume, fresh.Client.Calls); // Reattachment consumes existing baseline evidence without another evaluation series.
            var freshCallsBeforeReplay = fresh.Client.Calls; // Capture the exact fresh client's counter immediately before native preparation replay.
            var replay = await fresh.Open.Session.CaptureStartupApprovedNativeSelectionAsync(b, freshStartup, h.Fixture.OrdinarySelector); // Reconstruct from current owners while retaining the persisted nonce and bytes.
            Assert.True(replay.IsSuccess, replay.ErrorCode); Assert.True(replay.Value!.Replayed);
            var after = NativePreparationBytes(fresh.Open.Session); Assert.Equal(bytes.Request, after.Request); Assert.Equal(bytes.Schema, after.Schema);
            Assert.Equal(first.Value!.RequestSha256, replay.Value.RequestSha256); Assert.Equal(first.Value.RequestLength, replay.Value.RequestLength);
            Assert.Equal(2, captures); Assert.True(raw > 0); AssertNativePreparationReleased(fresh.Open.Session); // Both replay attempts reconstruct real materials and release their preparation authority.
            Assert.Equal(freshCallsBeforeReplay, fresh.Client.Calls); Assert.Equal(0, fresh.Client.Calls); // The fresh owner neither inherits the original counter nor issues an additional Generate call.
            Assert.Equal(originalCalls, h.Client.Calls); Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), fresh.Open.Session.Snapshot.Effects); // Verify both independent clients and fresh Session effects without weakening the global ten-call helper.
        }
        finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = old; CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = oldRead; }
    }
    private static void WriteNativeHeld(FileStream stream, byte[] bytes)
    { stream.Position = 0; stream.SetLength(0); stream.Write(bytes); stream.Flush(true); }
    private static byte[] ReadNativePath(string path)
    { using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); using var bytes = new MemoryStream(); stream.CopyTo(bytes); return bytes.ToArray(); }

    private static CodeDocumentationOneLessonTeachingCurriculumV2 NativeCapacityCurriculum(
        CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture fixture, string task)
        => fixture.SealGenuineNativePreparation(task);

    private static string NativeCapacityTeachingTask(int cases)
        => "中文\nT7/0\tEND" + string.Concat(Enumerable.Range(0, cases).Select(i => $"\nCase {i:D2}: preserve complete owned readback."));

    private static byte[] IndependentlyExpectedNativeRequest(ProtectedDisclosureHarness p,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, string nonce)
        => IndependentlyExpectedNativeRequest(p.H.Open.Session.Snapshot.SessionId, context,
            p.Startup.ResolveProtectedTeachingDisclosureApproval(p.H.Grant).Value!.CopyBytes(), nonce);

    internal static byte[] IndependentlyExpectedNativeRequest(string sessionId,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, byte[] approval, string nonce)
        => NativeV2Oracle(sessionId, JsonNode.Parse(context.CopyBindingBytes())!, JsonNode.Parse(context.MetadataInput.CopyBytes())!,
            JsonNode.Parse(approval)!, NativeCandidatesOracle(context), nonce);
    internal static byte[] IndependentlyExpectedNativePrompt(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context, string nonce)
        => NativePromptOracle(JsonNode.Parse(context.MetadataInput.CopyBytes())!, NativeCandidatesOracle(context), nonce);

    // Independent test contract: literal shape/order, its own first-use interning and complete preimages.
    private static (string Name,string Kind)[] NativeShape(string type) => type switch
    {
        "Binding" => [("ObjectiveId","s"), ("GrantId","s"), ("GrantVersion","s"), ("TrainingRunId","s"), ("CandidateSha256","s"), ("ParentManifestSha256","s"), ("ParentWeightsSha256","s"), ("ConfigurationSha256","s"), ("ExamConditionSha256","s"), ("DescriptorsV2","Descriptors")],
        "Descriptors" => [("SchemaVersion","n"), ("SourceSelectionSha256","s"), ("ParentCheckpointSha256","s"), ("CandidateSelectionSha256","s"), ("CorpusDescriptorSha256","s"), ("EvaluationPlanSha256","s"), ("CaseCount","n"), ("RepeatCount","n"), ("EvaluatorProfileSha256","s"), ("ProfileSelectionSha256","s")],
        "Input" => [("Schema","s"), ("OriginalReceiptId","s"), ("OriginalReceiptSha256","s"), ("SourceSha256","s"), ("Categories","[s]"), ("TeachingGraph","Graph")],
        "Graph" => [("Selection","Selection"), ("Sources","[GraphSource]"), ("Items","[ResolvedItem]"), ("CategoryBindings","[Category]")],
        "Selection" => [("Sources","[TrainingSource]"), ("PairSelections","[Pair]"), ("TeachingCurriculum","Curriculum")],
        "TrainingSource" => [("SourceId","s"), ("Role","s"), ("DomainSlug","s"), ("DesignPatternSlug","s"), ("DatasetVersion","s"), ("ManifestFileSha256","s"), ("ManifestFileByteLength","n"), ("SealedManifestSha256","s")],
        "Pair" => [("TrainingPairId","s"), ("SourceId","s")],
        "Curriculum" => [("PolicyId","s"), ("PolicyVersion","s"), ("Sources","[Descriptor]"), ("Items","[Item]"), ("CategoryBindings","[Category]")],
        "Descriptor" => [("SourceId","s"), ("CorpusId","s"), ("DomainSlug","s"), ("SubjectSlug","s"), ("CorpusVersion","s"), ("ManifestFileSha256","s"), ("ManifestFileByteLength","n"), ("CorpusSha256","s"), ("PayloadSha256","s"), ("PayloadByteLength","n")],
        "Item" => [("ItemId","s"), ("SourceId","s"), ("ReferenceEntryId","s"), ("SourceEntityFingerprint","s")],
        "Category" => [("Category","s"), ("OrderedItemIds","[s]")],
        "GraphSource" => [("Descriptor","Descriptor"), ("Manifest","Manifest"), ("ManifestFileSha256","s"), ("ManifestFileByteLength","n"), ("Entries","[Entry]")],
        "Manifest" => [("Schema","s"), ("Context","s"), ("Domain","s"), ("Subject","s"), ("CorpusVersion","s"), ("CorpusName","s"), ("FrozenAtUtc","s"), ("EntryCount","n"), ("CorpusSha256","s"), ("RepositoryRelativeCorpusDirectory","s"), ("RepositoryRelativePayloadPath","s"), ("PayloadByteLength","n"), ("PayloadSha256","s"), ("RepositoryRelativeManifestPath","s"), ("ManifestSha256","s")],
        "Entry" => [("EntryId","s"), ("SourceEntityFingerprint","s"), ("DomainTag","s"), ("SubjectId","s"), ("Partition","s"), ("OwnerInputSha256","s"), ("PaidReferenceProvenance","Provenance")],
        "Provenance" => [("ProviderRef","s"), ("ModelId","s"), ("ModelVersion","s"), ("RequestId","s"), ("CapturedAtUtc","s"), ("ReferenceAnswerSha256","s"), ("CanonicalRepresentation","s"), ("ProvenanceSha256","s")],
        "ResolvedItem" => [("DeclaredItem","Item"), ("SourceId","s"), ("ReferenceEntryId","s"), ("SourceEntityFingerprint","s")],
        "Approval" => [("SchemaVersion","n"), ("ApprovalId","s"), ("ApprovalRevision","n"), ("GrantId","s"), ("GrantVersion","s"), ("ApproverId","s"), ("ApprovedAtUtc","s"), ("OperatorDecision","s"), ("Use","s"), ("Recipient","s"), ("Model","s"), ("ReasoningEffort","s"), ("Evidence","[Evidence]"), ("SourceSelection","Selection"), ("AllTeachingSources","[ApprovedSource]")],
        "Evidence" => [("EvidenceRef","s"), ("EvidenceSha256","s")],
        "ApprovedSource" => [("Descriptor","Descriptor"), ("Entries","[ApprovedEntry]")],
        "ApprovedEntry" => [("EntryId","s"), ("SourceEntityFingerprint","s"), ("OwnerInputSha256","s"), ("ReferenceAnswerSha256","s"), ("PaidReferenceProvenanceSha256","s"), ("Eligibility","Eligibility")],
        "Eligibility" => [("LicenseAccepted","b"), ("IsQuarantined","b"), ("EvidenceRefs","[s]")],
        "Candidate" => [("ItemId","s"), ("SourceId","s"), ("ReferenceEntryId","s"), ("SourceEntityFingerprint","s"), ("OwnerInput","s"), ("PaidModelReferenceAnswer","s"), ("PaidReferenceProvenance","Provenance"), ("MaterialAdmissionEvidenceSha256","s")],
        _ => throw new InvalidOperationException("Unknown test tuple")
    };
    private static readonly JsonSerializerOptions NativeJsonOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    private static byte[] NativeCanonical(JsonNode value)
    {
        var result=new StringBuilder();
        void Quote(string text){result.Append('"');foreach(var c in text){result.Append(c switch{'"'=>"\\\"",'\\'=>"\\\\",'\b'=>"\\b",'\t'=>"\\t",'\n'=>"\\n",'\f'=>"\\f",'\r'=>"\\r",< ' ' =>"\\u"+((int)c).ToString("x4"),_=>c.ToString()});}result.Append('"');}
        void Write(JsonNode node){if(node is JsonObject obj){result.Append('{');var first=true;foreach(var p in obj.OrderBy(x=>x.Key,StringComparer.Ordinal)){if(!first)result.Append(',');first=false;Quote(p.Key);result.Append(':');Write(p.Value!);}result.Append('}');}
            else if(node is JsonArray arr){result.Append('[');for(int i=0;i<arr.Count;i++){if(i>0)result.Append(',');Write(arr[i]!);}result.Append(']');}
            else if(node.GetValueKind()==JsonValueKind.String)Quote(node.GetValue<string>());else result.Append(node.ToJsonString());}
        Write(value);return new UTF8Encoding(false,true).GetBytes(result.ToString());
    }
    private static JsonNode NativeTuple(JsonNode value,string type,JsonArray atoms,Dictionary<string,int> indexes)
    {
        if(type=="s"){var text=value.GetValue<string>();if(!indexes.TryGetValue(text,out var index)){index=atoms.Count;indexes.Add(text,index);atoms.Add(text);}return JsonValue.Create(index)!;}
        if(type is "n" or "b")return value.DeepClone();
        if(type.StartsWith('[')){var a=new JsonArray();foreach(var x in value.AsArray())a.Add(NativeTuple(x!,type[1..^1],atoms,indexes));return a;}
        var tuple=new JsonArray();foreach(var field in NativeShape(type))tuple.Add(NativeTuple(value[field.Name]!,field.Kind,atoms,indexes));return tuple;
    }
    private static JsonNode NativeExpand(JsonNode value,string type,JsonArray atoms)
    {
        if(type=="s")return atoms[value.GetValue<int>()]!.DeepClone();if(type is "n" or "b")return value.DeepClone();
        if(type.StartsWith('[')){var a=new JsonArray();foreach(var x in value.AsArray())a.Add(NativeExpand(x!,type[1..^1],atoms));return a;}
        var obj=new JsonObject();var fields=NativeShape(type);for(var i=0;i<fields.Length;i++)obj.Add(fields[i].Name,NativeExpand(value[i]!,fields[i].Kind,atoms));return obj;
    }
    private static byte[] NativePacketOracle(JsonNode binding,JsonNode input,JsonNode candidates, bool plainBindingHash = false)
    {
        using var m=new MemoryStream();using var w=new BinaryWriter(m,new UTF8Encoding(false,true),true);
        w.Write(Encoding.UTF8.GetBytes("code-documentation-selection-material-input-v1\n"));void Bytes(byte[] b){w.Write(b.Length);w.Write(b);}void Field(string t)=>Bytes(Encoding.UTF8.GetBytes(t));
        Field("Judge task-specific pedagogical suitability first. Use the supplied curriculum order only to break otherwise equivalent eligible choices.");
        var bb=NativeCanonical(binding);
        Field(HBytes(plainBindingHash ? bb : Encoding.UTF8.GetBytes("code-documentation-one-lesson-binding-v2\n").Concat(bb).ToArray()));
        Bytes(bb);Bytes(NativeCanonical(input));w.Write(input["Categories"]!.AsArray().Count);
        foreach(var category in input["Categories"]!.AsArray())Field(category!.GetValue<string>());w.Write(candidates.AsArray().Count);
        foreach(var candidate in candidates.AsArray()){foreach(var name in new[]{"ItemId","SourceId","ReferenceEntryId","SourceEntityFingerprint","OwnerInput","PaidModelReferenceAnswer"})Field(candidate![name]!.GetValue<string>());
            Field(candidate!["PaidReferenceProvenance"]!["CanonicalRepresentation"]!.GetValue<string>());Field(candidate["MaterialAdmissionEvidenceSha256"]!.GetValue<string>());}w.Flush();return m.ToArray();
    }
    private static byte[] NativePromptOracle(JsonNode input,JsonNode candidates,string nonce)
    {
        var array=new JsonArray();foreach(var node in candidates.AsArray()){var c=node!;var p=c["PaidReferenceProvenance"]!;var provenance=new JsonObject();
            foreach(var name in new[]{"ProviderRef","ModelId","ModelVersion","RequestId"})provenance.Add(name,p[name]!.DeepClone());
            provenance.Add("CapturedAtUtc",JsonSerializer.SerializeToNode(DateTimeOffset.Parse(p["CapturedAtUtc"]!.GetValue<string>()),NativeJsonOptions));
            provenance.Add("ReferenceAnswerSha256",p["ReferenceAnswerSha256"]!.DeepClone());provenance.Add("ClosedPaidReference",true);provenance.Add("PaidReferenceOutputNeverTrainable",true);provenance.Add("PaidReferenceOutputNeverDpoPairSide",true);
            provenance.Add("CanonicalRepresentation",p["CanonicalRepresentation"]!.DeepClone());provenance.Add("ProvenanceSha256",p["ProvenanceSha256"]!.DeepClone());
            var candidate=new JsonObject();foreach(var name in new[]{"ItemId","SourceId","ReferenceEntryId","SourceEntityFingerprint","OwnerInput","PaidModelReferenceAnswer"})candidate.Add(name,c[name]!.DeepClone());
            candidate.Add("PaidReferenceProvenance",provenance);candidate.Add("MaterialAdmissionEvidenceSha256",c["MaterialAdmissionEvidenceSha256"]!.DeepClone());array.Add(candidate);}
        return Encoding.UTF8.GetBytes(CodeDocumentationNativeCodexSelectionHost.PolicyContent+"\n\nSaved sanitized weakness categories:\n"+input["Categories"]!.ToJsonString(NativeJsonOptions)+
            "\nAll eligible teaching candidates in declared order:\n"+array.ToJsonString(NativeJsonOptions)+"\nReturn one future selection tuple and reason under the fixed output schema. Prepared nonce: "+nonce);
    }
    private static byte[] NativeV2Oracle(string sessionId,JsonNode binding,JsonNode input,JsonNode approval,JsonNode candidates,string nonce)
    {
        var atoms=new JsonArray();var indexes=new Dictionary<string,int>(StringComparer.Ordinal);
        var b=NativeTuple(binding,"Binding",atoms,indexes);var i=NativeTuple(input,"Input",atoms,indexes);var a=NativeTuple(approval,"Approval",atoms,indexes);var c=NativeTuple(candidates,"[Candidate]",atoms,indexes);
        var prompt=NativePromptOracle(input,candidates,nonce);
        var root=new JsonObject { ["SchemaVersion"]=2,["Domain"]="code-documentation-native-selection-prepared-v2",["SessionId"]=sessionId,["Atoms"]=atoms,["Binding"]=b,["Input"]=i,["Approval"]=a,["Candidates"]=c,
            ["ApprovalEvidenceSha256"]=HBytes(NativeCanonical(approval)),["PolicyId"]="S04-CODEX-TEACHING-SELECTION-v1",["PolicyVersion"]="ba967b55007606dcfed2dca4e8c5f856c2134170a020706ca8ff9b6d6f35062a",
            ["PolicyContentUtf8"]=CodeDocumentationNativeCodexSelectionHost.PolicyContent,["Nonce"]=nonce,["PromptRecipeVersion"]=1,["RenderedPromptUtf8ByteLength"]=prompt.Length,["RenderedPromptSha256"]=HBytes(prompt),
            ["RequestedModel"]="gpt-6-astra",["RequestedReasoningEffort"]="ultra",["OutputSchemaCanonicalBase64"]=Convert.ToBase64String(CodeDocumentationNativeCodexSelectionWire.CopySchemaBytes())};
        return Encoding.UTF8.GetBytes(root.ToJsonString(NativeJsonOptions));
    }
    private static JsonNode NativeCandidatesOracle(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext context)
    {
        var input=JsonNode.Parse(context.MetadataInput.CopyBytes())!;var candidates=new JsonArray();foreach(var c in context.Candidates){var source=input["TeachingGraph"]!["Sources"]!.AsArray().Single(x=>x!["Descriptor"]!["SourceId"]!.GetValue<string>()==c.SourceId)!;
            var entry=source["Entries"]!.AsArray().Single(x=>x!["EntryId"]!.GetValue<string>()==c.ReferenceEntryId)!;
            candidates.Add(new JsonObject{["ItemId"]=c.ItemId,["SourceId"]=c.SourceId,["ReferenceEntryId"]=c.ReferenceEntryId,["SourceEntityFingerprint"]=c.SourceEntityFingerprint,
                ["OwnerInput"]=c.OwnerInput,["PaidModelReferenceAnswer"]=c.PaidModelReferenceAnswer,["PaidReferenceProvenance"]=entry["PaidReferenceProvenance"]!.DeepClone(),["MaterialAdmissionEvidenceSha256"]=c.MaterialAdmissionEvidenceSha256});}return candidates;
    }
    private static byte[] NativeMutatedBindingOracle(byte[] saved,bool foreignBinding)
    {
        var root=JsonNode.Parse(saved)!;var atoms=root["Atoms"]!.AsArray();var b=NativeExpand(root["Binding"]!,"Binding",atoms);var i=NativeExpand(root["Input"]!,"Input",atoms);var a=NativeExpand(root["Approval"]!,"Approval",atoms);var c=NativeExpand(root["Candidates"]!,"[Candidate]",atoms);
        if(foreignBinding){b["ParentWeightsSha256"]=new string('e',64);foreach(var candidate in c.AsArray()){
            var entry=i["TeachingGraph"]!["Sources"]!.AsArray().Single(s=>s!["Descriptor"]!["SourceId"]!.GetValue<string>()==candidate!["SourceId"]!.GetValue<string>())!["Entries"]!.AsArray().Single(e=>e!["EntryId"]!.GetValue<string>()==candidate!["ReferenceEntryId"]!.GetValue<string>())!;
            using var m=new MemoryStream();using var w=new BinaryWriter(m,Encoding.UTF8,true);w.Write(Encoding.UTF8.GetBytes("code-documentation-startup-teaching-disclosure-v1\n"));void Bytes(byte[] bytes){w.Write(bytes.Length);w.Write(bytes);}void Field(string text)=>Bytes(Encoding.UTF8.GetBytes(text));
            Bytes(NativeCanonical(a));Bytes(NativeCanonical(b));Bytes(NativeCanonical(i));foreach(var name in new[]{"ItemId","SourceId","ReferenceEntryId","SourceEntityFingerprint"})Field(candidate![name]!.GetValue<string>());Field(entry["OwnerInputSha256"]!.GetValue<string>());Field(candidate!["PaidReferenceProvenance"]!["CanonicalRepresentation"]!.GetValue<string>());w.Flush();candidate["MaterialAdmissionEvidenceSha256"]=HBytes(m.ToArray());}}
        return NativeV2Oracle(foreignBinding?root["SessionId"]!.GetValue<string>():new string('e',64),b,i,a,c,root["Nonce"]!.GetValue<string>());
    }

    [Fact]
    public async Task NativePrepared_PartialForeignAndChangedInputsPreserveEvidence()
    {
        foreach (var fault in new[] { "request", "schema", "foreign", "foreign-binding", "receiving-parent", "request-reparse", "schema-reparse", "request-only", "schema-only", "empty-pair", "unknown", "fresh-lock", "unused-source", "revision" })
        {
            using var p = new ProtectedDisclosureHarness(nativePreparation: true); var h = p.H; var b = (await h.Consume()).Value!;
            var control = await p.PrepareNative(b); Assert.True(control.IsSuccess, control.ErrorCode); // Every negative case requires an actual positive prepare.
            var saved = NativePreparationBytes(h.Open.Session); var session = h.Open.Session;
            var requestPath = Path.Combine(Path.GetDirectoryName(h.Fixture.OrdinaryHistory)!, CodeDocumentationNativeCodexSelectionWire.RequestFileName);
            var schemaPath = Path.Combine(Path.GetDirectoryName(h.Fixture.OrdinaryHistory)!, CodeDocumentationNativeCodexSelectionWire.SchemaFileName);
            var lockPath = (string)FreshSessionField(session, "_lockPath")!; // Keep the exact Session owner-lock path for explicit fixture restoration.
            var requestTime = File.GetLastWriteTimeUtc(requestPath); var schemaTime = File.GetLastWriteTimeUtc(schemaPath); var lockTime = File.GetLastWriteTimeUtc(lockPath); // Restore original timestamps together with complete saved family bytes and custody.
            var receivingBinding = session.Snapshot.Binding;
            var lockBytes = new byte[((FileStream)FreshSessionField(session, "_ownerLock")!).Length];
            var heldLock = (FileStream)FreshSessionField(session, "_ownerLock")!;
            heldLock.Position = 0; heldLock.ReadExactly(lockBytes); // Read through the genuine exclusive owner, never a second ordinary lock open.
            var retainedPath = Path.Combine(h.Fixture.Root, "native-reparse-" + fault);
            string? reparsePath = null;
            var source = h.Grant.SourceSelection!.TeachingCurriculum!.Sources[1];
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, source.DomainSlug, source.SubjectSlug, source.CorpusVersion).Value!;
            var sourcePath = Path.Combine(address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
            var sourceBytes = File.ReadAllBytes(sourcePath); var sourceTime = File.GetLastWriteTimeUtc(sourcePath); Exception? primary = null;
            var closed = fault is not "unused-source" and not "revision"; var extra = Path.Combine(Path.GetDirectoryName(requestPath)!, "unknown-native-leaf");
            try
            {
                if (closed) session.Dispose();
                switch (fault)
                {
                    case "request": File.WriteAllBytes(requestPath, saved.Request.Append((byte)' ').ToArray()); break;
                    case "schema": File.WriteAllBytes(schemaPath, saved.Schema.Append((byte)' ').ToArray()); break;
                    case "foreign":
                        var foreignSession = NativeMutatedBindingOracle(saved.Request, false);
                        Assert.True(CodeDocumentationNativeCodexSelectionWire.DecodePrepared(foreignSession).IsSuccess);
                        File.WriteAllBytes(requestPath, foreignSession); break;
                    case "foreign-binding":
                        var foreignRequest = NativeMutatedBindingOracle(saved.Request, true);
                        var foreign = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(foreignRequest); Assert.True(foreign.IsSuccess, foreign.ErrorCode);
                        var originalData = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(saved.Request).Value!;
                        Assert.Equal(originalData.SessionId, foreign.Value!.SessionId); Assert.NotEqual(originalData.CopyBindingBytes(), foreign.Value.CopyBindingBytes());
                        File.WriteAllBytes(requestPath, foreignRequest); break;
                    case "request-reparse": case "schema-reparse":
                        reparsePath = fault == "request-reparse" ? requestPath : schemaPath;
                        File.Move(reparsePath, retainedPath);
                        try { File.CreateSymbolicLink(reparsePath, retainedPath); }
                        catch (Exception ex) { output.WriteLine("NATIVE_REPARSE_CREATE_FAILED path={0}; type={1}; hresult=0x{2:X8}; error={3}", reparsePath, ex.GetType().Name, ex.HResult, ex.Message); throw; }
                        Assert.True(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(reparsePath));
                        break;
                    case "request-only": File.Delete(schemaPath); break;
                    case "schema-only": File.Delete(requestPath); break;
                    case "empty-pair": File.WriteAllBytes(requestPath, []); File.WriteAllBytes(schemaPath, []); break;
                    case "unknown": using (var leaf = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(extra, FileMode.CreateNew, FileAccess.Write, FileShare.Read, FileOptions.None)) leaf.WriteByte(1); break;
                    case "fresh-lock": File.Delete(lockPath); break;
                    case "unused-source": RestoreReferenceOwnerFile(sourcePath, sourceBytes.Append((byte)' ').ToArray(), sourceTime); break;
                    case "revision": System.Environment.SetEnvironmentVariable(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable, p.Environment.Json.Replace("\"ApprovalRevision\":1", "\"ApprovalRevision\":2", StringComparison.Ordinal)); break;
                }
                var injectedRequest = File.Exists(requestPath) ? ReadNativePath(requestPath) : null; // Snapshot actual injected request evidence, including an absent leaf.
                var injectedSchema = File.Exists(schemaPath) ? ReadNativePath(schemaPath) : null; // Snapshot the separate schema evidence before the production refusal.
                var injectedRequestTime = File.Exists(requestPath) ? File.GetLastWriteTimeUtc(requestPath) : (DateTime?)null; var injectedSchemaTime = File.Exists(schemaPath) ? File.GetLastWriteTimeUtc(schemaPath) : (DateTime?)null; // Preserve injected timestamps independently of saved positive-control timestamps.
                var injectedLock = closed && File.Exists(lockPath) ? ReadNativePath(lockPath) : null; var injectedLockTime = closed && File.Exists(lockPath) ? File.GetLastWriteTimeUtc(lockPath) : (DateTime?)null; // Snapshot closed-owner lock evidence independently; active exclusive owners are never reopened for this assertion.
                var injectedExtra = fault == "unknown" ? ReadNativePath(extra) : null; var injectedExtraTime = fault == "unknown" ? File.GetLastWriteTimeUtc(extra) : (DateTime?)null; // Retain the test-owned unknown leaf as evidence rather than allowing refusal to cleanse it.
                if (closed)
                {
                    // A changed receiving parent is metadata-only opener input, never a substitute grant or a preparation receipt.
                    var refused = fault == "receiving-parent"
                        ? CodeDocumentationOneLessonSession.OpenCapacityValidationV2(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(h.Fixture.RepositoryRoot).Value!,
                            receivingBinding with { ParentManifestSha256 = new string('e', 64) }, h.Catalog.MaxTeacherCalls)
                        : h.Reopen();
                    output.WriteLine("NATIVE_PREPARED_NEGATIVE_REOPEN fault={0}; success={1}; openerError={2}", fault, refused.IsSuccess, refused.ErrorCode); // Record the fixed injected case and safe result code before the strict refusal assertion.
                    Assert.False(refused.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, refused.ErrorCode); // Every injected closed-family fault must remain a recovery refusal.
                    Assert.Null(refused.Value);
                }
                else
                {
                    var previousCapture = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting; var previousPoint = session.SelectionMaterialPointForTesting; // Restore the actual hooks after these active-owner early refusals.
                    var finals = new List<string>(); var captures = 0; var issued = 0; var dispatched = 0; // Count real final attempts and forbid disclosure work on the unsuccessful resolver path.
                    var retainedApproval = p.Startup.OneLessonGrantCatalog.FindTeachingApproval(h.Grant); Assert.NotNull(retainedApproval); // The revision negative must retain exact grant membership independently of current revision success.
                    try // Observe genuine preparation/finals without substituting any result or authority.
                    { // Limit fault observations to this active-owner refusal attempt.
                        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++; // An early refusal must never construct the raw recipient.
                        session.SelectionMaterialPointForTesting = point => // Inspect the check-only lifetime while the independent finals still hold CAS.
                        { // Every real owner final remains observable on both active negative branches.
                            if (DisclosureFinalPoints.Contains(point) || point == "native-prepared-family-final-none") finals.Add(point); // Count the five owners, revision and native family independently.
                            if (point == "teaching-disclosure-receipt-issued") issued++; if (point == "native-preparation-fixed-consumer") dispatched++; // No issuer or fixed consumer may run after failed resolution.
                            if (fault == "revision" && point == "teaching-disclosure-revision-final-none") // Check the retained owner exactly where the old null overwrite occurred.
                            { // Revision checking must keep its genuine input without reviving disclosure authority.
                                Assert.Same(retainedApproval, FreshSessionField(session, "_teachingDisclosureRevisionInput")); Assert.Null(FreshSessionField(session, "_teachingDisclosureApproval")); // Check-only membership and approval authority have separate lifetimes.
                                Assert.Null(FreshSessionField(session, "_teachingDisclosureReceipt")); Assert.Null(FreshSessionField(session, "_selectionMaterialReceipt")); // A stale resolver has issued no disclosure or material capability.
                            } // Finish lifetime evidence before the final result is arbitrated.
                        }; // Retain this observer only during the single refused attempt.
                        var refused = await p.PrepareNative(b); Assert.False(refused.IsSuccess); Assert.Null(refused.Value); // Neither active fault returns a payload or capability.
                        output.WriteLine("NATIVE_PREPARED_ACTIVE_REFUSAL fault={0}; errorCode={1}", fault, refused.ErrorCode); // Fixed fault names and public refusal codes only; no closed material.
                        Assert.Equal(fault == "unused-source" ? RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode : CodeDocumentationOneLessonTeachingDisclosureApproval.Stale, refused.ErrorCode); // Keep the exact original stale contract rather than tolerating Unavailable.
                        foreach (var point in DisclosureFinalPoints.Append("native-prepared-family-final-none")) Assert.Equal(1, finals.Count(actual => actual == point)); // All seven None attempts run even when preparation never reaches issuance.
                        if (fault == "revision") Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Stale, session.TeachingDisclosureRevisionFinalErrorForTesting); // The independent revision final must preserve the actual stale owner reason.
                        Assert.Equal(0, captures); Assert.Equal(0, issued); Assert.Equal(0, dispatched); // Failed resolution cannot open raw material, mint authority or enter the fixed native consumer.
                    } // Shared byte/time retention and restored positive replay remain below this strict branch.
                    finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previousCapture; session.SelectionMaterialPointForTesting = previousPoint; } // Restore hooks even when a lifetime or refusal assertion fails.
                    AssertNativePreparationReleased(session);
                }
                Assert.Equal(injectedRequest, File.Exists(requestPath) ? ReadNativePath(requestPath) : null); // Refusal must preserve the exact request bytes or absence.
                Assert.Equal(injectedSchema, File.Exists(schemaPath) ? ReadNativePath(schemaPath) : null); // Refusal must preserve the exact schema bytes or absence.
                Assert.Equal(injectedRequestTime, File.Exists(requestPath) ? File.GetLastWriteTimeUtc(requestPath) : (DateTime?)null); Assert.Equal(injectedSchemaTime, File.Exists(schemaPath) ? File.GetLastWriteTimeUtc(schemaPath) : (DateTime?)null); // Product refusal must not rewrite or retimestamp either injected family member.
                if (closed && fault != "fresh-lock") { Assert.Equal(injectedLock, ReadNativePath(lockPath)); Assert.Equal(injectedLockTime, File.GetLastWriteTimeUtc(lockPath)); } // A rejected family must leave its preexisting owner lock bytes and timestamp unchanged.
                if (fault == "fresh-lock") { Assert.True(File.Exists(lockPath)); Assert.Empty(ReadNativePath(lockPath)); Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(lockPath)); } // Preserve the new refused empty custody lock as unknown evidence; it must not be adopted or rewritten as a valid lock.
                if (fault == "unknown") { Assert.Equal(injectedExtra, ReadNativePath(extra)); Assert.Equal(injectedExtraTime, File.GetLastWriteTimeUtc(extra)); } // Assert the injected unknown leaf survives unchanged before explicit fixture-only cleanup.
                Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); AssertFullMaterialNoEffects(h);
                if (reparsePath is not null) { Assert.NotNull(new FileInfo(reparsePath).LinkTarget); Assert.Equal(fault == "request-reparse" ? saved.Request : saved.Schema, File.ReadAllBytes(retainedPath)); }
            }
            catch (Exception ex) { primary = ex; throw; }
            finally
            {
                // Retention is asserted before this explicit test-owned restoration; the product never repairs or adopts injected evidence.
                RestoreFixtureAfterFault(() =>
                {
                    RestoreReferenceOwnerFile(sourcePath, sourceBytes, sourceTime);
                    System.Environment.SetEnvironmentVariable(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable, p.Environment.Json);
                    if (reparsePath is not null && File.Exists(retainedPath))
                    {
                        if (File.Exists(reparsePath)) File.Delete(reparsePath);
                        File.Move(retainedPath, reparsePath);
                    }
                    if (closed)
                    {
                        if (File.Exists(extra)) File.Delete(extra);
                        RestoreCustodyFile(requestPath, saved.Request, requestTime); RestoreCustodyFile(schemaPath, saved.Schema, schemaTime); // Recreate missing fixture leaves with the original owner-only ACL, exact bytes and timestamps; ordinary WriteAllBytes loses custody on Windows.
                        if (fault == "fresh-lock")
                        {
                            RestoreCustodyFile(lockPath, lockBytes, lockTime); // Restore the original owner lock using supported Open/CreateNew modes, never the Windows boundary's invalid OpenOrCreate mode.
                        }
                    }
                }, primary);
            }
            if (closed) // Only closed owners require a new custody opener after explicit fixture restoration.
            { // Reopen and reattach the saved baseline only for this explicitly restored closed owner.
                var reopened = h.Reopen(); // Retain the actual restored opener result instead of discarding its safe error code.
                output.WriteLine("NATIVE_PREPARED_RESTORED_REOPEN fault={0}; success={1}; openerError={2}", fault, reopened.IsSuccess, reopened.ErrorCode); // Identify the exact remaining case without exposing material or arbitrary messages.
                Assert.True(reopened.IsSuccess, "Restored native preparation fault=" + fault + "; openerError=" + reopened.ErrorCode); // Restoration must restore a positive custody control for this particular fault.
                b = (await h.Consume()).Value!; Assert.NotNull(b); // Reattach the saved baseline only after the genuine owner has reopened successfully.
            } // Finish closed-owner reattachment before the shared preparation replay assertions.
            var restored = await p.PrepareNative(b); Assert.True(restored.IsSuccess, restored.ErrorCode); Assert.True(restored.Value!.Replayed);
            Assert.Equal(saved.Request, NativePreparationBytes(h.Open.Session).Request); Assert.Equal(saved.Schema, NativePreparationBytes(h.Open.Session).Schema);
            AssertNativePreparationReleased(h.Open.Session); AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory));
        }
    }

    private sealed class NativeReadFaultStream(string path, bool shortRead) : FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
    {
        private int reads;
        public override int Read(byte[] buffer, int offset, int count)
        {
            reads++;
            if (shortRead) return 0; // Controlled stable-reader short result, not an alleged OS race.
            var n = base.Read(buffer, offset, count);
            if (reads >= 2 && n > 0) buffer[offset] ^= 1; // The second complete read differs while actual Length stays bounded.
            return n;
        }
    }

    private sealed record NativeCapacityProbe(ProtectedDisclosureHarness Owner,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline Baseline,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext Context, byte[] Request, int Cases) : IDisposable
    { public void Dispose() => Owner.Dispose(); }

    private async Task<NativeCapacityProbe> ProbeNativeCapacity(int cases)
    {
        var p = new ProtectedDisclosureHarness(nativeCapacityTask: NativeCapacityTeachingTask(cases));
        var previous = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext? context = null;
        try
        {
            var baseline = await p.H.Consume(); Assert.True(baseline.IsSuccess, baseline.ErrorCode + ": " + baseline.ErrorMessage);
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = c => context = c;
            var inspected = await p.Inspect(baseline.Value!); Assert.True(inspected.IsSuccess, inspected.ErrorCode);
            Assert.NotNull(context); Assert.Equal("ProtectedStartupOperatorApproval", context!.Origin);
            Assert.Equal(3, context.Candidates.Count); Assert.Contains(context.Candidates, c => c.OwnerInput == NativeCapacityTeachingTask(cases));
            foreach (var candidate in context.Candidates)
            {
                var source = p.H.Grant.SourceSelection!.TeachingCurriculum!.Sources.Single(x => x.SourceId == candidate.SourceId);
                var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(p.H.Fixture.RepositoryRoot, source.DomainSlug, source.SubjectSlug, source.CorpusVersion).Value!;
                var entry = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address).Value!.Corpus.Entries.Single(e => e.EntryId == candidate.ReferenceEntryId);
                Assert.Equal(entry.OwnerInput, candidate.OwnerInput); Assert.Equal(entry.PaidModelReferenceAnswer, candidate.PaidModelReferenceAnswer);
                Assert.Equal(entry.PaidReferenceProvenance, candidate.PaidReferenceProvenance);
            }
            var request = IndependentlyExpectedNativeRequest(p, context, new string('1', 32));
            output.WriteLine("NATIVE_GENUINE_CAPACITY cases={0}; input={1}; packet={2}; approval={3}; request={4}; schema=666; family={5}",
                cases, context.MetadataInput.CopyBytes().Length, context.CopyCanonicalBytes().Length,
                p.Startup.ResolveProtectedTeachingDisclosureApproval(p.H.Grant).Value!.CopyBytes().Length, request.Length, request.Length + 666);
            Assert.False(File.Exists(p.H.Fixture.OrdinaryHistory)); AssertFullMaterialNoEffects(p.H); AssertNativePreparationReleased(p.H.Open.Session);
            return new(p, baseline.Value!, context, request, cases);
        }
        catch { p.Dispose(); throw; }
        finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previous; }
    }

    private async Task AssertNativeExistingSelectedStudentPrefixAccounting()
    {
        // The existing private Student lease remains valid storage grammar; it grants no protected fresh material receipt.
        using var p = new ProtectedDisclosureHarness(nativeSelectedStudent: true); var h = p.H;
        var baseline = await h.Consume(); Assert.True(baseline.IsSuccess, baseline.ErrorCode);
        Assert.Equal(0, h.Open.Session.ReadCapacityPrefixV2().Value!.PrefixFrames); Assert.False(File.Exists(h.Fixture.OrdinaryHistory));
        var prepared = await p.PrepareNative(baseline.Value!); Assert.True(prepared.IsSuccess, prepared.ErrorCode);
        var pair = NativePreparationBytes(h.Open.Session); Assert.Equal(666, pair.Schema.Length);
        using var savedRequest = JsonDocument.Parse(pair.Request); var nativeNonce = savedRequest.RootElement.GetProperty("Nonce").GetString();
        var inputs = await BindSelectedStudent(h, "item-teach-7-0"); var generated = 0;
        try
        {
            h.Open.Session.SelectedStudentDurablePointForTesting = point => { if (point == "student-before-generate") generated++; };
            var student = await ConsumeSelectedStudent(h, inputs.Baseline, inputs.Admission);
            Assert.True(student.IsSuccess, student.ErrorCode); Assert.Equal("a", student.Value!.StudentResponse);
            Assert.Equal("SyntheticTest", student.Value.Origin); Assert.False(student.Value.FullTeacherContextReady);
            Assert.Equal(1, generated); Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting); Assert.Equal(10, h.Client.Calls);
        }
        finally { h.Open.Session.SelectedStudentDurablePointForTesting = null; }
        var history = h.HistoryBytes(); var read = h.Open.Session.ReadCapacityPrefixV2(); Assert.True(read.IsSuccess, read.ErrorCode);
        Assert.Equal(4, read.Value!.PrefixFrames); Assert.Equal(history.Length, read.Value.PrefixBytes);
        var heldLock = (FileStream)FreshSessionField(h.Open.Session, "_ownerLock")!; Assert.Equal(91, heldLock.Length);
        var charge = checked((long)history.Length + heldLock.Length + pair.Request.Length + pair.Schema.Length);
        Assert.InRange(charge, 1, 229376);
        output.WriteLine("NATIVE_VALID_STUDENT_PREFIX history={0}; lock={1}; request={2}; schema={3}; aggregate={4}; frames=4; isolatedGenerate=1", history.Length, heldLock.Length, pair.Request.Length, pair.Schema.Length, charge);
        Assert.True(h.Reopen().IsSuccess); // Genuine typed opener authenticates the complete saved grammar and all co-resident files.
        var reattached = await h.Consume(); Assert.True(reattached.IsSuccess, reattached.ErrorCode); Assert.Equal(10, h.Client.Calls);
        var owner = h.Open.Session; var capacity = owner.CapacityV2; var effects = owner.Snapshot.Effects; var accounting = owner.SelectedStudentAccounting; var files = h.Files();
        var heldOwners = new[] { "_ownerLock", "_v2History", "_nativePreparedRequest", "_nativePreparedSchema", "_studentLease", "_selectionFrame" }
            .ToDictionary(name => name, name => FreshSessionField(owner, name));
        Assert.All(heldOwners.Values, value => Assert.NotNull(value));
        Assert.Equal(4, owner.ReadCapacityPrefixV2().Value!.PrefixFrames); Assert.Equal(history, h.HistoryBytes());
        Assert.Equal(pair.Request, NativePreparationBytes(owner).Request); Assert.Equal(pair.Schema, NativePreparationBytes(owner).Schema);
        Assert.Equal(charge, checked((long)history.Length + ((FileStream)FreshSessionField(owner, "_ownerLock")!).Length + pair.Request.Length + pair.Schema.Length));
        var captures = 0; var fixedConsumer = 0; var issued = 0; var writes = 0;
        var previous = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++;
            owner.SelectedStudentDurablePointForTesting = point => { if (point.Contains("generate", StringComparison.Ordinal)) throw new InvalidOperationException("Existing Completed must never Generate again"); };
            owner.SelectionMaterialPointForTesting = point =>
            {
                if (point == "native-preparation-fixed-consumer") fixedConsumer++;
                if (point == "teaching-disclosure-receipt-issued") issued++;
                if (point is "native-preparation-request-flushed" or "native-preparation-readback-staged") writes++;
            };
            var refused = await p.PrepareNative(reattached.Value!); Assert.False(refused.IsSuccess); Assert.Null(refused.Value);
            // Core1845 refuses the saved/nonempty owner with OwnerMismatch; absent issued approval makes revision final1884 Unavailable win at2111.
            Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable, refused.ErrorCode);
            Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable, owner.TeachingDisclosureRevisionFinalErrorForTesting);
            Assert.Null(owner.SelectionMaterialReceivingFinalErrorForTesting); Assert.Null(owner.NativePreparedFamilyFinalErrorForTesting);
            Assert.Equal(0, captures); Assert.Equal(0, fixedConsumer); Assert.Equal(0, issued); Assert.Equal(0, writes);
            AssertNativePreparationReleased(owner); Assert.Same(capacity, owner.CapacityV2); Assert.Equal(effects, owner.Snapshot.Effects); Assert.Equal(accounting, owner.SelectedStudentAccounting);
            foreach (var held in heldOwners) Assert.Same(held.Value, FreshSessionField(owner, held.Key));
            Assert.Equal(history, h.HistoryBytes()); Assert.Equal(pair.Request, NativePreparationBytes(owner).Request); Assert.Equal(pair.Schema, NativePreparationBytes(owner).Schema); h.AssertFiles(files);
            var replay = await ConsumeSelectedStudent(h, reattached.Value!, inputs.Admission); Assert.True(replay.IsSuccess, replay.ErrorCode);
            Assert.Equal("a", replay.Value!.StudentResponse); Assert.Equal(10, h.Client.Calls); Assert.Equal(1, generated);
            Assert.Equal(history, h.HistoryBytes()); Assert.Equal(accounting, owner.SelectedStudentAccounting); h.AssertFiles(files);
            foreach (var held in heldOwners) Assert.Same(held.Value, FreshSessionField(owner, held.Key));
            using var afterRequest = JsonDocument.Parse(NativePreparationBytes(owner).Request); Assert.Equal(nativeNonce, afterRequest.RootElement.GetProperty("Nonce").GetString());
        }
        finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previous; owner.SelectionMaterialPointForTesting = null; owner.SelectedStudentDurablePointForTesting = null; }
    }

    [Fact]
    public async Task NativePrepared_ObservedOverageDiffersFromShortUnstableCustody()
    {
        using var seed = await ProbeNativeCapacity(0);
        Assert.InRange(seed.Request.Length, 1, 25731); Assert.InRange(seed.Context.CopyCanonicalBytes().Length, 1, 16384);
        var seedControl = await seed.Owner.PrepareNative(seed.Baseline); Assert.True(seedControl.IsSuccess, seedControl.ErrorCode);
        // Complete born-fixture task cases measure the independent V2 writer and old packet separately.
        // The old packet can saturate first: no invented genuine request-near25731 minimum.
        var clauseBytes = Encoding.UTF8.GetByteCount(NativeCapacityTeachingTask(1)) - Encoding.UTF8.GetByteCount(NativeCapacityTeachingTask(0));
        var nearCases = Math.Max(0, (16384 - seed.Context.CopyCanonicalBytes().Length - 32) / clauseBytes);
        // Each case contributes at least clauseBytes to one complete candidate in the old packet.
        // At this finite last case, task bytes alone exceed 16384, so the search must cross the cap.
        var lastMeasuredCase = checked(16384 / clauseBytes + 1); var foundNextOverage = false;
        for (var cases = nearCases; cases <= lastMeasuredCase; cases++)
        {
            // Separate born-complete sealed fixture; dispose in LIFO order before the next owned root.
            using var measured = await ProbeNativeCapacity(cases);
            if (measured.Context.CopyCanonicalBytes().Length > 16384)
            { Assert.Equal(nearCases + 1, cases); foundNextOverage = true; break; }
            nearCases = cases;
        }
        Assert.True(foundNextOverage);
        output.WriteLine("NATIVE_GENUINE_FINITE_PACKET_BOUNDARY nearCases={0}; nextOverCases={1}; lastMeasuredCase={2}; maximumMeasurementFixtures={3}",
            nearCases, nearCases + 1, lastMeasuredCase, lastMeasuredCase + 1);
        using (var near = await ProbeNativeCapacity(nearCases))
        {
            Assert.InRange(near.Context.CopyCanonicalBytes().Length, 1, 16384); Assert.InRange(near.Request.Length, 1, 25731);
            Assert.InRange(16384 - near.Context.CopyCanonicalBytes().Length, 0, clauseBytes + 64);
            var control = await near.Owner.PrepareNative(near.Baseline); Assert.True(control.IsSuccess, control.ErrorCode);
            var saved = NativePreparationBytes(near.Owner.H.Open.Session); var decoded = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(saved.Request).Value!;
            Assert.Equal(IndependentlyExpectedNativeRequest(near.Owner, near.Context, decoded.Nonce), saved.Request);
            var restored = await near.Owner.PrepareNative(near.Baseline); Assert.True(restored.IsSuccess, restored.ErrorCode); Assert.True(restored.Value!.Replayed);
            Assert.Equal(saved.Request, NativePreparationBytes(near.Owner.H.Open.Session).Request); AssertNativePreparationReleased(near.Owner.H.Open.Session); AssertFullMaterialNoEffects(near.Owner.H);
        }
        using (var over = await ProbeNativeCapacity(nearCases + 1))
        {
            Assert.True(over.Context.CopyCanonicalBytes().Length > 16384); Assert.InRange(over.Request.Length, 1, 25731);
            var owner = over.Owner.H.Open.Session; var files = over.Owner.H.Files(); var flushes = 0;
            owner.SelectionMaterialPointForTesting = point => { if (point is "native-preparation-request-flushed" or "native-preparation-readback-staged") flushes++; };
            try { var refusal = await over.Owner.PrepareNative(over.Baseline); Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode, refusal.ErrorCode);
                Assert.Equal(0, flushes); Assert.Null(FreshSessionField(owner, "_nativePreparedRequestBytes")); Assert.Null(FreshSessionField(owner, "_nativePreparedSchemaBytes"));
                Assert.Equal(false, FreshSessionField(owner, "_nativePreparedPoisoned")); over.Owner.H.AssertFiles(files); AssertNativePreparationReleased(owner); AssertFullMaterialNoEffects(over.Owner.H); }
            finally { owner.SelectionMaterialPointForTesting = null; }
        }
        using (var oversized = new ProtectedDisclosureHarness(nativePreparation: true, oversizedNativePreparation: true))
        {
            var baseline = (await oversized.H.Consume()).Value!; var files = oversized.H.Files();
            var refusal = await oversized.PrepareNative(baseline); Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode, refusal.ErrorCode);
            oversized.H.AssertFiles(files); Assert.Null(FreshSessionField(oversized.H.Open.Session,"_nativePreparedRequestBytes")); AssertFullMaterialNoEffects(oversized.H);
        }
        // Fixed schema + maximum admitted request + genuine v2 owner-lock91 and absent history: no fresh aggregate overage is reachable.
        Assert.Equal(26397, 25731 + 666); Assert.True(26397 < 57364); Assert.Equal(26488, 26397 + 91); Assert.True(26488 < 229376);

        foreach (var fault in new[] { "overage", "family-overage", "occupied-held-session", "empty", "null", "short", "unstable", "path-mismatch" })
        {
            using var p = new ProtectedDisclosureHarness(nativeCapacityTask: NativeCapacityTeachingTask(0)); var h = p.H; var b = (await h.Consume()).Value!;
            var control = await p.PrepareNative(b); Assert.True(control.IsSuccess, control.ErrorCode);
            Assert.InRange(control.Value!.RequestLength, 1, CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes);
            var saved = NativePreparationBytes(h.Open.Session); var owner = h.Open.Session;
            var field = typeof(CodeDocumentationOneLessonSession).GetField("_nativePreparedRequest", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var stream = (FileStream)field.GetValue(owner)!; FileStream? injected = null; Exception? primary = null;
            var schemaStream = (FileStream)FreshSessionField(owner, "_nativePreparedSchema")!; // Retain the actual Session schema custody handle.
            var requestPath = Path.Combine(Path.GetDirectoryName(h.Fixture.OrdinaryHistory)!, CodeDocumentationNativeCodexSelectionWire.RequestFileName); // Resolve the owned request pathname independently of SafeFileHandle-backed FileStream.Name.
            var schemaPath = Path.Combine(Path.GetDirectoryName(h.Fixture.OrdinaryHistory)!, CodeDocumentationNativeCodexSelectionWire.SchemaFileName); // Resolve the separate schema pathname from the same owned family layout.
            Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, requestPath)); Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(schemaStream.SafeFileHandle, schemaPath)); // Confirm both retained handles belong to the independently resolved positive-control paths.
            var lockStream = (FileStream)FreshSessionField(owner, "_ownerLock")!; var lockBytes = new byte[lockStream.Length];
            lockStream.Position = 0; lockStream.ReadExactly(lockBytes); Assert.Equal(91, lockBytes.Length);
            var effects = owner.Snapshot.Effects; var capacity = owner.CapacityV2;
            try
            {
                switch (fault)
                {
                    case "overage": stream.SetLength(CodeDocumentationNativeCodexSelectionWire.RequestMaximumBytes + 1); stream.Flush(true); break;
                    case "family-overage": schemaStream.SetLength(57364L - stream.Length + 1); schemaStream.Flush(true); Assert.Equal(57365, stream.Length + schemaStream.Length); break;
                    case "occupied-held-session":
                        // Controlled observed storage only: an overlong held lock is NOT a valid reopenable lock or new history authority.
                        lockStream.SetLength(229376L - stream.Length - schemaStream.Length + 1); lockStream.Flush(true);
                        Assert.Equal(229377, stream.Length + schemaStream.Length + lockStream.Length); break;
                    case "empty": stream.SetLength(0); stream.Flush(true); break;
                    case "null": field.SetValue(owner, null); break;
                    case "short": case "unstable": injected = new NativeReadFaultStream(requestPath, fault == "short"); field.SetValue(owner, injected); break; // Inject the controlled reader over the real request file rather than a handle display name.
                    case "path-mismatch": injected = new FileStream(schemaPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); field.SetValue(owner, injected); break; // Keep the genuine wrong-file handle so the production path-versus-handle guard is exercised.
                }
                var injectedRequest = ReadNativePath(requestPath); var injectedSchema = ReadNativePath(schemaPath); // Capture actual on-disk fault bytes before invoking the guarded preparation path.
                var injectedLockLength = lockStream.Length;
                var refused = await p.PrepareNative(b); Assert.False(refused.IsSuccess);
                var overage = fault is "overage" or "family-overage" or "occupied-held-session";
                Assert.Equal(overage && fault != "occupied-held-session" ? CodeDocumentationOneLessonSession.CapacityExceededErrorCode : CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, refused.ErrorCode);
                Assert.Equal(overage ? CodeDocumentationOneLessonSession.CapacityExceededErrorCode : CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, owner.NativePreparedFamilyFinalErrorForTesting);
                // Receiving custody retains priority over the separate occupied-storage final; both guards actually run.
                if (fault == "occupied-held-session") Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, owner.SelectionMaterialReceivingFinalErrorForTesting);
                Assert.Equal(injectedRequest, ReadNativePath(requestPath)); Assert.Equal(injectedSchema, ReadNativePath(schemaPath)); Assert.Equal(injectedLockLength, lockStream.Length); // Prove the product retained both injected disk files and the observed held-lock length unchanged.
                Assert.Equal(saved.Request, NativePreparationBytes(owner).Request); Assert.Equal(saved.Schema, NativePreparationBytes(owner).Schema);
                Assert.Equal(effects, owner.Snapshot.Effects); Assert.Same(capacity, owner.CapacityV2);
                AssertNativePreparationReleased(owner); AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory));
            }
            catch (Exception ex) { primary = ex; throw; }
            finally { RestoreFixtureAfterFault(() => { field.SetValue(owner, stream); injected?.Dispose(); WriteNativeHeld(stream, saved.Request); WriteNativeHeld(schemaStream, saved.Schema); WriteNativeHeld(lockStream, lockBytes); }, primary); }
            var recovered = await p.PrepareNative(b); Assert.True(recovered.IsSuccess, recovered.ErrorCode); Assert.True(recovered.Value!.Replayed);
            Assert.Equal(saved.Request, NativePreparationBytes(owner).Request); Assert.Equal(saved.Schema, NativePreparationBytes(owner).Schema);
        }
        await AssertNativeExistingSelectedStudentPrefixAccounting();
    }

    [Fact]
    public async Task NativePrepared_AllIndependentFinalsPreservePrimaryAndRelease()
    {
        foreach (var fault in new[] { "business-source", "business-receiving", "business-baseline", "exception", "cancellation" })
        {
            using var p = new ProtectedDisclosureHarness(nativePreparation: true); var h = p.H; var b = (await h.Consume()).Value!;
            var control = await p.PrepareNative(b); Assert.True(control.IsSuccess, control.ErrorCode);
            var owner = h.Open.Session; var saved = NativePreparationBytes(owner);
            var descriptor = h.Grant.SourceSelection!.TeachingCurriculum!.Sources[1];
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, descriptor.DomainSlug, descriptor.SubjectSlug, descriptor.CorpusVersion).Value!;
            var sourcePath = Path.Combine(address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
            var sourceBytes = File.ReadAllBytes(sourcePath); var sourceTime = File.GetLastWriteTimeUtc(sourcePath);
            var baselineBytes = File.ReadAllBytes(h.Fixture.OrdinaryBaseline); var baselineTime = File.GetLastWriteTimeUtc(h.Fixture.OrdinaryBaseline);
            var prefixField = typeof(CodeDocumentationOneLessonSession).GetField("_v2Prefix", BindingFlags.NonPublic | BindingFlags.Instance)!; var prefix = prefixField.GetValue(owner);
            var schemaStream = (FileStream)FreshSessionField(owner, "_nativePreparedSchema")!;
            using var cancellation = new CancellationTokenSource();
            Exception expected = fault == "cancellation" ? new OperationCanceledException("native prepared primary", null, cancellation.Token) : new InvalidOperationException("native prepared primary");
            var secondary = new IOException("native family final secondary"); var finals = new List<string>();
            CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt? receipt = null; var staged = false; Exception? primary = null;
            try
            {
                owner.TeachingDisclosureInspectionRefusalForTesting = fault.StartsWith("business", StringComparison.Ordinal);
                owner.SelectionMaterialPointForTesting = point =>
                {
                    if (DisclosureFinalPoints.Contains(point) || point == "native-prepared-family-final-none")
                    {
                        finals.Add(point); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, p.PrepareNative(b).GetAwaiter().GetResult().ErrorCode);
                        if (staged && point == "native-prepared-family-final-none" && fault is "exception" or "cancellation") throw secondary;
                    }
                    var stage = fault.StartsWith("business", StringComparison.Ordinal) ? "teaching-disclosure-inspection-result-staged" : "native-preparation-readback-staged";
                    if (point != stage) return;
                    staged = true;
                    if (fault.StartsWith("business", StringComparison.Ordinal)) Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, owner.TeachingDisclosureStagedErrorForTesting);
                    else { receipt = (CodeDocumentationOneLessonSession.NativeSelectionPreparationReceipt)FreshSessionField(owner, "_nativePreparationReceipt")!; Assert.True(receipt.IsActiveFor(owner)); }
                    foreach (var pnt in DisclosureFinalPoints.Append("native-prepared-family-final-none")) Assert.Equal(1, finals.Count(x => x == pnt));
                    RestoreReferenceOwnerFile(sourcePath, sourceBytes.Append((byte)' ').ToArray(), sourceTime);
                    System.Environment.SetEnvironmentVariable(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable, p.Environment.Json.Replace("\"ApprovalRevision\":1", "\"ApprovalRevision\":2", StringComparison.Ordinal));
                    WriteNativeHeld(schemaStream, saved.Schema.Append((byte)' ').ToArray());
                    if (fault == "business-receiving") prefixField.SetValue(owner, new byte[] { 1 });
                    if (fault == "business-baseline") RestoreCustodyFile(h.Fixture.OrdinaryBaseline, baselineBytes.Append((byte)' ').ToArray(), baselineTime);
                    if (fault == "cancellation") cancellation.Cancel();
                    if (fault is "exception" or "cancellation") throw expected;
                };
                if (fault is "exception" or "cancellation")
                {
                    var thrown = await Record.ExceptionAsync(() => p.PrepareNative(b, cancellation.Token)); Assert.Same(expected, thrown);
                    if (thrown is OperationCanceledException canceled) Assert.Equal(cancellation.Token, canceled.CancellationToken);
                    Assert.Same(secondary, thrown!.Data["NativePreparedFamilyFinalFailure"]);
                    Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, thrown.Data["SelectionMaterialFinalSourceError"]);
                    Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Stale, thrown.Data["SelectionMaterialTeachingDisclosureRevisionFinalError"]);
                }
                else
                {
                    var refused = await p.PrepareNative(b); Assert.False(refused.IsSuccess);
                    Assert.Equal(fault == "business-source" ? RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode : fault == "business-baseline" ? CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode : CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, refused.ErrorCode);
                    Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Stale, owner.TeachingDisclosureRevisionFinalErrorForTesting);
                    Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, owner.NativePreparedFamilyFinalErrorForTesting);
                }
                Assert.True(staged); foreach (var pnt in DisclosureFinalPoints.Append("native-prepared-family-final-none")) Assert.Equal(2, finals.Count(x => x == pnt));
                if (receipt is not null) Assert.False(receipt.IsActiveFor(owner)); AssertNativePreparationReleased(owner); AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory));
            }
            catch (Exception ex) { primary = ex; throw; }
            finally
            {
                owner.SelectionMaterialPointForTesting = null; owner.TeachingDisclosureInspectionRefusalForTesting = false;
                RestoreFixtureAfterFault(() => { prefixField.SetValue(owner, prefix); RestoreReferenceOwnerFile(sourcePath, sourceBytes, sourceTime); RestoreCustodyFile(h.Fixture.OrdinaryBaseline, baselineBytes, baselineTime); WriteNativeHeld(schemaStream, saved.Schema); System.Environment.SetEnvironmentVariable(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable, p.Environment.Json); }, primary);
            }
            var restored = await p.PrepareNative(b); Assert.True(restored.IsSuccess, restored.ErrorCode); Assert.True(restored.Value!.Replayed);
        }
    }

    [Fact]
    public void WirePrepared_ClosedDataNeverMintsCapability()
    {
        // This is deliberately plain serialized data. No fixture, grant, Session, receipt, process or model is arranged.
        const string policyHash = "ba967b55007606dcfed2dca4e8c5f856c2134170a020706ca8ff9b6d6f35062a";
        const string schema = "{\"type\":\"object\",\"additionalProperties\":false,\"required\":[\"Nonce\",\"PreparedRequestSha256\",\"ItemId\",\"SourceId\",\"ReferenceEntryId\",\"SourceEntityFingerprint\",\"MaterialAdmissionEvidenceSha256\",\"Reason\"],\"properties\":{\"Nonce\":{\"type\":\"string\",\"pattern\":\"^[0-9a-f]{32}$\"},\"PreparedRequestSha256\":{\"type\":\"string\",\"pattern\":\"^[0-9a-f]{64}$\"},\"ItemId\":{\"type\":\"string\",\"minLength\":1},\"SourceId\":{\"type\":\"string\",\"minLength\":1},\"ReferenceEntryId\":{\"type\":\"string\",\"minLength\":1},\"SourceEntityFingerprint\":{\"type\":\"string\",\"minLength\":1},\"MaterialAdmissionEvidenceSha256\":{\"type\":\"string\",\"pattern\":\"^[0-9a-f]{64}$\"},\"Reason\":{\"type\":\"string\",\"minLength\":1,\"maxLength\":1024}}}";
        var policy = CodeDocumentationNativeCodexSelectionHost.PolicyContent;
        Assert.Equal(3662, Encoding.UTF8.GetByteCount(policy)); Assert.Equal(policyHash, CodeDocumentationNativeCodexSelectionWire.Sha256(Encoding.UTF8.GetBytes(policy)));
        var data = new CodeDocumentationNativeCodexSelectionWire.PreparedNativeSelectionRequestV1(new string('a', 64), "e30=", new string('b', 32), new string('c', 64),
            Array.AsReadOnly(new[] { "missing_essential_behavior" }), "e30=", "e30=", "e30=", new string('d', 64), policyHash, policy,
            new string('1', 32), "Full permitted Unicode task 中文\nreference suffix", Convert.ToBase64String(Encoding.UTF8.GetBytes(schema)));
        // A separate ordered literal JSON writer establishes the expected canonical whole request, rather than trusting Write's own round-trip alone.
        var options = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        var literal = "{\"SchemaVersion\":1,\"Domain\":\"code-documentation-native-selection-prepared-v1\",\"SessionId\":" + JsonSerializer.Serialize(data.SessionId, options) +
            ",\"BindingCanonicalBase64\":\"e30=\",\"OriginalReceiptId\":" + JsonSerializer.Serialize(data.OriginalReceiptId, options) + ",\"OriginalReceiptSha256\":" + JsonSerializer.Serialize(data.OriginalReceiptSha256, options) +
            ",\"Categories\":[\"missing_essential_behavior\"],\"InputCanonicalBase64\":\"e30=\",\"MaterialPacketCanonicalBase64\":\"e30=\",\"ApprovalCanonicalBase64\":\"e30=\",\"ApprovalEvidenceSha256\":" + JsonSerializer.Serialize(data.ApprovalEvidenceSha256, options) +
            ",\"PolicyId\":\"S04-CODEX-TEACHING-SELECTION-v1\",\"PolicyVersion\":\"" + policyHash + "\",\"PolicyContentUtf8\":" + JsonSerializer.Serialize(policy, options) +
            ",\"Nonce\":\"" + new string('1', 32) + "\",\"RenderedPromptUtf8\":" + JsonSerializer.Serialize(data.RenderedPromptUtf8, options) +
            ",\"RequestedModel\":\"gpt-6-astra\",\"RequestedReasoningEffort\":\"ultra\",\"OutputSchemaCanonicalBase64\":" + JsonSerializer.Serialize(data.OutputSchemaCanonicalBase64, options) + "}";
        var bytes = Encoding.UTF8.GetBytes(literal); Assert.Equal(bytes, CodeDocumentationNativeCodexSelectionWire.Write(data));
        var parsed = CodeDocumentationNativeCodexSelectionWire.Decode(bytes); Assert.True(parsed.IsSuccess, parsed.ErrorCode);
        Assert.IsType<CodeDocumentationNativeCodexSelectionWire.PreparedNativeSelectionRequestV1>(parsed.Value);
        Assert.Equal(Encoding.UTF8.GetBytes(schema), CodeDocumentationNativeCodexSelectionWire.CopySchemaBytes());
        var malformed = new List<byte[]> {
            Encoding.UTF8.GetBytes(literal.Replace("\"SchemaVersion\":1,", "\"SchemaVersion\":1,\"SchemaVersion\":1,", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(literal.Replace("\"SchemaVersion\":1,", "\"SchemaVersion\":1.0,", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(literal.Replace("\"SchemaVersion\":1,", "\"SchemaVersion\":1e0,", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(literal.Replace("\"SchemaVersion\"", "\"schemaVersion\"", StringComparison.Ordinal)),
            bytes.Append((byte)'\n').ToArray(), new byte[] { 0xff }, "null"u8.ToArray()
        };
        foreach (var mutation in new[] { "missing", "extra", "null", "base64", "duplicate-category", "empty-category", "nonce", "policy", "schema", "synthetic-model" })
        {
            var node = JsonNode.Parse(literal)!.AsObject();
            switch (mutation)
            {
                case "missing": node.Remove("Nonce"); break;
                case "extra": node["Unexpected"] = true; break;
                case "null": node["SessionId"] = null; break;
                case "base64": node["InputCanonicalBase64"] = "e3 0="; break;
                case "duplicate-category": node["Categories"] = new JsonArray("missing_essential_behavior", "missing_essential_behavior"); break;
                case "empty-category": node["Categories"] = new JsonArray(); break;
                case "nonce": node["Nonce"] = new string('0', 32); break;
                case "policy": node["PolicyVersion"] = new string('e', 64); break;
                case "schema": node["OutputSchemaCanonicalBase64"] = "e30="; break;
                case "synthetic-model": node["RequestedModel"] = "synthetic"; break;
            }
            malformed.Add(Encoding.UTF8.GetBytes(node.ToJsonString(options)));
        }
        foreach (var invalid in malformed) { var refusal = CodeDocumentationNativeCodexSelectionWire.Decode(invalid); Assert.False(refusal.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, refusal.ErrorCode); }
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode, CodeDocumentationNativeCodexSelectionWire.Decode(new byte[25732]).ErrorCode);
        // Measure duplicated physical representations without claiming genuine admission or future P1 coverage.
        var sized = CodeDocumentationNativeCodexSelectionWire.Write(data with { InputCanonicalBase64 = Convert.ToBase64String(new byte[8000]), MaterialPacketCanonicalBase64 = Convert.ToBase64String(new byte[12000]) });
        output.WriteLine("NATIVE_WIRE_DATA_ONLY_LENGTHS minimal={0}; illustrativeInput=8000; illustrativePacket=12000; request={1}; schema={2}; family={3}; genuineAdmission=false", bytes.Length, sized.Length, Encoding.UTF8.GetByteCount(schema), sized.Length + Encoding.UTF8.GetByteCount(schema));
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode, CodeDocumentationNativeCodexSelectionWire.Decode(sized).ErrorCode);

        // Independent SOURCE36 shape with corrected exact existing owner UTC spelling: hashes/full graph, no grant or legal authority.
        const string compactLiteral = """
{"SchemaVersion":2,"Domain":"code-documentation-native-selection-prepared-v2","SessionId":"aaad2412180af315d85e9f9cff5428fbf8ea5574bef3312e564513831920767b","Atoms":["ordinary-bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb","ordinary-grant","v2","ordinary-run-bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb","dda18a0e21ae47c53b4309434cbc02ae8bf764fa83a6defbb719431242722aa7","c4ae02debc56c8ffba2e3434b90a37092ac1449093ae0ac261f9fe8627ec315e","d3e6fcc543e93a22ebb2793d9aa5401478a4982d455f98107a60a604ddcce9f0","3c5229cc80babc781ea3461dcb3065b3992cf3a272bccd7d850f7f47204921fa","fe8394ebd0eca05ab87620ed5355a84cf5941f5eb4de60e5b5142889a24e1691","915bec14c36c4a76e9e1412304d2ddff08866fcaf47ced1677cb16e0abe14860","d8463bd3ba4b10e5916f65fa7b0c1f9f91f67ca40cc25b48810fb2f5a3340488","3b8b91c75627bee566dcb88f4805901b20a3eab2520bcff8d26c87157a035026","21e721c35a5823fdb452fa2f9f0a612c74fb952e06927489c6b27a43b817bed4","64879f7d6b960a01909762d911a32d4582c20010c5641ee90278b644a9e3b525","67d35554c1f60638d6d72a0edc045450a841f7efabe0f9843b7e653499094538","0510eddd781102030eb8860671503a28e6a37f5346de429bdd47c0a37c77cc7d","code-documentation-selection-binding-input-v1","14f8d2b86fa8a6516827a5b39430d1a0","0682c5f2076f099c34cfdd15a9e063849ed437a49677e6fcc5b4198c76575be5","missing_essential_behavior","ordinary-source","TrainingPairs","csharp-code-documentation","ilearning-unit","s0304-cccccccccccccccccccccccccccccccc","b22fb993abcb4bfe551b9758c540c8a4bd0b0ae2f256056d756c5205eda06f3d","240429a517d44f32432bd5ec9d2a671458d3a595e648dc2740c038d2d6c0b2c3","47d8bd46877831ccd61c1cfac962cd4143bc818c7d5fd0989de4f061ce60351e","np","a","c7","bug","np7-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","8e9214d9a2ce197b06639aa1b2a06ecd4649a9ed01fc4aadd5ff0eb6a3f6309d","d2196c13a15213d83ddce2373f307081c7d3132df05136181d4702a86fac9f09","a2a9cf6047dc46b2eea1fafb92dbb2c9d94e4c37ad9b1a9ce4a1429670d11e36","b","c2","np2-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","329c9a0dcbd7f3c3b715ead376a66986d5598cafb459fa03ed46ea4173163052","4f792a9324d06cdc102d35c54d1daec3a4fe8f31790e0a4e88ab26c9cca0f453","bddd3a6e5dab59fa05fa8fb0d789a3c2ad3c9982dcc4a368f3d5719f421ffda8","a0","sha256:7799f6dec07415371f59a8ec1934813754771f159920aadf02bbf2f2e89c86e5","sha256:e1d09ec9ce808cc19f26791265eca833c7f96bedc09f24d8c16cb297ad505d6f","a1","sha256:9a47bcfa65119b5c51109e58f2742d9b87270b80049527a39eb3eca2790b3fdc","sha256:45148aab611878de3b164f5ef47f87cc4b4810a58fa24e43511f46f76ed3da02","a2","sha256:d4bcb82a1e894d1fd91ec1ecb4b0da4a929ed9e42eef04da4ae30fcd5372c8f2","sha256:2b9efbbd6793d9b8f4cd65deb64c1419a8837449b4cc8e2a99ccc5ed4ae5edc3","b0","sha256:c3e65b605ca1b246a91c4c0fa315f24b523e4888624fefa09ce7f17ea3246560","sha256:5b5f6e3820cb98e674e4322752e31ef244cd6cadd94742cce867101299ba6799","unused","repository-owned-reference-corpus-manifest-v1","continuous-learning-frozen-paid-reference-corpus-v1","n7","1970-01-01T00:00:00.0000000+00:00","sha256:d2196c13a15213d83ddce2373f307081c7d3132df05136181d4702a86fac9f09","training-data/reference-corpora/bug/np/np7-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","training-data/reference-corpora/bug/np/np7-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/corpus.json","training-data/reference-corpora/bug/np/np7-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/manifest.json","71dd25781deb6198e71ff5e2ecd42d95f7afe6618c5f7840ab52c975e9e41288","Train","sha256:bc0cdff189e4b75d7ec416d86e16744cd384739ac5ed564090e56d1e2f9c5ea9","fp","fm","v1","n7-0","sha256:9a352eda26b92f9337a2bab46626ef3d9fcc94b5c9f5d99db2ee113a90cf82cc","6:schema=V42:xiigen.closed-paid-reference-provenance.v1;12:provider_ref=V2:fp;8:model_id=V2:fm;13:model_version=V2:v1;10:request_id=V4:n7-0;15:captured_at_utc=V33:1970-01-01T00:00:00.0000000+00:00;23:reference_answer_sha256=V71:sha256:9a352eda26b92f9337a2bab46626ef3d9fcc94b5c9f5d99db2ee113a90cf82cc;21:closed_paid_reference=V4:true;37:paid_reference_output_never_trainable=V4:true;41:paid_reference_output_never_dpo_pair_side=V4:true;","sha256:dee66cda40c58cb6408d4a6cca2d7307bad1789ca1b30c4421283c495c3905dd","HeldOut","sha256:221901bb595f8bc468b19847f267a62460d4144c93be1b8b4a04b27b2487f6ec","n7-1","sha256:47836fe94644f71c63f952a09ac0432dd1b012daf79e62a9137cec745cdcd1cc","6:schema=V42:xiigen.closed-paid-reference-provenance.v1;12:provider_ref=V2:fp;8:model_id=V2:fm;13:model_version=V2:v1;10:request_id=V4:n7-1;15:captured_at_utc=V33:1970-01-01T00:00:00.0000000+00:00;23:reference_answer_sha256=V71:sha256:47836fe94644f71c63f952a09ac0432dd1b012daf79e62a9137cec745cdcd1cc;21:closed_paid_reference=V4:true;37:paid_reference_output_never_trainable=V4:true;41:paid_reference_output_never_dpo_pair_side=V4:true;","sha256:594b3508f9be5ad8c1d164701c00579767f89c4a293709147660b74fb9c6b034","sha256:a4d6257ef3922c41d23089b139a3572e9ff00bb7d18ade389bc657dd3b23999c","n7-2","sha256:38a0462daa2e1baa6f6a3390f66e22ca634d481320f5de911e513e1eaaaf63dd","6:schema=V42:xiigen.closed-paid-reference-provenance.v1;12:provider_ref=V2:fp;8:model_id=V2:fm;13:model_version=V2:v1;10:request_id=V4:n7-2;15:captured_at_utc=V33:1970-01-01T00:00:00.0000000+00:00;23:reference_answer_sha256=V71:sha256:38a0462daa2e1baa6f6a3390f66e22ca634d481320f5de911e513e1eaaaf63dd;21:closed_paid_reference=V4:true;37:paid_reference_output_never_trainable=V4:true;41:paid_reference_output_never_dpo_pair_side=V4:true;","sha256:9699c5e6e981066ef0c707bdbbf4e93c2739d16333c3809afedd8af206ab5fc8","n2","sha256:4f792a9324d06cdc102d35c54d1daec3a4fe8f31790e0a4e88ab26c9cca0f453","training-data/reference-corpora/bug/np/np2-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","training-data/reference-corpora/bug/np/np2-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/corpus.json","training-data/reference-corpora/bug/np/np2-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/manifest.json","8609f1753a27537fa5ea65336ba11cdfbc364f9430995d7e3039b6d3b0a31a27","sha256:5663491ed5c477cbbd4e23e279861fb634e09e653999ce299c7258f1edc26eb1","n2-0","sha256:1bc14faedfd00bbcfd745dd4915b645964be57ed7682301e061f72fd872b0201","6:schema=V42:xiigen.closed-paid-reference-provenance.v1;12:provider_ref=V2:fp;8:model_id=V2:fm;13:model_version=V2:v1;10:request_id=V4:n2-0;15:captured_at_utc=V33:1970-01-01T00:00:00.0000000+00:00;23:reference_answer_sha256=V71:sha256:1bc14faedfd00bbcfd745dd4915b645964be57ed7682301e061f72fd872b0201;21:closed_paid_reference=V4:true;37:paid_reference_output_never_trainable=V4:true;41:paid_reference_output_never_dpo_pair_side=V4:true;","sha256:2ed54185a3cd9ba276b51814bdcfb82ee3e6024804bf5a03b87a85b146be95f5","synthetic-legal-operator-fixture","fixture-operator","2026-01-01T00:00:00.0000000Z","PermitFullTeachingSourceDisclosure","PedagogicalSelectionContextOnly","ServerOwnedNativeCodexPedagogicalSelection","gpt-6-astra","ultra","test:synthetic-legal-truth","ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff","sha256:eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee","中文\nT7/0\tEND","中文\nR7/0\tEND","0d008e10b1a3c6a7e9b7b8f4755bb7fe870bdb5654cc7ab22ef912160396ac2c","中文\nT7/1\tEND","中文\nR7/1\tEND","7c5ba84c2a6c7f3d02d8a0a57197ab9306a373c0b0f208e71fa9536aec3df583","中文\nT7/2\tEND","中文\nR7/2\tEND","a406c9658646a9ce8a42faa77dbe2ab42f586037dd88161517859f18cf082a15"],"Binding":[0,1,2,3,4,5,6,7,8,[2,9,10,11,12,13,1,5,14,15]],"Input":[16,17,18,9,[19],[[[[20,21,22,23,24,25,999999,26]],[[27,20]],[28,2,[[29,30,31,28,32,33,999999,34,35,999999],[36,37,31,28,38,39,999999,40,41,999999]],[[42,29,43,44],[45,29,46,47],[48,29,49,50],[51,36,52,53]],[[19,[42,45,48]],[54,[51]]]]],[[[29,30,31,28,32,33,999999,34,35,999999],[55,56,31,28,32,57,58,3,59,60,61,999999,35,62,63],33,999999,[[43,44,31,28,64,65,[66,67,68,69,58,70,71,72]],[46,47,31,28,73,74,[66,67,68,75,58,76,77,78]],[49,50,31,28,64,79,[66,67,68,80,58,81,82,83]]]],[[36,37,31,28,38,39,999999,40,41,999999],[55,56,31,28,38,84,58,1,85,86,87,999999,41,88,89],39,999999,[[52,53,31,28,64,90,[66,67,68,91,58,92,93,94]]]]],[[[42,29,43,44],29,43,44],[[45,29,46,47],29,46,47],[[48,29,49,50],29,49,50],[[51,36,52,53],36,52,53]],[[19,[42,45,48]],[54,[51]]]]],"Approval":[1,95,1,1,2,96,97,98,99,100,101,102,[[103,104]],[[[20,21,22,23,24,25,999999,26]],[[27,20]],[28,2,[[29,30,31,28,32,33,999999,34,35,999999],[36,37,31,28,38,39,999999,40,41,999999]],[[42,29,43,44],[45,29,46,47],[48,29,49,50],[51,36,52,53]],[[19,[42,45,48]],[54,[51]]]]],[[[29,30,31,28,32,33,999999,34,35,999999],[[43,44,65,70,72,[true,false,[105]]],[46,47,74,76,78,[true,false,[105]]],[49,50,79,81,83,[true,false,[105]]]]],[[36,37,31,28,38,39,999999,40,41,999999],[[52,53,90,92,94,[true,false,[105]]]]]]],"Candidates":[[42,29,43,44,106,107,[66,67,68,69,58,70,71,72],108],[45,29,46,47,109,110,[66,67,68,75,58,76,77,78],111],[48,29,49,50,112,113,[66,67,68,80,58,81,82,83],114]],"ApprovalEvidenceSha256":"5230e392a20b34b31b3b2aaf1346ab55439368b921b887f4aa1b3b69f4d4d7ec","PolicyId":"S04-CODEX-TEACHING-SELECTION-v1","PolicyVersion":"ba967b55007606dcfed2dca4e8c5f856c2134170a020706ca8ff9b6d6f35062a","PolicyContentUtf8":"### 2.1 CODEX teaching-selection policy — S04-CODEX-TEACHING-SELECTION-v1\n\nThis procedural policy implements the owner's 2026-10-07 role instruction. It is not a claim that an earlier C# selector, fixed category ranking or runtime policy registry already existed. The human-role decision is performed by CODEX with `GPT-6-Astra` and reasoning mode `ultra`, which the owner explicitly identifies as `astra 6 ultracode`; the product validates and reuses its bound result. The policy name here is a document identifier, not a magic runtime permit. The model alias is already resolved: use the supported native model/mode controls, not a search for a separate preset or a new policy approval.\n\n1. Reuse a previously accepted, still-valid selection for this objective and identical input bindings. Do not rerun selection merely after a worker message, restart, review or commit. If relevant inputs changed, preserve the old evidence and perform the existing conflict/recovery handling before any new effects.\n2. Obtain the saved sanitized weakness categories through the genuine held Session. No raw closed task, paid exam answer, Student exam answer or certification payload may enter selection or authoring context. Baseline-sufficient and missing-usable-diagnosis remain their existing branches; do not invent a weakness.\n3. Open the complete declared teaching graph through its existing owner. Establish actual teaching-purpose and disclosure rights, novelty and required ORIGINAL/certification disjointness before a task is eligible. Descriptive PolicyId/PolicyVersion/order, a valid hash or Train partition alone establishes none of these rights. Preserve all supplied sources/items/order; do not silently truncate the catalog.\n4. CODEX judges which eligible task addresses the diagnosed weakness and explains the task-specific pedagogical fit using only permitted material. It selects ONE task, not a closed exam question renamed as teaching. Reuse an actually established approved policy when available; otherwise exercise the delegated choice here instead of requesting a category/item ranking from Luba. Do not impose an invented global ranking of categories or claim an alphabetical first item is a semantic decision. For otherwise equivalent eligible choices, preserve the existing supplied curriculum order as the stable tie-break.\n5. Before asking the Chinese author, freeze objective/grant, policy-document version, SOURCE/curriculum hash, sanitized categories, selected item/source/entry identities, material-admission evidence and the reason for selection. Persist and reopen through the existing session/selection ownership boundary. A work-record note is evidence, not a production bearer capability. If runtime consumption of this bound decision is absent, implement and test that narrow boundary; do not assert an existing API or ask Luba to name one.\n6. Build the complete typed teaching packet: full selected task; the matching saved separate-task weak Student response or the one already allowed isolated response; permitted teaching reference; prior accepted good/bad examples with their context-only IDs. Check the real configured input limit before a call; byte-size or JSON validity alone is not proof of tokenizer-level completeness. Do not replace the Student response with a routing candidate, omit mandatory history or cut the task to fit.\n\nReplay means reusing the saved decision, not assuming a generative model will independently make an identical choice twice. A synthetic fixture may provide a clearly labelled test operator decision to test its consumer; it must not be represented as a real Astra decision or policy admission.","Nonce":"11111111111111111111111111111111","PromptRecipeVersion":1,"RenderedPromptUtf8ByteLength":7858,"RenderedPromptSha256":"4015b63de2d80d23c118708069eeb580d1777961d1685d1523e877f57df34fa2","RequestedModel":"gpt-6-astra","RequestedReasoningEffort":"ultra","OutputSchemaCanonicalBase64":"eyJ0eXBlIjoib2JqZWN0IiwiYWRkaXRpb25hbFByb3BlcnRpZXMiOmZhbHNlLCJyZXF1aXJlZCI6WyJOb25jZSIsIlByZXBhcmVkUmVxdWVzdFNoYTI1NiIsIkl0ZW1JZCIsIlNvdXJjZUlkIiwiUmVmZXJlbmNlRW50cnlJZCIsIlNvdXJjZUVudGl0eUZpbmdlcnByaW50IiwiTWF0ZXJpYWxBZG1pc3Npb25FdmlkZW5jZVNoYTI1NiIsIlJlYXNvbiJdLCJwcm9wZXJ0aWVzIjp7Ik5vbmNlIjp7InR5cGUiOiJzdHJpbmciLCJwYXR0ZXJuIjoiXlswLTlhLWZdezMyfSQifSwiUHJlcGFyZWRSZXF1ZXN0U2hhMjU2Ijp7InR5cGUiOiJzdHJpbmciLCJwYXR0ZXJuIjoiXlswLTlhLWZdezY0fSQifSwiSXRlbUlkIjp7InR5cGUiOiJzdHJpbmciLCJtaW5MZW5ndGgiOjF9LCJTb3VyY2VJZCI6eyJ0eXBlIjoic3RyaW5nIiwibWluTGVuZ3RoIjoxfSwiUmVmZXJlbmNlRW50cnlJZCI6eyJ0eXBlIjoic3RyaW5nIiwibWluTGVuZ3RoIjoxfSwiU291cmNlRW50aXR5RmluZ2VycHJpbnQiOnsidHlwZSI6InN0cmluZyIsIm1pbkxlbmd0aCI6MX0sIk1hdGVyaWFsQWRtaXNzaW9uRXZpZGVuY2VTaGEyNTYiOnsidHlwZSI6InN0cmluZyIsInBhdHRlcm4iOiJeWzAtOWEtZl17NjR9JCJ9LCJSZWFzb24iOnsidHlwZSI6InN0cmluZyIsIm1pbkxlbmd0aCI6MSwibWF4TGVuZ3RoIjoxMDI0fX19"}
""";
        var compactBytes=Encoding.UTF8.GetBytes(compactLiteral); var compact=CodeDocumentationNativeCodexSelectionWire.DecodePrepared(compactBytes);
        Assert.True(compact.IsSuccess,compact.ErrorCode);Assert.Equal(2,compact.Value!.SchemaVersion);
        var raw=JsonNode.Parse(compactBytes)!;var atoms=raw["Atoms"]!.AsArray();
        var binding=NativeExpand(raw["Binding"]!,"Binding",atoms);var input=NativeExpand(raw["Input"]!,"Input",atoms);var approval=NativeExpand(raw["Approval"]!,"Approval",atoms);var candidates=NativeExpand(raw["Candidates"]!,"[Candidate]",atoms);
        Assert.Equal(compactBytes,NativeV2Oracle(compact.Value.SessionId,binding,input,approval,candidates,compact.Value.Nonce));
        Assert.Equal(NativeCanonical(binding),compact.Value.CopyBindingBytes());Assert.Equal(NativeCanonical(input),compact.Value.CopyInputBytes());
        Assert.Equal(NativePacketOracle(binding,input,candidates),compact.Value.CopyPacketBytes());Assert.True(compact.Value.MatchesApprovalBytes(NativeCanonical(approval)));
        Assert.True(compact.Value.MatchesPromptBytes(NativePromptOracle(input,candidates,compact.Value.Nonce)));
        Assert.Equal(1,CodeDocumentationNativeCodexSelectionWire.DecodePrepared(bytes).Value!.SchemaVersion);Assert.Equal(bytes,CodeDocumentationNativeCodexSelectionWire.Write(parsed.Value!));
        foreach(var attack in new[]{"negative-ref","outside-ref","fraction-ref","exponent-ref","duplicate-atom","unused-atom","first-use","arity","kind","depth","recipe","prompt-hash","prompt-length","overflow","version","preimage"})
        {
            var x=JsonNode.Parse(compactBytes)!;
            switch(attack){
                case "negative-ref":x["Binding"]![0]=-1;break;case "outside-ref":x["Binding"]![0]=25731;break;
                case "fraction-ref":x["Binding"]![0]=JsonNode.Parse("0.0");break;case "exponent-ref":x["Binding"]![0]=JsonNode.Parse("0e0");break;
                case "duplicate-atom":x["Atoms"]!.AsArray().Add(x["Atoms"]![0]!.DeepClone());break;case "unused-atom":x["Atoms"]!.AsArray().Add("unreferenced");break;
                case "first-use":x["Binding"]![0]=1;break;case "arity":x["Binding"]!.AsArray().RemoveAt(9);break;case "kind":x["Binding"]![0]=false;break;
                case "depth":x["Binding"]![0]=JsonNode.Parse("[[[[[[[[[0]]]]]]]]]");break;case "recipe":x["PromptRecipeVersion"]=2;break;
                case "prompt-hash":x["RenderedPromptSha256"]=new string('f',64);break;case "prompt-length":x["RenderedPromptUtf8ByteLength"]=1;break;
                case "overflow":x["RenderedPromptUtf8ByteLength"]=JsonNode.Parse("9223372036854775808");break;case "version":x["SchemaVersion"]=3;break;
                case "preimage":x["ApprovalEvidenceSha256"]=new string('f',64);break;}
            var invalid=Encoding.UTF8.GetBytes(x.ToJsonString(NativeJsonOptions));Assert.InRange(invalid.Length,1,25731);
            var refused=CodeDocumentationNativeCodexSelectionWire.DecodePrepared(invalid);Assert.False(refused.IsSuccess);Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,refused.ErrorCode);
        }
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,CodeDocumentationNativeCodexSelectionWire.DecodePrepared(compactBytes.Append((byte)' ').ToArray()).ErrorCode);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,CodeDocumentationNativeCodexSelectionWire.DecodePrepared(new byte[]{0xff}).ErrorCode);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,CodeDocumentationNativeCodexSelectionWire.DecodePrepared(new byte[25732]).ErrorCode);
        // Closed typed references amplify complete input before semantic joins; packet preflight refuses it.
        // This is malformed DATA, with no grant/receipt; existing atoms stay unique and first-use ordered.
        var amplified = JsonNode.Parse(compactBytes)!;
        var repeatedEntries = amplified["Input"]![5]![1]![0]![4]!.AsArray();
        var originalFirstEntry = repeatedEntries[0]!.DeepClone();
        for (var repeat = 0; repeat < 96; repeat++) repeatedEntries.Insert(1, originalFirstEntry.DeepClone());
        var amplifiedBytes = Encoding.UTF8.GetBytes(amplified.ToJsonString(NativeJsonOptions));
        Assert.InRange(amplifiedBytes.Length, 1, 25731);
        var amplifiedResult = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(amplifiedBytes);
        Assert.False(amplifiedResult.IsSuccess); Assert.Null(amplifiedResult.Value);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode, amplifiedResult.ErrorCode);
        output.WriteLine("NATIVE_V2_REFERENCE_AMPLIFICATION physicalBytes={0}; repeatedEntries=96; earlyPacketOverage=true; genuineAdmission=false; semanticMaxAttained=false", amplifiedBytes.Length);
        // Physically bounded references can expand strings quadratically in work, never byte-array allocation.
        Assert.Equal(662084361L,25731L*25731L);Assert.Equal(8,CodeDocumentationNativeCodexSelectionWire.ClosedMaximumDepth);
        Assert.False(compact.Value.MatchesApprovalBytes([]));Assert.False(compact.Value.MatchesPromptBytes([]));
        output.WriteLine("NATIVE_V2_DATA_ONLY bytes={0}; packet={1}; genuineAdmission=false; semanticMaxAttained=false",compactBytes.Length,compact.Value.CopyPacketBytes().Length);
        AssertNativeSealedOwnerWireData(binding, approval);
        AssertNativeSealedOwnerWireData(binding, approval, legacyFullMaterial: true);
        AssertNativeSealedOwnerWireData(binding, approval, protectedProcessStrings: true);
    }

    private void AssertNativeSealedOwnerWireData(JsonNode templateBinding, JsonNode templateApproval, bool legacyFullMaterial = false, bool protectedProcessStrings = false)
    {
        CodeDocumentationOneLessonSourceSelectionTests.WithNativePreparationSealedGraph((graph, corpora) =>
        {
            var captured = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(new string('b', 32), new string('c', 64),
                ["missing_essential_behavior"], graph);
            Assert.True(captured.IsSuccess, captured.ErrorCode);
            var input = JsonNode.Parse(captured.Value!.CopyBytes())!;
            var binding = templateBinding.DeepClone(); binding["DescriptorsV2"]!["SourceSelectionSha256"] = graph.Selection.SourceSelectionSha256;
            var approval = templateApproval.DeepClone(); approval["SourceSelection"] = input["TeachingGraph"]!["Selection"]!.DeepClone();
            var approvedSources = new JsonArray(); var candidates = new JsonArray();
            var materialCandidates = new List<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCandidate>();
            foreach (var source in graph.Sources)
            {
                var approvedEntries = new JsonArray();
                foreach (var entry in source.Entries)
                    approvedEntries.Add(new JsonObject {
                        ["EntryId"] = entry.EntryId, ["SourceEntityFingerprint"] = entry.SourceEntityFingerprint,
                        ["OwnerInputSha256"] = entry.OwnerInputSha256, ["ReferenceAnswerSha256"] = entry.PaidReferenceProvenance.ReferenceAnswerSha256,
                        ["PaidReferenceProvenanceSha256"] = entry.PaidReferenceProvenance.ProvenanceSha256,
                        ["Eligibility"] = new JsonObject { ["LicenseAccepted"] = true, ["IsQuarantined"] = false,
                            ["EvidenceRefs"] = new JsonArray("sha256:" + new string('e', 64)) } });
                var descriptor = input["TeachingGraph"]!["Sources"]!.AsArray().Single(s => s!["Descriptor"]!["SourceId"]!.GetValue<string>() == source.Descriptor.SourceId)!["Descriptor"]!;
                approvedSources.Add(new JsonObject { ["Descriptor"] = descriptor.DeepClone(), ["Entries"] = approvedEntries });
            }
            approval["AllTeachingSources"] = approvedSources;
            var eligible = graph.CategoryBindings.Single(c => c.Category == "missing_essential_behavior").OrderedItemIds;
            foreach (var item in graph.Items.Where(i => eligible.Contains(i.DeclaredItem.ItemId, StringComparer.Ordinal)))
            {
                var entry = corpora[item.DeclaredItem.SourceId].Entries.Single(e => e.EntryId == item.Entry.EntryId);
                var metadataEntry = input["TeachingGraph"]!["Sources"]!.AsArray().Single(s => s!["Descriptor"]!["SourceId"]!.GetValue<string>() == item.DeclaredItem.SourceId)!["Entries"]!.AsArray().Single(e => e!["EntryId"]!.GetValue<string>() == entry.EntryId)!;
                var candidate = new JsonObject { ["ItemId"] = item.DeclaredItem.ItemId, ["SourceId"] = item.DeclaredItem.SourceId,
                    ["ReferenceEntryId"] = entry.EntryId, ["SourceEntityFingerprint"] = entry.SourceEntityFingerprint,
                    ["OwnerInput"] = entry.OwnerInput, ["PaidModelReferenceAnswer"] = entry.PaidModelReferenceAnswer,
                    ["PaidReferenceProvenance"] = metadataEntry["PaidReferenceProvenance"]!.DeepClone() };
                using var bytes = new MemoryStream(); using var writer = new BinaryWriter(bytes, new UTF8Encoding(false, true), true);
                writer.Write(Encoding.UTF8.GetBytes("code-documentation-startup-teaching-disclosure-v1\n"));
                void Bytes(byte[] value) { writer.Write(value.Length); writer.Write(value); }
                void Field(string value) => Bytes(Encoding.UTF8.GetBytes(value));
                Bytes(NativeCanonical(approval)); Bytes(NativeCanonical(binding)); Bytes(captured.Value.CopyBytes());
                foreach (var name in new[] { "ItemId", "SourceId", "ReferenceEntryId", "SourceEntityFingerprint" }) Field(candidate[name]!.GetValue<string>());
                Field(FrozenReferenceEntry.ComputeNormalizedTextSha256(entry.OwnerInput)); Field(entry.PaidReferenceProvenance.CanonicalRepresentation); writer.Flush();
                var evidence = HBytes(bytes.ToArray()); candidate["MaterialAdmissionEvidenceSha256"] = evidence; candidates.Add(candidate);
                materialCandidates.Add(new(item.DeclaredItem.ItemId, item.DeclaredItem.SourceId, entry.EntryId, entry.SourceEntityFingerprint,
                    entry.OwnerInput, entry.PaidModelReferenceAnswer, entry.PaidReferenceProvenance, evidence));
            }
            var anticipated = NativeV2Oracle(new string('a', 64), binding, input, approval, candidates, new string('1', 32));
            var decoded = CodeDocumentationNativeCodexSelectionWire.DecodePrepared(anticipated);
            output.WriteLine("NATIVE_V2_SEALED_OWNER_DATA input={0}; packet={1}; request={2}; decodeSuccess={3}; code={4}; protectedAdmission=false",
                captured.Value.CopyBytes().Length, NativePacketOracle(binding, input, candidates).Length, anticipated.Length, decoded.IsSuccess, decoded.ErrorCode);
            // This private DATA construction is never submitted to a Session or receipt issuer. Its origin is deliberately untrusted.
            var constructor = typeof(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
            var ownerBindingHash = HBytes(Encoding.UTF8.GetBytes("code-documentation-one-lesson-binding-v2\n").Concat(NativeCanonical(binding)).ToArray());
            var projection = binding.Deserialize<CodeDocumentationOneLessonBindingProjectionV2>()!;
            var ownerBinding = new CodeDocumentationOneLessonSessionBinding(projection.ObjectiveId, projection.GrantId, projection.GrantVersion,
                projection.TrainingRunId, projection.CandidateSha256, projection.ParentManifestSha256, projection.ParentWeightsSha256,
                projection.ConfigurationSha256, projection.ExamConditionSha256) { FormatVersion = 2, DescriptorsV2 = projection.DescriptorsV2 };
            Assert.Equal(ownerBindingHash, CodeDocumentationOneLessonSession.ComputeBindingSha256(ownerBinding));
            var context = (CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext)constructor.Invoke([
                ownerBindingHash, NativeCanonical(binding), captured.Value, materialCandidates, "WireDataOnly"]);
            Assert.Equal("WireDataOnly", context.Origin); Assert.False(context.FullTeacherContextReady);
            if (protectedProcessStrings)
            {
                Assert.Equal(3, graph.Sources.Sum(s => s.Entries.Count)); Assert.Equal(2, context.Candidates.Count);
                Assert.Equal(2, graph.Sources.Count); Assert.Equal(3, graph.Items.Count);
                Assert.DoesNotContain(context.Candidates, c => c.SourceId == "b");
                var previousCompleteCells = new Dictionary<string, string>(StringComparer.Ordinal) {
                    ["Полная задача 中文\nsecond line\toriginal-exclusion-7-0"] = "Полный reference 中文\nfinal line\toriginal-exclusion-7-0",
                    ["Полная задача 中文\nsecond line\toriginal-exclusion-7-1"] = "Полный reference 中文\nfinal line\toriginal-exclusion-7-1" };
                Assert.Equal(previousCompleteCells.Keys.Order(StringComparer.Ordinal), context.Candidates.Select(c => c.OwnerInput).Order(StringComparer.Ordinal));
                foreach (var cell in context.Candidates) Assert.Equal(previousCompleteCells[cell.OwnerInput], cell.PaidModelReferenceAnswer);
                output.WriteLine("NATIVE_V2_PROTECTED_HTTP_FIXTURE_DATA entries=3; candidates=2; unusedSourceEntries=1; packet={0}; request={1}; fullRussianStrings=true; protectedHttpNotRun=true",
                    context.CopyCanonicalBytes().Length, anticipated.Length);
            }
            Assert.Equal(NativePacketOracle(binding, input, candidates), context.CopyCanonicalBytes());
            Assert.False(context.CopyCanonicalBytes().AsSpan().SequenceEqual(NativePacketOracle(binding, input, candidates, plainBindingHash: true)));
            output.WriteLine("NATIVE_V2_OWNER_BINDING_DOMAIN matchesSessionOwner=true; plainHashEqualsOwner={0}; plainPacketEqualsOwner={1}; origin=WireDataOnly",
                ownerBindingHash == HBytes(NativeCanonical(binding)), context.CopyCanonicalBytes().AsSpan().SequenceEqual(NativePacketOracle(binding, input, candidates, plainBindingHash: true)));
            if (legacyFullMaterial)
            {
                Assert.Equal(8, graph.Sources.Sum(s => s.Entries.Count)); Assert.Equal(4, context.Candidates.Count);
                Assert.True(context.CopyCanonicalBytes().Length > 16384);
                Assert.False(decoded.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode, decoded.ErrorCode);
                Assert.Throws<CodeDocumentationNativeCodexSelectionWire.PreparationCapacityException>(() => CodeDocumentationNativeCodexSelectionWire.ProjectV2(
                    new string('a', 64), context.CopyBindingBytes(), captured.Value.CopyBytes(), NativeCanonical(approval), HBytes(NativeCanonical(approval)), context, new string('1', 32)));
                output.WriteLine("NATIVE_V2_LEGACY_HTTP_FIXTURE_DATA entries=8; candidates=4; firstHostDimension=packet; packet={0}; packetLimit=16384; request={1}; protectedHttpNotRun=true",
                    context.CopyCanonicalBytes().Length, anticipated.Length);
                return;
            }
            Assert.True(decoded.IsSuccess, "Real sealed-owner canonical DATA decode: " + decoded.ErrorCode);
            Assert.Equal(captured.Value.CopyBytes(), decoded.Value!.CopyInputBytes());
            Assert.Equal(NativeCanonical(binding), decoded.Value.CopyBindingBytes());
            Assert.Equal(NativePacketOracle(binding, input, candidates), decoded.Value.CopyPacketBytes());
            Assert.True(decoded.Value.MatchesApprovalBytes(NativeCanonical(approval)));
            Assert.True(decoded.Value.MatchesPromptBytes(NativePromptOracle(input, candidates, new string('1', 32))));
            var plainContext = (CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext)constructor.Invoke([
                HBytes(NativeCanonical(binding)), NativeCanonical(binding), captured.Value, materialCandidates, "WireDataOnly"]);
            var plainRefusal = Assert.Throws<JsonException>(() => CodeDocumentationNativeCodexSelectionWire.ProjectV2(new string('a', 64),
                plainContext.CopyBindingBytes(), captured.Value.CopyBytes(), NativeCanonical(approval), HBytes(NativeCanonical(approval)), plainContext, new string('1', 32)));
            Assert.Equal("Complete owner preimages differ.", plainRefusal.Message);
            try
            {
                var projected = CodeDocumentationNativeCodexSelectionWire.ProjectV2(new string('a', 64), context.CopyBindingBytes(), captured.Value.CopyBytes(),
                    NativeCanonical(approval), HBytes(NativeCanonical(approval)), context, new string('1', 32));
                var actual = CodeDocumentationNativeCodexSelectionWire.WriteV2(projected);
                output.WriteLine("NATIVE_V2_SEALED_OWNER_PROJECT request={0}; matchesIndependent={1}; origin=WireDataOnly; protectedAdmission=false", actual.Length, actual.AsSpan().SequenceEqual(anticipated));
                Assert.Equal(anticipated, actual);
            }
            catch (Exception exception)
            {
                output.WriteLine("NATIVE_V2_SEALED_OWNER_PROJECT_FAILURE type={0}; origin=WireDataOnly; protectedAdmission=false", exception.GetType().Name);
                foreach (var frame in new System.Diagnostics.StackTrace(exception, true).GetFrames().Take(4))
                    output.WriteLine("NATIVE_V2_SEALED_PROJECT_FRAME file={0}; line={1}; method={2}", Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber(), frame.GetMethod()?.Name);
                throw;
            }
        }, legacyFullMaterial, protectedProcessStrings);
    }

    private static readonly string[] DisclosureFinalPoints = ["baseline-reference-final-none", "receiving-final-none", "original-before-final-none", "source-reopen-final-none", "original-after-final-none", "teaching-disclosure-revision-final-none"];
    private static void AssertDisclosureReleased(CodeDocumentationOneLessonSession session)
    {
        Assert.Equal(0, FreshSessionField(session,"_selectionMaterialInFlight"));
        foreach (var field in new[] { "_teachingDisclosureStartup", "_teachingDisclosureApproval", "_teachingDisclosureRevisionInput", "_teachingDisclosureGraph", "_teachingDisclosureInput", "_teachingDisclosureBinding", "_teachingDisclosureReceipt", "_selectionMaterialReceipt", "_selectionMaterialPrepared", "_selectionMaterialFinalSnapshot" }) Assert.Null(FreshSessionField(session,field)); // Release the check-only input along with every authority-bearing transient field.
        Assert.Equal(false, FreshSessionField(session,"_teachingDisclosureIssuing"));
    }
    [Fact]
    public async Task StartupApprovedTeachingDisclosure_FullFixedRecipientAndSyntheticProviderSeparation()
    {
        foreach (var rawUtf8References in new[] { false, true })
        {
            using var p = new ProtectedDisclosureHarness(rawUtf8References: rawUtf8References); var h = p.H; var b = (await h.Consume()).Value!; Assert.NotNull(b);
            var files = h.Files(); var calls = h.Client.Calls; var captures = 0; var syntheticCalls = 0;
            var old = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
            CodeDocumentationOneLessonSession.PreparedTeachingDisclosureReceipt? disclosure = null;
            var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialInspectionForTesting((context,_) => { syntheticCalls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[0]))); });
            var finals = new List<string>();
            try
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = context =>
                {
                    captures++; Assert.Equal("ProtectedStartupOperatorApproval",context.Origin); Assert.False(context.FullTeacherContextReady); Assert.Equal(2,context.Candidates.Count);
                    foreach (var c in context.Candidates)
                    {
                        var descriptor = h.Grant.SourceSelection!.TeachingCurriculum!.Sources.Single(s=>s.SourceId==c.SourceId);
                        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,descriptor.DomainSlug,descriptor.SubjectSlug,descriptor.CorpusVersion).Value!;
                        var entry = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address).Value!.Corpus.Entries.Single(e=>e.EntryId==c.ReferenceEntryId);
                        Assert.Equal(entry.OwnerInput,c.OwnerInput); Assert.Equal(entry.PaidModelReferenceAnswer,c.PaidModelReferenceAnswer); Assert.Equal(entry.PaidReferenceProvenance.CanonicalRepresentation,c.PaidReferenceProvenance.CanonicalRepresentation);
                        if (rawUtf8References)
                        {
                            Assert.Equal(string.Empty,entry.PaidReferenceProvenance.ReferenceAnswerSha256);
                            Assert.Equal(FrozenReferenceCanonicalization.RawUtf8Sha256Ref(c.PaidModelReferenceAnswer),c.PaidReferenceProvenance.ReferenceAnswerRawUtf8Sha256);
                            Assert.NotEqual(FrozenReferenceEntry.ComputeNormalizedTextSha256(c.PaidModelReferenceAnswer),c.PaidReferenceProvenance.ReferenceAnswerRawUtf8Sha256);
                            Assert.Contains("\r\nCafe\u0301\tEND",c.PaidModelReferenceAnswer);
                            Assert.Equal(Encoding.UTF8.GetBytes(entry.PaidModelReferenceAnswer),Encoding.UTF8.GetBytes(c.PaidModelReferenceAnswer));
                        }
                        Assert.Contains(c.OwnerInput,Encoding.UTF8.GetString(context.CopyCanonicalBytes())); Assert.Contains(c.PaidModelReferenceAnswer,Encoding.UTF8.GetString(context.CopyCanonicalBytes()));
                    }
                    disclosure = (CodeDocumentationOneLessonSession.PreparedTeachingDisclosureReceipt)FreshSessionField(h.Open.Session,"_teachingDisclosureReceipt")!;
                    Assert.True(disclosure.IsCurrentFor(h.Open.Session));
                    Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,p.Inspect(b).GetAwaiter().GetResult().ErrorCode);
                };
                h.Open.Session.SelectionMaterialPointForTesting = point =>
                {
                    if (DisclosureFinalPoints.Contains(point)) finals.Add(point);
                    if (point != "teaching-disclosure-inspection-result-staged") return;
                    var receipt = (CodeDocumentationOneLessonSession.PreparedSelectionMaterialReceipt)FreshSessionField(h.Open.Session,"_selectionMaterialReceipt")!;
                    var rejected = provider.InspectPreparedMaterialAsync(h.Open.Session,receipt,CancellationToken.None).GetAwaiter().GetResult();
                    Assert.False(rejected.IsSuccess); Assert.Equal(CodeDocumentationOneLessonV2Wire.SelectionInvalid<bool>().ErrorCode,rejected.ErrorCode);
                };
                var inspected = await p.Inspect(b); Assert.True(inspected.IsSuccess,inspected.ErrorCode); Assert.NotNull(inspected.Value);
                Assert.Equal(2,inspected.Value!.CandidateCount); Assert.Equal(64,inspected.Value.PacketSha256.Length); Assert.Equal(64,inspected.Value.ApprovalEvidenceSha256.Length);
                Assert.Equal(1,captures); Assert.Equal(0,syntheticCalls); Assert.Equal(calls,h.Client.Calls);
                foreach(var point in DisclosureFinalPoints) Assert.Equal(2,finals.Count(x=>x==point));
                Assert.NotNull(disclosure); Assert.False(disclosure!.IsCurrentFor(h.Open.Session)); AssertDisclosureReleased(h.Open.Session);
                h.AssertFiles(files); AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); p.Environment.AssertStillConfigured();
            }
            finally { h.Open.Session.SelectionMaterialPointForTesting=null; CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=old; }
        }
        using (var denied = new ProtectedDisclosureHarness(rawUtf8References: true, alter: json =>
        {
            var root = JsonNode.Parse(json)!; var approval = root["TeachingDisclosureApprovals"]![0]!;
            approval["SchemaVersion"] = 1;
            foreach (var source in approval["AllTeachingSources"]!.AsArray())
                foreach (var entry in source!["Entries"]!.AsArray())
                {
                    var rawHash = entry!["ReferenceAnswerRawUtf8Sha256"]!.GetValue<string>();
                    entry.AsObject().Remove("ReferenceAnswerRawUtf8Sha256"); entry["ReferenceAnswerSha256"] = rawHash;
                }
            return DisclosureJcs(root);
        }))
        {
            var h = denied.H; var b = (await h.Consume()).Value!; Assert.NotNull(b); var files = h.Files(); var calls = h.Client.Calls; var captures = 0;
            var old = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
            try
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++;
                var refused = await denied.Inspect(b);
                Assert.False(refused.IsSuccess); Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Invalid,refused.ErrorCode);
                Assert.Equal(0,captures); Assert.Equal(calls,h.Client.Calls); AssertDisclosureReleased(h.Open.Session);
                h.AssertFiles(files); AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); denied.Environment.AssertStillConfigured();
            }
            finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=old; }
        }
        await AssertProtectedOriginRejectedBeforeSyntheticDurableInvocation();
    }

    private static async Task AssertProtectedOriginRejectedBeforeSyntheticDurableInvocation()
    {
        using var h = FullMaterialHarness(); var b = (await h.Consume()).Value!; var admissions = SelectionMaterialAdmissions(h,b); var callbacks = 0;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) => { callbacks++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[0]))); });
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext? context = null;
        var previous = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting; var observedUnconsumed = false;
        var origin = typeof(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PreparedSelectionMaterialContext).GetField("<Origin>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic)!;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = c => context = c;
            h.Open.Session.SelectionMaterialPointForTesting = point => {
                if(point != "final-none-checked" || h.HistoryBytes().Length == 0) return;
                Assert.NotNull(context);
                // Adversarial test-only field mutation on a genuine Synthetic dispatch, never an authority constructor.
                origin.SetValue(context,"ProtectedStartupOperatorApproval");
                if(h.Open.Session.FullMaterialDispatchForTesting is { } receipt) {
                    Assert.Equal(0,typeof(CodeDocumentationOneLessonSession.MaterialSelectionDispatchReceipt).GetField("invocationConsumed",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(receipt));
                    observedUnconsumed = true;
                }
            };
            var result = await BindFullMaterial(h,b,admissions,provider);
            Assert.Equal(CodeDocumentationOneLessonV2Wire.SelectionInvalid<bool>().ErrorCode,result.ErrorCode); Assert.False(result.IsSuccess);
            Assert.Equal(0,callbacks); Assert.True(observedUnconsumed); Assert.Equal(2,FullMaterialLines(h.HistoryBytes()).Length); AssertFullMaterialNoEffects(h);
        }
        finally { h.Open.Session.SelectionMaterialPointForTesting=null; if(context is not null) origin.SetValue(context,"SyntheticTest"); CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=previous; }
    }

    [Fact]
    public async Task StartupApprovedTeachingDisclosure_CallbackAndUnusedApprovalMismatchRefuseBeforeCapture()
    {
        foreach (var scenario in new[] { "callback", "unused", "alias" })
        {
            using var p = new ProtectedDisclosureHarness(callback:scenario=="callback", alter:scenario!="callback" ? json => {
                var tree=JsonNode.Parse(json)!; var entries=tree["TeachingDisclosureApprovals"]![0]!["AllTeachingSources"]![0]!["Entries"]!.AsArray();
                if(scenario=="unused")entries[^1]!["OwnerInputSha256"]="sha256:"+new string('a',64);
                else tree["TeachingDisclosureApprovals"]![0]!["ApproverId"]="sha256:"+tree["Grants"]![0]!["DescriptorsV2"]!["Corpus"]!["CorpusSha256"]!.GetValue<string>();
                return DisclosureJcs(tree);
            } : null);
            var h=p.H; var b=(await h.Consume()).Value!; var files=h.Files(); var count=0; var raw=0; var old=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;var oldRead=CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
            try { CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting=_=>raw++;CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=_=>count++; var result=await p.Inspect(b); Assert.False(result.IsSuccess); Assert.Equal(scenario=="callback"?CodeDocumentationOneLessonTeachingDisclosureApproval.Unavailable:scenario=="unused"?CodeDocumentationOneLessonTeachingDisclosureApproval.Invalid:"ONE_LESSON_TEACHING_ORIGINAL_OVERLAP",result.ErrorCode); Assert.Equal(0,count);Assert.Equal(0,raw); AssertDisclosureReleased(h.Open.Session); h.AssertFiles(files); AssertFullMaterialNoEffects(h); }
            finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=old;CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting=oldRead; }
        }
    }
    private static string DisclosureJcs(JsonNode node) => node switch {
        JsonObject o=>"{"+string.Join(",",o.OrderBy(p=>p.Key,StringComparer.Ordinal).Select(p=>JsonSerializer.Serialize(p.Key)+":"+DisclosureJcs(p.Value!)))+"}",
        JsonArray a=>"["+string.Join(",",a.Select(n=>DisclosureJcs(n!)))+"]", _=>node.ToJsonString(new JsonSerializerOptions{Encoder=System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}) };

    [Fact]
    public async Task StartupApprovedTeachingDisclosure_BusinessRefusalYieldsToRevisionFinal()
        => await DisclosureBusinessFinal(sourceFault:false);
    [Fact]
    public async Task StartupApprovedTeachingDisclosure_BusinessRefusalYieldsToSourceFinal()
        => await DisclosureBusinessFinal(sourceFault:true);
    private static async Task DisclosureBusinessFinal(bool sourceFault)
    {
        using var p=new ProtectedDisclosureHarness(); var h=p.H; var b=(await h.Consume()).Value!; var files=h.Files(); var finals=new List<string>(); var captures=0;
        var d=h.Grant.SourceSelection!.TeachingCurriculum!.Sources[0]; var address=RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,d.DomainSlug,d.SubjectSlug,d.CorpusVersion).Value!;
        var path=Path.Combine(address.CorpusRoot,RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName); var bytes=File.ReadAllBytes(path); var tick=File.GetLastWriteTimeUtc(path); Exception? failure=null;
        var old=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        try
        {
            h.Open.Session.TeachingDisclosureInspectionRefusalForTesting=true; CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=_=>captures++;
            h.Open.Session.SelectionMaterialPointForTesting=point=> {
                if(DisclosureFinalPoints.Contains(point)) finals.Add(point);
                if(point!="teaching-disclosure-inspection-result-staged") return;
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode,h.Open.Session.TeachingDisclosureStagedErrorForTesting);
                foreach(var f in DisclosureFinalPoints) Assert.Equal(1,finals.Count(x=>x==f));
                System.Environment.SetEnvironmentVariable(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable,p.Environment.Json.Replace("\"ApprovalRevision\":1","\"ApprovalRevision\":2",StringComparison.Ordinal));
                if(sourceFault) RestoreReferenceOwnerFile(path,bytes.Append((byte)' ').ToArray(),tick);
            };
            var result=await p.Inspect(b); Assert.False(result.IsSuccess); Assert.Null(result.Value);
            Assert.Equal(sourceFault?RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode:CodeDocumentationOneLessonTeachingDisclosureApproval.Stale,result.ErrorCode);
            Assert.Equal(CodeDocumentationOneLessonTeachingDisclosureApproval.Stale,h.Open.Session.TeachingDisclosureRevisionFinalErrorForTesting);
            foreach(var f in DisclosureFinalPoints) Assert.Equal(2,finals.Count(x=>x==f)); Assert.Equal(1,captures); AssertDisclosureReleased(h.Open.Session);
            if(sourceFault) { Assert.Equal(bytes.Append((byte)' ').ToArray(),File.ReadAllBytes(path)); Assert.Equal(tick,File.GetLastWriteTimeUtc(path)); }
            AssertFullMaterialNoEffects(h); Assert.False(File.Exists(h.Fixture.OrdinaryHistory));
        }
        catch(Exception ex) { failure=ex; throw; }
        finally {
            h.Open.Session.SelectionMaterialPointForTesting=null; h.Open.Session.TeachingDisclosureInspectionRefusalForTesting=false; CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=old;
            RestoreFixtureAfterFault(()=>{ RestoreReferenceOwnerFile(path,bytes,tick); System.Environment.SetEnvironmentVariable(CodeDocumentationOneLessonGrantCatalog.EnvironmentVariable,p.Environment.Json); },failure);
        }
        h.AssertFiles(files); p.Environment.AssertStillConfigured();
    }

    [Fact]
    public async Task StartupApprovedTeachingDisclosure_CancellationFinalExceptionsAndOwnerMutationRelease()
    {
        foreach(var scenario in new[] {"cancellation","baseline","original","revision-exception","reopen","stop"})
        {
            using var p=new ProtectedDisclosureHarness(); var h=p.H; var b=(await h.Consume()).Value!; var files=h.Files(); var finals=new List<string>();var session=h.Open.Session;
            using var cancellation=new CancellationTokenSource(); var primary=new OperationCanceledException("protected primary",null,cancellation.Token); var secondary=new IOException("protected revision final sentinel");
            var old=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting; var baselineBytes=File.ReadAllBytes(h.Fixture.OrdinaryBaseline); var baselineTick=File.GetLastWriteTimeUtc(h.Fixture.OrdinaryBaseline);
            var originalPath=Path.Combine(h.Fixture.CorpusAddress.CorpusRoot,RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName); var originalBytes=File.ReadAllBytes(originalPath); var originalTick=File.GetLastWriteTimeUtc(originalPath); Exception? failure=null;
            try
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=_=> {
                    if(scenario=="cancellation" || scenario=="revision-exception") { cancellation.Cancel(); throw primary; }
                    if(scenario=="baseline") RestoreCustodyFile(h.Fixture.OrdinaryBaseline,baselineBytes.Append((byte)' ').ToArray(),baselineTick);
                    if(scenario=="original") RestoreReferenceOwnerFile(originalPath,originalBytes.Append((byte)' ').ToArray(),originalTick);
                    if(scenario=="reopen") { var reopened=h.Reopen(); Assert.True(reopened.IsSuccess,reopened.ErrorCode); }
                    if(scenario=="stop") p.Startup.RevokeProtectedTeachingDisclosureApprovals();
                };
                h.Open.Session.SelectionMaterialPointForTesting=point=> { if(DisclosureFinalPoints.Contains(point)) finals.Add(point); if(scenario=="revision-exception" && point=="teaching-disclosure-revision-final-none") throw secondary; };
                if(scenario=="cancellation" || scenario=="revision-exception") { var actual=await Assert.ThrowsAsync<OperationCanceledException>(()=>p.Inspect(b,cancellation.Token)); Assert.Same(primary,actual); Assert.Equal(cancellation.Token,actual.CancellationToken); if(scenario=="revision-exception") Assert.Same(secondary,actual.Data["SelectionMaterialTeachingDisclosureRevisionFinalFailure"]); }
                else { var result=await p.Inspect(b); Assert.False(result.IsSuccess); Assert.Null(result.Value);
                    Assert.Equal(scenario switch { "baseline" => CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, "original" => RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, "stop" => CodeDocumentationOneLessonTeachingDisclosureApproval.Stale, "reopen" => CodeDocumentationOneLessonSession.CapacityInvalidErrorCode, _ => throw new ArgumentOutOfRangeException(nameof(scenario)) },result.ErrorCode); }
                // Throwing capture reaches one final sequence; returned contexts, including disposed-owner reopen, reach precheck and unconditional finalization.
                var expectedFinalSequences = scenario is "cancellation" or "revision-exception" ? 1 : 2;
                Assert.Equal(Enumerable.Range(0,expectedFinalSequences).SelectMany(_=>DisclosureFinalPoints),finals);
                foreach(var f in DisclosureFinalPoints) Assert.Equal(expectedFinalSequences,finals.Count(x=>x==f));
                if(scenario=="reopen") Assert.NotSame(session,h.Open.Session); // The disposed issuing owner and reopened owner must remain distinct and both release below.
                AssertDisclosureReleased(session); AssertDisclosureReleased(h.Open.Session); AssertFullMaterialNoEffects(h);
            }
            catch(Exception ex) { failure=ex; throw; }
            finally { session.SelectionMaterialPointForTesting=null;h.Open.Session.SelectionMaterialPointForTesting=null; CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting=old; RestoreFixtureAfterFault(()=>{RestoreCustodyFile(h.Fixture.OrdinaryBaseline,baselineBytes,baselineTick);RestoreReferenceOwnerFile(originalPath,originalBytes,originalTick);},failure); }
            h.AssertFiles(files);
        }
    }
    [Fact]
    public void CustodyStorage_ProducerCreatesPrivateJournalAndFreshReopenPreservesLegacyAncestor()
    {
        using var f = new JournalStorageFixture();
        var ancestorMode = JournalStorageMode(f.ModelVersionRoot);
        InvokeJournalStorage("EnsureJournalDirectories", f.Address, true);
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(f.JournalParent));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(f.Address.BaselineRoot));
        using (var ownerLock = (FileStream)InvokeJournalStorage("OpenOwnerLock", f.Address.LockPath, true)!)
        {
            ownerLock.Write("storage-lock"u8);
            ownerLock.Flush(true);
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(f.Address.LockPath));
        }
        foreach (var path in new[] { f.Address.BaselinePath, f.Address.ClaimPath })
        {
            InvokeJournalStorage("CreateNew", path, "storage-only-regression"u8.ToArray());
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path));
            Assert.Equal("storage-only-regression"u8.ToArray(), (byte[])InvokeJournalStorage("Read", path, 256)!);
        }
        var storage = f.Snapshot();
        InvokeJournalStorage("EnsureJournalDirectories", f.Address, false);
        using (var receiverLock = (FileStream)InvokeJournalStorage("OpenOwnerLock", f.Address.LockPath, false)!)
            Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(receiverLock.SafeFileHandle, f.Address.LockPath));
        Assert.Equal(storage, f.Snapshot());
        Assert.Equal(ancestorMode, JournalStorageMode(f.ModelVersionRoot));
        output.WriteLine("NATIVE_JOURNAL_MODULES " + JsonSerializer.Serialize(new
        {
            platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            loaded = new[] { typeof(CodeDocumentationRepositoryOwnedPreparedBaselineJournal).Assembly, GetType().Assembly }
                .Select(assembly => new { name = assembly.GetName().Name, path = assembly.Location,
                    mvid = assembly.ManifestModule.ModuleVersionId, sha256 = Hash(File.ReadAllBytes(assembly.Location)) }).ToArray(),
        }));
        output.WriteLine("NATIVE_JOURNAL_CUSTODY storage boundary only: protected journal directories/files, existing-only lock reopen, ancestor unchanged; no model/tokenizer/evaluator path");
    }

    [Theory]
    [InlineData("missing-history", false)]
    [InlineData("missing-ancestor", false)]
    [InlineData("missing-ancestor", true)]
    [InlineData("foreign-parent", false)]
    [InlineData("foreign-parent", true)]
    [InlineData("foreign-objective", false)]
    [InlineData("foreign-objective", true)]
    public void CustodyStorage_AbsentOrUntrustedDirectoriesRefuseWithoutCreationOrPermissionRepair(string state, bool producer)
    {
        using var f = new JournalStorageFixture(createAncestor: state != "missing-ancestor");
        if (state == "foreign-parent")
            Directory.CreateDirectory(f.JournalParent);
        if (state == "foreign-objective")
        {
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(f.JournalParent);
            Directory.CreateDirectory(f.Address.BaselineRoot);
        }
        if (state.StartsWith("foreign", StringComparison.Ordinal))
            Assert.False(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(state == "foreign-parent" ? f.JournalParent : f.Address.BaselineRoot));
        var before = f.Snapshot();
        Assert.Throws<InvalidDataException>(() => InvokeJournalStorage("EnsureJournalDirectories", f.Address, producer));
        Assert.Equal(before, f.Snapshot());
        Assert.False(File.Exists(f.Address.LockPath));
        Assert.False(File.Exists(f.Address.ClaimPath));
        Assert.False(File.Exists(f.Address.BaselinePath));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("foreign")]
    [InlineData("held")]
    public void CustodyStorage_ExistingOnlyLockRefusesMissingForeignAndHeldFilesWithoutRepair(string state)
    {
        using var f = new JournalStorageFixture();
        InvokeJournalStorage("EnsureJournalDirectories", f.Address, true);
        FileStream? held = null;
        if (state == "foreign")
        {
            File.WriteAllText(f.Address.LockPath, "foreign lock");
            Assert.False(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(f.Address.LockPath));
        }
        if (state == "held") held = (FileStream)InvokeJournalStorage("OpenOwnerLock", f.Address.LockPath, true)!;
        try
        {
            var mode = File.Exists(f.Address.LockPath) ? JournalStorageMode(f.Address.LockPath, directory: false) : null;
            var bytes = state == "foreign" ? File.ReadAllBytes(f.Address.LockPath) : null;
            var refusal = Record.Exception(() => InvokeJournalStorage("OpenOwnerLock", f.Address.LockPath, false));
            if (state == "foreign")
                Assert.IsType<UnauthorizedAccessException>(refusal);
            else
            {
                var io = Assert.IsAssignableFrom<IOException>(refusal);
                var nativeCode = io.HResult & 0xffff;
                if (state == "missing") Assert.True(io is FileNotFoundException || nativeCode == 2, io.ToString());
                else Assert.True(OperatingSystem.IsWindows() ? nativeCode is 32 or 33 : nativeCode is 11 or 13, io.ToString());
            }
            Assert.Equal(state != "missing", File.Exists(f.Address.LockPath));
            if (mode is not null) Assert.Equal(mode, JournalStorageMode(f.Address.LockPath, directory: false));
            if (bytes is not null) Assert.Equal(bytes, File.ReadAllBytes(f.Address.LockPath));
        }
        finally { held?.Dispose(); }
    }

    private static object? InvokeJournalStorage(string name, params object[] args)
    {
        var method = typeof(CodeDocumentationRepositoryOwnedPreparedBaselineJournal).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!;
        try { return method.Invoke(null, args); }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        { System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException).Throw(); throw; }
    }

    private static string JournalStorageMode(string path, bool directory = true)
        => OperatingSystem.IsWindows()
            ? (directory ? new DirectoryInfo(path).GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.All)
                : new FileInfo(path).GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.All))
            : File.GetUnixFileMode(path).ToString();

    private sealed class JournalStorageFixture : IDisposable
    {
        private readonly string _root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "prepared-baseline-storage-" + Guid.NewGuid().ToString("N")));
        internal JournalStorageFixture(bool createAncestor = true)
        {
            Directory.CreateDirectory(_root);
            File.WriteAllText(Path.Combine(_root, "LlmFromScratch.sln"), "storage-only fixture");
            Directory.CreateDirectory(Path.Combine(_root, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(_root, "11.DPO"));
            var address = RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(_root, "csharp", "code-documentation", "storage-model", "v1", H("storage-objective"));
            Assert.True(address.IsSuccess, address.ErrorCode);
            Address = address.Value!;
            if (createAncestor) Directory.CreateDirectory(ModelVersionRoot);
        }
        internal RepositoryOwnedPreparedBaselineAddress Address { get; }
        internal string JournalParent => Path.GetDirectoryName(Address.BaselineRoot)!;
        internal string ModelVersionRoot => Path.GetDirectoryName(JournalParent)!;
        internal string[] Snapshot() => Directory.GetDirectories(_root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(_root, path) + ":directory:" + JournalStorageMode(path))
            .Concat(Directory.GetFiles(_root, "*", SearchOption.AllDirectories).Select(path => Path.GetRelativePath(_root, path) + ":file:" + JournalStorageMode(path, false) + ":" + Hash(File.ReadAllBytes(path))))
            .Order(StringComparer.Ordinal).ToArray();
        public void Dispose()
        {
            var temp = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())) + Path.DirectorySeparatorChar;
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!_root.StartsWith(temp, comparison) || Path.GetFullPath(_root) != _root || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(_root))
                throw new IOException("Storage-only fixture cleanup escaped its exact native temporary root.");
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public async Task OrdinaryDiagnostic_GenuineProducerReceiver_AllClosedCodesDetachedAndRepeatHasNoEffects()
    {
        foreach (var code in new[] {
            CodeDocumentationSavedModelAssessmentService.MissingEssentialBehaviorReasonCode,
            CodeDocumentationSavedModelAssessmentService.InventedBehaviorReasonCode,
            CodeDocumentationSavedModelAssessmentService.WrongPurposeOrResultReasonCode,
            CodeDocumentationSavedModelAssessmentService.UnclearOrUnverifiableReasonCode })
        {
        using var h = new OrdinaryDiagnosticHarness((_, _) => (false, code));
        var original = await h.Consume();
        Assert.True(original.IsSuccess, original.ErrorCode + ":" + original.ErrorMessage);
        var baseline = original.Value!;
        var receivingOwner = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        var reopened = await receivingOwner.OpenExistingByExternalReferenceAsync(baseline.ExternalReference,
            h.Catalog, h.Fixture.OrdinarySelector, h.Start.RepositoryRoot, h.Start.DomainSlug, h.Start.DesignPatternSlug,
            h.Start.ModelIdSlug, h.Start.ModelVersion, h.Start.ModelConfig, h.Start.RuntimeCompatibility,
            baseline.Context, h.Start.LoraConfig, h.Start.AdapterSeedAuthority);
        Assert.True(reopened.IsSuccess, reopened.ErrorCode);
        Assert.True(receivingOwner.IsIssuedReadback(reopened.Value));
        Assert.False(receivingOwner.IsIssuedReadback(baseline.Readback));
        var files = h.Files();
        var graph = JsonSerializer.SerializeToUtf8Bytes(baseline.Readback.Baseline);
        var effects = h.Open.Session.Snapshot.Effects;
        var calls = h.Client.Calls;
        var first = h.Classify(baseline);
        var second = h.Classify(baseline);
        Assert.True(first.IsSuccess, first.ErrorCode);
        Assert.True(second.IsSuccess, second.ErrorCode);
        Assert.Equal(new[] { code }, first.Value!.Categories);
        Assert.Equal(first.Value.Categories, second.Value!.Categories);
        Assert.Equal(baseline.ExternalReference.ReceiptId, first.Value.OriginalReceiptId);
        Assert.Equal(baseline.ExternalReference.ReceiptSha256, first.Value.OriginalReceiptSha256);
        Assert.Equal(first.Value.OriginalReceiptSha256, second.Value.OriginalReceiptSha256);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)first.Value.Categories)[0] = "caller-code");
        var leaves = DiagnosticLeaves(first.Value).ToArray();
        var allowed = first.Value.Categories.Concat([first.Value.OriginalReceiptId, first.Value.OriginalReceiptSha256]).ToHashSet(StringComparer.Ordinal);
        Assert.All(leaves, leaf => Assert.Contains(leaf, allowed));
        foreach (var secret in h.Client.ClosedValues.Concat(baseline.Readback.Baseline.Calibration.RepresentativeAssessment.Cases
            .SelectMany(c => new[] { c.CaseId, c.Reason, c.StudentAnswerPreview, c.StudentAnswerSha256 })).Where(s => s.Length > 0))
            Assert.DoesNotContain(secret, leaves);
        Assert.Equal(10, calls);
        Assert.Equal(calls, h.Client.Calls);
        Assert.Equal(effects, h.Open.Session.Snapshot.Effects);
        Assert.Equal(graph, JsonSerializer.SerializeToUtf8Bytes(baseline.Readback.Baseline));
        h.AssertFiles(files);
        }
        output.WriteLine("ORDINARY_DIAGNOSTIC all4 genuine producer/distinct receivers; every backing leaf closed; repeat inference/update/profile/file delta0");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OrdinaryDiagnostic_AllPassRepresentativeWithLaterFailuresRefusesMissingDiagnosis(bool noisy)
    {
        using var h = new OrdinaryDiagnosticHarness((n, _) => n <= 2 || noisy && n > 4
            ? (true, "correct") : (false, "missing_essential_behavior"));
        var original = await h.Consume();
        Assert.True(original.IsSuccess, original.ErrorCode);
        Assert.All(original.Value!.Readback.Baseline.Calibration.RepresentativeAssessment.Cases, c => Assert.True(c.Passed));
        var policy = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BaselineRequiresNewTrainingPair(original.Value.Readback.Baseline);
        Assert.True(policy.IsSuccess && policy.Value);
        var files = h.Files();
        var result = h.Classify(original.Value);
        Assert.False(result.IsSuccess);
        Assert.Equal(CodeDocumentationOneLessonSession.UsableDiagnosisMissingErrorCode, result.ErrorCode);
        Assert.Equal(10, h.Client.Calls);
        h.AssertFiles(files);
    }

    [Theory]
    [InlineData(false, "unknown")]
    [InlineData(false, "free form closed answer")]
    [InlineData(false, "correct")]
    [InlineData(true, "invented_behavior")]
    public async Task OrdinaryDiagnostic_MalformedCheckerPairsAreRefusedBeforeMint(bool passed, string reason)
    {
        using var h = new OrdinaryDiagnosticHarness((_, _) => (passed, reason));
        var result = await h.Consume();
        Assert.False(result.IsSuccess);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, result.ErrorCode);
        Assert.Equal("Prepared baseline calibration did not complete: " + CodeDocumentationSavedModelAssessmentService.EvaluationOutputInvalidErrorCode
            + ". Owner history is preserved without remeasurement. Проверяющий вернул неверный или чужой JSON; оценка не усредняет оставшиеся задания.; actual evaluator calls=1; baseline=False; claim=True", result.ErrorMessage);
        Assert.Equal(1, h.Client.Calls);
        Assert.False(File.Exists(h.Fixture.OrdinaryBaseline));
        var claim = Assert.Single(File.ReadAllLines(h.Fixture.OrdinaryClaim));
        using var started = JsonDocument.Parse(claim);
        Assert.Equal("Started", started.RootElement.GetProperty("Kind").GetString());
    }

    [Theory]
    [InlineData("case")]
    [InlineData("observation")]
    [InlineData("conditions")]
    [InlineData("context")]
    [InlineData("tokenizer")]
    [InlineData("evidence")]
    [InlineData("external-reference")]
    [InlineData("source-reference")]
    [InlineData("grant")]
    [InlineData("grant-case")]
    [InlineData("foreign-original")]
    public async Task OrdinaryDiagnostic_PostMintMutationsRefuseHeldGuardWithoutEffects(string mutation)
    {
        using var h = new OrdinaryDiagnosticHarness((_, _) => (false, "missing_essential_behavior"));
        var original = await h.Consume(); Assert.True(original.IsSuccess, original.ErrorCode);
        var b = original.Value!;
        Assert.True(h.Classify(b).IsSuccess);
        OrdinaryDiagnosticHarness? foreign = null;
        try
        {
            switch (mutation)
            {
                case "case": SetDiagnosticBacking(b.Readback.Baseline.Calibration.RepresentativeAssessment.Cases[1], "Reason", "unknown"); break;
                case "observation": SetDiagnosticBacking(b.Readback.Baseline.Calibration.Calibration.Observations[1], "PassedCaseCount", 1); break;
                case "conditions": SetDiagnosticBacking(b.Readback.Baseline.Calibration.RepresentativeAssessment.GenerationSettings, "Seed", 999); break;
                case "context": SetDiagnosticBacking(b.Context, "MaximumDuration", TimeSpan.FromSeconds(1)); break;
                case "tokenizer":
                    var tokenizerBefore = TokenizerBytes(b.Context.Tokenizer);
                    b.Context.Tokenizer.Train("changed changed tokenizer tokenizer material", h.Fixture.ModelConfiguration.VocabSize + 8);
                    Assert.NotEqual(tokenizerBefore, TokenizerBytes(b.Context.Tokenizer)); break;
                case "evidence": SetDiagnosticBacking(b.Context, "EvidenceRefs", new[] { "test:mutated-evidence" }); break;
                case "external-reference": SetDiagnosticBacking(b.ExternalReference, "ReceiptSha256", new string('a', 64)); break;
                case "source-reference": SetDiagnosticBacking(b.Readback.Reference, "BaselinePath", "changed-private-path"); break;
                case "grant": SetDiagnosticBacking(h.Grant, "TrainingRunId", "changed-run"); break;
                case "grant-case": SetDiagnosticBacking(h.Grant.CorpusSelection!.Corpus, "CorpusId", "changed-corpus"); break;
                case "foreign-original":
                    foreign = new OrdinaryDiagnosticHarness((_, _) => (false, "missing_essential_behavior"));
                    var other = await foreign.Consume(); Assert.True(other.IsSuccess, other.ErrorCode); b = other.Value!; break;
            }
            var files = h.Files();
            var effects = h.Open.Session.Snapshot.Effects;
            var result = h.Classify(b);
            Assert.False(result.IsSuccess);
            Assert.Equal(mutation == "grant" ? CodeDocumentationOneLessonSession.ObjectiveConflictErrorCode
                : CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, result.ErrorCode);
            Assert.Equal(10, h.Client.Calls);
            Assert.Equal(effects, h.Open.Session.Snapshot.Effects);
            h.AssertFiles(files);
        }
        finally { foreign?.Dispose(); }
    }

    [Fact]
    public void OrdinaryDiagnostic_PurePairValidatorChecksWholeVectorAndExistingForwardPhrases()
    {
        var good = new CodeDocumentationSavedModelAssessmentCaseResult("canary-case", false,
            "Ответ пропустил существенное поведение.", "canary-preview", H("answer"), 13);
        foreach (var bad in new[] { good with { Reason = "unknown" }, good with { Reason = "canary closed free form" },
            good with { Reason = "Ответ по смыслу описывает ожидаемое поведение." }, good with { Passed = true } })
        {
            var result = CodeDocumentationSavedModelAssessmentService.ValidateSavedDiagnosticPairs([good, bad]);
            Assert.False(result.IsSuccess);
            Assert.Equal(CodeDocumentationSavedModelAssessmentService.EvaluationOutputInvalidErrorCode, result.ErrorCode);
        }
    }

    private static void SetDiagnosticBacking(object value, string name, object replacement)
        => value.GetType().GetField("<" + name + ">k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(value, replacement);

    // Walk actual instance backing values, including the readonly collection's private array.
    private static IEnumerable<string> DiagnosticLeaves(object value)
        => DiagnosticLeaves(value, new HashSet<object>(ReferenceEqualityComparer.Instance));

    private static IEnumerable<string> DiagnosticLeaves(object value, HashSet<object> visited)
    {
        if (value is string text) { yield return text; yield break; }
        if (!visited.Add(value)) yield break;
        if (value is Array array)
        {
            foreach (var item in array) foreach (var leaf in DiagnosticLeaves(item!, visited)) yield return leaf;
            yield break;
        }
        Assert.True(value is CodeDocumentationOneLessonSession.OrdinaryDiagnostic or
            System.Collections.ObjectModel.ReadOnlyCollection<string>, value.GetType().FullName);
        foreach (var field in value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var item = field.GetValue(value); Assert.NotNull(item);
            foreach (var leaf in DiagnosticLeaves(item!, visited)) yield return leaf;
        }
    }

    [Theory]
    [InlineData("teaching-reopen")]
    [InlineData("closed-after")]
    [InlineData("receiving-after")]
    [InlineData("cancel-after-owner")]
    public async Task OriginalTeachingExclusion_FiniteOwnerAndClosedRechecksRejectLateChangeAndRestoreHooks(string fault)
    {
        using var h = new OrdinaryDiagnosticHarness((_, _) => (false, "missing_essential_behavior"),
            f => f.SealGenuineTeaching());
        var consumed = await h.Consume(); Assert.True(consumed.IsSuccess, consumed.ErrorCode);
        var baseline = consumed.Value!; Assert.True(h.Inspect(baseline).IsSuccess);
        var sources = h.Grant.SourceSelection!.TeachingCurriculum!.Sources;
        var source = sources[0];
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, source.DomainSlug, source.SubjectSlug, source.CorpusVersion).Value!;
        var teachingPath = Path.Combine(address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        var closedPath = Path.Combine(h.Fixture.CorpusAddress.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        var target = fault == "closed-after" ? closedPath : teachingPath;
        var bytes = File.ReadAllBytes(target); var files = h.Files(); var effects = h.Open.Session.Snapshot.Effects;
        var originalEvidence = baseline.Context.EvidenceRefs;
        var prior = RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting;
        using var cancellation = new CancellationTokenSource(); var opens = 0; var injected = 0;
        try
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting = observation =>
            {
                if (observation.RepositoryRelativeRoot != "training-data/reference-corpora") return;
                opens++;
                // One closed open, two teaching opens, two teaching reopens, then one closed after-open.
                var trigger = fault == "teaching-reopen" ? 4 : fault == "cancel-after-owner" ? 2 : 6;
                if (opens != trigger) return;
                injected++;
                if (fault == "cancel-after-owner") cancellation.Cancel();
                else if (fault == "receiving-after") SetDiagnosticBacking(baseline.Context, "EvidenceRefs", new[] { "late-mutated-context" });
                else { var corrupt = bytes.ToArray(); corrupt[^1] ^= 1; File.WriteAllBytes(target, corrupt); }
            };
            if (OperatingSystem.IsLinux())
            {
                if (fault == "cancel-after-owner") Assert.Throws<OperationCanceledException>(() => h.Inspect(baseline, cancellation.Token));
                else { var result = h.Inspect(baseline); Assert.False(result.IsSuccess); Assert.Null(result.Value); }
                Assert.Equal(1, injected);
                output.WriteLine("ORIGINAL_EXCLUSION_EXISTING_CUSTODY_HOOK fault=" + fault + " actualOpens=" + opens + " injected=" + injected);
            }
            else
            {
                Assert.True(h.Inspect(baseline).IsSuccess); Assert.Equal(0, opens);
                Assert.Throws<OperationCanceledException>(() => h.Inspect(baseline, new CancellationToken(true)));
                var corrupt = bytes.ToArray(); corrupt[^1] ^= 1; File.WriteAllBytes(target, corrupt);
                Assert.False(h.Inspect(baseline).IsSuccess);
                output.WriteLine("ORIGINAL_EXCLUSION_WINDOWS actual positive/pre-cancel/replacement; mid-open race and DACL NOT_PROVEN");
            }
            Assert.Equal(10, h.Client.Calls); Assert.Equal(effects, h.Open.Session.Snapshot.Effects);
        }
        finally
        {
            RepositoryOwnedTrainingArtifactCloneCustodyPreparer.RootPreparationObservedForTesting = prior;
            SetDiagnosticBacking(baseline.Context, "EvidenceRefs", originalEvidence);
            File.WriteAllBytes(target, bytes); File.SetLastWriteTimeUtc(target, new DateTime(files[target].Ticks, DateTimeKind.Utc));
        }
        Assert.True(h.Inspect(baseline).IsSuccess); h.AssertFiles(files);
    }

    private static OrdinaryDiagnosticHarness SelectionHarness()
        => new((_,_) => (false,"missing_essential_behavior"),SelectionCurriculum);
    internal static void RestoreCustodyFile(string path,byte[] bytes,DateTime lastWriteUtc)
    {
        // Re-create a deleted fixture through the same owner boundary; an inherited Windows DACL is not custody.
        using(var owned=SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path,
            File.Exists(path)?FileMode.Open:FileMode.CreateNew,FileAccess.ReadWrite,FileShare.Read,FileOptions.WriteThrough))
        {owned.SetLength(0);owned.Write(bytes);owned.Flush(flushToDisk:true);}
        File.SetLastWriteTimeUtc(path,lastWriteUtc);
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path),"Restored fixture must retain genuine custody: "+path);
        Assert.Equal(bytes,File.ReadAllBytes(path)); Assert.Equal(lastWriteUtc,File.GetLastWriteTimeUtc(path));
    }
    internal static void RestoreReferenceOwnerFile(string path, byte[] bytes, DateTime lastWriteUtc)
    {
        // ReferenceCorpusManifest.WriteNoOverwrite uses the ordinary owner boundary, not the journal's protected Windows ACL.
        Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName, Path.GetFileName(path));
        Assert.True(File.Exists(path), "Reference fault may only edit its already-sealed fixture payload: " + path);
        Assert.True(SecureLocalFileBoundary.HasOwnerOnlyFileMode(path));
        var originalMode = JournalStorageMode(path, directory: false);
        using (var owned = SecureLocalFileBoundary.OpenOwnerOnlyFile(path, FileMode.Open,
                   FileAccess.ReadWrite, FileShare.Read, FileOptions.WriteThrough))
        {
            Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(owned.SafeFileHandle, path));
            owned.SetLength(0); owned.Write(bytes); owned.Flush(flushToDisk: true);
        }
        File.SetLastWriteTimeUtc(path, lastWriteUtc);
        Assert.Equal(originalMode, JournalStorageMode(path, directory: false));
        Assert.True(SecureLocalFileBoundary.HasOwnerOnlyFileMode(path));
        Assert.Equal(bytes, File.ReadAllBytes(path)); Assert.Equal(lastWriteUtc, File.GetLastWriteTimeUtc(path));
    }
    internal static void RestoreFixtureAfterFault(Action restore, Exception? primaryFailure)
    {
        try { restore(); }
        catch (Exception cleanupFailure) when (primaryFailure is not null)
        {
            // Both failures remain visible; cleanup must never replace the primary fault or become a passing continuation.
            throw new AggregateException("Fixture restoration failed after a primary fault.", primaryFailure, cleanupFailure);
        }
    }
    internal static CodeDocumentationOneLessonTeachingCurriculumV2 SelectionCurriculum(
        CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture fixture)
    {
        var curriculum=fixture.SealGenuineTeaching();
        return SelectionCategories(curriculum);
    }
    internal static CodeDocumentationOneLessonTeachingCurriculumV2 SelectedMaterialCurriculum(
        CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture fixture)
        => SelectionCategories(fixture.SealGenuineTeaching("material-unicode"));
    private static CodeDocumentationOneLessonTeachingCurriculumV2 SelectionCategories(CodeDocumentationOneLessonTeachingCurriculumV2 curriculum)
    {
        // Declare the synthetic diagnostic labels before sealing the grant; preserve both complete item orders.
        // The general source-selection fixture deliberately uses descriptive category-a/category-b labels.
        return curriculum with { CategoryBindings = new[] {
            new CodeDocumentationOneLessonTeachingCategoryBindingV2("missing_essential_behavior",curriculum.CategoryBindings[0].OrderedItemIds),
            new CodeDocumentationOneLessonTeachingCategoryBindingV2("invented_behavior",curriculum.CategoryBindings[1].OrderedItemIds) } };
    }
    private static CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice SelectionChoice(
        CodeDocumentationOneLessonSession.OrdinaryOriginalTeachingExclusion witness,string reason="Synthetic fixture selected the declared teaching link.")
    {
        var graph=witness.TeachingIdentities;
        var item=graph.Items.Single(i=>i.DeclaredItem.ItemId==graph.CategoryBindings
            .Single(c=>c.Category=="missing_essential_behavior").OrderedItemIds[0]);
        return new(item.DeclaredItem.ItemId,item.DeclaredItem.SourceId,item.Entry.EntryId,
            item.Entry.SourceEntityFingerprint,reason,H("synthetic-material-evidence"));
    }


    internal static OrdinaryDiagnosticHarness TeacherContextHarness(int historyPairs = 1, int padding = 0, string? historyFault = null, string? chosenHistorySuffix = null, bool rawSelectedReference = false) // Raw reference opt-in retains default fixture identities/content.
        => new((_, _) => (false, "missing_essential_behavior"),
            f => SelectionCategories(f.SealGenuineTeaching(rawSelectedReference ? "student-task-branches-raw" : "student-task-branches")),
            configureGenuineParent: CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.ConfigureSelectedTaskParent,
            syntheticSelectedStudentMode: true, syntheticContextHistoryPairs: historyPairs, contextHistoryPadding: padding, contextHistoryFault: historyFault, contextHistoryChosenSuffix: chosenHistorySuffix);
    internal static async Task<(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline Baseline,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission Admission)> BindTeacherCompleted(
        OrdinaryDiagnosticHarness h, string item = "item-teach-2-0")
    {
        var inputs = await BindSelectedStudent(h, item); var response = await ConsumeSelectedStudent(h, inputs.Baseline, inputs.Admission);
        Assert.True(response.IsSuccess, response.ErrorCode + ":" + response.ErrorMessage);
        Assert.Equal(item == "item-teach-7-0" ? "a" : "b", response.Value!.StudentResponse);
        Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting);
        Assert.Equal(4, h.HistoryBytes().Count(x => x == (byte)'\n'));
        return inputs;
    }
    internal static OperationResult<bool> AssessTeacherHistory(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SyntheticHistoryAssessmentInput input, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); Assert.Equal("SyntheticTest", input.Student.Origin);
        Assert.Equal(2, input.History.Pairs.Count); Assert.Equal(2, input.History.Sources.Count);
        Assert.Equal(new[] { "history-source", "ordinary-source" }, input.History.Pairs.Select(p => p.SourceId));
        Assert.StartsWith("Document a previously accepted context function ", input.History.Pairs[0].Pair.Prompt);
        Assert.StartsWith("Explain the complete earlier behavior ", input.History.Pairs[0].Pair.Chosen);
        Assert.StartsWith("Incorrect earlier explanation ", input.History.Pairs[0].Pair.Rejected);
        Assert.Equal("Document a S03.04 repository-owned endpoint method.", input.History.Pairs[1].Pair.Prompt);
        Assert.Equal("Explain the method's accepted endpoint behavior.", input.History.Pairs[1].Pair.Chosen);
        Assert.Equal("Give an unhelpful endpoint summary.", input.History.Pairs[1].Pair.Rejected);
        Assert.All(input.History.Pairs, p =>
        {
            Assert.NotNull(p.Pair.TeacherProvenance); Assert.NotNull(p.Pair.GovernanceDecision);
            Assert.NotNull(p.Pair.ChosenAuthorship); Assert.NotNull(p.Pair.RejectedAuthorship);
            Assert.True(TrainingDataGovernanceAdmission.ValidateDecision(p.Pair.GovernanceDecision).IsSuccess);
            Assert.Equal(p.Row.PairId, FileDpoPairStore.ComputeIdentity(p.Pair, schemaVersion: 5));
        });
        Assert.Equal(Encoding.UTF8.GetBytes(input.Student.Request.Task), input.Student.Material.CopyOwnerInputUtf8());
        Assert.Equal(input.Student.Request.SourceSha256, input.History.SourceSha256);
        return OperationResult<bool>.Ok(true); // Explicit invoked full-content Synthetic assessment; not real Astra acceptance.
    }
    internal static Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContent>> ConsumeTeacherContent(
        OrdinaryDiagnosticHarness h, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission material,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedHistoryAdmission? saved = null,
        Func<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SyntheticHistoryAssessmentInput, CancellationToken, OperationResult<bool>>? assess = null,
        CancellationToken token = default)
        => h.Open.Session.PrepareOrdinarySelectedTeacherContentForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, material, saved, assess, token);
    internal static CodeDocumentationOneLessonSourceSelection.DeclaredContextHistoryReadback ReadTeacherHistory(
        OrdinaryDiagnosticHarness h, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b)
    {
        var c = b.Readback.Baseline.Calibration;
        var opened = h.Grant.SourceSelection!.OpenDeclaredContextHistoryExcludingOriginal(h.Fixture.RepositoryRoot, b.Context.Corpus,
            c.RepresentativeAssessment.Cases, c.Calibration.Observations);
        Assert.True(opened.IsSuccess, opened.ErrorCode + ":" + opened.ErrorMessage); return opened.Value!;
    }
    internal static void RestoreOwnedHistoryPair(OrdinaryDiagnosticHarness h, string path, byte[] bytes, DateTime ticks)
    {
        Assert.Contains(path, h.Fixture.EnumerateOwnedOrdinaryFiles());
        Assert.True(File.Exists(path)); Assert.True(SecureLocalFileBoundary.HasOwnerOnlyFileMode(path));
        var mode = JournalStorageMode(path, false);
        using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, FileOptions.WriteThrough))
        { Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path)); stream.SetLength(0); stream.Write(bytes); stream.Flush(true); }
        File.SetLastWriteTimeUtc(path, ticks); Assert.Equal(mode, JournalStorageMode(path, false));
        Assert.Equal(bytes, File.ReadAllBytes(path)); Assert.Equal(ticks, File.GetLastWriteTimeUtc(path));
    }

    [Fact]
    public async Task OrdinaryTeacherContent_FullTaskRawResponseAndAssessedHistoryReachRecipient()
    {
        foreach (var (item, rawReference) in new[] { ("item-teach-7-0", false), ("item-teach-2-0", false), ("item-teach-2-0", true) })
        {
            using var h = TeacherContextHarness(rawSelectedReference: rawReference); var (b, a) = await BindTeacherCompleted(h, item);
            var files = h.Files(); var prefix = h.HistoryBytes(); var calls = 0;
            var received = new List<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContent>();
            var old = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting;
            try
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = received.Add;
                var result = await ConsumeTeacherContent(h, b, a, assess: (input, token) => { calls++; return AssessTeacherHistory(input, token); });
                Assert.True(result.IsSuccess, result.ErrorCode + ":" + result.ErrorMessage); var context = result.Value!;
                Assert.Same(context, Assert.Single(received)); Assert.Equal(1, calls);
                Assert.Equal(item == "item-teach-7-0" ? "Teach branch X" : "Teach branch Y", context.Student.Material.OwnerInput);
                Assert.Equal(item == "item-teach-7-0" ? "a" : "b", context.Student.StudentResponse);
                Assert.Equal("SyntheticTest", context.Student.Material.Origin); Assert.Equal("SyntheticTest", context.Student.Origin);
                if (rawReference)
                {
                    const string rawPaid = "independent teaching paid answer original-exclusion-2-1\r\nCafe\u0301\tEND";
                    var expectedBytes = new UTF8Encoding(false, true).GetBytes(rawPaid);
                    Assert.Equal(rawPaid, context.Student.Material.PaidModelReferenceAnswer);
                    Assert.Equal(expectedBytes, context.Student.Material.CopyPaidReferenceUtf8());
                    Assert.Equal(string.Empty, context.Student.Material.PaidReferenceProvenance.ReferenceAnswerSha256);
                    Assert.Equal("sha256:" + HBytes(expectedBytes), context.Student.Material.PaidReferenceProvenance.ReferenceAnswerRawUtf8Sha256);
                    Assert.NotEqual(FrozenReferenceEntry.ComputeNormalizedTextSha256(rawPaid), context.Student.Material.PaidReferenceProvenance.ReferenceAnswerRawUtf8Sha256);
                }
                var history = ReadTeacherHistory(h, b); Assert.True(history.SameGraph(context.History));
                using var json = JsonDocument.Parse(context.CopyTeacherContentUtf8()); var root = json.RootElement;
                Assert.Equal(context.Student.Material.OwnerInput, root.GetProperty("task").GetString());
                Assert.Equal(context.Student.StudentResponse, root.GetProperty("weakStudentResponse").GetString());
                Assert.Equal(context.Student.Material.PaidModelReferenceAnswer, root.GetProperty("teachingReference").GetString());
                Assert.Equal(2, root.GetProperty("history").GetArrayLength());
                foreach (var (row, i) in context.History.Pairs.Select((p, i) => (p, i)))
                {
                    var cell = root.GetProperty("history")[i]; var pair = cell.GetProperty("pair");
                    Assert.Equal(row.Row.PairId, cell.GetProperty("pairId").GetString());
                    Assert.Equal(row.Pair.Prompt, pair.GetProperty("prompt").GetString());
                    Assert.Equal(row.Pair.Chosen, pair.GetProperty("chosen").GetString());
                    Assert.Equal(row.Pair.Rejected, pair.GetProperty("rejected").GetString());
                    Assert.Equal("xiigen.dpo-pair.v5", pair.GetProperty("recordDomain").GetString());
                    Assert.Equal(row.Pair.TeacherProvenance!.TrainingContentSha256,
                        pair.GetProperty("teacherProvenance").GetProperty("provenance").GetProperty("trainingContentSha256").GetString());
                    Assert.Equal(row.Pair.GovernanceDecision!.EvidenceRefs,
                        pair.GetProperty("teacherProvenance").GetProperty("governanceDecision").GetProperty("evidenceRefs").EnumerateArray().Select(e => e.GetString()!));
                    Assert.Equal(row.Pair.ChosenAuthorship!.AuthorKind.ToString(), pair.GetProperty("sideAuthorship").GetProperty("chosen").GetProperty("authorKind").GetString());
                }
                var bytes = context.CopyTeacherContentUtf8(); bytes[0] ^= 1; Assert.NotEqual(bytes, context.CopyTeacherContentUtf8());
                Assert.Equal("Unavailable", context.TeacherTokenizerAdmission); Assert.False(context.FullTeacherContextReady);
                Assert.Equal(prefix, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls); Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting);
                Assert.Equal(0, h.Open.Session.Snapshot.Effects.OptimizerEntries); Assert.Equal(0, h.Open.Session.Snapshot.Effects.TeachingDispatched); h.AssertFiles(files);
            }
            finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = old; }
        }
    }

    [Fact]
    public async Task OrdinaryTeacherContent_HistoryAdmissionMissingStaleForeignOrDeniedRefuses()
    {
        using var h = TeacherContextHarness(); var (b, a) = await BindTeacherCompleted(h);
        var accepted = await ConsumeTeacherContent(h, b, a, assess: AssessTeacherHistory); Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        var files = h.Files(); var prefix = h.HistoryBytes(); var assessmentCalls = 0;
        var missing = await ConsumeTeacherContent(h, b, a); Assert.False(missing.IsSuccess); Assert.Null(missing.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.HistoryAdmissionRequiredErrorCode, missing.ErrorCode);
        var rejected = await ConsumeTeacherContent(h, b, a, assess: (input, token) =>
        { assessmentCalls++; Assert.True(AssessTeacherHistory(input, token).IsSuccess); return OperationResult<bool>.Ok(false); });
        Assert.False(rejected.IsSuccess); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.HistoryAdmissionInvalidErrorCode, rejected.ErrorCode);
        var both = await ConsumeTeacherContent(h, b, a, saved: accepted.Value!.Admission, assess: (_, _) => { assessmentCalls++; throw new InvalidOperationException("Mutually exclusive mode must refuse before invoking."); });
        Assert.False(both.IsSuccess); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.HistoryAdmissionInvalidErrorCode, both.ErrorCode); Assert.Equal(1, assessmentCalls);
        using var foreign = TeacherContextHarness(); var (fb, fa) = await BindTeacherCompleted(foreign, "item-teach-7-0");
        var foreignControl = await ConsumeTeacherContent(foreign, fb, fa, assess: AssessTeacherHistory); Assert.True(foreignControl.IsSuccess, foreignControl.ErrorMessage);
        var foreignResult = await ConsumeTeacherContent(h, b, a, saved: foreignControl.Value!.Admission);
        Assert.False(foreignResult.IsSuccess); Assert.Null(foreignResult.Value); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.HistoryAdmissionInvalidErrorCode, foreignResult.ErrorCode);
        var field = accepted.Value.Admission.GetType().GetField("requestSha256", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var original = field.GetValue(accepted.Value.Admission);
        try
        {
            field.SetValue(accepted.Value.Admission, H("stale-context"));
            var stale = await ConsumeTeacherContent(h, b, a, saved: accepted.Value.Admission);
            Assert.False(stale.IsSuccess); Assert.Null(stale.Value); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.HistoryAdmissionInvalidErrorCode, stale.ErrorCode);
        }
        finally { field.SetValue(accepted.Value.Admission, original); }
        Assert.Equal(prefix, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls); Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting); h.AssertFiles(files);
        var replay = await ConsumeTeacherContent(h, b, a, saved: accepted.Value.Admission); Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.Equal(accepted.Value.CopyTeacherContentUtf8(), replay.Value!.CopyTeacherContentUtf8());
    }

    [Fact]
    public async Task OrdinaryTeacherContent_CompletedReopenReusesExactContextWithoutEffects()
    {
        using var h = TeacherContextHarness(); var (b, a) = await BindTeacherCompleted(h); var count = 0;
        var first = await ConsumeTeacherContent(h, b, a, assess: (input, token) => { count++; return AssessTeacherHistory(input, token); });
        Assert.True(first.IsSuccess, first.ErrorMessage); Assert.Equal(1, count); var files = h.Files(); var prefix = h.HistoryBytes();
        Assert.True(h.Reopen().IsSuccess); var baseline = await h.Consume(); Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var old = h.Open.Session.SelectedStudentDurablePointForTesting; var generated = 0;
        try
        {
            h.Open.Session.SelectedStudentDurablePointForTesting = stage =>
            { if (stage.Contains("generate", StringComparison.Ordinal)) { generated++; throw new InvalidOperationException("Completed context must not Generate."); } };
            var replay = await ConsumeTeacherContent(h, baseline.Value!, a, saved: first.Value!.Admission, assess: null);
            Assert.True(replay.IsSuccess, replay.ErrorCode + ":" + replay.ErrorMessage);
            Assert.Equal(first.Value.ContentSha256, replay.Value!.ContentSha256); Assert.Equal(first.Value.CopyTeacherContentUtf8(), replay.Value.CopyTeacherContentUtf8());
            Assert.Equal(1, count); Assert.Equal(0, generated); Assert.Equal(prefix, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls); h.AssertFiles(files);
        }
        finally { h.Open.Session.SelectedStudentDurablePointForTesting = old; }
        using var started = TeacherContextHarness(); var (sb, sa) = await BindSelectedStudent(started);
        var untouched = started.HistoryBytes();
        var noCompleted = await ConsumeTeacherContent(started, sb, sa, assess: (_, _) => throw new InvalidOperationException("Missing completed input cannot assess."));
        Assert.False(noCompleted.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, noCompleted.ErrorCode); Assert.Null(noCompleted.Value);
        Assert.Equal(untouched, started.HistoryBytes()); Assert.Equal((0, 0, false), started.Open.Session.SelectedStudentAccounting);
        var previousHook = started.Open.Session.SelectedStudentDurablePointForTesting;
        var primary = new InvalidOperationException("Durable Started cannot authorize a Teacher context or retry.");
        try
        {
            started.Open.Session.SelectedStudentDurablePointForTesting = stage => { if (stage == "student-before-generate") throw primary; };
            Assert.Same(primary, await Assert.ThrowsAsync<InvalidOperationException>(() => ConsumeSelectedStudent(started, sb, sa)));
            Assert.Equal(3, started.HistoryBytes().Count(x => x == (byte)'\n')); Assert.Equal((1, 0, true), started.Open.Session.SelectedStudentAccounting);
            var unknownBytes = started.HistoryBytes(); var startedFiles = started.Files();
            var unknown = await ConsumeTeacherContent(started, sb, sa, assess: (_, _) => throw new InvalidOperationException("Unknown Started is never history acceptance."));
            Assert.False(unknown.IsSuccess); Assert.Null(unknown.Value); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, unknown.ErrorCode);
            Assert.Equal(unknownBytes, started.HistoryBytes()); Assert.Equal(10, started.Client.Calls); started.AssertFiles(startedFiles);
        }
        finally { started.Open.Session.SelectedStudentDurablePointForTesting = previousHook; }
    }

    [Fact]
    public async Task OrdinaryTeacherContent_PrimaryFailureAndLateOwnersFinishNoneChecks()
    {
        foreach (var fault in new[] { "history", "history-manifest", "original", "unused-source", "reference", "response", "receiving", "throw", "cancel", "cancel-history",
            "throw-owner-cancel", "cancel-owner-cancel", "throw-hook-cancel", "cancel-hook-cancel" })
        {
            using var h = TeacherContextHarness(); var (b, a) = await BindTeacherCompleted(h);
            var first = await ConsumeTeacherContent(h, b, a, assess: AssessTeacherHistory); Assert.True(first.IsSuccess, fault + ":" + first.ErrorMessage);
            var files = h.Files(); var historyBytes = h.HistoryBytes(); var refs = b.Context.EvidenceRefs;
            var row = first.Value!.History.Pairs[0]; var path = Path.Combine(h.Fixture.RepositoryRoot, row.Row.RepositoryRelativePairFilePath.Replace('/', Path.DirectorySeparatorChar));
            if (fault == "history-manifest")
            {
                var source = first.Value.History.Sources[0].Descriptor;
                var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(h.Fixture.RepositoryRoot, source.DomainSlug, source.DesignPatternSlug, source.DatasetVersion);
                Assert.True(address.IsSuccess, address.ErrorCode); path = address.Value!.ManifestPath;
            }
            if (fault == "original") path = Path.Combine(h.Fixture.CorpusAddress.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
            if (fault is "unused-source" or "reference")
            {
                var source = h.Grant.SourceSelection!.TeachingCurriculum!.Sources.Single(s => s.SourceId == (fault == "unused-source" ? "teach-7" : first.Value.Student.Request.SourceId));
                if (fault == "unused-source") Assert.NotEqual(source.SourceId, first.Value.Student.Request.SourceId);
                var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, source.DomainSlug, source.SubjectSlug, source.CorpusVersion);
                Assert.True(address.IsSuccess, address.ErrorCode); path = Path.Combine(address.Value!.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
            }
            if (fault == "response") path = h.Fixture.OrdinaryHistory;
            var originalBytes = fault == "response" ? h.HistoryBytes() : File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path); var final = 0; var touched = 0;
            var secondaryOwner = fault is "throw-owner-cancel" or "cancel-owner-cancel";
            var secondaryHook = fault is "throw-hook-cancel" or "cancel-hook-cancel";
            var primaryCancellation = fault is "cancel" or "cancel-history" or "cancel-owner-cancel" or "cancel-hook-cancel";
            var ownerFinal = 0; var generated = 0;
            using var cancellation = new CancellationTokenSource(); var primary = new InvalidOperationException("primary Teacher context fixture");
            var canceled = new OperationCanceledException("primary canceled Teacher context", cancellation.Token);
            using var secondaryCancellation = new CancellationTokenSource(); secondaryCancellation.Cancel();
            var secondary = new OperationCanceledException("secondary Teacher cleanup cancellation", secondaryCancellation.Token);
            var oldObserver = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting;
            var oldHook = h.Open.Session.SelectedStudentDurablePointForTesting; Exception? failure = null;
            var oldOwnerHook = h.Open.Session.SelectionDurablePointForTesting;
            var logicalOwner = (AsyncLocal<CodeDocumentationOneLessonSession?>)typeof(CodeDocumentationOneLessonSession)
                .GetField("_studentCheckOwner", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(h.Open.Session)!;
            var previousLogicalOwner = logicalOwner.Value;
            try
            {
                h.Open.Session.SelectionDurablePointForTesting = stage =>
                {
                    if (stage != "teacher-context-final-owner-checked") return;
                    Assert.Same(h.Open.Session, logicalOwner.Value);
                    ownerFinal++; if (secondaryOwner) throw secondary;
                };
                h.Open.Session.SelectedStudentDurablePointForTesting = stage =>
                {
                    if (stage == "teacher-context-final-owner-checked") { final++; if (secondaryHook) throw secondary; }
                    if (stage.Contains("generate", StringComparison.Ordinal)) { generated++; throw new InvalidOperationException("Completed only."); }
                };
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = context =>
                {
                    touched++; Assert.Equal(first.Value.ContentSha256, context.ContentSha256);
                    if (secondaryOwner || secondaryHook)
                    {
                        // The actual invalid pair makes the independent final history attempt observable after primary failure.
                        File.WriteAllBytes(path, originalBytes.Concat(new byte[] { (byte)' ' }).ToArray());
                        if (primaryCancellation) { cancellation.Cancel(); throw canceled; }
                        throw primary;
                    }
                    if (fault == "throw") throw primary;
                    if (fault == "cancel") { cancellation.Cancel(); throw canceled; }
                    if (fault == "cancel-history") { File.WriteAllBytes(path, originalBytes.Concat(new byte[] { (byte)' ' }).ToArray()); cancellation.Cancel(); throw canceled; }
                    if (fault == "response")
                    {
                        var stream = (FileStream)typeof(CodeDocumentationOneLessonSession).GetField("_v2History", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(h.Open.Session)!;
                        stream.Position = stream.Length - 1; stream.WriteByte((byte)' '); stream.Flush(true); return;
                    }
                    if (fault == "receiving") SetDiagnosticBacking(b.Context, "EvidenceRefs", new[] { "test:changed-context" });
                    else if (fault == "history-manifest") { var invalid = originalBytes.ToArray(); invalid[0] = 0xff; File.WriteAllBytes(path, invalid); }
                    else File.WriteAllBytes(path, originalBytes.Concat(new byte[] { (byte)' ' }).ToArray());
                };
                if (fault is "throw" or "throw-owner-cancel" or "throw-hook-cancel")
                {
                    var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => ConsumeTeacherContent(h, b, a, saved: first.Value.Admission));
                    Assert.Same(primary, actual);
                    if (secondaryOwner || secondaryHook)
                    {
                        Assert.Same(secondary, actual.Data[secondaryOwner ? "TeacherContextFinalOwnerFailure" : "TeacherContextFinalHookFailure"]);
                        Assert.Equal(RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode, actual.Data["TeacherContextFinalHistoryErrorCode"]);
                        if (secondaryOwner) Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, actual.Data["TeacherContextFinalOwnerErrorCode"]);
                    }
                }
                else if (primaryCancellation)
                {
                    var actual = await Assert.ThrowsAsync<OperationCanceledException>(() => ConsumeTeacherContent(h, b, a, saved: first.Value.Admission, token: cancellation.Token));
                    Assert.Same(canceled, actual); Assert.Equal(cancellation.Token, actual.CancellationToken);
                    if (secondaryOwner || secondaryHook)
                    {
                        var cleanup = Assert.IsType<OperationCanceledException>(actual.Data[secondaryOwner ? "TeacherContextFinalOwnerFailure" : "TeacherContextFinalHookFailure"]);
                        Assert.Same(secondary, cleanup); Assert.Equal(secondaryCancellation.Token, cleanup.CancellationToken);
                        Assert.Equal(RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode, actual.Data["TeacherContextFinalHistoryErrorCode"]);
                        if (secondaryOwner) Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, actual.Data["TeacherContextFinalOwnerErrorCode"]);
                    }
                    else if (fault == "cancel-history") Assert.Equal(RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode, actual.Data["TeacherContextFinalHistoryErrorCode"]);
                    else { Assert.False(actual.Data.Contains("TeacherContextFinalOwnerErrorCode")); Assert.False(actual.Data.Contains("TeacherContextFinalHistoryErrorCode")); }
                }
                else
                {
                    var refused = await ConsumeTeacherContent(h, b, a, saved: first.Value.Admission);
                    Assert.False(refused.IsSuccess, fault); Assert.Null(refused.Value);
                    Assert.Equal(fault is "history" or "history-manifest" ? RepositoryOwnedDpoDatasetManifestSealer.InvalidErrorCode :
                        fault is "receiving" or "response" ? CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode : RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, refused.ErrorCode);
                }
                Assert.Equal(1, touched); Assert.Equal(1, final); Assert.Equal(1, ownerFinal); if (fault != "response") Assert.Equal(historyBytes, h.HistoryBytes());
                Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting); Assert.Equal(10, h.Client.Calls);
                Assert.Equal(0, generated);
                Assert.Equal(0, (int)typeof(CodeDocumentationOneLessonSession).GetField("_studentResponseInFlight", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(h.Open.Session)!);
                Assert.False((bool)typeof(CodeDocumentationOneLessonSession).GetField("_teacherContextIssuing", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(h.Open.Session)!);
                Assert.False((bool)typeof(CodeDocumentationOneLessonSession).GetField("_studentIssuing", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(h.Open.Session)!);
                Assert.Same(previousLogicalOwner, logicalOwner.Value);
                if (fault is "history" or "history-manifest" or "original" or "unused-source" or "reference" or "cancel-history" || secondaryOwner || secondaryHook) Assert.NotEqual(originalBytes, File.ReadAllBytes(path));
                if (fault == "response") Assert.NotEqual(historyBytes, h.HistoryBytes());
            }
            catch (Exception ex) { failure = ex; throw; }
            finally
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeacherContentCapturedForTesting = oldObserver;
                h.Open.Session.SelectedStudentDurablePointForTesting = oldHook; h.Open.Session.SelectionDurablePointForTesting = oldOwnerHook; SetDiagnosticBacking(b.Context, "EvidenceRefs", refs);
                if (fault is "history" or "history-manifest" or "cancel-history" || secondaryOwner || secondaryHook) RestoreFixtureAfterFault(() => RestoreOwnedHistoryPair(h, path, originalBytes, ticks), failure);
                if (fault is "original" or "unused-source" or "reference") RestoreFixtureAfterFault(() => RestoreReferenceOwnerFile(path, originalBytes, ticks), failure);
                if (fault == "response") { h.Open.Dispose(); RestoreFixtureAfterFault(() => RestoreCustodyFile(path, originalBytes, ticks), failure); }
            }
            if (fault == "response") { Assert.True(h.Reopen().IsSuccess); var rebound = await h.Consume(); Assert.True(rebound.IsSuccess, rebound.ErrorMessage); b = rebound.Value!; }
            h.AssertFiles(files);
            var restoredHook = h.Open.Session.SelectedStudentDurablePointForTesting;
            try
            {
                h.Open.Session.SelectedStudentDurablePointForTesting = stage =>
                { if (stage.Contains("generate", StringComparison.Ordinal)) { generated++; throw new InvalidOperationException("Restored full context replay cannot Generate."); } };
                var restored = await ConsumeTeacherContent(h, b, a, saved: first.Value.Admission); Assert.True(restored.IsSuccess, fault + ":" + restored.ErrorMessage);
                Assert.Equal(first.Value.CopyTeacherContentUtf8(), restored.Value!.CopyTeacherContentUtf8()); Assert.Equal(historyBytes, h.HistoryBytes());
                Assert.Equal(0, generated); Assert.Equal(10, h.Client.Calls); Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting);
                Assert.Equal(0, h.Open.Session.Snapshot.Effects.TeachingDispatched); Assert.Equal(0, h.Open.Session.Snapshot.Effects.OptimizerEntries); h.AssertFiles(files);
            }
            finally { h.Open.Session.SelectedStudentDurablePointForTesting = restoredHook; }
        }
    }


    internal static OrdinaryDiagnosticHarness SelectedStudentHarness()
        => new((_, _) => (false, "missing_essential_behavior"),
            f => SelectionCategories(f.SealGenuineTeaching("student-task-branches")),
            configureGenuineParent: CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.ConfigureSelectedTaskParent,
            syntheticSelectedStudentMode: true);
    internal static async Task<(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline Baseline,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission Admission)> BindSelectedStudent(
        OrdinaryDiagnosticHarness h, string itemId = "item-teach-2-0")
    {
        var produced = await h.Consume(); Assert.True(produced.IsSuccess, produced.ErrorMessage); var b = produced.Value!;
        Assert.Equal(10, h.Client.Calls); Assert.All(h.Client.StudentAnswers, answer => Assert.Equal("~", answer));
        var closed = H("~"); Assert.All(b.Readback.Baseline.Calibration.RepresentativeAssessment.Cases, c => Assert.Equal(closed, c.StudentAnswerSha256));
        Assert.All(b.Readback.Baseline.Calibration.Calibration.Observations,
            o => Assert.All(o.StudentAnswerSha256, hash => Assert.Equal(closed, hash)));
        var admission = SelectedMaterialAdmission(h, b, itemId: itemId);
        await BindSelectedMaterial(h, b, admission, requireNonFirst: itemId != "item-teach-7-0");
        var prefix=h.Open.Session.ReadCapacityPrefixV2();Assert.True(prefix.IsSuccess,prefix.ErrorCode+":"+prefix.ErrorMessage);
        Assert.Equal(2,prefix.Value!.PrefixFrames);Assert.Equal(40,prefix.Value.MaximumFrames);
        Assert.Equal(2,h.HistoryBytes().Count(value=>value==(byte)'\n'));Assert.Equal((0,0,false),h.Open.Session.SelectedStudentAccounting);
        var saved=h.Open.Session.TryReadOrdinarySelectionBinding(b,h.Catalog,h.Fixture.OrdinarySelector);
        Assert.True(saved.IsSuccess,saved.ErrorCode+":"+saved.ErrorMessage);Assert.Equal(itemId,saved.Value!.Payload.ItemId);
        var material=ConsumeSelectedMaterial(h,b,admission);Assert.True(material.IsSuccess,material.ErrorCode+":"+material.ErrorMessage);
        var task=itemId=="item-teach-7-0"?"Teach branch X":"Teach branch Y";
        Assert.Equal(task,material.Value!.OwnerInput);Assert.Equal(Encoding.UTF8.GetBytes(task),material.Value.CopyOwnerInputUtf8());
        Assert.Equal(FrozenReferencePartition.HeldOut,admission.Entry.Partition);
        var tokens=b.Context.Tokenizer.Encode(material.Value.OwnerInput);Assert.NotEmpty(tokens);
        Assert.True((long)tokens.Count+b.Context.GenerationSettings.MaxNewTokens-1<=h.Start.ModelConfig.ContextLen,
            $"Full selected task {itemId} needs {tokens.Count}+{b.Context.GenerationSettings.MaxNewTokens}-1 tokens; context={h.Start.ModelConfig.ContextLen}.");
        Assert.Equal(10,h.Client.Calls);Assert.Equal((0,0,false),h.Open.Session.SelectedStudentAccounting);
        return (b, admission);
    }
    internal static Task<OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>> ConsumeSelectedStudent(
        OrdinaryDiagnosticHarness h, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission admission, CancellationToken token = default)
        => h.Open.Session.PrepareOrdinarySelectedStudentContextForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admission, token);

    [Fact]
    public async Task OrdinarySelectedStudent_GenuineTaskDependentParentFeedsPartialRecipient()
    {
        var responses = new List<string>();
        foreach (var branch in new[] { (Item: "item-teach-7-0", Task: "Teach branch X", Answer: "a"), (Item: "item-teach-2-0", Task: "Teach branch Y", Answer: "b") })
        {
            using var h = SelectedStudentHarness(); var (b, admission) = await BindSelectedStudent(h, branch.Item);
            Assert.Equal(branch.Task, ConsumeSelectedMaterial(h,b,admission).Value!.OwnerInput);
            Assert.Equal(new[] { (int)branch.Task[^1] }, b.Context.Tokenizer.Encode(branch.Task[^1].ToString()));
            var before = h.Files(); var history = h.HistoryBytes();
            var captures = new List<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedStudentContext>();
            var observer = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting;
            var entered = 0; var completed = 0; var final = 0;
            try
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting = captures.Add;
                h.Open.Session.SelectedStudentDurablePointForTesting = stage =>
                {
                    if (stage == "student-before-generate") { entered++; Assert.Equal(3, h.Open.Session.SelectedStudentAccounting.ReservedGenerationSlots == 1 ? h.HistoryBytes().Count(v => v == '\n') : -1); }
                    if (stage == "student-after-generate") completed++;
                    if (stage == "student-final-owner-checked") final++;
                };
                var actual = await ConsumeSelectedStudent(h, b, admission); Assert.True(actual.IsSuccess, actual.ErrorMessage);
                var context = actual.Value!; Assert.Same(Assert.Single(captures), context);
                Assert.Equal(branch.Answer, context.StudentResponse); Assert.Equal(branch.Task, context.Material.OwnerInput);
                Assert.Equal(Encoding.UTF8.GetBytes(branch.Answer), context.CopyStudentResponseUtf8());
                Assert.Equal(H(branch.Answer), context.StudentResponseSha256); Assert.NotEqual(context.Material.PaidModelReferenceAnswer, context.StudentResponse);
                Assert.Equal(admission.Item, context.Material.Item); Assert.Equal(admission.BindingSha256, context.Request.BindingSha256);
                Assert.Equal(admission.SourceSha256, context.Request.SourceSha256); Assert.Equal(admission.FrozenContextSha256, context.Request.FrozenContextSha256);
                Assert.Equal(admission.EvidenceSha256, context.Request.MaterialAdmissionEvidenceSha256);
                Assert.Equal(b.ExternalReference.ReceiptId, context.Request.OriginalReceiptId); Assert.Equal(b.ExternalReference.ReceiptSha256, context.Request.OriginalReceiptSha256);
                Assert.Equal(h.Grant.ParentWeightsSha256, context.Request.ParentWeightsSha256); Assert.Equal(h.Grant.ParentManifestSha256, context.Request.ParentManifestSha256);
                var modelConfigSha256=TrainingSourceProvenance.Identify(h.Start.ModelConfig).Sha256;
                Assert.Equal(modelConfigSha256,h.Start.RuntimeCompatibility.ConfigSha256);Assert.NotEqual(modelConfigSha256,h.Grant.ConfigurationSha256);
                Assert.Equal(h.Grant.EvaluatorSelection!.Evaluator.ConfigurationSha256,context.Request.ConfigurationSha256);
                Assert.Equal(h.Grant.ConfigurationSha256,context.Request.ConfigurationSha256);
                Assert.Equal(CodeDocumentationOneLessonV2Wire.SelectedStudentRuntimeSha256(h.Start.RuntimeCompatibility),context.Request.RuntimeCompatibilitySha256);
                Assert.Equal(context.Request.ModelStateBeforeSha256, context.ModelStateAfterSha256);
                Assert.Equal(CodeDocumentationOneLessonV2Wire.CaptureSelectedStudentSettings(b.Context.GenerationSettings), context.Request.GenerationSettings);
                using var tokenizerBytes = new MemoryStream(); b.Context.Tokenizer.Save(tokenizerBytes);
                Assert.Equal(HBytes(tokenizerBytes.ToArray()), context.Request.TokenizerSha256);
                Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting); Assert.Equal(4, h.Open.Session.ReadCapacityPrefixV2().Value!.PrefixFrames);
                Assert.Equal(1, entered); Assert.Equal(1, completed); Assert.Equal(1, final); Assert.False(context.FullTeacherContextReady);
                Assert.Equal(10, h.Client.Calls); Assert.Equal(history, h.HistoryBytes()[..history.Length]);
                var after = h.Files(); Assert.Equal(before.Keys.Order(), after.Keys.Order());
                foreach (var path in before.Keys.Where(p => p != h.Fixture.OrdinaryHistory)) { Assert.Equal(before[path].Bytes, after[path].Bytes); Assert.Equal(before[path].Ticks, after[path].Ticks); }
                responses.Add(context.StudentResponse);
            }
            finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedStudentContextCapturedForTesting = observer; }
        }
        Assert.Equal(new[] { "a", "b" }, responses);
    }

    [Fact]
    public async Task OrdinarySelectedStudent_CompletedReopenHasZeroGenerateAndRevalidatesBindings()
    {
        using var h = SelectedStudentHarness(); var (b, admission) = await BindSelectedStudent(h);
        var first = await ConsumeSelectedStudent(h, b, admission); Assert.True(first.IsSuccess, first.ErrorMessage);
        var bytes = h.HistoryBytes(); var files = h.Files(); Assert.True(h.Reopen().IsSuccess);
        var reopened = await h.Consume(); Assert.True(reopened.IsSuccess, reopened.ErrorMessage); b = reopened.Value!;
        h.Open.Session.SelectedStudentDurablePointForTesting = stage => { if (stage.Contains("generate", StringComparison.Ordinal)) throw new InvalidOperationException("Completed replay must never Generate"); };
        var replay = await ConsumeSelectedStudent(h, b, admission); Assert.True(replay.IsSuccess, replay.ErrorMessage);
        Assert.Equal(first.Value!.CopyStudentResponseUtf8(), replay.Value!.CopyStudentResponseUtf8());
        Assert.Equal(first.Value.Request, replay.Value.Request); Assert.Equal(bytes, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls); h.AssertFiles(files);
        var foreign = SelectedMaterialAdmission(h, b, new string('f',64), admission.Item.ItemId);
        Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, (await ConsumeSelectedStudent(h,b,foreign)).ErrorCode);
        var originalEvidence = b.Context.EvidenceRefs;
        try
        {
            SetDiagnosticBacking(b.Context, "EvidenceRefs", new[] { "late-stale-input" });
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await ConsumeSelectedStudent(h,b,admission)).ErrorCode);
        }
        finally { SetDiagnosticBacking(b.Context,"EvidenceRefs",originalEvidence); }
        Assert.True((await ConsumeSelectedStudent(h,b,admission)).IsSuccess);
        var path = h.Fixture.OrdinaryBaseline; var originalBytes = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path);
        var baselineAccounting = h.Open.Session.SelectedStudentAccounting; var replayCanary = h.Open.Session.SelectedStudentDurablePointForTesting;
        var corruptedBaseline = originalBytes.Concat(new byte[] { (byte)' ' }).ToArray();
        Exception? primary = null;
        try
        {
            RestoreCustodyFile(path, corruptedBaseline, ticks);
            // The existing prepared-baseline owner detects its changed committed bytes; Session preserves that precise refusal.
            var changed = await ConsumeSelectedStudent(h,b,admission); Assert.False(changed.IsSuccess);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, changed.ErrorCode); Assert.Null(changed.Value);
            Assert.Equal(corruptedBaseline,File.ReadAllBytes(path)); Assert.Equal(ticks,File.GetLastWriteTimeUtc(path));
            Assert.Equal(bytes,h.HistoryBytes()); Assert.Equal(baselineAccounting,h.Open.Session.SelectedStudentAccounting); Assert.Equal(10,h.Client.Calls);
            Assert.Same(replayCanary,h.Open.Session.SelectedStudentDurablePointForTesting);
            var rejectedFiles=h.Files(); Assert.Equal(files.Keys.Order(),rejectedFiles.Keys.Order());
            foreach(var owned in files.Keys.Where(p=>p!=path)) { Assert.Equal(files[owned].Bytes,rejectedFiles[owned].Bytes); Assert.Equal(files[owned].Ticks,rejectedFiles[owned].Ticks); }
        }
        catch(Exception ex) { primary=ex; throw; }
        finally { RestoreFixtureAfterFault(() => RestoreCustodyFile(path,originalBytes,ticks),primary); }
        var restoredBaseline=await ConsumeSelectedStudent(h,b,admission); Assert.True(restoredBaseline.IsSuccess,restoredBaseline.ErrorCode+":"+restoredBaseline.ErrorMessage);
        Assert.Equal(first.Value.Request,restoredBaseline.Value!.Request); Assert.Equal(first.Value.CopyStudentResponseUtf8(),restoredBaseline.Value.CopyStudentResponseUtf8());
        Assert.Equal(bytes,h.HistoryBytes()); Assert.Equal(baselineAccounting,h.Open.Session.SelectedStudentAccounting); h.AssertFiles(files); Assert.Equal(10,h.Client.Calls);
        var heldStart=(CodeDocumentationRepositoryOwnedModelCheckpointStartRequest)typeof(CodeDocumentationOneLessonSession)
            .GetField("_ordinaryStart",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.GetValue(h.Open.Session)!;
        Assert.NotSame(h.Start.ModelConfig,heldStart.ModelConfig);
        var config=heldStart.ModelConfig;var rope=config.RopeTheta;var accounting=h.Open.Session.SelectedStudentAccounting;
        try
        {
            typeof(ModelConfig).GetProperty(nameof(ModelConfig.RopeTheta))!.SetValue(config,rope+1f);
            Assert.NotEqual(TrainingSourceProvenance.Identify(config).Sha256,heldStart.RuntimeCompatibility.ConfigSha256);
            var changed=await ConsumeSelectedStudent(h,b,admission);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,changed.ErrorCode);Assert.Null(changed.Value);
            Assert.Equal(bytes,h.HistoryBytes());Assert.Equal(accounting,h.Open.Session.SelectedStudentAccounting);Assert.Equal(10,h.Client.Calls);
        }
        finally {typeof(ModelConfig).GetProperty(nameof(ModelConfig.RopeTheta))!.SetValue(config,rope);}
        var restoredConfig=await ConsumeSelectedStudent(h,b,admission);Assert.True(restoredConfig.IsSuccess,restoredConfig.ErrorCode+":"+restoredConfig.ErrorMessage);
        Assert.Equal(first.Value.Request,restoredConfig.Value!.Request);h.AssertFiles(files);Assert.Equal(bytes,h.HistoryBytes());Assert.Equal(10,h.Client.Calls);
        var lf=Encoding.UTF8.GetString(bytes).Split('\n').Take(4).Select(s=>Encoding.UTF8.GetBytes(s+"\n")).ToArray();
        var bp=h.Open.Session.Snapshot.Binding;var projection=new CodeDocumentationOneLessonBindingProjectionV2(bp.ObjectiveId,bp.GrantId,bp.GrantVersion,bp.TrainingRunId,bp.CandidateSha256,bp.ParentManifestSha256,bp.ParentWeightsSha256,bp.ConfigurationSha256,bp.ExamConditionSha256,bp.DescriptorsV2!);
        var savedRequest=CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(lf[2],projection).Value!.Request!;
        foreach(var fault in new[]{"task","tokenizer","settings","runtime","evaluator-config","parent","item","selection"})
        {
            var forged=savedRequest;
            if(fault=="task") forged=forged with {Task="Teach branch X",TaskSha256=H("Teach branch X")};
            if(fault=="tokenizer") forged=forged with {TokenizerSha256=new string('c',64)};
            if(fault=="runtime") forged=forged with {RuntimeCompatibilitySha256=new string('f',64)};
            if(fault=="evaluator-config") forged=forged with {ConfigurationSha256=new string('f',64)};
            if(fault=="settings") {var settings=forged.GenerationSettings with {Seed="1"};forged=forged with {GenerationSettings=settings,GenerationSettingsSha256=CodeDocumentationOneLessonV2Wire.SelectedStudentSettingsSha256(settings)};}
            if(fault=="parent") forged=forged with {ParentWeightsSha256=new string('d',64)};
            if(fault=="item") forged=forged with {ItemId="item-teach-7-0"};
            if(fault=="selection") forged=forged with {SelectionFrameSha256=new string('e',64)};
            var started=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentStartedFrameBytes(projection,h.Open.Session.Snapshot.SessionId,forged);
            Assert.True(CodeDocumentationOneLessonV2Wire.DecodeSelectedStudentStarted(started,projection).IsSuccess,fault+" structurally canonical control");
            var completed=CodeDocumentationOneLessonV2Wire.CanonicalSelectedStudentCompletedFrameBytes(projection,h.Open.Session.Snapshot.SessionId,forged,
                HBytes(started[..^1]),"b",forged.ModelStateBeforeSha256);
            var replacement=lf[0].Concat(lf[1]).Concat(started).Concat(completed).ToArray();var historyTicks=new DateTime(files[h.Fixture.OrdinaryHistory].Ticks,DateTimeKind.Utc);
            h.Open.Dispose();Exception? faultPrimary=null;
            try
            {
                RestoreCustodyFile(h.Fixture.OrdinaryHistory,replacement,historyTicks);var open=h.Reopen();
                if(fault is "task" or "tokenizer" or "settings" or "runtime")
                {
                    Assert.True(open.IsSuccess,fault+" must reach the genuine pure input validator after syntax");var original=await h.Consume();
                    Assert.True(original.IsSuccess,original.ErrorCode+":"+original.ErrorMessage);b=original.Value!;
                    h.Open.Session.SelectedStudentDurablePointForTesting=stage=>{if(stage.Contains("generate",StringComparison.Ordinal))throw new InvalidOperationException("Forged replay Generate");};
                    Assert.Equal("ONE_LESSON_SELECTED_STUDENT_INPUT_INVALID",(await ConsumeSelectedStudent(h,b,admission)).ErrorCode);
                }
                else Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,open.ErrorCode);
                Assert.Equal(replacement,h.HistoryBytes());Assert.Equal(10,h.Client.Calls);
            }
            catch(Exception ex){faultPrimary=ex;throw;}
            finally
            {
                h.Open.Dispose();RestoreFixtureAfterFault(()=>RestoreCustodyFile(h.Fixture.OrdinaryHistory,bytes,historyTicks),faultPrimary);
            }
            Assert.True(h.Reopen().IsSuccess);var restoredOriginal=await h.Consume();Assert.True(restoredOriginal.IsSuccess,restoredOriginal.ErrorCode+":"+restoredOriginal.ErrorMessage);b=restoredOriginal.Value!;
            h.Open.Session.SelectedStudentDurablePointForTesting=stage=>{if(stage.Contains("generate",StringComparison.Ordinal))throw new InvalidOperationException("Restored replay Generate");};
            Assert.True((await ConsumeSelectedStudent(h,b,admission)).IsSuccess);h.AssertFiles(files);Assert.Equal(10,h.Client.Calls);
        }
        h.Open.Dispose(); Assert.False((await ConsumeSelectedStudent(h,b,admission)).IsSuccess);
    }

    [Fact]
    public async Task OrdinarySelectedStudent_StartedFaultCancellationAndExceptionNeverRepeat()
    {
        using (var control = SelectedStudentHarness())
        {
            var (b,a) = await BindSelectedStudent(control);var received=await ConsumeSelectedStudent(control,b,a);
            Assert.True(received.IsSuccess,received.ErrorCode+":"+received.ErrorMessage);Assert.Equal("b",received.Value!.StudentResponse);
        }
        foreach(var fault in new[] { "student-started-before-append", "student-started-flushed", "student-started-readback", "cancel", "student-after-generate", "state", "torn-completed", "student-completed-before-append", "student-completed-flushed", "student-completed-readback" })
        {
            using var h=SelectedStudentHarness(); var (b,a)=await BindSelectedStudent(h); var prefix=h.HistoryBytes();
            using var cancel=new CancellationTokenSource(); var primary=new InvalidOperationException("genuine-selected-student-"+fault);
            var entered=0;var returned=0;var final=0;var fired=0;
            h.Open.Session.SelectedStudentDurablePointForTesting=stage=>
            {
                if(stage=="student-final-owner-checked") final++;
                if(stage=="student-before-generate") { entered++; if(fault=="cancel") {cancel.Cancel();fired++;} }
                if(stage=="student-after-generate") returned++;
                if(fault=="state" && stage=="student-after-generate")
                {
                    var operation=typeof(CodeDocumentationOneLessonSession).GetField("_activeSelectedStudentOperation",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(h.Open.Session)!;
                    var inputs=operation.GetType().GetField("inputs",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(operation)!;
                    var model=(LlmModel)inputs.GetType().GetProperty("Model")!.GetValue(inputs)!;
                    model.BaseParameters[0].Value.Data[0]+=1f;fired++; // Tests-only reflection changes the genuinely private model, never a production export or generated-response DTO.
                }
                if(fault=="torn-completed" && stage=="student-completed-flushed")
                {
                    var stream=(FileStream)typeof(CodeDocumentationOneLessonSession).GetField("_v2History",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(h.Open.Session)!;
                    stream.SetLength(stream.Length-1);stream.Flush(flushToDisk:true);fired++; // Deliberately remove the actual final LF under the existing held fixture handle.
                }
                if(stage==fault) { fired++; throw primary; }
            };
            if(fault=="cancel")
            {
                var actual=await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>ConsumeSelectedStudent(h,b,a,cancel.Token));
                Assert.Equal(cancel.Token,actual.CancellationToken);
            }
            else if(fault is "state" or "torn-completed") Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,(await ConsumeSelectedStudent(h,b,a)).ErrorCode);
            else Assert.Same(primary,await Assert.ThrowsAsync<InvalidOperationException>(()=>ConsumeSelectedStudent(h,b,a)));
            Assert.Equal(1,final); Assert.Equal(1,fired); Assert.Equal(fault.StartsWith("student-started",StringComparison.Ordinal)?0:1,entered);
            Assert.Equal(fault=="cancel"||fault.StartsWith("student-started",StringComparison.Ordinal)?0:1,returned);
            var after=h.HistoryBytes(); Assert.Equal(prefix,after[..prefix.Length]); Assert.Equal(10,h.Client.Calls);
            h.Open.Session.SelectedStudentDurablePointForTesting=null;
            if(fault=="student-completed-readback")
            {
                Assert.Equal((1,1,false),h.Open.Session.SelectedStudentAccounting);
                Assert.True((await ConsumeSelectedStudent(h,b,a)).IsSuccess); Assert.True(h.Reopen().IsSuccess);
            }
            else
            {
                Assert.Equal((1,0,true),h.Open.Session.SelectedStudentAccounting);
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,(await ConsumeSelectedStudent(h,b,a)).ErrorCode);
                var reopened=h.Reopen();
                if(fault=="student-started-before-append")
                {
                    Assert.True(reopened.IsSuccess);var original=await h.Consume();Assert.True(original.IsSuccess,original.ErrorCode+":"+original.ErrorMessage);var rb=original.Value!;
                    var received=await ConsumeSelectedStudent(h,rb,a);Assert.True(received.IsSuccess,received.ErrorCode+":"+received.ErrorMessage);
                    Assert.Equal("b",received.Value!.StudentResponse); // Zero-byte Started means Generate never occurred, so the exact durable two-frame owner can perform its first effect.
                }
                else if(fault=="student-completed-flushed")
                {
                    Assert.True(reopened.IsSuccess);var original=await h.Consume();Assert.True(original.IsSuccess,original.ErrorCode+":"+original.ErrorMessage);var rb=original.Value!;
                    h.Open.Session.SelectedStudentDurablePointForTesting=stage=>{if(stage.Contains("generate",StringComparison.Ordinal))throw new InvalidOperationException("Flushed Completed replay must not Generate");};
                    var received=await ConsumeSelectedStudent(h,rb,a);Assert.True(received.IsSuccess,received.ErrorCode+":"+received.ErrorMessage);
                    Assert.Equal("b",received.Value!.StudentResponse);
                }
                else Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,reopened.ErrorCode);
            }
            output.WriteLine("SELECTED_STUDENT_FAULT="+fault+" entered="+entered+" returned="+returned+" finalNone="+final);
        }
    }
    internal static OrdinaryDiagnosticHarness FreshFullMaterialHarness()
        => new((_, _) => (false, "missing_essential_behavior"), f => FullMaterialFixtureCurriculum(SelectionCategories(f.SealGenuineTeaching("material-unicode", 4))));

    private static object? FreshSessionField(CodeDocumentationOneLessonSession session, string name)
        => typeof(CodeDocumentationOneLessonSession).GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(session);

    [Fact]
    public async Task OrdinaryFreshFullMaterialSelection_ConsumesFamilyEntryAndRefusesSyntheticBeforeEffects()
    {
        using var h = FreshFullMaterialHarness(); var b = (await h.Consume()).Value!;
        var a = SelectionMaterialAdmissions(h, b); var files = h.Files(); var oldCapacity = h.Open.Session.CapacityV2;
        Assert.Null(FreshSessionField(h.Open.Session, "_fullMaterialLease")); Assert.False(File.Exists(h.Fixture.OrdinaryHistory));
        var raw = 0; var captures = 0; var calls = 0; var reservations = 0; var staged = 0;
        var previousRead = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        var previousCapture = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((context, _) =>
        {
            calls++; Assert.Equal(a.Select(x => x.Item.ItemId), context.Candidates.Select(x => x.ItemId));
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[^1])));
        });
        try
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = point => { if (point == "raw-before-open") raw++; };
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++;
            h.Open.Session.FullMaterialCapacityReservedForTesting = (_, _, _, _, _) => reservations++;
            h.Open.Session.SelectionMaterialPointForTesting = point =>
            {
                if (point == "fresh-after-stage") { staged++; Assert.Equal(3, h.Open.Session.CapacityV2!.MaximumFrames); Assert.Equal(57364, h.Open.Session.CapacityV2.MaximumBytes); }
            };
            var refused = await h.Open.Session.BindFreshOrdinaryFullMaterialSelectionAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, provider);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, refused.ErrorCode); Assert.Null(refused.Value);
            Assert.Equal(1, staged); Assert.Equal(0, raw); Assert.Equal(0, captures); Assert.Equal(0, calls); Assert.Equal(0, reservations);
            Assert.Same(oldCapacity, h.Open.Session.CapacityV2); Assert.Null(FreshSessionField(h.Open.Session, "_fullMaterialLease"));
            Assert.Equal((0, 0, false), h.Open.Session.FullMaterialSelectionAccounting); AssertFullMaterialNoEffects(h); h.AssertFiles(files);
            var selected = await h.Open.Session.BindFreshSyntheticFullMaterialSelectionForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, provider);
            Assert.True(selected.IsSuccess, selected.ErrorCode + ":" + selected.ErrorMessage); Assert.Equal(2, staged);
            Assert.Equal(a[^1].Item.ItemId, selected.Value!.Choice.ItemId); Assert.NotEqual(a[0].Item.ItemId, selected.Value.Choice.ItemId);
            Assert.Equal("SyntheticTest", selected.Value.Origin); Assert.False(selected.Value.FullTeacherContextReady);
            Assert.Equal(1, captures); Assert.Equal(1, calls); Assert.Equal(a.Length, reservations); Assert.True(raw > 0);
            var history = h.HistoryBytes(); Assert.Equal(3, FullMaterialLines(history).Length); Assert.Equal((1, 1, false), h.Open.Session.FullMaterialSelectionAccounting);
            var completedFiles = h.Files(); var binding = h.Open.Session.Snapshot.Binding; h.ReleaseSession();
            var root = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(h.Fixture.RepositoryRoot).Value!;
            var reopened = CodeDocumentationOneLessonSession.OpenSyntheticFullMaterialSelectionForTesting(root, binding, h.Catalog, h.Fixture.OrdinarySelector);
            Assert.True(reopened.IsSuccess, reopened.ErrorCode); using var owned = reopened.Value!;
            var rebound = await owned.Session.ReattachCompletedFullMaterialBaselineAsync(h.Catalog, h.Fixture.OrdinarySelector, h.Start, h.Context);
            Assert.True(rebound.IsSuccess, rebound.ErrorCode);
            var replay = await owned.Session.TryReadOrdinaryFullMaterialSelectionSyntheticForTestingAsync(rebound.Value!, h.Catalog, h.Fixture.OrdinarySelector, a);
            Assert.True(replay.IsSuccess, replay.ErrorCode); Assert.Equal(selected.Value.Context.CopyCanonicalBytes(), replay.Value!.Context.CopyCanonicalBytes());
            Assert.Equal(CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionPayloadBytes(selected.Value.Choice), CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionPayloadBytes(replay.Value.Choice));
            Assert.Equal(1, calls); Assert.Equal(history, h.HistoryBytes()); h.AssertFiles(completedFiles); AssertFullMaterialNoEffects(h);
        }
        finally
        {
            h.Open.Session.SelectionMaterialPointForTesting = null; h.Open.Session.FullMaterialCapacityReservedForTesting = null;
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = previousRead;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previousCapture;
        }
    }

    [Fact]
    public async Task OrdinaryFreshFullMaterialSelection_ExcludesBaselineAndMetadataCompetition()
    {
        using (var h = FreshFullMaterialHarness())
        {
            var b0 = (await h.Consume()).Value!; var a = SelectionMaterialAdmissions(h, b0); var witness = h.Inspect(b0).Value!; var oldCapacity = h.Open.Session.CapacityV2;
            var calls = 0; var item = a[^1].Item;
            var metadata = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((_, _) =>
            { calls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(new(item.ItemId, item.SourceId, item.ReferenceEntryId, item.SourceEntityFingerprint, "Synthetic explicit last task.", a[^1].EvidenceSha256))); });
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); using var release = new ManualResetEventSlim();
            var callbackRefusals = 0;
            h.Open.Session.SelectionMaterialPointForTesting = point =>
            {
                if (point != "fresh-after-stage") return;
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, h.Consume().GetAwaiter().GetResult().ErrorCode);
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, h.Open.Session.BindOrdinarySelectionDecisionSyntheticForTestingAsync(b0, h.Catalog, h.Fixture.OrdinarySelector, witness, metadata).GetAwaiter().GetResult().ErrorCode);
                callbackRefusals += 2; entered.SetResult(); Assert.True(release.Wait(TimeSpan.FromSeconds(20)));
            };
            var running = Task.Run(() => h.Open.Session.BindFreshOrdinaryFullMaterialSelectionAsync(b0, h.Catalog, h.Fixture.OrdinarySelector, a, metadata));
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(20));
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await h.Consume()).ErrorCode);
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await h.Open.Session.BindOrdinarySelectionDecisionAsync(b0, h.Catalog, h.Fixture.OrdinarySelector, witness, metadata)).ErrorCode);
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, h.Open.Session.CompleteEarlyBaselineSufficient(b0, null!, null!, h.Catalog, h.Fixture.OrdinarySelector, null!, h.Start.RuntimeCompatibility, CancellationToken.None).ErrorCode);
                Assert.Same(b0, FreshSessionField(h.Open.Session, "_ordinaryBaseline")); Assert.Equal(0, calls); Assert.False(File.Exists(h.Fixture.OrdinaryHistory));
            }
            finally { release.Set(); await running.WaitAsync(TimeSpan.FromSeconds(20)); h.Open.Session.SelectionMaterialPointForTesting = null; }
            Assert.Equal(2, callbackRefusals); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, (await running).ErrorCode);
            Assert.Same(oldCapacity, h.Open.Session.CapacityV2); var b1 = (await h.Consume()).Value!; Assert.NotSame(b0, b1); Assert.Equal(10, h.Client.Calls);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await h.Open.Session.BindFreshOrdinaryFullMaterialSelectionAsync(b0, h.Catalog, h.Fixture.OrdinarySelector, a, metadata)).ErrorCode);
            var legacyEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); using var legacyRelease = new ManualResetEventSlim();
            var currentWitness = h.Inspect(b1).Value!; var before = h.Files();
            h.Open.Session.SelectionDurablePointForTesting = point => { if (point == "before-genesis") { legacyEntered.SetResult(); Assert.True(legacyRelease.Wait(TimeSpan.FromSeconds(20))); } };
            var legacy = Task.Run(() => h.Open.Session.BindOrdinarySelectionDecisionSyntheticForTestingAsync(b1, h.Catalog, h.Fixture.OrdinarySelector, currentWitness, metadata));
            try
            {
                await legacyEntered.Task.WaitAsync(TimeSpan.FromSeconds(20));
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await h.Open.Session.BindFreshOrdinaryFullMaterialSelectionAsync(b1, h.Catalog, h.Fixture.OrdinarySelector, a, metadata)).ErrorCode);
                Assert.Null(FreshSessionField(h.Open.Session, "_fullMaterialLease")); Assert.Same(oldCapacity, h.Open.Session.CapacityV2); Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); h.AssertFiles(before);
            }
            finally { legacyRelease.Set(); await legacy.WaitAsync(TimeSpan.FromSeconds(20)); h.Open.Session.SelectionDurablePointForTesting = null; }
            Assert.True((await legacy).IsSuccess, (await legacy).ErrorCode); Assert.Equal(1, calls); Assert.Equal(2, FullMaterialLines(h.HistoryBytes()).Length);
        }
        using (var h = new OrdinaryDiagnosticHarness((_, _) => (true, "correct")))
        {
            var b = (await h.Consume()).Value!; var batch = new CodeDocumentationBatchFileRunner();
            var root = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(h.Fixture.RepositoryRoot).Value!;
            var material = batch.OpenProfileInputsFresh(h.Catalog, h.Fixture.OrdinarySelector, root); Assert.True(material.IsSuccess, material.ErrorCode);
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); using var release = new ManualResetEventSlim();
            h.Open.Session.SelectionDurablePointForTesting = point => { if (point == "early-sufficient-entered") { entered.SetResult(); Assert.True(release.Wait(TimeSpan.FromSeconds(20))); } };
            var completing = Task.Run(() => h.Open.Session.CompleteEarlyBaselineSufficient(b, batch, material.Value!, h.Catalog, h.Fixture.OrdinarySelector, root, h.Start.RuntimeCompatibility, CancellationToken.None));
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(20));
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await h.Open.Session.BindFreshOrdinaryFullMaterialSelectionAsync(b, h.Catalog, h.Fixture.OrdinarySelector, null, null)).ErrorCode);
                Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); Assert.Null(FreshSessionField(h.Open.Session, "_fullMaterialLease"));
            }
            finally { release.Set(); await completing.WaitAsync(TimeSpan.FromSeconds(20)); h.Open.Session.SelectionDurablePointForTesting = null; }
            Assert.True((await completing).IsSuccess, (await completing).ErrorCode); Assert.NotNull(h.Open.Session.EarlyOutcome);
        }
    }

    [Fact]
    public async Task OrdinaryFreshFullMaterialSelection_PreEffectCancellationAndDisposeRestoreOnlyOwnedState()
    {
        foreach (var scenario in new[] { "before-stage", "after-stage", "dispose", "receiving-changed" })
        {
            using var h = FreshFullMaterialHarness(); var b = (await h.Consume()).Value!; var a = SelectionMaterialAdmissions(h, b);
            var capacity = h.Open.Session.CapacityV2; var files = h.Files(); var evidence = b.Context.EvidenceRefs;
            using var cancellation = new CancellationTokenSource();
            var primary = new OperationCanceledException("fresh " + scenario, null, cancellation.Token); var secondary = new IOException("fresh final check sentinel");
            var finals = new List<string>(); var expectedFinals = new[] { "baseline-reference-final-none", "receiving-final-none", "original-before-final-none", "source-reopen-final-none", "original-after-final-none" };
            h.Open.Session.SelectionMaterialPointForTesting = point =>
            {
                if (expectedFinals.Contains(point, StringComparer.Ordinal)) finals.Add(point);
                if (point == "final-none-checked") throw secondary;
                if (point != (scenario == "before-stage" ? "fresh-before-stage" : "fresh-after-stage")) return;
                if (scenario == "dispose") h.Open.Session.Dispose();
                if (scenario == "receiving-changed") SetDiagnosticBacking(b.Context, "EvidenceRefs", new[] { "fresh late receiving change" });
                cancellation.Cancel(); throw primary;
            };
            try
            {
                var actual = await Assert.ThrowsAsync<OperationCanceledException>(() => h.Open.Session.BindFreshOrdinaryFullMaterialSelectionAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, null, cancellation.Token));
                Assert.Same(primary, actual); Assert.Equal(cancellation.Token, actual.CancellationToken); Assert.Same(secondary, actual.Data["SelectionMaterialFinalFailure"]);
                Assert.Equal(expectedFinals, finals); Assert.Equal(0, (int)FreshSessionField(h.Open.Session, "_selectionMaterialInFlight")!);
                Assert.Null(FreshSessionField(h.Open.Session, "_selectionMaterialReceipt")); Assert.Null(FreshSessionField(h.Open.Session, "_fullMaterialDispatch"));
                Assert.False(File.Exists(h.Fixture.OrdinaryHistory)); Assert.Null(FreshSessionField(h.Open.Session, "_v2History")); h.AssertFiles(files); AssertFullMaterialNoEffects(h);
                if (scenario == "receiving-changed")
                {
                    Assert.NotSame(capacity, h.Open.Session.CapacityV2); Assert.NotNull(FreshSessionField(h.Open.Session, "_fullMaterialLease"));
                    Assert.Equal((1, 0, true), h.Open.Session.FullMaterialSelectionAccounting); Assert.NotNull(actual.Data["SelectionMaterialFinalReceivingError"]);
                }
                else { Assert.Same(capacity, h.Open.Session.CapacityV2); Assert.Null(FreshSessionField(h.Open.Session, "_fullMaterialLease")); }
                if (scenario == "dispose")
                {
                    Assert.True((bool)FreshSessionField(h.Open.Session, "_disposed")!); Assert.True(((FileStream)FreshSessionField(h.Open.Session, "_ownerLock")!).SafeFileHandle.IsClosed);
                    Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await h.Consume()).ErrorCode);
                }
            }
            finally { h.Open.Session.SelectionMaterialPointForTesting = null; SetDiagnosticBacking(b.Context, "EvidenceRefs", evidence); }
            if (scenario is "before-stage" or "after-stage")
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, (await h.Open.Session.BindFreshOrdinaryFullMaterialSelectionAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, null)).ErrorCode);
            if (scenario == "receiving-changed") Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await h.Open.Session.BindFreshOrdinaryFullMaterialSelectionAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, null)).ErrorCode);
            output.WriteLine("FRESH_CANCELLATION scenario=" + scenario + " fiveNone=" + finals.Count + " gate=" + FreshSessionField(h.Open.Session, "_selectionMaterialInFlight"));
        }
    }

    [Fact]
    public async Task OrdinaryFreshFullMaterialSelection_AppendFailureRetainsUnknownFamily()
    {
        using var h = FreshFullMaterialHarness(); var b = (await h.Consume()).Value!; var a = SelectionMaterialAdmissions(h, b);
        var packet = IndependentlyExpectedFullMaterialPacket(h, b, a); var request = IndependentAsciiRequestJson(IndependentlyExpectedFullMaterialRequest(h, b, a, 32));
        Assert.Equal(13761, packet.Length); Assert.Equal(20999, request.Length); // Complete declared graph, including both eligible candidates and every used/unused SOURCE row.
        var capacity = h.Open.Session.CapacityV2; var files = h.Files(); var calls = 0; var attempts = 0;
        var reservations = new List<(string Item, int Request, int Genesis, int Started, int Completed)>();
        var finals = new List<string>(); var expectedFinals = new[] { "baseline-reference-final-none", "receiving-final-none", "original-before-final-none", "source-reopen-final-none", "original-after-final-none" };
        var appendFault = new IOException("actual first append fault after empty history creation");
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((context, _) =>
        { calls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[^1]))); });
        h.Open.Session.FullMaterialCapacityReservedForTesting = (item, requestBytes, genesis, started, completed) => reservations.Add((item, requestBytes, genesis, started, completed));
        h.Open.Session.SelectionMaterialPointForTesting = point => { if (expectedFinals.Contains(point, StringComparer.Ordinal)) finals.Add(point); };
        h.Open.Session.SelectionDurablePointForTesting = point =>
        {
            if (point != "full-material-genesis-before-write") return;
            attempts++; Assert.True(File.Exists(h.Fixture.OrdinaryHistory)); Assert.Empty(h.HistoryBytes()); Assert.Equal(0, h.Open.Session.CapacityV2!.PrefixFrames);
            var heldHistory = Assert.IsType<FileStream>(FreshSessionField(h.Open.Session, "_v2History")); Assert.Equal(0, heldHistory.Length);
            Assert.Empty(Assert.IsType<byte[]>(FreshSessionField(h.Open.Session, "_v2Prefix"))); Assert.Null(FreshSessionField(h.Open.Session, "_fullMaterialExpectedPrefix"));
            Assert.True((bool)FreshSessionField(h.Open.Session, "_fullMaterialPoisoned")!); Assert.NotNull(FreshSessionField(h.Open.Session, "_fullMaterialLease"));
            finals.Clear(); // Distinguish the five post-fault None checks from the successful preparation checks.
            output.WriteLine("FRESH_APPEND_FAULT point=" + point + " attempts=" + attempts + " history=" + heldHistory.Length + " frames=" + h.Open.Session.CapacityV2.PrefixFrames + " type=" + appendFault.GetType().Name + " message=" + appendFault.Message);
            throw appendFault;
        };
        try
        {
            var failed = await h.Open.Session.BindFreshSyntheticFullMaterialSelectionForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, provider);
            output.WriteLine("FRESH_APPEND_RESULT error=" + failed.ErrorCode + " attempts=" + attempts + " reservations=" + reservations.Count + " callbacks=" + calls + " packet=" + packet.Length + " request=" + request.Length + " fiveNone=" + finals.Count + " receiving=" + h.Open.Session.SelectionMaterialReceivingFinalErrorForTesting + " baseline=" + h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting + " source=" + h.Open.Session.SelectionMaterialSourceFinalErrorForTesting);
            foreach (var reserve in reservations)
                output.WriteLine("FRESH_APPEND_RESERVE item=" + reserve.Item + " request=" + reserve.Request + " genesis=" + reserve.Genesis + " started=" + reserve.Started + " completed=" + reserve.Completed + " total=" + (reserve.Genesis + reserve.Started + reserve.Completed));
            Assert.Equal(1, attempts); Assert.Equal(a.Select(x => x.Item.ItemId), reservations.Select(x => x.Item));
            Assert.All(reservations, reserve =>
            {
                Assert.Equal(request.Length, reserve.Request); Assert.Equal(2102, reserve.Genesis); Assert.Equal(2375, reserve.Started); Assert.Equal(37566, reserve.Completed);
                Assert.InRange(reserve.Request, 1, 25731); Assert.InRange(reserve.Genesis, 2, 4096); Assert.InRange(reserve.Started, 2, 8192);
                Assert.InRange(reserve.Completed, 2, 45076); Assert.InRange(reserve.Genesis + reserve.Started + reserve.Completed, 6, 57364);
            });
            Assert.Equal(expectedFinals, finals); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, failed.ErrorCode);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, h.Open.Session.SelectionMaterialReceivingFinalErrorForTesting);
            Assert.Null(h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting); Assert.Null(h.Open.Session.SelectionMaterialSourceFinalErrorForTesting);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, h.Open.Session.SelectionMaterialSecondaryErrorForTesting);
            h.Open.Session.SelectionMaterialPointForTesting = null;
            Assert.Equal(0, calls); Assert.NotSame(capacity, h.Open.Session.CapacityV2); Assert.Equal(0, h.Open.Session.CapacityV2!.PrefixFrames);
            Assert.NotNull(FreshSessionField(h.Open.Session, "_fullMaterialLease")); Assert.Equal((1, 0, true), h.Open.Session.FullMaterialSelectionAccounting);
            Assert.True(File.Exists(h.Fixture.OrdinaryHistory)); Assert.Empty(h.HistoryBytes()); Assert.Equal(0, (int)FreshSessionField(h.Open.Session, "_selectionMaterialInFlight")!);
            Assert.Null(FreshSessionField(h.Open.Session, "_selectionMaterialReceipt")); Assert.Null(FreshSessionField(h.Open.Session, "_fullMaterialDispatch"));
            AssertFullMaterialOtherFiles(h, files); AssertFullMaterialNoEffects(h); var unknownFiles = h.Files();
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await h.Open.Session.BindFreshSyntheticFullMaterialSelectionForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, provider)).ErrorCode);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, (await h.Open.Session.BindOrdinaryFullMaterialSelectionSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, provider)).ErrorCode);
            Assert.Equal(1, attempts); Assert.Equal(a.Length, reservations.Count); Assert.Equal(0, calls); h.AssertFiles(unknownFiles);
            var heldOwnerLock = Assert.IsType<FileStream>(FreshSessionField(h.Open.Session, "_ownerLock")); var ownerLockPath = Assert.IsType<string>(FreshSessionField(h.Open.Session, "_lockPath"));
            var ownerLockBytes = new byte[checked((int)heldOwnerLock.Length)]; var ownerLockPosition = heldOwnerLock.Position;
            try { heldOwnerLock.Position = 0; heldOwnerLock.ReadExactly(ownerLockBytes); }
            finally { heldOwnerLock.Position = ownerLockPosition; }
            var reopened = h.Reopen(); var retainedHistory = h.HistoryBytes(); var reopenedOwnerLockBytes = File.ReadAllBytes(ownerLockPath);
            output.WriteLine("FRESH_APPEND_REOPEN success=" + reopened.IsSuccess + " error=" + reopened.ErrorCode + " historyExists=" + File.Exists(h.Fixture.OrdinaryHistory) + " history=" + retainedHistory.Length + " attempts=" + attempts + " reservations=" + reservations.Count + " callbacks=" + calls + " ownerLockPreserved=" + ownerLockBytes.AsSpan().SequenceEqual(reopenedOwnerLockBytes));
            h.AssertFiles(unknownFiles); Assert.Equal(ownerLockBytes, reopenedOwnerLockBytes); // Inspect all owned file bytes/timestamps at opener exit, including actual lock bytes after its refused-open handles close.
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, reopened.ErrorCode); Assert.True(File.Exists(h.Fixture.OrdinaryHistory)); Assert.Empty(retainedHistory);
            Assert.Equal(1, attempts); Assert.Equal(a.Length, reservations.Count); Assert.Equal(0, calls); Assert.Equal((1, 0, true), h.Open.Session.FullMaterialSelectionAccounting); AssertFullMaterialNoEffects(h);
        }
        finally { h.Open.Session.SelectionDurablePointForTesting = null; h.Open.Session.FullMaterialCapacityReservedForTesting = null; h.Open.Session.SelectionMaterialPointForTesting = null; }

        // A separate genuine fresh owner with the same whole declaration succeeds when the first append fault is absent; the poisoned owner above is never retried.
        using var control = FreshFullMaterialHarness(); var controlBaseline = (await control.Consume()).Value!; var controlAdmissions = SelectionMaterialAdmissions(control, controlBaseline);
        var controlPacket = IndependentlyExpectedFullMaterialPacket(control, controlBaseline, controlAdmissions); var controlFiles = control.Files(); var controlCalls = 0;
        Assert.Equal(packet.Length, controlPacket.Length); Assert.Equal(a.Length, controlAdmissions.Length);
        var controlProvider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((context, _) =>
        {
            controlCalls++; Assert.Equal(controlPacket, context.CopyCanonicalBytes()); Assert.Equal(controlAdmissions.Select(x => x.Item.ItemId), context.Candidates.Select(x => x.ItemId));
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[^1])));
        });
        var selected = await control.Open.Session.BindFreshSyntheticFullMaterialSelectionForTestingAsync(controlBaseline, control.Catalog, control.Fixture.OrdinarySelector, controlAdmissions, controlProvider);
        output.WriteLine("FRESH_APPEND_CONTROL success=" + selected.IsSuccess + " error=" + selected.ErrorCode + " callbacks=" + controlCalls + " packet=" + controlPacket.Length + " frames=" + control.Open.Session.CapacityV2!.PrefixFrames);
        Assert.True(selected.IsSuccess, selected.ErrorCode + ":" + selected.ErrorMessage); Assert.Equal(1, controlCalls); Assert.Equal((1, 1, false), control.Open.Session.FullMaterialSelectionAccounting);
        Assert.Equal(3, FullMaterialLines(control.HistoryBytes()).Length); AssertFullMaterialOtherFiles(control, controlFiles); AssertFullMaterialNoEffects(control);
    }
    private static string HBytes(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    internal static CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission[] SelectionMaterialAdmissions(
        OrdinaryDiagnosticHarness h, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline)
    {
        var witness = h.Inspect(baseline).Value!; var categories = h.Classify(baseline).Value!.Categories;
        var eligible = witness.TeachingIdentities.CategoryBindings.Where(c => categories.Contains(c.Category, StringComparer.Ordinal))
            .SelectMany(c => c.OrderedItemIds).ToHashSet(StringComparer.Ordinal);
        return witness.TeachingIdentities.Items.Where(i => eligible.Contains(i.DeclaredItem.ItemId))
            .Select(i => SelectedMaterialAdmission(h, baseline, itemId: i.DeclaredItem.ItemId)).ToArray();
    }
    private static CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice SelectionMaterialChoice(
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCandidate item, string reason = "Synthetic fixture inspected complete permitted candidate bytes.")
        => new(item.ItemId, item.SourceId, item.ReferenceEntryId, item.SourceEntityFingerprint, reason, item.MaterialAdmissionEvidenceSha256);

    private static CodeDocumentationOneLessonTeachingCurriculumV2 FullMaterialFixtureCurriculum(CodeDocumentationOneLessonTeachingCurriculumV2 complete)
    {
        // Define this finite fixture before grant sealing: one declared genuine corpus, both eligible items and all its used/unused rows.
        // Production never drops candidates/sources from an already frozen selection. Predecessor fixture declarations remain unchanged.
        var source = complete.Sources[0]; var items = complete.Items.Where(i => i.SourceId==source.SourceId).ToArray(); var ids = items.Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        return complete with { Sources = new[]{source}, Items = items,
            CategoryBindings = complete.CategoryBindings.Select(c => c with { OrderedItemIds = c.OrderedItemIds.Where(ids.Contains).ToArray() }).ToArray() };
    }
    private static OrdinaryDiagnosticHarness FullMaterialHarness(int teachingRows = 4)
        => new((_, _) => (false,"missing_essential_behavior"), f => FullMaterialFixtureCurriculum(SelectionCategories(f.SealGenuineTeaching("material-unicode",teachingRows))), syntheticFullMaterialMode:true);
    private static OrdinaryDiagnosticHarness FullMaterialTaskHarness(string task)
        => new((_,_) => (false,"missing_essential_behavior"), f =>
        {
            // The adjustable task is genuine owner input before the corpus/grant seal, never an imported prepared DTO or an after-seal text mutation.
            var curriculum = FullMaterialFixtureCurriculum(SelectionCategories(f.SealGenuineTeaching("saved-answer",savedAnswer:task)));
            var descriptor = curriculum.Sources.Single(s => s.SourceId=="teach-7");
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(f.RepositoryRoot,descriptor.DomainSlug,descriptor.SubjectSlug,descriptor.CorpusVersion).Value!;
            var entries = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address).Value!.Corpus.Entries;
            var exact = entries.Single(e => e.OwnerInput==task); var other = entries.First(e => e.EntryId!=exact.EntryId);
            return curriculum with { Items = curriculum.Items.Select(i => i.SourceId=="teach-7" ?
                i with { ReferenceEntryId = (i.ItemId=="item-teach-7-1"?exact:other).EntryId,
                    SourceEntityFingerprint = (i.ItemId=="item-teach-7-1"?exact:other).SourceEntityFingerprint } : i).ToArray() };
        },syntheticFullMaterialMode:true);
    private static CodeDocumentationFullMaterialRequestV1 IndependentlyExpectedFullMaterialRequest(OrdinaryDiagnosticHarness h,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission[] a, int invocationLength)
    {
        var m = h.Classify(b).Value!; var graph = h.Inspect(b).Value!;
        var metadata = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(m.OriginalReceiptId,m.OriginalReceiptSha256,m.Categories,graph.TeachingIdentities).Value!;
        var packet = IndependentlyExpectedFullMaterialPacket(h,b,a); var projection = FullMaterialProjection(h);
        var genesis = CodeDocumentationOneLessonV2Wire.FullMaterialGenesisBytes(projection,h.Open.Session.Snapshot.SessionId,m.OriginalReceiptId,m.OriginalReceiptSha256);
        return new("code-documentation-full-material-selection-v1","SyntheticTest","gpt-6-astra","ultra",new string('i',invocationLength),HBytes(genesis[..^1]),
            graph.BindingSha256,Convert.ToBase64String(CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(projection))),
            Convert.ToBase64String(packet),packet.Length,HBytes(packet),m.OriginalReceiptId,m.OriginalReceiptSha256,metadata.SourceSha256,metadata.FrozenContextSha256,m.Categories.ToArray());
    }
    private static byte[] IndependentAsciiRequestJson(CodeDocumentationFullMaterialRequestV1 request)
        => IndependentAsciiCanonicalJson(JsonSerializer.SerializeToElement(request));
    private static byte[] IndependentAsciiCanonicalJson(JsonElement root)
    {
        // Independently encode whole request/frame JSON, including zero-effects and outcome booleans; the raw binary packet is never decoded as UTF8/JSON.
        string Write(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.Object => "{"+string.Join(",",value.EnumerateObject().OrderBy(p => p.Name,StringComparer.Ordinal).Select(p => JsonSerializer.Serialize(p.Name)+":"+Write(p.Value)))+"}",
            JsonValueKind.Array => "["+string.Join(",",value.EnumerateArray().Select(Write))+"]",
            JsonValueKind.String => JsonSerializer.Serialize(value.GetString(),new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true", JsonValueKind.False => "false", JsonValueKind.Null => "null",
            _ => throw new InvalidOperationException("Unsupported JSON kind in the independent canonical encoder.")
        };
        return Encoding.UTF8.GetBytes(Write(root));
    }
    private static Task<OperationResult<CodeDocumentationOneLessonSession.FullMaterialSelectionReadback>> BindFullMaterial(
        OrdinaryDiagnosticHarness h, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b,
        IReadOnlyList<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission>? admissions,
        CodeDocumentationOneLessonSelectionDecisionProvider? provider, CancellationToken token = default)
        => h.Open.Session.BindOrdinaryFullMaterialSelectionSyntheticForTestingAsync(b,h.Catalog,h.Fixture.OrdinarySelector,admissions,provider,token);
    private static byte[][] FullMaterialLines(byte[] bytes)
        => Encoding.UTF8.GetString(bytes).Split('\n',StringSplitOptions.RemoveEmptyEntries).Select(s => Encoding.UTF8.GetBytes(s+"\n")).ToArray();
    private static CodeDocumentationOneLessonBindingProjectionV2 FullMaterialProjection(OrdinaryDiagnosticHarness h)
    {
        var b = h.Open.Session.Snapshot.Binding;
        return new(b.ObjectiveId,b.GrantId,b.GrantVersion,b.TrainingRunId,b.CandidateSha256,b.ParentManifestSha256,b.ParentWeightsSha256,b.ConfigurationSha256,b.ExamConditionSha256,b.DescriptorsV2!);
    }
    private static void AssertFullMaterialNoEffects(OrdinaryDiagnosticHarness h)
    { Assert.Equal(10,h.Client.Calls); Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),h.Open.Session.Snapshot.Effects); }
    private static void AssertFullMaterialOtherFiles(OrdinaryDiagnosticHarness h, Dictionary<string,(byte[] Bytes,long Ticks)> before)
    {
        var after = h.Files(); var expected = before.Keys.Where(p => p != h.Fixture.OrdinaryHistory).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expected,after.Keys.Where(p => p != h.Fixture.OrdinaryHistory).Order(StringComparer.Ordinal).ToArray());
        foreach(var path in expected) { Assert.Equal(before[path].Bytes,after[path].Bytes); Assert.Equal(before[path].Ticks,after[path].Ticks); }
    }
    private static byte[] IndependentlyExpectedFullMaterialPacket(OrdinaryDiagnosticHarness h,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission[] admissions)
    {
        // Read genuine sealed owner inputs before the consumer, then independently encode every ordered tuple, including unchosen items.
        var graph = h.Inspect(b).Value!; var diagnosis = h.Classify(b).Value!;
        var metadata = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.OriginalReceiptId,diagnosis.OriginalReceiptSha256,diagnosis.Categories,graph.TeachingIdentities).Value!;
        using var buffer = new MemoryStream(); using var writer = new BinaryWriter(buffer,new UTF8Encoding(false,true),true);
        void Bytes(byte[] value) { writer.Write(value.Length); writer.Write(value); }
        void Field(string value) => Bytes(Encoding.UTF8.GetBytes(value));
        writer.Write(Encoding.UTF8.GetBytes("code-documentation-selection-material-input-v1\n"));
        Field("Judge task-specific pedagogical suitability first. Use the supplied curriculum order only to break otherwise equivalent eligible choices.");
        Field(graph.BindingSha256); Bytes(CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(FullMaterialProjection(h)))); Bytes(metadata.CopyBytes());
        writer.Write(diagnosis.Categories.Count); foreach(var category in diagnosis.Categories) Field(category);
        writer.Write(admissions.Length);
        foreach(var a in admissions)
        {
            var descriptor = graph.TeachingIdentities.Sources.Single(s => s.Descriptor.SourceId == a.Item.SourceId).Descriptor;
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,descriptor.DomainSlug,descriptor.SubjectSlug,descriptor.CorpusVersion).Value!;
            var entry = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address).Value!.Corpus.Entries.Single(e => e.EntryId == a.Item.ReferenceEntryId);
            Field(a.Item.ItemId); Field(a.Item.SourceId); Field(a.Item.ReferenceEntryId); Field(a.Item.SourceEntityFingerprint);
            Field(entry.OwnerInput); Field(entry.PaidModelReferenceAnswer); Field(entry.PaidReferenceProvenance.CanonicalRepresentation); Field(a.EvidenceSha256);
        }
        writer.Flush(); return buffer.ToArray();
    }

    [Fact]
    public async Task OrdinaryFullMaterialSelection_PersistsCompleteChoiceAndReplaysWithoutProvider()
    {
        using var h = FullMaterialHarness(); var b = (await h.Consume()).Value!; var a = SelectionMaterialAdmissions(h,b);
        var expected = IndependentlyExpectedFullMaterialPacket(h,b,a); var files = h.Files(); var calls = 0;
        var completeReservations = new List<(string Item,int Request,int Genesis,int Started,int Completed)>();
        h.Open.Session.FullMaterialCapacityReservedForTesting = (item,request,genesis,started,completed) => completeReservations.Add((item,request,genesis,started,completed));
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((context,_) =>
        {
            calls++; Assert.Equal(expected,context.CopyCanonicalBytes()); Assert.Equal(a.Select(x => x.Item.ItemId),context.Candidates.Select(x => x.ItemId));
            Assert.All(context.Candidates,c => { Assert.Contains("中文",c.OwnerInput); Assert.Contains("中文",c.PaidModelReferenceAnswer); Assert.NotNull(c.PaidReferenceProvenance); });
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[^1],"Full Synthetic reasoning selected the last eligible task after inspecting every permitted reference.")));
        });
        var selected = await BindFullMaterial(h,b,a,provider); Assert.True(selected.IsSuccess,selected.ErrorCode+":"+selected.ErrorMessage); Assert.Equal(1,calls);
        Assert.Equal(a[^1].Item.ItemId,selected.Value!.Choice.ItemId); Assert.NotEqual(a[0].Item.ItemId,selected.Value.Choice.ItemId);
        Assert.Equal("SyntheticTest",selected.Value.Origin); Assert.False(selected.Value.FullTeacherContextReady); Assert.False(h.Open.Session.HasSavedSelectionBinding);
        var history = h.HistoryBytes(); var lines = FullMaterialLines(history); Assert.Equal(3,lines.Length);
        using var genesis = JsonDocument.Parse(lines[0]); using var started = JsonDocument.Parse(lines[1]); using var completed = JsonDocument.Parse(lines[2]);
        // Independent literal hashing excludes physical LF; sharing the production writer/hash helper would conceal a framing error.
        Assert.Equal(HBytes(lines[0][..^1]),started.RootElement.GetProperty("PreviousFrameSha256").GetString());
        Assert.Equal(HBytes(lines[1][..^1]),completed.RootElement.GetProperty("PreviousFrameSha256").GetString());
        Assert.Equal(HBytes(lines[1][..^1]),completed.RootElement.GetProperty("FullMaterialSelection").GetProperty("StartedFrameSha256").GetString());
        var savedBytes = Convert.FromBase64String(completed.RootElement.GetProperty("FullMaterialSelection").GetProperty("RequestCanonicalBase64").GetString()!);
        var request = JsonSerializer.Deserialize<CodeDocumentationFullMaterialRequestV1>(savedBytes)!;
        Assert.Equal(expected,Convert.FromBase64String(request.PacketCanonicalBase64)); Assert.Equal(expected.Length,request.PacketByteLength);
        Assert.Equal(expected,selected.Value!.Context.CopyCanonicalBytes()); Assert.Equal(HBytes(expected),request.PacketSha256);
        Assert.Equal(savedBytes,IndependentAsciiRequestJson(request)); Assert.InRange(expected.Length,1,16384); Assert.InRange(savedBytes.Length,1,25731);
        Assert.Equal(a.Select(x => x.Item.ItemId),completeReservations.Select(x => x.Item));
        foreach(var reserve in completeReservations)
        {
            Assert.Equal(savedBytes.Length,reserve.Request); Assert.InRange(reserve.Genesis,2,4096); Assert.InRange(reserve.Started,2,8192);
            Assert.InRange(reserve.Completed,2,45076); Assert.InRange(reserve.Genesis+reserve.Started+reserve.Completed,6,57364);
            output.WriteLine("FULL_COMPLETE_RESERVE item="+reserve.Item+" request="+reserve.Request+" genesis="+reserve.Genesis+" started="+reserve.Started+" completed="+reserve.Completed+" total="+(reserve.Genesis+reserve.Started+reserve.Completed));
        }
        output.WriteLine("FULL_COMPLETE_BYTES context="+selected.Value.Context.CopyCanonicalBytes().Length+" independentPacket="+expected.Length+" declared="+request.PacketByteLength+" decoded="+Convert.FromBase64String(request.PacketCanonicalBase64).Length+" binding="+Convert.FromBase64String(request.BindingCanonicalBase64).Length+" request="+savedBytes.Length+" genesis="+lines[0].Length+" started="+lines[1].Length+" completed="+lines[2].Length+" total="+history.Length+" packetSha="+request.PacketSha256+" requestSha="+HBytes(savedBytes));
        h.Open.Session.FullMaterialCapacityReservedForTesting = null;
        Assert.Equal(HBytes(savedBytes),started.RootElement.GetProperty("FullMaterialSelection").GetProperty("RequestSha256").GetString());
        Assert.Equal(HBytes(lines[0][..^1]),request.GenesisFrameSha256); AssertFullMaterialOtherFiles(h,files); AssertFullMaterialNoEffects(h);
        Assert.Equal((1,1,false),h.Open.Session.FullMaterialSelectionAccounting);
        var completedFiles = h.Files(); var formerSession = h.Open.Session; Assert.True(h.Reopen().IsSuccess); Assert.NotSame(formerSession,h.Open.Session);
        var raw = 0; var observer = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => raw++;
            Assert.False((await BindFullMaterial(h,b,a,null)).IsSuccess); Assert.Equal(0,raw); // A borrowed baseline is not fresh receiving authority.
            var rebound = await h.Consume(); Assert.True(rebound.IsSuccess,rebound.ErrorMessage); Assert.NotSame(b,rebound.Value);
            var current = SelectionMaterialAdmissions(h,rebound.Value!); var replay = await BindFullMaterial(h,rebound.Value!,current,null);
            Assert.True(replay.IsSuccess,replay.ErrorCode+":"+replay.ErrorMessage); Assert.Equal(1,raw); Assert.Equal(1,calls);
            Assert.Equal(expected,replay.Value!.Context.CopyCanonicalBytes()); Assert.Equal(selected.Value.Choice.ItemId,replay.Value.Choice.ItemId);
            Assert.Equal(selected.Value.Choice.Reason,replay.Value.Choice.Reason); Assert.Equal(history,h.HistoryBytes()); h.AssertFiles(completedFiles);
            foreach(var path in new[]{h.Fixture.OrdinaryClaim,h.Fixture.OrdinaryBaseline})
            foreach(var missing in new[]{false,true})
            {
                Assert.True(h.Reopen().IsSuccess); var original = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path); Exception? failure = null;
                try
                {
                    if(missing) File.Delete(path); else RestoreCustodyFile(path,original.Concat(new byte[]{(byte)' '}).ToArray(),ticks);
                    var oldRaw = raw; var refused = await h.Consume(); output.WriteLine("FULL_REATTACH_REFUSAL path="+Path.GetFileName(path)+" missing="+missing+" code="+refused.ErrorCode);
                    Assert.False(refused.IsSuccess); Assert.Equal(oldRaw,raw); Assert.Equal(1,calls); AssertFullMaterialNoEffects(h);
                    if(missing) Assert.False(File.Exists(path));
                    else { Assert.Equal(original.Concat(new byte[]{(byte)' '}).ToArray(),File.ReadAllBytes(path)); Assert.Equal(ticks,File.GetLastWriteTimeUtc(path)); }
                    Assert.Equal(history,h.HistoryBytes());
                }
                catch(Exception ex) { failure = ex; throw; }
                finally { RestoreFixtureAfterFault(() => RestoreCustodyFile(path,original,ticks),failure); }
                var restored = await h.Consume(); Assert.True(restored.IsSuccess,restored.ErrorMessage);
                Assert.True((await BindFullMaterial(h,restored.Value!,SelectionMaterialAdmissions(h,restored.Value!),null)).IsSuccess);
                Assert.Equal(history,h.HistoryBytes()); h.AssertFiles(completedFiles);
            }
        }
        finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = observer; }
        AssertFullMaterialNoEffects(h);
    }

    [Fact]
    public async Task OrdinaryFullMaterialSelection_StartedUnknownRefusesRechoice()
    {
        foreach(var point in new[]{"full-material-genesis-readback","full-material-started-after-write","full-material-started-flushed","full-material-started-readback","callback","full-material-completed-after-write"})
        {
            using var h = FullMaterialHarness(); var b = (await h.Consume()).Value!; var a = SelectionMaterialAdmissions(h,b); var calls = 0; var entered = 0;
            var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((context,_) =>
            { calls++; if(point == "callback") { entered++; throw new IOException("actual Synthetic callback failure"); } return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[^1]))); });
            h.Open.Session.SelectionDurablePointForTesting = stage => { if(stage == point) { entered++; throw new IOException("actual append interruption "+stage); } };
            try { var result = await BindFullMaterial(h,b,a,provider); Assert.False(result.IsSuccess); }
            catch(IOException ex) { Assert.Equal("callback",point); Assert.Contains("callback",ex.Message); }
            finally { h.Open.Session.SelectionDurablePointForTesting = null; }
            Assert.Equal(1,entered); var actual = h.HistoryBytes(); Assert.NotEmpty(actual); var before = h.Files(); var oldCalls = calls;
            Assert.False((await BindFullMaterial(h,b,a,provider)).IsSuccess); Assert.Equal(oldCalls,calls); Assert.Equal(actual,h.HistoryBytes()); h.AssertFiles(before);
            var lines = FullMaterialLines(actual); var reopen = h.Reopen(); output.WriteLine("FULL_UNKNOWN point="+point+" lines="+lines.Length+" callbacks="+calls+" reopen="+reopen.ErrorCode);
            if(lines.Length == 3)
            {
                // Fully flushed bytes may exist despite an unknown caller outcome; only independent matching Completed reopen can recover the same saved choice.
                Assert.True(reopen.IsSuccess,reopen.ErrorMessage); var fresh = await h.Consume(); Assert.True(fresh.IsSuccess,fresh.ErrorMessage);
                Assert.True((await BindFullMaterial(h,fresh.Value!,SelectionMaterialAdmissions(h,fresh.Value!),null)).IsSuccess); Assert.Equal(oldCalls,calls);
            }
            else { Assert.False(reopen.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,reopen.ErrorCode); }
            Assert.Equal(actual,h.HistoryBytes()); Assert.Equal(10,h.Client.Calls);
        }
        using(var h = FullMaterialHarness())
        {
            var b = (await h.Consume()).Value!; var a = SelectionMaterialAdmissions(h,b); var descriptor = h.Grant.SourceSelection!.TeachingCurriculum!.Sources[0];
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,descriptor.DomainSlug,descriptor.SubjectSlug,descriptor.CorpusVersion).Value!;
            var sourcePath = Path.Combine(address.CorpusRoot,RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName); var sourceBytes = File.ReadAllBytes(sourcePath); var sourceTicks = File.GetLastWriteTimeUtc(sourcePath);
            var baselineBytes = File.ReadAllBytes(h.Fixture.OrdinaryBaseline); var baselineTicks = File.GetLastWriteTimeUtc(h.Fixture.OrdinaryBaseline); var calls = 0; var reached = 0; Exception? failure = null;
            var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) => { calls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[^1]))); });
            try
            {
                h.Open.Session.SelectionDurablePointForTesting = stage =>
                {
                    if(stage=="full-material-started-readback")
                    { reached++; RestoreCustodyFile(h.Fixture.OrdinaryBaseline,baselineBytes.Concat(new byte[]{(byte)' '}).ToArray(),baselineTicks); RestoreReferenceOwnerFile(sourcePath,sourceBytes.Concat(new byte[]{(byte)' '}).ToArray(),sourceTicks); }
                };
                var result = await BindFullMaterial(h,b,a,provider);
                output.WriteLine("FULL_POST_STARTED_OWNERS baseline="+h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting+" source="+h.Open.Session.SelectionMaterialSourceFinalErrorForTesting+" callbacks="+calls);
                Assert.False(result.IsSuccess); Assert.Equal(1,reached); Assert.Equal(0,calls);
                Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode,h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting);
                Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode,h.Open.Session.SelectionMaterialSourceFinalErrorForTesting);
                Assert.Equal(2,FullMaterialLines(h.HistoryBytes()).Length); Assert.Equal((1,0,true),h.Open.Session.FullMaterialSelectionAccounting);
                Assert.Equal(baselineBytes.Concat(new byte[]{(byte)' '}).ToArray(),File.ReadAllBytes(h.Fixture.OrdinaryBaseline)); Assert.Equal(baselineTicks,File.GetLastWriteTimeUtc(h.Fixture.OrdinaryBaseline));
                Assert.Equal(sourceBytes.Concat(new byte[]{(byte)' '}).ToArray(),File.ReadAllBytes(sourcePath)); Assert.Equal(sourceTicks,File.GetLastWriteTimeUtc(sourcePath)); AssertFullMaterialNoEffects(h);
            }
            catch(Exception ex) { failure=ex; throw; }
            finally
            { h.Open.Session.SelectionDurablePointForTesting=null; RestoreFixtureAfterFault(() => { RestoreCustodyFile(h.Fixture.OrdinaryBaseline,baselineBytes,baselineTicks); RestoreReferenceOwnerFile(sourcePath,sourceBytes,sourceTicks); },failure); }
            var pending=h.HistoryBytes(); Assert.False((await BindFullMaterial(h,b,a,provider)).IsSuccess); Assert.Equal(0,calls); Assert.Equal(pending,h.HistoryBytes()); Assert.False(h.Reopen().IsSuccess);
        }
        using var control = FullMaterialHarness(); var cb = (await control.Consume()).Value!; var ca = SelectionMaterialAdmissions(control,cb);
        var good = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) => Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[^1]))));
        Assert.True((await BindFullMaterial(control,cb,ca,good)).IsSuccess); AssertFullMaterialNoEffects(control);
    }

    [Fact]
    public async Task OrdinaryFullMaterialSelection_BindsEveryCandidateAndOpaqueAdmission()
    {
        using var h = FullMaterialHarness(); var b = (await h.Consume()).Value!; var valid = SelectionMaterialAdmissions(h,b);
        using var foreign = FullMaterialHarness(); var fb = (await foreign.Consume()).Value!; var fa = SelectionMaterialAdmissions(foreign,fb);
        var reads = 0; var raw = 0; var calls = 0; var files = h.Files();
        var previousRead = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        var previousRaw = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) => { calls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[^1]))); });
        try
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = _ => reads++;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => raw++;
            var stale = SelectedMaterialAdmission(h,b,H("foreign-binding-full"),valid[0].Item.ItemId);
            foreach(var rejected in new CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission[]?[] {null,valid[..^1],valid.Concat(new[]{valid[0]}).ToArray(),new[]{fa[0]}.Concat(valid.Skip(1)).ToArray(),new[]{stale}.Concat(valid.Skip(1)).ToArray()})
            {
                var result = await BindFullMaterial(h,b,rejected,provider); output.WriteLine("FULL_ADMISSION_REFUSAL code="+result.ErrorCode+" raw="+raw+" reads="+reads);
                Assert.False(result.IsSuccess); Assert.Equal(0,reads); Assert.Equal(0,raw); Assert.Equal(0,calls); h.AssertFiles(files); Assert.Empty(h.HistoryBytes());
            }
            var witness=h.Inspect(b).Value!; var diagnosis=h.Classify(b).Value!;
            var input=CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.OriginalReceiptId,diagnosis.OriginalReceiptSha256,diagnosis.Categories,witness.TeachingIdentities).Value!;
            // Denial/quarantine cannot produce an opaque admission; exercise the durable consumer with that actual missing result, without fabricating an issued-denied object.
            foreach(var policy in new[]{new TrainingDataGovernanceDecision(false,false,["sha256:"+H("denied")]),new TrainingDataGovernanceDecision(true,true,["sha256:"+H("quarantined")])})
            {
                var denied=CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(witness.TeachingIdentities,input,witness.BindingSha256,valid[0].Item.ItemId,CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference,policy);
                Assert.False(denied.IsSuccess); Assert.Null(denied.Value);
                var rejected=new[]{denied.Value!}.Concat(valid.Skip(1)).ToArray();
                Assert.False((await BindFullMaterial(h,b,rejected,provider)).IsSuccess);
                Assert.Equal(0,reads); Assert.Equal(0,raw); Assert.Equal(0,calls); h.AssertFiles(files); Assert.Empty(h.HistoryBytes());
            }
            var positive = await BindFullMaterial(h,b,valid,provider); Assert.True(positive.IsSuccess,positive.ErrorMessage); Assert.Equal(1,raw); Assert.Equal(1,calls); Assert.True(reads>0);
        }
        finally { CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = previousRead; CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previousRaw; }
        Assert.True(h.Reopen().IsSuccess); var fresh = (await h.Consume()).Value!; var current = SelectionMaterialAdmissions(h,fresh); var savedFiles = h.Files();
        var source = h.Grant.SourceSelection!.TeachingCurriculum!.Sources[0]; var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,source.DomainSlug,source.SubjectSlug,source.CorpusVersion).Value!;
        var path = Path.Combine(address.CorpusRoot,RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName); var bytes = File.ReadAllBytes(path); var tick = File.GetLastWriteTimeUtc(path); Exception? failure = null;
        try
        {
            RestoreReferenceOwnerFile(path,bytes.Concat(new byte[]{(byte)' '}).ToArray(),tick);
            var refused = await BindFullMaterial(h,fresh,current,null); output.WriteLine("FULL_UNUSED_SOURCE_REFUSAL code="+refused.ErrorCode);
            Assert.False(refused.IsSuccess); Assert.Equal(1,calls); Assert.Equal(bytes.Concat(new byte[]{(byte)' '}).ToArray(),File.ReadAllBytes(path)); Assert.Equal(tick,File.GetLastWriteTimeUtc(path));
        }
        catch(Exception ex) { failure = ex; throw; }
        finally { RestoreFixtureAfterFault(() => RestoreReferenceOwnerFile(path,bytes,tick),failure); }
        Assert.True((await BindFullMaterial(h,fresh,current,null)).IsSuccess); h.AssertFiles(savedFiles); AssertFullMaterialNoEffects(h);
        using var alias = FullMaterialHarness(); var ab = (await alias.Consume()).Value!; var aa = SelectionMaterialAdmissions(alias,ab); var aliasCalls = 0;
        var closed = alias.Context.Corpus.Entries[0].OwnerInput;
        var rejectedReason = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) => { aliasCalls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[0],closed))); });
        Assert.False((await BindFullMaterial(alias,ab,aa,rejectedReason)).IsSuccess); Assert.Equal(1,aliasCalls); Assert.Equal(2,FullMaterialLines(alias.HistoryBytes()).Length); AssertFullMaterialNoEffects(alias);
    }

    [Fact]
    public async Task OrdinaryFullMaterialSelection_FinalNoneChecksPreservePrimaryAndRelease()
    {
        using(var h = FullMaterialHarness())
        {
            var b = (await h.Consume()).Value!; var a = SelectionMaterialAdmissions(h,b); var files = h.Files();
            var beforeRaw = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            var primary = new OperationCanceledException("full pre-Started primary",null,cancellation.Token); var secondary = new IOException("full secondary None check");
            try
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => throw primary;
                h.Open.Session.SelectionMaterialPointForTesting = stage => { if(stage=="final-none-checked") throw secondary; };
                var actual = await Assert.ThrowsAsync<OperationCanceledException>(() => BindFullMaterial(h,b,a,null));
                Assert.Same(primary,actual); Assert.Equal(cancellation.Token,actual.CancellationToken); Assert.Same(secondary,actual.Data["SelectionMaterialFinalFailure"]);
            }
            finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = beforeRaw; h.Open.Session.SelectionMaterialPointForTesting = null; }
            h.AssertFiles(files); Assert.Empty(h.HistoryBytes());
            var good = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) => Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[^1]))));
            Assert.True((await BindFullMaterial(h,b,a,good)).IsSuccess); AssertFullMaterialNoEffects(h); // Exact pre-Started release permits a real same-owner control.
        }
        using(var h = FullMaterialHarness())
        {
            var b = (await h.Consume()).Value!; var a = SelectionMaterialAdmissions(h,b); var source = h.Grant.SourceSelection!.TeachingCurriculum!.Sources[0];
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot,source.DomainSlug,source.SubjectSlug,source.CorpusVersion).Value!;
            var path = Path.Combine(address.CorpusRoot,RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName); var bytes = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path);
            var baselineBytes = File.ReadAllBytes(h.Fixture.OrdinaryBaseline); var baselineTicks = File.GetLastWriteTimeUtc(h.Fixture.OrdinaryBaseline);
            var prefixField = typeof(CodeDocumentationOneLessonSession).GetField("_v2Prefix",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
            byte[]? originalPrefix = null; byte[]? prefixCopy = null; var calls = 0; var receiving = 0; var baseline = 0; var material = 0; Exception? failure = null;
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel(); var primary = new OperationCanceledException("full actual callback cancellation",null,cancellation.Token);
            var secondary = new IOException("full late None-check failure");
            var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((_,_) =>
            {
                calls++; originalPrefix = (byte[])prefixField.GetValue(h.Open.Session)!; prefixCopy = originalPrefix.ToArray();
                prefixField.SetValue(h.Open.Session,prefixCopy.Concat(new byte[]{1}).ToArray());
                RestoreCustodyFile(h.Fixture.OrdinaryBaseline,baselineBytes.Concat(new byte[]{(byte)' '}).ToArray(),baselineTicks);
                RestoreReferenceOwnerFile(path,bytes.Concat(new byte[]{(byte)' '}).ToArray(),ticks); throw primary;
            });
            try
            {
                h.Open.Session.SelectionMaterialPointForTesting = stage =>
                {
                    if(stage=="receiving-final-attempted") receiving++; if(stage=="baseline-final-attempted") baseline++; if(stage=="source-final-attempted") material++;
                    if(stage=="final-none-checked" && material==3) throw secondary;
                };
                var actual = await Assert.ThrowsAsync<OperationCanceledException>(() => BindFullMaterial(h,b,a,provider));
                output.WriteLine("FULL_COMBINED receiving="+h.Open.Session.SelectionMaterialReceivingFinalErrorForTesting+" baseline="+h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting+" source="+h.Open.Session.SelectionMaterialSourceFinalErrorForTesting+" attempts="+receiving+"/"+baseline+"/"+material);
                Assert.Same(primary,actual); Assert.Equal(cancellation.Token,actual.CancellationToken); Assert.Same(secondary,actual.Data["SelectionMaterialFinalFailure"]);
                Assert.Equal(1,calls); Assert.Equal(3,receiving); Assert.Equal(3,baseline); Assert.Equal(3,material);
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,actual.Data["SelectionMaterialFinalReceivingError"]);
                Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode,actual.Data["SelectionMaterialFinalBaselineError"]);
                Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode,actual.Data["SelectionMaterialFinalSourceError"]);
                Assert.Equal(baselineBytes.Concat(new byte[]{(byte)' '}).ToArray(),File.ReadAllBytes(h.Fixture.OrdinaryBaseline)); Assert.Equal(baselineTicks,File.GetLastWriteTimeUtc(h.Fixture.OrdinaryBaseline));
                Assert.Equal(bytes.Concat(new byte[]{(byte)' '}).ToArray(),File.ReadAllBytes(path)); Assert.Equal(ticks,File.GetLastWriteTimeUtc(path)); AssertFullMaterialNoEffects(h);
            }
            catch(Exception ex) { failure = ex; throw; }
            finally
            {
                h.Open.Session.SelectionMaterialPointForTesting = null;
                if(originalPrefix is not null) { prefixField.SetValue(h.Open.Session,originalPrefix); Assert.Same(originalPrefix,prefixField.GetValue(h.Open.Session)); Assert.Equal(prefixCopy,originalPrefix); }
                RestoreFixtureAfterFault(() => { RestoreCustodyFile(h.Fixture.OrdinaryBaseline,baselineBytes,baselineTicks); RestoreReferenceOwnerFile(path,bytes,ticks); },failure);
            }
            var pending = h.HistoryBytes(); Assert.Equal(2,FullMaterialLines(pending).Length);
            Assert.False((await BindFullMaterial(h,b,a,provider)).IsSuccess); Assert.Equal(1,calls); Assert.Equal(pending,h.HistoryBytes()); Assert.False(h.Reopen().IsSuccess);
        }
        using(var h = FullMaterialHarness())
        using(var foreign = FullMaterialHarness())
        {
            var b = (await h.Consume()).Value!; var a = SelectionMaterialAdmissions(h,b); var fb = (await foreign.Consume()).Value!; var calls = 0;
            var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting(async (c,_) =>
            { Interlocked.Increment(ref calls); entered.TrySetResult(true); await release.Task; return OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[^1])); });
            Assert.False((await BindFullMaterial(h,fb,a,provider)).IsSuccess); Assert.Equal(0,Volatile.Read(ref calls)); // Foreign receiving owner cannot acquire raw input.
            var running = BindFullMaterial(h,b,a,provider); CodeDocumentationOneLessonSession.MaterialSelectionDispatchReceipt? receipt = null; Exception? failure = null;
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(20)); receipt = h.Open.Session.FullMaterialDispatchForTesting;
                Assert.NotNull(receipt); var pending = h.HistoryBytes(); var files = h.Files(); var accounting = h.Open.Session.FullMaterialSelectionAccounting; var effects = h.Open.Session.Snapshot.Effects;
                Assert.Equal(2,FullMaterialLines(pending).Length); Assert.Equal((1,0,true),accounting);
                // Use the actual issued receipt through the same production entrypoint while the first callback is still awaiting release.
                var duplicate = await provider.InvokePreparedMaterialSelectionAsync(h.Open.Session,receipt!,CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(20));
                output.WriteLine("FULL_SAME_RECEIPT_REFUSAL code="+duplicate.ErrorCode+" callbacks="+Volatile.Read(ref calls)+" frames="+FullMaterialLines(h.HistoryBytes()).Length);
                Assert.False(duplicate.IsSuccess); Assert.Null(duplicate.Value); Assert.Equal("ONE_LESSON_SELECTION_BINDING_INVALID",duplicate.ErrorCode);
                Assert.Equal(1,Volatile.Read(ref calls)); Assert.Equal(pending,h.HistoryBytes()); Assert.Equal(accounting,h.Open.Session.FullMaterialSelectionAccounting);
                Assert.Equal(effects,h.Open.Session.Snapshot.Effects); h.AssertFiles(files); AssertFullMaterialNoEffects(h);
                Assert.False((await BindFullMaterial(h,b,a,provider)).IsSuccess); Assert.Equal(1,Volatile.Read(ref calls)); Assert.Equal(pending,h.HistoryBytes());
                Assert.False((await provider.InvokePreparedMaterialSelectionAsync(foreign.Open.Session,receipt!,CancellationToken.None)).IsSuccess);
                var inspectionCalls = 0; var inspection = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialInspectionForTesting((c,_) =>
                { inspectionCalls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[0]))); });
                Assert.False((await inspection.InvokePreparedMaterialSelectionAsync(h.Open.Session,receipt!,CancellationToken.None)).IsSuccess); Assert.Equal(0,inspectionCalls);
            }
            catch(Exception ex) { failure=ex; throw; }
            finally
            {
                release.TrySetResult(true);
                try { await running.WaitAsync(TimeSpan.FromSeconds(20)); }
                catch(Exception cleanupFailure) when(failure is not null) { throw new AggregateException("Held callback cleanup failed after the primary test fault.",failure,cleanupFailure); }
            }
            var completed = await running; Assert.True(completed.IsSuccess,completed.ErrorMessage); Assert.Equal(1,Volatile.Read(ref calls));
            Assert.False((await provider.InvokePreparedMaterialSelectionAsync(h.Open.Session,receipt!,CancellationToken.None)).IsSuccess); Assert.Equal(1,Volatile.Read(ref calls)); // Cleanup invalidates the exact old receipt; hashes cannot reactivate it.
            Assert.True((await BindFullMaterial(h,b,a,null)).IsSuccess); Assert.Equal(1,Volatile.Read(ref calls)); AssertFullMaterialNoEffects(h);
        }
    }

    [Fact]
    public async Task OrdinaryFullMaterialSelection_ClosedWireCapacityAndLegacyIsolation()
    {
        using(var primitives = JsonDocument.Parse("[true,false,null,2]"))
            Assert.Equal(Encoding.UTF8.GetBytes("[true,false,null,2]"),IndependentAsciiCanonicalJson(primitives.RootElement));
        // Literal physical fences are tested independently of canonical grammar; passing these byte subchecks never mints a trusted request or frame.
        static byte[] Physical(int size) => Enumerable.Repeat((byte)' ',size-1).Append((byte)'\n').ToArray();
        Assert.True(CodeDocumentationOneLessonV2Wire.FullMaterialRequestByteCapacityWithin(new byte[25731]));
        Assert.False(CodeDocumentationOneLessonV2Wire.FullMaterialRequestByteCapacityWithin(new byte[25732]));
        var maximum = new[]{Physical(4096),Physical(8192),Physical(45076)}; Assert.Equal(57364,maximum.Sum(x => x.Length));
        Assert.True(CodeDocumentationOneLessonV2Wire.FullMaterialPhysicalCapacityWithin(maximum));
        foreach(var (index,size) in new[]{(0,4097),(1,8193),(2,45077)})
        { var over = maximum.ToArray(); over[index] = Physical(size); Assert.Equal(size,over[index].Length); Assert.False(CodeDocumentationOneLessonV2Wire.FullMaterialPhysicalCapacityWithin(over)); }
        var familyOver = new[]{Physical(4096),Physical(8192),Physical(45077)}; Assert.Equal(57365,familyOver.Sum(x => x.Length)); Assert.False(CodeDocumentationOneLessonV2Wire.FullMaterialPhysicalCapacityWithin(familyOver));
        const string taskPrefix = "Genuine exact byte boundary task\n";
        async Task<(OrdinaryDiagnosticHarness Harness, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline Baseline,
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission[] Admissions, string Task, byte[] Packet)> ExactPacket(int target)
        {
            var padding = 1;
            for(var attempt = 0; attempt < 8; attempt++)
            {
                var task = taskPrefix + new string('x',padding); var h = FullMaterialTaskHarness(task);
                try
                {
                    var produced = await h.Consume(); Assert.True(produced.IsSuccess,produced.ErrorCode+":"+produced.ErrorMessage);
                    var b = produced.Value!; var a = SelectionMaterialAdmissions(h,b); var packet = IndependentlyExpectedFullMaterialPacket(h,b,a);
                    output.WriteLine("FULL_PACKET_CONSTRUCTION target="+target+" attempt="+attempt+" padding="+padding+" actual="+packet.Length);
                    Assert.Empty(h.HistoryBytes()); AssertFullMaterialNoEffects(h);
                    if(packet.Length == target) return (h,b,a,task,packet);
                    padding = checked(padding+target-packet.Length); Assert.InRange(padding,1,65536);
                }
                catch { h.Dispose(); throw; }
                h.Dispose();
            }
            throw new InvalidOperationException("Genuine complete packet did not attain its exact byte boundary within eight fixture constructions.");
        }
        var exact = await ExactPacket(16384);
        using(var h = exact.Harness)
        {
            var b = exact.Baseline; var a = exact.Admissions; var task = exact.Task; var calls = 0; const int invocationLength = 32;
            var expectedRequest = IndependentlyExpectedFullMaterialRequest(h,b,a,invocationLength); var expectedRequestBytes = IndependentAsciiRequestJson(expectedRequest);
            Assert.Equal(16384,expectedRequest.PacketByteLength); Assert.InRange(expectedRequestBytes.Length,1,25731);
            Assert.Equal(expectedRequestBytes,CodeDocumentationOneLessonV2Wire.FullMaterialRequestBytes(expectedRequest));
            var filesBefore = h.Files(); var packetBefore = IndependentlyExpectedFullMaterialPacket(h,b,a);
            var reservations = new List<(string Item,int Request,int Genesis,int Started,int Completed)>();
            h.Open.Session.FullMaterialCapacityReservedForTesting = (item,request,genesis,started,completed) => reservations.Add((item,request,genesis,started,completed));
            var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) =>
            { calls++; Assert.Equal(packetBefore,c.CopyCanonicalBytes()); Assert.Contains(c.Candidates,x => x.OwnerInput==task); return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[^1],new string('\0',1024)))); });
            var over = await ExactPacket(16385);
            using(var overflow = over.Harness)
            {
                var overRequest = IndependentlyExpectedFullMaterialRequest(overflow,over.Baseline,over.Admissions,invocationLength);
                var exactOver = IndependentAsciiRequestJson(overRequest); var before = overflow.Files(); var overReservations = 0; var captured = 0;
                Assert.Equal(16385,over.Packet.Length); Assert.Equal(over.Packet.Length,overRequest.PacketByteLength);
                Assert.Equal(over.Packet,Convert.FromBase64String(overRequest.PacketCanonicalBase64)); Assert.Equal(HBytes(over.Packet),overRequest.PacketSha256);
                Assert.InRange(exactOver.Length,1,25731); Assert.True(CodeDocumentationOneLessonV2Wire.FullMaterialRequestByteCapacityWithin(exactOver));
                var oldCaptured = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
                try
                {
                    CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = c => { captured++; Assert.Equal(over.Packet,c.CopyCanonicalBytes()); };
                    overflow.Open.Session.FullMaterialCapacityReservedForTesting = (_,_,_,_,_) => overReservations++;
                    var overProvider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) =>
                    { calls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[^1]))); });
                    var overflowResult = await BindFullMaterial(overflow,over.Baseline,over.Admissions,overProvider);
                    output.WriteLine("FULL_EXACT_PACKET_OVERFLOW packet="+over.Packet.Length+" request="+exactOver.Length+" hash="+overRequest.PacketSha256+" code="+overflowResult.ErrorCode+" captured="+captured+" reservations="+overReservations+" callbacks="+calls);
                    Assert.False(overflowResult.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,overflowResult.ErrorCode);
                    Assert.Equal(1,captured); Assert.Equal(0,overReservations); Assert.Equal(0,calls); Assert.Empty(reservations);
                    Assert.Empty(overflow.HistoryBytes()); overflow.AssertFiles(before); Assert.Equal((0,0,false),overflow.Open.Session.FullMaterialSelectionAccounting); AssertFullMaterialNoEffects(overflow);
                }
                finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = oldCaptured; overflow.Open.Session.FullMaterialCapacityReservedForTesting = null; }
            }
            Assert.Empty(h.HistoryBytes()); h.AssertFiles(filesBefore); Assert.Equal(packetBefore,IndependentlyExpectedFullMaterialPacket(h,b,a));
            h.Open.Session.FullMaterialInvocationIdForTesting = expectedRequest.InvocationId;
            var result = await BindFullMaterial(h,b,a,provider); Assert.True(result.IsSuccess,result.ErrorCode+":"+result.ErrorMessage); Assert.Equal(1024,Encoding.UTF8.GetByteCount(result.Value!.Choice.Reason)); Assert.Equal(1,calls);
            var raw = h.HistoryBytes(); var lines = FullMaterialLines(raw); var projection = FullMaterialProjection(h);
            using var actualCompleted = JsonDocument.Parse(lines[2]);
            Assert.Equal(expectedRequestBytes,Convert.FromBase64String(actualCompleted.RootElement.GetProperty("FullMaterialSelection").GetProperty("RequestCanonicalBase64").GetString()!));
            var persistedRequest = JsonSerializer.Deserialize<CodeDocumentationFullMaterialRequestV1>(
                Convert.FromBase64String(actualCompleted.RootElement.GetProperty("FullMaterialSelection").GetProperty("RequestCanonicalBase64").GetString()!))!;
            Assert.Equal(exact.Packet,packetBefore); Assert.Equal(packetBefore,result.Value.Context.CopyCanonicalBytes());
            Assert.Equal(16384,persistedRequest.PacketByteLength); Assert.Equal(packetBefore,Convert.FromBase64String(persistedRequest.PacketCanonicalBase64));
            Assert.Equal(HBytes(packetBefore),persistedRequest.PacketSha256);
            Assert.Equal(expectedRequestBytes,IndependentAsciiRequestJson(persistedRequest));
            var bindingBytes = result.Value.Context.CopyBindingBytes(); var metadataBytes = result.Value.Context.MetadataInput.CopyBytes();
            var escapedReasonBytes = IndependentAsciiCanonicalJson(JsonSerializer.SerializeToElement(result.Value.Choice.Reason)).Length-2;
            Assert.Equal(6144,escapedReasonBytes);
            output.WriteLine("FULL_PACKET_BOUNDARY packet="+packetBefore.Length+" declared="+persistedRequest.PacketByteLength+" decoded="+Convert.FromBase64String(persistedRequest.PacketCanonicalBase64).Length+" binding="+bindingBytes.Length+" metadata="+metadataBytes.Length+" request="+expectedRequestBytes.Length+" reasonUtf8="+Encoding.UTF8.GetByteCount(result.Value.Choice.Reason)+" reasonEscaped="+escapedReasonBytes+" genesis="+lines[0].Length+" started="+lines[1].Length+" completed="+lines[2].Length+" total="+raw.Length+" packetSha="+persistedRequest.PacketSha256);
            // These maximum-width shapes prove a closed syntax bound, not genuine material admission or simultaneous semantic attainability.
            var scalarMaximum = expectedRequest with { InvocationId = new string('i',128), BindingCanonicalBase64 = Convert.ToBase64String(new byte[2120]),
                PacketCanonicalBase64 = Convert.ToBase64String(new byte[16384]), PacketByteLength = 16384, PacketSha256 = HBytes(new byte[16384]),
                Categories = new[] { "missing_essential_behavior", "invented_behavior", "wrong_purpose_or_result", "unclear_or_unverifiable" } };
            var scalarMaximumBytes = IndependentAsciiRequestJson(scalarMaximum); Assert.Equal(25731,scalarMaximumBytes.Length);
            Assert.Equal(scalarMaximumBytes,CodeDocumentationOneLessonV2Wire.FullMaterialRequestBytes(scalarMaximum));
            var maxChoice = new { Schema = "code-documentation-teaching-selection-binding-v1", PolicyDocument = "S04-CODEX-TEACHING-SELECTION-v1",
                scalarMaximum.OriginalReceiptId, scalarMaximum.OriginalReceiptSha256, scalarMaximum.SourceSha256, scalarMaximum.Categories,
                ItemId = new string('i',128), SourceId = new string('s',128), ReferenceEntryId = "sha256:"+H("max-ref"), SourceEntityFingerprint = "sha256:"+H("max-fingerprint"),
                Reason = new string('\0',1024), scalarMaximum.FrozenContextSha256, MaterialAdmissionEvidenceSha256 = H("max-material"), Origin = "SyntheticTest",
                scalarMaximum.InvocationId, LaunchSettingsSha256 = H("max-settings"), LaunchAckSha256 = H("max-ack"),
                RequestedModel = "gpt-6-astra", RequestedReasoning = "ultra", ProviderTelemetry = "UNKNOWN" };
            Assert.Equal(7713,IndependentAsciiCanonicalJson(JsonSerializer.SerializeToElement(maxChoice)).Length);
            object MaxFrame(int revision, object payload) => new { Kind = revision==0?"Genesis":revision==1?"TeachingSelectionStarted":"TeachingSelectionCompleted",
                FormatVersion = 2, SessionId = h.Open.Session.Snapshot.SessionId, BindingV2 = projection, BindingSha256 = scalarMaximum.BindingSha256,
                Revision = revision, PreviousFrameSha256 = H("max-prev"), Effects = new { BaselineDispatched="0",BaselineCompleted="0",TeachingDispatched="0",TeachingCompleted="0",
                    EvaluatorDispatched="0",EvaluatorCompleted="0",OptimizerEntries="0",EffectOutcomeUnknown=false }, FullMaterialSelection = payload };
            int MaxFrameBytes(int revision, object payload) => IndependentAsciiCanonicalJson(JsonSerializer.SerializeToElement(MaxFrame(revision,payload))).Length+1+2120-bindingBytes.Length;
            Assert.Equal(2901,MaxFrameBytes(0,new { scalarMaximum.Schema,scalarMaximum.Origin,scalarMaximum.OriginalReceiptId,scalarMaximum.OriginalReceiptSha256 }));
            Assert.Equal(3270,MaxFrameBytes(1,new { scalarMaximum.Schema,scalarMaximum.Origin,scalarMaximum.InvocationId,RequestSha256=HBytes(scalarMaximumBytes),
                RequestByteLength=scalarMaximumBytes.Length,scalarMaximum.PacketSha256,LaunchSettingsSha256=H("max-settings"),ReservedInvocations=1,CompletedInvocations=0,OutcomeUnknown=true }));
            var maxCompleted = MaxFrameBytes(2,new { scalarMaximum.Schema,scalarMaximum.Origin,RequestCanonicalBase64=Convert.ToBase64String(scalarMaximumBytes),
                RequestSha256=HBytes(scalarMaximumBytes),StartedFrameSha256=H("max-started"),Choice=maxChoice,ReservedInvocations=1,CompletedInvocations=1,OutcomeUnknown=false });
            Assert.Equal(45076,maxCompleted); Assert.Equal(maxCompleted,CodeDocumentationOneLessonV2Wire.FullMaterialCompletedMaximumBytes);
            Assert.Equal(25731,CodeDocumentationOneLessonV2Wire.FullMaterialRequestMaximumBytes);
            Assert.Equal(16384,CodeDocumentationOneLessonV2Wire.FullMaterialPacketMaximumBytes);
            Assert.Equal(57364,CodeDocumentationOneLessonV2Wire.FullMaterialTotalMaximumBytes);
            output.WriteLine("FULL_CLOSED_SYNTAX_BOUND request="+scalarMaximumBytes.Length+" choice=7713 genesis=2901 started=3270 completed="+maxCompleted+" slots=4096/8192/45076 total=57364 semanticMaxAttained=false");
            Assert.Equal(a.Select(x => x.Item.ItemId),reservations.Select(x => x.Item));
            foreach(var r in reservations)
            {
                output.WriteLine("FULL_CAPACITY item="+r.Item+" request="+r.Request+" genesis="+r.Genesis+" started="+r.Started+" completed="+r.Completed+" total="+(r.Genesis+r.Started+r.Completed));
                Assert.Equal(expectedRequestBytes.Length,r.Request); Assert.InRange(r.Genesis,2,4096); Assert.InRange(r.Started,2,8192); Assert.InRange(r.Completed,2,45076); Assert.InRange(r.Genesis+r.Started+r.Completed,6,57364);
            }
            Assert.Equal(lines[2].Length,reservations.Single(x => x.Item==result.Value.Choice.ItemId).Completed); // The genuinely consumed worst-escaped reason matches its pre-Started whole record reserve.
            h.Open.Session.FullMaterialCapacityReservedForTesting = null;
            Assert.True(CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(raw,projection,h.Open.Session.Snapshot.SessionId,false).IsSuccess);
            Assert.False(CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(maximum.SelectMany(x => x).ToArray(),projection,h.Open.Session.Snapshot.SessionId,false).IsSuccess); // Byte capacity alone cannot authenticate even a syntactically non-JSON record.
            var parsed = 0; var innerParsed = 0; var oldParse = CodeDocumentationOneLessonV2Wire.FullMaterialParsePointForTesting;
            var oldInnerParse = CodeDocumentationOneLessonV2Wire.FullMaterialRequestParsePointForTesting;
            try
            {
                CodeDocumentationOneLessonV2Wire.FullMaterialParsePointForTesting = () => parsed++;
                CodeDocumentationOneLessonV2Wire.FullMaterialRequestParsePointForTesting = () => innerParsed++;
                foreach(var (index,size) in new[]{(0,4097),(1,8193),(2,45077)})
                { var oversized = lines.ToArray(); oversized[index]=Physical(size); Assert.False(CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(oversized.SelectMany(x => x).ToArray(),projection,h.Open.Session.Snapshot.SessionId,false).IsSuccess); Assert.Equal(0,parsed); Assert.Equal(0,innerParsed); }
                Assert.False(CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(familyOver.SelectMany(x => x).ToArray(),projection,h.Open.Session.Snapshot.SessionId,false).IsSuccess); Assert.Equal(0,parsed); Assert.Equal(0,innerParsed);
                Assert.True(CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(raw,projection,h.Open.Session.Snapshot.SessionId,false).IsSuccess); Assert.Equal(1,parsed); Assert.Equal(1,innerParsed);
                foreach(var requestSize in new[]{25731,25732})
                {
                    parsed=0; innerParsed=0; var requestVector = Enumerable.Repeat((byte)' ',requestSize).ToArray(); var requestHash = HBytes(requestVector);
                    var startNode = JsonNode.Parse(lines[1])!.AsObject(); var startPayload = startNode["FullMaterialSelection"]!;
                    // Clamp the Started count to pass its own admission and isolate Completed Base64's inner-request fence.
                    startPayload["RequestByteLength"]=Math.Min(requestSize,25731); startPayload["RequestSha256"]=requestHash;
                    var startVector=IndependentAsciiCanonicalJson(JsonSerializer.SerializeToElement(startNode)).Append((byte)'\n').ToArray();
                    var completedNode=JsonNode.Parse(lines[2])!.AsObject(); var completedPayload=completedNode["FullMaterialSelection"]!;
                    completedNode["PreviousFrameSha256"]=HBytes(startVector[..^1]); completedPayload["StartedFrameSha256"]=HBytes(startVector[..^1]);
                    completedPayload["RequestSha256"]=requestHash; completedPayload["RequestCanonicalBase64"]=Convert.ToBase64String(requestVector);
                    var completedVector=IndependentAsciiCanonicalJson(JsonSerializer.SerializeToElement(completedNode)).Append((byte)'\n').ToArray();
                    Assert.True(CodeDocumentationOneLessonV2Wire.FullMaterialPhysicalCapacityWithin(new[]{lines[0],startVector,completedVector}));
                    var refused=CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(lines[0].Concat(startVector).Concat(completedVector).ToArray(),projection,h.Open.Session.Snapshot.SessionId,false);
                    Assert.False(refused.IsSuccess); Assert.Equal(1,parsed); Assert.Equal(requestSize==25731?1:0,innerParsed);
                    output.WriteLine("FULL_INNER_TRANSPORT_FENCE request="+requestSize+" outer="+parsed+" inner="+innerParsed+" semanticAccepted=false");
                }
            }
            finally { CodeDocumentationOneLessonV2Wire.FullMaterialParsePointForTesting = oldParse; CodeDocumentationOneLessonV2Wire.FullMaterialRequestParsePointForTesting = oldInnerParse; }
            var capacity = h.Open.Session.ReadCapacityPrefixV2().Value!; Assert.Equal(3,capacity.MaximumFrames); Assert.Equal(57364,capacity.MaximumBytes); Assert.Equal(3,capacity.PrefixFrames); Assert.Equal(raw.LongLength,capacity.PrefixBytes);
            Assert.True(h.Open.Session.ValidateContinuationCapacityV2(3,raw.Length,0,57364-raw.Length).IsSuccess);
            Assert.False(h.Open.Session.ValidateContinuationCapacityV2(3,raw.Length,0,57365-raw.Length).IsSuccess);
            Assert.False(h.Open.Session.ValidateContinuationCapacityV2(3,raw.Length,1,0).IsSuccess);
            var malformed = new List<byte[]> { raw[..^1],raw.Concat(lines[2]).ToArray(),lines[0].Concat(lines[2]).ToArray() };
            using(var wholeCompleted = JsonDocument.Parse(lines[2]))
                Assert.Equal(lines[2][..^1],IndependentAsciiCanonicalJson(wholeCompleted.RootElement)); // Reach both real false fields before constructing the unchanged malformed vectors.
            foreach(var property in new[]{"Unknown","Origin","RequestedModel","Revision","PreviousFrameSha256","RequestCanonicalBase64"})
            {
                var node = JsonNode.Parse(lines[2])!.AsObject();
                if(property=="Unknown") node[property]="x";
                else if(property=="Revision") node[property]="2";
                else if(property=="PreviousFrameSha256") node[property]=H("foreign-started");
                else if(property=="RequestedModel") node["FullMaterialSelection"]!["Choice"]![property]="NativeConfigured";
                else node["FullMaterialSelection"]![property]=property=="RequestCanonicalBase64"?"AA==":"NativeConfigured";
                malformed.Add(lines[0].Concat(lines[1]).Concat(IndependentAsciiCanonicalJson(JsonSerializer.SerializeToElement(node))).Append((byte)'\n').ToArray()); // Canonicalize the actual mutated fields so refusal is not merely default JsonNode formatting/escaping.
            }
            var files = h.Files(); h.Open.Dispose();
            foreach(var bytes in malformed)
            {
                RestoreCustodyFile(h.Fixture.OrdinaryHistory,bytes,new DateTime(files[h.Fixture.OrdinaryHistory].Ticks,DateTimeKind.Utc));
                var refused = h.Reopen(); output.WriteLine("FULL_CLOSED_WIRE_REFUSAL bytes="+bytes.Length+" code="+refused.ErrorCode); Assert.False(refused.IsSuccess);
                Assert.Equal(bytes,h.HistoryBytes()); Assert.Equal(1,calls); Assert.Equal(10,h.Client.Calls);
            }
            RestoreCustodyFile(h.Fixture.OrdinaryHistory,raw,new DateTime(files[h.Fixture.OrdinaryHistory].Ticks,DateTimeKind.Utc)); Assert.True(h.Reopen().IsSuccess);
            var rebound = await h.Consume(); Assert.True(rebound.IsSuccess,rebound.ErrorMessage); Assert.True((await BindFullMaterial(h,rebound.Value!,SelectionMaterialAdmissions(h,rebound.Value!),null)).IsSuccess); h.AssertFiles(files);
            var binding = h.Open.Session.Snapshot.Binding; h.Open.Dispose(); var root = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(h.Fixture.RepositoryRoot).Value!;
            Assert.False(CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,binding,h.Catalog.MaxTeacherCalls,h.Grant).IsSuccess); // Default metadata-only decoder cannot import the scoped family/lease.
            Assert.True(h.Reopen().IsSuccess); AssertFullMaterialNoEffects(h);
        }
        using(var overflow = FullMaterialHarness(40))
        {
            var b = (await overflow.Consume()).Value!; var a = SelectionMaterialAdmissions(overflow,b); var before = overflow.Files(); var calls = 0; var captured = 0; var previous = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
            try
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = c => { captured++; Assert.True(c.CopyCanonicalBytes().Length > 16384); Assert.Equal(a.Length,c.Candidates.Count); };
                var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) => { calls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[^1]))); });
                var refused = await BindFullMaterial(overflow,b,a,provider); output.WriteLine("FULL_PREAPPEND_OVERFLOW code="+refused.ErrorCode+" recipients="+captured+" callbacks="+calls);
                Assert.False(refused.IsSuccess); Assert.Equal(CodeDocumentationOneLessonSession.CapacityExceededErrorCode,refused.ErrorCode); Assert.Equal(1,captured); Assert.Equal(0,calls);
                Assert.Empty(overflow.HistoryBytes()); overflow.AssertFiles(before); Assert.Equal((0,0,false),overflow.Open.Session.FullMaterialSelectionAccounting); AssertFullMaterialNoEffects(overflow);
            }
            finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previous; }
        }
        using(var tooLong = FullMaterialHarness())
        {
            var b = (await tooLong.Consume()).Value!; var a = SelectionMaterialAdmissions(tooLong,b); var calls = 0;
            var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((c,_) => { calls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[0],new string('\0',1025)))); });
            Assert.False((await BindFullMaterial(tooLong,b,a,provider)).IsSuccess); Assert.Equal(1,calls); Assert.Equal(2,FullMaterialLines(tooLong.HistoryBytes()).Length); AssertFullMaterialNoEffects(tooLong); // An oversized returned reason is unknown after the already reserved callback; it cannot be predicted before dispatch.
        }
        using var legacy = SelectionHarness(); var lb = (await legacy.Consume()).Value!; var witness = legacy.Inspect(lb).Value!;
        var oldProvider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((_,_) => Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionChoice(witness,new string('\0',4096)))));
        Assert.True((await BindSelection(legacy,lb,witness,oldProvider)).IsSuccess); Assert.Equal(2,FullMaterialLines(legacy.HistoryBytes()).Length);
        Assert.False(CodeDocumentationOneLessonV2Wire.DecodeFullMaterialPrefix(legacy.HistoryBytes(),FullMaterialProjection(legacy),legacy.Open.Session.Snapshot.SessionId,false).IsSuccess); AssertFullMaterialNoEffects(legacy);
    }

    [Fact]
    public async Task OrdinarySelectionMaterials_MatchingSyntheticRefusesOrdinaryRecipientBeforeRawRead()
    {
        using var h = SelectedMaterialHarness(); var baseline = await h.Consume(); Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var b = baseline.Value!; var admissions = SelectionMaterialAdmissions(h, b);
        Assert.True(admissions.Length > 1); Assert.All(admissions, a => Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialOrigin.SyntheticTest, a.Origin));
        var expected = IndependentlyExpectedFullMaterialPacket(h, b, admissions); var witness = h.Inspect(b).Value!; var diagnosis = h.Classify(b).Value!;
        var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.OriginalReceiptId, diagnosis.OriginalReceiptSha256, diagnosis.Categories, witness.TeachingIdentities).Value!;
        Assert.All(admissions, a => Assert.True(a.MatchesSelectionInput(witness.BindingSha256, input, a.Item).IsSuccess));
        var files = h.Files(); var history = h.HistoryBytes(); var effects = h.Open.Session.Snapshot.Effects; var callsBefore = h.Client.Calls;
        var raw = 0; var captures = 0; var callbacks = 0; var receiving = 0; var savedBaseline = 0; var source = 0;
        var readHook = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        var captureHook = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        var finalHook = h.Open.Session.SelectionMaterialPointForTesting;
        var late = admissions[^1]; var origin = late.GetType().GetField("<Origin>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialInspectionForTesting((context, _) =>
        {
            callbacks++; Assert.Equal(expected, context.CopyCanonicalBytes()); Assert.Equal(admissions.Select(a => a.Item.ItemId), context.Candidates.Select(c => c.ItemId));
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[^1])));
        });
        void Refused<T>(OperationResult<T> result)
        {
            Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, result.ErrorCode);
            Assert.Equal(0, raw); Assert.Equal(0, captures); Assert.Equal(0, callbacks); Assert.Equal(history, h.HistoryBytes());
            Assert.Equal(effects, h.Open.Session.Snapshot.Effects); Assert.Equal(callsBefore, h.Client.Calls); h.AssertFiles(files);
        }
        try
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = _ => raw++;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++;
            h.Open.Session.SelectionMaterialPointForTesting = stage =>
            {
                if (stage == "receiving-final-attempted") receiving++; if (stage == "baseline-final-attempted") savedBaseline++; if (stage == "source-final-attempted") source++;
            };
            Refused(h.Open.Session.PrepareOrdinarySelectionMaterialContext(b, h.Catalog, h.Fixture.OrdinarySelector, admissions));
            Refused(await h.Open.Session.InspectOrdinarySelectionMaterialsAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, provider));
            Assert.Equal((2, 2, 2), (receiving, savedBaseline, source));
            origin.SetValue(late, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialOrigin.Unknown);
            Assert.True(late.MatchesSelectionInput(witness.BindingSha256, input, late.Item).IsSuccess, "The unused last admission still matches its complete bytes; only its recipient lineage changed.");
            Refused(h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, admissions));
            Refused(await h.Open.Session.InspectOrdinarySelectionMaterialsSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, provider));
            origin.SetValue(late, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialOrigin.SyntheticTest);
            using var canceled = new CancellationTokenSource(); canceled.Cancel();
            var cancellation = Assert.Throws<OperationCanceledException>(() => h.Open.Session.PrepareOrdinarySelectionMaterialContext(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, canceled.Token));
            Assert.Equal(canceled.Token, cancellation.CancellationToken); Assert.Equal((5, 5, 5), (receiving, savedBaseline, source));
            Assert.Equal(0, raw); Assert.Equal(0, captures); Assert.Equal(0, callbacks); h.AssertFiles(files);
            var prepared = h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, admissions);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage); Assert.Equal(expected, prepared.Value!.Context.CopyCanonicalBytes());
            var inspected = await h.Open.Session.InspectOrdinarySelectionMaterialsSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, provider);
            Assert.True(inspected.IsSuccess, inspected.ErrorMessage); Assert.Equal(admissions[^1].Item.ItemId, inspected.Value!.Choice.ItemId);
            Assert.NotEqual(admissions[0].Item.ItemId, inspected.Value.Choice.ItemId); Assert.Equal(2, captures); Assert.Equal(1, callbacks); Assert.True(raw > 0);
            Assert.Equal("SyntheticTest", inspected.Value.Origin); Assert.False(prepared.Value.Context.FullTeacherContextReady);
        }
        finally
        {
            origin.SetValue(late, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialOrigin.SyntheticTest);
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = readHook;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = captureHook;
            h.Open.Session.SelectionMaterialPointForTesting = finalHook;
        }
        Assert.Equal(history, h.HistoryBytes()); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); Assert.Equal(callsBefore, h.Client.Calls); h.AssertFiles(files);
    }

    [Fact]
    public async Task OrdinaryFullMaterialSelection_MatchingSyntheticRefusesBeforeReservationAndPreservesFixtureChoice()
    {
        using var h = FullMaterialHarness(); var baseline = await h.Consume(); Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var b = baseline.Value!; var admissions = SelectionMaterialAdmissions(h, b); var expected = IndependentlyExpectedFullMaterialPacket(h, b, admissions);
        Assert.True(admissions.Length > 1); var files = h.Files(); var history = h.HistoryBytes(); var effects = h.Open.Session.Snapshot.Effects;
        var accounting = h.Open.Session.FullMaterialSelectionAccounting; var callsBefore = h.Client.Calls;
        var raw = 0; var captures = 0; var callbacks = 0; var appends = 0; var finals = 0; var reserved = new List<string>();
        var readHook = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        var captureHook = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        var capacityHook = h.Open.Session.FullMaterialCapacityReservedForTesting; var durableHook = h.Open.Session.SelectionDurablePointForTesting; var finalHook = h.Open.Session.SelectionMaterialPointForTesting;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((context, _) =>
        {
            callbacks++; Assert.Equal(expected, context.CopyCanonicalBytes()); Assert.Equal(admissions.Select(a => a.Item.ItemId), context.Candidates.Select(c => c.ItemId));
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[^1], "Explicit fixture selects the last task after receiving every complete reference.")));
        });
        try
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = _ => raw++;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++;
            h.Open.Session.FullMaterialCapacityReservedForTesting = (item, _, _, _, _) => reserved.Add(item);
            h.Open.Session.SelectionDurablePointForTesting = stage => { if (stage.EndsWith("-after-write", StringComparison.Ordinal)) appends++; };
            h.Open.Session.SelectionMaterialPointForTesting = stage => { if (stage == "final-none-checked") finals++; };
            var refused = await h.Open.Session.BindOrdinaryFullMaterialSelectionAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, provider);
            Assert.False(refused.IsSuccess); Assert.Null(refused.Value); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, refused.ErrorCode);
            Assert.Equal(0, raw); Assert.Equal(0, captures); Assert.Equal(0, callbacks); Assert.Empty(reserved); Assert.Equal(0, appends); Assert.Equal(1, finals);
            Assert.Equal(history, h.HistoryBytes()); Assert.Equal(accounting, h.Open.Session.FullMaterialSelectionAccounting); Assert.Equal(effects, h.Open.Session.Snapshot.Effects);
            Assert.Equal(callsBefore, h.Client.Calls); h.AssertFiles(files);
            var selected = await h.Open.Session.BindOrdinaryFullMaterialSelectionSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, provider);
            Assert.True(selected.IsSuccess, selected.ErrorMessage); Assert.Equal(expected, selected.Value!.Context.CopyCanonicalBytes());
            Assert.Equal(admissions[^1].Item.ItemId, selected.Value.Choice.ItemId); Assert.NotEqual(admissions[0].Item.ItemId, selected.Value.Choice.ItemId);
            Assert.Equal(admissions.Select(a => a.Item.ItemId), reserved); Assert.Equal(1, callbacks); Assert.Equal(1, captures); Assert.True(raw > 0);
            Assert.Equal(3, appends); Assert.Equal(3, FullMaterialLines(h.HistoryBytes()).Length); Assert.Equal((1, 1, false), h.Open.Session.FullMaterialSelectionAccounting);
            Assert.Equal("SyntheticTest", selected.Value.Origin); Assert.False(selected.Value.FullTeacherContextReady); Assert.Equal(effects, h.Open.Session.Snapshot.Effects);
            Assert.Equal(callsBefore, h.Client.Calls); AssertFullMaterialOtherFiles(h, files);
        }
        finally
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = readHook;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = captureHook;
            h.Open.Session.FullMaterialCapacityReservedForTesting = capacityHook; h.Open.Session.SelectionDurablePointForTesting = durableHook; h.Open.Session.SelectionMaterialPointForTesting = finalHook;
        }
    }

    [Fact]
    public async Task OrdinaryFullMaterialSelection_CompletedSyntheticRefusesOrdinaryRawReplayWithoutRepeat()
    {
        using var h = FullMaterialHarness(); var baseline = await h.Consume(); Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var b = baseline.Value!; var admissions = SelectionMaterialAdmissions(h, b); var expected = IndependentlyExpectedFullMaterialPacket(h, b, admissions); var callbacks = 0;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((context, _) =>
        {
            callbacks++; Assert.Equal(expected, context.CopyCanonicalBytes());
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[^1], "Saved explicit fixture decision uses the last complete teaching reference.")));
        });
        var selected = await BindFullMaterial(h, b, admissions, provider); Assert.True(selected.IsSuccess, selected.ErrorMessage); Assert.Equal(1, callbacks);
        var history = h.HistoryBytes(); var lines = FullMaterialLines(history); Assert.Equal(3, lines.Length);
        using var completed = JsonDocument.Parse(lines[^1]); var requestBytes = Convert.FromBase64String(completed.RootElement.GetProperty("FullMaterialSelection").GetProperty("RequestCanonicalBase64").GetString()!);
        var savedRequest = JsonSerializer.Deserialize<CodeDocumentationFullMaterialRequestV1>(requestBytes)!; Assert.Equal(expected, Convert.FromBase64String(savedRequest.PacketCanonicalBase64));
        Assert.True(h.Reopen().IsSuccess); var rebound = await h.Consume(); Assert.True(rebound.IsSuccess, rebound.ErrorMessage); Assert.NotSame(b, rebound.Value);
        b = rebound.Value!; admissions = SelectionMaterialAdmissions(h, b); var files = h.Files(); var effects = h.Open.Session.Snapshot.Effects;
        var accounting = h.Open.Session.FullMaterialSelectionAccounting; var callsBefore = h.Client.Calls;
        var raw = 0; var captures = 0; var canaryCalls = 0; var reservations = 0; var appends = 0; var finals = 0;
        var readHook = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        var captureHook = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        var capacityHook = h.Open.Session.FullMaterialCapacityReservedForTesting; var durableHook = h.Open.Session.SelectionDurablePointForTesting; var finalHook = h.Open.Session.SelectionMaterialPointForTesting;
        var canary = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialSelectionForTesting((_, _) => { canaryCalls++; throw new IOException("A saved Completed fixture must never resolve this provider."); });
        void Unchanged()
        {
            Assert.Equal(history, h.HistoryBytes()); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); Assert.Equal(accounting, h.Open.Session.FullMaterialSelectionAccounting);
            Assert.Equal(callsBefore, h.Client.Calls); Assert.Equal(1, callbacks); Assert.Equal(0, canaryCalls); Assert.Equal(0, reservations); Assert.Equal(0, appends); h.AssertFiles(files);
        }
        try
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = _ => raw++;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => captures++;
            h.Open.Session.FullMaterialCapacityReservedForTesting = (_, _, _, _, _) => reservations++;
            h.Open.Session.SelectionDurablePointForTesting = _ => appends++;
            h.Open.Session.SelectionMaterialPointForTesting = stage => { if (stage == "final-none-checked") finals++; };
            var reader = await h.Open.Session.TryReadOrdinaryFullMaterialSelectionAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions);
            var binder = await h.Open.Session.BindOrdinaryFullMaterialSelectionAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, canary);
            foreach (var result in new[] { reader, binder })
            {
                Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, result.ErrorCode);
            }
            Assert.Equal(0, raw); Assert.Equal(0, captures); Assert.Equal(2, finals); Unchanged();
            var replay = await h.Open.Session.TryReadOrdinaryFullMaterialSelectionSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions);
            Assert.True(replay.IsSuccess, replay.ErrorMessage); Assert.Equal(expected, replay.Value!.Context.CopyCanonicalBytes());
            var selectedChoiceBytes = CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionPayloadBytes(selected.Value.Choice);
            Assert.Equal(selectedChoiceBytes, CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionPayloadBytes(replay.Value.Choice));
            Assert.Equal(admissions[^1].Item.ItemId, replay.Value.Choice.ItemId); Assert.Equal("SyntheticTest", replay.Value.Origin); Assert.False(replay.Value.FullTeacherContextReady); Unchanged();
            var replayWithCanary = await h.Open.Session.BindOrdinaryFullMaterialSelectionSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, canary);
            Assert.True(replayWithCanary.IsSuccess, replayWithCanary.ErrorMessage);
            Assert.Equal(selectedChoiceBytes, CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionPayloadBytes(replayWithCanary.Value.Choice));
            Assert.Equal(2, captures); Assert.True(raw > 0); Unchanged();
            using var savedAgain = JsonDocument.Parse(FullMaterialLines(h.HistoryBytes())[^1]);
            Assert.Equal(requestBytes, Convert.FromBase64String(savedAgain.RootElement.GetProperty("FullMaterialSelection").GetProperty("RequestCanonicalBase64").GetString()!));
        }
        finally
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = readHook;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = captureHook;
            h.Open.Session.FullMaterialCapacityReservedForTesting = capacityHook; h.Open.Session.SelectionDurablePointForTesting = durableHook; h.Open.Session.SelectionMaterialPointForTesting = finalHook;
        }
    }

    [Fact]
    public async Task OrdinarySelectionMaterials_FullPacketReachesSyntheticProvider()
    {
        using var h = SelectedMaterialHarness(); var b = (await h.Consume()).Value!; var admissions = SelectionMaterialAdmissions(h, b);
        var witness = h.Inspect(b).Value!; var diagnosis = h.Classify(b).Value!;
        var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.OriginalReceiptId, diagnosis.OriginalReceiptSha256,
            diagnosis.Categories, witness.TeachingIdentities).Value!;
        // Expected full task/reference tuples are independently read from the real sealed owner before preparation, not from the callback/result.
        var expected = admissions.Select(a =>
        {
            var d = witness.TeachingIdentities.Sources.Single(s => s.Descriptor.SourceId == a.Item.SourceId).Descriptor;
            var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, d.DomainSlug, d.SubjectSlug, d.CorpusVersion).Value!;
            var entry = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealedFileIdentity(address).Value!.Corpus.Entries.Single(e => e.EntryId == a.Item.ReferenceEntryId);
            return (Admission: a, Entry: entry);
        }).ToArray();
        var binding = h.Open.Session.Snapshot.Binding;
        var bindingBytes = CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(new CodeDocumentationOneLessonBindingProjectionV2(
            binding.ObjectiveId, binding.GrantId, binding.GrantVersion, binding.TrainingRunId, binding.CandidateSha256, binding.ParentManifestSha256,
            binding.ParentWeightsSha256, binding.ConfigurationSha256, binding.ExamConditionSha256, binding.DescriptorsV2!)));
        using var buffer = new MemoryStream(); using var writer = new BinaryWriter(buffer, new UTF8Encoding(false, true), leaveOpen: true);
        void Bytes(byte[] bytes) { writer.Write(bytes.Length); writer.Write(bytes); }
        void Field(string value) => Bytes(Encoding.UTF8.GetBytes(value));
        writer.Write(Encoding.UTF8.GetBytes("code-documentation-selection-material-input-v1\n"));
        Field("Judge task-specific pedagogical suitability first. Use the supplied curriculum order only to break otherwise equivalent eligible choices.");
        Field(witness.BindingSha256); Bytes(bindingBytes); Bytes(input.CopyBytes());
        writer.Write(diagnosis.Categories.Count); foreach (var c in diagnosis.Categories) Field(c);
        writer.Write(expected.Length);
        foreach (var e in expected)
        {
            var item = e.Admission.Item; Field(item.ItemId); Field(item.SourceId); Field(item.ReferenceEntryId); Field(item.SourceEntityFingerprint);
            Field(e.Entry.OwnerInput); Field(e.Entry.PaidModelReferenceAnswer); Field(e.Entry.PaidReferenceProvenance.CanonicalRepresentation); Field(e.Admission.EvidenceSha256);
        }
        writer.Flush(); var expectedBytes = buffer.ToArray(); var files = h.Files(); var history = h.HistoryBytes(); var effects = h.Open.Session.Snapshot.Effects;
        var received = 0; var callbacks = 0; var previous = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        var originalAdmission = admissions[0];
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = context =>
            {
                received++; Assert.Equal(expectedBytes, context.CopyCanonicalBytes());
                admissions[0] = admissions[1]; // Mutation of the caller's outer array after capture cannot alter the admitted packet.
            };
            var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialInspectionForTesting((context, _) =>
            {
                callbacks++; Assert.Equal(expectedBytes, context.CopyCanonicalBytes()); Assert.Equal("SyntheticTest", context.Origin); Assert.False(context.FullTeacherContextReady);
                Assert.Equal(expected.Length, context.Candidates.Count); Assert.Equal(input.CopyBytes(), context.MetadataInput.CopyBytes());
                for (var i = 0; i < expected.Length; i++)
                {
                    Assert.Equal(expected[i].Admission.Item.ItemId, context.Candidates[i].ItemId);
                    Assert.Equal(expected[i].Entry.OwnerInput, context.Candidates[i].OwnerInput);
                    Assert.Equal(expected[i].Entry.PaidModelReferenceAnswer, context.Candidates[i].PaidModelReferenceAnswer);
                    Assert.Equal(expected[i].Entry.PaidReferenceProvenance, context.Candidates[i].PaidReferenceProvenance);
                }
                var copy = context.CopyCanonicalBytes(); copy[0] ^= 1; Assert.Equal(expectedBytes, context.CopyCanonicalBytes());
                Assert.Contains("Полная задача 中文", context.Candidates[1].OwnerInput);
                return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(context.Candidates[1])));
            });
            var inspected = await h.Open.Session.InspectOrdinarySelectionMaterialsSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, provider);
            Assert.True(inspected.IsSuccess, inspected.ErrorMessage); Assert.Equal(expected[1].Admission.Item.ItemId, inspected.Value!.Choice.ItemId);
            Assert.NotEqual(expected[0].Admission.Item.ItemId, inspected.Value.Choice.ItemId); Assert.Equal("SyntheticTest", inspected.Value.Origin);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(expectedBytes)).ToLowerInvariant(), inspected.Value.PacketSha256);
            Assert.Equal(1, received); Assert.Equal(1, callbacks); Assert.False(h.Open.Session.HasSavedSelectionBinding);
        }
        finally { admissions[0] = originalAdmission; CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previous; }
        Assert.Equal(history, h.HistoryBytes()); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); Assert.Equal(10, h.Client.Calls); h.AssertFiles(files);
        var prepared = h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, admissions);
        Assert.True(prepared.IsSuccess, prepared.ErrorMessage); Assert.Equal(expectedBytes, prepared.Value!.Context.CopyCanonicalBytes());
        // A returned receipt is data; it cannot invoke the provider outside the active held Session lifecycle.
        var outsideCalls = 0; var outside = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialInspectionForTesting((c, _) =>
        { outsideCalls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[0]))); });
        Assert.False((await outside.InspectPreparedMaterialAsync(h.Open.Session, prepared.Value, CancellationToken.None)).IsSuccess); Assert.Equal(0, outsideCalls);
        h.AssertFiles(files);
    }

    [Fact]
    public async Task OrdinarySelectionMaterials_AdmissionRefusesBeforeRawRead()
    {
        using var h = SelectedMaterialHarness(); var b = (await h.Consume()).Value!; var valid = SelectionMaterialAdmissions(h, b);
        using var foreign = SelectedMaterialHarness(); var fb = (await foreign.Consume()).Value!; var foreignAdmission = SelectionMaterialAdmissions(foreign, fb)[0];
        var files = h.Files(); var raw = 0; var recipient = 0; var callbacks = 0;
        var previousRead = CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting;
        var previousRecipient = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        try
        {
            CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = _ => raw++;
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => recipient++;
            var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialInspectionForTesting((c, _) =>
            { callbacks++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[0]))); });
            var stale = SelectedMaterialAdmission(h, b, H("stale-selection-binding"), valid[0].Item.ItemId);
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission[] Replace(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission value)
                => valid.Select((a, i) => i == 0 ? value : a).ToArray();
            var negatives = new CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission[]?[]
                { null, [], valid[..^1], valid.Append(valid[0]).ToArray(), Replace(valid[1]), Replace(null!), Replace(stale), Replace(foreignAdmission) };
            foreach (var admissions in negatives)
            {
                raw = recipient = callbacks = 0;
                var refused = await h.Open.Session.InspectOrdinarySelectionMaterialsSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, admissions, provider);
                Assert.False(refused.IsSuccess); Assert.Null(refused.Value);
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, refused.ErrorCode);
                Assert.Equal(0, raw); Assert.Equal(0, recipient); Assert.Equal(0, callbacks); h.AssertFiles(files);
                var control = h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid);
                Assert.True(control.IsSuccess, control.ErrorMessage); Assert.True(raw > 0); Assert.Equal(1, recipient); Assert.Equal(0, callbacks);
            }
            var witness = h.Inspect(b).Value!; var diagnosis = h.Classify(b).Value!;
            var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.OriginalReceiptId, diagnosis.OriginalReceiptSha256, diagnosis.Categories, witness.TeachingIdentities).Value!;
            // Denied factories never issue a capability; no reflection or fictitious issued-denied object is used.
            foreach (var (purpose, policy) in new[] {
                (CodeDocumentationRepositoryOwnedDpoMaterialPurpose.CertificationExam, new TrainingDataGovernanceDecision(true, false, ["sha256:" + H("policy")])),
                (CodeDocumentationRepositoryOwnedDpoMaterialPurpose.DiagnosticFeedback, new TrainingDataGovernanceDecision(true, false, ["sha256:" + H("policy")])),
                (CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference, new TrainingDataGovernanceDecision(false, false, ["sha256:" + H("policy")])),
                (CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference, new TrainingDataGovernanceDecision(true, true, ["sha256:" + H("policy")])) })
            {
                raw = recipient = callbacks = 0;
                var denied = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(
                    witness.TeachingIdentities, input, witness.BindingSha256, valid[0].Item.ItemId, purpose, policy);
                Assert.False(denied.IsSuccess); Assert.Null(denied.Value);
                Assert.False(h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid[..^1]).IsSuccess);
                Assert.Equal(0, raw); Assert.Equal(0, recipient); Assert.Equal(0, callbacks);
            }
        }
        finally { CodeDocumentationOneLessonSourceSelection.SelectionMaterialsReadPointForTesting = previousRead; CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previousRecipient; }
        Assert.True(h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid).IsSuccess);
        Assert.Equal(10, h.Client.Calls); Assert.False(h.Open.Session.HasSavedSelectionBinding); h.AssertFiles(files);
    }

    [Fact]
    public async Task OrdinarySelectionMaterials_FinalChecksReopenAndNoEffects()
    {
        using var h = SelectedMaterialHarness(); var b = (await h.Consume()).Value!; var valid = SelectionMaterialAdmissions(h, b);
        var files = h.Files(); var history = h.HistoryBytes(); var effects = h.Open.Session.Snapshot.Effects;
        var prepared = h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid); Assert.True(prepared.IsSuccess);
        var expected = prepared.Value!.Context.CopyCanonicalBytes(); var previous = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting;
        using var cancel = new CancellationTokenSource(); cancel.Cancel(); var primary = new OperationCanceledException("synthetic primary selection cancellation", null, cancel.Token);
        var secondary = new IOException("synthetic secondary final-check failure"); var finals = 0;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => throw primary;
            h.Open.Session.SelectionMaterialPointForTesting = point => { if (point == "final-none-checked") { finals++; throw secondary; } };
            var actual = Assert.Throws<OperationCanceledException>(() => h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid));
            Assert.Same(primary, actual); Assert.Equal(cancel.Token, actual.CancellationToken); Assert.Equal(1, finals);
            Assert.Same(secondary, actual.Data["SelectionMaterialFinalFailure"]); Assert.Equal(nameof(IOException), h.Open.Session.SelectionMaterialSecondaryErrorForTesting);
        }
        finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previous; h.Open.Session.SelectionMaterialPointForTesting = null; }
        Assert.True(h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid).IsSuccess);
        // Primary cancellation and changed receiving fields cannot hide either saved-baseline custody or an independently frozen unused SOURCE.
        var unusedDescriptor = h.Grant.SourceSelection!.TeachingCurriculum!.Sources[1];
        var unusedAddress = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, unusedDescriptor.DomainSlug, unusedDescriptor.SubjectSlug, unusedDescriptor.CorpusVersion).Value!;
        var unusedPath = Path.Combine(unusedAddress.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        var unusedBytes = File.ReadAllBytes(unusedPath); var unusedTicks = File.GetLastWriteTimeUtc(unusedPath); var originalEvidence = b.Context.EvidenceRefs;
        var baselineBytes = File.ReadAllBytes(h.Fixture.OrdinaryBaseline); var baselineTicks = File.GetLastWriteTimeUtc(h.Fixture.OrdinaryBaseline); var baselineMode = JournalStorageMode(h.Fixture.OrdinaryBaseline, false);
        var corruptedBaseline = baselineBytes.Concat(new byte[] { (byte)' ' }).ToArray(); var corruptedUnused = unusedBytes.Concat(new byte[] { (byte)' ' }).ToArray(); var unusedMode = JournalStorageMode(unusedPath, false);
        var originalSource = h.Grant.SourceSelection; var originalGeneration = b.Context.GenerationSettings; var representative = b.Readback.Baseline.Calibration.RepresentativeAssessment.Cases[0]; var savedReason = representative.Reason;
        using var combinedCancellation = new CancellationTokenSource(); combinedCancellation.Cancel();
        var combinedPrimary = new OperationCanceledException("primary cancellation after genuine full recipient", null, combinedCancellation.Token);
        var receivingAttempts = 0; var baselineAttempts = 0; var sourceAttempts = 0; var combinedCaptures = 0; Exception? combinedFailure = null;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ =>
            {
                combinedCaptures++; SetDiagnosticBacking(b.Context, "EvidenceRefs", new[] { "late-combined-receiving-fault" });
                // Substituted receiving inputs cannot redirect the independent final owner traversal away from the captured SOURCE.
                SetDiagnosticBacking(h.Grant, "SourceSelection", null!); SetDiagnosticBacking(b.Context, "GenerationSettings", originalGeneration with { MaxHeldOutCases = 1 });
                SetDiagnosticBacking(representative, "Reason", "changed-after-authenticated-snapshot");
                RestoreCustodyFile(h.Fixture.OrdinaryBaseline, corruptedBaseline, baselineTicks); RestoreReferenceOwnerFile(unusedPath, corruptedUnused, unusedTicks);
                throw combinedPrimary;
            };
            h.Open.Session.SelectionMaterialPointForTesting = point =>
            {
                if (point == "receiving-final-attempted") receivingAttempts++;
                if (point == "baseline-final-attempted") baselineAttempts++;
                if (point == "source-final-attempted") sourceAttempts++;
            };
            var actual = Assert.Throws<OperationCanceledException>(() => h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid));
            output.WriteLine("SELECTION_MATERIAL_COMBINED receiving=" + h.Open.Session.SelectionMaterialReceivingFinalErrorForTesting + " baseline=" + h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting + " source=" + h.Open.Session.SelectionMaterialSourceFinalErrorForTesting + " captures=" + combinedCaptures + " receivingAttempts=" + receivingAttempts + " baselineAttempts=" + baselineAttempts + " sourceAttempts=" + sourceAttempts);
            Assert.Same(combinedPrimary, actual); Assert.Equal(combinedCancellation.Token, actual.CancellationToken);
            Assert.Equal(1, combinedCaptures); Assert.Equal(1, receivingAttempts); Assert.Equal(1, baselineAttempts); Assert.Equal(1, sourceAttempts);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, h.Open.Session.SelectionMaterialReceivingFinalErrorForTesting);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting);
            Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, h.Open.Session.SelectionMaterialSourceFinalErrorForTesting);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, actual.Data["SelectionMaterialFinalReceivingError"]);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, actual.Data["SelectionMaterialFinalBaselineError"]);
            Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, actual.Data["SelectionMaterialFinalSourceError"]);
            Assert.Equal(corruptedBaseline, File.ReadAllBytes(h.Fixture.OrdinaryBaseline)); Assert.Equal(baselineTicks, File.GetLastWriteTimeUtc(h.Fixture.OrdinaryBaseline)); Assert.Equal(baselineMode, JournalStorageMode(h.Fixture.OrdinaryBaseline, false));
            Assert.Equal(corruptedUnused, File.ReadAllBytes(unusedPath)); Assert.Equal(unusedTicks, File.GetLastWriteTimeUtc(unusedPath)); Assert.Equal(unusedMode, JournalStorageMode(unusedPath, false));
            Assert.False(h.Open.Session.HasSavedSelectionBinding); Assert.Equal(history, h.HistoryBytes()); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); Assert.Equal(10, h.Client.Calls);
        }
        catch (Exception failure) { combinedFailure = failure; throw; }
        finally
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previous; h.Open.Session.SelectionMaterialPointForTesting = null;
            SetDiagnosticBacking(b.Context, "EvidenceRefs", originalEvidence); SetDiagnosticBacking(h.Grant, "SourceSelection", originalSource!);
            SetDiagnosticBacking(b.Context, "GenerationSettings", originalGeneration); SetDiagnosticBacking(representative, "Reason", savedReason);
            RestoreFixtureAfterFault(() => RestoreCustodyFile(h.Fixture.OrdinaryBaseline, baselineBytes, baselineTicks), combinedFailure);
            RestoreFixtureAfterFault(() => RestoreReferenceOwnerFile(unusedPath, unusedBytes, unusedTicks), combinedFailure);
        }
        Assert.Equal(baselineBytes, File.ReadAllBytes(h.Fixture.OrdinaryBaseline)); Assert.Equal(baselineTicks, File.GetLastWriteTimeUtc(h.Fixture.OrdinaryBaseline)); Assert.Equal(baselineMode, JournalStorageMode(h.Fixture.OrdinaryBaseline, false));
        Assert.Equal(unusedBytes, File.ReadAllBytes(unusedPath)); Assert.Equal(unusedTicks, File.GetLastWriteTimeUtc(unusedPath)); Assert.Equal(unusedMode, JournalStorageMode(unusedPath, false)); h.AssertFiles(files);
        var combinedRestored = h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid);
        Assert.True(combinedRestored.IsSuccess, combinedRestored.ErrorMessage); Assert.Equal(expected, combinedRestored.Value!.Context.CopyCanonicalBytes());
        Assert.Null(h.Open.Session.SelectionMaterialReceivingFinalErrorForTesting); Assert.Null(h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting); Assert.Null(h.Open.Session.SelectionMaterialSourceFinalErrorForTesting);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var calls = 0;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialInspectionForTesting(async (c, _) =>
        { calls++; entered.SetResult(); await release.Task; return OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[1])); });
        var inFlight = h.Open.Session.InspectOrdinarySelectionMaterialsSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, valid, provider);
        try
        {
            await entered.Task;
            var competing = await h.Open.Session.InspectOrdinarySelectionMaterialsSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, valid, provider);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, competing.ErrorCode); Assert.Equal(1, calls);
        }
        finally { release.TrySetResult(); }
        Assert.True((await inFlight).IsSuccess); Assert.Equal(1, calls);
        // Exercise untrusted callback scalars against the actual full-packet consumer, without minting a saved decision.
        var accepted = SelectionMaterialChoice(prepared.Value!.Context.Candidates[1]);
        var choices = new[]
        {
            accepted with { ItemId = "not-an-eligible-item" }, accepted with { SourceId = "not-an-eligible-source" },
            accepted with { ReferenceEntryId = "not-an-eligible-entry" }, accepted with { SourceEntityFingerprint = H("foreign-item") },
            accepted with { MaterialAdmissionEvidenceSha256 = H("foreign-admission") }, accepted with { Reason = "" },
            accepted with { Reason = b.Context.Corpus.Entries[0].SourceEntityFingerprint }
        };
        foreach (var choice in choices)
        {
            var invocations = 0;
            var invalid = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialInspectionForTesting((_, _) =>
            { invocations++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(choice)); });
            var refused = await h.Open.Session.InspectOrdinarySelectionMaterialsSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, valid, invalid);
            Assert.False(refused.IsSuccess); Assert.Null(refused.Value); Assert.Equal(1, invocations);
            if (choice.Reason == b.Context.Corpus.Entries[0].SourceEntityFingerprint) Assert.Equal("ONE_LESSON_TEACHING_ORIGINAL_OVERLAP", refused.ErrorCode);
            Assert.False(h.Open.Session.HasSavedSelectionBinding); h.AssertFiles(files);
        }
        using (var preCancelled = new CancellationTokenSource())
        {
            preCancelled.Cancel(); var invocations = 0;
            var never = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticMaterialInspectionForTesting((c, _) =>
            { invocations++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionMaterialChoice(c.Candidates[1]))); });
            var cancelled = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => h.Open.Session.InspectOrdinarySelectionMaterialsSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, valid, never, preCancelled.Token));
            Assert.Equal(preCancelled.Token, cancelled.CancellationToken); Assert.Equal(0, invocations); h.AssertFiles(files);
        }
        // Restore genuine ORIGINAL/custody and receiving faults precisely; each failed operation must release its CAS.
        foreach (var faultName in new[] { "original", "custody", "receiving", "prefix" })
        {
            var faultPath = faultName == "original" ? Path.Combine(h.Fixture.CorpusAddress.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName) : h.Fixture.OrdinaryBaseline;
            var originalBytes = File.ReadAllBytes(faultPath); var originalTicks = File.GetLastWriteTimeUtc(faultPath); var refs = b.Context.EvidenceRefs;
            var originalMode = JournalStorageMode(faultPath, false); var corruptBytes = originalBytes.Concat(new byte[] { (byte)' ' }).ToArray();
            var prefixField = typeof(CodeDocumentationOneLessonSession).GetField("_v2Prefix", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            var prefixBytes = (byte[])prefixField.GetValue(h.Open.Session)!; var prefixCopy = prefixBytes.ToArray(); Exception? injectedFailure = null; var reached = 0;
            // A held session may legitimately have an empty cache; substitute different bytes to exercise final receiving continuity without altering its original array.
            var substitutedPrefix = prefixCopy.Concat(new byte[] { 1 }).ToArray();
            var receivingFinals = 0; var baselineFinals = 0; var sourceFinals = 0; var priorPoint = h.Open.Session.SelectionMaterialPointForTesting;
            try
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ =>
                {
                    reached++;
                    if (faultName == "receiving") SetDiagnosticBacking(b.Context, "EvidenceRefs", new[] { "changed-full-selection-context" });
                    else if (faultName == "prefix") prefixField.SetValue(h.Open.Session, substitutedPrefix);
                    else if (faultName == "original") RestoreReferenceOwnerFile(faultPath, corruptBytes, originalTicks);
                    else RestoreCustodyFile(faultPath, corruptBytes, originalTicks);
                };
                h.Open.Session.SelectionMaterialPointForTesting = point =>
                {
                    if (point == "receiving-final-attempted") receivingFinals++;
                    if (point == "baseline-final-attempted") baselineFinals++;
                    if (point == "source-final-attempted") sourceFinals++;
                };
                var refused = h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid);
                output.WriteLine("SELECTION_MATERIAL_LATE fault=" + faultName + " code=" + refused.ErrorCode + " success=" + refused.IsSuccess + " reached=" + reached + " receiving=" + h.Open.Session.SelectionMaterialReceivingFinalErrorForTesting + " baseline=" + h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting + " source=" + h.Open.Session.SelectionMaterialSourceFinalErrorForTesting + " receivingAttempts=" + receivingFinals + " baselineAttempts=" + baselineFinals + " sourceAttempts=" + sourceFinals);
                if (faultName == "prefix") output.WriteLine("SELECTION_MATERIAL_PREFIX originalLength=" + prefixCopy.Length + " substitutedLength=" + substitutedPrefix.Length + " substitutedReferenceHeld=" + ReferenceEquals(substitutedPrefix, prefixField.GetValue(h.Open.Session)) + " receiving=" + h.Open.Session.SelectionMaterialReceivingFinalErrorForTesting + " baseline=" + h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting + " source=" + h.Open.Session.SelectionMaterialSourceFinalErrorForTesting);
                Assert.False(refused.IsSuccess, faultName); Assert.Null(refused.Value); Assert.Equal(1, reached);
                var expectedCode = faultName == "custody" ? CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode
                    : faultName == "original" ? RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode : CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode;
                Assert.Equal(expectedCode, refused.ErrorCode); Assert.True(receivingFinals > 0); Assert.True(baselineFinals > 0); Assert.True(sourceFinals > 0);
                if (faultName == "custody") Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting);
                if (faultName == "prefix")
                {
                    Assert.NotEmpty(substitutedPrefix); Assert.False(prefixCopy.AsSpan().SequenceEqual(substitutedPrefix));
                    Assert.Same(substitutedPrefix, prefixField.GetValue(h.Open.Session)); Assert.Equal(substitutedPrefix, (byte[])prefixField.GetValue(h.Open.Session)!); Assert.Equal(prefixCopy, prefixBytes);
                    Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, h.Open.Session.SelectionMaterialReceivingFinalErrorForTesting);
                    Assert.Null(h.Open.Session.SelectionMaterialBaselineFinalErrorForTesting); Assert.Null(h.Open.Session.SelectionMaterialSourceFinalErrorForTesting);
                }
                if (faultName is "original" or "custody") { Assert.Equal(corruptBytes, File.ReadAllBytes(faultPath)); Assert.Equal(originalTicks, File.GetLastWriteTimeUtc(faultPath)); Assert.Equal(originalMode, JournalStorageMode(faultPath, false)); }
                Assert.False(h.Open.Session.HasSavedSelectionBinding); Assert.Equal(history, h.HistoryBytes()); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); Assert.Equal(10, h.Client.Calls);
            }
            catch (Exception failure) { injectedFailure = failure; throw; }
            finally
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previous; h.Open.Session.SelectionMaterialPointForTesting = priorPoint;
                if (faultName == "receiving") SetDiagnosticBacking(b.Context, "EvidenceRefs", refs);
                else if (faultName == "prefix") RestoreFixtureAfterFault(() => { prefixCopy.CopyTo(prefixBytes, 0); prefixField.SetValue(h.Open.Session, prefixBytes); }, injectedFailure);
                else RestoreFixtureAfterFault(() => { if (faultName == "original") RestoreReferenceOwnerFile(faultPath, originalBytes, originalTicks); else RestoreCustodyFile(faultPath, originalBytes, originalTicks); }, injectedFailure);
            }
            var restored = h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid);
            Assert.True(restored.IsSuccess, restored.ErrorMessage); Assert.Equal(expected, restored.Value!.Context.CopyCanonicalBytes());
            if (faultName == "prefix") { Assert.Same(prefixBytes, prefixField.GetValue(h.Open.Session)); Assert.Equal(prefixCopy, (byte[])prefixField.GetValue(h.Open.Session)!); Assert.Equal(10, h.Client.Calls); }
            Assert.Equal(originalBytes, File.ReadAllBytes(faultPath)); Assert.Equal(originalTicks, File.GetLastWriteTimeUtc(faultPath)); Assert.Equal(originalMode, JournalStorageMode(faultPath, false));
            h.AssertFiles(files); Assert.Equal(history, h.HistoryBytes()); Assert.Equal(effects, h.Open.Session.Snapshot.Effects);
        }
        // Actual unused owner corruption after recipient capture must be rejected by the final None check, then precisely restored.
        var d = h.Grant.SourceSelection!.TeachingCurriculum!.Sources[1];
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, d.DomainSlug, d.SubjectSlug, d.CorpusVersion).Value!;
        var path = Path.Combine(address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName); var bytes = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path);
        Exception? fault = null;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = _ => RestoreReferenceOwnerFile(path, bytes.Concat(new byte[] { (byte)' ' }).ToArray(), ticks);
            var bad = h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid);
            Assert.False(bad.IsSuccess); Assert.Null(bad.Value); Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, bad.ErrorCode);
        }
        catch (Exception ex) { fault = ex; throw; }
        finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectionMaterialCapturedForTesting = previous; RestoreFixtureAfterFault(() => RestoreReferenceOwnerFile(path, bytes, ticks), fault); }
        Assert.Equal(expected, h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(b, h.Catalog, h.Fixture.OrdinarySelector, valid).Value!.Context.CopyCanonicalBytes());
        Assert.Equal(history, h.HistoryBytes()); Assert.Equal(effects, h.Open.Session.Snapshot.Effects); Assert.Equal(10, h.Client.Calls); h.AssertFiles(files);
        Assert.True(h.Reopen().IsSuccess); Assert.False(prepared.Value!.IsIssuedBy(h.Open.Session));
        var reopened = await h.Consume(); Assert.True(reopened.IsSuccess, reopened.ErrorMessage); Assert.Equal(10, h.Client.Calls);
        var fresh = h.Open.Session.PrepareOrdinarySelectionMaterialContextSyntheticForTesting(reopened.Value!, h.Catalog, h.Fixture.OrdinarySelector, valid);
        Assert.True(fresh.IsSuccess, fresh.ErrorMessage); Assert.Equal(expected, fresh.Value!.Context.CopyCanonicalBytes());
        Assert.Equal(1, calls); Assert.False(h.Open.Session.HasSavedSelectionBinding); h.AssertFiles(files);
    }

    internal static OrdinaryDiagnosticHarness SelectedMaterialHarness()
        => new((_, _) => (false, "missing_essential_behavior"), SelectedMaterialCurriculum);
    internal static CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission SelectedMaterialAdmission(
        OrdinaryDiagnosticHarness h, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline,
        string? binding = null, string itemId = "item-teach-7-1")
    {
        var witness = h.Inspect(baseline); Assert.True(witness.IsSuccess, witness.ErrorMessage);
        var diagnosis = h.Classify(baseline); Assert.True(diagnosis.IsSuccess, diagnosis.ErrorMessage);
        var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(diagnosis.Value!.OriginalReceiptId,
            diagnosis.Value.OriginalReceiptSha256, diagnosis.Value.Categories, witness.Value!.TeachingIdentities);
        Assert.True(input.IsSuccess, input.ErrorMessage);
        var admission = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission.CaptureSyntheticForTesting(
            witness.Value.TeachingIdentities, input.Value!, binding ?? witness.Value.BindingSha256, itemId,
            CodeDocumentationRepositoryOwnedDpoMaterialPurpose.ApprovedTeachingReference,
            new TrainingDataGovernanceDecision(true, false, new[] { "sha256:" + H("synthetic-material-policy") }));
        Assert.True(admission.IsSuccess, admission.ErrorMessage); return admission.Value!;
    }
    internal static async Task<CodeDocumentationOneLessonSession.SelectionBindingReadback> BindSelectedMaterial(
        OrdinaryDiagnosticHarness h, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission admission, string? evidence = null, bool requireNonFirst = true)
    {
        var witness = h.Inspect(baseline); Assert.True(witness.IsSuccess, witness.ErrorMessage); var calls = 0;
        var item = admission.Item;
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((_, _) =>
        {
            calls++;
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(new(
                item.ItemId, item.SourceId, item.ReferenceEntryId, item.SourceEntityFingerprint,
                "Synthetic semantic choice of the explicitly supplied non-first teaching task.", evidence ?? admission.EvidenceSha256)));
        });
        var bound = await BindSelection(h, baseline, witness.Value!, provider); Assert.True(bound.IsSuccess, bound.ErrorMessage);
        Assert.Equal(1, calls); if (requireNonFirst) Assert.NotEqual(witness.Value!.TeachingIdentities.Items[0].DeclaredItem.ItemId, bound.Value!.Payload.ItemId);
        return bound.Value!;
    }
    internal static OperationResult<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext> ConsumeSelectedMaterial(
        OrdinaryDiagnosticHarness h, CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline,
        CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialAdmission? admission, CancellationToken token = default)
        => h.Open.Session.PrepareOrdinarySelectedTeachingMaterialContextSyntheticForTesting(baseline, h.Catalog, h.Fixture.OrdinarySelector, admission, token);

    [Fact]
    public async Task OrdinarySelectedMaterial_MatchingSyntheticRefusesOrdinaryRecipientBeforeCapture()
    {
        using var h = TeacherContextHarness(); var (b, admission) = await BindTeacherCompleted(h);
        var saved = h.Open.Session.TryReadOrdinarySelectionBinding(b, h.Catalog, h.Fixture.OrdinarySelector);
        Assert.True(saved.IsSuccess, saved.ErrorCode); Assert.Equal(admission.Item.ItemId, saved.Value!.Payload.ItemId);
        var witness = h.Inspect(b); Assert.True(witness.IsSuccess, witness.ErrorCode);
        Assert.NotEqual(witness.Value!.TeachingIdentities.Items[0].DeclaredItem.ItemId, admission.Item.ItemId);
        var p = saved.Value.Payload;
        var input = CodeDocumentationOneLessonV2Wire.CaptureTeachingSelectionInput(p.OriginalReceiptId, p.OriginalReceiptSha256, p.Categories, witness.Value.TeachingIdentities);
        Assert.True(input.IsSuccess, input.ErrorCode); Assert.True(admission.Matches(saved.Value.BindingSha256, input.Value!, p).IsSuccess);
        var control = await ConsumeSelectedStudent(h, b, admission); Assert.True(control.IsSuccess, control.ErrorCode);
        var request = JsonSerializer.SerializeToUtf8Bytes(control.Value!.Request); var response = control.Value.CopyStudentResponseUtf8();
        var files = h.Files(); var history = h.HistoryBytes(); var effects = h.Open.Session.Snapshot.Effects;
        var accounting = h.Open.Session.SelectedStudentAccounting; var calls = h.Client.Calls;
        var captures = 0; var finalChecks = 0;
        var observer = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting;
        var hook = h.Open.Session.SelectionDurablePointForTesting;
        var origin = admission.GetType().GetField("<Origin>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = _ => captures++;
            h.Open.Session.SelectionDurablePointForTesting = stage => { if (stage == "material-final-owner-checked") finalChecks++; };
            var refused = h.Open.Session.PrepareOrdinarySelectedTeachingMaterialContext(b, h.Catalog, h.Fixture.OrdinarySelector, admission);
            Assert.False(refused.IsSuccess); Assert.Null(refused.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, refused.ErrorCode);
            Assert.Equal(0, captures); Assert.Equal(1, finalChecks);
            origin.SetValue(admission, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialOrigin.Unknown);
            Assert.True(admission.Matches(saved.Value.BindingSha256, input.Value!, p).IsSuccess, "Origin changes no evidence or selection identity.");
            var unknown = ConsumeSelectedMaterial(h, b, admission);
            Assert.False(unknown.IsSuccess); Assert.Null(unknown.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, unknown.ErrorCode);
            Assert.Equal(0, captures); Assert.Equal(2, finalChecks);
            origin.SetValue(admission, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialOrigin.SyntheticTest);
            using var canceled = new CancellationTokenSource(); canceled.Cancel();
            var cancellation = Assert.Throws<OperationCanceledException>(() => h.Open.Session.PrepareOrdinarySelectedTeachingMaterialContext(b, h.Catalog, h.Fixture.OrdinarySelector, admission, canceled.Token));
            Assert.Equal(canceled.Token, cancellation.CancellationToken); Assert.Equal(0, captures); Assert.Equal(3, finalChecks);
            Assert.Equal(history, h.HistoryBytes()); Assert.Equal(calls, h.Client.Calls);
            Assert.Equal(effects, h.Open.Session.Snapshot.Effects); Assert.Equal(accounting, h.Open.Session.SelectedStudentAccounting); h.AssertFiles(files);
        }
        finally
        {
            origin.SetValue(admission, CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedTeachingMaterialOrigin.SyntheticTest);
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = observer;
            h.Open.Session.SelectionDurablePointForTesting = hook;
        }
        var restored = await ConsumeSelectedStudent(h, b, admission); Assert.True(restored.IsSuccess, restored.ErrorCode);
        Assert.Equal(request, JsonSerializer.SerializeToUtf8Bytes(restored.Value!.Request)); Assert.Equal(response, restored.Value.CopyStudentResponseUtf8());
        Assert.False(restored.Value.FullTeacherContextReady); Assert.Equal("SyntheticTest", restored.Value.Origin);
        Assert.Equal(history, h.HistoryBytes()); Assert.Equal(calls, h.Client.Calls); Assert.Equal(accounting, h.Open.Session.SelectedStudentAccounting); h.AssertFiles(files);
    }

    [Fact]
    public async Task OrdinarySelectedMaterial_GenuineAdmissionReachesAuthoringRecipient()
    {
        using var h = SelectedMaterialHarness(); var original = await h.Consume(); Assert.True(original.IsSuccess, original.ErrorMessage);
        var b = original.Value!; var admission = SelectedMaterialAdmission(h, b); var binding = await BindSelectedMaterial(h, b, admission);
        var before = h.Files(); var history = h.HistoryBytes(); var captures = new List<CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.PartialSelectedMaterialContext>();
        var prior = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = captures.Add;
            var consumed = ConsumeSelectedMaterial(h, b, admission); Assert.True(consumed.IsSuccess, consumed.ErrorMessage);
            Assert.Same(Assert.Single(captures), consumed.Value); Assert.Equal(binding.Payload.Reason, consumed.Value!.SelectionReason);
            Assert.Equal("Полная задача 中文\nsecond line\toriginal-exclusion-7-0", consumed.Value.OwnerInput);
            Assert.Equal("Полный reference 中文\nfinal line\toriginal-exclusion-7-0", consumed.Value.PaidModelReferenceAnswer);
            Assert.Equal(Encoding.UTF8.GetBytes(consumed.Value.OwnerInput), consumed.Value.CopyOwnerInputUtf8());
            Assert.Equal(Encoding.UTF8.GetBytes(consumed.Value.PaidModelReferenceAnswer), consumed.Value.CopyPaidReferenceUtf8());
            Assert.Equal(admission.EvidenceSha256, consumed.Value.MaterialAdmissionEvidenceSha256);
            Assert.Equal(admission.SourceSha256, consumed.Value.SourceSha256); Assert.Equal(admission.BindingSha256, consumed.Value.BindingSha256);
            Assert.Equal(admission.FrozenContextSha256, consumed.Value.FrozenContextSha256); Assert.Equal(admission.Item, consumed.Value.Item);
            Assert.Equal(admission.Entry.PaidReferenceProvenance, consumed.Value.PaidReferenceProvenance); Assert.Equal("SyntheticTest", consumed.Value.Origin);
        }
        finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = prior; }
        h.AssertFiles(before); Assert.Equal(history, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls);
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), h.Open.Session.Snapshot.Effects);
    }

    [Fact]
    public async Task OrdinarySelectedMaterial_MissingForgedStaleAndForeignAdmissionReject()
    {
        using var h = SelectedMaterialHarness(); var b = (await h.Consume()).Value!;
        var admission = SelectedMaterialAdmission(h, b); await BindSelectedMaterial(h, b, admission);
        Assert.True(ConsumeSelectedMaterial(h, b, admission).IsSuccess, "valid opaque admission control");
        using var foreign = SelectedMaterialHarness(); var foreignBaseline = (await foreign.Consume()).Value!;
        var foreignAdmission = SelectedMaterialAdmission(foreign, foreignBaseline);
        var wrongBinding = SelectedMaterialAdmission(h, b, H("wrong-whole-session"));
        var wrongItem = SelectedMaterialAdmission(h, b, itemId: "item-teach-2-1");
        var before = h.Files(); var recipientCalls = 0; var prior = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting;
        try
        {
            CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = _ => recipientCalls++;
            foreach (var candidate in new[] { null, wrongBinding, wrongItem, foreignAdmission })
                Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, ConsumeSelectedMaterial(h, b, candidate).ErrorCode);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, ConsumeSelectedMaterial(h, foreignBaseline, admission).ErrorCode);
            Assert.Equal(0, recipientCalls); h.AssertFiles(before);
            var evidence = b.Context.EvidenceRefs;
            try { SetDiagnosticBacking(b.Context, "EvidenceRefs", new[] { "stale-whole-context" }); Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, ConsumeSelectedMaterial(h, b, admission).ErrorCode); }
            finally { SetDiagnosticBacking(b.Context, "EvidenceRefs", evidence); }
            Assert.Equal(0, recipientCalls); h.AssertFiles(before); Assert.Equal(10, h.Client.Calls);
            using var digestOnly = SelectedMaterialHarness(); var db = (await digestOnly.Consume()).Value!; var da = SelectedMaterialAdmission(digestOnly, db);
            await BindSelectedMaterial(digestOnly, db, da, H("caller-descriptive-evidence"));
            Assert.Equal(CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialAdmissionInvalidErrorCode, ConsumeSelectedMaterial(digestOnly, db, da).ErrorCode);
            Assert.Equal(0, recipientCalls); Assert.Equal(10, digestOnly.Client.Calls);
            h.Open.Dispose(); Assert.Equal(CodeDocumentationOneLessonSession.CapacityInvalidErrorCode, ConsumeSelectedMaterial(h, b, admission).ErrorCode);
        }
        finally { CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = prior; }
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), h.Open.Session.Snapshot.Effects);
    }

    [Fact]
    public async Task OrdinarySelectedMaterial_CancellationAndLateOwnerChangeFinishChecks()
    {
        foreach (var fault in new[] { "cancel", "throw", "original", "unused-source", "receiving" })
        {
            using var h = SelectedMaterialHarness(); var b = (await h.Consume()).Value!; var admission = SelectedMaterialAdmission(h, b);
            await BindSelectedMaterial(h, b, admission); Assert.True(ConsumeSelectedMaterial(h, b, admission).IsSuccess, fault + " valid recipient control");
            var before = h.Files(); var history = h.HistoryBytes(); using var cancellation = new CancellationTokenSource(); var finalChecks = 0; var recipientCalls = 0; var writtenFaults = 0;
            var observer = CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting; var hook = h.Open.Session.SelectionDurablePointForTesting;
            var path = fault == "original" ? Path.Combine(h.Fixture.CorpusAddress.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName)
                : TeachingPayload(h.Grant.SourceSelection!.TeachingCurriculum!.Sources[1]);
            var bytes = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path); var primary = new InvalidOperationException("recipient-primary-failure");
            var evidence = b.Context.EvidenceRefs; Exception? primaryFailure = null;
            try
            {
                h.Open.Session.SelectionDurablePointForTesting = stage => { if (stage == "material-final-owner-checked") finalChecks++; };
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = _ =>
                {
                    recipientCalls++;
                    if (fault == "cancel") cancellation.Cancel();
                    else if (fault == "throw") throw primary;
                    else if (fault == "receiving") SetDiagnosticBacking(b.Context, "EvidenceRefs", new[] { "late-receiving-context" });
                    else { RestoreReferenceOwnerFile(path, bytes.Concat(new byte[] { (byte)' ' }).ToArray(), ticks); writtenFaults++; }
                };
                if (fault == "cancel") Assert.ThrowsAny<OperationCanceledException>(() => ConsumeSelectedMaterial(h, b, admission, cancellation.Token));
                else if (fault == "throw") Assert.Same(primary, Assert.Throws<InvalidOperationException>(() => ConsumeSelectedMaterial(h, b, admission)));
                else
                {
                    var refused = ConsumeSelectedMaterial(h, b, admission);
                    Assert.Equal(fault == "receiving" ? CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode : RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, refused.ErrorCode);
                    Assert.False(refused.IsSuccess, fault + " must fail late genuine owner check");
                    output.WriteLine("MATERIAL_LATE_OWNER_FAULT fault=" + fault + " code=" + refused.ErrorCode + " writtenFaults=" + writtenFaults);
                }
                Assert.Equal(fault is "original" or "unused-source" ? 1 : 0, writtenFaults);
                Assert.Equal(1, recipientCalls); Assert.Equal(1, finalChecks); Assert.Equal(10, h.Client.Calls);
            }
            catch (Exception failure) { primaryFailure = failure; throw; }
            finally
            {
                CodeDocumentationRepositoryOwnedDpoDatasetAuthoring.SelectedMaterialCapturedForTesting = observer; h.Open.Session.SelectionDurablePointForTesting = hook;
                if (fault == "receiving") SetDiagnosticBacking(b.Context, "EvidenceRefs", evidence);
                if (fault is "original" or "unused-source") RestoreFixtureAfterFault(() => RestoreReferenceOwnerFile(path, bytes, ticks), primaryFailure);
            }
            h.AssertFiles(before); Assert.True(ConsumeSelectedMaterial(h, b, admission).IsSuccess, fault + " restored owner control");
            Assert.Equal(history, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls);
            Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), h.Open.Session.Snapshot.Effects);
            string TeachingPayload(CodeDocumentationOneLessonTeachingSourceV2 source)
            {
                var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, source.DomainSlug, source.SubjectSlug, source.CorpusVersion).Value!;
                return Path.Combine(address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
            }
        }
    }

    [Fact]
    public async Task OrdinarySelectedMaterial_ReopenReusesChoiceAndRevalidatesBytes()
    {
        using var h = SelectedMaterialHarness(); var b = (await h.Consume()).Value!; var admission = SelectedMaterialAdmission(h, b);
        await BindSelectedMaterial(h, b, admission); var first = ConsumeSelectedMaterial(h, b, admission); Assert.True(first.IsSuccess, first.ErrorMessage);
        var before = h.Files(); var history = h.HistoryBytes(); Assert.True(h.Reopen().IsSuccess);
        Assert.True((await h.Open.Session.InspectExistingOrdinarySelectionOriginalAsync(h.Catalog, h.Fixture.OrdinarySelector, h.Fixture.RepositoryRoot, default)).IsSuccess);
        var replay = await h.Consume(); Assert.True(replay.IsSuccess, replay.ErrorMessage); var actual = ConsumeSelectedMaterial(h, replay.Value!, admission);
        Assert.True(actual.IsSuccess, actual.ErrorMessage); Assert.Equal(first.Value!.CopyOwnerInputUtf8(), actual.Value!.CopyOwnerInputUtf8());
        Assert.Equal(first.Value.CopyPaidReferenceUtf8(), actual.Value.CopyPaidReferenceUtf8()); Assert.Equal(history, h.HistoryBytes()); h.AssertFiles(before);
        foreach (var path in new[] { h.Fixture.OrdinaryBaseline, h.Fixture.OrdinaryClaim })
        {
            var bytes = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path); Exception? primaryFailure = null;
            try
            {
                RestoreCustodyFile(path, bytes.Concat(new byte[] { (byte)' ' }).ToArray(), ticks);
                Assert.False((await h.Open.Session.InspectExistingOrdinarySelectionOriginalAsync(h.Catalog, h.Fixture.OrdinarySelector, h.Fixture.RepositoryRoot, default)).IsSuccess, path + " pre-factory replay refusal");
                Assert.Equal(history, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls);
            }
            catch (Exception failure) { primaryFailure = failure; throw; }
            finally { RestoreFixtureAfterFault(() => RestoreCustodyFile(path, bytes, ticks), primaryFailure); }
        }
        var source = h.Grant.SourceSelection!.TeachingCurriculum!.Sources[1];
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, source.DomainSlug, source.SubjectSlug, source.CorpusVersion).Value!;
        var materialPath = Path.Combine(address.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        var materialBytes = File.ReadAllBytes(materialPath); var materialTicks = File.GetLastWriteTimeUtc(materialPath); Exception? materialPrimaryFailure = null;
        try
        {
            RestoreReferenceOwnerFile(materialPath, materialBytes.Concat(new byte[] { (byte)' ' }).ToArray(), materialTicks);
            var refused = ConsumeSelectedMaterial(h, replay.Value!, admission);
            Assert.False(refused.IsSuccess, "changed unused material on fresh-owner replay");
            Assert.Equal(RepositoryOwnedReferenceCorpusManifestSealer.InvalidErrorCode, refused.ErrorCode);
            output.WriteLine("MATERIAL_REPLAY_UNUSED_SOURCE_FAULT code=" + refused.ErrorCode + " actualBytesChanged=True");
            Assert.Equal(10, h.Client.Calls); Assert.Equal(history, h.HistoryBytes());
        }
        catch (Exception failure) { materialPrimaryFailure = failure; throw; }
        finally { RestoreFixtureAfterFault(() => RestoreReferenceOwnerFile(materialPath, materialBytes, materialTicks), materialPrimaryFailure); }
        h.AssertFiles(before); Assert.Equal(history, h.HistoryBytes()); Assert.True(ConsumeSelectedMaterial(h, replay.Value!, admission).IsSuccess);
        Assert.Equal(10, h.Client.Calls);
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), h.Open.Session.Snapshot.Effects);
    }
    private static Task<OperationResult<CodeDocumentationOneLessonSession.SelectionBindingReadback>> BindSelection(
        OrdinaryDiagnosticHarness h,CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline baseline,
        CodeDocumentationOneLessonSession.OrdinaryOriginalTeachingExclusion witness,
        CodeDocumentationOneLessonSelectionDecisionProvider provider,CancellationToken token=default)
        => h.Open.Session.BindOrdinarySelectionDecisionSyntheticForTestingAsync(baseline,h.Catalog,h.Fixture.OrdinarySelector,witness,provider,token);

    internal static void AssertSyntheticSelectionHistory(byte[] history, string itemId)
    {
        var lines = Encoding.UTF8.GetString(history).Split('\n'); Assert.Equal(3, lines.Length); Assert.Equal("", lines[2]);
        using var frame = JsonDocument.Parse(lines[1]); var payload = frame.RootElement.GetProperty("TeachingSelectionBinding");
        Assert.Equal("SyntheticTest", payload.GetProperty("Origin").GetString()); Assert.Equal(itemId, payload.GetProperty("ItemId").GetString());
    }

    [Fact]
    public async Task OrdinarySelectionBinding_FreshSyntheticRefusesBeforeAttemptAndPreservesFixtureChoice()
    {
        using var h = SelectedMaterialHarness(); var consumed = await h.Consume(); Assert.True(consumed.IsSuccess, consumed.ErrorMessage);
        Assert.Equal(10, h.Client.Calls); h.Open.Dispose();
        var files = h.Fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        Assert.True(h.Reopen().IsSuccess); var original = await h.Consume(); Assert.True(original.IsSuccess, original.ErrorMessage);
        var witness = h.Inspect(original.Value!); Assert.True(witness.IsSuccess, witness.ErrorMessage);
        var admission = SelectedMaterialAdmission(h, original.Value!); var item = admission.Item; var calls = 0;
        Assert.NotEqual(witness.Value!.TeachingIdentities.Items[0].DeclaredItem.ItemId, item.ItemId);
        var provider = CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input, _) =>
        {
            calls++; Assert.Equal(1, h.Open.Session.ReadCapacityPrefixV2().Value!.PrefixFrames);
            Assert.True(admission.MatchesSelectionInput(witness.Value.BindingSha256, input, item).IsSuccess);
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(new(
                item.ItemId, item.SourceId, item.ReferenceEntryId, item.SourceEntityFingerprint, "Synthetic explicit second eligible item.", admission.EvidenceSha256)));
        });
        var refused = await h.Open.Session.BindOrdinarySelectionDecisionAsync(original.Value!, h.Catalog, h.Fixture.OrdinarySelector, witness.Value!, provider);
        Assert.False(refused.IsSuccess); Assert.Equal("ONE_LESSON_SELECTION_BINDING_INVALID", refused.ErrorCode);
        Assert.Equal(0, calls); Assert.False(h.Open.Session.HasSavedSelectionBinding); Assert.Equal(0, h.Open.Session.ReadCapacityPrefixV2().Value!.PrefixFrames);
        Assert.Empty(h.HistoryBytes()); Assert.Equal(10, h.Client.Calls); Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), h.Open.Session.Snapshot.Effects);
        h.Open.Dispose(); var after = h.Fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p, p => (Bytes: File.ReadAllBytes(p), Ticks: File.GetLastWriteTimeUtc(p).Ticks));
        Assert.Equal(files.Keys.Order(), after.Keys.Order()); foreach (var p in files.Keys) { Assert.Equal(files[p].Bytes, after[p].Bytes); Assert.Equal(files[p].Ticks, after[p].Ticks); }
        Assert.True(h.Reopen().IsSuccess); original = await h.Consume(); Assert.True(original.IsSuccess, original.ErrorMessage);
        witness = h.Inspect(original.Value!); Assert.True(witness.IsSuccess, witness.ErrorMessage);
        // Repeat the ordinary refusal, then restore through the explicit fixture entry on this exact held owner: refusal cannot poison its attempt.
        refused = await h.Open.Session.BindOrdinarySelectionDecisionAsync(original.Value!, h.Catalog, h.Fixture.OrdinarySelector, witness.Value!, provider);
        Assert.Equal("ONE_LESSON_SELECTION_BINDING_INVALID", refused.ErrorCode); Assert.Equal(0, calls); Assert.Empty(h.HistoryBytes());
        var bound = await BindSelection(h, original.Value!, witness.Value!, provider); Assert.True(bound.IsSuccess, bound.ErrorMessage);
        Assert.Equal(1, calls); Assert.Equal(item.ItemId, bound.Value!.Payload.ItemId); AssertSyntheticSelectionHistory(h.HistoryBytes(), item.ItemId);
        var history = h.HistoryBytes(); Assert.Equal(2, history.Count(value => value == (byte)'\n'));
        Assert.True(h.Reopen().IsSuccess); var replay = await h.Consume(); Assert.True(replay.IsSuccess, replay.ErrorMessage);
        var read = await h.Open.Session.BindOrdinarySelectionDecisionAsync(replay.Value!, h.Catalog, h.Fixture.OrdinarySelector, witness.Value!, provider);
        Assert.True(read.IsSuccess, read.ErrorMessage); Assert.Equal(bound.Value.Payload.ItemId, read.Value!.Payload.ItemId);
        Assert.Equal(bound.Value.Payload.Reason, read.Value.Payload.Reason); Assert.Equal(bound.Value.Payload.FrozenContextSha256, read.Value.Payload.FrozenContextSha256);
        Assert.Equal(1, calls); Assert.Equal(10, h.Client.Calls); Assert.Equal(history, h.HistoryBytes());
    }

    [Fact]
    public async Task OrdinarySelectionBinding_GenuineOwnerWritesReadsAndReusesChoice()
    {
        using var h=SelectionHarness(); var original=await h.Consume(); Assert.True(original.IsSuccess,original.ErrorMessage);
        var witness=h.Inspect(original.Value!).Value!; var choice=SelectionChoice(witness); var calls=0;
        var provider=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
        {
            calls++; var bytes=CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionInputBytes(input);
            using var json=JsonDocument.Parse(bytes);
            Assert.Equal(witness.TeachingIdentities.Sources.Count,json.RootElement.GetProperty("TeachingGraph").GetProperty("Sources").GetArrayLength());
            Assert.Equal(1,h.Open.Session.ReadCapacityPrefixV2().Value!.PrefixFrames);
            Assert.EndsWith("\n",Encoding.UTF8.GetString(h.HistoryBytes()),StringComparison.Ordinal);
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(choice));
        });
        var bound=await BindSelection(h,original.Value!,witness,provider); Assert.True(bound.IsSuccess,bound.ErrorMessage);
        Assert.True(bound.Value!.IsIssuedBy(h.Open.Session)); Assert.Equal(choice.Reason,bound.Value.Payload.Reason);
        Assert.Equal(choice.ItemId,bound.Value.Payload.ItemId); Assert.Equal(h.Classify(original.Value!).Value!.Categories,bound.Value.Payload.Categories);
        var bytes=h.HistoryBytes(); Assert.Equal(2,Encoding.UTF8.GetString(bytes).Split('\n').Length-1);
        using(var frame=JsonDocument.Parse(Encoding.UTF8.GetString(bytes).Split('\n')[1]))
        {Assert.Equal(JsonValueKind.Number,frame.RootElement.GetProperty("Revision").ValueKind); Assert.Equal(1,frame.RootElement.GetProperty("Revision").GetInt32());}
        Assert.True((await BindSelection(h,original.Value!,witness,provider)).IsSuccess); Assert.Equal(1,calls);
        Assert.True(h.Reopen().IsSuccess); Assert.False(bound.Value.IsIssuedBy(h.Open.Session));
        Assert.True((await h.Open.Session.InspectExistingOrdinarySelectionOriginalAsync(h.Catalog,h.Fixture.OrdinarySelector,h.Fixture.RepositoryRoot,default)).IsSuccess);
        var replay=await h.Consume(); Assert.True(replay.IsSuccess,replay.ErrorMessage);
        var read=h.Open.Session.TryReadOrdinarySelectionBinding(replay.Value!,h.Catalog,h.Fixture.OrdinarySelector);
        Assert.True(read.IsSuccess,read.ErrorMessage); Assert.Equal(bound.Value.Payload.Reason,read.Value!.Payload.Reason);
        Assert.Equal(bytes,h.HistoryBytes()); Assert.Equal(1,calls); Assert.Equal(10,h.Client.Calls);
        Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),h.Open.Session.Snapshot.Effects);
    }

    [Fact]
    public async Task OrdinarySelectionBinding_ForeignDisposedOrMutatedOwnerRejectsBeforeAppend()
    {
        using var h=SelectionHarness(); using var other=SelectionHarness();
        var b=(await h.Consume()).Value!; var foreign=(await other.Consume()).Value!;
        var witness=h.Inspect(b).Value!; var otherWitness=other.Inspect(foreign).Value!; var calls=0;
        var provider=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
        {calls++; return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionChoice(witness)));});
        var files=h.Files(); Assert.False((await BindSelection(h,foreign,witness,provider)).IsSuccess);
        Assert.False((await BindSelection(h,b,otherWitness,provider)).IsSuccess); h.AssertFiles(files); Assert.Equal(0,calls);
        var evidence=b.Context.EvidenceRefs;
        try {SetDiagnosticBacking(b.Context,"EvidenceRefs",new[]{"changed-receiving-context"}); Assert.False((await BindSelection(h,b,witness,provider)).IsSuccess); h.AssertFiles(files);}
        finally {SetDiagnosticBacking(b.Context,"EvidenceRefs",evidence);}
        h.Open.Dispose(); Assert.False((await BindSelection(h,b,witness,provider)).IsSuccess); Assert.Equal(0,calls);
        using var changed=SelectionHarness(); var cb=(await changed.Consume()).Value!; var cw=changed.Inspect(cb).Value!;
        var changedProvider=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
        {
            var privateBytes=(byte[])typeof(CodeDocumentationTeachingSelectionInputV1).GetField("bytes",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(input)!;
            privateBytes[0]^=1;
            return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionChoice(cw)));
        });
        Assert.False((await BindSelection(changed,cb,cw,changedProvider)).IsSuccess);
        Assert.Equal(1,Encoding.UTF8.GetString(changed.HistoryBytes()).Split('\n').Length-1);
        Assert.False(changed.Reopen().IsSuccess); Assert.Equal(10,changed.Client.Calls);
    }

    [Fact]
    public async Task OrdinarySelectionBinding_InvalidChoiceAndReasonRejectBeforeEffects()
    {
        foreach(var fault in new[]{"item","source","entry","fingerprint","blank","surrogate","over-limit","closed-reason"})
        {
            using var h=SelectionHarness(); var b=(await h.Consume()).Value!; var w=h.Inspect(b).Value!;
            var c=SelectionChoice(w); c=fault switch {
                "item"=>c with {ItemId="unknown-item"},"source"=>c with {SourceId="unknown-source"},
                "entry"=>c with {ReferenceEntryId="sha256:"+H("foreign-entry")},
                "fingerprint"=>c with {SourceEntityFingerprint="sha256:"+H("foreign-fingerprint")},
                "blank"=>c with {Reason=" "},"surrogate"=>c with {Reason="\uD800"},
                "over-limit"=>c with {Reason=new string('x',4097)},
                "closed-reason"=>c with {Reason=h.Context.Corpus.Entries[0].PaidModelReferenceAnswer},_=>c};
            var calls=0; var provider=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
            {calls++;return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(c));});
            var result=await BindSelection(h,b,w,provider); Assert.False(result.IsSuccess,fault); Assert.Null(result.Value);
            Assert.Equal(1,calls); Assert.Equal(1,Encoding.UTF8.GetString(h.HistoryBytes()).Split('\n').Length-1);
            Assert.False((await BindSelection(h,b,w,provider)).IsSuccess); Assert.Equal(1,calls);
            Assert.False(h.Reopen().IsSuccess); Assert.Equal(10,h.Client.Calls);
            Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),h.Open.Session.Snapshot.Effects);
        }
    }

    [Fact]
    public async Task OrdinarySelectionBinding_TornUnknownReorderedAndChangedPrefixFailsClosed()
    {
        using var h=SelectionHarness(); var b=(await h.Consume()).Value!; var w=h.Inspect(b).Value!;
        var providerCalls=0;
        var provider=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
        {providerCalls++;return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionChoice(w)));});
        var positive=await BindSelection(h,b,w,provider); Assert.True(positive.IsSuccess,positive.ErrorMessage);
        var p=positive.Value!.Payload; var initialBinding=h.Open.Session.Snapshot.Binding; var sessionId=h.Open.Session.Snapshot.SessionId;
        var initialProjection=new CodeDocumentationOneLessonBindingProjectionV2(initialBinding.ObjectiveId,initialBinding.GrantId,initialBinding.GrantVersion,initialBinding.TrainingRunId,
            initialBinding.CandidateSha256,initialBinding.ParentManifestSha256,initialBinding.ParentWeightsSha256,initialBinding.ConfigurationSha256,initialBinding.ExamConditionSha256,initialBinding.DescriptorsV2!);
        var good=h.HistoryBytes(); h.Open.Dispose();
        var lines=Encoding.UTF8.GetString(good).Split('\n');
        Assert.True(CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(Encoding.UTF8.GetBytes(lines[1]+"\n"),initialProjection).IsSuccess,"Valid bound frame must reach the canonical decoder.");
        var faults=new List<(string Name,byte[] Bytes)> {("torn-lf",good[..^1]),("duplicate-frame",good.Concat(Encoding.UTF8.GetBytes(lines[1]+"\n")).ToArray()),("leading-space",Encoding.UTF8.GetBytes(lines[0]+"\n "+lines[1]+"\n"))};
        foreach(var property in new[]{"Unknown","Revision","PreviousFrameSha256","SourceSha256"})
        {
            var node=JsonNode.Parse(lines[1])!.AsObject();
            if(property=="Unknown") node[property]="x";
            else if(property=="Revision") node[property]="1";
            else if(property=="PreviousFrameSha256") node[property]=H("wrong-genesis");
            else node["TeachingSelectionBinding"]![property]=H("foreign-"+property);
            // Raw caller JSON is deliberately untrusted; no default array-writer expansion is used.
            faults.Add((property,Encoding.UTF8.GetBytes(lines[0]+"\n"+node.ToJsonString()+"\n")));
        }
        faults.Add(("duplicate-kind",Encoding.UTF8.GetBytes(lines[0]+"\n"+lines[1].Replace("\"Kind\":", "\"Kind\":\"TeachingSelectionBinding\",\"Kind\":",StringComparison.Ordinal)+"\n")));
        foreach(var fault in faults)
        {
            File.WriteAllBytes(h.Fixture.OrdinaryHistory,fault.Bytes); var rejected=h.Reopen();
            Assert.False(rejected.IsSuccess,"Open must reject prefix fault "+fault.Name+"; actual="+rejected.ErrorCode);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,rejected.ErrorCode);
            Assert.Equal(fault.Bytes,h.HistoryBytes()); Assert.Equal(10,h.Client.Calls); Assert.Equal(1,providerCalls);
        }
        foreach(var fault in new[]{"OriginalReceiptSha256","Categories"})
        {
            // These frames are structurally canonical; the receiving ORIGINAL owner supplies their semantic check.
            var captured=CodeDocumentationTeachingSelectionPayloadV1.Capture(p.OriginalReceiptId,
                fault=="OriginalReceiptSha256"?H("foreign-OriginalReceiptSha256"):p.OriginalReceiptSha256,p.SourceSha256,
                fault=="Categories"?new[]{"invented_behavior"}:p.Categories,p.ItemId,p.SourceId,p.ReferenceEntryId,p.SourceEntityFingerprint,
                p.Reason,p.FrozenContextSha256,p.MaterialAdmissionEvidenceSha256,p.InvocationId,p.LaunchSettingsSha256,p.LaunchAckSha256);
            Assert.True(captured.IsSuccess,"Typed semantic fault must remain structurally valid: "+fault);
            var semanticFrame=CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionFrameBytes(initialProjection,sessionId,
                CodeDocumentationOneLessonSession.ComputeBindingSha256(initialBinding),H(lines[0]),captured.Value!);
            Assert.True(CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(semanticFrame,initialProjection).IsSuccess,"Canonical semantic frame decoder: "+fault);
            var raw=Encoding.UTF8.GetBytes(lines[0]+"\n").Concat(semanticFrame).ToArray();
            File.WriteAllBytes(h.Fixture.OrdinaryHistory,raw); var opened=h.Reopen(); Assert.True(opened.IsSuccess,"Structural Open control for "+fault+": "+opened.ErrorCode);
            var originalCheck=await h.Open.Session.InspectExistingOrdinarySelectionOriginalAsync(h.Catalog,h.Fixture.OrdinarySelector,h.Fixture.RepositoryRoot,default);
            var consumed=await h.Consume();
            if(fault=="OriginalReceiptSha256")
            {
                Assert.False(originalCheck.IsSuccess,"Changed receipt must hit the existing-only ORIGINAL guard before evaluation.");
                Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode,originalCheck.ErrorCode);
                Assert.False(consumed.IsSuccess,"Changed receipt must never reattach or recalibrate ORIGINAL.");
                Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode,consumed.ErrorCode);
            }
            else
            {
                Assert.True(originalCheck.IsSuccess,originalCheck.ErrorMessage); Assert.True(consumed.IsSuccess,consumed.ErrorMessage);
                var rejected=h.Open.Session.TryReadOrdinarySelectionBinding(consumed.Value!,h.Catalog,h.Fixture.OrdinarySelector);
                Assert.False(rejected.IsSuccess,"Changed categories must hit the receiving diagnostic-order guard.");
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode,rejected.ErrorCode);
            }
            Assert.Equal(raw,h.HistoryBytes()); Assert.Equal(10,h.Client.Calls); Assert.Equal(1,providerCalls);
            Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),h.Open.Session.Snapshot.Effects); h.Open.Dispose();
        }
        File.WriteAllBytes(h.Fixture.OrdinaryHistory,good); Assert.True(h.Reopen().IsSuccess);
        var replay=(await h.Consume()).Value!; Assert.True(h.Open.Session.TryReadOrdinarySelectionBinding(replay,h.Catalog,h.Fixture.OrdinarySelector).IsSuccess);
        Assert.Equal(10,h.Client.Calls);
        using var ordered=new OrdinaryDiagnosticHarness((i,_) => (false,i%2==0?"missing_essential_behavior":"invented_behavior"),SelectionCurriculum);
        var ob=(await ordered.Consume()).Value!; var ow=ordered.Inspect(ob).Value!;
        var op=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
            Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionChoice(ow))));
        var saved=await BindSelection(ordered,ob,ow,op); Assert.True(saved.IsSuccess,saved.ErrorMessage);
        var payload=saved.Value!.Payload; Assert.Equal(2,payload.Categories.Count);
        var changed=CodeDocumentationTeachingSelectionPayloadV1.Capture(payload.OriginalReceiptId,payload.OriginalReceiptSha256,payload.SourceSha256,
            payload.Categories.Reverse().ToArray(),payload.ItemId,payload.SourceId,payload.ReferenceEntryId,payload.SourceEntityFingerprint,
            payload.Reason,payload.FrozenContextSha256,payload.MaterialAdmissionEvidenceSha256,payload.InvocationId,payload.LaunchSettingsSha256,payload.LaunchAckSha256).Value!;
        var originalBytes=ordered.HistoryBytes(); var genesis=Encoding.UTF8.GetString(originalBytes).Split('\n')[0];
        var binding=ordered.Open.Session.Snapshot.Binding;
        var projection=new CodeDocumentationOneLessonBindingProjectionV2(binding.ObjectiveId,binding.GrantId,binding.GrantVersion,binding.TrainingRunId,
            binding.CandidateSha256,binding.ParentManifestSha256,binding.ParentWeightsSha256,binding.ConfigurationSha256,binding.ExamConditionSha256,binding.DescriptorsV2!);
        var frame=CodeDocumentationOneLessonV2Wire.CanonicalTeachingSelectionFrameBytes(projection,ordered.Open.Session.Snapshot.SessionId,
            CodeDocumentationOneLessonSession.ComputeBindingSha256(binding),H(genesis),changed);
        Assert.True(CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(frame,projection).IsSuccess);
        ordered.Open.Dispose(); File.WriteAllBytes(ordered.Fixture.OrdinaryHistory,Encoding.UTF8.GetBytes(genesis+"\n").Concat(frame).ToArray());
        Assert.True(ordered.Reopen().IsSuccess); var odb=(await ordered.Consume()).Value!;
        Assert.False(ordered.Open.Session.TryReadOrdinarySelectionBinding(odb,ordered.Catalog,ordered.Fixture.OrdinarySelector).IsSuccess);
        Assert.Equal(10,ordered.Client.Calls);
    }

    [Fact]
    public async Task OrdinarySelectionBinding_ReplayMissingOrChangedOriginalNeverRecalibrates()
    {
        foreach(var fault in new[]{"baseline-missing","baseline-changed","claim-missing","claim-changed"})
        {
            using var h=SelectionHarness(); var b=(await h.Consume()).Value!; var w=h.Inspect(b).Value!; var calls=0;
            var provider=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
            {calls++;return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionChoice(w)));});
            Assert.True((await BindSelection(h,b,w,provider)).IsSuccess); var selection=h.HistoryBytes(); Assert.True(h.Reopen().IsSuccess);
            var path=fault.StartsWith("baseline",StringComparison.Ordinal)?h.Fixture.OrdinaryBaseline:h.Fixture.OrdinaryClaim;
            var bytes=File.ReadAllBytes(path); var ticks=File.GetLastWriteTimeUtc(path);
            try
            {
                if(fault.EndsWith("missing",StringComparison.Ordinal)) File.Delete(path);
                else File.WriteAllBytes(path,bytes.Concat(new byte[]{(byte)' '}).ToArray());
                Assert.False((await h.Open.Session.InspectExistingOrdinarySelectionOriginalAsync(h.Catalog,h.Fixture.OrdinarySelector,h.Fixture.RepositoryRoot,default)).IsSuccess);
                Assert.False((await h.Consume()).IsSuccess); Assert.Equal(10,h.Client.Calls); Assert.Equal(1,calls);
                Assert.Equal(selection,h.HistoryBytes());
                if(fault.EndsWith("missing",StringComparison.Ordinal)) Assert.False(File.Exists(path));
            }
            finally {RestoreCustodyFile(path,bytes,ticks);}
            var restoredCheck=await h.Open.Session.InspectExistingOrdinarySelectionOriginalAsync(h.Catalog,h.Fixture.OrdinarySelector,h.Fixture.RepositoryRoot,default);
            Assert.True(restoredCheck.IsSuccess,"Restored ORIGINAL guard for "+fault+": "+restoredCheck.ErrorMessage);
            var restored=await h.Consume(); Assert.True(restored.IsSuccess,"Restored ORIGINAL reattachment for "+fault+": "+restored.ErrorMessage);
            Assert.True(h.Open.Session.TryReadOrdinarySelectionBinding(restored.Value!,h.Catalog,h.Fixture.OrdinarySelector).IsSuccess);
            Assert.Equal(selection,h.HistoryBytes()); Assert.Equal(10,h.Client.Calls); Assert.Equal(1,calls);
        }
    }

    [Fact]
    public async Task OrdinarySelectionBinding_MaxCanonicalBytesAndCapacityAreCharged()
    {
        using var h=SelectionHarness(); var b=(await h.Consume()).Value!; var w=h.Inspect(b).Value!;
        var reason=new string('\0',4096); var provider=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
            Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionChoice(w,reason))));
        var result=await BindSelection(h,b,w,provider); Assert.True(result.IsSuccess,result.ErrorMessage);
        var raw=h.HistoryBytes(); var lines=Encoding.UTF8.GetString(raw).Split('\n');
        Assert.InRange(Encoding.UTF8.GetByteCount(lines[1])+1,24576,32768);
        var capacity=h.Open.Session.ReadCapacityPrefixV2().Value!; Assert.Equal(2,capacity.PrefixFrames); Assert.Equal(raw.LongLength,capacity.PrefixBytes);
        Assert.Equal(18+2*capacity.SlotCount,capacity.MaximumFrames); Assert.Equal(95249L+6548L*capacity.SlotCount,capacity.MaximumBytes);
        Assert.True(h.Open.Session.ValidateContinuationCapacityV2(2,raw.LongLength,capacity.MaximumFrames-2,capacity.MaximumBytes-raw.LongLength).IsSuccess);
        Assert.False(h.Open.Session.ValidateContinuationCapacityV2(2,raw.LongLength,capacity.MaximumFrames-1,0).IsSuccess);
        Assert.False(h.Open.Session.ValidateContinuationCapacityV2(2,raw.LongLength,0,capacity.MaximumBytes-raw.LongLength+1).IsSuccess);
        Assert.False(h.Open.Session.ValidateContinuationCapacityV2(int.MaxValue,0,1,0).IsSuccess);
        Assert.Equal(10,h.Client.Calls); Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(),h.Open.Session.Snapshot.Effects);
    }

    [Fact]
    public async Task OrdinarySelectionBinding_GenesisBeforeInvocationPreventsUnknownResultReplay()
    {
        foreach(var fault in new[]{"before-genesis","genesis-before-write","genesis-after-write","genesis-flushed","genesis-readback",
            "provider-exception","provider-cancel","metadata-cancel","decision-before-write","decision-after-write","decision-flushed","decision-readback"})
        {
            using var h=SelectionHarness(); var b=(await h.Consume()).Value!; var w=h.Inspect(b).Value!; var calls=0; var finalChecks=0; var metadataChecks=0;
            using var cancellation=new CancellationTokenSource(); var prior=h.Open.Session.SelectionDurablePointForTesting;
            try
            {
                h.Open.Session.SelectionDurablePointForTesting=stage=>
                {if(stage=="provider-final-owner-checked") finalChecks++; if(stage=="metadata-final-owner-checked") metadataChecks++;
                    if(fault=="metadata-cancel" && stage=="metadata-before-open") cancellation.Cancel();
                    if(stage==fault) throw new IOException("Synthetic durable point: "+fault);};
                var provider=CodeDocumentationOneLessonSelectionDecisionProvider.CreateSyntheticForTesting((input,token)=>
                {
                    calls++; Assert.Equal(1,h.Open.Session.ReadCapacityPrefixV2().Value!.PrefixFrames);
                    if(fault=="provider-exception") throw new IOException("synthetic provider failure");
                    if(fault=="provider-cancel") {cancellation.Cancel(); throw new OperationCanceledException(cancellation.Token);}
                    return Task.FromResult(OperationResult<CodeDocumentationOneLessonSelectionDecisionProvider.SyntheticChoice>.Ok(SelectionChoice(w)));
                });
                if(fault is "provider-cancel" or "metadata-cancel")
                {
                    var canceled=await Assert.ThrowsAsync<OperationCanceledException>(()=>BindSelection(h,b,w,provider,cancellation.Token));
                    Assert.True(cancellation.IsCancellationRequested,fault);
                    Assert.Equal(cancellation.Token,canceled.CancellationToken);
                }
                else Assert.False((await BindSelection(h,b,w,provider,cancellation.Token)).IsSuccess,fault);
                var expected=fault.StartsWith("genesis",StringComparison.Ordinal)||fault=="before-genesis"?0:1;
                Assert.Equal(expected,calls); if(expected==1) Assert.True(finalChecks==1,$"{fault}: provider final owner checks={finalChecks}");
                if(fault=="metadata-cancel") Assert.True(metadataChecks==1,$"{fault}: metadata final owner checks={metadataChecks}");
                var bytes=h.HistoryBytes(); Assert.False((await BindSelection(h,b,w,provider)).IsSuccess); Assert.Equal(expected,calls);
                h.Open.Session.SelectionDurablePointForTesting=prior;
                var reopened=h.Reopen();
                if(fault.StartsWith("decision-",StringComparison.Ordinal)&&fault!="decision-before-write")
                {Assert.True(reopened.IsSuccess,reopened.ErrorMessage); var replay=(await h.Consume()).Value!; Assert.True(h.Open.Session.TryReadOrdinarySelectionBinding(replay,h.Catalog,h.Fixture.OrdinarySelector).IsSuccess);}
                else if(bytes.Length>0) Assert.False(reopened.IsSuccess);
                Assert.Equal(bytes,h.HistoryBytes()); Assert.Equal(expected,calls); Assert.Equal(10,h.Client.Calls);
            }
            finally {h.Open.Session.SelectionDurablePointForTesting=prior;}
        }
    }

    // Reuse the existing synthetic ordinary material, without building a HTTP host.
    // Production Consume performs issuer/save/distinct receiver/mint under this held Session.
    internal sealed class OrdinaryDiagnosticHarness : IDisposable
    {
        internal CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture Fixture { get; }
        internal CodeDocumentationOneLessonGrantCatalog Catalog { get; }
        internal CodeDocumentationOneLessonGrantV1 Grant { get; }
        internal CodeDocumentationOneLessonSessionOpen Open { get; private set; }
        internal CodeDocumentationRepositoryOwnedModelCheckpointStartRequest Start { get; }
        internal CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext Context { get; }
        internal DiagnosticClient Client { get; }
        internal ContinuousLearningModelServerStartup Startup { get; }
        private readonly bool syntheticSelectedStudentMode;
        private readonly bool syntheticFullMaterialMode; // Only new tests opt into the private full-material family; all predecessor fixtures retain their opener.
        private readonly bool ownsFixture; // Reattachment arranges the same existing fixture without provisioning or deleting another root.
        internal OrdinaryDiagnosticHarness(Func<int, string, (bool Passed, string Reason)> answer,
            Func<CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture, CodeDocumentationOneLessonTeachingCurriculumV2>? teaching = null,
            Stream? genuineParentWeights = null, Action<LlmModel>? configureGenuineParent = null, bool syntheticSelectedStudentMode = false,
            int? syntheticContextHistoryPairs = null, int contextHistoryPadding = 0, string? contextHistoryFault = null, string? contextHistoryChosenSuffix = null,
            bool syntheticFullMaterialMode = false,
            CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture? existingFixture = null,
            Func<CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture,ContinuousLearningModelServerStartup>? startupFactory = null) // Reuse a released HTTP fixture's saved ORIGINAL without another calibration series.
        {
            this.syntheticSelectedStudentMode = syntheticSelectedStudentMode; // Null/default callers retain the published ordinary fixture and native controls.
            this.syntheticFullMaterialMode = syntheticFullMaterialMode;
            Assert.False(syntheticSelectedStudentMode && syntheticFullMaterialMode);
            ownsFixture = existingFixture is null; Fixture = existingFixture ?? new(10000, 2);
            try
            {
                if (ownsFixture) Fixture.ProvisionOrdinaryV2(5, genuineTeachingCurriculum: teaching?.Invoke(Fixture), genuineParentWeights: genuineParentWeights,
                    configureGenuineParent: configureGenuineParent, syntheticContextHistoryPairs: syntheticContextHistoryPairs,
                    contextHistoryPadding: contextHistoryPadding, contextHistoryFault: contextHistoryFault, contextHistoryChosenSuffix: contextHistoryChosenSuffix);
                var startup = startupFactory?.Invoke(Fixture) ?? ContinuousLearningModelServerStartup.Create(Fixture.ReadEnvironment); Startup = startup;
                Catalog = startup.OneLessonGrantCatalog;
                var resolved = Catalog.Resolve(Fixture.OrdinarySelector);
                Assert.True(resolved.IsSuccess, resolved.ErrorCode); Assert.NotNull(resolved.Value); Grant = resolved.Value!;
                var rootResult = RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(Fixture.RepositoryRoot);
                Assert.True(rootResult.IsSuccess, rootResult.ErrorCode); Assert.NotNull(rootResult.Value); var root = rootResult.Value!;
                var binding = new CodeDocumentationOneLessonSessionBinding(Grant.ObjectiveId, Grant.GrantId, Grant.GrantVersion,
                    Grant.TrainingRunId, Grant.CandidateSha256, Grant.ParentManifestSha256, Grant.ParentWeightsSha256,
                    Grant.ConfigurationSha256, Grant.ExamConditionSha256) { FormatVersion = 2, DescriptorsV2 = Grant.DescriptorsV2 };
                var opened = syntheticFullMaterialMode ? CodeDocumentationOneLessonSession.OpenSyntheticFullMaterialSelectionForTesting(root,binding,Catalog,Fixture.OrdinarySelector) : syntheticSelectedStudentMode ? CodeDocumentationOneLessonSession.OpenSyntheticSelectedStudentCapacityForTesting(root,binding,Catalog,Fixture.OrdinarySelector) :
                    CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root, binding, Catalog.MaxTeacherCalls, Grant);
                Assert.True(opened.IsSuccess, opened.ErrorCode); Open = opened.Value!;
                var seed = ContinuousLearningModelServerConfigurationLoader.ReadRootInitializationSeed(Fixture.ReadEnvironment, required: true)!.Value;
                var authority = CodeDocumentationDpoSeedAuthorityResolver.Resolve(
                    CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture.DomainSlug, seed);
                Assert.True(authority.IsSuccess, authority.ErrorCode); Assert.NotNull(authority.Value);
                Start = new(Fixture.RepositoryRoot, CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture.DomainSlug,
                    CodeDocumentationRepositoryOwnedDpoTrainingEndpointTests.EndpointTrainingFixture.DesignPatternSlug, Fixture.ModelIdSlug, Fixture.ModelVersion,
                    Fixture.ModelConfiguration, Fixture.RuntimeCompatibility, RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied,
                    LoraConfig: IncrementalTrainer.DefaultConfig, AdapterSeedAuthority: authority.Value);
                Client = new(startup.TeacherConfiguration, answer);
                Context = new(Fixture.Corpus.Corpus, Fixture.Corpus.Manifest, BpeTokenizer.Load(Fixture.TokenizerPath), Client,
                    startup.TeacherConfiguration, Fixture.Profile.GenerationSettings, new(5, 2),
                    CodeDocumentationSavedModelAssessmentCalibrationService.DefaultMaximumDuration, DateTimeOffset.UnixEpoch,
                    ["test:ordinary-diagnostic"]);
                Assert.Equal(TrainingSourceProvenance.Identify(Start.ModelConfig).Sha256, Start.RuntimeCompatibility.ConfigSha256);
                var loaded = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(Start.RepositoryRoot,
                    Start.DomainSlug, Start.DesignPatternSlug, Start.ModelIdSlug, Start.ModelVersion, Start.RuntimeCompatibility);
                Assert.True(loaded.IsSuccess, loaded.ErrorCode); Assert.NotNull(loaded.Value); using var parent = loaded.Value!;
                Assert.Equal(Grant.ParentManifestSha256, parent.Manifest.ManifestSha256);
                Assert.Equal(Grant.ParentWeightsSha256, parent.WeightsSha256);
                var reopenedCorpus = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(Fixture.CorpusAddress);
                Assert.True(reopenedCorpus.IsSuccess, reopenedCorpus.ErrorCode);
                Assert.Equal(Context.CorpusManifest, reopenedCorpus.Value!.Manifest);
                Assert.Equal(Context.Corpus.CorpusSha256, reopenedCorpus.Value.Corpus.CorpusSha256);
            }
            catch
            {
                // A failed second seal or later setup must release only this fixture's exact registered material and held handles.
                Open?.Dispose(); Client?.Dispose(); if (ownsFixture) Fixture.Dispose();
                throw;
            }
        }
        internal async Task<OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>> Consume()
        {
            var result = syntheticFullMaterialMode && Open.Session.HasCompletedFullMaterialSelection ?
                await Open.Session.ReattachCompletedFullMaterialBaselineAsync(Catalog, Fixture.OrdinarySelector, Start, Context, CancellationToken.None) :
                await Open.Session.ConsumeOrdinaryBaselineAsync(Catalog, Fixture.OrdinarySelector, Start, Context, CancellationToken.None);
            return result.IsSuccess ? result : OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline>.Fail(
                result.ErrorMessage + "; actual evaluator calls=" + Client.Calls + "; baseline=" + File.Exists(Fixture.OrdinaryBaseline)
                + "; claim=" + File.Exists(Fixture.OrdinaryClaim), result.ErrorCode);
        }
        internal OperationResult<CodeDocumentationOneLessonSessionOpen> Reopen()
        {
            var binding=Open.Session.Snapshot.Binding; Open.Dispose();
            var root=RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(Fixture.RepositoryRoot).Value!;
            var reopened=syntheticFullMaterialMode ? CodeDocumentationOneLessonSession.OpenSyntheticFullMaterialSelectionForTesting(root,binding,Catalog,Fixture.OrdinarySelector) : syntheticSelectedStudentMode ? CodeDocumentationOneLessonSession.OpenSyntheticSelectedStudentCapacityForTesting(root,binding,Catalog,Fixture.OrdinarySelector) :
                CodeDocumentationOneLessonSession.OpenCapacityValidationV2(root,binding,Catalog.MaxTeacherCalls,Grant);
            if(reopened.IsSuccess) Open=reopened.Value!;
            return reopened;
        }
        internal byte[] HistoryBytes() => File.Exists(Fixture.OrdinaryHistory) ? ReadOwnedBytes(Fixture.OrdinaryHistory) : [];
        private static byte[] ReadOwnedBytes(string path)
        {
            // A read must share the already held writer on Windows; it does not acquire or replace owner custody.
            using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
            using var buffer=new MemoryStream(); stream.CopyTo(buffer); return buffer.ToArray();
        }
        internal OperationResult<CodeDocumentationOneLessonSession.OrdinaryDiagnostic> Classify(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b)
            => syntheticFullMaterialMode && Open.Session.HasCompletedFullMaterialSelection ? Open.Session.ClassifyCompletedFullMaterialBaseline(b, Catalog, Fixture.OrdinarySelector) : Open.Session.ClassifyOrdinaryBaseline(b, Catalog, Fixture.OrdinarySelector);
        internal OperationResult<CodeDocumentationOneLessonSession.OrdinaryOriginalTeachingExclusion> Inspect(
            CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b, CancellationToken token = default)
            => syntheticFullMaterialMode && Open.Session.HasCompletedFullMaterialSelection ? Open.Session.InspectCompletedFullMaterialExclusion(b, Catalog, Fixture.OrdinarySelector, token) : Open.Session.InspectOrdinaryOriginalTeachingExclusion(b, Catalog, Fixture.OrdinarySelector, token);
        internal Dictionary<string, (byte[] Bytes, long Ticks)> Files()
            => Fixture.EnumerateOwnedOrdinaryFiles().ToDictionary(p => p,
                // Held exclusive owner locks are represented by path/presence/mtime, not a claim that their bytes were read.
                p => (p.EndsWith(".owner.lock", StringComparison.Ordinal) ? [] : ReadOwnedBytes(p), File.GetLastWriteTimeUtc(p).Ticks));
        internal void AssertFiles(Dictionary<string, (byte[] Bytes, long Ticks)> before)
        {
            var after = Files(); Assert.Equal(before.Keys.Order(), after.Keys.Order());
            foreach (var p in before.Keys) { Assert.Equal(before[p].Bytes, after[p].Bytes); Assert.Equal(before[p].Ticks, after[p].Ticks); }
        }
        internal void ReleaseSession() { Open.Dispose(); Client.Dispose(); } // Release held owner/client before a real host reopens this same fixture.
        public void Dispose() { ReleaseSession(); if (ownsFixture) Fixture.Dispose(); }
    }

    internal sealed class DiagnosticClient(ContinuousLearningTp1DpoConfiguration configuration,
        Func<int, string, (bool Passed, string Reason)> answer) : ILegalTeacherEvaluationInferenceClient
    {
        internal int Calls;
        internal List<string> ClosedValues { get; } = [];
        internal List<string> StudentAnswers { get; } = [];
        public Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(string prompt,
            ILegalTeacherCallPermit permit, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = permit.TryIssue();
            if (!issued.IsSuccess) return Task.FromResult(issued.MapError<TeacherEvaluationInferenceResult>());
            var number = Interlocked.Increment(ref Calls);
            using var input = JsonDocument.Parse(prompt[prompt.IndexOf('{')..]);
            foreach (var property in input.RootElement.EnumerateObject())
                if (property.Value.ValueKind == JsonValueKind.String) ClosedValues.Add(property.Value.GetString()!);
            var caseId = input.RootElement.GetProperty("caseId").GetString()!;
            StudentAnswers.Add(input.RootElement.GetProperty("studentAnswer").GetString()!);
            var response = answer(number, caseId);
            var content = JsonSerializer.Serialize(new { caseId, pass = response.Passed, reason = response.Reason });
            var provenance = new TrainingTeacherProvenance(TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen, TrainingTeacherAccessMode.LocalHostedOpenWeight, TrainingTeacherHostingProvider.Ollama,
                configuration.ModelId, configuration.ExpectedOllamaManifestDigest, TrainingTeacherModelLicense.Apache20,
                false, false, false, false, "diagnostic-evaluation-" + number.ToString("D5"), 1)
                { ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                  ModelArtifactDigest = configuration.ExpectedOllamaModelLayerDigest };
            return Task.FromResult(OperationResult<TeacherEvaluationInferenceResult>.Ok(new(content, provenance)
                { InferenceProfile = LocalQwenOllamaInferenceProfileAuthority.Evaluation }));
        }
        public void Dispose() { }
    }

    [Fact]
    public void ExternalReference_IndependentFixedLiteralVectorIncludesFinalLfAndEveryScalar()
    {
        var objective = new string('0', 64);
        var request = new string('1', 64);
        var baseline = new string('a', 64);
        var preimage = "xiigen-s04-original-baseline-reference-v2\n" + objective + "\n" + request + "\n" + baseline + "\n1\n";
        Assert.Equal(239, Encoding.UTF8.GetByteCount(preimage));
        Assert.Equal("1bf3c8f111cbb0c0bbc1c964bed4c74e00fb6199ea083cf2cd11d979de949db7", H(preimage));
        Assert.Equal("d7570a03aff951bafaa5bb638ecbfdc4e1d1dfe60fb8f6098e173e670cb41718", H(preimage[..^1]));
        var result = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CreateExternalReferenceExpectation(objective, request, baseline, 1);
        Assert.True(result.IsSuccess, result.ErrorCode);
        Assert.Equal("1bf3c8f111cbb0c0bbc1c964bed4c74e", result.Value!.ReceiptId);
        Assert.Equal(baseline, result.Value.ReceiptSha256);
        Assert.NotEqual(H(preimage[..^1])[..32], result.Value.ReceiptId);
        foreach (var (o, r, b, n) in new[] { (new string('2', 64), request, baseline, 1L),
            (objective, new string('2', 64), baseline, 1L), (objective, request, new string('b', 64), 1L),
            (objective, request, baseline, 2L), (objective, request, baseline, (long)CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes) })
        {
            var changed = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CreateExternalReferenceExpectation(o, r, b, n);
            Assert.True(changed.IsSuccess, changed.ErrorCode);
            Assert.NotEqual(result.Value.ReceiptId, changed.Value!.ReceiptId);
            Assert.Equal(H("xiigen-s04-original-baseline-reference-v2\n" + o + "\n" + r + "\n" + b + "\n" +
                n.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n")[..32], changed.Value.ReceiptId);
            Assert.Equal(b, changed.Value.ReceiptSha256);
        }
        var priorCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");
            var invariant = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CreateExternalReferenceExpectation(objective, request, baseline, 1);
            Assert.Equal(result.Value.ReceiptId, invariant.Value!.ReceiptId);
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = priorCulture; }
    }

    [Theory]
    [InlineData("objective", "null")]
    [InlineData("objective", "short")]
    [InlineData("objective", "long")]
    [InlineData("request", "null")]
    [InlineData("request", "upper")]
    [InlineData("request", "prefix")]
    [InlineData("baseline", "null")]
    [InlineData("baseline", "unicode")]
    [InlineData("baseline", "space")]
    [InlineData("baseline", "control")]
    [InlineData("length", "zero")]
    [InlineData("length", "negative")]
    [InlineData("length", "over-cap")]
    [InlineData("length", "max-long")]
    public void ExternalReference_PureFormatterRejectsUnnormalizedScalarGrammar(string member, string mutation)
    {
        string? changed = mutation switch { "null" => null, "short" => new string('a', 63), "long" => new string('a', 65),
            "upper" => new string('A', 64), "prefix" => "sha256:" + new string('a', 64),
            "unicode" => "ａ" + new string('a', 63), "space" => " " + new string('a', 63),
            "control" => "\n" + new string('a', 63), _ => new string('a', 64) };
        var length = mutation switch { "zero" => 0L, "negative" => -1L,
            "over-cap" => CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes + 1L,
            "max-long" => long.MaxValue, _ => 1L };
        var result = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CreateExternalReferenceExpectation(
            member == "objective" ? changed! : new string('0', 64), member == "request" ? changed! : new string('1', 64),
            member == "baseline" ? changed! : new string('a', 64), length);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, result.ErrorCode);
    }

    [Fact]
    public void ExternalReference_InternalSurfaceHasOnlyTwoGetOnlyStringExpectations()
    {
        var type = typeof(CodeDocumentationPreparedBaselineExternalReference);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPublic);
        Assert.Empty(type.GetConstructors());
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.Equal(new[] { "ReceiptId", "ReceiptSha256" }, properties.Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.All(properties, p => { Assert.Equal(typeof(string), p.PropertyType); Assert.Null(p.SetMethod); });
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.Equal(2, fields.Length);
        Assert.All(fields, p => { Assert.True(p.IsInitOnly); Assert.Equal(typeof(string), p.FieldType); });
        var constructor = Assert.Single(type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.True(constructor.IsAssembly);
        Assert.Equal(new[] { typeof(string), typeof(string) }, constructor.GetParameters().Select(p => p.ParameterType));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ExternalReference_RealProducerDetachedProjectionFreshReceiverIgnoresBothMutableGraphs(bool adapters, bool sufficient)
    {
        using var f = new Fixture(adapters, sufficient);
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var original = produced.Value!;
        var projection = f.Owner.GetExternalReference(original);
        Assert.True(projection.IsSuccess, projection.ErrorCode);
        var expected = new CodeDocumentationPreparedBaselineExternalReference(projection.Value!.ReceiptId, projection.Value.ReceiptSha256);
        AssertExternalRecipe(original, expected, File.ReadAllBytes(f.Address.BaselinePath));
        var graph = DurableBaselineGraph(f);
        var storage = SnapshotFiles(f);
        var modelHooks = 0;
        var points = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(_ => modelHooks++, points.Add);
        var fakeIssuer = new object();
        var fake = new CodeDocumentationPreparedBaselineReadback(fakeIssuer, original.Address, original.RequestSha256,
            original.Sha256, original.ByteLength, original.Baseline, true);
        foreach (var owner in new[] { f.Owner, reader })
            foreach (var input in new CodeDocumentationPreparedBaselineReadback?[] { null, fake })
            {
                var refused = owner.GetExternalReference(input);
                Assert.False(refused.IsSuccess); Assert.Null(refused.Value);
                Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, refused.ErrorCode);
            }
        Assert.False(reader.GetExternalReference(original).IsSuccess);
        ((double[])original.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = -1;
        Assert.Equal(expected.ReceiptId, f.Owner.GetExternalReference(original).Value!.ReceiptId);
        CodeDocumentationPreparedBaselineReadback? prior = null;
        for (var i = 0; i < 2; i++)
        {
            var opened = await OpenByExternalReference(f, reader, expected);
            Assert.True(opened.IsSuccess, opened.ErrorCode);
            var fresh = opened.Value!;
            AssertReadbackIdentity(original, fresh);
            Assert.Equal(graph, JsonSerializer.Serialize(fresh.Baseline));
            Assert.NotSame(original, fresh); Assert.NotSame(prior, fresh);
            Assert.True(reader.IsIssuedReadback(fresh)); Assert.False(f.Owner.IsIssuedReadback(fresh)); Assert.False(fresh.WasIssuedBy(fakeIssuer));
            Assert.Equal(expected.ReceiptId, reader.GetExternalReference(fresh).Value!.ReceiptId);
            Assert.Equal(expected.ReceiptSha256, reader.GetExternalReference(fresh).Value!.ReceiptSha256);
            Assert.Equal(f.CaseCount * f.Context.NoisePolicyConfig.RepeatCount,
                fresh.Baseline.Calibration.Calibration.Observations.SelectMany(o => o.ObservedEvaluationRequestIds).Distinct(StringComparer.Ordinal).Count());
            ((double[])fresh.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = -1;
            Assert.Equal(expected.ReceiptId, reader.GetExternalReference(fresh).Value!.ReceiptId);
            prior = fresh;
        }
        Assert.Equal(f.CaseCount * f.Context.NoisePolicyConfig.RepeatCount, f.Client.Calls);
        Assert.Equal(0, modelHooks); Assert.Empty(points);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
        output.WriteLine("S04_EXTERNAL_REFERENCE_POSITIVE " + JsonSerializer.Serialize(new { adapters, sufficient,
            expected.ReceiptId, expected.ReceiptSha256, original.ByteLength, receivingIssuer = reader.IsIssuedReadback(prior),
            observations = prior!.Baseline.Calibration.Calibration.Observations.Count,
            requestIds = prior.Baseline.Calibration.Calibration.Observations.Sum(o => o.ObservedEvaluationRequestIds.Count), additionalCalls = 0 }));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("id-null")]
    [InlineData("sha-null")]
    [InlineData("id-short")]
    [InlineData("id-long")]
    [InlineData("id-upper")]
    [InlineData("id-unicode")]
    [InlineData("sha-short")]
    [InlineData("sha-long")]
    [InlineData("sha-upper")]
    [InlineData("sha-prefix")]
    [InlineData("sha-space")]
    [InlineData("sha-control")]
    public async Task ExternalReference_MalformedExpectationRejectsBeforeNullLowerArgumentsAndCancellationDominates(string mutation)
    {
        var id = mutation switch { "id-null" => null, "id-short" => new string('a', 31), "id-long" => new string('a', 33),
            "id-upper" => new string('A', 32), "id-unicode" => "ａ" + new string('a', 31), _ => new string('a', 32) };
        var sha = mutation switch { "sha-null" => null, "sha-short" => new string('a', 63), "sha-long" => new string('a', 65),
            "sha-upper" => new string('A', 64), "sha-prefix" => "sha256:" + new string('a', 64),
            "sha-space" => " " + new string('a', 63), "sha-control" => "\n" + new string('a', 63), _ => new string('a', 64) };
        var expected = mutation == "null" ? null : new CodeDocumentationPreparedBaselineExternalReference(id!, sha!);
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        var result = await reader.OpenExistingByExternalReferenceAsync(expected, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!);
        Assert.False(result.IsSuccess); Assert.Null(result.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, result.ErrorCode);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => reader.OpenExistingByExternalReferenceAsync(
            expected, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, cancellationToken: cancellation.Token));
    }

    [Theory]
    [InlineData("ReceiptId")]
    [InlineData("ReceiptSha256")]
    [InlineData("ObjectiveSha256")]
    [InlineData("RequestSha256")]
    [InlineData("ByteLength")]
    public async Task ExternalReference_ValidWrongTupleRefusesAfterSuccessfulLowerReopenWithNoNewEffects(string member)
    {
        using var f = new Fixture();
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var original = produced.Value!;
        var correct = f.Owner.GetExternalReference(original).Value!;
        var scalarMismatch = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CreateExternalReferenceExpectation(
            member == "ObjectiveSha256" ? H("different-objective") : original.Address.ObjectiveSha256,
            member == "RequestSha256" ? H("different-request") : original.RequestSha256,
            original.Sha256, member == "ByteLength" ? original.ByteLength + 1 : original.ByteLength).Value!;
        var wrong = new CodeDocumentationPreparedBaselineExternalReference(member == "ReceiptId" ? FlipHex(correct.ReceiptId) : scalarMismatch.ReceiptId,
            member == "ReceiptSha256" ? FlipHex(correct.ReceiptSha256) : scalarMismatch.ReceiptSha256);
        Assert.Matches("^[0-9a-f]{32}$", wrong.ReceiptId); Assert.Matches("^[0-9a-f]{64}$", wrong.ReceiptSha256);
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        var storage = SnapshotFiles(f);
        var lower = await OpenExisting(f, reader);
        Assert.True(lower.IsSuccess, lower.ErrorCode);
        AssertReadbackIdentity(original, lower.Value!);
        var result = await OpenByExternalReference(f, reader, wrong);
        Assert.False(result.IsSuccess); Assert.Null(result.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, result.ErrorCode);
        Assert.Equal(10, f.Client.Calls);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Theory]
    [InlineData("runtime-config")]
    [InlineData("runtime-topology")]
    [InlineData("adapter")]
    [InlineData("seeds")]
    [InlineData("missing-seeds")]
    public async Task ExternalReference_ActualRuntimeAndAdapterBindingFailuresPreserveLowerCodeAndBytes(string mutation)
    {
        using var f = new Fixture(adapters: true);
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var external = f.Owner.GetExternalReference(produced.Value!).Value!;
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        Assert.True((await OpenByExternalReference(f, reader, external)).IsSuccess);
        var storage = SnapshotFiles(f);
        switch (mutation)
        {
            case "runtime-config": f.Runtime = f.Runtime with { ConfigSha256 = H("different-config") }; break;
            case "runtime-topology": f.Runtime = f.Runtime with { TopologySha256 = H("different-topology") }; break;
            case "adapter": f.Adapter = f.Adapter! with { Rank = 3 }; break;
            case "seeds": f.Seeds = new(18, "policy", "reference"); break;
            case "missing-seeds": f.Seeds = null; break;
        }
        var lower = await OpenExisting(f, reader);
        Assert.False(lower.IsSuccess); Assert.Null(lower.Value); Assert.False(string.IsNullOrWhiteSpace(lower.ErrorCode));
        var externalRefused = await OpenByExternalReference(f, reader, external);
        Assert.False(externalRefused.IsSuccess); Assert.Null(externalRefused.Value);
        Assert.Equal(lower.ErrorCode, externalRefused.ErrorCode);
        if (mutation == "runtime-config") Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, lower.ErrorCode);
        if (mutation is "adapter" or "seeds" or "missing-seeds")
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, lower.ErrorCode);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
    }

    [Fact]
    public async Task ExternalReference_RealProjectionCancellationAndLinuxLockReplacementPreserveOwnerFiles()
    {
        using var f = new Fixture();
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var external = f.Owner.GetExternalReference(produced.Value!).Value!;
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        var storage = SnapshotFiles(f);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => reader.OpenExistingByExternalReferenceAsync(external,
            ConsumerCatalog(f), new(f.Grant.ObjectiveId, f.Grant.GrantSelector), f.RepositoryRoot,
            "csharp", "code-documentation", "prepared-model", "v1", f.Config, f.Runtime, f.Context,
            f.Adapter, f.Seeds, cancellation.Token));
        AssertFilesUnchanged(f, storage);
        if (OperatingSystem.IsLinux())
        {
            var retained = f.Address.LockPath + ".retained";
            File.Move(f.Address.LockPath, retained);
            File.CreateSymbolicLink(f.Address.LockPath, retained);
            storage = SnapshotFiles(f);
            var lower = await OpenExisting(f, reader);
            Assert.Equal(RepositoryOwnedTrainingArtifactLayout.InvalidErrorCode, lower.ErrorCode);
            var refused = await OpenByExternalReference(f, reader, external);
            Assert.False(refused.IsSuccess); Assert.Null(refused.Value); Assert.Equal(lower.ErrorCode, refused.ErrorCode);
            Assert.NotNull(new FileInfo(f.Address.LockPath).LinkTarget);
            AssertFilesUnchanged(f, storage);
        }
        else output.WriteLine("NOT_APPLICABLE: Linux lock-symlink custody; Windows DACL remains unproven.");
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Reference_FreshOwnerReturnsCompleteDiskGraphTwiceIgnoringCallerMutation(bool adapters, bool sufficient)
    {
        using var f = new Fixture(adapters, sufficient);
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var original = produced.Value!;
        var reference = original.Reference;
        var projection = ReferenceProjection(reference);
        var graph = DurableBaselineGraph(f);
        var storage = SnapshotFiles(f);
        var hooks = 0;
        var points = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(_ => hooks++, points.Add);
        var originalScore = original.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores[0];
        var changedScore = originalScore == 1 ? 0.0 : 1.0;
        Assert.InRange(changedScore, 0, 1);
        Assert.NotEqual(originalScore, changedScore);
        ((double[])original.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = changedScore;
        for (var i = 0; i < 2; i++)
        {
            var opened = await OpenByReference(f, reader, reference);
            Assert.True(opened.IsSuccess, opened.ErrorCode);
            var fresh = opened.Value!;
            AssertReadbackIdentity(original, fresh);
            Assert.Equal(projection, ReferenceProjection(fresh.Reference));
            Assert.Equal(graph, JsonSerializer.Serialize(fresh.Baseline));
            Assert.NotSame(original, fresh);
            Assert.NotSame(reference, fresh.Reference);
            Assert.True(reader.IsIssuedReadback(fresh));
            Assert.False(f.Owner.IsIssuedReadback(fresh));
            Assert.All(fresh.Baseline.Calibration.Calibration.Observations, o =>
            {
                Assert.Equal(f.CaseCount, o.CaseIds.Count);
                Assert.Equal(f.CaseCount, o.StudentAnswerSha256.Count);
                Assert.Equal(f.CaseCount, o.ObservedEvaluationRequestIds.Count);
            });
            ((double[])fresh.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = changedScore;
        }
        Assert.Equal(projection, ReferenceProjection(reference));
        Assert.Equal(f.CaseCount * f.Context.NoisePolicyConfig.RepeatCount, f.Client.Calls);
        Assert.Equal(0, hooks);
        Assert.Empty(points);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Fact]
    public async Task Reference_FakeReadbackTupleReturnsOnlyFreshOriginalGraphAndCannotCreateMissingHistory()
    {
        using var f = new Fixture();
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var original = produced.Value!;
        var graph = DurableBaselineGraph(f);
        Assert.DoesNotContain(1.0, original.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores);
        ((double[])original.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = 1.0;
        var foreignIssuer = new object();
        var fake = new CodeDocumentationPreparedBaselineReadback(foreignIssuer, original.Address,
            original.RequestSha256, original.Sha256, original.ByteLength, original.Baseline, true);
        var hooks = 0;
        var points = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(_ => hooks++, points.Add);
        Assert.False(reader.IsIssuedReadback(fake));
        var storage = SnapshotFiles(f);
        var opened = await OpenByReference(f, reader, fake.Reference);
        Assert.True(opened.IsSuccess, opened.ErrorCode);
        Assert.True(reader.IsIssuedReadback(opened.Value));
        Assert.False(opened.Value!.WasIssuedBy(foreignIssuer));
        Assert.NotSame(fake, opened.Value);
        Assert.NotSame(fake.Baseline, opened.Value.Baseline);
        Assert.Equal(graph, JsonSerializer.Serialize(opened.Value.Baseline));
        Assert.DoesNotContain(1.0, opened.Value.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores);
        AssertFilesUnchanged(f, storage);
        File.Delete(f.Address.ClaimPath);
        File.Delete(f.Address.BaselinePath);
        storage = SnapshotFiles(f);
        var absent = await OpenByReference(f, reader, fake.Reference);
        Assert.False(absent.IsSuccess);
        Assert.Null(absent.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, absent.ErrorCode);
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(0, hooks);
        Assert.Empty(points);
        AssertFilesUnchanged(f, storage);
    }

    [Theory]
    [InlineData("FormatVersion")]
    [InlineData("RepositoryRoot")]
    [InlineData("ObjectiveSha256")]
    [InlineData("BaselineRoot")]
    [InlineData("BaselineRootRelativePath")]
    [InlineData("BaselinePath")]
    [InlineData("ClaimPath")]
    [InlineData("LockPath")]
    [InlineData("RequestSha256")]
    [InlineData("Sha256")]
    [InlineData("ByteLength")]
    [InlineData("Classification")]
    public async Task Reference_OneExpectedMemberMismatchRefusesWithoutRebindingOrEffects(string member)
    {
        using var f = new Fixture();
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        var r = produced.Value!.Reference;
        Assert.True((await OpenByReference(f, reader, r)).IsSuccess);
        var changed = new CodeDocumentationPreparedBaselineReference(
            member == "FormatVersion" ? 2 : r.FormatVersion,
            member == "RepositoryRoot" ? r.RepositoryRoot + "-other" : r.RepositoryRoot,
            member == "ObjectiveSha256" ? H("another-objective") : r.ObjectiveSha256,
            member == "BaselineRoot" ? r.BaselineRoot + "-other" : r.BaselineRoot,
            member == "BaselineRootRelativePath" ? r.BaselineRootRelativePath + "-other" : r.BaselineRootRelativePath,
            member == "BaselinePath" ? r.BaselinePath + "-other" : r.BaselinePath,
            member == "ClaimPath" ? r.ClaimPath + "-other" : r.ClaimPath,
            member == "LockPath" ? r.LockPath + "-other" : r.LockPath,
            member == "RequestSha256" ? H("another-request") : r.RequestSha256,
            member == "Sha256" ? H("another-raw") : r.Sha256,
            member == "ByteLength" ? r.ByteLength + 1 : r.ByteLength,
            member == "Classification" ? "NO_TRAINING_NEEDED" : r.Classification);
        var expected = ReferenceProjection(changed);
        var storage = SnapshotFiles(f);
        var refused = await OpenByReference(f, reader, changed);
        Assert.False(refused.IsSuccess);
        Assert.Null(refused.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, refused.ErrorCode);
        Assert.Equal(expected, ReferenceProjection(changed));
        AssertFilesUnchanged(f, storage);
        Assert.Equal(10, f.Client.Calls);
    }

    [Fact]
    public async Task Reference_UntrustedExpectationBeforeProductionAndCancellationLeaveNoHistory()
    {
        var points = new List<string>();
        using var f = new Fixture(durablePoint: points.Add);
        var hooks = 0;
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(_ => hooks++, points.Add);
        var proposed = ProposedReference(f);
        var storage = SnapshotFiles(f);
        var absent = await OpenByReference(f, reader, proposed);
        Assert.False(absent.IsSuccess);
        Assert.Null(absent.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, absent.ErrorCode);
        var nullReference = await OpenByReference(f, reader, null);
        Assert.False(nullReference.IsSuccess);
        Assert.Null(nullReference.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, nullReference.ErrorCode);
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => reader.OpenExistingByReferenceAsync(
            null, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!,
            cancellationToken: cancellation.Token));
        Assert.Equal(0, f.Client.Calls);
        Assert.Equal(0, hooks);
        Assert.Empty(points);
        Assert.Null(f.LoadedModel);
        Assert.False(Directory.Exists(f.Address.BaselineRoot));
        Assert.False(File.Exists(f.Address.LockPath));
        Assert.False(File.Exists(f.Address.ClaimPath));
        Assert.False(File.Exists(f.Address.BaselinePath));
        Assert.Empty(Directory.EnumerateDirectories(f.RepositoryRoot, "attempts", SearchOption.AllDirectories));
        AssertFilesUnchanged(f, storage);
    }

    [Fact]
    public void Reference_InternalImmutableSurfaceRetainsOnlyTwelveScalarsAndExistingOwnerContracts()
    {
        var type = typeof(CodeDocumentationPreparedBaselineReference);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPublic);
        Assert.Empty(type.GetConstructors());
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.Equal(new[] { "BaselinePath", "BaselineRoot", "BaselineRootRelativePath", "ByteLength", "ClaimPath",
            "Classification", "FormatVersion", "LockPath", "ObjectiveSha256", "RepositoryRoot", "RequestSha256", "Sha256" },
            properties.Select(p => p.Name).Order(StringComparer.Ordinal));
        Assert.All(properties, p =>
        {
            Assert.Null(p.SetMethod);
            Assert.Contains(p.PropertyType, new[] { typeof(int), typeof(long), typeof(string) });
        });
        Assert.All(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic), field =>
        {
            Assert.True(field.IsInitOnly);
            Assert.Contains(field.FieldType, new[] { typeof(int), typeof(long), typeof(string) });
        });
        var api = typeof(CodeDocumentationRepositoryOwnedPreparedBaselineJournal)
            .GetMethod("OpenExistingByReferenceAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.True(api.IsAssembly);
        Assert.Equal(typeof(Task<OperationResult<CodeDocumentationPreparedBaselineReadback>>), api.ReturnType);
        Assert.Equal(new[] { typeof(CodeDocumentationPreparedBaselineReference), typeof(CodeDocumentationOneLessonGrantCatalog),
            typeof(CodeDocumentationOneLessonSelectorV1), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
            typeof(ModelConfig), typeof(RepositoryOwnedModelRuntimeCompatibility),
            typeof(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext), typeof(LoraConfig),
            typeof(LoraPairRandomSeedAuthority), typeof(CancellationToken) }, api.GetParameters().Select(p => p.ParameterType));
        Assert.Equal(1, CodeDocumentationRepositoryOwnedPreparedBaselineJournal.FormatVersion);
        Assert.Equal(16 * 1024 * 1024, CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes);
    }

    [Fact]
    public async Task Reference_NormalCompiledApiReportsActualLoadedModulesAndCompleteFreshResult()
    {
        using var f = new Fixture();
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var storage = SnapshotFiles(f);
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        var opened = await OpenByReference(f, reader, produced.Value!.Reference);
        Assert.True(opened.IsSuccess, opened.ErrorCode);
        AssertReadbackIdentity(produced.Value, opened.Value!);
        Assert.True(reader.IsIssuedReadback(opened.Value));
        Assert.False(f.Owner.IsIssuedReadback(opened.Value));
        Assert.Equal(DurableBaselineGraph(f), JsonSerializer.Serialize(opened.Value!.Baseline));
        Assert.Equal(10, f.Client.Calls);
        AssertFilesUnchanged(f, storage);
        var fqn = typeof(CodeDocumentationPreparedBaselineTests).FullName + "." +
            nameof(Reference_NormalCompiledApiReportsActualLoadedModulesAndCompleteFreshResult);
        output.WriteLine("S04_REFERENCE_API " + JsonSerializer.Serialize(new { fqn,
            method = typeof(CodeDocumentationRepositoryOwnedPreparedBaselineJournal).FullName + ".OpenExistingByReferenceAsync",
            receivingIssuer = reader.IsIssuedReadback(opened.Value), additionalEvaluationCalls = f.Client.Calls - 10,
            graphSha256 = H(JsonSerializer.Serialize(opened.Value.Baseline)), reference = ReferenceProjection(opened.Value.Reference) }));
        var tests = typeof(CodeDocumentationPreparedBaselineTests).Assembly.Location;
        var normalDirectory = Path.GetDirectoryName(tests)!;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic &&
            !string.IsNullOrEmpty(a.Location) && Path.GetDirectoryName(a.Location) == normalDirectory)
            .OrderBy(a => a.FullName, StringComparer.Ordinal).ToArray();
        Assert.Contains(typeof(CodeDocumentationPreparedBaselineTests).Assembly, assemblies);
        Assert.Contains(typeof(CodeDocumentationRepositoryOwnedPreparedBaselineJournal).Assembly, assemblies);
        Assert.Contains(typeof(CodeDocumentationOneLessonSession).Assembly, assemblies);
        foreach (var assembly in assemblies)
            foreach (var module in assembly.GetModules())
            {
                var path = module.FullyQualifiedName;
                var bytes = File.ReadAllBytes(path);
                output.WriteLine("S04_REFERENCE_MODULE " + JsonSerializer.Serialize(new { assembly = assembly.FullName,
                    path, module.ModuleVersionId, byteLength = bytes.LongLength, sha256 = Hash(bytes),
                    lastWriteTimeUtc = File.GetLastWriteTimeUtc(path) }));
            }
        foreach (var path in new[] { Path.ChangeExtension(tests, ".runtimeconfig.json"), Path.ChangeExtension(tests, ".deps.json") })
        {
            var bytes = File.ReadAllBytes(path);
            output.WriteLine("S04_REFERENCE_RUNTIME_FILE " + JsonSerializer.Serialize(new { path,
                byteLength = bytes.LongLength, sha256 = Hash(bytes), lastWriteTimeUtc = File.GetLastWriteTimeUtc(path) }));
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Consumer_HeldSessionReopensOriginalCompleteGraphTwiceWithNoEffects(bool adapters, bool sufficient)
    {
        using var f = new Fixture(adapters, sufficient);
        f.Context = f.Context with { EvidenceRefs = f.Context.EvidenceRefs.ToArray() };
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var original = produced.Value!;
        var graph = DurableBaselineGraph(f);
        var catalog = ConsumerCatalog(f);
        var storage = ConsumerSetupSnapshot(f);
        var tokenizer = TokenizerBytes(f.Context.Tokenizer);
        var evidence = f.Context.EvidenceRefs.ToArray();
        var modules = f.Adapter?.TargetModules.ToArray();
        using (var lease = ConsumerLease(f))
        {
            var snapshot = lease.Session.Snapshot;
            ((double[])original.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = -1;
            for (var i = 0; i < 2; i++)
            {
                var opened = await Consume(f, lease.Session, catalog);
                Assert.True(opened.IsSuccess, opened.ErrorCode + ": " + opened.ErrorMessage);
                AssertReadbackIdentity(original, opened.Value!);
                Assert.Equal(graph, JsonSerializer.Serialize(opened.Value!.Baseline));
                Assert.Equal(sufficient ? "NO_TRAINING_NEEDED" : "NEEDS_LESSON", opened.Value.Classification);
                Assert.False(f.Owner.IsIssuedReadback(opened.Value));
                Assert.All(opened.Value.Baseline.Calibration.Calibration.Observations, o =>
                {
                    Assert.Equal(f.CaseCount, o.CaseIds.Count);
                    Assert.Equal(f.CaseCount, o.StudentAnswerSha256.Count);
                    Assert.Equal(f.CaseCount, o.ObservedEvaluationRequestIds.Count);
                });
                // Each call returns a fresh durable graph, even after caller mutation.
                ((double[])opened.Value.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = 1;
                AssertHeldConsumerFiles(f, lease.Session, storage);
            }
            Assert.Same(snapshot, lease.Session.Snapshot);
            Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), snapshot.Effects);
            var secondOwner = CodeDocumentationOneLessonSession.OpenCapacityValidationV2(ConsumerRoot(f), ConsumerBinding(f), f.Allowance);
            Assert.False(secondOwner.IsSuccess);
            Assert.Null(secondOwner.Value);
            Assert.Equal(CodeDocumentationOneLessonSession.BusyErrorCode, secondOwner.ErrorCode);
        }
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.CaseCount * f.Context.NoisePolicyConfig.RepeatCount, f.Client.Calls);
        Assert.Equal(tokenizer, TokenizerBytes(f.Context.Tokenizer));
        Assert.Equal(evidence, f.Context.EvidenceRefs);
        if (modules is not null) Assert.Equal(modules, f.Adapter!.TargetModules);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
        Assert.False(File.Exists(ConsumerHistoryPath(f)));
        Assert.Empty(Directory.EnumerateDirectories(f.RepositoryRoot, "attempts", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task Consumer_NoEvidenceDoesNotCreateOriginalHistoryAndOrdinaryProducerStillRunsFirst()
    {
        var points = new List<string>();
        using var f = new Fixture(durablePoint: points.Add);
        var storage = ConsumerSetupSnapshot(f);
        using (var lease = ConsumerLease(f))
        {
            var absent = await Consume(f, lease.Session, ConsumerCatalog(f));
            Assert.False(absent.IsSuccess);
            Assert.Null(absent.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, absent.ErrorCode);
            AssertHeldConsumerFiles(f, lease.Session, storage);
            Assert.Equal(0, f.Client.Calls);
            Assert.Empty(points);
            Assert.Null(f.LoadedModel);
            Assert.False(Directory.Exists(f.Address.BaselineRoot));
            Assert.False(File.Exists(f.Address.LockPath));
            Assert.False(File.Exists(f.Address.ClaimPath));
            Assert.False(File.Exists(f.Address.BaselinePath));
        }
        AssertFilesUnchanged(f, storage);
        var first = await f.Produce();
        Assert.True(first.IsSuccess, first.ErrorCode);
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(new[] { "Started", "BaselinePersisted", "Committed" }, points);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Theory]
    [InlineData("Started")]
    [InlineData("BaselinePersisted")]
    [InlineData("Committed")]
    public async Task Consumer_ActualDurableInterruptionRequiresCommitAndNeverRemeasures(string point)
    {
        var points = new List<string>();
        using var f = new Fixture(durablePoint: p =>
        {
            points.Add(p);
            if (p == point) throw new IOException("actual producer lost return");
        });
        var produced = await f.Produce();
        Assert.False(produced.IsSuccess);
        var calls = f.Client.Calls;
        var beforePoints = points.ToArray();
        var storage = ConsumerSetupSnapshot(f);
        using (var lease = ConsumerLease(f))
        {
            for (var i = 0; i < 2; i++)
            {
                var opened = await Consume(f, lease.Session, ConsumerCatalog(f));
                Assert.Equal(point == "Committed", opened.IsSuccess);
                if (point == "Committed")
                {
                    Assert.Equal(DurableBaselineGraph(f), JsonSerializer.Serialize(opened.Value!.Baseline));
                    Assert.Equal(Hash(File.ReadAllBytes(f.Address.BaselinePath)), opened.Value.Sha256);
                }
                else
                {
                    Assert.Null(opened.Value);
                    Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, opened.ErrorCode);
                }
                AssertHeldConsumerFiles(f, lease.Session, storage);
            }
        }
        AssertFilesUnchanged(f, storage);
        Assert.Equal(point == "Started" ? 0 : 10, calls);
        Assert.Equal(calls, f.Client.Calls);
        Assert.Equal(beforePoints, points);
    }

    [Theory]
    [InlineData("missing-lock")]
    [InlineData("missing-claim")]
    [InlineData("missing-baseline")]
    [InlineData("empty-history")]
    [InlineData("torn-claim")]
    [InlineData("commit-hash")]
    [InlineData("commit-length")]
    [InlineData("corrupt-baseline")]
    [InlineData("noncanonical-baseline")]
    public async Task Consumer_OwnedHistoryMutationPropagatesOriginalGuardWithoutWrites(string mutation)
    {
        using var f = new Fixture();
        var control = await f.Produce();
        Assert.True(control.IsSuccess, control.ErrorCode);
        var lines = File.ReadAllLines(f.Address.ClaimPath);
        var committed = JsonNode.Parse(lines[1])!.AsObject();
        var code = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode;
        switch (mutation)
        {
            case "missing-lock": File.Delete(f.Address.LockPath); break;
            case "missing-claim": File.Delete(f.Address.ClaimPath); break;
            case "missing-baseline": File.Delete(f.Address.BaselinePath); break;
            case "empty-history":
                File.Delete(f.Address.ClaimPath); File.Delete(f.Address.BaselinePath);
                code = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode; break;
            case "torn-claim": File.WriteAllText(f.Address.ClaimPath, lines[0] + "\n" + lines[1][..^1], new UTF8Encoding(false)); break;
            case "commit-hash": committed["BaselineSha256"] = new string('0', 64); WriteCommittedClaim(f, lines[0], committed); break;
            case "commit-length": committed["BaselineByteLength"] = control.Value!.ByteLength + 1; WriteCommittedClaim(f, lines[0], committed); break;
            case "corrupt-baseline":
            case "noncanonical-baseline":
                var bytes = mutation == "corrupt-baseline" ? "{"u8.ToArray() : File.ReadAllBytes(f.Address.BaselinePath).Concat(" "u8.ToArray()).ToArray();
                File.WriteAllBytes(f.Address.BaselinePath, bytes);
                committed["BaselineSha256"] = Hash(bytes); committed["BaselineByteLength"] = bytes.LongLength;
                WriteCommittedClaim(f, lines[0], committed); break;
            default: throw new ArgumentOutOfRangeException(nameof(mutation));
        }
        var storage = ConsumerSetupSnapshot(f);
        using (var lease = ConsumerLease(f))
        {
            var refused = await Consume(f, lease.Session, ConsumerCatalog(f));
            Assert.False(refused.IsSuccess);
            Assert.Null(refused.Value);
            Assert.Equal(code, refused.ErrorCode);
            AssertHeldConsumerFiles(f, lease.Session, storage);
        }
        AssertFilesUnchanged(f, storage);
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Theory]
    [InlineData("observed-at")]
    [InlineData("duration")]
    [InlineData("evidence")]
    [InlineData("tokenizer")]
    [InlineData("generation")]
    [InlineData("noise")]
    [InlineData("evaluator")]
    [InlineData("modules")]
    public async Task Consumer_ValidChangedInputsReachOriginalRequestBindingGuard(string mutation)
    {
        using var f = new Fixture(adapters: mutation == "modules");
        if (mutation == "evidence")
            f.Context = f.Context with { EvidenceRefs = f.Context.EvidenceRefs.ToArray() };
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var external = f.Owner.GetExternalReference(produced.Value!).Value!;
        var storage = ConsumerSetupSnapshot(f);
        using (var lease = ConsumerLease(f))
        {
            Assert.True((await Consume(f, lease.Session, ConsumerCatalog(f))).IsSuccess);
            switch (mutation)
            {
                case "observed-at": f.Context = f.Context with { ObservedAtUtc = f.Context.ObservedAtUtc.AddSeconds(1) }; break;
                case "duration": f.Context = f.Context with { MaximumDuration = f.Context.MaximumDuration.Add(TimeSpan.FromSeconds(1)) }; break;
                case "evidence": ((string[])f.Context.EvidenceRefs)[0] = "test:other-valid-evidence"; break;
                case "tokenizer": f.Context.Tokenizer.Train("other tokenizer corpus", 257); break;
                case "generation": f.Context = f.Context with { GenerationSettings = f.Context.GenerationSettings with { Seed = 18 } }; break;
                case "noise": f.Context = f.Context with { NoisePolicyConfig = new(5, 3) }; break;
                case "evaluator": f.Context = f.Context with { EvaluatorConfiguration = Teacher(21) }; break;
                // Reordering is valid topology but changes the original exact supplied request bytes.
                case "modules": Array.Reverse(f.Adapter!.TargetModules); break;
            }
            Assert.True(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.ValidateContext(f.Context).IsSuccess);
            var validPlan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(
                f.Context.Corpus, f.Context.GenerationSettings, f.Context.EvaluatorConfiguration, f.Context.NoisePolicyConfig);
            Assert.True(validPlan.IsSuccess, validPlan.ErrorCode);
            Assert.Equal(10, validPlan.Value!.BaselinePlan.ExpectedEvaluationCallCount);
            var refused = await Consume(f, lease.Session, ConsumerCatalog(f));
            Assert.False(refused.IsSuccess);
            Assert.Null(refused.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, refused.ErrorCode);
            var externalRefused = await OpenByExternalReference(f, new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(), external);
            Assert.False(externalRefused.IsSuccess);
            Assert.Null(externalRefused.Value);
            Assert.Equal(refused.ErrorCode, externalRefused.ErrorCode);
            AssertHeldConsumerFiles(f, lease.Session, storage);
        }
        AssertFilesUnchanged(f, storage);
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
    }

    [Theory]
    [InlineData("objective")]
    [InlineData("grant")]
    [InlineData("version")]
    [InlineData("run")]
    [InlineData("candidate")]
    [InlineData("parent-manifest")]
    [InlineData("parent-weights")]
    [InlineData("configuration")]
    [InlineData("exam")]
    [InlineData("descriptor")]
    public async Task Consumer_ValidCatalogBindingDisagreementPrecedesOriginalAccess(string field)
    {
        using var f = new Fixture();
        var storage = ConsumerSetupSnapshot(f);
        using (var lease = ConsumerLease(f))
        {
            var g = f.Grant;
            var changed = field switch
            {
                "objective" => g with { ObjectiveId = "other-objective" },
                "grant" => g with { GrantId = "other-grant" },
                "version" => g with { GrantVersion = "v2" },
                "run" => g with { TrainingRunId = "other-run" },
                "candidate" => g with { CandidateSha256 = H("other-candidate") },
                "parent-manifest" => g with { ParentManifestSha256 = H("other-parent-manifest") },
                "parent-weights" => g with { ParentWeightsSha256 = H("other-parent-weights") },
                "configuration" => g with { ConfigurationSha256 = H("other-config") },
                "exam" => g with { ExamConditionSha256 = H("other-exam") },
                "descriptor" => g with { DescriptorsV2 = g.DescriptorsV2! with { ProfileSelectionSha256 = H("other-profile") } },
                _ => throw new ArgumentOutOfRangeException(nameof(field)),
            };
            var catalog = CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([changed], f.Allowance);
            var selector = new CodeDocumentationOneLessonSelectorV1(changed.ObjectiveId, changed.GrantSelector);
            Assert.True(catalog.Resolve(selector).IsSuccess);
            var refused = await lease.Session.ConsumePreparedBaselineAsync(catalog, selector, ConsumerStart(f), f.Context);
            Assert.False(refused.IsSuccess);
            Assert.Null(refused.Value);
            Assert.Equal(CodeDocumentationOneLessonSession.ObjectiveConflictErrorCode, refused.ErrorCode);
            AssertHeldConsumerFiles(f, lease.Session, storage);
        }
        AssertFilesUnchanged(f, storage);
        Assert.Equal(0, f.Client.Calls);
        Assert.False(Directory.Exists(f.Address.BaselineRoot));
    }

    [Fact]
    public async Task Consumer_ValidAlternativeRootRefusesBeforeOpeningEitherOriginalOwner()
    {
        using var f = new Fixture();
        using var other = new Fixture();
        Assert.True(RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(other.RepositoryRoot).IsSuccess);
        var storage = ConsumerSetupSnapshot(f);
        var alternative = SnapshotFiles(other);
        using (var lease = ConsumerLease(f))
        {
            var result = await Consume(f, lease.Session, ConsumerCatalog(f), ConsumerStart(f) with { RepositoryRoot = other.RepositoryRoot });
            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(CodeDocumentationOneLessonSession.InvalidErrorCode, result.ErrorCode);
            AssertHeldConsumerFiles(f, lease.Session, storage);
        }
        AssertFilesUnchanged(f, storage);
        AssertFilesUnchanged(other, alternative);
        Assert.Equal(0, f.Client.Calls);
        Assert.Equal(0, other.Client.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Consumer_DisposedOrNonV2OwnerRefusesBeforeArgumentsCancellationAndOriginalAccess(bool nonV2)
    {
        using var f = new Fixture();
        var binding = ConsumerBinding(f);
        var opened = nonV2 ? CodeDocumentationOneLessonSession.OpenOrCreate(ConsumerRoot(f), binding with { FormatVersion = 1, DescriptorsV2 = null })
            : CodeDocumentationOneLessonSession.OpenCapacityValidationV2(ConsumerRoot(f), binding, f.Allowance);
        Assert.True(opened.IsSuccess, opened.ErrorCode);
        opened.Value!.Dispose();
        var storage = SnapshotFiles(f);
        opened = nonV2 ? CodeDocumentationOneLessonSession.OpenOrCreate(ConsumerRoot(f), binding with { FormatVersion = 1, DescriptorsV2 = null })
            : CodeDocumentationOneLessonSession.OpenCapacityValidationV2(ConsumerRoot(f), binding, f.Allowance);
        Assert.True(opened.IsSuccess, opened.ErrorCode);
        using var owner = opened.Value!;
        if (!nonV2) owner.Dispose();
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var result = await owner.Session.ConsumePreparedBaselineAsync(null!, null!, null!, null!, cancellation.Token);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(CodeDocumentationOneLessonSession.CapacityInvalidErrorCode, result.ErrorCode);
        Assert.Equal(0, f.Client.Calls);
        Assert.False(Directory.Exists(f.Address.BaselineRoot));
        owner.Dispose();
        AssertFilesUnchanged(f, storage);
    }

    [Fact]
    public async Task Consumer_PreCancelledTokenThrowsBeforeAnyOriginalWorkAndPreservesInputs()
    {
        using var f = new Fixture(adapters: true);
        var storage = ConsumerSetupSnapshot(f);
        var tokenizer = TokenizerBytes(f.Context.Tokenizer);
        var evidence = f.Context.EvidenceRefs.ToArray();
        var modules = f.Adapter!.TargetModules.ToArray();
        using (var lease = ConsumerLease(f))
        {
            using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => Consume(f, lease.Session, ConsumerCatalog(f), token: cancellation.Token));
            AssertHeldConsumerFiles(f, lease.Session, storage);
        }
        AssertFilesUnchanged(f, storage);
        Assert.Equal(0, f.Client.Calls);
        Assert.False(Directory.Exists(f.Address.BaselineRoot));
        Assert.Equal(tokenizer, TokenizerBytes(f.Context.Tokenizer));
        Assert.Equal(evidence, f.Context.EvidenceRefs);
        Assert.Equal(modules, f.Adapter.TargetModules);
    }

    [Theory]
    [InlineData("lock")]
    [InlineData("history")]
    [InlineData("appeared-history")]
    public async Task Consumer_LinuxCurrentCustodyPrecheckRefusesReplacementAndPreservesBytes(string target)
    {
        // Preserve the historical three identities; Windows executes its native sharing/presence controls.
        if (OperatingSystem.IsWindows())
        {
            await ConsumerWindowsCurrentCustodyControl(target);
            return;
        }
        Assert.True(OperatingSystem.IsLinux(), "These custody controls require the actual Linux or Windows file owner.");
        using var f = new Fixture();
        ConsumerSetupSnapshot(f);
        if (target == "history")
        {
            File.WriteAllBytes(ConsumerHistoryPath(f), ConsumerGenesis(f));
            File.SetUnixFileMode(ConsumerHistoryPath(f), UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
        var storage = SnapshotFiles(f);
        using (var lease = ConsumerLease(f))
        {
            Assert.True(lease.Session.ReadCapacityPrefixV2().IsSuccess);
            var path = target == "lock" ? ConsumerLockPath(f) : ConsumerHistoryPath(f);
            var bytes = target == "appeared-history" ? "appeared untrusted prefix\n"u8.ToArray() : storage[Path.GetRelativePath(f.RepositoryRoot, path)];
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllBytes(path, bytes);
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            var replaced = Directory.GetFiles(f.RepositoryRoot, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
            var refused = await Consume(f, lease.Session, ConsumerCatalog(f));
            Assert.False(refused.IsSuccess);
            Assert.Null(refused.Value);
            Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, refused.ErrorCode);
            Assert.Equal(bytes, File.ReadAllBytes(path));
            Assert.Equal(replaced, Directory.GetFiles(f.RepositoryRoot, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
            Assert.Equal(0, f.Client.Calls);
            Assert.False(Directory.Exists(f.Address.BaselineRoot));
        }
        var final = SnapshotFiles(f);
        foreach (var entry in storage) Assert.Equal(entry.Value, final[entry.Key]);
    }

    private async Task ConsumerWindowsCurrentCustodyControl(string target)
    {
        Assert.Contains(target, new[] { "lock", "history", "appeared-history" });
        using var f = new Fixture();
        CodeDocumentationPreparedBaselineReadback? original = null;
        if (target != "appeared-history")
        {
            var produced = await f.Produce();
            Assert.True(produced.IsSuccess, target + " valid ORIGINAL: " + produced.ErrorCode);
            original = produced.Value!;
            Assert.Equal(f.CaseCount * f.Context.NoisePolicyConfig.RepeatCount, f.Client.Calls);
        }
        ConsumerSetupSnapshot(f);
        if (target == "history")
            RestoreCustodyFile(ConsumerHistoryPath(f), ConsumerGenesis(f), DateTime.UtcNow);
        var replacement = Path.Combine(f.RepositoryRoot, "native-custody-replacement.tmp");
        var replacementBytes = "owned replacement must not displace held custody\n"u8.ToArray();
        if (target != "appeared-history")
            RestoreCustodyFile(replacement, replacementBytes, DateTime.UtcNow);
        var storage = SnapshotFiles(f);
        var timestamps = storage.Keys.ToDictionary(p => p,
            p => File.GetLastWriteTimeUtc(Path.Combine(f.RepositoryRoot, p)), StringComparer.Ordinal);
        using (var lease = ConsumerLease(f))
        {
            var snapshot = lease.Session.Snapshot;
            var prefix = lease.Session.ReadCapacityPrefixV2();
            Assert.True(prefix.IsSuccess, target + " valid held owner: " + prefix.ErrorCode);
            Assert.Equal(target == "history" ? 1 : 0, prefix.Value!.PrefixFrames);
            AssertHeldConsumerFiles(f, lease.Session, storage);
            if (target == "appeared-history")
            {
                // The genuine receiving owner has no history handle; appearance must fail its first precheck.
                Assert.False(File.Exists(ConsumerHistoryPath(f)));
                Assert.False(Directory.Exists(f.Address.BaselineRoot));
                Assert.Equal(0, f.Client.Calls);
                var appeared = "appeared untrusted prefix\n"u8.ToArray();
                RestoreCustodyFile(ConsumerHistoryPath(f), appeared, DateTime.UtcNow);
                var relative = Path.GetRelativePath(f.RepositoryRoot, ConsumerHistoryPath(f));
                storage.Add(relative, appeared);
                timestamps.Add(relative, File.GetLastWriteTimeUtc(ConsumerHistoryPath(f)));
                var invalidPrefix = lease.Session.ReadCapacityPrefixV2();
                Assert.False(invalidPrefix.IsSuccess);
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, invalidPrefix.ErrorCode);
                // A pre-cancelled request still returns custody recovery before cancellation or lower-owner access.
                var refused = await Consume(f, lease.Session, ConsumerCatalog(f), token: new CancellationToken(true));
                Assert.False(refused.IsSuccess);
                Assert.Null(refused.Value);
                Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, refused.ErrorCode);
                Assert.Equal(0, f.Client.Calls);
                Assert.False(Directory.Exists(f.Address.BaselineRoot));
                AssertHeldConsumerFiles(f, lease.Session, storage,
                    CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode);
            }
            else
            {
                // Establish the actual successful consumer before testing denied Windows delete/overwrite operations.
                var control = await Consume(f, lease.Session, ConsumerCatalog(f));
                Assert.True(control.IsSuccess, target + " valid consumer: " + control.ErrorCode);
                AssertReadbackIdentity(original!, control.Value!);
                var path = target == "lock" ? ConsumerLockPath(f) : ConsumerHistoryPath(f);
                Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path));
                AssertWindowsDeleteSharingDenial(() => File.Delete(path), target + " delete");
                AssertWindowsOverwriteDenial(() => File.Move(replacement, path, overwrite: true), target + " overwrite");
                Assert.Equal(replacementBytes, File.ReadAllBytes(replacement));
                AssertHeldConsumerFiles(f, lease.Session, storage);
                var reopened = await Consume(f, lease.Session, ConsumerCatalog(f));
                Assert.True(reopened.IsSuccess, target + " consumer after native refusal: " + reopened.ErrorCode);
                AssertReadbackIdentity(original!, reopened.Value!);
                Assert.Equal(f.CaseCount * f.Context.NoisePolicyConfig.RepeatCount, f.Client.Calls);
                AssertHeldConsumerFiles(f, lease.Session, storage);
            }
            Assert.Same(snapshot, lease.Session.Snapshot);
            Assert.Equal(new CodeDocumentationOneLessonEffectAccounting(), snapshot.Effects);
        }
        // Closing custody permits exact lock readback; no denied operation may alter any owned bytes/mtime.
        AssertFilesUnchanged(f, storage);
        foreach (var entry in timestamps)
            Assert.Equal(entry.Value, File.GetLastWriteTimeUtc(Path.Combine(f.RepositoryRoot, entry.Key)));
        output.WriteLine("WINDOWS_NATIVE_CUSTODY target=" + target + "; native delete/overwrite/presence control; overwrite denial alone does not identify sharing or DACL cause; Linux unlink/race/DACL proof NOT_CLAIMED");
    }

    private void AssertWindowsDeleteSharingDenial(Action operation, string boundary)
    {
        var denied = Record.Exception(operation);
        var actual = denied is null ? "NO_EXCEPTION" : denied.GetType().FullName + "/HRESULT=0x" + denied.HResult.ToString("X8");
        output.WriteLine("WINDOWS_NATIVE_DENIAL boundary=" + boundary + "; actual=" + actual);
        Assert.True(denied is not null && denied.GetType() == typeof(IOException) && denied.HResult == unchecked((int)0x80070020),
            boundary + " requires exact IOException/ERROR_SHARING_VIOLATION0x80070020; actual=" + actual);
    }

    private void AssertWindowsOverwriteDenial(Action operation, string boundary)
    {
        // File.Move(overwrite) also documents access denial when the OS cannot acquire exclusive destination access.
        // Record the exception, then admit only these exact type/HRESULT pairs; access denial does not certify its cause.
        var denied = Record.Exception(operation);
        var actual = denied is null ? "NO_EXCEPTION" : denied.GetType().FullName + "/HRESULT=0x" + denied.HResult.ToString("X8");
        output.WriteLine("WINDOWS_NATIVE_DENIAL boundary=" + boundary + "; actual=" + actual);
        Assert.True(denied is not null && ((denied.GetType() == typeof(IOException) && denied.HResult == unchecked((int)0x80070020)) ||
            (denied.GetType() == typeof(UnauthorizedAccessException) && denied.HResult == unchecked((int)0x80070005))),
            boundary + " requires exact IOException/0x80070020 or UnauthorizedAccessException/0x80070005; actual=" + actual);
    }

    [Fact]
    public async Task Consumer_OriginalBusyPropagatesAndBlockedProducerAlwaysFinishesInFinally()
    {
        using var f = new Fixture();
        ConsumerSetupSnapshot(f);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.BeforeResponse = async () => { entered.TrySetResult(); await resume.Task; };
        var producer = f.Produce();
        OperationResult<CodeDocumentationPreparedBaselineReadback>? completed = null;
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var calls = f.Client.Calls;
            var claim = File.ReadAllBytes(f.Address.ClaimPath);
            var paths = Directory.GetFiles(f.RepositoryRoot, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
            // The blocked producer owns its exclusive lock: compare all other bytes exactly.
            var accessible = paths.Where(p => p != f.Address.LockPath).ToDictionary(p => p, File.ReadAllBytes);
            using (var lease = ConsumerLease(f))
            {
                var refused = await Consume(f, lease.Session, ConsumerCatalog(f));
                Assert.False(refused.IsSuccess);
                Assert.Null(refused.Value);
                Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.BusyErrorCode, refused.ErrorCode);
                Assert.True(lease.Session.ReadCapacityPrefixV2().IsSuccess);
                Assert.Equal(calls, f.Client.Calls);
                Assert.Equal(claim, File.ReadAllBytes(f.Address.ClaimPath));
                Assert.Equal(paths, Directory.GetFiles(f.RepositoryRoot, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal));
                foreach (var entry in accessible.Where(p => p.Key != ConsumerLockPath(f))) Assert.Equal(entry.Value, File.ReadAllBytes(entry.Key));
            }
            foreach (var entry in accessible) Assert.Equal(entry.Value, File.ReadAllBytes(entry.Key));
        }
        finally
        {
            resume.TrySetResult();
            completed = await producer;
        }
        Assert.True(completed.IsSuccess, completed.ErrorCode);
        Assert.Equal(10, f.Client.Calls);
        var storage = ConsumerSetupSnapshot(f);
        using (var lease = ConsumerLease(f))
        {
            var opened = await Consume(f, lease.Session, ConsumerCatalog(f));
            Assert.True(opened.IsSuccess, opened.ErrorCode);
            AssertReadbackIdentity(completed.Value!, opened.Value!);
            AssertHeldConsumerFiles(f, lease.Session, storage);
        }
        AssertFilesUnchanged(f, storage);
        Assert.Equal(10, f.Client.Calls);
    }

    [Fact]
    public void Consumer_InternalSurfaceAndNormalV2RouteGrantNoEffectCapability()
    {
        using var f = new Fixture();
        var storage = ConsumerSetupSnapshot(f);
        var normal = CodeDocumentationOneLessonSession.OpenOrCreate(ConsumerRoot(f), ConsumerBinding(f), f.Allowance);
        Assert.False(normal.IsSuccess);
        Assert.Null(normal.Value);
        Assert.Equal(CodeDocumentationOneLessonSession.BaselinePrerequisiteUnavailableErrorCode, normal.ErrorCode);
        AssertFilesUnchanged(f, storage);
        Assert.False(File.Exists(ConsumerHistoryPath(f)));
        Assert.Equal(0, f.Client.Calls);
        foreach (var method in new[]
        {
            typeof(CodeDocumentationOneLessonSession).GetMethod("ConsumePreparedBaselineAsync", BindingFlags.Instance | BindingFlags.NonPublic)!,
            typeof(CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator).GetMethod("ReadPreparedBaselinePrerequisiteAsync", BindingFlags.Static | BindingFlags.NonPublic)!,
        })
        {
            Assert.NotNull(method);
            Assert.True(method.IsAssembly);
            Assert.Equal(new[] { typeof(CodeDocumentationOneLessonGrantCatalog), typeof(CodeDocumentationOneLessonSelectorV1),
                typeof(CodeDocumentationRepositoryOwnedModelCheckpointStartRequest), typeof(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext),
                typeof(CancellationToken) }, method.GetParameters().Select(p => p.ParameterType));
            Assert.Equal(typeof(Task<OperationResult<CodeDocumentationPreparedBaselineReadback>>), method.ReturnType);
        }
    }

    [Theory]
    [InlineData("transition")]
    [InlineData("nonzero-genesis")]
    public void Consumer_UnsupportedPrefixIsLeaseAdmissionRefusalWithoutConsumerInvocation(string kind)
    {
        using var f = new Fixture();
        ConsumerSetupSnapshot(f);
        var genesis = ConsumerGenesis(f);
        var frame = JsonNode.Parse(genesis)!.AsObject();
        byte[] bytes;
        if (kind == "transition")
        {
            frame["Kind"] = "transition";
            frame["Revision"] = "1";
            frame["PreviousFrameSha256"] = Hash(genesis[..^1]);
            frame["TransitionEvent"] = "RunnerStarted";
            using var document = JsonDocument.Parse(frame.ToJsonString());
            bytes = genesis.Concat(CodeDocumentationOneLessonV2Wire.CanonicalBytes(document.RootElement)).Concat("\n"u8.ToArray()).ToArray();
        }
        else
        {
            frame["Effects"]!["BaselineDispatched"] = "1";
            using var document = JsonDocument.Parse(frame.ToJsonString());
            bytes = CodeDocumentationOneLessonV2Wire.CanonicalBytes(document.RootElement).Concat("\n"u8.ToArray()).ToArray();
        }
        File.WriteAllBytes(ConsumerHistoryPath(f), bytes);
        if (OperatingSystem.IsLinux()) File.SetUnixFileMode(ConsumerHistoryPath(f), UnixFileMode.UserRead | UnixFileMode.UserWrite);
        var storage = SnapshotFiles(f);
        var refused = CodeDocumentationOneLessonSession.OpenCapacityValidationV2(ConsumerRoot(f), ConsumerBinding(f), f.Allowance);
        Assert.False(refused.IsSuccess);
        Assert.Null(refused.Value);
        Assert.Equal(CodeDocumentationOneLessonSession.RecoveryRequiredErrorCode, refused.ErrorCode);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(0, f.Client.Calls);
        Assert.False(Directory.Exists(f.Address.BaselineRoot));
    }

    [Fact]
    public async Task Consumer_NewApiRuntimeReportsFiveLoadedNormalArtifacts()
    {
        using var f = new Fixture();
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var storage = ConsumerSetupSnapshot(f);
        using (var lease = ConsumerLease(f))
        {
            var opened = await Consume(f, lease.Session, ConsumerCatalog(f));
            Assert.True(opened.IsSuccess, opened.ErrorCode);
            AssertReadbackIdentity(produced.Value!, opened.Value!);
            AssertHeldConsumerFiles(f, lease.Session, storage);
        }
        AssertFilesUnchanged(f, storage);
        Assert.Equal(10, f.Client.Calls);
        var tests = typeof(CodeDocumentationPreparedBaselineTests).Assembly.Location;
        foreach (var path in new[]
        {
            typeof(CodeDocumentationRepositoryOwnedPreparedBaselineJournal).Assembly.Location,
            typeof(CodeDocumentationOneLessonSession).Assembly.Location,
            tests, Path.ChangeExtension(tests, ".runtimeconfig.json"), Path.ChangeExtension(tests, ".deps.json"),
        })
        {
            var bytes = File.ReadAllBytes(path);
            output.WriteLine("S04_CONSUMER_RUNTIME " + JsonSerializer.Serialize(new { path, byteLength = bytes.LongLength, sha256 = Hash(bytes) }));
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ExistingOnly_FreshOwnerReopensFullDiskGraphWithNewIssuerAndNoEffects(
        bool adapters, bool sufficient)
    {
        using var f = new Fixture(adapters, sufficient);
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode + ": " + produced.ErrorMessage);
        var original = produced.Value!;
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(Path.GetDirectoryName(f.Address.BaselineRoot)!));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(f.Address.BaselineRoot));
        Assert.All(new[] { f.Address.LockPath, f.Address.ClaimPath, f.Address.BaselinePath },
            path => Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path)));
        var expectedCalls = f.CaseCount * f.Context.NoisePolicyConfig.RepeatCount;
        Assert.Equal(expectedCalls, f.Client.Calls);
        var storage = SnapshotFiles(f);
        var graph = DurableBaselineGraph(f);
        var modelHooks = 0;
        var durableHooks = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(
            _ => modelHooks++, durableHooks.Add);
        // A copied mutable graph from the old issuer cannot replace the actual stored result.
        ((double[])original.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = -1;
        for (var i = 0; i < 2; i++)
        {
            var opened = await OpenExisting(f, reader);
            Assert.True(opened.IsSuccess, opened.ErrorCode + ": " + opened.ErrorMessage);
            var current = opened.Value!;
            AssertReadbackIdentity(original, current);
            Assert.Equal(sufficient ? "NO_TRAINING_NEEDED" : "NEEDS_LESSON", current.Classification);
            Assert.Equal(graph, JsonSerializer.Serialize(current.Baseline));
            Assert.Equal(f.Context.NoisePolicyConfig.RepeatCount,
                current.Baseline.Calibration.Calibration.Observations.Count);
            Assert.All(current.Baseline.Calibration.Calibration.Observations, observation =>
            {
                Assert.Equal(f.CaseCount, observation.CaseIds.Count);
                Assert.Equal(f.CaseCount, observation.StudentAnswerSha256.Count);
                Assert.Equal(f.CaseCount, observation.ObservedEvaluationRequestIds.Count);
            });
            Assert.False(f.Owner.IsIssuedReadback(current));
            Assert.True(reader.IsIssuedReadback(current));
            var rejectedByOld = await f.Reopen(current);
            Assert.False(rejectedByOld.IsSuccess);
            Assert.Null(rejectedByOld.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, rejectedByOld.ErrorCode);
            var issued = await ReopenIssued(f, reader, current);
            Assert.True(issued.IsSuccess, issued.ErrorCode);
            AssertReadbackIdentity(current, issued.Value!);
            Assert.Equal(graph, JsonSerializer.Serialize(issued.Value!.Baseline));
        }
        Assert.Equal(expectedCalls, f.Client.Calls);
        Assert.Equal(0, modelHooks);
        Assert.Empty(durableHooks);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Fact]
    public async Task ExistingOnly_BeforeProductionDoesNotCreateOrBurnHistory()
    {
        var producerPoints = new List<string>();
        using var f = new Fixture(durablePoint: producerPoints.Add);
        var storage = SnapshotFiles(f);
        var modelHooks = 0;
        var durableHooks = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(
            _ => modelHooks++, durableHooks.Add);
        var absent = await OpenExisting(f, reader);
        Assert.False(absent.IsSuccess);
        Assert.Null(absent.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode, absent.ErrorCode);
        var externalAbsent = await OpenByExternalReference(f, reader, ProposedExternalReference());
        Assert.False(externalAbsent.IsSuccess);
        Assert.Null(externalAbsent.Value);
        Assert.Equal(absent.ErrorCode, externalAbsent.ErrorCode);
        Assert.Equal(0, f.Client.Calls);
        Assert.Equal(0, modelHooks);
        Assert.Empty(durableHooks);
        Assert.Empty(producerPoints);
        Assert.Null(f.LoadedModel);
        Assert.False(Directory.Exists(f.Address.BaselineRoot));
        Assert.False(File.Exists(f.Address.LockPath));
        Assert.False(File.Exists(f.Address.ClaimPath));
        Assert.False(File.Exists(f.Address.BaselinePath));
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        Assert.Equal(f.CaseCount * f.Context.NoisePolicyConfig.RepeatCount, f.Client.Calls);
        Assert.Equal(new[] { "Started", "BaselinePersisted", "Committed" }, producerPoints);
        storage = SnapshotFiles(f);
        var opened = await OpenExisting(f, reader);
        Assert.True(opened.IsSuccess, opened.ErrorCode);
        AssertReadbackIdentity(produced.Value!, opened.Value!);
        await AssertExternalRoundTrip(f, reader, produced.Value!);
        Assert.Equal(f.CaseCount * f.Context.NoisePolicyConfig.RepeatCount, f.Client.Calls);
        Assert.Equal(0, modelHooks);
        Assert.Empty(durableHooks);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Theory]
    [InlineData("missing-lock")]
    [InlineData("missing-claim")]
    [InlineData("missing-baseline")]
    [InlineData("empty-history")]
    [InlineData("torn-claim")]
    [InlineData("commit-hash")]
    [InlineData("commit-length")]
    [InlineData("corrupt-baseline")]
    [InlineData("noncanonical-baseline")]
    [InlineData("parent")]
    [InlineData("config")]
    [InlineData("observed-at")]
    [InlineData("truncated-baseline")]
    [InlineData("truncated-claim")]
    [InlineData("oversized-claim")]
    public async Task ExistingOnly_OwnedSingleMutationRefusesExactStateWithoutEffects(string mutation)
    {
        using var f = new Fixture();
        var control = await f.Produce();
        Assert.True(control.IsSuccess, control.ErrorCode);
        Assert.True((await OpenByReference(f, new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(),
            control.Value!.Reference)).IsSuccess);
        var claimLines = File.ReadAllLines(f.Address.ClaimPath);
        var committed = JsonNode.Parse(claimLines[1])!.AsObject();
        var expectedCode = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode;
        switch (mutation)
        {
            case "missing-lock": File.Delete(f.Address.LockPath); break;
            case "missing-claim": File.Delete(f.Address.ClaimPath); break;
            case "missing-baseline": File.Delete(f.Address.BaselinePath); break;
            case "empty-history":
                File.Delete(f.Address.ClaimPath);
                File.Delete(f.Address.BaselinePath);
                expectedCode = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode;
                break;
            case "torn-claim":
                File.WriteAllText(f.Address.ClaimPath, claimLines[0] + "\n" + claimLines[1][..^1],
                    new UTF8Encoding(false));
                break;
            case "truncated-baseline":
                var truncated = File.ReadAllBytes(f.Address.BaselinePath);
                File.WriteAllBytes(f.Address.BaselinePath, truncated[..(truncated.Length / 2)]);
                break;
            case "truncated-claim":
                File.WriteAllText(f.Address.ClaimPath, claimLines[0] + "\n", new UTF8Encoding(false));
                break;
            case "oversized-claim":
                File.AppendAllText(f.Address.ClaimPath, new string(' ', 256 * 1024));
                break;
            case "commit-hash":
                committed["BaselineSha256"] = new string('0', 64);
                WriteCommittedClaim(f, claimLines[0], committed);
                break;
            case "commit-length":
                committed["BaselineByteLength"] = control.Value!.ByteLength + 1;
                WriteCommittedClaim(f, claimLines[0], committed);
                break;
            case "corrupt-baseline":
            case "noncanonical-baseline":
                var bytes = mutation == "corrupt-baseline" ? "{"u8.ToArray()
                    : File.ReadAllBytes(f.Address.BaselinePath).Concat(new byte[] { (byte)' ' }).ToArray();
                File.WriteAllBytes(f.Address.BaselinePath, bytes);
                // Preserve/rebind checksum and length so this mutation reaches the parser itself.
                committed["BaselineSha256"] = Hash(bytes);
                committed["BaselineByteLength"] = bytes.LongLength;
                WriteCommittedClaim(f, claimLines[0], committed);
                break;
            case "parent":
                f.Grant = f.Grant with { ParentManifestSha256 = new string('0', 64) };
                expectedCode = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode;
                break;
            case "config":
                f.Config = f.Config with { RopeTheta = 20000 };
                expectedCode = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.InvalidErrorCode;
                break;
            case "observed-at":
                f.Context = f.Context with { ObservedAtUtc = f.Context.ObservedAtUtc.AddSeconds(1) };
                // This valid input must reach the request-hash guard, not context preflight.
                Assert.True(CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.ValidateContext(f.Context).IsSuccess);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(mutation));
        }
        var storage = SnapshotFiles(f);
        var modelHooks = 0;
        var durableHooks = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(
            _ => modelHooks++, durableHooks.Add);
        var result = await OpenExisting(f, reader);
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        var byReference = await OpenByReference(f, reader, control.Value!.Reference);
        Assert.False(byReference.IsSuccess);
        Assert.Null(byReference.Value);
        Assert.Equal(result.ErrorCode, byReference.ErrorCode);
        var byExternal = await OpenByExternalReference(f, reader, f.Owner.GetExternalReference(control.Value!).Value!);
        Assert.False(byExternal.IsSuccess);
        Assert.Null(byExternal.Value);
        Assert.Equal(result.ErrorCode, byExternal.ErrorCode);
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(0, modelHooks);
        Assert.Empty(durableHooks);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Fact]
    public async Task ExistingOnly_ExistingDirectoryCustodyRefusalDoesNotRepairPermissions()
    {
        if (!OperatingSystem.IsLinux())
        {
            output.WriteLine("NOT_APPLICABLE: Linux UnixFileMode mutation; this test does not prove Windows DACL behavior.");
            return;
        }
        using var f = new Fixture();
        Assert.True((await f.Produce()).IsSuccess);
        var produced = await OpenExisting(f, f.Owner);
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        Assert.True((await OpenByReference(f, new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(),
            produced.Value!.Reference)).IsSuccess);
        var originalMode = File.GetUnixFileMode(f.Address.BaselineRoot);
        var mutatedMode = originalMode | UnixFileMode.GroupRead;
        File.SetUnixFileMode(f.Address.BaselineRoot, mutatedMode);
        var storage = SnapshotFiles(f);
        var modelHooks = 0;
        var durableHooks = new List<string>();
        try
        {
            var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(
                _ => modelHooks++, durableHooks.Add);
            var result = await OpenExisting(f, reader);
            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, result.ErrorCode);
            var byReference = await OpenByReference(f, reader, produced.Value!.Reference);
            Assert.False(byReference.IsSuccess);
            Assert.Null(byReference.Value);
            Assert.Equal(result.ErrorCode, byReference.ErrorCode);
            var byExternal = await OpenByExternalReference(f, reader, f.Owner.GetExternalReference(produced.Value!).Value!);
            Assert.False(byExternal.IsSuccess);
            Assert.Null(byExternal.Value);
            Assert.Equal(result.ErrorCode, byExternal.ErrorCode);
            Assert.Equal(mutatedMode, File.GetUnixFileMode(f.Address.BaselineRoot));
            Assert.Equal(10, f.Client.Calls);
            Assert.Equal(0, modelHooks);
            Assert.Empty(durableHooks);
            AssertFilesUnchanged(f, storage);
            Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
            Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
        }
        finally { File.SetUnixFileMode(f.Address.BaselineRoot, originalMode); }
    }

    [Theory]
    [InlineData("Started")]
    [InlineData("BaselinePersisted")]
    [InlineData("Committed")]
    public async Task ExistingOnly_ActualInterruptionReopensOnlyCompletedCommitWithoutNewMeasurement(string point)
    {
        var producerPoints = new List<string>();
        using var f = new Fixture(durablePoint: step =>
        {
            producerPoints.Add(step);
            if (step == point) throw new IOException("scripted lost return");
        });
        var interrupted = await f.Produce();
        Assert.False(interrupted.IsSuccess);
        Assert.Null(interrupted.Value);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, interrupted.ErrorCode);
        var expectedCalls = point == "Started" ? 0 : 10;
        Assert.Equal(expectedCalls, f.Client.Calls);
        var storage = SnapshotFiles(f);
        var beforePoints = producerPoints.ToArray();
        var modelHooks = 0;
        var durableHooks = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(
            _ => modelHooks++, durableHooks.Add);
        var opened = await OpenExisting(f, reader);
        Assert.Equal(point == "Committed", opened.IsSuccess);
        if (point == "Committed")
        {
            Assert.True(reader.IsIssuedReadback(opened.Value));
            Assert.Equal(DurableBaselineGraph(f), JsonSerializer.Serialize(opened.Value!.Baseline));
            Assert.Equal(Hash(File.ReadAllBytes(f.Address.BaselinePath)), opened.Value.Sha256);
            var byReference = await OpenByReference(f, reader, opened.Value.Reference);
            Assert.True(byReference.IsSuccess, byReference.ErrorCode);
            AssertReadbackIdentity(opened.Value, byReference.Value!);
            Assert.Equal(DurableBaselineGraph(f), JsonSerializer.Serialize(byReference.Value!.Baseline));
            Assert.True(reader.IsIssuedReadback(byReference.Value));
            var external = reader.GetExternalReference(opened.Value).Value!;
            var externalRead = await OpenByExternalReference(f, reader, external);
            Assert.True(externalRead.IsSuccess, externalRead.ErrorCode);
            AssertReadbackIdentity(opened.Value, externalRead.Value!);
            Assert.Equal(DurableBaselineGraph(f), JsonSerializer.Serialize(externalRead.Value!.Baseline));
        }
        else
        {
            Assert.Null(opened.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, opened.ErrorCode);
            // No completed reference exists; these are explicitly untrusted placeholders.
            var byReference = await OpenByReference(f, reader, ProposedReference(f));
            Assert.False(byReference.IsSuccess);
            Assert.Null(byReference.Value);
            Assert.Equal(opened.ErrorCode, byReference.ErrorCode);
            // Actual Started/interrupted history has no issued completed projection; use only valid untrusted syntax.
            var externalRead = await OpenByExternalReference(f, reader, ProposedExternalReference());
            Assert.False(externalRead.IsSuccess);
            Assert.Null(externalRead.Value);
            Assert.Equal(opened.ErrorCode, externalRead.ErrorCode);
        }
        // Original true producer retains its idempotent completed/uncertain behavior.
        f.Owner = reader;
        var producerReopen = await f.Produce();
        Assert.Equal(point == "Committed", producerReopen.IsSuccess);
        if (point == "Committed") AssertReadbackIdentity(opened.Value!, producerReopen.Value!);
        else
        {
            Assert.Null(producerReopen.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, producerReopen.ErrorCode);
        }
        Assert.Equal(expectedCalls, f.Client.Calls);
        Assert.Equal(beforePoints, producerPoints);
        Assert.Equal(0, modelHooks);
        Assert.Empty(durableHooks);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Fact]
    public async Task ExistingOnly_ConcurrentReaderReturnsBusyAndAlwaysReleasesOwnedProducer()
    {
        using var f = new Fixture();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.BeforeResponse = async () => { entered.TrySetResult(); await resume.Task; };
        var modelHooks = 0;
        var durableHooks = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(
            _ => modelHooks++, durableHooks.Add);
        var first = f.Produce();
        OperationResult<CodeDocumentationPreparedBaselineReadback>? completed = null;
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var calls = f.Client.Calls;
            var claim = File.ReadAllBytes(f.Address.ClaimPath);
            var names = Directory.GetFiles(f.Address.BaselineRoot).Order(StringComparer.Ordinal).ToArray();
            var accessible = Directory.GetFiles(f.RepositoryRoot, "*", SearchOption.AllDirectories)
                .Where(path => path != f.Address.LockPath).ToDictionary(path => path, File.ReadAllBytes);
            var busy = await OpenExisting(f, reader);
            Assert.False(busy.IsSuccess);
            Assert.Null(busy.Value);
            Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.BusyErrorCode, busy.ErrorCode);
            var byReference = await OpenByReference(f, reader, ProposedReference(f));
            Assert.False(byReference.IsSuccess);
            Assert.Null(byReference.Value);
            Assert.Equal(busy.ErrorCode, byReference.ErrorCode);
            var byExternal = await OpenByExternalReference(f, reader, ProposedExternalReference());
            Assert.False(byExternal.IsSuccess);
            Assert.Null(byExternal.Value);
            Assert.Equal(busy.ErrorCode, byExternal.ErrorCode);
            foreach (var entry in accessible) Assert.Equal(entry.Value, File.ReadAllBytes(entry.Key));
            Assert.Equal(calls, f.Client.Calls);
            Assert.Equal(claim, File.ReadAllBytes(f.Address.ClaimPath));
            Assert.Equal(names, Directory.GetFiles(f.Address.BaselineRoot).Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(0, modelHooks);
            Assert.Empty(durableHooks);
            Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
        }
        finally
        {
            resume.TrySetResult();
            // Even an assertion/timeout failure releases and awaits this actual owned producer.
            completed = await first;
        }
        Assert.True(completed.IsSuccess, completed.ErrorCode);
        Assert.Equal(10, f.Client.Calls);
        // Snapshot lock bytes only after the producer's FileShare.None handle is disposed.
        var storage = SnapshotFiles(f);
        var reopened = await OpenExisting(f, reader);
        Assert.True(reopened.IsSuccess, reopened.ErrorCode);
        AssertReadbackIdentity(completed.Value!, reopened.Value!);
        var byReferenceReopen = await OpenByReference(f, reader, completed.Value!.Reference);
        Assert.True(byReferenceReopen.IsSuccess, byReferenceReopen.ErrorCode);
        AssertReadbackIdentity(completed.Value, byReferenceReopen.Value!);
        Assert.Equal(DurableBaselineGraph(f), JsonSerializer.Serialize(byReferenceReopen.Value!.Baseline));
        await AssertExternalRoundTrip(f, reader, completed.Value!);
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(0, modelHooks);
        Assert.Empty(durableHooks);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
    }

    [Fact]
    public async Task ExistingOnly_NewApiRuntimeReportsLoadedNormalArtifacts()
    {
        using var f = new Fixture();
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode);
        var storage = SnapshotFiles(f);
        var opened = await OpenExisting(f, new CodeDocumentationRepositoryOwnedPreparedBaselineJournal());
        Assert.True(opened.IsSuccess, opened.ErrorCode);
        AssertReadbackIdentity(produced.Value!, opened.Value!);
        Assert.Equal(10, f.Client.Calls);
        AssertFilesUnchanged(f, storage);
        var tests = typeof(CodeDocumentationPreparedBaselineTests).Assembly.Location;
        foreach (var path in new[]
        {
            typeof(CodeDocumentationRepositoryOwnedPreparedBaselineJournal).Assembly.Location,
            tests, Path.ChangeExtension(tests, ".runtimeconfig.json"), Path.ChangeExtension(tests, ".deps.json"),
        })
        {
            var bytes = File.ReadAllBytes(path);
            output.WriteLine("S04_EXISTING_ONLY_RUNTIME " + JsonSerializer.Serialize(new
                { path, byteLength = bytes.LongLength, sha256 = Hash(bytes) }));
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ActualCalibrationRoundtrip_ReopensCompleteOwnerBytesWithoutRemeasurement(
        bool adapters, bool sufficient)
    {
        using var f = new Fixture(adapters, sufficient);
        var first = await f.Produce();
        Assert.True(first.IsSuccess, first.ErrorCode + ": " + first.ErrorMessage);
        var readback = first.Value!;
        Assert.Equal(f.CaseCount * 5, f.Client.Calls);
        Assert.True(f.Owner.IsIssuedReadback(readback));
        Assert.Equal(sufficient ? "NO_TRAINING_NEEDED" : "NEEDS_LESSON", readback.Classification);
        var bytes = File.ReadAllBytes(readback.Address.BaselinePath);
        Assert.Equal(Hash(bytes), readback.Sha256);
        Assert.Equal(bytes.Length, readback.ByteLength);
        Assert.Equal(5, readback.Baseline.Calibration.Calibration.Observations.Count);
        Assert.All(readback.Baseline.Calibration.Calibration.Observations, observation =>
        {
            Assert.Equal(f.CaseCount, observation.CaseIds.Count);
            Assert.Equal(f.CaseCount, observation.StudentAnswerSha256.Count);
            Assert.Equal(f.CaseCount, observation.ObservedEvaluationRequestIds.Count);
        });
        Assert.Equal(f.CaseCount * 5, readback.Baseline.Calibration.Calibration.Observations
            .SelectMany(o => o.ObservedEvaluationRequestIds).Distinct().Count());
        var reopened = await f.Reopen(readback);
        Assert.True(reopened.IsSuccess, reopened.ErrorCode + ": " + reopened.ErrorMessage);
        Assert.Equal(JsonSerializer.Serialize(readback.Baseline), JsonSerializer.Serialize(reopened.Value!.Baseline));
        Assert.Equal(readback.Sha256, reopened.Value.Sha256);
        Assert.Equal(readback.ByteLength, reopened.Value.ByteLength);
        Assert.Equal(f.CaseCount * 5, f.Client.Calls);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
        Assert.DoesNotContain(Directory.EnumerateDirectories(f.RepositoryRoot, "attempts", SearchOption.AllDirectories), _ => true);
    }

    [Theory]
    [InlineData("context")]
    [InlineData("corpus")]
    [InlineData("manifest")]
    [InlineData("tokenizer")]
    [InlineData("client")]
    [InlineData("evaluator")]
    [InlineData("generation")]
    [InlineData("noise")]
    [InlineData("duration-zero")]
    [InlineData("duration-negative")]
    [InlineData("observed-default")]
    [InlineData("observed-non-utc")]
    [InlineData("refs-null")]
    [InlineData("refs-empty")]
    [InlineData("refs-count")]
    [InlineData("ref-null")]
    [InlineData("ref-whitespace")]
    [InlineData("ref-trim")]
    [InlineData("ref-control")]
    [InlineData("ref-length")]
    [InlineData("ref-absolute")]
    [InlineData("ref-backslash")]
    [InlineData("ref-mount")]
    [InlineData("ref-file-uri")]
    [InlineData("ref-drive-root")]
    public async Task InvalidQualityContext_FirstAdmissionLeavesNoHistory_ThenSameObjectiveProducesOnce(
        string mutation)
    {
        var durablePoints = new List<string>();
        using var f = new Fixture(durablePoint: durablePoints.Add);
        var validContext = f.Context;
        var objectiveId = f.Grant.ObjectiveId;
        f.Context = mutation switch
        {
            "context" => null!,
            "corpus" => validContext with { Corpus = null! },
            "manifest" => validContext with { CorpusManifest = null! },
            "tokenizer" => validContext with { Tokenizer = null! },
            "client" => validContext with { EvaluationClient = null! },
            "evaluator" => validContext with { EvaluatorConfiguration = null! },
            "generation" => validContext with { GenerationSettings = null! },
            "noise" => validContext with { NoisePolicyConfig = null! },
            "duration-zero" => validContext with { MaximumDuration = TimeSpan.Zero },
            "duration-negative" => validContext with { MaximumDuration = TimeSpan.FromTicks(-1) },
            "observed-default" => validContext with { ObservedAtUtc = default },
            "observed-non-utc" => validContext with { ObservedAtUtc = validContext.ObservedAtUtc.ToOffset(TimeSpan.FromHours(2)) },
            "refs-null" => validContext with { EvidenceRefs = null! },
            "refs-empty" => validContext with { EvidenceRefs = [] },
            "refs-count" => validContext with { EvidenceRefs = Enumerable.Repeat("test:valid", 129).ToArray() },
            "ref-null" => validContext with { EvidenceRefs = [null!] },
            "ref-whitespace" => validContext with { EvidenceRefs = [" "] },
            "ref-trim" => validContext with { EvidenceRefs = [" test:valid"] },
            "ref-control" => validContext with { EvidenceRefs = ["test:\nvalid"] },
            "ref-length" => validContext with { EvidenceRefs = [new string('a', 513)] },
            "ref-absolute" => validContext with { EvidenceRefs = ["/absolute/evidence"] },
            "ref-backslash" => validContext with { EvidenceRefs = ["test:\\evidence"] },
            "ref-mount" => validContext with { EvidenceRefs = ["test:/MNT/c/evidence"] },
            "ref-file-uri" => validContext with { EvidenceRefs = ["test:FILE://evidence"] },
            "ref-drive-root" => validContext with { EvidenceRefs = ["test:C:/evidence"] },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        };

        var rejected = await f.Produce();
        Assert.False(rejected.IsSuccess);
        Assert.Equal(0, f.Client.Calls);
        Assert.True(rejected.ErrorCode ==
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.QualityGateInvalidErrorCode,
            $"mutation={mutation}; admission={rejected.ErrorCode}; evaluatorCalls={f.Client.Calls}; " +
            $"durablePoints={string.Join(',', durablePoints)}; " +
            $"ownerRoot={Directory.Exists(f.Address.BaselineRoot)}; " +
            $"claim={File.Exists(f.Address.ClaimPath)}; baseline={File.Exists(f.Address.BaselinePath)}");
        Assert.Empty(durablePoints);
        Assert.Null(f.LoadedModel);
        Assert.False(Directory.Exists(f.Address.BaselineRoot));
        Assert.False(File.Exists(f.Address.ClaimPath));
        Assert.False(File.Exists(f.Address.BaselinePath));
        Assert.False(File.Exists(f.Address.LockPath));

        f.Context = validContext;
        Assert.Equal(objectiveId, f.Grant.ObjectiveId);
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode + ": " + produced.ErrorMessage);
        var expectedCalls = f.CaseCount * validContext.NoisePolicyConfig.RepeatCount;
        Assert.Equal(expectedCalls, f.Client.Calls);
        Assert.Equal(new[] { "Started", "BaselinePersisted", "Committed" }, durablePoints);
        var claim = File.ReadAllBytes(f.Address.ClaimPath);
        var baseline = File.ReadAllBytes(f.Address.BaselinePath);

        f.Owner = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(durablePoint: durablePoints.Add);
        var reopened = await f.Produce();
        Assert.True(reopened.IsSuccess, reopened.ErrorCode + ": " + reopened.ErrorMessage);
        Assert.Equal(produced.Value!.RequestSha256, reopened.Value!.RequestSha256);
        Assert.Equal(produced.Value.Sha256, reopened.Value.Sha256);
        Assert.Equal(expectedCalls, f.Client.Calls);
        Assert.Equal(new[] { "Started", "BaselinePersisted", "Committed" }, durablePoints);
        Assert.Equal(claim, File.ReadAllBytes(f.Address.ClaimPath));
        Assert.Equal(baseline, File.ReadAllBytes(f.Address.BaselinePath));
    }

    [Theory]
    [InlineData("Started")]
    [InlineData("BaselinePersisted")]
    [InlineData("Committed")]
    public async Task DurableInterruption_NeverRetriesUncertainMeasurement(string point)
    {
        using var f = new Fixture(durablePoint: step =>
        { if (step == point) throw new IOException("scripted lost return"); });
        var interrupted = await f.Produce();
        Assert.False(interrupted.IsSuccess);
        var calls = f.Client.Calls;
        Assert.Equal(point == "Started" ? 0 : 10, calls);
        var claimBefore = File.ReadAllBytes(f.Address.ClaimPath);
        f.Owner = new();
        var reopened = await f.Produce();
        Assert.Equal(point == "Committed", reopened.IsSuccess);
        if (point != "Committed") Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, reopened.ErrorCode);
        Assert.Equal(calls, f.Client.Calls);
        Assert.Equal(claimBefore, File.ReadAllBytes(f.Address.ClaimPath));
    }

    [Theory]
    [InlineData("baseline-byte")]
    [InlineData("claim-hash")]
    [InlineData("claim-length")]
    [InlineData("torn-claim")]
    [InlineData("unknown-property")]
    [InlineData("null-property")]
    [InlineData("duplicate-property")]
    [InlineData("case-order")]
    [InlineData("missing-id")]
    [InlineData("duplicate-id")]
    [InlineData("model-identity")]
    [InlineData("model-before")]
    [InlineData("size-cap-excess")]
    public async Task SinglePersistedMutation_RejectsWithoutAnotherEvaluatorCall(string mutation)
    {
        using var f = new Fixture();
        var control = await f.Produce();
        Assert.True(control.IsSuccess, control.ErrorCode);
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        Assert.True((await OpenByReference(f, reader, control.Value!.Reference)).IsSuccess);
        var path = f.Address.BaselinePath;
        var root = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var claimLines = File.ReadAllLines(f.Address.ClaimPath);
        var committed = JsonNode.Parse(claimLines[1])!.AsObject();
        if (mutation == "baseline-byte") File.AppendAllText(path, " ");
        else if (mutation == "size-cap-excess") File.AppendAllText(path,
            new string(' ', CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes));
        else if (mutation == "claim-hash") committed["BaselineSha256"] = new string('0', 64);
        else if (mutation == "claim-length") committed["BaselineByteLength"] = control.Value!.ByteLength + 1;
        else if (mutation == "torn-claim") File.WriteAllText(f.Address.ClaimPath, claimLines[0] + "\n" + claimLines[1][..^1]);
        else
        {
            var observations = root["Baseline"]!["Calibration"]!["Calibration"]!["Observations"]!.AsArray();
            if (mutation == "unknown-property") root["surprise"] = 1;
            if (mutation == "null-property") root["Baseline"] = null;
            if (mutation == "case-order")
            {
                var ids = observations[0]!["CaseIds"]!.AsArray();
                var a = ids[0]!.GetValue<string>(); ids[0] = ids[1]!.GetValue<string>(); ids[1] = a;
            }
            if (mutation == "missing-id") observations[0]!["ObservedEvaluationRequestIds"]!.AsArray().RemoveAt(0);
            if (mutation == "duplicate-id") observations[1]!["ObservedEvaluationRequestIds"]![0] =
                observations[0]!["ObservedEvaluationRequestIds"]![0]!.GetValue<string>();
            if (mutation == "model-identity") root["Parent"]!["ManifestSha256"] = new string('0', 64);
            if (mutation == "model-before") observations[0]!["ModelSerializationBeforeSha256"] = new string('0', 64);
            var text = root.ToJsonString();
            if (mutation == "duplicate-property") text = "{\"FormatVersion\":1," + text[1..];
            File.WriteAllText(path, text, new UTF8Encoding(false));
            // Rebind the untrusted copied claim to reach the typed/completeness guard.
            committed["BaselineSha256"] = Hash(File.ReadAllBytes(path));
            committed["BaselineByteLength"] = new FileInfo(path).Length;
        }
        if (mutation != "torn-claim") File.WriteAllText(f.Address.ClaimPath,
            claimLines[0] + "\n" + committed.ToJsonString() + "\n", new UTF8Encoding(false));
        var bytesBefore = File.ReadAllBytes(path);
        var result = await f.Reopen(control.Value!);
        Assert.False(result.IsSuccess);
        var mutatedStorage = SnapshotFiles(f);
        var lower = await OpenExisting(f, reader);
        var byReference = await OpenByReference(f, reader, control.Value!.Reference);
        Assert.False(lower.IsSuccess);
        Assert.False(byReference.IsSuccess);
        Assert.Null(byReference.Value);
        Assert.Equal(lower.ErrorCode, byReference.ErrorCode);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.RecoveryRequiredErrorCode, byReference.ErrorCode);
        var byExternal = await OpenByExternalReference(f, reader, f.Owner.GetExternalReference(control.Value!).Value!);
        Assert.False(byExternal.IsSuccess);
        Assert.Null(byExternal.Value);
        Assert.Equal(lower.ErrorCode, byExternal.ErrorCode);
        AssertFilesUnchanged(f, mutatedStorage);
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(bytesBefore, File.ReadAllBytes(path));
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
    }

    [Theory]
    [InlineData("parent")]
    [InlineData("weights")]
    [InlineData("config")]
    [InlineData("generation")]
    [InlineData("evaluator")]
    [InlineData("noise")]
    [InlineData("corpus")]
    [InlineData("N")]
    [InlineData("R")]
    [InlineData("tokenizer")]
    public async Task SingleRequestMutation_RejectsBeforeMeasurementOrReopenEffects(string mutation)
    {
        using var f = new Fixture();
        var control = await f.Produce();
        Assert.True(control.IsSuccess, control.ErrorCode);
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        Assert.True((await OpenByReference(f, reader, control.Value!.Reference)).IsSuccess);
        switch (mutation)
        {
            case "parent": f.Grant = f.Grant with { ParentManifestSha256 = new string('0', 64) }; break;
            case "weights": f.Grant = f.Grant with { ParentWeightsSha256 = new string('0', 64) }; break;
            case "config": f.Config = f.Config with { RopeTheta = 20000 }; break;
            case "generation": f.Context = f.Context with { GenerationSettings = f.Context.GenerationSettings with { Seed = 18 } }; break;
            case "evaluator": f.Context = f.Context with { EvaluatorConfiguration = Teacher(21) }; break;
            case "noise": f.Context = f.Context with { NoisePolicyConfig = new(5, 3) }; break;
            case "corpus": f.Context = f.Context with { CorpusManifest = f.Context.CorpusManifest with { ManifestSha256 = new string('0', 64) } }; break;
            case "N": f.Grant = f.Grant with { DescriptorsV2 = f.Grant.DescriptorsV2! with { CaseCount = 1 } }; break;
            case "R": f.Grant = f.Grant with { DescriptorsV2 = f.Grant.DescriptorsV2! with { RepeatCount = 4 } }; break;
            case "tokenizer": var replacement = new BpeTokenizer(); replacement.Train("other tokenizer", 257); f.Context = f.Context with { Tokenizer = replacement }; break;
        }
        var result = await f.Produce();
        Assert.False(result.IsSuccess);
        var storage = SnapshotFiles(f);
        var lower = await OpenExisting(f, reader);
        var byReference = await OpenByReference(f, reader, control.Value!.Reference);
        Assert.False(lower.IsSuccess);
        Assert.False(byReference.IsSuccess);
        Assert.Null(byReference.Value);
        Assert.Equal(lower.ErrorCode, byReference.ErrorCode);
        var byExternal = await OpenByExternalReference(f, reader, f.Owner.GetExternalReference(control.Value!).Value!);
        Assert.False(byExternal.IsSuccess);
        Assert.Null(byExternal.Value);
        Assert.Equal(lower.ErrorCode, byExternal.ErrorCode);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(10, f.Client.Calls);
    }

    [Theory]
    [InlineData(20, true)]
    [InlineData(19, false)]
    [InlineData(10001, false)]
    public async Task ProtectedCatalogAllowance_IsCheckedBeforeFirstCall(int allowance, bool admitted)
    {
        using var f = new Fixture();
        f.Allowance = allowance;
        var result = await f.Produce();
        Assert.Equal(admitted, result.IsSuccess);
        Assert.Equal(admitted ? 10 : 0, f.Client.Calls);
        if (!admitted) Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.BudgetErrorCode, result.ErrorCode);
    }

    [Fact]
    public async Task CopiedCorpusHashWithShortenedEntries_CannotBypassActualOwnerQuota()
    {
        using var f = new Fixture();
        typeof(FrozenReferenceCorpus).GetField("<Entries>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(f.Context.Corpus, f.Context.Corpus.Entries.Take(1).ToArray());
        f.Grant = f.Grant with { DescriptorsV2 = f.Grant.DescriptorsV2! with { CaseCount = 1 } };
        f.Allowance = 10;
        var result = await f.Produce();
        Assert.False(result.IsSuccess);
        Assert.Equal(0, f.Client.Calls);
        Assert.False(File.Exists(f.Address.ClaimPath));
    }

    [Theory]
    [InlineData(false, "success")]
    [InlineData(true, "success")]
    [InlineData(false, "cancel")]
    [InlineData(true, "cancel")]
    [InlineData(false, "failure")]
    [InlineData(true, "failure")]
    public async Task AllParameterAndSeededAdapterState_IsolatedOnEveryExit(bool adapters, string exit)
    {
        using var f = new Fixture(adapters);
        using var cancellation = new CancellationTokenSource();
        f.Client.OnCall = () =>
        {
            Assert.True(f.LoadedModel!.TryGetTarget(out var model));
            Assert.Equal(f.LoadedState, f.Capture(model!));
            if (exit == "cancel") cancellation.Cancel();
            if (exit == "failure") throw new IOException("scripted evaluator failure");
        };
        var result = await f.Produce(cancellation.Token);
        Assert.Equal(exit == "success", result.IsSuccess);
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        if (f.LoadedModel!.TryGetTarget(out var loaded)) Assert.Equal(f.LoadedState, f.Capture(loaded));
        f.Client.OnCall = null;
        loaded = null;
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Assert.False(f.LoadedModel.TryGetTarget(out _));
    }

    [Fact]
    public async Task PublicDtosAndCopiedIdentity_CannotMintOwnerReadback()
    {
        using var f = new Fixture();
        Assert.Empty(typeof(CodeDocumentationPreparedBaselineReadback).GetConstructors());
        var control = await f.Produce();
        Assert.True(control.IsSuccess, control.ErrorCode);
        var r = control.Value!;
        var forged = new CodeDocumentationPreparedBaselineReadback(new object(), r.Address,
            r.RequestSha256, r.Sha256, r.ByteLength, r.Baseline, true);
        Assert.False(f.Owner.IsIssuedReadback(forged));
        Assert.False((await f.Reopen(forged)).IsSuccess);
        var callerGraph = (double[])r.Baseline.Calibration.Calibration.MeasurementNoisePassport.RepeatedScores;
        callerGraph[0] = 1;
        var reopened = await f.Reopen(r);
        Assert.True(reopened.IsSuccess, reopened.ErrorCode);
        Assert.Equal(2, reopened.Value!.Baseline.Calibration.Calibration.Observations[0].ObservedEvaluationRequestIds.Count);
        Assert.Equal(10, f.Client.Calls);
    }

    [Fact]
    public async Task NullIssuedReadback_CannotEnterFreshProductionBranch()
    {
        using var f = new Fixture();
        Assert.False((await f.Reopen(null!)).IsSuccess);
        Assert.Equal(0, f.Client.Calls);
        Assert.False(File.Exists(f.Address.ClaimPath));
    }

    [Theory]
    [InlineData("Sha256")]
    [InlineData("ByteLength")]
    [InlineData("RequestSha256")]
    public async Task ChangedIssuedIdentity_IsRejectedAgainstDurableBytes(string field)
    {
        using var f = new Fixture();
        var control = await f.Produce();
        Assert.True(control.IsSuccess, control.ErrorCode);
        var capability = control.Value!;
        typeof(CodeDocumentationPreparedBaselineReadback).GetField("<" + field + ">k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(capability,
                field == "ByteLength" ? (object)(capability.ByteLength + 1) : new string('0', 64));
        Assert.False((await f.Reopen(capability)).IsSuccess);
        Assert.Equal(10, f.Client.Calls);
    }

    [Fact]
    public async Task UnsupportedAdapterAuthority_FailsBeforeMeasurement()
    {
        using var f = new Fixture(adapters: true);
        f.Seeds = null;
        Assert.False((await f.Produce()).IsSuccess);
        Assert.Equal(0, f.Client.Calls);
        Assert.False(File.Exists(f.Address.ClaimPath));
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
    }

    [Fact]
    public async Task ReparseBaselinePath_IsRejectedWithoutRemeasurement()
    {
        using var f = new Fixture();
        var control = await f.Produce();
        Assert.True(control.IsSuccess, control.ErrorCode);
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal();
        Assert.True((await OpenByReference(f, reader, control.Value!.Reference)).IsSuccess);
        var retained = f.Address.BaselinePath + ".retained";
        File.Move(f.Address.BaselinePath, retained);
        File.CreateSymbolicLink(f.Address.BaselinePath, retained);
        Assert.False((await f.Reopen(control.Value!)).IsSuccess);
        var storage = SnapshotFiles(f);
        var lower = await OpenExisting(f, reader);
        var byReference = await OpenByReference(f, reader, control.Value!.Reference);
        Assert.False(byReference.IsSuccess);
        Assert.Null(byReference.Value);
        Assert.Equal(lower.ErrorCode, byReference.ErrorCode);
        var byExternal = await OpenByExternalReference(f, reader, f.Owner.GetExternalReference(control.Value!).Value!);
        Assert.False(byExternal.IsSuccess);
        Assert.Null(byExternal.Value);
        Assert.Equal(lower.ErrorCode, byExternal.ErrorCode);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(10, f.Client.Calls);
        Assert.Equal(control.Value!.Sha256, Hash(File.ReadAllBytes(retained)));
    }

    [Fact]
    public async Task IssuedReopenWithMissingFiles_DoesNotProduceAgain()
    {
        using var f = new Fixture();
        var control = await f.Produce();
        Assert.True(control.IsSuccess, control.ErrorCode);
        File.Delete(f.Address.BaselinePath); File.Delete(f.Address.ClaimPath);
        Assert.False((await f.Reopen(control.Value!)).IsSuccess);
        Assert.Equal(10, f.Client.Calls);
        Assert.False(File.Exists(f.Address.ClaimPath));
    }

    [Fact]
    public async Task ConcurrentObjectiveClaim_OneProducerMeasures()
    {
        using var f = new Fixture();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.BeforeResponse = async () => { entered.TrySetResult(); await resume.Task; };
        var first = f.Produce();
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var second = await f.Produce();
        Assert.False(second.IsSuccess);
        Assert.Equal(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.BusyErrorCode, second.ErrorCode);
        resume.SetResult();
        Assert.True((await first).IsSuccess);
        Assert.Equal(10, f.Client.Calls);
    }

    [Fact]
    public void CompleteSerializationPreflight_HasFiniteEscapedStringBoundWithoutCapWidening()
    {
        Assert.Equal(16 * 1024 * 1024, CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes);
        Assert.True(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.FitsCompleteRecord(
            Enumerable.Repeat(new string('"', 71), 200).ToArray(), 25));
        Assert.False(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.FitsCompleteRecord(
            Enumerable.Repeat(new string('"', 256), 200).ToArray(), 25));
        Assert.False(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.FitsCompleteRecord(["case"], int.MaxValue));
        Assert.False(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.FitsCompleteRecord(new string[201], 2));
        var invalid = RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(Path.GetTempPath(),
            "../escape", "pattern", "model", "v1", new string('a', 64));
        Assert.False(invalid.IsSuccess);
    }

    [Fact]
    public async Task MaximumAdmittedSeries_PersistsAll5000EscapedRequestIdsAndReopensWithoutCalls()
    {
        using var f = new Fixture(caseCount: 200, repeats: 25, escapedIds: true);
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode + ": " + produced.ErrorMessage);
        var baseline = produced.Value!;
        Assert.Equal(5000, f.Client.Calls);
        Assert.Equal(5000, baseline.Baseline.ObservedEvaluationCallCount);
        Assert.Equal(25, baseline.Baseline.Calibration.Calibration.Observations.Count);
        Assert.All(baseline.Baseline.Calibration.Calibration.Observations, observation =>
        {
            Assert.Equal(200, observation.CaseIds.Count);
            Assert.Equal(200, observation.StudentAnswerSha256.Count);
            Assert.Equal(200, observation.ObservedEvaluationRequestIds.Count);
            Assert.All(observation.ObservedEvaluationRequestIds, id => Assert.Equal(256, id.Length));
        });
        Assert.InRange(baseline.ByteLength, 7_000_000,
            CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes);
        Assert.True((await f.Reopen(baseline)).IsSuccess);
        Assert.Equal(5000, f.Client.Calls);
        Assert.Equal(baseline.Sha256, Hash(File.ReadAllBytes(f.Address.BaselinePath)));
        // Reuse this one 200x25 calibration; a fresh existing-only owner adds zero calls.
        var consumerStorage = ConsumerSetupSnapshot(f);
        var storage = SnapshotFiles(f);
        var graph = DurableBaselineGraph(f);
        var modelHooks = 0;
        var durableHooks = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(
            _ => modelHooks++, durableHooks.Add);
        var opened = await OpenExisting(f, reader);
        Assert.True(opened.IsSuccess, opened.ErrorCode);
        AssertReadbackIdentity(baseline, opened.Value!);
        Assert.Equal(graph, JsonSerializer.Serialize(opened.Value!.Baseline));
        Assert.True(reader.IsIssuedReadback(opened.Value));
        Assert.False(f.Owner.IsIssuedReadback(opened.Value));
        var issued = await ReopenIssued(f, reader, opened.Value);
        Assert.True(issued.IsSuccess, issued.ErrorCode);
        AssertReadbackIdentity(baseline, issued.Value!);
        Assert.Equal(graph, JsonSerializer.Serialize(issued.Value!.Baseline));
        // The application consumer reuses this same one 200x25 measurement.
        var byReference = await OpenByReference(f, reader, baseline.Reference);
        Assert.True(byReference.IsSuccess, byReference.ErrorCode);
        AssertReadbackIdentity(baseline, byReference.Value!);
        Assert.True(reader.IsIssuedReadback(byReference.Value));
        Assert.Equal(graph, JsonSerializer.Serialize(byReference.Value!.Baseline));
        Assert.Equal(25, byReference.Value.Baseline.Calibration.Calibration.Observations.Count);
        Assert.All(byReference.Value.Baseline.Calibration.Calibration.Observations, observation =>
        {
            Assert.Equal(200, observation.CaseIds.Count);
            Assert.Equal(200, observation.StudentAnswerSha256.Count);
            Assert.Equal(200, observation.ObservedEvaluationRequestIds.Count);
            Assert.All(observation.ObservedEvaluationRequestIds, id => Assert.Equal(256, id.Length));
        });
        Assert.Equal(5000, byReference.Value.Baseline.Calibration.Calibration.Observations
            .SelectMany(o => o.ObservedEvaluationRequestIds).Distinct(StringComparer.Ordinal).Count());
        Assert.InRange(byReference.Value.ByteLength, 7_000_000,
            CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes);
        var externalRead = await AssertExternalRoundTrip(f, reader, baseline);
        AssertRepeatHeavyGraph(externalRead, f.Context.Corpus.Entries.OrderBy(e => e.EntryId, StringComparer.Ordinal)
            .Select(e => e.EntryId).ToArray(), 25);
        output.WriteLine("S04_EXTERNAL_REFERENCE_MAXIMUM " + JsonSerializer.Serialize(new { cases = 200, repeats = 25,
            actualUtf8Bytes = externalRead.ByteLength, requestIds = 5000, additionalReaderEvaluationCalls = f.Client.Calls - 5000 }));
        // Test-local projection only: no persisted pair association or new durable format.
        var projection = Encoding.UTF8.GetBytes(ReferenceProjection(byReference.Value.Reference));
        Assert.InRange(projection.Length, 1, 2 * 1024 * 1024 - 1);
        using (var document = JsonDocument.Parse(projection))
        {
            Assert.Equal(12, document.RootElement.EnumerateObject().Count());
            Assert.All(document.RootElement.EnumerateObject(), property =>
                Assert.Contains(property.Value.ValueKind, new[] { JsonValueKind.String, JsonValueKind.Number }));
        }
        Assert.Empty(Directory.EnumerateDirectories(f.RepositoryRoot, "attempts", SearchOption.AllDirectories));
        AssertFilesUnchanged(f, storage);
        using (var lease = ConsumerLease(f))
        {
            var consumed = await Consume(f, lease.Session, ConsumerCatalog(f));
            Assert.True(consumed.IsSuccess, consumed.ErrorCode);
            AssertReadbackIdentity(baseline, consumed.Value!);
            Assert.Equal(graph, JsonSerializer.Serialize(consumed.Value!.Baseline));
            Assert.Equal(25, consumed.Value.Baseline.Calibration.Calibration.Observations.Count);
            Assert.All(consumed.Value.Baseline.Calibration.Calibration.Observations, observation =>
            {
                Assert.Equal(200, observation.CaseIds.Count);
                Assert.Equal(200, observation.StudentAnswerSha256.Count);
                Assert.Equal(200, observation.ObservedEvaluationRequestIds.Count);
                Assert.All(observation.ObservedEvaluationRequestIds, id => Assert.Equal(256, id.Length));
            });
            Assert.Equal(5000, consumed.Value.Baseline.Calibration.Calibration.Observations
                .SelectMany(o => o.ObservedEvaluationRequestIds).Distinct(StringComparer.Ordinal).Count());
            Assert.InRange(consumed.Value.ByteLength, 7_000_000,
                CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes);
            AssertHeldConsumerFiles(f, lease.Session, consumerStorage);
        }
        AssertFilesUnchanged(f, consumerStorage);
        Assert.Equal(5000, f.Client.Calls);
        Assert.Equal(0, modelHooks);
        Assert.Empty(durableHooks);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
        await AssertPreparedPairRoundTrip(f, baseline);
    }

    [Theory]
    [InlineData(1, 5000)]
    [InlineData(2, 2500)]
    public async Task RepeatHeavy5000Series_OriginalProducerPersistsCompleteGraphAndAllReadersAddNoCalls(
        int cases, int repeats)
    {
        using var f = new Fixture(caseCount: cases, repeats: repeats, escapedIds: true);
        var caseIds = f.Context.Corpus.Entries.OrderBy(e => e.EntryId, StringComparer.Ordinal)
            .Select(e => e.EntryId).ToArray();
        var oldUpperBound = 256L * 1024 + 5000L * (6L * (256 + caseIds.Max(id => id.Length)) + 256) +
            repeats * (6L * 256 + 1024) + cases * (6L * (1024 + 256) + 1024) +
            4L * 128 * (6 * 512 + 32);
        output.WriteLine("S04_RANGE_ADMISSION " + JsonSerializer.Serialize(new
        {
            cases, repeats, oldUpperBound,
            upperBoundBytes = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes(caseIds, repeats),
            admitted = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.FitsCompleteRecord(caseIds, repeats),
            maximumBytes = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes,
        }));
        // One real production per dimension; the existing 200x25 Fact remains separate.
        var produced = await f.Produce();
        Assert.True(produced.IsSuccess, produced.ErrorCode + ": " + produced.ErrorMessage);
        var baseline = produced.Value!;
        Assert.Equal(5000, f.Client.Calls);
        Assert.Equal(5000, baseline.Baseline.ExpectedEvaluationCallCount);
        Assert.Equal(5000, baseline.Baseline.ObservedEvaluationCallCount);
        var raw = File.ReadAllBytes(f.Address.BaselinePath);
        Assert.Equal(raw.LongLength, baseline.ByteLength);
        Assert.Equal(Hash(raw), baseline.Sha256);
        Assert.InRange(raw.LongLength, 1, CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes);
        var upperBound = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes(caseIds, repeats);
        Assert.NotNull(upperBound);
        Assert.InRange(raw.LongLength, 1, upperBound.Value);
        AssertRepeatHeavyGraph(baseline, caseIds, repeats);
        var graph = DurableBaselineGraph(f);
        var storage = ConsumerSetupSnapshot(f);
        var sameOwner = await f.Reopen(baseline);
        Assert.True(sameOwner.IsSuccess, sameOwner.ErrorCode);
        AssertReadbackIdentity(baseline, sameOwner.Value!);
        Assert.Equal(graph, JsonSerializer.Serialize(sameOwner.Value!.Baseline));
        var modelHooks = 0;
        var durableHooks = new List<string>();
        var reader = new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(_ => modelHooks++, durableHooks.Add);
        var fresh = await OpenExisting(f, reader);
        Assert.True(fresh.IsSuccess, fresh.ErrorCode);
        Assert.True(reader.IsIssuedReadback(fresh.Value));
        Assert.False(f.Owner.IsIssuedReadback(fresh.Value));
        var byReference = await OpenByReference(f, reader, baseline.Reference);
        Assert.True(byReference.IsSuccess, byReference.ErrorCode);
        var externalRead = await AssertExternalRoundTrip(f, reader, baseline);
        foreach (var readback in new[] { fresh.Value!, byReference.Value!, externalRead })
        {
            AssertReadbackIdentity(baseline, readback);
            Assert.Equal(graph, JsonSerializer.Serialize(readback.Baseline));
            AssertRepeatHeavyGraph(readback, caseIds, repeats);
        }
        using (var lease = ConsumerLease(f))
        {
            var consumed = await Consume(f, lease.Session, ConsumerCatalog(f));
            Assert.True(consumed.IsSuccess, consumed.ErrorCode);
            AssertReadbackIdentity(baseline, consumed.Value!);
            Assert.Equal(graph, JsonSerializer.Serialize(consumed.Value!.Baseline));
            AssertRepeatHeavyGraph(consumed.Value, caseIds, repeats);
            AssertHeldConsumerFiles(f, lease.Session, storage);
        }
        Assert.Equal(5000, f.Client.Calls);
        Assert.Equal(0, modelHooks);
        Assert.Empty(durableHooks);
        AssertFilesUnchanged(f, storage);
        Assert.Equal(raw, File.ReadAllBytes(f.Address.BaselinePath));
        Assert.Equal(f.ParentBytes, File.ReadAllBytes(f.ParentWeightsPath));
        Assert.Equal(f.CallerState, f.Capture(f.CallerModel));
        await AssertPreparedPairRoundTrip(f, baseline);
        output.WriteLine("S04_RANGE_COMPLETE " + JsonSerializer.Serialize(new
        {
            cases, repeats, upperBoundBytes = upperBound.Value, actualUtf8Bytes = raw.LongLength, baseline.Sha256,
            syntheticProductionEvaluationCalls = f.Client.Calls, additionalReaderEvaluationCalls = f.Client.Calls - 5000,
            observations = baseline.Baseline.Calibration.Calibration.Observations.Count,
            uniqueRequestIds = baseline.Baseline.Calibration.Calibration.Observations
                .SelectMany(o => o.ObservedEvaluationRequestIds).Distinct(StringComparer.Ordinal).Count(),
            unchangedRawBytes = raw.AsSpan().SequenceEqual(File.ReadAllBytes(f.Address.BaselinePath)),
        }));
    }

    [Fact]
    public void CompleteSerializationPreflight_RepeatEnvelopeUsesDerivedAsciiRunIdsAndFiniteTypedJson()
    {
        var ids = Enumerable.Repeat(new string('"', 71), 2).ToArray();
        var one = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes(ids[..1], 5000);
        var two = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes(ids, 2500);
        Assert.NotNull(one);
        Assert.NotNull(two);
        Assert.InRange(one.Value, 1, CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes);
        Assert.InRange(two.Value, 1, CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes);
        Assert.True(one > two);
        Assert.True(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.FitsCompleteRecord(ids[..1], 2));
        Assert.Null(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes([], 2));
        Assert.Null(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes(["case"], 1));
        Assert.Null(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes(["case"], 5001));
        Assert.Null(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes(["case"], int.MaxValue));
        Assert.Null(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes([new string('x', 257)], 2));
        Assert.Null(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CompleteRecordUpperBoundBytes(new string[201], int.MaxValue));
        output.WriteLine("S04_RANGE_FINITE_BOUND " + JsonSerializer.Serialize(new { one, two,
            quoteEscapedCaseChars = 71, requestIdChars = 256,
            maximumBytes = CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes }));
    }

    private static void AssertRepeatHeavyGraph(CodeDocumentationPreparedBaselineReadback readback,
        IReadOnlyList<string> caseIds, int repeats)
    {
        var calibration = readback.Baseline.Calibration;
        var report = calibration.Calibration;
        Assert.Equal(repeats, report.RepeatCount);
        Assert.Equal(repeats, report.Observations.Count);
        Assert.Equal(repeats, report.MeasurementNoisePassport.RepeatedScores.Count);
        Assert.Equal(caseIds.Count, report.HeldOutCaseCount);
        Assert.Equal(caseIds, calibration.RepresentativeAssessment.Cases.Select(c => c.CaseId));
        var baseRunId = "prepared-baseline:" + readback.Address.ObjectiveSha256;
        for (var repeat = 0; repeat < repeats; repeat++)
        {
            var observation = report.Observations[repeat];
            Assert.Equal(baseRunId + "-repeat-" + (repeat + 1).ToString("D3"), observation.AssessmentRunId);
            Assert.Equal(caseIds, observation.CaseIds);
            Assert.Equal(caseIds.Count, observation.StudentAnswerSha256.Count);
            Assert.All(observation.StudentAnswerSha256, hash => Assert.Matches("^[0-9a-f]{64}$", hash));
            Assert.Equal(caseIds.Count, observation.ObservedEvaluationRequestIds.Count);
            for (var index = 0; index < caseIds.Count; index++)
            {
                var requestId = observation.ObservedEvaluationRequestIds[index];
                Assert.Equal(256, requestId.Length);
                Assert.Equal("prepared-escaped-" + new string('"', 235) +
                    (repeat * caseIds.Count + index + 1).ToString("D4"), requestId);
            }
        }
        Assert.Equal(5000, report.Observations.SelectMany(o => o.ObservedEvaluationRequestIds)
            .Distinct(StringComparer.Ordinal).Count());
    }

    private static CodeDocumentationOneLessonGrantCatalog ConsumerCatalog(Fixture f)
        => CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([f.Grant], f.Allowance);

    private static CodeDocumentationOneLessonSessionBinding ConsumerBinding(Fixture f)
        => new(f.Grant.ObjectiveId, f.Grant.GrantId, f.Grant.GrantVersion, f.Grant.TrainingRunId,
            f.Grant.CandidateSha256, f.Grant.ParentManifestSha256, f.Grant.ParentWeightsSha256,
            f.Grant.ConfigurationSha256, f.Grant.ExamConditionSha256)
            { FormatVersion = 2, DescriptorsV2 = f.Grant.DescriptorsV2! with { } };

    private static RepositoryOwnedModelArtifactRoot ConsumerRoot(Fixture f)
        => RepositoryOwnedTrainingArtifactLayout.ResolveModelArtifactRoot(f.RepositoryRoot).Value!;

    private static CodeDocumentationRepositoryOwnedModelCheckpointStartRequest ConsumerStart(Fixture f)
        => new(f.RepositoryRoot, "csharp", "code-documentation", "prepared-model", "v1", f.Config, f.Runtime,
            RepositoryOwnedIncrementalTrainingColdStartPolicy.Denied, LoraConfig: f.Adapter, AdapterSeedAuthority: f.Seeds);

    private static CodeDocumentationOneLessonSessionOpen ConsumerLease(Fixture f)
    {
        var opened = CodeDocumentationOneLessonSession.OpenCapacityValidationV2(ConsumerRoot(f), ConsumerBinding(f), f.Allowance);
        Assert.True(opened.IsSuccess, opened.ErrorCode + ": " + opened.ErrorMessage);
        Assert.NotNull(opened.Value);
        return opened.Value;
    }

    private static Dictionary<string, byte[]> ConsumerSetupSnapshot(Fixture f)
    {
        // C2: close setup before reading its exclusive lock; the main owner
        // reacquires this same lock, binding and root without setup byte changes.
        using (var setup = ConsumerLease(f)) Assert.True(setup.Session.ReadCapacityPrefixV2().IsSuccess);
        return SnapshotFiles(f);
    }

    private static string ConsumerHistoryPath(Fixture f)
        => Path.Combine(ConsumerRoot(f).ModelArtifactRoot, "code-documentation-one-lesson-sessions", H(f.Grant.ObjectiveId), "session.jsonl");

    private static string ConsumerLockPath(Fixture f)
        => Path.Combine(Path.GetDirectoryName(ConsumerHistoryPath(f))!, ".owner.lock");

    private static void AssertHeldConsumerFiles(Fixture f, CodeDocumentationOneLessonSession session,
        Dictionary<string, byte[]> expected, string? expectedPrefixError = null)
    {
        var paths = Directory.GetFiles(f.RepositoryRoot, "*", SearchOption.AllDirectories);
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), paths.Select(p => Path.GetRelativePath(f.RepositoryRoot, p)).Order(StringComparer.Ordinal));
        var heldLock = ConsumerLockPath(f);
        Assert.Contains(Path.GetRelativePath(f.RepositoryRoot, heldLock), expected.Keys);
        foreach (var path in paths)
            if (path != heldLock) Assert.Equal(expected[Path.GetRelativePath(f.RepositoryRoot, path)], File.ReadAllBytes(path));
        var prefix = session.ReadCapacityPrefixV2();
        if (expectedPrefixError is null) Assert.True(prefix.IsSuccess);
        else
        {
            Assert.False(prefix.IsSuccess);
            Assert.Equal(expectedPrefixError, prefix.ErrorCode);
        }
    }

    private static Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> Consume(Fixture f,
        CodeDocumentationOneLessonSession session, CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest? start = null, CancellationToken token = default)
        => session.ConsumePreparedBaselineAsync(catalog, new(f.Grant.ObjectiveId, f.Grant.GrantSelector), start ?? ConsumerStart(f), f.Context, token);

    private static byte[] TokenizerBytes(BpeTokenizer tokenizer)
    {
        using var stream = new MemoryStream(); tokenizer.Save(stream); return stream.ToArray();
    }

    private static byte[] ConsumerGenesis(Fixture f)
    {
        var b = ConsumerBinding(f);
        using var document = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(new
        {
            Kind = "session", FormatVersion = 2, SessionId = H(b.ObjectiveId),
            BindingV2 = new { b.ObjectiveId, b.GrantId, b.GrantVersion, b.TrainingRunId, b.CandidateSha256,
                b.ParentManifestSha256, b.ParentWeightsSha256, b.ConfigurationSha256, b.ExamConditionSha256, b.DescriptorsV2 },
            BindingSha256 = CodeDocumentationOneLessonSession.ComputeBindingSha256(b), Revision = "0", PreviousFrameSha256 = "",
            Effects = new { BaselineDispatched = "0", BaselineCompleted = "0", TeachingDispatched = "0", TeachingCompleted = "0",
                EvaluatorDispatched = "0", EvaluatorCompleted = "0", OptimizerEntries = "0", EffectOutcomeUnknown = false },
        }));
        return CodeDocumentationOneLessonV2Wire.CanonicalBytes(document.RootElement).Concat("\n"u8.ToArray()).ToArray();
    }

    private static Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> OpenExisting(
        Fixture f, CodeDocumentationRepositoryOwnedPreparedBaselineJournal reader)
        => reader.OpenExistingAsync(CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([f.Grant], f.Allowance),
            new(f.Grant.ObjectiveId, f.Grant.GrantSelector), f.RepositoryRoot,
            "csharp", "code-documentation", "prepared-model", "v1",
            f.Config, f.Runtime, f.Context, f.Adapter, f.Seeds);

    private static Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> OpenByReference(
        Fixture f, CodeDocumentationRepositoryOwnedPreparedBaselineJournal reader,
        CodeDocumentationPreparedBaselineReference? reference)
        => reader.OpenExistingByReferenceAsync(reference, ConsumerCatalog(f),
            new(f.Grant.ObjectiveId, f.Grant.GrantSelector), f.RepositoryRoot,
            "csharp", "code-documentation", "prepared-model", "v1",
            f.Config, f.Runtime, f.Context, f.Adapter, f.Seeds);

    // Structural/untrusted expectation only; no committed result or issuer is claimed.
    private static CodeDocumentationPreparedBaselineReference ProposedReference(Fixture f)
        => new(1, f.Address.RepositoryRoot, f.Address.ObjectiveSha256, f.Address.BaselineRoot,
            f.Address.BaselineRootRelativePath, f.Address.BaselinePath, f.Address.ClaimPath,
            f.Address.LockPath, H("proposed-request"), H("proposed-raw"), 1, "NEEDS_LESSON");

    private static CodeDocumentationPreparedBaselineExternalReference ProposedExternalReference()
        => CodeDocumentationRepositoryOwnedPreparedBaselineJournal.CreateExternalReferenceExpectation(
            new string('0', 64), new string('1', 64), new string('a', 64), 1).Value!;

    private static string FlipHex(string value) => (value[0] == '0' ? "1" : "0") + value[1..];

    private static Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> OpenByExternalReference(
        Fixture f, CodeDocumentationRepositoryOwnedPreparedBaselineJournal reader,
        CodeDocumentationPreparedBaselineExternalReference? reference)
        => reader.OpenExistingByExternalReferenceAsync(reference, ConsumerCatalog(f),
            new(f.Grant.ObjectiveId, f.Grant.GrantSelector), f.RepositoryRoot,
            "csharp", "code-documentation", "prepared-model", "v1", f.Config, f.Runtime, f.Context, f.Adapter, f.Seeds);

    private static void AssertExternalRecipe(CodeDocumentationPreparedBaselineReadback baseline,
        CodeDocumentationPreparedBaselineExternalReference external, byte[] actualRaw)
    {
        Assert.Equal(actualRaw.LongLength, baseline.ByteLength);
        Assert.Equal(Hash(actualRaw), baseline.Sha256);
        Assert.InRange(actualRaw.LongLength, 1, CodeDocumentationRepositoryOwnedPreparedBaselineJournal.MaximumBaselineBytes);
        var directPreimage = "xiigen-s04-original-baseline-reference-v2\n" + baseline.Address.ObjectiveSha256 + "\n" +
            baseline.RequestSha256 + "\n" + Hash(actualRaw) + "\n" + actualRaw.LongLength.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n";
        Assert.Equal(H(directPreimage)[..32], external.ReceiptId);
        Assert.Equal(Hash(actualRaw), external.ReceiptSha256);
        using var projection = JsonDocument.Parse(JsonSerializer.Serialize(new { external.ReceiptId, external.ReceiptSha256 }));
        Assert.Equal(2, projection.RootElement.EnumerateObject().Count());
        Assert.All(projection.RootElement.EnumerateObject(), p => Assert.Equal(JsonValueKind.String, p.Value.ValueKind));
    }

    private static async Task<CodeDocumentationPreparedBaselineReadback> AssertExternalRoundTrip(Fixture f,
        CodeDocumentationRepositoryOwnedPreparedBaselineJournal reader, CodeDocumentationPreparedBaselineReadback original)
    {
        var projected = f.Owner.GetExternalReference(original);
        Assert.True(projected.IsSuccess, projected.ErrorCode);
        var external = new CodeDocumentationPreparedBaselineExternalReference(projected.Value!.ReceiptId, projected.Value.ReceiptSha256);
        AssertExternalRecipe(original, external, File.ReadAllBytes(f.Address.BaselinePath));
        var opened = await OpenByExternalReference(f, reader, external);
        Assert.True(opened.IsSuccess, opened.ErrorCode);
        AssertReadbackIdentity(original, opened.Value!);
        Assert.True(reader.IsIssuedReadback(opened.Value));
        Assert.False(f.Owner.IsIssuedReadback(opened.Value));
        Assert.Equal(DurableBaselineGraph(f), JsonSerializer.Serialize(opened.Value!.Baseline));
        var originalObservations = original.Baseline.Calibration.Calibration.Observations;
        var freshObservations = opened.Value.Baseline.Calibration.Calibration.Observations;
        Assert.Equal(originalObservations.Count, freshObservations.Count);
        for (var i = 0; i < originalObservations.Count; i++)
        {
            Assert.Equal(originalObservations[i].CaseIds, freshObservations[i].CaseIds);
            Assert.Equal(originalObservations[i].StudentAnswerSha256, freshObservations[i].StudentAnswerSha256);
            Assert.Equal(originalObservations[i].ObservedEvaluationRequestIds, freshObservations[i].ObservedEvaluationRequestIds);
        }
        return opened.Value;
    }

    private static string ReferenceProjection(CodeDocumentationPreparedBaselineReference reference)
        => JsonSerializer.Serialize(new { reference.FormatVersion, reference.RepositoryRoot,
            reference.ObjectiveSha256, reference.BaselineRoot, reference.BaselineRootRelativePath,
            reference.BaselinePath, reference.ClaimPath, reference.LockPath, reference.RequestSha256,
            reference.Sha256, reference.ByteLength, reference.Classification });

    private static Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> ReopenIssued(
        Fixture f, CodeDocumentationRepositoryOwnedPreparedBaselineJournal reader,
        CodeDocumentationPreparedBaselineReadback readback)
        => reader.ReopenIssuedAsync(readback,
            CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([f.Grant], f.Allowance),
            new(f.Grant.ObjectiveId, f.Grant.GrantSelector), f.RepositoryRoot,
            "csharp", "code-documentation", "prepared-model", "v1",
            f.Config, f.Runtime, f.Context, f.Adapter, f.Seeds);

    private static void AssertReadbackIdentity(CodeDocumentationPreparedBaselineReadback expected,
        CodeDocumentationPreparedBaselineReadback actual)
    {
        // The address is a class: compare every durable coordinate, not object reference equality.
        Assert.Equal(expected.Address.RepositoryRoot, actual.Address.RepositoryRoot);
        Assert.Equal(expected.Address.ObjectiveSha256, actual.Address.ObjectiveSha256);
        Assert.Equal(expected.Address.BaselineRoot, actual.Address.BaselineRoot);
        Assert.Equal(expected.Address.BaselineRootRelativePath, actual.Address.BaselineRootRelativePath);
        Assert.Equal(expected.Address.BaselinePath, actual.Address.BaselinePath);
        Assert.Equal(expected.Address.ClaimPath, actual.Address.ClaimPath);
        Assert.Equal(expected.Address.LockPath, actual.Address.LockPath);
        Assert.Equal(expected.RequestSha256, actual.RequestSha256);
        Assert.Equal(expected.Sha256, actual.Sha256);
        Assert.Equal(expected.ByteLength, actual.ByteLength);
        Assert.Equal(expected.Classification, actual.Classification);
    }

    private static string DurableBaselineGraph(Fixture f)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(f.Address.BaselinePath));
        var graph = document.RootElement.GetProperty("Baseline")
            .Deserialize<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality>();
        Assert.NotNull(graph);
        return JsonSerializer.Serialize(graph);
    }

    // All fixture files include exact prepared claim/baseline/lock and accepted parent manifest/weights.
    private static Dictionary<string, byte[]> SnapshotFiles(Fixture f)
        => Directory.GetFiles(f.RepositoryRoot, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(f.RepositoryRoot, path), File.ReadAllBytes,
                StringComparer.Ordinal);

    private static void AssertFilesUnchanged(Fixture f, Dictionary<string, byte[]> expected)
    {
        var actual = SnapshotFiles(f);
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), actual.Keys.Order(StringComparer.Ordinal));
        foreach (var entry in expected) Assert.Equal(entry.Value, actual[entry.Key]);
    }

    private static void WriteCommittedClaim(Fixture f, string started, JsonObject committed)
        => File.WriteAllText(f.Address.ClaimPath, started + "\n" + committed.ToJsonString() + "\n",
            new UTF8Encoding(false));

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string H(string text) => Hash(Encoding.UTF8.GetBytes(text));
    private static ContinuousLearningTp1DpoConfiguration Teacher(int allowance = 20)
        => ContinuousLearningTp1DpoConfiguration.Create("qwen-prepared-fixture:9b", new string('a', 64),
            new string('b', 64), allowance, "sha256:" + new string('c', 64),
            "sha256:" + new string('d', 64), Enumerable.Range(1, 32).Select(i => (byte)i).ToArray()).Value!;

    private sealed class Fixture : IDisposable
    {
        internal string RepositoryRoot { get; } = Path.Combine(Path.GetTempPath(), "prepared-baseline-tests", Guid.NewGuid().ToString("N"));
        internal ModelConfig Config = new() { VocabSize = 256, ContextLen = 64, HiddenDim = 8,
            NumLayers = 1, NumHeads = 1, NumKvHeads = 1, FfnHiddenDim = 16 };
        internal LoraConfig? Adapter;
        internal LoraPairRandomSeedAuthority? Seeds;
        internal LlmModel CallerModel;
        internal byte[] CallerState;
        internal WeakReference<LlmModel>? LoadedModel;
        internal byte[]? LoadedState;
        internal RepositoryOwnedModelRuntimeCompatibility Runtime;
        internal CodeDocumentationOneLessonGrantV1 Grant;
        internal CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext Context;
        internal CodeDocumentationRepositoryOwnedPreparedBaselineJournal Owner;
        internal Client Client;
        internal int Allowance = 20;
        internal int CaseCount = 2;
        internal string ParentWeightsPath;
        internal byte[] ParentBytes;
        internal RepositoryOwnedPreparedBaselineAddress Address;

        internal Fixture(bool adapters = false, bool sufficient = false, Action<string>? durablePoint = null,
            int? caseCount = null, int repeats = 5, bool escapedIds = false)
        {
            if (sufficient) { CaseCount = 100; Allowance = 1000; }
            if (caseCount is not null) { CaseCount = caseCount.Value; Allowance = checked(2 * CaseCount * repeats); }
            Directory.CreateDirectory(RepositoryRoot);
            File.WriteAllText(Path.Combine(RepositoryRoot, "LlmFromScratch.sln"), "synthetic fixture");
            Directory.CreateDirectory(Path.Combine(RepositoryRoot, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(RepositoryRoot, "11.DPO"));
            var tokenizer = new BpeTokenizer(); tokenizer.Train("Document Add method. Document Subtract method.", 256);
            using var tokenizerBytes = new MemoryStream(); tokenizer.Save(tokenizerBytes);
            Runtime = new(TrainingSourceProvenance.Identify(Config).Sha256, H("topology"), Hash(tokenizerBytes.ToArray()),
                H("trainer"), H("schema"), "prepared-fixture");
            if (adapters) { Adapter = new() { Rank = 2, Alpha = 4, Dropout = 0.2f }; Seeds = new(17, "policy", "reference"); }
            CallerModel = new(Config);
            if (Adapter is not null) LoraApplicator.ApplySeeded(CallerModel, Adapter, Seeds!.RootInitializationSeed, Seeds.PolicyDropoutStreamDomain);
            var parentAddress = RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(RepositoryRoot,
                "csharp", "code-documentation", "prepared-model", "v1", 1).Value!;
            var dpo = SealDpo(RepositoryRoot);
            var parent = RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(parentAddress,
                CallerModel.SaveCheckpointToStream, new(null, "synthetic accepted parent", dpo, Runtime,
                    DateTimeOffset.UnixEpoch, ["test:prepared-parent"]));
            Assert.True(parent.IsSuccess, parent.ErrorCode + ": " + parent.ErrorMessage);
            ParentWeightsPath = parentAddress.WeightsPath; ParentBytes = File.ReadAllBytes(ParentWeightsPath);
            SetSentinels(CallerModel); CallerState = Capture(CallerModel);
            var corpus = SealCorpus(RepositoryRoot, CaseCount);
            var teacher = Teacher(Allowance); Client = new(teacher, sufficient, escapedIds);
            Context = new(corpus.Corpus, corpus.Manifest, tokenizer, Client, teacher,
                new(1, 0, 1, 1, 17, CaseCount, 256), new MeasurementNoisePolicyConfig(repeats),
                TimeSpan.FromSeconds(caseCount is null ? 15 : 90), DateTimeOffset.UnixEpoch.AddHours(1), ["test:prepared-baseline"]);
            Grant = new("prepared-grant", "v1", "prepared-selector", "prepared-objective", "prepared-run",
                H("candidate"), parent.Value!.ManifestSha256, parent.Value.WeightsSha256,
                Runtime.ConfigSha256, H("exam"))
            { FormatVersion = 2, DescriptorsV2 = new(2, H("source"), H("parent"), H("candidate-selection"),
                H("corpus"), H("plan"), CaseCount, repeats, H("evaluator"), H("profile")) };
            Address = RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(RepositoryRoot,
                "csharp", "code-documentation", "prepared-model", "v1", H(Grant.ObjectiveId)).Value!;
            Owner = new(modelLoaded: model =>
            {
                SetSentinels(model);
                LoadedModel = new(model); LoadedState = Capture(model);
            }, durablePoint: durablePoint);
        }

        internal byte[] Capture(LlmModel model) => CodeDocumentationRepositoryOwnedPreparedBaselineJournal
            .CaptureState(model, Runtime, Adapter, Seeds);
        private void SetSentinels(LlmModel model)
        {
            var index = 0;
            foreach (var parameter in model.Parameters())
            {
                parameter.Value.RequiresGrad = ++index % 2 == 0;
                parameter.Value.Grad = index % 3 == 0 ? null : new Tensor(
                    Enumerable.Repeat(index * 0.125f, parameter.Value.Data.Length).ToArray(), parameter.Value.Shape);
            }
            if (Adapter is not null)
                foreach (var projection in new[] { "q_proj", "k_proj", "v_proj", "o_proj" })
                    if (model.GetAttachedProjectionAdapter(0, projection) is LoraLinear linear)
                        linear.Forward(new Tensor(new float[Config.HiddenDim], new TensorShape(1, Config.HiddenDim)), training: true);
        }
        internal Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> Produce(CancellationToken token = default)
            => Owner.ProduceOrOpenAsync(CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([Grant], Allowance),
                new(Grant.ObjectiveId, Grant.GrantSelector), RepositoryRoot, "csharp", "code-documentation", "prepared-model", "v1",
                Config, Runtime, Context, Adapter, Seeds, token);
        internal Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> Reopen(CodeDocumentationPreparedBaselineReadback readback)
            => Owner.ReopenIssuedAsync(readback, CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([Grant], Allowance),
                new(Grant.ObjectiveId, Grant.GrantSelector), RepositoryRoot, "csharp", "code-documentation", "prepared-model", "v1",
                Config, Runtime, Context, Adapter, Seeds);
        public void Dispose()
        {
            var root = Path.GetFullPath(RepositoryRoot);
            var allowed = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "prepared-baseline-tests")) + Path.DirectorySeparatorChar;
            if (!root.StartsWith(allowed, StringComparison.Ordinal)) throw new InvalidOperationException("Fixture cleanup escaped root.");
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreparedPair_SmallOriginalAndSeededOriginalUseDistinctCurrentLesson(bool adapters)
    {
        using var f = new Fixture(adapters: adapters);
        var original = await f.Produce();
        Assert.True(original.IsSuccess, original.ErrorCode);
        await AssertPreparedPairRoundTrip(f, original.Value!);
    }

    [Theory]
    [InlineData("original-bytes", false)]
    [InlineData("missing-original", false)]
    [InlineData("started-claim", false)]
    [InlineData("committed-claim", false)]
    [InlineData("reference-hash", false)]
    [InlineData("reference-length", false)]
    [InlineData("reference-path", false)]
    [InlineData("reference-classification", false)]
    [InlineData("objective", false)]
    [InlineData("parent", false)]
    [InlineData("conditions", false)]
    [InlineData("pair", false)]
    [InlineData("request-pair", false)]
    [InlineData("manifest", false)]
    [InlineData("identity", false)]
    [InlineData("lesson-address", false)]
    [InlineData("model", false)]
    [InlineData("supplied-state", false)]
    [InlineData("fake-root-absent-claim", false)]
    [InlineData("fake-root-bad-claim", false)]
    [InlineData("fake-root-matching-claim", false)]
    [InlineData("sufficient", false)]
    [InlineData("cancel", false)]
    [InlineData("original-bytes", true)]
    [InlineData("missing-original", true)]
    [InlineData("conditions", true)]
    [InlineData("evidence", true)]
    [InlineData("tokenizer", true)]
    [InlineData("lora-targets", true)]
    [InlineData("rng-entries", true)]
    [InlineData("optimizer-moments", true)]
    [InlineData("pair", true)]
    [InlineData("request-pair", true)]
    [InlineData("manifest", true)]
    [InlineData("model", true)]
    [InlineData("fake-root-absent-claim", true)]
    [InlineData("cancel", true)]
    public async Task PreparedPair_InitialAndBrokerMutationRefuseAtMeasuredBoundary(string mutation, bool afterBroker)
    {
        using var f = new Fixture(sufficient: mutation == "sufficient");
        var original = await f.Produce();
        Assert.True(original.IsSuccess, original.ErrorCode);
        var h = new PreparedPairHarness(f, original.Value!);
        var rngEntries = new[] { new LlmModelRandomStateEntry("fixture", "policy", 17, 23, 0) };
        AdamWPortableParameterState? optimizerEntry = null;
        if (mutation == "rng-entries") h.Request = h.Request with { RandomStateToRestore =
            new(LlmModelRandomStateSnapshot.CurrentFormatVersion, LlmModelRandomStateSnapshot.CurrentAlgorithm,
                rngEntries, H("topology"), H("lora"), 1) };
        if (mutation == "optimizer-moments")
        {
            optimizerEntry = new("fixture", [1], [0f], [0f]);
            h.Request = h.Request with { OptimizerStateToRestore =
                new(AdamWPortableState.CurrentFormatVersion, 0, [optimizerEntry]) };
        }
        var calls = f.Client.Calls;
        var policy = ModelBytes(h.Request.PolicyModel);
        var reference = ModelBytes(h.Request.ReferenceModel);
        using var cancellation = new CancellationTokenSource();
        void Mutate()
        {
            var r = h.Inputs.Reference;
            switch (mutation)
            {
                case "original-bytes": File.AppendAllText(f.Address.BaselinePath, " "); break;
                case "missing-original": File.Delete(f.Address.BaselinePath); break;
                case "started-claim": File.WriteAllText(f.Address.ClaimPath,
                    File.ReadAllText(f.Address.ClaimPath).Split('\n')[0] + "\n"); break;
                case "committed-claim": File.AppendAllText(f.Address.ClaimPath, "{}\n"); break;
                case "reference-hash":
                case "reference-length":
                case "reference-path":
                case "reference-classification":
                    h.Inputs = h.Inputs with { Reference = new(r.FormatVersion, r.RepositoryRoot, r.ObjectiveSha256,
                        r.BaselineRoot, r.BaselineRootRelativePath,
                        mutation == "reference-path" ? r.BaselinePath + ".wrong" : r.BaselinePath,
                        r.ClaimPath, r.LockPath, r.RequestSha256,
                        mutation == "reference-hash" ? H("stale-reference") : r.Sha256,
                        mutation == "reference-length" ? r.ByteLength + 1 : r.ByteLength,
                        mutation == "reference-classification" ? "NO_TRAINING_NEEDED" : r.Classification) }; break;
                case "objective": h.Inputs = h.Inputs with { Selector = new("different-objective", f.Grant.GrantSelector) }; break;
                case "parent": File.AppendAllText(f.ParentWeightsPath, "x"); break;
                case "conditions": typeof(ModelConfig).GetProperty(nameof(ModelConfig.RopeTheta))!
                    .SetValue(f.Config, f.Config.RopeTheta + 1); break;
                case "evidence": ((string[])f.Context.EvidenceRefs)[0] = "test:changed-evidence"; break;
                case "tokenizer": f.Context.Tokenizer.Train("A different tokenizer merges new words", 270); break;
                case "lora-targets": h.Request.LoraConfig.TargetModules[0] = "k_proj"; break;
                case "request-pair": ((List<ContinuousLearning.DpoPair>)h.Request.Pairs)[0] =
                    new("Changed current prompt", "changed chosen", "changed rejected", "csharp", DateTimeOffset.UnixEpoch);
                    break;
                case "rng-entries": rngEntries[0] = rngEntries[0] with { ConsumedCursor = 1 }; break;
                case "optimizer-moments":
                    // Portable moments expose a readonly wrapper; mutate its captured backing
                    // array through reflection to exercise a hostile same-process broker.
                    var wrapper = optimizerEntry!.FirstMoment;
                    var backing = typeof(System.Collections.ObjectModel.ReadOnlyCollection<float>)
                        .GetField("list", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(wrapper);
                    ((float[])backing!)[0] = 1f;
                    break;
                case "pair":
                    var pairFile = Directory.GetFiles(h.Inputs.LessonDatasetAddress.PairsRoot, "*.json").Single();
                    File.AppendAllText(pairFile, " "); break;
                case "manifest":
                    var manifest = JsonNode.Parse(File.ReadAllText(h.Inputs.LessonDatasetAddress.ManifestPath))!.AsObject();
                    manifest["manifestSha256"] = H("changed manifest");
                    File.WriteAllText(h.Inputs.LessonDatasetAddress.ManifestPath, manifest.ToJsonString()); break;
                case "identity": h.Inputs = h.Inputs with { LessonExpectedRequestIdentity =
                    h.Inputs.LessonExpectedRequestIdentity with { RequestIdentitySha256 = H("wrong identity") } }; break;
                case "lesson-address": h.Inputs = h.Inputs with { LessonDatasetAddress =
                    RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(f.RepositoryRoot,
                        "csharp", "code-documentation", "missing-lesson").Value! }; break;
                case "model": h.Request.PolicyModel.Parameters().First().Value.Data[0] += 1; break;
                case "supplied-state": h.Request = h.Request with { BaselineAttemptState =
                    new(1, "wrong", H("wrong"), "wrong", "wrong", "wrong", "wrong", "wrong", "wrong",
                        null!, "wrong", H("wrong"), H("wrong"), "wrong", H("wrong"), original.Value!.Baseline) }; break;
                case "fake-root-absent-claim":
                case "fake-root-bad-claim":
                case "fake-root-matching-claim":
                    SecureLocalFileBoundary.CreateOwnerOnlyDirectory(h.Request.ArtifactAddress.AttemptRoot);
                    if (mutation != "fake-root-absent-claim")
                    {
                        var a = h.Request.ArtifactAddress;
                        using var claim = SecureLocalFileBoundary.OpenOwnerOnlyFile(a.AttemptClaimPath,
                            FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough);
                        claim.Write(Encoding.UTF8.GetBytes(mutation == "fake-root-bad-claim" ? "bad" :
                            "training_run_id=" + a.TrainingRunId + Environment.NewLine +
                            "training_run_id_sha256=" + a.TrainingRunIdSha256 + Environment.NewLine +
                            "attempt_root=" + a.AttemptRootRelativePath + Environment.NewLine));
                    }
                    break;
                case "cancel": cancellation.Cancel(); break;
            }
        }
        if (afterBroker) h.Broker.Mutate = Mutate; else Mutate();
        var stateBefore = SnapshotFiles(f);
        var observer = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        var error = "";
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => observer++;
            if (mutation == "cancel")
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                    new CodeDocumentationServerOwnedDpoTrainingExecutor().ExecuteFromPreparedBaselineAsync(
                        h.Request, h.Inputs, cancellation.Token));
                error = "OperationCanceledException";
            }
            else
            {
                var result = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                    .ExecuteFromPreparedBaselineAsync(h.Request, h.Inputs);
                Assert.False(result.IsSuccess);
                Assert.NotNull(result.ErrorCode);
                error = result.ErrorCode!;
            }
            Assert.Equal(afterBroker ? 1 : 0, h.Broker.Calls);
            Assert.Equal(0, observer);
            Assert.Equal(calls, f.Client.Calls);
            if (afterBroker) Assert.True(h.Broker.LeaseDisposed);
            Assert.False(File.Exists(Path.Combine(h.Request.ArtifactAddress.AttemptRoot, "baseline.json")));
            if (!mutation.StartsWith("fake-root", StringComparison.Ordinal))
            {
                Assert.False(Directory.Exists(h.Request.ArtifactAddress.AttemptRoot));
                Assert.False(File.Exists(h.Request.ArtifactAddress.AttemptClaimPath));
            }
            if (!afterBroker) AssertFilesUnchanged(f, stateBefore);
            if (mutation != "model") Assert.Equal(policy, ModelBytes(h.Request.PolicyModel));
            Assert.Equal(reference, ModelBytes(h.Request.ReferenceModel));
            output.WriteLine("S04_PREPARED_BOUNDARY " + JsonSerializer.Serialize(new
            { mutation, afterBroker, error, broker = h.Broker.Calls, trainer = observer,
                evaluatorDelta = f.Client.Calls - calls, baselineCreated = false }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Theory]
    [InlineData("claim-missing")]
    [InlineData("claim-malformed")]
    [InlineData("claim-run")]
    [InlineData("claim-hash")]
    [InlineData("claim-root")]
    [InlineData("claim-bom")]
    [InlineData("claim-newline")]
    [InlineData("claim-unsafe")]
    [InlineData("root-unsafe")]
    [InlineData("baseline-unsafe")]
    [InlineData("root-moved")]
    [InlineData("claim-symlink")]
    [InlineData("original-missing")]
    [InlineData("original-changed")]
    [InlineData("original-started")]
    [InlineData("current-pair-changed")]
    public async Task PreparedPair_RecoveryRequiresActualCanonicalOwnerClaimAndFreshOriginal(string mutation)
    {
        using var f = new Fixture();
        var original = await f.Produce();
        Assert.True(original.IsSuccess, original.ErrorCode);
        var h = new PreparedPairHarness(f, original.Value!);
        var created = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
            .ExecuteFromPreparedBaselineAsync(h.Request, h.Inputs);
        Assert.Equal(RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode, created.ErrorCode);
        var a = h.Request.ArtifactAddress;
        var claim = File.ReadAllBytes(a.AttemptClaimPath);
        switch (mutation)
        {
            case "claim-missing": File.Delete(a.AttemptClaimPath); break;
            case "claim-malformed": File.WriteAllText(a.AttemptClaimPath, "bad"); break;
            case "claim-run": File.WriteAllText(a.AttemptClaimPath,
                Encoding.UTF8.GetString(claim).Replace(a.TrainingRunId, "wrong-run", StringComparison.Ordinal)); break;
            case "claim-hash": File.WriteAllText(a.AttemptClaimPath,
                Encoding.UTF8.GetString(claim).Replace(a.TrainingRunIdSha256, H("wrong claim hash"), StringComparison.Ordinal)); break;
            case "claim-root": File.WriteAllText(a.AttemptClaimPath,
                Encoding.UTF8.GetString(claim).Replace(a.AttemptRootRelativePath, "other/root", StringComparison.Ordinal)); break;
            case "claim-bom": File.WriteAllBytes(a.AttemptClaimPath, [0xef, 0xbb, 0xbf, .. claim]); break;
            case "claim-newline": File.WriteAllText(a.AttemptClaimPath,
                Encoding.UTF8.GetString(claim).Replace("\n", "\r\n", StringComparison.Ordinal)); break;
            case "claim-unsafe": if (OperatingSystem.IsLinux()) File.SetUnixFileMode(a.AttemptClaimPath,
                SecureLocalFileBoundary.OwnerFileMode | UnixFileMode.GroupRead); else File.WriteAllText(a.AttemptClaimPath, "unsafe"); break;
            case "root-unsafe": if (OperatingSystem.IsLinux()) File.SetUnixFileMode(a.AttemptRoot,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead);
                else File.WriteAllText(a.AttemptClaimPath, "unsafe"); break;
            case "baseline-unsafe": if (OperatingSystem.IsLinux()) File.SetUnixFileMode(Path.Combine(a.AttemptRoot, "baseline.json"),
                SecureLocalFileBoundary.OwnerFileMode | UnixFileMode.GroupRead);
                else File.WriteAllText(a.AttemptClaimPath, "unsafe"); break;
            case "root-moved": Directory.Move(a.AttemptRoot, a.AttemptRoot + "-moved"); break;
            case "claim-symlink":
                File.Move(a.AttemptClaimPath, a.AttemptClaimPath + ".target");
                File.CreateSymbolicLink(a.AttemptClaimPath, a.AttemptClaimPath + ".target"); break;
            case "original-missing": File.Delete(f.Address.BaselinePath); break;
            case "original-changed": File.AppendAllText(f.Address.BaselinePath, " "); break;
            case "original-started": File.WriteAllText(f.Address.ClaimPath,
                File.ReadAllText(f.Address.ClaimPath).Split('\n')[0] + "\n"); break;
            case "current-pair-changed": File.AppendAllText(
                Directory.GetFiles(h.Inputs.LessonDatasetAddress.PairsRoot, "*.json").Single(), " "); break;
        }
        var before = SnapshotFiles(f);
        var calls = f.Client.Calls;
        var observer = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => observer++;
            var opened = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenPreparedExistingAsync(a, h.Inputs);
            Assert.False(opened.IsSuccess);
            Assert.Equal(1, h.Broker.Calls);
            Assert.Equal(0, observer);
            Assert.Equal(calls, f.Client.Calls);
            AssertFilesUnchanged(f, before);
            if (OperatingSystem.IsLinux() && mutation == "claim-unsafe")
                Assert.Equal(SecureLocalFileBoundary.OwnerFileMode | UnixFileMode.GroupRead, File.GetUnixFileMode(a.AttemptClaimPath));
            if (OperatingSystem.IsLinux() && mutation == "baseline-unsafe")
                Assert.Equal(SecureLocalFileBoundary.OwnerFileMode | UnixFileMode.GroupRead,
                    File.GetUnixFileMode(Path.Combine(a.AttemptRoot, "baseline.json")));
            if (OperatingSystem.IsLinux() && mutation == "root-unsafe")
                Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead,
                    File.GetUnixFileMode(a.AttemptRoot));
            output.WriteLine("S04_PREPARED_RECOVERY_REFUSAL " + JsonSerializer.Serialize(new
            { mutation, error = opened.ErrorCode, brokerDelta = 0, trainer = observer, evaluatorDelta = f.Client.Calls - calls }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    [Theory]
    [InlineData("root-missing")]
    [InlineData("root-unknown")]
    [InlineData("root-duplicate")]
    [InlineData("root-alias")]
    [InlineData("passport-missing")]
    [InlineData("passport-unknown")]
    [InlineData("passport-duplicate")]
    [InlineData("passport-alias")]
    [InlineData("reference-missing")]
    [InlineData("reference-unknown")]
    [InlineData("reference-duplicate")]
    [InlineData("reference-alias")]
    [InlineData("null")]
    [InlineData("type")]
    [InlineData("format")]
    [InlineData("fraction")]
    [InlineData("exponent")]
    [InlineData("overflow")]
    [InlineData("hash-case")]
    [InlineData("hash-invalid")]
    [InlineData("attempt")]
    [InlineData("path")]
    [InlineData("parent")]
    [InlineData("pair")]
    [InlineData("manifest")]
    [InlineData("reference-format")]
    [InlineData("reference-length")]
    [InlineData("reference-path")]
    [InlineData("classification")]
    [InlineData("whitespace")]
    [InlineData("order")]
    [InlineData("escape")]
    [InlineData("bom")]
    [InlineData("utf16")]
    [InlineData("trailing")]
    [InlineData("empty")]
    [InlineData("at-cap")]
    [InlineData("over-cap")]
    public async Task PreparedPair_ClosedWireRefusesLiteralMutationsWithoutRepair(string mutation)
    {
        using var f = new Fixture();
        var original = await f.Produce();
        Assert.True(original.IsSuccess, original.ErrorCode);
        var h = new PreparedPairHarness(f, original.Value!);
        var created = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
            .ExecuteFromPreparedBaselineAsync(h.Request, h.Inputs);
        Assert.Equal(RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode, created.ErrorCode);
        var path = Path.Combine(h.Request.ArtifactAddress.AttemptRoot, "baseline.json");
        var literal = File.ReadAllText(path);
        var node = JsonNode.Parse(literal)!.AsObject();
        var passport = node["AcceptedModelPassport"]!.AsObject();
        var reference = node["PreparedBaselineReference"]!.AsObject();
        if (mutation.EndsWith("-missing", StringComparison.Ordinal))
        {
            (mutation.StartsWith("passport", StringComparison.Ordinal) ? passport :
                mutation.StartsWith("reference", StringComparison.Ordinal) ? reference : node)
                .Remove(mutation.StartsWith("root", StringComparison.Ordinal) ? "TrainingRunId" :
                    mutation.StartsWith("reference", StringComparison.Ordinal) ? "FormatVersion" : "DomainSlug");
        }
        else if (mutation.EndsWith("-unknown", StringComparison.Ordinal))
            (mutation.StartsWith("passport", StringComparison.Ordinal) ? passport :
                mutation.StartsWith("reference", StringComparison.Ordinal) ? reference : node)["Unknown"] = "extra";
        else if (mutation.EndsWith("-alias", StringComparison.Ordinal))
        {
            var target = mutation.StartsWith("passport", StringComparison.Ordinal) ? passport :
                mutation.StartsWith("reference", StringComparison.Ordinal) ? reference : node;
            var key = mutation.StartsWith("passport", StringComparison.Ordinal) ? "DomainSlug" : "FormatVersion";
            target[key.ToLowerInvariant()] = target[key]!.DeepClone(); target.Remove(key);
        }
        else switch (mutation)
        {
            case "null": node["TrainingRunId"] = null; break;
            case "type": node["TrainingRunId"] = 1; break;
            case "format": node["FormatVersion"] = 1; break;
            case "overflow": reference["ByteLength"] = ulong.MaxValue; break;
            case "hash-case": node["TrainingPairSha256"] = node["TrainingPairSha256"]!.GetValue<string>().ToUpperInvariant(); break;
            case "hash-invalid": node["TrainingPairSha256"] = "no hash"; break;
            case "attempt": node["TrainingRunId"] = "different-attempt"; break;
            case "path": node["TrainingPairRelativePath"] = "../wrong.json"; break;
            case "parent": passport["WeightsSha256"] = H("wrong parent"); break;
            case "pair": node["TrainingPairId"] = H("wrong pair"); break;
            case "manifest": node["DpoManifestSha256"] = H("wrong manifest"); break;
            case "reference-format": reference["FormatVersion"] = 2; break;
            case "reference-length": reference["ByteLength"] = original.Value!.ByteLength + 1; break;
            case "reference-path": reference["BaselinePath"] = "/untrusted/path.json"; break;
            case "classification": reference["Classification"] = "NO_TRAINING_NEEDED"; break;
        }
        var text = node.ToJsonString();
        switch (mutation)
        {
            case "root-duplicate": text = literal.Insert(1, "\"FormatVersion\":2,"); break;
            case "passport-duplicate": text = literal.Replace("\"AcceptedModelPassport\":{",
                "\"AcceptedModelPassport\":{\"DomainSlug\":\"csharp\",", StringComparison.Ordinal); break;
            case "reference-duplicate": text = literal.Replace("\"PreparedBaselineReference\":{",
                "\"PreparedBaselineReference\":{\"FormatVersion\":1,", StringComparison.Ordinal); break;
            case "fraction": text = literal.Replace("\"FormatVersion\":2", "\"FormatVersion\":2.0", StringComparison.Ordinal); break;
            case "exponent": text = literal.Replace("\"FormatVersion\":2", "\"FormatVersion\":2e0", StringComparison.Ordinal); break;
            case "whitespace": text = literal + "\n"; break;
            case "order": text = literal.Replace("{\"FormatVersion\":2,", "{", StringComparison.Ordinal);
                text = text[..^1] + ",\"FormatVersion\":2}"; break;
            case "escape": text = literal.Replace("csharp", "csh\\u0061rp", StringComparison.Ordinal); break;
            case "trailing": text = literal + "{}"; break;
            case "empty": text = ""; break;
            case "at-cap": text = literal + new string(' ', 2 * 1024 * 1024 - Encoding.UTF8.GetByteCount(literal)); break;
            case "over-cap": text = literal + new string(' ', 2 * 1024 * 1024 + 1 - Encoding.UTF8.GetByteCount(literal)); break;
        }
        var bytes = mutation == "utf16" ? Encoding.Unicode.GetBytes(literal) : Encoding.UTF8.GetBytes(text);
        if (mutation == "bom") bytes = [0xef, 0xbb, 0xbf, .. bytes];
        File.WriteAllBytes(path, bytes);
        var before = SnapshotFiles(f);
        var calls = f.Client.Calls;
        var opened = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
            .OpenPreparedExistingAsync(h.Request.ArtifactAddress, h.Inputs);
        Assert.False(opened.IsSuccess, mutation);
        Assert.Equal(1, h.Broker.Calls);
        Assert.Equal(calls, f.Client.Calls);
        AssertFilesUnchanged(f, before);
        output.WriteLine("S04_PREPARED_WIRE_REFUSAL " + JsonSerializer.Serialize(new
        { mutation, error = opened.ErrorCode, bytes = bytes.Length, brokerDelta = 0, evaluatorDelta = f.Client.Calls - calls }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2097152)]
    [InlineData(2097153)]
    public async Task PreparedPair_LegacyFullFormatOneRetainsActualTwoMiBSizeBoundary(int targetBytes)
    {
        using var f = new Fixture();
        var original = await f.Produce();
        Assert.True(original.IsSuccess, original.ErrorCode);
        var h = new PreparedPairHarness(f, original.Value!);
        var fresh = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
            .ValidatePreparedAsync(h.Request.ArtifactAddress, h.Inputs, CancellationToken.None);
        Assert.True(fresh.IsSuccess, fresh.ErrorCode);
        // Actual legacy executor creates this root; no fabricated positive root.
        var claimed = await new CodeDocumentationServerOwnedDpoTrainingExecutor().ExecuteAsync(h.Request);
        Assert.Equal(RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode, claimed.ErrorCode);
        var state = fresh.Value!.State with { FormatVersion = 1 };
        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = false,
            UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow };
        var originalSize = JsonSerializer.SerializeToUtf8Bytes(state, options).Length;
        if (targetBytes > 0)
        {
            var report = state.BaselineQuality.Calibration.Calibration;
            var observations = report.Observations.ToArray();
            observations[0] = observations[0] with { AssessmentRunId = observations[0].AssessmentRunId +
                new string('x', targetBytes - originalSize) };
            state = state with { BaselineQuality = state.BaselineQuality with { Calibration =
                state.BaselineQuality.Calibration with { Calibration = report with { Observations = observations } } } };
            Assert.Equal(targetBytes, JsonSerializer.SerializeToUtf8Bytes(state, options).Length);
        }
        var written = CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.WriteAndReadBack(
            h.Request.ArtifactAddress, state);
        Assert.Equal(targetBytes <= 2097152, written.IsSuccess);
        if (written.IsSuccess)
        {
            Assert.Equal(1, written.Value!.State.FormatVersion);
            Assert.True(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(h.Request.ArtifactAddress).IsSuccess);
        }
        else Assert.False(File.Exists(Path.Combine(h.Request.ArtifactAddress.AttemptRoot, "baseline.json")));
        output.WriteLine("S04_LEGACY_CAP " + JsonSerializer.Serialize(new
        { targetBytes, actualBytes = JsonSerializer.SerializeToUtf8Bytes(state, options).Length,
            success = written.IsSuccess, error = written.ErrorCode, evaluatorCalls = f.Client.Calls }));
    }

    private async Task AssertPreparedPairRoundTrip(Fixture f, CodeDocumentationPreparedBaselineReadback baseline)
    {
        var h = new PreparedPairHarness(f, baseline);
        var raw = File.ReadAllBytes(f.Address.BaselinePath);
        var graph = JsonSerializer.Serialize(baseline.Baseline);
        var calls = f.Client.Calls;
        var policy = ModelBytes(h.Request.PolicyModel);
        var reference = ModelBytes(h.Request.ReferenceModel);
        var observer = 0;
        var prior = StableDpoTrainingExecutor.LogicalRequestObserverForTesting;
        try
        {
            StableDpoTrainingExecutor.LogicalRequestObserverForTesting = _ => observer++;
            var refused = await new CodeDocumentationServerOwnedDpoTrainingExecutor()
                .ExecuteFromPreparedBaselineAsync(h.Request, h.Inputs);
            Assert.False(refused.IsSuccess);
            Assert.Equal(RepositoryOwnedDpoOptimizerStateFile.InvalidErrorCode, refused.ErrorCode);
            Assert.Equal(1, h.Broker.Calls);
            Assert.NotNull(h.Broker.Content);
            Assert.True(h.Broker.ContentBound);
            Assert.True(h.Broker.LeaseDisposed);
            Assert.True(Directory.Exists(h.Request.ArtifactAddress.AttemptRoot));
            Assert.True(File.Exists(h.Request.ArtifactAddress.AttemptClaimPath));
            var reopened = await CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenPreparedExistingAsync(h.Request.ArtifactAddress, h.Inputs);
            Assert.True(reopened.IsSuccess, reopened.ErrorCode + ": " + reopened.ErrorMessage);
            var state = reopened.Value!.State;
            Assert.Equal(2, state.FormatVersion);
            Assert.Equal(graph, JsonSerializer.Serialize(state.BaselineQuality));
            var compact = File.ReadAllBytes(reopened.Value.BaselineStatePath);
            Assert.InRange(compact.Length, 1, 2 * 1024 * 1024 - 1);
            Assert.Equal(compact.LongLength, reopened.Value.BaselineStateByteLength);
            Assert.Equal(Hash(compact), reopened.Value.BaselineStateSha256);
            Assert.Equal(ExpectedCompactLiteral(state, baseline.Reference), Encoding.UTF8.GetString(compact));
            Assert.False(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal
                .OpenExisting(h.Request.ArtifactAddress).IsSuccess);
            using var wire = JsonDocument.Parse(compact);
            Assert.Equal(12, wire.RootElement.GetProperty("PreparedBaselineReference").EnumerateObject().Count());
            Assert.DoesNotContain("BaselineQuality", Encoding.UTF8.GetString(compact));
            var freshOriginal = await OpenByReference(f,
                new CodeDocumentationRepositoryOwnedPreparedBaselineJournal(), baseline.Reference);
            Assert.True(freshOriginal.IsSuccess, freshOriginal.ErrorCode);
            Assert.Equal(graph, JsonSerializer.Serialize(freshOriginal.Value!.Baseline));
            var beforeObservations = baseline.Baseline.Calibration.Calibration.Observations;
            var afterObservations = state.BaselineQuality.Calibration.Calibration.Observations;
            Assert.Equal(beforeObservations.Count, afterObservations.Count);
            for (var i = 0; i < beforeObservations.Count; i++)
            {
                Assert.Equal(beforeObservations[i].CaseIds, afterObservations[i].CaseIds);
                Assert.Equal(beforeObservations[i].StudentAnswerSha256, afterObservations[i].StudentAnswerSha256);
                Assert.Equal(beforeObservations[i].ObservedEvaluationRequestIds,
                    afterObservations[i].ObservedEvaluationRequestIds);
            }
            Assert.Equal(raw, File.ReadAllBytes(f.Address.BaselinePath));
            Assert.Equal(baseline.Sha256, Hash(raw));
            Assert.Equal(baseline.ByteLength, raw.LongLength);
            Assert.Equal(policy, ModelBytes(h.Request.PolicyModel));
            Assert.Equal(reference, ModelBytes(h.Request.ReferenceModel));
            Assert.Equal(0, observer);
            Assert.Equal(calls, f.Client.Calls);
            output.WriteLine("S04_PREPARED_PAIR_RECOVERY " + JsonSerializer.Serialize(new
            {
                originalBytes = raw.LongLength, originalSha256 = baseline.Sha256,
                compactBytes = compact.LongLength, compactSha256 = Hash(compact),
                fullGraphExact = graph == JsonSerializer.Serialize(state.BaselineQuality),
                observations = afterObservations.Count, ids = afterObservations.Sum(o => o.ObservedEvaluationRequestIds.Count),
                broker = h.Broker.Calls, trainer = observer, evaluatorDelta = f.Client.Calls - calls,
                error = refused.ErrorCode, leaseDisposed = h.Broker.LeaseDisposed,
                scope = "storage/provenance/full recovery; existing invalid optimizer refuses before trainer",
            }));
        }
        finally { StableDpoTrainingExecutor.LogicalRequestObserverForTesting = prior; }
    }

    private static byte[] ModelBytes(LlmModel model)
    {
        using var stream = new MemoryStream();
        model.SaveCheckpointToStream(stream);
        return stream.ToArray();
    }

    // Independent literal contract, not a call to the production Utf8JsonWriter.
    private static string ExpectedCompactLiteral(CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptState s,
        CodeDocumentationPreparedBaselineReference r)
    {
        static string Q(string value) => JsonSerializer.Serialize(value);
        var p = s.AcceptedModelPassport;
        return "{\"FormatVersion\":2,\"TrainingRunId\":" + Q(s.TrainingRunId) +
            ",\"TrainingRunIdSha256\":" + Q(s.TrainingRunIdSha256) +
            ",\"AttemptRootRelativePath\":" + Q(s.AttemptRootRelativePath) +
            ",\"AttemptClaimRelativePath\":" + Q(s.AttemptClaimRelativePath) +
            ",\"DomainSlug\":" + Q(s.DomainSlug) + ",\"DesignPatternSlug\":" + Q(s.DesignPatternSlug) +
            ",\"ModelIdSlug\":" + Q(s.ModelIdSlug) + ",\"ModelVersion\":" + Q(s.ModelVersion) +
            ",\"AcceptedModelPassport\":{\"DomainSlug\":" + Q(p.DomainSlug) +
            ",\"DesignPatternSlug\":" + Q(p.DesignPatternSlug) + ",\"ModelIdSlug\":" + Q(p.ModelIdSlug) +
            ",\"ModelVersion\":" + Q(p.ModelVersion) + ",\"CheckpointStep\":" + p.CheckpointStep +
            ",\"RepositoryRelativeManifestPath\":" + Q(p.RepositoryRelativeManifestPath) +
            ",\"ManifestSha256\":" + Q(p.ManifestSha256) +
            ",\"RepositoryRelativeWeightsPath\":" + Q(p.RepositoryRelativeWeightsPath) +
            ",\"WeightsSha256\":" + Q(p.WeightsSha256) + ",\"WeightsByteLength\":" + p.WeightsByteLength +
            ",\"MaximumModelSerializationBytes\":" + p.MaximumModelSerializationBytes +
            ",\"ModelSourceKind\":" + Q(p.ModelSourceKind) +
            ",\"RepositoryRelativeModelIdentityPath\":" + Q(p.RepositoryRelativeModelIdentityPath) +
            ",\"ModelIdentitySha256\":" + Q(p.ModelIdentitySha256) + "},\"DpoManifestRelativePath\":" +
            Q(s.DpoManifestRelativePath) + ",\"DpoManifestSha256\":" + Q(s.DpoManifestSha256) +
            ",\"TrainingPairId\":" + Q(s.TrainingPairId) + ",\"TrainingPairRelativePath\":" + Q(s.TrainingPairRelativePath) +
            ",\"TrainingPairSha256\":" + Q(s.TrainingPairSha256) +
            ",\"PreparedBaselineReference\":{\"FormatVersion\":1,\"RepositoryRoot\":" + Q(r.RepositoryRoot) +
            ",\"ObjectiveSha256\":" + Q(r.ObjectiveSha256) + ",\"BaselineRoot\":" + Q(r.BaselineRoot) +
            ",\"BaselineRootRelativePath\":" + Q(r.BaselineRootRelativePath) + ",\"BaselinePath\":" + Q(r.BaselinePath) +
            ",\"ClaimPath\":" + Q(r.ClaimPath) + ",\"LockPath\":" + Q(r.LockPath) +
            ",\"RequestSha256\":" + Q(r.RequestSha256) + ",\"Sha256\":" + Q(r.Sha256) +
            ",\"ByteLength\":" + r.ByteLength + ",\"Classification\":" + Q(r.Classification) + "}}";
    }

    private sealed class PreparedPairHarness
    {
        internal CodeDocumentationServerOwnedDpoTrainingRequest Request;
        internal CodeDocumentationPreparedBaselineAttemptInputs Inputs;
        internal readonly PreparedAcceptingBroker Broker = new();
        internal PreparedPairHarness(Fixture f, CodeDocumentationPreparedBaselineReadback original)
        {
            f.Context = f.Context with { EvidenceRefs = f.Context.EvidenceRefs.ToArray() };
            var lesson = SealCurrentLesson(f.RepositoryRoot);
            Inputs = new(original.Reference,
                CodeDocumentationOneLessonGrantCatalog.FromTrustedGrants([f.Grant], f.Allowance),
                new(f.Grant.ObjectiveId, f.Grant.GrantSelector), f.Config, f.Runtime, f.Context,
                f.Adapter, f.Seeds, lesson.Address, lesson.Identity);
            LlmModel FreshModel()
            {
                var model = new LlmModel(f.Config);
                if (f.Adapter is not null) LoraApplicator.ApplySeeded(model, f.Adapter,
                    f.Seeds!.RootInitializationSeed, f.Seeds.PolicyDropoutStreamDomain);
                using var checkpoint = File.OpenRead(f.ParentWeightsPath);
                model.LoadCheckpoint(checkpoint);
                return model;
            }
            var run = "prepared-pair:" + Guid.NewGuid().ToString("N");
            var address = RepositoryOwnedTrainingArtifactLayout.CreateModelTrainingAttemptAddress(
                f.RepositoryRoot, "csharp", "code-documentation", "prepared-model", "v1", run).Value!;
            Request = new(new List<ContinuousLearning.DpoPair> { lesson.Pair }, FreshModel(), FreshModel(), f.Context.Tokenizer,
                f.Adapter ?? new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] },
                address, Broker, run, 1, OptimizerStateToRestore:
                    new AdamWPortableState(AdamWPortableState.CurrentFormatVersion, 0, []));
        }
    }

    private sealed class PreparedAcceptingBroker : IServerOwnedDpoTrainingLeaseBroker
    {
        internal int Calls;
        internal Action? Mutate;
        internal DpoTrainingContentBatch? Content;
        internal ServerOwnedLegalTrainingTeacherAdmissionLease? Lease;
        internal bool ContentBound;
        internal bool LeaseDisposed => Lease is not null && !Lease.AcquireMutationPermit().IsSuccess;
        private readonly IServerOwnedDpoTrainingLeaseBroker _inner =
            (IServerOwnedDpoTrainingLeaseBroker)ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                _ => OperationResult<LegalTrainingTeacherAdmission>.Fail("Unbound call", "UNBOUND_VALIDATOR_CALLED"),
                static () => true, concurrencyObserverForTesting: null, sealReplay: null, verifyReplay: null,
                sealExecutableManifest: null, verifyExecutableManifest: null,
                validateBoundBatch: (_, _) => OperationResult<LegalTrainingTeacherAdmission>.Ok(
                    new([], 0, "policy/prepared-pair", [])));
        public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease> AcquireTranslationTrainingLease(
            DpoTrainingContentBatch content, string run)
            => OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease>.Fail("Unexpected translation", "UNEXPECTED");
        public OperationResult<ServerOwnedLegalTrainingTeacherAdmissionLease> AcquireCodeDocumentationTrainingLease(
            DpoTrainingContentBatch content, string run)
        {
            Calls++;
            Content = content;
            var admitted = _inner.AcquireCodeDocumentationTrainingLease(content, run);
            Lease = admitted.Value;
            ContentBound = Lease?.MatchesExactTrainingContent(content) == true;
            Mutate?.Invoke();
            return admitted;
        }
    }

    private static (RepositoryOwnedDpoDatasetAddress Address, RepositoryOwnedDpoDatasetRequestIdentity Identity,
        ContinuousLearning.DpoPair Pair) SealCurrentLesson(string root)
    {
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(root,
            "csharp", "code-documentation", "current-lesson-v1").Value!;
        var identity = RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity([H("current lesson paid reference")],
            new(OpenWeightTeacherFamily.Qwen.ToString(), "qwen-fixture:9b", new string('c', 64),
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind, new string('d', 64)),
            new("current-lesson", "v1", H("current generation")), new("current-lesson", "v1", H("current creator"))).Value!;
        var provenance = new TrainingTeacherProvenance(TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen, TrainingTeacherAccessMode.LocalHostedOpenWeight, TrainingTeacherHostingProvider.Ollama,
            identity.OpenTeacher.ModelId, identity.OpenTeacher.ModelVersion, TrainingTeacherModelLicense.Apache20,
            false, false, false, true, "current-lesson-pair", 1)
        { ModelArtifactDigestKind = identity.OpenTeacher.ModelArtifactDigestKind, ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256 };
        var pair = new ContinuousLearning.DpoPair("Document a new current lesson method", "Current lesson correct explanation",
            "Current lesson misses behavior", "csharp", DateTimeOffset.UnixEpoch.AddMinutes(1))
        { TeacherProvenance = provenance, GovernanceDecision = new(true, false, ["sha256:" + H("current governance")]),
            ChosenAuthorship = new(DpoPairSideAuthorKind.OpenTeacher, provenance), RejectedAuthorship = new(DpoPairSideAuthorKind.LocalModel) };
        var bound = DpoPairTrainingContentAuthority.Bind(pair);
        Assert.True(bound.IsSuccess, bound.ErrorCode);
        var seal = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(address, identity, [bound.Value!]);
        Assert.True(seal.IsSuccess, seal.ErrorCode + ": " + seal.ErrorMessage);
        return (address, identity, bound.Value!);
    }

    private static RepositoryOwnedReferenceCorpusOpenResult SealCorpus(string root, int count)
    {
        var entries = Enumerable.Range(1, count).Select(i =>
        {
            var answer = "Reference " + i;
            var provenance = PaidReferenceProvenance.Create("fixture", "closed-reference", "2026-10-02", "reference-" + i,
                DateTimeOffset.UnixEpoch, FrozenReferenceEntry.ComputeNormalizedTextSha256(answer)).Value!;
            return FrozenReferenceEntry.Create("Document Add method.", answer, provenance, "csharp", "prepared-exam",
                FrozenReferencePartition.HeldOut, "sha256:" + H("source-" + i)).Value!;
        }).ToArray();
        var corpus = FrozenReferenceCorpus.Build("prepared-fixture", "v1", DateTimeOffset.UnixEpoch, entries).Value!;
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(root, "csharp", "prepared-exam", "v1").Value!;
        var result = RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address, corpus);
        Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage);
        return RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address).Value!;
    }

    private static RepositoryOwnedDpoDatasetManifestIdentity SealDpo(string root)
    {
        var address = RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(root, "csharp", "code-documentation", "v1").Value!;
        var identity = RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity([H("paid reference")],
            new(OpenWeightTeacherFamily.Qwen.ToString(), "qwen-fixture:9b", new string('c', 64),
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind, new string('d', 64)),
            new("prepared-fixture", "v1", H("generation")), new("prepared-fixture", "v1", H("creator"))).Value!;
        var provenance = new TrainingTeacherProvenance(TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen, TrainingTeacherAccessMode.LocalHostedOpenWeight, TrainingTeacherHostingProvider.Ollama,
            identity.OpenTeacher.ModelId, identity.OpenTeacher.ModelVersion, TrainingTeacherModelLicense.Apache20,
            false, false, false, true, "synthetic-pair", 1)
        { ModelArtifactDigestKind = identity.OpenTeacher.ModelArtifactDigestKind, ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256 };
        var pair = new ContinuousLearning.DpoPair("Document method", "Correct documentation", "Vague documentation", "csharp", DateTimeOffset.UnixEpoch)
        { TeacherProvenance = provenance, GovernanceDecision = new(true, false, ["sha256:" + H("governance")]),
            ChosenAuthorship = new(DpoPairSideAuthorKind.OpenTeacher, provenance), RejectedAuthorship = new(DpoPairSideAuthorKind.LocalModel) };
        var bound = DpoPairTrainingContentAuthority.Bind(pair);
        Assert.True(bound.IsSuccess, bound.ErrorCode);
        var sealedManifest = RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(address, identity, [bound.Value!]);
        Assert.True(sealedManifest.IsSuccess, sealedManifest.ErrorCode + ": " + sealedManifest.ErrorMessage);
        return new(address.ManifestRelativePath, sealedManifest.Value!.ManifestSha256);
    }

    private sealed class Client(ContinuousLearningTp1DpoConfiguration configuration, bool pass, bool escapedIds = false)
        : ILegalTeacherEvaluationInferenceClient
    {
        internal int Calls;
        internal Action? OnCall;
        internal Func<Task>? BeforeResponse;
        public async Task<OperationResult<TeacherEvaluationInferenceResult>> EvaluateAsync(string prompt,
            ILegalTeacherCallPermit permit, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = permit.TryIssue();
            if (!issued.IsSuccess) return issued.MapError<TeacherEvaluationInferenceResult>();
            var number = Interlocked.Increment(ref Calls);
            OnCall?.Invoke();
            if (BeforeResponse is not null) await BeforeResponse();
            cancellationToken.ThrowIfCancellationRequested();
            using var input = JsonDocument.Parse(prompt[prompt.IndexOf('{')..]);
            var caseId = input.RootElement.GetProperty("caseId").GetString();
            var output = JsonSerializer.Serialize(new { caseId, pass, reason = pass ? "correct" : "missing_essential_behavior" });
            var requestId = escapedIds ? "prepared-escaped-" + new string('"', 235) + number.ToString("D4")
                : "prepared-evaluation-" + number.ToString("D4");
            var provenance = new TrainingTeacherProvenance(TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen, TrainingTeacherAccessMode.LocalHostedOpenWeight, TrainingTeacherHostingProvider.Ollama,
                configuration.ModelId, configuration.ExpectedOllamaManifestDigest, TrainingTeacherModelLicense.Apache20,
                false, false, false, false, requestId, 1)
            { ModelArtifactDigestKind = LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = configuration.ExpectedOllamaModelLayerDigest };
            return OperationResult<TeacherEvaluationInferenceResult>.Ok(new(output, provenance)
            { InferenceProfile = LocalQwenOllamaInferenceProfileAuthority.Evaluation });
        }
        public void Dispose() { }
    }
}
