// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using ContinuousLearning;
using Xunit.Abstractions;
using H = LlmFromScratch.Tests.CodeDocumentationPreparedBaselineTests;
namespace LlmFromScratch.Tests;

// Explicit MSBuild opt-in only: unavailable or denied installed resources fail these Facts rather than skipping or passing.
[Collection(RepoArtifactSnapshotCollectionNames.CollectionName)] // Serialize real resource lifetime and legacy shared source observers through the existing collection.
public sealed class CodeDocumentationConfiguredQwenNativeResourceTests(ITestOutputHelper output)
{
    private static ConfiguredQwenVocabularyLease Open()
    {
        var opened = ConfiguredQwenVocabularyLease.OpenInstalledWindows(QwenAdmissionTestSupport.Configuration(installed: true));
        Assert.True(opened.IsSuccess, opened.ErrorCode + ":" + opened.ErrorMessage);
        Assert.NotNull(opened.Value); return opened.Value!;
    }
    [Fact]
    public void NativeInstalledVocabulary_ExactVectorsAndAbi()
    {
        using var native = Open(); var configuration = QwenAdmissionTestSupport.Configuration(installed: true);
        var shape = LocalQwenOllamaTeacherInferenceClient.PreparedAuthoringRequest.Prepare(QwenAdmissionTestSupport.ShapeControlPrompt, new string('a', 64), "SyntheticVector",
            configuration, native, null, CancellationToken.None);
        Assert.True(shape.IsSuccess, shape.ErrorCode); Assert.Equal("NativeConfigured", shape.Value!.TokenizerOrigin);
        Assert.Equal(QwenAdmissionTestSupport.ShapeControlIds(), shape.Value.CopyTokenIds()); Assert.Equal(144, shape.Value.InputTokens);
        foreach (var repeats in new[] { 6128, 6129 })
        {
            var rendered = LocalQwenOllamaTeacherInferenceClient.RenderRestrictedSingleUser(QwenAdmissionTestSupport.BoundaryPrompt(repeats));
            var counted = native.Count(rendered, configuration, CancellationToken.None);
            Assert.True(counted.IsSuccess, counted.ErrorCode); Assert.Equal(QwenAdmissionTestSupport.BoundaryIds(repeats), counted.Value!.CopyIds());
        }
        var unicode = native.Count("A\u0301 e\u0301 你好 Привет １２3\r\n", configuration, CancellationToken.None);
        Assert.True(unicode.IsSuccess, unicode.ErrorCode);
        Assert.Equal(new[] { 32, 52033, 378, 52033, 220, 109266, 239029, 220, 19496, 24128, 18, 317 }, unicode.Value!.CopyIds());
        Assert.Equal("28011bbc3b771ee270290ef0727c182f5c7dd2d2ce3ff330ea1471c0cdbaaf9b", ConfiguredQwenVocabularyLease.TokenIdsSha(unicode.Value.CopyIds()));
        Assert.NotEmpty(native.ResourceFingerprint);
        output.WriteLine(native.CopyNativeDiagnostics());
        Assert.Contains("vocab only - skipping tensors", native.CopyNativeDiagnostics());
    }
    [Fact]
    public async Task HeldCompleteContext_NativePrepareAndOverflow()
    {
        using var trace = NativeResourceTestStageTrace.FromEnvironment();
        trace.Stage("fact", "start"); trace.Stage("native-open", "start");
        var native = Open(); trace.Stage("native-open", "end");
        try
        {
        var configuration = QwenAdmissionTestSupport.Configuration(installed: true);
        trace.Stage("fit-harness-create", "start");
        using (var h = H.TeacherContextHarness())
        {
            trace.Stage("fit-harness-create", "end"); trace.Stage("fit-bind", "start");
            var (b, a) = await H.BindTeacherCompleted(h); var files = h.Files(); var history = h.HistoryBytes();
            trace.Stage("fit-bind", "end"); trace.Stage("fit-prepare", "start");
            var prepared = await h.Open.Session.PrepareOrdinarySelectedTeacherRequestNativeResourcesSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector,
                a, null, H.AssessTeacherHistory, configuration, native);
            trace.Stage("fit-prepare", "end"); trace.Stage("fit-assertions", "start");
            Assert.True(prepared.IsSuccess, prepared.ErrorCode + ":" + prepared.ErrorMessage);
            Assert.Equal("NativeConfigured", prepared.Value!.Request.TokenizerOrigin); Assert.Equal("SyntheticTest", prepared.Value.Request.SourceOrigin);
            Assert.InRange(prepared.Value.Request.InputTokens, 1, 6144); Assert.Equal(2048, prepared.Value.Request.OutputReserve);
            QwenAdmissionTestSupport.AssertCompleteRendered(prepared.Value.Request.Rendered, h, b); QwenAdmissionTestSupport.AssertBody(prepared.Value.Request);
            Assert.False(prepared.Value.FullTeacherContextReady); Assert.Equal(10, h.Client.Calls); Assert.Equal(history, h.HistoryBytes()); h.AssertFiles(files);
            trace.Stage("fit-assertions", "end"); trace.Stage("fit-harness-dispose", "start");
        }
        trace.Stage("fit-harness-dispose", "end"); trace.Stage("overflow-harness-create", "start");
        using (var h = H.TeacherContextHarness(chosenHistorySuffix: string.Concat(Enumerable.Repeat("a ", 6400))))
        {
            trace.Stage("overflow-harness-create", "end"); trace.Stage("overflow-bind", "start");
            var (b, a) = await H.BindTeacherCompleted(h); var files = h.Files(); var history = h.HistoryBytes();
            trace.Stage("overflow-bind", "end"); trace.Stage("overflow-prepare", "start");
            var result = await h.Open.Session.PrepareOrdinarySelectedTeacherRequestNativeResourcesSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector,
                a, null, H.AssessTeacherHistory, configuration, native);
            trace.Stage("overflow-prepare", "end"); trace.Stage("overflow-assertions", "start");
            Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(ConfiguredQwenVocabularyLease.BudgetExceeded, result.ErrorCode);
            Assert.Equal(history, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls); Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting); h.AssertFiles(files);
            trace.Stage("overflow-assertions", "end"); trace.Stage("overflow-harness-dispose", "start");
        }
        trace.Stage("overflow-harness-dispose", "end");
        }
        finally { trace.Stage("native-dispose", "start"); native.Dispose(); trace.Stage("native-dispose", "end"); }
        trace.Stage("fact", "end");
    }
    [Fact]
    public void NativeResourceDriftAndCancellation()
    {
        using var native = Open(); var configuration = QwenAdmissionTestSupport.Configuration(installed: true);
        var mismatch = native.Count("{}", QwenAdmissionTestSupport.Configuration(), CancellationToken.None);
        Assert.False(mismatch.IsSuccess); Assert.Equal(ConfiguredQwenVocabularyLease.Mismatch, mismatch.ErrorCode);
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        Assert.Throws<OperationCanceledException>(() => native.Count("{}", configuration, canceled.Token));
        var valid = native.Count(LocalQwenOllamaTeacherInferenceClient.RenderRestrictedSingleUser("Hello"), configuration, CancellationToken.None);
        Assert.True(valid.IsSuccess, valid.ErrorCode);
        Assert.Equal(new[] { 248045, 846, 198, 9419, 248046, 198, 248045, 74455, 198, 248068, 271, 248069, 271 }, valid.Value!.CopyIds());
        Assert.True(native.ValidateCurrent(CancellationToken.None).IsSuccess);
        // Request write access without writing bytes; the retained read-only custody must deny this exact installed manifest.
        var manifest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ollama", "models", "manifests", "registry.ollama.ai", "library", "qwen3.5", "9b-q4_K_M");
        var denied = Record.Exception(() => { using var attempt = new FileStream(manifest, FileMode.Open, FileAccess.Write, FileShare.ReadWrite); });
        Assert.NotNull(denied); Assert.True(denied is IOException or UnauthorizedAccessException, denied.ToString());
        native.Dispose(); var disposed = native.Count("{}", configuration, CancellationToken.None);
        Assert.False(disposed.IsSuccess); Assert.Equal(ConfiguredQwenVocabularyLease.Unavailable, disposed.ErrorCode);
    }
}

// Diagnostic only. Missing/partial rows never replace strict terminal TRX or authorize model effects.
internal sealed class NativeResourceTestStageTrace : IDisposable
{
    internal const string FactIdentity = "LlmFromScratch.Tests.CodeDocumentationConfiguredQwenNativeResourceTests.HeldCompleteContext_NativePrepareAndOverflow";
    private static readonly string[] Order = {
        "fact.start", "native-open.start", "native-open.end", "fit-harness-create.start", "fit-harness-create.end",
        "fit-bind.start", "fit-bind.end", "fit-prepare.start", "fit-prepare.end", "fit-assertions.start", "fit-assertions.end",
        "fit-harness-dispose.start", "fit-harness-dispose.end", "overflow-harness-create.start", "overflow-harness-create.end",
        "overflow-bind.start", "overflow-bind.end", "overflow-prepare.start", "overflow-prepare.end", "overflow-assertions.start", "overflow-assertions.end",
        "overflow-harness-dispose.start", "overflow-harness-dispose.end", "native-dispose.start", "native-dispose.end", "fact.end" };
    private readonly FileStream? stream;
    private readonly Stopwatch elapsed = Stopwatch.StartNew();
    private int sequence;
    private int bytesWritten;
    internal string? FailureStatus { get; private set; }
    internal string? FailureType { get; private set; }
    private NativeResourceTestStageTrace(FileStream? retained) { stream = retained; }
    internal static NativeResourceTestStageTrace FromEnvironment()
    {
        var path = Environment.GetEnvironmentVariable("S04_NATIVE_STAGE_PATH");
        var candidate = Environment.GetEnvironmentVariable("S04_NATIVE_STAGE_CANDIDATE");
        var run = Environment.GetEnvironmentVariable("S04_NATIVE_STAGE_RUN");
        if (path is null && candidate is null && run is null) return new(null);
        if (!OperatingSystem.IsWindows() || path is null || candidate is null || run is null ||
            !Regex.IsMatch(candidate, "\\A[0-9a-f]{40}\\z", RegexOptions.CultureInvariant) ||
            !Regex.IsMatch(run, "\\A[1-9][0-9]{0,19}\\z", RegexOptions.CultureInvariant) ||
            Environment.GetEnvironmentVariable("GITHUB_ACTIONS") != "true" || Environment.GetEnvironmentVariable("GITHUB_RUN_ID") != run ||
            Environment.GetEnvironmentVariable("GATE_NAME") != "S04.04A-NATIVE-RESOURCE" || Environment.GetEnvironmentVariable("RUNNER_ENVIRONMENT") != "github-hosted")
            throw new InvalidDataException("S04_TRACE_SETUP_BINDING");
        var temp = Environment.GetEnvironmentVariable("RUNNER_TEMP");
        if (string.IsNullOrWhiteSpace(temp) || !Path.IsPathFullyQualified(temp) || !Path.IsPathFullyQualified(path))
            throw new InvalidDataException("S04_TRACE_SETUP_ROOT");
        var expected = Path.Combine(Path.GetFullPath(temp), "s04-native-resource-evidence", "native3-progress.ndjson");
        if (!string.Equals(path, expected, StringComparison.Ordinal) || !string.Equals(Path.GetFullPath(path), expected, StringComparison.Ordinal) ||
            !Directory.Exists(Path.GetDirectoryName(expected)) || File.Exists(expected) || Directory.Exists(expected))
            throw new InvalidDataException("S04_TRACE_SETUP_PATH");
        for (var ancestor = Path.GetDirectoryName(expected); ancestor is not null; ancestor = Path.GetDirectoryName(ancestor))
            if ((File.GetAttributes(ancestor) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("S04_TRACE_SETUP_REPARSE");
        var retained = new FileStream(expected, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        var trace = new NativeResourceTestStageTrace(retained);
        try
        {
            trace.WriteFrame("{\"schema\":1,\"candidate\":\"" + candidate + "\",\"runId\":\"" + run + "\",\"fact\":\"" + FactIdentity + "\"}");
            return trace;
        }
        catch { retained.Dispose(); throw; } // Setup/header failure is before native Open.
    }
    internal void Stage(string stage, string edge)
    {
        if (stream is null || FailureStatus is not null) return;
        try
        {
            if (sequence >= Order.Length || !string.Equals(Order[sequence], stage + "." + edge, StringComparison.Ordinal))
                throw new InvalidDataException("S04_TRACE_STAGE_ORDER");
            WriteFrame("{\"sequence\":" + (sequence + 1).ToString(CultureInfo.InvariantCulture) + ",\"utc\":\"" + DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture) +
                "\",\"elapsedMs\":" + elapsed.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture) + ",\"stage\":\"" + stage + "\",\"edge\":\"" + edge + "\"}");
            sequence++;
        }
        catch (Exception error) { DiagnosticFailure("WRITE_FAILED", error); } // Never replace a native/assertion/OCE or block its cleanup.
    }
    private void WriteFrame(string frame)
    {
        var bytes = new UTF8Encoding(false, true).GetBytes(frame + "\n");
        if (bytes.Length > 512 || bytesWritten + bytes.Length > 16384) throw new InvalidDataException("S04_TRACE_SIZE");
        stream!.Write(bytes, 0, bytes.Length); stream.Flush(true); bytesWritten += bytes.Length;
    }
    private void DiagnosticFailure(string status, Exception error)
    {
        FailureStatus = status;
        FailureType = error switch { InvalidDataException => "INVALID_DATA", IOException => "IO", UnauthorizedAccessException => "ACCESS", ObjectDisposedException => "DISPOSED", _ => "OTHER" };
        try { Console.Error.WriteLine("S04_TRACE_" + status + ":" + FailureType); } catch { } // Fixed metadata only; no exception message.
    }
    public void Dispose()
    {
        try { stream?.Dispose(); } catch (Exception error) { DiagnosticFailure("DISPOSE_FAILED", error); }
    }
}
