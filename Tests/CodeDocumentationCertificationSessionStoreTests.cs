// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LlmFromScratch.Tests;

using System.Collections;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure;
using ContinuousLearning;
using ModelServer;
using Xunit.Abstractions;
using Store = ContinuousLearning.CodeDocumentationCertificationSessionStore;

public sealed class CodeDocumentationCertificationSessionStoreTests
{
    private readonly ITestOutputHelper _output;

    public CodeDocumentationCertificationSessionStoreTests(ITestOutputHelper output) => _output = output;

    private const string Domain = "code-documentation";
    private const string Pattern = "builder";
    private const string Subject = "api-reference";
    private const string Owner = "synthetic-source-owner";
    private const string Secret = "CLOSED_CERT_SENTINEL";
    private static readonly DateTimeOffset Cut = new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly CertificationHistoryCoverage Complete = new(CertificationHistoryCompleteness.Complete,
        CertificationHistoryCompleteness.Complete, CertificationHistoryCompleteness.Complete);
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [Fact]
    public void Session_ReserveAndStartReopenExactIdentityBeforeReturningPermit()
    {
        using var diagnostics = new FailureDiagnosticScope(observation =>
            _output.WriteLine("session-failure-observation: " + Serialize(observation)));
        using var progress = Hook(point => _output.WriteLine("session-observed-point: " + point));
        using var fixture = new Fixture();
        var scenario = fixture.Scenario();
        var authority = Issue(scenario);
        var admission = Admit(authority);
        var reservationPath = fixture.Event(admission.AdmissionIdentitySha256[7..], "reservation");
        _output.WriteLine($"session-path-lengths: repository={fixture.Repository.Length}; store={fixture.StoreRoot.Length}; " +
            $"session={Path.GetDirectoryName(reservationPath)!.Length}; reservation={reservationPath.Length}; stage={(reservationPath + ".stage").Length}");
        var inputs = fixture.InputSnapshot();
        using var store = fixture.Create();
        using var reservation = Require(store.ReserveOrReplay(authority, "first-run"));
        Assert.False(reservation.IsReplay);
        Assert.Null(reservation.Receipt);
        Assert.Equal(admission.AdmissionIdentitySha256[7..], reservation.SessionId);
        var bytes = File.ReadAllBytes(fixture.Event(reservation.SessionId, "reservation"));
        var payload = ReadEnvelope(bytes);
        Assert.Equal("first-run", payload.GetProperty("diagnosticRunId").GetString());
        Assert.Equal(authority.Evaluator.MaxTeacherCalls, payload.GetProperty("maximumEvaluationCalls").GetInt32());
        var bound = payload.GetProperty("admission");
        var expectedAdmission = JsonSerializer.SerializeToNode(admission, Json)!.AsObject();
        expectedAdmission["evaluator"]!.AsObject().Remove("observedEvaluationRequestIds");
        Assert.Equal(expectedAdmission.ToJsonString(Json), bound.GetRawText());
        var permit = Require(store.BeginEvaluation(reservation));
        Assert.NotNull(permit);
        var started = ReadEnvelope(File.ReadAllBytes(fixture.Event(reservation.SessionId, "started")));
        Assert.Equal(Hash(bytes), started.GetProperty("reservationEnvelopeSha256").GetString());
        Assert.Equal(3, fixture.AnchorLines().Length);
        Failure(store.BeginEvaluation(reservation), "INVALID_PERMIT");
        Assert.Equal(inputs, fixture.InputSnapshot());
        reservation.Dispose();
        using var reopened = fixture.Open();
        Failure(reopened.ReserveOrReplay(authority, "renamed"), "INCOMPLETE_EXPOSED");
        AssertStaleAnchorFailsBeforeAppend();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Session_CompletedExactReplayAfterReconstructionAndRunRenameReturnsSameReceipt(bool highScore)
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        CodeDocumentationCertificationSessionReceipt receipt;
        DateTimeOffset reserved;
        using (var store = fixture.Create())
        using (var reservation = Require(store.ReserveOrReplay(authority, "original-run")))
        {
            reserved = reservation.ReservedAtUtc;
            var permit = Require(store.BeginEvaluation(reservation));
            var result = Result(authority, highScore);
            receipt = Require(store.CompleteFromServerOwnedExecution(permit, result));
            Assert.Equal(highScore ? 1d : 0d, receipt.MeasurementNoisePassport.MeanScore);
            Assert.Equal(10, receipt.ObservedEvaluationCallCount);
        }
        var snapshot = fixture.Snapshot();
        using var reopened = fixture.Open();
        using var replay = Require(reopened.ReserveOrReplay(authority, "renamed-run"));
        Assert.True(replay.IsReplay);
        Assert.Equal("original-run", replay.DiagnosticRunId);
        Assert.Equal(reserved, replay.ReservedAtUtc);
        Assert.Equal(Serialize(receipt), Serialize(replay.Receipt));
        Failure(reopened.BeginEvaluation(replay), "INVALID_PERMIT");
        Assert.Equal(snapshot, fixture.Snapshot());
    }

    [Fact]
    public void Session_ReceiptIdentityIsStableFromFreshCompletionThroughRenamedReplay()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        var admission = Admit(authority);
        CodeDocumentationCertificationSessionReceipt receipt;
        using (var store = fixture.Create()) receipt = CompleteSession(store, authority);

        var completedBytes = File.ReadAllBytes(fixture.Event(admission.AdmissionIdentitySha256[7..], "completed"));
        Assert.Matches("^sha256:[0-9a-f]{64}$", receipt.AdmissionIdentitySha256);
        Assert.Equal(admission.AdmissionIdentitySha256, receipt.AdmissionIdentitySha256);
        Assert.Matches("^sha256:[0-9a-f]{64}$", receipt.ReceiptIdentitySha256);
        Assert.Equal("sha256:" + Hash(completedBytes), receipt.ReceiptIdentitySha256);

        var snapshot = fixture.Snapshot();
        using var reopened = fixture.Open();
        using var replay = Require(reopened.ReserveOrReplay(authority, "renamed-run"));
        Assert.True(replay.IsReplay);
        Assert.Equal(receipt.AdmissionIdentitySha256, replay.Receipt!.AdmissionIdentitySha256);
        Assert.Equal(receipt.ReceiptIdentitySha256, replay.Receipt.ReceiptIdentitySha256);
        Assert.Equal("sha256:" + Hash(completedBytes), replay.Receipt.ReceiptIdentitySha256);
        Assert.Equal(snapshot, fixture.Snapshot());
    }

    [Fact]
    public void Session_TamperedCompletedEnvelopeFailsClosedBeforeReceiptReplay()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using (var store = fixture.Create()) CompleteSession(store, authority);

        var completedPath = fixture.Event(Admit(authority).AdmissionIdentitySha256[7..], "completed");
        RewriteEnvelope(completedPath, payload => payload["completedAtUtc"] = Cut.AddMinutes(1).ToString("O"));
        Failure(Store.OpenExistingForServerOwnedRoot(fixture.Repository, fixture.StoreId), "HISTORY_MISMATCH");
    }

    [Fact]
    public void MaterialExposure_LegacyStoreOpensWithoutCreatingOrRewritingExposureState()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using (var store = fixture.Create())
            CompleteSession(store, authority);

        var before = fixture.Snapshot();
        Assert.False(Directory.Exists(Path.Combine(fixture.StoreRoot, "exposures")));
        using (var reopened = fixture.Open())
            Assert.Equal(before, fixture.Snapshot());
        Assert.False(Directory.Exists(Path.Combine(fixture.StoreRoot, "exposures")));
    }

    [Fact]
    public void MaterialBoundary_MatchesAssessmentContextRequiresExactRepositoryAndCorpusIdentity()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        var diagnosticCorpusManifestSha256 = authority.Corpus.Manifest.ManifestSha256;
        using var store = fixture.Create();
        var boundary = Require(CodeDocumentationMaterialBoundary.CreateFromServerOwnedCertificationScope(
            fixture.Repository, fixture.StoreId, store, authority, CertificationExposureRole.TeacherVisibleDiagnostic));

        Assert.True(boundary.MatchesAssessmentContext(fixture.Repository, diagnosticCorpusManifestSha256));
        Assert.False(boundary.MatchesAssessmentContext(
            Path.Combine(fixture.OwnRoot, "foreign-repository"), diagnosticCorpusManifestSha256));
        Assert.False(boundary.MatchesAssessmentContext(fixture.Repository, "sha256:" + Hex('f')));
    }

    [Fact]
    public void MaterialBoundary_DiagnosticProviderUsesServerOwnedMixedCorpusWithoutCertificationRelabeling()
    {
        using var fixture = new Fixture();
        var opened = fixture.OpenCorpus(
            "diagnostic-mixed",
            [
                Entry("diagnostic-train", Secret + ": train", FrozenReferencePartition.Train),
                Entry("diagnostic-heldout", Secret + ": heldout", FrozenReferencePartition.HeldOut),
            ]);
        using var store = fixture.Create();
        var provider = Require(CodeDocumentationServerOwnedMaterialBoundaryProvider.Create(
            fixture.Repository,
            fixture.StoreId,
            store));
        var boundary = Require(provider.IssueTeacherVisibleDiagnostic(
            fixture.Repository,
            opened));

        Assert.True(boundary.MatchesAssessmentContext(
            fixture.Repository,
            opened.Manifest.ManifestSha256));
        using var use = Require(boundary.BeginDiagnosticDisclosure());
        Assert.StartsWith("sha256:", use.ExposureIdentitySha256, StringComparison.Ordinal);
        Assert.False(
            Require(CodeDocumentationMaterialBoundary.CreateFromServerOwnedDiagnosticCorpus(
                fixture.Repository,
                fixture.StoreId,
                store,
                opened)).MatchesAssessmentContext(
                    Path.Combine(fixture.OwnRoot, "foreign"),
                    opened.Manifest.ManifestSha256));
    }

    [Fact]
    public void MaterialBoundary_DiagnosticProviderUsesOnlyHeldOutRowsAtDiagnosticLimit()
    {
        using var fixture = new Fixture();
        var train = Entry("diagnostic-train", Secret + ": train", FrozenReferencePartition.Train);
        var heldOut = Enumerable.Range(0, 200)
            .Select(i => Entry("diagnostic-heldout-" + i, Secret + ": heldout " + i))
            .ToArray();
        var opened = fixture.OpenCorpus(
            "diagnostic-limit",
            new[] { train }.Concat(heldOut));
        using var store = fixture.Create();
        var provider = Require(CodeDocumentationServerOwnedMaterialBoundaryProvider.Create(
            fixture.Repository,
            fixture.StoreId,
            store));
        var boundary = Require(provider.IssueTeacherVisibleDiagnostic(
            fixture.Repository,
            opened));

        using var use = Require(boundary.BeginDiagnosticDisclosure());
        var exposurePath = Path.Combine(
            fixture.StoreRoot,
            "exposures",
            use.ExposureIdentitySha256[7..],
            "exposure.json");
        var keys = ReadEnvelope(File.ReadAllBytes(exposurePath))
            .GetProperty("materialExposureKeys")
            .EnumerateArray()
            .Select(item => (
                Kind: item.GetProperty("kind").GetString()!,
                Digest: item.GetProperty("digest").GetString()!))
            .ToArray();

        Assert.Equal(400, keys.Length);
        Assert.Equal(200, keys.Count(key => key.Kind == "source"));
        Assert.Equal(200, keys.Count(key => key.Kind == "content"));
        Assert.DoesNotContain(keys, key =>
            key.Digest == train.SourceEntityFingerprint ||
            key.Digest == FrozenReferenceEntry.ComputeNormalizedTextSha256(train.OwnerInput));
        foreach (var entry in heldOut)
        {
            Assert.Contains(keys, key => key.Kind == "source" &&
                key.Digest == entry.SourceEntityFingerprint);
            Assert.Contains(keys, key => key.Kind == "content" &&
                key.Digest == FrozenReferenceEntry.ComputeNormalizedTextSha256(entry.OwnerInput));
        }
    }

    [Fact]
    public void MaterialBoundary_DiagnosticProviderRejectsMoreThan200HeldOutRowsBeforeExposure()
    {
        using var fixture = new Fixture();
        var opened = fixture.OpenCorpus(
            "diagnostic-over-limit",
            Enumerable.Range(0, 201)
                .Select(i => Entry("diagnostic-over-" + i, Secret + ": over " + i)));
        using var store = fixture.Create();
        var provider = Require(CodeDocumentationServerOwnedMaterialBoundaryProvider.Create(
            fixture.Repository,
            fixture.StoreId,
            store));
        var before = fixture.Snapshot();

        Failure(provider.IssueTeacherVisibleDiagnostic(fixture.Repository, opened), "INPUT_INVALID");
        Assert.Equal(before, fixture.Snapshot());
        Assert.False(Directory.Exists(Path.Combine(fixture.StoreRoot, "exposures")));
    }

    [Fact]
    public void MaterialBoundary_DiagnosticProviderRejectsManifestCorpusIdentityMismatch()
    {
        using var fixture = new Fixture();
        var opened = fixture.OpenCorpus(
            "diagnostic-identity",
            [Entry("diagnostic-identity", Secret + ": identity")]);
        using var store = fixture.Create();
        var provider = Require(CodeDocumentationServerOwnedMaterialBoundaryProvider.Create(
            fixture.Repository,
            fixture.StoreId,
            store));
        var mismatched = opened with
        {
            Manifest = opened.Manifest with { CorpusSha256 = "sha256:" + Hex('f') },
        };

        Failure(provider.IssueTeacherVisibleDiagnostic(fixture.Repository, mismatched), "INPUT_INVALID");
        Assert.False(Directory.Exists(Path.Combine(fixture.StoreRoot, "exposures")));
    }

    [Fact]
    public void MaterialExposure_FirstAppendIsAnchoredIdempotentAndBlocksCompletedReplay()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        var disjoint = Issue(fixture.Scenario("disjoint"));
        using var store = fixture.Create();
        CompleteSession(store, authority);

        var previousAnchor = File.ReadAllBytes(fixture.Anchor);
        var previousSessions = fixture.SessionBytes();
        var boundary = Require(CodeDocumentationMaterialBoundary.CreateFromServerOwnedCertificationScope(
            fixture.Repository, fixture.StoreId, store, authority, CertificationExposureRole.TeacherVisibleDiagnostic));
        var use = Require(boundary.BeginDiagnosticDisclosure());
        var exposureId = use.ExposureIdentitySha256[7..];
        var exposurePath = Path.Combine(fixture.StoreRoot, "exposures", exposureId, "exposure.json");
        var firstAnchor = File.ReadAllBytes(fixture.Anchor);
        Assert.True(firstAnchor.AsSpan(0, previousAnchor.Length).SequenceEqual(previousAnchor));
        Assert.Equal(previousSessions, fixture.SessionBytes());
        Assert.Equal(5, fixture.AnchorLines().Length);
        Assert.True(File.Exists(exposurePath));
        Assert.DoesNotContain(Secret, Encoding.UTF8.GetString(File.ReadAllBytes(exposurePath)));
        use.Dispose();

        using var reopened = fixture.Open();
        var replayBoundary = Require(CodeDocumentationMaterialBoundary.CreateFromServerOwnedCertificationScope(
            fixture.Repository, fixture.StoreId, reopened, authority, CertificationExposureRole.TeacherVisibleDiagnostic));
        using var replayUse = Require(replayBoundary.BeginDiagnosticDisclosure());
        Assert.Equal(exposureId, replayUse.ExposureIdentitySha256[7..]);
        Assert.Equal(firstAnchor, File.ReadAllBytes(fixture.Anchor));
        Assert.Equal(5, fixture.AnchorLines().Length);
        replayUse.Dispose();
        Failure(reopened.ReserveOrReplay(authority, "renamed-run"), "SOURCE_ALREADY_EXPOSED");
        using var disjointReservation = Require(reopened.ReserveOrReplay(disjoint, "disjoint-run"));
        Assert.False(disjointReservation.IsReplay);
    }

    [Fact]
    public void MaterialExposure_MissingCommittedChildFailsClosedWithoutGrantingUse()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using (var store = fixture.Create())
        {
            var boundary = Require(CodeDocumentationMaterialBoundary.CreateFromServerOwnedCertificationScope(
                fixture.Repository, fixture.StoreId, store, authority, CertificationExposureRole.TeacherVisibleDiagnostic));
            using var use = Require(boundary.BeginDiagnosticDisclosure());
            var exposureId = use.ExposureIdentitySha256[7..];
            use.Dispose();
            Directory.Delete(Path.Combine(fixture.StoreRoot, "exposures", exposureId), true);
        }

        RejectHistoryWithoutWrites(fixture, "HISTORY_INCOMPLETE");
    }

    [Fact]
    public void MaterialExposure_UnanchoredStageFailsClosed()
    {
        using var fixture = new Fixture();
        using (var store = fixture.Create()) { }
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Path.Combine(fixture.StoreRoot, "exposures"));
        RejectHistoryWithoutWrites(fixture, "HISTORY_MISMATCH");

        using var second = new Fixture();
        var authority = Issue(second.Scenario());
        using (var store = second.Create())
        {
            var boundary = Require(CodeDocumentationMaterialBoundary.CreateFromServerOwnedCertificationScope(
                second.Repository, second.StoreId, store, authority, CertificationExposureRole.TeacherVisibleDiagnostic));
            using var use = Require(boundary.BeginDiagnosticDisclosure());
            var exposureDirectory = Path.Combine(second.StoreRoot, "exposures", use.ExposureIdentitySha256[7..]);
            use.Dispose();
            File.WriteAllBytes(Path.Combine(exposureDirectory, "exposure.json.stage"), [1, 2, 3]);
        }
        RejectHistoryWithoutWrites(second, "HISTORY_MISMATCH");
    }

    [Theory]
    [InlineData("reserved")]
    [InlineData("started")]
    [InlineData("cancelled-before-start")]
    [InlineData("cancelled-after-start")]
    public void Session_IncompleteReservationNeverReopensAsClean(string cut)
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using (var store = fixture.Create())
        {
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            var clean = fixture.Snapshot();
            Assert.Throws<OperationCanceledException>(() => store.ReserveOrReplay(authority, "before", cancelled.Token));
            Assert.Equal(clean, fixture.Snapshot());
            using var reservation = Require(store.ReserveOrReplay(authority, "first"));
            if (cut == "cancelled-before-start")
                Assert.Throws<OperationCanceledException>(() => store.BeginEvaluation(reservation, cancelled.Token));
            else if (cut != "reserved")
            {
                var permit = Require(store.BeginEvaluation(reservation));
                if (cut == "cancelled-after-start")
                    Assert.Throws<OperationCanceledException>(() => store.CompleteFromServerOwnedExecution(permit, Result(authority), cancelled.Token));
            }
        }
        var retained = fixture.Snapshot();
        using var reopened = fixture.Open();
        Failure(reopened.ReserveOrReplay(authority, "first"), "INCOMPLETE_EXPOSED");
        Failure(reopened.ReserveOrReplay(authority, "renamed"), "INCOMPLETE_EXPOSED");
        Assert.Equal(retained, fixture.Snapshot());
    }

    [Theory]
    [InlineData("run")]
    [InlineData("candidate")]
    [InlineData("corpus-version")]
    [InlineData("evaluator")]
    [InlineData("generation")]
    [InlineData("scope-version")]
    [InlineData("index-version")]
    public void Session_ChangedAdmissionCannotResetSourceExposure(string field)
    {
        using var fixture = new Fixture();
        var original = fixture.Scenario();
        var first = Issue(original);
        using (var store = fixture.Create()) Require(store.ReserveOrReplay(first, "first")).Dispose();
        var changed = field switch
        {
            "candidate" => original with { Candidate = original.Candidate with { ModelVersion = "v2" } },
            "corpus-version" => original with { Opened = fixture.OpenCorpus("v2", original.Opened.Corpus.Entries) },
            "evaluator" => original with { Evaluator = Evaluator("qwen-assessment-other:9b") },
            "generation" => original with { Generation = original.Generation with { Seed = 9 } },
            "scope-version" => WithScope(original, Scope(original, version: "v2")),
            "index-version" => original with { Index = Index(original.Scope, original.Index.Sources, "v2") },
            _ => original
        };
        var second = Issue(changed);
        var admitted = Admit(second);
        Assert.Equal(field == "run", Admit(first).AdmissionIdentitySha256 == admitted.AdmissionIdentitySha256);
        var retained = fixture.Snapshot();
        using var reopened = fixture.Open();
        Failure(reopened.ReserveOrReplay(second, "renamed"), field == "run" ? "INCOMPLETE_EXPOSED" : "SOURCE_ALREADY_EXPOSED");
        Assert.Equal(retained, fixture.Snapshot());
    }

    [Theory]
    [InlineData("same-source")]
    [InlineData("normalized-content")]
    [InlineData("reverse-parent")]
    [InlineData("sibling-common-root")]
    public void Session_ExposureClosureAndContentAliasesSurviveReconstruction(string overlap)
    {
        using var fixture = new Fixture();
        var firstScenario = fixture.Scenario("a");
        var secondEntries = firstScenario.Opened.Corpus.Entries.Select((e, i) => Entry("b" + i,
            overlap == "normalized-content" ? e.OwnerInput.Replace("é", "e\u0301").Replace("\n", "\r\n") : Secret + ": other " + i)).ToArray();
        var secondScenario = fixture.Scenario("b", secondEntries);
        if (overlap == "same-source")
            secondScenario = fixture.Scenario("same", firstScenario.Opened.Corpus.Entries);
        else if (overlap == "reverse-parent")
        {
            var parent = secondScenario.Index.Sources.First();
            var nodes = firstScenario.Index.Sources.Select(n => Copy(n, ancestors: [parent.SourceEntityFingerprint])).Append(parent);
            firstScenario = firstScenario with { Index = Index(firstScenario.Scope, nodes) };
        }
        else if (overlap == "sibling-common-root")
        {
            var root = Source("shared-root");
            firstScenario = firstScenario with { Index = Index(firstScenario.Scope,
                firstScenario.Index.Sources.Select(n => Copy(n, ancestors: [root.SourceEntityFingerprint])).Append(root)) };
            secondScenario = secondScenario with { Index = Index(secondScenario.Scope,
                secondScenario.Index.Sources.Select(n => Copy(n, ancestors: [root.SourceEntityFingerprint])).Append(root)) };
        }
        var first = Issue(firstScenario);
        var second = Issue(secondScenario);
        Admit(first); Admit(second);
        using (var store = fixture.Create()) Require(store.ReserveOrReplay(first, "a")).Dispose();
        var retained = fixture.Snapshot();
        using var reopened = fixture.Open();
        Failure(reopened.ReserveOrReplay(second, "b"), "SOURCE_ALREADY_EXPOSED");
        Assert.Equal(retained, fixture.Snapshot());
    }

    [Fact]
    public void Session_DisjointSourcesWithSharedTopicAndReferenceRemainReservable()
    {
        using var fixture = new Fixture();
        var a = Issue(fixture.Scenario("a"));
        var b = Issue(fixture.Scenario("b"));
        Assert.Equal(a.Corpus.Corpus.Entries[0].PaidModelReferenceAnswer, b.Corpus.Corpus.Entries[0].PaidModelReferenceAnswer);
        Assert.Equal(a.Corpus.Corpus.SubjectId, b.Corpus.Corpus.SubjectId);
        using (var store = fixture.Create()) Require(store.ReserveOrReplay(a, "a")).Dispose();
        var initialLines = fixture.AnchorLines();
        using var reopened = fixture.Open();
        using var reservation = Require(reopened.ReserveOrReplay(b, "b"));
        Require(reopened.BeginEvaluation(reservation));
        var lines = fixture.AnchorLines();
        Assert.Equal(initialLines.Length + 2, lines.Length);
        Assert.Equal(initialLines, lines.Take(initialLines.Length));
    }

    [Fact]
    public void Session_WholeSessionDeletionAmongTwoFailsTrustedHistoryCompleteness()
    {
        using var fixture = new Fixture();
        var a = Issue(fixture.Scenario("a"));
        var b = Issue(fixture.Scenario("b"));
        using (var store = fixture.Create())
        {
            Require(store.ReserveOrReplay(a, "a")).Dispose();
            Require(store.ReserveOrReplay(b, "b")).Dispose();
        }
        using (var intact = fixture.Open()) Failure(intact.ReserveOrReplay(a, "a"), "INCOMPLETE_EXPOSED");
        using var originalCaller = fixture.Open();
        using var renamedCaller = fixture.Open();
        var anchor = File.ReadAllBytes(fixture.Anchor);
        var storeBytes = File.ReadAllBytes(fixture.StoreFile);
        var lockBytes = File.ReadAllBytes(fixture.Lock);
        fixture.DeleteSession(Admit(a).AdmissionIdentitySha256[7..]);
        Assert.Single(Directory.GetDirectories(fixture.Sessions));
        Assert.Equal(anchor, File.ReadAllBytes(fixture.Anchor));
        Assert.Equal(storeBytes, File.ReadAllBytes(fixture.StoreFile));
        Assert.Equal(lockBytes, File.ReadAllBytes(fixture.Lock));
        Failure(originalCaller.ReserveOrReplay(a, "a"), "HISTORY_INCOMPLETE");
        Failure(renamedCaller.ReserveOrReplay(a, "renamed"), "HISTORY_INCOMPLETE");
        RejectHistoryWithoutWrites(fixture, "HISTORY_INCOMPLETE");
        RejectHistoryWithoutWrites(fixture, "HISTORY_INCOMPLETE");
    }

    [Fact]
    public void Session_ValidSessionPrefixRollbackFailsAgainstRetainedCurrentAnchor()
    {
        using var fixture = new Fixture();
        var a = Issue(fixture.Scenario("a"));
        var b = Issue(fixture.Scenario("b"));
        var c = Issue(fixture.Scenario("c"));
        Dictionary<string, byte[]> prefix;
        using (var store = fixture.Create())
        {
            Require(store.ReserveOrReplay(a, "a")).Dispose();
            prefix = fixture.SessionBytes();
            Require(store.ReserveOrReplay(b, "b")).Dispose();
        }
        using (var intact = fixture.Open()) Failure(intact.ReserveOrReplay(b, "b"), "INCOMPLETE_EXPOSED");
        using var priorCaller = fixture.Open();
        using var freshCaller = fixture.Open();
        var anchor = File.ReadAllBytes(fixture.Anchor);
        fixture.DeleteSession(Admit(b).AdmissionIdentitySha256[7..]);
        Assert.Equal(prefix.Keys.Order(), fixture.SessionBytes().Keys.Order());
        foreach (var (path, bytes) in prefix) Assert.Equal(bytes, fixture.SessionBytes()[path]);
        Assert.Equal(anchor, File.ReadAllBytes(fixture.Anchor));
        Admit(c);
        Failure(priorCaller.ReserveOrReplay(b, "b"), "HISTORY_INCOMPLETE");
        Failure(freshCaller.ReserveOrReplay(c, "new-disjoint"), "HISTORY_INCOMPLETE");
        RejectHistoryWithoutWrites(fixture, "HISTORY_INCOMPLETE");
    }

    [Theory]
    [InlineData("reduced-keys")]
    [InlineData("replacement-envelope")]
    public void Session_RehashedNonCurrentExposureKeysFailAnchoredHashBeforeReuse(string mutation)
    {
        using var fixture = new Fixture();
        var aScenario = fixture.Scenario("a");
        var a = Issue(aScenario);
        var b = Issue(fixture.Scenario("b"));
        byte[]? oldEnvelope = null;
        if (mutation == "replacement-envelope")
        {
            using var oldFixture = new Fixture();
            var oldAuthority = Issue(oldFixture.Scenario("a"));
            Assert.Equal(Admit(a).AdmissionIdentitySha256, Admit(oldAuthority).AdmissionIdentitySha256);
            using var oldStore = oldFixture.Create();
            using var oldReservation = Require(oldStore.ReserveOrReplay(oldAuthority, "old-valid-reservation"));
            oldEnvelope = File.ReadAllBytes(oldFixture.Event(oldReservation.SessionId, "reservation"));
        }
        using (var store = fixture.Create())
        {
            Require(store.ReserveOrReplay(a, "a")).Dispose();
            CompleteSession(store, b);
        }
        using (var intact = fixture.Open()) Assert.True(Require(intact.ReserveOrReplay(b, "replay")).IsReplay);
        using var replayCaller = fixture.Open();
        using var overlapCaller = fixture.Open();
        var anchor = File.ReadAllBytes(fixture.Anchor);
        var bBytes = File.ReadAllBytes(fixture.Event(Admit(b).AdmissionIdentitySha256[7..], "completed"));
        var path = fixture.Event(Admit(a).AdmissionIdentitySha256[7..], "reservation");
        if (mutation == "reduced-keys") RewriteEnvelope(path, payload => payload["exposureKeys"] = new JsonArray());
        else File.WriteAllBytes(path, oldEnvelope!);
        ReadEnvelope(File.ReadAllBytes(path)); // local length/hash really pass after tamper
        Assert.Equal(anchor, File.ReadAllBytes(fixture.Anchor));
        Assert.Equal(bBytes, File.ReadAllBytes(fixture.Event(Admit(b).AdmissionIdentitySha256[7..], "completed")));
        Admit(a); Admit(b);
        var newOverlapping = Issue(aScenario with { Candidate = aScenario.Candidate with { ModelVersion = "v2" } });
        Failure(replayCaller.ReserveOrReplay(b, "replay"), "HISTORY_MISMATCH");
        Failure(overlapCaller.ReserveOrReplay(newOverlapping, "new-overlapping"), "HISTORY_MISMATCH");
        RejectHistoryWithoutWrites(fixture, "HISTORY_MISMATCH");
    }

    [Theory]
    [InlineData("missing-anchor")]
    [InlineData("partial-frame")]
    [InlineData("malformed-frame")]
    [InlineData("gap")]
    [InlineData("wrong-predecessor")]
    [InlineData("duplicate-event")]
    public void Session_AnchorCorruptionAndMissingCommitmentNeverFallBackToParsedPrefix(string fault)
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using (var store = fixture.Create()) CompleteSession(store, authority);
        using (var intact = fixture.Open()) Assert.True(Require(intact.ReserveOrReplay(authority, "replay")).IsReplay);
        var lines = fixture.AnchorLines();
        switch (fault)
        {
            case "missing-anchor": File.Delete(fixture.Anchor); break;
            case "partial-frame": File.AppendAllText(fixture.Anchor, "{partial", new UTF8Encoding(false)); break;
            case "malformed-frame": File.AppendAllText(fixture.Anchor, "{}\n", new UTF8Encoding(false)); break;
            case "gap": lines[1] = ChangeEnvelope(lines[1], p => p["sequence"] = 7); break;
            case "wrong-predecessor": lines[1] = ChangeEnvelope(lines[1], p => p["previousTipSha256"] = Hex('9')); break;
            case "duplicate-event": lines = lines.Concat([lines[1]]).ToArray(); break;
        }
        if (fault is "gap" or "wrong-predecessor" or "duplicate-event") WriteLines(fixture.Anchor, lines);
        RejectHistoryWithoutWrites(fixture, fault == "missing-anchor" ? "HISTORY_INCOMPLETE" : "HISTORY_MISMATCH");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Session_ConcurrentOwnersCannotAdmitTwoSeries(bool sameInstance)
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using var first = fixture.Create();
        using var second = sameInstance ? null : fixture.Open();
        using var barrier = new Barrier(2);
        var owners = new[] { first, second ?? first };
        var outcomes = await Task.WhenAll(owners.Select((owner, index) => Task.Run(() =>
        {
            Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(20)));
            var outcome = owner.ReserveOrReplay(authority, "race-" + index);
            // Both have returned while the winning reservation still owns the real lock.
            Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(20)));
            return outcome;
        })));
        var winner = Assert.Single(outcomes.Where(o => o.IsSuccess));
        Failure(Assert.Single(outcomes.Where(o => !o.IsSuccess)), "STORAGE_UNAVAILABLE");
        Assert.Equal(2, fixture.AnchorLines().Length);
        winner.Value!.Dispose();
        using var reopened = fixture.Open();
        Failure(reopened.ReserveOrReplay(authority, "after-race"), "INCOMPLETE_EXPOSED");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Session_FirstInitializationRaceHasOneWinnerAndPreservesWinnerBytes(bool differentStoreId)
    {
        using var fixture = new Fixture();
        using var barrier = new Barrier(2);
        var ids = new[] { fixture.StoreId, differentStoreId ? Hex('8') : fixture.StoreId };
        var outcomes = await Task.WhenAll(ids.Select(id => Task.Run(() =>
        {
            using var hook = Hook(point => { if (point == "initialization-absent") Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(20))); });
            return Store.CreateNewForServerOwnedRoot(fixture.Repository, id);
        })));
        var winnerIndex = Array.FindIndex(outcomes, o => o.IsSuccess);
        Assert.InRange(winnerIndex, 0, 1);
        Assert.Single(outcomes.Where(o => o.IsSuccess));
        Assert.Single(outcomes.Where(o => !o.IsSuccess));
        outcomes[winnerIndex].Value!.Dispose();
        var retained = fixture.Snapshot();
        using var reopened = Require(Store.OpenExistingForServerOwnedRoot(fixture.Repository, ids[winnerIndex]));
        Assert.Equal(ids[winnerIndex], ReadEnvelope(File.ReadAllBytes(fixture.StoreFile)).GetProperty("storeId").GetString());
        Assert.Equal(retained, fixture.Snapshot());
    }

    [Theory]
    [InlineData("initialization-directory")]
    [InlineData("initialization-lock")]
    [InlineData("initialization-genesis")]
    [InlineData("initialization-before-store-publish")]
    public void Session_InitializationCutsRemainNonAdoptable(string point)
    {
        using (var control = new Fixture()) { using var initialized = control.Create(); using var reopened = control.Open(); }
        using var fixture = new Fixture();
        var hit = false;
        using (Hook(p => { if (p == point) { hit = true; throw new IOException("synthetic initialization cut"); } }))
            Failure(Store.CreateNewForServerOwnedRoot(fixture.Repository, fixture.StoreId), "STORAGE_UNAVAILABLE");
        Assert.True(hit);
        Assert.True(Directory.Exists(fixture.StoreRoot));
        if (point != "initialization-directory") Assert.True(File.Exists(fixture.Lock));
        if (point is "initialization-genesis" or "initialization-before-store-publish") Assert.Single(fixture.AnchorLines());
        var retained = fixture.Snapshot();
        Failure(Store.CreateNewForServerOwnedRoot(fixture.Repository, fixture.StoreId), "STORE_EXISTS");
        Failure(Store.OpenExistingForServerOwnedRoot(fixture.Repository, fixture.StoreId),
            point == "initialization-directory" ? "STORE_INVALID" : "HISTORY_INCOMPLETE");
        Assert.Equal(retained, fixture.Snapshot());
    }

    [Fact]
    public void Session_MissingExpectedStoreCannotBecomeFreshHistory()
    {
        using var fixture = new Fixture();
        using (fixture.Create()) { }
        Assert.True(File.Exists(fixture.InitializationMarker));

        Directory.Delete(fixture.StoreRoot, recursive: true);

        Failure(
            Store.CreateNewForServerOwnedRoot(fixture.Repository, fixture.StoreId),
            "HISTORY_EXPECTED");
        Assert.False(Directory.Exists(fixture.StoreRoot));
        Assert.True(File.Exists(fixture.InitializationMarker));
    }

    [Fact]
    public void Session_ServerOwnedBootstrapReopensCompleteStoreWhenMarkerWinsCreateRace()
    {
        using var fixture = new Fixture();
        var competingInitializer = false;
        string[]? winnerSnapshot = null;
        using var reopened = Program.CreateServerOwnedMaterialBoundaryStore(
            fixture.Repository,
            fixture.StoreId,
            () =>
            {
                using var winner = Require(
                    Store.CreateNewForServerOwnedRoot(fixture.Repository, fixture.StoreId));
                competingInitializer = true;
                winnerSnapshot = fixture.Snapshot();
            });

        Assert.True(competingInitializer);
        Assert.True(
            reopened.ValidateOwnerBinding(fixture.Repository, fixture.StoreId).IsSuccess);
        Assert.Equal(winnerSnapshot, fixture.Snapshot());
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("corrupt")]
    public void Session_InitializationMarkerMustBePresentAndExact(string fault)
    {
        using var fixture = new Fixture();
        using (fixture.Create()) { }

        if (fault == "missing")
            File.Delete(fixture.InitializationMarker);
        else
            File.WriteAllText(fixture.InitializationMarker, "foreign-marker");

        Failure(
            Store.OpenExistingForServerOwnedRoot(fixture.Repository, fixture.StoreId),
            fault == "missing" ? "HISTORY_INCOMPLETE" : "HISTORY_MISMATCH");
    }

    [Fact]
    public void MaterialBoundary_ServerOwnedLifetimeDisposesStoreExactlyOnce()
    {
        using var fixture = new Fixture();
        using var store = fixture.Create();
        var provider = Require(CodeDocumentationServerOwnedMaterialBoundaryProvider.Create(
            fixture.Repository,
            fixture.StoreId,
            store));
        var lifetime = new CodeDocumentationServerOwnedMaterialBoundaryLifetime(
            provider,
            store);

        lifetime.Dispose();
        lifetime.Dispose();

        var disposedStoreResult = store.ValidateOwnerBinding(
            fixture.Repository,
            fixture.StoreId);
        Assert.False(disposedStoreResult.IsSuccess);
        Assert.Equal(
            Store.ErrorPrefix + "STORAGE_UNAVAILABLE",
            disposedStoreResult.ErrorCode);
        Assert.False(disposedStoreResult.Value);
    }

    [Theory]
    [InlineData("no-start")]
    [InlineData("other-store")]
    [InlineData("other-reservation")]
    [InlineData("disposed")]
    [InlineData("duplicate-completion")]
    public void Session_InvalidOrStalePermitCannotComplete(string scenario)
    {
        using var fixture = new Fixture();
        var a = Issue(fixture.Scenario("a"));
        var b = Issue(fixture.Scenario("b"));
        using var store = fixture.Create();
        var reservation = Require(store.ReserveOrReplay(a, "a"));
        var permit = scenario == "no-start" ? CodeDocumentationCertificationSessionStartPermit.Mint() : Require(store.BeginEvaluation(reservation));
        CodeDocumentationCertificationSessionStore target = store;
        CodeDocumentationCertificationSessionStore? other = null;
        CodeDocumentationCertificationSessionReservation? newReservation = null;
        try
        {
            if (scenario == "duplicate-completion") Require(store.CompleteFromServerOwnedExecution(permit, Result(a)));
            else if (scenario is "other-store" or "other-reservation" or "disposed")
            {
                reservation.Dispose();
                if (scenario == "other-store")
                {
                    target = other = fixture.Open();
                    newReservation = Require(target.ReserveOrReplay(b, "b"));
                    Require(target.BeginEvaluation(newReservation));
                }
                else if (scenario == "other-reservation")
                { newReservation = Require(store.ReserveOrReplay(b, "b")); Require(store.BeginEvaluation(newReservation)); }
                else store.Dispose();
            }
            var retained = fixture.Snapshot();
            Failure(target.CompleteFromServerOwnedExecution(permit, Result(a)), "INVALID_PERMIT");
            Assert.Equal(retained, fixture.Snapshot());
        }
        finally { newReservation?.Dispose(); other?.Dispose(); reservation.Dispose(); }
    }

    [Theory]
    [InlineData("missing-repeat")]
    [InlineData("wrong-cases")]
    [InlineData("wrong-count")]
    [InlineData("duplicate-response-id")]
    [InlineData("wrong-evaluator")]
    [InlineData("foreign-evaluation-set")]
    [InlineData("wrong-generation")]
    [InlineData("wrong-candidate")]
    [InlineData("mutated-model")]
    [InlineData("inconsistent-score")]
    [InlineData("foreign-noise")]
    [InlineData("changed-numeric-passport")]
    [InlineData("self-consistent-foreign-passport")]
    [InlineData("representative-failed")]
    [InlineData("calibration-failed")]
    public void Session_CompletionRequiresFullBoundSeries(string change)
    {
        using var fixture = new Fixture();
        var control = Issue(fixture.Scenario("control"));
        var authority = Issue(fixture.Scenario("negative"));
        using var store = fixture.Create();
        CompleteSession(store, control);
        using var reservation = Require(store.ReserveOrReplay(authority, "negative"));
        var permit = Require(store.BeginEvaluation(reservation));
        var valid = Result(authority);
        var altered = Mutate(valid, change);
        var retained = fixture.Snapshot();
        Failure(store.CompleteFromServerOwnedExecution(permit, altered), "COMPLETION_INVALID");
        Assert.Equal(retained, fixture.Snapshot());
        Assert.False(File.Exists(fixture.Event(reservation.SessionId, "completed")));
        using var reopened = fixture.Open();
        Failure(reopened.ReserveOrReplay(authority, "renamed"), "INCOMPLETE_EXPOSED");
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("truncated")]
    [InlineData("oversize")]
    [InlineData("unsupported-version")]
    [InlineData("duplicate-field")]
    [InlineData("missing-field")]
    [InlineData("unknown-field")]
    [InlineData("invalid-utf8")]
    [InlineData("trailing-json")]
    [InlineData("wrong-length")]
    [InlineData("wrong-hash")]
    [InlineData("missing-parent-event")]
    [InlineData("wrong-parent-hash")]
    [InlineData("orphan-completion")]
    [InlineData("leftover-stage")]
    [InlineData("empty-session-directory")]
    [InlineData("rehashed-foreign-tuple")]
    public void Session_ParserRejectsInvalidOrTornState(string fault)
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using (var store = fixture.Create()) CompleteSession(store, authority);
        using (var intact = fixture.Open()) Assert.True(Require(intact.ReserveOrReplay(authority, "control")).IsReplay);
        var id = Admit(authority).AdmissionIdentitySha256[7..];
        var path = fixture.Event(id, "completed");
        var original = File.ReadAllBytes(path);
        switch (fault)
        {
            case "empty": File.WriteAllBytes(path, []); break;
            case "truncated": File.WriteAllBytes(path, original[..^7]); break;
            case "oversize": File.WriteAllBytes(path, new byte[Store.MaximumEventBytes + 1]); break;
            case "unsupported-version": RewriteEnvelope(path, p => p["formatVersion"] = 2); break;
            case "duplicate-field": File.WriteAllText(path, Encoding.UTF8.GetString(original).Replace("\"formatVersion\":1", "\"formatVersion\":1,\"formatVersion\":1")); break;
            case "missing-field": RewriteEnvelope(path, p => p.Remove("receipt")); break;
            case "unknown-field": RewriteEnvelope(path, p => p["extension"] = "not-admitted"); break;
            case "invalid-utf8": File.WriteAllBytes(path, [0xc3, 0x28]); break;
            case "trailing-json": File.WriteAllBytes(path, original.Concat(Encoding.UTF8.GetBytes("{}" )).ToArray()); break;
            case "wrong-length": RewriteRaw(path, p => p["payloadByteLength"] = 1); break;
            case "wrong-hash": RewriteRaw(path, p => p["payloadSha256"] = Hex('0')); break;
            case "missing-parent-event": File.Delete(fixture.Event(id, "started")); break;
            case "wrong-parent-hash": RewriteEnvelope(path, p => p["startedEnvelopeSha256"] = Hex('0')); break;
            case "orphan-completion": File.Delete(fixture.Event(id, "reservation")); File.Delete(fixture.Event(id, "started")); break;
            case "leftover-stage": File.WriteAllBytes(path + ".stage", original); break;
            case "empty-session-directory": SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Path.Combine(fixture.Sessions, Hex('0'))); break;
            case "rehashed-foreign-tuple": RewriteEnvelope(path, p => p["storeId"] = Hex('8')); break;
        }
        var retained = fixture.Snapshot();
        var rejected = Store.OpenExistingForServerOwnedRoot(fixture.Repository, fixture.StoreId);
        Assert.False(rejected.IsSuccess, fault);
        Assert.Null(rejected.Value);
        Assert.Equal(Store.ErrorPrefix + (fault is "oversize" or "leftover-stage" ? "LIMIT_EXCEEDED" :
            fault is "missing-parent-event" or "orphan-completion" ? "HISTORY_INCOMPLETE" : "HISTORY_MISMATCH"), rejected.ErrorCode);
        Assert.Equal(retained, fixture.Snapshot());
    }

    [Theory]
    [InlineData("wrong-store-id")]
    [InlineData("missing-existing-root")]
    [InlineData("foreign-root")]
    [InlineData("traversal")]
    [InlineData("reparse")]
    [InlineData("unsafe-custody")]
    public void Session_RootBindingAndPrivateCustodyFailClosed(string fault)
    {
        using var fixture = new Fixture();
        using (var store = fixture.Create()) { }
        using (var reopened = fixture.Open()) { }
        var retained = fixture.Snapshot();
        if (fault == "wrong-store-id")
            Failure(Store.OpenExistingForServerOwnedRoot(fixture.Repository, Hex('8')), "STORE_INVALID");
        else if (fault == "missing-existing-root")
        {
            using var missing = new Fixture(createModelRoot: false);
            Failure(Store.CreateNewForServerOwnedRoot(missing.Repository, missing.StoreId), "ROOT_INVALID");
        }
        else if (fault == "foreign-root")
        {
            using var foreign = new Fixture();
            fixture.CopyStoreTo(foreign);
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(foreign.StoreRoot));
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(Path.GetDirectoryName(foreign.Anchor)!));
            Assert.All(Directory.GetFiles(foreign.StoreRoot, "*", SearchOption.AllDirectories),
                path => Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path)));
            Assert.Equal(File.ReadAllBytes(fixture.Anchor), File.ReadAllBytes(foreign.Anchor));
            Failure(Store.OpenExistingForServerOwnedRoot(foreign.Repository, fixture.StoreId), "HISTORY_MISMATCH");
        }
        else if (fault == "traversal")
            Failure(Store.OpenExistingForServerOwnedRoot(Path.Combine(fixture.Repository, "..", "repo"), fixture.StoreId), "ROOT_INVALID");
        else if (fault == "reparse")
        {
            var link = Path.Combine(fixture.OwnRoot, "linked-repo");
            Directory.CreateSymbolicLink(link, fixture.Repository);
            Assert.True(SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(link));
            Failure(Store.OpenExistingForServerOwnedRoot(link, fixture.StoreId), "ROOT_INVALID");
            Directory.Delete(link);
        }
        else
        {
            var directory = new DirectoryInfo(fixture.StoreRoot);
            if (OperatingSystem.IsWindows())
            {
                var security = directory.GetAccessControl();
                security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.Read, AccessControlType.Allow));
                directory.SetAccessControl(security);
            }
            else File.SetUnixFileMode(fixture.StoreRoot, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.OtherRead);
            Assert.False(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(fixture.StoreRoot));
            Failure(Store.OpenExistingForServerOwnedRoot(fixture.Repository, fixture.StoreId), "STORE_INVALID");
        }
        Assert.Equal(retained, fixture.Snapshot());
        Failure(Store.CreateNewForServerOwnedRoot(fixture.Repository, fixture.StoreId), "STORE_EXISTS");
    }

    [Theory]
    [InlineData("reservation", "stage-closed")]
    [InlineData("reservation", "anchor-partial")]
    [InlineData("reservation", "anchor-committed")]
    [InlineData("reservation", "event-published")]
    [InlineData("started", "stage-closed")]
    [InlineData("started", "anchor-partial")]
    [InlineData("started", "anchor-committed")]
    [InlineData("started", "event-published")]
    [InlineData("completed", "stage-closed")]
    [InlineData("completed", "anchor-partial")]
    [InlineData("completed", "anchor-committed")]
    [InlineData("completed", "event-published")]
    [InlineData("completed", "before-return")]
    public void Session_PublicationCutsNeverGrantUnvalidatedPermitOrEraseCompletion(string stage, string cut)
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using var store = fixture.Create();
        CodeDocumentationCertificationSessionReservation? reservation = null;
        CodeDocumentationCertificationSessionStartPermit? permit = null;
        if (stage != "reservation") reservation = Require(store.ReserveOrReplay(authority, "cut"));
        if (stage == "completed") permit = Require(store.BeginEvaluation(reservation!));
        var expected = cut == "before-return" ? "completion-before-return" : stage + "-" + cut;
        var hit = false;
        using (Hook(point =>
        {
            if (point != expected) return;
            hit = true;
            if (cut == "stage-closed")
            {
                // Реальное exclusive открытие доказывает закрытый writer, не изменение sharing.
                var id = Admit(authority).AdmissionIdentitySha256[7..];
                using var probe = new FileStream(fixture.Event(id, stage) + ".stage", FileMode.Open, FileAccess.Read, FileShare.None);
                Assert.True(probe.Length > 0);
            }
            throw new IOException("synthetic publication cut");
        }))
        {
            if (stage == "reservation") Failure(store.ReserveOrReplay(authority, "cut"), "STORAGE_UNAVAILABLE");
            else if (stage == "started") Failure(store.BeginEvaluation(reservation!), "STORAGE_UNAVAILABLE");
            else Failure(store.CompleteFromServerOwnedExecution(permit!, Result(authority)), "STORAGE_UNAVAILABLE");
        }
        Assert.True(hit);
        reservation?.Dispose();
        var retained = fixture.Snapshot();
        var open = Store.OpenExistingForServerOwnedRoot(fixture.Repository, fixture.StoreId);
        if (cut is "event-published" or "before-return")
        {
            using var reopened = Require(open);
            var outcome = reopened.ReserveOrReplay(authority, "renamed");
            if (stage == "completed") Assert.True(Require(outcome).IsReplay);
            else Failure(outcome, "INCOMPLETE_EXPOSED");
        }
        else Failure(open, cut == "anchor-committed" ? "HISTORY_INCOMPLETE" : "HISTORY_MISMATCH");
        Assert.Equal(retained, fixture.Snapshot());
    }

    [Fact]
    public void Session_ReceiptAndPassportCollectionsAreImmutableSnapshots()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using var store = fixture.Create();
        using var reservation = Require(store.ReserveOrReplay(authority, "mutable-inputs"));
        var permit = Require(store.BeginEvaluation(reservation));
        var input = Result(authority);
        var receipt = Require(store.CompleteFromServerOwnedExecution(permit, input));
        var text = Serialize(receipt);
        var retained = fixture.Snapshot();
        ((double[])input.Calibration.MeasurementNoisePassport.RepeatedScores)[0] = .25;
        ((string[])input.Calibration.Observations[0].CaseIds)[0] = "changed";
        ((string[])input.Calibration.Observations[0].ObservedEvaluationRequestIds)[0] = "changed";
        ((CodeDocumentationSavedModelAssessmentCaseResult[])input.RepresentativeAssessment.Cases)[0] = input.RepresentativeAssessment.Cases[0] with { Passed = false };
        Assert.Equal(text, Serialize(receipt));
        AssertReadOnly(receipt.Observations);
        AssertReadOnly(receipt.Observations[0].CaseIds);
        AssertReadOnly(receipt.Observations[0].StudentAnswerSha256);
        AssertReadOnly(receipt.Observations[0].ObservedEvaluationRequestIds);
        AssertReadOnly(receipt.Representative.Cases);
        AssertReadOnly(receipt.Representative.Evaluator.ObservedEvaluationRequestIds);
        AssertReadOnly(receipt.MeasurementNoisePassport.RepeatedScores);
        using var reopened = fixture.Open();
        var replay = Require(reopened.ReserveOrReplay(authority, "new-run"));
        Assert.Equal(text, Serialize(replay.Receipt));
        AssertReadOnly(replay.Receipt!.MeasurementNoisePassport.RepeatedScores);
        Assert.Equal(retained, fixture.Snapshot());
    }

    [Fact]
    public void Session_MetadataAndErrorsNeverPersistClosedPayloads()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using var store = fixture.Create();
        var receipt = CompleteSession(store, authority);
        var error = store.ReserveOrReplay(null!, Secret);
        Assert.False(error.IsSuccess);
        Assert.DoesNotContain(Secret, error.ErrorMessage, StringComparison.Ordinal);
        Assert.DoesNotContain(Secret, Serialize(receipt), StringComparison.Ordinal);
        foreach (var path in Directory.GetFiles(fixture.StoreRoot, "*", SearchOption.AllDirectories))
            Assert.DoesNotContain(Secret, File.ReadAllText(path), StringComparison.Ordinal);
        Assert.DoesNotContain("humanMessage", Serialize(receipt), StringComparison.Ordinal);
        Assert.DoesNotContain("studentAnswerPreview", Serialize(receipt), StringComparison.Ordinal);
    }

    [Fact]
    public void Session_InputByteBoundsRejectWithoutPartialSnapshotOrHistoryEviction()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using var store = fixture.Create();
        Require(store.ReserveOrReplay(authority, new string('x', 1024))).Dispose();
        var retained = fixture.Snapshot();
        Failure(store.ReserveOrReplay(authority, new string('x', 1025)), "INPUT_INVALID");
        Assert.Equal(retained, fixture.Snapshot());
        using var reopen = fixture.Open();
        Failure(reopen.ReserveOrReplay(authority, "never-evict"), "INCOMPLETE_EXPOSED");

        using (var utf8Fixture = new Fixture())
        using (var utf8Store = utf8Fixture.Create())
        {
            var exactUtf8Authority = Issue(utf8Fixture.Scenario("utf8-exact"));
            Require(utf8Store.ReserveOrReplay(exactUtf8Authority, new string('é', 512))).Dispose();
            var utf8Retained = utf8Fixture.Snapshot();
            var overLimitUtf8Authority = Issue(utf8Fixture.Scenario("utf8-over"));
            Failure(utf8Store.ReserveOrReplay(overLimitUtf8Authority, new string('é', 513)), "INPUT_INVALID");
            Assert.Equal(utf8Retained, utf8Fixture.Snapshot());
        }
    }

    [Fact]
    public void Session_HeldOutCaseBoundsRejectWithoutPartialSnapshotOrHistoryEviction()
    {
        using var fixture = new Fixture();
        var large = fixture.Scenario("h200", Enumerable.Range(0, 200).Select(i => Entry("large-" + i, Secret + ": large " + i)));
        var largeAuthority = Issue(large);
        using var store = fixture.Create();
        Assert.Equal(1000, Admit(largeAuthority).ExpectedEvaluationCallCount);
        var receipt = CompleteSession(store, largeAuthority);
        Assert.Equal(200, receipt.HeldOutCaseCount);
        Assert.Equal(1000, receipt.ObservedEvaluationCallCount);

        var extra = fixture.Scenario("h201", Enumerable.Range(0, 201).Select(i => Entry("over-" + i, Secret + ": over " + i)));
        var beforeExtra = fixture.Snapshot();
        var extraAuthority = ServerOwnedCodeDocumentationCertificationScope.CreateFromServerOwnedSnapshot(extra.Scope, extra.Candidate,
            extra.Opened, extra.Generation, extra.Evaluator, MeasurementNoisePolicyConfig.Canonical, extra.Index);
        Assert.False(extraAuthority.IsSuccess); // Real calibration admission refuses H201; no authority is fabricated.
        Assert.Equal(CodeDocumentationCertificationAdmission.ErrorPrefix + "CONDITIONS_MISMATCH", extraAuthority.ErrorCode);
        Assert.Equal(beforeExtra, fixture.Snapshot());
    }

    [Fact]
    public void Session_SourceExposureBoundsRejectWithoutPartialSnapshotOrHistoryEviction()
    {
        using var fixture = new Fixture();
        using var store = fixture.Create();
        var wide = fixture.Scenario("source-limit");
        var ancestors = Enumerable.Range(0, 9998).Select(i => Source("ancestor-" + i)).ToArray();
        var closure = wide.Index.Sources.Select(n => Copy(n, ancestors.Select(a => a.SourceEntityFingerprint))).Concat(ancestors).ToArray();
        wide = wide with { Index = Index(wide.Scope, closure) };
        var wideAuthority = Issue(wide);
        using (var reservation = Require(store.ReserveOrReplay(wideAuthority, "exact-source-bound")))
        {
            var payload = ReadEnvelope(File.ReadAllBytes(fixture.Event(reservation.SessionId, "reservation")));
            Assert.Equal(10000, payload.GetProperty("exposureKeys").EnumerateArray().Count(e => e.GetProperty("kind").GetString() == "source"));
            Assert.Equal(2, payload.GetProperty("exposureKeys").EnumerateArray().Count(e => e.GetProperty("kind").GetString() == "content"));
        }
        var beforeInvalidAuthority = fixture.Snapshot();
        var tooMany = Index(wide.Scope, closure.Append(Source("one-too-many")));
        var rejectedIssuer = ServerOwnedCodeDocumentationCertificationScope.CreateFromServerOwnedSnapshot(wide.Scope, wide.Candidate,
            wide.Opened, wide.Generation, wide.Evaluator, MeasurementNoisePolicyConfig.Canonical, tooMany);
        Assert.False(rejectedIssuer.IsSuccess); // The real07 bound rejects10001 before a store authority can exist.
        Assert.Equal(CodeDocumentationCertificationAdmission.ErrorPrefix + "INPUT_INVALID", rejectedIssuer.ErrorCode);
        Assert.Equal(beforeInvalidAuthority, fixture.Snapshot());
    }

    [Theory]
    [InlineData("cases-count")]
    [InlineData("cases-lying")]
    [InlineData("observations-count")]
    [InlineData("requests-lying")]
    [InlineData("throwing-enumerator")]
    [InlineData("cancelled-enumerator")]
    [InlineData("wrapped-cancellation-json")]
    [InlineData("wrapped-fatal-decoder")]
    public void Session_RuntimeCollectionsAreBoundedAndPrimaryExceptionsSurvive(string fault)
    {
        using var fixture = new Fixture();
        var good = Issue(fixture.Scenario("good"));
        var authority = Issue(fixture.Scenario("negative"));
        using var store = fixture.Create();
        CompleteSession(store, good);
        using var reservation = Require(store.ReserveOrReplay(authority, "negative"));
        var permit = Require(store.BeginEvaluation(reservation));
        var input = Result(authority);
        var before = fixture.Snapshot();
        Exception? primary = fault switch
        {
            "throwing-enumerator" => new InvalidOperationException("synthetic enumeration failure"),
            "cancelled-enumerator" => new OperationCanceledException("synthetic primary cancellation"),
            "wrapped-cancellation-json" => new JsonException("synthetic wrapper", new OperationCanceledException("primary")),
            "wrapped-fatal-decoder" => new DecoderFallbackException("synthetic wrapper", new OutOfMemoryException("primary")),
            _ => null
        };
        var cases = new AdversarialList<CodeDocumentationSavedModelAssessmentCaseResult>(input.RepresentativeAssessment.Cases.ToArray(),
            fault == "cases-count" ? 201 : 2, fault == "cases-lying", primary);
        var requests = new AdversarialList<string>(["one", "two"], 2, endless: true);
        var observations = new AdversarialList<CodeDocumentationSavedModelAssessmentCalibrationObservation>(input.Calibration.Observations.ToArray(), 6);
        if (fault == "requests-lying")
            input = input with { RepresentativeAssessment = input.RepresentativeAssessment with
                { Evaluator = input.RepresentativeAssessment.Evaluator with { ObservedEvaluationRequestIds = requests } } };
        else if (fault == "observations-count")
            input = input with { Calibration = input.Calibration with { Observations = observations } };
        else input = input with { RepresentativeAssessment = input.RepresentativeAssessment with { Cases = cases } };
        if (fault is "cancelled-enumerator" or "wrapped-cancellation-json" or "wrapped-fatal-decoder")
        {
            var actual = Record.Exception(() => store.CompleteFromServerOwnedExecution(permit, input));
            Assert.Same(primary, actual);
            Assert.Equal(1, cases.DisposeCalls);
        }
        else Failure(store.CompleteFromServerOwnedExecution(permit, input), "COMPLETION_INVALID");
        if (fault == "cases-count") Assert.Equal(0, cases.EnumeratorCalls);
        if (fault == "observations-count") Assert.Equal(0, observations.EnumeratorCalls);
        if (fault == "cases-lying") Assert.Equal(201, cases.MoveCalls);
        if (fault == "requests-lying") Assert.Equal(201, requests.MoveCalls);
        Assert.Equal(before, fixture.Snapshot());
        using var reopened = fixture.Open();
        Failure(reopened.ReserveOrReplay(authority, "renamed"), "INCOMPLETE_EXPOSED");
    }

    [Theory]
    [InlineData("root")]
    [InlineData("anchor")]
    [InlineData("sessions")]
    [InlineData("events")]
    public void Session_DirectoryEnumerationRejectsTheFirstExcessEntry(string level)
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using (var store = fixture.Create()) CompleteSession(store, authority);
        using (var intact = fixture.Open()) Assert.True(Require(intact.ReserveOrReplay(authority, "positive")).IsReplay);
        if (level == "sessions")
        {
            for (var i = 0; i < Store.MaximumSessions; i++)
                SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Path.Combine(fixture.Sessions, i.ToString("x64")));
            Assert.Equal(Store.MaximumSessions + 1, Directory.GetDirectories(fixture.Sessions).Length);
        }
        else
        {
            if (level == "root")
            {
                using (var exposureStore = fixture.Open())
                {
                    var boundary = Require(CodeDocumentationMaterialBoundary.CreateFromServerOwnedCertificationScope(
                        fixture.Repository, fixture.StoreId, exposureStore, authority,
                        CertificationExposureRole.TeacherVisibleDiagnostic));
                    using var use = Require(boundary.BeginDiagnosticDisclosure());
                }
            }
            var parent = level switch
            {
                "root" => fixture.StoreRoot,
                "anchor" => Path.GetDirectoryName(fixture.Anchor)!,
                _ => Path.GetDirectoryName(fixture.Event(Admit(authority).AdmissionIdentitySha256[7..], "completed"))!
            };
            File.WriteAllText(Path.Combine(parent, "extra"), "synthetic excess entry");
            Assert.Equal(level == "root" ? 6 : level == "anchor" ? 2 : 4, Directory.GetFileSystemEntries(parent).Length);
        }
        RejectHistoryWithoutWrites(fixture, "LIMIT_EXCEEDED");
    }

    [Theory]
    [InlineData("duplicate-field")]
    [InlineData("unknown-field")]
    [InlineData("missing-field")]
    [InlineData("unsupported-version")]
    [InlineData("invalid-utf8")]
    [InlineData("trailing-json")]
    [InlineData("wrong-length")]
    [InlineData("wrong-hash")]
    [InlineData("noncanonical-whitespace")]
    public void Session_AnchorStrictParserRejectsMalformedFramesAfterValidHistory(string fault)
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using (var store = fixture.Create()) CompleteSession(store, authority);
        using (var intact = fixture.Open()) Assert.True(Require(intact.ReserveOrReplay(authority, "control")).IsReplay);
        var originalEvents = fixture.SessionBytes();
        var lines = fixture.AnchorLines();
        var rawPayload = ReadEnvelope(Encoding.UTF8.GetBytes(lines[0])).GetRawText();
        switch (fault)
        {
            case "duplicate-field":
                lines[0] = EnvelopeFromRawPayload(rawPayload.Replace("\"formatVersion\":1", "\"formatVersion\":1,\"formatVersion\":1")); break;
            case "unknown-field": lines[0] = ChangeEnvelope(lines[0], p => p["extension"] = "not-admitted"); break;
            case "missing-field": lines[0] = ChangeEnvelope(lines[0], p => p.Remove("policyId")); break;
            case "unsupported-version": lines[0] = ChangeEnvelope(lines[0], p => p["formatVersion"] = 2); break;
            case "trailing-json": lines[0] += "{}"; break;
            case "wrong-length":
            case "wrong-hash":
                var node = JsonNode.Parse(lines[0])!.AsObject();
                if (fault == "wrong-length") node["payloadByteLength"] = 1;
                else node["payloadSha256"] = Hex('0');
                lines[0] = node.ToJsonString(Json); break;
            case "noncanonical-whitespace": lines[0] = " " + lines[0]; break;
        }
        WriteLines(fixture.Anchor, lines);
        if (fault == "invalid-utf8")
        {
            var bytes = File.ReadAllBytes(fixture.Anchor);
            bytes[0] = 0xff;
            File.WriteAllBytes(fixture.Anchor, bytes);
        }
        // We intentionally corrupt the synthetic trusted input to reach its strict parser;
        // this is not evidence of detecting a valid joint rollback of anchor and data.
        var read = 0;
        var parsed = 0;
        var retained = fixture.Snapshot();
        using (Hook(point => { if (point == "history-bytes-read") read++; if (point == "history-genesis-parsed") parsed++; }))
            Failure(Store.OpenExistingForServerOwnedRoot(fixture.Repository, fixture.StoreId), "HISTORY_MISMATCH");
        Assert.Equal(1, read);
        Assert.Equal(fault == "unsupported-version" ? 1 : 0, parsed);
        Assert.Equal(retained, fixture.Snapshot());
        foreach (var (path, bytes) in originalEvents) Assert.Equal(bytes, fixture.SessionBytes()[path]);
    }

    private static string EnvelopeFromRawPayload(string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        return "{\"schema\":\"code-documentation-certification-history-envelope-v1\",\"payloadByteLength\":" + bytes.Length +
            ",\"payloadSha256\":\"" + Hash(bytes) + "\",\"payload\":" + payload + "}";
    }

    private sealed class AdversarialList<T>(T[] values, int count, bool endless = false, Exception? primary = null) : IReadOnlyList<T>
    {
        private readonly T[] _values = values;
        private readonly bool _endless = endless;
        private readonly Exception? _primary = primary;
        internal int EnumeratorCalls { get; private set; }
        internal int MoveCalls { get; private set; }
        internal int DisposeCalls { get; private set; }
        public int Count => count;
        public T this[int index] => values[index];
        public IEnumerator<T> GetEnumerator() { EnumeratorCalls++; return new Iterator(this); }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        private sealed class Iterator(AdversarialList<T> owner) : IEnumerator<T>
        {
            private int _position = -1;
            public T Current => owner._values[_position % owner._values.Length];
            object? IEnumerator.Current => Current;
            public bool MoveNext()
            {
                owner.MoveCalls++;
                if (owner._primary is not null) throw owner._primary;
                _position++;
                return owner._endless || _position < owner._values.Length;
            }
            public void Reset() => throw new NotSupportedException();
            public void Dispose()
            {
                owner.DisposeCalls++;
                if (owner._primary is not null) throw new InvalidOperationException("synthetic secondary disposal failure");
            }
        }
    }

    private static void AssertStaleAnchorFailsBeforeAppend()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using var store = fixture.Create();
        var genesis = File.ReadAllBytes(fixture.Anchor);
        using var reservation = Require(store.ReserveOrReplay(authority, "stale-check"));
        var reservationBytes = File.ReadAllBytes(fixture.Event(reservation.SessionId, "reservation"));
        var hit = false;
        using (Hook(point =>
        {
            if (point != "started-stage-closed") return;
            hit = true;
            File.WriteAllBytes(fixture.Anchor, genesis);
        }))
            Failure(store.BeginEvaluation(reservation), "HISTORY_MISMATCH");
        Assert.True(hit);
        Assert.Equal(genesis, File.ReadAllBytes(fixture.Anchor));
        Assert.Equal(reservationBytes, File.ReadAllBytes(fixture.Event(reservation.SessionId, "reservation")));
        Assert.False(File.Exists(fixture.Event(reservation.SessionId, "started")));
        RejectHistoryWithoutWrites(fixture, "HISTORY_MISMATCH");
    }

    [Fact]
    public void Session_PerformanceDiagnosticReportsBoundedPhaseRows()
    {
        const long phaseDeadlineMilliseconds = 120_000;
        const long overallDeadlineMilliseconds = 600_000;
        var overall = System.Diagnostics.Stopwatch.StartNew();
        var sampleSizes = new[] { 1, 2, 4, 8, 16 };

        void Emit(string phase, int sampleSize, int completedIterations, int fixtureCalls,
            int storeOperationCalls, int reopenCalls, int anchorReadCalls,
            long elapsedMilliseconds, string status, string failureCode = "")
        {
            var row = new
            {
                phase,
                sample_size = sampleSize,
                completed_iterations = completedIterations,
                fixture_calls = fixtureCalls,
                store_operation_calls = storeOperationCalls,
                reopen_calls = reopenCalls,
                anchor_read_calls = anchorReadCalls,
                elapsed_ms = elapsedMilliseconds,
                deadline_ms = phaseDeadlineMilliseconds,
                status,
                failure_code = failureCode
            };
            if (status == "completed")
                Assert.InRange(elapsedMilliseconds, 0L, phaseDeadlineMilliseconds);
            _output.WriteLine("performance-diagnostic-row=" + JsonSerializer.Serialize(row, Json));
        }

        void CheckDeadline(string phase, int sampleSize, int completedIterations, int fixtureCalls,
            int storeOperationCalls, int reopenCalls, int anchorReadCalls,
            System.Diagnostics.Stopwatch phaseTimer)
        {
            var elapsed = phaseTimer.ElapsedMilliseconds;
            if (elapsed > phaseDeadlineMilliseconds || overall.ElapsedMilliseconds > overallDeadlineMilliseconds)
            {
                var code = overall.ElapsedMilliseconds > overallDeadlineMilliseconds
                    ? "OVERALL_DEADLINE_EXCEEDED" : "PHASE_DEADLINE_EXCEEDED";
                Emit(phase, sampleSize, completedIterations, fixtureCalls, storeOperationCalls,
                    reopenCalls, anchorReadCalls, elapsed, "deadline_exceeded", code);
                throw new Xunit.Sdk.XunitException($"Performance diagnostic deadline exceeded: {phase}; completed={completedIterations}; elapsedMs={elapsed}; code={code}.");
            }
        }

        foreach (var sampleSize in sampleSizes)
        {
            using var fixture = new Fixture();
            var phaseTimer = System.Diagnostics.Stopwatch.StartNew();
            var completed = 0;
            for (var i = 0; i < sampleSize; i++)
            {
                _ = fixture.Scenario($"performance-setup-{sampleSize}-{i}");
                completed++;
                CheckDeadline("fixture-scenario", sampleSize, completed, completed, 0, 0, 0, phaseTimer);
            }
            phaseTimer.Stop();
            Emit("fixture-scenario", sampleSize, completed, completed, 0, 0, 0,
                phaseTimer.ElapsedMilliseconds, "completed");
        }

        foreach (var sampleSize in sampleSizes)
        {
            using var fixture = new Fixture();
            var authorities = Enumerable.Range(0, sampleSize)
                .Select(i => Issue(fixture.Scenario($"performance-reservation-{sampleSize}-{i}")))
                .ToArray();
            using var store = fixture.Create();
            var phaseTimer = System.Diagnostics.Stopwatch.StartNew();
            var completed = 0;
            for (var i = 0; i < authorities.Length; i++)
            {
                using (var reservation = Require(store.ReserveOrReplay(authorities[i], $"performance-reservation-{sampleSize}-{i}"))) { }
                completed++;
                CheckDeadline("reservation-store", sampleSize, completed, 0, completed, 0, 0, phaseTimer);
            }
            phaseTimer.Stop();
            Emit("reservation-store", sampleSize, completed, 0, completed, 0, 0,
                phaseTimer.ElapsedMilliseconds, "completed");
        }

        foreach (var sampleSize in sampleSizes)
        {
            using var fixture = new Fixture();
            var authorities = Enumerable.Range(0, sampleSize)
                .Select(i => Issue(fixture.Scenario($"performance-completion-{sampleSize}-{i}")))
                .ToArray();
            using var store = fixture.Create();
            var phaseTimer = System.Diagnostics.Stopwatch.StartNew();
            var completed = 0;
            for (var i = 0; i < authorities.Length; i++)
            {
                CompleteSession(store, authorities[i]);
                completed++;
                CheckDeadline("completion-store", sampleSize, completed, 0, completed * 3, 0, 0, phaseTimer);
            }
            phaseTimer.Stop();
            Emit("completion-store", sampleSize, completed, 0, completed * 3, 0, 0,
                phaseTimer.ElapsedMilliseconds, "completed");
        }

        foreach (var sampleSize in sampleSizes)
        {
            using var fixture = new Fixture();
            var authorities = Enumerable.Range(0, sampleSize)
                .Select(i => Issue(fixture.Scenario($"performance-reopen-{sampleSize}-{i}")))
                .ToArray();
            using (var store = fixture.Create())
                foreach (var authority in authorities) CompleteSession(store, authority);

            var phaseTimer = System.Diagnostics.Stopwatch.StartNew();
            using (var reopened = fixture.Open())
            {
                _ = fixture.AnchorLines();
                _ = fixture.Snapshot();
            }
            phaseTimer.Stop();
            CheckDeadline("reopen-read", sampleSize, 1, 0, 0, 1, 2, phaseTimer);
            Emit("reopen-read", sampleSize, 1, 0, 0, 1, 2,
                phaseTimer.ElapsedMilliseconds, "completed");
        }
    }

    [Fact]
    public void Session_CommitRetainsPostPublicationEventValidation()
    {
        using var fixture = new Fixture();
        var authority = Issue(fixture.Scenario());
        using var store = fixture.Create();
        var published = false;
        using var reservation = Require(store.ReserveOrReplay(authority, "integrity-run"));
        var permit = Require(store.BeginEvaluation(reservation));
        using (Hook(point =>
        {
            if (point == "completed-event-published")
            {
                published = true;
                var path = fixture.Event(reservation.SessionId, "completed");
                var original = Encoding.UTF8.GetString(File.ReadAllBytes(path));
                var tampered = original.Replace("\"kind\":\"completed\"", "\"kind\":\"completeD\"", StringComparison.Ordinal);
                Assert.Equal(original.Length, tampered.Length);
                File.WriteAllBytes(path, Encoding.UTF8.GetBytes(tampered));
            }
        }))
        {
            Failure(store.CompleteFromServerOwnedExecution(permit, Result(authority)), "HISTORY_MISMATCH");
        }
        Assert.True(published);
        Failure(Store.OpenExistingForServerOwnedRoot(fixture.Repository, fixture.StoreId), "HISTORY_MISMATCH");
    }

    [Fact]
    public void Session_ReservationCapacityRejects257thDistinctRequestWithoutHistoryEviction()
    {
        using var fixture = new Fixture();
        using (var store = fixture.Create())
        {
            for (var i = 0; i < Store.MaximumSessions; i++)
            {
                var authority = Issue(fixture.Scenario("limit-" + i));
                using var reservation = Require(store.ReserveOrReplay(authority, "limit-" + i));
            }
        }
        Assert.Equal(256, Directory.GetDirectories(fixture.Sessions).Length);
        using (var reopened = fixture.Open())
        {
            var extra = Issue(fixture.Scenario("limit-extra"));
            var retained = fixture.Snapshot();
            Failure(reopened.ReserveOrReplay(extra, "extra"), "LIMIT_EXCEEDED");
            Assert.Equal(retained, fixture.Snapshot());
        }
        Assert.Equal(256, Directory.GetDirectories(fixture.Sessions).Length);
    }

    [Fact]
    public void Session_CompletedHistoryRetainsFullAnchorFrameCount()
    {
        using var fixture = new Fixture();
        using (var store = fixture.Create())
            for (var i = 0; i < Store.MaximumSessions; i++) CompleteSession(store, Issue(fixture.Scenario("limit-" + i)));
        Assert.Equal(256, Directory.GetDirectories(fixture.Sessions).Length);
        Assert.Equal(769, fixture.AnchorLines().Length); // 1 genesis frame + 3 frames for each of 256 completed sessions.
        using var reopened = fixture.Open();
    }

    [Fact]
    public void Session_HistoryAndScanByteBoundsRejectWithoutWrites()
    {
        foreach (var anchor in new[] { false, true })
        {
            using var fixture = new Fixture();
            var authority = Issue(fixture.Scenario());
            using (var store = fixture.Create()) CompleteSession(store, authority);
            using (var intact = fixture.Open()) { }
            var path = anchor ? fixture.Anchor : fixture.Event(Admit(authority).AdmissionIdentitySha256[7..], "completed");
            var bound = anchor ? Store.MaximumHistoryBytes : Store.MaximumEventBytes;
            // At the exact byte bound the byte gate permits inspection, then integrity/parser
            // rejects these deliberately padded bytes. This is not a valid receipt at2MiB.
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None)) stream.SetLength(bound);
            RejectHistoryWithoutWrites(fixture, "HISTORY_MISMATCH");
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None)) stream.SetLength(bound + 1L);
            RejectHistoryWithoutWrites(fixture, "LIMIT_EXCEEDED");
        }
        using var aggregate = new Fixture();
        using (var store = aggregate.Create())
            for (var i = 0; i < 11; i++) CompleteSession(store, Issue(aggregate.Scenario("aggregate-" + i)));
        using (var intact = aggregate.Open()) { }
        var remaining = Store.MaximumScanBytes - new FileInfo(aggregate.Anchor).Length - new FileInfo(aggregate.Lock).Length - new FileInfo(aggregate.StoreFile).Length;
        var events = Directory.GetFiles(aggregate.Sessions, "*.json", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(33, events.Length);
        for (var i = 0; i < events.Length; i++)
        {
            var bytes = Math.Min(Store.MaximumEventBytes, remaining - (events.Length - i - 1));
            Assert.InRange(bytes, 1, Store.MaximumEventBytes);
            using var stream = new FileStream(events[i], FileMode.Open, FileAccess.Write, FileShare.None);
            stream.SetLength(bytes);
            remaining -= bytes;
        }
        Assert.Equal(0, remaining);
        Assert.Equal(Store.MaximumScanBytes, Directory.GetFiles(aggregate.StoreRoot, "*", SearchOption.AllDirectories).Sum(p => new FileInfo(p).Length));
        // Exact64MiB reaches the later anchored-integrity guard, +1 reaches scan cap first.
        RejectHistoryWithoutWrites(aggregate, "HISTORY_MISMATCH");
        using (var stream = new FileStream(events[^1], FileMode.Open, FileAccess.Write, FileShare.None)) stream.SetLength(stream.Length + 1);
        RejectHistoryWithoutWrites(aggregate, "LIMIT_EXCEEDED");
    }

    private static CodeDocumentationSavedModelAssessmentCalibrationResult Mutate(CodeDocumentationSavedModelAssessmentCalibrationResult result, string change)
    {
        var c = result.Calibration;
        var r = result.RepresentativeAssessment;
        var observations = c.Observations.ToArray();
        switch (change)
        {
            case "missing-repeat": c = c with { Observations = observations[..^1] }; break;
            case "wrong-cases": observations[1] = observations[1] with { CaseIds = ["foreign", "cases"] }; break;
            case "wrong-count": c = c with { ObservedEvaluationCallCount = 9 }; break;
            case "duplicate-response-id": observations[1] = observations[1] with { ObservedEvaluationRequestIds = observations[0].ObservedEvaluationRequestIds }; break;
            case "wrong-evaluator": r = r with { Evaluator = r.Evaluator with { ExpectedModelId = "foreign" } }; break;
            case "foreign-evaluation-set": c = c with { EvaluationSetId = "foreign-evaluation-set" }; break;
            case "wrong-generation": r = r with { GenerationSettings = r.GenerationSettings with { Seed = 8 } }; break;
            case "wrong-candidate": r = r with { ModelManifestSha256 = Hex('8') }; break;
            case "mutated-model": observations[1] = observations[1] with { ModelSerializationAfterSha256 = Hex('8') }; break;
            case "inconsistent-score": observations[1] = observations[1] with { PassedCaseCount = 0 }; break;
            case "foreign-noise": c = c with { NoisePolicyIdentitySha256 = Hex('8') }; break;
            case "changed-numeric-passport": ((double[])c.MeasurementNoisePassport.RepeatedScores)[1] = .5; break;
            case "self-consistent-foreign-passport":
                c = c with { MeasurementNoisePassport = MeasurementNoisePolicy.BuildPassport(c.ScorerIdentity, c.EvaluationSetId,
                    c.HeldOutCaseCount, new double[5], MeasurementNoisePolicyConfig.Canonical, Cut) };
                Assert.True(MeasurementNoisePolicy.TryValidatePassport(c.MeasurementNoisePassport, out _));
                break;
            case "representative-failed": r = r with { IsSuccess = false }; break;
            case "calibration-failed": c = c with { IsSuccess = false }; break;
        }
        if (change != "missing-repeat") c = c with { Observations = observations };
        return new(r, c);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Session_WindowsLongPathReservationCompletionReplayPreservesCustody(bool lengthened)
    {
        Assert.True(OperatingSystem.IsWindows(), "This regression requires the admitted Windows CI lane.");
        using var fixture = new Fixture(lengthenRepositoryPath: lengthened);
        var authority = Issue(fixture.Scenario());
        var admission = Admit(authority);
        var id = admission.AdmissionIdentitySha256[7..];
        var sessionPath = Path.GetDirectoryName(fixture.Event(id, "reservation"))!;
        var stagePath = fixture.Event(id, "reservation") + ".stage";
        if (lengthened)
        {
            Assert.True(sessionPath.Length > 248);
            Assert.True(stagePath.Length > 260);
        }
        Assert.All(stagePath.Split(Path.DirectorySeparatorChar), component => Assert.InRange(component.Length, 1, 255));
        var inputs = fixture.InputSnapshot();
        CodeDocumentationCertificationSessionReceipt receipt;
        DateTimeOffset reservedAt;
        using (var store = fixture.Create())
        using (var reservation = Require(store.ReserveOrReplay(authority, "long-path")))
        {
            Assert.False(reservation.IsReplay);
            Assert.Null(reservation.Receipt);
            Assert.Equal(id, reservation.SessionId);
            reservedAt = reservation.ReservedAtUtc;
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(fixture.StoreRoot));
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(fixture.Sessions));
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(sessionPath));
            Assert.Throws<IOException>(() =>
            {
                using var competing = new FileStream(fixture.Lock, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            });
            var permit = Require(store.BeginEvaluation(reservation));
            receipt = Require(store.CompleteFromServerOwnedExecution(permit, Result(authority)));
            Assert.Equal(10, receipt.ObservedEvaluationCallCount);
            Assert.Equal(1d, receipt.MeasurementNoisePassport.MeanScore);
            foreach (var kind in new[] { "reservation", "started", "completed" })
                Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(fixture.Event(id, kind)));
            Assert.False(File.Exists(stagePath));
            Assert.Equal(4, fixture.AnchorLines().Length);
        }
        var retained = fixture.Snapshot();
        var receiptJson = Serialize(receipt);
        using (var reopened = fixture.Open())
        using (var replay = Require(reopened.ReserveOrReplay(authority, "renamed-long-path")))
        {
            Assert.True(replay.IsReplay);
            Assert.Equal(id, replay.SessionId);
            Assert.Equal(reservedAt, replay.ReservedAtUtc);
            Assert.Equal("long-path", replay.DiagnosticRunId);
            Assert.Equal(receiptJson, Serialize(replay.Receipt));
            Failure(reopened.BeginEvaluation(replay), "INVALID_PERMIT");
        }
        Assert.Equal(retained, fixture.Snapshot());
        Assert.Equal(inputs, fixture.InputSnapshot());
        _output.WriteLine($"long-path lifecycle completed: lengthened={lengthened}; session={sessionPath.Length}; stage={stagePath.Length}");
    }

    [Theory]
    [InlineData("directory-missing-parent")]
    [InlineData("file-missing-parent")]
    [InlineData("file-create-new-collision")]
    public void Session_WindowsNativeCreateOpenFailuresRetainNumericCause(string operation)
    {
        Assert.True(OperatingSystem.IsWindows(), "This regression requires the admitted Windows CI lane.");
        using var fixture = new Fixture();
        var root = Path.Combine(fixture.OwnRoot, "native-error-controls");
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(root);
        var missingParent = Path.Combine(root, "absent-parent");
        var collision = Path.Combine(root, "existing.bin");
        byte[] original = [4, 8, 15, 16, 23, 42];
        WritePrivateBytes(collision, original);
        Assert.False(Directory.Exists(missingParent));
        var before = SnapshotOwnedTree(fixture.OwnRoot);
        var error = Assert.Throws<IOException>(() =>
        {
            if (operation == "directory-missing-parent")
                SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Path.Combine(missingParent, "child"));
            else
            {
                var path = operation == "file-missing-parent" ? Path.Combine(missingParent, "file.bin") : collision;
                using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                    path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, FileOptions.WriteThrough);
            }
        });
        var expected = operation == "file-create-new-collision"
            ? unchecked((int)0x80070050) : unchecked((int)0x80070003);
        Assert.Equal(expected, error.HResult);
        Assert.Equal(operation == "directory-missing-parent"
            ? "Windows custody directory could not be created."
            : "Windows custody file could not be opened.", error.Message);
        Assert.DoesNotContain(fixture.OwnRoot, error.Message, StringComparison.Ordinal);
        Assert.Equal(before, SnapshotOwnedTree(fixture.OwnRoot));
        Assert.Equal(original, ReadPrivateBytes(collision));
        _output.WriteLine($"native failure control: operation={operation}; HResult={error.HResult}");
    }

    [Fact]
    public void Session_WindowsLongPathPublicationCollisionAndIdentityFailClosed()
    {
        Assert.True(OperatingSystem.IsWindows(), "This regression requires the admitted Windows CI lane.");
        using var fixture = new Fixture(lengthenRepositoryPath: true);
        var root = Path.Combine(fixture.Repository, "publication-custody");
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(root);
        var stage = Path.Combine(root, "payload.stage");
        var destination = Path.Combine(root, "payload.bin");
        Assert.True(stage.Length > 260);
        Assert.True(destination.Length > 260);
        byte[] original = [1, 3, 5, 7];
        byte[] replacement = [2, 4, 6, 8];
        WritePrivateBytes(stage, original); // Writer is closed before the real durable rename.
        Assert.True(SecureLocalFileBoundary.PublishAtomicDurable(stage, destination));
        Assert.False(File.Exists(stage));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(root));
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(destination));
        Assert.Equal(original, ReadPrivateBytes(destination));
        WritePrivateBytes(stage, replacement);
        using (var opened = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
            destination, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.SequentialScan))
        {
            Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(opened.SafeFileHandle, destination));
            Assert.False(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(opened.SafeFileHandle, stage));
            Assert.Throws<IOException>(() =>
            {
                using var competing = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                    destination, FileMode.Open, FileAccess.ReadWrite, FileShare.None, FileOptions.WriteThrough);
            });
        }
        var beforeCollision = SnapshotOwnedTree(fixture.OwnRoot);
        Assert.False(SecureLocalFileBoundary.PublishAtomicDurable(stage, destination));
        Assert.Equal(original, ReadPrivateBytes(destination));
        Assert.Equal(replacement, ReadPrivateBytes(stage));
        Assert.Equal(beforeCollision, SnapshotOwnedTree(fixture.OwnRoot));
    }

    [Fact]
    public void Session_WindowsNativePathRepresentationPreservesOrdinaryMeaning()
    {
        Assert.True(OperatingSystem.IsWindows(), "This regression requires the admitted Windows CI lane.");
        using var fixture = new Fixture();
        var root = Path.Combine(fixture.OwnRoot, "ordinary-meaning");
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(root);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Path.Combine(root, "segment"));
        var spelling = root.Replace('\\', '/') + "/segment/.././leaf";
        var canonical = Path.Combine(root, "leaf");
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(spelling);
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(canonical));
        Assert.Equal(new[] { "leaf", "segment" }, Directory.GetDirectories(root).Select(Path.GetFileName).Order(StringComparer.Ordinal));
        var fileSpelling = spelling + "/./café.bin";
        var source = Path.Combine(canonical, "café.bin");
        byte[] original = [11, 22, 33];
        WritePrivateBytes(fileSpelling, original);
        Assert.Equal(original, ReadPrivateBytes(source));
        using (var opened = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
            fileSpelling, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.SequentialScan))
            Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(opened.SafeFileHandle, source));

        var drive = Path.GetPathRoot(root)![..2];
        var invalid = new List<string>
        {
            "", null!,
            Path.Combine("relative-" + Path.GetFileName(fixture.OwnRoot), "child"),
            drive + Path.GetRelativePath(Path.GetFullPath(drive + "."), Path.Combine(root, "drive-relative")),
            Path.Combine(root, "root-relative")[2..],
            @"\\?\" + Path.Combine(root, "extended"),
            @"\\.\" + Path.Combine(root, "device"),
            @"\??\" + Path.Combine(root, "nt-device"),
            Path.Combine(root, "trailing."), Path.Combine(root, "trailing "),
            source + ":alternate-stream",
            root + @"\trailing.\..\hidden-dot",
            root + @"\trailing \..\hidden-space",
            root + @"\CON\..\hidden-device"
        };
        invalid.AddRange(new[] { "CON", "prn.txt", "AUX", "nul.tar.gz", "CON .txt", "CONIN$", "CONOUT$",
            "COM1", "com9.txt", "LPT1", "lpt9.txt", "COM¹", "COM².log", "COM³", "LPT¹", "LPT²", "LPT³.txt" }
            .Select(name => Path.Combine(root, name)));
        invalid.AddRange(new[] { '<', '>', ':', '"', '|', '?', '*', '\0', '\u0001', '\u007f' }
            .Select(character => Path.Combine(root, "bad" + character + "name")));
        var before = SnapshotOwnedTree(fixture.OwnRoot);
        for (var index = 0; index < invalid.Count; index++)
        {
            var path = invalid[index];
            var directoryError = Assert.Throws<IOException>(() => SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(path));
            var fileError = Assert.Throws<IOException>(() =>
            {
                using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                    path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, FileOptions.WriteThrough);
            });
            Assert.Equal("Windows custody path representation is invalid.", directoryError.Message);
            Assert.Equal(directoryError.Message, fileError.Message);
            Assert.False(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(path));
            Assert.False(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path));
            Assert.False(SecureLocalFileBoundary.PublishAtomicDurable(source, path));
            Assert.False(SecureLocalFileBoundary.PublishAtomicDurable(path, Path.Combine(root, "destination-" + index)));
            Assert.Equal(before, SnapshotOwnedTree(fixture.OwnRoot));
        }
        Assert.Equal(original, ReadPrivateBytes(source));
    }

    private static void WritePrivateBytes(string path, byte[] bytes)
    {
        using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
            path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, FileOptions.WriteThrough);
        Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path));
        stream.Write(bytes);
        stream.Flush(true);
    }

    private static byte[] ReadPrivateBytes(string path)
    {
        using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
            path, FileMode.Open, FileAccess.Read, FileShare.None, FileOptions.SequentialScan);
        Assert.True(SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path));
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, 1024);
        Assert.NotNull(bytes);
        return bytes!;
    }

    private static string[] SnapshotOwnedTree(string root)
        => Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
            .Select(path => "directory:" + Path.GetRelativePath(root, path))
            .Concat(Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(path => "file:" + Path.GetRelativePath(root, path) + ":" + Hash(File.ReadAllBytes(path))))
            .Order(StringComparer.Ordinal).ToArray();

    private static CodeDocumentationCertificationSessionReceipt CompleteSession(Store store, ServerOwnedCodeDocumentationCertificationScope authority)
    {
        using var reservation = Require(store.ReserveOrReplay(authority, "original-run"));
        return Require(store.CompleteFromServerOwnedExecution(Require(store.BeginEvaluation(reservation)), Result(authority)));
    }

    private static CodeDocumentationSavedModelAssessmentCalibrationResult Result(ServerOwnedCodeDocumentationCertificationScope authority, bool high = true)
    {
        var a = Admit(authority);
        var count = a.CaseIds.Count;
        var passed = high ? count : 0;
        var observations = Enumerable.Range(0, 5).Select(i => new CodeDocumentationSavedModelAssessmentCalibrationObservation(
            "assessment-" + i, count, passed, (double)passed / count, a.GenerationSettingsSha256, Hex('7'), Hex('7'),
            a.CaseIds.ToArray(), Enumerable.Repeat(Hex('6'), count).ToArray(),
            Enumerable.Range(0, count).Select(j => "response-" + i + "-" + j).ToArray())).ToArray();
        var evaluator = a.Evaluator with { ObservedEvaluationRequestIds = observations[0].ObservedEvaluationRequestIds.ToArray() };
        var c = a.Candidate;
        var m = a.CorpusManifest;
        var report = new CodeDocumentationSavedModelAssessmentReport(true, Secret, "assessment-0", c.DomainSlug, c.DesignPatternSlug,
            count, passed, (double)passed / count, m.CorpusVersion, m.ManifestSha256, m.CorpusSha256, a.GenerationSettings, a.GenerationSettingsSha256,
            c.ModelSourceKind, c.RepositoryRelativeManifestPath, c.ManifestSha256, c.ManifestSha256, c.WeightsSha256,
            Hex('7'), 128, Hex('7'), 128, evaluator,
            a.CaseIds.Select(id => new CodeDocumentationSavedModelAssessmentCaseResult(id, high, Secret, Secret, Hex('6'), 7)).ToArray(), [Secret]);
        var scorer = new HeldOutScorerIdentity(c.DomainSlug, evaluator.ExpectedModelId, evaluator.ExpectedOllamaManifestDigest,
            evaluator.EvaluationProfileId, a.MetricProfileId);
        var set = CodeDocumentationSavedModelAssessmentCalibrationService.BuildEvaluationSetId(c.DomainSlug, c.DesignPatternSlug,
            m.CorpusVersion, m.ManifestSha256, m.CorpusSha256, a.GenerationSettingsSha256, evaluator, a.NoisePolicyIdentitySha256);
        var passport = MeasurementNoisePolicy.BuildPassport(scorer, set, count, observations.Select(o => o.CorrectFraction).ToArray(),
            MeasurementNoisePolicyConfig.Canonical, Cut);
        return new(report, new(true, Secret, "synthetic-calibration", 5, count, count * 5, count * 5, set,
            a.NoisePolicyIdentitySha256, scorer, passport, observations, [Secret]));
    }

    private static void AssertReadOnly<T>(IReadOnlyList<T> values)
    {
        Assert.False(values is T[]);
        var list = Assert.IsAssignableFrom<IList<T>>(values);
        Assert.True(list.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => list[0] = values[0]);
        Assert.Throws<NotSupportedException>(() => list.Add(values[0]));
    }

    private static void RejectHistoryWithoutWrites(Fixture fixture, string suffix)
    {
        var retained = fixture.Snapshot();
        Failure(Store.OpenExistingForServerOwnedRoot(fixture.Repository, fixture.StoreId), suffix);
        Assert.Equal(retained, fixture.Snapshot());
    }

    private static JsonElement ReadEnvelope(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        var payload = root.GetProperty("payload");
        var payloadBytes = Encoding.UTF8.GetBytes(payload.GetRawText());
        Assert.Equal(payloadBytes.Length, root.GetProperty("payloadByteLength").GetInt32());
        Assert.Equal(Hash(payloadBytes), root.GetProperty("payloadSha256").GetString());
        return payload.Clone();
    }

    private static string ChangeEnvelope(string text, Action<JsonObject> change)
    {
        var envelope = JsonNode.Parse(text)!.AsObject();
        var payload = envelope["payload"]!.AsObject();
        change(payload);
        var bytes = Encoding.UTF8.GetBytes(payload.ToJsonString(Json));
        envelope["payloadByteLength"] = bytes.Length;
        envelope["payloadSha256"] = Hash(bytes);
        return envelope.ToJsonString(Json);
    }
    private static void RewriteEnvelope(string path, Action<JsonObject> change)
        => File.WriteAllText(path, ChangeEnvelope(File.ReadAllText(path), change), new UTF8Encoding(false));
    private static void RewriteRaw(string path, Action<JsonObject> change)
    {
        var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        change(node);
        File.WriteAllText(path, node.ToJsonString(Json), new UTF8Encoding(false));
    }
    private static void WriteLines(string path, IEnumerable<string> lines) => File.WriteAllText(path, string.Join("\n", lines) + "\n", new UTF8Encoding(false));
    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Json);
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static string Hex(char digit) => new(digit, 64);
    private static string Fingerprint(string seed) => FrozenReferenceEntry.ComputeNormalizedTextSha256("source:" + seed);
    private static T Require<T>(OperationResult<T> result)
    { Assert.True(result.IsSuccess, result.ErrorCode + ": " + result.ErrorMessage); Assert.NotNull(result.Value); return result.Value!; }
    private static void Failure<T>(OperationResult<T> result, string suffix)
    { Assert.False(result.IsSuccess); Assert.Null(result.Value); Assert.Equal(Store.ErrorPrefix + suffix, result.ErrorCode); }
    private static IDisposable Hook(Action<string> action) => new HookScope(action);
    private sealed class FailureDiagnosticScope : IDisposable
    {
        private readonly Action<Store.FailureObservation>? _previous;
        internal FailureDiagnosticScope(Action<Store.FailureObservation> observer)
        { _previous = Store.FailureDiagnostic.Value; Store.FailureDiagnostic.Value = observer; }
        public void Dispose() => Store.FailureDiagnostic.Value = _previous;
    }
    private sealed class HookScope : IDisposable
    {
        private readonly Action<string>? _previous;
        internal HookScope(Action<string> action) { _previous = Store.FaultInjection.Value; Store.FaultInjection.Value = action; }
        public void Dispose() => Store.FaultInjection.Value = _previous;
    }

    private sealed record Scenario(CodeDocumentationCertificationScopeDeclaration Scope, CodeDocumentationSavedModelAssessmentModelPassport Candidate,
        RepositoryOwnedReferenceCorpusOpenResult Opened, CodeDocumentationSavedModelAssessmentGenerationSettings Generation,
        ContinuousLearningTp1DpoConfiguration Evaluator, CodeDocumentationCertificationExposureIndex Index);
    private static ServerOwnedCodeDocumentationCertificationScope Issue(Scenario s)
    {
        var authority = Require(ServerOwnedCodeDocumentationCertificationScope.CreateFromServerOwnedSnapshot(
            s.Scope, s.Candidate, s.Opened, s.Generation, s.Evaluator, MeasurementNoisePolicyConfig.Canonical, s.Index));
        Admit(authority);
        return authority;
    }
    private static CodeDocumentationCertificationAdmission Admit(ServerOwnedCodeDocumentationCertificationScope a)
        => Require(CodeDocumentationCertificationAdmission.TryCreate(a, a.Candidate, a.Corpus, a.Index));
    private static Scenario WithScope(Scenario s, CodeDocumentationCertificationScopeDeclaration scope)
        => s with { Scope = scope, Index = Index(scope, s.Index.Sources) };
    private static CodeDocumentationCertificationScopeDeclaration Scope(Scenario s, string version)
        => new(CodeDocumentationCertificationScopeDeclaration.CurrentSchema, Domain, Pattern, Subject, s.Scope.ScopeId, version,
            s.Scope.Purpose, Cut, "coverage-v1", s.Scope.CheckpointLineageManifestIdentities, s.Scope.OwnerSnapshots, Complete, s.Scope.CertificationSourceBindings);
    private static CodeDocumentationCertificationExposureIndex Index(CodeDocumentationCertificationScopeDeclaration scope,
        IEnumerable<CodeDocumentationCertificationSource> sources, string version = "v1")
        => new(CodeDocumentationCertificationExposureIndex.CurrentSchema, version, scope, Hex('a'), Complete, sources);
    private static ContinuousLearningTp1DpoConfiguration Evaluator(string model = "qwen-assessment-test:9b", int budget = 1000)
        => Require(ContinuousLearningTp1DpoConfiguration.Create(model, Hex('c'), Hex('d'), budget,
            "sha256:" + Hex('e'), "sha256:" + Hex('f'), Enumerable.Range(1, 32).Select(i => (byte)i).ToArray()));
    private static FrozenReferenceEntry Entry(
        string seed,
        string input,
        FrozenReferencePartition partition = FrozenReferencePartition.HeldOut)
    {
        var reference = Secret + ": shared reference";
        var provenance = Require(PaidReferenceProvenance.Create("synthetic-paid-provider", "synthetic-paid-model", "v1",
            "request-" + seed, Cut.AddHours(-1), FrozenReferenceEntry.ComputeNormalizedTextSha256(reference)));
        return Require(FrozenReferenceEntry.Create(input, reference, provenance, Domain, Subject, partition, Fingerprint(seed)));
    }
    private static CodeDocumentationCertificationSource Node(FrozenReferenceEntry entry)
        => new(entry.SourceEntityFingerprint, Domain, Subject, Owner, "v1", entry.OwnerInput, Fingerprint("origin"),
            CertificationExposureRole.ReferenceAcquisitionOnly, CertificationLineageKind.KnownRoot, []);
    private static CodeDocumentationCertificationSource Source(string seed)
        => new(Fingerprint(seed), Domain, Subject, Owner, "v1", "separate " + seed, Fingerprint("origin"), CertificationExposureRole.None,
            CertificationLineageKind.KnownRoot, []);
    private static CodeDocumentationCertificationSource Copy(CodeDocumentationCertificationSource n, IEnumerable<string> ancestors)
        => new(n.SourceEntityFingerprint, n.Domain, n.Subject, n.OwnerSnapshotId, n.OwnerSnapshotVersion, n.SourceInput, n.OriginEvidenceRef,
            n.Roles, CertificationLineageKind.Derived, ancestors);

    private sealed class Fixture : IDisposable
    {
        private readonly string _base;
        internal Fixture(bool createModelRoot = true, bool lengthenRepositoryPath = false)
        {
            _base = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "code-documentation-certification-session-fixtures"));
            OwnRoot = Path.Combine(_base, Guid.NewGuid().ToString("N"));
            Repository = lengthenRepositoryPath
                ? Path.Combine(OwnRoot, "depth-" + new string('a', 64), "depth-" + new string('b', 64), "repo")
                : Path.Combine(OwnRoot, "repo");
            Directory.CreateDirectory(Repository);
            File.WriteAllText(Path.Combine(Repository, "LlmFromScratch.sln"), "synthetic storage sentinel; not a checkout");
            Directory.CreateDirectory(Path.Combine(Repository, "14.ContinuousLearning"));
            Directory.CreateDirectory(Path.Combine(Repository, "11.DPO"));
            if (createModelRoot) Directory.CreateDirectory(Path.Combine(Repository, "training-artifacts", "models"));
        }
        internal string OwnRoot { get; }
        internal string Repository { get; }
        internal string StoreId => Hex('1');
        internal string StoreRoot => Path.Combine(Repository, "training-artifacts", "models", Store.DirectoryName);
        internal string StoreFile => Path.Combine(StoreRoot, "store.json");
        internal string Lock => Path.Combine(StoreRoot, ".owner.lock");
        internal string InitializationStateRoot => Path.Combine(
            Repository,
            "training-artifacts",
            "models",
            Store.InitializationStateDirectoryName);
        internal string InitializationMarker => Path.Combine(
            InitializationStateRoot,
            Store.InitializationMarkerFileName);
        internal string Anchor => Path.Combine(StoreRoot, "history-anchor", "commitments.jsonl");
        internal string Sessions => Path.Combine(StoreRoot, "sessions");
        internal Store Create() => Require(Store.CreateNewForServerOwnedRoot(Repository, StoreId));
        internal Store Open() => Require(Store.OpenExistingForServerOwnedRoot(Repository, StoreId));
        internal string Event(string id, string kind) => Path.Combine(Sessions, id, kind + ".json");
        internal string[] AnchorLines() => File.ReadAllLines(Anchor, Encoding.UTF8);
        internal Scenario Scenario(string seed = "default", IEnumerable<FrozenReferenceEntry>? entries = null)
        {
            var opened = OpenCorpus(seed, entries ?? [Entry(seed + "-a", Secret + ": café\n" + seed), Entry(seed + "-b", Secret + ": second " + seed)]);
            var candidate = new CodeDocumentationSavedModelAssessmentModelPassport(Domain, Pattern, "synthetic-model", "v1", 1,
                "models/synthetic/manifest.json", Hex('a'), "models/synthetic/weights.bin", Hex('b'), 64, 1024);
            var scope = new CodeDocumentationCertificationScopeDeclaration(CodeDocumentationCertificationScopeDeclaration.CurrentSchema,
                Domain, Pattern, Subject, "synthetic-certification-scope", "v1", "independent-certification-v1", Cut, "coverage-v1",
                [candidate.ManifestSha256], [new(Owner, "v1")], Complete,
                opened.Corpus.Entries.Select(e => new CertificationSourceBinding(e.EntryId, e.SourceEntityFingerprint, Owner, "v1")));
            return new(scope, candidate, opened, new(32, 0, 0, 1, 7, 200, 512), Evaluator(), Index(scope, opened.Corpus.Entries.Select(Node)));
        }
        internal RepositoryOwnedReferenceCorpusOpenResult OpenCorpus(string version, IEnumerable<FrozenReferenceEntry> entries)
        {
            var corpus = Require(FrozenReferenceCorpus.Build("synthetic-certification", version, Cut, entries));
            var address = Require(RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(Repository, Domain, Subject, version));
            var sealedManifest = Require(RepositoryOwnedReferenceCorpusManifestSealer.PersistAndSeal(address, corpus));
            var opened = Require(RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(address));
            Assert.Equal(sealedManifest, opened.Manifest);
            Assert.Equal(corpus.CanonicalRepresentation, opened.Corpus.CanonicalRepresentation);
            return opened;
        }
        internal string[] InputSnapshot() => FilesUnder(Path.Combine(Repository, "training-data"));
        internal string[] Snapshot() => FilesUnder(StoreRoot).Concat(Directory.Exists(StoreRoot)
            ? Directory.GetDirectories(StoreRoot, "*", SearchOption.AllDirectories).Select(path => "directory:" + Path.GetRelativePath(StoreRoot, path))
            : Array.Empty<string>()).Order(StringComparer.Ordinal).ToArray();
        private static string[] FilesUnder(string root) => !Directory.Exists(root) ? [] : Directory.GetFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => Path.GetRelativePath(root, path) != ".owner.lock")
            .Order(StringComparer.Ordinal).Select(path => Path.GetRelativePath(root, path) + ":" + new FileInfo(path).Length + ":" + Hash(File.ReadAllBytes(path))).ToArray();
        internal Dictionary<string, byte[]> SessionBytes() => Directory.GetFiles(Sessions, "*", SearchOption.AllDirectories)
            .ToDictionary(p => Path.GetRelativePath(Sessions, p), File.ReadAllBytes, StringComparer.Ordinal);
        internal void DeleteSession(string id)
        {
            Assert.Matches("^[0-9a-f]{64}$", id);
            var path = Path.GetFullPath(Path.Combine(Sessions, id));
            Assert.Equal(Sessions, Path.GetDirectoryName(path));
            Directory.Delete(path, true);
        }
        internal void CopyStoreTo(Fixture other)
        {
            SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(other.StoreRoot);
            foreach (var directory in Directory.GetDirectories(StoreRoot, "*", SearchOption.AllDirectories).OrderBy(p => p.Length))
                SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(Path.Combine(other.StoreRoot, Path.GetRelativePath(StoreRoot, directory)));
            foreach (var path in Directory.GetFiles(StoreRoot, "*", SearchOption.AllDirectories))
            {
                var destination = Path.Combine(other.StoreRoot, Path.GetRelativePath(StoreRoot, path));
                using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, FileOptions.WriteThrough);
                stream.Write(File.ReadAllBytes(path)); stream.Flush(true);
            }
        }
        public void Dispose()
        {
            var full = Path.GetFullPath(OwnRoot);
            Assert.Equal(_base, Path.GetDirectoryName(full));
            Assert.True(Guid.TryParseExact(Path.GetFileName(full), "N", out _));
            if (Directory.Exists(full)) Directory.Delete(full, true);
        }
    }
}
