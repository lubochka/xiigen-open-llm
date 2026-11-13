// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using TrainableRouter.Training;
using H = LlmFromScratch.Tests.CodeDocumentationPreparedBaselineTests;
using A = TrainableRouter.Training.CodeDocumentationRepositoryOwnedDpoDatasetAuthoring;
namespace LlmFromScratch.Tests;

// These portable Facts execute the real held source consumer with a named Synthetic seam, never claim native tokenization.
[Collection(RepoArtifactSnapshotCollectionNames.CollectionName)] // Existing nonparallel collection isolates legacy process-global source observers while these real fixture consumers execute.
public sealed class CodeDocumentationConfiguredQwenTokenAdmissionTests
{
    [Fact]
    public async Task CompleteContext_PreparesBoundBody()
    {
        using var h = H.TeacherContextHarness(); var (b, a) = await H.BindTeacherCompleted(h);
        var files = h.Files(); var history = h.HistoryBytes(); var seen = 0;
        var result = await QwenAdmissionTestSupport.Prepare(h, b, a, (rendered, token) =>
        {
            token.ThrowIfCancellationRequested(); seen++;
            QwenAdmissionTestSupport.AssertCompleteRendered(rendered, h, b);
            return OperationResult<int[]>.Ok(Enumerable.Repeat(264, 512).ToArray());
        });
        Assert.True(result.IsSuccess, result.ErrorCode + ":" + result.ErrorMessage); Assert.Equal(1, seen);
        var prepared = result.Value!; Assert.False(prepared.FullTeacherContextReady); Assert.False(prepared.Content.FullTeacherContextReady);
        using var complete = JsonDocument.Parse(prepared.Request.Prompt);
        Assert.Equal(prepared.Content.Student.Material.OwnerInput, complete.RootElement.GetProperty("selected_context").GetProperty("task").GetString());
        Assert.Equal(prepared.Content.Student.Material.PaidModelReferenceAnswer, complete.RootElement.GetProperty("selected_context").GetProperty("teachingReference").GetString());
        Assert.Equal(prepared.Content.CopyTeacherContentUtf8(), Encoding.UTF8.GetBytes(complete.RootElement.GetProperty("selected_context").GetRawText()));
        Assert.Equal("SyntheticTest", prepared.Request.SourceOrigin); Assert.Equal("SyntheticTest", prepared.Request.TokenizerOrigin);
        Assert.Equal(512, prepared.Request.InputTokens); Assert.Equal(2048, prepared.Request.OutputReserve);
        Assert.True(prepared.Request.Matches(prepared.Content.ContentSha256, QwenAdmissionTestSupport.Configuration()));
        var copied = prepared.Request.CopyBody(); copied[0] = 0; Assert.Equal((byte)'{', prepared.Request.CopyBody()[0]);
        var ids = prepared.Request.CopyTokenIds(); ids[0] = 0; Assert.Equal(264, prepared.Request.CopyTokenIds()[0]);
        QwenAdmissionTestSupport.AssertBody(prepared.Request);
        Assert.Equal(history, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls); Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting); h.AssertFiles(files);
    }
    [Fact]
    public async Task CompleteContext_UnderByteOverflow_RefusesBeforeEffects()
    {
        var suffix = string.Concat(Enumerable.Repeat("a ", 6400));
        using var h = H.TeacherContextHarness(chosenHistorySuffix: suffix); var (b, a) = await H.BindTeacherCompleted(h);
        var files = h.Files(); var history = h.HistoryBytes(); var seen = 0;
        var result = await QwenAdmissionTestSupport.Prepare(h, b, a, (rendered, token) =>
        {
            token.ThrowIfCancellationRequested(); seen++;
            QwenAdmissionTestSupport.AssertCompleteRendered(rendered, h, b);
            using var document = JsonDocument.Parse(QwenAdmissionTestSupport.Prompt(rendered));
            var packet = document.RootElement.GetProperty("selected_context");
            Assert.Contains(suffix, packet.GetProperty("history")[0].GetProperty("pair").GetProperty("chosen").GetString()!);
            Assert.True(Encoding.UTF8.GetByteCount(packet.GetRawText()) < A.MaximumTeacherContentUtf8Bytes);
            Assert.True(Encoding.UTF8.GetByteCount(QwenAdmissionTestSupport.Prompt(rendered)) < LocalQwenOllamaTeacherInferenceClient.MaximumPromptUtf8Bytes);
            return OperationResult<int[]>.Ok(Enumerable.Repeat(264, 6145).ToArray());
        });
        Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(ConfiguredQwenVocabularyLease.BudgetExceeded, result.ErrorCode); Assert.Equal(1, seen);
        // A separate matching control reaches the same held consumer successfully; overflow is not constant unavailability.
        var control = await QwenAdmissionTestSupport.Prepare(h, b, a, (_, _) => OperationResult<int[]>.Ok([264]));
        Assert.True(control.IsSuccess, control.ErrorCode); Assert.Equal(history, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls); h.AssertFiles(files);
    }
    [Fact]
    public void BudgetBoundary_RenderVectors()
    {
        var configuration = QwenAdmissionTestSupport.Configuration();
        var shape = LocalQwenOllamaTeacherInferenceClient.PreparedAuthoringRequest.Prepare(QwenAdmissionTestSupport.ShapeControlPrompt, new string('a', 64), "SyntheticVector",
            configuration, null, (rendered, _) => { Assert.Equal(QwenAdmissionTestSupport.Prefix + QwenAdmissionTestSupport.ShapeControlPrompt + QwenAdmissionTestSupport.Suffix, rendered); return OperationResult<int[]>.Ok(QwenAdmissionTestSupport.ShapeControlIds()); }, CancellationToken.None);
        Assert.True(shape.IsSuccess, shape.ErrorCode); Assert.Equal(144, shape.Value!.InputTokens);
        Assert.Equal("22825c1ad5a984f3667337ea40c08ca7c0ed495faada2e473e88c31feed375e7", ConfiguredQwenVocabularyLease.TokenIdsSha(shape.Value.CopyTokenIds()));
        foreach (var repeat in new[] { 6128, 6129 })
        {
            var prompt = QwenAdmissionTestSupport.BoundaryPrompt(repeat); var rendered = LocalQwenOllamaTeacherInferenceClient.RenderRestrictedSingleUser(prompt);
            var ids = QwenAdmissionTestSupport.BoundaryIds(repeat);
            Assert.Equal(repeat + 16, ids.Length);
            Assert.Equal(repeat == 6128 ? "55731ec9f3e2e3e2d702df5d9f72131f5274c12cd9e97c2b78a5a7e4d7d278e9" : "0e39232d1de133a572831e33c247fa987d7b7c684754953181a496c593f1d2bd",
                ConfiguredQwenVocabularyLease.TokenIdsSha(ids));
            var result = LocalQwenOllamaTeacherInferenceClient.PreparedAuthoringRequest.Prepare(prompt, new string('a', 64), "SyntheticVector",
                configuration, null, (actual, _) => { Assert.Equal(rendered, actual); return OperationResult<int[]>.Ok(ids); }, CancellationToken.None);
            Assert.Equal(repeat == 6128, result.IsSuccess);
            if (result.IsSuccess) Assert.Equal(8192, result.Value!.InputTokens + result.Value.OutputReserve);
            else Assert.Equal(ConfiguredQwenVocabularyLease.BudgetExceeded, result.ErrorCode);
        }
        Assert.Equal("<|im_start|>user\n{\"x\":\"A\\u0301 你好\"}<|im_end|>\n<|im_start|>assistant\n<think>\n\n</think>\n\n",
            LocalQwenOllamaTeacherInferenceClient.RenderRestrictedSingleUser("{\"x\":\"A\\u0301 你好\"}"));
    }
    [Fact]
    public async Task StaleBindingAndOwnerChanges()
    {
        using var h = H.TeacherContextHarness(); var (b, a) = await H.BindTeacherCompleted(h); var files = h.Files(); var history = h.HistoryBytes();
        var result = await QwenAdmissionTestSupport.Prepare(h, b, a, (_, _) => OperationResult<int[]>.Ok([264]));
        Assert.True(result.IsSuccess, result.ErrorCode);
        Assert.False(result.Value!.Request.Matches(new string('f', 64), QwenAdmissionTestSupport.Configuration()));
        Assert.False(result.Value.Request.Matches(result.Value.Content.ContentSha256, QwenAdmissionTestSupport.Configuration(changed: true)));
        using var cancellation = new CancellationTokenSource(); var primary = new OperationCanceledException("prepared token primary", cancellation.Token);
        var finals = 0; h.Open.Session.SelectedStudentDurablePointForTesting = stage => { if (stage == "teacher-context-final-owner-checked") finals++; };
        var thrown = await Assert.ThrowsAsync<OperationCanceledException>(() => QwenAdmissionTestSupport.Prepare(h, b, a, (_, _) => throw primary));
        Assert.Same(primary, thrown); Assert.Equal(1, finals);
        OperationResult<A.SelectedTeacherPreparedRequest>? nested = null;
        var restored = await QwenAdmissionTestSupport.Prepare(h, b, a, (_, _) =>
        {
            nested = QwenAdmissionTestSupport.Prepare(h, b, a, (_, _) => throw new InvalidOperationException("reentry provider")).GetAwaiter().GetResult();
            return OperationResult<int[]>.Ok([264]);
        });
        Assert.True(restored.IsSuccess, restored.ErrorCode); Assert.NotNull(nested); Assert.False(nested!.IsSuccess);
        Assert.Equal(2, finals); Assert.Equal(history, h.HistoryBytes()); Assert.Equal(10, h.Client.Calls); h.AssertFiles(files);
        var source = h.Grant.SourceSelection!.TeachingCurriculum!.Sources.Single(s => s.SourceId == "teach-7");
        Assert.NotEqual(source.SourceId, restored.Value!.Content.Student.Request.SourceId);
        var address = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(h.Fixture.RepositoryRoot, source.DomainSlug, source.SubjectSlug, source.CorpusVersion);
        Assert.True(address.IsSuccess, address.ErrorCode);
        var path = Path.Combine(address.Value!.CorpusRoot, RepositoryOwnedReferenceCorpusManifestSealer.PayloadFileName);
        var bytes = File.ReadAllBytes(path); var ticks = File.GetLastWriteTimeUtc(path);
        try
        {
            var changed = await QwenAdmissionTestSupport.Prepare(h, b, a, (_, _) =>
            {
                File.WriteAllBytes(path, bytes.Concat(new byte[] { (byte)' ' }).ToArray());
                return OperationResult<int[]>.Ok([264]);
            });
            Assert.False(changed.IsSuccess); Assert.Null(changed.Value); Assert.Equal(3, finals);
        }
        finally { H.RestoreOwnedHistoryPair(h, path, bytes, ticks); }
        var validAgain = await QwenAdmissionTestSupport.Prepare(h, b, a, (_, _) => OperationResult<int[]>.Ok([264]));
        Assert.True(validAgain.IsSuccess, validAgain.ErrorCode); Assert.Equal(4, finals); Assert.Equal(history, h.HistoryBytes()); h.AssertFiles(files);
    }
    [Fact]
    public void ResourceUnavailable_NoFallback()
    {
        // Deliberately mismatched configured identity refuses before any native allocation on every installed-resource host.
        var result = ConfiguredQwenVocabularyLease.OpenInstalledWindows(QwenAdmissionTestSupport.Configuration());
        Assert.False(result.IsSuccess); Assert.Null(result.Value);
        Assert.Equal(OperatingSystem.IsWindows() && System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.X64
            ? ConfiguredQwenVocabularyLease.Mismatch : ConfiguredQwenVocabularyLease.Unsupported, result.ErrorCode);
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        Assert.Throws<OperationCanceledException>(() => ConfiguredQwenVocabularyLease.OpenInstalledWindows(QwenAdmissionTestSupport.Configuration(), canceled.Token));
        // The actual SafeHandle cleanup pipeline receives Synthetic release operations; no DLL, file or tokenizer is opened.
        var releases = new List<string>(); var secondary = new InvalidOperationException("Synthetic module release failure");
        using var partial = ConfiguredQwenVocabularyLease.OpenSyntheticCleanupForTesting(QwenAdmissionTestSupport.Configuration(), stage =>
        {
            releases.Add(stage); if (stage == "module:1") throw secondary;
        });
        var partialCustody = partial.AddSyntheticCustodyForTesting();
        var primary = new OperationCanceledException("Synthetic partial-open primary", canceled.Token);
        var actual = Assert.Throws<OperationCanceledException>(() => partial.FailSyntheticOpenForTesting(primary));
        Assert.Same(primary, actual); Assert.Equal(canceled.Token, actual.CancellationToken);
        Assert.Equal(new[] { "model", "callbacks", "module:1", "module:0", "path:1", "path:0", "dependency:0", "reservation" }, partial.CopyCleanupAttempts());
        Assert.Equal(new[] { "model", "callbacks", "module:1", "module:0", "path:1", "path:0" }, releases);
        var failures = Assert.IsType<(string Stage, Exception Error)[]>(actual.Data["ConfiguredQwenCleanupFailures"]);
        Assert.Single(failures); Assert.Equal("module:1", failures[0].Stage); Assert.Same(secondary, failures[0].Error);
        partial.Dispose(); Assert.Equal(8, partial.CopyCleanupAttempts().Length); Assert.Equal(1, partialCustody.ReleaseCount); // Repeated Dispose cannot double-free the model or balance custody twice.
        Assert.Equal(ConfiguredQwenVocabularyLease.Unavailable, partial.Count("{}", QwenAdmissionTestSupport.Configuration(), CancellationToken.None).ErrorCode);
        // Before a model was allocated, an invalid SafeHandle still releases the remaining partial-open graph through the same flow.
        var unopenedReleases = new List<string>(); var unopenedSecondary = new InvalidOperationException("Synthetic partial-open path release failure");
        using var unopened = ConfiguredQwenVocabularyLease.OpenSyntheticCleanupForTesting(QwenAdmissionTestSupport.Configuration(), stage =>
        {
            unopenedReleases.Add(stage); if (stage == "path:1") throw unopenedSecondary;
        }, allocatedModel: false);
        var unopenedCustody = unopened.AddSyntheticCustodyForTesting();
        var unopenedPrimary = new OperationCanceledException("Synthetic before-model primary", canceled.Token);
        var unopenedActual = Assert.Throws<OperationCanceledException>(() => unopened.FailSyntheticOpenForTesting(unopenedPrimary));
        Assert.Same(unopenedPrimary, unopenedActual); Assert.Equal(canceled.Token, unopenedActual.CancellationToken);
        Assert.Equal(new[] { "model", "callbacks", "module:1", "module:0", "path:1", "path:0", "dependency:0", "reservation" }, unopened.CopyCleanupAttempts());
        Assert.Equal(1, unopenedCustody.ReleaseCount);
        Assert.Equal(new[] { "callbacks", "module:1", "module:0", "path:1", "path:0" }, unopenedReleases);
        var unopenedFailures = Assert.IsType<(string Stage, Exception Error)[]>(unopenedActual.Data["ConfiguredQwenCleanupFailures"]);
        Assert.Single(unopenedFailures); Assert.Equal("path:1", unopenedFailures[0].Stage); Assert.Same(unopenedSecondary, unopenedFailures[0].Error);
        // An ambiguous model-free or callback-detach result must retain code/custody and the slot; it is not the reusable module-failure case.
        foreach (var failedStage in new[] { "model", "callbacks" })
        {
            var unsafeReleases = new List<string>(); var unsafeSecondary = new InvalidOperationException("Synthetic uncertain " + failedStage);
            using var retained = ConfiguredQwenVocabularyLease.OpenSyntheticCleanupForTesting(QwenAdmissionTestSupport.Configuration(), stage =>
            {
                unsafeReleases.Add(stage); if (stage == failedStage) throw unsafeSecondary;
            });
            var retainedCustody = retained.AddSyntheticCustodyForTesting();
            try
            {
                var unsafePrimary = new OperationCanceledException("Synthetic retained primary", canceled.Token);
                var unsafeActual = Assert.Throws<OperationCanceledException>(() => retained.FailSyntheticOpenForTesting(unsafePrimary));
                Assert.Same(unsafePrimary, unsafeActual); Assert.Equal(canceled.Token, unsafeActual.CancellationToken);
                Assert.Equal(failedStage == "model" ? new[] { "model" } : new[] { "model", "callbacks" }, unsafeReleases);
                Assert.Equal(new[] { "model", failedStage == "model" ? "callbacks-retained" : "callbacks", "module-retained:1", "module-retained:0",
                    "path-retained:1", "path-retained:0", "dependency-retained:0", "reservation-retained" }, retained.CopyCleanupAttempts());
                Assert.Equal(0, retainedCustody.ReleaseCount);
                var unsafeFailures = Assert.IsType<(string Stage, Exception Error)[]>(unsafeActual.Data["ConfiguredQwenCleanupFailures"]);
                Assert.Single(unsafeFailures); Assert.Equal(failedStage, unsafeFailures[0].Stage); Assert.Same(unsafeSecondary, unsafeFailures[0].Error);
                var refusedReopen = Assert.Throws<InvalidOperationException>(() => ConfiguredQwenVocabularyLease.OpenSyntheticCleanupForTesting(
                    QwenAdmissionTestSupport.Configuration(), _ => throw new InvalidOperationException("A retained slot must refuse before release effects.")));
                Assert.Equal("Native runtime reservation is held.", refusedReopen.Message);
                Assert.Equal(ConfiguredQwenVocabularyLease.Unavailable, retained.Count("{}", QwenAdmissionTestSupport.Configuration(), CancellationToken.None).ErrorCode);
            }
            finally { retained.ResetRetainedSyntheticCleanupForTesting(); } // Reset only the inert, asset-free test graph after observing its held reservation; native owners have no recovery seam.
            Assert.Equal(1, retainedCustody.ReleaseCount);
            Assert.Equal("synthetic-retention-reset", retained.CopyCleanupAttempts()[^1]);
        }
        // Abandon the lease and finalizable custody together: a queued custody finalizer must not free its underlying managed pin in either unsafe branch.
        foreach (var failedStage in new[] { "model", "callbacks" })
        {
            var abandoned = QwenAdmissionTestSupport.AbandonSyntheticCustody(failedStage);
            QwenAdmissionTestSupport.CollectAbandonedCustody();
            var found = abandoned.Owner.TryGetTarget(out var abandonedOwner);
            try
            {
                Assert.True(found); Assert.Equal(1, abandoned.Custody.FinalizerCount); Assert.Equal(0, abandoned.Custody.ReleaseCount);
                Assert.Equal(new[] { "model", failedStage == "model" ? "callbacks-retained" : "callbacks", "module-retained:1", "module-retained:0",
                    "path-retained:1", "path-retained:0", "dependency-retained:0", "reservation-retained" }, abandonedOwner!.CopyCleanupAttempts());
                var abandonedFailures = abandonedOwner!.CopyCleanupFailures(); Assert.Single(abandonedFailures);
                Assert.Equal(failedStage, abandonedFailures[0].Stage); Assert.Same(abandoned.Secondary, abandonedFailures[0].Error);
                Assert.Throws<InvalidOperationException>(() => ConfiguredQwenVocabularyLease.OpenSyntheticCleanupForTesting(QwenAdmissionTestSupport.Configuration(), _ => { }));
            }
            finally { if (found) abandonedOwner!.ResetRetainedSyntheticCleanupForTesting(); } // Only the sealed managed test pin is retired after proving that critical finalization did not close its dependency.
            Assert.Equal(1, abandoned.Custody.ReleaseCount);
        }
        var abandonedControl = QwenAdmissionTestSupport.AbandonSyntheticCustody(null);
        QwenAdmissionTestSupport.CollectAbandonedCustody();
        Assert.Equal(1, abandonedControl.Custody.ReleaseCount); // Safe abandoned cleanup balances the real managed pin exactly once.
        QwenAdmissionTestSupport.CollectAbandonedCustody();
        Assert.Equal(1, abandonedControl.Custody.ReleaseCount); // Later critical finalization cannot double-free the already released custody.
        var restoredReleases = new List<string>();
        using var restored = ConfiguredQwenVocabularyLease.OpenSyntheticCleanupForTesting(QwenAdmissionTestSupport.Configuration(), restoredReleases.Add);
        Assert.Equal("SyntheticCleanupOnly", restored.ResourceFingerprint);
        Assert.Equal(ConfiguredQwenVocabularyLease.Unsupported, restored.Count("{}", QwenAdmissionTestSupport.Configuration(), CancellationToken.None).ErrorCode);
        var noPromotion = LocalQwenOllamaTeacherInferenceClient.PreparedAuthoringRequest.Prepare("{}", new string('a', 64), "SyntheticTest",
            QwenAdmissionTestSupport.Configuration(), restored, null, CancellationToken.None);
        Assert.False(noPromotion.IsSuccess); Assert.Null(noPromotion.Value); Assert.Equal(ConfiguredQwenVocabularyLease.Unsupported, noPromotion.ErrorCode);
        restored.Dispose(); Assert.Empty(restored.CopyCleanupFailures()); Assert.Equal(6, restoredReleases.Count);
    }
    [Fact]
    public void LegacyProfileAndBodyPreserved()
    {
        Assert.Null(LocalQwenOllamaInferenceProfileAuthority.AuthoringDescriptor.Think); Assert.Null(LocalQwenOllamaInferenceProfileAuthority.AuthoringDescriptor.NumPredict);
        Assert.Equal(256, LocalQwenOllamaInferenceProfileAuthority.EvaluationDescriptor.NumPredict); Assert.False(LocalQwenOllamaInferenceProfileAuthority.EvaluationDescriptor.Think!.Value);
        Assert.NotEqual(LocalQwenOllamaInferenceProfileAuthority.Authoring, LocalQwenOllamaInferenceProfileAuthority.TokenBoundAuthoring);
        Assert.False(LocalQwenOllamaInferenceProfileAuthority.IsCanonicalAuthoring(LocalQwenOllamaInferenceProfileAuthority.TokenBoundAuthoring));
        Assert.True(LocalQwenOllamaInferenceProfileAuthority.IsCanonicalTokenBoundAuthoring(LocalQwenOllamaInferenceProfileAuthority.TokenBoundAuthoring));
        var method = typeof(LocalQwenOllamaTeacherInferenceClient).GetMethod("BuildRequestBody", BindingFlags.Static | BindingFlags.NonPublic)!;
        var body = (OperationResult<byte[]>)method.Invoke(null, [ "qwen3.5:9b-q4_K_M", "{}", LocalQwenOllamaInferenceProfileAuthority.AuthoringDescriptor, false ])!;
        Assert.True(body.IsSuccess);
        Assert.Equal("{\"model\":\"qwen3.5:9b-q4_K_M\",\"stream\":false,\"messages\":[{\"role\":\"user\",\"content\":\"{}\"}],\"format\":\"json\",\"options\":{\"num_ctx\":8192,\"temperature\":0,\"seed\":0},\"keep_alive\":\"5m\"}", Encoding.UTF8.GetString(body.Value!));
    }
}

// Shared helpers always compile; the excluded native file is a consumer only and contributes exactly three opt-in Facts.
internal static class QwenAdmissionTestSupport
{
    [MethodImpl(MethodImplOptions.NoInlining)] // End strong lease/custody locals before the caller forces collection in Release builds.
    internal static (WeakReference<ConfiguredQwenVocabularyLease> Owner, ConfiguredQwenVocabularyLease.SyntheticCustodyObservation Custody,
        Exception Secondary) AbandonSyntheticCustody(string? failedStage)
    {
        var secondary = new InvalidOperationException("Synthetic abandoned " + (failedStage ?? "safe control"));
        var lease = ConfiguredQwenVocabularyLease.OpenSyntheticCleanupForTesting(Configuration(), stage =>
        {
            if (stage == failedStage) throw secondary; // This callback captures no lease/custody reference and cannot prevent their finalization.
        });
        var custody = lease.AddSyntheticCustodyForTesting();
        return (new WeakReference<ConfiguredQwenVocabularyLease>(lease, trackResurrection: true), custody, secondary);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void CollectAbandonedCustody()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
    }
    internal const string Prefix = "<|im_start|>user\n";
    internal const string ShapeControlPrompt = "{\"instruction\":\"Return only JSON with string fields: chosen, rejected, rejected_reason.\",\"context\":{\"origin\":\"SyntheticVectorOnly\",\"task\":\"Document int Add(int a, int b) => a + b;\",\"teachingReference\":\"Returns the sum of a and b.\",\"weakStudentResponse\":\"Returns a number.\",\"history\":[{\"contextOnlyId\":\"history-0\",\"prompt\":\"Document the earlier multiply function.\",\"chosen\":\"Explains both operands and their product.\",\"rejected\":\"Returns a value.\"},{\"contextOnlyId\":\"ordinary-0\",\"prompt\":\"Document the earlier subtract function.\",\"chosen\":\"Explains the first operand minus the second.\",\"rejected\":\"Does something.\"}]}}"; // Immutable resource vector only; this is not a held source acceptance receipt.
    internal static int[] ShapeControlIds() => [248045, 846, 198, 4754, 53125, 3147, 5423, 1132, 4566, 440, 886, 4881, 25, 11543, 11, 17030, 11, 17030, 36962, 46292, 2078, 21624, 8348, 3147, 35904, 17034, 3653, 7081, 2129, 7945, 3147, 7288, 514, 2604, 1494, 264, 11, 514, 292, 8, 574, 264, 478, 292, 26, 2129, 647, 11490, 8589, 3147, 15964, 279, 2542, 314, 264, 321, 292, 46292, 12768, 13692, 2497, 3147, 15964, 264, 1324, 46292, 18274, 64331, 2078, 7081, 739, 3147, 18274, 12, 15, 2129, 39242, 3147, 7288, 279, 6575, 29283, 709, 46292, 60947, 3147, 42498, 1674, 2107, 52680, 321, 836, 1918, 46292, 92060, 3147, 15964, 264, 869, 1149, 35607, 2078, 7081, 739, 3147, 20388, 12, 15, 2129, 39242, 3147, 7288, 279, 6575, 31192, 709, 46292, 60947, 3147, 42498, 1674, 279, 1118, 26336, 26401, 279, 2018, 46292, 92060, 3147, 20810, 2414, 1149, 24634, 3307, 248046, 198, 248045, 74455, 198, 248068, 271, 248069, 271]; // Recorded int32LE vocabulary output, never a synthetic claim of configured runtime execution.
    internal const string Suffix = "<|im_end|>\n<|im_start|>assistant\n<think>\n\n</think>\n\n";
    internal static ContinuousLearningTp1DpoConfiguration Configuration(bool installed = false, bool changed = false)
    {
        var result = ContinuousLearningTp1DpoConfiguration.Create(installed ? "qwen3.5:9b-q4_K_M" : "qwen-configured-fixture:9b",
            installed ? ConfiguredQwenVocabularyLease.ManifestSha : new string(changed ? 'f' : 'a', 64),
            installed ? ConfiguredQwenVocabularyLease.ModelSha : new string('b', 64), 10000, "sha256:" + new string('c', 64), "sha256:" + new string('d', 64), new byte[32]);
        Assert.True(result.IsSuccess, result.ErrorCode + ":" + result.ErrorMessage); return result.Value!;
    }
    internal static Task<OperationResult<A.SelectedTeacherPreparedRequest>> Prepare(H.OrdinaryDiagnosticHarness h,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b, A.SelectedTeachingMaterialAdmission a,
        Func<string, CancellationToken, OperationResult<int[]>> provider, CancellationToken token = default)
        => h.Open.Session.PrepareOrdinarySelectedTeacherRequestSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector,
            a, null, H.AssessTeacherHistory, Configuration(), provider, token);
    internal static string Prompt(string rendered)
    { Assert.StartsWith(Prefix, rendered); Assert.EndsWith(Suffix, rendered); return rendered[Prefix.Length..^Suffix.Length]; }
    internal static void AssertCompleteRendered(string rendered, H.OrdinaryDiagnosticHarness h,
        CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.OrdinaryBaseline b)
    {
        using var json = JsonDocument.Parse(Prompt(rendered)); var root = json.RootElement;
        Assert.Equal("Return only JSON with string fields: chosen, rejected, rejected_reason.", root.GetProperty("instruction").GetString());
        Assert.Equal(3, root.GetProperty("rules").GetArrayLength());
        Assert.Equal("Chosen and rejected must be authored by the permitted external open Chinese teacher; follow the selected task's content and language.", root.GetProperty("rules")[0].GetString());
        var packet = root.GetProperty("selected_context");
        Assert.Equal("code-documentation-selected-teacher-content-v1", packet.GetProperty("schema").GetString());
        Assert.Equal("SyntheticTest", packet.GetProperty("origin").GetString());
        Assert.Equal("b", packet.GetProperty("weakStudentResponse").GetString());
        Assert.False(string.IsNullOrEmpty(packet.GetProperty("task").GetString())); Assert.False(string.IsNullOrEmpty(packet.GetProperty("teachingReference").GetString()));
        var history = H.ReadTeacherHistory(h, b); var rows = packet.GetProperty("history"); Assert.Equal(2, rows.GetArrayLength());
        for (var i = 0; i < 2; i++)
        {
            Assert.Equal(history.Pairs[i].SourceId, rows[i].GetProperty("sourceId").GetString());
            Assert.Equal(history.Pairs[i].Row.PairId, rows[i].GetProperty("pairId").GetString());
            var pair = rows[i].GetProperty("pair");
            Assert.Equal(history.Pairs[i].Pair.Prompt, pair.GetProperty("prompt").GetString());
            Assert.Equal(history.Pairs[i].Pair.Chosen, pair.GetProperty("chosen").GetString());
            Assert.Equal(history.Pairs[i].Pair.Rejected, pair.GetProperty("rejected").GetString());
        }
    }
    internal static void AssertBody(LocalQwenOllamaTeacherInferenceClient.PreparedAuthoringRequest request)
    {
        using var json = JsonDocument.Parse(request.CopyBody()); var root = json.RootElement;
        Assert.False(root.GetProperty("stream").GetBoolean()); Assert.False(root.GetProperty("truncate").GetBoolean()); Assert.False(root.GetProperty("shift").GetBoolean());
        Assert.False(root.GetProperty("think").GetBoolean()); Assert.Equal("json", root.GetProperty("format").GetString());
        Assert.Equal(1, root.GetProperty("messages").GetArrayLength()); Assert.Equal("user", root.GetProperty("messages")[0].GetProperty("role").GetString());
        Assert.Equal(request.Prompt, root.GetProperty("messages")[0].GetProperty("content").GetString());
        var options = root.GetProperty("options"); Assert.Equal(8192, options.GetProperty("num_ctx").GetInt32());
        Assert.Equal(2048, options.GetProperty("num_predict").GetInt32()); Assert.Equal(0, options.GetProperty("seed").GetInt32()); Assert.Equal(0, options.GetProperty("temperature").GetInt32());
        Assert.False(root.TryGetProperty("tools", out _)); Assert.False(root.TryGetProperty("system", out _)); Assert.False(root.TryGetProperty("images", out _));
    }
    internal static string BoundaryPrompt(int repeats) => "{\"x\":\"" + string.Concat(Enumerable.Repeat("a ", repeats)) + "\"}";
    internal static int[] BoundaryIds(int repeats)
        => new[] { 248045, 846, 198, 4754, 87, 3147, 64 }.Concat(Enumerable.Repeat(264, repeats - 1))
            .Concat(new[] { 77769, 248046, 198, 248045, 74455, 198, 248068, 271, 248069, 271 }).ToArray();
}
